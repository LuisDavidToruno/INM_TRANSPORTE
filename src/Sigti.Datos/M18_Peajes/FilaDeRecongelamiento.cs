namespace Sigti.Datos.M18_Peajes;

/// <summary>
/// El <b>asiento de diferencia</b> de `RN-61`: qué cambió el estimado de peajes al sustituir el
/// vehículo, contra qué congelamiento, y cuánto.
///
/// ── Por qué es un asiento y no una actualización ────────────────────────────
/// El estimado congelado <b>es lo que el autorizador autorizó</b>. Sustituir un pick-up por un
/// camión de dos ejes puede duplicar el peaje de una ruta larga, y esa diferencia es
/// precisamente lo que un control interno quiere ver: no el número final, sino <b>que hubo un
/// cambio, de cuánto, por qué y quién lo hizo</b>.
///
/// Actualizar los subtotales en su lugar dejaría un expediente donde el monto autorizado y el
/// monto vigente coinciden siempre — que es lo mismo que no tener control.
/// </summary>
public sealed class FilaDeRecongelamiento
{
    public required Ulid Id { get; init; }
    public required Ulid MisionId { get; init; }

    /// <summary>
    /// Qué vehículo salió y cuál entró.
    ///
    /// <b>El saliente puede ser nulo</b> cuando el congelamiento original se hizo sin vehículo
    /// resuelto —se estimó por tipo—, y eso no es un dato que falte: es la razón de que el
    /// estimado anterior fuera genérico.
    /// </summary>
    public Ulid? VehiculoSaliente { get; init; }

    /// <inheritdoc cref="VehiculoSaliente"/>
    public required Ulid VehiculoEntrante { get; init; }

    /// <summary>La categoría de peaje con la que se valoró cada uno. `RN-10`: por ejes.</summary>
    public string? CategoriaAnterior { get; init; }

    /// <inheritdoc cref="CategoriaAnterior"/>
    public string? CategoriaNueva { get; init; }

    /// <summary>
    /// Los dos totales y su diferencia.
    ///
    /// ⚠️ <b>Nulos cuando alguna línea no se pudo valorar</b> —sin tarifa cargada, sin categoría
    /// resuelta—. Nunca cero: un cero diría que la ruta no cuesta, y eso es una afirmación
    /// distinta de «no se pudo calcular».
    /// </summary>
    public decimal? TotalAnterior { get; init; }

    /// <inheritdoc cref="TotalAnterior"/>
    public decimal? TotalNuevo { get; init; }

    /// <summary>
    /// El motivo tipificado de la reasignación que lo disparó, más lo que se escribió.
    ///
    /// Sin esto el asiento dice que el monto cambió y no por qué, y `RN-61` existe justamente
    /// para que la diferencia sea explicable.
    /// </summary>
    public required string Motivo { get; init; }

    public required string Recongela { get; init; }
    public required DateTime MomentoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }
}
