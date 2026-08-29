using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// `RN-86` — el bloqueo de nueva asignación y su válvula.
///
/// `HU-078`: <i>«hoy nada impide seguir entregándole fondo a quien no liquidó el anterior. El
/// saldo se acumula sobre unas pocas personas y aparece recién cuando alguien hace el arqueo
/// del período, meses después»</i>.
/// </summary>
public class ReglasDelReintegroPruebas
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 24, 10, 0, 0, TimeSpan.FromHours(-6));
    private static readonly DateOnly Hoy = new(2026, 9, 24);
    private static readonly Ulid Motorista = Ulid.NewUlid();
    private static readonly Ulid Mision = Ulid.NewUlid();

    private static Autoria Quien(string persona, string puesto = "PU-GERENCIA-ADMIN") =>
        Autoria.De(new IdPersona(persona), new IdPuesto(puesto), Hoy);

    private static ObligacionDeReintegro Nominada(decimal monto = 3_400m) =>
        ObligacionDeReintegro.Nominar(
            Ulid.NewUlid(),
            DireccionDelReintegro.AFavorDeLaInstitucion,
            CausaDelReintegro.SinCausaIdentificada,
            Motorista, monto, Mision, Ulid.NewUlid(),
            fechaDelHecho: new DateOnly(2026, 8, 28),
            Quien("P-AUDITORIA"),
            "Faltante constatado en la liquidación de la misión, sin causa declarada.",
            Ahora);

    private static SaldoAfuera Vale(decimal monto, DateOnly? vence) =>
        new(Ulid.NewUlid(), "FC-2026-0491", Motorista, Mision, "OM-2026-0491",
            monto, new DateOnly(2026, 9, 10), vence, "prueba");

    [Fact]
    public void Sin_deudas_no_bloquea_nada()
    {
        ReglasDelReintegro.ExigirQueNoDebaReintegro("Denis Fúnez", Mision, [], [], Hoy);
    }

    [Fact]
    public void La_obligacion_abierta_bloquea_y_el_mensaje_NOMBRA_la_deuda_con_su_origen()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelReintegro.ExigirQueNoDebaReintegro(
                "Denis Fúnez", Mision, [Nominada()], [], Hoy));

        Assert.Contains("Denis Fúnez no puede recibir nueva asignación", error.Message);
        Assert.Contains("3,400.00", error.Message);
        Assert.Contains("faltante sin causa identificada", error.Message);
        Assert.Contains("28/08/2026", error.Message);

        // Y dice cuáles son las dos salidas, porque un bloqueo sin salida se esquiva por
        // fuera del sistema emitiendo a nombre de otro motorista.
        Assert.Contains("Gerencia Administrativa", error.Message);
        Assert.Contains("RN-14", error.Message);
    }

    [Fact]
    public void La_obligacion_A_FAVOR_del_servidor_NO_bloquea()
    {
        // Negarle un vale a quien puso de su bolsillo sería castigarlo por haber puesto.
        var aFavor = ObligacionDeReintegro.Nominar(
            Ulid.NewUlid(), DireccionDelReintegro.AFavorDelServidor,
            CausaDelReintegro.PeculioPropio, Motorista, 350m, Mision, null,
            new DateOnly(2026, 9, 1), Quien("P-JEFE"), "Cargó en Iriona con su dinero.", Ahora);

        ReglasDelReintegro.ExigirQueNoDebaReintegro("Denis Fúnez", Mision, [aFavor], [], Hoy);
    }

    [Fact]
    public void La_obligacion_saldada_deja_de_bloquear()
    {
        var o = Nominada();
        o.RegistrarPago(Quien("P-CAJA"), 3_400m, Hoy, "Acta RE-2026-0007.", Ahora);

        ReglasDelReintegro.ExigirQueNoDebaReintegro("Denis Fúnez", Mision, [o], [], Hoy);
    }

    [Fact]
    public void El_saldo_VENCIDO_bloquea_aunque_nadie_haya_nominado_una_obligacion()
    {
        // Es la segunda mitad de `RN-86` y el segundo escenario de `HU-078`: el intervalo
        // entre que el plazo vence y que alguien se sienta a nominar es justo donde `CE-26`
        // dice que nace el faltante.
        var vencido = Vale(1_850m, vence: new DateOnly(2026, 9, 18));

        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelReintegro.ExigirQueNoDebaReintegro(
                "Óscar Banegas", Mision, [], [vencido], Hoy));

        Assert.Contains("1,850.00 sin comprobar", error.Message);
        Assert.Contains("18/09/2026", error.Message);
    }

    [Fact]
    public void El_saldo_DENTRO_de_plazo_no_bloquea()
    {
        // El motorista que volvió anoche tiene dinero afuera y está en su derecho.
        var enPlazo = Vale(1_850m, vence: new DateOnly(2026, 10, 2));

        ReglasDelReintegro.ExigirQueNoDebaReintegro("Óscar Banegas", Mision, [], [enPlazo], Hoy);
    }

    [Fact]
    public void Sin_plazo_definido_el_saldo_NO_se_declara_vencido()
    {
        // `[C]` insumo #32. Sin el parámetro no hay contra qué decir que venció, y declararlo
        // vencido sería inventarle un plazo a la institución.
        var sinPlazo = Vale(1_850m, vence: null);

        ReglasDelReintegro.ExigirQueNoDebaReintegro("Óscar Banegas", Mision, [], [sinPlazo], Hoy);
    }

    // ── El levantamiento ────────────────────────────────────────────────────

    [Fact]
    public void El_levantamiento_de_ACT_08_deja_pasar_la_emision_de_ESA_mision()
    {
        var acto = ReglasDelReintegro.Levantar(
            Mision, Quien("P-GERENCIA"),
            "Único motorista habilitado categoría C disponible para el traslado del 25/09/2026.",
            Ahora);

        ReglasDelReintegro.ExigirQueNoDebaReintegro(
            "Denis Fúnez", Mision, [Nominada()], [], Hoy, acto);
    }

    [Fact]
    public void El_levantamiento_de_OTRA_mision_no_sirve()
    {
        // Un levantamiento por persona sin fecha de fin sería un permiso permanente que nadie
        // se acuerda de revocar.
        var otra = ReglasDelReintegro.Levantar(
            Ulid.NewUlid(), Quien("P-GERENCIA"), "Urgencia de la semana pasada.", Ahora);

        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelReintegro.ExigirQueNoDebaReintegro(
                "Denis Fúnez", Mision, [Nominada()], [], Hoy, otra));
    }

    [Fact]
    public void El_levantamiento_sin_motivo_escrito_se_rechaza()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelReintegro.Levantar(Mision, Quien("P-GERENCIA"), "   ", Ahora));

        Assert.Contains("motivo escrito", error.Message);
        Assert.Contains("indicador", error.Message);
    }
}
