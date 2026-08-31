using Sigti.Dominio.M13_Cierre;

namespace Sigti.Pruebas.M13_Cierre;

/// <summary>
/// §7.2 — la propuesta de clasificación al cerrar.
///
/// ── ⚠️ Lo que estas pruebas defienden ───────────────────────────────────────
/// <b>«No se cumple» y «nadie lo miró» no son lo mismo.</b> Con dos valores se vuelven
/// indistinguibles, y el expediente cierra `CERRADA` afirmando trece verificaciones de las que
/// hizo cinco. Es la disciplina de todo el sistema —nulo ≠ cero ≠ «no se preguntó»— en el acto
/// que vuelve el expediente inmutable.
///
/// Y la otra mitad: <b>un criterio sin verificar no produce hallazgo</b>. Sería declarar un
/// hallazgo que nadie constató, y `CERRADA_CON_HALLAZGO` es una marca sobre la conducta de la
/// institución.
/// </summary>
public class ReglasDeLaPropuestaDeCierrePruebas
{
    // ── El expediente limpio ────────────────────────────────────────────────

    [Fact]
    public void Sin_nada_que_reprochar_no_hay_hallazgo()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio());

        Assert.False(propuesta.HayHallazgo);
        Assert.Empty(propuesta.Cumplidos);
    }

    /// <summary>
    /// ⚠️ <b>Y aun así declara qué no miró.</b> Es justo en el expediente que cierra limpio
    /// donde ocultar los criterios sin verificar haría creer lo que no es.
    /// </summary>
    [Fact]
    public void El_expediente_limpio_declara_igual_lo_que_no_se_verifico()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio());

        Assert.NotEmpty(propuesta.SinVerificar);

        // Con lo que le falta a cada uno: un «no verificado» sin motivo es un hueco que nadie
        // va a poder cerrar porque nadie va a saber qué le falta.
        Assert.All(propuesta.SinVerificar,
            c => Assert.False(string.IsNullOrWhiteSpace(c.Detalle)));
    }

    /// <summary>
    /// <b>Un criterio sin verificar NO produce hallazgo.</b> Marcar el expediente por lo que el
    /// sistema todavía no sabe mirar acusaría a la institución de una conducta que nadie
    /// constató.
    /// </summary>
    [Fact]
    public void Lo_no_verificado_no_produce_hallazgo()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio());

        Assert.False(propuesta.HayHallazgo);
        Assert.DoesNotContain(propuesta.Cumplidos,
            c => c.Resultado == ResultadoDelCriterio.NoVerificado);
    }

    /// <summary>Los trece de §7.2 aparecen, se evalúen o no. Lo que falta se ve porque está.</summary>
    [Fact]
    public void Los_trece_criterios_aparecen_en_la_propuesta()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio());

        Assert.Equal(13, propuesta.Criterios.Count);

        Assert.Equal(
            Enumerable.Range(1, 13).Select(n => $"H-{n:00}"),
            propuesta.Criterios.Select(c => c.Criterio));
    }

    // ── `H-01` · la desviación de consumo ───────────────────────────────────

    /// <summary>
    /// Fuera de umbral <b>en cualquier dirección</b> — `RN-30`. Consumir de menos también es
    /// desviación: puede ser combustible que nunca entró al tanque.
    /// </summary>
    [Fact]
    public void Un_vale_con_desviacion_dispara_H01_y_nombra_el_folio()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { ValesConDesviacion = ["VC-2026-00081"] });

        var h01 = De(propuesta, "H-01");

        Assert.Equal(ResultadoDelCriterio.SeCumple, h01.Resultado);

        // El caso concreto, no «hay una desviación»: un hallazgo sin el hecho que lo produjo
        // no se puede seguir, y seguirlo es para lo que existe el estado.
        Assert.Contains("VC-2026-00081", h01.Detalle);
        Assert.True(propuesta.HayHallazgo);
    }

    // ── `H-03` · el cruce de peajes ─────────────────────────────────────────

    /// <summary>
    /// ⚠️ <b>El defecto que sólo se vio abriendo la pantalla.</b>
    ///
    /// `H-03` salía como <i>no verificado</i> diciendo que «todavía no hay quien juzgue», y M-18
    /// <b>ya juzga</b>. El expediente mostraba arriba <i>«peaje fuera de la ruta autorizada»</i>
    /// y, dos paneles más abajo, que nadie podía juzgar eso. Dos afirmaciones contradictorias en
    /// la misma pantalla, y la segunda era falsa.
    /// </summary>
    [Fact]
    public void Una_incoherencia_de_peajes_dispara_H03()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { Peajes = [new DictamenDePeajes(1, true, [])] });

        Assert.Equal(ResultadoDelCriterio.SeCumple, De(propuesta, "H-03").Resultado);
    }

    /// <summary>
    /// <b>Sin hallazgos no es lo mismo que coherente</b> — lo dice `RN-37`: <i>«un dictamen que
    /// no pudo mirar nada no es conformidad, es silencio»</i>. Un dictamen limpio que no miró la
    /// dimensión temporal <b>no verifica</b> `H-03`.
    /// </summary>
    [Fact]
    public void Un_dictamen_limpio_que_no_miro_todo_no_verifica_H03()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio() with
        {
            Peajes = [new DictamenDePeajes(0, false, ["velocidad_media_maxima no definida"])],
        });

        var h03 = De(propuesta, "H-03");

        Assert.Equal(ResultadoDelCriterio.NoVerificado, h03.Resultado);

        // Y dice cuál dimensión faltó, que es lo que alguien puede ir a cargar.
        Assert.Contains("velocidad_media_maxima", h03.Detalle);
    }

    [Fact]
    public void Un_dictamen_limpio_con_las_cuatro_dimensiones_si_verifica_H03()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { Peajes = [new DictamenDePeajes(0, true, [])] });

        Assert.Equal(ResultadoDelCriterio.NoSeCumple, De(propuesta, "H-03").Resultado);
    }

    /// <summary>
    /// <b>Cero pasos no es una secuencia coherente</b>: es que no hay secuencia. Afirmar
    /// conformidad sobre cero pasos es la clase de silencio que `RN-37` nombra.
    /// </summary>
    [Fact]
    public void Sin_pasos_registrados_H03_queda_sin_verificar()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio() with { Peajes = [] });

        Assert.Equal(ResultadoDelCriterio.NoVerificado, De(propuesta, "H-03").Resultado);
    }

    // ── `H-04` · el fondo que no volvió ─────────────────────────────────────

    /// <summary>
    /// ⚠️ <b>Nulo no es cero.</b> «No se consultó M-09» y «se consultó y no había ninguno» son
    /// cosas distintas, y tratarlas igual haría que una consulta que falló se viera exactamente
    /// como una misión sin fondos pendientes.
    /// </summary>
    [Fact]
    public void Sin_consultar_M09_el_criterio_del_fondo_queda_sin_verificar()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { FondoEntregadoSinDevolver = null });

        Assert.Equal(ResultadoDelCriterio.NoVerificado, De(propuesta, "H-04").Resultado);
        Assert.False(propuesta.HayHallazgo);
    }

    [Fact]
    public void Con_cero_fondos_pendientes_el_criterio_se_declara_verificado()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { FondoEntregadoSinDevolver = 0 });

        Assert.Equal(ResultadoDelCriterio.NoSeCumple, De(propuesta, "H-04").Resultado);
    }

    /// <summary>
    /// Un fondo que sigue entregado cuando el expediente se vuelve inmutable es <b>dinero
    /// público sin descargo</b>. No impide cerrar —`RN-86` deja la obligación viva en su propio
    /// expediente—, pero el expediente queda marcado.
    /// </summary>
    [Fact]
    public void El_fondo_entregado_sin_devolver_dispara_H04()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { FondoEntregadoSinDevolver = 2 });

        var h04 = De(propuesta, "H-04");

        Assert.Equal(ResultadoDelCriterio.SeCumple, h04.Resultado);
        Assert.Contains("RN-86", h04.Detalle);
    }

    // ── `H-05` · circular en franja inhábil ─────────────────────────────────

    /// <summary>
    /// ⚠️ <b>No es `BD-04` otra vez.</b> `BD-04` mira al despachar contra la ventana solicitada;
    /// esto mira al conciliar contra lo que efectivamente pasó — una prórroga que metió el
    /// sábado, un relevo que invalidó el permiso, o una salida que entró por sincronización.
    /// </summary>
    [Fact]
    public void Circular_en_dia_inhabil_sin_permiso_que_ampare_dispara_H05()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio() with
        {
            DiasInhabilesCirculados = [new DateOnly(2026, 4, 4)],
            AmparadaPorPermiso = false,
        });

        var h05 = De(propuesta, "H-05");

        Assert.Equal(ResultadoDelCriterio.SeCumple, h05.Resultado);
        Assert.Contains("2026-04-04", h05.Detalle);
    }

    /// <summary>Con permiso que ampara, se declara <b>verificado y limpio</b>, no «no aplica».</summary>
    [Fact]
    public void Circular_en_dia_inhabil_con_permiso_que_ampara_no_es_hallazgo()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio() with
        {
            DiasInhabilesCirculados = [new DateOnly(2026, 4, 4)],
            AmparadaPorPermiso = true,
        });

        Assert.Equal(ResultadoDelCriterio.NoSeCumple, De(propuesta, "H-05").Resultado);
    }

    /// <summary>
    /// Sin días inhábiles no hay nada que amparar, <b>y la falta de permiso no es hallazgo</b>:
    /// exigir permiso a una misión que circuló de martes a jueves marcaría expedientes limpios.
    /// </summary>
    [Fact]
    public void Sin_dias_inhabiles_la_falta_de_permiso_no_es_hallazgo()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { DiasInhabilesCirculados = [], AmparadaPorPermiso = false });

        Assert.Equal(ResultadoDelCriterio.NoSeCumple, De(propuesta, "H-05").Resultado);
        Assert.False(propuesta.HayHallazgo);
    }

    // ── `H-06` · el incidente sin desenlace ─────────────────────────────────

    [Fact]
    public void Un_incidente_sin_desenlace_dispara_H06()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { IncidentesSinResolver = ["INC-2026-0012"] });

        var h06 = De(propuesta, "H-06");

        Assert.Equal(ResultadoDelCriterio.SeCumple, h06.Resultado);
        Assert.Contains("INC-2026-0012", h06.Detalle);
    }

    /// <summary>La misma disciplina de `H-04`: no consultar M-12 no es no tener incidentes.</summary>
    [Fact]
    public void Sin_consultar_M12_el_criterio_del_incidente_queda_sin_verificar()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(
            Limpio() with { IncidentesSinResolver = null });

        Assert.Equal(ResultadoDelCriterio.NoVerificado, De(propuesta, "H-06").Resultado);
    }

    // ── Varios a la vez ─────────────────────────────────────────────────────

    /// <summary>
    /// <b>Se acumulan.</b> Un expediente con desviación de consumo y un incidente abierto tiene
    /// dos hallazgos, no uno: quien lo lea después necesita los dos hechos, y quedarse con el
    /// primero convierte el acta en media respuesta.
    /// </summary>
    [Fact]
    public void Los_criterios_cumplidos_se_acumulan()
    {
        var propuesta = ReglasDeLaPropuestaDeCierre.Evaluar(Limpio() with
        {
            ValesConDesviacion = ["VC-2026-00081"],
            IncidentesSinResolver = ["INC-2026-0012"],
        });

        Assert.Equal(2, propuesta.Cumplidos.Count);
        Assert.Contains(propuesta.Cumplidos, c => c.Criterio == "H-01");
        Assert.Contains(propuesta.Cumplidos, c => c.Criterio == "H-06");
    }

    // ── Andamios ────────────────────────────────────────────────────────────

    /// <summary>
    /// Un expediente sin nada que reprochar y <b>con las dos consultas hechas</b>: los ceros son
    /// ceros, no ausencias.
    /// </summary>
    private static HechosDelCierre Limpio() => new(
        ValesConDesviacion: [],
        FondoEntregadoSinDevolver: 0,
        DiasInhabilesCirculados: [],
        AmparadaPorPermiso: false,
        IncidentesSinResolver: [],

        // Un dictamen de peajes que miró las cuatro dimensiones y no encontró nada. Sin las
        // cuatro, `H-03` quedaría sin verificar y las pruebas de los otros criterios estarían
        // midiendo un reporte a medias.
        Peajes: [new DictamenDePeajes(0, true, [])]);

    private static CriterioEvaluado De(PropuestaDeCierre propuesta, string criterio) =>
        propuesta.Criterios.Single(c => c.Criterio == criterio);
}
