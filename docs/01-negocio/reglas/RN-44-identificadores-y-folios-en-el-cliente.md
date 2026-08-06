# RN-44 — Los identificadores se generan en el cliente y los folios se asignan de rangos por delegación

| Campo | Valor |
|---|---|
| **Módulos** | M-16, M-15, M-09 |
| **Origen** | Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` la exigencia de identificadores generados en el cliente y emisión anticipada con folio pre-asignado |
| **Tipo** | Bloqueo duro (requisito de construcción verificable) |
| **Configurable** | Sí — tamaño y asignación de los rangos de folio por delegación |

## Enunciado

Todo registro creado en el cliente de campo **debe** recibir su **identificador único generado localmente**, sin consultar al servidor. Ese identificador **debe** ser el mismo en el cliente y en el servidor después de sincronizar.

Todo documento con folio que deba imprimirse antes de salir a zona sin cobertura — orden de misión, salvoconducto, asignación de combustible — **debe** poder emitirse con **folio pre-asignado de un rango reservado a la delegación**.

Los rangos de folio **no deben** solaparse entre delegaciones, y un folio **no debe** reciclarse nunca, ni siquiera si el documento se anula.

## Justificación

[NRM-09](../normativa/NRM-09-realidad-operativa.md) exige ambas cosas: identificadores generados en el cliente (UUID) como base de la resolución de conflictos, y *"emisión anticipada de documentos con folio pre-asignado del rango de la delegación, para imprimirlos antes de salir a zona sin cobertura"*.

Si el identificador lo asigna el servidor, no hay creación sin red, y todo el requisito de operación desconectada cae. Si el folio lo asigna el servidor, no hay documento imprimible antes de salir — y el control en carretera es físico.

El folio, además, es la unidad de trazabilidad del combustible ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)): dos folios iguales en dos delegaciones destruyen la conciliación.

## Condiciones de aplicación

Aplica a todos los registros creables desde el cliente de campo y a todos los documentos del catálogo `documento_imprimible_control` ([RN-25](RN-25-salvoconducto-con-folio-y-qr.md)).

## Comportamiento esperado

1. El identificador local es globalmente único por construcción, no un consecutivo. El servidor lo **acepta tal cual**; no lo reemplaza por uno propio.
2. Los rangos de folio se asignan por delegación con marca de asignación, cantidad disponible y consumida. El sistema **alerta** cuando a una delegación le queda poco rango, con anticipación configurable.
3. Un folio consumido queda registrado aunque el documento se anule; su estado se consulta en la página pública de verificación ([RN-25](RN-25-salvoconducto-con-folio-y-qr.md)).
4. Al sincronizar, si dos registros distintos llegan con el mismo folio, el sistema **no elige**: eleva ambos a la cola de conflictos ([RN-45](RN-45-cero-sobrescritura-silenciosa.md)) y bloquea la reutilización del rango.
5. Si la institución usa folios preimpresos en papel, el sistema debe poder **capturar el folio existente** en lugar de generarlo. `[C]` decisión abierta de `PROP-01`.

## Casos límite

- **Rango agotado en una delegación sin conectividad.** No hay forma de pedir más. Mitigación: alerta anticipada y rangos dimensionados con holgura. Si aun así se agota, la delegación opera en papel y digita después ([RN-47](RN-47-digitacion-diferida-desde-papel.md)) — nunca inventando folios fuera de rango.
- **Reinstalación de la aplicación** que reasigna el mismo rango a otro dispositivo. Es la causa más probable de folios duplicados. El rango se asigna a la **delegación**, y dentro de ella a un dispositivo concreto con registro; una reinstalación debe reclamar el estado del rango antes de emitir.
- **Documento impreso con folio y misión que nunca se ejecuta.** El folio queda consumido y el documento anulado con acta. No se recicla: el consecutivo con huecos explicados es auditable; el consecutivo sin huecos pero reutilizado, no.
- **Dos dispositivos de la misma delegación.** Cada uno recibe un subrango propio. Compartir un rango entre dispositivos sin conectividad garantiza colisión.
- **Migración o reinstalación del servidor** que reinicia contadores. Los rangos ya asignados deben restaurarse desde el respaldo antes de emitir cualquier folio nuevo. `[C]` procedimiento de restauración probado — [NRM-09](../normativa/NRM-09-realidad-operativa.md) lo exige como requisito de despliegue.
- **Identificador local que colisiona.** Con generación adecuada la probabilidad es despreciable, pero si ocurre, el sistema **no sobrescribe**: eleva a conflicto. La regla no admite un "gana el último" en ningún punto.
- **Registro creado en el cliente y también en la oficina** para el mismo hecho. Tendrán identificadores distintos y son duplicados lógicos, no colisión. Se resuelven anulando uno con motivo *duplicado* y vinculándolo al superviviente ([RN-04](RN-04-anulacion-como-asiento-reverso.md)).

## Trazabilidad

- Norma: [NRM-09 — Realidad operativa](../normativa/NRM-09-realidad-operativa.md)
- Reglas relacionadas: [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md)
- Actores: ACT-01, ACT-05, ACT-06, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
