using Sigti.Dominio.Bitacora;

namespace Sigti.Pruebas.Bitacora;

/// <summary>
/// La cadena de hash existe para una sola cosa: detectar que un asiento fue alterado
/// después de escrito (RNF-04). Estas pruebas verifican esa propiedad, no la
/// implementación del algoritmo.
/// </summary>
public class CadenaDeHashPruebas
{
    [Fact]
    public void El_hash_depende_del_contenido_del_asiento()
    {
        var anterior = CadenaDeHash.Origen;

        var uno = CadenaDeHash.Calcular(anterior, "APROBADA por ACT-03");
        var otro = CadenaDeHash.Calcular(anterior, "APROBADA por ACT-04");

        Assert.NotEqual(uno, otro);
    }

    [Fact]
    public void Una_cadena_intacta_verifica()
    {
        var cadena = Encadenar(
            "SOLICITADA por ACT-02",
            "APROBADA por ACT-03",
            "PROGRAMADA por ACT-04");

        Assert.True(CadenaDeHash.Verificar(cadena));
    }

    [Fact]
    public void Alterar_el_contenido_de_un_asiento_rompe_la_verificacion()
    {
        var cadena = Encadenar(
            "SOLICITADA por ACT-02",
            "APROBADA por ACT-03",
            "PROGRAMADA por ACT-04");

        // Alguien edita el asiento del medio directamente en la base: cambia quién aprobó.
        cadena[1] = cadena[1] with { Contenido = "APROBADA por ACT-04" };

        Assert.False(CadenaDeHash.Verificar(cadena));
    }

    [Fact]
    public void Truncar_la_cola_de_la_cadena_falla_contra_el_sello_anclado()
    {
        var cadena = Encadenar(
            "SOLICITADA por ACT-02",
            "APROBADA por ACT-03",
            "PROGRAMADA por ACT-04");

        var sello = cadena[^1].Hash;

        // Alguien borra el último asiento. La cadena que queda es internamente
        // impecable: no hay nada dentro de ella que delate lo que falta.
        cadena.RemoveAt(cadena.Count - 1);
        Assert.True(CadenaDeHash.Verificar(cadena));

        // Contra el sello anclado fuera del alcance de quien administra la base, no pasa.
        Assert.False(CadenaDeHash.Verificar(cadena, sello));
    }

    /// <summary>
    /// Encadena contenidos como lo haría el escritor de bitácora, para tener una
    /// cadena legítima contra la cual probar.
    /// </summary>
    private static List<EslabonDeCadena> Encadenar(params string[] contenidos)
    {
        var cadena = new List<EslabonDeCadena>();
        var anterior = CadenaDeHash.Origen;

        foreach (var contenido in contenidos)
        {
            anterior = CadenaDeHash.Calcular(anterior, contenido);
            cadena.Add(new EslabonDeCadena(contenido, anterior));
        }

        return cadena;
    }
}
