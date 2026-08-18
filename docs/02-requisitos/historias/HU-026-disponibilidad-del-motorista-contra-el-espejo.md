# HU-026 — Verificar la disponibilidad del motorista contra el espejo de Talento Humano, y degradar cuando el espejo está viejo

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-20 Integraciones |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 9, A6, E3 · `T-08` · `BD-10` |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema me impida asignar a un motorista con vacaciones, permiso, incapacidad o suspensión de habilitación solapadas con la ventana de la misión, y que me advierta explícitamente cuando esa verificación se hizo sobre datos que llevan días sin sincronizar
**para** no programar contra información vieja y descubrir la ausencia el día de la salida, cuando ya no hay con quién cubrir el servicio

## Contexto

El expediente del empleado es de Talento Humano, no de SIGTI ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)). SIGTI mantiene un **espejo de solo lectura** de la disponibilidad y **no lo edita**. Pero un espejo tiene una edad, y programar contra un espejo de once días es programar a ciegas: la incapacidad que ingresó ayer no está.

La respuesta no es bloquear la operación cuando la integración falla —eso deja a la institución sin poder trabajar—, sino **degradar explícitamente**: advertir antes de asignar, registrar la advertencia en el diario e imprimirla en el documento. Que quien reciba la orden en carretera pueda ver con qué datos se decidió.

La licencia, en cambio, **es dato propio de SIGTI**: no viene del espejo y no se degrada nunca.

## Reglas que la gobiernan

- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) — No se asigna un motorista con permiso, vacaciones o incapacidad vigente según el espejo
- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — Los datos de Talento Humano son espejo de solo lectura y no se editan desde SIGTI
- [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) — Cada entidad muestra su última sincronización
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Si la sincronización lleva detenida más del umbral, el sistema degrada explícitamente antes de operar
- [RN-13](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) — Tampoco se asigna a quien ya está comprometido en otra misión en esa franja

## Casos especiales que la afectan

- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Motorista no disponible por permiso, vacaciones o incapacidad
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La suspensión de habilitación por expediente de M-12 se trata igual que la ausencia

## Criterios de aceptación

```gherkin
# language: es
Característica: Disponibilidad del motorista y edad del espejo de Talento Humano

  Antecedentes:
    Dada una misión con ventana del "2026-09-10" al "2026-09-12"
    Y un motorista "José Martínez" con licencia categoría "C1" vigente hasta el "2027-03-15"
    Y un umbral configurable de antigüedad del espejo de "3" días

  Escenario: Se rechaza por incapacidad solapada con la ventana
    Dado que el espejo de Talento Humano registra para "José Martínez"
      una incapacidad del "2026-09-08" al "2026-09-15"
    Y que el espejo se sincronizó por última vez el "2026-09-01"
    Cuando el Jefe de Transporte intenta asignar a "José Martínez" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "José Martínez tiene incapacidad registrada del 08/09/2026 al 15/09/2026, solapada con la ventana de la misión."
    Y registra el intento con la fecha de la última sincronización del espejo

  Escenario: Se rechaza por vacaciones que cubren parte de la ventana
    Dado que el espejo registra para "José Martínez" vacaciones del "2026-09-11" al "2026-09-20"
    Cuando el Jefe de Transporte intenta asignar a "José Martínez" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "José Martínez tiene vacaciones del 11/09/2026 al 20/09/2026, que se solapan con la misión del 10/09/2026 al 12/09/2026."

  Escenario: Se rechaza por suspensión de habilitación derivada de un expediente
    Dado un expediente de incidente "INC-2026-0044" con suspensión de habilitación de conducir
      para "José Martínez" desde el "2026-09-05" y sin fecha de levantamiento
    Cuando el Jefe de Transporte intenta asignar a "José Martínez" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "José Martínez tiene la habilitación suspendida desde el 05/09/2026 por el expediente INC-2026-0044."

  Escenario: Se advierte antes de asignar cuando el espejo supera el umbral
    Dado que el espejo de Talento Humano se sincronizó por última vez el "2026-08-30"
    Y que la fecha actual es "2026-09-08"
    Y que el espejo no registra ausencias para "José Martínez"
    Cuando el Jefe de Transporte asigna a "José Martínez" a esa misión
    Entonces el sistema muestra la advertencia "La disponibilidad se verificó sobre datos de Talento Humano sincronizados hace 9 días, por encima del umbral de 3 días."
    Y exige el acuse del Jefe de Transporte antes de confirmar
    Y registra la advertencia en el diario de la misión
    Y la advertencia se imprimirá en la Orden de Misión

  Escenario: La novedad posterior del espejo marca la misión ya programada
    Dada una misión "OM-2026-0451" en estado "PROGRAMADA" con "José Martínez" asignado
    Cuando el espejo de Talento Humano incorpora una incapacidad de "José Martínez"
      del "2026-09-09" al "2026-09-13"
    Entonces el sistema marca la misión "OM-2026-0451" como afectada por indisponibilidad del motorista
    Y notifica al Jefe de Transporte con la ventana de la misión y la dependencia solicitante
    Y no cambia el estado de la misión por sí solo

  Escenario: El espejo no se edita desde SIGTI
    Dado que el Jefe de Transporte considera que la incapacidad de "José Martínez" ya terminó
    Cuando intenta modificar el registro de disponibilidad en SIGTI
    Entonces el sistema rechaza la modificación
    Y muestra "La disponibilidad del personal es dato de Talento Humano. Gestione la corrección en el sistema de origen: SIGTI no edita el espejo."
```

## Fuera de alcance

- El contrato técnico de la integración y la mecánica de sincronización — son de M-20 y de [`RNF-07`](../no-funcionales/RNF-07-sincronizacion-del-espejo-local.md)
- La verificación de licencia, que es **dato propio de SIGTI** y no depende del espejo — es [HU-025](HU-025-habilitacion-de-quien-efectivamente-conduce.md)
- La sustitución del motorista que quedó no disponible — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)
- La apertura del expediente que suspende la habilitación — es de M-12

## Notas y pendientes

- `[C]` **Contrato de API de Talento Humano** — insumo #17: expediente, permisos, vacaciones, incapacidades y calendario. Sin él, el espejo es especulación de esquema, aunque la regla de degradación no depende del contrato.
- `[C]` **Umbral de antigüedad del espejo** que dispara la advertencia y, eventualmente, el bloqueo — insumo #17 / #68.
- `[C]` **Qué ocurre con un empleado dado de baja en Talento Humano que tiene misiones abiertas en SIGTI** — pendiente abierto de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md).
