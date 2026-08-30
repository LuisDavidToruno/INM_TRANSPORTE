using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// La tabla `W-01`..`W-19` del estado operativo del vehículo — §10.2 de
/// <c>orden-de-mision.md</c>, que es la <b>autoridad</b>.
///
/// ── Lo que estas pruebas cuidan ─────────────────────────────────────────────
/// Que la tabla del código siga siendo la transcripción del diagrama. Si las dos difieren,
/// manda el documento y esto es el defecto.
/// </summary>
public class ReglasDelEstadoOperativoPruebas
{
    // ── La tabla es la del diagrama ─────────────────────────────────────────

    /// <summary>
    /// Las del diagrama: diecinueve numeradas más `W-16b`. <b>Su identificador se conserva tal
    /// cual</b>: renumerarla haría que el asiento del sistema no se pudiera cruzar contra el
    /// documento.
    /// </summary>
    [Fact]
    public void La_tabla_es_la_del_diagrama_de_la_autoridad()
    {
        // Diecinueve numeradas más `W-16b`, que la autoridad numera así y acá se conserva tal
        // cual: renumerarla haría que el asiento no se pudiera cruzar contra el documento.
        Assert.Equal(20, ReglasDelEstadoOperativo.Tabla.Count);

        Assert.Contains(ReglasDelEstadoOperativo.Tabla, t => t.Id == "W-16b");

        // Ninguna repetida: dos filas con el mismo par origen→destino dejarían sin decidir cuál
        // identificador va al asiento.
        Assert.Equal(
            ReglasDelEstadoOperativo.Tabla.Count,
            ReglasDelEstadoOperativo.Tabla.Select(t => (t.Desde, t.Hasta)).Distinct().Count());
    }

    /// <summary>
    /// `W-01` — el vehículo <b>nace `NO_DISPONIBLE`</b>. §10.2 lista <i>«alta reciente sin
    /// habilitar»</i> entre las causas: un vehículo no se habilita por el solo hecho de existir.
    /// </summary>
    [Fact]
    public void El_vehiculo_nace_no_disponible()
    {
        var alta = ReglasDelEstadoOperativo.Buscar(null, EstadoOperativo.NoDisponible);

        Assert.Equal("W-01", alta?.Id);
        Assert.Null(ReglasDelEstadoOperativo.Buscar(null, EstadoOperativo.Disponible));
    }

    /// <summary>
    /// §10.2 — <b>`ASIGNADO` y `EN_MISION` los fija el sistema</b>, y son las únicas cuatro
    /// automáticas: entrar y salir de cada uno.
    /// </summary>
    [Fact]
    public void Solo_son_automaticas_las_consecuencias_de_transiciones_de_la_mision()
    {
        var automaticas = ReglasDelEstadoOperativo.Tabla.Where(t => t.Automatica).ToList();

        Assert.Equal(["W-03", "W-04", "W-05", "W-06"], automaticas.Select(t => t.Id));
    }

    /// <summary>Los dos terminales no tienen salida.</summary>
    [Theory]
    [InlineData(EstadoOperativo.DadoDeBaja)]
    [InlineData(EstadoOperativo.RetiradoDeFlota)]
    public void Los_estados_terminales_no_tienen_salida(EstadoOperativo terminal) =>
        Assert.DoesNotContain(ReglasDelEstadoOperativo.Tabla, t => t.Desde == terminal);

    // ── El bloqueo, y su mensaje ────────────────────────────────────────────

    [Fact]
    public void Una_transicion_que_la_autoridad_no_contempla_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirTransicion(
                EstadoOperativo.EnMision, EstadoOperativo.DadoDeBaja));

        // La autoridad de esta precondición es la sección, no una transición: se dispara
        // justamente cuando **no hay** una `W-nn` que la nombre. Antes decía `W-xx`, que siete
        // bloqueos distintos compartían — y `PT-004` muestra ese identificador en pantalla.
        Assert.Equal(ReglasDelEstadoOperativo.PrecondicionDeSeccion, error.Precondicion);
        Assert.Contains("§10.2 no contempla", error.Message);
    }

    /// <summary>
    /// El mensaje <b>enumera los destinos legales</b>. Un «transición no permitida» a secas
    /// obliga a quien opera a adivinar el camino.
    /// </summary>
    [Fact]
    public void El_bloqueo_dice_a_donde_SI_se_puede_ir()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirTransicion(
                EstadoOperativo.EnTaller, EstadoOperativo.Prestado));

        Assert.Contains("W-10 alta de taller", error.Message);
        Assert.Contains("W-13 irreparable o pendiente", error.Message);
    }

    [Fact]
    public void Desde_un_terminal_el_mensaje_lo_dice()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirTransicion(
                EstadoOperativo.DadoDeBaja, EstadoOperativo.Disponible));

        Assert.Contains("es un estado terminal", error.Message);
    }

    /// <summary>
    /// <b>El vehículo sin estado declarado.</b> `W-01` dice que nace `NO_DISPONIBLE`, pero
    /// `BD-07` ya decidió no bloquear con estado nulo —lo declara en el diario— y esa decisión es
    /// de la máquina de estados.
    ///
    /// Si `BD-07` dejó programar, negarse a anotar la consecuencia dejaría la misión programada y
    /// el vehículo sin asiento: peor que el asiento que falta.
    /// </summary>
    [Fact]
    public void La_consecuencia_automatica_se_anota_aunque_no_hubiera_estado_declarado()
    {
        var transicion = ReglasDelEstadoOperativo.ExigirTransicion(
            null, EstadoOperativo.Asignado);

        Assert.Equal("W-03", transicion.Id);
    }

    /// <summary>
    /// <b>Pero sólo las automáticas.</b> Una persona que declara un estado sobre un vehículo sin
    /// historial sigue teniendo que empezar por `W-01`.
    /// </summary>
    [Fact]
    public void Una_declaracion_manual_sobre_un_vehiculo_sin_historial_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirTransicion(null, EstadoOperativo.EnTaller));

    // ── Quién la fija ───────────────────────────────────────────────────────

    /// <summary>
    /// §10.2: <i>«permitir fijarlos a mano abre la puerta a un vehículo "en misión" sin
    /// misión»</i>.
    /// </summary>
    [Fact]
    public void Las_automaticas_no_se_declaran_a_mano()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirQuienLaFija(
                ReglasDelEstadoOperativo.Buscar(
                    EstadoOperativo.Asignado, EstadoOperativo.EnMision)!,
                automatica: false));

        Assert.Contains("«en misión» sin misión", error.Message);
    }

    /// <summary>
    /// Y al revés: si una persona la declara y el asiento dice que la puso el sistema, nadie
    /// respondería por ella.
    /// </summary>
    [Fact]
    public void Las_manuales_no_se_anotan_como_automaticas()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirQuienLaFija(
                ReglasDelEstadoOperativo.Buscar(
                    EstadoOperativo.Disponible, EstadoOperativo.EnTaller)!,
                automatica: true));

        Assert.Contains("Nadie respondería por ella", error.Message);
    }

    // ── Causa tipificada y acta ─────────────────────────────────────────────

    /// <summary>
    /// §10.2: <i>«sin tipificación, este estado se convierte en el <b>cementerio donde se
    /// esconde la flota que nadie repara</b>»</i>.
    /// </summary>
    [Fact]
    public void No_disponible_sin_causa_tipificada_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirCausaOActa(EstadoOperativo.NoDisponible, "  "));

        Assert.Contains("cementerio donde se esconde la flota", error.Message);
    }

    [Theory]
    [InlineData(EstadoOperativo.DadoDeBaja, "NRM-02")]
    [InlineData(EstadoOperativo.RetiradoDeFlota, "nunca fue del Estado")]
    [InlineData(EstadoOperativo.Prestado, "responsabilidad patrimonial")]
    [InlineData(EstadoOperativo.EnTaller, "indisponibilidad de flota")]
    public void Los_estados_que_exigen_acta_o_causa_la_exigen(
        EstadoOperativo estado, string esperado)
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirCausaOActa(estado, null));

        Assert.Contains(esperado, error.Message);
    }

    /// <summary>
    /// `DISPONIBLE` no exige motivo: habilitar un vehículo no es lo que hay que justificar —
    /// exigirlo llenaría el diario de «se habilitó porque sí» y devaluaría los que sí importan.
    /// </summary>
    [Fact]
    public void Disponible_no_exige_motivo() =>
        ReglasDelEstadoOperativo.ExigirCausaOActa(EstadoOperativo.Disponible, null);

    // ── Misiones abiertas y régimen de tenencia ─────────────────────────────

    [Fact]
    public void Un_vehiculo_con_misiones_abiertas_no_se_da_de_baja()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirSinMisionesAbiertas(EstadoOperativo.DadoDeBaja, 3));

        Assert.Contains("una unidad que para el sistema ya no existe", error.Message);
    }

    /// <summary>El retiro de flota también: es el otro terminal.</summary>
    [Fact]
    public void Tampoco_se_retira_de_flota_con_misiones_abiertas() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirSinMisionesAbiertas(
                EstadoOperativo.RetiradoDeFlota, 1));

    [Fact]
    public void Un_estado_no_terminal_no_mira_las_misiones_abiertas() =>
        ReglasDelEstadoOperativo.ExigirSinMisionesAbiertas(EstadoOperativo.EnTaller, 5);

    /// <summary>
    /// La corrección de `HB3-17` — <b>el descargo es de bienes propios; el retiro, de ajenos</b>.
    ///
    /// §10.2: declarar <i>«dado de baja del registro de bienes del Estado»</i> un vehículo en
    /// comodato es <i>«un asiento falso sobre un bien ajeno, detectable cruzando el inventario
    /// institucional contra el padrón de flota»</i>.
    /// </summary>
    [Fact]
    public void Un_bien_ajeno_no_se_descarga_del_registro_de_bienes_del_Estado()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirTerminalDelRegimenCorrecto(
                EstadoOperativo.DadoDeBaja, esBienPropio: false));

        Assert.Contains("asiento falso sobre un bien ajeno", error.Message);
        Assert.Contains("RETIRADO_DE_FLOTA", error.Message);
    }

    [Fact]
    public void Un_bien_propio_no_se_retira_de_flota()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelEstadoOperativo.ExigirTerminalDelRegimenCorrecto(
                EstadoOperativo.RetiradoDeFlota, esBienPropio: true));

        Assert.Contains("sale del registro por DESCARGO", error.Message);
    }

    [Fact]
    public void Cada_terminal_con_su_regimen_pasa()
    {
        Assert.Null(ReglasDelEstadoOperativo.ExigirTerminalDelRegimenCorrecto(
            EstadoOperativo.DadoDeBaja, esBienPropio: true));

        Assert.Null(ReglasDelEstadoOperativo.ExigirTerminalDelRegimenCorrecto(
            EstadoOperativo.RetiradoDeFlota, esBienPropio: false));
    }

    /// <summary>
    /// <b>Sin régimen declarado se advierte, no se bloquea.</b> El régimen de tenencia no está
    /// cargado para toda la flota, y bloquear el descargo de todas las unidades por un dato de
    /// alta que nadie llenó sería peor que el asiento que se quiere evitar.
    /// </summary>
    [Fact]
    public void Sin_regimen_declarado_se_advierte_y_no_se_bloquea()
    {
        var advertencia = ReglasDelEstadoOperativo.ExigirTerminalDelRegimenCorrecto(
            EstadoOperativo.DadoDeBaja, esBienPropio: null);

        Assert.NotNull(advertencia);
        Assert.Contains("no está declarado", advertencia);
    }

    [Fact]
    public void Un_estado_no_terminal_no_mira_el_regimen() =>
        Assert.Null(ReglasDelEstadoOperativo.ExigirTerminalDelRegimenCorrecto(
            EstadoOperativo.Prestado, esBienPropio: null));
}
