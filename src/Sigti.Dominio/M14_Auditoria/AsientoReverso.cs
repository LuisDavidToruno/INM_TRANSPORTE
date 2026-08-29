using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Qué clase de reverso es — §8.2 y §8.3, artefacto autoridad.
/// </summary>
public enum NaturalezaDelReverso
{
    /// <summary>
    /// Declara que el valor registrado es incorrecto y cuál es el correcto. <b>No reemplaza el
    /// original</b>: §8.2 lo dice de los tres.
    /// </summary>
    CorreccionDeDato,

    /// <summary>Contrapartida de igual magnitud y signo contrario a un asiento previo.</summary>
    ReversoEconomico,

    /// <summary>
    /// Marca un folio como anulado y, si procede, emite un sustituto con folio nuevo.
    /// <b>Nunca se reemite el mismo folio con contenido distinto</b> (§8.3).
    /// </summary>
    AnulacionDeDocumento,
}

/// <summary>
/// A qué asiento concreto se refiere el reverso.
///
/// ── No existe el reverso genérico «de la misión» ────────────────────────────
/// §8.3 lo dice con esas palabras: <i>«referencia explícita al asiento o al dato que revierte.
/// <b>No existe el reverso genérico "de la misión"</b>: se revierte un asiento concreto»</i>.
///
/// Un reverso sin destinatario exacto no se puede verificar: nadie puede decir si ya se
/// revirtió, ni cuántas veces, ni contra qué.
/// </summary>
/// <param name="Tipo">
/// Qué clase de asiento es — transición de la misión, movimiento del fondo, asiento de la
/// bitácora, paso por caseta. Va porque el identificador solo no dice dónde buscarlo.
/// </param>
public sealed record ReferenciaAlAsiento(string Tipo, string Identificador, string Descripcion);

/// <summary>
/// Un asiento reverso — §8.3, con su contenido obligatorio completo.
///
/// ── El expediente cerrado lo MUESTRA, no lo esconde ─────────────────────────
/// §8.3: <i>«todo reporte sobre esa misión presenta el valor original, el reverso y el valor
/// resultante, con su cadena. <b>Nunca solo el resultado</b>»</i>. Por eso los dos valores van
/// siempre, y por eso el original no se toca.
/// </summary>
/// <param name="ValorAnterior">
/// <b>Siempre</b>, incluso cuando el nuevo es nulo (§8.3). Sin él, el reverso dice a qué se
/// llegó pero no de dónde se venía, y el reporte que exige mostrar los tres valores queda con
/// dos.
/// </param>
/// <param name="ValorNuevo">
/// <b>Siempre, incluso si es nulo.</b> Nulo es un valor: significa que el dato se declara sin
/// valor correcto conocido, y eso es distinto de no haberlo declarado.
/// </param>
/// <param name="FechaDelHechoOriginal">
/// La del asiento revertido. Es contra ella que se resuelven las tablas paramétricas si se
/// recalculó (`RN-40`).
/// </param>
/// <param name="PeriodoAfectado">
/// A qué período pertenece el asiento revertido. <b>El reverso se imputa al corriente</b> y
/// referencia a éste: los históricos ya publicados siguen siendo reproducibles.
/// </param>
/// <param name="EfectoEconomico">
/// Monto y signo, si lo tiene. <b>Nulo cuando el reverso es de dato o de documento</b>: una
/// corrección de un odómetro mal leído no mueve dinero, y ponerle cero diría que lo movió y
/// cuadró.
/// </param>
/// <param name="TablasParametricas">
/// Los identificadores de las tablas usadas para recalcular, si se recalculó (§8.3). Sin ellos
/// el recálculo no se puede reproducir, y un número que no se puede rehacer es una opinión.
/// </param>
public sealed record AsientoReverso(
    Ulid Id,
    ReferenciaAlAsiento Revertido,
    NaturalezaDelReverso Naturaleza,
    string ValorAnterior,
    string? ValorNuevo,
    DateOnly FechaDelHechoOriginal,
    DateTimeOffset FechaDelReverso,
    Autoria Autor,
    IdPersona Autoriza,
    IdPersona AutorDelAsientoOriginal,
    string MotivoTipificado,
    string Fundamento,
    string? Adjunto,
    string PeriodoAfectado,
    string PeriodoDeImputacion,
    decimal? EfectoEconomico = null,
    IReadOnlyList<string>? TablasParametricas = null)
{
    /// <summary>
    /// Cómo se lee en un reporte — §8.3: valor original, reverso y resultado, <b>nunca sólo el
    /// resultado</b>.
    /// </summary>
    public string Cadena =>
        $"{Revertido.Descripcion}: «{ValorAnterior}» → " +
        $"«{ValorNuevo ?? "SIN VALOR CORRECTO CONOCIDO"}»" +
        (EfectoEconomico is { } efecto ? $", efecto económico {efecto:N2}" : "") +
        $". Hecho del {FechaDelHechoOriginal:dd/MM/yyyy} (período {PeriodoAfectado}), " +
        $"revertido el {FechaDelReverso:dd/MM/yyyy} e imputado a {PeriodoDeImputacion}. " +
        $"{MotivoTipificado}: {Fundamento}";
}

/// <summary>
/// Los controles del asiento reverso — §8.3, artefacto autoridad.
/// </summary>
public static class ReglasDelAsientoReverso
{
    /// <summary>
    /// `BD-06` — <b>quien autoriza el reverso no puede ser quien produjo el asiento
    /// revertido.</b>
    ///
    /// Se verifica por <b>identidad de persona</b>, no por rol: un mismo servidor con dos
    /// cuentas sigue siendo la misma persona, y corregirse a sí mismo un asiento cerrado es
    /// exactamente lo que la inmutabilidad existe para impedir.
    /// </summary>
    public static void ExigirQueQuienRevierteNoSeaQuienRegistro(
        IdPersona autoriza, IdPersona autorDelOriginal)
    {
        if (autoriza != autorDelOriginal) return;

        throw new BloqueoDuro("BD-06",
            $"{autoriza} produjo el asiento que se quiere revertir y no puede autorizar su " +
            "reverso. Corregirse a sí mismo un asiento cerrado es exactamente lo que la " +
            "inmutabilidad existe para impedir.");
    }

    /// <summary>
    /// El contenido obligatorio de §8.3, comprobado entero.
    ///
    /// ── Por qué el valor anterior va aunque parezca redundante ──────────────
    /// Porque el reporte tiene que mostrar <b>los tres</b> valores —original, reverso y
    /// resultante— y sin el anterior sólo puede mostrar dos. §8.3: <i>«nunca solo el
    /// resultado»</i>.
    /// </summary>
    public static void ExigirContenidoCompleto(
        ReferenciaAlAsiento revertido,
        string valorAnterior,
        string motivoTipificado,
        string fundamento,
        string periodoAfectado,
        string periodoDeImputacion)
    {
        if (string.IsNullOrWhiteSpace(revertido.Identificador) ||
            string.IsNullOrWhiteSpace(revertido.Tipo))
            throw new BloqueoDuro("RN-93",
                "El reverso exige referencia explícita al asiento que revierte, con su tipo y " +
                "su identificador exacto. No existe el reverso genérico «de la misión»: sin " +
                "destinatario, nadie puede decir si ya se revirtió ni cuántas veces.");

        if (string.IsNullOrWhiteSpace(valorAnterior))
            throw new BloqueoDuro("RN-93",
                "El reverso exige el valor anterior, siempre. Todo reporte sobre el expediente " +
                "presenta el valor original, el reverso y el resultante — sin el anterior sólo " +
                "puede presentar dos, y el que falta es contra el que se juzga.");

        if (string.IsNullOrWhiteSpace(motivoTipificado))
            throw new BloqueoDuro("RN-93",
                "El reverso exige motivo tipificado. Un motivo libre no se puede agrupar, y un " +
                "reverso que no se puede agrupar no produce ningún indicador de control.");

        if (string.IsNullOrWhiteSpace(fundamento))
            throw new BloqueoDuro("RN-93",
                "El reverso exige fundamento documental. Sin él, revertir un asiento cerrado es " +
                "la palabra de quien revierte contra un registro que se dio por firme.");

        if (string.IsNullOrWhiteSpace(periodoAfectado) ||
            string.IsNullOrWhiteSpace(periodoDeImputacion))
            throw new BloqueoDuro("RN-93",
                "El reverso exige el período afectado y el de imputación. §8.3: el reverso " +
                "económico afecta los acumulados del período en que se registra, NO los del " +
                "original — y sin los dos, el histórico ya publicado deja de ser reproducible.");
    }

    /// <summary>
    /// §8.3 — <b>el reverso económico se imputa al período corriente</b>, no al original.
    ///
    /// ── Por qué no se corrige el histórico ──────────────────────────────────
    /// Porque <i>«los históricos ya publicados siguen siendo reproducibles»</i>. Reimputar al
    /// período original haría que un reporte de marzo diera un número distinto según cuándo se
    /// pidiera, y un reporte no reproducible no sirve para rendir cuentas (`RN-94`).
    /// </summary>
    public static void ExigirImputacionAlCorriente(
        string periodoAfectado, string periodoDeImputacion, decimal? efectoEconomico)
    {
        if (efectoEconomico is null) return;

        if (!string.Equals(periodoAfectado, periodoDeImputacion, StringComparison.OrdinalIgnoreCase))
            return;

        throw new BloqueoDuro("RN-93",
            $"El reverso económico se está imputando al mismo período que afecta " +
            $"(«{periodoAfectado}»). §8.3 lo prohíbe: el reverso afecta los acumulados del " +
            "período en que se registra, no los del original. Reimputarlo haría que un reporte " +
            "ya publicado diera un número distinto según cuándo se pida.");
    }

    /// <summary>
    /// §8.3, documentos impresos — <b>nunca el mismo folio con contenido distinto</b>.
    ///
    /// El corregido es un documento nuevo, con folio nuevo, que declara <i>«sustituye al folio
    /// X»</i>, y el folio X queda anulado con referencia cruzada. <b>Ambos se conservan y ambos
    /// se imprimen si se piden.</b>
    /// </summary>
    public static void ExigirSustitutoConFolioNuevo(
        NaturalezaDelReverso naturaleza, string folioAnulado, string? folioSustituto)
    {
        if (naturaleza is not NaturalezaDelReverso.AnulacionDeDocumento) return;

        if (folioSustituto is null) return;

        if (!string.Equals(folioAnulado.Trim(), folioSustituto.Trim(),
                StringComparison.OrdinalIgnoreCase))
            return;

        throw new BloqueoDuro("RN-93",
            $"El sustituto lleva el mismo folio «{folioAnulado}» que el anulado. Un documento " +
            "oficial nunca se reemite con el mismo folio y contenido distinto: el corregido es " +
            "un documento nuevo, con folio nuevo, que declara «sustituye al folio " +
            $"{folioAnulado}» — y ambos se conservan.");
    }
}
