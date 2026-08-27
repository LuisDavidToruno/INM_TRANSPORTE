namespace Sigti.Aplicacion.M16_Sincronizacion;

/// <summary>
/// El almacén de adjuntos — <b>sistema de archivos plano, por fecha</b> (`ADR-004`).
///
/// ── Por qué plano y no una jerarquía elaborada ───────────────────────────────
/// `ADR-004` descartó `FILESTREAM`, `FileTable` y almacenamiento de objetos por la misma
/// razón: *«cada pieza móvil es una pieza que alguien tiene que operar»*, y el despliegue
/// es on-premise, sin equipo de TI. Un árbol de carpetas por año y mes lo entiende quien
/// hace el respaldo sin que nadie se lo explique.
///
/// ── Por qué por fecha del hecho y no de subida ───────────────────────────────
/// Porque `P-4` manda: un adjunto capturado el 20 de marzo y subido el 27 —siete días sin
/// red— pertenece a marzo. Ordenarlo por fecha de subida dispersaría una misma misión
/// entre dos carpetas, y el respaldo por período dejaría de coincidir con el expediente.
///
/// ── La ruta que se guarda es relativa ────────────────────────────────────────
/// Nunca absoluta. La institución puede mover el almacén a un disco más barato o de solo
/// lectura sin tocar una fila de la base — una de las consecuencias que `ADR-004` buscaba.
/// </summary>
public sealed class AlmacenDeArchivos
{
    private readonly string _raiz;

    public AlmacenDeArchivos(string raiz)
    {
        _raiz = raiz;
        Directory.CreateDirectory(_raiz);
    }

    /// <summary>Devuelve la ruta <b>relativa a la raíz</b>, que es lo que va a la base.</summary>
    public async Task<string> GuardarAsync(
        Ulid idAdjunto,
        string tipo,
        DateTimeOffset capturadoEn,
        byte[] contenido,
        CancellationToken cancelacion = default)
    {
        var carpeta = Path.Combine(
            capturadoEn.UtcDateTime.ToString("yyyy"),
            capturadoEn.UtcDateTime.ToString("MM"));

        Directory.CreateDirectory(Path.Combine(_raiz, carpeta));

        // El nombre es el identificador, no el que traía el dispositivo: un nombre de
        // archivo de origen puede colisionar, traer caracteres del sistema de archivos
        // ajeno, o revelar algo que no corresponde.
        var relativa = Path.Combine(carpeta, $"{idAdjunto}{ExtensionDe(tipo)}");

        await File.WriteAllBytesAsync(Path.Combine(_raiz, relativa), contenido, cancelacion);

        return relativa;
    }

    /// <summary>
    /// La extensión sale del tipo declarado, no del nombre de origen.
    ///
    /// Es lista corta a propósito: lo que el cliente de campo produce hoy son fotografías
    /// y, cuando exista `M-15`, documentos firmados. Aceptar cualquier extensión sería
    /// aceptar cualquier archivo, y este almacén no valida contenido.
    /// </summary>
    private static string ExtensionDe(string tipo) => tipo switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "application/pdf" => ".pdf",
        _ => ".bin",
    };
}
