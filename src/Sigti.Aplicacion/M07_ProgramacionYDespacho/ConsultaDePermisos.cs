using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>
/// Los permisos de circulación emitidos para un expediente — el insumo de `BD-04`.
///
/// ── Se traen todos, sin filtrar por lo que amparan ───────────────────────────
/// Porque «¿este permiso ampara esta salida?» es <b>la regla</b>, y vive en
/// <see cref="PermisoDeCirculacion.Ampara"/>. Filtrar acá por vehículo y motorista metería
/// la regla en un <c>WHERE</c> — y, peor, haría indistinguible «no hay ningún permiso» de
/// «hay permisos pero ninguno sirve». Son dos problemas con dos arreglos distintos, y el
/// mensaje del bloqueo los separa.
///
/// Son pocas filas: los permisos de <b>un</b> expediente.
/// </summary>
public sealed class ConsultaDePermisos(SigtiDbContext contexto)
{
    public async Task<IReadOnlyList<PermisoDeCirculacion>> DeExpedienteAsync(
        Ulid expediente,
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.Permisos
            .AsNoTracking()
            .Where(p => p.ExpedienteId == expediente)
            .ToListAsync(cancelacion);

        return filas
            .Select(p => new PermisoDeCirculacion(
                p.Folio, new IdPersona(p.EmitidoPor), p.Vehiculo, p.Motorista,
                p.Destino, p.Desde, p.Hasta))
            .ToList();
    }
}
