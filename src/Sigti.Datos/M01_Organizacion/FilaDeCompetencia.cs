using Sigti.Dominio.M01_Organizacion;

namespace Sigti.Datos;

/// <summary>
/// Un rol otorgado a un puesto — <b>maestro, no espejo</b>.
///
/// ── La diferencia con <see cref="FilaDeAsignacionDePuesto"/>, que importa ────
/// La asignación de puesto es <b>espejo de ARGOS</b>: se puebla por integración y ninguna
/// pantalla de SIGTI puede editarla (`RN-48`, `DP-001`). Esta tabla es lo contrario:
/// <b>qué facultades tiene cada puesto dentro de SIGTI es nuestro</b>, porque ARGOS no sabe
/// qué es despachar un vehículo ni entregar un vale, y esperar que lo modele sería pedirle
/// que implemente nuestro dominio.
///
/// Por eso ésta sí tiene endpoint de escritura, y la otra no.
///
/// ── Por qué lleva vigencia y no se borra ────────────────────────────────────
/// `P-4` y `RN-46`: un acto de febrero se juzga con la competencia vigente en febrero. Borrar
/// la fila al quitar el rol haría que reevaluar un expediente viejo dijera que quien lo
/// autorizó no tenía competencia — y el expediente quedaría indefendible por un artefacto del
/// sistema, no por un defecto real. <b>Se cierra con <c>Hasta</c>; no se elimina.</b>
/// </summary>
public sealed class FilaDeCompetencia
{
    public required Ulid Id { get; init; }

    public required string Puesto { get; init; }

    /// <summary>El `ACT-xx`, guardado por nombre del enum y no por número.</summary>
    public required Rol Rol { get; init; }

    /// <summary>
    /// Hasta dónde ve. <b>Se otorga acá y no en el rol</b>: el mismo `ACT-04` tiene alcance
    /// institución si el puesto es de sede y delegación si es regional.
    /// </summary>
    public required AlcanceDeDatos Alcance { get; init; }

    public required DateOnly Desde { get; init; }

    /// <summary>Nulo es <b>indefinido, no eterno</b>.</summary>
    public DateOnly? Hasta { get; set; }

    /// <summary>Quién otorgó el rol. El otorgamiento es un acto, y tiene autor.</summary>
    public required string Otorga { get; init; }

    /// <summary>
    /// Si al otorgarlo quedó una acumulación vigilada, y cuál.
    ///
    /// <b>Nulo es «no quedó vigilada»</b>, no «no se evaluó»: la evaluación es obligatoria al
    /// otorgar. Se guarda el resultado para que el tablero de `ACT-08` y `ACT-12` no tenga que
    /// recalcular la tabla entera de la institución en cada carga.
    /// </summary>
    public string? ParesVigilados { get; set; }
}
