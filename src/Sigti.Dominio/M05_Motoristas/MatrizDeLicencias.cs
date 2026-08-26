using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.Reglas;

namespace Sigti.Dominio.M05_Motoristas;

/// <summary>
/// Un renglón de la matriz: hasta dónde llega una categoría, y <b>desde cuándo</b>.
///
/// Lleva los dos ejes de `ADR-006` como cualquier otro parámetro normativo. La matriz no
/// es una constante del programa: es reglamento, y el reglamento cambia.
/// </summary>
/// <param name="Clase">
/// Qué clase de vehículo cubre esta entrada. Es lo que permite expresar `A` y `B1`, que
/// el Artículo 4 define por clase y no por umbral, y lo que impide que una licencia `B`
/// habilite una motocicleta — texto de la norma: «automóviles livianos <b>no comprendidos
/// en la categoría A y B1</b>».
/// </param>
public sealed record EntradaDeMatriz(
    CategoriaDeLicencia Categoria,
    ClaseNormativa Clase,
    int PesoBrutoMaximoKg,
    int CapacidadMaximaPasajeros,
    bool PermiteRemolque,
    DateOnly VigenteDesde,
    DateOnly? VigenteHasta,
    DateTimeOffset RegistradoDesde,
    DateTimeOffset? RegistradoHasta) : IConVigencia;

/// <summary>
/// Qué habilita cada categoría, resuelto por los atributos de la ficha técnica y
/// <b>a la fecha del hecho</b>.
///
/// Es un catálogo con vigencia (`M-02`), no una tabla cableada: la matriz oficial de la
/// DNVT es insumo abierto `[C]`, y cuando llegue tiene que poder cargarse sin recompilar.
/// La versión se conserva porque `BD-02` exige registrar <b>con qué versión de la matriz</b>
/// se evaluó.
/// </summary>
public sealed class MatrizDeLicencias
{
    private readonly IReadOnlyList<EntradaDeMatriz> _entradas;

    private MatrizDeLicencias(string version, IReadOnlyList<EntradaDeMatriz> entradas)
    {
        Version = version;
        _entradas = entradas;
    }

    /// <summary>Identifica qué versión de la matriz respaldó una evaluación.</summary>
    public string Version { get; }

    public static MatrizDeLicencias Con(string version, IReadOnlyList<EntradaDeMatriz> entradas) =>
        new(version, entradas);

    /// <summary>
    /// ¿Esta categoría habilita este vehículo, según lo que el reglamento decía en la
    /// fecha del hecho?
    ///
    /// Una categoría <b>sin entrada vigente no habilita</b>. La ausencia se trata como
    /// negativa, nunca como permiso: si nadie declaró que la categoría A puede conducir
    /// un camión, no puede — y si la entrada que lo permitía dejó de estar vigente,
    /// tampoco.
    /// </summary>
    public bool Habilita(
        CategoriaDeLicencia categoria, FichaTecnica ficha, DateOnly fechaDelHecho, DateTimeOffset conocidoAl) =>
        ReglasDeVigencia
            .TodasLasVigentesA(_entradas.Where(e => e.Categoria == categoria), fechaDelHecho, conocidoAl)
            .Any(e =>
                e.Clase == ficha.Clase &&
                ficha.PesoBrutoKg <= e.PesoBrutoMaximoKg &&
                ficha.CapacidadPasajeros <= e.CapacidadMaximaPasajeros &&
                (!ficha.LlevaRemolque || e.PermiteRemolque));
}
