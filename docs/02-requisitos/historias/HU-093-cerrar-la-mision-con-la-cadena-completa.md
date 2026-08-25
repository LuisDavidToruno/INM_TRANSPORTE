# HU-093 — Cerrar la misión con la cadena completa y sellar el expediente como inmutable

| Campo | Valor |
|---|---|
| **Módulo** | M-13 Liquidación y Cierre · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-08 Gerencia Administrativa |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta qué expedientes vinculados abiertos condicionan el cierre —incidente en investigación, orden de trabajo por novedad no atendida— y cuáles no (insumo #1). Es la condición de la transición, no un umbral. Falta también el plazo de retención documental del expediente cerrado (insumo #1) |

## Historia

**Como** Gerencia Administrativa
**quiero** cerrar el expediente de una misión que no cumple ningún criterio de hallazgo, y que a partir de ese momento el expediente sea inmutable y exportable como paquete de evidencia
**para** que lo que se entregue al Tribunal Superior de Cuentas sea reproducible y no cambie según el día en que se imprima

## Contexto

**Cerrar es un acto de Gerencia Administrativa, no de quien liquidó.** El sistema evalúa los criterios y propone; la persona confirma con su justificación.

La inmutabilidad es dura y deliberada: no se modifica ningún dato, ni un odómetro, ni un monto, ni una fecha, ni un motivo, ni un adjunto, **ni siquiera una errata en un campo de texto**. La razón es que si un estado terminal puede cambiar meses después, entonces ningún reporte histórico es reproducible, y un reporte no reproducible no sirve para rendir cuentas.

Por eso también toda salida declara su **fecha de corte de conocimiento**: sin ella, no reabrir el expediente no sirve de nada, porque el reporte cambia igual.

## Reglas que la gobiernan

- [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) — La cadena completa es condición del cierre limpio
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Quien cierra ≠ quien liquidó; el cierre es de ACT-08 sin excepción posible
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El expediente cerrado es inmutable
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Toda salida declara su fecha de corte y es reproducible a esa fecha. **No desactivable**
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — Al cerrar se consolidan los indicadores por causa tipificada
- [RN-81](../../01-negocio/reglas/RN-81-sigti-expone-hechos-a-argos.md) — Los hechos se exponen a ARGOS por la clave de vinculación
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — No se cierra mientras haya datos en camino

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Datos pendientes que no son datos ausentes
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — El cierre de ejercicio no cierra expedientes
- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — Lo que se puede y no se puede hacer después de cerrar

## Criterios de aceptación

```gherkin
# language: es
Característica: Cierre del expediente de la misión sin hallazgo

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "LIQUIDADA"
    Y todas sus asignaciones de fondo en estado "CONCILIADA"
    Y que "Marvin Aguilar" liquidó la misión

  Escenario: Se bloquea el cierre por quien liquidó
    Cuando "Marvin Aguilar" intenta cerrar "OM-2026-0512"
    Entonces el sistema rechaza el cierre y no guarda nada
    Y muestra "Marvin Aguilar liquidó OM-2026-0512 el 30/09/2026. Quien liquida no puede cerrar la misma misión."
    Y el intento queda en la pista de auditoría con el par de incompatibilidad detectado

  Escenario: El cierre en delegación se ejerce desde la sede
    Dado una misión liquidada por el Encargado de Delegación
    Cuando el Encargado de Delegación intenta cerrarla
    Entonces el sistema rechaza el cierre
    Y muestra "El cierre de la misión es acto de Gerencia Administrativa, sin excepción posible."

  Escenario: Se bloquea el cierre con datos pendientes de sincronizar
    Dado un dispositivo de la misión con 12 eventos pendientes de sincronizar
    Cuando la Gerencia Administrativa intenta cerrar "OM-2026-0512"
    Entonces el sistema rechaza el cierre
    Y muestra "OM-2026-0512 tiene 12 eventos pendientes de sincronizar del dispositivo asignado a Wilmer Cáceres. No se cierra con hallazgo por datos que están en camino."

  Escenario: Se bloquea el cierre con una asignación de fondo sin conciliar
    Dado una asignación "ASG-2026-00812" en estado "LIQUIDADA" sin cruzar contra kilometraje
    Cuando la Gerencia Administrativa intenta cerrar "OM-2026-0512"
    Entonces el sistema rechaza el cierre
    Y muestra "La asignación ASG-2026-00812 no está conciliada contra kilometraje. Complete la conciliación: no se cierra sin ella, ni siquiera con hallazgo."

  Escenario: Se cierra la misión limpia y se sella la cadena
    Dado que ningún criterio de hallazgo se cumple
    Cuando la Gerencia Administrativa cierra "OM-2026-0512" con su justificación
    Entonces la misión pasa a estado "CERRADA"
    Y la cadena de auditoría queda sellada con el hash de la última transición
    Y los indicadores se consolidan en los acumulados del vehículo, del motorista y de la dependencia

  Escenario: El expediente cerrado no admite ninguna modificación
    Dado que "OM-2026-0512" está en estado "CERRADA"
    Cuando alguien intenta corregir una errata en el campo de motivo de una desviación
    Entonces el sistema rechaza la modificación
    Y muestra "El expediente OM-2026-0512 está cerrado desde el 02/10/2026. No se modifica ningún dato, ni siquiera una errata. Toda corrección posterior es un asiento reverso visible."

  Escenario: Desde un estado terminal no sale ninguna transición
    Dado que "OM-2026-0512" está en estado "CERRADA"
    Cuando alguien intenta devolver la liquidación o reabrir la misión
    Entonces el sistema rechaza la acción
    Y muestra "OM-2026-0512 está en estado terminal. No hay reapertura, por ningún rol."

  Escenario: El expediente queda exportable como paquete de evidencia
    Cuando el Auditor Interno exporta el expediente de "OM-2026-0512"
    Entonces el paquete contiene índice, documentos, adjuntos, hoja de cálculo y la cadena representada explícitamente
    Y la exportación queda registrada como consulta del Auditor Interno

  Escenario: Todo reporte declara su fecha de corte de conocimiento
    Cuando se genera un reporte que incluye "OM-2026-0512"
    Entonces el reporte declara su fecha de corte de conocimiento
    Y el mismo reporte a la misma fecha de corte produce el mismo resultado
    Y no existe opción de desactivar la fecha de corte

  Escenario: Se exponen los hechos a ARGOS sin escribir en el sistema origen
    Cuando "OM-2026-0512" queda cerrada
    Entonces el sistema expone los hechos de la misión a ARGOS por la clave de vinculación
    Y no escribe ningún dato en el sistema origen
```

## Fuera de alcance

- El cierre con hallazgo — es [HU-094](HU-094-cerrar-con-hallazgo-tipificado.md)
- El hallazgo posterior sobre una misión ya cerrada — es [HU-095](HU-095-registrar-hallazgo-posterior-sobre-mision-cerrada.md)
- La devolución de la liquidación: la ejerce Gerencia Administrativa con motivo obligatorio, conservando la liquidación anterior como versión
- La política de retención documental del expediente cerrado

## Notas y pendientes

- `[C]` **Qué expedientes vinculados abiertos condicionan el cierre** —incidente en investigación, orden de trabajo por novedad no atendida— y cuáles no. El reclamo de peaje **ya no condiciona el cierre** (hallazgo `HB3-02`, ver [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)) — insumo **#1**
- `[C]` ¿Habrá migración de expedientes históricos y con qué alcance? Se marcan como *expediente migrado* y se excluyen de los indicadores de hallazgo — insumo **#1**
- `[C]` Plazo de retención documental del expediente cerrado — insumo **#1**
- `[I]` La cadena de eslabones y la exportación de paquetes de evidencia son implicaciones de requerimiento del equipo derivadas de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), no articulado citable
