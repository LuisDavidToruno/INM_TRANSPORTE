namespace Sigti.Datos.M14_Auditoria;

/// <summary>
/// El acta de cierre de ejercicio, tal como se guarda — `RN-96` punto 1.
///
/// ── Lo que NO se guarda acá, a propósito ────────────────────────────────────
/// El inventario de expedientes no terminales. <b>Vive en el saldo de apertura</b> (`RN-97`),
/// que se congela con su propio folio, y el acta lo cita. Duplicarlo dejaría dos inventarios
/// del mismo corte que se pueden separar — que es exactamente lo que `RN-97` impide al admitir
/// un solo saldo por ejercicio.
///
/// Lo que sí se congela acá es lo que <b>sólo el acta produce</b>: los folios listados para
/// anular, y las diferencias contra el saldo al momento de producirla.
/// </summary>
public sealed class FilaDeActaDeCierre
{
    public required Ulid Id { get; init; }

    /// <summary>Sin folio el saldo de apertura no tiene a qué acta corresponder.</summary>
    public required string Folio { get; init; }

    /// <summary>Uno por ejercicio.</summary>
    public required string Ejercicio { get; init; }

    /// <summary>La fecha que fija la norma contable.</summary>
    public required DateOnly CorteLegal { get; init; }

    /// <summary>Hasta cuándo la operación siguió registrando hechos del ejercicio.</summary>
    public required DateOnly CorteOperativo { get; init; }

    public required string Persona { get; init; }

    public required string Puesto { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    /// <summary>
    /// El folio del saldo de apertura que el acta cita. <b>Nulo cuando el saldo todavía no se
    /// produjo</b>, y entonces el acta lo dice en vez de fingir que cuadró contra algo.
    /// </summary>
    public string? SaldoDeAperturaFolio { get; init; }

    /// <summary>
    /// Las diferencias contra el saldo <b>al momento de producir el acta</b>. Se congelan porque
    /// el inventario sigue vivo: recalcularlas meses después no diría qué se vio ese día.
    /// </summary>
    public required string DiferenciasConElSaldo { get; init; }

    /// <summary>Lo que el acta encontró y alguien tiene que mirar.</summary>
    public required string Observaciones { get; init; }

    public List<FilaDeFolioDelActa> Folios { get; } = [];
}

/// <summary>
/// Un folio reservado y no consumido, listado por el acta — `RN-96` punto 5.
///
/// ── Listar y anular son dos actos, y no por burocracia ──────────────────────
/// El acta <b>lista</b>; anular es un acto posterior con autor y motivo que la cita. Un
/// documento que anulara decenas de folios al producirse sería un cierre masivo por fecha con
/// otro nombre — el mismo que `RN-96` existe para impedir, un nivel más abajo.
/// </summary>
public sealed class FilaDeFolioDelActa
{
    public required Ulid Id { get; init; }

    public required Ulid ActaId { get; init; }

    /// <summary>El vale al que corresponde.</summary>
    public required Ulid AsignacionId { get; init; }

    public required string Folio { get; init; }

    /// <summary>El ámbito del fondo del que salió. `RN-96` anula <b>por rango y delegación</b>.</summary>
    public required string Delegacion { get; init; }

    public required decimal Monto { get; init; }

    public required DateOnly Emitido { get; init; }

    /// <summary>En qué estado estaba al corte.</summary>
    public required string Estado { get; init; }

    /// <summary>
    /// <b>Falso para el vale entregado</b>: `V-03` sólo corre sobre uno emitido. Va en la lista
    /// igual —es dinero fuera de la caja al cierre— pero marcado, para que el acta no lo esconda
    /// entre los que sí se anulan.
    /// </summary>
    public required bool SePuedeAnular { get; init; }

    /// <summary>Cuándo se ejecutó la anulación. Nulo mientras siga solamente listado.</summary>
    public DateTime? AnuladoUtc { get; set; }

    public string? AnuladoPor { get; set; }
}
