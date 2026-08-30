using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M17_PersonasExternas;

/// <summary>
/// Quién va a bordo — `RN-53`, `HU-113`, `HU-116`.
///
/// ── El manifiesto es una declaración, no un resumen ─────────────────────────
/// `RN-53`: <i>«El manifiesto es la declaración de para qué salió el vehículo. Si se puede
/// editar después, deja de ser una declaración y pasa a ser <b>un resumen ajustado a lo que
/// ocurrió</b> — que es exactamente lo contrario de un control.»</i>
///
/// Por eso al despachar <b>se cierra</b>, y todo lo que pasa después es una novedad de ruta que
/// se suma sin tocarlo. La liquidación compara las dos cosas.
/// </summary>
public sealed record Manifiesto(
    Ulid Id,
    Ulid Mision,
    IReadOnlyList<PersonaEnManifiesto> Personas,
    DateTimeOffset? CerradoEl,
    IdPersona? CierraQuien,
    IReadOnlyList<NovedadDeRuta> Novedades)
{
    public bool EstaCerrado => CerradoEl is not null;

    /// <summary>
    /// Quiénes iban <b>según lo declarado</b>. No cambia después del cierre.
    /// </summary>
    public int Declaradas => Personas.Count;

    /// <summary>
    /// Quiénes fueron de verdad, según las novedades. <b>Se calcula, no se guarda</b>: guardarlo
    /// sería una segunda cifra que se puede desincronizar de los asientos que la sostienen.
    /// </summary>
    public int Efectivas =>
        Declaradas
        - Novedades.Count(n => n.Tipo is TipoDeNovedad.NoSePresento)
        + Novedades.Count(n => n.Tipo is TipoDeNovedad.SubioEnRuta);

    /// <summary>
    /// Si lo declarado y lo ocurrido no coinciden. <b>Es lo que la liquidación señala</b>: no es
    /// un error, es la diferencia que alguien tiene que explicar.
    /// </summary>
    public bool HayDiferencias => Novedades.Count > 0;
}

/// <param name="Identificacion">
/// <b>Nula cuando la persona no se identificó.</b> Ver <see cref="FormaDeIdentificacion"/>: nulo
/// no significa que no se sepa quién es — significa que no trajo documento, y eso es un caso
/// previsto, no un dato faltante.
/// </param>
/// <param name="QueMotivaElTraslado">
/// La institución o condición que motiva el traslado. Del catálogo mínimo de `RN-51`: <b>no es
/// el motivo personal</b> de la persona, que sería un dato sensible por la puerta de atrás.
/// </param>
public sealed record PersonaEnManifiesto(
    string? Nombre,
    string? Identificacion,
    FormaDeIdentificacion Forma,
    string QueMotivaElTraslado,
    string Origen,
    string Destino,

    /// <summary>
    /// Lo que el traslado necesita operativamente — camilla, acompañante, silla de ruedas.
    ///
    /// ⚠️ <b>No es un dato de salud y no debe serlo.</b> `RN-51`: la necesidad se satisface
    /// <i>«sin consignar diagnóstico»</i>. Escribir acá una condición médica mete un dato
    /// sensible por un campo que no lo pide.
    /// </summary>
    string? RequerimientoOperativo);

public enum FormaDeIdentificacion
{
    /// <summary>Tarjeta de identidad, pasaporte, carné institucional.</summary>
    Documento,

    /// <summary>
    /// Otra cosa que permite identificarla: constancia, número de expediente de otra
    /// institución, declaración de un tercero identificado.
    /// </summary>
    Alternativa,

    /// <summary>
    /// No se identificó. <b>Es un caso previsto</b>, no un registro incompleto.
    /// </summary>
    NoIdentificada,
}

/// <param name="Tipo">Qué pasó. Del catálogo de `HU-116`, no texto libre.</param>
/// <param name="FechaDelHecho">
/// Cuándo pasó, no cuándo se registró (`RN-46`). Una novedad capturada al volver describe algo
/// que ocurrió en la carretera.
/// </param>
public sealed record NovedadDeRuta(
    Ulid Id,
    TipoDeNovedad Tipo,
    string? AQuien,
    string Motivo,
    string? DondePaso,
    DateTimeOffset FechaDelHecho,
    IdPersona Registra,
    IdPersona? Autoriza);

public enum TipoDeNovedad
{
    /// <summary>Estaba en la lista y no llegó. La más frecuente.</summary>
    NoSePresento,

    /// <summary>
    /// Subió alguien que no estaba declarado. <b>Exige quién lo autorizó</b>: es lo que separa
    /// una decisión operativa de un favor.
    /// </summary>
    SubioEnRuta,

    /// <summary>Bajó antes del destino declarado.</summary>
    BajoAntes,
}

/// <summary>
/// Qué se puede hacer con el manifiesto, y cuándo — `RN-53`.
/// </summary>
public static class ReglasDelManifiesto
{
    /// <summary>
    /// Exigir documento <b>no impide que la persona suba</b>: impide que figure.
    ///
    /// ── `HU-113`, y por qué es una regla y no una comodidad ─────────────────
    /// <i>«Para que el traslado salga amparado y con constancia de quién iba, <b>en lugar de que
    /// la persona suba sin figurar en ningún papel</b>.»</i>
    ///
    /// El vehículo sale igual. Lo único que cambia es si queda constancia. Un campo obligatorio
    /// de identidad produce manifiestos con menos gente de la que viajó — que es peor que un
    /// manifiesto con una persona no identificada, porque el primero <b>miente</b> y el segundo
    /// declara lo que sabe.
    /// </summary>
    public static void ExigirIdentificacionCoherente(
        FormaDeIdentificacion forma, string? identificacion)
    {
        if (forma is FormaDeIdentificacion.NoIdentificada) return;

        if (string.IsNullOrWhiteSpace(identificacion))
            throw new BloqueoDuro("RN-51",
                $"Declaró identificación de tipo «{forma}» y no puso cuál. Si la persona no " +
                "trae documento ni forma alternativa, regístrela como no identificada: el " +
                "traslado sale igual y queda constancia de que iba.");
    }

    /// <summary>
    /// El cierre al despachar. <b>Después de esto el manifiesto no se toca.</b>
    /// </summary>
    public static void ExigirAbierto(Manifiesto manifiesto)
    {
        if (manifiesto.EstaCerrado)
            throw new BloqueoDuro("RN-53",
                $"El manifiesto se cerró el {manifiesto.CerradoEl:yyyy-MM-dd HH:mm} al " +
                "despachar. Lo que cambió después se registra como novedad de ruta, con su " +
                "fecha del hecho y su motivo — el manifiesto original queda como lo que se " +
                "autorizó, y la liquidación compara los dos.");
    }

    /// <summary>Una novedad sólo tiene sentido sobre un manifiesto ya cerrado.</summary>
    public static void ExigirCerrado(Manifiesto manifiesto)
    {
        if (!manifiesto.EstaCerrado)
            throw new BloqueoDuro("RN-53",
                "El manifiesto todavía no se cerró: la misión no ha salido. Mientras esté " +
                "abierto, agregue o quite personas directamente — una novedad describe lo que " +
                "cambió respecto de lo declarado, y todavía no hay nada declarado.");
    }

    /// <summary>
    /// Quien sube en ruta a alguien que no estaba declarado <b>tiene que decir quién lo
    /// autorizó</b>.
    ///
    /// Es la novedad que más se presta: el vehículo institucional que lleva a un conocido. Con
    /// autorización nombrada es una decisión de alguien; sin ella, es un favor que nadie firmó.
    /// </summary>
    public static void ExigirAutorizacionSiSubio(TipoDeNovedad tipo, IdPersona? autoriza)
    {
        if (tipo != TipoDeNovedad.SubioEnRuta) return;

        if (autoriza is null || string.IsNullOrWhiteSpace(autoriza.Value.Valor))
            throw new BloqueoDuro("RN-53",
                "Diga quién autorizó que subiera. Sin eso, el traslado de una persona que no " +
                "estaba declarada no tiene responsable — y es la novedad que un auditor mira " +
                "primero.");
    }

    /// <summary>El motivo de la novedad, que va al expediente.</summary>
    public static void ExigirMotivo(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 8)
            throw new BloqueoDuro("RN-53",
                "Escriba qué pasó. La liquidación compara el manifiesto con las novedades, y " +
                "una diferencia sin explicación es lo que queda como hallazgo.");
    }
}
