using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Aplicacion.M03_Flota;

/// <summary>Un vehículo de la flota, tal como lo elige quien programa.</summary>
/// <param name="Placa">
/// Nula cuando no tiene placa metálica. <b>Es estado válido</b>: hay desabastecimiento
/// nacional, y un campo obligatorio acá rompería el sistema para la flota real.
/// </param>
public sealed record VehiculoDeFlota(
    string Id,
    string Siglas,
    string? Placa,
    FichaTecnica Ficha);

/// <summary>
/// Quien conduce. <b>No siempre es del padrón</b>: `RN-57` verifica la habilitación
/// sobre quien <i>efectivamente</i> conduce, sea o no motorista — el funcionario con
/// vehículo asignado no se exceptúa.
/// </summary>
public sealed record ConductorDisponible(
    string Id,
    string Nombre,
    bool EsDelPadron,
    Licencia Licencia);

/// <summary>
/// ⚠️ <b>Catálogo provisional.</b> `M-03` y `M-05` no están construidos: no hay tabla
/// de flota ni padrón de motoristas.
///
/// Vive en el servidor y no en el cliente <b>a propósito</b>. Si el cliente tuviera su
/// propia flota, tendría también que evaluar `BD-02` para mostrar el resultado antes de
/// enviar — y dos implementaciones de la precondición que traslada responsabilidad
/// legal es la peor duplicación posible de este sistema.
///
/// Cuando existan `M-03` y `M-05`, esto se reemplaza por sus repositorios y la firma
/// que ve el cliente no cambia.
/// </summary>
public sealed class CatalogoProvisionalDeFlota
{
    public IReadOnlyList<VehiculoDeFlota> Vehiculos { get; } =
    [
        new("v-001", "INS-P-014", "PBM8842",
            new FichaTecnica("Pick-up doble cabina", ClaseNormativa.Automovil, 2_800, 5, false)),

        // Sin placa metálica: estado válido.
        new("v-002", "INS-C-002", null,
            new FichaTecnica("Camión de carga", ClaseNormativa.Camion, 12_000, 3, false)),

        new("v-003", "INS-P-021", "PCH1190",
            new FichaTecnica("Pick-up con plataforma enganchada", ClaseNormativa.Automovil, 3_100, 5, true)),

        new("v-004", "INS-M-007", "MHA221",
            new FichaTecnica("Motocicleta de mensajería", ClaseNormativa.Motocicleta, 180, 1, false)),
    ];

    public IReadOnlyList<ConductorDisponible> Conductores { get; } =
    [
        new("c-001", "José Ramón Cruz", true,
            new Licencia("08-1988-77120", CategoriaDeLicencia.B, new DateOnly(2028, 4, 30), [])),

        new("c-002", "Óscar Banegas", true,
            new Licencia("05-1979-31288", CategoriaDeLicencia.C, new DateOnly(2028, 9, 12), [])),

        // Vence pronto a propósito: es el caso que BD-02 más se olvida.
        new("c-003", "Elmer Sauceda", true,
            new Licencia("01-1991-44907", CategoriaDeLicencia.B, new DateOnly(2026, 9, 5), [])),

        new("c-004", "Nery Portillo", true,
            new Licencia("03-1985-20411", CategoriaDeLicencia.BE, new DateOnly(2028, 1, 18), [])),

        new("c-005", "Dilcia Amaya", false,
            new Licencia("08-1994-10233", CategoriaDeLicencia.B, new DateOnly(2029, 7, 26),
                ["No conducir en horario nocturno"])),

        new("c-006", "Karla Munguía", true,
            new Licencia("04-1996-55231", CategoriaDeLicencia.A, new DateOnly(2028, 6, 2), [])),
    ];

    public VehiculoDeFlota? Vehiculo(string id) => Vehiculos.FirstOrDefault(v => v.Id == id);

    public ConductorDisponible? Conductor(string id) => Conductores.FirstOrDefault(c => c.Id == id);
}
