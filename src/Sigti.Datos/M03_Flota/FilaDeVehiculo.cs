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

    public required bool TieneConstanciaSustitutaDePlaca { get; init; }

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

    /// <summary>Bloqueante — [`RN-103`]. La institución puede renovarla; es trámite propio.</summary>
    public required DateOnly VenceMatricula { get; init; }

    /// <summary>Nula si no tiene. <b>No bloquea por defecto</b>: no es obligatoria por ley.</summary>
    public DateOnly? VencePoliza { get; init; }

    /// <summary>Igual que la póliza.</summary>
    public DateOnly? VenceRevisionMecanica { get; init; }

    /// <summary>Franjas, leyenda y siglas verificadas — `RN-18`. Hallazgo frecuente.</summary>
    public required bool IdentificacionInstitucionalVerificada { get; init; }

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
        new(TipoDeVehiculo, Clase, PesoBrutoKg, CapacidadPasajeros, LlevaRemolque);

    /// <summary>La documentación que `BD-03` evalúa. <b>Con fechas reales.</b></summary>
    public DocumentacionDelVehiculo Documentacion() => new()
    {
        Placa = Placa,
        TieneConstanciaSustitutaDePlaca = TieneConstanciaSustitutaDePlaca,
        VenceMatricula = VenceMatricula,
        VencePoliza = VencePoliza,
        VenceRevisionMecanica = VenceRevisionMecanica,
        IdentificacionInstitucionalVerificada = IdentificacionInstitucionalVerificada,
    };
}
