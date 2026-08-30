using Sigti.Dominio.M16_Sincronizacion;

namespace Sigti.Datos.M16_Sincronizacion;

/// <summary>
/// Un conflicto de sincronización, tal como se guarda — `RN-45`.
///
/// ── Las dos versiones viven en la misma fila ────────────────────────────────
/// <b>Ninguna de las dos se pierde</b>, y por eso las dos son columnas y no una referencia a lo
/// aplicado más un texto con lo descartado. `RN-45` punto 5: la resolución conserva la versión
/// descartada <b>como asiento vinculado</b>, no como una nota.
/// </summary>
public sealed class FilaDeConflicto
{
    public required Ulid Id { get; init; }

    public required Ulid ExpedienteId { get; init; }

    /// <summary>La transición sobre la que divergen: `T-14`, `T-18`, `V-04`…</summary>
    public required string Transicion { get; init; }

    /// <summary>
    /// El campo que diverge. <b>Uno por fila</b>: dos versiones que difieren en el odómetro y en
    /// la hora de arribo producen <b>dos</b> conflictos, y se deciden por separado. Combinarlos
    /// en uno obligaría a resolver los dos con la misma decisión — que es la fusión automática
    /// que `RN-45` prohíbe.
    /// </summary>
    public required string Campo { get; init; }

    /// <summary>
    /// El identificador de captura del hecho que llegó. Sirve para no encolar dos veces el
    /// mismo reintento: el dispositivo que no supo si el servidor recibió <b>va a reenviar</b>.
    /// </summary>
    public required Ulid IdDeCaptura { get; init; }

    // ── La versión que el servidor ya tenía ─────────────────────────────────
    public required string ValorDelServidor { get; init; }
    public required string CapturadaPorServidor { get; init; }
    public required DateTime OcurrioServidorUtc { get; init; }
    public required DateTime RegistradoServidorUtc { get; init; }
    public string? DispositivoDelServidor { get; init; }
    public Ulid? FotoDelServidor { get; init; }

    // ── La versión que llegó del campo ──────────────────────────────────────
    public required string ValorDeCampo { get; init; }
    public required string CapturadaPorCampo { get; init; }
    public required DateTime OcurrioCampoUtc { get; init; }
    public required DateTime RegistradoCampoUtc { get; init; }
    public string? DispositivoDeCampo { get; init; }
    public Ulid? FotoDeCampo { get; init; }

    /// <summary>Pendiente <b>bloquea la liquidación</b> de su misión — `RN-45` punto 4.</summary>
    public required EstadoDelConflicto Estado { get; set; }

    // ── La resolución. Nulas mientras esté pendiente ────────────────────────
    public OrigenElegido? SeTomo { get; set; }
    public string? Motivo { get; set; }
    public string? Resuelve { get; set; }
    public DateTime? ResueltoUtc { get; set; }

    /// <summary>
    /// El criterio declarado cuando vino de un lote. <b>Nulo es «se resolvió una por una»</b>.
    /// Va en cada conflicto y no en un registro aparte: dentro de dos años, quien mire uno solo
    /// tiene que poder ver que salió de un lote y con qué regla.
    /// </summary>
    public string? CriterioDelLote { get; set; }
}
