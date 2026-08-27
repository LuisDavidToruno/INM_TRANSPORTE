namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Por qué se cambia el vehículo o el motorista de una misión ya programada — `T-10`.
///
/// ── Por qué NO reusa <see cref="MotivoDeAnulacion"/> ─────────────────────────
/// Porque contestan preguntas distintas y alimentan indicadores distintos. El de anulación
/// dice <i>por qué esta movilización no se hizo</i> y mide <b>déficit de flota</b>. Éste dice
/// <i>por qué el recurso que se había comprometido dejó de servir</i> y mide otra cosa:
/// <b>fiabilidad de la flota y del padrón</b>. Un vehículo que entra a taller tres veces en
/// el mes no es déficit —hay vehículos—, es un vehículo malo, y mezclarlos haría que el
/// reporte de déficit contara averías.
///
/// ── El catálogo es el de la autoridad, sin agregados ─────────────────────────
/// Los cuatro valores salen literalmente de la ficha de `T-10`: <i>«vehículo a taller,
/// motorista no disponible, cambio de requerimiento, consolidación»</i>. No se inventa un
/// quinto: un catálogo que crece por conveniencia deja de ser comparable entre períodos, y
/// el comentario libre existe justamente para lo que no encaja.
/// </summary>
public enum MotivoDeReasignacion
{
    /// <summary>El vehículo saliente entró a mantenimiento — correctivo o preventivo.</summary>
    VehiculoATaller,

    /// <summary>
    /// Quien iba a conducir dejó de estar disponible: incapacidad, permiso, vacaciones,
    /// licencia vencida entre la programación y hoy.
    /// </summary>
    MotoristaNoDisponible,

    /// <summary>
    /// Cambió lo que hay que mover, y el vehículo que servía ya no sirve. La ventana no
    /// cambia —eso sería otra solicitud—: cambia la carga o el número de personas.
    /// </summary>
    CambioDeRequerimiento,

    /// <summary>
    /// Se junta con otra misión en una sola Orden. Es el camino preferente de `EF-01` ante
    /// un conflicto, y el único que produce ahorro real en vez de sólo resolverlo.
    /// </summary>
    Consolidacion,
}
