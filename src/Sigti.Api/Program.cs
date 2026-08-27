using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Aplicacion.M05_Motoristas;
using Sigti.Aplicacion.M16_Sincronizacion;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Dominio.M02_Parametros;
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
constructor.Services.AddScoped<ServicioDeParametros>();
constructor.Services.AddScoped<ConsultaDeMisiones>();
constructor.Services.AddScoped<EvaluacionDeAsignacion>();
constructor.Services.AddScoped<ServicioDeSincronizacion>();
constructor.Services.AddSingleton<CatalogoProvisionalDeFlota>();
constructor.Services.AddSingleton<CatalogoProvisionalDeRestricciones>();
constructor.Services.AddSingleton<IParametrosDeLaInstitucion, ParametrosProvisionales>();
// El cliente de oficina corre en otro origen durante el desarrollo. En producción
// se sirve desde el mismo host y esto sobra — por eso solo se activa en Development.
constructor.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5180").AllowAnyHeader().AllowAnyMethod()));

constructor.Services.AddOpenApi();

var app = constructor.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors();
}

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
        CargaRechazada c => (StatusCodes.Status409Conflict,
            new { motivo = c.Motivo.ToString(), mensaje = c.Message }),
        ExpedienteNoEncontrado n => (StatusCodes.Status404NotFound,
            new { mensaje = n.Message }),
        VersionNoEncontrada v => (StatusCodes.Status404NotFound,
            new { mensaje = v.Message }),
        // La caducidad no es un BD-xx: su salida no es cambiar de vehículo sino anular
        // con motivo tipificado, y por eso lleva su propia forma en la respuesta.
        AprobacionCaducada c2 => (StatusCodes.Status409Conflict,
            new { caducada = true, inicioDeLaVentana = c2.InicioDeLaVentana, mensaje = c2.Message }),
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
        new DatosDeLaSolicitud(
            peticion.Dependencia,
            peticion.ObjetoDelTraslado,
            peticion.Destino,
            new VentanaDeMision(peticion.Salida, peticion.Retorno, peticion.HolguraDias)),
        peticion.Momento);

    return Results.Created($"/misiones/{peticion.Id}", new { peticion.Id, estado = estado.ToString() });
});

misiones.MapGet("/", async (EstadoDeMision? estado, ConsultaDeMisiones consulta) =>
    Results.Ok(await consulta.PorEstadoAsync(estado ?? EstadoDeMision.Solicitada)));

misiones.MapGet("/{id}", async (string id, ConsultaDeMisiones consulta) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    return await consulta.PorIdAsync(ulid) is { } vista
        ? Results.Ok(vista)
        : Results.NotFound(new { mensaje = $"No existe el expediente {id}." });
});

Transicion("enviar", (e, quien, cuando) => e.Enviar(quien, cuando));
TransicionConMotivo("aprobar", (e, quien, cuando, motivo) => e.Aprobar(quien, cuando, motivo));
Transicion("iniciar-ruta", (e, quien, cuando) => e.IniciarRuta(quien, cuando));
Transicion("retornar", (e, quien, cuando) => e.Retornar(quien, cuando));
Transicion("liquidar", (e, quien, cuando) => e.Liquidar(quien, cuando));

// Catálogo provisional: M-03 y M-05 no existen. Va por el servidor y no por el
// cliente para que la evaluación de BD-02 tenga UNA sola implementación.
app.MapGet("/flota", (CatalogoProvisionalDeFlota flota) => Results.Ok(flota.Vehiculos));
app.MapGet("/conductores", (CatalogoProvisionalDeFlota flota) => Results.Ok(flota.Conductores));

// Evalúa sin comprometer nada: la pantalla muestra el resultado AL ELEGIR, y sale del
// mismo dominio que después bloquea T-08.
misiones.MapPost("/{id}/evaluar-asignacion", async (
    string id, EvaluarAsignacion peticion, EvaluacionDeAsignacion evaluacion) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    return await evaluacion.EvaluarAsync(
        ulid, peticion.IdVehiculo, peticion.IdConductor,
        peticion.HayConduccionNocturna, peticion.Momento) is { } resultado
        ? Results.Ok(resultado)
        : Results.NotFound(new { mensaje = "No existe el expediente, el vehículo o el conductor." });
});

var parametros = app.MapGroup("/parametros");

// HU-144: la carga nace PENDIENTE. Una carga que ya resolviera volvería decorativo el
// doble control de HU-145.
parametros.MapPost("/", async (CargarParametro peticion, ServicioDeParametros servicio) =>
{
    var id = await servicio.CargarAsync(
        new SolicitudDeCarga(
            peticion.Clave,
            peticion.Valor,
            peticion.VigenteDesde,
            peticion.VigenteHasta,
            peticion.RespaldoAdjunto is null
                ? null
                : new RespaldoDocumental(
                    Ulid.Parse(peticion.RespaldoAdjunto),
                    peticion.Fuente ?? "",
                    peticion.VerificadoEl ?? default),
            new IdPersona(peticion.CargadoPor)),
        peticion.Momento);

    return Results.Created($"/parametros/{id}", new { id = id.ToString(), estado = "PENDIENTE" });
});

// HU-146: responde 200 en los dos casos, con `concedida`. El rechazo no es un error del
// sistema — es el control funcionando, y queda asentado en la bitácora igual.
parametros.MapPost("/{id}/aprobar", async (
    string id, EjecutarTransicion peticion, ServicioDeParametros servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var intento = await servicio.AprobarAsync(
        ulid, new IdPersona(peticion.Ejecuta), peticion.Momento);

    return Results.Ok(new { id, concedida = intento.Concedida, motivo = intento.MotivoDelRechazo });
});

// T-09: la anulación exige motivo TIPIFICADO. El comentario es complemento, no
// sustituto: sin tipificación no hay indicador de déficit de flota.
misiones.MapPost("/{id}/anular", async (
    string id, AnularMision peticion, ServicioDeMisiones servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.Anular(new IdPersona(peticion.Ejecuta), peticion.Motivo, peticion.Comentario, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// Programar y despachar llevan la asignación en el cuerpo: son las dos transiciones que
// evalúan BD-02 y BD-03, y se revalidan en cada una con los datos del momento.
ConAsignacion("programar", (e, quien, a, m, p, cuando) => e.Programar(quien, a, m, p, cuando));
ConAsignacion("despachar", (e, quien, a, m, p, cuando) => e.Despachar(quien, a, m, p, cuando));

// M-16 — Donde aterriza lo que el dispositivo capturó sin red.
//
// `RNF-03`: 7 días sin conectividad y **0 registros perdidos**. Del lado del servidor eso
// es una sola propiedad: reenviar es inofensivo. Y tiene que serlo, porque el dispositivo
// que no supo si el servidor recibió VA a reenviar.
//
// Responde 200 aunque haya rechazos: el lote no es atómico. Que una transición no entre
// no puede impedir que las otras seis sí — el dispositivo lleva siete días de trabajo y
// perderlo todo por un expediente inexistente sería el fallo que este endpoint evita.
app.MapPost("/sincronizacion", async (
    LoteDeSincronizacion peticion, ServicioDeSincronizacion servicio) =>
{
    var hechos = new List<HechoCapturado>();

    foreach (var h in peticion.Transiciones ?? [])
    {
        if (!Identificador.Valido(h.IdDeCaptura, out var idDeCaptura, out var error)) return error;
        if (!Identificador.Valido(h.IdExpediente, out var idExpediente, out var errorExpediente))
            return errorExpediente;

        hechos.Add(new HechoCapturado(
            idDeCaptura, idExpediente, h.Transicion, h.Ejecuta, h.OcurridoEn));
    }

    var resultado = await servicio.RecibirAsync(hechos);

    return Results.Ok(new
    {
        dispositivo = peticion.IdDispositivo,
        // Lo que el dispositivo puede sacar de su cola: aplicadas y ya conocidas por
        // igual. Distinguirlas importa para diagnosticar, no para depurar la cola.
        acusadas = resultado.Aplicadas.Concat(resultado.YaConocidas).Select(i => i.ToString()),
        aplicadas = resultado.Aplicadas.Select(i => i.ToString()),
        yaConocidas = resultado.YaConocidas.Select(i => i.ToString()),
        rechazadas = resultado.Rechazadas.Select(r => new
        {
            idDeCaptura = r.IdDeCaptura.ToString(),
            motivo = r.Motivo,
        }),
    });
});

// T-20: devolver la liquidación para rehacerla. La alternativa a devolverla es cerrarla
// mal, y un descargo mal conciliado que se cierra ya no se corrige: se revierte.
misiones.MapPost("/{id}/devolver-liquidacion", async (
    string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
{
    if (string.IsNullOrWhiteSpace(peticion.Motivo))
        return Results.BadRequest(new { mensaje = "Devolver una liquidación exige motivo: quien la rehace tiene que saber qué corregir." });

    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.DevolverLiquidacion(new IdPersona(peticion.Ejecuta), peticion.Momento, peticion.Motivo!),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// T-21 y T-22 son UN SOLO endpoint a propósito. El cliente manda los criterios
// detectados; el destino lo decide el dominio. Si fueran dos rutas, quien cierra elegiría
// —y §7.2 dice exactamente lo contrario: «el criterio decide y él lo confirma».
misiones.MapPost("/{id}/cerrar", async (
    string id, CerrarMision peticion, ServicioDeMisiones servicio) =>
{
    var criterios = (peticion.Criterios ?? [])
        .Select(c => new HallazgoDetectado(c.Criterio, c.Detalle))
        .ToList();

    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.Cerrar(new IdPersona(peticion.Ejecuta), peticion.Momento, criterios, peticion.Justificacion),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

app.Run();
return;

void ConAsignacion(
    string ruta,
    Action<OrdenDeMision, IdPersona, AsignacionDeMision, MatrizDeLicencias, PoliticaDeDocumentacion, DateTimeOffset> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (
        string id,
        AsignarYTransicionar peticion,
        ServicioDeMisiones servicio,
        CatalogoProvisionalDeFlota flota,
        IParametrosDeLaInstitucion parametros) =>
    {
        // El cliente manda IDENTIFICADORES, no la ficha técnica. Si mandara la ficha,
        // podría declarar 2,800 kg de un camión de 12,000 y BD-02 se evaluaría contra
        // un vehículo que no existe.
        if (flota.Vehiculo(peticion.IdVehiculo) is not { } vehiculo)
            return Results.NotFound(new { mensaje = $"No existe el vehículo {peticion.IdVehiculo}." });

        if (flota.Conductor(peticion.IdConductor) is not { } conductor)
            return Results.NotFound(new { mensaje = $"No existe el conductor {peticion.IdConductor}." });

        var asignacion = new AsignacionDeMision(
            conductor.Licencia,
            vehiculo.Ficha,
            new DocumentacionDelVehiculo
            {
                Placa = vehiculo.Placa,
                TieneConstanciaSustitutaDePlaca = vehiculo.Placa is null,
                // ⚠️ M-04 no existe: no hay vencimientos reales. Queda dicho acá y en
                // EvaluacionDeAsignacion, en lugar de fingir que se verificó.
                VenceMatricula = new DateOnly(2030, 12, 31),
                VencePoliza = new DateOnly(2030, 12, 31),
                VenceRevisionMecanica = new DateOnly(2030, 12, 31),
                IdentificacionInstitucionalVerificada = true,
            });

        if (!Identificador.Valido(id, out var ulid, out var error)) return error;

        var estado = await servicio.TransicionarAsync(
            ulid,
            expediente =>
            {
                // Los parámetros se resuelven a la fecha del hecho, que sale de la
                // solicitud y no de la petición (P-4).
                var salida = expediente.Solicitud.Ventana.Salida;
                aplicar(expediente, new IdPersona(peticion.Ejecuta), asignacion,
                        parametros.MatrizVigenteAl(salida), parametros.PoliticaVigenteAl(salida),
                        peticion.Momento);
            },
            peticion.Momento);

        return Results.Ok(new { id, estado = estado.ToString() });
    });

void TransicionConMotivo(
    string ruta,
    Action<OrdenDeMision, IdPersona, DateTimeOffset, string?> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
    {
        if (!Identificador.Valido(id, out var ulid, out var error)) return error;

        var estado = await servicio.TransicionarAsync(
            ulid,
            expediente => aplicar(expediente, new IdPersona(peticion.Ejecuta), peticion.Momento, peticion.Motivo),
            peticion.Momento);

        return Results.Ok(new { id, estado = estado.ToString() });
    });

void Transicion(string ruta, Action<OrdenDeMision, IdPersona, DateTimeOffset> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
    {
        if (!Identificador.Valido(id, out var ulid, out var error)) return error;

        var estado = await servicio.TransicionarAsync(
            ulid,
            expediente => aplicar(expediente, new IdPersona(peticion.Ejecuta), peticion.Momento),
            peticion.Momento);

        return Results.Ok(new { id, estado = estado.ToString() });
    });

internal sealed record CrearMision(
    string Id,
    string CapturadaPor,
    string SolicitanteDeDerecho,
    string Dependencia,
    string ObjetoDelTraslado,
    string Destino,
    DateOnly Salida,
    DateOnly Retorno,
    int HolguraDias,
    DateTimeOffset Momento);

/// <summary>
/// El momento lo declara el cliente, no lo inventa el servidor: puede venir de un
/// dispositivo que capturó el hecho hace cuatro días sin señal (`ADR-007`).
/// </summary>
internal sealed record EjecutarTransicion(string Ejecuta, DateTimeOffset Momento, string? Motivo = null);

/// <summary>Qué se quiere asignar. La ventana NO viaja: sale de la solicitud.</summary>
internal sealed record EvaluarAsignacion(
    string IdVehiculo, string IdConductor, bool HayConduccionNocturna, DateTimeOffset Momento);

/// <summary>El motivo sale del catálogo cerrado; el comentario lo acompaña.</summary>
internal sealed record AnularMision(
    string Ejecuta, MotivoDeAnulacion Motivo, string? Comentario, DateTimeOffset Momento);

/// <summary>
/// Programar y despachar. La <b>placa es opcional</b>: sin placa metálica es un estado
/// válido y no bloquea (`BD-03`).
/// </summary>
internal sealed record AsignarYTransicionar(
    string Ejecuta,
    DateTimeOffset Momento,
    string IdVehiculo,
    string IdConductor);

/// <summary>
/// Carga de un parámetro normativo. El respaldo y la fuente son <b>obligatorios</b>:
/// «un parámetro sin respaldo no se puede sostener ante el Tribunal Superior de Cuentas».
/// Se reciben como opcionales para poder <b>rechazar con el mensaje correcto</b> en lugar
/// de devolver un error de formato que no explica nada.
/// </summary>
internal sealed record CargarParametro(
    string Clave,
    string Valor,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    string? RespaldoAdjunto,
    string? Fuente,
    DateOnly? VerificadoEl,
    string CargadoPor,
    DateTimeOffset Momento);

/// <summary>Expuesto para que las pruebas de punta a punta puedan levantar la aplicación.</summary>
public partial class Program;

/// <summary>
/// El cierre. <b>No lleva estado destino</b>: lo decide el dominio a partir de los
/// criterios, porque `orden-de-mision.md` §7.2 dice que quien cierra no elige.
/// </summary>
internal sealed record CerrarMision(
    string Ejecuta,
    DateTimeOffset Momento,
    IReadOnlyList<CriterioDetectado>? Criterios,
    string? Justificacion);

/// <summary>Un `H-nn` que se cumplió, con el caso concreto que lo demuestra.</summary>
internal sealed record CriterioDetectado(string Criterio, string Detalle);

/// <summary>
/// Convierte el identificador de la ruta, o dice por qué no pudo.
///
/// <b>Existe porque el identificador lo genera el cliente de campo, no el servidor</b>
/// (`RNF-21`, `ADR-005`). Un dispositivo con un error de generación sincronizaría contra
/// un <c>500 «Error no controlado»</c> y quien lo diagnostique no tendría nada — que es
/// exactamente lo que pasaba antes de esto.
///
/// El mensaje dice la longitud y el alfabeto porque son los dos errores reales: 25
/// caracteres en vez de 26, y las letras <c>I</c>, <c>L</c>, <c>O</c> y <c>U</c>, que
/// base32 excluye para que nadie confunda un uno con una ele.
/// </summary>
internal static class Identificador
{
    internal static bool Valido(string id, out Ulid resultado, out IResult error)
    {
        if (Ulid.TryParse(id, out resultado))
        {
            error = Results.Empty;
            return true;
        }

        error = Results.BadRequest(new
        {
            mensaje =
                $"«{id}» no es un identificador de expediente válido. Son 26 caracteres en " +
                "base32 de Crockford, sin las letras I, L, O ni U.",
        });
        return false;
    }
}

/// <summary>Lo que un dispositivo entrega al reconectar.</summary>
internal sealed record LoteDeSincronizacion(
    string IdDispositivo,
    IReadOnlyList<HechoDelDispositivo>? Transiciones);

/// <param name="IdDeCaptura">Lo generó el dispositivo (`ADR-005`). Identidad del hecho.</param>
internal sealed record HechoDelDispositivo(
    string IdDeCaptura,
    string IdExpediente,
    string Transicion,
    string Ejecuta,
    DateTimeOffset OcurridoEn);
