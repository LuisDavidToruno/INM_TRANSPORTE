namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// El estado operativo del vehículo — §10.2 de la máquina de estados.
///
/// ── Por qué es un estado propio y no un booleano ─────────────────────────────
/// Porque las causas de no poder asignar un vehículo <b>no son intercambiables</b>: un
/// vehículo en taller vuelve, uno prestado vuelve con acta, uno dado de baja no vuelve, y
/// uno retirado de flota nunca fue nuestro. Colapsarlas en «disponible sí/no» borra la
/// única información con la que se planifica.
/// </summary>
public enum EstadoOperativo
{
    /// <summary>
    /// Documentación vigente, sin orden de trabajo que lo inmovilice, con custodio asignado.
    /// <b>Es el único estado desde el que se puede programar</b> — `BD-07`.
    /// </summary>
    Disponible,

    /// <summary>
    /// Comprometido a una misión que aún no ha salido. Cubre `PROGRAMADA` y `DESPACHADA`.
    ///
    /// <b>Lo fija el sistema, nunca una persona</b>: es consecuencia de `T-08`, y permitir
    /// fijarlo a mano abre la puerta a un vehículo «asignado» sin misión que lo respalde.
    /// </summary>
    Asignado,

    /// <summary>Fuera, con misión `EN_RUTA`. También lo fija el sistema, por `T-14`.</summary>
    EnMision,

    /// <summary>Con orden de trabajo abierta, preventivo o correctivo. No asignable. `ACT-11`.</summary>
    EnTaller,

    /// <summary>
    /// No asignable por causa <b>tipificada</b>: documentación vencida, incidente bajo
    /// investigación, resguardo ordenado —el caso de Semana Santa—, sin custodio, en trámite
    /// de descargo, alta reciente sin habilitar.
    /// </summary>
    NoDisponible,

    /// <summary>
    /// Cedido a otra dependencia o institución. <b>Sigue siendo bien nuestro</b> y devenga
    /// responsabilidad patrimonial, pero no se asigna a misiones propias. Ocupa ventana.
    ///
    /// Existe porque su ausencia obligaba a declarar prestado un vehículo como averiado o de
    /// baja — hallazgo detectado al escribir `CE-14`.
    /// </summary>
    Prestado,

    /// <summary>
    /// Descargado del registro de bienes. <b>Terminal.</b> Solo para bienes <b>propios</b>.
    /// </summary>
    DadoDeBaja,

    /// <summary>
    /// Fin de la tenencia de un bien <b>ajeno</b>: devolución de comodato, fin de alquiler.
    /// <b>Terminal.</b>
    ///
    /// <b>No es descargo</b>, y confundirlos produce un asiento falso: declarar <i>dado de baja
    /// del registro de bienes del Estado</i> un vehículo que nunca fue del Estado es
    /// detectable cruzando el inventario institucional contra el padrón de flota.
    /// </summary>
    RetiradoDeFlota,
}

/// <summary>
/// Un cambio de estado operativo, con quién y por qué.
///
/// ── Por qué el estado es un diario y no una columna ──────────────────────────
/// Porque la pregunta que se hace la auditoría es <i>«¿por qué este vehículo no estuvo
/// disponible en abril?»</i>, y una columna `estado_actual` no la contesta. Es la misma
/// razón por la que la custodia es una tabla con rangos y no un campo.
///
/// Y porque §10.2 exige <b>causa tipificada</b> para `NO_DISPONIBLE` y <b>acta</b> para los
/// terminales y para el préstamo: eso no cabe en un enum guardado en el vehículo.
/// </summary>
/// <param name="Automatico">
/// Si lo fijó el sistema por una transición de la Orden de Misión. <b>`ASIGNADO` y
/// `EN_MISION` sólo llegan así</b>: §10.2 lo dice sin margen —<i>«los fija el sistema, no una
/// persona»</i>— y esta marca es lo que permite verificarlo después.
/// </param>
public sealed record CambioDeEstadoOperativo(
    EstadoOperativo Estado,
    DateTimeOffset Momento,
    string Ejecuta,
    string? Motivo,
    bool Automatico);
