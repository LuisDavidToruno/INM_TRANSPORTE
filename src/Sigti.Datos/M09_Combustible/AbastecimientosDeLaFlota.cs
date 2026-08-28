using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// Los abastecimientos, con las preguntas que `RN-30` y `RN-83` hacen.
///
/// ── La pregunta que importa ─────────────────────────────────────────────────
/// <b>Cuántos galones entraron a este tanque, vengan de donde vengan.</b> Es el numerador de la
/// conciliación, y hasta hoy el sistema sólo sabía contestar por los del fondo — que es la mitad
/// que pasa por un folio.
/// </summary>
public sealed class AbastecimientosDeLaFlota(SigtiDbContext contexto)
{
    /// <summary>
    /// Los de una misión, <b>de todas las fuentes</b>.
    ///
    /// `RN-30` punto 4: la conciliación <i>«usa todos los abastecimientos del período, no solo
    /// los del fondo, y expone la fuente de cada uno»</i>.
    /// </summary>
    public async Task<IReadOnlyList<Abastecimiento>> DeLaMisionAsync(
        Ulid misionId, CancellationToken cancelacion = default)
    {
        var filas = await contexto.Abastecimientos
            .Where(a => a.MisionId == misionId)
            .OrderBy(a => a.MomentoUtc)
            .ToListAsync(cancelacion);

        return [.. filas.Select(AAbastecimiento)];
    }

    /// <summary>
    /// Los de un vehículo en un rango — para el análisis agregado, que es donde `RN-30` dice que
    /// el patrón se ve, «no en una misión aislada».
    /// </summary>
    public async Task<IReadOnlyList<Abastecimiento>> DelVehiculoAsync(
        Ulid vehiculo, DateTimeOffset desde, DateTimeOffset hasta,
        CancellationToken cancelacion = default)
    {
        var d = desde.UtcDateTime;
        var h = hasta.UtcDateTime;

        var filas = await contexto.Abastecimientos
            .Where(a => a.VehiculoId == vehiculo && a.MomentoUtc >= d && a.MomentoUtc <= h)
            .OrderBy(a => a.MomentoUtc)
            .ToListAsync(cancelacion);

        return [.. filas.Select(AAbastecimiento)];
    }

    /// <summary>
    /// Guarda uno. <b>Sólo agrega</b>: un abastecimiento registrado no se corrige editándolo —
    /// nada se borra físicamente, y una carga mal capturada se corrige con un asiento nuevo.
    /// </summary>
    public async Task GuardarAsync(
        Abastecimiento abastecimiento,
        Ulid? transicionDelVale = null,
        Ulid? idDeCaptura = null,
        CancellationToken cancelacion = default)
    {
        contexto.Abastecimientos.Add(new FilaDeAbastecimiento
        {
            Id = abastecimiento.Id,
            VehiculoId = abastecimiento.Vehiculo,
            MomentoUtc = abastecimiento.OcurridoEn.UtcDateTime,
            DesfaseMinutos = (int)abastecimiento.OcurridoEn.Offset.TotalMinutes,
            Galones = abastecimiento.Galones,
            Odometro = abastecimiento.Odometro,
            Fuente = abastecimiento.Fuente,
            Registra = abastecimiento.Registra.Valor,
            MisionId = abastecimiento.Mision,
            AsignacionId = abastecimiento.Asignacion,
            TransicionDelValeId = transicionDelVale,
            IdDeCaptura = idDeCaptura,
            Monto = abastecimiento.Monto,
            Estacion = abastecimiento.Estacion,
            Comprobante = abastecimiento.Comprobante,
            CausaSinComprobante = abastecimiento.CausaSinComprobante,
            Excedido = abastecimiento.Excedido,
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    private static Abastecimiento AAbastecimiento(FilaDeAbastecimiento f) =>
        Abastecimiento.Reconstruir(
            f.Id, f.VehiculoId,
            new DateTimeOffset(f.MomentoUtc, TimeSpan.Zero)
                .ToOffset(TimeSpan.FromMinutes(f.DesfaseMinutos)),
            f.Galones, f.Odometro, f.Fuente, new IdPersona(f.Registra),
            f.MisionId, f.AsignacionId, f.Monto, f.Estacion, f.Comprobante,
            f.CausaSinComprobante, f.Excedido);
}
