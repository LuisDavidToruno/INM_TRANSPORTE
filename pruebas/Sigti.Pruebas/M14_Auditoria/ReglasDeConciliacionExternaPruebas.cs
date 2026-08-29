using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M14_Auditoria;

namespace Sigti.Pruebas.M14_Auditoria;

/// <summary>
/// `RN-95` — la conciliación contra fuentes externas.
///
/// ── Lo que revela y `RN-30` no puede ver ─────────────────────────────────────
/// `RN-95`, textual: <i>«una conciliación que solo compara nuestros datos con nuestros datos
/// verifica coherencia interna, no veracidad. <b>Un registro completo y coherente puede ser
/// completamente falso</b>, y solo la fuente externa lo revela»</i>.
///
/// De ahí salieron los tres casos de `CE-28`: el comprobante duplicado que apareció en el estado
/// de cuenta del proveedor, el paso por caseta de un domingo sin misión, y las multas
/// notificadas meses después.
/// </summary>
public class ReglasDeConciliacionExternaPruebas
{
    private static readonly Ulid Fuente = Ulid.NewUlid();
    private static readonly Ulid Pickup = Ulid.NewUlid();
    private static readonly Ulid Camion = Ulid.NewUlid();

    private static readonly DateOnly Desde = new(2026, 8, 1);
    private static readonly DateOnly Hasta = new(2026, 8, 31);

    private static readonly DateTimeOffset Corte =
        new(2026, 9, 5, 9, 0, 0, TimeSpan.FromHours(-6));

    private static readonly AnclasDelVehiculo[] Flota =
    [
        new(Pickup, "INS-PU-021", BienDelInventario: "BN-4471", Chasis: "CH-99001",
            Placa: "HAB-1234"),
        new(Camion, "INS-C-002", BienDelInventario: "BN-4472", Chasis: "CH-99002",
            Placa: "HAB-5678"),
    ];

    private static LineaExterna Linea(
        string id, int dia, decimal monto, string? referencia = null,
        string? placa = "HAB-1234", string? bien = null) =>
        new(id, new DateOnly(2026, 8, dia), monto,
            new IdentificacionExterna(BienDelInventario: bien, Placa: placa), referencia);

    private static AsientoPropio Asiento(
        int dia, decimal monto, string? referencia = null, Ulid? vehiculo = null) =>
        new(Ulid.NewUlid(), "consumo de vale", new DateOnly(2026, 8, dia), monto,
            vehiculo ?? Pickup, referencia);

    private static ResultadoDeConciliacion Conciliar(
        IReadOnlyList<LineaExterna> lineas,
        IReadOnlyList<AsientoPropio> asientos,
        int tolerancia = 1,
        DateOnly? desde = null,
        DateOnly? hasta = null) =>
        ReglasDeConciliacionExterna.Conciliar(
            Fuente, desde ?? Desde, hasta ?? Hasta, lineas, asientos, Flota,
            tolerancia, Corte, "estado-de-cuenta-agosto-2026.pdf");

    // ── Las tres listas ─────────────────────────────────────────────────────

    [Fact]
    public void Lo_que_cuadra_queda_en_COINCIDENTES_por_comprobante()
    {
        var r = Conciliar(
            [Linea("L1", 12, 1_760m, "F-88201")],
            [Asiento(12, 1_760m, "F-88201")]);

        Assert.Single(r.Coincidentes);
        Assert.Equal(CriterioDeCoincidencia.Referencia, r.Coincidentes[0].Criterio);
        Assert.Equal(0, r.Diferencias);
    }

    [Fact]
    public void Lo_que_el_proveedor_tiene_y_nosotros_no_abre_expediente()
    {
        // Puede ser un cobro indebido o un consumo que nadie registró. La conciliación no
        // presume cuál.
        var r = Conciliar([Linea("L1", 12, 1_760m, "F-88201")], []);

        Assert.Single(r.SoloEnLaFuente);
        Assert.Equal(1_760m, r.MontoSoloEnLaFuente);
        Assert.Equal(1, r.Diferencias);
    }

    [Fact]
    public void Lo_que_nosotros_tenemos_y_el_proveedor_no_TAMBIEN_abre_expediente()
    {
        // `RN-95`: «puede ser un comprobante falso, o una estación que no reportó. La
        // conciliación no presume cuál». Conciliar en un solo sentido dejaría fuera el caso
        // más grave.
        var r = Conciliar([], [Asiento(12, 1_760m, "F-88201")]);

        Assert.Single(r.SoloEnSigti);
        Assert.Equal(1_760m, r.MontoSoloEnSigti);
    }

    // ── El comprobante duplicado, uno de los tres casos de `CE-28` ──────────

    [Fact]
    public void El_comprobante_DUPLICADO_en_el_estado_de_cuenta_aparece()
    {
        // Dos líneas del proveedor con el mismo comprobante y un solo consumo registrado. La
        // segunda no puede casarse con el mismo asiento: `RN-84` hace único el comprobante en
        // la institución, así que dos cobros con el mismo son un cobro de más.
        var r = Conciliar(
            [Linea("L1", 12, 1_760m, "F-88201"), Linea("L2", 12, 1_760m, "F-88201")],
            [Asiento(12, 1_760m, "F-88201")]);

        Assert.Single(r.Coincidentes);
        Assert.Single(r.SoloEnLaFuente);
        Assert.Equal(1_760m, r.MontoSoloEnLaFuente);
    }

    // ── La fecha del hecho, no el período del estado de cuenta ──────────────

    [Fact]
    public void El_consumo_del_31_que_llega_en_el_estado_de_cuenta_siguiente_CUADRA()
    {
        // `RN-95` casos límite. Conciliar por período del estado de cuenta lo dejaría como
        // diferencia todos los meses, y a los tres meses nadie miraría el reporte.
        var r = Conciliar(
            [Linea("L1", 31, 980m, "F-88999")],
            [Asiento(31, 980m, "F-88999")]);

        Assert.Single(r.Coincidentes);
        Assert.Empty(r.SoloEnLaFuente);
    }

    [Fact]
    public void Un_asiento_fuera_del_rango_no_entra_a_la_conciliacion()
    {
        // El rango acota qué se concilia. Un consumo de julio no es una diferencia del estado
        // de cuenta de agosto: es un hecho de otro período.
        var r = Conciliar(
            [],
            [new AsientoPropio(Ulid.NewUlid(), "consumo", new DateOnly(2026, 7, 15), 500m, Pickup)]);

        Assert.Empty(r.SoloEnSigti);
    }

    // ── El criterio débil ───────────────────────────────────────────────────

    [Fact]
    public void Sin_comprobante_se_casa_por_vehiculo_monto_y_fecha()
    {
        // Hay estaciones que no numeran el cupón. Sin este criterio, toda carga sin comprobante
        // aparecería como diferencia.
        var r = Conciliar(
            [Linea("L1", 12, 1_760m, referencia: null)],
            [Asiento(12, 1_760m, referencia: null)]);

        Assert.Single(r.Coincidentes);
        Assert.Equal(CriterioDeCoincidencia.VehiculoMontoYFecha, r.Coincidentes[0].Criterio);
    }

    [Fact]
    public void El_desfase_de_UN_dia_se_tolera_y_el_de_tres_no()
    {
        // El proveedor factura con la fecha que él tiene, y un día de diferencia en un cupón no
        // es una diferencia de conciliación.
        Assert.Single(Conciliar(
            [Linea("L1", 12, 1_760m)], [Asiento(13, 1_760m)], tolerancia: 1).Coincidentes);

        Assert.Empty(Conciliar(
            [Linea("L1", 12, 1_760m)], [Asiento(15, 1_760m)], tolerancia: 1).Coincidentes);
    }

    [Fact]
    public void El_criterio_debil_NO_casa_lineas_de_otro_vehiculo()
    {
        // Mismo monto, misma fecha, otro vehículo. Casarlas atribuiría el consumo al vehículo
        // equivocado, que es peor que dejarlo como diferencia.
        var r = Conciliar(
            [Linea("L1", 12, 1_760m, placa: "HAB-5678")],
            [Asiento(12, 1_760m, vehiculo: Pickup)]);

        Assert.Empty(r.Coincidentes);
        Assert.Single(r.SoloEnLaFuente);
        Assert.Single(r.SoloEnSigti);
    }

    [Fact]
    public void Sin_vehiculo_resuelto_NO_se_casa_por_parecido()
    {
        // `RN-66`: lo que no se resuelve no se asigna por parecido. Casar una línea sin
        // vehículo contra el único asiento del monto sería adivinar de quién es.
        var r = Conciliar(
            [Linea("L1", 12, 1_760m, placa: "XXX-0000")],
            [Asiento(12, 1_760m)]);

        Assert.Empty(r.Coincidentes);
        Assert.False(r.SoloEnLaFuente[0].Vehiculo.EstaResuelto);
        Assert.Equal(1, r.SinVehiculoResuelto);
    }

    [Fact]
    public void La_referencia_manda_sobre_el_parecido()
    {
        // Si el comprobante casa, la fecha discrepante es el dato interesante — no un motivo
        // para no casar. Por eso el criterio fuerte no lleva tolerancia.
        var r = Conciliar(
            [Linea("L1", 12, 1_760m, "F-88201")],
            [Asiento(28, 1_760m, "F-88201")]);

        Assert.Single(r.Coincidentes);
        Assert.Equal(CriterioDeCoincidencia.Referencia, r.Coincidentes[0].Criterio);
    }

    // ── Los controles ───────────────────────────────────────────────────────

    [Fact]
    public void Una_fuente_NO_DISPONIBLE_no_se_puede_conciliar()
    {
        // Conciliar contra ella produciría cero diferencias sobre cero líneas, y ese cero se
        // lee después como conformidad. No disponible es distinto de conciliada.
        var sinTag = new FuenteExterna(
            Ulid.NewUlid(), TipoDeFuenteExterna.EstadoDeCuentaDePeaje, "COVI-H", "CSV",
            "P-COMBUSTIBLE", Disponible: false,
            PorQueNoEstaDisponible: "La institución no tiene tag CoviPass.");

        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeConciliacionExterna.ExigirFuenteDisponible(sinTag));

        Assert.Contains("se lee después como conformidad", error.Message);
        Assert.Contains("No disponible es distinto de conciliada", error.Message);
    }

    [Fact]
    public void El_rango_invertido_se_rechaza()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeConciliacionExterna.ExigirRangoValido(Hasta, Desde));

        Assert.Contains("cero diferencias sobre cero datos", error.Message);
    }

    [Fact]
    public void La_conciliacion_exige_identificar_el_documento_fuente()
    {
        // `RN-95` punto 6. Sin él, una diferencia no se puede volver a comprobar contra el
        // papel del que salió.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeConciliacionExterna.ExigirDocumentoFuente("   "));

        Assert.Contains("el papel del que salió", error.Message);
    }

    [Fact]
    public void El_resultado_lleva_su_fecha_de_CORTE_y_su_documento()
    {
        // `RN-94`. Sin corte, dos ejecuciones del mismo reporte con datos distintos se ven
        // idénticas y no se pueden comparar.
        var r = Conciliar([], []);

        Assert.Equal(Corte, r.FechaDeCorte);
        Assert.Equal("estado-de-cuenta-agosto-2026.pdf", r.DocumentoFuente);
    }
}
