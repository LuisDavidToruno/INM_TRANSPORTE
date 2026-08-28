using Sigti.Dominio.M09_Combustible;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// La conciliación galonaje–kilometraje — `RN-30`.
///
/// ── Lo que el auditor pregunta ───────────────────────────────────────────────
/// `NRM-01`, citado por la regla: <i>«el auditor no busca comprobantes, busca correlación entre
/// consumo, kilometraje y misión autorizada. Un sistema que solo archiva facturas no responde a
/// lo que se le va a preguntar»</i>.
///
/// ── Las dos direcciones, y por qué la segunda es la que importa ──────────────
/// Un control ingenuo busca consumo de más. `RN-30` exige también lo contrario: <b>un
/// rendimiento imposiblemente bueno casi siempre significa un despacho que no se registró</b>.
/// Los galones anotados no alcanzan a explicar los kilómetros porque el vehículo cargó de una
/// fuente que nadie apuntó.
/// </summary>
public class ReglasDeConciliacionPruebas
{
    private static readonly RendimientoEsperado Esperado =
        new(10m, OrigenDelRendimiento.Institucional, "RENDIMIENTO-2026-Q1");

    /// <summary>
    /// Asimétricos a propósito. `RN-30` punto 2: <i>«un exceso de consumo del 20% y un ahorro del
    /// 20% no significan lo mismo»</i> — un exceso puede ser montaña; un ahorro imposible casi
    /// siempre es un despacho sin registrar, y por eso se aprieta más.
    /// </summary>
    private static readonly UmbralesDeDesviacion Umbrales = new(0.20m, 0.15m);

    [Fact]
    public void El_rendimiento_dentro_de_umbral_no_es_hallazgo()
    {
        // 1,000 km con 105 galones = 9.52 km/gal contra 10 esperado: −4.8%.
        var c = ReglasDeConciliacion.Evaluar(1_000, 105m, Esperado, Umbrales);

        Assert.Equal(DictamenDeConciliacion.DentroDeUmbral, c.Dictamen);
        Assert.False(c.EsHallazgo);
        Assert.Equal(1_000, c.KilometrosRecorridos);
    }

    [Fact]
    public void Consumir_de_MAS_es_hallazgo_por_debajo_del_esperado()
    {
        // 1,000 km con 140 galones = 7.14 km/gal: −28.6%, fuera de la tolerancia inferior.
        var c = ReglasDeConciliacion.Evaluar(1_000, 140m, Esperado, Umbrales);

        Assert.Equal(DictamenDeConciliacion.ConsumoExcesivo, c.Dictamen);
        Assert.True(c.EsHallazgo);
        Assert.Contains("no imputable a esta misión", c.Evidencia);
    }

    [Fact]
    public void Consumir_de_MENOS_tambien_es_hallazgo_y_es_el_que_un_control_ingenuo_no_busca()
    {
        // 1,000 km con 80 galones = 12.5 km/gal: +25%, fuera de la tolerancia superior.
        //
        // **Este es el caso que motiva la regla.** Los galones registrados no alcanzan a
        // explicar los kilómetros: el vehículo cargó combustible que nadie anotó.
        var c = ReglasDeConciliacion.Evaluar(1_000, 80m, Esperado, Umbrales);

        Assert.Equal(DictamenDeConciliacion.RendimientoImposible, c.Dictamen);
        Assert.True(c.EsHallazgo);
        Assert.Contains("despacho de combustible que no se registró", c.Evidencia);
    }

    [Fact]
    public void Los_dos_umbrales_son_INDEPENDIENTES()
    {
        // El corazón de `RN-30` punto 2: «un umbral único simétrico es un error de diseño».
        // Con inferior 20% y superior 15%, una desviación de +18% es hallazgo y una de −18% no.
        var arriba = ReglasDeConciliacion.Evaluar(1_180, 100m, Esperado, Umbrales);
        var abajo = ReglasDeConciliacion.Evaluar(820, 100m, Esperado, Umbrales);

        Assert.Equal(DictamenDeConciliacion.RendimientoImposible, arriba.Dictamen);
        Assert.Equal(DictamenDeConciliacion.DentroDeUmbral, abajo.Dictamen);
    }

    [Fact]
    public void Justo_en_el_umbral_NO_es_hallazgo()
    {
        // El borde. Exactamente −20% con tolerancia inferior de 20%: la tolerancia incluye su
        // propio valor, o el parámetro significaría «hasta un poco menos de 20», que nadie lee así.
        var c = ReglasDeConciliacion.Evaluar(800, 100m, Esperado, Umbrales);

        Assert.Equal(DictamenDeConciliacion.DentroDeUmbral, c.Dictamen);
    }

    // ── Lo que NO se puede evaluar, y no se disfraza de conforme ────────────

    [Fact]
    public void Sin_rendimiento_esperado_NO_es_conforme_es_NO_EVALUABLE()
    {
        // La distinción que sostiene todo lo demás. Un control que devuelve «dentro de umbral»
        // cuando no pudo comparar nada es peor que no existir: además tranquiliza.
        var c = ReglasDeConciliacion.Evaluar(1_000, 100m, esperado: null, Umbrales);

        Assert.Equal(DictamenDeConciliacion.NoEvaluable, c.Dictamen);
        Assert.False(c.EsHallazgo);
        Assert.Null(c.RendimientoObservado);
        Assert.Contains("no hay contra qué comparar", c.Evidencia);
    }

    [Fact]
    public void Sin_umbrales_tampoco_se_evalua()
    {
        var c = ReglasDeConciliacion.Evaluar(1_000, 100m, Esperado, umbrales: null);

        Assert.Equal(DictamenDeConciliacion.NoEvaluable, c.Dictamen);
        Assert.Contains("las dos cosas son falsas", c.Evidencia);
    }

    [Fact]
    public void Una_mision_sin_carga_no_tiene_nada_que_conciliar()
    {
        // Salió con el tanque lleno y no cargó. No es un defecto del registro: es que esta
        // misión no tiene consumo, y dividir entre cero no produce un hallazgo, produce un error.
        var c = ReglasDeConciliacion.Evaluar(1_000, 0m, Esperado, Umbrales);

        Assert.Equal(DictamenDeConciliacion.NoEvaluable, c.Dictamen);
        Assert.Contains("no cargó combustible", c.Evidencia);
    }

    [Fact]
    public void Sin_kilometros_no_hay_recorrido_que_dividir()
    {
        // La misión que no se ejecutó y se liquidó igual por `T-16`. Su kilometraje es cero y
        // su rendimiento no significa nada.
        var c = ReglasDeConciliacion.Evaluar(0, 30m, Esperado, Umbrales);

        Assert.Equal(DictamenDeConciliacion.NoEvaluable, c.Dictamen);
    }

    // ── Los reparos: se calcula igual, y se conserva ────────────────────────

    [Fact]
    public void Con_el_odometro_averiado_el_calculo_se_CONSERVA_marcado_no_concluyente()
    {
        // `RN-30`: el cálculo no concluyente «se conserva para el análisis agregado, que sí es
        // válido». Descartarlo perdería el dato justo donde el patrón se ve.
        var c = ReglasDeConciliacion.Evaluar(
            1_000, 140m, Esperado, Umbrales, new ReparosDelCalculo(OdometroAveriado: true));

        Assert.Equal(DictamenDeConciliacion.NoConcluyente, c.Dictamen);
        Assert.False(c.EsHallazgo);

        // Y las cifras siguen ahí: no concluyente no es no calculado.
        Assert.NotNull(c.RendimientoObservado);
        Assert.NotNull(c.Desviacion);
        Assert.Contains("RN-90", c.Evidencia);
    }

    [Fact]
    public void El_nivel_de_tanque_dispar_tambien_lo_vuelve_no_concluyente()
    {
        var c = ReglasDeConciliacion.Evaluar(
            1_000, 140m, Esperado, Umbrales, new ReparosDelCalculo(NivelDeTanqueDispar: true));

        Assert.Equal(DictamenDeConciliacion.NoConcluyente, c.Dictamen);
        Assert.Contains("los galones consumidos no son los cargados", c.Evidencia);
    }

    [Fact]
    public void La_espera_prolongada_registrada_desactiva_el_hallazgo_de_consumo_excesivo()
    {
        // `RN-30`: el motor encendido en espera consume sin recorrer, y «una desviación con
        // espera prolongada registrada no produce hallazgo por sí sola. Sin esa medición, el
        // hallazgo sería infundado.»
        var c = ReglasDeConciliacion.Evaluar(
            1_000, 140m, Esperado, Umbrales,
            new ReparosDelCalculo(EsperaProlongadaRegistrada: true));

        Assert.Equal(DictamenDeConciliacion.NoConcluyente, c.Dictamen);
        Assert.Contains("consume sin recorrer", c.Evidencia);
    }

    [Fact]
    public void La_espera_prolongada_NO_desactiva_el_rendimiento_imposible()
    {
        // Esperar con el motor encendido explica gastar de MÁS. No explica gastar de menos:
        // un vehículo que consume parado no puede rendir mejor de lo posible, así que la
        // espera no es coartada para el despacho sin registrar.
        var c = ReglasDeConciliacion.Evaluar(
            1_000, 80m, Esperado, Umbrales,
            new ReparosDelCalculo(EsperaProlongadaRegistrada: true));

        Assert.Equal(DictamenDeConciliacion.RendimientoImposible, c.Dictamen);
        Assert.True(c.EsHallazgo);
    }

    // ── La propuesta del histórico ──────────────────────────────────────────

    [Fact]
    public void Con_suficiente_historico_el_sistema_PROPONE_el_esperado()
    {
        // `RN-30` punto 1 lo autoriza: «el sistema puede proponerlo a partir del histórico del
        // propio vehículo, marcando la propuesta como tal».
        var propuesta = ReglasDeConciliacion.ProponerDelHistorico(
        [
            (1_000, 100m), (800, 82m), (1_200, 118m), (600, 61m), (900, 90m),
        ]);

        Assert.NotNull(propuesta);
        Assert.Equal(OrigenDelRendimiento.PropuestoDelHistorico, propuesta.Origen);
        Assert.Contains("PROPUESTA", propuesta.Version);
    }

    [Fact]
    public void La_propuesta_pondera_por_kilometros_no_por_mision()
    {
        // Un viaje de 40 km no puede pesar lo mismo que uno de 900. Promediar rendimientos
        // haría que la media saliera del viaje corto, que es el menos representativo.
        var propuesta = ReglasDeConciliacion.ProponerDelHistorico(
        [
            (40, 10m),      // 4 km/gal — un viaje corto con mucho ralentí
            (900, 90m), (900, 90m), (900, 90m), (900, 90m),  // 10 km/gal
        ]);

        // Total: 3,640 km / 370 gal = 9.84. El promedio de rendimientos daría 8.8.
        Assert.NotNull(propuesta);
        Assert.True(propuesta.KmPorGalon > 9.5m,
            $"la media ponderada debería acercarse a 10 y dio {propuesta.KmPorGalon:N2}");
    }

    [Fact]
    public void Con_poco_historico_NO_se_propone_nada()
    {
        // Con dos misiones una carga atípica mueve la media entera, y la propuesta diría más de
        // lo que sabe. Devolver null deja la conciliación en «no evaluable», que es la verdad.
        Assert.Null(ReglasDeConciliacion.ProponerDelHistorico([(1_000, 100m), (800, 82m)]));
    }

    [Fact]
    public void El_historico_inservible_no_cuenta_para_el_minimo()
    {
        // Cinco misiones, pero tres sin galones: la media saldría de dos. Se descartan antes de
        // contar, no después.
        Assert.Null(ReglasDeConciliacion.ProponerDelHistorico(
        [
            (1_000, 100m), (800, 82m), (500, 0m), (400, 0m), (300, 0m),
        ]));
    }

    [Fact]
    public void El_origen_del_esperado_VIAJA_en_la_evidencia()
    {
        // Un dictamen calculado contra una propuesta y otro contra el valor institucional no
        // valen lo mismo, y sólo el segundo sostiene un hallazgo firme. Guardar sólo el número
        // los volvería indistinguibles dentro de dos años.
        var propuesta = new RendimientoEsperado(
            10m, OrigenDelRendimiento.PropuestoDelHistorico, "PROPUESTA-DEL-HISTORICO-7-MISIONES");

        var c = ReglasDeConciliacion.Evaluar(1_000, 105m, propuesta, Umbrales);

        Assert.Contains("PROPUESTA del propio histórico", c.Evidencia);
        Assert.Contains("PROPUESTA-DEL-HISTORICO-7-MISIONES", c.Evidencia);
    }

    [Fact]
    public void La_evidencia_lleva_las_cuentas_para_poder_rehacerlas()
    {
        // Una conciliación que no dice contra qué se juzgó no se puede rehacer, y `RN-30` se
        // evalúa contra el esperado vigente a la fecha del hecho.
        var c = ReglasDeConciliacion.Evaluar(1_000, 105m, Esperado, Umbrales);

        Assert.Contains("1,000 km", c.Evidencia);
        Assert.Contains("105.00 gal", c.Evidencia);
        Assert.Contains("RENDIMIENTO-2026-Q1", c.Evidencia);
    }
}
