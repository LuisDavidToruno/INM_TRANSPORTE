namespace Sigti.Datos;

/// <summary>
/// Un intento de acto bloqueado por segregación — <b>§5.3.B.2</b>.
///
/// ── Por qué se guarda lo que NO pasó ────────────────────────────────────────
/// <i>«El intento bloqueado es información de control, no ruido. Un mismo usuario intentando
/// quince veces autorizar sus propias solicitudes es exactamente lo que Auditoría Interna
/// quiere ver.»</i>
///
/// Un sistema que sólo guarda lo que se consumó no puede contestar la pregunta que el TSC hace:
/// <b>si el control operó</b>. Sin esta tabla, un bloqueo perfecto y un bloqueo que nunca se
/// activó se ven exactamente igual — no hay rastro de ninguno de los dos.
///
/// ── No se borra y no se edita ───────────────────────────────────────────────
/// Es pista de auditoría. Vale lo mismo que para el diario: nada se borra físicamente, y esta
/// tabla no tiene endpoint de escritura fuera del propio bloqueo.
/// </summary>
public sealed class FilaDeIntentoBloqueado
{
    public required Ulid Id { get; init; }

    /// <summary>Identidad de <b>persona</b>. Dos cuentas del mismo servidor son la misma.</summary>
    public required string Quien { get; init; }

    /// <summary>Qué pretendía hacer.</summary>
    public required string Pretendia { get; init; }

    /// <summary>Sobre qué expediente.</summary>
    public required string Expediente { get; init; }

    /// <summary>El par de §5.2 que se activó — `I-01` a `I-19`.</summary>
    public required string Par { get; init; }

    /// <summary>Contra qué acto propio chocó.</summary>
    public required string ChocaCon { get; init; }

    /// <summary>
    /// Cómo se nombró ese acto previo.
    ///
    /// Se guarda el texto y no una referencia: el mensaje que se le mostró a quien intentó
    /// <b>es parte del asiento</b>, y reconstruirlo después contra datos que pudieron cambiar
    /// diría algo distinto de lo que la persona leyó.
    /// </summary>
    public required string Referencia { get; init; }

    public required DateTime MomentoUtc { get; init; }

    /// <summary>
    /// De dónde vino el intento — §5.3.B.2 lo enumera entre los siete datos.
    ///
    /// <b>Nulo es «no se supo»</b>, no «desde el servidor». Un origen inventado en la pista de
    /// auditoría es peor que un origen ausente.
    /// </summary>
    public string? Origen { get; init; }

    /// <summary>
    /// Por cual de los tres saltos de §5.3.B.3 se resolvio el destino.
    ///
    /// <b>Nulo es «no se resolvio»</b> —el intento es anterior al escalamiento— y no «fue a
    /// Gerencia». Los intentos viejos existen y decir que fueron al ultimo recurso seria
    /// inventar un encaminamiento que nunca ocurrio.
    /// </summary>
    public string? Salto { get; init; }

    /// <summary>
    /// El puesto donde quedó pendiente. <b>Nulo cuando el destino es Gerencia Administrativa</b>,
    /// que es el último recurso y no un puesto de la jerarquía de quien intentó.
    /// </summary>
    public string? EscalaA { get; init; }

    /// <summary>
    /// Que fallo en los saltos previos.
    ///
    /// <b>No es decoracion.</b> Un escalamiento que siempre termina en Gerencia Administrativa
    /// sin decir por que se lee como que la jerarquia no sirve, cuando lo que puede estar
    /// pasando es que el puesto superior este vacante — un problema de organizacion que
    /// alguien tiene que resolver, y que solo se ve si se dice.
    /// </summary>
    public string? PorQueNoAntes { get; init; }
}
