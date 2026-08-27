using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Datos;
using Sigti.Dominio.Bitacora;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// El walking skeleton: una orden de misión de punta a punta, atravesando todas las
/// capas —API, aplicación, dominio, base de datos real y bitácora encadenada—.
///
/// No prueba un módulo completo. Prueba que el hilo camina, que es lo que valida el
/// stack antes de invertir en él.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class HiloDeMisionPruebas(BaseDePruebas baseDePruebas)
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
    public async Task Una_mision_recorre_el_hilo_completo_y_deja_su_rastro_encadenado()
    {
        var id = Ulid.NewUlid().ToString();
        await using (var siembra = baseDePruebas.Contexto()) await FlotaSembrada.SembrarAsync(siembra);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var creacion = await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-JEFE",
            Dependencia = "Delegación de Choluteca",
            ObjetoDelTraslado = "Traslado de personal y equipo",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 20),
            Retorno = new DateOnly(2026, 3, 22),
            HolguraDias = 1,
            Momento,
        });
        Assert.Equal(HttpStatusCode.Created, creacion.StatusCode);

        await Transicionar(cliente, id, "enviar", "P-ASISTENTE");

        // BD-01: el solicitante de derecho no autoriza lo suyo, aunque no lo haya
        // capturado. El bloqueo tiene que sobrevivir el viaje por la API, no quedarse
        // en el dominio.
        var bloqueada = await Transicionar(cliente, id, "aprobar", "P-JEFE", esperado: HttpStatusCode.Conflict);
        Assert.Contains("BD-01", await bloqueada.Content.ReadAsStringAsync());

        await Transicionar(cliente, id, "aprobar", "P-JEFATURA");

        // BD-02 al despachar: una licencia que vence antes del fin del rango bloquea,
        // aunque esté vigente el día de salida.
        // BD-02: la licencia B de José Ramón Cruz no habilita un camión de 12,000 kg.
        var noHabilita = await Asignar(cliente, id, "programar", "P-TRANSPORTE",
            idVehiculo: FlotaSembrada.Camion.ToString(), idConductor: FlotaSembrada.Conductor.ToString(), esperado: HttpStatusCode.Conflict);
        Assert.Contains("BD-02", await noHabilita.Content.ReadAsStringAsync());

        await Asignar(cliente, id, "programar", "P-TRANSPORTE", idVehiculo: FlotaSembrada.Pickup.ToString(), idConductor: FlotaSembrada.Conductor.ToString());
        await Asignar(cliente, id, "despachar", "P-ENCARGADO", idVehiculo: FlotaSembrada.Pickup.ToString(), idConductor: FlotaSembrada.Conductor.ToString());
        await Transicionar(cliente, id, "iniciar-ruta", "P-MOTORISTA");
        await Transicionar(cliente, id, "retornar", "P-MOTORISTA");
        var liquidada = await Transicionar(cliente, id, "liquidar", "P-TRANSPORTE");
        Assert.Contains("Liquidada", await liquidada.Content.ReadAsStringAsync());

        // `BD-06` en el cierre: quien liquidó no puede cerrar. Es el último par de la
        // cadena, y en una delegación pequeña la misma persona tiene los dos botones.
        var cierraQuienLiquido = await cliente.PostAsJsonAsync(
            $"/misiones/{id}/cerrar",
            new { Ejecuta = "P-TRANSPORTE", Momento, Criterios = Array.Empty<object>(), Justificacion = (string?)null });
        Assert.Equal(HttpStatusCode.Conflict, cierraQuienLiquido.StatusCode);
        Assert.Contains("BD-06", await cierraQuienLiquido.Content.ReadAsStringAsync());

        // Sin criterios detectados, el expediente cierra limpio. Quien cierra no eligió
        // ese destino: no hay forma de pedirlo.
        var final = await cliente.PostAsJsonAsync(
            $"/misiones/{id}/cerrar",
            new { Ejecuta = "P-GERENCIA", Momento, Criterios = Array.Empty<object>(), Justificacion = (string?)null });
        Assert.Equal(HttpStatusCode.OK, final.StatusCode);
        Assert.Contains("Cerrada", await final.Content.ReadAsStringAsync());

        await using var contexto = baseDePruebas.Contexto();

        // El expediente se reconstruye desde su diario: siete transiciones, y ninguna
        // columna de estado que se pueda desincronizar (P-1).
        var transiciones = await contexto.Expedientes
            .Where(e => e.Id == Ulid.Parse(id))
            .SelectMany(e => e.Transiciones)
            .OrderBy(t => t.Orden)
            .ToListAsync();

        // T-01 crear · T-02 enviar · T-05 aprobar · T-08 programar · T-12 despachar
        // T-14 iniciar ruta · T-18 retornar · T-19 liquidar · T-21 cerrar.
        // Son nueve, y que sean nueve y no once es la prueba de que los dos intentos
        // bloqueados --BD-01 al autorizar y BD-06 al cerrar-- no dejaron rastro: no
        // ocurrieron. Un bloqueo duro no es una transición fallida; es una que no pasó.
        Assert.Equal(
            new[] { "T-01", "T-02", "T-05", "T-08", "T-12", "T-14", "T-18", "T-19", "T-21" },
            transiciones.Select(t => t.Transicion));

        // Y cada transición dejó su asiento encadenado, uno por una.
        var asientos = await contexto.Asientos
            .Where(a => a.Cola == $"mision:{id}")
            .OrderBy(a => a.Secuencia)
            .ToListAsync();

        Assert.Equal(transiciones.Count, asientos.Count);

        var cadena = asientos.Select(a => new EslabonDeCadena(a.Contenido, a.Hash)).ToList();
        Assert.True(CadenaDeHash.Verificar(cadena),
            "La bitácora del hilo completo no verifica: la cadena está rota.");
    }

    [Fact]
    public async Task Un_vehiculo_que_no_existe_en_la_flota_es_404_y_no_una_ficha_inventada()
    {
        // El cliente manda IDENTIFICADORES, no la ficha técnica. Antes mandaba la ficha
        // y podía declarar 2,800 kg de un camión de 12,000: BD-02 se evaluaba contra un
        // vehículo que no existe. Ahora ese error ni se puede expresar.
        var id = Ulid.NewUlid().ToString();
        await using (var siembra = baseDePruebas.Contexto()) await FlotaSembrada.SembrarAsync(siembra);

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await CrearYAprobar(cliente, id);

        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            IdVehiculo = Ulid.NewUlid().ToString(),
            IdConductor = FlotaSembrada.Conductor.ToString(),
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Un_identificador_mal_formado_se_explica_en_vez_de_reventar()
    {
        // Encontrado usando la API a mano: un ULID de 25 caracteres —o con una `I`, que el
        // alfabeto base32 excluye— producía **500 «Error no controlado»**.
        //
        // Importa más de lo que parece por `RNF-21`: **el identificador lo genera el
        // cliente de campo**, no el servidor. Un dispositivo con un error de generación
        // sincronizaría contra un 500 opaco, y quien lo diagnostique no tendría nada.
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await cliente.GetAsync("/misiones/NO-ES-UN-ULID");

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("identificador", await respuesta.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Programar y despachar llevan <b>identificadores del catálogo</b>, no la ficha ni
    /// la ventana: la ficha la resuelve el servidor y la ventana sale de la solicitud.
    /// </summary>
    private static async Task<HttpResponseMessage> Asignar(
        HttpClient cliente,
        string id,
        string ruta,
        string ejecuta,
        string idVehiculo,
        string idConductor,
        HttpStatusCode esperado = HttpStatusCode.OK)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/{ruta}", new
        {
            Ejecuta = ejecuta,
            Momento,
            IdVehiculo = idVehiculo,
            IdConductor = idConductor,
        });

        Assert.Equal(esperado, respuesta.StatusCode);
        return respuesta;
    }

    /// <summary>Deja un expediente aprobado y listo para programar.</summary>
    private async Task CrearYAprobar(HttpClient cliente, string id)
    {
        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-JEFE",
            Dependencia = "Delegación de Choluteca",
            ObjetoDelTraslado = "Traslado de personal y equipo",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 20),
            Retorno = new DateOnly(2026, 3, 22),
            HolguraDias = 1,
            Momento,
        });
        await Transicionar(cliente, id, "enviar", "P-ASISTENTE");
        await Transicionar(cliente, id, "aprobar", "P-JEFATURA");
    }

    private static async Task<HttpResponseMessage> Transicionar(
        HttpClient cliente,
        string id,
        string ruta,
        string ejecuta,
        HttpStatusCode esperado = HttpStatusCode.OK)
    {
        var respuesta = await cliente.PostAsJsonAsync(
            $"/misiones/{id}/{ruta}", new { Ejecuta = ejecuta, Momento });

        Assert.Equal(esperado, respuesta.StatusCode);
        return respuesta;
    }
}
