namespace Sigti.Datos.M16_Sincronizacion;

/// <summary>
/// Un hecho que llegó antes que aquello de lo que depende — `HU-067`.
///
/// ── Por qué se guarda en vez de rechazarse ──────────────────────────────────
/// El motorista capturó una salida y la envió; el expediente todavía no había llegado al
/// servidor porque el lote se cortó a la mitad. Rechazar ese hecho lo devuelve al dispositivo
/// <b>sin decirle cuándo reintentar</b>, y encolarlo como conflicto le pide a una persona que
/// decida sobre algo que no es una discrepancia: <b>sólo llegó en desorden</b>.
///
/// Se retiene, y cuando llega lo que faltaba se aplica solo. <i>«El hueco se cierra y todo se
/// aplica en orden.»</i>
/// </summary>
public sealed class FilaDeHechoRetenido
{
    public required Ulid Id { get; init; }

    /// <summary>
    /// El identificador que generó el dispositivo. <b>Es la identidad del hecho</b> y lo que
    /// hace inofensivo el reenvío: el mismo hecho retenido dos veces es uno solo.
    /// </summary>
    public required Ulid IdDeCaptura { get; init; }

    /// <summary>De qué depende: el expediente que todavía no está.</summary>
    public required Ulid EsperaExpediente { get; init; }

    public required string Transicion { get; init; }

    public required string Ejecuta { get; init; }

    public required DateTime OcurridoEnUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    /// <summary>Nulo salvo en `T-14` y `T-18`. Nulo es «no consignado», no cero.</summary>
    public int? Odometro { get; init; }

    public string? Dispositivo { get; init; }

    /// <summary>Cuándo llegó y quedó esperando. Es la antigüedad que el panel muestra.</summary>
    public required DateTime RetenidoUtc { get; init; }

    /// <summary>
    /// Cuántas veces se intentó aplicarlo desde entonces. <b>Un retenido que lleva veinte
    /// intentos no espera un predecesor: espera algo que no va a llegar</b>, y eso hay que
    /// poder verlo en el panel antes de que el motorista pregunte.
    /// </summary>
    public required int Intentos { get; set; }
}
