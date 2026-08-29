using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M09_Combustible;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <summary>
/// Arma los actos de un fondo de combustible para el control bloqueante de §5.3.B.
///
/// ── Por qué el fondo necesita el suyo, y no le sirve el de la misión ────────
/// <b>El fondo es objeto DE PERÍODO, no de misión.</b> Es exactamente el motivo por el que
/// `I-19` existe: el par <i>solicita el fondo × aprueba el fondo</i> *«se caía entre las dos»*
/// —`RN-01` razona por Orden de Misión y el fondo no es una— y vivía sólo en el numeral 4 de
/// `RN-26`. Hallazgos `HN1-15` y `HB3-06`.
///
/// Pasarle a la segregación los actos de una misión sería contestar sobre el objeto equivocado:
/// un fondo cubre muchas misiones y ninguna en particular.
/// </summary>
public static class ActosDelFondo
{
    /// <summary>
    /// Qué movimiento del fondo ejerce cada función.
    ///
    /// <b>Sólo dos, y no es una lista incompleta:</b> ampliar y cerrar no son ninguna de las
    /// funciones que el MARCI separa. Inventarles una haría que un cierre bloqueara una
    /// aprobación.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Funcion> Funciones =
        new Dictionary<string, Funcion>
        {
            // `F-01` — `ACT-04` solicita el fondo del período.
            ["F-01"] = Funcion.SolicitaFondo,

            // `F-02` — `ACT-08` aprueba contra cuota y partida.
            ["F-02"] = Funcion.ApruebaFondo,
        };

    /// <summary>
    /// Los actos del fondo, listos para <c>ReglasDeSegregacion</c>.
    /// </summary>
    /// <param name="referencia">
    /// Cómo nombrar el fondo en el mensaje del bloqueo. §5.3.B.1 exige precisión, y «este
    /// fondo» no le dice a nadie cuál de los que tiene abiertos.
    ///
    /// <b>Viene con su preposición incluida</b> —«del fondo de …»— porque el mensaje la
    /// concatena tal cual: anteponerle una acá producía «solicitud de el fondo».
    /// </param>
    public static ActosDelExpediente De(FondoDeCombustible fondo, string referencia)
    {
        var actos = new List<ActoDelExpediente>();

        foreach (var m in fondo.Diario)
        {
            if (!Funciones.TryGetValue(m.Id, out var funcion)) continue;

            actos.Add(new ActoDelExpediente(
                funcion,
                m.Ejecuta,
                // Sin «de» delante: la referencia ya trae su preposición. Concatenarla
                // producía «solicitud de el fondo…», que se lee como un error del sistema —y
                // en §5.3.B.1 el mensaje **es** el control.
                $"{Nombre(funcion)} {referencia} ({m.Id})",
                DateOnly.FromDateTime(m.Momento.Date)));
        }

        return new ActosDelExpediente(actos);
    }

    private static string Nombre(Funcion funcion) => funcion switch
    {
        Funcion.SolicitaFondo => "solicitud",
        Funcion.ApruebaFondo => "aprobación",
        _ => funcion.ToString(),
    };
}
