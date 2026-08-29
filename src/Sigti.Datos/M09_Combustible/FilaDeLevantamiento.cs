namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// El levantamiento del bloqueo de `RN-86`, tal como se guarda.
///
/// ── Por qué es una fila y no un campo del vale ──────────────────────────────
/// Porque el acto ocurre <b>antes</b> de que el vale exista: es lo que permite emitirlo. Y
/// porque `RN-86` pide que <i>«la excepción figure en el indicador de levantamientos por
/// persona y por período»</i> — una excepción que sólo se puede leer abriendo el vale que la
/// usó no es un indicador de nada.
///
/// Es también, deliberadamente, un registro <b>incómodo de acumular</b>: cada levantamiento es
/// una fila con nombre, puesto, fecha y motivo, y la lista completa se lee de un vistazo.
/// </summary>
public sealed class FilaDeLevantamiento
{
    public required Ulid Id { get; init; }

    /// <summary>A qué orden se le levantó el bloqueo. <b>No es por persona</b>: `HU-078`.</summary>
    public required Ulid MisionId { get; init; }

    /// <summary>A quién. El ULID del motorista en el padrón.</summary>
    public required Ulid Responsable { get; init; }

    public required string Persona { get; init; }

    /// <summary>Con qué competencia — `RN-86` lo reserva a ACT-08.</summary>
    public required string Puesto { get; init; }

    public required DateOnly FechaDelHecho { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    /// <summary>
    /// Obligatorio. `RN-86`: <i>«se levanta solo por acto registrado de ACT-08 <b>con
    /// motivo</b>»</i>, y `HU-078` rechaza el levantamiento sin motivo escrito.
    /// </summary>
    public required string Motivo { get; init; }
}
