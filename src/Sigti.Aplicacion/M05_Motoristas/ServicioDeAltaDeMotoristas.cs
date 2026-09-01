using Sigti.Aplicacion.M01_Organizacion;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;
using Microsoft.EntityFrameworkCore;
using Sigti.Datos;

using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Aplicacion.M05_Motoristas;

/// <summary>Lo que contesta un alta de motorista.</summary>
public sealed record ResultadoDelRegistroDeMotorista(
    bool Procede,
    Ulid? Id,
    string Mensaje,
    IReadOnlyList<ObservacionDelMotorista> Observaciones,

    /// <summary>El rechazo fue por el puesto de quien pidió, no por el dato — <c>403</c>.</summary>
    bool EsFaltaDeCompetencia = false);

/// <summary>
/// Incorpora un motorista al padrón — <b>`M-05`</b>, acción 24 de la matriz de permisos.
/// </summary>
public sealed class ServicioDeAltaDeMotoristas(SigtiDbContext contexto)
{
    /// <summary>
    /// Quién <b>ejecuta</b> la acción 24 —«habilitar o inhabilitar motorista en el padrón»—
    /// según §4 de <c>actores-y-roles.md</c>: <b>sólo `ACT-04`</b>.
    ///
    /// ⚠️ <b>No es la misma lista que el alta de vehículo, y eso es deliberado.</b> `ACT-10`
    /// Encargado de Delegación ejecuta la acción 23 y en la 24 figura como <c>P</c>: propone,
    /// sin consumar el acto. La delegación incorpora el vehículo que le asignaron; no mete
    /// gente al padrón de quien conduce.
    /// </summary>
    private static readonly Rol[] Ejecutan = [Rol.JefeDeTransporte];

    public async Task<ResultadoDelRegistroDeMotorista> RegistrarAsync(
        AltaDeMotorista alta,
        DateOnly hoy,
        IdPersona quien,
        CancellationToken cancelacion = default)
    {
        var suyas = await new ServicioDeCompetencias(contexto)
            .DeLaPersonaAsync(quien, hoy, cancelacion);

        if (!suyas.Roles.Any(Ejecutan.Contains))
        {
            return new ResultadoDelRegistroDeMotorista(
                false, null,
                "Habilitar un motorista en el padrón es la acción 24 de la matriz de permisos y " +
                "la ejecuta el Jefe de Transporte. Otros puestos la proponen, y proponer no " +
                "consuma el acto.",
                [], EsFaltaDeCompetencia: true);
        }

        var juicio = ReglasDelAltaDeMotorista.Evaluar(alta, hoy);

        if (!juicio.Procede)
        {
            return new ResultadoDelRegistroDeMotorista(
                false, null, Explicar(juicio.Reparos), juicio.Observaciones);
        }

        var licencia = alta.NumeroDeLicencia.Trim();

        // ⚠️ **Una licencia no puede estar en dos personas**, y comprobarlo acá es lo que
        // convierte una violación de índice —un `500` sin instrucción— en una respuesta que
        // dice qué pasó. El índice protege el dato; no explica.
        if (await contexto.Conductores.AnyAsync(c => c.NumeroDeLicencia == licencia, cancelacion))
        {
            return new ResultadoDelRegistroDeMotorista(
                false, null,
                $"La licencia «{licencia}» ya está registrada a nombre de otro motorista del " +
                "padrón. Un número de licencia pertenece a una sola persona.",
                juicio.Observaciones);
        }

        var fila = new FilaDeConductor
        {
            Id = Ulid.NewUlid(),
            Nombre = alta.Nombre.Trim(),
            EsDelPadron = alta.EsDelPadron,
            NumeroDeLicencia = licencia,
            Categoria = alta.Categoria,
            VenceLicencia = alta.VenceLicencia,
            Restricciones = string.IsNullOrWhiteSpace(alta.Restricciones)
                ? null
                : alta.Restricciones.Trim(),
        };

        contexto.Conductores.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);

        return new ResultadoDelRegistroDeMotorista(
            true, fila.Id, $"«{fila.Nombre}» incorporado al padrón de motoristas.",
            juicio.Observaciones);
    }

    /// <summary>El reparo en el vocabulario de quien lo lee, no en el del enum.</summary>
    private static string Explicar(IReadOnlyList<MotivoDeRechazoDelMotorista> reparos) =>
        string.Join(" ", reparos.Select(r => r switch
        {
            MotivoDeRechazoDelMotorista.SinNombre =>
                "Falta el nombre del motorista, que es como aparece en la Orden de Misión y en " +
                "la bitácora.",

            MotivoDeRechazoDelMotorista.SinNumeroDeLicencia =>
                "Falta el número de licencia, que es lo que se cita ante un retén.",

            _ => "No procede el alta.",
        }));
}
