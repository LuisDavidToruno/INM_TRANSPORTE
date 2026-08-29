using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M18_Peajes;

namespace Sigti.Datos.M18_Peajes;

/// <summary>
/// Un punto de peaje del país. <b>Catálogo ampliable en producción</b> — `RN-34` punto 5:
/// `NRM-10` advierte que hay proyectos en cartera, y un enum obligaría a desplegar para agregar
/// una caseta.
/// </summary>
public sealed class FilaDePunto
{
    public required Ulid Id { get; init; }

    public required string Nombre { get; init; }

    /// <summary>COVI-H y quien venga. La exoneración por operador se resuelve contra esto.</summary>
    public required string Operador { get; init; }

    public required string Carretera { get; init; }

    /// <summary>Nulo cuando cobra en ambos sentidos, que es la condición normal.</summary>
    public string? SentidoDeCobro { get; init; }

    public List<FilaDeVigenciaDelPunto> Vigencias { get; } = [];
}

/// <summary>
/// Una vigencia del estado operativo del punto. <b>Sin esto no se puede recalcular un viaje
/// pasado por una caseta que ya no existe</b> — `NRM-10`.
/// </summary>
public sealed class FilaDeVigenciaDelPunto
{
    public required Ulid Id { get; init; }

    public required Ulid PuntoId { get; init; }

    public required EstadoDelPunto Estado { get; init; }

    public required string Fundamento { get; init; }

    public required DateOnly VigenteDesde { get; init; }

    public DateOnly? VigenteHasta { get; set; }

    /// <summary>El eje de transacción de `ADR-006`: desde cuándo el sistema lo supo.</summary>
    public required DateTime RegistradoDesdeUtc { get; init; }

    public DateTime? RegistradoHastaUtc { get; init; }
}

/// <summary>
/// Una categoría de peaje. <b>Tabla, no enum</b> — `RN-33`: debe admitir de 2 a 9 ejes,
/// montacargas y categorías futuras sin cambio de código.
/// </summary>
public sealed class FilaDeCategoriaDePeaje
{
    /// <summary>El código es la llave. Se compara sin importar la caja de las letras.</summary>
    public required string Codigo { get; init; }

    public required string Nombre { get; init; }
}

/// <summary>
/// Una fila de la tabla de tarifas — `RN-34`: punto × categoría × vigencia.
///
/// <b>Nunca una fórmula.</b> La progresión de 2 a 9 ejes es casi lineal `[I]` y por eso alguien
/// va a proponer calcularla; `NRM-10` lo prohíbe porque una fórmula inferida se vuelve falsa al
/// primer ajuste asimétrico.
/// </summary>
public sealed class FilaDeTarifa
{
    public required Ulid Id { get; init; }

    public required Ulid PuntoId { get; init; }

    public required string Categoria { get; init; }

    public required decimal Monto { get; init; }

    /// <summary>SAPP, COVI-H, contrato, comunicado de la SIT. <b>Obligatoria</b>: `RN-34` punto 3.</summary>
    public required string Fuente { get; init; }

    /// <summary>Cuándo se confirmó contra la fuente. Alimenta la alerta de los 12 meses.</summary>
    public required DateOnly FechaDeVerificacion { get; init; }

    public required DateOnly VigenteDesde { get; init; }

    public DateOnly? VigenteHasta { get; set; }

    public required DateTime RegistradoDesdeUtc { get; init; }

    public DateTime? RegistradoHastaUtc { get; init; }
}

/// <summary>
/// Una fila de la matriz <c>derivacion_categoria_peaje</c> — `RN-33`.
///
/// Los criterios son anulables porque <b>nulo significa «esta fila no mira ese atributo»</b>.
/// Forzarlos todos obligaría a inventar rangos que la norma no fija — y el Artículo 51 es un
/// escaneo sin capa de texto (`[C]`, insumo #23).
/// </summary>
public sealed class FilaDeReglaDeCategoria
{
    public required Ulid Id { get; init; }

    public required string Categoria { get; init; }

    /// <summary>
    /// Menor gana. Existe porque la clasificación tiene excepciones nominales: la resolución de
    /// la SAPP saca al H-100 de la categoría a la que su peso lo llevaría.
    /// </summary>
    public required int Prioridad { get; init; }

    public required string Fundamento { get; init; }

    public ClaseNormativa? Clase { get; init; }

    public string? TipoDeVehiculo { get; init; }

    public int? PesoBrutoDesdeKg { get; init; }

    public int? PesoBrutoHastaKg { get; init; }

    public int? EjesDesde { get; init; }

    public int? EjesHasta { get; init; }

    public int? PasajerosDesde { get; init; }

    public int? PasajerosHasta { get; init; }

    public bool? LlevaRemolque { get; init; }

    public required DateOnly VigenteDesde { get; init; }

    public DateOnly? VigenteHasta { get; set; }

    public required DateTime RegistradoDesdeUtc { get; init; }

    public DateTime? RegistradoHastaUtc { get; init; }
}

/// <summary>
/// Una exoneración — `RN-38`. <b>El valor por defecto es paga</b>: ninguna se carga sola.
/// </summary>
public sealed class FilaDeExoneracion
{
    public required Ulid Id { get; init; }

    public required Ulid VehiculoId { get; init; }

    /// <summary>Nulo significa <b>todos los puntos del operador</b>, que es como se otorgan.</summary>
    public Ulid? PuntoId { get; init; }

    public string? Operador { get; init; }

    /// <summary>Obligatorio, con adjunto. Es un acto autorizado y registrado (`RN-03`).</summary>
    public required string Fundamento { get; init; }

    public required DateOnly VigenteDesde { get; init; }

    public DateOnly? VigenteHasta { get; set; }

    public required DateTime RegistradoDesdeUtc { get; init; }

    public DateTime? RegistradoHastaUtc { get; init; }
}

/// <summary>
/// Un paso por caseta, tal como ocurrió — `RN-36`.
///
/// <b>Las dos categorías van en columnas separadas.</b> Si el sistema guardara sólo la cobrada,
/// el error de la caseta se volvería la verdad institucional y el reclamo nunca ocurriría.
/// </summary>
public sealed class FilaDePaso
{
    public required Ulid Id { get; init; }

    /// <summary>Nulo cuando el punto no está en el catálogo. El paso <b>no se descarta</b>.</summary>
    public Ulid? PuntoId { get; init; }

    public required Ulid VehiculoId { get; init; }

    public Ulid? MisionId { get; init; }

    public required DateTime MomentoUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    /// <summary>Lo que ancla el paso al recorrido, y lo que permite el cruce de `RN-37`.</summary>
    public required int Odometro { get; init; }

    public required decimal MontoPagado { get; init; }

    public required MedioDePagoDelPeaje Medio { get; init; }

    public required string Registra { get; init; }

    public string? CategoriaEsperada { get; init; }

    public string? CategoriaCobrada { get; init; }

    public decimal? MontoEsperado { get; init; }

    public string? Ticket { get; init; }

    public string? CausaSinTicket { get; init; }

    /// <summary>Marcado para depuración del catálogo — `RN-34`.</summary>
    public required bool PuntoNoCatalogado { get; init; }

    public string? UbicacionDeclarada { get; init; }

    /// <summary>
    /// El identificador del dispositivo de campo. El paso por caseta se captura <b>sin
    /// conectividad</b> (`RN-43`), y el reintento duplicaría el gasto.
    /// </summary>
    public Ulid? IdDeCaptura { get; init; }
}
