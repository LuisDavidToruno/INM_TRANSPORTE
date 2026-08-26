using System.Reflection;
using Sigti.Dominio.Bitacora;

namespace Sigti.Pruebas.Arquitectura;

/// <summary>
/// La regla de dependencia de ADR-009, hecha cumplir mecánicamente:
/// Sigti.Dominio no referencia EF Core ni ASP.NET.
///
/// Sin esta guarda, la regla es una intención escrita en un documento que nadie
/// vuelve a abrir.
/// </summary>
public class DominioNoConoceInfraestructura
{
    private static readonly string[] Prohibidos =
    [
        "Microsoft.EntityFrameworkCore",
        "Microsoft.AspNetCore",
        "Microsoft.Data.SqlClient",
        "System.Data.SqlClient"
    ];

    [Fact]
    public void El_ensamblado_del_dominio_no_referencia_infraestructura()
    {
        var dominio = typeof(CadenaDeHash).Assembly;
        var referencias = dominio.GetReferencedAssemblies()
            .Select(r => r.Name ?? string.Empty)
            .ToList();

        // Cordura: si el dominio no referencia nada, algo se rompió y la guarda
        // estaría pasando por vacía, no por correcta.
        Assert.True(referencias.Count > 0,
            "El dominio no declara ninguna referencia. La guarda no está inspeccionando nada.");

        var infractoras = referencias
            .Where(r => Prohibidos.Any(p => r.StartsWith(p, StringComparison.Ordinal)))
            .ToList();

        Assert.True(infractoras.Count == 0,
            "Sigti.Dominio referencia infraestructura, y ADR-009 lo prohíbe: " +
            string.Join(", ", infractoras));
    }

    [Fact]
    public void El_proyecto_del_dominio_no_declara_paquetes_de_infraestructura()
    {
        var csproj = Path.Combine(Repositorio.Fuente("Sigti.Dominio"), "Sigti.Dominio.csproj");

        Assert.True(File.Exists(csproj), $"No se encontró {csproj}. La guarda no está inspeccionando nada.");

        var contenido = File.ReadAllText(csproj);

        // Cordura: el archivo tiene que tener contenido real de proyecto.
        Assert.Contains("<Project", contenido, StringComparison.Ordinal);

        foreach (var prohibido in Prohibidos)
        {
            Assert.DoesNotContain(prohibido, contenido, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void El_dominio_expone_tipos_publicos()
    {
        var tipos = typeof(CadenaDeHash).Assembly
            .GetTypes()
            .Where(t => t.IsPublic)
            .ToList();

        // Cordura de todas las guardas que recorren el dominio: si no hay tipos,
        // cualquier aserción sobre ellos pasa por vacía.
        Assert.True(tipos.Count > 0, "El dominio no expone ningún tipo público.");
    }
}
