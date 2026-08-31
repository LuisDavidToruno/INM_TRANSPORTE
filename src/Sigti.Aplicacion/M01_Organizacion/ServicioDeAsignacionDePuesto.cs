using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <param name="Roles">Los que la persona pasa a tener sumando los del puesto nuevo.</param>
/// <param name="Vigilados">
/// Los pares que quedan acumulados sin ser prohibidos — `RN-01` los admite y los vigila. Se
/// devuelven para que quien asigna los vea <b>antes</b> de confirmar, no en un reporte.
/// </param>
public sealed record AsignacionOtorgada(
    Ulid Id,
    string Persona,
    string Puesto,
    IReadOnlyList<Rol> Roles,
    IReadOnlyList<AcumulacionVigilada> Vigilados);

/// <param name="PorQue">
/// Por qué el par existe, en las palabras de la regla. <b>Va con el par y no aparte</b>:
/// «I-08» a secas obliga a ir a buscar qué es, y quien asigna decide en ese momento.
/// </param>
public sealed record AcumulacionVigilada(string Par, string Una, string Otra, string PorQue);

/// <summary>
/// Asigna una persona a un <b>puesto funcional de SIGTI</b> — Jefe de Transporte, Encargado de
/// Despacho, Custodio.
///
/// ── ⚠️ Esto NO es otorgar un permiso a una persona ──────────────────────────
/// `RNF-14` es taxativo: <i>«permisos asignados directamente a una persona: 0. El modelo no
/// ofrece la operación»</i>. Y aquí no se ofrece: <b>la competencia sigue viviendo en el
/// puesto</b>. Lo que esta operación hace es decir quién lo ocupa.
///
/// La diferencia no es formal. Cuando esa persona rota, se cierra su asignación y <b>el
/// siguiente ocupante hereda las competencias del puesto sin tocarlas</b> — que es exactamente
/// lo que `NRM-09` `[V]` protege: con el permiso colgando de la persona, cada rotación termina
/// con alguien copiando los permisos del saliente al entrante «para que pueda trabajar», y
/// arrastrando toda la acumulación indebida que el saliente había juntado.
///
/// ── Y por qué el puesto funcional no lo trae ARGOS ──────────────────────────
/// Porque ARGOS no gestiona flota. Conoce el <b>cargo del contrato</b> —«Técnico/a»,
/// «Inspector/a de Migración»— y eso no dice qué papel juega la persona en el transporte. Un
/// cargo no es un rol: 154 inspectores de migración no comparten una función de flota.
///
/// Por eso estas asignaciones son <c>Propia</c> y la sincronización no las toca.
/// </summary>
public sealed class ServicioDeAsignacionDePuesto(
    SigtiDbContext contexto, ServicioDeCompetencias competencias)
{
    public async Task<AsignacionOtorgada> AsignarAsync(
        IdPersona persona,
        IdPuesto puesto,
        DateOnly desde,
        DateOnly? hasta,
        IdPersona otorga,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (hasta is not null && hasta < desde)
            throw new BloqueoDuro("RN-100", "La asignación no puede terminar antes de empezar.");

        // ⚠️ **La persona tiene que estar en el padrón.** Asignar un puesto a alguien que el
        // organigrama no conoce produce competencias a nombre de un identificador que nadie
        // puede resolver — y el día de la auditoría, un acto sin persona detrás.
        var enElPadron = await contexto.AsignacionesDePuesto
            .AsNoTracking()
            .AnyAsync(a => a.Persona == persona.Valor, cancelacion);

        if (!enElPadron)
        {
            throw new BloqueoDuro("RN-100",
                $"El organigrama no conoce a «{persona.Valor}». Sincronice el espejo con el " +
                "sistema dueño del padrón antes de asignarle un puesto.");
        }

        var yaLoOcupa = await contexto.AsignacionesDePuesto
            .AsNoTracking()
            .AnyAsync(
                a => a.Persona == persona.Valor
                     && a.Puesto == puesto.Valor
                     && (a.Hasta == null || a.Hasta >= desde),
                cancelacion);

        if (yaLoOcupa)
        {
            throw new BloqueoDuro("RN-100",
                $"«{persona.Valor}» ya ocupa {puesto.Valor} al {desde:dd/MM/yyyy}. Dos filas " +
                "que dicen lo mismo abren una tercera pregunta —cuál manda— que nadie quiere " +
                "contestar.");
        }

        // ── El control preventivo, y en el sentido que faltaba ──────────────
        //
        // `OtorgarAsync` ya evalúa la acumulación cuando se agrega un rol a un puesto. El caso
        // espejo —**asignar a alguien un puesto que YA tiene roles**— no lo miraba nadie, y es
        // el camino por el que la incompatibilidad entra en la práctica: los puestos se crean
        // una vez y la gente rota todo el tiempo.
        var todas = await competencias.TodasAsync(cancelacion);
        var suyas = await competencias.DeLaPersonaAsync(persona, desde, cancelacion);

        var delPuesto = todas
            .Where(c => c.Puesto == puesto.Valor
                        && c.Desde <= desde
                        && (c.Hasta is null || c.Hasta >= desde))
            .ToList();

        var resultantes = suyas.Roles
            .Concat(delPuesto.Select(c => c.Rol))
            .Distinct()
            .Order()
            .ToList();

        var efecto = ReglasDeLaAsignacion.Evaluar(resultantes);

        // El mensaje nombra a la persona y el rol que rompe: «la asignación es incompatible»
        // no le dice a nadie qué hacer.
        foreach (var rol in delPuesto.Select(c => c.Rol).Distinct())
            ReglasDeLaAsignacion.Exigir(efecto, persona.Valor, rol);

        var id = Ulid.NewUlid();

        contexto.AsignacionesDePuesto.Add(new FilaDeAsignacionDePuesto
        {
            Id = id,
            Persona = persona.Valor,
            Puesto = puesto.Valor,

            // Propia: es el puesto funcional de SIGTI, que ARGOS no modela y nunca va a traer.
            Origen = OrigenDeLaAsignacion.Propia,

            Desde = desde,
            Hasta = hasta,

            // Se confirma en el acto de otorgarla: la puso una persona de esta institución, no
            // una integración.
            ConfirmadoAlUtc = momento.UtcDateTime,
        });

        await contexto.SaveChangesAsync(cancelacion);

        return new AsignacionOtorgada(
            id, persona.Valor, puesto.Valor, resultantes,
            [.. efecto.Vigilados.Select(v => new AcumulacionVigilada(
                v.Id, v.Una.ToString(), v.Otra.ToString(), v.PorQue))]);
    }

    /// <summary>
    /// Cierra una asignación. <b>No la borra</b> — `RN-04`.
    ///
    /// Los actos que la persona ejecutó bajo ese puesto siguen siendo suyos y siguen siendo
    /// válidos: `RN-100` los juzga contra la ocupación <b>a la fecha del hecho</b>. Borrar la
    /// fila haría que un expediente de febrero pareciera autorizado por alguien sin competencia
    /// — indefendible ante el auditor, y por un artefacto del sistema.
    /// </summary>
    public async Task CerrarAsync(
        Ulid id, DateOnly hasta, string motivo, DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new BloqueoDuro("RN-04",
                "Cerrar una asignación de puesto exige motivo. Sin él, dentro de un año nadie " +
                "va a poder decir si fue una rotación, una sanción o un error de carga.");
        }

        var fila = await contexto.AsignacionesDePuesto
            .SingleOrDefaultAsync(a => a.Id == id, cancelacion)
            ?? throw new BloqueoDuro("RN-100", $"No existe la asignación {id}.");

        // ⚠️ **Las espejadas no se cierran a mano** — `RN-48`. Quien dejó el cargo lo dejó en
        // el sistema dueño, y la sincronización lo va a reflejar. Cerrarla acá produciría un
        // espejo que contradice a su fuente, y la siguiente sincronización lo volvería a abrir.
        if (fila.Origen == OrigenDeLaAsignacion.Espejo)
        {
            throw new BloqueoDuro("RN-48",
                "Esa asignación es espejo del sistema dueño del padrón y SIGTI no la edita. " +
                "Ciérrela allá: la próxima sincronización la refleja acá.");
        }

        fila.Hasta = hasta;
        fila.ConfirmadoAlUtc = momento.UtcDateTime;

        await contexto.SaveChangesAsync(cancelacion);
    }
}
