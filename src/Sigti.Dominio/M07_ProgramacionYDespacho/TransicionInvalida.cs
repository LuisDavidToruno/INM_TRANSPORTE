namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Se intentó una transición desde un estado que no la admite.
///
/// Cubre también las transiciones prohibidas de §3.4, que no existen por diseño: la más
/// peligrosa es APROBADA → DESPACHADA, «el atajo que produce el siniestro con
/// responsabilidad institucional», porque sin programación no hay verificación de
/// licencia, documentación ni reserva.
/// </summary>
public sealed class TransicionInvalida(
    string transicion,
    EstadoDeMision estadoActual,
    EstadoDeMision estadoRequerido)
    : Exception($"La transición {transicion} exige el estado {estadoRequerido}, y el expediente está en {estadoActual}.")
{
    public string Transicion { get; } = transicion;
    public EstadoDeMision EstadoActual { get; } = estadoActual;
    public EstadoDeMision EstadoRequerido { get; } = estadoRequerido;
}
