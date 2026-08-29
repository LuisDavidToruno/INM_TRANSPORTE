using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M14_Auditoria;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M14_Auditoria;

/// <summary>
/// El expediente de hallazgo posterior — `RN-93`.
///
/// ── Lo que hace posible ─────────────────────────────────────────────────────
/// Corregir el efecto económico de un hallazgo descubierto meses después <b>sin reabrir el
/// expediente cerrado</b>. La regla explica por qué importa: <i>«basta con que la reapertura de
/// un expediente cerrado exista para que se use, y basta con que se use una vez para que ningún
/// reporte histórico vuelva a ser reproducible»</i>.
///
/// ── Y lo que nunca hace ─────────────────────────────────────────────────────
/// <b>Tocar la misión.</b> Este servicio no tiene una sola escritura sobre el expediente de la
/// orden, y eso es deliberado: lo que se entrega a quien pide es el paquete sellado tal como
/// cerró <b>más</b> este expediente. Es más información, no menos.
/// </summary>
public sealed class ServicioDeHallazgosPosteriores(SigtiDbContext contexto)
{
    /// <summary>
    /// `H-01` — abre el expediente.
    /// </summary>
    public async Task<Ulid> AbrirAsync(
        Ulid id,
        string tipo,
        DateOnly fechaDelHecho,
        DateOnly fechaDelDescubrimiento,
        string comoSeDescubrio,
        string fuente,
        string? documentoAdjunto,
        IReadOnlyList<Ulid> misiones,
        Ulid? vehiculo,
        Ulid? motorista,
        string? periodo,
        Autoria descubre,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var expediente = ExpedienteDeHallazgoPosterior.Abrir(
            id, tipo, fechaDelHecho, fechaDelDescubrimiento, comoSeDescubrio, fuente,
            documentoAdjunto, misiones, vehiculo, motorista, periodo, descubre, momento);

        await GuardarAsync(expediente, cancelacion);
        return expediente.Id;
    }

    /// <summary>Aplica un movimiento sobre un expediente existente.</summary>
    public async Task MoverAsync(
        Ulid id, Action<ExpedienteDeHallazgoPosterior> movimiento,
        CancellationToken cancelacion = default)
    {
        var expediente = await BuscarAsync(id, cancelacion)
            ?? throw new HallazgoNoEncontrado(id);

        movimiento(expediente);

        await GuardarAsync(expediente, cancelacion);
    }

    public async Task<ExpedienteDeHallazgoPosterior?> BuscarAsync(
        Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.HallazgosPosteriores
            .Include(h => h.Misiones)
            .Include(h => h.Movimientos)
            .Include(h => h.Reversos)
            .SingleOrDefaultAsync(h => h.Id == id, cancelacion);

        return fila is null ? null : A(fila);
    }

    public async Task<IReadOnlyList<ExpedienteDeHallazgoPosterior>> TodosAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.HallazgosPosteriores
            .Include(h => h.Misiones)
            .Include(h => h.Movimientos)
            .Include(h => h.Reversos)
            .ToListAsync(cancelacion);

        // Por antigüedad del **hecho**, no del descubrimiento: es el orden que `RN-97` arrastra
        // al ejercicio siguiente, y el que pone arriba lo que lleva más tiempo sin resolverse.
        return [.. filas.Select(A).OrderBy(h => h.FechaDelHecho)];
    }

    /// <summary>
    /// Los hallazgos vinculados a una misión — §7.5: <i>«la misión cerrada muestra desde
    /// entonces, de forma visible, que tiene hallazgos posteriores vinculados»</i>.
    ///
    /// <b>Se consulta desde la misión y no se guarda en ella.</b> Guardar una marca en el
    /// expediente cerrado sería modificarlo, que es justo lo que la inmutabilidad prohíbe.
    /// </summary>
    public async Task<IReadOnlyList<ExpedienteDeHallazgoPosterior>> DeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var ids = await contexto.Set<FilaDeMisionDelHallazgo>()
            .Where(v => v.MisionId == mision)
            .Select(v => v.HallazgoId)
            .ToListAsync(cancelacion);

        var expedientes = new List<ExpedienteDeHallazgoPosterior>();

        // Punto por punto: un `Contains` sobre ULID con conversión de valor devuelve vacío en
        // silencio bajo `UseCompatibilityLevel(120)`, y acá el silencio diría que una misión no
        // tiene hallazgos cuando sí los tiene.
        foreach (var id in ids)
            if (await BuscarAsync(id, cancelacion) is { } expediente)
                expedientes.Add(expediente);

        return expedientes;
    }

    /// <summary>
    /// El efecto económico imputado a un período — §8.3: el reverso afecta los acumulados
    /// <b>del período en que se registra</b>.
    ///
    /// Es la capa identificada que `RN-93` punto 3 manda mostrar: <i>«no se recalculan los
    /// históricos ya publicados. Se ajusta el período corriente y se muestra el ajuste como capa
    /// identificada»</i>.
    /// </summary>
    public async Task<decimal> AjusteDelPeriodoAsync(
        string periodo, CancellationToken cancelacion = default) =>
        await contexto.AsientosReversos
            .Where(r => r.PeriodoDeImputacion == periodo && r.EfectoEconomico != null)
            .SumAsync(r => r.EfectoEconomico ?? 0m, cancelacion);

    private async Task GuardarAsync(
        ExpedienteDeHallazgoPosterior expediente, CancellationToken cancelacion)
    {
        var fila = await contexto.HallazgosPosteriores
            .Include(h => h.Misiones)
            .Include(h => h.Movimientos)
            .Include(h => h.Reversos)
            .SingleOrDefaultAsync(h => h.Id == expediente.Id, cancelacion);

        if (fila is null)
        {
            fila = new FilaDeHallazgo
            {
                Id = expediente.Id,
                Tipo = expediente.Tipo,
                FechaDelHecho = expediente.FechaDelHecho,
                FechaDelDescubrimiento = expediente.FechaDelDescubrimiento,
                ComoSeDescubrio = expediente.ComoSeDescubrio,
                Fuente = expediente.Fuente,
                DocumentoAdjunto = expediente.DocumentoAdjunto,
                VehiculoId = expediente.Vehiculo,
                MotoristaId = expediente.Motorista,
                Periodo = expediente.Periodo,
            };
            contexto.HallazgosPosteriores.Add(fila);
        }

        // La resolución sí se escribe: es el único dato del expediente que cambia, y sólo una
        // vez — el agregado impide tocarlo después.
        fila.Resolucion = expediente.Resolucion;
        fila.Fundamento = expediente.Fundamento;

        foreach (var mision in expediente.Misiones)
        {
            if (fila.Misiones.Any(m => m.MisionId == mision)) continue;

            fila.Misiones.Add(new FilaDeMisionDelHallazgo
            {
                HallazgoId = expediente.Id,
                MisionId = mision,
            });
        }

        // Sólo agrega. Un asiento escrito no se actualiza ni se borra (P-3) — y acá pesa el
        // doble: este diario es el que se opone a un expediente que se dio por firme.
        for (var orden = fila.Movimientos.Count; orden < expediente.Diario.Count; orden++)
        {
            var m = expediente.Diario[orden];

            fila.Movimientos.Add(new FilaDeMovimientoDelHallazgo
            {
                Id = Ulid.NewUlid(),
                HallazgoId = expediente.Id,
                Orden = orden,
                Movimiento = m.Id,
                Persona = m.Autor.Persona.Valor,
                Puesto = m.Autor.Puesto.Valor,
                FechaDelHecho = m.Autor.FechaDelHecho,
                MomentoUtc = m.Momento.UtcDateTime,
                DesfaseMinutos = (int)m.Momento.Offset.TotalMinutes,
                Motivo = m.Motivo,
                ReversoId = m.Reverso,
            });
        }

        for (var i = fila.Reversos.Count; i < expediente.Reversos.Count; i++)
        {
            var r = expediente.Reversos[i];

            fila.Reversos.Add(new FilaDeReverso
            {
                Id = r.Id,
                HallazgoId = expediente.Id,
                TipoDeAsiento = r.Revertido.Tipo,
                IdentificadorDelAsiento = r.Revertido.Identificador,
                DescripcionDelAsiento = r.Revertido.Descripcion,
                Naturaleza = r.Naturaleza,
                ValorAnterior = r.ValorAnterior,
                ValorNuevo = r.ValorNuevo,
                FechaDelHechoOriginal = r.FechaDelHechoOriginal,
                FechaDelReversoUtc = r.FechaDelReverso.UtcDateTime,
                DesfaseMinutos = (int)r.FechaDelReverso.Offset.TotalMinutes,
                Persona = r.Autor.Persona.Valor,
                Puesto = r.Autor.Puesto.Valor,
                Autoriza = r.Autoriza.Valor,
                AutorDelAsientoOriginal = r.AutorDelAsientoOriginal.Valor,
                MotivoTipificado = r.MotivoTipificado,
                Fundamento = r.Fundamento,
                Adjunto = r.Adjunto,
                PeriodoAfectado = r.PeriodoAfectado,
                PeriodoDeImputacion = r.PeriodoDeImputacion,
                EfectoEconomico = r.EfectoEconomico,
                TablasParametricas = r.TablasParametricas is { Count: > 0 } t
                    ? string.Join(", ", t)
                    : null,
            });
        }

        await contexto.SaveChangesAsync(cancelacion);
    }

    private static ExpedienteDeHallazgoPosterior A(FilaDeHallazgo f) =>
        ExpedienteDeHallazgoPosterior.Reconstruir(
            f.Id, f.Tipo, f.FechaDelHecho, f.FechaDelDescubrimiento, f.ComoSeDescubrio,
            f.Fuente, f.DocumentoAdjunto,
            [.. f.Misiones.Select(m => m.MisionId)],
            f.VehiculoId, f.MotoristaId, f.Periodo, f.Resolucion, f.Fundamento,
            f.Movimientos
                .OrderBy(m => m.Orden)
                .Select(m => new MovimientoDelHallazgo(
                    m.Movimiento,
                    Autoria.De(new IdPersona(m.Persona), new IdPuesto(m.Puesto), m.FechaDelHecho),
                    new DateTimeOffset(m.MomentoUtc, TimeSpan.Zero)
                        .ToOffset(TimeSpan.FromMinutes(m.DesfaseMinutos)),
                    m.Motivo,
                    m.ReversoId)),
            f.Reversos.Select(r => new AsientoReverso(
                r.Id,
                new ReferenciaAlAsiento(
                    r.TipoDeAsiento, r.IdentificadorDelAsiento, r.DescripcionDelAsiento),
                r.Naturaleza, r.ValorAnterior, r.ValorNuevo, r.FechaDelHechoOriginal,
                new DateTimeOffset(r.FechaDelReversoUtc, TimeSpan.Zero)
                    .ToOffset(TimeSpan.FromMinutes(r.DesfaseMinutos)),
                Autoria.De(new IdPersona(r.Persona), new IdPuesto(r.Puesto),
                    r.FechaDelHechoOriginal),
                new IdPersona(r.Autoriza), new IdPersona(r.AutorDelAsientoOriginal),
                r.MotivoTipificado, r.Fundamento, r.Adjunto,
                r.PeriodoAfectado, r.PeriodoDeImputacion, r.EfectoEconomico,
                r.TablasParametricas?.Split(", ", StringSplitOptions.RemoveEmptyEntries))));
}

public sealed class HallazgoNoEncontrado(Ulid id)
    : Exception($"No existe el expediente de hallazgo posterior {id}.");
