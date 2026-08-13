# CU-05 — Emitir la Orden de Misión y sus documentos imprimibles

| Campo | Valor |
|---|---|
| **Módulos** | M-15 Formatos Oficiales e Impresión · M-07 Programación y Despacho |
| **Actor principal** | `ACT-05` Encargado de Despacho — ejecuta `T-12`, que es donde **se consume el folio y se emiten los documentos oficiales** (`EF-02`) · `ACT-10` Encargado de Delegación en su ámbito |
| **Actores secundarios** | `ACT-04` Jefe de Transporte (arma el contenido y reserva el folio en `T-08`), `ACT-06` Motorista (recibe y porta los documentos), `ACT-07` Encargado de Combustible (emite la asignación de fondo con folio propio), `ACT-09` Máxima Autoridad (salvoconducto), `ACT-15` Verificador en Carretera (**no autenticado**, destinatario del QR), `ACT-12` Auditor Interno |
| **Precondiciones** | 1. El expediente está en `PROGRAMADA` con `INV-12` a `INV-16`: vehículo y motorista asignados y reservados, verificaciones registradas con sus datos concretos, **folio reservado y no consumido**. 2. El vehículo tiene **categoría de peaje resuelta y vigente**; sin ella no hay tarifa esperada y el documento no cumple [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md). 3. Si la ventana toca franja inhábil, existe permiso vigente de `ACT-09` — `BD-04`. 4. Si hay personas externas, el manifiesto y la cadena de custodia están resueltos — `PC-12`. 5. Hay rango de folios disponible y **capacidad de impresión** |
| **Postcondiciones** | El folio de la Orden de Misión está **consumido** y el documento impreso tiene su huella registrada (`INV-18`). Está emitido el juego completo que corresponda —Orden de Misión, salvoconducto, manifiesto, hoja de bitácora, asignación de fondo—, cada uno con folio, QR de verificación, espacio de firma y sello y huella del contenido electrónico. El **paquete normativo de la misión está congelado** (`EF-03`, `INV-21`). Los documentos están impresos y en manos de `ACT-06`. El expediente está en `DESPACHADA` |
| **Disparador** | `ACT-05` inicia el despacho de una misión programada (`PR-01` E6 y E8) |

> **Alcance.** Este caso de uso cubre **el acto documental**: qué se emite, con qué contenido, con qué folio, cuándo se consume y cómo se verifica. La verificación física del vehículo, la entrega de llaves y de la custodia y la entrega del fondo contra firma ocurren en la misma transición `T-12` pero se documentan en el caso de uso de despacho.

## Flujo principal

1. Con el expediente en `PROGRAMADA`, `ACT-04` revisa el contenido de la Orden. El sistema ofrece una **vista previa marcada visiblemente como no válida para circulación**. `INV-15`: el folio está **reservado**, no consumido, y en `PROGRAMADA` **no se puede imprimir la Orden como documento válido**.
2. El sistema comprueba las precondiciones documentales: categoría de peaje resuelta ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)); permiso de circulación vigente si la ventana toca franja inhábil (`BD-04`); manifiesto de personas externas si aplica (`PC-12`); paquete de identificación impreso y acusado si el vehículo no tiene lámina ([`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md)).
3. `ACT-05` inicia **`T-12` despachar**. Antes de emitir nada, el sistema **revalida contra el momento del despacho, no contra el de la programación**: `BD-02` licencia, `BD-03` documentación del vehículo, `BD-10` disponibilidad del motorista y estado operativo del vehículo. Entre programar y despachar pueden pasar días, y una licencia puede haber vencido — y pasa.
4. El sistema verifica `BD-06` **segregación de funciones operativas**: quien despacha **no es** el solicitante, ni el autorizador, ni el motorista, ni quien entrega el combustible — `PC-09`, incompatibilidades `I-02`, `I-05`, `I-08`, `I-11`. `I-10` e `I-11` son núcleo irreductible: **no los levanta ningún régimen**.
5. El sistema aplica `EF-02` y **consume el folio** de la Orden de Misión, del rango asignado a la delegación.
6. El sistema emite el **juego de documentos que corresponda al caso**:

   | Documento | Cuándo se emite | Regla |
   |---|---|---|
   | **Orden de Misión** | Siempre | [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [`RN-59`](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) |
   | **Salvoconducto** | Si la ventana toca día u hora inhábil | [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) |
   | **Manifiesto de personas externas** | Si hay traslado de personas externas (M-17) | [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) |
   | **Hoja de bitácora en papel** | Siempre | [`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) |
   | **Constancia de asignación de fondo de combustible** | Si hay fondo asignado a la misión | [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) |
   | **Acta de entrega-recepción de la carga** | Si el objeto del traslado incluye carga | [`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) |
   | **Paquete de identificación del vehículo** | Si el vehículo no tiene lámina metálica | [`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) |

7. Cada documento lleva, sin excepción ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)): **folio único** en la institución; **QR de verificación**; espacio de **firma y sello**; **huella del documento electrónico** en el pie; identificación del vehículo por **correlativo institucional y placa si existe**; y **vigencia explícita: desde cuándo y hasta cuándo ampara**.
8. La Orden de Misión contiene, además: vehículo y motorista; la solicitud o **solicitudes vinculadas** si la misión fue consolidada; objeto del traslado con ocupantes o descripción de la carga; ruta autorizada con sus destinos en orden; ventana temporal; estimado de combustible; y la **sección de peajes por punto** — nombre y ubicación, categoría asignada **con su fundamento**, tarifa esperada del paquete congelado, marca de exoneración si la hay, e identificador y vigencia de la tabla usada ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)).
9. Junto a cada punto de peaje, el impreso deja un **espacio de captura manual** —monto efectivamente cobrado y número de ticket— y la **instrucción de actuación**: exigir el ticket, anotar el monto, presentar el documento y registrar la discrepancia. El sobrecosto **nunca se imputa al motorista**.
10. La Orden imprime también **las advertencias que se superaron y quién continuó a pesar de ellas**: póliza o revisión vencidas ([`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md)), constatación de rotulación caducada ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md)), evaluación realizada sobre un espejo desactualizado ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)), y el **escalamiento de la autorización** si lo hubo — *quien reciba la orden en carretera debe poder ver por qué firmó quien firmó* ([`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md)).
11. La hoja de bitácora se imprime con folio, QR y **paridad exacta de campos, nombres y orden con la pantalla de digitación** y con el formato en papel vigente de la institución ([`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)). `[C]` Formatos en papel vigentes — insumo #2.
12. El sistema aplica `EF-03` y **congela el paquete normativo** de la misión: tabla de tarifas de peaje por punto y categoría, categoría asignada al vehículo y su fundamento, calendario de días hábiles de la delegación, matriz licencia↔vehículo, rendimiento esperado del vehículo, umbrales de desviación, holguras y plazos. **Todo cálculo posterior de esta misión usa ese paquete**, aunque las tablas cambien mientras el vehículo está en ruta.
13. Los documentos **se imprimen**. `ACT-05` los entrega a `ACT-06`. El sistema registra la huella del documento impreso (`INV-18`) y el conteo de impresiones.
14. El expediente pasa a `DESPACHADA`. En ese estado **no se puede reimprimir la Orden con el mismo folio y contenido distinto**, ni cambiar de vehículo o motorista sin revertir primero a `PROGRAMADA` mediante devolución de lo entregado.
15. A partir de aquí el papel circula. Su destinatario en carretera es `ACT-15`, que verifica por QR el **mínimo verificable** y nunca el expediente.

## Flujos alternos

**A1 — Emisión anticipada para delegación sin cobertura** (desde el paso 3)

1. `NRM-09` `[V]` exige **emisión anticipada de documentos** para las delegaciones que salen a zona sin cobertura: por eso el folio se **reserva** en `T-08` y no en `T-12`.
2. `ACT-10` ejecuta `T-12` **sin conectividad**, con el **código de autorización fuera de línea** (§6.6 de la máquina de estados), tomando el folio del rango de su delegación ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)).
3. Los documentos se imprimen **en la delegación, antes de salir**. El paquete normativo se congela con las tablas que el dispositivo tenga sincronizadas, y esa condición **se imprime en el documento**.
4. Si la misión se modifica después, el papel que porta `ACT-06` deja de corresponder: la página de verificación debe poder devolver **desactualizado**, no solo *vigente* o *anulado*.
5. Ver la nota de hallazgo `HCU-09` sobre qué significa exactamente "anticipada".

**A2 — Reimpresión** (desde el paso 13)

1. Un documento se reimprime **tantas veces como haga falta, con el mismo folio y el mismo contenido**. Cada reimpresión se registra con actor, momento y motivo; el conteo de impresiones es dato de auditoría.
2. **Lo que no existe es reimprimir con contenido distinto**: eso es un documento nuevo, con folio nuevo, que declara *"sustituye al folio X"*.
3. Documento perdido en ruta: se reimprime con el mismo folio registrando el motivo. **No se emite folio nuevo**: dos folios para un mismo permiso u orden rompen la conciliación.

**A3 — Verificación en carretera** (desde el paso 15)

1. `ACT-15` escanea el QR **sin autenticarse**. El sistema devuelve folio, tipo de documento, institución, estado —vigente, anulado, vencido o desactualizado—, vehículo, ventana temporal autorizada y huella. **Nunca** nombres de personas trasladadas, montos ni el expediente.
2. Cada consulta y **cada verificación fallida** quedan registradas: un patrón de folios inexistentes consultados es información valiosa.
3. Sin datos móviles del lado del verificador, quedan el contraste visual de la huella impresa, el código de verificación corto y la consulta telefónica `[I]`.
4. `[C]` Si la institución acepta exponer un punto de verificación público siendo el despliegue on-premise — pendiente G.

**A4 — Traslado de personas externas** (desde el paso 6)

1. El manifiesto **se cierra al despachar**: los cambios en ruta se registran como novedad, **no como edición** ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)).
2. `ACT-06` ve el manifiesto **de su misión y de ninguna otra**; `ACT-05` lo ve el día del despacho. **Toda consulta se registra**, incluidas las de `ACT-12` ([`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)).
3. El manifiesto impreso lleva folio y QR, pero su verificación pública **no expone identidades**.

**A5 — Traslado de carga** (desde el paso 6)

1. Se emite el **acta de entrega-recepción** con el inventario declarado, para firma de quien recibe ([`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md)).
2. `ACT-15` puede ser personal de la institución receptora que verifica la entrega, no solo autoridad de tránsito.

## Flujos de excepción

**E1 — El vehículo no tiene categoría de peaje resuelta** (en el paso 2)

1. **El despacho se bloquea**: sin categoría no hay tarifa esperada, y sin tarifa esperada el documento no cumple [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md).
2. **La salida es completar la ficha técnica, no imprimir sin el dato.**
3. Ver la nota de hallazgo `HCU-06` de [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md): esta comprobación debería haber bloqueado ya en `T-08`.

**E2 — La ventana toca franja inhábil y no hay permiso vigente** (en el paso 2)

1. `BD-04` bloquea `T-12` — **bloqueo duro**, `PC-03`. Sin permiso no hay emisión y sin emisión no hay salida.
2. Mensaje accionable con el folio, la fecha inhábil y la antigüedad del trámite pendiente. Ver [CU-03](CU-03-permiso-de-circulacion-en-dia-inhabil.md) E4.

**E3 — Personas externas sin manifiesto ni cadena de custodia** (en el paso 2)

1. `PC-12` **bloquea el despacho**: el manifiesto emitido y la cadena de custodia registrada son previos a la salida.
2. `[C]` Requisitos documentales concretos según el tipo de institución y la naturaleza del traslado — insumos #1 y #39. **No se inventan.**

**E4 — La revalidación al despacho falla** (en el paso 3)

1. Si la licencia venció entre la programación y la salida, si el vehículo pasó a `EN_TALLER`, o si el motorista dejó de estar disponible, **no se emite nada**: `BD-02`, `BD-03` y `BD-10` bloquean `T-12`.
2. El expediente sigue en `PROGRAMADA`. La salida es `T-10` sustituyendo el recurso —revalidando todo para el entrante— o `T-11` devolviendo la misión a la cola.
3. **No hay forma de forzarlo.** Ver [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) y [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md).

**E5 — El rango de folios de la delegación se agota estando desconectada** (en el paso 5)

1. Es un incidente operativo **previsible**: el sistema alerta por consumo del rango con anticipación configurable.
2. `[C]` Procedimiento de ampliación de rango sin conectividad — insumo #1. Mientras no exista, una delegación con el rango agotado y sin red **no puede emitir**, y eso es un requisito de despliegue, no una excepción a la regla.

**E6 — La misión se anula después de emitidos los documentos** (después del paso 13)

1. `T-15` exige, **todas obligatorias**: que el vehículo no haya salido; **devolución íntegra del fondo o de los vales** con acta firmada; devolución de la custodia con acta y odómetro; y devolución física de los documentos impresos **o constancia de su destrucción con acta**. `[C]` Cuál de las dos exige la institución — insumo #1.
2. **Todos los folios emitidos —Orden, salvoconducto, manifiesto, vales— pasan a `ANULADO`** con referencia cruzada a la misión y al acta. **No se reciclan.**
3. La página de verificación refleja la anulación **de inmediato**, para que un papel anulado no pase un control ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)).
4. **Nada se borra**: la anulación de la asignación de fondo es un **asiento reverso** con motivo y autor (`EF-06`, [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)).
5. Si hubo **cualquier consumo**, aunque sea parcial, `T-15` **no está disponible** y el camino es `T-16` misión no ejecutada con consumo: hubo movimiento de fondos públicos y tiene que liquidarse, aunque el kilometraje sea cero. Ver [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md).
6. Mientras la devolución no esté completa, la misión **sigue en `DESPACHADA`** con la marca *anulación en trámite* y la lista de pendientes visible. No se inventa un estado intermedio.

**E7 — Hay que sustituir el vehículo después de emitida la Orden** (después del paso 13)

1. En `DESPACHADA` **no se cambia de vehículo ni de motorista** sin revertir primero a `PROGRAMADA` mediante devolución de lo entregado.
2. El documento nuevo lleva la categoría, la tarifa esperada y el rendimiento del vehículo sustituto, **recalculados y vueltos a congelar**, con asiento de la diferencia ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)).
3. El documento anterior queda `ANULADO` y el nuevo declara a cuál sustituye.

**E8 — La tarifa de un punto no está cargada o está marcada como no verificada** (en el paso 8)

1. Si está marcada **no verificada**, el documento **la imprime igual**, rotulada *tarifa no verificada — referencia*, y la detección de discrepancia sobre ese punto se presenta como **no concluyente**.
2. Si no hay tabla cargada para el punto, el impreso dice *tarifa no disponible* y el paso se registrará sin esperado.
3. Un detector montado sobre una tabla no verificada **produce reclamos falsos en masa y destruye la credibilidad del primero que sí era cierto**.
4. `[C]` Tarifa efectivamente vigente — insumo #21; exoneraciones oficiales — insumo #22.

**E9 — La delegación no tiene impresora** (en el paso 13)

1. **No hay excepción.** El control en carretera es físico: sin documento impreso no hay salida.
2. Es un **requisito de despliegue**. `[C]` Verificar la capacidad de impresión de todas las delegaciones — insumo #27.

## Reglas aplicables

| Regla | Qué gobierna en este caso de uso |
|---|---|
| [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) | Folio único, QR, firma y sello, huella y vigencia explícita en todo documento de control en carretera |
| [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) | Folios de rango por delegación — es lo que hace posible la emisión sin conectividad |
| [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) | Categoría, fundamento y tarifa esperada por punto de peaje, con espacio de captura manual |
| [`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) | Hoja de bitácora impresa con paridad exacta con la pantalla de digitación |
| [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) | Constancia de asignación de fondo con folio; **la entrega ocurre dentro de `T-12`**, no antes |
| [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md), [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) | Manifiesto cerrado al despachar, minimización y registro de consultas |
| [`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) | Inventario de la carga y acta de entrega-recepción |
| [`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md), [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) | Identificación por correlativo institucional; paquete de identificación cuando no hay lámina |
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | `BD-06` en el despacho: quien despacha ≠ solicitante ≠ autorizador ≠ motorista ≠ quien entrega el fondo |
| [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) | Folios anulados con asiento reverso; el documento emitido no se edita |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) | Congelamiento del paquete normativo y corrección retroactiva con asiento de diferencia |
| [`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) | Sustitución de vehículo: recalcular, volver a congelar y reemitir |

## Notas de hallazgo

**`HCU-08` — quién emite la Orden de Misión y en qué estado.** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) E6 dice que *"el Jefe de Transporte (`ACT-04`) emite la Orden de Misión"* y que a continuación *"la misión pasa a `PROGRAMADA`"*. La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) dice lo contrario en dos lugares: en `PROGRAMADA` **"no se puede imprimir la Orden de Misión como documento válido"** —solo vista previa marcada como no válida para circulación—, y `EF-02` sitúa el **consumo del folio y la emisión de los documentos oficiales dentro de `T-12`**, que ejecuta `ACT-05`.

Este caso de uso sigue a la máquina de estados, que es la autoridad en transiciones y precondiciones: **`ACT-04` arma el contenido y reserva el folio en `T-08`; `ACT-05` consume el folio y emite en `T-12`.** En delegación, `ACT-10` concentra ambos papeles sin que eso levante `BD-06`. Se reporta contra `PR-01` E6 y contra la tabla de la §5 de `PR-01`, que atribuye E6 a `ACT-04` o `ACT-10`. Es la misma clase de corrección que ya se aplicó en `HB1-06` para la entrega del fondo.

**`HCU-09` — qué significa "emisión anticipada".** [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` exige poder emitir documentos por adelantado para zonas sin cobertura, y por eso el folio se reserva al programar. Pero el folio **se consume al despachar**, y en `PROGRAMADA` el documento no es válido para circulación. Las dos afirmaciones solo son compatibles bajo una lectura: **anticipada significa ejecutar `T-12` sin conectividad, con folio del rango local, e imprimir en la delegación antes de salir** — no imprimir un documento válido días antes con la misión todavía en `PROGRAMADA`.

Es la lectura que se adopta aquí. Si lo que la institución necesita es lo segundo —imprimir con antelación real, estando la misión aún programada—, entonces `INV-15` y §10.1 de la máquina de estados no lo permiten y hace falta una decisión de producto explícita, porque implicaría emitir un documento oficial antes de la revalidación del despacho. `[C]` Insumos #1, #41.

**`HCU-10` — nada dice qué pasa con la vista previa impresa.** La máquina de estados admite imprimir en `PROGRAMADA` una vista previa *"marcada visiblemente como no válida para circulación"*, pero ningún artefacto define si esa impresión se registra ni cómo se distingue del documento válido en un operativo. Recomendación: la vista previa **no lleva folio consumido, no lleva QR resoluble y lleva marca de agua**, y su impresión se registra igual. Se deja escalado al PO por no tener regla que lo gobierne.

## Trazabilidad

- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E6, E7 y E8; puntos de control `PC-03`, `PC-04`, `PC-05`, `PC-08b`, `PC-09`, `PC-12`, `PC-16`
- **Transiciones**: `T-12` despachar —donde se consume el folio y se emiten los documentos—, `T-08` donde se reserva, `T-11` donde se anula el folio reservado, `T-15` y `T-16` donde se anulan los emitidos — [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)
- **Invariantes**: `INV-15` folio reservado en `PROGRAMADA`; `INV-17` a `INV-23` en `DESPACHADA`
- **Bloqueos**: `BD-02`, `BD-03`, `BD-04`, `BD-06`, `BD-10`, revalidados **al momento del despacho**
- **Efectos**: `EF-02` folios: reserva, consumo y anulación · `EF-03` congelamiento del paquete normativo · `EF-04` entrega del fondo · `EF-06` asiento reverso al anular
- **Actores**: [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) — `ACT-04`, `ACT-05`, `ACT-06`, `ACT-07`, `ACT-09`, `ACT-10`, `ACT-12`, `ACT-15` **no autenticado**; incompatibilidades `I-02`, `I-05`, `I-08`, `I-11`
- **Casos especiales**: [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) (E6), [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) y [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) (E4 y E7), [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) (paso 7), [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) (pasos 8 y 9, y E8), [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) (paso 11 y A1), [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) (A5). **Descartados explícitamente:** [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md), [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md), [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md), [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md), [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — todos posteriores al retorno, no condicionan la emisión
- **Normativa**: [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` salvoconducto portable y verificable, identificación del vehículo del Estado · [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) documentos que siguen requiriendo papel y página pública de verificación · [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` emisión anticipada y paridad con el formato en papel · [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) `[P]` tarifas por punto y categoría · [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) minimización en el manifiesto
- **Decisiones**: [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-03, D-04 · premisa rectora 4 de `CLAUDE.md` · corrección `HB1-06` sobre el momento de entrega del fondo
- **Insumos pendientes**: #1, #2, #21, #22, #24, #27, #39, #41, pendiente G en [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md)
- **Aguas arriba**: [CU-03](CU-03-permiso-de-circulacion-en-dia-inhabil.md), [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md) · **Aguas abajo**: despacho físico y bitácora (M-08), verificación en carretera por `ACT-15`
