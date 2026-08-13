# RN-92 — El reclamo por discrepancia de clasificación es un objeto con estado, destinatario y resultado económico; las discrepancias no se cierran sin él

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-13, M-14 |
| **Origen** | Caso especial [CE-24](../../02-requisitos/casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) · Norma [NRM-10](../normativa/NRM-10-peajes.md) |
| **Verificación** | `[P]` la existencia de categorías por punto y del mecanismo de reclamo — [NRM-10](../normativa/NRM-10-peajes.md). `[V]` el precedente del comunicado de la SAPP del 17/09/2025, citado en [NRM-10](../normativa/NRM-10-peajes.md) con nivel `[V]`; se conserva ese nivel |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo de destinatarios y umbral de agrupación para presentar |

## Enunciado

El **reclamo por discrepancia de clasificación de peaje** es un **objeto propio** con: **estado**, **destinatario**, **fecha de presentación**, **resolución** y **resultado económico**.

Las discrepancias que lo integran **no se dan por cerradas hasta que el reclamo se resuelve** — pero **el reclamo abierto no impide cerrar la Orden de Misión**.

> **Corrección — hallazgo `HB3-02`.** La redacción anterior, leída junto con `T-21`, dejaba **el expediente atrapado en `LIQUIDADA` indefinidamente**: el reclamo ante la SAPP tarda meses y la discrepancia de clasificación no está entre los criterios `H-01` a `H-08`, cuya lista está cerrada. No había salida ni por `T-21` ni por `T-22`.
>
> Es lo que [`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md) evitó al admitir `CERRADA_CON_HALLAZGO`: *un expediente que no puede cerrarse se abandona.*
>
> **Lo que se cierra es la misión; lo que sigue abierto es el reclamo.** Son dos expedientes distintos. El reclamo es una gestión **contra un tercero** por un cobro indebido de la concesionaria — es una **cuenta por cobrar**, no un hallazgo sobre la conducta de la institución. Sobrevive al cierre con su monto, igual que la obligación de reintegro de [`RN-86`](.).
>
> `[C]` Decisión del PO pendiente. La alternativa era abrir un `H-09` y cerrar como `CERRADA_CON_HALLAZGO`; se descartó porque marcaría a la institución por un error del concesionario.

El **sobrecosto por clasificación** se tipifica en cada liquidación con la constancia de que **no se imputó al motorista**.

## Justificación

[`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md), en su comportamiento 4, describe un **reporte** —*"listo para presentar ante la SAPP"*— **no un objeto con ciclo de vida**. Sin estado, **un reclamo presentado y jamás respondido no se distingue de uno que nadie presentó**, y la recuperación del sobrecosto no tiene dónde registrarse.

El costo de esa indistinción es exactamente el hallazgo que la institución quiere evitar: un sobrecosto detectado, tipificado y reclamado **es control interno funcionando**; un sobrecosto detectado y abandonado en un reporte es dinero público pagado de más con la agravante de que alguien lo vio.

Existe además precedente institucional: el comunicado de la SAPP del 17/09/2025 `[V]` es el fundamento que un reclamo puede invocar. Un expediente que agrupa discrepancias por punto y período, con monto acumulado y precedente citado, tiene peso; una lista suelta de tickets, no.

## Condiciones de aplicación

Aplica a toda discrepancia detectada por [`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md) entre la categoría esperada y la cobrada.

**No aplica** a la diferencia por **cambio de tarifa** entre el despacho y el paso por la caseta, que no es discrepancia sino actualización, y se tipifica distinto ([`RN-91`](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)).

**No aplica** cuando la tarifa del punto está marcada como **no verificada**: ahí la detección es **no concluyente** y no alimenta reclamo. Un detector montado sobre una tabla no verificada produce reclamos falsos en masa y destruye la credibilidad del primero que sí era cierto.

## Comportamiento esperado

1. Cada discrepancia se registra con: punto, fecha y hora, **categoría esperada con su fundamento**, categoría cobrada, monto esperado, monto pagado, diferencia, medio de pago y fotografía del ticket o referencia a la línea del estado de cuenta.
2. Cada discrepancia lleva su **fuerza probatoria**, para que quien audite sepa cuánto pesa el expediente sin abrir cada adjunto.
3. El sistema **agrupa** discrepancias por punto y período y propone abrir reclamo cuando el monto acumulado supera el umbral configurado.
4. El reclamo transita estados: `EN_PREPARACION` → `PRESENTADO` → `EN_RESOLUCION` → `RESUELTO_FAVORABLE` / `RESUELTO_DESFAVORABLE` / `SIN_RESPUESTA`. Cada transición registra actor, fecha y documento.
5. `SIN_RESPUESTA` **no es un estado terminal silencioso**: tiene antigüedad contada y aparece en el reporte de control interno mientras exista.
6. El **resultado económico** —monto recuperado, monto perdido— se asienta contra el período corriente con referencia al período afectado ([`RN-94`](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md), [`RN-42`](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)), sin reescribir las liquidaciones históricas.
7. Las discrepancias descubiertas al conciliar un estado de cuenta después del cierre de la misión abren expediente de hallazgo posterior ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)) y desde ahí alimentan el reclamo.

## Casos límite

- **Discrepancia a favor de la institución** — cobraron menos de lo esperado. Se registra igual y no genera reclamo; su acumulación puede indicar que la categoría asignada al vehículo está mal ([`RN-33`](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)).
- **Reclamo resuelto desfavorablemente con fundamento válido.** El fundamento se incorpora: si la caseta tenía razón, la categoría del vehículo se corrige con vigencia desde la fecha que corresponda, y las discrepancias futuras de ese punto dejan de generarse.
- **Reclamo que la institución decide no presentar** por monto bajo. Se registra la decisión con autor y motivo. Un reclamo no presentado por decisión es distinto de uno olvidado, y solo el registro los distingue.
- **`[C]` Tarifa vigente no confirmada** — insumo #21. Hay contradicción abierta entre el comunicado de la SIT del 28/02/2026 `[V]` y un agregador comercial. **No se carga ninguna tarifa sin confirmar la vigente** ([NRM-10 §4 y §10](../normativa/NRM-10-peajes.md)).
- **Sobrecosto imputado al motorista** en la liquidación. Está prohibido: la constancia de que **no** se le imputó es parte del expediente ([`RN-74`](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).

## Trazabilidad

- Norma: [NRM-10](../normativa/NRM-10-peajes.md) `[P]` — §9 fuente y fecha de verificación de cada tarifa; precedente SAPP 17/09/2025 `[V]`
- Reglas relacionadas: [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [RN-91](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)
- Casos especiales: [CE-24](../../02-requisitos/casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — candidatas `RN-C24b`, `RN-C24c`
- Insumos pendientes: #21 tarifa de peaje efectivamente vigente · #24 tags de peaje
