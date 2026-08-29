using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Reglas;

namespace Sigti.Dominio.M18_Peajes;

/// <summary>
/// Una categoría de peaje.
///
/// ── Por qué es un código y NO un enum ───────────────────────────────────────
/// `RN-33` lo exige sin margen: <i>«tabla abierta, capaz de admitir "Liviano/Turismo",
/// "Vehículo de N Ejes" hasta 9, montacargas y categorías futuras <b>sin cambio de
/// código</b>»</i>. Un enumerado de 2 a 9 ejes deja fuera dos categorías que la SAPP ya publicó
/// `[V]`, y la próxima resolución agregaría otra.
///
/// Es la excepción deliberada al criterio que usa `FuenteDeAbastecimiento` en `M-09`: allá el
/// comportamiento cambia por valor, así que un valor nuevo no sabría a qué grupo pertenece. Acá
/// <b>ninguna categoría se comporta distinto</b>: todas son una llave contra la tabla de
/// tarifas.
/// </summary>
public sealed record CategoriaDePeaje(string Codigo, string Nombre)
{
    public override string ToString() => Nombre;

    /// <summary>
    /// Las categorías se comparan por código y sin importar la caja de las letras: la tabla la
    /// carga una persona, y «LIVIANO» y «Liviano» son la misma.
    /// </summary>
    public bool Es(string codigo) =>
        string.Equals(Codigo, codigo, StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Si el punto cobra hoy. <b>Va con vigencia</b> — `NRM-10`: <i>«sin el estado con vigencia no
/// se puede recalcular un viaje pasado por una caseta que ya no existe»</i>.
/// </summary>
public enum EstadoDelPunto
{
    Activo,

    /// <summary>Cobro suspendido por resolución. Vuelve.</summary>
    Suspendido,

    /// <summary>
    /// Dejó de cobrar — Canal Seco, Corredor Turístico en terminación anticipada `[V]`.
    /// <b>No es exoneración del vehículo</b>: es estado del punto. Confundirlos haría que al
    /// reactivarse el cobro el sistema siguiera estimando cero.
    /// </summary>
    Cerrado,
}

/// <summary>
/// Un punto de peaje del país.
///
/// ⚠️ <b>Catálogo ampliable en producción, sin cambio de código</b> — `RN-34` punto 5:
/// `NRM-10` advierte que hay proyectos en cartera.
/// </summary>
/// <param name="SentidoDeCobro">
/// Nulo cuando el punto cobra en ambos sentidos. <b>Nulo no es «no se sabe»</b> acá: es la
/// condición normal, y los puntos que sólo cobran en un sentido la declaran. Importa para
/// contar cruces: un punto de sentido único no se cruza dos veces en un viaje de ida y vuelta.
/// </param>
/// <param name="Corredor">
/// A qué corredor pertenece. <b>Es lo que hace comparable el kilómetro</b>: el km 60 de la
/// CA-5 y el km 60 de la CA-1 no están cerca. Nulo deja la dimensión geográfica de `RN-37`
/// sin evaluar en vez de comparar kilómetros de carreteras distintas.
/// </param>
/// <param name="Kilometro">
/// El kilómetro del punto dentro de su corredor — `RN-37` punto 1: <i>«el catálogo de puntos
/// incluye ubicación, corredor, kilómetro y sentido de cobro, <b>lo que permite ordenar
/// geográficamente</b>»</i>.
///
/// ⚠️ Nulo cuando no se ha cargado, y entonces el orden geográfico <b>no se deduce del orden
/// de captura</b>: eso invertiría la respuesta en toda misión de retorno.
/// </param>
public sealed record PuntoDePeaje(
    Ulid Id,
    string Nombre,
    string Operador,
    string Carretera,
    string? SentidoDeCobro = null,
    string? Corredor = null,
    int? Kilometro = null)
{
    public bool CobraEnAmbosSentidos => SentidoDeCobro is null;
}

/// <summary>Una vigencia del estado operativo del punto.</summary>
public sealed record VigenciaDelPunto(
    Ulid Punto,
    EstadoDelPunto Estado,
    string Fundamento,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    DateTimeOffset RegistradoDesde,
    DateTimeOffset? RegistradoHasta = null) : IConVigencia;

/// <summary>
/// Una fila de la tabla de tarifas — `RN-34`: <b>punto × categoría × vigencia</b>.
///
/// ── Nunca una fórmula ───────────────────────────────────────────────────────
/// La progresión de 2 a 9 ejes es casi lineal (~L 45 por eje) `[I]`, y por eso alguien va a
/// proponer calcularla. `NRM-10` lo prohíbe expresamente: <i>«una fórmula inferida se vuelve
/// falsa al primer ajuste asimétrico»</i>. Es una tabla publicada, y se carga.
/// </summary>
/// <param name="Fuente">
/// SAPP, COVI-H, contrato, comunicado de la SIT. <b>Una tarifa sin fuente no se guarda</b>
/// (`RN-34` punto 3): la tarifa que ve el usuario es política, no contractual, y sin saber
/// quién la publicó no se puede defender un cobro ante nadie.
/// </param>
/// <param name="FechaDeVerificacion">
/// Cuándo se confirmó contra la fuente. `RN-34` manda alertar a los 12 meses — la tarifa cambia
/// al menos una vez al año, en enero, con alta probabilidad de reversión a mitad de proceso.
/// </param>
public sealed record TarifaDePeaje(
    Ulid Id,
    Ulid Punto,
    string Categoria,
    decimal Monto,
    string Fuente,
    DateOnly FechaDeVerificacion,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    DateTimeOffset RegistradoDesde,
    DateTimeOffset? RegistradoHasta = null) : IConVigencia
{
    /// <summary>
    /// `RN-34`: alerta a los 12 meses sin revisar. <b>No invalida la tarifa</b> — una tarifa
    /// vieja sigue siendo la mejor información que hay, y bloquear por antigüedad detendría la
    /// operación por no haber hecho una gestión administrativa.
    /// </summary>
    public bool SinRevisarHaceMasDeUnAnio(DateOnly hoy) =>
        FechaDeVerificacion.AddYears(1) < hoy;
}

/// <summary>
/// Cómo se resuelve una tarifa — `RN-34`.
/// </summary>
public static class ReglasDeTarifaDePeaje
{
    /// <summary>
    /// El monto de un paso. <b>Nunca una constante ni una fórmula por ejes.</b>
    ///
    /// ── Ausencia de tarifa NO produce cero ──────────────────────────────────
    /// `RN-34`: <i>«si no existe tarifa vigente para esa combinación en esa fecha, el sistema no
    /// debe calcular un valor por defecto»</i>. Un cero indistinguible de un error es peor que
    /// la ausencia declarada, así que esto devuelve <b>nulo</b> y quien llama decide si eso es
    /// bloqueo o una línea marcada como no disponible.
    /// </summary>
    /// <param name="conocidoAl">
    /// Desde qué momento se mira (`ADR-006`). El instante de la liquidación reproduce el número
    /// que se pagó; el instante actual da el corregido. Son dos preguntas legítimas.
    /// </param>
    public static TarifaDePeaje? Resolver(
        IEnumerable<TarifaDePeaje> tabla,
        Ulid punto,
        CategoriaDePeaje categoria,
        DateOnly fechaDelHecho,
        DateTimeOffset conocidoAl) =>
        ReglasDeVigencia.VigenteA(
            tabla.Where(t =>
                t.Punto == punto &&
                string.Equals(t.Categoria, categoria.Codigo, StringComparison.OrdinalIgnoreCase)),
            fechaDelHecho,
            conocidoAl);

    /// <summary>
    /// El estado del punto a la fecha del hecho. <b>Sin vigencia declarada no se supone
    /// activo</b>: se devuelve nulo, y la estimación lo dice. Suponer que cobra produciría un
    /// estimado de más sobre una caseta que quizá ya cerró; suponer que no, uno de menos y un
    /// faltante de efectivo en ruta.
    /// </summary>
    public static VigenciaDelPunto? EstadoA(
        IEnumerable<VigenciaDelPunto> vigencias,
        Ulid punto,
        DateOnly fechaDelHecho,
        DateTimeOffset conocidoAl) =>
        ReglasDeVigencia.VigenteA(
            vigencias.Where(v => v.Punto == punto), fechaDelHecho, conocidoAl);

    /// <summary>
    /// `RN-34` punto 3 — <b>una tarifa sin fuente no se guarda.</b>
    ///
    /// La instrucción de `NRM-10` es no cargar ninguna tarifa hasta confirmarla con COVI-H o la
    /// SAPP, porque hay contradicción abierta entre el comunicado de la SIT y un agregador
    /// comercial. Exigir la fuente es lo que hace que esa contradicción sea visible en la tabla
    /// en vez de disolverse en un número.
    /// </summary>
    public static void ExigirFuenteYVerificacion(string fuente, decimal monto)
    {
        if (string.IsNullOrWhiteSpace(fuente))
            throw new BloqueoDuro("RN-34",
                "Una tarifa sin fuente no se guarda. La tarifa de peaje es política y no " +
                "contractual: sin saber quién la publicó —SAPP, COVI-H, contrato, comunicado de " +
                "la SIT— no se puede defender un cobro ante nadie.");

        if (monto < 0)
            throw new BloqueoDuro("RN-34", "Una tarifa negativa no describe ningún cobro.");
    }
}
