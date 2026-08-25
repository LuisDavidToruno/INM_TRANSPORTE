# HU-082 — Registrar un abastecimiento de combustible en carretera, sin conectividad

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-08 Ejecución y Bitácora · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — faltan los formatos en papel vigentes de la hoja de bitácora y del vale para lograr la paridad exacta que exige `RN-80` (insumo #2), el `desfase_maximo_sin_justificacion` entre fecha del hecho y fecha de captura (insumo #1) y el umbral de monto a partir del cual el comprobante es exigible (insumo #1, con Auditoría Interna) |

## Historia

**Como** Motorista
**quiero** registrar en la estación los galones, el monto, el odómetro y la foto del comprobante sin necesidad de señal
**para** que el consumo quede atado a un kilometraje real en el momento en que ocurre, y no reconstruido de memoria una semana después

## Contexto

El escenario de diseño es un teléfono con señal intermitente o nula, batería limitada, a plena luz del sol, y un servidor que puede no saber nada durante días. Más de 2 millones de personas del área rural hondureña no tienen acceso a internet `[V]` ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)).

**Todo lo que le exija al motorista más de un minuto o más de tres toques por registro se llenará en papel y se digitará después, mal.** El silencio del servidor no es una anomalía: es lo que el diseño espera.

El odómetro al momento de cargar es el ancla. Sin él, el galón no se puede correlacionar con nada — y la correlación es exactamente lo que busca el auditor del TSC `[V]`.

## Reglas que la gobiernan

- [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) — Galones, monto, estación, odómetro y fotografía del comprobante
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La captura funciona sin ninguna conectividad
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho, fecha de captura y modo de captura, las tres registradas
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Ningún conflicto de sincronización se resuelve sobrescribiendo
- [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) — Lo que se llenó en papel se digita con original adjunto y fechas distintas visibles
- [RN-80](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) — La hoja impresa con folio tiene paridad exacta con la pantalla de digitación

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — La bitácora se llenó en papel y se digita días después
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — La estación no emite factura o el comprobante se pierde
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — Lectura de odómetro que no cuadra

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro de abastecimiento en campo

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "EN_RUTA"
    Y un vehículo "TR-0045" con última lectura conocida de "84,500" km
    Y un dispositivo portador designado, sin conectividad

  Escenario: La captura funciona sin ninguna conectividad
    Dado que el dispositivo no tiene señal desde hace "31" horas
    Cuando el motorista abre "registrar abastecimiento"
    Entonces la pantalla se abre precargada con la misión "OM-2026-0512", el vehículo "TR-0045" y el odómetro "84,500" km
    Y admite guardar el registro sin conexión

  Escenario: Se rechaza el abastecimiento sin odómetro
    Cuando el motorista registra "12.0" galones por "L 1,560.00" en la estación "Uno Zambrano" sin capturar el odómetro
    Entonces el sistema rechaza el registro
    Y muestra "Capture el odómetro del momento de la carga. Sin él, estos galones no se pueden correlacionar con ningún kilometraje."

  Escenario: Se rechaza el abastecimiento sin instrumento ni medio de pago
    Cuando el motorista registra "12.0" galones por "L 1,560.00" sin indicar folio de vale, efectivo, orden de pago o tarjeta
    Entonces el sistema rechaza el registro
    Y muestra "Indique el instrumento y el medio de pago. De ellos depende qué evidencia se exigirá al liquidar."

  Escenario: Se registra el abastecimiento con las dos fechas y el modo de captura
    Dado un hecho ocurrido el "2026-09-25 14:20" y capturado en el dispositivo el "2026-09-25 14:22"
    Cuando el motorista registra "12.0" galones por "L 1,560.00" con odómetro "84,730" km, estación "Uno Zambrano", folio de vale "VC-01201" y fotografía del comprobante
    Entonces el sistema guarda el evento con fecha del hecho "25/09/2026 14:20", fecha de captura "25/09/2026 14:22" y modo de captura "captura en campo sin conectividad"
    Y numera el evento con la secuencia monotónica del dispositivo, no con el reloj
    Y la asignación de fondo pasa a estado "CONSUMIDA"

  Escenario: El consumo parcial no exige agotar la asignación
    Dado una asignación "ASG-2026-00812" por "L 4,800.00"
    Cuando el motorista registra un consumo de "L 1,560.00"
    Entonces la asignación queda en estado "CONSUMIDA" con "L 3,240.00" pendientes de consumo o devolución
    Y el sistema no exige agotar el saldo

  Escenario: La estación no está en el catálogo
    Cuando el motorista registra la estación como texto libre "Gasolinera El Rancho, salida de Comayagua"
    Entonces el sistema acepta el registro
    Y marca la estación como "fuera de catálogo" para su normalización posterior

  Escenario: Al sincronizar se envía el diario completo, no el estado
    Dado 7 eventos capturados sin conectividad
    Cuando el dispositivo recupera señal
    Entonces envía las 7 transiciones y eventos en orden de secuencia del dispositivo
    Y el servidor descarta duplicados por identificador y retiene los que esperan predecesor
    Y registra el desfase medido entre el reloj del dispositivo y el del servidor

  Escenario: Un conflicto de sincronización nunca se resuelve sobrescribiendo
    Dado un evento del servidor y un evento del dispositivo que se contradicen sobre el mismo abastecimiento
    Cuando el dispositivo sincroniza
    Entonces el sistema no sobrescribe ninguna de las dos versiones
    Y abre un conflicto en la cola de resolución del Jefe de Transporte con ambas versiones lado a lado, campo por campo

  Escenario: Digitación diferida desde la hoja de bitácora en papel
    Dado que el motorista llenó la hoja de bitácora impresa con folio "BIT-2026-00344"
    Cuando el Encargado de Delegación digita el abastecimiento el "2026-09-29"
    Entonces el sistema exige fecha del hecho tomada del papel, constancia de quién digitó y adjunto del original escaneado o fotografiado
    Y el expediente muestra visiblemente la diferencia entre fecha del hecho "25/09/2026" y fecha de captura "29/09/2026"
```

## Fuera de alcance

- La declaración de la fuente del combustible — es [HU-083](HU-083-declarar-la-fuente-de-todo-abastecimiento.md)
- La validación de coherencia del odómetro — es [HU-084](HU-084-coherencia-del-odometro-y-kilometraje-acumulado.md)
- La ausencia de comprobante y su descargo alternativo — es [HU-087](HU-087-registrar-consumo-sin-comprobante-y-unicidad.md)
- El registro del paso por caseta — es [HU-085](HU-085-registrar-el-paso-por-caseta-y-marcar-discrepancia.md)
- La resolución de los conflictos de sincronización: pertenece a M-16

## Notas y pendientes

- `[C]` Formatos en papel vigentes de la hoja de bitácora y del vale, para lograr la paridad exacta que exige [`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) — insumo **#2**
- `[C]` `desfase_maximo_sin_justificacion` entre fecha del hecho y fecha de captura — insumo **#1**
- `[C]` `comprobante_obligatorio_por_monto`: umbral a partir del cual el comprobante es exigible — insumo **#1**, con Auditoría Interna
- `[V]` Que más de 2 millones de personas del área rural no tienen acceso a internet — INE, EPHPM julio 2025, vía [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
