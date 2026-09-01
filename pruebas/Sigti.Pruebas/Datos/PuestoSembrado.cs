using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// Una persona que ocupa un puesto con la competencia que la prueba necesita.
///
/// ── Por qué se siembra un puesto y no un permiso a la persona ───────────────
/// Porque `RNF-14` es literal: <i>«permisos asignados directamente a una persona: 0. El modelo
/// no ofrece la operación.»</i> La competencia vive en el puesto y `RN-100` la resuelve a la
/// fecha del hecho. Una prueba que se saltara eso estaría probando un modelo que no existe.
/// </summary>
public static class PuestoSembrado
{
    /// <summary>
    /// Crea un puesto con <paramref name="rol"/>, le asigna una persona nueva, y devuelve
    /// quién es. <b>Los identificadores son únicos por llamada</b>: la base de pruebas se
    /// comparte entre clases y dos sembrados con el mismo nombre se pisan.
    /// </summary>
    public static async Task<IdPersona> ConRolAsync(
        SigtiDbContext contexto, Rol rol, DateOnly desde)
    {
        // ⚠️ **La COLA, no el prefijo.** Un ULID es ordenado por tiempo: sus primeros 10
        // caracteres son la marca de tiempo en milisegundos, y dos llamadas seguidas la
        // comparten. Un sufijo tomado del prefijo no distingue nada — parece azar y es reloj.
        var sufijo = Ulid.NewUlid().ToString()[^10..];
        var puesto = $"PUE-PRUEBA-{sufijo}";
        var persona = $"P-PRUEBA-{sufijo}";

        contexto.Competencias.Add(new FilaDeCompetencia
        {
            Id = Ulid.NewUlid(),
            Puesto = puesto,
            Rol = rol,
            Alcance = AlcanceDeDatos.Institucion,
            Desde = desde,
            Otorga = "P-SEMBRADO",
        });

        contexto.AsignacionesDePuesto.Add(new FilaDeAsignacionDePuesto
        {
            Id = Ulid.NewUlid(),
            Persona = persona,
            Puesto = puesto,

            // Propia: es un puesto funcional de SIGTI, no una fila que llegó del padrón.
            Origen = OrigenDeLaAsignacion.Propia,
            Desde = desde,
            ConfirmadoAlUtc = DateTime.UtcNow,
        });

        await contexto.SaveChangesAsync();

        return new IdPersona(persona);
    }
}
