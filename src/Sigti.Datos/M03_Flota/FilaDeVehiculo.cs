using Sigti.Datos.M03_Flota;
using Sigti.Dominio.M03_Flota;

namespace Sigti.Datos;

/// <summary>
/// El vehículo en la base — <b>`M-03` ficha y `M-04` documentación en una sola fila</b>.
///
/// ── Por qué van juntas por ahora ─────────────────────────────────────────────
/// Porque los vencimientos que hoy se necesitan son cuatro campos, y separarlos en una
/// tabla de documentos con su ciclo de vida es `M-04` completo — que incluye alertas
/// (`RN-17`), renovaciones y adjuntos. Partirlo antes de necesitarlo produciría un
/// `JOIN` por cada evaluación de `BD-03` sin ganar nada.
///
/// <b>Lo que sí cambia respecto de antes:</b> los vencimientos son reales. Mientras la
/// flota vivía en código, la documentación provisional devolvía 2030 para todo y `BD-03`
/// no podía bloquear — el propio código lo decía, para no fingir que había verificado.
/// </summary>
public sealed class FilaDeVehiculo
{
    public required Ulid Id { get; init; }

    /// <summary>
    /// El correlativo institucional. <b>Es la identidad estable del bien</b>, no la placa:
    /// la placa cambia y puede no existir (`RN-15`).
    /// </summary>
    public required string Siglas { get; init; }

    /// <summary>
    /// <b>Nula es estado válido.</b> Hay desabastecimiento nacional de placas metálicas, y
    /// un campo obligatorio y único acá rompería el sistema para la flota real.
    /// </summary>
    public string? Placa { get; init; }

    /// <summary>
    /// ⚠️ <b>Reemplazado por <see cref="EstadoDePlaca"/> y el historial de respaldo</b> —
    /// `RN-64`, `RN-65`.
    ///
    /// Decía <i>que hay una constancia</i> y nada más: una vencida a mitad de la misión pasaba
    /// exactamente igual que una vigente. Se conserva mientras dure la migración de las filas
    /// existentes; no lo lea código nuevo.
    /// </summary>
    public required bool TieneConstanciaSustitutaDePlaca { get; init; }

    /// <summary>
    /// El estado de la <b>lámina física</b> — `RN-64`, catálogo `estado_de_placa`.
    ///
    /// Es un dato distinto y no intercambiable con <see cref="Placa"/>: el número puede existir
    /// aunque la lámina no. Un vehículo con número y sin lámina, uno con la lámina retenida por
    /// la DNVT y uno que nunca tuvo número son <b>tres situaciones administrativas distintas</b>
    /// que con un campo `placa` vacío se ven iguales.
    /// </summary>
    public EstadoDePlaca EstadoDePlaca { get; set; } = EstadoDePlaca.ConLamina;

    /// <summary>Los respaldos de circulación sin lámina, con su historial de vigencia.</summary>
    public List<FilaDeRespaldoDePlaca> RespaldosDePlaca { get; } = [];

    /// <summary>Texto libre de cada institución: «pick-up», «microbús», «cisterna».</summary>
    public required string TipoDeVehiculo { get; init; }

    /// <summary>
    /// <b>Conjunto cerrado del Artículo 4 del Acuerdo 1012-2021</b>, y distinto del tipo de
    /// arriba. Es lo que resuelve la matriz licencia↔vehículo: con masa, pasajeros y
    /// remolque no se distingue una motocicleta de un automóvil liviano.
    /// </summary>
    public required ClaseNormativa Clase { get; init; }

    public required int PesoBrutoKg { get; init; }
    public required int CapacidadPasajeros { get; init; }

    /// <summary>
    /// Si va enganchado a un remolque o semirremolque. <b>No es «articulado»</b>: un pick-up
    /// con plataforma enganchada requiere `BE` y no es articulado en ningún sentido.
    /// </summary>
    public required bool LlevaRemolque { get; init; }

    /// <summary>
    /// Cuánto le cabe al tanque, en galones. <b>Dato del fabricante</b>, no de la institución.
    ///
    /// Nula cuando no se ha cargado, y entonces las lecturas del indicador en fracción no se
    /// pueden convertir a galones: un octavo <b>no es una cantidad</b> hasta saber de qué
    /// tanque, y sin la conversión el remanente de `RN-83` no se separa del consumo.
    /// </summary>
    public decimal? CapacidadDeTanqueGalones { get; init; }

    /// <summary>
    /// Qué combustible usa. <b>Nulo es «la ficha no lo declara»</b>, no un dato pendiente.
    ///
    /// Es contra lo que `RN-32` compara el vale: sin esto el bloqueo del vale de diésel en un
    /// motor de gasolina no puede evaluarse, y se llamaba con nulo en todos los casos.
    /// </summary>
    public string? TipoDeCombustible { get; set; }

    /// <summary>
    /// Cuántos ejes tiene. Lo exige la matriz de derivación de categoría de peaje de
    /// <c>RN-33</c>.
    ///
    /// <b>Nunca es la única llave.</b> Un liviano de 2 ejes paga L 22 y un «Vehículo de 2
    /// Ejes» paga L 90: resolver la tarifa por este número cobraría cuatro veces de más a
    /// cada pickup de la flota.
    ///
    /// ⚠️ Nulo deja la categoría <i>no resuelta</i> —no adivinada— y el vehículo no se puede
    /// programar (<c>BD-07</c>).
    /// </summary>
    public int? NumeroDeEjes { get; set; }

    // ── Las anclas de `RN-66` ───────────────────────────────────────────
    // Con qué se identifica el vehículo desde afuera: una multa, una línea de estado de
    // cuenta, un acta de autoridad. **La placa va última** en la jerarquía porque se
    // reasigna y porque hay vehículos circulando sin ella (`RN-15`); estas tres son las
    // estables, y hasta hoy el padrón no las tenía.
    //
    // ⚠️ Anulables, y nulas para toda la flota cargada: son datos de alta y `M-03` no tiene
    // pantalla de alta. Una imputación que sólo trae número de bien no se va a resolver
    // hasta que se carguen — y eso es mejor que resolverla por parecido de placa.

    /// <summary>El número de bien del inventario nacional. El ancla más estable.</summary>
    public string? BienDelInventario { get; set; }

    public string? Chasis { get; set; }

    public string? Motor { get; set; }

    /// <summary>El correlativo institucional, distinto de las siglas de rotulación.</summary>
    public string? CorrelativoInstitucional { get; set; }

    /// <summary>Bloqueante — [`RN-103`]. La institución puede renovarla; es trámite propio.</summary>
    public required DateOnly VenceMatricula { get; init; }

    /// <summary>Nula si no tiene. <b>No bloquea por defecto</b>: no es obligatoria por ley.</summary>
    public DateOnly? VencePoliza { get; init; }

    /// <summary>Igual que la póliza.</summary>
    public DateOnly? VenceRevisionMecanica { get; init; }

    /// <summary>
    /// ⚠️ <b>Reemplazado por <see cref="Constataciones"/></b> — `RN-18`.
    ///
    /// Un <c>true</c> no decía <b>cuándo</b> se miró, ni <b>quién</b>, ni dejaba nada que
    /// mostrar. Una constatación de hace tres años se veía igual que una de ayer, y `CLAUDE.md`
    /// pide el campo <i>«verificable con fecha y foto»</i>.
    ///
    /// Se conserva mientras dure la migración de las filas existentes; no lo lea código nuevo.
    /// </summary>
    public required bool IdentificacionInstitucionalVerificada { get; init; }

    /// <summary>
    /// Las constataciones de rotulación, con su historial — `RN-18`.
    ///
    /// <b>Una fila por elemento</b>: un vehículo puede tener las franjas y no la leyenda, y con
    /// un solo dato para los cuatro «rotulación verificada» afirma de más sobre tres de ellos.
    /// </summary>
    public List<FilaDeConstatacion> Constataciones { get; } = [];

    /// <summary>
    /// El servicio exceptuado del vehículo — `RN-24`. Nulo es el caso normal: <b>no</b> está
    /// exceptuado.
    ///
    /// Va en columnas del vehículo y no en tabla aparte porque `RN-24` es taxativo: <i>«la
    /// excepción es atributo del vehículo, no del viaje»</i>. Una tabla de excepciones
    /// invitaría a registrar una por misión, que es exactamente lo que la regla prohíbe —
    /// <i>«cualquier misión podría autoexceptuarse alegando urgencia, y el control se
    /// vaciaría en una semana»</i>.
    /// </summary>
    public string? TipoDeServicioExceptuado { get; init; }

    /// <summary>Qué documento sostiene la excepción. `RN-24` no admite la casilla sin respaldo.</summary>
    public string? FundamentoDeLaExcepcion { get; init; }

    public DateOnly? ExceptuadoDesde { get; init; }

    /// <summary>Nulo con excepción vigente es <b>indefinida</b>, no eterna.</summary>
    public DateOnly? ExceptuadoHasta { get; init; }

    /// <summary>
    /// La excepción como valor del dominio, o nula.
    ///
    /// <b>Exige tipo, fundamento y fecha de inicio a la vez.</b> Los tres o ninguno: una
    /// excepción con tipo y sin fundamento es la casilla marcada que `RN-24` rechaza, y
    /// dejarla pasar acá la volvería operativa igual.
    /// </summary>
    public ServicioExceptuado? Excepcion() =>
        TipoDeServicioExceptuado is { } tipo
        && FundamentoDeLaExcepcion is { } fundamento
        && ExceptuadoDesde is { } desde
            ? new ServicioExceptuado(tipo, fundamento, desde, ExceptuadoHasta)
            : null;

    /// <summary>La ficha técnica que `BD-02` necesita, armada desde las columnas.</summary>
    public FichaTecnica Ficha() =>
        new(TipoDeVehiculo, Clase, PesoBrutoKg, CapacidadPasajeros, LlevaRemolque,
            CapacidadDeTanqueGalones, NumeroDeEjes, TipoDeCombustible);

    /// <summary>
    /// La documentación que `BD-03` evalúa. <b>Con fechas reales.</b>
    /// </summary>
    /// <param name="alSalir">
    /// La fecha de salida de la misión, para elegir <b>qué respaldo regía entonces</b>.
    ///
    /// ⚠️ Es `P-4`: un despacho capturado tarde se juzga contra el respaldo que estaba vigente
    /// el día de la salida, no contra el de hoy. Tomar «el último cargado» diría si el vehículo
    /// está respaldado ahora, que es otra pregunta.
    ///
    /// Nula sólo donde no hay misión de la cual sacarla — y entonces el respaldo no se resuelve
    /// y `RN-65` bloquea por «sin respaldo», que es lo correcto: no se pudo comprobar.
    /// </param>
    public DocumentacionDelVehiculo Documentacion(DateOnly? alSalir = null) => new()
    {
        Placa = Placa,
        TieneConstanciaSustitutaDePlaca = TieneConstanciaSustitutaDePlaca,
        EstadoDePlaca = EstadoDePlaca,

        // El respaldo vigente a la fecha del hecho. **Nulo es que no hay ninguno**, y con el
        // vehículo sin lámina eso bloquea: `RN-65` — lo que impide despachar no es la ausencia
        // de placa, es la ausencia de respaldo.
        Respaldo = alSalir is not { } fecha
            ? null
            : RespaldosDePlaca
                .Where(r => r.VigenteDesde <= fecha)
                .OrderByDescending(r => r.VigenteDesde)
                .Select(r => new RespaldoDePlaca(
                    r.Tipo, r.Emisor, r.Folio, r.Adjunto, r.VigenteDesde, r.VigenteHasta))
                .FirstOrDefault(),

        VenceMatricula = VenceMatricula,
        VencePoliza = VencePoliza,
        VenceRevisionMecanica = VenceRevisionMecanica,
        IdentificacionInstitucionalVerificada = IdentificacionInstitucionalVerificada,
    };
}
