using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Cómo terminó el expediente — `RN-93` punto 6: <b>no se cierra sin resolución</b>.
/// </summary>
public enum ResolucionDelHallazgo
{
    /// <summary>
    /// Se corrigió el efecto económico con uno o varios asientos reversos. <b>Exige que los
    /// haya</b>: declarar esto sin reverso diría que se corrigió algo que nadie tocó.
    /// </summary>
    ConAsientoReverso,

    /// <summary>
    /// El hallazgo era real y <b>no tiene efecto en dinero</b>. Es el caso del vehículo que
    /// circuló sin orden: <i>«la ausencia de misión es el hallazgo»</i> (`RN-59`), y no hay monto
    /// que revertir.
    /// </summary>
    SinEfectoEconomico,

    /// <summary>
    /// <b>Era un error del propio descubridor.</b> `RN-93` casos límite: <i>«el expediente se
    /// resuelve como sin efecto, con su fundamento. <b>Se cierra, no se borra</b>»</i>.
    ///
    /// Borrarlo dejaría a quien fue señalado sin constancia de que se le señaló y de que no
    /// procedía — y al descubridor sin registro de haberse equivocado.
    /// </summary>
    SinEfecto,
}

/// <summary>
/// Un asiento del diario del expediente. Mismo P-1: el estado es la proyección del diario.
/// </summary>
/// <param name="Id">`H-01` abrir · `H-02` vincular · `H-03` revertir · `H-04` resolver.</param>
/// <param name="Autor">Quién y con qué competencia. Va congelado (`Autoria`).</param>
public sealed record MovimientoDelHallazgo(
    string Id,
    Autoria Autor,
    DateTimeOffset Momento,
    string Motivo,
    Ulid? Reverso = null);

/// <summary>
/// El expediente de hallazgo posterior — `RN-93`.
///
/// ── Por qué existe, con las palabras de la regla ────────────────────────────
/// <i>«Basta con que la reapertura de un expediente cerrado exista para que se use, y basta con
/// que se use una vez para que <b>ningún reporte histórico vuelva a ser reproducible</b>. El
/// expediente de hallazgo posterior es la salida que permite corregir el efecto económico sin
/// destruir la reproducibilidad»</i>.
///
/// ── Y lo que nunca hace ─────────────────────────────────────────────────────
/// <b>Ni su apertura ni su resolución alteran el estado ni los datos del objeto vinculado.</b>
/// Una Orden de Misión `CERRADA` no se reabre, ni por auditoría. Lo que se entrega a quien la
/// pide es el paquete sellado tal como cerró <b>más</b> este expediente: es más información, no
/// menos.
/// </summary>
public sealed class ExpedienteDeHallazgoPosterior
{
    private readonly List<MovimientoDelHallazgo> _diario = [];
    private readonly List<Ulid> _misiones = [];
    private readonly List<AsientoReverso> _reversos = [];

    private ExpedienteDeHallazgoPosterior(
        Ulid id, string tipo, DateOnly fechaDelHecho, DateOnly fechaDelDescubrimiento,
        string comoSeDescubrio, string fuente, string? documentoAdjunto,
        Ulid? vehiculo, Ulid? motorista, string? periodo)
    {
        Id = id;
        Tipo = tipo;
        FechaDelHecho = fechaDelHecho;
        FechaDelDescubrimiento = fechaDelDescubrimiento;
        ComoSeDescubrio = comoSeDescubrio;
        Fuente = fuente;
        DocumentoAdjunto = documentoAdjunto;
        Vehiculo = vehiculo;
        Motorista = motorista;
        Periodo = periodo;
    }

    public Ulid Id { get; }

    /// <summary>
    /// Del catálogo `tipo_de_hallazgo_posterior`, configurable. <b>Tipificado y no libre</b>: un
    /// motivo libre no se agrupa, y lo que no se agrupa no produce indicador.
    /// </summary>
    public string Tipo { get; }

    /// <summary>
    /// <b>Cuándo ocurrió el hecho.</b> `RN-93`: la antigüedad se cuenta desde acá y no desde el
    /// descubrimiento — <i>«evita el incentivo perverso más obvio: descubrir tarde para que el
    /// indicador se vea mejor»</i>.
    /// </summary>
    public DateOnly FechaDelHecho { get; }

    /// <summary>
    /// Cuándo se descubrió. <b>Campo distinto del anterior, y ambos obligatorios</b> — la regla
    /// lo dice con esas palabras.
    /// </summary>
    public DateOnly FechaDelDescubrimiento { get; }

    /// <summary>Cómo se descubrió — `RN-93` punto 1. Una conciliación, una revisión, un aviso.</summary>
    public string ComoSeDescubrio { get; }

    /// <summary>Contra qué fuente. `RN-95` la nombra: el estado de cuenta, el acta, la notificación.</summary>
    public string Fuente { get; }

    public string? DocumentoAdjunto { get; }

    /// <summary>
    /// Las misiones vinculadas. <b>Cero, una o varias</b> — `RN-93`.
    ///
    /// Cero es un caso previsto y frecuente: el paso por caseta de un domingo, el consumo de un
    /// vehículo que ese día no tenía orden. <b>La ausencia de misión es el hallazgo</b>.
    /// </summary>
    public IReadOnlyList<Ulid> Misiones => _misiones;

    public Ulid? Vehiculo { get; }

    public Ulid? Motorista { get; }

    /// <summary>El período, cuando el hallazgo no se ata a un objeto concreto.</summary>
    public string? Periodo { get; }

    public IReadOnlyList<MovimientoDelHallazgo> Diario => _diario;

    /// <summary>Un asiento por cada efecto económico — `RN-93` casos límite.</summary>
    public IReadOnlyList<AsientoReverso> Reversos => _reversos;

    public ResolucionDelHallazgo? Resolucion { get; private set; }

    public string? Fundamento { get; private set; }

    public bool EstaAbierto => Resolucion is null;

    /// <summary>
    /// La antigüedad, <b>desde el hecho</b>. Es la que `RN-97` arrastra al ejercicio siguiente y
    /// la que hace inútil descubrir tarde.
    /// </summary>
    public int AntiguedadEnDias(DateOnly a) => a.DayNumber - FechaDelHecho.DayNumber;

    /// <summary>
    /// Cuánto tardó en descubrirse. <b>Es un indicador por sí mismo</b>: un hallazgo de hace dos
    /// años descubierto ayer dice algo del control, no sólo del hecho.
    /// </summary>
    public int DiasHastaElDescubrimiento =>
        FechaDelDescubrimiento.DayNumber - FechaDelHecho.DayNumber;

    public decimal EfectoEconomicoTotal => _reversos.Sum(r => r.EfectoEconomico ?? 0m);

    /// <summary>
    /// `H-01` — abrir el expediente.
    /// </summary>
    public static ExpedienteDeHallazgoPosterior Abrir(
        Ulid id,
        string tipo,
        DateOnly fechaDelHecho,
        DateOnly fechaDelDescubrimiento,
        string comoSeDescubrio,
        string fuente,
        string? documentoAdjunto,
        IReadOnlyList<Ulid> misiones,
        Ulid? vehiculo,
        Ulid? motorista,
        string? periodo,
        Autoria descubre,
        DateTimeOffset momento)
    {
        ReglasDelHallazgoPosterior.ExigirDatosDelDescubrimiento(
            tipo, comoSeDescubrio, fuente, fechaDelHecho, fechaDelDescubrimiento);

        ReglasDelHallazgoPosterior.ExigirAlgoAQueVincularse(
            misiones, vehiculo, motorista, periodo);

        var expediente = new ExpedienteDeHallazgoPosterior(
            id, tipo.Trim(), fechaDelHecho, fechaDelDescubrimiento, comoSeDescubrio.Trim(),
            fuente.Trim(), documentoAdjunto?.Trim(), vehiculo, motorista, periodo?.Trim());

        expediente._misiones.AddRange(misiones);

        expediente._diario.Add(new MovimientoDelHallazgo(
            "H-01", descubre, momento,
            $"{tipo.Trim()}. Hecho del {fechaDelHecho:dd/MM/yyyy}, descubierto el " +
            $"{fechaDelDescubrimiento:dd/MM/yyyy} " +
            $"({expediente.DiasHastaElDescubrimiento} días después) por {comoSeDescubrio.Trim()}, " +
            $"contra {fuente.Trim()}. " +
            (misiones.Count == 0
                ? "SIN MISIÓN VINCULABLE."
                : $"{misiones.Count} misión(es) vinculada(s).")));

        return expediente;
    }

    public static ExpedienteDeHallazgoPosterior Reconstruir(
        Ulid id, string tipo, DateOnly fechaDelHecho, DateOnly fechaDelDescubrimiento,
        string comoSeDescubrio, string fuente, string? documentoAdjunto,
        IReadOnlyList<Ulid> misiones, Ulid? vehiculo, Ulid? motorista, string? periodo,
        ResolucionDelHallazgo? resolucion, string? fundamento,
        IEnumerable<MovimientoDelHallazgo> diario, IEnumerable<AsientoReverso> reversos)
    {
        var expediente = new ExpedienteDeHallazgoPosterior(
            id, tipo, fechaDelHecho, fechaDelDescubrimiento, comoSeDescubrio, fuente,
            documentoAdjunto, vehiculo, motorista, periodo)
        {
            Resolucion = resolucion,
            Fundamento = fundamento,
        };

        expediente._misiones.AddRange(misiones);
        expediente._diario.AddRange(diario);
        expediente._reversos.AddRange(reversos);

        if (expediente._diario.Count == 0)
            throw new ArgumentException(
                "Un expediente sin diario no tiene historia que mostrar.", nameof(diario));

        return expediente;
    }

    /// <summary>
    /// `H-02` — vincular otra misión.
    ///
    /// <b>Un expediente, varias misiones</b>: `RN-93` casos límite — un comprobante duplicado en
    /// dos delegaciones es un solo hallazgo con dos misiones y un asiento por cada efecto.
    /// </summary>
    public void Vincular(Ulid mision, Autoria vincula, string motivo, DateTimeOffset momento)
    {
        ExigirAbierto("H-02");

        if (_misiones.Contains(mision)) return;

        _misiones.Add(mision);

        _diario.Add(new MovimientoDelHallazgo(
            "H-02", vincula, momento,
            $"Vinculada la misión {mision}. {motivo.Trim()}"));
    }

    /// <summary>
    /// `H-03` — asentar un reverso. §8.3.
    ///
    /// <b>El expediente vinculado no se toca.</b> Este asiento se agrega y se refiere a él; el
    /// original queda como estaba, y el reporte muestra los tres valores.
    /// </summary>
    public void Revertir(AsientoReverso reverso, DateTimeOffset momento)
    {
        ExigirAbierto("H-03");

        ReglasDelAsientoReverso.ExigirQueQuienRevierteNoSeaQuienRegistro(
            reverso.Autoriza, reverso.AutorDelAsientoOriginal);

        ReglasDelAsientoReverso.ExigirContenidoCompleto(
            reverso.Revertido, reverso.ValorAnterior, reverso.MotivoTipificado,
            reverso.Fundamento, reverso.PeriodoAfectado, reverso.PeriodoDeImputacion);

        ReglasDelAsientoReverso.ExigirImputacionAlCorriente(
            reverso.PeriodoAfectado, reverso.PeriodoDeImputacion, reverso.EfectoEconomico);

        // **No se revierte dos veces el mismo asiento.** Un segundo reverso sobre el mismo
        // asiento duplicaría el efecto económico, y el acumulado del período quedaría con una
        // corrección de más que nadie va a poder rastrear.
        if (_reversos.Any(r =>
                r.Revertido.Tipo == reverso.Revertido.Tipo &&
                string.Equals(r.Revertido.Identificador, reverso.Revertido.Identificador,
                    StringComparison.OrdinalIgnoreCase)))
            throw new BloqueoDuro("RN-93",
                $"El asiento «{reverso.Revertido.Identificador}» ya tiene un reverso en este " +
                "expediente. Un segundo duplicaría el efecto económico sobre el período " +
                "corriente, y esa corrección de más no la va a poder rastrear nadie.");

        _reversos.Add(reverso);

        _diario.Add(new MovimientoDelHallazgo(
            "H-03", reverso.Autor, momento, reverso.Cadena, Reverso: reverso.Id));
    }

    /// <summary>
    /// `H-04` — resolver. <b>El expediente no se cierra sin resolución</b> (`RN-93` punto 6).
    ///
    /// ── Y la resolución tiene que ser cierta ────────────────────────────────
    /// Declarar «con asiento reverso» sin reversos diría que se corrigió algo que nadie tocó; y
    /// declarar «sin efecto» habiendo revertido dinero diría que el hallazgo no tuvo efecto
    /// cuando lo tuvo. Las dos son falsas de una forma que ningún reporte podría detectar
    /// después.
    /// </summary>
    public void Resolver(
        ResolucionDelHallazgo resolucion, string fundamento, Autoria resuelve,
        DateTimeOffset momento)
    {
        ExigirAbierto("H-04");

        if (string.IsNullOrWhiteSpace(fundamento))
            throw new BloqueoDuro("RN-93",
                "La resolución exige fundamento escrito. Sin él, resolver es indistinguible de " +
                "archivar el expediente sin mirarlo.");

        ReglasDelHallazgoPosterior.ExigirResolucionCoherente(resolucion, _reversos.Count);

        Resolucion = resolucion;
        Fundamento = fundamento.Trim();

        _diario.Add(new MovimientoDelHallazgo(
            "H-04", resuelve, momento,
            $"{Texto(resolucion)}. {fundamento.Trim()}" +
            (_reversos.Count > 0
                ? $" {_reversos.Count} asiento(s) reverso(s), efecto económico total " +
                  $"{EfectoEconomicoTotal:N2}."
                : "")));
    }

    private static string Texto(ResolucionDelHallazgo r) => r switch
    {
        ResolucionDelHallazgo.ConAsientoReverso => "RESUELTO con asiento reverso",
        ResolucionDelHallazgo.SinEfectoEconomico =>
            "RESUELTO: el hallazgo era real y no tiene efecto económico",
        ResolucionDelHallazgo.SinEfecto =>
            "RESUELTO SIN EFECTO: era un error del propio descubridor",
        _ => r.ToString(),
    };

    private void ExigirAbierto(string movimiento)
    {
        if (EstaAbierto) return;

        throw new BloqueoDuro("RN-93",
            $"El expediente ya está resuelto como {Resolucion}, y `{movimiento}` lo modificaría. " +
            "Lo que aparezca después es un hallazgo nuevo, no una corrección de éste — igual " +
            "que una misión cerrada no se reabre.");
    }
}

/// <summary>Los controles del expediente — `RN-93`.</summary>
public static class ReglasDelHallazgoPosterior
{
    /// <summary>
    /// `RN-93` punto 1 y el enunciado — quién, cómo, cuándo y contra qué fuente.
    ///
    /// ── Las dos fechas son distintas y ambas obligatorias ───────────────────
    /// La regla lo dice con esas palabras. Y la razón está en su justificación: contar la
    /// antigüedad desde el descubrimiento <i>«evita el incentivo perverso más obvio: descubrir
    /// tarde para que el indicador se vea mejor»</i>.
    /// </summary>
    public static void ExigirDatosDelDescubrimiento(
        string tipo, string comoSeDescubrio, string fuente,
        DateOnly fechaDelHecho, DateOnly fechaDelDescubrimiento)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new BloqueoDuro("RN-93",
                "El hallazgo exige tipo del catálogo. Un tipo libre no se agrupa, y lo que no " +
                "se agrupa no produce ningún indicador de control.");

        if (string.IsNullOrWhiteSpace(comoSeDescubrio))
            throw new BloqueoDuro("RN-93",
                "El expediente exige decir CÓMO se descubrió. Es lo que después permite saber " +
                "qué control funcionó — y cuál habría que haber tenido.");

        if (string.IsNullOrWhiteSpace(fuente))
            throw new BloqueoDuro("RN-93",
                "El expediente exige contra qué fuente se descubrió. Sin ella, el hallazgo es " +
                "la afirmación de quien lo abrió y no se puede volver a comprobar.");

        if (fechaDelDescubrimiento < fechaDelHecho)
            throw new BloqueoDuro("RN-93",
                $"El descubrimiento ({fechaDelDescubrimiento:dd/MM/yyyy}) es anterior al hecho " +
                $"({fechaDelHecho:dd/MM/yyyy}). Eso no describe un hallazgo posterior: una de " +
                "las dos fechas está mal, y son campos distintos precisamente para poder verlo.");
    }

    /// <summary>
    /// `RN-93` — el expediente vincula <b>cero, una o varias misiones</b>, un vehículo, un
    /// motorista o un período.
    ///
    /// <b>Cero misiones es válido y es el caso interesante</b>: el paso por caseta de un domingo,
    /// el consumo de un vehículo que ese día no tenía orden. Pero <b>algo</b> tiene que
    /// vincular: un hallazgo sin objeto ni período no se puede investigar ni reportar.
    /// </summary>
    public static void ExigirAlgoAQueVincularse(
        IReadOnlyList<Ulid> misiones, Ulid? vehiculo, Ulid? motorista, string? periodo)
    {
        if (misiones.Count > 0 || vehiculo is not null || motorista is not null ||
            !string.IsNullOrWhiteSpace(periodo))
            return;

        throw new BloqueoDuro("RN-93",
            "El expediente no vincula misión, ni vehículo, ni motorista, ni período. Cero " +
            "misiones es un caso previsto —el paso de un domingo sin orden— pero entonces el " +
            "vehículo y el período son el vínculo: sin ninguno, el hallazgo no se puede " +
            "investigar ni reportar.");
    }

    /// <summary>
    /// La resolución tiene que ser cierta respecto de lo que el expediente contiene.
    /// </summary>
    public static void ExigirResolucionCoherente(
        ResolucionDelHallazgo resolucion, int reversos)
    {
        if (resolucion is ResolucionDelHallazgo.ConAsientoReverso && reversos == 0)
            throw new BloqueoDuro("RN-93",
                "No se puede resolver «con asiento reverso» sin ningún reverso asentado. Eso " +
                "diría que se corrigió algo que nadie tocó.");

        if (resolucion is not ResolucionDelHallazgo.ConAsientoReverso && reversos > 0)
            throw new BloqueoDuro("RN-93",
                $"El expediente tiene {reversos} asiento(s) reverso(s) y se está resolviendo " +
                "como sin efecto. Si ya se revirtió dinero, el hallazgo tuvo efecto: lo " +
                "contrario es falso de una forma que ningún reporte podría detectar después.");
    }
}
