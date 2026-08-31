using System.Net;
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
/// `RN-30` cableada — el cálculo está probado aparte; acá se prueba <b>de dónde salen los
/// números</b>.
///
/// ── Lo que sólo se ve cruzando las capas ─────────────────────────────────────
/// Los kilómetros salen del diario de la <b>misión</b> (`T-18` menos `T-14`), los galones de los
/// asientos `V-04` del <b>vale</b>, y el esperado de los parámetros o del histórico del
/// <b>vehículo</b>. Tres agregados. Un servicio que tome el dato equivocado de cualquiera de los
/// tres produce un dictamen con la misma pinta de autoridad y con el número mal.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class ConciliacionPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task Los_kilometros_salen_del_diario_de_la_mision_y_los_galones_del_vale()
    {
        var r = await Sembrar("CN-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        // Sale en 84,000 y vuelve en 84,420: 420 km. Carga 30 galones en dos veces.
        var (_, vale) = await MisionLiquidada(cliente, r, salida: 84_000, retorno: 84_420,
                                              cargas: [(18m, 900m), (12m, 600m)]);

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");

        Assert.Equal(420, c.GetProperty("kilometros").GetInt32());
        Assert.Equal(30m, c.GetProperty("galones").GetDecimal());
    }

    [Fact]
    public async Task Sin_rendimiento_esperado_ni_historico_el_dictamen_es_NO_EVALUABLE()
    {
        // La institución no ha fijado el `rendimiento_esperado` —es `[C]`— y este vehículo es
        // nuevo, así que tampoco hay histórico del que proponerlo. **No hay contra qué comparar**,
        // y eso NO es «conforme»: un control que tranquiliza sin haber comparado es peor que
        // ninguno.
        var r = await Sembrar("CN-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var (_, vale) = await MisionLiquidada(cliente, r, 84_000, 84_420, [(30m, 1_500m)]);

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");

        Assert.Equal("NoEvaluable", c.GetProperty("dictamen").GetString());
        Assert.False(c.GetProperty("esHallazgo").GetBoolean());
        Assert.Contains("no hay contra qué comparar", c.GetProperty("evidencia").GetString());
    }

    [Fact]
    public async Task El_dictamen_NO_EVALUABLE_igual_concilia_y_deja_cerrar_la_mision()
    {
        // Si no conciliara, `T-21` no se podría cumplir nunca y **ninguna misión cerraría** hasta
        // que la institución entregue el parámetro. El asiento dice lo que no se pudo evaluar,
        // que es la salida honesta: el hecho ocurre y el diario declara qué no se comprobó.
        var r = await Sembrar("CN-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var (mision, vale) = await MisionLiquidada(cliente, r, 84_000, 84_420, [(30m, 1_500m)]);

        await Post(cliente, $"/combustible/{vale}/conciliar", new
        {
            Ejecuta = "P-AUDITORIA", Momento,
        });

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var uno = vales.EnumerateArray().Single();

        Assert.Equal("Conciliada", uno.GetProperty("estado").GetString());

        // Y el asiento no disimula.
        var motivo = uno.GetProperty("diario").EnumerateArray().Last()
            .GetProperty("motivo").GetString();

        Assert.Contains("NoEvaluable", motivo);
        Assert.Contains("NO EVALUABLE", motivo);
    }

    [Fact]
    public async Task Sin_causa_una_desviacion_NO_concilia_y_el_sistema_decide_que_lo_es()
    {
        // El vehículo ya tiene histórico conciliado, así que el sistema **propone** el esperado
        // (`RN-30` punto 1) y con eso el cálculo corre. Esta misión rinde imposiblemente bien.
        var r = await Sembrar("CN-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await HistoricoConciliado(cliente, r);

        // 1,000 km con 40 galones = 25 km/gal contra ~10 del histórico: +150%.
        var (_, vale) = await MisionLiquidada(cliente, r, 90_000, 91_000, [(40m, 2_000m)], dia: 25);

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");

        Assert.Equal("RendimientoImposible", c.GetProperty("dictamen").GetString());
        Assert.True(c.GetProperty("esHallazgo").GetBoolean());

        // **Quien concilia no elige.** Sin causa, no pasa.
        var rechazo = await cliente.PostComoAsync($"/combustible/{vale}/conciliar", new
        {
            Ejecuta = "P-AUDITORIA", Momento,
        });

        Assert.Equal(HttpStatusCode.Conflict, rechazo.StatusCode);
        Assert.Contains("INV-35", await rechazo.Content.ReadAsStringAsync());

        // Con causa, sí — y queda como desviación, no como conforme.
        await Post(cliente, $"/combustible/{vale}/conciliar", new
        {
            Ejecuta = "P-AUDITORIA",
            Momento,
            Causa = "Ruta de bajada continua. Se cruza contra peajes.",
        });

        var estado = await cliente.GetFromJsonAsync<JsonElement>(
            $"/combustible/{vale}/conciliacion");

        Assert.Equal("RendimientoImposible", estado.GetProperty("dictamen").GetString());
    }

    [Fact]
    public async Task El_esperado_propuesto_dice_que_es_una_PROPUESTA()
    {
        // Un dictamen contra la media del propio vehículo y otro contra el valor institucional no
        // valen lo mismo, y sólo el segundo sostiene un hallazgo firme.
        var r = await Sembrar("CN-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await HistoricoConciliado(cliente, r);

        var (_, vale) = await MisionLiquidada(cliente, r, 90_000, 91_000, [(100m, 5_000m)], dia: 25);

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");
        var esperado = c.GetProperty("esperado");

        Assert.Equal("PropuestoDelHistorico", esperado.GetProperty("origen").GetString());
        Assert.Contains("PROPUESTA", esperado.GetProperty("version").GetString());
        Assert.Contains("PROPUESTA del propio histórico", c.GetProperty("evidencia").GetString());
    }

    [Fact]
    public async Task Con_el_odometro_averiado_declarado_el_calculo_queda_no_concluyente()
    {
        // `RN-30`: se conserva para el análisis agregado, «que sí es válido».
        var r = await Sembrar("CN-0006");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await HistoricoConciliado(cliente, r);

        var (mision, vale) = await MisionLiquidada(cliente, r, 90_000, 91_000, [(40m, 2_000m)], dia: 25);

        await Post(cliente, $"/combustible/{vale}/conciliar", new
        {
            Ejecuta = "P-AUDITORIA", Momento, OdometroAveriado = true,
        });

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var uno = vales.EnumerateArray().Single();

        // No es hallazgo, así que el vale queda `Conciliada` — y el asiento conserva las cuentas.
        Assert.Equal("Conciliada", uno.GetProperty("estado").GetString());

        var motivo = uno.GetProperty("diario").EnumerateArray().Last()
            .GetProperty("motivo").GetString();

        Assert.Contains("NoConcluyente", motivo);
        Assert.Contains("RN-90", motivo);
        // Las cifras siguen ahí: no concluyente no es no calculado.
        Assert.Contains("km/gal", motivo);
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    /// <summary>Cinco misiones conciliadas a ~10 km/gal, para que la propuesta tenga de dónde salir.</summary>
    private static async Task HistoricoConciliado(
        HttpClient cliente, FlotaSembrada.ParaProgramar r)
    {
        for (var i = 0; i < 5; i++)
        {
            var desde = 10_000 + (i * 2_000);

            // Lunes a viernes del 16 al 20: después del `Momento` en que se aprueba —si no, la
            // aprobación caduca—, uno por día para que `BD-11` no vea solape del mismo par, y
            // hábiles para que `BD-04` no exija salvoconducto por un sábado.
            var (_, vale) = await MisionLiquidada(
                cliente, r, desde, desde + 1_000, [(100m, 5_000m)], dia: 16 + i);

            await Post(cliente, $"/combustible/{vale}/conciliar", new
            {
                Ejecuta = "P-AUDITORIA", Momento,
                Causa = "Primera conciliación: sin referencia previa.",
            });
        }
    }

    private static async Task<(string Mision, string Vale)> MisionLiquidada(
        HttpClient cliente,
        FlotaSembrada.ParaProgramar r,
        int salida,
        int retorno,
        (decimal Galones, decimal Monto)[] cargas,
        int dia = 16)
    {
        var fondo = Ulid.NewUlid().ToString();

        await Post(cliente, "/fondos", new
        {
            Id = fondo,
            Ambito = "Dependencia",
            AmbitoDeclarado = "Delegacion de Choluteca",
            Desde = new DateOnly(2026, 3, 1),
            Hasta = new DateOnly(2026, 3, 31),
            Solicita = "P-TRANSPORTE",
            Monto = 40_000m,
            Justificacion = "Marzo.",
            Momento,
        });

        await Post(cliente, $"/fondos/{fondo}/aprobar", new
        {
            Ejecuta = "P-GERENCIA", Monto = 40_000m, Partida = "12-01", Momento,
        });

        var mision = Ulid.NewUlid().ToString();

        await Post(cliente, "/misiones", new
        {
            Id = mision,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, dia),
            Retorno = new DateOnly(2026, 3, dia),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 0,
            Momento,
        });

        await Post(cliente, $"/misiones/{mision}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await Post(cliente, $"/misiones/{mision}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });
        await Post(cliente, $"/misiones/{mision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE", Momento, IdVehiculo = r.Vehiculo, IdConductor = r.Conductor,
        });

        var vale = Ulid.NewUlid().ToString();
        var total = cargas.Sum(c => c.Monto);

        await Post(cliente, "/combustible", new
        {
            Id = vale,
            Folio = $"VAL-{Ulid.NewUlid()}",
            IdFondo = fondo,
            IdMision = mision,
            IdMotoristaReceptor = r.Conductor,
            Ejecuta = "P-TRANSPORTE",
            Monto = total,
            Galones = cargas.Sum(c => c.Galones),
            Instrumento = "vale",
            TipoDeCombustible = "Diesel",
            Momento,
        });

        await Post(cliente, $"/misiones/{mision}/despachar", new
        {
            Ejecuta = "P-DESPACHO", Momento, IdVehiculo = r.Vehiculo, IdConductor = r.Conductor,
        });

        await Post(cliente, $"/combustible/{vale}/entregar", new
        {
            Ejecuta = "P-COMBUSTIBLE", Constancia = "Firma", Momento,
        });

        await Post(cliente, $"/misiones/{mision}/iniciar-ruta", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = salida,
        });

        var odometro = salida;

        foreach (var carga in cargas)
        {
            odometro += 10;

            await Post(cliente, $"/combustible/{vale}/consumo", new
            {
                Ejecuta = "P-MOTORISTA",
                carga.Galones,
                carga.Monto,
                Estacion = "Estación Uno",
                Odometro = odometro,
                Comprobante = "F-1",
                Momento,
            });
        }

        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = retorno,
        });

        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD", SaldoDevuelto = 0m, Observacion = (string?)null, Momento,
        });

        return (mision, vale);
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
