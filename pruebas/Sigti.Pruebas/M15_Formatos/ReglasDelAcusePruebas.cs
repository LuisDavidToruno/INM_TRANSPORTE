using Sigti.Dominio.M15_Formatos;

namespace Sigti.Pruebas.M15_Formatos;

/// <summary>
/// `RN-65` — <b>emitir, imprimir y entregar contra acuse</b>.
///
/// ── Por qué el acuse no es una formalidad ───────────────────────────────────
/// Emitir e imprimir son actos de oficina. <b>El documento sirve cuando está en la guantera</b>,
/// y entre la impresora y el vehículo se pierde: se imprime y queda en el escritorio, se
/// entrega al motorista equivocado, o se despacha antes de que salga la impresión.
///
/// El acuse separa <i>«el sistema emitió el papel»</i> de <i>«el motorista lo tiene»</i>, y en
/// un operativo sólo la segunda importa.
/// </summary>
public class ReglasDelAcusePruebas
{
    [Fact]
    public void Con_documento_emitido_y_el_motorista_de_la_orden_se_acusa()
    {
        Assert.Null(ReglasDelAcuse.PorQueNoSeAcusa(
            DocumentoEntregado.Salvoconducto,
            documentoEmitido: true,
            recibeElMotoristaDeLaOrden: true,
            yaAcusado: false));
    }

    /// <summary>
    /// <b>No se acusa lo que no se emitió.</b> Un acuse sobre un papel inexistente es una firma
    /// sobre nada: deja constancia de una entrega que no ocurrió, que es peor que no tener
    /// constancia.
    /// </summary>
    [Fact]
    public void No_se_acusa_un_documento_que_no_se_emitio()
    {
        var porQue = ReglasDelAcuse.PorQueNoSeAcusa(
            DocumentoEntregado.Salvoconducto,
            documentoEmitido: false,
            recibeElMotoristaDeLaOrden: true,
            yaAcusado: false);

        Assert.NotNull(porQue);
        Assert.Contains("firma sobre nada", porQue);
    }

    /// <summary>
    /// ⚠️ <b>El documento es nominativo.</b> El salvoconducto ampara a <b>ese</b> motorista y el
    /// paquete identifica al vehículo que <b>ese</b> motorista conduce: entregárselo a otro
    /// produce un acuse que no prueba nada, y el papel viaja igual sin que conste quién lo lleva.
    /// </summary>
    [Fact]
    public void No_lo_acusa_alguien_distinto_del_motorista_de_la_orden()
    {
        var porQue = ReglasDelAcuse.PorQueNoSeAcusa(
            DocumentoEntregado.Salvoconducto,
            documentoEmitido: true,
            recibeElMotoristaDeLaOrden: false,
            yaAcusado: false);

        Assert.NotNull(porQue);
        Assert.Contains("nominativo", porQue);
        Assert.Contains("sin que conste quién lo lleva", porQue);
    }

    /// <summary>
    /// Dos acuses del mismo documento dejarían <b>dos personas declarando haberlo recibido</b>,
    /// y ninguna de las dos se podría sostener.
    /// </summary>
    [Fact]
    public void No_se_acusa_dos_veces_el_mismo_documento()
    {
        var porQue = ReglasDelAcuse.PorQueNoSeAcusa(
            DocumentoEntregado.PaqueteDeIdentificacion,
            documentoEmitido: true,
            recibeElMotoristaDeLaOrden: true,
            yaAcusado: true);

        Assert.NotNull(porQue);
        Assert.Contains("dos personas declarando", porQue);
    }

    /// <summary>
    /// El orden de los rechazos: <b>«no se emitió» antes que «no es el motorista»</b>.
    ///
    /// Decirle a alguien que no puede firmar la recepción de un papel que no existe lo manda a
    /// buscar al motorista correcto para un documento que nadie imprimió.
    /// </summary>
    [Fact]
    public void Sobre_un_documento_inexistente_se_reporta_eso_y_no_el_motorista()
    {
        var porQue = ReglasDelAcuse.PorQueNoSeAcusa(
            DocumentoEntregado.Salvoconducto,
            documentoEmitido: false,
            recibeElMotoristaDeLaOrden: false,
            yaAcusado: false);

        Assert.NotNull(porQue);
        Assert.Contains("No hay salvoconducto emitido", porQue);
        Assert.DoesNotContain("nominativo", porQue);
    }

    /// <summary>
    /// El mensaje nombra <b>cuál</b> de los dos documentos. No se confunden: el salvoconducto
    /// ampara circular en franja inhábil y el paquete identifica al vehículo sin lámina, y un
    /// aviso genérico dejaría sin saber cuál falta.
    /// </summary>
    [Theory]
    [InlineData(DocumentoEntregado.Salvoconducto, "salvoconducto")]
    [InlineData(DocumentoEntregado.PaqueteDeIdentificacion, "paquete de identificación")]
    public void El_mensaje_nombra_el_documento(DocumentoEntregado documento, string esperado)
    {
        var porQue = ReglasDelAcuse.PorQueNoSeAcusa(
            documento, documentoEmitido: false, recibeElMotoristaDeLaOrden: true,
            yaAcusado: false);

        Assert.NotNull(porQue);
        Assert.Contains(esperado, porQue);
    }
}
