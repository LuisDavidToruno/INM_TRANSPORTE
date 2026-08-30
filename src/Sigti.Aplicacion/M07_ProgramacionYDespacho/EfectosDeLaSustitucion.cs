using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M03_Flota;
using Sigti.Aplicacion.M18_Peajes;
using Sigti.Datos;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>
/// `RN-61` — <b>todo lo que arrastra sustituir el vehículo de una misión ya programada</b>.
///
/// ── Por qué esto tiene un lugar con nombre ──────────────────────────────────
/// La regla lista <b>nueve</b> valores derivados del vehículo que hay que recalcular, revalidar
/// o anular, y cada uno vive en un módulo distinto: peajes en `M-18`, salvoconducto en `M-15`,
/// vales en `M-09`, custodia en `M-03`. Repartidos por el enrutador serían nueve llamadas
/// sueltas que nadie puede contar, y <b>la forma en que una regla así se rompe es que alguien
/// agregue el décimo valor derivado y no toque las nueve llamadas</b>.
///
/// Acá están juntos, y lo que falta está declarado en el mismo sitio.
///
/// ── Y por qué corre DESPUÉS de la transición ────────────────────────────────
/// Si un bloqueo duro rechazara la reasignación, los efectos aplicados antes habrían dejado el
/// expediente hablando de un vehículo que la misión nunca tomó.
/// </summary>
public sealed class EfectosDeLaSustitucion(
    SigtiDbContext contexto,
    ServicioDePeajes peajes,
    ServicioDePermisos permisos,
    ConsultaDeFlota flota)
{
    /// <summary>
    /// Aplica los efectos y devuelve <b>qué pasó</b>, para que el enrutador lo pueda contestar.
    ///
    /// No lanza: la reasignación ya ocurrió y es válida. Lo que estos efectos producen son
    /// consecuencias que hay que <b>declarar</b>, no obstáculos que la impidan.
    /// </summary>
    public async Task<Arrastre> AplicarAsync(
        Ulid mision,
        Ulid vehiculoEntrante,
        DateOnly fechaDelHecho,
        string motivo,
        IdPersona quien,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        // ── 1 · El estimado de peajes ───────────────────────────────────────
        //
        // La categoría cambia con el vehículo y con ella la tarifa de cada caseta. Sin esto la
        // misión se liquida contra una cifra que ya no corresponde a ningún vehículo real.
        var peaje = await peajes.RecongelarPorSustitucionAsync(
            mision, vehiculoEntrante, fechaDelHecho, motivo, quien, momento, cancelacion);

        // ── 2 · El salvoconducto ────────────────────────────────────────────
        //
        // `RN-61`: <i>«se anula el anterior y se emite uno nuevo»</i>. Lo que se puede hacer
        // solo es la primera mitad — el permiso nuevo **necesita firma de la máxima autoridad**,
        // y eso no lo puede fabricar el sistema.
        //
        // ⚠️ Que la segunda mitad quede pendiente es exactamente por qué la primera no puede
        // esperar: mientras el papel viejo siga verificando como válido, el motorista puede
        // salir amparado en un documento que ya no lo ampara.
        var permisoReemitido = await ReemitirElPermisoSiHaceFaltaAsync(
            mision, motivo, quien, momento, cancelacion);

        // ── 3 · Los vales de combustible ────────────────────────────────────
        var vales = await ValesQueDejanDeCorresponderAsync(mision, vehiculoEntrante, cancelacion);

        return new Arrastre(peaje, permisoReemitido, vales);
    }

    /// <summary>
    /// Reemite el permiso de circulación cuando el vehículo sustituido lo dejó sin cubrir.
    ///
    /// <b>Nulo cuando no había permiso firmado</b> —la misión no circula en franja inhábil, o
    /// nunca se tramitó—, y eso no es un fallo.
    /// </summary>
    private async Task<string?> ReemitirElPermisoSiHaceFaltaAsync(
        Ulid mision, string motivo, IdPersona quien, DateTimeOffset momento,
        CancellationToken cancelacion)
    {
        var diagnosticados = await permisos.DiagnosticoDelExpedienteAsync(mision, cancelacion);

        // Sólo el que **exige** reemisión: si la misión se reprogramó con lo mismo, el permiso
        // sigue amparando y reemitirlo quemaría un folio y pediría una firma para nada.
        var caduco = diagnosticados.FirstOrDefault(d => d.ExigeReemision);

        if (caduco is null) return null;

        await permisos.ReemitirAsync(caduco.Permiso.Id, motivo, quien, momento, cancelacion);
        return caduco.Permiso.Folio;
    }

    /// <summary>
    /// Los vales <b>vivos</b> cuyo combustible ya no es el que el vehículo entrante usa.
    ///
    /// ── Por qué se reportan y no se anulan solos ────────────────────────────
    /// `RN-61` dice que se anulan folio por folio y se re-emiten. Anularlos acá <b>sin que nadie
    /// lo pida</b> haría desaparecer un vale que puede estar ya entregado —con dinero público
    /// fuera de la caja— o incluso consumido: la anulación de un vale entregado exige el acta
    /// de devolución de `RN-27`, y ese acto tiene su propia persona y su propio momento.
    ///
    /// Lo que sí es inaceptable es que nadie se entere. Se devuelven para que el enrutador y la
    /// pantalla lo digan.
    /// </summary>
    private async Task<IReadOnlyList<ValeQueYaNoCorresponde>> ValesQueDejanDeCorresponderAsync(
        Ulid mision, Ulid vehiculoEntrante, CancellationToken cancelacion)
    {
        var combustible = (await flota.PorIdAsync(vehiculoEntrante, cancelacion))?.TipoDeCombustible;

        // ⚠️ **Nulo es «la ficha no lo declara»**, no «coincide». Sin saber qué usa el vehículo
        // entrante no se puede afirmar que los vales sigan sirviendo — pero tampoco que no —, y
        // decir que están bien sería la afirmación falsa.
        if (string.IsNullOrWhiteSpace(combustible)) return [];

        var vivos = await contexto.AsignacionesDeCombustible
            .AsNoTracking()
            .Include(a => a.Transiciones)
            .Where(a => a.MisionId == mision)
            .ToListAsync(cancelacion);

        return
        [
            .. vivos
                .Where(a => a.Transiciones.MaxBy(t => t.Orden)?.Destino
                    is not (EstadoDeAsignacion.Anulada or EstadoDeAsignacion.Liquidada))
                .Where(a => !string.Equals(
                    a.TipoDeCombustible, combustible, StringComparison.OrdinalIgnoreCase))
                .Select(a => new ValeQueYaNoCorresponde(
                    a.Folio,
                    a.TipoDeCombustible,
                    combustible,
                    a.Transiciones.MaxBy(t => t.Orden)?.Destino.ToString() ?? "sin estado")),
        ];
    }
}

/// <param name="Peaje">
/// El asiento de diferencia. <b>Nulo cuando no había estimado congelado</b> — una misión
/// reasignada antes de aprobarse no tiene nada que recongelar.
/// </param>
/// <param name="PermisoReemitido">
/// El folio del permiso que se reemitió. <b>Nulo cuando no había ninguno que lo exigiera.</b>
///
/// ⚠️ El permiso nuevo <b>nace sin firma</b> y el salvoconducto anterior queda anulado: la
/// misión no puede despacharse en franja inhábil hasta que la máxima autoridad firme de nuevo y
/// se emita el documento. Es una consecuencia real de la sustitución, y por eso se devuelve.
/// </param>
/// <param name="Vales">
/// Los vales que dejaron de corresponder al combustible del vehículo. <b>Vacía es que ninguno
/// dejó de corresponder, o que la ficha del entrante no declara su combustible</b> — y las dos
/// cosas se distinguen mirando la ficha, no esta lista.
/// </param>
public sealed record Arrastre(
    DiferenciaDelRecongelamiento? Peaje,
    string? PermisoReemitido,
    IReadOnlyList<ValeQueYaNoCorresponde> Vales);

/// <param name="Estado">
/// En qué va el vale. <b>Decide qué se puede hacer con él</b>: uno emitido se anula; uno
/// entregado exige el acta de devolución de `RN-27`, porque hay dinero público fuera de la caja.
/// </param>
public sealed record ValeQueYaNoCorresponde(
    string Folio, string CombustibleDelVale, string CombustibleDelVehiculo, string Estado);
