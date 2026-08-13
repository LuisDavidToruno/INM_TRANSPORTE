# CU-02 — Autorizar una solicitud de transporte

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor principal** | `ACT-03` Jefatura Inmediata |
| **Actores secundarios** | `ACT-02` Solicitante, `ACT-04` Jefe de Transporte (recibe la cola de programación), `ACT-08` Gerencia Administrativa (resuelve lo escalado `[C]`), `ACT-09` Máxima Autoridad (resuelve conflictos de segregación), `ACT-12` Auditor Interno (recibe los expedientes marcados), `ACT-16` Sistema ARGOS (cadena y niveles de autorización) |
| **Precondiciones** | 1. El expediente está en `SOLICITADA` con `INV-05` a `INV-08` satisfechos: contenido sustantivo congelado con su huella, número de expediente asignado, estimado de peajes congelado, sin recursos reservados. 2. El actor es autorizador competente del solicitante según la jerarquía espejada de `ACT-16`. 3. El espejo de la jerarquía no está desactualizado más allá del umbral configurable ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)). 4. Se han registrado todas las autorizaciones de nivel anteriores requeridas |
| **Postcondiciones** | El expediente queda en `APROBADA`, `RECHAZADA` o de vuelta en `BORRADOR`, con actor, **rol ejercido en ese momento** —copia, no referencia—, marca de tiempo del hecho y de captura, dispositivo y huella del contenido autorizado, todo inmutable. En `APROBADA` se verifican `INV-09`, `INV-10` (quien autorizó no es quien solicitó — `BD-01`) e `INV-11` (**no se reservó ningún recurso**). Queda calculada la fecha de caducidad de la aprobación. Si la ventana toca franja inhábil, el expediente conserva la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` |
| **Disparador** | `ACT-02` envía la solicitud (`T-02`) y el expediente entra en la bandeja del autorizador, o `ACT-03` abre su bandeja de pendientes (`PR-01` E3) |

## Flujo principal

1. `ACT-03` abre su bandeja de solicitudes pendientes de las dependencias sobre las que tiene competencia. El sistema las ordena por fecha de salida más próxima y señala las que salen dentro de las próximas 24 horas y las marcadas **urgente** por antelación mínima incumplida.
2. `ACT-03` abre un expediente y ve: solicitante de derecho, capturador si lo hubo, dependencia, motivo institucional, **objeto del traslado** con su detalle, origen, destinos en orden, ventana, tipo de vehículo requerido y clave de vínculo con `ACT-16` si hay viático asociado.
3. El sistema muestra las **validaciones ya evaluadas** en `T-02`, todas visibles antes de decidir: si la ventana toca día u hora inhábil y qué tramos; el **estimado de peajes desglosado punto por punto** con la categoría y la tarifa usadas ([`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md)); el estimado de combustible; la disponibilidad orientativa de vehículos del tipo requerido; y si el solicitante tiene misiones anteriores sin liquidar — `PC-15`.
4. El sistema declara la **antigüedad del espejo de `ACT-16`** con que se resolvió la competencia del autorizador. Si supera el umbral, advierte y registra la advertencia en el diario antes de permitir continuar ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md), mitigación 5 de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)).
5. `ACT-03` se pronuncia sobre **la procedencia de la necesidad**: si el traslado corresponde a la función institucional, si la fecha es razonable, si el gasto se justifica. **No decide vehículo ni motorista**: eso es de `ACT-04` en [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md).
6. `ACT-03` autoriza. **Antes de registrar nada**, el sistema verifica `BD-01` **segregación entre solicitante y autorizador** — `PC-01`, incompatibilidad `I-01`, [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md). La comparación es por **identidad de persona**, no por identificador de usuario: un mismo servidor con dos cuentas sigue siendo la misma persona.
7. El sistema verifica que el actor sea **autorizador competente** sobre la dependencia y el tipo de misión, resuelto contra el espejo de `ACT-16` — `PC-02`. `[C]` Los umbrales de escalamiento por monto, destino, duración o tipo de recurso movilizado son propiedad de `ACT-16` — insumo #16. **No se cablea ninguno.**
8. El sistema ejecuta **`T-05` autorizar** y registra la autorización con identidad, puesto, **rol ejercido en ese momento**, marca de tiempo del hecho y de captura, dispositivo y **huella del contenido autorizado** — `PC-16`, [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md). No hay firma electrónica certificada: la autorización es interna, con registro completo (`DP-001` D-04).
9. El expediente pasa a `APROBADA`. **Aprobar no compromete flota**: no se reserva vehículo ni motorista (`INV-11`). El expediente entra en la cola de programación de `ACT-04`.
10. El sistema calcula la **fecha de caducidad de la aprobación**: si no se programa antes del inicio de la ventana solicitada, caduca y `ACT-04` deberá anularla con motivo tipificado (`T-09`).
11. El sistema notifica a `ACT-02` y, si la ventana toca franja inhábil, deja visible en la bandeja de Transporte la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` que dispara [CU-03](CU-03-permiso-de-circulacion-en-dia-inhabil.md).

## Flujos alternos

**A1 — Autorización de varios niveles** (desde el paso 8)

1. Una misión puede requerir **una o varias** autorizaciones según monto, destino, duración o tipo de recurso movilizado. La matriz es propiedad de `ACT-16`.
2. Cada autorización parcial se registra como transición de tipo *autorización de nivel N* en el diario, con su actor, **sin cambiar el estado**. No se inventan estados intermedios por nivel.
3. El expediente **permanece en `SOLICITADA` hasta que se registra la última autorización requerida**; solo entonces pasa a `APROBADA`.
4. `BD-01` se evalúa **en cada nivel**, no solo en el primero.
5. `[C]` Esquema exacto de niveles y sus disparadores — insumo #16.

**A2 — Devolución para corrección** (desde el paso 5)

1. `ACT-03` ejecuta `T-04` con motivo obligatorio y visible para `ACT-02`, en vez de rechazar por un dato mal escrito.
2. El expediente vuelve a `BORRADOR` incrementando su versión y conservando la anterior íntegra; conserva el número de expediente; los estimados se anulan y se recalculan al reenviar.
3. Si ya había autorizaciones parciales de nivel, devolver **las invalida todas** y así se advierte antes de confirmar.
4. Existe para que la observación menor no obligue a rechazar: un histórico lleno de rechazos por errores de digitación esconde los rechazos reales.

**A3 — Rechazo** (desde el paso 5)

1. `ACT-03` ejecuta `T-06` con **motivo obligatorio del catálogo configurable más texto libre** — `INV-39`.
2. El expediente pasa a `RECHAZADA`, **estado terminal**. Se notifica a `ACT-02` y se libera el número de expediente sin reciclarlo.
3. `ACT-02` **no reabre el rechazo**: crea una solicitud nueva, y el sistema ofrece hacerlo *a partir de* la rechazada, **conservando el vínculo entre ambas**. Dos expedientes vinculados dejan un rastro legible; un expediente reabierto hasta que pasa, no.
4. `BD-01` se evalúa también aquí: rechazar es un acto de autoridad sobre el expediente y no lo puede ejercer el solicitante.

**A4 — Autorización por delegación de firma** (desde el paso 1)

1. El autorizador titular tiene una **delegación de autorización vigente y acotada** a favor de otro servidor ([`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md)).
2. El delegado ve la bandeja del titular, marcada como tal.
3. La autorización se registra indicando que se actuó **por delegación**, con el folio del acto que la confiere y su vigencia — `PC-16`.
4. **La delegación no rompe la segregación**: si el delegado es el solicitante, `BD-01` bloquea igual.

**A5 — Autorización sin conectividad, en delegación** (desde el paso 6)

1. `T-05` y `T-06` se ejecutan **con código de autorización fuera de línea** (§6.6 de la máquina de estados), no en modo libre.
2. El código lo genera el autorizador competente **en la sede**, sobre una transición concreta de una misión concreta, con ventana de validez corta, y se transmite por el canal que haya —llamada, radio, mensaje—. El canal no forma parte del sistema.
3. El dispositivo verifica el código sin conectividad y solo lo acepta para esa misión y esa transición. Es de un solo uso y no transferible.
4. Queda registrado quién lo generó, para qué, cuándo, quién lo usó y en qué dispositivo.
5. Este es el mecanismo con que se resuelve la falta de personal en delegaciones: **escalamiento a sede**, no levantamiento de incompatibilidades — [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md).
6. `[C]` Canal operativo real, longitud del código y ventana de validez — insumo #1; habilitación del modo delegación desconectada — insumo #41.

## Flujos de excepción

**E1 — El autorizador es el solicitante** (en el paso 6) — **bloqueo duro**

1. `BD-01` impide la transición. **No hay confirmación con advertencia, no hay "autorizar de todos modos", no hay excepción configurable** ([`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), `PC-01`, `I-01`).
2. El sistema registra el intento en la pista de auditoría **con el par de incompatibilidad detectado**, la identidad, el momento y el expediente.
3. El expediente **escala automáticamente al nivel inmediato superior** de la cadena y se le notifica, con el asiento *"Escalado a &lt;cargo&gt; por coincidencia entre solicitante y autorizador (`RN-02`)"*. El escalamiento es **visible en el expediente y en el documento impreso**: quien reciba la orden en carretera debe poder ver por qué firmó quien firmó.
4. El escalamiento **no altera** los niveles de monto ni de alcance de `ACT-16`: el nivel superior autoriza con sus propias facultades.
5. Si la cadena se agota sin autorizador válido, el sistema bloquea y muestra la ruta evaluada completa, para que se corrija la configuración y no el expediente.
6. **El régimen de excepción para delegaciones con personal insuficiente no existe**: quedó suspendido y no se implementa por [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md). La vía es el escalamiento a sede, con el código fuera de línea si no hay red (A5).

**E2 — Quien solicita es la máxima autoridad** (en el paso 7)

1. No hay nivel superior en la cadena. `ACT-09` no puede autorizarse a sí misma sin romper `I-01`.
2. Hasta que la institución lo defina, el sistema **trata el expediente como cualquier otro y escala**: exige registrar el **fundamento documental** del acto y marca la orden para revisión de `ACT-12`.
3. `[C]` Quién autoriza la misión de la máxima autoridad — insumo #28, pendiente B de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md). **No se inventa.**

**E3 — Conflicto de interés por parentesco o subordinación** (en el paso 5)

1. SIGTI **no conoce** los vínculos de parentesco y no los detecta automáticamente.
2. Se resuelve con **declaración de excusa del autorizador**, que el sistema registra y que dispara el escalamiento por la misma vía de `RN-02`.
3. `[C]` Si la institución tiene régimen formal de excusa — insumo #30.

**E4 — La ventana toca día u hora inhábil** (en el paso 3)

1. El sistema advierte con los tramos señalados. **La autorización de `ACT-03` es válida y no se bloquea.**
2. Al aprobar, el expediente queda con la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD`, que **no se puede retirar a mano**: se extingue solo cuando existe el permiso vigente que la cubre, o cuando la ventana se reprograma a franja hábil.
3. La marca **dispara** el expediente de permiso de circulación ante `ACT-09` — [CU-03](CU-03-permiso-de-circulacion-en-dia-inhabil.md). El permiso **no exige que la misión esté programada**: basta con que esté aprobada.
4. El bloqueo real está en `T-12` despachar (`BD-04`, `PC-03`). Bloquear la aprobación produciría el deadlock corregido por `HB1-08` en [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md).

**E5 — El solicitante tiene misiones anteriores sin liquidar** (en el paso 3)

1. El sistema muestra el detalle de lo pendiente con su antigüedad — `PC-15`.
2. El comportamiento es **configurable por la institución**: advertencia o bloqueo. Si es advertencia y `ACT-03` continúa, la decisión queda registrada **con la advertencia visible en el expediente y el nombre de quien continuó**. Una advertencia que nadie ve no es un control.
3. `[C]` Si bloquea o advierte, y el plazo máximo de liquidación — insumos #1 y #32.

**E6 — La solicitud llega marcada `EMERGENCIA` con fecha de salida en el pasado** (desde el paso 1)

1. El sistema la presenta como **convalidación posterior, no como autorización previa**, y así se rotula en la pantalla y en el expediente.
2. La convalidación exige motivo, causal clasificada y se registra con la **cronología real**: no se ajusta ninguna fecha ([`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).
3. La misión **no puede pasar a `CERRADA` hasta ser convalidada** — `PC-18`; vencido el plazo cierra como `CERRADA_CON_HALLAZGO`, nunca en silencio.
4. `ACT-08` y `ACT-12` reciben notificación en la primera sincronización. El sistema **mide la frecuencia de esta vía por dependencia** y la expone en el reporte de control interno: si la emergencia se vuelve la forma normal de saltarse a `ACT-03`, el control desaparece.
5. `[C]` Qué puesto convalida y en qué plazo — insumo #32. Ver [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md).

**E7 — El expediente se anula antes de resolverse** (desde cualquier paso)

1. `ACT-02` desiste o `ACT-08` anula administrativamente mediante `T-07`, con motivo obligatorio.
2. Si hay autorizaciones de nivel en curso de firma, se notifica a esos autorizadores.
3. El expediente se retira de todas las bandejas. `RECHAZADA` y `ANULADA` son terminales: no se reabren.

## Reglas aplicables

| Regla | Qué gobierna en este caso de uso |
|---|---|
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | Quien solicita no autoriza. **Bloqueo duro, no desactivable** — `BD-01`, `PC-01`, `I-01` |
| [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) | Escalamiento al nivel inmediato superior cuando el autorizador natural es el solicitante |
| [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) | Identidad, puesto, rol ejercido, momento, origen y huella del contenido autorizado — `PC-16` |
| [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) | Una autorización registrada no se borra; se revierte por `T-09` |
| [`RN-06`](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) | Solo las transiciones definidas, cada una con actor, rol, momento y motivo |
| [`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) | Delegación con vigencia acotada, que consta en el expediente y **no rompe la segregación** |
| [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) | Marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD`; **la aprobación no se bloquea** |
| [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) | El estimado desglosado por punto se pone a la vista de quien autoriza |
| [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) | Degradación explícita si el espejo de `ACT-16` lleva detenido más del umbral |
| [`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) | El compromiso se valida contra la **cuota trimestral**, no solo contra el presupuesto anual `[C]` insumo #16 |
| [`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) | Convalidación en plazo del acto sin autorización previa |

## Notas de hallazgo

**`HCU-01` (reiterado desde [CU-01](CU-01-registrar-solicitud-de-transporte.md)) — `BD-01` no alcanza al solicitante de derecho.** La precondición está redactada contra *quien creó* y *quien envió* el expediente. Cuando una asistente captura por encargo de su jefe y el jefe autoriza, `BD-01` leída literalmente **no bloquea**, aunque `I-01` sí se viola. Este caso de uso ejecuta la verificación sobre **el solicitante de derecho, el capturador y el remitente**, los tres, porque es lo que `RN-01` e `I-01` exigen. Se reporta contra la máquina de estados, que es la autoridad y es la que debe corregirse.

**`HCU-02` — `PC-01` habla de "puesto superior" y `RN-02` de "nivel inmediato superior de la cadena de autorización".** No son necesariamente lo mismo: la cadena de `ACT-16` puede no coincidir con la línea jerárquica de puestos. Este caso de uso sigue a [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) —la cadena espejada de `ACT-16`— porque la determinación del nivel competente es materia de `ACT-16` por `DP-001` D-05. Se deja señalado contra `PR-01` `PC-01`, que no es autoridad en la materia.

## Trazabilidad

- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapa E3; puntos de control `PC-01`, `PC-02`, `PC-15`, `PC-16`, `PC-18`
- **Transiciones**: `T-04`, `T-05`, `T-06`, `T-07`, y `T-09` como reverso de la aprobación — [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)
- **Invariantes**: `INV-09`, `INV-10`, `INV-11`, `INV-39`
- **Bloqueos**: `BD-01` segregación entre solicitante y autorizador
- **Actores e incompatibilidades**: [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) — `ACT-02`, `ACT-03`, `ACT-04`, `ACT-08`, `ACT-09`, `ACT-12`, `ACT-16`; `I-01`, y el núcleo irreductible `I-07`, `I-10`, `I-11` que ningún régimen levanta
- **Casos especiales**: [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) (E6). **Descartados explícitamente:** [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md), [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md), [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md), [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md), [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — la aprobación **no reserva recursos** (`INV-11`), de modo que ni la flota ni el fondo condicionan este acto; todos se materializan en [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md)
- **Normativa**: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) `[P]` segregación de funciones incompatibles y autorización por servidor competente · [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` día u hora inhábil · [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) autorización interna sin firma electrónica certificada
- **Decisiones**: [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-04, D-05 · [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md) — **régimen de excepción suspendido** · [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- **Insumos pendientes**: #1, #16, #26, #27, #28, #30, #32, #41 en [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md)
- **Aguas arriba**: [CU-01](CU-01-registrar-solicitud-de-transporte.md) · **Aguas abajo**: [CU-03](CU-03-permiso-de-circulacion-en-dia-inhabil.md), [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md)
