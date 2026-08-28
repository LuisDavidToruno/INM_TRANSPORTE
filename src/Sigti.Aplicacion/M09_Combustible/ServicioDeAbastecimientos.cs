using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Datos.Bitacora;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Datos.M09_Combustible;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M09_Combustible;

/// <summary>
/// Los ingresos de combustible que <b>no pasan por un vale</b> — `RN-83`.
///
/// ── Por qué hace falta una puerta aparte ────────────────────────────────────
/// Porque el circuito del vale sólo cubre el combustible del fondo, y ése es el que ya tenía
/// registro. Lo que faltaba es todo lo demás: el tanque de la sede, la donación en una
/// emergencia, el galón que puso el motorista de su bolsillo. Sin esta puerta esos galones
/// <b>no existen</b>, y `RN-30` los echa de menos como si fueran fraude.
///
/// ── Lo que este servicio NO hace ────────────────────────────────────────────
/// <b>No descuenta de las existencias del tanque institucional.</b> `RN-83` punto 5 lo exige, y
/// eso es un inventario de combustible que no está construido: el abastecimiento se registra e
/// imputa al vehículo, pero del otro lado no hay de qué restar.
/// </summary>
public sealed class ServicioDeAbastecimientos(SigtiDbContext contexto)
{
    private readonly AbastecimientosDeLaFlota _abastecimientos = new(contexto);
    private readonly ExpedientesDeMision _expedientes = new(contexto);
    private readonly EscritorDeBitacora _bitacora = new(contexto);

    /// <summary>
    /// Registra un ingreso de combustible de cualquier fuente <b>salvo el fondo</b>, que tiene su
    /// propia puerta —`V-04`— porque además mueve el vale.
    /// </summary>
    public async Task<Ulid> RegistrarAsync(
        Ulid id,
        Ulid vehiculo,
        DateTimeOffset ocurridoEn,
        decimal galones,
        int odometro,
        FuenteDeAbastecimiento fuente,
        IdPersona registra,
        Ulid? mision = null,
        decimal? monto = null,
        string? estacion = null,
        string? comprobante = null,
        string? causaSinComprobante = null,
        CancellationToken cancelacion = default)
    {
        if (fuente is FuenteDeAbastecimiento.FondoDeLaMision)
            throw new BloqueoDuro("RN-83",
                "El combustible con cargo al fondo se registra contra su vale, no por acá: mueve " +
                "el instrumento y descuenta del saldo. Use el consumo del vale.");

        // La misión, si se declara, tiene que existir y ser de ese vehículo. Imputarle galones a
        // una misión que no los recibió falsearía su rendimiento y el de la que sí los recibió.
        if (mision is { } idMision)
        {
            var expediente = await _expedientes.BuscarAsync(idMision, cancelacion)
                ?? throw new ExpedienteNoEncontrado(idMision);

            var deLaMision = expediente.Diario
                .LastOrDefault(t => t.Recursos is not null)?.Recursos?.Vehiculo;

            if (deLaMision is { } reservado && reservado != vehiculo)
                throw new BloqueoDuro("RN-83",
                    "Ese abastecimiento se está imputando a una misión que lleva otro vehículo. " +
                    "Los galones de un tanque no explican los kilómetros de otro.");
        }

        var abastecimiento = Abastecimiento.Registrar(
            id, vehiculo, ocurridoEn, galones, odometro, fuente, registra,
            mision, asignacion: null, monto, estacion, comprobante, causaSinComprobante);

        var estrategia = contexto.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion =
                await contexto.Database.BeginTransactionAsync(cancelacion);

            await _abastecimientos.GuardarAsync(abastecimiento, null, cancelacion);

            // La bitácora cuelga del VEHÍCULO, no de la misión: `RN-83` aplica en misión o fuera
            // de ella, y un reabastecimiento de rutina no tiene expediente al que anotarse.
            await _bitacora.EscribirAsync(
                $"vehiculo:{vehiculo}",
                $"Abastecimiento por {registra}: {abastecimiento.Descripcion}",
                ocurridoEn,
                cancelacion);

            await transaccion.CommitAsync(cancelacion);
        });

        return abastecimiento.Id;
    }

    public Task<IReadOnlyList<Abastecimiento>> DeLaMisionAsync(
        Ulid mision, CancellationToken cancelacion = default) =>
        _abastecimientos.DeLaMisionAsync(mision, cancelacion);

    public Task<IReadOnlyList<Abastecimiento>> DelVehiculoAsync(
        Ulid vehiculo, DateTimeOffset desde, DateTimeOffset hasta,
        CancellationToken cancelacion = default) =>
        _abastecimientos.DelVehiculoAsync(vehiculo, desde, hasta, cancelacion);
}
