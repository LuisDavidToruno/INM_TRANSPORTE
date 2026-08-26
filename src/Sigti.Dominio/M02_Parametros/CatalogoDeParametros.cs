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
    public ValorResuelto Resolver(string clave, DateOnly fechaDelHecho, DateTimeOffset conocidoAl)
    {
        var version = versiones
            .Where(v => v.Clave == clave)
            .Where(v => v.EstaAprobada)
            .Where(v => v.RegiaEl(fechaDelHecho))
            .Where(v => v.EraConocidaAl(conocidoAl))
            // Si dos versiones aprobadas se solapan, manda la de vigencia más reciente.
            // No debería ocurrir —`HU-144` no admite solapes— y aun así se resuelve de
            // forma determinista en lugar de depender del orden de la consulta.
            .OrderByDescending(v => v.VigenteDesde)
            .FirstOrDefault()
            ?? throw new ParametroSinVigencia(clave, fechaDelHecho);

        return new ValorResuelto(clave, version.Valor, fechaDelHecho, conocidoAl, version.VigenteDesde);
    }
}
