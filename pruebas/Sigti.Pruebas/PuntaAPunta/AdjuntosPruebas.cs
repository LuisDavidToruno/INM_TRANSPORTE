using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sigti.Aplicacion.M16_Sincronizacion;
using Sigti.Datos;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.PuntaAPunta;

/// <summary>
/// La subida de adjuntos — `ADR-004`.
///
/// ── La decisión que estas pruebas ejercen ────────────────────────────────────
/// <b>El archivo vive en el sistema de archivos; la base guarda ruta, hash, tipo,
/// tamaño y clasificación.</b> La aritmética que lo decidió: ≈ 8 GB anuales de datos
/// relacionales contra ≈ 30 GB de adjuntos. Meter los binarios en la base cuadruplica
/// el respaldo y saca la restauración de las 2 h que `RNF-09` exige de personal no
/// especialista.
///
/// ── Por qué el hash se verifica al recibir, y no solo se guarda ──────────────
/// `ADR-004`: el hash «permite detectar que un adjunto **fue sustituido o se
/// corrompió**». Guardarlo sin comprobarlo lo volvería decorativo — un archivo que
/// llegó truncado por una red de retén quedaría registrado como íntegro, y el defecto
/// aparecería meses después, cuando alguien arme el paquete de evidencia.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class AdjuntosPruebas(BaseDePruebas baseDePruebas) : IDisposable
{
    private readonly string _almacen = Path.Combine(Path.GetTempPath(), $"sigti-adj-{Ulid.NewUlid()}");

    // Igual que las demas, mas la raiz del almacen de archivos que esta prueba necesita.
    private WebApplicationFactory<Program> Aplicacion() =>
        FabricaDeSigti.Crear(baseDePruebas, constructor =>
            constructor.UseSetting("Adjuntos:Raiz", _almacen));

    [Fact]
    public async Task El_binario_va_al_sistema_de_archivos_y_la_base_guarda_su_rastro()
    {
        var idAdjunto = Ulid.NewUlid();
        var contenido = Encoding.UTF8.GetBytes("una fotografía del odómetro");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await Subir(cliente, idAdjunto, contenido, HashDe(contenido));

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);

        await using var contexto = baseDePruebas.Contexto();
        var fila = await contexto.Set<FilaDeAdjunto>().SingleAsync(a => a.Id == idAdjunto);

        // Lo que la base guarda: el RASTRO, no el binario.
        Assert.Equal(contenido.Length, fila.Bytes);
        Assert.Equal(HashDe(contenido), fila.Hash);
        Assert.False(string.IsNullOrWhiteSpace(fila.Ruta));

        // Y el archivo está donde la ruta dice, con su contenido intacto.
        var enDisco = await File.ReadAllBytesAsync(Path.Combine(_almacen, fila.Ruta));
        Assert.Equal(contenido, enDisco);
    }

    [Fact]
    public async Task Un_adjunto_que_llego_corrupto_se_rechaza_en_vez_de_registrarse_como_intacto()
    {
        // Es el caso real: la subida se corta por la red de un retén y llega truncada.
        // Sin esta comprobación el archivo quedaría registrado como íntegro y el defecto
        // aparecería meses después, al armar el paquete de evidencia — cuando ya no hay
        // forma de volver a tomar la foto.
        var idAdjunto = Ulid.NewUlid();
        var contenido = Encoding.UTF8.GetBytes("una fotografía truncada");
        var hashDeLoQueEraAntes = HashDe(Encoding.UTF8.GetBytes("la fotografía completa"));

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var respuesta = await Subir(cliente, idAdjunto, contenido, hashDeLoQueEraAntes);

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
        Assert.Contains("hash", (await respuesta.Content.ReadAsStringAsync()).ToLowerInvariant());

        await using var contexto = baseDePruebas.Contexto();
        Assert.False(await contexto.Set<FilaDeAdjunto>().AnyAsync(a => a.Id == idAdjunto));
    }

    [Fact]
    public async Task Reenviar_el_mismo_adjunto_no_lo_duplica()
    {
        // Igual que con las transiciones: el dispositivo que no supo si el servidor
        // recibió VA a reenviar, y con 200 fotografías por dispositivo eso ocurre a
        // menudo. La identidad la da el ULID que generó el dispositivo (`ADR-005`).
        var idAdjunto = Ulid.NewUlid();
        var contenido = Encoding.UTF8.GetBytes("una fotografía del comprobante");

        using var aplicacion = Aplicacion();
        using var cliente = aplicacion.CreateClient();

        var primera = await Subir(cliente, idAdjunto, contenido, HashDe(contenido));
        var segunda = await Subir(cliente, idAdjunto, contenido, HashDe(contenido));

        Assert.Equal(HttpStatusCode.Created, primera.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segunda.StatusCode);

        await using var contexto = baseDePruebas.Contexto();
        Assert.Equal(1, await contexto.Set<FilaDeAdjunto>().CountAsync(a => a.Id == idAdjunto));
    }

    private static async Task<HttpResponseMessage> Subir(
        HttpClient cliente, Ulid idAdjunto, byte[] contenido, string hash)
    {
        using var formulario = new MultipartFormDataContent();

        var archivo = new ByteArrayContent(contenido);
        archivo.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        formulario.Add(archivo, "archivo", $"{idAdjunto}.jpg");

        formulario.Add(new StringContent(idAdjunto.ToString()), "idAdjunto");
        formulario.Add(new StringContent(Ulid.NewUlid().ToString()), "idTransicion");
        formulario.Add(new StringContent(hash), "hash");
        formulario.Add(new StringContent("OPERATIVO"), "clasificacion");
        formulario.Add(new StringContent("2026-03-20T06:41:00-06:00"), "capturadoEn");

        return await cliente.PostAsync("/adjuntos", formulario);
    }

    /// <summary>SHA-256 en minúsculas, con prefijo. El algoritmo va en el dato: dentro de
    /// diez años alguien va a querer saber con qué se calculó.</summary>
    private static string HashDe(byte[] contenido) =>
        "sha256:" + Convert.ToHexStringLower(SHA256.HashData(contenido));

    public void Dispose()
    {
        if (Directory.Exists(_almacen)) Directory.Delete(_almacen, recursive: true);
    }
}
