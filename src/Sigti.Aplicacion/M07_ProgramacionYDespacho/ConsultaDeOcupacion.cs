using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>Un vehículo y lo que lo tiene tomado en la ventana consultada.</summary>
public sealed record CarrilDeVehiculo(
    string Vehiculo,
    string Siglas,
    string? Placa,
    string TipoDeVehiculo,
    IReadOnlyList<BarraDeOcupacion> Barras);

/// <summary>
/// Un tramo tomado. <b>Los dos extremos son inclusivos</b>: el retorno previsto es un día
/// en que el vehículo sigue tomado, no el primer día libre.
///
/// <b>Las fechas son las reales, no las recortadas contra la ventana.</b> Una barra que
/// empieza antes se descarta o se devuelve entera, nunca truncada: el cliente necesita
/// saber que se sale para dibujar la marca de corte, y sin eso un rango cortado se ve
/// igual que uno que efectivamente empieza el lunes.
/// </summary>
public sealed record BarraDeOcupacion(
    string Mision,
    string Folio,
    string Destino,
    DateOnly Desde,
    DateOnly Hasta,
    string Estado);

/// <summary>
/// Qué tiene tomado cada vehículo, y cuándo — la ocupación de la flota de `PT-026`.
///
/// ── Es una PROYECCIÓN del diario, no una tabla de reservas ───────────────────
/// P-1. La reserva vive en la transición que reservó (<see cref="RecursosTomados"/>), y
/// esta consulta la lee. Eso hace que <b>liberar sea no volver a tomar</b>: si después de
/// `T-08` el diario siguió a `T-11` o `T-13`, la misión ya no ocupa, sin que nadie haya
/// tenido que acordarse de borrar una fila.
///
/// ── Qué estados ocupan, y por qué no es «las que no están canceladas» ───────
/// Ocupa la misión cuya <b>última</b> transición la deja en un estado en que el vehículo
/// está comprometido: `PROGRAMADA`, `DESPACHADA` o `EN_RUTA`. Una `RETORNADA` no ocupa —
/// el vehículo volvió, aunque falte liquidar. Enumerarlo por lista blanca y no por
/// descarte es deliberado: el día que se agregue un estado nuevo, el descarte lo daría
/// por ocupante en silencio y una lista blanca obliga a decidir.
///
/// ── Qué NO hace ─────────────────────────────────────────────────────────────
/// No marca conflictos ni decide compatibilidad. Devuelve lo que está tomado; quién
/// choca con qué lo resuelve `EvaluacionDeAsignacion`, que es donde vive `BD-02`.
/// </summary>
public sealed class ConsultaDeOcupacion(SigtiDbContext contexto)
{
    /// <summary>
    /// Los estados en que el vehículo está comprometido.
    ///
    /// `RETORNADA`, `LIQUIDADA` y `CERRADA` no están: el vehículo ya volvió. `APROBADA`
    /// tampoco — todavía no se le asignó vehículo, y de hecho no puede haberlo tomado.
    /// </summary>
    private static readonly EstadoDeMision[] Ocupan =
    [
        EstadoDeMision.Programada,
        EstadoDeMision.Despachada,
        EstadoDeMision.EnRuta,
    ];

    public async Task<IReadOnlyList<CarrilDeVehiculo>> EnVentanaAsync(
        DateOnly desde,
        DateOnly hasta,
        CancellationToken cancelacion = default)
    {
        var vehiculos = await contexto.Vehiculos
            .AsNoTracking()
            .OrderBy(v => v.Siglas)
            .ToListAsync(cancelacion);

        // El recorte por ventana va en SQL: el diario de una institución crece para
        // siempre y traerlo entero para descartar en memoria sería peor cada año.
        // Los dos extremos son inclusivos, de los dos lados de la comparación.
        var filas = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Retorno >= desde && e.Salida <= hasta)
            .ToListAsync(cancelacion);

        var barras = new Dictionary<Ulid, List<BarraDeOcupacion>>();

        foreach (var fila in filas)
        {
            // El estado es el destino de la ÚLTIMA transición: eso es lo que significa que
            // el estado sea la proyección del diario, y no una columna que se actualiza.
            var ultima = fila.Transiciones.MaxBy(t => t.Orden);
            if (ultima is null || !Ocupan.Contains(ultima.Destino)) continue;

            // La reserva vigente es la de la última transición que reservó. Reprogramar
            // —`T-11` y de vuelta `T-08`— deja dos en el diario, y ocupa la segunda.
            var reserva = fila.Transiciones
                .Where(t => t.VehiculoTomado is not null)
                .MaxBy(t => t.Orden);

            if (reserva?.VehiculoTomado is not { } vehiculo) continue;

            if (!barras.TryGetValue(vehiculo, out var lista))
                barras[vehiculo] = lista = [];

            lista.Add(new BarraDeOcupacion(
                Mision: fila.Id.ToString(),
                Folio: ConsultaDeMisiones.FolioProvisional(fila.Id),
                Destino: fila.Destino,
                Desde: fila.Salida,
                Hasta: fila.Retorno,
                Estado: ultima.Destino.ToString()));
        }

        return vehiculos
            .Select(v => new CarrilDeVehiculo(
                Vehiculo: v.Id.ToString(),
                Siglas: v.Siglas,
                Placa: v.Placa,
                TipoDeVehiculo: v.TipoDeVehiculo,
                Barras: barras.TryGetValue(v.Id, out var b)
                    ? b.OrderBy(x => x.Desde).ToList()
                    : []))
            .ToList();
    }
}
