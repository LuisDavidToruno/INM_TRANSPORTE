using Sigti.Dominio.M09_Combustible;

namespace Sigti.Datos.M09_Combustible;

/// <summary>
/// Un ingreso de combustible al tanque, tal como se guarda — `RN-83`.
///
/// ── Por qué cuelga del vehículo y no de la misión ───────────────────────────
/// Porque `RN-83` aplica <i>«a todo vehículo de la flota, en misión o fuera de ella»</i>. Un
/// reabastecimiento de rutina en el predio no tiene misión, y colgarlo de una obligaría a
/// inventar el expediente al que pertenece.
/// </summary>
public sealed class FilaDeAbastecimiento
{
    public required Ulid Id { get; init; }

    public required Ulid VehiculoId { get; init; }

    /// <summary>La fecha del <b>hecho</b> — P-4. Puede llegar días después por sincronización.</summary>
    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    public required decimal Galones { get; init; }

    public required int Odometro { get; init; }

    public required FuenteDeAbastecimiento Fuente { get; init; }

    public required string Registra { get; init; }

    /// <summary>Nula en el reabastecimiento sin misión.</summary>
    public Ulid? MisionId { get; init; }

    /// <summary>
    /// El vale del que salió. <b>Sólo con fuente del fondo.</b>
    ///
    /// Es lo que impide contar el galón dos veces: el asiento `V-04` del vale y esta fila son
    /// <b>el mismo hecho</b> visto desde dos lados, no dos hechos. El índice único sobre esta
    /// columna junto con el asiento lo garantiza en la base, no en una comprobación que se
    /// puede olvidar.
    /// </summary>
    public Ulid? AsignacionId { get; init; }

    /// <summary>
    /// El asiento `V-04` que produjo este abastecimiento. <b>Único</b>: dos filas apuntando al
    /// mismo asiento serían el mismo galón contado dos veces en el denominador de `RN-30`.
    /// </summary>
    public Ulid? TransicionDelValeId { get; init; }

    public decimal? Monto { get; init; }

    public string? Estacion { get; init; }

    public string? Comprobante { get; init; }

    public string? CausaSinComprobante { get; init; }

    public required bool Excedido { get; init; }

    /// <summary>
    /// El identificador que puso el dispositivo de campo (`ADR-005`).
    ///
    /// <b>Es lo que hace inofensivo el reenvío.</b> El dispositivo que no supo si el servidor
    /// recibió va a reintentar, y un galón contado dos veces infla el denominador de `RN-30`:
    /// produce una desviación inventada por el propio sistema, que es peor que no detectar
    /// ninguna.
    ///
    /// <b>Nulo cuando el hecho nació en la oficina</b>, contra la API y con red: ahí no hubo
    /// captura diferida que reconciliar.
    /// </summary>
    public Ulid? IdDeCaptura { get; init; }
}
