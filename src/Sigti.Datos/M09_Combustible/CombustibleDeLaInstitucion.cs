using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// Repositorio con intención (`ADR-009`): expone lo que el circuito del combustible pregunta,
/// no operaciones de tabla.
///
/// ── Las cuatro preguntas reales ─────────────────────────────────────────────
/// <b>¿Cuánto queda en este fondo?</b> · <b>¿Qué vales tiene esta misión?</b> ·
/// <b>¿Se puede liquidar ya?</b> · <b>¿De dónde salió este galón?</b> Todo lo demás son
/// detalles de cómo están guardadas las filas.
/// </summary>
public sealed class CombustibleDeLaInstitucion(SigtiDbContext contexto)
{
    // ── Fondos ──────────────────────────────────────────────────────────────

    public async Task<FondoDeCombustible?> BuscarFondoAsync(
        Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Fondos
            .Include(f => f.Movimientos)
            .SingleOrDefaultAsync(f => f.Id == id, cancelacion);

        return fila is null ? null : AFondo(fila);
    }

    public async Task<IReadOnlyList<FondoDeCombustible>> TodosLosFondosAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.Fondos
            .Include(f => f.Movimientos)
            .OrderByDescending(f => f.Desde)
            .ToListAsync(cancelacion);

        return [.. filas.Select(AFondo)];
    }

    /// <summary>
    /// <b>El saldo disponible</b> — `RN-26`: <c>aprobado − asignado + devoluciones constatadas</c>.
    ///
    /// ── Qué se cuenta como asignado, y por qué NO es todo ───────────────────
    /// Los vales <b>anulados y devueltos no consumen saldo</b>: `RN-27` punto 4 devuelve su
    /// valor al fondo porque no fueron canjeados. Contarlos igual haría que un mes con muchas
    /// misiones desprogramadas apareciera sin fondo teniendo el dinero intacto — y la salida
    /// que ofrece el sistema para eso es pedir una ampliación que no hace falta.
    ///
    /// ── La devolución constatada ────────────────────────────────────────────
    /// Sólo cuenta el saldo devuelto <b>en un asiento</b>, que es lo que `RN-26` llama
    /// constatado. Una devolución declarada de palabra no llega nunca a esta suma.
    /// </summary>
    public async Task<decimal> SaldoAsync(Ulid fondoId, CancellationToken cancelacion = default)
    {
        var fondo = await BuscarFondoAsync(fondoId, cancelacion)
            ?? throw new InvalidOperationException($"No existe el fondo {fondoId}.");

        var asignaciones = await AsignacionesDelFondoAsync(fondoId, cancelacion);

        var comprometido = asignaciones
            .Where(a => a.Estado is not (EstadoDeAsignacion.Anulada or EstadoDeAsignacion.Devuelta))
            .Sum(a => a.Monto);

        // Lo devuelto al liquidar, que sí volvió a la caja. Los anulados y devueltos ya no
        // están en `comprometido`, así que sumar su devolución los contaría dos veces a favor.
        var devuelto = asignaciones
            .Where(a => a.Estado is not (EstadoDeAsignacion.Anulada or EstadoDeAsignacion.Devuelta))
            .Sum(a => a.Devuelto);

        return fondo.SaldoDisponible(comprometido, devuelto);
    }

    public async Task GuardarFondoAsync(
        FondoDeCombustible fondo, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Fondos
            .Include(f => f.Movimientos)
            .SingleOrDefaultAsync(f => f.Id == fondo.Id, cancelacion);

        if (fila is null)
        {
            fila = new FilaDeFondo
            {
                Id = fondo.Id,
                Ambito = fondo.Ambito,
                AmbitoDeclarado = fondo.AmbitoDeclarado,
                Desde = fondo.Desde,
                Hasta = fondo.Hasta,
                Solicita = fondo.Solicita.Valor,
            };
            contexto.Fondos.Add(fila);
        }

        // Estas dos sí se actualizan, y no contradice a P-1: no son estado proyectable sino
        // datos del expediente que se completan — `RN-26` prevé expresamente que la partida
        // llegue después.
        fila.Aprueba = fondo.Aprueba?.Valor;
        fila.PartidaPresupuestaria = fondo.PartidaPresupuestaria;

        // Sólo agrega. Un asiento escrito no se actualiza ni se borra (P-3).
        for (var orden = fila.Movimientos.Count; orden < fondo.Diario.Count; orden++)
        {
            var m = fondo.Diario[orden];

            fila.Movimientos.Add(new FilaDeMovimientoDelFondo
            {
                Id = Ulid.NewUlid(),
                FondoId = fondo.Id,
                Orden = orden,
                Movimiento = m.Id,
                Destino = m.Destino,
                Ejecuta = m.Ejecuta.Valor,
                MomentoUtc = m.Momento.UtcDateTime,
                DesfaseMinutos = (int)m.Momento.Offset.TotalMinutes,
                Motivo = m.Motivo,
                Monto = m.Monto,
            });
        }

        await contexto.SaveChangesAsync(cancelacion);
    }

    // ── Asignaciones ────────────────────────────────────────────────────────

    public async Task<AsignacionDeCombustible?> BuscarAsignacionAsync(
        Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .SingleOrDefaultAsync(a => a.Id == id, cancelacion);

        return fila is null ? null : AAsignacion(fila);
    }

    public async Task<IReadOnlyList<AsignacionDeCombustible>> DeLaMisionAsync(
        Ulid misionId, CancellationToken cancelacion = default)
    {
        var filas = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .Where(a => a.MisionId == misionId)
            .ToListAsync(cancelacion);

        return [.. filas.Select(AAsignacion)];
    }

    /// <summary>
    /// Los vales de una persona — la pregunta de `RN-86`: <b>¿qué tiene esta persona en la
    /// mano?</b>
    ///
    /// Va por <see cref="FilaDeAsignacion.Receptor"/> y no por quién ejecutó `V-02`: el que
    /// entrega es de la institución, y el que responde por el dinero es el que lo recibió
    /// (`CE-26`: <i>«la persona que firmó la recepción, no el rol»</i>).
    /// </summary>
    public async Task<IReadOnlyList<AsignacionDeCombustible>> DelReceptorAsync(
        Ulid receptor, CancellationToken cancelacion = default)
    {
        var filas = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .Where(a => a.Receptor == receptor)
            .ToListAsync(cancelacion);

        return [.. filas.Select(AAsignacion)];
    }

    public async Task<IReadOnlyList<AsignacionDeCombustible>> AsignacionesDelFondoAsync(
        Ulid fondoId, CancellationToken cancelacion = default)
    {
        var filas = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .Where(a => a.FondoId == fondoId)
            .ToListAsync(cancelacion);

        return [.. filas.Select(AAsignacion)];
    }

    /// <summary>
    /// El recuento que `T-15`, `T-19`, `T-21` y `T-22` necesitan para decidir.
    ///
    /// Se calcula acá y no en cada llamador porque los cuatro números tienen que salir de la
    /// <b>misma</b> lectura: contar «con consumo» en un lugar y «sin liquidar» en otro deja la
    /// puerta a que dos consultas vean estados distintos del mismo vale.
    /// </summary>
    public async Task<RecuentoDeAsignaciones> RecuentoDeLaMisionAsync(
        Ulid misionId, CancellationToken cancelacion = default)
    {
        var asignaciones = await DeLaMisionAsync(misionId, cancelacion);

        return new RecuentoDeAsignaciones(
            Total: asignaciones.Count,

            // Anuladas y devueltas cuentan como resueltas: `RN-26` dice «liquidadas o
            // formalmente anuladas», y una devuelta íntegra ya no tiene nada que liquidar.
            SinLiquidar: asignaciones.Count(a => !a.EstaResuelta),

            SinConciliar: asignaciones.Count(a => a.Estado
                is not (EstadoDeAsignacion.Conciliada
                     or EstadoDeAsignacion.ConciliadaConDesviacion
                     or EstadoDeAsignacion.Anulada
                     or EstadoDeAsignacion.Devuelta)),

            ConConsumo: asignaciones.Count(a => a.TuvoConsumo),

            EntregadasSinDevolver: asignaciones.Count(a => a.Estado is EstadoDeAsignacion.Entregada));
    }

    public async Task GuardarAsignacionAsync(
        AsignacionDeCombustible asignacion, CancellationToken cancelacion = default)
    {
        var fila = await contexto.AsignacionesDeCombustible
            .Include(a => a.Transiciones)
            .SingleOrDefaultAsync(a => a.Id == asignacion.Id, cancelacion);

        if (fila is null)
        {
            fila = new FilaDeAsignacion
            {
                Id = asignacion.Id,
                Folio = asignacion.Folio,
                FondoId = asignacion.Fondo,
                MisionId = asignacion.Mision,
                VehiculoId = asignacion.Vehiculo,
                Receptor = asignacion.Receptor,
                Monto = asignacion.Monto,
                Galones = asignacion.Galones,
                Instrumento = asignacion.Instrumento,
                TipoDeCombustible = asignacion.TipoDeCombustible,
            };
            contexto.AsignacionesDeCombustible.Add(fila);
        }

        for (var orden = fila.Transiciones.Count; orden < asignacion.Diario.Count; orden++)
        {
            var t = asignacion.Diario[orden];

            fila.Transiciones.Add(new FilaDeTransicionDeAsignacion
            {
                Id = Ulid.NewUlid(),
                AsignacionId = asignacion.Id,
                Orden = orden,
                Transicion = t.Id,
                Destino = t.Destino,
                Ejecuta = t.Ejecuta.Valor,
                MomentoUtc = t.Momento.UtcDateTime,
                DesfaseMinutos = (int)t.Momento.Offset.TotalMinutes,
                IdDeCaptura = t.IdDeCaptura,
                Motivo = t.Motivo,
                ConsumoGalones = t.Consumo?.Galones,
                ConsumoMonto = t.Consumo?.Monto,
                ConsumoEstacion = t.Consumo?.Estacion,
                ConsumoOdometro = t.Consumo?.Odometro,
                ConsumoComprobante = t.Consumo?.Comprobante,
                ConsumoCausaSinComprobante = t.Consumo?.CausaSinComprobante,
                Devuelto = t.Devuelto,
            });
        }

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// El identificador de la fila de un asiento del vale, por su posición en el diario.
    ///
    /// Existe para que el abastecimiento de `RN-83` pueda apuntar al asiento `V-04` que lo
    /// produjo: es lo que ata las dos filas y lo que el índice único usa para impedir que el
    /// mismo galón se cuente dos veces en el denominador de `RN-30`.
    /// </summary>
    public async Task<Ulid> IdDeLaTransicionAsync(
        Ulid asignacionId, int orden, CancellationToken cancelacion = default) =>
        await contexto.Set<FilaDeTransicionDeAsignacion>()
            .Where(t => t.AsignacionId == asignacionId && t.Orden == orden)
            .Select(t => t.Id)
            .SingleAsync(cancelacion);

    // ── Rehidratación ───────────────────────────────────────────────────────

    private static FondoDeCombustible AFondo(FilaDeFondo fila) =>
        FondoDeCombustible.Reconstruir(
            fila.Id, fila.Ambito, fila.AmbitoDeclarado, fila.Desde, fila.Hasta,
            new IdPersona(fila.Solicita),
            fila.Aprueba is null ? null : new IdPersona(fila.Aprueba),
            fila.PartidaPresupuestaria,
            fila.Movimientos
                .OrderBy(m => m.Orden)
                .Select(m => new MovimientoDelFondo(
                    m.Movimiento, m.Destino, new IdPersona(m.Ejecuta),
                    Momento(m.MomentoUtc, m.DesfaseMinutos), m.Motivo, m.Monto)));

    private static AsignacionDeCombustible AAsignacion(FilaDeAsignacion fila) =>
        AsignacionDeCombustible.Reconstruir(
            fila.Id, fila.Folio, fila.FondoId, fila.MisionId, fila.VehiculoId,
            fila.Receptor, fila.Monto, fila.Galones,
            fila.Instrumento, fila.TipoDeCombustible,
            fila.Transiciones
                .OrderBy(t => t.Orden)
                .Select(t => new TransicionDeAsignacion(
                    t.Transicion, t.Destino, new IdPersona(t.Ejecuta),
                    Momento(t.MomentoUtc, t.DesfaseMinutos), t.Motivo, t.IdDeCaptura,
                    // Los cinco o ninguno. Un consumo con galones y sin odómetro no es un
                    // estado que el dominio pueda representar — la regla lo rechaza al
                    // registrarlo, así que rehidratarlo a medias sería inventar un caso que
                    // nunca se pudo escribir.
                    t.ConsumoGalones is { } galones && t.ConsumoMonto is { } monto
                        && t.ConsumoEstacion is { } estacion && t.ConsumoOdometro is { } odometro
                        ? new ConsumoRegistrado(galones, monto, estacion, odometro,
                                                t.ConsumoComprobante, t.ConsumoCausaSinComprobante)
                        : null,
                    t.Devuelto)));

    private static DateTimeOffset Momento(DateTime utc, int desfaseMinutos) =>
        new DateTimeOffset(utc, TimeSpan.Zero).ToOffset(TimeSpan.FromMinutes(desfaseMinutos));
}
