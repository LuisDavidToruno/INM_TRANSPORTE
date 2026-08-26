using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// El circuito de `HU-144` y `HU-146` completo: cargar, intentar aprobar, aprobar.
///
/// Lo que se prueba acá y no en el dominio es <b>que el intento rechazado sobreviva</b>.
/// Bloquear sin dejar rastro deja al auditor sin saber que alguien intentó aprobar su
/// propia carga, y ese intento es justamente lo que un control interno quiere ver.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class ServicioDeParametrosPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly IdPersona Carlos = new("P-CARLOS");
    private static readonly IdPersona Gerencia = new("P-GERENCIA");

    private static readonly DateTimeOffset Momento =
        new(2026, 9, 18, 11, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public async Task El_intento_de_aprobar_la_propia_carga_se_rechaza_y_queda_en_la_bitacora()
    {
        var clave = $"umbral:consumo:{Ulid.NewUlid()}";

        Ulid id;
        await using (var contexto = baseDePruebas.Contexto())
        {
            id = await new ServicioDeParametros(contexto).CargarAsync(
                new SolicitudDeCarga(clave, "25", new DateOnly(2026, 10, 1), null, Respaldo(), Carlos),
                Momento);
        }

        // Carlos, que cargó, intenta aprobar.
        await using (var contexto = baseDePruebas.Contexto())
        {
            var intento = await new ServicioDeParametros(contexto).AprobarAsync(id, Carlos, Momento);
            Assert.False(intento.Concedida);
        }

        await using var lectura = baseDePruebas.Contexto();

        // La versión sigue sin aprobar.
        var version = await lectura.Parametros.SingleAsync(p => p.Id == id);
        Assert.Null(version.AprobadoPor);

        // Y el intento dejó su asiento, aunque no se haya concedido.
        var asientos = await lectura.Asientos
            .Where(a => a.Cola == $"parametro:{clave}")
            .OrderBy(a => a.Secuencia)
            .ToListAsync();

        Assert.Equal(2, asientos.Count);
        Assert.Contains("CARGA", asientos[0].Contenido);
        Assert.Contains("RECHAZADA", asientos[1].Contenido);
        Assert.Contains(Carlos.Valor, asientos[1].Contenido);
    }

    [Fact]
    public async Task Otra_persona_aprueba_y_el_parametro_pasa_a_resolver()
    {
        var clave = $"umbral:consumo:{Ulid.NewUlid()}";

        Ulid id;
        await using (var contexto = baseDePruebas.Contexto())
        {
            id = await new ServicioDeParametros(contexto).CargarAsync(
                new SolicitudDeCarga(clave, "25", new DateOnly(2026, 10, 1), null, Respaldo(), Carlos),
                Momento);
        }

        await using (var contexto = baseDePruebas.Contexto())
        {
            var intento = await new ServicioDeParametros(contexto).AprobarAsync(id, Gerencia, Momento);
            Assert.True(intento.Concedida);
        }

        await using var lectura = baseDePruebas.Contexto();
        var catalogo = await new ServicioDeParametros(lectura).CatalogoDeAsync(clave);

        var resuelto = catalogo.Resolver(clave, new DateOnly(2026, 10, 5), Momento.AddDays(30));
        Assert.Equal("25", resuelto.Valor);
    }

    private static RespaldoDocumental Respaldo() => new(
        Adjunto: Ulid.NewUlid(),
        Fuente: "Circular de la Gerencia Administrativa",
        FechaDeVerificacion: new DateOnly(2026, 9, 17));
}
