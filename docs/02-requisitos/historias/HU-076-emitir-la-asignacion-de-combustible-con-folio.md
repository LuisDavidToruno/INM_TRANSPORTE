# HU-076 — Emitir la asignación de combustible con folio contra una Orden de Misión programada

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-07 Programación y Despacho |
| **Actor** | ACT-07 Encargado de Combustible |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta saber si la institución opera asignación **por misión o por período** (`PROP-01` / insumo #7): decide la entidad, no un parámetro. Faltan también el procedimiento de ampliación del rango de folios sin conectividad (insumo #1) y el formato en papel del vale vigente (insumo #2) |

## Historia

**Como** Encargado de Combustible
**quiero** emitir la asignación de combustible de una misión ya programada, con folio, instrumento declarado y receptor verificado contra la Orden de Misión
**para** que ningún lempira del fondo salga sin quedar atado a un folio, a un vehículo, a un motorista y a una misión autorizada

## Contexto

El desvío más simple del control de combustible no es falsificar un vale: es sacarlo a nombre de una misión real y cargarlo en otro vehículo. Por eso la emisión verifica que el vehículo y el motorista receptores sean **los asignados a esa orden**, no unos cualesquiera.

La emisión ocurre con la misión en `PROGRAMADA`. El instrumento se imprime y **permanece bajo custodia del Encargado de Combustible**: entregarlo antes del despacho dejaría fondo público en manos de alguien cuya misión aún puede no salir.

## Reglas que la gobiernan

- [RN-32](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) — Estado mínimo `PROGRAMADA` para emitir; el receptor debe ser el vehículo y el motorista de la orden
- [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) — Folio único, responsable receptor, misión vinculada y constancia de recepción
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Los folios se toman de rangos por delegación, para emitir sin conectividad
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Documento impreso con folio, QR de verificación, espacio de firma y sello, y hash del contenido
- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — Sin fondo vigente entregado con saldo no hay asignación
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — El vehículo se identifica por correlativo institucional, no por placa

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — La emisión bloqueada por saldo, no la misión
- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Por qué la emisión y la entrega son dos momentos distintos
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — Sustitución del vehículo después de emitir

## Criterios de aceptación

```gherkin
# language: es
Característica: Emisión de la asignación de combustible de una misión

  Antecedentes:
    Dado un fondo "FND-2026-09-004" en estado "ENTREGADO" con saldo disponible de "L 74,500.00"
    Y una Orden de Misión "OM-2026-0512" en estado "PROGRAMADA"
    Y un vehículo asignado con correlativo institucional "TR-0045", "Pickup Toyota Hilux"
    Y un motorista asignado "Wilmer Cáceres"

  Escenario: Se rechaza emitir sobre una misión que no está programada
    Dado una Orden de Misión "OM-2026-0530" en estado "APROBADA"
    Cuando el Encargado de Combustible intenta emitir la asignación de "OM-2026-0530"
    Entonces el sistema rechaza la emisión
    Y muestra "La Orden de Misión OM-2026-0530 está APROBADA. Para emitir combustible debe estar al menos PROGRAMADA, con vehículo y motorista asignados."

  Escenario: Se rechaza emitir a nombre de un vehículo distinto al de la orden
    Cuando el Encargado de Combustible intenta emitir la asignación de "OM-2026-0512" a nombre del vehículo "TR-0071"
    Entonces el sistema rechaza la emisión
    Y muestra "El vehículo TR-0071 no está asignado a la Orden de Misión OM-2026-0512. El vehículo asignado es TR-0045."

  Escenario: Se rechaza emitir a nombre de un motorista distinto al de la orden
    Cuando el Encargado de Combustible intenta emitir la asignación de "OM-2026-0512" a nombre de "Denis Fúnez"
    Entonces el sistema rechaza la emisión
    Y muestra "Denis Fúnez no es el motorista titular ni de relevo de la Orden de Misión OM-2026-0512. El motorista asignado es Wilmer Cáceres."

  Escenario: Se rechaza emitir sin declarar el instrumento
    Cuando el Encargado de Combustible emite una asignación de "L 4,800.00" para "OM-2026-0512" sin indicar el instrumento
    Entonces el sistema rechaza la emisión
    Y muestra "Indique el instrumento: efectivo, vale u orden de pago. De él depende qué evidencia se exigirá al liquidar."

  Escenario: Se emite la asignación y el instrumento no sale de la custodia
    Cuando el Encargado de Combustible emite una asignación de "L 4,800.00" en vales para "OM-2026-0512" a nombre de "TR-0045" y "Wilmer Cáceres"
    Entonces la asignación queda con folio "ASG-2026-00812" en estado "EMITIDA"
    Y el saldo disponible del fondo pasa a "L 69,700.00"
    Y el sistema muestra "Emitida. El instrumento permanece bajo su custodia hasta el despacho del 24/09/2026."
    Y no se registra ninguna firma de recepción

  Escenario: El documento impreso lleva folio, QR, firma, sello y hash
    Cuando el Encargado de Combustible imprime la asignación "ASG-2026-00812"
    Entonces el documento lleva el folio "ASG-2026-00812", un código QR de verificación, espacio de firma y sello, y el hash del contenido electrónico
    Y lleva el correlativo institucional "TR-0045" del vehículo

  Escenario: El folio se toma del rango de la delegación y se alerta el agotamiento del rango
    Dado un rango de folios asignado a la delegación "San Pedro Sula" con 12 folios disponibles
    Y un umbral de alerta de "20" folios restantes
    Cuando el Encargado de Delegación emite una asignación
    Entonces el sistema toma el siguiente folio del rango de la delegación
    Y muestra "Quedan 11 folios en el rango de la delegación San Pedro Sula. Solicite ampliación del rango."

  Escenario: Se advierte cuando se emite con datos sincronizados hace días
    Dado una delegación cuyo paquete lleva "9" días sin sincronizar
    Y un horizonte de validez declarado de "7" días
    Cuando el Encargado de Delegación emite e imprime la asignación
    Entonces el documento impreso lleva la leyenda "Emitida con datos sincronizados hace 9 días"
```

## Fuera de alcance

- La entrega contra firma al motorista, que ocurre dentro del despacho — es [HU-079](HU-079-entregar-el-fondo-contra-firma-dentro-del-despacho.md)
- El bloqueo por saldo insuficiente y su distinción con la cuota copada — es [HU-077](HU-077-bloquear-la-emision-por-saldo-insuficiente.md)
- El bloqueo por obligación de reintegro abierta del receptor — es [HU-078](HU-078-bloquear-asignacion-a-quien-debe-reintegro.md)
- La impresión de la categoría y tarifa de peaje en la Orden de Misión — es [HU-081](HU-081-imprimir-categoria-y-tarifa-de-peaje-en-la-orden.md)
- La anulación de la asignación emitida — es [HU-080](HU-080-anular-la-asignacion-de-combustible.md)

## Notas y pendientes

- `[C]` **¿La institución opera asignación por misión o por período (motorista + período + fondo)?** El modelo admite ambos esquemas hasta que se confirme — insumo **#7 / `PROP-01`**
- `[C]` **¿Puede un Encargado de Delegación recibir en nombre del motorista?** Previsto en [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) numeral 3 y **no confirmado** — insumo **#1**
- `[C]` Procedimiento de ampliación del rango de folios sin conectividad. Sin él, una delegación desconectada se queda sin poder emitir — insumo **#1**
- `[C]` Formato en papel del vale de combustible vigente en la institución — insumo **#2**
- `[P]` La autorización previa de toda transacción proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) (TSC-NOGECI V-07), con articulado no extraído
