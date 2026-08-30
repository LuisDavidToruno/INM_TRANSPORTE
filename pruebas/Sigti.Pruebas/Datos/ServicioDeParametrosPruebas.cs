using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Dominio.M02_Parametros;
using Sigti.Datos;
using Sigti.Datos.M16_Sincronizacion;
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
                new SolicitudDeCarga(clave, "25", new DateOnly(2026, 10, 1), null,
                    await RespaldoRealAsync(contexto), Carlos),
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
                new SolicitudDeCarga(clave, "25", new DateOnly(2026, 10, 1), null,
                    await RespaldoRealAsync(contexto), Carlos),
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

    /// <summary>
    /// Un respaldo cuyo documento <b>existe</b>.
    ///
    /// ⚠️ Antes devolvía un `Ulid.NewUlid()` suelto, y pasaba: nada comprobaba que el adjunto
    /// estuviera. Ese era el defecto —`RespaldoDocumental.Adjunto` apuntando a una fila que no
    /// existe— y estas pruebas lo reproducían sin notarlo. Ver
    /// <see cref="El_respaldo_que_no_existe_bloquea_la_aprobacion"/>.
    /// </summary>
    private static async Task<RespaldoDocumental> RespaldoRealAsync(SigtiDbContext contexto)
    {
        var id = Ulid.NewUlid();

        contexto.Adjuntos.Add(new FilaDeAdjunto
        {
            Id = id,

            // Nulo: un respaldo de parámetro NO cuelga de una transición de misión.
            IdTransicion = null,

            Ruta = $"parametros/{id}.pdf",
            Hash = "sha256:" + new string('0', 64),
            Tipo = "application/pdf",
            Bytes = 1024,
            Clasificacion = "ADMINISTRATIVO",
            CapturadoEnUtc = Momento.UtcDateTime,
            RecibidoEnUtc = Momento.UtcDateTime,
        });

        await contexto.SaveChangesAsync();

        return new RespaldoDocumental(
            Adjunto: id,
            Fuente: "Circular de la Gerencia Administrativa",
            FechaDeVerificacion: new DateOnly(2026, 9, 17));
    }

    /// <summary>
    /// `HU-145` — <b>el identificador del adjunto no es el adjunto</b>, y la aprobación lo
    /// comprueba contra la tabla.
    ///
    /// Se prueba acá y no en el dominio porque la regla es pura: recibe un booleano. Lo que
    /// puede fallar —y fallaba— es <b>quién responde ese booleano</b>. Con el respaldo declarado
    /// en el tipo como `Ulid` obligatorio, la columna nunca estaba vacía y nadie miraba si
    /// apuntaba a algo.
    /// </summary>
    [Fact]
    public async Task El_respaldo_que_no_existe_bloquea_la_aprobacion()
    {
        var clave = $"umbral:consumo:{Ulid.NewUlid()}";

        Ulid id;
        await using (var contexto = baseDePruebas.Contexto())
        {
            // Un ULID que no corresponde a ningún adjunto: exactamente lo que se cargaba antes.
            var inventado = new RespaldoDocumental(
                Adjunto: Ulid.NewUlid(),
                Fuente: "Circular que nadie adjuntó",
                FechaDeVerificacion: new DateOnly(2026, 9, 17));

            id = await new ServicioDeParametros(contexto).CargarAsync(
                new SolicitudDeCarga(clave, "25", new DateOnly(2026, 10, 1), null, inventado, Carlos),
                Momento);
        }

        await using (var contexto = baseDePruebas.Contexto())
        {
            // Gerencia es otra persona: el doble control de RN-39 está satisfecho, y aun así
            // no puede aprobar. Es la mitad del control que faltaba.
            var intento = await new ServicioDeParametros(contexto).AprobarAsync(id, Gerencia, Momento);

            Assert.False(intento.Concedida);
            Assert.Contains("respaldo documental", intento.MotivoDelRechazo);
        }

        await using var lectura = baseDePruebas.Contexto();
        var version = await lectura.Parametros.SingleAsync(p => p.Id == id);
        Assert.Null(version.AprobadoPor);

        // Y no se aplica en ningún cálculo mientras siga pendiente.
        var catalogo = await new ServicioDeParametros(lectura).CatalogoDeAsync(clave);
        Assert.Null(catalogo.ResolverSiHay(clave, new DateOnly(2026, 10, 5), Momento.AddDays(30)));
    }
}
