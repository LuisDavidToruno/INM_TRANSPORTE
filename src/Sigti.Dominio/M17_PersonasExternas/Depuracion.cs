using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M17_PersonasExternas;

/// <summary>
/// La depuración de datos personales al vencer su plazo — `HU-124`, `RN-51` punto 4.
///
/// ── Es lo único en todo el sistema que destruye contenido ───────────────────
/// Todo lo demás se reversa, se anula o se marca; nada se borra. Esto sí borra, y por eso lleva
/// tres bloqueos que no están para incomodar:
///
/// <b>Sin plazo configurado no depura nada</b>, y no aplica ninguno por omisión. Un plazo por
/// omisión sería el equipo decidiendo cuánto conserva la institución los datos de las personas
/// que trasladó — que es exactamente la decisión que `[C]` deja a Auditoría Interna y al
/// Oficial de Información Pública.
///
/// <b>No toca lo financiero ni lo de bienes.</b> Esos se conservan por el plazo de fiscalización.
/// Borrarlos dejaría al Tribunal Superior de Cuentas sin con qué probar un asiento.
///
/// <b>No se ejecuta sin aviso previo.</b> Una destrucción silenciosa es indistinguible de una
/// pérdida de datos, y nadie podría decir cuál de las dos ocurrió.
///
/// ── El criterio de éxito es doble ───────────────────────────────────────────
/// Cero datos personales sobrevivientes <b>y</b> la cadena de auditoría verificando después. La
/// primera mitad sola se logra borrando la base entera.
/// </summary>
public static class ReglasDeLaDepuracion
{
    /// <summary>
    /// El plazo, en días, tras el cual los datos personales de un manifiesto se depuran.
    /// `[C]` — se acuerda con Auditoría Interna y el OIP.
    /// </summary>
    public const string ClaveDelPlazo = "personas.plazo_depuracion_dias";

    /// <summary>
    /// Lo que la depuración <b>nunca</b> alcanza.
    ///
    /// `HU-124`: <i>«Los registros financieros y de bienes se conservan por el plazo de
    /// fiscalización y no se depuran. La depuración alcanza únicamente el segmento de datos
    /// personales.»</i>
    /// </summary>
    public static readonly IReadOnlySet<string> FueraDeAlcance =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "liquidacion", "vale", "combustible", "abastecimiento", "fondo",
            "vehiculo", "flota", "expediente", "peaje", "saldo",
        };

    /// <summary>
    /// Sin plazo no se depura, y <b>no hay plazo por omisión</b>.
    /// </summary>
    /// <param name="plazoEnDias">
    /// <b>Nulo es «no está configurado»</b>. No se sustituye por un valor razonable: cuánto
    /// tiempo conserva la institución la identidad de quien trasladó no es una decisión técnica.
    /// </param>
    public static void ExigirPlazoConfigurado(int? plazoEnDias)
    {
        if (plazoEnDias is null or <= 0)
            throw new BloqueoDuro("RN-51",
                "Depuración no ejecutada: el plazo de depuración de datos personales no está " +
                "configurado. Acuerde el plazo con Auditoría Interna y el Oficial de " +
                "Información Pública.");
    }

    /// <summary>
    /// La depuración alcanza <b>sólo</b> el segmento de datos personales.
    /// </summary>
    public static void ExigirSoloDatosPersonales(IEnumerable<string> segmentos)
    {
        var prohibidos = segmentos.Where(FueraDeAlcance.Contains).ToList();

        if (prohibidos.Count > 0)
            throw new BloqueoDuro("RN-51",
                "Los registros financieros y de bienes se conservan por el plazo de " +
                "fiscalización y no se depuran. La depuración alcanza únicamente el segmento " +
                $"de datos personales. Quedan fuera: {string.Join(", ", prohibidos)}.");
    }

    /// <summary>
    /// No se ejecuta sin aviso previo.
    ///
    /// ── Por qué el aviso no es una formalidad ───────────────────────────────
    /// Una destrucción silenciosa <b>es indistinguible de una pérdida de datos</b>. El día que
    /// alguien busque un manifiesto de hace tres años y no esté, nadie va a poder decir si se
    /// depuró conforme al plazo o si se perdió — y la institución tendrá que responder por lo
    /// segundo sin poder probar lo primero.
    /// </summary>
    /// <param name="avisadoEl">Nulo cuando no se emitió aviso.</param>
    public static void ExigirAvisoPrevio(DateTimeOffset? avisadoEl, DateTimeOffset ejecucion)
    {
        if (avisadoEl is null)
            throw new BloqueoDuro("RN-51",
                "La depuración se anuncia con antelación al responsable y queda en la pantalla " +
                "de estado. No se ejecuta sin aviso previo.");

        if (avisadoEl.Value >= ejecucion)
            throw new BloqueoDuro("RN-51",
                $"El aviso es del {avisadoEl:yyyy-MM-dd} y la ejecución del " +
                $"{ejecucion:yyyy-MM-dd}: el aviso tiene que ser **previo**. Avisar el mismo " +
                "día no le da a nadie tiempo de objetar.");
    }

    /// <summary>
    /// Qué manifiestos alcanzó el plazo.
    ///
    /// Se cuenta desde la <b>fecha del hecho</b> —cuándo se cerró el manifiesto—, no desde la
    /// captura: un manifiesto digitado con tres meses de retraso no gana tres meses de
    /// conservación por eso.
    /// </summary>
    public static bool AlcanzoElPlazo(
        DateTimeOffset cerradoEl, int plazoEnDias, DateTimeOffset ahora) =>
        (ahora - cerradoEl).TotalDays >= plazoEnDias;
}

/// <summary>
/// Una rectificación por hábeas data — `HU-122`, `RN-04`.
///
/// ── Rectificar no es corregir ───────────────────────────────────────────────
/// El manifiesto original <b>queda intacto</b>. La rectificación es un asiento aparte que dice
/// qué decía, qué dice ahora y quién lo pidió — igual que una anulación es un asiento reverso y
/// no un borrado.
///
/// La razón no es formal: <i>«sin romper la cadena de auditoría que el Tribunal Superior de
/// Cuentas va a revisar»</i>. Un manifiesto editado deja de coincidir con la lista impresa que
/// el motorista llevó, y esa discrepancia aparece años después sin nadie que pueda explicarla.
/// </summary>
public sealed record Rectificacion(
    Ulid Id,
    Ulid Manifiesto,
    string Campo,

    /// <summary>Lo que decía. <b>No se pierde</b>: es lo que estaba en el papel.</summary>
    string ValorAnterior,

    string ValorRectificado,

    /// <summary>
    /// Quién la pidió. En un hábeas data <b>sólo el titular puede interponerlo</b>, así que este
    /// dato es parte de la legitimación del acto.
    /// </summary>
    string QuienLaPidio,

    string Motivo,
    IdPersona Registra,
    DateTimeOffset Momento);

public static class ReglasDeLaRectificacion
{
    /// <summary>
    /// Rectificar exige decir <b>quién lo pidió</b> y <b>por qué</b>.
    ///
    /// Sin el solicitante, la rectificación es indistinguible de una corrección interna — y una
    /// corrección interna sobre un dato personal es justamente lo que `RN-04` prohíbe hacer sin
    /// asiento.
    /// </summary>
    public static void Exigir(string? quienLaPidio, string? motivo)
    {
        if (string.IsNullOrWhiteSpace(quienLaPidio))
            throw new BloqueoDuro("RN-04",
                "Diga quién pidió la rectificación. El hábeas data sólo lo puede interponer el " +
                "titular, y sin ese dato el cambio se parece a una corrección interna.");

        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 8)
            throw new BloqueoDuro("RN-04",
                "Escriba por qué se rectifica. El manifiesto original queda como estaba, y " +
                "esto es lo que explica la diferencia entre los dos.");
    }
}
