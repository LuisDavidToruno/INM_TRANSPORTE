using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Datos;

/// <summary>
/// Quien conduce — <b>`M-05`</b>.
///
/// ── Por qué «conductor» y no «motorista» ─────────────────────────────────────
/// `RN-57` verifica la habilitación sobre <b>quien efectivamente conduce</b>, sea o no
/// del padrón: el funcionario con vehículo asignado no se exceptúa. Llamar a la tabla
/// «motorista» habría invitado a excluir a los demás, que es exactamente el hueco por
/// donde se cuela un siniestro con responsabilidad institucional.
///
/// ── La licencia va en columnas, no en una tabla aparte ───────────────────────
/// Por ahora. Un historial de licencias con renovaciones y adjuntos es `M-05` completo;
/// lo que `BD-02` necesita hoy son cuatro datos, y partirlos antes de necesitarlo
/// produciría un `JOIN` por cada evaluación sin ganar nada.
/// </summary>
public sealed class FilaDeConductor
{
    public required Ulid Id { get; init; }

    public required string Nombre { get; init; }

    /// <summary>
    /// Si figura en el padrón de motoristas de la institución.
    ///
    /// <b>No decide si puede conducir</b> — eso lo decide la licencia. Sirve para el
    /// reporte y para saber a quién le corresponde el puesto, no para exceptuar a nadie
    /// del bloqueo (`RN-57`).
    /// </summary>
    public required bool EsDelPadron { get; init; }

    public required string NumeroDeLicencia { get; init; }

    /// <summary>Una de las <b>nueve</b> del Artículo 4 del Acuerdo 1012-2021.</summary>
    public required CategoriaDeLicencia Categoria { get; init; }

    /// <summary>
    /// `BD-02` lo mide contra <b>todo el rango</b> de la misión, holgura incluida — no
    /// contra el día de salida (`RN-10`).
    /// </summary>
    public required DateOnly VenceLicencia { get; init; }

    /// <summary>
    /// Separadas por `;`. Es dato de salud: el despachador ve <b>que existe una
    /// restricción operativa aplicable</b>, no el diagnóstico (`RN-11`, `RN-52`).
    ///
    /// Van en una columna y no en tabla aparte por la misma razón que la licencia, y
    /// además porque el catálogo oficial de la DNVT <b>no existe</b> (insumo #42): fijar
    /// un esquema para clasificarlas antes de saber qué códigos hay sería adivinar.
    /// </summary>
    public string? Restricciones { get; init; }

    /// <summary>La licencia que `BD-02` y `BD-12` evalúan.</summary>
    public Licencia Licencia() => new(
        NumeroDeLicencia,
        Categoria,
        VenceLicencia,
        string.IsNullOrWhiteSpace(Restricciones)
            ? []
            : Restricciones.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
