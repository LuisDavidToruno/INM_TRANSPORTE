using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M17_PersonasExternas;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M17_PersonasExternas;

/// <summary>
/// `RN-51` y `HU-112` — la minimización de datos de personas externas.
///
/// <i>«Un dato que no se captura no se puede filtrar, no se puede publicar por error y no se
/// puede pedir por hábeas data.»</i>
/// </summary>
public class ReglasDelCampoSensiblePruebas
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 1, 10, 0, 0, TimeSpan.Zero);

    private static FundamentoDelCampo Fundamento(
        string legal = "Convenio interinstitucional 12-2026",
        string necesidad = "identificar a quien se traslada ante el control en carretera") =>
        new(legal, necesidad, new IdPersona("P-ADMIN"), Ahora);

    // ── Activar sin fundamento: advierte y marca ────────────────────────────

    [Fact]
    public void Un_campo_sensible_sin_fundamento_se_activa_y_queda_marcado()
    {
        // ⚠️ Va contra la intuición y es lo que `HU-112` pide: **el sistema activa el campo Y lo
        // marca**. Bloquear parece más seguro y es peor — quien necesita el dato hoy lo va a
        // capturar igual, en observaciones o en una libreta, y ahí queda fuera de todo control.
        var advertencia = ReglasDelCampoSensible.AdvertenciaAlActivar(
            ClaseDelCampo.Salud, fundamento: null);

        Assert.NotNull(advertencia);
        Assert.Contains("queda marcado", advertencia);
        Assert.Contains("Auditoría Interna", advertencia);
    }

    [Fact]
    public void El_campo_marcado_se_reconoce_como_tal()
    {
        var campo = new CampoDelManifiesto(
            "condicion-medica", "Condición médica", ClaseDelCampo.Salud, Activo: true, null);

        Assert.True(campo.SinFundamento);
    }

    [Fact]
    public void Un_campo_sensible_INACTIVO_no_se_marca()
    {
        // No está capturando nada. Marcarlo llenaría el reporte del auditor de campos que
        // existen en el catálogo y nadie usa — y un reporte con ruido se deja de mirar.
        var campo = new CampoDelManifiesto(
            "etnia", "Etnia", ClaseDelCampo.Etnia, Activo: false, null);

        Assert.False(campo.SinFundamento);
    }

    [Fact]
    public void Un_campo_minimo_no_exige_fundamento()
    {
        // Identificación, institución que motiva el traslado, origen y destino son el catálogo
        // autorizado de `RN-51`: pedirles fundamento convertiría el control en un trámite.
        Assert.Null(ReglasDelCampoSensible.AdvertenciaAlActivar(ClaseDelCampo.Minimo, null));

        var campo = new CampoDelManifiesto(
            "identificacion", "Identificación", ClaseDelCampo.Minimo, Activo: true, null);

        Assert.False(campo.SinFundamento);
    }

    [Fact]
    public void Con_fundamento_no_hay_advertencia_y_deja_de_estar_marcado()
    {
        Assert.Null(ReglasDelCampoSensible.AdvertenciaAlActivar(
            ClaseDelCampo.Salud, Fundamento()));

        var campo = new CampoDelManifiesto(
            "condicion-medica", "Condición médica", ClaseDelCampo.Salud, true, Fundamento());

        Assert.False(campo.SinFundamento);
    }

    [Theory]
    [InlineData(ClaseDelCampo.Salud, "salud")]
    [InlineData(ClaseDelCampo.Etnia, "etnia")]
    [InlineData(ClaseDelCampo.SituacionMigratoria, "situación migratoria")]
    [InlineData(ClaseDelCampo.CondicionDeVulnerabilidad, "condición de vulnerabilidad")]
    public void Las_cuatro_clases_sensibles_son_las_que_la_norma_enumera(
        ClaseDelCampo clase, string palabra)
    {
        // Son exactamente las que `NRM-07` nombra. Agregar una quinta por criterio propio
        // sería inventar normativa; quitar una dejaría un dato sensible sin control.
        Assert.True(ReglasDelCampoSensible.EsSensible(clase));
        Assert.Equal(palabra, ReglasDelCampoSensible.EnPalabras(clase));
    }

    // ── El fundamento exige las dos mitades ─────────────────────────────────

    [Theory]
    [InlineData("Convenio 12-2026", null)]
    [InlineData("Convenio 12-2026", "")]
    [InlineData("Convenio 12-2026", "   ")]
    [InlineData(null, "para el control en carretera")]
    [InlineData("", "para el control en carretera")]
    public void Media_justificacion_se_rechaza(string? legal, string? necesidad)
    {
        // `HU-112`, textual: «El fundamento requiere las dos cosas».
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelCampoSensible.ExigirFundamentoCompleto(legal, necesidad));

        Assert.Contains("las dos cosas", e.Message);
    }

    [Fact]
    public void La_necesidad_operativa_es_la_mitad_que_de_verdad_limita()
    {
        // La base legal sola autoriza capturar todo lo que la norma no prohíba, que en un país
        // sin ley de datos es casi todo. La pregunta que limita es la otra: ¿para qué operación
        // del traslado hace falta? — y hay campos que no la pueden contestar.
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelCampoSensible.ExigirFundamentoCompleto(
                "Convenio interinstitucional 12-2026", necesidadOperativa: null));
    }

    [Fact]
    public void Un_fundamento_completo_pasa()
    {
        ReglasDelCampoSensible.ExigirFundamentoCompleto(
            "Convenio interinstitucional 12-2026",
            "identificar a quien se traslada ante el control en carretera");
    }

    // ── La salida que no captura el dato ────────────────────────────────────

    [Fact]
    public void Hay_una_salida_que_satisface_la_necesidad_sin_capturar_el_dato()
    {
        // `RN-51`, caso límite: la persona que requiere ambulancia. El campo no se agrega al
        // manifiesto — se registra el REQUERIMIENTO OPERATIVO (camilla, acompañante) sin
        // consignar diagnóstico.
        var salida = ReglasDelCampoSensible.LaSalidaSinCapturarElDato;

        Assert.Contains("requerimiento operativo", salida);
        Assert.Contains("sin capturar el diagnóstico", salida);
    }
}
