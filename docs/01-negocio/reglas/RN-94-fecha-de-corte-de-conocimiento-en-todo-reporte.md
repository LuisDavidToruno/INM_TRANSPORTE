# RN-94 — Todo reporte declara su fecha de corte de conocimiento y es reproducible a esa fecha

| Campo | Valor |
|---|---|
| **Módulos** | M-14, M-13, M-09, M-18, M-03 |
| **Origen** | Caso especial [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) · [orden-de-mision.md §7.5](../../03-arquitectura/estados/orden-de-mision.md) — **artefacto autoridad** · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de información confiable y verificable del control interno. `[I]` la fecha de corte como mecanismo: implicación de requerimiento del equipo, no articulado citable |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Todo reporte, indicador y exportación producidos por el sistema **deben** declarar dos fechas distintas y visibles en su encabezado:

- **Período del hecho** — el rango de fechas del hecho que el reporte cubre
- **Fecha de corte de conocimiento** — el momento hasta el cual se consideran los registros existentes, cualquiera sea la fecha del hecho a la que se refieran

El mismo reporte, con el mismo período y la misma fecha de corte, **debe** producir el mismo resultado hoy, dentro de un año y dentro de cinco. Un reporte que cambia de valor sin que cambie ninguno de sus dos parámetros **es un defecto**, no una actualización.

Los registros incorporados después de una fecha de corte — digitación diferida, asientos de hallazgo posterior, ajustes por conciliación externa — **se presentan como capa identificada**, nunca fundidos en el dato histórico.

## Justificación

[§7.5 de la máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) sostiene que una Orden de Misión `CERRADA` no se reabre, con el argumento de que la reapertura haría que *ningún reporte histórico fuera reproducible*. **La promesa es correcta y hoy nada la cumple.** No reabrir el expediente no sirve de nada si el reporte cambia igual: basta con que un asiento de hallazgo posterior ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)), una bitácora digitada tarde ([`RN-47`](RN-47-digitacion-diferida-desde-papel.md)) o un ajuste por conciliación externa ([`RN-95`](RN-95-conciliacion-contra-fuentes-externas.md)) entren al mismo período para que el reporte de julio ya no se pueda volver a producir en septiembre.

Y ese es exactamente el escenario del TSC: el auditor pide el reporte que la institución le entregó en su momento, y el reporte que sale ahora no coincide. Sin fecha de corte, la institución no puede explicar la diferencia; con fecha de corte, la explica en un renglón.

## Condiciones de aplicación

Aplica a **todo** reporte, indicador y exportación, operativos y de control interno, incluidos los que se muestran en pantalla sin imprimirse.

**No aplica** a las consultas de expediente individual, que muestran el estado corriente del registro y su diario completo — ahí el diario ya es la reproducibilidad.

**No aplica** a los tableros de operación en curso — dónde está cada vehículo hoy — cuya naturaleza es el ahora. Estos declaran, en su lugar, la **antigüedad del dato** ([`RN-76`](RN-76-estado-en-ruta-declarado-por-el-motorista.md)).

## Comportamiento esperado

1. Toda emisión de reporte captura la fecha de corte por defecto en el **momento de la emisión**, y el usuario puede fijarla en una fecha anterior para reproducir un reporte histórico.
2. El encabezado impreso y el archivo exportado llevan: identificador del reporte, período del hecho, fecha de corte de conocimiento, parámetros usados con su vigencia, quién lo emitió y cuándo. Con folio y QR verificable cuando el reporte sale de la institución ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md)).
3. Cuando el reporte contiene registros cuya **fecha de captura** es posterior a una fecha de corte anterior ya emitida, el sistema lo señala: *"Incluye N registros incorporados después del corte del \<fecha\>"*, con el detalle disponible.
4. Los **ajustes** por hallazgo posterior se imputan al **período corriente** con referencia al período afectado, y se presentan como línea de ajuste identificada. **No se recalculan los históricos ya publicados** para dejarlos correctos.
5. El sistema conserva el registro de cada emisión de reporte hacia afuera de la institución — a quién, cuándo, con qué corte — para poder reproducir exactamente lo que se entregó.
6. Los indicadores derivados de un reporte heredan su fecha de corte. Un indicador sin corte declarado no se publica.

## Casos límite

- **Reporte pedido "al día de hoy" sobre un período cerrado.** Es legítimo: mismo período, corte distinto. El sistema lo produce y muestra la diferencia contra el último corte emitido de ese período, no la esconde.
- **Registro con fecha del hecho dentro del período pero capturado después del corte.** No entra en el reporte reproducido a esa fecha de corte. Entra en cualquier reporte con corte posterior. Es la definición misma del corte, y es la que hace que el reporte de julio siga siendo el de julio.
- **Corrección de un dato mal digitado** — no un hecho nuevo, un error de captura. Sigue el camino del asiento reverso ([`RN-04`](RN-04-anulacion-como-asiento-reverso.md)); el reporte anterior al corte conserva el valor errado, y ahí es donde el ajuste identificado hace su trabajo.
- **Reporte emitido en operación desconectada** desde una delegación con datos incompletos. Declara además el **estado de sincronización** de la delegación y la antigüedad del espejo ([`RN-49`](RN-49-reconciliacion-periodica-del-espejo.md)); sin eso, un reporte offline se presenta como completo sin serlo.
- **Cambio de un parámetro con vigencia retroactiva** posterior al corte. El reporte reproducido usa la tabla vigente a la fecha del hecho **según se conocía al corte**; el efecto del cambio aparece como ajuste ([`RN-42`](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)), no reescribiendo el reporte.

## Trazabilidad

- Autoridad: [orden-de-mision.md §7.5 y §8.2](../../03-arquitectura/estados/orden-de-mision.md) — inmutabilidad del expediente terminal
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-05](RN-05-registro-cerrado-no-se-edita.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-95](RN-95-conciliacion-contra-fuentes-externas.md), [RN-96](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)
- Casos especiales: [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — candidata `RN-C28b` · [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)
- Actores: ACT-08, ACT-12 emiten y entregan · ACT-01 configura
