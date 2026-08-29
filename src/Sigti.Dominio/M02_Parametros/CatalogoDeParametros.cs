using Sigti.Dominio.Reglas;

namespace Sigti.Dominio.M02_Parametros;

/// <summary>
/// No hay ninguna versión aprobada que rigiera en la fecha del hecho.
///
/// <b>Se bloquea, no se aproxima.</b> Tomar la vigencia más cercana o un valor por
/// omisión produciría un número plausible y equivocado, que nadie detectaría y que
/// acabaría en un reporte del Tribunal Superior de Cuentas (`HU-147`).
/// </summary>
public sealed class ParametroSinVigencia(string clave, DateOnly fechaDelHecho)
    : Exception($"No hay vigencia aprobada del parámetro '{clave}' para el {fechaDelHecho:yyyy-MM-dd}.")
{
    public string Clave { get; } = clave;
    public DateOnly FechaDelHecho { get; } = fechaDelHecho;
}

/// <summary>
/// El valor que rigió, con la evidencia de por qué. Lo que se guarda junto al cálculo
/// para que sea reproducible años después (`RNF-06`).
/// </summary>
public sealed record ValorResuelto(
    string Clave,
    string Valor,
    DateOnly FechaDelHecho,
    DateTimeOffset ConocidoAl,
    DateOnly VigenteDesde);

/// <summary>
/// Resuelve parámetros normativos <b>a la fecha del hecho</b>, no a la de captura
/// (`P-4`, `RNF-05`). Puro: sin base de datos, sin reloj — las dos fechas entran como
/// parámetro, que es exactamente la firma que `ADR-006` describe.
/// </summary>
public sealed class CatalogoDeParametros(IReadOnlyList<VersionDeParametro> versiones)
{
    /// <param name="fechaDelHecho">Cuándo ocurrió. Decide qué decía el reglamento.</param>
    /// <param name="conocidoAl">
    /// Desde qué momento se mira. Pasar el instante de la liquidación reproduce el número
    /// que se pagó; pasar el instante actual da el número corregido. <b>Son dos preguntas
    /// distintas y las dos son legítimas</b>, por eso hay dos ejes.
    /// </param>
    public ValorResuelto Resolver(string clave, DateOnly fechaDelHecho, DateTimeOffset conocidoAl) =>
        ResolverSiHay(clave, fechaDelHecho, conocidoAl)
            ?? throw new ParametroSinVigencia(clave, fechaDelHecho);

    /// <summary>
    /// Como <see cref="Resolver"/>, pero <b>devuelve nulo en vez de bloquear</b> cuando la
    /// institución no fijó la clave.
    ///
    /// ── Cuándo se usa esto y cuándo se usa el que bloquea ───────────────────
    /// <b>Bloquea</b> lo que decide un número que alguien va a cobrar o pagar: una tarifa
    /// ausente no se aproxima, porque produciría una cifra plausible y equivocada que nadie
    /// detectaría.
    ///
    /// <b>Devuelve nulo</b> lo que decide si un <i>reporte</i> se puede evaluar. Ahí el
    /// bloqueo sería peor que el vacío: impediría producir el documento entero por un
    /// parámetro que solo afecta a una de sus secciones, y la salida sería cablear un valor
    /// «razonable» — que es como se llega a que un control diga cero cuando lo que pasa es
    /// que nadie lo configuró.
    ///
    /// Quien llame a esto <b>tiene que declarar el nulo</b>, no tratarlo como cero. Es la
    /// misma disciplina de <c>RendimientoEsperadoDe</c> y del horario hábil.
    /// </summary>
    public ValorResuelto? ResolverSiHay(
        string clave, DateOnly fechaDelHecho, DateTimeOffset conocidoAl)
    {
        // Los dos ejes los resuelve ReglasDeVigencia, compartida con todo lo demás que
        // tiene vigencia. Lo propio de acá es el filtro de aprobación: una carga
        // pendiente no resuelve, o el doble control de `HU-145` sería decorativo.
        var version = ReglasDeVigencia.VigenteA(
            versiones.Where(v => v.Clave == clave && v.EstaAprobada),
            fechaDelHecho,
            conocidoAl);

        return version is null
            ? null
            : new ValorResuelto(clave, version.Valor, fechaDelHecho, conocidoAl, version.VigenteDesde);
    }
}
