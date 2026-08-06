# RN-27 — Toda asignación de combustible tiene folio único, responsable receptor, misión vinculada y constancia de recepción

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-15 |
| **Origen** | `PROP-01` de [insumos-pendientes](../../07-gestion/insumos-pendientes.md); normas [NRM-09](../normativa/NRM-09-realidad-operativa.md) y [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[I]` el vale o cupón físico como mecanismo predominante — `[C]` el mecanismo real de la institución |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Toda entrega de combustible, vale, cupón, efectivo u orden de pago destinada a combustible **debe** registrarse como una **asignación** con:

1. **Folio único** dentro de la institución, no reciclable
2. Fondo del que se descuenta ([RN-26](RN-26-fondo-de-combustible-aprobado.md))
3. **Responsable receptor** identificado — el motorista o el encargado de delegación
4. **Orden de Misión vinculada**, o motorista y período cuando el esquema sea por período `[C]`
5. Monto y/o galones, e instrumento entregado
6. **Constancia de recepción** del receptor

Sin constancia de recepción, la asignación queda en estado *emitida no entregada* y **no se considera consumible ni liquidable**.

## Justificación

`PROP-01`: *"Cada asignación tiene folio, monto, responsable y misión vinculada. El motorista firma la recepción... ningún lempira se mueve sin quedar atado a un folio, un responsable, una misión y un odómetro."*

[NRM-09](../normativa/NRM-09-realidad-operativa.md) exige manejar los vales o cupones como **objetos con folio, estado y ciclo de vida**: emitido → entregado con firma → canjeado con factura → conciliado; o anulado o extraviado con acta.

El folio es lo que permite responder la pregunta de auditoría: *este galón de combustible, ¿de qué fondo salió, quién lo recibió y a qué misión sirvió?*

## Condiciones de aplicación

Aplica a todo instrumento de combustible, incluidos los eventuales medios electrónicos (tarjeta de flota) si la institución los adopta. [NRM-09](../normativa/NRM-09-realidad-operativa.md) exige soportar ambos.

**No aplica** al combustible cargado directamente por el proveedor en el predio institucional sin intermediación de vale, si la institución tiene esa modalidad. `[C]` confirmar; de existir, requiere su propio circuito de control.

## Comportamiento esperado

1. El folio se asigna de un **rango por delegación**, permitiendo emisión anticipada sin conectividad ([RN-44](RN-44-identificadores-y-folios-en-el-cliente.md)).
2. Los estados del instrumento son: emitido, entregado, canjeado, liquidado, anulado, extraviado. Cada transición con actor, fecha y motivo.
3. La constancia de recepción se registra con el esquema interno de autorización ([DP-001 D-04](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)) y se imprime para firma manuscrita cuando el receptor no tenga dispositivo.
4. La anulación o el extravío **exigen acta** con motivo y responsable; el instrumento queda inutilizable y su valor retorna al saldo del fondo solo si no fue canjeado.
5. Quien entrega el combustible (ACT-07) **no puede** ser el receptor ni el liquidador ([RN-01](RN-01-segregacion-de-funciones.md)).

## Casos límite

- **Vale emitido y viaje cancelado.** El instrumento no se destruye ni desaparece: se **anula con acta** y su valor retorna al fondo. Si ya fue entregado al motorista, la anulación exige la **devolución física constatada** o el registro de extravío. Este es el caso especial que más se repite en la operación real y merece su propio `CE-xx`.
- **Vale entregado y canjeado parcialmente.** El instrumento se liquida por lo canjeado y el remanente se devuelve o se anula. No se admite un vale en estado ambiguo indefinido.
- **Motorista que recibe combustible para dos misiones consecutivas.** Si el esquema es por misión, son dos asignaciones. Si el esquema es por período, es una asignación con consumos imputados a varias misiones. `[C]` `PROP-01` deja abierto si un motorista puede arrastrar saldo entre misiones. **El diseño debe soportar ambos** hasta que se decida.
- **Asignación en delegación sin conectividad.** Se emite con folio pre-asignado del rango local y sincroniza después. Si al sincronizar el fondo no tiene saldo, el sistema **no revierte la entrega ya ocurrida**: la registra como **sobregiro** y genera hallazgo. Ver [RN-45](RN-45-cero-sobrescritura-silenciosa.md).
- **Vale extraviado que aparece canjeado en la factura del proveedor.** Contradicción entre el acta de extravío y el canje. El sistema debe detectarla en la conciliación y elevarla como hallazgo con ambos documentos. Es exactamente el tipo de fraude que el circuito de folios existe para descubrir.
- **Combustible entregado a un vehículo distinto al de la orden.** Bloqueo por [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md).
- **Institución que aún no usa folios preimpresos y quiere que el sistema los genere.** Es una decisión abierta de `PROP-01`. El sistema debe soportar folio propio y folio preimpreso capturado, sin obligar a migrar de golpe.

## Trazabilidad

- Normas: [NRM-09](../normativa/NRM-09-realidad-operativa.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-08](../normativa/NRM-08-firma-electronica.md)
- Decisión: [DP-001, D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); `PROP-01`
- Reglas relacionadas: [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md)
- Actores: ACT-04, ACT-06, ACT-07, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
