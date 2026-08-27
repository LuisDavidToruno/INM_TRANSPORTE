using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>
/// El historial de custodia de un vehículo — el insumo de `BD-13`.
///
/// ── Se trae ENTERO, no filtrado por «vigente hoy» ────────────────────────────
/// Porque la vigencia se resuelve <b>a la fecha del hecho</b> (P-4), no a la de consulta. Un
/// despacho capturado en campo y sincronizado tres días después se juzga con el custodio
/// que había el día en que el vehículo salió: filtrar en SQL por «vigente hoy» haría que
/// una rotación posterior invalidara un despacho que fue correcto cuando ocurrió.
///
/// Y son pocas filas: las custodias de <b>un</b> vehículo a lo largo de su vida.
///
/// ── Qué NO hace ─────────────────────────────────────────────────────────────
/// No decide si hay custodio. Eso es la regla, y vive en
/// <c>OrdenDeMision.ExigirCustodiaVigente</c> junto a las demás precondiciones de `T-12`.
/// </summary>
public sealed class ConsultaDeCustodias(SigtiDbContext contexto)
{
    public async Task<IReadOnlyList<CustodiaDelVehiculo>> DeVehiculoAsync(
        Ulid vehiculo,
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.Custodias
            .AsNoTracking()
            .Where(c => c.VehiculoId == vehiculo)
            .OrderByDescending(c => c.Desde)
            .ToListAsync(cancelacion);

        // Descendente por `Desde`: cuando varias son vigentes a la misma fecha —un traspaso
        // registrado el mismo día, que es como ocurre—, la que responde es la más reciente.
        // Sin orden explícito el resultado dependería del plan de la consulta.
        return filas
            .Select(c => new CustodiaDelVehiculo(new IdPersona(c.Custodio), c.Desde, c.Hasta))
            .ToList();
    }
}
