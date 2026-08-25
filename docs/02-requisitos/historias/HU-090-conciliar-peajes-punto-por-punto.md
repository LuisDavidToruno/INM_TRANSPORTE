# HU-090 — Conciliar peajes punto por punto y correlacionarlos con la ruta autorizada

| Campo | Valor |
|---|---|
| **Módulo** | M-18 Peajes · M-13 Liquidación y Cierre |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el catálogo de distancias entre puntos y la velocidad media máxima por tipo de vehículo para evaluar la coherencia temporal (insumo #1), si la institución tiene tags y si el concesionario emite estado de cuenta empresarial (insumo #24), y qué objeto del gasto se usa para peajes (`NRM-10` §8) |

## Historia

**Como** Jefe de Transporte
**quiero** conciliar el estimado contra lo pagado punto por punto, con causa tipificada de cada diferencia, y que el sistema detecte solo los pasos incompatibles con la ruta autorizada
**para** que un peaje de Yojoa en una misión autorizada a Choluteca sea un hallazgo que el sistema produce, y no algo que dependa de que alguien lo note

## Contexto

*Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y el sistema debe producirlo solo.* Esto es exactamente lo que busca el auditor: correlación, no comprobantes archivados `[V]`.

El orden Zambrano → Siguatepeque → Yojoa es el sentido Tegucigalpa → San Pedro Sula `[V]`. Una secuencia imposible o un intervalo inviable entre dos casetas es una alerta.

La evaluación se hace siempre contra el **alcance vigente a la fecha de cada hecho**: un reordenamiento de destinos justificado y autorizado no es desviación.

## Reglas que la gobiernan

- [RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) — Coherencia geográfica y temporal de la secuencia de casetas
- [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) — El sobrecosto se registra tipificado y no se imputa al motorista
- [RN-92](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) — Las discrepancias se agregan al expediente de reclamo por punto, clase y período
- [RN-77](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md) — La desviación se evalúa contra el alcance vigente a la fecha del hecho
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — El cambio de tarifa entre aprobación y ejecución es causa tipificada, no anomalía
- [RN-85](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) — Cada discrepancia lleva marcada su fuerza probatoria
- [RN-95](../../01-negocio/reglas/RN-95-conciliacion-contra-fuentes-externas.md) — Conciliación mensual contra estado de cuenta del tag

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Eje de la historia
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — El ticket faltante advierte, no bloquea
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — Pasos adicionales por extensión autorizada
- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — El estado de cuenta del tag llega después del cierre

## Criterios de aceptación

```gherkin
# language: es
Característica: Conciliación de peajes de la misión

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "RETORNADA", autorizada a "San Pedro Sula"
    Y un estimado de 6 pasos por "L 22.00" cada uno, total "L 132.00"
    Y un vehículo "TR-0045" con categoría "Liviano/Turismo"

  Escenario: Se detecta un paso incompatible con la ruta autorizada
    Dado una Orden de Misión "OM-2026-0518" autorizada a "Choluteca"
    Y un paso registrado por el punto "Yojoa" el "2026-09-25"
    Cuando el Jefe de Transporte ejecuta la conciliación de peajes
    Entonces el sistema genera un hallazgo de tipo "PASO_INCOMPATIBLE_CON_RUTA"
    Y muestra "Paso por Yojoa (Cortés) el 25/09/2026 en una misión autorizada a Choluteca. Justifique o quedará como hallazgo."

  Escenario: Se detecta una secuencia de casetas temporalmente imposible
    Dado un paso por "Zambrano" a las "09:15" y un paso por "Yojoa" a las "09:50" del "2026-09-25"
    Cuando el Jefe de Transporte ejecuta la conciliación de peajes
    Entonces el sistema marca la secuencia como incoherente
    Y muestra "Zambrano 09:15 y Yojoa 09:50: 145 km en 35 minutos. Intervalo inviable para el tipo Pickup."

  Escenario: Se rechaza cerrar la conciliación con una diferencia sin causa tipificada
    Dado un pagado de "L 200.00" contra un estimado de "L 132.00"
    Cuando el Jefe de Transporte intenta cerrar la conciliación de peajes sin tipificar la diferencia de "L 68.00"
    Entonces el sistema rechaza la acción
    Y muestra "Tipifique la diferencia de L 68.00: cambio de tarifa entre aprobación y ejecución, ruta distinta a la autorizada, paso adicional no previsto, cobro en categoría equivocada, o peaje pagado sin paso registrado."

  Escenario: La discrepancia de clasificación alimenta el expediente de reclamo
    Dado 3 pasos con cobro en "Vehículo de 2 Ejes" sobre un vehículo "Liviano/Turismo"
    Cuando el Jefe de Transporte ejecuta la conciliación de peajes
    Entonces el sistema agrega las 3 discrepancias al expediente de reclamo del punto, la clase de vehículo y el período
    Y muestra "3 discrepancias de clasificación por L 204.00 agregadas al expediente de reclamo del punto Zambrano, período septiembre 2026."

  Escenario: El sobrecosto no se imputa al motorista al liquidar
    Cuando el sistema registra el sobrecosto de "L 204.00" por clasificación
    Entonces no se genera obligación de reintegro a cargo de "Wilmer Cáceres"
    Y el sobrecosto figura tipificado como "sobrecosto por clasificación"

  Escenario: Una extensión autorizada no produce desviación
    Dado un destino adicional a "Puerto Cortés" autorizado el "2026-09-26"
    Y 2 pasos adicionales por "Yojoa" el "2026-09-26"
    Cuando el Jefe de Transporte ejecuta la conciliación de peajes
    Entonces los 2 pasos se evalúan contra el alcance vigente al "2026-09-26"
    Y no producen hallazgo

  Escenario: El ticket faltante advierte y no bloquea la liquidación
    Dado un paso de "L 22.00" sin fotografía del ticket, con causa "la caseta no entregó ticket"
    Cuando el Jefe de Transporte ejecuta la conciliación de peajes
    Entonces la liquidación no se bloquea
    Y la discrepancia queda marcada con fuerza probatoria "solo declarada"
    Y muestra "1 paso sin ticket. Cuenta para el criterio de hallazgo al cerrar."

  Escenario: Cada discrepancia lleva su fuerza probatoria visible
    Cuando el Jefe de Transporte abre la conciliación de peajes
    Entonces cada discrepancia muestra si está respaldada con ticket fotografiado, con estado de cuenta, o solo declarada
    Y el auditor puede leer el peso del expediente sin abrir cada adjunto

  Escenario: Un punto con tarifa no verificada no produce discrepancia
    Dado un paso por "San Manuel", cuya tarifa está marcada como "no verificada"
    Cuando el Jefe de Transporte ejecuta la conciliación de peajes
    Entonces el resultado de ese punto se presenta como "no concluyente"
    Y no se agrega al expediente de reclamo
```

## Fuera de alcance

- El registro del paso en caseta — es [HU-085](HU-085-registrar-el-paso-por-caseta-y-marcar-discrepancia.md)
- La gestión del reclamo ante la SAPP: SIGTI arma el expediente; la presentación es de la institución
- La conciliación de combustible — es [HU-088](HU-088-conciliar-galonaje-contra-kilometraje.md)
- El componente de mapas y el cálculo de distancias entre puntos: se reutiliza el de ARGOS ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))

## Notas y pendientes

- ⚠️ **Hallazgo `HB3-02` incorporado: el reclamo de peaje pendiente ante la SAPP ya no condiciona el cierre de la misión.** El bloqueo de [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) recae sobre el cierre de **la discrepancia**, que es objeto propio de M-18. La misión cierra; el reclamo sigue su curso y su resultado económico se registra después por asiento
- 🔴 `[C]` **bloqueante — tarifa efectivamente vigente** (insumo **#21**). Ver [HU-086](HU-086-no-emitir-discrepancia-sobre-tarifa-no-verificada.md)
- `[C]` `velocidad_media_maxima_por_tipo_vehiculo` y catálogo de distancias entre puntos, para evaluar la coherencia temporal — insumo **#1**
- `[C]` Si la institución tiene tags CoviPass y si COVI-H emite estado de cuenta empresarial — insumo **#24**
- `[C]` ¿Qué objeto del gasto se usa para peajes? ¿El peaje se financia con el viático —y entonces es de ARGOS— o es gasto de misión separado? — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §8
- `[V]` Que el auditor busca correlación entre consumo, kilometraje y misión autorizada — [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
