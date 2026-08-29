using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// `RN-86` — el ciclo propio de la obligación de reintegro.
///
/// Lo que defiende es el agujero que la regla nombra: <i>«sin ella, el cobro se pierde cuando
/// la misión cierra: el expediente se archiva, el hallazgo queda como marca, y el dinero no
/// vuelve»</i>.
/// </summary>
public class ObligacionDeReintegroPruebas
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

    [Fact]
    public void La_obligacion_nace_determinada_y_con_el_monto_congelado()
    {
        var o = Nominada();

        Assert.Equal(EstadoDeObligacion.Determinada, o.Estado);
        Assert.Equal(3_400m, o.Saldo);
        Assert.True(o.EstaAbierta);

        // La antigüedad se cuenta desde el hecho, no desde la nominación: es lo que `RN-97`
        // arrastra al ejercicio siguiente.
        Assert.Equal(27, o.AntiguedadEnDias(Hoy));
    }

    [Fact]
    public void No_se_puede_nominar_una_obligacion_en_cero()
    {
        var error = Assert.Throws<BloqueoDuro>(() => Nominada(0m));
        Assert.Contains("no obliga a nada", error.Message);
    }

    [Fact]
    public void El_peculio_propio_NO_puede_ir_a_cargo_del_servidor()
    {
        // Cruzarlas produciría una obligación que dice que el motorista debe el dinero que él
        // mismo puso. Cuadra formalmente y miente en el signo.
        var error = Assert.Throws<BloqueoDuro>(() => ObligacionDeReintegro.Nominar(
            Ulid.NewUlid(),
            DireccionDelReintegro.AFavorDeLaInstitucion,
            CausaDelReintegro.PeculioPropio,
            Motorista, 350m, Mision, null, Hoy, Quien("P-JEFE"), "Cargó en Iriona.", Ahora));

        Assert.Contains("lo haría deudor de lo que puso", error.Message);
    }

    [Fact]
    public void Un_faltante_NO_puede_ir_a_favor_del_servidor()
    {
        var error = Assert.Throws<BloqueoDuro>(() => ObligacionDeReintegro.Nominar(
            Ulid.NewUlid(),
            DireccionDelReintegro.AFavorDelServidor,
            CausaDelReintegro.Extravio,
            Motorista, 900m, Mision, null, Hoy, Quien("P-JEFE"), "Se perdió el sobre.", Ahora));

        Assert.Contains("va a cargo del servidor", error.Message);
    }

    [Fact]
    public void No_se_puede_registrar_descargo_de_quien_nunca_fue_notificado()
    {
        var o = Nominada();

        // No se le puede exigir a alguien que conteste lo que no se le dijo.
        Assert.Throws<TransicionInvalidaDeObligacion>(() =>
            o.RegistrarDescargo(Quien("P-JEFE"), "No me dijeron nada.", Ahora));
    }

    [Fact]
    public void Resolver_sin_descargo_se_puede_y_el_asiento_lo_DICE()
    {
        var o = Nominada();
        o.Notificar(Quien("P-ADMIN"), "Notificado en persona, acta NT-2026-0031.", Ahora);
        o.Resolver(Quien("P-GERENCIA"), "Se confirma el faltante.", Ahora);

        Assert.Equal(EstadoDeObligacion.Resuelta, o.Estado);

        // Un servidor que no contesta no puede cerrar el expediente con su silencio — pero
        // que no contestó tiene que constar.
        Assert.Contains("RESUELTA SIN DESCARGO DEL SERVIDOR", o.Diario[^1].Motivo);
    }

    [Fact]
    public void Pagar_antes_de_que_se_resuelva_SALDA_pero_no_borra_que_existio()
    {
        // `CE-26`: «se le da tiempo al motorista para que lo reponga; si repone, no queda
        // registro de que hubo faltante». Acá repone, y el registro queda entero.
        var o = Nominada();

        o.RegistrarPago(Quien("P-CAJA"), 3_400m, new DateOnly(2026, 9, 24),
            "Acta de reintegro RE-2026-0007.", Ahora);

        Assert.Equal(EstadoDeObligacion.Saldada, o.Estado);
        Assert.False(o.EstaAbierta);
        Assert.Equal(0m, o.Saldo);

        // El asiento de nominación sigue ahí, con su causa. P-3.
        Assert.Equal("R-01", o.Diario[0].Id);
        Assert.Contains("SinCausaIdentificada", o.Diario[0].Motivo);
    }

    [Fact]
    public void El_abono_parcial_baja_el_saldo_y_NO_avanza_el_ciclo()
    {
        var o = Nominada();
        o.Notificar(Quien("P-ADMIN"), "Acta NT-2026-0031.", Ahora);

        o.RegistrarPago(Quien("P-CAJA"), 1_400m, Hoy, "Abono parcial.", Ahora);

        // Sigue notificada, sigue abierta, y el saldo bajó exactamente lo que entró. El
        // sistema nunca redondea ni ajusta para cuadrar.
        Assert.Equal(EstadoDeObligacion.Notificada, o.Estado);
        Assert.True(o.EstaAbierta);
        Assert.Equal(2_000m, o.Saldo);
        Assert.Contains("ABONO PARCIAL", o.Diario[^1].Motivo);
    }

    [Fact]
    public void No_se_admite_cobrar_de_mas()
    {
        var o = Nominada();

        var error = Assert.Throws<BloqueoDuro>(() =>
            o.RegistrarPago(Quien("P-CAJA"), 5_000m, Hoy, "Acta.", Ahora));

        Assert.Contains("Cobrar de más no es un reintegro", error.Message);
    }

    [Fact]
    public void Dejarla_sin_efecto_NO_la_borra()
    {
        var o = Nominada();
        o.Notificar(Quien("P-ADMIN"), "Acta NT-2026-0031.", Ahora);
        o.RegistrarDescargo(Quien("P-JEFE"), "Traigo la factura que faltaba.", Ahora);
        o.DejarSinEfecto(Quien("P-GERENCIA"), "Se acoge el descargo: el comprobante existe.", Ahora);

        Assert.Equal(EstadoDeObligacion.DejadaSinEfecto, o.Estado);
        Assert.False(o.EstaAbierta);

        // Cuatro asientos, y el primero sigue diciendo que se le imputó. Es la única
        // constancia que le queda al servidor de que se le acusó y de que no procedía.
        Assert.Equal(4, o.Diario.Count);
        Assert.Equal("R-01", o.Diario[0].Id);
    }

    [Fact]
    public void Una_obligacion_saldada_ya_no_admite_movimientos()
    {
        var o = Nominada();
        o.RegistrarPago(Quien("P-CAJA"), 3_400m, Hoy, "Acta.", Ahora);

        Assert.Throws<TransicionInvalidaDeObligacion>(() =>
            o.RegistrarPago(Quien("P-CAJA"), 100m, Hoy, "Otra vez.", Ahora));
    }
}
