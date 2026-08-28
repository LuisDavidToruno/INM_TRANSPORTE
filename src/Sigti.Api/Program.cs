using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M01_Organizacion;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Aplicacion.M05_Motoristas;
using Sigti.Aplicacion.M16_Sincronizacion;
using Sigti.Aplicacion.M06_Solicitudes;
using Sigti.Aplicacion.M08_Bitacora;
using Sigti.Dominio.M08_Bitacora;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Aplicacion.M09_Combustible;
using Sigti.Dominio.M09_Combustible;
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
constructor.Services.AddScoped<ConsultaDeCustodias>();
constructor.Services.AddScoped<ConsultaDePermisos>();
constructor.Services.AddScoped<ConsultaDelDiaDeDespacho>();
constructor.Services.AddScoped<ConsultaDeOdometro>();
constructor.Services.AddScoped<EstadoDeLaFlota>();
constructor.Services.AddScoped<ServicioDeCombustible>();
constructor.Services.AddScoped<ServicioDeConciliacion>();
constructor.Services.AddSingleton<CatalogoProvisionalDeMotivosDeRechazo>();
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
        FondoNoEncontrado f => (StatusCodes.Status404NotFound,
            new { mensaje = f.Message }),
        AsignacionNoEncontrada an => (StatusCodes.Status404NotFound,
            new { mensaje = an.Message }),
        // El vale y el fondo tienen su propia excepción de transición, y no reutilizan la
        // de la misión: sus estados son otros, y devolver «exige Programada» sobre un vale
        // mandaría a buscar el problema en el expediente equivocado.
        TransicionInvalidaDeAsignacion ta => (StatusCodes.Status409Conflict,
            new { transicion = ta.Transicion, estadoActual = ta.EstadoActual.ToString(), mensaje = ta.Message }),
        TransicionInvalidaDelFondo tf => (StatusCodes.Status409Conflict,
            new { movimiento = tf.Movimiento, estadoActual = tf.EstadoActual.ToString(), mensaje = tf.Message }),
        VersionNoEncontrada v => (StatusCodes.Status404NotFound,
            new { mensaje = v.Message }),
        // La caducidad no es un BD-xx: su salida no es cambiar de vehículo sino anular
        // con motivo tipificado, y por eso lleva su propia forma en la respuesta.
        AdjuntoCorrupto a => (StatusCodes.Status409Conflict,
            (object)new { hashDeclarado = a.HashDeclarado, hashRecibido = a.HashRecibido, mensaje = a.Message }),
        AprobacionCaducada c2 => (StatusCodes.Status409Conflict,
            new { caducada = true, inicioDeLaVentana = c2.InicioDeLaVentana, mensaje = c2.Message }),
        // En desarrollo el tipo y el mensaje interno van en la respuesta: un «Error no
        // controlado» a secas obliga a reproducir el fallo para saber qué pasó, y así es como
        // un `Contains` que no traducía sobrevivió sin que nadie lo notara. **En la
        // institución no sale**: el detalle de una excepción puede llevar datos de la fila.
        _ => (StatusCodes.Status500InternalServerError, app.Environment.IsDevelopment()
            ? (object)new
            {
                mensaje = "Error no controlado.",
                tipo = excepcion?.GetType().Name,
                detalle = excepcion?.Message,
                interno = excepcion?.InnerException?.Message,
            }
            : new { mensaje = "Error no controlado." })
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

    // Las custodias de la flota de desarrollo. Sin ellas `BD-13` bloquea el despacho de los
    // cuatro vehículos y ninguna pantalla se puede recorrer — la ausencia de custodia no era
    // una decisión, era un hueco de la semilla.
    //
    // **`INS-C-002` queda deliberadamente sin custodio.** Es lo que mantiene alcanzable en
    // desarrollo el bloqueo de `BD-13` y la pastilla roja del padrón: una regla cuya condición
    // no se puede producir es una regla que nadie ve funcionar.
    if (!contextoDeSiembra.Custodias.Any())
    {
        contextoDeSiembra.Custodias.AddRange(
            CustodiaDeDesarrollo("01JQ8Z000000000000000CSTD1", "01JQ8Z000000000000000VEH01",
                "Rolando Discua", new DateOnly(2025, 2, 3), "Acta de entrega-recepción 2025-014"),

            CustodiaDeDesarrollo("01JQ8Z000000000000000CSTD2", "01JQ8Z000000000000000VEH03",
                "Wilmer Alvarado", new DateOnly(2025, 7, 18), "Acta de entrega-recepción 2025-061"),

            CustodiaDeDesarrollo("01JQ8Z000000000000000CSTD3", "01JQ8Z000000000000000VEH04",
                "Dilcia Fúnez", new DateOnly(2026, 1, 12), "Acta de entrega-recepción 2026-003"));

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

static FilaDeCustodia CustodiaDeDesarrollo(
    string id, string vehiculo, string custodio, DateOnly desde, string acta) => new()
{
    Id = Ulid.Parse(id),
    VehiculoId = Ulid.Parse(vehiculo),
    Custodio = custodio,
    Desde = desde,

    // Nulo es **vigente**, no eterno. Poner una fecha de cese acá inventaría un final que
    // nadie decidió.
    Hasta = null,
    Acta = acta,
};

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
    // Las horas se EXIGEN acá y no en el tipo del dominio. El dominio tiene que poder
    // representar los expedientes viejos, que no las tienen; lo que no puede es dejar entrar
    // uno nuevo sin ellas -- son lo que `BD-04` necesita para juzgar la hora inhabil y lo
    // que `PT-038` necesita para ordenar el dia del despachador.
    //
    // Las DOS o ninguna no aplica: acá son las dos, y punto. Media ventana con hora es peor
    // que ninguna, porque parece completa.
    if (peticion.HoraDeSalida is null || peticion.HoraDeRetorno is null)
        return Results.BadRequest(new
        {
            mensaje = "Declare la hora de salida y la de retorno (formato HH:mm). Sin ellas no " +
                      "se puede juzgar si la misión circula en hora inhábil ni ordenar el día " +
                      "del despachador.",
        });

    var estado = await servicio.CrearAsync(
        Ulid.Parse(peticion.Id),
        new IdPersona(peticion.CapturadaPor),
        new IdPersona(peticion.SolicitanteDeDerecho),
        new DatosDeLaSolicitud(
            peticion.Dependencia,
            peticion.ObjetoDelTraslado,
            peticion.Destino,
            new VentanaDeMision(peticion.Salida, peticion.Retorno, peticion.HolguraDias,
                                peticion.HoraDeSalida, peticion.HoraDeRetorno)),
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
// `T-14` y `T-18` llevan ODOMETRO, y por eso no usan el helper genérico: es el único ancla
// que el sistema tiene para detectar consumo de combustible sin relación con el uso, y el
// hallazgo típico del Tribunal Superior de Cuentas en flota es exactamente ése.
//
// La lectura de referencia se busca por VEHÍCULO y cruza misiones: un odómetro que
// retrocede entre dos misiones distintas es lo que `BD-05` existe para detectar.
ConOdometro("iniciar-ruta", (e, quien, cuando, o, captura, _, __) =>
    e.IniciarRuta(quien, cuando, new OdometroAlSalir(o.Lectura, o.UltimaConocida), captura));

ConOdometro("retornar", (e, quien, cuando, o, captura, subtipo, justificacion) =>
    e.Retornar(quien, cuando,
               new OdometroAlRetornar(o.Lectura, subtipo, justificacion), captura));
// `T-19` ya no usa el helper genérico: **`INV-34` exige que todas las asignaciones de
// combustible estén liquidadas**, y ese recuento no está en el expediente. Dejarlo en el
// helper significaba pasar nulo, y nulo es «no evaluada»: la regla quedaría escrita y sin
// ejecutar, que es exactamente lo que se está cerrando.
misiones.MapPost("/{id}/liquidar", async (
    string id, EjecutarTransicion peticion,
    ServicioDeMisiones servicio, ServicioDeCombustible combustible) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var recuento = await combustible.RecuentoDeLaMisionAsync(ulid);

    var estado = await servicio.TransicionarAsync(
        ulid,
        expediente => expediente.Liquidar(
            new IdPersona(peticion.Ejecuta), peticion.Momento, recuento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// La flota sale de la BASE (`M-03`). El padrón de conductores sigue provisional: es
// `M-05` y no está construido. Los dos van por el servidor y no por el cliente para que
// la evaluación de `BD-02` tenga UNA sola implementación.
// El padrón de flota — `PT-072`. Lleva el estado operativo y la custodia porque son las
// dos preguntas que se hacen al abrirlo: con cuáles se puede contar, y quién responde por
// cada uno. Sin ellas la lista es un catálogo, no un padrón.
app.MapGet("/flota", async (
    ConsultaDeFlota flota, EstadoDeLaFlota estados, ConsultaDeCustodias custodias) =>
{
    var vehiculos = await flota.TodosAsync();
    var salida = new List<object>();

    foreach (var v in vehiculos)
    {
        var estado = await estados.ActualAsync(v.Id);
        var historial = await custodias.DeVehiculoAsync(v.Id);

        // El custodio VIGENTE a hoy. La fecha se lee del reloj acá y no antes porque esto
        // es una lista para mirar, no una precondición: nada se juzga contra ella.
        var custodio = historial.FirstOrDefault(
            c => c.VigenteAl(DateOnly.FromDateTime(DateTime.UtcNow.Date)));

        salida.Add(new
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
            vencePoliza = v.VencePoliza,
            venceRevisionMecanica = v.VenceRevisionMecanica,

            // Nulo es «nunca se declaró», no «disponible» — §10.2 lista «alta reciente sin
            // habilitar» entre las causas de NO_DISPONIBLE.
            estado = estado?.ToString(),

            // Nulo es «sin custodio», y `BD-13` lo bloquea al despachar. Va en la lista
            // para que se vea ANTES de que alguien intente sacar el vehículo.
            custodio = custodio?.Custodio.Valor,

            excepcion = v.Excepcion() is { } e
                ? new { tipo = e.Tipo, desde = e.Desde, hasta = e.Hasta }
                : null,
        });
    }

    return Results.Ok(salida);
});
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

// El estado operativo del vehículo — §10.2. Sin esta puerta, el estado sólo se movía solo
// y ningún vehículo llegaba nunca a EN_TALLER: `BD-07` existía sin poder bloquear nada.
//
// `ASIGNADO` y `EN_MISION` NO entran por acá: los fija el sistema como consecuencia de una
// transición de la Orden de Misión, y declararlos a mano abriría la puerta a un vehículo
// «en misión» sin misión que lo respalde.
app.MapPost("/flota/{id}/estado", async (
    string id, DeclararEstado peticion, EstadoDeLaFlota flota, ConsultaDeFlota padron) =>
{
    if (!Identificador.Valido(id, out var idVehiculo, out var error)) return error;

    if (await padron.PorIdAsync(idVehiculo) is null)
        return Results.NotFound(new { mensaje = $"No existe el vehículo {id}." });

    var actual = await flota.ActualAsync(idVehiculo);

    // Sólo se cuentan las misiones abiertas cuando el destino es terminal: es una consulta
    // que recorre el diario del vehículo entero, y pedirla para declarar un taller sería
    // pagarla en cada cambio de estado sin usarla.
    var abiertas = ReglasDeEstadoOperativo.EsTerminal(peticion.Estado)
        ? await flota.MisionesAbiertasAsync(idVehiculo)
        : 0;

    try
    {
        ReglasDeEstadoOperativo.ExigirDeclarable(peticion.Estado, actual, abiertas);
    }
    catch (CambioDeEstadoInvalido invalido)
    {
        return Results.Conflict(new { precondicion = "10.2", mensaje = invalido.Message });
    }

    if (string.IsNullOrWhiteSpace(peticion.Motivo))
        return Results.BadRequest(new
        {
            mensaje = "Declare el motivo: §10.2 pide causa tipificada para NO_DISPONIBLE y " +
                      "acta para el préstamo y los estados terminales.",
        });

    await flota.AnotarAsync(idVehiculo, new CambioDeEstadoOperativo(
        peticion.Estado, peticion.Momento, peticion.Ejecuta, peticion.Motivo,
        Automatico: false));

    await flota.ConfirmarAsync();

    return Results.Ok(new { id, estado = peticion.Estado.ToString() });
});

// El estado actual y su historial. El historial va entero porque la pregunta que se hace la
// auditoría es «¿por qué no estuvo disponible en abril?», y el estado actual no la contesta.
app.MapGet("/flota/{id}/estado", async (string id, EstadoDeLaFlota flota) =>
{
    if (!Identificador.Valido(id, out var idVehiculo, out var error)) return error;

    return Results.Ok(new
    {
        // Nulo es «nunca se declaró», no «disponible»: §10.2 lista «alta reciente sin
        // habilitar» entre las causas de NO_DISPONIBLE.
        actual = (await flota.ActualAsync(idVehiculo))?.ToString(),
        historial = await flota.HistorialAsync(idVehiculo),
    });
});

// `PT-038` — el tablero del despachador. Cuatro listas y no una tabla ordenable: qué sale
// hoy, qué vuelve hoy, qué está afuera y qué debía haber vuelto. Son cuatro acciones con
// cuatro urgencias, y la cuarta es la que ninguna lista ordenada por fecha muestra sola —
// un retorno vencido no aparece «arriba», aparece en el pasado.
//
// La fecha se RECIBE y no se lee del reloj (`ADR-007`). Sin ella, hoy en UTC: el
// despachador abre la pantalla y ve su día, y quien reconstruye un día pasado lo pide.
// ── M-09 Combustible ────────────────────────────────────────────────────────
//
// `RN-26` el fondo del período, `RN-27` el vale con folio, `RN-32` contra qué se emite, y
// la máquina §10.1. Todas las comprobaciones viven en el dominio: acá sólo se traduce.

var fondos = app.MapGroup("/fondos");

fondos.MapPost("/", async (SolicitarFondo peticion, ServicioDeCombustible servicio) =>
{
    var id = await servicio.SolicitarFondoAsync(
        Ulid.Parse(peticion.Id), peticion.Ambito, peticion.AmbitoDeclarado,
        peticion.Desde, peticion.Hasta, new IdPersona(peticion.Solicita),
        peticion.Monto, peticion.Justificacion, peticion.Momento);

    return Results.Created($"/fondos/{id}", new { id = id.ToString() });
});

fondos.MapGet("/", async (ServicioDeCombustible servicio) =>
{
    var lista = await servicio.FondosAsync();
    var salida = new List<object>();

    foreach (var f in lista)
    {
        // El saldo se calcula por fondo y no en la proyección: es la resta sobre asientos
        // de `RN-26`, y hacerla en una consulta agregada la volvería un número que ya no
        // se puede rastrear hasta los movimientos que lo forman.
        var saldo = f.Estado is EstadoDelFondo.Solicitado ? 0m : await servicio.SaldoAsync(f.Id);

        salida.Add(new
        {
            id = f.Id.ToString(),
            ambito = f.Ambito.ToString(),
            ambitoDeclarado = f.AmbitoDeclarado,
            desde = f.Desde,
            hasta = f.Hasta,
            estado = f.Estado.ToString(),
            solicita = f.Solicita.Valor,
            aprueba = f.Aprueba?.Valor,

            // Nula es PENDIENTE, y el cliente necesita distinguirla de «no aplica»: es lo
            // que bloquea el cierre del período.
            partida = f.PartidaPresupuestaria,

            aprobado = f.Aprobado,
            saldo,
            diario = f.Diario.Select(m => new
            {
                movimiento = m.Id,
                destino = m.Destino.ToString(),
                ejecuta = m.Ejecuta.Valor,
                momento = m.Momento,
                motivo = m.Motivo,
                monto = m.Monto,
            }),
        });
    }

    return Results.Ok(salida);
});

fondos.MapPost("/{id}/aprobar", async (
    string id, AprobarFondo peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.MoverFondoAsync(
        Ulid.Parse(id),
        fondo => fondo.Aprobar(new IdPersona(peticion.Ejecuta), peticion.Monto,
                               peticion.Partida, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

fondos.MapPost("/{id}/ampliar", async (
    string id, AmpliarFondo peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.MoverFondoAsync(
        Ulid.Parse(id),
        fondo => fondo.Ampliar(new IdPersona(peticion.Ejecuta), peticion.Monto,
                               peticion.Motivo, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

fondos.MapPost("/{id}/cerrar", async (
    string id, CerrarFondo peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.CerrarFondoAsync(
        Ulid.Parse(id), new IdPersona(peticion.Ejecuta), peticion.Partida, peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

var vales = app.MapGroup("/combustible");

// **La petición NO trae el vehículo.** `RN-32` manda que el sistema lo precargue de la
// orden y no lo capture libremente; pedirlo aunque fuera sólo para rotular la respuesta
// obliga al cliente a conocer la reserva, y el próximo que lea el contrato va a creer que
// es contra ese valor que se valida.
vales.MapPost("/", async (
    EmitirVale peticion, ServicioDeCombustible servicio,
    IParametrosDeLaInstitucion parametros) =>
{
    var id = await servicio.EmitirAsync(
        Ulid.Parse(peticion.Id), peticion.Folio, Ulid.Parse(peticion.IdFondo),
        Ulid.Parse(peticion.IdMision), new IdPersona(peticion.Ejecuta),
        Ulid.Parse(peticion.IdMotoristaReceptor),
        peticion.Monto, peticion.Galones, peticion.Instrumento, peticion.TipoDeCombustible,

        // ⚠️ **La ficha del vehículo no declara el combustible que usa.** No hay columna
        // para eso en `M-03`, así que `RN-32` no puede comprobar la compatibilidad y lo dice
        // pasando nulo, en vez de suponer que coincide.
        combustibleDelVehiculo: null,

        parametros.EstadoMinimoParaEmitirCombustible,
        parametros.ToleranciaDeSobregiro,
        peticion.Momento);

    return Results.Created($"/combustible/{id}", new { id = id.ToString() });
});

vales.MapGet("/mision/{id}", async (string id, ServicioDeCombustible servicio) =>
    Results.Ok((await servicio.DeLaMisionAsync(Ulid.Parse(id))).Select(a => new
    {
        id = a.Id.ToString(),
        folio = a.Folio,
        estado = a.Estado.ToString(),
        instrumento = a.Instrumento,
        tipoDeCombustible = a.TipoDeCombustible,
        monto = a.Monto,
        galones = a.Galones,
        consumido = a.Consumido,
        galonesConsumidos = a.GalonesConsumidos,
        devuelto = a.Devuelto,
        tuvoConsumo = a.TuvoConsumo,
        resuelta = a.EstaResuelta,
        diario = a.Diario.Select(t => new
        {
            transicion = t.Id,
            destino = t.Destino.ToString(),
            ejecuta = t.Ejecuta.Valor,
            momento = t.Momento,
            motivo = t.Motivo,
            consumo = t.Consumo is null ? null : new
            {
                galones = t.Consumo.Galones,
                monto = t.Consumo.Monto,
                estacion = t.Consumo.Estacion,
                odometro = t.Consumo.Odometro,
                comprobante = t.Consumo.Comprobante,
            },
            devuelto = t.Devuelto,
        }),
    })));

vales.MapPost("/{id}/entregar", async (
    string id, EntregarVale peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.EntregarAsync(
        Ulid.Parse(id), new IdPersona(peticion.Ejecuta), peticion.Constancia, peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

vales.MapPost("/{id}/anular", async (
    string id, MotivarVale peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.TransicionarAsync(
        Ulid.Parse(id),
        vale => vale.Anular(new IdPersona(peticion.Ejecuta), peticion.Motivo, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

vales.MapPost("/{id}/consumo", async (
    string id, RegistrarConsumo peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.RegistrarConsumoAsync(
        Ulid.Parse(id), new IdPersona(peticion.Ejecuta),
        new ConsumoRegistrado(peticion.Galones, peticion.Monto, peticion.Estacion,
                              peticion.Odometro, peticion.Comprobante),
        peticion.Momento,
        peticion.IdDeCaptura is null ? null : Ulid.Parse(peticion.IdDeCaptura));

    return Results.Ok(new { estado = estado.ToString() });
});

vales.MapPost("/{id}/devolver", async (
    string id, MotivarVale peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.TransicionarAsync(
        Ulid.Parse(id),
        vale => vale.DevolverIntegra(new IdPersona(peticion.Ejecuta), peticion.Motivo, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

vales.MapPost("/{id}/extravio", async (
    string id, MotivarVale peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.TransicionarAsync(
        Ulid.Parse(id),
        vale => vale.DeclararExtravio(new IdPersona(peticion.Ejecuta), peticion.Motivo, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

vales.MapPost("/{id}/liquidar", async (
    string id, LiquidarVale peticion, ServicioDeCombustible servicio) =>
{
    var estado = await servicio.TransicionarAsync(
        Ulid.Parse(id),
        vale => vale.Liquidar(new IdPersona(peticion.Ejecuta), peticion.SaldoDevuelto,
                              peticion.Observacion, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { estado = estado.ToString() });
});

// `RN-30` — la conciliación galonaje–kilometraje.
//
// **El dictamen lo calcula el sistema.** Antes venía en la petición, y eso dejaba a quien
// concilia eligiendo si su propio caso era hallazgo: en seis meses no habría una sola
// desviación. Es el mismo invariante de §7.2 sobre el cierre — el criterio decide, la
// persona lo confirma con su causa.
vales.MapPost("/{id}/conciliar", async (
    string id, ConciliarVale peticion, ServicioDeConciliacion conciliacion) =>
{
    var estado = await conciliacion.ConciliarAsync(
        Ulid.Parse(id), new IdPersona(peticion.Ejecuta), peticion.Causa, peticion.Momento,
        // Los tres reparos que invalidan el cálculo sin invalidar el registro. Los declara
        // quien concilia porque hoy **el sistema no los sabe**: el nivel de tanque es de
        // `RN-83` y la espera con motor encendido de `M-19`, y ninguno existe.
        new ReparosDelCalculo(
            peticion.OdometroAveriado, peticion.NivelDeTanqueDispar,
            peticion.EsperaProlongadaRegistrada));

    return Results.Ok(new { estado = estado.ToString() });
});

// El dictamen ANTES de aplicarlo: quien concilia necesita ver contra qué se le va a juzgar,
// y una causa que se escribe sin saber el resultado es una causa escrita a ciegas.
vales.MapGet("/{id}/conciliacion", async (
    string id, ServicioDeConciliacion conciliacion) =>
{
    var r = await conciliacion.EvaluarAsync(Ulid.Parse(id));

    return Results.Ok(new
    {
        dictamen = r.Dictamen.ToString(),
        esHallazgo = r.EsHallazgo,
        kilometros = r.KilometrosRecorridos,
        galones = r.GalonesConsumidos,
        observado = r.RendimientoObservado,
        esperado = r.Esperado is null ? null : new
        {
            kmPorGalon = r.Esperado.KmPorGalon,
            // El origen viaja: un dictamen contra una propuesta del propio histórico y otro
            // contra el valor institucional no valen lo mismo, y sólo el segundo sostiene un
            // hallazgo firme.
            origen = r.Esperado.Origen.ToString(),
            version = r.Esperado.Version,
        },
        desviacion = r.Desviacion,
        evidencia = r.Evidencia,
    });
});

app.MapGet("/despacho/dia", async (string? fecha, ConsultaDelDiaDeDespacho tablero) =>
{
    // Una fecha mal formada NO es «hoy». Caer al día actual en silencio haría que un
    // enlace roto mostrara un tablero plausible del día equivocado, que es peor que un
    // error: el despachador actuaría sobre él.
    if (fecha is not null && !DateOnly.TryParse(fecha, out _))
        return Results.BadRequest(new
        {
            mensaje = $"«{fecha}» no es una fecha. Use el formato aaaa-mm-dd.",
        });

    var dia = fecha is null
        ? DateOnly.FromDateTime(DateTime.UtcNow.Date)
        : DateOnly.Parse(fecha);

    return Results.Ok(await tablero.DeLaFechaAsync(dia));
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

// Las salidas de SOLICITADA que no son aprobar. Hasta que existieron, la jefatura podía
// aprobar y nada más: la bandeja ofrecía media función de autoridad.
//
// `T-06` RECHAZAR es terminal y dice «no». `T-04` DEVOLVER dice «así no» y el expediente
// vuelve a quien lo capturó. Confundirlas hace que una solicitud arreglable muera, o que
// una improcedente dé vueltas para siempre — por eso son dos rutas y no una con bandera.
misiones.MapPost("/{id}/rechazar", async (
    string id, RechazarMision peticion,
    ServicioDeMisiones servicio, CatalogoProvisionalDeMotivosDeRechazo catalogo) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.Rechazar(new IdPersona(peticion.Ejecuta), peticion.Motivo,
                        peticion.Comentario, catalogo.Vigente, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// Qué motivos hay. La pantalla no los puede cablear: el catálogo es configurable por la
// institución (`HU-014`, insumo #1), y una lista duplicada en el cliente sería una lista
// que se separa de la que el servidor valida.
app.MapGet("/motivos-de-rechazo", (CatalogoProvisionalDeMotivosDeRechazo catalogo) =>
    Results.Ok(catalogo.Vigente.Codigos));

// `T-04` — devolver para corrección. Motivo LIBRE, a diferencia del rechazo: acá no se
// mide por qué se dijo que no, se dice qué falta, y un catálogo no puede enumerar lo que
// falta en un expediente concreto.
misiones.MapPost("/{id}/devolver", async (
    string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.DevolverParaCorreccion(new IdPersona(peticion.Ejecuta),
                                      peticion.Motivo ?? "", peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// `T-07` — desistir. NO exige segregación: no es un pronunciamiento sobre la solicitud,
// es que quien la pidió ya no la quiere.
misiones.MapPost("/{id}/desistir", async (
    string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.Desistir(new IdPersona(peticion.Ejecuta), peticion.Motivo ?? "", peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// `T-03` — descartar un borrador que nunca se envió.
misiones.MapPost("/{id}/descartar", async (
    string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.DescartarBorrador(new IdPersona(peticion.Ejecuta),
                                 peticion.Motivo ?? "", peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
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

// `T-11` — desprogramar. La única puerta por la que se libera un recurso: `EF-01` prohíbe
// quitarle el vehículo a una misión sin devolverla explícitamente a la cola, porque una
// misión que lo pierde en silencio se descubre el día de la salida, en el predio.
//
// Motivo LIBRE y no tipificado, a diferencia de la anulación: acá la misión sigue viva y
// lo que el motivo explica es a la dependencia por qué perdió el vehículo que ya tenía.
misiones.MapPost("/{id}/desprogramar", async (
    string id, EjecutarTransicion peticion, ServicioDeMisiones servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.Desprogramar(new IdPersona(peticion.Ejecuta), peticion.Motivo ?? "", peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// `T-13` — anular una ya programada. Motivo TIPIFICADO, como `T-09`: es el mismo indicador
// de déficit de flota, y una programada que se anula libera además recursos comprometidos.
misiones.MapPost("/{id}/anular-programada", async (
    string id, AnularMision peticion, ServicioDeMisiones servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.AnularProgramada(new IdPersona(peticion.Ejecuta), peticion.Motivo,
                                peticion.Comentario, peticion.Momento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

// Programar y despachar llevan la asignación en el cuerpo: son las dos transiciones que
// evalúan BD-02 y BD-03, y se revalidan en cada una con los datos del momento.
// Sólo `T-08` recibe los recursos: es la que reserva. `T-12` revalida sobre lo ya
// reservado y volver a tomar ahí duplicaría la reserva sin liberar la anterior.
// `BD-11` sólo la evalúa `T-08`: es la que toma. `T-12` despacha sobre lo ya reservado,
// y volver a comprobar el solape ahí chocaría contra la reserva de la propia misión.
ConAsignacion("programar", (e, quien, a, m, p, cuando, recursos, reservas, _, __, ___, operativo) =>
    e.Programar(quien, a, m, p, cuando, recursos, reservas, operativo));
ConAsignacion("despachar", (e, quien, a, m, p, cuando, _, __, ___, custodias, circulacion, ____) =>
    e.Despachar(quien, a, m, p, cuando, custodias, circulacion));

// `T-10` — cambiar el vehículo o quien conduce SIN soltar la misión. Comparte la
// resolución de recursos con programar y despachar: es la misma verificación de que el
// identificador existe y la misma construcción de la asignación contra la que se evalúan
// `BD-02` y `BD-03`. Lo único propio es el motivo, y por eso viaja en la misma petición.
ConAsignacion("reasignar", (e, quien, a, m, p, cuando, recursos, reservas, peticion, _, __, ___) =>
    e.Reasignar(quien, a, peticion.Motivo, peticion.Comentario, m, p, cuando, recursos, reservas));

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

        Ulid? idAsignacion = null;

        if (h.IdAsignacion is { } declarada)
        {
            if (!Identificador.Valido(declarada, out var idVale, out var errorVale))
                return errorVale;

            idAsignacion = idVale;
        }

        hechos.Add(new HechoCapturado(
            idDeCaptura, idExpediente, h.Transicion, h.Ejecuta, h.OcurridoEn,
            h.Odometro, h.Subtipo, h.Justificacion,
            idAsignacion,
            h.Carga is null
                ? null
                : new CargaSincronizada(
                    h.Carga.Galones, h.Carga.Monto, h.Carga.Estacion, h.Carga.Odometro,
                    h.Carga.Comprobante, h.Carga.CausaSinComprobante)));
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
    string id, CerrarMision peticion,
    ServicioDeMisiones servicio, ServicioDeCombustible combustible) =>
{
    var criterios = (peticion.Criterios ?? [])
        .Select(c => new HallazgoDetectado(c.Criterio, c.Detalle))
        .ToList();

    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    // §10.1: `T-21` y `T-22` exigen que todas las asignaciones estén conciliadas, en
    // cualquiera de las dos formas. Una desviación explicada no impide cerrar; un vale que
    // nadie contrastó contra el kilometraje, sí.
    var recuento = await combustible.RecuentoDeLaMisionAsync(ulid);

    var estado = await servicio.TransicionarAsync(
        ulid,
        e => e.Cerrar(new IdPersona(peticion.Ejecuta), peticion.Momento, criterios,
                      peticion.Justificacion, recuento),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});

app.Run();
return;

void ConAsignacion(
    string ruta,
    Action<OrdenDeMision, IdPersona, AsignacionDeMision, MatrizDeLicencias, PoliticaDeDocumentacion, DateTimeOffset, RecursosTomados?, IReadOnlyList<ReservaDeRecurso>?, AsignarYTransicionar, CustodiaAlDespachar, CirculacionEnDiaInhabil, EstadoOperativo?> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (
        string id,
        AsignarYTransicionar peticion,
        ServicioDeMisiones servicio,
        ConsultaDeConductores padron,
        ConsultaDeFlota flota,
        ConsultaDeOcupacion ocupacion,
        ConsultaDeCustodias custodias,
        ConsultaDelOrganigrama organigrama,
        ConsultaDePermisos permisos,
        EstadoDeLaFlota estadoDeLaFlota,
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

        // Las reservas se traen SIN filtrar por fecha: el solape lo decide el dominio.
        var reservas = await ocupacion.ReservasDeAsync(idVehiculo, idConductor, ulid);

        // El historial de custodia, igual: la vigencia se resuelve a la fecha del HECHO, y
        // solo `T-12` la usa. Se trae siempre porque son pocas filas y porque una consulta
        // condicional aqui obligaria a saber, en el enrutador, cual transicion la necesita.
        var historialDeCustodia = await custodias.DeVehiculoAsync(idVehiculo);

        // El espejo del organigrama, para la CUSTODIA VACANTE de `RN-22`: tener tarjeta de
        // responsabilidad abierta y tener custodio no son lo mismo cuando la persona ceso y
        // no quedo nadie para firmar el traspaso.
        var espejoDePuestos = await organigrama.VigenteAsync();

        // Los permisos de circulacion del expediente. Se traen TODOS: distinguir «no hay
        // ninguno» de «hay pero ninguno ampara» es lo que hace accionable el bloqueo, y ese
        // juicio es de la regla.
        var permisosDelExpediente = await permisos.DeExpedienteAsync(ulid);

        // `BD-07`: solo se programa desde DISPONIBLE. Nulo es «nadie le declaro estado», y
        // el dominio lo dice en el diario en vez de darlo por disponible.
        var estadoDelVehiculo = await estadoDeLaFlota.ActualAsync(idVehiculo);

        var estado = await servicio.TransicionarAsync(
            ulid,
            expediente =>
            {
                // Los parámetros se resuelven a la fecha del hecho, que sale de la
                // solicitud y no de la petición (P-4).
                var salida = expediente.Solicitud.Ventana.Salida;
                aplicar(expediente, new IdPersona(peticion.Ejecuta), asignacion,
                        parametros.MatrizVigenteAl(salida), parametros.PoliticaVigenteAl(salida),
                        peticion.Momento, new RecursosTomados(idVehiculo, idConductor), reservas,
                        peticion, new CustodiaAlDespachar(historialDeCustodia, espejoDePuestos),
                        new CirculacionEnDiaInhabil(
                            // El calendario se resuelve a la fecha del HECHO, como todo
                            // parametro normativo (P-4).
                            parametros.CalendarioVigenteAl(salida),
                            idVehiculo, idConductor,
                            vehiculo.Excepcion(),
                            permisosDelExpediente),
                        estadoDelVehiculo);
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

/// <summary>
/// `T-14` y `T-18`: las dos transiciones que registran odómetro.
///
/// Resuelven la <b>última lectura conocida del vehículo</b> antes de aplicar. Se busca por el
/// recurso que la misión tiene reservado —la última transición que reservó— porque la
/// referencia de `BD-05` cruza misiones y no es la de este expediente.
/// </summary>
void ConOdometro(
    string ruta,
    Action<OrdenDeMision, IdPersona, DateTimeOffset, LecturaResuelta, Ulid?, SubtipoDeRetorno, string?> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (
        string id,
        RegistrarOdometro peticion,
        ServicioDeMisiones servicio,
        ConsultaDeOdometro odometros) =>
    {
        if (!Identificador.Valido(id, out var ulid, out var error)) return error;

        if (peticion.Odometro is not { } lectura)
            return Results.BadRequest(new
            {
                mensaje = "Declare la lectura del odómetro. Es el único ancla que el sistema " +
                          "tiene para detectar consumo sin relación con el uso.",
            });

        // La referencia se resuelve ANTES de abrir la transacción: es una lectura, no
        // participa del cambio de estado, y meterla adentro alargaría el bloqueo por una
        // consulta que no lo necesita.
        var ultima = await odometros.UltimaLecturaDeLaMisionAsync(ulid);

        var estado = await servicio.TransicionarAsync(
            ulid,
            expediente => aplicar(
                expediente, new IdPersona(peticion.Ejecuta), peticion.Momento,
                new LecturaResuelta(lectura, ultima), peticion.IdDeCaptura,
                peticion.Subtipo, peticion.Justificacion),
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
    DateTimeOffset Momento,
    /// <summary>
    /// A qué hora sale y a qué hora vuelve.
    ///
    /// <b>Anulables en el tipo y exigidas en el endpoint.</b> El tipo tiene que poder
    /// representar una petición sin ellas para poder <b>rechazarla con un mensaje</b>; si
    /// fueran obligatorias, el enlace de modelo devolvería un error genérico que no dice
    /// cuál falta ni por qué importa.
    /// </summary>
    TimeOnly? HoraDeSalida = null,
    TimeOnly? HoraDeRetorno = null);

/// <summary>
/// El momento lo declara el cliente, no lo inventa el servidor: puede venir de un
/// dispositivo que capturó el hecho hace cuatro días sin señal (`ADR-007`).
/// </summary>
internal sealed record EjecutarTransicion(string Ejecuta, DateTimeOffset Momento, string? Motivo = null);

// ── M-09 Combustible ────────────────────────────────────────────────────────

internal sealed record SolicitarFondo(
    string Id,
    AmbitoDelFondo Ambito,
    string AmbitoDeclarado,
    DateOnly Desde,
    DateOnly Hasta,
    string Solicita,
    decimal Monto,
    string Justificacion,
    DateTimeOffset Momento);

/// <param name="Partida">
/// Nula es **pendiente**: `RN-26` manda registrar el fondo igual cuando el espejo de ARGOS no
/// la tiene, y bloquear su cierre. No es un campo que se pueda omitir por comodidad.
/// </param>
internal sealed record AprobarFondo(
    string Ejecuta, decimal Monto, string? Partida, DateTimeOffset Momento);

internal sealed record AmpliarFondo(
    string Ejecuta, decimal Monto, string Motivo, DateTimeOffset Momento);

internal sealed record CerrarFondo(string Ejecuta, string? Partida, DateTimeOffset Momento);

/// <param name="IdMotoristaReceptor">
/// **Quién está en la ventanilla**, por el ULID de su registro en el padrón. `RN-32` lo compara
/// contra el motorista de la orden: el servidor NO lo deduce de la reserva, porque entonces
/// compararía la orden consigo misma y el bloqueo no dispararía nunca.
/// </param>
internal sealed record EmitirVale(
    string Id,
    string Folio,
    string IdFondo,
    string IdMision,
    string IdMotoristaReceptor,
    string Ejecuta,
    decimal Monto,
    decimal? Galones,
    string Instrumento,
    string TipoDeCombustible,
    DateTimeOffset Momento);

internal sealed record EntregarVale(string Ejecuta, string Constancia, DateTimeOffset Momento);

/// <summary>Anular, devolver y declarar extravío: los tres exigen acta, y el acta va acá.</summary>
internal sealed record MotivarVale(string Ejecuta, string Motivo, DateTimeOffset Momento);

/// <param name="Comprobante">
/// Nulo es un caso previsto y no un descuido: `RN-85` tipifica la ausencia de comprobante, y el
/// registro del abastecimiento **no se omite nunca por falta de papel**.
/// </param>
/// <param name="IdDeCaptura">
/// El identificador del dispositivo. Es lo que hace inofensivo el reintento de un consumo
/// capturado sin conectividad — un galón contado dos veces inventa una desviación que nadie
/// va a poder explicar.
/// </param>
internal sealed record RegistrarConsumo(
    string Ejecuta,
    decimal Galones,
    decimal Monto,
    string Estacion,
    int Odometro,
    string? Comprobante,
    DateTimeOffset Momento,
    string? IdDeCaptura = null);

internal sealed record LiquidarVale(
    string Ejecuta, decimal SaldoDevuelto, string? Observacion, DateTimeOffset Momento);

/// <param name="Causa">
/// Por qué se desvió. **Obligatoria sólo si el cálculo dio hallazgo** — no se le puede pedir a
/// nadie que explique una desviación que no hubo. El sistema dice *qué* se desvió; el *por qué*
/// lo declara quien concilia, y `INV-35` lo exige para poder cerrar.
/// </param>
/// <param name="OdometroAveriado">
/// `RN-90` — el instrumento intervenido no mide, y su lectura no divide nada.
/// </param>
/// <param name="NivelDeTanqueDispar">
/// Salió con un nivel y volvió con otro muy distinto: los galones consumidos no son los
/// cargados. ⚠️ Se **declara** porque el sistema no lo sabe — el nivel de tanque es dato
/// obligatorio de bitácora en `RN-83`, que no está construido.
/// </param>
/// <param name="EsperaProlongadaRegistrada">
/// Motor encendido esperando: consume sin recorrer. ⚠️ También se declara, porque la medición
/// es de `M-19` y no existe. `RN-30` advierte que sin ella el hallazgo sería infundado.
/// </param>
internal sealed record ConciliarVale(
    string Ejecuta,
    DateTimeOffset Momento,
    string? Causa = null,
    bool OdometroAveriado = false,
    bool NivelDeTanqueDispar = false,
    bool EsperaProlongadaRegistrada = false);

/// <summary>Lo que `T-14` y `T-18` reciben — `BD-05`.</summary>
/// <param name="Odometro">
/// Anulable en el tipo y exigido en el endpoint, para poder <b>rechazarlo con un mensaje</b>
/// en vez de con un error genérico de enlace de modelo.
/// </param>
/// <param name="Subtipo">
/// Sólo lo usa `T-18`. <b>Ordinario</b> bloquea una lectura menor que la de salida —es error
/// de digitación, con el tablero delante—; <b>constatado</b> la registra y marca la
/// inconsistencia, porque el vehículo ya está en el predio y negarse a registrarlo lo deja
/// secuestrado por un trámite (`RN-79`, `HB3-04`).
/// </param>
internal sealed record RegistrarOdometro(
    string Ejecuta,
    DateTimeOffset Momento,
    int? Odometro = null,
    SubtipoDeRetorno Subtipo = SubtipoDeRetorno.Ordinario,
    string? Justificacion = null,
    Ulid? IdDeCaptura = null);

/// <summary>Declarar el estado operativo de un vehículo — §10.2.</summary>
/// <param name="Motivo">
/// Causa tipificada, referencia de acta, o la explicación. <b>Obligatorio</b>: §10.2 pide
/// causa tipificada para `NO_DISPONIBLE` y acta para el préstamo y los terminales, y un
/// cambio de estado sin razón no se sostiene ante el Tribunal Superior de Cuentas.
/// </param>
internal sealed record DeclararEstado(
    string Ejecuta,
    EstadoOperativo Estado,
    DateTimeOffset Momento,
    string? Motivo = null);

/// <summary>La lectura del hecho junto con la referencia contra la que se juzga.</summary>
internal sealed record LecturaResuelta(int Lectura, int? UltimaConocida);

/// <summary>Qué se quiere asignar. La ventana NO viaja: sale de la solicitud.</summary>
internal sealed record EvaluarAsignacion(
    string IdVehiculo, string IdConductor, bool HayConduccionNocturna, DateTimeOffset Momento);

/// <summary>El motivo sale del catálogo cerrado; el comentario lo acompaña.</summary>
internal sealed record AnularMision(
    string Ejecuta, MotivoDeAnulacion Motivo, string? Comentario, DateTimeOffset Momento);

/// <summary>
/// Un rechazo — `T-06`. <b>El comentario NO es opcional</b>, a diferencia del de la
/// anulación: el motivo tipificado dice qué se cuenta y el comentario dice a la
/// dependencia qué pasó. La exigencia vive en el dominio; acá el tipo sólo lo refleja.
/// </summary>
internal sealed record RechazarMision(
    string Ejecuta, string Motivo, string Comentario, DateTimeOffset Momento);

/// <summary>
/// Programar y despachar. La <b>placa es opcional</b>: sin placa metálica es un estado
/// válido y no bloquea (`BD-03`).
/// </summary>
internal sealed record AsignarYTransicionar(
    string Ejecuta,
    DateTimeOffset Momento,
    string IdVehiculo,
    string IdConductor,
    /// <summary>
    /// Sólo lo usa `T-10`. <b>Anulable acá y exigido en el dominio</b>: si la API lo hiciera
    /// obligatorio, programar y despachar tendrían que mandar un motivo que no significa
    /// nada, y la regla —que el cambio de recurso deja razón registrada— viviría en el
    /// contrato HTTP en vez de en el negocio.
    /// </summary>
    MotivoDeReasignacion? Motivo = null,
    string? Comentario = null);

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

/// <summary>La carga que el motorista capturó en la estación — los cinco datos de §10.1.</summary>
internal sealed record CargaDelDispositivo(
    decimal Galones,
    decimal Monto,
    string Estacion,
    int Odometro,
    string? Comprobante = null,
    string? CausaSinComprobante = null);

/// <param name="IdDeCaptura">Lo generó el dispositivo (`ADR-005`). Identidad del hecho.</param>
internal sealed record HechoDelDispositivo(
    string IdDeCaptura,
    string IdExpediente,
    string Transicion,
    string Ejecuta,
    DateTimeOffset OcurridoEn,
    /// <summary>
    /// La lectura del odómetro que el motorista capturó <b>sin red</b>. `BD-05` se evalúa en
    /// el dispositivo y <b>se revalida acá</b>: la referencia cruza misiones y el dispositivo
    /// sólo conoce la suya.
    /// </summary>
    int? Odometro = null,
    SubtipoDeRetorno Subtipo = SubtipoDeRetorno.Ordinario,
    string? Justificacion = null,
    /// <summary>
    /// El vale contra el que se consume. <b>Sólo `V-04`</b>: una misión lleva varios vales,
    /// y sin esto el servidor tendría que adivinar a cuál cargarle el galón.
    /// </summary>
    string? IdAsignacion = null,
    CargaDelDispositivo? Carga = null);
