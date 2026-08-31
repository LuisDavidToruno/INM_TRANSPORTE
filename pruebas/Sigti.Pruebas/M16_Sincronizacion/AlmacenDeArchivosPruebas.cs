using Sigti.Aplicacion.M16_Sincronizacion;

namespace Sigti.Pruebas.M16_Sincronizacion;

/// <summary>
/// El almacén de adjuntos — `ADR-004`: el binario al sistema de archivos, a la base sólo su
/// rastro.
///
/// ── ⚠️ Lo que estas pruebas vinieron a cubrir ───────────────────────────────
/// El almacén tenía <c>GuardarAsync</c> y nada más. <b>Todo lo que el sistema exige adjuntar
/// entraba y no salía nunca</b>: el respaldo documental del parámetro normativo —que `HU-145`
/// manda poder abrir <b>antes</b> de aprobar—, la fotografía obligatoria de la constatación de
/// rotulación, el documento de respaldo de placa que el agente pide en carretera.
/// </summary>
public class AlmacenDeArchivosPruebas : IDisposable
{
    private readonly string _raiz = Path.Combine(
        Path.GetTempPath(), "sigti-almacen-" + Ulid.NewUlid());

    private static readonly DateTimeOffset Momento =
        new(2026, 9, 4, 8, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public async Task Lo_guardado_se_puede_leer()
    {
        var almacen = new AlmacenDeArchivos(_raiz);
        var id = Ulid.NewUlid();
        var contenido = "Permiso provisional de circulación"u8.ToArray();

        var ruta = await almacen.GuardarAsync(id, "application/pdf", Momento, contenido);

        Assert.Equal(contenido, await almacen.LeerAsync(ruta));
    }

    /// <summary>
    /// <b>Nulo cuando el archivo no está</b>, no excepción.
    ///
    /// La fila puede estar y el archivo no: es lo que `ADR-004` avisó al separar el binario de
    /// la base. Un almacén movido, restaurado a medias o montado en la ruta equivocada produce
    /// exactamente eso, y quien llama tiene que poder decirlo <b>con la ruta</b> en vez de
    /// recibir un fallo genérico.
    /// </summary>
    [Fact]
    public async Task Una_ruta_sin_archivo_devuelve_nulo()
    {
        var almacen = new AlmacenDeArchivos(_raiz);

        Assert.Null(await almacen.LeerAsync(Path.Combine("2026", "09", "no-esta.pdf")));
    }

    /// <summary>
    /// ⚠️ <b>Una ruta que sale de la raíz no se sirve.</b>
    ///
    /// La ruta viene de la base, y eso no la hace confiable: una fila con <c>..\..\</c> serviría
    /// cualquier archivo del servidor — la cadena de conexión, una clave, el registro de
    /// Windows. <b>El almacén no decide en quién confiar, y por eso no confía.</b>
    /// </summary>
    [Theory]
    [InlineData(@"..\..\appsettings.json")]
    [InlineData("../../appsettings.json")]
    [InlineData(@"2026\..\..\..\secreto.txt")]
    public async Task Una_ruta_que_se_escapa_de_la_raiz_no_se_sirve(string ruta)
    {
        var almacen = new AlmacenDeArchivos(_raiz);

        // Un archivo real justo afuera: si el guardia no operara, esto lo devolvería.
        var afuera = Path.Combine(Path.GetDirectoryName(_raiz)!, "appsettings.json");
        await File.WriteAllTextAsync(afuera, "no se debe servir");

        try
        {
            Assert.Null(await almacen.LeerAsync(ruta));
        }
        finally
        {
            File.Delete(afuera);
        }
    }

    /// <summary>
    /// La raíz misma tampoco: <c>""</c> resuelve a la carpeta, y devolverla sería un error
    /// distinto y más ruidoso, pero error al fin.
    /// </summary>
    [Fact]
    public async Task La_raiz_misma_no_se_sirve()
    {
        var almacen = new AlmacenDeArchivos(_raiz);

        Assert.Null(await almacen.LeerAsync(""));
    }

    /// <summary>
    /// El nombre sale del <b>identificador</b>, no del que traía el dispositivo. Es lo que
    /// impide una colisión, un carácter del sistema de archivos ajeno, o un nombre que revele
    /// algo que no corresponde.
    /// </summary>
    [Fact]
    public async Task El_archivo_se_nombra_por_su_identificador()
    {
        var almacen = new AlmacenDeArchivos(_raiz);
        var id = Ulid.NewUlid();

        var ruta = await almacen.GuardarAsync(id, "image/jpeg", Momento, [1, 2, 3]);

        Assert.Contains(id.ToString(), ruta);

        // Y se organiza por la fecha del HECHO, no la de subida (`P-4`).
        Assert.Contains(Path.Combine("2026", "09"), ruta);
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
        GC.SuppressFinalize(this);
    }
}
