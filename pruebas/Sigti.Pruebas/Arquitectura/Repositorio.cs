namespace Sigti.Pruebas.Arquitectura;

/// <summary>
/// Ubica la raíz del repositorio desde donde corren las pruebas. Las guardas
/// inspeccionan el código fuente, no solo los ensamblados compilados.
/// </summary>
internal static class Repositorio
{
    public static string Raiz { get; } = Localizar();

    public static string Fuente(params string[] partes) =>
        Path.Combine([Raiz, "src", .. partes]);

    private static string Localizar()
    {
        // Local, no campo estático: Raiz se inicializa primero por orden textual, y un
        // campo declarado más abajo todavía sería null cuando este método corre.
        string[] archivosDeSolucion = ["Sigti.slnx", "Sigti.sln"];

        var directorio = new DirectoryInfo(AppContext.BaseDirectory);

        while (directorio is not null)
        {
            if (archivosDeSolucion.Any(a => File.Exists(Path.Combine(directorio.FullName, a))))
                return directorio.FullName;

            directorio = directorio.Parent;
        }

        throw new InvalidOperationException(
            $"No se encontró la solución ({string.Join(" ni ", archivosDeSolucion)}) subiendo desde " +
            AppContext.BaseDirectory +
            ". Las guardas de arquitectura no pueden inspeccionar el código fuente.");
    }

    public static IReadOnlyList<string> ArchivosDeCodigo(string directorio) =>
        Directory.Exists(directorio)
            ? Directory.GetFiles(directorio, "*.cs", SearchOption.AllDirectories)
                .Where(a => !a.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
                .Where(a => !a.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToList()
            : [];
}
