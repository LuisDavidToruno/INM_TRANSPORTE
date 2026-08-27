namespace Sigti.Dominio.M07_ProgramacionYDespacho;

/// <summary>
/// Qué quedó tomado por una transición — el vehículo y quien conduce.
///
/// ── Por qué esto va en el diario y no en una tabla de reservas ───────────────
/// `T-08` está descrito como <i>«aquí se reserva vehículo y motorista»</i> desde que se
/// escribió la máquina de estados, y durante todo ese tiempo <b>no se reservó nada</b>:
/// la identidad del vehículo sólo quedaba dentro del texto de evidencia, en prosa. Con
/// eso no se puede contestar «¿qué tiene tomado el pick-up el jueves?» sin leer cadenas.
///
/// La reparación obvia sería una tabla <c>Reserva</c>. <b>No se hizo, y a propósito.</b>
/// P-1: el estado es la proyección del diario, y <c>FilaDeExpediente</c> ya lo declara —
/// <i>«no tiene columna de estado; guardarla sería duplicar lo que el diario ya dice»</i>.
/// Una tabla de reservas sería exactamente esa segunda copia, con su propia manera de
/// desincronizarse: una misión anulada cuya reserva sobrevive deja un vehículo fantasma
/// ocupado, y el sistema seguiría diciendo que no hay flota disponible.
///
/// Poniéndolo en la transición, <b>liberar es no volver a tomar</b>: `T-11` y `T-13` no
/// tienen que acordarse de borrar nada. Lo que ocupa es la última transición que tomó, y
/// si el diario siguió, ya no ocupa.
///
/// ── Por qué son identificadores y no la ficha ────────────────────────────────
/// La ficha técnica cambia —se corrige un peso, se agrega un remolque— y el diario es
/// inmutable. Guardar la ficha congelaría un dato que tiene su propio ciclo de vida en
/// `M-03`; guardar el identificador deja que la ficha evolucione donde le toca. La
/// evidencia de `BD-02`, que sí tiene que quedar congelada porque respalda la decisión,
/// se sigue guardando aparte, en el motivo.
/// </summary>
/// <param name="Vehiculo">
/// El identificador del vehículo en `M-03`. <b>Nunca la placa</b>: hay desabastecimiento
/// nacional de placa metálica y circular sin ella es estado válido.
/// </param>
/// <param name="Conductor">
/// Quien conduce. <b>Puede no estar en el padrón</b> — `RN-57` verifica sobre quien
/// efectivamente conduce, y el funcionario con vehículo asignado no se exceptúa.
/// </param>
public sealed record RecursosTomados(Ulid Vehiculo, Ulid Conductor);
