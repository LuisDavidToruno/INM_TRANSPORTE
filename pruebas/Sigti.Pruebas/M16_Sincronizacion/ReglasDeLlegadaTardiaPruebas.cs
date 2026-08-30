using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M16_Sincronizacion;

namespace Sigti.Pruebas.M16_Sincronizacion;

/// <summary>
/// `HU-070` — el registro de campo que llega después del cierre.
///
/// <i>«Es el caso más frecuente y el que más tienta a implementar un descarte automático.»</i>
/// </summary>
public class ReglasDeLlegadaTardiaPruebas
{
    [Theory]
    [InlineData(EstadoDeMision.Cerrada)]
    [InlineData(EstadoDeMision.CerradaConHallazgo)]
    public void Lo_que_llega_a_una_mision_cerrada_abre_hallazgo_posterior(EstadoDeMision estado)
    {
        // ⚠️ **No se reabre.** Reabrir haría que «un reporte ya emitido cambie de contenido a
        // espaldas» de quien lo firmó. El hecho entra por su propio expediente.
        Assert.Equal(DestinoDeLoTardio.HallazgoPosterior,
                     ReglasDeLlegadaTardia.Resolver(estado));
    }

    [Fact]
    public void Cerrada_con_hallazgo_cuenta_igual_que_cerrada()
    {
        // Tener un hallazgo previo no vuelve editable el expediente: lo vuelve un expediente
        // cerrado con más historia. Tratarlo distinto abriría la puerta a reabrir por la vía de
        // acumular hallazgos.
        Assert.True(ReglasDeLlegadaTardia.EstaCerrada(EstadoDeMision.CerradaConHallazgo));
        Assert.True(ReglasDeLlegadaTardia.EstaCerrada(EstadoDeMision.Cerrada));
    }

    [Fact]
    public void Lo_que_llega_a_una_liquidada_va_a_la_cola()
    {
        // La cifra ya se emitió, pero el expediente no terminó. De la cola sale un asiento de
        // diferencia, que conserva la liquidación original íntegra.
        Assert.Equal(DestinoDeLoTardio.ColaDeConflictos,
                     ReglasDeLlegadaTardia.Resolver(EstadoDeMision.Liquidada));
    }

    [Theory]
    [InlineData(EstadoDeMision.EnRuta)]
    [InlineData(EstadoDeMision.Retornada)]
    [InlineData(EstadoDeMision.Despachada)]
    public void Lo_que_llega_a_una_mision_viva_es_una_divergencia_comun(EstadoDeMision estado)
    {
        Assert.Equal(DestinoDeLoTardio.ColaDeConflictos,
                     ReglasDeLlegadaTardia.Resolver(estado));
    }

    [Fact]
    public void Ninguna_mision_viva_se_declara_cerrada()
    {
        // El defecto que esta prueba impide: dar por cerrada una misión que sigue su curso
        // mandaría a hallazgo posterior hechos que se aplican sin problema — y llenaría la
        // auditoría de expedientes que no debieron abrirse.
        foreach (var estado in Enum.GetValues<EstadoDeMision>())
        {
            if (estado is EstadoDeMision.Cerrada or EstadoDeMision.CerradaConHallazgo) continue;
            Assert.False(ReglasDeLlegadaTardia.EstaCerrada(estado), $"{estado}");
        }
    }

    // ── Los dos mensajes, que son textuales en la historia ──────────────────

    [Fact]
    public void El_mensaje_de_la_liquidacion_dice_por_donde_se_sale()
    {
        // «La liquidación de OM-2026-0451 está cerrada. Registre un asiento de diferencia con
        // su motivo y su respaldo.»
        var m = ReglasDeLlegadaTardia.PorQueNoSeEditaLaLiquidacion("OM-2026-0451");

        Assert.Contains("OM-2026-0451", m);
        Assert.Contains("asiento de diferencia", m);
    }

    [Fact]
    public void El_mensaje_del_cierre_NOMBRA_el_expediente_que_se_abrio()
    {
        // «OM-2026-0430 está cerrada y no se reabre. Se abrió el expediente de hallazgo
        // posterior HP-2026-0012.»
        //
        // Sin ese dato, quien lo lee sabe que su registro no entró y no sabe dónde quedó — y
        // vuelve a enviarlo, o lo anota en papel.
        var m = ReglasDeLlegadaTardia.PorQueNoSeReabre("OM-2026-0430", "HP-2026-0012");

        Assert.Contains("no se reabre", m);
        Assert.Contains("HP-2026-0012", m);
    }
}
