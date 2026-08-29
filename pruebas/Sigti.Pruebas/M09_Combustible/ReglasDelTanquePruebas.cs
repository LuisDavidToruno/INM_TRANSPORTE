using Sigti.Dominio.M01_Organizacion;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M09_Combustible;
using Sigti.Dominio.Organizacion;

namespace Sigti.Pruebas.M09_Combustible;

/// <summary>
/// Los controles del libro de existencias, uno por uno — `RN-83` punto 5.
/// </summary>
public class ReglasDelTanquePruebas
{
    private static Autoria Quien(string persona) =>
        Autoria.De(new IdPersona(persona), new IdPuesto("PU-COMBUSTIBLE"),
            new DateOnly(2026, 3, 16));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Un_movimiento_de_cero_o_negativo_no_mueve_existencias(decimal galones)
    {
        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTanque.ExigirGalonesPositivos(galones));

        // El signo lo pone el tipo de asiento, no el número: un galonaje negativo en una
        // columna es un dato que se puede teclear al revés sin que nada lo note.
        Assert.Contains("El signo lo pone el tipo", error.Message);
    }

    [Fact]
    public void Despachar_exactamente_lo_que_hay_se_admite()
    {
        // El límite es inclusivo: vaciar el tanque es legítimo, y dejarlo en cero no es lo
        // mismo que dejarlo en negativo.
        ReglasDelTanque.ExigirExistenciaSuficiente("Cisterna", 80m, 80m);
    }

    [Fact]
    public void Despachar_un_galon_de_mas_bloquea()
    {
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTanque.ExigirExistenciaSuficiente("Cisterna", 80m, 80.001m));
    }

    [Fact]
    public void La_segregacion_se_juzga_por_identidad_de_persona()
    {
        // Un mismo servidor con dos cuentas sigue siendo la misma persona. Se compara el valor
        // de la identidad, no el puesto ni el usuario.
        Assert.Throws<BloqueoDuro>(() => ReglasDelTanque.ExigirQueDespachaNoSeaQuienRecibe(
            Quien("P-JUAN"), new IdPersonaDelReceptor("P-JUAN")));

        ReglasDelTanque.ExigirQueDespachaNoSeaQuienRecibe(
            Quien("P-JUAN"), new IdPersonaDelReceptor("P-PEDRO"));
    }

    [Fact]
    public void El_combustible_se_compara_sin_importar_mayusculas()
    {
        // «Diesel» y «diesel» son el mismo combustible. Bloquear por la caja de las letras
        // pararía despachos legítimos y enseñaría a desconfiar del bloqueo.
        ReglasDelTanque.ExigirCombustibleCompatible("Diesel", "diesel");
    }

    [Fact]
    public void Un_trasiego_de_un_tanque_a_si_mismo_no_mueve_nada()
    {
        var uno = Ulid.NewUlid();

        var error = Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTanque.ExigirTanquesDistintos(uno, uno));

        Assert.Contains("dos asientos que se anulan", error.Message);
    }

    [Fact]
    public void No_se_trasiega_entre_combustibles_distintos()
    {
        Assert.Throws<BloqueoDuro>(() =>
            ReglasDelTanque.ExigirMismoCombustible("Diesel", "Gasolina"));

        ReglasDelTanque.ExigirMismoCombustible("Diesel", "Diesel");
    }
}
