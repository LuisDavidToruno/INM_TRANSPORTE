using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// El ciclo del fondo del período — `RN-26` punto 1 del comportamiento esperado:
/// <i>«solicitado, aprobado, entregado, agotado, cerrado. Cada transición con actor, fecha y
/// motivo»</i>.
///
/// ── Por qué es un objeto de PERÍODO y no de misión ──────────────────────────
/// Es la corrección del hallazgo `HN1-15`, y no es un matiz: `RN-01` segrega funciones
/// <b>sobre una misma Orden de Misión</b>, y el fondo no es de una misión. Leída al pie de la
/// letra, `RN-01` <b>no alcanza al fondo</b> — así que la incompatibilidad más sensible del
/// circuito de dinero quedaba enunciada sin regla que la sostuviera. Vive acá, como control
/// propio del expediente del fondo. Ver <see cref="ReglasDelFondo"/>.
/// </summary>
public enum EstadoDelFondo
{
    /// <summary>ACT-04 pidió, con monto y justificación operativa del período.</summary>
    Solicitado,

    /// <summary>
    /// ACT-08 aprobó, con monto, fecha, aprobador y partida. <b>Desde acá ya se puede
    /// asignar</b>: `RN-26` exige fondo aprobado vigente con saldo, no fondo entregado.
    /// </summary>
    Aprobado,

    /// <summary>El efectivo o las órdenes de pago están en manos de quien las administra.</summary>
    Entregado,

    /// <summary>
    /// Sin saldo. <b>No es terminal</b>: `RN-26` prevé la ampliación, que sigue el mismo
    /// circuito y devuelve el fondo a `Aprobado`.
    /// </summary>
    Agotado,

    /// <summary>
    /// Terminal. Exige que <b>todas</b> sus asignaciones estén liquidadas o formalmente
    /// anuladas, y que la partida presupuestaria esté completa.
    /// </summary>
    Cerrado,
}

/// <summary>Un asiento del diario del fondo. Mismo principio P-1 que todo lo demás.</summary>
/// <param name="Id">`F-01` solicitar · `F-02` aprobar · `F-03` entregar · `F-04` agotar · `F-05` ampliar · `F-06` cerrar.</param>
/// <param name="Monto">
/// Lo que este asiento aprueba. <b>Sólo lo llevan `F-02` y `F-05`</b> — aprobar y ampliar son
/// los dos únicos actos que crean saldo. El techo del fondo es la suma de estos asientos, no
/// una columna que alguien pueda editar.
/// </param>
public sealed record MovimientoDelFondo(
    string Id,
    EstadoDelFondo Destino,
    IdPersona Ejecuta,
    DateTimeOffset Momento,
    string? Motivo,
    decimal? Monto = null);

/// <summary>
/// Hasta dónde alcanza un fondo. `RN-26`: <i>«las asignaciones solo pueden imputarse a fondos
/// de su ámbito»</i>.
///
/// `[C]` <b>Si las delegaciones manejan fondo propio no está confirmado</b> — `RN-26` lo deja
/// abierto. El tipo admite los tres niveles para no tener que rehacer el modelo cuando se
/// conteste; que exista el nivel no significa que la institución lo use.
/// </summary>
public enum AmbitoDelFondo
{
    Institucion,
    Dependencia,
    Delegacion,
}
