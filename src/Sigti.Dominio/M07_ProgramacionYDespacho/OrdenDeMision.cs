using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// El expediente de misión.
///
/// <b>P-1 — El estado es el resultado del diario, no un campo.</b> Cualquier valor de
/// estado que el sistema guarde es una proyección y debe poder reconstruirse desde el
/// diario de transiciones. Sin esto la sincronización desconectada no tiene solución:
/// dos dispositivos no negocian «el estado», intercambian <b>transiciones</b>.
/// </summary>
public sealed class OrdenDeMision
{
    private readonly List<Transicion> _diario = [];

    private OrdenDeMision(IdPersona capturadaPor, IdPersona solicitanteDeDerecho)
    {
        CapturadaPor = capturadaPor;
        SolicitanteDeDerecho = solicitanteDeDerecho;
    }

    /// <summary>Quién digitó la solicitud. Puede no ser el solicitante — ver `BD-01`.</summary>
    public IdPersona CapturadaPor { get; }

    /// <summary>
    /// La persona a cuyo nombre se solicita la movilización. Se <b>declara</b>, no se
    /// infiere del usuario autenticado: sin ese dato el bloqueo de `BD-01` vuelve a ser ciego.
    /// </summary>
    public IdPersona SolicitanteDeDerecho { get; }

    public IReadOnlyList<Transicion> Diario => _diario;

    /// <summary>Proyección del diario. Nunca un campo almacenado que se pueda desincronizar.</summary>
    public EstadoDeMision Estado => _diario[^1].Destino;

    /// <summary>`T-01` — Creación del expediente en borrador.</summary>
    public static OrdenDeMision Crear(
        IdPersona capturadaPor,
        IdPersona solicitanteDeDerecho,
        DateTimeOffset momento)
    {
        var expediente = new OrdenDeMision(capturadaPor, solicitanteDeDerecho);

        expediente._diario.Add(new Transicion(
            Id: "T-01",
            Destino: EstadoDeMision.Borrador,
            Ejecuta: capturadaPor,
            Momento: momento,
            Motivo: null));

        return expediente;
    }

    /// <summary>
    /// Rehidrata el expediente desde su diario. Es lo que usa la persistencia al leer y
    /// lo que usará la sincronización al recibir transiciones de un dispositivo: <b>el
    /// estado no viaja, viajan las transiciones</b> (P-1).
    /// </summary>
    public static OrdenDeMision Reconstruir(
        IdPersona capturadaPor,
        IdPersona solicitanteDeDerecho,
        IEnumerable<Transicion> diario)
    {
        var expediente = new OrdenDeMision(capturadaPor, solicitanteDeDerecho);
        expediente._diario.AddRange(diario);

        if (expediente._diario.Count == 0)
            throw new ArgumentException(
                "Un expediente sin diario no tiene estado que proyectar.", nameof(diario));

        return expediente;
    }

    /// <summary>`T-02` — BORRADOR → SOLICITADA. Sin motivo obligatorio.</summary>
    public void Enviar(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Borrador, "T-02");

        Registrar("T-02", EstadoDeMision.Solicitada, ejecuta, momento, motivo: null);
    }

    /// <summary>
    /// `T-05` — SOLICITADA → APROBADA. Evalúa `BD-01`.
    /// </summary>
    public void Aprobar(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Solicitada, "T-05");
        ExigirSegregacionDeAutorizacion(ejecuta);

        Registrar("T-05", EstadoDeMision.Aprobada, ejecuta, momento, motivo: null);
    }

    /// <summary>`T-08` — APROBADA → PROGRAMADA. Aquí se reserva vehículo y motorista (`EF-01`).</summary>
    public void Programar(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Aprobada, "T-08");
        Registrar("T-08", EstadoDeMision.Programada, ejecuta, momento, motivo: null);
    }

    /// <summary>
    /// `T-12` — PROGRAMADA → DESPACHADA. Exige estado PROGRAMADA: §3.4 prohíbe
    /// APROBADA → DESPACHADA, porque sin programación no hay verificación de licencia,
    /// documentación ni reserva.
    /// </summary>
    public void Despachar(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Programada, "T-12");
        Registrar("T-12", EstadoDeMision.Despachada, ejecuta, momento, motivo: null);
    }

    /// <summary>`T-14` — DESPACHADA → EN_RUTA. La ejecuta el motorista, y opera desconectado.</summary>
    public void IniciarRuta(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Despachada, "T-14");
        Registrar("T-14", EstadoDeMision.EnRuta, ejecuta, momento, motivo: null);
    }

    /// <summary>
    /// `T-18` — EN_RUTA → RETORNADA. Registra un hecho consumado: por `P-2` no se bloquea,
    /// se validan coherencias que pueden derivar en cierre con hallazgo.
    /// </summary>
    public void Retornar(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.EnRuta, "T-18");
        Registrar("T-18", EstadoDeMision.Retornada, ejecuta, momento, motivo: null);
    }

    /// <summary>`T-19` — RETORNADA → LIQUIDADA.</summary>
    public void Liquidar(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Retornada, "T-19");
        Registrar("T-19", EstadoDeMision.Liquidada, ejecuta, momento, motivo: null);
    }

    /// <summary>
    /// `BD-01` — Segregación entre solicitante y autorizador.
    ///
    /// Quien autoriza no puede ser ninguna de las tres, si fueran distintas entre sí:
    /// quien creó la solicitud, quien la envió, o el solicitante de derecho.
    /// </summary>
    private void ExigirSegregacionDeAutorizacion(IdPersona ejecuta)
    {
        if (ejecuta == CapturadaPor)
            throw new BloqueoDuro("BD-01", "Quien capturó la solicitud no puede autorizarla.");

        // Se deriva del diario, no de un campo: P-1 vale también para los datos que las
        // precondiciones necesitan, o el estado y el diario se desincronizan.
        var enviadaPor = _diario.FirstOrDefault(t => t.Id == "T-02")?.Ejecuta;

        if (enviadaPor is { } remitente && ejecuta == remitente)
            throw new BloqueoDuro("BD-01", "Quien envió la solicitud no puede autorizarla.");

        if (ejecuta == SolicitanteDeDerecho)
            throw new BloqueoDuro("BD-01",
                "El solicitante de derecho no puede autorizar su propia solicitud, " +
                "aunque no la haya capturado ni enviado.");
    }

    private void ExigirEstado(EstadoDeMision esperado, string transicion)
    {
        if (Estado != esperado)
            throw new TransicionInvalida(transicion, Estado, esperado);
    }

    private void Registrar(
        string id, EstadoDeMision destino, IdPersona ejecuta, DateTimeOffset momento, string? motivo) =>
        _diario.Add(new Transicion(id, destino, ejecuta, momento, motivo));
}
