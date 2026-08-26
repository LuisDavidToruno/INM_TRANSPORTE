using System.Reflection;
using Sigti.Dominio.Bitacora;

namespace Sigti.Pruebas.Arquitectura;

/// <summary>
/// Toda clase de reglas del dominio tiene su propia clase de pruebas.
///
/// En SICOV_CORE8 solo el 57 % las tenía: 21 clases Reglas*, 12 con prueba propia.
/// Esa es la medición que motiva esta guarda, y es la contramedida obligatoria de
/// haber elegido módulos verticales — con verticales el riesgo de reimplementar la
/// misma regla en cada camino sube, no baja (ADR-009).
/// </summary>
public class CadaReglaTieneSuPrueba
{
    [Fact]
    public void Toda_clase_de_reglas_del_dominio_tiene_clase_de_pruebas()
    {
        var reglas = typeof(CadenaDeHash).Assembly
            .GetTypes()
            .Where(t => t.IsPublic && t.IsAbstract && t.IsSealed) // static
            .ToList();

        // Cordura: si no hay clases de reglas, la aserción de abajo pasa por vacía.
        Assert.True(reglas.Count > 0,
            "La guarda no encontró ninguna clase de reglas en el dominio. No está inspeccionando nada.");

        var nombresDePruebas = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Select(t => t.Name)
            .ToHashSet(StringComparer.Ordinal);

        var sinPrueba = reglas
            .Where(r => !nombresDePruebas.Contains(r.Name + "Pruebas"))
            .Select(r => r.Name)
            .ToList();

        Assert.True(sinPrueba.Count == 0,
            "Hay reglas del dominio sin clase de pruebas propia. La regla y su prueba nacen juntas (ADR-009):\n  " +
            string.Join("\n  ", sinPrueba));
    }
}
