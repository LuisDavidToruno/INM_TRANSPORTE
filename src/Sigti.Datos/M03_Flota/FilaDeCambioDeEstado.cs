using Sigti.Dominio.M03_Flota;

namespace Sigti.Datos;

/// <summary>
/// Un cambio de estado operativo del vehículo, tal como se guarda — §10.2.
///
/// ── Es un diario, y por las mismas razones que el de la misión ───────────────
/// Sólo se agrega. La pregunta de la auditoría no es <i>«¿en qué estado está?»</i> sino
/// <i>«¿por qué no estuvo disponible en abril, y quién lo decidió?»</i>, y una columna
/// `estado_actual` en el vehículo no la contesta — la borra cada vez que cambia.
/// </summary>
public sealed class FilaDeCambioDeEstado
{
    public required Ulid Id { get; init; }

    public required Ulid VehiculoId { get; init; }

    public required EstadoOperativo Estado { get; init; }

    /// <summary>
    /// Desde cuándo rige. <b>No es la fecha de captura</b>: un taller que empezó el lunes se
    /// registra el miércoles y rige desde el lunes (P-4).
    /// </summary>
    public required DateTimeOffset MomentoUtc { get; init; }

    /// <summary>
    /// Orden dentro del vehículo. <b>El orden del diario es parte del dato, no del azar de la
    /// consulta</b> — y hace falta acá más que en la misión, porque dos cambios pueden
    /// compartir marca de tiempo cuando uno lo fija el sistema y otro una persona.
    /// </summary>
    public required int Orden { get; init; }

    public required string Ejecuta { get; init; }

    /// <summary>
    /// Causa tipificada, referencia de acta, o la explicación libre. <b>Obligatorio para todo
    /// lo que no fije el sistema</b>: §10.2 exige causa tipificada para `NO_DISPONIBLE` y acta
    /// para el préstamo y los dos terminales.
    /// </summary>
    public string? Motivo { get; init; }

    /// <summary>
    /// Si lo fijó el sistema por una transición de la Orden de Misión.
    ///
    /// Se guarda porque es <b>verificable</b>: §10.2 dice que `ASIGNADO` y `EN_MISION` los fija
    /// el sistema y no una persona, y sin esta marca esa afirmación no se puede auditar
    /// después — sólo confiar.
    /// </summary>
    public required bool Automatico { get; init; }
}
