namespace Sigti.Datos.M18_Peajes;

/// <summary>
/// Un punto del estimado <b>congelado al aprobar</b> — `RN-35` punto 4 y `RN-41`.
///
/// ── Por qué se congela y no se recalcula ────────────────────────────────────
/// Porque es <b>lo que el autorizador autorizó</b>. Recalcularlo después haría que la pregunta
/// «¿esta caseta estaba en la ruta aprobada?» se contestara contra la ruta de hoy — y entonces
/// un cambio de destino posterior borraría el desvío en vez de mostrarlo.
///
/// Es además lo único contra lo que `RN-37` puede evaluar su tercera dimensión: sin esto, toda
/// caseta parecería fuera de ruta o ninguna lo parecería.
/// </summary>
public sealed class FilaDeRutaAutorizada
{
    public required Ulid Id { get; init; }

    public required Ulid MisionId { get; init; }

    public required Ulid PuntoId { get; init; }

    /// <summary>Cuántas veces se autorizó cruzarlo. `RN-35`: cruces, no puntos distintos.</summary>
    public required int Cruces { get; init; }

    /// <summary>
    /// Lo estimado para este punto. <b>Nulo cuando la línea no se pudo valorar</b> — sin tarifa
    /// cargada, sin categoría resuelta. Nunca cero: un cero diría que este punto no cuesta.
    /// </summary>
    public decimal? Subtotal { get; init; }

    /// <summary>La fila de tarifa que se usó, congelada con el resto (`RN-41`).</summary>
    public Ulid? TarifaId { get; init; }

    public required DateTime CongeladoUtc { get; init; }

    /// <summary>
    /// Qué recongelamiento la superó. <b>Nulo es la línea vigente</b>, que es el caso normal.
    ///
    /// ── Por qué no se borra ni se sobrescribe ───────────────────────────────
    /// `RN-61` es explícito: <i>«el asiento anterior no se sobrescribe: la asignación original
    /// se conserva junto a la sustituta»</i> (`RN-04`). Un estimado que se pisa deja al auditor
    /// sin poder contestar qué se autorizó originalmente ni cuánto cambió — y el estimado es lo
    /// que el autorizador autorizó, no una cifra de trabajo.
    ///
    /// ⚠️ <b>Todo lector tiene que filtrar por esto.</b> Una línea superada que siga contando
    /// haría que `RN-37` —«¿esta caseta estaba en la ruta aprobada?»— se contestara contra dos
    /// rutas a la vez, y un desvío desaparecería por coincidir con la ruta vieja.
    /// </summary>
    public Ulid? SupersedidaPor { get; set; }

    public required int DesfaseMinutos { get; init; }

    /// <summary>Quién congeló el estimado. El acto tiene autor como todo lo demás.</summary>
    public required string Congela { get; init; }
}

/// <summary>
/// Un desvío declarado desde el campo — el mínimo que `RN-37` necesita de `RN-76`.
///
/// ── Sin esto la regla no se puede encender ──────────────────────────────────
/// `RN-37`: Honduras tiene derrumbes y cierres de carretera con regularidad, y sin poder
/// declararlos la regla <i>«produciría hallazgos falsos en masa»</i>. Un control que grita todos
/// los días es un control que nadie mira.
/// </summary>
public sealed class FilaDeDesvio
{
    public required Ulid Id { get; init; }

    public required Ulid MisionId { get; init; }

    /// <summary>
    /// El vehículo. <b>La secuencia se valida por vehículo y no por misión</b> (`RN-37`): en una
    /// sustitución en ruta, dos vehículos pueden pasar por la misma caseta a horas distintas.
    /// </summary>
    public required Ulid VehiculoId { get; init; }

    /// <summary>
    /// La <b>fecha del hecho</b>: el derrumbe ocurrió a una hora, no cuando hubo señal para
    /// reportarlo (`RN-46`).
    /// </summary>
    public required DateTime DesdeUtc { get; init; }

    public required int DesfaseDesde { get; init; }

    /// <summary>Nulo mientras el desvío sigue vigente.</summary>
    public DateTime? HastaUtc { get; init; }

    public int? DesfaseHasta { get; init; }

    /// <summary>Obligatorio. Un desvío sin motivo no explica nada.</summary>
    public required string Motivo { get; init; }

    public required string Declara { get; init; }

    /// <summary>
    /// El identificador del dispositivo. El desvío se declara <b>desde el campo y sin
    /// conectividad</b>, y el reintento duplicaría la justificación.
    /// </summary>
    public Ulid? IdDeCaptura { get; init; }
}
