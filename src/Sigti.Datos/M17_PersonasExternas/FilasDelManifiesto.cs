using Sigti.Dominio.M17_PersonasExternas;

namespace Sigti.Datos.M17_PersonasExternas;

/// <summary>
/// El manifiesto de una misión — `RN-53`.
///
/// ── Tabla propia, y no columnas del expediente ──────────────────────────────
/// `RN-51` exige que los datos personales estén <b>estructuralmente separados</b> de los de
/// gestión pública, «de modo que estos últimos puedan exportarse sin aquellos». Si las personas
/// vivieran en el expediente, la exportación de transparencia tendría que <b>filtrar</b> — y un
/// filtro es algo que alguien puede olvidar, o que una consulta nueva puede saltarse.
///
/// Separadas, el reporte público sale de otra tabla y no hay nada que filtrar.
/// </summary>
public sealed class FilaDeManifiesto
{
    public required Ulid Id { get; init; }

    public required Ulid MisionId { get; init; }

    /// <summary>Nulo mientras está abierto. Al despachar se cierra y ya no se toca.</summary>
    public DateTime? CerradoUtc { get; set; }

    public string? CierraQuien { get; set; }

    public List<FilaDePersonaEnManifiesto> Personas { get; } = [];
    public List<FilaDeNovedadDeRuta> Novedades { get; } = [];
}

/// <summary>Una persona declarada a bordo.</summary>
public sealed class FilaDePersonaEnManifiesto
{
    public required Ulid Id { get; init; }
    public required Ulid ManifiestoId { get; init; }

    /// <summary>Nulo cuando no se identificó. Es un caso previsto, no un dato faltante.</summary>
    public string? Nombre { get; init; }
    public string? Identificacion { get; init; }

    public required FormaDeIdentificacion Forma { get; init; }

    /// <summary>La institución o condición que motiva el traslado, del catálogo mínimo.</summary>
    public required string QueMotivaElTraslado { get; init; }

    public required string Origen { get; init; }
    public required string Destino { get; init; }

    /// <summary>
    /// Camilla, acompañante, silla de ruedas. <b>No es un dato de salud</b>: `RN-51` satisface
    /// la necesidad «sin consignar diagnóstico».
    /// </summary>
    public string? RequerimientoOperativo { get; init; }
}

/// <summary>Lo que cambió después del cierre. <b>Se suma; no edita el manifiesto.</b></summary>
public sealed class FilaDeNovedadDeRuta
{
    public required Ulid Id { get; init; }
    public required Ulid ManifiestoId { get; init; }
    public required TipoDeNovedad Tipo { get; init; }
    public string? AQuien { get; init; }
    public required string Motivo { get; init; }
    public string? DondePaso { get; init; }

    /// <summary>Cuándo pasó, no cuándo se registró — `RN-46`.</summary>
    public required DateTime FechaDelHechoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }

    public required string Registra { get; init; }

    /// <summary>Obligatorio sólo cuando alguien subió en ruta. Nulo en las demás.</summary>
    public string? Autoriza { get; init; }
}
