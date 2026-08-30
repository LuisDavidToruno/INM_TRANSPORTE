using System.Text.RegularExpressions;

using Sigti.Dominio.Reglas;

namespace Sigti.Pruebas.Reglas;

/// <summary>
/// `R-3` tercera parte — el camino de salida.
///
/// <i>«Un mensaje genérico produce una llamada a soporte; un mensaje preciso produce la acción
/// correcta.»</i>
/// </summary>
public class ReglasDeLaSalidaPruebas
{
    [Theory]
    [InlineData("BD-01")]
    [InlineData("BD-02")]
    [InlineData("BD-03")]
    [InlineData("BD-04")]
    [InlineData("BD-05")]
    [InlineData("BD-06")]
    [InlineData("BD-07")]
    [InlineData("BD-08")]
    [InlineData("BD-09")]
    [InlineData("BD-10")]
    [InlineData("BD-11")]
    [InlineData("BD-12")]
    [InlineData("BD-13")]
    public void Las_trece_precondiciones_de_la_autoridad_tienen_camino(string bd)
    {
        var camino = ReglasDeLaSalida.De(bd);

        Assert.NotNull(camino);
        Assert.False(string.IsNullOrWhiteSpace(camino.QuePuedeHacer));
        Assert.False(string.IsNullOrWhiteSpace(camino.AQuienAcudir));
    }

    [Fact]
    public void Ningun_camino_manda_al_administrador_del_sistema()
    {
        // `ACT-01` no tiene acceso al contenido de negocio: mandar ahí un bloqueo de negocio
        // agrega un paso y no resuelve nada. Es la salida cómoda que esta prueba impide.
        foreach (var bd in Enumerable.Range(1, 13).Select(n => $"BD-{n:00}"))
        {
            var camino = ReglasDeLaSalida.De(bd)!;

            Assert.DoesNotContain("administrador", camino.AQuienAcudir!,
                StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("soporte", camino.AQuienAcudir!,
                StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void La_salida_de_BD_10_nombra_la_transicion_que_la_ficha_declara()
    {
        // La ficha dice que se cubre con otro **sin perder la trazabilidad de la asignación
        // original**, y que eso es `T-10`. Decir sólo «asigne otro motorista» perdería justo
        // la parte que distingue el acto correcto del atajo.
        var camino = ReglasDeLaSalida.De("BD-10")!;

        Assert.Contains("T-10", camino.QuePuedeHacer);
        Assert.Contains("trazabilidad", camino.QuePuedeHacer);
    }

    [Fact]
    public void Una_regla_de_negocio_sin_camino_concreto_devuelve_su_ficha()
    {
        var camino = ReglasDeLaSalida.De("RN-83");

        Assert.NotNull(camino);
        Assert.Contains("RN-83", camino.Ficha);

        // Y no se inventa a quién acudir: nulo es «no se sabe quién resuelve».
        Assert.Null(camino.AQuienAcudir);
    }

    [Fact]
    public void Lo_que_no_se_conoce_devuelve_nulo_y_no_una_instruccion_generica()
    {
        // Devolver «comuníquese con el administrador» para todo lo desconocido convertiría el
        // silencio en una instrucción, y la instrucción sería falsa la mayoría de las veces.
        Assert.Null(ReglasDeLaSalida.De("XX-99"));
        Assert.Null(ReglasDeLaSalida.De(""));
    }

    // ── La guarda de arquitectura ───────────────────────────────────────────

    [Fact]
    public void Ningun_bloqueo_del_dominio_usa_un_identificador_comodin()
    {
        // ⚠️ Esta prueba existe porque **siete bloqueos distintos compartían el literal
        // `"W-xx"`**. `PT-004` muestra ese identificador en pantalla: «W-xx» no le dice a nadie
        // qué regla lo detuvo, no se puede rastrear contra la autoridad, y no se puede
        // documentar un camino de salida para él.
        var raiz = RaizDelRepositorio();

        var comodines = Directory
            .EnumerateFiles(Path.Combine(raiz, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .SelectMany(f => File.ReadLines(f)
                .Select((linea, i) => (Archivo: Path.GetFileName(f), Numero: i + 1, linea))
                .Where(x => Regex.IsMatch(x.linea, @"new BloqueoDuro\(""[A-Za-z§-]*[xX]{2}")))
            .Select(x => $"{x.Archivo}:{x.Numero}")
            .ToList();

        Assert.True(comodines.Count == 0,
            "Estos bloqueos usan un identificador comodín, y la pantalla de bloqueo de " +
            $"`PT-004` lo muestra tal cual: {string.Join(", ", comodines)}");
    }

    private static string RaizDelRepositorio()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        return dir.FullName;
    }
}
