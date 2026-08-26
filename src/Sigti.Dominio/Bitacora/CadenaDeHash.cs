using System.Security.Cryptography;
using System.Text;

namespace Sigti.Dominio.Bitacora;

/// <summary>
/// El cálculo del hash de la cadena de auditoría. Es puro: dado el hash anterior y el
/// contenido, produce el siguiente. Sin base de datos, sin reloj, sin red.
///
/// Vive en el dominio precisamente para que la verificación de la cadena se pueda
/// probar sin base de datos, que es lo que una auditoría necesita poder hacer
/// (ADR-009). La serialización de la escritura vive en Sigti.Datos.
/// </summary>
public static class CadenaDeHash
{
    /// <summary>Eslabón cero. Contra este se encadena el primer asiento de una cadena.</summary>
    public const string Origen = "0000000000000000000000000000000000000000000000000000000000000000";

    public static string Calcular(string hashAnterior, string contenido)
    {
        var bytes = Encoding.UTF8.GetBytes(hashAnterior + contenido);
        return Convert.ToHexStringLower(SHA256.HashData(bytes));
    }

    /// <summary>
    /// Recalcula la cadena desde el origen y la compara contra los hashes guardados.
    /// Es lo que ejecuta una auditoría sobre lo que extrajo de la base.
    /// </summary>
    public static bool Verificar(IEnumerable<EslabonDeCadena> cadena)
    {
        var anterior = Origen;

        foreach (var eslabon in cadena)
        {
            anterior = Calcular(anterior, eslabon.Contenido);
            if (anterior != eslabon.Hash) return false;
        }

        return true;
    }

    /// <summary>
    /// Verifica la cadena contra el sello anclado fuera del alcance de quien administra
    /// la base (RNF-04). Es la única forma de detectar que se truncó la cola: una cadena
    /// truncada es internamente impecable, y nada dentro de ella delata lo que falta.
    /// </summary>
    public static bool Verificar(IReadOnlyList<EslabonDeCadena> cadena, string selloAnclado)
    {
        if (!Verificar(cadena)) return false;

        var ultimo = cadena.Count == 0 ? Origen : cadena[^1].Hash;
        return ultimo == selloAnclado;
    }
}

/// <summary>Un asiento reducido a lo que la verificación necesita: qué dice y qué hash quedó guardado.</summary>
public sealed record EslabonDeCadena(string Contenido, string Hash);
