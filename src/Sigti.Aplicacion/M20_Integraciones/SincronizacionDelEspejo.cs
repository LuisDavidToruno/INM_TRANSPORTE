using Microsoft.EntityFrameworkCore;

using Sigti.Datos;


namespace Sigti.Aplicacion.M20_Integraciones;

/// <param name="Personas">Cuántas trajo la fuente.</param>
/// <param name="ConPuesto">
/// Cuántas ocupan un puesto. <b>Se cuenta aparte</b>: una persona sin puesto vigente existe, se
/// espeja, y no puede ejercer ninguna competencia — y esa diferencia es la que explica por qué
/// alguien entra y no ve nada.
/// </param>
/// <param name="PuestosDistintos">
/// Cuántos cargos distintos quedaron en el espejo de puestos.
/// </param>
public sealed record ResultadoDeLaSincronizacion(
    string Fuente,
    DateTimeOffset Momento,
    int Personas,
    int ConPuesto,
    int PuestosDistintos);

/// <summary>
/// Trae el organigrama de su dueño y lo deja en el espejo de SIGTI.
///
/// ── ⚠️ Es un espejo, no un maestro — `RN-48` ────────────────────────────────
/// SIGTI no edita esto. Lo copia, lo marca con <b>cuándo lo confirmó</b>, y ninguna pantalla
/// ofrece cambiarlo. Quien necesite corregir un puesto lo corrige donde vive.
///
/// La marca de confirmación no es adorno: un espejo envejece, y `GET /organigrama/antiguedad`
/// la expone. <b>Nulo es «nunca se confirmó»</b>, deliberadamente distinto de cero días — una
/// integración que jamás corrió y una que corrió hace un minuto no se pueden mostrar igual.
/// </summary>
public sealed class SincronizacionDelEspejo(SigtiDbContext contexto, IEspejoDeOrganizacion fuente)
{
    public async Task<ResultadoDeLaSincronizacion> EjecutarAsync(
        DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        var padron = await fuente.PadronAsync(cancelacion);

        // ⚠️ **Un padrón vacío no se aplica.** Si la fuente contesta bien pero sin filas, borrar
        // el espejo dejaría a toda la institución sin competencias — y el sistema se vería
        // «funcionando» mientras nadie puede hacer nada. Es más seguro conservar lo viejo y
        // decir desde cuándo.
        if (padron.Count == 0)
        {
            throw new EspejoNoDisponible(
                "El servicio de identidad respondió sin ninguna persona. El espejo anterior se " +
                "conserva: aplicarlo dejaría a la institución entera sin competencias.");
        }

        var ahora = momento.UtcDateTime;

        await ReemplazarPuestosAsync(padron, ahora, cancelacion);
        await ReemplazarAsignacionesAsync(padron, ahora, cancelacion);

        await contexto.SaveChangesAsync(cancelacion);

        return new ResultadoDeLaSincronizacion(
            fuente.Fuente,
            momento,
            padron.Count,
            padron.Count(p => p.Puesto is not null),
            padron.Select(p => p.Puesto).Where(p => p is not null).Distinct().Count());
    }

    /// <summary>
    /// Los puestos que el padrón nombra.
    ///
    /// La unidad sale de la asignación de quien lo ocupa, que es lo que la fuente sabe. Cuando
    /// dos personas ocupan el mismo cargo en unidades distintas, gana la primera — y eso es una
    /// simplificación que hay que decir: el espejo de puestos de SIGTI supone <b>un cargo, una
    /// unidad</b>, y ARGOS no lo garantiza.
    /// </summary>
    private async Task ReemplazarPuestosAsync(
        IReadOnlyList<PersonaDelPadron> padron, DateTime ahora, CancellationToken cancelacion)
    {
        var existentes = await contexto.PuestosEspejo.ToListAsync(cancelacion);

        var deLaFuente = padron
            .Where(p => p.Puesto is not null)
            .GroupBy(p => p.Puesto!)
            .Select(g => new
            {
                Puesto = g.Key,
                Unidad = g.Select(x => x.Unidad).FirstOrDefault(u => u is not null),
                Delegacion = g.Select(x => x.Oficina).FirstOrDefault(o => o is not null),
            })
            .ToList();

        foreach (var p in deLaFuente)
        {
            var fila = existentes.FirstOrDefault(e => e.Puesto == p.Puesto);

            if (fila is null)
            {
                contexto.PuestosEspejo.Add(new FilaDePuestoEspejo
                {
                    Id = Ulid.NewUlid(),
                    Puesto = p.Puesto,
                    Denominacion = p.Puesto,
                    Unidad = p.Unidad ?? "(sin unidad declarada)",

                    // Nulo es «la cima de su rama». La fuente no entrega jerarquía de cargos.
                    Superior = null,

                    Delegacion = p.Delegacion,
                    ConfirmadoAlUtc = ahora,
                });
            }
            else
            {
                // Se reemplaza la fila para refrescar la marca de confirmación: un espejo que
                // no dice cuándo se miró por última vez no se puede juzgar.
                contexto.PuestosEspejo.Remove(fila);
                contexto.PuestosEspejo.Add(new FilaDePuestoEspejo
                {
                    Id = fila.Id,
                    Puesto = p.Puesto,
                    Denominacion = p.Puesto,
                    Unidad = p.Unidad ?? fila.Unidad,
                    Superior = fila.Superior,
                    Delegacion = p.Delegacion ?? fila.Delegacion,
                    ConfirmadoAlUtc = ahora,
                });
            }
        }
    }

    /// <summary>
    /// Quién ocupa qué, y desde cuándo.
    ///
    /// ── ⚠️ Lo que ya no está en la fuente NO se borra ───────────────────────
    /// Se le pone fecha de fin. `RN-100` juzga cada acto contra la ocupación <b>a la fecha del
    /// hecho</b>: borrar la asignación de quien se fue haría que un expediente de febrero
    /// dijera que lo autorizó alguien sin competencia — indefendible ante el auditor, y por un
    /// artefacto del sistema.
    /// </summary>
    private async Task ReemplazarAsignacionesAsync(
        IReadOnlyList<PersonaDelPadron> padron, DateTime ahora, CancellationToken cancelacion)
    {
        // ⚠️ **Solo las espejadas.** Las propias de SIGTI —el puesto funcional en la gestion
        // de flota— no vienen de esta fuente y nunca van a venir: ARGOS no gestiona flota.
        // Incluirlas aca las cerraria en la primera sincronizacion nocturna, y el sintoma
        // seria «ayer podia y hoy no» sin nada que apunte a la causa.
        var existentes = await contexto.AsignacionesDePuesto
            .Where(a => a.Origen == OrigenDeLaAsignacion.Espejo)
            .ToListAsync(cancelacion);

        var hoy = DateOnly.FromDateTime(ahora);

        var vigentes = padron.Where(p => p.Puesto is not null).ToList();

        foreach (var p in vigentes)
        {
            var fila = existentes.FirstOrDefault(
                e => e.Persona == p.Persona && e.Puesto == p.Puesto);

            if (fila is not null)
            {
                fila.ConfirmadoAlUtc = ahora;
                fila.Hasta = p.Hasta;
                continue;
            }

            contexto.AsignacionesDePuesto.Add(new FilaDeAsignacionDePuesto
            {
                Id = Ulid.NewUlid(),
                Persona = p.Persona,
                Puesto = p.Puesto!,

                // Sin fecha declarada se toma hoy. No se inventa una anterior: decir que alguien
                // ocupa el cargo desde enero cuando la fuente no lo dice haría que un acto de
                // enero se juzgue con una competencia que nadie puede demostrar.
                Desde = p.Desde ?? hoy,
                Hasta = p.Hasta,
                Origen = OrigenDeLaAsignacion.Espejo,
                ConfirmadoAlUtc = ahora,
            });
        }

        // Las que la fuente ya no trae: se cierran, no se borran.
        var enLaFuente = vigentes
            .Select(p => (p.Persona, Puesto: p.Puesto!))
            .ToHashSet();

        foreach (var fila in existentes)
        {
            if (enLaFuente.Contains((fila.Persona, fila.Puesto)) || fila.Hasta is not null)
                continue;

            fila.Hasta = hoy;
            fila.ConfirmadoAlUtc = ahora;
        }
    }
}
