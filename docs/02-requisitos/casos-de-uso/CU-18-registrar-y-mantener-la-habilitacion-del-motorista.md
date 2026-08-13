# CU-18 — Registrar y mantener la habilitación del motorista

| Campo | Valor |
|---|---|
| **Módulo** | M-05 Motoristas y Habilitación (con M-02 catálogo de categorías y matriz, M-20 espejo de Talento Humano) |
| **Proceso** | [`PR-03`](../../01-negocio/mapa-de-procesos.md) — Habilitación del motorista, habilitante de `PR-01` |
| **Actor principal** | `ACT-04` Jefe de Transporte — habilita e inhabilita en el padrón (acción 24 de la matriz de permisos) |
| **Actores secundarios** | `ACT-10` Encargado de Delegación (propone y digita en territorio, no consuma la habilitación) · `ACT-06` Motorista (aporta el documento físico) · `ACT-17` Sistema de Talento Humano (identidad, puesto, permisos, vacaciones e incapacidades, por espejo) · `ACT-01` Administrador (carga la matriz licencia↔vehículo con vigencia) · `ACT-08` Gerencia Administrativa (aprueba la puesta en vigencia de la matriz) · `ACT-12` Auditor Interno (consulta) |
| **Precondiciones** | La persona existe en el **espejo de Talento Humano**: SIGTI no crea personas, las espeja ([`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)). Existe una versión **vigente y aprobada** del catálogo de categorías de licencia y de la matriz licencia↔vehículo ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), doble control de `PR-09`). El actor tiene rol vigente y alcance sobre la dependencia o delegación del servidor. |
| **Postcondiciones** | El servidor queda **`HABILITADO`** para un conjunto determinado de tipos de vehículo, con **fecha de fin de habilitación explícita**, o **`NO HABILITADO`** con causa tipificada. La evaluación queda congelada con la versión de la matriz utilizada. Las alertas de vencimiento quedan programadas **por categoría** y dirigidas al puesto. Todo queda con autor, puesto, momento y huella. |
| **Disparador** | Un servidor con funciones de conducción se incorpora; o su licencia se renueva, se amplía, se vence, se suspende o cambian sus restricciones médicas; o se necesita habilitar a quien conduce sin ser motorista de padrón. |

**Por qué esto no es un CRUD.** Este caso de uso es la **fuente única del dato que sostiene el control de mayor valor legal del sistema**. `BD-02` y `PC-04` bloquean la programación y el despacho contra la licencia, sin excepción configurable, porque *"nos tenemos que proteger con la ley también"* ([DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)) y porque una excepción registrada sería evidencia en contra ante un siniestro. **Sin este caso de uso, `BD-02` no tiene de dónde leer.**

> **La licencia es dato propio de SIGTI, no espejo de Talento Humano.** Corrección incorporada en [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md): un control de esta criticidad legal no puede depender del modelo de datos de un sistema ajeno que no tiene motivo para mantenerlo. **Alguien de la institución tiene que capturarlo y mantenerlo dentro de SIGTI**, con su alerta de vencimiento. Es trabajo adicional real y hay que decirlo de frente: es el precio de que el bloqueo sea defendible. `[C]` Si el contrato de API de Talento Humano (insumo #17) resulta mantener la categoría con el detalle requerido, se reconsidera; hasta entonces, es propio.

## Flujo principal — alta de la habilitación

1. `ACT-04` abre el padrón de motoristas y **busca a la persona en el espejo** de Talento Humano por identidad. Si no existe en el espejo, el caso de uso no continúa: SIGTI no crea personas.
2. El sistema muestra, en modo lectura, el dato espejado: identidad, puesto vigente, dependencia, y la **marca de última sincronización** de la entidad ([`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md)).
3. `ACT-04` captura la **licencia de conducir** como dato propio: número, autoridad emisora, fecha de emisión, **cada categoría con su propia fecha de vencimiento**, restricciones médicas del catálogo, y **fotografía o escaneo del documento físico** — el servidor presenta la licencia; no se captura de memoria.
4. El sistema valida cada categoría contra el **catálogo de categorías vigente a la fecha del hecho** ([`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)). Las ocho categorías conocidas son `A`, `B`, `B1`, `C1`, `C`, `D1`, `D` y `CE` — [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[V]` por fuentes concordantes, `[C]` contraste con el texto oficial y con la reforma al Art. 48 (2025).
5. El sistema **deriva el conjunto de tipos de vehículo habilitados** cruzando las categorías vigentes contra la **matriz licencia↔vehículo** vigente, resuelta por los atributos de la ficha técnica —tipo, peso bruto vehicular en kg, capacidad de pasajeros y condición de articulado—, **nunca por el nombre comercial del modelo** ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md)).
6. El sistema muestra el resultado en el lenguaje del despacho: **qué vehículos concretos de la flota** puede conducir esta persona y cuáles no, con el atributo que excluye a cada uno. Es lo que evita que el despacho trabaje por ensayo y error.
7. `ACT-04` registra las **capacitaciones** y habilitaciones internas adicionales que la institución exija, con su vigencia.
8. `ACT-04` **habilita** al servidor en el padrón. El sistema calcula la **vigencia de la habilitación** como la menor fecha de vencimiento entre las categorías que la sostienen, y la muestra explícitamente junto al estado.
9. El sistema programa las **alertas anticipadas por categoría** —no por licencia— con umbrales configurables, valor de referencia 60 / 30 / 15 días `[C]`, dirigidas al **puesto** y no a la persona, porque la rotación es alta y una alerta a quien ya no está en el cargo no llega a nadie ([`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md)).
10. El sistema registra el acto con identidad, puesto, rol ejercido, marca de tiempo y huella del contenido ([`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md)), y **congela** la evaluación con el identificador de la versión de la matriz usada ([`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)).
11. Desde ese momento el servidor es asignable en `CU-04`. `BD-02` y `PC-04` se evalúan contra este expediente **al programar y otra vez al despachar**: entre una cosa y la otra pueden pasar días y una licencia puede haber vencido.

> Lo que se guarda no es "verificado": se guarda **el resultado con todos sus insumos** — número de licencia, categoría, vencimiento consultado, versión de la matriz, atributos del vehículo usados y fecha de fin de rango evaluada. Guardar solo un sí o un no no defiende a nadie ante un siniestro.

## Flujos alternos

**A1 — Renovación de la licencia** (desde el paso 3)

1. `ACT-04` registra la licencia renovada con su adjunto y su nuevo rango de vigencia.
2. El sistema **cierra el rango anterior y abre uno nuevo**; **no sobrescribe** el vencimiento ([`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)). El expediente debe poder decir qué licencia amparaba una misión de hace ocho meses.
3. El sistema recalcula la vigencia de la habilitación y **lista las misiones ya programadas** que el nuevo rango desbloquea o sigue bloqueando.
4. **No basta la promesa de renovación**: mientras el dato renovado no conste con adjunto, el bloqueo sigue vigente ([`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md)).

**A2 — Cambio del conjunto de categorías** (desde el paso 5)

1. El servidor obtiene una categoría nueva, o pierde una porque venció mientras las demás siguen vigentes.
2. El sistema recalcula el conjunto de vehículos habilitados y produce una **habilitación parcial**: perder la categoría `C` y conservar la `B` no es perder la habilitación, es perder parte de ella.
3. El sistema lista las **misiones programadas que dependían de la categoría perdida** para que `ACT-04` las reasigne antes de que el bloqueo aparezca en el despacho.

**A3 — Registro o cambio de restricciones médicas** (desde el paso 3)

1. `ACT-04` registra las restricciones que constan en la licencia: corrección visual, prohibición de conducción nocturna u otras.
2. El sistema evalúa su compatibilidad con las condiciones de las misiones y **bloquea o advierte según el parámetro institucional** ([`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md)).
3. `[C]` **El catálogo oficial de restricciones de la DNVT no se tiene** — insumo **#42**. El catálogo se entrega vacío y configurable: **no se inventan valores**.

**A4 — Inhabilitación** (desde el paso 8)

1. `ACT-04` inhabilita por causa tipificada: vencimiento no renovado, suspensión derivada de un expediente de M-12, restricción médica incompatible, decisión administrativa o cese del servidor.
2. La inhabilitación exige **motivo tipificado y vigencia** —definitiva o hasta fecha—, y se refleja de inmediato en `BD-10` como *suspensión de habilitación*.
3. El sistema lista las **misiones programadas afectadas** y las encamina a sustitución de motorista ([`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md), `CU-07`), conservando en el historial la asignación original.
4. La inhabilitación **no borra** el expediente: el historial de conducción, incidentes y habilitaciones pasadas se conserva íntegro.

**A5 — Habilitación de quien conduce sin ser motorista de padrón** ([`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md))

1. Se necesita habilitar a un funcionario asignatario, a un servidor de otra dependencia o a un conductor eventual incorporado por incapacidad del motorista ([CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md), [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md)).
2. El sistema exige **exactamente los mismos datos y el mismo rigor** que a un motorista de padrón: identidad, número de licencia, categorías, vencimientos, restricciones y **fotografía de la licencia física**.
3. Evaluadas `RN-09` y `RN-10`, **bloquea igual**. Ningún régimen de uso, jerarquía ni excepción operativa exime de esta verificación.
4. Desde ese momento le aplican las mismas incompatibilidades de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) que a cualquier conductor de misión.
5. `[C]` Si la institución admite la figura de **motorista eventual** —insumo **#48**— y si la póliza cubre a un conductor no registrado como motorista —insumo **#49**—. Mientras no consten, el sistema exige la licencia y bloquea, que es la posición sostenible ante un siniestro.

**A6 — Captura en delegación sin conectividad** (desde el paso 3)

1. `ACT-10` **propone** la habilitación desde la delegación: captura los datos y la fotografía de la licencia sin red ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)).
2. El registro distingue **fecha del hecho** de **fecha de captura**, ambas obligatorias ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)), y adjunta el original digitalizado si vino en papel ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)).
3. La habilitación **la consuma `ACT-04`** al sincronizar: `ACT-10` propone, no habilita (acción 24 de la matriz de permisos). Es una de las funciones que **no requieren presencia física** y por eso salen de la delegación — Nivel 1 de [`actores-y-roles.md` §5.4](../../01-negocio/actores-y-roles.md).
4. Ningún conflicto de sincronización se resuelve por sobrescritura: va a cola de resolución humana ([`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)).

**A7 — Cese del servidor en Talento Humano**

1. El espejo notifica la baja del empleado.
2. La habilitación se cierra con causa *cese*, y el servidor deja de aparecer entre los asignables. Sus registros históricos **no se tocan**.
3. `[C]` **Qué ocurre con un empleado dado de baja que tiene misiones abiertas en SIGTI** — pendiente expreso de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) y de `BD-10`.

## Flujos de excepción

**E1 — La licencia presentada ya está vencida** (en el paso 3)

1. El sistema **registra el dato tal como es** —el hecho no se falsea— y deja al servidor en **`NO HABILITADO`** con causa *licencia vencida*.
2. **Nunca se habilita con licencia vencida**, por ningún rol, ni siquiera `ACT-09`: `RN-09` y `RN-10` no admiten excepción configurable y **no existe pantalla de excepción**.
3. El sistema abre la alerta de vencimiento en estado *vencido*, que permanece hasta que se registre la renovación o la baja del recurso.

**E2 — Categoría que no existe en el catálogo vigente** (en el paso 4)

1. El sistema **bloquea la captura** de la categoría y no la crea al vuelo: un catálogo que se completa escribiendo en el campo deja de ser catálogo.
2. La incorporación de una categoría nueva pasa por `PR-09`: `ACT-01` la carga con su respaldo documental y `ACT-08` aprueba su puesta en vigencia — doble control.
3. `[C]` **Texto de la reforma al Art. 48 (2025)** sobre las categorías `CD` y `CE` — insumos **#20** y **#23**. Es el pendiente más importante de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md): **sin él la matriz definitiva no se puede fijar**.

**E3 — Captura sin adjunto de la licencia física** (en el paso 3)

1. El sistema no consuma la habilitación sin el escaneo o la fotografía del documento. La evidencia es el respaldo del bloqueo: sin ella, ante un siniestro solo queda la palabra de quien capturó.
2. La exigencia del adjunto es **implicación de requerimiento del equipo** derivada de [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[I]`, no articulado citable; por eso el bloqueo es **configurable**, encendido por defecto. `[C]` Confirmar con Auditoría Interna.

**E4 — El propio servidor intenta capturar o modificar su licencia** (en cualquier paso)

1. **Bloqueo.** `ACT-06` no tiene facultad de ejecución sobre la acción 24 *habilitar o inhabilitar motorista en el padrón*, y su alcance de datos es **PROPIO** y de consulta sobre sus misiones ([`actores-y-roles.md` §3.2 y §4](../../01-negocio/actores-y-roles.md)).
2. El motorista **aporta** el documento físico; no registra su propia habilitación. Autohabilitarse es el equivalente, en M-05, de autoliquidarse en M-09.
3. El intento se registra en la pista de auditoría.

**E5 — Quien habilita es la misma persona que va a ser habilitada**

1. Ocurre en delegaciones pequeñas: el mismo servidor ocupa el puesto con rol `ACT-04` y conduce.
2. **Tratamiento provisional:** el sistema permite el acto, lo marca como *acumulación vigilada* en el tablero de `ACT-08` y `ACT-12`, y exige motivo escrito.

> ⚠️ **Nota de hallazgo — no existe par de incompatibilidad que cubra "habilita × es habilitado".** La tabla `I-01` a `I-17` de [`actores-y-roles.md` §5.2](../../01-negocio/actores-y-roles.md) —autoridad en actores e incompatibilidades— cubre las funciones de la misión y el descargo del bien, pero **no la autohabilitación para conducir**. Dado que `RN-09` y `RN-10` son bloqueos sin excepción precisamente porque la responsabilidad se traslada a quien autorizó, quien se habilita a sí mismo se autoriza a sí mismo el control. **Este caso de uso no crea la incompatibilidad**: la eleva como propuesta a la autoridad competente, para que se decida si es bloqueo duro con escalamiento al puesto superior o advertencia con motivo escrito.

**E6 — El espejo de Talento Humano lleva más del umbral sin sincronizar** (en el paso 2)

1. El sistema **degrada explícitamente antes de operar** ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)): muestra la antigüedad del dato y advierte.
2. La licencia **sí se puede capturar y mantener**, porque es dato propio y no depende del espejo. Lo que queda marcado como **no confirmado** es la disponibilidad del servidor —permisos, vacaciones, incapacidades— y la vigencia de su puesto.
3. Esa marca **se imprime en el documento** cuando la evaluación de `BD-02` se haya hecho sobre datos posiblemente desactualizados. `[C]` umbral — insumo **#17**.

**E7 — La licencia vence dentro del rango de misiones ya programadas** (desde el paso 8)

1. Al habilitar o renovar, el sistema **lista las misiones programadas cuya ventana excede la vigencia** y señala la fecha exacta.
2. `RN-10` bloquea la programación y el despacho: una licencia que vence el miércoles no habilita una misión que retorna el viernes. El sistema **propone motoristas alternos habilitados en el mismo acto del bloqueo** — bloquear sin alternativa es lo que empuja a operar fuera del sistema.
3. Si el vencimiento sobreviene con la misión ya `EN_RUTA`, **no se detiene la ejecución**: se rige por [`RN-55`](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) y el expediente **cierra con hallazgo** ([CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md)).

**E8 — La licencia vence exactamente el día de retorno** (en el paso 8)

1. El sistema aplica el parámetro `criterio_vencimiento_licencia`, con valor inicial *fin del día* y **advertencia visible**.
2. `[C]` Confirmar el criterio —inicio o fin del día— contra el texto de la Ley de Tránsito — insumo **#33**. No se cablea ninguna de las dos interpretaciones.

**E9 — Dos servidores con el mismo número de licencia**

1. El sistema **advierte**, indica con quién colisiona y exige verificación contra el documento físico antes de guardar, dejando el motivo registrado.
2. `[C]` Si la institución quiere que sea bloqueo duro en lugar de advertencia. No se decide aquí: casi siempre es error de digitación, y bloquear sin poder corregir deja al motorista fuera del padrón por un dígito.

**E10 — El servidor no aparece en el espejo de Talento Humano** (en el paso 1)

1. El caso de uso **no continúa**: SIGTI no crea personas ([`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)).
2. El sistema genera una **incidencia de espejo** dirigida a `ACT-01` para que se verifique la sincronización o el alta en el sistema origen. La respuesta correcta es corregir el origen, no capturar la persona a mano en SIGTI.

## Reglas aplicables

| Regla | Qué aporta a este caso de uso |
|---|---|
| [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) | La categoría debe habilitar tipo, peso bruto y capacidad del vehículo. **Bloqueo duro sin excepción**; la matriz es catálogo con vigencia |
| [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) | Vigencia durante **todo el rango** de la misión, no solo el día de salida |
| [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) | Compatibilidad de restricciones médicas con las condiciones de la misión |
| [`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) | La habilitación se verifica sobre **quien conduce**, cualquiera sea su puesto |
| [`RN-55`](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) | El vencimiento sobrevenido en ruta no detiene la misión, pero cierra con hallazgo |
| [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) | Disponibilidad —permisos, vacaciones, incapacidad— desde el espejo de Talento Humano |
| [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) | La sustitución revalida todas las habilitaciones y conserva la asignación original |
| [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) | Alertas **por categoría**, al puesto, con umbrales configurables; la alerta convierte un bloqueo en una gestión |
| [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) · [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) · [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) | El espejo es de solo lectura, se reconcilia y degrada explícitamente |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) · [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) | Catálogo de categorías y matriz con vigencia; evaluación a la fecha del hecho y congelada |
| [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) · [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) | Registro inmutable; los rangos cerrados de licencia no se editan |
| [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) · [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) · [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) | Captura sin red en delegación, fechas distintas, original adjunto, cero sobrescritura |
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | Marco de las incompatibilidades y del registro del intento bloqueado |

## Anclas de estado y de control

- **Bloqueos duros que este caso de uso alimenta:** `BD-02` licencia habilitante y vigente durante todo el rango —se evalúa en `T-08`, `T-10`, `T-12` y en `T-17` tanto en el relevo como en la prórroga— y `BD-10` disponibilidad del motorista.
- **Punto de control de `PR-01`:** `PC-04` licencia habilitante y vigente, **sin excepción configurable**; `PC-10` disponibilidad del motorista.
- **Transiciones que dependen de este expediente:** `T-08` programar · `T-10` reasignar · `T-12` despachar · `T-17` prórroga y relevo en ruta — [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md), autoridad en transiciones.
- **Matriz de permisos:** acción 24 *habilitar o inhabilitar motorista en el padrón* — `ACT-04` ejecuta, `ACT-10` propone, `ACT-12` consulta, el resto sin acceso.

## Trazabilidad

- **Proceso:** `PR-03` habilitación del motorista · alimenta `PR-01` (E5 asignación, E8 despacho) y `PR-06` cuando un incidente suspende la habilitación
- **Casos especiales:** [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) licencia que vence dentro del rango · [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) motorista no disponible · [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) incapacidad en ruta · [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) relevo en curso · [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) funcionario que conduce su vehículo asignado
- **Normativa:** [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) — ocho categorías `[V]` por fuentes concordantes, `[C]` contraste con el texto oficial; texto de la reforma al Art. 48 `[C]`; la formulación *"bloquear si la licencia estará vencida en cualquier fecha del rango"* es **implicación de requerimiento del equipo** `[I]`, no articulado citable
- **Decisiones:** [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — la licencia es dato **propio** de SIGTI · [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-07 disponibilidad desde Talento Humano, D-12 bloqueo duro de licencias sin excepción
- **Actores:** `ACT-04` principal · `ACT-10` · `ACT-06` · `ACT-17` · `ACT-01` · `ACT-08` · `ACT-12`
- **Historias `HU-xxx`:** pendientes — se derivan de este caso de uso en el bloque de historias
- **Insumos pendientes:** **#17** contrato de API de Talento Humano y umbral de antigüedad del espejo · **#20** y **#23** texto de la reforma al Art. 48 · **#33** criterio de vencimiento de licencia, inicio o fin del día · **#42** catálogo oficial de restricciones médicas de la DNVT · **#48** si puede conducir un servidor que no es motorista de planilla · **#49** si la póliza cubre a un conductor no registrado · **#50** reevaluación de aptitud tras un evento de salud en ruta
- **Insumo nuevo a registrar:** `[C]` **¿Puede un mismo servidor habilitar su propia licencia en el padrón?** Ver la nota de hallazgo de `E5`. La respuesta la debe dar **Auditoría Interna**, y de ella depende si se incorpora un par `I-nn` nuevo a [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md)
- **Insumo nuevo a registrar:** `[C]` **¿Exige la institución capacitaciones o certificaciones internas** —manejo defensivo, primeros auxilios, carga especializada— **como condición de la habilitación?** Determina si el paso 7 es un dato informativo o una precondición bloqueante
