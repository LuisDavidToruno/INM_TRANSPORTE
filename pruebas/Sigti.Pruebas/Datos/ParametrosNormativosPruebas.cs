using Sigti.Datos.M02_Parametros;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// La bitemporalidad tiene que sobrevivir el viaje a la base. En memoria es fácil: lo
/// que importa es que los cuatro campos de fecha lleguen al esquema y vuelvan intactos,
/// porque `ADR-006` advierte que agregar el eje de vigencia después obliga a
/// <b>inventar una historia que no se tiene</b>.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class ParametrosNormativosPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly IdPersona Carlos = new("P-CARLOS");
    private static readonly IdPersona Gerencia = new("P-GERENCIA");

    private static readonly RespaldoDocumental Respaldo = new(
        Adjunto: Ulid.NewUlid(),
        Fuente: "Fuente de prueba",
        FechaDeVerificacion: new DateOnly(2026, 1, 1));

    [Fact]
    public async Task Una_correccion_retroactiva_sobrevive_el_viaje_a_la_base()
    {
        // El escenario de HU-148, ahora contra SQL Server real.
        var clave = $"peaje:zambrano:liviana:{Ulid.NewUlid()}";
        var correccion = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.FromHours(-6));

        await using (var escritura = baseDePruebas.Contexto())
        {
            var parametros = new ParametrosNormativos(escritura);

            await parametros.GuardarAsync(Version(clave, "22.00",
                registradoDesde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6)),
                registradoHasta: correccion));

            await parametros.GuardarAsync(Version(clave, "24.00",
                registradoDesde: correccion,
                registradoHasta: null));
        }

        await using var lectura = baseDePruebas.Contexto();
        var catalogo = await new ParametrosNormativos(lectura).CatalogoDeAsync(clave);

        var marzo = new DateOnly(2026, 3, 12);
        var cuandoSeLiquido = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-6));
        var hoy = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(-6));

        Assert.Equal("22.00", catalogo.Resolver(clave, marzo, cuandoSeLiquido).Valor);
        Assert.Equal("24.00", catalogo.Resolver(clave, marzo, hoy).Valor);
    }

    private static VersionDeParametro Version(
        string clave, string valor, DateTimeOffset registradoDesde, DateTimeOffset? registradoHasta) =>
        new(Clave: clave,
            Valor: valor,
            VigenteDesde: new DateOnly(2026, 1, 1),
            VigenteHasta: new DateOnly(2026, 6, 30),
            RegistradoDesde: registradoDesde,
            RegistradoHasta: registradoHasta,
            CargadoPor: Carlos,
            AprobadoPor: Gerencia)
        { Respaldo = Respaldo };
}
