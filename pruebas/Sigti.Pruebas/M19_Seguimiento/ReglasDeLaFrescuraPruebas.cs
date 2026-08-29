using Sigti.Dominio.M19_Seguimiento;

namespace Sigti.Pruebas.M19_Seguimiento;

/// <summary>
/// `HU-057` — la antigüedad del dato, y las cuatro formas de no tenerla.
///
/// <i>«Un tablero que muestra una posición de hace once horas como si fuera de ahora es peor que
/// un tablero vacío: produce decisiones seguras sobre información falsa»</i>.
/// </summary>
public class ReglasDeLaFrescuraPruebas
{
    private static readonly DateTimeOffset Ahora = new(2026, 5, 14, 18, 0, 0, TimeSpan.Zero);

    // ── El escenario textual de la historia ─────────────────────────────────

    [Fact]
    public void La_antiguedad_se_dice_en_horas_y_minutos_como_pide_la_historia()
    {
        // `HU-057`: última posición a las 07:20, consulta a las 18:00 → «hace 10 horas 40 minutos».
        var hecho = new DateTimeOffset(2026, 5, 14, 7, 20, 0, TimeSpan.Zero);

        var f = ReglasDeLaFrescura.Evaluar(hecho, Ahora, TimeSpan.FromHours(4));

        Assert.Equal(TimeSpan.FromMinutes(640), f.Antiguedad);
        Assert.Contains("10 horas 40 minutos", ReglasDeLaFrescura.EnPalabras(f.Antiguedad!.Value));
    }

    [Fact]
    public void Tres_dias_de_silencio_degradan_el_dato_y_no_lo_declaran_anomalia()
    {
        var hecho = Ahora.AddDays(-3);

        var f = ReglasDeLaFrescura.Evaluar(hecho, Ahora, TimeSpan.FromHours(12));

        Assert.Equal(GradoDeFrescura.Degradado, f.Grado);

        // El silencio es la condición esperada, no una anomalía. La frase tiene que decirlo:
        // un tablero que grita cada vez que un vehículo entra a una zona sin cobertura se
        // deja de mirar, y con él se dejan de ver las alarmas verdaderas.
        Assert.Contains("esperable", f.PorQue);
        Assert.DoesNotContain("alerta", f.PorQue, StringComparison.OrdinalIgnoreCase);
    }

    // ── Las cuatro formas de no tener el dato ───────────────────────────────

    [Fact]
    public void Nunca_haber_declarado_no_es_haber_declarado_hace_mucho()
    {
        var f = ReglasDeLaFrescura.Evaluar(null, Ahora, TimeSpan.FromHours(12));

        Assert.Equal(GradoDeFrescura.NuncaHuboDato, f.Grado);

        // Nulo, no TimeSpan.Zero ni una antigüedad enorme: las dos mentirían. Cero diría
        // «acabamos de saber de él»; una antigüedad grande diría «declaró hace mucho», y no
        // declaró nunca.
        Assert.Null(f.Antiguedad);
    }

    [Fact]
    public void Sin_umbral_fijado_se_muestra_la_antiguedad_y_no_se_clasifica()
    {
        // El umbral es `[C]`, insumo #68. Cablear doce horas produciría un tablero que degrada
        // según un número que nadie decidió.
        var f = ReglasDeLaFrescura.Evaluar(Ahora.AddHours(-30), Ahora, umbral: null);

        Assert.Equal(GradoDeFrescura.NoSeClasifica, f.Grado);

        // Lo duro de `HU-057` se cumple igual: la antigüedad se muestra siempre.
        Assert.Equal(TimeSpan.FromHours(30), f.Antiguedad);
        Assert.Contains("1 día 6 horas", f.PorQue);
        Assert.Contains("#68", f.PorQue);
    }

    [Fact]
    public void El_reloj_adelantado_no_se_aplasta_a_cero()
    {
        // El defecto que esta prueba impide: `Math.Max(0, ahora - hecho)` haría que el
        // dispositivo con el reloj roto apareciera como el dato MÁS fresco del tablero —
        // justamente el menos confiable, presentado como el mejor.
        var f = ReglasDeLaFrescura.Evaluar(Ahora.AddHours(5), Ahora, TimeSpan.FromHours(12));

        Assert.Equal(GradoDeFrescura.RelojAdelantado, f.Grado);
        Assert.True(f.Antiguedad < TimeSpan.Zero);
        Assert.NotEqual(GradoDeFrescura.Fresco, f.Grado);
        Assert.Contains("futuro", f.PorQue);
    }

    [Fact]
    public void Dentro_del_umbral_es_fresco()
    {
        var f = ReglasDeLaFrescura.Evaluar(Ahora.AddHours(-2), Ahora, TimeSpan.FromHours(12));

        Assert.Equal(GradoDeFrescura.Fresco, f.Grado);
        Assert.Contains("2 horas", f.PorQue);
    }

    [Fact]
    public void El_umbral_exacto_todavia_es_fresco()
    {
        // Un límite excluyente haría que el dato degradara un segundo antes de lo que la
        // institución fijó, y nadie podría explicar por qué.
        var f = ReglasDeLaFrescura.Evaluar(Ahora.AddHours(-12), Ahora, TimeSpan.FromHours(12));

        Assert.Equal(GradoDeFrescura.Fresco, f.Grado);
    }

    // ── La forma de decirlo ─────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 0, 30, "30 minutos")]
    [InlineData(0, 1, 0, "1 hora")]
    [InlineData(0, 10, 40, "10 horas 40 minutos")]
    [InlineData(3, 2, 15, "3 días 2 horas")]
    [InlineData(1, 0, 0, "1 día")]
    public void Las_palabras_no_redondean_lo_que_se_queria_ver(
        int dias, int horas, int minutos, string esperado)
    {
        // «Hace un día» borra la diferencia entre veintiséis horas y cuarenta y siete, que es
        // justo la diferencia que el Jefe de Transporte necesita.
        Assert.Equal(esperado,
            ReglasDeLaFrescura.EnPalabras(new TimeSpan(dias, horas, minutos, 0)));
    }

    [Fact]
    public void Menos_de_un_minuto_se_dice_asi_y_no_como_cero()
    {
        Assert.Equal("menos de un minuto",
            ReglasDeLaFrescura.EnPalabras(TimeSpan.FromSeconds(20)));
    }
}
