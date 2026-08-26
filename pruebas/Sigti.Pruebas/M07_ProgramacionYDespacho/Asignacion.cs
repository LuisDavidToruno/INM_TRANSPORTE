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
    public static readonly VentanaDeMision Ventana =
        new(new DateOnly(2026, 3, 12), new DateOnly(2026, 3, 14), HolguraDias: 1);

    public static readonly FichaTecnica Pickup =
        new("PICKUP", PesoBrutoKg: 2_800, CapacidadPasajeros: 5, LlevaRemolque: false);

    public static readonly MatrizDeLicencias Matriz = MatrizDeLicencias.Con("PRUEBA-01",
    [
        new EntradaDeMatriz(CategoriaDeLicencia.B, 3_500, 8, PermiteRemolque: false,
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
        },
        Ventana: Ventana);
}
