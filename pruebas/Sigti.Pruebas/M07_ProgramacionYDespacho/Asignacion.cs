using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Pruebas.M07_ProgramacionYDespacho;

/// <summary>
/// Asignaciones de prueba. <b>La matriz de acá es dato de prueba, no la matriz oficial</b>,
/// que sigue siendo insumo abierto `[C]`.
/// </summary>
internal static class Asignacion
{
    /// <summary>
    /// La ventana arranca DESPUÉS del momento de las pruebas —2026-03-12— a propósito.
    /// Si empezara el mismo día, la aprobación estaría caducada y `T-08` no se podría
    /// ejercer: programar el día de salida ya es tarde.
    /// </summary>
    public static readonly VentanaDeMision Ventana =
        new(new DateOnly(2026, 3, 20), new DateOnly(2026, 3, 22), HolguraDias: 1);

    public static readonly FichaTecnica Pickup =
        new("PICKUP", ClaseNormativa.Automovil, PesoBrutoKg: 2_800, CapacidadPasajeros: 5, LlevaRemolque: false);

    public static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("PRUEBA-01",
    [
        new EntradaDeMatriz(CategoriaDeLicencia.B, ClaseNormativa.Automovil, 3_500, 8, PermiteRemolque: false,
            VigenteDesde: new DateOnly(2026, 1, 1), VigenteHasta: null,
            RegistradoDesde: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6)),
            RegistradoHasta: null)
    ]);

    public static AsignacionDeMision Valida() => ConLicenciaHasta(new DateOnly(2027, 1, 1));

    public static AsignacionDeMision ConLicenciaHasta(DateOnly vencimiento) => new(
        Licencia: new Licencia("0801-1990-01234", CategoriaDeLicencia.B, vencimiento, []),
        Vehiculo: Pickup,
        Documentacion: new DocumentacionDelVehiculo
        {
            Placa = "PAA1234",
            TieneConstanciaSustitutaDePlaca = false,
            VenceMatricula = new DateOnly(2027, 1, 1),
            VencePoliza = new DateOnly(2027, 1, 1),
            VenceRevisionMecanica = new DateOnly(2027, 1, 1),
            IdentificacionInstitucionalVerificada = true
        });
}
