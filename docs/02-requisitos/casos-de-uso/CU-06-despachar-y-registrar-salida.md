# CU-06 — Despachar la misión y registrar la salida

| Campo | Valor |
|---|---|
| **Módulos** | M-07 Programación y Despacho · M-08 Ejecución y Bitácora · M-09 Combustible |
| **Actor principal** | `ACT-05` Encargado de Despacho |
| **Actores secundarios** | `ACT-07` Encargado de Combustible · `ACT-06` Motorista · `ACT-13` Custodio del Vehículo · `ACT-04` Jefe de Transporte · `ACT-10` Encargado de Delegación · `ACT-15` Verificador en Carretera (destinatario del documento impreso) |
| **Precondiciones** | La Orden de Misión está en `PROGRAMADA` con `INV-12` a `INV-16`: un vehículo y un motorista titular asignados con reserva exclusiva, verificaciones `BD-02` a `BD-04` registradas, **folio reservado y no consumido** (`EF-02`), vehículo en estado operativo `ASIGNADO`. La asignación de fondo de combustible está `EMITIDA` y **en custodia de `ACT-07`** — no entregada (`PC-08`, [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md)). Si la ventana toca día u hora inhábil, existe el permiso de `ACT-09`. |
| **Postcondiciones** | La misión queda en `EN_RUTA`. El folio está **consumido** y los documentos oficiales emitidos con QR y hash. El paquete normativo está congelado (`EF-03`). El fondo está `ENTREGADA` contra firma de recepción (`EF-04`, `V-02`). La custodia del vehículo está trasladada al motorista. Hay un dispositivo portador designado con el paquete de misión transferido. El vehículo está `EN_MISION` y la bitácora abierta. |
| **Disparador** | El motorista se presenta al predio en la ventana programada, o `ACT-05` abre la cola de despachos del día. |

Este caso de uso cubre **dos transiciones consecutivas y de actor distinto**: `T-12` despachar, que ejecuta `ACT-05` en el predio, y `T-14` registrar salida, que ejecuta `ACT-06` en su dispositivo y **sin conectividad**. Se documentan juntas porque el orden entre ellas tiene consecuencia legal: entre `DESPACHADA` y `EN_RUTA` hay bienes y dinero público entregados sin ejecución que los justifique — es el estado de mayor exposición de todo el ciclo.

Corresponde a las etapas **E7** y **E8** de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md).

---

## Flujo principal

1. `ACT-05` abre la cola de despachos de su predio y selecciona la misión. El sistema muestra folio reservado, vehículo con su correlativo institucional, motorista titular y de relevo, ruta autorizada con destinos, objeto del traslado, ventana y estimados congelados de combustible y peajes.
2. El sistema **revalida al momento del despacho** —no da por buena la verificación de la programación— `BD-02` licencia habilitante y vigente durante todo el rango, y `BD-03` documentación del vehículo (`PC-04`, `PC-05`). Registra el resultado con los datos concretos usados: número de licencia, categoría, vencimiento, versión de la matriz licencia↔vehículo, atributos del vehículo evaluados y antigüedad del espejo de Talento Humano (§9.2 de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)).
3. El sistema verifica `BD-04`: si la ventana toca día u hora inhábil, exige permiso vigente de `ACT-09` para ese vehículo y esa ventana (`PC-03`, [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md)).
4. El sistema verifica `BD-06` **por identidad de persona, no por rol**: quien despacha no es el solicitante, ni el autorizador, ni el motorista, ni quien entrega el combustible (`PC-09`; pares `I-02`, `I-05`, `I-08`, `I-11` de [`actores-y-roles.md §5.2`](../../01-negocio/actores-y-roles.md); [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)).
5. `ACT-05` y `ACT-06` hacen **juntos** la verificación física del vehículo y el sistema levanta el **acta de entrega**: odómetro inicial con fotografía, nivel de tanque, llantas y llanta de repuesto, herramientas, extintor, documentos a bordo, daños preexistentes fotografiados, y **constatación de la identificación institucional** —franjas, leyenda, siglas y correlativo— con fecha y foto ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md), [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
6. Si el traslado incluye personas externas, `ACT-05` emite el **manifiesto** con folio y registra la cadena de custodia antes de la salida; el manifiesto se cierra al despachar (`PC-12`, [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md), [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)). Si incluye carga, se levanta el inventario y el acta de entrega de la carga ([`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md)).
7. `ACT-05` confirma el despacho. El sistema **consume el folio** (`EF-02`) y emite los documentos oficiales, cada uno con folio, QR de verificación, espacio de firma y sello, y hash del contenido electrónico: Orden de Misión, salvoconducto si aplica, manifiesto si aplica, **hoja de bitácora** ([`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)) y la asignación de fondo ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), M-15). La Orden impresa lleva, **por punto de peaje, la categoría asignada al vehículo y la tarifa esperada** ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)).
8. El sistema **congela el paquete normativo** de la misión (`EF-03`): tabla de tarifas de peaje y categoría del vehículo, calendario de días hábiles de la delegación, matriz licencia↔vehículo, rendimiento esperado, umbrales de desviación, holguras y plazos. Todo cálculo posterior de esta misión usa ese paquete aunque las tablas cambien mientras el vehículo está en ruta ([`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)).
9. **`ACT-07` entrega el fondo o los vales contra firma de recepción de `ACT-06`, dentro de este acto y no antes** (`EF-04`, `V-02`, `PC-08b`). La asignación pasa de `EMITIDA` a `ENTREGADA`; el monto o galonaje entregado queda congelado con la misión. Quien entrega no es quien despacha ni el motorista ([`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md), pares `I-08` e `I-10` — este último del núcleo irreductible, que no se levanta nunca).
10. `ACT-05` entrega llaves, documentos impresos y la **custodia de la misión** contra firma. `ACT-13` conserva la custodia patrimonial permanente del bien; la custodia de la misión es temporal y se registra aparte ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
11. El sistema designa el **dispositivo portador** (`INV-22`) y le transfiere el paquete de misión: expediente, documentos, paquete normativo congelado y catálogos para operar sin red — puntos de peaje de la ruta, estaciones, tipificaciones de evento y guía de actuación en accidente. Registra la transferencia con marca de tiempo: **desde aquí, lo que el dispositivo capture es la fuente primaria**.
12. La misión queda en `DESPACHADA` (`T-12`). El vehículo sigue `ASIGNADO`.
13. `ACT-06` abre la misión en su dispositivo, **sin necesidad de conectividad**, y registra la salida: odómetro de salida, mayor o igual al del acta de entrega, y hora real del hecho.
14. El dispositivo evalúa localmente `BD-05` contra la última lectura conocida del vehículo que trae en su paquete. Si la hora del hecho cae fuera de la ventana autorizada más la tolerancia, **no bloquea** —el vehículo está saliendo— pero exige justificación y marca la misión para revisión (principio `P-2`).
15. La misión pasa a `EN_RUTA` (`T-14`). El vehículo pasa a `EN_MISION` (`W-05`), el motorista queda no disponible y **se abre la bitácora**, cuyos eventos se numeran con secuencia monotónica por dispositivo, no por reloj.
16. `EF-07`: la misión entra en captura desconectada. **El servidor no debe inferir nada del silencio posterior**; el tablero muestra `EN_RUTA` con la leyenda "sin sincronizar desde" y la cuenta de días. La ejecución continúa en [`CU-08`](CU-08-ejecucion-en-ruta-sin-conectividad.md).

---

## Flujos alternos

**A1 — Despacho sin fondo de combustible asignado** (desde el paso 9)
1. La misión no tiene asignación de fondo, o el fondo del período está agotado ([`CE-23`](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md), [`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md)).
2. `PC-08` **no bloquea la misión**: bloquea la emisión del fondo. Despachar sin fondo es posible y queda como **decisión registrada con responsable nombrado**, visible en el expediente y en el documento impreso.
3. `[C]` Confirmar con la institución si admite despachar sin fondo asignado — insumo #1 / `PROP-01`, insumo #7.

**A2 — Emisión anticipada para delegación sin cobertura** (desde el paso 7)
1. `ACT-10` emite e imprime la Orden de Misión y sus documentos **antes** de la salida, con folio del rango pre-asignado de su delegación ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md), [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md)).
2. El folio se **consume** igualmente al despachar; la emisión anticipada no lo desvincula del acto.
3. Si la emisión se hizo con espejo desactualizado más allá del umbral, **la marca se imprime en el documento**: "emitida con datos sincronizados hace N días" ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)).

**A3 — Sale un motorista de relevo declarado en la programación** (en el paso 13)
1. El dispositivo acepta la autenticación de un motorista de relevo **declarado en la programación con su propia verificación de licencia** (`INV-12`).
2. La habilitación se verifica sobre **quien efectivamente conduce**, cualquiera sea su puesto ([`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md)).
3. Quien no está declarado no puede registrar la salida desde el dispositivo: el camino es la sustitución, [`CU-07`](CU-07-sustituir-vehiculo-o-motorista.md).

**A4 — Salida fuera de la ventana autorizada** (en el paso 14)
1. El sistema exige motivo del catálogo y texto libre, registra la desviación y marca la misión para revisión.
2. **No impide la salida.** Un sistema que se niega a registrar que el vehículo salió tres horas tarde deja el hecho fuera del expediente, que es exactamente lo que el auditor busca y no encuentra.

**A5 — Vehículo sin placa metálica** (en el paso 5)
1. "Sin placa metálica" es estado válido: hay desabastecimiento nacional ([`CE-17`](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md)).
2. Se exige documento de respaldo vigente durante todo el rango y el **paquete de identificación impreso y acusado** ([`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md), [`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md)).
3. El umbral de caducidad de la constatación de rotulación es **más corto** sin lámina ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md)).

---

## Flujos de excepción

**E1 — La licencia del motorista venció entre la programación y el despacho** (en el paso 2)
1. `BD-02` bloquea. **No hay excepción configurable, ni por urgencia, ni por autorización superior** ([`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`DP-001 D-12`](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).
2. El sistema muestra el dato concreto: número de licencia, fecha de vencimiento y fecha de fin de rango evaluada, y registra el intento.
3. El camino es sustituir motorista por `T-10` conservando la asignación original ([`CU-07`](CU-07-sustituir-vehiculo-o-motorista.md), [`CE-11`](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md)).

**E2 — El vehículo pasó a `EN_TALLER` después de la programación** (en el paso 2)
1. `BD-03` y el estado operativo bloquean el despacho ([`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md)).
2. Cada reserva afectada exige desenlace explícito ([`RN-60`](../../01-negocio/reglas/RN-60-indisponibilidad-sobrevenida-y-reservas.md), [`CE-16`](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md)): sustituir vehículo (`T-10`), desprogramar (`T-11`) o anular (`T-13`).
3. Si se sustituye vehículo, **todo valor derivado se recalcula y se vuelve a congelar** con asiento de diferencia ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)).

**E3 — Falta el permiso de circulación en día u hora inhábil** (en el paso 3)
1. `BD-04` bloquea el despacho. No existe "continuar de todos modos".
2. El sistema indica qué falta y quién lo emite: `ACT-09` Máxima Autoridad. La autorización de la jefatura no lo sustituye.
3. Excepción única: vehículo marcado como **de servicio exceptuado** con fundamento y vigencia registrados ([`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md)).

**E4 — Quien despacha ejerce otra función de control sobre la misma misión** (en el paso 4)
1. `BD-06` bloquea. Se registra el intento con el par `I-nn` detectado.
2. **No se ofrece régimen de excepción.** En delegación con dotación insuficiente la salida es el **escalamiento a sede** —la función incompatible la ejerce remotamente alguien de la sede— y si la delegación está desconectada, el **código de autorización fuera de línea** (§6.6). Ratificado por [`DP-002`](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md).
3. `[C]` Pronunciamiento de Auditoría Interna sobre el régimen de excepción — insumo #26; dotación real de delegaciones — insumo #27.

**E5 — El motorista no firma la recepción del fondo o no se presenta** (en el paso 9)
1. Sin firma de recepción **no hay entrega**: la asignación permanece `EMITIDA` en custodia de `ACT-07`.
2. La misión **no avanza a `DESPACHADA`**. No se emiten documentos ni se consume folio.
3. El hecho se registra con motivo; si la misión se cae, el camino es `T-11` o `T-13`, no `T-15`, porque nada salió de la caja.

**E6 — La misión se suspende con el fondo ya entregado y el vehículo sin salir** (después del paso 12)
1. Si la devolución es **íntegra**: acta de devolución firmada por `ACT-06` y `ACT-07`, devolución de la custodia con odómetro coincidente, y devolución o constancia de destrucción de los documentos impresos. Se aplica `T-15`, con **asiento reverso** de la asignación (`EF-06`, [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)). Todos los folios emitidos pasan a `ANULADO`; **no se reciclan**.
2. Si hubo **cualquier consumo**, aunque sea parcial —el tanque se llenó la tarde anterior—, `T-15` **no está disponible**: el camino obligatorio es `T-16` hacia `RETORNADA` y la misión se liquida aunque su kilometraje sea cero ([`CE-20`](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md)).
3. Mientras la devolución no esté completa, la misión sigue en `DESPACHADA` con la marca "anulación en trámite" y la lista de pendientes visible.

**E7 — La diferencia de estimado de peajes supera el umbral respecto a lo autorizado** (en el paso 7)
1. El sistema bloquea el despacho hasta que exista la **reautorización**: lo autorizado tenía un costo y ese costo cambió ([`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)).
2. `[C]` Umbral concreto — insumo #1 / #19.

**E8 — Se agotó el rango de folios de la delegación y no hay conectividad** (en el paso 7)
1. El sistema debe haber alertado por consumo del rango con anticipación configurable ([`RNF-21`](../no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md)).
2. Agotado el rango, no se emite documento oficial: un folio duplicado o reciclado destruye la integridad del correlativo.
3. `[C]` Procedimiento de ampliación de rango sin conectividad — insumo #1.

**E9 — No hay dispositivo de campo disponible** (en el paso 11)
1. El despacho continúa: la **hoja de bitácora impresa con folio** cubre la captura en papel, con las casillas en el mismo orden y con los mismos nombres que la pantalla de digitación ([`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)).
2. La misión queda marcada como operada en papel; la digitación diferida se rige por [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) y [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md).
3. La ausencia de dispositivo es **condición institucional**, no falta del motorista, y así se imputa en el indicador de oportunidad de registro.

**E10 — El motorista es también el custodio permanente del vehículo** (en el paso 10)
1. `I-15` —custodio que autoriza la salida de su propio vehículo— es **advertencia con motivo escrito**, no bloqueo.
2. Se permite continuar exigiendo el motivo, que se adjunta al expediente y se lista en el reporte de excepciones.

---

## Reglas aplicables

| Regla | Qué gobierna en este caso |
|---|---|
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | `BD-06` por identidad de persona en el paso 4 |
| [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) | Registro del acto de despacho con identidad, rol ejercido, momento, origen y hash |
| [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) | `EF-06` en la anulación con fondo entregado (E6) |
| [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) · [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) | Revalidación de `BD-02` al despachar |
| [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) | Póliza y revisión: advertencia, bloqueo configurable apagado por defecto |
| [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) | Constatación de identificación institucional con fecha y foto |
| [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) | Estado operativo del vehículo al momento del despacho |
| [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) | Traslado de custodia con constancia |
| [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) · [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) | `BD-04` y el salvoconducto impreso con folio y QR |
| [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) · [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) | Entrega del fondo dentro de `T-12`, contra firma, al motorista de esa orden |
| [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) | Coherencia de odómetro de salida — `BD-05` |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) · [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) | `EF-03` congelamiento del paquete normativo |
| [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) · [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) | `T-14` sin conectividad; folios de rango por delegación |
| [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) · [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) | Manifiesto de personas externas cerrado al despacho |
| [`RN-59`](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) | Ningún vehículo sale sin Orden de Misión, cualquiera sea su régimen |
| [`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md) · [`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) | Despacho de vehículo sin lámina |
| [`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) | Inventario y acta de la carga |
| [`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) · [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) | Hoja de bitácora y tarifas de peaje impresas |
| [`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) | Saldo proyectado del fondo al emitir |

**Requisitos no funcionales que condicionan el caso:** [`RNF-11`](../no-funcionales/RNF-11-formatos-oficiales-imprimibles-y-verificables.md) impresión en la impresora que la delegación ya tiene · [`RNF-21`](../no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md) folios sin duplicar ni reciclar aunque se emitan sin red · [`RNF-03`](../no-funcionales/RNF-03-operacion-sin-conectividad.md) `T-14` sin red · [`RNF-04`](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) registro append-only del acto.

---

## Nota de hallazgo — no se resuelve aquí

**Alcance del salvoconducto.** `BD-04` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) exige permiso vigente "para esa ventana y ese vehículo". `PC-03` de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) exige salvoconducto vigente "para ese vehículo, **motorista** y ventana". La diferencia no es menor: si el salvoconducto ampara al motorista, un relevo en ruta lo invalida. Divergencia ya registrada en el índice de casos especiales como pendiente; se reporta contra `PR-01` porque **la máquina de estados es la autoridad en precondiciones**. Este caso de uso sigue a `BD-04`.

---

## Trazabilidad

- **Transiciones:** `T-12` despachar · `T-14` registrar salida · `T-15` y `T-16` en la excepción E6 · `W-05` estado operativo del vehículo · `V-02` entrega de la asignación de fondo
- **Invariantes y efectos:** `INV-17` a `INV-23` · `EF-02` folios · `EF-03` paquete normativo · `EF-04` entrega del fondo · `EF-06` anulación con fondo entregado · `EF-07` captura desconectada
- **Bloqueos duros:** `BD-02`, `BD-03`, `BD-04`, `BD-05`, `BD-06`
- **Puntos de control de `PR-01`:** `PC-03`, `PC-04`, `PC-05`, `PC-06`, `PC-08`, `PC-08b`, `PC-09`, `PC-12`, `PC-16`
- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E7 y E8
- **Casos especiales:** [`CE-11`](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) · [`CE-16`](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) · [`CE-17`](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) · [`CE-20`](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) · [`CE-23`](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) · [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) · [`CE-01`](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md)
- **Casos de uso relacionados:** [`CU-07`](CU-07-sustituir-vehiculo-o-motorista.md) sustitución · [`CU-08`](CU-08-ejecucion-en-ruta-sin-conectividad.md) continúa desde el paso 16
- **Normativa:** [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) segregación y registro oportuno · [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) custodia, rotulación y circulación en día inhábil · [`NRM-06`](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) licencias y documentación · [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) emisión anticipada y operación desconectada · [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md) categoría y tarifa
- **Decisiones:** [`DP-001`](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-03, D-04, D-12, D-13 · [`DP-002`](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)
- **Historias:** pendientes del Bloque 3
- **Insumos pendientes:** #1 (horario hábil, umbrales, ampliación de rango de folios, despacho sin fondo) · #7 / `PROP-01` (fondo de combustible) · #26 y #27 (segregación en delegaciones) · #2 (formatos en papel vigentes)
