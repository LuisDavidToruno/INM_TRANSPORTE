using Microsoft.EntityFrameworkCore;

using Sigti.Aplicacion.M17_PersonasExternas;
using Sigti.Datos;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M17_PersonasExternas;
using Sigti.Dominio.Organizacion;

namespace Sigti.Aplicacion.M16_Sincronizacion;

/// <summary>
/// La lectura de un adjunto — <b>el camino que no existía</b>.
///
/// ── ⚠️ Los adjuntos entraban y no salían nunca ──────────────────────────────
/// <c>AlmacenDeArchivos</c> tenía <c>GuardarAsync</c> y nada más. Todo lo que el sistema exige
/// adjuntar quedaba escrito y <b>fuera de alcance</b>:
///
/// <list type="bullet">
/// <item>El respaldo documental del parámetro normativo — y <c>HU-145</c> dice que <i>«quien
/// aprueba tiene que poder abrir el documento»</i>. Se bloqueó la aprobación sin él, y no había
/// forma de abrirlo.</item>
/// <item>La fotografía de la constatación de rotulación, obligatoria por `RN-18`.</item>
/// <item>El documento de respaldo de placa que el agente en carretera pide.</item>
/// <item>El paquete de evidencia de una misión, que es lo que un auditor viene a ver.</item>
/// </list>
///
/// ── Y por qué la lectura no es simétrica de la escritura ────────────────────
/// Escribir un adjunto es un hecho del dispositivo. <b>Leerlo es un acceso</b>, y algunos
/// adjuntos llevan datos personales: `RN-52` exige que toda consulta quede asentada <b>antes</b>
/// de mostrar. Un almacén que devolviera bytes sin más convertiría el registro de consultas en
/// una formalidad que se puede saltar pidiendo la foto directamente.
/// </summary>
public sealed class LecturaDeAdjuntos(
    SigtiDbContext contexto,
    AlmacenDeArchivos almacen,
    ServicioDePersonasExternas personas)
{
    /// <summary>
    /// Abre un adjunto. <b>Registra el acceso antes de devolverlo</b> cuando lleva datos
    /// personales.
    /// </summary>
    /// <param name="quien">
    /// Quién lo pide. <b>Obligatorio</b> y no por formalidad: un adjunto con dato personal que
    /// se sirve sin saber quién lo miró deja el hábeas data sin poder contestarse.
    /// </param>
    /// <param name="necesidadDeConocer">
    /// Por qué lo necesita. Sólo se exige en los de dato personal — pedirlo para la foto de un
    /// odómetro convertiría el campo en un trámite que todos aprenden a rellenar con «consulta».
    /// </param>
    public async Task<AdjuntoAbierto> AbrirAsync(
        Ulid id,
        IdPersona quien,
        string rol,
        string? necesidadDeConocer,
        DateTimeOffset momento,
        string? origen = null,
        CancellationToken cancelacion = default)
    {
        var fila = await contexto.Adjuntos
            .AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancelacion)
            ?? throw new AdjuntoNoEncontrado(id);

        // ⚠️ **El asiento va ANTES de leer el archivo**, no después. Si el registro fuera
        // posterior, una lectura que revienta a mitad habría abierto el archivo sin dejar
        // rastro — y ése es justamente el acceso que interesa auditar (`RN-52`).
        if (EsDatoPersonal(fila.Clasificacion))
        {
            await personas.RegistrarConsultaAsync(
                quien, rol, $"adjunto:{id}",
                AlcanceDeLaConsulta.ManifiestoCompleto,
                momento, necesidadDeConocer, origen, cancelacion);
        }

        var contenido = await almacen.LeerAsync(fila.Ruta, cancelacion)
            ?? throw new AdjuntoSinArchivo(id, fila.Ruta);

        return new AdjuntoAbierto(
            contenido, fila.Tipo, fila.Hash, fila.Clasificacion,
            EsDatoPersonal(fila.Clasificacion));
    }

    /// <summary>
    /// Si el adjunto lleva dato personal — `HB34-53`: <c>OPERATIVO</c> o <c>DATO_PERSONAL</c>.
    ///
    /// <b>Lo desconocido cuenta como dato personal.</b> Una clasificación que nadie reconoce es
    /// un adjunto del que no se sabe qué contiene, y servirlo sin registrar el acceso sería
    /// decidir por omisión que no importa.
    /// </summary>
    private static bool EsDatoPersonal(string clasificacion) =>
        !string.Equals(clasificacion, "OPERATIVO", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(clasificacion, "ADMINISTRATIVO", StringComparison.OrdinalIgnoreCase);
}

/// <param name="LlevaDatoPersonal">
/// Si el acceso quedó asentado. Va en la respuesta para que quien la consume <b>sepa que su
/// consulta se registró</b> — y no lo descubra después en un reporte de accesos.
/// </param>
public sealed record AdjuntoAbierto(
    byte[] Contenido, string Tipo, string Hash, string Clasificacion, bool LlevaDatoPersonal);

public sealed class AdjuntoNoEncontrado(Ulid id)
    : Exception($"No existe el adjunto {id}.");

/// <summary>
/// La fila está y el archivo no.
///
/// <b>Es el caso que `ADR-004` avisó que podía pasar</b>: el binario vive en el sistema de
/// archivos y la base sólo guarda su rastro, así que un almacén movido, restaurado a medias o
/// montado en la ruta equivocada produce exactamente esto. Se dice con la ruta, porque es lo que
/// permite averiguar dónde quedó.
/// </summary>
public sealed class AdjuntoSinArchivo(Ulid id, string ruta)
    : Exception(
        $"El adjunto {id} está registrado y su archivo no está en el almacén ({ruta}). " +
        "El binario vive en el sistema de archivos y la base sólo guarda su rastro (ADR-004): " +
        "revise que el almacén esté montado en la ruta configurada y completo.");
