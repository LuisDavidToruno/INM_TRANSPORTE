# H-B34-002 — Revisión de arquitectura del Bloque 4

| Campo | Valor |
|---|---|
| **Origen** | Revisión adversarial del arquitecto sobre el modelo de datos, el mapa de navegación y el inventario de pantallas del Bloque 4 |
| **Fecha** | 2026-08-24 |
| **Ámbito revisado** | `docs/03-arquitectura/modelo-datos/README.md`, `docs/03-arquitectura/modelo-datos/diccionario-de-datos.md`, `docs/04-diseno/mapa-de-navegacion.md`, `docs/04-diseno/inventario-de-pantallas.md`, `docs/04-diseno/brief-para-diseno.md` |
| **Contrastado contra** | `estados/orden-de-mision.md` (autoridad en estados), los 21 `RNF`, las 97 `RN`, `actores-y-roles.md` (autoridad en actores e incompatibilidades), los 28 `CE`, las 125 `HU`, `CLAUDE.md` |
| **Estado** | **Los 25 llevan nota de corrección en su artefacto.** Verificados por muestreo, no uno por uno — ver la sección siguiente |
| **Verificación de cierre** | 2026-08-26, contra los artefactos vivos |
| **Numeración** | `HB34-50` en adelante, para no colisionar con la revisión del Bloque 3 |


## Estado de corrección — verificado el 2026-08-26

El encabezado decía *«Emitido. Ninguna corrección aplicada — este archivo es el único que se escribió»*. **Eso dejó de ser cierto:** los 25 hallazgos están citados por identificador en el modelo de datos, el diccionario, los `RNF`, tres ADR y el inventario de pantallas.

**Lo que se verificó y lo que no.** Se comprobó uno por uno que cada identificador esté citado, y se abrieron a lectura los cuatro críticos —los que el propio informe declaró *imposibles de agregar después*—. **No se releyeron los veinticinco completos.**

| Crítico | Qué se comprobó |
|---|---|
| `HB34-50` | El segundo eje de tiempo existe. [`ADR-006`](../../03-arquitectura/adr/ADR-006-temporalidad-bitemporal.md) fija las **dos parejas de fechas**, y está marcado irreversible en `CLAUDE.md`. El código las lleva |
| `HB34-51` | El alcance del encadenamiento quedó definido; la cadena de hash se serializa con `sp_getapplock` dentro de la transacción, para que dos escrituras concurrentes no la bifurquen |
| `HB34-52` | [`RNF-21`](../../02-requisitos/no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md) separa hoy el **identificador interno** —técnico, generado en el dispositivo— del **folio** impreso, y plantea el caso de cinco dispositivos sin conexión entre sí |
| `HB34-53` | [`ADR-004`](../../03-arquitectura/adr/ADR-004-adjuntos-fuera-de-la-base.md) — los adjuntos viven fuera de la base, y con ellos el segmento depurable |
| `HB34-74` | `docs/04-diseno/mockups/` existe, con su `README.md` y el tablero |

Los altos, medios y bajos —`HB34-54` a `HB34-73`— están citados en el [modelo de datos](../../03-arquitectura/modelo-datos/README.md), el [diccionario](../../03-arquitectura/modelo-datos/diccionario-de-datos.md), el [mapa de navegación](../../04-diseno/mapa-de-navegacion.md) y el [inventario de pantallas](../../04-diseno/inventario-de-pantallas.md); no se auditó el contenido de cada corrección.
**Nota de severidad.** Todo corre en documentación de Sprint 0; no hay código ni datos reales. «Crítico» aquí significa **propiedad que no se puede agregar después sin rehacer el modelo**, no incidente. «Alto» significa contradicción entre un artefacto y su autoridad que va a producir código equivocado. «Medio» y «Bajo», inconsistencias corregibles en su sitio.

---

## Resumen ejecutivo

El modelo de datos es, en conjunto, **bueno**: la separación persona/puesto/rol, el diario de transiciones en lugar de columna de estado, la partición del dato personal, el historial de placa en dos ejes y la imputación por tramo son decisiones correctas y bien argumentadas. Ninguna de ellas hay que rehacerla.

Pero el `README.md §16` afirma que *«los cinco que el índice de `RNF` declara imposibles de agregar después —`RNF-03`, `RNF-04`, `RNF-05`, `RNF-17`, `RNF-21`— están todos resueltos en §1»*. **Cuatro de los cinco tienen un agujero estructural**, y los cuatro son exactamente del tipo que el propio documento dice que no se puede corregir tarde:

| `RNF` | Qué falta | Hallazgo |
|---|---|---|
| `RNF-05` | El **segundo eje de tiempo**. El propio `RNF-05` lo nombra («tiempo del sistema»); `version_tabla_parametrica` solo tiene vigencia | `HB34-50` |
| `RNF-04` | El **alcance y el momento del encadenamiento**. La única definición que existe es «encadenamiento por misión», que no satisface su batería de verificación | `HB34-51` |
| `RNF-21` | El rango de folio está modelado **por delegación**, no por dispositivo. Dos dispositivos de la misma delegación sin red colisionan | `HB34-52` |
| `RNF-17` | Los **adjuntos** quedan fuera del segmento depurable, y el segmento solo cubre el manifiesto | `HB34-53` |

`RNF-03` sí está resuelto: el bloque `BTT`, la identidad generada en cliente y el diario de sincronización con conflicto a cola humana sostienen la operación desconectada.

En navegación e inventario, lo entregado es utilizable por un diseñador externo desde mañana. Los problemas son de **trazabilidad y de roles**, no de concepción: 15 historias sin pantalla, 14 pantallas sin historia, y dos pantallas que ofrecen a un actor lo que la matriz de permisos le niega.

---

# Parte 1 — Modelo de datos

## Críticos

### `HB34-50` — La temporalidad es unitemporal, y el `RNF` que la motiva pide dos ejes

**Severidad:** Crítico · **Artefactos:** `modelo-datos/README.md §1 M-01`, `diccionario-de-datos.md §12` · **Contra:** `RNF-05`, `RNF-06`, `RN-94`

`RNF-05` lo dice con una tabla de dos filas:

| Eje | Qué responde |
|---|---|
| Tiempo del hecho | ¿Qué tarifa estaba vigente el día que el vehículo cruzó la caseta? |
| **Tiempo del sistema** | **¿Qué sabía el sistema sobre esa tarifa el día que se emitió el reporte?** |

`version_tabla_parametrica` tiene `vigencia` (tiempo del hecho), `fecha_de_verificacion` (calidad del dato) y `fuente`. **No tiene ningún campo que diga desde cuándo el sistema conoce esa versión.** El `README §21` de arquitectura registra la capacidad como «temporalidad bitemporal»; lo modelado es unitemporal.

**El caso concreto.** En marzo de 2027 se descubre que la tarifa del punto Zambrano cargada para enero–marzo de 2026 estaba mal, y se carga la versión corregida con vigencia retroactiva a enero de 2026 (`RN-42` lo permite explícitamente). El reporte de conciliación de peajes del primer trimestre de 2026, **con fecha de corte 30 de abril de 2026**, se regenera en 2028 para un requerimiento del TSC. La consulta resuelve contra las versiones vigentes a la fecha del hecho, y ahora encuentra la versión corregida —que en abril de 2026 no existía—. El hash del reporte cambia. `RNF-06` exige exactamente lo contrario: *«diferencia entre dos generaciones del mismo reporte, mismos parámetros y misma fecha de corte: **0**»*.

Los valores ya congelados (`valor_congelado.id_version_tabla_parametrica`) sí resisten. Lo que no resiste es **todo reporte agregado que recalcule**, que es la mayoría de lo que `M-14` produce. `REPORTE_GENERADO ||--|| FECHA_CORTE_CONOCIMIENTO` guarda la fecha de corte en el reporte, pero no hay contra qué aplicarla en el lado de los parámetros.

**Qué corregir.** `version_tabla_parametrica` necesita un segundo rango: `conocido_desde` / `conocido_hasta` (tiempo de sistema), distinto de `vigencia`. Toda resolución paramétrica se hace con dos fechas: la del hecho y la de corte de conocimiento. Es la definición de bitemporalidad y no se puede reconstruir después, porque la fecha en que se cargó cada versión se pierde en el momento en que se carga.

---

### `HB34-51` — La cadena de auditoría no tiene alcance definido, y el único alcance que se declara no cumple `RNF-04`

**Severidad:** Crítico · **Artefactos:** `README.md §10 y §12`, `diccionario §13` · **Contra:** `RNF-04`, `RNF-17`

Tres cosas que el modelo no dice y que deciden si la cadena sirve:

1. **Alcance.** La única definición existente está en la tabla de cardinalidades del `§12`: `asiento_auditoria — asiento_auditoria` = `1 : 0..1`, *«**Encadenamiento por misión.** El primer asiento de una cadena no tiene anterior»*. Con una cadena por misión, **borrar íntegra una misión —su orden, sus transiciones y sus asientos— no rompe ninguna cadena**: solo desaparece una cadena entera. La batería de `RNF-04` exige detectar *«(b) eliminar un asiento intermedio»* con identificación del punto exacto de ruptura, y promete *«detección de una alteración de un solo asiento en 4,000,000»*. Eso presupone una cadena de 4 millones, no miles de cadenitas de veinte asientos.
2. **Los asientos que no son de ninguna misión.** `asiento_auditoria.operacion` incluye `ALTA`, `CONSULTA`, `DEPURACION` y `FUSION`. El alta de un vehículo, la carga de una `version_tabla_parametrica`, una consulta a un manifiesto (`RN-52`) y un `evento_depuracion` no pertenecen a ninguna misión. Con encadenamiento por misión, **todos serían «primeros de cadena»**, y el diccionario dice que `id_asiento_anterior` es nulo *«solo en el primero de la cadena»*.
3. **Quién calcula `huella_anterior` y en qué orden.** `asiento_auditoria.id_asiento` es *«generado en el origen»* y la entidad lleva `BTT` completo, o sea que nace en el dispositivo. Un dispositivo con nueve días sin red **no puede conocer el hash del asiento anterior de una cadena del servidor**. `RNF-04` lo resuelve por escrito —*«generados en el cliente y encadenados al integrarse al servidor sin reordenar los ya sellados»*— pero el diccionario marca `huella_anterior` como `Cd.` sin decir que la asigna el servidor al integrar, ni cuál es el criterio de orden. El `§18` sí resuelve esto para `entrada_diario_sincronizacion` (cadena **por dispositivo**, ordenada por `secuencia_dispositivo`); `asiento_auditoria` se quedó sin la decisión equivalente.

**Qué corregir.** Declarar en el modelo: (a) el alcance de la cadena —la recomendación es **cadena global por instancia, más subcadena por dispositivo** para lo que nace desconectado, con empalme al integrar—; (b) que `huella_anterior` la fija el servidor en la integración y es `Dv.`, no `Cd.`; (c) el criterio de orden de encadenamiento (`recibido_en` + desempate determinista), explícitamente distinto del criterio de aplicación de transiciones (`secuencia_dispositivo`). Y borrar «por misión» del `§12`, que hoy es la única definición vigente y es la equivocada.

---

### `HB34-52` — El rango de folios se asigna a la delegación; la prosa dice que lo porta el dispositivo. Dos dispositivos sin red colisionan

**Severidad:** Crítico · **Artefactos:** `README.md §1 M-03 y §3`, `diccionario §14` · **Contra:** `RNF-21`, `RN-44`, `EF-02`

El `§1 M-03` dice que el folio lo genera *«el dispositivo, **consumiendo de un rango pre-asignado a su delegación**»*, y la nota de la Vista 1 llama a `DISPOSITIVO` *«el portador de un rango de folios»*. Pero:

- El diagrama V1 tiene `DELEGACION ||--|{ RANGO_DE_FOLIO`. **No hay ninguna relación `DISPOSITIVO — RANGO_DE_FOLIO`.**
- `rango_de_folio` (diccionario `§14`) es `id_delegacion` + `id_tipo_documento` + `desde` + `hasta`. **No tiene dimensión de dispositivo.**

Con eso, dos dispositivos de la misma delegación que llevan el mismo rango descargado y están ambos sin red toman el mismo número siguiente.

**El caso concreto, y no es hipotético porque el propio inventario lo describe.** En la delegación de Tocoa, `PT-121` («Registro de la salida sin conectividad, en el predio», cliente de campo, `ACT-05`) emite documentos con folio desde la tableta de la caseta, y `PT-037` («Emisión anticipada para delegación sin cobertura», `ACT-10`) y `PT-123` («Digitación diferida») emiten desde el equipo del encargado. `PT-114` («hoja de bitácora con folio») se emite desde el teléfono del motorista. Son tres o cuatro dispositivos, la misma delegación, y el rango es uno solo. `RNF-21` exige **0 folios duplicados a nivel institución**.

Obsérvese que la prueba de verificación de `RNF-21` («cinco dispositivos, **cada uno con el rango de una delegación distinta**») pasaría igual, porque está escrita para el caso fácil. La prueba no cubre el caso que rompe.

**Qué corregir.** El rango se asigna en dos niveles: `rango_de_folio` a la delegación y `subrango_de_folio` (o un `id_dispositivo` en el propio rango) al dispositivo, con su propio saldo, su propio umbral de alerta y su devolución al reincorporarse. Y añadir a la batería de `RNF-21` la prueba de dos dispositivos de la **misma** delegación.

---

### `HB34-53` — La depuración de datos personales no alcanza los adjuntos, y el segmento solo cubre el manifiesto

**Severidad:** Crítico · **Artefactos:** `README.md §1 M-02, §8`, `diccionario §13` · **Contra:** `RNF-17`, `RN-51`

`RNF-17` fija el umbral sin margen: *«datos personales que sobrevivan la depuración en respaldos, **adjuntos**, registros técnicos o dispositivos de campo: **0**»*.

En el modelo, `ADJUNTO` es una entidad que cuelga de `EVENTO_BITACORA`, `EXPEDIENTE_INCIDENTE`, `LICENCIA_CONDUCIR` y del bloque `BTT` (`id_adjunto_original`). **No tiene ninguna relación con `SEGMENTO_DATO_PERSONAL`, ni un campo que clasifique su contenido.** No hay forma de saber qué adjuntos contienen dato personal ni de alcanzarlos con el `evento_depuracion`.

**El caso concreto.** `PT-123` «Digitación diferida desde el papel, **con foto del original**» — obligatoria por `RN-47` y por el `BTT` (`id_adjunto_original`, `Cd.` bloqueante). El original digitado es la hoja de manifiesto, con nombres y números de identidad manuscritos. A los cinco años se ejecuta la depuración: se vacía el `contenido` del `segmento_dato_personal`, se elimina la `sal_de_huella`, la cadena sigue verificando —y **los nombres siguen en el JPEG**, íntegros y consultables. La depuración fue cosmética exactamente en el punto donde el diseño se enorgullece de no serlo.

**Segunda parte del mismo hallazgo: el alcance del segmento.** El `README §16` de las reglas registra que `RN-51` fue ampliada a *«terceros de siniestro y al dato de salud del servidor»* (`CE-03`, `CE-10`). En el modelo, el dato personal separable existe **solo** para `linea_manifiesto`. Quedan fuera, en claro y sin plazo de retención propio:

- `EXPEDIENTE_INCIDENTE }o--o| PERSONA : "involucra"` — el tercero lesionado en un accidente, que no es empleado y no está en el espejo de Talento Humano.
- `RESTRICCION_MEDICA` colgando de `MOTORISTA` — dato de salud del servidor, la categoría que el propio `segmento_dato_personal` marca como exigente de `base_legal_del_campo`.
- `FIRMANTE_ACTA` — quien firma un acta de entrega sin ser de la institución.

**Qué corregir.** `adjunto` necesita `clasificacion_de_contenido` y, cuando contenga dato personal, referencia a su `segmento_dato_personal` con la misma indirección que `linea_manifiesto`; la depuración lo reemplaza por una constancia de depuración con la huella del original. Y el segmento debe poder colgar de incidente, restricción médica y firmante, no solo de línea de manifiesto.

---

## Altos

### `HB34-54` — El diagrama de la Vista 5 contiene la relación que su propia nota prohíbe

**Severidad:** Alto · **Artefacto:** `README.md §7` · **Contra:** `RN-51`, `RNF-17`

El `erDiagram` de la Vista 5 incluye:

```
OBJETO_DEL_TRASLADO |o--o{ PERSONA : "identifica a"
```

Y ocho líneas más abajo, en la misma sección: *«La identificación de personas trasladadas **no está en `OBJETO_DEL_TRASLADO`**: está en `MANIFIESTO_PERSONA_EXTERNA`, cuyo contenido personal vive en `segmento_dato_personal`»*. La nota del diccionario `§4` lo repite: *«El objeto del traslado **no contiene identidades**»*.

El diagrama es lo que alguien va a traducir a esquema. Tal como está, existe un camino directo `objeto_del_traslado → persona` que salta la indirección completa sobre la que descansa `HB34-53` y toda la minimización de `RN-51`.

**Qué corregir.** Eliminar la relación del diagrama. Si lo que se quería expresar es el vínculo con el personal institucional trasladado (que sí es persona del espejo y no dato minimizable), nombrarla distinto y acotarla al subtipo `PERSONAL_INSTITUCIONAL`, nunca al supertipo.

---

### `HB34-55` — Cardinalidades mínimas obligatorias que el ciclo de vida no puede satisfacer

**Severidad:** Alto · **Artefactos:** `README.md §5, §7, §12`, `diccionario §2` · **Contra:** `estados/orden-de-mision.md §2, §3.1, §10.2`

`||--|{` significa «al menos uno, obligatorio». Seis relaciones lo declaran donde el ciclo de vida garantiza cero:

| Relación | Estado en que se rompe |
|---|---|
| `ORDEN_MISION ||--\|{ TRAMO_MISION` | Toda orden en `BORRADOR`, `SOLICITADA` y `APROBADA` no tiene tramo: el vehículo se asigna en `T-08`. Y una orden `RECHAZADA` (`T-06`) o `ANULADA` antes de programar (`T-03`, `T-07`, `T-09`) no tendrá tramo **nunca** |
| `ORDEN_MISION ||--\|{ VERSION_ALCANCE_AUTORIZADO` | El propio `§12` dice *«la primera versión nace con la aprobación»*. En `BORRADOR` y `SOLICITADA` no hay ninguna; en `RECHAZADA`, jamás la habrá |
| `VERSION_ALCANCE_AUTORIZADO ||--\|{ VALOR_CONGELADO` | Una misión urbana sin peajes, sin fondo y sin umbral aplicado no congela nada. Y ver `HB34-59`: si el congelamiento ocurre al despachar, la versión 1 pasa semanas sin valores |
| `TRANSICION_ORDEN_MISION ||--\|{ RESULTADO_VERIFICACION` | `T-01` (crear borrador) no evalúa ningún `BD-nn`. Además **el diccionario `§2` declara la misma relación como `1 : 0..N`** — el diagrama y el diccionario se contradicen entre sí |
| `VEHICULO ||--\|{ CUSTODIA_VEHICULO` | `§10.2` enumera *«sin custodio»* como causa tipificada válida de `NO_DISPONIBLE`. Un vehículo donado que llega al predio antes de designar custodio es un alta legítima que este mínimo impide |
| `VEHICULO ||--\|{ ASIGNACION_CATEGORIA_PEAJE` y `||--\|{ SERIE_INSTRUMENTO_MEDICION` | `HU-024` sitúa la categoría de peaje resuelta como requisito **para programar**, no para dar de alta. Y un vehículo recibido con el odómetro inutilizado (`CE-22`) no tiene serie con `lectura_inicial_de_serie` conocida |

No es purismo de notación: un mínimo obligatorio mal puesto se convierte en una restricción de integridad que **bloquea el alta**, y el usuario resuelve inventando el dato —una serie de odómetro falsa, un custodio que no existe—, que es la corrupción que el modelo entero intenta evitar.

**Qué corregir.** Bajar los seis a `o{` y expresar la obligatoriedad **donde vive de verdad**: como precondición de la transición correspondiente (`BD-07` para el vehículo al programar, `T-08` para el tramo, `T-05` para la versión de alcance), que es lo que la máquina de estados ya dice.

---

### `HB34-56` — `RESERVA_RECURSO` cuelga obligatoriamente de la Orden de Misión, y `D-10` dice que también la ocupan préstamos e indisponibilidades

**Severidad:** Alto · **Artefactos:** `README.md §7 (V5), §12, D-10` · **Contra:** `RN-13`, `RN-60`, `RN-63`, `estados §10.2`

El diagrama V5 declara `ORDEN_MISION ||--o{ RESERVA_RECURSO : "compromete"`, que en `erDiagram` significa: cada reserva pertenece a **exactamente una** orden de misión.

Y la decisión `D-10`, tres páginas después, dice: *«`RESERVA_RECURSO` … la ocupan misiones, **préstamos e indisponibilidades sobrevenidas**»*, y el `§12` lo repite: *«las ocupan también préstamo e indisponibilidad sobrevenida, no solo misiones (`RN-60`, `RN-63`)»*.

**El caso concreto.** Se presta un pickup a la Dependencia de Salud por diez días (`W-16`, `PRESTAMO_VEHICULO`, `RN-63`). Esa reserva debe ocupar la ventana para que `RN-13` impida programarlo, y no tiene ninguna orden de misión a la cual colgarse — `RN-63` es explícita: el préstamo *«nunca es una Orden de Misión»*. Con la cardinalidad dibujada, esa reserva no se puede escribir, y el estado `PRESTADO` deja de ocupar ventana, que es justo lo que `CE-14` obligó a corregir.

**Qué corregir.** `RESERVA_RECURSO` necesita origen polimórfico —`tipo_de_origen` ∈ {`MISION`, `PRESTAMO`, `INDISPONIBILIDAD`, `MANTENIMIENTO`} más `id_origen`— y la relación desde `ORDEN_MISION` pasa a `}o--o|`.

---

### `HB34-57` — `orden_mision.id_folio` obligatorio contradice `EF-02`, y no puede representar el folio anulado

**Severidad:** Alto · **Artefacto:** `diccionario §1` · **Contra:** `estados §5 EF-02`, `RNF-21`

El diccionario declara `orden_mision.id_folio` como **`Ob.`** — *«sin él, el registro no se guarda»*, según la propia convención `§0.2`. Dos problemas:

1. `EF-02` (autoridad) dice que el folio **se reserva en `T-08` programar** y se consume en `T-12` despachar. Una orden en `BORRADOR` existe desde `T-01`, y `T-01` es ejecutable **sin conectividad**. Exigir folio para guardar la orden hace imposible crear una solicitud en campo. La propia columna «Qué pasa si falta» lo admite sin darse cuenta: dice *«no se puede imprimir ni despachar»*, que es un bloqueo de `T-12`, no de creación.
2. `EF-02` también dice: *«`T-11` desprogramar → el folio reservado se **anula**. Al reprogramar se toma uno nuevo»*. Un campo escalar `id_folio` **no puede guardar los dos**: al reprogramar se sobrescribe, y el folio anulado se convierte en un hueco sin explicación — exactamente el `0` que `RNF-21` exige (*«huecos en la numeración sin explicación registrada: 0»*).

**Qué corregir.** Quitar `id_folio` de `orden_mision`. La relación ya existe y es la correcta: `ORDEN_MISION ||--o{ DOCUMENTO_EMITIDO` y `FOLIO ||--o| DOCUMENTO_EMITIDO`, más `FOLIO ||--o{ EVENTO_ESTADO_FOLIO` para el ciclo `RESERVADO → ANULADO`. El campo escalar es un atajo que duplica y contradice esa estructura.

---

### `HB34-58` — Muchas transiciones a un solo asiento de auditoría: la cardinalidad está invertida

**Severidad:** Alto · **Artefacto:** `README.md §7 (V5)` · **Contra:** `RNF-04`

```
TRANSICION_ORDEN_MISION }o--|| ASIENTO_AUDITORIA : "produce"
```

Leído como `erDiagram`: **N transiciones se atribuyen a exactamente 1 asiento**. `RNF-04` exige lo contrario y sin margen: *«transacciones que producen asiento de auditoría: **100 %**. Ninguna operación de negocio escribe sin dejar asiento»*.

Con la cardinalidad dibujada, veinte transiciones podrían compartir un asiento y la cobertura sería del 5 %. Es casi con seguridad un error de tipeo de `||--||`, pero es el diagrama el que se traduce a esquema.

**Qué corregir.** `TRANSICION_ORDEN_MISION ||--|| ASIENTO_AUDITORIA`. Y revisar en el mismo pase `ASIENTO_AUDITORIA }o--|| AUTORIA_CONGELADA`, que sí admite reutilización de la autoría pero convendría declarar si se reutiliza por acto o por sesión, porque decide si `autoria_congelada` es dato de un hecho o de un actor.

---

### `HB34-59` — No está decidido en qué transición se congela el paquete normativo

**Severidad:** Alto · **Artefactos:** `README.md §7 (V5), D-08`, `diccionario §12` · **Contra:** `estados §5 EF-03`, `RN-41`, `CLAUDE.md` premisa 6

Tres fuentes, tres momentos:

- `CLAUDE.md` y `RN-41`: *«el valor calculado se congela **al autorizar**, junto con el identificador de la tabla usada»*.
- `EF-03` (autoridad en efectos de transición): *«**al pasar a `DESPACHADA`** se registran los identificadores y versiones de: tarifas de peaje, categoría de peaje, calendario, matriz licencia↔vehículo, rendimiento esperado, umbrales, holguras y plazos»*.
- El modelo: `VERSION_ALCANCE_AUTORIZADO ||--|{ VALOR_CONGELADO : "congela"`, y la versión 1 nace en `T-05` (aprobar). O sea, congela al autorizar.

**El caso concreto.** Solicitud aprobada el 5 de enero. Programada el 28. Despachada el 3 de febrero. La tarifa de peaje cambia el 20 de enero, y `NRM-10` documenta que en 2026 hubo tres reversiones tarifarias en dos meses. ¿Qué tarifa lleva impresa la orden que el motorista recibe el 3 de febrero, la de la aprobación o la vigente al despachar? `RN-91` exige que la orden impresa lleve *«la tarifa esperada del paquete congelado»* para que el motorista discuta en la caseta con el papel en la mano. Si el paquete es el de enero, el papel está mal y el motorista pierde la discusión.

Y hay un tercer momento que ninguna fuente cubre: `RN-61`, *«la sustitución de vehículo recalcula y vuelve a congelar todo valor derivado, con asiento de diferencia»*, que ocurre en `T-10` o incluso `EN_RUTA`. Un `valor_congelado` obligatoriamente colgado de una `version_alcance_autorizado` no tiene dónde alojar el recongelamiento por sustitución, que no cambia el alcance autorizado.

**Qué corregir.** Declararlo en el modelo: `valor_congelado.id_acto_que_congela` ya existe y admite transición **o** autorización, así que la estructura aguanta; lo que falta es (a) romper la obligatoriedad de colgar de `version_alcance_autorizado` y (b) una decisión explícita —recomendación: **estimación indicativa al autorizar** (`RN-35` la pide para decidir) y **congelamiento vinculante al despachar** (`EF-03`), ambos como `valor_congelado` con `concepto` distinto, nunca uno pisando al otro.

---

### `HB34-60` — `BD-01` exige el solicitante de derecho y ninguna entidad lo guarda

**Severidad:** Alto · **Artefactos:** `diccionario §1`, `README.md §7` · **Contra:** `estados §4 BD-01`, `RN-02`, `HB3-01`

`BD-01` fue corregido tras el hallazgo `HB3-01` para bloquear el escenario cotidiano —la asistente captura, el jefe autoriza— comparando al autorizador contra **tres** personas: quien creó, quien envió, y **el solicitante de derecho**.

En el modelo, `solicitud_transporte` figura en el diagrama V5 y en la lista de «otras entidades» del `§19`, **sin diccionario**. `orden_mision` no tiene el campo. `id_unidad_organizativa_requirente` identifica la unidad, no a la persona. `autoria_congelada` congela a quien ejecuta el acto, no a aquel por cuenta de quien se ejecuta.

Resultado: el dato sobre el que descansa el bloqueo duro más cotidiano del sistema no tiene dónde vivir, y `HU-003` («captura por encargo y solicitante de derecho») no es implementable.

**Qué corregir.** `solicitud_transporte` entra al diccionario con `id_persona_solicitante_de_derecho` obligatorio —igual al capturador cuando no hay encargo— y `BD-01` compara contra los tres. Es un campo; el costo de agregarlo hoy es nulo y el de descubrirlo en implementación es un control que no controla.

---

### `HB34-61` — Reglas `RN-xx` que exigen un dato que ninguna entidad guarda

**Severidad:** Alto · **Artefactos:** ambos documentos del modelo · **Contra:** las reglas citadas

Búsqueda dirigida sobre `RN-55` a `RN-97`. Seis reglas no tienen soporte:

| Regla | Efecto declarado | Qué falta |
|---|---|---|
| **`RN-78`** grado de cumplimiento del objeto | **Bloqueo duro** — *«toda misión cierra declarando el grado de cumplimiento de su objeto, por destino y consolidado»* | Ni la palabra «cumplimiento» aparece en el modelo. `liquidacion_mision` tiene `linea_liquidacion`, `conciliacion` y `desviacion`, todas económicas. **No se puede ejecutar `T-21` sin este dato y no hay dónde escribirlo** |
| **`RN-56`** prelación entre solicitudes | Derivación + bloqueo — *«aplica el criterio parametrizado y **deja constancia de las desplazadas**»* | No existe entidad de conflicto de recurso ni de desplazamiento. `EF-01` dice que *«cada conflicto registrado, con su resolución, es la medición del déficit de flota»* y que es *«uno de los pocos indicadores llevables a una gestión presupuestaria con evidencia»*. Sin entidad, ese indicador no existe. Arrastra a `RN-82` |
| **`RN-96`** cierre de ejercicio y **`RN-97`** saldo de apertura | Bloqueo duro ambas | No hay entidad `ejercicio` / `periodo_fiscal`. El `§14` de `folio` invoca *«el invariante de cierre de ejercicio»* contra un objeto que no existe. `RN-97` («lo no terminal al corte constituye el saldo de apertura, con antigüedad desde el hecho») no tiene ni corte ni saldo |
| **`RN-73`** convalidación de actos sin autorización previa | Bloqueo | `orden_mision.id_solicitud_transporte` nulo *«obliga a expediente de convalidación con cronología declarada»*. **Ese expediente no es ninguna entidad.** `CE-01` y `HU-008` dependen de él |
| **`RN-88`** saldo proyectado del fondo | Cálculo + advertencia | `fondo_combustible` no tiene comprometido proyectado, y la alerta de `RN-88` se dispara *sobre el proyectado*, no sobre el disponible |
| **`RN-21`** ampliada por `CE-18` | Bloqueo | *«Peso y ocupación **efectivos** con indicador de desviación»*. `objeto_del_traslado` solo tiene `peso_declarado` y `cantidad_personas` declarados. Lo efectivo se constata al despachar y no tiene campo |

**Qué corregir.** Cuatro entidades nuevas (`cumplimiento_de_objeto` por destino, `conflicto_de_recurso` con su resolución, `ejercicio` con su corte, `expediente_convalidacion`) y dos campos (`peso_efectivo`/`ocupacion_efectiva` en el despacho, comprometido proyectado en el fondo). Ninguna es estructural; todas son necesarias para que reglas ya declaradas de bloqueo duro sean ejecutables.

---

## Medios y bajos

### `HB34-62` — `ACTA` no tiene dueño polimórfico y hay actas que no caben en ningún lado

**Severidad:** Medio · **Artefactos:** `README.md §5, §8`

`ACTA` aparece colgando de `TRAMO_MISION ||--o{ ACTA` (V6) y de `CUSTODIA_VEHICULO ||--|| ACTA` (V3). No hay más anclajes.

Actas exigidas por artefactos vigentes que no cuelgan de un tramo ni de una custodia:

- **Acta de devolución del fondo** de `EF-06`, firmada por `ACT-07` y el motorista, en una anulación `T-15` de misión **despachada pero no salida**: no hay tramo, porque el tramo abre con `INICIO_DE_MISION` en la salida.
- **Acta de anulación de vale** (`RN-04`, `PT-049`), que cuelga de `asignacion_combustible`.
- **Acta de préstamo** y **acta de devolución del préstamo** (`RN-63`, `W-16`/`W-17`).
- **Acta de descargo** (`W-14`) y **acta de devolución al comodante** (`W-19`).
- **Acta de constatación física** (`PT-124`, `RN-18`).
- `ACTA_CIERRE_ASIGNACION` de un puesto (V1) — que además está modelada como entidad aparte, duplicando el concepto.

**Qué corregir.** `acta` con `tipo_de_objeto` + `id_objeto`, como ya se hizo bien en `alcance_de_datos`. Y fusionar `ACTA_CIERRE_ASIGNACION` en `acta` con su `tipo_acta`.

### `HB34-63` — El resumen de entidades por módulo omite 41 de las 167 que dibujan las vistas

**Severidad:** Medio · **Artefacto:** `diccionario §19`

Las nueve vistas contienen **167 entidades distintas**. El `§19`, presentado como el inventario por módulo, enumera 126. Faltan 41, y varias son portadoras de invariantes:

`objeto_en_tramo` (la entidad de la que depende `RN-68`, compatibilidad por tramo) · `desenlace_interrupcion` (bloqueo duro de `RN-70`) · `acta_traspaso` (bloqueo duro de `RN-71`) · `hallazgo_de_cierre` (`CERRADA_CON_HALLAZGO`) · `valor_anterior_y_nuevo` (dos de los seis campos obligatorios de `RNF-04`) · `entrada_matriz_licencia_vehiculo` (`BD-02`) · `evento_estado_folio`, `evento_estado_asignacion`, `evento_estado_fondo` y `evento_estado_reintegro` (los diarios sobre los que descansa la decisión `D-03`) · `linea_liquidacion` · `kilometraje_acumulado` · `tarifa_peaje` · `fecha_corte_conocimiento` (`RNF-06`).

Como el `§19` es la lista que el Sprint 1 va a usar para saber qué falta documentar, lo omitido se queda sin diccionario dos veces.

### `HB34-64` — `§15` declara `[C]` que no se registraron en `insumos-pendientes.md`

**Severidad:** Bajo · **Artefacto:** `README.md §15` · **Contra:** `CLAUDE.md`

El `§15` abre con *«Ninguno se inventó. Todos están **o entran** en `insumos-pendientes.md`»*. El commit del Bloque 4 no modificó `insumos-pendientes.md`. El punto 6 («solape máximo en días entre titular saliente y entrante») cita `actores-y-roles §2.3` en lugar de un número de insumo, y no tiene entrada propia. `CLAUDE.md` es explícito: *«márcalo `[C]` **y regístralo**»*. «O entran» no es registrarlo.

---

# Parte 2 — Navegación e inventario de pantallas

## `HB34-65` — El recuento correcto es 28 / 9 / 89, y el desglose por cliente subcuenta el campo

**Severidad:** Medio · **Artefacto:** `inventario-de-pantallas.md §5 y encabezado`

Recuento sobre las 126 filas, columna a columna:

| Columna «Papel» | Declarado en `§5` | **Real** |
|---|---|---|
| `Sí` — bloqueadas por insumo #2 | 27 | **28** |
| `Parc.` — parcialmente bloqueadas | 8 | **9** |
| `No` — se diseñan ya | 91 | **89** |
| Total | 126 | 126 ✓ |

Las 28 son: `PT-007`, `020`, `023`, `024`, `034`, `035`, `036`, `037`, `039`, `040`, `041`, `042`, `044`, `047`, `048`, `049`, `074`, `077`, `080`, `081`, `094`, `106`, `114`, `118`, `121`, `122`, `123`, `124`. **La propia `§5.2` las enumera todas 28** bajo el título «Las 27 bloqueadas»: la tabla del `§5` es la que está mal, no la lista.

Las 9 parciales son `PT-008`, `012`, `062`, `065`, `076`, `083`, `105`, `109`, `119`. La `§5.2` solo asigna formato a ocho: **`PT-062` («Relevo de motorista en ruta: resolución desde oficina») está marcada `Parc.` y no aparece en la lista de formatos por pedir.** Su formato es el acta de relevo, ya listada para `PT-118`.

**Segundo error, en el encabezado.** «126 pantallas · 102 administrativo · 23 campo · 1 pública». El conteo real por columna `Cli` es `A`=95, `A/C`=7, `C`=23, `P`=1. La suma cuadra solo si las 7 duales se cuentan como administrativas. Pero **son superficies que hay que diseñar también para el cliente de campo**, y el `mapa §0.2` es tajante en que son dos productos distintos y no uno responsive. Las superficies de campo a diseñar son **30**, no 23: las 23 de la `§3` más `PT-004`, `037`, `040`, `041`, `042`, `094`, `095`. Es un 30 % más de trabajo de campo del que el diseñador está planificando, y el de campo es el que tiene las restricciones duras.

**Qué corregir.** Tabla del `§5`: 28 / 9 / 89. Encabezado: «95 solo administrativo · 7 duales · 23 solo campo · 1 pública», y decir en la `§5.1` que las superficies de campo a diseñar son 30. Añadir `PT-062` a la `§5.2`.

## `HB34-66` — Quince historias de `M-17` no tienen ninguna pantalla, y el inventario no lo declara

**Severidad:** Alto · **Artefactos:** `inventario-de-pantallas.md §2.15 y §7` · **Contra:** `docs/02-requisitos/historias/`, `RN-51`, `RN-52`, `RNF-17`

El inventario referencia `HU-001` a `HU-110`. **`HU-111` a `HU-125` no aparecen en ninguna fila.** Son las quince del módulo M-17, escritas en el mismo commit que el inventario.

La `§2.15` «M-17 Traslado de personas externas» tiene dos pantallas, y una de ellas (`PT-095`) no cita ninguna historia; la otra (`PT-094`) cita `HU-031`, que es del juego documental, no del manifiesto.

Quedan sin superficie, entre otras:

| Historia | Qué queda sin pantalla |
|---|---|
| `HU-112` fundamentar campo sensible | La base legal expresa que `segmento_dato_personal.base_legal_del_campo` exige, y sin la cual `RNF-17` fija el umbral en cero |
| `HU-113` persona sin documento de identidad | El caso operativo más frecuente del traslado de personas externas |
| `HU-116` novedades del manifiesto en ruta | `RN-53`: el manifiesto se cierra al despachar y los cambios son novedad. Sin pantalla en el cliente de campo, se resuelven a mano |
| `HU-119` reporte de accesos y alerta de patrón anómalo | El control que hace útil el registro de consultas de `RN-52` |
| `HU-121`, `HU-122` hábeas data buscar, exportar y rectificar | `RNF-17` exige *«≤ 5 min desde la interfaz, sin intervención de desarrollo»*. Sin pantalla, es intervención de desarrollo por definición |
| `HU-124` depurar sin romper la cadena | La operación completa de `evento_depuracion`, con su aviso previo obligatorio |

Y en sentido inverso: `PT-093` («Registro de consultas a datos de personas externas») no tiene ni CU ni HU, cuando `HU-118` y `HU-119` son exactamente eso. **La trazabilidad está rota en las dos direcciones para el mismo módulo.**

Lo agravante es que la `§7` «Lo que este inventario no cubre» declara honestamente los huecos de M-11, M-12 y M-18 — **y no menciona M-17**. Un hueco declarado es una decisión; uno no declarado es un olvido que nadie va a buscar.

**Qué corregir.** Trazar `PT-093`, `PT-094`, `PT-095` a sus historias reales, e inventariar las pantallas faltantes de M-17 (mínimo: fundamentación de campo sensible, novedad en ruta, reporte de accesos, hábeas data, depuración). O declarar el hueco en la `§7` con su motivo.

## `HB34-67` — Catorce pantallas sin historia; y el ciclo de vida del parámetro normativo no tiene ni caso de uso ni historia

**Severidad:** Alto · **Artefacto:** `inventario-de-pantallas.md §2.16, §2.14`

Sin historia: `PT-001`, `002`, `005`, `088`, `090`, `092`, `093`, `095`, `096`, `098`, `099`, `100`, `101`, `102`. De esas, nueve tampoco tienen caso de uso.

Dos son legítimas y están bien resueltas porque citan su `RNF` en lugar de una historia: `PT-101` (`RNF-20`) y `PT-102` (`RNF-09`). El resto no.

**El grave es el trío `PT-099` + `PT-100` + `PT-092`:** carga del parámetro normativo con vigencia y respaldo, aprobación de su puesta en vigencia con doble control, e histórico de cambios. Es el mecanismo del que cuelga **todo `RNF-05`**, el invariante `M-01` del modelo de datos, `RN-39` a `RN-42` y el doble control de `actores-y-roles §4.3`. No hay `CU-xx` ni `HU-xxx` para nada de eso: ni una historia menciona parámetros, catálogos, usuarios o puestos.

Consecuencia práctica: la pantalla desde la que se carga una tarifa de peaje —la que decide si `RNF-05` se cumple o se cablea— **no tiene un solo criterio de aceptación en Gherkin**, mientras que registrar un arribo tiene doce.

**Qué corregir.** Un `CU-19` de administración de parámetros normativos con doble control y sus historias, antes de que la implementación resuelva el doble control con un `if`.

## `HB34-68` — `PT-041` da a `ACT-05` una acción que la matriz le niega y que `I-08` bloquea duro

**Severidad:** Alto · **Artefacto:** `inventario-de-pantallas.md §2.7` · **Contra:** `actores-y-roles.md §4 fila 10 y §5.2 I-08`, `estados EF-04`

```
| PT-041 | Entrega del fondo contra firma, dentro del despacho | A/C | ACT-07 ACT-05 | ...
```

- Matriz de permisos, acción 10 «Entregar fondo o vale al motorista», columna `05`: **`–` sin acceso**.
- `I-08` «Despacha × Entrega fondo», misma misión: **bloqueo duro**.
- `EF-04` (autoridad en efectos): *«quien entrega **no puede ser quien despacha** ni el motorista (`BD-06`)»*.

`PT-041` está descrita literalmente como *«dentro del despacho»* y asignada al despachador. Las tres autoridades dicen que no. El `mapa §5` la dibuja además como paso siguiente de `PT-040` en el flujo de `ACT-05`, con `ACT-05` recorriendo `PT-039 → PT-040 → PT-041` sin ningún nodo de bloqueo entre medio.

**Qué corregir.** `PT-041` es de `ACT-07`, presente en el acto de despacho. En el flujo de `ACT-05` el nodo debe ser *«espera la entrega del fondo por `ACT-07`»*, no una pantalla que él consuma. Y si lo que se quería expresar es que `ACT-05` la ve, la columna `Rol` necesita distinguir quién ejecuta de quién consulta —hoy no lo hace, y por eso este error es indetectable leyendo la tabla.

## `HB34-69` — La navegación de `ACT-10` se apoya en un régimen de excepción que `DP-002` suspendió

**Severidad:** Alto · **Artefacto:** `mapa-de-navegacion.md §8.2` · **Contra:** `actores-y-roles.md §4 notas 4 y ⛔`, `DP-002`, `RNF-14`

El flujo de `ACT-10` Encargado de Delegación ofrece, desde una sola raíz y sin ninguna compuerta declarada: `PT-122` capturar solicitud (solicita) → `PT-121` registrar salida (despacha) → `PT-041` entregar el fondo (entrega) → `PT-042` registrar retorno.

En la matriz de permisos, las tres celdas de `ACT-10` que lo permiten —acción 6 despachar, acción 10 entregar fondo, acción 13 liquidar— están marcadas **`E⁴`**, y la nota 4 dice: *«Solo bajo **régimen de excepción declarado** por insuficiencia de personal, con convalidación posterior»*.

Ese régimen **no existe**. `DP-002` lo suspendió, las acciones 27 y 28 están tachadas con ⛔ y `RNF-14` lo dice sin rodeos: *«la consecuencia práctica es que las delegaciones pequeñas no podrán despachar dentro del sistema mientras ese insumo siga abierto (#26). Es un riesgo de despliegue, no un detalle de configuración»*.

El mapa sí tiene el nodo `N` «Bloqueo + escalamiento a sede», pero cuelga de una rama genérica («acto que su puesto no puede consumar»), no de las tres ramas concretas que hoy están bloqueadas. Un diseñador que lea ese diagrama va a dibujar la delegación operando de punta a punta, que es precisamente el escenario que hoy no se puede construir.

**Qué corregir.** Marcar en el `§8.2` las tres ramas como condicionadas al insumo #26, con el escalamiento como camino por defecto y no como excepción. Es además el hallazgo con mayor consecuencia de despliegue de todo el bloque: si el insumo #26 se resuelve en contra, hay que rediseñar la navegación del actor que sostiene la operación rural.

## `HB34-70` — `PT-104` se usa como dos raíces distintas

**Severidad:** Medio · **Artefactos:** `inventario §3` contra `mapa §8.2`

- Inventario: `PT-104` = *«**Mi misión** — raíz única del cliente de campo»*, y el `mapa §6` insiste: *«el motorista tiene exactamente **una** misión activa. Si tiene dos, algo se hizo mal antes»*.
- `mapa §8.2`: `PT-104 Mi delegación hoy — raíz · misiones, pendientes, papeles por digitar`.

Son dos pantallas con propósitos opuestos —una misión sin menú contra un tablero de varias misiones y una cola de digitación— compartiendo identificador. O falta una pantalla en el inventario (y el total es 127, no 126), o el mapa está reutilizando un ID que `CLAUDE.md` declara estable y no reciclable.

**Qué corregir.** `PT-127` «Mi delegación hoy», raíz de `ACT-10` en el cliente de campo. Los IDs no se reciclan ni se comparten.

## `HB34-71` — `PT-105` está marcada como parcialmente bloqueada y a la vez declarada diseñable hoy

**Severidad:** Medio · **Artefactos:** `inventario §5.1 y §5.2`, `mapa §7`, `brief-para-diseno.md`

`PT-105` («Registro en ruta») tiene `Papel = Parc.` en la tabla, y la `§5.2` la lista como parcial del formato «Bitácora de vehículo». Pero:

- `inventario §5.1`: *«las cinco pantallas difíciles no replican papel … y **todas se pueden diseñar hoy**»*.
- `mapa §7`: *«Ninguna replica papel: **las cinco se pueden diseñar hoy**»*.
- `brief`: la sitúa como trabajo #3 disponible desde el primer día.

Es la pantalla que el propio mapa llama *«la navegación más importante del sistema»* y de la que dice que *«decide la adopción»*. Que el diseñador descubra a mitad del trabajo que su bloque de campos depende del insumo #2 es exactamente lo que la partición del inventario buscaba evitar.

**Qué corregir.** Decidir cuál es cierta. La lectura razonable es que la parte difícil de `PT-105` —los tres botones, el odómetro, la confirmación sin red— **no** replica papel y sí se diseña hoy, y que lo que espera al formato es el bloque de campos de la bitácora. Decirlo así en las tres partes, en lugar de que dos digan `No` y una diga `Parc.`.

## `HB34-72` — `PT-020` y `PT-024` producen documento oficial con folio y no están en la lista del `§6`

**Severidad:** Bajo · **Artefacto:** `inventario §6`

El `§6` («Todo formato impreso, sin excepción») enumera las pantallas cuyos documentos llevan folio, QR, firma, sello y hash. No incluye `PT-020` (trámite del permiso de circulación en día u hora inhábil) ni `PT-024` (reemisión del permiso por cambio de elementos amparados), ambas marcadas `Papel = Sí`.

`PT-024` es la más sensible: una reemisión es, según el modelo, **un documento nuevo con folio nuevo que declara «sustituye al folio X»** (`DOCUMENTO_EMITIDO |o--o| DOCUMENTO_EMITIDO`), y el anterior queda `ANULADO`. Si esa pantalla no está en la lista de formatos con folio, el diseño va a producir una edición del permiso vigente, que es el `0` que `RNF-21` prohíbe.

## `HB34-73` — `PT-124` asigna a `ACT-13` una captura que la matriz le concede solo como consulta

**Severidad:** Bajo · **Artefacto:** `inventario §3`

`PT-124` («Constatación de la identificación institucional del vehículo», `RN-18`, `HU-100`) figura con roles `ACT-14` `ACT-13`. La acción 23 de la matriz, «Mantener expediente y vencimientos del vehículo», da `ACT-13` = **`C`** consulta y `ACT-14` = `E`. La constatación con fecha y fotografía es un asiento sobre el expediente, no una consulta.

Puede ser correcto —el custodio es quien tiene el vehículo delante— pero entonces la matriz necesita una acción propia o una nota, no la pantalla decidiéndolo por su cuenta.

## `HB34-74` — El artefacto `mockups/README.md` y sus diez hallazgos no existen en el repositorio

**Severidad:** Medio (de gestión) · **Artefacto:** ausente

El encargo de esta revisión da por entregado `docs/04-diseno/mockups/README.md` con diez hallazgos en su `§5`, y en particular un hallazgo `5.4` en que el diseño habría reportado por su cuenta el descuadre del recuento.

**No existe.** No hay directorio `mockups/`, ni `wireframes/`, ni `formatos-impresos/` —los tres que el `04-diseno/README.md` anuncia—, ni ningún documento del repositorio que contenga hallazgos de diseño. El commit `8908900` del Bloque 4 entregó cuatro archivos: `README.md`, `brief-para-diseno.md`, `inventario-de-pantallas.md` y `mapa-de-navegacion.md`.

Lo digo porque cambia la lectura de dos cosas: el descuadre del recuento **no fue autorreportado** (lo encontró esta revisión, `HB34-65`), y los diez hallazgos que se creen devueltos por diseño **no están abiertos ni cerrados: no se escribieron**. Si esa conversación ocurrió, `CLAUDE.md` es explícito: *«lo que no queda escrito, se pierde»*.

---

# Cierre

## Qué revisé y qué no

**Revisado línea a línea.** Las nueve vistas `erDiagram` y sus 167 entidades, las 20 secciones del diccionario, las 30 cardinalidades justificadas del `§12`, las 12 decisiones `D-01` a `D-12`, las 126 filas del inventario recontadas por las cuatro columnas, los ocho diagramas de navegación por rol, la matriz de 28 acciones × 14 roles, los pares `I-01` a `I-19`, la máquina de estados completa (`T-01` a `T-22`, `BD-01` a `BD-11`, `EF-01` a `EF-07`, `V-01` a `V-10`, `W-01` a `W-19`), los umbrales de `RNF-03`, `04`, `05`, `06`, `14`, `17`, `21`, y la cobertura `HU` cruzada en las dos direcciones sobre las 125 historias.

**No revisado.** El contenido de las 97 reglas una por una —solo el índice y las que los hallazgos citan—; los 28 casos especiales completos (leí `CE-10` íntegro y el índice del resto); las 18 fichas de caso de uso; las 10 fichas de normativa; el `backlog.md`. Tampoco evalué la calidad de la redacción para el diseñador externo, que me parece la mejor de todo el repositorio y no necesita ayuda.

## Lo que más me preocupa, en una frase

Que `§16` del modelo afirme por escrito que los cinco `RNF` irreversibles están resueltos, porque esa frase es la que va a hacer que nadie los vuelva a mirar hasta que haya tres años de cadena construida sin eje de conocimiento, sin alcance definido y con los nombres de los pasajeros intactos dentro de los adjuntos.

## ¿El modelo de datos soporta empezar a programar?

**Sí para una rebanada vertical, no para el sistema.** Concretamente:

**Se puede empezar hoy**, y es lo que yo empezaría: el núcleo de la Orden de Misión —`orden_mision`, `transicion_orden_mision`, `version_alcance_autorizado`, `tramo_mision`, `objeto_del_traslado`, `evento_bitacora`— más el expediente del vehículo y el diario de sincronización. Esas entidades tienen diccionario campo por campo, dominio, obligatoriedad y regla de origen; son suficientes para construir el recorrido `T-01 → T-22` con el cliente desconectado. Las decisiones difíciles que suelen descubrirse tarde —estado como proyección del diario, imputación por tramo, fecha del hecho separada de la de captura, identidad en el cliente— ya están tomadas y están bien.

**No se puede empezar sin resolver antes cuatro cosas**, porque son las que no se pueden agregar después y las cuatro tocan la primera línea de código que se escriba:

1. El segundo eje temporal de `version_tabla_parametrica` (`HB34-50`).
2. El alcance, el momento y el orden del encadenamiento de `asiento_auditoria` (`HB34-51`).
3. El subrango de folio por dispositivo (`HB34-52`).
4. La clasificación y el alcance depurable de `adjunto` (`HB34-53`).

Ninguna de las cuatro es cara hoy: son un rango de fechas, una decisión de alcance, un nivel de partición y un campo de clasificación. Las cuatro son carísimas después, y esa asimetría es la razón entera por la que el Bloque 4 se escribió antes que el código.

**Deuda técnica que hay que nombrar.** El `§19` deja fuera del diccionario unas 126 de las 167 entidades y lo declara pendiente del Sprint 1. Eso es aceptable y está bien declarado. **La señal para pagarla** es concreta: cuando la primera historia de un módulo distinto de M-06/M-07/M-08 entre a construcción. Si se empieza a programar M-09 o M-13 con esas entidades solo dibujadas en un `erDiagram`, los campos los va a inventar quien las implemente, y `RN-83` —la separación entre abastecimiento y consumo, que es de las mejores decisiones del modelo— va a durar hasta el primer atajo.
