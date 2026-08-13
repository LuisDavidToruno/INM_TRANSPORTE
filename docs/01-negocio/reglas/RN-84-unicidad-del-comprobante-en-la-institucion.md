# RN-84 — Todo comprobante de gasto es único en la institución por emisor y número, y su reutilización se bloquea al registrarlo

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-18, M-13, M-14, M-16 |
| **Origen** | Caso especial [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-14 conciliación periódica de registros |
| **Verificación** | `[P]` la exigencia de conciliación periódica — [NRM-01](../normativa/NRM-01-control-interno-tsc.md) marca `[P]` el catálogo NOGECI. `[I]` la unicidad como mecanismo: es implicación de requerimiento escrita por el equipo, no articulado citable |
| **Tipo** | Bloqueo duro |
| **Configurable** | No el bloqueo. Sí el catálogo de tipos de comprobante y la forma de la clave por tipo (parámetro `clave_unicidad_comprobante`) |

## Enunciado

Todo comprobante que sustente un gasto imputado a una Orden de Misión — factura de combustible, ticket de peaje, recibo de servicio en ruta — **debe** ser único dentro de la institución por la terna **tipo de comprobante + emisor + número de comprobante**.

El sistema **no debe** aceptar el registro de un segundo consumo o gasto que invoque una terna ya registrada, **aunque el registro provenga de otra dependencia, de otra delegación, de otro fondo, de otra Orden de Misión o de otro ejercicio fiscal**.

La verificación de unicidad **debe** atravesar el alcance de datos: dos delegaciones no se ven entre sí, pero la unicidad del comprobante sí las cruza.

## Justificación

[`RN-28`](RN-28-comprobacion-del-consumo-de-combustible.md) exige galones, monto, estación, odómetro y fotografía. **No exige que el comprobante sea único.** Ese hueco es lo que permite que el mismo papel sostenga dos consumos en dos delegaciones distintas, y que la diferencia aparezca ocho meses después, al conciliar a mano el estado de cuenta del proveedor — cuando el expediente ya está `CERRADA` y solo queda el camino largo del hallazgo posterior ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)).

El control barato es el que se ejecuta en el momento del registro. El caro es el que se ejecuta ocho meses después.

## Condiciones de aplicación

Aplica a todo comprobante con emisor y número identificables, cualquiera sea el módulo que lo registre.

**No aplica** a la evidencia sustituta de [`RN-85`](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md): la constancia de gasto sin comprobante tiene folio propio del sistema, que ya es único por construcción.

**No aplica** al estado de cuenta consolidado de un tag de peaje o de una tarjeta de consumo: ahí el documento único es el estado de cuenta y la unicidad se evalúa sobre la **línea** — referencia de transacción del operador — no sobre el estado.

## Comportamiento esperado

1. Al registrar un consumo o gasto con comprobante, el sistema exige tipo, emisor y número. Si el emisor no está en el catálogo de estaciones y proveedores, se captura con su identificación tributaria y queda pendiente de normalización.
2. Si la terna ya existe, **bloquea** con el mensaje: *"El comprobante \<tipo\> \<emisor\> N.º \<número\> ya está registrado en \<referencia de la Orden de Misión\>, delegación \<X\>, fecha del hecho \<fecha\>. Verifique el número antes de continuar."* El mensaje **no** revela datos del expediente ajeno más allá de su referencia, la delegación y la fecha.
3. El intento bloqueado **se registra** con autor, momento, terna intentada y expediente de origen. Un intento no es un error de tecleo hasta que alguien lo demuestre; sin registro, el segundo intento exitoso con el número alterado en un dígito no deja rastro.
4. En **operación desconectada** la unicidad no se puede evaluar contra la institución completa. El cliente de campo valida contra lo que tiene y marca el registro como *unicidad pendiente de verificación*. Al sincronizar, la colisión **no se resuelve por sobrescritura** ([`RN-45`](RN-45-cero-sobrescritura-silenciosa.md)): va a cola de resolución humana como conflicto de comprobante duplicado.
5. La conciliación periódica contra la fuente externa ([`RN-95`](RN-95-conciliacion-contra-fuentes-externas.md)) vuelve a evaluar la unicidad sobre el universo completo, incluyendo lo digitado tarde.
6. El sistema reporta por período los comprobantes con más de un intento de registro, por delegación y por persona que los presentó.

## Casos límite

- **Estación que reinicia su numeración** por cambio de sistema de facturación o de establecimiento. La terna incluye al **emisor**, y el emisor es el establecimiento, no la cadena. Si aun así colisiona, el parámetro `clave_unicidad_comprobante` admite agregar el CAI o el punto de emisión a la clave para ese emisor.
- **Comprobante que sustenta un gasto de dos vehículos** — dos unidades cargando con una sola factura. Es un comprobante único con **varios renglones de imputación**: se registra una vez y se distribuye por vehículo con galones y monto por renglón. No se registra dos veces.
- **Número ilegible en la fotografía.** No se inventa. Se registra por la vía de [`RN-85`](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) con causa *comprobante deteriorado*, y queda fuera del control de unicidad — con la calificación de suficiencia probatoria que le corresponda.
- **Digitación diferida desde papel** ([`RN-47`](RN-47-digitacion-diferida-desde-papel.md)) que colisiona con un registro ya sincronizado. Prevalece el orden por **fecha del hecho**, no por fecha de captura: el conflicto se abre igual y lo resuelve una persona.
- **Anulación legítima y re-registro.** Un comprobante cuyo registro se anuló por asiento reverso ([`RN-04`](RN-04-anulacion-como-asiento-reverso.md)) libera la terna, pero el sistema conserva la traza del registro anulado y advierte al re-registrarla.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — conciliación periódica de registros `[P]`
- Reglas relacionadas: [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md), [RN-85](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-95](RN-95-conciliacion-contra-fuentes-externas.md)
- Casos especiales: [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — candidata `RN-C28d`
- Actores: ACT-06 registra en campo · ACT-07 recibe el descargo · ACT-10 digita · ACT-12 concilia
