# H-B1-002 — Revisión normativa adversarial del Bloque 1

| Campo | Valor |
|---|---|
| **Revisor** | Especialista en marco normativo y control interno |
| **Fecha** | 2026-08-06 |
| **Artefactos revisados** | `docs/01-negocio/reglas/` (RN-01 a RN-53), `docs/01-negocio/actores-y-roles.md`, `docs/01-negocio/procesos/PR-01-movilizacion-institucional.md`, `docs/03-arquitectura/estados/orden-de-mision.md` |
| **Contrastados contra** | `NRM-01` a `NRM-10`, `riesgos-normativos.md`, `DP-001`, `ADR-001`, `insumos-pendientes.md` |
| **Estado** | **14 de 20 corregidos y verificados. 6 siguen abiertos** — desglose en la sección siguiente |
| **Verificación de cierre** | 2026-08-26, contra los artefactos vivos |


## Estado de corrección — verificado el 2026-08-26

Verificado contra los artefactos vivos, hallazgo por hallazgo. **Siete de los veinte siguen abiertos, y cinco de ellos por la misma causa: la obligación normativa existe y ninguna regla la implementa.** Eso no se corrige redactando: hay que escribir la regla.

### Corregidos y verificados — 14

| Hallazgo | Dónde se comprueba |
|---|---|
| `HN1-01` | [`DP-002`](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md) y el bloque ⛔ *«DISEÑADO, PERO NO SE IMPLEMENTA»* del Nivel 2 de [`actores-y-roles`](../../01-negocio/actores-y-roles.md) §5.4. Las acciones 27 y 28 de la matriz quedaron tachadas. **Ver la salvedad del Nivel 3 en `HB1-01`** de [`H-B1-001`](H-B1-001-revision-qa-bloque-1.md) |
| `HN1-02` · `HN1-03` · `HN1-05` · `HN1-06` | [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md): cubre la función *cerrar*, cita el núcleo irreductible seis veces, y el nivel de verificación dejó de escalar al bajar de la ficha a la regla |
| `HN1-04` | La matriz 8 × 8 de `orden-de-mision.md` §3.3 **se eliminó** por duplicar `I-01`–`I-17`. Una tabla copiada es una tabla que diverge |
| `HN1-07` | [`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) existe, y seis reglas citan hoy `NRM-04` |
| `HN1-08` | [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md): doble control carga↔aprobación, no desactivable |
| `HN1-10` | [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) |
| `HN1-12` | [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) separa hoy lo `[V]` de lo `[C]`: los elementos de identificación están verificados; la vigencia del Acuerdo 303 en su redacción original, no |
| `HN1-15` | [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) lleva su **nota de corrección** nombrando este hallazgo: la incompatibilidad solicita fondo × aprueba fondo pasó a ser control propio de `RN-26`, en vez de invocar un `RN-01` que razona por misión |
| `HN1-19` · `HN1-20` | [`README` de reglas](../../01-negocio/reglas/README.md) |
| `HN1-14` | **Cerrado el 2026-08-26.** [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) declaraba `[V]` que el MARCI exige control de acceso y registro de consultas, cuando [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) tiene esa familia `[C]`. **La cabecera contradecía a su propio cuerpo**, que ya decía lo correcto. La verificación se separó en sus tres afirmaciones: `[V]` el hábeas data del Artículo 182 — [`NRM-07`](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md); `[C]` la exigencia del MARCI; **`[I]` que del hábeas data se siga registrar cada consulta**, que es implicación de requerimiento del equipo y no articulado. Se fue más lejos que el `[P]` único que proponía el hallazgo, porque un `[P]` plano subestimaba el hábeas data y sobrestimaba la inferencia. **Sigue siendo bloqueo duro y no configurable**, y la regla lo dice expresamente para que la corrección no se lea como permiso para relajarlo. Alineados los cuatro artefactos que repetían la escalada: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) `V-03`, [`actores-y-roles`](../../01-negocio/actores-y-roles.md) §3.3 y `ACT-12`, [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) y [`HU-132`](../../02-requisitos/historias/HU-132-alcance-de-datos-verificado-en-cada-consulta.md) |

### Siguen abiertos — 6

| Hallazgo | Qué falta, comprobado hoy |
|---|---|
| `HN1-09` | **Sin regla.** No hay ninguna que produzca el paquete de evidencia **por vehículo o por período**. El sistema entrega por misión y el requerimiento del TSC llega por vehículo o por período. El propio informe lo puso primero en su lista de riesgo de auditoría |
| `HN1-11` | **Sin regla y sin norma extraída.** El bloqueo duro por matrícula vencida sigue sin `RN-xx` propia |
| `HN1-13` | [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) sigue declarando `Configurable: Sí` con efecto por catálogo, y la máquina de estados sigue bloqueando sin distinguir. Uno de los dos tiene que ceder |
| `HN1-16` | [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) sigue resolviendo la ausencia del superior por delegación vigente y rechazando el salto automático; `actores-y-roles` §7.3 sigue prescribiéndolo |
| `HN1-17` | [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) sigue exigiendo como bloqueo duro no configurable que exista una página pública de verificación del QR, cuando el pendiente **G** de `ACT-15` deja `[C]` si la institución acepta exponer ese punto desde un servidor on-premise |
| `HN1-18` | **Sin regla.** La constatación física de la flota es `[V]` —NOGECI V-15 y Circular CGR-010-2026— y sigue sin `RN-xx`. Es la más grave de las cuatro sin cubrir, porque la norma está verificada |
> **Nota de alcance.** Este documento no corrige artefactos. Señala. Ningún hallazgo se resuelve aquí, y ninguno de los `[C]` que se citan se resuelve por inferencia.

---

## 0. Valoración general

El Bloque 1 está muy por encima de la media de lo que una institución hondureña lleva a una auditoría. Las 53 reglas son verificables, la mayoría distingue norma de práctica, y hay tres puntos donde el equipo hizo exactamente lo que el TSC va a buscar: la conciliación en ambas direcciones (`RN-30`), la correlación peaje × kilometraje × ruta autorizada (`RN-37`), y la distinción entre fecha del hecho y fecha de captura (`RN-46`).

Los hallazgos que siguen no contradicen eso. Se concentran en tres patrones:

1. **El Bloque 1 tiene dos respuestas incompatibles a la pregunta que el auditor hará primero** (segregación en delegaciones pequeñas), y ambas están escritas como definitivas en artefactos distintos.
2. **La verificación se fortalece al bajar de la ficha a la regla.** Lo que `NRM-01` marca `[C]` llega a `RN-01` como `[V]`. Es el mecanismo exacto por el que una inferencia se convierte en obligación legal aparente.
3. **Hay bloqueos duros sin regla y sin norma extraída**, y obligaciones normativas vivas sin ninguna regla.

---

## 1. Hallazgos

### Severidad crítica

---

#### `HN1-01` — Tres artefactos del Bloque 1 dan tres respuestas incompatibles a la segregación en delegaciones pequeñas

| | |
|---|---|
| **Severidad** | **Crítica** |
| **Artefactos** | `RN-01` (casos límite) · `actores-y-roles.md` §5.4 · `orden-de-mision.md` §3.3 · `PR-01` PC-18 y matriz 4.1 acciones 27–28 |
| **Norma en juego** | MARCI — separación de funciones incompatibles. `NRM-01` línea 37, marcada `[C]` |

**Lo que dice cada uno:**

| Artefacto | Postura |
|---|---|
| `RN-01`, caso límite "Delegación única con un solo servidor" | *"La regla **no se relaja**: la función faltante se ejerce por el nivel correspondiente de la dependencia matriz"* |
| `orden-de-mision.md` §3.3, "El problema de las delegaciones pequeñas" | *"La solución **no es una excepción configurable**: una excepción registrada es evidencia en contra ante el TSC. La solución es el **escalamiento a sede**"* |
| `actores-y-roles.md` §5.4 Nivel 2 | Régimen de excepción declarado por `ACT-09` que **levanta expresamente** los pares I-02, I-03, I-04, I-05, I-06, I-08 e I-09, con seis controles compensatorios |
| `PR-01` PC-18 y matriz de permisos acciones 27 y 28 | Dan el régimen **por existente**: "Declarar régimen de excepción de delegación", "Convalidar actos ejecutados en régimen de excepción" |

Las dos primeras posturas y las dos últimas son mutuamente excluyentes. No es un matiz de redacción: una construye el sistema con escalamiento obligatorio y sin excepciones, la otra construye un expediente administrativo de excepción con convalidación posterior. Son diseños distintos, tablas distintas y pantallas distintas.

**Por qué es crítico y no alto.** El día que el TSC pregunte *"¿cómo garantiza este sistema la separación de funciones en la delegación de tres personas?"*, la institución tiene que dar **una** respuesta. Hoy la documentación da dos, y la que aparece en la matriz de permisos (`PR-01`) es la que un desarrollador implementaría.

**Qué corregir.** Decidir una postura y propagarla a los cuatro artefactos. Si se adopta el régimen de excepción, **necesita su propia `RN-xx`** — hoy no existe ninguna regla que lo gobierne, pese a que `PR-01` PC-18 lo cita como punto de control. Si no se adopta, `actores-y-roles.md` §5.4 Nivel 2, `PR-01` PC-18 y las acciones 27–28 de la matriz deben retirarse o marcarse como propuesta no aprobada.

**Estado de la verificación del régimen — ver §2 de este documento.** Lo que se pudo y no se pudo confirmar sobre controles compensatorios en el MARCI y en las guías de ONADICI está en la sección 2. El resumen: **no se verificó ningún respaldo para levantar incompatibilidades**, y sí hay indicio concordante de respaldo para la salida por escalamiento a personal independiente. Eso inclina la balanza hacia la postura de `RN-01` y `orden-de-mision.md`, pero **no la resuelve**: es `[P]`.

---

#### `HN1-02` — `RN-01` atribuye la segregación de funciones a TSC-NOGECI V-07, que es la norma de autorización

| | |
|---|---|
| **Severidad** | **Crítica** |
| **Artefactos** | `RN-01`, campo **Origen**: *"Norma NRM-01 — MARCI / TSC-NOGECI V-07"* |
| **Norma en juego** | TSC-NOGECI V-07 |

`NRM-01`, en su propia tabla, define **TSC-NOGECI V-07 = "Autorización y Aprobación de Transacciones y Operaciones"**, y en la línea siguiente dice, marcado `[C]`:

> *"El MARCI contiene **además** normas sobre segregación de funciones, documentación de procesos y transacciones, acceso restringido a activos y registros... Los códigos y títulos exactos deben tomarse del MARCI impreso que tenga la institución."*

Es decir: la propia ficha declara que la norma de segregación **es otra**, y que su código **no se conoce**. `RN-01` la cita como si fuera V-07.

**Este es el hallazgo más grave del tipo "regla que cita mal su norma".** Una vez que `RN-01` entra al código con "TSC-NOGECI V-07" en el comentario, nadie vuelve a revisarlo, y la institución termina invocando ante el TSC una norma que no dice lo que ella afirma que dice. Un auditor que abra V-07 y no encuentre segregación de funciones concluirá que el control se fundamentó a ojo.

**Dato nuevo de esta revisión, `[P]`:** informes de auditoría del propio TSC referencian **TSC-NOGECI V-08 = "Documentación de Procesos y Transacciones"**, lo que confirma que V-08 tampoco es la norma de segregación. El código de la norma de separación de funciones incompatibles **sigue sin confirmarse**. Ver §2.

**Qué corregir.** En `RN-01`, sustituir el origen por *"`NRM-01` — MARCI, norma de separación de funciones incompatibles; `[C]` código y título exactos, pendientes del MARCI impreso (riesgo #5, insumo de `riesgos-normativos.md`)"*. Revisar el mismo patrón en `PR-01` PC-01 y PC-09, que citan "NRM-01, segregación MARCI/TSC" sin código — eso sí es correcto y debe ser el modelo.

*(Las demás citas a NOGECI del Bloque 1 sí corresponden: `RN-02`, `RN-06` y `RN-32` invocan V-07 para autorización previa por servidor competente, que es exactamente lo que V-07 regula; `RN-28` y `RN-46` invocan V-10 Registro Oportuno; `RN-29` invoca V-14 Conciliación Periódica. Todas concordantes con la tabla de `NRM-01`.)*

---

#### `HN1-03` — Degradación sistemática del nivel de verificación en el control más duro del sistema

| | |
|---|---|
| **Severidad** | **Crítica** |
| **Artefactos** | `NRM-01` → `actores-y-roles.md` §5.2 → `RN-01` → `PR-01` PC-01 |

La misma afirmación —*el MARCI exige separación de funciones incompatibles*— aparece con tres niveles distintos según el artefacto, y **el nivel sube conforme el artefacto se acerca al código**:

| Artefacto | Nivel declarado |
|---|---|
| `NRM-01` (la ficha, fuente) | `[C]` — *"los códigos y títulos exactos deben tomarse del MARCI impreso"* |
| `actores-y-roles.md` §5.2, los 17 pares | `[P]` — NRM-01 `[P]` |
| `RN-01` | **`[V]`** la exigencia de segregación |
| `PR-01` PC-01, PC-09, PC-13 | **`[V]`** NRM-01, segregación MARCI/TSC |

El orden correcto es el inverso: el nivel de una regla **nunca puede ser superior al de su ficha**. Aquí lo es, y en el control que el sistema declara como el primero de sus cinco bloqueos indesactivables.

**Matiz honesto, que no atenúa el hallazgo.** La exigencia de fondo es real: el MARCI se construye sobre COSO e INTOSAI `[V]` (`NRM-01`), y la separación de funciones incompatibles es un principio nuclear de ambos. Lo que **no** está verificado es el texto hondureño concreto, su código y su alcance literal. La diferencia importa: el TSC no fiscaliza contra COSO, fiscaliza contra el MARCI.

**Qué corregir.** Alinear `RN-01` y `PR-01` en `[P]`, con la aclaración de qué sostiene ese `[P]` (COSO/INTOSAI como base declarada del MARCI, más citas en informes del TSC), y mantener el `[C]` sobre el código y el articulado. Y **añadir una regla de método al `README.md` de reglas**: *ninguna `RN-xx` puede declarar un nivel superior al de la ficha `NRM-xx` que invoca.* Sin esa regla, este defecto se repetirá en cada bloque.

---

### Severidad alta

---

#### `HN1-04` — La matriz de segregación de `orden-de-mision.md` §3.3 declara compatible un par que `actores-y-roles.md` bloquea

| | |
|---|---|
| **Severidad** | Alta |
| **Artefactos** | `orden-de-mision.md` §3.3 · `actores-y-roles.md` §5.2 par I-03 · `RN-01` |

La matriz de `orden-de-mision.md` §3.3 marca **Solicita × Entrega combustible = ✓ compatible** (en ambas direcciones de la matriz, de forma coherente consigo misma).

`actores-y-roles.md` I-03 marca ese mismo par **Bloqueo duro**. `RN-01` también lo bloquea: "Solicitar" y "Entregar combustible" son dos de sus cinco funciones de control, y la regla prohíbe que una persona ejerza dos.

**Por qué importa.** Es precisamente el par que habilita el fraude de combustible más simple que existe: el mismo servidor pide la misión y entrega el vale. `orden-de-mision.md` es la especificación que el equipo declara "escrita para que se pueda implementar sin preguntar nada" — es la que se va a implementar.

`orden-de-mision.md` §3.3 justifica dos pares permitidos (programa×liquida, autoriza×cierra) pero **no justifica éste**, lo que sugiere descuido y no decisión.

**Qué corregir.** Alinear la matriz con I-03, o justificar la excepción con fundamento. Y revisar la matriz completa contra `actores-y-roles.md` §5.2: son dos representaciones del mismo control y hoy no coinciden.

---

#### `HN1-05` — `RN-01` es incompleta: le faltan la función "cerrar", el núcleo irreductible y las incompatibilidades absolutas

| | |
|---|---|
| **Severidad** | Alta |
| **Artefactos** | `RN-01` · `PR-01` PC-13, PC-17 · `orden-de-mision.md` `BD-06`, `T-21` · `actores-y-roles.md` I-12, I-13 |

`RN-01` enumera cinco funciones de control: solicitar, autorizar, despachar, entregar combustible, liquidar. Tres controles que el resto del Bloque 1 declara como bloqueo duro **no tienen regla que los sostenga**:

1. **Quien cierra ≠ quien liquidó.** `PR-01` PC-13 y `orden-de-mision.md` `T-21` lo exigen como bloqueo duro. `RN-01` no distingue "cerrar" de "liquidar", así que no lo cubre. Y `RN-08` (cierre) tampoco lo menciona.
2. **`ACT-12` Auditor Interno × cualquier rol ejecutor** (I-12) — incompatibilidad **absoluta y permanente**, no por misión. `RN-01` se evalúa "sobre una misma Orden de Misión" y explícitamente excluye la auditoría de su alcance. Ninguna regla la cubre.
3. **`ACT-01` Administrador × facultad de autorizar, aprobar fondo o liquidar** (I-13) — igual. `RN-01` lo trata en un caso límite, no en el enunciado, y `PR-01` PC-17 lo declara "bloqueo duro permanente — núcleo irreductible".

**Qué corregir.** Ampliar `RN-01` a seis funciones incluyendo el cierre, y crear una regla separada para las incompatibilidades **absolutas** (auditoría e administración), que son de naturaleza distinta: no se evalúan por expediente sino por asignación de rol. `actores-y-roles.md` §5.3 ya describe ese doble momento (preventivo al asignar el rol, bloqueante al ejecutar); la regla no lo recoge.

---

#### `HN1-06` — Verificación circular: la regla marca `[V]` porque la ficha dijo "el sistema debe"

| | |
|---|---|
| **Severidad** | Alta |
| **Artefactos** | `RN-10`, `RN-21`, `RN-25`, `RN-29`, `RN-31`, `RN-47`, entre otras |

Varias reglas declaran `[V]` sobre una formulación del tipo *"la exigencia de X"*, donde X es una **implicación de requerimiento** escrita por el propio equipo en la ficha, no una norma. Ejemplos literales:

| Regla | Verificación declarada | Qué es en realidad |
|---|---|---|
| `RN-25` | `[V]` *"la exigencia de permiso portable y control físico en carretera"* | La lista de documentos que seguirán requiriendo papel está marcada **`[I]`** en `NRM-08` |
| `RN-31` | `[V]` *"la exigencia de registrar odómetro de salida y retorno y detectar inconsistencias"* | Implicación de requerimiento de `NRM-09`, ficha que la propia normativa describe como *"no describe una norma sino el terreno"* |
| `RN-47` | `[V]` *"la exigencia de digitación diferida con constancia y adjunto del original"* | Implicación de requerimiento de `NRM-09` y `NRM-01`, no articulado |
| `RN-10` | `[V]` sin matiz | La exigencia de licencia vigente es razonable, pero el articulado de la Ley de Tránsito **no se extrajo** (PDF sin capa de texto, riesgo #17) |
| `RN-21` | `[V]` *"la exigencia de registrar capacidad de pasajeros y carga"* | Correcto para el **registro**; el **bloqueo por exceso** es inferencia sin norma citada |

La circularidad es el problema: *ficha dice "el sistema debe" → regla marca `[V]` → nadie vuelve a mirar la ficha.* El `[V]` deja de significar "verificado con fuente oficial" y pasa a significar "escrito por nosotros dos veces".

**Contraste positivo:** `RN-13`, `RN-19` y `RN-20` se marcan honestamente `[I]` regla operativa / regla de producto, y `RN-22` se marca `[P]`. Ése es el estándar.

**Qué corregir.** Separar en el campo Verificación dos cosas distintas: **el nivel de la norma** y **el nivel de la derivación**. Una regla puede ser `[V]` en su norma y `[I]` en su enforcement, y decirlo es más defendible que un `[V]` liso.

---

#### `HN1-07` — La cuota trimestral de compromiso (`NRM-04`) no tiene regla, y ninguna de las 53 reglas cita `NRM-04`

| | |
|---|---|
| **Severidad** | Alta |
| **Artefactos** | `RN-26` · `README.md` de reglas, columna Origen |
| **Norma en juego** | `NRM-04` — programación financiera de SIAFI, cuotas trimestrales de compromiso `[V]` |

`NRM-04` quedó reducida por `DP-001` D-09, pero conserva explícitamente una obligación **dentro** del alcance de SIGTI:

> *"Lo que sí queda vigente para SIGTI: entender que el gasto está sujeto a **cuota trimestral de compromiso**, no solo a presupuesto anual. Ese dato viene de ARGOS, pero **SIGTI debe respetarlo al aprobar la asignación de fondos de combustible y peajes**."*

Y la ficha añade por qué importa: *"Un sistema que solo controla contra el presupuesto anual permitirá comprometer gasto que la institución no puede ejecutar."*

`RN-26` controla el saldo del **fondo**, que es un objeto interno de SIGTI, no la cuota. Ninguna de las 53 reglas menciona la cuota trimestral, y `NRM-04` **no aparece ni una vez** en la columna Origen del `README.md` de reglas.

**Consecuencia práctica.** La Gerencia Administrativa aprueba un fondo, SIGTI lo registra conforme a `RN-26`, y el compromiso no cabe en la cuota del trimestre. El descuadre aparece en ARGOS o en SIAFI, no en SIGTI, y SIGTI queda como el sistema que lo permitió.

**Qué corregir.** Escribir la regla, o dejar por escrito y con fundamento que el control de cuota es responsabilidad exclusiva de ARGOS y que SIGTI no lo replica — que es una respuesta legítima bajo `DP-001`, pero hoy **no está escrita en ninguna parte**, y el silencio no es una decisión.

---

#### `HN1-08` — `RN-39` permite que el Administrador del Sistema ponga en vigencia un parámetro normativo por sí solo

| | |
|---|---|
| **Severidad** | Alta |
| **Artefactos** | `RN-39` · `actores-y-roles.md` §4.3 y regla candidata 6 · `PR-01` E13 |

`actores-y-roles.md` §4.3 establece un doble control explícito y lo fundamenta bien: *"Una tarifa de peaje, un umbral de desviación de rendimiento o un plazo de liquidación **son dinero**. Quien pueda cambiarlos solo puede alterar el resultado de toda conciliación pasada y futura."* — `ACT-01` **carga**, `ACT-08` **aprueba la puesta en vigencia**. `PR-01` E13 lo repite para el umbral de hallazgo.

`RN-39` dice, en cambio, que el parámetro debe ser *"consultable y modificable por ACT-01 Administrador del Sistema **o** por el rol facultado"*, y su comportamiento 1 solo exige registrar quién lo cargó. **No hay doble control.**

Esto atraviesa el sistema entero: `RN-39` gobierna las tarifas de peaje, los umbrales de desviación de `RN-30` (los que deciden si hay hallazgo), los plazos de liquidación y la matriz licencia↔vehículo. Un `ACT-01` que pueda subir solo el umbral de desviación puede hacer desaparecer los hallazgos de `H-01` sin tocar un solo dato operativo.

Y contradice el propio límite duro que `actores-y-roles.md` fija a `ACT-01`: *"No puede ejecutar ni autorizar ninguna transacción de negocio."*

**Qué corregir.** Incorporar el doble control a `RN-39` — carga y puesta en vigencia como dos actos con dos actores —, o escribir la regla candidata 6 de `actores-y-roles.md` §8 como `RN-xx` propia y referenciarla desde `RN-39`.

---

#### `HN1-09` — No hay regla que produzca el paquete de evidencia **por vehículo o por período**, que es lo que pide el TSC

| | |
|---|---|
| **Severidad** | Alta |
| **Artefactos** | `RN-08` comportamiento 4 · `RN-30` comportamiento 4 · `RN-37` comportamiento 4 |
| **Norma en juego** | `NRM-01`, implicación de requerimiento |

`NRM-01` es explícita:

> *"El sistema debe **exportar paquetes de evidencia por período o por vehículo** en formato entregable a auditoría: PDF con índice y sello de tiempo, anexos, y hoja de cálculo."*

Lo que existe en el Bloque 1:

- `RN-08` exporta el paquete **por expediente de misión**.
- `RN-30` produce el reporte de conciliación periódica de combustible.
- `RN-37` produce el reporte de peajes por vehículo, motorista, dependencia y período.

Falta el ensamblaje. **Un requerimiento del TSC no llega por misión: llega por vehículo o por período** — "entrégueme todo lo del vehículo 042 del ejercicio 2026". Hoy la institución tendría que exportar misión por misión y armar el paquete a mano, que es exactamente el trabajo manual que el sistema existe para evitar, y el que produce omisiones cuando hay prisa.

Esto conecta con el riesgo de auditoría de la sección 3: el diseño **produce la correlación**, pero no la **entrega** en la unidad en que se la van a pedir.

**Qué corregir.** Una regla de paquete de evidencia con dos ejes de agregación —vehículo y período— que ensamble expedientes, conciliaciones, hallazgos, constataciones y cambios de parámetro aplicables al alcance solicitado.

---

#### `HN1-10` — `RN-35` bloquea la aprobación con un dato que en el proceso todavía no existe

| | |
|---|---|
| **Severidad** | Alta |
| **Artefactos** | `RN-35` · `RN-33` comportamiento 3 · `PR-01` E2, E3, E5 · `orden-de-mision.md` `INV-07`, `T-02` |

`RN-35` exige que antes de `APROBADA` se presente la estimación con *"categoría aplicada al vehículo"*, y **bloquea la aprobación** si la categoría no está resuelta (`RN-33`) o no hay tarifa vigente (`RN-34`).

Pero en el proceso, **el vehículo se asigna en E5, después de `APROBADA`**. En E2/E3 no hay vehículo: hay *tipo de vehículo requerido*. `PR-01` E2 lo dice bien —*"la categoría que corresponde al tipo de vehículo requerido"*— y `orden-de-mision.md` `T-02` también. `RN-35` no.

**Dos consecuencias, ambas malas.** Si se implementa literalmente, ninguna solicitud puede aprobarse hasta tener vehículo asignado, y el ciclo `APROBADA → PROGRAMADA` deja de tener sentido. Si se implementa "razonablemente", cada desarrollador resolverá a su criterio qué categoría usar en la estimación previa, y la estimación congelada del `RN-41` dejará de ser reproducible.

Hay además un segundo bloqueo con el mismo problema: `RN-34` y `RN-35` bloquean cuando no hay tarifa vigente, y `NRM-10` ordena expresamente **no cargar ninguna tarifa** hasta confirmarla (riesgo #14, insumo #21). Con el sistema recién instalado y sin tarifas, `RN-35` bloquea la aprobación de toda misión que atraviese la CA-5. `RN-34` lo asume conscientemente —*"el sistema arranca sin tarifas cargadas, bloqueando la estimación"*— pero `RN-35` convierte ese bloqueo de la **estimación** en bloqueo de la **aprobación**, que es cosa distinta y detiene la operación.

**Qué corregir.** En `RN-35`, distinguir la categoría del *tipo requerido* (estimación previa) de la del *vehículo asignado* (estimación de despacho), y decidir explícitamente si la ausencia de tarifa bloquea la aprobación o solo marca el estimado como no disponible. Sugerencia de contraste, no de solución: `orden-de-mision.md` `T-08` ya prevé recalcular al programar y **reautorizar si la diferencia supera el umbral** — ese mecanismo hace innecesario bloquear en E3.

---

### Severidad media

---

#### `HN1-11` — Bloqueo duro por matrícula vencida, sin regla y sin norma extraída

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | `orden-de-mision.md` `BD-03` · `PR-01` PC-05 y diagrama 3.1 nodo K6 |

`BD-03` declara la matrícula **"Sí, duro"**, vigente durante todo el rango de la misión, y `PR-01` PC-05 lo repite como bloqueo. Ninguna `RN-xx` lo establece: `RN-16` cubre póliza y revisión, `RN-17` cubre alertas, `RN-15` cubre identidad y placa. **No hay regla de matrícula.**

Y `NRM-06` no lo sustenta expresamente: exige *"registrar matrícula y placa"*, no bloquear por vencimiento. El articulado de la Ley de Tránsito no se pudo extraer (riesgo #17).

**El contraste es el que preocupa.** El Bloque 1 fue escrupuloso en no bloquear por seguro ni por revisión, precisamente porque verificó que no son obligatorios `[V]`. Con la matrícula hizo lo contrario sin verificar nada. Puede que la conclusión sea correcta —conducir con matrícula vencida sí es infracción—, pero hoy es un bloqueo duro sostenido por nada escrito, y en el mismo documento donde se explicó con cuidado por qué el seguro no bloquea.

**Qué corregir.** Escribir la regla con su nivel real (`[I]` o `[P]` mientras no se extraiga el articulado), o degradar el bloqueo a configurable como `RN-16`.

---

#### `HN1-12` — `RN-18` cablea datos normativos en el enunciado y los marca `[V]`

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | `RN-18` · `RN-39` · `NRM-02` · `riesgos-normativos.md` documento a obtener #6 |

`RN-18` fija en el enunciado, marcados `[V]` uno por uno: franjas de **10 cm**, azul–blanco–azul, leyenda en letras de **2.54 cm**.

**Dos problemas.**

1. **Contradice `RN-39`.** Son datos de origen normativo susceptibles de cambiar por un nuevo acuerdo o PCM, y `RN-39` ordena que *todo* dato normativo sea parámetro con vigencia. Deberían ser un catálogo `elemento_identificacion_vehiculo` versionado, no constantes en el texto de la regla. La regla misma dice, con razón, que un booleano "rotulado: sí" no prueba nada — el mismo argumento aplica a las medidas literales.
2. **El `[V]` es más fuerte de lo que la fuente sostiene.** El **Acuerdo No. 303 de 1981 figura en `riesgos-normativos.md` como documento por obtener (#6)** y su articulado no se extrajo. Entre las fuentes de `NRM-02` la que contiene especificaciones de rotulación es el *Reglamento para el control en el uso de vehículos municipales* de AMHON — norma **municipal**, no del Poder Ejecutivo. Trasladar medidas de un reglamento municipal a la flota del gobierno central es exactamente la clase de inferencia que hay que marcar.

`RN-18` sí marca `[C]` la vigencia del Acuerdo 303 "en su redacción original", lo que es correcto pero insuficiente: si la redacción está en duda, las medidas no pueden estar `[V]`.

**Qué corregir.** Bajar las medidas a `[P]` con la fuente identificada, parametrizarlas con vigencia, y elevar el `[C]` sobre el articulado del Acuerdo 303. Nota adicional: el insumo #43 (cómo se rotula una motocicleta) ya está bien capturado en la regla — ese tratamiento es el correcto y debería extenderse a las medidas.

---

#### `HN1-13` — Las restricciones médicas bloquean todo en la máquina de estados y son configurables en la regla

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | `orden-de-mision.md` `BD-02` condición 3 · `RN-11` · insumo #42 |

`BD-02` lista como tercera condición de un **bloqueo duro sin excepción configurable**: *"Restricciones médicas compatibles. Si la licencia tiene restricciones registradas —corrección visual, prohibición de conducción nocturna, u otras— y la misión las contradice, bloquea."*

`RN-11` establece lo contrario y con mejor criterio: bloquea solo lo tipificado como **incompatibilizante**, y advierte con acuse en lo demás, *"porque las restricciones no son homogéneas: 'usar lentes correctores' no se puede verificar por sistema y no debe bloquear"*.

Además, `BD-02` hereda de `DP-001` D-12 la etiqueta "sin excepción configurable", que el PO dio a la **matriz licencia↔vehículo**, no a las restricciones médicas. Y el catálogo oficial de restricciones de la DNVT es `[C]` (insumo #42): hoy no existe base para clasificar nada.

**Qué corregir.** Alinear `BD-02` con `RN-11` y sacar las restricciones médicas del alcance del "sin excepción configurable", que debe quedar acotado a lo que `DP-001` D-12 efectivamente decidió.

---

#### `HN1-14` — `RN-52` y `PR-01` marcan `[V]` una exigencia del MARCI que `NRM-01` marca `[C]`

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | `RN-52` · `PR-01` §7 V-03 y matriz 4.1 nota 12 · `actores-y-roles.md` §3.3 |
| **Norma en juego** | MARCI — acceso restringido a activos y registros |

`RN-52` declara `[V]` *"que el MARCI exige control de acceso y registro de consultas, aun sin ley de datos"*. `NRM-01` marca esa familia de normas del MARCI como `[C]`, con los códigos por confirmar. `NRM-07` afirma la exigencia sin marcarla individualmente.

Peor en `PR-01` V-03, que atribuye el requisito a la ficha equivocada: *"Control de acceso por rol y registro de todas las consultas al expediente `[V]` **NRM-01** MARCI"* — cuando el requisito está redactado en `NRM-07`.

**Matiz importante, y es a favor del diseño.** `DP-001` D-14 conservó el control de acceso y el registro de consultas **precisamente porque el MARCI lo exige de todos modos**, y esa decisión es correcta y prudente: el hábeas data del Art. 182 constitucional sí está `[V]` vigente, y sin registro de consultas la institución no puede responder nada a un titular que pregunte quién vio sus datos. El defecto es de etiqueta, no de sustancia. Pero la etiqueta es lo que el auditor lee.

**Qué corregir.** `RN-52` a `[P]`, citando `NRM-07` como ficha principal y el hábeas data `[V]` como fundamento adicional; corregir la atribución de ficha en `PR-01`.

*(Se verifica como cumplido el encargo sobre `DP-001` D-14: `RN-51` y `RN-52` conservan minimización, control de acceso por necesidad de conocer y registro de consultas, y `RN-51` descarta correctamente lo que D-14 sacó del alcance. La cobertura es buena.)*

---

#### `HN1-15` — `RN-26` invoca `RN-01` fuera del alcance que `RN-01` se fija a sí misma

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | `RN-26` enunciado y caso límite final · `RN-01` condiciones de aplicación |

`RN-26` afirma: *"Quien solicita el fondo **no puede** ser quien lo aprueba (`RN-01`)"*, y remata un caso límite con *"El fondo es dinero: aquí la segregación es más importante, no menos."*

Pero `RN-01` se aplica **"sobre una misma Orden de Misión"**, y sus cinco funciones son funciones sobre una misión. El **fondo de combustible es un objeto de período**, no de misión: lo solicita `ACT-04` y lo aprueba `ACT-08` para un período completo. `RN-01`, leída como está escrita, **no alcanza al fondo**.

Resultado: la incompatibilidad más sensible del circuito de dinero —solicitar el fondo × aprobar el fondo— queda enunciada en el cuerpo de `RN-26` pero sin regla que la sostenga y sin aparecer en la matriz de `actores-y-roles.md` §5.2, que también razona por misión.

**Qué corregir.** Ampliar el alcance de `RN-01` a los objetos de control que no son la misión (fondo, orden de trabajo, expediente de descargo), o incorporar la incompatibilidad al enunciado de `RN-26` como control propio. Lo mismo aplica al par I-17 de `actores-y-roles.md` (propone descargo × aprueba descargo), que tampoco tiene regla.

---

#### `HN1-16` — `RN-02` prohíbe el escalamiento automático por ausencia; `actores-y-roles.md` §7.3 lo prescribe

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | `RN-02` caso límite "El nivel superior está de vacaciones" · `actores-y-roles.md` §7.3 y regla candidata 11 |

`RN-02`: *"Se resuelve con `RN-07`, delegación vigente; **no con salto de nivel automático**. Un salto automático a un nivel más alto sin acto de delegación deja la aprobación sin fundamento."*

`actores-y-roles.md` §7.3: *"Si no hay delegación y la ausencia no fue prevista, las solicitudes pendientes **escalan automáticamente al puesto superior** transcurrido un plazo parametrizable."*

Ambas posiciones son defendibles y ambas están escritas como definitivas. La de `RN-02` es más conservadora ante el TSC (una autorización necesita competencia, y la competencia nace de un acto); la de `actores-y-roles.md` es más operativa (ninguna misión se traba por una incapacidad súbita).

**No la resuelvo.** Señalo que hay que elegir, y que la elección tiene consecuencia normativa: si se adopta el escalamiento automático, hay que poder explicar de dónde nace la competencia del puesto superior sobre un acto que la jerarquía no le asignó. Esto se cruza con el insumo #28.

---

#### `HN1-17` — `RN-25` impone como bloqueo duro no configurable una capacidad cuya viabilidad institucional está `[C]`

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | `RN-25` (Configurable: **No**) · `actores-y-roles.md` `ACT-15` pendiente G · `PR-01` E8 |

`RN-25` exige, como bloqueo duro sin configuración, que todo documento de control lleve QR que resuelva a *"una página pública de verificación"*, y que exista esa página.

`actores-y-roles.md` `ACT-15` deja `[C]` la pregunta de fondo: *"Si la institución acepta exponer un punto de verificación público en internet, siendo el despliegue on-premise"* (pendiente G), y propone una alternativa sin exposición externa marcada `[I]`.

Una regla no configurable que depende de una decisión institucional no tomada es una regla que se va a incumplir o a desactivar. Y el supuesto de fondo —que hay internet publicable desde el servidor on-premise de la institución— es de los que el propio proyecto advierte que no deben darse por seguros.

**A favor de la regla:** sus casos límite ya resuelven bien el problema del verificador **sin señal en carretera** (código corto legible más contraste visual del hash), que es el escenario real hondureño. Lo que falta es el mismo realismo aplicado al lado del servidor.

**Qué corregir.** Separar dos requisitos: **el documento impreso con folio, QR y hash** (bloqueo duro, no configurable — correcto) y **el canal de verificación** (con al menos dos modalidades: página pública o consulta interna con código corto, según lo que la institución acepte).

---

#### `HN1-18` — Obligaciones normativas del Bloque 1 sin ninguna regla

| | |
|---|---|
| **Severidad** | Media |
| **Artefactos** | Recorrido de las secciones "Implicaciones de requerimiento" de `NRM-01`, `NRM-02`, `NRM-06`, `NRM-07`, `NRM-09`, `NRM-10` |

Además de `HN1-07` (cuota trimestral) y `HN1-09` (paquete por vehículo/período), quedaron sin regla:

| Obligación | Ficha | Observación |
|---|---|---|
| **Constatación física periódica de la flota y conciliación contra el registro de bienes** | `NRM-01` (NOGECI V-15), `NRM-02`, Circular CGR-010-2026 `[V]` | `RN-18` cubre la constatación de **rotulación**, no el inventario. `actores-y-roles.md` creó `ACT-14` justamente para esto y no hay regla que lo gobierne |
| **Roles y permisos por puesto, no por persona** | `NRM-09`, implicación literal | Está modelado con detalle en `actores-y-roles.md` §2 y es regla candidata 1, **sin escribir**. Es el mecanismo que absorbe la rotación, que la propia ficha señala como alta |
| **No se cierra una asignación de puesto con custodias físicas activas** | `NRM-02` | Regla candidata 2 de `actores-y-roles.md` §2.4, sin escribir. `RN-22` menciona "custodia vacante" en un caso límite pero no cubre el cierre de la asignación ni la entrega unilateral con hallazgo |
| **Reporte público de flota agregado o anonimizado para el Portal Único** | `NRM-07`, implicación literal | `RN-51` crea la separación estructural que lo hace posible; nadie obliga a producirlo |
| **TAG prepago como instrumento institucional con ciclo de vida** | `NRM-10` | Aparece en `orden-de-mision.md` `EF-04` y en casos límite de `RN-35`/`RN-36`; ninguna regla lo modela. Depende del insumo #24 |
| **Guía de actuación en accidente accesible sin conexión** | `NRM-06`, implicación literal | Aparece en `orden-de-mision.md` `T-12`; `RN-43` no la incluye en su lista de capacidades offline |
| **Infracciones y multas de tránsito asociadas a vehículo y motorista** | `NRM-06` | Sin regla. Presumiblemente M-12, bloque posterior — **conviene decirlo explícitamente** en el `README.md` de reglas |
| **Registro de pérdida, robo o siniestro con denuncia y deducción de responsabilidad** | `NRM-02`, `NRM-06` | Igual que el anterior |

Los dos últimos pueden ser deliberadamente de un bloque posterior. **El problema es que no se dice.** El `README.md` de reglas no declara qué módulos cubre el Bloque 1, así que no hay forma de distinguir un hueco de una postergación — y esa distinción es la que evita que un hueco sobreviva tres bloques.

---

### Severidad baja

---

#### `HN1-19` — Verificación agregada que oculta el eslabón más débil

| | |
|---|---|
| **Severidad** | Baja |
| **Artefactos** | `RN-23`, `RN-15` |

`RN-23` declara como origen *"Acuerdo No. 303, Decreto 135-94, Circular 003-2025-Presidencia-TSC"* y marca `[V]` la prohibición y la exigencia de permiso. En la tabla de `NRM-02`, el Decreto 135-94 y el Decreto 48 están `[P]`, y la ficha añade `[C]` sobre la cita completa del Decreto 48. El `[V]` agregado tapa dos eslabones más débiles.

Lo mismo en `RN-15`, que marca `[V]` *"la exigencia de numeración consecutiva institucional"*, dato que proviene del Acuerdo 303 no extraído.

**Qué corregir.** Cuando una regla se apoya en varias normas de niveles distintos, el nivel de la regla es **el del eslabón más débil**, y conviene decir cuál es.

*(La otra mitad de `RN-15` —el desabastecimiento de placas y el tratamiento de "sin placa metálica" como estado válido— está correctamente `[V]` y bien fundamentada. Es de las mejores reglas del conjunto.)*

---

#### `HN1-20` — `[V]` usado para decisiones de producto, que no son verificación normativa

| | |
|---|---|
| **Severidad** | Baja |
| **Artefactos** | `RN-12`, `RN-14`, `RN-26`, `RN-50`, `PR-01` §7 V-03 |

Varias reglas declaran `[V] la decisión de producto` o `[V] la decisión de arquitectura`. La escala de `CLAUDE.md` define `[V]` como *"verificado con fuente oficial o fuentes concordantes"* — es una escala **normativa**. Una decisión del PO no se verifica: se toma, y se cita por su identificador.

Es menor porque no engaña sobre ninguna norma, pero mezcla dos escalas en el mismo campo, y a la larga diluye qué significa `[V]` en el proyecto.

**Qué corregir.** Para decisiones internas, citar `DP-001 D-xx` o `ADR-001` sin marca de verificación, y reservar `[V]` `[P]` `[C]` `[I]` para afirmaciones sobre normas y sobre la realidad externa.

---

## 2. Investigación sobre controles compensatorios — qué se pudo y qué no se pudo verificar

Realizada específicamente para evaluar `HN1-01`. **Fecha de consulta: 2026-08-06.**

### Lo verificado `[V]`

- Existe la **Guía General para la Implementación del MARCI**, ONADICI, 3ª ed., enero 2023, en `https://www.onadici.gob.hn/wp-content/uploads/2023/02/Guia-General-Implementacion-MARCI-SP3.pdf` — coincide con la que ya cita `NRM-01`.
- Existe el **Manual de Normas de Control Interno** del TSC en `https://www.tsc.gob.hn/web/leyes/MANUAL_DE_NORMAS_DE_CONTROL_INTERNO.pdf`.
- Las guías de ONADICI incluyen un apartado denominado **"Separación de Funciones Incompatibles"**, lo que confirma que la materia está tratada como norma propia y **no** como parte de TSC-NOGECI V-07 — refuerza `HN1-02`.

### Lo parcialmente verificado `[P]`

- **TSC-NOGECI V-08 = "Documentación de Procesos y Transacciones"**, según referencias en informes de auditoría del TSC (p. ej. `006-2010-DFBN`). Dato nuevo, no presente en `NRM-01`. Se obtuvo de la síntesis del buscador sobre esos informes; **no se abrió el documento**, por lo que no se transcribe como cita.
- La Guía General del MARCI (3ª ed.) **reconoce la dificultad de las entidades pequeñas** para separar funciones por concentración de responsabilidades, y propone como salida **delegar autoridad a mandos medios cuando la estructura lo permita y, alternativamente, a personal independiente.** Indicio concordante obtenido de la indexación del PDF; **no se pudo transcribir el texto literal**.

### Lo que no se pudo verificar `[C]`

- **El código y el título exactos de la norma de separación de funciones incompatibles.** Sigue abierto, igual que en `NRM-01`.
- **El texto literal** del apartado sobre entidades pequeñas.
- **Que el MARCI o las guías de ONADICI contemplen un régimen de excepción que *levante* incompatibilidades a cambio de controles compensatorios.** No se encontró nada que lo sostenga.

### Limitación técnica, reportada como corresponde

- `www.onadici.gob.hn` y `onadici.gob.hn` **devuelven un certificado TLS que no corresponde al dominio** (los `altnames` son de `a2hosting.com`). La descarga automatizada del PDF falla. Es un obstáculo nuevo, no documentado en `riesgos-normativos.md`, y **afecta a la fuente principal de este punto**.
- El **Manual de Normas de Control Interno del TSC sí se descargó (407 KB) pero no tiene capa de texto utilizable**: la extracción devolvió contenido corrupto. Confirma la advertencia metodológica de `riesgos-normativos.md`.

### Lectura, sin resolver el punto

La evidencia disponible apunta a que la doctrina hondureña recogida por ONADICI resuelve el problema de la entidad pequeña **por delegación a personal independiente** —que es exactamente el "Nivel 1, escalamiento a sede" de `orden-de-mision.md` §3.3 y de `RN-01`— y **no** por un régimen que levante incompatibilidades. Eso hace que la postura de `RN-01` y de la máquina de estados sea, con la información de hoy, **la más defendible ante el TSC**.

Pero es `[P]`, y hay una razón adicional para no cerrarlo por mi cuenta: la pregunta que el insumo #26 dirige a Auditoría Interna no es solo qué dice la norma, sino **qué acepta la Auditoría Interna de la institución**, que en la práctica pesa más. `NRM-08` ya hace esa misma observación para otro asunto: *"En la práctica esto pesa más que la norma."*

**Recomendación de método, no de contenido:** el trabajo de OCR ya previsto (insumo #23, riesgo #17) debería **incluir la Guía General del MARCI 3ª ed. y el Manual de Normas de Control Interno**, no solo la Ley de Tránsito y las tarifas de la SAPP. Es el mismo esfuerzo y cierra tres `[C]` más: el código de la norma de segregación, el texto de entidades pequeñas y el catálogo NOGECI completo.

### Fuentes consultadas — todas el 2026-08-06

- [ONADICI — Guía General para la Implementación del MARCI, 3ª ed.](https://www.onadici.gob.hn/wp-content/uploads/2023/02/Guia-General-Implementacion-MARCI-SP3.pdf) — **inaccesible por certificado TLS inválido**
- [ONADICI — Guías para la Implementación del Control Interno Institucional, 2ª ed.](https://www.onadici.gob.hn/wp-content/uploads/2021/12/Guias-Para-la-Implementacion-del-Control-Interno-Institucional-ONADICI-2da-edicion.pdf)
- [TSC — Manual de Normas de Control Interno](https://www.tsc.gob.hn/web/leyes/MANUAL_DE_NORMAS_DE_CONTROL_INTERNO.pdf) — **descargado, sin capa de texto utilizable**
- [TSC — MARCI, Acuerdo Administrativo 001-2008](https://www.tsc.gob.hn/wp-content/uploads/MARCI-2009.pdf)
- [TSC — Marco Rector del Control Interno (portal)](https://www.tsc.gob.hn/index.php/marco-rector-del-control-interno-marci/)
- [TSC — Informe 006-2010-DFBN](https://www.tsc.gob.hn/wp-content/uploads/006-2010-DFBN.pdf) — referenciado por el buscador para TSC-NOGECI V-07 y V-08; no abierto
- [TSC — Ley Orgánica, versión LOTSC_2024](https://www.tsc.gob.hn/web/leyes/LOTSC_2024.pdf) — confirma la existencia del texto de 2024 que `NRM-01` marca `[C]`

---

## 3. Riesgo de auditoría — ¿el diseño produce lo que el TSC va a pedir?

El auditor busca **correlación entre consumo, kilometraje y misión autorizada**, no comprobantes archivados. Evaluación honesta:

### Lo que el diseño sí produce

| Pregunta del auditor | Qué responde |
|---|---|
| ¿Este consumo guarda relación con el kilometraje? | `RN-30`, con desviación en **ambas direcciones**, que es lo que distingue este control de uno ingenuo |
| ¿Este peaje corresponde a la ruta autorizada? | `RN-37`, con coherencia geográfica y temporal. Es de lo mejor del Bloque 1 |
| ¿Quién autorizó esto y sobre qué contenido exacto? | `RN-03`, con huella del contenido — resuelve *"¿autorizó este viaje o autorizó otro que después alguien editó?"* |
| ¿Con qué regla se calculó este monto en marzo de 2024? | `RN-40` + `RN-41` + `EF-03`, congelamiento del paquete normativo |
| ¿Este registro se hizo cuando ocurrió o se reconstruyó después? | `RN-46` + `RN-47`, con indicador de oportunidad de registro |
| ¿Por qué este vale no tiene comprobante? | `RN-08` + `RN-29`, cierre con hallazgo tipificado en vez de expediente abandonado |

Esto es más de lo que la mayoría de las instituciones puede mostrar hoy.

### Lo que le faltaría a la institución ante un requerimiento real

1. **La unidad de entrega.** `HN1-09`. El requerimiento llega por vehículo o por período; el sistema entrega por misión.
2. **La conciliación con el proveedor.** `NRM-01` pide cruzar *"galones despachados por vale, galones facturados por el proveedor, kilómetros según bitácora"*. `RN-30` menciona la factura del proveedor en su comportamiento 4, pero **ninguna regla modela la factura del proveedor como objeto conciliable**. Con el reencuadre de `DP-001` D-03 (SIGTI no compra combustible) es coherente, pero el auditor va a pedir ese cruce igual. Conviene decidir de dónde sale.
3. **El descargo del peaje.** `NRM-10` lo advierte: si el ticket de caseta no es documento fiscal a nombre de la institución, *"el descargo de peajes será el punto débil de la liquidación ante auditoría"*. `RN-36` y `RN-08` hacen lo correcto (advertir sin bloquear), pero el problema de fondo depende del insumo #24 y no se resuelve con reglas.
4. **La constatación física de la flota.** `HN1-18`. Es NOGECI V-15 y Circular CGR-010-2026 `[V]`, y hoy no tiene regla.
5. **El expediente del régimen de excepción**, si se adopta. `HN1-01`. Un acto ejecutado sin segregación y sin expediente que lo declare es indefendible; con expediente, convalidación y marca en el papel, es discutible pero sostenible. La diferencia la decide algo que hoy no está escrito.

---

## 4. Cierre

### (a) Obligaciones normativas del Bloque 1 que quedaron sin cubrir

1. **Cuota trimestral de compromiso** — `NRM-04`, en alcance explícito, sin ninguna regla y sin ninguna referencia (`HN1-07`).
2. **Paquete de evidencia por vehículo y por período** — `NRM-01`, la unidad en que el TSC pide (`HN1-09`).
3. **Constatación física de flota y conciliación contra el registro de bienes** — `NRM-01` NOGECI V-15, `NRM-02`, Circular CGR-010-2026 (`HN1-18`).
4. **Permisos por puesto y no por persona** — `NRM-09`; modelado con detalle, sin regla (`HN1-18`).
5. **Cierre de asignación de puesto con custodias activas** — `NRM-02` (`HN1-18`).
6. **Reporte público de flota para el Portal Único** — `NRM-07` (`HN1-18`).
7. **Segregación "quien cierra ≠ quien liquidó"** y las **incompatibilidades absolutas** de auditoría y administración — declaradas como bloqueo duro en tres artefactos, sin regla (`HN1-05`).
8. **Doble control sobre la puesta en vigencia de parámetros normativos** — `HN1-08`.
9. **Bloqueo por matrícula vencida** — existe como bloqueo, no existe como regla ni como norma extraída (`HN1-11`).
10. **Guía de actuación en accidente offline** y **TAG institucional** — `NRM-06` y `NRM-10` (`HN1-18`).

### (b) El riesgo normativo que más me preocupa, en una frase

Que el sistema se construya con la matriz de permisos de `PR-01` —que da por existente un régimen de excepción a la segregación de funciones **cuyo respaldo en el MARCI nadie ha verificado**— mientras `RN-01` y la máquina de estados dicen por escrito que ese régimen no existe: el día del hallazgo, la institución tendrá en su propia documentación la prueba de que sabía que el control estaba levantado y de que no supo decidir si podía levantarlo.

### (c) ¿Es defendible ante el TSC tal como está?

**El diseño sí. La documentación todavía no.**

Los controles sustantivos —correlación consumo/kilometraje/misión, inmutabilidad, congelamiento a la fecha del hecho, cadena trazable hasta el cierre, registro de consultas a datos personales— responden a lo que el TSC pregunta, y en varios puntos van por delante de lo que se le exige a una institución hondureña hoy.

Lo que **no** es defendible en su estado actual son tres cosas concretas y acotadas:

1. **La contradicción de `HN1-01`.** Mientras exista, la institución no tiene una respuesta a la primera pregunta que le van a hacer sobre delegaciones. Bloqueante.
2. **La cita errónea de `HN1-02` y la degradación de `HN1-03`.** Un control fundamentado en la norma equivocada, y presentado como verificado cuando la ficha lo marca por confirmar, se cae solo en cuanto alguien abre TSC-NOGECI V-07. Bloqueante, y de corrección barata.
3. **Los bloqueos sin regla** (`HN1-05`, `HN1-11`) y las **obligaciones sin regla** (`HN1-07`, `HN1-09`, `HN1-18`). No son bloqueantes para cerrar el bloque, pero cada uno es un hueco por el que se cuela un hallazgo.

Con `HN1-01`, `HN1-02` y `HN1-03` corregidos, el Bloque 1 es defendible. Ninguno de los tres exige investigación nueva: exigen una decisión, una corrección de cita y una corrección de etiqueta.

---

## 5. Trazabilidad

- **Fichas normativas**: `NRM-01`, `NRM-02`, `NRM-04`, `NRM-06`, `NRM-07`, `NRM-08`, `NRM-09`, `NRM-10`
- **Riesgos relacionados**: #5 (MARCI por obtener), #7 (Art. 48), #14 y #15 (peajes), #17 (PDF sin capa de texto) en `riesgos-normativos.md`
- **Insumos relacionados**: #22, #23, #24, #25, #26, #27, #28, #29, #42, #43 en `insumos-pendientes.md`
- **Riesgo nuevo propuesto**: el sitio de ONADICI presenta certificado TLS inválido, lo que impide la consulta automatizada de la fuente principal sobre separación de funciones. Registrar en `riesgos-normativos.md` — **lo registra quien mantiene ese archivo, no este revisor.**
- **Insumo nuevo propuesto**: ampliar el alcance del trabajo de OCR (insumo #23) a la Guía General del MARCI 3ª ed. y al Manual de Normas de Control Interno del TSC.
