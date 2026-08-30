using Sigti.Dominio.M17_PersonasExternas;

namespace Sigti.Datos.M17_PersonasExternas;

/// <summary>
/// Un campo del catálogo del manifiesto — `RN-51`, `HU-112`.
/// </summary>
public sealed class FilaDeCampoDelManifiesto
{
    public required Ulid Id { get; init; }

    /// <summary>La clave técnica. Única: dos campos con la misma clave serían el mismo campo.</summary>
    public required string Clave { get; init; }

    public required string Etiqueta { get; init; }

    public required ClaseDelCampo Clase { get; init; }

    /// <summary>
    /// Si se está capturando hoy. <b>Un campo inactivo no marca nada</b>: existe en el catálogo
    /// y no toma datos.
    /// </summary>
    public required bool Activo { get; set; }

    // ── El fundamento. Nulo es «activado sin fundamentar» ───────────────────
    public string? BaseLegal { get; set; }
    public string? NecesidadOperativa { get; set; }
    public string? FundamentaPersona { get; set; }
    public DateTime? FundamentadoUtc { get; set; }

    public required string Activa { get; init; }
    public required DateTime ActivadoUtc { get; init; }
}

/// <summary>
/// El asiento de cada acceso a datos de personas trasladadas — `RN-52`.
///
/// ── Esta tabla no se actualiza nunca ────────────────────────────────────────
/// `RN-52`: <i>«el registro de consultas debe ser inmutable»</i>. Por eso no lleva ninguna
/// columna mutable: un asiento equivocado no se corrige, se explica con otro.
///
/// ── Y está SEPARADA del manifiesto a propósito ──────────────────────────────
/// Depurar los datos personales al vencer su plazo (`PT-137`) <b>no puede borrar el rastro de
/// quién los consultó</b>. Si vivieran juntos, la depuración destruiría la única respuesta que
/// la institución tiene ante un hábeas data — justo sobre los datos más viejos, que son los que
/// más probablemente se reclamen.
/// </summary>
public sealed class FilaDeConsultaAManifiesto
{
    public required Ulid Id { get; init; }

    public required string Consultante { get; init; }

    /// <summary>
    /// Con qué rol miró. <b>Copia, no referencia</b>: si apuntara al rol vivo, un cambio de
    /// puesto reescribiría con qué competencia se hizo un acceso de hace dos años.
    /// </summary>
    public required string Rol { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required string RegistroConsultado { get; init; }

    /// <summary>Qué se mostró, no sólo qué se abrió.</summary>
    public required AlcanceDeLaConsulta Alcance { get; init; }

    /// <summary>
    /// Para qué. <b>Nulo es «no lo declaró»</b>, y esa ausencia es la que vuelve inauditable el
    /// acceso: queda el rastro de quién miró y ninguna forma de juzgar si debía.
    /// </summary>
    public string? NecesidadDeConocer { get; init; }

    /// <summary>Desde dónde. Nulo cuando la petición no lo trajo.</summary>
    public string? Origen { get; init; }
}
