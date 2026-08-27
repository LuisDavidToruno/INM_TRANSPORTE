using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;
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

    private OrdenDeMision(
        Ulid id, IdPersona capturadaPor, IdPersona solicitanteDeDerecho, DatosDeLaSolicitud solicitud)
    {
        Id = id;
        CapturadaPor = capturadaPor;
        SolicitanteDeDerecho = solicitanteDeDerecho;
        Solicitud = solicitud;
    }

    /// <summary>Qué se pidió movilizar, y cuándo. Lo declara quien pide, no quien programa.</summary>
    public DatosDeLaSolicitud Solicitud { get; }

    /// <summary>
    /// Identificador ULID generado en el cliente (`ADR-005`). Nace con el expediente,
    /// en el dispositivo, para que la parada pueda referenciar a la salida antes de que
    /// exista ningún servidor de por medio.
    ///
    /// <b>No es el folio.</b> El folio es el número impreso que la institución cita en su
    /// descargo, lo asigna el servidor contra el rango de la delegación, y nunca se
    /// muestra este identificador en su lugar.
    /// </summary>
    public Ulid Id { get; }

    /// <summary>La cola de bitácora a la que pertenecen los asientos de este expediente.</summary>
    public string ColaDeBitacora => $"mision:{Id}";

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
        Ulid id,
        IdPersona capturadaPor,
        IdPersona solicitanteDeDerecho,
        DatosDeLaSolicitud solicitud,
        DateTimeOffset momento)
    {
        var expediente = new OrdenDeMision(id, capturadaPor, solicitanteDeDerecho, solicitud);

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
        Ulid id,
        IdPersona capturadaPor,
        IdPersona solicitanteDeDerecho,
        DatosDeLaSolicitud solicitud,
        IEnumerable<Transicion> diario)
    {
        var expediente = new OrdenDeMision(id, capturadaPor, solicitanteDeDerecho, solicitud);
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
    /// <param name="motivo">
    /// Sobre qué dato se autorizó. `T-05` no lo exige siempre, pero cuando la jefatura
    /// acusó una advertencia —espejo antiguo, misiones sin liquidar— <b>es la constancia
    /// que se imprime en la orden</b> (`HU-009`). Recibirlo y descartarlo dejaría a quien
    /// autoriza respondiendo por una decisión cuya justificación no existe.
    /// </param>
    public void Aprobar(IdPersona ejecuta, DateTimeOffset momento, string? motivo = null)
    {
        ExigirEstado(EstadoDeMision.Solicitada, "T-05");
        ExigirSegregacionDeAutorizacion(ejecuta);

        Registrar("T-05", EstadoDeMision.Aprobada, ejecuta, momento, motivo);
    }

    /// <summary>`T-08` — APROBADA → PROGRAMADA. Aquí se reserva vehículo y motorista (`EF-01`).</summary>
    public void Programar(
        IdPersona ejecuta,
        AsignacionDeMision asignacion,
        MatrizDeLicencias matriz,
        PoliticaDeDocumentacion politica,
        DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Aprobada, "T-08");
        ExigirAprobacionVigente(DateOnly.FromDateTime(momento.Date));
        var evidencia = ExigirHabilitacionYDocumentacion(asignacion, matriz, politica, momento);

        Registrar("T-08", EstadoDeMision.Programada, ejecuta, momento, evidencia);
    }

    /// <summary>
    /// `T-09` — APROBADA → ANULADA. Motivo obligatorio y <b>tipificado</b>.
    ///
    /// El comentario acompaña al motivo; no lo reemplaza. Sin tipificación no hay
    /// indicador de déficit de flota, que es para lo que sirve depurar esta cola.
    /// </summary>
    public void Anular(
        IdPersona ejecuta,
        MotivoDeAnulacion motivo,
        string? comentario,
        DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Aprobada, "T-09");

        var texto = string.IsNullOrWhiteSpace(comentario)
            ? motivo.ToString()
            : $"{motivo} · {comentario.Trim()}";

        Registrar("T-09", EstadoDeMision.Anulada, ejecuta, momento, texto);
    }

    /// <summary>
    /// «Si no se programa antes del <b>inicio</b> de la ventana solicitada, caduca»
    /// —efectos de `T-05`—. Programar el mismo día de salida ya es tarde.
    /// </summary>
    private void ExigirAprobacionVigente(DateOnly fechaDelHecho)
    {
        if (fechaDelHecho >= Solicitud.Ventana.Salida)
            throw new AprobacionCaducada(Solicitud.Ventana.Salida, fechaDelHecho);
    }

    /// <summary>
    /// ¿Caducó la aprobación a esta fecha? Lo usa la cola de programación para
    /// mostrarlo <b>antes</b> de que alguien lo intente, no como sorpresa al guardar.
    /// </summary>
    public bool AprobacionCaducadaAl(DateOnly fecha) =>
        Estado == EstadoDeMision.Aprobada && fecha >= Solicitud.Ventana.Salida;

    /// <summary>
    /// `T-12` — PROGRAMADA → DESPACHADA. Exige estado PROGRAMADA: §3.4 prohíbe
    /// APROBADA → DESPACHADA, porque sin programación no hay verificación de licencia,
    /// documentación ni reserva.
    ///
    /// `BD-02` y `BD-03` <b>se revalidan acá con los datos del momento</b>. Entre programar
    /// y despachar pueden pasar días, y una licencia no deja de vencerse porque ya la
    /// hayamos verificado una vez.
    /// </summary>
    public void Despachar(
        IdPersona ejecuta,
        AsignacionDeMision asignacion,
        MatrizDeLicencias matriz,
        PoliticaDeDocumentacion politica,
        DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Programada, "T-12");
        var evidencia = ExigirHabilitacionYDocumentacion(asignacion, matriz, politica, momento);

        Registrar("T-12", EstadoDeMision.Despachada, ejecuta, momento, evidencia);
    }

    /// <summary>
    /// Evalúa `BD-02` y `BD-03`, y devuelve la evidencia que va al diario.
    ///
    /// Se registra <b>con todos sus insumos</b>, no un «verificado» a secas: número de
    /// licencia, categoría, vencimiento, versión de la matriz, atributos del vehículo y
    /// fin de rango evaluado. Es lo que se muestra ante un siniestro.
    /// </summary>
    private static string ExigirHabilitacionYDocumentacion(
        AsignacionDeMision asignacion, MatrizDeLicencias matriz, PoliticaDeDocumentacion politica, DateTimeOffset conocidoAl)
    {
        var habilitacion = ReglasDeHabilitacion.Evaluar(
            asignacion.Licencia, asignacion.Vehiculo, asignacion.Ventana, matriz, conocidoAl);

        if (!habilitacion.Habilita)
            throw new BloqueoDuro("BD-02",
                $"La licencia no habilita esta misión: {habilitacion.Motivo}. " +
                $"Licencia {habilitacion.NumeroDeLicencia}, categoría {habilitacion.Categoria}, " +
                $"vence {habilitacion.VencimientoDeLicencia:yyyy-MM-dd}, " +
                $"rango evaluado hasta {habilitacion.FinDeRangoEvaluado:yyyy-MM-dd}.");

        var documentacion = ReglasDeDocumentacion.Evaluar(
            asignacion.Documentacion, asignacion.Ventana, politica);

        if (!documentacion.Habilita)
            throw new BloqueoDuro("BD-03",
                $"La documentación del vehículo no habilita esta misión: {documentacion.Motivo}, " +
                $"con rango evaluado hasta {documentacion.FinDeRangoEvaluado:yyyy-MM-dd}.");

        var advertencias = documentacion.Advertencias.Count == 0
            ? ""
            : " · advertencias: " + string.Join(", ", documentacion.Advertencias);

        return
            $"BD-02 verificada · licencia {habilitacion.NumeroDeLicencia} " +
            $"categoría {habilitacion.Categoria} vence {habilitacion.VencimientoDeLicencia:yyyy-MM-dd} · " +
            $"matriz {habilitacion.VersionDeMatriz} · " +
            $"vehículo {habilitacion.AtributosDelVehiculo.TipoDeVehiculo} " +
            $"{habilitacion.AtributosDelVehiculo.PesoBrutoKg} kg " +
            $"{habilitacion.AtributosDelVehiculo.CapacidadPasajeros} pasajeros · " +
            $"rango hasta {habilitacion.FinDeRangoEvaluado:yyyy-MM-dd} · " +
            $"BD-03 verificada{advertencias}";
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
