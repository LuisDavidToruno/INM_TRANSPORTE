namespace Sigti.Dominio.M03_Flota;

/// <summary>Por qué no se pudo declarar el estado.</summary>
public sealed class CambioDeEstadoInvalido(string mensaje) : Exception(mensaje);

/// <summary>
/// Qué puede declarar una persona sobre el estado de un vehículo — §10.2.
///
/// ── Por qué esto es una regla y no una validación de formulario ──────────────
/// Porque las dos cosas que impone tienen consecuencia patrimonial. Un vehículo puesto «en
/// misión» a mano es un vehículo que figura fuera sin que ninguna misión lo respalde; y un
/// descargo con misiones abiertas deja expedientes vivos colgando de un bien que ya no
/// existe en el registro.
/// </summary>
public static class ReglasDeEstadoOperativo
{
    /// <summary>
    /// Los que <b>fija el sistema</b> y nadie más.
    ///
    /// §10.2 sin margen: <i>«`ASIGNADO` y `EN_MISION` los fija el sistema, no una persona. Son
    /// consecuencia de transiciones de la Orden de Misión, y permitir fijarlos a mano abre la
    /// puerta a un vehículo "en misión" sin misión»</i>.
    /// </summary>
    private static readonly EstadoOperativo[] SoloAutomaticos =
    [
        EstadoOperativo.Asignado,
        EstadoOperativo.EnMision,
    ];

    /// <summary>
    /// Los terminales. De acá no se vuelve, y por eso exigen que no quede nada abierto.
    /// </summary>
    public static bool EsTerminal(EstadoOperativo estado) =>
        estado is EstadoOperativo.DadoDeBaja or EstadoOperativo.RetiradoDeFlota;

    /// <summary>
    /// Valida lo que una persona quiere declarar.
    /// </summary>
    /// <param name="misionesAbiertas">
    /// Cuántas misiones del vehículo <b>no</b> están en estado terminal. §10.2: <i>«un vehículo
    /// con misiones abiertas no puede ser dado de baja. Todas deben estar en estado
    /// terminal»</i>.
    ///
    /// <b>Se recibe contado y no se cuenta acá</b>: la regla es pura y quien la llama trae los
    /// datos vigentes (`ADR-009`).
    /// </param>
    /// <param name="actual">
    /// En qué estado está. Nulo si nunca se declaró — se puede declarar el primero.
    /// </param>
    public static void ExigirDeclarable(
        EstadoOperativo destino,
        EstadoOperativo? actual,
        int misionesAbiertas)
    {
        if (SoloAutomaticos.Contains(destino))
            throw new CambioDeEstadoInvalido(
                $"«{destino}» lo fija el sistema como consecuencia de una transición de la " +
                "Orden de Misión, y no se declara a mano: permitirlo abriría la puerta a un " +
                "vehículo «en misión» sin misión que lo respalde.");

        if (actual is { } desde && EsTerminal(desde))
            throw new CambioDeEstadoInvalido(
                $"El vehículo está en «{desde}», que es un estado terminal. " +
                (desde == EstadoOperativo.DadoDeBaja
                    // Las dos salidas son distintas y conviene decir cuál es cuál: un descargo
                    // se revierte por el circuito de bienes del Estado; una devolución de
                    // comodato ni siquiera es nuestra para revertirla.
                    ? "Revertir un descargo es un trámite del registro de bienes del Estado, " +
                      "no un cambio de estado en este sistema."
                    : "El bien ya no está bajo tenencia de la institución."));

        if (EsTerminal(destino) && misionesAbiertas > 0)
            throw new CambioDeEstadoInvalido(
                $"El vehículo tiene {misionesAbiertas} misión(es) sin cerrar. " +
                "Todas deben estar en estado terminal antes de darlo de baja o retirarlo: " +
                // El daño concreto, no la regla abstracta.
                "un expediente vivo colgando de un bien que ya no figura en el registro es " +
                "un hallazgo que nadie puede explicar después.");
    }
}
