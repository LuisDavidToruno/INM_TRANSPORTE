namespace Sigti.Datos;

/// <summary>
/// Quién ocupa qué puesto, y desde cuándo.
///
/// ── ⚠️ Dos orígenes que no se mezclan — ver <see cref="OrigenDeLaAsignacion"/> ──
/// Esta tabla lleva dos clases de fila y confundirlas rompe las dos:
///
/// <list type="bullet">
/// <item><b>Espejadas.</b> El cargo del contrato, que es de ARGOS y Talento Humano. `RN-48`
/// es taxativo: los datos de otro dueño se almacenan marcados como espejo y <b>ninguna
/// pantalla de SIGTI debe permitir editarlos</b>. Quien necesite corregir un cargo lo corrige
/// allá.</item>
/// <item><b>Propias.</b> El puesto <i>funcional en la gestión de flota</i> —Jefe de Transporte,
/// Encargado de Despacho—, que <b>ARGOS no modela</b> y por lo tanto no es dato de otro dueño.
/// SIGTI es su dueño y las administra.</item>
/// </list>
///
/// Que la competencia siga viviendo en el <b>puesto</b> y no en la persona es lo que `RNF-14`
/// exige —<i>«permisos asignados directamente a una persona: 0»</i>— y lo que hace que la
/// rotación sea cerrar una fila: el siguiente ocupante hereda las competencias sin tocarlas.
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

    /// <summary>
    /// De dónde viene esta fila. <b>Decide quién la puede tocar</b>, y sobre todo decide qué
    /// hace la sincronización con ella.
    /// </summary>
    public required OrigenDeLaAsignacion Origen { get; init; }

    public required string Puesto { get; init; }

    public required DateOnly Desde { get; init; }

    /// <summary>
    /// Nulo es <b>indefinido, no eterno</b>. Un `Hasta` obligatorio obligaría a inventar
    /// una fecha de salida el día del nombramiento.
    /// </summary>
    /// Es <b>mutable</b> porque una asignación se cierra: la persona deja el puesto y la fila
    /// no se borra. `RN-100` juzga cada acto contra la ocupación <b>a la fecha del hecho</b>, y
    /// borrar la del que se fue haría que un expediente de febrero pareciera autorizado por
    /// alguien sin competencia — indefendible ante el auditor, y por un artefacto del sistema.
    public DateOnly? Hasta { get; set; }

    /// <summary>
    /// Cuándo se confirmó esta fila contra el sistema dueño.
    ///
    /// <b>No es «cuándo se creó».</b> Una integración que corre todas las noches confirma
    /// filas que no cambiaron, y eso es justamente lo que hay que saber: que el dato sigue
    /// siendo cierto. Sin este campo, un espejo detenido hace dos semanas se ve idéntico a
    /// uno recién sincronizado.
    /// </summary>
    public required DateTime ConfirmadoAlUtc { get; set; }
}

/// <summary>
/// Quién es dueño de una asignación de puesto.
///
/// ── ⚠️ Por qué hace falta distinguirlas ─────────────────────────────────────
/// La sincronización con el sistema dueño del padrón <b>cierra las filas que la fuente ya no
/// trae</b> — así es como una persona que dejó la institución deja de tener competencias sin
/// que nadie se acuerde de quitárselas.
///
/// Sin esta marca, esa misma lógica <b>cerraría también los puestos funcionales de SIGTI</b>,
/// que ARGOS no conoce y nunca va a traer. El Jefe de Transporte perdería su rol en la primera
/// sincronización nocturna, y el síntoma —«ayer podía y hoy no»— no señalaría a la causa.
/// </summary>
public enum OrigenDeLaAsignacion
{
    /// <summary>
    /// El cargo del contrato, copiado del sistema dueño. <b>SIGTI no lo edita</b> (`RN-48`):
    /// la sincronización lo refresca, y lo que la fuente deja de traer se cierra con fecha.
    /// </summary>
    Espejo,

    /// <summary>
    /// El puesto funcional en la gestión de flota, otorgado dentro de SIGTI.
    ///
    /// <b>No es dato de otro dueño</b>: ARGOS no modela «Encargado de Despacho», porque no
    /// gestiona flota. La sincronización no las toca.
    /// </summary>
    Propia,
}
