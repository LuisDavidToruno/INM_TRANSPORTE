using Sigti.Dominio.Reglas;

namespace Sigti.Pruebas.Reglas;

/// <summary>
/// El eje normativo de `RNF-05`, extraído para que <b>todo</b> lo que tiene vigencia lo
/// resuelva igual: tarifas de peaje, umbrales, feriados y la matriz licencia↔vehículo.
///
/// `ADR-006` advierte el fallo que esto evita: que alguien implemente un eje y suponga
/// que el otro viene puesto. Con una sola regla compartida, o están los dos o no está
/// ninguno.
/// </summary>
public class ReglasDeVigenciaPruebas
{
    private sealed record Tarifa(
        string Valor,
        DateOnly VigenteDesde,
        DateOnly? VigenteHasta,
        DateTimeOffset RegistradoDesde,
        DateTimeOffset? RegistradoHasta) : IConVigencia;

    private static readonly DateTimeOffset Enero = new(2026, 1, 1, 0, 0, 0, TimeSpan.FromHours(-6));
    private static readonly DateTimeOffset Septiembre = new(2026, 9, 1, 0, 0, 0, TimeSpan.FromHours(-6));

    [Fact]
    public void Resuelve_por_los_dos_ejes_a_la_vez()
    {
        var original = new Tarifa("22.00", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), Enero, Septiembre);
        var corregida = new Tarifa("24.00", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), Septiembre, null);
        var posterior = new Tarifa("25.00", new DateOnly(2026, 7, 1), null, Enero, null);

        Tarifa[] tarifas = [original, corregida, posterior];

        var marzo = new DateOnly(2026, 3, 12);
        var agosto = new DateOnly(2026, 8, 2);
        var abril = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-6));
        var octubre = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.FromHours(-6));

        // Marzo visto desde abril: lo que el sistema creía al liquidar.
        Assert.Equal("22.00", ReglasDeVigencia.VigenteA(tarifas, marzo, abril)?.Valor);

        // El mismo marzo visto desde octubre: lo que la norma decía, ya corregido.
        Assert.Equal("24.00", ReglasDeVigencia.VigenteA(tarifas, marzo, octubre)?.Valor);

        // Agosto no fue tocado por la corrección.
        Assert.Equal("25.00", ReglasDeVigencia.VigenteA(tarifas, agosto, octubre)?.Valor);
    }

    [Fact]
    public void Sin_version_para_la_fecha_devuelve_nulo_y_no_se_aproxima()
    {
        // Devolver la más cercana produciría un número plausible y equivocado. Quien
        // llama decide si eso es un bloqueo o una advertencia; la regla no adivina.
        Tarifa[] tarifas = [new("25.00", new DateOnly(2026, 7, 1), null, Enero, null)];

        Assert.Null(ReglasDeVigencia.VigenteA(tarifas, new DateOnly(2026, 3, 12), Septiembre));
    }
}
