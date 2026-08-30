using Sigti.Aplicacion.M03_Flota;
using Sigti.Datos;
using Sigti.Datos.Bitacora;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Aplicacion.M05_Motoristas;
using Sigti.Datos.M09_Combustible;
using Sigti.Aplicacion.M07_ProgramacionYDespacho;
using Microsoft.EntityFrameworkCore;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M09_Combustible;

/// <summary>
/// El circuito del combustible — `RN-26`, `RN-27`, `RN-32` y la máquina §10.1.
///
/// ── Por qué la emisión vive acá y no en el agregado ─────────────────────────
/// Porque emitir un vale <b>lee tres agregados</b>: el fondo (para el saldo), la Orden de
/// Misión (para el estado, el vehículo y el motorista) y el vehículo (para el tipo de
/// combustible). Que <see cref="AsignacionDeCombustible"/> los fuera a buscar la volvería
/// dependiente del repositorio, y las reglas dejarían de poder probarse solas (`ADR-009`).
///
/// Lo que este servicio <b>no</b> hace es decidir: las comprobaciones están en el dominio, y
/// acá sólo se traen los datos que necesitan.
/// </summary>
public sealed class ServicioDeCombustible(SigtiDbContext contexto)
{
    private readonly CombustibleDeLaInstitucion _combustible = new(contexto);
    private readonly ConsultaDeConductores _motoristas = new(contexto);
    private readonly ExpedientesDeMision _expedientes = new(contexto);
    private readonly EscritorDeBitacora _bitacora = new(contexto);
    private readonly AbastecimientosDeLaFlota _abastecimientos = new(contexto);
    private readonly ConsultaDeFlota _flota = new(contexto);

    // ── El fondo ────────────────────────────────────────────────────────────

    public async Task<Ulid> SolicitarFondoAsync(
        Ulid id,
        AmbitoDelFondo ambito,
        string ambitoDeclarado,
        DateOnly desde,
        DateOnly hasta,
        IdPersona solicita,
        decimal monto,
        string justificacion,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fondo = FondoDeCombustible.Solicitar(
            id, ambito, ambitoDeclarado, desde, hasta, solicita, monto, justificacion, momento);

        await ConfirmarFondoAsync(fondo, momento, cancelacion);
        return fondo.Id;
    }

    /// <summary>Aplica un movimiento sobre un fondo existente.</summary>
    public async Task<EstadoDelFondo> MoverFondoAsync(
        Ulid id,
        Action<FondoDeCombustible> movimiento,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fondo = await _combustible.BuscarFondoAsync(id, cancelacion)
            ?? throw new FondoNoEncontrado(id);

        movimiento(fondo);

        await ConfirmarFondoAsync(fondo, momento, cancelacion);
        return fondo.Estado;
    }

    /// <summary>
    /// `F-06` — cerrar el período. El recuento de asignaciones sin liquidar lo trae el
    /// servicio, porque el fondo no puede contar lo que no conoce.
    /// </summary>
    public async Task<EstadoDelFondo> CerrarFondoAsync(
        Ulid id,
        IdPersona liquida,
        string? partida,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var asignaciones = await _combustible.AsignacionesDelFondoAsync(id, cancelacion);
        var vivas = asignaciones.Count(a => !a.EstaResuelta);

        return await MoverFondoAsync(
            id, fondo => fondo.Cerrar(liquida, vivas, partida, momento), momento, cancelacion);
    }

    public Task<decimal> SaldoAsync(Ulid fondoId, CancellationToken cancelacion = default) =>
        _combustible.SaldoAsync(fondoId, cancelacion);

    /// <summary>
    /// Un fondo por su identificador.
    ///
    /// Existe por `I-19`: el control bloqueante compara **quien pretende aprobar contra quien
    /// solicito**, y esos actos estan en el diario del fondo. El fondo es objeto de PERIODO, asi
    /// que no le sirven los actos de ninguna mision.
    /// </summary>
    public Task<FondoDeCombustible?> BuscarFondoAsync(
        Ulid id, CancellationToken cancelacion = default) =>
        _combustible.BuscarFondoAsync(id, cancelacion);

    public Task<IReadOnlyList<FondoDeCombustible>> FondosAsync(
        CancellationToken cancelacion = default) =>
        _combustible.TodosLosFondosAsync(cancelacion);

    // ── El vale ─────────────────────────────────────────────────────────────

    /// <summary>
    /// `V-01` — emitir contra una misión.
    ///
    /// ── Lo que se trae, y de dónde ──────────────────────────────────────────
    /// El estado, el vehículo y el motorista salen <b>de la orden</b>, no de la petición:
    /// `RN-32` manda que el sistema los <i>precargue</i> y no los capture libremente. Recibirlos
    /// del cliente sería dejar que quien emite declare contra qué se está validando.
    /// </summary>
    public async Task<Ulid> EmitirAsync(
        Ulid id,
        string folio,
        Ulid fondoId,
        Ulid misionId,
        IdPersona emite,
        // **Quién está en la ventanilla**, por el ULID de su registro en el padrón. Se recibe
        // porque `RN-32` requisito 3 lo compara contra el motorista de la orden: pasarle el de
        // la orden a los dos lados dejaría el bloqueo comparando algo consigo mismo.
        Ulid motoristaReceptor,
        decimal monto,
        decimal? galones,
        string instrumento,
        string tipoDeCombustible,
        EstadoDeMision estadoMinimoConfigurado,
        decimal toleranciaSobregiro,
        DateTimeOffset momento,
        // **El circuito de reintegro.** Se recibe y no se construye acá porque necesita los
        // parámetros de la institución —el plazo, el calendario— y este servicio no los
        // tiene. Nulo <b>no</b> significa «sin deudas»: significa que quien llama no lo
        // consultó, y por eso se rechaza en vez de dejar pasar.
        ServicioDeReintegro reintegro,
        CancellationToken cancelacion = default)
    {
        var expediente = await _expedientes.BuscarAsync(misionId, cancelacion)
            ?? throw new ExpedienteNoEncontrado(misionId);

        var fondo = await _combustible.BuscarFondoAsync(fondoId, cancelacion)
            ?? throw new FondoNoEncontrado(fondoId);

        // `RN-26`: el fondo tiene que estar **vigente a la fecha del hecho** (P-4). Se juzga
        // contra el momento del acto y no contra el día de captura, que puede ser otro.
        if (!fondo.VigenteAl(DateOnly.FromDateTime(momento.Date)))
            throw new BloqueoDuro("RN-26",
                $"El fondo cubre del {fondo.Desde:dd/MM/yyyy} al {fondo.Hasta:dd/MM/yyyy}, y este " +
                $"vale se emite el {momento:dd/MM/yyyy}. No hay fondo vigente para esa fecha.");

        if (fondo.Estado is EstadoDelFondo.Solicitado)
            throw new BloqueoDuro("RN-26",
                "El fondo todavía no está aprobado. `RN-26` exige fondo aprobado vigente con " +
                "saldo: asignar contra una solicitud es comprometer dinero que nadie autorizó.");

        if (fondo.Estado is EstadoDelFondo.Cerrado)
            throw new BloqueoDuro("RN-26",
                "El fondo está cerrado. Imputarle un vale ahora reabriría un período ya " +
                "descargado, y el cuadre que se presentó dejaría de cuadrar.");

        ReglasDelFondo.ExigirMismoAmbito(
            fondo.Ambito, fondo.AmbitoDeclarado, expediente.Solicitud.Dependencia);

        // La reserva vigente dice qué vehículo y qué motorista tomó la misión. Es la misma
        // proyección del diario que usa la ocupación de flota — no una segunda tabla.
        var recursos = expediente.Diario
            .LastOrDefault(t => t.Recursos is not null)?.Recursos
            ?? throw new BloqueoDuro("RN-32",
                "La misión no tiene vehículo ni motorista reservados, así que no hay contra qué " +
                "validar el receptor. `INV-11`: aprobar no es programar.");

        // ⚠️ **El combustible del vehículo se RESUELVE acá, no lo manda el cliente.**
        //
        // `RN-32` compara el tipo del vale contra el que el vehículo usa —«un vale de diésel
        // para un vehículo de gasolina es un error caro y perfectamente evitable»—. Recibirlo
        // por parámetro dejaba la comparación en manos de quien emite: mandar el mismo valor
        // en los dos lados la vuelve una tautología, y pasar nulo la apaga.
        //
        // Nulo acá sí es legítimo: significa que **la ficha no lo declara**, y la regla lo
        // distingue de «coincide».
        var combustibleDelVehiculo = (await _flota.PorIdAsync(recursos.Vehiculo, cancelacion))
            ?.TipoDeCombustible;

        var saldo = await _combustible.SaldoAsync(fondoId, cancelacion);

        // ── `RN-86`, armado contra el receptor real ─────────────────────────────
        // Contra `motoristaReceptor` y no contra el motorista de la orden: es a quien se le
        // va a entregar el dinero, y `RN-32` ya se encarga de que sean el mismo. Evaluarlo
        // contra el de la orden dejaría pasar al sustituto que sí debe.
        var obligaciones = await reintegro.DeLaPersonaAsync(motoristaReceptor, cancelacion);
        var saldosAfuera = await reintegro.SaldosAfueraDeAsync(motoristaReceptor, cancelacion);
        var levantamiento = await reintegro.LevantamientoVigenteAsync(
            misionId, motoristaReceptor, cancelacion);

        var receptor = await _motoristas.PorIdAsync(motoristaReceptor, cancelacion);

        var asignacion = AsignacionDeCombustible.Emitir(
            id, folio, fondoId, misionId,
            expediente.Estado, estadoMinimoConfigurado,
            vehiculoDeLaOrden: recursos.Vehiculo,
            motoristaDeLaOrden: recursos.Conductor,

            // **El vehículo SÍ se precarga y el receptor NO.** `RN-32`: el sistema precarga
            // vehículo y motorista «y no los captura libremente» — pero su requisito 3 compara
            // contra «el receptor presente». Nadie teclea un vehículo en la ventanilla; a quien
            // tiene enfrente sí lo identifica quien entrega, y ésa es la comparación.
            vehiculoReceptor: recursos.Vehiculo,
            motoristaReceptor: motoristaReceptor,
            combustibleDelVehiculo, tipoDeCombustible,
            monto, galones, instrumento, emite, saldo, toleranciaSobregiro, momento,
            nombreDelReceptor: receptor?.Nombre ?? motoristaReceptor.ToString(),
            obligacionesDelReceptor: obligaciones,
            saldosDelReceptor: saldosAfuera,
            levantamiento: levantamiento);

        await ConfirmarAsignacionAsync(asignacion, momento, cancelacion);
        return asignacion.Id;
    }

    /// <summary>Aplica una transición sobre un vale existente.</summary>
    public async Task<EstadoDeAsignacion> TransicionarAsync(
        Ulid id,
        Action<AsignacionDeCombustible> transicion,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var asignacion = await _combustible.BuscarAsignacionAsync(id, cancelacion)
            ?? throw new AsignacionNoEncontrada(id);

        transicion(asignacion);

        await ConfirmarAsignacionAsync(asignacion, momento, cancelacion);
        return asignacion.Estado;
    }

    /// <summary>
    /// `V-02` — entregar. <b>Verifica que la misión esté despachada</b>, que es la regla de
    /// acoplamiento que §10.1 y `EF-04` imponen y que el agregado solo no puede comprobar.
    ///
    /// `PROGRAMADA` lista expresamente <i>«Entregar fondo de combustible»</i> entre lo que no
    /// se puede: el combustible entregado a una misión que todavía puede no salir es dinero
    /// fuera de la caja sin acto que lo respalde.
    /// </summary>
    public async Task<EstadoDeAsignacion> EntregarAsync(
        Ulid id,
        IdPersona entrega,
        string constancia,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var asignacion = await _combustible.BuscarAsignacionAsync(id, cancelacion)
            ?? throw new AsignacionNoEncontrada(id);

        var expediente = await _expedientes.BuscarAsync(asignacion.Mision, cancelacion)
            ?? throw new ExpedienteNoEncontrado(asignacion.Mision);

        if (expediente.Estado is not (EstadoDeMision.Despachada or EstadoDeMision.EnRuta))
            throw new BloqueoDuro("EF-04",
                $"La misión está {expediente.Estado} y el fondo se entrega dentro del despacho. " +
                "No se entrega fondo a una misión no despachada: mientras no se despacha, el " +
                "vale existe emitido y no sale de la custodia de quien lo guarda.");

        asignacion.Entregar(entrega, constancia, momento);

        await ConfirmarAsignacionAsync(asignacion, momento, cancelacion);
        return asignacion.Estado;
    }

    /// <summary>
    /// `V-04` — registrar consumo. <b>Sólo mientras la misión está `EN_RUTA`</b>, que es la
    /// otra regla de acoplamiento de §10.1.
    ///
    /// ── Y escribe el ABASTECIMIENTO, en la misma transacción ────────────────
    /// `RN-83`: <b>todo</b> ingreso de combustible al tanque se registra como abastecimiento,
    /// cualquiera sea su fuente. El consumo del vale es uno de ellos —fuente
    /// `FONDO_DE_LA_MISION`—, y si sólo viviera en el diario del vale, el numerador de
    /// `RN-30` tendría dos orígenes distintos según de dónde vino el galón.
    ///
    /// <b>No son dos hechos</b>: son el mismo visto desde dos lados. El asiento del vale mueve
    /// el instrumento; el abastecimiento cuenta el galón. Por eso van juntos o no van, y la
    /// base impide que dos abastecimientos apunten al mismo asiento.
    /// </summary>
    public async Task<EstadoDeAsignacion> RegistrarConsumoAsync(
        Ulid id,
        IdPersona consume,
        ConsumoRegistrado consumo,
        DateTimeOffset momento,
        Ulid? idDeCaptura = null,
        CancellationToken cancelacion = default)
    {
        var asignacion = await _combustible.BuscarAsignacionAsync(id, cancelacion)
            ?? throw new AsignacionNoEncontrada(id);

        var expediente = await _expedientes.BuscarAsync(asignacion.Mision, cancelacion)
            ?? throw new ExpedienteNoEncontrado(asignacion.Mision);

        // `RETORNADA` se admite: el consumo se captura **sin conectividad** y llega días
        // después, cuando el vehículo ya volvió. Rechazarlo por llegar tarde perdería el
        // hecho — y `P-2` dice que los hechos consumados se registran, no se bloquean.
        if (expediente.Estado is not (EstadoDeMision.EnRuta or EstadoDeMision.Retornada))
            throw new BloqueoDuro("V-04",
                $"La misión está {expediente.Estado}. Un consumo sólo ocurre en ruta: registrar " +
                "uno antes de salir sería declarar un gasto que todavía no pudo pasar.");

        asignacion.RegistrarConsumo(consume, consumo, momento, idDeCaptura);

        // El vehículo sale de la reserva de la misión, no de la petición: el abastecimiento
        // cuelga del vehículo (`RN-83` aplica en misión o fuera de ella) y dejar que el
        // cliente lo declare abriría la puerta a cargarle el galón a otro tanque.
        var vehiculo = expediente.Diario
            .LastOrDefault(t => t.Recursos is not null)?.Recursos?.Vehiculo
            ?? asignacion.Vehiculo;

        var abastecimiento = Abastecimiento.Registrar(
            Ulid.NewUlid(), vehiculo, momento, consumo.Galones, consumo.Odometro,
            FuenteDeAbastecimiento.FondoDeLaMision, consume,
            mision: asignacion.Mision,
            asignacion: asignacion.Id,
            monto: consumo.Monto,
            estacion: consumo.Estacion,
            comprobante: consumo.Comprobante,
            causaSinComprobante: consumo.CausaSinComprobante,
            // `RN-83` punto 6: lo que excede el fondo se registra igual, MARCADO. Omitirlo
            // dejaría el galón fuera del denominador, que es donde más falta hace.
            excedido: asignacion.Consumido > asignacion.Monto);

        await ConfirmarAsignacionAsync(
            asignacion, momento, cancelacion,
            abastecimiento, asignacion.Diario[^1].Id, asignacion.Diario.Count - 1);

        return asignacion.Estado;
    }

    /// <summary>
    /// Un vale por su identificador.
    ///
    /// Existe por el control bloqueante de §5.3.B: **el vale sabe de que mision es y la mision
    /// no sabe del vale**, asi que entregar el fondo tiene que entrar por aca para poder
    /// comparar contra los actos del expediente.
    /// </summary>
    public Task<AsignacionDeCombustible?> BuscarValeAsync(
        Ulid id, CancellationToken cancelacion = default) =>
        _combustible.BuscarAsignacionAsync(id, cancelacion);

    public Task<IReadOnlyList<AsignacionDeCombustible>> DeLaMisionAsync(
        Ulid misionId, CancellationToken cancelacion = default) =>
        _combustible.DeLaMisionAsync(misionId, cancelacion);

    public Task<RecuentoDeAsignaciones> RecuentoDeLaMisionAsync(
        Ulid misionId, CancellationToken cancelacion = default) =>
        _combustible.RecuentoDeLaMisionAsync(misionId, cancelacion);

    // ── Confirmación ────────────────────────────────────────────────────────

    /// <summary>
    /// El vale y su asiento de bitácora, <b>en la misma transacción</b>. Misma razón que en la
    /// misión: un movimiento de dinero sin rastro en bitácora es invisible para la auditoría, y
    /// un asiento de algo que no se guardó es peor todavía.
    /// </summary>
    /// <param name="abastecimiento">
    /// El ingreso de combustible que produjo esta transición, cuando la hubo. Va <b>dentro de
    /// la misma transacción</b>: un asiento `V-04` sin su abastecimiento dejaría el galón
    /// fuera del numerador de `RN-30`, y un abastecimiento sin su asiento movería el
    /// instrumento sin que nadie lo pidiera.
    /// </param>
    private async Task ConfirmarAsignacionAsync(
        AsignacionDeCombustible asignacion, DateTimeOffset momento, CancellationToken cancelacion,
        Abastecimiento? abastecimiento = null, string? transicion = null, int? orden = null)
    {
        var estrategia = contexto.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(cancelacion);

            await _combustible.GuardarAsignacionAsync(asignacion, cancelacion);

            if (abastecimiento is not null)
            {
                // El identificador del asiento recién escrito. Es lo que ata las dos filas y
                // lo que el índice único usa para impedir que el galón se cuente dos veces.
                var idDelAsiento = await _combustible.IdDeLaTransicionAsync(
                    asignacion.Id, orden!.Value, cancelacion);

                await _abastecimientos.GuardarAsync(
                    abastecimiento, idDelAsiento, cancelacion: cancelacion);
            }

            var ultima = asignacion.Diario[^1];
            await _bitacora.EscribirAsync(
                $"combustible:{asignacion.Id}",
                $"{ultima.Id} → {ultima.Destino} por {ultima.Ejecuta} · folio {asignacion.Folio}" +
                    (ultima.Motivo is null ? "" : $" · {ultima.Motivo}"),
                momento,
                cancelacion);

            await transaccion.CommitAsync(cancelacion);
        });
    }

    private async Task ConfirmarFondoAsync(
        FondoDeCombustible fondo, DateTimeOffset momento, CancellationToken cancelacion)
    {
        var estrategia = contexto.Database.CreateExecutionStrategy();

        await estrategia.ExecuteAsync(async () =>
        {
            await using var transaccion = await contexto.Database.BeginTransactionAsync(cancelacion);

            await _combustible.GuardarFondoAsync(fondo, cancelacion);

            var ultimo = fondo.Diario[^1];
            await _bitacora.EscribirAsync(
                $"fondo:{fondo.Id}",
                $"{ultimo.Id} → {ultimo.Destino} por {ultimo.Ejecuta}" +
                    (ultimo.Motivo is null ? "" : $" · {ultimo.Motivo}"),
                momento,
                cancelacion);

            await transaccion.CommitAsync(cancelacion);
        });
    }
}

public sealed class FondoNoEncontrado(Ulid id)
    : Exception($"No existe el fondo de combustible {id}.")
{
    public Ulid Id { get; } = id;
}

public sealed class AsignacionNoEncontrada(Ulid id)
    : Exception($"No existe la asignación de combustible {id}.")
{
    public Ulid Id { get; } = id;
}
