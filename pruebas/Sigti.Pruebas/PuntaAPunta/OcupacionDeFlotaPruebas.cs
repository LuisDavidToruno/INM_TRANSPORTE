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
/// La ocupación de la flota — lo que hace que elegir vehículo deje de ser adivinar.
///
/// ── Lo que estaba roto y no se veía ──────────────────────────────────────────
/// `T-08` decía <i>«aquí se reserva vehículo y motorista»</i> desde que se escribió la
/// máquina de estados, y <b>no reservaba nada</b>: la identidad del vehículo quedaba
/// dentro del texto de evidencia, en prosa. La misión se programaba, el diario quedaba
/// perfecto, y el vehículo se seguía ofreciendo libre. Nadie lo notó porque el síntoma
/// —una pantalla que no muestra la ocupación— es indistinguible de una que no la tiene.
///
/// ── Por qué la reserva vive en el diario ─────────────────────────────────────
/// P-1: el estado es la proyección del diario. Una tabla de reservas sería una segunda
/// copia con su propia forma de desincronizarse — una misión anulada cuya reserva
/// sobrevive deja un vehículo fantasma ocupado y el sistema reporta falta de flota que no
/// existe. Con la reserva en la transición, <b>liberar es no volver a tomar</b>.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class OcupacionDeFlotaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    /// <summary>La ventana de la misión: del 20 al 22, con un día de holgura.</summary>
    private const string Desde = "2026-03-18";
    private const string Hasta = "2026-03-24";

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
    public async Task Programar_ocupa_el_vehiculo_y_la_ocupacion_lo_dice()
    {
        var idVehiculo = await SembrarVehiculo("OC-0001");
        var idMision = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, idMision);

        // Antes de programar, el carril del vehículo está vacío. Se comprueba **antes** y
        // no sólo después: una prueba que sólo mira el final pasaría igual si el carril
        // hubiera estado ocupado desde siempre por otra cosa.
        Assert.Empty(await BarrasDe(cliente, idVehiculo));

        await Programar(cliente, idMision, idVehiculo);

        var barras = await BarrasDe(cliente, idVehiculo);
        var barra = Assert.Single(barras);

        Assert.Equal("2026-03-20", barra.GetProperty("desde").GetString());
        // El retorno, **inclusivo**: es un día en que el vehículo sigue tomado.
        Assert.Equal("2026-03-22", barra.GetProperty("hasta").GetString());
        Assert.Equal("Programada", barra.GetProperty("estado").GetString());
    }

    [Fact]
    public async Task El_vehiculo_que_retorna_deja_de_ocupar_sin_que_nadie_borre_una_reserva()
    {
        // **Es la prueba que justifica la decisión de diseño.** Con una tabla de reservas
        // habría que acordarse de borrar la fila en cada salida del estado; acá la reserva
        // deja de contar porque el diario siguió, y no hay nada que olvidar.
        //
        // Se recorre entero `T-08 → T-12 → T-14 → T-16` porque el punto es JUSTO ESE: el
        // vehículo sigue ocupando mientras está despachado y en ruta —está afuera—, y deja
        // de ocupar al retornar. Una prueba que sólo mirara el final no distinguiría
        // «libera al retornar» de «nunca ocupó».
        var idVehiculo = await SembrarVehiculo("OC-0002");
        var idMision = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, idMision);
        await Programar(cliente, idMision, idVehiculo);
        Assert.Single(await BarrasDe(cliente, idVehiculo));

        await Despachar(cliente, idMision, idVehiculo);
        Assert.Equal("Despachada", (await BarrasDe(cliente, idVehiculo))[0].GetProperty("estado").GetString());

        await cliente.PostAsJsonAsync($"/misiones/{idMision}/iniciar-ruta", new { Ejecuta = "P-MOTORISTA", Momento });
        Assert.Equal("EnRuta", (await BarrasDe(cliente, idVehiculo))[0].GetProperty("estado").GetString());

        var retorno = await cliente.PostAsJsonAsync(
            $"/misiones/{idMision}/retornar", new { Ejecuta = "P-MOTORISTA", Momento });
        retorno.EnsureSuccessStatusCode();

        // Retornada NO ocupa: el vehículo volvió, aunque falte liquidar.
        Assert.Empty(await BarrasDe(cliente, idVehiculo));
    }

    [Fact]
    public async Task Una_mision_fuera_de_la_ventana_no_aparece()
    {
        // Sin esto, la pantalla de una semana mostraría la ocupación de todo el año y el
        // dibujo dejaría de decir nada. El recorte va en SQL: el diario crece para
        // siempre y traerlo entero para descartar en memoria sería peor cada año.
        var idVehiculo = await SembrarVehiculo("OC-0003");
        var idMision = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, idMision);
        await Programar(cliente, idMision, idVehiculo);

        // Una semana de abril: la misión es del 20 al 22 de marzo.
        var barras = await BarrasDe(cliente, idVehiculo, "2026-04-06", "2026-04-12");

        Assert.Empty(barras);
    }

    [Fact]
    public async Task Un_rango_invertido_se_rechaza_en_vez_de_pasar_por_flota_libre()
    {
        // Devolver cero carriles ante un rango al revés haría pasar una petición mal
        // armada por «no hay nada ocupado» — que es la respuesta que lleva a asignar un
        // vehículo que ya está tomado.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.GetAsync("/flota/ocupacion?desde=2026-03-24&hasta=2026-03-18");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    private static async Task<JsonElement[]> BarrasDe(
        HttpClient cliente,
        string idVehiculo,
        string desde = Desde,
        string hasta = Hasta)
    {
        var cuerpo = await cliente.GetFromJsonAsync<JsonElement>(
            $"/flota/ocupacion?desde={desde}&hasta={hasta}");

        var carril = cuerpo.GetProperty("carriles")
            .EnumerateArray()
            .Single(c => c.GetProperty("vehiculo").GetString() == idVehiculo);

        return [.. carril.GetProperty("barras").EnumerateArray()];
    }

    /// <summary>
    /// `T-12`. <b>No manda recursos y es correcto</b>: despachar revalida sobre lo que ya
    /// se reservó en `T-08`. Volver a tomar acá dejaría dos reservas en el diario para la
    /// misma misión, y la segunda no libera a la primera.
    /// </summary>
    private static async Task Despachar(HttpClient cliente, string idMision, string idVehiculo)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{idMision}/despachar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = idVehiculo,
            IdConductor = FlotaSembrada.Conductor.ToString(),
        });

        respuesta.EnsureSuccessStatusCode();
    }

    private static async Task Programar(HttpClient cliente, string idMision, string idVehiculo)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{idMision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = idVehiculo,
            IdConductor = FlotaSembrada.Conductor.ToString(),
        });

        respuesta.EnsureSuccessStatusCode();
    }

    private async Task<string> SembrarVehiculo(string siglas)
    {
        var id = Ulid.NewUlid();

        await using var contexto = baseDePruebas.Contexto();
        await FlotaSembrada.SembrarAsync(contexto);

        contexto.Vehiculos.Add(new FilaDeVehiculo
        {
            Id = id,
            Siglas = siglas,
            Placa = null,
            TieneConstanciaSustitutaDePlaca = true,
            TipoDeVehiculo = "Pick-up",
            Clase = ClaseNormativa.Automovil,
            PesoBrutoKg = 2_800,
            CapacidadPasajeros = 5,
            LlevaRemolque = false,
            VenceMatricula = new DateOnly(2030, 12, 31),
            VencePoliza = null,
            VenceRevisionMecanica = null,
            IdentificacionInstitucionalVerificada = true,
        });

        await contexto.SaveChangesAsync();

        return id.ToString();
    }

    private static async Task CrearYAprobar(HttpClient cliente, string id)
    {
        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 20),
            Retorno = new DateOnly(2026, 3, 22),
            HolguraDias = 1,
            Momento,
        });

        await cliente.PostAsJsonAsync($"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await cliente.PostAsJsonAsync($"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });
    }
}
