using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Los roles del sistema — los `ACT-xx` de <c>actores-y-roles.md</c>, que es la autoridad.
///
/// <b>No se reciclan y no se inventan acá.</b> Si aparece un actor nuevo, primero entra al
/// documento y después al enum.
/// </summary>
public enum Rol
{
    /// <summary>`ACT-01` Administrador del Sistema.</summary>
    Administrador,
    /// <summary>`ACT-02` Solicitante.</summary>
    Solicitante,
    /// <summary>`ACT-03` Jefatura Inmediata.</summary>
    JefaturaInmediata,
    /// <summary>`ACT-04` Jefe de Transporte.</summary>
    JefeDeTransporte,
    /// <summary>`ACT-05` Encargado de Despacho.</summary>
    EncargadoDeDespacho,
    /// <summary>`ACT-06` Motorista.</summary>
    Motorista,
    /// <summary>`ACT-07` Encargado de Combustible.</summary>
    EncargadoDeCombustible,
    /// <summary>`ACT-08` Gerencia Administrativa.</summary>
    GerenciaAdministrativa,
    /// <summary>`ACT-09` Máxima Autoridad.</summary>
    MaximaAutoridad,
    /// <summary>`ACT-10` Encargado de Delegación.</summary>
    EncargadoDeDelegacion,
    /// <summary>`ACT-11` Encargado de Mantenimiento.</summary>
    EncargadoDeMantenimiento,
    /// <summary>`ACT-12` Auditor Interno.</summary>
    AuditorInterno,
    /// <summary>`ACT-13` Custodio del Vehículo.</summary>
    CustodioDelVehiculo,
    /// <summary>`ACT-14` Encargado de Bienes Institucionales.</summary>
    EncargadoDeBienes,
    /// <summary>`ACT-15` Verificador en Carretera.</summary>
    VerificadorEnCarretera,
}

/// <summary>
/// Hasta dónde ve un puesto — §3.1 de <c>actores-y-roles.md</c>.
///
/// ── Se otorga en la relación puesto↔rol, no en el rol ───────────────────────
/// El mismo `ACT-04` tiene alcance <see cref="Institucion"/> si el puesto es de la sede y
/// <see cref="Delegacion"/> si es regional. Ponerlo en el rol obligaría a inventar un rol
/// por región.
/// </summary>
public enum AlcanceDeDatos
{
    /// <summary>Sólo donde la persona es autor, solicitante, motorista o custodio.</summary>
    Propio,
    /// <summary>La unidad organizativa del puesto <b>y sus descendientes</b>.</summary>
    Dependencia,
    /// <summary>La delegación territorial, <b>atravesando dependencias</b>.</summary>
    Delegacion,
    /// <summary>Todo. Reservado a `ACT-08`, `ACT-09` y `ACT-12`.</summary>
    Institucion,
}

/// <summary>
/// Un rol otorgado a un puesto, con su alcance y su vigencia.
/// </summary>
/// <param name="Hasta">
/// Nulo es <b>indefinido, no eterno</b>. Mismo criterio que <see cref="AsignacionDePuesto"/>.
/// </param>
public sealed record CompetenciaDelPuesto(
    IdPuesto Puesto,
    Rol Rol,
    AlcanceDeDatos Alcance,
    DateOnly Desde,
    DateOnly? Hasta)
{
    /// <summary>Ambos extremos inclusive.</summary>
    public bool VigenteAl(DateOnly fecha) =>
        fecha >= Desde && (Hasta is null || fecha <= Hasta);
}

/// <summary>
/// Lo que una persona puede hacer en una fecha — <b>`RN-100` resuelto</b>.
///
/// ── La regla, en una línea ──────────────────────────────────────────────────
/// <i>«Los permisos efectivos de un usuario, en una fecha dada, son la unión de los roles de
/// todos los puestos que esa persona ocupa vigentes a esa fecha. No hay permisos otorgados
/// directamente a una persona. Sin excepción»</i> — §2.2.
///
/// ── Por qué la unión importa tanto ──────────────────────────────────────────
/// Porque <b>las incompatibilidades se evalúan sobre la persona, nunca sobre el puesto</b>
/// (§5.2). Una persona con tres puestos acumula las tres competencias y el sistema tiene que
/// verla como una sola; mirar puesto por puesto es exactamente cómo se cuela la acumulación
/// que la segregación existe para impedir.
/// </summary>
public sealed record CompetenciasDeLaPersona(
    IdPersona Persona,
    DateOnly FechaDelHecho,
    IReadOnlyList<IdPuesto> Puestos,
    IReadOnlyList<CompetenciaDelPuesto> Competencias)
{
    /// <summary>
    /// La persona sin ningún puesto vigente.
    ///
    /// <b>No es lo mismo que «no existe».</b> §2.3: *«una persona sin puesto vigente es un
    /// usuario sin permisos. No se borra: sus actos históricos lo referencian»*.
    /// </summary>
    public static CompetenciasDeLaPersona SinPuesto(IdPersona persona, DateOnly fecha) =>
        new(persona, fecha, [], []);

    /// <summary>Los roles distintos, que es lo que se compara.</summary>
    public IReadOnlyList<Rol> Roles => [.. Competencias.Select(c => c.Rol).Distinct().Order()];

    public bool Tiene(Rol rol) => Competencias.Any(c => c.Rol == rol);

    /// <summary>
    /// El alcance <b>más amplio</b> que la persona tiene sobre el sistema.
    ///
    /// Se toma el mayor y no el del puesto que se esté mirando: quien ocupa un puesto de sede
    /// con alcance institución no deja de verlo todo porque además ocupe uno regional.
    /// </summary>
    public AlcanceDeDatos? AlcanceMaximo =>
        Competencias.Count == 0 ? null : Competencias.Max(c => c.Alcance);

    /// <summary>Sin puesto vigente no hay permiso, aunque la cuenta exista y esté activa.</summary>
    public bool SinCompetencia => Competencias.Count == 0;
}

/// <summary>
/// Qué roles tiene cada puesto — la mitad de `M-01` <b>que sí es de SIGTI</b>.
///
/// ── La frontera con ARGOS, que no se cruza ──────────────────────────────────
/// `DP-001` y `RN-48`: la estructura de puestos y quién los ocupa son <b>propiedad de ARGOS
/// y Talento Humano</b>, y SIGTI los guarda como espejo que ninguna pantalla puede editar.
/// <see cref="Organigrama"/> es esa mitad.
///
/// <b>Qué facultades tiene cada puesto dentro de SIGTI no es de ARGOS.</b> ARGOS no sabe qué
/// es despachar un vehículo ni entregar un vale de combustible, y esperar que lo modele
/// sería pedirle que implemente nuestro dominio. Por eso esta mitad se administra acá, con
/// su propia vigencia y su propio control de acumulación.
/// </summary>
public sealed class TablaDeCompetencias(IReadOnlyList<CompetenciaDelPuesto> competencias)
{
    /// <summary>
    /// Lo que la persona puede hacer a una fecha, resolviendo puestos y roles juntos.
    ///
    /// <b>Se resuelve a la fecha del hecho</b> (`P-4`, `RN-46`): un acto de febrero se juzga
    /// con la competencia vigente en febrero, aunque hoy sea abril. Sin eso, reevaluar un
    /// expediente viejo diría que quien lo autorizó no tenía competencia, y el expediente
    /// quedaría indefendible por un artefacto del sistema.
    /// </summary>
    public CompetenciasDeLaPersona De(
        IdPersona persona, Organigrama organigrama, DateOnly fechaDelHecho)
    {
        var puestos = organigrama.PuestosDe(persona, fechaDelHecho);

        var suyas = competencias
            .Where(c => puestos.Contains(c.Puesto) && c.VigenteAl(fechaDelHecho))
            .ToList();

        return new CompetenciasDeLaPersona(persona, fechaDelHecho, puestos, suyas);
    }

    /// <summary>Las competencias vigentes de un puesto, ocupado o vacante.</summary>
    public IReadOnlyList<CompetenciaDelPuesto> DelPuesto(IdPuesto puesto, DateOnly fecha) =>
        [.. competencias.Where(c => c.Puesto == puesto && c.VigenteAl(fecha))];

    /// <summary>
    /// Qué quedaría en la persona si se le otorgara este rol a este puesto.
    ///
    /// Existe para el <b>control preventivo</b> de §5.3.A: la asignación se juzga contra lo
    /// que produciría, no contra lo que ya hay. Preguntarlo después de guardar sería
    /// detectar el conflicto cuando ya está.
    /// </summary>
    public IReadOnlyList<Rol> RolesSiSeAgrega(
        IdPersona persona, Organigrama organigrama, DateOnly fecha, Rol nuevo)
    {
        var actuales = De(persona, organigrama, fecha).Roles;
        return [.. actuales.Append(nuevo).Distinct().Order()];
    }
}
