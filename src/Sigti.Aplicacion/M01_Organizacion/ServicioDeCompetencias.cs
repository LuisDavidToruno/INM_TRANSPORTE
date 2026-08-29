using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <summary>
/// Otorga y quita competencias, y responde qué puede hacer una persona — `PT-096` y `PT-097`.
///
/// ── Por qué el otorgamiento pasa por acá y no por un CRUD ───────────────────
/// Porque **el control preventivo de §5.3.A es obligatorio**: *«si la asignación produce en una
/// persona una acumulación incompatible de carácter absoluto, se rechaza la asignación»*. Un
/// repositorio genérico que guarda filas dejaría esa evaluación en manos de quien llame, y el
/// día que alguien la olvide el sistema pierde su control más importante sin que nada falle.
///
/// ── Y por qué se evalúa sobre TODAS las personas del puesto ─────────────────
/// La incompatibilidad se juzga **sobre la persona, nunca sobre el puesto** (§5.2). Un puesto
/// puede estar coocupado durante un traspaso, y otorgarle un rol afecta a las dos personas: si
/// para una de ellas la acumulación resulta absoluta, la asignación se rechaza.
/// </summary>
public sealed class ServicioDeCompetencias(SigtiDbContext contexto)
{
    /// <summary>
    /// Otorga un rol a un puesto, con su alcance y su vigencia.
    ///
    /// <b>Rechaza si produce una acumulación absoluta en cualquiera de sus ocupantes.</b> Si la
    /// acumulación es sólo por expediente, la guarda y **deja constancia de los pares
    /// vigilados** — que es lo que alimenta el tablero de `ACT-08` y `ACT-12`.
    /// </summary>
    public async Task<EfectoDeLaAsignacion> OtorgarAsync(
        Ulid id,
        IdPuesto puesto,
        Rol rol,
        AlcanceDeDatos alcance,
        DateOnly desde,
        DateOnly? hasta,
        IdPersona otorga,
        CancellationToken cancelacion = default)
    {
        if (hasta is not null && hasta < desde)
        {
            throw new BloqueoDuro("RN-100",
                "La competencia no puede terminar antes de empezar.");
        }

        var organigrama = await OrganigramaAsync(cancelacion);
        var tabla = await TablaAsync(cancelacion);

        // Ya otorgado y vigente: repetirlo produciría dos filas que dicen lo mismo y una
        // tercera pregunta —cuál manda— que nadie quiere contestar.
        var yaLoTiene = tabla.DelPuesto(puesto, desde).Any(c => c.Rol == rol);

        if (yaLoTiene)
        {
            throw new BloqueoDuro("RN-100",
                $"El puesto {puesto} ya tiene {rol} vigente al {desde:dd/MM/yyyy}. " +
                "Para cambiar el alcance, cierre la competencia actual y otorgue la nueva.");
        }

        // Se evalúa contra CADA ocupante: el puesto puede estar coocupado en un traspaso, y la
        // acumulación es de la persona.
        var ocupantes = organigrama.QuienesOcupan(puesto, desde);

        var peor = new EfectoDeLaAsignacion([rol], [], []);

        foreach (var persona in ocupantes)
        {
            var resultantes = tabla.RolesSiSeAgrega(persona, organigrama, desde, rol);
            var efecto = ReglasDeLaAsignacion.Evaluar(resultantes);

            // El primer rechazo corta: el mensaje nombra a quién le resulta incompatible, que
            // es lo que dice qué hacer. «La asignación es incompatible» no le sirve a nadie.
            ReglasDeLaAsignacion.Exigir(efecto, persona.Valor, rol);

            if (efecto.Vigilados.Count > peor.Vigilados.Count) peor = efecto;
        }

        // **Un puesto vacante también recibe competencias.** El puesto existe aunque esté
        // vacío (§2.2), y esperar a que alguien lo ocupe para configurarlo obligaría a
        // configurarlo con prisa el día del nombramiento.
        if (ocupantes.Count == 0)
        {
            peor = ReglasDeLaAsignacion.Evaluar(
                [.. tabla.DelPuesto(puesto, desde).Select(c => c.Rol).Append(rol).Distinct()]);

            ReglasDeLaAsignacion.Exigir(peor, $"el puesto vacante {puesto}", rol);
        }

        contexto.Competencias.Add(new FilaDeCompetencia
        {
            Id = id,
            Puesto = puesto.Valor,
            Rol = rol,
            Alcance = alcance,
            Desde = desde,
            Hasta = hasta,
            Otorga = otorga.Valor,

            // Nulo es «no quedó vigilada», no «no se evaluó»: la evaluación es obligatoria.
            ParesVigilados = peor.Vigilados.Count == 0
                ? null
                : string.Join(", ", peor.Vigilados.Select(v => v.Id).Distinct()),
        });

        await contexto.SaveChangesAsync(cancelacion);
        return peor;
    }

    /// <summary>
    /// Cierra una competencia con fecha.
    ///
    /// <b>No la borra.</b> `P-4`: un acto de febrero se juzga con la competencia vigente en
    /// febrero, y una fila borrada haría que ese expediente quedara sin respaldo de
    /// competencia — indefendible por un artefacto del sistema.
    /// </summary>
    public async Task CerrarAsync(Ulid id, DateOnly hasta, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Competencias.FirstOrDefaultAsync(c => c.Id == id, cancelacion)
            ?? throw new BloqueoDuro("RN-100", $"No existe la competencia {id}.");

        if (hasta < fila.Desde)
        {
            throw new BloqueoDuro("RN-100",
                $"La competencia rige desde el {fila.Desde:dd/MM/yyyy} y no puede cerrarse antes.");
        }

        fila.Hasta = hasta;
        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>Lo que una persona puede hacer a una fecha — la unión de todos sus puestos.</summary>
    public async Task<CompetenciasDeLaPersona> DeLaPersonaAsync(
        IdPersona persona, DateOnly fechaDelHecho, CancellationToken cancelacion = default)
    {
        var organigrama = await OrganigramaAsync(cancelacion);
        var tabla = await TablaAsync(cancelacion);

        return tabla.De(persona, organigrama, fechaDelHecho);
    }

    /// <summary>Todas las filas, para el padrón de `PT-096`.</summary>
    public async Task<IReadOnlyList<FilaDeCompetencia>> TodasAsync(
        CancellationToken cancelacion = default) =>
        await contexto.Competencias.AsNoTracking().OrderBy(c => c.Puesto).ToListAsync(cancelacion);

    private async Task<Organigrama> OrganigramaAsync(CancellationToken cancelacion)
    {
        var filas = await contexto.AsignacionesDePuesto.AsNoTracking().ToListAsync(cancelacion);

        return new Organigrama(
        [
            .. filas.Select(f => new AsignacionDePuesto(
                new IdPersona(f.Persona), new IdPuesto(f.Puesto), f.Desde, f.Hasta)),
        ]);
    }

    /// <summary>
    /// La tabla completa, no filtrada por fecha.
    ///
    /// Misma razón que <c>ConsultaDelOrganigrama</c>: `RN-100` resuelve **a la fecha del
    /// hecho**, y filtrar en SQL por «vigentes hoy» impediría reevaluar un expediente de
    /// febrero. Son cientos de filas, no millones.
    /// </summary>
    private async Task<TablaDeCompetencias> TablaAsync(CancellationToken cancelacion)
    {
        var filas = await contexto.Competencias.AsNoTracking().ToListAsync(cancelacion);

        return new TablaDeCompetencias(
        [
            .. filas.Select(f => new CompetenciaDelPuesto(
                new IdPuesto(f.Puesto), f.Rol, f.Alcance, f.Desde, f.Hasta)),
        ]);
    }
}
