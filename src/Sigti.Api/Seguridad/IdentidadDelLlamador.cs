using System.Security.Claims;

using Sigti.Dominio.Organizacion;

namespace Sigti.Api.Seguridad;

/// <summary>
/// Quién está ejecutando esta petición, resuelto <b>del token y de ningún otro lado</b>.
///
/// ── ⚠️ Por qué esto existe ──────────────────────────────────────────────────
/// Hasta acá el actor viajaba <b>en el cuerpo de la petición</b>: cada endpoint recibía un
/// campo <c>Ejecuta</c> y le creía. Eso convertía en decorado todo el aparato de control del
/// sistema, porque quien llama declaraba quién es:
///
/// <list type="bullet">
/// <item>`BD-06` comparaba a quien liquidó contra quien cierra — <b>dos cadenas que mandaba la
/// misma persona</b>.</item>
/// <item>`RN-23` reservaba la firma del permiso a la máxima autoridad, y bastaba escribir su
/// identificador en un JSON para firmar veinte permisos.</item>
/// <item>La bitácora encadenada e inmutable registraba, con hash y todo, <b>lo que el llamador
/// dijo ser</b>. Un rastro perfectamente íntegro de una ficción.</item>
/// </list>
///
/// La segregación de funciones del MARCI es el corazón de este sistema. Declarada por el propio
/// llamador, no es un control: es un comentario.
///
/// ── Y por qué no se acepta un respaldo ──────────────────────────────────────
/// No hay «si no viene en el token, tómalo del cuerpo». Un respaldo así deja el agujero abierto
/// exactamente igual, y encima escondido detrás de algo que parece resuelto — que es peor,
/// porque nadie lo vuelve a mirar.
/// </summary>
public sealed class IdentidadDelLlamador(IHttpContextAccessor acceso)
{
    /// <summary>
    /// El claim que lleva el identificador de la persona en el padrón institucional.
    ///
    /// No es el nombre de usuario ni el correo: es la clave con que `RN-100` resuelve el puesto
    /// a la fecha del hecho. En el piloto del INM lo emite ARGOS y corresponde al empleado.
    /// </summary>
    public const string ClaimDePersona = "sigti:persona";

    /// <summary>
    /// Quién ejecuta. <b>Lanza si no hay token o no trae el claim</b> — nunca devuelve un valor
    /// por omisión, porque un actor por omisión es un actor suplantado.
    /// </summary>
    public IdPersona Persona =>
        Resuelta ?? throw new SinIdentidad(
            "La petición no trae identidad. Todo acto de este sistema se atribuye a una persona " +
            "concreta, y esa persona sale del token — no del cuerpo de la petición.");

    /// <summary>
    /// Quién ejecuta, o nulo. Para los pocos lugares que legítimamente atienden sin identidad
    /// —la verificación pública del salvoconducto— y necesitan <b>saber que no la hay</b>.
    /// </summary>
    public IdPersona? Resuelta
    {
        get
        {
            var usuario = acceso.HttpContext?.User;

            if (usuario?.Identity?.IsAuthenticated != true) return null;

            var valor = usuario.FindFirstValue(ClaimDePersona);

            return string.IsNullOrWhiteSpace(valor) ? null : new IdPersona(valor);
        }
    }

    /// <summary>
    /// Desde dónde llega. Va a la bitácora de intentos bloqueados: un rechazo de segregación sin
    /// origen no se puede seguir.
    /// </summary>
    public string? Origen => acceso.HttpContext?.Connection.RemoteIpAddress?.ToString();
}

/// <summary>
/// No hay identidad en la petición. <b>Es 401, no 400</b>: no es que el cuerpo esté mal formado,
/// es que no se sabe quién está pidiendo.
/// </summary>
public sealed class SinIdentidad(string mensaje) : Exception(mensaje);
