using System.Net.Http.Headers;

namespace Sigti.Api.Seguridad;

/// <summary>
/// Pasa el token de quien pidió la operación al servicio del que SIGTI depende.
///
/// ── ⚠️ Por qué no una credencial de servicio ────────────────────────────────
/// La alternativa sería que SIGTI tuviera su propia cuenta y la usara para todo. Eso tiene dos
/// costos que no se ven hasta que es tarde:
///
/// <list type="bullet">
/// <item>Del otro lado <b>todas las lecturas se ven iguales</b>: una cuenta de sistema
/// consultando el padrón, sin decir a nombre de quién. Si algún día hay que saber quién trajo
/// esos datos, no se puede.</item>
/// <item>Una credencial de servicio compartida <b>es la que nadie revoca</b>. Se crea una vez,
/// se pone en la configuración de tres ambientes, y sobrevive a todo el mundo.</item>
/// </list>
///
/// Reenviando el token, la lectura se hace en nombre de una persona concreta, con su vigencia y
/// su baja. El día que esa persona deja la institución, deja de poder hacerlo — sin que nadie
/// tenga que acordarse.
/// </summary>
public sealed class ReenviarElToken(IHttpContextAccessor acceso) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage peticion, CancellationToken cancelacion)
    {
        var entrante = acceso.HttpContext?.Request.Headers.Authorization.ToString();

        // Sin token entrante no se inventa uno. La llamada saldrá anónima y el otro servicio la
        // rechazará — que es lo correcto: significa que algo llamó a esto fuera del contexto de
        // una petición autenticada, y eso hay que verlo, no taparlo.
        if (!string.IsNullOrWhiteSpace(entrante)
            && AuthenticationHeaderValue.TryParse(entrante, out var cabecera))
        {
            peticion.Headers.Authorization = cabecera;
        }

        return base.SendAsync(peticion, cancelacion);
    }
}
