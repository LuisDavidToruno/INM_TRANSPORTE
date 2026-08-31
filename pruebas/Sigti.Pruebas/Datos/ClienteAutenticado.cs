using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// Peticiones que llevan <b>quién las hace</b> donde el sistema lo espera: en la identidad de la
/// petición, no en el cuerpo.
///
/// ── ⚠️ Por qué el actor sigue escribiéndose en el objeto ────────────────────
/// Porque casi toda prueba de este sistema es una conversación entre <b>personas distintas</b>
/// —quien captura no es quien autoriza, quien despacha no es quien liquida, y quien liquida no
/// es quien cierra: eso es `RN-01`, y es la mitad de lo que estas pruebas verifican—. Escribir
/// <c>Ejecuta = "P-JEFATURA"</c> junto al resto del acto deja la prueba legible como la
/// secuencia de actos que es.
///
/// Lo que cambió es <b>por dónde viaja</b>: <see cref="PostComoAsync"/> lo saca del cuerpo y lo
/// pone en la identidad de la petición. El API ya no lo aceptaría del cuerpo — el campo no
/// existe en ningún record.
///
/// Es un traductor del andamiaje, no una puerta trasera: la identidad la sigue resolviendo el
/// servidor de los claims, exactamente igual que con un token firmado.
/// </summary>
public static class ClienteAutenticado
{
    /// <summary>
    /// Quién ejecuta cuando la prueba no lo declara.
    ///
    /// Existe para las peticiones donde <b>el actor no es lo que se está probando</b> — una
    /// consulta de catálogo, un GET de verificación. Que tenga nombre propio y no sea una cadena
    /// suelta es a propósito: si aparece en el mensaje de un bloqueo de segregación, se sabe de
    /// inmediato que la prueba se olvidó de declarar el actor.
    /// </summary>
    public const string SinDeclarar = "P-PRUEBA-SIN-DECLARAR";

    /// <summary>
    /// Un cliente con identidad de base. <b>Toda petición sale autenticada</b>, porque el API
    /// ya no atiende a nadie sin identidad.
    /// </summary>
    public static HttpClient CrearCliente(this WebApplicationFactory<Program> aplicacion)
    {
        var cliente = aplicacion.CreateClient();
        cliente.DefaultRequestHeaders.Add(AutenticacionDePrueba.CabeceraDePersona, SinDeclarar);
        return cliente;
    }

    /// <summary>
    /// Publica el acto <b>como</b> quien el cuerpo declare en <c>Ejecuta</c>.
    ///
    /// Si el cuerpo no lo declara, va con <see cref="SinDeclarar"/>: la petición se autentica
    /// igual, pero con una identidad que se reconoce a simple vista en cualquier mensaje de
    /// bloqueo.
    /// </summary>
    public static Task<HttpResponseMessage> PostComoAsync<T>(
        this HttpClient cliente, string ruta, T cuerpo)
    {
        var json = JsonSerializer.SerializeToNode(cuerpo) as JsonObject;

        // El nombre viaja en minúscula: `SerializeToNode` respeta el nombre de la propiedad
        // del objeto anónimo, y los objetos de prueba lo escriben `Ejecuta`.
        var actor = Sacar(json, "Ejecuta") ?? Sacar(json, "ejecuta");

        var peticion = new HttpRequestMessage(HttpMethod.Post, ruta)
        {
            Content = new StringContent(
                json?.ToJsonString() ?? "{}", Encoding.UTF8, "application/json"),
        };

        peticion.Headers.Add(AutenticacionDePrueba.CabeceraDePersona, actor ?? SinDeclarar);

        return cliente.SendAsync(peticion);
    }

    /// <summary>
    /// Una petición <b>sin identidad alguna</b>. Tiene nombre propio para que la prueba que
    /// verifica que el sistema la rechaza se lea como lo que prueba, y no como un olvido.
    /// </summary>
    public static Task<HttpResponseMessage> PostAnonimoAsync<T>(
        this HttpClient cliente, string ruta, T cuerpo)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Post, ruta)
        {
            Content = JsonContent.Create(cuerpo),
        };

        // El cliente lleva identidad de base; esta petición la quita explícitamente.
        peticion.Headers.Remove(AutenticacionDePrueba.CabeceraDePersona);

        return cliente.SendAsync(peticion);
    }

    private static string? Sacar(JsonObject? json, string propiedad)
    {
        if (json is null || !json.TryGetPropertyValue(propiedad, out var valor)) return null;

        json.Remove(propiedad);
        return valor?.GetValue<string>();
    }
}
