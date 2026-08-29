using Sigti.Dominio.M09_Combustible;

namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// La obligación de reintegro, tal como se guarda — `RN-86`.
///
/// ── Vive fuera del expediente de la misión, y ese es el punto ───────────────
/// `CE-26` §4: la misión cierra por `T-22`, terminal e inmutable, y la obligación queda viva.
/// Si esto colgara del expediente, archivar la misión archivaría el cobro — que es exactamente
/// el agujero que `RN-86` existe para tapar.
/// </summary>
public sealed class FilaDeObligacion
{
    public required Ulid Id { get; init; }

    public required DireccionDelReintegro Direccion { get; init; }

    public required CausaDelReintegro Causa { get; init; }

    /// <summary>
    /// El motorista nominado, por su ULID del padrón. <b>Indexado</b>: la pregunta del bloqueo
    /// de `RN-86` es siempre «¿qué debe esta persona?», y se hace en cada emisión de vale.
    /// </summary>
    public required Ulid Responsable { get; init; }

    public required decimal Monto { get; init; }

    /// <summary>Nulo cuando la obligación nace de un hallazgo posterior sobre un período.</summary>
    public Ulid? MisionId { get; init; }

    public Ulid? AsignacionId { get; init; }

    /// <summary>
    /// La fecha del hecho original, no la de nominación. Es contra ésta que `RN-97` cuenta la
    /// antigüedad al arrastrar la obligación al ejercicio siguiente.
    /// </summary>
    public required DateOnly FechaDelHecho { get; init; }

    public List<FilaDeMovimientoDeObligacion> Movimientos { get; } = [];
}

/// <summary>Un asiento del diario de la obligación — `R-01` a `R-06`.</summary>
public sealed class FilaDeMovimientoDeObligacion
{
    public required Ulid Id { get; init; }

    public required Ulid ObligacionId { get; init; }

    public required int Orden { get; init; }

    public required string Movimiento { get; init; }

    public required EstadoDeObligacion Destino { get; init; }

    /// <summary>Quién. No se reasigna jamás.</summary>
    public required string Persona { get; init; }

    /// <summary>
    /// Con qué competencia, <b>congelado</b>. Va aparte de la persona porque el auditor
    /// pregunta con qué competencia se autorizó, y el puesto pudo cambiar de manos desde
    /// entonces.
    /// </summary>
    public required string Puesto { get; init; }

    /// <summary>La fecha del hecho con la que se resolvió la competencia (`RN-46`).</summary>
    public required DateOnly FechaDelHecho { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    public required string Motivo { get; init; }

    /// <summary>
    /// Lo que este asiento abonó — sólo `R-06`. En columna y no dentro del motivo: el saldo se
    /// calcula sumándolos, y sacar un número de una cadena es el error que este módulo ya
    /// corrigió dos veces.
    /// </summary>
    public decimal? Pagado { get; init; }
}
