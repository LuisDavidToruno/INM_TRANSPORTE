# HU-054 — Saber en todo momento qué está pendiente de enviar y qué adjunto quedó pendiente

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista · ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** ver cuántos registros míos están pendientes de enviar y desde cuándo, y que el dispositivo nunca me diga que algo se envió si no se envió
**para** saber si puedo confiar en lo que llevo capturado antes de entregar el vehículo, y no volver a llevar la libreta como respaldo por desconfianza

## Contexto

La razón por la que un motorista abandona una aplicación de campo no suele ser que falle: es que **no sabe si funcionó**. Si el dispositivo presenta como sincronizado algo que sigue en la cola, la primera vez que se pierda un registro se pierde también la confianza, y ya no vuelve.

Esta historia sostiene el compromiso más duro del proyecto: **7 días sin red, cero pérdida de registros** ([RNF-03](../no-funcionales/RNF-03-operacion-sin-conectividad.md)). Y su corolario operativo: cuando el almacenamiento del dispositivo se llena, **ningún registro se descarta jamás**. Si no cabe una fotografía, se conserva el evento y la fotografía queda declarada **pendiente**, que no es lo mismo que **ausente** ([RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)).

## Reglas que la gobiernan

- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — **Regla rectora**: nada se resuelve por sobrescritura; pendiente y ausente son estados distintos
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La captura nunca se pierde
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Cada registro tiene identificador propio desde que se captura, y el adjunto se vincula por él
- [RN-85](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) — Un adjunto que nunca llega se trata como ausencia de comprobante, no como pendiente eterno

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — El motorista pierde la confianza en el dispositivo y vuelve al papel
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — Comprobante que nunca llega a subir

## Criterios de aceptación

```gherkin
# language: es
Característica: Visibilidad de lo pendiente de envío en el dispositivo de campo

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un dispositivo portador "DEL-CHO-03" asignado al motorista "José Martínez"

  Escenario: El dispositivo no presenta como enviado lo que sigue en cola
    Dado que el dispositivo lleva 4 días sin conectividad y tiene "34" registros pendientes
    Cuando "José Martínez" consulta el estado de su bitácora
    Entonces el sistema muestra "34 registros pendientes de enviar. El más antiguo es del 12/05/2026."
    Y no marca ningún registro como enviado ni como recibido en oficina

  Escenario: Se avisa antes de llegar al techo de almacenamiento
    Dado que el dispositivo tiene ocupado el "85" por ciento de su almacenamiento con "180" fotografías pendientes
    Cuando "José Martínez" abre la Orden de Misión "OM-2026-0451"
    Entonces el sistema muestra "El almacenamiento del teléfono está casi lleno. Las fotos nuevas se guardarán más comprimidas. Ningún registro se va a perder."

  Escenario: Sin espacio para la fotografía, el evento se conserva igual
    Dado que el almacenamiento del dispositivo está lleno
    Cuando "José Martínez" registra un abastecimiento de "15.0" galones con fotografía del comprobante
    Entonces el sistema guarda el abastecimiento completo
    Y marca la fotografía como "PENDIENTE, no cupo en el teléfono"
    Y muestra "El abastecimiento quedó registrado. La foto no cupo: conserve el comprobante en físico y entréguelo al retornar."
    Y no descarta ningún registro

  Escenario: El evento sincroniza aunque su adjunto no
    Dado que aparece señal después de 4 días
    Cuando el dispositivo envía el diario de "OM-2026-0451"
    Entonces el evento de abastecimiento se aplica en el servidor
    Y la fotografía queda vinculada al identificador del evento y sube después
    Y el expediente muestra el comprobante como "PENDIENTE de envío desde el dispositivo", no como ausente

  Escenario: Adjunto pendiente por más del plazo configurado
    Dado un adjunto en estado "PENDIENTE" desde hace "15" días
    Cuando el Jefe de Transporte consulta el expediente de "OM-2026-0451"
    Entonces el sistema muestra "Comprobante pendiente desde hace 15 días. Si no llega, este gasto se descarga como comprobante ausente."
    Y escala la alerta al responsable de la delegación

  Escenario: Cada registro dice si ya fue recibido en oficina
    Dado que la sincronización se completó
    Cuando "José Martínez" consulta el estado de su bitácora
    Entonces el sistema muestra registro por registro su estado: "enviado y aceptado", "esperando un registro anterior", "ya estaba registrado" o "necesita que alguien decida"
    Y no resume el resultado como "sincronizado"
```

## Fuera de alcance

- El envío en sí y la resolución de conflictos en el servidor — es [HU-066](HU-066-sincronizar-sola-y-reanudable.md) y [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- El cifrado del almacenamiento local del dispositivo — es [RNF-13](../no-funcionales/RNF-13-cifrado-en-transito-y-en-reposo.md)
- El mecanismo de compresión de fotografías: `ADR-000` difiere el stack al Sprint 2

## Notas y pendientes

- `[C]` Plazo tras el cual un adjunto pendiente se declara ausente y escala — insumo #1
- `[C]` Dispositivo de campo de referencia, sin el cual el techo de almacenamiento y el volumen de ≥ 200 fotografías se miden contra el equipo del desarrollador — insumo #69
- `[C]` Volumen operativo real: cuántas misiones y cuántas fotografías por misión — insumo #67
