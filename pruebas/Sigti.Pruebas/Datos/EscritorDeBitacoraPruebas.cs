using Microsoft.EntityFrameworkCore;
using Sigti.Datos.Bitacora;
using Sigti.Dominio.Bitacora;

namespace Sigti.Pruebas.Datos;

/// <summary>
/// La cadena de hash es inherentemente secuencial: el asiento n necesita el hash del
/// n−1. Con escrituras concurrentes, dos transacciones que lean la misma cola
/// <b>bifurcan la cadena</b> y deja de detectar alteraciones, que es lo único para lo
/// que existe (ADR-002).
///
/// Esta es la prueba que separa «funciona con un usuario» de «funciona en producción».
/// </summary>
public class EscritorDeBitacoraPruebas : IClassFixture<BaseDePruebas>
{
    private readonly BaseDePruebas _base;
    private static readonly DateTimeOffset Momento =
        new(2026, 3, 12, 9, 0, 0, TimeSpan.FromHours(-6));

    public EscritorDeBitacoraPruebas(BaseDePruebas baseDePruebas) => _base = baseDePruebas;

    [Fact]
    public async Task Veinte_escrituras_concurrentes_no_bifurcan_la_cadena()
    {
        const string cola = "mision:01JQPRUEBA000000000000000";
        const int escritores = 20;

        var tareas = Enumerable.Range(1, escritores).Select(async i =>
        {
            await using var contexto = _base.Contexto();
            var escritor = new EscritorDeBitacora(contexto);
            await escritor.EscribirAsync(cola, $"EVENTO-{i:D2}", Momento);
        });

        await Task.WhenAll(tareas);

        await using var lectura = _base.Contexto();
        var asientos = await lectura.Asientos
            .Where(a => a.Cola == cola)
            .OrderBy(a => a.Secuencia)
            .ToListAsync();

        // Cordura: si no se escribió nada, la verificación de abajo pasa por vacía.
        Assert.Equal(escritores, asientos.Count);

        // Las secuencias son contiguas: ninguna transacción reutilizó la del vecino.
        Assert.Equal(
            Enumerable.Range(1, escritores).Select(i => (long)i),
            asientos.Select(a => a.Secuencia));

        var cadena = asientos.Select(a => new EslabonDeCadena(a.Contenido, a.Hash)).ToList();
        Assert.True(CadenaDeHash.Verificar(cadena),
            "La cadena se bifurcó: hay asientos cuyo hash no encadena con el anterior.");
    }
}
