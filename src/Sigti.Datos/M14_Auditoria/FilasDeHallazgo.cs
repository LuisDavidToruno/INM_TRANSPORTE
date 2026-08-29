using Sigti.Dominio.M14_Auditoria;

namespace Sigti.Datos.M14_Auditoria;

/// <summary>
/// El expediente de hallazgo posterior — `RN-93`.
///
/// ── Vive fuera del expediente de la misión, y ese es el punto ───────────────
/// <b>Ni su apertura ni su resolución alteran el estado ni los datos del objeto vinculado.</b>
/// Si esto colgara de la misión, tocarlo tocaría la misión — y una `CERRADA` no se reabre, ni
/// por auditoría.
/// </summary>
public sealed class FilaDeHallazgo
{
    public required Ulid Id { get; init; }

    /// <summary>Del catálogo `tipo_de_hallazgo_posterior`. Tipificado, no libre.</summary>
    public required string Tipo { get; init; }

    /// <summary>
    /// Cuándo ocurrió. <b>La antigüedad se cuenta desde acá</b>, no desde el descubrimiento:
    /// contarla al revés premia descubrir tarde.
    /// </summary>
    public required DateOnly FechaDelHecho { get; init; }

    /// <summary>Cuándo se descubrió. <b>Campo distinto, y ambos obligatorios.</b></summary>
    public required DateOnly FechaDelDescubrimiento { get; init; }

    /// <summary>Cómo. Es lo que después dice qué control funcionó — y cuál faltaba.</summary>
    public required string ComoSeDescubrio { get; init; }

    /// <summary>Contra qué fuente, con el documento externo (`RN-95`).</summary>
    public required string Fuente { get; init; }

    public string? DocumentoAdjunto { get; init; }

    /// <summary>Nulo cuando el hallazgo no se ata a un vehículo concreto.</summary>
    public Ulid? VehiculoId { get; init; }

    public Ulid? MotoristaId { get; init; }

    /// <summary>El período, cuando el vínculo es sólo temporal.</summary>
    public string? Periodo { get; init; }

    /// <summary>
    /// Nula mientras el expediente está abierto. <b>El expediente no se cierra sin
    /// resolución</b> (`RN-93` punto 6), y los abiertos al cierre del ejercicio integran el
    /// saldo de apertura del siguiente (`RN-97`).
    /// </summary>
    public ResolucionDelHallazgo? Resolucion { get; set; }

    public string? Fundamento { get; set; }

    public List<FilaDeMisionDelHallazgo> Misiones { get; } = [];

    public List<FilaDeMovimientoDelHallazgo> Movimientos { get; } = [];

    public List<FilaDeReverso> Reversos { get; } = [];
}

/// <summary>
/// Una misión vinculada. <b>Cero, una o varias</b> — y cero es el caso interesante: el paso por
/// caseta de un domingo, donde <i>la ausencia de misión es el hallazgo</i>.
/// </summary>
public sealed class FilaDeMisionDelHallazgo
{
    public required Ulid HallazgoId { get; init; }

    public required Ulid MisionId { get; init; }
}

/// <summary>Un asiento del diario del expediente — `H-01` a `H-04`.</summary>
public sealed class FilaDeMovimientoDelHallazgo
{
    public required Ulid Id { get; init; }

    public required Ulid HallazgoId { get; init; }

    public required int Orden { get; init; }

    public required string Movimiento { get; init; }

    public required string Persona { get; init; }

    public required string Puesto { get; init; }

    public required DateOnly FechaDelHecho { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    public required string Motivo { get; init; }

    public Ulid? ReversoId { get; init; }
}

/// <summary>
/// Un asiento reverso — §8.3, con su contenido obligatorio completo.
///
/// <b>El original no se toca.</b> Esta fila se agrega y se refiere a él; el reporte muestra los
/// tres valores y nunca sólo el resultado.
/// </summary>
public sealed class FilaDeReverso
{
    public required Ulid Id { get; init; }

    public required Ulid HallazgoId { get; init; }

    // ── La referencia exacta ────────────────────────────────────────────────
    // §8.3: «no existe el reverso genérico "de la misión"». Sin destinatario exacto, nadie
    // puede decir si ya se revirtió ni cuántas veces.

    public required string TipoDeAsiento { get; init; }

    public required string IdentificadorDelAsiento { get; init; }

    public required string DescripcionDelAsiento { get; init; }

    public required NaturalezaDelReverso Naturaleza { get; init; }

    /// <summary>Siempre. Sin él el reporte sólo puede mostrar dos de los tres valores.</summary>
    public required string ValorAnterior { get; init; }

    /// <summary>
    /// Siempre, <b>incluso nulo</b>. Nulo significa que el dato se declara sin valor correcto
    /// conocido, y eso es distinto de no haberlo declarado.
    /// </summary>
    public string? ValorNuevo { get; init; }

    public required DateOnly FechaDelHechoOriginal { get; init; }

    public required DateTime FechaDelReversoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    public required string Persona { get; init; }

    public required string Puesto { get; init; }

    /// <summary>Quien autoriza. <b>No puede ser quien produjo el asiento</b> (`BD-06`).</summary>
    public required string Autoriza { get; init; }

    public required string AutorDelAsientoOriginal { get; init; }

    public required string MotivoTipificado { get; init; }

    public required string Fundamento { get; init; }

    public string? Adjunto { get; init; }

    /// <summary>El período del asiento revertido.</summary>
    public required string PeriodoAfectado { get; init; }

    /// <summary>
    /// El corriente. <b>Distinto del afectado cuando hay efecto económico</b>: los históricos ya
    /// publicados siguen siendo reproducibles.
    /// </summary>
    public required string PeriodoDeImputacion { get; init; }

    /// <summary>Nulo en la corrección de dato y en la anulación de documento.</summary>
    public decimal? EfectoEconomico { get; init; }

    /// <summary>Las tablas usadas para recalcular, separadas por coma. Sin ellas no se rehace.</summary>
    public string? TablasParametricas { get; init; }
}
