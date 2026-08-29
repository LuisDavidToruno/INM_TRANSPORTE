using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// El vale — la porción de fondo entregada a una misión. Máquina §10.1, artefacto autoridad.
///
/// <b>P-1.</b> El estado es la proyección del diario. Y acá importa más que en ningún otro
/// lado: `V-04` <b>se ejecuta sin conectividad</b>, contra el reloj del dispositivo, y llega
/// al servidor días después. Dos dispositivos no negocian «el estado» del vale: intercambian
/// asientos.
///
/// ── Por qué el consumo va como asiento y no como total ──────────────────────
/// Porque el motorista carga <b>varias veces</b> en una misión, cada una con su odómetro. Un
/// campo <c>galones_consumidos</c> contesta cuánto y pierde <b>dónde</b>, que es justo lo que
/// `RN-30` necesita para decir si la diferencia está en un tramo o repartida en todos.
///
/// ── Lo que hace que `EMITIDA` no sea una formalidad ─────────────────────────
/// `RN-27`: sin constancia de recepción la asignación <i>«no se considera consumible ni
/// liquidable»</i>. Emitida es un papel con folio que <b>no salió de la custodia de ACT-07</b>;
/// entregada es dinero público fuera de la caja. Colapsar los dos estados borraría la única
/// pregunta que la auditoría hace primero: ¿quién lo tenía cuando desapareció?
/// </summary>
public sealed class AsignacionDeCombustible
{
    private readonly List<TransicionDeAsignacion> _diario = [];

    private AsignacionDeCombustible(
        Ulid id, string folio, Ulid fondo, Ulid mision, Ulid vehiculo,
        Ulid receptor, decimal monto, decimal? galones, string instrumento, string tipoDeCombustible)
    {
        Id = id;
        Folio = folio;
        Fondo = fondo;
        Mision = mision;
        Vehiculo = vehiculo;
        Receptor = receptor;
        Monto = monto;
        Galones = galones;
        Instrumento = instrumento;
        TipoDeCombustible = tipoDeCombustible;
    }

    public Ulid Id { get; }

    /// <summary>
    /// `RN-27` requisito 1 — <b>único en la institución y no reciclable</b>. Sale del rango de
    /// la delegación (`RN-44`), que es lo que permite emitirlo sin conectividad.
    /// </summary>
    public string Folio { get; }

    public Ulid Fondo { get; }

    public Ulid Mision { get; }

    /// <summary>El vehículo de la orden. Se congela acá: es contra esto que `RN-32` compara.</summary>
    public Ulid Vehiculo { get; }

    /// <summary>
    /// Quién recibe — el <b>motorista</b> de la orden, por el ULID de su registro en el padrón.
    ///
    /// No es un <c>IdPersona</c>, y la distinción no es cosmética: `IdPersona` es la identidad
    /// con que se juzga la <b>segregación de funciones</b>, y `RN-57` admite que conduzca quien
    /// no está en el padrón. Mezclarlos obligaría a convertir un ULID en identidad de persona,
    /// y esa conversión no coincide con ninguna persona real — dejando a `RN-32` comparando dos
    /// cosas que no se pueden comparar.
    /// </summary>
    public Ulid Receptor { get; }

    public decimal Monto { get; }

    /// <summary>
    /// Nulo cuando el instrumento es en dinero y no en galones. <b>`RN-27` admite monto y/o
    /// galones</b>: exigir los dos obligaría a inventar una conversión con un precio que el
    /// sistema no conoce.
    /// </summary>
    public decimal? Galones { get; }

    /// <summary>Efectivo, vale, cupón u orden de pago. Cada uno concilia distinto.</summary>
    public string Instrumento { get; }

    public string TipoDeCombustible { get; }

    public IReadOnlyList<TransicionDeAsignacion> Diario => _diario;

    public EstadoDeAsignacion Estado => _diario[^1].Destino;

    /// <summary>
    /// Lo consumido, sumando asientos `V-04`. <b>Nunca un campo</b>: es lo que permite auditar
    /// carga por carga.
    /// </summary>
    public decimal Consumido => _diario.Sum(t => t.Consumo?.Monto ?? 0m);

    public decimal GalonesConsumidos => _diario.Sum(t => t.Consumo?.Galones ?? 0m);

    /// <summary>Lo devuelto, sumando `V-05` y el saldo devuelto al liquidar.</summary>
    public decimal Devuelto => _diario.Sum(t => t.Devuelto ?? 0m);

    /// <summary>
    /// <b>Hubo movimiento de dinero.</b> Es la pregunta que decide entre `T-15` y `T-16` en la
    /// misión: §10.1 — <i>«si hubo cualquier consumo, la asignación no puede ir a `DEVUELTA` y
    /// la misión toma el camino `T-16`»</i>.
    /// </summary>
    public bool TuvoConsumo => _diario.Any(t => t.Consumo is not null);

    /// <summary>
    /// Ya no cuenta contra el saldo ni impide cerrar el fondo. <b>`ANULADA` cuenta como
    /// resuelta</b> (`RN-26`: liquidadas <i>«o formalmente anuladas»</i>), `DEVUELTA` también:
    /// el dinero volvió íntegro.
    /// </summary>
    public bool EstaResuelta => Estado
        is EstadoDeAsignacion.Liquidada
        or EstadoDeAsignacion.Conciliada
        or EstadoDeAsignacion.ConciliadaConDesviacion
        or EstadoDeAsignacion.Anulada
        or EstadoDeAsignacion.Devuelta;

    /// <summary>Quién ejecutó un acto dado, para la segregación de `BD-06`.</summary>
    public IdPersona? QuienHizo(string transicion) =>
        _diario.FirstOrDefault(t => t.Id == transicion)?.Ejecuta;

    private Dictionary<string, IdPersona> ActosPrevios()
    {
        var actos = new Dictionary<string, IdPersona>();

        void Anotar(string v, string verbo)
        {
            if (QuienHizo(v) is { } quien && !actos.ContainsKey(verbo))
                actos[verbo] = quien;
        }

        Anotar("V-01", "emitió");
        Anotar("V-02", "entregó");
        Anotar("V-04", "consumió de");
        Anotar("V-07", "liquidó");
        Anotar("V-08", "liquidó");

        return actos;
    }

    /// <summary>Rehidrata el vale desde su diario. El estado es la proyección (P-1).</summary>
    public static AsignacionDeCombustible Reconstruir(
        Ulid id,
        string folio,
        Ulid fondo,
        Ulid mision,
        Ulid vehiculo,
        Ulid receptor,
        decimal monto,
        decimal? galones,
        string instrumento,
        string tipoDeCombustible,
        IEnumerable<TransicionDeAsignacion> diario)
    {
        var asignacion = new AsignacionDeCombustible(
            id, folio, fondo, mision, vehiculo, receptor, monto, galones, instrumento,
            tipoDeCombustible);

        asignacion._diario.AddRange(diario);

        if (asignacion._diario.Count == 0)
            throw new ArgumentException(
                "Una asignación sin diario no tiene estado que proyectar.", nameof(diario));

        return asignacion;
    }

    /// <summary>
    /// `V-01` — emitir con folio.
    ///
    /// Las comprobaciones de `RN-32` —estado mínimo, vehículo, motorista, tipo de combustible—
    /// y la de saldo de `RN-26` se hacen <b>acá</b>, no en el servicio: quien construya otra
    /// puerta de emisión las hereda sin tener que acordarse de llamarlas.
    /// </summary>
    public static AsignacionDeCombustible Emitir(
        Ulid id,
        string folio,
        Ulid fondo,
        Ulid mision,
        EstadoDeMision estadoDeLaMision,
        EstadoDeMision estadoMinimoConfigurado,
        Ulid vehiculoDeLaOrden,
        Ulid motoristaDeLaOrden,
        Ulid vehiculoReceptor,
        // **Quién está de verdad en la ventanilla.** Se recibe y no se asume: si quien llama
        // pasara el mismo valor a los dos lados, `RN-32` compararía algo consigo mismo y el
        // bloqueo no podría disparar nunca.
        Ulid motoristaReceptor,
        string? combustibleDelVehiculo,
        string tipoDeCombustible,
        decimal monto,
        decimal? galones,
        string instrumento,
        IdPersona emite,
        decimal saldoDisponible,
        decimal toleranciaSobregiro,
        DateTimeOffset momento,
        // ── El bloqueo de `RN-86`, y por qué entra por acá ──────────────────────
        // Igual que `RN-32` y `RN-26`: acá, no en el servicio. Quien construya otra puerta
        // de emisión lo hereda sin tener que acordarse de llamarlo. `HU-078` describe el
        // agujero que cierra: hoy nada impide seguir entregándole fondo a quien no devolvió
        // el anterior, y el saldo se acumula sobre unas pocas personas hasta que alguien
        // hace el arqueo del período, meses después.
        //
        // Listas vacías es el caso normal y no una omisión: la inmensa mayoría de los
        // motoristas no debe nada. Lo que no puede pasar es que quien llama decida no
        // consultarlas — por eso el servicio las arma siempre.
        string nombreDelReceptor,
        IReadOnlyList<ObligacionDeReintegro> obligacionesDelReceptor,
        IReadOnlyList<SaldoAfuera> saldosDelReceptor,
        LevantamientoDeBloqueo? levantamiento = null)
    {
        if (string.IsNullOrWhiteSpace(folio))
            throw new BloqueoDuro("RN-27",
                "Una asignación sin folio no existe para la auditoría. El folio es lo que " +
                "contesta de qué fondo salió este galón, quién lo recibió y a qué misión sirvió.");

        if (monto <= 0)
            throw new BloqueoDuro("RN-27", "Una asignación en cero no asigna nada.");

        ReglasDeEmisionDeCombustible.ExigirEstadoMinimo(estadoDeLaMision, estadoMinimoConfigurado);
        ReglasDeEmisionDeCombustible.ExigirReceptorDeLaOrden(
            vehiculoDeLaOrden, vehiculoReceptor, motoristaDeLaOrden, motoristaReceptor);
        ReglasDeEmisionDeCombustible.ExigirCombustibleCompatible(
            combustibleDelVehiculo, tipoDeCombustible);
        ReglasDelFondo.ExigirSaldoSuficiente(saldoDisponible, monto, toleranciaSobregiro);

        // **Se juzga a la fecha del hecho** (P-4): un vale emitido con fecha de la semana
        // pasada se evalúa contra los plazos que estaban vencidos entonces, no contra los
        // de hoy. Capturarlo tarde no puede cambiar si el bloqueo correspondía.
        ReglasDelReintegro.ExigirQueNoDebaReintegro(
            nombreDelReceptor, mision, obligacionesDelReceptor, saldosDelReceptor,
            DateOnly.FromDateTime(momento.Date), levantamiento);

        var asignacion = new AsignacionDeCombustible(
            id, folio, fondo, mision, vehiculoReceptor, motoristaReceptor,
            monto, galones, instrumento, tipoDeCombustible);

        asignacion._diario.Add(new TransicionDeAsignacion(
            "V-01", EstadoDeAsignacion.Emitida, emite, momento,
            $"Folio {folio}, {monto:N2} en {instrumento}" +
            (galones is { } g ? $" ({g:N2} galones de {tipoDeCombustible})." : $" de {tipoDeCombustible}.")));

        return asignacion;
    }

    /// <summary>
    /// `V-02` — entregar contra firma. <b>Ocurre dentro de `T-12` despachar</b>, nunca antes:
    /// §10.1 y `EF-04` son taxativos, y `PROGRAMADA` lista expresamente <i>«Entregar fondo de
    /// combustible»</i> entre lo que no se puede.
    ///
    /// Que este método exista suelto no abre esa puerta: el servicio lo llama <b>desde</b> el
    /// despacho. Lo que este objeto impone es lo suyo — que no se entregue dos veces, y que
    /// quien entrega no sea quien emitió.
    /// </summary>
    public void Entregar(IdPersona entrega, string constanciaDeRecepcion, DateTimeOffset momento)
    {
        ExigirEstado("V-02", EstadoDeAsignacion.Emitida);
        ReglasDeEmisionDeCombustible.ExigirActorDistinto("entregarla", entrega, ActosPrevios());

        if (string.IsNullOrWhiteSpace(constanciaDeRecepcion))
            throw new BloqueoDuro("RN-27",
                "Sin constancia de recepción la asignación no es entregable: `RN-27` la deja " +
                "«emitida no entregada», y en ese estado no se considera consumible ni liquidable.");

        // El receptor NO se recibe como parámetro: es el de la emisión, ya validado contra la
        // orden por `RN-32`. Volver a pedirlo acá abriría la puerta a entregarle a otro.
        _diario.Add(new TransicionDeAsignacion(
            "V-02", EstadoDeAsignacion.Entregada, entrega, momento,
            $"Recibe {Receptor}. Constancia: {constanciaDeRecepcion}"));
    }

    /// <summary>
    /// `V-03` — anular antes de entregar. El folio queda anulado y <b>no se recicla</b>.
    ///
    /// El valor retorna al fondo <b>porque no fue canjeado</b> (`RN-27` punto 4). Después de
    /// entregado ya no cabe: ahí el camino es la devolución con acta, o el extravío.
    /// </summary>
    public void Anular(IdPersona ejecuta, string motivo, DateTimeOffset momento)
    {
        ExigirEstado("V-03", EstadoDeAsignacion.Emitida);

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("RN-27", "La anulación de un folio exige motivo y acta.");

        _diario.Add(new TransicionDeAsignacion(
            "V-03", EstadoDeAsignacion.Anulada, ejecuta, momento, motivo, Devuelto: Monto));
    }

    /// <summary>
    /// `V-04` — registrar consumo. <b>Sólo mientras la misión está `EN_RUTA`</b>, y se ejecuta
    /// sin conectividad.
    ///
    /// ── Se admiten varios, y el estado no cambia después del primero ────────
    /// Consumir dos veces es lo normal: se carga a la ida y a la vuelta. `CONSUMIDA` significa
    /// «ya se tocó», no «se acabó» — §10.1 lo dice: <i>«Puede ser consumo parcial»</i>.
    /// </summary>
    public void RegistrarConsumo(
        IdPersona consume, ConsumoRegistrado consumo, DateTimeOffset momento, Ulid? idDeCaptura = null)
    {
        if (Estado is not (EstadoDeAsignacion.Entregada or EstadoDeAsignacion.Consumida))
            throw new TransicionInvalidaDeAsignacion("V-04", Estado, EstadoDeAsignacion.Entregada);

        ReglasDeEmisionDeCombustible.ExigirActorDistinto(
            "consumirla", consume, SoloDe("emitió", "entregó"));

        if (consumo.Galones <= 0)
            throw new BloqueoDuro("RN-83", "Un consumo de cero galones no es un abastecimiento.");

        if (consumo.Odometro <= 0)
            throw new BloqueoDuro("RN-83",
                "El consumo exige el odómetro del momento. Sin él el galón no queda anclado a " +
                "ningún tramo, y la conciliación sólo puede comparar un total contra otro total.");

        // `RN-85`: la ausencia de comprobante se registra **con causa**. Es lo único que
        // distingue «la estación no dio factura» de un campo que nadie llenó, y esa
        // diferencia decide si el descargo alternativo procede.
        if (consumo.Comprobante is null &&
            string.IsNullOrWhiteSpace(consumo.CausaSinComprobante))
            throw new BloqueoDuro("RN-85",
                "Un consumo sin comprobante exige causa tipificada. El registro del " +
                "abastecimiento no se omite nunca por falta de papel, pero tampoco se disimula.");

        _diario.Add(new TransicionDeAsignacion(
            "V-04", EstadoDeAsignacion.Consumida, consume, momento,
            $"{consumo.Galones:N2} galones por {consumo.Monto:N2} en {consumo.Estacion}, " +
            $"odómetro {consumo.Odometro:N0}" +
            (consumo.Comprobante is null
                ? $". SIN COMPROBANTE (`RN-85`), causa declarada: {consumo.CausaSinComprobante}"
                : $", comprobante {consumo.Comprobante}."),
            IdDeCaptura: idDeCaptura,
            Consumo: consumo));
    }

    /// <summary>
    /// `V-05` — devolver íntegra.
    ///
    /// <b>Si hubo cualquier consumo, esta puerta no existe.</b> §10.1 es explícito, y el
    /// motivo es económico, no formal: devolver «íntegro» algo ya tocado sería declarar que
    /// volvió un dinero que no volvió.
    /// </summary>
    public void DevolverIntegra(IdPersona ejecuta, string acta, DateTimeOffset momento)
    {
        ExigirEstado("V-05", EstadoDeAsignacion.Entregada);

        if (TuvoConsumo)
            throw new BloqueoDuro("RN-27",
                $"Esta asignación ya tuvo consumo por {Consumido:N2}. La devolución íntegra no " +
                "aplica: el camino es liquidarla por lo consumido, y la misión va por `T-16`.");

        if (string.IsNullOrWhiteSpace(acta))
            throw new BloqueoDuro("RN-27",
                "La devolución exige acta firmada por quien entregó y por quien devuelve. Una " +
                "devolución declarada y no constatada no libera saldo del fondo.");

        _diario.Add(new TransicionDeAsignacion(
            "V-05", EstadoDeAsignacion.Devuelta, ejecuta, momento, acta, Devuelto: Monto));
    }

    /// <summary>
    /// `V-06` — declarar extravío con acta.
    ///
    /// `[C]` <b>Si la institución exige denuncia no está confirmado</b> — insumo #1. Se registra
    /// igual: el extravío no declarado es un vale que sigue figurando entregado y que puede
    /// aparecer canjeado en la factura del proveedor. <b>Esa contradicción es exactamente lo
    /// que el circuito de folios existe para descubrir</b>, y sólo se descubre si el extravío
    /// consta.
    /// </summary>
    public void DeclararExtravio(IdPersona ejecuta, string acta, DateTimeOffset momento)
    {
        ExigirEstado("V-06", EstadoDeAsignacion.Entregada);

        if (string.IsNullOrWhiteSpace(acta))
            throw new BloqueoDuro("RN-27", "El extravío exige acta con motivo y responsable.");

        _diario.Add(new TransicionDeAsignacion(
            "V-06", EstadoDeAsignacion.Extraviada, ejecuta, momento, acta));
    }

    /// <summary>
    /// `V-07` — liquidar con comprobantes.
    ///
    /// Cuadra asignado, consumido y devuelto. El <b>saldo devuelto entra como dato</b> y no
    /// sale de una resta implícita: `RN-26` sólo libera saldo del fondo con devolución
    /// constatada, y una resta no constata nada.
    /// </summary>
    public void Liquidar(IdPersona liquida, decimal saldoDevuelto, string? observacion, DateTimeOffset momento)
    {
        ExigirEstado("V-07", EstadoDeAsignacion.Consumida);
        ReglasDeEmisionDeCombustible.ExigirActorDistinto("liquidarla", liquida, ActosPrevios());

        if (saldoDevuelto < 0)
            throw new BloqueoDuro("RN-29", "Un saldo devuelto negativo no es una devolución.");

        var diferencia = Monto - Consumido - saldoDevuelto;

        _diario.Add(new TransicionDeAsignacion(
            "V-07", EstadoDeAsignacion.Liquidada, liquida, momento,
            $"Asignado {Monto:N2}, consumido {Consumido:N2}, devuelto {saldoDevuelto:N2}." +
            // La diferencia se NOMBRA aunque sea cero. Callarla cuando cuadra y decirla cuando
            // no, entrena a leer su ausencia como «no se calculó».
            (diferencia == 0
                ? " Cuadra exacto."
                : $" DIFERENCIA SIN EXPLICAR de {diferencia:N2} — `H-11`.") +
            (observacion is null ? "" : $" {observacion}"),
            Devuelto: saldoDevuelto));
    }

    /// <summary>
    /// `V-08` — liquidar con acta de extravío. El instrumento se pierde, <b>el descargo no</b>.
    /// </summary>
    public void LiquidarPorExtravio(IdPersona liquida, string acta, DateTimeOffset momento)
    {
        ExigirEstado("V-08", EstadoDeAsignacion.Extraviada);
        ReglasDeEmisionDeCombustible.ExigirActorDistinto("liquidarla", liquida, ActosPrevios());

        _diario.Add(new TransicionDeAsignacion(
            "V-08", EstadoDeAsignacion.Liquidada, liquida, momento,
            $"Liquidada por extravío, sin comprobantes de consumo. {acta}"));
    }

    /// <summary>
    /// `V-09` y `V-10` — conciliar contra kilometraje, con el dictamen que calculó `RN-30`.
    ///
    /// ── Quien concilia NO elige el resultado ────────────────────────────────
    /// Es el mismo invariante que §7.2 impone al cierre: <i>«quien cierra no elige entre
    /// cerrar limpio o con hallazgo, el criterio decide y él lo confirma»</i>. Si quien
    /// concilia pudiera declarar «dentro de umbral» sobre un rendimiento imposible, en seis
    /// meses no habría una sola desviación y el estado dejaría de significar algo.
    ///
    /// Por eso la firma recibe la <see cref="Conciliacion"/> ya calculada y no un booleano:
    /// el booleano era una decisión de quien llamaba.
    ///
    /// ── Y la causa sigue siendo del analista ────────────────────────────────
    /// El sistema dice <b>qué</b> se desvió; por qué se desvió lo declara quien concilia, y
    /// `INV-35` lo exige para poder cerrar.
    /// </summary>
    /// <param name="causa">
    /// Obligatoria cuando hay hallazgo. En los demás dictámenes es opcional: no se le puede
    /// pedir a nadie que explique una desviación que no hubo.
    /// </param>
    public void Conciliar(
        IdPersona concilia, Conciliacion resultado, string? causa, DateTimeOffset momento)
    {
        var transicion = resultado.EsHallazgo ? "V-10" : "V-09";

        ExigirEstado(transicion, EstadoDeAsignacion.Liquidada);
        ReglasDeEmisionDeCombustible.ExigirActorDistinto("conciliarla", concilia, ActosPrevios());

        if (resultado.EsHallazgo && string.IsNullOrWhiteSpace(causa))
            throw new BloqueoDuro("RN-30",
                "Una desviación fuera de umbral exige causa tipificada. Sin ella no se puede " +
                "cerrar la misión (`INV-35`).");

        _diario.Add(new TransicionDeAsignacion(
            transicion,
            resultado.EsHallazgo
                ? EstadoDeAsignacion.ConciliadaConDesviacion
                : EstadoDeAsignacion.Conciliada,
            concilia, momento,
            // La evidencia entera va al asiento: una conciliación que no dice contra qué se
            // juzgó no se puede rehacer, y el dictamen sin sus cuentas es una opinión.
            $"{resultado.Dictamen}: {resultado.Evidencia}" +
            (causa is null ? "" : $" · causa declarada: {causa.Trim()}")));
    }

    private Dictionary<string, IdPersona> SoloDe(params string[] verbos)
    {
        var todos = ActosPrevios();
        return verbos
            .Where(todos.ContainsKey)
            .ToDictionary(v => v, v => todos[v]);
    }

    private void ExigirEstado(string transicion, EstadoDeAsignacion requerido)
    {
        if (Estado != requerido)
            throw new TransicionInvalidaDeAsignacion(transicion, Estado, requerido);
    }
}

/// <summary>Se intentó una transición del vale desde un estado que no la admite.</summary>
public sealed class TransicionInvalidaDeAsignacion(
    string transicion, EstadoDeAsignacion estadoActual, EstadoDeAsignacion estadoRequerido)
    : Exception($"La transición {transicion} exige la asignación en {estadoRequerido}, y está en {estadoActual}.")
{
    public string Transicion { get; } = transicion;
    public EstadoDeAsignacion EstadoActual { get; } = estadoActual;
    public EstadoDeAsignacion EstadoRequerido { get; } = estadoRequerido;
}
