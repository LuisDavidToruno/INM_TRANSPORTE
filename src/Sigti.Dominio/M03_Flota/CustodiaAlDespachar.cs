using Sigti.Dominio.M01_Organizacion;

namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Lo que hace falta para juzgar la custodia al despachar — `BD-13` y la <b>custodia
/// vacante</b> de `RN-22`.
///
/// ── Por qué el organigrama entra acá ─────────────────────────────────────────
/// Porque tener custodia registrada y tener custodio <b>no son lo mismo</b>. `RN-22` nombra
/// el caso: <i>«custodio que cesa en el cargo dejando el vehículo asignado»</i>. La tarjeta de
/// responsabilidad sigue abierta —nadie la cerró, porque la persona ya no está para
/// firmarla— y `BD-13` la ve vigente. El vehículo se despacharía a nombre de alguien que ya
/// no trabaja ahí.
///
/// Es el mismo daño que `BD-13` existe para evitar, por otro camino: cuando aparezca el
/// golpe o la multa, <b>no hay a quién imputarla</b> porque la persona ya no está y nadie
/// recibió formalmente el bien.
///
/// ── Y por qué el organigrama y no un booleano precalculado ───────────────────
/// Porque <i>«la custodia cuyo custodio no ocupa ningún puesto está vacante»</i> es <b>la
/// regla</b>, y una regla evaluada afuera es una regla que no se puede probar sin las tres
/// capas. Lo que entra es el <b>dato</b> —quién ocupaba qué, y cuándo—; el juicio se hace acá.
/// </summary>
/// <param name="Organigrama">
/// El espejo de ARGOS. <b>Puede venir vacío</b>, y entonces nadie ocupa ningún puesto: ver
/// <c>OrdenDeMision.ExigirCustodiaVigente</c> para por qué eso <b>no</b> convierte a toda la
/// flota en custodia vacante.
/// </param>
public sealed record CustodiaAlDespachar(
    IReadOnlyList<CustodiaDelVehiculo> Historial,
    Organigrama Organigrama);
