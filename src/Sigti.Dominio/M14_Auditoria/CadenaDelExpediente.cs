using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Los eslabones que `ACT-12` revisa — su ficha los enumera en ese orden:
/// <i>«solicitud → autorización → orden de misión → bitácora → vale → comprobante →
/// liquidación»</i>.
///
/// <b>El orden es el de la cadena, no alfabético</b>: un hueco en el medio se ve porque los de
/// después están y el de antes también.
/// </summary>
public enum Eslabon
{
    Solicitud,
    Autorizacion,
    OrdenDeMision,
    Bitacora,
    Vale,
    Comprobante,
    Liquidacion,
}

/// <summary>
/// En qué situación está un eslabón.
///
/// ── Las tres no son grados de lo mismo ──────────────────────────────────────
/// <b><see cref="NoAplica"/> es la que hace útil al reporte.</b> Una misión sin combustible
/// asignado no tiene vale, y eso <b>no es un hueco</b>: es que no correspondía. Pintarlo como
/// faltante llenaría la pista de alarmas falsas, y una pista con alarmas falsas se deja de mirar
/// — que es exactamente perder el control que `NRM-01` exige.
/// </summary>
public enum EstadoDelEslabon
{
    /// <summary>Está, con su asiento y su autor.</summary>
    Presente,

    /// <summary>
    /// <b>Falta, y correspondía.</b> Es el hallazgo: la cadena se cortó donde no debía.
    /// </summary>
    Ausente,

    /// <summary>
    /// <b>No correspondía.</b> Una misión sin fondo no tiene vale ni comprobante; una que nunca
    /// salió no tiene bitácora.
    /// </summary>
    NoAplica,

    /// <summary>
    /// <b>Todavía no toca.</b> El expediente no llegó a esa etapa, y llamarlo hueco diría que
    /// algo se perdió cuando lo que pasa es que la misión sigue su curso.
    /// </summary>
    Pendiente,
}

/// <summary>Un eslabón resuelto, con lo que lo respalda.</summary>
/// <param name="Quien">
/// Quién lo ejecutó. <b>Nulo cuando el eslabón no está presente</b>: inventar un autor para algo
/// que no ocurrió es la peor forma de llenar un reporte de auditoría.
/// </param>
/// <param name="PorQue">
/// Por qué no aplica o por qué falta. <b>Nulo cuando está presente</b>: un motivo ahí es ruido.
/// </param>
public sealed record EslabonResuelto(
    Eslabon Eslabon,
    EstadoDelEslabon Estado,
    string? Referencia,
    IdPersona? Quien,
    DateOnly? Fecha,
    string? PorQue);

/// <summary>
/// La cadena de un expediente, de extremo a extremo — <b>`PT-089`</b>.
///
/// ── Por qué el inventario la llama «con sus huecos visibles» ────────────────
/// Porque un rastro que sólo muestra lo que está <b>no sirve para auditar</b>: lo que el TSC
/// busca es dónde se cortó la cadena. Un reporte que enumera cinco asientos presentes y calla
/// los dos que faltan es exactamente el reporte que deja pasar el hallazgo.
///
/// ── Y por qué distingue cuatro estados y no dos ─────────────────────────────
/// <i>«Falta»</i> y <i>«no correspondía»</i> se ven iguales en una casilla vacía y son
/// opuestos. Juntarlos produce dos daños: alarma sobre lo que está bien —y una pista con
/// alarmas falsas se deja de mirar— y silencio sobre lo que está mal.
/// </summary>
public sealed record CadenaDelExpediente(
    string Expediente,
    string Folio,
    IReadOnlyList<EslabonResuelto> Eslabones)
{
    /// <summary>Los huecos: lo que falta y correspondía. <b>Es el hallazgo.</b></summary>
    public IReadOnlyList<EslabonResuelto> Huecos =>
        [.. Eslabones.Where(e => e.Estado == EstadoDelEslabon.Ausente)];

    /// <summary>
    /// La cadena está completa cuando <b>no hay huecos y no queda nada pendiente</b>.
    ///
    /// Se exige lo segundo a propósito: una misión en curso no tiene huecos y tampoco está
    /// completa, y decir que sí daría por cerrado un expediente vivo.
    /// </summary>
    public bool Completa =>
        Eslabones.All(e => e.Estado is EstadoDelEslabon.Presente or EstadoDelEslabon.NoAplica);

    /// <summary>Cuántos eslabones no corresponden. Sirve para leer un reporte corto sin alarmarse.</summary>
    public int NoAplican => Eslabones.Count(e => e.Estado == EstadoDelEslabon.NoAplica);
}

/// <summary>
/// Arma la cadena a partir de lo que cada módulo sabe.
///
/// ── Qué NO hace ─────────────────────────────────────────────────────────────
/// <b>No consulta nada.</b> Recibe los hechos ya resueltos, igual que
/// <c>ActosDelExpediente</c>: los eslabones viven en `M-06`, `M-07`, `M-08` y `M-09`, y una
/// clase que fuera a buscarlos sería la única del dominio que conoce los cuatro.
/// </summary>
public static class ReglasDeLaCadena
{
    /// <summary>
    /// Resuelve un eslabón.
    /// </summary>
    /// <param name="corresponde">
    /// Si ese eslabón <b>debía existir</b> en este expediente. Es el dato que separa el hueco de
    /// lo que no aplicaba, y se recibe porque quien lo sabe es el módulo: la misión sabe si tuvo
    /// fondo asignado; esta clase no.
    /// </param>
    /// <param name="alcanzado">
    /// Si el expediente <b>llegó a la etapa</b>. Distingue «todavía no toca» de «falta»: una
    /// misión programada no tiene liquidación y eso no es un hallazgo.
    /// </param>
    public static EslabonResuelto Resolver(
        Eslabon eslabon,
        bool corresponde,
        bool alcanzado,
        string? referencia,
        IdPersona? quien,
        DateOnly? fecha,
        string? porQueNoCorresponde = null)
    {
        if (!corresponde)
        {
            return new EslabonResuelto(eslabon, EstadoDelEslabon.NoAplica, null, null, null,
                porQueNoCorresponde ?? "No correspondía a este expediente.");
        }

        if (referencia is not null)
        {
            // Presente: sin motivo, que ahí sería ruido.
            return new EslabonResuelto(
                eslabon, EstadoDelEslabon.Presente, referencia, quien, fecha, null);
        }

        if (!alcanzado)
        {
            return new EslabonResuelto(eslabon, EstadoDelEslabon.Pendiente, null, null, null,
                "El expediente todavía no llegó a esta etapa.");
        }

        // **El hallazgo.** Correspondía, el expediente pasó por la etapa, y no hay asiento.
        return new EslabonResuelto(eslabon, EstadoDelEslabon.Ausente, null, null, null,
            "Correspondía y no hay asiento. La cadena se cortó acá.");
    }
}
