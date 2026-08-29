using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// Hacia dónde va el dinero. <b>Las dos direcciones existen</b>, y esto no es simetría por
/// prolijidad: `RN-86` cierra con <i>«el combustible pagado con recursos propios del servidor
/// genera obligación de reintegro <b>a favor</b> del servidor»</i>, y `CE-26` lo remata —
/// <i>«un sistema que solo mide lo que el servidor le debe a la institución no es un sistema
/// de control: es un sistema de cobro»</i>.
/// </summary>
public enum DireccionDelReintegro
{
    /// <summary>El servidor le debe a la institución. El caso del faltante.</summary>
    AFavorDeLaInstitucion,

    /// <summary>
    /// La institución le debe al servidor — el peculio propio de `RN-C26d`.
    ///
    /// <b>No afecta el cuadre del fondo</b>: ese combustible no salió del fondo. `CE-26` lo
    /// dice sin margen: registrarlo como consumo del fondo hace que <i>«el cuadre mienta en
    /// los dos lados a la vez»</i>.
    ///
    /// `[C]` <b>si la institución admite y reembolsa esta figura</b> — insumo #37. Registrarla
    /// no la aprueba: la hace visible con su propia antigüedad, que es lo único que se puede
    /// hacer mientras se decide.
    /// </summary>
    AFavorDelServidor,
}

/// <summary>
/// Por qué nació la obligación. <b>Son las tres que `RN-86` nombra</b>, más la del peculio.
///
/// ── Esto no es el catálogo de la liquidación ────────────────────────────────
/// `CE-26` §3 tipifica <b>ocho</b> motivos de diferencia al liquidar, y la mayoría
/// <b>no</b> generan obligación: una variación de precio se recalcula, un redondeo dentro de
/// tolerancia no abre nada. Esa tipificación es de `HU-089` y no de acá. Este enum es el
/// subconjunto que `RN-86` declara generador — y tenerlo aparte impide que el día que el
/// catálogo de liquidación crezca, crezca con él la lista de cosas que nominan a una persona.
/// </summary>
public enum CausaDelReintegro
{
    /// <summary>
    /// <b>El motivo que tiene que existir.</b> `CE-26`: <i>«si el catálogo no lo tiene, el
    /// liquidador elige el motivo más cercano que no genere problema, y el faltante desaparece
    /// del reporte»</i>.
    /// </summary>
    SinCausaIdentificada,

    /// <summary>Fondo aplicado a un fin distinto al autorizado.</summary>
    AplicacionAFinDistinto,

    /// <summary>Extravío del instrumento o del efectivo, con acta — `V-06`.</summary>
    Extravio,

    /// <summary>
    /// El servidor puso de su bolsillo. <b>Única causa que va a favor de él</b>, y la que hace
    /// que esta entidad no sea un registro de cobranza.
    /// </summary>
    PeculioPropio,
}

/// <summary>
/// El ciclo propio de la obligación — `RN-86`: <i>«monto, responsable, notificación, descargo
/// del servidor, resolución y —si se paga— el asiento reverso»</i>.
///
/// ── Por qué es una entidad y no una marca en la misión ──────────────────────
/// Porque <b>sobrevive al cierre</b>. `CE-26` §4 lo decide de frente: la misión cierra por
/// `T-22` a `CERRADA_CON_HALLAZGO`, terminal e inmutable, y lo que queda vivo es esto. La
/// razón es la que sostiene toda la máquina — <i>«un expediente que no puede cerrarse se
/// abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver»</i>.
/// </summary>
public enum EstadoDeObligacion
{
    /// <summary>
    /// Nominada: hay monto, responsable y causa. <b>No nace en la liquidación</b> — `RN-86`
    /// punto 5 y `RN-74`: la determinación es materia del expediente y de quien corresponde.
    /// La liquidación tipifica el faltante; nominar a una persona es otro acto, de otro.
    /// </summary>
    Determinada,

    /// <summary>Se le notificó al servidor. Sin esto, el descargo no se le puede exigir.</summary>
    Notificada,

    /// <summary>El servidor presentó su descargo.</summary>
    ConDescargo,

    /// <summary>Quien corresponde resolvió confirmando la obligación. Sigue debiéndose.</summary>
    Resuelta,

    /// <summary>
    /// Pagada, con su asiento reverso. <b>Terminal.</b> `CE-26`: el reverso afecta los
    /// acumulados <b>del período en que se registra</b>, no los del período original.
    /// </summary>
    Saldada,

    /// <summary>
    /// La resolución acogió el descargo, o la determinación resultó errada. <b>Terminal, y no
    /// se borra</b>: que una obligación se haya dejado sin efecto es parte del expediente, y
    /// borrarla dejaría al servidor sin constancia de que se le imputó y de que no procedía.
    /// </summary>
    DejadaSinEfecto,
}

/// <summary>Un asiento del diario de la obligación. Mismo P-1 que todo lo demás.</summary>
/// <param name="Id">`R-01` nominar · `R-02` notificar · `R-03` descargo · `R-04` resolver · `R-05` dejar sin efecto · `R-06` pagar.</param>
/// <param name="Autor">
/// Quién y <b>con qué competencia</b>. Va como <see cref="Autoria"/> y no como
/// <see cref="IdPersona"/> sola porque el auditor no pregunta quién firmó: pregunta quién
/// autorizó y con qué competencia, y el puesto pudo cambiar de manos desde entonces.
/// </param>
/// <param name="Pagado">
/// Lo que este asiento abonó. <b>Sólo lo lleva `R-06`.</b> Va como dato y no dentro del
/// motivo porque el saldo se calcula sumándolos: sacarlo de una cadena de texto es el error
/// que ya se corrigió dos veces en este módulo.
/// </param>
public sealed record MovimientoDeObligacion(
    string Id,
    EstadoDeObligacion Destino,
    Autoria Autor,
    DateTimeOffset Momento,
    string Motivo,
    decimal? Pagado = null);

/// <summary>
/// La obligación de reintegro — `RN-86`, la entidad que `RN-29` numeral 4 daba por existente
/// y que no existía en ninguna regla ni en ninguna máquina de estados.
///
/// ── El agujero que cierra ───────────────────────────────────────────────────
/// `RN-86`: <i>«Sin ella, el cobro se pierde cuando la misión cierra: el expediente se
/// archiva, el hallazgo queda como marca, y el dinero no vuelve»</i>.
///
/// ── Y el que cierra pagar antes de que se resuelva ──────────────────────────
/// `CE-26` nombra la práctica: <i>«Se le da tiempo al motorista para que lo reponga. Si
/// repone, no queda registro de que hubo faltante»</i> — y sentencia: <b>«un control interno
/// que se activa solo cuando la persona no coopera no es un control»</b>. Por eso
/// <see cref="RegistrarPago"/> se admite desde cualquier estado vivo: pagar salda la deuda y
/// <b>no borra que existió</b>.
/// </summary>
public sealed class ObligacionDeReintegro
{
    private readonly List<MovimientoDeObligacion> _diario = [];

    private ObligacionDeReintegro(
        Ulid id, DireccionDelReintegro direccion, CausaDelReintegro causa,
        Ulid responsable, decimal monto, Ulid? mision, Ulid? asignacion, DateOnly fechaDelHecho)
    {
        Id = id;
        Direccion = direccion;
        Causa = causa;
        Responsable = responsable;
        Monto = monto;
        Mision = mision;
        Asignacion = asignacion;
        FechaDelHecho = fechaDelHecho;
    }

    public Ulid Id { get; }

    public DireccionDelReintegro Direccion { get; }

    public CausaDelReintegro Causa { get; }

    /// <summary>
    /// El motorista nominado, por su ULID del padrón — el mismo con que se identifica al
    /// receptor de un vale, que es contra quien se evalúa el bloqueo de nueva asignación.
    ///
    /// `CE-26`: <i>«La persona que firmó la recepción (`V-02`), no el rol»</i>.
    /// </summary>
    public Ulid Responsable { get; }

    /// <summary>
    /// <b>Congelado desde que se nomina.</b> `RN-86` punto 3 lo dice del monto asignado y vale
    /// igual acá: corregirlo hacia abajo para que cuadre es reescribir el pasado. Si el monto
    /// estaba mal, el camino es dejarla sin efecto y nominar otra.
    /// </summary>
    public decimal Monto { get; }

    /// <summary>
    /// La misión donde nació. <b>Nulo es posible</b>: `RN-93` prevé el hallazgo posterior, que
    /// puede nominar sobre un período y no sobre una misión concreta.
    /// </summary>
    public Ulid? Mision { get; }

    /// <summary>El vale del que salió el dinero, cuando lo hubo.</summary>
    public Ulid? Asignacion { get; }

    /// <summary>
    /// La fecha del hecho que la originó — el retorno, la anulación, la carga de bolsillo. No
    /// la de captura (`RN-46`), y no la de nominación: `RN-86` punto 7 exige que la antigüedad
    /// se cuente <b>desde el hecho original</b> para el saldo de apertura de `RN-97`.
    /// </summary>
    public DateOnly FechaDelHecho { get; }

    public IReadOnlyList<MovimientoDeObligacion> Diario => _diario;

    public EstadoDeObligacion Estado => _diario[^1].Destino;

    /// <summary>Lo abonado, sumando asientos `R-06`. Nunca un campo.</summary>
    public decimal Pagado => _diario.Sum(m => m.Pagado ?? 0m);

    /// <summary>Lo que todavía falta. Nunca negativo: pagar de más no genera crédito acá.</summary>
    public decimal Saldo => Math.Max(0m, Monto - Pagado);

    /// <summary>
    /// <b>Sigue viva.</b> Es la pregunta del bloqueo de `RN-86` y la del arqueo: una obligación
    /// resuelta y no pagada sigue abierta, y una notificada sin descargo también.
    /// </summary>
    public bool EstaAbierta => Estado
        is not (EstadoDeObligacion.Saldada or EstadoDeObligacion.DejadaSinEfecto);

    /// <summary>
    /// Cuántos días lleva afuera a una fecha dada. Es lo que el arqueo presenta y lo que
    /// `RN-97` arrastra al ejercicio siguiente.
    /// </summary>
    public int AntiguedadEnDias(DateOnly a) => a.DayNumber - FechaDelHecho.DayNumber;

    /// <summary>Rehidrata desde el diario. El estado es la proyección (P-1).</summary>
    public static ObligacionDeReintegro Reconstruir(
        Ulid id,
        DireccionDelReintegro direccion,
        CausaDelReintegro causa,
        Ulid responsable,
        decimal monto,
        Ulid? mision,
        Ulid? asignacion,
        DateOnly fechaDelHecho,
        IEnumerable<MovimientoDeObligacion> diario)
    {
        var obligacion = new ObligacionDeReintegro(
            id, direccion, causa, responsable, monto, mision, asignacion, fechaDelHecho);

        obligacion._diario.AddRange(diario);

        if (obligacion._diario.Count == 0)
            throw new ArgumentException(
                "Una obligación sin diario no tiene estado que proyectar.", nameof(diario));

        return obligacion;
    }

    /// <summary>
    /// `R-01` — nominar. <b>El acto que `RN-74` mantiene fuera del campo y fuera de la
    /// liquidación</b>: quien liquida constata el hueco; quien nomina a una persona responsable
    /// de él es otro, y lo hace con su competencia registrada.
    /// </summary>
    /// <param name="causa">
    /// Sólo las que `RN-86` declara generadoras. La validación de que la tipificación del
    /// faltante corresponda a una de éstas vive en <see cref="ReglasDelReintegro"/>.
    /// </param>
    public static ObligacionDeReintegro Nominar(
        Ulid id,
        DireccionDelReintegro direccion,
        CausaDelReintegro causa,
        Ulid responsable,
        decimal monto,
        Ulid? mision,
        Ulid? asignacion,
        DateOnly fechaDelHecho,
        Autoria nomina,
        string motivo,
        DateTimeOffset momento)
    {
        if (monto <= 0)
            throw new BloqueoDuro("RN-86",
                "Una obligación de reintegro en cero no obliga a nada. Si no hay monto, lo que " +
                "hay es una diferencia explicada, y ésa se cierra en la liquidación.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("RN-86",
                "Nominar a una persona como responsable de dinero público exige motivo escrito. " +
                "Es lo que después se le notifica y lo que su descargo contesta.");

        // La dirección y la causa tienen que ser coherentes: el peculio propio es lo único que
        // va a favor del servidor, y un faltante nunca va a su favor. Cruzarlas produciría una
        // obligación que dice que la institución le debe al motorista el dinero que él perdió.
        ReglasDelReintegro.ExigirCausaCoherenteConLaDireccion(direccion, causa);

        var obligacion = new ObligacionDeReintegro(
            id, direccion, causa, responsable, monto, mision, asignacion, fechaDelHecho);

        obligacion._diario.Add(new MovimientoDeObligacion(
            "R-01", EstadoDeObligacion.Determinada, nomina, momento,
            $"{Texto(direccion)} por {monto:N2}, causa {causa}, hecho del " +
            $"{fechaDelHecho:dd/MM/yyyy}. {motivo.Trim()}"));

        return obligacion;
    }

    /// <summary>
    /// `R-02` — notificar al servidor. <b>Sin esto no hay descargo que exigirle</b>: `RN-86`
    /// punto 5 lista la notificación antes del descargo, y resolver contra quien nunca supo
    /// que se le imputaba algo es resolver sin oírlo.
    /// </summary>
    public void Notificar(Autoria notifica, string constancia, DateTimeOffset momento)
    {
        ExigirEstado("R-02", EstadoDeObligacion.Determinada);

        if (string.IsNullOrWhiteSpace(constancia))
            throw new BloqueoDuro("RN-86",
                "La notificación exige constancia. Una notificación declarada y no constatada " +
                "no habilita a resolver: deja al servidor sin la oportunidad de descargarse y " +
                "al expediente sin cómo demostrar que la tuvo.");

        _diario.Add(new MovimientoDeObligacion(
            "R-02", EstadoDeObligacion.Notificada, notifica, momento, constancia.Trim()));
    }

    /// <summary>
    /// `R-03` — el descargo del servidor. Lo registra quien lo recibe; el contenido es de él.
    /// </summary>
    public void RegistrarDescargo(Autoria recibe, string descargo, DateTimeOffset momento)
    {
        ExigirEstado("R-03", EstadoDeObligacion.Notificada);

        if (string.IsNullOrWhiteSpace(descargo))
            throw new BloqueoDuro("RN-86",
                "Un descargo vacío no es un descargo. Si el servidor no presentó ninguno, eso " +
                "se dice al resolver — no se registra como si hubiera hablado.");

        _diario.Add(new MovimientoDeObligacion(
            "R-03", EstadoDeObligacion.ConDescargo, recibe, momento, descargo.Trim()));
    }

    /// <summary>
    /// `R-04` — resolver confirmando. La obligación sigue debiéndose y el cobro es de
    /// Administración y Talento Humano, fuera de SIGTI (`HU-078`, fuera de alcance).
    ///
    /// <b>Se admite resolver sin descargo</b>, y el asiento lo dice: hay servidores que no
    /// contestan, y un expediente que no puede avanzar sin su firma es un expediente que él
    /// cierra con su silencio.
    /// </summary>
    public void Resolver(Autoria resuelve, string resolucion, DateTimeOffset momento)
    {
        ExigirAlgunEstado("R-04", EstadoDeObligacion.Notificada, EstadoDeObligacion.ConDescargo);

        if (string.IsNullOrWhiteSpace(resolucion))
            throw new BloqueoDuro("RN-86", "La resolución exige fundamento escrito.");

        var sinDescargo = Estado is EstadoDeObligacion.Notificada;

        _diario.Add(new MovimientoDeObligacion(
            "R-04", EstadoDeObligacion.Resuelta, resuelve, momento,
            (sinDescargo ? "RESUELTA SIN DESCARGO DEL SERVIDOR. " : "") + resolucion.Trim()));
    }

    /// <summary>
    /// `R-05` — dejar sin efecto. La resolución acogió el descargo, o la determinación estaba
    /// errada.
    ///
    /// <b>No borra nada.</b> P-3: los dos asientos quedan en el diario para siempre, y es lo
    /// único que le deja constancia al servidor de que se le imputó y de que no procedía.
    /// </summary>
    public void DejarSinEfecto(Autoria resuelve, string fundamento, DateTimeOffset momento)
    {
        ExigirAlgunEstado("R-05",
            EstadoDeObligacion.Determinada, EstadoDeObligacion.Notificada,
            EstadoDeObligacion.ConDescargo, EstadoDeObligacion.Resuelta);

        if (string.IsNullOrWhiteSpace(fundamento))
            throw new BloqueoDuro("RN-86",
                "Dejar sin efecto una obligación exige fundamento escrito. Sin él, la salida " +
                "más cómoda de todo faltante sería borrarlo.");

        _diario.Add(new MovimientoDeObligacion(
            "R-05", EstadoDeObligacion.DejadaSinEfecto, resuelve, momento,
            $"Sin efecto sobre {Monto:N2}" +
            (Pagado > 0 ? $", con {Pagado:N2} ya abonados que quedan a devolver" : "") +
            $". {fundamento.Trim()}"));
    }

    /// <summary>
    /// `R-06` — registrar el pago, con su asiento reverso.
    ///
    /// ── Se puede pagar en cualquier momento, a propósito ────────────────────
    /// `CE-26` describe la práctica que hay que impedir: <i>«se le da tiempo al motorista para
    /// que lo reponga; si repone, no queda registro de que hubo faltante»</i>. Exigir que la
    /// obligación esté resuelta para admitir el pago empujaría a no nominarla hasta ver si
    /// paga — que es exactamente el mismo agujero con otro nombre.
    ///
    /// ── Y el pago parcial es válido tal cual ────────────────────────────────
    /// `CE-26`: <i>«El sistema nunca redondea ni ajusta para cuadrar»</i>. Un abono que no
    /// cubre <b>no avanza el ciclo</b>: el asiento queda, el saldo baja, y el estado sigue
    /// donde estaba porque la obligación sigue abierta.
    /// </summary>
    /// <param name="fechaDelHecho">
    /// <b>Cuándo entró el dinero a la caja</b>, no cuándo se capturó. `RN-86` punto 1 y
    /// `CE-26` §5: capturarla distinto para que el plazo no aparezca vencido es falsificar un
    /// dato.
    /// </param>
    public void RegistrarPago(
        Autoria recibe, decimal monto, DateOnly fechaDelHecho, string acta, DateTimeOffset momento)
    {
        if (!EstaAbierta)
            throw new TransicionInvalidaDeObligacion("R-06", Estado, EstadoDeObligacion.Resuelta);

        if (monto <= 0)
            throw new BloqueoDuro("RN-86", "Un pago de cero no abona nada.");

        if (monto > Saldo)
            throw new BloqueoDuro("RN-86",
                $"Se abonan {monto:N2} sobre un saldo de {Saldo:N2}. Cobrar de más no es un " +
                "reintegro: si la institución recibió un excedente, es otro hecho económico y " +
                "tiene su propio asiento.");

        if (string.IsNullOrWhiteSpace(acta))
            throw new BloqueoDuro("RN-86",
                "El reintegro se registra con acta: monto, fecha del hecho, quién pagó y quién " +
                "recibió. Un pago sin constancia no descarga a nadie.");

        var cubre = Pagado + monto >= Monto;

        _diario.Add(new MovimientoDeObligacion(
            "R-06",
            // El destino se calcula en el acto, y por eso P-1 sigue intacto: un abono parcial
            // deja el estado donde estaba porque la obligación efectivamente sigue ahí.
            cubre ? EstadoDeObligacion.Saldada : Estado,
            recibe, momento,
            $"Reintegro de {monto:N2} el {fechaDelHecho:dd/MM/yyyy}" +
            (cubre
                ? ". SALDADA — asiento reverso sobre los acumulados del período en que se registra."
                : $", ABONO PARCIAL: quedan {Monto - Pagado - monto:N2}.") +
            $" {acta.Trim()}",
            Pagado: monto));
    }

    private static string Texto(DireccionDelReintegro direccion) =>
        direccion is DireccionDelReintegro.AFavorDelServidor
            ? "Obligación a favor del servidor"
            : "Obligación a cargo del servidor";

    private void ExigirEstado(string transicion, EstadoDeObligacion requerido)
    {
        if (Estado != requerido)
            throw new TransicionInvalidaDeObligacion(transicion, Estado, requerido);
    }

    private void ExigirAlgunEstado(string transicion, params EstadoDeObligacion[] admitidos)
    {
        if (!admitidos.Contains(Estado))
            throw new TransicionInvalidaDeObligacion(transicion, Estado, admitidos[0]);
    }
}

/// <summary>Se intentó una transición de la obligación desde un estado que no la admite.</summary>
public sealed class TransicionInvalidaDeObligacion(
    string transicion, EstadoDeObligacion estadoActual, EstadoDeObligacion estadoRequerido)
    : Exception($"La transición {transicion} exige la obligación en {estadoRequerido}, y está en {estadoActual}.")
{
    public string Transicion { get; } = transicion;
    public EstadoDeObligacion EstadoActual { get; } = estadoActual;
    public EstadoDeObligacion EstadoRequerido { get; } = estadoRequerido;
}
