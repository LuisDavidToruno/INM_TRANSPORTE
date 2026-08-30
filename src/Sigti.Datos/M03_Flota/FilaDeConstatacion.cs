using Sigti.Dominio.M03_Flota;

namespace Sigti.Datos.M03_Flota;

/// <summary>
/// Una constatación de un elemento de identificación — `RN-18`.
///
/// ── Por qué es una tabla y no un booleano ───────────────────────────────────
/// Lo era: <c>IdentificacionInstitucionalVerificada</c>. Un <c>true</c> no dice <b>cuándo</b>
/// se miró, ni <b>quién</b> lo miró, ni deja nada que mostrar — y `CLAUDE.md` lo pone entre las
/// restricciones que condicionan el diseño: <i>«es campo verificable con fecha y foto: es
/// hallazgo frecuente de auditoría»</i>.
///
/// Una constatación de hace tres años se veía igual que una de ayer.
///
/// ── Y por qué es una fila por elemento ──────────────────────────────────────
/// Un vehículo puede tener las franjas y no la leyenda. Con un solo dato para los cuatro,
/// «rotulación verificada» afirma de más sobre tres de ellos.
/// </summary>
public sealed class FilaDeConstatacion
{
    public required Ulid Id { get; init; }
    public required Ulid VehiculoId { get; init; }

    public required ElementoDeIdentificacion Elemento { get; init; }

    /// <summary>
    /// Si el elemento <b>está</b>.
    ///
    /// ⚠️ Falso no es lo mismo que no haber constatado: uno es un <b>hallazgo</b> —se miró y no
    /// está— y el otro una tarea pendiente. Que la fila exista es lo que los separa.
    /// </summary>
    public required bool Presente { get; init; }

    public required DateOnly ConstatadoEl { get; init; }

    /// <summary>
    /// ⚠️ <b>Obligatoria y no anulable.</b> `RN-18`: <i>«una constatación sin fotografía no
    /// debe aceptarse»</i>. Sin ella lo único que queda registrado es que alguien dijo que
    /// miró, y eso es exactamente lo que un hallazgo de auditoría discute.
    /// </summary>
    public required Ulid Fotografia { get; init; }

    public required string ConstatadoPor { get; init; }

    /// <summary>Qué se vio, cuando hace falta decirlo. Nulo es que no hubo nada que agregar.</summary>
    public string? Observacion { get; init; }

    public required DateTime RegistradoEnUtc { get; init; }
}
