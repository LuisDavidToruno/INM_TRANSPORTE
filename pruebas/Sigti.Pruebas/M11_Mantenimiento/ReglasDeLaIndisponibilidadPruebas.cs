using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M11_Mantenimiento;

namespace Sigti.Pruebas.M11_Mantenimiento;

/// <summary>
/// `RN-60` — la indisponibilidad sobrevenida exige causa, ventana y acuse, y deja las reservas
/// afectadas en conflicto hasta que alguien les dé un desenlace.
///
/// <i>«Una reserva en conflicto <b>no expira en silencio</b> ni se resuelve por el paso del
/// tiempo»</i>.
/// </summary>
public class ReglasDeLaIndisponibilidadPruebas
{
    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly Ulid MisionA = Ulid.NewUlid();
    private static readonly Ulid MisionB = Ulid.NewUlid();

    private static readonly DateOnly Desde = new(2026, 5, 11);
    private static readonly DateOnly FinEstimado = new(2026, 5, 25);

    // ── Causa, ventana y acuse ──────────────────────────────────────────────

    [Fact]
    public void Una_indisponibilidad_completa_pasa() =>
        ReglasDeLaIndisponibilidad.ExigirCausaVentanaYAcuse(
            EstadoOperativo.EnTaller, "Cambio de embrague", Desde, FinEstimado,
            "P-TRANSPORTE", [Reserva(MisionA)]);

    /// <summary>
    /// La lista <b>puede ir vacía</b> —hay vehículos sin reservas— y eso es distinto de no haber
    /// mirado: la lista vacía también se conserva.
    /// </summary>
    [Fact]
    public void Un_vehiculo_sin_reservas_tambien_se_puede_declarar_indisponible() =>
        ReglasDeLaIndisponibilidad.ExigirCausaVentanaYAcuse(
            EstadoOperativo.EnTaller, "Cambio de embrague", Desde, FinEstimado,
            "P-TRANSPORTE", []);

    [Fact]
    public void Sin_causa_tipificada_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirCausaVentanaYAcuse(
                EstadoOperativo.EnTaller, "  ", Desde, FinEstimado, "P-TRANSPORTE", []));

        Assert.Equal("RN-60", error.Precondicion);
        Assert.Contains("indistinguible de uno que nadie usó", error.Message);
    }

    [Fact]
    public void Sin_quien_la_declara_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirCausaVentanaYAcuse(
                EstadoOperativo.EnTaller, "Cambio de embrague", Desde, FinEstimado, "", []));

    /// <summary>
    /// Sin fecha de fin no hay contra qué contrastar la real, y el indicador de gestión del
    /// taller que `RN-60` punto 6 pide se queda sin la mitad de su cuenta.
    /// </summary>
    [Fact]
    public void Una_ventana_que_termina_antes_de_empezar_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirCausaVentanaYAcuse(
                EstadoOperativo.EnTaller, "Cambio de embrague", FinEstimado, Desde,
                "P-TRANSPORTE", []));

    /// <summary>
    /// Esta regla cubre las transiciones a estados que <b>no habilitan asignación</b>. Aplicarla
    /// a `DISPONIBLE` exigiría acuse sobre reservas afectadas al dar de alta un vehículo, que es
    /// lo contrario de lo que pasa.
    /// </summary>
    [Theory]
    [InlineData(EstadoOperativo.Disponible)]
    [InlineData(EstadoOperativo.EnMision)]
    public void Los_estados_que_SI_habilitan_asignacion_no_son_indisponibilidad(
        EstadoOperativo estado)
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirCausaVentanaYAcuse(
                estado, "Causa", Desde, FinEstimado, "P-TRANSPORTE", []));

        Assert.Contains("no es un estado de indisponibilidad", error.Message);
    }

    // ── La marca impide el despacho ─────────────────────────────────────────

    /// <summary>
    /// `RN-60` — la marca de conflicto <b>impide el despacho</b> y obliga a un desenlace, no a
    /// esperar.
    /// </summary>
    [Fact]
    public void Una_reserva_en_conflicto_impide_el_despacho()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirSinConflicto(
                new ConflictoPorIndisponibilidad(true, "Cambio de embrague", FinEstimado)));

        Assert.Equal("RN-60", error.Precondicion);
        Assert.Contains("Cambio de embrague", error.Message);
        Assert.Contains("no el paso del tiempo", error.Message);
    }

    [Fact]
    public void Sin_conflicto_el_despacho_procede() =>
        ReglasDeLaIndisponibilidad.ExigirSinConflicto(ConflictoPorIndisponibilidad.Ninguno);

    // ── El desenlace de la reserva ──────────────────────────────────────────

    /// <summary>
    /// La lista se conserva como se presentó: agregarle una misión después haría que el acuse
    /// cubriera algo que quien ejecutó no vio.
    /// </summary>
    [Fact]
    public void Una_mision_que_no_estaba_en_la_lista_no_se_puede_resolver()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirDesenlaceRegistrable(
                Indisponibilidad(), MisionB, "Se le asignó el INS-P-021"));

        Assert.Contains("quien ejecutó no vio", error.Message);
    }

    [Fact]
    public void Un_segundo_desenlace_sobre_la_misma_reserva_no_pasa()
    {
        var resuelta = Indisponibilidad() with
        {
            Resoluciones =
            [
                new ResolucionDeLaReserva(
                    MisionA, DesenlaceDeLaReserva.Reprogramar, "P-TRANSPORTE",
                    new DateTimeOffset(2026, 5, 12, 9, 0, 0, TimeSpan.FromHours(-6)),
                    "Se movió al 2 de junio"),
            ],
        };

        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirDesenlaceRegistrable(resuelta, MisionA, "Otra cosa"));

        Assert.Contains("borraría el que constaba", error.Message);
    }

    [Fact]
    public void Un_desenlace_sin_motivo_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirDesenlaceRegistrable(Indisponibilidad(), MisionA, " "));

    /// <summary>
    /// <b>No expiran en silencio.</b> Sin desenlace registrado, la reserva sigue en conflicto
    /// aunque su ventana ya haya pasado.
    /// </summary>
    [Fact]
    public void Las_reservas_sin_desenlace_siguen_en_conflicto()
    {
        Assert.Single(Indisponibilidad().SinDesenlace);

        var resuelta = Indisponibilidad() with
        {
            Resoluciones =
            [
                new ResolucionDeLaReserva(
                    MisionA, DesenlaceDeLaReserva.SustituirVehiculo, "P-TRANSPORTE",
                    new DateTimeOffset(2026, 5, 12, 9, 0, 0, TimeSpan.FromHours(-6)),
                    "Se le asignó el INS-P-021"),
            ],
        };

        Assert.Empty(resuelta.SinDesenlace);
    }

    // ── El alta ─────────────────────────────────────────────────────────────

    [Fact]
    public void El_alta_con_orden_de_trabajo_y_odometro_pasa() =>
        ReglasDeLaIndisponibilidad.ExigirAltaConOrdenYOdometro(
            Indisponibilidad(), new DateOnly(2026, 5, 28), "OT-2026-114", 94_300);

    /// <summary>
    /// Sin la orden de trabajo, el vehículo vuelve a la flota sin que conste qué se le hizo
    /// mientras estuvo parado.
    /// </summary>
    [Fact]
    public void El_alta_sin_orden_de_trabajo_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirAltaConOrdenYOdometro(
                Indisponibilidad(), new DateOnly(2026, 5, 28), "  ", 94_300));

        Assert.Contains("sin que conste qué se le hizo", error.Message);
    }

    [Fact]
    public void El_alta_de_un_vehiculo_ya_dado_de_alta_no_pasa()
    {
        var dada = Indisponibilidad() with { FinReal = new DateOnly(2026, 5, 20) };

        Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaIndisponibilidad.ExigirAltaConOrdenYOdometro(
                dada, new DateOnly(2026, 5, 28), "OT-2026-114", 94_300));
    }

    /// <summary>
    /// `RN-60` punto 6 — <i>«la desviación sistemática entre estimado y real es indicador de la
    /// gestión del taller»</i>.
    ///
    /// <b>Nula mientras el vehículo no vuelva</b>: sin fecha real no hay desviación que medir, y
    /// suponerla haría que el indicador midiera estimaciones contra sí mismas.
    /// </summary>
    [Fact]
    public void La_desviacion_contra_lo_estimado_se_mide_al_dar_de_alta()
    {
        Assert.Null(Indisponibilidad().DesviacionEnDias);

        // Tres días más de lo estimado.
        var tarde = Indisponibilidad() with { FinReal = FinEstimado.AddDays(3) };
        Assert.Equal(3, tarde.DesviacionEnDias);

        // Y dos menos: la desviación también mide lo que se adelantó.
        var temprano = Indisponibilidad() with { FinReal = FinEstimado.AddDays(-2) };
        Assert.Equal(-2, temprano.DesviacionEnDias);
    }

    [Fact]
    public void Excede_lo_estimado_solo_mientras_el_vehiculo_no_vuelva()
    {
        Assert.True(Indisponibilidad().ExcedeLoEstimado(FinEstimado.AddDays(1)));
        Assert.False(Indisponibilidad().ExcedeLoEstimado(FinEstimado));

        var dada = Indisponibilidad() with { FinReal = FinEstimado };
        Assert.False(dada.ExcedeLoEstimado(FinEstimado.AddDays(30)));
    }

    // ── Andamio ─────────────────────────────────────────────────────────────

    private static ReservaAfectada Reserva(Ulid mision) =>
        new(mision, mision.ToString(), "Delegación de Choluteca",
            new DateOnly(2026, 5, 18), new DateOnly(2026, 5, 20), "P-MOTORISTA",
            "Traslado de personal", EstadoDeMision.Programada);

    private static IndisponibilidadDelVehiculo Indisponibilidad() =>
        new(Ulid.NewUlid(),
            Vehiculo,
            EstadoOperativo.EnTaller,
            "Cambio de embrague",
            Desde,
            FinEstimado,
            "P-TRANSPORTE",
            new DateTimeOffset(2026, 5, 11, 8, 30, 0, TimeSpan.FromHours(-6)),
            [Reserva(MisionA)],
            []);
}
