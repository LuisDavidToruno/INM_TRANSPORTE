namespace Sigti.Dominio.Bitacora;

/// <summary>
/// Un asiento de la bitácora append-only (`RNF-04`). Se escribe, nunca se actualiza ni
/// se borra: `RNF-02` lo pone como métrica —«registros eliminados físicamente: 0»—.
///
/// No lleva atributos de persistencia: el mapeo vive en Sigti.Datos, porque el dominio
/// no conoce el ORM (`ADR-009`).
/// </summary>
public sealed class Asiento
{
    /// <summary>Identificador ULID generado en el cliente (`ADR-005`). Es la clave agrupada.</summary>
    public required Ulid Id { get; init; }

    /// <summary>
    /// La cadena a la que pertenece este asiento. El alcance del encadenamiento fue el
    /// hallazgo `HB34-51`: no todo cuelga de una misión.
    /// </summary>
    public required string Cola { get; init; }

    /// <summary>
    /// Secuencia monótona dentro de la cola. <b>Es la que fija el orden de la cadena</b>,
    /// no la marca de tiempo: el reloj del dispositivo no es confiable (`ADR-007`).
    /// </summary>
    public required long Secuencia { get; init; }

    public required string Contenido { get; init; }

    /// <summary>Hash que encadena contra el asiento anterior de esta cola.</summary>
    public required string Hash { get; init; }

    /// <summary>Cuándo ocurrió el hecho, en UTC (`ADR-007`).</summary>
    public required DateTime MomentoUtc { get; init; }

    /// <summary>
    /// Desfase local del dispositivo al capturar. Se guarda porque el hecho ocurrió en
    /// una hora local concreta, y hay reglas —día y hora inhábil— que dependen de ella.
    /// </summary>
    public required int DesfaseMinutos { get; init; }

    /// <summary>Cuándo llegó al servidor. Distingue «cuándo pasó» de «cuándo lo supimos».</summary>
    public required DateTime MomentoRecibidoUtc { get; init; }
}
