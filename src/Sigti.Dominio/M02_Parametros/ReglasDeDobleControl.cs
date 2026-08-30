using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M02_Parametros;

/// <summary>
/// El registro de un intento de aprobación, concedido o no.
///
/// <b>Se escribe siempre, también cuando se rechaza.</b> Bloquear sin registrar dejaría
/// al auditor sin saber que alguien intentó aprobar su propia carga — y ese intento es
/// justamente lo que un control interno quiere ver (`HU-146`).
/// </summary>
public sealed record IntentoDeAprobacion(
    string Clave,
    IdPersona Quien,
    DateTimeOffset Momento,
    bool Concedida,
    string? MotivoDelRechazo);

/// <summary>
/// `RN-39` — Doble control sobre parámetros normativos: quien carga no aprueba.
///
/// Devuelve el intento en lugar de lanzar, porque el llamador tiene que <b>registrarlo
/// igual</b> antes de decidir. Una excepción invitaría a que el rechazo se pierda en un
/// bloque de captura.
/// </summary>
public static class ReglasDeDobleControl
{
    public static IntentoDeAprobacion Evaluar(
        VersionDeParametro version, IdPersona quienAprueba, DateTimeOffset momento,
        bool elRespaldoExiste)
    {
        var rechazo = Rechazo(version, quienAprueba, elRespaldoExiste);

        return new IntentoDeAprobacion(
            version.Clave, quienAprueba, momento,
            Concedida: rechazo is null,
            MotivoDelRechazo: rechazo);
    }

    /// <summary>
    /// Aplica la aprobación. Devuelve la versión aprobada, o nulo si el intento no se
    /// concedió: el llamador registra el intento en ambos casos.
    /// </summary>
    public static VersionDeParametro? Aplicar(VersionDeParametro version, IntentoDeAprobacion intento) =>
        intento.Concedida ? version with { AprobadoPor = intento.Quien } : null;

    private static string? Rechazo(
        VersionDeParametro version, IdPersona quienAprueba, bool elRespaldoExiste)
    {
        if (version.EstaAprobada)
            return "La versión ya fue aprobada. Una segunda aprobación no agrega control, lo simula.";

        // ⚠️ **El identificador del adjunto no es el adjunto.** El tipo exige un `Ulid`, así que
        // la columna nunca está vacía — pero nada garantizaba que apuntara a un archivo que
        // existe, y un respaldo que no está **se ve igual que uno que sí**.
        //
        // `HU-145` lo pone como escenario propio: quien aprueba tiene que poder abrir el
        // documento. Aprobar sin él es firmar que se verificó una fuente que nadie vio.
        // Va antes que la identidad **a propósito**: un respaldo que no está bloquea a
        // cualquiera, y decirle a quien cargó «usted no puede aprobar» lo manda a buscar un
        // colega que se estrellaría contra el mismo muro. Primero lo que le pasa a todos.
        if (!elRespaldoExiste)
            return "La carga no tiene respaldo documental adjunto. Devuelva al Administrador " +
                   "del Sistema para que lo agregue.";

        // La comparación es por identidad de PERSONA, no de usuario: un mismo servidor
        // con dos cuentas sigue siendo la misma persona.
        if (quienAprueba == version.CargadoPor)
            return "Quien carga un parámetro normativo no puede aprobarlo. " +
                   "El doble control exige dos personas distintas (RN-39).";

        return null;
    }
}
