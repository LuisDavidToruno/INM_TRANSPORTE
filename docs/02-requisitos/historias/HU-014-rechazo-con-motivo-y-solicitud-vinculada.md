# HU-014 — Rechazar la solicitud con motivo tipificado y encadenar la nueva

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-03 Jefatura Inmediata |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Jefatura Inmediata
**quiero** rechazar una solicitud improcedente con un motivo del catálogo más texto libre, y que el sistema le permita al solicitante crear una solicitud nueva **a partir de** la rechazada, conservando el vínculo entre ambas
**para** que quede rastro legible de que la necesidad se replanteó, en lugar de un expediente que se reabre y se corrige hasta que pasa

## Contexto

`RECHAZADA` es **estado terminal**. No se reabre y no se reintenta sobre el mismo expediente.

La razón es de control: un expediente que se reabre hasta que finalmente se aprueba borra la evidencia de que hubo una negativa. Dos expedientes vinculados —uno rechazado y otro nuevo que lo cita— dejan un rastro que el auditor puede leer, y le permiten a la Gerencia Administrativa ver el patrón: qué dependencias insisten con solicitudes que ya fueron negadas y por qué.

El rechazo también es acto de autoridad sobre el expediente: **la segregación se evalúa aquí igual que al autorizar**. El solicitante no puede rechazar su propia solicitud.

## Reglas que la gobiernan

- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — `T-06` lleva a `RECHAZADA`, estado terminal; toda transición registra actor, rol, momento y motivo
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Rechazar es acto de autoridad; el solicitante no lo ejerce sobre su propio expediente
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El rechazo se registra de forma inmutable con identidad, rol ejercido y momento
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — El expediente rechazado no se borra ni se recicla su número
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — El correlativo del expediente rechazado no vuelve a asignarse

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` toca este flujo. Constancia dejada

## Criterios de aceptación

```gherkin
# language: es
Característica: Rechazo de una solicitud de transporte
  Como Jefatura Inmediata
  quiero rechazar con motivo tipificado
  para que la negativa quede documentada y no se borre reabriendo el expediente

  Antecedentes:
    Dado un expediente "CHO-2026-00087" en estado "SOLICITADA"
    Y un catálogo de motivos de rechazo con las entradas "No corresponde a la función institucional", "Gasto no justificado", "Fecha no viable" y "Duplica una misión ya autorizada"
    Y una Jefatura Inmediata "Rolando Discua" con competencia sobre el expediente

  Escenario: Se bloquea el rechazo ejercido por el solicitante de derecho
    Dado un expediente cuyo solicitante de derecho es "Rolando Discua"
    Cuando "Rolando Discua" intenta rechazar el expediente "CHO-2026-00087"
    Entonces el sistema no ejecuta el rechazo
    Y muestra "Usted figura como solicitante de derecho. Rechazar es un acto de autoridad sobre el expediente y no lo ejerce quien solicita (RN-01)."

  Escenario: Se rechaza el registro sin motivo del catálogo
    Cuando "Rolando Discua" intenta rechazar el expediente indicando solo texto libre
    Entonces el sistema no ejecuta el rechazo
    Y muestra "Seleccione un motivo del catálogo. El texto libre complementa el motivo tipificado, no lo sustituye."

  Escenario: El expediente rechazado no se reabre
    Dado un expediente "CHO-2026-00087" en estado "RECHAZADA"
    Cuando el Solicitante intenta devolver el expediente a borrador para corregirlo
    Entonces el sistema rechaza la acción
    Y muestra "RECHAZADA es estado terminal. Cree una solicitud nueva a partir de esta; ambas quedarán vinculadas."

  Escenario: El rechazo se registra y se notifica con su motivo
    Cuando "Rolando Discua" rechaza el expediente con motivo "Gasto no justificado" y texto "El destino se atiende con la misión OM-2026-0402 del mismo día"
    Entonces el expediente pasa a estado "RECHAZADA"
    Y el Solicitante ve "Rechazado por Rolando Discua el 14/03/2026 — Gasto no justificado: El destino se atiende con la misión OM-2026-0402 del mismo día."
    Y el registro del rechazo no se edita después

  Escenario: La nueva solicitud conserva el vínculo con la rechazada
    Dado un expediente "CHO-2026-00087" en estado "RECHAZADA"
    Cuando el Solicitante crea una solicitud nueva a partir de "CHO-2026-00087"
    Entonces la solicitud nueva recibe el número "CHO-2026-00088"
    Y registra el vínculo "Derivada de CHO-2026-00087, rechazada el 14/03/2026 por Gasto no justificado"
    Y ese vínculo es visible para el autorizador de la solicitud nueva

  Escenario: El reporte de control interno muestra el encadenamiento por dependencia
    Dada la dependencia "Subgerencia de Operaciones" con 3 solicitudes derivadas de expedientes rechazados en el trimestre
    Cuando Auditoría Interna consulta el reporte de control interno del trimestre
    Entonces el reporte muestra las 3 cadenas con su expediente de origen y el motivo de cada rechazo
```

## Fuera de alcance

- La **devolución** para corrección menor — es [HU-013](HU-013-devolucion-para-correccion-con-versionado.md), y existe precisamente para no rechazar por errores de digitación
- La anulación administrativa por desistimiento del solicitante o por Gerencia Administrativa (`T-07`) — es historia aparte del backlog de M-06
- Un límite a la cantidad de veces que una necesidad puede replantearse tras un rechazo: no se define aquí, se mide y se expone

## Notas y pendientes

- `[C]` **Contenido del catálogo de motivos de rechazo** — insumo #1. Los motivos de los criterios son de ejemplo; el catálogo es configurable por la institución
- `[I]` La exigencia de que el rechazo no se reabra es coherente con la máquina de estados, que es autoridad: desde los terminales no sale ninguna transición
- Trazabilidad: [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) flujo alterno A3; transición `T-06`; invariante `INV-39`
