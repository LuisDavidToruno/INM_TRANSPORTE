using Sigti.Dominio.M19_Seguimiento;

namespace Sigti.Datos.M19_Seguimiento;

/// <summary>
/// El reporte de campo, tal como se guarda — M-19.
///
/// ── Las dos fechas son dos columnas, no una ─────────────────────────────────
/// `RN-46`. Guardar sólo una obligaría a elegir cuál se pierde, y las dos se necesitan: la del
/// hecho para calcular la antigüedad y ordenar los reportes, la de captura para saber cuánto
/// estuvo el dispositivo sin cobertura. Cuatro días de distancia entre ambas es operación
/// normal (`RN-43`), no un error a corregir.
/// </summary>
public sealed class FilaDeReporteDeCampo
{
    public required Ulid Id { get; init; }

    public required Ulid MisionId { get; init; }

    public required TipoDeReporte Tipo { get; init; }

    /// <summary>Del catálogo `estado_en_ruta`. Nulo en arribo y salida.</summary>
    public string? Estado { get; init; }

    /// <summary>Nulo en la declaración de estado: se declara en cualquier punto de la ruta.</summary>
    public string? Destino { get; init; }

    // ── Cuándo pasó, por el reloj del dispositivo ───────────────────────────
    public required DateTime MomentoDelHechoUtc { get; init; }
    public required int DesfaseDelHechoMinutos { get; init; }

    // ── Cuándo llegó al servidor ────────────────────────────────────────────
    public required DateTime MomentoDeCapturaUtc { get; init; }
    public required int DesfaseDeCapturaMinutos { get; init; }

    /// <summary>
    /// Nulas juntas o presentes juntas: <b>media posición no es una posición</b>. Nulo dice que
    /// el dispositivo no tenía fijado el GPS, que es distinto de estar en el meridiano cero.
    /// </summary>
    public decimal? Latitud { get; init; }

    public decimal? Longitud { get; init; }

    /// <summary>Nula cuando el dispositivo no la informó. Con 3 km de error el punto es otro dato.</summary>
    public int? PrecisionMetros { get; init; }

    /// <summary>Del catálogo `causa_de_espera`. Nula mientras la espera siga sin tipificar.</summary>
    public string? CausaDeEspera { get; init; }

    public string? SeAtribuyeA { get; init; }

    /// <summary><b>Nulo es «no se preguntó»</b>, que no es «apagado» (`RN-30`).</summary>
    public bool? MotorEncendido { get; init; }

    public required string Declara { get; init; }
}
