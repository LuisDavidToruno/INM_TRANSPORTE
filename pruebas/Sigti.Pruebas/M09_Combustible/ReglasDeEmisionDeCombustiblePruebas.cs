using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// Contra qué se emite un vale y a quién se entrega — `RN-32`.
/// </summary>
public class ReglasDeEmisionDeCombustiblePruebas
{
    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly Ulid Otro = Ulid.NewUlid();
    /// <summary>El ULID del motorista en el padrón. `RN-32` compara registros, no personas.</summary>
    private static readonly Ulid Motorista = Ulid.NewUlid();

    private static readonly Ulid OtroMotorista = Ulid.NewUlid();

    [Theory]
    [InlineData(EstadoDeMision.Borrador)]
    [InlineData(EstadoDeMision.Solicitada)]
    [InlineData(EstadoDeMision.Aprobada)]
    public void Antes_de_PROGRAMADA_no_se_emite(EstadoDeMision estado)
    {
        // `INV-11`: «aprobar no es programar». En `APROBADA` no hay vehículo ni motorista, así
        // que los requisitos de receptor de la propia regla no tendrían contra qué evaluarse.
        var fallo = Assert.Throws<TransicionInvalida>(
            () => ReglasDeEmisionDeCombustible.ExigirEstadoMinimo(estado, EstadoDeMision.Programada));

        Assert.Equal("V-01", fallo.Transicion);
    }

    [Theory]
    [InlineData(EstadoDeMision.Programada)]
    [InlineData(EstadoDeMision.Despachada)]
    [InlineData(EstadoDeMision.EnRuta)]
    public void Desde_PROGRAMADA_en_adelante_si(EstadoDeMision estado)
    {
        // `EN_RUTA` incluido a propósito: la prórroga de `T-17` es un caso real y necesita
        // combustible que no estaba previsto.
        ReglasDeEmisionDeCombustible.ExigirEstadoMinimo(estado, EstadoDeMision.Programada);
    }

    [Theory]
    [InlineData(EstadoDeMision.Rechazada)]
    [InlineData(EstadoDeMision.Anulada)]
    public void Contra_una_mision_rechazada_o_anulada_NO_se_emite(EstadoDeMision estado)
    {
        // **El caso que el orden del enum dejaba pasar.** `Rechazada` y `Anulada` están
        // declaradas después de `Cerrada`, así que una comparación por orden las da por
        // buenas — y un vale contra una misión que no va a ocurrir es un desembolso sin
        // expediente al cual imputarlo.
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDeEmisionDeCombustible.ExigirEstadoMinimo(estado, EstadoDeMision.Programada));

        Assert.Equal("RN-32", fallo.Precondicion);
        Assert.Contains("no va a ocurrir", fallo.Message);
    }

    [Fact]
    public void El_parametro_no_se_puede_configurar_por_debajo_del_piso()
    {
        // `RN-32` sin margen: configurarlo por debajo de `PROGRAMADA` «dejaría los requisitos 2
        // y 3 sin nada contra qué evaluarse». Se rechaza donde el parámetro SE USA, no sólo al
        // cargarlo: si no, basta otra puerta de carga para dejar la regla inerte.
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDeEmisionDeCombustible.ExigirEstadoMinimo(
                EstadoDeMision.Aprobada, minimoConfigurado: EstadoDeMision.Aprobada));

        Assert.Contains("por debajo de Programada", fallo.Message);
    }

    [Fact]
    public void El_piso_declarado_es_PROGRAMADA()
    {
        // Fija el valor en una prueba para que cambiarlo sea una decisión visible y no un
        // efecto secundario de tocar el enum.
        Assert.Equal(EstadoDeMision.Programada, ReglasDeEmisionDeCombustible.PisoDelEstadoMinimo);
    }

    [Fact]
    public void El_vale_no_sale_para_otro_vehiculo()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDeEmisionDeCombustible.ExigirReceptorDeLaOrden(
                Vehiculo, Otro, Motorista, Motorista));

        Assert.Contains("otro tanque", fallo.Message);
    }

    [Fact]
    public void El_vale_no_lo_recibe_otro_motorista_y_el_mensaje_nombra_al_asignado()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDeEmisionDeCombustible.ExigirReceptorDeLaOrden(
                Vehiculo, Vehiculo, Motorista, OtroMotorista));

        // Quien está en la ventanilla necesita saber quién SÍ puede recibir, y por dónde se
        // cambia. «No coincide» lo manda a adivinar las dos cosas.
        Assert.Contains(Motorista.ToString(), fallo.Message);
        Assert.Contains("RN-14", fallo.Message);
    }

    [Fact]
    public void Con_el_vehiculo_y_el_motorista_de_la_orden_procede()
    {
        ReglasDeEmisionDeCombustible.ExigirReceptorDeLaOrden(
            Vehiculo, Vehiculo, Motorista, Motorista);
    }

    [Fact]
    public void Un_vale_de_diesel_no_va_a_un_vehiculo_de_gasolina()
    {
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDeEmisionDeCombustible.ExigirCombustibleCompatible("Gasolina", "Diesel"));

        Assert.Contains("Gasolina", fallo.Message);
        Assert.Contains("Diesel", fallo.Message);
    }

    [Fact]
    public void El_mismo_tipo_pasa_sin_importar_mayusculas()
    {
        ReglasDeEmisionDeCombustible.ExigirCombustibleCompatible("Diesel", "diesel");
    }

    [Fact]
    public void Si_la_ficha_no_declara_el_combustible_no_se_supone_que_coincide()
    {
        // No evaluada nunca se disfraza de conforme: la regla no truena, y no truena porque no
        // tiene con qué comparar — no porque haya comparado y salido bien.
        ReglasDeEmisionDeCombustible.ExigirCombustibleCompatible(null, "Diesel");
        ReglasDeEmisionDeCombustible.ExigirCombustibleCompatible("  ", "Diesel");
    }

    [Fact]
    public void Quien_emitio_no_entrega()
    {
        // `I-03`, bloqueo duro: «es el par que habilita el fraude de combustible más simple».
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDeEmisionDeCombustible.ExigirActorDistinto(
                "entregarla", new IdPersona("P-JEFE"),
                new Dictionary<string, IdPersona> { ["emitió"] = new("P-JEFE") }));

        Assert.Equal("BD-06", fallo.Precondicion);
        Assert.Contains("ya emitió", fallo.Message);
    }

    [Fact]
    public void Personas_distintas_en_cada_eslabon_pasan()
    {
        ReglasDeEmisionDeCombustible.ExigirActorDistinto(
            "liquidarla", new IdPersona("P-CONTABILIDAD"),
            new Dictionary<string, IdPersona>
            {
                ["emitió"] = new("P-JEFE"),
                ["entregó"] = new("P-COMBUSTIBLE"),
                ["consumió"] = new("P-MOTORISTA"),
            });
    }

    [Fact]
    public void El_choque_se_detecta_contra_CUALQUIER_acto_previo_no_solo_el_ultimo()
    {
        // Recorrer sólo el acto anterior dejaría pasar a quien emitió y aparece liquidando tres
        // pasos después — que es el recorrido completo del fraude, no un tropiezo.
        var fallo = Assert.Throws<BloqueoDuro>(
            () => ReglasDeEmisionDeCombustible.ExigirActorDistinto(
                "liquidarla", new IdPersona("P-JEFE"),
                new Dictionary<string, IdPersona>
                {
                    ["emitió"] = new("P-JEFE"),
                    ["entregó"] = new("P-COMBUSTIBLE"),
                    ["consumió"] = new("P-MOTORISTA"),
                }));

        Assert.Contains("ya emitió", fallo.Message);
    }
}
