# RN-95 — El sistema concilia periódicamente contra fuentes externas, y cada diferencia abre expediente de hallazgo posterior

| Campo | Valor |
|---|---|
| **Módulos** | M-14, M-09, M-18, M-12, M-20 |
| **Origen** | Caso especial [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-14 |
| **Verificación** | `[P]` la exigencia de conciliación periódica de registros — [NRM-01](../normativa/NRM-01-control-interno-tsc.md) marca `[P]` el catálogo NOGECI donde vive V-14 |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo de fuentes y periodicidad por fuente |

## Enunciado

El sistema **debe** ejecutar **conciliaciones periódicas contra fuentes externas**, al menos:

| Fuente | Contra qué se concilia |
|---|---|
| Estado de cuenta del proveedor de combustible | Consumos registrados ([`RN-28`](RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-83`](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)) |
| Estado de cuenta de peaje o de tag | Pasos por caseta registrados ([`RN-34`](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md)) |
| Notificaciones de infracción de tránsito | Bitácora y conductor de la fecha ([`RN-66`](RN-66-imputacion-externa-por-jerarquia-de-anclas.md)) |
| Dictámenes, resoluciones y actas de autoridad | Expedientes de M-12 |

**Cada diferencia abre expediente de hallazgo posterior de forma automática** ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)), en ambos sentidos: lo que la fuente externa tiene y el sistema no, y lo que el sistema tiene y la fuente externa no.

## Justificación

[`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) concilia **hacia adentro**: galones contra kilómetros, ambos registrados por nosotros. **Nada concilia contra una fuente externa** — y de ahí vinieron los tres casos que originaron [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md): el comprobante duplicado que apareció en el estado de cuenta del proveedor, el paso por caseta de un domingo sin misión, y las multas notificadas meses después.

Una conciliación que solo compara nuestros datos con nuestros datos verifica coherencia interna, no veracidad. **Un registro completo y coherente puede ser completamente falso**, y solo la fuente externa lo revela.

TSC-NOGECI V-14 exige conciliación periódica de registros `[P]`. Sin conciliación externa, la institución cumple la letra con el cruce interno y deja abierta la puerta por la que entran los hallazgos que después le cuestan.

## Condiciones de aplicación

Aplica a toda fuente externa disponible en la institución.

**No aplica** cuando la fuente no existe —una institución sin tag de peaje no tiene estado de cuenta que conciliar—; en ese caso la fuente se declara **no disponible**, que es distinto de conciliada.

## Comportamiento esperado

1. Cada fuente se registra en el catálogo con: emisor, periodicidad, formato, responsable de la carga y **fecha de la última conciliación ejecutada**.
2. La conciliación produce tres listas: **coincidentes**, **solo en la fuente externa**, **solo en SIGTI**. Las dos últimas abren expediente.
3. La conciliación **cruza el alcance de datos**: dos delegaciones no se ven entre sí, pero un comprobante duplicado entre ellas sí se detecta ([`RN-84`](RN-84-unicidad-del-comprobante-en-la-institucion.md)).
4. Toda imputación externa se resuelve al vehículo por la **jerarquía de anclas a la fecha del hecho** ([`RN-66`](RN-66-imputacion-externa-por-jerarquia-de-anclas.md)); lo que no se resuelve queda **no resuelto con responsable y plazo**, nunca asignado por parecido.
5. El **retraso** de una conciliación es dato visible: *"Estado de cuenta de combustible — última conciliación hace 97 días"*. Una fuente sin conciliar durante meses es en sí misma una observación de control interno.
6. Los resultados se reportan con **fecha de corte de conocimiento** ([`RN-94`](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)) y con la identificación del archivo o documento fuente usado.

## Casos límite

- **Fuente que llega en papel.** Se digitaliza y se concilia igual, con constancia de quién la digitó. El formato no exime de conciliar.
- **Diferencia por desfase temporal** — el consumo del 31 aparece en el estado de cuenta del mes siguiente. Se concilia por **fecha del hecho**, no por período del estado de cuenta ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
- **Línea del estado de cuenta que no corresponde a ningún vehículo de la flota.** Expediente de hallazgo posterior sin objeto vinculable ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)); puede ser un error del proveedor y puede no serlo.
- **Consumo registrado por el motorista que no aparece en el estado de cuenta.** También abre expediente: puede ser un comprobante falso, o una estación que no reportó. La conciliación no presume cuál.
- **Volumen alto de diferencias en la primera conciliación** tras el despliegue. Se admite un período de calibración declarado, con las diferencias agrupadas por causa. Lo que no se admite es apagar la conciliación hasta que "los datos estén limpios".
- **`[C]` Contratos de integración con los proveedores.** Insumos #16 y #17 cubren ARGOS y Talento Humano; los proveedores de combustible y peaje no tienen insumo asociado y hay que abrirlo.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` — TSC-NOGECI V-14
- Reglas relacionadas: [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md), [RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md), [RN-84](RN-84-unicidad-del-comprobante-en-la-institucion.md), [RN-92](RN-92-reclamo-por-discrepancia-de-peaje.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)
- Casos especiales: [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — candidata `RN-C28c`
- Insumos pendientes: contratos con proveedores de combustible y de peaje — **insumo nuevo a registrar**
