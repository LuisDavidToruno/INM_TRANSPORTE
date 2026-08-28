using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// Todo ingreso de combustible al tanque — `RN-83`.
///
/// ── El agujero que esta regla tapa ───────────────────────────────────────────
/// Las siete reglas de `M-09` modelan el consumo <b>del fondo</b>. Un despacho desde el tanque
/// de la sede no pasa por ningún folio y por eso <b>no existía para el sistema</b> — y es
/// exactamente lo que produce un rendimiento imposiblemente bueno: el vehículo recorrió 900 km
/// con 20 galones registrados porque los otros 40 salieron de ahí.
///
/// El efecto es peor que un dato faltante: `RN-30` detecta una desviación y <b>señala un síntoma
/// cuya causa el sistema no puede registrar</b>. El conciliador busca un fraude donde hay un
/// procedimiento no modelado, y cuando el patrón se repite deja de mirar el indicador.
/// </summary>
public class ReglasDeAbastecimientoPruebas
{
    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly Ulid Vale = Ulid.NewUlid();
    private static readonly IdPersona Quien = new("P-MOTORISTA");

    private static readonly DateTimeOffset Momento =
        new(2026, 3, 16, 11, 20, 0, TimeSpan.FromHours(-6));

    private static Abastecimiento DelFondo(bool conComprobante = true) =>
        Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 30m, 84_120,
            FuenteDeAbastecimiento.FondoDeLaMision, Quien,
            asignacion: Vale,
            monto: 1_500m,
            estacion: "Estación Uno",
            comprobante: conComprobante ? "F-0011-9932" : null,
            causaSinComprobante: conComprobante ? null : "Sistema del proveedor caído.");

    // ── Lo que exige todo abastecimiento ────────────────────────────────────

    [Fact]
    public void Sin_galones_no_hay_abastecimiento()
    {
        Assert.Throws<BloqueoDuro>(() => Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 0m, 84_120,
            FuenteDeAbastecimiento.TanqueInstitucional, Quien));
    }

    [Fact]
    public void Sin_odometro_tampoco()
    {
        var fallo = Assert.Throws<BloqueoDuro>(() => Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 30m, 0,
            FuenteDeAbastecimiento.TanqueInstitucional, Quien));

        Assert.Contains("anclado a ningún tramo", fallo.Message);
    }

    // ── Las fuentes, y qué cambia cada una ──────────────────────────────────

    [Fact]
    public void Solo_el_del_fondo_entra_al_cuadre_del_fondo()
    {
        // `RN-83`: lo de otras fuentes entra al denominador de `RN-30` y **no** al cuadre de
        // `RN-29` hasta que exista el acto que corresponda.
        Assert.True(DelFondo().EntraAlCuadreDelFondo);

        var deLaSede = Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 40m, 84_120,
            FuenteDeAbastecimiento.TanqueInstitucional, Quien);

        Assert.False(deLaSede.EntraAlCuadreDelFondo);
    }

    [Fact]
    public void El_peculio_del_servidor_genera_reintegro_y_NO_toca_el_fondo()
    {
        // Si tocara el fondo, el cuadre mentiría en los dos lados a la vez: diría que el fondo
        // pagó un galón que pagó una persona, y que a esa persona no se le debe nada.
        var deSuBolsillo = Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 10m, 84_500,
            FuenteDeAbastecimiento.PeculioDelServidor, Quien,
            monto: 500m, estacion: "Estación en ruta", comprobante: "F-2231");

        Assert.True(deSuBolsillo.GeneraReintegro);
        Assert.False(deSuBolsillo.EntraAlCuadreDelFondo);
    }

    [Fact]
    public void Una_donacion_sin_monto_se_registra_igual()
    {
        // `RN-83`: «un galón sin precio sigue siendo un galón en el denominador». Exigir monto
        // dejaría fuera del cálculo el combustible de una emergencia.
        var donado = Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 25m, 84_800,
            FuenteDeAbastecimiento.Donacion, Quien, monto: null);

        Assert.Equal(25m, donado.Galones);
        Assert.Null(donado.Monto);
        Assert.Contains("sin monto", donado.Descripcion);
    }

    // ── El vínculo con el vale ──────────────────────────────────────────────

    [Fact]
    public void Con_cargo_al_fondo_hay_que_decir_de_que_VALE_salio()
    {
        var fallo = Assert.Throws<BloqueoDuro>(() => Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 30m, 84_120,
            FuenteDeAbastecimiento.FondoDeLaMision, Quien,
            asignacion: null, monto: 1_500m, comprobante: "F-1"));

        Assert.Contains("de qué fondo salió este galón", fallo.Message);
    }

    [Fact]
    public void Una_donacion_NO_se_cuelga_de_un_vale()
    {
        // Vincularla la metería en el cuadre del fondo, que no la pagó — y el cuadre saldría
        // corto por un galón que nadie descontó.
        var fallo = Assert.Throws<BloqueoDuro>(() => Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 25m, 84_800,
            FuenteDeAbastecimiento.Donacion, Quien, asignacion: Vale));

        Assert.Contains("no lo pagó", fallo.Message);
    }

    // ── RN-85: el papel ─────────────────────────────────────────────────────

    [Fact]
    public void Sin_comprobante_y_sin_causa_una_compra_no_se_registra()
    {
        var fallo = Assert.Throws<BloqueoDuro>(() => Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 30m, 84_120,
            FuenteDeAbastecimiento.FondoDeLaMision, Quien,
            asignacion: Vale, monto: 1_500m, comprobante: null, causaSinComprobante: null));

        Assert.Equal("RN-85", fallo.Precondicion);
        Assert.Contains("tampoco se disimula", fallo.Message);
    }

    [Fact]
    public void Con_causa_declarada_si_se_registra()
    {
        // «El registro del abastecimiento no se omite nunca por falta de papel.»
        var a = DelFondo(conComprobante: false);

        Assert.Contains("SIN COMPROBANTE", a.Descripcion);
        Assert.Contains("Sistema del proveedor caído", a.Descripcion);
    }

    [Theory]
    [InlineData(FuenteDeAbastecimiento.TanqueInstitucional)]
    [InlineData(FuenteDeAbastecimiento.Donacion)]
    [InlineData(FuenteDeAbastecimiento.OtraDependencia)]
    [InlineData(FuenteDeAbastecimiento.TerceroEnApoyo)]
    public void A_quien_no_genera_factura_no_se_le_pide_causa(FuenteDeAbastecimiento fuente)
    {
        // Exigirla obligaría a escribir «no aplica» en cada registro, y una casilla que siempre
        // dice lo mismo deja de leerse — con ella se pierde la que sí significaba algo.
        Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 40m, 84_120, fuente, Quien);
    }

    [Theory]
    [InlineData(FuenteDeAbastecimiento.FondoDeLaMision, true)]
    [InlineData(FuenteDeAbastecimiento.PeculioDelServidor, true)]
    [InlineData(FuenteDeAbastecimiento.TanqueInstitucional, false)]
    [InlineData(FuenteDeAbastecimiento.Donacion, false)]
    public void Cuales_fuentes_deberian_traer_papel(FuenteDeAbastecimiento fuente, bool esperado) =>
        Assert.Equal(esperado, ReglasDeAbastecimiento.DeberiaTraerComprobante(fuente));

    // ── El excedido ─────────────────────────────────────────────────────────

    [Fact]
    public void Lo_que_EXCEDE_el_fondo_se_registra_marcado_no_se_omite()
    {
        // `RN-83` punto 6: «su cobertura se resuelve en la liquidación, nunca omitiendo el
        // registro». Omitirlo dejaría el galón fuera del denominador de `RN-30`, que es donde
        // más falta hace.
        var a = Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 30m, 84_120,
            FuenteDeAbastecimiento.FondoDeLaMision, Quien,
            asignacion: Vale, monto: 1_500m, comprobante: "F-1", excedido: true);

        Assert.True(a.Excedido);
        Assert.Contains("EXCEDE el fondo asignado", a.Descripcion);
    }

    // ── El nivel de tanque ──────────────────────────────────────────────────

    [Fact]
    public void Dos_niveles_de_la_MISMA_escala_se_restan()
    {
        var salida = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 1m);
        var retorno = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 0.25m);

        Assert.Equal(-0.75m, salida.DiferenciaCon(retorno));
    }

    [Fact]
    public void Un_octavo_de_indicador_y_quince_galones_NO_se_restan()
    {
        // `RN-83` punto 2: la escala se registra porque «un octavo de tanque no es lo mismo en
        // un pickup que en un bus». Convertir una en otra exige la capacidad del tanque, que la
        // ficha técnica no declara — así que se dice que no se puede, en vez de inventar.
        var enFraccion = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 0.125m);
        var enGalones = new NivelDeTanque(EscalaDeNivel.Galones, 15m);

        Assert.Null(enFraccion.DiferenciaCon(enGalones));
    }

    [Fact]
    public void Salir_lleno_y_volver_a_un_octavo_es_MUY_DISTINTO()
    {
        // Es lo que vuelve no concluyente la conciliación: los galones consumidos no son los
        // cargados, y el rendimiento observado no significa nada.
        var lleno = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 1m);
        var casiVacio = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 0.125m);

        Assert.True(lleno.MuyDistintoDe(casiVacio));
    }

    [Fact]
    public void Volver_con_un_octavo_menos_NO_es_muy_distinto()
    {
        // El caso normal de una misión corta. Si esto marcara reparo, toda conciliación saldría
        // no concluyente y el control dejaría de decir nada.
        var lleno = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 1m);
        var algoMenos = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 0.875m);

        Assert.False(lleno.MuyDistintoDe(algoMenos));
    }

    [Fact]
    public void En_GALONES_la_referencia_es_lo_que_llevaba_al_salir()
    {
        // Veinte galones de cuarenta es la mitad: muy distinto. Veinte de cien no lo es, y un
        // umbral absoluto habría dicho lo mismo de los dos.
        var conCuarenta = new NivelDeTanque(EscalaDeNivel.Galones, 40m);
        var conCien = new NivelDeTanque(EscalaDeNivel.Galones, 100m);

        Assert.True(conCuarenta.MuyDistintoDe(new NivelDeTanque(EscalaDeNivel.Galones, 20m)));
        Assert.False(conCien.MuyDistintoDe(new NivelDeTanque(EscalaDeNivel.Galones, 80m)));
    }

    [Fact]
    public void Escalas_distintas_devuelven_NULO_no_falso()
    {
        // «No se puede comparar» y «no hay diferencia» son cosas opuestas. Dar por parejo lo que
        // no se midió es justo lo que `RN-80` prohíbe al decir que el campo no consignado no se
        // estima.
        var enFraccion = new NivelDeTanque(EscalaDeNivel.FraccionDelIndicador, 1m);
        var enGalones = new NivelDeTanque(EscalaDeNivel.Galones, 15m);

        Assert.Null(enFraccion.MuyDistintoDe(enGalones));
    }

    [Fact]
    public void La_descripcion_dice_la_FUENTE_porque_es_lo_que_cambia_el_juicio()
    {
        // `RN-30` punto 4: la conciliación «usa todos los abastecimientos del período, no solo
        // los del fondo, y expone la fuente de cada uno». Sin la fuente, cuarenta galones del
        // tanque de la sede y cuarenta comprados se leen igual.
        var deLaSede = Abastecimiento.Registrar(
            Ulid.NewUlid(), Vehiculo, Momento, 40m, 84_120,
            FuenteDeAbastecimiento.TanqueInstitucional, Quien);

        Assert.Contains("el tanque institucional", deLaSede.Descripcion);
    }
}
