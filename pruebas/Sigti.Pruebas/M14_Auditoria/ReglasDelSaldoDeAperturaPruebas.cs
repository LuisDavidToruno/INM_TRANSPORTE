using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// `RN-97` — el saldo de apertura de control interno.
///
/// ── La regla que impide el abandono ──────────────────────────────────────────
/// <i>«Sin saldo de apertura, el mecanismo de olvido es automático y no requiere mala fe: llega
/// enero, el sistema arranca con reportes en cero, y una misión interrumpida en noviembre, un
/// préstamo vencido en agosto y una obligación de reintegro de mayo simplemente dejan de
/// aparecer en ninguna pantalla. <b>Nadie decidió abandonarlos: se abandonaron solos</b>»</i>.
/// </summary>
public class ReglasDelSaldoDeAperturaPruebas
{
    private static readonly DateOnly Corte2026 = new(2026, 12, 31);
    private static readonly DateOnly Corte2027 = new(2027, 12, 31);
    private static readonly DateOnly Corte2028 = new(2028, 12, 31);

    private static readonly DateTimeOffset Ahora =
        new(2027, 1, 5, 9, 0, 0, TimeSpan.FromHours(-6));

    private static RenglonDelSaldo Renglon(
        TipoDeRenglon tipo = TipoDeRenglon.ObligacionDeReintegro,
        string referencia = "OR-2026-0011",
        DateOnly? hecho = null,
        string responsable = "P-AUDITORIA",
        int anteriores = 0,
        CausaDelRenglon causa = CausaDelRenglon.PendienteDeGestionInterna) =>
        new(tipo, referencia, "Faltante sin causa identificada",
            hecho ?? new DateOnly(2026, 5, 12), causa, responsable, "Abierto", anteriores);

    private static SaldoDeApertura Saldo(
        IReadOnlyList<RenglonDelSaldo> renglones,
        DateOnly? corte = null,
        bool inicial = false) =>
        new(Ulid.NewUlid(), "SA-2027-001", "2027", corte ?? Corte2026, renglones,
            [new FuenteDelSaldo(TipoDeRenglon.ObligacionDeReintegro, true, renglones.Count)],
            Autoria.De(new IdPersona("P-AUDITORIA"), new IdPuesto("PU-AUDITORIA"), Corte2026),
            Ahora, inicial);

    // ── La antigüedad no se reinicia ────────────────────────────────────────

    [Fact]
    public void La_antiguedad_se_cuenta_desde_el_HECHO_no_desde_el_corte()
    {
        // `RN-97` punto 3. Es «la parte que hace incómoda a la regla, y por eso mismo la que
        // sirve».
        var r = Renglon(hecho: new DateOnly(2026, 5, 12));

        Assert.Equal(233, r.AntiguedadEnDias(Corte2026));
        Assert.Equal(598, r.AntiguedadEnDias(Corte2027));
                // 964 y no 963: 2028 es bisiesto. La cuenta la hace `DayNumber`, no una división.
        Assert.Equal(964, r.AntiguedadEnDias(Corte2028));
    }

    [Fact]
    public void Un_renglon_que_llega_al_tercer_ejercicio_NO_se_ve_como_reciente()
    {
        // «Un expediente que llega al tercer ejercicio con 800 días de antigüedad no se puede
        // presentar como pendiente reciente».
        RenglonDelSaldo[] primero = [Renglon()];
        var segundo = ReglasDelSaldoDeApertura.ArrastrarDesde([Renglon()], primero);
        var tercero = ReglasDelSaldoDeApertura.ArrastrarDesde([Renglon()], segundo);

        Assert.Equal(0, primero[0].SaldosAnteriores);
        Assert.Equal(1, segundo[0].SaldosAnteriores);
        Assert.Equal(2, tercero[0].SaldosAnteriores);

        // Y la antigüedad sigue corriendo desde el hecho, no desde cada corte.
        Assert.Equal(964, tercero[0].AntiguedadEnDias(Corte2028));
    }

    [Fact]
    public void El_arrastre_conserva_la_fecha_del_hecho_del_saldo_ANTERIOR()
    {
        // Si la fuente hoy reporta otra fecha, la que vale es la de la primera vez: la
        // antigüedad no se reinicia ni siquiera por una corrección de dato.
        var anterior = new[] { Renglon(hecho: new DateOnly(2026, 5, 12)) };

        var nuevo = ReglasDelSaldoDeApertura.ArrastrarDesde(
            [Renglon(hecho: new DateOnly(2027, 11, 1))], anterior);

        Assert.Equal(new DateOnly(2026, 5, 12), nuevo[0].FechaDelHecho);
    }

    [Fact]
    public void Un_renglon_NUEVO_no_arrastra_nada()
    {
        var nuevo = ReglasDelSaldoDeApertura.ArrastrarDesde(
            [Renglon(referencia: "OR-2027-0044")], [Renglon(referencia: "OR-2026-0011")]);

        Assert.Equal(0, nuevo[0].SaldosAnteriores);
    }

    [Fact]
    public void El_arrastre_casa_por_tipo_y_referencia_sin_importar_la_caja()
    {
        var anterior = new[] { Renglon(referencia: "or-2026-0011") };
        var nuevo = ReglasDelSaldoDeApertura.ArrastrarDesde(
            [Renglon(referencia: "OR-2026-0011")], anterior);

        Assert.Equal(1, nuevo[0].SaldosAnteriores);
    }

    [Fact]
    public void Dos_renglones_del_MISMO_id_pero_distinto_tipo_no_se_confunden()
    {
        // La referencia sola no identifica: una misión y un vale pueden compartir correlativo.
        var anterior = new[] { Renglon(TipoDeRenglon.MisionSinCerrar, "R-001") };

        var nuevo = ReglasDelSaldoDeApertura.ArrastrarDesde(
            [Renglon(TipoDeRenglon.ValeSinLiquidar, "R-001")], anterior);

        Assert.Equal(0, nuevo[0].SaldosAnteriores);
    }

    // ── El responsable ──────────────────────────────────────────────────────

    [Fact]
    public void Un_renglon_SIN_responsable_se_rechaza()
    {
        // «Un expediente sin responsable es un expediente muerto». Y si quien lo tenía ya no
        // está, se reasigna a la jefatura — no se borra ni se deja huérfano.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSaldoDeApertura.ExigirResponsable(
                TipoDeRenglon.PrestamoVencido, "PR-0007", "   "));

        Assert.Contains("un expediente muerto", error.Message);
        Assert.Contains("se reasigna a la jefatura", error.Message);
    }

    // ── El bloqueo del cierre ───────────────────────────────────────────────

    [Fact]
    public void Un_prestamo_vencido_IMPIDE_cerrar_el_periodo()
    {
        // `RN-97` punto 4: ningún período se cierra con préstamos vencidos ni con
        // interrupciones sin desenlace.
        var bloqueantes = new[]
        {
            Renglon(TipoDeRenglon.PrestamoVencido, "PR-0007"),
            Renglon(TipoDeRenglon.InterrupcionSinDesenlace, "OM-2026-0468"),
        };

        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSaldoDeApertura.ExigirCierrePosible(bloqueantes, null));

        Assert.Contains("PrestamoVencido «PR-0007»", error.Message);
        Assert.Contains("InterrupcionSinDesenlace", error.Message);
        Assert.Contains("a cargo de P-AUDITORIA", error.Message);
    }

    [Fact]
    public void Se_pueden_declarar_explicitamente_y_eso_NO_es_ignorarlos()
    {
        // La regla dice «hay que resolverlos o declararlos explícitamente». Declararlos es un
        // acto con motivo que queda en el documento.
        ReglasDelSaldoDeApertura.ExigirCierrePosible(
            [Renglon(TipoDeRenglon.PrestamoVencido, "PR-0007")],
            "El comodante no ha respondido a tres requerimientos. Oficio GA-2026-0912.");
    }

    [Fact]
    public void Los_demas_renglones_NO_impiden_cerrar()
    {
        // Una obligación de reintegro abierta arrastra al saldo, pero no detiene el ejercicio:
        // detenerlo por todo lo vivo haría que ningún período cerrara nunca.
        ReglasDelSaldoDeApertura.ExigirCierrePosible(
            [Renglon(TipoDeRenglon.ObligacionDeReintegro, "OR-0011")], null);

        Assert.False(Renglon(TipoDeRenglon.ObligacionDeReintegro).ImpideCerrarElPeriodo);
        Assert.True(Renglon(TipoDeRenglon.PrestamoVencido).ImpideCerrarElPeriodo);
    }

    // ── El documento ────────────────────────────────────────────────────────

    [Fact]
    public void El_saldo_exige_folio_y_ejercicio()
    {
        Assert.Contains("no se puede citar en el acta", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSaldoDeApertura.ExigirFolioYEjercicio("  ", "2027")).Message);

        Assert.Contains("la serie histórica no se puede ordenar", Assert.Throws<BloqueoDuro>(() =>
            ReglasDelSaldoDeApertura.ExigirFolioYEjercicio("SA-2027-001", " ")).Message);
    }

    // ── Coincidir con el inventario ─────────────────────────────────────────

    [Fact]
    public void Lo_que_esta_vivo_y_no_figura_en_el_saldo_se_nombra()
    {
        var saldo = new[] { Renglon(referencia: "OR-0011") };
        var inventario = new[] { Renglon(referencia: "OR-0011"), Renglon(referencia: "OR-0022") };

        var diferencias = ReglasDelSaldoDeApertura.DiferenciasContraElInventario(
            saldo, inventario);

        Assert.Single(diferencias);
        Assert.Contains("«OR-0022» está vivo y no figura en el saldo", diferencias[0]);
    }

    [Fact]
    public void Lo_que_figura_en_el_saldo_y_ya_no_esta_vivo_tambien()
    {
        var saldo = new[] { Renglon(referencia: "OR-0011"), Renglon(referencia: "OR-0022") };
        var inventario = new[] { Renglon(referencia: "OR-0011") };

        var diferencias = ReglasDelSaldoDeApertura.DiferenciasContraElInventario(
            saldo, inventario);

        Assert.Contains("«OR-0022» figura en el saldo y ya no está vivo", diferencias[0]);
    }

    [Fact]
    public void Un_saldo_que_coincide_no_produce_diferencias()
    {
        var renglones = new[] { Renglon(referencia: "OR-0011") };

        Assert.Empty(ReglasDelSaldoDeApertura.DiferenciasContraElInventario(
            renglones, renglones));
    }

    // ── El documento como conjunto ──────────────────────────────────────────

    [Fact]
    public void Un_saldo_con_fuentes_sin_consultar_NO_es_completo()
    {
        // Un saldo que omite en silencio los préstamos vencidos **es el abandono que la regla
        // existe para impedir, con formato de reporte**.
        var conHueco = Saldo([Renglon()]) with
        {
            Fuentes =
            [
                new FuenteDelSaldo(TipoDeRenglon.ObligacionDeReintegro, true, 1),
                new FuenteDelSaldo(TipoDeRenglon.PrestamoVencido, false, 0,
                    "`RN-63` no está construida: el expediente de préstamo no existe."),
            ],
        };

        Assert.False(conHueco.EsCompleto);
        Assert.Single(conHueco.SinConsultar);
        Assert.Contains("no está construida", conHueco.SinConsultar[0].PorQueNo);
    }

    [Fact]
    public void El_saldo_nombra_su_renglon_mas_VIEJO()
    {
        // Es la cifra que un auditor mira primero.
        var s = Saldo([
            Renglon(referencia: "A", hecho: new DateOnly(2026, 11, 1)),
            Renglon(referencia: "B", hecho: new DateOnly(2026, 5, 12)),
        ]);

        Assert.Equal(233, s.AntiguedadMaximaEnDias);
    }

    [Fact]
    public void Los_ARRASTRADOS_se_pueden_leer_aparte()
    {
        // Son los que más importan: el arrastre es justamente lo que la regla existe para hacer
        // visible.
        var s = Saldo([
            Renglon(referencia: "A", anteriores: 0),
            Renglon(referencia: "B", anteriores: 2),
        ]);

        Assert.Single(s.Arrastrados);
        Assert.Equal("B", s.Arrastrados[0].Referencia);
    }

    [Fact]
    public void El_saldo_INICIAL_de_implantacion_se_declara_como_tal()
    {
        // «Es la primera vez que la institución ve todo junto». Se declara para que no se
        // compare contra los siguientes como si fueran la misma medición.
        Assert.True(Saldo([Renglon()], inicial: true).EsInicialDeImplantacion);
        Assert.False(Saldo([Renglon()]).EsInicialDeImplantacion);
    }

    [Fact]
    public void Un_saldo_VACIO_no_rompe_nada()
    {
        var s = Saldo([]);

        Assert.Equal(0, s.AntiguedadMaximaEnDias);
        Assert.Empty(s.Bloqueantes);
        Assert.Equal(0m, s.MontoTotal);
    }
}
