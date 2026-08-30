using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <summary>
/// El puesto vigente y lo que ve — `R-1`, `R-2` y `actores-y-roles` §3.
///
/// ── Por qué el puesto y no la persona ───────────────────────────────────────
/// Los permisos se otorgan al puesto, y una persona puede ocupar varios (§2). El Jefe de
/// Transporte que además es custodio <b>ve dos raíces distintas, no una mezclada</b>: mezclarlas
/// produciría un menú que ninguno de los dos puestos tiene, y un alcance de datos que es la
/// unión de dos permisos que nadie otorgó junto.
/// </summary>
public sealed class ServicioDelPuesto(SigtiDbContext contexto)
{
    /// <summary>
    /// `PT-001` — los puestos que la persona ocupa a la fecha, con lo que cada uno le da.
    /// </summary>
    public async Task<PuestosDeLaPersona> DeLaPersonaAsync(
        IdPersona persona, DateOnly fecha, CancellationToken cancelacion = default)
    {
        var asignaciones = await contexto.AsignacionesDePuesto
            .AsNoTracking().ToListAsync(cancelacion);

        var conoce = asignaciones.Any(a => a.Persona == persona.Valor);

        var suyos = asignaciones
            .Where(a => a.Persona == persona.Valor &&
                        a.Desde <= fecha && (a.Hasta is null || a.Hasta >= fecha))
            .Select(a => new IdPuesto(a.Puesto))
            .Distinct()
            .ToList();

        var espejo = await EspejoAsync(cancelacion);
        var competencias = await contexto.Competencias.AsNoTracking().ToListAsync(cancelacion);

        var puestos = suyos.Select(p =>
        {
            var enEspejo = espejo.FirstOrDefault(e => e.Id == p);

            var suyas = competencias
                .Where(c => c.Puesto == p.Valor &&
                            c.Desde <= fecha && (c.Hasta is null || c.Hasta >= fecha))
                .Select(c => new CompetenciaEnPuesto(c.Rol, c.Alcance))
                .ToList();

            return new PuestoVigente(
                p.Valor,

                // Nulos cuando el puesto no está en el espejo. No se sustituye por el
                // identificador: eso mostraría «PUE-JEFE-TRANSPORTE» como si fuera el nombre
                // que la institución le da, y ocultaría que el espejo no lo tiene.
                enEspejo?.Denominacion,
                enEspejo?.Unidad,
                enEspejo?.Delegacion,
                enEspejo is not null,
                suyas,
                ReglasDeLaRaiz.DeTodos(suyas.Select(c => c.Rol)),

                // Los roles del puesto que el mapa de navegación no cubre. Se nombran: un rol
                // sin raíz declarada deja a su ocupante sin punto de entrada, y eso es una
                // brecha del diseño que conviene ver, no una pantalla en blanco.
                [.. suyas.Select(c => c.Rol).Where(r => ReglasDeLaRaiz.De(r) is null).Distinct()]);
        }).ToList();

        return new PuestosDeLaPersona(persona.Valor, fecha, conoce, puestos);
    }

    /// <summary>
    /// `PT-002` — lo que le toca al puesto ahora.
    ///
    /// ── `R-2`: es una bandeja de trabajo, no un tablero ─────────────────────
    /// Nadie entra a SIGTI a ver indicadores. Por eso lo que se cuenta son <b>cosas que esperan
    /// una decisión de este puesto</b>, y cada contador está atado a la raíz que lo resuelve.
    /// Un número sin destino sería justamente el tablero decorativo que la regla rechaza.
    /// </summary>
    public async Task<InicioDelPuesto?> InicioAsync(
        IdPersona persona, IdPuesto puesto, DateOnly fecha,
        CancellationToken cancelacion = default)
    {
        var suyos = await DeLaPersonaAsync(persona, fecha, cancelacion);
        var elegido = suyos.Puestos.FirstOrDefault(p => p.Puesto == puesto.Valor);

        if (elegido is null) return null;

        var roles = elegido.Competencias.Select(c => c.Rol).Distinct().ToHashSet();

        var expedientes = await contexto.Expedientes
            .AsNoTracking().Include(e => e.Transiciones).ToListAsync(cancelacion);

        var alcance = await AlcanceParaMisionesAsync(persona, puesto, elegido, cancelacion);

        // El alcance se aplica ANTES de contar. Contar sobre todo y filtrar al abrir daría un
        // número que no corresponde con la lista que después se ve, y el usuario creería que
        // algo se perdió.
        var porUnidad = await DelegacionPorUnidadAsync(cancelacion);

        var visibles = expedientes
            .Where(e => ReglasDelAlcance.Alcanza(alcance, ParaAlcance(e, porUnidad)))
            .Select(e => (Fila: e, Estado: e.Transiciones.MaxBy(t => t.Orden)?.Destino))
            .ToList();

        int Cuantas(EstadoDeMision estado) => visibles.Count(x => x.Estado == estado);

        var pendientes = new List<PendienteDelPuesto>();

        if (roles.Contains(Rol.JefaturaInmediata))
            pendientes.Add(new("PT-013", "esperan su autorización", Cuantas(EstadoDeMision.Solicitada)));

        if (roles.Contains(Rol.JefeDeTransporte))
            pendientes.Add(new("PT-025", "aprobadas sin programar", Cuantas(EstadoDeMision.Aprobada)));

        if (roles.Contains(Rol.EncargadoDeDespacho))
            pendientes.Add(new("PT-038", "programadas por despachar", Cuantas(EstadoDeMision.Programada)));

        if (roles.Contains(Rol.GerenciaAdministrativa) || roles.Contains(Rol.MaximaAutoridad))
            pendientes.Add(new(null, "liquidadas por cerrar", Cuantas(EstadoDeMision.Liquidada)));

        // La bandeja de §5.3.B.3 no depende del rol: le llega a quien fue escalada.
        var tareas = await contexto.Tareas
            .AsNoTracking()
            .CountAsync(t => t.Estado == EstadoDeTarea.Pendiente && t.PuestoDestino == puesto.Valor,
                        cancelacion);

        if (tareas > 0)
            pendientes.Add(new("PT-003", "tareas escaladas a este puesto", tareas));

        return new InicioDelPuesto(elegido, pendientes, alcance.SePudoResolver, alcance.PorQueNo);
    }

    /// <summary>
    /// `PT-005` — el buscador, <b>con el alcance de datos aplicado</b>.
    ///
    /// ── Lo que el filtro no hace ────────────────────────────────────────────
    /// No recorta después de traer: filtra sobre el conjunto y devuelve <b>cuántos quedaron
    /// fuera</b>. Un buscador que oculta sin decir que oculta hace creer que el expediente no
    /// existe, y eso manda a la gente a crear uno duplicado.
    /// </summary>
    public async Task<Busqueda?> BuscarAsync(
        IdPersona persona, IdPuesto puesto, string? texto, DateOnly fecha,
        CancellationToken cancelacion = default)
    {
        var suyos = await DeLaPersonaAsync(persona, fecha, cancelacion);
        var elegido = suyos.Puestos.FirstOrDefault(p => p.Puesto == puesto.Valor);

        if (elegido is null) return null;

        var alcance = await AlcanceParaMisionesAsync(persona, puesto, elegido, cancelacion);

        var todos = await contexto.Expedientes
            .AsNoTracking().Include(e => e.Transiciones).ToListAsync(cancelacion);

        var porUnidad = await DelegacionPorUnidadAsync(cancelacion);
        var dentro = todos
            .Where(e => ReglasDelAlcance.Alcanza(alcance, ParaAlcance(e, porUnidad)))
            .ToList();

        var busqueda = texto?.Trim();

        var encontrados = string.IsNullOrEmpty(busqueda)
            ? dentro
            : [.. dentro.Where(e =>
                ConsultaDeMisiones.FolioProvisional(e.Id).Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                e.Destino.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                e.ObjetoDelTraslado.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                e.Dependencia.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                e.SolicitanteDeDerecho.Contains(busqueda, StringComparison.OrdinalIgnoreCase))];

        return new Busqueda(
            elegido,
            alcance.Nivel,
            alcance.SePudoResolver,
            alcance.PorQueNo,

            // Cuántos hay en total fuera del alcance. **No se dice cuáles**: el número es
            // información de control —«hay más de lo que ve»— y los datos serían el permiso
            // que no tiene.
            todos.Count - dentro.Count,

            [.. encontrados
                .OrderByDescending(e => e.Salida)
                .Take(50)
                .Select(e => new ResultadoDeBusqueda(
                    e.Id.ToString(),
                    ConsultaDeMisiones.FolioProvisional(e.Id),
                    e.Transiciones.MaxBy(t => t.Orden)?.Destino.ToString() ?? "sin diario",
                    e.Dependencia,
                    e.Destino,
                    e.ObjetoDelTraslado,
                    e.SolicitanteDeDerecho,
                    e.Salida))],

            encontrados.Count);
    }

    // ── Lo compartido ───────────────────────────────────────────────────────

    /// <summary>
    /// El alcance del puesto sobre expedientes de misión.
    ///
    /// ── El corte por objeto de §3.3 no está modelado ────────────────────────
    /// §3.3 dice que un puesto puede tener alcance `Dependencia` sobre misiones e `Institucion`
    /// sobre vehículos, y la competencia no registra sobre qué objeto rige. Mientras tanto se
    /// toma el <b>mayor</b> alcance del puesto, y eso es más permisivo de lo que la regla
    /// quiere: `ACT-11` tiene institución sobre vehículos y <b>no debe ver solicitudes</b>.
    /// Queda anotado como insumo en vez de resuelto con una tabla inventada.
    /// </summary>
    private async Task<AlcanceResuelto> AlcanceParaMisionesAsync(
        IdPersona persona, IdPuesto puesto, PuestoVigente elegido, CancellationToken cancelacion)
    {
        if (elegido.Competencias.Count == 0)
            return AlcanceResuelto.Nada(AlcanceDeDatos.Propio,
                $"El puesto {puesto} no tiene ninguna competencia vigente. Un puesto sin " +
                "competencia es un puesto sin permisos, y no se le muestra nada.");

        var mayor = elegido.Competencias.Max(c => c.Alcance);
        var espejo = await EspejoAsync(cancelacion);

        return ReglasDelAlcance.Resolver(puesto, mayor, espejo).De(persona);
    }

    /// <summary>Unidad → delegación, derivado del espejo. Nulo cuando la unidad no está.</summary>
    private async Task<IReadOnlyDictionary<string, string?>> DelegacionPorUnidadAsync(
        CancellationToken cancelacion)
    {
        var espejo = await EspejoAsync(cancelacion);

        return espejo
            .GroupBy(p => p.Unidad, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.Delegacion).FirstOrDefault(d => d is not null),
                StringComparer.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyList<Puesto>> EspejoAsync(CancellationToken cancelacion)
    {
        var filas = await contexto.PuestosEspejo.AsNoTracking().ToListAsync(cancelacion);

        return
        [
            .. filas.Select(f => new Puesto(
                new IdPuesto(f.Puesto), f.Denominacion, f.Unidad,
                f.Superior is null ? null : new IdPuesto(f.Superior),
                f.Delegacion)),
        ];
    }

    /// <summary>
    /// ⚠️ <b>La delegación del expediente no existe como campo</b>: se deriva de su dependencia
    /// buscando en el espejo un puesto de esa unidad. Cuando la unidad no está en el espejo la
    /// delegación queda <b>nula</b>, y nulo excluye — no incluye.
    /// </summary>
    private static ExpedienteParaAlcance ParaAlcance(
        Datos.M07_ProgramacionYDespacho.FilaDeExpediente e,
        IReadOnlyDictionary<string, string?> delegacionPorUnidad) =>
        new(e.Id.ToString(),
            e.Dependencia,
            delegacionPorUnidad.GetValueOrDefault(e.Dependencia),
            new IdPersona(e.CapturadaPor),
            new IdPersona(e.SolicitanteDeDerecho),

            // Vacíos: el conductor de la reserva se guarda como identificador de conductor y el
            // alcance razona sobre identificadores de persona. Ese puente no existe todavía, y
            // suponerlo haría que un motorista viera misiones que no son suyas.
            [],
            []);

}

/// <param name="Conocida">
/// Si la persona existe en el organigrama. <b>Falso no es «no tiene puestos hoy»</b>: es «nunca
/// tuvo ninguno». Las dos muestran una lista vacía y sólo una significa que alguien escribió
/// mal el identificador.
/// </param>
public sealed record PuestosDeLaPersona(
    string Persona, DateOnly Fecha, bool Conocida, IReadOnlyList<PuestoVigente> Puestos);

/// <param name="EnElEspejo">
/// Si el puesto está en el espejo del organigrama. Cuando es falso, la denominación, la unidad y
/// la delegación son nulas y <b>el alcance no se puede resolver</b>.
/// </param>
/// <param name="RolesSinRaiz">
/// Roles del puesto que el mapa de navegación no cubre. Se nombran en vez de callarse: dejan a
/// su ocupante sin punto de entrada.
/// </param>
public sealed record PuestoVigente(
    string Puesto,
    string? Denominacion,
    string? Unidad,
    string? Delegacion,
    bool EnElEspejo,
    IReadOnlyList<CompetenciaEnPuesto> Competencias,
    IReadOnlyList<Raiz> Raices,
    IReadOnlyList<Rol> RolesSinRaiz);

public sealed record CompetenciaEnPuesto(Rol Rol, AlcanceDeDatos Alcance);

/// <param name="Pantalla">Nulo cuando el mapa describe la raíz sin darle identificador.</param>
public sealed record PendienteDelPuesto(string? Pantalla, string Que, int Cuantos);

public sealed record InicioDelPuesto(
    PuestoVigente Puesto,
    IReadOnlyList<PendienteDelPuesto> Pendientes,
    bool AlcanceResuelto,
    string? PorQueNoSeResolvio);

/// <param name="FueraDelAlcance">
/// Cuántos expedientes quedaron fuera. <b>El número sí, los datos no</b>: saber que hay más es
/// control interno; verlos sería el permiso que no se tiene.
/// </param>
public sealed record Busqueda(
    PuestoVigente Puesto,
    AlcanceDeDatos Nivel,
    bool AlcanceResuelto,
    string? PorQueNoSeResolvio,
    int FueraDelAlcance,
    IReadOnlyList<ResultadoDeBusqueda> Resultados,
    int Total);

public sealed record ResultadoDeBusqueda(
    string Mision,
    string Folio,
    string Estado,
    string Dependencia,
    string Destino,
    string ObjetoDelTraslado,
    string SolicitanteDeDerecho,
    DateOnly Salida);
