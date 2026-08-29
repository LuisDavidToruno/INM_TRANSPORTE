using Sigti.Dominio.M02_Parametros;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// Dinero público en poder de una persona nominada — `RN-86` y `CE-26` §1.
///
/// ── Por qué es una proyección y no un estado del vale ───────────────────────
/// `CE-26` §1 propone incorporar a la máquina de la asignación el estado
/// `PENDIENTE_DE_DEVOLUCION`. La <b>autoridad en transiciones</b> es
/// <c>docs/03-arquitectura/estados/orden-de-mision.md</c> §10.1, y ahí ese estado
/// <b>no existe</b>: la máquina va de `CONSUMIDA` a `LIQUIDADA`.
///
/// Acá se calcula en vez de agregarse al enum, por dos razones. La sustancia de `CE-26` es
/// que el hueco <b>sea visible</b> —<i>«el saldo no devuelto es un estado, no un vacío»</i>—
/// y eso lo da el arqueo. Y agregarle un estado a la máquina autoridad desde el módulo que la
/// consume sería resolver la contradicción en silencio, que es justo lo que no se hace.
/// <b>Queda como hallazgo para el PO.</b>
///
/// ── Lo que este objeto contesta ─────────────────────────────────────────────
/// La primera pregunta de un arqueo: <i>quién tiene cuánto dinero del Estado en la mano,
/// desde cuándo</i>.
/// </summary>
/// <param name="Monto">
/// Asignado menos consumido menos devuelto. <b>El sistema lo calcula</b> (`CE-26` §1) y nunca
/// lo ajusta para que cuadre.
/// </param>
/// <param name="Desde">
/// La <b>fecha del hecho del retorno</b> (`T-18`), no la de captura ni la de sincronización.
/// Nula mientras la misión no haya retornado: el plazo todavía no empezó a correr, y el
/// dinero está afuera legítimamente.
/// </param>
/// <param name="Vence">
/// Cuándo se vence el plazo. <b>Nulo es «no se puede saber», nunca «no vence»</b>, y la
/// <see cref="Explicacion"/> dice cuál de los dos motivos es.
/// </param>
public sealed record SaldoAfuera(
    Ulid Asignacion,
    string FolioDelVale,
    Ulid Responsable,
    Ulid Mision,
    string ReferenciaDeLaMision,
    decimal Monto,
    DateOnly? Desde,
    DateOnly? Vence,
    string Explicacion)
{
    /// <summary>
    /// <b>Vencido</b> es lo que `RN-86` bloquea — no el saldo a secas. Sin fecha de
    /// vencimiento no se vence nada: declarar vencido lo que no se pudo fechar sería
    /// inventarle un plazo a la institución.
    /// </summary>
    public bool VencidoAl(DateOnly fecha) => Vence is { } v && fecha > v;

    public int DiasAfueraAl(DateOnly fecha) =>
        Desde is { } d ? fecha.DayNumber - d.DayNumber : 0;
}

/// <summary>
/// Cómo se calcula lo que está afuera y cuándo vence — `RN-86` punto 1 y `CE-26` §1.
/// </summary>
public static class ReglasDelSaldoAfuera
{
    /// <summary>
    /// El saldo de un vale, con su plazo resuelto contra el calendario vigente.
    /// </summary>
    /// <param name="retornoDeLaMision">
    /// La fecha del hecho de `T-18`. Nula mientras la misión no retorne.
    /// </param>
    /// <param name="plazoEnDiasHabiles">
    /// El parámetro `plazo_devolucion_saldo`. <b>Nulo mientras la institución no lo defina</b>
    /// — `[C]`, insumo #32. Y nulo <b>no es cero</b>: con cero, todo saldo estaría vencido el
    /// mismo día del retorno y el sistema bloquearía a la flota entera por un dato que nadie
    /// entregó. `RN-86` es explícito sobre lo que cuesta no tenerlo: <i>«sin plazo definido,
    /// el sistema no puede decir si el dinero estuvo afuera dos días o dos meses»</i>.
    /// </param>
    public static SaldoAfuera? De(
        Ulid asignacion,
        string folioDelVale,
        Ulid responsable,
        Ulid mision,
        string referenciaDeLaMision,
        decimal asignado,
        decimal consumido,
        decimal devuelto,
        bool valeResuelto,
        DateOnly? retornoDeLaMision,
        int? plazoEnDiasHabiles,
        CalendarioDeDiasHabiles calendario)
    {
        // Un vale liquidado, conciliado, anulado o devuelto ya no tiene dinero afuera: su
        // descargo está hecho. Lo que quede sin explicar después de eso es materia de la
        // obligación de reintegro, que es otra entidad y sobrevive por su cuenta.
        if (valeResuelto) return null;

        var pendiente = asignado - consumido - devuelto;

        if (pendiente <= 0) return null;

        var (vence, explicacion) = ResolverPlazo(retornoDeLaMision, plazoEnDiasHabiles, calendario);

        return new SaldoAfuera(
            asignacion, folioDelVale, responsable, mision, referenciaDeLaMision,
            pendiente, retornoDeLaMision, vence, explicacion);
    }

    private static (DateOnly? Vence, string Explicacion) ResolverPlazo(
        DateOnly? retorno, int? plazo, CalendarioDeDiasHabiles calendario)
    {
        if (retorno is not { } desde)
            return (null,
                "La misión no ha retornado: el plazo de devolución todavía no empieza a correr. " +
                "El dinero está afuera y eso es lo normal mientras la misión esté en ruta.");

        if (plazo is not { } dias)
            return (null,
                "`plazo_devolucion_saldo` no está definido (`[C]`, insumo #32), así que no se " +
                "puede decir si este saldo está vencido. Está afuera desde el " +
                $"{desde:dd/MM/yyyy} y eso sí consta.");

        var vencimiento = calendario.SumarDiasHabiles(desde, dias);

        return (vencimiento,
            $"Retorno del {desde:dd/MM/yyyy}, {dias} día(s) hábil(es) de plazo según el " +
            $"calendario {calendario.Version}: vence el {vencimiento:dd/MM/yyyy}.");
    }
}
