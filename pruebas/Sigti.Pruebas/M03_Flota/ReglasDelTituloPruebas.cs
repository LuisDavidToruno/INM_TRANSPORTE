using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// `RN-62` — el título de tenencia con régimen, titular, vigencia y rubros.
///
/// <i>«Sin título vigente el vehículo no se habilita en la flota, y <b>ninguna misión se programa
/// ni se despacha si su ventana excede la vigencia del título</b>»</i>.
/// </summary>
public class ReglasDelTituloPruebas
{
    private static readonly Ulid Vehiculo = Ulid.NewUlid();
    private static readonly DateOnly Desde = new(2026, 1, 1);
    private static readonly DateOnly Hasta = new(2026, 12, 31);

    // ── Lo que el título exige para existir ─────────────────────────────────

    [Fact]
    public void Un_comodato_con_titular_documento_y_vigencia_pasa() =>
        ReglasDelTitulo.ExigirElTitulo(
            RegimenDeTenencia.Comodato, "Secretaría de Salud",
            "Convenio de comodato SS-2026-04", Desde, Hasta);

    [Fact]
    public void Sin_titular_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTitulo.ExigirElTitulo(
                RegimenDeTenencia.Comodato, "  ", "Convenio", Desde, Hasta));

        Assert.Equal("RN-62", error.Precondicion);
        Assert.Contains("no hay a quién devolverle el bien", error.Message);
    }

    /// <summary>
    /// `RN-62` casos límite: <i>«comodato prorrogado verbalmente <b>no existe para el
    /// sistema</b>. La vigencia es la del documento; sin adenda adjunta, el título vence y el
    /// bloqueo opera. <b>Es incómodo y es correcto</b>»</i>.
    /// </summary>
    [Fact]
    public void Sin_documento_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTitulo.ExigirElTitulo(
                RegimenDeTenencia.Comodato, "Secretaría de Salud", "", Desde, Hasta));

        Assert.Contains("prorrogado verbalmente no existe", error.Message);
    }

    /// <summary>
    /// <b>La propiedad es el único régimen que no vence.</b> Ponerle fecha de fin haría que el
    /// vehículo se inhabilitara solo el día que alguien eligió sin que ninguna norma lo mandara.
    /// </summary>
    [Fact]
    public void La_propiedad_no_lleva_fecha_de_fin()
    {
        ReglasDelTitulo.ExigirElTitulo(
            RegimenDeTenencia.Propiedad, "Estado de Honduras", "Matrícula 2019-4471",
            Desde, null);

        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTitulo.ExigirElTitulo(
                RegimenDeTenencia.Propiedad, "Estado de Honduras", "Matrícula", Desde, Hasta));

        Assert.Contains("no vence", error.Message);
    }

    /// <summary>
    /// Y los demás <b>sí la exigen</b>: sin ella el título no vence nunca — y un comodato que no
    /// vence es una apropiación.
    /// </summary>
    [Theory]
    [InlineData(RegimenDeTenencia.Comodato)]
    [InlineData(RegimenDeTenencia.Alquiler)]
    [InlineData(RegimenDeTenencia.DonacionEnTramite)]
    [InlineData(RegimenDeTenencia.AsignacionPorOtraInstitucion)]
    public void Los_regimenes_temporales_exigen_fecha_de_fin(RegimenDeTenencia regimen)
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTitulo.ExigirElTitulo(regimen, "Titular", "Documento", Desde, null));

        Assert.Contains("una apropiación", error.Message);
    }

    // ── Sin título vigente no se habilita ───────────────────────────────────

    [Fact]
    public void Un_vehiculo_sin_titulo_no_se_habilita()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTitulo.ExigirTituloParaHabilitar(null, Desde));

        Assert.Contains("no consta bajo qué régimen lo tenemos", error.Message);
    }

    [Fact]
    public void Un_titulo_vencido_no_habilita()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTitulo.ExigirTituloParaHabilitar(Titulo(), Hasta.AddDays(1)));

        Assert.Contains("una prórroga verbal no existe", error.Message);
    }

    [Fact]
    public void Un_titulo_vigente_habilita() =>
        ReglasDelTitulo.ExigirTituloParaHabilitar(Titulo(), new DateOnly(2026, 6, 1));

    // ── La ventana no excede la vigencia ────────────────────────────────────

    /// <summary>
    /// El mismo patrón de `RN-10` con la licencia: <b>no alcanza con que el título esté vigente
    /// el día de la salida</b>. Un comodato que vence el 20 no ampara una misión que vuelve el
    /// 22 — los dos últimos días el vehículo ya no sería nuestro para usarlo.
    /// </summary>
    [Fact]
    public void Una_ventana_que_termina_despues_del_vencimiento_no_pasa()
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTitulo.ExigirVigenciaEnTodoElRango(
                new TituloAlProgramar([Titulo()]),
                new DateOnly(2026, 12, 29),
                new DateOnly(2027, 1, 3)));

        Assert.Equal("RN-62", error.Precondicion);
        Assert.Contains("tiene que cubrir todo el rango", error.Message);
        Assert.Contains("31/12/2026", error.Message);
    }

    [Fact]
    public void Una_ventana_dentro_de_la_vigencia_pasa()
    {
        var evidencia = ReglasDelTitulo.ExigirVigenciaEnTodoElRango(
            new TituloAlProgramar([Titulo()]),
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 3));

        Assert.Contains("31/12/2026", evidencia);
    }

    /// <summary>La propiedad no vence, así que ninguna ventana la excede.</summary>
    [Fact]
    public void La_propiedad_ampara_cualquier_ventana()
    {
        var evidencia = ReglasDelTitulo.ExigirVigenciaEnTodoElRango(
            new TituloAlProgramar([Titulo(RegimenDeTenencia.Propiedad, sinVencimiento: true)]),
            new DateOnly(2040, 1, 1),
            new DateOnly(2040, 1, 5));

        Assert.Contains("sin vencimiento", evidencia);
    }

    /// <summary>
    /// <b>Sin título se dice, no se bloquea</b> — igual que `BD-07` con el estado nulo. Hay
    /// vehículos cargados antes de que el título existiera, y frenar toda la flota por un dato
    /// de alta que nadie llenó sería peor que la evidencia que falta.
    /// </summary>
    [Fact]
    public void Sin_titulo_registrado_se_declara_en_el_diario()
    {
        var evidencia = ReglasDelTitulo.ExigirVigenciaEnTodoElRango(
            TituloAlProgramar.SinTitulo, Desde, Hasta);

        Assert.Contains("RN-62 NO evaluada", evidencia);
    }

    /// <summary>
    /// De una serie de títulos manda <b>el que regía al salir</b>. Un vehículo que pasó de
    /// comodato a propiedad no se juzga contra el comodato vencido.
    /// </summary>
    [Fact]
    public void De_la_serie_manda_el_que_regia_al_salir()
    {
        var serie = new TituloAlProgramar(
        [
            Titulo(),
            Titulo(RegimenDeTenencia.Propiedad, sinVencimiento: true,
                desde: new DateOnly(2027, 1, 1)),
        ]);

        // En junio de 2026 regía el comodato.
        Assert.Equal(RegimenDeTenencia.Comodato,
            serie.VigenteAl(new DateOnly(2026, 6, 1))?.Regimen);

        // Y en 2027, la propiedad — que ampara una ventana que el comodato no habría cubierto.
        Assert.Contains("sin vencimiento", ReglasDelTitulo.ExigirVigenciaEnTodoElRango(
            serie, new DateOnly(2027, 6, 1), new DateOnly(2027, 6, 3)));
    }

    // ── El bien propio y los rubros ─────────────────────────────────────────

    /// <summary>
    /// <b>Sólo la propiedad hace propio el bien</b>, y de eso depende cuál terminal corresponde
    /// (`HB3-17`). La donación en trámite todavía no lo es: hasta que el traspaso se perfeccione,
    /// darlo de baja del registro sería anticipar un título que no está.
    /// </summary>
    [Theory]
    [InlineData(RegimenDeTenencia.Propiedad, true)]
    [InlineData(RegimenDeTenencia.Comodato, false)]
    [InlineData(RegimenDeTenencia.Alquiler, false)]
    [InlineData(RegimenDeTenencia.DonacionEnTramite, false)]
    [InlineData(RegimenDeTenencia.AsignacionPorOtraInstitucion, false)]
    public void Solo_la_propiedad_hace_propio_el_bien(
        RegimenDeTenencia regimen, bool esperado) =>
        Assert.Equal(esperado,
            Titulo(regimen, sinVencimiento: regimen is RegimenDeTenencia.Propiedad).EsBienPropio);

    /// <summary>
    /// `RN-62` punto 3 — <b>lo que cubre el contrato no se imputa al presupuesto de la
    /// institución</b>. Un mantenimiento que cubre el arrendador y se carga igual es gasto
    /// público pagado dos veces.
    /// </summary>
    [Fact]
    public void El_rubro_dice_a_quien_se_imputa()
    {
        var titulo = Titulo() with
        {
            Rubros = new RubrosDelTitulo(
                Combustible: QuienAsume.Institucion,
                Mantenimiento: QuienAsume.Titular,
                Llantas: QuienAsume.Titular),
        };

        Assert.Equal(QuienAsume.Institucion,
            ReglasDelTitulo.AQuienSeImputa(titulo, "combustible"));

        Assert.Equal(QuienAsume.Titular,
            ReglasDelTitulo.AQuienSeImputa(titulo, "mantenimiento"));

        Assert.Equal(["mantenimiento", "llantas"], titulo.Rubros.DelTitular);
    }

    /// <summary>
    /// <b>«Sin pactar» no es «la institución».</b> Es el rubro que aparece cuando llega la
    /// factura y empieza la discusión con el contrato en la mano, así que se responde nulo y
    /// quien pregunte lo declara.
    /// </summary>
    [Fact]
    public void Un_rubro_sin_pactar_responde_nulo_y_no_la_institucion()
    {
        var titulo = Titulo();

        Assert.Null(ReglasDelTitulo.AQuienSeImputa(titulo, "multas"));
        Assert.Contains("multas", titulo.Rubros.SinPactar);
        Assert.Equal(7, titulo.Rubros.SinPactar.Count);
    }

    [Fact]
    public void Sin_titulo_no_se_puede_decir_a_quien_se_imputa() =>
        Assert.Null(ReglasDelTitulo.AQuienSeImputa(null, "combustible"));

    /// <summary>
    /// Los días restantes van <b>nulos en propiedad</b>: no vence, y mostrar un número inventado
    /// haría que la ficha alertara sobre un vencimiento que no existe.
    /// </summary>
    [Fact]
    public void Los_dias_restantes_son_nulos_en_propiedad()
    {
        Assert.Null(Titulo(RegimenDeTenencia.Propiedad, sinVencimiento: true)
            .DiasRestantes(new DateOnly(2026, 6, 1)));

        Assert.Equal(213, Titulo().DiasRestantes(new DateOnly(2026, 6, 1)));
    }

    private static TituloDeTenencia Titulo(
        RegimenDeTenencia regimen = RegimenDeTenencia.Comodato,
        bool sinVencimiento = false,
        DateOnly? desde = null) =>
        new(Ulid.NewUlid(),
            Vehiculo,
            regimen,
            "Secretaría de Salud",
            "Convenio de comodato SS-2026-04",
            desde ?? Desde,
            sinVencimiento ? null : Hasta,
            new RubrosDelTitulo());
}
