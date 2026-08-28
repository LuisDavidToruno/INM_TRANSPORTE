using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Datos.M07_ProgramacionYDespacho;

/// <summary>
/// Repositorio con intención, no genérico (`ADR-009`). Expone lo que el negocio
/// pregunta, no operaciones de tabla.
/// </summary>
public sealed class ExpedientesDeMision(SigtiDbContext contexto)
{
    /// <summary>
    /// Rehidrata el expediente <b>desde su diario</b>. No hay columna de estado que leer:
    /// el estado es la proyección (P-1).
    /// </summary>
    public async Task<OrdenDeMision?> BuscarAsync(Ulid id, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Expedientes
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == id, cancelacion);

        if (fila is null) return null;

        return OrdenDeMision.Reconstruir(
            fila.Id,
            new IdPersona(fila.CapturadaPor),
            new IdPersona(fila.SolicitanteDeDerecho),
            new DatosDeLaSolicitud(
                fila.Dependencia,
                fila.ObjetoDelTraslado,
                fila.Destino,
                new VentanaDeMision(fila.Salida, fila.Retorno, fila.HolguraDias,
                                    fila.HoraDeSalida, fila.HoraDeRetorno)),
            fila.Transiciones
                .OrderBy(t => t.Orden)
                .Select(t => new Transicion(
                    t.Transicion,
                    t.Destino,
                    new IdPersona(t.Ejecuta),
                    new DateTimeOffset(t.MomentoUtc, TimeSpan.Zero).ToOffset(TimeSpan.FromMinutes(t.DesfaseMinutos)),
                    t.Motivo,
                    t.IdDeCaptura,
                    // Los dos o ninguno. Una reserva a medias -- vehiculo sin conductor --
                    // no es un estado que el dominio pueda representar, y dejarla pasar
                    // pondria a ocupar un vehiculo sin decir quien lo lleva.
                    t.VehiculoTomado is { } vehiculo && t.ConductorTomado is { } conductor
                        ? new RecursosTomados(vehiculo, conductor)
                        : null,
                    t.Odometro)));
    }

    /// <summary>
    /// Persiste el expediente y las transiciones que aún no están guardadas.
    ///
    /// <b>Solo agrega.</b> Una transición ya escrita no se actualiza ni se borra: nada se
    /// deshace, ambas transiciones quedan en el diario para siempre (P-3).
    /// </summary>
    public async Task GuardarAsync(OrdenDeMision expediente, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Expedientes
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == expediente.Id, cancelacion);

        if (fila is null)
        {
            fila = new FilaDeExpediente
            {
                Id = expediente.Id,
                CapturadaPor = expediente.CapturadaPor.Valor,
                SolicitanteDeDerecho = expediente.SolicitanteDeDerecho.Valor,
                Dependencia = expediente.Solicitud.Dependencia,
                ObjetoDelTraslado = expediente.Solicitud.ObjetoDelTraslado,
                Destino = expediente.Solicitud.Destino,
                Salida = expediente.Solicitud.Ventana.Salida,
                Retorno = expediente.Solicitud.Ventana.Retorno,
                HoraDeSalida = expediente.Solicitud.Ventana.HoraDeSalida,
                HoraDeRetorno = expediente.Solicitud.Ventana.HoraDeRetorno,
                HolguraDias = expediente.Solicitud.Ventana.HolguraDias
            };
            contexto.Expedientes.Add(fila);
        }

        for (var orden = fila.Transiciones.Count; orden < expediente.Diario.Count; orden++)
        {
            var transicion = expediente.Diario[orden];

            fila.Transiciones.Add(new FilaDeTransicion
            {
                Id = Ulid.NewUlid(),
                ExpedienteId = expediente.Id,
                IdDeCaptura = transicion.IdDeCaptura,
                Orden = orden,
                Transicion = transicion.Id,
                Destino = transicion.Destino,
                Ejecuta = transicion.Ejecuta.Valor,
                MomentoUtc = transicion.Momento.UtcDateTime,
                DesfaseMinutos = (int)transicion.Momento.Offset.TotalMinutes,
                Motivo = transicion.Motivo,
                VehiculoTomado = transicion.Recursos?.Vehiculo,
                ConductorTomado = transicion.Recursos?.Conductor,
                Odometro = transicion.Odometro
            });
        }

        await contexto.SaveChangesAsync(cancelacion);
    }
}
