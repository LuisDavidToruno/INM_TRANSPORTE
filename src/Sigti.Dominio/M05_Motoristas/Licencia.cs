namespace Sigti.Dominio.M05_Motoristas;

/// <summary>
/// Categorías de licencia de conducir. Ver `NRM-06`.
///
/// Qué habilita cada una <b>no vive aquí</b>: vive en la matriz licencia↔vehículo, que es
/// un catálogo con vigencia (`M-02`). Cablearlo acá lo volvería inmodificable sin
/// recompilar, y la matriz oficial todavía es insumo abierto `[C]`.
/// </summary>
public enum CategoriaDeLicencia { A, B, B1, C1, C, D1, D, CE }

/// <param name="Restricciones">
/// Corrección visual, prohibición de conducción nocturna, u otras. `[C]` el catálogo que
/// usa la DNVT — insumo #23.
/// </param>
public sealed record Licencia(
    string Numero,
    CategoriaDeLicencia Categoria,
    DateOnly Vencimiento,
    IReadOnlyList<string> Restricciones);
