using Sigti.Dominio.M09_Combustible;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// El remanente en tanque — `RN-83` punto 3 y `CE-07`.
///
/// ── La fórmula, tal como la escribe `CE-07` ──────────────────────────────────
/// <c>consumido por la misión = entregado − devuelto en vales − remanente en tanque
/// atribuible</c>. Sin esa resta, un vehículo que vuelve con el tanque servido aparece
/// consumiendo de más, y `RN-30` lo marca como desviación — de un combustible que sigue en el
/// tanque, a la vista de cualquiera que abra la tapa.
///
/// ── Y lo que el caso prohíbe ─────────────────────────────────────────────────
/// <i>«Lo que no puede pasar es que un tanque lleno pagado con fondo de esta misión desaparezca
/// del expediente.»</i>
/// </summary>
public class ReglasDelRemanentePruebas
{
    private static NivelDeTanque Fraccion(decimal valor) =>
        new(EscalaDeNivel.FraccionDelIndicador, valor);

    private static NivelDeTanque Galones(decimal valor) =>
        new(EscalaDeNivel.Galones, valor);

    [Fact]
    public void Volver_con_mas_de_lo_que_salio_deja_remanente_positivo()
    {
        // Cargó de más: parte de esos galones no los gastó esta misión, están en el tanque
        // esperando a la siguiente.
        var r = ReglasDelRemanente.Calcular(Galones(10m), Galones(35m), null);

        Assert.Equal(25m, r.Galones);
        Assert.Contains("no los gastó esta misión", r.Explicacion);
    }

    [Fact]
    public void Salir_lleno_y_volver_vacio_da_remanente_NEGATIVO()
    {
        // El caso que `RN-30` nombra: «los galones consumidos exceden a los cargados». La
        // misión quemó combustible que ya estaba en el tanque al salir, y sin esta resta el
        // rendimiento saldría mejor de lo que fue.
        var r = ReglasDelRemanente.Calcular(Galones(40m), Galones(5m), null);

        Assert.Equal(-35m, r.Galones);
        Assert.Contains("ya estaba en el tanque", r.Explicacion);
    }

    [Fact]
    public void Volver_al_mismo_nivel_da_cero_y_lo_dice()
    {
        var r = ReglasDelRemanente.Calcular(Galones(20m), Galones(20m), null);

        Assert.Equal(0m, r.Galones);
        Assert.True(r.EsCalculable);
        Assert.Contains("mismo nivel", r.Explicacion);
    }

    // ── Lo que la misión quemó ──────────────────────────────────────────────

    [Fact]
    public void El_consumo_de_la_mision_resta_el_remanente()
    {
        // Abasteció 60 y volvió con 25 de más: quemó 35. Usar los 60 haría que el rendimiento
        // apareciera un 70% peor de lo que fue.
        var remanente = ReglasDelRemanente.Calcular(Galones(10m), Galones(35m), null);

        Assert.Equal(35m, ReglasDelRemanente.ConsumidoPorLaMision(60m, remanente));
    }

    [Fact]
    public void Con_remanente_negativo_el_consumo_SUPERA_lo_abastecido()
    {
        var remanente = ReglasDelRemanente.Calcular(Galones(40m), Galones(5m), null);

        // Abasteció 20 y además quemó 35 que llevaba: 55.
        Assert.Equal(55m, ReglasDelRemanente.ConsumidoPorLaMision(20m, remanente));
    }

    [Fact]
    public void Sin_remanente_calculable_el_consumo_es_lo_abastecido_y_es_lo_mejor_que_se_puede_afirmar()
    {
        // No es lo mismo que decir que el remanente fue cero. La diferencia queda dicha en la
        // explicación, y quien concilia decide si el número le sirve.
        var remanente = ReglasDelRemanente.Calcular(null, Galones(20m), null);

        Assert.Equal(60m, ReglasDelRemanente.ConsumidoPorLaMision(60m, remanente));
        Assert.False(remanente.EsCalculable);
    }

    // ── Lo que NO se puede calcular, y no se estima ─────────────────────────

    [Fact]
    public void Sin_una_de_las_dos_lecturas_no_hay_diferencia_que_medir()
    {
        // `RN-80`: el campo no consignado se declara y no se estima. Un remanente inventado
        // entra directo al denominador del rendimiento y después nadie lo distingue de uno
        // medido.
        var r = ReglasDelRemanente.Calcular(Galones(10m), null, 60m);

        Assert.Null(r.Galones);
        Assert.Contains("prohíbe estimarlo", r.Explicacion);
    }

    [Fact]
    public void Escalas_distintas_no_se_restan()
    {
        var r = ReglasDelRemanente.Calcular(Fraccion(1m), Galones(15m), 60m);

        Assert.Null(r.Galones);
        Assert.Contains("escalas distintas", r.Explicacion);
    }

    [Fact]
    public void En_FRACCION_hace_falta_la_capacidad_del_tanque()
    {
        // «Un octavo no es una cantidad hasta saber de qué tanque.» La ficha técnica puede no
        // declararla, y ahí el remanente no se calcula.
        var r = ReglasDelRemanente.Calcular(Fraccion(0.25m), Fraccion(0.75m), null);

        Assert.Null(r.Galones);
        Assert.Contains("no declara la capacidad del tanque", r.Explicacion);
    }

    [Fact]
    public void Con_la_capacidad_declarada_la_fraccion_SI_se_convierte()
    {
        // Medio tanque de sesenta galones son treinta.
        var r = ReglasDelRemanente.Calcular(Fraccion(0.25m), Fraccion(0.75m), 60m);

        Assert.Equal(30m, r.Galones);
        Assert.Contains("tanque de 60 galones", r.Explicacion);
    }

    [Fact]
    public void Una_capacidad_en_cero_NO_se_usa_para_convertir()
    {
        // Cero es un dato mal cargado, no un tanque sin volumen. Usarlo daría un remanente de
        // cero galones sobre cualquier fracción, y eso se leería como «el tanque no se movió».
        var r = ReglasDelRemanente.Calcular(Fraccion(0.25m), Fraccion(0.75m), 0m);

        Assert.Null(r.Galones);
    }

    [Fact]
    public void La_explicacion_va_SIEMPRE_haya_o_no_remanente()
    {
        // Un remanente ausente sin razón se lee como un tanque que no se movió, y son cosas
        // distintas.
        foreach (var r in new[]
        {
            ReglasDelRemanente.Calcular(Galones(10m), Galones(35m), null),
            ReglasDelRemanente.Calcular(null, null, null),
            ReglasDelRemanente.Calcular(Fraccion(1m), Fraccion(0.5m), null),
        })
        {
            Assert.False(string.IsNullOrWhiteSpace(r.Explicacion));
        }
    }
}
