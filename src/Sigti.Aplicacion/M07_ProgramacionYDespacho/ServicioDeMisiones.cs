using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.Bitacora;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>
/// Ejecuta transiciones del expediente y deja su asiento en bitácora.
///
/// Vive en la capa de aplicación porque coordina dos cosas que ningún módulo posee por
/// sí solo: el expediente y la bitácora. Es la misma razón por la que `Sigti.Aplicacion`
/// existe — en SICOV, la regla «quien debe una liquidación no viaja» nació dentro de un
/// controlador y desde ahí el otro asistente de captura no podía consumirla.
/// </summary>
public sealed class ServicioDeMisiones(SigtiDbContext contexto)
{
    private readonly ExpedientesDeMision _expedientes = new(contexto);
    private readonly EscritorDeBitacora _bitacora = new(contexto);

    public async Task<EstadoDeMision> CrearAsync(
        Ulid id,
        IdPersona capturadaPor,
        IdPersona solicitanteDeDerecho,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var expediente = OrdenDeMision.Crear(id, capturadaPor, solicitanteDeDerecho, momento);
        await ConfirmarAsync(expediente, momento, cancelacion);
        return expediente.Estado;
    }

    /// <summary>
    /// Aplica una transición sobre un expediente existente.
    ///
    /// El delegado recibe el expediente ya rehidratado: si la transición viola una
    /// precondición de bloqueo duro, lanza <see cref="BloqueoDuro"/> <b>antes</b> de que
    /// se escriba nada.
    /// </summary>
    public async Task<EstadoDeMision> TransicionarAsync(
        Ulid id,
        Action<OrdenDeMision> transicion,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var expediente = await _expedientes.BuscarAsync(id, cancelacion)
            ?? throw new ExpedienteNoEncontrado(id);

        transicion(expediente);

        await ConfirmarAsync(expediente, momento, cancelacion);
        return expediente.Estado;
    }

    /// <summary>
    /// El expediente y su asiento se confirman <b>en la misma transacción</b>.
    ///
    /// Si fueran dos transacciones, una caída entre ambas dejaría una de dos cosas: una
    /// transición sin rastro en la bitácora —invisible para la auditoría— o un asiento de
    /// algo que no ocurrió. Las dos son peores que fallar.
    /// </summary>
    private async Task ConfirmarAsync(
        OrdenDeMision expediente, DateTimeOffset momento, CancellationToken cancelacion)
    {
        var estrategia = contexto.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(cancelacion);

            await _expedientes.GuardarAsync(expediente, cancelacion);

            var ultima = expediente.Diario[^1];
            await _bitacora.EscribirAsync(
                expediente.ColaDeBitacora,
                $"{ultima.Id} → {ultima.Destino} por {ultima.Ejecuta}" +
                    (ultima.Motivo is null ? "" : $" · motivo: {ultima.Motivo}"),
                momento,
                cancelacion);

            await transaccion.CommitAsync(cancelacion);
        });
    }
}

public sealed class ExpedienteNoEncontrado(Ulid id)
    : Exception($"No existe el expediente de misión {id}.")
{
    public Ulid Id { get; } = id;
}
