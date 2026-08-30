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
using Sigti.Aplicacion.M18_Peajes;
using Sigti.Datos.M18_Peajes;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Aplicacion.M11_Mantenimiento;
using Sigti.Aplicacion.M12_Incidentes;
using Sigti.Aplicacion.M14_Auditoria;
using Sigti.Datos.M14_Auditoria;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M11_Mantenimiento;
using Sigti.Dominio.M12_Incidentes;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.M20_Integraciones;
using Sigti.Dominio.M18_Peajes;
using Sigti.Dominio.Reglas;
using Sigti.Aplicacion.M06_Solicitudes;
using Sigti.Dominio.M06_Solicitudes;
using Sigti.Aplicacion.M19_Seguimiento;
using Sigti.Dominio.M19_Seguimiento;
using Sigti.Dominio.M01_Organizacion;
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
constructor.Services.AddScoped<ServicioDeAbastecimientos>();
constructor.Services.AddScoped<ServicioDeReintegro>();
constructor.Services.AddScoped<ServicioDeTanques>();
constructor.Services.AddScoped<ServicioDePeajes>();
constructor.Services.AddScoped<ServicioDeConciliacionExterna>();
constructor.Services.AddScoped<ServicioDeHallazgosPosteriores>();
constructor.Services.AddScoped<ServicioDeSaldoDeApertura>();
constructor.Services.AddScoped<ServicioDeCierreDeEjercicio>();
constructor.Services.AddScoped<ServicioDeIncidentes>();
constructor.Services.AddScoped<ServicioDePrestamos>();
constructor.Services.AddScoped<ServicioDeIndisponibilidad>();
constructor.Services.AddScoped<ServicioDeTitulos>();
constructor.Services.AddScoped<ServicioDeCompetencias>();
constructor.Services.AddScoped<ServicioDeTareas>();
constructor.Services.AddScoped<ServicioDeSegregacion>();
constructor.Services.AddScoped<ServicioDeSeguimiento>();
constructor.Services.AddScoped<ServicioDelPuesto>();
constructor.Services.AddScoped<ServicioDeFolios>();
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
        // `R-3` — la pantalla de bloqueo tiene TRES partes, y la tercera es el camino de
        // salida. El mensaje del dominio cubre las dos primeras —que se impidio, y por que con
        // nombres y numeros—; la tercera se resuelve aca y viaja con el 409.
        //
        // **Nulo cuando no esta documentado.** Rellenarlo con «comuniquese con el
        // administrador» convertiria el silencio en una instruccion, y seria falsa: `ACT-01`
        // no tiene acceso al negocio y no puede resolver un bloqueo de negocio.
        BloqueoDuro b => (StatusCodes.Status409Conflict,
            (object)new
            {
                precondicion = b.Precondicion,
                mensaje = b.Message,
                salida = Salida(ReglasDeLaSalida.De(b.Precondicion)),
            }),
        TransicionInvalida t => (StatusCodes.Status409Conflict,
            new { transicion = t.Transicion, estadoActual = t.EstadoActual.ToString(), mensaje = t.Message }),
        // **El bloqueo por segregacion trae su propia forma.** El par de §5.2 es lo que
        // dice por donde se sale, y meterlo en 'precondicion' junto a los BD-xx haria que la
        // pantalla no pudiera ofrecer el escalamiento que §5.3.B.3 exige.
        SegregacionIncompatible seg => (StatusCodes.Status409Conflict,
            new
            {
                par = seg.Par,
                mensaje = seg.Message,
                pretendia = seg.Intento.Pretendia.ToString(),
                chocaCon = seg.Intento.ChocaCon.ToString(),
                escalarA = ReglasDeSegregacion.DestinoDelEscalamiento(false),

                // El par de §5.2 ya dice por donde se sale, y `BD-06` lo documenta en las
                // mismas palabras: se ejecuta desde otro puesto, o se escala.
                salida = Salida(ReglasDeLaSalida.De("BD-06")),
            }),
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

    // El espejo del organigrama. **En la institución lo puebla la integración con ARGOS**
    // (`RN-48`, `DP-001`) y no hay endpoint de escritura; acá se siembra porque sin ocupantes
    // no se puede ver operar la segregación de funciones, que es el control que más importa.
    //
    // Los puestos reproducen el caso de §5.4 a propósito: la sede tiene las cinco funciones
    // repartidas, y **la delegación de Choluteca las acumula en una sola persona**. Una
    // siembra donde todo cumple no muestra nunca el problema que el sistema existe para ver.
    if (!contextoDeSiembra.AsignacionesDePuesto.Any())
    {
        contextoDeSiembra.AsignacionesDePuesto.AddRange(
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG01", "P-ASISTENTE", "PUE-ASISTENTE-ADMIN"),
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG02", "P-JEFATURA", "PUE-JEFATURA-ADMIN"),
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG03", "P-TRANSPORTE", "PUE-JEFE-TRANSPORTE"),
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG04", "P-DESPACHO", "PUE-DESPACHO-SEDE"),
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG05", "P-COMBUSTIBLE", "PUE-COMBUSTIBLE"),
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG06", "P-GERENCIA", "PUE-GERENCIA-ADMIN"),
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG07", "P-AUDITORIA", "PUE-AUDITORIA-INTERNA"),

            // **El caso de §5.4.** Una persona, un puesto, todas las funciones.
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG08", "P-CHOLUTECA", "PUE-DELEGACION-CHOLUTECA"),

            // Y la coocupación de un traspaso: dos personas en el mismo puesto a la vez. Es
            // acotada y se registra, y existe porque el traspaso real dura días.
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG09", "P-CUSTODIO", "PUE-CUSTODIO-FLOTA"),
            OcupacionDeDesarrollo("01JQ8Z000000000000000ASG10", "P-TRANSPORTE", "PUE-CUSTODIO-FLOTA"));

        contextoDeSiembra.SaveChanges();
    }

    // La estructura de puestos con su jerarquia. **En la institucion la puebla la integracion
    // con ARGOS** y no hay endpoint de escritura; aca se siembra porque sin ella el escalamiento
    // de §5.3.B.3 no puede dar su primer salto y todo bloqueo termina en Gerencia
    // Administrativa, que es el ultimo recurso y no el primero.
    //
    // La delegacion de Choluteca cuelga de un Jefe Regional **de su misma unidad**: es lo que
    // hace que el primer salto exista. Y el respaldo de sede se designa aparte, porque no es
    // dato del organigrama sino politica de control interno nuestra.
    if (!contextoDeSiembra.PuestosEspejo.Any())
    {
        contextoDeSiembra.PuestosEspejo.AddRange(
            PuestoDeDesarrollo("01JQ8Z000000000000000PST01", "PUE-ASISTENTE-ADMIN",
                "Asistente administrativo", "Unidad de Transporte", "PUE-JEFE-TRANSPORTE", null),
            PuestoDeDesarrollo("01JQ8Z000000000000000PST02", "PUE-JEFATURA-ADMIN",
                "Jefatura de la dependencia", "Gerencia Administrativa", "PUE-GERENCIA-ADMIN", null),
            PuestoDeDesarrollo("01JQ8Z000000000000000PST03", "PUE-JEFE-TRANSPORTE",
                "Jefe de Transporte", "Unidad de Transporte", null, null),
            PuestoDeDesarrollo("01JQ8Z000000000000000PST04", "PUE-DESPACHO-SEDE",
                "Encargado de Despacho", "Unidad de Transporte", "PUE-JEFE-TRANSPORTE", null),
            PuestoDeDesarrollo("01JQ8Z000000000000000PST05", "PUE-COMBUSTIBLE",
                "Encargado de Combustible", "Unidad de Transporte", "PUE-JEFE-TRANSPORTE", null),
            PuestoDeDesarrollo("01JQ8Z000000000000000PST06", "PUE-GERENCIA-ADMIN",
                "Gerencia Administrativa", "Gerencia Administrativa", null, null),
            PuestoDeDesarrollo("01JQ8Z000000000000000PST07", "PUE-AUDITORIA-INTERNA",
                "Auditor Interno", "Auditoria Interna", null, null),

            // El caso de §5.4, ahora con su rama: el delegado cuelga de un jefe regional de su
            // misma unidad, y por eso el primer salto del escalamiento puede resolver.
            PuestoDeDesarrollo("01JQ8Z000000000000000PST08", "PUE-DELEGACION-CHOLUTECA",
                "Encargado de Delegacion", "Delegacion de Choluteca", "PUE-JEFE-REGIONAL",
                "Choluteca"),
            PuestoDeDesarrollo("01JQ8Z000000000000000PST09", "PUE-JEFE-REGIONAL",
                "Jefe Regional de Choluteca", "Delegacion de Choluteca", "PUE-JEFE-TRANSPORTE",
                "Choluteca"),

            PuestoDeDesarrollo("01JQ8Z000000000000000PST10", "PUE-CUSTODIO-FLOTA",
                "Custodio de flota", "Unidad de Transporte", "PUE-JEFE-TRANSPORTE", null));

        contextoDeSiembra.SaveChanges();
    }

    if (!contextoDeSiembra.RespaldosDeSede.Any())
    {
        contextoDeSiembra.RespaldosDeSede.Add(new FilaDeRespaldoDeSede
        {
            Id = Ulid.Parse("01JQ8Z000000000000000RSP01"),
            Delegacion = "Choluteca",
            Puesto = "PUE-JEFE-TRANSPORTE",
            Designa = "P-GERENCIA",
            Desde = new DateOnly(2026, 1, 1),
        });

        contextoDeSiembra.SaveChanges();
    }

    if (!contextoDeSiembra.Competencias.Any())
    {
        contextoDeSiembra.Competencias.AddRange(
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP01", "PUE-ASISTENTE-ADMIN",
                Rol.Solicitante, AlcanceDeDatos.Propio),
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP02", "PUE-JEFATURA-ADMIN",
                Rol.JefaturaInmediata, AlcanceDeDatos.Dependencia),
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP03", "PUE-JEFE-TRANSPORTE",
                Rol.JefeDeTransporte, AlcanceDeDatos.Institucion),
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP04", "PUE-DESPACHO-SEDE",
                Rol.EncargadoDeDespacho, AlcanceDeDatos.Dependencia),
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP05", "PUE-COMBUSTIBLE",
                Rol.EncargadoDeCombustible, AlcanceDeDatos.Dependencia),
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP06", "PUE-GERENCIA-ADMIN",
                Rol.GerenciaAdministrativa, AlcanceDeDatos.Institucion),
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP07", "PUE-AUDITORIA-INTERNA",
                Rol.AuditorInterno, AlcanceDeDatos.Institucion),
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP08", "PUE-CUSTODIO-FLOTA",
                Rol.CustodioDelVehiculo, AlcanceDeDatos.Propio),

            // Queda de acumulación vigilada, y es lo correcto: no se puede prohibir de entrada
            // que la delegación opere, sería inoperante. El bloqueo llega al ejecutar.
            CompetenciaDeDesarrollo("01JQ8Z000000000000000CMP09", "PUE-DELEGACION-CHOLUTECA",
                Rol.EncargadoDeDelegacion, AlcanceDeDatos.Delegacion,
                vigilados: "I-01, I-02, I-03, I-04, I-05, I-06, I-07, I-08, I-09, I-10"));

        contextoDeSiembra.SaveChanges();
    }
}

/// <summary>
/// Una ocupación del espejo. Sin fecha de fin: son los titulares vigentes.
/// </summary>
static FilaDeAsignacionDePuesto OcupacionDeDesarrollo(string id, string persona, string puesto) => new()
{
    Id = Ulid.Parse(id),
    Persona = persona,
    Puesto = puesto,
    Desde = new DateOnly(2026, 1, 1),
    Hasta = null,

    // El espejo se confirma, no se crea. Sin esto la bandeja diría que nunca se sincronizó.
    ConfirmadoAlUtc = DateTime.UtcNow,
};

static FilaDePuestoEspejo PuestoDeDesarrollo(
    string id, string puesto, string denominacion, string unidad,
    string? superior, string? delegacion) => new()
{
    Id = Ulid.Parse(id),
    Puesto = puesto,
    Denominacion = denominacion,
    Unidad = unidad,

    // Nulo es «la cima de su rama», no «falta el dato».
    Superior = superior,

    // Nulo es sede: el corte territorial y el jerarquico coexisten.
    Delegacion = delegacion,

    ConfirmadoAlUtc = DateTime.UtcNow,
};

static FilaDeCompetencia CompetenciaDeDesarrollo(
    string id, string puesto, Rol rol, AlcanceDeDatos alcance, string? vigilados = null) => new()
{
    Id = Ulid.Parse(id),
    Puesto = puesto,
    Rol = rol,
    Alcance = alcance,
    Desde = new DateOnly(2026, 1, 1),
    Hasta = null,
    Otorga = "P-ADMIN",
    ParesVigilados = vigilados,
};

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

/// <summary>
/// El resumen de una obligación. <b>Lleva el saldo y el monto original</b>, no sólo el
/// saldo: `CE-26` — el reporte muestra el valor original, el reverso y el resultado, nunca
/// sólo el resultado.
/// </summary>
/// <summary>
/// Un paso por caseta. <b>Lleva las dos categorías</b>: guardar sólo la cobrada haría que el
/// error de la caseta se volviera la verdad institucional y el reclamo nunca ocurriría.
/// </summary>
static object ResumirPaso(PasoPorCaseta p) => new
{
    id = p.Id.ToString(),
    punto = p.Punto.ToString(),
    vehiculo = p.Vehiculo.ToString(),
    mision = p.Mision?.ToString(),
    momento = p.OcurridoEn,
    odometro = p.Odometro,
    montoPagado = p.MontoPagado,
    montoEsperado = p.MontoEsperado,
    diferencia = p.Diferencia,
    medio = p.Medio.ToString(),
    categoriaEsperada = p.CategoriaEsperada?.Nombre,
    categoriaCobrada = p.CategoriaCobrada?.Nombre,
    discrepancia = p.HayDiscrepanciaDeClasificacion,
    ticket = p.Ticket,
    puntoNoCatalogado = p.PuntoNoCatalogado,
    ubicacion = p.UbicacionDeclarada,
    registra = p.Registra.Valor,
};

/// <summary>
/// El resumen de un expediente de hallazgo posterior. <b>Lleva las dos fechas</b>: `RN-93` las
/// exige como campos distintos, y la antigüedad se cuenta desde el hecho — contarla desde el
/// descubrimiento premiaría descubrir tarde.
/// </summary>
/// <summary>
/// Un renglón del saldo. <b>La antigüedad se cuenta desde el hecho</b>, no desde el corte:
/// contarla al revés dejaría presentar como reciente lo que lleva tres ejercicios.
/// </summary>
static object ResumirRenglon(RenglonDelSaldo r, DateOnly corte) => new
{
    tipo = r.Tipo.ToString(),
    referencia = r.Referencia,
    descripcion = r.Descripcion,
    fechaDelHecho = r.FechaDelHecho,
    antiguedadEnDias = r.AntiguedadEnDias(corte),
    causa = r.Causa.ToString(),
    responsable = r.Responsable,
    estado = r.Estado,

    // En cuántos saldos anteriores ya venía. Un renglón que aparece en tres consecutivos es
    // visible como tal, y eso impide presentarlo como nuevo cada enero.
    saldosAnteriores = r.SaldosAnteriores,

    monto = r.Monto,
    impideCerrar = r.ImpideCerrarElPeriodo,
};

/// <summary>Una reserva afectada, tal como se le presento a quien acusa — `RN-60` punto 1.</summary>
static object ResumirReserva(ReservaAfectada r) => new
{
    mision = r.Mision.ToString(),
    referencia = r.Referencia,
    dependencia = r.Dependencia,
    salida = r.Salida,
    retorno = r.Retorno,
    motorista = r.Motorista,
    objetoDelTraslado = r.ObjetoDelTraslado,
    estadoAlAcusar = r.EstadoAlAcusar.ToString(),
};

/// <summary>
/// La indisponibilidad sobrevenida — `RN-60`.
///
/// La lista de reservas va **tal como se conservo**, no reconstruida: quien acuso lo hizo sobre
/// esa lista, y mostrarla como estan hoy haria que el acuse cubriera otra cosa.
/// </summary>
static object ResumirIndisponibilidad(IndisponibilidadDelVehiculo i)
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    return new
    {
        id = i.Id.ToString(),
        vehiculo = i.VehiculoId.ToString(),
        estado = i.Estado.ToString(),
        causa = i.Causa,
        desde = i.Desde,
        finEstimado = i.FinEstimado,
        ejecuta = i.Ejecuta,
        momentoDelAcuse = i.MomentoDelAcuse,
        estaVigente = i.EstaVigente,
        excedeLoEstimado = i.ExcedeLoEstimado(hoy),

        reservas = i.Reservas.Select(ResumirReserva),

        // No expiran en silencio: sin desenlace registrado siguen aca aunque su ventana pasara.
        sinDesenlace = i.SinDesenlace.Select(ResumirReserva),

        resoluciones = i.Resoluciones.Select(r => new
        {
            mision = r.Mision.ToString(),
            desenlace = r.Desenlace.ToString(),
            ejecuta = r.Ejecuta,
            momento = r.Momento,
            motivo = r.Motivo,
        }),

        finReal = i.FinReal,
        ordenDeTrabajo = i.OrdenDeTrabajo,
        odometroDeSalida = i.OdometroDeSalida,

        // `RN-60` punto 6 — indicador de la gestion del taller. Nulo mientras no vuelva.
        desviacionEnDias = i.DesviacionEnDias,
    };
}

/// <summary>El titulo de tenencia — `RN-62`.</summary>
static object ResumirTitulo(TituloDeTenencia t)
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    return new
    {
        id = t.Id.ToString(),
        vehiculo = t.VehiculoId.ToString(),
        regimen = t.Regimen.ToString(),
        titular = t.Titular,
        documento = t.Documento,
        desde = t.Desde,

        // Nula en propiedad: el bien es del Estado y no vence.
        hasta = t.Hasta,
        diasRestantes = t.DiasRestantes(hoy),
        vigente = t.VigenteAl(hoy),

        // Lo que decide cual de los dos terminales corresponde (`HB3-17`).
        esBienPropio = t.EsBienPropio,

        // La matriz de `RN-62`. Lo que cubre el titular NO se imputa a nuestro presupuesto.
        rubros = t.Rubros.Todos.Select(r => new { rubro = r.Rubro, quien = r.Quien.ToString() }),
        rubrosDelTitular = t.Rubros.DelTitular,
        rubrosSinPactar = t.Rubros.SinPactar,
    };
}

/// <summary>
/// Un expediente de préstamo — `RN-63`.
/// </summary>
static object ResumirPrestamo(ExpedienteDePrestamo p)
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    return new
    {
        id = p.Id.ToString(),
        vehiculo = p.VehiculoId.ToString(),
        acto = new { folio = p.Acto.Folio, firmante = p.Acto.Firmante, fecha = p.Acto.Fecha },
        autoriza = p.Autoriza,
        receptor = new
        {
            persona = p.Receptor.Persona,
            cargo = p.Receptor.Cargo,
            institucion = p.Receptor.Institucion,
        },
        motivo = p.MotivoDelPrestamo,
        desde = p.Desde,
        devolucionComprometida = p.DevolucionComprometida,
        estaVigente = p.EstaVigente,

        // `RN-63` punto 4 — escalamiento diario mientras dure, y `RN-97` punto 4 le da poder
        // de bloqueo sobre el cierre del período.
        diasDeMora = p.DiasDeMora(hoy),
        estaVencido = p.EstaVencido(hoy),

        // `RN-63` punto 3 — NO entran en la conciliación galonaje-kilometraje: no hubo consumo
        // nuestro contra esos kilómetros. Nulo mientras no haya acta de devolución.
        kilometrosBajoTenenciaAjena = p.KilometrosBajoTenenciaAjena,

        // Hallazgo frecuente de auditoría, y por eso se reconstata al devolver.
        volvioSinRotulacion = p.VolvioSinRotulacion,

        // Un rubro sin pactar es el que aparece cuando llega la multa.
        rubrosSinPactar = p.Rubros.SinPactar,

        devolucion = p.Devolucion is null ? null : new
        {
            fecha = p.Devolucion.Fecha,
            odometro = p.Devolucion.Odometro,
            rotulacionConstatada = p.Devolucion.RotulacionConstatada,
            novedades = p.Devolucion.NovedadesODanios,
            firma = p.QuienFirmaLaDevolucion,
        },
    };
}

/// <summary>
/// Un expediente de incidente — M-12.
///
/// ── No lleva un solo campo de responsabilidad ───────────────────────────────
/// `RN-74`. Lo más cercano es <c>determinacion</c>, que es el <b>acto de otra instancia</b>
/// adjuntado al expediente, con su número y su emisor.
/// </summary>
static object ResumirIncidente(ExpedienteDeIncidente i) => new
{
    id = i.Id.ToString(),
    tipo = i.Tipo.ToString(),
    causa = i.Causa,

    // Las dos fechas, siempre (`RN-46`): `RN-70` admite captura sin conectividad, y un
    // incidente capturado cinco días después no es un incidente de ese día.
    fechaDelHecho = i.FechaDelHecho,
    momentoDelHecho = i.MomentoDelHecho,
    momentoDeCaptura = i.MomentoDeCaptura,
    diasEntreElHechoYLaCaptura = i.DiasEntreElHechoYLaCaptura,

    descripcion = i.Descripcion,
    registra = i.Registra,
    mision = i.MisionId?.ToString(),
    vehiculo = i.VehiculoId?.ToString(),
    ubicacion = i.Ubicacion,

    // Nulo es **no leído**, no cero: un odómetro en cero sería una lectura falsa.
    odometro = i.Odometro,

    // `RN-70` — marca la misión como interrumpida y **no le cambia el estado**.
    interrumpe = i.Interrumpe,
    desenlace = i.Desenlace?.ToString(),
    detalleDelDesenlace = i.DetalleDelDesenlace,

    // La propiedad que le da poder de bloqueo al cierre del período (`RN-97` punto 4).
    esInterrupcionSinDesenlace = i.EsInterrupcionSinDesenlace,

    responsableDeSeguimiento = i.ResponsableDeSeguimiento,
    plazo = i.Plazo,

    constancia = i.Constancia is null ? null : new
    {
        numero = i.Constancia.Numero,
        autoridad = i.Constancia.AutoridadReceptora,
        fecha = i.Constancia.Fecha,
    },

    // Su ausencia no impide registrar el evento, pero genera obligación con plazo (`RN-75`).
    debeConstancia = i.DebeConstancia,

    // `RN-75` — el bien permanece en el registro hasta su recuperación o su descargo formal.
    bienes = i.Bienes.Select(b => new
    {
        id = b.Id.ToString(),
        descripcion = b.Descripcion,
        esElVehiculo = b.EsElVehiculo,
        estado = b.Estado.ToString(),
        fechaDelHecho = b.FechaDelHecho,
        diasFuera = b.DiasFuera(DateOnly.FromDateTime(DateTime.UtcNow)),
        ubicacionConocida = b.UbicacionConocida,
        autoridadCustodia = b.AutoridadCustodia,
        numeroDeExpedienteExterno = b.NumeroDeExpedienteExterno,
        descargo = b.Descargo is null ? null : new
        {
            numero = b.Descargo.Numero,
            autoridad = b.Descargo.Autoridad,
            fecha = b.Descargo.Fecha,
        },
    }),

    gestiones = i.Gestiones.Select(g => new
    {
        fecha = g.Fecha,
        descripcion = g.Descripcion,
        responsable = g.Responsable,
        plazo = g.Plazo,
    }),

    // **El acto de otra instancia**, no un campo que alguien llenó (`RN-74`).
    determinacion = i.Determinacion is null ? null : new
    {
        numero = i.Determinacion.Numero,
        instancia = i.Determinacion.InstanciaQueLaEmite,
        fecha = i.Determinacion.Fecha,
        resolucion = i.Determinacion.Resolucion,
    },

    movimientos = i.Movimientos.Select(m => new
    {
        movimiento = m.Movimiento,
        momento = m.Momento,
        ejecuta = m.Ejecuta,
        detalle = m.Detalle,
    }),

    resueltoEn = i.ResueltoEn,
    comoSeResolvio = i.ComoSeResolvio,
    estaAbierto = i.EstaAbierto,
};

/// <summary>
/// El acta de cierre de ejercicio — `RN-96`.
///
/// <b>El inventario no se repite acá:</b> vive en el saldo de apertura, que es el documento
/// con el que `RN-96` punto 2 manda que cuadre renglón por renglón. Lo que va es su conteo y
/// las diferencias contra él.
/// </summary>
static object ResumirActa(ActaDeCierreDeEjercicio a) => new
{
    id = a.Id.ToString(),
    folio = a.Folio,
    ejercicio = a.Ejercicio,
    corteLegal = a.CorteLegal,
    corteOperativo = a.CorteOperativo,
    ejecuta = a.Ejecuta.Persona.Valor,
    momento = a.Momento,

    inventario = a.InventarioNoTerminal.Count,

    // **Nulo es que no hay saldo producido**, y por lo tanto nada contra qué cuadrar. Sin
    // este campo una lista de diferencias vacía se lee como coincidencia perfecta.
    saldoDeAperturaFolio = a.SaldoDeAperturaFolio,
    diferenciasConElSaldo = a.DiferenciasConElSaldo,

    // `RN-96` punto 4 — la misión no se divide; sus hechos se imputan a su propia fecha.
    misionesQueCruzan = a.MisionesQueCruzan.Select(m => new
    {
        mision = m.Mision.ToString(),
        referencia = m.Referencia,
        salida = m.Salida,
        retorno = m.Retorno,
        porEjercicio = m.PorEjercicio,
        hechos = m.Hechos.Select(h => new
        {
            ejercicio = h.Ejercicio,
            fechaDelHecho = h.FechaDelHecho,
            concepto = h.Concepto,
            monto = h.Monto,
            tablaParametrica = h.TablaParametrica,
        }),
        sinTablaParametrica = m.SinTablaParametrica.Count,
    }),

    // `RN-96` punto 5 — ni el compromiso ni el folio se arrastran al ejercicio siguiente.
    foliosPorAnular = a.FoliosPorAnular.Select(f => new
    {
        asignacion = f.Asignacion.ToString(),
        folio = f.Folio,
        delegacion = f.Delegacion,
        monto = f.Monto,
        emitido = f.Emitido,
        estado = f.Estado,
        sePuedeAnular = f.SePuedeAnular,
    }),
    montoPorAnular = a.MontoPorAnular,

    // `RN-96` punto 6 — la evidencia de que nadie aflojó un umbral en diciembre.
    cambiosDeParametros = a.CambiosDeParametros.Select(c => new
    {
        clave = c.Clave,
        valorAnterior = c.ValorAnterior,
        valorNuevo = c.ValorNuevo,
        vigenteDesde = c.VigenteDesde,
        registrado = c.Registrado,
        cargadoPor = c.CargadoPor,
        aprobadoPor = c.AprobadoPor,
    }),

    // `RN-96` punto 3 — nunca un motivo compartido por varios expedientes.
    motivosCompartidos = a.MotivosCompartidos.Select(m => new
    {
        motivo = m.Motivo,
        misiones = m.Misiones.Select(x => x.ToString()),
        primero = m.Primero,
        ultimo = m.Ultimo,
        ventanaEnMinutos = (int)m.Ventana.TotalMinutes,
    }),

    // `RN-96` — la ventana es **parámetro con vigencia**, resuelta a la fecha del corte legal.
    // Nula cuando la institución no la fijó, y entonces los dos reportes que dependen de ella
    // salieron sin medir, no en cero.
    ventana = a.Ventana is null ? null : new
    {
        desde = a.Ventana.Desde,
        hasta = a.Ventana.Hasta,
        dias = a.Ventana.Dias,

        // De qué versión salió. Un indicador que no dice contra qué ventana se midió no se
        // puede reproducir ni discutir.
        origen = a.Ventana.Origen,
    },

    sinVentana = a.SinVentana is null ? null : new
    {
        clave = a.SinVentana.Clave,
        porQueNo = a.SinVentana.PorQueNo,
    },

    // El indicador que expone el cierre apurado. `Veces` va nulo cuando no hay con qué
    // comparar: decir «infinito» sería inventar el hallazgo. Y el indicador **entero** va nulo
    // cuando no hay ventana contra la que medirlo.
    apuro = a.Apuro is null ? null : new
    {
        cerradasEnLaVentana = a.Apuro.CerradasEnLaVentana,
        cerradasEnElAnio = a.Apuro.CerradasEnElAnio,
        diasDeLaVentana = a.Apuro.DiasDeLaVentana,
        promedioDiarioEnLaVentana = a.Apuro.PromedioDiarioEnLaVentana,
        promedioDiarioDelAnio = a.Apuro.PromedioDiarioDelAnio,
        veces = a.Apuro.Veces,
    },

    // De donde salieron las dos fechas de corte. Un acta producida las toma del parametro; una
    // vista previa con fechas impuestas lo dice, para que no se confunda con el cierre real.
    origenDeLosCortes = a.OrigenDeLosCortes,

    observaciones = a.Observaciones,
};

static object ResumirSaldo(SaldoDeApertura s) => new
{
    id = s.Id.ToString(),
    folio = s.Folio,
    ejercicio = s.Ejercicio,
    corte = s.Corte,
    produce = s.Produce.Persona.Valor,
    momento = s.Momento,
    renglones = s.Renglones.Count,

    // Los que ya venían de saldos anteriores son los que más importan: el arrastre es
    // justamente lo que la regla existe para hacer visible.
    arrastrados = s.Arrastrados.Count,

    antiguedadMaximaEnDias = s.AntiguedadMaximaEnDias,
    montoTotal = s.MontoTotal,
    bloqueantes = s.Bloqueantes.Count,

    // El primero tras el despliegue se declara para que no se compare contra los siguientes
    // como si fueran la misma medición.
    esInicialDeImplantacion = s.EsInicialDeImplantacion,
};

static object ResumirHallazgo(ExpedienteDeHallazgoPosterior h) => new
{
    id = h.Id.ToString(),
    tipo = h.Tipo,
    fechaDelHecho = h.FechaDelHecho,
    fechaDelDescubrimiento = h.FechaDelDescubrimiento,
    antiguedadEnDias = h.AntiguedadEnDias(DateOnly.FromDateTime(DateTime.UtcNow)),
    diasHastaElDescubrimiento = h.DiasHastaElDescubrimiento,
    comoSeDescubrio = h.ComoSeDescubrio,
    fuente = h.Fuente,
    documentoAdjunto = h.DocumentoAdjunto,

    // Cero es un caso previsto: el paso de un domingo sin orden. **La ausencia de misión es
    // el hallazgo.**
    misiones = h.Misiones.Select(m => m.ToString()),

    vehiculo = h.Vehiculo?.ToString(),
    motorista = h.Motorista?.ToString(),
    periodo = h.Periodo,
    abierto = h.EstaAbierto,
    resolucion = h.Resolucion?.ToString(),
    fundamento = h.Fundamento,
    reversos = h.Reversos.Count,
    efectoEconomicoTotal = h.EfectoEconomicoTotal,
};

static object Resumir(ObligacionDeReintegro o) => new
{
    id = o.Id.ToString(),
    direccion = o.Direccion.ToString(),
    causa = o.Causa.ToString(),
    responsable = o.Responsable.ToString(),
    estado = o.Estado.ToString(),
    monto = o.Monto,
    pagado = o.Pagado,
    saldo = o.Saldo,
    abierta = o.EstaAbierta,
    fechaDelHecho = o.FechaDelHecho,
    antiguedadEnDias = o.AntiguedadEnDias(DateOnly.FromDateTime(DateTime.UtcNow)),
    mision = o.Mision?.ToString(),
    asignacion = o.Asignacion?.ToString(),
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

/// `T-02` — enviar a autorizacion. **Congela el contenido y emite el folio** (`HU-004`).
///
/// Ya no usa el helper generico por lo mismo que `T-14` y `T-19`: la transicion hace algo mas
/// que mover el estado. Aca son dos cosas, y las dos son de auditoria.
///
/// ── Por que el folio se emite al ENVIAR ─────────────────────────────────────
/// Antes de esto es un borrador que su autor puede descartar, y darle folio gastaria numeros
/// del rango en cosas que nunca existieron — dejando huecos que despues hay que explicar uno
/// por uno ante el auditor.
misiones.MapPost("/{id}/enviar", async (
    string id, EjecutarTransicion peticion,
    ServicioDeMisiones servicio, ServicioDeFolios folios, SigtiDbContext contexto) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var fila = await contexto.Expedientes.SingleOrDefaultAsync(e => e.Id == ulid);

    if (fila is null)
        return Results.NotFound(new { mensaje = $"No existe el expediente {id}." });

    var estado = await servicio.TransicionarAsync(
        ulid,
        expediente => expediente.Enviar(new IdPersona(peticion.Ejecuta), peticion.Momento),
        peticion.Momento);

    // ── El congelamiento ────────────────────────────────────────────────────
    // Se calcula DESPUES de que la transicion paso: si `T-02` se rechaza, no hay nada que
    // congelar y guardar la huella dejaria un expediente en borrador con marca de sometido.
    fila.HuellaCongelada = ReglasDelCongelamiento.Huella(new ContenidoSometido(
        fila.Dependencia, fila.ObjetoDelTraslado, fila.Destino, fila.SolicitanteDeDerecho,
        fila.Salida, fila.Retorno, fila.HoraDeSalida, fila.HoraDeRetorno, fila.HolguraDias));

    // ── El folio ────────────────────────────────────────────────────────────
    // **Nulo no bloquea**: hoy ninguna delegacion tiene rango asignado, y negar el envio a
    // todas dejaria el sistema inoperante por una configuracion que nadie cargo. Sin rango se
    // sigue con el provisional, y la pantalla dice que lo es. Lo que si bloquea es el rango
    // AGOTADO — ahi hay rango, se acabo, y emitir fuera de el pisaria el de otra delegacion.
    var folio = await folios.EmitirAsync(
        fila.Dependencia, ServicioDeFolios.TipoOrdenDeMision,
        DateOnly.FromDateTime(peticion.Momento.UtcDateTime));

    if (folio is not null)
    {
        fila.FolioRangoId = folio.RangoId;
        fila.FolioNumero = folio.Numero;
        fila.FolioTexto = folio.Texto;
    }

    await contexto.SaveChangesAsync();

    return Results.Ok(new
    {
        id,
        estado = estado.ToString(),
        huella = fila.HuellaCongelada,

        // Nulos cuando la delegacion no tiene rango, o cuando no hay plantilla configurada
        // (insumo #34). La pantalla los distingue del folio real.
        folio = fila.FolioTexto,
        folioNumero = fila.FolioNumero,
    });
});
/// `T-05` — aprobar. **Coteja el congelamiento antes de dejar pasar** (`HU-004`).
///
/// Deja de usar el helper generico porque hace algo mas que mover el estado, y ese algo es la
/// razon de ser del congelamiento: **quien autoriza tiene que autorizar el contenido que se
/// sometio**, no una version editada despues.
///
/// El cotejo va ANTES de la transicion. Si fuera despues, la aprobacion ya estaria asentada
/// cuando se detecta la alteracion, y habria que reversarla — dejando en el diario un acto de
/// autoridad que nunca debio existir.
misiones.MapPost("/{id}/aprobar", async (
    string id, EjecutarTransicion peticion,
    ServicioDeMisiones servicio, SigtiDbContext contexto) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var fila = await contexto.Expedientes.AsNoTracking().SingleOrDefaultAsync(e => e.Id == ulid);

    if (fila is null)
        return Results.NotFound(new { mensaje = $"No existe el expediente {id}." });

    // **Nulo no bloquea**: son los expedientes anteriores al congelamiento, y negarles la
    // autorizacion dejaria trabajo legitimo detenido por una funcion que no existia cuando se
    // capturaron. Se autoriza, y el cotejo lo declara — que es lo que el auditor necesita ver.
    ReglasDelCongelamiento.ExigirIntacto(
        fila.HuellaCongelada,
        new ContenidoSometido(
            fila.Dependencia, fila.ObjetoDelTraslado, fila.Destino, fila.SolicitanteDeDerecho,
            fila.Salida, fila.Retorno, fila.HoraDeSalida, fila.HoraDeRetorno, fila.HolguraDias));

    var estado = await servicio.TransicionarAsync(
        ulid,
        expediente => expediente.Aprobar(
            new IdPersona(peticion.Ejecuta), peticion.Momento, peticion.Motivo),
        peticion.Momento);

    return Results.Ok(new { id, estado = estado.ToString() });
});
// `T-14` y `T-18` llevan ODOMETRO, y por eso no usan el helper genérico: es el único ancla
// que el sistema tiene para detectar consumo de combustible sin relación con el uso, y el
// hallazgo típico del Tribunal Superior de Cuentas en flota es exactamente ése.
//
// La lectura de referencia se busca por VEHÍCULO y cruza misiones: un odómetro que
// retrocede entre dos misiones distintas es lo que `BD-05` existe para detectar.
ConOdometro("iniciar-ruta", (e, quien, cuando, o, captura, _, __, nivel, razon) =>
    e.IniciarRuta(quien, cuando,
                  new OdometroAlSalir(o.Lectura, o.UltimaConocida, nivel, razon), captura));

ConOdometro("retornar", (e, quien, cuando, o, captura, subtipo, justificacion, nivel, razon) =>
    e.Retornar(quien, cuando,
               new OdometroAlRetornar(o.Lectura, subtipo, justificacion, null, nivel, razon),
               captura));
// `T-19` ya no usa el helper genérico: **`INV-34` exige que todas las asignaciones de
// combustible estén liquidadas**, y ese recuento no está en el expediente. Dejarlo en el
// helper significaba pasar nulo, y nulo es «no evaluada»: la regla quedaría escrita y sin
// ejecutar, que es exactamente lo que se está cerrando.
/// `T-20` — liquidar, **con el control bloqueante de §5.3.B**.
///
/// Es la transición donde convergen cinco pares —`I-04`, `I-07`, `I-09`, `I-10` e `I-11`— y
/// hasta ahora **ninguno bloqueaba acá**: `BD-01` cubre la autorización de la misión y nada mas.
/// El caso que esto impide es el clasico: **quien condujo liquidando su propia mision**.
misiones.MapPost("/{id}/liquidar", async (
    string id, EjecutarTransicion peticion, HttpContext http,
    ServicioDeMisiones servicio, ServicioDeCombustible combustible,
    ServicioDeSegregacion segregacion, ConsultaDeConductores padron) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var recuento = await combustible.RecuentoDeLaMisionAsync(ulid);

    var expediente = await servicio.BuscarAsync(ulid)
        ?? throw new ExpedienteNoEncontrado(ulid);

    // Quien conduce. **Nulo es «no se pudo resolver», no «nadie conduce»**: sin el, `I-11`
    // —nucleo irreductible, el vector de fraude clasico en combustible— no se evalua.
    var recursos = expediente.Diario.LastOrDefault(t => t.Recursos is not null)?.Recursos;

    IdPersona? conductor = null;

    if (recursos is not null)
    {
        var enElPadron = (await padron.TodosAsync())
            .FirstOrDefault(c => c.Id == recursos.Conductor);

        // La identidad de PERSONA, no la del registro de conductor: `I-11` compara personas.
        // Mientras `M-05` no traiga el vinculo con Talento Humano, el nombre es lo que hay —y
        // es lo mismo que ya usan `BD-13` y la custodia.
        if (enElPadron is not null) conductor = new IdPersona(enElPadron.Nombre);
    }

    // Quien entrego el fondo vive en `M-09`, que la mision no conoce. `V-02` es la entrega.
    var entrego = (await combustible.DeLaMisionAsync(ulid))
        .Select(v => v.QuienHizo("V-02"))
        .FirstOrDefault(e => e is not null);

    await segregacion.ExigirAsync(
        new IdPersona(peticion.Ejecuta),
        Funcion.Liquida,
        ActosDeLaMision.De(expediente, id, conductor, entrego),
        id,
        peticion.Momento,

        // De donde vino el intento — §5.3.B.2 lo enumera entre los siete datos.
        origen: http.Connection.RemoteIpAddress?.ToString());

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

/// `PT-084` — **qué habilita cada categoría de licencia, sobre la flota real**.
///
/// ── Por qué esto vive en el servidor ────────────────────────────────────────
/// La matriz licencia↔vehículo es la que sostiene `BD-02`, y `BD-02` traslada responsabilidad
/// legal directa a quien autoriza. Que el cliente la derive por su cuenta —aunque sea «sólo para
/// mostrar»— produce **dos implementaciones de la misma precondición**, y la que se ve en pantalla
/// sería la que nadie verifica. El comentario de `flota.ts` ya lo dice para la evaluación: acá
/// vale igual para el catálogo.
///
/// ── Por qué contra la flota y no contra vehículos de muestra ────────────────
/// La primera versión evaluaba la matriz contra fichas técnicas inventadas acá, y **mintió**:
/// declaró que `B1` y `C1` no habilitaban nada, cuando las dos tienen entrada en la matriz —
/// lo que faltaba era un triciclo y un camión liviano entre las muestras. Un catálogo que
/// depende de qué ejemplos se le ocurrieron a quien lo escribió no es un catálogo.
///
/// Contra la flota la respuesta es la que se necesita de verdad: *«con una licencia B, ¿cuáles de
/// nuestras unidades puedo conducir?»*. Y cuando la institución no tiene ningún vehículo de una
/// clase, se dice eso —no «la categoría no habilita nada», que es falso.
///
/// **Se evalúa a una fecha**, como todo acá: la matriz es parámetro con vigencia, y preguntar qué
/// habilita la B sin decir cuándo no tiene una sola respuesta.
app.MapGet("/matriz-de-licencias", async (
    IParametrosDeLaInstitucion parametros, ConsultaDeFlota flota, DateOnly? fecha) =>
{
    var alDia = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var matriz = parametros.MatrizVigenteAl(alDia);
    var ahora = DateTimeOffset.UtcNow;
    var vehiculos = await flota.TodosAsync();

    return Results.Ok(new
    {
        fecha = alDia,
        version = matriz.Version,

        // Las nueve categorías del Artículo 4. **BE es B enganchada a remolque, y no existe
        // ninguna DE**: el enum es la autoridad y no se recorta acá.
        categorias = Enum.GetValues<CategoriaDeLicencia>().Select(c => new
        {
            categoria = c.ToString(),
            habilita = vehiculos
                .Where(v => matriz.Habilita(c, v.Ficha(), alDia, ahora))
                .Select(v => new { id = v.Id.ToString(), siglas = v.Siglas, tipo = v.TipoDeVehiculo }),
        }),

        // Para poder decir «ningún vehículo de la flota es de esa clase» en vez de dejar que se
        // lea como «la categoría no habilita nada».
        clasesEnLaFlota = vehiculos.Select(v => v.Clase.ToString()).Distinct(),
        vehiculosEnLaFlota = vehiculos.Count,
    });
});

// ── M-14 · La auditoria de ACT-12 ───────────────────────────────────────────
//
// Su ficha: *«Verificar. Nada mas y nada menos. Revisar la cadena solicitud → autorizacion →
// orden de mision → bitacora → vale → comprobante → liquidacion»*. Y su limite absoluto: **solo
// lectura y exportacion**, sin excepciones — por eso este grupo no tiene ningun `MapPost`.
var auditoria = app.MapGroup("/auditoria");

/// `PT-089` — el rastro de un expediente **con sus huecos visibles**.
///
/// Un rastro que solo muestra lo que esta no sirve para auditar: lo que el TSC busca es **donde
/// se corto la cadena**. Y distingue cuatro estados, no dos: «falta» y «no correspondia» se ven
/// iguales en una casilla vacia y son opuestos.
auditoria.MapGet("/expediente/{id}", async (
    string id, ServicioDeMisiones misiones, ServicioDeCombustible combustible) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var expediente = await misiones.BuscarAsync(ulid);

    if (expediente is null)
        return Results.NotFound(new { mensaje = $"No existe el expediente {id}." });

    var vales = await combustible.DeLaMisionAsync(ulid);
    var cadena = ArmadoDeLaCadena.De(expediente, id, vales);

    return Results.Ok(new
    {
        expediente = cadena.Expediente,
        folio = cadena.Folio,
        estado = expediente.Estado.ToString(),

        // **Completa exige que no haya huecos Y que no quede nada pendiente**: una mision en
        // curso no tiene huecos y tampoco esta completa.
        completa = cadena.Completa,
        huecos = cadena.Huecos.Count,
        noAplican = cadena.NoAplican,

        eslabones = cadena.Eslabones.Select(e => new
        {
            eslabon = e.Eslabon.ToString(),
            estado = e.Estado.ToString(),

            // Nulos cuando el eslabon no esta: inventar un autor para algo que no ocurrio es la
            // peor forma de llenar un reporte de auditoria.
            referencia = e.Referencia,
            quien = e.Quien?.Valor,
            fecha = e.Fecha,

            // Nulo cuando esta presente: un motivo ahi seria ruido.
            porQue = e.PorQue,
        }),
    });
});

/// `PT-088` — la pista de auditoria transversal.
///
/// **No es el diario de un expediente**: es qué paso en la institucion. Por eso junta las tres
/// fuentes que hoy existen y **declara las que no**, en vez de mostrar una lista incompleta que
/// se lea como completa.
auditoria.MapGet("/", async (SigtiDbContext contexto, DateOnly? desde, DateOnly? hasta) =>
{
    var inicio = desde ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30).Date);
    var fin = hasta ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

    if (fin < inicio)
    {
        return Results.BadRequest(new
        {
            mensaje = "El rango esta invertido. Devolver cero asientos lo haria pasar por «no " +
                      "hubo actividad», que es lo contrario de lo que una pista tiene que decir.",
        });
    }

    var d = inicio.ToDateTime(TimeOnly.MinValue);
    var h = fin.ToDateTime(TimeOnly.MaxValue);

    var intentos = await contexto.IntentosBloqueados.AsNoTracking()
        .Where(i => i.MomentoUtc >= d && i.MomentoUtc <= h).ToListAsync();

    var tareas = await contexto.Tareas.AsNoTracking()
        .Where(t => t.MomentoUtc >= d && t.MomentoUtc <= h).ToListAsync();

    var parametros = await contexto.Parametros.AsNoTracking()
        .Where(v => v.RegistradoDesde >= new DateTimeOffset(d, TimeSpan.Zero)
                 && v.RegistradoDesde <= new DateTimeOffset(h, TimeSpan.Zero))
        .ToListAsync();

    return Results.Ok(new
    {
        desde = inicio,
        hasta = fin,

        // Las tres fuentes que la ficha de `ACT-12` enumera y que **hoy existen**.
        intentosBloqueados = intentos.Count,
        tareasEscaladas = tareas.Count,
        cambiosDeParametros = parametros.Count,

        // **Y las que NO.** Una pista que enumera tres fuentes sin decir que faltan dos se lee
        // como completa, y `ACT-12` concluiria que no hubo actos en regimen de excepcion cuando
        // lo que pasa es que nadie los registra.
        fuentesQueFaltan = new[]
        {
            "Los actos ejecutados en régimen de excepción: DP-002 suspendió el Nivel 2 y no hay " +
            "registro de régimen en el sistema.",
            "El diario de cada expediente por separado — se consulta por expediente en PT-089, " +
            "no hay vista transversal de transiciones.",
        },

        asientos = intentos
            .Select(i => new
            {
                momento = i.MomentoUtc,
                tipo = "Intento bloqueado",
                quien = i.Quien,
                detalle = $"{i.Par}: quiso {i.Pretendia} sobre {i.Expediente}",
            })
            .Concat(tareas.Select(t => new
            {
                momento = t.MomentoUtc,
                tipo = "Tarea escalada",
                quien = t.QuienLaOrigino,
                detalle = t.Asunto,
            }))
            .Concat(parametros.Select(v => new
            {
                momento = v.RegistradoDesde.UtcDateTime,
                tipo = "Cambio de parámetro",
                quien = v.CargadoPor.Valor,
                detalle = $"{v.Clave} = {v.Valor}, vigente desde {v.VigenteDesde:dd/MM/yyyy}",
            }))
            .OrderByDescending(a => a.momento),
    });
});

/// `PT-092` — el historico de parametros normativos **con su vigencia**.
///
/// Las dos parejas de fechas de `ADR-006` van completas: la normativa dice **desde cuando
/// regia**, y la de transaccion **desde cuando lo supimos**. Mostrar solo la primera impediria
/// explicar por que una liquidacion vieja uso otro numero.
auditoria.MapGet("/parametros", async (SigtiDbContext contexto, string? clave) =>
{
    var versiones = await contexto.Parametros.AsNoTracking()
        .Where(v => clave == null || v.Clave == clave)
        .OrderBy(v => v.Clave).ThenByDescending(v => v.VigenteDesde)
        .ToListAsync();

    return Results.Ok(versiones.Select(v => new
    {
        id = v.Id.ToString(),
        clave = v.Clave,
        valor = v.Valor,

        // Eje NORMATIVO: desde cuándo regía.
        vigenteDesde = v.VigenteDesde,
        vigenteHasta = v.VigenteHasta,

        // Eje de TRANSACCION: desde cuándo lo supimos. **Nulo en registradoHasta es «sigue
        // siendo lo que creemos»**, no «se dejó de creer».
        registradoDesde = v.RegistradoDesde,
        registradoHasta = v.RegistradoHasta,

        cargadoPor = v.CargadoPor.Valor,

        // **Nulo es «sin aprobar», y una versión sin aprobar no resuelve**: el doble control de
        // `HU-145` sería decorativo si el valor rigiera igual.
        aprobadoPor = v.AprobadoPor == null ? null : v.AprobadoPor.Value.Valor,
        estaAprobada = v.AprobadoPor != null,

        respaldo = new { fuente = v.Respaldo.Fuente, verificadoEl = v.Respaldo.FechaDeVerificacion },
    }));
});

// ── M-01 · La bandeja de tareas pendientes ──────────────────────────────────
//
// §5.3.B.3: *«el sistema encola la accion como pendiente de resolucion»*, y la mision *«queda
// visiblemente pendiente en la bandeja de alguien»*.
//
// **La bandeja es el sistema de registro y el aviso es una cortesia.** Un correo que no llega
// deja el trabajo perdido; una bandeja que se abre al entrar no depende de que haya red, correo
// ni telefono — y esto se despliega on-premise donde nada de eso esta garantizado.
var tareas = app.MapGroup("/tareas");

tareas.MapGet("/", async (ServicioDeTareas servicio, SigtiDbContext contexto) =>
{
    var todas = await servicio.TodasAsync();

    // Los intentos de aviso, por tarea. **Se guarda el intento y no solo el exito**: sin eso,
    // un aviso perfecto y uno que nunca se intento se ven exactamente igual.
    var avisos = await contexto.Avisos.AsNoTracking().ToListAsync();
    var ahora = DateTimeOffset.UtcNow;

    var pendientes = todas.Where(t => t.Estado == EstadoDeTarea.Pendiente).ToList();

    return Results.Ok(new
    {
        pendientes = pendientes.Count,

        // **Ninguna se aviso**, porque no hay canal. Se publica el numero para que la pantalla
        // lo pueda decir en vez de dejar que una bandeja llena se lea como gente que ignora su
        // trabajo.
        sinAvisar = pendientes.Count(t => t.NotificadoUtc is null),

        // Lo que lleva mas esperando. Nulo cuando no hay pendientes — que no es cero dias.
        diasDeLaMasVieja = pendientes.Count == 0
            ? (int?)null
            : Math.Max(0, pendientes.Max(t => (ahora.Date - t.MomentoUtc.Date).Days)),

        tareas = todas.Select(t => new
        {
            id = t.Id.ToString(),
            tipo = t.Tipo.ToString(),
            asunto = t.Asunto,
            detalle = t.Detalle,
            expediente = t.Expediente,
            quienLaOrigino = t.QuienLaOrigino,

            // Nulo es Gerencia Administrativa: el ultimo recurso del escalamiento no es un
            // puesto de la jerarquia de quien quedo bloqueado.
            puestoDestino = t.PuestoDestino,
            personasDestino = t.PersonasDestino,

            momento = t.MomentoUtc,
            estado = t.Estado.ToString(),

            // **Nulo es «no se aviso»**, no «se aviso y no contestaron».
            notificado = t.NotificadoUtc,

            // Por que no salio, si no salio. Tres razones distintas que arreglan personas
            // distintas: la institucion eligiendo el canal, quien programa construyendolo, o
            // quien opera la infraestructura.
            avisos = avisos.Where(a => a.Tarea == t.Id).Select(a => new
            {
                destinatario = a.Destinatario,

                // Nulo es «la institucion no fijo el canal» —insumo #102—, que no es lo mismo
                // que un canal que fallo.
                canal = a.Canal,
                resultado = a.Resultado,
                detalle = a.Detalle,
            }),

            resuelve = t.Resuelve,
            resuelta = t.ResueltaUtc,
            resolucion = t.Resolucion,

            diasEsperando = t.Estado == EstadoDeTarea.Pendiente
                ? Math.Max(0, (ahora.Date - t.MomentoUtc.Date).Days)
                : 0,
        }),
    });
});

/// Cierra una tarea. **Quien la origino no puede**: el escalamiento la puso en otra bandeja
/// justamente para que decida otra persona.
tareas.MapPost("/{id}/cerrar", async (
    string id, CerrarTarea peticion, ServicioDeTareas servicio) =>
{
    await servicio.CerrarAsync(
        Ulid.Parse(id), new IdPersona(peticion.Ejecuta), peticion.Motivo,
        peticion.Descartar, peticion.Momento);

    return Results.Ok(new { id, descartada = peticion.Descartar });
});

// ── M-01 · Intentos bloqueados por segregacion ──────────────────────────────
//
// `PT-091`. §5.3.B.2: **el intento bloqueado es informacion de control, no ruido**. Un sistema
// que solo guarda lo que se consumo no puede contestar si el control opero: un bloqueo perfecto
// y uno que nunca se activo se ven exactamente igual.
app.MapGet("/segregacion/intentos", async (ServicioDeSegregacion servicio) =>
{
    var todos = await servicio.TodosAsync();

    return Results.Ok(new
    {
        total = todos.Count,

        // **La reincidencia es lo que Auditoria mira.** «Un mismo usuario intentando quince
        // veces autorizar sus propias solicitudes es exactamente lo que quiere ver.»
        reincidentes = todos
            .GroupBy(i => i.Quien)
            .Where(g => g.Count() > 1)
            .Select(g => new { persona = g.Key, intentos = g.Count() })
            .OrderByDescending(x => x.intentos),

        porPar = todos
            .GroupBy(i => i.Par)
            .Select(g => new { par = g.Key, intentos = g.Count() })
            .OrderByDescending(x => x.intentos),

        intentos = todos.Select(i => new
        {
            id = i.Id.ToString(),
            quien = i.Quien,
            pretendia = i.Pretendia,
            expediente = i.Expediente,
            par = i.Par,
            chocaCon = i.ChocaCon,
            referencia = i.Referencia,
            momento = i.MomentoUtc,

            // §5.3.B.3 — donde quedo pendiente. **Nulo es «no se resolvio»**, no «fue a
            // Gerencia»: los intentos anteriores al escalamiento existen, y decir que fueron al
            // ultimo recurso seria inventar un encaminamiento que nunca ocurrio.
            salto = i.Salto,
            escalaA = i.EscalaA,
            porQueNoAntes = i.PorQueNoAntes,

            // Nulo es «no se supo», no «desde el servidor».
            origen = i.Origen,
        }),
    });
});

// ── M-01 · Competencias del puesto ──────────────────────────────────────────
//
// **La mitad de M-01 que SI es de SIGTI.** `DP-001` y `RN-48`: la estructura de puestos y quien
// los ocupa son propiedad de ARGOS y Talento Humano, y viven como espejo sin escritura. Que
// facultades tiene cada puesto DENTRO de SIGTI no es de ARGOS —no sabe que es despachar un
// vehiculo— y por eso esta mitad se administra aca.
var competencias = app.MapGroup("/competencias");

/// `PT-096` — el padron de puestos con sus competencias vigentes, y quien los ocupa.
///
/// Une las dos mitades: el espejo dice quien ocupa que, esta tabla dice que puede hacer cada
/// puesto, y la union por persona es lo que `RN-100` llama permisos efectivos.
competencias.MapGet("/", async (
    ServicioDeCompetencias servicio, ConsultaDelOrganigrama organigrama, DateOnly? fecha) =>
{
    var alDia = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var todas = await servicio.TodasAsync();
    var mapa = await organigrama.VigenteAsync();

    return Results.Ok(todas
        .GroupBy(c => c.Puesto)
        .Select(g =>
        {
            var puesto = new IdPuesto(g.Key);
            var ocupantes = mapa.QuienesOcupan(puesto, alDia);

            var vigentes = g
                .Where(c => c.Desde <= alDia && (c.Hasta is null || alDia <= c.Hasta))
                .ToList();

            return new
            {
                puesto = g.Key,

                // **Vacante no es «sin ocupantes por error»**: el puesto existe aunque este
                // vacio, y por eso se le pueden configurar competencias antes del nombramiento.
                ocupantes = ocupantes.Select(o => o.Valor),
                vacante = ocupantes.Count == 0,

                competencias = vigentes.Select(c => new
                {
                    id = c.Id.ToString(),
                    rol = c.Rol.ToString(),
                    alcance = c.Alcance.ToString(),
                    desde = c.Desde,
                    hasta = c.Hasta,
                    otorga = c.Otorga,

                    // Nulo es «no quedo vigilada», no «no se evaluo».
                    paresVigilados = c.ParesVigilados,
                }),

                // **«Ya no rige» y «todavia no empieza» son cosas opuestas**, y las dos llegan
                // como «no vigente». Una es historia y la otra es una asignacion programada que
                // alguien espera que entre a funcionar; contarlas juntas haria que un
                // otorgamiento futuro se leyera como una competencia que se quito.
                cerradas = g.Count(c => c.Hasta is not null && c.Hasta < alDia),
                futuras = g.Count(c => c.Desde > alDia),
            };
        }));
});

/// Lo que una persona puede hacer a una fecha — **la union de todos sus puestos**.
///
/// `RN-100`: los permisos efectivos son la union de los roles de todos los puestos que la
/// persona ocupa vigentes a esa fecha. Y por eso las incompatibilidades se evaluan aca y no
/// puesto por puesto: mirar puesto por puesto es como se cuela la acumulacion.
competencias.MapGet("/persona/{persona}", async (
    string persona, ServicioDeCompetencias servicio, DateOnly? fecha) =>
{
    var alDia = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var suyas = await servicio.DeLaPersonaAsync(new IdPersona(persona), alDia);
    var efecto = ReglasDeLaAsignacion.Evaluar(suyas.Roles);

    return Results.Ok(new
    {
        persona,
        fecha = alDia,
        puestos = suyas.Puestos.Select(p => p.Valor),
        roles = suyas.Roles.Select(r => r.ToString()),
        funciones = Incompatibilidades.FuncionesDeTodos(suyas.Roles).Select(f => f.ToString()),

        // **Nulo es «no tiene alcance», que no es `Propio`**: `Propio` ya es un permiso.
        alcanceMaximo = suyas.AlcanceMaximo?.ToString(),

        // Una persona sin puesto vigente es un usuario sin permisos. No se borra: sus actos
        // historicos la referencian.
        sinCompetencia = suyas.SinCompetencia,

        // La acumulacion que quedo latente. Bloquea al ejecutar sobre un expediente concreto,
        // no aca — por eso se muestra como vigilancia y no como impedimento.
        vigilados = efecto.Vigilados.Select(v => new
        {
            par = v.Id,
            una = v.Una.ToString(),
            otra = v.Otra.ToString(),
            nivel = v.Nivel.ToString(),
            porQue = v.PorQue,
        }),
        quedaVigilada = efecto.QuedaVigilada,
    });
});

/// `PT-097` — otorgar un rol a un puesto, **con el control preventivo de §5.3.A**.
///
/// Rechaza si produce una acumulacion absoluta (`I-12`, `I-13`) en cualquiera de sus
/// ocupantes; si la acumulacion es solo por expediente, la guarda y deja constancia de los
/// pares vigilados. No se puede prohibir de entrada que el Encargado de Delegacion sea tambien
/// Solicitante: seria inoperante.
competencias.MapPost("/", async (
    OtorgarCompetencia peticion, ServicioDeCompetencias servicio) =>
{
    var efecto = await servicio.OtorgarAsync(
        Ulid.Parse(peticion.Id),
        new IdPuesto(peticion.Puesto),
        peticion.Rol,
        peticion.Alcance,
        peticion.Desde,
        peticion.Hasta,
        new IdPersona(peticion.Otorga));

    return Results.Created($"/competencias/{peticion.Id}", new
    {
        id = peticion.Id,
        quedaVigilada = efecto.QuedaVigilada,
        vigilados = efecto.Vigilados.Select(v => new { par = v.Id, porQue = v.PorQue }).Distinct(),
    });
});

/// Cierra una competencia. **No la borra**: un acto de febrero se juzga con la competencia
/// vigente en febrero, y una fila borrada dejaria ese expediente sin respaldo.
competencias.MapPost("/{id}/cerrar", async (
    string id, CerrarCompetencia peticion, ServicioDeCompetencias servicio) =>
{
    await servicio.CerrarAsync(Ulid.Parse(id), peticion.Hasta);
    return Results.Ok(new { id, hasta = peticion.Hasta });
});

/// El catalogo que la pantalla necesita para ofrecer opciones: los roles, los alcances y la
/// tabla de incompatibilidades **con su nivel**.
///
/// Va del servidor porque la tabla es la autoridad transcrita de `actores-y-roles.md`, y una
/// copia en el cliente diria que `I-14` bloquea el dia que alguien la actualice a medias.
competencias.MapGet("/catalogo", () => Results.Ok(new
{
    roles = Enum.GetValues<Rol>().Select(r => new
    {
        rol = r.ToString(),
        funciones = Incompatibilidades.Funciones(r).Select(f => f.ToString()),
    }),
    alcances = Enum.GetValues<AlcanceDeDatos>().Select(a => a.ToString()),
    incompatibilidades = Incompatibilidades.Tabla
        .GroupBy(p => p.Id)
        .Select(g => new
        {
            par = g.Key,
            nivel = g.First().Nivel.ToString(),
            alcance = g.First().Alcance.ToString(),
            porQue = g.First().PorQue,
            funciones = g.Select(p => new { una = p.Una.ToString(), otra = p.Otra.ToString() }),
        }),
}));

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
    string id, DeclararEstado peticion, EstadoDeLaFlota flota, ConsultaDeFlota padron,
    ServicioDeTitulos titulos) =>
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
        return Results.Conflict(new
        {
            // El identificador tiene que ser el MISMO que el dominio usa, o el camino de
            // salida de `R-3` no se encuentra: aca decia "10.2" y el dominio emite "§10.2".
            precondicion = ReglasDelEstadoOperativo.PrecondicionDeSeccion,
            mensaje = invalido.Message,
            salida = Salida(ReglasDeLaSalida.De(ReglasDelEstadoOperativo.PrecondicionDeSeccion)),
        });
    }

    if (string.IsNullOrWhiteSpace(peticion.Motivo))
        return Results.BadRequest(new
        {
            mensaje = "Declare el motivo: §10.2 pide causa tipificada para NO_DISPONIBLE y " +
                      "acta para el préstamo y los estados terminales.",
        });

    // `RN-62` + `HB3-17` — si el bien es del Estado decide cual de los dos terminales
    // corresponde: el descargo extingue un bien propio, el retiro devuelve uno ajeno. Nulo es
    // «el vehiculo no tiene titulo registrado», y entonces se advierte en vez de juzgar.
    var esBienPropio = await titulos.EsBienPropioAsync(
        idVehiculo, DateOnly.FromDateTime(peticion.Momento.UtcDateTime));

    var advertencia = await flota.AnotarAsync(
        idVehiculo,
        new CambioDeEstadoOperativo(
            peticion.Estado, peticion.Momento, peticion.Ejecuta, peticion.Motivo,
            Automatico: false),
        esBienPropio);

    await flota.ConfirmarAsync();

    return Results.Ok(new { id, estado = peticion.Estado.ToString(), advertencia });
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

/// `F-02` — aprobar el fondo, **con el control bloqueante de §5.3.B**.
///
/// ── `I-19`, y por que no basta con `RN-26.4` ─────────────────────────────────
/// `RN-26.4` ya impedia que quien solicito aprobara, y **sigue donde esta**: es control propio
/// de `RN-26`, no una aplicacion de `RN-01`, y es la ultima linea si alguien llama al dominio
/// directo. Lo que le faltaba es lo que §5.3.B exige alrededor del bloqueo: **el asiento en la
/// pista de auditoria**, el par nombrado y el destino del escalamiento.
///
/// El fondo es objeto **de periodo**: por eso `I-19` existe como par propio y por eso se le
/// pasan los actos del fondo y no los de ninguna mision.
fondos.MapPost("/{id}/aprobar", async (
    string id, AprobarFondo peticion, HttpContext http,
    ServicioDeCombustible servicio, ServicioDeSegregacion segregacion) =>
{
    var idFondo = Ulid.Parse(id);

    if (await servicio.BuscarFondoAsync(idFondo) is { } fondo)
    {
        // El ambito DECLARADO —«Delegacion de Danli»— y no el enum —«Dependencia»—: quien lee
        // el bloqueo necesita saber cual de sus fondos es, y el enum dice de que tipo es.
        var referencia =
            $"del fondo de {fondo.AmbitoDeclarado}, del {fondo.Desde:dd/MM/yyyy} al {fondo.Hasta:dd/MM/yyyy}";

        await segregacion.ExigirAsync(
            new IdPersona(peticion.Ejecuta),
            Funcion.ApruebaFondo,
            ActosDelFondo.De(fondo, referencia),
            id,
            peticion.Momento,
            origen: http.Connection.RemoteIpAddress?.ToString());
    }

    var estado = await servicio.MoverFondoAsync(
        idFondo,
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

// `RN-83` — **todo** ingreso de combustible al tanque, venga de donde venga.
//
// El del fondo entra por su vale, que además mueve el instrumento. Ésta es la puerta de lo
// demás: el tanque de la sede, la donación en una emergencia, el galón que el motorista puso
// de su bolsillo. Sin ella esos galones no existen, y `RN-30` los echa de menos como si
// fueran fraude.
app.MapPost("/abastecimientos", async (
    RegistrarAbastecimiento peticion, ServicioDeAbastecimientos servicio,
    ServicioDeTanques tanques) =>
{
    var id = await servicio.RegistrarAsync(
        Ulid.Parse(peticion.Id),
        Ulid.Parse(peticion.IdVehiculo),
        peticion.OcurridoEn,
        peticion.Galones,
        peticion.Odometro,
        peticion.Fuente,
        new IdPersona(peticion.Registra),
        peticion.IdMision is null ? null : Ulid.Parse(peticion.IdMision),
        peticion.Monto,
        peticion.Estacion,
        peticion.Comprobante,
        peticion.CausaSinComprobante,
        idDeCaptura: null,

        // **El otro lado de `RN-83` punto 5.** Sin `IdTanque` el abastecimiento se registra
        // igual y queda como discrepancia -- que es lo correcto para el hecho consumado que
        // llega del campo. Con tanque, el despacho descuenta en la misma transacción y aplica
        // sus bloqueos: existencia, segregación y compatibilidad de combustible.
        tanques: tanques,
        tanque: peticion.IdTanque is null ? null : Ulid.Parse(peticion.IdTanque),
        despacha: peticion.PuestoDespacha is null
            ? null
            : Autoria.De(new IdPersona(peticion.Registra), new IdPuesto(peticion.PuestoDespacha),
                DateOnly.FromDateTime(peticion.OcurridoEn.Date)),
        recibe: peticion.IdReceptor is null
            ? null
            : new IdPersonaDelReceptor(peticion.IdReceptor),
        combustibleDelVehiculo: peticion.CombustibleDelVehiculo ?? "");

    return Results.Created($"/abastecimientos/{id}", new { id = id.ToString() });
});

// Los de una misión, con su fuente. Es el desglose que `RN-30` manda mostrar junto a la
// desviación: sin la fuente, cuarenta galones del tanque de la sede y cuarenta comprados se
// leen igual.
app.MapGet("/abastecimientos/mision/{id}", async (
    string id, ServicioDeAbastecimientos servicio) =>
    Results.Ok((await servicio.DeLaMisionAsync(Ulid.Parse(id))).Select(a => new
    {
        id = a.Id.ToString(),
        momento = a.OcurridoEn,
        galones = a.Galones,
        odometro = a.Odometro,
        fuente = a.Fuente.ToString(),
        registra = a.Registra.Valor,
        monto = a.Monto,
        estacion = a.Estacion,
        comprobante = a.Comprobante,
        causaSinComprobante = a.CausaSinComprobante,
        excedido = a.Excedido,

        // Los dos que deciden a qué cuadre pertenece el galón. Van resueltos y no como
        // cadena de la fuente: el cliente no tiene por qué reimplementar `RN-83`.
        entraAlCuadreDelFondo = a.EntraAlCuadreDelFondo,
        generaReintegro = a.GeneraReintegro,

        descripcion = a.Descripcion,
    })));

// ── `RN-95` — la conciliación contra fuentes externas ───────────────────────
// `RN-30` concilia hacia adentro: nuestros datos contra nuestros datos. Eso verifica
// coherencia interna, no veracidad — **un registro completo y coherente puede ser
// completamente falso**, y sólo la fuente externa lo revela.
var conciliacion = app.MapGroup("/conciliacion");

/// El catálogo, con el **retraso de cada fuente como dato visible** (`RN-95` punto 5): una
/// fuente sin conciliar durante meses es en sí misma una observación de control interno.
conciliacion.MapGet("/fuentes", async (ServicioDeConciliacionExterna servicio) =>
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    return Results.Ok((await servicio.FuentesAsync()).Select(f => new
    {
        id = f.Id.ToString(),
        tipo = f.Tipo.ToString(),
        emisor = f.Emisor,
        formato = f.Formato,
        responsable = f.ResponsableDeLaCarga,

        // **No disponible NO es conciliada.** Confundirlas hace que la ausencia de
        // diferencias se lea como conformidad.
        disponible = f.Disponible,
        porQueNoEstaDisponible = f.PorQueNoEstaDisponible,

        periodicidadEnDias = f.PeriodicidadEnDias,

        // Nula significa **nunca conciliada**, que no es cero días de retraso.
        ultimaConciliacion = f.UltimaConciliacion,
        diasDesdeLaUltima = f.DiasDesdeLaUltima(hoy),
        atrasada = f.Atrasada(hoy),
        retraso = f.Retraso(hoy),
    }));
});

conciliacion.MapPost("/fuentes", async (
    RegistrarFuente peticion, ServicioDeConciliacionExterna servicio) =>
{
    var id = await servicio.RegistrarFuenteAsync(
        Ulid.Parse(peticion.Id), peticion.Tipo, peticion.Emisor, peticion.Formato,
        peticion.Responsable, peticion.Disponible, peticion.PeriodicidadEnDias,
        peticion.PorQueNoEstaDisponible);

    return Results.Created($"/conciliacion/fuentes/{id}", new { id = id.ToString() });
});

/// Ejecuta la conciliación. Produce **tres listas** —coincidentes, solo en la fuente, solo en
/// SIGTI— y las dos últimas abren expediente, en ambos sentidos.
conciliacion.MapPost("/ejecutar", async (
    EjecutarConciliacion peticion, ServicioDeConciliacionExterna servicio,
    ServicioDeHallazgosPosteriores hallazgos) =>
{
    var r = await servicio.ConciliarAsync(
        Ulid.Parse(peticion.IdFuente),
        peticion.Desde,
        peticion.Hasta,
        [.. peticion.Lineas.Select(l => new LineaExterna(
            l.Id, l.FechaDelHecho, l.Monto,
            new IdentificacionExterna(
                l.BienDelInventario, l.Chasis, l.Motor, l.Correlativo, l.Placa),
            l.Referencia, l.Descripcion))],
        peticion.DocumentoFuente,
        new IdPersona(peticion.Ejecuta),
        peticion.ResponsableDeSeguimiento,
        peticion.Plazo,
        peticion.Momento,

        // `RN-95`: cada diferencia abre expediente de hallazgo posterior, en ambos sentidos.
        // El expediente es lo que les da ciclo propio, asiento reverso y resolución que no se
        // borra — y lo que impide que se resuelvan reabriendo la misión.
        hallazgos,
        Autoria.De(new IdPersona(peticion.Ejecuta), new IdPuesto(peticion.Puesto ?? "PU-AUDITORIA"),
            DateOnly.FromDateTime(peticion.Momento.Date)),

        peticion.ToleranciaEnDias ?? 1);

    return Results.Ok(new
    {
        coincidentes = r.Coincidentes.Count,
        diferencias = r.Diferencias,
        montoSoloEnLaFuente = r.MontoSoloEnLaFuente,
        montoSoloEnSigti = r.MontoSoloEnSigti,

        // Las que ni siquiera se pudieron atribuir a un vehículo van aparte: no hay a quién
        // preguntarle, hay que ir al proveedor.
        sinVehiculoResuelto = r.SinVehiculoResuelto,

        // `RN-94` — sin corte, dos ejecuciones con datos distintos se ven idénticas.
        fechaDeCorte = r.FechaDeCorte,
        documentoFuente = r.DocumentoFuente,

        soloEnLaFuente = r.SoloEnLaFuente.Select(d => new
        {
            linea = d.Linea.Id,
            fechaDelHecho = d.Linea.FechaDelHecho,
            monto = d.Linea.Monto,
            referencia = d.Linea.Referencia,
            vehiculo = d.Vehiculo.Vehiculo?.ToString(),
            ancla = d.Vehiculo.Ancla?.ToString(),
            explicacion = d.Vehiculo.Explicacion,
        }),

        soloEnSigti = r.SoloEnSigti.Select(d => new
        {
            asiento = d.Asiento.Id.ToString(),
            origen = d.Asiento.Origen,
            fechaDelHecho = d.Asiento.FechaDelHecho,
            monto = d.Asiento.Monto,
            referencia = d.Asiento.Referencia,
            vehiculo = d.Asiento.Vehiculo?.ToString(),
        }),
    });
});

/// Los expedientes que las diferencias abrieron, ordenados por plazo — lo que alguien tiene
/// que resolver antes de que el auditor lo encuentre primero.
conciliacion.MapGet("/diferencias", async (ServicioDeConciliacionExterna servicio) =>
    Results.Ok((await servicio.DiferenciasAbiertasAsync()).Select(d => new
    {
        id = d.Id.ToString(),
        lado = d.Lado.ToString(),
        fechaDelHecho = d.FechaDelHecho,
        monto = d.Monto,
        referencia = d.Referencia,
        origen = d.Origen,
        vehiculo = d.VehiculoId?.ToString(),
        ancla = d.Ancla?.ToString(),
        explicacion = d.Explicacion,
        responsable = d.ResponsableDeSeguimiento,
        plazo = d.Plazo,
    })));

conciliacion.MapGet("/ejecuciones", async (ServicioDeConciliacionExterna servicio) =>
    Results.Ok((await servicio.EjecucionesAsync()).Select(e => new
    {
        id = e.Id.ToString(),
        fuente = e.FuenteId.ToString(),
        desde = e.Desde,
        hasta = e.Hasta,
        documentoFuente = e.DocumentoFuente,
        fechaDeCorte = new DateTimeOffset(e.FechaDeCorteUtc, TimeSpan.Zero)
            .ToOffset(TimeSpan.FromMinutes(e.DesfaseMinutos)),
        ejecuta = e.Ejecuta,
        coincidentes = e.Coincidentes,
        soloEnLaFuente = e.SoloEnLaFuente,
        soloEnSigti = e.SoloEnSigti,
        sinResolver = e.Diferencias.Count(d => d.Resolucion == null),
    })));

conciliacion.MapPost("/diferencias/{id}/resolver", async (
    string id, ResolverDiferencia peticion, ServicioDeConciliacionExterna servicio) =>
{
    await servicio.ResolverAsync(Ulid.Parse(id), peticion.Resolucion, peticion.Momento);
    return Results.Ok(new { resuelta = true });
});

// ── `RN-93` — el expediente de hallazgo posterior ───────────────────────────
// Ni su apertura ni su resolución alteran el objeto vinculado. Una misión `CERRADA` **no se
// reabre**, ni por auditoría: lo que se entrega es el paquete sellado tal como cerró MÁS este
// expediente. Es más información, no menos.
var hallazgosPosteriores = app.MapGroup("/hallazgos");

hallazgosPosteriores.MapGet("/", async (ServicioDeHallazgosPosteriores servicio) =>
    Results.Ok((await servicio.TodosAsync()).Select(ResumirHallazgo)));

/// Los hallazgos de una misión — §7.5: la misión cerrada muestra que los tiene, **sin que eso
/// la modifique**. Se consulta desde acá y no se guarda una marca en el expediente cerrado.
hallazgosPosteriores.MapGet("/mision/{id}", async (
    string id, ServicioDeHallazgosPosteriores servicio) =>
    Results.Ok((await servicio.DeLaMisionAsync(Ulid.Parse(id))).Select(ResumirHallazgo)));

hallazgosPosteriores.MapGet("/{id}", async (
    string id, ServicioDeHallazgosPosteriores servicio) =>
    await servicio.BuscarAsync(Ulid.Parse(id)) is { } h
        ? Results.Ok(new
        {
            resumen = ResumirHallazgo(h),

            // El diario entero. Un expediente que sólo muestra su resolución no sirve: lo que
            // el auditor pide es quién lo abrió, cómo, y qué se asentó en el camino.
            diario = h.Diario.Select(m => new
            {
                movimiento = m.Id,
                persona = m.Autor.Persona.Valor,
                puesto = m.Autor.Puesto.Valor,
                momento = m.Momento,
                motivo = m.Motivo,
            }),

            // **Los tres valores, siempre.** §8.3: nunca sólo el resultado.
            reversos = h.Reversos.Select(r => new
            {
                id = r.Id.ToString(),
                naturaleza = r.Naturaleza.ToString(),
                asientoRevertido = r.Revertido.Identificador,
                tipoDeAsiento = r.Revertido.Tipo,
                descripcion = r.Revertido.Descripcion,
                valorAnterior = r.ValorAnterior,
                valorNuevo = r.ValorNuevo,
                efectoEconomico = r.EfectoEconomico,
                periodoAfectado = r.PeriodoAfectado,
                periodoDeImputacion = r.PeriodoDeImputacion,
                motivo = r.MotivoTipificado,
                fundamento = r.Fundamento,
                autoriza = r.Autoriza.Valor,
                cadena = r.Cadena,
            }),
        })
        : Results.NotFound());

hallazgosPosteriores.MapPost("/", async (
    AbrirHallazgo peticion, ServicioDeHallazgosPosteriores servicio) =>
{
    var id = await servicio.AbrirAsync(
        Ulid.Parse(peticion.Id), peticion.Tipo,
        peticion.FechaDelHecho, peticion.FechaDelDescubrimiento,
        peticion.ComoSeDescubrio, peticion.Fuente, peticion.DocumentoAdjunto,
        [.. (peticion.Misiones ?? []).Select(Ulid.Parse)],
        peticion.IdVehiculo is null ? null : Ulid.Parse(peticion.IdVehiculo),
        peticion.IdMotorista is null ? null : Ulid.Parse(peticion.IdMotorista),
        peticion.Periodo,
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            peticion.FechaDelDescubrimiento),
        peticion.Momento);

    return Results.Created($"/hallazgos/{id}", new { id = id.ToString() });
});

/// `H-03` — el asiento reverso de §8.3, con su contenido obligatorio completo. **El asiento
/// original no se toca**: éste se agrega y se refiere a él.
hallazgosPosteriores.MapPost("/{id}/reverso", async (
    string id, AsentarReverso peticion, ServicioDeHallazgosPosteriores servicio) =>
{
    await servicio.MoverAsync(Ulid.Parse(id), h => h.Revertir(
        new AsientoReverso(
            Ulid.NewUlid(),
            new ReferenciaAlAsiento(
                peticion.TipoDeAsiento, peticion.IdentificadorDelAsiento,
                peticion.DescripcionDelAsiento),
            peticion.Naturaleza,
            peticion.ValorAnterior,
            peticion.ValorNuevo,
            peticion.FechaDelHechoOriginal,
            peticion.Momento,
            Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
                DateOnly.FromDateTime(peticion.Momento.Date)),
            new IdPersona(peticion.Autoriza),
            new IdPersona(peticion.AutorDelAsientoOriginal),
            peticion.MotivoTipificado,
            peticion.Fundamento,
            peticion.Adjunto,
            peticion.PeriodoAfectado,
            peticion.PeriodoDeImputacion,
            peticion.EfectoEconomico,
            peticion.TablasParametricas),
        peticion.Momento));

    return Results.Ok(new { asentado = true });
});

hallazgosPosteriores.MapPost("/{id}/vincular", async (
    string id, VincularMision peticion, ServicioDeHallazgosPosteriores servicio) =>
{
    await servicio.MoverAsync(Ulid.Parse(id), h => h.Vincular(
        Ulid.Parse(peticion.IdMision),
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            DateOnly.FromDateTime(peticion.Momento.Date)),
        peticion.Motivo, peticion.Momento));

    return Results.Ok(new { vinculada = true });
});

/// `H-04` — resolver. **El expediente no se cierra sin resolución**, y la resolución tiene que
/// ser cierta respecto de lo que el expediente contiene.
hallazgosPosteriores.MapPost("/{id}/resolver", async (
    string id, ResolverHallazgo peticion, ServicioDeHallazgosPosteriores servicio) =>
{
    await servicio.MoverAsync(Ulid.Parse(id), h => h.Resolver(
        peticion.Resolucion, peticion.Fundamento,
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            DateOnly.FromDateTime(peticion.Momento.Date)),
        peticion.Momento));

    return Results.Ok(new { resuelto = peticion.Resolucion.ToString() });
});

/// El ajuste imputado a un período — `RN-93` punto 3: **no se recalculan los históricos ya
/// publicados**; se ajusta el corriente y se muestra el ajuste como capa identificada.
hallazgosPosteriores.MapGet("/ajuste/{periodo}", async (
    string periodo, ServicioDeHallazgosPosteriores servicio) =>
    Results.Ok(new { periodo, ajuste = await servicio.AjusteDelPeriodoAsync(periodo) }));

// ── `RN-97` — el saldo de apertura de control interno ───────────────────────
// La regla que impide el abandono. Sin ella, «llega enero, el sistema arranca con reportes en
// cero, y una misión interrumpida en noviembre... simplemente deja de aparecer en ninguna
// pantalla. **Nadie decidió abandonarlos: se abandonaron solos**».
var saldos = app.MapGroup("/saldo-de-apertura");

/// El inventario de lo que sigue vivo a una fecha, **sin producir el documento**. Es lo que
/// se mira antes de cerrar, para saber qué hay que resolver.
saldos.MapGet("/inventario/{corte}", async (
    string corte, ServicioDeSaldoDeApertura servicio) =>
{
    var (renglones, fuentes) = await servicio.InventarioAsync(DateOnly.Parse(corte));
    var alCorte = DateOnly.Parse(corte);

    return Results.Ok(new
    {
        corte = alCorte,
        renglones = renglones.Select(r => ResumirRenglon(r, alCorte)),

        // **Las fuentes van siempre, consultadas o no.** Un saldo que omite en silencio los
        // préstamos vencidos es el abandono que la regla existe para impedir, con formato de
        // reporte.
        fuentes = fuentes.Select(f => new
        {
            tipo = f.Tipo.ToString(),
            sePudoConsultar = f.SePudoConsultar,
            renglones = f.Renglones,
            porQueNo = f.PorQueNo,
        }),

        completo = fuentes.All(f => f.SePudoConsultar),

        // Lo que impide cerrar el período — `RN-97` punto 4.
        bloqueantes = renglones.Count(r => r.ImpideCerrarElPeriodo),
    });
});

/// Produce el documento con folio — `RN-97` punto 1. Se conserva junto al acta de cierre.
saldos.MapPost("/", async (ProducirSaldo peticion, ServicioDeSaldoDeApertura servicio) =>
{
    var saldo = await servicio.ProducirAsync(
        Ulid.Parse(peticion.Id), peticion.Folio, peticion.Ejercicio, peticion.Corte,
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            peticion.Corte),
        peticion.Momento, peticion.DeclaracionDeBloqueantes);

    return Results.Created($"/saldo-de-apertura/{saldo.Ejercicio}",
        new { id = saldo.Id.ToString(), renglones = saldo.Renglones.Count });
});

/// La serie histórica — `RN-97` punto 5: se reporta a Gerencia Administrativa y a Auditoría
/// Interna al inicio del ejercicio, **con su serie**.
saldos.MapGet("/", async (ServicioDeSaldoDeApertura servicio) =>
    Results.Ok((await servicio.TodosAsync()).Select(ResumirSaldo)));

saldos.MapGet("/{ejercicio}", async (
    string ejercicio, ServicioDeSaldoDeApertura servicio) =>
    await servicio.DelEjercicioAsync(ejercicio) is { } saldo
        ? Results.Ok(new
        {
            resumen = ResumirSaldo(saldo),
            renglones = saldo.Renglones.Select(r => ResumirRenglon(r, saldo.Corte)),
        })
        : Results.NotFound());

/// `RN-97` punto 6 — el renglón resuelto se marca con su fecha. **No se borra**: que estuvo en
/// el saldo es parte de la serie, y el residuo al cierre siguiente es el nuevo saldo.
saldos.MapPost("/renglones/{id}/resolver", async (
    string id, ResolverRenglon peticion, ServicioDeSaldoDeApertura servicio) =>
{
    await servicio.ResolverRenglonAsync(
        Ulid.Parse(id), peticion.ComoSeResolvio, peticion.Fecha);

    return Results.Ok(new { resuelto = true });
});

// ── RN-62 · El titulo de tenencia ───────────────────────────────────────────
//
// **Sin titulo vigente el vehiculo no se habilita**, y ninguna mision se programa si su ventana
// excede la vigencia. Y es lo que decide cual de los dos terminales corresponde (`HB3-17`).
var titulosDeTenencia = app.MapGroup("/titulos");

titulosDeTenencia.MapPost("/", async (
    RegistrarTitulo peticion, ServicioDeTitulos servicio) =>
{
    var id = await servicio.RegistrarAsync(
        Ulid.Parse(peticion.Id),
        Ulid.Parse(peticion.IdVehiculo),
        peticion.Regimen,
        peticion.Titular,
        peticion.Documento,
        peticion.Desde,
        peticion.Hasta,
        new RubrosDelTitulo(
            peticion.Combustible, peticion.Mantenimiento, peticion.Llantas, peticion.Seguro,
            peticion.Peajes, peticion.Multas, peticion.Danios));

    return Results.Created($"/titulos/{id}", new { id = id.ToString() });
});

/// **La cobertura de titulos sobre la flota entera** — lo que contesta «cuantos controles estan
/// apagados». Cada vehiculo sin titulo es RN-62 sin evaluar en ese vehiculo: no se contrasta la
/// ventana de sus misiones contra ninguna vigencia, y el terminal correcto se advierte en vez de
/// juzgarse.
///
/// Va en una sola pregunta y no una por vehiculo: el cliente no puede armar esta lista pidiendo
/// /titulos/{vehiculo} en bucle sin decidir el «vigente a hoy» por su cuenta, y esa decision es
/// del dominio.
titulosDeTenencia.MapGet("/", async (
    ConsultaDeFlota flota, ServicioDeTitulos servicio, EstadoDeLaFlota estados) =>
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
    var salida = new List<object>();

    foreach (var v in await flota.TodosAsync())
    {
        var serie = await servicio.DelVehiculoAsync(v.Id);
        var vigente = serie.FirstOrDefault(t => t.VigenteAl(hoy));
        var estado = await estados.ActualAsync(v.Id);

        salida.Add(new
        {
            vehiculo = v.Id.ToString(),
            siglas = v.Siglas,
            placa = v.Placa,
            tipoDeVehiculo = v.TipoDeVehiculo,

            // **Nulo es «no consta bajo que regimen lo tenemos»**, y eso no es «propio»: la
            // suposicion comoda es justo la que produce el asiento falso que HB3-17 impide.
            titulo = vigente is null ? null : ResumirTitulo(vigente),

            // **«Nunca tuvo titulo» y «se le vencio» son cosas opuestas**, y sin este campo se
            // ven iguales: las dos llegarian con `titulo` nulo. La primera es un dato de alta que
            // nadie lleno; la segunda es un comodato corrido de plazo, con un bien ajeno que ya
            // deberia haberse devuelto. Confundirlas esconde la segunda entre las primeras.
            ultimo = serie.Count == 0 ? null : ResumirTitulo(serie[0]),

            // Cuantos titulos tuvo en total. Mayor que uno significa que el regimen cambio, y
            // que las misiones viejas se juzgan contra el que regia entonces.
            enLaSerie = serie.Count,

            // §10.2. **Un vehiculo en estado terminal ya no tiene ningun control que encender**:
            // no se le va a programar nada y su salida de la flota ya ocurrio. Contarlo entre los
            // que «faltan» inflaria el hueco con unidades que no lo son.
            estado = estado?.ToString(),
            fueraDeLaFlota = estado is EstadoOperativo.DadoDeBaja or EstadoOperativo.RetiradoDeFlota,
        });
    }

    return Results.Ok(salida);
});

/// La serie de titulos del vehiculo. **Es una serie y no un campo**: un vehiculo que pasa de
/// comodato a propiedad conserva el anterior, porque las misiones de ese periodo se hicieron bajo
/// comodato y sus rubros los cubria el cedente.
titulosDeTenencia.MapGet("/{vehiculo}", async (
    string vehiculo, ServicioDeTitulos servicio) =>
    Results.Ok((await servicio.DelVehiculoAsync(Ulid.Parse(vehiculo))).Select(ResumirTitulo)));

// ── RN-60 · Indisponibilidad sobrevenida del vehiculo ───────────────────────
//
// **El acuse es lo que convierte el hecho en una decision.** Sin el, el conflicto sobre las
// reservas aparece despues y nadie lo decidio.
var indisponibilidades = app.MapGroup("/indisponibilidades");

/// Lo que se le muestra a quien va a acusar — `RN-60` punto 1: folio, dependencia, ventana,
/// motorista y objeto de cada mision afectada.
indisponibilidades.MapGet("/reservas-afectadas/{vehiculo}/{desde}/{hasta}", async (
    string vehiculo, string desde, string hasta, ServicioDeIndisponibilidad servicio) =>
    Results.Ok((await servicio.ReservasAfectadasAsync(
            Ulid.Parse(vehiculo), DateOnly.Parse(desde), DateOnly.Parse(hasta)))
        .Select(ResumirReserva)));

/// Declara la indisponibilidad con su acuse. **La lista se congela aca** y no se reconstruye.
indisponibilidades.MapPost("/", async (
    DeclararIndisponibilidad peticion, ServicioDeIndisponibilidad servicio) =>
{
    var id = await servicio.DeclararAsync(
        Ulid.Parse(peticion.Id),
        Ulid.Parse(peticion.IdVehiculo),
        peticion.Estado,
        peticion.Causa,
        peticion.Desde,
        peticion.FinEstimado,
        peticion.Ejecuta,
        peticion.MomentoDelAcuse);

    return Results.Created($"/indisponibilidades/{id}", new { id = id.ToString() });
});

indisponibilidades.MapGet("/", async (ServicioDeIndisponibilidad servicio) =>
    Results.Ok((await servicio.TodasAsync()).Select(ResumirIndisponibilidad)));

/// El desenlace de una reserva en conflicto — `RN-60` punto 4. **No expira en silencio.**
indisponibilidades.MapPost("/{id}/reservas/{mision}/resolver", async (
    string id, string mision, ResolverReserva peticion,
    ServicioDeIndisponibilidad servicio) =>
{
    await servicio.ResolverReservaAsync(
        Ulid.Parse(id), Ulid.Parse(mision), peticion.Desenlace, peticion.Ejecuta,
        peticion.Motivo, peticion.Momento);

    return Results.Ok(new { resuelta = true });
});

/// El alta — `RN-60` punto 6: fecha real, orden de trabajo cerrada y odometro de salida.
indisponibilidades.MapPost("/{id}/alta", async (
    string id, DarDeAlta peticion, ServicioDeIndisponibilidad servicio) =>
{
    await servicio.DarDeAltaAsync(
        Ulid.Parse(id), peticion.FinReal, peticion.OrdenDeTrabajo, peticion.OdometroDeSalida);

    return Results.Ok(new { dadoDeAlta = true });
});

// ── RN-63 · El préstamo como expediente del bien ────────────────────────────
//
// **Nunca una Orden de Misión.** Cedido con motorista propio, la tenencia no se cedió: eso es
// una misión con motivo «apoyo institucional», y el endpoint lo bloquea.
var prestamos = app.MapGroup("/prestamos");

prestamos.MapPost("/", async (PrestarVehiculo peticion, ServicioDePrestamos servicio) =>
{
    var id = await servicio.PrestarAsync(
        Ulid.Parse(peticion.Id),
        Ulid.Parse(peticion.IdVehiculo),
        new ActoAutorizante(
            peticion.ActoFolio, peticion.ActoFirmante, peticion.ActoFecha, peticion.ActoAdjunto),
        peticion.Autoriza,
        new ResponsableReceptor(
            peticion.ReceptorPersona, peticion.ReceptorCargo, peticion.ReceptorInstitucion,
            peticion.ReceptorConstancia),
        peticion.Motivo,
        peticion.Desde,
        peticion.DevolucionComprometida,
        new ActaDeTenencia(
            peticion.Desde, peticion.EntregaOdometro, peticion.EntregaFirma,
            peticion.EntregaCombustible, peticion.EntregaAccesorios, peticion.EntregaDocumentos,
            peticion.EntregaRotulacion, peticion.EntregaNovedades),
        new RubrosPactados(
            peticion.RubroCombustible, peticion.RubroPeajes, peticion.RubroMantenimiento,
            peticion.RubroMultas, peticion.RubroDanios),
        peticion.ConMotoristaPropio);

    return Results.Created($"/prestamos/{id}", new { id = id.ToString() });
});

/// El acta de devolución. **El vehículo no vuelve a `DISPONIBLE` sin ella** (`RN-63`).
prestamos.MapPost("/{id}/devolver", async (
    string id, DevolverVehiculo peticion, ServicioDePrestamos servicio) =>
{
    await servicio.DevolverAsync(
        Ulid.Parse(id),
        new ActaDeTenencia(
            peticion.Fecha, peticion.Odometro, peticion.Firma, peticion.NivelDeCombustible,
            null, null, peticion.RotulacionConstatada, peticion.Novedades),
        peticion.QuienFirmaLaDevolucion);

    return Results.Ok(new { devuelto = true });
});

prestamos.MapGet("/", async (ServicioDePrestamos servicio) =>
    Results.Ok((await servicio.TodosAsync()).Select(ResumirPrestamo)));

/// Los vencidos al corte — `RN-97` punto 4, la fuente que faltaba para que el bloqueo del cierre
/// quedara completo.
prestamos.MapGet("/vencidos/{corte}", async (string corte, ServicioDePrestamos servicio) =>
    Results.Ok((await servicio.VencidosAsync(DateOnly.Parse(corte))).Select(ResumirPrestamo)));

/// **El entregable de `RN-63`** punto 7: quién respondía por la unidad en una fecha.
///
/// Se resuelve por la fecha y no por el estado de hoy: un vehículo que hoy está disponible pudo
/// estar prestado el día que se cometió la infracción.
prestamos.MapGet("/quien-respondia/{vehiculo}/{fecha}", async (
    string vehiculo, string fecha, ServicioDePrestamos servicio) =>
{
    var quien = await servicio.QuienRespondiaPorAsync(
        Ulid.Parse(vehiculo), DateOnly.Parse(fecha));

    return Results.Ok(new
    {
        fecha = quien.Fecha,

        // Falso significa que respondía la institución propietaria por su custodio ordinario.
        esTenenciaAjena = quien.EsTenenciaAjena,

        persona = quien.Persona,
        cargo = quien.Cargo,
        institucion = quien.Institucion,
        prestamo = quien.Prestamo?.ToString(),
    });
});

// ── M-12 · Incidentes, siniestros y sanciones ───────────────────────────────
//
// **Ninguna ruta de acá captura responsabilidad.** `RN-74`: la responsabilidad se determina en
// el expediente, por la instancia que corresponde. Lo más cerca es
// `POST /incidentes/{id}/determinacion`, que adjunta el acto de OTRA instancia con su número y
// su emisor — SIGTI lo registra, no lo produce.
var incidentes = app.MapGroup("/incidentes");

/// `I-01` — registrar el hecho. Abre expediente con responsable de seguimiento y plazo.
incidentes.MapPost("/", async (RegistrarIncidente peticion, ServicioDeIncidentes servicio) =>
{
    var id = await servicio.RegistrarAsync(
        Ulid.Parse(peticion.Id),
        peticion.Tipo,
        peticion.Causa,
        peticion.MomentoDelHecho,
        peticion.MomentoDeCaptura,
        peticion.Descripcion,
        peticion.Registra,
        peticion.ResponsableDeSeguimiento,
        peticion.Plazo,
        peticion.Interrumpe,
        peticion.IdMision is null ? null : Ulid.Parse(peticion.IdMision),
        peticion.IdVehiculo is null ? null : Ulid.Parse(peticion.IdVehiculo),
        peticion.Ubicacion,
        peticion.Odometro,
        [.. (peticion.Bienes ?? []).Select(b => (b.Descripcion, b.EsElVehiculo))]);

    return Results.Created($"/incidentes/{id}", new { id = id.ToString() });
});

incidentes.MapGet("/", async (ServicioDeIncidentes servicio) =>
    Results.Ok((await servicio.TodosAsync()).Select(ResumirIncidente)));

incidentes.MapGet("/{id}", async (string id, ServicioDeIncidentes servicio) =>
    await servicio.BuscarAsync(Ulid.Parse(id)) is { } expediente
        ? Results.Ok(ResumirIncidente(expediente))
        : Results.NotFound());

/// Las interrupciones sin desenlace al corte — `RN-70`, la fuente que le da poder de bloqueo al
/// cierre del período (`RN-97` punto 4) y que hasta M-12 no podía disparar.
incidentes.MapGet("/interrupciones-sin-desenlace/{corte}", async (
    string corte, ServicioDeIncidentes servicio) =>
    Results.Ok((await servicio.InterrupcionesSinDesenlaceAsync(DateOnly.Parse(corte)))
        .Select(ResumirIncidente)));

/// Los bienes que siguen fuera del alcance de la institución — `RN-75`.
incidentes.MapGet("/bienes-no-recuperados", async (ServicioDeIncidentes servicio) =>
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    return Results.Ok((await servicio.BienesNoRecuperadosAsync()).Select(x => new
    {
        incidente = x.Expediente.Id.ToString(),
        tipo = x.Expediente.Tipo.ToString(),
        responsable = x.Expediente.ResponsableDeSeguimiento,
        bien = x.Bien.Id.ToString(),
        descripcion = x.Bien.Descripcion,
        esElVehiculo = x.Bien.EsElVehiculo,
        fechaDelHecho = x.Bien.FechaDelHecho,

        // Desde el hecho, como toda antigüedad de este sistema: un bien que lleva tres años
        // sustraído no se presenta como reciente.
        diasFuera = x.Bien.DiasFuera(hoy),

        ubicacionConocida = x.Bien.UbicacionConocida,
        autoridadCustodia = x.Bien.AutoridadCustodia,
        numeroDeExpedienteExterno = x.Bien.NumeroDeExpedienteExterno,
    }));
});

incidentes.MapPost("/{id}/constancia", async (
    string id, AdjuntarConstancia peticion, ServicioDeIncidentes servicio) =>
{
    await servicio.AdjuntarConstanciaAsync(
        Ulid.Parse(id),
        new ConstanciaAnteAutoridad(peticion.Numero, peticion.Autoridad, peticion.Fecha),
        peticion.Ejecuta, peticion.Momento);

    return Results.Ok(new { adjuntada = true });
});

/// `I-03` — el desenlace de la interrupción. **No le cambia el estado a la misión** (`RN-70`).
incidentes.MapPost("/{id}/desenlace", async (
    string id, RegistrarDesenlace peticion, ServicioDeIncidentes servicio) =>
{
    await servicio.RegistrarDesenlaceAsync(
        Ulid.Parse(id), peticion.Desenlace, peticion.Detalle, peticion.Ejecuta, peticion.Momento);

    return Results.Ok(new { resuelta = true });
});

incidentes.MapPost("/{id}/gestiones", async (
    string id, RegistrarGestion peticion, ServicioDeIncidentes servicio) =>
{
    await servicio.RegistrarGestionAsync(
        Ulid.Parse(id),
        new GestionDeRecuperacion(
            peticion.Fecha, peticion.Descripcion, peticion.Responsable, peticion.Plazo),
        peticion.Ejecuta, peticion.Momento);

    return Results.Ok(new { registrada = true });
});

incidentes.MapPost("/{id}/bienes/{bien}/recuperar", async (
    string id, string bien, RecuperarBien peticion, ServicioDeIncidentes servicio) =>
{
    await servicio.RecuperarBienAsync(
        Ulid.Parse(id), Ulid.Parse(bien), peticion.Ejecuta, peticion.Momento, peticion.Donde);

    return Results.Ok(new { recuperado = true });
});

/// `I-06` — el descargo formal, la única salida del registro que no es la recuperación.
incidentes.MapPost("/{id}/bienes/{bien}/descargar", async (
    string id, string bien, DescargarBien peticion, ServicioDeIncidentes servicio) =>
{
    await servicio.DescargarBienAsync(
        Ulid.Parse(id), Ulid.Parse(bien),
        new ConstanciaDeDescargo(peticion.Numero, peticion.Autoridad, peticion.Fecha),
        peticion.Ejecuta, peticion.Momento);

    return Results.Ok(new { descargado = true });
});

/// `I-07` — adjuntar el acto de determinación de responsabilidad de la instancia competente.
/// **SIGTI lo registra; no lo produce** (`RN-74`).
incidentes.MapPost("/{id}/determinacion", async (
    string id, AdjuntarDeterminacion peticion, ServicioDeIncidentes servicio) =>
{
    await servicio.AdjuntarDeterminacionAsync(
        Ulid.Parse(id),
        new DeterminacionDeResponsabilidad(
            peticion.Numero, peticion.Instancia, peticion.Fecha, peticion.Resolucion),
        peticion.Ejecuta, peticion.Momento);

    return Results.Ok(new { adjuntada = true });
});

incidentes.MapPost("/{id}/resolver", async (
    string id, ResolverIncidente peticion, ServicioDeIncidentes servicio) =>
{
    await servicio.ResolverAsync(
        Ulid.Parse(id), peticion.ComoSeResolvio, peticion.Fecha, peticion.Ejecuta,
        peticion.Momento, peticion.DeclaracionDeBienes);

    return Results.Ok(new { resuelto = true });
});

// ── RN-96 · El cierre de ejercicio ──────────────────────────────────────────
//
// **Ninguna de estas rutas mueve un expediente.** `RN-96`: «no ejecuta ni habilita ninguna
// transición de la Orden de Misión. Ningún expediente cambia de estado por efecto de una
// fecha». La única que escribe sobre otro agregado es la anulación de folios, que es un acto
// aparte con autor y motivo.
var cierreDeEjercicio = app.MapGroup("/cierre-de-ejercicio");

/// El acta armada y **sin congelar**. Es lo que se mira antes de producir.
///
/// Sin `corteLegal` ni `corteOperativo` usa **los parámetros de la institución** (`RN-96`).
/// Pasarlos es explorar «qué pasaría si», y el acta lo declara en su origen.
cierreDeEjercicio.MapGet("/{ejercicio}/vista-previa", async (
    string ejercicio, DateOnly? corteLegal, DateOnly? corteOperativo,
    ServicioDeCierreDeEjercicio servicio) =>
    Results.Ok(ResumirActa(await servicio.ArmarAsync(
        ejercicio, corteLegal, corteOperativo,
        Autoria.De(new IdPersona("P-ADMIN"), new IdPuesto("PU-GERENCIA"),
            corteLegal ?? DateOnly.FromDateTime(DateTime.UtcNow)),
        DateTimeOffset.UtcNow))));

/// Las dos fechas de corte que rigen para un ejercicio, o por qué no se pudieron resolver.
cierreDeEjercicio.MapGet("/{ejercicio}/cortes", async (
    string ejercicio, ServicioDeCierreDeEjercicio servicio) =>
{
    var (cortes, sin) = await servicio.CortesAsync(ejercicio, DateTimeOffset.UtcNow);

    return Results.Ok(new
    {
        cortes = cortes is null ? null : new
        {
            legal = cortes.Legal,
            operativo = cortes.Operativo,
            origen = cortes.Origen,
        },
        sinCortes = sin is null ? null : new { clave = sin.Clave, porQueNo = sin.PorQueNo },
    });
});

/// Produce el acta con folio — `RN-96` punto 1. **No anula nada**: eso es un acto aparte.
///
/// **No recibe las fechas de corte.** Salen de los parámetros de la institución: recibirlas
/// dejaría producir el documento del cierre contra un criterio que nadie autorizó.
cierreDeEjercicio.MapPost("/", async (
    ProducirActaDeCierre peticion, ServicioDeCierreDeEjercicio servicio) =>
{
    var acta = await servicio.ProducirAsync(
        peticion.Folio, peticion.Ejercicio,
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            DateOnly.FromDateTime(peticion.Momento.UtcDateTime)),
        peticion.Momento);

    return Results.Created($"/cierre-de-ejercicio/{acta.Ejercicio}", ResumirActa(acta));
});

/// `RN-96` punto 5 — el acta de anulación de folios no consumidos, **por rango y delegación**.
///
/// Va aparte de producir el acta a propósito: anular decenas de folios al producirse un
/// documento sería un cierre masivo por fecha con otro nombre.
cierreDeEjercicio.MapPost("/{ejercicio}/anular-folios", async (
    string ejercicio, AnularFolios peticion, ServicioDeCierreDeEjercicio servicio) =>
{
    var anulados = await servicio.AnularFoliosAsync(
        ejercicio, new IdPersona(peticion.Persona), peticion.Motivo, peticion.Momento);

    return Results.Ok(new { anulados });
});

/// `RN-96` punto 5 — el reporte de reversión de compromisos para ARGOS y SIAFI (`RN-81`).
///
/// `RN-94`: el corte de conocimiento entra como parámetro para que el reporte sea reproducible.
/// Sin él se toma «ahora», que es la pregunta de hoy; con él se reproduce la de un día pasado.
cierreDeEjercicio.MapGet("/{ejercicio}/reversion", async (
    string ejercicio, DateTimeOffset? corteDeConocimiento,
    ServicioDeCierreDeEjercicio servicio) =>
{
    var reporte = await servicio.ReversionAsync(
        ejercicio, corteDeConocimiento ?? DateTimeOffset.UtcNow);

    return reporte is null
        ? Results.NotFound(new
        {
            mensaje = $"No hay acta de cierre del ejercicio {ejercicio}. La reversión de " +
                "compromisos reporta lo que un acta listó y se anuló: sin acta no hay nada " +
                "que conciliar contra SIAFI.",
        })
        : Results.Ok(new
        {
            ejercicio = reporte.Ejercicio,

            // `RN-94` — las dos fechas, en el encabezado del reporte.
            periodoDesde = reporte.PeriodoDesde,
            periodoHasta = reporte.PeriodoHasta,
            corteDeConocimiento = reporte.CorteDeConocimiento,

            actaQueLoRespalda = reporte.ActaQueLoRespalda,

            renglones = reporte.Renglones.Select(r => new
            {
                // ⚠️ Hoy es el ULID de la misión: la clave de vinculación con ARGOS no existe
                // como campo, y ARGOS no va a reconocer este valor.
                claveDeVinculacion = r.ClaveDeVinculacion,
                mision = r.Mision.ToString(),
                folio = r.Folio,
                delegacion = r.Delegacion,

                // Nulo es **sin partida**, no cero: ese renglón no se puede imputar en SIAFI.
                objetoDelGasto = r.ObjetoDelGasto,

                fechaDelHecho = r.FechaDelHecho,
                fechaDeCaptura = r.FechaDeCaptura,
                comprometido = r.Comprometido,
                ejecutado = r.Ejecutado,

                // **Neto.** `RN-81`: el bruto haría que SIAFI revirtiera dinero ya gastado.
                liberado = r.Liberado,
                tuvoEjecucionParcial = r.TuvoEjecucionParcial,
                seConcilia = r.SeConcilia,
            }),

            totalComprometido = reporte.TotalComprometido,
            totalEjecutado = reporte.TotalEjecutado,
            totalLiberado = reporte.TotalLiberado,

            // El detalle por objeto del gasto que `RN-81` punto 4 pide para conciliar.
            porObjetoDelGasto = reporte.PorObjetoDelGasto,

            sinObjetoDelGasto = reporte.SinObjetoDelGasto.Count,
            conEjecucionParcial = reporte.ConEjecucionParcial.Count,
            advertencias = reporte.Advertencias,
        });
});

/// El archivo de conciliación — `RN-96` punto 5.
///
/// ⚠️ **No es el formato de SIAFI.** `RN-81` punto 3: sin contrato de API conocido —insumos #16
/// y #17— el mecanismo inicial es el reporte con formato acordado, y este CSV es el mínimo que
/// se puede conciliar a mano.
cierreDeEjercicio.MapGet("/{ejercicio}/reversion.csv", async (
    string ejercicio, DateTimeOffset? corteDeConocimiento,
    ServicioDeCierreDeEjercicio servicio) =>
{
    var reporte = await servicio.ReversionAsync(
        ejercicio, corteDeConocimiento ?? DateTimeOffset.UtcNow);

    if (reporte is null) return Results.NotFound();

    return Results.File(
        System.Text.Encoding.UTF8.GetBytes(
            ReglasDeLaReversion.ArchivoDeConciliacion(reporte)),

        // Con codificación declarada: el archivo lleva nombres de delegación con tildes y ñ, y
        // abierto como ANSI se ven partidos.
        "text/csv; charset=utf-8",
        $"reversion-de-compromisos-{reporte.Ejercicio}.csv");
});

cierreDeEjercicio.MapGet("/", async (ServicioDeCierreDeEjercicio servicio) =>
    Results.Ok((await servicio.ProducidasAsync()).Select(a => new
    {
        ejercicio = a.Ejercicio,
        folio = a.Folio,
        corteLegal = a.CorteLegal,
        corteOperativo = a.CorteOperativo,
        folios = a.Folios,
        anulados = a.Anulados,
        monto = a.Monto,
        saldoDeAperturaFolio = a.SaldoDeAperturaFolio,
    })));

// ── M-18 Peajes ─────────────────────────────────────────────────────────────
var peajes = app.MapGroup("/peajes");

/// El catálogo, con el estado y la tarifa vigentes **hoy**. Para valorar un paso pasado se
/// usa la fecha del hecho, que es otra pregunta y otra ruta.
peajes.MapGet("/puntos", async (ServicioDePeajes servicio) =>
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
    var ahora = DateTimeOffset.UtcNow;

    var puntos = await servicio.PuntosAsync();
    var vigencias = await servicio.VigenciasAsync();
    var tarifas = await servicio.TarifasAsync();

    return Results.Ok(puntos.Select(p =>
    {
        var estado = ReglasDeTarifaDePeaje.EstadoA(vigencias, p.Id, hoy, ahora);

        return new
        {
            id = p.Id.ToString(),
            nombre = p.Nombre,
            operador = p.Operador,
            carretera = p.Carretera,
            sentidoDeCobro = p.SentidoDeCobro,

            // Nulo es **sin estado declarado**, y no «activo». Suponerlo activo estimaría de
            // más sobre una caseta que quizá cerró.
            estado = estado?.Estado.ToString(),
            fundamentoDelEstado = estado?.Fundamento,

            tarifas = tarifas
                .Where(t => t.Punto == p.Id &&
                            t.VigenteDesde <= hoy &&
                            (t.VigenteHasta == null || hoy <= t.VigenteHasta))
                .Select(t => new
                {
                    categoria = t.Categoria,
                    monto = t.Monto,
                    fuente = t.Fuente,
                    verificada = t.FechaDeVerificacion,
                    desde = t.VigenteDesde,

                    // La tarifa cambia al menos una vez al año, en enero. Se advierte y no se
                    // invalida: una tarifa vieja sigue siendo la mejor información que hay.
                    sinRevisar = t.SinRevisarHaceMasDeUnAnio(hoy),
                }),
        };
    }));
});

/// La carga del catálogo. `RN-34` punto 5: los puntos son catálogo ampliable **en producción,
/// sin cambio de código** — `NRM-10` advierte que hay proyectos en cartera.
peajes.MapPost("/puntos", async (AbrirPunto peticion, ServicioDePeajes servicio) =>
{
    var id = await servicio.AbrirPuntoAsync(
        Ulid.Parse(peticion.Id), peticion.Nombre, peticion.Operador, peticion.Carretera,
        peticion.SentidoDeCobro, peticion.Estado, peticion.Fundamento, peticion.VigenteDesde,

        // El corredor y el kilometro son lo que permite ordenar geograficamente (`RN-37`).
        // Nulos dejan esa dimension sin evaluar en vez de deducir el orden del de captura.
        peticion.Corredor, peticion.Kilometro);

    return Results.Created($"/peajes/puntos/{id}", new { id = id.ToString() });
});

/// El cambio de estado **abre una vigencia nueva**, no edita la que hay: un viaje pasado por
/// una caseta que ya cerró tiene que seguir valorándose con el estado que regía entonces.
peajes.MapPost("/puntos/{id}/estado", async (
    string id, CambiarEstadoDelPunto peticion, ServicioDePeajes servicio) =>
{
    await servicio.CambiarEstadoAsync(
        Ulid.Parse(id), peticion.Estado, peticion.Fundamento, peticion.VigenteDesde);

    return Results.Ok(new { estado = peticion.Estado.ToString() });
});

peajes.MapPost("/categorias", async (CargarCategoria peticion, ServicioDePeajes servicio) =>
{
    await servicio.CargarCategoriaAsync(peticion.Codigo, peticion.Nombre);
    return Results.Created($"/peajes/categorias/{peticion.Codigo}", new { peticion.Codigo });
});

/// `RN-34` — exige fuente y fecha de verificación. **Una tarifa sin fuente no se guarda.**
peajes.MapPost("/tarifas", async (CargarTarifa peticion, ServicioDePeajes servicio) =>
{
    var id = await servicio.CargarTarifaAsync(
        Ulid.Parse(peticion.IdPunto), peticion.Categoria, peticion.Monto, peticion.Fuente,
        peticion.FechaDeVerificacion, peticion.VigenteDesde);

    return Results.Created($"/peajes/tarifas/{id}", new { id = id.ToString() });
});

/// `RN-33` — una fila de la matriz de derivación. **No es una fórmula**: es una tabla, y el
/// criterio legal —el Artículo 51— sigue sin transcribirse (`[C]`, insumo #23).
peajes.MapPost("/matriz", async (CargarRegla peticion, ServicioDePeajes servicio) =>
{
    var id = await servicio.CargarReglaAsync(new FilaDeReglaDeCategoria
    {
        Id = Ulid.NewUlid(),
        Categoria = peticion.Categoria,
        Prioridad = peticion.Prioridad,
        Fundamento = peticion.Fundamento,
        Clase = peticion.Clase,
        TipoDeVehiculo = peticion.TipoDeVehiculo,
        PesoBrutoDesdeKg = peticion.PesoBrutoDesdeKg,
        PesoBrutoHastaKg = peticion.PesoBrutoHastaKg,
        EjesDesde = peticion.EjesDesde,
        EjesHasta = peticion.EjesHasta,
        PasajerosDesde = peticion.PasajerosDesde,
        PasajerosHasta = peticion.PasajerosHasta,
        LlevaRemolque = peticion.LlevaRemolque,
        VigenteDesde = peticion.VigenteDesde,
        RegistradoDesdeUtc = DateTime.UtcNow,
    });

    return Results.Created($"/peajes/matriz/{id}", new { id = id.ToString() });
});

/// `RN-38` — el valor por defecto es **paga**. Ninguna exoneración se carga sola.
peajes.MapPost("/exoneraciones", async (
    CargarExoneracion peticion, ServicioDePeajes servicio) =>
{
    var id = await servicio.CargarExoneracionAsync(
        Ulid.Parse(peticion.IdVehiculo),
        peticion.IdPunto is null ? null : Ulid.Parse(peticion.IdPunto),
        peticion.Operador, peticion.Fundamento,
        peticion.VigenteDesde, peticion.VigenteHasta);

    return Results.Created($"/peajes/exoneraciones/{id}", new { id = id.ToString() });
});

/// `RN-33` — la categoría de un vehículo, **con la explicación de qué la determinó**. Una
/// categoría sin explicación no se puede defender ante la SAPP ni ante un auditor.
peajes.MapGet("/categoria/{idVehiculo}", async (
    string idVehiculo, ServicioDePeajes servicio) =>
{
    var r = await servicio.CategoriaDelVehiculoAsync(
        Ulid.Parse(idVehiculo), DateOnly.FromDateTime(DateTime.UtcNow));

    return Results.Ok(new
    {
        resuelta = r.EstaResuelta,
        codigo = r.Categoria?.Codigo,
        nombre = r.Categoria?.Nombre,
        baseDeLaCategoria = r.Base.ToString(),
        explicacion = r.Explicacion,
        provisional = r.Provisional,
        atributoQueFalta = r.AtributoQueFalta,
    });
});

/// `RN-35` — el estimado **desglosado por punto**, nunca un total opaco. Sin desglose, quien
/// autoriza no puede distinguir un estimado correcto de uno que duplicó un cruce.
peajes.MapPost("/estimacion", async (EstimarPeajes peticion, ServicioDePeajes servicio) =>
{
    var e = await servicio.EstimarAsync(
        [.. peticion.Cruces.Select(c => (Ulid.Parse(c.IdPunto), c.Cruces))],
        peticion.IdVehiculo is null ? null : Ulid.Parse(peticion.IdVehiculo),
        peticion.CategoriaDelTipo is null
            ? null
            : new CategoriaDePeaje(peticion.CategoriaDelTipo, peticion.CategoriaDelTipo),
        peticion.TipoDeVehiculo ?? "",
        peticion.FechaPrevista);

    return Results.Ok(new
    {
        // Nulo cuando ninguna línea se pudo valorar. Un total de cero sobre líneas no
        // valoradas diría que la misión no cuesta peaje.
        total = e.Total,
        disponible = e.Disponible,

        // Se dice aunque el total exista: un total parcial presentado como completo
        // subestima el costo y produce faltante de efectivo en ruta.
        parcial = e.Parcial,

        baseDeLaCategoria = e.Base.ToString(),
        provisional = e.Provisional,
        faltantes = e.Faltantes,

        lineas = e.Lineas.Select(l => new
        {
            punto = l.Punto.ToString(),
            nombre = l.NombreDelPunto,
            cruces = l.Cruces,
            categoria = l.Categoria?.Nombre,
            tarifaUnitaria = l.TarifaUnitaria,
            subtotal = l.Subtotal,
            valorada = l.SeValoro,
            fundamento = l.Fundamento,
        }),
    });
});

/// `RN-36` — registra un paso tal como ocurrió. La categoría esperada la resuelve el
/// servidor: si el cliente pudiera declararla, el error de la caseta entraría por la puerta
/// de atrás como «esperada».
peajes.MapPost("/pasos", async (RegistrarPaso peticion, ServicioDePeajes servicio) =>
{
    var id = await servicio.RegistrarPasoAsync(
        Ulid.Parse(peticion.Id),
        peticion.IdPunto is null ? default : Ulid.Parse(peticion.IdPunto),
        Ulid.Parse(peticion.IdVehiculo),
        peticion.IdMision is null ? null : Ulid.Parse(peticion.IdMision),
        peticion.OcurridoEn,
        peticion.Odometro,
        peticion.MontoPagado,
        peticion.Medio,
        new IdPersona(peticion.Registra),
        peticion.CategoriaCobrada,
        peticion.Ticket,
        peticion.CausaSinTicket,
        peticion.PuntoNoCatalogado,
        peticion.UbicacionDeclarada,
        peticion.IdDeCaptura is null ? null : Ulid.Parse(peticion.IdDeCaptura));

    return Results.Created($"/peajes/pasos/{id}", new { id = id.ToString() });
});

peajes.MapGet("/pasos/mision/{id}", async (string id, ServicioDePeajes servicio) =>
    Results.Ok((await servicio.PasosDeLaMisionAsync(Ulid.Parse(id))).Select(ResumirPaso)));

/// **Dónde nos están cobrando mal** — el insumo del expediente de reclamo ante la SAPP.
peajes.MapGet("/discrepancias", async (ServicioDePeajes servicio) =>
    Results.Ok((await servicio.DiscrepanciasAsync()).Select(ResumirPaso)));

/// `RN-35` punto 4 y `RN-41` — congela el estimado al aprobar. Es lo que el autorizador
/// autorizó, y lo único contra lo que `RN-37` puede juzgar si una caseta estaba en la ruta.
peajes.MapPost("/estimacion/congelar", async (
    CongelarEstimado peticion, ServicioDePeajes servicio) =>
{
    var estimacion = await servicio.EstimarAsync(
        [.. peticion.Cruces.Select(c => (Ulid.Parse(c.IdPunto), c.Cruces))],
        peticion.IdVehiculo is null ? null : Ulid.Parse(peticion.IdVehiculo),
        peticion.CategoriaDelTipo is null
            ? null
            : new CategoriaDePeaje(peticion.CategoriaDelTipo, peticion.CategoriaDelTipo),
        peticion.TipoDeVehiculo ?? "",
        peticion.FechaPrevista);

    await servicio.CongelarEstimadoAsync(
        Ulid.Parse(peticion.IdMision), estimacion,
        new IdPersona(peticion.Congela), peticion.Momento);

    return Results.Ok(new { total = estimacion.Total, lineas = estimacion.Lineas.Count });
});

/// El desvío declarado desde el campo — el mínimo que `RN-37` necesita de `RN-76`. Sin él la
/// regla produciría hallazgos falsos en masa: Honduras tiene derrumbes con regularidad.
peajes.MapPost("/desvios", async (DeclararDesvio peticion, ServicioDePeajes servicio) =>
{
    var id = await servicio.DeclararDesvioAsync(
        Ulid.Parse(peticion.IdMision), Ulid.Parse(peticion.IdVehiculo),
        peticion.Desde, peticion.Hasta, peticion.Motivo,
        new IdPersona(peticion.Declara),
        peticion.IdDeCaptura is null ? null : Ulid.Parse(peticion.IdDeCaptura));

    return Results.Created($"/peajes/desvios/{id}", new { id = id.ToString() });
});

/// **El cruce de `RN-37`**: peaje × kilometraje × ruta autorizada. Un dictamen por vehículo,
/// porque en una sustitución en ruta dos vehículos pasan por la misma caseta legítimamente.
peajes.MapGet("/coherencia/{idMision}", async (
    string idMision, ServicioDePeajes servicio, IParametrosDeLaInstitucion parametros) =>
{
    var dictamenes = await servicio.EvaluarCoherenciaAsync(
        Ulid.Parse(idMision), parametros.VelocidadMediaMaximaKmH);

    return Results.Ok(dictamenes.Select(d => new
    {
        vehiculo = d.Vehiculo.ToString(),
        pasosEvaluados = d.Dictamen.PasosEvaluados,

        // **Sin hallazgos NO es lo mismo que coherente.** Un dictamen que no pudo mirar nada
        // no es conformidad: es silencio, y `RN-37` manda que eso se vea.
        coherente = d.Dictamen.Coherente,

        dimensiones = new
        {
            geografica = d.Dictamen.Dimensiones.Geografica,
            temporal = d.Dictamen.Dimensiones.Temporal,
            contraLaRutaAutorizada = d.Dictamen.Dimensiones.ContraLaRutaAutorizada,
            contraElKilometraje = d.Dictamen.Dimensiones.ContraElKilometraje,
            todas = d.Dictamen.Dimensiones.Todas,
            porQueNo = d.Dictamen.Dimensiones.PorQueNo,
        },

        // Todas, no sólo los hallazgos: una incoherencia justificada o no concluyente sigue
        // siendo parte del expediente, y el auditor pregunta por ella.
        incoherencias = d.Dictamen.Incoherencias.Select(i => new
        {
            tipo = i.Tipo.ToString(),
            explicacion = i.Explicacion,
            pasos = i.Pasos.Select(x => x.ToString()),
            concluyente = i.Concluyente,
            justificada = i.Justificada,
            justificacion = i.Justificacion,
            esHallazgo = i.EsHallazgo,
        }),
    }));
});

// ── `RN-83` punto 5 — el libro de existencias del tanque institucional ──────

// ═══ M-19 Seguimiento en Ruta ═══════════════════════════════════════════════
//
// **Este grupo no infiere nada.** No cierra misiones por inactividad, no marca
// interrupciones, no deduce que un vehiculo se detuvo. `RN-76` lo prohibe, y la razon es
// operativa antes que normativa: el silencio es la condicion ESPERADA en buena parte del
// pais, asi que toda inferencia sobre el seria falsa la mayoria de las veces.
var seguimiento = app.MapGroup("/seguimiento");

/// `PT-058` — el tablero, **con la antiguedad de cada dato a la vista**.
///
/// La antiguedad no es un adorno de la pantalla: viaja con el dato porque «ultima posicion
/// conocida hace 10 h 40 min» y «ultima posicion conocida» llevan a decisiones distintas, y la
/// segunda afirma algo falso sin decir ninguna mentira.
seguimiento.MapGet("/", async (ServicioDeSeguimiento servicio) =>
{
    var tablero = await servicio.TableroAsync(DateTimeOffset.UtcNow);

    return Results.Ok(new
    {
        ahora = tablero.Ahora,

        // **Nulo cuando la institucion no fijo el umbral** — insumo #68. La pantalla lo dice
        // en vez de mostrar un tablero que degrada segun un numero que nadie decidio.
        umbralHoras = tablero.Umbral?.TotalHours,

        misiones = tablero.Misiones.Select(m => new
        {
            mision = m.Mision,
            folio = m.Folio,
            dependencia = m.Dependencia,
            destino = m.Destino,
            objetoDelTraslado = m.ObjetoDelTraslado,
            vehiculo = m.Vehiculo,
            motorista = m.Motorista,
            retorno = m.Retorno,

            // Lo DECLARADO por el motorista, con LA HORA DE ESE REPORTE. Nulo es que no
            // declaro nada, y no se rellena con nada calculado.
            ultimoEstado = m.UltimoEstado,
            declaradoEl = m.DeclaradoEl,

            // La ultima senal de vida, del tipo que sea: sobre esta se mide la antiguedad.
            ultimoHecho = m.UltimoHecho,

            posicion = m.UltimaPosicion is { } pos
                ? new { latitud = pos.Latitud, longitud = pos.Longitud, precisionMetros = pos.PrecisionMetros }
                : null,

            frescura = new
            {
                grado = m.Frescura.Grado.ToString(),
                // Nula cuando nunca hubo dato, y NEGATIVA cuando el reloj del dispositivo va
                // adelantado. Las dos se muestran como son.
                minutos = m.Frescura.Antiguedad?.TotalMinutes,
                porQue = m.Frescura.PorQue,
            },
        }),
    });
});

/// `PT-059` — el detalle de la mision en ruta con sus hitos.
seguimiento.MapGet("/{id}", async (string id, ServicioDeSeguimiento servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    var d = await servicio.DetalleAsync(ulid, DateTimeOffset.UtcNow);

    if (d is null)
        return Results.NotFound(new { mensaje = $"No existe el expediente {id}." });

    return Results.Ok(new
    {
        mision = d.Mision,
        folio = d.Folio,
        estado = d.Estado,
        dependencia = d.Dependencia,
        destino = d.Destino,
        objetoDelTraslado = d.ObjetoDelTraslado,

        ultimoEstadoDeclarado = d.UltimoEstadoDeclarado,
        declaradoEl = d.DeclaradoEl,

        frescura = new
        {
            grado = d.Frescura.Grado.ToString(),
            minutos = d.Frescura.Antiguedad?.TotalMinutes,
            porQue = d.Frescura.PorQue,
        },

        hitos = d.Hitos.Select(h => new
        {
            id = h.Id.ToString(),
            tipo = h.Tipo.ToString(),
            estado = h.Estado,
            destino = h.Destino,
            momentoDelHecho = h.MomentoDelHecho,
            momentoDeCaptura = h.MomentoDeCaptura,

            // El desfase NO es un error: mide cuanto estuvo el dispositivo sin cobertura
            // (`RN-43`). Va como dato para que la pantalla pueda decirlo.
            desfaseMinutos = ReglasDelSeguimiento.DesfaseDeCaptura(h).TotalMinutes,

            posicion = h.Posicion is { } pos
                ? new { latitud = pos.Latitud, longitud = pos.Longitud, precisionMetros = pos.PrecisionMetros }
                : null,

            causaDeEspera = h.CausaDeEspera,
            seAtribuyeA = h.SeAtribuyeA,
            motorEncendido = h.MotorEncendido,
            declara = h.Declara.Valor,
        }),

        estadias = d.Estadias.Estadias.Select(e => new
        {
            destino = e.Destino,
            arribo = e.Arribo,
            // Nula mientras siga en el sitio: el reloj corre, pero no se da por cerrada.
            salida = e.Salida,
            como = e.Como.ToString(),
            minutos = e.Duracion.TotalMinutes,
            causa = e.Causa,
            seAtribuyeA = e.SeAtribuyeA,
            motorEncendido = e.MotorEncendido,
            // **Nulo es «no se pudo clasificar»**, nunca falso.
            esImproductiva = e.EsImproductiva,
        }),

        // Huecos visibles: una salida sin arribo no se rellena con una estadia de cero.
        salidasSinArribo = d.Estadias.SalidasSinArribo.Select(x => new
        {
            destino = x.Destino,
            momento = x.Momento,
        }),

        improductivoMinutos = d.Estadias.Improductivo.TotalMinutes,

        // Van SIEMPRE al lado del total: «4 horas improductivas» y «4 horas improductivas,
        // con 3 estadias sin tipificar» sostienen conclusiones distintas.
        sinTipificar = d.Estadias.SinTipificar,
        sinCatalogoDeCausas = d.Estadias.SinCatalogoDeCausas,
    });
});

/// El ingreso del cliente de campo. **Acepta reportes atrasados**: se juzga contra el diario a
/// la fecha del hecho, no contra el estado de hoy, porque un lote que sube cuatro dias tarde
/// encuentra la mision ya liquidada y rechazarlo perderia justo lo que hay que conservar.
seguimiento.MapPost("/{id}/reportes", async (
    string id, ReporteEntrante cuerpo, ServicioDeSeguimiento servicio) =>
{
    if (!Identificador.Valido(id, out var ulid, out var error)) return error;

    if (!Enum.TryParse<TipoDeReporte>(cuerpo.Tipo, ignoreCase: true, out var tipo))
        return Results.BadRequest(new
        {
            mensaje = $"«{cuerpo.Tipo}» no es un tipo de reporte. Los validos son: " +
                      string.Join(", ", Enum.GetNames<TipoDeReporte>()) + ".",
        });

    var posicion = cuerpo is { Latitud: not null, Longitud: not null }
        ? new Posicion(cuerpo.Latitud.Value, cuerpo.Longitud.Value, cuerpo.PrecisionMetros)
        : (Posicion?)null;

    var nuevo = await servicio.RegistrarAsync(
        ulid, tipo, cuerpo.MomentoDelHecho, new IdPersona(cuerpo.Declara),
        cuerpo.Estado, cuerpo.Destino, posicion,
        cuerpo.CausaDeEspera, cuerpo.SeAtribuyeA, cuerpo.MotorEncendido);

    return Results.Created($"/seguimiento/{id}", new { id = nuevo.ToString() });
});


// ═══ El puesto vigente y su alcance ═════════════════════════════════════════
//
// `R-1`: **no hay un menu unico, hay una raiz por puesto**. Y `actores-y-roles` §3: el alcance
// de datos se otorga en la relacion puesto↔rol, no en el rol.
//
// ⚠️ **No hay autenticacion.** La persona se pasa como parametro y el servidor no verifica que
// quien pide sea quien dice ser. Eso hace que el alcance sea hoy **un filtro de presentacion**,
// no un control de acceso: cualquiera puede pedir el de otro. Se construye igual porque el
// filtro es la parte que falta y la comprobacion de identidad es una capa aparte — pero
// quedaria falso llamarlo control mientras no exista.
var puestos = app.MapGroup("/puesto");

/// Quienes ocupan algun puesto hoy.
///
/// ⚠️ **Esto sustituye a la autenticacion que no existe.** No es una lista de usuarios: es el
/// organigrama, y por eso incluye a cualquiera que ocupe un puesto. Cuando haya autenticacion
/// real, quien entra sale del token y este endpoint deja de tener sentido en la pantalla de
/// ingreso — pero sigue sirviendo para el padron.
puestos.MapGet("/personas", async (SigtiDbContext contexto, DateOnly? fecha) =>
{
    var alDia = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

    var filas = await contexto.AsignacionesDePuesto.AsNoTracking().ToListAsync();

    return Results.Ok(filas
        .Where(a => a.Desde <= alDia && (a.Hasta == null || a.Hasta >= alDia))
        .GroupBy(a => a.Persona)
        .OrderBy(g => g.Key)
        .Select(g => new { persona = g.Key, puestos = g.Select(a => a.Puesto).Distinct() }));
});

/// `PT-001` — que puestos ocupa una persona hoy, y que le da cada uno.
puestos.MapGet("/de/{persona}", async (
    string persona, ServicioDelPuesto servicio, DateOnly? fecha) =>
{
    var alDia = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var suyos = await servicio.DeLaPersonaAsync(new IdPersona(persona), alDia);

    return Results.Ok(new
    {
        persona = suyos.Persona,
        fecha = suyos.Fecha,

        // **Falso no es «no tiene puestos hoy»**: es «nunca tuvo ninguno». Las dos muestran
        // una lista vacia y solo una significa que alguien escribio mal el identificador.
        conocida = suyos.Conocida,

        puestos = suyos.Puestos.Select(x => new
        {
            puesto = x.Puesto,

            // Nulos cuando el puesto no esta en el espejo. No se sustituyen por el
            // identificador: eso lo mostraria como si fuera el nombre que la institucion le da.
            denominacion = x.Denominacion,
            unidad = x.Unidad,
            delegacion = x.Delegacion,
            enElEspejo = x.EnElEspejo,

            competencias = x.Competencias.Select(c => new
            {
                rol = c.Rol.ToString(),
                alcance = c.Alcance.ToString(),
            }),

            // Puede haber mas de una, y **no se elige por el ocupante**: cual manda es
            // politica, no codigo.
            raices = x.Raices.Select(r => new
            {
                pantalla = r.Pantalla,
                nombre = r.Nombre,
                porQue = r.PorQue,
            }),

            // Los roles que el mapa de navegacion no cubre. Se nombran: dejan a su ocupante
            // sin punto de entrada, y eso es una brecha del diseño que conviene ver.
            rolesSinRaiz = x.RolesSinRaiz.Select(r => r.ToString()),
        }),
    });
});

/// `PT-002` — lo que le toca al puesto ahora. `R-2`: bandeja de trabajo, no tablero.
puestos.MapGet("/{puesto}/inicio", async (
    string puesto, string persona, ServicioDelPuesto servicio, DateOnly? fecha) =>
{
    var alDia = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var inicio = await servicio.InicioAsync(new IdPersona(persona), new IdPuesto(puesto), alDia);

    if (inicio is null)
        return Results.NotFound(new
        {
            mensaje = $"«{persona}» no ocupa el puesto {puesto} al {alDia:yyyy-MM-dd}.",
        });

    return Results.Ok(new
    {
        puesto = inicio.Puesto.Puesto,
        denominacion = inicio.Puesto.Denominacion,
        unidad = inicio.Puesto.Unidad,
        delegacion = inicio.Puesto.Delegacion,

        raices = inicio.Puesto.Raices.Select(r => new
        {
            pantalla = r.Pantalla, nombre = r.Nombre, porQue = r.PorQue,
        }),

        // Cada contador atado a la raiz que lo resuelve: un numero sin destino seria el
        // tablero decorativo que `R-2` rechaza.
        pendientes = inicio.Pendientes.Select(x => new
        {
            pantalla = x.Pantalla, que = x.Que, cuantos = x.Cuantos,
        }),

        // **Cero pendientes y alcance sin resolver son cosas distintas.** Las dos se ven como
        // una bandeja vacia y solo una significa que no hay trabajo.
        alcanceResuelto = inicio.AlcanceResuelto,
        porQueNo = inicio.PorQueNoSeResolvio,
    });
});

/// `PT-005` — el buscador **con el alcance de datos aplicado**.
puestos.MapGet("/{puesto}/expedientes", async (
    string puesto, string persona, ServicioDelPuesto servicio, string? q, DateOnly? fecha) =>
{
    var alDia = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
    var r = await servicio.BuscarAsync(new IdPersona(persona), new IdPuesto(puesto), q, alDia);

    if (r is null)
        return Results.NotFound(new
        {
            mensaje = $"«{persona}» no ocupa el puesto {puesto} al {alDia:yyyy-MM-dd}.",
        });

    return Results.Ok(new
    {
        puesto = r.Puesto.Puesto,
        denominacion = r.Puesto.Denominacion,

        // `R-7` aplicado al alcance: la pantalla dice **con que regla** filtro.
        nivel = r.Nivel.ToString(),
        alcanceResuelto = r.AlcanceResuelto,
        porQueNo = r.PorQueNoSeResolvio,

        // El numero si, los datos no. Saber que hay mas es control interno; verlos seria el
        // permiso que no se tiene.
        fueraDelAlcance = r.FueraDelAlcance,

        total = r.Total,
        resultados = r.Resultados.Select(x => new
        {
            mision = x.Mision,
            folio = x.Folio,
            estado = x.Estado,
            dependencia = x.Dependencia,
            destino = x.Destino,
            objetoDelTraslado = x.ObjetoDelTraslado,
            solicitanteDeDerecho = x.SolicitanteDeDerecho,
            salida = x.Salida,
        }),
    });
});


/// `PT-004` — el catalogo del patron de bloqueo duro.
///
/// **Un patron que solo existe repartido no se puede revisar.** Nadie puede contestar «como se
/// ve un bloqueo» sin provocar uno, y las siete historias que citan `PT-004` quedarian sin nada
/// que señalar. Aca se ve cada precondicion con su camino de salida **antes** de que alguien
/// quede detenido frente a ella en el predio a las seis de la mañana.
app.MapGet("/bloqueos", () => Results.Ok(new
{
    precondiciones = ReglasDeLaSalida.Todos.Select(c => new
    {
        precondicion = c.Precondicion,
        titulo = c.Titulo,

        // Contesta «¿por que me aparecio ahora?»: la misma precondicion se revalida en varios
        // momentos, y `BD-02` lo hace a proposito en el despacho aunque ya haya pasado en la
        // programacion — una licencia vence entre una y otra.
        seEvaluaEn = c.SeEvaluaEn,

        salida = new
        {
            quePuedeHacer = c.QuePuedeHacer,
            aQuienAcudir = c.AQuienAcudir,
            ficha = c.Ficha,
        },
    }),
}));


/// El control de folios de `RNF-21`: rangos, saldo, aviso.
app.MapGet("/folios", async (ServicioDeFolios folios) =>
{
    var control = await folios.ControlAsync(DateOnly.FromDateTime(DateTime.UtcNow));

    return Results.Ok(control.Select(c => new
    {
        rango = c.Rango.Id.ToString(),
        delegacion = c.Rango.Delegacion,
        tipoDeDocumento = c.Rango.TipoDeDocumento,

        // **Nulo es «toda la delegacion»**, y solo sirve con un equipo emitiendo.
        dispositivo = c.Rango.Dispositivo,

        desde = c.Rango.Desde,
        hasta = c.Rango.Hasta,
        // Incluye los anulados: el folio de un documento anulado no vuelve al rango.
        emitidos = c.Rango.Emitidos,
        disponibles = c.Rango.Disponibles,

        grado = c.Aviso.Grado.ToString(),
        porQue = c.Aviso.PorQue,
    }));
});

/// Asigna un rango a una delegacion. Rechaza el solape — `RN-44`.
app.MapPost("/folios", async (AsignarRango peticion, ServicioDeFolios folios) =>
{
    var id = await folios.AsignarAsync(
        peticion.Delegacion, peticion.TipoDeDocumento, peticion.Desde, peticion.Hasta,
        new IdPersona(peticion.Asigna), peticion.AsignadoEl, peticion.Dispositivo);

    return Results.Created($"/folios/{id}", new { id = id.ToString() });
});

var tanques = app.MapGroup("/tanques");

tanques.MapGet("/", async (ServicioDeTanques servicio) =>
    Results.Ok((await servicio.TodosAsync()).Select(t => new
    {
        id = t.Id.ToString(),
        nombre = t.Nombre,
        ambito = t.AmbitoDeclarado,
        tipoDeCombustible = t.TipoDeCombustible,
        capacidad = t.CapacidadGalones,

        // La suma del libro. **No hay columna de existencia**: una se desincroniza el primer
        // día en que dos despachos entren a la vez.
        existencia = t.Existencia,

        // Nula significa **nunca se arqueó**, y eso no es cero: de un tanque nunca medido no
        // se deduce que cuadre.
        diferenciaDelUltimoArqueo = t.DiferenciaDelUltimoArqueo,
        ultimoArqueo = t.UltimaConstatacion?.Momento,
        movimientos = t.Libro.Count,
    })));

tanques.MapGet("/{id}", async (string id, ServicioDeTanques servicio) =>
    await servicio.BuscarAsync(Ulid.Parse(id)) is { } t
        ? Results.Ok(new
        {
            id = t.Id.ToString(),
            nombre = t.Nombre,
            ambito = t.AmbitoDeclarado,
            tipoDeCombustible = t.TipoDeCombustible,
            capacidad = t.CapacidadGalones,
            existencia = t.Existencia,
            diferenciaDelUltimoArqueo = t.DiferenciaDelUltimoArqueo,

            libro = t.Libro.Select(m => new
            {
                movimiento = m.Id,
                tipo = m.Tipo.ToString(),
                galones = m.Galones,
                persona = m.Autor.Persona.Valor,
                puesto = m.Autor.Puesto.Valor,
                momento = m.Momento,
                motivo = m.Motivo,
                vehiculo = m.Vehiculo?.ToString(),
                mision = m.Mision?.ToString(),
                abastecimiento = m.Abastecimiento?.ToString(),
                contraparte = m.Contraparte?.ToString(),
                existenciaMedida = m.ExistenciaMedida,
                motivoDelAjuste = m.MotivoDelAjuste?.ToString(),
                comprobante = m.Comprobante,
            }),
        })
        : Results.NotFound());

tanques.MapPost("/", async (AbrirTanque peticion, ServicioDeTanques servicio) =>
{
    var id = await servicio.AbrirAsync(
        Ulid.Parse(peticion.Id), peticion.Nombre, peticion.Ambito,
        peticion.TipoDeCombustible, peticion.Capacidad,
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            DateOnly.FromDateTime(peticion.Momento.Date)),
        peticion.ExistenciaInicial, peticion.Momento);

    return Results.Created($"/tanques/{id}", new { id = id.ToString() });
});

/// `E-01`, `E-05` y `E-06`. El despacho a vehículo (`E-02`) **no entra por acá**: va con su
/// abastecimiento, porque son el mismo hecho visto desde dos lados.
tanques.MapPost("/{id}/movimiento", async (
    string id, MoverExistencias peticion, ServicioDeTanques servicio) =>
{
    var autor = Autoria.De(
        new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
        DateOnly.FromDateTime(peticion.Momento.Date));

    var existencia = await servicio.MoverAsync(Ulid.Parse(id), t =>
    {
        switch (peticion.Movimiento)
        {
            case "E-01": t.Recibir(autor, peticion.Galones ?? 0m,
                peticion.Comprobante ?? "", peticion.Momento); break;

            case "E-05": t.Constatar(autor, peticion.ExistenciaMedida ?? 0m,
                peticion.Texto, peticion.Momento); break;

            case "E-06": t.Ajustar(autor, peticion.Galones ?? 0m,
                Enum.Parse<MotivoDeAjuste>(peticion.MotivoDelAjuste ?? ""),
                peticion.Texto, peticion.Momento); break;

            default: throw new BloqueoDuro("RN-83",
                $"«{peticion.Movimiento}» no entra por acá. Son `E-01` recibir, `E-05` " +
                "constatar y `E-06` ajustar. El despacho a un vehículo va con su " +
                "abastecimiento, y el trasiego mueve dos tanques a la vez.");
        }
    });

    return Results.Ok(new { existencia });
});

/// `E-03` y `E-04` — los dos lados en una sola llamada. Registrar sólo la salida haría que el
/// combustible se evaporara del sistema entero en vez de sólo de un tanque.
tanques.MapPost("/trasiegos", async (Trasiego peticion, ServicioDeTanques servicio) =>
{
    await servicio.TrasegarAsync(
        Ulid.Parse(peticion.IdOrigen), Ulid.Parse(peticion.IdDestino),
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            DateOnly.FromDateTime(peticion.Momento.Date)),
        peticion.Galones, peticion.Momento);

    return Results.Ok(new { trasegados = peticion.Galones });
});

/// **El préstamo invisible, vuelto lista** — galones que alguien declaró sacados del tanque
/// institucional y que ningún tanque registró haber despachado.
tanques.MapGet("/despachos-sin-respaldo", async (ServicioDeTanques servicio) =>
    Results.Ok((await servicio.DespachosSinRespaldoAsync()).Select(d => new
    {
        abastecimiento = d.Abastecimiento.ToString(),
        vehiculo = d.Vehiculo.ToString(),
        mision = d.Mision?.ToString(),
        galones = d.Galones,
        momento = d.OcurridoEn,
        registra = d.Registra,
    })));

// ── `RN-86` — el circuito de reintegro ──────────────────────────────────────
// Vive fuera de `/combustible` a propósito: la obligación **sobrevive al cierre de la
// misión** y al del fondo, y colgarla del recurso que la originó daría a entender que se
// archiva con él — que es exactamente el agujero que `RN-86` existe para tapar.
var reintegros = app.MapGroup("/reintegros");

/// El arqueo por persona: quién tiene cuánto dinero del Estado en la mano, desde cuándo.
/// `RN-86` punto 6 — la primera pregunta de un arqueo, y hoy no la contesta nadie.
reintegros.MapGet("/arqueo", async (ServicioDeReintegro servicio) =>
{
    var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
    var arqueo = await servicio.ArqueoPorPersonaAsync(hoy);

    return Results.Ok(arqueo.Select(p => new
    {
        responsable = p.Responsable.ToString(),
        aCargo = p.ACargo,
        aFavor = p.AFavor,
        sinComprobar = p.SinComprobar,
        vencido = p.Vencido,

        // Cada saldo con su explicación entera. Un monto sin la ventana de tiempo no
        // demuestra si el dinero estuvo afuera dos días o dos meses.
        saldos = p.Saldos.Select(s => new
        {
            vale = s.FolioDelVale,
            mision = s.Mision.ToString(),
            monto = s.Monto,
            desde = s.Desde,
            vence = s.Vence,
            vencido = s.VencidoAl(hoy),
            diasAfuera = s.DiasAfueraAl(hoy),
            explicacion = s.Explicacion,
        }),

        obligaciones = p.Obligaciones.Select(Resumir),
    }));
});

reintegros.MapGet("/", async (ServicioDeReintegro servicio) =>
    Results.Ok((await servicio.TodasAsync()).Select(Resumir)));

reintegros.MapGet("/{id}", async (string id, ServicioDeReintegro servicio) =>
    await servicio.BuscarAsync(Ulid.Parse(id)) is { } o
        ? Results.Ok(new
        {
            resumen = Resumir(o),

            // El diario entero. Un expediente de reintegro que sólo muestra el saldo no
            // sirve para nada: lo que el auditor pide es la notificación, el descargo y la
            // resolución, con quién y con qué competencia.
            diario = o.Diario.Select(m => new
            {
                movimiento = m.Id,
                destino = m.Destino.ToString(),
                persona = m.Autor.Persona.Valor,
                puesto = m.Autor.Puesto.Valor,
                momento = m.Momento,
                motivo = m.Motivo,
                pagado = m.Pagado,
            }),
        })
        : Results.NotFound());

/// `R-01` — nominar. Acto propio: `RN-86` punto 5 es explícito en que la obligación no nace
/// en la liquidación, y `RN-74` reserva la determinación a quien corresponde.
reintegros.MapPost("/", async (NominarReintegro peticion, ServicioDeReintegro servicio) =>
{
    var id = await servicio.NominarAsync(
        Ulid.Parse(peticion.Id),
        Enum.Parse<DireccionDelReintegro>(peticion.Direccion),
        Enum.Parse<CausaDelReintegro>(peticion.Causa),
        Ulid.Parse(peticion.IdResponsable),
        peticion.Monto,
        peticion.IdMision is { } m ? Ulid.Parse(m) : null,
        peticion.IdAsignacion is { } a ? Ulid.Parse(a) : null,
        peticion.FechaDelHecho,
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            peticion.FechaDelHecho),
        peticion.Motivo,
        peticion.Momento);

    return Results.Created($"/reintegros/{id}", new { id = id.ToString() });
});

reintegros.MapPost("/{id}/movimiento", async (
    string id, MoverReintegro peticion, ServicioDeReintegro servicio) =>
{
    var autor = Autoria.De(
        new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
        peticion.FechaDelHecho ?? DateOnly.FromDateTime(peticion.Momento.Date));

    var estado = await servicio.MoverAsync(Ulid.Parse(id), o =>
    {
        switch (peticion.Movimiento)
        {
            case "R-02": o.Notificar(autor, peticion.Texto, peticion.Momento); break;
            case "R-03": o.RegistrarDescargo(autor, peticion.Texto, peticion.Momento); break;
            case "R-04": o.Resolver(autor, peticion.Texto, peticion.Momento); break;
            case "R-05": o.DejarSinEfecto(autor, peticion.Texto, peticion.Momento); break;

            // La fecha del hecho del pago es **cuándo entró el dinero a la caja**, no cuándo
            // se capturó: `RN-86` punto 1 y `CE-26` §5 — capturarla distinta para que el
            // plazo no aparezca vencido es falsificar un dato.
            case "R-06": o.RegistrarPago(
                autor, peticion.Monto ?? 0m,
                peticion.FechaDelHecho ?? DateOnly.FromDateTime(peticion.Momento.Date),
                peticion.Texto, peticion.Momento); break;

            default: throw new BloqueoDuro("RN-86",
                $"«{peticion.Movimiento}» no es un movimiento de la obligación. Son `R-02` a `R-06`.");
        }
    });

    return Results.Ok(new { estado = estado.ToString() });
});

/// El levantamiento del bloqueo — acto de ACT-08, por misión y con motivo escrito.
reintegros.MapPost("/levantamientos", async (
    LevantarBloqueo peticion, ServicioDeReintegro servicio) =>
{
    var id = await servicio.LevantarBloqueoAsync(
        Ulid.Parse(peticion.IdMision),
        Ulid.Parse(peticion.IdResponsable),
        Autoria.De(new IdPersona(peticion.Persona), new IdPuesto(peticion.Puesto),
            DateOnly.FromDateTime(peticion.Momento.Date)),
        peticion.Motivo,
        peticion.Momento);

    return Results.Created($"/reintegros/levantamientos/{id}", new { id = id.ToString() });
});

/// El indicador que `RN-86` pide: levantamientos por persona y por período.
reintegros.MapGet("/levantamientos", async (ServicioDeReintegro servicio) =>
    Results.Ok((await servicio.LevantamientosAsync()).Select(l => new
    {
        id = l.Id.ToString(),
        mision = l.MisionId.ToString(),
        responsable = l.Responsable.ToString(),
        persona = l.Persona,
        puesto = l.Puesto,
        momento = new DateTimeOffset(l.MomentoUtc, TimeSpan.Zero)
            .ToOffset(TimeSpan.FromMinutes(l.DesfaseMinutos)),
        motivo = l.Motivo,
    })));

var vales = app.MapGroup("/combustible");

// **La petición NO trae el vehículo.** `RN-32` manda que el sistema lo precargue de la
// orden y no lo capture libremente; pedirlo aunque fuera sólo para rotular la respuesta
// obliga al cliente a conocer la reserva, y el próximo que lea el contrato va a creer que
// es contra ese valor que se valida.
vales.MapPost("/", async (
    EmitirVale peticion, ServicioDeCombustible servicio,
    IParametrosDeLaInstitucion parametros, ServicioDeReintegro reintegro) =>
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
        peticion.Momento,

        // `RN-86`: el bloqueo se arma acá y no lo decide el cliente. Que sea un parámetro
        // obligatorio y no un opcional es deliberado — un endpoint nuevo que se olvide de
        // pasarlo no compila, en vez de emitir sin verificar.
        reintegro);

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

/// `V-02` — entregar el fondo contra firma, **con el control bloqueante de §5.3.B**.
///
/// ── Lo que `BD-06` ya cubría, y lo que no ───────────────────────────────────
/// `BD-06` exige que quien entrega no sea quien emitió: es segregación **dentro del vale**.
/// Los pares que cruzan el vale con la misión —`I-03` solicita, `I-06` autoriza, `I-08`
/// despacha, `I-11` conduce— **no los veía nadie**, porque el vale no conoce el expediente y el
/// expediente no conoce el vale.
vales.MapPost("/{id}/entregar", async (
    string id, EntregarVale peticion, HttpContext http,
    ServicioDeCombustible servicio, ServicioDeMisiones misionesDelVale,
    ServicioDeSegregacion segregacion, ConsultaDeConductores padron) =>
{
    var vale = Ulid.Parse(id);

    // El vale sabe de qué misión es; la misión no sabe del vale. Por eso se entra por acá.
    if (await servicio.BuscarValeAsync(vale) is { } encontrada)
    {
        var expediente = await misionesDelVale.BuscarAsync(encontrada.Mision)
            ?? throw new ExpedienteNoEncontrado(encontrada.Mision);

        var recursos = expediente.Diario.LastOrDefault(t => t.Recursos is not null)?.Recursos;

        IdPersona? quienConduce = null;

        if (recursos is not null &&
            (await padron.TodosAsync()).FirstOrDefault(c => c.Id == recursos.Conductor) is { } c)
        {
            quienConduce = new IdPersona(c.Nombre);
        }

        await segregacion.ExigirAsync(
            new IdPersona(peticion.Ejecuta),
            Funcion.EntregaFondo,
            ActosDeLaMision.De(
                expediente, encontrada.Mision.ToString(), quienConduce, entregoElFondo: null),
            encontrada.Mision.ToString(),
            peticion.Momento,
            origen: http.Connection.RemoteIpAddress?.ToString());
    }

    var estado = await servicio.EntregarAsync(
        vale, new IdPersona(peticion.Ejecuta), peticion.Constancia, peticion.Momento);

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

        // **Las dos cifras, no una.** Lo abastecido es lo que entró al tanque; lo consumido es
        // lo que la misión quemó. Difieren en el remanente, y mostrar sólo una haría que un
        // vehículo que vuelve con el tanque servido pareciera consumir de más.
        abastecidos = r.GalonesAbastecidos,
        galones = r.GalonesConsumidos,

        remanente = r.Remanente is null ? null : new
        {
            // Nulo es «no se pudo calcular», no cero. La explicación dice por qué.
            galones = r.Remanente.Galones,
            calculable = r.Remanente.EsCalculable,
            explicacion = r.Remanente.Explicacion,
        },

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

        // De dónde salió cada galón — `RN-30` punto 4. Va desglosado y no sólo dentro de la
        // evidencia: sin la fuente, cuarenta galones del tanque de la sede y cuarenta comprados
        // con el vale se leen igual, y el conciliador no puede saber cuál mirar.
        composicion = r.Composicion?.Select(c => new
        {
            fuente = c.Key.ToString(),
            galones = c.Value,
        }),

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
// `T-08` — programar es **emitir la Orden de Misión**, que es de `ACT-04`. Hoy sólo cruza con
// `I-14`, que está apagado; se declara igual para que encender `I-14` sea un parámetro y no un
// cambio de código.
ConAsignacion("programar", Funcion.EmiteOrdenDeMision, (e, quien, a, m, p, cuando, recursos, reservas, _, __, ___, operativo, ____, titulo) =>
    e.Programar(quien, a, m, p, cuando, recursos, reservas, operativo, titulo));
// `T-12` — el acto físico de entrega. Convergen `I-02`, `I-05`, `I-08`, `I-09` e `I-11`, y
// hasta ahora **ninguno bloqueaba acá**.
ConAsignacion("despachar", Funcion.Despacha, (e, quien, a, m, p, cuando, _, __, ___, custodias, circulacion, ____, conflicto, _____) =>
    e.Despachar(quien, a, m, p, cuando, custodias, circulacion, conflicto));

// `T-10` — cambiar el vehículo o quien conduce SIN soltar la misión. Comparte la
// resolución de recursos con programar y despachar: es la misma verificación de que el
// identificador existe y la misma construcción de la asignación contra la que se evalúan
// `BD-02` y `BD-03`. Lo único propio es el motivo, y por eso viaja en la misma petición.
// `T-10` — **no es ninguna de las cinco funciones**: cambia el recurso de una misión ya
// programada. Nulo dice eso, y no «se olvidó evaluarla».
ConAsignacion("reasignar", null, (e, quien, a, m, p, cuando, recursos, reservas, peticion, _, __, ___, ____, _____) =>
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

        // **El expediente es opcional en `A-01`**: `RN-83` aplica en misión o fuera de ella, y el
        // reabastecimiento de rutina en el predio no tiene expediente al que colgarse. Exigirlo
        // obligaría al dispositivo a inventar uno.
        var idExpediente = default(Ulid);

        if (h.IdExpediente is { Length: > 0 })
        {
            if (!Identificador.Valido(h.IdExpediente, out idExpediente, out var errorExpediente))
                return errorExpediente;
        }
        else if (h.Transicion != "A-01")
            return Results.BadRequest(new
            {
                mensaje = $"«{h.Transicion}» necesita el expediente al que pertenece.",
            });

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
                    h.Carga.Comprobante, h.Carga.CausaSinComprobante),
            h.Abastecimiento is null
                ? null
                : new AbastecimientoSincronizado(
                    Ulid.Parse(h.Abastecimiento.IdVehiculo), h.Abastecimiento.Fuente,
                    h.Abastecimiento.Galones, h.Abastecimiento.Odometro,
                    h.Abastecimiento.Estacion, h.Abastecimiento.Monto,
                    h.Abastecimiento.Comprobante, h.Abastecimiento.CausaSinComprobante),
            h.NivelDeTanque is { } nivel
                ? new NivelDeTanqueSincronizado(h.EscalaDelNivel, nivel)
                : null,
            h.TanqueNoConsignado));
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

/// <param name="funcion">
/// Cuál de las funciones que el MARCI separa ejerce esta transición — §5.3.B.
///
/// <b>Nulo es «ninguna de las cinco»</b>, no «no se evalúa por descuido». Reasignar no es
/// ninguna: cambia el recurso de una misión ya programada, y no es solicitar, autorizar,
/// despachar, entregar el fondo ni liquidar.
/// </param>
void ConAsignacion(
    string ruta,
    Funcion? funcion,
    Action<OrdenDeMision, IdPersona, AsignacionDeMision, MatrizDeLicencias, PoliticaDeDocumentacion, DateTimeOffset, RecursosTomados?, IReadOnlyList<ReservaDeRecurso>?, AsignarYTransicionar, CustodiaAlDespachar, CirculacionEnDiaInhabil, EstadoOperativo?, ConflictoPorIndisponibilidad, TituloAlProgramar> aplicar) =>
    misiones.MapPost($"/{{id}}/{ruta}", async (
        string id,
        AsignarYTransicionar peticion,
        HttpContext http,
        ServicioDeMisiones servicio,
        ServicioDeSegregacion segregacion,
        ServicioDeCombustible combustible,
        ConsultaDeConductores padron,
        ConsultaDeFlota flota,
        ConsultaDeOcupacion ocupacion,
        ConsultaDeCustodias custodias,
        ConsultaDelOrganigrama organigrama,
        ConsultaDePermisos permisos,
        EstadoDeLaFlota estadoDeLaFlota,
        ServicioDeIndisponibilidad indisponibilidad,
        ServicioDeTitulos titulos,
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

        // `RN-60`: si esta reserva quedo marcada en conflicto por una indisponibilidad del
        // vehiculo y nadie le registro desenlace, el despacho se bloquea. Se consulta SIEMPRE
        // --como la custodia-- porque el dominio exige la respuesta: un llamador que no
        // preguntara apagaria el bloqueo sin darse cuenta.
        var conflicto = await indisponibilidad.ConflictoDeAsync(ulid);

        // Los titulos del vehiculo, SIN filtrar por fecha: cual regia lo decide el dominio
        // contra la ventana de la solicitud, igual que las reservas de `BD-11`.
        var titulo = await titulos.AlProgramarAsync(idVehiculo);

        // §5.3.B — **el control bloqueante**. Va ANTES de la transición porque el acto no se
        // consuma: no se guarda nada, y lo único que queda es el asiento del intento.
        if (funcion is { } pretendida)
        {
            var expedienteAhora = await servicio.BuscarAsync(ulid)
                ?? throw new ExpedienteNoEncontrado(ulid);

            // Quien va a conducir es el de ESTA petición, no el del diario: en el despacho
            // todavía se está decidiendo, y `I-11` compara contra quien efectivamente conduce.
            var quienConduce = new IdPersona(conductor.Nombre);

            // Quien entregó el fondo vive en `M-09`. `V-02` es la entrega.
            var entregoElFondo = (await combustible.DeLaMisionAsync(ulid))
                .Select(v => v.QuienHizo("V-02"))
                .FirstOrDefault(e => e is not null);

            await segregacion.ExigirAsync(
                new IdPersona(peticion.Ejecuta),
                pretendida,
                ActosDeLaMision.De(expedienteAhora, id, quienConduce, entregoElFondo),
                id,
                peticion.Momento,
                origen: http.Connection.RemoteIpAddress?.ToString());
        }

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
                        estadoDelVehiculo,
                        conflicto,

                        // `RN-62` — el titulo se resuelve a la fecha de SALIDA de la mision, que
                        // es la fecha del hecho (P-4). Consultarlo a hoy diria si lo tenemos
                        // ahora, no si lo teniamos cuando la mision sale.
                        titulo);
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
    Action<OrdenDeMision, IdPersona, DateTimeOffset, LecturaResuelta, Ulid?, SubtipoDeRetorno,
           string?, NivelDeTanque?, string?> aplicar) =>
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
                peticion.Subtipo, peticion.Justificacion,
                // Los dos o ninguno: un valor sin escala no se puede interpretar. Ausente
                // queda como «no consignado», que es lo que `RN-80` manda declarar.
                peticion.NivelDeTanque is { } v
                    ? new NivelDeTanque(peticion.EscalaDelNivel, v)
                    : null,
                peticion.TanqueNoConsignado),
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

/// Aplana el camino de salida de `R-3` para el 409. <b>Nulo cuando no hay camino documentado</b>,
/// y la pantalla lo dice en vez de inventar una instruccion.
static object? Salida(CaminoDeSalida? c) =>
    c is null
        ? null
        : new
        {
            quePuedeHacer = c.QuePuedeHacer,
            // Nulo es «no se sabe quien resuelve», que es distinto de «resuelvalo usted».
            aQuienAcudir = c.AQuienAcudir,
            ficha = c.Ficha,
        };

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

/// <param name="Fuente">
/// De dónde salió. **No admite `FondoDeLaMision`**: ése entra por su vale, porque además mueve
/// el instrumento y descuenta del saldo.
/// </param>
/// <param name="IdMision">
/// A qué misión sirvió. Nula en el reabastecimiento de rutina — `RN-83` aplica a todo vehículo
/// **en misión o fuera de ella**.
/// </param>
/// <param name="Monto">
/// Nulo cuando la fuente no lo tiene. Una donación no trae precio, y **un galón sin precio
/// sigue siendo un galón en el denominador**.
/// </param>
internal sealed record RegistrarAbastecimiento(
    string Id,
    string IdVehiculo,
    DateTimeOffset OcurridoEn,
    decimal Galones,
    int Odometro,
    FuenteDeAbastecimiento Fuente,
    string Registra,
    string? IdMision = null,
    decimal? Monto = null,
    string? Estacion = null,
    string? Comprobante = null,
    string? CausaSinComprobante = null,

    // ── El despacho del tanque, cuando lo hay ───────────────────────────────
    // Los cuatro van juntos o ninguno: sin quién despacha y quién recibe no hay despacho,
    // hay una resta. `RN-83` punto 5 exige responsable identificado con la segregación de
    // `RN-01`.
    string? IdTanque = null,
    string? PuestoDespacha = null,
    string? IdReceptor = null,
    string? CombustibleDelVehiculo = null);

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
/// <param name="NivelDeTanque">
/// El nivel del tanque — **dato obligatorio de bitácora** por `RN-83`, y lo que permite separar
/// el remanente del consumo de la misión.
///
/// ⚠️ **Nulo es «no consignado», no cero.** `RN-80`: el campo que no se llenó se declara como
/// no consignado y **no se estima**. Un cero diría que el vehículo salió con el tanque vacío.
/// </param>
/// <param name="EscalaDelNivel">
/// En qué se leyó. Con `FraccionDelIndicador` el valor va de 0 a 1 — `0.125` es un octavo.
/// Se registra porque **un octavo de tanque no es lo mismo en un pickup que en un bus**.
/// </param>
internal sealed record RegistrarOdometro(
    string Ejecuta,
    DateTimeOffset Momento,
    int? Odometro = null,
    SubtipoDeRetorno Subtipo = SubtipoDeRetorno.Ordinario,
    string? Justificacion = null,
    Ulid? IdDeCaptura = null,
    decimal? NivelDeTanque = null,
    EscalaDeNivel EscalaDelNivel = EscalaDeNivel.FraccionDelIndicador,
    /// <summary>Por qué no se leyó. `RN-80` manda declararlo, no estimarlo.</summary>
    string? TanqueNoConsignado = null);

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
    CargaDelDispositivo? Carga = null,
    /// <summary>
    /// El combustible que entró al tanque y **no salió del vale** — `A-01`, `RN-83`.
    ///
    /// El motorista que llena de una donación camino a La Mosquitia, o que pone de su bolsillo
    /// porque el vale no alcanzó, no tenía dónde anotarlo: ese galón no llegaba al denominador y
    /// su ausencia se leía como rendimiento imposible.
    /// </summary>
    AbastecimientoDelDispositivo? Abastecimiento = null,
    /// <summary>
    /// El nivel del tanque leído en el predio — `RN-83`. **Nulo es «no consignado», no cero.**
    /// </summary>
    decimal? NivelDeTanque = null,
    EscalaDeNivel EscalaDelNivel = EscalaDeNivel.FraccionDelIndicador,
    /// <summary>Por qué no se leyó. Va al diario: `RN-80` manda declararlo, no estimarlo.</summary>
    string? TanqueNoConsignado = null);

/// <param name="IdVehiculo">
/// A qué tanque entró. **Es lo único que no puede faltar**: el abastecimiento cuelga del
/// vehículo, no de la misión — `RN-83` aplica «en misión o fuera de ella».
/// </param>
internal sealed record AbastecimientoDelDispositivo(
    string IdVehiculo,
    FuenteDeAbastecimiento Fuente,
    decimal Galones,
    int Odometro,
    string Estacion,
    decimal? Monto = null,
    string? Comprobante = null,
    string? CausaSinComprobante = null);

/// <summary>`R-01` — nominar una obligación de reintegro.</summary>
internal sealed record NominarReintegro(
    string Id,
    string Direccion,
    string Causa,
    string IdResponsable,
    decimal Monto,
    string? IdMision,
    string? IdAsignacion,
    /// <summary>La del hecho original, no la de nominación: `RN-97` cuenta desde ahí.</summary>
    DateOnly FechaDelHecho,
    string Persona,
    string Puesto,
    string Motivo,
    DateTimeOffset Momento);

/// <summary>`R-02` a `R-06` sobre una obligación existente.</summary>
internal sealed record MoverReintegro(
    string Movimiento,
    string Persona,
    string Puesto,
    string Texto,
    decimal? Monto,
    /// <summary>Sólo `R-06`: cuándo entró el dinero a la caja.</summary>
    DateOnly? FechaDelHecho,
    DateTimeOffset Momento);

internal sealed record LevantarBloqueo(
    string IdMision,
    string IdResponsable,
    string Persona,
    string Puesto,
    string Motivo,
    DateTimeOffset Momento);

/// <summary>Alta del tanque, con su existencia inicial como asiento de apertura.</summary>
internal sealed record AbrirTanque(
    string Id,
    string Nombre,
    string Ambito,
    string TipoDeCombustible,
    decimal? Capacidad,
    decimal ExistenciaInicial,
    string Persona,
    string Puesto,
    DateTimeOffset Momento);

/// <summary>`E-01` recibir · `E-05` constatar · `E-06` ajustar.</summary>
internal sealed record MoverExistencias(
    string Movimiento,
    string Persona,
    string Puesto,
    string Texto,
    /// <summary>Positivo en `E-01`; con signo en `E-06`, que va en las dos direcciones.</summary>
    decimal? Galones,
    decimal? ExistenciaMedida,
    string? MotivoDelAjuste,
    string? Comprobante,
    DateTimeOffset Momento);

internal sealed record Trasiego(
    string IdOrigen,
    string IdDestino,
    decimal Galones,
    string Persona,
    string Puesto,
    DateTimeOffset Momento);

/// <summary>Un punto de la ruta y **cuántas veces se cruza** — no cuántos puntos hay.</summary>
internal sealed record CruceDeclarado(string IdPunto, int Cruces);

internal sealed record EstimarPeajes(
    IReadOnlyList<CruceDeclarado> Cruces,
    /// <summary>Nulo en la estimación previa de `T-02`: todavía no hay unidad asignada.</summary>
    string? IdVehiculo,
    string? CategoriaDelTipo,
    string? TipoDeVehiculo,
    DateOnly FechaPrevista);

internal sealed record RegistrarPaso(
    string Id,
    string? IdPunto,
    string IdVehiculo,
    string? IdMision,
    DateTimeOffset OcurridoEn,
    int Odometro,
    decimal MontoPagado,
    MedioDePagoDelPeaje Medio,
    string Registra,
    /// <summary>Con la que cobró la caseta. Nula cuando el ticket no la dice.</summary>
    string? CategoriaCobrada = null,
    string? Ticket = null,
    string? CausaSinTicket = null,
    bool PuntoNoCatalogado = false,
    string? UbicacionDeclarada = null,
    string? IdDeCaptura = null);

internal sealed record AbrirPunto(
    string Id, string Nombre, string Operador, string Carretera,
    string? SentidoDeCobro, EstadoDelPunto Estado, string Fundamento, DateOnly VigenteDesde,
    /// <summary>El corredor y el kilómetro: lo que permite ordenar geográficamente.</summary>
    string? Corredor = null, int? Kilometro = null);

internal sealed record CambiarEstadoDelPunto(
    EstadoDelPunto Estado, string Fundamento, DateOnly VigenteDesde);

internal sealed record CargarCategoria(string Codigo, string Nombre);

internal sealed record CargarTarifa(
    string IdPunto, string Categoria, decimal Monto,
    /// <summary>SAPP, COVI-H, contrato o comunicado de la SIT. **Sin ella no se guarda.**</summary>
    string Fuente, DateOnly FechaDeVerificacion, DateOnly VigenteDesde);

internal sealed record CargarRegla(
    string Categoria, int Prioridad, string Fundamento, DateOnly VigenteDesde,
    ClaseNormativa? Clase = null, string? TipoDeVehiculo = null,
    int? PesoBrutoDesdeKg = null, int? PesoBrutoHastaKg = null,
    int? EjesDesde = null, int? EjesHasta = null,
    int? PasajerosDesde = null, int? PasajerosHasta = null,
    bool? LlevaRemolque = null);

internal sealed record CargarExoneracion(
    string IdVehiculo, string? IdPunto, string? Operador, string Fundamento,
    DateOnly VigenteDesde, DateOnly? VigenteHasta);

internal sealed record CongelarEstimado(
    string IdMision,
    IReadOnlyList<CruceDeclarado> Cruces,
    string? IdVehiculo,
    string? CategoriaDelTipo,
    string? TipoDeVehiculo,
    DateOnly FechaPrevista,
    string Congela,
    DateTimeOffset Momento);

internal sealed record DeclararDesvio(
    string IdMision,
    string IdVehiculo,
    /// <summary>La fecha del hecho: el derrumbe ocurrió a una hora, no cuando hubo señal.</summary>
    DateTimeOffset Desde,
    DateTimeOffset? Hasta,
    string Motivo,
    string Declara,
    string? IdDeCaptura = null);

internal sealed record RegistrarFuente(
    string Id,
    TipoDeFuenteExterna Tipo,
    string Emisor,
    string Formato,
    string Responsable,
    bool Disponible,
    int? PeriodicidadEnDias = null,
    /// <summary>Obligatorio cuando no está disponible: «no disponible» ≠ «conciliada».</summary>
    string? PorQueNoEstaDisponible = null);

/// <summary>Una línea del estado de cuenta, tal como la emitió el proveedor.</summary>
internal sealed record LineaDeclarada(
    string Id,
    /// <summary>Cuándo ocurrió, **no el período del estado de cuenta** (`RN-46`).</summary>
    DateOnly FechaDelHecho,
    decimal Monto,
    string? Referencia = null,
    string? Descripcion = null,
    string? BienDelInventario = null,
    string? Chasis = null,
    string? Motor = null,
    string? Correlativo = null,
    string? Placa = null);

internal sealed record EjecutarConciliacion(
    string IdFuente,
    DateOnly Desde,
    DateOnly Hasta,
    IReadOnlyList<LineaDeclarada> Lineas,
    /// <summary>El archivo o documento del que salieron las líneas. **Obligatorio.**</summary>
    string DocumentoFuente,
    string Ejecuta,
    string ResponsableDeSeguimiento,
    DateOnly Plazo,
    DateTimeOffset Momento,
    /// <summary>Con qué competencia se ejecuta. Va al expediente que abre cada diferencia.</summary>
    string? Puesto = null,
    int? ToleranciaEnDias = null);

internal sealed record ResolverDiferencia(string Resolucion, DateTimeOffset Momento);

internal sealed record AbrirHallazgo(
    string Id,
    /// <summary>Del catálogo `tipo_de_hallazgo_posterior`. Tipificado, no libre.</summary>
    string Tipo,
    /// <summary>Cuándo ocurrió. **La antigüedad se cuenta desde acá.**</summary>
    DateOnly FechaDelHecho,
    /// <summary>Cuándo se descubrió. **Campo distinto, y ambos obligatorios.**</summary>
    DateOnly FechaDelDescubrimiento,
    string ComoSeDescubrio,
    string Fuente,
    string Persona,
    string Puesto,
    DateTimeOffset Momento,
    string? DocumentoAdjunto = null,
    /// <summary>Cero, una o varias. **Cero es el caso interesante.**</summary>
    IReadOnlyList<string>? Misiones = null,
    string? IdVehiculo = null,
    string? IdMotorista = null,
    string? Periodo = null);

internal sealed record AsentarReverso(
    string TipoDeAsiento,
    /// <summary>El identificador exacto. **No existe el reverso genérico «de la misión».**</summary>
    string IdentificadorDelAsiento,
    string DescripcionDelAsiento,
    NaturalezaDelReverso Naturaleza,
    /// <summary>Siempre. Sin él el reporte sólo muestra dos de los tres valores.</summary>
    string ValorAnterior,
    DateOnly FechaDelHechoOriginal,
    string Persona,
    string Puesto,
    string Autoriza,
    /// <summary>Quien produjo el asiento. **No puede ser quien autoriza** (`BD-06`).</summary>
    string AutorDelAsientoOriginal,
    string MotivoTipificado,
    string Fundamento,
    string PeriodoAfectado,
    /// <summary>El corriente. Distinto del afectado cuando hay efecto económico.</summary>
    string PeriodoDeImputacion,
    DateTimeOffset Momento,
    /// <summary>Siempre, **incluso nulo**: nulo significa sin valor correcto conocido.</summary>
    string? ValorNuevo = null,
    string? Adjunto = null,
    decimal? EfectoEconomico = null,
    IReadOnlyList<string>? TablasParametricas = null);

internal sealed record VincularMision(
    string IdMision, string Persona, string Puesto, string Motivo, DateTimeOffset Momento);

internal sealed record ResolverHallazgo(
    ResolucionDelHallazgo Resolucion, string Fundamento,
    string Persona, string Puesto, DateTimeOffset Momento);

internal sealed record ProducirSaldo(
    string Id,
    /// <summary>Sin folio no se puede citar en el acta de cierre.</summary>
    string Folio,
    string Ejercicio,
    DateOnly Corte,
    string Persona,
    string Puesto,
    DateTimeOffset Momento,
    /// <summary>
    /// El motivo para producirlo con préstamos vencidos o interrupciones sin desenlace vivos.
    /// `RN-97` punto 4: hay que resolverlos **o declararlos explícitamente**.
    /// </summary>
    string? DeclaracionDeBloqueantes = null);

internal sealed record ResolverRenglon(string ComoSeResolvio, DateOnly Fecha);

/// <summary>Producir el acta de cierre de ejercicio — `RN-96` punto 1.</summary>
internal sealed record ProducirActaDeCierre(
    /// <summary>Sin folio el saldo de apertura no tiene a qué acta corresponder.</summary>
    string Folio,
    string Ejercicio,
    string Persona,
    string Puesto,
    DateTimeOffset Momento);

/// <summary>
/// Anular los folios que el acta listó — `RN-96` punto 5.
///
/// El motivo es lo que distingue el acta de anulación de un borrado en bloque.
/// </summary>
internal sealed record AnularFolios(string Persona, string Motivo, DateTimeOffset Momento);

// ── M-12 · Incidentes ───────────────────────────────────────────────────────
//
// **Ninguno de estos contratos tiene un campo de responsabilidad, culpa o dolo.** `RN-74`: el
// formulario de campo captura hechos observables, y la responsabilidad la determina la instancia
// competente en su propio acto.

/// <param name="Interrumpe">
/// Si impidió continuar la misión. Lo declara quien registra: una avería leve no interrumpe y
/// una que dejó el vehículo en la carretera sí.
/// </param>
internal sealed record RegistrarIncidente(
    string Id,
    TipoDeIncidente Tipo,
    /// <summary>Del catálogo `causa_interrupcion`, configurable según `RN-70`.</summary>
    string Causa,
    /// <summary>Cuándo pasó.</summary>
    DateTimeOffset MomentoDelHecho,
    /// <summary>Cuándo se capturó. `RN-70` admite captura sin ninguna conectividad.</summary>
    DateTimeOffset MomentoDeCaptura,
    string Descripcion,
    string Registra,
    string ResponsableDeSeguimiento,
    DateOnly Plazo,
    bool Interrumpe,
    string? IdMision,
    string? IdVehiculo,
    string? Ubicacion,
    int? Odometro,
    IReadOnlyList<BienDelIncidente>? Bienes);

internal sealed record BienDelIncidente(string Descripcion, bool EsElVehiculo);

internal sealed record AdjuntarConstancia(
    string Numero, string Autoridad, DateOnly Fecha, string Ejecuta, DateTimeOffset Momento);

internal sealed record RegistrarDesenlace(
    DesenlaceDeLaInterrupcion Desenlace,
    /// <summary>Quién lo autorizó y contra qué acto. `RN-70` lo exige en los cuatro desenlaces.</summary>
    string Detalle,
    string Ejecuta,
    DateTimeOffset Momento);

internal sealed record RegistrarGestion(
    DateOnly Fecha, string Descripcion, string Responsable, DateOnly Plazo,
    string Ejecuta, DateTimeOffset Momento);

internal sealed record RecuperarBien(string Donde, string Ejecuta, DateTimeOffset Momento);

internal sealed record DescargarBien(
    string Numero, string Autoridad, DateOnly Fecha, string Ejecuta, DateTimeOffset Momento);

/// <summary>El acto de OTRA instancia. SIGTI lo registra, no lo produce (`RN-74`).</summary>
internal sealed record AdjuntarDeterminacion(
    string Numero, string Instancia, DateOnly Fecha, string Resolucion,
    string Ejecuta, DateTimeOffset Momento);

internal sealed record ResolverIncidente(
    string ComoSeResolvio,
    DateOnly Fecha,
    string Ejecuta,
    DateTimeOffset Momento,
    /// <summary>Por qué se cierra con bienes todavía afuera — `RN-75`.</summary>
    string? DeclaracionDeBienes);

// ── RN-63 · El préstamo ─────────────────────────────────────────────────────

/// <param name="ConMotoristaPropio">
/// Si el vehículo se cede con motorista de la institución propietaria. **Bloquea**: eso no es un
/// préstamo, es una Orden de Misión con motivo «apoyo institucional».
/// </param>
internal sealed record PrestarVehiculo(
    string Id,
    string IdVehiculo,
    string ActoFolio,
    string ActoFirmante,
    DateOnly ActoFecha,
    string? ActoAdjunto,
    /// <summary>Quien autoriza. **No puede ser el receptor** — `RN-63` punto 2.</summary>
    string Autoriza,
    string ReceptorPersona,
    string ReceptorCargo,
    string ReceptorInstitucion,
    string ReceptorConstancia,
    string Motivo,
    DateOnly Desde,
    /// <summary>La fecha pactada. Vencerla pone el préstamo en mora y bloquea el cierre.</summary>
    DateOnly DevolucionComprometida,
    int EntregaOdometro,
    string EntregaFirma,
    string? EntregaCombustible,
    string? EntregaAccesorios,
    string? EntregaDocumentos,
    bool EntregaRotulacion,
    string? EntregaNovedades,
    string? RubroCombustible,
    string? RubroPeajes,
    string? RubroMantenimiento,
    string? RubroMultas,
    string? RubroDanios,
    bool ConMotoristaPropio);

internal sealed record DevolverVehiculo(
    DateOnly Fecha,
    int Odometro,
    string Firma,
    string? NivelDeCombustible,
    /// <summary>La **reconstatación** de `RN-63` punto 6.</summary>
    bool RotulacionConstatada,
    string? Novedades,
    /// <summary>**No puede ser quien recibió**: el acta sería una autodeclaración.</summary>
    string QuienFirmaLaDevolucion);

// ── RN-60 · Indisponibilidad ────────────────────────────────────────────────

internal sealed record DeclararIndisponibilidad(
    string Id,
    string IdVehiculo,
    /// <summary>`EnTaller` o `NoDisponible`: los dos que no habilitan asignación.</summary>
    EstadoOperativo Estado,
    /// <summary>Del catálogo `causa_indisponibilidad`, configurable según `RN-60`.</summary>
    string Causa,
    DateOnly Desde,
    /// <summary>Con fecha de fin, siempre: es contra ella que se contrasta la real.</summary>
    DateOnly FinEstimado,
    string Ejecuta,
    /// <summary>Cuándo acusó sobre la lista de reservas afectadas.</summary>
    DateTimeOffset MomentoDelAcuse);

internal sealed record ResolverReserva(
    DesenlaceDeLaReserva Desenlace, string Ejecuta, string Motivo, DateTimeOffset Momento);

internal sealed record DarDeAlta(
    DateOnly FinReal, string OrdenDeTrabajo, int OdometroDeSalida);

// ── RN-62 · El título de tenencia ───────────────────────────────────────────

/// <param name="Hasta">
/// **Nula sólo en propiedad**, que es el único régimen que no vence. En los demás su ausencia
/// haría que el título no venciera nunca — y un comodato que no vence es una apropiación.
/// </param>
internal sealed record CerrarTarea(
    string Ejecuta,
    /// <summary>Qué se hizo. Sin esto, «lo autorizó el jefe» y «ya no hacía falta» se ven igual.</summary>
    string Motivo,
    /// <summary>
    /// <b>Descartar no es resolver.</b> Descartar dice que nadie tuvo que hacer nada, y juntarlas
    /// impediría distinguir el control que operó del que se volvió innecesario.
    /// </summary>
    bool Descartar,
    DateTimeOffset Momento);

internal sealed record OtorgarCompetencia(
    string Id,
    string Puesto,
    Rol Rol,
    AlcanceDeDatos Alcance,
    DateOnly Desde,
    DateOnly? Hasta,
    /// <summary>Quien otorga. El otorgamiento es un acto y tiene autor.</summary>
    string Otorga);

internal sealed record CerrarCompetencia(DateOnly Hasta);

internal sealed record RegistrarTitulo(
    string Id,
    string IdVehiculo,
    RegimenDeTenencia Regimen,
    /// <summary>Quién es el propietario o cedente.</summary>
    string Titular,
    /// <summary>Convenio, contrato, acta o resolución. Una prórroga verbal no existe.</summary>
    string Documento,
    DateOnly Desde,
    DateOnly? Hasta,
    QuienAsume Combustible,
    QuienAsume Mantenimiento,
    QuienAsume Llantas,
    QuienAsume Seguro,
    QuienAsume Peajes,
    QuienAsume Multas,
    QuienAsume Danios);

/// Lo que sube el cliente de campo. <b>La posicion es opcional</b>: exigirla haria que el
/// motorista no pudiera declarar su estado donde mas importa — donde no hay senal.
public sealed record ReporteEntrante(
    string Tipo,
    DateTimeOffset MomentoDelHecho,
    string Declara,
    string? Estado = null,
    string? Destino = null,
    decimal? Latitud = null,
    decimal? Longitud = null,
    int? PrecisionMetros = null,
    string? CausaDeEspera = null,
    string? SeAtribuyeA = null,
    bool? MotorEncendido = null);


/// Un rango de folios que se reserva a una delegacion.
public sealed record AsignarRango(
    string Delegacion,
    string TipoDeDocumento,
    int Desde,
    int Hasta,
    string Asigna,
    DateOnly AsignadoEl,
    /// <summary>Nulo es toda la delegacion. Con dos equipos hace falta un subrango a cada uno.</summary>
    string? Dispositivo = null);
