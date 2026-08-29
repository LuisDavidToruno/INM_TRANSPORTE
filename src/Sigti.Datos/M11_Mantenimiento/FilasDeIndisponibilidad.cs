using Sigti.Dominio.M03_Flota;
using Sigti.Dominio.M07_ProgramacionYDespacho;
using Sigti.Dominio.M11_Mantenimiento;

namespace Sigti.Datos.M11_Mantenimiento;

/// <summary>
/// La indisponibilidad sobrevenida de un vehículo, tal como se guarda — `RN-60`.
/// </summary>
public sealed class FilaDeIndisponibilidad
{
    public required Ulid Id { get; init; }

    public required Ulid VehiculoId { get; init; }

    /// <summary>`EN_TALLER` o `NO_DISPONIBLE`: los dos que no habilitan asignación.</summary>
    public required EstadoOperativo Estado { get; init; }

    /// <summary>Del catálogo `causa_indisponibilidad`, que `RN-60` declara configurable.</summary>
    public required string Causa { get; init; }

    public required DateOnly Desde { get; init; }

    /// <summary>
    /// Con fecha de fin, siempre. `RN-60` punto 6 la contrasta contra la real: sin ella el
    /// indicador de gestión del taller se queda sin la mitad de su cuenta.
    /// </summary>
    public required DateOnly FinEstimado { get; init; }

    public required string Ejecuta { get; init; }

    /// <summary>
    /// Cuándo acusó sobre la lista de reservas. `RN-60` punto 2 manda conservar la lista con su
    /// marca de tiempo, y ésta es esa marca.
    /// </summary>
    public required DateTime MomentoDelAcuseUtc { get; init; }

    public required int DesfaseMinutos { get; init; }

    // ── El alta — `RN-60` punto 6 ───────────────────────────────────────────
    // Nulos mientras el vehículo no vuelva.
    public DateOnly? FinReal { get; set; }
    public string? OrdenDeTrabajo { get; set; }
    public int? OdometroDeSalida { get; set; }

    /// <summary>
    /// ⚠️ Por qué el vehiculo NO cambio de estado operativo, si no lo hizo.
    ///
    /// `RN-60` habla de indisponibilidad sobrevenida sobre un vehiculo con reservas, pero §10.2
    /// no tiene `ASIGNADO → EN_TALLER`. La autoridad sobre transiciones es el diagrama, asi que
    /// el asiento no se pone y **queda escrito por que** — en vez de moverlo igual, que seria
    /// escribir en el documento desde el codigo.
    /// </summary>
    public string? EstadoNoAplicado { get; set; }

    public List<FilaDeReservaAfectada> Reservas { get; } = [];
    public List<FilaDeResolucionDeReserva> Resoluciones { get; } = [];
}

/// <summary>
/// Una reserva afectada, <b>congelada al acusar</b> — `RN-60` punto 2.
///
/// Sus columnas duplican datos de la Orden de Misión a propósito: es una foto, no una
/// referencia. Reconstruirla después mostraría las misiones como están hoy, y quien acusó habría
/// acusado sobre una lista distinta de la que consta.
/// </summary>
public sealed class FilaDeReservaAfectada
{
    public required Ulid Id { get; init; }
    public required Ulid IndisponibilidadId { get; init; }
    public required Ulid MisionId { get; init; }
    public required string Referencia { get; init; }
    public required string Dependencia { get; init; }
    public required DateOnly Salida { get; init; }
    public required DateOnly Retorno { get; init; }
    public required string Motorista { get; init; }
    public required string ObjetoDelTraslado { get; init; }
    public required EstadoDeMision EstadoAlAcusar { get; init; }
}

/// <summary>El desenlace registrado de una reserva en conflicto — `RN-60` punto 4.</summary>
public sealed class FilaDeResolucionDeReserva
{
    public required Ulid Id { get; init; }
    public required Ulid IndisponibilidadId { get; init; }
    public required Ulid MisionId { get; init; }
    public required DesenlaceDeLaReserva Desenlace { get; init; }
    public required string Ejecuta { get; init; }
    public required DateTime MomentoUtc { get; init; }
    public required int DesfaseMinutos { get; init; }
    public required string Motivo { get; init; }
}
