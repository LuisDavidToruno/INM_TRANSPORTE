using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// `RN-66` — la jerarquía de anclas, con la <b>placa en último lugar</b>.
///
/// ── Por qué el orden importa ─────────────────────────────────────────────────
/// Hay desabastecimiento nacional de placas (`RN-15`), la placa se reasigna, y un vehículo pudo
/// haber llevado otra —o ninguna— el día del hecho. Resolver por placa primero atribuiría la
/// multa del año pasado al vehículo que hoy tiene esa chapa.
/// </summary>
public class ReglasDeImputacionExternaPruebas
{
    private static readonly Ulid Pickup = Ulid.NewUlid();
    private static readonly Ulid Camion = Ulid.NewUlid();

    private static readonly AnclasDelVehiculo[] Flota =
    [
        new(Pickup, "INS-PU-021", BienDelInventario: "BN-4471", Chasis: "CH-99001",
            Motor: "MT-1001", CorrelativoInstitucional: "021", Placa: "HAB-1234"),

        new(Camion, "INS-C-002", BienDelInventario: "BN-4472", Chasis: "CH-99002",
            Motor: "MT-1002", CorrelativoInstitucional: "002", Placa: "HAB-5678"),
    ];

    private static VehiculoResuelto Resolver(IdentificacionExterna i) =>
        ReglasDeImputacionExterna.Resolver(i, Flota);

    [Fact]
    public void El_numero_de_bien_manda_sobre_la_placa()
    {
        // La línea trae las dos y apuntan a vehículos distintos: gana el número de bien, que es
        // el ancla más estable. Es el caso de la placa reasignada.
        var r = Resolver(new IdentificacionExterna(
            BienDelInventario: "BN-4471", Placa: "HAB-5678"));

        Assert.Equal(Pickup, r.Vehiculo);
        Assert.Equal(AnclaDeVehiculo.BienDelInventario, r.Ancla);
        Assert.Contains("Resuelto por número de bien", r.Explicacion);
    }

    [Fact]
    public void El_chasis_manda_sobre_el_motor_y_el_correlativo()
    {
        var r = Resolver(new IdentificacionExterna(
            Chasis: "CH-99002", Motor: "MT-1001", CorrelativoInstitucional: "021"));

        Assert.Equal(Camion, r.Vehiculo);
        Assert.Equal(AnclaDeVehiculo.Chasis, r.Ancla);
    }

    [Fact]
    public void Resolver_por_PLACA_se_admite_pero_queda_advertido()
    {
        // Es la última de la jerarquía. El expediente tiene que decir que se resolvió así,
        // porque esa atribución admite discusión y la del número de bien no.
        var r = Resolver(new IdentificacionExterna(Placa: "HAB-1234"));

        Assert.Equal(Pickup, r.Vehiculo);
        Assert.Equal(AnclaDeVehiculo.Placa, r.Ancla);
        Assert.Contains("Resuelto por PLACA", r.Explicacion);
        Assert.Contains("la placa se reasigna", r.Explicacion);
    }

    [Fact]
    public void Lo_que_no_corresponde_a_ningun_vehiculo_queda_NO_RESUELTO()
    {
        // `RN-66`: no se asigna por parecido. «Puede ser un error del proveedor y puede no
        // serlo».
        var r = Resolver(new IdentificacionExterna(Placa: "XXX-0000"));

        Assert.False(r.EstaResuelto);
        Assert.Null(r.Ancla);
        Assert.Contains("puede no serlo", r.Explicacion);
    }

    [Fact]
    public void Una_linea_sin_ninguna_identificacion_no_se_resuelve()
    {
        var r = Resolver(new IdentificacionExterna());

        Assert.False(r.EstaResuelto);
        Assert.Contains("no se asigna por parecido", r.Explicacion);
    }

    [Fact]
    public void Dos_vehiculos_con_el_mismo_ancla_NO_se_desempatan_solos()
    {
        // Es un padrón corrupto, no una decisión que esta función deba tomar. Elegir uno sería
        // inventar la respuesta.
        AnclasDelVehiculo[] duplicada =
        [
            new(Pickup, "INS-PU-021", Chasis: "CH-IGUAL"),
            new(Camion, "INS-C-002", Chasis: "CH-IGUAL"),
        ];

        var r = ReglasDeImputacionExterna.Resolver(
            new IdentificacionExterna(Chasis: "CH-IGUAL"), duplicada);

        Assert.False(r.EstaResuelto);
        Assert.Contains("comparten chasis", r.Explicacion);
        Assert.Contains("lo que hay que corregir es el padrón", r.Explicacion);
    }

    [Fact]
    public void Un_vehiculo_SIN_placa_nunca_se_resuelve_por_placa()
    {
        // `RN-15`: sin placa metálica es un estado válido — hay desabastecimiento nacional.
        AnclasDelVehiculo[] sinPlaca = [new(Pickup, "INS-PU-021", Chasis: "CH-99001")];

        var porPlaca = ReglasDeImputacionExterna.Resolver(
            new IdentificacionExterna(Placa: "HAB-1234"), sinPlaca);

        Assert.False(porPlaca.EstaResuelto);

        // Y por chasis sí, que es el punto de tener jerarquía.
        var porChasis = ReglasDeImputacionExterna.Resolver(
            new IdentificacionExterna(Chasis: "CH-99001"), sinPlaca);

        Assert.Equal(Pickup, porChasis.Vehiculo);
    }

    [Fact]
    public void Se_compara_sin_importar_la_caja_ni_los_espacios()
    {
        var r = Resolver(new IdentificacionExterna(Placa: "  hab-1234 "));

        Assert.Equal(Pickup, r.Vehiculo);
    }

    [Fact]
    public void NO_se_normaliza_mas_alla_de_eso()
    {
        // Quitar guiones para que «HAB1234» case con «HAB-1234» empezaría a resolver por
        // parecido, que es justo lo que la regla prohíbe.
        Assert.False(Resolver(new IdentificacionExterna(Placa: "HAB1234")).EstaResuelto);
    }

    [Fact]
    public void Lo_no_resuelto_exige_responsable_y_plazo()
    {
        // Sin ellos, «no resuelto» se vuelve un montón que crece y que nadie revisa.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeImputacionExterna.ExigirResponsableYPlazoDeLoNoResuelto(null, null));

        Assert.Contains("un montón que crece y que nadie revisa", error.Message);

        ReglasDeImputacionExterna.ExigirResponsableYPlazoDeLoNoResuelto(
            "P-AUDITORIA", new DateOnly(2026, 10, 15));
    }
}
