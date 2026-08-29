using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M01_Organizacion;

/// <summary>
/// El aviso al destinatario — <b>§5.3.B.3, la mitad que faltaba</b>.
///
/// ── Lo que estas pruebas defienden ──────────────────────────────────────────
/// Que el sistema <b>nunca diga haber avisado cuando no avisó</b>, y que distinga las tres
/// razones por las que un aviso no sale — porque las arreglan personas distintas: la
/// institución eligiendo el canal, quien programa construyéndolo, o quien opera la
/// infraestructura.
/// </summary>
public class ReglasDelAvisoPruebas
{
    private static readonly IdPersona Karla = new("P-KARLA");
    private static readonly Ulid Tarea = Ulid.NewUlid();

    private static readonly DateTimeOffset Momento =
        new(2026, 8, 29, 10, 0, 0, TimeSpan.FromHours(-6));

    private static Aviso Resolver(
        CanalDeAviso? canal, IReadOnlyList<CanalDeAviso>? implementados = null) =>
        ReglasDelAviso.Resolver(
            Ulid.NewUlid(), Tarea, Karla, canal,
            implementados ?? ReglasDelAviso.Implementados, Momento);

    // ── Las tres razones por las que no sale ────────────────────────────────

    /// <summary>
    /// <b>Sin canal fijado, el aviso NO se da por entregado.</b>
    ///
    /// Es la distinción que separa *«no contestó»* de *«nadie le escribió»*, y sin ella una
    /// bandeja llena se lee como gente que ignora su trabajo.
    /// </summary>
    [Fact]
    public void Sin_canal_fijado_lo_declara_en_vez_de_darlo_por_entregado()
    {
        var aviso = Resolver(canal: null);

        Assert.Equal(ResultadoDelAviso.SinCanalConfigurado, aviso.Resultado);
        Assert.False(aviso.LlegoAlDestinatario);
        Assert.Null(aviso.Canal);
        Assert.Contains("nadie le escribió", aviso.Detalle);
    }

    /// <summary>
    /// <b>«No lo fijaron» y «no está construido» son cosas distintas</b>, y por eso son dos
    /// resultados: la primera la resuelve la institución eligiendo; la segunda, quien programa.
    /// </summary>
    [Fact]
    public void Un_canal_fijado_pero_sin_construir_se_distingue_del_que_nadie_fijo()
    {
        var aviso = Resolver(CanalDeAviso.CorreoInstitucional);

        Assert.Equal(ResultadoDelAviso.CanalNoImplementado, aviso.Resultado);
        Assert.Equal(CanalDeAviso.CorreoInstitucional, aviso.Canal);
        Assert.Contains("pendiente de construcción", aviso.Detalle);

        // Y no se confunde con el otro caso.
        Assert.NotEqual(ResultadoDelAviso.SinCanalConfigurado, aviso.Resultado);
    }

    /// <summary>
    /// Los dos canales que exigen infraestructura ajena <b>no están implementados</b>, y
    /// declararlos antes de tiempo haría que el sistema dijera «entregado» sobre un envío que
    /// nunca salió.
    /// </summary>
    [Theory]
    [InlineData(CanalDeAviso.CorreoInstitucional)]
    [InlineData(CanalDeAviso.MensajeDeTexto)]
    public void Correo_y_mensaje_de_texto_no_estan_implementados(CanalDeAviso canal)
    {
        Assert.DoesNotContain(canal, ReglasDelAviso.Implementados);
    }

    // ── El canal que sí opera ───────────────────────────────────────────────

    /// <summary>
    /// <b><c>SoloBandeja</c> entrega de verdad</b>, y no es un rodeo.
    ///
    /// La tarea ya está en la bandeja cuando esto corre. Si la institución elige ese canal, el
    /// aviso se cumplió: lo que cambia no es si la persona puede enterarse, sino si tiene que
    /// entrar a mirar. Marcarlo como fallo diría que el sistema no hizo lo que se le pidió.
    ///
    /// Y es un canal legítimo, no un consuelo: en una delegación sin señal el correo y el SMS no
    /// llegan, y más de dos millones de personas del área rural no tienen internet.
    /// </summary>
    [Fact]
    public void Solo_bandeja_es_un_canal_que_entrega()
    {
        var aviso = Resolver(CanalDeAviso.SoloBandeja);

        Assert.Equal(ResultadoDelAviso.Entregado, aviso.Resultado);
        Assert.True(aviso.LlegoAlDestinatario);

        // Nulo cuando se entregó: un motivo para un envío exitoso es ruido en la pista.
        Assert.Null(aviso.Detalle);
    }

    // ── El parámetro, resuelto a la fecha del hecho ─────────────────────────

    /// <summary>
    /// El canal <b>se lee del catálogo con vigencia</b>, no de una constante. Cablearlo obligaría
    /// a un despliegue el día que una delegación consiga señal, y a otro el día que la pierda.
    /// </summary>
    [Fact]
    public void El_canal_sale_del_catalogo_a_la_fecha_del_hecho()
    {
        var catalogo = new CatalogoDeParametros(
        [
            Version("SoloBandeja", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30)),
            Version("CorreoInstitucional", new DateOnly(2026, 7, 1), null),
        ]);

        Assert.Equal(
            CanalDeAviso.SoloBandeja,
            ReglasDelAviso.CanalVigente(catalogo, new DateOnly(2026, 5, 1), Momento));

        Assert.Equal(
            CanalDeAviso.CorreoInstitucional,
            ReglasDelAviso.CanalVigente(catalogo, new DateOnly(2026, 8, 1), Momento));
    }

    /// <summary>
    /// <b>Sin la clave cargada devuelve nulo, no bloquea.</b>
    ///
    /// La frontera de <c>ResolverSiHay</c> lo explica: se bloquea lo que decide un número que
    /// alguien va a cobrar o pagar. Un aviso ausente no cambia ningún monto —deja a alguien sin
    /// enterarse, que es grave y es otra cosa—, y bloquear el encolado por falta de canal
    /// dejaría el acto <b>sin bandeja y sin aviso</b>.
    /// </summary>
    [Fact]
    public void Sin_la_clave_cargada_devuelve_nulo_y_no_bloquea()
    {
        var vacio = new CatalogoDeParametros([]);

        Assert.Null(ReglasDelAviso.CanalVigente(vacio, new DateOnly(2026, 8, 29), Momento));
    }

    /// <summary>
    /// <b>Un valor que no corresponde a ningún canal no se aproxima al más parecido.</b>
    ///
    /// Se trata como si no estuviera fijado. Interpretar «correo» como
    /// <c>CorreoInstitucional</c> parecería servicial y produciría un sistema que cree haber
    /// avisado por un canal que nadie configuró.
    /// </summary>
    [Fact]
    public void Un_valor_que_no_es_un_canal_se_trata_como_no_fijado()
    {
        var catalogo = new CatalogoDeParametros(
            [Version("paloma mensajera", new DateOnly(2026, 1, 1), null)]);

        Assert.Null(ReglasDelAviso.CanalVigente(catalogo, new DateOnly(2026, 8, 29), Momento));
    }

    /// <summary>
    /// Una versión <b>aprobada</b>: sin aprobar no resuelve, y el doble control de `HU-145`
    /// sería decorativo.
    /// </summary>
    private static VersionDeParametro Version(string valor, DateOnly desde, DateOnly? hasta) =>
        new(ReglasDelAviso.ClaveDelCanal,
            valor,
            desde,
            hasta,
            new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero),
            null,
            new IdPersona("P-ADMIN"),
            new IdPersona("P-GERENCIA"))
        {
            Respaldo = new RespaldoDocumental(
                Ulid.NewUlid(), "Insumo #102 — decisión de la institución", desde),
        };
}
