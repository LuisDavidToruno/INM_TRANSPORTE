using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M02_Parametros;

/// <summary>
/// `HU-144` — Carga de un parámetro normativo con vigencia.
///
/// Las dos reglas de rango son las que un desarrollador razonable no escribiría solo:
/// una vigencia no puede solapar con otra, y <b>tampoco puede dejar un hueco</b>. El
/// hueco es el peor de los dos, porque no rompe nada al cargarlo — rompe meses después,
/// cuando alguien liquida una misión de esos días y el cálculo no se puede hacer.
/// </summary>
public class ReglasDeCargaPruebas
{
    private const string Zambrano = "peaje:zambrano:liviana";

    private static readonly IdPersona Carlos = new("P-CARLOS");
    private static readonly IdPersona Gerencia = new("P-GERENCIA");

    private static readonly RespaldoDocumental Respaldo = new(
        Adjunto: Ulid.NewUlid(),
        Fuente: "Acuerdo tarifario publicado por la concesionaria",
        FechaDeVerificacion: new DateOnly(2026, 6, 20));

    /// <summary>La vigente: L 22.00 del 1 de enero al 30 de junio.</summary>
    private static readonly VersionDeParametro[] Existentes =
    [
        new(Clave: Zambrano, Valor: "22.00",
            VigenteDesde: new DateOnly(2026, 1, 1), VigenteHasta: new DateOnly(2026, 6, 30),
            RegistradoDesde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6)),
            RegistradoHasta: null,
            CargadoPor: Carlos, AprobadoPor: Gerencia) { Respaldo = Respaldo }
    ];

    [Fact]
    public void Se_rechaza_la_vigencia_que_deja_un_hueco()
    {
        // Cargar desde el 4 de julio deja el 1, 2 y 3 sin tarifa. Ningún hecho de esos
        // tres días se podría calcular, y nadie lo notaría hasta la primera liquidación.
        var solicitud = Solicitud(desde: new DateOnly(2026, 7, 4));

        var resultado = ReglasDeCarga.Evaluar(solicitud, Existentes);

        Assert.False(resultado.Aceptada);
        Assert.Equal(MotivoDeRechazoDeCarga.DejaHuecoSinVigencia, resultado.Motivo);
    }

    [Fact]
    public void Se_rechaza_la_vigencia_que_solapa_con_otra()
    {
        var solicitud = Solicitud(desde: new DateOnly(2026, 6, 15));

        var resultado = ReglasDeCarga.Evaluar(solicitud, Existentes);

        Assert.False(resultado.Aceptada);
        Assert.Equal(MotivoDeRechazoDeCarga.SolapaConOtraVigencia, resultado.Motivo);
    }

    [Fact]
    public void La_vigencia_contigua_se_acepta()
    {
        // El día siguiente al cierre de la anterior: sin solape y sin hueco.
        var resultado = ReglasDeCarga.Evaluar(Solicitud(desde: new DateOnly(2026, 7, 1)), Existentes);

        Assert.True(resultado.Aceptada);
    }

    [Fact]
    public void La_primera_carga_de_una_clave_no_exige_continuidad()
    {
        // HU-150: la institución arranca con parámetros vacíos. La primera vigencia no
        // tiene predecesora contra la cual dejar hueco.
        var resultado = ReglasDeCarga.Evaluar(Solicitud(desde: new DateOnly(2026, 7, 4)), []);

        Assert.True(resultado.Aceptada);
    }

    [Fact]
    public void Se_rechaza_la_carga_sin_respaldo_documental()
    {
        var solicitud = Solicitud(desde: new DateOnly(2026, 7, 1)) with { Respaldo = null };

        var resultado = ReglasDeCarga.Evaluar(solicitud, Existentes);

        Assert.False(resultado.Aceptada);
        Assert.Equal(MotivoDeRechazoDeCarga.SinRespaldoDocumental, resultado.Motivo);
        Assert.Contains("Tribunal Superior de Cuentas", resultado.Mensaje);
    }

    [Fact]
    public void Se_rechaza_la_carga_sin_declarar_la_fuente()
    {
        var solicitud = Solicitud(desde: new DateOnly(2026, 7, 1),
            respaldo: Respaldo with { Fuente = "   " });

        var resultado = ReglasDeCarga.Evaluar(solicitud, Existentes);

        Assert.False(resultado.Aceptada);
        Assert.Equal(MotivoDeRechazoDeCarga.SinFuenteDeclarada, resultado.Motivo);
    }

    [Fact]
    public void Una_version_superada_en_el_eje_de_transaccion_no_ocupa_lugar()
    {
        // Una versión corregida ya no está en la línea de vigencias: si contara, toda
        // corrección retroactiva volvería imposible cargar la siguiente vigencia.
        var superada = Existentes[0] with
        {
            RegistradoHasta = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.FromHours(-6))
        };

        var resultado = ReglasDeCarga.Evaluar(Solicitud(desde: new DateOnly(2026, 3, 1)), [superada]);

        Assert.True(resultado.Aceptada);
    }

    private static SolicitudDeCarga Solicitud(
        DateOnly desde, DateOnly? hasta = null, RespaldoDocumental? respaldo = null) =>
        new(Clave: Zambrano,
            Valor: "25.00",
            VigenteDesde: desde,
            VigenteHasta: hasta,
            Respaldo: respaldo ?? Respaldo,
            CargadoPor: Carlos);
}
