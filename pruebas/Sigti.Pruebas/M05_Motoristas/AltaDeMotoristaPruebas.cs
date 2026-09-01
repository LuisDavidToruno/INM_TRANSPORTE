using Sigti.Aplicacion.M05_Motoristas;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.M05_Motoristas;

/// <summary>
/// El alta de un motorista en el padrón, contra la base real — <b>`M-05`</b>.
///
/// Igual que la flota, el padrón <b>sólo entraba por siembra</b>: cuatro motoristas y ninguna
/// forma de registrar al quinto.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class AltaDeMotoristaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateOnly Hoy = new(2026, 9, 1);

    /// <summary>
    /// <b>Una licencia no puede estar en dos personas.</b> Hay índice único encima, y sin esta
    /// comprobación el segundo alta sale como violación de restricción en vez de decir que ese
    /// número ya está registrado — que es lo único que le sirve a quien lo está capturando.
    /// </summary>
    [Fact]
    public async Task Una_licencia_repetida_se_rechaza_diciendo_el_numero()
    {
        await using var contexto = baseDePruebas.Contexto();
        var quien = await PuestoSembrado.ConRolAsync(contexto, Rol.JefeDeTransporte, Hoy.AddYears(-1));
        var servicio = new ServicioDeAltaDeMotoristas(contexto);

        var licencia = $"0801-{Ulid.NewUlid().ToString()[^10..]}";

        var primera = await servicio.RegistrarAsync(Alta("Nery Alvarado", licencia), Hoy, quien);
        Assert.True(primera.Procede);

        var segunda = await servicio.RegistrarAsync(Alta("Otro Distinto", licencia), Hoy, quien);

        Assert.False(segunda.Procede);
        Assert.Contains(licencia, segunda.Mensaje);
    }

    private static AltaDeMotorista Alta(string nombre, string licencia) => new()
    {
        Nombre = nombre,
        EsDelPadron = true,
        NumeroDeLicencia = licencia,
        Categoria = CategoriaDeLicencia.C,
        VenceLicencia = Hoy.AddYears(2),
    };
}
