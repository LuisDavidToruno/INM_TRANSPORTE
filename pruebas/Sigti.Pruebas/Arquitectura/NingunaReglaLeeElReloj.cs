using System.Text.RegularExpressions;

namespace Sigti.Pruebas.Arquitectura;

/// <summary>
/// Ninguna regla del dominio lee el reloj. La fecha entra como parámetro —
/// Reglas.CalcularX(datos, vigenteAl)— que es la firma que ADR-006 y ADR-007
/// necesitan.
///
/// Si una sola regla llama a DateTime.Now por dentro, se pierden dos cosas a la vez:
/// la pureza que la hace probable y la temporalidad que la hace correcta.
/// </summary>
public partial class NingunaReglaLeeElReloj
{
    [GeneratedRegex(@"\b(DateTime|DateTimeOffset)\s*\.\s*(Now|UtcNow|Today)\b")]
    private static partial Regex LecturaDeReloj();

    [Fact]
    public void El_dominio_no_lee_el_reloj_por_dentro()
    {
        var archivos = Repositorio.ArchivosDeCodigo(Repositorio.Fuente("Sigti.Dominio"));

        // Cordura: una guarda que no encuentra archivos pasa por vacía. ADR-009 exige
        // que en ese caso falle, no que pase.
        Assert.True(archivos.Count > 0,
            "La guarda no encontró ningún archivo en Sigti.Dominio. No está inspeccionando nada.");

        var infractores = new List<string>();

        foreach (var archivo in archivos)
        {
            var contenido = File.ReadAllText(archivo);
            var coincidencia = LecturaDeReloj().Match(contenido);

            if (coincidencia.Success)
            {
                var linea = contenido[..coincidencia.Index].Count(c => c == '\n') + 1;
                infractores.Add($"{Path.GetFileName(archivo)}:{linea} → {coincidencia.Value}");
            }
        }

        Assert.True(infractores.Count == 0,
            "Hay reglas del dominio que leen el reloj. La fecha se recibe como parámetro (ADR-006, ADR-007):\n  " +
            string.Join("\n  ", infractores));
    }
}
