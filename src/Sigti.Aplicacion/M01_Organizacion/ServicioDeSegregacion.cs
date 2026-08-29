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
public sealed class ServicioDeSegregacion(SigtiDbContext contexto)
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
            await RegistrarAsync(bloqueo.Intento, origen, cancelacion);
            throw;
        }
    }

    /// <summary>
    /// El asiento de §5.3.B.2, con los siete datos que la sección enumera.
    /// </summary>
    private async Task RegistrarAsync(
        IntentoBloqueado intento, string? origen, CancellationToken cancelacion)
    {
        contexto.IntentosBloqueados.Add(new FilaDeIntentoBloqueado
        {
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
