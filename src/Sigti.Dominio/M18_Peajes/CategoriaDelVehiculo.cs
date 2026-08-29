using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.Reglas;

namespace Sigti.Dominio.M18_Peajes;

/// <summary>
/// Una fila de la matriz <c>derivacion_categoria_peaje</c> — `RN-33`.
///
/// ── Es una tabla cargada, no una fórmula ────────────────────────────────────
/// `RN-33` punto 5 y `NRM-10`: <i>«la progresión de tarifas por eje no se implementa como
/// fórmula; una fórmula inferida se vuelve falsa al primer ajuste asimétrico»</i>. Y el criterio
/// legal —el Artículo 51 de la Ley de Tránsito— <b>no se pudo transcribir</b>: el PDF oficial es
/// un escaneo sin capa de texto (insumo #23, `[C]`).
///
/// Por eso las condiciones se cargan y la matriz se marca provisional. <b>No se inventa ningún
/// criterio de corte.</b>
///
/// ── Por qué los criterios son anulables ─────────────────────────────────────
/// Nulo es <b>«esta fila no mira ese atributo»</b>. Una fila que resuelve por tipo de vehículo
/// no debe exigir peso; una que separa por ejes no debe exigir pasajeros. Forzar todos los
/// criterios obligaría a inventar rangos que la norma no fija.
/// </summary>
/// <param name="Prioridad">
/// Cuál gana cuando dos filas aplican. Menor número, antes. Existe porque la clasificación
/// tiene <b>excepciones nominales</b>: la resolución de la SAPP del 17/09/2025 saca a Hyundai
/// H-100, Kia K2700 y Sprinter de la categoría a la que su peso los llevaría, y esa fila tiene
/// que ganarle a la general.
/// </param>
public sealed record ReglaDeCategoria(
    Ulid Id,
    string Categoria,
    int Prioridad,
    string Fundamento,
    ClaseNormativa? Clase = null,
    string? TipoDeVehiculo = null,
    int? PesoBrutoDesdeKg = null,
    int? PesoBrutoHastaKg = null,
    int? EjesDesde = null,
    int? EjesHasta = null,
    int? PasajerosDesde = null,
    int? PasajerosHasta = null,
    bool? LlevaRemolque = null,
    DateOnly VigenteDesde = default,
    DateOnly? VigenteHasta = null,
    DateTimeOffset RegistradoDesde = default,
    DateTimeOffset? RegistradoHasta = null) : IConVigencia
{
    /// <summary>
    /// Qué atributos mira esta fila. Es lo que permite decir <b>cuál falta</b> cuando la ficha
    /// no lo trae, en vez de un «no se pudo resolver» sin destinatario.
    /// </summary>
    public IReadOnlyList<string> AtributosQueExige
    {
        get
        {
            var exige = new List<string>();

            if (Clase is not null) exige.Add("clase normativa");
            if (TipoDeVehiculo is not null) exige.Add("tipo de vehículo");
            if (PesoBrutoDesdeKg is not null || PesoBrutoHastaKg is not null)
                exige.Add("peso bruto vehicular");
            if (EjesDesde is not null || EjesHasta is not null) exige.Add("número de ejes");
            if (PasajerosDesde is not null || PasajerosHasta is not null)
                exige.Add("capacidad de pasajeros");
            if (LlevaRemolque is not null) exige.Add("condición de articulado");

            return exige;
        }
    }

    /// <summary>
    /// Si esta fila aplica a una ficha. <b>Nulo en el número de ejes hace que la fila que los
    /// exige NO aplique</b> —no que aplique por omisión—, y eso es lo que produce la categoría
    /// no resuelta de `RN-33` punto 3 en vez de una adivinada.
    /// </summary>
    public bool AplicaA(FichaTecnica ficha)
    {
        if (Clase is { } clase && ficha.Clase != clase) return false;

        if (TipoDeVehiculo is { } tipo &&
            !string.Equals(ficha.TipoDeVehiculo, tipo, StringComparison.OrdinalIgnoreCase))
            return false;

        if (PesoBrutoDesdeKg is { } desdePeso && ficha.PesoBrutoKg < desdePeso) return false;
        if (PesoBrutoHastaKg is { } hastaPeso && ficha.PesoBrutoKg > hastaPeso) return false;

        if (EjesDesde is not null || EjesHasta is not null)
        {
            if (ficha.NumeroDeEjes is not { } ejes) return false;
            if (EjesDesde is { } desdeEjes && ejes < desdeEjes) return false;
            if (EjesHasta is { } hastaEjes && ejes > hastaEjes) return false;
        }

        if (PasajerosDesde is { } desdePax && ficha.CapacidadPasajeros < desdePax) return false;
        if (PasajerosHasta is { } hastaPax && ficha.CapacidadPasajeros > hastaPax) return false;

        if (LlevaRemolque is { } remolque && ficha.LlevaRemolque != remolque) return false;

        return true;
    }
}

/// <summary>
/// Sobre qué se derivó la categoría — `RN-33`, las <b>dos</b> derivaciones distintas.
///
/// El estimado tiene que decir cuál usó: `RN-33` — <i>«un estimado que no dice sobre qué base se
/// calculó no se puede defender ante quien lo autorizó»</i>.
/// </summary>
public enum BaseDeLaCategoria
{
    /// <summary>
    /// La ficha técnica de la unidad concreta. Es la que se compara con lo cobrado en caseta
    /// (`RN-36`), y la única que vale para programar (`BD-07`).
    /// </summary>
    VehiculoAsignado,

    /// <summary>
    /// La categoría declarada en el catálogo de tipos de vehículo (M-02), para la estimación de
    /// `T-02` cuando <b>todavía no hay unidad asignada</b>.
    ///
    /// Es dato de catálogo cargado, no una fórmula. Si el tipo agrupa unidades de categorías
    /// distintas, declara la más frecuente y se marca <b>estimativa</b>.
    /// </summary>
    TipoDeVehiculoRequerido,

    /// <summary>
    /// Corrección manual de ACT-01 o ACT-04 con fundamento y adjunto — típicamente una
    /// resolución de la SAPP. <b>No se pierde al recalcular</b> (`RN-33` punto 4).
    /// </summary>
    CorreccionManual,
}

/// <summary>
/// El resultado de derivar. <b>O hay categoría, o hay una razón con nombre</b> — nunca una
/// categoría supuesta.
/// </summary>
/// <param name="Categoria">
/// Nula cuando no se resolvió. `RN-33` punto 3: <i>«si falta un atributo necesario, el sistema
/// no adivina»</i>.
/// </param>
/// <param name="Explicacion">
/// Qué atributos la determinaron, o qué faltó. `RN-33` punto 2: <i>«una categoría sin
/// explicación no se puede defender ante la SAPP ni ante un auditor»</i>.
/// </param>
/// <param name="Provisional">
/// La matriz que la produjo está marcada provisional porque el Artículo 51 no se pudo
/// transcribir (`[C]`, insumo #23). <b>Viaja con el resultado</b>: una categoría provisional que
/// se muestra igual que una firme se cita después como si lo fuera.
/// </param>
public sealed record CategoriaResuelta(
    CategoriaDePeaje? Categoria,
    BaseDeLaCategoria Base,
    string Explicacion,
    bool Provisional = false,
    string? AtributoQueFalta = null)
{
    public bool EstaResuelta => Categoria is not null;
}

/// <summary>
/// La derivación de `RN-33` — <b>nunca por número de ejes como única llave</b>.
/// </summary>
public static class ReglasDeCategoriaDePeaje
{
    /// <summary>
    /// Deriva la categoría del vehículo desde su ficha técnica.
    ///
    /// ── El error que esto existe para impedir ───────────────────────────────
    /// `NRM-10`, con evidencia: <i>«un vehículo liviano tiene 2 ejes y paga L. 22. Un "Vehículo
    /// de 2 Ejes" paga L. 90. Ambos tienen dos ejes»</i> `[V]`. Y la consecuencia, textual:
    /// <i>«cualquier modelo que use `numero_ejes` como única llave para resolver la tarifa está
    /// mal y va a cobrar cuatro veces de más a cada pickup de la flota»</i>.
    ///
    /// Por eso no hay ninguna aritmética acá: hay una tabla, y gana la fila de menor prioridad
    /// que aplique. Las excepciones nominales de la SAPP —H-100, K2700, Sprinter— son filas de
    /// prioridad alta, no casos especiales en el código.
    /// </summary>
    public static CategoriaResuelta Derivar(
        FichaTecnica ficha,
        IEnumerable<ReglaDeCategoria> matriz,
        IReadOnlyDictionary<string, string> nombresDeCategoria,
        DateOnly fechaDelHecho,
        DateTimeOffset conocidoAl,
        bool matrizProvisional)
    {
        var vigentes = ReglasDeVigencia
            .TodasLasVigentesA(matriz, fechaDelHecho, conocidoAl)
            .OrderBy(r => r.Prioridad)
            .ToList();

        if (vigentes.Count == 0)
            return new CategoriaResuelta(null, BaseDeLaCategoria.VehiculoAsignado,
                "No hay matriz de derivación cargada y vigente a esa fecha. La matriz de " +
                "`RN-33` es un catálogo que carga la institución: sin ella no se inventa " +
                "ningún criterio de corte (`[C]`, insumo #23 — el Artículo 51 es un escaneo " +
                "sin capa de texto).");

        var gana = vigentes.FirstOrDefault(r => r.AplicaA(ficha));

        if (gana is not null)
        {
            var nombre = nombresDeCategoria.TryGetValue(gana.Categoria, out var n)
                ? n
                : gana.Categoria;

            return new CategoriaResuelta(
                new CategoriaDePeaje(gana.Categoria, nombre),
                BaseDeLaCategoria.VehiculoAsignado,
                $"Resuelta por {string.Join(", ", gana.AtributosQueExige)}. {gana.Fundamento}",
                matrizProvisional);
        }

        // Ninguna fila aplicó. Si alguna exige el número de ejes y la ficha no lo trae, ése es
        // el dato que falta — y decirlo es lo que convierte un «no se pudo» en algo que alguien
        // puede ir a cargar.
        var falta = AtributoQueFalta(ficha, vigentes);

        return new CategoriaResuelta(
            null,
            BaseDeLaCategoria.VehiculoAsignado,
            falta is null
                ? "Ninguna fila de la matriz aplica a esta ficha técnica. La matriz no cubre " +
                  "este vehículo: hay que cargar la fila que lo clasifica, con su fundamento."
                : $"Falta {falta} en la ficha técnica del vehículo. Sin ese dato la matriz de " +
                  "`RN-33` no puede clasificarlo, y el sistema no adivina: una categoría " +
                  "supuesta cobra cuatro veces de más o de menos y nadie lo nota.",
            matrizProvisional,
            AtributoQueFalta: falta);
    }

    /// <summary>
    /// La categoría del <b>tipo</b> requerido, para la estimación previa de `T-02`.
    ///
    /// Antes de asignar el vehículo no hay ficha técnica contra la cual derivar —hallazgos
    /// `HB1-09` y `HN1-10`—, y la estimación se calcula igual. Esto es dato de catálogo, no una
    /// fórmula, y sale marcado como estimativo.
    /// </summary>
    public static CategoriaResuelta DelTipoRequerido(
        CategoriaDePeaje? delCatalogo, string tipoDeVehiculo, bool provisional = false) =>
        delCatalogo is null
            ? new CategoriaResuelta(null, BaseDeLaCategoria.TipoDeVehiculoRequerido,
                $"El tipo de vehículo «{tipoDeVehiculo}» no declara categoría de peaje en el " +
                "catálogo de M-02. El estimado previo no se puede calcular sobre una unidad " +
                "que todavía no existe ni sobre un supuesto.")
            : new CategoriaResuelta(delCatalogo, BaseDeLaCategoria.TipoDeVehiculoRequerido,
                $"Estimativa: categoría declarada para el tipo «{tipoDeVehiculo}», no de una " +
                "unidad concreta. Si el tipo agrupa unidades de categorías distintas, la " +
                "diferencia se resuelve al programar.",
                provisional);

    private static string? AtributoQueFalta(
        FichaTecnica ficha, IEnumerable<ReglaDeCategoria> vigentes)
    {
        if (ficha.NumeroDeEjes is null &&
            vigentes.Any(r => r.EjesDesde is not null || r.EjesHasta is not null))
            return "el número de ejes";

        return null;
    }
}
