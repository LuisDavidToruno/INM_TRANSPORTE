# CU-01 — Registrar una solicitud de transporte

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor principal** | `ACT-02` Solicitante |
| **Actores secundarios** | `ACT-03` Jefatura Inmediata (destinataria), `ACT-10` Encargado de Delegación (captura y digitación diferida en ámbito territorial), `ACT-16` Sistema ARGOS (espejo de la cadena de autorización y clave de vínculo de viáticos), `ACT-17` Sistema de Talento Humano (espejo del calendario de feriados) |
| **Precondiciones** | 1. El actor tiene rol vigente con permiso de solicitar sobre al menos una dependencia — precondición de `T-01`. 2. Existen, vigentes **a la fecha prevista de salida**, el catálogo de motivos de viaje, el de tipos de vehículo y la matriz de compatibilidad de M-02 ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)). 3. El calendario de días hábiles y el horario hábil de la delegación están cargados para la ventana solicitada `[C]` insumo #32. 4. El catálogo de puntos de peaje y su tabla de tarifas están sincronizados; si la antigüedad supera el umbral, el sistema lo declara antes de mostrar cualquier estimado ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)) |
| **Postcondiciones** | El expediente está en `SOLICITADA` y se verifican `INV-05` a `INV-08`: contenido sustantivo congelado con su huella; número de expediente institucional correlativo por delegación y año, sin reciclado; estimado de peajes desglosado por punto congelado con el identificador de la tabla usada; **ninguna reserva de vehículo ni de motorista**. Si la ventana toca franja inhábil, el expediente lleva la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD`. El expediente está en la bandeja del autorizador competente resuelto contra el espejo de `ACT-16` |
| **Disparador** | Una dependencia necesita movilizar un recurso institucional — personal, personas externas, carga o combinación (`PR-01` E1) |

## Flujo principal

1. `ACT-02` abre una nueva solicitud. El sistema ejecuta **`T-01` crear solicitud** y el expediente queda en `BORRADOR`. El identificador se **genera en el cliente**, no en el servidor ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)), y el paso completo funciona sin ninguna conectividad ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)). Rigen `INV-01` a `INV-04`: sin folio, sin vehículo ni motorista vinculados, sin reserva de ventana, visible solo para su creador.
2. El sistema pregunta **qué se traslada** antes que ninguna otra cosa: personal de la institución, personas externas, carga, o mixto. Es el dato que determina el tipo de vehículo compatible, los documentos a emitir y las validaciones que se aplicarán (premisa rectora 1 y 2 de `CLAUDE.md`; [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md)).
3. `ACT-02` detalla el objeto declarado: cantidad de pasajeros, o naturaleza, peso en kilogramos, volumen, embalaje, remitente y consignatario de la carga; o ambos conjuntos si el traslado es mixto (`PR-01` V-01, V-02).
4. `ACT-02` registra origen, uno o más destinos **con su orden previsto** y la permanencia estimada en cada uno (`PR-01` V-04), y las paradas previstas.
5. `ACT-02` registra la ventana solicitada con fecha y hora de salida y de retorno, el motivo del catálogo, la dependencia solicitante, el tipo de vehículo requerido, si existe viático asociado en `ACT-16` —solo la clave de vínculo, SIGTI no lo calcula ni lo liquida— y si se trata de una emergencia.
6. El sistema evalúa la **compatibilidad entre lo declarado y el tipo de vehículo requerido** — `BD-09` —, contra la matriz de M-02 y no contra el criterio del solicitante ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md), [`RN-67`](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md)).
7. El sistema resuelve si la ventana cae, total o parcialmente, en **día inhábil, feriado u hora inhábil**, contra el calendario vigente **a la fecha prevista de salida** y no a la de hoy ([`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)). Señala los tramos inhábiles y **avisa**; no bloquea ([`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md)).
8. El sistema **estima el costo de peajes** de la ruta: punto por punto, con la categoría que corresponde al tipo de vehículo requerido y la tarifa vigente a la fecha prevista de cada paso ([`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md)). Se presenta **desglosado, nunca como total opaco**: quien autoriza tiene que poder verificar el cálculo.
9. El sistema estima el consumo de combustible según la distancia prevista y el rendimiento esperado del tipo de vehículo, y muestra si hay vehículos de ese tipo con disponibilidad en la ventana. **Mostrar disponibilidad no la reserva** — `INV-08`, `INV-11`.
10. `ACT-02` envía la solicitud. El sistema ejecuta **`T-02` enviar a autorización** verificando el contenido mínimo completo del punto 5, `BD-09`, y los requisitos de manifiesto de M-17 si el traslado es de personas externas.
11. El sistema aplica los efectos de `T-02`: asigna el **número de expediente institucional**, correlativo por delegación y año sin reciclado; congela el contenido sustantivo y calcula su huella —quien autorice después autorizará ese contenido concreto ([`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md))—; congela el estimado de peajes con el identificador de la tabla usada (`INV-07`); deja la marca de franja inhábil si corresponde; y registra la clave de vínculo con `ACT-16` si la hay.
12. El sistema resuelve la **cadena de autorización** contra el espejo de `ACT-16` y encamina el expediente a la bandeja del autorizador competente. Si el primer autorizador coincide con el solicitante de derecho, avanza al siguiente nivel y lo deja asentado ([`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md)) — continúa en [CU-02](CU-02-autorizar-solicitud-de-transporte.md).
13. El expediente queda en `SOLICITADA`. `ACT-02` ya no puede editar el contenido sustantivo: para corregir hay que devolver a `BORRADOR` por `T-04`, lo que incrementa la versión.

## Flujos alternos

**A1 — Captura por encargo de la jefatura** (desde el paso 1)

1. Quien opera el sistema es la asistente o la secretaria de la unidad, que captura por encargo del servidor que requiere la movilización (`PR-01` E1 `[I]`).
2. El sistema registra **dos campos distintos**: el **capturador** y el **solicitante de derecho**, que es el servidor que requiere la movilización ([`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md), condiciones de no aplicación).
3. La segregación de `CU-02` y el escalamiento de `RN-02` se evalúan sobre el **solicitante de derecho**. Ver la nota de hallazgo `HCU-01` al final: `BD-01` está redactada contra el creador y el remitente del expediente, no contra el solicitante de derecho.

**A2 — Captura en delegación sin conectividad** (desde el paso 1)

1. `T-01` y `T-02` se ejecutan sin red. El número de expediente se toma del **rango de la delegación** ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)).
2. Los estimados de peaje y combustible se calculan con la tabla de parámetros sincronizada localmente. Si su antigüedad supera el umbral configurable, el sistema **lo advierte antes de mostrar el número** y deja la advertencia en el diario ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)).
3. El expediente se encola y sincroniza con **fecha del hecho distinta de la fecha de captura** ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
4. Si no hubo dispositivo y la solicitud se llenó en el formato en papel, `ACT-10` la digita después con constancia de quién digitó, cuándo, y el original escaneado adjunto ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)). Ver [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md).

**A3 — Devolución para corrección** (desde el paso 13)

1. `ACT-03` ejecuta `T-04` con motivo obligatorio y visible para el solicitante.
2. El expediente vuelve a `BORRADOR`, **incrementa su versión** y conserva íntegra la anterior. **Conserva el mismo número de expediente**: es el mismo expediente en su versión 2, no uno nuevo.
3. Los estimados congelados se anulan y se recalculan al reenviar. Si ya existían autorizaciones parciales de nivel, devolver **las invalida todas** y así se advierte.

**A4 — Traslado de personas externas** (desde el paso 3)

1. `ACT-02` identifica a las personas externas con **minimización de datos**: solo los del catálogo autorizado ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)).
2. El cumplimiento de los requisitos de manifiesto de M-17 es **precondición de `T-02`**: sin ellos la solicitud no se envía.
3. Toda consulta posterior a ese dato queda registrada, incluidas las de `ACT-12` ([`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)).
4. `[C]` Los requisitos documentales concretos dependen del tipo de institución y de la naturaleza del traslado — insumos #1 y #39. **No se inventan.**

**A5 — Solicitud que no cumple la antelación mínima** (en el paso 10)

1. El sistema marca la solicitud como **urgente** y no la bloquea.
2. Su autorización exige el nivel adicional que defina la institución.
3. `[C]` Antelación mínima y nivel requerido para urgencia — insumo #32. **No se cablea ningún valor.**

**A6 — Descarte del borrador** (desde cualquier paso anterior al 10)

1. `ACT-02` ejecuta `T-03` con motivo obligatorio. El expediente pasa a `ANULADA` marcado como descarte previo al circuito de control.
2. **No es un asiento reverso** porque no hubo transacción que revertir, y **tampoco es un borrado físico**: queda fuera de los paquetes de evidencia de auditoría, marcado como tal ([`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)).

## Flujos de excepción

**E1 — El tipo de vehículo requerido no puede mover lo declarado** (en el paso 6)

1. `BD-09` impide el envío. El sistema no ofrece la acción de enviar.
2. Muestra qué atributo falla —plazas, peso, volumen o naturaleza— y qué tipos de vehículo del catálogo sí lo cubren.
3. `ACT-02` corrige el tipo requerido o el detalle del objeto y reintenta.

**E2 — Objeto mixto con incompatibilidad entre lo que se traslada** (en el paso 6)

1. El sistema evalúa la matriz de compatibilidad **objeto × objeto**, par a par ([`RN-67`](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md)). El ejemplo canónico es personas junto a bidones de combustible.
2. **La ausencia de entrada en la matriz bloquea**: no se interpreta como compatible.
3. `ACT-02` separa el traslado en dos solicitudes, o declara la configuración por tramo ([`RN-68`](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md)). Ver [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md).

**E3 — La ventana toca día u hora inhábil** (en el paso 7)

1. El sistema señala los tramos inhábiles y advierte que la circulación requerirá **permiso de la máxima autoridad**.
2. **No bloquea la captura ni el envío, y no bloqueará la aprobación.** El bloqueo está en el despacho, `BD-04`, evaluado en `T-12`. Bloquear antes produce un deadlock cuya única salida es que el usuario declare una fecha falsa — corrección `HB1-08` de [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md).
3. Al aprobarse, el expediente quedará con la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD`, que **no se puede retirar a mano**, y disparará [CU-03](CU-03-permiso-de-circulacion-en-dia-inhabil.md).

**E4 — La salida ya ocurrió: emergencia por convalidar** (desde el paso 1)

1. `ACT-02` —típicamente `ACT-10` en delegación y sin señal— registra la solicitud con marca `EMERGENCIA`, causal clasificada del catálogo, motivo obligatorio y **fecha de salida en el pasado**.
2. El sistema **no la bloquea**, y **no la presenta como autorización previa sino como convalidación posterior** ([`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)). La cronología se declara tal como ocurrió: no se ajusta ninguna fecha para que el expediente "cuadre".
3. La misión **no puede pasar a `CERRADA` hasta que se convalide** — `PC-18`. Vencido el plazo, cierra como `CERRADA_CON_HALLAZGO`.
4. La emergencia **no levanta el núcleo irreductible**: `I-07`, `I-10`, `I-11` siguen en pie.
5. `[C]` Qué puesto convalida y en qué plazo máximo — insumo #32. Ver [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md).

**E5 — La tabla de tarifas de peaje no es confiable o falta un punto** (en el paso 8)

1. Si la sincronización de la tabla supera el umbral, el sistema **declara la antigüedad antes de mostrar el estimado** ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)).
2. Si un punto de la ruta no tiene tarifa cargada, el estimado se presenta con ese punto rotulado **sin tarifa disponible** y el total marcado como incompleto. No se sustituye por una cifra inventada.
3. La solicitud puede enviarse igual: el estimado es insumo del autorizador, no requisito de existencia del expediente.
4. `[C]` Tarifa efectivamente vigente — insumo #21; lista oficial de exoneraciones — insumo #22.

**E6 — El vehículo que se va a usar está asignado a un funcionario** (desde el paso 1)

1. El uso de un vehículo del Estado bajo cualquier régimen —incluido el de asignación permanente a un funcionario— **se ampara igualmente en una Orden de Misión**, y por lo tanto empieza con esta solicitud ([`RN-59`](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md), [`RN-58`](../../01-negocio/reglas/RN-58-regimen-de-uso-del-vehiculo.md)).
2. El régimen de uso se declara en la solicitud porque condiciona la verificación de habilitación de quien conducirá ([`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md)) en [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md). Ver [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md).

**E7 — La cadena de autorización no se puede resolver** (en el paso 12)

1. Si la cadena se agota sin encontrar autorizador válido —dependencia unipersonal, o toda la cadena en la misma persona—, el sistema **bloquea y muestra la ruta evaluada completa**, para que el problema se resuelva en la configuración y no en el expediente ([`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md)).
2. Si el espejo de `ACT-16` lleva detenido más del umbral, aplica [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md): se advierte y, superado el umbral de bloqueo, no se autoriza contra una jerarquía que ya no existe.
3. `[C]` Autorizador alterno por dependencia y por delegación — insumo #28.

## Reglas aplicables

| Regla | Qué gobierna en este caso de uso |
|---|---|
| [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) | Resolución de la cadena de autorización y distinción capturador ≠ solicitante de derecho |
| [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) | Huella del contenido congelado al enviar |
| [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) | El descarte no borra |
| [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) | Compatibilidad y capacidad del tipo de vehículo requerido — `BD-09` |
| [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) | Detección de franja inhábil y marca; **el bloqueo no está aquí** |
| [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) | Estimado de peajes desglosado, por punto y categoría, a la fecha del hecho |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) | Calendario, tarifas y umbrales como parámetros con vigencia; congelamiento |
| [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) | Captura sin conectividad, identificadores en el cliente, digitación diferida |
| [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) | Degradación explícita cuando el espejo o la tabla están desactualizados |
| [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) | Personas externas: minimización y registro de consultas |
| [`RN-58`](../../01-negocio/reglas/RN-58-regimen-de-uso-del-vehiculo.md), [`RN-59`](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) | Todo uso se ampara en Orden de Misión, cualquiera sea el régimen |
| [`RN-67`](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md), [`RN-68`](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md) | Compatibilidad objeto × objeto y por tramo |
| [`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) | Emergencia registrada como convalidación, con la cronología real |

## Notas de hallazgo

**`HCU-01` — `BD-01` no alcanza al solicitante de derecho.** [`BD-01`](../../03-arquitectura/estados/orden-de-mision.md) define la segregación como *"la persona que ejecuta la autorización no puede ser la persona que creó la solicitud (`T-01`) ni quien la envió (`T-02`)"*. Pero [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) establece que cuando alguien captura por encargo, **el solicitante de derecho es el servidor que requiere la movilización**, no el capturador. En el escenario más frecuente de la operación real —la asistente captura para su jefe y el jefe autoriza— `BD-01`, leída literalmente, **no bloquea**: el jefe ni creó ni envió el expediente. La incompatibilidad `I-01` sí se viola.

No se resuelve aquí: la máquina de estados es la autoridad en precondiciones. Se reporta para que `BD-01` compare la identidad del autorizador contra **el solicitante de derecho, el capturador y el remitente**, los tres.

## Trazabilidad

- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E1 y E2; variantes V-01, V-02, V-03, V-04, V-05
- **Transiciones**: [`T-01`](../../03-arquitectura/estados/orden-de-mision.md), `T-02`, `T-03`, `T-04` — máquina de estados de la Orden de Misión
- **Invariantes**: `INV-01` a `INV-08`, `INV-11`
- **Bloqueos**: `BD-09` compatibilidad entre lo solicitado y el tipo de vehículo
- **Puntos de control**: ninguno de `PR-01` §4 se ejecuta en esta etapa. `PC-16` —registro de persona, puesto, rol, momento, origen y huella— aplica al envío como acto registrable
- **Actores**: [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) — `ACT-02`, `ACT-03`, `ACT-10`, `ACT-16`, `ACT-17`; incompatibilidad `I-01` evaluada en [CU-02](CU-02-autorizar-solicitud-de-transporte.md)
- **Casos especiales**: [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) (E4), [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) (A2), [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) (E2), [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) (E6). **Descartados explícitamente:** [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md), [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md), [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md), [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md), [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — todos se materializan al asignar recursos, y en `SOLICITADA` no hay recursos asignados (`INV-08`)
- **Normativa**: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) `[P]` autorización por servidor competente · [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` circulación en día u hora inhábil, con el eslabón débil declarado en `RN-23` · [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) minimización y registro de consultas · [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` operación sin conectividad · [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) `[P]` tarifas por punto y categoría
- **Decisiones**: [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-01, D-02, D-05 · [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- **Insumos pendientes**: #1, #2, #21, #22, #28, #32, #39 en [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md)
- **Aguas abajo**: [CU-02](CU-02-autorizar-solicitud-de-transporte.md), [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md); historias `HU-xxx` de M-06
