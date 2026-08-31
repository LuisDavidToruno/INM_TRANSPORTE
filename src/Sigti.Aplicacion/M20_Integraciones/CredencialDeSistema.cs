using System.Net.Http.Headers;
using System.Net.Http.Json;

using Microsoft.Extensions.Configuration;

namespace Sigti.Aplicacion.M20_Integraciones;

/// <summary>
/// El token con que <b>SIGTI como sistema</b> se presenta ante el servicio de identidad.
///
/// ── ⚠️ Por qué esto NO reemplaza al token de la persona ─────────────────────
/// Son dos portadores distintos y no se mezclan:
///
/// <list type="bullet">
/// <item>Cuando alguien pide <b>su propia ficha</b>, va su token: los datos son suyos.</item>
/// <item>Cuando SIGTI baja el <b>padrón completo</b> —193 personas con número de identidad y
/// correo—, va éste. Ese volumen de datos de terceros no debe abrirlo el token de cualquier
/// empleado sólo porque necesitaba pedir un vehículo.</item>
/// </list>
///
/// ── El costo que esto tiene, y hay que decirlo ──────────────────────────────
/// Una credencial de sistema <b>es la que nadie revoca</b>: se crea una vez, se copia a tres
/// ambientes y sobrevive a todo el mundo. Contra eso hay dos cosas concretas: <b>vive sólo en
/// la configuración local</b>, que no se versiona ni viaja en el paquete, y <b>se puede rotar
/// sin tocar código</b> — es una línea en dos archivos.
///
/// Lo que <b>no</b> se pierde es saber quién pidió la sincronización: eso lo registra SIGTI de
/// su lado, con el token de la persona que apretó el botón.
/// </summary>
public sealed class CredencialDeSistema(HttpClient cliente, IConfiguration configuracion)
{
    private string? _token;
    private DateTimeOffset _expira = DateTimeOffset.MinValue;

    /// <summary>
    /// Un token válido, pidiendo uno nuevo sólo si hace falta.
    ///
    /// ── El margen de un minuto no es paranoia ───────────────────────────────
    /// Un token que vence <i>durante</i> la llamada produce un `401` a mitad de una
    /// sincronización de 193 filas, y el mensaje habla de autorización cuando el problema es de
    /// reloj. Se renueva un minuto antes y ese caso deja de existir.
    /// </summary>
    public async Task<AuthenticationHeaderValue> TokenAsync(CancellationToken cancelacion = default)
    {
        if (_token is not null && _expira > DateTimeOffset.UtcNow.AddMinutes(1))
            return new AuthenticationHeaderValue("Bearer", _token);

        var identidad = configuracion["Argos:Cliente"];
        var secreto = configuracion["Argos:Secreto"];

        if (string.IsNullOrWhiteSpace(identidad) || string.IsNullOrWhiteSpace(secreto))
        {
            throw new EspejoNoDisponible(
                "Falta la credencial de sistema («Argos:Cliente» y «Argos:Secreto»), que va en " +
                "la configuración local y no se versiona. Sin ella SIGTI no puede bajar el " +
                "padrón: el servicio de identidad sólo lo abre a un sistema declarado.");
        }

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/auth/sistema",
            new { cliente = identidad, secreto },
            cancelacion);

        if (!respuesta.IsSuccessStatusCode)
        {
            // ⚠️ **El secreto no aparece acá ni en ningún mensaje.** Un secreto en un texto de
            // error termina en un log, y un log termina en cualquier parte.
            throw new EspejoNoDisponible(
                $"El servicio de identidad rechazó la credencial de sistema de SIGTI " +
                $"({(int)respuesta.StatusCode}). Verifique que el consumidor «{identidad}» esté " +
                "declarado allá con el mismo secreto que tiene SIGTI acá.");
        }

        var datos = await respuesta.Content.ReadFromJsonAsync<TokenDeSistema>(cancelacion)
            ?? throw new EspejoNoDisponible(
                "El servicio de identidad respondió sin token a una credencial que aceptó.");

        _token = datos.Token;
        _expira = datos.Expira;

        return new AuthenticationHeaderValue("Bearer", _token);
    }

    private sealed record TokenDeSistema(
        string Token, DateTimeOffset Expira, string Cliente, string Descripcion);
}
