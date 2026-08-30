namespace Sigti.Datos;

/// <summary>
/// Un permiso de circulación en día inhábil, tal como se guarda — `RN-23`.
///
/// ── Guarda las cuatro cosas que ampara ───────────────────────────────────────
/// Vehículo, motorista, destino y ventana. No es redundancia con el expediente: <b>el
/// permiso es un documento con vida propia</b>, firmado por la máxima autoridad, y lo que
/// ampara quedó fijado cuando se firmó. Si mañana la misión cambia de motorista, el permiso
/// no cambia — deja de amparar, que es lo que `RN-23` prescribe.
///
/// ── Por qué NO apunta al expediente ──────────────────────────────────────────
/// Sí apunta, y también guarda los cuatro. El expediente es el contexto; los cuatro campos
/// son <b>lo que dice el papel</b>. Derivarlos del expediente al comparar haría que el permiso
/// amparara siempre lo que la misión tenga hoy, y entonces un relevo nunca lo invalidaría.
/// </summary>
public sealed class FilaDePermisoDeCirculacion
{
    public required Ulid Id { get; init; }

    /// <summary>Para qué expediente se emitió. El contexto, no lo amparado.</summary>
    public required Ulid ExpedienteId { get; init; }

    /// <summary>
    /// Qué permiso viene a reemplazar. <b>Nulo en el primero</b>, que es el caso normal.
    ///
    /// ── Por qué la referencia va en los dos sentidos ────────────────────────
    /// Sin ella hay dos permisos sueltos para una misma misión y nada dice cuál superó a cuál:
    /// un auditor ve dos folios, dos firmas y dos salvoconductos, y tiene que reconstruir el
    /// orden por las fechas. `RN-04` pide <b>referencia cruzada</b> — el anulado dice por qué
    /// se anuló y el nuevo dice a quién reemplaza.
    /// </summary>
    public Ulid? Reemplaza { get; init; }

    /// <summary>
    /// El folio del salvoconducto.
    ///
    /// ⚠️ <b>Provisional</b>, como el de la Orden de Misión: los rangos por delegación son de
    /// `M-01` y no están repartidos. El documento impreso con QR es de `M-15`, que tampoco
    /// existe — hoy esto es el número del papel que alguien tecleó.
    /// </summary>
    public required string Folio { get; init; }

    /// <summary>
    /// `SOLICITADO`, `FIRMADO` o `DESISTIDO`. <b>La diferencia es todo `BD-04`</b>: un
    /// trámite abierto no ampara nada, y si los dos primeros se trataran igual, cualquiera
    /// destrabaría el despacho de un domingo abriendo un trámite y sin esperar la firma.
    /// </summary>
    public required string Estado { get; set; }

    /// <summary>
    /// `ACT-09` Máxima Autoridad. Identidad de persona.
    ///
    /// <b>Nulo mientras no se firme</b>, y ésa es la razón de que el permiso no ampare: no es
    /// un dato que falte cargar.
    /// </summary>
    public string? EmitidoPor { get; set; }

    /// <summary>
    /// <b>Nulos mientras la misión no esté programada.</b> `RN-23` dice dos cosas que sólo se
    /// cumplen a la vez si se separa abrir de firmar: el permiso no exige que la misión esté
    /// programada, y el permiso es nominativo. Se abre sin ellos; <b>no se firma sin ellos</b>.
    /// </summary>
    public Ulid? Vehiculo { get; set; }

    /// <inheritdoc cref="Vehiculo"/>
    public Ulid? Motorista { get; set; }

    /// <summary>Quién encaminó el trámite — `ACT-04` o `ACT-10`.</summary>
    public required string Solicita { get; init; }

    public required DateTime SolicitadoEnUtc { get; init; }

    /// <summary>
    /// Por qué la misión tiene que circular en franja inhábil.
    ///
    /// <b>Es lo único que la máxima autoridad tiene para decidir.</b> Sin esto la pantalla de
    /// firma muestra un vehículo, un destino y unas fechas, y firmar se vuelve un trámite: se
    /// aprueba lo que aparece, porque no hay nada que juzgar.
    /// </summary>
    public required string Justificacion { get; init; }

    /// <summary>
    /// Los días y franjas inhábiles que el permiso viene a cubrir, separados por ` · `.
    ///
    /// Van en el documento porque <b>el agente en carretera lee el papel</b>: un permiso que
    /// dice «ampara del 1 al 5» sin decir qué días de esos eran inhábiles no le deja verificar
    /// nada. Se congelan al abrir el trámite —con el calendario vigente a la fecha del hecho,
    /// `RN-40`— y no se recalculan al firmar.
    /// </summary>
    public required string TramosInhabiles { get; init; }

    /// <summary>Nulo mientras no se firme.</summary>
    public DateTime? FirmadoEnUtc { get; set; }

    /// <summary>
    /// Por qué se desistió. Nulo salvo en `DESISTIDO`.
    ///
    /// <b>El trámite no se borra</b>: que alguien haya pedido circular un domingo es un hecho, y
    /// que se haya desistido también. Uno desaparecido y uno que nunca existió se ven iguales.
    /// </summary>
    public string? MotivoDelDesistimiento { get; set; }

    /// <summary>
    /// ⚠️ La <b>ruta</b> que `RN-23` pide, representada por el destino declarado — es lo único
    /// que el expediente lleva hoy. Dos misiones a Choluteca por caminos distintos se ven
    /// iguales. `[C]` con Auditoría Interna.
    /// </summary>
    public required string Destino { get; init; }

    public required DateOnly Desde { get; init; }
    public required DateOnly Hasta { get; init; }
}
