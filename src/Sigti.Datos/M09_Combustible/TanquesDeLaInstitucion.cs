using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// Repositorio con intención (`ADR-009`) del libro de existencias.
///
/// ── Las preguntas reales ────────────────────────────────────────────────────
/// <b>¿Cuánto hay en el tanque?</b> · <b>¿De dónde salió este galón?</b> ·
/// <b>¿Qué se declaró sacado del tanque que el tanque no registró?</b> — la tercera es el
/// préstamo invisible de `CE-23`, y es la que justifica que este libro exista.
/// </summary>
public sealed class TanquesDeLaInstitucion(SigtiDbContext contexto)
{
    public async Task<TanqueInstitucional?> BuscarAsync(
        Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Tanques
            .Include(t => t.Movimientos)
            .SingleOrDefaultAsync(t => t.Id == id, cancelacion);

        return fila is null ? null : A(fila);
    }

    public async Task<IReadOnlyList<TanqueInstitucional>> TodosAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.Tanques
            .Include(t => t.Movimientos)
            .ToListAsync(cancelacion);

        return [.. filas.Select(A).OrderBy(t => t.AmbitoDeclarado).ThenBy(t => t.Nombre)];
    }

    /// <summary>
    /// Si este abastecimiento tiene un despacho del tanque que lo respalde.
    ///
    /// <b>Punto por punto y no con <c>Contains</c></b>: bajo `UseCompatibilityLevel(120)` un
    /// <c>Contains</c> sobre un ULID con conversión de valor devuelve vacío en silencio, y acá
    /// vacío en silencio significaría reportar como no respaldado un galón que sí lo está.
    /// </summary>
    public Task<bool> TieneDespachoAsync(
        Ulid abastecimiento, CancellationToken cancelacion = default) =>
        contexto.MovimientosDeExistencias
            .AnyAsync(m => m.AbastecimientoId == abastecimiento, cancelacion);

    public async Task GuardarAsync(
        TanqueInstitucional tanque, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Tanques
            .Include(t => t.Movimientos)
            .SingleOrDefaultAsync(t => t.Id == tanque.Id, cancelacion);

        if (fila is null)
        {
            fila = new FilaDeTanque
            {
                Id = tanque.Id,
                Nombre = tanque.Nombre,
                AmbitoDeclarado = tanque.AmbitoDeclarado,
                TipoDeCombustible = tanque.TipoDeCombustible,
                CapacidadGalones = tanque.CapacidadGalones,
            };
            contexto.Tanques.Add(fila);
        }

        // Sólo agrega. Un asiento escrito no se actualiza ni se borra (P-3) — y acá pesa más
        // que en ningún otro libro: corregir un egreso viejo cambia la existencia de todos los
        // días posteriores, y los arqueos que se hicieron contra ella dejan de reproducirse.
        for (var orden = fila.Movimientos.Count; orden < tanque.Libro.Count; orden++)
        {
            var m = tanque.Libro[orden];

            fila.Movimientos.Add(new FilaDeMovimientoDeExistencias
            {
                Id = Ulid.NewUlid(),
                TanqueId = tanque.Id,
                Orden = orden,
                Movimiento = m.Id,
                Tipo = m.Tipo,
                Galones = m.Galones,
                Persona = m.Autor.Persona.Valor,
                Puesto = m.Autor.Puesto.Valor,
                FechaDelHecho = m.Autor.FechaDelHecho,
                MomentoUtc = m.Momento.UtcDateTime,
                DesfaseMinutos = (int)m.Momento.Offset.TotalMinutes,
                Motivo = m.Motivo,
                VehiculoId = m.Vehiculo,
                MisionId = m.Mision,
                AbastecimientoId = m.Abastecimiento,
                ContraparteId = m.Contraparte,
                ExistenciaMedida = m.ExistenciaMedida,
                MotivoDelAjuste = m.MotivoDelAjuste,
                Comprobante = m.Comprobante,
            });
        }

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Guarda los <b>dos</b> lados de un trasiego en una sola transacción.
    ///
    /// Registrar sólo la salida haría que el combustible se evaporara del sistema entero en vez
    /// de sólo de este tanque — y ésa es exactamente la forma en que un faltante se disfraza de
    /// traslado. <c>SaveChanges</c> es atómico, así que o entran los dos asientos o ninguno.
    /// </summary>
    public async Task GuardarTrasiegoAsync(
        TanqueInstitucional origen, TanqueInstitucional destino,
        CancellationToken cancelacion = default)
    {
        await SincronizarAsync(origen, cancelacion);
        await SincronizarAsync(destino, cancelacion);
        await contexto.SaveChangesAsync(cancelacion);
    }

    private async Task SincronizarAsync(
        TanqueInstitucional tanque, CancellationToken cancelacion)
    {
        var fila = await contexto.Tanques
            .Include(t => t.Movimientos)
            .SingleAsync(t => t.Id == tanque.Id, cancelacion);

        for (var orden = fila.Movimientos.Count; orden < tanque.Libro.Count; orden++)
        {
            var m = tanque.Libro[orden];

            fila.Movimientos.Add(new FilaDeMovimientoDeExistencias
            {
                Id = Ulid.NewUlid(),
                TanqueId = tanque.Id,
                Orden = orden,
                Movimiento = m.Id,
                Tipo = m.Tipo,
                Galones = m.Galones,
                Persona = m.Autor.Persona.Valor,
                Puesto = m.Autor.Puesto.Valor,
                FechaDelHecho = m.Autor.FechaDelHecho,
                MomentoUtc = m.Momento.UtcDateTime,
                DesfaseMinutos = (int)m.Momento.Offset.TotalMinutes,
                Motivo = m.Motivo,
                ContraparteId = m.Contraparte,
            });
        }
    }

    private static TanqueInstitucional A(FilaDeTanque fila) =>
        TanqueInstitucional.Reconstruir(
            fila.Id, fila.Nombre, fila.AmbitoDeclarado, fila.TipoDeCombustible,
            fila.CapacidadGalones,
            fila.Movimientos
                .OrderBy(m => m.Orden)
                .Select(m => new MovimientoDeExistencias(
                    m.Movimiento,
                    m.Tipo,
                    m.Galones,
                    Autoria.De(new IdPersona(m.Persona), new IdPuesto(m.Puesto), m.FechaDelHecho),
                    new DateTimeOffset(m.MomentoUtc, TimeSpan.Zero)
                        .ToOffset(TimeSpan.FromMinutes(m.DesfaseMinutos)),
                    m.Motivo,
                    m.VehiculoId,
                    m.MisionId,
                    m.AbastecimientoId,
                    m.ContraparteId,
                    m.ExistenciaMedida,
                    m.MotivoDelAjuste,
                    m.Comprobante)));
}
