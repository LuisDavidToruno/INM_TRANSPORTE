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
/// `PT-038` — el tablero de despacho del día.
///
/// ── Las cuatro preguntas, y por qué no son una tabla ordenable ───────────────
/// <b>Qué sale hoy</b>, <b>qué vuelve hoy</b>, <b>qué está afuera</b> y <b>qué debía haber
/// vuelto y no volvió</b>. Cuatro acciones distintas con cuatro urgencias distintas.
///
/// La cuarta es la que ninguna lista ordenada por fecha muestra sola: <b>un retorno vencido
/// no aparece «arriba», aparece en el pasado</b>, donde nadie mira. El dictamen de elementos
/// visuales llamó a esta pantalla el error de mayor daño del inventario justamente por eso:
/// se declaró «completa» y se maquetó como lista.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class TableroDeDespachoPruebas(BaseDePruebas baseDePruebas)
{
    /// <summary>Jueves 12 de marzo de 2026, el mismo momento del resto de las pruebas.</summary>
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
    public async Task Una_mision_programada_que_sale_hoy_aparece_en_salen_hoy()
    {
        var r = await Sembrar("TD-0001");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        // Del lunes 16 al miércoles 18 — entre semana, para no chocar con `BD-04`.
        await CrearAprobarYProgramar(cliente, id, r, new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18));

        var tablero = await Tablero(cliente, "2026-03-16");

        var mision = Assert.Single(Lista(tablero, "salenHoy"), m => m.GetProperty("mision").GetString() == id);

        Assert.Equal("Programada", mision.GetProperty("estado").GetString());
        // Los nombres, no los identificadores: el despachador entrega el vehículo, no un ULID.
        Assert.Contains("TD-0001", mision.GetProperty("vehiculo").GetString()!);
        Assert.False(string.IsNullOrWhiteSpace(mision.GetProperty("motorista").GetString()));
    }

    [Fact]
    public async Task Una_mision_que_debia_salir_ayer_y_no_salio_SIGUE_en_salen_hoy()
    {
        // **El caso que decide si el tablero sirve.** Con `==` en vez de `<=`, la misión que
        // no salió desaparece del tablero al día siguiente — que es exactamente cuando hay
        // que ir a buscarla.
        var r = await Sembrar("TD-0002");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearAprobarYProgramar(cliente, id, r, new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18));

        // Dos días después de la salida prevista, y sigue PROGRAMADA.
        var tablero = await Tablero(cliente, "2026-03-18");

        Assert.Contains(Lista(tablero, "salenHoy"), m => m.GetProperty("mision").GetString() == id);
    }

    [Fact]
    public async Task Un_retorno_vencido_va_a_ATRASADAS_y_no_a_las_que_vuelven_hoy()
    {
        // **La lista que ninguna tabla ordenada por fecha muestra.** Un retorno vencido no
        // aparece «arriba»: aparece en el pasado. Y contarla también en «vuelven hoy» la
        // duplicaría, escondiendo la urgencia real.
        var r = await Sembrar("TD-0003");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearAprobarYProgramar(cliente, id, r, new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18));
        await Despachar(cliente, id, r);

        // Cinco días después del retorno previsto.
        var tablero = await Tablero(cliente, "2026-03-23");

        var mision = Assert.Single(Lista(tablero, "atrasadas"), m => m.GetProperty("mision").GetString() == id);
        Assert.Equal(5, mision.GetProperty("diasDeAtraso").GetInt32());

        // Y NO está en las otras dos: una misión atrasada se cuenta una vez.
        Assert.DoesNotContain(Lista(tablero, "vuelvenHoy"), m => m.GetProperty("mision").GetString() == id);
        Assert.DoesNotContain(Lista(tablero, "afuera"), m => m.GetProperty("mision").GetString() == id);
    }

    [Fact]
    public async Task Una_despachada_que_vuelve_hoy_esta_en_vuelven_hoy_y_no_en_afuera()
    {
        var r = await Sembrar("TD-0004");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearAprobarYProgramar(cliente, id, r, new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18));
        await Despachar(cliente, id, r);

        var tablero = await Tablero(cliente, "2026-03-18");

        Assert.Contains(Lista(tablero, "vuelvenHoy"), m => m.GetProperty("mision").GetString() == id);
        Assert.DoesNotContain(Lista(tablero, "afuera"), m => m.GetProperty("mision").GetString() == id);
    }

    [Fact]
    public async Task Una_despachada_que_vuelve_despues_esta_AFUERA()
    {
        // «Afuera» no es una alarma: es el vehículo con el que hoy no se puede contar. Sin
        // esta lista, el despachador sólo ve lo que le entra y lo que le sale, y no lo que
        // le falta.
        var r = await Sembrar("TD-0005");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearAprobarYProgramar(cliente, id, r, new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 18));
        await Despachar(cliente, id, r);

        var tablero = await Tablero(cliente, "2026-03-17");

        var mision = Assert.Single(Lista(tablero, "afuera"), m => m.GetProperty("mision").GetString() == id);
        Assert.Equal(0, mision.GetProperty("diasDeAtraso").GetInt32());
    }

    [Fact]
    public async Task Una_mision_que_sale_la_semana_que_viene_NO_esta_en_el_tablero_de_hoy()
    {
        // El recíproco, y hace falta: sin él, el tablero podría estar mostrando todo lo vivo
        // y las otras pruebas seguirían en verde. Lo que no es de hoy es de la cola de
        // programación, no del despachador.
        var r = await Sembrar("TD-0006");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearAprobarYProgramar(cliente, id, r, new DateOnly(2026, 3, 24), new DateOnly(2026, 3, 25));

        var tablero = await Tablero(cliente, "2026-03-16");

        foreach (var lista in new[] { "salenHoy", "vuelvenHoy", "afuera", "atrasadas" })
            Assert.DoesNotContain(Lista(tablero, lista), m => m.GetProperty("mision").GetString() == id);
    }

    [Fact]
    public async Task Una_fecha_mal_formada_se_explica_en_vez_de_reventar()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.GetAsync("/despacho/dia?fecha=el-jueves");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    private static JsonElement[] Lista(JsonElement tablero, string nombre) =>
        [.. tablero.GetProperty(nombre).EnumerateArray()];

    private static async Task<JsonElement> Tablero(HttpClient cliente, string fecha) =>
        await cliente.GetFromJsonAsync<JsonElement>($"/despacho/dia?fecha={fecha}");

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    private static async Task Despachar(HttpClient cliente, string id, FlotaSembrada.ParaProgramar r)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/despachar", new
        {
            Ejecuta = "P-ENCARGADO",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        respuesta.EnsureSuccessStatusCode();
    }

    private static async Task CrearAprobarYProgramar(
        HttpClient cliente,
        string id,
        FlotaSembrada.ParaProgramar r,
        DateOnly salida,
        DateOnly retorno)
    {
        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = salida,
            Retorno = retorno,
            HolguraDias = 0,
            Momento,
        });

        await cliente.PostAsJsonAsync($"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await cliente.PostAsJsonAsync($"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });

        var programacion = await cliente.PostAsJsonAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        programacion.EnsureSuccessStatusCode();
    }
}
