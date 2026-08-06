# RN-13 — Un motorista y un vehículo no pueden estar asignados a dos misiones cuyas ventanas se traslapen

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-03, M-05 |
| **Origen** | Práctica de programación y despacho; norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — conciliación de registros |
| **Verificación** | `[I]` regla operativa — `[V]` la exigencia de registros consistentes |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — parámetro `holgura_entre_misiones` (minutos) |

## Enunciado

El sistema **no debe** permitir que un mismo motorista, ni un mismo vehículo, quede asignado simultáneamente a dos Órdenes de Misión cuyas ventanas `[salida, retorno previsto]` se traslapen, incluyendo la **holgura configurada** entre misiones consecutivas.

La consolidación de varias solicitudes en **una misma** Orden de Misión no es doble asignación: es el mecanismo previsto para atender necesidades coincidentes con un solo recurso.

## Justificación

Un recurso asignado dos veces produce, en cascada, todos los defectos que el sistema intenta evitar: dos bitácoras del mismo vehículo con odómetros contradictorios, combustible asignado dos veces para el mismo trayecto, y un rendimiento km/galón imposible de conciliar. La conciliación periódica de [NRM-01](../normativa/NRM-01-control-interno-tsc.md) se vuelve irresoluble.

Además es el mecanismo por el cual se puede duplicar consumo real sin dejar rastro evidente.

## Condiciones de aplicación

Aplica desde el estado `PROGRAMADA` en adelante. Dos órdenes en `APROBADA` sin recursos asignados no compiten entre sí.

La holgura entre misiones consecutivas cubre el tiempo de retorno al predio, revisión y reabastecimiento. `[C]` su valor con el Jefe de Transporte; **no se cablea**.

## Comportamiento esperado

1. Al asignar, el sistema evalúa traslape contra todas las órdenes activas del recurso y contra los períodos de **indisponibilidad** conocidos: taller ([RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md)), ausencias ([RN-12](RN-12-disponibilidad-del-motorista.md)).
2. El bloqueo identifica la orden en conflicto por folio, con sus fechas: *"El vehículo <correlativo> está asignado a la Orden N.º <folio> del <fecha> al <fecha>."*
3. El sistema ofrece **consolidar** las solicitudes en una sola Orden de Misión cuando origen, destino y ventana lo permiten, en vez de solo bloquear.
4. La agenda de flota y la agenda de motoristas son consultables por día, semana y delegación, con las ventanas ocupadas visibles.
5. Si una misión se **extiende** e invade la ventana de la siguiente, el sistema alerta al despacho de inmediato y marca la orden posterior como *en riesgo*, exigiendo reprogramación o sustitución.

## Casos límite

- **Misión sin fecha de retorno cierta.** Ocupa la ventana hasta su fecha máxima prevista. Sin fecha máxima no hay agenda posible: se exige.
- **Dos misiones el mismo día que no se traslapan por minutos.** La holgura decide. Si la holgura configurada es cero, se permite — y esa decisión queda registrada como parámetro, no como criterio del despachador de turno.
- **Traslape deliberado por relevo de motoristas en ruta larga.** Dos motoristas, un vehículo, una sola orden: se modela como **turnos dentro de la misma orden**, con constancia de traspaso de custodia ([RN-22](RN-22-custodia-del-vehiculo.md)) y odómetro en cada relevo. No como dos órdenes.
- **Vehículo prestado a otra dependencia.** Es una asignación que ocupa ventana igual que una misión. Debe modelarse como préstamo con fechas, no como ausencia de registro.
- **Orden creada en campo sin conectividad** que al sincronizar resulta traslapada con otra creada en la oficina. No se rechaza ni se aplica automáticamente: entra a la cola de conflictos ([RN-45](RN-45-cero-sobrescritura-silenciosa.md)) porque **una de las dos ocurrió realmente** y hay que averiguar cuál.
- **Vehículo asignado a una misión y llamado a una emergencia.** Se resuelve reprogramando o anulando la primera orden con motivo, no ignorando el traslape. La emergencia justifica la decisión; no justifica el registro incoherente.
- **Motorista asignado a dos vehículos en la misma orden** — traslada uno al taller y regresa en otro. Es una sola ventana y un solo motorista: permitido, con ambos vehículos vinculados a la orden y sus odómetros registrados por separado.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-22](RN-22-custodia-del-vehiculo.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md)
- Actores: ACT-04, ACT-05, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
