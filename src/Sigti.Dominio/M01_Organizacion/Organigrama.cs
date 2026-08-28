using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Identidad de <b>puesto</b>, no de persona ni de rol.
///
/// Es un tipo propio por la misma razón que <see cref="IdPersona"/>: para que el
/// compilador impida pasar uno donde va el otro. Confundirlos es el error que `RN-100`
/// existe para impedir, y un <c>string</c> no lo impide.
/// </summary>
public readonly record struct IdPuesto(string Valor)
{
    public override string ToString() => Valor;
}

/// <summary>
/// Una persona ocupando un puesto durante un rango.
/// </summary>
/// <param name="Hasta">
/// Nulo es <b>indefinido, no eterno</b>: se cierra cuando la persona deja el puesto. La
/// distinción importa porque un `Hasta` obligatorio obligaría a inventar una fecha de
/// salida el día del nombramiento.
/// </param>
public sealed record AsignacionDePuesto(
    IdPersona Persona,
    IdPuesto Puesto,
    DateOnly Desde,
    DateOnly? Hasta)
{
    /// <summary>Ambos extremos inclusive: el último día de la asignación todavía cuenta.</summary>
    public bool VigenteAl(DateOnly fecha) =>
        fecha >= Desde && (Hasta is null || fecha <= Hasta);
}

/// <summary>
/// Quién ocupa qué puesto, y desde cuándo — <b>`M-01`</b>.
///
/// ── La regla, en una línea ───────────────────────────────────────────────────
/// `RN-100`: <b>el permiso se concede al puesto</b>. Una persona ejerce un permiso porque
/// ocupa un puesto que lo tiene, y por ninguna otra vía. No existe la concesión a un
/// usuario nominal.
///
/// ── Por qué esto no es burocracia de modelado ────────────────────────────────
/// `NRM-09` `[V]`: la rotación en el sector público es alta y Honduras cambió de gobierno
/// en enero de 2026. Con el permiso colgando de la persona, cada rotación obliga a
/// reconstruir a mano quién puede hacer qué — y lo que ocurre en la práctica es que se
/// copian los permisos del saliente al entrante *«para que pueda trabajar»*, arrastrando
/// toda la acumulación indebida que el saliente había juntado. <b>La segregación de
/// `RN-01` se pierde sin que nadie tome la decisión de perderla.</b>
///
/// Con el permiso en el puesto, el alta del entrante es un solo acto —ocupa el puesto— y
/// hereda exactamente la competencia definida, ni más ni menos.
///
/// ── Todo se resuelve a la FECHA DEL HECHO ────────────────────────────────────
/// `P-4` y `RN-46`. Un acto de febrero se juzga con la ocupación vigente en febrero,
/// aunque hoy sea abril. Sin eso, reevaluar un expediente viejo diría que quien lo
/// autorizó no tenía competencia — y el expediente quedaría indefendible por un artefacto
/// del sistema, no por un defecto real.
///
/// ⚠️ <b>La estructura de puestos es propiedad de ARGOS y Talento Humano</b> (`DP-001`).
/// SIGTI consume el espejo y <b>no crea puestos</b>. Esta clase resuelve la pregunta;
/// no administra el organigrama.
/// </summary>
public sealed class Organigrama(IReadOnlyList<AsignacionDePuesto> asignaciones)
{
    /// <summary>¿Ocupaba esta persona este puesto en esta fecha?</summary>
    public bool Ocupa(IdPersona persona, IdPuesto puesto, DateOnly fechaDelHecho) =>
        asignaciones.Any(a =>
            a.Persona == persona && a.Puesto == puesto && a.VigenteAl(fechaDelHecho));

    /// <summary>
    /// Los puestos que la persona ocupaba en esa fecha.
    ///
    /// <b>Puede ser más de uno</b> —frecuente en delegaciones— y sus permisos son la
    /// unión. Acumular puestos <b>no</b> levanta incompatibilidades: `RN-01` bloquea por
    /// identidad de persona, no por rol.
    ///
    /// <b>Puede ser ninguno</b>, y entonces la persona no tiene ningún permiso aunque su
    /// cuenta exista, esté activa y tenga contraseña.
    /// </summary>
    public IReadOnlyList<IdPuesto> PuestosDe(IdPersona persona, DateOnly fechaDelHecho) =>
        asignaciones
            .Where(a => a.Persona == persona && a.VigenteAl(fechaDelHecho))
            .Select(a => a.Puesto)
            .Distinct()
            .ToList();

    /// <summary>
    /// ¿El espejo sabe algo de esta persona, en cualquier fecha?
    ///
    /// ── Por qué hace falta, y no basta con <c>PuestosDe</c> ──────────────────
    /// Porque «no ocupa ningún puesto hoy» tiene <b>dos causas opuestas</b>: que la persona
    /// cesó, o que el espejo no sabe de ella —la integración no corrió, o esa dependencia
    /// todavía no se sincronizó—. Tratarlas igual haría que un espejo vacío declarara
    /// cesada a toda la institución.
    ///
    /// Es la misma distinción que <c>ConsultaDelOrganigrama.AntiguedadDelEspejoAsync</c>
    /// hace con el nulo: ausencia de dato no es dato de ausencia.
    /// </summary>
    public bool Conoce(IdPersona persona) => asignaciones.Any(a => a.Persona == persona);

    /// <summary>
    /// Quiénes ocupaban un puesto en esa fecha.
    ///
    /// Puede haber <b>dos durante un traspaso</b>: la coocupación es acotada y se registra,
    /// y existe porque el traspaso real dura días. Negarla obligaría a dejar el puesto
    /// vacante justo cuando hay más trabajo. Cada acto queda a nombre de quien lo hizo, y
    /// eso es lo que impide que el solape borre la responsabilidad.
    /// </summary>
    public IReadOnlyList<IdPersona> QuienesOcupan(IdPuesto puesto, DateOnly fechaDelHecho) =>
        asignaciones
            .Where(a => a.Puesto == puesto && a.VigenteAl(fechaDelHecho))
            .Select(a => a.Persona)
            .Distinct()
            .ToList();
}
