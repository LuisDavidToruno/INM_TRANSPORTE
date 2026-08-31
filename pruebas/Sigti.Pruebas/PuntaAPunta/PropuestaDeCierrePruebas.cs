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
/// §7.2 — <b>la propuesta de cierre la hace el sistema</b>, de punta a punta.
///
/// ── ⚠️ El agujero que estas pruebas cierran ─────────────────────────────────
/// La detección de criterios vivía <b>en el navegador</b> y evaluaba <b>uno</b> de los trece. El
/// endpoint recibía la lista <b>del cuerpo de la petición</b>: quien llamara con la lista vacía
/// cerraba `CERRADA`, y el asiento decía que cerró limpio.
///
/// La precondición de `T-21` es <i>«no se cumple ninguno de los criterios»</i>. Una precondición
/// que declara el propio llamador no es una precondición — es un comentario.
///
/// Esto sólo se ve cruzando el servicio: en el dominio la regla está probada aparte, y lo que
/// puede volver a romperse es <b>de dónde salen los criterios</b>.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class PropuestaDeCierrePruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    /// <summary>
    /// ⚠️ <b>El expediente cierra CON HALLAZGO sin que nadie lo pida.</b>
    ///
    /// Es la prueba entera del bloque: el incidente sin desenlace lo registró otra persona en
    /// M-12, quien cierra no lo menciona, el cuerpo de la petición <b>no lleva criterios</b>, y
    /// el expediente igual queda marcado. Antes, la misma llamada cerraba limpio.
    /// </summary>
    [Fact]
    public async Task Un_incidente_sin_desenlace_cierra_el_expediente_con_hallazgo()
    {
        var r = await SembrarAsync("PROPUESTA-A");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = await HastaLiquidarAsync(cliente, r);
        await RegistrarIncidenteAsync(cliente, mision);

        var propuesta = await PropuestaAsync(cliente, mision);

        Assert.True(propuesta.GetProperty("hayHallazgo").GetBoolean());
        Assert.Equal("CerradaConHallazgo", propuesta.GetProperty("destino").GetString());

        // ⚠️ Sin criterios en el cuerpo. Quien cierra declara **qué se hizo** con el hallazgo;
        // cuál es el hallazgo lo decide el sistema.
        var cierre = await cliente.PostAsJsonAsync($"/misiones/{mision}/cerrar", new
        {
            Ejecuta = "P-GERENCIA",
            Momento,
            Justificacion = "Se remitió al expediente de investigación GA-2026-0031.",
        });

        Assert.True(cierre.IsSuccessStatusCode, await cierre.Content.ReadAsStringAsync());

        var cuerpo = await cierre.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("CerradaConHallazgo", cuerpo.GetProperty("estado").GetString());

        var criterio = cuerpo.GetProperty("criterios").EnumerateArray()
            .Single(c => c.GetProperty("criterio").GetString() == "H-06");

        // El caso concreto y no «hay un incidente»: un hallazgo sin el hecho que lo produjo no
        // se puede seguir, y seguirlo es para lo que existe el estado.
        Assert.Contains("AveriaMecanica", criterio.GetProperty("detalle").GetString());
    }

    /// <summary>
    /// ⚠️ <b>Y no se puede pedir que cierre limpio.</b>
    ///
    /// Mandar la lista vacía era el bypass: el cuerpo declaraba la precondición de `T-21`. Hoy
    /// el campo no existe, y aunque llegue, el servidor evalúa por su cuenta.
    /// </summary>
    [Fact]
    public async Task Mandar_criterios_vacios_ya_no_cierra_limpio()
    {
        var r = await SembrarAsync("PROPUESTA-B");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = await HastaLiquidarAsync(cliente, r);
        await RegistrarIncidenteAsync(cliente, mision);

        var cierre = await cliente.PostAsJsonAsync($"/misiones/{mision}/cerrar", new
        {
            Ejecuta = "P-GERENCIA",
            Momento,

            // Lo que antes bastaba para cerrar limpio un expediente con hallazgo.
            Criterios = Array.Empty<object>(),
            Justificacion = "Se remitió al expediente de investigación GA-2026-0032.",
        });

        Assert.True(cierre.IsSuccessStatusCode, await cierre.Content.ReadAsStringAsync());

        var cuerpo = await cierre.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("CerradaConHallazgo", cuerpo.GetProperty("estado").GetString());

        // Y quedó en el diario como `T-22`, no como `T-21`: el destino lo decidió el criterio.
        await using var contexto = baseDePruebas.Contexto();

        var ultima = await contexto.Expedientes
            .Where(e => e.Id == Ulid.Parse(mision))
            .SelectMany(e => e.Transiciones)
            .OrderByDescending(t => t.Orden)
            .FirstAsync();

        Assert.Equal("T-22", ultima.Transicion);
    }

    /// <summary>
    /// Un expediente sin nada que reprochar cierra limpio — y <b>declara igual qué no se
    /// verificó</b>.
    ///
    /// Es justo acá donde ocultarlo haría creer lo que no es: «cerrada» sobre trece criterios de
    /// los que se miraron cuatro parece un expediente revisado, y no lo está.
    /// </summary>
    [Fact]
    public async Task El_expediente_limpio_cierra_y_declara_lo_que_no_se_verifico()
    {
        var r = await SembrarAsync("PROPUESTA-C");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = await HastaLiquidarAsync(cliente, r);

        var propuesta = await PropuestaAsync(cliente, mision);

        Assert.False(propuesta.GetProperty("hayHallazgo").GetBoolean());

        // Los trece de §7.2 aparecen. Lo que falta se ve porque está.
        Assert.Equal(13, propuesta.GetProperty("criterios").GetArrayLength());
        Assert.True(propuesta.GetProperty("sinVerificar").GetInt32() > 0);

        // Y cada uno dice **qué le falta**: un «no verificado» sin motivo es un hueco que nadie
        // va a poder cerrar porque nadie va a saber qué le falta.
        Assert.All(
            propuesta.GetProperty("criterios").EnumerateArray()
                .Where(c => c.GetProperty("resultado").GetString() == "NoVerificado"),
            c => Assert.False(
                string.IsNullOrWhiteSpace(c.GetProperty("detalle").GetString())));

        var cierre = await cliente.PostAsJsonAsync($"/misiones/{mision}/cerrar", new
        {
            Ejecuta = "P-GERENCIA",
            Momento,
            Justificacion = (string?)null,
        });

        var cuerpo = await cierre.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("Cerrada", cuerpo.GetProperty("estado").GetString());

        // ⚠️ **Lo que no se verificó viaja en la respuesta del cierre.** Es lo que quien cierra
        // acaba de firmar, y verlo después del acto es cuando se descubre que se firmó otra cosa.
        Assert.NotEmpty(cuerpo.GetProperty("sinVerificar").EnumerateArray());
    }

    /// <summary>
    /// `RN-08` — la cadena viaja con la propuesta, <b>eslabón por eslabón y con su fundamento</b>.
    ///
    /// La regla lo manda: <i>«el sistema presenta al liquidador una lista de verificación de la
    /// cadena, eslabón por eslabón, con su estado: presente, ausente, o no aplicable con
    /// fundamento»</i>.
    /// </summary>
    [Fact]
    public async Task La_cadena_viaja_con_la_propuesta_y_cada_eslabon_dice_por_que()
    {
        var r = await SembrarAsync("PROPUESTA-D");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var mision = await HastaLiquidarAsync(cliente, r);

        var cadena = (await PropuestaAsync(cliente, mision)).GetProperty("cadena");

        var eslabones = cadena.GetProperty("eslabones").EnumerateArray().ToList();

        Assert.Equal(8, eslabones.Count);

        // Cada uno con su detalle: en los no aplicables ése es el **fundamento** que `RN-08`
        // exige, y sin él «no aplica» es indistinguible de una omisión.
        Assert.All(eslabones,
            e => Assert.False(string.IsNullOrWhiteSpace(e.GetProperty("detalle").GetString())));

        // ⚠️ La misión no movió combustible ni cruzó peajes: **no aplican, y no se dan por
        // cumplidos**. Marcarlos presentes con consumo cero es lo que `RN-08` prohíbe literal.
        var combustible = eslabones.Single(e => e.GetProperty("eslabon").GetString() == "Combustible");

        Assert.Equal("NoAplicable", combustible.GetProperty("estado").GetString());
        Assert.Contains("no se da por cumplido", combustible.GetProperty("detalle").GetString());

        // Y la cadena queda completa: un no aplicable con fundamento no la rompe.
        Assert.True(cadena.GetProperty("completa").GetBoolean());
    }

    // ── Andamios ────────────────────────────────────────────────────────────

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

    private async Task<FlotaSembrada.ParaProgramar> SembrarAsync(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
    }

    private static async Task<JsonElement> PropuestaAsync(HttpClient cliente, string mision) =>
        await cliente.GetFromJsonAsync<JsonElement>($"/misiones/{mision}/propuesta-de-cierre");

    /// <summary>
    /// La misión completa hasta `LIQUIDADA`, en franja hábil para que `H-05` no se dispare por
    /// el camino — lo que esta prueba mide es de dónde salen los criterios, no cuáles.
    /// </summary>
    private static async Task<string> HastaLiquidarAsync(
        HttpClient cliente, FlotaSembrada.ParaProgramar r)
    {
        var id = Ulid.NewUlid().ToString();

        // Del martes 17 al jueves 19 de marzo de 2026: ningún día inhábil.
        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de equipo de cómputo",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 17),
            Retorno = new DateOnly(2026, 3, 19),
            HoraDeSalida = "07:00",
            HoraDeRetorno = "17:00",
            HolguraDias = 0,
            Momento,
        });

        await cliente.PostAsJsonAsync($"/misiones/{id}/enviar",
            new { Ejecuta = "P-ASISTENTE", Momento });

        await cliente.PostAsJsonAsync($"/misiones/{id}/aprobar",
            new { Ejecuta = "P-JEFATURA", Momento });

        var programada = await cliente.PostAsJsonAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        Assert.True(programada.IsSuccessStatusCode, await programada.Content.ReadAsStringAsync());

        var despachada = await cliente.PostAsJsonAsync($"/misiones/{id}/despachar", new
        {
            Ejecuta = "P-DESPACHO",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        Assert.True(despachada.IsSuccessStatusCode, await despachada.Content.ReadAsStringAsync());

        await cliente.PostAsJsonAsync($"/misiones/{id}/iniciar-ruta",
            new { Ejecuta = "P-DESPACHO", Momento, Odometro = 10_000 });

        await cliente.PostAsJsonAsync($"/misiones/{id}/retornar",
            new { Ejecuta = "P-DESPACHO", Momento, Odometro = 10_450 });

        var liquidada = await cliente.PostAsJsonAsync($"/misiones/{id}/liquidar",
            new { Ejecuta = "P-TRANSPORTE", Momento });

        Assert.True(liquidada.IsSuccessStatusCode, await liquidada.Content.ReadAsStringAsync());

        return id;
    }

    /// <summary>
    /// Un incidente de la misión <b>sin resolver</b> — el que dispara `H-06`. Lo registra
    /// alguien distinto de quien cierra, que es como pasa.
    /// </summary>
    private static async Task RegistrarIncidenteAsync(HttpClient cliente, string mision)
    {
        var hecho = new DateTimeOffset(2026, 3, 18, 11, 0, 0, TimeSpan.FromHours(-6));

        var respuesta = await cliente.PostAsJsonAsync("/incidentes", new
        {
            Id = Ulid.NewUlid().ToString(),
            Tipo = "AveriaMecanica",
            Causa = "Falla de transmisión",
            MomentoDelHecho = hecho,
            MomentoDeCaptura = hecho.AddHours(2),
            Descripcion = "El vehículo quedó en el km 61 sin poder avanzar.",
            Registra = "P-MOTORISTA",
            ResponsableDeSeguimiento = "P-TRANSPORTE",
            Plazo = new DateOnly(2026, 3, 25),
            Interrumpe = false,
            IdMision = mision,
            IdVehiculo = (string?)null,
            Ubicacion = "km 61, CA-5",
            Odometro = (int?)null,
            Bienes = Array.Empty<object>(),
        });

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());
    }
}
