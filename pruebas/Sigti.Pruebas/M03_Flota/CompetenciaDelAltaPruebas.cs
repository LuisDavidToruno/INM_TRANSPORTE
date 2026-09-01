using Sigti.Aplicacion.M03_Flota;
using Sigti.Aplicacion.M05_Motoristas;
using Sigti.Datos;
using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M05_Motoristas;
using Sigti.Dominio.Organizacion;
using Sigti.Pruebas.Datos;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// Quién puede dar de alta — <b>§4 de <c>actores-y-roles.md</c>, que es la autoridad</b>.
///
/// ── La asimetría que esto defiende ──────────────────────────────────────────
/// La matriz de permisos no da lo mismo en las dos altas, y la diferencia es deliberada:
///
/// <list type="bullet">
/// <item><b>Acción 23</b> — mantener el expediente del vehículo: `ACT-04` Jefe de Transporte,
/// `ACT-10` Encargado de Delegación y `ACT-14` Encargado de Bienes <b>ejecutan</b>.</item>
/// <item><b>Acción 24</b> — habilitar motorista en el padrón: <b>sólo `ACT-04` ejecuta</b>.
/// `ACT-10` figura como <c>P</c>: <i>propone</i>, y proponer no es consumar el acto.</item>
/// </list>
///
/// Es la delegación que puede incorporar el vehículo que le asignaron, y no puede meter gente
/// al padrón de quien conduce. Si las dos altas se resolvieran con el mismo permiso, esa
/// distinción se perdería sin que nadie la hubiera derogado.
/// </summary>
[Collection(ColeccionDeBaseDeDatos.Nombre)]
public class CompetenciaDelAltaPruebas(BaseDePruebas baseDePruebas)
{
    private static readonly DateOnly Hoy = new(2026, 9, 1);

    [Fact]
    public async Task El_encargado_de_delegacion_registra_un_vehiculo()
    {
        await using var contexto = baseDePruebas.Contexto();
        var quien = await PuestoSembrado.ConRolAsync(contexto, Rol.EncargadoDeDelegacion, Hoy.AddYears(-1));

        var servicio = new ServicioDeAltaDeFlota(contexto);

        var r = await servicio.RegistrarAsync(Vehiculo(), Hoy, quien);

        Assert.True(r.Procede);
    }

    /// <summary>
    /// El mismo puesto, la misma persona, el mismo día — y el padrón de motoristas le dice que
    /// no. <b>La acción 24 le da `P`, no `E`.</b>
    /// </summary>
    [Fact]
    public async Task El_encargado_de_delegacion_NO_registra_un_motorista()
    {
        await using var contexto = baseDePruebas.Contexto();
        var quien = await PuestoSembrado.ConRolAsync(contexto, Rol.EncargadoDeDelegacion, Hoy.AddYears(-1));

        var servicio = new ServicioDeAltaDeMotoristas(contexto);

        var r = await servicio.RegistrarAsync(Motorista(), Hoy, quien);

        Assert.False(r.Procede);
        Assert.Contains("padrón", r.Mensaje);
    }

    [Fact]
    public async Task El_jefe_de_transporte_registra_un_motorista()
    {
        await using var contexto = baseDePruebas.Contexto();
        var quien = await PuestoSembrado.ConRolAsync(contexto, Rol.JefeDeTransporte, Hoy.AddYears(-1));

        var servicio = new ServicioDeAltaDeMotoristas(contexto);

        var r = await servicio.RegistrarAsync(Motorista(), Hoy, quien);

        Assert.True(r.Procede);
    }

    /// <summary>
    /// Un solicitante cualquiera no incorpora bienes a la flota. Sin esto el alta quedaría
    /// abierta a quien tenga un token, que es <b>cualquiera de las 193 personas del padrón</b>.
    /// </summary>
    [Fact]
    public async Task Un_solicitante_no_registra_vehiculos()
    {
        await using var contexto = baseDePruebas.Contexto();
        var quien = await PuestoSembrado.ConRolAsync(contexto, Rol.Solicitante, Hoy.AddYears(-1));

        var servicio = new ServicioDeAltaDeFlota(contexto);

        var r = await servicio.RegistrarAsync(Vehiculo(), Hoy, quien);

        Assert.False(r.Procede);

        // ⚠️ **Y hay que poder distinguirlo de un rechazo por el dato.** No tener competencia y
        // repetir unas siglas son cosas distintas: la primera es un `403` —usted no puede— y la
        // segunda un `409` —el dato choca—. Sin esta marca la API las contesta igual, y quien
        // recibe un 409 se pone a cambiarle las siglas al vehículo cuando el problema es su puesto.
        Assert.True(r.EsFaltaDeCompetencia);
    }

    private static AltaDeVehiculo Vehiculo() => new()
    {
        Siglas = $"INM-{Ulid.NewUlid().ToString()[^8..]}",
        TipoDeVehiculo = "Pick-up doble cabina",
        Clase = ClaseNormativa.Automovil,
        PesoBrutoKg = 2800,
        CapacidadPasajeros = 5,
        LlevaRemolque = false,
        VenceMatricula = Hoy.AddYears(1),
        Placa = "PAA-9911",
        EstadoDePlaca = EstadoDePlaca.ConLamina,
        NumeroDeEjes = 2,
    };

    private static AltaDeMotorista Motorista() => new()
    {
        Nombre = "Nery Alvarado",
        EsDelPadron = true,
        NumeroDeLicencia = $"0801-{Ulid.NewUlid().ToString()[^10..]}",
        Categoria = CategoriaDeLicencia.C,
        VenceLicencia = Hoy.AddYears(2),
    };
}
