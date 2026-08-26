using Sigti.Dominio.M03_Flota;

namespace Sigti.Dominio.M05_Motoristas;

/// <summary>
/// Qué habilita cada categoría, resuelto por los atributos de la ficha técnica.
///
/// Es un <b>parámetro normativo con vigencia</b> (`ADR-006`), no una tabla cableada: la
/// matriz oficial de la DNVT es insumo abierto `[C]`, y cuando llegue tiene que poder
/// cargarse sin recompilar. La versión se conserva porque `BD-02` exige registrar
/// <b>con qué versión de la matriz</b> se evaluó.
/// </summary>
public sealed class MatrizDeLicencias
{
    private readonly IReadOnlyList<EntradaDeMatriz> _entradas;

    private MatrizDeLicencias(DateOnly vigenteDesde, string version, IReadOnlyList<EntradaDeMatriz> entradas)
    {
        VigenteDesde = vigenteDesde;
        Version = version;
        _entradas = entradas;
    }

    public DateOnly VigenteDesde { get; }

    /// <summary>Identifica qué versión de la matriz respaldó una evaluación.</summary>
    public string Version { get; }

    public static MatrizDeLicencias Con(
        DateOnly vigenteDesde, string version, IReadOnlyList<EntradaDeMatriz> entradas) =>
        new(vigenteDesde, version, entradas);

    /// <summary>
    /// ¿Esta categoría habilita este vehículo?
    ///
    /// Una categoría sin entrada en la matriz <b>no habilita</b>. La ausencia se trata
    /// como negativa, nunca como permiso: si nadie declaró que la categoría A puede
    /// conducir un camión, no puede.
    /// </summary>
    public bool Habilita(CategoriaDeLicencia categoria, FichaTecnica ficha) =>
        _entradas.Any(e =>
            e.Categoria == categoria &&
            ficha.PesoBrutoKg <= e.PesoBrutoMaximoKg &&
            ficha.CapacidadPasajeros <= e.CapacidadMaximaPasajeros &&
            (!ficha.EsArticulado || e.PermiteArticulado));
}

/// <summary>Un renglón de la matriz: hasta dónde llega una categoría.</summary>
public sealed record EntradaDeMatriz(
    CategoriaDeLicencia Categoria,
    int PesoBrutoMaximoKg,
    int CapacidadMaximaPasajeros,
    bool PermiteArticulado);
