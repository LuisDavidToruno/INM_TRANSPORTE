using Sigti.Dominio.M19_Seguimiento;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M19_Seguimiento;

/// <summary>
/// `RN-76` y `CE-08` — el tiempo en sitio derivado, y la espera improductiva tipificada.
///
/// <i>«Un vehículo detenido tres horas en una bodega de Choluteca y un vehículo detenido tres
/// horas porque el dispositivo perdió señal son dos hechos completamente distintos»</i>.
/// </summary>
public class ReglasDeLaEstadiaPruebas
{
    private static readonly DateTimeOffset Base = new(2026, 5, 14, 6, 0, 0, TimeSpan.Zero);
    private static readonly Ulid Mision = Ulid.NewUlid();

    private static readonly IReadOnlySet<string> Improductivas =
        new HashSet<string> { "esperando a quien recibe", "sin personal en el destino" };

    private static ReporteDeCampo R(
        TipoDeReporte tipo,
        double horas,
        string? destino = null,
        string? estado = null,
        string? causa = null,
        string? atribuida = null,
        bool? motor = null,
        double? capturaHoras = null) =>
        new()
        {
            Id = Ulid.NewUlid(),
            MisionId = Mision,
            Tipo = tipo,
            Destino = destino,
            Estado = estado,
            MomentoDelHecho = Base.AddHours(horas),
            MomentoDeCaptura = Base.AddHours(capturaHoras ?? horas),
            CausaDeEspera = causa,
            SeAtribuyeA = atribuida,
            MotorEncendido = motor,
            Declara = new IdPersona("motorista-1"),
        };

    // ── Lo derivado, no lo digitado ─────────────────────────────────────────

    [Fact]
    public void El_tiempo_en_sitio_sale_de_los_dos_eventos()
    {
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Bodega Choluteca"),
             R(TipoDeReporte.Salida, 5, "Bodega Choluteca")],
            Base.AddHours(9), Improductivas);

        var e = Assert.Single(r.Estadias);
        Assert.Equal(TimeSpan.FromHours(3), e.Duracion);
        Assert.Equal(ComoSeSupoLaSalida.Declarada, e.Como);
    }

    [Fact]
    public void Los_reportes_que_llegan_de_golpe_se_ordenan_por_la_hora_del_hecho()
    {
        // `HU-057`: el dispositivo estuvo cuatro días sin señal y sube todo junto. En orden de
        // captura la salida llega ANTES que el arribo, y la estadía saldría negativa.
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Salida, 5, "Bodega", capturaHoras: 100),
             R(TipoDeReporte.Arribo, 2, "Bodega", capturaHoras: 101)],
            Base.AddHours(120), Improductivas);

        var e = Assert.Single(r.Estadias);
        Assert.Equal(TimeSpan.FromHours(3), e.Duracion);
        Assert.True(e.Duracion > TimeSpan.Zero);
        Assert.Empty(r.SalidasSinArribo);
    }

    // ── Los tres modos de cerrar ────────────────────────────────────────────

    [Fact]
    public void La_salida_que_nadie_declaro_se_deriva_y_se_marca_como_derivada()
    {
        // `RN-76`, caso límite textual: «el registro señala que la salida fue derivada, no
        // declarada». Sin la marca, un tiempo deducido se leería con la misma confianza que
        // uno declarado.
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Primer destino"),
             R(TipoDeReporte.Arribo, 6, "Segundo destino")],
            Base.AddHours(9), Improductivas);

        Assert.Equal(2, r.Estadias.Count);

        var primera = r.Estadias[0];
        Assert.Equal(ComoSeSupoLaSalida.DerivadaDelSiguienteEvento, primera.Como);
        Assert.Equal(TimeSpan.FromHours(4), primera.Duracion);
    }

    [Fact]
    public void La_estadia_en_curso_no_se_da_por_cerrada()
    {
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Bodega")], Base.AddHours(9), Improductivas);

        var e = Assert.Single(r.Estadias);
        Assert.Equal(ComoSeSupoLaSalida.SinCerrar, e.Como);

        // El reloj corre —son siete horas y hay que verlas— pero la salida es nula: dar por
        // terminada una espera en curso subestimaría justamente lo que se quiere medir.
        Assert.Equal(TimeSpan.FromHours(7), e.Duracion);
        Assert.Null(e.Salida);
    }

    [Fact]
    public void La_salida_sin_arribo_es_un_hueco_visible_y_no_una_estadia_de_cero()
    {
        // Rellenar el arribo con la hora de la salida produciría una estadía de cero minutos
        // que se leería como «no esperó nada» — lo contrario de «no sabemos cuánto esperó».
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Salida, 5, "Bodega")], Base.AddHours(9), Improductivas);

        Assert.Empty(r.Estadias);
        var hueco = Assert.Single(r.SalidasSinArribo);
        Assert.Equal("Bodega", hueco.Destino);
    }

    // ── Espera ≠ espera improductiva ────────────────────────────────────────

    [Fact]
    public void Solo_lo_tipificado_como_improductivo_cuenta_al_indicador()
    {
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Bodega"),
             R(TipoDeReporte.Salida, 5, "Bodega", causa: "carga y descarga",
               atribuida: "Almacén Central"),
             R(TipoDeReporte.Arribo, 6, "Delegación"),
             R(TipoDeReporte.Salida, 9, "Delegación", causa: "esperando a quien recibe",
               atribuida: "Delegación Choluteca")],
            Base.AddHours(10), Improductivas);

        // La carga y descarga es operación normal; las tres horas esperando a quien recibe son
        // un costo atribuible a alguien.
        Assert.Equal(TimeSpan.FromHours(3), r.Improductivo);
        Assert.False(r.Estadias[0].EsImproductiva);
        Assert.True(r.Estadias[1].EsImproductiva);
        Assert.Equal("Delegación Choluteca", r.Estadias[1].SeAtribuyeA);
    }

    [Fact]
    public void Sin_causa_declarada_no_se_clasifica_y_se_cuenta_aparte()
    {
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Bodega"),
             R(TipoDeReporte.Salida, 5, "Bodega")],
            Base.AddHours(9), Improductivas);

        // Nulo, no falso. Colapsarlo a falso reportaría cero horas improductivas cuando lo que
        // pasa es que nadie las tipificó — y el cero se lee como un buen resultado.
        Assert.Null(Assert.Single(r.Estadias).EsImproductiva);
        Assert.Equal(1, r.SinTipificar);
        Assert.Equal(TimeSpan.Zero, r.Improductivo);
    }

    [Fact]
    public void Con_el_catalogo_sin_poblar_nada_se_declara_productivo()
    {
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Bodega"),
             R(TipoDeReporte.Salida, 5, "Bodega", causa: "esperando a quien recibe")],
            Base.AddHours(9), new HashSet<string>());

        // El catálogo vacío no vuelve productiva a la espera: la vuelve inclasificable, y quien
        // muestre el total tiene que decirlo en vez de reportar cero.
        Assert.True(r.SinCatalogoDeCausas);
        Assert.Null(Assert.Single(r.Estadias).EsImproductiva);
        Assert.Equal(1, r.SinTipificar);
    }

    [Fact]
    public void La_causa_declarada_en_ruta_vale_igual_que_la_declarada_al_salir()
    {
        // `RN-76` punto 3: se tipifica al declararla O al salir del sitio. Exigir la segunda
        // vía perdería lo que el motorista ya había dicho.
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Delegación"),
             R(TipoDeReporte.EstadoDeclarado, 3, estado: "en espera",
               causa: "sin personal en el destino", atribuida: "Delegación Choluteca",
               motor: true),
             R(TipoDeReporte.Salida, 5, "Delegación")],
            Base.AddHours(9), Improductivas);

        var e = Assert.Single(r.Estadias);
        Assert.True(e.EsImproductiva);
        Assert.Equal("sin personal en el destino", e.Causa);
        Assert.True(e.MotorEncendido);
    }

    [Fact]
    public void La_espera_por_culpa_propia_se_atribuye_a_la_institucion()
    {
        // `RN-76`, caso límite: «el indicador que solo mide culpas ajenas no lo cree nadie».
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Bodega"),
             R(TipoDeReporte.Salida, 4, "Bodega", causa: "esperando a quien recibe",
               atribuida: "la propia institución")],
            Base.AddHours(9), Improductivas);

        var e = Assert.Single(r.Estadias);
        Assert.True(e.EsImproductiva);
        Assert.Equal("la propia institución", e.SeAtribuyeA);
        Assert.Equal(TimeSpan.FromHours(2), r.Improductivo);
    }

    [Fact]
    public void El_motor_no_preguntado_no_es_el_motor_apagado()
    {
        var r = ReglasDeLaEstadia.Derivar(
            [R(TipoDeReporte.Arribo, 2, "Bodega"),
             R(TipoDeReporte.Salida, 4, "Bodega", causa: "carga y descarga")],
            Base.AddHours(9), Improductivas);

        // Tratarlo como apagado convertiría el silencio en evidencia de un consumo indebido en
        // la conciliación de `RN-30`.
        Assert.Null(Assert.Single(r.Estadias).MotorEncendido);
    }

    [Fact]
    public void Sin_reportes_no_hay_estadias_ni_huecos()
    {
        var r = ReglasDeLaEstadia.Derivar([], Base.AddHours(9), Improductivas);

        Assert.Empty(r.Estadias);
        Assert.Empty(r.SalidasSinArribo);
        Assert.Equal(0, r.SinTipificar);
    }
}
