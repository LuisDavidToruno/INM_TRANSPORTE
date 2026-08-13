# RN-85 — La ausencia de comprobante se registra con causa tipificada y calificación de suficiencia probatoria, y admite descargo alternativo con folio

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-13, M-15, M-18 |
| **Origen** | Caso especial [CE-25](../../02-requisitos/casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) · Normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-03](../normativa/NRM-03-viaticos.md), [NRM-08](../normativa/NRM-08-firma-electronica.md) |
| **Verificación** | `[P]` la exigencia de documentación de respaldo del gasto — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[C]` si la institución admite constancia o declaración jurada como descargo, y con qué tope — insumo #1 y consulta a Auditoría Interna |
| **Tipo** | Bloqueo duro sobre la causa + derivación sobre la suficiencia |
| **Configurable** | Sí — catálogo `causa_de_ausencia_de_comprobante`, `tope_descargo_alternativo`, `umbral_hallazgo_sin_descargo` |

## Enunciado

La ausencia de comprobante **debe** registrarse con **causa tipificada** de catálogo configurable, y **la causa determina el descargo exigible y la consecuencia**. *"No lo emiten"*, *"se deterioró"*, *"se extravió"* y *"no se solicitó"* **no son el mismo hecho**.

Todo consumo o gasto lleva una **calificación de suficiencia probatoria**, derivada de la evidencia efectivamente capturada —fotografía del surtidor, del odómetro, del ticket, ubicación, testigo, constancia— **independiente de la existencia del comprobante fiscal**. Esa calificación se presenta a quien liquida y a quien audita.

La **constancia de gasto sin comprobante** es documento oficial con **folio, QR, hash, firma del servidor y aval de un segundo**, vinculada al consumo, y se identifica como **descargo alternativo** en todo reporte y en todo total. **Nunca se suma con las facturas en un mismo renglón.**

## Justificación

[`RN-28`](RN-28-comprobacion-del-consumo-de-combustible.md) tipifica la **ausencia** en la liquidación, no su **causa**, y aplica un umbral único. Sin causa, una zona sin comercio formalizado y un descuido reciben el mismo tratamiento — y el motorista que hizo todo bien queda igual que el que perdió el papel.

La razón de fondo, escrita para que no se revierta en la primera revisión de seguridad: **bloquear la liquidación por un comprobante que el motorista no pudo conseguir no produce comprobantes. Produce comprobantes falsos, o produce que el consumo no se registre.** Las dos cosas son peores que un gasto sustentado con evidencia sustituta y marcado como tal.

La trazabilidad inmutable prevalece sobre la comodidad del usuario en los puntos críticos. **Esto no es comodidad**: es la diferencia entre un dato real y un dato inventado.

Y la calificación de suficiencia convierte en dato lo que hoy es criterio no registrado del liquidador. El auditor no evalúa presencia de papel: evalúa si el gasto está sustentado.

## Condiciones de aplicación

Aplica a todo consumo de combustible, paso por peaje y gasto en ruta sin comprobante fiscal.

**No aplica** al gasto que sí tiene comprobante, que rige por [`RN-28`](RN-28-comprobacion-del-consumo-de-combustible.md) y [`RN-84`](RN-84-unicidad-del-comprobante-en-la-institucion.md).

## Comportamiento esperado

1. Al registrar sin comprobante, el sistema exige la causa del catálogo y, según la causa, **pide la evidencia sustituta que corresponda**: fotografía del surtidor con la lectura, del odómetro, ubicación, identidad de un testigo.
2. La **calificación de suficiencia probatoria** se deriva de la evidencia capturada, con reglas explícitas y auditables. Se muestra al liquidador junto al monto.
3. La constancia se emite con folio del rango de la delegación, QR y hash del contenido ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md), [`RN-44`](RN-44-identificadores-y-folios-en-el-cliente.md)), firmada por el servidor y **avalada por un segundo** que no puede ser el liquidador ([`RN-01`](RN-01-segregacion-de-funciones.md)).
4. Todo reporte separa **gasto con comprobante fiscal** de **gasto con descargo alternativo**, con sus dos totales. Sumarlos en un renglón haría invisible justo lo que hay que vigilar.
5. Superado `umbral_hallazgo_sin_descargo` en monto o en proporción, la misión **cierra con hallazgo**.
6. **El costo no se traslada al servidor sin acto administrativo.** Si la institución decide que un gasto sin descargo lo asume el servidor, eso es una **responsabilidad determinada**: procedimiento, descargo del interesado, resolución y notificación. **No es un ajuste que hace el liquidador en la hoja** ([`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), [`RN-74`](RN-74-sin-atribucion-de-responsabilidad-en-campo.md)).

## Casos límite

- **`[C]` ¿La institución admite constancia o declaración jurada como descargo de combustible?** [NRM-03](../normativa/NRM-03-viaticos.md) lo prevé para viáticos, `[C]` su forma. **Sí, con tope de monto y aval de segunda persona**: el gasto real se descarga por vía legítima y queda marcado. **No**: hay que decirlo por escrito y aceptar que el motorista seguirá pagando de su bolsillo o consiguiendo papel. Se propone *sí, con tope*, consultado con Auditoría Interna antes de fijarlo.
- **`[C]` ¿Cuál es el umbral que fuerza cierre con hallazgo?** Muy bajo: casi toda misión rural cierra con hallazgo y el hallazgo pierde significado. Muy alto: el control no existe. Se propone fijarlo con Auditoría Interna sobre datos reales de tres meses de operación.
- **`[C]` ¿Se acepta comprobante a nombre del motorista?** [`RN-28`](RN-28-comprobacion-del-consumo-de-combustible.md) lo deja abierto. Si no se acepta, hay que decir qué se hace con los que ya existen.
- **Número de comprobante ilegible** en la fotografía. Causa *comprobante deteriorado*; queda fuera del control de unicidad de [`RN-84`](RN-84-unicidad-del-comprobante-en-la-institucion.md) con la calificación que le corresponda. No se transcribe un número que no se lee.
- **Estación que emite un papel sin valor fiscal.** Es evidencia sustituta, no comprobante. Se adjunta y eleva la calificación de suficiencia sin convertirse en factura.
- **Constancia usada de forma recurrente por la misma persona o la misma ruta.** Es el patrón que hay que vigilar: el reporte de descargos alternativos por persona, ruta y período existe para eso.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · [NRM-03](../normativa/NRM-03-viaticos.md) `[P]` · [NRM-08](../normativa/NRM-08-firma-electronica.md) `[P]` — sin firma electrónica certificada, el aval es interno
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md), [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md), [RN-84](RN-84-unicidad-del-comprobante-en-la-institucion.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)
- Casos especiales: [CE-25](../../02-requisitos/casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — candidatas `RN-C25a`, `RN-C25b`, `RN-C25c`, `RN-C25d`
- Insumos pendientes: #1 reglamento interno de uso de vehículos · tope y umbral con Auditoría Interna
