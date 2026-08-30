namespace Sigti.Dominio.M16_Sincronizacion;

/// <summary>
/// Qué pasó con cada registro enviado — `HU-067`.
///
/// ── Por qué no basta con «sincronizado» ─────────────────────────────────────
/// <i>«Un "sincronizado con éxito" que en realidad significa "de 34 registros, 31 se aplicaron,
/// 1 espera a otro que no llegó y 2 están en conflicto" <b>es una mentira operativa</b>. El día
/// que se descubra, el motorista deja de confiar y vuelve al papel.»</i>
///
/// Volver al papel no es una molestia: es perder el sistema entero en esa delegación.
/// </summary>
public enum EstadoDelEnvio
{
    /// <summary>Entró y quedó registrado.</summary>
    Aceptado,

    /// <summary>
    /// Llegó dos veces. <b>No es un error</b>: es el reintento normal de un dispositivo que no
    /// supo si el servidor recibió.
    /// </summary>
    YaEstaba,

    /// <summary>
    /// Espera un registro anterior que todavía no llegó — `HU-067`.
    ///
    /// ⚠️ <b>Ni se aplica ni se rechaza.</b> Es transitorio y se resuelve solo cuando llega el
    /// que falta: rechazarlo obligaría al dispositivo a reenviarlo sin saber cuándo, y
    /// encolarlo como conflicto le pediría a una persona que <b>decida sobre algo que no es una
    /// discrepancia</b> — sólo llegó en desorden.
    /// </summary>
    EsperandoUnAnterior,

    /// <summary>Dos versiones que no coinciden. Va a la cola y alguien decide.</summary>
    NecesitaQueAlguienDecida,

    /// <summary>
    /// No entra, y reintentar no lo arregla: el hecho está mal armado —un retorno sin lectura
    /// de odómetro— y el dispositivo no lo puede resolver reenviando.
    /// </summary>
    NoSePudoRegistrar,
}

/// <summary>
/// Cómo se le dice al motorista — `HU-067`, criterio de aceptación <b>literal</b>.
/// </summary>
public static class ReglasDelEnvio
{
    /// <summary>
    /// Las frases exactas que la historia enumera.
    ///
    /// <i>«Cada registro aparece como "enviado y aceptado", "esperando un registro anterior que
    /// no ha llegado", "ya estaba registrado" o "necesita que alguien decida". Y ningún texto de
    /// la pantalla contiene "merge", "versión del registro", "timestamp" ni "conflicto de
    /// escritura".»</i>
    /// </summary>
    public static string EnPalabras(EstadoDelEnvio estado) => estado switch
    {
        EstadoDelEnvio.Aceptado => "enviado y aceptado",
        EstadoDelEnvio.YaEstaba => "ya estaba registrado",
        EstadoDelEnvio.EsperandoUnAnterior => "esperando un registro anterior que no ha llegado",
        EstadoDelEnvio.NecesitaQueAlguienDecida => "necesita que alguien decida",
        EstadoDelEnvio.NoSePudoRegistrar => "no se pudo registrar",
        _ => "sin clasificar",
    };

    /// <summary>
    /// Si un resumen puede presentarse como «todo bien».
    ///
    /// <b>Sólo cuando de verdad no queda nada por hacer.</b> Un envío con registros en espera o
    /// en conflicto no está terminado, y decir que sí es la mentira operativa que `HU-067`
    /// nombra.
    /// </summary>
    public static bool TerminoLimpio(IEnumerable<EstadoDelEnvio> estados) =>
        estados.All(e => e is EstadoDelEnvio.Aceptado or EstadoDelEnvio.YaEstaba);

    /// <summary>
    /// Los que todavía piden algo de alguien. Se cuentan aparte porque piden cosas distintas:
    /// el que espera se resuelve solo cuando llegue el que falta; el que necesita decisión
    /// espera a una persona, y no se va a mover hasta que alguien la tome.
    /// </summary>
    public static bool SigueAbierto(EstadoDelEnvio estado) =>
        estado is EstadoDelEnvio.EsperandoUnAnterior
                or EstadoDelEnvio.NecesitaQueAlguienDecida
                or EstadoDelEnvio.NoSePudoRegistrar;
}
