using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// El control <b>bloqueante</b> de §5.3.B — impedir el acto sobre un expediente concreto.
///
/// <i>«Es aquí donde se decide de verdad.»</i> El preventivo de §5.3.A sólo puede rechazar la
/// acumulación absoluta; acá hay un expediente y se puede comparar persona contra persona.
/// </summary>
public class ReglasDeSegregacionPruebas
{
    private static readonly IdPersona Nery = new("P-NERY");
    private static readonly IdPersona Karla = new("P-KARLA");
    private static readonly DateTimeOffset Momento =
        new(2026, 8, 29, 10, 0, 0, TimeSpan.FromHours(-6));

    private static ActosDelExpediente Actos(params ActoDelExpediente[] actos) => new(actos);

    private static ActoDelExpediente Acto(Funcion funcion, IdPersona quien, string referencia) =>
        new(funcion, quien, referencia, new DateOnly(2026, 8, 3));

    // ── El bloqueo, y su recíproco ──────────────────────────────────────────

    /// <summary>
    /// `I-04`: quien pide no declara cómo terminó lo que pidió.
    ///
    /// Es uno de los pares que <b>hasta ahora no bloqueaba en ninguna parte</b>: `BD-01` cubre
    /// la autorización de la misión y nada más.
    /// </summary>
    [Fact]
    public void Quien_solicito_no_puede_liquidar()
    {
        var error = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Liquida,
                Actos(Acto(Funcion.Solicita, Nery, "solicitud SOL-2026-00417")),
                "MIS-2026-0031", Momento));

        Assert.Equal("I-04", error.Par);
    }

    /// <summary>
    /// <b>El recíproco, que es lo que hace útil al bloqueo.</b> Si otra persona lo hizo, pasa.
    /// Sin esta prueba, una regla que bloqueara siempre pasaría la anterior y dejaría a la
    /// institución sin poder liquidar nada.
    /// </summary>
    [Fact]
    public void Si_lo_solicito_otra_persona_se_puede_liquidar()
    {
        var advertencias = ReglasDeSegregacion.Exigir(
            Nery, Funcion.Liquida,
            Actos(Acto(Funcion.Solicita, Karla, "solicitud SOL-2026-00417")),
            "MIS-2026-0031", Momento);

        Assert.Empty(advertencias);
    }

    /// <summary>
    /// <b>El par se lee en los dos sentidos.</b> Quien autoriza no liquida, y quien liquida no
    /// autoriza: es el mismo `I-07` leído al revés, y evaluarlo en una sola dirección dejaría
    /// abierta la mitad del par según el orden en que ocurrieran los actos.
    /// </summary>
    [Fact]
    public void El_par_bloquea_en_los_dos_sentidos()
    {
        var autorizoYQuiereLiquidar = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Liquida,
                Actos(Acto(Funcion.Autoriza, Nery, "autorización del 03/08")),
                "MIS-1", Momento));

        var liquidoYQuiereAutorizar = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Autoriza,
                Actos(Acto(Funcion.Liquida, Nery, "liquidación del 03/08")),
                "MIS-1", Momento));

        Assert.Equal("I-07", autorizoYQuiereLiquidar.Par);
        Assert.Equal("I-07", liquidoYQuiereAutorizar.Par);
    }

    /// <summary>
    /// `I-11` — <b>la autoliquidación, el vector de fraude clásico en combustible.</b> Quien
    /// condujo no liquida su propia misión.
    /// </summary>
    [Fact]
    public void Quien_condujo_no_liquida_su_propia_mision()
    {
        var error = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Liquida,
                Actos(Acto(Funcion.Conduce, Nery, "motorista asignado")),
                "MIS-1", Momento));

        Assert.Equal("I-11", error.Par);
        Assert.Contains("núcleo irreductible", error.Message);
    }

    /// <summary>
    /// `I-10`: quien entrega el dinero no puede declarar en qué se gastó.
    /// </summary>
    [Fact]
    public void Quien_entrego_el_fondo_no_liquida()
    {
        var error = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Liquida,
                Actos(Acto(Funcion.EntregaFondo, Nery, "vale VAL-2026-0088")),
                "MIS-1", Momento));

        Assert.Equal("I-10", error.Par);
    }

    // ── El mensaje de §5.3.B.1 ──────────────────────────────────────────────

    /// <summary>
    /// <b>El mensaje nombra el acto concreto y su fecha</b>, como el ejemplo del documento:
    /// *«Usted registró la solicitud SOL-2026-00417 el 03/08/2026. No puede autorizarla»*.
    ///
    /// §5.3: *«un mensaje genérico produce una llamada a soporte; un mensaje preciso produce la
    /// acción correcta»*. Sin esta prueba, un «incompatibilidad detectada» pasaría todas las
    /// demás.
    /// </summary>
    [Fact]
    public void El_mensaje_nombra_el_acto_concreto_su_fecha_y_el_par()
    {
        var error = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Liquida,
                Actos(Acto(Funcion.Solicita, Nery, "solicitud SOL-2026-00417")),
                "MIS-1", Momento));

        Assert.Contains("SOL-2026-00417", error.Message);
        Assert.Contains("03/08/2026", error.Message);
        Assert.Contains("I-04", error.Message);

        // Y dice qué hacer, en vez de dejar un callejón sin salida.
        Assert.Contains("otra persona", error.Message);
        Assert.Contains("escale", error.Message);
    }

    /// <summary>
    /// El mensaje <b>no lleva nombres de tipos</b>. Quien lo lee está tratando de resolver un
    /// trámite, no de depurar el sistema.
    /// </summary>
    [Fact]
    public void El_mensaje_no_muestra_identificadores_del_enum()
    {
        var error = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Liquida,
                Actos(Acto(Funcion.EntregaFondo, Nery, "vale VAL-1")),
                "MIS-1", Momento));

        Assert.DoesNotContain("EntregaFondo", error.Message);
        Assert.DoesNotContain("Funcion.", error.Message);
        Assert.Contains("la entrega del fondo", error.Message);
    }

    // ── El asiento de auditoría de §5.3.B.2 ─────────────────────────────────

    /// <summary>
    /// <b>El intento bloqueado es información de control, no ruido.</b> Trae los siete datos que
    /// §5.3.B.2 enumera, y viene armado desde el dominio para que quien lo persista no tenga que
    /// reconstruir cuál par se activó.
    /// </summary>
    [Fact]
    public void El_intento_bloqueado_trae_todo_lo_que_la_pista_necesita()
    {
        var error = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Autoriza,
                Actos(Acto(Funcion.Despacha, Nery, "despacho del 03/08")),
                "MIS-2026-0031", Momento));

        var i = error.Intento;

        Assert.Equal(Nery, i.Quien);
        Assert.Equal(Funcion.Autoriza, i.Pretendia);
        Assert.Equal("MIS-2026-0031", i.Expediente);
        Assert.Equal("I-05", i.Par);
        Assert.Equal(Funcion.Despacha, i.ChocaCon);
        Assert.Equal("despacho del 03/08", i.Referencia);
        Assert.Equal(Momento, i.Momento);
    }

    // ── Las advertencias de §5.3.B.4 ────────────────────────────────────────

    /// <summary>
    /// `I-15` es <b>advertencia, no bloqueo</b>: el custodio que autoriza la salida de su propio
    /// vehículo continúa **con motivo escrito**. Es práctica de control marcada `[I]`, sin norma
    /// expresa, y convertirla en bloqueo inventaría una norma.
    /// </summary>
    [Fact]
    public void El_custodio_que_autoriza_su_vehiculo_advierte_pero_no_bloquea()
    {
        var advertencias = ReglasDeSegregacion.Exigir(
            Nery, Funcion.Autoriza,
            Actos(Acto(Funcion.Custodia, Nery, "custodia del INS-P-014")),
            "MIS-1", Momento);

        var i15 = Assert.Single(advertencias);

        Assert.Equal("I-15", i15.Par);
        Assert.Equal(Funcion.Custodia, i15.ChocaCon);
    }

    // ── `I-14`, apagado por omisión ─────────────────────────────────────────

    /// <summary>
    /// <b>`I-14` está apagado por omisión y no bloquea; encendido, sí.</b>
    ///
    /// Se prueban las dos ramas a propósito: <b>probar sólo una deja el parámetro sin
    /// verificar</b>, y fue esta prueba la que destapó que `I-14` estaba escrito con las
    /// funciones de `I-07` y era inalcanzable — el configurable no decidía nada nunca.
    ///
    /// El par es <i>emitir la Orden de Misión</i> × liquidar, que es de `ACT-04`. No es
    /// <i>autorizar la necesidad</i>, que es de `ACT-03` y es `I-07`.
    /// </summary>
    [Fact]
    public void I14_no_bloquea_apagado_y_si_bloquea_encendido()
    {
        var actos = Actos(Acto(Funcion.EmiteOrdenDeMision, Nery, "orden ORD-2026-0114"));

        // Apagado: ACT-04 emite y liquida por diseño, y encenderlo por omisión lo dejaría
        // sin operar. No está en la enumeración del MARCI.
        Assert.Empty(ReglasDeSegregacion.Exigir(
            Nery, Funcion.Liquida, actos, "MIS-1", Momento, i14Activo: false));

        // Encendido: bloquea, y es `I-14` y no otro par.
        var error = Assert.Throws<SegregacionIncompatible>(() =>
            ReglasDeSegregacion.Exigir(
                Nery, Funcion.Liquida, actos, "MIS-1", Momento, i14Activo: true));

        Assert.Equal("I-14", error.Par);
    }

    // ── El expediente limpio ────────────────────────────────────────────────

    /// <summary>
    /// Sin actos previos no hay contra qué chocar. <b>`Ninguno` es una afirmación</b> —consta
    /// que no hay actos— y no una lista vacía por descuido.
    /// </summary>
    [Fact]
    public void Un_expediente_sin_actos_previos_no_bloquea_nada()
    {
        var advertencias = ReglasDeSegregacion.Exigir(
            Nery, Funcion.Autoriza, ActosDelExpediente.Ninguno, "MIS-1", Momento);

        Assert.Empty(advertencias);
    }

    /// <summary>
    /// Los pares <b>absolutos</b> no entran acá: `I-12` e `I-13` hablan de acumulación de roles,
    /// no de actos, y ya se rechazaron al asignarlos. Evaluarlos otra vez sobre un expediente
    /// bloquearía a un auditor por haber consultado, que es exactamente lo que debe hacer.
    /// </summary>
    [Fact]
    public void Los_pares_absolutos_no_se_evaluan_sobre_actos()
    {
        var advertencias = ReglasDeSegregacion.Exigir(
            Nery, Funcion.Audita,
            Actos(Acto(Funcion.Despacha, Nery, "despacho del 03/08")),
            "MIS-1", Momento);

        Assert.Empty(advertencias);
    }

    // ── El escalamiento de §5.3.B.3 ─────────────────────────────────────────

    /// <summary>
    /// ⚠️ <b>El escalamiento está a medias, y lo dice.</b> El documento pide tres saltos y el
    /// espejo del organigrama sólo trae persona↔puesto: sin puesto superior ni respaldo de sede,
    /// los dos primeros no se pueden resolver.
    ///
    /// Inventar un destinatario sería peor que declararlo: la misión quedaría *«visiblemente
    /// pendiente»* en la bandeja equivocada.
    /// </summary>
    [Fact]
    public void El_escalamiento_declara_que_le_falta_la_jerarquia()
    {
        var sinJerarquia = ReglasDeSegregacion.DestinoDelEscalamiento(false);

        Assert.Contains("ACT-08", sinJerarquia);
        Assert.Contains("no trae todavía el puesto superior", sinJerarquia);

        Assert.Contains("puesto superior", ReglasDeSegregacion.DestinoDelEscalamiento(true));
    }
}
