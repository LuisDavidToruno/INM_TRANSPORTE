using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// `RN-95` punto 1 y punto 5 — el catálogo de fuentes y el retraso como dato visible.
///
/// <i>«Una fuente sin conciliar durante meses es <b>en sí misma</b> una observación de control
/// interno»</i>.
/// </summary>
public class ReglasDeFuenteExternaPruebas
{
    private static readonly DateOnly Hoy = new(2026, 8, 29);

    private static FuenteExterna Fuente(
        bool disponible = true,
        int? periodicidad = 30,
        DateOnly? ultima = null,
        string? porQueNo = null) =>
        new(Ulid.NewUlid(), TipoDeFuenteExterna.EstadoDeCuentaDeCombustible,
            "Distribuidora de combustible", "CSV", "P-COMBUSTIBLE",
            disponible, periodicidad, ultima, porQueNo);

    // ── El retraso ──────────────────────────────────────────────────────────

    [Fact]
    public void El_retraso_se_dice_con_las_palabras_de_la_regla()
    {
        // `RN-95` punto 5, textual: «Estado de cuenta de combustible — última conciliación hace
        // 97 días».
        var f = Fuente(ultima: new DateOnly(2026, 5, 24));

        Assert.Equal(97, f.DiasDesdeLaUltima(Hoy));
        Assert.Contains("hace 97 día(s)", f.Retraso(Hoy));
        Assert.Contains("ATRASADA", f.Retraso(Hoy));
        Assert.True(f.Atrasada(Hoy));
    }

    [Fact]
    public void Dentro_de_la_periodicidad_no_esta_atrasada()
    {
        var f = Fuente(ultima: new DateOnly(2026, 8, 15));

        Assert.False(f.Atrasada(Hoy));
        Assert.DoesNotContain("ATRASADA", f.Retraso(Hoy));
    }

    [Fact]
    public void NUNCA_conciliada_no_es_cero_dias_de_retraso()
    {
        // De una fuente que nadie miró no se puede decir que lleva N días: se puede decir que
        // nunca. Y es el peor caso, no la ausencia de caso.
        var f = Fuente(ultima: null);

        Assert.Null(f.DiasDesdeLaUltima(Hoy));
        Assert.True(f.Atrasada(Hoy));
        Assert.Contains("NUNCA se ha conciliado", f.Retraso(Hoy));
    }

    [Fact]
    public void Sin_periodicidad_declarada_no_se_puede_llamar_atrasada()
    {
        // `[C]`. Es la misma disciplina del plazo de `RN-86`: sin el parámetro, el retraso se
        // mide pero no se juzga.
        var f = Fuente(periodicidad: null, ultima: new DateOnly(2026, 5, 24));

        Assert.Equal(97, f.DiasDesdeLaUltima(Hoy));
        Assert.False(f.Atrasada(Hoy));
        Assert.Contains("no está declarada", f.Retraso(Hoy));
    }

    [Fact]
    public void Una_ultima_conciliacion_en_el_FUTURO_no_pasa_por_al_dia()
    {
        // Salió al probar contra la base de desarrollo: el texto decía «hace -7 días», que no
        // describe nada. Una fecha posterior a hoy no es una conciliación reciente — es un dato
        // mal capturado o un reloj que no es el que se cree, y de esa fuente no se sabe nada.
        var f = Fuente(ultima: new DateOnly(2026, 9, 5));

        Assert.True(f.Atrasada(Hoy));
        Assert.Contains("posterior a hoy", f.Retraso(Hoy));
        Assert.DoesNotContain("hace -", f.Retraso(Hoy));
    }

    // ── No disponible NO es conciliada ──────────────────────────────────────

    [Fact]
    public void Una_fuente_NO_DISPONIBLE_lo_dice_y_no_cuenta_como_atrasada()
    {
        // Una institución sin tag de peaje no tiene estado de cuenta que conciliar. Marcarla
        // atrasada la pondría en la lista de pendientes para siempre, y a los tres meses la
        // lista deja de mirarse.
        var f = Fuente(disponible: false, ultima: null,
            porQueNo: "La institución no tiene tag CoviPass.");

        Assert.False(f.Atrasada(Hoy));
        Assert.Contains("No disponible NO es conciliada", f.Retraso(Hoy));
        Assert.Contains("no tiene tag CoviPass", f.Retraso(Hoy));
    }

    // ── El catálogo ─────────────────────────────────────────────────────────

    [Fact]
    public void La_fuente_exige_emisor()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeFuenteExterna.ExigirDatosDelCatalogo("  ", "P-COMBUSTIBLE", true, null));

        Assert.Contains("a quién reclamarle una diferencia", error.Message);
    }

    [Fact]
    public void La_fuente_exige_RESPONSABLE_de_la_carga()
    {
        // Una fuente sin responsable es una fuente que nadie carga, y a los tres meses la
        // conciliación existe en el papel y no en la práctica.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeFuenteExterna.ExigirDatosDelCatalogo("Proveedor", "   ", true, null));

        Assert.Contains("en el papel y no en la práctica", error.Message);
    }

    [Fact]
    public void Declararla_no_disponible_exige_decir_POR_QUE()
    {
        // Sin la razón, «no disponible» y «conciliada sin diferencias» se leen igual en el
        // reporte.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeFuenteExterna.ExigirDatosDelCatalogo("COVI-H", "P-COMBUSTIBLE", false, null));

        Assert.Contains("las dos se leen igual", error.Message);

        ReglasDeFuenteExterna.ExigirDatosDelCatalogo(
            "COVI-H", "P-COMBUSTIBLE", false, "La institución no tiene tag.");
    }
}
