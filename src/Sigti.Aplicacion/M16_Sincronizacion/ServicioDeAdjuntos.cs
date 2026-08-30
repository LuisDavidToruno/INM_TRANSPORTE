using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Sigti.Datos;

namespace Sigti.Aplicacion.M16_Sincronizacion;

/// <summary>Lo que el dispositivo declara sobre el archivo que sube.</summary>
/// <param name="IdTransicion">
/// El hecho de misión que respalda. <b>Nulo cuando respalda una versión de parámetro
/// normativo</b> —el comunicado o el acuerdo del que salió una tarifa— que no es un hecho de
/// misión y no tiene transición a la cual colgarse.
/// </param>
public sealed record AdjuntoQueLlega(
    Ulid IdAdjunto,
    Ulid? IdTransicion,
    string Hash,
    string Tipo,
    string Clasificacion,
    DateTimeOffset CapturadoEn);

public sealed record ResultadoDeAdjunto(bool EsNuevo, string Ruta);

/// <summary>El archivo llegó distinto de lo que el dispositivo dijo que era.</summary>
public sealed class AdjuntoCorrupto(string hashDeclarado, string hashRecibido)
    : Exception(
        $"El adjunto llegó con hash {hashRecibido} y el dispositivo declaró {hashDeclarado}. " +
        "Llegó incompleto o alterado en tránsito; el dispositivo debe reintentar.")
{
    public string HashDeclarado { get; } = hashDeclarado;
    public string HashRecibido { get; } = hashRecibido;
}

/// <summary>
/// Recibe los adjuntos del cliente de campo y los deja donde `ADR-004` decidió.
///
/// ── Qué va a dónde ───────────────────────────────────────────────────────────
/// <b>El binario al sistema de archivos; a la base solo su rastro</b> —ruta, hash, tipo,
/// tamaño y clasificación—. La aritmética: ≈ 8 GB anuales de datos relacionales contra
/// ≈ 30 GB de adjuntos. Meterlos en la base cuadruplica el respaldo y saca la
/// restauración de las 2 h que `RNF-09` exige de personal no especialista.
///
/// ── Por qué el hash se verifica y no solo se guarda ──────────────────────────
/// Guardarlo sin comprobarlo lo volvería decorativo. Un archivo que llegó truncado por
/// la red de un retén quedaría registrado como íntegro, y el defecto aparecería meses
/// después, al armar el paquete de evidencia — cuando ya no se puede volver a tomar la
/// foto.
///
/// ── El orden importa: primero el archivo, después la fila ────────────────────
/// Si se cayera entre medias, queda un archivo huérfano en disco: ocupa espacio y no
/// molesta a nadie. Al revés quedaría una fila que promete un archivo que no existe, y
/// eso rompe el paquete de evidencia sin avisar.
/// </summary>
public sealed class ServicioDeAdjuntos(SigtiDbContext contexto, AlmacenDeArchivos almacen)
{
    public async Task<ResultadoDeAdjunto> RecibirAsync(
        AdjuntoQueLlega declarado,
        Stream contenido,
        DateTimeOffset recibidoEn,
        CancellationToken cancelacion = default)
    {
        var yaEsta = await contexto.Adjuntos
            .SingleOrDefaultAsync(a => a.Id == declarado.IdAdjunto, cancelacion);

        // El dispositivo que no supo si el servidor recibió reenvía, y con 200
        // fotografías por dispositivo eso ocurre a menudo. No es un error.
        if (yaEsta is not null) return new ResultadoDeAdjunto(EsNuevo: false, yaEsta.Ruta);

        using var memoria = new MemoryStream();
        await contenido.CopyToAsync(memoria, cancelacion);
        var bytes = memoria.ToArray();

        var hashRecibido = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes));

        if (!string.Equals(hashRecibido, declarado.Hash, StringComparison.OrdinalIgnoreCase))
            throw new AdjuntoCorrupto(declarado.Hash, hashRecibido);

        var ruta = await almacen.GuardarAsync(
            declarado.IdAdjunto, declarado.Tipo, declarado.CapturadoEn, bytes, cancelacion);

        contexto.Adjuntos.Add(new FilaDeAdjunto
        {
            Id = declarado.IdAdjunto,
            IdTransicion = declarado.IdTransicion,
            Ruta = ruta,
            Hash = hashRecibido,
            Tipo = declarado.Tipo,
            Bytes = bytes.LongLength,
            Clasificacion = declarado.Clasificacion,
            CapturadoEnUtc = declarado.CapturadoEn.UtcDateTime,
            RecibidoEnUtc = recibidoEn.UtcDateTime,
        });

        await contexto.SaveChangesAsync(cancelacion);

        return new ResultadoDeAdjunto(EsNuevo: true, ruta);
    }
}
