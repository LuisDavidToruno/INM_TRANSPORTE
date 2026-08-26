namespace Sigti.Pruebas.Arquitectura;

/// <summary>
/// «Común» es el nombre al que las cosas van a la deriva: nadie lo abre a preguntarse
/// si la regla ya está ahí. Reglas/ sí se abre, porque su nombre dice qué contiene
/// (ADR-009).
/// </summary>
public class NoExisteCarpetaComun
{
    private static readonly string[] NombresALaDeriva =
    [
        "Comun", "Common", "Compartido", "Shared", "Utils", "Utilidades", "Helpers", "Misc"
    ];

    [Fact]
    public void Ninguna_carpeta_del_codigo_fuente_se_llama_comun_ni_equivalente()
    {
        var src = Path.Combine(Repositorio.Raiz, "src");

        Assert.True(Directory.Exists(src), $"No existe {src}. La guarda no está inspeccionando nada.");

        var carpetas = Directory.GetDirectories(src, "*", SearchOption.AllDirectories)
            .Where(d => !d.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(d => !d.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToList();

        // Cordura: si no hay carpetas que revisar, la guarda pasa por vacía.
        Assert.True(carpetas.Count > 0, "La guarda no encontró ninguna carpeta bajo src/.");

        var infractoras = carpetas
            .Where(d => NombresALaDeriva.Contains(Path.GetFileName(d), StringComparer.OrdinalIgnoreCase))
            .ToList();

        Assert.True(infractoras.Count == 0,
            "Hay carpetas con nombre a la deriva. Lo compartido va a Reglas/ o baja a dominio/ (ADR-009):\n  " +
            string.Join("\n  ", infractoras));
    }
}
