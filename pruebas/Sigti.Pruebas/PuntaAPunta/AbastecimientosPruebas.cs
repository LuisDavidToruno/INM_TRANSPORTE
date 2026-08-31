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
/// `RN-83` cableada — todo ingreso de combustible entra al denominador de `RN-30`.
///
/// ── El hueco que esto cierra, dicho con números ──────────────────────────────
/// Un vehículo recorre 900 km. El vale registra 20 galones y los otros 40 salieron del tanque de
/// la sede sin pasar por ningún folio. Con sólo los del fondo, el rendimiento observado da
/// <b>45 km/gal</b> — imposible, y `RN-30` lo marca como probable despacho no registrado.
///
/// <b>Y tiene razón: hubo un despacho que no se registró.</b> Lo que faltaba era poder
/// registrarlo. Sin esta regla, el conciliador busca un fraude donde hay un procedimiento no
/// modelado, y cuando el patrón se repite deja de mirar el indicador.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class AbastecimientosPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly DateTimeOffset EnRuta =
        new(2026, 3, 16, 11, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task El_consumo_del_vale_produce_su_abastecimiento_con_fuente_del_fondo()
    {
        // No son dos hechos: es el mismo visto desde dos lados. El asiento del vale mueve el
        // instrumento; el abastecimiento cuenta el galón.
        var r = await Sembrar("AB-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRuta(cliente, r);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 20m, Monto = 1_000m,
            Estacion = "Estación Uno", Odometro = 84_100, Comprobante = "F-1",
            Momento = EnRuta,
        });

        var lista = await cliente.GetFromJsonAsync<JsonElement>($"/abastecimientos/mision/{mision}");
        var uno = lista.EnumerateArray().Single();

        Assert.Equal("FondoDeLaMision", uno.GetProperty("fuente").GetString());
        Assert.Equal(20m, uno.GetProperty("galones").GetDecimal());
        Assert.True(uno.GetProperty("entraAlCuadreDelFondo").GetBoolean());
    }

    [Fact]
    public async Task El_galon_del_TANQUE_DE_LA_SEDE_entra_al_denominador_y_ya_no_se_ve_como_fraude()
    {
        // **La prueba que resume la regla.** 900 km con 20 galones del vale da 45 km/gal:
        // imposible. Con los 40 del tanque de la sede contados, da 15 — que es lo que ocurrió.
        var r = await Sembrar("AB-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRuta(cliente, r);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 20m, Monto = 1_000m,
            Estacion = "Estación Uno", Odometro = 84_100, Comprobante = "F-1",
            Momento = EnRuta,
        });

        // El despacho que antes no existía para el sistema.
        await Post(cliente, "/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            IdMision = mision,
            OcurridoEn = EnRuta,
            Galones = 40m,
            Odometro = 84_050,
            Fuente = "TanqueInstitucional",
            Registra = "P-ALMACEN",
        });

        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_900,
        });

        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD", SaldoDevuelto = 1_500m,
            Observacion = (string?)null, Momento,
        });

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");

        // 900 km recorridos y **60 galones**, no 20.
        Assert.Equal(900, c.GetProperty("kilometros").GetInt32());
        Assert.Equal(60m, c.GetProperty("galones").GetDecimal());

        // Y la composición se expone — `RN-30` punto 4.
        Assert.Contains("composición", c.GetProperty("evidencia").GetString());
        Assert.Contains("del tanque institucional", c.GetProperty("evidencia").GetString());
    }

    [Fact]
    public async Task Una_donacion_sin_monto_entra_igual_al_denominador()
    {
        // «Un galón sin precio sigue siendo un galón en el denominador.»
        var r = await Sembrar("AB-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, _) = await MisionEnRuta(cliente, r);

        await Post(cliente, "/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            IdMision = mision,
            OcurridoEn = EnRuta,
            Galones = 25m,
            Odometro = 84_200,
            Fuente = "Donacion",
            Registra = "P-MOTORISTA",
            Monto = (decimal?)null,
        });

        var lista = await cliente.GetFromJsonAsync<JsonElement>($"/abastecimientos/mision/{mision}");
        var uno = lista.EnumerateArray().Single();

        Assert.Equal(25m, uno.GetProperty("galones").GetDecimal());
        Assert.Equal(JsonValueKind.Null, uno.GetProperty("monto").ValueKind);
        Assert.False(uno.GetProperty("entraAlCuadreDelFondo").GetBoolean());
    }

    [Fact]
    public async Task El_peculio_del_servidor_marca_reintegro_y_NO_entra_al_cuadre_del_fondo()
    {
        var r = await Sembrar("AB-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, _) = await MisionEnRuta(cliente, r);

        await Post(cliente, "/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            IdMision = mision,
            OcurridoEn = EnRuta,
            Galones = 10m,
            Odometro = 84_300,
            Fuente = "PeculioDelServidor",
            Registra = "P-MOTORISTA",
            Monto = 500m,
            Estacion = "Estación en ruta",
            Comprobante = "F-2231",
        });

        var uno = (await cliente.GetFromJsonAsync<JsonElement>($"/abastecimientos/mision/{mision}"))
            .EnumerateArray().Single();

        Assert.True(uno.GetProperty("generaReintegro").GetBoolean());
        Assert.False(uno.GetProperty("entraAlCuadreDelFondo").GetBoolean());
    }

    [Fact]
    public async Task El_del_fondo_NO_entra_por_esta_puerta()
    {
        // Tiene la suya, y además mueve el vale. Dejarlo entrar acá crearía un galón del fondo
        // que no descontó saldo de ningún folio.
        var r = await Sembrar("AB-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            OcurridoEn = EnRuta,
            Galones = 20m,
            Odometro = 84_100,
            Fuente = "FondoDeLaMision",
            Registra = "P-MOTORISTA",
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("se registra contra su vale", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Un_abastecimiento_NO_se_imputa_a_una_mision_de_otro_vehiculo()
    {
        // Los galones de un tanque no explican los kilómetros de otro: imputarlos mal falsearía
        // el rendimiento de las dos misiones a la vez.
        var uno = await Sembrar("AB-0006");
        var otro = await Sembrar("AB-0007");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, _) = await MisionEnRuta(cliente, uno);

        var respuesta = await cliente.PostAsJsonAsync("/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = otro.Vehiculo,
            IdMision = mision,
            OcurridoEn = EnRuta,
            Galones = 30m,
            Odometro = 1_000,
            Fuente = "TanqueInstitucional",
            Registra = "P-ALMACEN",
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("no explican los kilómetros de otro",
            await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Un_abastecimiento_SIN_mision_se_registra_igual()
    {
        // `RN-83` aplica «a todo vehículo de la flota, **en misión o fuera de ella**». El
        // reabastecimiento de rutina en el predio no tiene expediente al que colgarse.
        var r = await Sembrar("AB-0008");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, "/abastecimientos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdVehiculo = r.Vehiculo,
            OcurridoEn = EnRuta,
            Galones = 15m,
            Odometro = 500,
            Fuente = "TanqueInstitucional",
            Registra = "P-ALMACEN",
        });
    }

    [Fact]
    public async Task El_nivel_del_tanque_queda_en_la_bitacora_a_la_salida_y_al_retorno()
    {
        // **El reparo deja de ser una casilla.** Hasta ahora quien conciliaba marcaba a mano
        // «salió y volvió con niveles muy distintos», porque el sistema no tenía el dato — y una
        // casilla que alguien olvida marcar deja pasar un cálculo que no significa nada.
        var r = await Sembrar("AB-0009");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRuta(cliente, r, nivelDeSalida: 1m);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 20m, Monto = 1_000m,
            Estacion = "Estación Uno", Odometro = 84_100, Comprobante = "F-1",
            Momento = EnRuta,
        });

        // Salió con el tanque lleno y volvió a un octavo: la resta de galones cargados no
        // explica lo que se gastó, y el rendimiento observado no significa nada.
        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_900,
            NivelDeTanque = 0.125m, EscalaDelNivel = "FraccionDelIndicador",
        });

        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD", SaldoDevuelto = 1_500m,
            Observacion = (string?)null, Momento,
        });

        // Las dos lecturas quedan en el diario. Es lo que el servicio lee para decidir si el
        // cálculo es concluyente, sin que nadie tenga que marcar una casilla.
        var expediente = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{mision}");
        var diario = expediente.GetProperty("diario").EnumerateArray().ToList();

        var salida = diario.Single(t => t.GetProperty("id").GetString() == "T-14");
        var retorno = diario.Single(t => t.GetProperty("id").GetString() == "T-18");

        Assert.Contains("tanque a 100", salida.GetProperty("motivo").GetString());
        Assert.Contains("tanque a 13", retorno.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task Sin_las_DOS_lecturas_el_nivel_no_se_estima_y_el_reparo_no_se_activa()
    {
        // `RN-80`: el campo no consignado se declara y **no se estima**. Estimarlo produciría un
        // remanente inventado que después nadie podría distinguir de uno medido.
        var r = await Sembrar("AB-0010");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRuta(cliente, r, nivelDeSalida: 1m);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 20m, Monto = 1_000m,
            Estacion = "Estación Uno", Odometro = 84_100, Comprobante = "F-1",
            Momento = EnRuta,
        });

        // Sin nivel al retornar: el diario lo dice, y el cálculo no se declara no concluyente
        // por un dato que nadie tomó.
        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_900,
        });

        var expediente = await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{mision}");

        var retorno = expediente.GetProperty("diario").EnumerateArray()
            .Single(t => t.GetProperty("id").GetString() == "T-18");

        Assert.Contains("NO CONSIGNADO", retorno.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task El_tanque_que_vuelve_SERVIDO_no_se_cuenta_como_consumo_de_esta_mision()
    {
        // **El caso que `CE-07` describe.** El vehículo sale a un cuarto de tanque, carga 60
        // galones y vuelve con tres cuartos: 30 de esos galones siguen en el tanque y no los
        // gastó esta misión. Contarlos la haría aparecer consumiendo el doble.
        //
        // «Lo que no puede pasar es que un tanque lleno pagado con fondo de esta misión
        // desaparezca del expediente.»
        var r = await Sembrar("RM-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRuta(cliente, r, nivelDeSalida: 0.25m);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 60m, Monto = 2_400m,
            Estacion = "Estación Uno", Odometro = 84_100, Comprobante = "F-1",
            Momento = EnRuta,
        });

        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_600,
            NivelDeTanque = 0.75m, EscalaDelNivel = "FraccionDelIndicador",
        });

        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD", SaldoDevuelto = 100m,
            Observacion = (string?)null, Momento,
        });

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");

        // Entraron 60 galones al tanque…
        Assert.Equal(60m, c.GetProperty("abastecidos").GetDecimal());

        // …y la misión quemó 30: medio tanque de sesenta galones quedó adentro.
        Assert.Equal(30m, c.GetProperty("galones").GetDecimal());

        var remanente = c.GetProperty("remanente");
        Assert.True(remanente.GetProperty("calculable").GetBoolean());
        Assert.Equal(30m, remanente.GetProperty("galones").GetDecimal());
        Assert.Contains("no los gastó esta misión", remanente.GetProperty("explicacion").GetString());
    }

    [Fact]
    public async Task Salir_lleno_y_volver_casi_vacio_hace_que_el_consumo_SUPERE_lo_cargado()
    {
        // `RN-30` lo nombra: «el problema aparece cuando sale lleno y retorna vacío: los galones
        // consumidos exceden a los cargados». Sin el nivel, ese exceso no se ve.
        var r = await Sembrar("RM-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRuta(cliente, r, nivelDeSalida: 1m);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 20m, Monto = 800m,
            Estacion = "Estación Uno", Odometro = 84_100, Comprobante = "F-1",
            Momento = EnRuta,
        });

        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_900,
            NivelDeTanque = 0.25m, EscalaDelNivel = "FraccionDelIndicador",
        });

        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD", SaldoDevuelto = 1_700m,
            Observacion = (string?)null, Momento,
        });

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");

        // Cargó 20 y además quemó 45 que llevaba: perdió tres cuartos de un tanque de sesenta.
        Assert.Equal(20m, c.GetProperty("abastecidos").GetDecimal());
        Assert.Equal(65m, c.GetProperty("galones").GetDecimal());

        Assert.Contains("ya estaba en el tanque",
            c.GetProperty("remanente").GetProperty("explicacion").GetString());
    }

    [Fact]
    public async Task Sin_el_nivel_al_retorno_el_remanente_NO_se_estima()
    {
        // `RN-80`. El consumido iguala a lo abastecido porque es lo mejor que se puede afirmar,
        // y **la evidencia dice que no es lo mismo que un remanente de cero**.
        var r = await Sembrar("RM-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vale) = await MisionEnRuta(cliente, r, nivelDeSalida: 1m);

        await Post(cliente, $"/combustible/{vale}/consumo", new
        {
            Ejecuta = "P-MOTORISTA", Galones = 40m, Monto = 1_600m,
            Estacion = "Estación Uno", Odometro = 84_100, Comprobante = "F-1",
            Momento = EnRuta,
        });

        await Post(cliente, $"/misiones/{mision}/retornar", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_900,
            TanqueNoConsignado = "El indicador está averiado.",
        });

        await Post(cliente, $"/combustible/{vale}/liquidar", new
        {
            Ejecuta = "P-CONTABILIDAD", SaldoDevuelto = 900m,
            Observacion = (string?)null, Momento,
        });

        var c = await cliente.GetFromJsonAsync<JsonElement>($"/combustible/{vale}/conciliacion");

        Assert.Equal(40m, c.GetProperty("abastecidos").GetDecimal());
        Assert.Equal(40m, c.GetProperty("galones").GetDecimal());

        var remanente = c.GetProperty("remanente");
        Assert.False(remanente.GetProperty("calculable").GetBoolean());
        Assert.Equal(JsonValueKind.Null, remanente.GetProperty("galones").ValueKind);
        Assert.Contains("prohíbe estimarlo", remanente.GetProperty("explicacion").GetString());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    private static async Task<(string Mision, string Vale)> MisionEnRuta(
        HttpClient cliente, FlotaSembrada.ParaProgramar r, decimal? nivelDeSalida = null)
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

        await Post(cliente, $"/combustible/{vale}/entregar", new
        {
            Ejecuta = "P-COMBUSTIBLE", Constancia = "Firma", Momento,
        });

        await Post(cliente, $"/misiones/{mision}/iniciar-ruta", new
        {
            Ejecuta = "P-MOTORISTA", Momento, Odometro = 84_000,
            NivelDeTanque = nivelDeSalida, EscalaDelNivel = "FraccionDelIndicador",
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
