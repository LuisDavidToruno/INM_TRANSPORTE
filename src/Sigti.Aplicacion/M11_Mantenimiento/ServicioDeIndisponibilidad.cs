using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.M11_Mantenimiento;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M11_Mantenimiento;

namespace Sigti.Aplicacion.M11_Mantenimiento;

/// <summary>
/// La indisponibilidad sobrevenida del vehículo — `RN-60`, M-11.
///
/// ── El acuse es lo que convierte el hecho en una decisión ───────────────────
/// <i>«Antes de confirmar la indisponibilidad, el sistema muestra las Órdenes de Misión afectadas
/// dentro del `horizonte_reservas_afectadas`: folio, dependencia solicitante, ventana, motorista
/// y objeto. <b>Quien ejecuta acusa</b>»</i>.
///
/// Sin ese paso, el conflicto aparece después y nadie lo decidió. Con él, quien manda el vehículo
/// al taller vio qué misiones quedaban en el aire y siguió adelante.
/// </summary>
public sealed class ServicioDeIndisponibilidad(SigtiDbContext contexto)
{
    /// <summary>
    /// Las reservas que se verían afectadas — <b>lo que se le muestra a quien va a acusar</b>.
    ///
    /// ── El horizonte, y por qué no es «todas» ───────────────────────────────
    /// `RN-60` lo declara configurable (`horizonte_reservas_afectadas`). Mostrar todas las
    /// misiones futuras del vehículo llenaría la lista de reservas que la indisponibilidad no
    /// alcanza, y una lista que nadie puede leer es un acuse que nadie leyó.
    ///
    /// ⚠️ El horizonte es la ventana estimada de la indisponibilidad, no un parámetro cargado:
    /// `horizonte_reservas_afectadas` sigue sin declararse. Es lo defendible sin inventarlo —
    /// las reservas que caen dentro de la ventana son exactamente las que quedan en el aire.
    /// </summary>
    public async Task<IReadOnlyList<ReservaAfectada>> ReservasAfectadasAsync(
        Ulid vehiculo, DateOnly desde, DateOnly hasta, CancellationToken cancelacion = default)
    {
        // Se buscan por la asignación viva del vehículo y por ventana que se solapa. El estado
        // se juzga por el diario (P-1), como todo en este sistema.
        var expedientes = await contexto.Expedientes
            .Include(e => e.Transiciones)
            .Where(e => e.Salida <= hasta && e.Retorno >= desde)
            .ToListAsync(cancelacion);

        var reservas = new List<ReservaAfectada>();

        foreach (var e in expedientes)
        {
            var ultima = e.Transiciones.OrderBy(t => t.Orden).LastOrDefault();

            if (ultima is null) continue;

            // `RN-60`: sólo las **ya PROGRAMADA o DESPACHADA**. Una solicitud sin programar no
            // tiene vehículo reservado, así que la indisponibilidad no le quita nada.
            if (ultima.Destino is not (EstadoDeMision.Programada or EstadoDeMision.Despachada))
                continue;

            // ── La reserva vive en el diario, no en una tabla aparte ─────────
            // «Liberar es no volver a tomar»: la última transición que tomó vehículo es la que
            // manda. Buscarlo en una tabla de reservas dejaría vehículos fantasma ocupados el
            // día que alguien olvide borrar.
            var reserva = e.Transiciones
                .Where(t => t.VehiculoTomado is not null)
                .OrderBy(t => t.Orden)
                .LastOrDefault();

            if (reserva?.VehiculoTomado != vehiculo) continue;

            reservas.Add(new ReservaAfectada(
                e.Id,

                // ⚠️ El ULID: la orden de misión sigue sin folio (`RN-44`).
                e.Id.ToString(),
                e.Dependencia,
                e.Salida,
                e.Retorno,
                reserva.ConductorTomado?.ToString() ?? "(sin motorista)",
                e.ObjetoDelTraslado,
                ultima.Destino));
        }

        return reservas;
    }

    /// <summary>
    /// Declara la indisponibilidad con su acuse — `RN-60`.
    ///
    /// <b>La lista se congela acá.</b> Lo que se guarda es lo que se le mostró a quien acusó, con
    /// su marca de tiempo, y no se reconstruye después.
    /// </summary>
    public async Task<Ulid> DeclararAsync(
        Ulid id,
        Ulid vehiculo,
        EstadoOperativo estado,
        string causa,
        DateOnly desde,
        DateOnly finEstimado,
        string ejecuta,
        DateTimeOffset momentoDelAcuse,
        CancellationToken cancelacion = default)
    {
        var reservas = await ReservasAfectadasAsync(vehiculo, desde, finEstimado, cancelacion);

        ReglasDeLaIndisponibilidad.ExigirCausaVentanaYAcuse(
            estado, causa, desde, finEstimado, ejecuta, reservas);

        var vigente = await contexto.Indisponibilidades
            .AnyAsync(i => i.VehiculoId == vehiculo && i.FinReal == null, cancelacion);

        if (vigente)
            throw new BloqueoDuro("RN-60",
                "Este vehículo ya está indisponible y no se ha dado de alta. Declarar una " +
                "segunda indisponibilidad dejaría dos ventanas abiertas sobre la misma unidad, " +
                "y las reservas en conflicto no sabrían a cuál responden.");

        var fila = new FilaDeIndisponibilidad
        {
            Id = id,
            VehiculoId = vehiculo,
            Estado = estado,
            Causa = causa.Trim(),
            Desde = desde,
            FinEstimado = finEstimado,
            Ejecuta = ejecuta.Trim(),
            MomentoDelAcuseUtc = momentoDelAcuse.UtcDateTime,
            DesfaseMinutos = (int)momentoDelAcuse.Offset.TotalMinutes,
        };

        foreach (var r in reservas)
            fila.Reservas.Add(new FilaDeReservaAfectada
            {
                Id = Ulid.NewUlid(),
                IndisponibilidadId = id,
                MisionId = r.Mision,
                Referencia = r.Referencia,
                Dependencia = r.Dependencia,
                Salida = r.Salida,
                Retorno = r.Retorno,
                Motorista = r.Motorista,
                ObjetoDelTraslado = r.ObjetoDelTraslado,
                EstadoAlAcusar = r.EstadoAlAcusar,
            });

        contexto.Indisponibilidades.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);

        return id;
    }

    /// <summary>
    /// Registra el desenlace de una reserva en conflicto — `RN-60` punto 4.
    ///
    /// <b>Esto no ejecuta la transición sobre la misión.</b> Sustituir el vehículo, reprogramar o
    /// anular son actos de la Orden de Misión, con su propia transición, su actor y su motivo.
    /// Acá se registra que el conflicto se resolvió y por cuál camino.
    /// </summary>
    public async Task ResolverReservaAsync(
        Ulid indisponibilidad,
        Ulid mision,
        DesenlaceDeLaReserva desenlace,
        string ejecuta,
        string motivo,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fila = await BuscarAsync(indisponibilidad, cancelacion);

        ReglasDeLaIndisponibilidad.ExigirDesenlaceRegistrable(A(fila), mision, motivo);

        fila.Resoluciones.Add(new FilaDeResolucionDeReserva
        {
            Id = Ulid.NewUlid(),
            IndisponibilidadId = indisponibilidad,
            MisionId = mision,
            Desenlace = desenlace,
            Ejecuta = ejecuta.Trim(),
            MomentoUtc = momento.UtcDateTime,
            DesfaseMinutos = (int)momento.Offset.TotalMinutes,
            Motivo = motivo.Trim(),
        });

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Da de alta el vehículo — `RN-60` punto 6: <b>fecha real, orden de trabajo cerrada y
    /// odómetro de salida</b>, contrastada contra la ventana estimada.
    /// </summary>
    public async Task DarDeAltaAsync(
        Ulid id,
        DateOnly finReal,
        string ordenDeTrabajo,
        int odometroDeSalida,
        CancellationToken cancelacion = default)
    {
        var fila = await BuscarAsync(id, cancelacion);

        ReglasDeLaIndisponibilidad.ExigirAltaConOrdenYOdometro(
            A(fila), finReal, ordenDeTrabajo, odometroDeSalida);

        fila.FinReal = finReal;
        fila.OrdenDeTrabajo = ordenDeTrabajo.Trim();
        fila.OdometroDeSalida = odometroDeSalida;

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Si una misión está en conflicto — <b>lo que el despacho consulta antes de `T-12`</b>.
    ///
    /// Devuelve <see cref="ConflictoPorIndisponibilidad.Ninguno"/> cuando no lo está, y nunca un booleano
    /// suelto: quien lo use está afirmando que consultó.
    /// </summary>
    public async Task<ConflictoPorIndisponibilidad> ConflictoDeAsync(
        Ulid mision, CancellationToken cancelacion = default)
    {
        var reserva = await contexto.Set<FilaDeReservaAfectada>()
            .Where(r => r.MisionId == mision)
            .ToListAsync(cancelacion);

        if (reserva.Count == 0) return ConflictoPorIndisponibilidad.Ninguno;

        var ids = reserva.Select(r => r.IndisponibilidadId).ToHashSet();

        var vigentes = await contexto.Indisponibilidades
            .Include(i => i.Resoluciones)
            .Where(i => ids.Contains(i.Id) && i.FinReal == null)
            .ToListAsync(cancelacion);

        // **Sin desenlace registrado.** `RN-60`: una reserva en conflicto no expira en silencio
        // ni se resuelve por el paso del tiempo.
        var enConflicto = vigentes
            .FirstOrDefault(i => !i.Resoluciones.Any(r => r.MisionId == mision));

        return enConflicto is null
            ? ConflictoPorIndisponibilidad.Ninguno
            : new ConflictoPorIndisponibilidad(true, enConflicto.Causa, enConflicto.FinEstimado);
    }

    public async Task<IReadOnlyList<IndisponibilidadDelVehiculo>> TodasAsync(
        CancellationToken cancelacion = default) =>
        [.. (await Consulta().OrderByDescending(i => i.Desde).ToListAsync(cancelacion)).Select(A)];

    private IQueryable<FilaDeIndisponibilidad> Consulta() =>
        contexto.Indisponibilidades
            .Include(i => i.Reservas)
            .Include(i => i.Resoluciones);

    private async Task<FilaDeIndisponibilidad> BuscarAsync(Ulid id, CancellationToken cancelacion) =>
        await Consulta().SingleOrDefaultAsync(i => i.Id == id, cancelacion)
            ?? throw new BloqueoDuro("RN-60", $"No hay indisponibilidad con id {id}.");

    private static IndisponibilidadDelVehiculo A(FilaDeIndisponibilidad f) => new(
        f.Id,
        f.VehiculoId,
        f.Estado,
        f.Causa,
        f.Desde,
        f.FinEstimado,
        f.Ejecuta,
        new DateTimeOffset(f.MomentoDelAcuseUtc, TimeSpan.Zero)
            .ToOffset(TimeSpan.FromMinutes(f.DesfaseMinutos)),
        [.. f.Reservas.Select(r => new ReservaAfectada(
            r.MisionId, r.Referencia, r.Dependencia, r.Salida, r.Retorno, r.Motorista,
            r.ObjetoDelTraslado, r.EstadoAlAcusar))],
        [.. f.Resoluciones.Select(r => new ResolucionDeLaReserva(
            r.MisionId,
            r.Desenlace,
            r.Ejecuta,
            new DateTimeOffset(r.MomentoUtc, TimeSpan.Zero)
                .ToOffset(TimeSpan.FromMinutes(r.DesfaseMinutos)),
            r.Motivo))],
        f.FinReal,
        f.OrdenDeTrabajo,
        f.OdometroDeSalida);
}
