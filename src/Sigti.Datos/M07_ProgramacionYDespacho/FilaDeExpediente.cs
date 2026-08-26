using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Datos.M07_ProgramacionYDespacho;

/// <summary>
/// Forma de persistencia del expediente. Vive en Sigti.Datos y no en el dominio: el
/// dominio no conoce el ORM (`ADR-009`).
///
/// <b>No tiene columna de estado.</b> Guardarla sería duplicar lo que el diario ya dice,
/// y una copia es algo que se puede desincronizar del original (P-1).
/// </summary>
public sealed class FilaDeExpediente
{
    public required Ulid Id { get; init; }
    public required string CapturadaPor { get; init; }
    public required string SolicitanteDeDerecho { get; init; }
    public List<FilaDeTransicion> Transiciones { get; } = [];
}

/// <summary>Una transición del diario, tal como se guarda.</summary>
public sealed class FilaDeTransicion
{
    public required Ulid Id { get; init; }
    public required Ulid ExpedienteId { get; init; }

    /// <summary>Posición en el diario. El orden del diario es parte del dato, no del azar de la consulta.</summary>
    public required int Orden { get; init; }

    /// <summary>El identificador de la tabla de transiciones: `T-01` a `T-22`.</summary>
    public required string Transicion { get; init; }

    public required EstadoDeMision Destino { get; init; }
    public required string Ejecuta { get; init; }
    public required DateTime MomentoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }
    public string? Motivo { get; init; }
}
