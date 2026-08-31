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
/// El consumo capturado en la estación, sin red — `V-04` entrando por `POST /sincronizacion`.
///
/// ── Por qué esto no cabía en el circuito de oficina ──────────────────────────
/// Porque `V-04` es de <b>otro agregado</b>: el vale, no la misión. El endpoint de
/// sincronización estaba escrito suponiendo que todo hecho de campo es una transición de la
/// Orden de Misión, y la idempotencia miraba un solo diario. Un consumo reenviado pasaba por
/// nuevo — y <b>un galón contado dos veces inventa una desviación de conciliación que nadie
/// puede explicar</b>.
///
/// ── Lo que se está protegiendo ───────────────────────────────────────────────
/// `RNF-03`: siete días sin conectividad y <b>cero registros perdidos</b>. Para el
/// combustible eso significa que el galón cargado camino a La Mosquitia llegue con su
/// odómetro, su estación y su fecha del hecho — aunque el vehículo ya esté de vuelta cuando
/// el lote entra.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class ConsumoDesdeCampoPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    /// <summary>La carga ocurrió en ruta, y el lote llega cuatro días después.</summary>
    private static readonly DateTimeOffset EnLaEstacion =
        new(2026, 3, 16, 11, 20, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task El_consumo_capturado_sin_red_entra_con_sus_cinco_datos()
    {
        var r = await Sembrar("CP-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r);

        var lote = await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = mision,
                    Transicion = "V-04",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = EnLaEstacion,
                    IdAsignacion = vale,
                    Carga = new
                    {
                        Galones = 12.5m,
                        Monto = 1_250m,
                        Estacion = "Estación Uno, Choluteca",
                        Odometro = 84_120,
                        Comprobante = "F-0011-9932",
                    },
                },
            },
        });

        Assert.Single(lote.GetProperty("aplicadas").EnumerateArray());
        Assert.Empty(lote.GetProperty("rechazadas").EnumerateArray());

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var uno = vales.EnumerateArray().Single();

        Assert.Equal("Consumida", uno.GetProperty("estado").GetString());
        Assert.Equal(1_250m, uno.GetProperty("consumido").GetDecimal());
        Assert.Equal(12.5m, uno.GetProperty("galonesConsumidos").GetDecimal());

        var consumo = uno.GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("transicion").GetString() == "V-04")
            .GetProperty("consumo");

        Assert.Equal(84_120, consumo.GetProperty("odometro").GetInt32());
        Assert.Equal("Estación Uno, Choluteca", consumo.GetProperty("estacion").GetString());
    }

    [Fact]
    public async Task La_fecha_que_queda_es_la_del_HECHO_no_la_de_la_sincronizacion()
    {
        // `P-4` y `RN-46`. El motorista cargó el 16 y el lote entra cuando vuelve: que el
        // servidor se entere después no cambia a qué día pertenece ese galón.
        var r = await Sembrar("CP-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r);

        await Sincronizar(cliente, Lote(mision, vale, Ulid.NewUlid().ToString()));

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var consumo = vales.EnumerateArray().Single()
            .GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("transicion").GetString() == "V-04");

        Assert.Equal(
            EnLaEstacion,
            consumo.GetProperty("momento").GetDateTimeOffset());
    }

    [Fact]
    public async Task Reenviar_el_mismo_consumo_NO_lo_cuenta_dos_veces()
    {
        // **La prueba que motivó tocar la idempotencia.** El endpoint miraba sólo el diario
        // de la misión, y `V-04` vive en el del vale: cada reintento pasaba por nuevo.
        var r = await Sembrar("CP-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r);
        var captura = Ulid.NewUlid().ToString();

        var primero = await Sincronizar(cliente, Lote(mision, vale, captura));
        Assert.Single(primero.GetProperty("aplicadas").EnumerateArray());

        // El dispositivo que no supo si el servidor recibió reintenta el lote entero.
        var segundo = await Sincronizar(cliente, Lote(mision, vale, captura));

        Assert.Empty(segundo.GetProperty("aplicadas").EnumerateArray());
        Assert.Single(segundo.GetProperty("yaConocidas").EnumerateArray());

        // Y se acusa igual, para que el dispositivo pueda sacarlo de su cola: sin el acuse
        // reintentaría para siempre.
        Assert.Single(segundo.GetProperty("acusadas").EnumerateArray());

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var uno = vales.EnumerateArray().Single();

        Assert.Equal(1_250m, uno.GetProperty("consumido").GetDecimal());
        Assert.Equal(12.5m, uno.GetProperty("galonesConsumidos").GetDecimal());
    }

    [Fact]
    public async Task Un_consumo_sin_vale_se_rechaza_con_motivo_legible_y_el_resto_del_lote_entra()
    {
        // El lote **no es atómico**, a propósito: el dispositivo lleva siete días encima y
        // perderlo todo por un hecho mal armado sería el fallo que este endpoint evita.
        var r = await Sembrar("CP-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r);

        var lote = await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new object[]
            {
                // Sin vale: no hay a qué imputarle el galón.
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = mision,
                    Transicion = "V-04",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = EnLaEstacion,
                    Carga = new
                    {
                        Galones = 5m, Monto = 500m, Estacion = "Otra", Odometro = 84_500,
                        Comprobante = (string?)null, CausaSinComprobante = "Sin factura",
                    },
                },
                // Y uno bien armado, que sí tiene que entrar.
                Hecho(mision, vale, Ulid.NewUlid().ToString()),
            },
        });

        Assert.Single(lote.GetProperty("aplicadas").EnumerateArray());

        var rechazo = lote.GetProperty("rechazadas").EnumerateArray().Single();
        Assert.Contains("adivinar a cuál", rechazo.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task Un_consumo_sin_comprobante_entra_con_su_causa_declarada()
    {
        // `RN-85`: **el registro del abastecimiento no se omite nunca por falta de papel.**
        var r = await Sembrar("CP-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r);

        var lote = await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = mision,
                    Transicion = "V-04",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = EnLaEstacion,
                    IdAsignacion = vale,
                    Carga = new
                    {
                        Galones = 8m,
                        Monto = 800m,
                        Estacion = "Estación sin factura, Danlí",
                        Odometro = 84_300,
                        Comprobante = (string?)null,
                        CausaSinComprobante = "La estación no emitió factura: sistema caído.",
                    },
                },
            },
        });

        Assert.Single(lote.GetProperty("aplicadas").EnumerateArray());

        var vales = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/mision/{mision}");
        var motivo = vales.EnumerateArray().Single()
            .GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("transicion").GetString() == "V-04")
            .GetProperty("motivo").GetString();

        // La causa llega hasta el asiento: perderla en el camino dejaría la ausencia sin
        // explicación en el único lugar donde alguien la va a leer.
        Assert.Contains("SIN COMPROBANTE", motivo);
        Assert.Contains("sistema caído", motivo);
    }

    [Fact]
    public async Task Un_consumo_sin_comprobante_y_SIN_causa_se_rechaza()
    {
        var r = await Sembrar("CP-0006");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r);

        var lote = await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = mision,
                    Transicion = "V-04",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = EnLaEstacion,
                    IdAsignacion = vale,
                    Carga = new
                    {
                        Galones = 8m, Monto = 800m, Estacion = "Sin factura", Odometro = 84_300,
                        Comprobante = (string?)null, CausaSinComprobante = (string?)null,
                    },
                },
            },
        });

        var rechazo = lote.GetProperty("rechazadas").EnumerateArray().Single();
        Assert.Contains("tampoco se disimula", rechazo.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task Un_consumo_contra_un_vale_NO_entregado_se_rechaza()
    {
        // El vale emitido no salió de la custodia de quien lo guarda: no puede haberse
        // consumido. Si el hecho ocurrió igual, lo que hay es una entrega sin registrar — y
        // el motivo tiene que decirlo para que alguien la busque.
        var r = await Sembrar("CP-0007");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r, entregar: false);

        var lote = await Sincronizar(cliente, Lote(mision, vale, Ulid.NewUlid().ToString()));

        var rechazo = lote.GetProperty("rechazadas").EnumerateArray().Single();
        Assert.Contains("V-04", rechazo.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task El_galon_de_OTRA_FUENTE_capturado_sin_red_entra_al_denominador()
    {
        // **El galón que hoy desaparece.** El motorista llena de una donación camino a La
        // Mosquitia y no tiene dónde anotarlo: ese galón no llega al denominador de `RN-30`, y
        // su ausencia se lee como rendimiento imposiblemente bueno.
        var r = await Sembrar("CF-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRutaConVale(cliente, r);

        var lote = await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = mision,
                    Transicion = "A-01",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = EnLaEstacion,
                    Abastecimiento = new
                    {
                        IdVehiculo = r.Vehiculo,
                        Fuente = "Donacion",
                        Galones = 25m,
                        Odometro = 84_300,
                        Estacion = "Puesto de la comunidad",
                    },
                },
            },
        });

        Assert.Single(lote.GetProperty("aplicadas").EnumerateArray());

        var lista = await cliente.GetFromJsonAsync<JsonElement>(
            $"/abastecimientos/mision/{mision}");

        var uno = lista.EnumerateArray().Single();

        Assert.Equal("Donacion", uno.GetProperty("fuente").GetString());
        Assert.Equal(25m, uno.GetProperty("galones").GetDecimal());
        // Sin monto y sin comprobante: una donación no trae ni lo uno ni lo otro, y el galón
        // cuenta igual.
        Assert.Equal(JsonValueKind.Null, uno.GetProperty("monto").ValueKind);
        Assert.False(uno.GetProperty("entraAlCuadreDelFondo").GetBoolean());

        // Y lo que importa: está en el denominador.
        var c = await cliente.GetFromJsonAsync<JsonElement>(
            $"/combustible/{vale}/conciliacion");

        Assert.Equal(25m, c.GetProperty("galones").GetDecimal());
    }

    [Fact]
    public async Task Reenviar_el_mismo_abastecimiento_NO_lo_cuenta_dos_veces()
    {
        // Tercer diario, misma regla: un galón contado dos veces infla el denominador y produce
        // una desviación inventada por el propio sistema.
        var r = await Sembrar("CF-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, _) = await MisionEnRutaConVale(cliente, r);
        var captura = Ulid.NewUlid().ToString();

        object lote = new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = captura,
                    IdExpediente = mision,
                    Transicion = "A-01",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = EnLaEstacion,
                    Abastecimiento = new
                    {
                        IdVehiculo = r.Vehiculo,
                        Fuente = "TanqueInstitucional",
                        Galones = 40m,
                        Odometro = 84_050,
                        Estacion = "Predio de la sede",
                    },
                },
            },
        };

        await Sincronizar(cliente, lote);
        var segundo = await Sincronizar(cliente, lote);

        Assert.Empty(segundo.GetProperty("aplicadas").EnumerateArray());
        Assert.Single(segundo.GetProperty("yaConocidas").EnumerateArray());

        var lista = await cliente.GetFromJsonAsync<JsonElement>(
            $"/abastecimientos/mision/{mision}");

        Assert.Single(lista.EnumerateArray());
    }

    [Fact]
    public async Task Un_abastecimiento_de_rutina_entra_SIN_mision()
    {
        // `RN-83` aplica «a todo vehículo de la flota, **en misión o fuera de ella**». El
        // reabastecimiento en el predio no tiene expediente, y exigirle uno obligaría al
        // dispositivo a inventarlo.
        var r = await Sembrar("CF-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var lote = await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = "",
                    Transicion = "A-01",
                    Ejecuta = "P-ALMACEN",
                    OcurridoEn = EnLaEstacion,
                    Abastecimiento = new
                    {
                        IdVehiculo = r.Vehiculo,
                        Fuente = "TanqueInstitucional",
                        Galones = 15m,
                        Odometro = 500,
                        Estacion = "Predio de la sede",
                    },
                },
            },
        });

        Assert.Single(lote.GetProperty("aplicadas").EnumerateArray());
        Assert.Empty(lote.GetProperty("rechazadas").EnumerateArray());
    }

    [Fact]
    public async Task El_nivel_de_tanque_capturado_en_el_predio_LLEGA_al_asiento()
    {
        // **El defecto que esta prueba atrapa.** El nivel llegaba y se descartaba en silencio: la
        // API lo aceptaba en `T-14`, pero la ruta de sincronización —la única que el cliente de
        // campo usa— construía el odómetro sin él. Se tecleaba, se sincronizaba, y no aparecía en
        // ninguna parte.
        //
        // Es el peor de los tres modos de fallar: no hay error, no hay hueco visible, y el reparo
        // de `RN-30` que depende del nivel nunca se activaba porque el nivel nunca estaba.
        var r = await Sembrar("NT-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = await MisionDespachada(cliente, r);

        await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = mision,
                    Transicion = "T-14",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = Momento,
                    Odometro = 84_000,
                    NivelDeTanque = 1m,
                    EscalaDelNivel = "FraccionDelIndicador",
                },
            },
        });

        var expediente = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{mision}");

        var salida = expediente.GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("id").GetString() == "T-14");

        Assert.Contains("tanque a 100", salida.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task El_tanque_NO_leido_llega_con_su_razon_y_no_se_estima()
    {
        // `RN-80`: el campo no consignado se declara y **no se estima**. Y declararlo sin decir
        // por qué deja la ausencia sin nada que reclamar — no se sabe si faltó porque el
        // indicador estaba averiado o porque nadie se acordó, y sólo la primera se corrige.
        var r = await Sembrar("NT-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = await MisionDespachada(cliente, r);

        await Sincronizar(cliente, new
        {
            IdDispositivo = "DISP-CHO-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = Ulid.NewUlid().ToString(),
                    IdExpediente = mision,
                    Transicion = "T-14",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = Momento,
                    Odometro = 84_000,
                    TanqueNoConsignado = "El indicador está averiado — orden de trabajo 2026-0071.",
                },
            },
        });

        var expediente = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{mision}");

        var motivo = expediente.GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("id").GetString() == "T-14")
            .GetProperty("motivo").GetString();

        Assert.Contains("NO CONSIGNADO", motivo);
        Assert.Contains("indicador está averiado", motivo);
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static object Hecho(string mision, string vale, string captura) => new
    {
        IdDeCaptura = captura,
        IdExpediente = mision,
        Transicion = "V-04",
        Ejecuta = "P-MOTORISTA",
        OcurridoEn = EnLaEstacion,
        IdAsignacion = vale,
        Carga = new
        {
            Galones = 12.5m,
            Monto = 1_250m,
            Estacion = "Estación Uno, Choluteca",
            Odometro = 84_120,
            Comprobante = "F-0011-9932",
        },
    };

    private static object Lote(string mision, string vale, string captura) => new
    {
        IdDispositivo = "DISP-CHO-01",
        Transiciones = new[] { Hecho(mision, vale, captura) },
    };

    private static async Task<JsonElement> Sincronizar(HttpClient cliente, object lote)
    {
        var respuesta = await cliente.PostAsJsonAsync("/sincronizacion", lote);

        // El cuerpo va en el fallo: un «500» a secas obliga a reproducirlo para saber qué pasó.
        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST /sincronizacion devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());

        return await respuesta.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    /// <summary>Una misión despachada, lista para que el dispositivo registre la salida.</summary>
    private static async Task<string> MisionDespachada(
        HttpClient cliente, FlotaSembrada.ParaProgramar r)
    {
        var (mision, _) = await MisionEnRutaConVale(cliente, r, iniciarRuta: false);
        return mision;
    }

    /// <summary>Una misión en ruta con un vale entregado — el estado en que `V-04` cabe.</summary>
    private static async Task<(string Mision, string Vale)> MisionEnRutaConVale(
        HttpClient cliente, FlotaSembrada.ParaProgramar r, bool entregar = true,
        bool iniciarRuta = true)
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
            Salida = new DateOnly(2026, 3, 16),
            Retorno = new DateOnly(2026, 3, 18),
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

        await Post(cliente, "/combustible", new
        {
            Id = vale,
            Folio = $"VAL-{Ulid.NewUlid()}",
            IdFondo = fondo,
            IdMision = mision,
            IdMotoristaReceptor = r.Conductor,
            Ejecuta = "P-TRANSPORTE",
            Monto = 2_500m,
            Galones = 50m,
            Instrumento = "vale",
            TipoDeCombustible = "Diesel",
            Momento,
        });

        await Post(cliente, $"/misiones/{mision}/despachar", new
        {
            Ejecuta = "P-DESPACHO", Momento, IdVehiculo = r.Vehiculo, IdConductor = r.Conductor,
        });

        if (entregar)
            await Post(cliente, $"/combustible/{vale}/entregar", new
            {
                Ejecuta = "P-COMBUSTIBLE", Constancia = "Firma de recepción", Momento,
            });

        if (iniciarRuta)
            await Post(cliente, $"/misiones/{mision}/iniciar-ruta", new
            {
                Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_000,
            });

        return (mision, vale);
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
