using Sigti.Dominio.M03_Flota;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// `BD-03` — Documentación del vehículo vigente.
///
/// Lo que bloquea y lo que no está fijado en la tabla de `BD-03`, y no todo bloquea:
/// la matrícula sí, la placa metálica no, la póliza y la revisión son configurables y
/// vienen <b>apagadas por defecto</b> porque no son obligatorias por ley vigente.
/// </summary>
public class ReglasDeDocumentacionPruebas
{
    private static readonly VentanaDeMision Ventana =
        new(new DateOnly(2026, 3, 12), new DateOnly(2026, 3, 14), HolguraDias: 1);

    [Fact]
    public void Un_vehiculo_sin_placa_metalica_se_puede_despachar()
    {
        // Hay desabastecimiento nacional de placas. «Sin placa metálica» es estado válido,
        // y exige constancia o documento sustitutivo del IP — no bloquea.
        var documentacion = Vigente() with
        {
            Placa = null,
            TieneConstanciaSustitutaDePlaca = true
        };

        var resultado = ReglasDeDocumentacion.Evaluar(documentacion, Ventana, PoliticaDeDocumentacion.PorDefecto);

        Assert.True(resultado.Habilita);
    }

    [Fact]
    public void Sin_placa_y_sin_constancia_sustituta_si_bloquea()
    {
        // Sin placa no bloquea; sin placa y sin constancia, sí: entonces no queda ningún
        // documento que identifique al vehículo en carretera.
        var documentacion = Vigente() with { Placa = null, TieneConstanciaSustitutaDePlaca = false };

        var resultado = ReglasDeDocumentacion.Evaluar(documentacion, Ventana, PoliticaDeDocumentacion.PorDefecto);

        Assert.False(resultado.Habilita);
        Assert.Equal(MotivoDeDocumentacionInsuficiente.SinPlacaNiConstanciaSustituta, resultado.Motivo);
    }

    [Fact]
    public void La_matricula_vencida_dentro_del_rango_bloquea_duro()
    {
        var documentacion = Vigente() with { VenceMatricula = new DateOnly(2026, 3, 14) };

        var resultado = ReglasDeDocumentacion.Evaluar(documentacion, Ventana, PoliticaDeDocumentacion.PorDefecto);

        Assert.False(resultado.Habilita);
        Assert.Equal(MotivoDeDocumentacionInsuficiente.MatriculaVenceDentroDelRango, resultado.Motivo);
    }

    [Fact]
    public void La_poliza_vencida_advierte_pero_no_bloquea_por_defecto()
    {
        // No es obligatoria por ley vigente (DP-001, D-13). Rastreable y alertable
        // siempre: que no bloquee no significa que no quede registrado.
        var documentacion = Vigente() with { VencePoliza = new DateOnly(2026, 1, 1) };

        var resultado = ReglasDeDocumentacion.Evaluar(documentacion, Ventana, PoliticaDeDocumentacion.PorDefecto);

        Assert.True(resultado.Habilita);
        Assert.Contains(MotivoDeDocumentacionInsuficiente.PolizaVenceDentroDelRango, resultado.Advertencias);
    }

    [Fact]
    public void La_poliza_vencida_bloquea_si_la_institucion_lo_configura()
    {
        var documentacion = Vigente() with { VencePoliza = new DateOnly(2026, 1, 1) };
        var politica = PoliticaDeDocumentacion.PorDefecto with { BloquearPorPolizaVencida = true };

        var resultado = ReglasDeDocumentacion.Evaluar(documentacion, Ventana, politica);

        Assert.False(resultado.Habilita);
        Assert.Equal(MotivoDeDocumentacionInsuficiente.PolizaVenceDentroDelRango, resultado.Motivo);
    }

    [Fact]
    public void Una_poliza_ausente_pesa_igual_que_una_vencida()
    {
        // Un vehículo sin póliza registrada no está en mejor situación que uno con la
        // póliza vencida. Tratar el nulo como «sin problema» sería premiar la falta de dato.
        var documentacion = Vigente() with { VencePoliza = null };

        var resultado = ReglasDeDocumentacion.Evaluar(documentacion, Ventana, PoliticaDeDocumentacion.PorDefecto);

        Assert.Contains(MotivoDeDocumentacionInsuficiente.PolizaVenceDentroDelRango, resultado.Advertencias);
    }

    private static DocumentacionDelVehiculo Vigente() => new()
    {
        Placa = "PAA1234",
        TieneConstanciaSustitutaDePlaca = false,
        VenceMatricula = new DateOnly(2027, 1, 1),
        VencePoliza = new DateOnly(2027, 1, 1),
        VenceRevisionMecanica = new DateOnly(2027, 1, 1),
        IdentificacionInstitucionalVerificada = true
    };
}
