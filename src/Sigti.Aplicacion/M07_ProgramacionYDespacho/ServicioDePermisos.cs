using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M01_Organizacion;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Aplicacion.M03_Flota;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Dominio;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M07_ProgramacionYDespacho;

/// <summary>
/// `PT-020` y `PT-021` — el trámite del permiso de circulación y su firma.
///
/// ── ⚠️ Lo que este servicio vino a destrabar ────────────────────────────────
/// `BD-04` bloquea el despacho de toda misión que circule en franja inhábil sin permiso de la
/// máxima autoridad, y estaba escrito, probado y operando. <b>Pero nadie podía emitir un
/// permiso</b>: la tabla existía y sólo se leía. Cualquier misión que tocara un sábado, un
/// domingo o un feriado era indespachable, y el mensaje del bloqueo decía «no hay ningún
/// permiso registrado para esta misión» sin que existiera forma de registrar uno.
///
/// ── Abrir y firmar son dos actos, y esa es la razón de ser del estado ───────
/// Ver <see cref="ReglasDelPermiso"/>. Acá sólo importa la consecuencia: un trámite
/// <c>SOLICITADO</c> <b>no entra en `BD-04`</b> — <see cref="ConsultaDePermisos"/> filtra por
/// estado. Si entrara, cualquiera destrabaría el domingo abriendo un trámite y despachando sin
/// esperar la firma.
/// </summary>
public sealed class ServicioDePermisos(
    SigtiDbContext contexto,
    ServicioDeCompetencias competencias,
    ConsultaDeFlota flota,
    IParametrosDeLaInstitucion parametros)
{
    /// <summary>
    /// Abre el trámite y lo encamina a la máxima autoridad.
    ///
    /// <b>Se abre sin vehículo ni motorista.</b> `RN-23` no exige que la misión esté programada
    /// —y tiene que poder adelantarse, que es lo que se hace un viernes por la tarde para una
    /// salida del sábado—. Lo que no se puede es firmarlo así.
    /// </summary>
    /// <returns>
    /// El identificador del trámite, o el motivo por el que <b>no hacía falta</b> abrirlo.
    /// Decirle «no puede» a quien la respuesta correcta es «no le hace falta» lo manda a
    /// resolver un problema que no tiene.
    /// </returns>
    public async Task<AperturaResuelta> AbrirAsync(
        Ulid id,
        Ulid expediente,
        string justificacion,
        IdPersona solicita,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        if (string.IsNullOrWhiteSpace(justificacion))
        {
            throw new BloqueoDuro("RN-23",
                "Escriba por qué la misión tiene que circular en franja inhábil. Es lo único " +
                "que la máxima autoridad tiene para decidir: sin eso, firmar es un trámite.");
        }

        var fila = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == expediente, cancelacion)
            ?? throw new ExpedienteNoEncontrado(expediente);

        var ventana = new VentanaDeMision(
            fila.Salida, fila.Retorno, fila.HolguraDias, fila.HoraDeSalida, fila.HoraDeRetorno);

        // ⚠️ El calendario se resuelve **a la fecha del hecho** (`P-4`, `RN-40`), no a hoy. Un
        // permiso tramitado en septiembre para una salida de marzo se juzga con el calendario
        // que regía en marzo.
        var calendario = parametros.CalendarioVigenteAl(ventana.Salida);

        var tramos = Tramos(calendario, ventana);

        if (tramos.Count == 0)
        {
            return AperturaResuelta.NoHizoFalta(new NoHaceFalta(
                "SIN_FRANJA_INHABIL",
                "La ventana de la misión no toca ningún día ni ninguna hora inhábil con el " +
                $"calendario {calendario.Version}. No requiere permiso."));
        }

        // El vehículo puede no estar asignado todavía — y entonces no se puede saber si tiene
        // excepción. **Nulo acá no es «no tiene excepción»**: es que no se pudo mirar, y por eso
        // el trámite se abre igual en vez de darse por innecesario.
        var excepcion = await ExcepcionSiSeSabeAsync(Reserva(fila).Vehiculo, cancelacion);

        var existentes = await DelExpedienteAsync(expediente, cancelacion);

        var apertura = new AperturaDelPermiso(
            expediente, fila.Destino, ventana.Salida, ventana.FinDelRango, excepcion, existentes);

        if (ReglasDelPermiso.PorQueNoHaceFalta(apertura) is { } porQue)
            return AperturaResuelta.NoHizoFalta(porQue);

        contexto.Permisos.Add(new FilaDePermisoDeCirculacion
        {
            Id = id,

            // ⚠️ Folio provisional, como el de la Orden de Misión: los rangos por delegación
            // son de `M-01` y el formato es insumo #34. Se marca para que nadie lo confunda
            // con el correlativo oficial del salvoconducto impreso.
            Folio = $"PC-PROV-{id.ToString()[^8..]}",

            ExpedienteId = expediente,
            Estado = EstadoDelPermiso.Solicitado.ToString(),

            // Nulos a propósito: se resuelven al programar, y la firma los exige.
            Vehiculo = null,
            Motorista = null,
            EmitidoPor = null,

            Destino = fila.Destino,
            Desde = ventana.Salida,
            Hasta = ventana.FinDelRango,
            Solicita = solicita.Valor,
            SolicitadoEnUtc = momento.UtcDateTime,
            Justificacion = justificacion.Trim(),

            // Congelados con el calendario vigente a la fecha del hecho. No se recalculan al
            // firmar: si el calendario cambiara entre el trámite y la firma, el permiso ampara
            // lo que se pidió amparar, no lo que hoy sería inhábil.
            TramosInhabiles = string.Join(" · ", tramos),
        });

        await contexto.SaveChangesAsync(cancelacion);

        return AperturaResuelta.Abierto(id, tramos);
    }

    /// <summary>
    /// La firma de la máxima autoridad — `ACT-09`, y sólo ella.
    ///
    /// <b>Responde el motivo en lugar de lanzar</b>, porque el intento se registra igual: que
    /// alguien que no es la máxima autoridad haya intentado firmar un permiso de circulación es
    /// justamente lo que un control interno quiere poder ver.
    /// </summary>
    public async Task<IntentoDeFirma> FirmarAsync(
        Ulid id,
        IdPersona quienFirma,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Permisos.SingleOrDefaultAsync(p => p.Id == id, cancelacion)
            ?? throw new PermisoNoEncontrado(id);

        // Los recursos se toman del EXPEDIENTE, no de la fila: entre el trámite y la firma la
        // misión se programó, y es esa asignación la que el permiso viene a amparar.
        var expediente = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == fila.ExpedienteId, cancelacion)
            ?? throw new ExpedienteNoEncontrado(fila.ExpedienteId);

        var reserva = Reserva(expediente);

        var enTramite = Convertir(fila) with
        {
            Vehiculo = reserva.Vehiculo,
            Motorista = reserva.Motorista,
        };

        // ⚠️ Los roles se resuelven **a la fecha del hecho**, no a hoy: quien era máxima
        // autoridad el día de la salida es quien podía firmar ese permiso.
        var suyas = await competencias.DeLaPersonaAsync(quienFirma, fila.Desde, cancelacion);

        var motivo = ReglasDelPermiso.PorQueNoSeFirma(enTramite, suyas.Roles);
        var intento = new IntentoDeFirma(fila.Folio, quienFirma, momento, motivo is null, motivo);

        if (!intento.Concedida) return intento;

        fila.Estado = EstadoDelPermiso.Firmado.ToString();
        fila.EmitidoPor = quienFirma.Valor;
        fila.FirmadoEnUtc = momento.UtcDateTime;

        // Se copian del expediente **al firmar**, y quedan congelados: el permiso es un
        // documento con vida propia. Si mañana hay relevo de motorista, el permiso no cambia
        // — deja de amparar, que es lo que `RN-23` prescribe.
        fila.Vehiculo = enTramite.Vehiculo;
        fila.Motorista = enTramite.Motorista;

        await contexto.SaveChangesAsync(cancelacion);
        return intento;
    }

    /// <summary>
    /// Retira un trámite que ya no se pide — la misión se reprogramó a franja hábil, o se anuló.
    ///
    /// <b>No borra.</b> Que alguien haya pedido circular un domingo es un hecho, y que se haya
    /// desistido también: uno desaparecido y uno que nunca existió se ven iguales.
    /// </summary>
    public async Task DesistirAsync(
        Ulid id, string motivo, CancellationToken cancelacion = default)
    {
        var fila = await contexto.Permisos.SingleOrDefaultAsync(p => p.Id == id, cancelacion)
            ?? throw new PermisoNoEncontrado(id);

        if (fila.Estado == EstadoDelPermiso.Firmado.ToString())
        {
            throw new BloqueoDuro("RN-23",
                $"El permiso {fila.Folio} ya está firmado. Un permiso firmado no se retira: " +
                "si la misión cambió, reemítalo — el firmado queda como el acto que fue.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("RN-23", "Diga por qué se retira el trámite.");

        fila.Estado = EstadoDelPermiso.Desistido.ToString();
        fila.MotivoDelDesistimiento = motivo.Trim();

        await contexto.SaveChangesAsync(cancelacion);
    }

    /// <summary>
    /// Reemite el permiso cuando cambió lo que ampara — `HU-018`.
    ///
    /// ── Tres cosas en un solo acto, y las tres importan ─────────────────────
    /// <b>1 · Anula el salvoconducto anterior</b>, con motivo y autor (`RN-04`). El papel sigue
    /// impreso y en la mano de alguien: el punto de verificación tiene que empezar a decir que
    /// no vale <b>de inmediato</b>, o un documento anulado pasa un control.
    ///
    /// <b>2 · Desiste el permiso anterior.</b> Deja de contar para `BD-04` — si siguiera
    /// firmado, el despacho podría ampararse en él aunque ya no cubra.
    ///
    /// <b>3 · Abre un trámite nuevo SIN FIRMA.</b> `HU-018` es literal: <i>«el permiso nuevo
    /// requiere firma nueva, no la firma anterior»</i>. Arrastrarla convertiría el acto de la
    /// máxima autoridad en una casilla heredada, y lo que firmó fue <b>otro</b> vehículo con
    /// <b>otro</b> motorista.
    ///
    /// ── ⚠️ Y por qué el folio nuevo no recicla el anterior ──────────────────
    /// `RN-04`: el folio no se recicla. Dos papeles distintos con el mismo folio son
    /// indistinguibles para quien los compara, y el anulado seguiría verificando como el vivo.
    /// </summary>
    public async Task<Ulid> ReemitirAsync(
        Ulid id,
        string motivo,
        IdPersona quien,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var anterior = await contexto.Permisos.SingleOrDefaultAsync(p => p.Id == id, cancelacion)
            ?? throw new PermisoNoEncontrado(id);

        if (ReglasDelPermiso.PorQueNoSeReemite(Convertir(anterior), motivo) is { } porQue)
            throw new BloqueoDuro("RN-23", porQue);

        var expediente = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == anterior.ExpedienteId, cancelacion)
            ?? throw new ExpedienteNoEncontrado(anterior.ExpedienteId);

        var ventana = new VentanaDeMision(
            expediente.Salida, expediente.Retorno, expediente.HolguraDias,
            expediente.HoraDeSalida, expediente.HoraDeRetorno);

        var calendario = parametros.CalendarioVigenteAl(ventana.Salida);
        var tramos = Tramos(calendario, ventana);

        var nuevo = Ulid.NewUlid();

        // ── 1 · El papel anterior deja de valer, con quién y por qué ────────
        var papel = await contexto.Salvoconductos
            .SingleOrDefaultAsync(sc => sc.PermisoId == id, cancelacion);

        if (papel is not null)
        {
            papel.Anulado = true;
            papel.MotivoDeLaAnulacion = motivo.Trim();
            papel.AnuladoPor = quien.Valor;
            papel.AnuladoEnUtc = momento.UtcDateTime;
        }

        // ── 2 · El permiso anterior deja de contar para BD-04 ───────────────
        anterior.Estado = EstadoDelPermiso.Desistido.ToString();
        anterior.MotivoDelDesistimiento = $"Reemitido: {motivo.Trim()}";

        // ── 3 · El nuevo nace SIN FIRMA ─────────────────────────────────────
        contexto.Permisos.Add(new FilaDePermisoDeCirculacion
        {
            Id = nuevo,
            Folio = $"PC-PROV-{nuevo.ToString()[^8..]}",
            ExpedienteId = anterior.ExpedienteId,

            // La referencia cruzada de `RN-04`. Sin ella un auditor ve dos folios para una
            // misma misión y nada dice cuál superó a cuál.
            Reemplaza = anterior.Id,

            Estado = EstadoDelPermiso.Solicitado.ToString(),

            // ⚠️ Nulos **a propósito**. No se copian del anterior: se resuelven al firmar
            // contra la reserva de HOY, que es justamente lo que cambió.
            Vehiculo = null,
            Motorista = null,
            EmitidoPor = null,
            FirmadoEnUtc = null,

            // El destino y la ventana salen del EXPEDIENTE, no del permiso anterior: si lo que
            // cambió fue la ventana, copiarla del anterior reproduciría el problema.
            Destino = expediente.Destino,
            Desde = ventana.Salida,
            Hasta = ventana.FinDelRango,

            Solicita = quien.Valor,
            SolicitadoEnUtc = momento.UtcDateTime,
            Justificacion = anterior.Justificacion,

            // Se recalculan contra el calendario vigente a la fecha del hecho: si lo que cambió
            // fue la ventana, los tramos inhábiles que hay que cubrir son otros.
            TramosInhabiles = string.Join(" · ", tramos),
        });

        await contexto.SaveChangesAsync(cancelacion);
        return nuevo;
    }

    /// <summary>Todos los trámites de un expediente, en cualquier estado.</summary>
    public async Task<IReadOnlyList<PermisoEnTramite>> DelExpedienteAsync(
        Ulid expediente, CancellationToken cancelacion = default)
    {
        var filas = await contexto.Permisos
            .AsNoTracking()
            .Where(p => p.ExpedienteId == expediente)
            .ToListAsync(cancelacion);

        return [.. filas.Select(Convertir)];
    }

    /// <summary>
    /// Los trámites de un expediente <b>con el diagnóstico de si todavía cubren</b> — `PT-024`.
    ///
    /// ── Por qué el diagnóstico va acá y no al despachar ─────────────────────
    /// `BD-04` ya bloquea el despacho con un permiso que dejó de cubrir, y llega tarde: el
    /// sábado por la mañana, con el vehículo cargado y la máxima autoridad sin trabajar. Esta
    /// consulta contesta la misma pregunta <b>el jueves</b>, en la pantalla donde el Jefe de
    /// Transporte ya está mirando el expediente.
    ///
    /// Y contesta <b>qué</b> cambió, no sólo que cambió: cada elemento tiene su propio arreglo.
    /// </summary>
    public async Task<IReadOnlyList<PermisoDiagnosticado>> DiagnosticoDelExpedienteAsync(
        Ulid expediente, CancellationToken cancelacion = default)
    {
        var filas = await contexto.Permisos
            .AsNoTracking()
            .Where(p => p.ExpedienteId == expediente)
            .ToListAsync(cancelacion);

        if (filas.Count == 0) return [];

        var mision = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .SingleOrDefaultAsync(e => e.Id == expediente, cancelacion)
            ?? throw new ExpedienteNoEncontrado(expediente);

        var reserva = Reserva(mision);
        var ultima = mision.Transiciones.MaxBy(t => t.Orden);

        // ⚠️ La excepción deliberada de `HU-018`: **un relevo documentado en ruta no invalida
        // el permiso de la misión ya iniciada**. El vehículo está en la carretera; declarar el
        // papel inválido no lo devuelve, y sí dejaría al motorista relevado sin nada.
        var enRuta = ultima?.Destino is EstadoDeMision.EnRuta or EstadoDeMision.Despachada;

        var ventana = new VentanaDeMision(
            mision.Salida, mision.Retorno, mision.HolguraDias,
            mision.HoraDeSalida, mision.HoraDeRetorno);

        var resultado = new List<PermisoDiagnosticado>();

        foreach (var f in filas.OrderBy(f => f.SolicitadoEnUtc))
        {
            var permiso = Convertir(f);

            var porQue = ReglasDelPermiso.PorQueYaNoCubre(
                permiso,
                reserva.Vehiculo, reserva.Motorista,
                mision.Destino, ventana.Salida, ventana.FinDelRango,
                await NombreDeVehiculoAsync(permiso.Vehiculo, cancelacion) ?? "(sin asignar)",
                await NombreDeVehiculoAsync(reserva.Vehiculo, cancelacion) ?? "(sin asignar)",
                enRuta);

            resultado.Add(new PermisoDiagnosticado(
                permiso,
                await NombreDeVehiculoAsync(permiso.Vehiculo, cancelacion),
                await NombreDeMotoristaAsync(permiso.Motorista, cancelacion),
                porQue?.Detalle,
                porQue?.ExigeReemision ?? false,

                // **Ampara** es la conjunción de las dos cosas: firmado Y todavía cubre. Que
                // estén separadas en el contrato deja que la pantalla explique cuál de las dos
                // falta, que es lo accionable.
                Ampara: permiso.Estado == EstadoDelPermiso.Firmado && porQue is null));
        }

        return resultado;
    }

    /// <summary>
    /// La bandeja de firma de `PT-021`.
    ///
    /// Trae también los que <b>no se pueden firmar todavía</b> por falta de programación: la
    /// máxima autoridad tiene que ver que hay algo esperándola aunque no pueda resolverlo, o
    /// creerá que no hay nada y el trámite se descubrirá el sábado.
    /// </summary>
    public async Task<IReadOnlyList<PermisoParaFirmar>> PendientesAsync(
        CancellationToken cancelacion = default)
    {
        var filas = await contexto.Permisos
            .AsNoTracking()
            .Where(p => p.Estado == EstadoDelPermiso.Solicitado.ToString())
            .OrderBy(p => p.Desde)
            .ToListAsync(cancelacion);

        if (filas.Count == 0) return [];

        var expedientes = filas.Select(f => f.ExpedienteId).Distinct().ToList();

        var misiones = await contexto.Expedientes
            .AsNoTracking()
            .Include(e => e.Transiciones)
            .Where(e => expedientes.Contains(e.Id))
            .ToListAsync(cancelacion);

        var resultado = new List<PermisoParaFirmar>();

        foreach (var f in filas)
        {
            var mision = misiones.SingleOrDefault(m => m.Id == f.ExpedienteId);

            var reserva = mision is null ? default : Reserva(mision);

            var enTramite = Convertir(f) with
            {
                Vehiculo = reserva.Vehiculo,
                Motorista = reserva.Motorista,
            };

            resultado.Add(new PermisoParaFirmar(
                enTramite,
                mision?.FolioTexto,
                mision?.Dependencia ?? "",
                mision?.ObjetoDelTraslado ?? "",

                // Se resuelven los nombres acá y no en la pantalla: el agente en carretera lee
                // un nombre, no un ULID, y quien firma decide sobre lo mismo que se imprime.
                await NombreDeVehiculoAsync(enTramite.Vehiculo, cancelacion),
                await NombreDeMotoristaAsync(enTramite.Motorista, cancelacion),

                // La razón por la que no se puede firmar, resuelta acá para que la pantalla no
                // la reimplemente. Nulo es que sí se puede.
                ReglasDelPermiso.PorQueNoSeFirma(enTramite, [Rol.MaximaAutoridad])));
        }

        return resultado;
    }

    private static PermisoEnTramite Convertir(FilaDePermisoDeCirculacion f) => new(
        f.Id,
        f.Folio,
        Enum.Parse<EstadoDelPermiso>(f.Estado),
        f.Vehiculo,
        f.Motorista,
        f.Destino,
        f.Desde,
        f.Hasta,
        f.Justificacion,
        f.TramosInhabiles.Length == 0 ? [] : [.. f.TramosInhabiles.Split(" · ")],
        new IdPersona(f.Solicita),
        f.EmitidoPor is null ? null : new IdPersona(f.EmitidoPor))
    { Reemplaza = f.Reemplaza };

    /// <summary>
    /// Los estados en los que la reserva <b>cuenta</b>.
    ///
    /// Mismos que los de <c>ConsultaDeOcupacion</c>, y por la misma razón: la ocupación es la
    /// proyección del diario y sólo vale mientras el estado la sostiene.
    /// </summary>
    private static readonly EstadoDeMision[] Sostienen =
    [
        EstadoDeMision.Programada,
        EstadoDeMision.Despachada,
        EstadoDeMision.EnRuta,
    ];

    /// <summary>
    /// Qué vehículo y qué motorista tiene reservados la misión <b>hoy</b>.
    ///
    /// Sale de la última transición que tomó recursos, no de una columna: el expediente no
    /// guarda «el vehículo asignado» — lo asignado es un hecho del diario (`P-1`), y un relevo
    /// posterior es otro hecho que lo supera. Leer una columna daría el primero para siempre.
    ///
    /// ── ⚠️ Y no basta con la última transición que tomó recursos ────────────
    /// <b>Liberar es no volver a tomar.</b> `T-11` desprograma y la transición de `T-08` que
    /// reservó <b>permanece en el diario</b> —nada se deshace (`P-3`)—, simplemente deja de
    /// contar. Mirar sólo la última con vehículo devolvería la reserva de una misión que ya
    /// volvió a `APROBADA` y no tiene ninguna.
    ///
    /// Costó el estado <c>Desactualizado</c> del salvoconducto: desprogramar una misión dejaba
    /// el papel impreso contestando «documento válido» a quien lo verificara en la carretera,
    /// porque acá seguía apareciendo el motorista que ya no iba.
    ///
    /// <b>Nulos cuando la misión no sostiene reserva.</b> Ésa es la condición que impide firmar.
    /// </summary>
    internal static (Ulid? Vehiculo, Ulid? Motorista) Reserva(FilaDeExpediente expediente)
    {
        var ultima = expediente.Transiciones.MaxBy(t => t.Orden);

        if (ultima is null || !Sostienen.Contains(ultima.Destino)) return (null, null);

        var reserva = expediente.Transiciones
            .Where(t => t.VehiculoTomado is not null)
            .MaxBy(t => t.Orden);

        return (reserva?.VehiculoTomado, reserva?.ConductorTomado);
    }

    private static IReadOnlyList<string> Tramos(
        CalendarioDeDiasHabiles calendario, VentanaDeMision ventana) =>
    [
        .. calendario.InhabilesEn(ventana).Select(d => d.ToString("dd/MM/yyyy")),
        .. calendario.HorasInhabilesEn(ventana),
    ];

    private async Task<ServicioExceptuado?> ExcepcionSiSeSabeAsync(
        Ulid? vehiculo, CancellationToken cancelacion)
    {
        if (vehiculo is null) return null;
        return (await flota.PorIdAsync(vehiculo.Value, cancelacion))?.Excepcion();
    }

    private async Task<string?> NombreDeVehiculoAsync(Ulid? id, CancellationToken cancelacion)
    {
        if (id is null) return null;
        var v = await flota.PorIdAsync(id.Value, cancelacion);
        // Las siglas institucionales primero: **es lo que el agente compara con la calcomanía
        // del vehículo**, y `CE-17` deja la placa sin ser obligatoria por el desabastecimiento.
        return v is null
            ? null
            : $"{v.Siglas} · {v.TipoDeVehiculo} · " +
              (v.Placa is null ? "sin placa metálica" : $"placa {v.Placa}");
    }

    private async Task<string?> NombreDeMotoristaAsync(Ulid? id, CancellationToken cancelacion)
    {
        if (id is null) return null;

        return await contexto.Conductores
            .AsNoTracking()
            .Where(c => c.Id == id.Value)
            .Select(c => c.Nombre)
            .SingleOrDefaultAsync(cancelacion);
    }
}

/// <param name="NoHizoFaltaPorque">
/// Nulo cuando el trámite <b>sí</b> hacía falta y se abrió. No nulo cuando no: el vehículo tiene
/// excepción, ya hay un permiso que cubre lo mismo, o la ventana no toca franja inhábil.
/// </param>
public sealed record AperturaResuelta(
    Ulid? Id, IReadOnlyList<string> Tramos, NoHaceFalta? NoHizoFaltaPorque)
{
    public static AperturaResuelta Abierto(Ulid id, IReadOnlyList<string> tramos) =>
        new(id, tramos, null);

    public static AperturaResuelta NoHizoFalta(NoHaceFalta porQue) => new(null, [], porQue);
}

/// <summary>
/// El intento de firma, concedido o no. <b>Se devuelve en los dos casos</b> porque en los dos
/// se registra: un bloqueo perfecto y uno que nunca se activó se ven iguales.
/// </summary>
public sealed record IntentoDeFirma(
    string Folio, IdPersona Quien, DateTimeOffset Momento, bool Concedida, string? Motivo);

/// <param name="PorQueNoSeFirma">
/// Nulo es que sí se puede firmar. Se resuelve acá y no en la pantalla para que no exista una
/// segunda copia de la regla que después diverja.
/// </param>
public sealed record PermisoParaFirmar(
    PermisoEnTramite Permiso,
    string? FolioDeLaMision,
    string Dependencia,
    string ObjetoDelTraslado,
    string? Vehiculo,
    string? Motorista,
    string? PorQueNoSeFirma);

public sealed class PermisoNoEncontrado(Ulid id)
    : Exception($"No existe el permiso {id}.");

/// <param name="PorQueYaNoCubre">
/// Nulo es que sigue cubriendo —o que nunca llegó a cubrir, porque no está firmado—. No nulo es
/// <b>qué elemento cambió</b>, en palabras que dicen qué hacer con él.
/// </param>
/// <param name="ExigeReemision">
/// ⚠️ <b>No todo lo que deja de cubrir hay que reemitirlo.</b> Falso significa <b>espere</b>
/// —la misión se desprogramó y puede volver a amparar sola—; verdadero significa <b>actúe</b>.
/// Ofrecer reemitir cuando no hace falta quema un folio y pide una firma para nada.
/// </param>
/// <param name="Ampara">
/// Firmado <b>y</b> todavía cubre. Las dos condiciones van separadas en el contrato para que la
/// pantalla pueda decir cuál de las dos falta: esperar una firma y reemitir un permiso son dos
/// acciones distintas de dos personas distintas.
/// </param>
public sealed record PermisoDiagnosticado(
    PermisoEnTramite Permiso,
    string? Vehiculo,
    string? Motorista,
    string? PorQueYaNoCubre,
    bool ExigeReemision,
    bool Ampara);
