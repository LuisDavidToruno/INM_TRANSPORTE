# HU-021 — Trabajar la cola de programación con la caducidad de cada aprobación a la vista

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 1 y E7 · `T-08`, `T-09` |

## Historia

**Como** Jefe de Transporte
**quiero** ver la cola de solicitudes aprobadas ordenada por proximidad de salida, con la fecha de caducidad de cada aprobación y una marca sobre las que ya caducaron
**para** programar primero lo que sale antes y depurar la cola con motivo tipificado, en lugar de arrastrar aprobaciones muertas que ocultan el déficit real de flota

## Contexto

Hoy la cola vive en un cuaderno y en el correo de la jefatura. Una solicitud aprobada para el 3 de septiembre que nadie programó sigue apareciendo como pendiente en octubre, y cuando la Gerencia Administrativa pregunta cuántos servicios no se pudieron atender, nadie tiene el número. Una cola que nadie depura no es un descuido administrativo: **es el indicador de déficit de flota borrado**, y es justo el dato con el que la institución podría sostener una gestión presupuestaria con evidencia ([`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md)).

La caducidad no anula sola: exige un acto con autor y motivo, porque anular una necesidad de movilización de una dependencia es una decisión, no un vencimiento silencioso.

## Reglas que la gobiernan

- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — Cada transición registra actor, rol, momento y motivo; `T-09` anular no es excepción
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — Los indicadores se acumulan por **causa tipificada**; un texto libre no produce ningún indicador
- [RN-56](../../01-negocio/reglas/RN-56-prelacion-entre-solicitudes-que-compiten.md) — La adjudicación del recurso escaso aplica criterio parametrizado y deja constancia de las desplazadas
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La anulación es asiento reverso con motivo y autor: la solicitud no desaparece de la cola, queda anulada y visible

## Casos especiales que la afectan

- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — Dos solicitudes aprobadas compiten por el único vehículo compatible: la que se desplaza vuelve a la cola, no se pierde
- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — La cola se sigue trabajando aunque el fondo de combustible del período esté agotado

## Criterios de aceptación

```gherkin
# language: es
Característica: Cola de programación con caducidad visible y depuración tipificada

  Antecedentes:
    Dada la fecha actual "2026-09-01"
    Y una solicitud "SOL-2026-0310" de la Gerencia de Operaciones, aprobada el "2026-08-20",
      con ventana solicitada del "2026-08-28" al "2026-08-29"
    Y una solicitud "SOL-2026-0344" de la Delegación Choluteca, aprobada el "2026-08-29",
      con ventana solicitada del "2026-09-02" al "2026-09-04"

  Escenario: Se rechaza programar una aprobación caducada
    Cuando el Jefe de Transporte intenta programar la solicitud "SOL-2026-0310"
    Entonces el sistema rechaza la programación
    Y muestra "La aprobación de SOL-2026-0310 caducó: la ventana solicitada terminó el 29/08/2026. Anúlela con motivo tipificado o pida a la dependencia una solicitud nueva."
    Y registra el intento en la bitácora de auditoría

  Escenario: Se rechaza anular una solicitud caducada sin motivo tipificado
    Cuando el Jefe de Transporte anula "SOL-2026-0310" escribiendo únicamente el texto "ya no sirve"
    Entonces el sistema rechaza la anulación
    Y muestra "Seleccione el motivo del catálogo: sin flota disponible, sin motorista habilitado, caducada por falta de programación, desistimiento de la dependencia, causa externa. El texto libre es complemento, no sustituto."

  Escenario: Se anula la aprobación caducada con motivo tipificado
    Cuando el Jefe de Transporte anula "SOL-2026-0310" con motivo "caducada por falta de programación"
      y el comentario "no hubo pickup disponible en la ventana"
    Entonces la solicitud pasa al estado "ANULADA"
    Y el asiento de anulación registra al Jefe de Transporte, la fecha "2026-09-01" y el motivo
    Y la solicitud sigue siendo consultable en la cola con el filtro "anuladas"
    Y el hecho suma al indicador de déficit de flota por causa "caducada por falta de programación"
    Y notifica a la Gerencia de Operaciones como dependencia solicitante

  Escenario: La cola señala las solicitudes cuya ventana está por iniciar
    Cuando el Jefe de Transporte abre la cola de programación
    Entonces "SOL-2026-0344" aparece con la marca "sale en 1 día"
    Y muestra la fecha de caducidad de la aprobación "2026-09-04"
    Y las solicitudes se ordenan por fecha de inicio de la ventana solicitada, la más próxima primero

  Escenario: El Encargado de Delegación solo ve la cola de su ámbito territorial
    Cuando el Encargado de Delegación de Choluteca abre la cola de programación
    Entonces ve "SOL-2026-0344"
    Y no ve "SOL-2026-0310", que pertenece a la sede
```

## Fuera de alcance

- La captura y la autorización de la solicitud — son de M-06 y del caso de uso [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md)
- La asignación de vehículo y motorista — es [HU-022](HU-022-compatibilidad-vehiculo-objeto-del-traslado.md) en adelante
- El reporte de déficit de flota construido sobre estas causas tipificadas — es de M-14

## Notas y pendientes

- `[C]` **Plazo de caducidad de la aprobación cuando la ventana solicitada es abierta o de varias semanas.** La postura provisional es que la aprobación caduca al terminar la ventana solicitada. Insumo #32 (plazos) en [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md).
- `[C]` El **criterio de prelación** entre solicitudes que compiten no está definido — insumo #31. Esta historia solo ordena por proximidad de salida; **no adjudica**.
- El catálogo de motivos de anulación es parámetro configurable con vigencia ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)), no una lista en el código.
