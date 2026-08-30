namespace Sigti.Datos.M15_Formatos;

/// <summary>
/// El salvoconducto emitido, tal como se guarda — `RN-25`, `HU-017`.
///
/// ── Guarda lo IMPRESO, no una referencia a ello ─────────────────────────────
/// Los campos del papel se congelan al emitir. Derivarlos del permiso o del expediente al
/// reimprimir produciría <b>dos impresiones del mismo folio con contenidos distintos</b>, que es
/// exactamente lo que la huella existe para hacer imposible — y el motorista tendría en la mano
/// un papel que no coincide con el que el sistema cree haber emitido.
///
/// Es la misma razón por la que <c>FilaDePermisoDeCirculacion</c> guarda sus cuatro elementos:
/// un documento firmado tiene vida propia.
/// </summary>
public sealed class FilaDeSalvoconducto
{
    public required Ulid Id { get; init; }

    /// <summary>Qué permiso materializa. Uno por permiso — `RN-04`.</summary>
    public required Ulid PermisoId { get; init; }

    /// <summary>Para poder responder «de qué misión es este papel» sin dos saltos.</summary>
    public required Ulid ExpedienteId { get; init; }

    /// <summary>
    /// El folio del documento físico.
    ///
    /// Sale del <b>rango de la delegación</b> (`RN-44`) para que una delegación sin conectividad
    /// pueda emitir e imprimir antes de salir. Si no hay rango asignado, es provisional y el
    /// documento lo dice — un folio inventado que se ve oficial es peor que uno declarado
    /// provisional.
    /// </summary>
    public required string Folio { get; init; }

    /// <summary>Nulo cuando el folio es provisional: no hay rango del que salga.</summary>
    public int? FolioNumero { get; init; }

    /// <inheritdoc cref="FolioNumero"/>
    public Ulid? FolioRangoId { get; init; }

    /// <summary>
    /// La huella del documento electrónico — `RN-25` punto 3.
    ///
    /// <b>La reimpresión no la recalcula.</b> Si cambiara, dos papeles con el mismo folio
    /// dirían huellas distintas y ninguna verificación podría decidir cuál es el bueno.
    /// </summary>
    public required string Huella { get; init; }

    /// <summary>
    /// Ocho caracteres para dictar por teléfono cuando no hay señal para escanear.
    ///
    /// Se guarda en vez de derivarse en cada consulta porque <b>es por lo que se busca</b>: un
    /// agente lo teclea y el sistema tiene que encontrar el documento por él.
    /// </summary>
    public required string CodigoCorto { get; init; }

    // ── Lo impreso, congelado ───────────────────────────────────────────────

    public required string FolioDelPermiso { get; init; }
    public required string Vehiculo { get; init; }
    public required string Motorista { get; init; }
    public required string Destino { get; init; }
    public required DateOnly Desde { get; init; }
    public required DateOnly Hasta { get; init; }

    /// <summary>Separados por ` · `, como en el permiso.</summary>
    public required string TramosInhabiles { get; init; }

    public required string Justificacion { get; init; }
    public required string FirmadoPor { get; init; }
    public required DateTime FirmadoEnUtc { get; init; }

    // ── El acto de emitir ───────────────────────────────────────────────────

    public required string EmitidoPor { get; init; }
    public required DateTime EmitidoEnUtc { get; init; }

    /// <summary>
    /// Anulado cuando el permiso se retiró o el expediente se anuló.
    ///
    /// <b>No se borra la fila</b>: un salvoconducto anulado sigue estando impreso en la mano de
    /// alguien, y el punto de verificación tiene que poder contestar por él.
    /// </summary>
    public bool Anulado { get; set; }

    /// <summary>
    /// Cada vez que el papel salió de la impresora.
    ///
    /// La primera es la emisión y no lleva motivo; las siguientes sí — `HU-017` exige registrar
    /// quién reimprimió, cuándo y por qué.
    /// </summary>
    public List<FilaDeImpresion> Impresiones { get; } = [];
}

/// <summary>
/// Una salida por impresora del mismo folio.
///
/// ── Por qué es una tabla y no un contador ───────────────────────────────────
/// Un contador dice <b>cuántas</b> y no dice ninguna de las tres cosas que importan: quién,
/// cuándo y por qué. Un salvoconducto reimpreso cinco veces sin motivo es un hallazgo de
/// auditoría; con cinco motivos registrados es una ruta con problemas.
/// </summary>
public sealed class FilaDeImpresion
{
    public required Ulid Id { get; init; }
    public required Ulid SalvoconductoId { get; init; }

    /// <summary>1 es la emisión. Es lo que hace que «la tercera impresión» signifique algo.</summary>
    public required int Orden { get; init; }

    public required string Quien { get; init; }
    public required DateTime MomentoUtc { get; init; }

    /// <summary>
    /// <b>Nulo sólo en la primera</b>, que es la emisión misma. Toda reimpresión lo exige: sin
    /// motivo, una reimpresión es indistinguible de una copia de más.
    /// </summary>
    public string? Motivo { get; init; }
}
