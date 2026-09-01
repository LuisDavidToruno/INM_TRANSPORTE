using Sigti.Aplicacion.M01_Organizacion;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.Organizacion;
using Microsoft.EntityFrameworkCore;

using Sigti.Datos;

using Sigti.Dominio.M03_Flota;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>Lo que contesta un alta de vehículo.</summary>
public sealed record ResultadoDelRegistroDeVehiculo(
    bool Procede,
    Ulid? Id,
    string Mensaje,
    IReadOnlyList<ObservacionDelAlta> Observaciones,

    /// <summary>
    /// El rechazo fue <b>por el puesto de quien pidió</b>, no por el dato.
    ///
    /// Existe porque la API tiene que contestar distinto: no tener competencia es un
    /// <c>403</c> —usted no puede—, y unas siglas repetidas un <c>409</c> —el dato choca—.
    /// Contestar lo mismo a las dos manda a corregir el vehículo a quien lo que tiene mal es
    /// el puesto.
    /// </summary>
    bool EsFaltaDeCompetencia = false);

/// <summary>
/// Incorpora un vehículo a la flota — <b>`M-03`</b>, acción 23 de la matriz de permisos.
/// </summary>
public sealed class ServicioDeAltaDeFlota(SigtiDbContext contexto)
{
    /// <summary>
    /// Quiénes <b>ejecutan</b> la acción 23 de la matriz de permisos —«mantener expediente y
    /// vencimientos del vehículo»—, transcrito de §4 de <c>actores-y-roles.md</c>.
    ///
    /// ⚠️ <b>`ACT-11` Encargado de Mantenimiento figura como <c>E³</c> y no está acá.</b> La
    /// nota al pie 3 dice <i>«informa disponibilidad y estado técnico; no decide la
    /// asignación»</i>, que describe un rol de consulta y parece pertenecer a la fila 4. Ante la
    /// contradicción se toma lo restrictivo y <b>se levanta como hallazgo</b> en vez de
    /// resolverla acá: el estado técnico ya lo mueve por <c>/flota/{id}/estado</c>.
    /// </summary>
    private static readonly Rol[] Ejecutan =
    [
        Rol.JefeDeTransporte,        // ACT-04
        Rol.EncargadoDeDelegacion,   // ACT-10
        Rol.EncargadoDeBienes,       // ACT-14
    ];

    public async Task<ResultadoDelRegistroDeVehiculo> RegistrarAsync(
        AltaDeVehiculo alta,
        DateOnly hoy,
        IdPersona quien,
        CancellationToken cancelacion = default)
    {
        // ⚠️ **La competencia se resuelve a la fecha del hecho** — `RN-100`. No a hoy ni a
        // quién es la persona: a qué puesto ocupaba el día que se registra el alta.
        var suyas = await new ServicioDeCompetencias(contexto)
            .DeLaPersonaAsync(quien, hoy, cancelacion);

        if (!suyas.Roles.Any(Ejecutan.Contains))
        {
            return new ResultadoDelRegistroDeVehiculo(
                false, null,
                "Incorporar un vehículo a la flota es la acción 23 de la matriz de permisos, y " +
                "la ejecutan el Jefe de Transporte, el Encargado de Delegación y el Encargado " +
                "de Bienes Institucionales. El puesto que ocupa hoy no la tiene.",
                [], EsFaltaDeCompetencia: true);
        }

        var juicio = ReglasDelAltaDeVehiculo.Evaluar(alta, hoy);

        if (!juicio.Procede)
        {
            return new ResultadoDelRegistroDeVehiculo(
                false, null, Explicar(juicio.Reparos), juicio.Observaciones);
        }

        var siglas = alta.Siglas.Trim();

        // ⚠️ **Se comprueba antes de insertar, y no se deja al índice único.** El índice
        // protege el dato; lo que no hace es explicar. Sin esto el segundo alta sale como una
        // violación de restricción —un `500` sin instrucción— cuando la respuesta que sirve es
        // «esas siglas ya son de otro vehículo».
        if (await contexto.Vehiculos.AnyAsync(v => v.Siglas == siglas, cancelacion))
        {
            return new ResultadoDelRegistroDeVehiculo(
                false, null,
                $"Las siglas «{siglas}» ya son de otro vehículo de la flota. Las siglas son la " +
                "identidad estable del bien y no se repiten.",
                juicio.Observaciones);
        }

        var fila = new FilaDeVehiculo
        {
            Id = Ulid.NewUlid(),
            Siglas = siglas,
            Placa = string.IsNullOrWhiteSpace(alta.Placa) ? null : alta.Placa.Trim(),
            TieneConstanciaSustitutaDePlaca = false,
            EstadoDePlaca = alta.EstadoDePlaca,
            TipoDeVehiculo = alta.TipoDeVehiculo.Trim(),
            Clase = alta.Clase,
            PesoBrutoKg = alta.PesoBrutoKg,
            CapacidadPasajeros = alta.CapacidadPasajeros,
            LlevaRemolque = alta.LlevaRemolque,
            NumeroDeEjes = alta.NumeroDeEjes,
            VenceMatricula = alta.VenceMatricula,
            IdentificacionInstitucionalVerificada = false,
        };

        contexto.Vehiculos.Add(fila);
        await contexto.SaveChangesAsync(cancelacion);

        return new ResultadoDelRegistroDeVehiculo(
            true, fila.Id, $"Vehículo «{siglas}» incorporado a la flota.", juicio.Observaciones);
    }

    /// <summary>El reparo dicho en el vocabulario de quien lo va a leer, no en el del enum.</summary>
    private static string Explicar(IReadOnlyList<MotivoDeRechazoDelAlta> reparos) =>
        string.Join(" ", reparos.Select(r => r switch
        {
            MotivoDeRechazoDelAlta.SinSiglas =>
                "Faltan las siglas del vehículo, que son su identidad estable y lo que se cita " +
                "en el descargo.",

            MotivoDeRechazoDelAlta.LaminaSinNumero =>
                "Se declaró que el vehículo tiene lámina puesta y no se dio el número de placa. " +
                "La lámina es el número: si no hay número, el estado de la placa es otro.",

            _ => "No procede el alta.",
        }));
}
