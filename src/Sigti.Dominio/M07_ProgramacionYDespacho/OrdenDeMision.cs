using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M08_Bitacora;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.M11_Mantenimiento;
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
        IReadOnlyList<ReservaDeRecurso>? reservas = null,
        EstadoOperativo? estadoDelVehiculo = null)
    {
        ExigirEstado(EstadoDeMision.Aprobada, "T-08");
        ExigirAprobacionVigente(DateOnly.FromDateTime(momento.Date));
        var operativo = ExigirVehiculoDisponible(estadoDelVehiculo);
        var evidencia = ExigirHabilitacionYDocumentacion(asignacion, matriz, politica, momento);
        var sinSolape = ExigirSinSolapamiento(reservas);

        Registrar("T-08", EstadoDeMision.Programada, ejecuta, momento,
            evidencia + sinSolape + operativo, recursos: recursos);
    }

    /// <summary>
    /// `BD-07` — <b>el vehículo tiene que estar `DISPONIBLE`.</b>
    ///
    /// ── Nulo NO es disponible ───────────────────────────────────────────────
    /// Un vehículo al que nadie le declaró estado no está disponible: §10.2 lista <i>«alta
    /// reciente sin habilitar»</i> entre las causas de `NO_DISPONIBLE`. Tratar el nulo como
    /// disponible haría que <b>el alta habilitara sola</b>, que es lo contrario de lo que la
    /// sección dice.
    ///
    /// Se recibe anulable —y no se exige— porque hay expedientes y pruebas anteriores al
    /// estado operativo. <b>Cuando llega nulo se dice en el diario</b>, en vez de dejar creer
    /// que se verificó.
    ///
    /// ── `ASIGNADO` y `EN_MISION` NO bloquean, y §10.2 dice lo contrario ─────
    /// §10.2 dice que `DISPONIBLE` <i>«es el único estado desde el que se puede programar»</i>.
    /// <b>Leído al pie de la letra, eso rompe la operación normal</b>: un vehículo comprometido
    /// a una misión de diciembre queda `ASIGNADO` desde hoy, y bloquearía programar una
    /// misión de marzo con la que no se solapa en nada.
    ///
    /// El sistema entero está construido sobre lo contrario: `EF-01` reserva <b>por ventana</b>,
    /// `BD-11` compara ventanas, y el cronograma de flota dibuja <b>varias barras por carril</b>
    /// precisamente porque un vehículo tiene varias misiones a lo largo del mes.
    ///
    /// Por eso acá se bloquea sólo lo que vuelve al vehículo <b>inutilizable</b> —taller, no
    /// disponible, prestado, terminal— y el solape lo decide `BD-11`, que además nombra al
    /// titular. Si `ASIGNADO` bloqueara, taparía a `BD-11` con un mensaje mucho peor: «está
    /// asignado» en vez de «lo tiene la misión X de la delegación Y, del 20 al 23».
    ///
    /// ⚠️ <b>Queda como hallazgo para el PO</b>, no resuelto en silencio: o §10.2 se corrige, o
    /// hay que explicar cómo se programan dos misiones de un mismo vehículo en meses
    /// distintos.
    ///
    /// ── La otra mitad de `BD-07` no se evalúa ───────────────────────────────
    /// La <b>compatibilidad</b> entre lo que se mueve y el tipo de vehículo necesita la matriz
    /// de `M-02`, que no existe, y el objeto del traslado es texto libre: no hay nada
    /// estructurado contra lo que contrastarla. No se finge.
    /// </summary>
    private static string ExigirVehiculoDisponible(EstadoOperativo? estado)
    {
        if (estado is null)
            return " · BD-07 NO evaluada: el vehículo no tiene estado operativo declarado";

        // Los tres que dejan al vehículo utilizable. `ASIGNADO` y `EN_MISION` describen un
        // compromiso vigente, no una imposibilidad futura.
        if (estado is EstadoOperativo.Disponible
                   or EstadoOperativo.Asignado
                   or EstadoOperativo.EnMision)
            return $" · BD-07 verificada: vehículo {estado}";

        throw new BloqueoDuro("BD-07",
            $"El vehículo está en estado {estado} y no se puede comprometer. " +
            // Decir en cuál está y no sólo que «no está disponible»: de EN_TALLER se sale
            // esperando, de DADO_DE_BAJA no se sale, y quien programa necesita saber si
            // vale la pena volver mañana.
            (estado is EstadoOperativo.DadoDeBaja or EstadoOperativo.RetiradoDeFlota
                ? "Es un estado terminal: este vehículo ya no vuelve a la flota."
                : "Elija otro vehículo o espere a que vuelva a estar disponible."));
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
    /// `T-06` — SOLICITADA → RECHAZADA. <b>La otra mitad del pronunciamiento.</b>
    ///
    /// ── Por qué el motivo es del catálogo Y ADEMÁS texto libre ───────────────
    /// `HU-014`: <i>«seleccione un motivo del catálogo. El texto libre complementa el motivo
    /// tipificado, no lo sustituye»</i>. Sin tipificación no hay forma de contar cuántas
    /// solicitudes se rechazan por gasto no justificado; sin texto libre, la jefatura no
    /// puede decirle a la dependencia qué pasó. Hacen falta las dos.
    ///
    /// ── Es terminal, y eso es lo que le da valor ─────────────────────────────
    /// De `RECHAZADA` no sale ninguna transición. <i>«La negativa queda documentada y no se
    /// borra reabriendo el expediente»</i>: quien quiera insistir presenta una solicitud
    /// nueva. Un rechazo que se puede deshacer no es un pronunciamiento, es un borrador.
    ///
    /// ⚠️ <b>Dos efectos no ocurren.</b> Liberar el número de expediente sin reciclarlo es de
    /// `M-01` —los rangos de folio por delegación—, y la acción <i>«crear nueva solicitud a
    /// partir de esta»</i>, que preserva el vínculo, es de `M-06`. Ninguno se finge.
    /// </summary>
    /// <param name="motivo">Un código del catálogo. <b>Obligatorio.</b></param>
    /// <param name="comentario">
    /// El texto libre. <b>También obligatorio</b>: el motivo tipificado dice qué se cuenta, el
    /// comentario dice a la dependencia qué pasó. Un rechazo sin explicación la deja sin
    /// saber si vale la pena replantearlo.
    /// </param>
    public void Rechazar(
        IdPersona ejecuta,
        string motivo,
        string comentario,
        CatalogoDeMotivosDeRechazo catalogo,
        DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Solicitada, "T-06");
        ExigirSegregacionDeAutorizacion(ejecuta, "rechazarla");

        if (!catalogo.Contiene(motivo))
            throw new BloqueoDuro("T-06",
                $"«{motivo}» no está en el catálogo de motivos de rechazo. El texto libre " +
                "complementa el motivo tipificado, no lo sustituye. Motivos disponibles: " +
                string.Join(", ", catalogo.Codigos) + ".");

        if (string.IsNullOrWhiteSpace(comentario))
            throw new BloqueoDuro("T-06",
                "Un rechazo exige explicación además del motivo: el tipificado dice qué se " +
                "cuenta, el texto dice a la dependencia qué pasó y si vale la pena replantearlo.");

        Registrar("T-06", EstadoDeMision.Rechazada, ejecuta, momento,
            $"{motivo} · {comentario.Trim()}");
    }

    /// <summary>
    /// `T-04` — SOLICITADA → BORRADOR. <b>Devolver para corrección</b>, que no es rechazar.
    ///
    /// ── La diferencia, que es toda ──────────────────────────────────────────
    /// `T-06` dice <i>«no»</i> y es terminal. Ésta dice <i>«así no»</i>: el expediente vuelve a
    /// manos de quien lo capturó, se corrige y se reenvía por `T-02`. Confundirlas hace que
    /// una solicitud arreglable muera, o que una improcedente dé vueltas para siempre.
    ///
    /// ── Motivo obligatorio, libre y visible para el solicitante ─────────────
    /// Libre y no tipificado, a diferencia del rechazo: acá no se mide por qué se dijo que
    /// no —no se dijo—, se dice <b>qué falta</b>. Un catálogo no puede enumerar lo que falta
    /// en un expediente concreto.
    ///
    /// ⚠️ <b>Tres efectos no ocurren, y ninguno se finge.</b> El <b>versionado</b> del
    /// expediente —<i>«se incrementa la versión; la anterior se conserva íntegra»</i>— no
    /// existe: el diario conserva el rastro de la devolución, pero no una versión 1 del
    /// contenido frente a una versión 2. La <b>liberación del número</b> es de `M-01`. Y la
    /// precondición de que <i>«ninguna autorización de nivel se haya registrado»</i> es hoy
    /// vacua: el escalamiento de `RN-02` no está construido y sólo hay un `T-05`.
    /// </summary>
    public void DevolverParaCorreccion(IdPersona ejecuta, string motivo, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Solicitada, "T-04");
        ExigirSegregacionDeAutorizacion(ejecuta, "devolverla");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("T-04",
                "Devolver exige decir qué corregir: el expediente vuelve a quien lo capturó, " +
                "y sin el motivo no sabe qué arreglar antes de reenviarlo.");

        Registrar("T-04", EstadoDeMision.Borrador, ejecuta, momento, motivo.Trim());
    }

    /// <summary>
    /// `T-03` — BORRADOR → ANULADA. <b>Descartar un borrador que nunca se envió.</b>
    ///
    /// <i>«No hay asiento reverso porque no hubo transacción.»</i> El expediente no entró al
    /// circuito de control, así que descartarlo no revierte nada — pero <b>se registra</b>,
    /// porque un borrador que desaparece sin rastro es indistinguible de uno que nunca
    /// existió, y el diario es de sólo agregar.
    /// </summary>
    public void DescartarBorrador(IdPersona ejecuta, string motivo, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Borrador, "T-03");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("T-03", "Descartar un borrador exige motivo.");

        Registrar("T-03", EstadoDeMision.Anulada, ejecuta, momento, motivo.Trim());
    }

    /// <summary>
    /// `T-07` — SOLICITADA → ANULADA. <b>Desistimiento</b> del solicitante (`ACT-02`) o
    /// <b>anulación administrativa</b> (`ACT-08`).
    ///
    /// ── Por qué NO lleva segregación ────────────────────────────────────────
    /// Porque no es un pronunciamiento sobre la solicitud: es que <b>quien la pidió ya no la
    /// quiere</b>. `BD-01` existe para que nadie autorice lo que él mismo pidió; desistir de
    /// lo propio es lo contrario, y exigir un tercero para retirar una solicitud obligaría a
    /// molestar a la jefatura para deshacer algo que no llegó a nada.
    /// </summary>
    public void Desistir(IdPersona ejecuta, string motivo, DateTimeOffset momento)
    {
        ExigirEstado(EstadoDeMision.Solicitada, "T-07");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("T-07",
                "Retirar una solicitud exige motivo: se retira de las bandejas de quienes " +
                "iban a pronunciarse, y tienen derecho a saber por qué dejó de estar.");

        Registrar("T-07", EstadoDeMision.Anulada, ejecuta, momento, motivo.Trim());
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
    /// <param name="circulacion">
    /// Lo que hace falta para juzgar `BD-04`: el calendario vigente, la excepción del
    /// vehículo si la tiene, y los permisos emitidos. También obligatorio, y por lo mismo.
    /// </param>
    /// <param name="conflicto">
    /// Si la reserva está en conflicto por indisponibilidad del vehículo — `RN-60`.
    /// <b>Obligatorio y sin omisión</b>, por la misma razón que la custodia: un booleano por
    /// omisión dejaría que un llamador nuevo despachara sin haber consultado, y el bloqueo se
    /// apagaría solo.
    ///
    /// ⚠️ <b>No tiene identificador `BD-xx`.</b> La máquina de estados no lo cataloga y `RN-60`
    /// sí lo declara; queda como hallazgo para que la autoridad lo incorpore.
    /// </param>
    public void Despachar(
        IdPersona ejecuta,
        AsignacionDeMision asignacion,
        MatrizDeLicencias matriz,
        PoliticaDeDocumentacion politica,
        DateTimeOffset momento,
        CustodiaAlDespachar custodias,
        CirculacionEnDiaInhabil circulacion,
        ConflictoPorIndisponibilidad conflicto,
        int? odometroDeEntrega = null)
    {
        ExigirEstado(EstadoDeMision.Programada, "T-12");

        // `RN-60` — la marca de conflicto impide el despacho, y no expira en silencio.
        ReglasDeLaIndisponibilidad.ExigirSinConflicto(conflicto);
        var evidencia = ExigirHabilitacionYDocumentacion(asignacion, matriz, politica, momento);
        var custodia = ExigirCustodiaVigente(custodias.Historial, momento)
                       + AdvertirSiLaCustodiaEstaVacante(custodias, momento);
        var inhabil = ExigirPermisoSiCirculaEnDiaInhabil(circulacion);

        // `INV-17` exige acta de entrega **con odómetro**. Va como dato del asiento y no
        // dentro del texto porque `T-15` y `T-16` lo vuelven a leer: son las dos
        // transiciones que tienen que probar que el vehículo NUNCA SALIÓ, y ocurren antes
        // de `T-14` — así que la lectura de salida no existe todavía. La única contra la
        // que pueden comparar es ésta.
        var entrega = odometroDeEntrega is { } km
            ? $" · acta de entrega con odómetro {km:N0} km"
            : " · acta de entrega SIN odómetro: `INV-17` no se pudo verificar";

        Registrar("T-12", EstadoDeMision.Despachada, ejecuta, momento,
            evidencia + custodia + inhabil + entrega, odometro: odometroDeEntrega);
    }

    /// <summary>
    /// `T-15` — Anular con devolución íntegra · `DESPACHADA` → `ANULADA`.
    ///
    /// <b>Es la transición más delicada del sistema</b>: hay documentos con folio emitidos y
    /// dinero público entregado. La autoridad la describe así, y de ahí sale todo lo que sigue.
    ///
    /// ── La bifurcación que decide si esto es una anulación o un hecho ───────
    /// §10.1 no deja margen: <i>«si hubo cualquier consumo, aunque sea parcial, `T-15` no está
    /// disponible y el camino es `T-16`»</i>. No es un tecnicismo — <b>anular sería borrar un
    /// hecho económico</b>. El dinero salió, se gastó, y un expediente `ANULADA` diría que
    /// nunca ocurrió.
    ///
    /// ── No hay estado «anulación en trámite», y es a propósito ──────────────
    /// La autoridad lo decidió: <i>«multiplicaría las transiciones sin agregar control, porque
    /// el control real es la lista de devoluciones pendientes, no un nombre de estado»</i>.
    /// Mientras falte una devolución, la misión <b>sigue en `DESPACHADA`</b> — que es lo que
    /// hace este método al rechazar.
    /// </summary>
    /// <param name="odometroDeRetorno">
    /// La lectura al recibir el vehículo. <b>El vehículo no salió</b>, así que tiene que
    /// coincidir con la de entrega dentro de la tolerancia: si no coincide, salió y volvió, y
    /// entonces esta transición no es la que corresponde.
    /// </param>
    public void AnularDespachada(
        IdPersona ejecuta,
        DateTimeOffset momento,
        string motivo,
        int odometroDeRetorno,
        int toleranciaKm,
        RecuentoDeAsignaciones? combustible = null)
    {
        ExigirEstado(EstadoDeMision.Despachada, "T-15");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("T-15",
                "Anular una misión despachada exige motivo tipificado: hay folios emitidos y " +
                "dinero entregado, y el descargo de ambos se sostiene en esta razón.");

        var combustibleOk = ExigirDevolucionIntegra(combustible);
        var odometro = ExigirQueElVehiculoNoHayaSalido(odometroDeRetorno, toleranciaKm);

        Registrar("T-15", EstadoDeMision.Anulada, ejecuta, momento,
            $"{motivo.Trim()} · {odometro}{combustibleOk}");
    }

    /// <summary>
    /// `T-16` — Misión no ejecutada con consumo · `DESPACHADA` → `RETORNADA`.
    ///
    /// El caso típico que la autoridad nombra: <i>el motorista llenó el tanque la tarde
    /// anterior y la misión se suspendió esa noche</i>. Hubo movimiento de fondos públicos, así
    /// que la misión <b>tiene que liquidarse</b> aunque su kilometraje sea cero.
    ///
    /// Queda marcada como <b>no ejecutada</b> para que no contamine los indicadores de
    /// kilometraje y rendimiento: una misión de cero kilómetros con treinta galones consumidos
    /// destruiría el promedio de la flota y haría que `RN-30` señalara al vehículo equivocado.
    /// </summary>
    public void RegistrarNoEjecutadaConConsumo(
        IdPersona ejecuta,
        DateTimeOffset momento,
        string motivo,
        int odometroDeRetorno,
        int toleranciaKm,
        RecuentoDeAsignaciones? combustible = null)
    {
        ExigirEstado(EstadoDeMision.Despachada, "T-16");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("T-16", "`T-16` exige motivo tipificado.");

        var odometro = ExigirQueElVehiculoNoHayaSalido(odometroDeRetorno, toleranciaKm);

        // **No se exige que HAYA habido consumo.** La autoridad admite el otro caso en la
        // misma frase: «hubo consumo O parte de lo entregado no es devolvible». Exigir consumo
        // dejaría sin salida al vale entregado que no se puede devolver, que es un hecho
        // económico igual de real y que tampoco cabe en `T-15`.
        var detalle = combustible is null
            ? " · asignaciones de combustible NO consultadas"
            : $" · {combustible.ConConsumo} de {combustible.Total} asignación(es) con consumo";

        Registrar("T-16", EstadoDeMision.Retornada, ejecuta, momento,
            $"NO EJECUTADA — no computa para indicadores de kilometraje ni rendimiento. " +
            $"{motivo.Trim()} · {odometro}{detalle}");
    }

    /// <summary>
    /// `T-15` — la devolución íntegra del fondo, que es la precondición que más se incumple.
    /// </summary>
    private static string ExigirDevolucionIntegra(RecuentoDeAsignaciones? combustible)
    {
        if (combustible is null)
            return " · devolución del fondo NO verificada: no se consultaron las asignaciones";

        // El consumo manda sobre todo lo demás: aunque todo lo demás esté devuelto, un solo
        // galón gastado convierte esto en un hecho económico que no se puede anular.
        if (combustible.HuboConsumo)
            throw new BloqueoDuro("T-15",
                $"{combustible.ConConsumo} asignación(es) de esta misión ya tuvieron consumo. " +
                "`T-15` no está disponible: anular sería borrar un hecho económico. El camino " +
                "es `T-16`, que retorna la misión sin ejecutar y la liquida igual.");

        if (combustible.EntregadasSinDevolver > 0)
            throw new BloqueoDuro("T-15",
                $"Faltan {combustible.EntregadasSinDevolver} devolución(es) de vales entregados. " +
                "Mientras la devolución no esté completa la misión sigue DESPACHADA, con la " +
                "anulación en trámite: el control es la lista de pendientes, no un estado nuevo.");

        return combustible.Total == 0
            ? " · sin combustible asignado"
            : $" · {combustible.Total} asignación(es) devueltas o anuladas íntegras";
    }

    /// <summary>
    /// `T-15` y `T-16` — <b>el vehículo no salió.</b>
    ///
    /// La autoridad lo exige en las dos: <i>«el odómetro final coincide con el de entrega
    /// dentro de la tolerancia»</i>. Si no coincide, el vehículo salió y volvió — y entonces
    /// ninguna de las dos transiciones es la que corresponde: es una misión ejecutada, y va
    /// por `T-14` y `T-18`.
    ///
    /// La tolerancia existe porque mover el vehículo dentro del predio suma kilómetros reales.
    /// </summary>
    private string ExigirQueElVehiculoNoHayaSalido(int odometroDeRetorno, int toleranciaKm)
    {
        // **La de la ENTREGA, no la de la salida.** `T-14` no ocurrió: el vehículo tiene las
        // llaves y no arrancó. Leer `T-14` acá devolvería siempre nulo y el control quedaría
        // inerte — pasando siempre, que es la peor forma de no existir.
        var salida = _diario.LastOrDefault(t => t.Id == "T-12")?.Odometro;

        if (salida is null)
            return "odómetro de entrega NO registrado: no se pudo comprobar que el vehículo no salió";

        var recorrido = odometroDeRetorno - salida.Value;

        if (recorrido < 0)
            throw new BloqueoDuro("BD-05",
                $"El odómetro de retorno ({odometroDeRetorno:N0} km) es menor que el de entrega " +
                $"({salida:N0} km). Es físicamente imposible: corrija la lectura.");

        if (recorrido > toleranciaKm)
            throw new BloqueoDuro("T-15",
                $"El vehículo recorrió {recorrido:N0} km desde la entrega, y la tolerancia es " +
                $"{toleranciaKm:N0} km. Salió: esta misión se ejecutó, y se cierra por `T-18`, " +
                "no anulándola.");

        return $"odómetro {odometroDeRetorno:N0} km, {recorrido:N0} km desde la entrega — el vehículo no salió";
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

    /// <summary>
    /// `BD-04` — <b>circular en día inhábil exige permiso de la máxima autoridad.</b>
    ///
    /// ── Las tres respuestas posibles, y ninguna se calla ─────────────────────
    /// <b>No toca día inhábil</b> → nada que exigir, y se deja constancia del calendario contra
    /// el que se juzgó: dentro de dos años, reconstruir por qué un sábado no exigió permiso
    /// requiere saber qué calendario estaba vigente.
    ///
    /// <b>Toca día inhábil y el vehículo está exceptuado</b> → pasa, <b>y el uso de la excepción
    /// queda registrado</b>. `BD-04` lo exige expresamente. Una excepción que se usa sin
    /// dejar rastro es una excepción que nadie puede auditar, y `RN-24` existe justamente
    /// porque la alternativa —autoexceptuarse alegando urgencia— vacía el control en una
    /// semana.
    ///
    /// <b>Toca día inhábil y no hay excepción</b> → tiene que haber permiso que ampare
    /// <b>vehículo, motorista, ruta y ventana</b>. Si no, bloqueo.
    ///
    /// ── La hora, y sus dos condiciones ───────────────────────────────────────
    /// Se evalúa <b>sólo</b> cuando la misión declara sus horas y la institución declaró su
    /// horario hábil. Falta cualquiera de las dos y no se juzga — y se dice cuál falta, en
    /// vez de dejar creer que se verificó.
    ///
    /// ── Lo que este control sigue sin cubrir ─────────────────────────────────
    /// El <b>salvoconducto impreso</b> que debe emitirse junto con la Orden de Misión es de
    /// `M-15`, que no existe. No se finge.
    /// </summary>
    private string ExigirPermisoSiCirculaEnDiaInhabil(CirculacionEnDiaInhabil circulacion)
    {
        var ventana = Solicitud.Ventana;
        var inhabiles = circulacion.Calendario.InhabilesEn(ventana);
        var horasFuera = circulacion.Calendario.HorasInhabilesEn(ventana);

        // Qué NO se pudo mirar. Va al diario incluso cuando `BD-04` no aplica: un asiento
        // que dice «no aplica» sin decir contra qué se miró es indistinguible de uno que
        // verificó las dos mitades, y dentro de dos años nadie podrá saber cuál fue.
        var sinEvaluar = !ventana.DeclaraHoras
            ? " · hora NO evaluada: la misión no declara horas"
            : circulacion.Calendario.Horario is null
                ? " · hora NO evaluada: la institución no declaró horario hábil"
                : "";

        if (inhabiles.Count == 0 && horasFuera.Count == 0)
            return $" · BD-04 no aplica · calendario {circulacion.Calendario.Version}{sinEvaluar}";

        var motivos = new List<string>();

        if (inhabiles.Count > 0)
            motivos.Add("días inhábiles: " +
                        string.Join(", ", inhabiles.Select(d => d.ToString("yyyy-MM-dd"))));

        if (horasFuera.Count > 0)
            motivos.Add("fuera del horario hábil: " + string.Join(", ", horasFuera));

        var dias = string.Join(" · ", motivos);

        if (circulacion.Excepcion is { } excepcion && excepcion.VigenteAl(ventana.Salida))
            return $" · BD-04 exceptuada · servicio {excepcion.Tipo} vigente desde " +
                   $"{excepcion.Desde:yyyy-MM-dd} · {dias}{sinEvaluar}";

        var permiso = circulacion.Permisos.FirstOrDefault(
            p => p.Ampara(circulacion.Vehiculo, circulacion.Motorista, Solicitud.Destino, ventana));

        if (permiso is null)
            throw new BloqueoDuro("BD-04",
                $"La misión circula en franja inhábil ({dias}) y no hay permiso de la máxima " +
                "autoridad que ampare este vehículo, este motorista, este destino y esta " +
                $"ventana ({ventana.Salida:yyyy-MM-dd} al {ventana.FinDelRango:yyyy-MM-dd}). " +
                // Un relevo de motorista invalida el permiso, y quien despacha necesita
                // saber si el problema es que no hay ninguno o que el que hay no le sirve.
                (circulacion.Permisos.Count == 0
                    ? "No hay ningún permiso registrado para esta misión."
                    : $"Hay {circulacion.Permisos.Count} permiso(s) registrado(s), ninguno que ampare " +
                      "esta combinación — un relevo de motorista invalida el permiso."));

        return $" · BD-04 verificada · permiso {permiso.Folio} de {permiso.EmitidoPor.Valor} · " +
               $"{dias} · calendario {circulacion.Calendario.Version}{sinEvaluar}";
    }

    /// <summary>
    /// La <b>custodia vacante</b> de `RN-22`: <i>«custodio que cesa en el cargo dejando el
    /// vehículo asignado»</i>.
    ///
    /// ── El hueco que `BD-13` sola no cubre ──────────────────────────────────
    /// `BD-13` mira la tarjeta de responsabilidad y la encuentra <b>abierta</b> — nadie la
    /// cerró, porque la persona ya no está para firmarla. Y despacha. El vehículo sale a
    /// nombre de alguien que ya no trabaja en la institución, que es el mismo daño que
    /// `BD-13` existe para evitar por otro camino: cuando aparezca el golpe o la multa,
    /// <b>no hay a quién imputarla</b>.
    ///
    /// ── Advierte, no bloquea, y eso está decidido afuera ────────────────────
    /// `RN-22`: <i>«el sistema marca el vehículo como custodia vacante, con alerta al Jefe
    /// de Transporte y bloqueo de despacho <b>tras un plazo configurable</b>»</i> — y el plazo
    /// es `[C]`. Mientras no se decida, hay alerta y no hay bloqueo. Inventar el plazo
    /// dejaría vehículos varados contra un número que nadie acordó.
    ///
    /// ── Ausencia de dato no es dato de ausencia ─────────────────────────────
    /// Sólo se advierte cuando el espejo <b>conoce</b> a la persona y ninguno de sus puestos
    /// está vigente. Si el espejo no sabe nada de ella —la integración no corrió, o esa
    /// dependencia no se ha sincronizado—, no se dice nada: lo contrario declararía cesada
    /// a toda la institución cada vez que el espejo esté vacío, que es hoy.
    /// </summary>
    private static string AdvertirSiLaCustodiaEstaVacante(
        CustodiaAlDespachar custodias,
        DateTimeOffset momento)
    {
        var fecha = DateOnly.FromDateTime(momento.Date);
        var vigente = custodias.Historial.FirstOrDefault(c => c.VigenteAl(fecha));

        // Si no hay custodia vigente, `BD-13` ya bloqueó y no se llega acá.
        if (vigente is null) return "";

        if (!custodias.Organigrama.Conoce(vigente.Custodio)) return "";

        if (custodias.Organigrama.PuestosDe(vigente.Custodio, fecha).Count > 0) return "";

        return $" · ⚠ CUSTODIA VACANTE: {vigente.Custodio.Valor} no ocupa ningún puesto " +
               $"al {fecha:yyyy-MM-dd} y el vehículo sigue a su nombre. Levante el acta de " +
               "entrega-recepción, o la unilateral con hallazgo abierto si ya no está (`RN-101`).";
    }

    /// <summary>`T-14` — DESPACHADA → EN_RUTA. La ejecuta el motorista, y opera desconectado.</summary>
    /// <param name="odometro">
    /// La lectura al salir y la última conocida del vehículo — `BD-05`. <b>Sin valor por
    /// omisión</b>: el compilador obliga a que todo llamador conteste, porque «este vehículo
    /// no tiene lectura previa» y «nadie consultó» no pueden verse igual en un bloqueo duro.
    /// </param>
    public void IniciarRuta(
        IdPersona ejecuta,
        DateTimeOffset momento,
        OdometroAlSalir odometro,
        Ulid? idDeCaptura = null)
    {
        ExigirEstado(EstadoDeMision.Despachada, "T-14");

        // `BD-05`: la lectura de salida no puede ser menor que la última conocida. O es
        // error de digitación, o es retroceso de odómetro -- y las dos se corrigen en el
        // momento, con el tablero delante.
        if (odometro.UltimaConocida is { } ultima && odometro.Lectura < ultima)
            throw new BloqueoDuro("BD-05",
                $"El odómetro de salida ({odometro.Lectura:N0} km) es menor que la última " +
                $"lectura conocida del vehículo ({ultima:N0} km). Es error de digitación o " +
                "retroceso de odómetro: verifique el tablero antes de continuar.");

        var constancia = odometro.UltimaConocida is { } previa
            ? $"odómetro de salida {odometro.Lectura:N0} km · última conocida {previa:N0} km"
            // Primera misión del vehículo. Se dice, porque «sin lectura previa» y «no se
            // verificó» son cosas distintas y el diario tiene que distinguirlas.
            : $"odómetro de salida {odometro.Lectura:N0} km · sin lectura previa registrada";

        Registrar("T-14", EstadoDeMision.EnRuta, ejecuta, momento,
                  constancia + TextoDelNivel(odometro.Nivel, odometro.RazonSinNivel),
                  idDeCaptura, odometro: odometro.Lectura, nivel: odometro.Nivel);
    }

    /// <summary>
    /// `T-18` — EN_RUTA → RETORNADA. Registra un hecho consumado: por `P-2` no se bloquea,
    /// se validan coherencias que pueden derivar en cierre con hallazgo.
    ///
    /// ── La excepción a `P-2`, y por qué la autoridad la puso ─────────────────
    /// `BD-05` <b>sí</b> bloquea en el `T-18` <b>ordinario</b>: una lectura de retorno menor
    /// que la de salida es <b>físicamente imposible</b>, así que no es un hecho consumado que
    /// registrar — es un número mal tecleado, y hay alguien con el tablero delante que puede
    /// corregirlo.
    ///
    /// En el subtipo <b>constatado</b> no bloquea, y eso corrigió el hallazgo `HB3-04`: ahí el
    /// vehículo ya está en el predio y negarse a registrarlo <b>lo deja secuestrado por un
    /// trámite</b> mientras la delegación se queda sin unidad. Se registra tal cual y se marca.
    /// </summary>
    public void Retornar(
        IdPersona ejecuta,
        DateTimeOffset momento,
        OdometroAlRetornar odometro,
        Ulid? idDeCaptura = null)
    {
        ExigirEstado(EstadoDeMision.EnRuta, "T-18");

        var salida = OdometroDeSalida();
        var constancia = ExigirCoherenciaDelOdometro(odometro, salida);

        Registrar("T-18", EstadoDeMision.Retornada, ejecuta, momento,
                  constancia + TextoDelNivel(odometro.Nivel, odometro.RazonSinNivel),
                  idDeCaptura, odometro: odometro.Lectura, nivel: odometro.Nivel);
    }

    /// <summary>
    /// Qué se anotó del tanque. <b>La ausencia se dice</b> — `RN-80`: el campo no consignado
    /// se declara y <b>no se estima</b>, porque estimarlo produciría un remanente inventado
    /// que después nadie podría distinguir de uno medido.
    /// </summary>
    private static string TextoDelNivel(NivelDeTanque? nivel, string? razon = null)
    {
        if (nivel is not null)
            return nivel.Escala is EscalaDeNivel.FraccionDelIndicador
                ? $" · tanque a {nivel.Valor:P0} del indicador"
                : $" · tanque con {nivel.Valor:N2} galones";

        // La razón va cuando la hay. Sin ella la ausencia queda declarada pero sin nada que
        // reclamar: no se sabe si faltó porque el indicador estaba averiado o porque nadie
        // se acordó, y sólo la primera se puede corregir.
        return string.IsNullOrWhiteSpace(razon)
            ? " · nivel de tanque NO CONSIGNADO (`RN-83`)"
            : $" · nivel de tanque NO CONSIGNADO (`RN-83`): {razon.Trim()}";
    }

    /// <summary>
    /// El nivel al salir y al retornar, cuando los dos se anotaron.
    ///
    /// Es lo que hace real el reparo <c>NivelDeTanqueDispar</c> de `RN-30`: hasta ahora se
    /// marcaba a mano, y una casilla que alguien olvida marcar deja pasar un cálculo que no
    /// significa nada.
    /// </summary>
    public (NivelDeTanque? Salida, NivelDeTanque? Retorno) NivelesDelTanque =>
        (_diario.LastOrDefault(t => t.Id == "T-14")?.Nivel,
         _diario.LastOrDefault(t => t.Id == "T-18")?.Nivel);

    /// <summary>
    /// La lectura con que salió, sacada del diario y no de un campo.
    ///
    /// P-1 vale también para los datos que las precondiciones necesitan: guardar el odómetro
    /// de salida en una propiedad sería una copia que se puede desincronizar del asiento que
    /// lo registró.
    /// </summary>
    private int? OdometroDeSalida() =>
        _diario.LastOrDefault(t => t.Id == "T-14")?.Odometro;

    /// <summary>
    /// `BD-05` al retornar. Devuelve la constancia para el diario.
    ///
    /// ── Lo que este control NO evalúa, y no se finge ─────────────────────────
    /// Los kilómetros recorridos contra la <b>distancia estimada</b> por un factor
    /// configurable — en las dos direcciones, porque `NRM-01` vigila el exceso y el defecto.
    /// No hay distancia estimada en el sistema: sale del mapa de ARGOS o de una tabla de
    /// rutas, y ninguno existe. Tampoco el <b>salto imposible respecto al tiempo</b>, que
    /// necesita un umbral de velocidad que nadie declaró — `[C]`.
    ///
    /// Los dos son marcas para revisión, no bloqueos, así que su ausencia no deja pasar nada
    /// que debiera detenerse. Lo que sí deja es <b>sin detectar</b> el hallazgo `H-02`.
    /// </summary>
    private static string ExigirCoherenciaDelOdometro(OdometroAlRetornar odometro, int? salida)
    {
        if (salida is not { } desde)
            // Sin `T-14` en el diario no hay contra qué comparar. Puede pasar en un
            // expediente anterior a que el odómetro existiera; se dice y se sigue.
            return $"odómetro de retorno {odometro.Lectura:N0} km · sin lectura de salida registrada";

        // El acta de sustitución cambia la aritmética: el odómetro instalado arranca donde
        // arranque, y comparar contra la lectura del retirado no significa nada. Es un hecho
        // mecánico que hay que poder registrar, no un permiso para saltarse el control.
        if (odometro.Acta is { } acta)
            return $"odómetro de retorno {odometro.Lectura:N0} km · BD-05 no comparable: " +
                   $"acta de sustitución {acta.Folio} del {acta.Fecha:yyyy-MM-dd} " +
                   $"(retirado {acta.LecturaDelRetirado:N0} km, instalado {acta.LecturaDelInstalado:N0} km)";

        if (odometro.Lectura < desde)
        {
            if (odometro.Subtipo == SubtipoDeRetorno.Ordinario)
                throw new BloqueoDuro("BD-05",
                    $"El odómetro de retorno ({odometro.Lectura:N0} km) es menor que el de " +
                    $"salida ({desde:N0} km). Es físicamente imposible: corrija la lectura. " +
                    "Si el odómetro se sustituyó o se reinició, registre el acta de `M-11` " +
                    "antes de cerrar la bitácora.");

            // `RN-79`: se registra tal cual, se marca la inconsistencia, y el vehículo se
            // libera igual.
            return $"odómetro de retorno {odometro.Lectura:N0} km · ⚠ INCONSISTENTE: menor que " +
                   $"el de salida ({desde:N0} km) · retorno constatado por un tercero, el " +
                   "vehículo se libera igual (RN-79)";
        }

        if (odometro.Lectura == desde)
        {
            // No bloquea, pero no pasa en silencio: es el patrón de la misión que nunca se
            // hizo, y ése es justamente el que busca el Tribunal Superior de Cuentas.
            if (string.IsNullOrWhiteSpace(odometro.Justificacion))
                throw new BloqueoDuro("BD-05",
                    $"El odómetro de retorno iguala al de salida ({desde:N0} km): la misión no " +
                    "recorrió un solo kilómetro. Puede registrarse, pero exige justificación — " +
                    "es el patrón de la misión que nunca se hizo.");

            return $"odómetro de retorno {odometro.Lectura:N0} km · sin recorrido · " +
                   $"justificación: {odometro.Justificacion!.Trim()}";
        }

        return $"odómetro de retorno {odometro.Lectura:N0} km · recorrido " +
               $"{odometro.Lectura - desde:N0} km";
    }

    /// <summary>
    /// `T-19` — RETORNADA → LIQUIDADA.
    ///
    /// ── `INV-34`, que hasta hoy no se evaluaba ──────────────────────────────
    /// <i>«Todas las asignaciones de fondo vinculadas están `LIQUIDADAS`»</i>, y §10.1 lo
    /// repite: <b>`T-19` liquidar la misión exige que todas sus asignaciones estén
    /// `LIQUIDADAS`</b>. Liquidar la misión con vales vivos es declarar cerrado el resultado
    /// económico de un viaje cuyo dinero todavía nadie cuadró.
    /// </summary>
    /// <param name="combustible">
    /// Nulo es <b>no evaluada</b>, y el diario lo dice. Hay expedientes anteriores a `M-09`, y
    /// hacer pasar «no había con qué comprobar» por «se comprobó y estaba bien» es la mentira
    /// que estas nulidades existen para no contar.
    /// </param>
    public void Liquidar(
        IdPersona ejecuta, DateTimeOffset momento, RecuentoDeAsignaciones? combustible = null)
    {
        ExigirEstado(EstadoDeMision.Retornada, "T-19");

        var constancia = ExigirCombustibleLiquidado(combustible);
        Registrar("T-19", EstadoDeMision.Liquidada, ejecuta, momento, constancia);
    }

    /// <summary>`INV-34` — ningún vale vivo al liquidar la misión.</summary>
    private static string ExigirCombustibleLiquidado(RecuentoDeAsignaciones? combustible)
    {
        if (combustible is null)
            return "INV-34 NO evaluada: no se consultaron las asignaciones de combustible";

        if (combustible.SinLiquidar > 0)
            throw new BloqueoDuro("INV-34",
                $"La misión tiene {combustible.SinLiquidar} asignación(es) de combustible sin " +
                "liquidar. Liquidar la misión ahora sería declarar cerrado el resultado " +
                "económico de un viaje cuyo dinero todavía nadie cuadró.");

        return combustible.Total == 0
            // Cero no se calla: «sin combustible asignado» y «no se revisó» tienen que
            // distinguirse en el diario dos años después.
            ? "sin combustible asignado a esta misión"
            : $"{combustible.Total} asignación(es) de combustible, todas liquidadas";
    }

    public void DevolverLiquidacion(IdPersona ejecuta, DateTimeOffset momento, string motivo)
    {
        ExigirEstado(EstadoDeMision.Liquidada, "T-20");
        Registrar("T-20", EstadoDeMision.Retornada, ejecuta, momento, motivo);
    }

    /// <param name="combustible">
    /// §10.1: <b>`T-21` y `T-22` cerrar la misión exigen que todas estén conciliadas</b>, en
    /// cualquiera de las dos formas. Una desviación <i>explicada</i> no impide cerrar; lo que
    /// impide cerrar es un vale que nadie contrastó contra el kilometraje.
    /// </param>
    public void Cerrar(
        IdPersona ejecuta,
        DateTimeOffset momento,
        IReadOnlyList<HallazgoDetectado> criterios,
        string? justificacion,
        RecuentoDeAsignaciones? combustible = null)
    {
        ExigirEstado(EstadoDeMision.Liquidada, "T-21");
        ExigirSegregacionDeCierre(ejecuta);
        ExigirCombustibleConciliado(combustible);

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
    /// §10.1 — <b>ningún vale sin conciliar al cerrar la misión.</b>
    ///
    /// `CONCILIADA` y `CONCILIADA_CON_DESVIACION` cuentan las dos: la desviación con causa
    /// tipificada es un cierre con hallazgo, no un impedimento. Lo que no puede quedar es un
    /// vale liquidado que nadie contrastó contra el kilometraje — ahí `RN-30` no llegó a
    /// mirar, y el expediente se cerraría sin la única comprobación cruzada que tiene.
    /// </summary>
    private static void ExigirCombustibleConciliado(RecuentoDeAsignaciones? combustible)
    {
        if (combustible is null || combustible.SinConciliar == 0)
            return;

        throw new BloqueoDuro("T-21",
            $"Quedan {combustible.SinConciliar} asignación(es) de combustible sin conciliar " +
            "contra el kilometraje. Una desviación explicada no impide cerrar; un vale que " +
            "nadie contrastó, sí.");
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
    /// <param name="acto">
    /// Qué se intentó hacer — «autorizarla», «rechazarla», «devolverla». Va en el mensaje
    /// porque decir <i>«no puede autorizarla»</i> a quien intentó rechazar manda a buscar el
    /// problema donde no está.
    /// </param>
    private void ExigirSegregacionDeAutorizacion(IdPersona ejecuta, string acto = "autorizarla")
    {
        if (ejecuta == CapturadaPor)
            throw new BloqueoDuro("BD-01", $"Quien capturó la solicitud no puede {acto}.");

        // Se deriva del diario, no de un campo: P-1 vale también para los datos que las
        // precondiciones necesitan, o el estado y el diario se desincronizan.
        var enviadaPor = _diario.FirstOrDefault(t => t.Id == "T-02")?.Ejecuta;

        if (enviadaPor is { } remitente && ejecuta == remitente)
            throw new BloqueoDuro("BD-01", $"Quien envió la solicitud no puede {acto}.");

        if (ejecuta == SolicitanteDeDerecho)
            throw new BloqueoDuro("BD-01",
                "Usted figura como solicitante de derecho. El pronunciamiento sobre el " +
                "expediente es un acto de autoridad y no lo ejerce quien solicita: no puede " +
                $"{acto}, aunque no la haya capturado ni enviado.");
    }

    private void ExigirEstado(EstadoDeMision esperado, string transicion)
    {
        if (Estado != esperado)
            throw new TransicionInvalida(transicion, Estado, esperado);
    }

    private void Registrar(
        string id, EstadoDeMision destino, IdPersona ejecuta, DateTimeOffset momento, string? motivo,
        Ulid? idDeCaptura = null, RecursosTomados? recursos = null, int? odometro = null,
        NivelDeTanque? nivel = null) =>
        _diario.Add(new Transicion(
            id, destino, ejecuta, momento, motivo, idDeCaptura, recursos, odometro, nivel));
}
