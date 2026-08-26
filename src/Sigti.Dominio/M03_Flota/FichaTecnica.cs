namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Los atributos del vehículo contra los que se resuelve la matriz licencia↔vehículo.
///
/// `BD-02` es explícito en que la matriz <b>no se resuelve por número de ejes ni por
/// nombre del tipo de vehículo</b>, sino por estos atributos. El nombre del tipo se
/// conserva porque es el eje de compatibilidad de `BD-07`, que es otra pregunta.
/// </summary>
/// <param name="PesoBrutoKg">Peso bruto vehicular en kilogramos.</param>
/// <param name="EsArticulado">Determina por sí solo la exigencia de categoría CE.</param>
public sealed record FichaTecnica(
    string TipoDeVehiculo,
    int PesoBrutoKg,
    int CapacidadPasajeros,
    bool EsArticulado);

/// <summary>
/// La ventana de la misión. `BD-02` exige vigencia <b>durante todo el rango, incluida la
/// holgura posterior</b> — no basta que la licencia esté vigente el día de salida.
/// </summary>
public sealed record VentanaDeMision(DateOnly Salida, DateOnly Retorno, int HolguraDias)
{
    /// <summary>El último día en que el motorista podría estar conduciendo.</summary>
    public DateOnly FinDelRango => Retorno.AddDays(HolguraDias);
}
