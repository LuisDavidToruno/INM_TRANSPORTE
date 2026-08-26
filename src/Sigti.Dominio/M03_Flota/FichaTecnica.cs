namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Los atributos del vehículo contra los que se resuelve la matriz licencia↔vehículo.
///
/// `BD-02` es explícito en que la matriz <b>no se resuelve por número de ejes ni por
/// nombre del tipo de vehículo</b>, sino por estos atributos. El nombre del tipo se
/// conserva porque es el eje de compatibilidad de `BD-07`, que es otra pregunta.
/// </summary>
/// <param name="PesoBrutoKg">Peso bruto vehicular en kilogramos.</param>
/// <param name="LlevaRemolque">
/// Si la configuración va <b>enganchada a un remolque o acoplada a un semirremolque</b>.
///
/// Es el eje que separa `B` de `BE` y `C` de `CE` en el Artículo 4 del Acuerdo 1012-2021,
/// y <b>no es lo mismo que «articulado»</b>: un pick-up de 2,800 kg con una plataforma
/// enganchada requiere `BE` y no es articulado en ningún sentido. Confundirlos deja pasar
/// exactamente el caso que `BD-02` existe para impedir.
/// </param>
public sealed record FichaTecnica(
    string TipoDeVehiculo,
    int PesoBrutoKg,
    int CapacidadPasajeros,
    bool LlevaRemolque);

/// <summary>
/// La ventana de la misión. `BD-02` exige vigencia <b>durante todo el rango, incluida la
/// holgura posterior</b> — no basta que la licencia esté vigente el día de salida.
/// </summary>
public sealed record VentanaDeMision(DateOnly Salida, DateOnly Retorno, int HolguraDias)
{
    /// <summary>El último día en que el motorista podría estar conduciendo.</summary>
    public DateOnly FinDelRango => Retorno.AddDays(HolguraDias);
}
