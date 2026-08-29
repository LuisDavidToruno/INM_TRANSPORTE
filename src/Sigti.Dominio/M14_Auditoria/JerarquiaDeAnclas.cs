using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Dominio.M14_Auditoria;

/// <summary>
/// Con qué se identifica un vehículo desde afuera — `RN-66`, en su orden inicial.
///
/// ── El orden no es arbitrario y la placa va última ──────────────────────────
/// `RN-66` la pone <b>en último lugar</b> y resuelta a la fecha del hecho. La razón es de acá:
/// hay desabastecimiento nacional de placas, la placa se reasigna, y un vehículo puede haber
/// llevado otra —o ninguna— el día del hecho. Resolver por placa primero atribuiría la multa
/// del año pasado al vehículo que hoy tiene esa chapa.
///
/// ⚠️ <b>Es el orden inicial, y `RN-66` lo declara configurable.</b> Acá está fijo porque la
/// institución no ha declarado el suyo (`[C]`), y se dice en vez de prometer una configuración
/// que no existe.
/// </summary>
public enum AnclaDeVehiculo
{
    /// <summary>Número de bien del inventario nacional. El más estable de todos.</summary>
    BienDelInventario,

    Chasis,

    Motor,

    CorrelativoInstitucional,

    /// <summary>
    /// <b>Última, y a la fecha del hecho.</b> Es la que más cambia y la que más se reasigna.
    /// </summary>
    Placa,
}

/// <summary>
/// Cómo viene identificado el vehículo en una línea de fuente externa.
///
/// Los cinco son anulables porque <b>ninguna fuente los trae todos</b>: un estado de cuenta de
/// peaje trae la placa, una notificación de infracción trae placa y a veces chasis, y un acta de
/// autoridad puede traer el número de bien. Exigirlos todos dejaría fuera a todas las fuentes.
/// </summary>
public sealed record IdentificacionExterna(
    string? BienDelInventario = null,
    string? Chasis = null,
    string? Motor = null,
    string? CorrelativoInstitucional = null,
    string? Placa = null)
{
    public bool NoTraeNada =>
        string.IsNullOrWhiteSpace(BienDelInventario) &&
        string.IsNullOrWhiteSpace(Chasis) &&
        string.IsNullOrWhiteSpace(Motor) &&
        string.IsNullOrWhiteSpace(CorrelativoInstitucional) &&
        string.IsNullOrWhiteSpace(Placa);
}

/// <summary>
/// Un vehículo de la flota, reducido a sus anclas. Se arma en la capa de aplicación.
/// </summary>
/// <param name="Placa">
/// <b>Nula es un estado válido</b> — `RN-15`: hay desabastecimiento nacional de placas y un
/// campo obligatorio y único rompería el sistema. Un vehículo sin placa nunca se resuelve por
/// placa, y eso no es un defecto: es el dato.
/// </param>
public sealed record AnclasDelVehiculo(
    Ulid Id,
    string Siglas,
    string? BienDelInventario = null,
    string? Chasis = null,
    string? Motor = null,
    string? CorrelativoInstitucional = null,
    string? Placa = null);

/// <summary>
/// A qué vehículo se resolvió una imputación externa, y por cuál ancla.
/// </summary>
/// <param name="Vehiculo">
/// <b>Nulo cuando no se resolvió.</b> `RN-66`: <i>«una imputación que no se resuelve no se
/// asigna por parecido: queda no resuelta, con responsable de seguimiento y plazo»</i>.
/// </param>
/// <param name="Ancla">
/// Cuál ancla la resolvió. Va al expediente porque no es lo mismo haber resuelto por número de
/// bien que por placa: la segunda admite discusión y la primera no.
/// </param>
public sealed record VehiculoResuelto(
    Ulid? Vehiculo,
    AnclaDeVehiculo? Ancla,
    string Explicacion)
{
    public bool EstaResuelto => Vehiculo is not null;
}

/// <summary>
/// La jerarquía de anclas de `RN-66` — el mínimo que `RN-95` necesita.
///
/// ── Lo que impide ───────────────────────────────────────────────────────────
/// Que una multa se le cargue al vehículo equivocado por parecido de placa. La placa se
/// reasigna y hay vehículos circulando sin ella (`RN-15`); resolver por placa primero es
/// exactamente cómo se atribuye mal una imputación que después cuesta explicar.
///
/// ⚠️ <b>Es el mínimo, no `RN-66` completo.</b> La regla además atribuye al <b>tenedor vigente
/// a la fecha del hecho</b> cuando el vehículo estaba prestado (`RN-63`) y al <b>conductor
/// registrado</b> de esa fecha y hora (`RN-57`). Eso necesita el expediente de préstamo y la
/// jornada declarada, que no existen — y se dice en vez de fingir que la atribución está
/// completa.
/// </summary>
public static class ReglasDeImputacionExterna
{
    /// <summary>
    /// Resuelve el vehículo probando las anclas <b>en orden</b> y parando en la primera que
    /// acierte.
    ///
    /// ── Por qué para en la primera y no busca la mejor ──────────────────────
    /// Porque el orden <b>es</b> el criterio de confianza. Si el número de bien resuelve, la
    /// placa no tiene nada que aportar; y si dos anclas apuntaran a vehículos distintos, eso es
    /// un dato de flota corrupto y no una decisión que esta función deba tomar.
    /// </summary>
    public static VehiculoResuelto Resolver(
        IdentificacionExterna identificacion,
        IReadOnlyList<AnclasDelVehiculo> flota)
    {
        if (identificacion.NoTraeNada)
            return new VehiculoResuelto(null, null,
                "La línea no trae ninguna identificación de vehículo. No se puede resolver, y " +
                "no se asigna por parecido.");

        foreach (var ancla in Enum.GetValues<AnclaDeVehiculo>())
        {
            var buscado = Valor(identificacion, ancla);
            if (string.IsNullOrWhiteSpace(buscado)) continue;

            var candidatos = flota
                .Where(v => Coincide(Valor(v, ancla), buscado))
                .ToList();

            if (candidatos.Count == 1)
                return new VehiculoResuelto(candidatos[0].Id, ancla,
                    $"Resuelto por {Nombre(ancla)} «{buscado.Trim()}» → {candidatos[0].Siglas}." +
                    (ancla is AnclaDeVehiculo.Placa
                        ? " ⚠️ Resuelto por PLACA, que es la última de la jerarquía: la placa se " +
                          "reasigna y hay vehículos sin ella. Conviene confirmarlo contra el " +
                          "historial de la placa a la fecha del hecho (`RN-64`)."
                        : ""));

            // Dos vehículos con el mismo número de bien o el mismo chasis es un dato de flota
            // corrupto. Elegir uno sería inventar la respuesta; se dice cuál es el problema.
            if (candidatos.Count > 1)
                return new VehiculoResuelto(null, null,
                    $"{candidatos.Count} vehículos de la flota comparten {Nombre(ancla)} " +
                    $"«{buscado.Trim()}»: {string.Join(", ", candidatos.Select(c => c.Siglas))}. " +
                    "No se elige uno por parecido — lo que hay que corregir es el padrón.");
        }

        return new VehiculoResuelto(null, null,
            "Ninguna de las anclas que trae la línea corresponde a un vehículo de la flota. " +
            "Puede ser un error del proveedor y puede no serlo: queda no resuelto, con " +
            "responsable y plazo (`RN-66`).");
    }

    private static string? Valor(IdentificacionExterna i, AnclaDeVehiculo ancla) => ancla switch
    {
        AnclaDeVehiculo.BienDelInventario => i.BienDelInventario,
        AnclaDeVehiculo.Chasis => i.Chasis,
        AnclaDeVehiculo.Motor => i.Motor,
        AnclaDeVehiculo.CorrelativoInstitucional => i.CorrelativoInstitucional,
        AnclaDeVehiculo.Placa => i.Placa,
        _ => null,
    };

    private static string? Valor(AnclasDelVehiculo v, AnclaDeVehiculo ancla) => ancla switch
    {
        AnclaDeVehiculo.BienDelInventario => v.BienDelInventario,
        AnclaDeVehiculo.Chasis => v.Chasis,
        AnclaDeVehiculo.Motor => v.Motor,
        AnclaDeVehiculo.CorrelativoInstitucional => v.CorrelativoInstitucional,
        AnclaDeVehiculo.Placa => v.Placa,
        _ => null,
    };

    /// <summary>
    /// Se compara sin importar la caja ni los espacios. <b>No se normaliza más que eso</b>: quitar
    /// guiones para que «HAB-1234» case con «HAB1234» empezaría a resolver por parecido, que es
    /// justo lo que la regla prohíbe.
    /// </summary>
    private static bool Coincide(string? deLaFlota, string deLaFuente) =>
        !string.IsNullOrWhiteSpace(deLaFlota) &&
        string.Equals(deLaFlota.Trim(), deLaFuente.Trim(), StringComparison.OrdinalIgnoreCase);

    private static string Nombre(AnclaDeVehiculo ancla) => ancla switch
    {
        AnclaDeVehiculo.BienDelInventario => "número de bien del inventario",
        AnclaDeVehiculo.Chasis => "chasis",
        AnclaDeVehiculo.Motor => "número de motor",
        AnclaDeVehiculo.CorrelativoInstitucional => "correlativo institucional",
        AnclaDeVehiculo.Placa => "placa",
        _ => ancla.ToString(),
    };

    /// <summary>
    /// `RN-66` — lo no resuelto exige <b>responsable y plazo</b>. Sin ellos, «no resuelto» es un
    /// montón que crece y que nadie revisa.
    /// </summary>
    public static void ExigirResponsableYPlazoDeLoNoResuelto(
        string? responsable, DateOnly? plazo)
    {
        if (string.IsNullOrWhiteSpace(responsable) || plazo is null)
            throw new BloqueoDuro("RN-66",
                "Una imputación externa no resuelta exige responsable de seguimiento y plazo. " +
                "Sin ellos, «no resuelto» se vuelve un montón que crece y que nadie revisa — y " +
                "el auditor lo va a encontrar antes que nosotros.");
    }
}
