using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Qué originó la tarea.
///
/// <b>Se enumera y no se deja como texto libre</b> porque de esto depende quién la puede
/// resolver y qué significa resolverla. Un tipo suelto en una cadena se convierte en catorce
/// variantes escritas de catorce maneras, y ninguna consulta las encuentra todas.
/// </summary>
public enum TipoDeTarea
{
    /// <summary>
    /// Un acto bloqueado por segregación de funciones — §5.3.B.3.
    /// </summary>
    SegregacionBloqueada,

    /// <summary>
    /// Una reserva quedó en conflicto por indisponibilidad sobrevenida — `RN-60` punto 3.
    /// </summary>
    ReservaEnConflicto,

    /// <summary>Un préstamo pasó su fecha de devolución comprometida — `RN-63` punto 4.</summary>
    PrestamoVencido,
}

/// <summary>En qué situación está la tarea.</summary>
public enum EstadoDeTarea
{
    Pendiente,
    /// <summary>Alguien la atendió. <b>Exige decir qué hizo</b>, no sólo que la vio.</summary>
    Resuelta,
    /// <summary>
    /// Ya no aplica: el expediente siguió otro camino.
    ///
    /// <b>No es lo mismo que resuelta</b>, y por eso son dos estados: descartar dice que nadie
    /// tuvo que hacer nada, y un reporte que las junte no puede distinguir el control que operó
    /// del que se volvió innecesario.
    /// </summary>
    Descartada,
}

/// <summary>
/// Algo que quedó esperando a alguien — <b>la bandeja de §5.3.B.3</b>.
///
/// ── Por qué la bandeja es el sistema de registro y el aviso es una cortesía ──
/// El documento pide dos cosas: que la acción <i>«quede visiblemente pendiente en la bandeja de
/// alguien»</i> y que el sistema <i>«notifique al destinatario»</i>. <b>No son intercambiables.</b>
/// Un correo que no llega deja el trabajo perdido; una bandeja que se abre al entrar al sistema
/// no depende de que haya red, servidor de correo ni teléfono — y esto se despliega
/// <i>on-premise</i> en instituciones donde nada de eso está garantizado.
///
/// Por eso la bandeja se construye primero y el aviso se declara.
///
/// ── Por qué no es sólo para la segregación ──────────────────────────────────
/// `RN-60` punto 3 espera notificar a `ACT-04` y a la dependencia solicitante cuando una reserva
/// queda en conflicto; `RN-63` punto 4 espera el escalamiento diario por mora. Las dos venían
/// arrastrando el mismo pendiente. Una bandeja específica de segregación las habría dejado
/// esperando otra vez.
/// </summary>
/// <param name="PuestoDestino">
/// A qué puesto le toca. <b>Nulo cuando el destino es Gerencia Administrativa</b>, que es el
/// último recurso del escalamiento y no un puesto de la jerarquía de quien quedó bloqueado.
/// </param>
/// <param name="PersonasDestino">
/// Quiénes lo ocupaban <b>al momento de encolar</b>. Es una foto congelada, como
/// <c>ReservaAfectada</c>: si mañana rota el puesto, la tarea sigue diciendo a quién le tocó —y
/// quien la resuelva se compara contra el puesto, no contra esta lista.
/// </param>
/// <param name="Notificado">
/// Cuándo se avisó al destinatario. <b>Nulo es «no se avisó»</b>, no «se avisó y no contestó».
/// Hoy es siempre nulo: no hay canal de notificación construido, y decirlo es lo que impide que
/// una bandeja llena se lea como gente que ignora su trabajo.
/// </param>
public sealed record TareaPendiente(
    Ulid Id,
    TipoDeTarea Tipo,
    string Asunto,
    string Detalle,
    string Expediente,
    IdPersona QuienLaOrigino,
    IdPuesto? PuestoDestino,
    IReadOnlyList<IdPersona> PersonasDestino,
    DateTimeOffset Momento,
    EstadoDeTarea Estado,
    DateTimeOffset? Notificado)
{
    /// <summary>
    /// Si el aviso salió. <b>Falso no acusa a nadie</b>: hoy no hay canal.
    /// </summary>
    public bool SeAviso => Notificado is not null;

    /// <summary>
    /// Cuántos días lleva esperando, a una fecha.
    ///
    /// <b>Sólo tiene sentido en las pendientes.</b> Una resuelta lleva los días que llevó, y
    /// mostrarlos como espera diría que sigue esperando.
    /// </summary>
    public int DiasEsperando(DateTimeOffset ahora) =>
        Estado == EstadoDeTarea.Pendiente
            // **Nunca negativo.** Una tarea cuya fecha del hecho es posterior a hoy todavia no
            // espera, y «-4 dias esperando» se lee como un error del sistema. Cero es la
            // respuesta correcta: no ha esperado nada.
            ? Math.Max(0, (ahora.Date - Momento.Date).Days)
            : 0;
}

/// <summary>
/// Las reglas de la bandeja.
/// </summary>
public static class ReglasDeLaTarea
{
    public const string Precondicion = "§5.3.B.3";

    /// <summary>
    /// <b>Quien originó la tarea no la resuelve.</b>
    ///
    /// ── Es el punto entero del escalamiento ─────────────────────────────────
    /// La tarea existe porque a esa persona se le impidió el acto. Dejarla resolverla convierte
    /// el escalamiento en una formalidad: apretaría «resuelto» y seguiría. §5.3.B.3 la manda a
    /// <i>otra</i> bandeja precisamente para que decida alguien más.
    /// </summary>
    public static void ExigirQueNoLaResuelvaQuienLaOrigino(
        TareaPendiente tarea, IdPersona resuelve)
    {
        if (resuelve != tarea.QuienLaOrigino) return;

        throw new BloqueoDuro(Precondicion,
            $"Esta tarea existe porque a usted se le impidió el acto ({tarea.Asunto}). " +
            "No puede darla por resuelta: el escalamiento la puso en otra bandeja justamente " +
            "para que decida otra persona. Si nadie la atiende, lo que corresponde es " +
            "escalarla de nuevo, no cerrarla.");
    }

    /// <summary>
    /// Resolver <b>exige decir qué se hizo</b>.
    ///
    /// Una tarea cerrada sin motivo no distingue *«lo autorizó el jefe»* de *«ya no hacía
    /// falta»*, y las dos dejan el mismo rastro vacío en el reporte que Auditoría Interna
    /// revisa. §5.3.B.2 ya trata el intento como información de control; su desenlace vale igual.
    /// </summary>
    public static void ExigirMotivo(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length < 8)
        {
            throw new BloqueoDuro(Precondicion,
                "Resolver una tarea escalada exige decir qué se hizo. Sin eso, «lo autorizó el " +
                "jefe» y «ya no hacía falta» dejan el mismo rastro vacío, y son cosas distintas " +
                "para quien audite.");
        }
    }

    /// <summary>
    /// Una tarea ya cerrada no se vuelve a cerrar.
    ///
    /// <b>Y no es un detalle técnico</b>: dos resoluciones sobre el mismo hecho dejarían dos
    /// versiones de qué pasó, y la pista de auditoría no podría decir cuál rigió.
    /// </summary>
    public static void ExigirPendiente(TareaPendiente tarea)
    {
        if (tarea.Estado == EstadoDeTarea.Pendiente) return;

        throw new BloqueoDuro(Precondicion,
            $"Esta tarea ya está {tarea.Estado.ToString().ToLowerInvariant()}. " +
            "Dos resoluciones sobre el mismo hecho dejarían dos versiones de qué pasó, y la " +
            "pista no podría decir cuál rigió.");
    }
}
