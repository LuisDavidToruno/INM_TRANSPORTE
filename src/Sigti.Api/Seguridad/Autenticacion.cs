using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Sigti.Api.Seguridad;

/// <summary>
/// SIGTI <b>no tiene padrón de contraseñas propio</b>: valida el token que emite el servicio de
/// identidad institucional.
///
/// ── Por qué no se replican los usuarios ─────────────────────────────────────
/// Porque la alternativa es que cada sistema de la institución tenga el suyo, y entonces dar de
/// baja a alguien exige acordarse de todos. `NRM-09` `[V]`: la rotación en el sector público es
/// alta y Honduras cambió de gobierno en enero de 2026 — el día que alguien deja de trabajar
/// ahí, la baja tiene que cerrar todas las puertas a la vez.
///
/// ── Y por qué el producto sigue siendo genérico ─────────────────────────────
/// Lo que SIGTI exige es un <b>token firmado por un emisor declarado</b>, no ARGOS. En el piloto
/// del INM ese emisor es `ARGOS_API`; en otra institución será otro, y lo único que cambia es la
/// configuración. Ninguna regla de negocio conoce a ARGOS.
/// </summary>
public static class Autenticacion
{
    /// <summary>
    /// ⚠️ <b>Limitación conocida: la clave es simétrica.</b>
    ///
    /// El emisor firma con HMAC-SHA256 y el consumidor valida con <b>la misma clave</b>. Eso
    /// significa que cualquier consumidor que la tenga <b>puede emitir tokens en nombre de
    /// cualquiera</b>: validar y firmar son la misma capacidad.
    ///
    /// Con un solo consumidor —hoy, SIGTI— el riesgo es acotado: quien comprometa SIGTI ya tiene
    /// la base de SIGTI. <b>Deja de serlo con el segundo consumidor</b>, porque entonces
    /// comprometer al más débil permite falsificar identidad frente a todos.
    ///
    /// La salida es RS256: el emisor firma con la clave privada, los consumidores validan con la
    /// pública, y nadie más que el emisor puede firmar. Se deja anotado para no descubrirlo el
    /// día que entre el tercer sistema.
    /// </summary>
    public static IServiceCollection AgregarAutenticacionInstitucional(
        this IServiceCollection servicios, IConfiguration configuracion)
    {
        // Hace falta para resolver quién ejecuta desde los claims de la petición en curso.
        servicios.AddHttpContextAccessor();
        servicios.AddScoped<IdentidadDelLlamador>();

        // ⚠️ **Falla al arrancar, no en la primera petición.** Sin esto, una instancia mal
        // configurada levanta, responde `/salud`, y recién rechaza cuando alguien intenta
        // trabajar — con un error que habla de criptografía y no de configuración. Quien la
        // desplegó ya se fue.
        //
        // No hay clave por omisión, y es deliberado: una clave por omisión es una clave
        // pública, y con ella cualquiera firma tokens a nombre de quien quiera.
        var clave = configuracion["Jwt:Clave"];

        if (string.IsNullOrWhiteSpace(clave))
        {
            throw new InvalidOperationException(
                "Falta «Jwt:Clave»: es la clave con que se valida el token del servicio de " +
                "identidad institucional, y va en la configuración local, que no se versiona. " +
                "Sin ella SIGTI no puede saber quién ejecuta nada.");
        }

        servicios
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opciones =>
            {
                opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuracion["Jwt:Emisor"] ?? "argos-api",
                    ValidateAudience = true,
                    ValidAudience = configuracion["Jwt:Audiencia"] ?? "sistemas-inm",
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),

                    // Sin holgura. Los cinco minutos que vienen por omisión hacen que un token
                    // vencido siga sirviendo, que es exactamente lo que la expiración evita.
                    ClockSkew = TimeSpan.Zero,
                };
            });

        servicios.AddAuthorization();

        return servicios;
    }
}
