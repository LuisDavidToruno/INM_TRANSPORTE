using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M01_Organizacion;

/// <summary>
/// Qué expedientes ve un puesto — `actores-y-roles` §3.
///
/// ── Falla cerrado, siempre ──────────────────────────────────────────────────
/// Cuando el alcance <b>no se puede resolver</b> —el puesto no está en el espejo, la unidad no
/// se conoce— esto devuelve <b>nada</b> y dice por qué. Nunca devuelve todo.
///
/// Un control de acceso que ante la duda abre no es un control: es un control que sólo funciona
/// mientras nada falle, y lo que falla es justamente el espejo de `ACT-16`. La consecuencia de
/// fallar cerrado es que alguien ve una lista vacía y llama; la de fallar abierto es que ve los
/// expedientes de toda la institución y <b>nadie se entera</b>.
/// </summary>
public static class ReglasDelAlcance
{
    /// <summary>
    /// Las unidades que un puesto alcanza con nivel <c>Dependencia</c>: la suya <b>y las de sus
    /// puestos subordinados</b>, recorriendo la cadena hacia abajo.
    ///
    /// §3.1 dice «la unidad organizativa del puesto y sus unidades descendientes». La jerarquía
    /// que el espejo publica es de <b>puestos</b>, no de unidades, así que las unidades
    /// descendientes se obtienen de los puestos que cuelgan. Es la lectura fiel del dato que hay;
    /// si algún día el espejo publica el árbol de unidades, esto se reemplaza por él.
    /// </summary>
    public static IReadOnlySet<string> UnidadesAlcanzadas(
        IdPuesto puesto, IReadOnlyList<Puesto> espejo)
    {
        var suyo = espejo.FirstOrDefault(p => p.Id == puesto);
        if (suyo is null) return new HashSet<string>();

        var unidades = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { suyo.Unidad };

        // Recorrido por niveles. Un ciclo en el espejo —A superior de B y B de A— colgaría un
        // recorrido recursivo ingenuo; `vistos` lo corta. El espejo viene de otro sistema y no
        // se puede suponer que siempre sea un árbol.
        var vistos = new HashSet<IdPuesto> { puesto };
        var frontera = new Queue<IdPuesto>([puesto]);

        while (frontera.Count > 0)
        {
            var actual = frontera.Dequeue();

            foreach (var hijo in espejo.Where(p => p.Superior == actual))
            {
                if (!vistos.Add(hijo.Id)) continue;
                unidades.Add(hijo.Unidad);
                frontera.Enqueue(hijo.Id);
            }
        }

        return unidades;
    }

    /// <summary>
    /// Resuelve el alcance de un puesto en algo con lo que se pueda filtrar.
    ///
    /// <b>El resultado dice si se pudo resolver</b>, y cuando no, por qué. Quien filtre tiene
    /// que mirar eso: tratar un alcance irresoluble como «sin restricción» es exactamente el
    /// fallo abierto que esta clase existe para impedir.
    /// </summary>
    public static AlcanceResuelto Resolver(
        IdPuesto puesto, AlcanceDeDatos nivel, IReadOnlyList<Puesto> espejo)
    {
        if (nivel == AlcanceDeDatos.Institucion)
            return AlcanceResuelto.Todo(nivel);

        var suyo = espejo.FirstOrDefault(p => p.Id == puesto);

        if (suyo is null)
            return AlcanceResuelto.Nada(nivel,
                $"El puesto {puesto} no está en el espejo del organigrama, así que no se sabe " +
                "a qué unidad ni a qué delegación pertenece. No se muestra nada: suponer que " +
                "alcanza todo convertiría una falla de la integración en un permiso.");

        return nivel switch
        {
            AlcanceDeDatos.Propio => AlcanceResuelto.SoloPropio(nivel),

            AlcanceDeDatos.Dependencia => AlcanceResuelto.PorUnidades(
                nivel, UnidadesAlcanzadas(puesto, espejo)),

            // Nulo en `Delegacion` es **sede**, no dato faltante (§3.1: los dos ejes coexisten).
            // Un puesto de sede con alcance de delegación no alcanza ninguna delegación, y eso
            // es una configuración incoherente que conviene ver, no rellenar.
            AlcanceDeDatos.Delegacion => suyo.Delegacion is null
                ? AlcanceResuelto.Nada(nivel,
                    $"El puesto {puesto} tiene alcance de delegación y no pertenece a ninguna: " +
                    "es un puesto de sede. La competencia está mal otorgada, y mostrar todo " +
                    "sería tapar el error con un permiso.")
                : AlcanceResuelto.PorDelegacion(nivel, suyo.Delegacion),

            _ => AlcanceResuelto.Nada(nivel, $"Nivel de alcance no contemplado: {nivel}."),
        };
    }

    /// <summary>
    /// Si un expediente entra en el alcance ya resuelto.
    ///
    /// La comparación de unidad y delegación es por texto porque eso es lo que hay: ni el
    /// expediente ni el espejo referencian un catálogo de unidades. <b>Es una debilidad
    /// conocida</b> —«Delegacion Choluteca» y «Delegacion de Choluteca» son dos cosas distintas
    /// para esta comparación y una sola en la realidad— y se anota en vez de disimularse con una
    /// normalización que adivinaría.
    /// </summary>
    public static bool Alcanza(AlcanceResuelto alcance, ExpedienteParaAlcance expediente)
    {
        if (!alcance.SePudoResolver) return false;

        return alcance.Nivel switch
        {
            AlcanceDeDatos.Institucion => true,

            // §3.1: autor, solicitante, motorista asignado o custodio.
            AlcanceDeDatos.Propio => expediente.EsSuyoDe(alcance.Persona),

            AlcanceDeDatos.Dependencia =>
                expediente.Dependencia is { Length: > 0 } d && alcance.Unidades.Contains(d),

            AlcanceDeDatos.Delegacion =>
                expediente.Delegacion is { Length: > 0 } g &&
                string.Equals(g, alcance.Delegacion, StringComparison.OrdinalIgnoreCase),

            _ => false,
        };
    }
}

/// <summary>
/// El alcance de un puesto, ya resuelto contra el espejo.
/// </summary>
/// <param name="SePudoResolver">
/// <b>Falso no es «no ve nada por permiso»</b>: es «no se pudo saber qué ve». Las dos cosas
/// muestran una lista vacía y sólo una es correcta; por eso viaja <see cref="PorQueNo"/>.
/// </param>
public sealed record AlcanceResuelto(
    AlcanceDeDatos Nivel,
    bool SePudoResolver,
    IReadOnlySet<string> Unidades,
    string? Delegacion,
    IdPersona Persona,
    string? PorQueNo)
{
    private static readonly IReadOnlySet<string> Ninguna = new HashSet<string>();

    public static AlcanceResuelto Todo(AlcanceDeDatos nivel) =>
        new(nivel, true, Ninguna, null, default, null);

    public static AlcanceResuelto SoloPropio(AlcanceDeDatos nivel) =>
        new(nivel, true, Ninguna, null, default, null);

    public static AlcanceResuelto PorUnidades(AlcanceDeDatos nivel, IReadOnlySet<string> unidades) =>
        new(nivel, true, unidades, null, default, null);

    public static AlcanceResuelto PorDelegacion(AlcanceDeDatos nivel, string delegacion) =>
        new(nivel, true, Ninguna, delegacion, default, null);

    public static AlcanceResuelto Nada(AlcanceDeDatos nivel, string porQueNo) =>
        new(nivel, false, Ninguna, null, default, porQueNo);

    /// <summary>Fija de quién es el alcance. Necesario para <c>Propio</c>.</summary>
    public AlcanceResuelto De(IdPersona persona) => this with { Persona = persona };
}

/// <summary>
/// Lo mínimo de un expediente que el alcance necesita mirar.
/// </summary>
/// <param name="Motoristas">
/// Quiénes conducen. <b>Hoy suele venir vacío</b>: el conductor de la reserva se guarda como
/// identificador de conductor y el alcance razona sobre identificadores de persona, y ese
/// puente no existe. Se declara vacío en vez de resolverse con una suposición — un motorista
/// que no se reconoce como suyo ve una lista de menos, que es el error seguro.
/// </param>
public sealed record ExpedienteParaAlcance(
    string Expediente,
    string? Dependencia,
    string? Delegacion,
    IdPersona CapturadaPor,
    IdPersona SolicitanteDeDerecho,
    IReadOnlyList<IdPersona> Motoristas,
    IReadOnlyList<IdPersona> Custodios)
{
    public bool EsSuyoDe(IdPersona persona) =>
        CapturadaPor == persona ||
        SolicitanteDeDerecho == persona ||
        Motoristas.Contains(persona) ||
        Custodios.Contains(persona);
}
