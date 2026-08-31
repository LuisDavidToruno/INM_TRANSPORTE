using Sigti.Dominio.M15_Formatos;

namespace Sigti.Datos.M15_Formatos;

/// <summary>
/// El acuse de recepción de un documento impreso — `RN-65`: <i>«emitir, imprimir y entregar
/// <b>contra acuse</b>»</i>.
///
/// ── Lo que el acuse separa ──────────────────────────────────────────────────
/// Emitir e imprimir son actos de oficina. <b>El documento sirve cuando está en la guantera</b>,
/// y entre la impresora y el vehículo se pierde: se imprime y queda en el escritorio, o se
/// despacha antes de que salga la impresión.
///
/// Esto es lo que separa <i>«el sistema emitió el papel»</i> de <i>«el motorista lo tiene»</i>,
/// y en un operativo sólo la segunda importa.
/// </summary>
public sealed class FilaDeAcuse
{
    public required Ulid Id { get; init; }
    public required Ulid MisionId { get; init; }

    /// <summary>
    /// Cuál de los dos. <b>No se confunden</b>: el salvoconducto ampara circular en franja
    /// inhábil y el paquete identifica al vehículo sin lámina, y un acuse genérico dejaría sin
    /// saber cuál llegó a la mano del motorista.
    /// </summary>
    public required DocumentoEntregado Documento { get; init; }

    /// <summary>
    /// El folio del papel entregado. Nulo en el paquete de identificación, que <b>no lleva
    /// folio</b>: se arma en cada impresión y no se congela.
    /// </summary>
    public string? Folio { get; init; }

    public required string Entrega { get; init; }

    /// <summary>
    /// Quién firmó la recepción. <b>Tiene que ser el motorista de la orden</b>: el documento es
    /// nominativo, y un acuse a nombre de otro no prueba nada.
    /// </summary>
    public required string Recibe { get; init; }

    public required DateTime MomentoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }

    public string? Observaciones { get; init; }
}
