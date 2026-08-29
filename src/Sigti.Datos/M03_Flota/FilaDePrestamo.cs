namespace Sigti.Datos.M03_Flota;

/// <summary>
/// El expediente de préstamo, tal como se guarda — `RN-63`.
///
/// <b>Nunca es una Orden de Misión</b>, y por eso vive en su propia tabla: modelarlo como misión
/// diría que la unidad seguía bajo custodia de la institución cuando la tenencia se cedió.
/// </summary>
public sealed class FilaDePrestamo
{
    public required Ulid Id { get; init; }

    public required Ulid VehiculoId { get; init; }

    // ── El acto autorizante — `RN-63` punto 1 ───────────────────────────────
    public required string ActoFolio { get; init; }
    public required string ActoFirmante { get; init; }
    public required DateOnly ActoFecha { get; init; }
    public string? ActoAdjunto { get; init; }

    /// <summary>
    /// Quién autorizó la salida. <b>No puede ser el receptor</b>: `RN-63` punto 2 lo declara
    /// incompatibilidad de segregación.
    /// </summary>
    public required string Autoriza { get; init; }

    // ── El responsable receptor — `RN-63` punto 2 ───────────────────────────
    public required string ReceptorPersona { get; init; }
    public required string ReceptorCargo { get; init; }
    public required string ReceptorInstitucion { get; init; }
    public required string ReceptorConstancia { get; init; }

    /// <summary>Del catálogo `motivo_de_prestamo`, que `RN-63` declara configurable.</summary>
    public required string Motivo { get; init; }

    public required DateOnly Desde { get; init; }

    /// <summary>
    /// La fecha pactada. Vencerla no cierra el préstamo: lo pone en mora, con escalamiento
    /// diario, y `RN-97` punto 4 impide cerrar el período con préstamos vencidos.
    /// </summary>
    public required DateOnly DevolucionComprometida { get; init; }

    // ── El acta de entrega — `RN-63` punto 4 ────────────────────────────────
    public required DateOnly EntregaFecha { get; init; }
    public required int EntregaOdometro { get; init; }
    public required string EntregaFirma { get; init; }
    public string? EntregaCombustible { get; init; }
    public string? EntregaAccesorios { get; init; }
    public string? EntregaDocumentos { get; init; }
    public required bool EntregaRotulacion { get; init; }
    public string? EntregaNovedades { get; init; }

    // ── Los rubros pactados — `RN-63` punto 5 ───────────────────────────────
    // Nulo es «no pactado», y eso es lo que aparece cuando llega la multa.
    public string? RubroCombustible { get; init; }
    public string? RubroPeajes { get; init; }
    public string? RubroMantenimiento { get; init; }
    public string? RubroMultas { get; init; }
    public string? RubroDanios { get; init; }

    // ── El acta de devolución — `RN-63` punto 6 ─────────────────────────────
    // Nula mientras el vehículo no vuelva. **No vuelve a DISPONIBLE sin ella.**
    public DateOnly? DevolucionFecha { get; set; }
    public int? DevolucionOdometro { get; set; }
    public string? DevolucionFirma { get; set; }
    public string? DevolucionCombustible { get; set; }
    public string? DevolucionNovedades { get; set; }

    /// <summary>
    /// La <b>reconstatación</b> de la rotulación (`RN-63` punto 6). La identificación del
    /// vehículo del Estado es hallazgo frecuente de auditoría, y uno que vuelve sin ella volvió
    /// distinto de como salió.
    /// </summary>
    public bool? DevolucionRotulacion { get; set; }

    /// <summary>
    /// Quién firma la devolución por la institución propietaria. <b>No puede ser quien
    /// recibió</b>: el acta dejaría de ser constatación para volverse autodeclaración.
    /// </summary>
    public string? QuienFirmaLaDevolucion { get; set; }
}
