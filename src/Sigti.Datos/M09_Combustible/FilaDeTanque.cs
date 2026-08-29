using Sigti.Dominio.M09_Combustible;

namespace Sigti.Datos.M09_Combustible;

/// <summary>El tanque institucional, tal como se guarda — `RN-83` punto 5.</summary>
public sealed class FilaDeTanque
{
    public required Ulid Id { get; init; }

    public required string Nombre { get; init; }

    /// <summary>Dónde está físicamente. El tanque no despacha a otra delegación.</summary>
    public required string AmbitoDeclarado { get; init; }

    /// <summary>
    /// Un tanque, un combustible. No es decoración: sin esto, un asiento puede llenar un camión
    /// diésel desde el tanque de gasolina y cuadrar en galones.
    /// </summary>
    public required string TipoDeCombustible { get; init; }

    public decimal? CapacidadGalones { get; init; }

    public List<FilaDeMovimientoDeExistencias> Movimientos { get; } = [];
}

/// <summary>
/// Un asiento del libro de existencias — `E-01` a `E-06`.
///
/// <b>No hay columna de existencia.</b> La existencia es la suma de estas filas (P-1 aplicado a
/// una cantidad): una columna <c>existencia_actual</c> se desincroniza el primer día en que dos
/// despachos entren a la vez, y desde ahí el arqueo compara la realidad contra un número que ya
/// no es la suma de nada.
/// </summary>
public sealed class FilaDeMovimientoDeExistencias
{
    public required Ulid Id { get; init; }

    public required Ulid TanqueId { get; init; }

    public required int Orden { get; init; }

    public required string Movimiento { get; init; }

    public required TipoDeMovimiento Tipo { get; init; }

    /// <summary>
    /// Positivo salvo en el ajuste, que es el único que va en las dos direcciones. El signo de
    /// los demás lo pone <see cref="Tipo"/>.
    /// </summary>
    public required decimal Galones { get; init; }

    public required string Persona { get; init; }

    public required string Puesto { get; init; }

    public required DateOnly FechaDelHecho { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    public required string Motivo { get; init; }

    /// <summary>A qué vehículo se despachó — `E-02`. Lo que imputa el galón a una placa.</summary>
    public Ulid? VehiculoId { get; init; }

    public Ulid? MisionId { get; init; }

    /// <summary>
    /// El abastecimiento que este despacho respalda. <b>Indexado</b>: la pregunta «¿qué galones
    /// dicen haber salido del tanque sin que el tanque los registre?» se hace por acá, y es el
    /// préstamo invisible de `CE-23` vuelto consulta.
    /// </summary>
    public Ulid? AbastecimientoId { get; init; }

    /// <summary>El otro tanque, en el trasiego. Es lo que permite cuadrar los dos lados.</summary>
    public Ulid? ContraparteId { get; init; }

    /// <summary>Lo que dio la medición física — `E-05`. Nulo en todo lo demás.</summary>
    public decimal? ExistenciaMedida { get; init; }

    public MotivoDeAjuste? MotivoDelAjuste { get; init; }

    public string? Comprobante { get; init; }
}
