using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Datos.M18_Peajes;
using Sigti.Dominio.M18_Peajes;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// `RN-37` cableada — el cruce peaje × kilometraje × ruta autorizada.
///
/// ── Lo que `NRM-10` pide textualmente ────────────────────────────────────────
/// <i>«Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y <b>el sistema
/// tiene que producirlo solo</b>. Esto es exactamente lo que busca el auditor del TSC:
/// correlación, no comprobantes archivados»</i>.
///
/// ── Y la mitad de estas pruebas es que NO grite ─────────────────────────────
/// La regla advierte que sin poder declarar un desvío <i>«produciría hallazgos falsos en
/// masa»</i>. Un control que grita todos los días es un control que nadie mira.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class CoherenciaDePeajesPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateOnly Salida = new(2026, 5, 12);

    private static readonly DateTimeOffset Partida =
        new(2026, 5, 12, 6, 0, 0, TimeSpan.FromHours(-6));

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    [Fact]
    public async Task Un_peaje_fuera_de_la_ruta_AUTORIZADA_sale_como_hallazgo()
    {
        // La misión se autorizó hasta Comayagua y el vehículo pagó en Yojoa.
        var c = await Sembrar("CO-0001");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();

        await Congelar(cliente, mision, c, [(c.Zambrano, 2), (c.Comayagua, 2)]);

        await Pasar(cliente, mision, c, c.Zambrano, 0.5, 84_030);
        await Pasar(cliente, mision, c, c.Comayagua, 1.5, 84_080);
        await Pasar(cliente, mision, c, c.Yojoa, 3.0, 84_190);

        var d = await Dictamen(cliente, mision);

        var hallazgos = d.GetProperty("incoherencias").EnumerateArray()
            .Where(i => i.GetProperty("esHallazgo").GetBoolean())
            .ToList();

        Assert.Contains(hallazgos, i =>
            i.GetProperty("tipo").GetString() == "PuntoFueraDeRutaAutorizada" &&
            i.GetProperty("explicacion").GetString()!.Contains("«Yojoa» no está en la ruta"));
    }

    [Fact]
    public async Task Un_desvio_declarado_desde_el_campo_lo_deja_de_ser()
    {
        // Honduras tiene derrumbes con regularidad. Sin esta capacidad la regla produciría
        // hallazgos falsos en masa.
        var c = await Sembrar("CO-0002");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();

        await Congelar(cliente, mision, c, [(c.Zambrano, 2), (c.Comayagua, 2)]);

        await Pasar(cliente, mision, c, c.Zambrano, 0.5, 84_030);
        await Pasar(cliente, mision, c, c.Comayagua, 1.5, 84_080);
        await Pasar(cliente, mision, c, c.Siguatepeque, 2.3, 84_130);
        await Pasar(cliente, mision, c, c.Yojoa, 3.0, 84_190);

        await Post(cliente, "/peajes/desvios", new
        {
            IdMision = mision,
            IdVehiculo = c.Vehiculo,
            Desde = Partida.AddHours(2),
            Hasta = Partida.AddHours(6),
            Motivo = "Derrumbe en el km 120 de la CA-5, desvío autorizado por la Policía.",
            Declara = "P-MOTORISTA",
        });

        var d = await Dictamen(cliente, mision);

        Assert.DoesNotContain(d.GetProperty("incoherencias").EnumerateArray(), i =>
            i.GetProperty("esHallazgo").GetBoolean());

        // Pero la incoherencia NO se borra: que existió y que alguien la explicó son dos
        // hechos, y el auditor pregunta por los dos.
        // Son DOS: la ruta congelada llegaba a Comayagua, y el vehículo pasó por Siguatepeque
        // y por Yojoa. El desvío cubre a las dos, y las dos quedan constando.
        var fueraDeRuta = d.GetProperty("incoherencias").EnumerateArray()
            .Where(i => i.GetProperty("tipo").GetString() == "PuntoFueraDeRutaAutorizada")
            .ToList();

        Assert.Equal(2, fueraDeRuta.Count);
        Assert.All(fueraDeRuta, i =>
        {
            Assert.True(i.GetProperty("justificada").GetBoolean());
            Assert.Contains("Derrumbe en el km 120", i.GetProperty("justificacion").GetString());
        });
    }

    [Fact]
    public async Task Saltar_una_caseta_ACTIVA_se_senala_y_nombra_cual()
    {
        var c = await Sembrar("CO-0003");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();

        await Pasar(cliente, mision, c, c.Zambrano, 0.5, 84_030);
        await Pasar(cliente, mision, c, c.Siguatepeque, 2.3, 84_130);

        var d = await Dictamen(cliente, mision);

        var salto = d.GetProperty("incoherencias").EnumerateArray().Single(i =>
            i.GetProperty("tipo").GetString() == "SecuenciaGeograficamenteImposible");

        Assert.Contains("«Comayagua»", salto.GetProperty("explicacion").GetString());
        Assert.Contains("no hay paso registrado", salto.GetProperty("explicacion").GetString());
    }

    [Fact]
    public async Task Una_caseta_CERRADA_ese_dia_no_se_echa_de_menos()
    {
        // `RN-37`: el estado del punto con vigencia evita marcar como omisión un peaje que
        // nadie cobró.
        var c = await Sembrar("CO-0004");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Post(cliente, $"/peajes/puntos/{c.Comayagua}/estado", new
        {
            Estado = "Cerrado",
            Fundamento = "Terminación anticipada de la concesión en este tramo.",
            VigenteDesde = new DateOnly(2026, 5, 1),
        });

        var mision = Ulid.NewUlid().ToString();

        await Pasar(cliente, mision, c, c.Zambrano, 0.5, 84_030);
        await Pasar(cliente, mision, c, c.Siguatepeque, 2.3, 84_130);

        var d = await Dictamen(cliente, mision);

        Assert.DoesNotContain(d.GetProperty("incoherencias").EnumerateArray(), i =>
            i.GetProperty("tipo").GetString() == "SecuenciaGeograficamenteImposible");
    }

    [Fact]
    public async Task Sin_estimado_congelado_la_tercera_dimension_se_declara_NO_EVALUADA()
    {
        // Es la misión de ruta abierta. «Se marca así explícitamente para que la ausencia de
        // hallazgos no se lea como conformidad».
        var c = await Sembrar("CO-0005");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();
        await Pasar(cliente, mision, c, c.Zambrano, 0.5, 84_030);

        var d = await Dictamen(cliente, mision);
        var dim = d.GetProperty("dimensiones");

        Assert.False(dim.GetProperty("contraLaRutaAutorizada").GetBoolean());
        Assert.False(dim.GetProperty("todas").GetBoolean());
        Assert.False(d.GetProperty("coherente").GetBoolean());

        Assert.Contains(dim.GetProperty("porQueNo").EnumerateArray(), m =>
            m.GetString()!.Contains("estimado de peajes congelado"));
    }

    [Fact]
    public async Task Sin_velocidad_maxima_la_dimension_temporal_se_declara_NO_EVALUADA()
    {
        // `[C]`. El parámetro es nulo en la implementación provisional, y el dictamen lo dice
        // en vez de fingir que evaluó los intervalos.
        var c = await Sembrar("CO-0006");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();
        await Pasar(cliente, mision, c, c.Zambrano, 0.5, 84_030);
        await Pasar(cliente, mision, c, c.Comayagua, 0.6, 84_080);

        var d = await Dictamen(cliente, mision);

        Assert.False(d.GetProperty("dimensiones").GetProperty("temporal").GetBoolean());

        Assert.Contains(
            d.GetProperty("dimensiones").GetProperty("porQueNo").EnumerateArray(),
            m => m.GetString()!.Contains("velocidad_media_maxima"));
    }

    [Fact]
    public async Task El_estimado_congelado_no_se_congela_dos_veces()
    {
        var c = await Sembrar("CO-0007");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();
        await Congelar(cliente, mision, c, [(c.Zambrano, 2)]);

        var respuesta = await cliente.PostAsJsonAsync("/peajes/estimacion/congelar", new
        {
            IdMision = mision,
            Cruces = new[] { new { IdPunto = c.Comayagua, Cruces = 2 } },
            IdVehiculo = c.Vehiculo,
            CategoriaDelTipo = (string?)null,
            TipoDeVehiculo = (string?)null,
            FechaPrevista = Salida,
            Congela = "P-JEFATURA",
            Momento = Partida,
        });

        Assert.False(respuesta.IsSuccessStatusCode);
        Assert.Contains("sin respuesta única", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Un_desvio_reenviado_por_el_dispositivo_NO_se_duplica()
    {
        var c = await Sembrar("CO-0008");
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = Ulid.NewUlid().ToString();
        var captura = Ulid.NewUlid().ToString();

        object Cuerpo() => new
        {
            IdMision = mision,
            IdVehiculo = c.Vehiculo,
            Desde = Partida,
            Hasta = (DateTimeOffset?)null,
            Motivo = "Cierre de la CA-5 por manifestación.",
            Declara = "P-MOTORISTA",
            IdDeCaptura = captura,
        };

        await Post(cliente, "/peajes/desvios", Cuerpo());
        await Post(cliente, "/peajes/desvios", Cuerpo());

        await using var contexto = baseDePruebas.Contexto();
        var id = Ulid.Parse(mision);

        Assert.Equal(1, await contexto.DesviosDeclarados.CountAsync(d => d.MisionId == id));
    }

    [Fact]
    public async Task Una_mision_sin_pasos_no_produce_dictamen()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var d = await Leer(cliente, $"/peajes/coherencia/{Ulid.NewUlid()}");

        Assert.Empty(d.EnumerateArray());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private sealed record Corredor(
        string Vehiculo, string Zambrano, string Comayagua, string Siguatepeque, string Yojoa);

    /// <summary>
    /// El Corredor Logístico con su orden geográfico real de sur a norte `[V]`, y un pickup.
    ///
    /// Cada prueba planta <b>su propio corredor</b> con nombres únicos: la base se comparte, y
    /// una prueba que cierra Comayagua no puede cerrárselo a las demás.
    /// </summary>
    private async Task<Corredor> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        var r = await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);

        var vehiculo = Ulid.Parse(r.Vehiculo);
        (await contexto.Vehiculos.SingleAsync(v => v.Id == vehiculo)).NumeroDeEjes = 2;

        if (!await contexto.CategoriasDePeaje.AnyAsync(c => c.Codigo == "LIVIANO"))
        {
            contexto.CategoriasDePeaje.Add(new FilaDeCategoriaDePeaje
            {
                Codigo = "LIVIANO", Nombre = "Liviano/Turismo",
            });

            contexto.ReglasDeCategoriaDePeaje.Add(new FilaDeReglaDeCategoria
            {
                Id = Ulid.NewUlid(), Categoria = "LIVIANO", Prioridad = 20,
                Fundamento = "Automóviles livianos por peso bruto.",
                Clase = Dominio.M03_Flota.ClaseNormativa.Automovil, PesoBrutoHastaKg = 3_500,
                VigenteDesde = new DateOnly(2025, 1, 1), RegistradoDesdeUtc = DateTime.UtcNow,
            });
        }

        var puntos = new List<string>();

        foreach (var (nombre, km) in new[]
        {
            ("Zambrano", 35), ("Comayagua", 85), ("Siguatepeque", 130), ("Yojoa", 190),
        })
        {
            var id = Ulid.NewUlid();

            contexto.PuntosDePeaje.Add(new FilaDePunto
            {
                Id = id,
                Nombre = $"{nombre}",
                Operador = "COVI-H",
                Carretera = "CA-5 Norte",
                Corredor = $"CA-5-{prefijo}",
                Kilometro = km,
            });

            contexto.VigenciasDePunto.Add(new FilaDeVigenciaDelPunto
            {
                Id = Ulid.NewUlid(), PuntoId = id, Estado = EstadoDelPunto.Activo,
                Fundamento = "Concesión vigente.",
                VigenteDesde = new DateOnly(2025, 1, 1), RegistradoDesdeUtc = DateTime.UtcNow,
            });

            contexto.TarifasDePeaje.Add(new FilaDeTarifa
            {
                Id = Ulid.NewUlid(), PuntoId = id, Categoria = "LIVIANO", Monto = 22m,
                Fuente = "SAPP", FechaDeVerificacion = new DateOnly(2026, 3, 1),
                VigenteDesde = new DateOnly(2026, 1, 1), RegistradoDesdeUtc = DateTime.UtcNow,
            });

            puntos.Add(id.ToString());
        }

        await contexto.SaveChangesAsync();

        return new Corredor(r.Vehiculo, puntos[0], puntos[1], puntos[2], puntos[3]);
    }

    private static Task Congelar(
        HttpClient cliente, string mision, Corredor c, (string Punto, int Cruces)[] cruces) =>
        Post(cliente, "/peajes/estimacion/congelar", new
        {
            IdMision = mision,
            Cruces = cruces.Select(x => new { IdPunto = x.Punto, x.Cruces }),
            IdVehiculo = c.Vehiculo,
            CategoriaDelTipo = (string?)null,
            TipoDeVehiculo = (string?)null,
            FechaPrevista = Salida,
            Congela = "P-JEFATURA",
            Momento = Partida,
        });

    private static Task Pasar(
        HttpClient cliente, string mision, Corredor c, string punto,
        double horas, int odometro) =>
        Post(cliente, "/peajes/pasos", new
        {
            Id = Ulid.NewUlid().ToString(),
            IdPunto = punto,
            IdVehiculo = c.Vehiculo,
            IdMision = mision,
            OcurridoEn = Partida.AddHours(horas),
            Odometro = odometro,
            MontoPagado = 22m,
            Medio = "Efectivo",
            Registra = "P-MOTORISTA",
        });

    private static async Task<JsonElement> Dictamen(HttpClient cliente, string mision)
    {
        var todos = await Leer(cliente, $"/peajes/coherencia/{mision}");
        return todos.EnumerateArray().Single();
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
