namespace Sigti.Datos.M04_Documentacion;

/// <summary>
/// La confirmación de que un vehículo quedó resguardado durante un feriado largo — `HU-020`.
///
/// ── Lo que esto contesta ────────────────────────────────────────────────────
/// <i>«¿Dónde está cada vehículo que no salió?»</i> El reporte previo tiene dos mitades y ambas
/// importan: los que circulan con permiso, y los que <b>deben estar resguardados con
/// confirmación</b>. Un vehículo del que nadie confirmó dónde está es exactamente lo que un
/// operativo del TSC encuentra en Semana Santa.
/// </summary>
public sealed class FilaDeResguardo
{
    public required Ulid Id { get; init; }
    public required Ulid VehiculoId { get; init; }

    /// <summary>El período cubierto. Un resguardo confirmado no vale para el feriado siguiente.</summary>
    public required DateOnly Desde { get; init; }

    /// <inheritdoc cref="Desde"/>
    public required DateOnly Hasta { get; init; }

    /// <summary>
    /// Dónde. <b>Obligatorio</b>: «confirmado» sin lugar no contesta la pregunta que el reporte
    /// hace.
    /// </summary>
    public required string Predio { get; init; }

    /// <summary>
    /// ⚠️ <b>Obligatoria y no anulable.</b> Misma disciplina que `RN-18`: sin evidencia lo único
    /// que queda registrado es que alguien dijo que el vehículo estaba ahí, y eso es lo que un
    /// operativo viene a discutir.
    /// </summary>
    public required Ulid Evidencia { get; init; }

    /// <summary>
    /// La fecha del hecho, no la de captura (`P-4`): cuándo alguien fue a mirar. Una foto de
    /// hace tres semanas confirma menos que una de ayer, y sin la fecha las dos se ven iguales.
    /// </summary>
    public required DateOnly ConfirmadoEl { get; init; }

    public required string ConfirmadoPor { get; init; }
    public required DateTime RegistradoEnUtc { get; init; }
}
