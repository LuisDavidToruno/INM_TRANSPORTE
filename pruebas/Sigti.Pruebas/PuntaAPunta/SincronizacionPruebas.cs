using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// El endpoint de sincronización — donde aterriza lo que el dispositivo capturó sin red.
///
/// ── Lo que estas pruebas defienden ───────────────────────────────────────────
/// `RNF-03` no dice «con soporte offline». Dice **7 días continuos sin conectividad
/// y 0 registros perdidos al sincronizar**. Del lado del servidor eso se traduce en
/// una sola propiedad: <b>reenviar es inofensivo</b>.
///
/// Y tiene que serlo, porque el dispositivo que no supo si el servidor recibió
/// **va a reenviar**. Se corta la conexión bajo un puente, el servidor cierra el
/// socket después de aplicar pero antes de acusar, la batería se agota. En todos
/// esos casos el dispositivo reintenta con el mismo lote.
///
/// Si el servidor duplicara, cada corte de red produciría una transición fantasma
/// en el diario — y el diario es de donde se reconstruye el estado (`P-1`).
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class SincronizacionPruebas(BaseDePruebas baseDePruebas)
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
    public async Task Reenviar_el_mismo_lote_no_duplica_la_transicion_en_el_diario()
    {
        var id = Ulid.NewUlid().ToString();
        await using (var siembra = baseDePruebas.Contexto()) await FlotaSembrada.SembrarAsync(siembra);
        var idDeCaptura = Ulid.NewUlid().ToString();
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await LlevarHastaDespachada(cliente, id);

        var lote = new
        {
            IdDispositivo = "DEV-TOCOA-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = idDeCaptura,
                    IdExpediente = id,
                    Transicion = "T-14",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = Momento,
                    // `BD-05`: el dispositivo captura la lectura del odómetro sin red, y el
                    // servidor la revalida al recibir contra la última conocida del vehículo.
                    Odometro = 10_000,
                },
            },
        };

        var primera = await cliente.PostAsJsonAsync("/sincronizacion", lote);
        Assert.Equal(HttpStatusCode.OK, primera.StatusCode);

        // El dispositivo no recibió el acuse y reenvía. Es el caso NORMAL, no el raro.
        var segunda = await cliente.PostAsJsonAsync("/sincronizacion", lote);
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);

        await using var contexto = baseDePruebas.Contexto();

        var transiciones = await contexto.Expedientes
            .Where(e => e.Id == Ulid.Parse(id))
            .SelectMany(e => e.Transiciones)
            .Where(t => t.Transicion == "T-14")
            .ToListAsync();

        Assert.Single(transiciones);
    }

    [Fact]
    public async Task El_acuse_dice_QUE_se_aplico_para_que_el_dispositivo_pueda_confirmar()
    {
        // Sin esto la sincronización no es reanudable: el dispositivo tiene que saber
        // exactamente qué salió de su cola de pendientes. Un «200 OK» a secas lo deja
        // adivinando, y adivinar acá significa reenviar todo o perder algo.
        var id = Ulid.NewUlid().ToString();
        await using (var siembra = baseDePruebas.Contexto()) await FlotaSembrada.SembrarAsync(siembra);
        var idDeCaptura = Ulid.NewUlid().ToString();
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await LlevarHastaDespachada(cliente, id);

        var respuesta = await cliente.PostAsJsonAsync("/sincronizacion", new
        {
            IdDispositivo = "DEV-TOCOA-01",
            Transiciones = new[]
            {
                new
                {
                    IdDeCaptura = idDeCaptura,
                    IdExpediente = id,
                    Transicion = "T-14",
                    Ejecuta = "P-MOTORISTA",
                    OcurridoEn = Momento,
                    // `BD-05`: el dispositivo captura la lectura del odómetro sin red, y el
                    // servidor la revalida al recibir contra la última conocida del vehículo.
                    Odometro = 10_000,
                },
            },
        });

        var cuerpo = await respuesta.Content.ReadAsStringAsync();

        Assert.Contains(idDeCaptura, cuerpo);
    }

    /// <summary>Deja el expediente listo para que el dispositivo registre la salida.</summary>
    private async Task LlevarHastaDespachada(HttpClient cliente, string id)
    {
        var creacion = await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegación de Tocoa",
            ObjetoDelTraslado = "Traslado de equipo",
            Destino = "Tocoa",
            Salida = new DateOnly(2026, 3, 16),
            Retorno = new DateOnly(2026, 3, 18),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 1,
            Momento,
        });
        Assert.Equal(HttpStatusCode.Created, creacion.StatusCode);

        await Paso(cliente, id, "enviar", "P-ASISTENTE");
        await Paso(cliente, id, "aprobar", "P-JEFATURA");
        await Asignar(cliente, id, "programar", "P-TRANSPORTE");
        await Asignar(cliente, id, "despachar", "P-ENCARGADO");
    }

    private static async Task Paso(HttpClient cliente, string id, string ruta, string ejecuta)
    {
        var r = await cliente.PostAsJsonAsync($"/misiones/{id}/{ruta}", new { Ejecuta = ejecuta, Momento });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    /// <summary>
    /// Asigna con recursos <b>propios de la prueba</b>. Desde `BD-11`, reutilizar el
    /// pick-up y el motorista del catálogo sembrado choca con las otras pruebas de punta a
    /// punta — y con razón: son la misma franja.
    /// </summary>
    private async Task Asignar(HttpClient cliente, string id, string ruta, string ejecuta)
    {
        _recursos ??= await ParaProgramar();

        var r = await cliente.PostAsJsonAsync($"/misiones/{id}/{ruta}", new
        {
            Ejecuta = ejecuta,
            Momento,
            IdVehiculo = _recursos.Vehiculo,
            IdConductor = _recursos.Conductor,
        });
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    /// <summary>Se reusa dentro de la MISMA misión: programar y despachar son el mismo par.</summary>
    private FlotaSembrada.ParaProgramar? _recursos;

    private async Task<FlotaSembrada.ParaProgramar> ParaProgramar()
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, "SN-0001");
    }
}
