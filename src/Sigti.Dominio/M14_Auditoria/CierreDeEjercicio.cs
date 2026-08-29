using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Un hecho económico de una misión que cruzó el corte — `RN-96`.
///
/// ── La misión NO se divide ──────────────────────────────────────────────────
/// La regla es explícita: <i>«la Orden de Misión que cruza el corte no se divide. Cada hecho
/// económico se imputa al ejercicio de su fecha del hecho»</i>. El expediente sigue siendo uno;
/// lo que se reparte entre ejercicios son <b>sus hechos</b>, no él.
/// </summary>
/// <param name="Ejercicio">
/// El de la <b>fecha del hecho</b>, no el de la captura ni el del cierre de la misión. Un
/// consumo del 28 de diciembre es del ejercicio que cerró, aunque la misión se liquide en
/// marzo.
/// </param>
/// <param name="TablaParametrica">
/// Con qué se valoró. `RN-96` punto 4 lo exige <b>por cada hecho</b>: sin ella el cálculo no se
/// puede reproducir, y un desglose que no se puede rehacer no defiende nada.
/// </param>
public sealed record HechoImputado(
    string Ejercicio,
    DateOnly FechaDelHecho,
    string Concepto,
    decimal Monto,
    string? TablaParametrica = null);

/// <summary>
/// Una misión que cruzó el corte, con su desglose — `RN-96` punto 4.
/// </summary>
public sealed record MisionQueCruza(
    Ulid Mision,
    string Referencia,
    DateOnly Salida,
    DateOnly? Retorno,
    IReadOnlyList<HechoImputado> Hechos)
{
    /// <summary>Lo imputado a cada ejercicio. Es el desglose que la liquidación presenta.</summary>
    public IReadOnlyDictionary<string, decimal> PorEjercicio =>
        Hechos.GroupBy(h => h.Ejercicio)
            .ToDictionary(g => g.Key, g => g.Sum(h => h.Monto));

    /// <summary>
    /// Los hechos sin tabla paramétrica declarada. <b>Van nombrados</b>: `RN-96` punto 4 exige
    /// que el cálculo sea reproducible, y uno que no dice contra qué se valoró no lo es.
    /// </summary>
    public IReadOnlyList<HechoImputado> SinTablaParametrica =>
        [.. Hechos.Where(h => string.IsNullOrWhiteSpace(h.TablaParametrica))];
}

/// <summary>
/// Un folio reservado y no consumido al corte — `RN-96`.
///
/// <b>No se arrastra al ejercicio siguiente.</b> Se anula con acta, por rango y delegación.
/// </summary>
/// <param name="SePuedeAnular">
/// ── El vale entregado no se anula, y decir lo contrario sería mentir ────────
/// `V-03` sólo corre sobre un vale <c>Emitida</c>: <i>«después de entregado ya no cabe: ahí el
/// camino es la devolución con acta, o el extravío»</i>. Un vale entregado y sin consumir al 31
/// de diciembre <b>es dinero fuera de la caja al cierre</b> — un problema mayor que el folio
/// ocioso, no menor. Va en la lista, marcado, para que el acta no lo esconda entre los que sí
/// se anulan de un plumazo.
/// </param>
public sealed record FolioPorAnular(
    Ulid Asignacion,
    string Folio,
    string Delegacion,
    decimal Monto,
    DateOnly Emitido,
    string Estado,
    bool SePuedeAnular);

/// <summary>
/// Un cambio de parámetro dentro de la ventana de cierre — `RN-96` punto 6.
///
/// ── Por qué esto es un reporte propio ───────────────────────────────────────
/// `RN-96`, textual: <i>«es la evidencia de que <b>nadie aflojó un umbral en diciembre para
/// cerrar limpio</b>, o de que alguien lo hizo y quedó a la vista»</i>.
///
/// No acusa: registra. Un umbral que se movió en diciembre puede tener una razón perfecta, y
/// el reporte la muestra junto al cambio — lo que no puede es no aparecer.
/// </summary>
public sealed record CambioDeParametro(
    string Clave,
    string? ValorAnterior,
    string ValorNuevo,
    DateOnly VigenteDesde,
    DateTimeOffset Registrado,
    string CargadoPor,
    string? AprobadoPor);

/// <summary>
/// Dos o más misiones cerradas con el mismo motivo — `RN-96` punto 3.
///
/// ── Lo que esto detecta ─────────────────────────────────────────────────────
/// El cierre masivo disfrazado. `RN-96`: <i>«ante el Tribunal Superior de Cuentas, cincuenta
/// expedientes cerrados el 31 de diciembre a la misma hora con el mismo motivo <b>son el
/// hallazgo</b>, no su solución»</i>.
/// </summary>
public sealed record MotivoCompartido(
    string Motivo,
    IReadOnlyList<Ulid> Misiones,
    DateTimeOffset Primero,
    DateTimeOffset Ultimo)
{
    /// <summary>
    /// Cuánto tiempo pasó entre el primero y el último. <b>Minutos son peor que días</b>: un
    /// motivo repetido a lo largo del año puede ser una causa real que se repite; el mismo
    /// motivo en cincuenta expedientes en una hora es un cierre en bloque.
    /// </summary>
    public TimeSpan Ventana => Ultimo - Primero;
}

/// <summary>
/// La ventana de cierre — `RN-96`, <b>parámetro con vigencia</b>.
///
/// ── Por qué no es una constante, y por qué tampoco tiene omisión ────────────
/// Cuánto dura «la ventana de cierre» es una decisión de cada institución: una que cierra
/// contablemente el 31 y opera hasta el 15 de enero no tiene la misma que otra que corta el
/// 20 de diciembre. `RN-96` la declara configurable con vigencia, y por eso se resuelve
/// <b>a la fecha del corte legal</b> (`RN-40`): reevaluar un cierre de 2026 tiene que usar la
/// ventana que regía entonces, no la de hoy.
///
/// Y no tiene valor por omisión. Un «15 días razonable» convertiría los dos reportes que
/// dependen de ella —motivos compartidos y ritmo de cierre— en cifras calculadas contra un
/// número que nadie declaró. La regla de este sistema es la misma que para el rendimiento
/// esperado: <i>«suponer uno produciría hallazgos falsos que en tres meses nadie miraría —
/// que es como muere un control»</i>.
/// </summary>
/// <param name="Origen">
/// De qué versión del parámetro salió, con su vigencia. Va al acta: un indicador que no dice
/// contra qué ventana se midió no se puede reproducir ni discutir.
/// </param>
public sealed record VentanaDeCierre(
    DateOnly Desde,
    DateOnly Hasta,
    int Dias,
    string Origen);

/// <summary>
/// Por qué la ventana de cierre no se pudo resolver — `RN-96`.
///
/// <b>Se declara, no se sustituye.</b> Los reportes que dependen de ella salen marcados como
/// no evaluados, que no es lo mismo que salir en cero.
/// </summary>
public sealed record VentanaSinResolver(string Clave, string PorQueNo);

/// <summary>
/// Cuántas misiones se cerraron en la ventana de cierre, contra el resto del año.
///
/// `RN-96` casos límite, sobre la presión por cerrar todo antes del 31: <i>«el sistema no la
/// resuelve; <b>la hace visible</b>. El indicador de misiones cerradas en la ventana de cierre,
/// contra el promedio del año, es el dato que expone el cierre apurado»</i>.
/// </summary>
/// <param name="PromedioDiarioDelAnio">
/// Nulo cuando el año no tiene suficientes cierres para promediar. <b>Nulo no es cero</b>: sin
/// promedio no se puede decir que la ventana esté apurada, y decirlo igual sería inventar el
/// hallazgo.
/// </param>
public sealed record CierreApurado(
    int CerradasEnLaVentana,
    int CerradasEnElAnio,
    int DiasDeLaVentana,
    double? PromedioDiarioDelAnio)
{
    public double PromedioDiarioEnLaVentana =>
        DiasDeLaVentana == 0 ? 0 : (double)CerradasEnLaVentana / DiasDeLaVentana;

    /// <summary>
    /// Cuántas veces más rápido se cerró en la ventana. <b>Nulo cuando no hay con qué
    /// comparar</b>, y entonces el indicador se declara no evaluable en vez de dar un número.
    /// </summary>
    public double? Veces => PromedioDiarioDelAnio is { } promedio && promedio > 0
        ? PromedioDiarioEnLaVentana / promedio
        : null;
}

/// <summary>
/// El acta de cierre de ejercicio — `RN-96` punto 1.
///
/// ── Lo que el acta NO hace, y es su razón de ser ────────────────────────────
/// <b>No ejecuta ni habilita ninguna transición. Ningún expediente cambia de estado por efecto
/// de una fecha.</b>
///
/// `RN-96` explica el riesgo con nombre propio: <i>«sin esta regla escrita la primera
/// implementación va a poner un cierre masivo por fecha, porque es lo que resuelve ese
/// problema»</i>. Y por qué no se puede: un cierre masivo <i>«cerraría en bloque misiones con
/// criterios de hallazgo sin evaluar, con un motivo compartido por decenas de expedientes, y
/// destruiría la evaluación individual que `RN-08` exige»</i>.
///
/// Este tipo <b>es un reporte</b>. No tiene un solo método que mueva nada.
/// </summary>
/// <param name="CorteLegal">
/// La fecha que fija la norma. <b>Distinta de la operativa</b>: la contabilidad cierra a una
/// fecha y la operación sigue hasta otra, y confundirlas imputaría al ejercicio equivocado los
/// hechos de los días de en medio.
/// </param>
/// <param name="CorteOperativo">
/// Hasta cuándo se siguen registrando hechos del ejercicio que cierra.
/// </param>
public sealed record ActaDeCierreDeEjercicio(
    Ulid Id,
    string Folio,
    string Ejercicio,
    DateOnly CorteLegal,
    DateOnly CorteOperativo,
    Autoria Ejecuta,
    DateTimeOffset Momento,
    IReadOnlyList<RenglonDelSaldo> InventarioNoTerminal,
    IReadOnlyList<MisionQueCruza> MisionesQueCruzan,
    IReadOnlyList<FolioPorAnular> FoliosPorAnular,
    IReadOnlyList<CambioDeParametro> CambiosDeParametros,
    IReadOnlyList<MotivoCompartido> MotivosCompartidos,

    /// <summary>
    /// El ritmo de cierre en la ventana. <b>Nulo cuando la ventana no está parametrizada</b>, y
    /// entonces el indicador se declara no evaluado — que no es lo mismo que cero.
    /// </summary>
    CierreApurado? Apuro,
    IReadOnlyList<string> DiferenciasConElSaldo,

    /// <summary>
    /// El folio del saldo de apertura que el acta cita — `RN-97` punto 1.
    ///
    /// ── Nulo no es «cuadró» ─────────────────────────────────────────────────
    /// <b>Nulo es que no hay saldo producido, y por lo tanto nada contra qué cuadrar.</b> Sin
    /// este campo, una lista de diferencias vacía se lee como coincidencia perfecta — que es la
    /// misma mentira que `RN-97` persigue cuando un inventario se ve completo estando
    /// incompleto. Salió al abrir la pantalla contra un ejercicio sin saldo.
    /// </summary>
    string? SaldoDeAperturaFolio = null,

    /// <summary>
    /// La ventana contra la que se midieron los motivos compartidos y el ritmo de cierre.
    /// <b>Nula cuando la institución no fijó el parámetro</b>, y entonces esos dos reportes no
    /// se calcularon.
    /// </summary>
    VentanaDeCierre? Ventana = null,

    /// <summary>Por qué no se pudo resolver. Presente exactamente cuando la ventana es nula.</summary>
    VentanaSinResolver? SinVentana = null)
{
    /// <summary>
    /// El valor de los folios que <b>sí se pueden anular</b>. Es la cifra del reporte de
    /// reversión de compromisos, y no incluye los entregados: ese dinero no vuelve al fondo por
    /// una anulación — vuelve por devolución con acta, o se convierte en obligación de reintegro
    /// (`RN-86`).
    /// </summary>
    public decimal MontoPorAnular => FoliosPorAnular.Where(f => f.SePuedeAnular).Sum(f => f.Monto);

    /// <summary>
    /// Los que quedaron entregados y sin consumir al corte. <b>Dinero fuera de la caja al cierre
    /// del ejercicio</b>, y `RN-96` no los alcanza: un vale entregado no se anula.
    /// </summary>
    public IReadOnlyList<FolioPorAnular> FoliosAfuera =>
        [.. FoliosPorAnular.Where(f => !f.SePuedeAnular)];

    /// <summary>
    /// <b>Lo que el acta encontró y alguien tiene que mirar.</b> No es lo mismo que un cierre
    /// que no se puede hacer: el acta se produce igual, y los hallazgos quedan en ella.
    /// </summary>
    public IReadOnlyList<string> Observaciones
    {
        get
        {
            var observaciones = new List<string>();

            if (MotivosCompartidos.Count > 0)
                observaciones.Add(
                    $"{MotivosCompartidos.Count} motivo(s) de cierre compartido(s) por varias " +
                    "misiones. `RN-08` exige evaluación individual: un motivo repetido en " +
                    "decenas de expedientes es lo que el auditor lee como cierre en bloque.");

            if (SaldoDeAperturaFolio is null)
                observaciones.Add(
                    $"No hay saldo de apertura producido para el ejercicio {Ejercicio}. " +
                    "El inventario de este acta no se cuadró contra nada: `RN-96` punto 2 " +
                    "manda que coincida renglón por renglón con el saldo, y sin él la ausencia " +
                    "de diferencias no significa que coincida.");

            else if (DiferenciasConElSaldo.Count > 0)
                observaciones.Add(
                    $"{DiferenciasConElSaldo.Count} diferencia(s) entre este inventario y el " +
                    $"saldo de apertura {SaldoDeAperturaFolio}. `RN-97` exige que coincidan " +
                    "renglón por renglón.");

            var sinTabla = MisionesQueCruzan.Sum(m => m.SinTablaParametrica.Count);

            if (sinTabla > 0)
                observaciones.Add(
                    $"{sinTabla} hecho(s) de misiones que cruzan el corte sin tabla paramétrica " +
                    "declarada. Sin ella el desglose por ejercicio no se puede reproducir.");

            if (FoliosAfuera.Count > 0)
                observaciones.Add(
                    $"{FoliosAfuera.Count} vale(s) quedaron entregados y sin consumir al corte, " +
                    $"por {FoliosAfuera.Sum(f => f.Monto):N2}. `RN-96` no los alcanza: un vale " +
                    "entregado no se anula. El camino es la devolución con acta o la obligación " +
                    "de reintegro (`RN-86`), y ninguno ocurre por efecto de una fecha.");

            if (Apuro?.Veces is { } veces && veces > 2)
                observaciones.Add(
                    $"En la ventana de cierre se cerraron misiones a {veces:N1} veces el ritmo " +
                    "del año. El sistema no resuelve la presión por cerrar antes del 31: la " +
                    "hace visible.");

            // **Va de últimas y con nombre propio.** Sin ventana, los dos reportes de arriba
            // salieron vacíos por falta de parámetro y no por falta de hallazgos, y el acta
            // tiene que decir cuál de las dos cosas fue.
            if (SinVentana is { } sin)
                observaciones.Add(
                    $"La ventana de cierre no está parametrizada: {sin.PorQueNo} Ni los motivos " +
                    "de cierre compartidos (`RN-96` punto 3) ni el ritmo de cierre se " +
                    $"evaluaron — están sin medir, no en cero. Se carga en «{sin.Clave}», con " +
                    "vigencia y doble control.");

            return observaciones;
        }
    }
}

/// <summary>
/// Los controles del cierre de ejercicio — `RN-96`.
/// </summary>
public static class ReglasDelCierreDeEjercicio
{
    /// <summary>
    /// `RN-96` punto 1 — el acta declara <b>las dos fechas de corte</b>.
    ///
    /// ── Por qué son dos y no una ────────────────────────────────────────────
    /// La contabilidad cierra a una fecha —la legal— y la operación sigue registrando hechos del
    /// ejercicio hasta otra —la operativa—. Con una sola, los hechos de los días de en medio se
    /// imputarían al ejercicio equivocado, y el desglose de `RN-96` punto 4 diría cualquier cosa.
    /// </summary>
    public static void ExigirCortes(
        string folio, string ejercicio, DateOnly corteLegal, DateOnly corteOperativo)
    {
        if (string.IsNullOrWhiteSpace(folio))
            throw new BloqueoDuro("RN-96",
                "El acta de cierre de ejercicio es un documento con folio. Sin él no se puede " +
                "citar, y el saldo de apertura no tiene a qué acta corresponder.");

        if (string.IsNullOrWhiteSpace(ejercicio))
            throw new BloqueoDuro("RN-96", "El acta exige qué ejercicio cierra.");

        if (corteOperativo < corteLegal)
            throw new BloqueoDuro("RN-96",
                $"El corte operativo ({corteOperativo:dd/MM/yyyy}) es anterior al legal " +
                $"({corteLegal:dd/MM/yyyy}). La operación no puede dejar de registrar hechos del " +
                "ejercicio antes de que la contabilidad lo cierre: los hechos de los días de en " +
                "medio quedarían sin ejercicio al que imputarse.");
    }

    /// <summary>
    /// Reparte los hechos de una misión entre ejercicios — `RN-96`.
    ///
    /// ── Por la fecha del HECHO, no por la de captura ni la del cierre ───────
    /// `RN-40` y `RN-46`. Un consumo del 28 de diciembre es del ejercicio que cerró aunque la
    /// misión se liquide en marzo; imputarlo por la fecha de liquidación movería gasto de un
    /// año a otro sin que nadie lo decidiera.
    /// </summary>
    public static string EjercicioDe(DateOnly fechaDelHecho) => $"{fechaDelHecho.Year}";

    /// <summary>
    /// `RN-96` punto 3 — <b>nunca un motivo compartido por varios expedientes</b>.
    ///
    /// ── Lo que se compara y lo que no ───────────────────────────────────────
    /// Se compara el motivo <b>normalizado</b>: espacios colapsados y sin distinguir mayúsculas,
    /// porque quien cierra cincuenta expedientes copiando y pegando no va a escribir distinto
    /// cada vez. Lo que no se hace es buscar parecidos: dos motivos que dicen lo mismo con otras
    /// palabras son dos evaluaciones, y presumir lo contrario acusaría a quien sí evaluó.
    /// </summary>
    public static IReadOnlyList<MotivoCompartido> DetectarMotivosCompartidos(
        IReadOnlyList<(Ulid Mision, string Motivo, DateTimeOffset Momento)> cierres)
    {
        return
        [
            .. cierres
                .Where(c => !string.IsNullOrWhiteSpace(c.Motivo))
                .GroupBy(c => Normalizar(c.Motivo))
                .Where(g => g.Select(c => c.Mision).Distinct().Count() > 1)
                .Select(g => new MotivoCompartido(
                    g.First().Motivo,
                    [.. g.Select(c => c.Mision).Distinct()],
                    g.Min(c => c.Momento),
                    g.Max(c => c.Momento)))
                .OrderByDescending(m => m.Misiones.Count),
        ];
    }

    private static string Normalizar(string motivo) =>
        string.Join(' ', motivo.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();

    /// <summary>
    /// La clave del parámetro que fija cuántos días antes del corte legal empieza la ventana de
    /// cierre — `RN-96`, configurable con vigencia.
    /// </summary>
    public const string ClaveDeLaVentana = "cierre.ventana_de_cierre_dias";

    /// <summary>
    /// Arma la ventana de cierre a partir del valor cargado — `RN-96`.
    ///
    /// ── Devuelve nulo con razón, y nunca un valor de reemplazo ──────────────
    /// Ni cuando falta el parámetro ni cuando viene mal cargado. Un valor de reemplazo haría
    /// que los dos reportes que dependen de la ventana salieran calculados contra un número que
    /// nadie declaró, y un lector no podría distinguirlos de los que sí se midieron.
    /// </summary>
    /// <param name="valor">
    /// Lo que dice la versión vigente del parámetro. <b>Nulo es que no hay ninguna</b>.
    /// </param>
    /// <param name="vigenteDesde">Desde cuándo rige esa versión. Va al origen, para reproducir.</param>
    public static (VentanaDeCierre? Ventana, VentanaSinResolver? Sin) VentanaDe(
        string? valor, DateOnly? vigenteDesde, DateOnly corteLegal, DateOnly corteOperativo)
    {
        if (valor is null)
            return (null, new VentanaSinResolver(ClaveDeLaVentana,
                $"no hay versión aprobada que rigiera al {corteLegal:dd/MM/yyyy}."));

        if (!int.TryParse(valor, out var dias))
            return (null, new VentanaSinResolver(ClaveDeLaVentana,
                $"la versión vigente dice «{valor}», que no es un número de días."));

        // **Cero no es una ventana corta: es ninguna ventana.** Con cero días el indicador de
        // apuro nunca podría disparar y los motivos compartidos no se buscarían en ningún lado,
        // y las dos cosas se leerían como «no hubo hallazgos».
        if (dias <= 0)
            return (null, new VentanaSinResolver(ClaveDeLaVentana,
                $"la versión vigente dice «{dias}» días. Una ventana de cero o menos no deja " +
                "dónde buscar, y los dos reportes saldrían vacíos como si no hubiera hallazgos."));

        var desde = corteLegal.AddDays(-dias);

        return (
            new VentanaDeCierre(desde, corteOperativo,
                corteOperativo.DayNumber - desde.DayNumber + 1,
                $"{ClaveDeLaVentana} = {dias} días, versión vigente desde el " +
                $"{vigenteDesde:dd/MM/yyyy}"),
            null);
    }

    /// <summary>
    /// `RN-96` casos límite — el indicador que expone el cierre apurado.
    ///
    /// <b>El promedio del año excluye la ventana.</b> Incluirla la diluiría contra sí misma: si
    /// la mitad de los cierres del año ocurren en diciembre, un promedio que los cuente diría
    /// que diciembre fue normal.
    /// </summary>
    public static CierreApurado Apuro(
        IReadOnlyList<DateOnly> cierresDelAnio, DateOnly desdeLaVentana, DateOnly hastaLaVentana)
    {
        var enLaVentana = cierresDelAnio
            .Count(f => f >= desdeLaVentana && f <= hastaLaVentana);

        var fueraDeLaVentana = cierresDelAnio.Count - enLaVentana;

        var diasDeLaVentana = hastaLaVentana.DayNumber - desdeLaVentana.DayNumber + 1;
        var diasDelResto = 365 - diasDeLaVentana;

        // Sin cierres fuera de la ventana no hay contra qué comparar. Decir que el ritmo se
        // multiplicó por infinito sería inventar el hallazgo.
        double? promedio = fueraDeLaVentana > 0 && diasDelResto > 0
            ? (double)fueraDeLaVentana / diasDelResto
            : null;

        return new CierreApurado(
            enLaVentana, cierresDelAnio.Count, diasDeLaVentana, promedio);
    }

    /// <summary>
    /// `RN-96` — <b>ni el compromiso ni el folio se arrastran al ejercicio siguiente.</b>
    ///
    /// ── Y por eso anular es un acto aparte del acta ─────────────────────────
    /// El acta <b>lista</b> los folios; anularlos es un acto con autor y motivo que la cita. Es
    /// la misma razón por la que el acta no cierra misiones: un documento que ejecuta
    /// transiciones en bloque al producirse es un cierre masivo con otro nombre, aunque lo que
    /// cierre sean folios.
    /// </summary>
    public static void ExigirQueElFolioNoSeArrastre(
        string folio, string ejercicioDelFolio, string ejercicioActual)
    {
        if (string.Equals(ejercicioDelFolio, ejercicioActual, StringComparison.Ordinal)) return;

        throw new BloqueoDuro("RN-96",
            $"El folio «{folio}» se reservó en el ejercicio {ejercicioDelFolio} y se está " +
            $"queriendo consumir en {ejercicioActual}. Ni el compromiso ni el folio se arrastran " +
            "al ejercicio siguiente: el que quedó sin consumir se anula con acta, y el gasto " +
            "nuevo lleva folio nuevo del rango vigente.");
    }
}
