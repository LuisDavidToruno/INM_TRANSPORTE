# HU-006 — Señalar los tramos en día u hora inhábil sin bloquear la solicitud

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-02 Solicitante |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Solicitante
**quiero** que el sistema me señale qué tramos de la ventana solicitada caen en día inhábil, feriado u hora inhábil, y me advierta que la circulación requerirá permiso de la máxima autoridad, **sin impedirme capturar ni enviar**
**para** saber desde el primer momento qué trámite adicional tengo por delante, y para no verme obligado a declarar una fecha falsa con tal de que el sistema me deje continuar

## Contexto

Circular un vehículo del Estado en día u hora inhábil requiere permiso firmado por la máxima autoridad `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md). La tentación de diseño es bloquear la captura, y es un error: **el bloqueo temprano produce un deadlock cuya única salida es que el usuario mienta en la fecha** — corrección `HB1-08` de [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md).

El bloqueo real está **en el despacho** (`BD-04`). Aquí solo se señala, se advierte y se deja la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD`, que **no se puede retirar a mano**.

El calendario se evalúa **a la fecha prevista de salida y con el horario de la delegación**, no con la fecha de hoy ni con el horario de la sede: una delegación fronteriza de atención continua no tiene el mismo horario hábil que la oficina central.

## Reglas que la gobiernan

- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — Circular en día u hora inhábil requiere permiso vigente firmado por la máxima autoridad. Aquí **solo se marca**
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Calendario, feriados y horario hábil son parámetros con vigencia, nunca constantes
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — El calendario que se aplica es el vigente a la fecha prevista de salida
- [RN-24](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) — La excepción de circulación es atributo del vehículo, no del viaje: aquí no se puede resolver porque aún no hay vehículo asignado
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — La marca acompaña al expediente y se conserva en la transición a `APROBADA`

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` se materializa aquí. El que roza el flujo es [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) —la prórroga en ruta que empuja la misión a franja inhábil no cubierta—, que ocurre con la misión `EN_RUTA` y queda **fuera** de esta historia

## Criterios de aceptación

```gherkin
# language: es
Característica: Detección de franja inhábil en la ventana solicitada
  Como Solicitante
  quiero ver qué tramos caen en día u hora inhábil
  para saber que la misión requerirá permiso de la máxima autoridad

  Antecedentes:
    Dado un calendario de la delegación "Choluteca" vigente para el año "2026"
    Y un horario hábil de esa delegación de lunes a viernes de "08:00" a "16:00"
    Y el "2026-04-02" declarado feriado en ese calendario
    Y el "2026-03-21" es sábado

  Escenario: La ventana enteramente hábil no produce marca
    Dada una ventana solicitada del "2026-03-18 08:00" al "2026-03-18 15:00"
    Cuando el Solicitante envía la solicitud a autorización
    Entonces el sistema no señala ningún tramo inhábil
    Y el expediente no lleva la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD"

  Escenario: La ventana que toca un sábado se señala y no se bloquea
    Dada una ventana solicitada del "2026-03-20 08:00" al "2026-03-21 17:00"
    Cuando el Solicitante envía la solicitud a autorización
    Entonces el sistema ejecuta el envío
    Y señala el tramo inhábil "sábado 21/03/2026, de 00:00 a 17:00"
    Y muestra "La circulación en esa franja requerirá permiso firmado por la máxima autoridad (RN-23). Puede enviar la solicitud; el permiso se tramita después de la aprobación."
    Y el expediente queda con la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD"

  Escenario: La ventana que solo excede el horario hábil de un día laborable también se señala
    Dada una ventana solicitada del "2026-03-18 06:00" al "2026-03-18 19:00"
    Cuando el Solicitante envía la solicitud a autorización
    Entonces el sistema señala los tramos inhábiles "miércoles 18/03/2026 de 06:00 a 08:00" y "miércoles 18/03/2026 de 16:00 a 19:00"
    Y el expediente queda con la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD"

  Escenario: El feriado intermedio de una misión de varios días se señala como tramo propio
    Dada una ventana solicitada del "2026-04-01 08:00" al "2026-04-03 16:00"
    Cuando el Solicitante envía la solicitud a autorización
    Entonces el sistema señala el tramo inhábil "jueves 02/04/2026, feriado, de 00:00 a 24:00"
    Y muestra la versión y la vigencia del calendario con que se resolvió

  Escenario: Se rechaza retirar la marca a mano
    Dado un expediente con la marca "REQUIERE_PERMISO_MAXIMA_AUTORIDAD"
    Cuando el Solicitante intenta retirar la marca del expediente
    Entonces el sistema rechaza la acción
    Y muestra "La marca solo se extingue cuando existe el permiso vigente que la cubre o cuando la ventana se reprograma a franja hábil. No se retira a mano."

  Escenario: Se evalúa el calendario vigente a la fecha prevista de salida
    Dado un calendario que incorpora el "2026-05-01" como feriado a partir de una vigencia declarada el "2026-04-15"
    Y una captura realizada el "2026-03-14" con salida prevista el "2026-05-01"
    Cuando el Solicitante consulta los tramos inhábiles de su ventana
    Entonces el sistema evalúa con el calendario vigente al "2026-05-01"
    Y señala el "viernes 01/05/2026" como tramo inhábil
```

## Fuera de alcance

- El **bloqueo** por falta de permiso: ocurre en el despacho (`T-12`, `BD-04`), no aquí. Bloquear antes produce el deadlock corregido por `HB1-08`
- La tramitación y firma del permiso — es [HU-016](HU-016-tramite-y-firma-del-permiso-de-circulacion.md)
- La excepción del **vehículo de servicio exceptuado**: no se puede evaluar en `SOLICITADA` porque no hay vehículo asignado ([`INV-08`](../../03-arquitectura/estados/orden-de-mision.md)). Se resuelve al tramitar el permiso
- La carga y el mantenimiento del calendario de feriados y del horario hábil — es M-02, alimentado por el espejo de Talento Humano

## Notas y pendientes

- `[C]` **Horario hábil oficial de la institución**, y el horario propio de delegaciones de atención continua o fronterizas — insumo #32. Los horarios de los criterios son de ejemplo y **son parámetros**
- `[C]` **Legislación posterior sobre los feriados de octubre**, que [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) no pudo verificar — insumo #14. **No se codifica ninguna suposición**
- `[V]` La prohibición de circular en días y horas inhábiles sin permiso de la máxima autoridad consta en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); la cita completa del decreto sigue `[C]` y el eslabón débil está declarado en [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md)
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) paso 7 y excepción E3; [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) excepción E4
