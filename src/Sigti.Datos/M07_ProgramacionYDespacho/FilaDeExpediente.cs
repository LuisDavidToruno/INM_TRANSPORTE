using Sigti.Dominio.M07_ProgramacionYDespacho;

namespace Sigti.Datos.M07_ProgramacionYDespacho;

/// <summary>
/// Forma de persistencia del expediente. Vive en Sigti.Datos y no en el dominio: el
/// dominio no conoce el ORM (`ADR-009`).
///
/// <b>No tiene columna de estado.</b> Guardarla sería duplicar lo que el diario ya dice,
/// y una copia es algo que se puede desincronizar del original (P-1).
/// </summary>
public sealed class FilaDeExpediente
{
    public required Ulid Id { get; init; }
    public required string CapturadaPor { get; init; }
    public required string SolicitanteDeDerecho { get; init; }

    /// <summary>Qué se pidió movilizar. Ver <c>DatosDeLaSolicitud</c> en el dominio.</summary>
    public required string Dependencia { get; init; }

    public required string ObjetoDelTraslado { get; init; }
    public required string Destino { get; init; }

    /// <summary>La ventana declarada por quien pide, no por quien programa.</summary>
    public required DateOnly Salida { get; init; }

    public required DateOnly Retorno { get; init; }

    /// <summary>
    /// A qué hora sale. <b>Nula en los expedientes creados antes de que el campo existiera</b>,
    /// y por eso la columna admite nulo: fabricarles un «08:00» los haría indistinguibles de
    /// los que sí lo declararon, y sobre ese dato inventado se juzgaría `BD-04`.
    ///
    /// Lo nuevo sí la trae: `POST /misiones` la exige.
    /// </summary>
    public TimeOnly? HoraDeSalida { get; init; }

    public TimeOnly? HoraDeRetorno { get; init; }

    public required int HolguraDias { get; init; }
    public List<FilaDeTransicion> Transiciones { get; } = [];
}

/// <summary>Una transición del diario, tal como se guarda.</summary>
public sealed class FilaDeTransicion
{
    public required Ulid Id { get; init; }
    public required Ulid ExpedienteId { get; init; }

    /// <summary>Posición en el diario. El orden del diario es parte del dato, no del azar de la consulta.</summary>
    public required int Orden { get; init; }

    /// <summary>El identificador de la tabla de transiciones: `T-01` a `T-22`.</summary>
    public required string Transicion { get; init; }

    public required EstadoDeMision Destino { get; init; }
    public required string Ejecuta { get; init; }
    public required DateTime MomentoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }

    /// <summary>
    /// El identificador que le puso <b>quien capturó el hecho</b> — el dispositivo de campo
    /// (`ADR-005`). <b>Nulo cuando el hecho nació en la oficina</b>, con red.
    ///
    /// Su índice único es lo que hace que reenviar un lote sea inofensivo: la segunda vez
    /// la base rechaza, en vez de duplicar. Es garantía, no comprobación — una comprobación
    /// se puede olvidar al escribir el próximo endpoint.
    /// </summary>
    public Ulid? IdDeCaptura { get; init; }
    public string? Motivo { get; init; }

    /// <summary>
    /// El vehículo que esta transición tomó. <b>Nulo en toda transición que no reserva</b>,
    /// que son casi todas.
    ///
    /// Va acá y no en una tabla de reservas porque el estado es la proyección del diario
    /// (P-1). Con esto, <b>liberar es no volver a tomar</b>: `T-11` y `T-13` no borran
    /// nada, y una misión anulada deja de ocupar por el solo hecho de que el diario siguió.
    /// Una tabla aparte tendría que acordarse de borrar, y el día que no lo haga queda un
    /// vehículo fantasma ocupado que el sistema reporta como sin disponibilidad.
    /// </summary>
    public Ulid? VehiculoTomado { get; init; }

    /// <summary>Quien conduce. Va en pareja con <see cref="VehiculoTomado"/>.</summary>
    public Ulid? ConductorTomado { get; init; }

    /// <summary>
    /// La lectura del odómetro. <b>Sólo la llevan `T-14` y `T-18`</b>, y es lo que `BD-05`
    /// vuelve a leer para comparar el retorno contra la salida.
    /// </summary>
    public int? Odometro { get; init; }
}
