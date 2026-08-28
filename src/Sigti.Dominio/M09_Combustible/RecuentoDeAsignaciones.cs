namespace Sigti.Dominio.M09_Combustible;

/// <summary>
/// El estado del combustible de una misión, resumido para que la Orden de Misión pueda
/// decidir sin cargar los vales.
///
/// ── Por qué se pasa ya calculado ────────────────────────────────────────────
/// `ADR-009`: la regla es pura y recibe los datos ya traídos. Si `OrdenDeMision` fuera a
/// buscar sus asignaciones, el dominio tendría que conocer el repositorio — y las
/// precondiciones dejarían de poder probarse sin montar tres capas.
///
/// ── Los cuatro números no son intercambiables ───────────────────────────────
/// Cada uno contesta una precondición distinta de §10.1, y usar uno por otro rompe el
/// control sin romper ninguna prueba obvia:
///
/// | Número | Qué decide |
/// |---|---|
/// | <c>SinLiquidar</c> | `T-19` — `INV-34`, todas las asignaciones `LIQUIDADAS` |
/// | <c>SinConciliar</c> | `T-21`/`T-22` — todas conciliadas, en cualquiera de las dos formas |
/// | <c>ConConsumo</c> | `T-15` contra `T-16` — <b>si hubo un solo consumo, no hay anulación</b> |
/// | <c>EntregadasSinDevolver</c> | `T-15` — hay vales en la calle que todavía no volvieron |
/// </summary>
/// <param name="Total">
/// Cuántas asignaciones tiene la misión. <b>Cero es un dato, no un vacío</b>: una misión sin
/// combustible asignado es normal —el vehículo salió con el tanque lleno— y sus precondiciones
/// se cumplen por vacuidad, no por omisión.
/// </param>
public sealed record RecuentoDeAsignaciones(
    int Total,
    int SinLiquidar,
    int SinConciliar,
    int ConConsumo,
    int EntregadasSinDevolver)
{
    /// <summary>La misión no movió combustible. Todo lo demás se cumple sin nada que revisar.</summary>
    public static readonly RecuentoDeAsignaciones Ninguna = new(0, 0, 0, 0, 0);

    /// <summary>
    /// <b>Hubo movimiento de dinero público.</b> Es lo que separa `T-15` de `T-16`: anular
    /// sería borrar un hecho económico.
    /// </summary>
    public bool HuboConsumo => ConConsumo > 0;
}
