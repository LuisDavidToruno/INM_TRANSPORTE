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
            Salida = new DateOnly(2026, 3, 12),
            Retorno = new DateOnly(2026, 3, 14),
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
        var vencida = await Asignar(cliente, id, "programar", "P-TRANSPORTE",
            venceLicencia: new DateOnly(2026, 3, 14), esperado: HttpStatusCode.Conflict);
        Assert.Contains("BD-02", await vencida.Content.ReadAsStringAsync());

        await Asignar(cliente, id, "programar", "P-TRANSPORTE", venceLicencia: new DateOnly(2027, 1, 1));
        await Asignar(cliente, id, "despachar", "P-ENCARGADO", venceLicencia: new DateOnly(2027, 1, 1));
        await Transicionar(cliente, id, "iniciar-ruta", "P-MOTORISTA");
        await Transicionar(cliente, id, "retornar", "P-MOTORISTA");
        var final = await Transicionar(cliente, id, "liquidar", "P-TRANSPORTE");

        Assert.Contains("Liquidada", await final.Content.ReadAsStringAsync());

        await using var contexto = baseDePruebas.Contexto();

        // El expediente se reconstruye desde su diario: siete transiciones, y ninguna
        // columna de estado que se pueda desincronizar (P-1).
        var transiciones = await contexto.Expedientes
            .Where(e => e.Id == Ulid.Parse(id))
            .SelectMany(e => e.Transiciones)
            .OrderBy(t => t.Orden)
            .ToListAsync();

        // T-01 crear · T-02 enviar · T-05 aprobar · T-08 programar · T-12 despachar
        // T-14 iniciar ruta · T-18 retornar · T-19 liquidar.
        // Son ocho, y que sean ocho y no nueve es la prueba de que el intento bloqueado
        // por BD-01 no dejó rastro: no ocurrió.
        Assert.Equal(
            new[] { "T-01", "T-02", "T-05", "T-08", "T-12", "T-14", "T-18", "T-19" },
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
    public async Task Omitir_la_clase_normativa_es_error_de_peticion_y_no_una_moto_por_omision()
    {
        // Sin este rechazo, `ClaseNormativa` ausente se deserializa como el valor 0 del
        // enumerado —`Motocicleta`— y el servidor evalúa `BD-02` contra un vehículo que
        // el cliente nunca declaró. Bloquearía, que es la dirección segura, pero con un
        // mensaje que no tiene nada que ver con lo que pasó.
        var id = Ulid.NewUlid().ToString();
        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        await cliente.PostAsJsonAsync("/misiones", new
        {
            Id = id,
            CapturadaPor = "P-ASISTENTE",
            SolicitanteDeDerecho = "P-JEFE",
            Dependencia = "Delegación de Choluteca",
            ObjetoDelTraslado = "Traslado de personal y equipo",
            Destino = "Choluteca",
            Salida = new DateOnly(2026, 3, 12),
            Retorno = new DateOnly(2026, 3, 14),
            HolguraDias = 1,
            Momento,
        });
        await Transicionar(cliente, id, "enviar", "P-ASISTENTE");
        await Transicionar(cliente, id, "aprobar", "P-JEFATURA");

        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/programar", new
        {
            Ejecuta = "P-TRANSPORTE",
            Momento,
            Salida = new DateOnly(2026, 3, 12),
            Retorno = new DateOnly(2026, 3, 14),
            HolguraDias = 1,
            NumeroDeLicencia = "0801-1990-01234",
            CategoriaDeLicencia = "B",
            VenceLicencia = new DateOnly(2027, 1, 1),
            TipoDeVehiculo = "PICKUP",
            // ClaseNormativa ausente a propósito.
            PesoBrutoKg = 2_800,
            CapacidadPasajeros = 5,
            LlevaRemolque = false,
            TieneConstanciaSustitutaDePlaca = true,
            VenceMatricula = new DateOnly(2027, 1, 1),
            IdentificacionInstitucionalVerificada = true,
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    /// <summary>
    /// Programar y despachar llevan la asignación. La <b>placa va nula a propósito</b>:
    /// sin placa metálica es estado válido y no debe bloquear (`BD-03`).
    /// </summary>
    private static async Task<HttpResponseMessage> Asignar(
        HttpClient cliente,
        string id,
        string ruta,
        string ejecuta,
        DateOnly venceLicencia,
        HttpStatusCode esperado = HttpStatusCode.OK)
    {
        var respuesta = await cliente.PostAsJsonAsync($"/misiones/{id}/{ruta}", new
        {
            Ejecuta = ejecuta,
            Momento,
            Salida = new DateOnly(2026, 3, 12),
            Retorno = new DateOnly(2026, 3, 14),
            HolguraDias = 1,
            NumeroDeLicencia = "0801-1990-01234",
            CategoriaDeLicencia = "B",
            VenceLicencia = venceLicencia,
            RestriccionesDeLicencia = (string[]?)null,
            TipoDeVehiculo = "PICKUP",
            // La clase normativa es la del Artículo 4, no el nombre del catálogo
            // institucional: `PICKUP` es texto libre de la institución y `Automovil`
            // es lo que la matriz resuelve.
            ClaseNormativa = "Automovil",
            PesoBrutoKg = 2_800,
            CapacidadPasajeros = 5,
            LlevaRemolque = false,
            Placa = (string?)null,
            TieneConstanciaSustitutaDePlaca = true,
            VenceMatricula = new DateOnly(2027, 1, 1),
            VencePoliza = new DateOnly(2027, 1, 1),
            VenceRevisionMecanica = new DateOnly(2027, 1, 1),
            IdentificacionInstitucionalVerificada = true
        });

        Assert.Equal(esperado, respuesta.StatusCode);
        return respuesta;
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
