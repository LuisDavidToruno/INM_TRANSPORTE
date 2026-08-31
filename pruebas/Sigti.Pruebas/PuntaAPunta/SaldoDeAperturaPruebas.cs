using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// `RN-97` cableada — el saldo de apertura de control interno.
///
/// ── La regla que impide el abandono ──────────────────────────────────────────
/// <i>«Sin saldo de apertura, el mecanismo de olvido es automático y no requiere mala fe: llega
/// enero, el sistema arranca con reportes en cero, y una misión interrumpida en noviembre, un
/// préstamo vencido en agosto y una obligación de reintegro de mayo simplemente dejan de
/// aparecer en ninguna pantalla. <b>Nadie decidió abandonarlos: se abandonaron solos</b>»</i>.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class SaldoDeAperturaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Ahora =
        new(2027, 1, 5, 9, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task El_inventario_declara_TODAS_las_fuentes_incluidas_las_que_no_puede_contar()
    {
        // Un saldo que omite en silencio los préstamos vencidos es el abandono que la regla
        // existe para impedir, con formato de reporte.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var inv = await Leer(cliente, "/saldo-de-apertura/inventario/2026-12-31");
        var fuentes = inv.GetProperty("fuentes").EnumerateArray().ToList();

        // Las diez que `RN-97` enumera, ni una menos.
        Assert.Equal(10, fuentes.Count);
        Assert.False(inv.GetProperty("completo").GetBoolean());

        // ── Las dos con poder de bloqueo YA se consultan ─────────────────────
        // Estuvieron declaradas y vacías durante varios turnos, con el bloqueo del cierre
        // escrito y sin poder disparar. `RN-63` trajo los préstamos vencidos y M-12 las
        // interrupciones sin desenlace. **Esta prueba antes verificaba lo contrario**, y
        // cambiarla es la forma de dejar constancia de que el hueco se cerró.
        foreach (var tipo in new[] { "PrestamoVencido", "InterrupcionSinDesenlace" })
            Assert.True(
                fuentes.Single(f => f.GetProperty("tipo").GetString() == tipo)
                    .GetProperty("sePudoConsultar").GetBoolean(),
                $"La fuente {tipo} tiene poder de bloqueo del cierre y debe ser consultable.");

        // ── Y las que siguen sin poder contarse van declaradas, no omitidas ──
        // Un saldo que las omite en silencio es el abandono que la regla existe para impedir,
        // con formato de reporte.
        var peaje = fuentes.Single(f => f.GetProperty("tipo").GetString() == "ReclamoDePeaje");

        Assert.False(peaje.GetProperty("sePudoConsultar").GetBoolean());
        Assert.Contains("`RN-92` no está construida", peaje.GetProperty("porQueNo").GetString());
    }

    [Fact]
    public async Task Una_obligacion_de_reintegro_abierta_ENTRA_al_saldo_con_su_antiguedad()
    {
        var v = await Sembrar("SA-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, "/reintegros", new
        {
            Id = Ulid.NewUlid().ToString(),
            Direccion = "AFavorDeLaInstitucion",
            Causa = "SinCausaIdentificada",
            IdResponsable = v,
            Monto = 3_400m,
            IdMision = (string?)null,
            IdAsignacion = (string?)null,

            // Mayo de 2026 — el ejemplo que la regla usa: «una obligación de reintegro de mayo».
            FechaDelHecho = new DateOnly(2026, 5, 12),
            Persona = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            Motivo = "Faltante constatado al liquidar.",
            Momento = new DateTimeOffset(2026, 5, 20, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        var inv = await Leer(cliente, "/saldo-de-apertura/inventario/2026-12-31");

        var r = inv.GetProperty("renglones").EnumerateArray().First(x =>
            x.GetProperty("tipo").GetString() == "ObligacionDeReintegro" &&
            x.GetProperty("descripcion").GetString()!.Contains(v));

        // La antigüedad se cuenta desde el hecho: 233 días del 12/05 al 31/12.
        Assert.Equal("2026-05-12", r.GetProperty("fechaDelHecho").GetString());
        Assert.Equal(233, r.GetProperty("antiguedadEnDias").GetInt32());
        Assert.Equal(3_400m, r.GetProperty("monto").GetDecimal());
        Assert.False(r.GetProperty("impideCerrar").GetBoolean());
    }

    [Fact]
    public async Task El_saldo_es_un_documento_con_FOLIO_y_uno_por_ejercicio()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Producir(cliente, "SA-2098-001", "2098", new DateOnly(2097, 12, 31));

        var s = await Leer(cliente, "/saldo-de-apertura/2098");
        Assert.Equal("SA-2098-001", s.GetProperty("resumen").GetProperty("folio").GetString());

        // Un segundo dejaría dos inventarios del mismo corte, y el acta no podría citar cuál.
        var segunda = await cliente.PostAsJsonAsync("/saldo-de-apertura", Cuerpo(
            "SA-2098-002", "2098", new DateOnly(2097, 12, 31)));

        Assert.False(segunda.IsSuccessStatusCode);
        Assert.Contains("no podría citar", await segunda.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task El_saldo_SIN_folio_se_rechaza()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/saldo-de-apertura", Cuerpo(
            "  ", "2099", new DateOnly(2098, 12, 31)));

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("no se puede citar en el acta",
            await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task El_ARRASTRE_entre_ejercicios_hace_visible_lo_que_ya_venia()
    {
        // `RN-97` punto 3: un renglón que aparece en tres saldos consecutivos es visible como
        // tal. Es lo que impide presentarlo como pendiente reciente cada enero.
        var v = await Sembrar("SA-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, "/reintegros", new
        {
            Id = Ulid.NewUlid().ToString(),
            Direccion = "AFavorDeLaInstitucion",
            Causa = "AplicacionAFinDistinto",
            IdResponsable = v,
            Monto = 900m,
            IdMision = (string?)null,
            IdAsignacion = (string?)null,
            FechaDelHecho = new DateOnly(2090, 3, 1),
            Persona = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            Motivo = "Fondo aplicado a fin distinto.",
            Momento = new DateTimeOffset(2090, 3, 10, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        await Producir(cliente, "SA-2091-001", "2091", new DateOnly(2090, 12, 31));
        await Producir(cliente, "SA-2092-001", "2092", new DateOnly(2091, 12, 31));
        await Producir(cliente, "SA-2093-001", "2093", new DateOnly(2092, 12, 31));

        var tercero = await Leer(cliente, "/saldo-de-apertura/2093");

        var r = tercero.GetProperty("renglones").EnumerateArray().First(x =>
            x.GetProperty("fechaDelHecho").GetString() == "2090-03-01");

        // Vino de dos saldos anteriores, y la antigüedad sigue corriendo desde marzo de 2090.
        Assert.Equal(2, r.GetProperty("saldosAnteriores").GetInt32());
        Assert.True(r.GetProperty("antiguedadEnDias").GetInt32() > 1_000);

        // El primero de todos se declara como inicial de implantación; los siguientes no.
        var primero = await Leer(cliente, "/saldo-de-apertura/2091");
        Assert.False(
            tercero.GetProperty("resumen").GetProperty("esInicialDeImplantacion").GetBoolean());
        Assert.True(tercero.GetProperty("resumen").GetProperty("arrastrados").GetInt32() > 0);
        Assert.NotEqual(
            primero.GetProperty("resumen").GetProperty("folio").GetString(),
            tercero.GetProperty("resumen").GetProperty("folio").GetString());
    }

    [Fact]
    public async Task Un_renglon_RESUELTO_deja_de_arrastrar_pero_no_se_borra()
    {
        // `RN-97` punto 6: el residuo al cierre siguiente es el nuevo saldo. Y borrarlo haría
        // que el arrastre entre ejercicios dejara de verse.
        var v = await Sembrar("SA-0006");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, "/reintegros", new
        {
            Id = Ulid.NewUlid().ToString(),
            Direccion = "AFavorDeLaInstitucion",
            Causa = "Extravio",
            IdResponsable = v,
            Monto = 500m,
            IdMision = (string?)null,
            IdAsignacion = (string?)null,
            FechaDelHecho = new DateOnly(2080, 6, 1),
            Persona = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            Motivo = "Extravío del sobre.",
            Momento = new DateTimeOffset(2080, 6, 5, 9, 0, 0, TimeSpan.FromHours(-6)),
        });

        await Producir(cliente, "SA-2081-001", "2081", new DateOnly(2080, 12, 31));

        var s = await Leer(cliente, "/saldo-de-apertura/2081");
        var antes = s.GetProperty("renglones").EnumerateArray().Count();

        await using var contexto = baseDePruebas.Contexto();
        var saldo = await contexto.SaldosDeApertura
            .Include(x => x.Renglones)
            .SingleAsync(x => x.Ejercicio == "2081");

        var renglon = saldo.Renglones.First(r =>
            r.FechaDelHecho == new DateOnly(2080, 6, 1));

        await Post(cliente, $"/saldo-de-apertura/renglones/{renglon.Id}/resolver", new
        {
            ComoSeResolvio = "El motorista reintegró los L 500 con acta RE-2081-0004.",
            Fecha = new DateOnly(2081, 3, 20),
        });

        // Ya no figura como vivo, pero la fila sigue en la base con su resolución.
        var despues = await Leer(cliente, "/saldo-de-apertura/2081");
        Assert.Equal(antes - 1, despues.GetProperty("renglones").EnumerateArray().Count());

        await using var otro = baseDePruebas.Contexto();
        var fila = await otro.Set<Sigti.Datos.M14_Auditoria.FilaDeRenglon>()
            .SingleAsync(r => r.Id == renglon.Id);

        Assert.Equal(new DateOnly(2081, 3, 20), fila.ResueltoEn);
        Assert.Contains("RE-2081-0004", fila.ComoSeResolvio);
    }

    [Fact]
    public async Task Resolver_un_renglon_exige_decir_COMO()
    {
        // Sin eso, resolver es indistinguible de vaciar el saldo — que es la presión que la
        // regla nombra.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync(
            $"/saldo-de-apertura/renglones/{Ulid.NewUlid()}/resolver",
            new { ComoSeResolvio = "   ", Fecha = new DateOnly(2081, 3, 20) });

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("vaciar el saldo", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task La_serie_historica_se_puede_leer_entera()
    {
        // `RN-97` punto 5: se reporta a Gerencia Administrativa y a Auditoría Interna al inicio
        // del ejercicio, **con su serie**.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Producir(cliente, "SA-2071-001", "2071", new DateOnly(2070, 12, 31));

        var serie = await Leer(cliente, "/saldo-de-apertura");

        Assert.Contains(serie.EnumerateArray(), s =>
            s.GetProperty("ejercicio").GetString() == "2071");
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private async Task<string> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        var r = await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
        return r.Conductor;
    }

    private static object Cuerpo(string folio, string ejercicio, DateOnly corte) => new
    {
        Id = Ulid.NewUlid().ToString(),
        Folio = folio,
        Ejercicio = ejercicio,
        Corte = corte,
        Persona = "P-AUDITORIA",
        Puesto = "PU-AUDITORIA",
        Momento = Ahora,
    };

    private static Task Producir(
        HttpClient cliente, string folio, string ejercicio, DateOnly corte) =>
        Post(cliente, "/saldo-de-apertura", Cuerpo(folio, ejercicio, corte));

    private static async Task<JsonElement> Leer(HttpClient cliente, string ruta)
    {
        var respuesta = await cliente.GetAsync(ruta);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"GET {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());

        return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static async Task Post(HttpClient cliente, string ruta, object cuerpo)
    {
        var respuesta = await cliente.PostAsJsonAsync(ruta, cuerpo);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());
    }
}
