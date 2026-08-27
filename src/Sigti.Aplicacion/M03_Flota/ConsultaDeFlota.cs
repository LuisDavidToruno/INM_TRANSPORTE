using Microsoft.EntityFrameworkCore;
using Sigti.Datos;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// La flota, desde la base.
///
/// <b>Reemplaza a `CatalogoProvisionalDeFlota` en la parte de vehículos.</b> Mientras la
/// flota vivía en código, `BD-03` no podía bloquear: la documentación provisional
/// devolvía vencimientos de 2030 para todo, y el código lo declaraba en un comentario
/// para no fingir que había verificado algo.
///
/// El padrón de conductores sigue siendo provisional — es `M-05`, y no está construido.
/// </summary>
public sealed class ConsultaDeFlota(SigtiDbContext contexto)
{
    public async Task<IReadOnlyList<FilaDeVehiculo>> TodosAsync(CancellationToken cancelacion = default) =>
        await contexto.Vehiculos
            .AsNoTracking()
            .OrderBy(v => v.Siglas)
            .ToListAsync(cancelacion);

    public async Task<FilaDeVehiculo?> PorIdAsync(Ulid id, CancellationToken cancelacion = default) =>
        await contexto.Vehiculos
            .AsNoTracking()
            .SingleOrDefaultAsync(v => v.Id == id, cancelacion);

    /// <summary>
    /// Los que habilita una asignación, para las salidas de la pantalla de rechazo.
    ///
    /// Se traen todos y se filtran en memoria a propósito: la matriz licencia↔vehículo es
    /// un parámetro con vigencia que vive en el dominio, y traducirla a SQL sería una
    /// segunda copia de `BD-02` — con otra oportunidad de diverger, sobre la precondición
    /// que traslada responsabilidad legal. Con una flota institucional el costo es nulo.
    /// </summary>
    public async Task<IReadOnlyList<FilaDeVehiculo>> ParaEvaluarAsync(CancellationToken cancelacion = default) =>
        await TodosAsync(cancelacion);
}
