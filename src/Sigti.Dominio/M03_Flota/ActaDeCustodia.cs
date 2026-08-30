using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Los dos extremos del traslado temporal de custodia — `RN-22`.
///
/// <b>Son dos actos distintos y del mismo peso.</b> Una entrega sin devolución deja el vehículo
/// en manos de alguien para siempre según el sistema, y una devolución sin entrega no tiene
/// contra qué compararse — que es lo único para lo que el acta sirve.
/// </summary>
public enum TipoDeActa
{
    /// <summary>El vehículo pasa del custodio al motorista. Ocurre al despachar.</summary>
    Entrega,

    /// <summary>Vuelve. Ocurre al retornar, y se compara con la entrega.</summary>
    Devolucion,
}

/// <summary>
/// Un elemento entregado con el vehículo — `RN-22`: <i>«accesorios y herramientas entregadas»</i>.
/// </summary>
/// <param name="Presente">
/// Si está. En la entrega, falso significa <b>que se entregó sin él</b> y queda dicho; en la
/// devolución, que <b>no volvió</b>.
///
/// ⚠️ No es lo mismo que no listarlo: un elemento ausente de la lista <b>nunca se miró</b>. El
/// gato que nadie anotó y el gato que no volvió son dos situaciones distintas.
/// </param>
public sealed record ElementoDeLaUnidad(string Nombre, bool Presente, string? Observacion);

/// <summary>
/// El acta de entrega-recepción del vehículo — `RN-22`, `NRM-02`.
/// </summary>
/// <param name="NivelDeTanque">
/// En fracción de tanque. <b>Nulo es que no se leyó</b>, no cero: cero es un tanque vacío, y
/// entregar un vehículo vacío es una afirmación distinta de no haber mirado el indicador.
/// </param>
public sealed record ActaDeCustodia(
    TipoDeActa Tipo,
    IdPersona Entrega,
    IdPersona Recibe,
    DateTimeOffset Momento,
    int Odometro,
    decimal? NivelDeTanque,
    string EstadoDeLaUnidad,
    IReadOnlyList<ElementoDeLaUnidad> Elementos,
    string? Observaciones);

/// <param name="NoVolvieron">
/// Lo que se entregó y no volvió. <b>Es el hallazgo</b>, y va con nombre: «faltan 2 elementos»
/// no le sirve a nadie que tenga que deducir responsabilidad.
/// </param>
/// <param name="NoSeEntregaron">
/// Lo que aparece en la devolución y no estaba en la entrega. Suele ser un elemento que se
/// olvidó anotar al salir — y a veces es uno que el motorista repuso de su bolsillo, que es
/// información distinta y también hay que poder verla.
/// </param>
/// <param name="KilometrosRecorridos">
/// <b>Nulo cuando el odómetro de la devolución es menor que el de la entrega.</b> No es cero:
/// es que el odómetro se reinició, se sustituyó, o alguien tecleó mal — y las tres exigen
/// mirarlo, no promediarlo.
/// </param>
public sealed record CotejoDeLaDevolucion(
    IReadOnlyList<string> NoVolvieron,
    IReadOnlyList<string> NoSeEntregaron,
    int? KilometrosRecorridos,
    decimal? DiferenciaDeTanque,
    string Veredicto);

/// <summary>
/// `RN-22` — el <b>traslado temporal de custodia</b> al motorista, con acta en los dos extremos.
///
/// ── La pregunta que esto contesta ───────────────────────────────────────────
/// <i>«¿Quién tenía el vehículo en ese momento, y con qué?»</i> Es la que aparece cuando algo
/// falta o algo se daña, y sin cadena de custodia <b>la deducción de responsabilidad no tiene
/// sobre quién recaer</b> — lo que ante el TSC agrava en vez de atenuar.
///
/// ── Y por qué el acta sirve sólo si se cotejan las dos ──────────────────────
/// Un acta de entrega con cinco elementos y una devolución con cuatro son, por separado, dos
/// listas que nadie lee. <b>El cotejo es el producto</b>: el gato que no volvió tiene nombre,
/// fecha y dos personas.
/// </summary>
public static class ReglasDelActaDeCustodia
{
    /// <summary>
    /// Por qué no se puede registrar esta acta. <b>Nulo es que sí.</b>
    /// </summary>
    /// <param name="hayEntregaPrevia">
    /// Si ya se registró la entrega de esta misión. <b>Una devolución sin entrega no tiene
    /// contra qué compararse</b>, y comparar es lo único para lo que el acta sirve.
    /// </param>
    public static string? PorQueNoSeRegistra(
        TipoDeActa tipo, bool hayEntregaPrevia, bool yaHayDeLaMismaClase, string estado)
    {
        if (yaHayDeLaMismaClase)
        {
            return tipo == TipoDeActa.Entrega
                ? "Esta misión ya tiene acta de entrega. Registrar dos dejaría dos inventarios " +
                  "distintos del mismo vehículo, y el cotejo del retorno se quedaría sin saber " +
                  "contra cuál correr."
                : "Esta misión ya tiene acta de devolución. El vehículo volvió una vez.";
        }

        if (tipo == TipoDeActa.Devolucion && !hayEntregaPrevia)
        {
            return "No hay acta de entrega para esta misión. Una devolución sin entrega no " +
                   "tiene contra qué compararse, y comparar es lo único para lo que el acta " +
                   "sirve: sin la de salida, nadie puede decir qué faltó.";
        }

        return string.IsNullOrWhiteSpace(estado)
            ? "Declare el estado en que se entrega la unidad. Es lo que después distingue un " +
              "golpe que ya venía de uno que ocurrió en la misión."
            : null;
    }

    /// <summary>
    /// Coteja la devolución contra la entrega. <b>Es el producto del acta.</b>
    /// </summary>
    public static CotejoDeLaDevolucion Cotejar(ActaDeCustodia entrega, ActaDeCustodia devolucion)
    {
        // Se compara por nombre y sin importar la caja: quien llena el acta del retorno escribe
        // «Gato hidráulico» donde la de salida decía «gato hidraulico», y un cotejo que los
        // tratara como dos elementos produciría un faltante y un agregado inventados.
        var entregados = entrega.Elementos
            .Where(e => e.Presente)
            .Select(e => e.Nombre.Trim())
            .ToList();

        var devueltos = devolucion.Elementos
            .Where(e => e.Presente)
            .Select(e => e.Nombre.Trim())
            .ToList();

        var noVolvieron = entregados
            .Where(n => !devueltos.Contains(n, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var noSeEntregaron = devueltos
            .Where(n => !entregados.Contains(n, StringComparer.OrdinalIgnoreCase))
            .ToList();

        // ⚠️ **Nulo, no cero.** Un odómetro que retrocede no significa que la misión recorrió
        // cero kilómetros: significa que el odómetro se reinició, se sustituyó, o alguien
        // tecleó mal. Las tres exigen mirarlo.
        int? kilometros = devolucion.Odometro >= entrega.Odometro
            ? devolucion.Odometro - entrega.Odometro
            : null;

        var tanque = entrega.NivelDeTanque is { } a && devolucion.NivelDeTanque is { } b
            ? b - a
            : (decimal?)null;

        return new CotejoDeLaDevolucion(
            noVolvieron, noSeEntregaron, kilometros, tanque,
            Veredicto(noVolvieron, noSeEntregaron, kilometros));
    }

    private static string Veredicto(
        IReadOnlyList<string> noVolvieron, IReadOnlyList<string> noSeEntregaron, int? kilometros)
    {
        if (noVolvieron.Count > 0)
        {
            return $"⚠️ No volvió: {string.Join(", ", noVolvieron)}. Es hallazgo de custodia " +
                   "(RN-22): el vehículo estuvo bajo responsabilidad de quien lo recibió, y la " +
                   "deducción recae sobre esa persona salvo que se documente lo contrario.";
        }

        var extra = noSeEntregaron.Count > 0
            ? $" Aparecen sin constar en la entrega: {string.Join(", ", noSeEntregaron)} — " +
              "puede que se omitieran al salir, o que se repusieran en ruta."
            : "";

        return kilometros is null
            ? "Los elementos volvieron completos. ⚠️ El odómetro del retorno es menor que el de " +
              "salida: no se puede calcular el recorrido, y eso exige revisarlo." + extra
            : $"Los elementos volvieron completos. Recorrido: {kilometros:N0} km." + extra;
    }
}
