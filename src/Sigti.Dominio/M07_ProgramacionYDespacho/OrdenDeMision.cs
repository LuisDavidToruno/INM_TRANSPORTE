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

    /// <summary>
    /// `T-08` — APROBADA → PROGRAMADA. Aquí se reserva vehículo y motorista (`EF-01`).
    ///
    /// <b>Y ahora la reserva existe como dato.</b> Hasta que se agregó <paramref name="recursos"/>,
    /// esta transición decía que reservaba y la identidad del vehículo sólo quedaba dentro
    /// del texto de evidencia — con lo cual nadie podía preguntar qué tiene tomado el
    /// pick-up el jueves sin leer prosa. Ver <see cref="RecursosTomados"/> para por qué va
    /// en el diario y no en una tabla de reservas.
    /// </summary>
    /// <param name="recursos">
    /// Opcional para no romper a quien sólo evalúa la regla. <b>Sin esto la misión queda
    /// programada y el vehículo se sigue ofreciendo libre</b>, así que la API siempre lo manda.
    /// </param>
    /// <param name="reservas">
    /// Lo que <b>otras</b> misiones ya tienen tomado sobre este vehículo o sobre quien va a
    /// conducir — `BD-11`. Se reciben <b>sin filtrar por fecha</b>: quien llama trae las
    /// reservas del recurso y <b>el solape lo decide acá</b>, porque el solape es la regla y
    /// una regla evaluada en la consulta es una regla que no se puede probar sin base.
    /// </param>
    public void Programar(
        IdPersona ejecuta,
        AsignacionDeMision asignacion,
        MatrizDeLicencias matriz,
        PoliticaDeDocumentacion politica,
        DateTimeOffset momento,
        RecursosTomados? recursos = null,
        IReadOnlyList<ReservaDeRecurso>? reservas = null)
    {
        ExigirEstado(EstadoDeMision.Aprobada, "T-08");
        ExigirAprobacionVigente(DateOnly.FromDateTime(momento.Date));
        var evidencia = ExigirHabilitacionYDocumentacion(asignacion, matriz, politica, momento);
        var sinSolape = ExigirSinSolapamiento(reservas);

        Registrar("T-08", EstadoDeMision.Programada, ejecuta, momento,
            evidencia + sinSolape, recursos: recursos);
    }

    /// <summary>
    /// `BD-11` — <b>no hay solapamiento de reserva</b> de vehículo ni de motorista.
    ///
    /// ── Bloqueo, y sin escalón de advertencia ────────────────────────────────
    /// `EF-01` no deja margen: <i>«no sobre-asigna, <b>ni siquiera con advertencia</b>. Dos
    /// misiones con el mismo vehículo el mismo día es el error que termina con un servidor
    /// público esperando en la puerta»</i>. Por eso no admite acuse como `BD-12` ni es
    /// configurable como la póliza de `BD-03`.
    ///
    /// ── Por qué el mensaje nombra al titular ─────────────────────────────────
    /// Porque las cuatro salidas que `EF-01` ofrece —consolidar, asignar otro recurso,
    /// reprogramar, escalar— <b>empiezan todas por saber a qué dependencia llamar</b>. Un
    /// «el vehículo está ocupado» a secas convierte un bloqueo accionable en un callejón.
    ///
    /// ── El rango que se reserva ──────────────────────────────────────────────
    /// `[Salida, FinDelRango]`, el mismo que evalúa `BD-02`: hasta el último día en que el
    /// motorista podría estar conduciendo.
    ///
    /// ⚠️ <b>`EF-01` prescribe además holguras institucionales</b> —previa y posterior, por
    /// institución y por tipo de vehículo— que <b>hoy no existen</b>: son el insumo #1, `[C]`.
    /// Mientras no se decidan, la ventana reservada es <b>más angosta</b> que la que la regla
    /// final tendrá, y este bloqueo <b>deja pasar solapes que después bloqueará</b>. Es la
    /// dirección segura del error: inventar valores de holgura bloquearía misiones legítimas
    /// contra números que nadie decidió.
    /// </summary>
    /// <returns>La constancia para el diario, o vacío si no había nada que verificar.</returns>
    private string ExigirSinSolapamiento(IReadOnlyList<ReservaDeRecurso>? reservas)
    {
        // Nulo y vacío NO son lo mismo, y la diferencia importa para el diario. Vacío es
        // «se consultó y el recurso está libre»; nulo es «nadie consultó». Registrar
        // «BD-11 verificada» en el segundo caso sería dejar constancia de un control que
        // no ocurrió, que es la peor clase de asiento en un expediente auditable.
        if (reservas is null) return "";

        var choque = reservas.FirstOrDefault(r => r.SeSolapaCon(Solicitud.Ventana));

        if (choque is not null)
        {
            var recurso = (choque.Vehiculo, choque.Conductor) switch
            {
                (true, true) => "el vehículo y quien conduce están",
                (true, false) => "el vehículo está",
                _ => "quien conduce está",
            };

            throw new BloqueoDuro("BD-11",
                $"No se puede sobre-asignar: {recurso} tomado por la misión {choque.Folio} " +
                $"de {choque.Dependencia}, del {choque.Desde:yyyy-MM-dd} al {choque.Hasta:yyyy-MM-dd}. " +
                $"Esta misión ocupa del {Solicitud.Ventana.Salida:yyyy-MM-dd} " +
                $"al {Solicitud.Ventana.FinDelRango:yyyy-MM-dd}.");
        }

        return $" · BD-11 verificada contra {reservas.Count} reserva(s) del recurso";
    }

    /// <summary>
    /// `T-10` — PROGRAMADA → PROGRAMADA. <b>Cambiar el vehículo o quien conduce</b>, sin
    /// soltar la misión.
    ///
    /// ── Por qué existe teniendo `T-11` + `T-08` ───────────────────────────────
    /// Porque el rodeo pierde algo. Desprogramar y volver a programar devuelve la misión a
    /// la cola —donde cualquiera puede tomar el vehículo entre medio— y, según `EF-02`,
    /// <b>anula el folio reservado</b>. Acá <i>«el folio reservado no cambia: es el mismo
    /// expediente»</i>. El vehículo se cambia sin que la misión pase por un estado en que no
    /// tiene ninguno.
    ///
    /// ── La trazabilidad de la asignación original ────────────────────────────
    /// `DP-001 D-07` la exige: <i>«el diario muestra a quién se había asignado, por qué se
    /// cambió y a quién se asignó»</i>. Sale sola de que el diario sea de sólo agregar — la
    /// reserva anterior <b>permanece</b> —, pero el motivo hay que escribirlo, y por eso es
    /// obligatorio y tipificado.
    ///
    /// ── Por qué NO se revisa la caducidad de la aprobación ───────────────────
    /// `T-08` sí la revisa: programar el día de salida ya es tarde. Acá sería al revés. La
    /// misión <b>ya está programada</b> y legítimamente a punto de salir; si el vehículo se
    /// avería la mañana de la salida, cambiarlo es exactamente lo que hay que poder hacer.
    /// Revisar caducidad acá dejaría a la institución sin la única maniobra que le queda,
    /// justo el día en que la necesita.
    ///
    /// Lo demás sí se revalida entero —`BD-02`, `BD-03` y `BD-11`— sobre el recurso
    /// <b>entrante</b>: la ficha de `T-10` dice «todas las de `T-08` para el recurso entrante».
    /// </summary>
    /// <param name="reservas">
    /// Las de <b>otras</b> misiones. Quien llama tiene que excluir a ésta: acá la misión
    /// <b>está ocupando</b> —a diferencia de `T-08`, que sale de `APROBADA`— y sin excluirla
    /// chocaría contra su propia reserva y ningún cambio sería posible.
    /// </param>
    /// <param name="motivo">
    /// <b>Obligatorio y tipificado</b>, y por eso se recibe anulable: el motivo es el que
    /// alimenta el indicador de fiabilidad de la flota, y un `T-10` sin él sería un cambio
    /// de vehículo sin razón registrada. La exigencia vive acá y no en la API porque es
    /// regla de negocio — la misma decisión que en `T-11`.
    /// </param>
    public void Reasignar(
        IdPersona ejecuta,
        AsignacionDeMision asignacion,
        MotivoDeReasignacion? motivo,
        string? comentario,
        MatrizDeLicencias matriz,
        PoliticaDeDocumentacion politica,
        DateTimeOffset momento,
        RecursosTomados? recursos = null,
        IReadOnlyList<ReservaDeRecurso>? reservas = null)
    {
        ExigirEstado(EstadoDeMision.Programada, "T-10");

        if (motivo is not { } tipificado)
            throw new BloqueoDuro("T-10",
                "Reasignar exige motivo tipificado: es lo que distingue un vehículo que se " +
                "avería seguido de uno que se cambió por consolidación, y sin esa distinción " +
                "no hay indicador de fiabilidad de la flota.");

        var evidencia = ExigirHabilitacionYDocumentacion(asignacion, matriz, politica, momento);
        var sinSolape = ExigirSinSolapamiento(reservas);

        var texto = string.IsNullOrWhiteSpace(comentario)
            ? tipificado.ToString()
            : $"{tipificado} · {comentario.Trim()}";

        Registrar("T-10", EstadoDeMision.Programada, ejecuta, momento,
            $"{texto} — {evidencia}{sinSolape}", recursos: recursos);
    }

    /// <summary>
    /// `T-11` — PROGRAMADA → APROBADA. <b>Desprogramar liberando recursos.</b>
    ///
    /// ── Por qué no existe un «quitar el vehículo» sin esto ───────────────────
    /// `EF-01`: <i>«nunca se le quita el vehículo a una misión sin devolverla explícitamente
    /// a la cola: una misión que pierde su vehículo en silencio se descubre el día de la
    /// salida, en el predio»</i>. Ésta es la puerta por la que se libera un recurso, y la
    /// única. Es además el paso obligado de la cuarta salida de un conflicto de `BD-11` —
    /// escalar la prioridad desplaza a la primera misión <b>por acá</b>, no borrándole la
    /// reserva.
    ///
    /// ── Liberar es no volver a tomar ─────────────────────────────────────────
    /// No hay reserva que borrar: la ocupación es la proyección del diario y sólo cuenta
    /// mientras el estado la sostiene. Al volver a `APROBADA`, el vehículo queda libre por
    /// el solo hecho de que el diario siguió. La transición de `T-08` que reservó
    /// <b>permanece</b> en el diario — nada se deshace (P-3) —, simplemente deja de contar.
    ///
    /// ── La aprobación NO se pierde ───────────────────────────────────────────
    /// <i>«La solicitud vuelve a la cola de programación conservando su aprobación
    /// original»</i>. Por eso vuelve a `APROBADA` y no a `SOLICITADA`: obligar a que la
    /// jefatura vuelva a firmar por un problema de flota es castigar a quien pidió.
    ///
    /// ⚠️ <b>Dos efectos de `T-11` no ocurren todavía</b>, y no por descuido: la anulación
    /// del folio reservado (`EF-02`) necesita el circuito de rangos por delegación, que es
    /// de `M-01`; y el retorno del vehículo a `DISPONIBLE` necesita el estado operativo de
    /// `M-03`, que tampoco existe. Ninguno de los dos se finge.
    /// </summary>
    /// <param name="motivo">
    /// <b>Obligatorio.</b> Texto libre y no tipificado, a diferencia de la anulación: acá el
    /// motivo no alimenta el indicador de déficit —la misión sigue viva y se va a
    /// reprogramar—, sino que explica a la dependencia por qué perdió el vehículo que ya
    /// tenía. Es la notificación que `EF-01` exige al desplazar por prioridad.
    /// </param>
    public void Desprogramar(IdPersona ejecuta, string motivo, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Programada, "T-11");

        if (string.IsNullOrWhiteSpace(motivo))
            // Se usa `BloqueoDuro` con el identificador de la transición, como ya hace
            // `T-22` con su justificación. Un tipo de excepción nuevo para lo mismo
            // obligaría a la API a mapear dos formas de la misma negativa.
            throw new BloqueoDuro("T-11",
                "Desprogramar exige motivo: la dependencia pierde un vehículo que ya tenía " +
                "asignado y tiene derecho a saber por qué.");

        Registrar("T-11", EstadoDeMision.Aprobada, ejecuta, momento, motivo.Trim());
    }

    /// <summary>
    /// `T-13` — PROGRAMADA → ANULADA. <b>Anular una misión ya programada</b>, con motivo
    /// tipificado.
    ///
    /// ── Qué la distingue de `T-09` ───────────────────────────────────────────
    /// Que acá <b>había recursos comprometidos</b>. La misión no muere sola: libera un
    /// vehículo y un motorista que estaban tomados, y por eso la tipificación importa más,
    /// no menos — es la que dice si el recurso se liberó por déficit, por desistimiento o
    /// por causa externa.
    ///
    /// ── Y de `T-11` ──────────────────────────────────────────────────────────
    /// `T-11` devuelve la misión a la cola: sigue queriendo salir. `T-13` la mata. La
    /// tabla de transiciones lo marca <b>irreversible</b>: de `ANULADA` no se vuelve, y
    /// quien quiera el viaje presenta una solicitud nueva. Confundirlas dejaría una misión
    /// viva sin vehículo o una muerta ocupando flota.
    /// </summary>
    public void AnularProgramada(
        IdPersona ejecuta,
        MotivoDeAnulacion motivo,
        string? comentario,
        DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Programada, "T-13");

        var texto = string.IsNullOrWhiteSpace(comentario)
            ? motivo.ToString()
            : $"{motivo} · {comentario.Trim()}";

        Registrar("T-13", EstadoDeMision.Anulada, ejecuta, momento, texto);
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
    /// <param name="custodias">
    /// El historial de custodia del vehículo — `BD-13`. <b>Obligatorio, no anulable</b>: es la
    /// diferencia entre «no hay custodio» y «nadie preguntó», y en un bloqueo duro las dos
    /// no pueden verse igual. El compilador obliga a que todo llamador conteste.
    /// </param>
    public void Despachar(
        IdPersona ejecuta,
        AsignacionDeMision asignacion,
        MatrizDeLicencias matriz,
        PoliticaDeDocumentacion politica,
        DateTimeOffset momento,
        IReadOnlyList<CustodiaDelVehiculo> custodias)
    {
        ExigirEstado(EstadoDeMision.Programada, "T-12");
        var evidencia = ExigirHabilitacionYDocumentacion(asignacion, matriz, politica, momento);
        var custodia = ExigirCustodiaVigente(custodias, momento);

        Registrar("T-12", EstadoDeMision.Despachada, ejecuta, momento, evidencia + custodia);
    }

    /// <summary>
    /// Evalúa `BD-02` y `BD-03`, y devuelve la evidencia que va al diario.
    ///
    /// Se registra <b>con todos sus insumos</b>, no un «verificado» a secas: número de
    /// licencia, categoría, vencimiento, versión de la matriz, atributos del vehículo y
    /// fin de rango evaluado. Es lo que se muestra ante un siniestro.
    /// </summary>
    private string ExigirHabilitacionYDocumentacion(
        AsignacionDeMision asignacion, MatrizDeLicencias matriz, PoliticaDeDocumentacion politica, DateTimeOffset conocidoAl)
    {
        // La ventana es la de la solicitud. No hay forma de pasar otra.
        var ventana = Solicitud.Ventana;

        var habilitacion = ReglasDeHabilitacion.Evaluar(
            asignacion.Licencia, asignacion.Vehiculo, ventana, matriz, conocidoAl);

        if (!habilitacion.Habilita)
            throw new BloqueoDuro("BD-02",
                $"La licencia no habilita esta misión: {habilitacion.Motivo}. " +
                $"Licencia {habilitacion.NumeroDeLicencia}, categoría {habilitacion.Categoria}, " +
                $"vence {habilitacion.VencimientoDeLicencia:yyyy-MM-dd}, " +
                $"rango evaluado hasta {habilitacion.FinDeRangoEvaluado:yyyy-MM-dd}.");

        var documentacion = ReglasDeDocumentacion.Evaluar(
            asignacion.Documentacion, ventana, politica);

        if (!documentacion.Habilita)
            throw new BloqueoDuro("BD-03",
                $"La documentación del vehículo no habilita esta misión: {documentacion.Motivo}" +
                // La fecha decide si esperar sirve o hay que cambiar de vehículo. Sin
                // ella, quien programa tiene que ir a buscarla al expediente.
                (documentacion.VenceElQueBloquea is { } vence ? $", vence {vence:yyyy-MM-dd}" : "") +
                $", con rango evaluado hasta {documentacion.FinDeRangoEvaluado:yyyy-MM-dd}.");

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

    /// <summary>
    /// `BD-13` — <b>un vehículo sin custodio vigente no se despacha.</b>
    ///
    /// ── Por qué es bloqueo y no advertencia ──────────────────────────────────
    /// La máquina de estados lo dice sin margen: <i>«trasladar una custodia que no existe no
    /// es posible: si nadie responde hoy por el bien, tampoco hay de quién recibirlo ni a
    /// quién devolverlo, y el acta de entrega queda sin una de sus dos firmas»</i>. No es una
    /// formalidad administrativa — es que la operación que `T-12` describe <b>no se puede
    /// ejecutar</b>: no hay contraparte.
    ///
    /// `RN-22` lo declara <b>no configurable</b>, y `RN-22` misma admite que es incómodo:
    /// <i>«vehículo asignado a una delegación sin custodio designado: bloqueo del despacho. Es
    /// incómodo y es correcto — un vehículo del Estado sin responsable identificado es un
    /// hallazgo esperando ocurrir»</i>.
    ///
    /// ── A qué fecha se evalúa ────────────────────────────────────────────────
    /// A la del <b>hecho</b>, no a la de captura (P-4). Un despacho que se sincroniza tres días
    /// después se juzga con el custodio que había el día en que el vehículo salió, no con el
    /// de hoy: de lo contrario una rotación posterior invalidaría un despacho que fue
    /// correcto cuando ocurrió.
    /// </summary>
    /// <returns>La constancia para el diario: quién respondía por el bien al salir.</returns>
    private static string ExigirCustodiaVigente(
        IReadOnlyList<CustodiaDelVehiculo> custodias,
        DateTimeOffset momento)
    {
        var fecha = DateOnly.FromDateTime(momento.Date);
        var vigente = custodias.FirstOrDefault(c => c.VigenteAl(fecha));

        if (vigente is null)
            throw new BloqueoDuro("BD-13",
                $"El vehículo no tiene custodio vigente al {fecha:yyyy-MM-dd}: no hay de quién " +
                "recibirlo ni a quién devolverlo, y el acta de entrega quedaría sin una de sus " +
                "dos firmas. Registre la tarjeta de responsabilidad antes de despachar." +
                // Decir cuántas hubo distingue «nunca tuvo custodio» de «la custodia cesó»,
                // y son dos problemas con dos arreglos distintos.
                (custodias.Count == 0
                    ? " Este vehículo no tiene ninguna custodia registrada."
                    : $" Tiene {custodias.Count} custodia(s) registrada(s), ninguna vigente a esa fecha."));

        return $" · BD-13 verificada · custodio {vigente.Custodio.Valor} desde {vigente.Desde:yyyy-MM-dd}";
    }

    /// <summary>`T-14` — DESPACHADA → EN_RUTA. La ejecuta el motorista, y opera desconectado.</summary>
    public void IniciarRuta(IdPersona ejecuta, DateTimeOffset momento, Ulid? idDeCaptura = null)
    {
        ExigirEstado(EstadoDeMision.Despachada, "T-14");
        Registrar("T-14", EstadoDeMision.EnRuta, ejecuta, momento, motivo: null, idDeCaptura);
    }

    /// <summary>
    /// `T-18` — EN_RUTA → RETORNADA. Registra un hecho consumado: por `P-2` no se bloquea,
    /// se validan coherencias que pueden derivar en cierre con hallazgo.
    /// </summary>
    public void Retornar(IdPersona ejecuta, DateTimeOffset momento, Ulid? idDeCaptura = null)
    {
        ExigirEstado(EstadoDeMision.EnRuta, "T-18");
        Registrar("T-18", EstadoDeMision.Retornada, ejecuta, momento, motivo: null, idDeCaptura);
    }

    /// <summary>`T-19` — RETORNADA → LIQUIDADA.</summary>
    public void Liquidar(IdPersona ejecuta, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Retornada, "T-19");
        Registrar("T-19", EstadoDeMision.Liquidada, ejecuta, momento, motivo: null);
    }

    public void DevolverLiquidacion(IdPersona ejecuta, DateTimeOffset momento, string motivo)
    {
        ExigirEstado(EstadoDeMision.Liquidada, "T-20");
        Registrar("T-20", EstadoDeMision.Retornada, ejecuta, momento, motivo);
    }

    public void Cerrar(
        IdPersona ejecuta,
        DateTimeOffset momento,
        IReadOnlyList<HallazgoDetectado> criterios,
        string? justificacion)
    {
        ExigirEstado(EstadoDeMision.Liquidada, "T-21");
        ExigirSegregacionDeCierre(ejecuta);

        if (criterios.Count > 0)
        {
            if (string.IsNullOrWhiteSpace(justificacion))
                throw new BloqueoDuro("T-22",
                    "Un cierre con hallazgo exige justificación: el criterio lo decide el sistema, " +
                    "pero qué se hizo con él lo declara quien cierra. Criterios detectados: " +
                    string.Join(", ", criterios.Select(c => c.Criterio)) + ".");

            var detectados = string.Join(" · ", criterios.Select(c => $"{c.Criterio}: {c.Detalle}"));
            Registrar("T-22", EstadoDeMision.CerradaConHallazgo, ejecuta, momento,
                motivo: $"{detectados} — justificación: {justificacion}");
            return;
        }

        Registrar("T-21", EstadoDeMision.Cerrada, ejecuta, momento, motivo: null);
    }

    /// <summary>
    /// `BD-06` en `T-21` y `T-22` — <b>quien cierra ≠ quien liquidó</b>.
    ///
    /// Es el último par de la cadena de segregación, y el más fácil de saltarse en una
    /// delegación pequeña: la misma persona elaboró el descargo conciliado y tiene a mano
    /// el botón de cerrar. Si cierra su propia liquidación, nadie la revisó.
    ///
    /// Se deriva del diario y no de un campo — `P-1` vale también para los datos que las
    /// precondiciones necesitan.
    /// </summary>
    private void ExigirSegregacionDeCierre(IdPersona ejecuta)
    {
        var liquidoPor = _diario.LastOrDefault(t => t.Id == "T-19")?.Ejecuta;

        if (liquidoPor is not null && ejecuta == liquidoPor)
            throw new BloqueoDuro("BD-06",
                "Quien elaboró la liquidación no puede cerrar la misión. " +
                "Cerrar es el acto que verifica el descargo, y verificar el propio trabajo no es verificar.");
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
        string id, EstadoDeMision destino, IdPersona ejecuta, DateTimeOffset momento, string? motivo,
        Ulid? idDeCaptura = null, RecursosTomados? recursos = null) =>
        _diario.Add(new Transicion(id, destino, ejecuta, momento, motivo, idDeCaptura, recursos));
}
