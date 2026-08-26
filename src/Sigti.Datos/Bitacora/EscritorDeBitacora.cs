using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.Bitacora;

namespace Sigti.Datos.Bitacora;

/// <summary>
/// Escribe asientos serializando la cola.
///
/// La cadena de hash es inherentemente secuencial —el asiento n necesita el hash del
/// n−1—, así que dos transacciones que lean la misma cola a la vez <b>bifurcan la
/// cadena</b>. La serialización es <b>sp_getapplock sobre la cola, dentro de la
/// transacción</b>, más la secuencia monótona.
///
/// <b>No</b> se calcula en un interceptor de SaveChanges sin serializar: funciona con un
/// usuario y bifurca en producción (`ADR-002`).
///
/// El cálculo del hash es puro y vive en el dominio; la serialización vive aquí. Así la
/// verificación de la cadena se prueba sin base de datos, que es lo que una auditoría
/// necesita poder hacer (`ADR-009`).
/// </summary>
public sealed class EscritorDeBitacora(SigtiDbContext contexto)
{
    /// <summary>Sesenta usuarios no son sesenta escrituras por segundo; 15 s es holgado.</summary>
    private const int TiempoDeEsperaMs = 15_000;

    public async Task<Asiento> EscribirAsync(
        string cola,
        string contenido,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        // Si ya hay una transacción en curso, el asiento entra en ella: la transición del
        // expediente y su asiento tienen que confirmarse juntos o no confirmarse. Abrir una
        // transacción anidada, además, lanza excepción en EF Core.
        var transaccionPropia = contexto.Database.CurrentTransaction is null
            ? await contexto.Database.BeginTransactionAsync(cancelacion)
            : null;

        try
        {
            var asiento = await EscribirEnLaTransaccionAsync(cola, contenido, momento, cancelacion);

            if (transaccionPropia is not null)
                await transaccionPropia.CommitAsync(cancelacion);

            return asiento;
        }
        finally
        {
            if (transaccionPropia is not null)
                await transaccionPropia.DisposeAsync();
        }
    }

    private async Task<Asiento> EscribirEnLaTransaccionAsync(
        string cola, string contenido, DateTimeOffset momento, CancellationToken cancelacion)
    {
        await TomarBloqueoDeCola(cola, cancelacion);

        var ultimo = await contexto.Asientos
            .Where(a => a.Cola == cola)
            .OrderByDescending(a => a.Secuencia)
            .FirstOrDefaultAsync(cancelacion);

        var recibido = DateTime.UtcNow;

        var asiento = new Asiento
        {
            Id = Ulid.NewUlid(),
            Cola = cola,
            Secuencia = (ultimo?.Secuencia ?? 0) + 1,
            Contenido = contenido,
            Hash = CadenaDeHash.Calcular(ultimo?.Hash ?? CadenaDeHash.Origen, contenido),
            MomentoUtc = momento.UtcDateTime,
            DesfaseMinutos = (int)momento.Offset.TotalMinutes,
            MomentoRecibidoUtc = recibido
        };

        contexto.Asientos.Add(asiento);
        await contexto.SaveChangesAsync(cancelacion);

        return asiento;
    }

    /// <summary>
    /// El bloqueo se toma con LockOwner = 'Transaction': se libera al confirmar o
    /// revertir, sin que nadie tenga que acordarse de soltarlo.
    ///
    /// Si no se obtiene, <b>falla ruidosamente</b>. Continuar sin el bloqueo sería
    /// escribir la bifurcación que este método existe para impedir.
    /// </summary>
    private async Task TomarBloqueoDeCola(string cola, CancellationToken cancelacion) =>
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"""
             DECLARE @resultado int;
             EXEC @resultado = sp_getapplock
                 @Resource = {cola},
                 @LockMode = 'Exclusive',
                 @LockOwner = 'Transaction',
                 @LockTimeout = {TiempoDeEsperaMs};
             IF @resultado < 0
                 THROW 50001, 'No se obtuvo el bloqueo de la cola de bitácora.', 1;
             """,
            cancelacion);
}
