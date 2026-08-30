using Sigti.Dominio.M03_Flota;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// `RN-65` — <b>lo que bloquea el despacho de un vehículo sin lámina no es la ausencia de placa:
/// es la ausencia de respaldo.</b>
///
/// ── Lo que esto vino a reemplazar ───────────────────────────────────────────
/// Un booleano, <c>TieneConstanciaSustitutaDePlaca</c>. Decía <b>que hay una constancia</b> y
/// nada más: una vencida a mitad de la misión pasaba exactamente igual que una vigente, y un
/// permiso provisional de treinta días emitido hace un año se veía idéntico a uno de la semana
/// pasada.
///
/// Y no es un caso raro: <b>hay desabastecimiento nacional de láminas</b>. La flota real
/// circula así.
/// </summary>
public class ReglasDelRespaldoDePlacaPruebas
{
    private static readonly DateOnly Salida = new(2026, 9, 4);
    private static readonly DateOnly Fin = new(2026, 9, 8);

    /// <summary>La lámina puesta <b>es</b> la identificación: no exige nada más.</summary>
    [Fact]
    public void Con_lamina_no_se_exige_respaldo()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.ConLamina, respaldo: null, Salida, Fin);

        Assert.True(r.Habilita);
        Assert.Equal(MotivoDeRespaldoInsuficiente.Ninguno, r.Motivo);
    }

    [Fact]
    public void Sin_lamina_y_sin_respaldo_bloquea()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.NumeroAsignadoSinLamina, respaldo: null, Salida, Fin);

        Assert.False(r.Habilita);
        Assert.Equal(MotivoDeRespaldoInsuficiente.SinRespaldo, r.Motivo);

        // El mensaje nombra el estado: «sin placa» y «con la lámina retenida por la DNVT» son
        // dos situaciones distintas, y quien lo lee tiene que saber cuál está mirando.
        Assert.Contains("número asignado, sin lámina", r.Detalle);
    }

    [Fact]
    public void Con_respaldo_que_cubre_todo_el_rango_habilita()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.NumeroAsignadoSinLamina, Respaldo(hasta: Fin), Salida, Fin);

        Assert.True(r.Habilita);
        Assert.Contains("PP-2026-0044", r.Detalle);
    }

    /// <summary>
    /// ⚠️ <b>El caso que el booleano no podía ver.</b>
    ///
    /// Un respaldo que cubre tres de los cinco días de la misión <b>no sirve</b>: el agente que
    /// revise el cuarto tiene enfrente un vehículo del Estado sin lámina y sin nada que lo
    /// explique, y el problema ya no se puede arreglar desde una oficina.
    ///
    /// Mismo patrón que `RN-10` para la licencia.
    /// </summary>
    [Fact]
    public void Un_respaldo_que_vence_a_mitad_del_rango_bloquea()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.NumeroAsignadoSinLamina,
            Respaldo(hasta: new DateOnly(2026, 9, 6)),
            Salida, Fin);

        Assert.False(r.Habilita);
        Assert.Equal(MotivoDeRespaldoInsuficiente.VenceDentroDelRango, r.Motivo);

        // Con las dos fechas: sin ellas, quien lo lee no sabe cuánto le falta al documento.
        Assert.Contains("06/09/2026", r.Detalle);
        Assert.Contains("08/09/2026", r.Detalle);
        Assert.Equal(new DateOnly(2026, 9, 6), r.VenceElQueBloquea);
    }

    /// <summary>
    /// <b>Extremos incluidos</b>: uno que vence el último día del rango sí cubre ese día. Si
    /// bloqueara, cada misión exigiría un respaldo con un día de sobra que nadie pidió.
    /// </summary>
    [Fact]
    public void Un_respaldo_que_vence_el_ultimo_dia_del_rango_alcanza()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.NumeroAsignadoSinLamina, Respaldo(hasta: Fin), Salida, Fin);

        Assert.True(r.Habilita);
    }

    /// <summary>
    /// ⚠️ <b>Sin fecha de vencimiento NO es «vigente para siempre».</b>
    ///
    /// Un documento provisional sin fecha declarada es precisamente lo que hay que preguntar
    /// antes de despachar. Tratarlo como indefinido convertiría el dato faltante en una
    /// autorización, que es la peor forma de resolver una ausencia.
    /// </summary>
    [Fact]
    public void Un_respaldo_sin_fecha_de_vencimiento_no_se_da_por_vigente()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.NumeroAsignadoSinLamina, Respaldo(hasta: null), Salida, Fin);

        Assert.False(r.Habilita);
        Assert.Contains("no declara hasta cuándo vige", r.Detalle);

        // Y dice qué hacer: confirmarlo con el emisor.
        Assert.Contains("confírmelo con el emisor", r.Detalle.ToLowerInvariant());
    }

    [Fact]
    public void Un_respaldo_que_todavia_no_rige_bloquea_y_lo_dice_distinto()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.NumeroAsignadoSinLamina,
            Respaldo(hasta: Fin) with { VigenteDesde = new DateOnly(2026, 9, 10) },
            Salida, Fin);

        Assert.False(r.Habilita);
        Assert.Equal(MotivoDeRespaldoInsuficiente.NoRigeAlSalir, r.Motivo);

        // Distinto de «vencido»: uno todavía no empieza y el otro ya terminó, y el arreglo no
        // es el mismo.
        Assert.Contains("Todavía no ampara nada", r.Detalle);
    }

    /// <summary>
    /// El respaldo declarado y <b>sin el documento adjunto</b> no alcanza.
    ///
    /// El agente en carretera pide el papel: uno que sólo existe como texto en una pantalla no
    /// se le puede mostrar. Es la misma distinción del respaldo del parámetro normativo — el
    /// identificador de un adjunto no es el adjunto.
    /// </summary>
    [Fact]
    public void Un_respaldo_sin_documento_adjunto_no_alcanza()
    {
        var r = ReglasDelRespaldoDePlaca.Evaluar(
            EstadoDePlaca.NumeroAsignadoSinLamina,
            Respaldo(hasta: Fin) with { Adjunto = null },
            Salida, Fin);

        Assert.False(r.Habilita);
        Assert.Equal(MotivoDeRespaldoInsuficiente.SinAdjunto, r.Motivo);
        Assert.Contains("pide el papel", r.Detalle);
    }

    /// <summary>
    /// Los cinco estados sin lámina exigen respaldo. <b>Ninguno es una excepción</b>: la lámina
    /// retenida por la DNVT deja al vehículo tan sin identificar como la que nunca llegó.
    /// </summary>
    [Theory]
    [InlineData(EstadoDePlaca.NumeroAsignadoSinLamina)]
    [InlineData(EstadoDePlaca.SinNumeroAsignado)]
    [InlineData(EstadoDePlaca.LaminaExtraviada)]
    [InlineData(EstadoDePlaca.LaminaRetenidaPorAutoridad)]
    [InlineData(EstadoDePlaca.EnTramiteDeReposicion)]
    public void Todo_estado_sin_lamina_exige_respaldo(EstadoDePlaca estado)
    {
        Assert.False(
            ReglasDelRespaldoDePlaca.Evaluar(estado, respaldo: null, Salida, Fin).Habilita);
    }

    private static RespaldoDePlaca Respaldo(DateOnly? hasta) => new(
        "Permiso provisional de circulación",
        "Instituto de la Propiedad · Registro Vehicular",
        "PP-2026-0044",
        Adjunto: Ulid.NewUlid(),
        VigenteDesde: new DateOnly(2026, 8, 15),
        VigenteHasta: hasta);
}
