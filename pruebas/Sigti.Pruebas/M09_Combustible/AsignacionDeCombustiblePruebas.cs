using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// El vale — máquina §10.1.
///
/// ── Lo que estas pruebas sostienen ───────────────────────────────────────────
/// `PROP-01`: <i>«ningún lempira se mueve sin quedar atado a un folio, un responsable, una
/// misión y un odómetro»</i>. Cada prueba de acá es uno de esos cuatro amarres, o el momento
/// en que se pueden soltar.
/// </summary>
public class AsignacionDeCombustiblePruebas
{
    private static readonly Ulid Fondo = Ulid.NewUlid();
    private static readonly Ulid Mision = Ulid.NewUlid();
    private static readonly Ulid Vehiculo = Ulid.NewUlid();

    private static readonly IdPersona Jefe = new("P-TRANSPORTE");
    private static readonly IdPersona Encargado = new("P-COMBUSTIBLE");
    /// <summary>Quien conduce, como persona — para la segregación de `BD-06`.</summary>
    private static readonly IdPersona Motorista = new("P-MOTORISTA");

    /// <summary>Y su registro en el padrón, que es lo que `RN-32` compara.</summary>
    private static readonly Ulid Conductor = Ulid.NewUlid();
    private static readonly IdPersona Contador = new("P-CONTABILIDAD");
    private static readonly IdPersona Auditor = new("P-AUDITORIA");

    private static readonly DateTimeOffset Momento =
        new(2026, 3, 16, 7, 30, 0, TimeSpan.FromHours(-6));

    private static AsignacionDeCombustible Emitida(
        decimal monto = 2_500m,
        decimal saldo = 40_000m,
        EstadoDeMision estado = EstadoDeMision.Programada) =>
        AsignacionDeCombustible.Emitir(
            Ulid.NewUlid(), "VAL-CHO-2026-000418", Fondo, Mision, estado,
            EstadoDeMision.Programada, Vehiculo, Conductor, Vehiculo, Conductor,
            combustibleDelVehiculo: "Diesel", tipoDeCombustible: "Diesel",
            monto: monto, galones: 50m, instrumento: "vale",
            emite: Jefe, saldoDisponible: saldo, toleranciaSobregiro: 0m, momento: Momento);

    private static AsignacionDeCombustible Entregada(decimal monto = 2_500m)
    {
        var a = Emitida(monto);
        a.Entregar(Encargado, "Firma de recepción digital, 16/03 07:30", Momento);
        return a;
    }

    /// <summary>Lo que `RN-30` devuelve cuando el rendimiento cuadra.</summary>
    private static readonly Conciliacion Conforme = new(
        DictamenDeConciliacion.DentroDeUmbral, 1_000, 105m, 9.52m,
        new RendimientoEsperado(10m, OrigenDelRendimiento.Institucional, "RENDIMIENTO-2026-Q1"),
        -0.048m, "1,000 km / 105.00 gal = 9.52 km/gal contra 10.00 esperado");

    /// <summary>Y cuando no. Es el dictamen, no una opinión de quien concilia.</summary>
    private static readonly Conciliacion ConDesviacion = new(
        DictamenDeConciliacion.RendimientoImposible, 1_000, 80m, 12.5m,
        new RendimientoEsperado(10m, OrigenDelRendimiento.Institucional, "RENDIMIENTO-2026-Q1"),
        0.25m, "RENDIMIENTO IMPOSIBLE: menos galones de los que el recorrido exige");

    private static ConsumoRegistrado Carga(decimal galones = 30m, decimal monto = 1_500m) =>
        new(galones, monto, "Estación Uno, Choluteca", Odometro: 84_120, Comprobante: "F-0011-9932");

    // ── V-01 emitir ─────────────────────────────────────────────────────────

    [Fact]
    public void Emitida_no_es_entregada_y_esa_diferencia_no_es_formalismo()
    {
        var a = Emitida();

        Assert.Equal(EstadoDeAsignacion.Emitida, a.Estado);
        // `EMITIDA` es un papel con folio que no salió de la custodia de ACT-07. `ENTREGADA` es
        // dinero público fuera de la caja. Colapsarlos borraría la primera pregunta de la
        // auditoría: ¿quién lo tenía cuando desapareció?
        Assert.False(a.TuvoConsumo);
    }

    [Fact]
    public void Sin_folio_no_hay_asignacion()
    {
        var fallo = Assert.Throws<BloqueoDuro>(() => AsignacionDeCombustible.Emitir(
            Ulid.NewUlid(), "  ", Fondo, Mision, EstadoDeMision.Programada,
            EstadoDeMision.Programada, Vehiculo, Conductor, Vehiculo, Conductor,
            "Diesel", "Diesel", 2_500m, 50m, "vale", Jefe, 40_000m, 0m, Momento));

        Assert.Equal("RN-27", fallo.Precondicion);
        Assert.Contains("no existe para la auditoría", fallo.Message);
    }

    [Fact]
    public void El_saldo_del_fondo_se_verifica_al_emitir_no_al_entregar()
    {
        // Verificarlo al entregar sería tarde: el vale ya tiene folio, ya está impreso y el
        // motorista ya está en la ventanilla.
        var fallo = Assert.Throws<BloqueoDuro>(() => Emitida(monto: 5_000m, saldo: 3_000m));

        Assert.Equal("RN-26", fallo.Precondicion);
        Assert.Contains("Faltan 2,000.00", fallo.Message);
    }

    [Fact]
    public void Contra_una_mision_aprobada_pero_no_programada_no_se_emite()
    {
        Assert.Throws<TransicionInvalida>(() => Emitida(estado: EstadoDeMision.Aprobada));
    }

    // ── V-02 entregar ───────────────────────────────────────────────────────

    [Fact]
    public void Sin_constancia_de_recepcion_no_se_entrega()
    {
        var a = Emitida();

        var fallo = Assert.Throws<BloqueoDuro>(() => a.Entregar(Encargado, "   ", Momento));

        Assert.Contains("no se considera consumible ni liquidable", fallo.Message);
        // Y sigue emitida: el fallo no dejó el vale a medio camino.
        Assert.Equal(EstadoDeAsignacion.Emitida, a.Estado);
    }

    [Fact]
    public void Quien_emitio_el_vale_no_lo_entrega()
    {
        // `I-03`, bloqueo duro: «el par que habilita el fraude de combustible más simple».
        var a = Emitida();

        var fallo = Assert.Throws<BloqueoDuro>(
            () => a.Entregar(Jefe, "Firma de recepción", Momento));

        Assert.Equal("BD-06", fallo.Precondicion);
    }

    [Fact]
    public void Un_vale_no_se_entrega_dos_veces()
    {
        var a = Entregada();

        Assert.Throws<TransicionInvalidaDeAsignacion>(
            () => a.Entregar(Encargado, "Otra vez", Momento));
    }

    [Fact]
    public void El_receptor_del_asiento_es_el_de_la_emision_no_uno_nuevo()
    {
        // El receptor NO se vuelve a pedir al entregar: es el que `RN-32` ya validó contra la
        // orden. Volver a pedirlo abriría la puerta a entregarle a otro justo en el paso
        // donde el dinero sale de la caja.
        var a = Entregada();

        Assert.Contains(Conductor.ToString(), a.Diario[^1].Motivo);
    }

    // ── V-03 anular ─────────────────────────────────────────────────────────

    [Fact]
    public void Un_vale_emitido_se_anula_y_su_valor_vuelve_al_fondo()
    {
        var a = Emitida(2_500m);
        a.Anular(Jefe, "Misión desprogramada por avería del vehículo. Acta 2026-041.", Momento);

        Assert.Equal(EstadoDeAsignacion.Anulada, a.Estado);
        // Vuelve porque NO fue canjeado (`RN-27` punto 4). Después de entregado ya no cabe.
        Assert.Equal(2_500m, a.Devuelto);
        Assert.True(a.EstaResuelta);
    }

    [Fact]
    public void Un_vale_ya_entregado_NO_se_anula()
    {
        var a = Entregada();

        Assert.Throws<TransicionInvalidaDeAsignacion>(
            () => a.Anular(Jefe, "Mejor no", Momento));
    }

    [Fact]
    public void La_anulacion_exige_motivo()
    {
        var a = Emitida();
        Assert.Throws<BloqueoDuro>(() => a.Anular(Jefe, "", Momento));
    }

    // ── V-04 consumir ───────────────────────────────────────────────────────

    [Fact]
    public void Un_vale_no_entregado_no_se_consume()
    {
        var a = Emitida();

        Assert.Throws<TransicionInvalidaDeAsignacion>(
            () => a.RegistrarConsumo(Motorista, Carga(), Momento));
    }

    [Fact]
    public void El_consumo_exige_odometro_del_momento()
    {
        var a = Entregada();

        var fallo = Assert.Throws<BloqueoDuro>(() => a.RegistrarConsumo(
            Motorista, new ConsumoRegistrado(30m, 1_500m, "Estación Uno", Odometro: 0), Momento));

        Assert.Contains("un total contra otro total", fallo.Message);
    }

    [Fact]
    public void Se_pueden_registrar_VARIAS_cargas_y_se_suman()
    {
        // Cargar a la ida y a la vuelta es lo normal. `CONSUMIDA` significa «ya se tocó», no
        // «se acabó»: §10.1 dice «Puede ser consumo parcial».
        var a = Entregada(2_500m);
        a.RegistrarConsumo(Motorista, Carga(30m, 1_500m), Momento);
        a.RegistrarConsumo(Motorista, Carga(18m, 900m), Momento);

        Assert.Equal(EstadoDeAsignacion.Consumida, a.Estado);
        Assert.Equal(2_400m, a.Consumido);
        Assert.Equal(48m, a.GalonesConsumidos);
    }

    [Fact]
    public void El_consumo_sin_comprobante_se_registra_y_el_asiento_lo_dice()
    {
        // `RN-85`: el registro del abastecimiento **no se omite nunca por falta de papel**.
        // Pero tampoco se disimula: el asiento nombra la deuda documental.
        var a = Entregada();
        a.RegistrarConsumo(
            Motorista,
            new ConsumoRegistrado(30m, 1_500m, "Estación sin factura", 84_120,
                                  Comprobante: null,
                                  CausaSinComprobante: "La estación no emitió factura: sistema caído."),
            Momento);

        Assert.Equal(EstadoDeAsignacion.Consumida, a.Estado);
        Assert.Contains("SIN COMPROBANTE", a.Diario[^1].Motivo);
        Assert.Contains("RN-85", a.Diario[^1].Motivo);
        // Y la causa va en el asiento: sin ella el registro dice que falta el papel pero no
        // si eso se puede defender.
        Assert.Contains("sistema caído", a.Diario[^1].Motivo);
    }

    [Fact]
    public void Un_consumo_sin_comprobante_y_SIN_causa_NO_se_registra()
    {
        // La causa es lo único que distingue «la estación no dio factura» de un campo que
        // nadie llenó, y esa diferencia decide si el descargo alternativo procede.
        var a = Entregada();

        var fallo = Assert.Throws<BloqueoDuro>(() => a.RegistrarConsumo(
            Motorista,
            new ConsumoRegistrado(30m, 1_500m, "Sin factura", 84_120, Comprobante: null),
            Momento));

        Assert.Equal("RN-85", fallo.Precondicion);
        Assert.Contains("tampoco se disimula", fallo.Message);
    }

    [Fact]
    public void El_consumo_llega_de_campo_con_su_identificador_de_captura()
    {
        // `V-04` se ejecuta sin conectividad y el dispositivo reintenta. El identificador de
        // captura es lo que hace inofensivo el reenvío.
        var a = Entregada();
        var captura = Ulid.NewUlid();
        a.RegistrarConsumo(Motorista, Carga(), Momento, idDeCaptura: captura);

        Assert.Equal(captura, a.Diario[^1].IdDeCaptura);
    }

    [Fact]
    public void Quien_entrego_el_vale_no_lo_consume()
    {
        var a = Entregada();

        Assert.Throws<BloqueoDuro>(() => a.RegistrarConsumo(Encargado, Carga(), Momento));
    }

    // ── V-05 devolver ───────────────────────────────────────────────────────

    [Fact]
    public void Un_vale_intacto_se_devuelve_y_libera_saldo()
    {
        var a = Entregada(2_500m);
        a.DevolverIntegra(Encargado, "Acta de devolución 2026-018, firmada.", Momento);

        Assert.Equal(EstadoDeAsignacion.Devuelta, a.Estado);
        Assert.Equal(2_500m, a.Devuelto);
    }

    [Fact]
    public void Un_vale_con_CUALQUIER_consumo_NO_se_devuelve_integro()
    {
        // §10.1: «si hubo cualquier consumo, la asignación no puede ir a DEVUELTA y la misión
        // toma el camino T-16». Es económico, no formal: devolver «íntegro» algo ya tocado es
        // declarar que volvió un dinero que no volvió.
        var a = Entregada(2_500m);
        a.RegistrarConsumo(Motorista, Carga(5m, 250m), Momento);

        var fallo = Assert.Throws<TransicionInvalidaDeAsignacion>(
            () => a.DevolverIntegra(Encargado, "Acta", Momento));

        Assert.Equal("V-05", fallo.Transicion);
    }

    [Fact]
    public void La_devolucion_exige_acta_constatada()
    {
        var a = Entregada();

        var fallo = Assert.Throws<BloqueoDuro>(() => a.DevolverIntegra(Encargado, "  ", Momento));

        Assert.Contains("no libera saldo", fallo.Message);
    }

    // ── V-06 y V-08 extravío ────────────────────────────────────────────────

    [Fact]
    public void El_extravio_se_declara_con_acta_y_se_liquida_igual()
    {
        // El instrumento se pierde; el descargo no. Un extravío no declarado deja un vale que
        // sigue figurando entregado y que puede aparecer canjeado en la factura del proveedor
        // — la contradicción que el circuito de folios existe para descubrir.
        var a = Entregada();
        a.DeclararExtravio(Motorista, "Acta de extravío 2026-004.", Momento);
        Assert.Equal(EstadoDeAsignacion.Extraviada, a.Estado);

        a.LiquidarPorExtravio(Contador, "Acta de extravío 2026-004, descargo aceptado.", Momento);

        Assert.Equal(EstadoDeAsignacion.Liquidada, a.Estado);
        Assert.True(a.EstaResuelta);
    }

    [Fact]
    public void El_extravio_exige_acta()
    {
        var a = Entregada();
        Assert.Throws<BloqueoDuro>(() => a.DeclararExtravio(Motorista, "", Momento));
    }

    // ── V-07 liquidar ───────────────────────────────────────────────────────

    [Fact]
    public void La_liquidacion_que_cuadra_lo_dice_con_todas_sus_letras()
    {
        var a = Entregada(2_500m);
        a.RegistrarConsumo(Motorista, Carga(30m, 1_500m), Momento);
        a.Liquidar(Contador, saldoDevuelto: 1_000m, observacion: null, Momento);

        Assert.Equal(EstadoDeAsignacion.Liquidada, a.Estado);
        Assert.Contains("Cuadra exacto", a.Diario[^1].Motivo);
        Assert.Equal(1_000m, a.Devuelto);
    }

    [Fact]
    public void La_diferencia_sin_explicar_se_nombra_y_apunta_a_H_11()
    {
        var a = Entregada(2_500m);
        a.RegistrarConsumo(Motorista, Carga(30m, 1_500m), Momento);
        a.Liquidar(Contador, saldoDevuelto: 600m, observacion: null, Momento);

        // 2500 − 1500 − 600 = 400 que nadie puede explicar. `RN-29` y `H-11`: con diferencia
        // sin explicar la orden no debe poder pasar a `LIQUIDADA`.
        Assert.Contains("DIFERENCIA SIN EXPLICAR de 400.00", a.Diario[^1].Motivo);
        Assert.Contains("H-11", a.Diario[^1].Motivo);
    }

    [Fact]
    public void Quien_consumio_no_liquida()
    {
        var a = Entregada();
        a.RegistrarConsumo(Motorista, Carga(), Momento);

        Assert.Throws<BloqueoDuro>(() => a.Liquidar(Motorista, 0m, null, Momento));
    }

    [Fact]
    public void Quien_EMITIO_tampoco_liquida_aunque_hayan_pasado_tres_pasos()
    {
        // El recorrido completo del fraude, no un tropiezo: emitir y después declarar que todo
        // cuadró. Comparar sólo contra el acto anterior lo dejaría pasar.
        var a = Entregada();
        a.RegistrarConsumo(Motorista, Carga(), Momento);

        var fallo = Assert.Throws<BloqueoDuro>(() => a.Liquidar(Jefe, 0m, null, Momento));

        Assert.Contains("ya emitió", fallo.Message);
    }

    // ── V-09 y V-10 conciliar ───────────────────────────────────────────────

    [Fact]
    public void La_conciliacion_dentro_de_umbral_cierra_el_vale()
    {
        var a = Liquidada();
        a.Conciliar(Auditor, Conforme, causa: null, Momento);

        Assert.Equal(EstadoDeAsignacion.Conciliada, a.Estado);
        Assert.True(a.EstaResuelta);

        // Y la evidencia entera va al asiento: el dictamen sin sus cuentas es una opinión, y
        // una conciliación que no dice contra qué se juzgó no se puede rehacer.
        Assert.Contains("contra 10.00 esperado", a.Diario[^1].Motivo);
    }

    [Fact]
    public void Fuera_de_umbral_SIN_causa_tipificada_no_concilia()
    {
        // `INV-35`: toda desviación fuera de umbral tiene causa tipificada. Sin ella la misión
        // no se puede cerrar, y dejarla conciliar acá volvería inalcanzable ese invariante.
        var a = Liquidada();

        var fallo = Assert.Throws<BloqueoDuro>(
            () => a.Conciliar(Auditor, ConDesviacion, causa: "   ", Momento));

        Assert.Contains("INV-35", fallo.Message);
    }

    [Fact]
    public void Fuera_de_umbral_CON_causa_concilia_con_desviacion()
    {
        var a = Liquidada();
        a.Conciliar(Auditor, ConDesviacion, "Ruta de montaña. Causa: terreno.", Momento);

        Assert.Equal(EstadoDeAsignacion.ConciliadaConDesviacion, a.Estado);
        // Sigue siendo resuelta: `T-21`/`T-22` exigen conciliada «en cualquiera de las dos
        // formas». Una desviación explicada no bloquea el cierre; una sin explicar sí.
        Assert.True(a.EstaResuelta);
    }

    // ── El diario completo ──────────────────────────────────────────────────

    [Fact]
    public void El_recorrido_entero_queda_en_el_diario_con_su_actor()
    {
        var a = Liquidada();
        a.Conciliar(Auditor, Conforme, causa: null, Momento);

        Assert.Equal(["V-01", "V-02", "V-04", "V-07", "V-09"], a.Diario.Select(t => t.Id));

        // Cinco actos, cinco personas distintas. Es el circuito que `BD-06` describe, y el
        // diario es donde se puede comprobar que ocurrió así.
        Assert.Equal(5, a.Diario.Select(t => t.Ejecuta).Distinct().Count());
    }

    private static AsignacionDeCombustible Liquidada()
    {
        var a = Entregada(2_500m);
        a.RegistrarConsumo(Motorista, Carga(30m, 1_500m), Momento);
        a.Liquidar(Contador, 1_000m, null, Momento);
        return a;
    }
}
