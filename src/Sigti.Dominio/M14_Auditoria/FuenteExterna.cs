using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Qué clase de fuente externa es. Cada una se concilia contra otra cosa — `RN-95`.
/// </summary>
public enum TipoDeFuenteExterna
{
    /// <summary>Contra los consumos y abastecimientos registrados (`RN-28`, `RN-83`).</summary>
    EstadoDeCuentaDeCombustible,

    /// <summary>
    /// Contra los pasos por caseta (`RN-34`). <b>Es la que ve el paso de un domingo sin
    /// misión</b> — uno de los tres casos que originaron `CE-28`.
    /// </summary>
    EstadoDeCuentaDePeaje,

    /// <summary>Contra la bitácora y el conductor de la fecha (`RN-66`).</summary>
    InfraccionesDeTransito,

    /// <summary>Dictámenes, resoluciones y actas de autoridad, contra los expedientes de M-12.</summary>
    ActasDeAutoridad,
}

/// <summary>
/// Una fuente externa del catálogo — `RN-95` punto 1.
///
/// ── Por qué esto existe siendo que `RN-30` ya concilia ──────────────────────
/// `RN-30` concilia <b>hacia adentro</b>: galones contra kilómetros, ambos registrados por
/// nosotros. `RN-95` lo dice sin rodeos: <i>«una conciliación que solo compara nuestros datos
/// con nuestros datos verifica coherencia interna, no veracidad. <b>Un registro completo y
/// coherente puede ser completamente falso</b>, y solo la fuente externa lo revela»</i>.
///
/// De ahí salieron los tres casos de `CE-28`: el comprobante duplicado que apareció en el
/// estado de cuenta del proveedor, el paso por caseta de un domingo sin misión, y las multas
/// notificadas meses después.
/// </summary>
/// <param name="Disponible">
/// <b>Falso no es «pendiente»: es «no la tenemos»</b>. `RN-95`: una institución sin tag de peaje
/// no tiene estado de cuenta que conciliar, y esa fuente se declara <b>no disponible, que es
/// distinto de conciliada</b>. Confundirlas haría que la ausencia de diferencias se leyera como
/// conformidad.
/// </param>
/// <param name="PeriodicidadEnDias">
/// Cada cuánto debería conciliarse. <b>Nula mientras la institución no la fije</b> — y entonces
/// el retraso se puede medir pero no se puede llamar vencido, igual que el plazo de `RN-86`.
/// </param>
/// <param name="UltimaConciliacion">
/// <b>Nula significa que nunca se ha conciliado</b>, y eso no es cero días de retraso: es una
/// fuente que nadie ha mirado nunca, que es una observación de control interno más fuerte que
/// una atrasada.
/// </param>
public sealed record FuenteExterna(
    Ulid Id,
    TipoDeFuenteExterna Tipo,
    string Emisor,
    string Formato,
    string ResponsableDeLaCarga,
    bool Disponible,
    int? PeriodicidadEnDias = null,
    DateOnly? UltimaConciliacion = null,
    string? PorQueNoEstaDisponible = null)
{
    /// <summary>
    /// Cuántos días lleva sin conciliarse. <b>Nulo cuando nunca se concilió</b>: de una fuente
    /// que nadie miró no se puede decir que lleva N días, se puede decir que nunca.
    /// </summary>
    public int? DiasDesdeLaUltima(DateOnly hoy) =>
        UltimaConciliacion is { } ultima ? hoy.DayNumber - ultima.DayNumber : null;

    /// <summary>
    /// `RN-95` punto 5 — <i>«una fuente sin conciliar durante meses es en sí misma una
    /// observación de control interno»</i>.
    ///
    /// <b>Nunca conciliada cuenta como atrasada</b> en cuanto haya periodicidad declarada: es el
    /// peor caso, no la ausencia de caso.
    /// </summary>
    public bool Atrasada(DateOnly hoy) =>
        Disponible &&
        PeriodicidadEnDias is { } cada &&
        (DiasDesdeLaUltima(hoy) is not { } dias || dias > cada || dias < 0);

    /// <summary>
    /// El dato que `RN-95` manda mostrar, con sus palabras: <i>«Estado de cuenta de combustible —
    /// última conciliación hace 97 días»</i>.
    /// </summary>
    public string Retraso(DateOnly hoy) =>
        !Disponible
            ? $"No disponible: {PorQueNoEstaDisponible ?? "sin razón declarada"}. " +
              "No disponible NO es conciliada."
            : DiasDesdeLaUltima(hoy) is not { } dias
                ? "NUNCA se ha conciliado."
                : dias < 0
                    ? $"La última conciliación está registrada el {UltimaConciliacion:dd/MM/yyyy}, " +
                      "que es posterior a hoy. Eso no describe ninguna conciliación: o la fecha " +
                      "se capturó mal, o el reloj del servidor no es el que se cree."
                : PeriodicidadEnDias is { } cada
                    ? $"Última conciliación hace {dias} día(s), sobre una periodicidad de {cada}." +
                      (dias > cada ? " ATRASADA." : "")
                    : $"Última conciliación hace {dias} día(s). La periodicidad no está " +
                      "declarada (`[C]`), así que no se puede decir si va atrasada.";
}

/// <summary>Los controles del catálogo de fuentes — `RN-95` punto 1.</summary>
public static class ReglasDeFuenteExterna
{
    /// <summary>
    /// Lo que toda fuente exige para poder conciliarse.
    ///
    /// El <b>responsable de la carga</b> no es burocracia: una fuente sin responsable es una
    /// fuente que nadie carga, y a los tres meses la conciliación existe en el papel y no en la
    /// práctica.
    /// </summary>
    public static void ExigirDatosDelCatalogo(
        string emisor, string responsableDeLaCarga, bool disponible, string? porQueNo)
    {
        if (string.IsNullOrWhiteSpace(emisor))
            throw new BloqueoDuro("RN-95",
                "La fuente exige emisor. Sin él no se sabe contra quién se está conciliando ni " +
                "a quién reclamarle una diferencia.");

        if (string.IsNullOrWhiteSpace(responsableDeLaCarga))
            throw new BloqueoDuro("RN-95",
                "La fuente exige responsable de la carga. Una fuente sin responsable es una " +
                "fuente que nadie carga, y a los tres meses la conciliación existe en el papel " +
                "y no en la práctica.");

        if (!disponible && string.IsNullOrWhiteSpace(porQueNo))
            throw new BloqueoDuro("RN-95",
                "Declarar una fuente NO disponible exige decir por qué. «No disponible» es " +
                "distinto de «conciliada», y sin la razón las dos se leen igual en el reporte.");
    }
}
