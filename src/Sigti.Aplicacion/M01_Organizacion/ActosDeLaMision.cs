using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M01_Organizacion;

/// <summary>
/// Arma los actos de una Orden de Misión para el control bloqueante de §5.3.B.
///
/// ── Todo sale del diario, no de campos ──────────────────────────────────────
/// `P-1`: el estado es la proyección del diario. Quién ejerció cada función también — el diario
/// ya guarda <c>Ejecuta</c> en cada transición, así que no hace falta ningún campo nuevo ni
/// ninguna tabla nueva para saber quién solicitó, quién autorizó y quién despachó.
///
/// ── El mapa transición → función es donde se mete la pata ───────────────────
/// Igual que el puente rol→función: la tabla puede estar perfecta y este mapa mal, y entonces
/// el bloqueo compara contra el acto equivocado. Cada línea cita qué transición es.
/// </summary>
public static class ActosDeLaMision
{
    /// <summary>
    /// Qué función ejerce cada transición del ciclo de vida.
    ///
    /// <b>No están todas las transiciones y no deben estarlo:</b> devolver, rechazar o anular no
    /// son ninguna de las cinco funciones que el MARCI separa. Una transición sin entrada no
    /// aporta actos, que es lo correcto — inventarle una función haría que un rechazo bloqueara
    /// una liquidación.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Funcion> Funciones =
        new Dictionary<string, Funcion>
        {
            // `T-02` — el envío a autorización es el acto de solicitar. **No `T-01`**: la
            // captura del borrador puede hacerla un asistente, y `BD-01` ya distingue quien
            // captura de quien solicita de derecho.
            ["T-02"] = Funcion.Solicita,

            // `T-05` — el pronunciamiento sobre la procedencia de la necesidad.
            ["T-05"] = Funcion.Autoriza,

            // `T-08` — programar es **emitir la Orden de Misión**, que es de `ACT-04` y no es
            // autorizar. La diferencia es todo `I-14`.
            ["T-08"] = Funcion.EmiteOrdenDeMision,

            // `T-12` — el acto físico de entrega del vehículo.
            ["T-12"] = Funcion.Despacha,

            // `T-20` — el descargo conciliado.
            ["T-20"] = Funcion.Liquida,
        };

    /// <summary>
    /// Los actos del expediente, listos para <c>ReglasDeSegregacion</c>.
    /// </summary>
    /// <param name="conductor">
    /// Quién conduce, resuelto contra el padrón. <b>Se recibe</b> porque el diario guarda el
    /// identificador del conductor y no su identidad de persona, y `I-11` compara personas.
    ///
    /// Nulo es <b>«no se pudo resolver»</b>, no «nadie conduce»: sin él `I-11` —núcleo
    /// irreductible, el vector de fraude clásico— no se evalúa, y eso se declara en vez de
    /// pasar por alto.
    /// </param>
    /// <param name="entregoElFondo">
    /// Quién entregó el vale. Vive en `M-09`, que la misión no conoce. Mismo criterio: nulo es
    /// que no se resolvió.
    /// </param>
    public static ActosDelExpediente De(
        OrdenDeMision expediente,
        string folio,
        IdPersona? conductor,
        IdPersona? entregoElFondo)
    {
        var actos = new List<ActoDelExpediente>();

        foreach (var t in expediente.Diario)
        {
            if (!Funciones.TryGetValue(t.Id, out var funcion)) continue;

            actos.Add(new ActoDelExpediente(
                funcion,
                t.Ejecuta,
                $"{Nombre(funcion)} de {folio} ({t.Id})",
                DateOnly.FromDateTime(t.Momento.Date)));
        }

        // El solicitante de derecho **no siempre es quien envió**: `BD-01` los separa a
        // propósito, y `I-01` a `I-04` hablan de quien solicita, no de quien tecleó.
        actos.Add(new ActoDelExpediente(
            Funcion.Solicita,
            expediente.SolicitanteDeDerecho,
            $"solicitud de {folio}, a su nombre",
            DateOnly.FromDateTime(expediente.Diario[0].Momento.Date)));

        if (conductor is { } quienConduce)
        {
            actos.Add(new ActoDelExpediente(
                Funcion.Conduce,
                quienConduce,
                $"conducción de {folio}",
                DateOnly.FromDateTime(expediente.Diario[^1].Momento.Date)));
        }

        if (entregoElFondo is { } quienEntrego)
        {
            actos.Add(new ActoDelExpediente(
                Funcion.EntregaFondo,
                quienEntrego,
                $"entrega del fondo de {folio}",
                DateOnly.FromDateTime(expediente.Diario[^1].Momento.Date)));
        }

        return new ActosDelExpediente(actos);
    }

    private static string Nombre(Funcion funcion) => funcion switch
    {
        Funcion.Solicita => "solicitud",
        Funcion.Autoriza => "autorización",
        Funcion.EmiteOrdenDeMision => "emisión de la Orden de Misión",
        Funcion.Despacha => "despacho",
        Funcion.Liquida => "liquidación",
        _ => funcion.ToString(),
    };
}
