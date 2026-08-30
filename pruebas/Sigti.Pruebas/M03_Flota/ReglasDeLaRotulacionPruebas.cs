using Sigti.Dominio.M03_Flota;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// `RN-18` — la identificación del vehículo del Estado, <b>constatada con fecha y foto</b>.
///
/// ── Lo que esto reemplaza ───────────────────────────────────────────────────
/// Un booleano: <c>IdentificacionInstitucionalVerificada</c>. `CLAUDE.md` lo dice entre las
/// restricciones que condicionan el diseño — <i>«es campo verificable con fecha y foto: es
/// hallazgo frecuente de auditoría»</i>.
///
/// Un booleano en <c>true</c> no dice cuándo se miró, ni quién, ni deja nada que mostrar. Una
/// constatación de hace tres años se ve igual que una de ayer.
/// </summary>
public class ReglasDeLaRotulacionPruebas
{
    private static readonly DateOnly Hoy = new(2026, 9, 1);
    private static readonly DateOnly Ayer = new(2026, 8, 31);

    /// <summary>`RN-18` es literal: <i>«sin fotografía no debe aceptarse»</i>.</summary>
    [Fact]
    public void Una_constatacion_sin_fotografia_no_se_acepta()
    {
        var porQue = ReglasDeLaRotulacion.PorQueNoSeAcepta(tieneFotografia: false);

        Assert.NotNull(porQue);
        Assert.Contains("alguien dijo que miró", porQue);
    }

    [Fact]
    public void Con_fotografia_se_acepta()
    {
        Assert.Null(ReglasDeLaRotulacion.PorQueNoSeAcepta(tieneFotografia: true));
    }

    [Fact]
    public void Los_cuatro_elementos_constatados_y_presentes_dan_constatada()
    {
        var r = ReglasDeLaRotulacion.Evaluar(Todos(Ayer), vigenciaEnDias: 180, Hoy);

        Assert.Equal(EstadoDeLaIdentificacion.Constatada, r.Estado);
        Assert.Empty(r.Faltantes);
        Assert.Empty(r.SinConstatar);
    }

    /// <summary>
    /// <b>Constatar tres no es constatar.</b> Un vehículo puede tener las franjas y no la
    /// leyenda, y decir «rotulación verificada» sobre eso es afirmar de más — que es
    /// exactamente lo que el booleano hacía.
    /// </summary>
    [Fact]
    public void Con_un_elemento_sin_constatar_no_esta_constatada()
    {
        var parciales = Todos(Ayer)
            .Where(c => c.Elemento != ElementoDeIdentificacion.Leyenda)
            .ToList();

        var r = ReglasDeLaRotulacion.Evaluar(parciales, vigenciaEnDias: 180, Hoy);

        Assert.Equal(EstadoDeLaIdentificacion.NoConstatada, r.Estado);
        Assert.Contains(ElementoDeIdentificacion.Leyenda, r.SinConstatar);
    }

    /// <summary>
    /// ⚠️ <b>Un elemento que NO ESTÁ es un hallazgo; uno que nunca se miró es una tarea.</b>
    ///
    /// Reportar «no constatada» sobre un vehículo al que se le vio la leyenda borrada
    /// escondería el hallazgo detrás de la omisión, y son dos cosas con dos destinos distintos.
    /// </summary>
    [Fact]
    public void Un_elemento_ausente_se_reporta_como_hallazgo_y_no_como_omision()
    {
        var conFaltante = Todos(Ayer)
            .Select(c => c.Elemento == ElementoDeIdentificacion.Leyenda
                ? c with { Presente = false }
                : c)
            .ToList();

        var r = ReglasDeLaRotulacion.Evaluar(conFaltante, vigenciaEnDias: 180, Hoy);

        Assert.Equal(EstadoDeLaIdentificacion.ConElementoFaltante, r.Estado);
        Assert.Contains(ElementoDeIdentificacion.Leyenda, r.Faltantes);
        Assert.Contains("hallazgo de auditoría", r.Detalle);
    }

    /// <summary>El hallazgo gana sobre la omisión cuando conviven.</summary>
    [Fact]
    public void Con_un_faltante_y_uno_sin_constatar_manda_el_faltante()
    {
        var mezcla = Todos(Ayer)
            .Where(c => c.Elemento != ElementoDeIdentificacion.Correlativo)
            .Select(c => c.Elemento == ElementoDeIdentificacion.Franjas
                ? c with { Presente = false }
                : c)
            .ToList();

        var r = ReglasDeLaRotulacion.Evaluar(mezcla, vigenciaEnDias: 180, Hoy);

        Assert.Equal(EstadoDeLaIdentificacion.ConElementoFaltante, r.Estado);

        // Y el que falta por mirar sigue reportado: se hacen las dos cosas, no una.
        Assert.Contains(ElementoDeIdentificacion.Correlativo, r.SinConstatar);
    }

    /// <summary>
    /// ⚠️ <b>«Caducada» no dice que la rotulación se haya borrado: dice que nadie la ha vuelto
    /// a mirar.</b> Y esa distinción es la que hace accionable el aviso — una manda a repintar,
    /// la otra manda a ir a ver.
    /// </summary>
    [Fact]
    public void La_constatacion_vieja_caduca_y_el_mensaje_dice_de_que_se_trata()
    {
        var r = ReglasDeLaRotulacion.Evaluar(
            Todos(new DateOnly(2025, 1, 1)), vigenciaEnDias: 180, Hoy);

        Assert.Equal(EstadoDeLaIdentificacion.Caducada, r.Estado);
        Assert.Contains("nadie la ha vuelto a mirar", r.Detalle);

        // Y no la reporta como faltante: la rotulación puede estar perfecta.
        Assert.Empty(r.Faltantes);
    }

    /// <summary>
    /// Sin plazo cargado <b>no caduca</b>. Inventar uno fijaría por omisión una regla que
    /// `RN-18` deja explícitamente configurable — y el detalle lo dice, para que nadie lea
    /// «constatada» como «vigente por siempre».
    /// </summary>
    [Fact]
    public void Sin_plazo_cargado_no_caduca_y_lo_declara()
    {
        var r = ReglasDeLaRotulacion.Evaluar(
            Todos(new DateOnly(2020, 1, 1)), vigenciaEnDias: null, Hoy);

        Assert.Equal(EstadoDeLaIdentificacion.Constatada, r.Estado);
        Assert.Null(r.CaducaEl);
        Assert.Contains("no ha cargado el plazo", r.Detalle);
    }

    /// <summary>
    /// ⚠️ <b>El plazo es más corto sin lámina, y no es un detalle.</b>
    ///
    /// En un vehículo sin lámina la rotulación es su <b>única identificación visible</b> como
    /// bien del Estado. Si caducara al mismo ritmo que la del resto de la flota, el vehículo
    /// que más depende de ella sería el que más tiempo pasa sin que nadie la mire.
    /// </summary>
    [Fact]
    public void Sin_lamina_aplica_el_plazo_mas_corto()
    {
        Assert.Equal(
            90,
            ReglasDeLaRotulacion.VigenciaQueAplica(
                EstadoDePlaca.NumeroAsignadoSinLamina, general: 365, sinLamina: 90));

        Assert.Equal(
            365,
            ReglasDeLaRotulacion.VigenciaQueAplica(
                EstadoDePlaca.ConLamina, general: 365, sinLamina: 90));
    }

    /// <summary>
    /// Sin plazo diferenciado cargado, el sin lámina usa el general. <b>No se inventa uno más
    /// corto</b>: la proporción entre los dos es decisión de la institución, no nuestra.
    /// </summary>
    [Fact]
    public void Sin_plazo_diferenciado_el_sin_lamina_usa_el_general()
    {
        Assert.Equal(
            365,
            ReglasDeLaRotulacion.VigenciaQueAplica(
                EstadoDePlaca.LaminaRetenidaPorAutoridad, general: 365, sinLamina: null));
    }

    /// <summary>Volver a constatar supera lo anterior: se mira la última de cada elemento.</summary>
    [Fact]
    public void Volver_a_constatar_supera_la_constatacion_vieja()
    {
        var historial = Todos(new DateOnly(2025, 1, 1))
            .Concat(Todos(Ayer))
            .ToList();

        var r = ReglasDeLaRotulacion.Evaluar(historial, vigenciaEnDias: 180, Hoy);

        Assert.Equal(EstadoDeLaIdentificacion.Constatada, r.Estado);
    }

    private static List<Constatacion> Todos(DateOnly fecha) =>
    [
        .. ReglasDeLaRotulacion.Obligatorios.Select(e => new Constatacion(
            e, Presente: true, fecha, Ulid.NewUlid(), "P-CUSTODIO", null)),
    ];
}
