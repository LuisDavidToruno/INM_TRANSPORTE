namespace Sigti.Dominio.M15_Formatos;

/// <summary>
/// Qué documento se entregó. <b>Cada uno prueba algo distinto</b> y por eso no se confunden: el
/// salvoconducto ampara circular en franja inhábil, y el paquete identifica al vehículo sin
/// lámina. Un acuse genérico dejaría sin saber cuál de los dos llegó a la mano del motorista.
/// </summary>
public enum DocumentoEntregado
{
    /// <summary>`RN-25` — el salvoconducto de circulación en día u hora inhábil.</summary>
    Salvoconducto,

    /// <summary>`RN-65` — el paquete de identificación en carretera del vehículo sin lámina.</summary>
    PaqueteDeIdentificacion,
}

/// <summary>
/// `RN-65` — <b>emitir, imprimir y entregar contra acuse</b>.
///
/// ── Por qué el acuse no es una formalidad ───────────────────────────────────
/// Emitir e imprimir son actos de oficina. <b>El documento sirve cuando está en la guantera</b>,
/// y entre la impresora y el vehículo se pierde: se imprime y queda en el escritorio, se
/// entrega al motorista equivocado, o se despacha antes de que salga la impresión.
///
/// El acuse es lo que separa <i>«el sistema emitió el papel»</i> de <i>«el motorista lo
/// tiene»</i>, y en un operativo sólo la segunda importa.
///
/// ── Y por qué se registra a quién, no sólo que sí ───────────────────────────
/// Porque `§10.2` describe `DESPACHADA` como el estado donde <i>«el motorista ya tiene en la
/// mano … los documentos del vehículo … Firmó la recepción»</i>. Un acuse sin nombre no
/// distingue al motorista que recibió de quien pasaba por ahí.
/// </summary>
public static class ReglasDelAcuse
{
    /// <summary>
    /// Por qué no se puede registrar este acuse. <b>Nulo es que sí.</b>
    /// </summary>
    /// <param name="documentoEmitido">
    /// Si el documento existe. <b>No se acusa lo que no se emitió</b>: un acuse sobre un papel
    /// inexistente es una firma sobre nada, y deja constancia de una entrega que no ocurrió.
    /// </param>
    /// <param name="recibeElMotoristaDeLaOrden">
    /// Si quien firma la recepción es el motorista asignado. <b>Falso bloquea</b>: el
    /// salvoconducto es nominativo y el paquete identifica al vehículo que <b>ese</b> motorista
    /// conduce — entregárselo a otro produce un acuse que no prueba nada.
    /// </param>
    public static string? PorQueNoSeAcusa(
        DocumentoEntregado documento,
        bool documentoEmitido,
        bool recibeElMotoristaDeLaOrden,
        bool yaAcusado)
    {
        if (!documentoEmitido)
        {
            return $"No hay {Texto(documento)} emitido para esta misión. Un acuse sobre un " +
                   "papel inexistente es una firma sobre nada: deja constancia de una entrega " +
                   "que no ocurrió.";
        }

        if (yaAcusado)
        {
            return $"El {Texto(documento)} de esta misión ya tiene acuse. Dos acuses del mismo " +
                   "documento dejarían dos personas declarando haberlo recibido, y ninguna de " +
                   "las dos se podría sostener.";
        }

        return recibeElMotoristaDeLaOrden
            ? null
            : "Quien firma la recepción no es el motorista asignado a la orden. El documento " +
              "es nominativo: entregárselo a otro produce un acuse que no prueba nada, y el " +
              "papel viaja igual sin que conste quién lo lleva.";
    }

    public static string Texto(DocumentoEntregado documento) => documento switch
    {
        DocumentoEntregado.Salvoconducto => "salvoconducto",
        DocumentoEntregado.PaqueteDeIdentificacion => "paquete de identificación",
        _ => documento.ToString(),
    };
}
