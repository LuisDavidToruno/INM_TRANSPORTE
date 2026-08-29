using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <summary>
/// La bandeja de tareas pendientes — <b>§5.3.B.3</b>.
///
/// ── Qué hace y qué no ───────────────────────────────────────────────────────
/// <b>Encola.</b> Es la mitad que el documento llama *«queda visiblemente pendiente en la
/// bandeja de alguien»*, y es la que funciona sin red, sin correo y sin teléfono — que es lo que
/// un despliegue <i>on-premise</i> no puede suponer.
///
/// <b>No notifica</b>, porque no hay canal construido en ningún módulo. Y eso <b>se declara en
/// el dato</b>, no se disimula: <c>NotificadoUtc</c> nulo significa «no se avisó», y la pantalla
/// lo dice. Una bandeja llena sin esa marca se lee como gente que ignora su trabajo.
/// </summary>
public sealed class ServicioDeTareas(SigtiDbContext contexto)
{
    /// <summary>
    /// Encola una tarea. <b>Idempotente por expediente, tipo y persona</b>: quince intentos de
    /// autorizar la misma solicitud son quince asientos en la pista —eso es lo que Auditoría
    /// quiere ver— pero <b>una sola tarea</b>, porque hay una sola cosa que resolver.
    /// </summary>
    public async Task<Ulid?> EncolarAsync(
        TipoDeTarea tipo,
        string asunto,
        string detalle,
        string expediente,
        IdPersona quienLaOrigino,
        IdPuesto? puestoDestino,
        IReadOnlyList<IdPersona> personasDestino,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var yaEsta = await contexto.Tareas.AnyAsync(
            t => t.Expediente == expediente
                && t.Tipo == tipo
                && t.QuienLaOrigino == quienLaOrigino.Valor
                && t.Estado == EstadoDeTarea.Pendiente,
            cancelacion);

        if (yaEsta) return null;

        var id = Ulid.NewUlid();

        contexto.Tareas.Add(new FilaDeTarea
        {
            Id = id,
            Tipo = tipo,
            Asunto = asunto,
            Detalle = detalle,
            Expediente = expediente,
            QuienLaOrigino = quienLaOrigino.Valor,
            PuestoDestino = puestoDestino?.Valor,
            PersonasDestino = string.Join(", ", personasDestino.Select(p => p.Valor)),
            MomentoUtc = momento.UtcDateTime,
            Estado = EstadoDeTarea.Pendiente,

            // **Nulo a propósito.** No hay canal de notificación; fingir que se avisó sería la
            // peor forma de fallar acá, porque nadie iría a mirar la bandeja.
            NotificadoUtc = null,
        });

        await contexto.SaveChangesAsync(cancelacion);
        return id;
    }

    /// <summary>
    /// Cierra una tarea.
    /// </summary>
    /// <param name="descartar">
    /// <b>Descartar no es resolver</b>, y por eso son dos caminos y no una bandera con nombre
    /// bonito: descartar dice que nadie tuvo que hacer nada. Un reporte que las junte no puede
    /// distinguir el control que operó del que se volvió innecesario.
    /// </param>
    public async Task CerrarAsync(
        Ulid id,
        IdPersona resuelve,
        string motivo,
        bool descartar,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Tareas.FirstOrDefaultAsync(t => t.Id == id, cancelacion)
            ?? throw new BloqueoDuro(ReglasDeLaTarea.Precondicion, $"No existe la tarea {id}.");

        var tarea = A(fila);

        ReglasDeLaTarea.ExigirPendiente(tarea);
        ReglasDeLaTarea.ExigirQueNoLaResuelvaQuienLaOrigino(tarea, resuelve);
        ReglasDeLaTarea.ExigirMotivo(motivo);

        fila.Estado = descartar ? EstadoDeTarea.Descartada : EstadoDeTarea.Resuelta;
        fila.Resuelve = resuelve.Valor;
        fila.ResueltaUtc = momento.UtcDateTime;
        fila.Resolucion = motivo.Trim();

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Todas, con las pendientes primero y las más viejas arriba.
    ///
    /// <b>La más vieja arriba y no la más nueva</b>: lo que lleva nueve días esperando es lo que
    /// hay que atender, y ordenar por lo reciente la empuja al fondo justo cuando más urge.
    /// </summary>
    public async Task<IReadOnlyList<FilaDeTarea>> TodasAsync(
        CancellationToken cancelacion = default) =>
        await contexto.Tareas
            .AsNoTracking()
            .OrderBy(t => t.Estado == EstadoDeTarea.Pendiente ? 0 : 1)
            .ThenBy(t => t.MomentoUtc)
            .ToListAsync(cancelacion);

    private static TareaPendiente A(FilaDeTarea f) => new(
        f.Id,
        f.Tipo,
        f.Asunto,
        f.Detalle,
        f.Expediente,
        new IdPersona(f.QuienLaOrigino),
        f.PuestoDestino is null ? null : new IdPuesto(f.PuestoDestino),
        [.. f.PersonasDestino.Split(", ", StringSplitOptions.RemoveEmptyEntries)
            .Select(p => new IdPersona(p))],
        new DateTimeOffset(f.MomentoUtc, TimeSpan.Zero),
        f.Estado,
        f.NotificadoUtc is { } n ? new DateTimeOffset(n, TimeSpan.Zero) : null);
}
