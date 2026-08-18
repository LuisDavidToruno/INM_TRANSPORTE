# HU-069 — Que el espejo de ARGOS y Talento Humano nunca diverja en silencio

| Campo | Valor |
|---|---|
| **Módulo** | M-20 Integraciones · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-04 Jefe de Transporte · ACT-05 Encargado de Despacho · ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Encargado de Despacho
**quiero** que el sistema me diga con qué antigüedad de datos estoy trabajando antes de asignar un motorista, y que degrade explícitamente cuando la sincronización lleva días detenida
**para** no despachar contra un espejo viejo que dice que el motorista está activo cuando Talento Humano lo tiene de vacaciones desde el lunes

## Contexto

SIGTI **no reimplementa** lo que ya poseen ARGOS y Talento Humano ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). Lo que guarda de ellos es un **espejo de solo lectura**, y el dueño del dato es el sistema origen.

Por eso el espejo **no entra a la cola de conflictos**: ahí no hay dos versiones legítimas que una persona deba arbitrar. El origen prevalece y la divergencia se corrige por **reconciliación** ([RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md), [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md)).

Lo que sí es común a ambos mecanismos: **nunca diverger en silencio** ([RNF-07](../no-funcionales/RNF-07-sincronizacion-del-espejo-local.md)). `ADR-001` lo dice sin rodeos — la divergencia silenciosa *"es la peor forma de fallar"*. Un despacho hecho contra un espejo de nueve días no es un error del despachador si el sistema no le dijo que el dato tenía nueve días.

## Reglas que la gobiernan

- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — **Regla rectora**: los datos de ARGOS y Talento Humano son espejo de solo lectura y no se editan desde SIGTI
- [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) — El espejo se reconcilia contra el origen y cada entidad muestra su última sincronización
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Superado el umbral, el sistema degrada explícitamente antes de operar
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — El principio común: nada diverge en silencio
- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) — La disponibilidad del motorista se resuelve contra el espejo de Talento Humano

## Casos especiales que la afectan

- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — El motorista que Talento Humano tiene no disponible y el espejo todavía no sabe
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — La habilitación que cambió en el origen sin que el espejo lo reflejara

## Criterios de aceptación

```gherkin
# language: es
Característica: Espejo de solo lectura, reconciliación y degradación explícita

  Antecedentes:
    Dado un espejo local del expediente de motoristas proveniente de Talento Humano
    Y un umbral de antigüedad de espejo configurado en "48" horas para advertir y "7" días para bloquear

  Escenario: Se rechaza editar en SIGTI un dato del sistema origen
    Cuando el Jefe de Transporte intenta modificar la fecha de reincorporación de "José Martínez" en el expediente espejo
    Entonces el sistema rechaza la edición
    Y muestra "Este dato lo administra Talento Humano. Corríjalo allá; aquí solo se refleja."

  Escenario: Se bloquea despachar con el espejo detenido más allá del umbral
    Dado que el espejo de Talento Humano lleva "9" días sin reconciliarse
    Cuando el Encargado de Despacho intenta asignar a "José Martínez" a la Orden de Misión "OM-2026-0470"
    Entonces el sistema rechaza la asignación
    Y muestra "Los datos de Talento Humano tienen 9 días sin actualizarse. No se puede verificar si José Martínez está disponible. Avise al Administrador del Sistema."

  Escenario: Advertencia explícita con el espejo desactualizado dentro del rango tolerado
    Dado que el espejo de Talento Humano lleva "60" horas sin reconciliarse
    Cuando el Encargado de Despacho abre la asignación de "OM-2026-0470"
    Entonces el sistema muestra "Datos de Talento Humano actualizados hace 60 horas. Verifique disponibilidad antes de despachar."
    Y permite continuar dejando constancia de que se operó con espejo desactualizado

  Escenario: Cada entidad declara su última sincronización
    Cuando el Jefe de Transporte consulta el expediente espejo de "José Martínez"
    Entonces el sistema muestra "Datos de Talento Humano al 14/05/2026 08:00"
    Y no presenta ningún dato del espejo sin su fecha de última sincronización

  Escenario: La divergencia con el origen se corrige por reconciliación, no por cola de conflictos
    Dado que el espejo dice que "José Martínez" está activo y Talento Humano lo tiene de vacaciones desde el "2026-05-11"
    Cuando se ejecuta la reconciliación periódica
    Entonces el espejo toma el valor del sistema origen
    Y el conflicto no entra a la cola de resolución humana de sincronización de campo
    Y el sistema registra la divergencia detectada con su fecha, para el reporte de calidad de la integración

  Escenario: Se despachó contra espejo desactualizado y al reconciliar el bloqueo duro falla
    Dado un despacho ejecutado el "2026-05-12" con espejo de "48" horas de antigüedad
    Cuando al reconciliar resulta que "José Martínez" estaba de vacaciones desde el "2026-05-11"
    Entonces el sistema no revierte el hecho: la misión ya se ejecutó
    Y abre el hallazgo "H-07" con expediente propio
    Y notifica al Jefe de Transporte y al Auditor Interno
    Y el hallazgo no imputa responsabilidad a nadie: es marca de seguimiento

  Escenario: El sistema no escribe en el sistema origen
    Cuando SIGTI expone a ARGOS los hechos de la misión "OM-2026-0451"
    Entonces envía los hechos con la clave de vinculación de la orden
    Y no modifica ningún registro dentro de ARGOS
```

## Fuera de alcance

- Los contratos de API de ARGOS y Talento Humano — dependen de los insumos #16 y #17
- La cola de conflictos de la captura de campo, que es otro mecanismo — es [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- El modo delegación desconectada —autorizar y despachar sin red contra un espejo viejo— cuya habilitación es decisión del PO, insumo #41

## Notas y pendientes

- `[C]` Umbral de antigüedad del espejo para advertir y para bloquear, por tipo de entidad — insumo #68
- `[C]` Periodicidad de la reconciliación y ventana en que puede ejecutarse — insumos #68 y #72
- `[C]` ¿Se habilita el modo delegación desconectada y con qué tope de días? — insumos #11 y #41
- `[C]` Contratos de API de ARGOS y de Talento Humano — insumos #16 y #17
