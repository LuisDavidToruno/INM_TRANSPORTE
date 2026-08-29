using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// Repositorio con intención (`ADR-009`) del circuito de reintegro — `RN-86`.
///
/// ── Las preguntas reales ────────────────────────────────────────────────────
/// <b>¿Qué debe esta persona?</b> — la del bloqueo, en cada emisión de vale ·
/// <b>¿Quién debe, cuánto y desde cuándo?</b> — la del arqueo por persona ·
/// <b>¿Qué quedó abierto al cierre del ejercicio?</b> — la del saldo de apertura de `RN-97`.
/// </summary>
public sealed class ObligacionesDeReintegro(SigtiDbContext contexto)
{
    public async Task<ObligacionDeReintegro?> BuscarAsync(
        Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.ObligacionesDeReintegro
            .Include(o => o.Movimientos)
            .SingleOrDefaultAsync(o => o.Id == id, cancelacion);

        return fila is null ? null : A(fila);
    }

    /// <summary>
    /// Lo que debe —o le deben a— una persona. <b>Trae también las cerradas</b>: el expediente
    /// de una obligación saldada es evidencia, y el bloqueo filtra por su cuenta.
    /// </summary>
    public async Task<IReadOnlyList<ObligacionDeReintegro>> DeLaPersonaAsync(
        Ulid responsable, CancellationToken cancelacion = default)
    {
        var filas = await contexto.ObligacionesDeReintegro
            .Include(o => o.Movimientos)
            .Where(o => o.Responsable == responsable)
            .ToListAsync(cancelacion);

        return [.. filas.Select(A).OrderBy(o => o.FechaDelHecho)];
    }

    public async Task<IReadOnlyList<ObligacionDeReintegro>> TodasAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.ObligacionesDeReintegro
            .Include(o => o.Movimientos)
            .ToListAsync(cancelacion);

        return [.. filas.Select(A).OrderBy(o => o.FechaDelHecho)];
    }

    public async Task GuardarAsync(
        ObligacionDeReintegro obligacion, CancellationToken cancelacion = default)
    {
        var fila = await contexto.ObligacionesDeReintegro
            .Include(o => o.Movimientos)
            .SingleOrDefaultAsync(o => o.Id == obligacion.Id, cancelacion);

        if (fila is null)
        {
            fila = new FilaDeObligacion
            {
                Id = obligacion.Id,
                Direccion = obligacion.Direccion,
                Causa = obligacion.Causa,
                Responsable = obligacion.Responsable,
                Monto = obligacion.Monto,
                MisionId = obligacion.Mision,
                AsignacionId = obligacion.Asignacion,
                FechaDelHecho = obligacion.FechaDelHecho,
            };
            contexto.ObligacionesDeReintegro.Add(fila);
        }

        // Sólo agrega. El monto y el responsable no se tocan nunca: `RN-86` punto 3 congela el
        // monto, y corregirlo hacia abajo para que cuadre es reescribir el pasado.
        for (var orden = fila.Movimientos.Count; orden < obligacion.Diario.Count; orden++)
        {
            var m = obligacion.Diario[orden];

            fila.Movimientos.Add(new FilaDeMovimientoDeObligacion
            {
                Id = Ulid.NewUlid(),
                ObligacionId = obligacion.Id,
                Orden = orden,
                Movimiento = m.Id,
                Destino = m.Destino,
                Persona = m.Autor.Persona.Valor,
                Puesto = m.Autor.Puesto.Valor,
                FechaDelHecho = m.Autor.FechaDelHecho,
                MomentoUtc = m.Momento.UtcDateTime,
                DesfaseMinutos = (int)m.Momento.Offset.TotalMinutes,
                Motivo = m.Motivo,
                Pagado = m.Pagado,
            });
        }

        await contexto.SaveChangesAsync(cancelacion);
    }

    private static ObligacionDeReintegro A(FilaDeObligacion fila) =>
        ObligacionDeReintegro.Reconstruir(
            fila.Id, fila.Direccion, fila.Causa, fila.Responsable, fila.Monto,
            fila.MisionId, fila.AsignacionId, fila.FechaDelHecho,
            fila.Movimientos
                .OrderBy(m => m.Orden)
                .Select(m => new MovimientoDeObligacion(
                    m.Movimiento,
                    m.Destino,
                    Autoria.De(new IdPersona(m.Persona), new IdPuesto(m.Puesto), m.FechaDelHecho),
                    new DateTimeOffset(m.MomentoUtc, TimeSpan.Zero)
                        .ToOffset(TimeSpan.FromMinutes(m.DesfaseMinutos)),
                    m.Motivo,
                    m.Pagado)));
}
