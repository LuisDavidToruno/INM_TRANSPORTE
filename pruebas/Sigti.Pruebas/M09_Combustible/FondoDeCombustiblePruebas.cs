using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// El fondo del período — `RN-26`.
///
/// ── Lo que se está protegiendo ───────────────────────────────────────────────
/// `PROP-01` nombra el punto de fuga: <i>«el efectivo sin trazabilidad»</i>. El saldo como
/// proyección del diario es lo que impide que se asigne más de lo aprobado y que la
/// diferencia aparezca meses después, sin responsable, en un cruce del TSC.
/// </summary>
public class FondoDeCombustiblePruebas
{
    private static readonly IdPersona Jefe = new("P-TRANSPORTE");
    private static readonly IdPersona Gerente = new("P-GERENCIA");
    private static readonly IdPersona Contador = new("P-CONTABILIDAD");

    private static readonly DateTimeOffset Momento =
        new(2026, 3, 2, 9, 0, 0, TimeSpan.FromHours(-6));

    private static FondoDeCombustible Solicitado(decimal pide = 50_000m) =>
        FondoDeCombustible.Solicitar(
            Ulid.NewUlid(), AmbitoDelFondo.Dependencia, "Delegacion de Choluteca",
            new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31),
            Jefe, pide, "Operación ordinaria de marzo: 14 misiones programadas.", Momento);

    private static FondoDeCombustible Aprobado(decimal monto = 40_000m, string? partida = "12-01-001-4-31200")
    {
        var fondo = Solicitado();
        fondo.Aprobar(Gerente, monto, partida, Momento);
        return fondo;
    }

    [Fact]
    public void El_fondo_nace_solicitado_y_sin_saldo()
    {
        var fondo = Solicitado(pide: 50_000m);

        Assert.Equal(EstadoDelFondo.Solicitado, fondo.Estado);

        // **Solicitar no crea saldo.** Si lo creara, el techo del fondo lo fijaría quien lo
        // pide — y la aprobación de Gerencia sería decorativa.
        Assert.Equal(0m, fondo.Aprobado);
    }

    [Fact]
    public void Sin_justificacion_no_se_solicita()
    {
        var fallo = Assert.Throws<BloqueoDuro>(() => FondoDeCombustible.Solicitar(
            Ulid.NewUlid(), AmbitoDelFondo.Institucion, "Instituto",
            new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31),
            Jefe, 50_000m, "   ", Momento));

        Assert.Contains("no se puede defender ante el Tribunal", fallo.Message);
    }

    [Fact]
    public void Un_periodo_que_termina_antes_de_empezar_no_se_registra()
    {
        Assert.Throws<BloqueoDuro>(() => FondoDeCombustible.Solicitar(
            Ulid.NewUlid(), AmbitoDelFondo.Institucion, "Instituto",
            new DateOnly(2026, 3, 31), new DateOnly(2026, 3, 1),
            Jefe, 50_000m, "Marzo", Momento));
    }

    [Fact]
    public void El_aprobado_es_la_suma_de_los_asientos_no_un_campo()
    {
        var fondo = Aprobado(40_000m);
        fondo.Ampliar(Gerente, 15_000m, "Operativo no previsto en la frontera.", Momento);

        Assert.Equal(55_000m, fondo.Aprobado);
        // Y la ampliación devuelve el fondo a `Aprobado`, que es lo que permite seguir
        // asignando sin abrir una vía corta.
        Assert.Equal(EstadoDelFondo.Aprobado, fondo.Estado);
    }

    [Fact]
    public void Quien_solicito_no_aprueba_ni_amplia()
    {
        var fondo = Solicitado();
        Assert.Throws<BloqueoDuro>(() => fondo.Aprobar(Jefe, 40_000m, "12-01", Momento));

        var aprobado = Aprobado();
        // La ampliación es la puerta trasera obvia: si no llevara el mismo control, bastaría
        // aprobar un lempira y ampliarlo a cuarenta mil.
        Assert.Throws<BloqueoDuro>(
            () => aprobado.Ampliar(Jefe, 15_000m, "Urgente", Momento));
    }

    [Fact]
    public void Un_fondo_aprobado_en_cero_no_es_un_fondo()
    {
        var fondo = Solicitado();
        Assert.Throws<BloqueoDuro>(() => fondo.Aprobar(Gerente, 0m, "12-01", Momento));
    }

    [Fact]
    public void El_asiento_de_aprobacion_DICE_que_la_cuota_trimestral_no_se_verifico()
    {
        // `RN-26` exige verificar `RN-54`, y no hay espejo presupuestario. Lo que no se puede
        // hacer se declara en el asiento: si el diario callara, dentro de dos años nadie
        // podría distinguir «se verificó y pasó» de «no se verificó».
        var fondo = Aprobado();

        Assert.Contains("RN-54", fondo.Diario[^1].Motivo);
        Assert.Contains("NO verificada", fondo.Diario[^1].Motivo);
    }

    [Fact]
    public void Un_fondo_sin_partida_se_registra_igual_y_el_asiento_lo_advierte()
    {
        // `RN-26`: la partida la define ARGOS; si el espejo no la tiene, el fondo se registra
        // con partida pendiente. Impedir el registro dejaría a la institución sin poder operar
        // por una integración que no es suya.
        var fondo = Aprobado(partida: null);

        Assert.Equal(EstadoDelFondo.Aprobado, fondo.Estado);
        Assert.Contains("Partida PENDIENTE", fondo.Diario[^1].Motivo);
    }

    [Fact]
    public void Y_ese_fondo_sin_partida_no_se_cierra()
    {
        var fondo = Aprobado(partida: null);

        var fallo = Assert.Throws<BloqueoDuro>(
            () => fondo.Cerrar(Contador, asignacionesSinLiquidar: 0, partida: null, Momento));

        Assert.Contains("no se inventa", fallo.Message);
    }

    [Fact]
    public void La_partida_se_puede_completar_al_cerrar()
    {
        var fondo = Aprobado(partida: null);
        fondo.Cerrar(Contador, 0, "12-01-001-4-31200", Momento);

        Assert.Equal(EstadoDelFondo.Cerrado, fondo.Estado);
        Assert.Equal("12-01-001-4-31200", fondo.PartidaPresupuestaria);
    }

    [Fact]
    public void El_saldo_es_aprobado_menos_asignado_mas_devoluciones_constatadas()
    {
        var fondo = Aprobado(40_000m);

        Assert.Equal(40_000m, fondo.SaldoDisponible(asignado: 0m, devolucionesConstatadas: 0m));
        Assert.Equal(15_000m, fondo.SaldoDisponible(asignado: 25_000m, devolucionesConstatadas: 0m));

        // La devolución constatada devuelve saldo. La declarada y no constatada no llega
        // nunca a este parámetro: quien llama la filtra, y por eso no se puede colar.
        Assert.Equal(18_000m, fondo.SaldoDisponible(asignado: 25_000m, devolucionesConstatadas: 3_000m));
    }

    [Fact]
    public void Quien_aprobo_no_cierra_el_periodo()
    {
        var fondo = Aprobado();

        var fallo = Assert.Throws<BloqueoDuro>(
            () => fondo.Cerrar(Gerente, 0, "12-01", Momento));

        Assert.Contains("no es quien declara que el gasto cuadró", fallo.Message);
    }

    [Fact]
    public void Un_fondo_con_vales_vivos_no_se_cierra()
    {
        var fondo = Aprobado();

        var fallo = Assert.Throws<BloqueoDuro>(
            () => fondo.Cerrar(Contador, asignacionesSinLiquidar: 2, "12-01", Momento));

        Assert.Contains("2 asignación(es)", fallo.Message);
    }

    [Fact]
    public void De_cerrado_no_se_sale()
    {
        var fondo = Aprobado();
        fondo.Cerrar(Contador, 0, "12-01", Momento);

        Assert.Throws<TransicionInvalidaDelFondo>(
            () => fondo.Ampliar(Gerente, 5_000m, "Se me olvidó", Momento));
        Assert.Throws<TransicionInvalidaDelFondo>(
            () => fondo.MarcarAgotado(Jefe, Momento));
    }

    [Fact]
    public void Agotado_no_es_terminal_la_ampliacion_lo_revive()
    {
        // `RN-26`: «Fondo agotado a mitad de mes con misiones urgentes pendientes. Se solicita
        // ampliación, que sigue el mismo circuito».
        var fondo = Aprobado(40_000m);
        fondo.MarcarAgotado(Jefe, Momento);
        Assert.Equal(EstadoDelFondo.Agotado, fondo.Estado);

        fondo.Ampliar(Gerente, 20_000m, "Operativo de frontera no previsto.", Momento);

        Assert.Equal(EstadoDelFondo.Aprobado, fondo.Estado);
        Assert.Equal(60_000m, fondo.Aprobado);
    }

    [Fact]
    public void La_vigencia_se_juzga_a_la_fecha_del_hecho()
    {
        var fondo = Aprobado();

        Assert.True(fondo.VigenteAl(new DateOnly(2026, 3, 1)));
        Assert.True(fondo.VigenteAl(new DateOnly(2026, 3, 31)));
        Assert.False(fondo.VigenteAl(new DateOnly(2026, 4, 1)));
        Assert.False(fondo.VigenteAl(new DateOnly(2026, 2, 28)));
    }

    [Fact]
    public void El_diario_conserva_todo_el_recorrido()
    {
        // P-3: nada se deshace. Y acá el recorrido ES el descargo: quién pidió, quién aprobó
        // cuánto, quién amplió y quién cerró.
        var fondo = Aprobado(40_000m);
        fondo.RegistrarEntrega(Jefe, "efectivo y 3 órdenes de pago", Momento);
        fondo.Ampliar(Gerente, 10_000m, "Ampliación de marzo.", Momento);
        fondo.Cerrar(Contador, 0, "12-01", Momento);

        Assert.Equal(
            ["F-01", "F-02", "F-03", "F-05", "F-06"],
            fondo.Diario.Select(m => m.Id));
    }
}
