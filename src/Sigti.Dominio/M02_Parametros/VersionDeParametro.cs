using Sigti.Dominio.Organizacion;

namespace Sigti.Dominio.M02_Parametros;

/// <summary>
/// Una versión de un parámetro normativo, con <b>los dos ejes de tiempo</b> (`ADR-006`).
///
/// | Eje | Columnas | Qué responde |
/// |---|---|---|
/// | Vigencia normativa | `VigenteDesde` / `VigenteHasta` | Qué decía el reglamento el día del viaje |
/// | Tiempo de transacción | `RegistradoDesde` / `RegistradoHasta` | Qué sabía el sistema el día que se liquidó |
///
/// <b>Ninguno de los dos es nativo del motor.</b> SQL Server 2014 no tiene temporal
/// tables, y aunque las tuviera darían el eje de transacción, no el de vigencia
/// normativa — que es el que más importa acá.
/// </summary>
/// <param name="VigenteHasta">Nulo mientras es la versión normativa vigente.</param>
/// <param name="RegistradoHasta">
/// Nulo mientras es lo que el sistema cree hoy. Al corregir retroactivamente se cierra
/// esta fecha y se inserta la versión corregida: <b>la fila anterior no se actualiza</b>,
/// para que las liquidaciones ya emitidas sigan siendo explicables (`P-3`, `RN-04`).
/// </param>
/// <param name="AprobadoPor">
/// Nulo mientras la carga está pendiente. Una versión sin aprobar <b>no resuelve</b>:
/// el doble control de `HU-145` sería decorativo si el valor ya estuviera en uso.
/// </param>
public sealed record VersionDeParametro(
    string Clave,
    string Valor,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    DateTimeOffset RegistradoDesde,
    DateTimeOffset? RegistradoHasta,
    IdPersona CargadoPor,
    IdPersona? AprobadoPor)
{
    public bool EstaAprobada => AprobadoPor is not null;

    /// <summary>¿Regía normativamente en la fecha del hecho?</summary>
    public bool RegiaEl(DateOnly fechaDelHecho) =>
        VigenteDesde <= fechaDelHecho && (VigenteHasta is null || fechaDelHecho <= VigenteHasta);

    /// <summary>¿El sistema ya conocía esta versión en ese instante?</summary>
    public bool EraConocidaAl(DateTimeOffset instante) =>
        RegistradoDesde <= instante && (RegistradoHasta is null || instante < RegistradoHasta);
}
