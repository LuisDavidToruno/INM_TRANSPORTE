namespace Sigti.Dominio.Reglas;

/// <summary>
/// Lo que tiene los dos ejes de tiempo de `ADR-006`.
///
/// Está en `Reglas/` y no en un módulo porque lo implementan cosas de módulos distintos:
/// tarifas de peaje (`M-18`), umbrales de consumo (`M-09`), feriados y la matriz
/// licencia↔vehículo (`M-05`). <b>La carpeta se llama `Reglas/` y no `Comun/`</b>: «común»
/// es el nombre al que las cosas van a la deriva, y nadie lo abre a preguntarse si la
/// regla ya está ahí.
/// </summary>
public interface IConVigencia
{
    /// <summary>Eje normativo: desde cuándo lo decía el reglamento.</summary>
    DateOnly VigenteDesde { get; }

    /// <summary>Nulo mientras es la versión normativa vigente.</summary>
    DateOnly? VigenteHasta { get; }

    /// <summary>Eje de transacción: desde cuándo el sistema lo sabía.</summary>
    DateTimeOffset RegistradoDesde { get; }

    /// <summary>Nulo mientras es lo que el sistema cree hoy.</summary>
    DateTimeOffset? RegistradoHasta { get; }
}

/// <summary>
/// El eje normativo de `RNF-05`, en un solo lugar.
///
/// Existe para que ningún módulo implemente un eje y suponga que el otro viene puesto,
/// que es el fallo concreto que `ADR-006` advierte. Es pura: las dos fechas entran como
/// parámetro, sin reloj adentro.
/// </summary>
public static class ReglasDeVigencia
{
    /// <param name="fechaDelHecho">Cuándo ocurrió. Decide qué decía el reglamento.</param>
    /// <param name="conocidoAl">
    /// Desde qué momento se mira. El instante de la liquidación reproduce el número que
    /// se pagó; el instante actual da el corregido. Son dos preguntas legítimas.
    /// </param>
    /// <returns>
    /// La versión que regía, o <b>nulo</b> si no hay ninguna. No se aproxima a la más
    /// cercana: quien llama decide si la ausencia es bloqueo o advertencia, y la regla
    /// no adivina por él.
    /// </returns>
    public static T? VigenteA<T>(
        IEnumerable<T> versiones, DateOnly fechaDelHecho, DateTimeOffset conocidoAl)
        where T : class, IConVigencia =>
        versiones
            .Where(v => Regia(v, fechaDelHecho) && EraConocida(v, conocidoAl))
            // Si dos versiones se solapan —que no deberían—, manda la de vigencia más
            // reciente. Determinista, en vez de depender del orden de la consulta.
            .OrderByDescending(v => v.VigenteDesde)
            .FirstOrDefault();

    /// <summary>Todas las versiones que regían, para catálogos que devuelven varias filas.</summary>
    public static IReadOnlyList<T> TodasLasVigentesA<T>(
        IEnumerable<T> versiones, DateOnly fechaDelHecho, DateTimeOffset conocidoAl)
        where T : IConVigencia =>
        versiones.Where(v => Regia(v, fechaDelHecho) && EraConocida(v, conocidoAl)).ToList();

    private static bool Regia(IConVigencia v, DateOnly fechaDelHecho) =>
        v.VigenteDesde <= fechaDelHecho && (v.VigenteHasta is null || fechaDelHecho <= v.VigenteHasta);

    private static bool EraConocida(IConVigencia v, DateTimeOffset instante) =>
        v.RegistradoDesde <= instante && (v.RegistradoHasta is null || instante < v.RegistradoHasta);
}
