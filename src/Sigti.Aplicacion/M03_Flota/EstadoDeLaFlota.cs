using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// El estado operativo de los vehículos — §10.2.
///
/// ── Lee y escribe, y por eso no se llama «consulta» ──────────────────────────
/// El resto de las lecturas del sistema son consultas puras. Ésta también <b>anota</b>,
/// porque `ASIGNADO` y `EN_MISION` los fija el sistema como consecuencia de una transición
/// de la Orden de Misión, y ese asiento tiene que escribirse <b>dentro de la misma
/// transacción</b> que la transición que lo causó. Separarlos dejaría un vehículo asignado a
/// una misión que no llegó a guardarse, o al revés.
/// </summary>
public sealed class EstadoDeLaFlota(SigtiDbContext contexto)
{
    /// <summary>
    /// En qué estado está el vehículo <b>ahora</b>.
    ///
    /// ── Nulo es «nunca se declaró», no «disponible» ─────────────────────────
    /// Y la diferencia decide si `BD-07` bloquea. Un vehículo recién dado de alta al que
    /// nadie le fijó estado <b>no está disponible</b>: §10.2 lista <i>«alta reciente sin
    /// habilitar»</i> entre las causas de `NO_DISPONIBLE`. Devolver `Disponible` por omisión
    /// haría que el alta habilitara sola, que es exactamente lo contrario.
    /// </summary>
    public async Task<EstadoOperativo?> ActualAsync(
        Ulid vehiculo,
        CancellationToken cancelacion = default)
    {
        var cambios = await contexto.CambiosDeEstado
            .AsNoTracking()
            .Where(c => c.VehiculoId == vehiculo)
            .ToListAsync(cancelacion);

        // Por `Orden` y no por marca de tiempo: dos cambios pueden compartir instante cuando
        // uno lo fija el sistema por una transición y otro una persona.
        return cambios.Count == 0 ? null : cambios.MaxBy(c => c.Orden)!.Estado;
    }

    /// <summary>
    /// Cuántas misiones del vehículo <b>no</b> están en estado terminal — §10.2: <i>«un vehículo
    /// con misiones abiertas no puede ser dado de baja»</i>.
    ///
    /// ── Terminal por lista blanca, otra vez ─────────────────────────────────
    /// Se cuenta lo que <b>no</b> está en un estado terminal, y los terminales se enumeran. El
    /// día que se agregue un estado nuevo, contar por descarte lo daría por cerrado en
    /// silencio y dejaría dar de baja un vehículo con una misión viva.
    /// </summary>
    public async Task<int> MisionesAbiertasAsync(
        Ulid vehiculo,
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Transiciones.Any(t => t.VehiculoTomado == vehiculo))
            .ToListAsync(cancelacion);

        return filas.Count(e =>
        {
            var ultima = e.Transiciones.MaxBy(t => t.Orden);
            return ultima is not null && !Terminales.Contains(ultima.Destino);
        });
    }

    private static readonly EstadoDeMision[] Terminales =
    [
        EstadoDeMision.Cerrada,
        EstadoDeMision.CerradaConHallazgo,
        EstadoDeMision.Rechazada,
        EstadoDeMision.Anulada,
    ];

    /// <summary>
    /// Anota un cambio, <b>validado contra la tabla de transiciones de §10.2</b>.
    ///
    /// ── Antes no validaba la transición, y era por un error de lectura ──────
    /// Este comentario decía: <i>«no valida la transición entre estados, y es deliberado: §10.2
    /// no publica una tabla de transiciones permitidas del vehículo como sí lo hace para la
    /// misión»</i>. <b>Sí la publica</b>: el diagrama de §10.2 enumera `W-01` a `W-19`. Lo que
    /// faltaba era transcribirla — está en
    /// <see cref="ReglasDelEstadoOperativo.Tabla"/>— y ahora se valida contra ella.
    ///
    /// <b>La regla sigue viviendo en el documento.</b> Si la tabla y el diagrama difieren, manda
    /// el diagrama y esto es el defecto.
    /// </summary>
    /// <param name="esBienPropio">
    /// Si el vehículo pertenece al Estado — decide cuál de los dos terminales corresponde
    /// (`HB3-17`). <b>Nulo es «no se sabe»</b>, y entonces no se juzga: bloquear el descargo de
    /// toda la flota por un dato de alta que nadie llenó sería peor que el asiento que se
    /// quiere evitar.
    /// </param>
    /// <returns>
    /// La advertencia que quedó, si la hubo. Hoy sólo una: que no se pudo verificar el régimen
    /// de tenencia contra el terminal declarado.
    /// </returns>
    public async Task<string?> AnotarAsync(
        Ulid vehiculo,
        CambioDeEstadoOperativo cambio,
        bool? esBienPropio = null,
        CancellationToken cancelacion = default)
    {
        // ── Contra la tabla de §10.2 ────────────────────────────────────────
        var actual = await ActualAsync(vehiculo, cancelacion);

        var transicion = ReglasDelEstadoOperativo.ExigirTransicion(actual, cambio.Estado);

        ReglasDelEstadoOperativo.ExigirQuienLaFija(transicion, cambio.Automatico);
        ReglasDelEstadoOperativo.ExigirCausaOActa(cambio.Estado, cambio.Motivo);

        if (cambio.Estado is EstadoOperativo.DadoDeBaja or EstadoOperativo.RetiradoDeFlota)
            ReglasDelEstadoOperativo.ExigirSinMisionesAbiertas(
                cambio.Estado, await MisionesAbiertasAsync(vehiculo, cancelacion));

        var advertencia = ReglasDelEstadoOperativo.ExigirTerminalDelRegimenCorrecto(
            cambio.Estado, esBienPropio);

        var ultimo = await contexto.CambiosDeEstado
            .AsNoTracking()
            .Where(c => c.VehiculoId == vehiculo)
            .Select(c => (int?)c.Orden)
            .MaxAsync(cancelacion) ?? -1;

        contexto.CambiosDeEstado.Add(new FilaDeCambioDeEstado
        {
            Id = Ulid.NewUlid(),
            VehiculoId = vehiculo,
            Estado = cambio.Estado,
            MomentoUtc = cambio.Momento,
            Orden = ultimo + 1,
            Ejecuta = cambio.Ejecuta,
            Motivo = cambio.Motivo,
            Automatico = cambio.Automatico,
        });

        // Sin `SaveChanges`: quien llama decide cuándo confirmar. Cuando el cambio lo dispara
        // una transición de la misión, el asiento tiene que entrar en la MISMA transacción —
        // ver `ServicioDeMisiones.ConfirmarAsync`.
        return advertencia;
    }

    /// <summary>
    /// Confirma lo anotado. Sólo lo llama quien declara un estado <b>por sí mismo</b>: cuando el
    /// cambio viene de una transición de la misión, quien confirma es la transacción de
    /// <c>ServicioDeMisiones</c> y llamar acá la partiría en dos.
    /// </summary>
    public Task ConfirmarAsync(CancellationToken cancelacion = default) =>
        contexto.SaveChangesAsync(cancelacion);

    /// <summary>
    /// El historial completo, en orden. Es lo que contesta <i>«¿por qué no estuvo disponible en
    /// abril?»</i>, que es la pregunta real — el estado actual sólo contesta el presente.
    /// </summary>
    public async Task<IReadOnlyList<CambioDeEstadoOperativo>> HistorialAsync(
        Ulid vehiculo,
        CancellationToken cancelacion = default)
    {
        var cambios = await contexto.CambiosDeEstado
            .AsNoTracking()
            .Where(c => c.VehiculoId == vehiculo)
            .ToListAsync(cancelacion);

        return cambios
            .OrderBy(c => c.Orden)
            .Select(c => new CambioDeEstadoOperativo(
                c.Estado, c.MomentoUtc, c.Ejecuta, c.Motivo, c.Automatico))
            .ToList();
    }
}
