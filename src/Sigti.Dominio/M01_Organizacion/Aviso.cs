using Sigti.Dominio.M02_Parametros;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Por dónde se le avisa a quien tiene algo pendiente.
///
/// ── Ninguno se elige acá ────────────────────────────────────────────────────
/// <b>Cuál usa la institución es parámetro con vigencia</b>, no una constante del código.
/// Cablearlo obligaría a un despliegue el día que una delegación consiga señal, y a otro el día
/// que la pierda.
/// </summary>
public enum CanalDeAviso
{
    /// <summary>
    /// Sólo la bandeja del sistema.
    ///
    /// <b>No es «ningún canal».</b> Es un canal legítimo y puede ser el único posible: más de dos
    /// millones de personas del área rural hondureña no tienen acceso a internet, y en una
    /// delegación sin señal el correo y el SMS no llegan. Lo que declara es que <b>el aviso
    /// depende de que la persona entre al sistema</b>, y eso hay que poder decirlo.
    /// </summary>
    SoloBandeja,

    /// <summary>Correo institucional. Exige servidor de correo configurado.</summary>
    CorreoInstitucional,

    /// <summary>Mensaje de texto. Exige pasarela contratada.</summary>
    MensajeDeTexto,
}

/// <summary>Cómo terminó el intento de avisar.</summary>
public enum ResultadoDelAviso
{
    /// <summary>Salió por el canal configurado.</summary>
    Entregado,

    /// <summary>
    /// <b>La institución no fijó el canal.</b> No es un fallo del envío: es que nadie decidió
    /// por dónde avisar, y son cosas distintas para quien tenga que arreglarlo.
    /// </summary>
    SinCanalConfigurado,

    /// <summary>
    /// El canal está fijado pero <b>no está implementado</b> en el sistema.
    ///
    /// Se distingue de <see cref="Fallido"/> a propósito: un correo que no sale porque no hay
    /// servidor SMTP es un problema de infraestructura; uno que no sale porque el sistema no
    /// sabe mandar correos es un problema de construcción, y los arreglan personas distintas.
    /// </summary>
    CanalNoImplementado,

    /// <summary>El canal existe y el envío falló. El motivo se guarda.</summary>
    Fallido,
}

/// <summary>
/// Un intento de avisarle a alguien — <b>§5.3.B.3, la mitad que faltaba</b>.
///
/// ── Por qué se guarda el intento y no sólo el éxito ─────────────────────────
/// Mismo criterio que el intento bloqueado de §5.3.B.2: <b>un sistema que sólo guarda lo que
/// salió no puede contestar si se avisó</b>. Un aviso perfecto y uno que nunca se intentó se
/// ven exactamente igual —no hay rastro de ninguno— y eso es justamente lo que separa «no
/// contestó» de «nadie le escribió».
/// </summary>
/// <param name="Detalle">
/// Por qué no salió, cuando no salió. <b>Nulo cuando se entregó</b>: un motivo inventado para
/// un envío exitoso es ruido en la pista.
/// </param>
public sealed record Aviso(
    Ulid Id,
    Ulid Tarea,
    IdPersona Destinatario,
    CanalDeAviso? Canal,
    ResultadoDelAviso Resultado,
    DateTimeOffset Momento,
    string? Detalle)
{
    /// <summary>Si el destinatario se enteró por fuera del sistema.</summary>
    public bool LlegoAlDestinatario => Resultado == ResultadoDelAviso.Entregado;
}

/// <summary>
/// Decide por dónde avisar y con qué resultado — <b>sin inventar la decisión</b>.
///
/// ── Lo que esta regla NO hace ───────────────────────────────────────────────
/// <b>No elige el canal.</b> Cuál usa la institución es `[C]`: el insumo #102. Y no se supone
/// uno «razonable» —correo, por ejemplo— porque suponerlo produciría un sistema que cree haber
/// avisado y una persona que nunca recibió nada, que es peor que no avisar.
///
/// ── Por qué <see cref="CanalDeAviso.SoloBandeja"/> entrega de verdad ────────
/// Porque la tarea <b>ya está</b> en la bandeja cuando esto corre. Si la institución elige ese
/// canal, el aviso se cumplió: lo que cambia no es si la persona puede enterarse, sino si tiene
/// que entrar a mirar. Marcarlo como fallo diría que el sistema no hizo lo que se le pidió.
/// </summary>
public static class ReglasDelAviso
{
    /// <summary>
    /// La clave del parámetro. <b>Sin valor por omisión a propósito</b>: la institución elige, y
    /// suponer uno haría que el sistema dijera haber avisado por un canal que nadie configuró.
    /// </summary>
    public const string ClaveDelCanal = "aviso.canal";

    /// <summary>
    /// Resuelve el aviso para un destinatario.
    /// </summary>
    /// <param name="canal">
    /// El canal vigente a la fecha del hecho, o <b>nulo si la institución no lo fijó</b>. Se
    /// recibe resuelto: esta clase no lee el catálogo ni el reloj.
    /// </param>
    /// <param name="implementados">
    /// Qué canales sabe usar el sistema hoy. Se recibe en vez de cablearse para que agregar el
    /// correo sea registrar una implementación, no editar esta regla.
    /// </param>
    public static Aviso Resolver(
        Ulid id,
        Ulid tarea,
        IdPersona destinatario,
        CanalDeAviso? canal,
        IReadOnlyList<CanalDeAviso> implementados,
        DateTimeOffset momento)
    {
        if (canal is not { } elegido)
        {
            return new Aviso(id, tarea, destinatario, null,
                ResultadoDelAviso.SinCanalConfigurado, momento,
                $"La institución no fijó «{ClaveDelCanal}». Quien tenga algo pendiente sólo se " +
                "entera si abre la bandeja. No es que no contestara: es que nadie le escribió.");
        }

        if (!implementados.Contains(elegido))
        {
            return new Aviso(id, tarea, destinatario, elegido,
                ResultadoDelAviso.CanalNoImplementado, momento,
                $"La institución fijó «{elegido}» y el sistema todavía no sabe usar ese canal. " +
                "Es un pendiente de construcción, no una falla de envío.");
        }

        return new Aviso(id, tarea, destinatario, elegido,
            ResultadoDelAviso.Entregado, momento,

            // Nulo cuando se entregó: un motivo para un envío exitoso es ruido en la pista.
            null);
    }

    /// <summary>
    /// Los canales que el sistema sabe usar hoy.
    ///
    /// ── Sólo uno, y se dice ─────────────────────────────────────────────────
    /// La bandeja. El correo institucional y el mensaje de texto <b>no están construidos</b>, y
    /// además necesitan cada uno un dato que tampoco existe: servidor de correo o pasarela
    /// contratada. Declararlos acá antes de tiempo haría que el sistema dijera «entregado» sobre
    /// un envío que nunca salió.
    /// </summary>
    public static readonly IReadOnlyList<CanalDeAviso> Implementados = [CanalDeAviso.SoloBandeja];

    /// <summary>
    /// Lee el canal del catálogo. <b>Nulo cuando la institución no lo fijó</b>.
    ///
    /// Usa <see cref="CatalogoDeParametros.ResolverSiHay"/> y no el que bloquea, y la frontera de
    /// esa clase lo explica: se bloquea lo que decide un número que alguien va a cobrar o pagar.
    /// <b>Un aviso ausente no cambia ningún monto</b> — deja a alguien sin enterarse, que es
    /// grave y es otra cosa. Bloquear el encolado por falta de canal dejaría el acto sin bandeja
    /// <i>y</i> sin aviso.
    /// </summary>
    public static CanalDeAviso? CanalVigente(
        CatalogoDeParametros catalogo, DateOnly fechaDelHecho, DateTimeOffset conocidoAl)
    {
        var valor = catalogo.ResolverSiHay(ClaveDelCanal, fechaDelHecho, conocidoAl);

        if (valor is null) return null;

        return Enum.TryParse<CanalDeAviso>(valor.Valor, ignoreCase: true, out var canal)
            ? canal

            // Un valor que no corresponde a ningún canal **no se aproxima al más parecido**:
            // se trata como si no estuviera fijado, y el detalle del aviso lo dirá.
            : null;
    }
}
