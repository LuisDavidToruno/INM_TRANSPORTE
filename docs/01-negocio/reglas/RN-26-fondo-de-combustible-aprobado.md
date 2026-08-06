# RN-26 — El fondo de combustible lo solicita el Jefe de Transporte y lo aprueba la Gerencia Administrativa; sin fondo vigente no hay asignación

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-13 |
| **Origen** | Decisión [DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) y `PROP-01` de [insumos-pendientes](../../07-gestion/insumos-pendientes.md); norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` la decisión de producto — `[C]` el mecanismo real de la institución (insumo #7 reencuadrado) |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — `periodicidad_fondo` y `tolerancia_sobregiro`, esta última con valor inicial cero |

## Enunciado

Toda asignación de combustible **debe** imputarse a un **fondo aprobado vigente**, constituido por:

1. **Solicitud** de ACT-04 Jefe de Transporte, con monto o cantidad de órdenes de pago y justificación operativa del período
2. **Aprobación** de ACT-08 Gerencia Administrativa, con monto aprobado, fecha, aprobador y partida contra la que se afecta
3. **Entrega** registrada del efectivo o de las órdenes de pago

El sistema **no debe** permitir asignar combustible si no existe fondo vigente con **saldo disponible suficiente**. Quien solicita el fondo **no puede** ser quien lo aprueba ([RN-01](RN-01-segregacion-de-funciones.md)).

## Justificación

[DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) reencuadró M-09: SIGTI **no compra combustible ni gestiona contratos**. Modela un fondo asignado y su consumo. `PROP-01` describe el mecanismo con precisión y explica por qué: *"el punto de fuga clásico es el efectivo sin trazabilidad"*.

Un fondo con saldo controlado es lo que impide que se asigne más combustible del aprobado y que la diferencia aparezca meses después, sin responsable, en un cruce del TSC entre lo entregado y lo presupuestado.

## Condiciones de aplicación

Aplica a toda asignación de combustible, en sede y en delegaciones.

`[C]` Decisiones abiertas de `PROP-01` que condicionan esta regla y **no se resuelven por inferencia**:
- ¿El fondo se asigna por período (mensual) o por misión?
- ¿La orden de pago es documento con folio preimpreso o la genera el sistema?

Hasta que se confirmen, el sistema modela el fondo como **entidad con período de vigencia y saldo**, que admite ambos esquemas.

## Comportamiento esperado

1. El fondo tiene estado: solicitado, aprobado, entregado, agotado, cerrado. Cada transición con actor, fecha y motivo.
2. El saldo disponible se calcula como aprobado − asignado + devoluciones liquidadas. Se muestra **antes** de cada asignación.
3. Si la asignación excede el saldo, el sistema **bloquea** e indica cuánto falta y qué fondo lo cubriría. Con `tolerancia_sobregiro` en cero — su valor inicial — no hay excepción.
4. El cierre del fondo exige que **todas** sus asignaciones estén liquidadas ([RN-29](RN-29-liquidacion-de-combustible.md)) o formalmente anuladas.
5. El sistema reporta, por período y dependencia: fondo aprobado, asignado, consumido, devuelto y saldo — el cuadre que Gerencia Administrativa presenta.

## Casos límite

- **Fondo agotado a mitad de mes con misiones urgentes pendientes.** Se solicita ampliación, que sigue el mismo circuito. No hay asignación sin fondo: si se permitiera, el control se pierde exactamente cuando más presión hay.
- **Delegación con fondo propio.** El fondo tiene ámbito (institución, dependencia o delegación) y las asignaciones solo pueden imputarse a fondos de su ámbito. `[C]` confirmar si las delegaciones manejan fondo propio.
- **Fondo entregado en efectivo y en órdenes de pago simultáneamente.** Son dos instrumentos con conciliación distinta. El fondo registra la composición, y cada asignación indica con qué instrumento se hizo.
- **Cambio de Jefe de Transporte con fondo abierto.** El fondo no se cierra por rotación: se registra el traspaso de responsabilidad con acta y saldo verificado, análogo a la custodia de vehículo ([RN-22](RN-22-custodia-del-vehiculo.md)).
- **Devolución de saldo no consumido al final del período.** Se registra como devolución con constancia, y solo entonces incrementa el saldo disponible. Una devolución declarada pero no constatada no libera saldo.
- **Fondo aprobado sin partida presupuestaria indicada.** La estructura presupuestaria la define ARGOS ([DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). Si el espejo no la tiene disponible, el fondo se registra con partida pendiente y **se bloquea su cierre** hasta que se complete.
- **Solicitud y aprobación por la misma persona en una delegación pequeña.** Bloqueo por [RN-01](RN-01-segregacion-de-funciones.md), con escalamiento por [RN-02](RN-02-escalamiento-de-autorizacion.md). El fondo es dinero: aquí la segregación es más importante, no menos.

## Trazabilidad

- Decisión: [DP-001, D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); `PROP-01` en [insumos-pendientes](../../07-gestion/insumos-pendientes.md)
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-29](RN-29-liquidacion-de-combustible.md)
- Actores: ACT-04, ACT-07, ACT-08, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
