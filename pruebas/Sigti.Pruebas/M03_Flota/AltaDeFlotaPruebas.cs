using Sigti.Aplicacion.M03_Flota;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// El alta de un vehículo, contra la base real — <b>`M-03`</b>.
///
/// ── Por qué esto existe ─────────────────────────────────────────────────────
/// Hasta ahora la flota <b>sólo entraba por siembra</b>: había cuatro vehículos y ningún
/// endpoint capaz de crear el quinto. Una institución no puede cargar su flota real, y sin
/// flota real no hay piloto.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class AltaDeFlotaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateOnly Hoy = new(2026, 9, 1);

    /// <summary>
    /// <b>Las siglas son la identidad estable del bien</b> y hay un índice único encima. Sin
    /// esta comprobación el segundo alta revienta con una violación de índice: un `500` que no
    /// le dice a nadie qué hacer, cuando la respuesta correcta es «esas siglas ya son de otro».
    /// </summary>
    [Fact]
    public async Task Unas_siglas_repetidas_se_rechazan_diciendo_de_quien_son()
    {
        await using var contexto = baseDePruebas.Contexto();
        var quien = await PuestoSembrado.ConRolAsync(contexto, Rol.JefeDeTransporte, Hoy.AddYears(-1));
        var servicio = new ServicioDeAltaDeFlota(contexto);

        var siglas = $"INM-{Ulid.NewUlid().ToString()[^8..]}";

        var primera = await servicio.RegistrarAsync(Alta(siglas), Hoy, quien);
        Assert.True(primera.Procede);

        var segunda = await servicio.RegistrarAsync(Alta(siglas), Hoy, quien);

        Assert.False(segunda.Procede);
        Assert.Contains(siglas, segunda.Mensaje);
    }

    private static AltaDeVehiculo Alta(string siglas) => new()
    {
        Siglas = siglas,
        TipoDeVehiculo = "Pick-up doble cabina",
        Clase = ClaseNormativa.Automovil,
        PesoBrutoKg = 2800,
        CapacidadPasajeros = 5,
        LlevaRemolque = false,
        VenceMatricula = Hoy.AddYears(1),
        Placa = "PAA-1234",
        EstadoDePlaca = EstadoDePlaca.ConLamina,
        NumeroDeEjes = 2,
    };
}
