# RN-19 — Un vehículo cuyo estado operativo no es disponible no puede ser asignado ni despachado

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-07, M-11 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — control de bienes; decisión [DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[I]` regla operativa — `[V]` la exigencia de control del estado del bien |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — el catálogo `estado_operativo_vehiculo` define qué estados habilitan asignación |

## Enunciado

El sistema **no debe** permitir asignar ni despachar un vehículo cuyo estado operativo vigente sea distinto de **disponible**.

Los estados no disponibles incluyen, como mínimo: en taller, indisponible por falla reportada, siniestrado, en proceso de descargo o baja, robado, prestado a otra dependencia o institución, y resguardado por disposición superior.

La apertura de una orden de trabajo en M-11 y el reporte de una falla incapacitante desde el campo **deben** cambiar el estado del vehículo automáticamente. El retorno a disponible **debe** ser un acto explícito de ACT-11 Encargado de Mantenimiento, nunca automático por cierre de la orden de trabajo.

## Justificación

Asignar un vehículo que está en el taller produce una misión imposible que alguien resolverá tomando otro vehículo sin registrarlo — y a partir de ahí el kilometraje, el combustible y los peajes quedan atribuidos al vehículo equivocado. Toda la conciliación de [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) se contamina.

[DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) establece que el estado del vehículo lo registran **también los propios motoristas desde el campo**: la falla se reporta donde ocurre, no cuando el vehículo llega al taller.

## Condiciones de aplicación

Aplica a asignación, despacho y sustitución.

**No aplica** al traslado del propio vehículo al taller o desde el taller, que es una misión legítima de un vehículo no disponible. Ese traslado se modela como **orden de misión de tipo traslado a taller**, que el sistema admite explícitamente y que exige motorista habilitado igual ([RN-09](RN-09-matriz-licencia-vehiculo.md)).

## Comportamiento esperado

1. El estado operativo es un dato con **historial y vigencia**: quién lo cambió, cuándo, por qué y con qué documento de respaldo.
2. El bloqueo informa el estado y el expediente que lo origina: *"El vehículo <correlativo> está en estado <en taller> desde el <fecha> por la orden de trabajo N.º <folio>."*
3. Una falla reportada desde el campo por el motorista con severidad **incapacitante** cambia el estado de inmediato y **cancela en ruta** las misiones futuras que dependían de ese vehículo, notificando al despacho.
4. El sistema muestra la **fecha estimada de retorno a disponible** cuando el taller la registre, para que la programación pueda planificar en lugar de solo tropezar con el bloqueo.
5. Los períodos de indisponibilidad alimentan el indicador de **disponibilidad de flota** por vehículo, tipo y dependencia.

## Casos límite

- **Falla no incapacitante** — un vidrio roto, aire acondicionado dañado. No bloquea, pero se registra y se muestra como observación al despachar. `[C]` la escala de severidad con el Encargado de Mantenimiento; **no se inventa**.
- **Vehículo que se avería a mitad de ruta.** No se le cambia el estado para "bloquear" una misión que ya está ocurriendo. Se registra el evento en la bitácora, se cambia el estado a partir de ese momento, y la misión sigue su ciclo hasta liquidarse ([RN-06](RN-06-transiciones-de-estado-de-la-orden.md)). Es el caso especial más importante de este módulo y merece su propio `CE-xx` en el Bloque 2.
- **Retorno a disponible sin que el taller cierre la orden de trabajo.** No se permite: sería un vehículo operando con reparación abierta. Si el taller lo libera provisionalmente, lo registra como **liberación condicionada** con restricciones anotadas, que se muestran al despachar.
- **Vehículo resguardado por operativo de Semana Santa.** Es un estado no disponible con fundamento y vigencia acotada, alimentado por el reporte que exige [NRM-02](../normativa/NRM-02-bienes-del-estado.md). Al terminar el período vuelve a disponible automáticamente, y ese es el único retorno automático admitido.
- **Vehículo prestado a otra dependencia dentro de la misma institución.** No es indisponible para la institución, pero sí para la dependencia de origen. El estado se acompaña de la dependencia tenedora, y la agenda de [RN-13](RN-13-sin-doble-asignacion.md) refleja la ocupación.
- **Vehículo robado que aparece.** El retorno a disponible exige el acta correspondiente y la resolución del expediente de M-12; no basta con cambiar el estado.

## Trazabilidad

- Norma: [NRM-02 — Bienes del Estado](../normativa/NRM-02-bienes-del-estado.md)
- Decisión: [DP-001, D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-13](RN-13-sin-doble-asignacion.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-22](RN-22-custodia-del-vehiculo.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md)
- Actores: ACT-04, ACT-05, ACT-06, ACT-11
- Historias y casos especiales: pendientes — Bloque 2
