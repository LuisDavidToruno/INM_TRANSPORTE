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
    /// El folio del salvoconducto.
    ///
    /// ⚠️ <b>Provisional</b>, como el de la Orden de Misión: los rangos por delegación son de
    /// `M-01` y no están repartidos. El documento impreso con QR es de `M-15`, que tampoco
    /// existe — hoy esto es el número del papel que alguien tecleó.
    /// </summary>
    public required string Folio { get; init; }

    /// <summary>`ACT-09` Máxima Autoridad. Identidad de persona.</summary>
    public required string EmitidoPor { get; init; }

    public required Ulid Vehiculo { get; init; }
    public required Ulid Motorista { get; init; }

    /// <summary>
    /// ⚠️ La <b>ruta</b> que `RN-23` pide, representada por el destino declarado — es lo único
    /// que el expediente lleva hoy. Dos misiones a Choluteca por caminos distintos se ven
    /// iguales. `[C]` con Auditoría Interna.
    /// </summary>
    public required string Destino { get; init; }

    public required DateOnly Desde { get; init; }
    public required DateOnly Hasta { get; init; }
}
