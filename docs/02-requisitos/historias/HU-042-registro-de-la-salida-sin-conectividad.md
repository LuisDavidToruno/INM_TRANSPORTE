# HU-042 — Registrar la salida desde el dispositivo del motorista, sin ninguna conectividad

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) pasos 13 a 16, A3, A4, E9 · `T-14` · `BD-05` · `EF-07` |

## Historia

**Como** Motorista
**quiero** abrir mi misión y registrar la salida —odómetro y hora real del hecho— en mi dispositivo, aunque no tenga señal en el predio
**para** que el viaje quede abierto desde el momento en que realmente salí, y no desde el momento en que alguien tuvo internet

## Contexto

El predio de una delegación rural no tiene señal. Si el registro de salida exige conectividad, el motorista sale igual y el dato se inventa después: hora aproximada, odómetro "el que tenía la orden". Eso destruye la única base sobre la que se puede conciliar galonaje y kilometraje.

`T-14` **se ejecuta sin red, siempre**. La fecha del hecho y la fecha de captura son campos distintos y ambos obligatorios: son las que permiten distinguir "salió tarde" de "registró tarde", que son dos problemas distintos con dos responsables distintos.

Salir fuera de la ventana autorizada **no bloquea**: el vehículo está saliendo, y un sistema que se niega a registrar que salió tres horas tarde deja el hecho fuera del expediente — que es exactamente lo que el auditor busca y no encuentra. Exige justificación y marca la misión para revisión.

## Reglas que la gobiernan

- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — **Regla rectora**: toda captura de campo se completa sin ninguna conectividad y nunca se pierde
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho y fecha de captura son campos distintos, ambos obligatorios
- [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) — Coherencia del odómetro contra la última lectura conocida
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — El kilometraje acumulado es del expediente, no de la lectura del instrumento
- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) — Solo un conductor declarado en la misión puede abrirla en el dispositivo
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Los identificadores de los eventos se generan en el cliente
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Ningún conflicto de sincronización se resuelve por sobrescritura

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Sin dispositivo, la captura es en papel y la digitación es diferida
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — La lectura se registra tal como se ve y la inconsistencia se declara
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Quien abre la misión puede ser un relevo declarado

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro de la salida en el dispositivo del motorista sin conectividad

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" en estado "DESPACHADA"
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014"
    Y un acta de entrega con odómetro inicial de "84520" km
    Y una ventana autorizada de salida del "2026-09-15 06:00" al "2026-09-15 07:00"
      con tolerancia configurada de "30" minutos
    Y un motorista titular "José Martínez" y un relevo declarado "Elder Zavala"
    Y un dispositivo portador sin ninguna conectividad

  Escenario: Se rechaza la apertura por una persona no declarada como conductor
    Cuando "Marvin Discua", que no está declarado en la misión, intenta abrir "OM-2026-0451"
      en el dispositivo
    Entonces el dispositivo rechaza la apertura
    Y muestra "Marvin Discua no está declarado como conductor de esta misión. El camino es la sustitución, que requiere autorización."
    Y registra el intento para sincronizarlo después

  Escenario: Se rechaza un odómetro de salida menor al del acta de entrega
    Cuando el motorista registra un odómetro de salida de "84400" km
    Entonces el dispositivo rechaza el registro
    Y muestra "El odómetro de salida (84,400) es menor al del acta de entrega (84,520). Verifique la lectura."
    Y permite declarar "intervención del instrumento de medición" con respaldo fotográfico

  Escenario: La salida fuera de ventana exige justificación pero no se impide
    Dada la hora del hecho "2026-09-15 09:40"
    Cuando el motorista registra la salida con odómetro "84520" km
    Entonces el dispositivo exige un motivo del catálogo de desviación y un texto libre
    Y muestra "La salida ocurre fuera de la ventana autorizada (06:00 a 07:00, tolerancia 30 minutos). Indique el motivo."
    Y al registrar el motivo la misión pasa a "EN_RUTA"
    Y la misión queda marcada para revisión con la desviación registrada

  Escenario: La salida se registra sin conectividad y distingue hecho de captura
    Dada la hora del hecho "2026-09-15 06:20" y el dispositivo sin señal
    Cuando el motorista registra la salida con odómetro "84520" km
    Entonces el dispositivo acepta el registro sin conectividad
    Y guarda la fecha del hecho "2026-09-15 06:20" y la fecha de captura del dispositivo
    Y la Orden de Misión pasa al estado "EN_RUTA"
    Y el vehículo pasa al estado operativo "EN_MISION"
    Y se abre la bitácora con numeración por secuencia del dispositivo, no por reloj

  Escenario: Un motorista de relevo declarado puede registrar la salida
    Cuando "Elder Zavala" abre "OM-2026-0451" en el dispositivo y registra la salida
    Entonces el dispositivo acepta el registro validando su habilitación contra el paquete local
    Y registra a "Elder Zavala" como conductor del primer tramo

  Escenario: El servidor no infiere nada del silencio posterior
    Dado que la misión pasó a "EN_RUTA" el "2026-09-15" y el dispositivo no ha sincronizado
    Cuando el Jefe de Transporte consulta el tablero el "2026-09-18"
    Entonces la misión aparece en estado "EN_RUTA"
    Y muestra la leyenda "sin sincronizar desde hace 3 días"
    Y el sistema no cambia el estado de la misión por la falta de sincronización

  Escenario: Sin dispositivo, la salida se registra en la hoja de bitácora impresa
    Dado que la misión está marcada como "operada en papel"
    Cuando el motorista sale sin dispositivo de campo
    Entonces la salida se anota en la hoja de bitácora impresa con su folio
    Y la digitación diferida al retorno registra quién digitó y adjunta el original escaneado
    Y la ausencia de dispositivo se imputa como condición institucional en el indicador de oportunidad de registro
```

## Fuera de alcance

- El registro de eventos en ruta, paradas, arribos, consumo e incidentes — es del caso de uso de ejecución en ruta
- La reconciliación de la cola de sincronización al reconectar — es de M-16
- El registro del retorno y el cierre de la bitácora — son de M-08 y M-13
- El seguimiento de posición en tiempo real — es de M-19

## Notas y pendientes

- `[C]` **Dispositivo de campo de referencia**: qué equipos tienen hoy los motoristas, quién los provee y quién paga el plan de datos — insumo #69. Sin dispositivo declarado, las mediciones se hacen contra el equipo del desarrollador, que es diseñar para nadie.
- `[C]` **Tolerancia de salida fuera de ventana** y catálogo de motivos de desviación — insumo #32.
- `[C]` **Plazo máximo de digitación diferida** — insumo #45.
- `[V]` La exigencia de operación sin conectividad proviene de [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) y de la premisa rectora 5.
