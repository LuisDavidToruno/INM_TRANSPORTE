using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M12_Incidentes;

namespace Sigti.Pruebas.M12_Incidentes;

/// <summary>
/// `RN-74` — el registro de campo no captura atribución de responsabilidad.
///
/// ── La razón, en las palabras de la regla ───────────────────────────────────
/// <i>«Un motorista que acaba de tener un accidente, a la orilla de la carretera, con un tercero
/// gritándole, no está en condiciones de calificar jurídicamente lo que pasó — y no le
/// corresponde»</i>. Y la consecuencia práctica: <i>«si registrar el hecho implica autoinculparse,
/// <b>el hecho no se registra</b>. Y un accidente no registrado es peor que cualquier atribución
/// mal hecha»</i>.
/// </summary>
public class ReglasDelRegistroDeIncidentePruebas
{
    // ── Lo que el registro de campo SÍ exige ────────────────────────────────

    [Fact]
    public void Un_registro_con_hecho_causa_y_responsable_pasa() =>
        ReglasDelRegistroDeIncidente.ExigirElHecho(
            "Colisión con vehículo particular",
            "Impacto lateral en el km 42 de la CA-5, sin lesionados aparentes.",
            "P-MOTORISTA",
            "P-TRANSPORTE");

    [Fact]
    public void Sin_descripcion_del_hecho_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelRegistroDeIncidente.ExigirElHecho(
                "Colisión", "   ", "P-MOTORISTA", "P-TRANSPORTE"));

        Assert.Equal("RN-74", error.Precondicion);
        Assert.Contains("nunca de quién fue la culpa", error.Message);
    }

    [Fact]
    public void Sin_causa_del_catalogo_no_pasa() =>
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelRegistroDeIncidente.ExigirElHecho(
                "", "Impacto lateral", "P-MOTORISTA", "P-TRANSPORTE"));

    /// <summary>
    /// `RN-74` punto 4 — el evento abre expediente <b>con responsable de seguimiento y plazo</b>.
    /// Un expediente sin responsable es el mismo expediente muerto que `RN-97` describe.
    /// </summary>
    [Fact]
    public void Sin_responsable_de_seguimiento_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelRegistroDeIncidente.ExigirElHecho(
                "Colisión", "Impacto lateral", "P-MOTORISTA", " "));

        Assert.Contains("sin que nadie lo tenga en la mano", error.Message);
    }

    // ── La determinación es un acto de otra instancia ───────────────────────

    /// <summary>
    /// `RN-74`: <i>«el sistema <b>registra</b> esa determinación cuando existe, con su acto y su
    /// autor; <b>no la produce</b>»</i>. Sin número ni emisor no es un acto: es una opinión
    /// escrita en el expediente por quien no tiene competencia para hacerla.
    /// </summary>
    [Fact]
    public void La_determinacion_sin_numero_ni_instancia_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelRegistroDeIncidente.ExigirActoDeLaInstanciaCompetente(
                new DeterminacionDeResponsabilidad("", "", new DateOnly(2026, 5, 3), "Algo")));

        Assert.Equal("RN-74", error.Precondicion);
        Assert.Contains("SIGTI la registra, no la produce", error.Message);
    }

    [Fact]
    public void La_determinacion_sin_decir_que_resolvio_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelRegistroDeIncidente.ExigirActoDeLaInstanciaCompetente(
                new DeterminacionDeResponsabilidad(
                    "RES-2026-14", "Auditoría Interna", new DateOnly(2026, 5, 3), "  ")));

        Assert.Contains("sin decir cuál", error.Message);
    }

    [Fact]
    public void El_acto_completo_de_la_instancia_competente_pasa() =>
        ReglasDelRegistroDeIncidente.ExigirActoDeLaInstanciaCompetente(
            new DeterminacionDeResponsabilidad(
                "RES-2026-14",
                "Auditoría Interna",
                new DateOnly(2026, 5, 3),
                "Sin responsabilidad atribuible al servidor público."));
}
