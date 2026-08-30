using Sigti.Dominio.M03_Flota;

namespace Sigti.Datos.M03_Flota;

/// <summary>
/// Un documento de respaldo de la circulación sin lámina — `RN-65`.
///
/// ── Por qué es una tabla y no un booleano ───────────────────────────────────
/// Lo era: <c>TieneConstanciaSustitutaDePlaca</c>. Y eso decía <b>que hay una constancia</b> y
/// nada más — una vencida a mitad de la misión pasaba exactamente igual que una vigente, y un
/// permiso provisional de treinta días emitido hace un año se veía idéntico a uno de la semana
/// pasada.
///
/// ── Y por qué es historial, no un registro que se pisa ──────────────────────
/// `RN-64`: los datos de placa se conservan con <b>rangos de vigencia</b>. La pregunta que el
/// auditor hace de verdad es <i>«¿con qué documento circulaba este vehículo en marzo?»</i>, y
/// sobreescribir el respaldo la vuelve incontestable.
/// </summary>
public sealed class FilaDeRespaldoDePlaca
{
    public required Ulid Id { get; init; }
    public required Ulid VehiculoId { get; init; }

    /// <summary>
    /// Del catálogo configurable: permiso provisional, constancia del registro, acta de
    /// retención, constancia de trámite.
    ///
    /// <b>No es un enum</b>: la lista de documentos que la autoridad emite cambia por
    /// resolución, y cablearla obligaría a desplegar para admitir uno nuevo.
    /// </summary>
    public required string Tipo { get; init; }

    /// <summary>Quién lo emitió — el registro vehicular, la DNVT.</summary>
    public required string Emisor { get; init; }

    public required string Folio { get; init; }

    /// <summary>
    /// El documento escaneado. <b>Nulo es que se declaró y no se adjuntó.</b>
    ///
    /// No alcanza: el agente en carretera pide el papel, y uno que sólo existe como texto en
    /// una pantalla no se le puede mostrar.
    /// </summary>
    public Ulid? Adjunto { get; init; }

    public required DateOnly VigenteDesde { get; init; }

    /// <summary>
    /// ⚠️ <b>Nulo NO es «vigente para siempre»</b>: es un provisional sin fecha de vencimiento
    /// declarada, que es justo lo que hay que preguntar antes de despachar. La regla lo trata
    /// como insuficiente.
    /// </summary>
    public DateOnly? VigenteHasta { get; set; }

    /// <summary>Contra qué estado de lámina se emitió. Es lo que el paquete imprime.</summary>
    public required EstadoDePlaca EstadoDePlaca { get; init; }

    public required string Registra { get; init; }
    public required DateTime RegistradoEnUtc { get; init; }
}
