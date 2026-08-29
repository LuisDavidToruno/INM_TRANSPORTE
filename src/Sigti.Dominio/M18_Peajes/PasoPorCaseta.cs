using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M18_Peajes;

/// <summary>Con qué se pagó. Cada uno deja una evidencia distinta.</summary>
public enum MedioDePagoDelPeaje
{
    Efectivo,

    Tarjeta,

    /// <summary>
    /// Tag CoviPass. `[C]` insumo #24 — <b>si la institución tiene tags y a nombre de quién</b>.
    /// El estimado no cambia por el medio; lo que cambia es la evidencia y quién descarga.
    /// </summary>
    Tag,

    /// <summary>
    /// Pasó sin pagar por exoneración vigente. <b>El paso se registra igual</b> — `RN-38` punto
    /// 5: el dato de ruta y tiempo se necesita para la coherencia de la secuencia.
    /// </summary>
    LibrePaso,
}

/// <summary>
/// Un paso por una caseta, tal como ocurrió — `RN-36`.
///
/// ── El cobro es un hecho; la clasificación es una derivación ────────────────
/// `RN-36` es taxativa: <i>«una discrepancia no debe modificar automáticamente la categoría
/// asignada al vehículo»</i>. Y explica por qué: <b>si el sistema ajustara la categoría al cobro
/// recibido, el error de la caseta se volvería la verdad institucional y el reclamo nunca
/// ocurriría</b>.
///
/// Entre agosto y septiembre de 2025 COVI-H reclasificó Hyundai H-100, Kia K2700 y Sprinter
/// cobrándoles <b>L 90 en lugar de L 22</b> — cuatro veces de más. La SAPP lo resolvió el
/// 17/09/2025 `[V]`. La flota típica de una institución pública hondureña cae exactamente en esa
/// zona gris, así que <b>es previsible que a un vehículo institucional le cobren mal</b>.
/// </summary>
/// <param name="CategoriaEsperada">
/// La del vehículo según `RN-33`. Nula cuando no se pudo derivar — y entonces no hay
/// discrepancia que declarar, porque no hay contra qué comparar.
/// </param>
/// <param name="CategoriaCobrada">
/// Con la que efectivamente cobró la caseta. Nula cuando el ticket no la dice, que pasa.
/// </param>
/// <param name="Ticket">
/// La fotografía. `RN-36` punto 2 la <b>exige cuando hay discrepancia y existe el ticket</b>: es
/// la evidencia del reclamo, y sin ella el expediente ante la SAPP es la palabra del motorista.
/// </param>
public sealed record PasoPorCaseta(
    Ulid Id,
    Ulid Punto,
    Ulid Vehiculo,
    Ulid? Mision,
    DateTimeOffset OcurridoEn,
    int Odometro,
    decimal MontoPagado,
    MedioDePagoDelPeaje Medio,
    IdPersona Registra,
    CategoriaDePeaje? CategoriaEsperada = null,
    CategoriaDePeaje? CategoriaCobrada = null,
    decimal? MontoEsperado = null,
    string? Ticket = null,
    bool PuntoNoCatalogado = false,
    string? UbicacionDeclarada = null)
{
    /// <summary>
    /// <b>Le cobraron con otra categoría.</b> Es la discrepancia de `RN-36`.
    ///
    /// Sólo cuando las dos categorías constan: sin la esperada no hay contra qué comparar, y sin
    /// la cobrada lo único que se sabe es el monto — que se juzga aparte.
    /// </summary>
    public bool HayDiscrepanciaDeClasificacion =>
        CategoriaEsperada is { } esperada &&
        CategoriaCobrada is { } cobrada &&
        !esperada.Es(cobrada.Codigo);

    /// <summary>
    /// Pagó distinto de lo esperado. <b>Puede no haber discrepancia de categoría</b>: la tarifa
    /// pudo cambiar entre la aprobación y el viaje, que `RN-34` tipifica como causa legítima.
    /// </summary>
    public decimal? Diferencia =>
        MontoEsperado is { } esperado ? MontoPagado - esperado : null;

    /// <summary>
    /// Pagó donde estaba exonerado — `RN-38` punto 3. <b>Pudo ser cobro indebido</b>, y habilita
    /// reclamo igual que la discrepancia de clasificación.
    ///
    /// Se calcula con la exoneración vigente <b>a la fecha del hecho</b>, no a la de hoy.
    /// </summary>
    public bool PagoEstandoExonerado(bool estabaExonerado) =>
        estabaExonerado && MontoPagado > 0m && Medio is not MedioDePagoDelPeaje.LibrePaso;
}

/// <summary>
/// Los controles del paso por caseta — `RN-36` y `RN-34`.
/// </summary>
public static class ReglasDelPasoPorCaseta
{
    /// <summary>
    /// Lo que todo paso exige, pase lo que pase.
    ///
    /// ── El odómetro, otra vez ───────────────────────────────────────────────
    /// Es lo que ancla el paso a un punto del recorrido, y es lo que permite el cruce de
    /// `RN-37`: <i>«un vehículo que declara 980 km pero solo cruzó una caseta dos veces está
    /// diciendo dos cosas incompatibles»</i>.
    /// </summary>
    public static void ExigirDatosDelHecho(int odometro, decimal montoPagado)
    {
        if (odometro <= 0)
            throw new BloqueoDuro("RN-36",
                "El paso por caseta exige el odómetro del momento. Sin él el paso no queda " +
                "anclado al recorrido, y el cruce contra la bitácora —que es lo que detecta un " +
                "odómetro manipulado— no se puede hacer.");

        if (montoPagado < 0)
            throw new BloqueoDuro("RN-36", "Un monto pagado negativo no describe ningún paso.");
    }

    /// <summary>
    /// `RN-36` punto 2 — <b>la discrepancia exige la fotografía del ticket cuando exista</b>.
    ///
    /// No se rechaza el paso por falta de ticket: la caseta a veces no lo da, y `RN-83` ya fijó
    /// el principio de que el registro de un hecho no se omite nunca por falta de papel. Lo que
    /// se exige es que la ausencia se <b>declare</b>, porque decide si el reclamo procede.
    /// </summary>
    public static void ExigirEvidenciaDeLaDiscrepancia(
        bool hayDiscrepancia, string? ticket, string? causaSinTicket)
    {
        if (!hayDiscrepancia) return;
        if (!string.IsNullOrWhiteSpace(ticket)) return;

        if (string.IsNullOrWhiteSpace(causaSinTicket))
            throw new BloqueoDuro("RN-36",
                "Le cobraron con una categoría distinta a la del vehículo y no hay ticket. El " +
                "paso se registra igual, pero la ausencia del ticket exige causa: es la " +
                "evidencia del reclamo ante la SAPP, y sin ella el expediente es la palabra " +
                "del motorista.");
    }

    /// <summary>
    /// `RN-34` — <b>un paso por un punto que no está en el catálogo no se descarta</b>.
    ///
    /// Se registra como punto no catalogado con ubicación y monto, marcado para depuración.
    /// `NRM-10` menciona casetas antiguas en San Pedro Sula sin verificar si operan `[C]`:
    /// descartar el paso perdería el gasto y la evidencia de que la caseta existe.
    /// </summary>
    public static void ExigirUbicacionSiNoEstaCatalogado(
        bool puntoNoCatalogado, string? ubicacion)
    {
        if (!puntoNoCatalogado) return;

        if (string.IsNullOrWhiteSpace(ubicacion))
            throw new BloqueoDuro("RN-34",
                "Un paso por un punto que no está en el catálogo exige la ubicación declarada. " +
                "Es lo que después permite depurar el catálogo: sin ella queda un gasto sin " +
                "caseta que nadie va a poder ubicar.");
    }
}
