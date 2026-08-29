using Sigti.Dominio.M14_Auditoria;

namespace Sigti.Datos.M14_Auditoria;

/// <summary>
/// Una fuente externa del catálogo — `RN-95` punto 1.
/// </summary>
public sealed class FilaDeFuenteExterna
{
    public required Ulid Id { get; init; }

    public required TipoDeFuenteExterna Tipo { get; init; }

    /// <summary>Contra quién se concilia, y a quién se le reclama una diferencia.</summary>
    public required string Emisor { get; init; }

    public required string Formato { get; init; }

    /// <summary>
    /// <b>Obligatorio.</b> Una fuente sin responsable es una fuente que nadie carga, y a los
    /// tres meses la conciliación existe en el papel y no en la práctica.
    /// </summary>
    public required string ResponsableDeLaCarga { get; init; }

    /// <summary>
    /// Falso significa <b>«no la tenemos»</b>, no «pendiente». `RN-95`: no disponible es
    /// distinto de conciliada, y confundirlas hace que la ausencia de diferencias se lea como
    /// conformidad.
    /// </summary>
    public required bool Disponible { get; set; }

    public string? PorQueNoEstaDisponible { get; set; }

    /// <summary>Nula mientras la institución no la fije — `[C]`.</summary>
    public int? PeriodicidadEnDias { get; set; }

    /// <summary>
    /// <b>Nula significa que nunca se ha conciliado</b>, y eso no es cero días de retraso: es
    /// una fuente que nadie ha mirado nunca.
    /// </summary>
    public DateOnly? UltimaConciliacion { get; set; }
}

/// <summary>
/// Una ejecución de conciliación, con su <b>fecha de corte de conocimiento</b> (`RN-94`) y el
/// documento del que salió.
///
/// Se guarda entera porque `RN-95` punto 6 lo exige: sin el documento fuente, una diferencia no
/// se puede volver a comprobar contra el papel del que salió.
/// </summary>
public sealed class FilaDeEjecucion
{
    public required Ulid Id { get; init; }

    public required Ulid FuenteId { get; init; }

    public required DateOnly Desde { get; init; }

    public required DateOnly Hasta { get; init; }

    /// <summary>El archivo o documento fuente usado. <b>Obligatorio.</b></summary>
    public required string DocumentoFuente { get; init; }

    /// <summary>Hasta qué momento se conoce lo que este resultado afirma — `RN-94`.</summary>
    public required DateTime FechaDeCorteUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    public required string Ejecuta { get; init; }

    public required int Coincidentes { get; init; }

    public required int SoloEnLaFuente { get; init; }

    public required int SoloEnSigti { get; init; }

    public List<FilaDeDiferencia> Diferencias { get; } = [];
}

/// <summary>De qué lado está la diferencia. `RN-95` concilia <b>en ambos sentidos</b>.</summary>
public enum LadoDeLaDiferencia
{
    /// <summary>
    /// La fuente lo tiene y nosotros no. Puede ser un cobro indebido o un consumo que nadie
    /// registró — <b>la conciliación no presume cuál</b>.
    /// </summary>
    SoloEnLaFuente,

    /// <summary>
    /// Nosotros lo tenemos y la fuente no. Puede ser un comprobante falso o una estación que no
    /// reportó. Tampoco se presume.
    /// </summary>
    SoloEnSigti,
}

/// <summary>
/// Una diferencia — <b>el expediente que `RN-95` abre automáticamente</b>.
///
/// ── Es el mínimo de `RN-93`, no `RN-93` completo ────────────────────────────
/// `RN-93` gobierna el expediente de hallazgo posterior en general, incluido el que abre un
/// auditor revisando misiones de marzo en noviembre. Acá está el que nace de una conciliación:
/// con responsable, plazo y resolución, que es lo que impide que «no resuelto» se vuelva un
/// montón que crece y que nadie revisa.
/// </summary>
public sealed class FilaDeDiferencia
{
    public required Ulid Id { get; init; }

    public required Ulid EjecucionId { get; init; }

    public required LadoDeLaDiferencia Lado { get; init; }

    /// <summary>La <b>fecha del hecho</b>, no la del estado de cuenta (`RN-46`).</summary>
    public required DateOnly FechaDelHecho { get; init; }

    public required decimal Monto { get; init; }

    /// <summary>El comprobante o ticket, cuando lo hay.</summary>
    public string? Referencia { get; init; }

    /// <summary>El identificador de la línea en el documento del proveedor.</summary>
    public string? LineaExterna { get; init; }

    /// <summary>Nuestro asiento, cuando la diferencia es de nuestro lado.</summary>
    public Ulid? AsientoId { get; init; }

    public string? Origen { get; init; }

    /// <summary>
    /// A qué vehículo se resolvió. <b>Nulo es «no resuelto»</b>, y no se asigna por parecido
    /// (`RN-66`).
    /// </summary>
    public Ulid? VehiculoId { get; init; }

    /// <summary>
    /// Cuál ancla lo resolvió. Va al expediente porque no es lo mismo haber resuelto por número
    /// de bien que por placa: la segunda admite discusión.
    /// </summary>
    public AnclaDeVehiculo? Ancla { get; init; }

    public required string Explicacion { get; init; }

    /// <summary>
    /// Quién le da seguimiento. <b>Obligatorio para lo no resuelto</b> — `RN-66`.
    /// </summary>
    public string? ResponsableDeSeguimiento { get; set; }

    public DateOnly? Plazo { get; set; }

    /// <summary>
    /// Cómo terminó. Nulo mientras sigue abierta. <b>No se borra al resolverse</b>: que la
    /// diferencia existió es parte del expediente.
    /// </summary>
    public string? Resolucion { get; set; }

    public DateTime? ResueltaUtc { get; set; }
}
