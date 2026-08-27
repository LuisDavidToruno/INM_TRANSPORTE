using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Lo que se asigna a una misión, y contra lo que se evalúan `BD-02` y `BD-03`.
///
/// Los datos entran completos en lugar de consultarse desde el dominio: la regla es pura
/// y quien la llama es responsable de traerle los datos vigentes (`ADR-009`). Eso es
/// también lo que permite reevaluar la misma asignación en `T-12` con los datos de ese
/// momento, que es lo que `BD-02` exige — «se revalida en cada una».
///
/// <b>NO lleva ventana, y es deliberado.</b> La ventana la declara quien pide, no quien
/// programa: si quien programa pudiera pasarla, acortaría el rango hasta que la licencia
/// del motorista disponible alcanzara, y `BD-02` dejaría de proteger justo a quien
/// autoriza. Sale de <c>Solicitud.Ventana</c>, y el compilador impide sustituirla.
/// </summary>
public sealed record AsignacionDeMision(
    Licencia Licencia,
    FichaTecnica Vehiculo,
    DocumentacionDelVehiculo Documentacion);
