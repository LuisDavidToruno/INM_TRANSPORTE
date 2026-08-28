using Sigti.Dominio.M09_Combustible;

namespace Sigti.Datos.M09_Combustible;

/// <summary>El vale, tal como se guarda.</summary>
public sealed class FilaDeAsignacion
{
    public required Ulid Id { get; init; }

    /// <summary>
    /// `RN-27` requisito 1: <b>único en la institución y no reciclable</b>. La unicidad la
    /// impone un índice, no una comprobación que se puede olvidar al agregar el próximo
    /// endpoint — es la misma razón por la que `IdDeCaptura` la tiene.
    /// </summary>
    public required string Folio { get; init; }

    public required Ulid FondoId { get; init; }

    public required Ulid MisionId { get; init; }

    public required Ulid VehiculoId { get; init; }

    /// <summary>El ULID del motorista en el padrón — no una identidad de persona.</summary>
    public required Ulid Receptor { get; init; }

    public required decimal Monto { get; init; }

    /// <summary>Nulo cuando el instrumento se expresa en dinero y no en galones.</summary>
    public decimal? Galones { get; init; }

    public required string Instrumento { get; init; }

    public required string TipoDeCombustible { get; init; }

    public List<FilaDeTransicionDeAsignacion> Transiciones { get; } = [];
}

/// <summary>Un asiento del diario del vale — `V-01` a `V-10`.</summary>
public sealed class FilaDeTransicionDeAsignacion
{
    public required Ulid Id { get; init; }

    public required Ulid AsignacionId { get; init; }

    public required int Orden { get; init; }

    public required string Transicion { get; init; }

    public required EstadoDeAsignacion Destino { get; init; }

    public required string Ejecuta { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    /// <summary>
    /// El identificador que puso el dispositivo de campo. <b>Acá pesa más que en la misión</b>:
    /// `V-04` se ejecuta sin conectividad y el dispositivo reintenta hasta que le contesten.
    /// Sin esto, el reintento duplica un consumo — y un galón contado dos veces es una
    /// desviación de conciliación inventada por el propio sistema.
    /// </summary>
    public Ulid? IdDeCaptura { get; init; }

    public string? Motivo { get; init; }

    // ── El consumo, desglosado ──────────────────────────────────────────────
    // Va en columnas y no dentro del motivo porque la liquidación y `RN-30` los vuelven a
    // sumar. Sacar un número de una cadena de texto es el error que ya se corrigió una vez
    // en la reserva de `T-08`.

    public decimal? ConsumoGalones { get; init; }

    public decimal? ConsumoMonto { get; init; }

    public string? ConsumoEstacion { get; init; }

    /// <summary>El odómetro del momento de la carga — lo que ancla el galón a un tramo.</summary>
    public int? ConsumoOdometro { get; init; }

    /// <summary>
    /// Nulo es un caso previsto: `RN-85` tipifica la ausencia de comprobante con causa y
    /// descargo alternativo. <b>El registro del abastecimiento no se omite nunca por falta de
    /// papel</b>, y por eso esta columna admite nulo en lugar de bloquear la fila entera.
    /// </summary>
    public string? ConsumoComprobante { get; init; }

    /// <summary>
    /// Por qué no hubo comprobante. Va en su propia columna y no dentro del motivo porque
    /// <b>alguien la va a consultar en bloque</b>: «cuántos abastecimientos del trimestre no
    /// tienen factura, y por qué» es una pregunta de auditoría, no una lectura caso por caso.
    /// </summary>
    public string? ConsumoCausaSinComprobante { get; init; }

    /// <summary>Lo que este asiento devolvió al fondo — `V-03`, `V-05` y `V-07`.</summary>
    public decimal? Devuelto { get; init; }
}
