using Sigti.Dominio.M01_Organizacion;

namespace Sigti.Datos;

/// <summary>
/// Un intento de avisarle a alguien — <b>§5.3.B.3</b>.
///
/// ── Por qué se guarda el intento y no sólo el éxito ─────────────────────────
/// Mismo criterio que <see cref="FilaDeIntentoBloqueado"/>: <b>un sistema que sólo guarda lo que
/// salió no puede contestar si se avisó</b>. Un aviso perfecto y uno que nunca se intentó se ven
/// exactamente igual —no hay rastro de ninguno de los dos— y eso es justamente lo que separa
/// «no contestó» de «nadie le escribió».
///
/// ── Y una fila por destinatario, no por tarea ───────────────────────────────
/// Un puesto puede estar coocupado durante un traspaso. Guardar un solo aviso por tarea diría
/// que se avisó cuando a una de las dos personas no le llegó.
/// </summary>
public sealed class FilaDeAviso
{
    public required Ulid Id { get; init; }

    public required Ulid Tarea { get; init; }

    public required string Destinatario { get; init; }

    /// <summary>
    /// Por dónde se intentó. <b>Nulo cuando la institución no fijó el canal</b> —insumo #102—,
    /// que no es lo mismo que un canal que falló.
    /// </summary>
    public string? Canal { get; init; }

    public required string Resultado { get; init; }

    public required DateTime MomentoUtc { get; init; }

    /// <summary>Por qué no salió. <b>Nulo cuando se entregó</b>: un motivo ahí sería ruido.</summary>
    public string? Detalle { get; init; }
}
