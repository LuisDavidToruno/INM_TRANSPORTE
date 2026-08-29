using Sigti.Dominio.M01_Organizacion;

namespace Sigti.Datos;

/// <summary>
/// Una tarea de la bandeja — <b>§5.3.B.3</b>.
///
/// ── Por qué la bandeja es tabla y el aviso es una columna ───────────────────
/// Porque son cosas de distinta naturaleza. La bandeja es el <b>sistema de registro</b>: existe
/// aunque no haya red, correo ni teléfono, y esto se despliega <i>on-premise</i> en
/// instituciones donde nada de eso está garantizado. El aviso es una <b>cortesía</b> que puede
/// no llegar.
///
/// Guardar sólo el aviso —una cola de correos— dejaría el trabajo perdido cada vez que el envío
/// falla, y nadie se enteraría de que se perdió.
/// </summary>
public sealed class FilaDeTarea
{
    public required Ulid Id { get; init; }

    /// <summary>Qué la originó. Enum por nombre: un tipo suelto en texto se escribe de catorce maneras.</summary>
    public required TipoDeTarea Tipo { get; init; }

    public required string Asunto { get; init; }

    public required string Detalle { get; init; }

    /// <summary>Sobre qué expediente, fondo o vale.</summary>
    public required string Expediente { get; init; }

    /// <summary>A quién se le impidió el acto. <b>No puede resolver su propia tarea.</b></summary>
    public required string QuienLaOrigino { get; init; }

    /// <summary>
    /// A qué puesto le toca. <b>Nulo cuando el destino es Gerencia Administrativa</b>, que es el
    /// último recurso del escalamiento y no un puesto de la jerarquía de quien quedó bloqueado.
    /// </summary>
    public string? PuestoDestino { get; init; }

    /// <summary>
    /// Quiénes lo ocupaban <b>al encolar</b>, separados por coma.
    ///
    /// Es una foto congelada, como <c>ReservaAfectada</c>: si mañana rota el puesto, la tarea
    /// sigue diciendo a quién le tocó. <b>Quien la resuelva se compara contra el puesto</b>, no
    /// contra esta lista — o una rotación dejaría la tarea sin nadie que la pueda cerrar.
    /// </summary>
    public required string PersonasDestino { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required EstadoDeTarea Estado { get; set; }

    /// <summary>
    /// Cuándo se avisó al destinatario.
    ///
    /// <b>Nulo es «no se avisó»</b>, no «se avisó y no contestó». Hoy es siempre nulo: no hay
    /// canal de notificación construido en ningún módulo, y decirlo es lo que impide que una
    /// bandeja llena se lea como gente que ignora su trabajo.
    /// </summary>
    public DateTime? NotificadoUtc { get; set; }

    /// <summary>Quién la cerró. Nulo mientras está pendiente.</summary>
    public string? Resuelve { get; set; }

    public DateTime? ResueltaUtc { get; set; }

    /// <summary>
    /// Qué se hizo. <b>Obligatorio al cerrar</b>: «lo autorizó el jefe» y «ya no hacía falta»
    /// dejan el mismo rastro vacío si no se escribe, y son cosas distintas para quien audite.
    /// </summary>
    public string? Resolucion { get; set; }
}
