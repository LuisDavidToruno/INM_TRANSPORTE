namespace Sigti.Datos;

/// <summary>
/// Una custodia de vehículo, tal como se guarda — la tarjeta de responsabilidad de `NRM-02`.
///
/// ── Por qué es una tabla y no una columna en el vehículo ─────────────────────
/// Porque `RN-22` exige el <b>historial completo</b>, consultable por rango de fechas: <i>«en
/// cualquier momento del pasado se puede decir quién respondía por la unidad»</i>. Una
/// columna `custodio_actual` contesta el presente y borra el pasado — y el pasado es
/// justamente lo que pregunta la auditoría cuando algo falta o algo se daña.
///
/// ── Esto es la custodia PERMANENTE ───────────────────────────────────────────
/// La temporal —el traslado al motorista al despachar, que se extingue al retorno— es un
/// registro distinto y <b>todavía no existe</b>. Guardarlas en la misma tabla haría imposible
/// contestar quién respondía por el bien mientras estaba en ruta: la respuesta correcta son
/// <b>las dos</b> personas, no una.
/// </summary>
public sealed class FilaDeCustodia
{
    public required Ulid Id { get; init; }

    public required Ulid VehiculoId { get; init; }

    /// <summary>
    /// Identidad de <b>persona</b>. La custodia es un rol adherido a un vehículo concreto,
    /// no a la estructura organizativa (`ACT-13`).
    /// </summary>
    public required string Custodio { get; init; }

    public required DateOnly Desde { get; init; }

    /// <summary>
    /// Nulo es <b>vigente</b>, no eterno. Un `Hasta` obligatorio obligaría a inventar una
    /// fecha de cese el día en que se firma la tarjeta de responsabilidad.
    /// </summary>
    public DateOnly? Hasta { get; init; }

    /// <summary>
    /// La referencia al acta de entrega-recepción que respalda la custodia.
    ///
    /// ⚠️ <b>Texto libre y sin verificar</b> hasta que exista `M-15`: hoy es el número o la
    /// descripción que teclea quien registra. Se guarda igual porque una custodia sin
    /// respaldo documental no se sostiene ante el Tribunal Superior de Cuentas, y porque el
    /// día que el acta sea un documento del sistema esta columna dice a cuál apuntaba.
    /// </summary>
    public string? Acta { get; init; }
}
