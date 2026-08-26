using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M02_Parametros;

namespace Sigti.Datos.M02_Parametros;

/// <summary>
/// Repositorio con intención (`ADR-009`): expone lo que el negocio pregunta —«dame el
/// catálogo de esta clave»— y no operaciones de tabla.
/// </summary>
public sealed class ParametrosNormativos(SigtiDbContext contexto)
{
    /// <summary>
    /// Trae <b>todas</b> las versiones de la clave, incluidas las cerradas en el eje de
    /// transacción. Filtrar acá por «las vigentes» rompería la reproducibilidad: una
    /// liquidación de marzo necesita la versión que ya fue superada (`RNF-06`).
    /// </summary>
    public async Task<CatalogoDeParametros> CatalogoDeAsync(
        string clave, CancellationToken cancelacion = default)
    {
        var versiones = await contexto.Parametros
            .Where(v => v.Clave == clave)
            .AsNoTracking()
            .ToListAsync(cancelacion);

        return new CatalogoDeParametros(versiones);
    }

    /// <summary>
    /// Agrega una versión. <b>Nunca actualiza una existente</b>: corregir es cerrar el
    /// `RegistradoHasta` de la anterior e insertar la nueva, para que las liquidaciones
    /// ya emitidas sigan siendo explicables (`P-3`, `RN-04`).
    /// </summary>
    public async Task GuardarAsync(VersionDeParametro version, CancellationToken cancelacion = default)
    {
        contexto.Parametros.Add(version);
        await contexto.SaveChangesAsync(cancelacion);
    }
}
