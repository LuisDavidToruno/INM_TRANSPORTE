using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M12_Incidentes;

namespace Sigti.Pruebas.M12_Incidentes;

/// <summary>
/// `RN-70` — la interrupción en ruta exige desenlace explícito.
///
/// ── El bloqueo que hasta M-12 no podía disparar ─────────────────────────────
/// <i>«Ninguna misión con marca de interrupción sin desenlace puede quedar viva al cierre del
/// período»</i>. `RN-97` punto 4 le da poder de bloqueo, y el saldo de apertura la declaraba
/// <i>«no consultable»</i> porque no existía como registro.
/// </summary>
public class ReglasDeLaInterrupcionPruebas
{
    private static readonly DateOnly Hecho = new(2026, 11, 18);

    // ── Ninguna interrupción sin desenlace sobrevive al cierre ──────────────

    [Fact]
    public void Una_interrupcion_sin_desenlace_impide_cerrar_el_periodo()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaInterrupcion.ExigirDesenlaceAntesDelCierre(
                [Expediente(interrumpe: true, desenlace: null)], declaracionExplicita: null));

        Assert.Equal("RN-70", error.Precondicion);
        Assert.Contains("sin desenlace", error.Message);
        Assert.Contains("P-TRANSPORTE", error.Message);
    }

    /// <summary>
    /// `RN-97` punto 4: <i>«hay que resolverlos <b>o declararlos explícitamente</b>»</i>.
    /// Declararlos es un acto con motivo que queda en el documento; ignorarlos no es opción.
    /// </summary>
    [Fact]
    public void Declarada_explicitamente_la_interrupcion_deja_cerrar() =>
        ReglasDeLaInterrupcion.ExigirDesenlaceAntesDelCierre(
            [Expediente(interrumpe: true, desenlace: null)],
            "La unidad sigue retenida por la Fiscalía y el desenlace depende de esa resolución.");

    [Fact]
    public void Con_desenlace_registrado_el_periodo_cierra() =>
        ReglasDeLaInterrupcion.ExigirDesenlaceAntesDelCierre(
            [Expediente(interrumpe: true, desenlace: DesenlaceDeLaInterrupcion.RetornoAnticipado)],
            declaracionExplicita: null);

    /// <summary>
    /// Un incidente que <b>no interrumpió</b> —una multa, un uso indebido descubierto después—
    /// no bloquea el cierre. Si lo hiciera, el bloqueo dispararía con cualquier expediente
    /// abierto y dejaría de significar lo que `RN-70` quiere que signifique.
    /// </summary>
    [Fact]
    public void Un_incidente_que_no_interrumpio_no_bloquea_el_cierre() =>
        ReglasDeLaInterrupcion.ExigirDesenlaceAntesDelCierre(
            [Expediente(interrumpe: false, desenlace: null)], declaracionExplicita: null);

    /// <summary>
    /// Una interrupción <b>ya resuelta</b> tampoco bloquea: el expediente cerró, y su marca dejó
    /// de estar viva.
    /// </summary>
    [Fact]
    public void Una_interrupcion_resuelta_no_bloquea_el_cierre() =>
        ReglasDeLaInterrupcion.ExigirDesenlaceAntesDelCierre(
            [Expediente(interrumpe: true, desenlace: null, resuelto: new DateOnly(2026, 12, 1))],
            declaracionExplicita: null);

    // ── Registrar el desenlace ──────────────────────────────────────────────

    [Fact]
    public void El_desenlace_de_un_hecho_que_no_interrumpio_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaInterrupcion.ExigirDesenlaceRegistrable(
                Expediente(interrumpe: false, desenlace: null), "Autorizó ACT-04"));

        Assert.Contains("inventaría una interrupción que no ocurrió", error.Message);
    }

    /// <summary>
    /// Reescribir el desenlace borraría el que constaba. Una corrección se registra como asiento
    /// nuevo con referencia al anterior (`RN-42`), no sobreescribiendo la historia.
    /// </summary>
    [Fact]
    public void Un_segundo_desenlace_sobre_la_misma_interrupcion_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaInterrupcion.ExigirDesenlaceRegistrable(
                Expediente(interrumpe: true, desenlace: DesenlaceDeLaInterrupcion.Continuar),
                "Otro detalle"));

        Assert.Contains("borraría el que constaba", error.Message);
    }

    /// <summary>
    /// `RN-70` — continuar exige <b>constancia de quién lo autorizó</b>, y las otras tres exigen
    /// decir contra qué acto se resolvieron.
    /// </summary>
    [Fact]
    public void Un_desenlace_sin_constancia_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaInterrupcion.ExigirDesenlaceRegistrable(
                Expediente(interrumpe: true, desenlace: null), "   "));

        Assert.Contains("sin que se pueda decir por quién", error.Message);
    }

    [Fact]
    public void Un_desenlace_con_constancia_sobre_una_interrupcion_viva_pasa() =>
        ReglasDeLaInterrupcion.ExigirDesenlaceRegistrable(
            Expediente(interrumpe: true, desenlace: null),
            "Autorizado por ACT-04 mediante llamada registrada a las 14:20.");

    // ── La propiedad que sostiene todo lo anterior ──────────────────────────

    [Theory]
    [InlineData(true, false, true)]     // interrumpe y no tiene desenlace → bloquea
    [InlineData(true, true, false)]     // interrumpe y ya se resolvió → no
    [InlineData(false, false, false)]   // no interrumpió → no
    public void La_marca_de_interrupcion_sin_desenlace_se_calcula_bien(
        bool interrumpe, bool conDesenlace, bool esperado)
    {
        var expediente = Expediente(
            interrumpe,
            conDesenlace ? DesenlaceDeLaInterrupcion.Continuar : null);

        Assert.Equal(esperado, expediente.EsInterrupcionSinDesenlace);
    }

    private static ExpedienteDeIncidente Expediente(
        bool interrumpe,
        DesenlaceDeLaInterrupcion? desenlace,
        DateOnly? resuelto = null) =>
        new(Ulid.NewUlid(),
            TipoDeIncidente.AveriaMecanica,
            "Falla de transmisión",
            Hecho,
            new DateTimeOffset(2026, 11, 18, 14, 0, 0, TimeSpan.FromHours(-6)),
            new DateTimeOffset(2026, 11, 18, 19, 0, 0, TimeSpan.FromHours(-6)),
            "El vehículo quedó en el km 61 sin poder avanzar.",
            "P-MOTORISTA",
            Ulid.NewUlid(),
            Ulid.NewUlid(),
            "km 61, CA-5",
            84_310,
            interrumpe,
            [],
            [],
            [],
            "P-TRANSPORTE",
            new DateOnly(2026, 11, 25),
            Desenlace: desenlace,
            ResueltoEn: resuelto,
            ComoSeResolvio: resuelto is null ? null : "Cerrado con la unidad reparada.");
}
