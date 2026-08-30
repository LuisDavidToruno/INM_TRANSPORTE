using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M16_Sincronizacion;

/// <summary>
/// Qué se hace con un hecho de campo que llega <b>después</b> de que la oficina cerró — `HU-070`,
/// `RN-45`, `RN-05`.
///
/// ── El caso más frecuente y el que más tienta a descartar ───────────────────
/// `RN-45` lo dice así: <i>«Registro de campo que llega después del cierre en oficina. No se
/// descarta ni se aplica: entra a la cola con su fecha del hecho. <b>Es el caso más frecuente y
/// el que más tienta a implementar un descarte automático.</b>»</i>
///
/// Un dispositivo que estuvo seis días sin señal sincroniza un abastecimiento del día 15 cuando
/// la misión se liquidó el 20. El hecho ocurrió. Descartarlo borra un galón que alguien pagó.
///
/// ── Y el destino depende del estado, no del hecho ───────────────────────────
/// <b>No es lo mismo llegar tarde a una liquidación que a un expediente cerrado.</b> Sobre lo
/// liquidado se puede registrar un asiento de diferencia; sobre lo cerrado no, porque reabrir
/// haría que <i>«un reporte ya emitido cambie de contenido a espaldas»</i> de quien lo firmó.
/// </summary>
public static class ReglasDeLlegadaTardia
{
    /// <summary>
    /// Los estados en que la misión <b>ya no admite</b> que se le aplique un hecho nuevo.
    ///
    /// `CERRADA_CON_HALLAZGO` cuenta igual que `CERRADA`: el expediente está terminado, y que
    /// tenga un hallazgo previo no lo vuelve editable — lo vuelve un expediente cerrado con más
    /// historia.
    /// </summary>
    public static bool EstaCerrada(EstadoDeMision estado) =>
        estado is EstadoDeMision.Cerrada or EstadoDeMision.CerradaConHallazgo;

    /// <summary>
    /// A dónde va el hecho que llegó tarde.
    /// </summary>
    public static DestinoDeLoTardio Resolver(EstadoDeMision estado) => estado switch
    {
        // ⚠️ Cerrada NO se reabre. El hecho abre su propio expediente, con ciclo propio, y la
        // misión pasa a mostrar que tiene hallazgos vinculados — sin que su contenido cambie.
        _ when EstaCerrada(estado) => DestinoDeLoTardio.HallazgoPosterior,

        // Liquidada: la cifra ya se emitió, pero el expediente todavía no terminó. Va a la cola
        // y de ahí sale un asiento de diferencia, que conserva la liquidación original íntegra.
        EstadoDeMision.Liquidada => DestinoDeLoTardio.ColaDeConflictos,

        // Todavía en curso: es una divergencia común y se decide como cualquier otra.
        _ => DestinoDeLoTardio.ColaDeConflictos,
    };

    /// <summary>
    /// Lo que se le dice a quien intenta modificar una liquidación cerrada — `HU-070`, textual.
    /// </summary>
    public static string PorQueNoSeEditaLaLiquidacion(string folio) =>
        $"La liquidación de {folio} está cerrada. Registre un asiento de diferencia con su " +
        "motivo y su respaldo.";

    /// <summary>
    /// Lo que se le dice a quien esperaba que la misión se reabriera — `HU-070`, textual.
    ///
    /// <b>Nombra el expediente que se abrió.</b> Sin ese dato, quien lee sabe que su registro no
    /// entró y no sabe dónde quedó — y vuelve a enviarlo, o lo anota en papel.
    /// </summary>
    public static string PorQueNoSeReabre(string folio, string expedienteDelHallazgo) =>
        $"{folio} está cerrada y no se reabre. Se abrió el expediente de hallazgo posterior " +
        $"{expedienteDelHallazgo}.";
}

public enum DestinoDeLoTardio
{
    /// <summary>Una persona decide entre las dos versiones. La misión sigue viva.</summary>
    ColaDeConflictos,

    /// <summary>
    /// Expediente aparte, con su propio ciclo. <b>La misión cerrada no se toca</b>: sólo pasa a
    /// mostrar que tiene hallazgos vinculados.
    /// </summary>
    HallazgoPosterior,
}
