using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M17_PersonasExternas;

/// <summary>
/// Cada acceso a datos de personas trasladadas — `RN-52`, `PT-133`.
///
/// ── Es la única respuesta posible a un hábeas data ──────────────────────────
/// El hábeas data del Artículo 182 está vigente `[V]` y sólo el titular puede interponerlo.
/// <i>«Si una persona pregunta quién accedió a sus datos, la única respuesta defendible es el
/// registro de consultas. <b>Sin él, la institución no puede afirmar nada.</b>»</i>
///
/// No poder afirmar nada no significa quedar en empate: significa que la institución no puede
/// demostrar que <b>no</b> hubo acceso indebido.
///
/// ── Nadie está exento, y ése es el punto ────────────────────────────────────
/// `RN-52`: <i>«Ningún rol, <b>incluido ACT-01 Administrador del Sistema</b>, debe poder
/// consultar estos datos sin dejar rastro.»</i> El administrador es justamente quien podría
/// borrar su propio rastro, y por eso el registro es inmutable y lo lee `ACT-12`.
/// </summary>
/// <param name="Alcance">
/// <b>Qué se mostró</b>, no sólo qué se abrió. Ver una lista de nombres y ver un manifiesto
/// completo son dos accesos distintos al mismo registro, y el titular tiene derecho a saber
/// cuál de los dos ocurrió.
/// </param>
public sealed record ConsultaRegistrada(
    Ulid Id,
    IdPersona Consultante,
    string Rol,
    DateTimeOffset Momento,
    string RegistroConsultado,
    AlcanceDeLaConsulta Alcance,

    /// <summary>
    /// Por qué necesitaba verlo. `RN-52` restringe por <b>necesidad de conocer</b>, y sin este
    /// dato la restricción no se puede auditar: quedaría el rastro de quién miró y ninguna
    /// forma de juzgar si debía.
    /// </summary>
    string? NecesidadDeConocer,

    string? Origen);

public enum AlcanceDeLaConsulta
{
    /// <summary>Sólo cuántas personas van. <b>Sin ningún dato personal.</b></summary>
    SoloRecuento,

    /// <summary>La lista de nombres, para el control en carretera.</summary>
    ListaDeNombres,

    /// <summary>El manifiesto entero, con todos los campos activos.</summary>
    ManifiestoCompleto,
}

/// <summary>
/// Cuándo se puede mirar, y qué queda registrado — `RN-52`.
/// </summary>
public static class ReglasDeLaConsulta
{
    /// <summary>
    /// Exige el motivo de la consulta.
    ///
    /// ── Por qué se pide, si igual se va a mostrar ───────────────────────────
    /// No es una barrera: es el dato que vuelve auditable el acceso. Un registro que dice
    /// <i>«P-DESPACHO vio el manifiesto el martes»</i> no permite juzgar nada; uno que dice
    /// <i>«…para el despacho de la misión OM-451»</i> sí — y la diferencia aparece el día que
    /// alguien mire cien manifiestos que no le tocaban.
    ///
    /// Escribirlo también hace pensar. Quien no puede completar la frase suele no necesitar el
    /// dato.
    /// </summary>
    public static void ExigirNecesidadDeConocer(AlcanceDeLaConsulta alcance, string? necesidad)
    {
        // El recuento no lleva datos personales: cuántas personas van es dato de gestión, y
        // pedir justificación para verlo convertiría el control en un trámite que la gente
        // aprende a saltarse escribiendo cualquier cosa.
        if (alcance == AlcanceDeLaConsulta.SoloRecuento) return;

        if (string.IsNullOrWhiteSpace(necesidad) || necesidad.Trim().Length < 8)
            throw new BloqueoDuro("RN-52",
                "Diga para qué necesita ver estos datos. Queda registrado con su nombre, y el " +
                "titular puede pedirlo por hábeas data.");
    }

    /// <summary>
    /// Los accesos de una persona que <b>merecen una segunda mirada</b> — `PT-133`.
    ///
    /// ── Qué es un patrón anómalo, y qué no ──────────────────────────────────
    /// Un despachador que abre veinte manifiestos en la mañana de un lunes está trabajando. Uno
    /// que abre veinte <b>de misiones que no despachó</b> es otra cosa.
    ///
    /// Acá se cuenta lo segundo: consultas a registros con los que el consultante no tiene
    /// relación operativa declarada. <b>No es una acusación</b> — es lo que un reporte de
    /// control interno pone delante de alguien para que pregunte.
    /// </summary>
    public static IReadOnlyList<PatronDeAcceso> Patrones(
        IEnumerable<ConsultaRegistrada> consultas, DateTimeOffset desde, int umbral)
    {
        return
        [
            .. consultas
                .Where(c => c.Momento >= desde)
                .Where(c => c.Alcance != AlcanceDeLaConsulta.SoloRecuento)
                .GroupBy(c => c.Consultante)
                .Select(g => new PatronDeAcceso(
                    g.Key,
                    g.Count(),
                    g.Select(c => c.RegistroConsultado).Distinct().Count(),

                    // Sin necesidad declarada no se puede juzgar el acceso, y por eso se cuenta
                    // aparte: es la cifra que dice cuánto del registro es inauditable.
                    g.Count(c => string.IsNullOrWhiteSpace(c.NecesidadDeConocer)),
                    g.Count() >= umbral))
                .OrderByDescending(p => p.Consultas),
        ];
    }
}

/// <param name="Marcado">
/// Si superó el umbral. <b>No significa que hizo algo malo</b>: significa que alguien debería
/// preguntar. Un reporte que acusa se deja de leer tan rápido como uno que calla.
/// </param>
public sealed record PatronDeAcceso(
    IdPersona Consultante,
    int Consultas,
    int RegistrosDistintos,
    int SinNecesidadDeclarada,
    bool Marcado);
