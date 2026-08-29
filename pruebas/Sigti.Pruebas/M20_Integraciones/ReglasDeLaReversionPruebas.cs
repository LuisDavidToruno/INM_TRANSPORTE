using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M20_Integraciones;

namespace Sigti.Pruebas.M20_Integraciones;

/// <summary>
/// `RN-96` punto 5 y `RN-81` — el reporte de reversión de compromisos para ARGOS y SIAFI.
///
/// ── Por qué existe, en las palabras de `RN-81` ──────────────────────────────
/// <i>«`RN-48` prohíbe que SIGTI escriba en ARGOS, y hace bien. Pero de esa prohibición no se
/// sigue que SIGTI pueda <b>callar</b>: si SIGTI anula un compromiso de combustible y no lo
/// reporta, el descuadre aparece en SIAFI y nadie sabe de dónde vino»</i>.
/// </summary>
public class ReglasDeLaReversionPruebas
{
    private static readonly DateOnly Legal = new(2026, 12, 31);
    private static readonly DateOnly Operativo = new(2027, 1, 15);

    private static readonly DateTimeOffset Corte =
        new(2027, 1, 20, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly Ulid Mision = Ulid.NewUlid();

    // ── El liberado es neto, no bruto ───────────────────────────────────────

    /// <summary>
    /// `RN-81`, caso límite: <i>«se expone el compromiso liberado <b>neto</b>, con el detalle de
    /// lo ejecutado, no el bruto»</i>.
    ///
    /// Exponer el bruto haría que SIAFI revirtiera dinero que ya se gastó.
    /// </summary>
    [Fact]
    public void El_compromiso_con_ejecucion_parcial_libera_solo_la_diferencia()
    {
        var renglon = Renglon(comprometido: 1_500m, ejecutado: 400m);

        Assert.Equal(1_100m, renglon.Liberado);
        Assert.True(renglon.TuvoEjecucionParcial);
    }

    [Fact]
    public void El_compromiso_sin_ejecucion_libera_todo()
    {
        var renglon = Renglon(comprometido: 1_500m, ejecutado: 0m);

        Assert.Equal(1_500m, renglon.Liberado);
        Assert.False(renglon.TuvoEjecucionParcial);
    }

    /// <summary>
    /// Un vale consumido por encima de lo comprometido <b>no libera «menos que cero»</b>: libera
    /// nada. Un liberado negativo se leería en SIAFI como un compromiso nuevo, y el exceso es
    /// otro expediente (`RN-26`, `RN-86`).
    /// </summary>
    [Fact]
    public void Un_consumo_mayor_que_el_compromiso_no_libera_negativo()
    {
        var renglon = Renglon(comprometido: 1_000m, ejecutado: 1_400m);

        Assert.Equal(0m, renglon.Liberado);
    }

    /// <summary>
    /// El total del reporte es la suma de los netos, no la del bruto menos la del ejecutado. Con
    /// un renglón sobreejecutado las dos cuentas dan distinto, y sólo una es la que SIAFI puede
    /// revertir.
    /// </summary>
    [Fact]
    public void El_total_liberado_suma_netos_y_no_resta_globales()
    {
        var reporte = Reporte(
            Renglon(comprometido: 1_000m, ejecutado: 1_400m, folio: "VC-1"),
            Renglon(comprometido: 2_000m, ejecutado: 0m, folio: "VC-2"));

        // Bruto menos ejecutado daría 1,600. El neto por renglón da 2,000, que es lo que de
        // verdad vuelve al presupuesto.
        Assert.Equal(2_000m, reporte.TotalLiberado);
        Assert.Equal(3_000m, reporte.TotalComprometido);
        Assert.Equal(1_400m, reporte.TotalEjecutado);
    }

    // ── El renglón sin objeto del gasto se marca, no se omite ───────────────

    /// <summary>
    /// `RN-26` deja registrar el fondo sin partida cuando el espejo de ARGOS no la tiene. Ese
    /// renglón <b>no se puede imputar en SIAFI</b>, y va en el reporte igual: omitirlo haría que
    /// el total no cuadrara contra la anulación que sí ocurrió — el descuadre exacto que
    /// `RN-81` existe para impedir.
    /// </summary>
    [Fact]
    public void El_renglon_sin_partida_va_en_el_reporte_marcado()
    {
        var reporte = Reporte(
            Renglon(comprometido: 500m, ejecutado: 0m, folio: "VC-1", partida: null),
            Renglon(comprometido: 800m, ejecutado: 0m, folio: "VC-2", partida: "31200"));

        Assert.Equal(1_300m, reporte.TotalLiberado);

        var sinPartida = Assert.Single(reporte.SinObjetoDelGasto);
        Assert.Equal("VC-1", sinPartida.Folio);

        Assert.Contains(reporte.Advertencias, a => a.Contains("sin objeto del gasto"));
    }

    /// <summary>
    /// `RN-81` punto 4 pide el detalle <b>por objeto del gasto</b>. El que no lo tiene queda
    /// fuera de esa agrupación —no hay partida contra la cual sumarlo— pero sigue en el total.
    /// </summary>
    [Fact]
    public void El_detalle_por_objeto_del_gasto_agrupa_lo_imputable()
    {
        var reporte = Reporte(
            Renglon(comprometido: 500m, ejecutado: 0m, folio: "VC-1", partida: "31200"),
            Renglon(comprometido: 300m, ejecutado: 0m, folio: "VC-2", partida: "31200"),
            Renglon(comprometido: 900m, ejecutado: 0m, folio: "VC-3", partida: "31100"),
            Renglon(comprometido: 100m, ejecutado: 0m, folio: "VC-4", partida: null));

        Assert.Equal(800m, reporte.PorObjetoDelGasto["31200"]);
        Assert.Equal(900m, reporte.PorObjetoDelGasto["31100"]);
        Assert.Equal(2, reporte.PorObjetoDelGasto.Count);

        // El sin partida no desaparece: sigue en el total, y por eso los dos números difieren.
        Assert.Equal(1_800m, reporte.TotalLiberado);
    }

    /// <summary>Un reporte sin nada raro no advierte nada.</summary>
    [Fact]
    public void Un_reporte_limpio_no_produce_advertencias() =>
        Assert.Empty(Reporte(Renglon(comprometido: 500m, ejecutado: 0m)).Advertencias);

    // ── Las dos fechas de `RN-94` ───────────────────────────────────────────

    [Fact]
    public void Un_periodo_que_termina_antes_de_empezar_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaReversion.ExigirLasDosFechas(Operativo, Legal, Corte));

        Assert.Equal("RN-94", error.Precondicion);
    }

    [Fact]
    public void Sin_corte_de_conocimiento_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaReversion.ExigirLasDosFechas(Legal, Operativo, default));

        Assert.Equal("RN-94", error.Precondicion);
        Assert.Contains("fecha de corte de conocimiento", error.Message);
        Assert.Contains("un defecto y no una actualización", error.Message);
    }

    /// <summary>
    /// <b>Un corte anterior al fin del período es legítimo.</b> «Qué se sabía al 15 de enero de
    /// los hechos de todo el ejercicio» es exactamente la pregunta que permite reproducir un
    /// reporte viejo, y rechazarla haría irreproducible lo que `RN-94` manda reproducir.
    /// </summary>
    [Fact]
    public void Un_corte_anterior_al_fin_del_periodo_es_legitimo() =>
        ReglasDeLaReversion.ExigirLasDosFechas(
            Legal, Operativo, new DateTimeOffset(2027, 1, 5, 0, 0, 0, TimeSpan.Zero));

    // ── El archivo de conciliación ──────────────────────────────────────────

    /// <summary>
    /// `RN-94` exige las dos fechas visibles. <b>Van en cada fila</b>: una hoja de cálculo que
    /// ordena o filtra pierde un bloque de metadatos, y entonces cada fila queda sin decir de
    /// qué corte salió — que es lo único que hace el reporte reproducible.
    /// </summary>
    [Fact]
    public void Cada_fila_del_archivo_lleva_las_dos_fechas_y_el_acta()
    {
        var csv = ReglasDeLaReversion.ArchivoDeConciliacion(
            Reporte(Renglon(comprometido: 1_500m, ejecutado: 400m, folio: "VC-2026-0810")));

        var lineas = csv.Split('\n');

        Assert.Equal(2, lineas.Length);
        Assert.Contains("periodo_desde;periodo_hasta;corte_de_conocimiento", lineas[0]);

        Assert.Contains("2026-12-31", lineas[1]);
        Assert.Contains("2027-01-15", lineas[1]);
        Assert.Contains("AC-2026-001", lineas[1]);
    }

    /// <summary>
    /// El monto va con <b>punto decimal e invariante</b>. El archivo lo lee otro sistema, y un
    /// «1.500,00» leído como número inglés es un error de importación silencioso.
    /// </summary>
    [Fact]
    public void Los_montos_van_con_punto_decimal_y_sin_separador_de_miles()
    {
        var csv = ReglasDeLaReversion.ArchivoDeConciliacion(
            Reporte(Renglon(comprometido: 1_500m, ejecutado: 400m)));

        var fila = csv.Split('\n')[1];

        Assert.Contains("1500.00;400.00;1100.00", fila);
    }

    /// <summary>
    /// <b>Vacío es «sin partida», no cero.</b> Quien importe el archivo tiene que poder separar
    /// los renglones que no se pueden imputar de los que se imputan a una partida «0».
    /// </summary>
    [Fact]
    public void El_renglon_sin_partida_deja_la_columna_vacia_y_no_un_cero()
    {
        var csv = ReglasDeLaReversion.ArchivoDeConciliacion(
            Reporte(Renglon(comprometido: 500m, ejecutado: 0m, partida: null)));

        var fila = csv.Split('\n')[1];

        // Dos punto y coma seguidos: la columna del objeto del gasto va vacía.
        Assert.Contains(";;", fila);
        Assert.DoesNotContain(";0;", fila);
    }

    /// <summary>
    /// Una delegación con punto y coma en el nombre <b>no puede partir la fila</b>. El separador
    /// es punto y coma porque el decimal local es la coma, y por eso el campo se escapa igual.
    /// </summary>
    [Fact]
    public void Un_campo_con_el_separador_adentro_se_escapa()
    {
        var csv = ReglasDeLaReversion.ArchivoDeConciliacion(
            Reporte(Renglon(comprometido: 500m, ejecutado: 0m, delegacion: "Choluteca; sur")));

        var fila = csv.Split('\n')[1];

        Assert.Contains("\"Choluteca; sur\"", fila);

        // Y la fila sigue teniendo las quince columnas del encabezado.
        Assert.Equal(15, csv.Split('\n')[0].Split(';').Length);
    }

    [Fact]
    public void Un_reporte_sin_renglones_produce_solo_el_encabezado()
    {
        var csv = ReglasDeLaReversion.ArchivoDeConciliacion(Reporte());

        Assert.Single(csv.Split('\n'));
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static CompromisoLiberado Renglon(
        decimal comprometido,
        decimal ejecutado,
        string folio = "VC-2026-0810",
        string? partida = "12-01-001-4-31200",
        string delegacion = "Delegación de Choluteca") =>
        new(Mision.ToString(), Mision, folio, delegacion, partida,
            new DateOnly(2027, 1, 10), new DateOnly(2027, 1, 10), comprometido, ejecutado);

    private static ReporteDeReversion Reporte(params CompromisoLiberado[] renglones) =>
        new("2026", Legal, Operativo, Corte, "AC-2026-001", renglones);
}
