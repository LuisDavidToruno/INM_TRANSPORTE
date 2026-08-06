# RN-04 — Ningún registro se borra: toda anulación o corrección es un asiento reverso con motivo y autor

| Campo | Valor |
|---|---|
| **Módulos** | M-14, y todos los módulos transaccionales |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — pista de auditoría append-only |
| **Verificación** | `[V]` |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

El sistema **no debe** ofrecer, en ninguna pantalla ni por ningún rol, la eliminación física de una Orden de Misión, bitácora, asignación de combustible, registro de peaje, liquidación, incidente o acto de autorización.

Dejar sin efecto un registro se hace **exclusivamente** mediante un **asiento reverso**: un nuevo registro que referencia al original, lo neutraliza, y lleva motivo tipificado, motivo en texto libre, autor, rol y marca de tiempo. El registro original permanece consultable, marcado como anulado.

Una **corrección** es un asiento reverso seguido de un asiento nuevo, encadenados: nunca una escritura sobre el valor anterior.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md): *"Sin borrado físico: toda anulación es un asiento reverso con motivo y autor."* La premisa rectora 3 del proyecto lo eleva a principio: la trazabilidad inmutable prevalece sobre la comodidad del usuario.

El patrón de hallazgo que busca el TSC en flota es la **correlación entre consumo, kilometraje y misión autorizada**. Un borrado destruye precisamente el término de la correlación que estorba. Si el sistema permite borrar, el sistema es el instrumento del hallazgo, no su defensa.

## Condiciones de aplicación

Aplica desde el momento en que un registro **sale de `BORRADOR`**.

**No aplica** a registros en `BORRADOR` que nunca fueron enviados a autorización ni impresos: un borrador puede descartarse, y ese descarte se registra como evento sin conservar el contenido. `[C]` confirmar con Auditoría Interna que este trato del borrador es aceptable; si no lo es, el borrador también se conserva.

**No aplica** a datos espejo de ARGOS y Talento Humano, cuyo ciclo de vida pertenece al sistema origen ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)).

## Comportamiento esperado

1. La acción disponible se llama **"Anular"**, nunca "Eliminar". El sistema exige motivo tipificado de un catálogo configurable y motivo en texto libre no vacío.
2. La anulación **arrastra en cascada** los registros dependientes: anular una Orden de Misión exige resolver explícitamente qué ocurre con sus asignaciones de combustible, salvoconductos y folios emitidos. El sistema no anula lo dependiente en silencio: lo lista y exige decisión.
3. Todo documento impreso derivado de un registro anulado pasa a estado **anulado en la página pública de verificación por QR**, para que un control en carretera detecte que el papel ya no tiene respaldo.
4. La consulta del expediente muestra la **cadena completa**: registro original, asiento reverso, registro sustituto.
5. Los reportes operativos excluyen por defecto los registros anulados; los reportes de auditoría los incluyen con su motivo. Ambos comportamientos son explícitos en el encabezado del reporte.

## Casos límite

- **Anulación después de que el vehículo ya salió.** No se anula la Orden de Misión: se **cancela en ruta** y se liquida lo consumido. Anular retroactivamente una misión que efectivamente ocurrió es falsear el registro. Ver el caso de vale emitido y viaje cancelado en [RN-27](RN-27-asignacion-de-combustible-con-folio.md).
- **Anulación de un asiento reverso.** Permitida, con el mismo mecanismo: se genera un tercer asiento que reversa el reverso. Nunca se "deshace" borrando.
- **Duplicado creado por una sincronización defectuosa.** Es el único caso donde el usuario percibirá la regla como absurda: dos registros idénticos, uno espurio. Se resuelve anulando el duplicado con motivo tipificado *duplicado por sincronización* y vinculándolo al registro superviviente — no borrándolo. Ver [RN-45](RN-45-cero-sobrescritura-silenciosa.md).
- **Dato personal que debe suprimirse por hábeas data.** El [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md) exige poder rectificar **sin destruir el registro contable original**. Se resuelve seudonimizando el dato personal y conservando el asiento financiero y operativo. La supresión física del expediente no está disponible.
- **Purga por vencimiento del plazo de retención.** Es la única eliminación física admitida, ejecutada por proceso programado contra el parámetro de retención, con acta de purga que registra qué rango se eliminó, bajo qué política y con qué autorización. `[C]` plazo con Auditoría Interna.
- **Error de captura detectado un minuto después, antes de cualquier autorización.** Sigue siendo un registro fuera de borrador: se corrige con reverso y asiento nuevo. La incomodidad es aceptable; la alternativa es una ventana de edición silenciosa que alguien terminará explotando.

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-05](RN-05-registro-cerrado-no-se-edita.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md)
- Actores: ACT-01, ACT-04, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
