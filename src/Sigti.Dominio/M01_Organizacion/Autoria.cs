using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Quién hizo un acto, y <b>con qué competencia</b>.
///
/// ── Por qué guarda los dos ───────────────────────────────────────────────────
/// El auditor no pregunta *«¿quién firmó?»*. Pregunta <b>«¿quién autorizó esto y con qué
/// competencia?»</b>, y el nombre solo no responde: la competencia estaba en el puesto, y
/// el puesto pudo haber cambiado de manos tres veces desde entonces.
///
/// Guardar solo la persona deja el acto <b>sin fundamento</b>. Guardar solo el puesto lo
/// deja <b>sin responsable</b>. Por eso van los dos, y van <b>congelados</b>: el asiento
/// dice qué puesto ocupaba esa persona el día del hecho, no cuál ocupa hoy.
///
/// ── Por qué es inmutable, y no por elegancia ─────────────────────────────────
/// `P-3` — nada se sobrescribe. El día que una reestructuración renombre un puesto, va a
/// aparecer la tentación de «corregir» los asientos viejos para que cuadren. Siendo un
/// <c>record</c> con propiedades de solo inicialización, el compilador lo impide: quien
/// quiera cambiar algo tiene que crear un asiento nuevo que refiera al anterior, que es
/// exactamente lo que `RN-04` obliga.
///
/// Es también la razón de que un puesto suprimido <b>no se borre del catálogo</b>: se
/// cierra con vigencia. Los actos que autorizó siguen existiendo y tienen que poder
/// explicarse.
/// </summary>
public sealed record Autoria
{
    private Autoria(IdPersona persona, IdPuesto puesto, DateOnly fechaDelHecho)
    {
        Persona = persona;
        Puesto = puesto;
        FechaDelHecho = fechaDelHecho;
    }

    /// <summary>Quién. <b>No se reasigna jamás</b>, ni cuando la persona deja la institución.</summary>
    public IdPersona Persona { get; init; }

    /// <summary>
    /// Con qué competencia. <b>Copia, no referencia</b>: si apuntara al puesto vivo, una
    /// reestructuración reescribiría la historia sin que nadie lo pidiera.
    /// </summary>
    public IdPuesto Puesto { get; init; }

    /// <summary>
    /// La fecha del hecho, no la de captura (`P-4`, `RN-46`). Es contra ésta que se
    /// resuelve si la persona tenía el puesto — y por eso reevaluar un expediente viejo
    /// reproduce la decisión que se tomó, en vez de juzgarla con el organigrama de hoy.
    /// </summary>
    public DateOnly FechaDelHecho { get; init; }

    /// <summary>
    /// Registra la autoría de un acto.
    ///
    /// <b>Quien llama tiene que haber comprobado la competencia antes</b> — con
    /// <see cref="Organigrama.Ocupa"/>. Esta clase no verifica: registra lo verificado, y
    /// separarlo es deliberado, porque el organigrama vive en el espejo de ARGOS y el
    /// asiento vive para siempre en la bitácora.
    /// </summary>
    public static Autoria De(IdPersona persona, IdPuesto puesto, DateOnly fechaDelHecho) =>
        new(persona, puesto, fechaDelHecho);
}
