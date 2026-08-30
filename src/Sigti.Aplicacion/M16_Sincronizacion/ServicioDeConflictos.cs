using Microsoft.EntityFrameworkCore;

using Sigti.Datos;
using Sigti.Datos.M16_Sincronizacion;
using Sigti.Dominio.M16_Sincronizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M16_Sincronizacion;

/// <summary>
/// La cola de conflictos — `RN-45`, `PT-053`.
///
/// ── Por qué existe, y qué había antes ───────────────────────────────────────
/// El servicio de sincronización ya <b>detectaba</b> las divergencias y las devolvía como
/// rechazos con un motivo legible. El comentario de <c>HechoRechazado</c> decía: <i>«el motivo
/// tiene que ser legible: alguien va a leerlo en una cola de conflictos»</i>.
///
/// <b>La cola no existía.</b> El rechazo viajaba en la respuesta HTTP y desaparecía en cuanto el
/// dispositivo la procesaba — así que el hecho capturado en campo <b>se perdía</b>, que es
/// justamente lo que `RN-45` existe para impedir: <i>«ambas versiones deben conservarse»</i>.
/// </summary>
public sealed class ServicioDeConflictos(SigtiDbContext contexto)
{
    /// <summary>
    /// Encola una divergencia. <b>Idempotente</b>: el mismo reintento no se encola dos veces.
    ///
    /// El dispositivo que no supo si el servidor recibió va a reenviar, y un conflicto duplicado
    /// obligaría a decidir dos veces sobre el mismo hecho.
    /// </summary>
    public async Task<Ulid?> EncolarAsync(
        Ulid expediente, string transicion, string campo, Ulid idDeCaptura,
        VersionEnConflicto delServidor, VersionEnConflicto deCampo,
        CancellationToken cancelacion = default)
    {
        var yaEsta = await contexto.ConflictosDeSincronizacion
            .AnyAsync(c => c.IdDeCaptura == idDeCaptura && c.Campo == campo, cancelacion);

        if (yaEsta) return null;

        var id = Ulid.NewUlid();

        contexto.ConflictosDeSincronizacion.Add(new FilaDeConflicto
        {
            Id = id,
            ExpedienteId = expediente,
            Transicion = transicion,
            Campo = campo,
            IdDeCaptura = idDeCaptura,

            ValorDelServidor = delServidor.Valor,
            CapturadaPorServidor = delServidor.CapturadaPor.Valor,
            OcurrioServidorUtc = delServidor.OcurrioEl.UtcDateTime,
            RegistradoServidorUtc = delServidor.RegistradoEl.UtcDateTime,
            DispositivoDelServidor = delServidor.Dispositivo,
            FotoDelServidor = delServidor.Foto,

            ValorDeCampo = deCampo.Valor,
            CapturadaPorCampo = deCampo.CapturadaPor.Valor,
            OcurrioCampoUtc = deCampo.OcurrioEl.UtcDateTime,
            RegistradoCampoUtc = deCampo.RegistradoEl.UtcDateTime,
            DispositivoDeCampo = deCampo.Dispositivo,
            FotoDeCampo = deCampo.Foto,

            Estado = EstadoDelConflicto.Pendiente,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// La cola, ordenada por impacto y después por antigüedad.
    /// </summary>
    /// <param name="expediente">
    /// Acota a una misión. Nulo trae toda la cola — que es la vista de `PT-053`.
    /// </param>
    public async Task<IReadOnlyList<ConflictoDeSincronizacion>> ColaAsync(
        Ulid? expediente = null, bool soloPendientes = true,
        CancellationToken cancelacion = default)
    {
        var consulta = contexto.ConflictosDeSincronizacion.AsNoTracking();

        if (expediente is { } id) consulta = consulta.Where(c => c.ExpedienteId == id);
        if (soloPendientes)
            consulta = consulta.Where(c => c.Estado == EstadoDelConflicto.Pendiente);

        var filas = await consulta.ToListAsync(cancelacion);

        return ReglasDelConflicto.Ordenar(filas.Select(Dominio));
    }

    /// <summary>
    /// Resuelve uno. <b>No edita nada</b>: elige cuál de las dos versiones describe lo que pasó.
    ///
    /// La versión descartada <b>queda íntegra y consultable</b>, vinculada a la decisión que la
    /// descartó (`RN-45` punto 5). No se borra ni se sobrescribe: para eso están las dos
    /// columnas.
    /// </summary>
    public async Task ResolverAsync(
        Ulid conflicto, OrigenElegido seToma, string motivo, IdPersona resuelve,
        DateTimeOffset momento, string? criterioDelLote = null,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.ConflictosDeSincronizacion
            .SingleOrDefaultAsync(c => c.Id == conflicto, cancelacion)
            ?? throw new ConflictoNoEncontrado(conflicto);

        ReglasDelConflicto.ExigirPendiente(Dominio(fila));
        ReglasDelConflicto.ExigirMotivo(motivo);

        fila.Estado = EstadoDelConflicto.Resuelto;
        fila.SeTomo = seToma;
        fila.Motivo = motivo.Trim();
        fila.Resuelve = resuelve.Valor;
        fila.ResueltoUtc = momento.UtcDateTime;
        fila.CriterioDelLote = criterioDelLote;

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// `PT-055` — resolución por lote con criterio declarado.
    ///
    /// ── Y lo que el lote nunca toca ─────────────────────────────────────────
    /// Odómetro, monto y autorización quedan fuera <b>siempre</b>, y el resultado los enumera.
    /// Un lote que dice haber resuelto «todo» sin mencionar las exclusiones hace creer que la
    /// cola quedó vacía, y los que frenan liquidaciones siguen ahí sin que nadie los mire.
    /// </summary>
    public async Task<ResultadoDelLote> ResolverLoteAsync(
        Ulid expediente, OrigenElegido seToma, string criterio, IdPersona resuelve,
        DateTimeOffset momento, CancellationToken cancelacion = default)
    {
        ReglasDelConflicto.ExigirCriterioDelLote(criterio);

        var candidatos = await ColaAsync(expediente, soloPendientes: true, cancelacion);
        var reparto = ReglasDelConflicto.Repartir(candidatos);

        foreach (var c in reparto.EnElLote)
            await ResolverAsync(c.Id, seToma, criterio.Trim(), resuelve, momento,
                                criterioDelLote: criterio.Trim(), cancelacion);

        return new ResultadoDelLote(reparto.EnElLote.Count, reparto.FueraDelLote);
    }

    /// <summary>
    /// Cuántos conflictos pendientes tiene una misión. <b>`BD-08` lo consulta para liquidar.</b>
    /// </summary>
    public Task<int> PendientesDeAsync(Ulid expediente, CancellationToken cancelacion = default) =>
        contexto.ConflictosDeSincronizacion
            .CountAsync(c => c.ExpedienteId == expediente &&
                             c.Estado == EstadoDelConflicto.Pendiente, cancelacion);

    /// <summary>
    /// El reporte de `RN-45` punto 6: conflictos por dispositivo.
    ///
    /// <i>«Un dispositivo que genera conflictos con frecuencia es un problema a corregir, no un
    /// hecho a tolerar.»</i>
    /// </summary>
    public async Task<IReadOnlyList<ConflictosPorDispositivo>> PorDispositivoAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.ConflictosDeSincronizacion
            .AsNoTracking().ToListAsync(cancelacion);

        return
        [
            .. filas
                .GroupBy(c => c.DispositivoDeCampo)
                .Select(g => new ConflictosPorDispositivo(
                    // Nulo es «el hecho no dijo de qué equipo vino», y se muestra como tal: es
                    // un dato faltante del cliente, no un dispositivo llamado «desconocido».
                    g.Key,
                    g.Count(),
                    g.Count(c => c.Estado == EstadoDelConflicto.Pendiente),
                    g.Count(c => ReglasDelConflicto.ImpactoDe(c.Campo) == ImpactoDelConflicto.Alto)))
                .OrderByDescending(d => d.Pendientes)
                .ThenByDescending(d => d.Total),
        ];
    }

    /// <summary>
    /// Lo que está esperando un registro anterior — `HU-067`, para `PT-052`.
    ///
    /// <b>No son conflictos y no se mezclan con ellos.</b> Un retenido se resuelve solo cuando
    /// llega el que falta; un conflicto espera a que una persona decida. Ponerlos juntos haría
    /// que alguien intentara «resolver» un hueco de orden, que no tiene nada que decidir.
    /// </summary>
    public async Task<IReadOnlyList<Retenido>> RetenidosAsync(
        DateTimeOffset ahora, CancellationToken cancelacion = default)
    {
        var filas = await contexto.HechosRetenidos.AsNoTracking().ToListAsync(cancelacion);

        return
        [
            .. filas
                .Select(r => new Retenido(
                    r.IdDeCaptura.ToString(),
                    r.EsperaExpediente.ToString(),
                    r.Transicion,
                    r.Ejecuta,
                    r.Dispositivo,
                    new DateTimeOffset(r.OcurridoEnUtc, TimeSpan.Zero)
                        .ToOffset(TimeSpan.FromMinutes(r.DesfaseMinutos)),
                    new DateTimeOffset(r.RetenidoUtc, TimeSpan.Zero),
                    Math.Max(0, (int)(ahora - new DateTimeOffset(r.RetenidoUtc, TimeSpan.Zero)).TotalDays),
                    r.Intentos))
                .OrderByDescending(r => r.Intentos)
                .ThenBy(r => r.RetenidoEl),
        ];
    }

    private static ConflictoDeSincronizacion Dominio(FilaDeConflicto f) =>
        new(f.Id, f.ExpedienteId, f.Transicion, f.Campo,
            new VersionEnConflicto(
                f.ValorDelServidor, new IdPersona(f.CapturadaPorServidor),
                new DateTimeOffset(f.OcurrioServidorUtc, TimeSpan.Zero),
                new DateTimeOffset(f.RegistradoServidorUtc, TimeSpan.Zero),
                f.DispositivoDelServidor, f.FotoDelServidor),
            new VersionEnConflicto(
                f.ValorDeCampo, new IdPersona(f.CapturadaPorCampo),
                new DateTimeOffset(f.OcurrioCampoUtc, TimeSpan.Zero),
                new DateTimeOffset(f.RegistradoCampoUtc, TimeSpan.Zero),
                f.DispositivoDeCampo, f.FotoDeCampo),
            f.Estado,
            f.SeTomo is { } tomo && f.Motivo is { } motivo && f.Resuelve is { } quien &&
            f.ResueltoUtc is { } cuando
                ? new ResolucionDelConflicto(
                    tomo, motivo, new IdPersona(quien),
                    new DateTimeOffset(cuando, TimeSpan.Zero), f.CriterioDelLote)
                : null);
}

public sealed class ConflictoNoEncontrado(Ulid id)
    : Exception($"No existe el conflicto {id}.");

/// <param name="FueraDelLote">
/// Los de alto impacto. <b>Se enumeran siempre</b>, aunque estén vacíos.
/// </param>
public sealed record ResultadoDelLote(
    int Resueltos, IReadOnlyList<ConflictoDeSincronizacion> FueraDelLote);

/// <param name="Intentos">
/// Cuántas veces se intentó aplicarlo desde que quedó esperando. <b>Un retenido con veinte
/// intentos no espera un predecesor: espera algo que no va a llegar</b>, y eso hay que verlo
/// antes de que el motorista pregunte por qué su registro nunca entró.
/// </param>
public sealed record Retenido(
    string IdDeCaptura,
    string EsperaExpediente,
    string Transicion,
    string Ejecuta,
    string? Dispositivo,
    DateTimeOffset OcurrioEl,
    DateTimeOffset RetenidoEl,
    int DiasEsperando,
    int Intentos);

/// <param name="Dispositivo">
/// Nulo es «el hecho no dijo de qué equipo vino» — dato faltante del cliente, no un equipo
/// llamado «desconocido».
/// </param>
public sealed record ConflictosPorDispositivo(
    string? Dispositivo, int Total, int Pendientes, int DeAltoImpacto);
