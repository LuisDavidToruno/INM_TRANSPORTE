using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>Un expediente tal como lo lee la oficina.</summary>
/// <param name="Folio">
/// El número impreso que la institución cita en su descargo. Todavía no existe el
/// circuito de folios de `RNF-21`, así que va derivado del identificador y
/// <b>marcado como provisional</b> — no se muestra un ULID en su lugar.
/// </param>
public sealed record VistaDeExpediente(
    string Id,
    string Folio,
    string Estado,
    string CapturadaPor,
    string SolicitanteDeDerecho,
    string Dependencia,
    string ObjetoDelTraslado,
    string Destino,
    DateOnly SalidaPrevista,
    DateOnly RetornoPrevisto,
    /// <summary>
    /// A qué hora sale y a qué hora vuelve. <b>Nulas en los expedientes anteriores al campo</b>:
    /// se dicen como ausentes, no se rellenan con un 00:00 que se leería como medianoche.
    /// </summary>
    TimeOnly? HoraDeSalida,
    TimeOnly? HoraDeRetorno,
    int HolguraDias,
    /// <summary>
    /// Si la aprobación ya caducó. Va en la vista para que la cola lo muestre <b>antes</b>
    /// de que alguien lo intente, no como sorpresa al guardar.
    /// </summary>
    bool AprobacionCaducada,
    IReadOnlyList<VistaDeTransicion> Diario);

public sealed record VistaDeTransicion(
    string Id,
    string Destino,
    string Ejecuta,
    DateTimeOffset Momento,
    string? Motivo,
    /// <summary>
    /// El vehículo que esta transición tomó. <b>Nulo en toda transición que no reserva.</b>
    ///
    /// Viaja porque la pantalla de reasignación necesita decir <i>cuál tiene hoy</i>: sin
    /// eso, quien va a cambiar el vehículo tiene que deducirlo del cronograma buscando el
    /// folio de su propia misión, y elegir a ciegas es cómo se reasigna al mismo que ya
    /// estaba.
    /// </summary>
    string? VehiculoTomado,
    string? ConductorTomado);

/// <summary>
/// Las lecturas de la oficina.
///
/// Separadas del <see cref="ServicioDeMisiones"/> porque no comparten nada con él:
/// no abren transacción, no escriben bitácora y no evalúan precondiciones. Meterlas
/// ahí habría hecho que un servicio que existe para coordinar escrituras cargara
/// consultas que no coordinan nada.
/// </summary>
public sealed class ConsultaDeMisiones(SigtiDbContext contexto)
{
    public async Task<IReadOnlyList<VistaDeExpediente>> PorEstadoAsync(
        EstadoDeMision estado, CancellationToken cancelacion = default)
    {
        var filas = await contexto.Expedientes
            .Include(e => e.Transiciones)
            .AsNoTracking()
            .ToListAsync(cancelacion);

        // El estado es la PROYECCIÓN del diario, y no hay columna que filtrar (P-1).
        // Con volumen esto se resuelve con una vista materializada del último destino;
        // hoy no hay volumen y una vista materializada sería una copia que se puede
        // desincronizar del diario, que es justo lo que P-1 evita.
        return filas
            .Select(Proyectar)
            .Where(v => v.Estado == estado.ToString())
            .OrderBy(v => v.Folio)
            .ToList();
    }

    public async Task<VistaDeExpediente?> PorIdAsync(Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Expedientes
            .Include(e => e.Transiciones)
            .AsNoTracking()
            .SingleOrDefaultAsync(e => e.Id == id, cancelacion);

        return fila is null ? null : Proyectar(fila);
    }

    private static VistaDeExpediente Proyectar(FilaDeExpediente fila) => Proyectar(fila, DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6)));

    /// <param name="hoy">
    /// Entra como parámetro y no se lee del reloj acá adentro para que la proyección sea
    /// reproducible: el mismo expediente visto con la misma fecha da lo mismo siempre.
    /// </param>
    private static VistaDeExpediente Proyectar(FilaDeExpediente fila, DateOnly hoy)
    {
        var diario = fila.Transiciones.OrderBy(t => t.Orden).ToList();

        return new VistaDeExpediente(
            Id: fila.Id.ToString(),
            Folio: FolioProvisional(fila.Id),
            Estado: diario[^1].Destino.ToString(),
            CapturadaPor: fila.CapturadaPor,
            SolicitanteDeDerecho: fila.SolicitanteDeDerecho,
            Dependencia: fila.Dependencia,
            ObjetoDelTraslado: fila.ObjetoDelTraslado,
            Destino: fila.Destino,
            SalidaPrevista: fila.Salida,
            HoraDeSalida: fila.HoraDeSalida,
            HoraDeRetorno: fila.HoraDeRetorno,
            RetornoPrevisto: fila.Retorno,
            HolguraDias: fila.HolguraDias,
            AprobacionCaducada:
                diario[^1].Destino == EstadoDeMision.Aprobada && hoy >= fila.Salida,
            Diario: diario
                .Select(t => new VistaDeTransicion(
                    t.Transicion,
                    t.Destino.ToString(),
                    t.Ejecuta,
                    new DateTimeOffset(t.MomentoUtc, TimeSpan.Zero)
                        .ToOffset(TimeSpan.FromMinutes(t.DesfaseMinutos)),
                    t.Motivo,
                    t.VehiculoTomado?.ToString(),
                    t.ConductorTomado?.ToString()))
                .ToList());
    }

    /// <summary>
    /// ⚠️ Provisional. El folio real lo asigna el servidor contra el rango de la
    /// delegación (`RNF-21`, `ADR-005`), y ese circuito no existe todavía. Lleva
    /// prefijo `PROV-` para que nadie lo confunda con un folio oficial ni lo cite en
    /// un descargo.
    ///
    /// <b>Es `internal` y no privado a propósito.</b> Lo necesita también la ocupación de
    /// flota, y dos copias del mismo folio son dos folios que van a divergir el día que
    /// llegue el circuito real — la bandeja mostraría uno y el cronograma otro para la
    /// misma misión.
    /// </summary>
    internal static string FolioProvisional(Ulid id) => $"PROV-{id.ToString()[^6..]}";
}
