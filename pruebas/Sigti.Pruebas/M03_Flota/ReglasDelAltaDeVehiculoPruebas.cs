using Sigti.Dominio.M03_Flota;

namespace Sigti.Pruebas.M03_Flota;

/// <summary>
/// El alta de un vehículo en la flota — <b>`M-03`</b>, acción 23 de la matriz de permisos.
///
/// ── Por qué el alta tiene reglas propias ────────────────────────────────────
/// Porque es el único momento en que el expediente del vehículo <b>no existe todavía</b>, y
/// todo lo demás del sistema lo da por hecho. `BD-03` evalúa documentación, `RN-33` resuelve la
/// categoría de peaje y `BD-02` cruza la licencia contra la clase: los tres presuponen que
/// alguien puso esos datos el día del alta.
/// </summary>
public class ReglasDelAltaDeVehiculoPruebas
{
    private static readonly DateOnly Hoy = new(2026, 9, 1);

    /// <summary>
    /// Las siglas son <b>la identidad estable del bien</b> y lo que se cita en el descargo. Un
    /// vehículo sin siglas no se puede nombrar en un acta, y el acta es el producto.
    /// </summary>
    [Fact]
    public void Sin_siglas_no_hay_alta()
    {
        var alta = new AltaDeVehiculo
        {
            Siglas = "   ",
            TipoDeVehiculo = "Pick-up doble cabina",
            Clase = ClaseNormativa.Automovil,
            PesoBrutoKg = 2800,
            CapacidadPasajeros = 5,
            LlevaRemolque = false,
            VenceMatricula = Hoy.AddYears(1),
        };

        var r = ReglasDelAltaDeVehiculo.Evaluar(alta, Hoy);

        Assert.False(r.Procede);
        Assert.Contains(MotivoDeRechazoDelAlta.SinSiglas, r.Reparos);
    }

    /// <summary>
    /// <b>La lámina puesta ES el número.</b> Declarar <c>ConLamina</c> sin número de placa es un
    /// dato que se contradice a sí mismo, y entra sin ruido: nadie lo nota hasta que el vehículo
    /// está en un retén y el documento impreso sale con la casilla en blanco.
    /// </summary>
    [Fact]
    public void Con_lamina_declarada_exige_el_numero_de_placa()
    {
        var r = ReglasDelAltaDeVehiculo.Evaluar(Valido() with
        {
            Placa = null,
            EstadoDePlaca = EstadoDePlaca.ConLamina,
        }, Hoy);

        Assert.False(r.Procede);
        Assert.Contains(MotivoDeRechazoDelAlta.LaminaSinNumero, r.Reparos);
    }

    /// <summary>
    /// <b>Sin placa se da de alta, y esto es el corazón del asunto.</b> Hay desabastecimiento
    /// nacional de láminas: un alta que exija placa deja fuera a la flota que de verdad circula.
    ///
    /// ⚠️ Esta prueba <b>pasa desde el primer día y ese es su oficio</b>: no empuja código nuevo,
    /// impide que alguien agregue después la validación que parece obvia.
    /// </summary>
    [Fact]
    public void Sin_placa_y_sin_numero_asignado_se_da_de_alta()
    {
        var r = ReglasDelAltaDeVehiculo.Evaluar(Valido() with
        {
            Placa = null,
            EstadoDePlaca = EstadoDePlaca.SinNumeroAsignado,
        }, Hoy);

        Assert.True(r.Procede);
        Assert.Empty(r.Reparos);
    }

    /// <summary>
    /// El número de ejes decide la categoría de peaje (`RN-33`). <b>Sin él el alta pasa igual</b>:
    /// `CategoriaDelVehiculo` ya admite el nulo a propósito —dice qué atributo falta y estima—,
    /// y bloquear acá contradiría a `M-18`.
    ///
    /// Lo que no se vale es <b>callarlo</b>: el vehículo entra a la flota con la categoría sin
    /// resolver, y quien lo dio de alta tiene que enterarse en ese momento y no en el peaje.
    /// </summary>
    [Fact]
    public void Sin_numero_de_ejes_el_alta_pasa_pero_lo_declara()
    {
        var r = ReglasDelAltaDeVehiculo.Evaluar(Valido() with { NumeroDeEjes = null }, Hoy);

        Assert.True(r.Procede);
        Assert.Contains(ObservacionDelAlta.CategoriaDePeajeSinResolver, r.Observaciones);
    }

    /// <summary>Un alta correcta, para que los reparos se lean contra algo que sí pasa.</summary>
    private static AltaDeVehiculo Valido() => new()
    {
        Siglas = "INM-0042",
        TipoDeVehiculo = "Pick-up doble cabina",
        Clase = ClaseNormativa.Automovil,
        PesoBrutoKg = 2800,
        CapacidadPasajeros = 5,
        LlevaRemolque = false,
        VenceMatricula = Hoy.AddYears(1),
        Placa = "PAA-1234",
        EstadoDePlaca = EstadoDePlaca.ConLamina,
        NumeroDeEjes = 2,
    };
}
