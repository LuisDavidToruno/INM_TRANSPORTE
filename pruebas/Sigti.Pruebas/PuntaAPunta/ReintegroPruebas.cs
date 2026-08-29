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
/// `RN-86` cableada — el circuito de reintegro, de punta a punta.
///
/// ── Lo que estas pruebas demuestran que ya no pasa ───────────────────────────
/// `HU-078`: <i>«hoy nada impide seguir entregándole fondo a quien no liquidó el anterior. El
/// saldo se acumula sobre unas pocas personas y aparece recién cuando alguien hace el arqueo
/// del período, meses después»</i>.
///
/// Y `RN-86`: <i>«sin la obligación, el cobro se pierde cuando la misión cierra»</i>.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class ReintegroPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(constructor =>
            constructor.ConfigureServices(servicios =>
            {
                servicios.RemoveAll(typeof(DbContextOptions<SigtiDbContext>));
                servicios.AddDbContext<SigtiDbContext>(opciones =>
                    opciones.UseSqlServer(
                        baseDePruebas.CadenaDeConexion,
                        sql => sql.UseCompatibilityLevel(120)));
            }));

    [Fact]
    public async Task Quien_debe_reintegro_NO_recibe_un_vale_nuevo()
    {
        var r = await Sembrar("RI-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Nominar(cliente, r.Conductor, 3_400m);

        var (fondo, mision) = await MisionProgramada(cliente, r);
        var respuesta = await IntentarEmitir(cliente, fondo, mision, r.Conductor);

        var texto = await respuesta.Content.ReadAsStringAsync();

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("no puede recibir nueva asignación", texto);
        Assert.Contains("3,400.00", texto);

        // El mensaje nombra la deuda con su origen y las dos salidas. Un bloqueo que sólo dice
        // «no» se esquiva emitiendo a nombre de otro motorista, y entonces el registro miente
        // sobre quién recibió el dinero — que es peor que no haber bloqueado.
        Assert.Contains("faltante sin causa identificada", texto);
        Assert.Contains("Gerencia Administrativa", texto);
    }

    [Fact]
    public async Task Gerencia_Administrativa_levanta_el_bloqueo_y_entonces_SI_se_emite()
    {
        var r = await Sembrar("RI-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Nominar(cliente, r.Conductor, 3_400m);
        var (fondo, mision) = await MisionProgramada(cliente, r);

        await Post(cliente, "/reintegros/levantamientos", new
        {
            IdMision = mision,
            IdResponsable = r.Conductor,
            Persona = "P-GERENCIA",
            Puesto = "PU-GERENCIA-ADMIN",
            Motivo = "Único motorista habilitado categoría C disponible para el traslado.",
            Momento,
        });

        var respuesta = await IntentarEmitir(cliente, fondo, mision, r.Conductor);
        Assert.True(respuesta.IsSuccessStatusCode,
            await respuesta.Content.ReadAsStringAsync());

        // Y la excepción queda en el indicador que `RN-86` pide, no sepultada dentro del vale
        // que la usó.
        var indicador = await Leer(cliente, "/reintegros/levantamientos");
        var acto = indicador.EnumerateArray().Single(l =>
            l.GetProperty("mision").GetString() == mision);

        Assert.Equal("P-GERENCIA", acto.GetProperty("persona").GetString());
        Assert.Contains("Único motorista habilitado", acto.GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task El_levantamiento_de_OTRA_mision_no_desbloquea_esta()
    {
        var r = await Sembrar("RI-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Nominar(cliente, r.Conductor, 900m);
        var (fondo, mision) = await MisionProgramada(cliente, r);

        // Un levantamiento por persona sin fecha de fin sería un permiso permanente que nadie
        // se acuerda de revocar.
        await Post(cliente, "/reintegros/levantamientos", new
        {
            IdMision = Ulid.NewUlid().ToString(),
            IdResponsable = r.Conductor,
            Persona = "P-GERENCIA",
            Puesto = "PU-GERENCIA-ADMIN",
            Motivo = "Urgencia de la semana pasada.",
            Momento,
        });

        var respuesta = await IntentarEmitir(cliente, fondo, mision, r.Conductor);
        Assert.False(respuesta.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Saldar_la_obligacion_libera_a_la_persona_y_deja_el_expediente_entero()
    {
        var r = await Sembrar("RI-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var obligacion = await Nominar(cliente, r.Conductor, 3_400m);

        await Post(cliente, $"/reintegros/{obligacion}/movimiento", new
        {
            Movimiento = "R-06",
            Persona = "P-COMBUSTIBLE",
            Puesto = "PU-COMBUSTIBLE",
            Texto = "Acta de reintegro RE-2026-0007, recibido en efectivo.",
            Monto = 3_400m,

            // La fecha del hecho es **cuándo entró el dinero a la caja**. Capturarla distinta
            // para que el plazo no aparezca vencido es falsificar un dato (`CE-26` §5).
            FechaDelHecho = new DateOnly(2026, 3, 11),
            Momento,
        });

        var (fondo, mision) = await MisionProgramada(cliente, r);
        var respuesta = await IntentarEmitir(cliente, fondo, mision, r.Conductor);

        Assert.True(respuesta.IsSuccessStatusCode,
            await respuesta.Content.ReadAsStringAsync());

        // Y el expediente sigue diciendo que hubo faltante. `CE-26`: «si repone, no queda
        // registro de que hubo faltante» es el hallazgo, no el comportamiento deseado.
        var expediente = await Leer(cliente, $"/reintegros/{obligacion}");

        Assert.Equal("Saldada", expediente.GetProperty("resumen").GetProperty("estado").GetString());
        Assert.Equal(3_400m, expediente.GetProperty("resumen").GetProperty("monto").GetDecimal());
        Assert.Equal(0m, expediente.GetProperty("resumen").GetProperty("saldo").GetDecimal());

        var diario = expediente.GetProperty("diario").EnumerateArray().ToList();
        Assert.Equal("R-01", diario[0].GetProperty("movimiento").GetString());
        Assert.Contains("SinCausaIdentificada", diario[0].GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task El_arqueo_muestra_quien_tiene_dinero_afuera_y_desde_cuando()
    {
        var r = await Sembrar("RI-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Nominar(cliente, r.Conductor, 3_400m);

        var arqueo = await Leer(cliente, "/reintegros/arqueo");
        var fila = arqueo.EnumerateArray().Single(p =>
            p.GetProperty("responsable").GetString() == r.Conductor);

        Assert.Equal(3_400m, fila.GetProperty("aCargo").GetDecimal());
        Assert.True(fila.GetProperty("vencido").GetBoolean());

        var obligacion = fila.GetProperty("obligaciones").EnumerateArray().Single();
        Assert.Equal("Determinada", obligacion.GetProperty("estado").GetString());

        // La antigüedad se cuenta desde el hecho original, que es lo que `RN-97` arrastra al
        // ejercicio siguiente — no desde el día en que alguien se sentó a nominarla.
        Assert.True(obligacion.GetProperty("antiguedadEnDias").GetInt32() > 0);
    }

    [Fact]
    public async Task La_obligacion_A_FAVOR_del_servidor_figura_y_NO_lo_bloquea()
    {
        // `CE-26`: «un sistema que solo mide lo que el servidor le debe a la institución no es
        // un sistema de control: es un sistema de cobro».
        var r = await Sembrar("RI-0006");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, "/reintegros", new
        {
            Id = Ulid.NewUlid().ToString(),
            Direccion = "AFavorDelServidor",
            Causa = "PeculioPropio",
            IdResponsable = r.Conductor,
            Monto = 350m,
            IdMision = (string?)null,
            IdAsignacion = (string?)null,
            FechaDelHecho = new DateOnly(2026, 2, 20),
            Persona = "P-TRANSPORTE",
            Puesto = "PU-TRANSPORTE",
            Motivo = "Cargó en Iriona con su dinero porque el fondo se agotó y había que volver.",
            Momento,
        });

        var (fondo, mision) = await MisionProgramada(cliente, r);
        var respuesta = await IntentarEmitir(cliente, fondo, mision, r.Conductor);

        Assert.True(respuesta.IsSuccessStatusCode,
            await respuesta.Content.ReadAsStringAsync());

        var arqueo = await Leer(cliente, "/reintegros/arqueo");
        var fila = arqueo.EnumerateArray().Single(p =>
            p.GetProperty("responsable").GetString() == r.Conductor);

        Assert.Equal(350m, fila.GetProperty("aFavor").GetDecimal());
        Assert.Equal(0m, fila.GetProperty("aCargo").GetDecimal());
    }

    [Fact]
    public async Task Sin_el_plazo_definido_el_saldo_afuera_se_VE_pero_no_se_declara_vencido()
    {
        // `[C]` insumo #32. El vale entregado y no consumido es dinero afuera; sin el
        // parámetro no hay contra qué decir que venció, y el arqueo lo dice en vez de
        // inventarle un plazo a la institución.
        var r = await Sembrar("RI-0007");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (fondo, mision) = await MisionProgramada(cliente, r);
        var vale = await Emitir(cliente, fondo, mision, r.Conductor);

        await Post(cliente, $"/misiones/{mision}/despachar", new
        {
            Ejecuta = "P-DESPACHO", Momento, IdVehiculo = r.Vehiculo, IdConductor = r.Conductor,
        });

        await Post(cliente, $"/combustible/{vale}/entregar", new
        {
            Ejecuta = "P-COMBUSTIBLE", Constancia = "Firma", Momento,
        });

        var arqueo = await Leer(cliente, "/reintegros/arqueo");
        var fila = arqueo.EnumerateArray().Single(p =>
            p.GetProperty("responsable").GetString() == r.Conductor);

        Assert.Equal(2_500m, fila.GetProperty("sinComprobar").GetDecimal());
        Assert.False(fila.GetProperty("vencido").GetBoolean());

        var saldo = fila.GetProperty("saldos").EnumerateArray().Single();
        Assert.Equal(JsonValueKind.Null, saldo.GetProperty("vence").ValueKind);
        Assert.Contains("no ha retornado", saldo.GetProperty("explicacion").GetString());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    private static async Task<string> Nominar(
        HttpClient cliente, string responsable, decimal monto)
    {
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/reintegros", new
        {
            Id = id,
            Direccion = "AFavorDeLaInstitucion",
            Causa = "SinCausaIdentificada",
            IdResponsable = responsable,
            Monto = monto,
            IdMision = (string?)null,
            IdAsignacion = (string?)null,
            FechaDelHecho = new DateOnly(2026, 2, 28),
            Persona = "P-AUDITORIA",
            Puesto = "PU-AUDITORIA",
            Motivo = "Faltante constatado al liquidar, sin causa declarada por el servidor.",
            Momento,
        });

        return id;
    }

    private static async Task<(string Fondo, string Mision)> MisionProgramada(
        HttpClient cliente, FlotaSembrada.ParaProgramar r)
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

        return (fondo, mision);
    }

    private static Task<HttpResponseMessage> IntentarEmitir(
        HttpClient cliente, string fondo, string mision, string conductor) =>
        cliente.PostAsJsonAsync("/combustible", new
        {
            Id = Ulid.NewUlid().ToString(),
            Folio = $"VAL-{Ulid.NewUlid()}",
            IdFondo = fondo,
            IdMision = mision,
            IdMotoristaReceptor = conductor,
            Ejecuta = "P-TRANSPORTE",
            Monto = 2_500m,
            Galones = 50m,
            Instrumento = "vale",
            TipoDeCombustible = "Diesel",
            Momento,
        });

    private static async Task<string> Emitir(
        HttpClient cliente, string fondo, string mision, string conductor)
    {
        var vale = Ulid.NewUlid().ToString();

        await Post(cliente, "/combustible", new
        {
            Id = vale,
            Folio = $"VAL-{Ulid.NewUlid()}",
            IdFondo = fondo,
            IdMision = mision,
            IdMotoristaReceptor = conductor,
            Ejecuta = "P-TRANSPORTE",
            Monto = 2_500m,
            Galones = 50m,
            Instrumento = "vale",
            TipoDeCombustible = "Diesel",
            Momento,
        });

        return vale;
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
        var respuesta = await cliente.PostAsJsonAsync(ruta, cuerpo);

        if (!respuesta.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException(
                $"POST {ruta} devolvió {(int)respuesta.StatusCode}: " +
                await respuesta.Content.ReadAsStringAsync());
    }
}
