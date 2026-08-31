using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// La identidad de una petición <b>en las pruebas</b>: sale de una cabecera en vez de un token
/// firmado.
///
/// ── ⚠️ Por qué esto vive en el proyecto de pruebas y no en el API ───────────
/// Porque es una <b>puerta trasera</b>: quien mande la cabecera es quien diga ser. Eso es
/// exactamente lo que estamos sacando del sistema, y ponerlo detrás de una condición de
/// ambiente —<c>if (esDesarrollo)</c>— lo dejaría a un archivo de configuración de distancia de
/// producción.
///
/// Acá no puede pasar: este ensamblado no se despliega. El API no conoce este esquema y no
/// compila con él.
///
/// ── Qué sigue probando de verdad ────────────────────────────────────────────
/// Todo menos la firma del token. Los endpoints resuelven quién ejecuta <b>del mismo lugar</b>
/// —los claims de la petición— y la segregación, la autoría y la bitácora se ejercitan igual.
/// Lo que la firma agrega es que nadie pueda inventarse los claims, y eso se prueba aparte.
/// </summary>
public sealed class AutenticacionDePrueba(
    IOptionsMonitor<AuthenticationSchemeOptions> opciones,
    ILoggerFactory registro,
    UrlEncoder codificador)
    : AuthenticationHandler<AuthenticationSchemeOptions>(opciones, registro, codificador)
{
    public const string Esquema = "PruebasDeSigti";

    /// <summary>Quién dice ser esta petición. La pone <see cref="ClienteAutenticado"/>.</summary>
    public const string CabeceraDePersona = "X-Prueba-Persona";

    /// <summary>Sus roles, separados por coma. Ausente es «ninguno».</summary>
    public const string CabeceraDeRoles = "X-Prueba-Roles";

    /// <summary>
    /// El mismo claim que emite el servicio de identidad institucional. <b>Va acoplado a
    /// propósito</b>: si alguien lo renombra allá y no acá, estas pruebas fallan — que es
    /// justamente lo que uno quiere que pase.
    /// </summary>
    private const string ClaimDePersona = "sigti:persona";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(CabeceraDePersona, out var persona)
            || string.IsNullOrWhiteSpace(persona))
        {
            // Sin cabecera, la petición es anónima. **No se inventa una identidad por
            // omisión**: una identidad por omisión es una identidad suplantada, y haría que
            // las pruebas de «sin identidad no se puede» pasaran sin probar nada.
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimDePersona, persona!),
            new(ClaimTypes.NameIdentifier, persona!),
        };

        if (Request.Headers.TryGetValue(CabeceraDeRoles, out var roles))
        {
            claims.AddRange(roles.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(r => new Claim(ClaimTypes.Role, r)));
        }

        var identidad = new ClaimsIdentity(claims, Esquema);

        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identidad), Esquema)));
    }
}
