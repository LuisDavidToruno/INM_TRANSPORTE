using Sigti.Dominio.M12_Incidentes;

namespace Sigti.Datos.M12_Incidentes;

/// <summary>
/// El expediente de incidente, tal como se guarda — M-12.
///
/// ── Ninguna columna de responsabilidad ──────────────────────────────────────
/// `RN-74`. La única cercana es <see cref="DeterminacionNumero"/> y sus hermanas, que son el
/// <b>acto de otra instancia</b> adjuntado al expediente — no un campo que alguien llena.
/// </summary>
public sealed class FilaDeIncidente
{
    public required Ulid Id { get; init; }

    public required TipoDeIncidente Tipo { get; init; }

    /// <summary>Del catálogo `causa_interrupcion`, que `RN-70` declara configurable.</summary>
    public required string Causa { get; init; }

    /// <summary>Cuándo pasó. <b>No cuándo se capturó</b> (`RN-46`).</summary>
    public required DateOnly FechaDelHecho { get; init; }

    public required DateTime MomentoDelHechoUtc { get; init; }

    public required int DesfaseDelHechoMinutos { get; init; }

    /// <summary>
    /// Cuándo se registró. `RN-70` admite captura sin conectividad y sincronización posterior, así
    /// que la distancia entre las dos fechas es un dato del expediente y no un error.
    /// </summary>
    public required DateTime MomentoDeCapturaUtc { get; init; }

    public required string Descripcion { get; init; }

    public required string Registra { get; init; }

    /// <summary>Nula cuando el incidente no ocurrió en misión — una multa, un uso indebido.</summary>
    public Ulid? MisionId { get; init; }

    public Ulid? VehiculoId { get; init; }

    public string? Ubicacion { get; init; }

    /// <summary>Nulo es <b>no leído</b>, no cero. Un odómetro en cero sería una lectura falsa.</summary>
    public int? Odometro { get; init; }

    /// <summary>
    /// Si impidió continuar la misión según lo autorizado. Lo declara quien registra: el tipo no
    /// alcanza —una avería leve no interrumpe y una grave sí— y deducirlo pondría marca de
    /// interrupción a hechos que no la tuvieron.
    /// </summary>
    public required bool Interrumpe { get; init; }

    public required string ResponsableDeSeguimiento { get; set; }

    public required DateOnly Plazo { get; set; }

    // ── La constancia ante autoridad — `RN-75` punto 2 ──────────────────────
    // Su ausencia no impide registrar el evento; exigirla haría que el hecho no se registrara
    // hasta tener el papel, y para entonces nadie se acuerda de la hora ni del odómetro.
    public string? ConstanciaNumero { get; set; }
    public string? ConstanciaAutoridad { get; set; }
    public DateOnly? ConstanciaFecha { get; set; }

    // ── El desenlace — `RN-70` ──────────────────────────────────────────────
    public DesenlaceDeLaInterrupcion? Desenlace { get; set; }
    public string? DetalleDelDesenlace { get; set; }

    // ── El acto de otra instancia — `RN-74` punto 4 ─────────────────────────
    public string? DeterminacionNumero { get; set; }
    public string? DeterminacionInstancia { get; set; }
    public DateOnly? DeterminacionFecha { get; set; }
    public string? DeterminacionResolucion { get; set; }

    /// <summary>Nulo es abierto. Un expediente resuelto no entra al saldo de apertura.</summary>
    public DateOnly? ResueltoEn { get; set; }

    public string? ComoSeResolvio { get; set; }

    /// <summary>
    /// Por qué se resolvió con bienes todavía afuera — `RN-75`. Declararlo no es lo mismo que
    /// ignorarlos, y por eso queda escrito.
    /// </summary>
    public string? DeclaracionDeBienes { get; set; }

    public List<FilaDeMovimientoDelIncidente> Movimientos { get; } = [];
    public List<FilaDeBienAfectado> Bienes { get; } = [];
    public List<FilaDeGestionDeRecuperacion> Gestiones { get; } = [];
}

/// <summary>Un asiento del diario del expediente — `I-01` a `I-08`.</summary>
public sealed class FilaDeMovimientoDelIncidente
{
    public required Ulid Id { get; init; }
    public required Ulid IncidenteId { get; init; }
    public required int Orden { get; init; }
    public required string Movimiento { get; init; }
    public required string Ejecuta { get; init; }
    public required DateTime MomentoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }
    public string? Detalle { get; init; }
}

/// <summary>
/// Un bien afectado — `RN-75`.
///
/// <b>Esta fila no se borra nunca.</b> El bien permanece en el registro patrimonial hasta su
/// recuperación o su descargo formal, y las dos cosas son cambios de estado, no bajas.
/// </summary>
public sealed class FilaDeBienAfectado
{
    public required Ulid Id { get; init; }
    public required Ulid IncidenteId { get; init; }
    public required string Descripcion { get; init; }

    /// <summary>Si el bien afectado es el vehículo mismo. Cambia su estado operativo.</summary>
    public required bool EsElVehiculo { get; init; }

    public required EstadoDelBien Estado { get; set; }

    public required DateOnly FechaDelHecho { get; init; }

    /// <summary>Nula es <b>no se sabe dónde está</b>, que en una sustracción es lo normal.</summary>
    public string? UbicacionConocida { get; set; }

    public string? AutoridadCustodia { get; set; }
    public string? NumeroDeExpedienteExterno { get; set; }

    // ── El descargo formal, la única salida que no es la recuperación ───────
    public string? DescargoNumero { get; set; }
    public string? DescargoAutoridad { get; set; }
    public DateOnly? DescargoFecha { get; set; }
}

/// <summary>Una gestión de recuperación, con responsable y plazo — `RN-75`.</summary>
public sealed class FilaDeGestionDeRecuperacion
{
    public required Ulid Id { get; init; }
    public required Ulid IncidenteId { get; init; }
    public required DateOnly Fecha { get; init; }
    public required string Descripcion { get; init; }
    public required string Responsable { get; init; }
    public required DateOnly Plazo { get; init; }
}
