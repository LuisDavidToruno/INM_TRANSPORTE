using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M18_Peajes;

namespace Sigti.Pruebas.M18_Peajes;

/// <summary>
/// `RN-35` y `RN-38` — el estimado desglosado, y lo que hace cuando no se puede calcular.
///
/// ── Por qué el desglose es la regla y no una preferencia ─────────────────────
/// Un viaje Tegucigalpa → San Pedro Sula atraviesa las tres estaciones del Corredor Logístico;
/// ida y vuelta son <b>6 cruces</b> `[V]`. Sin desglose, el autorizador no puede distinguir un
/// estimado correcto de uno que duplicó un cruce, y <b>el estimado deja de ser un control para
/// volverse un trámite</b>.
/// </summary>
public class ReglasDeEstimacionDePeajesPruebas
{
    private static readonly DateOnly Salida = new(2026, 4, 10);
    private static readonly DateTimeOffset Ahora = new(2026, 3, 16, 8, 0, 0, TimeSpan.FromHours(-6));

    private static readonly CategoriaDePeaje Liviano = new("LIVIANO", "Liviano/Turismo");

    private static readonly PuntoDePeaje Zambrano =
        new(Ulid.NewUlid(), "Zambrano", "COVI-H", "CA-5 Norte");

    private static readonly PuntoDePeaje Comayagua =
        new(Ulid.NewUlid(), "Comayagua", "COVI-H", "CA-5 Norte");

    private static readonly Ulid Vehiculo = Ulid.NewUlid();

    private static CategoriaResuelta Resuelta => new(
        Liviano, BaseDeLaCategoria.VehiculoAsignado, "Resuelta por clase normativa, peso.");

    private static VigenciaDelPunto Activo(PuntoDePeaje p) => new(
        p.Id, EstadoDelPunto.Activo, "Concesión vigente.",
        new DateOnly(2025, 1, 1), null, Ahora);

    private static TarifaDePeaje Tarifa(PuntoDePeaje p, decimal monto, string categoria = "LIVIANO") =>
        new(Ulid.NewUlid(), p.Id, categoria, monto, "SAPP", new DateOnly(2026, 1, 15),
            new DateOnly(2026, 1, 1), null, Ahora);

    private static Estimacion Armar(
        IReadOnlyList<CruceDeRuta> ruta,
        CategoriaResuelta? categoria = null,
        IEnumerable<TarifaDePeaje>? tarifas = null,
        IEnumerable<VigenciaDelPunto>? estados = null,
        IEnumerable<ExoneracionDePeaje>? exoneraciones = null) =>
        ReglasDeEstimacionDePeajes.Armar(
            ruta, categoria ?? Resuelta,
            tarifas ?? [Tarifa(Zambrano, 22m), Tarifa(Comayagua, 22m)],
            estados ?? [Activo(Zambrano), Activo(Comayagua)],
            exoneraciones ?? [],
            Vehiculo, Salida, Ahora);

    // ── El desglose ─────────────────────────────────────────────────────────

    [Fact]
    public void El_estimado_cuenta_CRUCES_y_no_puntos_distintos()
    {
        // El conteo de cruces es lo que más se equivoca al hacerlo a mano, y es la razón entera
        // del desglose: ida y vuelta por dos casetas son cuatro cruces, no dos.
        var e = Armar([new CruceDeRuta(Zambrano, 2), new CruceDeRuta(Comayagua, 2)]);

        Assert.Equal(88m, e.Total);
        Assert.Equal(2, e.Lineas.Count);
        Assert.Equal(44m, e.Lineas[0].Subtotal);
    }

    [Fact]
    public void Cada_linea_dice_su_tarifa_su_vigencia_y_su_fuente()
    {
        var e = Armar([new CruceDeRuta(Zambrano, 2)]);
        var linea = e.Lineas[0];

        Assert.Equal(22m, linea.TarifaUnitaria);
        Assert.NotNull(linea.IdDeLaTarifa);
        Assert.Contains("2 cruce(s) × 22.00", linea.Fundamento);
        Assert.Contains("fuente SAPP", linea.Fundamento);
    }

    [Fact]
    public void La_tarifa_sin_revisar_hace_mas_de_un_anio_se_ADVIERTE_y_no_bloquea()
    {
        // La tarifa cambia al menos una vez al año, en enero. Pero una tarifa vieja sigue siendo
        // la mejor información que hay: bloquear por antigüedad detendría la operación por no
        // haber hecho una gestión administrativa.
        var vieja = Tarifa(Zambrano, 22m) with { FechaDeVerificacion = new DateOnly(2024, 1, 15) };

        var e = Armar([new CruceDeRuta(Zambrano, 2)], tarifas: [vieja]);

        Assert.Equal(44m, e.Total);
        Assert.Contains("SIN REVISAR HACE MÁS DE UN AÑO", e.Lineas[0].Fundamento);
    }

    // ── Cuando no se puede valorar ──────────────────────────────────────────

    [Fact]
    public void Sin_tarifa_cargada_la_linea_NO_vale_cero_sino_que_no_se_valora()
    {
        // `RN-34`: el sistema no calcula un valor por defecto. Un cero indistinguible de un
        // error es peor que la ausencia declarada.
        var e = Armar([new CruceDeRuta(Zambrano, 2)], tarifas: []);

        Assert.Null(e.Total);
        Assert.False(e.Disponible);
        Assert.Null(e.Lineas[0].Subtotal);

        // Y el mensaje es accionable: dice punto, categoría, fecha y a quién pedírselo.
        Assert.Contains("No hay tarifa vigente para el punto «Zambrano»", e.Lineas[0].Fundamento);
        Assert.Contains("Gerencia Administrativa", e.Lineas[0].Fundamento);
    }

    [Fact]
    public void Sin_categoria_resuelta_tampoco_se_inventa_un_monto()
    {
        var sinCategoria = new CategoriaResuelta(
            null, BaseDeLaCategoria.VehiculoAsignado,
            "Falta el número de ejes en la ficha técnica del vehículo.");

        var e = Armar([new CruceDeRuta(Zambrano, 2)], categoria: sinCategoria);

        Assert.Null(e.Total);
        Assert.Contains("Falta el número de ejes", e.Lineas[0].Fundamento);
    }

    [Fact]
    public void Un_estimado_PARCIAL_se_declara_parcial()
    {
        // Un total parcial presentado como completo subestima el costo y produce faltante de
        // efectivo en ruta.
        var e = Armar(
            [new CruceDeRuta(Zambrano, 2), new CruceDeRuta(Comayagua, 2)],
            tarifas: [Tarifa(Zambrano, 22m)]);

        Assert.Equal(44m, e.Total);
        Assert.True(e.Parcial);
        Assert.Single(e.Faltantes);
        Assert.Contains("Comayagua", e.Faltantes[0]);
    }

    [Fact]
    public void Sin_estado_declarado_el_punto_no_se_supone_activo()
    {
        // Suponerlo activo estimaría de más sobre una caseta que quizá cerró; suponerlo cerrado,
        // de menos, y eso es un faltante de efectivo en ruta.
        var e = Armar([new CruceDeRuta(Zambrano, 2)], estados: []);

        Assert.Null(e.Total);
        Assert.Contains("no tiene estado operativo declarado", e.Lineas[0].Fundamento);
    }

    // ── El punto cerrado y la exoneración, que NO son lo mismo ──────────────

    [Fact]
    public void Un_punto_CERRADO_estima_cero_con_su_fundamento()
    {
        var cerrado = new VigenciaDelPunto(
            Zambrano.Id, EstadoDelPunto.Cerrado, "Terminación anticipada de la concesión",
            new DateOnly(2026, 2, 1), null, Ahora);

        var e = Armar([new CruceDeRuta(Zambrano, 2)], estados: [cerrado]);

        Assert.Equal(0m, e.Total);
        Assert.Contains("CERRADO", e.Lineas[0].Fundamento);

        // Y se distingue de la exoneración a propósito: confundirlos haría que al reactivarse
        // el cobro el sistema siguiera estimando cero.
        Assert.Contains("No es exoneración del vehículo", e.Lineas[0].Fundamento);
    }

    [Fact]
    public void Un_vehiculo_EXONERADO_estima_cero_con_el_fundamento_VISIBLE()
    {
        // `RN-35` punto 3: un cero sin explicación es indistinguible de un error de cálculo.
        var exoneracion = new ExoneracionDePeaje(
            Ulid.NewUlid(), Vehiculo, Zambrano.Id, null,
            "Convenio SAPP-INM 2026-004 para unidades de rescate.",
            new DateOnly(2026, 1, 1), null, Ahora);

        var e = Armar([new CruceDeRuta(Zambrano, 2)], exoneraciones: [exoneracion]);

        Assert.Equal(0m, e.Total);
        Assert.Contains("Exonerado: Convenio SAPP-INM", e.Lineas[0].Fundamento);
    }

    [Fact]
    public void La_exoneracion_por_OPERADOR_cubre_todos_sus_puntos()
    {
        // Es como se otorgan: un acuerdo con un concesionario, no caseta por caseta.
        var porOperador = new ExoneracionDePeaje(
            Ulid.NewUlid(), Vehiculo, null, "COVI-H", "Convenio con el concesionario.",
            new DateOnly(2026, 1, 1), null, Ahora);

        var e = Armar(
            [new CruceDeRuta(Zambrano, 2), new CruceDeRuta(Comayagua, 2)],
            exoneraciones: [porOperador]);

        Assert.Equal(0m, e.Total);
        Assert.All(e.Lineas, l => Assert.Contains("Exonerado", l.Fundamento));
    }

    [Fact]
    public void La_exoneracion_VENCIDA_deja_de_aplicar_y_el_estimado_vuelve_a_cobrar()
    {
        // Una estimación que sigue calculando cero con exoneración vencida subestima el costo y
        // produce faltante de efectivo en ruta.
        var vencida = new ExoneracionDePeaje(
            Ulid.NewUlid(), Vehiculo, Zambrano.Id, null, "Convenio 2025.",
            new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31), Ahora);

        var e = Armar([new CruceDeRuta(Zambrano, 2)], exoneraciones: [vencida]);

        Assert.Equal(44m, e.Total);
    }

    [Fact]
    public void La_exoneracion_de_OTRO_vehiculo_no_aplica()
    {
        var deOtro = new ExoneracionDePeaje(
            Ulid.NewUlid(), Ulid.NewUlid(), Zambrano.Id, null, "Ambulancia.",
            new DateOnly(2026, 1, 1), null, Ahora);

        var e = Armar([new CruceDeRuta(Zambrano, 2)], exoneraciones: [deOtro]);

        // El valor por defecto es PAGA. La suposición contraria es la más probable y la más
        // costosa.
        Assert.Equal(44m, e.Total);
    }

    // ── La reautorización por desviación ────────────────────────────────────

    [Fact]
    public void Una_desviacion_dentro_del_umbral_no_exige_reautorizar()
    {
        // Una diferencia de dos lempiras por redondeo de tarifa no es una decisión nueva.
        ReglasDeEstimacionDePeajes.ExigirReautorizacionSiSeDesvio(
            congelado: 88m, recalculado: 92m, umbral: 0.10m, hayReautorizacion: false);
    }

    [Fact]
    public void Pasado_el_umbral_el_despacho_exige_nueva_autorizacion()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeEstimacionDePeajes.ExigirReautorizacionSiSeDesvio(
                congelado: 88m, recalculado: 360m, umbral: 0.10m, hayReautorizacion: false));

        Assert.Contains("pasó de 88.00 a 360.00", error.Message);
        Assert.Contains("ese costo cambió", error.Message);
    }

    [Fact]
    public void Con_la_reautorizacion_registrada_el_despacho_procede()
    {
        ReglasDeEstimacionDePeajes.ExigirReautorizacionSiSeDesvio(
            congelado: 88m, recalculado: 360m, umbral: 0.10m, hayReautorizacion: true);
    }

    [Fact]
    public void Sin_umbral_configurado_NO_se_bloquea()
    {
        // Exigir reautorización por cualquier diferencia detendría toda misión cuyo estimado se
        // afinó al asignar el vehículo, que es lo que se espera que pase.
        ReglasDeEstimacionDePeajes.ExigirReautorizacionSiSeDesvio(
            congelado: 88m, recalculado: 360m, umbral: null, hayReautorizacion: false);
    }

    [Fact]
    public void Un_costo_que_aparece_donde_no_habia_ninguno_siempre_exige_reautorizar()
    {
        // Cero a positivo no tiene proporción que calcular. Se juzga por el hecho: apareció un
        // costo que nadie autorizó.
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeEstimacionDePeajes.ExigirReautorizacionSiSeDesvio(
                congelado: 0m, recalculado: 88m, umbral: 0.10m, hayReautorizacion: false));

        Assert.Contains("no contemplaba peajes", error.Message);
    }

    [Fact]
    public void Una_ruta_sin_puntos_de_peaje_estima_cero_y_esta_disponible()
    {
        // No aplica a misiones dentro de zonas sin peajes: la ausencia se modela como ausencia
        // de puntos en el catálogo, no como excepción codificada.
        var e = Armar([]);

        Assert.Equal(0m, e.Total);
        Assert.True(e.Disponible);
        Assert.False(e.Parcial);
    }
}
