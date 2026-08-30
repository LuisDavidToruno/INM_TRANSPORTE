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

    /// <summary>
    /// A qué hecho respalda. Una foto sin su transición no prueba nada.
    ///
    /// ⚠️ <b>Nulo cuando el adjunto no respalda un hecho de misión sino una versión de
    /// parámetro normativo</b> — el comunicado o el acuerdo del que salió una tarifa.
    ///
    /// Fue obligatorio hasta el 2026-08-29, y eso hacía que `RespaldoDocumental.Adjunto` no
    /// tuviera dónde apuntar: el tipo exigía un `Ulid` y <b>no existía ninguna fila que
    /// pudiera contenerlo</b>. El identificador se cargaba, se mostraba en pantalla junto a la
    /// fuente, y no había documento detrás. Se descubrió al exigir que el respaldo exista
    /// antes de aprobar (`HU-145`).
    ///
    /// No se partió en dos tablas porque es el mismo objeto —archivo, hash, ruta, tipo,
    /// clasificación— con dos dueños posibles. Lo que sí queda pendiente es la restricción
    /// que obligue a tener exactamente uno: ver el hallazgo en `HANDOFF.md`.
    /// </summary>
    public Ulid? IdTransicion { get; init; }

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
