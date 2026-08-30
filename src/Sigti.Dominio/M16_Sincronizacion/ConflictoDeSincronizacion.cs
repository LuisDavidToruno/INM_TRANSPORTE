using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M16_Sincronizacion;

/// <summary>
/// Un conflicto entre dos versiones del mismo hecho — `RN-45`, `HU-068`, mapa §7.1.
///
/// ── El caso que define el diseño ────────────────────────────────────────────
/// <i>«El motorista anotó odómetro de retorno 93,610 el 16 de mayo con foto del tablero; la
/// delegación digitó del papel 93,061 el 28 de mayo con foto del original. <b>Los dos son de
/// buena fe. Uno de los dos está mal, y la diferencia son 549 kilómetros</b> que van a entrar en
/// una conciliación de combustible.»</i>
///
/// Ninguna resolución automática es aceptable: los datos en conflicto son odómetros, galones y
/// montos, y una sobrescritura silenciosa <b>destruye el término de una conciliación de
/// auditoría sin que nadie se entere hasta que el Tribunal Superior de Cuentas pregunta</b>.
/// </summary>
public sealed record ConflictoDeSincronizacion(
    Ulid Id,
    Ulid Expediente,
    string Transicion,

    /// <summary>Qué campo diverge. Uno por conflicto: <b>no se agrupan</b>, ver `ReglasDelConflicto`.</summary>
    string Campo,

    VersionEnConflicto DelServidor,
    VersionEnConflicto DeCampo,

    EstadoDelConflicto Estado,
    ResolucionDelConflicto? Resolucion)
{
    /// <summary>
    /// Cuánto pesa. Decide el orden de la cola y <b>si el lote lo puede tocar</b>.
    /// </summary>
    public ImpactoDelConflicto Impacto => ReglasDelConflicto.ImpactoDe(Campo);

    /// <summary>Días esperando, para el orden y para el escalamiento por plazo.</summary>
    public int DiasEsperando(DateTimeOffset ahora) =>
        Math.Max(0, (int)(ahora - DeCampo.RegistradoEl).TotalDays);
}

/// <param name="CapturadaPor">Quién la registró. Es uno de los tres datos que permiten decidir.</param>
/// <param name="OcurrioEl">
/// Cuándo pasó el hecho. <b>Distinto de cuándo se registró</b>, y esa distinción es <i>«exactamente
/// lo que permite decidir»</i>: una versión anotada en el momento pesa distinto que una digitada
/// del papel doce días después.
/// </param>
/// <param name="Foto">
/// El adjunto. <b>Las dos fotos se ven al mismo tiempo</b>, no detrás de un clic: la del tablero
/// contra la del original es, en la práctica, lo que resuelve el conflicto.
/// </param>
public sealed record VersionEnConflicto(
    string Valor,
    IdPersona CapturadaPor,
    DateTimeOffset OcurrioEl,
    DateTimeOffset RegistradoEl,
    string? Dispositivo,
    Ulid? Foto);

public enum EstadoDelConflicto
{
    /// <summary>Sin resolver. <b>Bloquea la liquidación</b> de su misión — `RN-45` punto 4.</summary>
    Pendiente,
    Resuelto,
}

public enum ImpactoDelConflicto
{
    /// <summary>
    /// Odómetro, monto o autorización. <b>Nunca entra en un lote</b>: son los que destruyen una
    /// conciliación.
    /// </summary>
    Alto,

    /// <summary>Todo lo demás: horas, ubicaciones, observaciones.</summary>
    Normal,
}

/// <param name="SeTomo">
/// Cuál versión describe lo que pasó. <b>La pregunta no es cuál «gana»</b>: es cuál describe el
/// hecho.
/// </param>
/// <param name="Criterio">
/// Cuando la resolución vino de un lote, el criterio declarado. Nulo cuando se resolvió una por
/// una. <i>«Hacerlo sin declarar el criterio es sobrescritura con más pasos.»</i>
/// </param>
public sealed record ResolucionDelConflicto(
    OrigenElegido SeTomo,
    string Motivo,
    IdPersona Resuelve,
    DateTimeOffset Momento,
    string? Criterio);

public enum OrigenElegido { Servidor, Campo }

/// <summary>
/// Las reglas de la cola — mapa §7.1 y `RN-45`.
/// </summary>
public static class ReglasDelConflicto
{
    /// <summary>
    /// Los campos que <b>nunca</b> entran en una resolución por lote.
    ///
    /// §7.1 punto 9, literal: <i>«el lote excluye siempre odómetro, monto y autorización»</i>.
    /// Son los tres que entran en una conciliación contable, y resolverlos en bloque con un
    /// criterio general es sobrescritura silenciosa con un paso de más.
    /// </summary>
    public static readonly IReadOnlySet<string> DeAltoImpacto =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "odometro", "odometroSalida", "odometroRetorno",
            "monto", "galones", "autorizacion",
        };

    public static ImpactoDelConflicto ImpactoDe(string campo) =>
        DeAltoImpacto.Contains(campo) ? ImpactoDelConflicto.Alto : ImpactoDelConflicto.Normal;

    /// <summary>
    /// El orden de la cola: <b>impacto primero, después antigüedad</b> — §7.1 punto 10.
    ///
    /// La antigüedad sola pondría un cambio de observación de hace un mes por encima de un
    /// odómetro de ayer que está frenando una liquidación.
    /// </summary>
    public static IReadOnlyList<ConflictoDeSincronizacion> Ordenar(
        IEnumerable<ConflictoDeSincronizacion> cola) =>
        [.. cola
            .OrderByDescending(c => c.Impacto == ImpactoDelConflicto.Alto)
            .ThenBy(c => c.DeCampo.RegistradoEl)];

    /// <summary>
    /// Exige lo que `RN-45` punto 5 pide para resolver: motivo escrito.
    ///
    /// <i>«Escriba por qué toma esa versión. La decisión queda en el expediente y el auditor la
    /// va a leer.»</i> Un motivo de tres letras no es un motivo, y la longitud mínima es lo
    /// único que se puede exigir sin juzgar el contenido.
    /// </summary>
    public static void ExigirMotivo(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 8)
            throw new BloqueoDuro("RN-45",
                "Escriba por qué toma esa versión. La decisión queda en el expediente y el " +
                "auditor la va a leer.");
    }

    /// <summary>No se resuelve dos veces: la segunda decisión pisaría a la primera sin dejar rastro.</summary>
    public static void ExigirPendiente(ConflictoDeSincronizacion conflicto)
    {
        if (conflicto.Estado != EstadoDelConflicto.Pendiente)
            throw new BloqueoDuro("RN-45",
                $"Este conflicto ya se resolvió el " +
                $"{conflicto.Resolucion?.Momento:yyyy-MM-dd HH:mm} por " +
                $"{conflicto.Resolucion?.Resuelve}. La versión descartada sigue consultable; " +
                "para cambiar el resultado hay que registrar un asiento nuevo, no volver a " +
                "decidir sobre el mismo.");
    }

    /// <summary>
    /// Qué entra en un lote y qué queda fuera — §7.1 punto 9.
    ///
    /// <b>El resultado dice siempre cuántos quedaron fuera</b>, aunque sean cero: un lote que
    /// resuelve «todo» sin mencionar las exclusiones hace creer que la cola quedó vacía, y los
    /// de alto impacto —los que frenan liquidaciones— siguen ahí sin que nadie los mire.
    /// </summary>
    public static ReparticionDelLote Repartir(
        IEnumerable<ConflictoDeSincronizacion> candidatos)
    {
        var pendientes = candidatos.Where(c => c.Estado == EstadoDelConflicto.Pendiente).ToList();

        return new ReparticionDelLote(
            [.. pendientes.Where(c => c.Impacto == ImpactoDelConflicto.Normal)],
            [.. Ordenar(pendientes.Where(c => c.Impacto == ImpactoDelConflicto.Alto))]);
    }

    /// <summary>
    /// El criterio del lote, que <b>se declara y se guarda</b>.
    ///
    /// <i>«Resolver de a uno miles de conflictos es inviable; hacerlo sin declarar el criterio es
    /// sobrescritura con más pasos.»</i> El criterio queda en cada conflicto resuelto, no en un
    /// registro aparte: dentro de dos años, quien mire uno solo tiene que poder ver que salió de
    /// un lote y con qué regla.
    /// </summary>
    public static void ExigirCriterioDelLote(string? criterio)
    {
        if (string.IsNullOrWhiteSpace(criterio) || criterio.Trim().Length < 8)
            throw new BloqueoDuro("RN-45",
                "Declare el criterio del lote — por ejemplo, «aceptar la versión de campo para " +
                "todos los registros de esta misión». Resolver en bloque sin declararlo es " +
                "sobrescritura con más pasos.");
    }

    /// <summary>
    /// La respuesta a quien busca el botón de editar — §7.1 punto 5.
    ///
    /// <b>Va a buscarlo.</b> `R-6` dice que ninguna pantalla edita un hecho pasado, y es lo que
    /// hace difícil esta pantalla: no se le puede dar al usuario la salida fácil.
    /// </summary>
    public const string PorQueNoSeEdita =
        "No se edita un registro. Elija entre las versiones que existen, o registre un asiento " +
        "nuevo con la fecha del hecho que corresponda.";

    /// <summary>
    /// Por qué no se combinan dos versiones que divergen en campos distintos — §7.1 punto 6.
    /// </summary>
    public const string PorQueNoSeCombina =
        "Decida campo por campo. Combinar sólo produciría un registro que nadie capturó.";
}

/// <param name="FueraDelLote">
/// Los de alto impacto. <b>Se enumeran siempre</b>, aunque estén vacíos: <i>«3 conflictos de alto
/// impacto quedan fuera del lote y se resuelven uno por uno»</i>.
/// </param>
public sealed record ReparticionDelLote(
    IReadOnlyList<ConflictoDeSincronizacion> EnElLote,
    IReadOnlyList<ConflictoDeSincronizacion> FueraDelLote);
