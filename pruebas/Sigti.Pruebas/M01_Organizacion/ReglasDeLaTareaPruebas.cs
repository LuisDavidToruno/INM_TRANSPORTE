using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// La bandeja de §5.3.B.3 — *«queda visiblemente pendiente en la bandeja de alguien»*.
/// </summary>
public class ReglasDeLaTareaPruebas
{
    private static readonly IdPersona Nery = new("P-NERY");
    private static readonly IdPersona Karla = new("P-KARLA");

    private static readonly DateTimeOffset Momento =
        new(2026, 8, 29, 10, 0, 0, TimeSpan.FromHours(-6));

    private static TareaPendiente Tarea(
        EstadoDeTarea estado = EstadoDeTarea.Pendiente,
        DateTimeOffset? notificado = null) =>
        new(Ulid.NewUlid(),
            TipoDeTarea.SegregacionBloqueada,
            "Liquidación bloqueada por I-04",
            "Quien solicitó no declara cómo terminó lo que pidió.",
            "MIS-2026-0031",
            Nery,
            new IdPuesto("PUE-JEFE-TRANSPORTE"),
            [Karla],
            Momento,
            estado,
            notificado);

    /// <summary>
    /// <b>Quien originó la tarea no la resuelve.</b> Es el punto entero del escalamiento:
    /// dejarla resolverla lo convierte en una formalidad — apretaría «resuelto» y seguiría.
    /// </summary>
    [Fact]
    public void Quien_quedo_bloqueado_no_puede_cerrar_su_propia_tarea()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaTarea.ExigirQueNoLaResuelvaQuienLaOrigino(Tarea(), Nery));

        Assert.Contains("se le impidió el acto", error.Message);

        // Y ofrece la salida en vez de dejar un callejón.
        Assert.Contains("escalarla de nuevo", error.Message);
    }

    /// <summary>
    /// El recíproco. Sin él, una regla que bloqueara a todo el mundo pasaría la prueba anterior
    /// y la bandeja no se podría vaciar nunca.
    /// </summary>
    [Fact]
    public void Otra_persona_si_la_puede_resolver()
    {
        ReglasDeLaTarea.ExigirQueNoLaResuelvaQuienLaOrigino(Tarea(), Karla);
    }

    /// <summary>
    /// Resolver <b>exige decir qué se hizo</b>: *«lo autorizó el jefe»* y *«ya no hacía falta»*
    /// dejan el mismo rastro vacío, y son cosas distintas para quien audite.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ok")]
    public void Sin_motivo_no_se_resuelve(string motivo)
    {
        var error = Assert.Throws<BloqueoDuro>(() => ReglasDeLaTarea.ExigirMotivo(motivo));
        Assert.Contains("exige decir qué se hizo", error.Message);
    }

    [Fact]
    public void Con_motivo_se_resuelve()
    {
        ReglasDeLaTarea.ExigirMotivo("Lo autorizó la Gerencia Administrativa por oficio 2026-31.");
    }

    /// <summary>
    /// <b>Una tarea cerrada no se vuelve a cerrar.</b> Dos resoluciones sobre el mismo hecho
    /// dejarían dos versiones de qué pasó, y la pista no podría decir cuál rigió.
    /// </summary>
    [Theory]
    [InlineData(EstadoDeTarea.Resuelta)]
    [InlineData(EstadoDeTarea.Descartada)]
    public void Una_tarea_cerrada_no_se_vuelve_a_cerrar(EstadoDeTarea estado)
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDeLaTarea.ExigirPendiente(Tarea(estado)));

        Assert.Contains("dos versiones de qué pasó", error.Message);
    }

    [Fact]
    public void Una_pendiente_se_puede_cerrar()
    {
        ReglasDeLaTarea.ExigirPendiente(Tarea());
    }

    /// <summary>
    /// <b>Nulo en «notificado» es «no se avisó»</b>, no «se avisó y no contestaron».
    ///
    /// Hoy es siempre nulo —no hay canal construido— y decirlo es lo que impide que una bandeja
    /// llena se lea como gente que ignora su trabajo.
    /// </summary>
    [Fact]
    public void Sin_aviso_lo_dice_en_vez_de_suponerlo()
    {
        Assert.False(Tarea().SeAviso);
        Assert.True(Tarea(notificado: Momento).SeAviso);
    }

    /// <summary>
    /// Los días de espera <b>sólo cuentan mientras espera</b>. Una resuelta llevó los días que
    /// llevó; mostrarlos como espera diría que sigue esperando.
    /// </summary>
    [Fact]
    public void Los_dias_de_espera_solo_corren_en_las_pendientes()
    {
        var dentroDeUnaSemana = Momento.AddDays(7);

        Assert.Equal(7, Tarea().DiasEsperando(dentroDeUnaSemana));
        Assert.Equal(0, Tarea(EstadoDeTarea.Resuelta).DiasEsperando(dentroDeUnaSemana));
    }

    /// <summary>
    /// <b>«Resuelta» y «descartada» son estados distintos a propósito.</b> Descartar dice que
    /// nadie tuvo que hacer nada; un reporte que las junte no puede distinguir el control que
    /// operó del que se volvió innecesario.
    /// </summary>
    [Fact]
    public void Resuelta_y_descartada_no_son_el_mismo_estado()
    {
        Assert.NotEqual(EstadoDeTarea.Resuelta, EstadoDeTarea.Descartada);
    }
}
