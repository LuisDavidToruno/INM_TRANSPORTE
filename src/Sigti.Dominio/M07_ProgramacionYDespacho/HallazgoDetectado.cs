namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Un criterio de `CERRADA_CON_HALLAZGO` que se cumplió, con el caso que lo demuestra.
///
/// <b>La lista de criterios es cerrada</b> — `H-01` a `H-13` en `orden-de-mision.md` §7.2.
/// Cerrada <b>para una misión concreta</b>: nadie inventa un criterio al cerrar, ni
/// desactiva uno para el caso que tiene delante. El catálogo sí se amplía por
/// configuración, y toda regla que produzca hallazgo tiene que figurar en él (`HB1-15`).
/// </summary>
/// <param name="Criterio">
/// El identificador `H-nn`. <b>No se valida contra un enum</b> a propósito: el catálogo es
/// parámetro con vigencia (`RN-39`), y cablearlo aquí lo volvería a fijar en código —
/// exactamente lo que la premisa rectora 6 prohíbe.
/// </param>
/// <param name="Detalle">
/// El caso concreto: <i>«diferencia de caja de L 400 sin explicar»</i>, no <i>«hay una
/// diferencia»</i>. Un hallazgo sin el hecho que lo produjo no se puede seguir, y seguirlo
/// es para lo que existe el estado.
/// </param>
public sealed record HallazgoDetectado(string Criterio, string Detalle);
