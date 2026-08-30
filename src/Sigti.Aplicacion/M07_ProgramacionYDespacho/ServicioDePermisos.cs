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
        f.EmitidoPor is null ? null : new IdPersona(f.EmitidoPor));

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
