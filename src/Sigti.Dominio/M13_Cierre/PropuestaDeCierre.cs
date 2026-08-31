namespace Sigti.Dominio.M13_Cierre;

/// <summary>
/// Qué contestó el sistema sobre un criterio `H-nn`.
///
/// ── ⚠️ Tres valores, y el tercero es el que importa ─────────────────────────
/// «No se cumple» y «nadie pudo mirarlo» <b>no son lo mismo</b>, y con dos valores se vuelven
/// indistinguibles: un criterio que el sistema no sabe evaluar contaría como limpio, y el
/// expediente cerraría `CERRADA` afirmando trece verificaciones de las que hizo seis.
///
/// Es la misma disciplina de todo el sistema —nulo ≠ cero ≠ «no se preguntó»— en el lugar
/// donde más cuesta: el acto que vuelve el expediente inmutable.
/// </summary>
public enum ResultadoDelCriterio
{
    /// <summary>Se cumplió. Hay hallazgo, y el detalle dice cuál es el caso.</summary>
    SeCumple,

    /// <summary>Se evaluó y no se cumple. <b>Esto sí es limpio</b>, y se puede sostener.</summary>
    NoSeCumple,

    /// <summary>
    /// <b>Nadie lo miró.</b> El sistema todavía no sabe evaluarlo, o le faltan los datos.
    ///
    /// No impide cerrar —un expediente que no puede cerrarse se abandona, y uno abandonado no
    /// produce el hallazgo que el auditor necesita ver (`RN-08`)—, pero <b>queda dicho en el
    /// acta</b>: quien cierra firma sabiendo qué se verificó y qué no.
    /// </summary>
    NoVerificado,
}

/// <param name="Enunciado">
/// Qué pregunta el criterio, en corto. El texto completo vive en §7.2 de
/// <c>orden-de-mision.md</c>, que es la autoridad; esto es el rótulo con que se muestra.
/// </param>
/// <param name="Detalle">
/// ⚠️ <b>Obligatorio en los tres resultados</b>, y cada uno dice algo distinto:
/// <b>se cumple</b> → el caso concreto («diferencia de caja de L 400 sin explicar», no «hay una
/// diferencia»); <b>no se cumple</b> → contra qué se miró; <b>no verificado</b> → <b>qué falta</b>
/// para poder mirarlo. Un «no verificado» sin motivo es un hueco que nadie va a poder cerrar
/// porque nadie va a saber qué le falta.
/// </param>
public sealed record CriterioEvaluado(
    string Criterio,
    string Enunciado,
    ResultadoDelCriterio Resultado,
    string Detalle);

/// <summary>
/// Lo que el sistema propone al cerrar — §7.2: <i>«el sistema propone la clasificación… la
/// propuesta no cierra: cerrar es acto de ACT-08»</i>.
///
/// ── Por qué la propuesta la hace el servidor ────────────────────────────────
/// `T-21` tiene como precondición que <b>no se cumpla ninguno</b> de los criterios. Una
/// precondición que el propio llamador declara no es una precondición: quien manda una lista
/// vacía cierra limpio, y la bitácora dice que cerró limpio.
/// </summary>
public sealed record PropuestaDeCierre(IReadOnlyList<CriterioEvaluado> Criterios)
{
    /// <summary>Los que se cumplieron. <b>Uno solo ya lleva a `T-22`.</b></summary>
    public IReadOnlyList<CriterioEvaluado> Cumplidos =>
        [.. Criterios.Where(c => c.Resultado == ResultadoDelCriterio.SeCumple)];

    /// <summary>
    /// Los que nadie pudo mirar. <b>Se muestran siempre</b>, también cuando el expediente
    /// cierra limpio: es justo entonces cuando ocultarlos haría creer lo que no es.
    /// </summary>
    public IReadOnlyList<CriterioEvaluado> SinVerificar =>
        [.. Criterios.Where(c => c.Resultado == ResultadoDelCriterio.NoVerificado)];

    public IReadOnlyList<CriterioEvaluado> Verificados =>
        [.. Criterios.Where(c => c.Resultado == ResultadoDelCriterio.NoSeCumple)];

    /// <summary>
    /// A dónde va el expediente. <b>Lo decide el criterio, no quien cierra</b> — §7.2.
    ///
    /// Un criterio sin verificar <b>no</b> produce hallazgo: sería declarar un hallazgo que
    /// nadie constató, y `CERRADA_CON_HALLAZGO` es una marca sobre la conducta de la
    /// institución. Lo que produce es la constancia de que no se miró.
    /// </summary>
    public bool HayHallazgo => Cumplidos.Count > 0;
}

/// <summary>
/// Los hechos del expediente contra los que se evalúan los criterios de cierre.
///
/// ── Van juntos por la razón de <c>CirculacionEnDiaInhabil</c> ───────────────
/// Es <b>una sola pregunta</b> —«¿este expediente cierra limpio?»— y separarla en trece
/// parámetros deja que un llamador conteste nueve. Agrupados, el compilador exige la respuesta
/// entera, y lo que no se puede contestar se declara <b>nulo a propósito</b>, no se omite.
/// </summary>
/// <param name="ValesConDesviacion">
/// Los folios de los vales que conciliaron <b>fuera de umbral en cualquier dirección</b>, con
/// causa tipificada — `RN-30`. Consumir de menos también es desviación: puede ser combustible
/// que no entró al tanque.
/// </param>
/// <param name="FondoEntregadoSinDevolver">
/// Cuántas asignaciones se entregaron y no se devolvieron ni comprobaron. <b>Nulo es que no se
/// consultó M-09</b>, y eso hace `H-04` no verificable — distinto de cero, que es que se
/// consultó y no había ninguna.
/// </param>
/// <param name="DiasInhabilesCirculados">
/// Los días inhábiles que la misión efectivamente tocó, resueltos con el calendario vigente a
/// la fecha del hecho (`P-4`). Vacía es que no tocó ninguno.
/// </param>
/// <param name="AmparadaPorPermiso">
/// Si hay permiso firmado que ampare vehículo, motorista, destino y ventana <b>reales</b>.
/// Se mira contra los de hoy y no contra los del despacho: `H-05` existe precisamente para
/// atrapar lo que cambió después — una prórroga al sábado, un relevo de motorista.
/// </param>
/// <param name="Peajes">
/// El dictamen de `RN-37` por vehículo: peaje × kilometraje × ruta autorizada.
///
/// ── ⚠️ Tres estados, no dos ─────────────────────────────────────────────────
/// Con hallazgos, `H-03` se cumple. <b>Sin hallazgos y con las cuatro dimensiones miradas</b>,
/// no se cumple. Sin hallazgos y con alguna dimensión que no se pudo mirar, <b>no verificado</b>
/// — el propio dictamen ya lo dice así: <i>«sin hallazgos no es lo mismo que coherente; un
/// dictamen que no pudo mirar nada no es conformidad, es silencio»</i>.
///
/// Nulo es que no se consultó M-18. Vacía es que la misión no registró ningún paso.
/// </param>
/// <param name="Cadena">
/// La cadena de `RN-08`, ya evaluada. <b>Nula es que no se pudo armar.</b>
/// </param>
/// <param name="IncidentesSinResolver">
/// Los incidentes de la misión que siguen sin desenlace. <b>Nulo es que no se consultó M-12.</b>
/// </param>
public sealed record HechosDelCierre(
    IReadOnlyList<string> ValesConDesviacion,
    int? FondoEntregadoSinDevolver,
    IReadOnlyList<DateOnly> DiasInhabilesCirculados,
    bool AmparadaPorPermiso,
    IReadOnlyList<string>? IncidentesSinResolver,
    IReadOnlyList<DictamenDePeajes>? Peajes,
    CadenaDeTrazabilidad? Cadena);

/// <summary>
/// Lo que `RN-37` dictaminó sobre un vehículo, reducido a lo que `H-03` necesita.
///
/// Se traduce en la capa de aplicación en vez de traer <c>DictamenDeCoherencia</c> entero: el
/// criterio de cierre no depende de cómo M-18 modela una incoherencia, sólo de si encontró
/// alguna y de si pudo mirar. Acoplarlos haría que un cambio de M-18 rompiera el cierre.
/// </summary>
/// <param name="PorQueNoSePudoMirar">
/// Las dimensiones que quedaron sin evaluar, en palabras. Vacía cuando se miraron las cuatro.
/// </param>
public sealed record DictamenDePeajes(
    int Hallazgos,
    bool MiroTodasLasDimensiones,
    IReadOnlyList<string> PorQueNoSePudoMirar);

/// <summary>
/// La evaluación de los criterios de cierre `H-01` a `H-13` — §7.2 de `orden-de-mision.md`.
///
/// ── ⚠️ Qué se corrigió acá ──────────────────────────────────────────────────
/// La detección vivía <b>en el navegador</b>, y evaluaba <b>uno</b> de los trece criterios. El
/// endpoint de cierre recibía la lista de criterios <b>del cliente</b>: cualquiera que llamara
/// con la lista vacía cerraba `CERRADA`, y el asiento decía que cerró limpio.
///
/// §7.2 dice lo contrario en dos lugares: <i>«el sistema propone la clasificación»</i>, y la
/// precondición de `T-21` es <i>«no se cumple ninguno de los criterios»</i>. Una precondición
/// que declara el propio llamador no es una precondición.
///
/// ── Lo que todavía no se evalúa se dice, no se calla ────────────────────────
/// De los trece, hoy se evalúan seis. Los otros siete salen como
/// <see cref="ResultadoDelCriterio.NoVerificado"/> <b>con lo que les falta nombrado</b>. Un
/// expediente que cierra limpio afirmando trece verificaciones que no ocurrieron es peor que
/// uno que declara cuáles hizo: el segundo se puede auditar, el primero engaña al auditor.
/// </summary>
public static class ReglasDeLaPropuestaDeCierre
{
    /// <summary>
    /// ⚠️ <b>La lista es cerrada para una misión concreta</b> — §7.2: nadie inventa un criterio
    /// al cerrar, ni desactiva uno para el caso que tiene delante.
    ///
    /// Lo que sí se configura son los <b>umbrales</b> (`RN-39`), no la existencia del criterio.
    /// El enunciado que va acá es el rótulo con que se muestra; el texto que manda es el de
    /// §7.2, que es la autoridad.
    /// </summary>
    public static PropuestaDeCierre Evaluar(HechosDelCierre hechos) =>
        new(
        [
            H01(hechos),
            NoVerificado("H-02",
                "Kilometraje fuera de umbral respecto a la ruta autorizada",
                "La misión no lleva distancia estimada contra la cual comparar el odómetro. " +
                "Falta que M-07 congele el estimado de la ruta al programar."),
            H03(hechos),
            H04(hechos),
            H05(hechos),
            H06(hechos),
            NoVerificado("H-07",
                "Bloqueo duro que falló al revalidarse tras sincronizar",
                "M-16 rechaza y retiene los hechos que no revalidan, y no los marca contra la " +
                "misión: al cerrar no hay de dónde leer cuál bloqueo falló."),
            NoVerificado("H-08",
                "Comprobante obligatorio ausente, o divergencia resuelta descartando el campo",
                "La política de comprobantes obligatorios de la institución no está cargada " +
                "(insumo pendiente), y la resolución de conflictos no marca cuál versión se " +
                "descartó."),
            H09(hechos),
            NoVerificado("H-10",
                "Exceso de capacidad de pasajeros o de carga por novedad en ruta",
                "M-19 registra las novedades y no cuantifica la ocupación resultante contra la " +
                "capacidad de la ficha técnica."),
            NoVerificado("H-11",
                "Diferencia de liquidación sin explicar por encima de la tolerancia",
                "La tolerancia configurada de `RN-29` no está cargada como parámetro con " +
                "vigencia, y sin ella no se puede decir qué diferencia es «sin explicar»."),
            NoVerificado("H-12",
                "Digitación diferida sin adjunto del original, vencido el plazo",
                "`RN-47` fija el plazo por parámetro y no está cargado; los adjuntos tampoco " +
                "distinguen todavía el original en papel de la evidencia de campo."),
            NoVerificado("H-13",
                "Entrega de combustible a vehículo o motorista distintos de los de la orden",
                "M-09 guarda el vehículo y el receptor de cada asignación; falta contrastarlos " +
                "contra los de la reserva vigente al momento de la entrega."),
        ]);

    /// <summary>
    /// `H-01` — desviación de consumo fuera de umbral, <b>en cualquier dirección</b>.
    ///
    /// Sale de `RN-30`, que ya emite el dictamen: un vale en <c>ConciliadaConDesviacion</c> es
    /// una desviación que alguien contrastó y tipificó, no una sospecha.
    /// </summary>
    private static CriterioEvaluado H01(HechosDelCierre hechos) =>
        hechos.ValesConDesviacion.Count == 0
            ? new("H-01", EnunciadoH01, ResultadoDelCriterio.NoSeCumple,
                "Ningún vale de la misión concilió con desviación de rendimiento.")
            : new("H-01", EnunciadoH01, ResultadoDelCriterio.SeCumple,
                hechos.ValesConDesviacion.Count == 1
                    ? $"El vale {hechos.ValesConDesviacion[0]} concilió con desviación de " +
                      "rendimiento fuera de umbral."
                    : $"{hechos.ValesConDesviacion.Count} vales conciliaron con desviación de " +
                      $"rendimiento: {string.Join(", ", hechos.ValesConDesviacion)}.");

    /// <summary>
    /// `H-03` — paso por peaje incompatible con la ruta autorizada, o secuencia imposible.
    ///
    /// ── ⚠️ Lo que se corrigió al mirar la pantalla ──────────────────────────
    /// Este criterio salía como <i>no verificado</i> diciendo que «todavía no hay quien juzgue»,
    /// y M-18 <b>ya juzga</b>: `RN-37` emite su dictamen y la pantalla de cierre lo muestra
    /// arriba. El expediente mostraba <i>«peaje fuera de la ruta autorizada»</i> y, dos paneles
    /// más abajo, que nadie podía juzgar eso. Dos afirmaciones contradictorias en la misma
    /// pantalla, y la segunda era falsa.
    ///
    /// ── Los tres estados salen del propio dictamen ──────────────────────────
    /// `RN-37` ya distingue lo que hace falta: <i>«sin hallazgos no es lo mismo que coherente;
    /// un dictamen que no pudo mirar nada no es conformidad, es silencio»</i>. Un dictamen sin
    /// hallazgos que no miró la dimensión temporal <b>no verifica</b> `H-03`, lo silencia.
    /// </summary>
    private static CriterioEvaluado H03(HechosDelCierre hechos)
    {
        if (hechos.Peajes is null)
            return NoVerificado("H-03", EnunciadoH03, "No se pudo consultar M-18.");

        // Ningún paso registrado. **No es que la secuencia sea coherente**: es que no hay
        // secuencia, y afirmar conformidad sobre cero pasos es la clase de silencio que
        // `RN-37` nombra.
        if (hechos.Peajes.Count == 0)
        {
            return NoVerificado("H-03", EnunciadoH03,
                "La misión no registró ningún paso por peaje. Sin pasos no hay secuencia que " +
                "juzgar — y eso no es lo mismo que una secuencia coherente.");
        }

        var conHallazgo = hechos.Peajes.Where(p => p.Hallazgos > 0).ToList();

        if (conHallazgo.Count > 0)
        {
            return new("H-03", EnunciadoH03, ResultadoDelCriterio.SeCumple,
                $"El cruce de RN-37 encontró {conHallazgo.Sum(p => p.Hallazgos)} incoherencia(s) " +
                $"en {conHallazgo.Count} vehículo(s) de la misión.");
        }

        var ciegos = hechos.Peajes.Where(p => !p.MiroTodasLasDimensiones).ToList();

        if (ciegos.Count > 0)
        {
            return NoVerificado("H-03", EnunciadoH03,
                "El cruce de RN-37 no encontró incoherencias, pero no pudo mirarlo todo: " +
                string.Join(" · ", ciegos.SelectMany(c => c.PorQueNoSePudoMirar).Distinct()));
        }

        return new("H-03", EnunciadoH03, ResultadoDelCriterio.NoSeCumple,
            "El cruce de RN-37 evaluó las cuatro dimensiones y no encontró incoherencias.");
    }

    /// <summary>
    /// `H-04` — fondo entregado no devuelto ni comprobado.
    ///
    /// El plazo de liquidación se da por vencido <b>al cerrar</b>: cerrar es el acto que lo
    /// agota. Un fondo que sigue entregado cuando el expediente se vuelve inmutable es dinero
    /// público sin descargo, y la obligación de reintegro le sobrevive (`RN-86`).
    /// </summary>
    private static CriterioEvaluado H04(HechosDelCierre hechos) => hechos.FondoEntregadoSinDevolver switch
    {
        // ⚠️ Nulo es «no se consultó M-09», y no es cero. Tratarlos igual haría que una
        // consulta que falló se viera exactamente como una misión sin fondos pendientes.
        null => NoVerificado("H-04", EnunciadoH04,
            "No se pudo consultar el recuento de asignaciones de M-09."),

        0 => new("H-04", EnunciadoH04, ResultadoDelCriterio.NoSeCumple,
            "No quedan fondos entregados sin devolver ni comprobar."),

        var n => new("H-04", EnunciadoH04, ResultadoDelCriterio.SeCumple,
            $"{n} asignación(es) de fondo siguen entregadas sin devolver ni comprobar al " +
            "cerrar. La obligación de reintegro sobrevive al cierre en su propio expediente " +
            "(RN-86)."),
    };

    /// <summary>
    /// `H-05` — circuló en franja inhábil sin permiso vigente.
    ///
    /// ── ⚠️ No es `BD-04` otra vez ───────────────────────────────────────────
    /// `BD-04` mira al despachar, contra la ventana <b>solicitada</b>. Esto mira al conciliar,
    /// contra lo que <b>efectivamente pasó</b>, y por eso atrapa lo que `BD-04` no puede ver:
    /// una prórroga que metió el sábado, un relevo de motorista que invalidó el permiso, o una
    /// salida que entró por sincronización con el bloqueo evaluado en el dispositivo.
    /// </summary>
    private static CriterioEvaluado H05(HechosDelCierre hechos)
    {
        if (hechos.DiasInhabilesCirculados.Count == 0)
        {
            return new("H-05", EnunciadoH05, ResultadoDelCriterio.NoSeCumple,
                "La misión no circuló en día inhábil.");
        }

        var dias = string.Join(", ", hechos.DiasInhabilesCirculados.Select(d => $"{d:yyyy-MM-dd}"));

        return hechos.AmparadaPorPermiso
            ? new("H-05", EnunciadoH05, ResultadoDelCriterio.NoSeCumple,
                $"Circuló en día inhábil ({dias}) con permiso firmado que ampara el vehículo, " +
                "el motorista, el destino y la ventana.")
            : new("H-05", EnunciadoH05, ResultadoDelCriterio.SeCumple,
                $"Circuló en día inhábil ({dias}) y ningún permiso firmado ampara el vehículo, " +
                "el motorista, el destino y la ventana con que la misión terminó.");
    }

    /// <summary>
    /// `H-06` — incidente de la misión aún sin resolución en M-12.
    ///
    /// Es de los pocos que §7.1 nombra como condicionante real del cierre: <i>«un incidente en
    /// investigación cuyo desenlace altera la responsabilidad»</i>.
    /// </summary>
    private static CriterioEvaluado H06(HechosDelCierre hechos) => hechos.IncidentesSinResolver switch
    {
        null => NoVerificado("H-06", EnunciadoH06, "No se pudo consultar M-12."),

        { Count: 0 } => new("H-06", EnunciadoH06, ResultadoDelCriterio.NoSeCumple,
            "No hay incidentes de la misión pendientes de desenlace."),

        var abiertos => new("H-06", EnunciadoH06, ResultadoDelCriterio.SeCumple,
            $"{abiertos.Count} incidente(s) de la misión siguen sin desenlace: " +
            $"{string.Join(", ", abiertos)}."),
    };

    /// <summary>
    /// `H-09` — eslabón faltante de la cadena de trazabilidad — `RN-08`.
    ///
    /// ── ⚠️ Lo que está en camino NO es un eslabón faltante ──────────────────
    /// `RN-08` lo separa a propósito: <i>«no se cierra con hallazgo por falta de datos que están
    /// en camino. El sistema distingue ausente de pendiente de sincronización»</i>. Marcar de
    /// hallazgo un expediente cuya bitácora viaja en el teléfono de un motorista en Tocoa es
    /// acusar a alguien de una omisión que no cometió — y el hallazgo quedaría en el expediente
    /// para siempre, porque el cierre es inmutable.
    ///
    /// Por eso sale <b>no verificado</b> y no <i>no se cumple</i>: la cadena todavía no se puede
    /// juzgar. Lo que impide cerrar en ese caso es `BD-08`, que es bloqueo y no marca.
    /// </summary>
    private static CriterioEvaluado H09(HechosDelCierre hechos)
    {
        if (hechos.Cadena is not { } cadena)
            return NoVerificado("H-09", EnunciadoH09, "No se pudo armar la cadena del expediente.");

        if (cadena.EnCamino.Count > 0)
        {
            return NoVerificado("H-09", EnunciadoH09,
                "La cadena no se puede juzgar todavía: " +
                string.Join(" · ", cadena.EnCamino.Select(e => e.Nombre)) +
                " espera(n) sincronización. No se marca hallazgo por datos que están en camino " +
                "(RN-08, RN-50).");
        }

        if (cadena.Faltantes.Count == 0)
        {
            return new("H-09", EnunciadoH09, ResultadoDelCriterio.NoSeCumple,
                "La cadena está completa de punta a punta.");
        }

        // ⚠️ **Identificando cuál falta**, que es lo que `RN-08` exige literalmente. «La cadena
        // está incompleta» manda a recorrer ocho eslabones a mano.
        return new("H-09", EnunciadoH09, ResultadoDelCriterio.SeCumple,
            "Faltan eslabones de la cadena: " +
            string.Join(" · ", cadena.Faltantes.Select(e => $"{e.Nombre} — {e.Detalle}")));
    }

    private static CriterioEvaluado NoVerificado(string criterio, string enunciado, string queFalta) =>
        new(criterio, enunciado, ResultadoDelCriterio.NoVerificado, queFalta);

    private const string EnunciadoH01 =
        "Desviación de consumo de combustible fuera de umbral, en cualquier dirección";

    private const string EnunciadoH03 =
        "Paso por peaje incompatible con la ruta autorizada, o secuencia imposible";

    private const string EnunciadoH09 =
        "Eslabón faltante de la cadena de trazabilidad al cierre";

    private const string EnunciadoH04 =
        "Fondo entregado no devuelto ni comprobado al vencer el plazo de liquidación";

    private const string EnunciadoH05 =
        "Circulación en día u hora inhábil sin permiso vigente, detectada al conciliar";

    private const string EnunciadoH06 =
        "Incidente, siniestro, multa o pérdida del bien de la misión aún sin resolución";
}
