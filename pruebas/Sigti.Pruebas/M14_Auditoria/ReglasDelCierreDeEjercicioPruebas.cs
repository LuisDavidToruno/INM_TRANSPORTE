using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// `RN-96` — el cierre de ejercicio como corte de imputación y de reporte.
///
/// ── Lo que estas pruebas cuidan ─────────────────────────────────────────────
/// Que el cierre <b>no cierre nada</b>. `RN-96` nombra el riesgo: <i>«sin esta regla escrita la
/// primera implementación va a poner un cierre masivo por fecha, porque es lo que resuelve ese
/// problema»</i>. Y ante el Tribunal Superior de Cuentas, <i>«cincuenta expedientes cerrados el
/// 31 de diciembre a la misma hora con el mismo motivo <b>son el hallazgo</b>, no su
/// solución»</i>.
/// </summary>
public class ReglasDelCierreDeEjercicioPruebas
{
    private static readonly DateOnly CorteLegal = new(2026, 12, 31);
    private static readonly DateOnly CorteOperativo = new(2027, 1, 15);

    private static readonly Ulid MisionA = Ulid.NewUlid();
    private static readonly Ulid MisionB = Ulid.NewUlid();
    private static readonly Ulid MisionC = Ulid.NewUlid();

    private static DateTimeOffset El(int dia, int hora = 10, int minuto = 0) =>
        new(2026, 12, dia, hora, minuto, 0, TimeSpan.FromHours(-6));

    // ── Las dos fechas de corte ─────────────────────────────────────────────

    [Fact]
    public void El_corte_operativo_anterior_al_legal_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelCierreDeEjercicio.ExigirCortes(
                "AC-2026-001", "2026", new DateOnly(2026, 12, 31), new DateOnly(2026, 12, 20)));

        Assert.Equal("RN-96", error.Precondicion);
        Assert.Contains("sin ejercicio al que imputarse", error.Message);
    }

    [Fact]
    public void El_acta_sin_folio_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelCierreDeEjercicio.ExigirCortes("  ", "2026", CorteLegal, CorteOperativo));

        Assert.Contains("no tiene a qué acta corresponder", error.Message);
    }

    [Fact]
    public void El_acta_sin_ejercicio_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelCierreDeEjercicio.ExigirCortes("AC-2026-001", "", CorteLegal, CorteOperativo));

    [Fact]
    public void Cortes_coherentes_pasan() =>
        ReglasDelCierreDeEjercicio.ExigirCortes(
            "AC-2026-001", "2026", CorteLegal, CorteOperativo);

    /// <summary>
    /// El mismo día en los dos cortes es válido: hay instituciones que no dan ventana operativa.
    /// Rechazarlo obligaría a inventar un día que la norma no da.
    /// </summary>
    [Fact]
    public void Los_dos_cortes_el_mismo_dia_pasan() =>
        ReglasDelCierreDeEjercicio.ExigirCortes(
            "AC-2026-001", "2026", CorteLegal, CorteLegal);

    // ── La imputación va por la fecha del hecho ─────────────────────────────

    /// <summary>
    /// `RN-40` y `RN-46`. Un consumo del 28 de diciembre es del ejercicio que cerró aunque la
    /// misión se liquide en marzo.
    /// </summary>
    [Fact]
    public void El_hecho_del_28_de_diciembre_es_del_ejercicio_que_cerro()
    {
        Assert.Equal("2026", ReglasDelCierreDeEjercicio.EjercicioDe(new DateOnly(2026, 12, 28)));
        Assert.Equal("2027", ReglasDelCierreDeEjercicio.EjercicioDe(new DateOnly(2027, 1, 2)));
    }

    /// <summary>
    /// `RN-96`: <i>«la Orden de Misión que cruza el corte no se divide»</i>. Lo que se reparte
    /// entre ejercicios son sus hechos.
    /// </summary>
    [Fact]
    public void La_mision_que_cruza_reparte_sus_hechos_y_no_se_parte()
    {
        var mision = new MisionQueCruza(
            MisionA, "OM-2026-0044", new DateOnly(2026, 12, 28), new DateOnly(2027, 1, 3),
            [
                new HechoImputado("2026", new DateOnly(2026, 12, 28), "Combustible", 1_200m, "Comprobante 88"),
                new HechoImputado("2026", new DateOnly(2026, 12, 29), "Peaje", 80m, "Tarifa vigente"),
                new HechoImputado("2027", new DateOnly(2027, 1, 2), "Combustible", 900m, "Comprobante 91"),
            ]);

        Assert.Equal(1_280m, mision.PorEjercicio["2026"]);
        Assert.Equal(900m, mision.PorEjercicio["2027"]);
        Assert.Empty(mision.SinTablaParametrica);
    }

    /// <summary>
    /// `RN-96` punto 4 exige la tabla paramétrica <b>de cada hecho</b>, para que el cálculo sea
    /// reproducible. El que no la trae va nombrado, no escondido.
    /// </summary>
    [Fact]
    public void El_hecho_sin_tabla_parametrica_queda_nombrado()
    {
        var mision = new MisionQueCruza(
            MisionA, "OM-2026-0044", new DateOnly(2026, 12, 28), new DateOnly(2027, 1, 3),
            [
                new HechoImputado("2026", new DateOnly(2026, 12, 28), "Combustible", 1_200m),
                new HechoImputado("2027", new DateOnly(2027, 1, 2), "Peaje", 80m, "Tarifa vigente"),
            ]);

        var sinTabla = Assert.Single(mision.SinTablaParametrica);
        Assert.Equal("Combustible", sinTabla.Concepto);
    }

    // ── Nunca un motivo compartido por varios expedientes ───────────────────

    [Fact]
    public void Dos_misiones_cerradas_con_el_mismo_motivo_salen_como_hallazgo()
    {
        var compartidos = ReglasDelCierreDeEjercicio.DetectarMotivosCompartidos(
        [
            (MisionA, "Cierre de ejercicio fiscal 2026", El(31, 16, 40)),
            (MisionB, "Cierre de ejercicio fiscal 2026", El(31, 16, 41)),
            (MisionC, "Bitácora conciliada, kilometraje coherente, sin desviación", El(20)),
        ]);

        var compartido = Assert.Single(compartidos);
        Assert.Equal(2, compartido.Misiones.Count);
        Assert.Contains(MisionA, compartido.Misiones);
        Assert.Contains(MisionB, compartido.Misiones);

        // Un minuto de diferencia es lo que delata el cierre en bloque. La ventana va en el
        // hallazgo justamente para poder distinguirlo de un motivo que se repite en el año.
        Assert.Equal(TimeSpan.FromMinutes(1), compartido.Ventana);
    }

    /// <summary>
    /// Quien cierra cincuenta expedientes copiando y pegando no escribe distinto cada vez, pero
    /// tampoco escribe idéntico: cambia una mayúscula, mete un espacio de más.
    /// </summary>
    [Fact]
    public void El_mismo_motivo_con_otra_caja_y_espacios_de_mas_sigue_siendo_el_mismo()
    {
        var compartidos = ReglasDelCierreDeEjercicio.DetectarMotivosCompartidos(
        [
            (MisionA, "Cierre de ejercicio", El(31, 16, 40)),
            (MisionB, "CIERRE  DE   EJERCICIO", El(31, 16, 41)),
        ]);

        Assert.Single(compartidos);
    }

    /// <summary>
    /// <b>Lo que no se hace es buscar parecidos.</b> Dos motivos que dicen lo mismo con otras
    /// palabras son dos evaluaciones distintas, y presumir lo contrario acusaría a quien sí
    /// evaluó cada expediente.
    /// </summary>
    [Fact]
    public void Dos_motivos_que_dicen_lo_mismo_con_otras_palabras_NO_se_agrupan()
    {
        var compartidos = ReglasDelCierreDeEjercicio.DetectarMotivosCompartidos(
        [
            (MisionA, "Sin desviación de kilometraje ni de galonaje", El(28)),
            (MisionB, "Kilometraje y galonaje sin desviaciones", El(29)),
        ]);

        Assert.Empty(compartidos);
    }

    /// <summary>
    /// La misma misión con dos asientos de cierre —un cierre y su cierre con hallazgo posterior—
    /// <b>no es un motivo compartido</b>: es un solo expediente. Contarlo produciría un hallazgo
    /// donde no hay ninguno.
    /// </summary>
    [Fact]
    public void Una_sola_mision_con_dos_asientos_no_es_motivo_compartido()
    {
        var compartidos = ReglasDelCierreDeEjercicio.DetectarMotivosCompartidos(
        [
            (MisionA, "Cierre de ejercicio", El(30)),
            (MisionA, "Cierre de ejercicio", El(31)),
        ]);

        Assert.Empty(compartidos);
    }

    /// <summary>
    /// El motivo vacío no agrupa. Un expediente cerrado sin motivo es otro problema —`RN-08`—, y
    /// juntarlos todos bajo «(vacío)» taparía tanto uno como el otro.
    /// </summary>
    [Fact]
    public void Los_motivos_vacios_no_se_agrupan_entre_si()
    {
        var compartidos = ReglasDelCierreDeEjercicio.DetectarMotivosCompartidos(
        [
            (MisionA, "", El(30)),
            (MisionB, "   ", El(31)),
        ]);

        Assert.Empty(compartidos);
    }

    // ── Las fechas de corte son parámetros con vigencia ─────────────────────

    /// <summary>
    /// `RN-96` las declara configurables. El legal se guarda como <b>día y mes</b> porque el
    /// parámetro rige para todos los ejercicios; el operativo como <b>días después</b> porque
    /// cae en el año siguiente y un «01-15» tendría que adivinar a cuál.
    /// </summary>
    [Fact]
    public void Los_cortes_salen_de_los_parametros_y_declaran_su_origen()
    {
        var (cortes, sin) = ReglasDelCierreDeEjercicio.CortesDe(
            "2026", "12-31", new DateOnly(2024, 1, 1), "15", new DateOnly(2024, 1, 1));

        Assert.Null(sin);
        Assert.Equal(new DateOnly(2026, 12, 31), cortes!.Legal);
        Assert.Equal(new DateOnly(2027, 1, 15), cortes.Operativo);
        Assert.Contains("12-31", cortes.Origen);
        Assert.Contains("15 días", cortes.Origen);
    }

    /// <summary>
    /// El mismo parámetro sirve para cualquier ejercicio. Eso es lo que se gana guardando día y
    /// mes en vez de la fecha completa: nadie tiene que cargar una versión cada enero.
    /// </summary>
    [Fact]
    public void El_mismo_parametro_fecha_cualquier_ejercicio()
    {
        var (a, _) = ReglasDelCierreDeEjercicio.CortesDe("2026", "12-31", null, "15", null);
        var (b, _) = ReglasDelCierreDeEjercicio.CortesDe("2031", "12-31", null, "15", null);

        Assert.Equal(new DateOnly(2026, 12, 31), a!.Legal);
        Assert.Equal(new DateOnly(2031, 12, 31), b!.Legal);
    }

    /// <summary>
    /// Una institución que cierra a mitad de año no rompe nada: el operativo se cuenta desde el
    /// legal, sin adivinar el año.
    /// </summary>
    [Fact]
    public void Un_corte_a_mitad_de_anio_tambien_resuelve()
    {
        var (cortes, _) = ReglasDelCierreDeEjercicio.CortesDe("2026", "06-30", null, "10", null);

        Assert.Equal(new DateOnly(2026, 6, 30), cortes!.Legal);
        Assert.Equal(new DateOnly(2026, 7, 10), cortes.Operativo);
    }

    /// <summary>
    /// Cero días es válido: hay instituciones sin ventana operativa. Rechazarlo obligaría a
    /// inventar un día que la norma no da.
    /// </summary>
    [Fact]
    public void Cero_dias_despues_deja_los_dos_cortes_el_mismo_dia()
    {
        var (cortes, _) = ReglasDelCierreDeEjercicio.CortesDe("2026", "12-31", null, "0", null);

        Assert.Equal(cortes!.Legal, cortes.Operativo);
    }

    [Fact]
    public void Sin_el_dia_y_mes_cargado_NO_hay_corte_por_omision()
    {
        var (cortes, sin) = ReglasDelCierreDeEjercicio.CortesDe("2026", null, null, "15", null);

        Assert.Null(cortes);
        Assert.Equal("cierre.corte_legal_dia_y_mes", sin!.Clave);
    }

    [Fact]
    public void Sin_los_dias_del_operativo_tampoco()
    {
        var (cortes, sin) = ReglasDelCierreDeEjercicio.CortesDe("2026", "12-31", null, null, null);

        Assert.Null(cortes);
        Assert.Equal("cierre.corte_operativo_dias_despues", sin!.Clave);
    }

    /// <summary>
    /// <b>El 29 de febrero de un año no bisiesto se rechaza, no se corre al 28.</b>
    ///
    /// Correrlo movería el corte un día sin que nadie lo decidiera, y ese día tiene hechos: los
    /// del 28 pasarían al ejercicio siguiente o se quedarían, según hacia dónde se corriera.
    /// </summary>
    [Fact]
    public void El_29_de_febrero_de_un_anio_no_bisiesto_se_rechaza()
    {
        var (bisiesto, _) = ReglasDelCierreDeEjercicio.CortesDe("2028", "02-29", null, "0", null);
        Assert.Equal(new DateOnly(2028, 2, 29), bisiesto!.Legal);

        var (comun, sin) = ReglasDelCierreDeEjercicio.CortesDe("2026", "02-29", null, "0", null);

        Assert.Null(comun);
        Assert.Contains("No se corre al día más cercano", sin!.PorQueNo);
    }

    [Theory]
    // dd-MM en vez de MM-DD. Cae en el chequeo del mes, y el mensaje lo dice tal cual: quien
    // cargó «31-12» leerá «mes 31, que no existe» y va a entender de inmediato qué invirtió.
    [InlineData("31-12", "dice mes 31, que no existe")]
    [InlineData("13-01", "dice mes 13, que no existe")]
    [InlineData("diciembre", "la forma MM-DD")]
    public void Un_dia_y_mes_mal_cargado_se_declara_con_lo_que_decia(
        string valor, string esperado)
    {
        var (cortes, sin) = ReglasDelCierreDeEjercicio.CortesDe("2026", valor, null, "15", null);

        Assert.Null(cortes);
        Assert.Contains(esperado, sin!.PorQueNo);
    }

    /// <summary>
    /// Días negativos dejarían el corte operativo antes del legal — los días de en medio sin
    /// ejercicio al que imputarse. Es el mismo bloqueo de <c>ExigirCortes</c>, un paso antes.
    /// </summary>
    [Fact]
    public void Dias_negativos_no_resuelven()
    {
        var (cortes, sin) = ReglasDelCierreDeEjercicio.CortesDe("2026", "12-31", null, "-5", null);

        Assert.Null(cortes);
        Assert.Contains("no puede ser anterior al legal", sin!.PorQueNo);
    }

    [Fact]
    public void Un_ejercicio_que_no_es_un_anio_no_resuelve()
    {
        var (cortes, sin) = ReglasDelCierreDeEjercicio.CortesDe(
            "el que viene", "12-31", null, "15", null);

        Assert.Null(cortes);
        Assert.Contains("no es un ejercicio", sin!.PorQueNo);
    }

    // ── La ventana de cierre es parámetro con vigencia ──────────────────────

    /// <summary>
    /// `RN-96` la declara configurable. La ventana va del corte legal menos los días cargados
    /// hasta el corte operativo, y <b>dice de qué versión salió</b>: un indicador que no declara
    /// contra qué ventana se midió no se puede reproducir ni discutir.
    /// </summary>
    [Fact]
    public void La_ventana_sale_del_parametro_y_declara_su_origen()
    {
        var (ventana, sin) = ReglasDelCierreDeEjercicio.VentanaDe(
            "15", new DateOnly(2024, 1, 1), CorteLegal, CorteOperativo);

        Assert.Null(sin);
        Assert.NotNull(ventana);
        Assert.Equal(new DateOnly(2026, 12, 16), ventana!.Desde);
        Assert.Equal(CorteOperativo, ventana.Hasta);
        Assert.Equal(31, ventana.Dias);
        Assert.Contains("15 días", ventana.Origen);
        Assert.Contains("01/01/2024", ventana.Origen);
    }

    /// <summary>
    /// Otra institución, otra ventana. Es el punto de que sea parámetro: una que corta el 20 de
    /// diciembre no mide lo mismo que otra que opera hasta el 15 de enero.
    /// </summary>
    [Fact]
    public void Otro_valor_cargado_da_otra_ventana()
    {
        var (ventana, _) = ReglasDelCierreDeEjercicio.VentanaDe(
            "45", new DateOnly(2024, 1, 1), CorteLegal, CorteOperativo);

        Assert.Equal(new DateOnly(2026, 11, 16), ventana!.Desde);
        Assert.Equal(61, ventana.Dias);
    }

    /// <summary>
    /// <b>Sin parámetro no hay ventana, y no hay ventana por omisión.</b>
    ///
    /// Un «15 días razonable» haría que los dos reportes que dependen de ella salieran
    /// calculados contra un número que nadie declaró, y un lector no podría distinguirlos de los
    /// que sí se midieron. Es la misma disciplina del rendimiento esperado: <i>«suponer uno
    /// produciría hallazgos falsos que en tres meses nadie miraría»</i>.
    /// </summary>
    [Fact]
    public void Sin_parametro_cargado_NO_hay_ventana_por_omision()
    {
        var (ventana, sin) = ReglasDelCierreDeEjercicio.VentanaDe(
            null, null, CorteLegal, CorteOperativo);

        Assert.Null(ventana);
        Assert.NotNull(sin);
        Assert.Equal("cierre.ventana_de_cierre_dias", sin!.Clave);
        Assert.Contains("no hay versión aprobada", sin.PorQueNo);
    }

    /// <summary>
    /// Un valor mal cargado <b>tampoco</b> se reemplaza por uno bueno: se dice qué decía.
    /// </summary>
    [Fact]
    public void Un_valor_que_no_es_numero_se_declara_con_lo_que_decia()
    {
        var (ventana, sin) = ReglasDelCierreDeEjercicio.VentanaDe(
            "quince", null, CorteLegal, CorteOperativo);

        Assert.Null(ventana);
        Assert.Contains("«quince»", sin!.PorQueNo);
    }

    /// <summary>
    /// <b>Cero no es una ventana corta: es ninguna ventana.</b> Con cero días el indicador de
    /// apuro nunca podría disparar y los motivos compartidos no se buscarían en ningún lado — y
    /// las dos cosas se leerían como «no hubo hallazgos».
    /// </summary>
    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void Una_ventana_de_cero_o_menos_no_resuelve(string valor)
    {
        var (ventana, sin) = ReglasDelCierreDeEjercicio.VentanaDe(
            valor, null, CorteLegal, CorteOperativo);

        Assert.Null(ventana);
        Assert.Contains("no deja dónde buscar", sin!.PorQueNo);
    }

    /// <summary>
    /// Sin ventana, el acta dice que los dos reportes <b>no se evaluaron</b> — no que salieron
    /// limpios. Es la diferencia entre «no hubo motivos compartidos» y «no se buscaron».
    /// </summary>
    [Fact]
    public void Sin_ventana_el_acta_declara_que_los_reportes_NO_se_evaluaron()
    {
        var acta = Acta(
            saldo: "SA-2026-001",
            sinVentana: new VentanaSinResolver(
                "cierre.ventana_de_cierre_dias", "no hay versión aprobada."));

        Assert.Contains(acta.Observaciones,
            o => o.Contains("están sin medir, no en cero"));
    }

    // ── El indicador de cierre apurado ──────────────────────────────────────

    /// <summary>
    /// `RN-96` casos límite: <i>«el sistema no la resuelve; la hace visible. El indicador de
    /// misiones cerradas en la ventana de cierre, contra el promedio del año, es el dato que
    /// expone el cierre apurado»</i>.
    /// </summary>
    [Fact]
    public void Cerrar_en_bloque_en_la_ventana_multiplica_el_ritmo_del_anio()
    {
        // 20 cierres en 5 días de ventana contra 35 repartidos en el resto del año.
        var cierres = new List<DateOnly>();
        for (var i = 0; i < 20; i++) cierres.Add(new DateOnly(2026, 12, 27 + i % 5));
        for (var i = 0; i < 35; i++) cierres.Add(new DateOnly(2026, 1, 1).AddDays(i * 9));

        var apuro = ReglasDelCierreDeEjercicio.Apuro(
            cierres, new DateOnly(2026, 12, 27), new DateOnly(2026, 12, 31));

        Assert.Equal(20, apuro.CerradasEnLaVentana);
        Assert.Equal(5, apuro.DiasDeLaVentana);
        Assert.Equal(4d, apuro.PromedioDiarioEnLaVentana);

        // El promedio del año **excluye la ventana**: incluirla la diluiría contra sí misma.
        Assert.NotNull(apuro.PromedioDiarioDelAnio);
        Assert.Equal(35d / 360d, apuro.PromedioDiarioDelAnio!.Value, 6);

        Assert.NotNull(apuro.Veces);
        Assert.True(apuro.Veces > 30);
    }

    /// <summary>
    /// <b>Sin cierres fuera de la ventana no hay con qué comparar.</b> Decir que el ritmo se
    /// multiplicó por infinito sería inventar el hallazgo, y el indicador se declara no
    /// evaluable en vez de dar un número.
    /// </summary>
    [Fact]
    public void Sin_nada_fuera_de_la_ventana_el_indicador_no_se_evalua()
    {
        var apuro = ReglasDelCierreDeEjercicio.Apuro(
            [new DateOnly(2026, 12, 28), new DateOnly(2026, 12, 29)],
            new DateOnly(2026, 12, 27), new DateOnly(2026, 12, 31));

        Assert.Equal(2, apuro.CerradasEnLaVentana);
        Assert.Null(apuro.PromedioDiarioDelAnio);
        Assert.Null(apuro.Veces);
    }

    /// <summary>
    /// Un año con la operación repartida no dispara el indicador. Si lo hiciera, la observación
    /// aparecería siempre y dejaría de significar algo.
    /// </summary>
    [Fact]
    public void Un_anio_parejo_no_dispara_el_indicador()
    {
        var cierres = new List<DateOnly>();
        for (var i = 0; i < 360; i++) cierres.Add(new DateOnly(2026, 1, 1).AddDays(i));

        var apuro = ReglasDelCierreDeEjercicio.Apuro(
            cierres, new DateOnly(2026, 12, 17), new DateOnly(2026, 12, 26));

        Assert.NotNull(apuro.Veces);
        Assert.True(apuro.Veces < 1.2, $"Un año parejo dio {apuro.Veces:N2} veces el ritmo.");
    }

    // ── Ni el compromiso ni el folio se arrastran ───────────────────────────

    [Fact]
    public void El_folio_del_ejercicio_anterior_no_se_consume_en_el_siguiente()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelCierreDeEjercicio.ExigirQueElFolioNoSeArrastre("VC-2026-0810", "2026", "2027"));

        Assert.Contains("Ni el compromiso ni el folio se arrastran", error.Message);
        Assert.Contains("folio nuevo del rango vigente", error.Message);
    }

    [Fact]
    public void El_folio_del_ejercicio_corriente_pasa() =>
        ReglasDelCierreDeEjercicio.ExigirQueElFolioNoSeArrastre("VC-2027-0001", "2027", "2027");

    // ── El vale entregado no se anula ───────────────────────────────────────

    /// <summary>
    /// `V-03` sólo corre sobre un vale emitido. Un vale entregado y sin consumir al 31 de
    /// diciembre <b>es dinero fuera de la caja al cierre</b> — y contarlo en el monto por anular
    /// diría que ese dinero vuelve al fondo por efecto del acta, que no es cierto.
    /// </summary>
    [Fact]
    public void El_monto_por_anular_excluye_los_vales_entregados()
    {
        var acta = Acta(folios:
        [
            new FolioPorAnular(Ulid.NewUlid(), "VC-2026-0810", "Delegación Norte", 1_500m,
                new DateOnly(2026, 12, 20), "Emitida", SePuedeAnular: true),
            new FolioPorAnular(Ulid.NewUlid(), "VC-2026-0811", "Delegación Norte", 900m,
                new DateOnly(2026, 12, 22), "Entregada", SePuedeAnular: false),
        ]);

        Assert.Equal(1_500m, acta.MontoPorAnular);
        Assert.Single(acta.FoliosAfuera);
        Assert.Contains(acta.Observaciones, o => o.Contains("entregados y sin consumir"));
    }

    // ── Las observaciones del acta ──────────────────────────────────────────

    [Fact]
    public void El_motivo_compartido_sale_como_observacion()
    {
        var acta = Acta(motivos:
        [
            new MotivoCompartido("Cierre de ejercicio", [MisionA, MisionB], El(31, 16, 40), El(31, 16, 41)),
        ]);

        Assert.Contains(acta.Observaciones, o => o.Contains("evaluación individual"));
    }

    [Fact]
    public void La_diferencia_contra_el_saldo_sale_como_observacion()
    {
        var acta = Acta(
            saldo: "SA-2026-001",
            diferencias: ["MisionSinCerrar «OM-44» está vivo y no figura en el saldo"]);

        Assert.Contains(acta.Observaciones, o => o.Contains("renglón por renglón"));
    }

    /// <summary>
    /// <b>Sin saldo producido no hay coincidencia: hay ausencia de comparación.</b>
    ///
    /// Salió al abrir la pantalla contra un ejercicio sin saldo: decía «coincide con el saldo de
    /// apertura» con la lista de diferencias vacía, cuando estaba vacía porque no había contra
    /// qué compararla. Es la misma mentira que `RN-97` persigue cuando un inventario se ve
    /// completo estando incompleto.
    /// </summary>
    [Fact]
    public void Sin_saldo_producido_el_acta_dice_que_NO_cuadro_contra_nada()
    {
        var acta = Acta(saldo: null, diferencias: []);

        Assert.Contains(acta.Observaciones, o => o.Contains("no se cuadró contra nada"));
    }

    /// <summary>
    /// Un acta limpia <b>no observa nada</b>. Si observara siempre algo, las observaciones
    /// dejarían de leerse.
    /// </summary>
    [Fact]
    public void Un_cierre_limpio_no_produce_observaciones() =>
        Assert.Empty(Acta(saldo: "SA-2026-001").Observaciones);

    /// <summary>
    /// <b>La ventana y su ausencia son excluyentes</b>, y el andamio lo impone: pasar
    /// <c>sinVentana</c> apaga la ventana y el indicador de apuro, que es exactamente lo que
    /// hace el servicio. Un andamio que dejara construir un acta con ventana nula y sin razón
    /// permitiría probar un estado que el sistema no produce.
    /// </summary>
    private static ActaDeCierreDeEjercicio Acta(
        IReadOnlyList<FolioPorAnular>? folios = null,
        IReadOnlyList<MotivoCompartido>? motivos = null,
        IReadOnlyList<string>? diferencias = null,
        IReadOnlyList<MisionQueCruza>? cruzan = null,
        CierreApurado? apuro = null,
        string? saldo = "SA-2026-001",
        VentanaSinResolver? sinVentana = null) =>
        new(Ulid.NewUlid(), "AC-2026-001", "2026", CorteLegal, CorteOperativo,
            Autoria.De(new IdPersona("P-ADMIN"), new IdPuesto("PU-GERENCIA"), CorteLegal),
            El(31, 17), [], cruzan ?? [], folios ?? [], [], motivos ?? [],
            sinVentana is null ? apuro ?? new CierreApurado(0, 0, 5, null) : null,
            diferencias ?? [], saldo,
            sinVentana is null
                ? new VentanaDeCierre(
                    new DateOnly(2026, 12, 16), CorteOperativo, 31, "andamio de prueba")
                : null,
            sinVentana);
}
