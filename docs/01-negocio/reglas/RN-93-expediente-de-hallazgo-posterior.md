# RN-93 — El hallazgo posterior es un expediente con ciclo propio; ni su apertura ni su resolución alteran el estado ni los datos del objeto vinculado

| Campo | Valor |
|---|---|
| **Módulos** | M-14, M-12, M-13 |
| **Origen** | Caso especial [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) · [orden-de-mision.md §7.5 y §8.2](../../03-arquitectura/estados/orden-de-mision.md) — **artefacto autoridad** · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de inmutabilidad de los registros y de asiento reverso — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` la entidad expediente de hallazgo posterior: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `tipo_de_hallazgo_posterior` |

## Enunciado

El **expediente de hallazgo posterior** es una **entidad con ciclo propio**. Puede vincular **cero, una o varias** Órdenes de Misión en estado terminal, un vehículo, un motorista o un período.

**Ni su apertura ni su resolución alteran el estado ni los datos del objeto vinculado.** Una Orden de Misión `CERRADA` **no se reabre**, ni por auditoría.

Todo efecto económico del hallazgo se materializa como **asiento reverso** con referencia al asiento concreto revertido, valor anterior, valor nuevo, motivo tipificado, fundamento, autor y autorizador, imputado al **período corriente** con referencia al período afectado.

La **antigüedad del hallazgo se cuenta desde el hecho original**, no desde el descubrimiento. **Fecha del hecho y fecha del descubrimiento son campos distintos y ambos obligatorios.**

## Justificación

[§7.5 de la máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) describe este mecanismo como **efecto de la inmutabilidad**, pero **ninguna de las 54 reglas originales lo enuncia**, y ninguna contempla el **hallazgo sin misión vinculable** — el paso por caseta de un domingo, el consumo de un vehículo que ese día no tenía orden.

Basta con que la reapertura de un expediente cerrado exista para que se use, y basta con que se use una vez para que ningún reporte histórico vuelva a ser reproducible ([`RN-94`](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)). El expediente de hallazgo posterior es la salida que permite corregir el efecto económico sin destruir la reproducibilidad.

Contar la antigüedad desde el hecho y no desde el descubrimiento evita el incentivo perverso más obvio: descubrir tarde para que el indicador se vea mejor.

## Condiciones de aplicación

Aplica a todo hallazgo descubierto después del cierre del expediente al que afecta.

Aplica al hallazgo **sin objeto vinculable**: un consumo o un paso por caseta que no corresponde a ninguna misión. El vehículo y el período bastan como vínculo.

**No aplica** al hallazgo detectado **antes** del cierre, que se resuelve en la liquidación y en el cierre con hallazgo (`T-22`).

## Comportamiento esperado

1. El expediente registra: quién lo descubrió, **cómo**, cuándo, **contra qué fuente**, con el documento externo adjunto ([`RN-95`](RN-95-conciliacion-contra-fuentes-externas.md)).
2. El asiento reverso verifica la segregación contra la **identidad del autor original**: quien revierte no puede ser quien registró ([`RN-01`](RN-01-segregacion-de-funciones.md), [`RN-04`](RN-04-anulacion-como-asiento-reverso.md)).
3. **No se recalculan los históricos ya publicados** para dejarlos correctos. Se ajusta el período corriente y **se muestra el ajuste** como capa identificada.
4. **No se reemite un documento con el mismo folio y contenido distinto.** El duplicado se anula con referencia cruzada, y el sustituto lleva folio nuevo — **con ambos conservados**.
5. Los **indicadores del vehículo antes y después del ajuste** se pueden mostrar, con el ajuste identificado y el período al que se imputó.
6. **El expediente no se cierra sin resolución.** Los abiertos al cierre del ejercicio integran el saldo de apertura del siguiente, con su antigüedad ([`RN-97`](RN-97-saldo-de-apertura-de-control-interno.md)).
7. El **paquete de evidencia de la misión tal como cerró** se conserva con su cadena sellada y sus hashes verificables: debe poder entregarse idéntico al que se pudo exportar el día del cierre.

## Casos límite

- **Hallazgo sin misión vinculable.** Vincula cero misiones. El vehículo y la fecha del hecho son suficientes, y la ausencia de misión **es el hallazgo**: el vehículo circuló sin amparo ([`RN-59`](RN-59-todo-uso-se-ampara-en-orden-de-mision.md)).
- **Hallazgo que afecta varias misiones** — un comprobante duplicado en dos delegaciones ([`RN-84`](RN-84-unicidad-del-comprobante-en-la-institucion.md)). Un expediente, varias misiones vinculadas, un asiento por cada efecto económico.
- **Hallazgo sobre una misión de un ejercicio fiscal cerrado.** Se abre igual; el asiento se imputa al ejercicio corriente con referencia al anterior ([`RN-96`](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)). `[C]` el criterio final de imputación entre ejercicios depende de SIAFI.
- **Auditoría que pide reabrir el expediente.** No se reabre. Lo que se entrega es el paquete sellado tal como cerró **más** el expediente de hallazgo posterior con su asiento. Es más información, no menos.
- **Hallazgo que resulta ser un error del propio descubridor.** El expediente se resuelve como **sin efecto**, con su fundamento. Se cierra, no se borra.

## Trazabilidad

- Autoridad: [orden-de-mision.md §7.5 y §8.2](../../03-arquitectura/estados/orden-de-mision.md) · `BD-06`
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-05](RN-05-registro-cerrado-no-se-edita.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md), [RN-84](RN-84-unicidad-del-comprobante-en-la-institucion.md), [RN-92](RN-92-reclamo-por-discrepancia-de-peaje.md), [RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md), [RN-95](RN-95-conciliacion-contra-fuentes-externas.md), [RN-97](RN-97-saldo-de-apertura-de-control-interno.md)
- Casos especiales: [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — candidata `RN-C28a`
