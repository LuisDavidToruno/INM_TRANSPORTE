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
/// `RN-60` cableada — la indisponibilidad sobrevenida y sus reservas en conflicto.
///
/// ── Lo que esta prueba demuestra ────────────────────────────────────────────
/// Que <b>la marca de conflicto impide el despacho de verdad</b>. Hasta que M-11 existió, un
/// vehículo podía irse al taller con misiones programadas encima y el despacho seguía saliendo.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class IndisponibilidadPruebas(BaseDePruebas baseDePruebas)
{
    private WebApplicationFactory<Program> Aplicacion() => FabricaDeSigti.Crear(baseDePruebas);

    /// <summary>
    /// <b>El circuito entero.</b> Se programa una misión, el vehículo se va al taller con acuse
    /// sobre esa reserva, el despacho se bloquea, y recién con el desenlace registrado vuelve a
    /// salir.
    ///
    /// `RN-60`: <i>«una reserva en conflicto no expira en silencio ni se resuelve por el paso del
    /// tiempo»</i>.
    /// </summary>
    [Fact]
    public async Task La_reserva_en_conflicto_IMPIDE_el_despacho_hasta_su_desenlace()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vehiculo, conductor, salida) = await MisionProgramadaAsync(cliente);

        // ── El sistema muestra qué queda en el aire, antes de acusar ─────────
        var afectadas = await Leer(cliente,
            $"/indisponibilidades/reservas-afectadas/{vehiculo}/{salida:yyyy-MM-dd}" +
            $"/{salida.AddDays(10):yyyy-MM-dd}");

        var reserva = Assert.Single(afectadas.EnumerateArray(),
            r => r.GetProperty("mision").GetString() == mision);

        // `RN-60` punto 1 — folio, dependencia, ventana, motorista y objeto.
        Assert.Equal("Programada", reserva.GetProperty("estadoAlAcusar").GetString());
        Assert.False(string.IsNullOrWhiteSpace(reserva.GetProperty("dependencia").GetString()));

        // ── Se declara la indisponibilidad, con el acuse ─────────────────────
        var indisponibilidad = Ulid.NewUlid().ToString();

        await Post(cliente, "/indisponibilidades", new
        {
            Id = indisponibilidad,
            IdVehiculo = vehiculo,
            Estado = "EnTaller",
            Causa = "Cambio de embrague",
            Desde = salida.AddDays(-1),
            FinEstimado = salida.AddDays(9),
            Ejecuta = "P-TRANSPORTE",
            MomentoDelAcuse = DateTimeOffset.UtcNow,
        });

        // ── Y el despacho se bloquea ────────────────────────────────────────
        var bloqueado = await Despachar(cliente, mision, vehiculo, conductor);

        Assert.False(bloqueado.IsSuccessStatusCode);

        var mensaje = await bloqueado.Content.ReadAsStringAsync();
        Assert.Contains("marcada en conflicto", mensaje);
        Assert.Contains("Cambio de embrague", mensaje);
        Assert.Contains("no el paso del tiempo", mensaje);

        // ── Con el desenlace registrado, vuelve a salir ──────────────────────
        await Post(cliente, $"/indisponibilidades/{indisponibilidad}/reservas/{mision}/resolver",
            new
            {
                Desenlace = "LevantarLaIndisponibilidad",
                Ejecuta = "P-TRANSPORTE",
                Motivo = "El repuesto llegó antes y la unidad salió del taller el mismo día.",
                Momento = DateTimeOffset.UtcNow,
            });

        var despachado = await Despachar(cliente, mision, vehiculo, conductor);

        Assert.True(despachado.IsSuccessStatusCode,
            await despachado.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// `RN-60` punto 2 — <b>la lista se conserva exactamente como se presentó</b>, con su marca
    /// de tiempo, y no se reconstruye después.
    ///
    /// Se verifica anulando la misión después del acuse: la reserva sigue en el expediente con
    /// el estado que tenía al acusar. Si se reconstruyera, habría desaparecido — y quien acusó
    /// habría acusado sobre una lista que ya no consta.
    /// </summary>
    [Fact]
    public async Task La_lista_acusada_se_conserva_aunque_la_mision_cambie_despues()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var (mision, vehiculo, conductor, salida) = await MisionProgramadaAsync(cliente);

        var indisponibilidad = Ulid.NewUlid().ToString();

        await Post(cliente, "/indisponibilidades", new
        {
            Id = indisponibilidad,
            IdVehiculo = vehiculo,
            Estado = "NoDisponible",
            Causa = "Siniestro en predio institucional",
            Desde = salida.AddDays(-1),
            FinEstimado = salida.AddDays(9),
            Ejecuta = "P-TRANSPORTE",
            MomentoDelAcuse = DateTimeOffset.UtcNow,
        });

        // ── La misión se reasigna a otro vehículo después del acuse ──────────
        // `T-10`. Es lo que mejor prueba el punto: si la lista se reconstruyera, esta reserva
        // desaparecería —la misión ya no usa el vehículo indisponible— y quien acusó habría
        // acusado sobre una lista que ya no consta.
        FlotaSembrada.ParaProgramar otra;

        await using (var contexto = baseDePruebas.Contexto())
            otra = await FlotaSembrada.ParaProgramarAsync(contexto, "IN2");

        await Post(cliente, $"/misiones/{mision}/reasignar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento = new DateTimeOffset(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6)),
            IdVehiculo = otra.Vehiculo,
            IdConductor = otra.Conductor,
            Motivo = "VehiculoATaller",
            Comentario = "La unidad original quedó en taller.",
        });

        var expediente = Assert.Single(
            (await Leer(cliente, "/indisponibilidades")).EnumerateArray(),
            i => i.GetProperty("id").GetString() == indisponibilidad);

        // **Sigue ahí, con el estado que tenía al acusar.** No se reconstruyó.
        var conservada = Assert.Single(expediente.GetProperty("reservas").EnumerateArray(),
            r => r.GetProperty("mision").GetString() == mision);

        Assert.Equal("Programada", conservada.GetProperty("estadoAlAcusar").GetString());

        // Y sigue sin desenlace: anular la misión no registra el desenlace por sí solo.
        Assert.Single(expediente.GetProperty("sinDesenlace").EnumerateArray());
    }

    /// <summary>
    /// `RN-60` punto 6 — el alta con <b>fecha real, orden de trabajo y odómetro</b>, contrastada
    /// contra la ventana estimada. <i>«La desviación sistemática entre estimado y real es
    /// indicador de la gestión del taller»</i>.
    /// </summary>
    [Fact]
    public async Task El_alta_exige_orden_de_trabajo_y_mide_la_desviacion()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var vehiculo = Ulid.NewUlid().ToString();
        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/indisponibilidades", new
        {
            Id = id,
            IdVehiculo = vehiculo,
            Estado = "EnTaller",
            Causa = "Mantenimiento preventivo de 40 mil km",
            Desde = new DateOnly(2026, 5, 11),
            FinEstimado = new DateOnly(2026, 5, 15),
            Ejecuta = "P-TRANSPORTE",
            MomentoDelAcuse = DateTimeOffset.UtcNow,
        });

        var sinOrden = await cliente.PostAsJsonAsync($"/indisponibilidades/{id}/alta", new
        {
            FinReal = new DateOnly(2026, 5, 18),
            OrdenDeTrabajo = "",
            OdometroDeSalida = 94_300,
        });

        Assert.False(sinOrden.IsSuccessStatusCode);
        Assert.Contains("sin que conste qué se le hizo", await sinOrden.Content.ReadAsStringAsync());

        await Post(cliente, $"/indisponibilidades/{id}/alta", new
        {
            FinReal = new DateOnly(2026, 5, 18),
            OrdenDeTrabajo = "OT-2026-114",
            OdometroDeSalida = 94_300,
        });

        var expediente = Assert.Single(
            (await Leer(cliente, "/indisponibilidades")).EnumerateArray(),
            i => i.GetProperty("id").GetString() == id);

        Assert.False(expediente.GetProperty("estaVigente").GetBoolean());

        // Tres días más de lo estimado: es el indicador de gestión del taller.
        Assert.Equal(3, expediente.GetProperty("desviacionEnDias").GetInt32());
    }

    /// <summary>
    /// Dos ventanas abiertas sobre la misma unidad dejarían a las reservas en conflicto sin saber
    /// a cuál responden.
    /// </summary>
    [Fact]
    public async Task Un_vehiculo_no_se_declara_indisponible_dos_veces()
    {
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var vehiculo = Ulid.NewUlid().ToString();

        await Post(cliente, "/indisponibilidades", Cuerpo(vehiculo));

        var segunda = await cliente.PostAsJsonAsync("/indisponibilidades", Cuerpo(vehiculo));

        Assert.False(segunda.IsSuccessStatusCode);
        Assert.Contains("ya está indisponible", await segunda.Content.ReadAsStringAsync());
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static object Cuerpo(string vehiculo) => new
    {
        Id = Ulid.NewUlid().ToString(),
        IdVehiculo = vehiculo,
        Estado = "EnTaller",
        Causa = "Cambio de embrague",
        Desde = new DateOnly(2026, 5, 11),
        FinEstimado = new DateOnly(2026, 5, 25),
        Ejecuta = "P-TRANSPORTE",
        MomentoDelAcuse = DateTimeOffset.UtcNow,
    };

    /// <summary>
    /// Una misión programada con vehículo y motorista reales. Reutiliza el mismo camino que
    /// las demás pruebas de punta a punta: la reserva vive en el diario.
    /// </summary>
    private async Task<(string Mision, string Vehiculo, string Conductor, DateOnly Salida)>
        MisionProgramadaAsync(
        HttpClient cliente)
    {
        FlotaSembrada.ParaProgramar flota;

        await using (var contexto = baseDePruebas.Contexto())
            flota = await FlotaSembrada.ParaProgramarAsync(contexto, "IND");

        // Un lunes de marzo de 2026, dentro de la vigencia de la licencia sembrada y sin días
        // inhábiles: `BD-02` y `BD-04` no son lo que esta prueba juzga.
        var salida = new DateOnly(2026, 3, 16);
        var momento = new DateTimeOffset(2026, 3, 10, 9, 0, 0, TimeSpan.FromHours(-6));

        var id = Ulid.NewUlid().ToString();

        await Post(cliente, "/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-ASISTENTE",
            Dependencia = "Delegacion de prueba RN-60",
            ObjetoDelTraslado = "Traslado de personal",
            Destino = "Choluteca",
            Salida = salida,
            Retorno = salida.AddDays(2),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 0,
            Momento = momento,
        });

        await Post(cliente, $"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento = momento });
        await Post(cliente, $"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento = momento });

        await Post(cliente, $"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento = momento,
            IdVehiculo = flota.Vehiculo,
            IdConductor = flota.Conductor,
        });

        return (id, flota.Vehiculo, flota.Conductor, salida);
    }

    private static Task<HttpResponseMessage> Despachar(
        HttpClient cliente, string mision, string vehiculo, string conductor) =>
        cliente.PostAsJsonAsync($"/misiones/{mision}/despachar", new
        {
            Ejecuta = "P-DESPACHO",
            Momento = new DateTimeOffset(2026, 3, 16, 7, 30, 0, TimeSpan.FromHours(-6)),
            IdVehiculo = vehiculo,
            IdConductor = conductor,
        });

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
