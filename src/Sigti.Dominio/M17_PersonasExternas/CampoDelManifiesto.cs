using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M17_PersonasExternas;

/// <summary>
/// Los campos que el manifiesto de personas externas puede capturar — `RN-51`, `HU-112`.
///
/// ── La medida de protección más barata es no capturar ───────────────────────
/// `RN-51` lo dice sin rodeos: <i>«un dato que no se captura no se puede filtrar, no se puede
/// publicar por error y no se puede pedir por hábeas data»</i>.
///
/// Por eso el catálogo es <b>cerrado</b> —identificación, institución o condición que motiva el
/// traslado, origen y destino— y todo lo demás exige fundamento.
///
/// ── Y no hay ley de datos personales que invocar ────────────────────────────
/// No la hay vigente en Honduras `[V]`, y `DP-001 D-14` decidió <b>no diseñar para
/// anticiparla</b>. Lo que sí está vigente es el <b>hábeas data del Artículo 182</b> `[V]`: si
/// una persona pregunta quién vio sus datos, la única respuesta defendible es el registro de
/// consultas. Sin él, la institución no puede afirmar nada.
/// </summary>
public sealed record CampoDelManifiesto(
    string Clave,
    string Etiqueta,
    ClaseDelCampo Clase,
    bool Activo,
    FundamentoDelCampo? Fundamento)
{
    /// <summary>
    /// Si está activo, es sensible, y nadie registró por qué.
    ///
    /// <b>Es un estado real y visible</b>, no un error de configuración: `HU-112` deja que el
    /// campo se active y lo <b>marca</b>. Ver <see cref="ReglasDelCampoSensible"/>.
    /// </summary>
    public bool SinFundamento =>
        Activo && Clase != ClaseDelCampo.Minimo && Fundamento is null;
}

/// <summary>
/// De qué clase es el dato. Las cuatro sensibles son las que `NRM-07` enumera.
/// </summary>
public enum ClaseDelCampo
{
    /// <summary>Del catálogo autorizado por `RN-51`. No exige fundamento.</summary>
    Minimo,

    Salud,
    Etnia,
    SituacionMigratoria,
    CondicionDeVulnerabilidad,
}

/// <param name="BaseLegal">Qué norma autoriza capturar el dato.</param>
/// <param name="NecesidadOperativa">
/// Para qué operación del traslado hace falta. <b>Es la mitad que se olvida</b>, y sin ella la
/// base legal sola justifica capturar cualquier cosa que la norma no prohíba.
/// </param>
public sealed record FundamentoDelCampo(
    string BaseLegal,
    string NecesidadOperativa,
    IdPersona Registra,
    DateTimeOffset Momento);

/// <summary>
/// Qué se exige para activar un campo sensible — `HU-112`.
/// </summary>
public static class ReglasDelCampoSensible
{
    /// <summary>Las clases que `NRM-07` nombra como sensibles.</summary>
    public static bool EsSensible(ClaseDelCampo clase) => clase != ClaseDelCampo.Minimo;

    /// <summary>
    /// Qué pasa al activar un campo sensible sin fundamento.
    ///
    /// ── ⚠️ Advierte y marca; <b>no bloquea</b> ──────────────────────────────
    /// Es lo que `HU-112` dice, y va contra la intuición: <i>«el sistema activa el campo Y lo
    /// marca como CAMPO SIN FUNDAMENTO REGISTRADO»</i>.
    ///
    /// Bloquear parece más seguro y es peor: quien necesita el campo hoy lo va a capturar
    /// igual —en el campo de observaciones, en una libreta, en un WhatsApp— y ahí sí queda
    /// fuera de todo control. Marcado, el dato está dentro del sistema, con su acceso
    /// registrado, y <b>aparece en el reporte que el Auditor Interno revisa</b> hasta que
    /// alguien lo fundamente.
    /// </summary>
    public static string? AdvertenciaAlActivar(ClaseDelCampo clase, FundamentoDelCampo? fundamento)
    {
        if (!EsSensible(clase) || fundamento is not null) return null;

        return $"Activó un campo de clase {EnPalabras(clase)} sin base legal ni necesidad " +
               "operativa. El campo queda marcado y se reporta a Auditoría Interna hasta que " +
               "registre el fundamento.";
    }

    /// <summary>
    /// El fundamento exige <b>las dos cosas</b>. Media justificación se rechaza.
    ///
    /// ── Por qué la necesidad operativa no es un formalismo ──────────────────
    /// La base legal sola autoriza capturar <b>todo lo que la norma no prohíba</b>, que en un
    /// país sin ley de datos es casi todo. La pregunta que limita de verdad es la otra:
    /// <i>¿para qué operación del traslado hace falta este dato?</i> — y hay campos que no la
    /// pueden contestar.
    /// </summary>
    public static void ExigirFundamentoCompleto(string? baseLegal, string? necesidadOperativa)
    {
        var faltaLegal = string.IsNullOrWhiteSpace(baseLegal);
        var faltaNecesidad = string.IsNullOrWhiteSpace(necesidadOperativa);

        if (faltaLegal || faltaNecesidad)
            throw new BloqueoDuro("RN-51",
                "El fundamento requiere las dos cosas: la base legal que autoriza el dato y " +
                "para qué operación del traslado se necesita.");
    }

    /// <summary>
    /// La salida que `RN-51` propone para el caso que más se repite.
    ///
    /// <i>«Traslado que operativamente exige un dato de salud — persona que requiere ambulancia
    /// o asistencia. El campo no se agrega al manifiesto general: se registra como
    /// <b>requerimiento operativo del traslado</b> (necesita camilla, requiere acompañante) sin
    /// consignar diagnóstico. La necesidad se satisface sin capturar el dato sensible.»</i>
    /// </summary>
    public const string LaSalidaSinCapturarElDato =
        "Si lo que hace falta es operar el traslado —una camilla, un acompañante, una silla de " +
        "ruedas—, regístrelo como requerimiento operativo del traslado y no como dato de la " +
        "persona. La necesidad se satisface sin capturar el diagnóstico.";

    public static string EnPalabras(ClaseDelCampo clase) => clase switch
    {
        ClaseDelCampo.Salud => "salud",
        ClaseDelCampo.Etnia => "etnia",
        ClaseDelCampo.SituacionMigratoria => "situación migratoria",
        ClaseDelCampo.CondicionDeVulnerabilidad => "condición de vulnerabilidad",
        _ => "mínimo",
    };
}
