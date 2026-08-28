using Sigti.Dominio.M06_Solicitudes;

namespace Sigti.Aplicacion.M06_Solicitudes;

/// <summary>
/// ⚠️ <b>Catálogo provisional de motivos de rechazo.</b>
///
/// Los cuatro valores son <b>los de los criterios de `HU-014`</b>, y la propia historia los
/// declara de ejemplo: <i>«el catálogo es configurable por la institución»</i> — insumo #1,
/// `[C]`. No se inventa un quinto.
///
/// ── Por qué se cargan valores de ejemplo en vez de dejarlo vacío ─────────────
/// Porque un catálogo vacío haría <b>imposible rechazar</b>: `T-06` exige un código del
/// catálogo, y sin ninguno la jefatura no tendría cómo decir que no. Entre una lista
/// provisional marcada como tal y una función de autoridad que no se puede ejercer, la
/// primera es el error menor — y es reversible con una carga de datos, no con código.
///
/// ── Qué cambia cuando llegue el real ─────────────────────────────────────────
/// Esta clase se borra y el catálogo entra por `M-02` como parámetro con vigencia por rango
/// de fechas. <b>El dominio no cambia</b>: ya recibe el catálogo en lugar de conocerlo, que
/// es exactamente por lo que no se hizo un `enum`.
/// </summary>
public sealed class CatalogoProvisionalDeMotivosDeRechazo
{
    /// <summary>
    /// Los códigos, tal como los cita `HU-014`. Se guardan como texto y no como
    /// identificadores cortos porque <b>hoy son también lo que se muestra</b>: no hay catálogo
    /// con descripción que traducir, y un código opaco sin tabla que lo explique es peor
    /// para quien lee el expediente dentro de dos años.
    /// </summary>
    public CatalogoDeMotivosDeRechazo Vigente { get; } = new([
        "No corresponde a la función institucional",
        "Gasto no justificado",
        "Fecha no viable",
        "Duplica una misión ya autorizada",
    ]);
}
