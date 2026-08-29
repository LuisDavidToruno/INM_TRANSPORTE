using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M12_Incidentes;

/// <summary>
/// Qué clase de hecho abrió el expediente — M-12.
///
/// ── Es la clase del hecho, no su causa ──────────────────────────────────────
/// El tipo cambia lo que el código hace: una sustracción arrastra bienes al registro
/// patrimonial (`RN-75`), una avería no. La <b>causa</b> —por qué pasó— va como texto contra un
/// catálogo configurable que la institución declara, y no como enum: `RN-70` la declara
/// configurable, y cablear una lista de causas obligaría a un despliegue cada vez que aparezca
/// una que nadie había visto.
/// </summary>
public enum TipoDeIncidente
{
    /// <summary>Falla mecánica del vehículo. `CE-02`.</summary>
    AveriaMecanica,

    /// <summary>Accidente de tránsito, con o sin terceros. `CE-03`.</summary>
    Accidente,

    /// <summary>Robo o hurto del vehículo o de la carga. `CE-04`.</summary>
    Sustraccion,

    /// <summary>
    /// Decomiso o retención por autoridad. <b>Distinto de la sustracción</b>: acá se sabe quién
    /// tiene el bien y bajo qué expediente, y `RN-75` exige registrar ambas cosas.
    /// </summary>
    RetencionPorAutoridad,

    /// <summary>El conductor no puede seguir. `CE-10`.</summary>
    IncapacidadDelConductor,

    /// <summary>Vía cerrada, derrumbe, inundación.</summary>
    ViaImpracticable,

    /// <summary>Condición de seguridad que impide continuar.</summary>
    CondicionDeSeguridad,

    /// <summary>Multa de tránsito imputada al vehículo. <b>No interrumpe</b>.</summary>
    Multa,

    /// <summary>Uso del vehículo fuera de lo autorizado.</summary>
    UsoIndebido,
}

/// <summary>
/// Cómo se resolvió una interrupción — `RN-70`.
///
/// <b>Los cuatro que la regla enumera, ni uno más.</b> Un quinto desenlace «otro» dejaría que la
/// mitad de las interrupciones se resolvieran sin decir cómo, y el desenlace existe justamente
/// para que no se pueda.
/// </summary>
public enum DesenlaceDeLaInterrupcion
{
    /// <summary>Continuar con el mismo vehículo y conductor, con constancia de quién autorizó.</summary>
    Continuar,

    /// <summary>Continuar sustituyendo vehículo o conductor (`RN-61`, `RN-71`).</summary>
    ContinuarConSustitucion,

    /// <summary>Retorno anticipado — `T-18` subtipo retorno anticipado (`RN-78`).</summary>
    RetornoAnticipado,

    /// <summary>Retorno sin vehículo, con la unidad resguardada o retenida (`RN-75`).</summary>
    RetornoSinVehiculo,
}

/// <summary>
/// En qué situación está un bien afectado — `RN-75`.
///
/// <b>Ninguno de estos estados borra el bien del registro patrimonial.</b> `RN-75` es explícita:
/// <i>«permanece en el registro patrimonial hasta su recuperación o su descargo formal. Nunca se
/// elimina»</i>.
/// </summary>
public enum EstadoDelBien
{
    /// <summary>Sustraído, retenido o perdido. Sigue en el registro, con su ubicación conocida.</summary>
    NoRecuperado,

    /// <summary>Volvió. El expediente conserva que estuvo afuera y cuánto tiempo.</summary>
    Recuperado,

    /// <summary>
    /// Salió del registro <b>por acto formal</b>, con su número y autoridad. Es la única salida
    /// que no es la recuperación, y por eso exige constancia.
    /// </summary>
    Descargado,
}

/// <summary>
/// Un bien afectado por el incidente — `RN-75`.
/// </summary>
/// <param name="UbicacionConocida">
/// Dónde está, si se sabe. <b>Nula es «no se sabe»</b>, y `RN-75` la exige junto con la autoridad
/// custodia y el número de expediente mientras dure la situación: un bien sin ubicación ni
/// expediente es un bien que nadie está buscando.
/// </param>
/// <param name="Descargo">
/// El acto formal que lo saca del registro. Presente exactamente cuando el estado es
/// <see cref="EstadoDelBien.Descargado"/>.
/// </param>
public sealed record BienAfectado(
    Ulid Id,
    string Descripcion,
    bool EsElVehiculo,
    EstadoDelBien Estado,
    DateOnly FechaDelHecho,
    string? UbicacionConocida,
    string? AutoridadCustodia,
    string? NumeroDeExpedienteExterno,
    ConstanciaDeDescargo? Descargo = null)
{
    /// <summary>
    /// Cuántos días lleva fuera. Se cuenta <b>desde el hecho</b>, como toda antigüedad de este
    /// sistema (`RN-97`): un bien que lleva tres años sustraído no se presenta como reciente.
    /// </summary>
    public int DiasFuera(DateOnly hoy) =>
        Estado is EstadoDelBien.NoRecuperado ? hoy.DayNumber - FechaDelHecho.DayNumber : 0;

    /// <summary>Sigue afuera, y por lo tanto sigue en el registro patrimonial.</summary>
    public bool SigueEnElRegistro => Estado is EstadoDelBien.NoRecuperado;
}

/// <summary>
/// El acto formal que descarga un bien del registro — `RN-75`.
///
/// Sin número y autoridad no es un descargo: es una baja sin respaldo, que es exactamente lo que
/// `NRM-02` no admite sobre un bien del Estado.
/// </summary>
public sealed record ConstanciaDeDescargo(
    string Numero,
    string Autoridad,
    DateOnly Fecha);

/// <summary>
/// La constancia de denuncia o acta ante autoridad — `RN-75` punto 2.
///
/// <b>Su ausencia no impide registrar el evento, pero genera obligación con plazo.</b> Exigirla
/// para poder registrar produciría el resultado contrario: el hecho no se registra hasta tener
/// el papel, y para entonces nadie se acuerda de la hora ni del odómetro.
/// </summary>
public sealed record ConstanciaAnteAutoridad(
    string Numero,
    string AutoridadReceptora,
    DateOnly Fecha);

/// <summary>
/// El acto por el que la instancia competente determina responsabilidad — `RN-74` punto 4.
///
/// ── SIGTI lo registra; no lo produce ────────────────────────────────────────
/// `RN-74`: <i>«La responsabilidad se determina en el expediente de investigación de M-12, por la
/// instancia que corresponde, con procedimiento, descargo del interesado, resolución y
/// notificación. El sistema <b>registra</b> esa determinación cuando existe, con su acto y su
/// autor; <b>no la produce</b>»</i>.
///
/// Por eso esto es un documento con emisor y número, y no un campo «responsable» que alguien
/// llena en una pantalla.
/// </summary>
public sealed record DeterminacionDeResponsabilidad(
    string Numero,
    string InstanciaQueLaEmite,
    DateOnly Fecha,
    string Resolucion);

/// <summary>
/// Un movimiento del expediente de incidente — el diario `I-01` a `I-08`.
/// </summary>
public sealed record MovimientoDelIncidente(
    string Movimiento,
    DateTimeOffset Momento,
    string Ejecuta,
    string? Detalle);

/// <summary>
/// Una gestión de recuperación — `RN-75`.
///
/// <b>Con responsable y plazo</b>, que es lo que la regla exige: un expediente que dice «se están
/// haciendo gestiones» sin decir quién ni para cuándo no se puede seguir.
/// </summary>
public sealed record GestionDeRecuperacion(
    DateOnly Fecha,
    string Descripcion,
    string Responsable,
    DateOnly Plazo);

/// <summary>
/// El expediente de incidente — M-12.
///
/// ── Lo que este tipo NO tiene, y es lo que lo define ────────────────────────
/// <b>Ningún campo de responsabilidad, culpa o dolo.</b> `RN-74` lo prohíbe en la captura, y la
/// razón está escrita en la regla: <i>«un motorista que acaba de tener un accidente, a la orilla
/// de la carretera, con un tercero gritándole, no está en condiciones de calificar jurídicamente
/// lo que pasó — y no le corresponde»</i>. Y la consecuencia práctica: <i>«si registrar el hecho
/// implica autoinculparse, <b>el hecho no se registra</b>. Y un accidente no registrado es peor
/// que cualquier atribución mal hecha»</i>.
///
/// Lo único parecido es <see cref="Determinacion"/>, que es un <b>documento emitido por otra
/// instancia</b> y se adjunta cuando existe.
///
/// ── Y lo que no le hace a la misión ─────────────────────────────────────────
/// <b>No le cambia el estado.</b> `RN-70`: <i>«el evento marca la misión como interrumpida y no
/// le cambia el estado. La Orden de Misión sigue `EN_RUTA`: el vehículo salió y hubo consumo real
/// de recursos públicos»</i>.
/// </summary>
/// <param name="FechaDelHecho">
/// Cuándo pasó, no cuándo se capturó. `RN-70` lo subraya para el estado del vehículo: <i>«desde
/// la hora del hecho — no desde la hora de captura»</i>.
/// </param>
/// <param name="Causa">
/// Del catálogo `causa_interrupcion`, que `RN-70` declara configurable.
///
/// ⚠️ <b>Hoy es texto validado contra no-vacío, no un catálogo.</b> La institución no ha
/// declarado sus causas, y cablear una lista obligaría a un despliegue cada vez que aparezca una
/// que nadie previó. Se dice acá en vez de fingir que está resuelto.
/// </param>
/// <param name="Interrumpe">
/// Si el hecho impidió continuar la misión según lo autorizado. <b>Lo declara quien registra</b>,
/// y no se deduce del tipo: una avería leve que se resolvió en la orilla no interrumpe, y una
/// que dejó el vehículo en la carretera sí. Deducirlo del tipo pondría marca de interrupción a
/// hechos que no la tuvieron —y entonces exigiría desenlace a expedientes que no lo necesitan.
/// </param>
public sealed record ExpedienteDeIncidente(
    Ulid Id,
    TipoDeIncidente Tipo,
    string Causa,
    DateOnly FechaDelHecho,
    DateTimeOffset MomentoDelHecho,
    DateTimeOffset MomentoDeCaptura,
    string Descripcion,
    string Registra,
    Ulid? MisionId,
    Ulid? VehiculoId,
    string? Ubicacion,
    int? Odometro,
    bool Interrumpe,
    IReadOnlyList<MovimientoDelIncidente> Movimientos,
    IReadOnlyList<BienAfectado> Bienes,
    IReadOnlyList<GestionDeRecuperacion> Gestiones,
    string ResponsableDeSeguimiento,
    DateOnly Plazo,
    ConstanciaAnteAutoridad? Constancia = null,
    DesenlaceDeLaInterrupcion? Desenlace = null,
    string? DetalleDelDesenlace = null,
    DeterminacionDeResponsabilidad? Determinacion = null,
    DateOnly? ResueltoEn = null,
    string? ComoSeResolvio = null)
{
    /// <summary>
    /// <b>Marca de interrupción sin desenlace.</b> `RN-70`: <i>«ninguna misión con marca de
    /// interrupción sin desenlace puede quedar viva al cierre del período»</i> (`RN-97` punto 4).
    ///
    /// Es la propiedad que hace que ese bloqueo pueda disparar — hasta que M-12 existió, no
    /// tenía de dónde salir.
    /// </summary>
    public bool EsInterrupcionSinDesenlace => Interrumpe && Desenlace is null;

    /// <summary>Sigue abierto. Un expediente resuelto no entra al saldo de apertura.</summary>
    public bool EstaAbierto => ResueltoEn is null;

    /// <summary>
    /// Los bienes que siguen fuera. <b>Nunca se borran del expediente</b>: `RN-75` los conserva
    /// hasta la recuperación o el descargo formal.
    /// </summary>
    public IReadOnlyList<BienAfectado> BienesNoRecuperados =>
        [.. Bienes.Where(b => b.SigueEnElRegistro)];

    /// <summary>
    /// El día que se registró, contra el día que pasó. `RN-46` — <b>las dos fechas, siempre</b>:
    /// un incidente capturado cinco días después no es un incidente de ese día.
    /// </summary>
    public int DiasEntreElHechoYLaCaptura =>
        DateOnly.FromDateTime(MomentoDeCaptura.UtcDateTime).DayNumber - FechaDelHecho.DayNumber;

    /// <summary>
    /// Sin constancia de denuncia o acta. `RN-75` punto 2: su ausencia no impide registrar, pero
    /// <b>genera obligación con plazo</b> — y por eso se ve.
    /// </summary>
    public bool DebeConstancia =>
        Constancia is null && Tipo is TipoDeIncidente.Sustraccion
            or TipoDeIncidente.RetencionPorAutoridad or TipoDeIncidente.Accidente;
}
