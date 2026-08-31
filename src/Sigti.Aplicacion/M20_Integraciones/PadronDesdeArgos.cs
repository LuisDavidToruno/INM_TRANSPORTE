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
public sealed class PadronDesdeArgos(HttpClient cliente) : IEspejoDeOrganizacion
{
    public string Fuente => "ARGOS · servicio de identidad institucional";

    /// <summary>
    /// ── ⚠️ Reenvía el token de quien pidió la sincronización ────────────────
    /// No usa una credencial de servicio propia. Que la lectura del padrón se haga <b>en nombre
    /// de una persona concreta</b> es lo que permite que quede en la bitácora quién trajo esos
    /// datos y cuándo — y una credencial de servicio compartida es exactamente la que nadie
    /// revoca cuando alguien se va.
    /// </summary>
    public async Task<IReadOnlyList<PersonaDelPadron>> PadronAsync(
        CancellationToken cancelacion = default)
    {
        var respuesta = await cliente.GetAsync("/api/v1/organizacion/empleados", cancelacion);

        if (!respuesta.IsSuccessStatusCode)
        {
            throw new EspejoNoDisponible(
                $"El servicio de identidad respondió {(int)respuesta.StatusCode}. El espejo no " +
                "se actualiza a medias: se deja como estaba y se dice desde cuándo.");
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
