# HU-066 — Sincronizar sola, en segundo plano, y reanudar sin duplicar ni perder nada

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista · ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** que lo que capturé se envíe solo cuando aparezca señal, sin que yo tenga que hacer nada ni entender qué está pasando
**para** no perder registros por un corte de red a mitad del envío ni terminar con el mismo peaje registrado tres veces

## Contexto

La señal en carretera aparece por dos minutos en la cima de una loma y desaparece. Una sincronización que exige que el usuario la inicie, la vigile y la complete **no se va a completar nunca** en esas condiciones.

El compromiso es duro y medible: **0 registros perdidos, 0 sobrescrituras, y una misión completa con 20 fotografías en menos de 3 minutos sobre 3G** ([RNF-03](../no-funcionales/RNF-03-operacion-sin-conectividad.md)).

Lo que hace posible el reintento seguro es que **el identificador lo genera el cliente**: reenviar el mismo registro no crea un duplicado, porque el servidor lo reconoce y lo ignora ([RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)). Los reenvíos son normales, no un error.

Y el cliente **no envía "la misión está en RETORNADA": envía el diario** — la secuencia completa de transiciones y eventos que produjo. Dos dispositivos no negocian un estado; intercambian transiciones.

## Reglas que la gobiernan

- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — **Regla rectora**: el identificador generado en el cliente es la llave de idempotencia
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Nada se pierde y la captura no se interrumpe por sincronizar
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Nada se resuelve por sobrescritura
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — El orden lo define la secuencia monotónica del dispositivo, no el reloj
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — Cada transición aplicada queda registrada con actor, rol y momento

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — La captura que nunca llega al sistema porque sincronizar es un trámite
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — El odómetro duplicado por un reenvío mal resuelto

## Criterios de aceptación

```gherkin
# language: es
Característica: Sincronización automática, reanudable e idempotente

  Antecedentes:
    Dado un dispositivo portador "DEL-CHO-03" con "34" registros pendientes de la Orden de Misión "OM-2026-0451"
    Y que el dispositivo lleva 4 días sin conectividad

  Escenario: La red se corta a mitad del envío y el reintento no duplica nada
    Dado que aparece señal y se enviaron "20" de "34" registros antes de perderla
    Cuando el dispositivo reintenta la sincronización al recuperar señal
    Entonces el servidor reconoce los "20" registros ya aplicados por su identificador de cliente
    Y los marca como "DUPLICADA_IGNORADA" en lugar de aplicarlos otra vez
    Y aplica únicamente los "14" restantes
    Y el total de registros aplicados es "34", ni uno más ni uno menos

  Escenario: El servidor aplica en orden de secuencia, no en orden de llegada
    Dado que los registros llegan desordenados por reintentos parciales
    Cuando el servidor procesa el diario del dispositivo "DEL-CHO-03"
    Entonces los aplica en orden del número de secuencia del dispositivo
    Y verifica en cada uno el estado origen esperado que el cliente declaró
    Y no aplica ninguna transición saltando una faltante

  Escenario: La sincronización arranca sola y no interrumpe la captura
    Dado que "José Martínez" está registrando un abastecimiento
    Cuando aparece señal
    Entonces la sincronización arranca en segundo plano sin pedir confirmación
    Y "José Martínez" termina de registrar el abastecimiento sin interrupción
    Y el registro nuevo entra a la cola de envío al terminar

  Escenario: El servidor mide y registra el desfase del reloj del dispositivo
    Dado un dispositivo cuyo reloj adelanta "18" minutos
    Cuando el servidor recibe el diario
    Entonces registra el desfase de "+18 minutos" del dispositivo "DEL-CHO-03" en el expediente
    Y no corrige ninguna marca de tiempo capturada
    Y el desfase queda disponible para el auditor: permite corregir el análisis sin corregir el dato

  Escenario: Marca de tiempo incoherente
    Dado un evento con "ocurrido_en" posterior a "capturado_en" fuera de la tolerancia configurada
    Cuando el servidor procesa ese evento
    Entonces no lo corrige ni lo descarta
    Y lo envía a la cola de resolución humana
    Y muestra al responsable "El evento dice que ocurrió después de haberse registrado. Alguien tiene que revisarlo."

  Escenario: Sincronización de una misión completa con 20 fotografías
    Dada una Orden de Misión con "34" registros y "20" fotografías pendientes
    Cuando el dispositivo sincroniza sobre una conexión 3G
    Entonces la sincronización completa termina en menos de "3" minutos
    Y ningún registro queda sin enviar
```

## Fuera de alcance

- La presentación del resultado al usuario y los huecos de secuencia — es [HU-067](HU-067-resultado-registro-por-registro-y-hueco-de-secuencia.md)
- La resolución de los conflictos que la sincronización detecte — es [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- La reconciliación del espejo de ARGOS y Talento Humano, que no usa este mecanismo — es [HU-069](HU-069-el-espejo-nunca-diverge-en-silencio.md)
- El protocolo y la tecnología de transporte: `ADR-000` difiere el stack al Sprint 2. Aquí se describe comportamiento observable

## Notas y pendientes

- `[C]` Enlace real de sede y delegaciones —tipo, ancho de banda, estabilidad— para fijar el umbral de tiempo de sincronización — insumo #68
- `[C]` Volumen operativo: cuántas misiones al mes y cuántos registros por misión — insumo #67
- `[I]` La tolerancia del desfase de reloj es parámetro con vigencia por fecha, no un número fijo
