using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M11_Mantenimiento;

/// <summary>
/// Una reserva afectada por la indisponibilidad — `RN-60` punto 1.
///
/// ── Es una FOTO, y por eso se conserva ──────────────────────────────────────
/// `RN-60` punto 2: <i>«la lista mostrada se <b>conserva exactamente como se presentó</b>, con su
/// marca de tiempo. <b>No se reconstruye después</b>»</i>.
///
/// Reconstruirla al abrir el expediente meses después mostraría las misiones como están hoy —con
/// otras ventanas, otros motoristas, algunas anuladas— y quien acusó habría acusado sobre una
/// lista distinta de la que consta. El acuse dejaría de significar nada.
/// </summary>
public sealed record ReservaAfectada(
    Ulid Mision,
    string Referencia,
    string Dependencia,
    DateOnly Salida,
    DateOnly Retorno,
    string Motorista,
    string ObjetoDelTraslado,
    EstadoDeMision EstadoAlAcusar);

/// <summary>
/// Cómo se resolvió una reserva en conflicto — `RN-60`.
///
/// <b>Los cuatro que la regla enumera.</b> <i>«Una reserva en conflicto no expira en silencio ni
/// se resuelve por el paso del tiempo»</i>: sin un desenlace de esta lista, sigue en conflicto.
/// </summary>
public enum DesenlaceDeLaReserva
{
    /// <summary>Se le asignó otro vehículo (`T-11`). La asignación original se conserva.</summary>
    SustituirVehiculo,

    /// <summary>Se movió a otra ventana (`T-10`).</summary>
    Reprogramar,

    /// <summary>Se anuló con motivo tipificado (`T-15`, `T-16`).</summary>
    Anular,

    /// <summary>
    /// El vehículo volvió antes de lo estimado y la reserva sigue en pie. Se registra igual:
    /// el conflicto existió y su desaparición es un hecho con autor, no un silencio.
    /// </summary>
    LevantarLaIndisponibilidad,
}

/// <summary>
/// El desenlace registrado de una reserva — `RN-60` punto 4.
/// </summary>
public sealed record ResolucionDeLaReserva(
    Ulid Mision,
    DesenlaceDeLaReserva Desenlace,
    string Ejecuta,
    DateTimeOffset Momento,
    string Motivo);

/// <summary>
/// La indisponibilidad sobrevenida de un vehículo — `RN-60`.
///
/// ── Lo que la regla no deja pasar ───────────────────────────────────────────
/// <i>«Toda transición del vehículo a un estado que no habilita asignación debe exigir causa
/// tipificada, ventana estimada con fecha de fin, y <b>acuse expreso de quien la ejecuta sobre la
/// lista de reservas afectadas</b>»</i>.
///
/// El acuse es lo que convierte el hecho en una decisión: quien manda el vehículo al taller vio
/// qué misiones quedaban en el aire y siguió adelante. Sin él, el conflicto aparece después y
/// nadie lo decidió.
/// </summary>
/// <param name="Reservas">
/// <b>Congeladas al acusar.</b> Ver <see cref="ReservaAfectada"/>.
/// </param>
/// <param name="FinEstimado">
/// Hasta cuándo se estima. `RN-60` punto 6 la contrasta contra la fecha real al dar de alta:
/// <i>«la desviación sistemática entre estimado y real es indicador de la gestión del taller»</i>.
/// </param>
/// <param name="FinReal">
/// Nula mientras el vehículo no vuelva. Al darlo de alta se registra <b>con la orden de trabajo
/// cerrada y el odómetro de salida</b>.
/// </param>
public sealed record IndisponibilidadDelVehiculo(
    Ulid Id,
    Ulid VehiculoId,
    EstadoOperativo Estado,
    string Causa,
    DateOnly Desde,
    DateOnly FinEstimado,
    string Ejecuta,
    DateTimeOffset MomentoDelAcuse,
    IReadOnlyList<ReservaAfectada> Reservas,
    IReadOnlyList<ResolucionDeLaReserva> Resoluciones,
    DateOnly? FinReal = null,
    string? OrdenDeTrabajo = null,
    int? OdometroDeSalida = null,

    /// <summary>
    /// ⚠️ Por qué el vehículo <b>no</b> cambió de estado operativo, si no lo hizo. Nulo cuando
    /// sí se movió.
    /// </summary>
    string? EstadoNoAplicado = null)
{
    public bool EstaVigente => FinReal is null;

    /// <summary>
    /// Las reservas que siguen en conflicto. <b>No expiran en silencio</b>: sin un desenlace
    /// registrado, siguen acá aunque su ventana ya haya pasado.
    /// </summary>
    public IReadOnlyList<ReservaAfectada> SinDesenlace =>
        [.. Reservas.Where(r => !Resoluciones.Any(x => x.Mision == r.Mision))];

    /// <summary>
    /// Cuánto se desvió lo real de lo estimado — `RN-60` punto 6.
    ///
    /// <b>Positivo es que tardó más.</b> Nulo mientras el vehículo no vuelva: sin fecha real no
    /// hay desviación que medir, y suponerla haría que el indicador de gestión del taller
    /// midiera estimaciones contra sí mismas.
    /// </summary>
    public int? DesviacionEnDias =>
        FinReal is null ? null : FinReal.Value.DayNumber - FinEstimado.DayNumber;

    /// <summary>
    /// Si a esta altura ya venció la ventana estimada y el vehículo no volvió. No bloquea nada
    /// por sí solo: es lo que el reporte de indisponibilidad de flota mira primero.
    /// </summary>
    public bool ExcedeLoEstimado(DateOnly hoy) => EstaVigente && hoy > FinEstimado;
}

/// <summary>
/// Los controles de la indisponibilidad — `RN-60`.
/// </summary>
public static class ReglasDeLaIndisponibilidad
{
    /// <summary>
    /// `RN-60` — causa tipificada, ventana con fecha de fin y <b>acuse sobre la lista</b>.
    ///
    /// ── El acuse no es un checkbox ──────────────────────────────────────────
    /// Lo que se acusa es <b>la lista concreta</b>, y por eso se recibe: quien ejecuta tiene que
    /// haber visto qué misiones quedaban en el aire. Un acuse sin lista sería una casilla marcada
    /// sobre nada.
    /// </summary>
    /// <param name="reservasMostradas">
    /// Las que se le presentaron a quien ejecuta. <b>Puede ir vacía</b> —hay vehículos sin
    /// reservas— y eso es distinto de no haber mirado: la lista vacía también se conserva.
    /// </param>
    public static void ExigirCausaVentanaYAcuse(
        EstadoOperativo estado,
        string causa,
        DateOnly desde,
        DateOnly finEstimado,
        string ejecuta,
        IReadOnlyList<ReservaAfectada> reservasMostradas)
    {
        if (estado is not (EstadoOperativo.EnTaller or EstadoOperativo.NoDisponible))
            throw new BloqueoDuro("RN-60",
                $"«{estado}» no es un estado de indisponibilidad. Esta regla cubre las " +
                "transiciones a estados que no habilitan asignación; el resto de los cambios de " +
                "estado operativo siguen su propio camino.");

        if (string.IsNullOrWhiteSpace(causa))
            throw new BloqueoDuro("RN-60",
                "La indisponibilidad exige causa tipificada del catálogo " +
                "`causa_indisponibilidad`. Sin ella, el reporte de indisponibilidad de flota no " +
                "se puede agrupar, y un vehículo parado sin causa declarada es indistinguible " +
                "de uno que nadie usó.");

        if (string.IsNullOrWhiteSpace(ejecuta))
            throw new BloqueoDuro("RN-60",
                "La indisponibilidad exige quién la declara: el acuse sobre las reservas " +
                "afectadas es suyo, y sin nombre no hay quien haya acusado.");

        // **Con fecha de fin, siempre.** Una indisponibilidad sin fin estimado no se puede
        // contrastar contra la real, y el indicador de gestión del taller que `RN-60` punto 6
        // pide se queda sin la mitad de su cuenta.
        if (finEstimado < desde)
            throw new BloqueoDuro("RN-60",
                $"La ventana de indisponibilidad termina ({finEstimado:dd/MM/yyyy}) antes de " +
                $"empezar ({desde:dd/MM/yyyy}).");

        _ = reservasMostradas;
    }

    /// <summary>
    /// `RN-60` — <b>la marca de conflicto impide el despacho</b>.
    ///
    /// ── Y obliga a un desenlace, no a esperar ───────────────────────────────
    /// <i>«Una reserva en conflicto no expira en silencio ni se resuelve por el paso del
    /// tiempo»</i>. Si al inicio de la ventana sigue sin desenlace, el despacho se bloquea y el
    /// hecho entra al reporte de indisponibilidad de flota.
    ///
    /// ⚠️ <b>No tiene identificador `BD-xx`.</b> La máquina de estados —autoridad sobre bloqueos
    /// duros del despacho— no lo cataloga, y `RN-60` sí lo declara. Se implementa citando la
    /// regla, y queda como hallazgo para que la autoridad lo incorpore.
    /// </summary>
    public static void ExigirSinConflicto(ConflictoPorIndisponibilidad conflicto)
    {
        if (!conflicto.HayConflicto) return;

        throw new BloqueoDuro("RN-60",
            $"Esta misión está marcada en conflicto por la indisponibilidad del vehículo " +
            $"({conflicto.Causa}, hasta el {conflicto.FinEstimado:dd/MM/yyyy}) y no tiene " +
            "desenlace registrado. La marca impide el despacho: hay que sustituir el vehículo, " +
            "reprogramar, anular o levantar la indisponibilidad — y cualquiera de las cuatro es " +
            "un acto con autor y motivo, no el paso del tiempo.");
    }

    /// <summary>
    /// El desenlace de una reserva en conflicto — `RN-60` punto 4.
    ///
    /// <b>La asignación original se conserva junto a la sustituta, nunca sobrescrita.</b> Esa
    /// parte la garantiza el diario de la Orden de Misión; acá se cuida que el desenlace tenga
    /// motivo y no se registre dos veces.
    /// </summary>
    public static void ExigirDesenlaceRegistrable(
        IndisponibilidadDelVehiculo indisponibilidad, Ulid mision, string motivo)
    {
        if (!indisponibilidad.Reservas.Any(r => r.Mision == mision))
            throw new BloqueoDuro("RN-60",
                "Esa misión no figura entre las reservas afectadas que se acusaron. La lista se " +
                "conserva como se presentó: agregarle una después haría que el acuse cubriera " +
                "algo que quien ejecutó no vio.");

        if (indisponibilidad.Resoluciones.Any(r => r.Mision == mision))
            throw new BloqueoDuro("RN-60",
                "Esa reserva ya tiene desenlace registrado. Reescribirlo borraría el que " +
                "constaba; un cambio posterior es su propia transición sobre la misión.");

        if (string.IsNullOrWhiteSpace(motivo))
            throw new BloqueoDuro("RN-60",
                "El desenlace de una reserva en conflicto exige motivo. Sin él, el expediente " +
                "diría que alguien resolvió sin decir por qué.");
    }

    /// <summary>
    /// El alta del vehículo — `RN-60` punto 6.
    ///
    /// <b>Con la orden de trabajo cerrada y el odómetro de salida.</b> Sin ellos, el alta sería
    /// una fecha suelta: no consta qué se hizo ni con cuántos kilómetros volvió, y la
    /// conciliación del período siguiente arranca sin lectura de apertura.
    /// </summary>
    public static void ExigirAltaConOrdenYOdometro(
        IndisponibilidadDelVehiculo indisponibilidad,
        DateOnly finReal,
        string ordenDeTrabajo,
        int odometroDeSalida)
    {
        if (!indisponibilidad.EstaVigente)
            throw new BloqueoDuro("RN-60",
                $"Este vehículo ya se dio de alta el {indisponibilidad.FinReal:dd/MM/yyyy}.");

        if (string.IsNullOrWhiteSpace(ordenDeTrabajo))
            throw new BloqueoDuro("RN-60",
                "El alta exige la orden de trabajo cerrada. Sin ella, el vehículo vuelve a la " +
                "flota sin que conste qué se le hizo mientras estuvo parado.");

        if (odometroDeSalida < 0)
            throw new BloqueoDuro("RN-60",
                "El alta exige el odómetro de salida. Es la lectura de apertura del período " +
                "siguiente, y sin ella la conciliación galonaje–kilometraje arranca sin contra " +
                "qué medir.");

        if (finReal < indisponibilidad.Desde)
            throw new BloqueoDuro("RN-60",
                $"La fecha real de alta ({finReal:dd/MM/yyyy}) es anterior al inicio de la " +
                $"indisponibilidad ({indisponibilidad.Desde:dd/MM/yyyy}).");
    }
}

/// <summary>
/// Si una misión está en conflicto por indisponibilidad del vehículo — `RN-60`.
///
/// ── Por qué esto es un tipo y no un booleano ────────────────────────────────
/// Es el mismo argumento que la máquina de estados usa para la custodia al despachar: <i>«es la
/// diferencia entre "no hay custodio" y "nadie preguntó", y en un bloqueo duro las dos no pueden
/// verse igual»</i>. Un <c>bool</c> por omisión dejaría que un llamador nuevo despachara sin
/// haber consultado, y el bloqueo se apagaría solo.
/// </summary>
/// <param name="HayConflicto">
/// Verdadero cuando el vehículo está indisponible y esta misión figura entre las reservas
/// afectadas <b>sin desenlace registrado</b>.
/// </param>
public sealed record ConflictoPorIndisponibilidad(
    bool HayConflicto,
    string? Causa = null,
    DateOnly FinEstimado = default)
{
    /// <summary>
    /// El vehículo está disponible, o la reserva ya tuvo su desenlace. <b>Se construye
    /// explícitamente</b>: quien la use está afirmando que consultó.
    /// </summary>
    public static ConflictoPorIndisponibilidad Ninguno { get; } = new(false);
}
