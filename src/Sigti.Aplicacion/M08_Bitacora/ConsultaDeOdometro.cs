using Microsoft.EntityFrameworkCore;
using Sigti.Datos;

namespace Sigti.Aplicacion.M08_Bitacora;

/// <summary>
/// La última lectura conocida de un vehículo — el insumo de `BD-05` al salir.
///
/// ── Es una proyección del diario, como la ocupación ──────────────────────────
/// El odómetro vive en las transiciones que lo registraron (`T-14` y `T-18`), no en una
/// columna del vehículo. Una columna `ultimo_odometro` sería una copia que se puede
/// desincronizar del asiento, y el asiento es lo que un auditor lee.
///
/// ── Cruza misiones, y tiene que hacerlo ──────────────────────────────────────
/// La lectura de referencia no es la de esta misión: es <b>la última del vehículo</b>, venga
/// de la misión que venga. Un odómetro que retrocede entre dos misiones distintas es
/// exactamente el fraude que `BD-05` existe para detectar — <i>«el hallazgo típico del TSC en
/// flota es el incremento de consumo sin relación con el uso habitual, y el odómetro es el
/// único ancla que tiene el sistema»</i>.
/// </summary>
public sealed class ConsultaDeOdometro(SigtiDbContext contexto)
{
    /// <summary>
    /// Devuelve nulo <b>sólo si el vehículo no tiene ninguna lectura</b> — su primera misión.
    /// El llamador tiene que pasar ese nulo tal cual: el dominio distingue «no hay previa» de
    /// «nadie consultó» y no puede hacerlo si alguien convierte el nulo en cero.
    /// </summary>
    public async Task<int?> UltimaLecturaAsync(Ulid vehiculo, CancellationToken cancelacion = default)
    {
        // Los expedientes que ese vehículo tuvo reservados. La reserva vive en la transición,
        // así que la pregunta se hace sobre el diario y no sobre una tabla de asignaciones.
        var filas = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => e.Transiciones.Any(t => t.VehiculoTomado == vehiculo))
            .ToListAsync(cancelacion);

        // La MÁS ALTA y no la más reciente. Con marcas de tiempo que vienen de dispositivos
        // que estuvieron sin red durante días, «la última en el tiempo» puede llegar después
        // de una lectura mayor — y tomar esa dejaría pasar un retroceso.
        var lecturas = filas
            .SelectMany(e => e.Transiciones)
            .Where(t => t.Odometro is not null)
            .Select(t => t.Odometro!.Value)
            .ToList();

        return lecturas.Count == 0 ? null : lecturas.Max();
    }

    /// <summary>
    /// La última lectura del vehículo que <b>esta misión</b> tiene reservado.
    ///
    /// Existe para que quien registra un `T-14` o un `T-18` no tenga que cargar el expediente
    /// sólo para averiguar de qué vehículo se trata. Devuelve nulo también cuando la misión
    /// no tiene reserva registrada — expedientes anteriores a <c>RecursosTomados</c>—, y el
    /// dominio lo trata como «sin lectura previa», que es lo que es.
    /// </summary>
    public async Task<int?> UltimaLecturaDeLaMisionAsync(
        Ulid expediente,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == expediente, cancelacion);

        var reserva = fila?.Transiciones
            .Where(t => t.VehiculoTomado is not null)
            .MaxBy(t => t.Orden);

        return reserva?.VehiculoTomado is { } vehiculo
            ? await UltimaLecturaAsync(vehiculo, cancelacion)
            : null;
    }
}
