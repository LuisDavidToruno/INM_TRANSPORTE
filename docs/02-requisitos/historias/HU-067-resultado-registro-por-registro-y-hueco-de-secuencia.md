# HU-067 — Devolver el resultado registro por registro y retener el que espera a su predecesor

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista · ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta cuánto tiempo retiene el servidor una transición cuya predecesora no ha llegado antes de escalarla, y el responsable por puesto de la cola de cada delegación (insumo #76). Sin dueño, la retención se convierte en un registro que nadie desatasca |

## Historia

**Como** Motorista
**quiero** que el sistema me diga qué pasó con **cada** registro que envié, en palabras que yo entienda, y no un "sincronizado: sí"
**para** saber exactamente qué quedó registrado y qué falta, sin tener que preguntar a nadie de oficina

## Contexto

Un "sincronizado con éxito" que en realidad significa "de 34 registros, 31 se aplicaron, 1 espera a otro que no llegó y 2 están en conflicto" es una mentira operativa. El día que se descubra, el motorista deja de confiar y vuelve al papel.

Cuando falta un registro intermedio de la secuencia —llega la 41 y falta la 40— el servidor **no aplica ni rechaza**: retiene la 41 en espera de su predecesor. **Nunca aplica una transición saltando una faltante**, porque eso produciría una misión `RETORNADA` sin odómetro de salida.

Y un hueco que no se cierra **no puede quedar en espera indefinida**: escala a la cola de resolución humana, porque mientras espera, la misión no se puede liquidar ni cerrar.

## Reglas que la gobiernan

- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — **Regla rectora**: nada se descarta; lo que no se aplica se conserva y se muestra
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Cada registro tiene identidad propia y su resultado se informa por separado
- [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) — Cadena incompleta y distinción entre pendiente y ausente
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — Cada transición se evalúa contra su estado origen esperado
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — El orden lo da la secuencia del dispositivo

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — El motorista que deja de confiar en el dispositivo
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — La misión que aparece retornada sin su odómetro de salida

## Criterios de aceptación

```gherkin
# language: es
Característica: Resultado de la sincronización registro por registro

  Antecedentes:
    Dado un dispositivo portador "DEL-CHO-03" con "34" registros enviados de la Orden de Misión "OM-2026-0451"
    Y que el dispositivo estuvo 4 días sin conectividad antes de este envío

  Escenario: No se admite un resultado global que oculte lo no aplicado
    Cuando el servidor devuelve el resultado del envío
    Entonces el cliente muestra el estado de cada uno de los "34" registros
    Y no presenta ningún resumen del tipo "sincronizado" que agrupe estados distintos
    Y no oculta ningún registro en conflicto hasta que alguien lo resuelva

  Escenario: El resultado se expresa en lenguaje del negocio
    Cuando el cliente muestra el resultado del envío
    Entonces cada registro aparece como "enviado y aceptado", "esperando un registro anterior que no ha llegado", "ya estaba registrado" o "necesita que alguien decida"
    Y ningún texto de la pantalla contiene "merge", "versión del registro", "timestamp" ni "conflicto de escritura"

  Escenario: Falta un registro intermedio y el posterior queda retenido
    Dado que llegó el registro de secuencia "41" y no llegó el "40"
    Cuando el servidor procesa el registro "41"
    Entonces no lo aplica ni lo rechaza
    Y lo deja en estado "EN_ESPERA_DE_PREDECESOR"
    Y el cliente muestra "Este registro espera a otro anterior que todavía no llegó."

  Escenario: El hueco se cierra y todo se aplica en orden
    Dado un registro "41" en espera de su predecesor
    Cuando llega el registro de secuencia "40"
    Entonces el servidor aplica primero el "40" y luego el "41"
    Y ambos quedan en estado "APLICADA"

  Escenario: Nunca se aplica una transición saltando una faltante
    Dado que falta el registro de salida con el odómetro inicial de "OM-2026-0451"
    Cuando llega el registro de retorno de la misma misión
    Entonces el servidor no aplica el retorno
    Y la misión no queda en estado "RETORNADA" sin odómetro de salida
    Y muestra al responsable "Llegó el retorno de OM-2026-0451 pero falta el registro de salida. No se aplica hasta resolverlo."

  Escenario: El hueco no se cierra en el plazo y escala a resolución humana
    Dado un registro "41" en espera de su predecesor desde hace más del plazo configurado
    Cuando vence ese plazo
    Entonces el servidor lo envía a la cola de resolución humana del responsable de la delegación
    Y muestra "Un registro de OM-2026-0451 lleva esperando 5 días a otro que no llegó. Esta misión no se puede liquidar hasta resolverlo."

  Escenario: Adjunto que no llegó no es conflicto
    Dado un consumo de combustible aplicado y su fotografía todavía no enviada
    Cuando el cliente muestra el resultado del envío
    Entonces el consumo aparece como "enviado y aceptado"
    Y la fotografía aparece como "pendiente de enviar", no como conflicto ni como ausente
```

## Fuera de alcance

- El mecanismo de envío y su idempotencia — es [HU-066](HU-066-sincronizar-sola-y-reanudable.md)
- La pantalla de resolución del conflicto — es [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- El contador de pendientes en el dispositivo — es [HU-054](HU-054-pendientes-de-envio-y-adjunto-pendiente.md)

## Notas y pendientes

- `[C]` Cuánto tiempo retiene el servidor una transición cuya predecesora no ha llegado, antes de escalarla — insumo #76
- `[C]` Responsable por puesto de la cola de cada delegación — insumo #76
