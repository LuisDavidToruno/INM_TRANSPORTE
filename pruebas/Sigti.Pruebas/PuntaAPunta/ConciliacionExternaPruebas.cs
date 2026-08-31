using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// `RN-95` cableada — la conciliación contra fuentes externas.
///
/// ── Lo que revela y `RN-30` no puede ver ─────────────────────────────────────
/// <i>«Una conciliación que solo compara nuestros datos con nuestros datos verifica coherencia
/// interna, no veracidad. <b>Un registro completo y coherente puede ser completamente falso</b>,
/// y solo la fuente externa lo revela»</i>.
///
/// Los tres casos que la originaron, de `CE-28`: el comprobante duplicado en el estado de cuenta
/// del proveedor, el paso por caseta de un domingo sin misión, y las multas notificadas meses
/// después.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class ConciliacionExternaPruebas(BaseDePruebas baseDePruebas)
{
    // **Cada prueba concilia su propio día.** La conciliación cruza el alcance de datos a
    // propósito (`RN-95` punto 3: un comprobante duplicado entre delegaciones se detecta), así
    // que dos pruebas sobre el mismo rango se ven los asientos entre sí. Eso es la regla
    // funcionando; lo que hay que aislar es la prueba.
    private static readonly DateOnly Desde = new(2026, 8, 1);
    private static readonly DateOnly Hasta = new(2026, 8, 31);
    private static readonly DateOnly Plazo = new(2026, 10, 15);

    private static readonly DateTimeOffset Corte =
        new(2026, 9, 5, 9, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task El_consumo_registrado_que_el_proveedor_SI_reporta_cuadra()
    {
        var v = await Sembrar("CN-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = await Fuente(cliente);
        await Abastecer(cliente, v, 11, 1_760m, "F-CN0001");

        var r = await Ejecutar(cliente, fuente, [Linea("L1", 11, 1_760m, "F-CN0001", v.Placa)], 11);

        Assert.Equal(1, r.GetProperty("coincidentes").GetInt32());
        Assert.Equal(0, r.GetProperty("diferencias").GetInt32());
    }

    [Fact]
    public async Task El_comprobante_DUPLICADO_del_proveedor_aparece_como_diferencia()
    {
        // Uno de los tres casos de `CE-28`. `RN-84` hace único el comprobante en la
        // institución: dos cobros con el mismo son un cobro de más.
        var v = await Sembrar("CN-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = await Fuente(cliente);
        await Abastecer(cliente, v, 12, 1_760m, "F-CN0002");

        var r = await Ejecutar(cliente, fuente,
        [
            Linea("L1", 12, 1_760m, "F-CN0002", v.Placa),
            Linea("L2", 12, 1_760m, "F-CN0002", v.Placa),
        ], 12);

        Assert.Equal(1, r.GetProperty("coincidentes").GetInt32());
        Assert.Equal(1, r.GetProperty("diferencias").GetInt32());
        Assert.Equal(1_760m, r.GetProperty("montoSoloEnLaFuente").GetDecimal());
    }

    [Fact]
    public async Task Lo_que_registramos_y_el_proveedor_NO_reporta_tambien_abre_expediente()
    {
        // «Puede ser un comprobante falso, o una estación que no reportó. La conciliación no
        // presume cuál». Conciliar en un solo sentido dejaría fuera el caso más grave.
        var v = await Sembrar("CN-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = await Fuente(cliente);
        await Abastecer(cliente, v, 13, 1_760m, "F-CN0003");

        var r = await Ejecutar(cliente, fuente, [], 13);

        Assert.Equal(1_760m, r.GetProperty("montoSoloEnSigti").GetDecimal());

        var solo = r.GetProperty("soloEnSigti").EnumerateArray().Single();
        Assert.Equal("abastecimiento", solo.GetProperty("origen").GetString());

        // Y queda como expediente con responsable y plazo.
        var abiertas = await Leer(cliente, "/conciliacion/diferencias");
        var d = abiertas.EnumerateArray().Single(x =>
            x.GetProperty("referencia").GetString() == "F-CN0003");

        Assert.Equal("SoloEnSigti", d.GetProperty("lado").GetString());
        Assert.Equal("P-AUDITORIA", d.GetProperty("responsable").GetString());
        Assert.Contains("no presume cuál", d.GetProperty("explicacion").GetString());
    }

    [Fact]
    public async Task Una_linea_que_no_corresponde_a_ningun_vehiculo_queda_NO_RESUELTA()
    {
        // `RN-95` casos límite: «puede ser un error del proveedor y puede no serlo». No se
        // asigna por parecido.
        var v = await Sembrar("CN-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = await Fuente(cliente);

        var r = await Ejecutar(cliente, fuente,
            [Linea("L1", 14, 900m, "F-AJENO", placa: "ZZZ-9999")], 14);

        Assert.Equal(1, r.GetProperty("sinVehiculoResuelto").GetInt32());

        var linea = r.GetProperty("soloEnLaFuente").EnumerateArray().Single();
        Assert.Equal(JsonValueKind.Null, linea.GetProperty("vehiculo").ValueKind);
        Assert.Contains("puede no serlo", linea.GetProperty("explicacion").GetString());
    }

    [Fact]
    public async Task El_numero_de_BIEN_resuelve_antes_que_la_placa()
    {
        // `RN-66`: la placa va última porque se reasigna. La línea trae las dos y apuntan a
        // vehículos distintos.
        var v = await Sembrar("CN-0005", bien: "BN-CN0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = await Fuente(cliente);

        var r = await Ejecutar(cliente, fuente,
            [Linea("L1", 15, 900m, "F-X", placa: "ZZZ-9999", bien: "BN-CN0005")], 15);

        var linea = r.GetProperty("soloEnLaFuente").EnumerateArray().Single();

        Assert.Equal(v.Id, linea.GetProperty("vehiculo").GetString());
        Assert.Equal("BienDelInventario", linea.GetProperty("ancla").GetString());
    }

    [Fact]
    public async Task Una_fuente_NO_DISPONIBLE_no_se_puede_conciliar()
    {
        // Produciría cero diferencias sobre cero líneas, y ese cero se lee después como
        // conformidad.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/conciliacion/fuentes", new
        {
            Id = id,
            Tipo = "EstadoDeCuentaDePeaje",
            Emisor = "COVI-H",
            Formato = "CSV",
            Responsable = "P-COMBUSTIBLE",
            Disponible = false,
            PorQueNoEstaDisponible = "La institución no tiene tag CoviPass.",
        });

        var respuesta = await cliente.PostComoAsync("/conciliacion/ejecutar", new
        {
            IdFuente = id,
            Desde,
            Hasta,
            Lineas = Array.Empty<object>(),
            DocumentoFuente = "vacio.csv",
            Ejecuta = "P-AUDITORIA",
            ResponsableDeSeguimiento = "P-AUDITORIA",
            Plazo,
            Momento = Corte,
        });

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("se lee después como conformidad",
            await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task El_retraso_de_cada_fuente_es_dato_VISIBLE()
    {
        // `RN-95` punto 5: una fuente sin conciliar durante meses es en sí misma una
        // observación de control interno.
        var v = await Sembrar("CN-0007");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Fuente(cliente);

        var antes = await BuscarFuente(cliente, id);
        Assert.Contains("NUNCA se ha conciliado", antes.GetProperty("retraso").GetString());
        Assert.True(antes.GetProperty("atrasada").GetBoolean());

        await Ejecutar(cliente, id, [], 17);

        var despues = await BuscarFuente(cliente, id);
        Assert.NotEqual(JsonValueKind.Null, despues.GetProperty("ultimaConciliacion").ValueKind);
    }

    [Fact]
    public async Task La_ejecucion_queda_con_su_fecha_de_corte_y_su_documento()
    {
        // `RN-94` y `RN-95` punto 6. Sin ellos, dos ejecuciones con datos distintos se ven
        // idénticas y una diferencia no se puede volver a comprobar contra el papel.
        var v = await Sembrar("CN-0008");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = await Fuente(cliente);
        await Ejecutar(cliente, fuente, [Linea("L1", 18, 900m, "F-CN0008", v.Placa)], 18);

        var ejecuciones = await Leer(cliente, "/conciliacion/ejecuciones");
        var e = ejecuciones.EnumerateArray().First(x =>
            x.GetProperty("fuente").GetString() == fuente);

        Assert.Equal("estado-de-cuenta-agosto-2026.pdf",
            e.GetProperty("documentoFuente").GetString());
        Assert.Equal(1, e.GetProperty("sinResolver").GetInt32());
    }

    [Fact]
    public async Task Resolver_una_diferencia_NO_la_borra_y_no_se_resuelve_dos_veces()
    {
        var v = await Sembrar("CN-0009");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = await Fuente(cliente);
        await Ejecutar(cliente, fuente, [Linea("L1", 19, 777m, "F-CN0009", v.Placa)], 19);

        var abiertas = await Leer(cliente, "/conciliacion/diferencias");
        var id = abiertas.EnumerateArray()
            .Single(x => x.GetProperty("referencia").GetString() == "F-CN0009")
            .GetProperty("id").GetString();

        await Post(cliente, $"/conciliacion/diferencias/{id}/resolver", new
        {
            Resolucion = "Error del proveedor: la estación facturó a otra institución. Nota de crédito NC-0091.",
            Momento = Corte,
        });

        // Ya no está entre las abiertas, pero la ejecución la sigue contando.
        var despues = await Leer(cliente, "/conciliacion/diferencias");
        Assert.DoesNotContain(despues.EnumerateArray(), x =>
            x.GetProperty("id").GetString() == id);

        var segunda = await cliente.PostComoAsync(
            $"/conciliacion/diferencias/{id}/resolver",
            new { Resolucion = "Otra cosa.", Momento = Corte });

        Assert.False(segunda.IsSuccessStatusCode);
        Assert.Contains("borraría la que constaba",
            await segunda.Content.ReadAsStringAsync());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private sealed record Vehiculo(string Id, string Placa);

    private async Task<Vehiculo> Sembrar(string prefijo, string? bien = null)
    {
        await using var contexto = baseDePruebas.Contexto();
        await FlotaSembrada.SembrarAsync(contexto);

        // Cada prueba planta su propio vehículo con su placa: la base se comparte, y dos
        // vehículos con la misma placa dejarían la resolución ambigua a propósito — que es
        // otra prueba, no ésta.
        var id = Ulid.NewUlid();
        var placa = $"{prefijo.Replace("-", "")}".ToUpperInvariant()[..6];

        var fila = FlotaSembrada.Vehiculo(
            id, $"{prefijo}-{id.ToString()[^5..]}", placa, "Pick-up doble cabina",
            ClaseNormativa.Automovil, 2_800, 5, remolque: false);

        fila.BienDelInventario = bien;
        contexto.Vehiculos.Add(fila);

        await contexto.SaveChangesAsync();
        return new Vehiculo(id.ToString(), placa);
    }

    private static async Task<string> Fuente(HttpClient cliente)
    {
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/conciliacion/fuentes", new
        {
            Id = id,
            Tipo = "EstadoDeCuentaDeCombustible",
            Emisor = $"Distribuidora {id[..6]}",
            Formato = "CSV",
            Responsable = "P-COMBUSTIBLE",
            Disponible = true,
            PeriodicidadEnDias = 30,
        });

        return id;
    }

    private static object Linea(
        string id, int dia, decimal monto, string? referencia,
        string? placa = null, string? bien = null) => new
        {
            Id = id,
            FechaDelHecho = new DateOnly(2026, 8, dia),
            Monto = monto,
            Referencia = referencia,
            Placa = placa,
            BienDelInventario = bien,
        };

    private static Task Abastecer(
        HttpClient cliente, Vehiculo v, int dia, decimal monto, string comprobante) =>
        Post(cliente, "/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = v.Id,
            OcurridoEn = new DateTimeOffset(2026, 8, dia, 9, 0, 0, TimeSpan.FromHours(-6)),
            Galones = 20m,
            Odometro = 84_000 + dia,
            Fuente = "PeculioDelServidor",
            Registra = "P-MOTORISTA",
            Monto = monto,
            Estacion = "Estación Uno",
            Comprobante = comprobante,
        });

    private static async Task<JsonElement> Ejecutar(
        HttpClient cliente, string fuente, object[] lineas, int dia)
    {
        var respuesta = await cliente.PostComoAsync("/conciliacion/ejecutar", new
        {
            IdFuente = fuente,
            Desde = new DateOnly(2026, 8, dia),
            Hasta = new DateOnly(2026, 8, dia),
            Lineas = lineas,
            DocumentoFuente = "estado-de-cuenta-agosto-2026.pdf",
            Ejecuta = "P-AUDITORIA",
            ResponsableDeSeguimiento = "P-AUDITORIA",
            Plazo,
            Momento = Corte,
        });

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST /conciliacion/ejecutar devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());

        return JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static async Task<JsonElement> BuscarFuente(HttpClient cliente, string id)
    {
        var todas = await Leer(cliente, "/conciliacion/fuentes");
        return todas.EnumerateArray().Single(f => f.GetProperty("id").GetString() == id);
    }

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
        var respuesta = await cliente.PostComoAsync(ruta, cuerpo);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());
    }
}
