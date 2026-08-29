using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M20_Integraciones;

/// <summary>
/// Un compromiso liberado por la reversión del cierre de ejercicio — `RN-96` punto 5, `RN-81`.
///
/// ── El liberado es NETO, y ése es el punto ──────────────────────────────────
/// `RN-81`, caso límite: <i>«se expone el compromiso liberado <b>neto</b>, con el detalle de lo
/// ejecutado, no el bruto. <b>El detalle es lo que permite conciliar</b>»</i>.
///
/// Por eso <see cref="Liberado"/> se calcula y no se recibe: un campo libre dejaría que alguien
/// escribiera el bruto, y SIAFI revertiría dinero que ya se gastó.
/// </summary>
/// <param name="ClaveDeVinculacion">
/// La de la Orden de Misión — `RN-81` punto 1: se establece al crearla y <b>no cambia</b> en todo
/// su ciclo.
///
/// ⚠️ <b>Hoy es el ULID de la misión.</b> La clave de vinculación con ARGOS no existe como
/// campo: el modelo de datos la nombra, el código no la tiene. Va el ULID para que el renglón
/// sea trazable dentro de SIGTI, pero <b>ARGOS no lo va a reconocer</b>.
/// </param>
/// <param name="ObjetoDelGasto">
/// La partida presupuestaria del fondo. <b>Nula cuando el espejo de ARGOS no la tenía</b>
/// (`RN-26` manda registrar el fondo igual). Un renglón sin objeto del gasto <b>no se puede
/// imputar en SIAFI</b>, y por eso va en el reporte marcado en vez de omitido: omitirlo haría
/// que el total no cuadrara contra la anulación que sí ocurrió.
/// </param>
/// <param name="FechaDelHecho">
/// La de la anulación, que es cuando el compromiso se liberó. `RN-81` punto 2 exige las dos:
/// ésta y la de captura.
/// </param>
public sealed record CompromisoLiberado(
    string ClaveDeVinculacion,
    Ulid Mision,
    string Folio,
    string Delegacion,
    string? ObjetoDelGasto,
    DateOnly FechaDelHecho,
    DateOnly FechaDeCaptura,
    decimal Comprometido,
    decimal Ejecutado)
{
    /// <summary>
    /// Lo que vuelve al presupuesto. <b>Calculado, nunca recibido.</b>
    ///
    /// Nunca negativo: un vale consumido por encima de lo comprometido no libera «menos que
    /// cero» — libera nada, y el exceso es otro expediente (`RN-26`, `RN-86`).
    /// </summary>
    public decimal Liberado => Math.Max(0m, Comprometido - Ejecutado);

    /// <summary>
    /// <b>Se ejecutó algo contra este folio antes de anularlo.</b> `RN-81` lo llama el caso de
    /// la ejecución parcial, y es el que obliga a exponer neto: el detalle es lo que permite
    /// conciliar la diferencia.
    /// </summary>
    public bool TuvoEjecucionParcial => Ejecutado > 0m;

    /// <summary>Sin objeto del gasto el renglón no se puede imputar en SIAFI.</summary>
    public bool SeConcilia => !string.IsNullOrWhiteSpace(ObjetoDelGasto);
}

/// <summary>
/// El reporte de reversión de compromisos — `RN-96` punto 5, para ARGOS y SIAFI (`RN-81`).
///
/// ── Por qué existe, en las palabras de `RN-81` ──────────────────────────────
/// <i>«`RN-48` prohíbe que SIGTI escriba en ARGOS, y hace bien. Pero de esa prohibición no se
/// sigue que SIGTI pueda <b>callar</b>: si SIGTI anula un compromiso de combustible y no lo
/// reporta, el descuadre aparece en SIAFI y nadie sabe de dónde vino»</i>.
///
/// ── Reporta reversiones HECHAS, no planeadas ────────────────────────────────
/// Sale de los folios que el acta listó <b>y que se anularon</b>. Un folio listado y todavía sin
/// anular no liberó nada: su compromiso sigue vivo, y reportarlo haría que SIAFI revirtiera un
/// dinero que en SIGTI sigue comprometido.
/// </summary>
/// <param name="PeriodoDesde">
/// `RN-94` — el <b>período del hecho</b> que el reporte cubre. Va junto al corte de conocimiento
/// y no en su lugar: son dos preguntas distintas.
/// </param>
/// <param name="CorteDeConocimiento">
/// `RN-94` — hasta qué momento se consideran los registros existentes. <b>El mismo reporte con
/// el mismo período y el mismo corte tiene que dar el mismo resultado dentro de cinco años</b>;
/// uno que cambia sin que cambie ninguno de los dos es un defecto.
/// </param>
/// <param name="ActaQueLoRespalda">
/// El folio del acta de cierre que listó estos compromisos. Sin él, el reporte es una lista de
/// anulaciones que no consta que correspondan a un cierre.
/// </param>
public sealed record ReporteDeReversion(
    string Ejercicio,
    DateOnly PeriodoDesde,
    DateOnly PeriodoHasta,
    DateTimeOffset CorteDeConocimiento,
    string ActaQueLoRespalda,
    IReadOnlyList<CompromisoLiberado> Renglones)
{
    public decimal TotalLiberado => Renglones.Sum(r => r.Liberado);

    public decimal TotalComprometido => Renglones.Sum(r => r.Comprometido);

    public decimal TotalEjecutado => Renglones.Sum(r => r.Ejecutado);

    /// <summary>
    /// Los que no se pueden imputar en SIAFI porque el fondo no traía partida.
    /// <b>Van en el reporte igual</b>: omitirlos haría que el total no cuadrara contra las
    /// anulaciones que sí ocurrieron, y ése es justo el descuadre que la regla existe para
    /// impedir.
    /// </summary>
    public IReadOnlyList<CompromisoLiberado> SinObjetoDelGasto =>
        [.. Renglones.Where(r => !r.SeConcilia)];

    /// <summary>Los que traían ejecución previa. Son los que obligan a exponer neto.</summary>
    public IReadOnlyList<CompromisoLiberado> ConEjecucionParcial =>
        [.. Renglones.Where(r => r.TuvoEjecucionParcial)];

    /// <summary>
    /// Lo liberado por objeto del gasto. Es la agrupación con la que se concilia en SIAFI —
    /// `RN-81` punto 4 pide el detalle <b>por Orden de Misión y por objeto del gasto</b>.
    /// </summary>
    public IReadOnlyDictionary<string, decimal> PorObjetoDelGasto =>
        Renglones
            .Where(r => r.SeConcilia)
            .GroupBy(r => r.ObjetoDelGasto!)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Liberado));

    /// <summary>Lo que quien reciba el reporte tiene que mirar antes de importarlo.</summary>
    public IReadOnlyList<string> Advertencias
    {
        get
        {
            var advertencias = new List<string>();

            if (SinObjetoDelGasto.Count > 0)
                advertencias.Add(
                    $"{SinObjetoDelGasto.Count} renglón(es) por " +
                    $"{SinObjetoDelGasto.Sum(r => r.Liberado):N2} sin objeto del gasto: el fondo " +
                    "no traía partida presupuestaria (`RN-26` permite registrarlo así). No se " +
                    "pueden imputar en SIAFI hasta que alguien les asigne partida, y van en el " +
                    "reporte para que el total cuadre contra la anulación.");

            if (ConEjecucionParcial.Count > 0)
                advertencias.Add(
                    $"{ConEjecucionParcial.Count} renglón(es) tenían ejecución previa. Lo " +
                    "liberado va **neto**, y el ejecutado va al lado: es lo que permite " +
                    "conciliar la diferencia contra lo que SIAFI tiene comprometido.");

            return advertencias;
        }
    }
}

/// <summary>
/// Los controles del reporte de reversión — `RN-96` punto 5, `RN-81`.
/// </summary>
public static class ReglasDeLaReversion
{
    /// <summary>
    /// `RN-94` — todo reporte declara <b>período del hecho</b> y <b>corte de conocimiento</b>.
    ///
    /// ── Son dos preguntas y las dos son legítimas ───────────────────────────
    /// El período dice qué hechos cubre; el corte, hasta cuándo se miran los registros que los
    /// describen. Un reporte con período y sin corte no se puede reproducir: vuelto a correr un
    /// año después incorporaría la digitación diferida y daría otro número sin que nadie hubiera
    /// cambiado un parámetro — que `RN-94` llama <i>«un defecto, no una actualización»</i>.
    /// </summary>
    public static void ExigirLasDosFechas(
        DateOnly desde, DateOnly hasta, DateTimeOffset corteDeConocimiento)
    {
        if (hasta < desde)
            throw new BloqueoDuro("RN-94",
                $"El período del reporte termina ({hasta:dd/MM/yyyy}) antes de empezar " +
                $"({desde:dd/MM/yyyy}).");

        // El corte de conocimiento anterior al fin del período es legítimo —«qué se sabía al
        // 15 de enero de los hechos de todo el ejercicio»— y es justamente la pregunta que
        // permite reproducir un reporte viejo. Lo que no puede es faltar.
        if (corteDeConocimiento == default)
            throw new BloqueoDuro("RN-94",
                "El reporte exige fecha de corte de conocimiento. Sin ella, vuelto a correr " +
                "dentro de un año daría otro número sin que nadie hubiera cambiado un " +
                "parámetro, y eso es un defecto y no una actualización.");
    }

    /// <summary>
    /// El archivo de conciliación — `RN-96` punto 5, `RN-81` punto 3.
    ///
    /// ── El formato es `[C]`, y el archivo lo dice ───────────────────────────
    /// `RN-81`: <i>«sin contrato conocido, el mecanismo inicial es el reporte con formato
    /// acordado»</i> — insumos #16 y #17. Este CSV <b>no es el formato de SIAFI</b>: es el
    /// mínimo que cualquiera puede abrir y conciliar a mano, que es lo que la regla prevé para
    /// una institución sin ARGOS: <i>«los reportes quedan disponibles para su uso manual»</i>.
    ///
    /// ── Las dos fechas de `RN-94` van en cada fila ──────────────────────────
    /// No en un bloque de metadatos arriba. Una hoja de cálculo que ordena o filtra pierde el
    /// encabezado, y entonces cada fila queda sin decir de qué corte salió — que es lo único que
    /// hace el reporte reproducible.
    /// </summary>
    public static string ArchivoDeConciliacion(ReporteDeReversion reporte)
    {
        var lineas = new List<string>
        {
            string.Join(';',
                "ejercicio", "periodo_desde", "periodo_hasta", "corte_de_conocimiento",
                "acta_de_cierre", "clave_de_vinculacion", "orden_de_mision", "folio_del_vale",
                "delegacion", "objeto_del_gasto", "fecha_del_hecho", "fecha_de_captura",
                "comprometido", "ejecutado", "liberado"),
        };

        foreach (var r in reporte.Renglones)
            lineas.Add(string.Join(';',
                Campo(reporte.Ejercicio),
                reporte.PeriodoDesde.ToString("yyyy-MM-dd"),
                reporte.PeriodoHasta.ToString("yyyy-MM-dd"),
                reporte.CorteDeConocimiento.ToString("o"),
                Campo(reporte.ActaQueLoRespalda),
                Campo(r.ClaveDeVinculacion),
                r.Mision.ToString(),
                Campo(r.Folio),
                Campo(r.Delegacion),

                // **Vacío es «sin partida», no cero.** Quien importe esto tiene que poder
                // separar los renglones que no se pueden imputar de los que se imputan a la
                // partida «0».
                Campo(r.ObjetoDelGasto ?? ""),

                r.FechaDelHecho.ToString("yyyy-MM-dd"),
                r.FechaDeCaptura.ToString("yyyy-MM-dd"),
                Monto(r.Comprometido),
                Monto(r.Ejecutado),
                Monto(r.Liberado)));

        return string.Join('\n', lineas);
    }

    /// <summary>
    /// Punto y coma como separador, y por eso el campo se escapa igual.
    ///
    /// Se eligió punto y coma porque el separador decimal de la configuración regional local es
    /// la coma: un CSV con coma se abriría partido en dos columnas. Aun así el campo se escapa —
    /// una delegación puede llamarse «Choluteca; sur» y partiría la fila.
    /// </summary>
    private static string Campo(string valor) =>
        valor.Contains(';') || valor.Contains('"') || valor.Contains('\n')
            ? $"\"{valor.Replace("\"", "\"\"")}\""
            : valor;

    /// <summary>
    /// El monto con punto decimal e invariante. <b>No con la coma local</b>: el archivo lo lee
    /// otro sistema, y un «1.500,00» leído como número inglés son mil quinientos con dos
    /// decimales de más o un error de importación.
    /// </summary>
    private static string Monto(decimal valor) =>
        valor.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
}
