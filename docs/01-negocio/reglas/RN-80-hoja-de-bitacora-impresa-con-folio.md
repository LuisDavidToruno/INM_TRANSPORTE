# RN-80 — El despacho emite la hoja de bitácora en papel, con folio, QR y paridad exacta con la pantalla de digitación

| Campo | Valor |
|---|---|
| **Módulos** | M-15, M-08, M-16, M-07 |
| **Origen** | Caso especial [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) · Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md) · Premisa rectora 4 |
| **Verificación** | `[V]` la falta de conectividad en amplias zonas — [NRM-09](../normativa/NRM-09-realidad-operativa.md), INE EPHPM julio 2025. `[C]` los formatos en papel vigentes de la institución, insumo #2 |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — la plantilla del formato; **no** la obligación de emitirla |

## Enunciado

Al despachar, el sistema **debe** emitir la **hoja de bitácora en papel** con:

1. **Folio del rango de la delegación** ([`RN-44`](RN-44-identificadores-y-folios-en-el-cliente.md)) y **QR verificable**
2. **Encabezado prellenado**: institución, dependencia, correlativo del vehículo, motorista, ventana, destinos autorizados y odómetro de salida
3. **Espacios de captura** para cada dato que la pantalla exige: kilometrajes, paradas, arribos, consumos, eventos y observaciones
4. **Espacio de firma y sello**

La hoja impresa y la pantalla de digitación **deben** tener **paridad exacta de campos**: ningún dato que la pantalla exija puede faltar en el papel, y ningún campo del papel puede quedar sin destino en el sistema.

## Justificación

Más de 2 millones de personas del área rural hondureña no tienen acceso a internet. En esas zonas el motorista **va a llenar papel**, con o sin sistema. La pregunta no es si eso ocurre: es si el papel que llena es el que el sistema necesita.

Cuando la hoja de papel y la pantalla no coinciden, la digitación exige datos que el original no consigna. Y entonces pasan dos cosas, ambas malas: o el digitador **inventa** el dato faltante, o **deja el registro incompleto** y la misión no liquida. La primera es un dato falso en un expediente de auditoría; la segunda es una misión atascada.

La paridad exacta es lo que convierte al papel de un obstáculo en un instrumento del sistema. Y el folio con QR es lo que permite, meses después, atar la hoja escaneada al expediente electrónico sin ambigüedad.

## Condiciones de aplicación

Aplica a todo despacho, no solo a los de destino rural: la señal se pierde donde no se espera y una hoja emitida y no usada no cuesta nada.

**No aplica** a la Orden de Misión impresa ni al salvoconducto, que son documentos distintos con su propia regla ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md), [`RN-91`](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)).

## Comportamiento esperado

1. La hoja se genera en el despacho junto con el resto del paquete impreso y se entrega con acuse.
2. Al digitar, el sistema exige el **folio de la hoja** y adjunta la **fotografía o escaneo del original**, con la firma del motorista — o la **constancia de que no firmó**, declarada como observación y **no ocultada**.
3. Los **campos no consignados en el original** se declaran como tales. **No se rellenan** con valores derivados, promedios ni supuestos.
4. La digitación deja constancia de **quién digitó y cuándo**, y de **quién es el autor del hecho**, como dos personas distintas y ambas identificadas ([`RN-47`](RN-47-digitacion-diferida-desde-papel.md)).
5. Cada registro lleva **fecha del hecho y fecha de captura**, con el desfase visible y su **motivo tipificado** ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
6. Los conflictos entre lo digitado y lo ya sincronizado van a **cola de resolución humana** con **ambas versiones conservadas** ([`RN-45`](RN-45-cero-sobrescritura-silenciosa.md)).

## Casos límite

- **`[C]` Talonario preimpreso de la institución.** Si la institución conserva talonarios con folio propio, la digitación exige **ambos folios** —el del talonario y el del sistema— y quedan cruzados en el expediente. Insumo #2, pendiente D-3 de [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md).
- **`[C]` ¿Puede digitar quien después liquida?** [`RN-47`](RN-47-digitacion-diferida-desde-papel.md) lo deja abierto con advertencia registrada, y en una delegación de tres personas la misma persona digita y liquida. **No se resuelve aquí**: es materia de [`actores-y-roles.md`](../actores-y-roles.md), autoridad sobre incompatibilidades. Pregunta abierta a Auditoría Interna, insumo #27. **Nota de hallazgo abierta.**
- **Hoja perdida.** El retorno ya está constatado y el vehículo operando ([`RN-79`](RN-79-el-retorno-constatado-libera-al-vehiculo.md)). La bitácora se reconstruye con lo que exista y lo que no se recupere se declara **perdido, no vacío**; la misión cierra con hallazgo.
- **Motorista que llena la hoja con datos distintos a los que ya capturó en el dispositivo.** Ambas versiones se conservan y el conflicto lo resuelve una persona. Ninguna gana por ser más reciente.
- **Formato en papel vigente de la institución distinto al propuesto.** El formato en papel es un documento de requisitos: se recorre campo por campo y cada casilla se resuelve como campo del sistema o como decisión explícita de no capturarla. Insumo #2.

## Trazabilidad

- Norma: [NRM-09](../normativa/NRM-09-realidad-operativa.md) `[V]` · Premisa rectora 4 y 5
- Autoridad de incompatibilidades: [actores-y-roles.md](../actores-y-roles.md)
- Reglas relacionadas: [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md), [RN-79](RN-79-el-retorno-constatado-libera-al-vehiculo.md), [RN-89](RN-89-kilometraje-acumulado-invariante-del-expediente.md)
- Casos especiales: [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — candidatas `RN-c:hoja-de-bitacora-impresa-con-folio`, `RN-c:correspondencia-de-folio-papel-sistema`
- Insumos pendientes: #2 formatos en papel vigentes · #27 incompatibilidad digita / liquida
