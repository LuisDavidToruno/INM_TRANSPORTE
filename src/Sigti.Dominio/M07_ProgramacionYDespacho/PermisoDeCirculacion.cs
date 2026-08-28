using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// El permiso de la máxima autoridad para circular en día inhábil — `ACT-09`, `RN-23`.
///
/// ── Qué ampara, y por qué las cuatro cosas ───────────────────────────────────
/// <b>Vehículo, motorista, ruta y ventana.</b> Los cuatro, que es la lectura más exigente de
/// las tres que convivían — el hallazgo `HB3-07` las encontró contradiciéndose entre
/// `BD-04`, `PC-03` y `RN-23`, y se resolvió a favor de ésta.
///
/// La razón no es formal: <b>el salvoconducto lo lee un agente en carretera que compara el
/// nombre del papel con quien va al volante</b>. Si no coinciden, el documento no sirve para
/// lo único que existe. Un permiso que ampara a cualquiera que conduzca no es un permiso
/// nominativo.
///
/// ── Por eso un relevo de motorista lo invalida ───────────────────────────────
/// Y obliga a reemitirlo para el tramo restante. Sale solo de que el motorista sea parte de
/// lo amparado: no hay una regla aparte que recordar.
///
/// Una versión anterior de la corrección adoptó la lectura contraria por un argumento
/// operativo —que un motorista incapacitado un domingo dejaría el vehículo varado esperando
/// otra firma—. <b>Ese argumento era erróneo y la salida ya existía</b>: el código de
/// autorización fuera de línea permite que la máxima autoridad autorice por teléfono con un
/// código que el motorista ingresa sin conectividad.
///
/// ⚠️ <b>La ruta se compara por el destino declarado</b>, que es lo único que el expediente
/// lleva hoy. Es más débil que «ruta»: dos misiones a Choluteca por caminos distintos se ven
/// iguales. `[C]` con Auditoría Interna el alcance literal — `NRM-02` no lo precisa.
/// </summary>
public sealed record PermisoDeCirculacion(
    string Folio,
    IdPersona EmitidoPor,
    Ulid Vehiculo,
    Ulid Motorista,
    string Destino,
    DateOnly Desde,
    DateOnly Hasta)
{
    /// <summary>
    /// ¿Cubre esta salida?
    ///
    /// <b>La ventana tiene que estar contenida entera</b>, no solaparse. Un permiso que cubre
    /// tres de los cinco días de la misión no ampara los otros dos, y el agente que revise
    /// el cuarto día tiene un vehículo del Estado circulando sin respaldo.
    /// </summary>
    public bool Ampara(Ulid vehiculo, Ulid motorista, string destino, VentanaDeMision ventana) =>
        Vehiculo == vehiculo
        && Motorista == motorista
        && string.Equals(Destino, destino, StringComparison.OrdinalIgnoreCase)
        && Desde <= ventana.Salida
        && Hasta >= ventana.FinDelRango;
}
