# H-B1-001 — Revisión adversarial del Bloque 1

| Campo | Valor |
|---|---|
| **Revisa** | `qa-tester` |
| **Artefactos revisados** | `docs/00-vision/vision-de-producto.md`, `docs/00-vision/glosario.md`, `docs/01-negocio/actores-y-roles.md`, `docs/01-negocio/mapa-de-procesos.md`, `docs/01-negocio/procesos/PR-01-movilizacion-institucional.md`, `docs/03-arquitectura/estados/orden-de-mision.md`, `docs/01-negocio/reglas/` (README + RN-01 a RN-53) |
| **Contexto aplicado** | `CLAUDE.md`, `DP-001`, `ADR-001`, fichas `NRM-01` a `NRM-10` (consultadas por referencia) |
| **Fecha** | 2026-08-06 |
| **Sprint / Bloque** | Sprint 0 / Bloque 1 |
| **Hallazgos** | 28 — 5 Críticas, 9 Altas, 10 Medias, 4 Bajas |
| **Estado** | **20 de 28 corregidos y verificados. 8 siguen abiertos** — desglose en la sección siguiente |
| **Verificación de cierre** | 2026-08-26, contra los artefactos vivos. No contra el mensaje del commit que dijo haberlos corregido |


## Estado de corrección — verificado el 2026-08-26

Este informe se emitió el 2026-08-06 y se quedó diciendo *«Abierto»* mientras las correcciones se aplicaban. **La verificación se hizo contra los artefactos vivos**, hallazgo por hallazgo, no contra el mensaje del commit que declaró el lote cerrado. Once no estaban corregidos.

### Corregidos y verificados — 20

| Hallazgo | Dónde se comprueba |
|---|---|
| `HB1-01` | **Cerrado el 2026-08-26.** El Nivel 2 ya estaba suspendido con ⛔ y [`DP-002`](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md); **faltaba el Nivel 3**, que seguía diciendo *«el sistema no lo bloquea»*. Reescrito en [`actores-y-roles`](../../01-negocio/actores-y-roles.md) §5.4 apoyándose en el principio **P-2** de la máquina de estados, que separa lo que este nivel mezclaba: los bloqueos duros rigen `T-05`, `T-08` y `T-12` —también en emergencia—, y nunca impiden **registrar el hecho**. La salida sin red es el **código de autorización fuera de línea** del [`§6.6`](../../03-arquitectura/estados/orden-de-mision.md), que la propia autoridad nombra como la respuesta a la segregación en delegaciones pequeñas. Se alinearon además la nota 4 de la matriz de permisos y el cierre de §5.2, y las dos `RN-xx propuesta` se sustituyeron por [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) y [`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md), que ya existen. **Queda declarado el hueco `[C]`**: si a las 03:15 la única persona disponible es el propio motorista, `I-11` no se levanta y ese caso no tiene salida escrita |
| `HB1-02` | [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) lleva el acta de corrección: el caso límite es hoy bloqueo duro, coherente con `I-11`. [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) cita `I-11` |
| `HB1-03` · `HB1-12` | [`RN-06`](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) |
| `HB1-04` | [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md): *«No existe la reapertura»*. Se retiró el `[C]` que preguntaba quién podía reabrir — era la pregunta equivocada |
| `HB1-05` | [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) exige el doble control carga↔aprobación, y declara que no se desactiva |
| `HB1-06` · `HB1-07` | [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md), [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) |
| `HB1-08` | [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md): el permiso ya no exige que la misión esté programada, basta que esté aprobada |
| `HB1-09` | [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) |
| `HB1-10` · `HB1-13` | [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md), [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md), [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) |
| `HB1-11` | La matriz 8 × 8 de `orden-de-mision.md` §3.3 **se eliminó**: duplicaba `I-01`–`I-17` y por eso divergía. Ahora remite a la autoridad. **Queda declarada, no oculta, una divergencia menor**: `Conduce × Programa` y `Conduce × Cierra` no existen en `I-01`–`I-17`, y [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) lo dice en su propia nota |
| `HB1-14` | [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md): `T-18` lo ejecuta `ACT-06`, o `ACT-10` en digitación diferida. `ACT-05` levanta **acta de recepción** y no cierra la bitácora |
| `HB1-16` | [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) mide contra todo el rango más la holgura, igual que `BD-02`. **Es lo que el código implementa hoy** |
| `HB1-19` | [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) |
| `HB1-21` | [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) |
| `HB1-22` | `PC-11` distingue el retorno ordinario del **retorno constatado en oficina**, donde no bloquea. Corregido por la vía de `HB3-04` |
| `HB1-15` | **Cerrado el 2026-08-26.** Los cinco criterios que las reglas creaban y la lista no tenía son hoy `H-09` a `H-13` en [`orden-de-mision.md` §7.2](../../03-arquitectura/estados/orden-de-mision.md): eslabón faltante de la cadena ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)), exceso de capacidad por novedad en ruta ([`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)), diferencia de liquidación sin explicar ([`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)), digitación diferida sin adjunto vencido el plazo ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)) y entrega de combustible sin orden aprobada ([`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md)). **Se aclaró qué significa «cerrada»**, que era la otra mitad: cerrada **para una misión concreta** —nadie inventa ni desactiva un criterio al cerrar—, no cerrada para el catálogo, que §7.2 ya declaraba ampliable. Y §7.1 incorpora la disciplina que faltaba: toda regla que produzca hallazgo tiene que figurar con su `H-nn`, o crea un expediente sin salida. La nota de `HB3-02` se ajustó: el siguiente ID libre es `H-14` |
| `HB1-17` | **Cerrado el 2026-08-26.** Eran **tres** redacciones de un solo número, y apareció una cuarta que el hallazgo no listaba porque es del Bloque 4: el modelo de datos congelaba el indicativo en `T-05`. Zanjado por la autoridad — **un solo congelamiento, en `T-02`**, porque `INV-07` lo exige ya en `SOLICITADA`, que es anterior a toda autorización. `T-05` **ratifica**: registra cuál valor congelado aprobó, y a partir de ahí es *el estimado ratificado en la aprobación*, que es contra el que compara `T-08`. El hueco que eso abría —tarifa que cambia entre envío y autorización— se cierra con recálculo de comparación en `T-05` y devolución por `T-04` si supera el umbral, **no** con un segundo congelamiento. Alineados [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), `T-08`, el [diccionario](../../03-arquitectura/modelo-datos/diccionario-de-datos.md), el [modelo](../../03-arquitectura/modelo-datos/README.md), `CU-04` y un escenario de `HU-147`. **La decisión `D-18` de `HB34-59` —dos congelamientos con carácter distinto— se mantiene íntegra**: solo se corrigió cuándo ocurre el indicativo |

### Siguen abiertos — 8

| Hallazgo | Sev. | Qué falta, comprobado hoy |
|---|---|---|
| `HB1-18` | Media | **Corregido a medias.** De 97 reglas, **4** citan la tabla `I-01`–`I-17`. Es más que las cero de entonces, y sigue sin ser la cobertura que el hallazgo pedía |
| `HB1-20` | Media | El [`README` de reglas](../../01-negocio/reglas/README.md) mantiene `RN-12` marcada `Sí*` con la leyenda *«el bloqueo es configurable»*, y [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) mantiene *«No el bloqueo»*. La leyenda tampoco define el valor `Sí` a secas |
| `HB1-23` | Media | `PC-12` sigue exigiendo el manifiesto **antes** del despacho, y `T-12` sigue emitiéndolo **como efecto** del despacho. La circularidad está intacta. Tampoco existe `BD-nn` de custodia vigente para `T-12` |
| `HB1-24` | Media | [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) sigue diciendo que el borrador descartado se registra *«sin conservar el contenido»*; `orden-de-mision.md` sigue diciendo que pasa a `ANULADA` con motivo. Se contradicen sobre si `INV-40`–`INV-43` aplican |
| `HB1-25` | Baja | Los objetivos 6 y 10 de [`vision-de-producto.md`](../../00-vision/vision-de-producto.md) están **sin tocar**. El 6 sigue sin número; el 10 sigue midiendo *«ubicación actualizada ≥ 90 %»* sin ventana temporal, penalizando la misión sin cobertura que el producto dice atender |
| `HB1-26` | Baja | El [`glosario`](../../00-vision/glosario.md) **no menciona ninguno** de los diez estados del ciclo de vida — cero coincidencias. Por su propia regla, `APROBADA` no podría usarse en un artefacto |
| `HB1-27` | Baja | Los pendientes A–K siguen en `actores-y-roles` §9 sin remitir al insumo numerado que los duplica. El pendiente **D** sigue siendo el insumo **#26** escrito dos veces |
| `HB1-28` | Baja | **Corregido a medias.** El diagrama de `PR-01` incorporó `T-03` y `T-10`. **Siguen faltando `T-11`, `T-16` y `T-20`** — y `T-16` es la que importa: el único camino de la misión suspendida con consumo |
> **Nota de método.** Cada hallazgo trae el caso concreto que lo demuestra: inputs, estado y qué sale mal. Donde el hallazgo se sostiene sobre una cita, la cita es literal. No se reporta nada que no se pueda convertir en una prueba.

---

## Resumen para el PO

Los cuatro artefactos están bien escritos por separado y **se contradicen entre sí en los puntos exactos donde más caro sale equivocarse**: la segregación de funciones en delegaciones pequeñas, la anulación después del despacho, la reapertura de la bitácora, y quién pone en vigencia una tarifa.

El patrón es reconocible: la máquina de estados fue escrita con una postura (*ninguna excepción, escalamiento a sede*), `actores-y-roles` con la opuesta (*régimen de excepción declarado y compensado*), y las 53 reglas con una tercera (*bloqueo duro sin mencionar el problema*). Nadie está equivocado en abstracto; lo que no se puede es implementar las tres.

Además, **las 53 reglas no referencian ni una sola vez la tabla de incompatibilidades I-01 a I-17** que `mapa-de-procesos` §7 y `PR-01` declaran fuente de verdad. El núcleo irreductible que `actores-y-roles` dice que no se levanta nunca no está implementado por ninguna regla.

---

# CRÍTICAS

## HB1-01 — El régimen de excepción a la segregación de funciones existe, no existe y no está escrito, según qué documento se lea

**Severidad:** Crítica
**Artefactos:** `actores-y-roles.md` §5.4 · `orden-de-mision.md` §3.3 · `mapa-de-procesos.md` §7 · `reglas/RN-01`

**Las tres posiciones, literales:**

- `actores-y-roles.md` §5.4 Nivel 2 define un **régimen de excepción declarado** que *"levanta únicamente los pares I-02, I-03, I-04, I-05, I-06, I-08, I-09"*, y el Nivel 3 dice que ante una emergencia *"el sistema **no lo bloquea**, pero lo marca"*.
- `orden-de-mision.md` §3.3 dice lo contrario: *"La solución **no es una excepción configurable**: una excepción registrada es evidencia en contra ante el TSC. La solución es el **escalamiento a sede**."*
- `RN-01` dice una tercera cosa: bloqueo duro, `Configurable: No`, y *"**La emergencia no es excepción.** Si no hay personal para cubrir las funciones, se aplica RN-02 — escalamiento — no la dispensa."*

**Verificación mecánica:** ninguna de las 53 reglas contiene las cadenas `régimen de excepción`, `convalidación`, `núcleo irreductible` ni `I-11`. El mecanismo más discutido del Bloque 1 no tiene una sola regla que lo implemente.

**Caso concreto.** Delegación de Choluteca: un encargado, un auxiliar, un motorista. La Gerencia Administrativa propone y la máxima autoridad declara el régimen de excepción del §5.4, enumerando I-05 (autoriza × despacha), vigente del 01/03 al 30/06. El 12/03 el encargado autoriza SOL-2026-00417 y a las 06:40 la despacha.

| Documento | Resultado |
|---|---|
| `actores-y-roles` §5.4 | Permitido, marcado como *ejecutado en régimen de excepción* folio X, con leyenda en el impreso y convalidación pendiente |
| `orden-de-mision` `BD-06` en `T-12` | **Bloqueado.** Bloqueo duro, sin excepción configurable |
| `RN-01` | **Bloqueado.** Y la emergencia tampoco lo levanta |

Quien implemente `T-12` va a bloquear, y la delegación no podrá mover un vehículo — que es exactamente el desenlace que §5.4 dice querer evitar (*"en dos semanas la delegación vuelve al papel"*).

**Qué hay que decidir.** No es un problema de redacción: es una decisión de producto pendiente, ya registrada como **insumo #26** y como pendiente D de `actores-y-roles` §9. Hasta que Auditoría Interna se pronuncie, los tres documentos deben decir lo mismo — sugerencia: que **ninguno** afirme que el régimen existe, y que `actores-y-roles` §5.4 quede marcada íntegra como propuesta `[C]` no aplicable, en lugar de estar escrita como diseño vigente con tabla de controles compensatorios.

**Escenario que hoy no tiene respuesta única:**

```gherkin
Escenario: Despacho por el mismo servidor que autorizó, en delegación con régimen de excepción declarado
  Dado un régimen de excepción vigente para la delegación "Choluteca" que enumera el par I-05
  Y que el servidor "J. Mejía" autorizó la Orden de Misión "OM-2026-00417"
  Cuando "J. Mejía" intenta despachar la Orden de Misión "OM-2026-00417"
  Entonces ???
```

---

## HB1-02 — RN-01 no alcanza el núcleo irreductible I-11, y RN-14 lo degrada a advertencia

**Severidad:** Crítica — área de rigor 1 (segregación de funciones)
**Artefactos:** `reglas/RN-01` · `reglas/RN-14` · `actores-y-roles.md` §5.2 · `orden-de-mision.md` §3.3

`RN-01` enumera cinco funciones de control — solicitar, autorizar, despachar, entregar combustible, liquidar — y excluye expresamente la conducción: *"**No aplica** a las funciones de consulta, registro de bitácora en ruta (ACT-06 Motorista) ni auditoría"*. Su caso límite lo confirma: *"El motorista es también el solicitante. **Permitido**: conducir no es función de control."*

`actores-y-roles` I-11 dice lo contrario y con el máximo énfasis: *"Motorista × Autoriza / Despacha / Entrega fondo / Liquida su propia misión — **Bloqueo duro — núcleo irreductible** — Autoliquidación: el vector de fraude clásico en combustible"*, y añade que el núcleo *"no se levanta nunca: ni por régimen de excepción, ni por delegación, ni por resolución de la máxima autoridad"*. La matriz de `orden-de-mision` §3.3 lo respalda: Autoriza × Conduce = ✗.

Y `RN-14`, caso límite, lo desactiva: *"Sustitución que rompe la segregación de funciones — el motorista entrante es quien autorizó la orden. ... `[C]` confirmar el criterio con Auditoría Interna; **hasta entonces, advertencia registrada, no bloqueo**."*

**Caso concreto.** El Encargado de Delegación autoriza `OM-2026-00417` el lunes (permitido: no es él quien la solicitó). El martes a las 05:50 el motorista asignado llama incapacitado. El encargado ejecuta `T-10` y se asigna a sí mismo como motorista.

- `RN-01`: no bloquea — conducir no está en su lista.
- `RN-14`: advertencia registrada, se puede continuar.
- `actores` I-11 y `orden-de-mision` §3.3: bloqueo duro que no se levanta jamás.

Resultado con las reglas escritas: la misma persona autoriza y ejecuta la misión, sale con el fondo, y el único que declara los kilómetros y el consumo es quien firmó la autorización. Es el vector que I-11 nombra por su nombre.

**Qué corregir.** `RN-01` debe incorporar la conducción de la propia misión como sexta relación de incompatibilidad, o debe escribirse una regla nueva que implemente I-11 a I-13. `RN-14` no puede dejar en `[C]` la aplicación de un par que otro artefacto declara irreductible: o I-11 es irreductible y `RN-14` bloquea, o I-11 no lo es y `actores-y-roles` debe rebajarlo.

---

## HB1-03 — Anular después del despacho: cuatro documentos, cuatro respuestas, y una que niega una transición que la máquina de estados define

**Severidad:** Crítica — área de rigor 5 (bitácora inmutable)
**Artefactos:** `orden-de-mision.md` §3.1/§3.4/§8.1 · `actores-y-roles.md` §4.2 · `PR-01` §3.3 · `reglas/RN-06`

| Fuente | Quién puede anular una misión `DESPACHADA` | ¿Y una `EN_RUTA`? | ¿Y una `CERRADA`? |
|---|---|---|---|
| `orden-de-mision` `T-15` | ACT-04 **+** ACT-07 **+** ACT-13, solo con devolución íntegra; con cualquier consumo, `T-16` | **Prohibida** (§3.4) | **Prohibida.** §8.1: *"Desde ellos no sale ninguna transición. Nunca"* |
| `actores-y-roles` §4.2 | **ACT-08** | **ACT-08**, con reversión de vales | *"Reapertura excepcional por **ACT-09**"* `[C]` |
| `PR-01` §3.3 | ACT-08 | — (pero *"ACT-09 puede anular en **cualquier estado** por causa grave"*) | Ídem |
| `RN-06` §3 | **Nadie.** *"`ANULADA` es alcanzable desde cualquier estado **anterior a `DESPACHADA`**"* | No | No |

**Caso concreto 1.** `OM-2026-0091` se despacha a las 07:00: vale de L 3,000 entregado y firmado, Orden impresa con folio consumido, llaves entregadas. A las 07:40 se suspende la misión y el vale vuelve sin consumir. ¿Quién ejecuta la anulación? Tres respuestas distintas y una cuarta que dice que la transición no existe. `RN-06` además justifica su posición con una premisa falsa — *"A partir de `DESPACHADA` **el vehículo ya salió**"* — cuando `orden-de-mision` es explícita: en `DESPACHADA` *"todavía no ha salido del predio"*; la salida es `T-14`.

**Caso concreto 2.** Misión `CERRADA` de marzo. En noviembre la máxima autoridad quiere anularla por causa grave. `actores-y-roles` §4.2 fila "Cualquiera / Cualquier estado / ACT-09" lo permite, y su fila `CERRADA` admite reapertura por ACT-09. `orden-de-mision` §7.5 lo prohíbe con fundamento explícito: *"si un estado terminal puede cambiar meses después, entonces ningún reporte histórico es reproducible"*. Esto no es un matiz: es la inmutabilidad del expediente, uno de los seis puntos de rigor máximo.

**Qué corregir.** `orden-de-mision` `T-15` es la especificación más detallada y la única que resuelve el fondo entregado; `actores-y-roles` §4.2 y `PR-01` §3.3 deben alinearse a ella, y `RN-06` §3 debe reescribirse: el corte no es `DESPACHADA`, es `EN_RUTA`.

---

## HB1-04 — RN-05 permite reabrir la bitácora cerrada; la máquina de estados lo prohíbe con fundamento

**Severidad:** Crítica — área de rigor 5
**Artefactos:** `reglas/RN-05` · `reglas/RN-53` · `orden-de-mision.md` §2 `RETORNADA` y §3.4

`RN-05`, tabla de "Condiciones de aplicación":

| Artefacto | Evento que lo cierra | **Quién puede reabrir** |
|---|---|---|
| Bitácora de misión | Registro de retorno confirmado por ACT-05 | **ACT-04, con motivo y asiento** |

Y su caso límite: *"Registro de campo que llega tarde... entra a la cola de conflictos con su fecha del hecho, y **quien resuelve decide si amerita reapertura**."* `RN-53` lo repite: *"Si la bitácora ya se cerró, entra a la cola de conflictos y **puede exigir reapertura autorizada (RN-05)**."*

`orden-de-mision`, estado `RETORNADA`: *"**No se puede**: Volver a `EN_RUTA`. Anular. Modificar odómetros o eventos capturados — solo corregirlos por asiento."* Y §3.4, transición prohibida: *"`RETORNADA → EN_RUTA` — La ejecución no se reabre. **Reabrir permitiría agregar eventos con fecha del hecho anterior sin control**."*

**Caso concreto.** Misión de cuatro días. El retorno se registra el viernes a las 18:00 y la bitácora se cierra. El sábado sincroniza el teléfono y llega un consumo de combustible de 12 galones con `ocurrido_en` = miércoles 14:30 y fotografía del comprobante.

- `RN-45` lo manda a la cola de conflictos (correcto en ambos documentos).
- `RN-05`: ACT-04 puede **reabrir la bitácora** para incorporarlo.
- `orden-de-mision`: no hay reapertura; se incorpora como **asiento de corrección** sobre bitácora cerrada.

La diferencia no es cosmética: la reapertura devuelve a ACT-04 la capacidad de escribir sobre el denominador de `RN-30` — el kilometraje — que es justamente lo que `RN-05` dice proteger en su propia justificación (*"El motorista es quien tiene el incentivo directo sobre el dato más sensible del sistema: el odómetro"*).

**Qué corregir.** Eliminar la reapertura de `RN-05` y dejar solo el asiento de corrección, o justificar por qué la máquina de estados se equivoca. El `[C]` que `RN-05` deja abierto (*"si ACT-04 es efectivamente quien puede reabrir o si esa facultad es de ACT-08"*) plantea la pregunta equivocada: primero hay que decidir si la reapertura existe.

---

## HB1-05 — RN-39 permite que el Administrador del Sistema ponga en vigencia una tarifa por sí solo

**Severidad:** Crítica — dinero + incompatibilidad I-13
**Artefactos:** `reglas/RN-39` · `actores-y-roles.md` §4.3 y §5.2 (I-13) · `mapa-de-procesos.md` PR-09

`RN-39`, enunciado: *"Todo dato de origen normativo o institucional **debe** existir como parámetro con rango de vigencia..., consultable y **modificable por ACT-01 Administrador del Sistema o por el rol facultado**, sin cambio de código y sin reinicio del sistema."* Ni el enunciado ni los cinco puntos de "Comportamiento esperado" mencionan una aprobación posterior. El punto 1 solo exige registrar *"valor, vigencia, fuente, fecha de verificación, y quién lo cargó"*.

`actores-y-roles` §4.3 dice otra cosa, y la titula **"Doble control sobre parámetros normativos"**: *"`ACT-01` **carga** el parámetro y su rango de vigencia... `ACT-08` **aprueba** su puesta en vigencia. **Sin la aprobación, el parámetro existe pero no se aplica.**"* La nota 10 de su matriz de permisos es aún más clara: *"**Carga** el parámetro; **no lo pone en vigencia**."* `mapa-de-procesos` PR-09 lo repite: *"ACT-01 carga el parámetro y su respaldo documental; ACT-08 aprueba su puesta en vigencia — **doble control**"*.

Y `actores-y-roles` I-13 declara núcleo irreductible: *"`ACT-01` Administrador × cualquier rol con facultad de autorizar, aprobar fondo o liquidar — Podría otorgarse a sí mismo la facultad y borrar el rastro."*

**Caso concreto.** ACT-01 carga `tarifa_peaje(Zambrano, Liviano/Turismo, desde 2026-03-01, L 45)` con fuente "comunicado COVI-H" y sin adjunto. Con `RN-39` tal como está escrita, a partir del 01/03 `RN-34` la resuelve, `RN-35` estima con ella, y `RN-30`/`RN-37` concilian contra ella. Una sola persona, sin segundo par, alteró la base de cálculo de todas las misiones de marzo. `RN-39` es además uno de los "cinco bloqueos que no se pueden desactivar" del README de reglas — y el control que lo hace defendible no está en su texto.

**Qué corregir.** `RN-39` debe incorporar el doble control como parte del enunciado, o escribirse una regla propia para él. La regla candidata 6 de `actores-y-roles` §8 lo pedía textualmente (*"Ningún parámetro normativo entra en vigencia con la sola acción del administrador del sistema"*) y no se convirtió en `RN-xx`.

---

# ALTAS

## HB1-06 — En qué estado se entrega el fondo de combustible: RN-32 dice APROBADA, la máquina dice DESPACHADA, PR-01 dice PROGRAMADA

**Severidad:** Alta
**Artefactos:** `reglas/RN-32` · `orden-de-mision.md` §2 `PROGRAMADA`, `EF-04`, §10.1 · `PR-01` §3.1 y E7

`RN-32` fija `estado_minimo_orden_para_asignar_combustible` con **valor inicial `APROBADA`**, y exige simultáneamente que *"el vehículo receptor sea el asignado a esa orden"* y *"el motorista receptor sea el asignado a esa orden"*.

`orden-de-mision`, estado `APROBADA`, `INV-11`: *"Sigue sin reservar recursos: aprobar no es programar."* Estado `PROGRAMADA`, "No se puede": *"**Entregar fondo de combustible.**"* `EF-04` y §10.1: *"`V-02` entregar ocurre **dentro de** `T-12` despachar. **No se entrega fondo a una misión no despachada.**"*

`PR-01` diagrama 3.1 dice una tercera cosa: el nodo `G2` ("Entrega efectivo, vale u orden de pago con folio, contra firma de ACT-06") desemboca en `FIN` = *"Misión **PROGRAMADA**. Documentos impresos y **fondo entregado**"*.

**Caso concreto.** `OM-2026-0120` está en `APROBADA`. ACT-07 registra la entrega del vale. `RN-32` lo permite porque el estado mínimo se cumple — pero **no puede evaluar sus propios requisitos 2 y 3**, porque en `APROBADA` no hay vehículo ni motorista asignados. La regla es incumplible con su propio valor inicial. Y si se implementa `EF-04`, ACT-07 no puede entregar nada hasta `T-12`, con lo que el diagrama 3.1 de `PR-01` describe un estado que nunca ocurre.

**Qué corregir.** Fijar el valor inicial de `estado_minimo_orden_para_asignar_combustible` en `PROGRAMADA` y decidir si la entrega física ocurre dentro de `T-12` (máquina) o antes (PR-01). El punto de control `PC-08` de `PR-01` depende de esta decisión.

---

## HB1-07 — RN-19, implementada literalmente, bloquea todos los despachos y todos los retornos

**Severidad:** Alta
**Artefactos:** `reglas/RN-19` · `orden-de-mision.md` §10.2 y `T-12`

**Bloqueo del despacho.** `RN-19` enunciado: *"El sistema **no debe** permitir asignar ni **despachar** un vehículo cuyo estado operativo vigente sea distinto de **disponible**."* `orden-de-mision` §10.2 define `ASIGNADO` como *"Comprometido a una misión que aún no ha salido. **Cubre `PROGRAMADA` y `DESPACHADA`**"*, y `T-12` exige como precondición que *"su estado operativo **sigue siendo `ASIGNADO`**"*.

*Caso:* el vehículo 04-217 pasa a `ASIGNADO` por `T-08` el lunes. El martes ACT-05 intenta `T-12`. `RN-19` bloquea porque el estado no es `DISPONIBLE`. Ninguna misión puede despacharse.

**Bloqueo del retorno.** `RN-19`: *"El retorno a disponible **debe** ser un acto explícito de ACT-11 Encargado de Mantenimiento, **nunca automático** por cierre de la orden de trabajo"*, y su caso límite del resguardo de Semana Santa afirma que ese es *"el único retorno automático admitido"*. `orden-de-mision` §10.2 dice que `EN_MISION → DISPONIBLE` (`W-06`, retorno sin novedad) es *"Automático por `T-14` y `T-18`"*, y que *"`ASIGNADO` y `EN_MISION` **los fija el sistema**, no una persona"*.

*Caso:* misión rutinaria que retorna sin novedades. Con `RN-19`, el vehículo queda en `EN_MISION` hasta que el jefe de taller — que no participó en nada — ejecute un acto explícito. Al día siguiente no se puede programar.

**Qué corregir.** `RN-19` debe hablar de "estados que habilitan asignación" según el catálogo de §10.2 y distinguir la asignación (`T-08`, exige `DISPONIBLE`) del despacho (`T-12`, exige `ASIGNADO`); y acotar la exigencia de acto explícito de ACT-11 al retorno **desde `EN_TALLER`**, no desde `EN_MISION`.

---

## HB1-08 — RN-23 bloquea la aprobación, y con ello impide que se tramite el permiso que exige

**Severidad:** Alta
**Artefactos:** `reglas/RN-23` · `PR-01` E3 y `PC-03` · `orden-de-mision.md` `BD-04`, `T-02`, `T-05`

`RN-23`: *"El sistema **no debe aprobar** ni despachar una misión cuya ventana de circulación caiga... en día inhábil... si no existe un permiso de circulación vigente firmado por la máxima autoridad."*

`PR-01` E3: *"Si la ventana cae en día u hora inhábil, la aprobación de `ACT-03` **es válida pero no habilita el despacho**. Queda con la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` y **se dispara `PR-07`** hacia `ACT-09`."*

`orden-de-mision`: `BD-04` *"Se evalúa en `T-12`"*; `T-02` *"Se avisa desde aquí, **aunque el bloqueo sea en `T-12`**"*; `T-05` no lista `BD-04` entre sus precondiciones.

**Caso concreto.** Solicitud capturada el jueves 12/03 para salir el sábado 14/03 a las 06:00. La jefatura abre su bandeja el jueves por la tarde.

- Con `RN-23`: no puede aprobar, porque no hay permiso.
- Con `PR-01` E3: `PR-07` — el trámite del permiso ante ACT-09 — **solo se dispara después de aprobar**.

Deadlock: no se puede aprobar sin permiso y no se puede pedir el permiso sin aprobar. En la práctica el usuario resolverá declarando la salida en día hábil, que es el fraude que la regla busca impedir.

**Qué corregir.** `RN-23` debe bloquear el **despacho**, no la aprobación, y describir la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` como salida de la aprobación. `PC-03` de `PR-01` ya lo dice bien (*"Marca en E3; bloqueo del despacho en E8"*).

---

## HB1-09 — RN-35 exige para aprobar una categoría de peaje que, en `APROBADA`, todavía no puede existir

**Severidad:** Alta
**Artefactos:** `reglas/RN-35` · `reglas/RN-33` · `orden-de-mision.md` `INV-07`, `INV-08`, `INV-11`, `T-08` · `PR-01` E2

`RN-35`: *"Si la ruta declarada atraviesa puntos de peaje y la estimación no se puede calcular — **categoría no resuelta (`RN-33`)** o tarifa no vigente (`RN-34`) — el sistema **debe bloquear la aprobación**."* Y su desglose exige por fila *"categoría aplicada **al vehículo**"*.

`RN-33` deriva la categoría de *"los atributos de **su ficha técnica**"* — es decir, de un vehículo concreto y existente en la flota.

Pero en `SOLICITADA` (`INV-08`) y en `APROBADA` (`INV-11`) **no hay vehículo asignado**; la asignación es `T-08`, posterior. Y `orden-de-mision` `T-08` sitúa la exigencia donde corresponde: *"El vehículo tiene resuelta su **categoría de peaje** vigente; sin ella el estimado no es verificable"*.

`PR-01` E2 resuelve el problema de otra manera: el estimado se calcula *"con la categoría que corresponde al **tipo de vehículo requerido**"* — pero `RN-33` no define ninguna derivación por tipo, solo por vehículo.

**Caso concreto.** Solicitud Tegucigalpa → San Pedro Sula, tipo requerido "pickup doble cabina", 6 cruces por las tres estaciones del Corredor Logístico. Al pedir la aprobación: ¿qué categoría se aplica? No hay vehículo. Si se aplica la del tipo, `RN-33` no la define y la fila del desglose no tiene fundamento que mostrar. Si se exige el vehículo, la aprobación no puede ocurrir nunca.

**Consecuencia adicional que conviene mirar de frente:** `RN-34` ordena *"El sistema arranca **sin tarifas cargadas**, bloqueando la estimación"* mientras el insumo #21 siga abierto. Combinado con `RN-35`, **ninguna misión con peaje se puede aprobar el día de la puesta en marcha**, salvo apagando `estimacion_peaje_obligatoria_para_aprobar` — decisión que nadie ha tomado y que ningún artefacto registra.

**Qué corregir.** Definir en `RN-33` la derivación de categoría **por tipo de vehículo** para la estimación previa, distinta de la derivación por vehículo para la conciliación; y reformular el bloqueo de `RN-35` en consecuencia.

---

## HB1-10 — RN-50 convierte en bloqueo lo que la máquina de estados y el ADR-001 resolvieron como advertencia, y abre una salida sobre un bloqueo duro

**Severidad:** Alta — área de rigor 2 y 6
**Artefactos:** `reglas/RN-50`, `RN-12`, `RN-10` · `orden-de-mision.md` `T-05`, `T-08` · `ADR-001` mitigación 5

`RN-50`: *"Superado el **umbral de bloqueo**, la operación sensible **debe bloquearse** hasta que la sincronización se restablezca **o hasta que un rol facultado autorice la operación degradada**"*, siendo sensibles *"asignar motorista, autorizar una orden de misión, aprobar un fondo de combustible y liquidar"*. `RN-12` y `RN-10` lo confirman como bloqueo en sus casos límite.

`orden-de-mision` `T-05`: *"El espejo de la jerarquía no está desactualizado más allá del umbral configurable. Si lo está, el sistema **advierte antes de permitir** y registra la advertencia en el diario."* `T-08`: *"Si lo está: **advertencia registrada** y visible en el documento impreso."* `ADR-001`, mitigación 5, usa la misma palabra: *"el sistema **advierte** antes de permitir operaciones sensibles"*.

**Caso concreto.** Espejo de Talento Humano sin sincronizar desde hace 20 días; `umbral_bloqueo_desincronizacion` = 15. ACT-04 programa `OM-2026-0207`.

- `RN-50` + `RN-12`: bloqueado.
- `T-08`: permitido, con advertencia registrada e impresa en la Orden.

Y si se implementa el bloqueo, la **"autorización degradada"** de `RN-50` deja pasar la asignación igual — un camino para asignar motoristas contra datos de disponibilidad no confiables que ni `actores-y-roles` ni la máquina de estados contemplan, y que ninguna incompatibilidad I-nn regula.

**Qué corregir.** Decidir advertencia o bloqueo escalonado, y si el bloqueo existe, decidir si la "autorización degradada" es admisible y quién la ejerce. Hoy no consta en ninguna matriz de permisos.

---

## HB1-11 — La matriz de segregación de la máquina de estados declara compatible un par que actores-y-roles y RN-01 declaran bloqueo duro

**Severidad:** Alta — área de rigor 1
**Artefactos:** `orden-de-mision.md` §3.3 · `actores-y-roles.md` §5.2 (I-03) · `reglas/RN-01` · `PR-01` §1

`orden-de-mision` §3.3, fila **Solicita**, columna **Entrega combustible**: **✓ compatible**. Simétrico en la fila "Entrega combustible", columna "Solicita".

`actores-y-roles` I-03: *"Solicita × Entrega fondo — Misma misión — **Bloqueo duro**"*.
`RN-01`: las cinco funciones incluyen solicitar y entregar combustible; *"**ninguna persona puede ejercer más de una**"*.
`PR-01` §1, ACT-02, columna "No puede hacer": *"Autorizar, despachar, **recibir el fondo** ni liquidar su propia misión — I-01 a I-04"*.

**Caso concreto.** La asistente de la Unidad de Archivo captura `SOL-2026-00512` por encargo de su jefatura — figura que `actores-y-roles` ACT-02 describe como habitual — y además es la encargada de caja chica que entrega el vale de combustible de esa misma misión. `RN-01` e I-03 bloquean; la matriz §3.3, que es la que va a leer quien implemente `BD-06` y M-09, lo permite.

**Añadido.** La misma matriz §3.3 introduce dos pares que **no existen** en la tabla I-01 a I-17 que `mapa-de-procesos` §10 declara fuente de verdad: `Programa × Despacha` = ✗ (bloqueo inventado, ausente de I-nn) y `Programa × Liquida` = ✓ (que I-14 trata como configurable apagado por defecto, no como compatible sin más).

**Qué corregir.** La matriz §3.3 y la tabla I-01 a I-17 deben derivar una de la otra, no coexistir. Si `actores-y-roles` es la fuente de verdad, §3.3 debe generarse a partir de ella y declararlo.

---

## HB1-12 — RN-06 contradice la máquina de estados en cuatro puntos, incluido el modelo de "un solo expediente con dos fases"

**Severidad:** Alta
**Artefactos:** `reglas/RN-06` · `orden-de-mision.md` §0.3, §3.1, §3.4, estado `DESPACHADA`

| # | `RN-06` dice | `orden-de-mision` dice |
|---|---|---|
| 1 | *"**No aplica** a las solicitudes de transporte previas a la emisión de la orden, que tienen **su propio ciclo más simple en M-06**"* — pese a que su propio enunciado incluye `BORRADOR → SOLICITADA → APROBADA` | §0.3: *"Es **un solo expediente con dos fases, no dos entidades que se copian**. La razón: partirlo en dos **rompe la cadena trazable** que exige NRM-01"* |
| 2 | *"Las transiciones hacia atrás permitidas son... **`APROBADA → SOLICITADA`** por devolución del autorizador"* | Esa transición **no existe**. La devolución es `T-04`: `SOLICITADA → BORRADOR` |
| 3 | Omite `LIQUIDADA → RETORNADA` de su lista de transiciones hacia atrás permitidas | `T-20` existe y es ejecutada por ACT-08 |
| 4 | *"Cambio de vehículo o motorista después de `DESPACHADA`. **No retrocede el estado**: se registra como sustitución en ruta"* | Estado `DESPACHADA`, "No se puede": *"**Cambiar de vehículo o motorista sin revertir primero a `PROGRAMADA`** mediante devolución de lo entregado"*. `T-17` (relevo) solo aplica desde `EN_RUTA` |

**Caso concreto del punto 4.** El vehículo se despacha a las 07:00 con vale entregado y Orden impresa. A las 07:15 no arranca y se cambia por otro, sin haber salido del predio. `RN-06` dice que se registra como sustitución en ruta sin retroceder el estado; `orden-de-mision` exige devolver el vale y los documentos, volver a `PROGRAMADA`, reasignar por `T-10` y volver a despachar consumiendo un folio nuevo (`EF-02`: *"El folio reservado se **anula**"*). El tratamiento del folio es distinto en cada camino, y el folio es la unidad de trazabilidad del combustible (`RN-27`).

**Añadido.** `RN-06` es la regla que `RN-06`, `RN-08` y el propio README declaran "estructural, no configurable". Una regla estructural que no coincide con la máquina de estados que la implementa es la peor combinación posible.

---

## HB1-13 — La licencia de conducir: la máquina de estados sigue tratándola como espejo, RN-48 también, y PR-01 se contradice consigo mismo

**Severidad:** Alta — área de rigor 2
**Artefactos:** `ADR-001` · `orden-de-mision.md` `BD-02` · `PR-01` E5 y §6 · `reglas/RN-48` y `RN-10` · `actores-y-roles.md` ACT-17

`ADR-001` tiene una sección titulada **"La licencia de conducir es dato PROPIO de SIGTI, no espejo"**, incorporada *"tras el análisis de PR-01 (2026-08-06)"*, con tabla de resolución explícita: *"Licencia: número, categorías, vigencia, restricciones médicas, escaneo | **SIGTI (propio)**"*.

Y sin embargo:

- `orden-de-mision` `BD-02`, apartado "Dependencia del espejo": *"**Los datos de licencia vienen de Talento Humano ([ADR-001])**. Si el espejo lleva más del umbral configurable sin confirmarse..."* — atribuye a `ADR-001` exactamente lo contrario de lo que `ADR-001` dice.
- `RN-48`, tabla del enunciado: *"Expediente del empleado, **licencias**, permisos, vacaciones, feriados | Talento Humano | **Espejo**"* — reproduce la tabla vieja de `ADR-001`, no la corregida.
- `PR-01` §1 y E5 dicen "dato propio de SIGTI"; pero `PR-01` §6 dice *"`[C]` **Frontera sin resolver** — quién es fuente de verdad de la licencia de conducir. `ADR-001` la lista como dato espejeado de Talento Humano"*. El documento se contradice consigo mismo con siete secciones de distancia.
- `actores-y-roles` ACT-17 lo deja `[C]` y el pendiente I de §9 lo mantiene abierto.
- `RN-10` §5 deja los dos: *"el origen del dato (**espejo de Talento Humano o expediente propio de SIGTI**)"*.

**Caso concreto.** Al escribir la historia de M-05 "registrar la licencia de un motorista": ¿es un formulario de captura o una pantalla de solo lectura? `RN-48` es **bloqueo duro, no configurable**: *"ninguna pantalla ni operación de SIGTI debe permitir editarlos"*. Implementada literalmente, `RN-48` **impide capturar la licencia dentro de SIGTI** — y con ello `BD-02`, `RN-09` y `RN-10`, el bloqueo de mayor valor legal del sistema, se quedan sin fuente de datos.

**Qué corregir.** Propagar la corrección de `ADR-001` a `RN-48` (quitar "licencias" de la fila de Talento Humano), a `BD-02`, a `PR-01` §6 y al pendiente I de `actores-y-roles`. Es una corrección mecánica de cuatro líneas cuyo coste de no hacerla es un módulo mal construido.

---

## HB1-14 — Quién ejecuta la salida y el retorno: PR-01 se los da a ACT-05, la máquina a ACT-06

**Severidad:** Alta — área de rigor 6
**Artefactos:** `PR-01` §3.3 y E11 · `orden-de-mision.md` `T-14`, `T-18`, `INV-27` · `actores-y-roles.md` §4 fila 7

`PR-01` §3.3: *"`DESPACHADA → EN_RUTA`: **ACT-05 registra la salida**"* y *"`EN_RUTA → RETORNADA`: **ACT-05 recibe y cierra bitácora**"*. E11 lo repite: *"`ACT-05` cierra la bitácora."*

`orden-de-mision` `T-14`: *"**ACT-06 Motorista** · sin conectividad"*. `T-18`: *"**ACT-06** · **ACT-10** en digitación diferida · sin conectividad"*. `INV-27`: *"La autoridad del expediente reside en el dispositivo portador; **ninguna oficina modifica datos capturados en campo**."*

`actores-y-roles` §4 fila 7 ("Registrar bitácora") da `E` en negrita a ACT-06 y `E⁵` a ACT-05 acotado a *"apertura y cierre de bitácora **en el punto de despacho**"*.

**Caso concreto.** Misión de cuatro días a La Mosquitia. El motorista llega a la delegación el sábado a las 21:00; no hay despachador — escenario que `RN-22` reconoce explícitamente (*"Motorista que devuelve el vehículo fuera de horario y no hay quien reciba... queda **pendiente de recepción**"*). Si `T-18` la ejecuta ACT-05, la misión no puede pasar a `RETORNADA` hasta el lunes, el odómetro final no se captura en el momento del hecho (contra TSC-NOGECI V-10 y `RN-46`), y el retorno registrado offline no tiene dueño. Es exactamente el escenario que la premisa rectora 5 dice cubrir.

**Qué corregir.** `PR-01` §3.3 y E11 deben reflejar que `T-14` y `T-18` los ejecuta ACT-06 desde el dispositivo, y que la recepción física de ACT-05 es un **acta de recepción** (`INV-31`) distinta del cierre de la bitácora.

---

# MEDIAS

## HB1-15 — La lista de criterios de hallazgo se declara cerrada, y cinco reglas crean criterios fuera de ella

**Severidad:** Media
**Artefactos:** `orden-de-mision.md` §7.1 y §7.2 · `reglas/RN-08`, `RN-21`, `RN-29`, `RN-32`, `RN-47`

`orden-de-mision` §7.2: *"`T-22` está disponible **si y solo si** se cumple al menos uno"* de `H-01` a `H-08`. §7.1: *"**No es un cajón de sastre.** Si el criterio no está en la lista de 7.2, no se cierra con hallazgo."*

Criterios que las reglas crean y que no tienen `H-nn`:

| Regla | Criterio que introduce |
|---|---|
| `RN-08` | Eslabón faltante de la cadena de trazabilidad (`H-08` solo cubre "ausencia de comprobante obligatorio") |
| `RN-21` | Exceso de capacidad producido por novedad en ruta |
| `RN-29` | Diferencia de liquidación sin explicar por encima de la tolerancia |
| `RN-32` | Entrega de combustible sin orden aprobada |
| `RN-47` | Digitación diferida sin adjunto del original, vencido el plazo |

**Caso concreto.** Misión con todos los comprobantes presentes, rendimiento dentro de umbral, ruta coherente, pero L 400 de diferencia de caja sin explicar. `RN-29`: *"la orden **no debe** poder pasar a `LIQUIDADA`: solo a `CERRADA_CON_HALLAZGO`"*. `orden-de-mision`: `T-22` no está disponible porque ningún `H-nn` se cumple, y `T-21` tampoco porque `RN-29` lo prohíbe. El expediente queda sin salida.

**Qué decidir.** Ampliar `H-01..H-08` con estos cinco criterios, o declarar la lista extensible. Las dos cosas a la vez no se puede.

---

## HB1-16 — RN-10 y BD-02 miden el rango de vigencia de la licencia contra fechas distintas

**Severidad:** Media — área de rigor 2
**Artefactos:** `reglas/RN-10` · `orden-de-mision.md` `BD-02` y `EF-01`

`RN-10`: la licencia no debe vencer *"en cualquier fecha comprendida entre la **fecha de salida y la fecha prevista de retorno**, ambas inclusive"*.
`BD-02` condición 2: *"`fecha_vencimiento_licencia ≥ fin de la ventana de la misión, **incluida la holgura posterior**`"*. La holgura posterior es el parámetro configurable de `EF-01`.

**Caso concreto.** Salida 10/03, retorno previsto 12/03, `holgura_posterior` = 1 día → ventana efectiva hasta el 13/03. Licencia con vencimiento 12/03 y `criterio_vencimiento_licencia` = *fin del día*.

- `RN-10`: **permite** (vigente el último día del rango).
- `BD-02`: **bloquea** (12/03 < 13/03).

Dos implementaciones legítimas dan resultados opuestos en el control que `NRM-06` califica como el de mayor valor legal del sistema. Y la elección no es indiferente: la holgura cubre *"retorno tardío, mantenimiento posterior"* — momentos en que el vehículo ya no circula, y por tanto la licencia ya no es exigible.

**Qué corregir.** Fijar un solo criterio y escribirlo en los dos sitios. Recomendación: la vigencia de licencia se evalúa contra la ventana **solicitada**, no contra la ventana efectiva con holguras, y decirlo expresamente en `BD-02`.

---

## HB1-17 — El momento en que se congela el estimado no coincide entre los tres documentos

**Severidad:** Media
**Artefactos:** `orden-de-mision.md` `T-02`, `INV-07`, `T-08`, `EF-03` · `reglas/RN-41`

- `T-02` (envío a `SOLICITADA`): *"Se calcula el estimado de peajes... y **se congela** junto con el identificador de la tabla de tarifas usada"*; `INV-07` lo convierte en invariante de `SOLICITADA`.
- `RN-41`: *"En el momento en que un valor calculado **se somete a autorización y es autorizado**, el sistema **debe congelarlo**"*. Los efectos de `T-05` no mencionan ningún congelamiento.
- `T-08`: *"Se **recalcula** el estimado de peajes... Si difiere del **estimado congelado en la aprobación** por encima del umbral, **se exige nueva autorización**"* — llama "congelado en la aprobación" a un valor que se congeló en el envío.
- `EF-03` congela el **paquete normativo completo** al despachar (`T-12`).

**Caso concreto.** Enviada el 01/02 con estimado L 264 (tabla v3). Autorizada el 05/02. Nueva tarifa vigente desde el 08/02. Programada el 10/02: recálculo da L 372. ¿Contra qué valor se compara el umbral que dispara la reautorización — el congelado en `T-02` o el que `RN-41` dice congelar en `T-05`? Si son el mismo, `RN-41` está mal enunciada; si no lo son, hay dos valores congelados y nadie dice cuál manda.

`RN-41` está en la lista de los cinco bloqueos irrenunciables del README. Su disparador tiene que ser único y verificable.

---

## HB1-18 — Las 53 reglas nunca referencian la tabla I-01 a I-17, y ocho reglas candidatas del Bloque 1 no se escribieron

**Severidad:** Media — trazabilidad
**Artefactos:** `reglas/` completo · `actores-y-roles.md` §5.2 y §8 · `mapa-de-procesos.md` §7 y §10 · `PR-01` §9

`mapa-de-procesos` §7: *"La tabla completa de incompatibilidades I-01 a I-17 y su núcleo irreductible están en actores-y-roles.md, sección 5."* §10: *"Es la **fuente de verdad**; este mapa solo referencia."* `PR-01` cita I-01, I-04, I-07, I-08, I-09, I-10, I-11, I-14 y I-15 en su tabla de actores y en sus puntos de control.

**Verificación mecánica sobre `docs/01-negocio/reglas/`:** cero ocurrencias de `I-11`, `irreductible`, `régimen de excepción` y `convalida`.

De las 12 reglas candidatas que `actores-y-roles` §8 declara derivadas de sí mismo, solo 4 tienen `RN-xx` (candidata 4 → `RN-52`, 5 → `RN-27`, 7 → `RN-01`, 10 → `RN-07`). Sin regla quedan:

| Candidata | Enunciado | Consecuencia de que falte |
|---|---|---|
| 1 | Permisos por puesto vigente a la fecha del hecho; el asiento registra persona **y puesto** | `RN-03` solo exige "cargo y rol vigentes" en actos de autorización, no en toda transición |
| 2 | No se cierra una asignación de puesto con custodias físicas activas | `RN-22` cubre la custodia del vehículo, no el cierre de asignación de puesto ni los vales sin canjear |
| 3 | Alcance de datos por tipo de objeto | Sin regla, ACT-11 y ACT-14 no tienen alcance probable |
| 6 | Doble control de parámetros | Ver **HB1-05** |
| 8 | El núcleo irreductible no admite excepción | Ver **HB1-02** |
| 9 | Los actos en régimen de excepción impiden el cierre hasta convalidar | Ver **HB1-01** |

De las 20 candidatas de `PR-01` §9 tampoco se escribieron `RN-c:consolidacion-conserva-expedientes`, `RN-c:advertencia-visible-en-expediente` y `RN-c:convalidacion-con-plazo-maximo`.

**Caso concreto de la más cara.** La consolidación aparece en `orden-de-mision` §0.3 y `EF-01` (camino preferente ante conflicto de recurso), en `PR-01` E4, en `RN-01` (la matriz se evalúa contra el conjunto de solicitantes consolidados) y en `RN-13` (*"no es doble asignación"*). Pero **ninguna regla fija que consolidar conserve el expediente y la autorización de cada solicitud**, que es lo que `PR-01` E4 declara importante *"porque el costo se prorratea y porque cada dependencia responde por lo suyo"*. Al escribir la historia de prorrateo no habrá regla contra la cual probarla.

---

## HB1-19 — Umbral de 12 meses cableado dentro de la regla que prohíbe cablear

**Severidad:** Media
**Artefactos:** `reglas/RN-34`, `RN-39` · `mapa-de-procesos.md` PR-09

`RN-39` enunciado: *"Un requisito, una historia o una prueba que contenga **un número normativo literal está mal escrito**."*

Y en el mismo documento, comportamiento esperado 4: *"cuáles llevan **más de 12 meses** sin revisión"*. `RN-34` enunciado: *"el sistema **debe** alertar cuando una tarifa lleve **más de 12 meses** sin revisar"*. `mapa-de-procesos` PR-09, disparador: *"alerta del sistema por parámetro sin revisar **más de 12 meses**"*.

`RN-17` lo hace bien y sirve de contraste: *"parámetro `umbrales_alerta_vencimiento` por tipo de documento, **valor de referencia** 60 / 30 / 15 días `[C]`"*.

**Caso concreto.** `NRM-10` documenta que en 2026 la tarifa de peaje cambió tres veces en dos meses. La institución quiere revisar tarifas cada 90 días. No hay parámetro que tocar: el 12 está en el enunciado de dos reglas.

---

## HB1-20 — El README de reglas contradice a RN-12 sobre si su bloqueo se puede desactivar

**Severidad:** Media
**Artefactos:** `reglas/README.md` · `reglas/RN-12`

README, leyenda: *"**Cfg.** — `Sí*` = **el bloqueo es configurable**; `No` = no se puede desactivar."*
README, fila `RN-12`: *"Bloqueo duro | **Sí\*** | DP-001 D-07, ADR-001"*.
`RN-12`, ficha: *"**Configurable** | **No el bloqueo.** Sí el catálogo `tipo_ausencia` y su efecto."*

**Caso concreto.** Quien lea solo la tabla del README concluye que se puede apagar el bloqueo por vacaciones o incapacidad — el control que `ADR-001` justifica diciendo que *"un motorista que Talento Humano dio de baja pero que SIGTI sigue asignando no es un problema técnico: es un problema legal"*. Que `RN-16` sea el único otro `Sí*` y ahí la marca **sí** sea correcta hace el error más creíble, no menos.

**Añadido.** La leyenda define `Sí*` y `No` pero no define `Sí`, que es el valor de 20 de las 53 filas. Un lector no puede saber si `RN-13` "Bloqueo duro / Sí" significa que la doble asignación se puede permitir.

---

## HB1-21 — BD-04 no contempla el vehículo de servicio exceptuado que RN-24 y PR-01 sí prevén

**Severidad:** Media
**Artefactos:** `orden-de-mision.md` `BD-04` e `INV-19` · `reglas/RN-24`, `RN-25` · `PR-01` `PC-03` y V-06

`RN-24`: *"Un vehículo **puede** circular en día u hora inhábil **sin permiso de la máxima autoridad** únicamente si tiene registrado un servicio exceptuado vigente."* `PR-01` `PC-03`: *"Excepción: vehículo marcado como de servicio exceptuado — emergencia, seguridad, salud."*

`orden-de-mision` `BD-04`: *"Si cualquier parte de la ventana de la misión cae en día inhábil u hora inhábil..., **debe existir** un permiso de circulación emitido por ACT-09..., y su **salvoconducto impreso debe emitirse** junto con la Orden de Misión."* Sin excepción. `INV-19` lo repite como invariante de `DESPACHADA`. `RN-25` refuerza: *"El despacho de una misión que requiere salvoconducto **debe bloquearse** si el salvoconducto no ha sido emitido."*

**Caso concreto.** Ambulancia institucional con servicio exceptuado vigente y fundamento adjunto. Sale un domingo a las 03:00. `RN-24` lo permite; `BD-04` e `INV-19` bloquean el despacho por falta de permiso y de salvoconducto.

**Qué corregir.** `BD-04` debe incorporar la excepción de `RN-24` como condición evaluada a la fecha del hecho, y `RN-25`/`RN-24` deben decidir si el vehículo exceptuado igualmente porta un documento que lo acredite en carretera — `RN-24` ya lo sugiere (*"La orden de misión impresa **muestra la excepción invocada y su fundamento**"*), pero `BD-04` no lo recoge.

---

## HB1-22 — PC-11 bloquea el cierre de bitácora por incoherencia de odómetro, contra el principio P-2 y contra BD-05

**Severidad:** Media
**Artefactos:** `PR-01` `PC-11` y §9 · `orden-de-mision.md` P-2 y `BD-05` · `reglas/RN-31`

`PR-01` `PC-11`: *"Coherencia del odómetro: sin retroceso, sin salto imposible, sin consumo sin recorrido — **Alerta bloqueante del cierre de bitácora hasta justificar**."* Su regla candidata `RN-c:odometro-coherente` lo repite: *"Retroceso, salto imposible o consumo sin recorrido **impiden cerrar la bitácora** hasta justificar."*

`orden-de-mision` P-2: a las transiciones que registran hechos consumados *"se les aplican validaciones de coherencia que exigen justificación..., **pero nunca impiden el registro**"*. `BD-05` marca **"No bloquea"** el salto imposible, el kilometraje por encima del estimado y el kilometraje por debajo. `RN-31` sigue a `BD-05`, no a `PC-11`: bloquea el retroceso; el salto *"exige justificación"*.

**Caso concreto.** El motorista retorna el viernes a las 22:00 en una delegación sin red, con 640 km cuando la ruta estimaba 210 (desvío por derrumbe en la CA-5, evento que `RN-37` reconoce como frecuente). Con `PC-11`, no puede cerrar la bitácora hasta que alguien justifique — sin red, sin autorizador disponible. El resultado es el que P-2 describe: *"lo deja fuera del expediente, que es exactamente lo que el auditor busca y no encuentra"*.

---

## HB1-23 — RN-22 y PC-12 declaran precondiciones del despacho que la máquina de estados no tiene, y una de ellas es circular

**Severidad:** Media
**Artefactos:** `reglas/RN-22`, `RN-53` · `PR-01` `PC-12` · `orden-de-mision.md` `T-12`

**Custodia.** `RN-22`, bloqueo duro no configurable: *"Un vehículo **sin custodio vigente no debe** poder ser despachado."* `T-12` no lo lista entre sus precondiciones. Aparece solo de forma indirecta en la definición de `DISPONIBLE` de §10.2 (*"con custodio asignado"*), que ya no es el estado del vehículo al despachar (ver **HB1-07**). No existe `BD-nn` para la custodia.

**Manifiesto — y aquí hay circularidad.** `PR-01` `PC-12`: *"Manifiesto emitido y cadena de custodia registrada **antes de la salida** — **Bloqueo del despacho**"*. Pero `orden-de-mision` `T-12`, entre sus **efectos**: *"Se consume el folio y **se emiten** los documentos oficiales: Orden de Misión, salvoconducto si aplica, **manifiesto de personas externas si aplica**"*. Y `RN-53`: *"**Al despachar** la misión, el manifiesto... **se cierra**"*, y su comportamiento esperado 2: *"**Al despachar se congela** y se imprime la versión que porta el motorista."*

**Caso concreto.** Traslado de 6 personas externas. `PC-12` exige el manifiesto emitido **antes** del despacho como condición para despachar; `T-12` y `RN-53` lo emiten **como consecuencia** del despacho. `PC-12` no se puede cumplir nunca.

**Qué corregir.** Decidir si el manifiesto se emite en `T-08` (con la Orden, folio reservado) o en `T-12`, y reformular `PC-12` en consecuencia. Y agregar la custodia vigente como `BD-nn` de `T-12`, o rebajar `RN-22` de bloqueo duro.

---

## HB1-24 — Qué queda de un borrador descartado: RN-04 dice que el contenido no se conserva, la máquina dice que sí

**Severidad:** Media
**Artefactos:** `reglas/RN-04` · `orden-de-mision.md` estado `BORRADOR`, `T-03`, `INV-40` a `INV-43` · `PR-01` §10

`RN-04`: *"**No aplica** a registros en `BORRADOR` que nunca fueron enviados a autorización ni impresos: un borrador puede descartarse, y ese descarte se registra como **evento sin conservar el contenido**. `[C]`"*

`orden-de-mision`, estado `BORRADOR`: *"El descarte de un borrador **no es un asiento reverso**... **Pero tampoco es un borrado físico**: el expediente pasa a `ANULADA` con motivo 'descartado antes de enviar' y **queda fuera de los paquetes de evidencia** de auditoría, marcado como tal."* `INV-40` exige para `ANULADA` motivo obligatorio, tipificado y con autor.

**Caso concreto.** Solicitud en borrador con tres adjuntos y el detalle de la carga, descartada por el solicitante. ¿Queda un expediente `ANULADA` con su contenido y sus adjuntos, o solo una línea de bitácora? La respuesta determina si `INV-40` a `INV-43` aplican a este camino y si el estado `ANULADA` puede o no estar vacío.

**Añadido, del mismo bloque.** `PR-01` §10 resuelve el caso especial *"La misión se canceló después de emitir los vales y entregar el fondo"* como *"vales a `ANULADO` con acta, devolución del fondo registrada con firma, **misión a `ANULADA`** con motivo"* — sin distinguir si hubo consumo. `EF-06` es taxativo: *"Si se consumió aunque sea un lempira, la misión **no se anula**: se liquida"*, camino `T-16`. El caso especial más frecuente de la operación real está resuelto de forma incompleta en `PR-01`.

---

# BAJAS

## HB1-25 — Dos objetivos de la visión no son observables, y uno de ellos castiga el escenario que el producto dice atender

**Severidad:** Baja
**Artefacto:** `vision-de-producto.md` §Objetivos medibles

- **Objetivo 6:** *"Producir evidencia de auditoría sin trabajo manual | Horas para armar un expediente por período | Meta: **De días a minutos** | `[C]`"*. No hay número, no hay definición de "expediente por período", no hay prueba escribible.
- **Objetivo 10:** *"Saber dónde está la flota | Vehículos en misión con estado y ubicación **actualizados** | ≥ 90%"*. "Actualizado" no tiene ventana temporal. Y con la premisa rectora 5 y `orden-de-mision` §6.5 (*"El silencio no es un estado"*), una misión de cuatro días en La Mosquitia **no puede** tener ubicación actualizada, por diseño. Medido sobre esa flota, el 90% penaliza precisamente el caso de uso que la visión pone como prueba de éxito (*"Un motorista pasa cuatro días en La Mosquitia sin señal y su bitácora llega completa"*).

**Qué corregir.** Objetivo 6: fijar un número (por ejemplo, ≤ 15 minutos para el paquete de evidencia de un vehículo por trimestre). Objetivo 10: definir la ventana y excluir explícitamente las misiones en zona declarada sin cobertura, o medir "antigüedad mediana del último dato conocido" en lugar de un porcentaje de "actualizados".

---

## HB1-26 — El glosario prohíbe un término que el ciclo de vida usa, y no define ninguno de los diez estados

**Severidad:** Baja
**Artefacto:** `glosario.md`

El glosario se declara *"Fuente de verdad para nombrar entidades, campos, pantallas y botones. **Si un término no está aquí, no se usa en un artefacto**"*, y en "Términos prohibidos" incluye: *"approve (como estado) → **autorizar**"*.

Pero el estado canónico es `APROBADA` (`CLAUDE.md`, `RN-06`, `orden-de-mision`) y el glosario **no define ninguno** de los diez estados del ciclo de vida. Por su propia regla, ninguno de ellos podría usarse en un artefacto.

También faltan términos que el Bloque 1 usa de forma intensiva y que un lector nuevo no puede resolver: **dispositivo portador**, **paquete normativo congelado**, **ventana efectiva**, **holgura**, **régimen de excepción**, **sobregiro**, **cadena divergente**.

---

## HB1-27 — Pendientes duplicados entre `actores-y-roles` §9, `mapa-de-procesos` §9 e `insumos-pendientes.md`

**Severidad:** Baja
**Artefactos:** `actores-y-roles.md` §9 · `mapa-de-procesos.md` §9 · `docs/07-gestion/insumos-pendientes.md`

`actores-y-roles` §9 abre pendientes con letras A–K y advierte que *"deben trasladarse a `insumos-pendientes.md`"*; `mapa-de-procesos` §9 los repite con las mismas letras. Pero al menos dos ya existen numerados:

- Pendiente **D** (*"¿acepta la institución el régimen de excepción con controles compensatorios?"*) = **insumo #26**, con texto prácticamente idéntico.
- Pendiente **I** (*"¿Talento Humano administra la licencia de conducir?"*) se solapa con el **insumo #17** y con el `[C]` ya resuelto en `ADR-001` (ver **HB1-13**).

**Riesgo concreto.** Auditoría Interna responde el insumo #26 y nadie cierra el pendiente D, o al revés. Dos registros del mismo pendiente producen dos estados distintos del mismo hecho.

---

## HB1-28 — El diagrama de estados de PR-01 §3.3 omite cinco transiciones, incluida la única salida de la misión suspendida con consumo

**Severidad:** Baja
**Artefactos:** `PR-01` §3.3 · `orden-de-mision.md` §3.1

El diagrama `stateDiagram-v2` de `PR-01` §3.3 no incluye: `T-03` (`BORRADOR → ANULADA`), `T-10` (reasignación), `T-11` (`PROGRAMADA → APROBADA`), `T-16` (`DESPACHADA → RETORNADA`, misión no ejecutada con consumo) y `T-20` (`LIQUIDADA → RETORNADA`).

La omisión de `T-16` es la que importa: es el **único** camino previsto para la misión que se suspende habiendo consumido fondo, que es a la vez el caso especial que el propio `PR-01` §10 lista como frecuente y que resuelve mal (ver **HB1-24**). Un lector que trabaje solo con `PR-01` no sabrá que existe.

`PR-01` no marca el diagrama como simplificado ni remite a `orden-de-mision` como versión normativa; convendría que lo hiciera explícitamente.

---

# Cierre

## (a) Qué revisé y qué no

**Revisado línea por línea:** los seis artefactos de la lista, completos, incluidos los 53 archivos de regla, el README de reglas, las matrices de permisos, las tablas de incompatibilidad, los diagramas Mermaid y las tablas de transición. Contrastado contra `CLAUDE.md`, `DP-001` y `ADR-001`.

**Verificaciones mecánicas ejecutadas:**

- **Enlaces:** los 6 documentos y las 53 reglas se comprobaron uno a uno. **Ningún enlace `.md` roto.** Es un resultado poco común y hay que decirlo.
- **Cobertura de conceptos** en `docs/01-negocio/reglas/`: `régimen de excepción` = 0, `convalidación` = 0, `núcleo irreductible` = 0, `I-nn` = 0, `consolida` = 3 archivos (`RN-01`, `RN-02`, `RN-13`, ninguno con la regla de fondo).
- **Insumos citados** (#1, #2, #4, #7, #11, #14, #16, #17, #19, #20, #21, #22, #23, #24, #25, #26): todos existen en `insumos-pendientes.md`.

**No revisado en profundidad, y por qué:**

- Las **fichas `NRM-01` a `NRM-10`** se consultaron por referencia, no se auditaron. Las reglas citan textualmente pasajes de `NRM-01`, `NRM-06`, `NRM-09` y `NRM-10`; **no verifiqué que esos pasajes existan en las fichas ni que digan lo que se les atribuye**. Esa revisión corresponde a `normativa-honduras` y es necesaria antes del Bloque 2: `RN-01` atribuye a `NRM-01` la exigencia de bloqueo duro y la propia ficha está marcada `[P]` en la numeración NOGECI.
- Las **plantillas** de `docs/plantillas/`, salvo para comprobar que la de regla de negocio existe.
- La **calidad de los diagramas Mermaid como diagramas** (sintaxis, renderizado). Revisé su contenido, no su compilación.
- **No conté** cuántas de las 53 reglas se pueden convertir hoy en una prueba automatizada; el muestreo que hice sugiere que la mayoría sí, con la excepción de los umbrales `[C]` que por definición no se pueden fijar todavía y están correctamente marcados.

## (b) Lo que más me preocupa, en una frase

Que la **segregación de funciones en delegaciones pequeñas** esté escrita de tres formas incompatibles y sin una sola regla que la implemente, porque es a la vez la decisión pendiente más costosa de tomar (`insumo #26`, requiere pronunciamiento de Auditoría Interna) y la que va a determinar si el sistema se usa o se abandona en el 70% del territorio.

## (c) ¿Listo para el Bloque 2?

**No, todavía no.**

La razón no es la cantidad de hallazgos: el Bloque 1 es sólido en contenido y buena parte de lo reportado son inconsistencias de acoplamiento entre documentos escritos en paralelo, que es exactamente lo que esta revisión existía para encontrar.

La razón es más concreta: el Bloque 2 produce **casos de uso, historias e historias con criterios de aceptación**, y `CLAUDE.md` exige que *"toda historia de usuario referencia al menos una regla de negocio"*. Con las contradicciones actuales, **no se puede saber a qué regla referenciar**:

- Una historia de despacho tendría que elegir entre `BD-06` (bloquea siempre) y `actores-y-roles` §5.4 (levanta siete pares) — **HB1-01**.
- Una historia de entrega de combustible tendría que elegir el estado en que ocurre entre tres — **HB1-06**.
- Una historia de M-05 "registrar licencia" no se puede escribir mientras `RN-48` prohíba editar lo que `ADR-001` declaró propio — **HB1-13**.
- Una historia de cierre de misión con diferencia de caja no tiene transición disponible — **HB1-15**.

**Condición mínima para avanzar** — lo demás puede corregirse en paralelo al Bloque 2:

1. Resolver las **cinco Críticas**. HB1-01 requiere decisión del PO (o dejar el régimen de excepción marcado como propuesta no vigente en los tres documentos, que es una salida legítima y barata); HB1-02 a HB1-05 son correcciones de redacción una vez decidido cuál documento manda.
2. Resolver **HB1-06, HB1-07, HB1-13** — son de corrección mecánica y bloquean historias concretas de M-05, M-07 y M-09.
3. Declarar explícitamente, en `CLAUDE.md` o en un DP nuevo, **qué artefacto manda cuando dos se contradicen**. Mi recomendación: `orden-de-mision.md` para transiciones, invariantes y precondiciones; `actores-y-roles.md` para actores, alcance de datos e incompatibilidades; las `RN-xx` para todo lo demás — y que cada documento genere sus tablas derivadas citando el origen en lugar de reescribirlas.

Sin el punto 3, este mismo hallazgo se vuelve a escribir dentro de tres semanas con otros números.
