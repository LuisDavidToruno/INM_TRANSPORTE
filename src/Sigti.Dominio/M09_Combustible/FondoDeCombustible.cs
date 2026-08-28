using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// El fondo de combustible del período — `RN-26`.
///
/// <b>P-1.</b> El estado y el saldo son proyecciones del diario. El saldo <b>no es una columna
/// que se decrementa</b>: es una resta sobre asientos, y esa es toda la diferencia entre un
/// número que se puede auditar y un número que alguien pudo haber editado.
///
/// ── Lo que este objeto NO hace ──────────────────────────────────────────────
/// <b>No compra combustible ni gestiona contratos</b> (`DP-001 D-03`). Modela un fondo
/// asignado y su consumo. El punto de fuga que `PROP-01` nombra es <i>«el efectivo sin
/// trazabilidad»</i>, y contra eso sirve el saldo, no el contrato.
/// </summary>
public sealed class FondoDeCombustible
{
    private readonly List<MovimientoDelFondo> _diario = [];

    private FondoDeCombustible(
        Ulid id, AmbitoDelFondo ambito, string ambitoDeclarado,
        DateOnly desde, DateOnly hasta, IdPersona solicita)
    {
        Id = id;
        Ambito = ambito;
        AmbitoDeclarado = ambitoDeclarado;
        Desde = desde;
        Hasta = hasta;
        Solicita = solicita;
    }

    public Ulid Id { get; }

    public AmbitoDelFondo Ambito { get; }

    /// <summary>La dependencia o delegación concreta, cuando el ámbito no es la institución.</summary>
    public string AmbitoDeclarado { get; }

    /// <summary>
    /// El período que cubre. <b>Es lo que hace del fondo un objeto de período y no de misión</b>
    /// — la distinción que sostiene el hallazgo `HN1-15`.
    /// </summary>
    public DateOnly Desde { get; }

    public DateOnly Hasta { get; }

    /// <summary>Quién lo pidió. Se conserva porque es la mitad de la segregación de `RN-26.4`.</summary>
    public IdPersona Solicita { get; }

    /// <summary>Quién lo aprobó. Nulo mientras nadie lo haya aprobado.</summary>
    public IdPersona? Aprueba { get; private set; }

    /// <summary>
    /// La partida contra la que se afecta. <b>Nula es «pendiente», no «sin partida»</b>: la
    /// define ARGOS y `RN-26` manda registrar el fondo igual y bloquear su cierre.
    /// </summary>
    public string? PartidaPresupuestaria { get; private set; }

    public IReadOnlyList<MovimientoDelFondo> Diario => _diario;

    public EstadoDelFondo Estado => _diario[^1].Destino;

    /// <summary>
    /// El techo: lo aprobado más las ampliaciones. <b>Suma de asientos, nunca un campo.</b>
    /// </summary>
    public decimal Aprobado => _diario.Sum(m => m.Monto ?? 0m);

    /// <summary>Está vigente a la fecha del hecho — P-4, no a la fecha de captura.</summary>
    public bool VigenteAl(DateOnly fecha) => fecha >= Desde && fecha <= Hasta;

    /// <summary>
    /// `RN-26` — <c>aprobado menos asignado más devoluciones liquidadas</c>.
    ///
    /// ── Por qué las devoluciones se reciben ya filtradas ────────────────────
    /// Porque `RN-26` distingue una devolución <b>constatada</b> de una declarada: <i>«una
    /// devolución declarada pero no constatada no libera saldo»</i>. Quien llama decide cuáles
    /// cuentan; el fondo no puede saberlo solo, y suponer que toda devolución cuenta liberaría
    /// saldo contra dinero que nadie vio volver.
    /// </summary>
    public decimal SaldoDisponible(decimal asignado, decimal devolucionesConstatadas) =>
        Aprobado - asignado + devolucionesConstatadas;

    /// <summary>
    /// Rehidrata el fondo <b>desde su diario</b>. No hay estado ni saldo que leer de una
    /// columna: los dos son proyecciones (P-1).
    /// </summary>
    public static FondoDeCombustible Reconstruir(
        Ulid id,
        AmbitoDelFondo ambito,
        string ambitoDeclarado,
        DateOnly desde,
        DateOnly hasta,
        IdPersona solicita,
        IdPersona? aprueba,
        string? partida,
        IEnumerable<MovimientoDelFondo> diario)
    {
        var fondo = new FondoDeCombustible(id, ambito, ambitoDeclarado, desde, hasta, solicita)
        {
            Aprueba = aprueba,
            PartidaPresupuestaria = partida,
        };

        fondo._diario.AddRange(diario);

        if (fondo._diario.Count == 0)
            throw new ArgumentException(
                "Un fondo sin diario no tiene estado que proyectar.", nameof(diario));

        return fondo;
    }

    /// <summary>`F-01` — ACT-04 solicita el fondo del período.</summary>
    public static FondoDeCombustible Solicitar(
        Ulid id,
        AmbitoDelFondo ambito,
        string ambitoDeclarado,
        DateOnly desde,
        DateOnly hasta,
        IdPersona solicita,
        decimal montoSolicitado,
        string justificacion,
        DateTimeOffset momento)
    {
        if (hasta < desde)
            throw new BloqueoDuro("RN-26",
                "El período del fondo termina antes de empezar.");

        if (string.IsNullOrWhiteSpace(justificacion))
            throw new BloqueoDuro("RN-26",
                "La solicitud del fondo exige justificación operativa del período. Un monto sin " +
                "sustento es lo que después no se puede defender ante el Tribunal.");

        var fondo = new FondoDeCombustible(id, ambito, ambitoDeclarado, desde, hasta, solicita);

        // Solicitar NO crea saldo: el monto pedido va en el motivo, no en `Monto`. Lo que se
        // pide y lo que se aprueba son dos cifras distintas, y confundirlas haría que el techo
        // del fondo lo fijara quien lo solicita.
        fondo._diario.Add(new MovimientoDelFondo(
            "F-01", EstadoDelFondo.Solicitado, solicita, momento,
            $"Solicita {montoSolicitado:N2}. {justificacion}"));

        return fondo;
    }

    /// <summary>
    /// `F-02` — ACT-08 Gerencia Administrativa aprueba, con monto, partida y aprobador.
    ///
    /// ⚠️ <b>La cuota trimestral de compromiso no se verifica acá.</b> `RN-26` la exige
    /// (`RN-54`: <i>tener saldo en la partida anual no significa que el compromiso quepa en el
    /// trimestre</i>), y necesita el espejo presupuestario de ARGOS, que no existe. <b>No se
    /// finge</b>: queda dicho en el asiento, en vez de aprobar en silencio contra un trimestre
    /// que nadie consultó.
    /// </summary>
    public void Aprobar(
        IdPersona aprueba, decimal montoAprobado, string? partida, DateTimeOffset momento)
    {
        ExigirEstado("F-02", EstadoDelFondo.Solicitado);
        ReglasDelFondo.ExigirQueQuienApruebaNoSeaQuienSolicito(Solicita, aprueba);

        if (montoAprobado <= 0)
            throw new BloqueoDuro("RN-26",
                "Un fondo aprobado en cero no es un fondo: o se aprueba un monto, o se rechaza " +
                "la solicitud.");

        Aprueba = aprueba;
        PartidaPresupuestaria = partida;

        _diario.Add(new MovimientoDelFondo(
            "F-02", EstadoDelFondo.Aprobado, aprueba, momento,
            $"Aprobado {montoAprobado:N2}" +
            (string.IsNullOrWhiteSpace(partida)
                ? ". Partida PENDIENTE: el cierre queda bloqueado hasta completarla (`RN-26`)."
                : $" contra la partida {partida}.") +
            " Cuota trimestral (`RN-54`) NO verificada: no hay espejo presupuestario.",
            Monto: montoAprobado));
    }

    /// <summary>`F-03` — el efectivo o las órdenes de pago quedan en manos de quien las administra.</summary>
    public void RegistrarEntrega(IdPersona ejecuta, string instrumento, DateTimeOffset momento)
    {
        ExigirEstado("F-03", EstadoDelFondo.Aprobado);

        _diario.Add(new MovimientoDelFondo(
            "F-03", EstadoDelFondo.Entregado, ejecuta, momento,
            $"Recibido en {instrumento}."));
    }

    /// <summary>
    /// `F-05` — ampliación. <b>Sigue el mismo circuito</b>: `RN-26` no admite una vía corta
    /// para el fondo agotado, porque <i>el control se perdería exactamente cuando más presión
    /// hay</i>.
    /// </summary>
    public void Ampliar(
        IdPersona aprueba, decimal montoAdicional, string motivo, DateTimeOffset momento)
    {
        if (Estado is EstadoDelFondo.Cerrado)
            throw new TransicionInvalidaDelFondo("F-05", Estado, EstadoDelFondo.Agotado);

        ReglasDelFondo.ExigirQueQuienApruebaNoSeaQuienSolicito(Solicita, aprueba);

        if (montoAdicional <= 0)
            throw new BloqueoDuro("RN-26", "Una ampliación en cero o negativa no amplía nada.");

        _diario.Add(new MovimientoDelFondo(
            "F-05", EstadoDelFondo.Aprobado, aprueba, momento,
            $"Ampliación de {montoAdicional:N2}. {motivo}", Monto: montoAdicional));
    }

    /// <summary>`F-04` — sin saldo. No es terminal: la ampliación lo devuelve a `Aprobado`.</summary>
    public void MarcarAgotado(IdPersona ejecuta, DateTimeOffset momento)
    {
        if (Estado is EstadoDelFondo.Cerrado)
            throw new TransicionInvalidaDelFondo("F-04", Estado, EstadoDelFondo.Aprobado);

        _diario.Add(new MovimientoDelFondo(
            "F-04", EstadoDelFondo.Agotado, ejecuta, momento, "Saldo agotado."));
    }

    /// <summary>
    /// `F-06` — cierre del período.
    ///
    /// Exige las dos cosas de `RN-26`: todas las asignaciones liquidadas o anuladas, y partida
    /// completa. Y la segregación de la tercera función: <b>quien cierra no puede ser quien
    /// pidió ni quien aprobó</b>.
    /// </summary>
    public void Cerrar(
        IdPersona liquida, int asignacionesSinLiquidar, string? partida, DateTimeOffset momento)
    {
        if (Estado is EstadoDelFondo.Cerrado)
            throw new TransicionInvalidaDelFondo("F-06", Estado, EstadoDelFondo.Aprobado);

        ReglasDelFondo.ExigirQueQuienLiquidaNoSeaNingunoDeLosDos(
            Solicita, Aprueba ?? Solicita, liquida);

        // Completar la partida al cerrar es el caso previsto: `RN-26` manda registrar el fondo
        // con partida pendiente, no impedir que exista.
        var partidaFinal = string.IsNullOrWhiteSpace(partida) ? PartidaPresupuestaria : partida;
        ReglasDelFondo.ExigirCierrable(asignacionesSinLiquidar, partidaFinal);
        PartidaPresupuestaria = partidaFinal;

        _diario.Add(new MovimientoDelFondo(
            "F-06", EstadoDelFondo.Cerrado, liquida, momento,
            $"Período cerrado contra la partida {partidaFinal}."));
    }

    private void ExigirEstado(string movimiento, EstadoDelFondo requerido)
    {
        if (Estado != requerido)
            throw new TransicionInvalidaDelFondo(movimiento, Estado, requerido);
    }
}

/// <summary>Se intentó un movimiento del fondo desde un estado que no lo admite.</summary>
public sealed class TransicionInvalidaDelFondo(
    string movimiento, EstadoDelFondo estadoActual, EstadoDelFondo estadoRequerido)
    : Exception($"El movimiento {movimiento} exige el fondo en {estadoRequerido}, y está en {estadoActual}.")
{
    public string Movimiento { get; } = movimiento;
    public EstadoDelFondo EstadoActual { get; } = estadoActual;
    public EstadoDelFondo EstadoRequerido { get; } = estadoRequerido;
}
