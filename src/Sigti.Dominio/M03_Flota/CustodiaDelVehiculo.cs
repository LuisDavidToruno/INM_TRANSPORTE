using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M03_Flota;

/// <summary>
/// Quién responde patrimonialmente por el vehículo, y desde cuándo — `ACT-13`.
///
/// ── La pregunta que existe para contestar ────────────────────────────────────
/// `RN-22` la formula sin rodeos: la que aparece cuando algo falta o algo se daña es
/// <b>«¿quién tenía el vehículo en ese momento?»</b>. Sin cadena de custodia, la deducción
/// de responsabilidad no tiene sobre quién recaer, y el hallazgo del Tribunal Superior de
/// Cuentas queda <b>sin responsable identificado — lo que agrava, no atenúa</b>.
///
/// ── Por qué es un rango y no un campo en el vehículo ─────────────────────────
/// Porque `RN-22` exige el <b>historial completo</b>, consultable por rango de fechas: <i>«en
/// cualquier momento del pasado se puede decir quién respondía por la unidad»</i>. Una
/// columna `custodio_actual` contesta el presente y borra el pasado, que es justamente el
/// que pregunta la auditoría.
///
/// ── Custodia permanente, no la de la misión ──────────────────────────────────
/// `RN-22` distingue dos registros: <i>«la permanente no se interrumpe, la temporal se
/// superpone durante la misión y se extingue al retorno»</i>. Esto es la <b>permanente</b>.
/// La temporal —el traslado al motorista al despachar— <b>todavía no está construida</b>, y
/// mezclarlas en un solo registro haría imposible responder quién respondía por el bien
/// mientras estaba en ruta: la respuesta correcta son las dos personas, no una.
/// </summary>
/// <param name="Custodio">
/// Identidad de <b>persona</b>. La custodia es un rol adherido a un vehículo concreto y no a
/// la estructura organizativa — `ACT-13`: una misma persona puede ser custodia de tres
/// vehículos y de ninguna otra cosa.
/// </param>
/// <param name="Hasta">
/// Nulo es <b>vigente</b>, no eterno. Un `Hasta` obligatorio obligaría a inventar una fecha
/// de cese el día en que se firma la tarjeta de responsabilidad.
/// </param>
public sealed record CustodiaDelVehiculo(IdPersona Custodio, DateOnly Desde, DateOnly? Hasta)
{
    /// <summary>
    /// <b>Los dos extremos inclusivos.</b> El día en que se firma el acta ya hay custodio, y
    /// el día del cese todavía lo hay: la responsabilidad se traspasa con acta, no a las
    /// cero horas.
    /// </summary>
    public bool VigenteAl(DateOnly fecha) =>
        fecha >= Desde && (Hasta is null || fecha <= Hasta);
}
