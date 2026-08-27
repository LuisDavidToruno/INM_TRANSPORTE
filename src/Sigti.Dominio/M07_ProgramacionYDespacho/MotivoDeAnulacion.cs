namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Por qué se anula una misión aprobada (`T-09`).
///
/// <b>Es un catálogo cerrado a propósito, y no por rigidez.</b> La tipificación de
/// estas anulaciones <b>es</b> el indicador de déficit de flota que la institución
/// necesita: un motivo de texto libre no produce ningún indicador. El comentario
/// existe y es útil, pero es complemento, nunca sustituto.
///
/// Una cola de aprobadas que nadie depura oculta el déficit real.
/// </summary>
public enum MotivoDeAnulacion
{
    SinFlotaDisponible,
    SinMotoristaHabilitado,
    CaducadaPorFaltaDeProgramacion,
    DesistimientoDelSolicitante,
    CausaExterna,
}

/// <summary>
/// La aprobación caducó: la ventana solicitada ya inició y nadie programó.
///
/// <b>El límite es el inicio de la ventana, no su fin</b> — así lo fija
/// `estados/orden-de-mision.md` en los efectos de `T-05`. Reservar un vehículo para
/// un viaje que ya debía haber salido no es programar: es tapar un hueco.
///
/// No es un <see cref="BloqueoDuro"/> porque no es una precondición de las de
/// `BD-xx`: es una condición del propio expediente, y su salida no es cambiar de
/// vehículo sino <b>anularla con motivo tipificado</b>.
/// </summary>
public sealed class AprobacionCaducada(DateOnly inicioDeLaVentana, DateOnly fechaDelHecho)
    : Exception(
        $"La aprobación caducó: la ventana solicitada inició el {inicioDeLaVentana:dd/MM/yyyy} " +
        $"y hoy es {fechaDelHecho:dd/MM/yyyy}. Anúlela con motivo tipificado o pida a la " +
        "dependencia una solicitud nueva.")
{
    public DateOnly InicioDeLaVentana { get; } = inicioDeLaVentana;
    public DateOnly FechaDelHecho { get; } = fechaDelHecho;
}
