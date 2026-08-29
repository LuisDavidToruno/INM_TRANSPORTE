using Sigti.Dominio.M18_Peajes;

namespace Sigti.Pruebas.M18_Peajes;

/// <summary>
/// `RN-37` — el cruce peaje × kilometraje × ruta autorizada.
///
/// ── Por qué esto importa más que la suma de montos ───────────────────────────
/// `NRM-10`, textual: <i>«Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo,
/// y el sistema tiene que producirlo solo. <b>Esto es exactamente lo que busca el auditor del
/// TSC: correlación, no comprobantes archivados</b>»</i>.
///
/// ── Y por qué la mitad de estas pruebas es que NO señale ────────────────────
/// La propia regla advierte que sin la capacidad de declarar un desvío <i>«produciría hallazgos
/// falsos en masa»</i>. Un control que grita todos los días es un control que nadie mira — el
/// mismo final que `RN-30` predice para el rendimiento inventado.
/// </summary>
public class ReglasDeCoherenciaDeSecuenciaPruebas
{
    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly Ulid Zambrano = Ulid.NewUlid();
    private static readonly Ulid Comayagua = Ulid.NewUlid();
    private static readonly Ulid Siguatepeque = Ulid.NewUlid();
    private static readonly Ulid Yojoa = Ulid.NewUlid();

    private static readonly DateTimeOffset Salida =
        new(2026, 4, 10, 6, 0, 0, TimeSpan.FromHours(-6));

    /// <summary>El orden geográfico real del Corredor Logístico, de sur a norte `[V]`.</summary>
    private static PasoParaCruzar Paso(
        Ulid punto, string nombre, int km, double horasDesdeLaSalida, int odometro = 84_000) =>
        new(Ulid.NewUlid(), punto, nombre, "CA-5", km, Vehiculo,
            Salida.AddHours(horasDesdeLaSalida), odometro);

    /// <summary>El Corredor Logístico, en su orden geográfico real de sur a norte `[V]`.</summary>
    private static readonly CasetaEnElCorredor[] Corredor =
    [
        new(Zambrano, "Zambrano", "CA-5", 35),
        new(Comayagua, "Comayagua", "CA-5", 85),
        new(Siguatepeque, "Siguatepeque", "CA-5", 130),
        new(Yojoa, "Yojoa", "CA-5", 190),
    ];

    private static readonly HashSet<Ulid> TodaLaRuta =
        [Zambrano, Comayagua, Siguatepeque, Yojoa];

    /// <summary>
    /// <c>sinRuta</c> distingue «no se pasó» de «explícitamente nula». Sin el sentinel, la
    /// prueba de la misión de ruta abierta caía en el valor por omisión y comprobaba lo
    /// contrario de lo que decía su nombre.
    /// </summary>
    private static DictamenDeCoherencia Evaluar(
        IReadOnlyList<PasoParaCruzar> pasos,
        IReadOnlySet<Ulid>? ruta = null,
        bool sinRuta = false,
        int? kilometros = 400,
        int? velocidad = 90,
        bool relojConfiable = true,
        IReadOnlyList<DesvioDeclarado>? desvios = null,
        IReadOnlyList<CasetaEnElCorredor>? corredor = null) =>
        ReglasDeCoherenciaDeSecuencia.Evaluar(
            pasos,
            sinRuta ? null : ruta ?? TodaLaRuta,
            kilometros, velocidad, relojConfiable, desvios ?? [],
            corredor ?? Corredor);

    // ── El viaje normal no produce nada ─────────────────────────────────────

    [Fact]
    public void Un_viaje_de_ida_y_vuelta_por_las_mismas_casetas_es_COHERENTE()
    {
        // Es el caso mayoritario y tiene que salir limpio. Marcar el retorno como incoherencia
        // geográfica produciría un hallazgo en cada misión del año.
        var d = Evaluar([
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Comayagua, "Comayagua", 85, 1.5),
            Paso(Siguatepeque, "Siguatepeque", 130, 2.3),
            Paso(Siguatepeque, "Siguatepeque", 130, 9.0),
            Paso(Comayagua, "Comayagua", 85, 9.8),
            Paso(Zambrano, "Zambrano", 35, 10.8),
        ]);

        Assert.True(d.Coherente);
        Assert.Empty(d.Hallazgos);
        Assert.True(d.Dimensiones.Todas);
        Assert.Equal(6, d.PasosEvaluados);
    }

    [Fact]
    public void El_orden_de_CAPTURA_no_importa_solo_la_fecha_del_hecho()
    {
        // El motorista que captura todos los pasos al final del día no cometió una incoherencia
        // de secuencia: cometió un orden de ingreso, que es otra cosa (`RN-46`).
        var d = Evaluar([
            Paso(Yojoa, "Yojoa", 190, 3.2),
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Siguatepeque, "Siguatepeque", 130, 2.3),
            Paso(Comayagua, "Comayagua", 85, 1.5),
        ]);

        Assert.Empty(d.Hallazgos);
    }

    // ── Dimensión 1: geográfica ─────────────────────────────────────────────

    [Fact]
    public void Saltar_sobre_una_caseta_activa_se_senala_y_NOMBRA_cual()
    {
        // No se puede estar en el km 190 habiendo venido del km 35 sin cruzar el 85 y el 130.
        // O falta anotar esos pasos, o el vehículo no fue por donde dicen estas dos casetas.
        var d = Evaluar([
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Yojoa, "Yojoa", 190, 3.0),
        ]);

        var salto = d.Hallazgos.Single(i =>
            i.Tipo is TipoDeIncoherencia.SecuenciaGeograficamenteImposible);

        Assert.Contains("«Comayagua» (km 85)", salto.Explicacion);
        Assert.Contains("«Siguatepeque» (km 130)", salto.Explicacion);
        Assert.Contains("no hay paso registrado", salto.Explicacion);
    }

    [Fact]
    public void Una_caseta_CERRADA_ese_dia_no_se_echa_de_menos()
    {
        // `RN-37`: «el estado del punto con vigencia evita marcar como omisión un peaje que
        // nadie cobró». Sólo entran al catálogo las que cobraban ese día.
        CasetaEnElCorredor[] sinComayaguaNiSiguatepeque =
        [
            new(Zambrano, "Zambrano", "CA-5", 35),
            new(Yojoa, "Yojoa", "CA-5", 190),
        ];

        var d = Evaluar(
            [Paso(Zambrano, "Zambrano", 35, 0.5), Paso(Yojoa, "Yojoa", 190, 3.0)],
            corredor: sinComayaguaNiSiguatepeque);

        Assert.DoesNotContain(d.Hallazgos, i =>
            i.Tipo is TipoDeIncoherencia.SecuenciaGeograficamenteImposible);
    }

    [Fact]
    public void Una_mision_multi_destino_con_varios_cambios_de_sentido_es_COHERENTE()
    {
        // `CE-08`. Contar cambios de sentido produciría un hallazgo en cada misión de varios
        // destinos, que es exactamente el ruido que mata el indicador.
        var d = Evaluar([
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Comayagua, "Comayagua", 85, 1.5),
            Paso(Comayagua, "Comayagua", 85, 5.0),
            Paso(Zambrano, "Zambrano", 35, 6.0),
            Paso(Zambrano, "Zambrano", 35, 20.0),
            Paso(Comayagua, "Comayagua", 85, 21.0),
        ]);

        Assert.Empty(d.Hallazgos);
    }
    [Fact]
    public void Sin_kilometro_cargado_la_dimension_geografica_NO_se_evalua()
    {
        // Y eso no es «salió limpio»: deducir el orden del orden de captura invertiría la
        // respuesta en toda misión de retorno.
        var d = Evaluar([
            new(Ulid.NewUlid(), Zambrano, "Zambrano", "CA-5", null, Vehiculo, Salida, 84_000),
            new(Ulid.NewUlid(), Comayagua, "Comayagua", "CA-5", 85, Vehiculo,
                Salida.AddHours(2), 84_100),
        ]);

        Assert.False(d.Dimensiones.Geografica);
        Assert.False(d.Coherente);
        Assert.Contains(d.Dimensiones.PorQueNo, m => m.Contains("orden de captura"));
    }

    [Fact]
    public void Cambiar_de_CORREDOR_no_es_incoherencia()
    {
        // El km 60 de la CA-5 y el km 60 de la CA-1 no están cerca. Compararlos produciría un
        // hallazgo en toda misión que combine dos carreteras.
        var d = Evaluar([
            new(Ulid.NewUlid(), Zambrano, "Zambrano", "CA-5", 35, Vehiculo, Salida, 84_000),
            new(Ulid.NewUlid(), Comayagua, "Peaje sur", "CA-1", 20, Vehiculo,
                Salida.AddHours(3), 84_200),
            new(Ulid.NewUlid(), Yojoa, "Otro sur", "CA-1", 70, Vehiculo,
                Salida.AddHours(4), 84_260),
        ]);

        Assert.DoesNotContain(d.Hallazgos, i =>
            i.Tipo is TipoDeIncoherencia.SecuenciaGeograficamenteImposible);
    }

    // ── Dimensión 2: temporal ───────────────────────────────────────────────

    [Fact]
    public void Un_intervalo_fisicamente_imposible_se_senala()
    {
        // 95 km en 15 minutos son 380 km/h. Uno de los dos momentos está mal, o uno de los dos
        // pasos no ocurrió.
        var d = Evaluar([
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Comayagua, "Comayagua", 85, 0.75),
        ]);

        var temporal = d.Hallazgos.Single(i => i.Tipo is TipoDeIncoherencia.IntervaloInviable);

        Assert.Contains("50 km", temporal.Explicacion);
        Assert.Contains("sobre un máximo de 90 km/h", temporal.Explicacion);
    }

    [Fact]
    public void Con_el_reloj_NO_CONFIABLE_la_incoherencia_temporal_no_es_concluyente()
    {
        // Un reloj de dispositivo desajustado fabrica intervalos imposibles. Presentarlos como
        // hallazgo produce exactamente el ruido que mata el indicador.
        var d = Evaluar(
            [Paso(Zambrano, "Zambrano", 35, 0.5), Paso(Comayagua, "Comayagua", 85, 0.75)],
            relojConfiable: false);

        var temporal = d.Incoherencias.Single(i => i.Tipo is TipoDeIncoherencia.IntervaloInviable);

        Assert.False(temporal.Concluyente);
        Assert.False(temporal.EsHallazgo);
        Assert.Empty(d.Hallazgos);
        Assert.Contains("NO CONFIABLE", temporal.Explicacion);

        // Y la dimensión queda declarada como no evaluada: la incoherencia consta, pero no se
        // puede afirmar nada con ella.
        Assert.False(d.Dimensiones.Temporal);
    }

    [Fact]
    public void Sin_velocidad_maxima_declarada_la_dimension_temporal_NO_se_evalua()
    {
        // `[C]`. Sin velocidad declarada, cualquier intervalo se podría llamar imposible y
        // ninguno se podría defender.
        var d = Evaluar(
            [Paso(Zambrano, "Zambrano", 35, 0.5), Paso(Comayagua, "Comayagua", 85, 0.75)],
            velocidad: null);

        Assert.False(d.Dimensiones.Temporal);
        Assert.DoesNotContain(d.Incoherencias, i =>
            i.Tipo is TipoDeIncoherencia.IntervaloInviable);
        Assert.Contains(d.Dimensiones.PorQueNo, m => m.Contains("velocidad_media_maxima"));
    }

    [Fact]
    public void Una_parada_larga_entre_casetas_no_es_incoherencia_temporal()
    {
        // Demasiado lento indica parada no registrada, que es una pregunta legítima — no una
        // imposibilidad física. Sólo se marca el extremo rápido.
        var d = Evaluar([
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Comayagua, "Comayagua", 85, 9.0),
        ]);

        Assert.Empty(d.Hallazgos);
    }

    // ── El duplicado ────────────────────────────────────────────────────────

    [Fact]
    public void Dos_pasos_por_la_misma_caseta_en_minutos_son_un_DUPLICADO()
    {
        var d = Evaluar([
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Zambrano, "Zambrano", 35, 0.6),
        ]);

        var duplicado = d.Hallazgos.Single(i => i.Tipo is TipoDeIncoherencia.PasoDuplicado);

        // O se capturó dos veces, o la caseta cobró dos veces. Las dos cosas se resuelven
        // distinto, y por eso el mensaje nombra las dos.
        Assert.Contains("capturó dos veces", duplicado.Explicacion);
        Assert.Contains("cobró dos veces", duplicado.Explicacion);
    }

    [Fact]
    public void El_duplicado_se_detecta_SIN_kilometro_ni_velocidad()
    {
        // Es el mismo punto dos veces en veinte minutos: eso no necesita geografía para saberse.
        var d = Evaluar(
            [
                new(Ulid.NewUlid(), Zambrano, "Zambrano", null, null, Vehiculo, Salida, 84_000),
                new(Ulid.NewUlid(), Zambrano, "Zambrano", null, null, Vehiculo,
                    Salida.AddMinutes(6), 84_000),
            ],
            velocidad: null);

        Assert.Single(d.Hallazgos, i => i.Tipo is TipoDeIncoherencia.PasoDuplicado);
    }

    [Fact]
    public void Volver_a_pasar_por_la_misma_caseta_HORAS_despues_no_es_duplicado()
    {
        // Es el retorno, y es lo normal.
        var d = Evaluar([
            Paso(Zambrano, "Zambrano", 35, 0.5),
            Paso(Zambrano, "Zambrano", 35, 10.5),
        ]);

        Assert.DoesNotContain(d.Hallazgos, i => i.Tipo is TipoDeIncoherencia.PasoDuplicado);
    }

    // ── Dimensión 3: contra la ruta autorizada ──────────────────────────────

    [Fact]
    public void Un_peaje_de_Yojoa_en_una_mision_autorizada_a_Comayagua_es_HALLAZGO()
    {
        // El caso que `NRM-10` pide textualmente que el sistema produzca solo.
        var d = Evaluar(
            [Paso(Siguatepeque, "Siguatepeque", 130, 2.3), Paso(Yojoa, "Yojoa", 190, 3.0)],
            ruta: new HashSet<Ulid> { Zambrano, Comayagua, Siguatepeque });

        var fuera = d.Hallazgos.Single(i =>
            i.Tipo is TipoDeIncoherencia.PuntoFueraDeRutaAutorizada);

        Assert.Contains("«Yojoa» no está en la ruta que se autorizó", fuera.Explicacion);

        // Y el mensaje no acusa: nombra las dos lecturas posibles, porque la regla observa y no
        // juzga.
        Assert.Contains("desvío legítimo", fuera.Explicacion);
        Assert.Contains("uso del vehículo fuera de la misión", fuera.Explicacion);
    }

    [Fact]
    public void Sin_estimado_congelado_la_tercera_dimension_NO_se_evalua()
    {
        // Es la misión de ruta abierta. `RN-37`: «se marca así explícitamente para que la
        // ausencia de hallazgos no se lea como conformidad».
        var d = Evaluar([Paso(Zambrano, "Zambrano", 35, 0.5)], sinRuta: true);

        Assert.False(d.Dimensiones.ContraLaRutaAutorizada);
        Assert.False(d.Coherente);
        Assert.Empty(d.Hallazgos);
        Assert.Contains(d.Dimensiones.PorQueNo, m => m.Contains("estimado de peajes congelado"));
    }

    // ── El desvío declarado ─────────────────────────────────────────────────

    [Fact]
    public void Un_desvio_declarado_desde_el_campo_JUSTIFICA_la_incoherencia()
    {
        // Honduras tiene derrumbes y cierres con regularidad. Sin esta capacidad la regla
        // produciría hallazgos falsos en masa.
        var paso = Paso(Yojoa, "Yojoa", 190, 3.0);

        var desvio = new DesvioDeclarado(
            Ulid.NewUlid(), Ulid.NewUlid(), Vehiculo,
            Salida.AddHours(2), Salida.AddHours(6),
            "Derrumbe en el km 120 de la CA-5, desvío por la ruta de Yojoa.");

        var d = Evaluar(
            [Paso(Siguatepeque, "Siguatepeque", 130, 2.3), paso],
            ruta: new HashSet<Ulid> { Zambrano, Comayagua, Siguatepeque },
            desvios: [desvio]);

        Assert.Empty(d.Hallazgos);

        // Pero la incoherencia NO se borra: que existió y que alguien la explicó son dos
        // hechos, y el auditor pregunta por los dos.
        var fuera = d.Incoherencias.Single(i =>
            i.Tipo is TipoDeIncoherencia.PuntoFueraDeRutaAutorizada);

        Assert.True(fuera.Justificada);
        Assert.Contains("Derrumbe en el km 120", fuera.Justificacion);
    }

    [Fact]
    public void Un_desvio_que_cubre_solo_PARTE_del_intervalo_no_lo_justifica()
    {
        // Un desvío que cubre la mitad de un intervalo no explica el intervalo.
        var desvio = new DesvioDeclarado(
            Ulid.NewUlid(), Ulid.NewUlid(), Vehiculo,
            Salida.AddHours(0.6), Salida.AddHours(0.7), "Cierre breve.");

        var d = Evaluar(
            [Paso(Zambrano, "Zambrano", 35, 0.5), Paso(Comayagua, "Comayagua", 85, 0.75)],
            desvios: [desvio]);

        Assert.Single(d.Hallazgos, i => i.Tipo is TipoDeIncoherencia.IntervaloInviable);
    }

    [Fact]
    public void Un_desvio_de_OTRO_momento_no_justifica_nada()
    {
        var deAyer = new DesvioDeclarado(
            Ulid.NewUlid(), Ulid.NewUlid(), Vehiculo,
            Salida.AddDays(-1), Salida.AddDays(-1).AddHours(4), "Derrumbe de ayer.");

        var d = Evaluar(
            [Paso(Siguatepeque, "Siguatepeque", 130, 2.3), Paso(Yojoa, "Yojoa", 190, 3.0)],
            ruta: new HashSet<Ulid> { Zambrano, Comayagua, Siguatepeque },
            desvios: [deAyer]);

        Assert.NotEmpty(d.Hallazgos);
    }

    // ── El cruce contra el kilometraje ──────────────────────────────────────

    [Fact]
    public void Noventa_kilometros_declarados_no_alcanzan_para_tres_casetas_lejanas()
    {
        // `RN-37` punto 3, con el ejemplo de la regla: si la misión declara 90 km y registra
        // pasos por casetas separadas por cientos, una de las dos cifras está mal.
        var d = Evaluar(
            [
                Paso(Zambrano, "Zambrano", 35, 0.5),
                Paso(Comayagua, "Comayagua", 85, 1.5),
                Paso(Siguatepeque, "Siguatepeque", 130, 2.3),
                Paso(Yojoa, "Yojoa", 190, 3.0),
            ],
            kilometros: 90);

        var contradiccion = d.Hallazgos.Single(i =>
            i.Tipo is TipoDeIncoherencia.PeajeSinKilometrajeQueLoRespalde);

        // El piso es 50 + 105 = 155 km, y la bitácora declara 90.
        Assert.Contains("al menos 155 km", contradiccion.Explicacion);
        Assert.Contains("declara 90 km", contradiccion.Explicacion);
    }

    [Fact]
    public void Un_kilometraje_holgado_no_produce_contradiccion()
    {
        // El piso es un piso: el vehículo pudo andar mucho más entre casetas, y eso es normal.
        var d = Evaluar(
            [Paso(Zambrano, "Zambrano", 35, 0.5), Paso(Comayagua, "Comayagua", 85, 1.5)],
            kilometros: 400);

        Assert.DoesNotContain(d.Hallazgos, i =>
            i.Tipo is TipoDeIncoherencia.PeajeSinKilometrajeQueLoRespalde);
    }

    [Fact]
    public void Sin_los_dos_odometros_el_cruce_contra_el_kilometraje_NO_se_hace()
    {
        var d = Evaluar([Paso(Zambrano, "Zambrano", 35, 0.5)], kilometros: null);

        Assert.False(d.Dimensiones.ContraElKilometraje);
        Assert.Contains(d.Dimensiones.PorQueNo, m => m.Contains("T-14"));
    }

    // ── El caso vacío ───────────────────────────────────────────────────────

    [Fact]
    public void Una_mision_sin_pasos_registrados_no_inventa_hallazgos()
    {
        var d = Evaluar([]);

        Assert.Empty(d.Hallazgos);
        Assert.Equal(0, d.PasosEvaluados);
    }
}
