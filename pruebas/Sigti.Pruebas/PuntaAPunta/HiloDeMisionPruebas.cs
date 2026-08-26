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
public class HiloDeMisionPruebas(BaseDePruebas baseDePruebas)
    : IClassFixture<BaseDePruebas>, IClassFixture<WebApplicationFactory<Program>>
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
            Momento
        });
        Assert.Equal(HttpStatusCode.Created, creacion.StatusCode);

        await Transicionar(cliente, id, "enviar", "P-ASISTENTE");

        // BD-01: el solicitante de derecho no autoriza lo suyo, aunque no lo haya
        // capturado. El bloqueo tiene que sobrevivir el viaje por la API, no quedarse
        // en el dominio.
        var bloqueada = await Transicionar(cliente, id, "aprobar", "P-JEFE", esperado: HttpStatusCode.Conflict);
        Assert.Contains("BD-01", await bloqueada.Content.ReadAsStringAsync());

        await Transicionar(cliente, id, "aprobar", "P-JEFATURA");
        await Transicionar(cliente, id, "programar", "P-TRANSPORTE");
        await Transicionar(cliente, id, "despachar", "P-ENCARGADO");
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

        Assert.Equal(7, transiciones.Count);
        Assert.Equal("T-19", transiciones[^1].Transicion);

        // Y cada transición dejó su asiento encadenado. El intento bloqueado por BD-01
        // NO dejó asiento: no ocurrió, así que no hay nada que asentar.
        var asientos = await contexto.Asientos
            .Where(a => a.Cola == $"mision:{id}")
            .OrderBy(a => a.Secuencia)
            .ToListAsync();

        Assert.Equal(7, asientos.Count);

        var cadena = asientos.Select(a => new EslabonDeCadena(a.Contenido, a.Hash)).ToList();
        Assert.True(CadenaDeHash.Verificar(cadena),
            "La bitácora del hilo completo no verifica: la cadena está rota.");
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
