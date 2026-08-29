using Microsoft.EntityFrameworkCore;
using Sigti.Aplicacion.M02_Parametros;
using Sigti.Datos;
using Sigti.Datos.M07_ProgramacionYDespacho;
using Sigti.Datos.M09_Combustible;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;

namespace Sigti.Aplicacion.M09_Combustible;

/// <summary>
/// El circuito de reintegro — `RN-86`.
///
/// ── Las dos cosas que arma, y son distintas ─────────────────────────────────
/// <b>El saldo afuera</b> se calcula: es dinero que no volvió y que todavía nadie determinó.
/// <b>La obligación</b> se nomina: es una deuda que alguien determinó, con competencia, y
/// tiene ciclo propio que sobrevive al cierre de la misión.
///
/// `RN-86` bloquea por las dos, y `HU-078` les da un escenario a cada una. Confundirlas sería
/// perder el intervalo entre que el plazo vence y que alguien se sienta a nominar — que es,
/// según `CE-26`, justo donde nace el faltante.
/// </summary>
public sealed class ServicioDeReintegro(
    SigtiDbContext contexto,
    IParametrosDeLaInstitucion parametros)
{
    private readonly ObligacionesDeReintegro _obligaciones = new(contexto);
    private readonly CombustibleDeLaInstitucion _combustible = new(contexto);
    private readonly ExpedientesDeMision _expedientes = new(contexto);

    // ── El saldo que está afuera ────────────────────────────────────────────

    /// <summary>
    /// Lo que esta persona tiene en la mano — `CE-26` §1.
    ///
    /// Cada vale vivo se resuelve contra <b>el retorno de su propia misión</b>, porque el
    /// plazo corre desde ahí y no desde la emisión. Un motorista con dos misiones puede tener
    /// una vencida y otra en curso, y meterlas en el mismo saco haría que la segunda lo
    /// bloqueara sin haber vencido.
    /// </summary>
    public async Task<IReadOnlyList<SaldoAfuera>> SaldosAfueraDeAsync(
        Ulid responsable, CancellationToken cancelacion = default)
    {
        var vales = await _combustible.DelReceptorAsync(responsable, cancelacion);
        var saldos = new List<SaldoAfuera>();

        foreach (var vale in vales)
        {
            var expediente = await _expedientes.BuscarAsync(vale.Mision, cancelacion);

            // El retorno es la **fecha del hecho** de `T-18`, no la de captura ni la de
            // sincronización (`RN-46`). `T-18` se registra desde el campo, de noche y sin
            // señal, y llega días después: usar la de captura correría el plazo desde que
            // hubo cobertura, que no es lo que la regla dice.
            var retorno = expediente?.Diario
                .LastOrDefault(t => t.Id == "T-18")?.Momento;

            // El calendario vigente **a la fecha del retorno**, no el de hoy (P-4): un feriado
            // decretado después no puede alargar hacia atrás un plazo que ya venció.
            var calendario = parametros.CalendarioVigenteAl(
                retorno is { } r ? DateOnly.FromDateTime(r.Date) : Hoy());

            var saldo = ReglasDelSaldoAfuera.De(
                vale.Id, vale.Folio, responsable, vale.Mision,
                // ⚠️ El ULID, porque **la orden de misión todavía no tiene folio**. `HU-078`
                // espera leer "de la misión OM-2026-0491", y `RN-44` reserva rangos por
                // delegación para eso. Mientras el folio no exista, se muestra lo que sí
                // identifica sin ambigüedad en vez de inventar un correlativo.
                referenciaDeLaMision: vale.Mision.ToString(),
                asignado: vale.Monto,
                consumido: vale.Consumido,
                devuelto: vale.Devuelto,
                valeResuelto: vale.EstaResuelta,
                retornoDeLaMision: retorno is { } m ? DateOnly.FromDateTime(m.Date) : null,
                plazoEnDiasHabiles: parametros.PlazoDeDevolucionDeSaldoEnDiasHabiles,
                calendario);

            if (saldo is not null) saldos.Add(saldo);
        }

        return saldos;
    }

    /// <summary>
    /// El arqueo por persona — `RN-86` punto 6: <i>«pendiente de devolución, con el detalle por
    /// persona de lo que está afuera»</i>.
    ///
    /// <b>Es la primera pregunta de un arqueo y hoy no la contesta nadie</b> (`CE-26` §1). Trae
    /// también a quien no debe nada pero tiene obligación a su favor: un sistema que sólo mide
    /// lo que el servidor le debe a la institución es un sistema de cobro.
    /// </summary>
    public async Task<IReadOnlyList<LoQueDebeUnaPersona>> ArqueoPorPersonaAsync(
        DateOnly hoy, CancellationToken cancelacion = default)
    {
        var obligaciones = await _obligaciones.TodasAsync(cancelacion);

        // Los vales vivos de cualquiera, no sólo los de quienes ya tienen obligación: el saldo
        // vencido sin obligación formalizada es la mitad que `HU-078` no quiere perder.
        var receptores = await contexto.AsignacionesDeCombustible
            .Select(a => a.Receptor)
            .Distinct()
            .ToListAsync(cancelacion);

        var personas = receptores
            .Concat(obligaciones.Select(o => o.Responsable))
            .Distinct()
            .ToList();

        var arqueo = new List<LoQueDebeUnaPersona>();

        foreach (var persona in personas)
        {
            var saldos = await SaldosAfueraDeAsync(persona, cancelacion);
            var suyas = obligaciones.Where(o => o.Responsable == persona).ToList();

            var abiertas = suyas.Where(o => o.EstaAbierta).ToList();

            if (saldos.Count == 0 && abiertas.Count == 0) continue;

            arqueo.Add(new LoQueDebeUnaPersona(
                persona,
                saldos,
                abiertas,
                ACargo: abiertas
                    .Where(o => o.Direccion is DireccionDelReintegro.AFavorDeLaInstitucion)
                    .Sum(o => o.Saldo),
                AFavor: abiertas
                    .Where(o => o.Direccion is DireccionDelReintegro.AFavorDelServidor)
                    .Sum(o => o.Saldo),
                SinComprobar: saldos.Sum(s => s.Monto),
                Vencido: saldos.Any(s => s.VencidoAl(hoy)) || abiertas.Any(
                    o => o.Direccion is DireccionDelReintegro.AFavorDeLaInstitucion)));
        }

        // El que más debe primero. Un arqueo ordenado por nombre esconde el caso que importa
        // en medio de una lista de gente que no debe nada relevante.
        return [.. arqueo.OrderByDescending(p => p.ACargo + p.SinComprobar)];
    }

    // ── La obligación ───────────────────────────────────────────────────────

    /// <summary>
    /// `R-01` — nominar. <b>Acto propio</b>: `RN-86` punto 5 es explícito en que la obligación
    /// <i>«no nace en la liquidación»</i>, y `RN-74` reserva la determinación a quien
    /// corresponde. Quien liquida constata el hueco; nominar a un responsable es de otro.
    /// </summary>
    public async Task<Ulid> NominarAsync(
        Ulid id,
        DireccionDelReintegro direccion,
        CausaDelReintegro causa,
        Ulid responsable,
        decimal monto,
        Ulid? mision,
        Ulid? asignacion,
        DateOnly fechaDelHecho,
        Autoria nomina,
        string motivo,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var obligacion = ObligacionDeReintegro.Nominar(
            id, direccion, causa, responsable, monto, mision, asignacion,
            fechaDelHecho, nomina, motivo, momento);

        await _obligaciones.GuardarAsync(obligacion, cancelacion);
        return obligacion.Id;
    }

    /// <summary>Aplica un movimiento sobre una obligación existente.</summary>
    public async Task<EstadoDeObligacion> MoverAsync(
        Ulid id,
        Action<ObligacionDeReintegro> movimiento,
        CancellationToken cancelacion = default)
    {
        var obligacion = await _obligaciones.BuscarAsync(id, cancelacion)
            ?? throw new ObligacionNoEncontrada(id);

        movimiento(obligacion);

        await _obligaciones.GuardarAsync(obligacion, cancelacion);
        return obligacion.Estado;
    }

    public Task<ObligacionDeReintegro?> BuscarAsync(
        Ulid id, CancellationToken cancelacion = default) =>
        _obligaciones.BuscarAsync(id, cancelacion);

    /// <summary>Lo que debe —o le deben a— una persona. La consulta del bloqueo.</summary>
    public Task<IReadOnlyList<ObligacionDeReintegro>> DeLaPersonaAsync(
        Ulid responsable, CancellationToken cancelacion = default) =>
        _obligaciones.DeLaPersonaAsync(responsable, cancelacion);

    public Task<IReadOnlyList<ObligacionDeReintegro>> TodasAsync(
        CancellationToken cancelacion = default) =>
        _obligaciones.TodasAsync(cancelacion);

    // ── El levantamiento del bloqueo ────────────────────────────────────────

    /// <summary>
    /// El acto de ACT-08 que deja pasar una emisión bloqueada — `RN-86` casos límite.
    ///
    /// ⚠️ <b>Que quien autoriza ocupe el puesto de ACT-08 no se verifica todavía</b>: el mapa
    /// rol↔puesto es de la institución y está `[C]` (insumo #1). Lo que sí queda es el asiento
    /// con persona, puesto declarado, fecha y motivo — que es lo que después permite revisar
    /// quién los firmó. <b>No se finge que el puesto se validó.</b>
    /// </summary>
    public async Task<Ulid> LevantarBloqueoAsync(
        Ulid mision,
        Ulid responsable,
        Autoria autoriza,
        string motivo,
        DateTimeOffset momento,
        CancellationToken cancelacion = default)
    {
        var acto = ReglasDelReintegro.Levantar(mision, autoriza, motivo, momento);

        var ya = await contexto.LevantamientosDeBloqueo
            .AnyAsync(l => l.MisionId == mision && l.Responsable == responsable, cancelacion);

        if (ya)
            throw new BloqueoDuro("RN-86",
                "Ya hay un levantamiento registrado para esta persona en esta misión. Levantar " +
                "dos veces no agrega nada y multiplica la excepción en el indicador.");

        var fila = new FilaDeLevantamiento
        {
            Id = Ulid.NewUlid(),
            MisionId = mision,
            Responsable = responsable,
            Persona = autoriza.Persona.Valor,
            Puesto = autoriza.Puesto.Valor,
            FechaDelHecho = autoriza.FechaDelHecho,
            MomentoUtc = acto.Momento.UtcDateTime,
            DesfaseMinutos = (int)acto.Momento.Offset.TotalMinutes,
            Motivo = acto.Motivo,
        };

        contexto.LevantamientosDeBloqueo.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);

        return fila.Id;
    }

    /// <summary>
    /// El levantamiento vigente para esta misión y esta persona, si lo hay.
    ///
    /// <b>Punto por punto y no por lista</b>: bajo `UseCompatibilityLevel(120)` un
    /// <c>Contains</c> sobre un ULID con conversión de valor devuelve vacío en silencio, y acá
    /// vacío en silencio significaría bloquear a alguien que tenía autorización.
    /// </summary>
    public async Task<LevantamientoDeBloqueo?> LevantamientoVigenteAsync(
        Ulid mision, Ulid responsable, CancellationToken cancelacion = default)
    {
        var fila = await contexto.LevantamientosDeBloqueo
            .SingleOrDefaultAsync(
                l => l.MisionId == mision && l.Responsable == responsable, cancelacion);

        return fila is null
            ? null
            : new LevantamientoDeBloqueo(
                fila.MisionId,
                Autoria.De(
                    new Dominio.Organizacion.IdPersona(fila.Persona),
                    new IdPuesto(fila.Puesto),
                    fila.FechaDelHecho),
                fila.Motivo,
                new DateTimeOffset(fila.MomentoUtc, TimeSpan.Zero)
                    .ToOffset(TimeSpan.FromMinutes(fila.DesfaseMinutos)));
    }

    /// <summary>
    /// El indicador de `RN-86`: <i>«levantamientos por persona y por período»</i>. Se lee de un
    /// vistazo a propósito.
    /// </summary>
    public async Task<IReadOnlyList<FilaDeLevantamiento>> LevantamientosAsync(
        CancellationToken cancelacion = default) =>
        await contexto.LevantamientosDeBloqueo
            .OrderByDescending(l => l.MomentoUtc)
            .ToListAsync(cancelacion);

    private static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.UtcNow);
}

/// <summary>
/// Lo que una persona tiene afuera — la fila del arqueo de `RN-86` punto 6.
/// </summary>
/// <param name="SinComprobar">
/// Dinero de vales vivos que no volvió ni se comprobó. <b>Vencido o no</b>: el arqueo muestra
/// todo lo que está afuera, y el bloqueo filtra por su cuenta.
/// </param>
/// <param name="AFavor">
/// Lo que la institución le debe a esta persona. <b>Va en la misma fila y no en otro
/// reporte</b>: `CE-26` — <i>«un sistema que solo mide lo que el servidor le debe a la
/// institución no es un sistema de control: es un sistema de cobro»</i>.
/// </param>
public sealed record LoQueDebeUnaPersona(
    Ulid Responsable,
    IReadOnlyList<SaldoAfuera> Saldos,
    IReadOnlyList<ObligacionDeReintegro> Obligaciones,
    decimal ACargo,
    decimal AFavor,
    decimal SinComprobar,
    bool Vencido);

public sealed class ObligacionNoEncontrada(Ulid id)
    : Exception($"No existe la obligación de reintegro {id}.");
