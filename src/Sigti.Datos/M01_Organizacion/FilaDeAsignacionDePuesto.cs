namespace Sigti.Datos;

/// <summary>
/// Una asignación de puesto — <b>espejo, no maestro</b>.
///
/// ── Lo que esta clase NO es ──────────────────────────────────────────────────
/// No es el organigrama de la institución. `DP-001`: la estructura de puestos es
/// <b>propiedad de ARGOS y Talento Humano</b>, y `RN-48` es taxativo — los datos de otro
/// dueño se almacenan como espejo marcado como tal, y <b>ninguna pantalla ni operación de
/// SIGTI debe permitir editarlos</b>.
///
/// Por eso <b>no hay endpoint de escritura</b>. Se puebla por la integración, y si alguien
/// necesita corregir un puesto, lo corrige en ARGOS.
///
/// ── Por qué lleva `ConfirmadoAl`, que un maestro no necesitaría ──────────────
/// <b>Un espejo envejece.</b> Y la antigüedad no es un detalle técnico: es lo que `HU-009`
/// muestra en la bandeja de autorización, porque una jefatura que va a firmar sobre un
/// organigrama de hace nueve días tiene derecho a saberlo <b>antes</b> de firmar.
/// </summary>
public sealed class FilaDeAsignacionDePuesto
{
    public required Ulid Id { get; init; }

    /// <summary>Identidad de <b>persona</b>, la que `BD-01` compara. No de usuario.</summary>
    public required string Persona { get; init; }

    public required string Puesto { get; init; }

    public required DateOnly Desde { get; init; }

    /// <summary>
    /// Nulo es <b>indefinido, no eterno</b>. Un `Hasta` obligatorio obligaría a inventar
    /// una fecha de salida el día del nombramiento.
    /// </summary>
    public DateOnly? Hasta { get; init; }

    /// <summary>
    /// Cuándo se confirmó esta fila contra el sistema dueño.
    ///
    /// <b>No es «cuándo se creó».</b> Una integración que corre todas las noches confirma
    /// filas que no cambiaron, y eso es justamente lo que hay que saber: que el dato sigue
    /// siendo cierto. Sin este campo, un espejo detenido hace dos semanas se ve idéntico a
    /// uno recién sincronizado.
    /// </summary>
    public required DateTime ConfirmadoAlUtc { get; init; }
}
