using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M17_PersonasExternas;

namespace Sigti.Pruebas.M17_PersonasExternas;

/// <summary>
/// `HU-124` — la depuración de datos personales.
///
/// Es <b>lo único en todo el sistema que destruye contenido</b>. Todo lo demás se reversa, se
/// anula o se marca.
/// </summary>
public class ReglasDeLaDepuracionPruebas
{
    private static readonly DateTimeOffset Ahora = new(2029, 1, 15, 8, 0, 0, TimeSpan.Zero);

    // ── Sin plazo no se depura, y no hay plazo por omisión ──────────────────

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-30)]
    public void Sin_plazo_configurado_no_se_depura_nada(int? plazo)
    {
        // ⚠️ Y **no se aplica ninguno por omisión**. Un plazo por defecto sería el equipo
        // decidiendo cuánto conserva la institución los datos de las personas que trasladó —
        // que es la decisión que `[C]` deja a Auditoría Interna y al OIP.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaDepuracion.ExigirPlazoConfigurado(plazo));

        Assert.Contains("no está configurado", e.Message);
        Assert.Contains("Oficial de Información Pública", e.Message);
    }

    [Fact]
    public void Con_plazo_configurado_pasa()
    {
        ReglasDeLaDepuracion.ExigirPlazoConfigurado(1095);
    }

    // ── Lo financiero y los bienes no se tocan ──────────────────────────────

    [Theory]
    [InlineData("liquidacion")]
    [InlineData("vale")]
    [InlineData("combustible")]
    [InlineData("vehiculo")]
    [InlineData("peaje")]
    public void Los_registros_financieros_y_de_bienes_no_se_depuran(string segmento)
    {
        // Se conservan por el plazo de fiscalización. Borrarlos dejaría al Tribunal Superior de
        // Cuentas sin con qué probar un asiento.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaDepuracion.ExigirSoloDatosPersonales([segmento]));

        Assert.Contains("plazo de fiscalización", e.Message);
        Assert.Contains(segmento, e.Message);
    }

    [Fact]
    public void El_mensaje_enumera_TODO_lo_que_queda_fuera()
    {
        // Nombrar sólo el primero obligaría a intentarlo una vez por cada segmento hasta dar
        // con la lista completa.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaDepuracion.ExigirSoloDatosPersonales(
                ["manifiesto-personas", "liquidacion", "vale"]));

        Assert.Contains("liquidacion", e.Message);
        Assert.Contains("vale", e.Message);
    }

    [Fact]
    public void El_segmento_de_datos_personales_si_se_depura()
    {
        ReglasDeLaDepuracion.ExigirSoloDatosPersonales(
            ["manifiesto-personas", "identificacion-de-pasajeros"]);
    }

    // ── El aviso previo ─────────────────────────────────────────────────────

    [Fact]
    public void Sin_aviso_previo_no_se_ejecuta()
    {
        // Una destrucción silenciosa **es indistinguible de una pérdida de datos**. El día que
        // falte un manifiesto de hace tres años, nadie podrá decir cuál de las dos ocurrió.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaDepuracion.ExigirAvisoPrevio(avisadoEl: null, Ahora));

        Assert.Contains("No se ejecuta sin aviso previo", e.Message);
    }

    [Fact]
    public void Avisar_el_mismo_dia_no_es_avisar_con_antelacion()
    {
        // `HU-124`, escenario textual: intentar ejecutarla el mismo día se rechaza. Un aviso
        // simultáneo no le da a nadie tiempo de objetar.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaDepuracion.ExigirAvisoPrevio(Ahora, Ahora));

        Assert.Contains("previo", e.Message);
    }

    [Fact]
    public void Con_aviso_anterior_a_la_ejecucion_pasa()
    {
        ReglasDeLaDepuracion.ExigirAvisoPrevio(Ahora.AddDays(-30), Ahora);
    }

    // ── El plazo se cuenta desde el hecho ───────────────────────────────────

    [Fact]
    public void El_plazo_corre_desde_que_se_cerro_el_manifiesto()
    {
        // Y no desde la captura: un manifiesto digitado con tres meses de retraso **no gana
        // tres meses de conservación** por eso.
        var cerrado = Ahora.AddDays(-1100);

        Assert.True(ReglasDeLaDepuracion.AlcanzoElPlazo(cerrado, 1095, Ahora));
        Assert.False(ReglasDeLaDepuracion.AlcanzoElPlazo(Ahora.AddDays(-100), 1095, Ahora));
    }

    [Fact]
    public void El_dia_exacto_del_plazo_ya_alcanza()
    {
        Assert.True(ReglasDeLaDepuracion.AlcanzoElPlazo(Ahora.AddDays(-1095), 1095, Ahora));
    }
}

/// <summary>
/// `HU-122` — la rectificación por hábeas data.
///
/// <i>«Sin romper la cadena de auditoría que el Tribunal Superior de Cuentas va a revisar.»</i>
/// </summary>
public class ReglasDeLaRectificacionPruebas
{
    [Theory]
    [InlineData(null, "el nombre estaba mal escrito")]
    [InlineData("", "el nombre estaba mal escrito")]
    [InlineData("   ", "el nombre estaba mal escrito")]
    public void Rectificar_exige_decir_quien_lo_pidio(string? quien, string motivo)
    {
        // El hábeas data **sólo lo puede interponer el titular**: sin ese dato, el cambio es
        // indistinguible de una corrección interna sobre un dato personal — que es lo que
        // `RN-04` prohíbe hacer sin asiento.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaRectificacion.Exigir(quien, motivo));

        Assert.Contains("titular", e.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("mal")]
    public void Y_exige_por_que(string? motivo)
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaRectificacion.Exigir("María Elena Zúniga", motivo));

        // El original queda como estaba: esto es lo único que explica la diferencia entre los
        // dos documentos.
        Assert.Contains("original queda como estaba", e.Message);
    }

    [Fact]
    public void Con_solicitante_y_motivo_pasa()
    {
        ReglasDeLaRectificacion.Exigir(
            "María Elena Zúniga", "la identidad quedó con un dígito cambiado al digitar");
    }

    [Fact]
    public void La_rectificacion_conserva_el_valor_anterior()
    {
        // No es una corrección: es un asiento que dice qué decía y qué dice ahora. Un
        // manifiesto editado deja de coincidir con la lista impresa que el motorista llevó.
        var r = new Rectificacion(
            Ulid.NewUlid(), Ulid.NewUlid(), "identificacion",
            "0801-1990-12345", "0801-1990-12354",
            "María Elena Zúniga", "un dígito cambiado al digitar",
            new Sigti.Dominio.Organizacion.IdPersona("P-ADMIN"),
            new DateTimeOffset(2029, 2, 1, 10, 0, 0, TimeSpan.Zero));

        Assert.Equal("0801-1990-12345", r.ValorAnterior);
        Assert.NotEqual(r.ValorAnterior, r.ValorRectificado);
    }
}
