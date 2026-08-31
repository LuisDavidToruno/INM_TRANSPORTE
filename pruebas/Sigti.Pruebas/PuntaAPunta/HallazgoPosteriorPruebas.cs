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
/// `RN-93` cableada — el expediente de hallazgo posterior.
///
/// ── Lo que hace posible ──────────────────────────────────────────────────────
/// Corregir el efecto económico de un hallazgo descubierto meses después <b>sin reabrir el
/// expediente cerrado</b>. La regla lo justifica así: <i>«basta con que la reapertura de un
/// expediente cerrado exista para que se use, y basta con que se use una vez para que ningún
/// reporte histórico vuelva a ser reproducible»</i>.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class HallazgoPosteriorPuntaAPuntaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateOnly Hecho = new(2026, 3, 15);
    private static readonly DateOnly Descubierto = new(2026, 11, 20);

    private static readonly DateTimeOffset Ahora =
        new(2026, 11, 20, 10, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task El_expediente_lleva_las_DOS_fechas_y_la_antiguedad_sale_del_hecho()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [Ulid.NewUlid().ToString()]);
        var h = await Leer(cliente, $"/hallazgos/{id}");
        var r = h.GetProperty("resumen");

        Assert.Equal("2026-03-15", r.GetProperty("fechaDelHecho").GetString());
        Assert.Equal("2026-11-20", r.GetProperty("fechaDelDescubrimiento").GetString());

        // 250 días entre el hecho y el descubrimiento. Contar la antigüedad desde el
        // descubrimiento premiaría descubrir tarde.
        Assert.Equal(250, r.GetProperty("diasHastaElDescubrimiento").GetInt32());
    }

    [Fact]
    public async Task Un_hallazgo_SIN_MISION_vinculable_se_abre_igual()
    {
        // El paso por caseta de un domingo, el consumo de un vehículo que ese día no tenía
        // orden. **La ausencia de misión es el hallazgo** (`RN-59`).
        var v = await Sembrar("HP-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [], vehiculo: v.Id);
        var h = await Leer(cliente, $"/hallazgos/{id}");

        Assert.Empty(h.GetProperty("resumen").GetProperty("misiones").EnumerateArray());
        Assert.Contains("SIN MISIÓN VINCULABLE",
            h.GetProperty("diario")[0].GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task El_reverso_muestra_los_TRES_valores_y_no_toca_el_original()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [Ulid.NewUlid().ToString()]);
        await Revertir(cliente, id, "V-04-HP0003", -1_760m);

        var h = await Leer(cliente, $"/hallazgos/{id}");
        var reverso = h.GetProperty("reversos").EnumerateArray().Single();

        Assert.Equal("1,760.00", reverso.GetProperty("valorAnterior").GetString());
        Assert.Equal("0.00", reverso.GetProperty("valorNuevo").GetString());
        Assert.Equal(-1_760m, reverso.GetProperty("efectoEconomico").GetDecimal());

        // §8.3: el reverso afecta los acumulados del período en que se registra, no los del
        // original. Los históricos ya publicados siguen siendo reproducibles.
        Assert.Equal("2026-03", reverso.GetProperty("periodoAfectado").GetString());
        Assert.Equal("2026-11", reverso.GetProperty("periodoDeImputacion").GetString());
    }

    [Fact]
    public async Task Quien_produjo_el_asiento_no_puede_autorizar_su_reverso()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [Ulid.NewUlid().ToString()]);

        var respuesta = await cliente.PostComoAsync($"/hallazgos/{id}/reverso", Reverso(
            "V-04-HP0004", -900m, autoriza: "P-MOTORISTA", autorOriginal: "P-MOTORISTA"));

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("Corregirse a sí mismo",
            await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task El_mismo_asiento_no_se_revierte_dos_veces()
    {
        // Un segundo reverso duplicaría el efecto económico sobre el período corriente, y esa
        // corrección de más no la va a poder rastrear nadie. Lo impone la base, no una
        // comprobación que el próximo endpoint pueda olvidar.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [Ulid.NewUlid().ToString()]);
        await Revertir(cliente, id, "V-04-HP0005", -500m);

        var respuesta = await cliente.PostComoAsync($"/hallazgos/{id}/reverso",
            Reverso("V-04-HP0005", -500m));

        Assert.False(respuesta.IsSuccessStatusCode);
    }

    [Fact]
    public async Task El_ajuste_del_periodo_es_una_CAPA_identificada()
    {
        // `RN-93` punto 3: no se recalculan los históricos ya publicados; se ajusta el período
        // corriente y se muestra el ajuste como capa identificada.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [Ulid.NewUlid().ToString()]);
        await Revertir(cliente, id, "V-04-HP0006", -777m, imputacion: "2099-01");

        var ajuste = await Leer(cliente, "/hallazgos/ajuste/2099-01");

        Assert.Equal(-777m, ajuste.GetProperty("ajuste").GetDecimal());
    }

    [Fact]
    public async Task Una_mision_cerrada_MUESTRA_sus_hallazgos_sin_que_eso_la_modifique()
    {
        // §7.5: «la misión cerrada muestra desde entonces, de forma visible, que tiene hallazgos
        // posteriores vinculados». Se consulta desde el hallazgo — guardar una marca en el
        // expediente cerrado sería modificarlo.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var mision = Ulid.NewUlid().ToString();
        var id = await Abrir(cliente, [mision]);

        var deLaMision = await Leer(cliente, $"/hallazgos/mision/{mision}");
        var h = deLaMision.EnumerateArray().Single();

        Assert.Equal(id, h.GetProperty("id").GetString());
        Assert.True(h.GetProperty("abierto").GetBoolean());
    }

    [Fact]
    public async Task El_error_del_propio_descubridor_se_CIERRA_no_se_borra()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [Ulid.NewUlid().ToString()]);

        await Post(cliente, $"/hallazgos/{id}/resolver", new
        {
            Resolucion = "SinEfecto",
            Fundamento = "La línea del estado de cuenta correspondía a otra institución.",
            Persona = "P-GERENCIA",
            Puesto = "PU-GERENCIA-ADMIN",
            Momento = Ahora,
        });

        var h = await Leer(cliente, $"/hallazgos/{id}");

        Assert.False(h.GetProperty("resumen").GetProperty("abierto").GetBoolean());
        Assert.Equal("SinEfecto", h.GetProperty("resumen").GetProperty("resolucion").GetString());

        // Y el asiento de apertura sigue ahí, con quién lo abrió y cómo.
        Assert.Equal("H-01", h.GetProperty("diario")[0].GetProperty("movimiento").GetString());
    }

    [Fact]
    public async Task Un_expediente_RESUELTO_ya_no_admite_reversos()
    {
        // Igual que una misión cerrada no se reabre: lo que aparezca después es un hallazgo
        // nuevo, no una corrección de éste.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var id = await Abrir(cliente, [Ulid.NewUlid().ToString()]);

        await Post(cliente, $"/hallazgos/{id}/resolver", new
        {
            Resolucion = "SinEfecto",
            Fundamento = "Error de lectura.",
            Persona = "P-GERENCIA",
            Puesto = "PU-GERENCIA-ADMIN",
            Momento = Ahora,
        });

        var respuesta = await cliente.PostComoAsync($"/hallazgos/{id}/reverso",
            Reverso("V-04-HP0009", -100m));

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("no una corrección de éste", await respuesta.Content.ReadAsStringAsync());
    }

    // ── El cableado con `RN-95` ─────────────────────────────────────────────

    [Fact]
    public async Task Cada_diferencia_de_la_conciliacion_ABRE_expediente()
    {
        // `RN-95`: «cada diferencia abre expediente de hallazgo posterior de forma automática
        // (`RN-93`)». Hasta ahora las diferencias quedaban como filas con responsable y plazo;
        // el expediente es lo que les da ciclo propio y asiento reverso.
        var v = await Sembrar("HP-0010");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var fuente = Ulid.NewUlid().ToString();

        await Post(cliente, "/conciliacion/fuentes", new
        {
            Id = fuente,
            Tipo = "EstadoDeCuentaDeCombustible",
            Emisor = $"Distribuidora {fuente[..6]}",
            Formato = "CSV",
            Responsable = "P-COMBUSTIBLE",
            Disponible = true,
            PeriodicidadEnDias = 30,
        });

        await Post(cliente, "/conciliacion/ejecutar", new
        {
            IdFuente = fuente,
            Desde = new DateOnly(2026, 3, 1),
            Hasta = new DateOnly(2026, 3, 31),
            Lineas = new[]
            {
                new
                {
                    Id = "L-HP0010",
                    FechaDelHecho = Hecho,
                    Monto = 1_500m,
                    Referencia = "F-HP0010",
                    Placa = v.Placa,
                },
            },
            DocumentoFuente = "estado-de-cuenta-marzo-2026.csv",
            Ejecuta = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            ResponsableDeSeguimiento = "P-AUDITORIA",
            Plazo = new DateOnly(2027, 1, 15),
            Momento = Ahora,
        });

        var todos = await Leer(cliente, "/hallazgos");

        var abierto = todos.EnumerateArray().Single(h =>
            h.GetProperty("comoSeDescubrio").GetString()!.Contains("L-HP0010"));

        // Sin misión vinculable, con el vehículo resuelto y el período — el caso que `RN-93`
        // describe.
        Assert.Empty(abierto.GetProperty("misiones").EnumerateArray());
        Assert.Equal(v.Id, abierto.GetProperty("vehiculo").GetString());
        Assert.Equal("2026-03", abierto.GetProperty("periodo").GetString());

        // Las dos fechas, distintas: el hecho es el de la línea, el descubrimiento el de la
        // conciliación.
        Assert.Equal("2026-03-15", abierto.GetProperty("fechaDelHecho").GetString());
        Assert.Equal("2026-11-20", abierto.GetProperty("fechaDelDescubrimiento").GetString());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private sealed record Vehiculo(string Id, string Placa);

    private async Task<Vehiculo> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        await FlotaSembrada.SembrarAsync(contexto);

        var id = Ulid.NewUlid();
        var placa = prefijo.Replace("-", "").ToUpperInvariant()[..6];

        contexto.Vehiculos.Add(FlotaSembrada.Vehiculo(
            id, $"{prefijo}-{id.ToString()[^5..]}", placa, "Pick-up doble cabina",
            ClaseNormativa.Automovil, 2_800, 5, remolque: false));

        await contexto.SaveChangesAsync();
        return new Vehiculo(id.ToString(), placa);
    }

    private static async Task<string> Abrir(
        HttpClient cliente, string[] misiones, string? vehiculo = null)
    {
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/hallazgos", new
        {
            Id = id,
            Tipo = "Comprobante duplicado en el estado de cuenta del proveedor",
            FechaDelHecho = Hecho,
            FechaDelDescubrimiento = Descubierto,
            ComoSeDescubrio = "Conciliación del estado de cuenta de marzo",
            Fuente = "Distribuidora Nacional, estado-de-cuenta-marzo-2026.csv",
            Persona = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            Momento = Ahora,
            DocumentoAdjunto = "adjunto.pdf",
            Misiones = misiones,
            IdVehiculo = vehiculo,
            Periodo = "2026-03",
        });

        return id;
    }

    private static object Reverso(
        string identificador, decimal efecto,
        string autoriza = "P-GERENCIA", string autorOriginal = "P-MOTORISTA",
        string imputacion = "2026-11") => new
        {
            TipoDeAsiento = "consumo del vale",
            IdentificadorDelAsiento = identificador,
            DescripcionDelAsiento = "Consumo del vale VAL-CHO-2026-000418",
            Naturaleza = "ReversoEconomico",
            ValorAnterior = "1,760.00",
            ValorNuevo = "0.00",
            FechaDelHechoOriginal = Hecho,
            Persona = "P-GERENCIA",
            Puesto = "PU-GERENCIA-ADMIN",
            Autoriza = autoriza,
            AutorDelAsientoOriginal = autorOriginal,
            MotivoTipificado = "Cobro duplicado del proveedor",
            Fundamento = "Nota de crédito NC-0091.",
            PeriodoAfectado = "2026-03",
            PeriodoDeImputacion = imputacion,
            Momento = Ahora,
            EfectoEconomico = efecto,
        };

    private static Task Revertir(
        HttpClient cliente, string hallazgo, string identificador, decimal efecto,
        string imputacion = "2026-11") =>
        Post(cliente, $"/hallazgos/{hallazgo}/reverso",
            Reverso(identificador, efecto, imputacion: imputacion));

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
