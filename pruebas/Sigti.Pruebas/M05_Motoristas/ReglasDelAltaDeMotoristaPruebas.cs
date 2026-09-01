using Sigti.Dominio.M05_Motoristas;

namespace Sigti.Pruebas.M05_Motoristas;

/// <summary>
/// El alta de un motorista en el padrón — <b>`M-05`</b>, acción 24 de la matriz de permisos.
///
/// ── La distinción que gobierna este archivo ─────────────────────────────────
/// <b>Registrar no es habilitar.</b> El padrón es el censo de quién conduce en la institución;
/// quién puede salir hoy en misión lo deciden `BD-02` y `RN-57` contra la licencia vigente
/// <i>a la fecha del hecho</i>. Confundir las dos cosas deja gente fuera del sistema por un
/// dato que se arregla renovando un documento.
/// </summary>
public class ReglasDelAltaDeMotoristaPruebas
{
    private static readonly DateOnly Hoy = new(2026, 9, 1);

    /// <summary>
    /// El número de licencia es <b>lo que se cita ante un retén</b>. Sin él no hay a qué
    /// referirse cuando la autoridad de tránsito pregunta.
    /// </summary>
    [Fact]
    public void Sin_numero_de_licencia_no_hay_alta()
    {
        var r = ReglasDelAltaDeMotorista.Evaluar(Valido() with { NumeroDeLicencia = "  " }, Hoy);

        Assert.False(r.Procede);
        Assert.Contains(MotivoDeRechazoDelMotorista.SinNumeroDeLicencia, r.Reparos);
    }

    /// <summary>
    /// Sin nombre no hay a quién nombrar en la Orden de Misión ni en la bitácora, que es donde
    /// el motorista aparece.
    /// </summary>
    [Fact]
    public void Sin_nombre_no_hay_alta()
    {
        var r = ReglasDelAltaDeMotorista.Evaluar(Valido() with { Nombre = "" }, Hoy);

        Assert.False(r.Procede);
        Assert.Contains(MotivoDeRechazoDelMotorista.SinNombre, r.Reparos);
    }

    /// <summary>
    /// <b>Una licencia vencida se registra.</b> El padrón es el censo de quién conduce en la
    /// institución; quién sale hoy lo deciden `BD-02` y `RN-57` contra la vigencia <i>a la fecha
    /// del hecho</i>, y ya bloquean por su cuenta.
    ///
    /// Negar el alta dejaría a la persona <b>fuera del sistema</b> por un documento que se
    /// renueva — y con ella su historial, sus capacitaciones y sus restricciones médicas.
    /// Registrarla sin decir nada sería el otro error: entra al padrón viéndose igual que
    /// alguien con la licencia al día.
    /// </summary>
    [Fact]
    public void Una_licencia_ya_vencida_se_registra_y_se_declara()
    {
        var r = ReglasDelAltaDeMotorista.Evaluar(
            Valido() with { VenceLicencia = Hoy.AddDays(-1) }, Hoy);

        Assert.True(r.Procede);
        Assert.Contains(ObservacionDelMotorista.LicenciaVencidaAlAlta, r.Observaciones);
    }

    private static AltaDeMotorista Valido() => new()
    {
        Nombre = "Nery Alvarado",
        EsDelPadron = true,
        NumeroDeLicencia = "0801-1988-04412",
        Categoria = CategoriaDeLicencia.C,
        VenceLicencia = Hoy.AddYears(2),
    };
}
