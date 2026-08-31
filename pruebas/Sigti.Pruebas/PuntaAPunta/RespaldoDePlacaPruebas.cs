using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Sigti.Datos;
using Sigti.Datos.M16_Sincronizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// `RN-64` y `RN-65` — <b>circular sin lámina</b>, de punta a punta.
///
/// ── Por qué esto no es un caso raro ─────────────────────────────────────────
/// <b>Hay desabastecimiento nacional de láminas.</b> `CLAUDE.md` lo dice como premisa: «sin
/// placa metálica es un estado válido». La flota real circula así, y el documento provisional
/// que la sostiene <b>vence</b>.
///
/// ── Lo que estas pruebas fijan ──────────────────────────────────────────────
/// El bloqueo era un booleano —<c>TieneConstanciaSustitutaDePlaca</c>— que decía «hay una
/// constancia» y nada más. Una vencida a mitad de la misión pasaba <b>exactamente igual</b> que
/// una vigente. Sólo se ve cruzando el servicio: en el dominio la regla nueva está probada
/// aparte, y lo que puede volver a romperse es la conexión.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class RespaldoDePlacaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 10, 8, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public async Task Sin_lamina_y_sin_respaldo_no_se_programa()
    {
        var r = await SembrarSinLaminaAsync("PLACA-001");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var mision = await CrearYAprobar(cliente);

        var respuesta = await Programar(cliente, mision, r);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("BD-03", cuerpo);
    }

    /// <summary>
    /// ⚠️ <b>El caso que el booleano no podía ver.</b>
    ///
    /// El respaldo existe, es real, y <b>vence a mitad de la misión</b>. Con el booleano esto
    /// pasaba: había constancia. El agente que revisara el cuarto día tendría enfrente un
    /// vehículo del Estado sin lámina y sin nada que lo explique.
    /// </summary>
    [Fact]
    public async Task Un_respaldo_que_vence_a_mitad_de_la_mision_tampoco_deja_programar()
    {
        var r = await SembrarSinLaminaAsync("PLACA-002");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        // La misión sale el 20 y vuelve el 23. El respaldo cubre hasta el 21.
        await RegistrarRespaldo(cliente, r.Vehiculo, hasta: new DateOnly(2026, 3, 21));

        var mision = await CrearYAprobar(cliente);
        var respuesta = await Programar(cliente, mision, r);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("BD-03", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>La otra mitad: con el respaldo cubriendo todo el rango, sí se programa.</summary>
    [Fact]
    public async Task Con_respaldo_que_cubre_todo_el_rango_si_se_programa()
    {
        var r = await SembrarSinLaminaAsync("PLACA-003");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        // Hasta el 25: cubre la ventana del 20 al 23 más la holgura.
        await RegistrarRespaldo(cliente, r.Vehiculo, hasta: new DateOnly(2026, 3, 25));

        var mision = await CrearYAprobar(cliente);
        var respuesta = await Programar(cliente, mision, r);

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// El historial responde <b>con veredicto</b>, no con fechas sueltas.
    ///
    /// Una lista de documentos obliga a quien la mira a hacer la resta a mano, y ésa es
    /// exactamente la resta que el sistema existe para no equivocar.
    /// </summary>
    [Fact]
    public async Task El_historial_dice_de_cada_respaldo_si_cubre_la_ventana()
    {
        var r = await SembrarSinLaminaAsync("PLACA-004");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        await RegistrarRespaldo(cliente, r.Vehiculo, hasta: new DateOnly(2026, 3, 21));
        await RegistrarRespaldo(cliente, r.Vehiculo, hasta: new DateOnly(2026, 3, 30));

        var historial = await cliente.GetFromJsonAsync<JsonElement>(
            $"/flota/{r.Vehiculo}/respaldo-de-placa?salida=2026-03-20&hasta=2026-03-25");

        var filas = historial.EnumerateArray().ToList();

        Assert.Equal(2, filas.Count);

        // **Los dos existen y sólo uno cubre.** Es la distinción entera.
        Assert.Single(filas, f => f.GetProperty("cubre").GetBoolean());
        Assert.Single(filas, f => !f.GetProperty("cubre").GetBoolean());
    }

    /// <summary>
    /// El adjunto se comprueba contra la tabla. <b>El identificador de un adjunto no es el
    /// adjunto</b>: uno que apunta a nada se ve igual que uno que existe.
    /// </summary>
    [Fact]
    public async Task No_se_registra_un_respaldo_cuyo_adjunto_no_existe()
    {
        var r = await SembrarSinLaminaAsync("PLACA-005");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CrearCliente();

        var respuesta = await cliente.PostComoAsync(
            $"/flota/{r.Vehiculo}/respaldo-de-placa", new
            {
                Tipo = "Permiso provisional de circulación",
                Emisor = "Instituto de la Propiedad",
                Folio = "PP-2026-9999",
                Adjunto = Ulid.NewUlid().ToString(),
                VigenteDesde = new DateOnly(2026, 1, 1),
                VigenteHasta = new DateOnly(2026, 12, 31),
                Registra = "P-TRANSPORTE",
                Momento,
            });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("pide el papel", await respuesta.Content.ReadAsStringAsync());
    }

    // ── Andamios ────────────────────────────────────────────────────────────

    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    private async Task<FlotaSembrada.ParaProgramar> SembrarSinLaminaAsync(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        var r = await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);

        // El vehículo pierde la lámina: es el estado que `RN-65` gobierna.
        var vehiculo = await contexto.Vehiculos.SingleAsync(v => v.Id == Ulid.Parse(r.Vehiculo));
        vehiculo.EstadoDePlaca = EstadoDePlaca.NumeroAsignadoSinLamina;

        await contexto.SaveChangesAsync();
        return r;
    }

    private async Task RegistrarRespaldo(HttpClient cliente, string vehiculo, DateOnly hasta)
    {
        // Un adjunto real: el respaldo sin documento no alcanza, y esa rama tiene su prueba.
        var adjunto = Ulid.NewUlid();

        await using (var contexto = baseDePruebas.Contexto())
        {
            contexto.Adjuntos.Add(new FilaDeAdjunto
            {
                Id = adjunto,
                IdTransicion = null,
                Ruta = $"placas/{adjunto}.pdf",
                Hash = "sha256:" + new string('0', 64),
                Tipo = "application/pdf",
                Bytes = 512,
                Clasificacion = "ADMINISTRATIVO",
                CapturadoEnUtc = Momento.UtcDateTime,
                RecibidoEnUtc = Momento.UtcDateTime,
            });

            await contexto.SaveChangesAsync();
        }

        var respuesta = await cliente.PostComoAsync(
            $"/flota/{vehiculo}/respaldo-de-placa", new
            {
                Tipo = "Permiso provisional de circulación",
                Emisor = "Instituto de la Propiedad · Registro Vehicular",
                Folio = $"PP-{Ulid.NewUlid().ToString()[^6..]}",
                Adjunto = adjunto.ToString(),
                VigenteDesde = new DateOnly(2026, 1, 1),
                VigenteHasta = hasta,
                Registra = "P-TRANSPORTE",
                Momento,
            });

        Assert.True(respuesta.IsSuccessStatusCode, await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>Del viernes 20 al lunes 23 de marzo de 2026.</summary>
    private static async Task<string> CrearYAprobar(HttpClient cliente)
    {
        var id = Ulid.NewUlid().ToString();

        await cliente.PostComoAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de Choluteca",
            ObjetoDelTraslado = "Traslado de equipo",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 20),
            Retorno = new DateOnly(2026, 3, 23),
            HoraDeSalida = "07:00",
            HoraDeRetorno = "17:00",
            HolguraDias = 1,
            Momento,
        });

        await cliente.PostComoAsync($"/misiones/{id}/enviar",
            new { Ejecuta = "P-ASISTENTE", Momento });

        await cliente.PostComoAsync($"/misiones/{id}/aprobar",
            new { Ejecuta = "P-JEFATURA", Momento });

        return id;
    }

    private static Task<HttpResponseMessage> Programar(
        HttpClient cliente, string mision, FlotaSembrada.ParaProgramar r) =>
        cliente.PostComoAsync($"/misiones/{mision}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });
}
