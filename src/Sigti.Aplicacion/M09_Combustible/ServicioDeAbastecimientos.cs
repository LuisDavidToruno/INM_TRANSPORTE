using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Sigti.Datos;
using Sigti.Datos.Bitacora;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Datos.M09_Combustible;
using Sigti.Dominio.M01_Organizacion;
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
/// ── El otro lado del tanque institucional ───────────────────────────────────
/// `RN-83` punto 5: el abastecimiento desde el tanque <b>descuenta de las existencias</b>. Se
/// hace acá, en la misma transacción: el asiento del tanque y el abastecimiento son <b>el
/// mismo hecho visto desde dos lados</b>, igual que `V-04` y el suyo.
///
/// El tanque es <b>opcional</b>, y no por comodidad. Un motorista que declara desde el campo
/// «cargué de la cisterna de la sede» está reportando un <b>hecho consumado</b>: no tiene el
/// tanque a mano, no puede firmar el despacho, y `RN-83` prohíbe omitir el registro. Ese
/// abastecimiento entra igual y queda como <b>discrepancia</b> — el préstamo invisible de
/// `CE-23`, ahora con nombre y en una lista.
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
        Ulid? idDeCaptura = null,

        // ── El otro lado, cuando la fuente es el tanque ─────────────────────────
        // Nulo significa **no se nombró tanque**, no «no hay tanque»: el abastecimiento se
        // registra igual y la discrepancia queda. Confundir las dos cosas convertiría un
        // hallazgo en un bloqueo, y el bloqueo perdería el galón en vez de encontrarlo.
        ServicioDeTanques? tanques = null,
        Ulid? tanque = null,
        Autoria? despacha = null,
        IdPersonaDelReceptor? recibe = null,
        string combustibleDelVehiculo = "",
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

            await _abastecimientos.GuardarAsync(abastecimiento, null, idDeCaptura, cancelacion);

            // **`RN-83` punto 5, cableado.** Dentro de la misma transacción: si el despacho
            // falla —existencia insuficiente, segregación, combustible incompatible— el
            // abastecimiento tampoco entra, y no queda un galón imputado a un vehículo contra
            // un tanque que nunca lo soltó.
            if (fuente is FuenteDeAbastecimiento.TanqueInstitucional && tanque is { } idTanque)
            {
                if (tanques is null || despacha is null || recibe is null)
                    throw new BloqueoDuro("RN-83",
                        "Para descontar del tanque hacen falta quién despacha y quién recibe. " +
                        "`RN-83` punto 5 exige responsable de despacho identificado con la " +
                        "misma segregación de `RN-01`: un egreso sin las dos personas no es un " +
                        "despacho, es una resta.");

                await tanques.DespacharAsync(
                    idTanque, despacha, galones, vehiculo, mision, abastecimiento.Id,
                    combustibleDelVehiculo, recibe.Value, ocurridoEn, cancelacion);
            }

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
