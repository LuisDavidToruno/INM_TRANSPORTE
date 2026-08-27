namespace Sigti.Datos;

/// <summary>
/// El <b>rastro</b> de un adjunto. No el adjunto.
///
/// `ADR-004`: el archivo vive en el sistema de archivos y la base guarda ruta, hash,
/// tipo, tamaño y clasificación. La aritmética que lo decidió son ≈ 8 GB anuales de
/// datos relacionales contra ≈ 30 GB de adjuntos — meter los binarios acá cuadruplica
/// el respaldo y saca la restauración de las 2 h que `RNF-09` exige.
/// </summary>
public sealed class FilaDeAdjunto
{
    /// <summary>El ULID que generó el dispositivo (`ADR-005`). Hace inofensivo el reenvío.</summary>
    public required Ulid Id { get; init; }

    /// <summary>A qué hecho respalda. Una foto sin su transición no prueba nada.</summary>
    public required Ulid IdTransicion { get; init; }

    /// <summary>
    /// Relativa a la raíz del almacén, <b>nunca absoluta</b>.
    ///
    /// La institución puede mover el almacén a un disco más barato o a uno de solo
    /// lectura sin tocar una fila — que es una de las consecuencias positivas que
    /// `ADR-004` buscaba.
    /// </summary>
    public required string Ruta { get; init; }

    /// <summary>
    /// Con el algoritmo en el dato: `sha256:…`.
    ///
    /// Es lo que permite detectar que un adjunto <b>fue sustituido o se corrompió</b>, y
    /// lo que sostiene los paquetes de evidencia. Llevar el algoritmo dentro importa
    /// porque dentro de diez años alguien va a necesitar saber con qué se calculó.
    /// </summary>
    public required string Hash { get; init; }

    public required string Tipo { get; init; }
    public required long Bytes { get; init; }

    /// <summary>
    /// `OPERATIVO` o `DATO_PERSONAL` — por `HB34-53`.
    ///
    /// <b>La depuración de datos personales alcanza a los adjuntos.</b> Sin esta columna
    /// no hay forma de encontrar la foto de un manifiesto entre treinta mil fotos de
    /// odómetro, y entonces el hábeas data no se puede atender.
    /// </summary>
    public required string Clasificacion { get; init; }

    /// <summary>La fecha del hecho, no la de subida (`P-4`). Organiza el almacén.</summary>
    public required DateTime CapturadoEnUtc { get; init; }

    public required DateTime RecibidoEnUtc { get; init; }
}
