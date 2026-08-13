# CU-13 — Emitir y entregar la asignación de combustible de una misión

| Campo | Valor |
|---|---|
| **Módulos** | M-09 Combustible · M-07 Programación y Despacho |
| **Actor principal** | `ACT-07` Encargado de Combustible |
| **Actores secundarios** | `ACT-04` Jefe de Transporte — programa y genera la propuesta · `ACT-05` Encargado de Despacho — ejecuta `T-12` · `ACT-06` Motorista — firma la recepción · `ACT-08` Gerencia Administrativa — fondo aprobado y levantamientos · `ACT-10` Encargado de Delegación |
| **Precondiciones** | La Orden de Misión está en **`PROGRAMADA`** tras `T-08`, con vehículo y motorista asignados (`INV-12`) y reserva constituida (`EF-01`). Existe **fondo aprobado vigente con saldo suficiente** ([CU-12](CU-12-solicitar-y-aprobar-fondo-de-combustible.md), `PC-08`). Hay folio disponible en el rango de la delegación (`EF-02`) |
| **Postcondiciones** | En el éxito: existe una asignación con **folio**, monto o galonaje, instrumento, misión vinculada y motorista receptor, en estado **`ENTREGADA`** con firma de recepción, y `INV-20` se cumple para el despacho. Si la misión no llega a despacharse, la asignación queda `EMITIDA` sin salir de la custodia de `ACT-07`, o `ANULADA` con acta |
| **Disparador** | `T-08` programar y asignar genera la **propuesta de asignación de fondo de combustible** ([orden-de-mision.md §3.2, efectos de `T-08`](../../03-arquitectura/estados/orden-de-mision.md)) |

> **Dos momentos, no uno.** La **emisión** (`V-01`) ocurre con la misión en `PROGRAMADA`. La **entrega contra firma** (`V-02`) ocurre **dentro de `T-12` despachar** (`EF-04`). En `PROGRAMADA` el instrumento existe emitido con folio y **no sale de la custodia de `ACT-07`**: el estado `PROGRAMADA` lista expresamente *"entregar fondo de combustible"* entre lo que **no se puede**. Corregido en [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) y en `PR-01` E7 por el hallazgo `HB1-06`. Entregar antes dejaría fondo público en manos de alguien cuya misión aún puede no despacharse.

## Flujo principal

### Fase 1 — Emisión, con la misión en `PROGRAMADA` (`V-01`, `PC-08`)

1. `T-08` deja la misión en `PROGRAMADA` y genera la **propuesta de asignación de fondo**. Entra a la bandeja de `ACT-07`.
2. `ACT-07` abre la propuesta. El sistema muestra: misión y su folio reservado (`INV-15`), vehículo con su **correlativo institucional** ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md)), motorista titular y de relevo si los hay, ruta autorizada, **estimado de combustible** según distancia prevista y rendimiento esperado del vehículo, y **estimado de peajes desglosado por punto** ([`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md)).
3. El sistema verifica **saldo suficiente en el fondo vigente** y presenta el saldo **contable y proyectado** ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), [`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md)). `PC-08`.
4. El sistema verifica que **el vehículo y el motorista receptores son los asignados a esa orden** ([`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md), numerales 2 y 3). Cierra la puerta al desvío más simple: sacar el vale a nombre de una misión real y cargarlo en otro vehículo.
5. El sistema verifica la **segregación** por identidad de persona: `ACT-07` que emite ≠ `ACT-05` que despachará (`I-08`) ≠ quien liquidará (`I-10`, **núcleo irreductible**) ≠ el motorista (`I-11`). `PC-09`, [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md).
6. El sistema verifica que el motorista receptor **no tenga saldo vencido ni obligación de reintegro abierta** ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).
7. `ACT-07` emite la asignación indicando **con qué instrumento** —efectivo, vale u orden de pago— y por qué monto o galonaje. El sistema toma un **folio del rango de la delegación** ([`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)). La asignación queda en **`EMITIDA`**.
8. El instrumento se imprime con **folio, QR de verificación, espacio de firma y sello, y hash del contenido electrónico** ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), M-15) — y **permanece bajo custodia de `ACT-07`**.

### Fase 2 — Entrega, dentro de `T-12` despachar (`V-02`, `EF-04`, `PC-08b`)

9. El día de la salida, `ACT-05` ejecuta `T-12` **despachar**: revalida `BD-02` licencia y `BD-03` documentación **al momento del despacho**, no las de la programación; verifica `BD-04` salvoconducto si la ventana toca día u hora inhábil; y levanta el acta de entrega del vehículo con odómetro, nivel de combustible, herramientas, llanta de repuesto, documentos a bordo e **identificación institucional del vehículo con fecha y fotografía**.
10. **Dentro de ese mismo acto**, `ACT-07` entrega el instrumento **contra firma de recepción de `ACT-06`** ([`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md), `PC-08b`). El sistema vuelve a evaluar `BD-06`: quien entrega ≠ quien despacha ≠ el motorista.
11. La asignación pasa a **`ENTREGADA`**. Se congela con la misión el **monto o galonaje entregado** (`EF-04`) y se satisface `INV-20`, precondición del estado `DESPACHADA`.
12. `EF-03` **congela el paquete normativo** de la misión: tabla de tarifas de peaje por punto y categoría vigente a la fecha prevista de cada paso, **categoría de peaje del vehículo y su fundamento**, calendario de días hábiles, matriz licencia↔vehículo, rendimiento esperado del vehículo y umbrales de desviación.
13. La Orden de Misión impresa lleva, **por cada punto de peaje de la ruta, la categoría asignada y la tarifa esperada** del paquete congelado ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)). Con eso en la mano el motorista tiene algo que decir en la caseta; sin eso no tiene nada — [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md).
14. Se emite además la **hoja de bitácora impresa con folio y QR**, con paridad exacta con la pantalla de digitación ([`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)), porque el registro en papel es parte del diseño y no un fracaso.
15. `EF-02` **consume el folio** de la Orden. La misión queda `DESPACHADA` y el fondo, en la mano del motorista, con su firma y su marca de tiempo en el expediente.

## Flujos alternos

**A1 — Misión que se despacha sin fondo asignado** (desde el paso 3) · [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md)

1. Sin saldo suficiente, **`PC-08` bloquea la emisión, no la misión**. La Orden queda en `PROGRAMADA` con la marca *sin fondo asignado*.
2. Despachar así es posible como **decisión registrada con responsable nominado y motivo**, y la marca se arrastra visible hasta la liquidación (`PC-08`).
3. El consumo que ocurra **se registra igual** ([`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)) y se imputa al fondo que se constituya después. *Un hecho se registra aunque exceda cualquier cuota; lo que se controla es el compromiso previo* ([`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)).
4. `[C]` **Si la institución admite despachar sin fondo asignado.** El propio `PC-08` lo deja abierto. Propuesta escalada al PO en [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md): *sí, con responsable nominado, motivo y marca visible hasta la liquidación* — insumo #1.

**A2 — Emisión anticipada para delegación sin cobertura** (desde el paso 7)

1. `ACT-10` emite con **folio pre-asignado del rango de su delegación** e imprime con antelación, junto con la Orden de Misión y el salvoconducto ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), `EF-02`).
2. La entrega sigue ocurriendo **dentro del despacho**, no antes: la anticipación es de la impresión, no de la custodia.
3. Si el paquete de la delegación lleva más del horizonte de validez declarado sin sincronizar, la marca *"emitida con datos sincronizados hace N días"* **se imprime en el documento** ([orden-de-mision.md §6.1](../../03-arquitectura/estados/orden-de-mision.md)).

**A3 — Reasignación de vehículo o motorista antes de salir (`T-10`)** (desde el paso 8)

1. `T-10` revalida todas las precondiciones de `T-08` para el recurso entrante y **conserva la trazabilidad de la asignación original**.
2. Si cambió el vehículo, **se recalcula y se vuelve a congelar todo valor derivado** —categoría de peaje, tarifa esperada, rendimiento esperado, estimado de combustible— **con asiento de diferencia** ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)).
3. El folio de la Orden **no cambia**: es el mismo expediente. El folio de la asignación de fondo sí se reemite si el receptor cambia, y el anterior se anula ([`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md)).

**A4 — Recepción por motorista de relevo o por encargado de delegación** (en el paso 10)

1. Solo puede recibir el **motorista titular** o un **motorista de relevo declarado en la programación**, cada uno con su verificación de licencia registrada (`INV-12`, `BD-02`).
2. `[C]` Si un encargado de delegación puede recibir en nombre del motorista — la posibilidad está prevista en [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) numeral 3 y **no está confirmada** con la institución. Insumo #1.

**A5 — La institución opera con tag prepago para peajes** (en el paso 12)

1. Se registra la **asignación del tag al vehículo y su saldo inicial** (`EF-04`).
2. El medio de pago queda como dato de la asignación, porque **de él depende qué evidencia existe**: con tag no hay ticket, hay estado de cuenta ([CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md), punto 9).
3. `[C]` Si la institución tiene tags y si COVI-H emite estado de cuenta empresarial a su nombre — insumo #24.

**A6 — Asignación por período en lugar de por misión** (desde el paso 1)

1. Si el esquema institucional resulta ser por período, el vínculo obligatorio es **motorista + período + fondo**, y cada consumo se imputa después a una misión concreta ([`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md)).
2. `[C]` El esquema real de la institución — insumo #7 / `PROP-01`. Hasta que se confirme, el fondo se modela como entidad con período de vigencia y saldo, que admite ambos esquemas.

## Flujos de excepción

**E1 — Saldo insuficiente en el fondo** (en el paso 3) · [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md)

1. **Bloqueo de la emisión.** Con `tolerancia_sobregiro` en cero —su valor inicial— no hay excepción.
2. El sistema indica **cuánto falta y qué fondo lo cubriría**, y **distingue las dos causas**: saldo del fondo agotado (se resuelve por ampliación, [CU-12](CU-12-solicitar-y-aprobar-fondo-de-combustible.md) A1) frente a **cuota trimestral copada** (no se resuelve en SIGTI, [`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)).
3. Nunca "fondo agotado" a secas.

**E2 — El motorista tiene obligación de reintegro abierta o saldo vencido** (en el paso 6)

1. El sistema **bloquea toda nueva asignación de fondo a esa persona** ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)). Hoy nada impide seguir entregándole fondo a quien no devolvió el anterior; esto lo impide.
2. El levantamiento —cuando la persona bloqueada es la única disponible para una misión urgente— es **acto registrado de `ACT-08` con motivo**, nunca decisión de quien programa. La excepción queda en el expediente y en el indicador.
3. Si no se levanta, la salida es `T-10` reasignar motorista.

**E3 — Quien va a entregar el fondo es quien va a despachar, o es el motorista** (en el paso 5 o en el paso 10)

1. **Bloqueo duro.** `I-08` despacha × entrega fondo; `I-10` entrega fondo × liquida —núcleo irreductible—; `I-11` motorista × entrega fondo de su propia misión. `PC-09`, [actores-y-roles §5.2](../../01-negocio/actores-y-roles.md).
2. No se guarda nada. El mensaje nombra el conflicto con precisión y el intento queda en la pista de auditoría con el par detectado.
3. El sistema **ofrece escalamiento en el acto**: puesto superior de la misma unidad, o puesto de sede designado como respaldo de esa delegación, o `ACT-08`. La misión no queda trabada: queda visiblemente pendiente en la bandeja de alguien.
4. `I-10` **no lo levanta ningún régimen de excepción, ninguna delegación y ninguna resolución de la máxima autoridad.** *Quien entrega el dinero no puede ser quien declara en qué se gastó.*

**E4 — La misión se desprograma antes del despacho (`T-11`) o se anula (`T-13`)** (desde el paso 8)

1. La asignación `EMITIDA` pasa a **`ANULADA`** (`V-03`) con **acta de anulación con folio** ([actores-y-roles §4.2](../../01-negocio/actores-y-roles.md)).
2. El folio de la asignación queda anulado con referencia cruzada a la misión. **No se recicla, no se reutiliza y no vuelve al rango** (`EF-02`).
3. El folio reservado de la Orden se anula igualmente; al reprogramar se toma uno nuevo (`T-11`).
4. Un vale **nominado a una misión se anula**; solo un vale sin nominar vuelve al inventario disponible (§10.1).

**E5 — La misión se anula ya `DESPACHADA`, con fondo entregado** (desde el paso 11) · [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md)

1. Es la situación más delicada del sistema: hay documentos con folio emitidos y dinero público entregado.
2. La misión **sigue en `DESPACHADA`** con la marca *"anulación en trámite"* y la lista de pendientes visible. No se crea estado intermedio: el control real es la lista de devoluciones pendientes.
3. **Devolución íntegra sin ningún consumo** → `T-15`: acta de devolución firmada por `ACT-06` y `ACT-07`, devolución de la custodia del vehículo con odómetro, y devolución o destrucción con acta de los documentos impresos (`[C]` cuál exige la institución — insumo #1). La asignación va a **`DEVUELTA`** y se registra el **asiento reverso** (`EF-06`, [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)).
4. **Si hubo cualquier consumo, aunque sea un lempira**, `T-15` **no está disponible**: el camino obligatorio es `T-16` hacia `RETORNADA`, y la misión **se liquida aunque su kilometraje sea cero** ([CU-15](CU-15-liquidar-la-mision-y-conciliar.md) A1). Hubo movimiento de fondos públicos y anular sería borrar un hecho económico.
5. El plazo de devolución del saldo corre **desde la anulación** ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).

**E6 — El receptor no es el vehículo ni el motorista de la orden** (en el paso 4 o en el paso 10)

1. **Bloqueo duro** ([`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md), numerales 2 y 3).
2. La salida legítima es `T-10` reasignar el recurso, no forzar la entrega.

**E7 — Se agota el rango de folios de la delegación estando desconectada** (en el paso 7)

1. Es un incidente operativo **previsible**: el sistema alerta por consumo del rango con anticipación configurable (`EF-02`).
2. `[C]` Procedimiento de ampliación de rango sin conectividad — insumo #1. Sin él, una delegación desconectada puede quedarse sin poder emitir instrumentos con folio, que es exactamente el escenario que la emisión anticipada venía a resolver.

**E8 — El estimado de peajes cambió entre la aprobación y la programación** (en el paso 2)

1. `T-08` recalcula el estimado con la tarifa vigente a la fecha ahora programada. Si difiere del congelado en la aprobación por encima del umbral configurable, **se exige nueva autorización antes de despachar**: lo autorizado tenía un costo y ese costo cambió ([`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)).
2. La emisión de la asignación puede hacerse, pero `T-12` no procede sin la reautorización.

## Reglas aplicables

| Regla | Qué aporta a este caso |
|---|---|
| [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) | **Regla eje.** Estado mínimo `PROGRAMADA` para emitir; entrega solo dentro del despacho; receptor = vehículo y motorista de la orden |
| [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) | Folio único, responsable receptor, misión vinculada y constancia de recepción |
| [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) | Sin fondo vigente con saldo no hay asignación; `tolerancia_sobregiro` en cero |
| [`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) | Saldo contable **y** proyectado a la vista antes de emitir |
| [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) | Bloqueo de nueva asignación a quien tiene saldo vencido u obligación abierta |
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | `I-08`, `I-10` y `I-11` evaluadas por identidad de persona sobre esta misión |
| [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) | Categoría de peaje y tarifa esperada, por punto, impresas en la Orden |
| [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) · [`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) | Documentos de control en carretera y hoja de bitácora impresos con folio y QR |
| [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) | Folios de rangos por delegación, para emitir sin conectividad |
| [`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) | La sustitución de vehículo recalcula y vuelve a congelar, con asiento de diferencia |
| [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) | Anulación de la asignación como asiento reverso; folios no se reciclan |
| [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) | El despacho traslada la custodia del vehículo al motorista con constancia |

## Trazabilidad

- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) E6, **E7 emisión**, **E8 despacho y entrega** · puntos de control **`PC-08`** (emisión), **`PC-08b`** (entrega), **`PC-09`** (segregación de quien entrega), `PC-03`, `PC-04`, `PC-05`
- **Autoridad en transiciones:** [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `T-08` (propuesta de asignación), `T-10`, `T-11`, **`T-12`**, `T-13`, `T-15`, `T-16`; `EF-01`, **`EF-02`**, **`EF-03`**, **`EF-04`**, `EF-06`; `INV-12`, `INV-15`, `INV-20`, `INV-21`; **§10.1 `V-01`, `V-02`, `V-03`**
- **Autoridad en actores e incompatibilidades:** [actores-y-roles.md](../../01-negocio/actores-y-roles.md) — matriz filas 10 y 6; §4.2 anulación por estado; §5.2 pares `I-08`, `I-10`, `I-11`
- **Casos especiales:** [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) (E5), [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) (A1, E1), [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) (paso 13), [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) y [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) (A3), [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) (paso 14)
- **Casos de uso encadenados:** ← [CU-12](CU-12-solicitar-y-aprobar-fondo-de-combustible.md) · → [CU-14](CU-14-registrar-consumo-de-combustible-y-peaje.md) consumo en ruta · → [CU-15](CU-15-liquidar-la-mision-y-conciliar.md) liquidación
- **Normativa:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) `[P]` TSC-NOGECI V-07, autorización previa de toda transacción · [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` emisión anticipada de documentos y ciclo del vale con acta · [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) categoría y tarifa congeladas · [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) custodia
- **Decisiones:** [DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md), `PROP-01`
- **Historias:** pendientes — no escritas en este bloque
- **Insumos pendientes:** #1 (¿se admite despachar sin fondo? ¿devolución o destrucción de documentos impresos? ¿ampliación de rango de folios sin red?) · #7 / `PROP-01` (fondo por período o por misión; folio preimpreso o generado) · #24 (tags prepago y estado de cuenta empresarial) · #2 (formatos en papel vigentes del vale y del acta de entrega)
