using Microsoft.EntityFrameworkCore;
using Sigti.Datos;
using Sigti.Datos.Bitacora;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Dominio.M03_Flota;
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
public sealed class ServicioDeMisiones(SigtiDbContext contexto, EstadoDeLaFlota flota)
{
    private readonly ExpedientesDeMision _expedientes = new(contexto);
    private readonly EscritorDeBitacora _bitacora = new(contexto);

    public async Task<EstadoDeMision> CrearAsync(
        Ulid id,
        IdPersona capturadaPor,
        IdPersona solicitanteDeDerecho,
        DatosDeLaSolicitud solicitud,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var expediente = OrdenDeMision.Crear(id, capturadaPor, solicitanteDeDerecho, solicitud, momento);
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
    /// Mueve el estado operativo del vehículo cuando la transición lo exige — §10.2.
    ///
    /// ── Por qué vive acá y no en el agregado ────────────────────────────────
    /// Porque son <b>dos agregados</b>: la Orden de Misión y el vehículo. Que `OrdenDeMision`
    /// escribiera el estado del vehículo lo volvería responsable de un bien que no le
    /// pertenece, y la coordinación entre agregados es de la capa de aplicación.
    ///
    /// ── Y por qué DENTRO de la transacción ──────────────────────────────────
    /// Por lo mismo que el asiento de bitácora: si fueran dos transacciones, una caída entre
    /// ambas dejaría un vehículo <b>asignado a una misión que no se guardó</b>, o una misión
    /// programada sobre un vehículo que sigue figurando libre. Las dos son peores que fallar.
    ///
    /// ── Qué transiciones lo mueven, y por qué sólo ésas ─────────────────────
    /// §10.2: <i>«`ASIGNADO` y `EN_MISION` los fija el sistema, no una persona. Son
    /// consecuencia de transiciones de la Orden de Misión, y permitir fijarlos a mano abre la
    /// puerta a un vehículo "en misión" sin misión»</i>.
    ///
    /// ── `T-15` y `T-16` devuelven el vehículo, y no es lo mismo que liberarlo ──
    /// En las dos el vehículo <b>nunca salió</b>: está en el predio con las llaves puestas.
    /// La autoridad dice <i>«vuelve a `DISPONIBLE` o al estado que corresponda si la causa
    /// fue una falla»</i>, y esa segunda mitad NO se resuelve acá: quien declara el taller es
    /// `ACT-11` por el padrón (§10.2). Devolverlo a `DISPONIBLE` cuando la causa fue una
    /// avería lo dejaría figurando listo para la próxima misión — así que el estado se
    /// devuelve y <b>quien anuló debe declarar el taller si corresponde</b>. Inferirlo del
    /// motivo sería adivinar sobre un bien.
    /// </summary>
    private async Task MoverElEstadoDelVehiculoAsync(
        OrdenDeMision expediente, CancellationToken cancelacion)
    {
        var ultima = expediente.Diario[^1];

        var destino = ultima.Id switch
        {
            // Comprometido a una misión que aún no ha salido.
            "T-08" or "T-10" => EstadoOperativo.Asignado,

            // Fuera.
            "T-14" => EstadoOperativo.EnMision,

            // Vuelve a la flota. `T-11` desprograma, `T-13` anula la programada, `T-18`
            // registra el retorno, y `T-15`/`T-16` devuelven un vehículo que nunca salió:
            // en los cinco el vehículo deja de estar comprometido.
            "T-11" or "T-13" or "T-15" or "T-16" or "T-18" => EstadoOperativo.Disponible,

            _ => (EstadoOperativo?)null,
        };

        if (destino is not { } estado) return;

        // El vehículo sale de la reserva vigente, no de un campo: es la misma proyección
        // del diario que usa la ocupación de flota.
        var vehiculo = expediente.Diario
            .LastOrDefault(t => t.Recursos is not null)?.Recursos?.Vehiculo;

        // Sin reserva registrada no hay a qué vehículo moverle el estado. Pasa en los
        // expedientes anteriores a `RecursosTomados`, y no es un error: es que esa misión
        // nunca dejó dicho qué vehículo tomó.
        if (vehiculo is not { } id) return;

        await flota.AnotarAsync(id, new CambioDeEstadoOperativo(
            estado, ultima.Momento, ultima.Ejecuta.Valor,
            Motivo: $"{ultima.Id} de la misión {expediente.Id}",
            Automatico: true), cancelacion: cancelacion);
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

            await MoverElEstadoDelVehiculoAsync(expediente, cancelacion);

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
