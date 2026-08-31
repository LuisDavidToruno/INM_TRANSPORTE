using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Sigti.Aplicacion.M20_Integraciones;

/// <summary>
/// El padrón leído del servicio de identidad institucional — <b>la implementación del piloto
/// del INM</b>.
///
/// Es la única clase de SIGTI que sabe que ARGOS existe. Todo lo demás habla con
/// <see cref="IEspejoDeOrganizacion"/>.
/// </summary>
public sealed class PadronDesdeArgos(HttpClient cliente, CredencialDeSistema credencial)
    : IEspejoDeOrganizacion
{
    public string Fuente => "ARGOS · servicio de identidad institucional";

    /// <summary>
    /// ── ⚠️ Se presenta como SISTEMA, no con el token de quien pidió ─────────
    /// Y es una corrección: al principio reenviaba el token de la persona, razonando que así
    /// quedaba registrado a nombre de quién se leyó el padrón.
    ///
    /// El argumento del otro lado es más fuerte: el padrón completo son <b>193 personas con su
    /// número de identidad y su correo</b>, y eso no lo debe abrir el token de cualquier
    /// empleado sólo porque necesitaba pedir un vehículo. Un endpoint que devuelve datos de
    /// terceros exige credencial de sistema; uno que devuelve los datos de quien pregunta, no.
    ///
    /// Lo que se quería conservar no se pierde: <b>quién pidió la sincronización lo registra
    /// SIGTI de su lado</b>, con el token de la persona que apretó el botón.
    /// </summary>
    public async Task<IReadOnlyList<PersonaDelPadron>> PadronAsync(
        CancellationToken cancelacion = default)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/v1/organizacion/empleados")
        {
            Headers = { Authorization = await credencial.TokenAsync(cancelacion) },
        };

        var respuesta = await cliente.SendAsync(peticion, cancelacion);

        if (!respuesta.IsSuccessStatusCode)
        {
            // El `403` tiene su propio mensaje: no es que el servicio esté caído, es que la
            // credencial de sistema no sirve — y el arreglo es otro.
            throw new EspejoNoDisponible(
                respuesta.StatusCode == System.Net.HttpStatusCode.Forbidden
                    ? "El servicio de identidad no abre el padrón a esta credencial de sistema. " +
                      "Verifique que el consumidor de SIGTI esté declarado allá con el mismo " +
                      "secreto."
                    : $"El servicio de identidad respondió {(int)respuesta.StatusCode}. El " +
                      "espejo no se actualiza a medias: se deja como estaba y se dice desde " +
                      "cuándo.");
        }

        var filas = await respuesta.Content.ReadFromJsonAsync<List<FilaDeArgos>>(cancelacion)
            ?? [];

        return
        [
            .. filas.Select(f => new PersonaDelPadron(
                f.Persona, f.Nombre, f.Puesto, f.PuestoDesde, f.PuestoHasta,
                f.Gerencia, f.Unidad, f.Oficina)),
        ];
    }

    /// <summary>
    /// La forma que entrega ARGOS. <b>Vive acá y no en el dominio</b>: el día que cambie, el
    /// cambio se para en esta clase.
    /// </summary>
    private sealed record FilaDeArgos(
        string Persona, string NumeroIdentidad, string Nombre, string? Correo, bool Habilitado,
        string? Puesto, DateOnly? PuestoDesde, DateOnly? PuestoHasta,
        string? Gerencia, string? Unidad, string? Oficina);
}

/// <summary>
/// El espejo no se pudo leer. <b>Es distinto de «el espejo está vacío»</b>, y por eso tiene su
/// propia excepción: un espejo vacío haría que nadie tuviera competencias y todo se bloqueara
/// sin decir por qué.
/// </summary>
public sealed class EspejoNoDisponible(string mensaje) : Exception(mensaje);
