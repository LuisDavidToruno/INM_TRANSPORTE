using Sigti.Dominio.M09_Combustible;

namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// El fondo del período, tal como se guarda.
///
/// ── Lo que NO tiene, y es a propósito ───────────────────────────────────────
/// <b>No hay columna <c>saldo</c>.</b> El saldo es <c>aprobado − asignado + devoluciones
/// constatadas</c>, y las tres cifras salen de asientos. Una columna de saldo es un número
/// que alguien pudo haber editado, y toda la razón de ser de `RN-26` es que ese número se
/// pueda auditar.
/// </summary>
public sealed class FilaDeFondo
{
    public required Ulid Id { get; init; }

    public required AmbitoDelFondo Ambito { get; init; }

    /// <summary>La dependencia o delegación concreta. «Instituto» cuando el ámbito es central.</summary>
    public required string AmbitoDeclarado { get; init; }

    public required DateOnly Desde { get; init; }

    public required DateOnly Hasta { get; init; }

    public required string Solicita { get; init; }

    public string? Aprueba { get; set; }

    /// <summary>
    /// Nula es <b>pendiente</b>, no «sin partida». La define ARGOS (`DP-001 D-09`) y `RN-26`
    /// manda registrar el fondo igual, bloqueando su cierre.
    /// </summary>
    public string? PartidaPresupuestaria { get; set; }

    public List<FilaDeMovimientoDelFondo> Movimientos { get; } = [];
}

/// <summary>Un asiento del diario del fondo — `F-01` a `F-06`.</summary>
public sealed class FilaDeMovimientoDelFondo
{
    public required Ulid Id { get; init; }

    public required Ulid FondoId { get; init; }

    /// <summary>
    /// La posición en el diario. Es lo que hace reconstruible el estado: dos asientos pueden
    /// compartir marca de tiempo, y entonces el orden es lo único que los separa.
    /// </summary>
    public required int Orden { get; init; }

    public required string Movimiento { get; init; }

    public required EstadoDelFondo Destino { get; init; }

    public required string Ejecuta { get; init; }

    public required DateTime MomentoUtc { get; init; }

    /// <summary>
    /// El huso en que ocurrió el hecho. Honduras es UTC−6 y no cambia, pero guardarlo aparte
    /// es lo que permite que el dato siga siendo cierto si mañana cambia — o si el asiento
    /// llega de un dispositivo mal configurado, que es el caso real.
    /// </summary>
    public required int DesfaseMinutos { get; init; }

    public string? Motivo { get; init; }

    /// <summary>
    /// Lo que este asiento aprueba. <b>Sólo `F-02` y `F-05`.</b> El techo del fondo es la suma
    /// de esta columna, y por eso no hay otra que la contradiga.
    /// </summary>
    public decimal? Monto { get; init; }
}
