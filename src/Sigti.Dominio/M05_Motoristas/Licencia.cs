namespace Sigti.Dominio.M05_Motoristas;

/// <summary>
/// Las <b>nueve</b> categorías del Artículo 4 del Acuerdo 1012-2021 `[V]`. Ver `NRM-06`.
///
/// <b>`BE` no es un adorno.</b> Es «categoría B enganchada a un remolque», y sin ella un
/// pick-up que remolca una plataforma se evalúa como `B` y pasa el bloqueo duro de
/// `BD-02` — justo el caso de siniestro con responsabilidad institucional que el bloqueo
/// existe para impedir.
///
/// <b>No existe `DE`.</b> El epígrafe de remolques del Artículo 4 solo crea `BE` y `CE`.
///
/// Qué habilita cada una <b>no vive aquí</b>: vive en la matriz licencia↔vehículo, que es
/// un catálogo con vigencia (`M-02`). Cablear los umbrales acá los volvería inmodificables
/// sin recompilar, y el reglamento cambia.
/// </summary>
public enum CategoriaDeLicencia { A, B1, B, C1, C, D1, D, BE, CE }

/// <param name="Restricciones">
/// Corrección visual, prohibición de conducción nocturna, u otras. `[C]` el catálogo que
/// usa la DNVT — insumo #23.
/// </param>
public sealed record Licencia(
    string Numero,
    CategoriaDeLicencia Categoria,
    DateOnly Vencimiento,
    IReadOnlyList<string> Restricciones);
