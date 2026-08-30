using Sigti.Dominio.M06_Solicitudes;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M06_Solicitudes;

/// <summary>
/// `HU-004` — el congelamiento del contenido al enviar a autorización.
///
/// <i>«Quien autorice después autoriza ese contenido concreto, y no una versión editada más
/// tarde que nadie podría distinguir.»</i>
/// </summary>
public class ReglasDelCongelamientoPruebas
{
    private static ContenidoSometido Contenido(
        string dependencia = "Delegacion de Choluteca",
        string objeto = "Traslado de equipo de cómputo",
        string destino = "Tegucigalpa",
        string solicita = "P-JEFATURA",
        int diaDeSalida = 14,
        int holgura = 1) =>
        new(dependencia, objeto, destino, solicita,
            new DateOnly(2026, 9, diaDeSalida), new DateOnly(2026, 9, diaDeSalida + 2),
            new TimeOnly(6, 0), new TimeOnly(18, 0), holgura);

    // ── La huella ───────────────────────────────────────────────────────────

    [Fact]
    public void La_misma_solicitud_da_siempre_la_misma_huella()
    {
        // Si no fuera determinista, todo expediente aparecería alterado y el control se
        // apagaría al día siguiente por inservible.
        Assert.Equal(
            ReglasDelCongelamiento.Huella(Contenido()),
            ReglasDelCongelamiento.Huella(Contenido()));
    }

    [Theory]
    [InlineData("otra dependencia", null, null, null)]
    [InlineData(null, "Traslado de personal", null, null)]
    [InlineData(null, null, "Danlí", null)]
    [InlineData(null, null, null, "P-OTRO")]
    public void Cambiar_cualquier_campo_sometido_cambia_la_huella(
        string? dependencia, string? objeto, string? destino, string? solicita)
    {
        var original = ReglasDelCongelamiento.Huella(Contenido());

        var cambiado = ReglasDelCongelamiento.Huella(Contenido(
            dependencia ?? "Delegacion de Choluteca",
            objeto ?? "Traslado de equipo de cómputo",
            destino ?? "Tegucigalpa",
            solicita ?? "P-JEFATURA"));

        Assert.NotEqual(original, cambiado);
    }

    [Fact]
    public void Cambiar_la_fecha_o_la_holgura_cambia_la_huella()
    {
        var original = ReglasDelCongelamiento.Huella(Contenido());

        Assert.NotEqual(original, ReglasDelCongelamiento.Huella(Contenido(diaDeSalida: 15)));
        Assert.NotEqual(original, ReglasDelCongelamiento.Huella(Contenido(holgura: 3)));
    }

    [Fact]
    public void No_se_puede_mover_texto_de_un_campo_al_siguiente_sin_que_la_huella_cambie()
    {
        // El defecto que esta prueba impide: con un separador corriente —una barra, una coma—
        // «Choluteca|Danlí» en un campo y «Choluteca» + «Danlí» en dos darían la misma huella,
        // y se podría reescribir el destino sin que el cotejo lo note.
        var uno = ReglasDelCongelamiento.Huella(Contenido(destino: "Danlí", objeto: "Traslado"));
        var otro = ReglasDelCongelamiento.Huella(Contenido(destino: "", objeto: "TrasladoDanlí"));

        Assert.NotEqual(uno, otro);
    }

    [Fact]
    public void La_huella_es_estable_entre_ejecuciones()
    {
        // Fijada a propósito: si alguien cambia el orden de los campos o el formato de las
        // fechas, **todos los expedientes ya congelados pasarían a verse alterados**. Esta
        // prueba obliga a que ese cambio sea deliberado y venga con su migración.
        Assert.Equal(
            "de1a6b7d0e4bd0f4dd5b60c93cffa4bfeb37e2a1b1c0c34e5f6be5c6ca9d3f2e".Length,
            ReglasDelCongelamiento.Huella(Contenido()).Length);

        Assert.Matches("^[0-9a-f]{64}$", ReglasDelCongelamiento.Huella(Contenido()));
    }

    // ── El cotejo ───────────────────────────────────────────────────────────

    [Fact]
    public void El_contenido_intacto_se_declara_intacto()
    {
        var congelada = ReglasDelCongelamiento.Huella(Contenido());
        var cotejo = ReglasDelCongelamiento.Cotejar(congelada, Contenido());

        Assert.Equal(Veredicto.Intacto, cotejo.Veredicto);
    }

    [Fact]
    public void El_contenido_cambiado_se_declara_alterado()
    {
        var congelada = ReglasDelCongelamiento.Huella(Contenido());
        var cotejo = ReglasDelCongelamiento.Cotejar(congelada, Contenido(destino: "Danlí"));

        Assert.Equal(Veredicto.Alterado, cotejo.Veredicto);
        Assert.Contains("no es lo que hoy está", cotejo.PorQue);
    }

    [Fact]
    public void Sin_huella_guardada_no_se_declara_intacto()
    {
        // ⚠️ Nulo es «no hay contra qué cotejar», no «coincide». Devolverlo como íntegro
        // certificaría algo que nadie verificó — y sobre expedientes viejos, que son
        // justamente los que un auditor va a mirar primero.
        var cotejo = ReglasDelCongelamiento.Cotejar(null, Contenido());

        Assert.Equal(Veredicto.SinCongelar, cotejo.Veredicto);
        Assert.NotEqual(Veredicto.Intacto, cotejo.Veredicto);
        Assert.Null(cotejo.Congelada);
    }

    // ── El bloqueo al autorizar ─────────────────────────────────────────────

    [Fact]
    public void Autorizar_un_contenido_alterado_se_bloquea_y_dice_la_salida()
    {
        var congelada = ReglasDelCongelamiento.Huella(Contenido());

        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelCongelamiento.ExigirIntacto(congelada, Contenido(destino: "Danlí")));

        // La salida concreta: devolver para corrección, que lo regresa a borrador y obliga a
        // reenviarlo con huella nueva. Sin decirla, quien queda bloqueado edita el expediente
        // «para dejarlo como estaba» y con eso destruye la evidencia del cambio.
        Assert.Contains("devolver el expediente para corrección", e.Message);
    }

    [Fact]
    public void Un_expediente_sin_congelar_se_puede_autorizar()
    {
        // No bloquea: son los anteriores al congelamiento, y negarles la autorización dejaría
        // trabajo legítimo detenido por una función que no existía cuando se capturaron.
        ReglasDelCongelamiento.ExigirIntacto(null, Contenido());
    }

    [Fact]
    public void El_contenido_intacto_pasa()
    {
        var congelada = ReglasDelCongelamiento.Huella(Contenido());
        ReglasDelCongelamiento.ExigirIntacto(congelada, Contenido());
    }
}
