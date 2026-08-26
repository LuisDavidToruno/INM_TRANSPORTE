namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Los estados del expediente de misión, según docs/03-arquitectura/estados/orden-de-mision.md.
///
/// Es un solo expediente con dos fases, no dos entidades que se copian: BORRADOR,
/// SOLICITADA, APROBADA y RECHAZADA son la fase de solicitud (M-06); desde PROGRAMADA
/// en adelante es la Orden de Misión propiamente dicha (M-07).
/// </summary>
public enum EstadoDeMision
{
    Borrador,
    Solicitada,
    Aprobada,
    Programada,
    Despachada,
    EnRuta,
    Retornada,
    Liquidada,
    Cerrada,
    CerradaConHallazgo,
    Rechazada,
    Anulada
}
