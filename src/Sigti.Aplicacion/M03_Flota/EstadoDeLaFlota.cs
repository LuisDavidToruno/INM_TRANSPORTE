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
    /// Anota un cambio. <b>No valida la transición entre estados</b>, y es deliberado: §10.2
    /// no publica una tabla de transiciones permitidas del vehículo como sí lo hace para la
    /// misión, y inventarla acá sería escribir la regla en la capa que menos autoridad tiene.
    ///
    /// Lo que sí impone es que quede <b>quién</b> y —salvo los automáticos— <b>por qué</b>.
    /// </summary>
    public async Task AnotarAsync(
        Ulid vehiculo,
        CambioDeEstadoOperativo cambio,
        CancellationToken cancelacion = default)
    {
        if (!cambio.Automatico && string.IsNullOrWhiteSpace(cambio.Motivo))
            throw new ArgumentException(
                "Un cambio de estado declarado por una persona exige motivo: §10.2 pide causa " +
                "tipificada para NO_DISPONIBLE y acta para el préstamo y los terminales.",
                nameof(cambio));

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
