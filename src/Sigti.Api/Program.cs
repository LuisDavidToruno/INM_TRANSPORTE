using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M01_Organizacion;
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
constructor.Services.AddScoped<ServicioDeAdjuntos>();
constructor.Services.AddScoped<ConsultaDeFlota>();
constructor.Services.AddScoped<ConsultaDeConductores>();
constructor.Services.AddScoped<ConsultaDelOrganigrama>();
constructor.Services.AddScoped<ConsultaDeOcupacion>();
// El almacén es un singleton con la raíz configurada: `ADR-004` quiere que la institución
// pueda moverlo a otro disco sin tocar el esquema, y eso empieza por no cablear la ruta.
constructor.Services.AddSingleton(new AlmacenDeArchivos(
    constructor.Configuration["Adjuntos:Raiz"]
    ?? Path.Combine(constructor.Environment.ContentRootPath, "adjuntos")));
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
        AdjuntoCorrupto a => (StatusCodes.Status409Conflict,
            (object)new { hashDeclarado = a.HashDeclarado, hashRecibido = a.HashRecibido, mensaje = a.Message }),
        AprobacionCaducada c2 => (StatusCodes.Status409Conflict,
            new { caducada = true, inicioDeLaVentana = c2.InicioDeLaVentana, mensaje = c2.Message }),
        _ => (StatusCodes.Status500InternalServerError, new { mensaje = "Error no controlado." })
    };

    contexto.Response.StatusCode = codigo;
    await contexto.Response.WriteAsJsonAsync(cuerpo);
}));

// ⚠️ SOLO EN DESARROLLO. Siembra una flota mínima para que las pantallas tengan algo
// contra qué trabajar mientras `M-03` no tiene alta de vehículos.
//
// Va acá y no en una migración a propósito: una migración con datos los mete también en
// la instancia de la institución, y una flota de prueba en producción es exactamente el
// tipo de dato que después nadie sabe si borrar.
if (app.Environment.IsDevelopment())
{
    using var ambito = app.Services.CreateScope();
    var contextoDeSiembra = ambito.ServiceProvider.GetRequiredService<SigtiDbContext>();

    if (!contextoDeSiembra.Vehiculos.Any())
    {
        contextoDeSiembra.Vehiculos.AddRange(
            VehiculoDeDesarrollo("01JQ8Z000000000000000VEH01", "INS-P-014", "PBM8842",
                "Pick-up doble cabina", ClaseNormativa.Automovil, 2_800, 5, false, new DateOnly(2027, 8, 31)),

            // Sin placa metálica: estado válido por el desabastecimiento nacional.
            VehiculoDeDesarrollo("01JQ8Z000000000000000VEH02", "INS-C-002", null,
                "Camión de carga", ClaseNormativa.Camion, 12_000, 3, false, new DateOnly(2027, 5, 30)),

            // Con plataforma enganchada: exige `BE`, y NO es articulado.
            VehiculoDeDesarrollo("01JQ8Z000000000000000VEH03", "INS-P-021", "PCH1190",
                "Pick-up con plataforma enganchada", ClaseNormativa.Automovil, 3_100, 5, true, new DateOnly(2027, 2, 28)),

            VehiculoDeDesarrollo("01JQ8Z000000000000000VEH04", "INS-M-007", "MHA221",
                "Motocicleta de mensajería", ClaseNormativa.Motocicleta, 180, 1, false, new DateOnly(2026, 11, 30)));

        contextoDeSiembra.SaveChanges();
    }

    if (!contextoDeSiembra.Conductores.Any())
    {
        contextoDeSiembra.Conductores.AddRange(
            ConductorDeDesarrollo("01JQ8Z000000000000000CON01", "José Ramón Cruz", true,
                "08-1988-77120", CategoriaDeLicencia.B, new DateOnly(2028, 4, 30), null),

            ConductorDeDesarrollo("01JQ8Z000000000000000CON02", "Marlon Pineda", true,
                "08-1979-40155", CategoriaDeLicencia.C, new DateOnly(2027, 9, 15), null),

            // `RN-57`: quien conduce, sea o no del padrón. El funcionario con vehículo
            // asignado no se exceptúa del bloqueo.
            ConductorDeDesarrollo("01JQ8Z000000000000000CON03", "Dilcia Fúnez", false,
                "08-1991-20388", CategoriaDeLicencia.A, new DateOnly(2029, 1, 31),
                "CONDUCCION DIURNA UNICAMENTE"),

            ConductorDeDesarrollo("01JQ8Z000000000000000CON04", "Wilmer Alvarado", true,
                "08-1985-61207", CategoriaDeLicencia.BE, new DateOnly(2027, 11, 20), null));

        contextoDeSiembra.SaveChanges();
    }
}

static FilaDeConductor ConductorDeDesarrollo(
    string id, string nombre, bool delPadron,
    string licencia, CategoriaDeLicencia categoria, DateOnly vence, string? restricciones) => new()
{
    Id = Ulid.Parse(id),
    Nombre = nombre,
    EsDelPadron = delPadron,
    NumeroDeLicencia = licencia,
    Categoria = categoria,
    VenceLicencia = vence,
    Restricciones = restricciones,
};

static FilaDeVehiculo VehiculoDeDesarrollo(
    string id, string siglas, string? placa, string tipo,
    ClaseNormativa clase, int kg, int pasajeros, bool remolque, DateOnly venceMatricula) => new()
{
    Id = Ulid.Parse(id),
    Siglas = siglas,
    Placa = placa,
    TieneConstanciaSustitutaDePlaca = placa is null,
    TipoDeVehiculo = tipo,
    Clase = clase,
    PesoBrutoKg = kg,
    CapacidadPasajeros = pasajeros,
    LlevaRemolque = remolque,
    VenceMatricula = venceMatricula,
    VencePoliza = null,
    VenceRevisionMecanica = null,
    IdentificacionInstitucionalVerificada = true,
};

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

// La flota sale de la BASE (`M-03`). El padrón de conductores sigue provisional: es
// `M-05` y no está construido. Los dos van por el servidor y no por el cliente para que
// la evaluación de `BD-02` tenga UNA sola implementación.
app.MapGet("/flota", async (ConsultaDeFlota flota) => Results.Ok(
    (await flota.TodosAsync()).Select(v => new
    {
        id = v.Id.ToString(),
        siglas = v.Siglas,
        placa = v.Placa,
        ficha = new
        {
            tipoDeVehiculo = v.TipoDeVehiculo,
            clase = v.Clase.ToString(),
            pesoBrutoKg = v.PesoBrutoKg,
            capacidadPasajeros = v.CapacidadPasajeros,
            llevaRemolque = v.LlevaRemolque,
        },
        venceMatricula = v.VenceMatricula,
    })));
app.MapGet("/conductores", async (ConsultaDeConductores padron) => Results.Ok(
    (await padron.TodosAsync()).Select(c => new
    {
        id = c.Id.ToString(),
        nombre = c.Nombre,
        esDelPadron = c.EsDelPadron,
        licencia = new
        {
            numero = c.NumeroDeLicencia,
            categoria = c.Categoria.ToString(),
            vencimiento = c.VenceLicencia,
            // El despachador ve QUE hay restriccion, no el diagnostico (RN-52).
            tieneRestricciones = !string.IsNullOrWhiteSpace(c.Restricciones),
        },
    })));

// `HU-009` — la bandeja de autorización muestra **desde cuándo no se confirma el
// espejo**, porque una jefatura que va a firmar sobre un organigrama de hace nueve días
// tiene derecho a saberlo ANTES de firmar.
//
// Es una sola pregunta para toda la bandeja, no una por expediente: repetir un dato
// global en cada fila lo convierte en ruido y deja de leerse.
//
// **Advierte, no bloquea.** La máquina de estados resuelve `T-05` como advertencia
// registrada; bloquear la autorización por un problema de integración paralizaría a la
// institución, que es el fallo que no se quiere (`HB1-10`).
app.MapGet("/organigrama/antiguedad", async (ConsultaDelOrganigrama organigrama) =>
{
    var antiguedad = await organigrama.AntiguedadDelEspejoAsync(DateTimeOffset.UtcNow);

    return Results.Ok(new
    {
        // Nulo y cero son cosas OPUESTAS: «nunca se confirmó» contra «se confirmó
        // hace un momento». Se distinguen en el contrato, no en la interpretación.
        nuncaConfirmado = antiguedad is null,
        diasSinConfirmar = antiguedad?.Days,
    });
});

// La ocupación de la flota — lo que `PT-026` necesita para que elegir vehículo deje de
// ser adivinar.
//
// **Es una proyección del diario, no una tabla de reservas** (P-1). La reserva vive en la
// transición que reservó, y por eso liberar es no volver a tomar: una misión anulada deja
// de ocupar porque el diario siguió, sin que nadie borre nada.
//
// La ventana se recibe y **no se lee del reloj** (`ADR-007`). Sin fechas, siete días desde
// hoy: es lo que la pantalla pide por omisión, y devolver la flota entera desde el origen
// del tiempo no serviría a nadie.
app.MapGet("/flota/ocupacion", async (DateOnly? desde, DateOnly? hasta, ConsultaDeOcupacion ocupacion) =>
{
    var inicio = desde ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
    var fin = hasta ?? inicio.AddDays(6);

    // Un rango invertido no es una consulta vacía: es una peticion mal armada, y
    // devolver cero carriles la haria pasar por «no hay flota ocupada».
    if (fin < inicio)
        return Results.BadRequest(new { mensaje = "La fecha final es anterior a la inicial." });

    return Results.Ok(new
    {
        desde = inicio,
        hasta = fin,
        carriles = await ocupacion.EnVentanaAsync(inicio, fin),
    });
});

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
// Sólo `T-08` recibe los recursos: es la que reserva. `T-12` revalida sobre lo ya
// reservado y volver a tomar ahí duplicaría la reserva sin liberar la anterior.
ConAsignacion("programar", (e, quien, a, m, p, cuando, recursos) => e.Programar(quien, a, m, p, cuando, recursos));
ConAsignacion("despachar", (e, quien, a, m, p, cuando, _) => e.Despachar(quien, a, m, p, cuando));

// M-16 — Donde aterriza lo que el dispositivo capturó sin red.
//
// `RNF-03`: 7 días sin conectividad y **0 registros perdidos**. Del lado del servidor eso

// `ADR-004` — El binario al sistema de archivos; a la base solo su rastro.
//
// Va como formulario y no como JSON porque el binario en base64 crece un 33 %, y sobre
// la red de un retén ese tercio se paga en tiempo y en batería.
app.MapPost("/adjuntos", async (HttpRequest peticion, ServicioDeAdjuntos servicio) =>
{
    if (!peticion.HasFormContentType)
        return Results.BadRequest(new { mensaje = "El adjunto se sube como formulario, con el archivo y su declaración." });

    var formulario = await peticion.ReadFormAsync();
    var archivo = formulario.Files["archivo"];

    if (archivo is null)
        return Results.BadRequest(new { mensaje = "Falta el archivo." });

    if (!Identificador.Valido(formulario["idAdjunto"].ToString(), out var idAdjunto, out var e1)) return e1;
    if (!Identificador.Valido(formulario["idTransicion"].ToString(), out var idTransicion, out var e2)) return e2;

    if (!DateTimeOffset.TryParse(formulario["capturadoEn"].ToString(), out var capturadoEn))
        return Results.BadRequest(new { mensaje = "«capturadoEn» tiene que ser una marca de tiempo con desfase (ADR-007)." });

    await using var contenido = archivo.OpenReadStream();

    var resultado = await servicio.RecibirAsync(
        new AdjuntoQueLlega(
            idAdjunto,
            idTransicion,
            formulario["hash"].ToString(),
            archivo.ContentType,
            formulario["clasificacion"].ToString(),
            capturadoEn),
        contenido,
        DateTimeOffset.UtcNow);

    // 201 la primera vez, 200 el reenvío. El dispositivo puede sacarlo de su cola en
    // los dos casos; distinguirlos sirve para diagnosticar, no para decidir.
    return resultado.EsNuevo
        ? Results.Created($"/adjuntos/{idAdjunto}", new { id = idAdjunto.ToString(), ruta = resultado.Ruta })
        : Results.Ok(new { id = idAdjunto.ToString(), ruta = resultado.Ruta, yaConocido = true });
});

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
    Action<OrdenDeMision, IdPersona, AsignacionDeMision, MatrizDeLicencias, PoliticaDeDocumentacion, DateTimeOffset, RecursosTomados?> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (
        string id,
        AsignarYTransicionar peticion,
        ServicioDeMisiones servicio,
        ConsultaDeConductores padron,
        ConsultaDeFlota flota,
        IParametrosDeLaInstitucion parametros) =>
    {
        // El cliente manda IDENTIFICADORES, no la ficha técnica. Si mandara la ficha,
        // podría declarar 2,800 kg de un camión de 12,000 y BD-02 se evaluaría contra
        // un vehículo que no existe.
        if (!Identificador.Valido(peticion.IdVehiculo, out var idVehiculo, out var errorVehiculo))
            return errorVehiculo;

        if (await flota.PorIdAsync(idVehiculo) is not { } vehiculo)
            return Results.NotFound(new { mensaje = $"No existe el vehículo {peticion.IdVehiculo}." });

        if (!Identificador.Valido(peticion.IdConductor, out var idConductor, out var errorConductor))
            return errorConductor;

        if (await padron.PorIdAsync(idConductor) is not { } conductor)
            return Results.NotFound(new { mensaje = $"No existe el conductor {peticion.IdConductor}." });

        // La documentación sale de la BASE, con vencimientos reales. `BD-03` puede
        // bloquear de verdad — antes no podía, y el código lo decía.
        var asignacion = new AsignacionDeMision(
            conductor.Licencia(),
            vehiculo.Ficha(),
            vehiculo.Documentacion());

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
                        peticion.Momento, new RecursosTomados(idVehiculo, idConductor));
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
