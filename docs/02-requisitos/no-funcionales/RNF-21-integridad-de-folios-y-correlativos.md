# RNF-21 — Ningún folio se duplica ni se recicla, aunque se emita en una delegación sin red

| Campo | Valor |
|---|---|
| **Categoría** | Auditoría / Disponibilidad |
| **Prioridad** | Crítico |
| **Origen** | [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md); [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) (emisión anticipada de documentos antes de salir a zona sin cobertura) |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack (Sprint 2).** Identificadores generados en el cliente y rangos de folio pre-asignados son decisiones de modelo, no de implementación |

## Enunciado

El sistema emite documentos con **folio** —orden de misión, salvoconducto, vale de combustible, acta— y esos folios se emiten **también sin conectividad**, desde delegaciones que no se ven entre sí durante días.

El folio es el eje de la trazabilidad documental exigida por [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md): cada eslabón de la cadena se cita por su folio. Por tanto:

1. **Ningún folio se duplica** a nivel institución, sin importar dónde ni cuándo se emitió.
2. **Ningún folio se recicla**, ni siquiera el de un documento anulado. Un folio anulado conserva su asiento reverso y su motivo.
3. **Todo hueco en la numeración se explica.** Un salto sin explicación es exactamente lo que el auditor busca.
4. Los identificadores internos se **generan en el cliente**, para que un registro tenga identidad desde el momento del hecho y no desde el momento en que alcanza el servidor.

Estas dos cosas son distintas y conviene no confundirlas: el **identificador interno** es técnico, opaco y se genera en el dispositivo; el **folio** es el número que va impreso, que la institución cita en su descargo, y que tiene que ser secuencial y explicable.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Folios duplicados a nivel institución | **0** |
| Folios reciclados tras una anulación | **0** |
| Colisiones de identificador interno entre dispositivos | **0** |
| Huecos en la numeración sin explicación registrada | **0.** Todo hueco corresponde a un folio anulado, extraviado con acta, o a un rango asignado y no consumido |
| Reporte de control de folios por delegación y por tipo de documento | Disponible en línea: emitidos, anulados, extraviados, huecos, rangos vigentes y saldo disponible |
| Documentos emitibles sin conectividad por una delegación | Los del rango pre-asignado a esa delegación |
| Tamaño del rango pre-asignado | Configurable, con alerta cuando queden < 20 % `[C]` insumo #34 |
| Agotamiento del rango de una delegación sin aviso previo | **0.** El aviso aparece con antelación suficiente para reponer, incluso sin red — la reposición sí requiere red y hay que decirlo |
| Formato del correlativo institucional del vehículo | Configurable: único por institución o compuesto por delegación `[C]` insumo #34. **No se decide por inferencia** |
| Unicidad del comprobante de consumo de combustible a nivel institución | **Verificada al registrar**, no al conciliar. Un mismo recibo sosteniendo dos consumos en dos delegaciones se detecta en el momento, no ocho meses después |
| Detección de un comprobante duplicado cuando ambos registros se capturaron sin red | Al sincronizar el segundo: **conflicto a cola de resolución humana**, nunca aceptación silenciosa del segundo ni descarte silencioso ([`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)) |
| Numeración del talonario preimpreso cruzada con la del sistema | Registrada como campo propio si el talonario trae folio `[C]` insumo #46 |
| Reimpresión de un documento que genere un folio nuevo | **0.** La reimpresión conserva el folio y se marca como reimpresión ([`RNF-11`](RNF-11-formatos-oficiales-imprimibles-y-verificables.md)) |

## Cómo se verifica

1. **Prueba de emisión distribuida** — la prueba que decide:
   - Cinco dispositivos, cada uno con el rango de una delegación distinta, **sin conexión entre sí**.
   - Cada uno emite 100 documentos de tres tipos, incluidas 10 anulaciones.
   - Se sincronizan todos, en orden desordenado y con una sincronización interrumpida a la mitad.
   - Se verifica: 0 duplicados, 0 colisiones, 0 folios reciclados, y que el reporte de huecos coincide exactamente con las 50 anulaciones.
2. **Prueba del comprobante duplicado**: dos delegaciones registran, sin red, un consumo con el mismo número de comprobante del mismo proveedor. Al sincronizar, ambos deben llegar, ninguno debe perderse, y el conflicto debe aparecer en la cola con las dos versiones y su contexto ([`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md)).
3. **Prueba de agotamiento de rango**: se consume el 85 % del rango de una delegación y se verifica el aviso; se agota completamente estando sin red y se verifica que el mensaje explica que la reposición requiere conexión y qué hacer mientras tanto —incluida la vía en papel, que es la que hoy existe.
4. **Prueba de anulación**: se anula un documento emitido y se verifica que su folio no vuelve a ofrecerse, que aparece en el reporte de control como anulado con motivo y autor, y que el hueco queda explicado.
5. **Prueba de reimpresión**: se reimprime tres veces el mismo documento y se verifica que el folio no cambia y que las tres reimpresiones quedan registradas.
6. **Prueba de volumen de identificadores**: se generan 1,000,000 de identificadores en 10 dispositivos y se verifica ausencia total de colisiones.

## Consecuencia de no cumplirlo

Un folio duplicado es un hallazgo directo de auditoría, y de los caros: dos documentos oficiales con el mismo número significa que uno de los dos —o los dos— no acreditan lo que dicen acreditar. La institución tiene que explicar cuál vale, y no tiene con qué.

Un folio reciclado es peor, porque es indistinguible de una alteración deliberada: el número que el auditor tiene en su expediente ahora corresponde a otro documento.

Y un comprobante duplicado que solo se detecta al conciliar es, exactamente, la fuga de combustible clásica: un recibo sostiene dos consumos en dos delegaciones distintas y nadie lo nota hasta ocho meses después, cuando ya no hay a quién preguntarle. Los analistas del Bloque 2 lo señalaron como una de las reglas candidatas de mayor retorno del proyecto.

## Trazabilidad

- Módulos: M-15, M-16, M-09, M-07
- Reglas: [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md), [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md), [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)
- Normativa: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- Casos especiales: [`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md), [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md), [`CE-20`](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md)
- Requisitos relacionados: [`RNF-03`](RNF-03-operacion-sin-conectividad.md), [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-11`](RNF-11-formatos-oficiales-imprimibles-y-verificables.md)
- Insumos: #34 (formato del correlativo institucional), #46 (folio del talonario preimpreso)
