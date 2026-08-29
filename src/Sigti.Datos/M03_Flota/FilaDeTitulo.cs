using Sigti.Dominio.M03_Flota;

namespace Sigti.Datos.M03_Flota;

/// <summary>
/// El título de tenencia del vehículo, tal como se guarda — `RN-62`.
///
/// ── Es una serie, no un campo del vehículo ──────────────────────────────────
/// Un vehículo puede pasar de comodato a propiedad cuando la donación se perfecciona, y el
/// período anterior sigue siendo cierto: las misiones de ese tiempo se hicieron bajo comodato y
/// sus rubros los cubría el cedente. Guardarlo como columna del vehículo reescribiría la
/// historia cada vez que cambie el régimen.
/// </summary>
public sealed class FilaDeTitulo
{
    public required Ulid Id { get; init; }

    public required Ulid VehiculoId { get; init; }

    /// <summary>Del catálogo `regimen_de_tenencia`, que `RN-62` declara configurable.</summary>
    public required RegimenDeTenencia Regimen { get; init; }

    /// <summary>Quién es el propietario o cedente.</summary>
    public required string Titular { get; init; }

    /// <summary>Convenio, contrato, acta o resolución. Sin él, el título no existe.</summary>
    public required string Documento { get; init; }

    public required DateOnly Desde { get; init; }

    /// <summary>
    /// <b>Nula sólo en propiedad</b>, que es el único régimen que no vence. En los demás, su
    /// ausencia haría que el título no venciera nunca — y un comodato que no vence es una
    /// apropiación.
    /// </summary>
    public DateOnly? Hasta { get; init; }

    // ── La matriz de rubros — `RN-62` ───────────────────────────────────────
    // «Sin pactar» no es «la institución»: es el rubro que aparece cuando llega la factura.
    public required QuienAsume Combustible { get; init; }
    public required QuienAsume Mantenimiento { get; init; }
    public required QuienAsume Llantas { get; init; }
    public required QuienAsume Seguro { get; init; }
    public required QuienAsume Peajes { get; init; }
    public required QuienAsume Multas { get; init; }
    public required QuienAsume Danios { get; init; }
}
