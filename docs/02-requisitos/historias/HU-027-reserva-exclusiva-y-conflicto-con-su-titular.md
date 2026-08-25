# HU-027 — Reservar el vehículo y el motorista en exclusiva, y mostrar el conflicto con su titular

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor** | ACT-04 Jefe de Transporte · ACT-08 Gerencia Administrativa (único que desplaza) · ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 10, A3, A5, E6 · `T-08`, `T-11` · `BD-11` · `EF-01` |

## Historia

**Como** Jefe de Transporte
**quiero** que la programación reserve vehículo y motorista **en exclusiva sobre la ventana efectiva, holguras incluidas**, y que cuando el recurso ya esté tomado el sistema me muestre quién lo tiene y me ofrezca los caminos de salida en orden
**para** que dos misiones no se descubran compartiendo el mismo pickup el día de la salida, y para que ninguna misión pierda su vehículo en silencio

## Contexto

Dos misiones con el mismo vehículo el mismo día es el error que termina con un servidor público esperando en la puerta y con una dependencia que no volverá a confiar en el sistema. **El sistema no sobre-asigna, ni siquiera con advertencia.**

La ventana que se reserva no es la solicitada: es `[salida − holgura previa, retorno + holgura posterior]`. Un vehículo que retorna a las 17:00 no puede salir en otra misión a las 17:15: hay revisión, combustible y entrega de custodia en el medio.

Cuando hay conflicto, el sistema **no encola en silencio ni rechaza a secas**: muestra el conflicto con su titular y ofrece cuatro caminos en orden — consolidar, asignar otro recurso, reprogramar, escalar la prioridad. Y el último solo lo puede ejercer la Gerencia Administrativa, **liberando explícitamente la misión desplazada a la cola**. Cada conflicto registrado con su resolución es la medición del déficit de flota, y es uno de los pocos indicadores que la institución puede llevar a una gestión presupuestaria con evidencia.

## Reglas que la gobiernan

- [RN-13](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) — Un motorista y un vehículo no pueden estar asignados a dos misiones con ventanas traslapadas
- [RN-56](../../01-negocio/reglas/RN-56-prelacion-entre-solicitudes-que-compiten.md) — La adjudicación del recurso escaso aplica el criterio parametrizado y deja constancia de las desplazadas
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — Los conflictos y sus resoluciones se acumulan por causa tipificada
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — Liberar recursos es una transición con actor, momento y motivo, no un efecto lateral

## Casos especiales que la afectan

- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — Dos solicitudes aprobadas compiten por el único vehículo compatible
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — La reserva constituida no sobrevive a la indisponibilidad sobrevenida sin desenlace explícito

## Criterios de aceptación

```gherkin
# language: es
Característica: Reserva exclusiva de recursos y resolución del conflicto

  Antecedentes:
    Dado un vehículo "Pickup Toyota Hilux" con correlativo institucional "INS-P-014"
    Y un motorista "José Martínez" habilitado para ese vehículo
    Y una holgura previa configurada de "2" horas y una holgura posterior de "4" horas
    Y una Orden de Misión "OM-2026-0451" de la Gerencia de Operaciones, en estado "PROGRAMADA",
      con el "INS-P-014" reservado del "2026-09-10 07:00" al "2026-09-10 17:00"

  Escenario: Se rechaza la segunda asignación por solapamiento dentro de la holgura posterior
    Dada una solicitud "SOL-2026-0360" con ventana del "2026-09-10 19:00" al "2026-09-10 22:00"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-014" a "SOL-2026-0360"
    Entonces el sistema rechaza la asignación
    Y muestra "El vehículo INS-P-014 está reservado por la Orden de Misión OM-2026-0451 de la Gerencia de Operaciones hasta el 10/09/2026 21:00, incluida la holgura posterior de 4 horas."
    Y no ofrece la opción de asignar de todos modos

  Escenario: El conflicto se presenta con su titular y con los cuatro caminos en orden
    Dada una solicitud "SOL-2026-0360" con ventana del "2026-09-10 08:00" al "2026-09-10 12:00"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-014" a "SOL-2026-0360"
    Entonces el sistema muestra la misión "OM-2026-0451", la Gerencia de Operaciones
      y la franja "10/09/2026 05:00 a 21:00" como titular del recurso
    Y ofrece, en este orden: "Consolidar con OM-2026-0451", "Asignar otro recurso",
      "Reprogramar una de las dos", "Escalar la prioridad"
    Y registra el conflicto con su causa tipificada aunque el Jefe de Transporte no resuelva ahora

  Escenario: Se rechaza el desplazamiento por prioridad ejercido por quien no es Gerencia Administrativa
    Cuando el Jefe de Transporte intenta desplazar la misión "OM-2026-0451" por prioridad superior
    Entonces el sistema rechaza la acción
    Y muestra "Solo la Gerencia Administrativa puede desplazar una programación existente. Escale la solicitud."
    Y registra el intento

  Escenario: El desplazamiento por prioridad libera la misión desplazada a la cola
    Cuando la Gerencia Administrativa desplaza la misión "OM-2026-0451"
      con motivo "desplazada por prioridad superior"
    Entonces la misión "OM-2026-0451" vuelve al estado "APROBADA" conservando su aprobación original
    Y se liberan las reservas del vehículo "INS-P-014" y del motorista "José Martínez"
    Y el folio reservado de "OM-2026-0451" queda anulado y no se recicla
    Y se notifica a la Gerencia de Operaciones el desplazamiento con su motivo
    Y queda constancia de la solicitud desplazada para el indicador de déficit de flota

  Escenario: Desprogramar libera las reservas y conserva la aprobación
    Cuando el Jefe de Transporte desprograma "OM-2026-0451" con motivo "cambio de requerimiento"
    Entonces la solicitud vuelve al estado "APROBADA" sin volver a autorizarse
    Y el vehículo "INS-P-014" vuelve al estado operativo "DISPONIBLE"
    Y el motorista "José Martínez" queda libre en esa franja
    Y el folio reservado queda anulado

  Escenario: Se acepta la reserva cuando no hay solapamiento ni en las holguras
    Dada una solicitud "SOL-2026-0361" con ventana del "2026-09-11 08:00" al "2026-09-11 16:00"
    Cuando el Jefe de Transporte asigna el "INS-P-014" y a "José Martínez" a "SOL-2026-0361"
    Entonces el sistema acepta la asignación
    Y la reserva registrada abarca del "2026-09-11 06:00" al "2026-09-11 20:00"
    Y el vehículo pasa al estado operativo "ASIGNADO"
```

## Fuera de alcance

- El **criterio de prelación** que decide cuál de dos solicitudes se queda con el recurso — no está definido y **no se inventa**
- La consolidación de dos misiones en una sola Orden — es [HU-030](HU-030-consolidacion-de-solicitudes-compatibles.md)
- La sustitución del recurso ya reservado por otro — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)
- **La segregación de funciones sobre el motorista que se reserva** (`I-11`, [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)) — es [HU-025](HU-025-habilitacion-de-quien-efectivamente-conduce.md), que la evalúa al declarar al conductor, **antes** de que exista reserva que constituir. Delimitación escrita por `HB34-02`: la reserva es exclusividad de franja, no habilitación de la persona, y no vuelve a evaluar lo que `HU-025` ya bloqueó
- El reporte de conflictos acumulados como medición del déficit de flota — es de M-14

## Notas y pendientes

- `[C]` **Criterio de prelación entre solicitudes que compiten** — insumo #31. Aparece la primera semana de operación. Mientras no exista, el sistema muestra el conflicto y **exige decisión humana registrada**; no adjudica solo.
- `[C]` **Valores de las holguras previa y posterior**, por institución y por tipo de vehículo — insumo #1 / #32. La lógica no depende de los valores; los valores son parámetros con vigencia.
