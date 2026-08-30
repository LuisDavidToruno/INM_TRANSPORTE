using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M17_PersonasExternas;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M17_PersonasExternas;

/// <summary>
/// `RN-53` y `HU-113` — el manifiesto que se cierra al despachar.
///
/// <i>«Si se puede editar después, deja de ser una declaración y pasa a ser un resumen ajustado
/// a lo que ocurrió — que es exactamente lo contrario de un control.»</i>
/// </summary>
public class ReglasDelManifiestoPruebas
{
    private static readonly DateTimeOffset Salida = new(2026, 9, 14, 6, 0, 0, TimeSpan.Zero);

    private static PersonaEnManifiesto Persona(
        string? nombre = "María Elena Zúniga",
        string? identificacion = "0801-1990-12345",
        FormaDeIdentificacion forma = FormaDeIdentificacion.Documento) =>
        new(nombre, identificacion, forma, "Traslado a audiencia", "Choluteca", "Tegucigalpa",
            null);

    private static NovedadDeRuta Novedad(
        TipoDeNovedad tipo = TipoDeNovedad.NoSePresento,
        string? autoriza = null) =>
        new(Ulid.NewUlid(), tipo, "María Elena Zúniga", "no llegó al punto de encuentro",
            "Choluteca", Salida.AddHours(1), new IdPersona("P-DESPACHO"),
            autoriza is null ? null : new IdPersona(autoriza));

    private static Manifiesto Manifiesto(
        bool cerrado = false, params NovedadDeRuta[] novedades) =>
        new(Ulid.NewUlid(), Ulid.NewUlid(), [Persona(), Persona("Juan Pérez", "0501-1985-99999")],
            cerrado ? Salida : null,
            cerrado ? new IdPersona("P-DESPACHO") : null,
            novedades);

    // ── La persona sin documento ────────────────────────────────────────────

    [Fact]
    public void Una_persona_sin_documento_se_registra_como_no_identificada()
    {
        // ⚠️ `HU-113`: exigir documento **no impide que la persona suba** — impide que figure.
        // El vehículo sale igual; lo único que cambia es si queda constancia.
        ReglasDelManifiesto.ExigirIdentificacionCoherente(
            FormaDeIdentificacion.NoIdentificada, identificacion: null);
    }

    [Theory]
    [InlineData(FormaDeIdentificacion.Documento)]
    [InlineData(FormaDeIdentificacion.Alternativa)]
    public void Declarar_una_forma_de_identificacion_sin_ponerla_se_rechaza(
        FormaDeIdentificacion forma)
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelManifiesto.ExigirIdentificacionCoherente(forma, "   "));

        // Y el mensaje ofrece la salida: registrarla como no identificada. Sin eso, quien
        // captura borra la fila y la persona viaja sin figurar.
        Assert.Contains("no identificada", e.Message);
    }

    [Fact]
    public void La_identificacion_alternativa_vale_igual_que_el_documento()
    {
        // Una constancia, un número de expediente de otra institución. Lo que importa es que
        // haya constancia de quién iba.
        ReglasDelManifiesto.ExigirIdentificacionCoherente(
            FormaDeIdentificacion.Alternativa, "expediente DINAF 2026-118");
    }

    // ── El cierre ───────────────────────────────────────────────────────────

    [Fact]
    public void Un_manifiesto_abierto_se_puede_modificar()
    {
        ReglasDelManifiesto.ExigirAbierto(Manifiesto(cerrado: false));
    }

    [Fact]
    public void Un_manifiesto_cerrado_no_se_toca_y_el_mensaje_dice_por_donde_se_sale()
    {
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelManifiesto.ExigirAbierto(Manifiesto(cerrado: true)));

        // La salida concreta: novedad de ruta. Sin decirla, quien necesita registrar el cambio
        // lo anota en observaciones — y ahí no lo compara nadie.
        Assert.Contains("novedad de ruta", e.Message);
    }

    [Fact]
    public void Una_novedad_sobre_un_manifiesto_abierto_no_tiene_sentido()
    {
        // Todavía no hay nada declarado contra qué comparar: se agrega o se quita la persona
        // directamente. Permitirlo produciría novedades que describen un cambio sobre una lista
        // que aún se estaba armando.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelManifiesto.ExigirCerrado(Manifiesto(cerrado: false)));

        Assert.Contains("todavía no hay nada declarado", e.Message);
    }

    // ── Lo declarado contra lo que pasó ─────────────────────────────────────

    [Fact]
    public void Sin_novedades_lo_declarado_y_lo_efectivo_coinciden()
    {
        var m = Manifiesto(cerrado: true);

        Assert.Equal(2, m.Declaradas);
        Assert.Equal(2, m.Efectivas);
        Assert.False(m.HayDiferencias);
    }

    [Fact]
    public void Quien_no_se_presento_baja_el_efectivo_y_no_toca_lo_declarado()
    {
        // Lo declarado es lo que se autorizó y **no cambia**. Si cambiara, el manifiesto sería
        // un resumen de lo que pasó y no habría contra qué comparar.
        var m = Manifiesto(true, Novedad(TipoDeNovedad.NoSePresento));

        Assert.Equal(2, m.Declaradas);
        Assert.Equal(1, m.Efectivas);
        Assert.True(m.HayDiferencias);
    }

    [Fact]
    public void Quien_subio_en_ruta_sube_el_efectivo()
    {
        var m = Manifiesto(true, Novedad(TipoDeNovedad.SubioEnRuta, autoriza: "P-TRANSPORTE"));

        Assert.Equal(2, m.Declaradas);
        Assert.Equal(3, m.Efectivas);
    }

    [Fact]
    public void Quien_bajo_antes_viajo_igual_y_no_cambia_el_conteo()
    {
        // Bajó antes del destino: iba a bordo. Restarlo diría que no viajó, y la diferencia que
        // hay que explicar es otra — dónde bajó, no si fue.
        var m = Manifiesto(true, Novedad(TipoDeNovedad.BajoAntes));

        Assert.Equal(2, m.Efectivas);
        Assert.True(m.HayDiferencias);
    }

    // ── Subir a alguien exige quién lo autorizó ─────────────────────────────

    [Fact]
    public void Subir_a_alguien_no_declarado_exige_quien_lo_autorizo()
    {
        // Es la novedad que más se presta: el vehículo institucional que lleva a un conocido.
        // Con autorización nombrada es la decisión de alguien; sin ella, un favor que nadie firmó.
        var e = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelManifiesto.ExigirAutorizacionSiSubio(
                TipoDeNovedad.SubioEnRuta, autoriza: null));

        Assert.Contains("quién autorizó", e.Message);
    }

    [Fact]
    public void Con_autorizacion_nombrada_pasa()
    {
        ReglasDelManifiesto.ExigirAutorizacionSiSubio(
            TipoDeNovedad.SubioEnRuta, new IdPersona("P-TRANSPORTE"));
    }

    [Theory]
    [InlineData(TipoDeNovedad.NoSePresento)]
    [InlineData(TipoDeNovedad.BajoAntes)]
    public void Las_otras_novedades_no_exigen_autorizacion(TipoDeNovedad tipo)
    {
        // Nadie autoriza que alguien no llegue. Pedirlo convertiría el registro de un hecho en
        // un trámite, y el hecho dejaría de registrarse.
        ReglasDelManifiesto.ExigirAutorizacionSiSubio(tipo, null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("no vino")]
    public void Toda_novedad_exige_motivo(string? motivo)
    {
        var e = Assert.Throws<BloqueoDuro>(() => ReglasDelManifiesto.ExigirMotivo(motivo));

        Assert.Contains("hallazgo", e.Message);
    }
}
