namespace Sigti.Dominio.M03_Flota;

/// <summary>Lo que se pide para incorporar un vehículo a la flota.</summary>
public sealed record AltaDeVehiculo
{
    public required string Siglas { get; init; }
    public required string TipoDeVehiculo { get; init; }
    public required ClaseNormativa Clase { get; init; }
    public required int PesoBrutoKg { get; init; }
    public required int CapacidadPasajeros { get; init; }
    public required bool LlevaRemolque { get; init; }
    public required DateOnly VenceMatricula { get; init; }

    /// <summary>
    /// Nulo cuando el vehículo no tiene número de placa asignado. <b>Es válido y es común</b>:
    /// hay desabastecimiento nacional de láminas.
    /// </summary>
    public string? Placa { get; init; }

    public EstadoDePlaca EstadoDePlaca { get; init; } = EstadoDePlaca.ConLamina;

    public int? NumeroDeEjes { get; init; }
}

/// <summary>Por qué no procede un alta.</summary>
public enum MotivoDeRechazoDelAlta
{
    SinSiglas,

    /// <summary>Se declaró lámina puesta y no se dio el número. La lámina <b>es</b> el número.</summary>
    LaminaSinNumero,
}

/// <summary>
/// Lo que el alta admite <b>y declara</b>, sin rechazarlo.
///
/// ── Por qué esto no son reparos ─────────────────────────────────────────────
/// Porque el dato que falta se puede conseguir después, y negar el alta por él dejaría al
/// vehículo fuera del sistema — que es peor que tenerlo dentro con una carencia anotada.
/// Callarlo sí sería un defecto: nadie se entera hasta que hace falta.
/// </summary>
public enum ObservacionDelAlta
{
    /// <summary>
    /// Falta el número de ejes y con él la categoría de peaje — `RN-33`.
    /// <see cref="M18_Peajes.CategoriaDelVehiculo"/> admite el nulo y estima; acá se avisa.
    /// </summary>
    CategoriaDePeajeSinResolver,
}

/// <summary>Lo que el alta contesta.</summary>
public sealed record ResultadoDelAltaDeVehiculo(
    bool Procede,
    IReadOnlyList<MotivoDeRechazoDelAlta> Reparos,
    IReadOnlyList<ObservacionDelAlta> Observaciones);

/// <summary>Las reglas del alta de un vehículo.</summary>
public static class ReglasDelAltaDeVehiculo
{
    public static ResultadoDelAltaDeVehiculo Evaluar(AltaDeVehiculo alta, DateOnly hoy)
    {
        var reparos = new List<MotivoDeRechazoDelAlta>();

        if (string.IsNullOrWhiteSpace(alta.Siglas)) reparos.Add(MotivoDeRechazoDelAlta.SinSiglas);

        if (alta.EstadoDePlaca == EstadoDePlaca.ConLamina && string.IsNullOrWhiteSpace(alta.Placa))
            reparos.Add(MotivoDeRechazoDelAlta.LaminaSinNumero);

        var observaciones = new List<ObservacionDelAlta>();

        if (alta.NumeroDeEjes is null)
            observaciones.Add(ObservacionDelAlta.CategoriaDePeajeSinResolver);

        return new ResultadoDelAltaDeVehiculo(reparos.Count == 0, reparos, observaciones);
    }
}
