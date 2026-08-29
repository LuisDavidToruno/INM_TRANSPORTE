using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M19_Seguimiento;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M19_Seguimiento;

/// <summary>
/// `RN-76` y `RN-43` — qué se acepta como reporte de campo.
///
/// La precondición se juzga <b>contra el diario a la fecha del hecho</b>, no contra el estado de
/// hoy. Es `P-1` aplicado a una precondición: un dispositivo que sube cuatro días de reportes
/// acumulados encuentra la misión ya liquidada, y rechazar por eso perdería exactamente los
/// datos que el módulo existe para conservar.
/// </summary>
public class ReglasDelSeguimientoPruebas
{
    private static readonly DateTimeOffset Salida = new(2026, 5, 14, 6, 0, 0, TimeSpan.Zero);
    private static readonly IdPersona Quien = new("motorista-1");

    private static Transicion T(string id, EstadoDeMision destino, DateTimeOffset momento) =>
        new(id, destino, Quien, momento, null);

    /// <summary>Salió a las 06:00 y volvió a las 18:00 del mismo día.</summary>
    private static readonly List<Transicion> MisionQueYaVolvio =
    [
        T("T-08", EstadoDeMision.Programada, Salida.AddDays(-1)),
        T("T-14", EstadoDeMision.EnRuta, Salida),
        T("T-18", EstadoDeMision.Retornada, Salida.AddHours(12)),
        T("T-19", EstadoDeMision.Liquidada, Salida.AddDays(3)),
    ];

    private static readonly List<Transicion> MisionAfuera =
    [
        T("T-08", EstadoDeMision.Programada, Salida.AddDays(-1)),
        T("T-14", EstadoDeMision.EnRuta, Salida),
    ];

    // ── La ventana ──────────────────────────────────────────────────────────

    [Fact]
    public void La_ventana_va_de_la_salida_al_retorno()
    {
        var v = ReglasDelSeguimiento.VentanaEnRuta(MisionQueYaVolvio);

        Assert.NotNull(v);
        Assert.Equal(Salida, v.Value.Inicio);
        Assert.Equal(Salida.AddHours(12), v.Value.Fin);
    }

    [Fact]
    public void Mientras_siga_afuera_el_fin_es_nulo()
    {
        var v = ReglasDelSeguimiento.VentanaEnRuta(MisionAfuera);

        // Nulo es «todavía no volvió», no «volvió en una fecha desconocida».
        Assert.NotNull(v);
        Assert.Null(v.Value.Fin);
    }

    [Fact]
    public void El_retorno_anticipado_por_interrupcion_tambien_cierra_la_ventana()
    {
        // `T-16` es el retorno por interrupción. Mirar sólo `T-18` dejaría la ventana abierta
        // para siempre en toda misión que se interrumpió, que es cuando más importa.
        List<Transicion> interrumpida =
        [
            T("T-14", EstadoDeMision.EnRuta, Salida),
            T("T-16", EstadoDeMision.Retornada, Salida.AddHours(4)),
        ];

        Assert.Equal(Salida.AddHours(4),
            ReglasDelSeguimiento.VentanaEnRuta(interrumpida)!.Value.Fin);
    }

    [Fact]
    public void Sin_asiento_de_salida_no_hay_ventana()
    {
        Assert.Null(ReglasDelSeguimiento.VentanaEnRuta(
            [T("T-08", EstadoDeMision.Programada, Salida.AddDays(-1))]));
    }

    // ── La precondición que no mira el estado de hoy ────────────────────────

    [Fact]
    public void Un_reporte_atrasado_se_acepta_aunque_la_mision_ya_este_liquidada()
    {
        // El caso que define el módulo: el dispositivo estuvo sin cobertura, la misión se
        // liquidó, y recién ahora sube lo que registró en ruta. El hecho ocurrió dentro de la
        // ventana; el estado de hoy no tiene nada que decir al respecto.
        ReglasDelSeguimiento.ExigirQueEstuvieraEnRuta(
            MisionQueYaVolvio, Salida.AddHours(5));
    }

    [Fact]
    public void Un_hecho_anterior_a_la_salida_se_rechaza_y_dice_por_que()
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirQueEstuvieraEnRuta(
                MisionAfuera, Salida.AddHours(-3)));

        // El motivo real casi siempre es el reloj del dispositivo, y «fuera de rango» no le
        // sirve a nadie para arreglarlo.
        Assert.Contains("reloj", e.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("3 hora(s)", e.Message);
    }

    [Fact]
    public void Un_hecho_posterior_al_retorno_se_rechaza()
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirQueEstuvieraEnRuta(
                MisionQueYaVolvio, Salida.AddHours(20)));

        Assert.Contains("después del retorno", e.Message);
    }

    [Fact]
    public void Sobre_una_mision_que_nunca_salio_no_hay_nada_que_reportar()
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirQueEstuvieraEnRuta(
                [T("T-08", EstadoDeMision.Programada, Salida)], Salida.AddHours(1)));

        Assert.Contains("nunca inició ruta", e.Message);
    }

    [Fact]
    public void Mientras_siga_afuera_cualquier_hecho_posterior_a_la_salida_pasa()
    {
        ReglasDelSeguimiento.ExigirQueEstuvieraEnRuta(MisionAfuera, Salida.AddDays(4));
    }

    // ── El catálogo cerrado ─────────────────────────────────────────────────

    private static readonly IReadOnlySet<string> Catalogo =
        new HashSet<string> { "en marcha", "en espera", "cargando o descargando" };

    [Fact]
    public void El_estado_tiene_que_venir_del_catalogo()
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirEstadoDelCatalogo("detenido por lluvia", Catalogo));

        // El mensaje enumera los válidos: un rechazo que no dice cuáles son obliga a adivinar.
        Assert.Contains("en marcha", e.Message);
    }

    [Fact]
    public void Con_el_catalogo_vacio_no_se_acepta_texto_libre()
    {
        // Aceptar cualquier cosa «mientras tanto» llenaría el histórico de variantes que
        // después nadie puede agrupar, y el catálogo llegaría tarde.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirEstadoDelCatalogo("en marcha", new HashSet<string>()));

        Assert.Contains("vacío", e.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void La_declaracion_de_estado_exige_un_estado(string? estado)
    {
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirEstadoDelCatalogo(estado, Catalogo));
    }

    // ── El destino ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(TipoDeReporte.Arribo)]
    [InlineData(TipoDeReporte.Salida)]
    public void Arribo_y_salida_exigen_destino(TipoDeReporte tipo)
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirDestino(tipo, null));

        Assert.Contains("atribuir", e.Message);
    }

    [Fact]
    public void La_declaracion_de_estado_no_exige_destino()
    {
        // Se declara en cualquier punto de la ruta, no sólo en un destino.
        ReglasDelSeguimiento.ExigirDestino(TipoDeReporte.EstadoDeclarado, null);
    }

    // ── La posición ─────────────────────────────────────────────────────────

    [Fact]
    public void Sin_posicion_se_registra_sin_posicion()
    {
        // Que el dispositivo no tenga GPS no puede impedir declarar el estado: `RN-76` pide un
        // toque sin conectividad, y la posición es lo primero que falta.
        ReglasDelSeguimiento.ExigirPosicionUsable(null);
    }

    [Fact]
    public void El_cero_cero_se_rechaza_aunque_este_dentro_de_los_rangos()
    {
        // Es lo que informa un GPS que todavía no fijó, y cae en el Golfo de Guinea. Guardarlo
        // pondría la flota en el Atlántico y —peor— la haría ver localizada.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirPosicionUsable(new Posicion(0, 0, 10)));

        Assert.Contains("no fijó", e.Message);
    }

    [Theory]
    [InlineData(91, -87)]
    [InlineData(-91, -87)]
    [InlineData(14, 181)]
    [InlineData(14, -181)]
    public void Fuera_de_los_rangos_posibles_se_rechaza(int lat, int lon)
    {
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirPosicionUsable(new Posicion(lat, lon, null)));
    }

    [Fact]
    public void Una_posicion_de_Honduras_pasa_con_o_sin_precision()
    {
        ReglasDelSeguimiento.ExigirPosicionUsable(new Posicion(13.4m, -87.3m, 12));
        ReglasDelSeguimiento.ExigirPosicionUsable(new Posicion(13.4m, -87.3m, null));
    }

    [Fact]
    public void La_precision_negativa_se_rechaza()
    {
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSeguimiento.ExigirPosicionUsable(new Posicion(13.4m, -87.3m, -1)));
    }

    // ── El desfase de captura ───────────────────────────────────────────────

    [Fact]
    public void El_desfase_es_un_dato_del_reporte_y_no_un_error()
    {
        // Cuatro días entre el hecho y la captura es operación normal: mide cuánto estuvo el
        // dispositivo sin cobertura (`RN-43`).
        var reporte = new ReporteDeCampo
        {
            Id = Ulid.NewUlid(),
            MisionId = Ulid.NewUlid(),
            Tipo = TipoDeReporte.EstadoDeclarado,
            Estado = "en marcha",
            MomentoDelHecho = Salida.AddHours(2),
            MomentoDeCaptura = Salida.AddDays(4),
            Declara = Quien,
        };

        Assert.Equal(TimeSpan.FromHours(94), ReglasDelSeguimiento.DesfaseDeCaptura(reporte));
    }
}
