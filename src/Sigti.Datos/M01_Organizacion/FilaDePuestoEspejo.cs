namespace Sigti.Datos;

/// <summary>
/// Un puesto de la estructura — <b>espejo, no maestro</b>.
///
/// ── Por qué se agrega, si ya estaba <see cref="FilaDeAsignacionDePuesto"/> ──
/// Esa dice <i>quién ocupa qué</i>. Ésta dice <b>qué es cada puesto y de quién depende</b>, y sin
/// eso el escalamiento de §5.3.B.3 no puede dar su primer salto: todo bloqueo terminaba en
/// Gerencia Administrativa, que es el <b>último</b> recurso y no el primero.
///
/// ── Mismas reglas que el otro espejo ────────────────────────────────────────
/// `RN-48` y `DP-001`: la estructura es propiedad de ARGOS y Talento Humano, <b>sin endpoint de
/// escritura</b>. Se puebla por la integración; corregir un puesto se hace en ARGOS.
/// </summary>
public sealed class FilaDePuestoEspejo
{
    public required Ulid Id { get; init; }

    /// <summary>El código del puesto — el mismo que usa la asignación.</summary>
    public required string Puesto { get; init; }

    /// <summary>Cómo se llama. Es lo que el mensaje del escalamiento muestra.</summary>
    public required string Denominacion { get; init; }

    /// <summary>
    /// La unidad organizativa. <b>§5.3.B.3 exige que el superior sea de la misma</b>, y sin este
    /// campo no se puede distinguir el primer salto del segundo.
    /// </summary>
    public required string Unidad { get; init; }

    /// <summary>
    /// De quién depende. <b>Nulo es «la cima de su rama»</b>, no «falta el dato»: un puesto sin
    /// superior existe —la máxima autoridad no depende de nadie— y tratarlo como dato faltante
    /// haría que el escalamiento buscara para siempre.
    /// </summary>
    public string? Superior { get; init; }

    /// <summary>
    /// La delegación territorial. <b>Nulo es sede</b>: el corte territorial y el jerárquico
    /// coexisten, y un puesto de sede no está en ninguna delegación.
    /// </summary>
    public string? Delegacion { get; init; }

    /// <summary>Cuándo se confirmó contra el sistema dueño. Un espejo envejece.</summary>
    public required DateTime ConfirmadoAlUtc { get; init; }
}

/// <summary>
/// A qué puesto de sede escala una delegación — <b>maestro de SIGTI</b>.
///
/// ── Por qué esto NO es de ARGOS, y el espejo sí ─────────────────────────────
/// ARGOS conoce la estructura; <b>no conoce nuestra política de control interno</b>. Que la
/// Delegación de Choluteca escale a tal puesto de sede cuando su encargado queda bloqueado por
/// segregación es una decisión de SIGTI, no un dato del organigrama.
/// </summary>
public sealed class FilaDeRespaldoDeSede
{
    public required Ulid Id { get; init; }

    public required string Delegacion { get; init; }

    /// <summary>El puesto de sede designado como respaldo.</summary>
    public required string Puesto { get; init; }

    /// <summary>Quién lo designó. Designar un respaldo es un acto y tiene autor.</summary>
    public required string Designa { get; init; }

    public required DateOnly Desde { get; init; }
}
