using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Aplicacion.M02_Parametros;

/// <summary>
/// Resuelve los parámetros normativos vigentes <b>a la fecha del hecho</b>, no a la de
/// captura (`P-4`, `RNF-05`).
/// </summary>
public interface IParametrosDeLaInstitucion
{
    MatrizDeLicencias MatrizVigenteAl(DateOnly fecha);
    PoliticaDeDocumentacion PoliticaVigenteAl(DateOnly fecha);
}

/// <summary>
/// ⚠️ <b>Implementación provisional del walking skeleton.</b>
///
/// La matriz oficial licencia↔vehículo es <b>insumo abierto `[C]`</b> — el PDF de la DNVT
/// está entre los cuatro documentos que se pueden descargar sin la institución. Los
/// valores de acá <b>no son normativos</b> y no deben citarse como tales.
///
/// Cuando llegue la matriz real, esto se reemplaza por la carga desde el catálogo con
/// vigencia de `M-02` — `HU-144` a `HU-150`, con su doble control—, y esta clase se borra.
/// Devolver una matriz vacía habría sido peor: bloquearía toda programación y el
/// esqueleto no podría caminar.
/// </summary>
public sealed class ParametrosProvisionales : IParametrosDeLaInstitucion
{
    private static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("PROVISIONAL-SIN-FUENTE-OFICIAL",
    [
        new EntradaDeMatriz(CategoriaDeLicencia.B, PesoBrutoMaximoKg: 3_500,
            CapacidadMaximaPasajeros: 8, PermiteArticulado: false,
            VigenteDesde: new DateOnly(2026, 1, 1), VigenteHasta: null,
            RegistradoDesde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6)),
            RegistradoHasta: null)
    ]);

    public MatrizDeLicencias MatrizVigenteAl(DateOnly fecha) => Matriz;

    /// <summary>Póliza y revisión apagadas: no son obligatorias por ley vigente (`DP-001, D-13`).</summary>
    public PoliticaDeDocumentacion PoliticaVigenteAl(DateOnly fecha) => PoliticaDeDocumentacion.PorDefecto;
}
