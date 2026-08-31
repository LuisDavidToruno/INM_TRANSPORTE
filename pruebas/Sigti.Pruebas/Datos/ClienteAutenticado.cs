using System.Net.Http.Json;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// Peticiones que declaran <b>quién las hace</b>, por cabecera y no por cuerpo.
///
/// ── Por qué la identidad va por petición y no por cliente ───────────────────
/// Porque casi toda prueba de este sistema es una conversación entre <b>personas distintas</b>:
/// quien captura no es quien autoriza, quien despacha no es quien liquida, y quien liquida no
/// es quien cierra. Eso es `RN-01`, y es la mitad de lo que estas pruebas verifican.
///
/// Un cliente con identidad fija obligaría a construir uno por actor, y la prueba dejaría de
/// leerse como lo que es: una secuencia de actos de gente distinta sobre el mismo expediente.
/// </summary>
public static class ClienteAutenticado
{
    public static Task<HttpResponseMessage> PostComoAsync<T>(
        this HttpClient cliente, string persona, string ruta, T cuerpo,
        params string[] roles)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Post, ruta)
        {
            Content = JsonContent.Create(cuerpo),
        };

        return cliente.SendAsync(Identificar(peticion, persona, roles));
    }

    public static Task<HttpResponseMessage> GetComoAsync(
        this HttpClient cliente, string persona, string ruta, params string[] roles) =>
        cliente.SendAsync(Identificar(new HttpRequestMessage(HttpMethod.Get, ruta), persona, roles));

    /// <summary>
    /// Una petición <b>sin identidad</b>. Existe con nombre propio para que la prueba que
    /// verifica que el sistema la rechaza se lea como lo que prueba, y no como un olvido.
    /// </summary>
    public static Task<HttpResponseMessage> PostAnonimoAsync<T>(
        this HttpClient cliente, string ruta, T cuerpo) =>
        cliente.PostAsJsonAsync(ruta, cuerpo);

    private static HttpRequestMessage Identificar(
        HttpRequestMessage peticion, string persona, string[] roles)
    {
        peticion.Headers.Add(AutenticacionDePrueba.CabeceraDePersona, persona);

        if (roles.Length > 0)
            peticion.Headers.Add(AutenticacionDePrueba.CabeceraDeRoles, string.Join(",", roles));

        return peticion;
    }
}
