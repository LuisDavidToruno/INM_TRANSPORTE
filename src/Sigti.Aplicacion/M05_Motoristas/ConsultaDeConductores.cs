using Microsoft.EntityFrameworkCore;
using Sigti.Datos;

namespace Sigti.Aplicacion.M05_Motoristas;

/// <summary>
/// El padrón, desde la base — <b>`M-05`</b>.
///
/// Con esto el último catálogo que sostenía una precondición legal sale del código:
/// `BD-02` evalúa licencias reales, no una lista compilada. Quedaba una asimetría rara
/// —la flota en la base y los conductores no— y esa asimetría se paga cuando alguien
/// intenta dar de alta un motorista y descubre que hay que recompilar.
/// </summary>
public sealed class ConsultaDeConductores(SigtiDbContext contexto)
{
    public async Task<IReadOnlyList<FilaDeConductor>> TodosAsync(CancellationToken cancelacion = default) =>
        await contexto.Conductores
            .AsNoTracking()
            .OrderBy(c => c.Nombre)
            .ToListAsync(cancelacion);

    public async Task<FilaDeConductor?> PorIdAsync(Ulid id, CancellationToken cancelacion = default) =>
        await contexto.Conductores
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id, cancelacion);
}
