using Sigti.Dominio.M03_Flota;

namespace Sigti.Datos.M03_Flota;

/// <summary>
/// Un acta de entrega-recepción del vehículo — `RN-22`, `NRM-02`.
///
/// ── La pregunta que esto conserva ───────────────────────────────────────────
/// <i>«¿Quién tenía el vehículo en ese momento, y con qué?»</i> Aparece cuando algo falta o algo
/// se daña, y sin cadena de custodia <b>la deducción de responsabilidad no tiene sobre quién
/// recaer</b> — lo que ante el TSC agrava en vez de atenuar.
/// </summary>
public sealed class FilaDeActaDeCustodia
{
    public required Ulid Id { get; init; }
    public required Ulid MisionId { get; init; }
    public required Ulid VehiculoId { get; init; }

    /// <summary>
    /// <c>Entrega</c> al despachar, <c>Devolucion</c> al retornar.
    ///
    /// <b>Una por misión de cada clase.</b> Dos entregas dejarían dos inventarios distintos del
    /// mismo vehículo, y el cotejo del retorno no sabría contra cuál correr.
    /// </summary>
    public required TipoDeActa Tipo { get; init; }

    public required string Entrega { get; init; }
    public required string Recibe { get; init; }

    public required DateTime MomentoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }

    public required int Odometro { get; init; }

    /// <summary>
    /// En fracción de tanque. <b>Nulo es que no se leyó</b>, no cero: cero es un tanque vacío, y
    /// entregar un vehículo vacío es una afirmación distinta de no haber mirado el indicador.
    /// </summary>
    public decimal? NivelDeTanque { get; init; }

    /// <summary>
    /// Cómo está la unidad. <b>Obligatorio</b>: es lo que después distingue un golpe que ya
    /// venía de uno que ocurrió en la misión.
    /// </summary>
    public required string EstadoDeLaUnidad { get; init; }

    public string? Observaciones { get; init; }

    public List<FilaDeElementoDelActa> Elementos { get; } = [];
}

/// <summary>
/// Un accesorio o herramienta consignado en el acta — `RN-22`.
///
/// ── Por qué es una fila y no un texto libre ─────────────────────────────────
/// Porque el <b>cotejo</b> es el producto del acta: dos párrafos descriptivos no se pueden
/// comparar, y el gato que no volvió se pierde entre las palabras. Con filas, el faltante tiene
/// nombre.
/// </summary>
public sealed class FilaDeElementoDelActa
{
    public required Ulid Id { get; init; }
    public required Ulid ActaId { get; init; }

    public required string Nombre { get; init; }

    /// <summary>
    /// Si está. En la entrega, falso es <b>que se entregó sin él</b>; en la devolución, que
    /// <b>no volvió</b>.
    ///
    /// ⚠️ No es lo mismo que no listarlo: un elemento ausente de la lista <b>nunca se miró</b>.
    /// El gato que nadie anotó y el gato que no volvió son dos situaciones distintas.
    /// </summary>
    public required bool Presente { get; init; }

    public string? Observacion { get; init; }
}
