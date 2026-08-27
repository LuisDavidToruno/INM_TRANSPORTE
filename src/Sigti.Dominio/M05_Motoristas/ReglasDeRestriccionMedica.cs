namespace Sigti.Dominio.M05_Motoristas;

public enum EfectoDeRestriccion
{
    Ninguno,
    Advertencia,
    Bloqueo
}

public sealed record RestriccionTipificada(
    string Codigo,
    string CondicionQueEvalua,
    EfectoDeRestriccion Efecto);

public sealed record CatalogoDeRestricciones(IReadOnlyList<RestriccionTipificada> Entradas);

public sealed record ResultadoDeRestricciones(
    EfectoDeRestriccion Efecto,
    string? RestriccionEnConflicto,
    string? CondicionQueLaActiva);

public static class ReglasDeRestriccionMedica
{
    public static ResultadoDeRestricciones Evaluar(
        Licencia licencia,
        IReadOnlyList<string> condicionesDeclaradas,
        CatalogoDeRestricciones catalogo)
    {
        foreach (var restriccion in licencia.Restricciones)
        {
            var entrada = catalogo.Entradas.FirstOrDefault(
                e => string.Equals(e.Codigo, restriccion, StringComparison.OrdinalIgnoreCase)
                     && condicionesDeclaradas.Contains(e.CondicionQueEvalua, StringComparer.OrdinalIgnoreCase));

            if (entrada is not null)
                return new ResultadoDeRestricciones(entrada.Efecto, restriccion, entrada.CondicionQueEvalua);

            var tipificada = catalogo.Entradas.Any(
                e => string.Equals(e.Codigo, restriccion, StringComparison.OrdinalIgnoreCase));

            if (!tipificada)
                return new ResultadoDeRestricciones(EfectoDeRestriccion.Advertencia, restriccion, null);
        }

        return new ResultadoDeRestricciones(EfectoDeRestriccion.Ninguno, null, null);
    }
}
