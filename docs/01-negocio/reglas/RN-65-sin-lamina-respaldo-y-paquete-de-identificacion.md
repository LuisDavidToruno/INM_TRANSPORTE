# RN-65 — El vehículo sin lámina se despacha con respaldo vigente en todo el rango y con paquete de identificación impreso y acusado

| Campo | Valor |
|---|---|
| **Módulos** | M-04, M-15, M-07, M-03 |
| **Origen** | Caso especial [CE-17](../../02-requisitos/casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) · Normas [NRM-02](../normativa/NRM-02-bienes-del-estado.md) y [NRM-06](../normativa/NRM-06-transito-y-licencias.md) · Premisa rectora 4 |
| **Verificación** | `[V]` que la ausencia de lámina no bloquea por sí sola — [`BD-03`](../../03-arquitectura/estados/orden-de-mision.md). `[I]` la exigencia de respaldo vigente y de paquete impreso: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `documento_respaldo_sin_lamina`; **no** la obligación de entregar el paquete |

## Enunciado

Lo que bloquea el despacho de un vehículo sin lámina **no es la ausencia de placa: es la ausencia de respaldo**.

Un vehículo en estado distinto de `CON_LAMINA` ([`RN-64`](RN-64-estado-de-la-placa-tipificado.md)) **debe** tener un **documento de respaldo** del catálogo configurable —permiso provisional de circulación, constancia del registro, acta de retención, constancia de trámite— con **emisor, folio, adjunto y vigencia que cubra todo el rango de la misión**, extremos incluidos. Vigencia insuficiente o vencida a mitad del rango: **bloquea**, con el mismo patrón de [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md).

Y el despacho **debe** emitir, imprimir y entregar contra acuse el **paquete de identificación en carretera**, con folio y QR verificable, que contiene:

1. Correlativo institucional, chasis/VIN, número de motor y número de bien del inventario nacional
2. Número de placa asignado, si existe, y el estado de placa vigente
3. El documento de respaldo con su emisor, folio y vigencia
4. Institución, dependencia, motorista y ventana de la misión
5. Fotografía vigente del vehículo con su rotulación ([`RN-18`](RN-18-rotulacion-del-vehiculo-del-estado.md))

## Justificación

[`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md) **permite** operar sin placa; **nadie lo exige documentar en carretera**. El control policial en Honduras es físico, y un vehículo del Estado sin lámina, sin papel y con un motorista que solo puede explicarlo de palabra es un vehículo retenido — con la misión abortada, el personal varado y un expediente ante autoridad que después hay que resolver.

Lo que no está en el paquete impreso **no viaja**. La premisa rectora 4 lo dice para todo el sistema; aquí es la diferencia entre continuar la misión y perderla.

La exigencia de vigencia en todo el rango, y no solo el día de salida, replica exactamente el razonamiento de [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md): un permiso provisional que vence el miércoles no ampara una misión que retorna el viernes, y el problema aparece a 200 km de la sede.

## Condiciones de aplicación

Aplica a todo despacho de un vehículo cuyo estado de placa no sea `CON_LAMINA`.

Aplica también a la **sustitución de vehículo** ([`RN-61`](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)): si el entrante no tiene lámina, se re-emite el paquete.

**No aplica** al vehículo `CON_LAMINA`, cuyo despacho ya lleva la Orden de Misión impresa y, cuando corresponde, el salvoconducto ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md)).

## Comportamiento esperado

1. Al despachar, el sistema evalúa la vigencia del respaldo contra la **fecha de fin de la ventana** más la holgura de retorno configurada `[C]` insumo #1. Si no cubre, bloquea indicando la fecha concreta: *"Permiso provisional folio \<x\> vence el \<fecha\>; la misión retorna el \<fecha\>."*
2. El paquete se genera con folio del rango de la delegación ([`RN-44`](RN-44-identificadores-y-folios-en-el-cliente.md)) y QR verificable, y funciona **sin conectividad** en el dispositivo del motorista además de en papel.
3. La entrega se acusa: quién recibió el paquete, cuándo, con marca de tiempo. Sin acuse registrado, el despacho no se completa.
4. El QR resuelve, para quien lo verifique, la correspondencia entre el vehículo, la misión y la institución — sin exponer datos personales del manifiesto ([`RN-51`](RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md)).
5. Si el respaldo vence **durante** la misión sin que se haya detectado antes, el hecho se registra como evento de bitácora con fecha, ubicación y odómetro, y la misión cierra con hallazgo — el mismo tratamiento sobrevenido de [`RN-55`](RN-55-habilitacion-vencida-durante-la-mision.md).
6. La **constatación de rotulación** de estos vehículos caduca con un umbral más corto que el de la flota con lámina: la rotulación es su única identificación visible como bien del Estado. Parámetro `vigencia_constatacion_rotulacion` diferenciado por estado de placa.

## Casos límite

- **Institución sin permiso provisional disponible** porque el trámite está detenido. El vehículo no se despacha a misión fuera del casco urbano hasta que exista respaldo, y el hecho —vehículos inmovilizados por falta de documento— entra al reporte de disponibilidad de flota con su causa. La salida no es despachar sin respaldo: es que el dato haga visible el costo del trámite detenido.
- **Retención en carretera pese al paquete.** Se registra como evento de interrupción ([`RN-70`](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)) con autoridad interviniente y número de expediente, y el vehículo pasa a `LAMINA_RETENIDA_POR_AUTORIDAD` o a retenido según el caso ([`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)).
- **Motocicletas.** `[C]` insumo #43 — la rotulación de motocicletas no está resuelta, y en ellas la identificación visible es todavía más escasa.
- **Vehículo alquilado con placa particular vigente.** Está `CON_LAMINA` y esta regla no le aplica; le aplica en cambio la declaración de tenencia ([`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)) en el paquete de despacho, para que el control en carretera no lo lea como un vehículo del Estado sin identificar.
- **Paquete emitido y misión reprogramada.** El paquete se anula con su folio y se emite uno nuevo. Ningún folio se reutiliza ([`RN-04`](RN-04-anulacion-como-asiento-reverso.md)).

## Trazabilidad

- Autoridad: [orden-de-mision.md `BD-03`](../../03-arquitectura/estados/orden-de-mision.md)
- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]`, [NRM-06](../normativa/NRM-06-transito-y-licencias.md) `[P]`
- Reglas relacionadas: [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-15](RN-15-identidad-del-vehiculo-y-placa.md), [RN-18](RN-18-rotulacion-del-vehiculo-del-estado.md), [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md), [RN-64](RN-64-estado-de-la-placa-tipificado.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md)
- Casos especiales: [CE-17](../../02-requisitos/casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — candidatas `RN-C17b`, `RN-C17c`, `RN-C17e`
- Insumos pendientes: #1 holgura de retorno · #43 rotulación de motocicletas
