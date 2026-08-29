using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <summary>
/// Aplica el control bloqueante de §5.3.B <b>y deja el rastro</b>.
///
/// ── Por qué el registro va acá y no en el dominio ───────────────────────────
/// <c>ReglasDeSegregacion</c> arma el asiento y no lo escribe: escribir desde el dominio lo
/// obligaría a conocer la base, y el bloqueo dejaría de ser probable sin ella. Acá se cierra el
/// circuito.
///
/// ── El registro ocurre AUNQUE el acto se impida ─────────────────────────────
/// Es la parte contraintuitiva y es el punto entero de §5.3.B.2: el acto <b>no se guarda</b>,
/// pero <b>el intento sí</b>. Un sistema que sólo guarda lo consumado no puede contestar si el
/// control operó — un bloqueo perfecto y uno que nunca se activó se ven iguales.
/// </summary>
public sealed class ServicioDeSegregacion(SigtiDbContext contexto, ServicioDeTareas tareas)
{
    /// <summary>
    /// Evalúa el acto. Si choca, <b>registra el intento y lanza</b>.
    ///
    /// El registro se confirma antes de lanzar, en su propia unidad de trabajo: si se dejara
    /// para el manejador de la excepción, el rollback de la operación bloqueada se llevaría
    /// también el asiento de auditoría — y el intento desaparecería justo cuando ocurrió.
    /// </summary>
    /// <returns>
    /// Las advertencias de `I-15` e `I-16`, que <b>no impiden</b> pero exigen motivo escrito.
    /// </returns>
    public async Task<IReadOnlyList<AdvertenciaDeSegregacion>> ExigirAsync(
        IdPersona quien,
        Funcion pretende,
        ActosDelExpediente previos,
        string expediente,
        DateTimeOffset momento,
        string? origen = null,
        bool i14Activo = false,
        CancellationToken cancelacion = default)
    {
        try
        {
            return ReglasDeSegregacion.Exigir(
                quien, pretende, previos, expediente, momento, i14Activo);
        }
        catch (SegregacionIncompatible bloqueo)
        {
            // §5.3.B.3 — **a dónde queda pendiente**. Se resuelve acá y no en el dominio porque
            // exige el espejo de puestos y el organigrama, que viven en la base.
            var (estructura, destino) = await EscalarAsync(
                quien, DateOnly.FromDateTime(momento.Date), cancelacion);

            await RegistrarAsync(bloqueo.Intento, origen, destino, cancelacion);

            // §5.3.B.3 — **encola la acción como pendiente de resolución**. La pista dice que
            // el intento ocurrió; la bandeja es lo que hace que alguien lo atienda.
            await tareas.EncolarAsync(
                TipoDeTarea.SegregacionBloqueada,
                $"{bloqueo.Par}: {ReglasDeSegregacion.EnPalabrasDe(bloqueo.Intento.Pretendia)} bloqueada",
                bloqueo.Message,
                bloqueo.Intento.Expediente,
                quien,
                destino.Puesto,
                destino.Ocupantes,
                momento,
                cancelacion);

            throw new SegregacionIncompatible(
                bloqueo.Par,
                $"{bloqueo.Message} {ReglasDelEscalamiento.EnPalabras(destino, estructura)}",
                bloqueo.Intento);
        }
    }

    /// <summary>
    /// Resuelve el destino del acto bloqueado.
    ///
    /// ── Se busca el puesto de quien intentó, no se recibe ───────────────────
    /// Porque quien ejecuta llega como identidad de <b>persona</b> —es lo que la segregación
    /// compara— y el escalamiento necesita su <b>puesto</b>. Pedirle el puesto a cada llamador
    /// abriría la puerta a que declare uno que no ocupa.
    ///
    /// <b>Si ocupa varios, se toma el primero.</b> Es una simplificación y conviene saberlo: la
    /// persona que ocupa dos puestos podría escalar por dos ramas distintas, y cuál corresponde
    /// depende de en calidad de qué actuaba — un dato que el acto no lleva. Se declara en el
    /// motivo en vez de elegir en silencio.
    /// </summary>
    private async Task<(EstructuraDePuestos, DestinoDelActo)> EscalarAsync(
        IdPersona quien, DateOnly fechaDelHecho, CancellationToken cancelacion)
    {
        var espejo = await contexto.PuestosEspejo.AsNoTracking().ToListAsync(cancelacion);

        var estructura = new EstructuraDePuestos(
        [
            .. espejo.Select(f => new Puesto(
                new IdPuesto(f.Puesto), f.Denominacion, f.Unidad,
                f.Superior is null ? null : new IdPuesto(f.Superior),
                f.Delegacion)),
        ]);

        var asignaciones = await contexto.AsignacionesDePuesto
            .AsNoTracking()
            .ToListAsync(cancelacion);

        var organigrama = new Organigrama(
        [
            .. asignaciones.Select(a => new AsignacionDePuesto(
                new IdPersona(a.Persona), new IdPuesto(a.Puesto), a.Desde, a.Hasta)),
        ]);

        var respaldos = await contexto.RespaldosDeSede.AsNoTracking().ToListAsync(cancelacion);

        var suyos = organigrama.PuestosDe(quien, fechaDelHecho);

        var destino = ReglasDelEscalamiento.Resolver(
            quien,
            suyos.Count > 0 ? suyos[0] : null,
            estructura,
            organigrama,
            [.. respaldos.Select(r => new RespaldoDeSede(r.Delegacion, new IdPuesto(r.Puesto)))],
            fechaDelHecho);

        return (estructura, destino);
    }

    /// <summary>
    /// El asiento de §5.3.B.2, con los siete datos que la sección enumera.
    /// </summary>
    private async Task RegistrarAsync(
        IntentoBloqueado intento, string? origen, DestinoDelActo destino,
        CancellationToken cancelacion)
    {
        contexto.IntentosBloqueados.Add(new FilaDeIntentoBloqueado
        {
            Salto = destino.Salto.ToString(),
            EscalaA = destino.Puesto?.Valor,

            // Vacío se guarda como nulo: «no hubo motivos» y «no se registraron» son cosas
            // distintas, y una cadena vacía las confunde.
            PorQueNoAntes = destino.PorQueNoAntes.Length == 0 ? null : destino.PorQueNoAntes,

            Id = Ulid.NewUlid(),
            Quien = intento.Quien.Valor,
            Pretendia = intento.Pretendia.ToString(),
            Expediente = intento.Expediente,
            Par = intento.Par,
            ChocaCon = intento.ChocaCon.ToString(),
            Referencia = intento.Referencia,
            MomentoUtc = intento.Momento.UtcDateTime,

            // Nulo es «no se supo», no «desde el servidor».
            Origen = origen,
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// La pista completa — `PT-091`.
    ///
    /// Lo más reciente primero: la pregunta habitual de Auditoría es qué está pasando ahora, y
    /// la reincidencia se ve mejor agrupando que recorriendo.
    /// </summary>
    public async Task<IReadOnlyList<FilaDeIntentoBloqueado>> TodosAsync(
        CancellationToken cancelacion = default) =>
        await contexto.IntentosBloqueados
            .AsNoTracking()
            .OrderByDescending(i => i.MomentoUtc)
            .ToListAsync(cancelacion);
}
