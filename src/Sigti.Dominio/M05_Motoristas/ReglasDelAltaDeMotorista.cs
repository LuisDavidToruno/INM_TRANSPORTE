namespace Sigti.Dominio.M05_Motoristas;

/// <summary>Lo que se pide para incorporar un motorista al padrón.</summary>
public sealed record AltaDeMotorista
{
    public required string Nombre { get; init; }

    /// <summary>
    /// Si figura en el padrón de motoristas de la institución. <b>No decide si puede conducir</b>
    /// — eso lo decide la licencia (`RN-57`).
    /// </summary>
    public required bool EsDelPadron { get; init; }

    public required string NumeroDeLicencia { get; init; }

    /// <summary>Una de las <b>nueve</b> del Artículo 4 del Acuerdo 1012-2021.</summary>
    public required CategoriaDeLicencia Categoria { get; init; }

    public required DateOnly VenceLicencia { get; init; }

    public string? Restricciones { get; init; }
}

/// <summary>Por qué no procede el alta de un motorista.</summary>
public enum MotivoDeRechazoDelMotorista
{
    SinNombre,

    /// <summary>Es lo que se cita ante un retén. Sin él no hay a qué referirse.</summary>
    SinNumeroDeLicencia,
}

/// <summary>Lo que el alta admite y declara, sin rechazarlo.</summary>
public enum ObservacionDelMotorista
{
    /// <summary>
    /// La licencia ya está vencida al día del alta. <b>Se registra igual</b>: el padrón es el
    /// censo de quién conduce, no la lista de quién puede salir hoy.
    /// </summary>
    LicenciaVencidaAlAlta,
}

/// <summary>Lo que el alta contesta.</summary>
public sealed record ResultadoDelAltaDeMotorista(
    bool Procede,
    IReadOnlyList<MotivoDeRechazoDelMotorista> Reparos,
    IReadOnlyList<ObservacionDelMotorista> Observaciones);

/// <summary>Las reglas del alta de un motorista en el padrón.</summary>
public static class ReglasDelAltaDeMotorista
{
    public static ResultadoDelAltaDeMotorista Evaluar(AltaDeMotorista alta, DateOnly hoy)
    {
        var reparos = new List<MotivoDeRechazoDelMotorista>();

        if (string.IsNullOrWhiteSpace(alta.Nombre))
            reparos.Add(MotivoDeRechazoDelMotorista.SinNombre);

        if (string.IsNullOrWhiteSpace(alta.NumeroDeLicencia))
            reparos.Add(MotivoDeRechazoDelMotorista.SinNumeroDeLicencia);

        var observaciones = new List<ObservacionDelMotorista>();

        if (alta.VenceLicencia < hoy)
            observaciones.Add(ObservacionDelMotorista.LicenciaVencidaAlAlta);

        return new ResultadoDelAltaDeMotorista(reparos.Count == 0, reparos, observaciones);
    }
}
