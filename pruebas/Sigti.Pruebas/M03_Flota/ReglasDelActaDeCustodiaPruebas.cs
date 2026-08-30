using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// `RN-22` — el <b>traslado temporal de custodia</b> al motorista, con acta en los dos extremos.
///
/// ── La pregunta que esto contesta ───────────────────────────────────────────
/// <i>«¿Quién tenía el vehículo en ese momento, y con qué?»</i> Es la que aparece cuando algo
/// falta o algo se daña, y sin cadena de custodia <b>la deducción de responsabilidad no tiene
/// sobre quién recaer</b> — lo que ante el TSC agrava en vez de atenuar.
///
/// ── Y el cotejo es el producto ──────────────────────────────────────────────
/// Un acta de entrega con cinco elementos y una devolución con cuatro son, por separado, dos
/// listas que nadie lee. El gato que no volvió tiene nombre, fecha y dos personas.
/// </summary>
public class ReglasDelActaDeCustodiaPruebas
{
    private static readonly IdPersona Custodio = new("P-CUSTODIO");
    private static readonly IdPersona Motorista = new("P-MOTORISTA");

    private static readonly DateTimeOffset Salida =
        new(2026, 9, 4, 7, 0, 0, TimeSpan.FromHours(-6));

    private static readonly DateTimeOffset Retorno =
        new(2026, 9, 6, 17, 0, 0, TimeSpan.FromHours(-6));

    // ── Qué se puede registrar ──────────────────────────────────────────────

    [Fact]
    public void La_entrega_se_registra_con_estado_declarado()
    {
        Assert.Null(ReglasDelActaDeCustodia.PorQueNoSeRegistra(
            TipoDeActa.Entrega, hayEntregaPrevia: false, yaHayDeLaMismaClase: false,
            "Carrocería sin golpes. Llanta delantera derecha con desgaste."));
    }

    /// <summary>
    /// <b>Sin estado declarado no hay acta.</b> Es lo que después distingue un golpe que ya
    /// venía de uno que ocurrió en la misión — y sin esa distinción la deducción de
    /// responsabilidad recae sobre quien no corresponde.
    /// </summary>
    [Fact]
    public void Sin_estado_de_la_unidad_no_se_registra()
    {
        var porQue = ReglasDelActaDeCustodia.PorQueNoSeRegistra(
            TipoDeActa.Entrega, hayEntregaPrevia: false, yaHayDeLaMismaClase: false, "  ");

        Assert.NotNull(porQue);
        Assert.Contains("un golpe que ya venía", porQue);
    }

    /// <summary>
    /// ⚠️ <b>Una devolución sin entrega no tiene contra qué compararse</b>, y comparar es lo
    /// único para lo que el acta sirve: sin la de salida, nadie puede decir qué faltó.
    /// </summary>
    [Fact]
    public void No_se_registra_una_devolucion_sin_entrega_previa()
    {
        var porQue = ReglasDelActaDeCustodia.PorQueNoSeRegistra(
            TipoDeActa.Devolucion, hayEntregaPrevia: false, yaHayDeLaMismaClase: false, "Sin novedad.");

        Assert.NotNull(porQue);
        Assert.Contains("nadie puede decir qué faltó", porQue);
    }

    /// <summary>
    /// Dos entregas dejarían <b>dos inventarios distintos del mismo vehículo</b>, y el cotejo
    /// del retorno se quedaría sin saber contra cuál correr.
    /// </summary>
    [Fact]
    public void No_se_registran_dos_entregas_de_la_misma_mision()
    {
        var porQue = ReglasDelActaDeCustodia.PorQueNoSeRegistra(
            TipoDeActa.Entrega, hayEntregaPrevia: true, yaHayDeLaMismaClase: true, "Sin novedad.");

        Assert.NotNull(porQue);
        Assert.Contains("dos inventarios distintos", porQue);
    }

    // ── El cotejo ───────────────────────────────────────────────────────────

    [Fact]
    public void Con_todo_devuelto_el_cotejo_no_reporta_faltantes()
    {
        var c = ReglasDelActaDeCustodia.Cotejar(Entrega(), Devolucion());

        Assert.Empty(c.NoVolvieron);
        Assert.Empty(c.NoSeEntregaron);
        Assert.Equal(420, c.KilometrosRecorridos);
        Assert.Contains("volvieron completos", c.Veredicto);
    }

    /// <summary>
    /// ⚠️ <b>El hallazgo va con nombre.</b> «Faltan 2 elementos» no le sirve a nadie que tenga
    /// que deducir responsabilidad: hay que poder decir qué, de quién y cuándo.
    /// </summary>
    [Fact]
    public void Lo_que_no_volvio_se_nombra()
    {
        var sinGato = Devolucion() with
        {
            Elementos = [.. Devolucion().Elementos.Where(e => e.Nombre != "Gato hidráulico")],
        };

        var c = ReglasDelActaDeCustodia.Cotejar(Entrega(), sinGato);

        Assert.Contains("Gato hidráulico", c.NoVolvieron);
        Assert.Contains("Gato hidráulico", c.Veredicto);
        Assert.Contains("RN-22", c.Veredicto);
        Assert.Contains("la deducción recae", c.Veredicto);
    }

    /// <summary>
    /// El elemento <b>listado y marcado ausente</b> cuenta como no devuelto: declararlo en la
    /// lista con una cruz es la forma normal de decir que no volvió.
    /// </summary>
    [Fact]
    public void Un_elemento_marcado_ausente_en_la_devolucion_cuenta_como_faltante()
    {
        var conCruz = Devolucion() with
        {
            Elementos =
            [
                .. Devolucion().Elementos.Select(e => e.Nombre == "Extintor"
                    ? e with { Presente = false, Observacion = "Se usó en un conato en ruta." }
                    : e),
            ],
        };

        var c = ReglasDelActaDeCustodia.Cotejar(Entrega(), conCruz);

        Assert.Contains("Extintor", c.NoVolvieron);
    }

    /// <summary>
    /// Quien llena el acta del retorno escribe «Gato Hidráulico» donde la de salida decía «gato
    /// hidráulico». Un cotejo sensible a la caja produciría <b>un faltante y un agregado
    /// inventados</b> — dos hallazgos falsos de una diferencia de mayúsculas.
    /// </summary>
    [Fact]
    public void El_cotejo_no_se_confunde_por_mayusculas()
    {
        var otraCaja = Devolucion() with
        {
            Elementos =
            [
                .. Devolucion().Elementos.Select(e => e.Nombre == "Gato hidráulico"
                    ? e with { Nombre = "GATO HIDRÁULICO" }
                    : e),
            ],
        };

        var c = ReglasDelActaDeCustodia.Cotejar(Entrega(), otraCaja);

        Assert.Empty(c.NoVolvieron);
        Assert.Empty(c.NoSeEntregaron);
    }

    /// <summary>
    /// Lo que aparece sin constar en la entrega <b>no es un hallazgo</b>: suele ser un elemento
    /// que se olvidó anotar al salir, y a veces uno que el motorista repuso de su bolsillo. Se
    /// dice, sin acusar.
    /// </summary>
    [Fact]
    public void Lo_que_aparece_sin_constar_en_la_entrega_se_dice_sin_acusar()
    {
        var conExtra = Devolucion() with
        {
            Elementos =
            [
                .. Devolucion().Elementos,
                new ElementoDeLaUnidad("Triángulo reflectivo", true, null),
            ],
        };

        var c = ReglasDelActaDeCustodia.Cotejar(Entrega(), conExtra);

        Assert.Contains("Triángulo reflectivo", c.NoSeEntregaron);
        Assert.Empty(c.NoVolvieron);
        Assert.Contains("repusieran en ruta", c.Veredicto);
    }

    /// <summary>
    /// ⚠️ <b>Un odómetro que retrocede no son cero kilómetros.</b>
    ///
    /// Significa que se reinició, se sustituyó, o alguien tecleó mal — y las tres exigen
    /// mirarlo. Devolver cero enterraría el problema dentro de un número que parece normal.
    /// </summary>
    [Fact]
    public void Un_odometro_que_retrocede_deja_el_recorrido_sin_calcular()
    {
        var alReves = Devolucion() with { Odometro = 84_000 };

        var c = ReglasDelActaDeCustodia.Cotejar(Entrega(), alReves);

        Assert.Null(c.KilometrosRecorridos);
        Assert.Contains("exige revisarlo", c.Veredicto);
    }

    /// <summary>
    /// La diferencia de tanque es <b>nula si alguno de los dos niveles no se leyó</b>. No cero:
    /// cero diría que el vehículo volvió con el mismo combustible con que salió.
    /// </summary>
    [Fact]
    public void Sin_una_de_las_dos_lecturas_la_diferencia_de_tanque_no_se_calcula()
    {
        var sinLectura = Devolucion() with { NivelDeTanque = null };

        Assert.Null(ReglasDelActaDeCustodia.Cotejar(Entrega(), sinLectura).DiferenciaDeTanque);
    }

    private static ActaDeCustodia Entrega() => new(
        TipoDeActa.Entrega, Custodio, Motorista, Salida,
        Odometro: 84_580,
        NivelDeTanque: 1.0m,
        "Carrocería sin golpes. Llanta delantera derecha con desgaste.",
        [
            new ElementoDeLaUnidad("Gato hidráulico", true, null),
            new ElementoDeLaUnidad("Llave de ruedas", true, null),
            new ElementoDeLaUnidad("Llanta de repuesto", true, "Con presión baja."),
            new ElementoDeLaUnidad("Extintor", true, null),
            new ElementoDeLaUnidad("Botiquín", false, "No se entregó: no había en existencia."),
        ],
        null);

    private static ActaDeCustodia Devolucion() => new(
        TipoDeActa.Devolucion, Motorista, Custodio, Retorno,
        Odometro: 85_000,
        NivelDeTanque: 0.25m,
        "Sin novedad.",
        [
            new ElementoDeLaUnidad("Gato hidráulico", true, null),
            new ElementoDeLaUnidad("Llave de ruedas", true, null),
            new ElementoDeLaUnidad("Llanta de repuesto", true, null),
            new ElementoDeLaUnidad("Extintor", true, null),
        ],
        null);
}
