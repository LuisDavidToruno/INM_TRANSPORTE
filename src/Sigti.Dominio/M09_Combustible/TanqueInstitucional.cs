using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// Qué mueve un asiento de existencias.
///
/// ── Por qué el arqueo NO mueve nada ─────────────────────────────────────────
/// `E-05` mide y no ajusta. Es la misma disciplina que `RN-86` punto 4 impone al plazo
/// vencido: <i>«nunca cuadre automático»</i>. Un arqueo que corrige el libro por su cuenta
/// hace desaparecer la diferencia en el mismo acto que la descubre, y entonces la única
/// pregunta que un arqueo existe para contestar —<b>¿cuánto falta?</b>— deja de tener
/// respuesta en el registro.
/// </summary>
public enum TipoDeMovimiento
{
    /// <summary>`E-01` — entra combustible comprado. Con comprobante.</summary>
    Ingreso,

    /// <summary>`E-02` a `E-03` — sale combustible: a un vehículo o a otro tanque.</summary>
    Egreso,

    /// <summary>
    /// `E-05` — se midió la existencia física. <b>No mueve el libro</b>: deja constancia de lo
    /// medido y de la diferencia, para que alguien con competencia decida qué fue.
    /// </summary>
    Constatacion,

    /// <summary>
    /// `E-06` — el acto que sí mueve el libro, con motivo tipificado y autoría. Separado de la
    /// constatación a propósito: medir es una cosa y decidir qué pasó con lo que falta es otra,
    /// y de otro.
    /// </summary>
    Ajuste,
}

/// <summary>
/// Qué fue la diferencia. <b>No hay opción «diferencia» a secas</b>, y es el mismo principio
/// que `CE-26` §3 impone al faltante del fondo: <i>«si el catálogo no lo tiene, el liquidador
/// elige el motivo más cercano que no genere problema, y el faltante desaparece del reporte»</i>.
/// </summary>
public enum MotivoDeAjuste
{
    /// <summary>
    /// Evaporación y merma técnica del almacenamiento. `[C]` <b>el rango admisible no está
    /// definido</b> — insumo #1: `RN-69` usa merma esperada de catálogo para carga a granel y
    /// acá no hay catálogo equivalente. Sin rango, el sistema registra la merma declarada y
    /// <b>no puede decir si es razonable</b>.
    /// </summary>
    MermaTecnica,

    /// <summary>Un asiento anterior estaba mal. El asiento viejo <b>no se toca</b> (P-3).</summary>
    ErrorDeRegistro,

    /// <summary>
    /// Falta y nadie sabe por qué. <b>Tiene que existir</b>, por la misma razón que
    /// `SinCausaIdentificada` existe en el reintegro.
    /// </summary>
    FaltanteSinCausaIdentificada,

    /// <summary>Sustracción con denuncia — expediente en M-12 (`RN-75`).</summary>
    Sustraccion,
}

/// <summary>
/// Un asiento del libro de existencias del tanque.
/// </summary>
/// <param name="Id">`E-01` recibir · `E-02` despachar a vehículo · `E-03` trasiego de salida · `E-04` trasiego de entrada · `E-05` constatar · `E-06` ajustar.</param>
/// <param name="Galones">
/// Siempre <b>positivo</b>. El signo lo pone <see cref="Tipo"/>, no el número: un galonaje
/// negativo en una columna es un dato que se puede teclear al revés sin que nada lo note.
/// </param>
/// <param name="Autor">
/// Quién y <b>con qué competencia</b>. `RN-83` punto 5 exige <i>«responsable de despacho
/// identificado»</i>, y `RN-01` que no sea cualquiera.
/// </param>
/// <param name="Vehiculo">
/// A qué vehículo se despachó — sólo `E-02`. Es lo que imputa el galón a una placa, y sin ello
/// el egreso dice cuánto salió pero no adónde fue.
/// </param>
/// <param name="Contraparte">
/// El otro tanque, en el trasiego. <b>Nunca nulo en `E-03` y `E-04`</b>: un trasiego sin destino
/// es una salida que se evapora del sistema entero en vez de sólo de este tanque.
/// </param>
/// <param name="ExistenciaMedida">
/// Lo que dio la medición física — sólo `E-05`. Va aparte de <see cref="Galones"/> porque no es
/// un movimiento: es una segunda opinión sobre el saldo.
/// </param>
public sealed record MovimientoDeExistencias(
    string Id,
    TipoDeMovimiento Tipo,
    decimal Galones,
    Autoria Autor,
    DateTimeOffset Momento,
    string Motivo,
    Ulid? Vehiculo = null,
    Ulid? Mision = null,
    Ulid? Abastecimiento = null,
    Ulid? Contraparte = null,
    decimal? ExistenciaMedida = null,
    MotivoDeAjuste? MotivoDelAjuste = null,
    string? Comprobante = null);

/// <summary>
/// El tanque o cisterna de la institución — `RN-83` punto 5 y el circuito propio que la regla
/// le reserva al trasiego.
///
/// ── El agujero que cierra ───────────────────────────────────────────────────
/// `FuenteDeAbastecimiento.TanqueInstitucional` existe desde `RN-83` y <b>hasta hoy no
/// descontaba de ninguna parte</b>: se podía declarar que el galón salió del tanque de la sede
/// y el tanque no era nada. La regla dice <i>«descuenta de las existencias del tanque»</i>, y no
/// había existencias.
///
/// Sin el libro, el despacho desde el tanque queda igual de invisible que antes de `RN-83` —
/// sólo que ahora con la apariencia de estar registrado, que es peor.
///
/// ── La existencia es la suma del libro, nunca una columna ───────────────────
/// P-1, aplicado a una cantidad en vez de a un estado. Una columna
/// <c>existencia_actual</c> se desincroniza el primer día en que dos despachos entren a la vez,
/// y a partir de ahí el arqueo compara la realidad contra un número que ya no es la suma de
/// nada.
///
/// ⚠️ `[C]` <b>Que la institución tenga almacenamiento propio no está confirmado</b> — insumo
/// #36, y `HU-041` advierte que <i>«cambiaría el circuito completo de M-09»</i>. Si no lo
/// tiene, no se da de alta ningún tanque y esto no estorba. Lo que no puede seguir pasando es
/// que la fuente se declare y no descuente de nada.
/// </summary>
public sealed class TanqueInstitucional
{
    private readonly List<MovimientoDeExistencias> _libro = [];

    private TanqueInstitucional(
        Ulid id, string nombre, string ambitoDeclarado, string tipoDeCombustible,
        decimal? capacidadGalones)
    {
        Id = id;
        Nombre = nombre;
        AmbitoDeclarado = ambitoDeclarado;
        TipoDeCombustible = tipoDeCombustible;
        CapacidadGalones = capacidadGalones;
    }

    public Ulid Id { get; }

    public string Nombre { get; }

    /// <summary>
    /// La dependencia o delegación donde está. El tanque es <b>físico</b>: no se despacha desde
    /// Choluteca a un vehículo que está en Puerto Lempira, y el ámbito es lo que después permite
    /// cuadrar el consumo por dependencia.
    /// </summary>
    public string AmbitoDeclarado { get; }

    /// <summary>
    /// Diésel o gasolina. <b>Un tanque despacha un solo combustible</b>, y por eso esto no es
    /// decoración: un registro que dice que un camión diésel se llenó del tanque de gasolina
    /// cuadra en galones y es imposible en la realidad.
    /// </summary>
    public string TipoDeCombustible { get; }

    /// <summary>
    /// Cuánto le cabe. <b>Nula cuando no se ha cargado.</b>
    ///
    /// ⚠️ <b>Hoy no se comprueba contra ella</b>, y no se finge que sí. Un ingreso que rebalse
    /// el tanque no se puede rechazar —el combustible ya entró, y rechazar el asiento no lo
    /// saca del tanque: lo saca del libro, que es justo lo que `RN-83` existe para impedir—,
    /// así que lo único útil sería una alerta. Las alertas persistidas son del circuito de
    /// indicadores de `M-14`, que todavía no existe.
    /// </summary>
    public decimal? CapacidadGalones { get; }

    public IReadOnlyList<MovimientoDeExistencias> Libro => _libro;

    /// <summary>
    /// <b>La existencia en libros</b> — la suma del libro, nunca un campo.
    ///
    /// La constatación no entra: mide, no mueve.
    /// </summary>
    public decimal Existencia => _libro.Sum(m => m.Tipo switch
    {
        TipoDeMovimiento.Ingreso => m.Galones,
        TipoDeMovimiento.Egreso => -m.Galones,
        TipoDeMovimiento.Ajuste => m.Galones,
        _ => 0m,
    });

    /// <summary>La última medición física, si la hubo. Nula significa <b>nunca se arqueó</b>.</summary>
    public MovimientoDeExistencias? UltimaConstatacion =>
        _libro.LastOrDefault(m => m.Tipo is TipoDeMovimiento.Constatacion);

    /// <summary>
    /// Lo que el libro dice de más respecto de la última medición. Positivo = falta en el
    /// tanque; negativo = hay más de lo que el libro sabe.
    ///
    /// <b>Nula cuando nunca se arqueó</b>, y eso no es cero: un tanque sin arqueo no está
    /// cuadrado, está sin verificar.
    /// </summary>
    public decimal? DiferenciaDelUltimoArqueo
    {
        get
        {
            if (UltimaConstatacion is not { } arqueo) return null;

            // Se compara contra la existencia **a ese momento**, no contra la de hoy: lo que se
            // despachó después del arqueo no es parte de la diferencia que el arqueo encontró.
            var hasta = _libro.IndexOf(arqueo);

            var enLibros = _libro.Take(hasta).Sum(m => m.Tipo switch
            {
                TipoDeMovimiento.Ingreso => m.Galones,
                TipoDeMovimiento.Egreso => -m.Galones,
                TipoDeMovimiento.Ajuste => m.Galones,
                _ => 0m,
            });

            return enLibros - arqueo.ExistenciaMedida!.Value;
        }
    }

    public static TanqueInstitucional Abrir(
        Ulid id,
        string nombre,
        string ambitoDeclarado,
        string tipoDeCombustible,
        decimal? capacidadGalones,
        Autoria abre,
        decimal existenciaInicial,
        DateTimeOffset momento)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new BloqueoDuro("RN-83", "Un tanque sin nombre no se puede citar en un acta.");

        if (string.IsNullOrWhiteSpace(tipoDeCombustible))
            throw new BloqueoDuro("RN-83",
                "El tanque despacha un solo combustible y hay que decir cuál. Sin eso, un " +
                "registro que llene un camión diésel desde el tanque de gasolina cuadra en " +
                "galones y es imposible en la realidad.");

        if (existenciaInicial < 0)
            throw new BloqueoDuro("RN-83", "Un tanque no puede abrir con existencia negativa.");

        var tanque = new TanqueInstitucional(
            id, nombre.Trim(), ambitoDeclarado.Trim(), tipoDeCombustible.Trim(), capacidadGalones);

        // La existencia inicial entra como asiento y no como columna, por la misma razón que
        // todo lo demás: es lo que después permite explicar de dónde salió el primer galón.
        // `RN-97` la llama saldo de apertura de control interno.
        tanque._libro.Add(new MovimientoDeExistencias(
            "E-01", TipoDeMovimiento.Ingreso, existenciaInicial, abre, momento,
            $"Apertura del libro con {existenciaInicial:N2} galones de {tanque.TipoDeCombustible} " +
            $"constatados. Saldo de apertura (`RN-97`)."));

        return tanque;
    }

    public static TanqueInstitucional Reconstruir(
        Ulid id, string nombre, string ambitoDeclarado, string tipoDeCombustible,
        decimal? capacidadGalones, IEnumerable<MovimientoDeExistencias> libro)
    {
        var tanque = new TanqueInstitucional(
            id, nombre, ambitoDeclarado, tipoDeCombustible, capacidadGalones);

        tanque._libro.AddRange(libro);

        if (tanque._libro.Count == 0)
            throw new ArgumentException(
                "Un tanque sin libro no tiene existencia que proyectar.", nameof(libro));

        return tanque;
    }

    /// <summary>
    /// `E-01` — recibir una compra.
    /// </summary>
    public void Recibir(
        Autoria recibe, decimal galones, string comprobante, DateTimeOffset momento)
    {
        ReglasDelTanque.ExigirGalonesPositivos(galones);

        if (string.IsNullOrWhiteSpace(comprobante))
            throw new BloqueoDuro("RN-83",
                "El ingreso al tanque exige comprobante de la compra. Es el único documento que " +
                "ata estos galones a un gasto autorizado; sin él, el tanque es una fuente de " +
                "combustible sin origen.");

        _libro.Add(new MovimientoDeExistencias(
            "E-01", TipoDeMovimiento.Ingreso, galones, recibe, momento,
            $"Ingreso de {galones:N2} galones. Comprobante {comprobante.Trim()}.",
            Comprobante: comprobante.Trim()));
    }

    /// <summary>
    /// `E-02` — despachar a un vehículo. <b>Es el acto que `RN-83` punto 5 exige</b>, y el que
    /// convierte «salió del tanque» en un descargo.
    ///
    /// ── Bloquea, y eso es coherente con P-2 ─────────────────────────────────
    /// Despachar es un acto de <b>entrega</b>, y los bloqueos duros se aplican a los actos que
    /// autorizan, reservan o entregan. Lo que <b>no</b> se bloquea es el abastecimiento que un
    /// motorista declara desde el campo: ése es un hecho consumado y se registra igual, quedando
    /// como <b>discrepancia</b> contra este libro — que es justo el préstamo invisible de
    /// `CE-23`, ahora visible.
    /// </summary>
    /// <param name="recibe">
    /// El motorista, por su ULID del padrón. Va al asiento para poder contestar la pregunta que
    /// `RN-01` obliga: <b>quien despacha no puede ser quien recibe</b>.
    /// </param>
    public MovimientoDeExistencias Despachar(
        Autoria despacha,
        decimal galones,
        Ulid vehiculo,
        Ulid? mision,
        Ulid? abastecimiento,
        string combustibleDelVehiculo,
        IdPersonaDelReceptor recibe,
        DateTimeOffset momento)
    {
        ReglasDelTanque.ExigirGalonesPositivos(galones);
        ReglasDelTanque.ExigirCombustibleCompatible(TipoDeCombustible, combustibleDelVehiculo);
        ReglasDelTanque.ExigirQueDespachaNoSeaQuienRecibe(despacha, recibe);
        ReglasDelTanque.ExigirExistenciaSuficiente(Nombre, Existencia, galones);

        var movimiento = new MovimientoDeExistencias(
            "E-02", TipoDeMovimiento.Egreso, galones, despacha, momento,
            $"Despacho de {galones:N2} galones de {TipoDeCombustible} al vehículo. " +
            $"Recibe {recibe.Valor}. Quedan {Existencia - galones:N2}.",
            Vehiculo: vehiculo,
            Mision: mision,
            Abastecimiento: abastecimiento);

        _libro.Add(movimiento);
        return movimiento;
    }

    /// <summary>
    /// `E-03` y `E-04` — el trasiego entre tanques, que `RN-83` deja expresamente fuera del
    /// abastecimiento: <i>«es movimiento de existencias y tiene su propio circuito»</i>.
    ///
    /// ── Son dos asientos, y tienen que cuadrar ──────────────────────────────
    /// El galón que sale de un tanque entra al otro. Registrar sólo la salida haría que el
    /// combustible se evaporara del sistema entero en vez de sólo de este tanque — y esa es
    /// exactamente la forma en que un faltante se disfraza de traslado.
    ///
    /// Por eso este método <b>no se llama solo</b>: el servicio mueve los dos lados o ninguno.
    /// </summary>
    public void Trasegar(
        Autoria autoriza, decimal galones, TanqueInstitucional otro, bool sale,
        DateTimeOffset momento)
    {
        ReglasDelTanque.ExigirGalonesPositivos(galones);
        ReglasDelTanque.ExigirTanquesDistintos(Id, otro.Id);
        ReglasDelTanque.ExigirMismoCombustible(TipoDeCombustible, otro.TipoDeCombustible);

        if (sale)
        {
            ReglasDelTanque.ExigirExistenciaSuficiente(Nombre, Existencia, galones);

            _libro.Add(new MovimientoDeExistencias(
                "E-03", TipoDeMovimiento.Egreso, galones, autoriza, momento,
                $"Trasiego de salida de {galones:N2} galones hacia «{otro.Nombre}».",
                Contraparte: otro.Id));
            return;
        }

        _libro.Add(new MovimientoDeExistencias(
            "E-04", TipoDeMovimiento.Ingreso, galones, autoriza, momento,
            $"Trasiego de entrada de {galones:N2} galones desde «{otro.Nombre}».",
            Contraparte: otro.Id));
    }

    /// <summary>
    /// `E-05` — constatar la existencia física. <b>Mide y no ajusta.</b>
    ///
    /// La diferencia queda nombrada en el asiento aunque sea cero: callarla cuando cuadra y
    /// decirla cuando no, entrena a leer su ausencia como «no se midió».
    /// </summary>
    public void Constatar(
        Autoria comision, decimal existenciaMedida, string acta, DateTimeOffset momento)
    {
        if (existenciaMedida < 0)
            throw new BloqueoDuro("RN-83", "Una medición negativa no es una medición.");

        if (string.IsNullOrWhiteSpace(acta))
            throw new BloqueoDuro("RN-83",
                "La constatación exige acta con quiénes midieron y cómo. Un número sin acta no " +
                "se puede oponer al libro: es la palabra de alguien contra un registro.");

        var diferencia = Existencia - existenciaMedida;

        _libro.Add(new MovimientoDeExistencias(
            "E-05", TipoDeMovimiento.Constatacion, 0m, comision, momento,
            $"Medición física: {existenciaMedida:N2} galones contra {Existencia:N2} en libros. " +
            (diferencia == 0
                ? "Cuadra exacto."
                : diferencia > 0
                    ? $"FALTAN {diferencia:N2} galones en el tanque."
                    : $"SOBRAN {-diferencia:N2} galones sobre lo que el libro sabe.") +
            $" {acta.Trim()}",
            ExistenciaMedida: existenciaMedida));
    }

    /// <summary>
    /// `E-06` — ajustar el libro, con motivo tipificado y competencia.
    ///
    /// ── Por qué está separado de la constatación ────────────────────────────
    /// Medir es una cosa y decidir qué pasó con lo que falta es otra, y de otro (`RN-74`). Un
    /// arqueo que ajusta solo hace desaparecer la diferencia en el mismo acto que la descubre.
    /// </summary>
    /// <param name="galones">
    /// Con signo: negativo baja el libro —lo normal en una merma o un faltante—, positivo lo
    /// sube. <b>Es el único movimiento donde el signo va en el número</b>, porque el ajuste
    /// puede ir en las dos direcciones y forzarlo a dos tipos distintos escondería que son el
    /// mismo acto.
    /// </param>
    public void Ajustar(
        Autoria autoriza, decimal galones, MotivoDeAjuste motivo, string fundamento,
        DateTimeOffset momento)
    {
        if (galones == 0)
            throw new BloqueoDuro("RN-83", "Un ajuste de cero no ajusta nada.");

        if (string.IsNullOrWhiteSpace(fundamento))
            throw new BloqueoDuro("RN-83",
                "El ajuste de existencias exige fundamento escrito. Sin él, la salida más " +
                "cómoda de todo faltante sería ajustar el libro hasta que cuadre.");

        if (Existencia + galones < 0)
            throw new BloqueoDuro("RN-83",
                $"El ajuste dejaría el tanque en {Existencia + galones:N2} galones. Una " +
                "existencia negativa no describe ningún tanque: si el libro llegó ahí, lo que " +
                "hay es un egreso mal registrado, y eso se corrige nombrándolo.");

        _libro.Add(new MovimientoDeExistencias(
            "E-06", TipoDeMovimiento.Ajuste, galones, autoriza, momento,
            $"Ajuste de {galones:N2} galones por {motivo}. Existencia {Existencia:N2} → " +
            $"{Existencia + galones:N2}. {fundamento.Trim()}",
            MotivoDelAjuste: motivo));
    }
}

/// <summary>
/// El ULID del motorista que recibe el despacho, en un tipo propio.
///
/// Existe por la misma razón que <c>IdPersona</c>: para que el compilador impida pasar la
/// identidad de quien despacha donde va la de quien recibe. Sin él, `RN-01` compararía dos
/// veces la misma variable y el bloqueo no podría disparar nunca — el error exacto que ya se
/// corrigió una vez en `RN-32`.
/// </summary>
public readonly record struct IdPersonaDelReceptor(string Valor)
{
    public override string ToString() => Valor;
}
