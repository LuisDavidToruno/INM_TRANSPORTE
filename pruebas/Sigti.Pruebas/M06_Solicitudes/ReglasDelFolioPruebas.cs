using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M06_Solicitudes;

/// <summary>
/// `RN-44` y `RNF-21` — los rangos de folio pre-asignados.
///
/// Cuatro ceros: <i>cero duplicados a nivel institución, cero folios reciclados, cero
/// colisiones entre dispositivos, cero huecos sin explicación</i>.
/// </summary>
public class ReglasDelFolioPruebas
{
    private static readonly DateOnly Hoy = new(2026, 8, 30);

    private static RangoDeFolios Rango(
        string delegacion = "Choluteca",
        int desde = 1,
        int hasta = 100,
        int emitidos = 0,
        string? dispositivo = null,
        string tipo = "orden-de-mision") =>
        new(Ulid.NewUlid(), delegacion, tipo, desde, hasta, emitidos, dispositivo,
            "P-GERENCIA", Hoy);

    // ── Cero duplicados: los rangos no se solapan ───────────────────────────

    [Fact]
    public void Dos_rangos_solapados_del_mismo_documento_se_rechazan()
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelFolio.ExigirSinSolape(
                Rango("Danlí", 50, 150),
                [Rango("Choluteca", 1, 100)]));

        // El mensaje nombra al otro rango y a quién lo tiene: sin eso, quien asigna no sabe
        // desde dónde continuar y vuelve a chocar en el siguiente intento.
        Assert.Contains("Choluteca", e.Message);
        Assert.Contains("1–100", e.Message);
    }

    [Fact]
    public void Tres_dispositivos_de_la_misma_delegacion_necesitan_subrangos_distintos()
    {
        // `RNF-21` la llama «la que realmente rompe»: tres equipos de la MISMA delegación,
        // los tres desconectados, emitiendo el mismo tipo de documento. La delegación no
        // alcanza como unidad de reserva — descubren la colisión al sincronizar, con el papel
        // ya impreso y entregado en una caseta.
        var primero = Rango("Choluteca", 1, 100, dispositivo: "TAB-01");

        // Un segundo equipo con el mismo rango: se rechaza aunque sea la misma delegación.
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelFolio.ExigirSinSolape(
                Rango("Choluteca", 1, 100, dispositivo: "TAB-02"), [primero]));

        // Con subrangos disjuntos, los tres conviven.
        ReglasDelFolio.ExigirSinSolape(
            Rango("Choluteca", 101, 200, dispositivo: "TAB-02"), [primero]);
        ReglasDelFolio.ExigirSinSolape(
            Rango("Choluteca", 201, 300, dispositivo: "TAB-03"),
            [primero, Rango("Choluteca", 101, 200, dispositivo: "TAB-02")]);
    }

    [Fact]
    public void Rangos_del_mismo_numero_para_documentos_distintos_conviven()
    {
        // La unicidad es por tipo de documento: el vale 1 y la orden de misión 1 son dos
        // documentos distintos y ningún descargo los confunde.
        ReglasDelFolio.ExigirSinSolape(
            Rango("Choluteca", 1, 100, tipo: "vale-de-combustible"),
            [Rango("Choluteca", 1, 100, tipo: "orden-de-mision")]);
    }

    [Theory]
    [InlineData(100, 50)]
    [InlineData(0, 100)]
    public void Un_rango_invertido_o_que_empieza_bajo_uno_se_rechaza(int desde, int hasta)
    {
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelFolio.ExigirSinSolape(Rango(desde: desde, hasta: hasta), []));
    }

    // ── Cero reciclados ─────────────────────────────────────────────────────

    [Fact]
    public void El_siguiente_folio_avanza_sobre_lo_emitido_incluidas_las_anulaciones()
    {
        // ⚠️ El contador cuenta emitidos, no vigentes. Si contara vigentes, anular el folio 5
        // haría que el siguiente volviera a ser el 5 — y **un correlativo reutilizado es un
        // expediente que sustituye a otro**. Un correlativo con huecos es normal; ése no.
        Assert.Equal(6, ReglasDelFolio.Siguiente(Rango(desde: 1, emitidos: 5)));
    }

    [Fact]
    public void El_primer_folio_del_rango_es_su_inicio()
    {
        Assert.Equal(500, ReglasDelFolio.Siguiente(Rango(desde: 500, hasta: 600)));
    }

    [Fact]
    public void Un_rango_agotado_no_entrega_mas_folios()
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelFolio.Siguiente(Rango(desde: 1, hasta: 10, emitidos: 10)));

        // Y dice lo que importa operativamente: reponer exige conectividad. Una delegación sin
        // enlace que se entera al agotarse ya no puede hacer nada.
        Assert.Contains("conectividad", e.Message);
    }

    // ── El aviso previo ─────────────────────────────────────────────────────

    [Fact]
    public void Sin_umbral_fijado_no_hay_aviso_y_se_dice_que_no_lo_habra()
    {
        // `RNF-21` exige cero agotamientos sin aviso previo. Un tablero silencioso porque falta
        // el parámetro se ve **idéntico** a uno silencioso porque todo está bien.
        var aviso = ReglasDelFolio.Evaluar(Rango(emitidos: 95), umbralDeSaldo: null);

        Assert.Equal(GradoDelRango.NoSeEvalua, aviso.Grado);
        Assert.Contains("no habrá aviso previo", aviso.PorQue);
        Assert.Contains("#34", aviso.PorQue);
    }

    [Fact]
    public void Con_umbral_del_veinte_por_ciento_avisa_al_quedar_menos()
    {
        var aviso = ReglasDelFolio.Evaluar(Rango(emitidos: 85), umbralDeSaldo: 0.20m);

        Assert.Equal(GradoDelRango.PorAgotarse, aviso.Grado);
        Assert.Equal(15, aviso.Disponibles);
        Assert.Contains("conectividad", aviso.PorQue);
    }

    [Fact]
    public void Con_saldo_suficiente_no_alarma()
    {
        var aviso = ReglasDelFolio.Evaluar(Rango(emitidos: 10), umbralDeSaldo: 0.20m);

        Assert.Equal(GradoDelRango.Suficiente, aviso.Grado);
        Assert.Equal(90, aviso.Disponibles);
    }

    [Fact]
    public void El_rango_agotado_se_declara_agotado_aunque_no_haya_umbral()
    {
        // Agotado es un hecho, no una comparación: no necesita umbral y no puede depender de él.
        var aviso = ReglasDelFolio.Evaluar(Rango(emitidos: 100), umbralDeSaldo: null);

        Assert.Equal(GradoDelRango.Agotado, aviso.Grado);
    }

    // ── El formato, que no se infiere ───────────────────────────────────────

    [Fact]
    public void Sin_plantilla_configurada_no_se_compone_un_folio()
    {
        // ⚠️ `RNF-21`: el formato del correlativo **«no se decide por inferencia»** — insumo
        // #34. Componer un «OM-CHO-2026-000123» plausible produciría folios que la institución
        // citaría en descargos y que no coinciden con su numeración oficial; corregirlos
        // después obligaría a reemitir todo lo impreso.
        Assert.Null(ReglasDelFolio.Componer(null, Rango(), 2026, 1));
        Assert.Null(ReglasDelFolio.Componer("   ", Rango(), 2026, 1));
    }

    [Fact]
    public void Con_plantilla_se_compone_con_ceros_a_la_izquierda()
    {
        var folio = ReglasDelFolio.Componer(
            "OM-{delegacion}-{anio}-{numero}", Rango(), 2026, 42);

        // Los ceros no son estética: un correlativo que se ordena como texto en un reporte
        // tiene que ordenarse igual que como número, o el 10 aparece antes que el 9.
        Assert.Equal("OM-Choluteca-2026-000042", folio);
    }

    [Fact]
    public void La_plantilla_admite_el_tipo_de_documento()
    {
        Assert.Equal(
            "vale-de-combustible/2026/000007",
            ReglasDelFolio.Componer("{tipo}/{anio}/{numero}",
                Rango(tipo: "vale-de-combustible"), 2026, 7));
    }

    // ── La aritmética del rango ─────────────────────────────────────────────

    [Fact]
    public void El_total_incluye_los_dos_extremos()
    {
        // Del 1 al 100 son cien folios, no noventa y nueve. Un error de uno acá deja el último
        // folio del rango inutilizable, y ese folio ya está impreso en un talonario.
        var r = Rango(desde: 1, hasta: 100);

        Assert.Equal(100, r.Total);
        Assert.Equal(100, r.Disponibles);
        Assert.False(r.Agotado);
    }

    [Fact]
    public void Un_rango_de_un_solo_folio_es_valido()
    {
        var r = Rango(desde: 7, hasta: 7);

        Assert.Equal(1, r.Total);
        Assert.Equal(7, ReglasDelFolio.Siguiente(r));
    }
}
