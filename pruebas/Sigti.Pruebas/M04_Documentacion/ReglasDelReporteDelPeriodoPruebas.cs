using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M04_Documentacion;

namespace Sigti.Pruebas.M04_Documentacion;

/// <summary>
/// `HU-020` — el reporte previo al feriado largo.
///
/// ── Por qué esto importa más de lo que parece ───────────────────────────────
/// El Tribunal Superior de Cuentas hace operativos de fiscalización vehicular <b>específicamente
/// en Semana Santa</b> `[V]`. Es el pico anual de riesgo, y es <b>predecible</b> — lo que lo
/// vuelve el caso más fácil de resolver bien y el más caro de resolver mal.
///
/// ── Y la propiedad que hace útil el reporte ─────────────────────────────────
/// <b>Las tres listas suman la flota entera.</b> Un reporte que liste sólo los que circulan deja
/// al resto invisible, y un vehículo del que nadie confirmó dónde está es exactamente lo que un
/// operativo encuentra.
/// </summary>
public class ReglasDelReporteDelPeriodoPruebas
{
    private static readonly DateOnly Inicio = new(2026, 3, 30);
    private static readonly DateOnly Fin = new(2026, 4, 5);

    private static readonly DateTimeOffset Corte =
        new(2026, 3, 26, 9, 0, 0, TimeSpan.FromHours(-6));

    // ── Quién entra al reporte ──────────────────────────────────────────────

    /// <summary>
    /// ⚠️ <b>Los dos terminales de §10.2 no son flota.</b>
    ///
    /// Pedirle a alguien que confirme dónde quedó resguardado un vehículo descargado del
    /// registro es mandarlo a una tarea que puede ser imposible. Y el daño no es la tarea de
    /// más: cada uno infla «sin confirmar», y en una institución con años de historia los tres
    /// que de verdad nadie fue a mirar quedan enterrados entre decenas.
    /// </summary>
    [Theory]
    [InlineData(EstadoOperativo.DadoDeBaja)]
    [InlineData(EstadoOperativo.RetiradoDeFlota)]
    public void El_vehiculo_en_estado_terminal_no_entra_al_reporte(EstadoOperativo terminal)
    {
        Assert.False(ReglasDelReporteDelPeriodo.EstaEnLaFlota(terminal));
    }

    /// <summary>
    /// <b>Prestado sigue siendo bien nuestro</b> y devenga responsabilidad patrimonial; el
    /// <b>taller es un lugar</b>, y un vehículo que nadie ubica no deja de estar perdido porque
    /// haya una orden de trabajo abierta.
    /// </summary>
    [Theory]
    [InlineData(EstadoOperativo.Disponible)]
    [InlineData(EstadoOperativo.EnTaller)]
    [InlineData(EstadoOperativo.Prestado)]
    [InlineData(EstadoOperativo.NoDisponible)]
    public void Los_que_siguen_siendo_flota_entran(EstadoOperativo estado)
    {
        Assert.True(ReglasDelReporteDelPeriodo.EstaEnLaFlota(estado));
    }

    /// <summary>
    /// <b>Nulo entra.</b> «Nunca se declaró estado» no es «no es flota»: es un vehículo del que
    /// se sabe todavía menos, y esconderlo sería lo contrario de lo que el reporte hace.
    /// </summary>
    [Fact]
    public void El_que_nunca_declaro_estado_entra_igual()
    {
        Assert.True(ReglasDelReporteDelPeriodo.EstaEnLaFlota(null));
    }

    // ── Situar cada vehículo ────────────────────────────────────────────────

    [Fact]
    public void Con_mision_en_el_periodo_va_a_la_lista_de_permisos()
    {
        Assert.Equal(
            SituacionEnElPeriodo.ConPermisoPropuesto,
            ReglasDelReporteDelPeriodo.Situar(null, Inicio, tieneMisionEnElPeriodo: true));
    }

    [Fact]
    public void Sin_mision_queda_a_resguardar()
    {
        Assert.Equal(
            SituacionEnElPeriodo.AResguardar,
            ReglasDelReporteDelPeriodo.Situar(null, Inicio, tieneMisionEnElPeriodo: false));
    }

    /// <summary>
    /// ⚠️ <b>La excepción se evalúa primero</b> — `RN-24`.
    ///
    /// Un exceptuado no circula «con permiso» ni queda «a resguardar»: clasificarlo en
    /// cualquiera de las otras dos produciría una firma que la regla dice que no hace falta, o
    /// una confirmación de resguardo que nadie debe dar sobre una ambulancia que está saliendo.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void El_exceptuado_va_aparte_tenga_mision_o_no(bool tieneMision)
    {
        var excepcion = new ServicioExceptuado(
            "AMBULANCIA", "Acuerdo 44-2019", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));

        Assert.Equal(
            SituacionEnElPeriodo.Exceptuado,
            ReglasDelReporteDelPeriodo.Situar(excepcion, Inicio, tieneMision));
    }

    /// <summary>Una excepción vencida al inicio del período no exime — `P-4`.</summary>
    [Fact]
    public void La_excepcion_vencida_al_inicio_del_periodo_no_exime()
    {
        var vencida = new ServicioExceptuado(
            "AMBULANCIA", "Acuerdo 44-2019", new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

        Assert.Equal(
            SituacionEnElPeriodo.AResguardar,
            ReglasDelReporteDelPeriodo.Situar(vencida, Inicio, tieneMisionEnElPeriodo: false));
    }

    // ── Que el reporte cuadre ───────────────────────────────────────────────

    [Fact]
    public void Con_las_tres_listas_sumando_la_flota_el_reporte_cuadra()
    {
        var reporte = Reporte(circulan: 5, resguardar: 11, exceptuados: 2);

        Assert.Null(ReglasDelReporteDelPeriodo.PorQueNoCuadra(reporte, vehiculosDeLaFlota: 18));
    }

    /// <summary>
    /// ⚠️ <b>Un reporte que no cuadra deja vehículos invisibles</b>, y uno del que nadie
    /// confirmó dónde está es exactamente lo que un operativo encuentra.
    /// </summary>
    [Fact]
    public void Si_falta_un_vehiculo_el_reporte_no_cuadra_y_lo_dice()
    {
        var reporte = Reporte(circulan: 5, resguardar: 10, exceptuados: 2);

        var porQue = ReglasDelReporteDelPeriodo.PorQueNoCuadra(reporte, vehiculosDeLaFlota: 18);

        Assert.NotNull(porQue);
        Assert.Contains("suman 17", porQue);
        Assert.Contains("18", porQue);
        Assert.Contains("deja vehículos invisibles", porQue);
    }

    /// <summary>Las tres situaciones son <b>excluyentes</b>: uno repetido también rompe la suma.</summary>
    [Fact]
    public void Un_vehiculo_en_dos_listas_no_cuadra()
    {
        var repetido = Ulid.NewUlid();

        var reporte = Reporte(circulan: 1, resguardar: 1, exceptuados: 0) with
        {
            Vehiculos =
            [
                Uno(repetido, SituacionEnElPeriodo.ConPermisoPropuesto),
                Uno(repetido, SituacionEnElPeriodo.AResguardar),
            ],
        };

        var porQue = ReglasDelReporteDelPeriodo.PorQueNoCuadra(reporte, vehiculosDeLaFlota: 2);

        Assert.NotNull(porQue);
        Assert.Contains("más de una lista", porQue);
    }

    // ── El orden, que es la mitad del valor ─────────────────────────────────

    /// <summary>
    /// <b>Los no confirmados primero.</b> Un reporte que lista dieciocho vehículos en orden
    /// alfabético obliga a buscar los tres que importan, y a las cinco de la tarde del jueves
    /// santo nadie los busca.
    /// </summary>
    [Fact]
    public void Los_resguardos_sin_confirmar_van_arriba()
    {
        var reporte = new ReporteDelPeriodo(Inicio, Fin, Corte,
        [
            Uno(Ulid.NewUlid(), SituacionEnElPeriodo.AResguardar, "AAA-001",
                EstadoDelResguardo.Confirmado),
            Uno(Ulid.NewUlid(), SituacionEnElPeriodo.AResguardar, "ZZZ-999",
                EstadoDelResguardo.NoConfirmado),
        ], []);

        Assert.Equal("ZZZ-999", reporte.Resguardados[0].Identificacion);
        Assert.Equal(1, reporte.SinConfirmar);
    }

    /// <summary>
    /// <b>«Cinco propuestos» y «cinco firmables» no son lo mismo.</b> La máxima autoridad
    /// necesita saber cuántos va a resolver antes de sentarse.
    /// </summary>
    [Fact]
    public void Los_firmables_se_cuentan_aparte_de_los_propuestos()
    {
        var reporte = new ReporteDelPeriodo(Inicio, Fin, Corte,
        [
            Uno(Ulid.NewUlid(), SituacionEnElPeriodo.ConPermisoPropuesto),
            Uno(Ulid.NewUlid(), SituacionEnElPeriodo.ConPermisoPropuesto) with
            {
                PorQueNoSeFirma = "El permiso es nominativo sobre vehículo, ruta y ventana.",
            },
        ], []);

        Assert.Equal(2, reporte.Circulan.Count);
        Assert.Equal(1, reporte.Firmables);
    }

    /// <summary>
    /// ⚠️ <b>El firmado deja de contar como firmable.</b>
    ///
    /// Es la mitad que se olvida: sin descontarlo, la cifra no baja al firmar y <b>la sesión de
    /// firma no termina nunca</b> — quien firma vuelve a abrirla creyendo que quedaron permisos
    /// pendientes, y encuentra los mismos.
    ///
    /// Y el firmado que sigue amparando <b>no lleva motivo</b>: «ya está firmado» y «no se puede
    /// firmar» dicen lo mismo y son cosas opuestas, y una pantalla que sólo mire el motivo pinta
    /// de rojo, con mensaje de bloqueo, justamente lo que ya se resolvió.
    /// </summary>
    [Fact]
    public void El_permiso_firmado_no_cuenta_entre_los_firmables()
    {
        var reporte = new ReporteDelPeriodo(Inicio, Fin, Corte,
        [
            Uno(Ulid.NewUlid(), SituacionEnElPeriodo.ConPermisoPropuesto) with
            {
                Firmado = true,
            },
            Uno(Ulid.NewUlid(), SituacionEnElPeriodo.ConPermisoPropuesto),
        ], []);

        Assert.Equal(2, reporte.Circulan.Count);
        Assert.Equal(1, reporte.Firmables);
    }

    /// <summary>
    /// Un permiso firmado <b>que ya no cubre</b> sí lleva motivo, y tampoco es firmable: lo que
    /// hace falta es reemitirlo, no volver a firmarlo.
    /// </summary>
    [Fact]
    public void El_firmado_que_ya_no_cubre_tampoco_es_firmable()
    {
        var reporte = new ReporteDelPeriodo(Inicio, Fin, Corte,
        [
            Uno(Ulid.NewUlid(), SituacionEnElPeriodo.ConPermisoPropuesto) with
            {
                Firmado = true,
                PorQueNoSeFirma = "El vehículo amparado no es el asignado hoy.",
            },
        ], []);

        Assert.Equal(0, reporte.Firmables);
    }

    // ── La confirmación de resguardo ────────────────────────────────────────

    [Fact]
    public void Con_evidencia_y_predio_se_confirma()
    {
        Assert.Null(ReglasDelReporteDelPeriodo.PorQueNoSeConfirma(
            tieneEvidencia: true, "Predio de la sede central"));
    }

    /// <summary>
    /// <b>Sin evidencia no vale.</b> Es la misma disciplina de `RN-18`: sin ella lo único que
    /// queda registrado es que alguien dijo que el vehículo estaba ahí.
    /// </summary>
    [Fact]
    public void Sin_evidencia_no_se_confirma()
    {
        var porQue = ReglasDelReporteDelPeriodo.PorQueNoSeConfirma(
            tieneEvidencia: false, "Predio de la sede central");

        Assert.NotNull(porQue);
        Assert.Contains("alguien dijo que el vehículo estaba ahí", porQue);
    }

    /// <summary>
    /// <b>«Confirmado» sin lugar no contesta la pregunta que el reporte hace</b>, que es dónde
    /// está cada vehículo.
    /// </summary>
    [Fact]
    public void Sin_predio_tampoco_se_confirma()
    {
        var porQue = ReglasDelReporteDelPeriodo.PorQueNoSeConfirma(tieneEvidencia: true, "  ");

        Assert.NotNull(porQue);
        Assert.Contains("dónde está cada vehículo", porQue);
    }

    // ── Andamios ────────────────────────────────────────────────────────────

    private static ReporteDelPeriodo Reporte(int circulan, int resguardar, int exceptuados) =>
        new(Inicio, Fin, Corte,
        [
            .. Enumerable.Range(0, circulan)
                .Select(_ => Uno(Ulid.NewUlid(), SituacionEnElPeriodo.ConPermisoPropuesto)),
            .. Enumerable.Range(0, resguardar)
                .Select(_ => Uno(Ulid.NewUlid(), SituacionEnElPeriodo.AResguardar)),
            .. Enumerable.Range(0, exceptuados)
                .Select(_ => Uno(Ulid.NewUlid(), SituacionEnElPeriodo.Exceptuado)),
        ], []);

    private static VehiculoEnElPeriodo Uno(
        Ulid id, SituacionEnElPeriodo situacion, string? siglas = null,
        EstadoDelResguardo? resguardo = null) =>
        new(id, siglas ?? $"INS-{id.ToString()[^4..]}", situacion,
            null, null, null,
            resguardo ?? (situacion == SituacionEnElPeriodo.AResguardar
                ? EstadoDelResguardo.NoConfirmado
                : null),
            null, null, null);
}
