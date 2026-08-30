namespace Sigti.Datos.M06_Solicitudes;

/// <summary>
/// Un rango de folios reservado, tal como se guarda — `RN-44`, `RNF-21`.
/// </summary>
public sealed class FilaDeRango
{
    public required Ulid Id { get; init; }

    public required string Delegacion { get; init; }

    /// <summary>Del catálogo `documento_imprimible_control` (`RN-25`).</summary>
    public required string TipoDeDocumento { get; init; }

    public required int Desde { get; init; }

    public required int Hasta { get; init; }

    /// <summary>
    /// Cuántos se sacaron. <b>Incluye los anulados</b>: el folio de un documento anulado no
    /// vuelve al rango, deja un hueco, y el hueco se explica con su asiento reverso.
    /// </summary>
    public required int Emitidos { get; set; }

    /// <summary>
    /// <b>Nulo es «toda la delegación»</b>, y sólo sirve con un equipo emitiendo. Con dos o más
    /// hace falta un subrango por equipo: es el caso que `RNF-21` llama «la que realmente
    /// rompe», porque los tres emiten sin verse y la colisión aparece al sincronizar.
    /// </summary>
    public string? Dispositivo { get; init; }

    public required string Asigna { get; init; }

    public required DateOnly AsignadoEl { get; init; }
}
