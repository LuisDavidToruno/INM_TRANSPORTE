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
/// El estado operativo declarado por una persona — §10.2, y con él `BD-07` empieza a bloquear
/// de verdad.
///
/// ── Lo que faltaba, y por qué el bloqueo era inerte ──────────────────────────
/// El estado sólo se movía <b>solo</b>, por transiciones de la misión. Ningún vehículo llegaba
/// nunca a `EN_TALLER`, así que `BD-07` existía sin poder bloquear nada: una regla cuya
/// condición de bloqueo es inalcanzable es una regla que no corre.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class EstadoDelVehiculoPruebas(BaseDePruebas baseDePruebas)
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
    public async Task Un_vehiculo_EN_TALLER_no_se_programa()
    {
        // **El caso que `BD-07` no podía alcanzar hasta hoy.**
        var r = await Sembrar("EV-0001");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Habilitar(cliente, r.Vehiculo);
        await Declarar(cliente, r.Vehiculo, "EnTaller", "Orden de trabajo 2026-0044, frenos");

        await CrearYAprobar(cliente, id);

        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);

        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        Assert.Contains("BD-07", cuerpo);
        // Dice en cuál está: de EN_TALLER se sale esperando, y quien programa necesita saber
        // si vale la pena volver mañana.
        Assert.Contains("EnTaller", cuerpo);
    }

    [Fact]
    public async Task Volver_a_DISPONIBLE_destraba_la_programacion()
    {
        // El recíproco. Sin él, `BD-07` podría estar bloqueando toda programación y la prueba
        // anterior seguiría en verde.
        var r = await Sembrar("EV-0002");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Habilitar(cliente, r.Vehiculo);
        await Declarar(cliente, r.Vehiculo, "EnTaller", "Orden de trabajo 2026-0045");
        await Declarar(cliente, r.Vehiculo, "Disponible", "Trabajo cerrado, revisión conforme");

        await CrearYAprobar(cliente, id);

        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        respuesta.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ASIGNADO_no_se_declara_a_mano()
    {
        // §10.2 sin margen: «los fija el sistema, no una persona. Permitir fijarlos a mano abre
        // la puerta a un vehículo "en misión" sin misión».
        var r = await Sembrar("EV-0003");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync($"/flota/{r.Vehiculo}/estado", new
        {
            Ejecuta = "P-TRANSPORTE",
            Estado = "Asignado",
            Momento,
            Motivo = "Lo quiero asignado",
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("sin misión que lo respalde", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Un_vehiculo_con_misiones_abiertas_no_se_da_de_baja()
    {
        // §10.2: «todas deben estar en estado terminal». Un expediente vivo colgando de un bien
        // que ya no figura en el registro es un hallazgo que nadie puede explicar después.
        var r = await Sembrar("EV-0004");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);
        await Programar(cliente, id, r);

        var respuesta = await cliente.PostAsJsonAsync($"/flota/{r.Vehiculo}/estado", new
        {
            Ejecuta = "P-GERENCIA",
            Estado = "DadoDeBaja",
            Momento,
            Motivo = "Acta de descargo 2026-11",
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("sin cerrar", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task De_un_estado_terminal_no_se_sale()
    {
        var r = await Sembrar("EV-0005");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        // `W-18` — fin de tenencia. Sólo se alcanza desde `NO_DISPONIBLE`: el vehículo se
        // inhabilita primero y después se devuelve, que es el orden real de los actos.
        await Declarar(cliente, r.Vehiculo, "NoDisponible", "Fin del comodato, pendiente de devolver");
        await Declarar(cliente, r.Vehiculo, "RetiradoDeFlota", "Acta de devolución de comodato 2026-03");

        var respuesta = await cliente.PostAsJsonAsync($"/flota/{r.Vehiculo}/estado", new
        {
            Ejecuta = "P-GERENCIA",
            Estado = "Disponible",
            Momento,
            Motivo = "Me equivoqué",
        });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        // Y distingue de qué terminal se trata: revertir un descargo es un trámite del
        // registro de bienes; una devolución de comodato ni siquiera es nuestra para revertir.
        Assert.Contains("ya no está bajo tenencia", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Sin_motivo_no_se_declara()
    {
        var r = await Sembrar("EV-0006");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync($"/flota/{r.Vehiculo}/estado", new
        {
            Ejecuta = "P-TRANSPORTE",
            Estado = "NoDisponible",
            Momento,
            Motivo = "   ",
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task El_historial_conserva_por_que_no_estuvo_disponible()
    {
        // «¿Por qué no estuvo disponible en abril, y quién lo decidió?» es la pregunta real, y
        // el estado actual no la contesta.
        var r = await Sembrar("EV-0007");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await Habilitar(cliente, r.Vehiculo);
        await Declarar(cliente, r.Vehiculo, "EnTaller", "Orden 2026-0050, transmisión");
        await Declarar(cliente, r.Vehiculo, "Disponible", "Trabajo cerrado");

        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/flota/{r.Vehiculo}/estado");

        Assert.Equal("Disponible", estado.GetProperty("actual").GetString());

        var historial = estado.GetProperty("historial").EnumerateArray().ToList();

        // Cuatro: los dos del alta —`W-01` y `W-02`— más el taller y la vuelta. **El historial
        // conserva el camino entero**, que es lo que contesta «¿por qué no estuvo disponible?».
        Assert.Equal(4, historial.Count);
        Assert.Contains("transmisión", historial[2].GetProperty("motivo").GetString()!);
        // Y distingue lo declarado de lo automático: §10.2 dice que ASIGNADO y EN_MISION los
        // fija el sistema, y sin esta marca esa afirmación no se puede auditar.
        Assert.False(historial[2].GetProperty("automatico").GetBoolean());
    }

    [Fact]
    public async Task Programar_deja_el_vehiculo_ASIGNADO_por_el_sistema()
    {
        // La otra cara: lo que sí fija el sistema, y queda marcado como tal.
        var r = await Sembrar("EV-0008");
        var id = Ulid.NewUlid().ToString();

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);
        await Programar(cliente, id, r);

        var estado = await cliente.GetFromJsonAsync<JsonElement>($"/flota/{r.Vehiculo}/estado");

        Assert.Equal("Asignado", estado.GetProperty("actual").GetString());

        var ultimo = estado.GetProperty("historial").EnumerateArray().Last();
        Assert.True(ultimo.GetProperty("automatico").GetBoolean());
        Assert.Contains("T-08", ultimo.GetProperty("motivo").GetString()!);
    }

    /// <summary>
    /// Lleva el vehículo recién sembrado a `DISPONIBLE` por el <b>camino legal</b> de §10.2:
    /// `W-01` alta en flota → `W-02` habilitar.
    ///
    /// ── Antes estas pruebas saltaban directo al estado que querían ──────────
    /// Y pasaban, porque el endpoint no validaba la transición. Al transcribir la tabla `W-xx`
    /// empezaron a fallar **con razón**: un vehículo no llega a `EN_TALLER` sin haber existido
    /// antes en la flota. Recorrer el camino es además lo que hace el alta real.
    /// </summary>
    private static async Task Habilitar(HttpClient cliente, string vehiculo)
    {
        await Declarar(cliente, vehiculo, "NoDisponible", "Alta en flota, pendiente de habilitar");
        await Declarar(cliente, vehiculo, "Disponible", "Documentación verificada y custodio asignado");
    }

    private static async Task Declarar(HttpClient cliente, string vehiculo, string estado, string motivo)
    {
        var r = await cliente.PostAsJsonAsync($"/flota/{vehiculo}/estado", new
        {
            Ejecuta = "P-MANTENIMIENTO",
            Estado = estado,
            Momento,
            Motivo = motivo,
        });

        r.EnsureSuccessStatusCode();
    }

    private static async Task Programar(HttpClient cliente, string id, FlotaSembrada.ParaProgramar r)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = r.Vehiculo,
            IdConductor = r.Conductor,
        });

        respuesta.EnsureSuccessStatusCode();
    }

    private async Task<FlotaSembrada.ParaProgramar> Sembrar(string prefijo)
    {
        await using var contexto = baseDePruebas.Contexto();
        return await FlotaSembrada.ParaProgramarAsync(contexto, prefijo);
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
            Salida = new DateOnly(2026, 3, 16),
            Retorno = new DateOnly(2026, 3, 18),
            HoraDeSalida = new TimeOnly(8, 0),
            HoraDeRetorno = new TimeOnly(16, 0),
            HolguraDias = 0,
            Momento,
        });

        await cliente.PostAsJsonAsync($"/misiones/{id}/enviar", new { Ejecuta = "P-ASISTENTE", Momento });
        await cliente.PostAsJsonAsync($"/misiones/{id}/aprobar", new { Ejecuta = "P-JEFATURA", Momento });
    }
}
