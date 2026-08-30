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
/// ── ⚠️ Pero SÍ se filtra por estado, y eso no es lo mismo ────────────────────
/// «Sin filtrar por lo que amparan» es de los cuatro campos. El <b>estado</b> no es uno de
/// ellos: un trámite abierto y sin firmar <b>no es un permiso</b>, es una petición. Dejarlo
/// entrar acá haría que `BD-04` lo contara, y cualquiera destrabaría el despacho de un domingo
/// abriendo un trámite y despachando sin esperar la firma — que es exactamente lo que el
/// permiso existe para impedir.
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
            .Where(p => p.ExpedienteId == expediente && p.Estado == EstadoDelPermiso.Firmado.ToString())
            .ToListAsync(cancelacion);

        return filas
            // Firmado implica vehículo, motorista y firmante resueltos: `ReglasDelPermiso`
            // no deja firmar sin ellos. Si alguno viniera nulo acá, la fila está corrupta y
            // callarlo con un valor por omisión produciría un permiso que ampara a nadie.
            .Select(p => new PermisoDeCirculacion(
                p.Folio,
                new IdPersona(p.EmitidoPor ?? throw new PermisoFirmadoIncompleto(p.Folio, "firmante")),
                p.Vehiculo ?? throw new PermisoFirmadoIncompleto(p.Folio, "vehículo"),
                p.Motorista ?? throw new PermisoFirmadoIncompleto(p.Folio, "motorista"),
                p.Destino, p.Desde, p.Hasta))
            .ToList();
    }
}

/// <summary>
/// Una fila `FIRMADO` a la que le falta uno de los cuatro elementos que ampara.
///
/// No debería ocurrir: <c>ReglasDelPermiso.PorQueNoSeFirma</c> lo impide. Se lanza en vez de
/// completar con un valor por omisión porque <b>un permiso que ampara al vehículo cero no
/// ampara a nadie</b>, y entraría en `BD-04` como si sirviera.
/// </summary>
public sealed class PermisoFirmadoIncompleto(string folio, string que)
    : Exception($"El permiso {folio} está firmado y le falta el {que}. " +
                "La fila está corrupta: un permiso firmado tiene los cuatro elementos.");
