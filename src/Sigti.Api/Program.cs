using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

var constructor = WebApplication.CreateBuilder(args);

constructor.Services.AddDbContext<SigtiDbContext>(opciones =>
    opciones.UseSqlServer(
        constructor.Configuration.GetConnectionString("Sigti"),
        // ADR-002: EF Core vale 150 por omisión, y sin esto emite SQL que 2014 no entiende.
        sql => sql.UseCompatibilityLevel(120)));

// Las enumeraciones viajan por su nombre, no por su número: una categoría de licencia se
// llama `B` en el papel que el motorista lleva encima, y un cliente que tenga que mandar
// `1` está traduciendo el dominio a un número que nadie reconoce. Vale también para los
// estados de la misión, que el cliente de campo muestra tal cual.
constructor.Services.ConfigureHttpJsonOptions(opciones =>
    opciones.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

constructor.Services.AddScoped<ServicioDeMisiones>();
constructor.Services.AddSingleton<IParametrosDeLaInstitucion, ParametrosProvisionales>();
constructor.Services.AddOpenApi();

var app = constructor.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

// Las precondiciones de bloqueo duro son negativas del negocio, no fallas del sistema:
// 409 con el identificador de la precondición, para que el cliente pueda decir CUÁL
// regla lo detuvo en lugar de «ocurrió un error».
app.UseExceptionHandler(rama => rama.Run(async contexto =>
{
    var excepcion = contexto.Features
        .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    var (codigo, cuerpo) = excepcion switch
    {
        BloqueoDuro b => (StatusCodes.Status409Conflict,
            (object)new { precondicion = b.Precondicion, mensaje = b.Message }),
        TransicionInvalida t => (StatusCodes.Status409Conflict,
            new { transicion = t.Transicion, estadoActual = t.EstadoActual.ToString(), mensaje = t.Message }),
        ExpedienteNoEncontrado n => (StatusCodes.Status404NotFound,
            new { mensaje = n.Message }),
        _ => (StatusCodes.Status500InternalServerError, new { mensaje = "Error no controlado." })
    };

    contexto.Response.StatusCode = codigo;
    await contexto.Response.WriteAsJsonAsync(cuerpo);
}));

var misiones = app.MapGroup("/misiones");

// El identificador lo trae el cliente (ADR-005): el expediente nace con su ULID puesto,
// en el dispositivo, aunque no haya servidor de por medio.
misiones.MapPost("/", async (CrearMision peticion, ServicioDeMisiones servicio) =>
{
    var estado = await servicio.CrearAsync(
        Ulid.Parse(peticion.Id),
        new IdPersona(peticion.CapturadaPor),
        new IdPersona(peticion.SolicitanteDeDerecho),
        peticion.Momento);

    return Results.Created($"/misiones/{peticion.Id}", new { peticion.Id, estado = estado.ToString() });
});

Transicion("enviar", (e, quien, cuando) => e.Enviar(quien, cuando));
Transicion("aprobar", (e, quien, cuando) => e.Aprobar(quien, cuando));
Transicion("iniciar-ruta", (e, quien, cuando) => e.IniciarRuta(quien, cuando));
Transicion("retornar", (e, quien, cuando) => e.Retornar(quien, cuando));
Transicion("liquidar", (e, quien, cuando) => e.Liquidar(quien, cuando));

// Programar y despachar llevan la asignación en el cuerpo: son las dos transiciones que
// evalúan BD-02 y BD-03, y se revalidan en cada una con los datos del momento.
ConAsignacion("programar", (e, quien, a, m, p, cuando) => e.Programar(quien, a, m, p, cuando));
ConAsignacion("despachar", (e, quien, a, m, p, cuando) => e.Despachar(quien, a, m, p, cuando));

app.Run();
return;

void ConAsignacion(
    string ruta,
    Action<OrdenDeMision, IdPersona, AsignacionDeMision, MatrizDeLicencias, PoliticaDeDocumentacion, DateTimeOffset> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (
        string id,
        AsignarYTransicionar peticion,
        ServicioDeMisiones servicio,
        IParametrosDeLaInstitucion parametros) =>
    {
        var ventana = new VentanaDeMision(peticion.Salida, peticion.Retorno, peticion.HolguraDias);

        // Los parámetros se resuelven a la FECHA DEL HECHO, no a la de captura (P-4).
        var matriz = parametros.MatrizVigenteAl(ventana.Salida);
        var politica = parametros.PoliticaVigenteAl(ventana.Salida);

        var asignacion = new AsignacionDeMision(
            new Licencia(peticion.NumeroDeLicencia, peticion.CategoriaDeLicencia,
                peticion.VenceLicencia, peticion.RestriccionesDeLicencia ?? []),
            new FichaTecnica(peticion.TipoDeVehiculo, peticion.PesoBrutoKg,
                peticion.CapacidadPasajeros, peticion.EsArticulado),
            new DocumentacionDelVehiculo
            {
                Placa = peticion.Placa,
                TieneConstanciaSustitutaDePlaca = peticion.TieneConstanciaSustitutaDePlaca,
                VenceMatricula = peticion.VenceMatricula,
                VencePoliza = peticion.VencePoliza,
                VenceRevisionMecanica = peticion.VenceRevisionMecanica,
                IdentificacionInstitucionalVerificada = peticion.IdentificacionInstitucionalVerificada
            },
            ventana);

        var estado = await servicio.TransicionarAsync(
            Ulid.Parse(id),
            expediente => aplicar(expediente, new IdPersona(peticion.Ejecuta), asignacion,
                                  matriz, politica, peticion.Momento),
            peticion.Momento);

        return Results.Ok(new { id, estado = estado.ToString() });
    });

void Transicion(string ruta, Action<OrdenDeMision, IdPersona, DateTimeOffset> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
    {
        var estado = await servicio.TransicionarAsync(
            Ulid.Parse(id),
            expediente => aplicar(expediente, new IdPersona(peticion.Ejecuta), peticion.Momento),
            peticion.Momento);

        return Results.Ok(new { id, estado = estado.ToString() });
    });

internal sealed record CrearMision(
    string Id, string CapturadaPor, string SolicitanteDeDerecho, DateTimeOffset Momento);

/// <summary>
/// El momento lo declara el cliente, no lo inventa el servidor: puede venir de un
/// dispositivo que capturó el hecho hace cuatro días sin señal (`ADR-007`).
/// </summary>
internal sealed record EjecutarTransicion(string Ejecuta, DateTimeOffset Momento);

/// <summary>
/// Programar y despachar. La <b>placa es opcional</b>: sin placa metálica es un estado
/// válido y no bloquea (`BD-03`).
/// </summary>
internal sealed record AsignarYTransicionar(
    string Ejecuta,
    DateTimeOffset Momento,
    DateOnly Salida,
    DateOnly Retorno,
    int HolguraDias,
    string NumeroDeLicencia,
    CategoriaDeLicencia CategoriaDeLicencia,
    DateOnly VenceLicencia,
    IReadOnlyList<string>? RestriccionesDeLicencia,
    string TipoDeVehiculo,
    int PesoBrutoKg,
    int CapacidadPasajeros,
    bool EsArticulado,
    string? Placa,
    bool TieneConstanciaSustitutaDePlaca,
    DateOnly VenceMatricula,
    DateOnly? VencePoliza,
    DateOnly? VenceRevisionMecanica,
    bool IdentificacionInstitucionalVerificada);

/// <summary>Expuesto para que las pruebas de punta a punta puedan levantar la aplicación.</summary>
public partial class Program;
