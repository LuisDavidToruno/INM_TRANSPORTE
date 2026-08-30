# Inventario de pantallas

| Campo | Valor |
|---|---|
| **Ámbito** | Toda pantalla del sistema, con su cliente, sus roles, su trazabilidad y las dos condiciones que deciden si se puede diseñar hoy |
| **Para quién está escrito** | El diseñador externo que va a producir los mockups. Esta tabla es su plan de trabajo |
| **Artefacto hermano** | [`mapa-de-navegacion.md`](mapa-de-navegacion.md) — cómo se recorren y por qué |
| **Total** | **138 pantallas** · 103 solo administrativo · 9 duales `A/C` · 25 solo campo · 1 pública |
| **Superficies a diseñar para el cliente de campo** | **34** — las 25 de la §3 más las 9 duales. No son 25: ver [§5](#5-recuento-qué-se-puede-diseñar-y-qué-no) |
| **Última actualización** | 2026-08-25 |

---

## 0. Correcciones aplicadas en esta versión

Todas provienen de [`H-B34-002`](../05-calidad/hallazgos/H-B34-002-revision-arquitectura-bloque-4.md), Parte 2. Cada una está además anotada en su sitio.

| Hallazgo | Qué decía | Qué se hizo |
|---|---|---|
| `HB34-65` | El recuento 27 / 8 / 91 no cierra; el desglose por cliente subcuenta el campo | Recuento rehecho columna a columna: sobre las 126 filas originales era **28 / 9 / 89**. Con las altas de `HB34-66` y `HB34-70` el total pasa a **138 · 29 / 10 / 99**. Encabezado y §5 corregidos; superficies de campo declaradas aparte |
| `HB34-66` | Las quince historias de M-17 (`HU-111`–`HU-125`) no tenían ninguna pantalla, y el hueco no estaba declarado | Se inventarían **once pantallas nuevas** (`PT-128` a `PT-138`) y se retraza `PT-093`, `PT-094`, `PT-095` y `PT-106` a sus historias reales. Lo que queda abierto se declara en §7 |
| `HB34-67` | Catorce pantallas sin historia; el trío del parámetro normativo sin `CU` ni `HU` | Se cierran dos por trazabilidad (`PT-093`, `PT-095`). Las doce restantes se declaran una por una en §7 con su motivo, y el trío `PT-099`/`PT-100`/`PT-092` queda marcado como **bloqueado por la ausencia de `CU-19`** |
| `HB34-68` | `PT-041` daba a `ACT-05` la entrega del fondo, que la matriz le niega e `I-08` bloquea duro | `PT-041` es de `ACT-07`. La columna **Rol** ahora distingue quién ejecuta de quién solo está presente o consulta |
| `HB34-69` | La navegación de `ACT-10` se apoyaba en el régimen de excepción que `DP-002` suspendió | Corregido en el [mapa §8.2](mapa-de-navegacion.md). Aquí se marcan las pantallas afectadas con **⚠ #26** |
| `HB34-70` | `PT-104` se usaba como dos raíces distintas | Se crea `PT-127` «Mi delegación hoy», raíz de `ACT-10`. `PT-104` queda solo del motorista |
| `HB34-71` | `PT-105` marcada `Parc.` y a la vez declarada diseñable hoy en tres documentos | Se conserva `Parc.` y se explicita qué parte se diseña hoy y cuál espera el formato. Ver §5.1 |
| `HB34-72` | `PT-020` y `PT-024` producen documento con folio y no estaban en §6 | Añadidas a §6, junto con la nueva `PT-130` |
| `HB34-73` | `PT-124` asignaba a `ACT-13` una captura que la matriz le da solo como consulta | Ejecuta `ACT-14`; `ACT-13` queda como presente. Se declara la pregunta abierta en §7 |

`HB34-74` no aplica a este documento: el artefacto que el revisor daba por ausente —[`mockups/README.md`](mockups/README.md)— **sí existe**. Sus diez hallazgos de §5 están leídos y los que tocan este inventario están resueltos abajo.

---

## 1. Cómo se lee esta tabla

**`PT-xxx` es un identificador nuevo, estable y no reciclable**, igual que el resto de los IDs del proyecto. Si una pantalla se descarta, su ID queda obsoleto y no se reutiliza.

| Columna | Qué significa |
|---|---|
| **Cli** | `A` cliente administrativo · `C` cliente de campo · `P` superficie pública sin sesión. Ver [mapa §0.2](mapa-de-navegacion.md) — **son dos productos distintos, no uno responsive** |
| **Rol** | Actor `ACT-xx` que **ejecuta** el acto. Un actor **entre paréntesis** `(ACT-xx)` está presente o la consulta, pero **no la ejecuta**. Otros roles pueden consultarla dentro de su alcance de datos. La autoridad es [`actores-y-roles.md`](../01-negocio/actores-y-roles.md) §4 y §5.2 — donde esta tabla la contradiga, manda aquélla |
| **CU** | Caso de uso que la recorre |
| **HU** | Historias que la implementan. Una pantalla puede implementarse en varias entregas |
| **Sin red** | `Sí` funciona con el dispositivo totalmente desconectado · `No` exige conexión · `Deg.` funciona degradada, declarando qué no puede verificar |
| **Papel** | **La columna que divide el trabajo en dos.** `Sí` = replica un formato preimpreso, **bloqueada por el insumo #2** · `Parc.` = una sección replica papel y el resto no · `No` = sin equivalente en papel, **se diseña ya** |

### La regla que gobierna la columna «Papel»

> El operador que lleva años llenando un formato preimpreso debe encontrar **los mismos campos, con los mismos nombres, en el mismo orden** en la pantalla.

Las pantallas marcadas `Sí` **no se diseñan libremente**. Se toma el formato que la institución usa hoy y se reproduce. Si alguien propone "mejorar" el orden de los campos, **la respuesta por defecto es no**, y quien lo proponga debe justificar por qué el costo de reaprendizaje vale la pena.

Las marcadas `Parc.` sí tienen trabajo disponible hoy: la estructura, los controles nuevos que el papel no tenía y los mensajes de bloqueo. Lo que queda en espera es el bloque de campos que reproduce el formato.

### La marca ⚠ #26

> **Nota `HB34-69`.** Una pantalla marcada **⚠ #26** es ejecutable por `ACT-10` Encargado de Delegación **solo si** el insumo #26 se resuelve a favor del régimen de excepción. Hoy no está resuelto y [`DP-002`](../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md) **suspendió ese régimen**: las acciones 27 y 28 de la matriz están tachadas con ⛔ y las celdas `E⁴` de `ACT-10` no son ejecutables. Mientras siga abierto, **el camino por defecto de esas tres pantallas es el escalamiento a sede**, no la ejecución local. El diseñador debe dibujar el escalamiento como camino normal y la ejecución local como rama condicionada, no al revés.

---

## 2. Cliente administrativo

### 2.1 Transversales

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-001 | Ingreso y selección de puesto vigente | A | todos | — | — | No | No |
| PT-002 | Inicio del puesto: pendientes, alertas y accesos | A | todos | — | — | No | No |
| PT-003 | Bandeja de tareas escaladas por segregación de funciones | A | `ACT-03` `ACT-04` `ACT-08` | CU-02, CU-06 | HU-010 | No | No |
| PT-004 | Patrón de pantalla de bloqueo duro: qué se impidió, por qué, cómo salir | A/C | todos | CU-02, CU-04, CU-06, CU-15 | HU-010, HU-025, HU-039, HU-077, HU-078, HU-091, HU-108 | Sí | No |
| PT-005 | Buscador de expedientes con alcance de datos aplicado | A | todos | — | — | No | No |

### 2.2 M-06 Solicitud de transporte

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-006 | Mis solicitudes | A | `ACT-02` | CU-01 | HU-001 | No | No |
| PT-007 | **Requisición de vehículo** (solicitud de transporte) | A | `ACT-02` | CU-01 | HU-001, HU-003 | No | **Sí** |
| PT-008 | Objeto del traslado: personas, carga o mixto | A | `ACT-02` | CU-01 | HU-001, HU-002 | No | Parc. |
| PT-009 | Estimado de peajes desglosado por punto | A | `ACT-02` `ACT-03` | CU-01 | HU-005 | No | No |
| PT-010 | Señalamiento de tramos inhábiles, sin bloquear | A | `ACT-02` | CU-01, CU-03 | HU-006 | No | No |
| PT-011 | Envío a autorización con número de expediente y congelamiento | A | `ACT-02` | CU-01 | HU-004 | No | No |
| PT-012 | Registro de salida de emergencia para convalidación posterior | A | `ACT-02` `ACT-10` | CU-01 | HU-008 | No | Parc. |

### 2.3 M-06 Autorización

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-013 | **Bandeja de autorización** — [difícil §7.2](mapa-de-navegacion.md) | A | `ACT-03` | CU-02 | HU-009, HU-012 | No | No |
| PT-014 | Expediente en decisión, en una sola pantalla | A | `ACT-03` | CU-02 | HU-009, HU-011 | No | No |
| PT-015 | Autorizar con constancia inmutable | A | `ACT-03` | CU-02 | HU-011, HU-015 | No | No |
| PT-016 | Rechazar con motivo y solicitud vinculada | A | `ACT-03` | CU-02 | HU-014 | No | No |
| PT-017 | Devolver para corrección con versionado | A | `ACT-03` | CU-02 | HU-013 | No | No |
| PT-018 | Escalamiento de autorización por nivel o umbral | A | `ACT-03` `ACT-08` | CU-02 | HU-012 | No | No |
| PT-019 | Autorización por delegación de firma vigente | A | `ACT-03` | CU-02 | HU-015 | No | No |

### 2.4 M-04 / M-15 Permiso de circulación en día u hora inhábil

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-020 | Trámite del permiso de circulación en día u hora inhábil | A | `ACT-04` `ACT-10` | CU-03 | HU-016 | No | **Sí** |
| PT-021 | Firma del permiso por la máxima autoridad (dos toques, celular) | A | `ACT-09` | CU-03 | HU-016 | No | No |
| PT-022 | Firma en lote de feriado largo con reporte previo | A | `ACT-09` | CU-03 | HU-020 | No | No |
| PT-023 | Emisión e impresión del **salvoconducto** | A | `ACT-04` `ACT-10` | CU-03, CU-05 | HU-017 | No | **Sí** |
| PT-024 | Reemisión del permiso por cambio de elementos amparados | A | `ACT-04` | CU-03, CU-07 | HU-018 | No | **Sí** |

### 2.5 M-07 Programación y asignación

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-025 | Cola de programación con caducidad de la aprobación | A | `ACT-04` `ACT-10` | CU-04 | HU-021 | No | No |
| PT-026 | Asignación de vehículo: compatibilidad, documentación y estado | A | `ACT-04` | CU-04 | HU-022, HU-023, HU-024 | No | No |
| PT-027 | Declaración de **quien conduce**: motorista titular y relevos | A | `ACT-04` | CU-04 | HU-025, HU-026 | No | No |
| PT-028 | **Rechazo por licencia no habilitante** — [difícil §7.5](mapa-de-navegacion.md) | A | `ACT-04` `ACT-10` | CU-04, CU-18 | HU-025, HU-108 | No | No |
| PT-029 | Reserva exclusiva y conflicto con su titular | A | `ACT-04` | CU-04 | HU-027 | No | No |
| PT-030 | Consolidación de solicitudes compatibles | A | `ACT-04` | CU-04 | HU-030 | No | No |
| PT-031 | Constancia probatoria de las verificaciones practicadas | A | `ACT-04` `ACT-12` | CU-04 | HU-028 | No | No |
| PT-032 | Sustitución de vehículo o motorista en `PROGRAMADA` | A | `ACT-04` | CU-07 | HU-043 | No | No |
| PT-033 | Sustitución con la misión ya `DESPACHADA` | A | `ACT-04` | CU-07 | HU-044 | No | No |
| PT-138 | Compatibilidad de personas externas con personal y carga *(ficha en §2.15)* | A | `ACT-04` | CU-04 | HU-125 | No | No |

> **Vocabulario.** `PT-027` se llamaba «Declaración de conductores». El [glosario](../00-vision/glosario.md) resolvió la duda que el diseño planteó en [`mockups §5.1`](mockups/README.md): **motorista** es quien está en el padrón, y *«quien conduce»* o *«conductor declarado»* es el término correcto cuando el padrón no aplica —el funcionario con vehículo asignado, o quien releva en una emergencia—. `RN-57` verifica la habilitación sobre **quien efectivamente conduce**, sea o no motorista: por eso esta pantalla y `PT-086` conservan ese término y el resto del documento dice motorista.
>
> `PT-138` es un eco de la §2.15; **no se cuenta dos veces** en el recuento de la §5.

### 2.6 M-15 Emisión de documentos

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-034 | Vista previa con folio reservado, marcada «no válida» | A | `ACT-04` | CU-05 | HU-029 | No | **Sí** |
| PT-035 | Emisión del juego documental: **orden de misión**, peajes, advertencias, bitácora | A | `ACT-04` `ACT-10` | CU-05 | HU-031, HU-032, HU-033, HU-034, HU-081 | No | **Sí** |
| PT-036 | Reimpresión con el mismo folio y marca de reimpresión | A | `ACT-04` | CU-05 | HU-036 | No | **Sí** |
| PT-037 | Emisión anticipada para delegación sin cobertura | A/C | `ACT-10` | CU-05 | HU-037 | Sí | **Sí** |

### 2.7 M-07 / M-08 Despacho y retorno

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-038 | Tablero de despacho del día: salidas y retornos previstos | A | `ACT-05` | CU-06, CU-10 | HU-038 | Deg. | No |
| PT-039 | Acto de despacho: revalidación, kilometraje de salida e inspección | A | `ACT-05` | CU-06 | HU-038, HU-039 | Deg. | **Sí** |
| PT-040 | **Acta de entrega y traslado de custodia** | A/C | `ACT-05` `ACT-13` | CU-06 | HU-040 | Sí | **Sí** |
| PT-041 | Entrega del fondo contra firma, en el acto del despacho | A/C | `ACT-07` `(ACT-05)` ⚠ #26 | CU-06, CU-13 | HU-041, HU-079 | Sí | **Sí** |
| PT-042 | Registro del retorno y cierre de la bitácora | A/C | `ACT-05` `ACT-10` | CU-10 | HU-062, HU-063 | Sí | **Sí** |
| PT-043 | Retorno sin vehículo: el bien queda resguardado en sitio | A | `ACT-05` `ACT-04` | CU-10 | HU-065 | Deg. | No |

> **Corrección `HB34-68` — `PT-041` no es una pantalla de `ACT-05`.** La matriz de permisos ([`actores-y-roles.md`](../01-negocio/actores-y-roles.md) §4, acción 10 «Entregar fondo o vale al motorista») da a `ACT-05` **`–` sin acceso**; `I-08` «Despacha × Entrega fondo» sobre la misma misión es **bloqueo duro**; y `EF-04` de [`estados/orden-de-mision.md`](../03-arquitectura/estados/orden-de-mision.md) dice literalmente que *quien entrega no puede ser quien despacha ni el motorista*. La entrega la **ejecuta `ACT-07`**, presente en el acto del despacho. `ACT-05` la ve —está delante— y por eso figura entre paréntesis. En el flujo del despachador el nodo correspondiente **no es una pantalla que él consuma**, sino la espera de que `ACT-07` entregue: ver [mapa §5](mapa-de-navegacion.md).
>
> La marca ⚠ #26 recoge el otro camino: `ACT-10` tiene esta acción como `E⁴`, es decir, **solo bajo el régimen de excepción que `DP-002` suspendió**. Hoy escala a sede.
>
> **Queda abierto (`mockups §5.5`):** `PT-041` y `PT-048` parecen el mismo acto visto desde dos lugares y replican el mismo formato en papel. Se resuelve al recibir el formato del insumo #2. Si son la misma pantalla en dos contextos, uno de los dos ID queda obsoleto — **no se recicla**.

### 2.8 M-09 Combustible

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-044 | Solicitud del fondo de combustible del período | A | `ACT-04` | CU-12 | HU-071 | No | **Sí** |
| PT-045 | Aprobación del fondo contra cuota y partida | A | `ACT-08` | CU-12 | HU-072, HU-073 | No | No |
| PT-046 | Ampliación del fondo agotado y resolución de la prelación | A | `ACT-04` `ACT-08` | CU-12 | HU-075 | No | No |
| PT-047 | Emisión de la asignación de combustible con folio | A | `ACT-07` | CU-13 | HU-076, HU-077, HU-078 | No | **Sí** |
| PT-048 | Entrega del fondo y registro de su custodia | A | `ACT-07` | CU-13 | HU-074 | No | **Sí** |
| PT-049 | Anulación de la asignación con acta | A | `ACT-07` | CU-13 | HU-080 | No | **Sí** |
| PT-050 | Ciclo de vida del vale y arqueo del fondo | A | `ACT-07` | CU-13, CU-15 | HU-074, HU-079, HU-080 | No | No |
| PT-051 | Declaración de la fuente de todo abastecimiento y unicidad del comprobante | A | `ACT-07` `ACT-04` | CU-14, CU-15 | HU-083, HU-087 | No | No |

### 2.9 M-16 Sincronización y conflictos

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-052 | Panel de sincronización de dispositivos | A | `ACT-04` `ACT-10` | CU-11 | HU-066, HU-067 | No | No |
| PT-053 | **Cola de conflictos** — [difícil §7.1](mapa-de-navegacion.md) | A | `ACT-04` `ACT-10` | CU-11 | HU-068 | No | No |
| PT-054 | **Comparador de dos versiones lado a lado** — [difícil §7.1](mapa-de-navegacion.md) | A | `ACT-04` `ACT-10` | CU-11 | HU-068 | No | No |
| PT-055 | Resolución por lote con criterio declarado | A | `ACT-04` | CU-11 | HU-068 | No | No |
| PT-056 | Estado del espejo de ARGOS y Talento Humano | A | `ACT-01` `ACT-04` | CU-11 | HU-069 | No | No |
| PT-057 | Registro de campo que llega después del cierre de la bitácora | A | `ACT-04` | CU-11 | HU-070 | No | No |

### 2.10 M-19 Seguimiento en ruta

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-058 | Tablero de seguimiento en ruta, con antigüedad del dato | A | `ACT-04` | CU-08 | HU-057 | No | No |
| PT-059 | Detalle de la misión en ruta con sus hitos | A | `ACT-04` `ACT-05` | CU-08 | HU-047, HU-055 | No | No |
| PT-060 | Ampliación del alcance autorizado, con versionado | A | `ACT-04` `ACT-03` | CU-08, CU-09 | HU-055 | No | No |
| PT-061 | Recepción de la interrupción y resolución de su desenlace | A | `ACT-04` | CU-09 | HU-058, HU-059, HU-060 | No | No |
| PT-062 | Relevo de motorista en ruta: resolución desde oficina | A | `ACT-04` | CU-09, CU-07 | HU-045, HU-061 | No | Parc. |

### 2.11 M-13 Liquidación y cierre

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-063 | Cola de liquidación, con lo que bloquea cada misión | A | `ACT-04` | CU-15 | HU-091 | No | No |
| PT-064 | **Conciliación galonaje contra kilometraje** — [difícil §7.4](mapa-de-navegacion.md) | A | `ACT-04` | CU-15 | HU-088, HU-084 | No | No |
| PT-065 | Conciliación del fondo: sobrante y faltante tipificados | A | `ACT-04` | CU-15 | HU-089 | No | Parc. |
| PT-066 | Conciliación de peajes punto por punto | A | `ACT-04` | CU-15 | HU-090, HU-086 | No | No |
| PT-067 | Bloqueo de la liquidación por segregación de funciones | A | `ACT-04` `ACT-07` | CU-15 | HU-091 | No | No |
| PT-068 | Cadena de trazabilidad y propuesta de cierre | A | `ACT-04` | CU-15 | HU-092 | No | No |
| PT-069 | Cierre de la misión con la cadena completa | A | `ACT-08` | CU-16 | HU-093 | No | No |
| PT-070 | Cierre con hallazgo tipificado | A | `ACT-08` | CU-16 | HU-094 | No | No |
| PT-071 | Hallazgo posterior sobre misión `CERRADA` — expediente nuevo, sin reapertura | A | `ACT-08` `ACT-12` | CU-16 | HU-095 | No | No |

### 2.12 M-03 / M-04 Flota y expediente del vehículo

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-072 | Padrón de flota con estado operativo | A | `ACT-04` `ACT-14` | CU-17 | HU-102 | No | No |
| PT-073 | Expediente del vehículo: vista completa del ciclo de vida | A | `ACT-04` `ACT-14` `ACT-11` | CU-17 | HU-096 – HU-104 | No | No |
| PT-074 | Alta del vehículo con título de tenencia | A | `ACT-14` | CU-17 | HU-096 | No | **Sí** |
| PT-075 | Placa y estado de la lámina (sin placa es estado válido) | A | `ACT-14` `ACT-04` | CU-17 | HU-097 | No | No |
| PT-076 | Ficha técnica que habilita: peso bruto, ejes, capacidad | A | `ACT-04` | CU-17 | HU-098 | No | Parc. |
| PT-077 | Tarjeta de responsabilidad y traspaso de custodia | A | `ACT-14` `ACT-13` | CU-17 | HU-099 | No | **Sí** |
| PT-078 | Vencimientos documentales y alertas dirigidas al puesto | A | `ACT-04` | CU-17 | HU-101 | No | No |
| PT-079 | Habilitación del vehículo para operar en flota | A | `ACT-04` | CU-17 | HU-102 | No | No |
| PT-080 | Descargo del bien propio con acta y resolución | A | `ACT-14` | CU-17 | HU-103 | No | **Sí** |
| PT-081 | Retiro de flota de un bien ajeno (comodato, alquiler) | A | `ACT-14` | CU-17 | HU-104 | No | **Sí** |

### 2.13 M-05 Motoristas y habilitación

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-082 | Padrón de motoristas con su habilitación vigente | A | `ACT-04` | CU-18 | HU-105, HU-107 | No | No |
| PT-083 | Captura de la licencia como dato propio de SIGTI, con fotografía | A | `ACT-04` | CU-18 | HU-105 | No | Parc. |
| PT-084 | Tipos de vehículo habilitados, derivados de la categoría | A | `ACT-04` | CU-18 | HU-106 | No | No |
| PT-085 | Vigencia de la habilitación y alertas anticipadas | A | `ACT-04` | CU-18 | HU-107 | No | No |
| PT-086 | Declaración de conductor fuera del padrón, con el mismo rigor | A | `ACT-04` | CU-18, CU-04 | HU-109, HU-108 | No | No |
| PT-087 | Inhabilitación con causa y encaminamiento de las misiones afectadas | A | `ACT-04` | CU-18 | HU-110 | No | No |

### 2.14 M-14 Auditoría y reportes

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-088 | Consulta de la pista de auditoría | A | `ACT-12` | CU-16 | — | No | No |
| PT-089 | Rastro del expediente de extremo a extremo, con sus huecos visibles | A | `ACT-12` | CU-16 | HU-092 | No | No |
| PT-090 | Exportación del paquete de evidencia (PDF con índice, anexos, hoja de cálculo) | A | `ACT-12` `ACT-08` | CU-16 | — | No | No |
| PT-091 | Reporte de intentos bloqueados por segregación de funciones | A | `ACT-12` `ACT-08` | CU-02, CU-06, CU-15 | HU-010, HU-039, HU-091 | No | No |
| PT-092 | Histórico de cambios de parámetros normativos con vigencia | A | `ACT-12` | ⛔ falta `CU-19` | ⛔ sin historia | No | No |
| PT-093 | Registro de consultas a datos de personas externas | A | `ACT-12` | CU-16 | HU-118 | No | No |

> **Corrección `HB34-66` / `HB34-67` — `PT-093` sí tenía historia.** `HU-118` «registrar cada consulta al manifiesto» es exactamente esta pantalla, y `HU-119` es su reporte, que ahora es `PT-133`. La trazabilidad estaba rota en las dos direcciones para el mismo módulo.
>
> **`PT-092` sigue sin historia y sin caso de uso**, y no es un olvido menor: forma trío con `PT-099` y `PT-100`. Ver §2.16 y §7.

### 2.15 M-17 Traslado de personas externas

> **Corrección `HB34-66`.** Esta sección tenía dos pantallas para **quince historias** (`HU-111` a `HU-125`), y la §7 declaraba los huecos de M-11, M-12 y M-18 pero no éste. Un hueco declarado es una decisión; uno no declarado es un olvido que nadie va a buscar. Se inventarían las once pantallas que faltaban y se retrazan las dos que había. Lo que sigue abierto está en §7.

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-094 | **Manifiesto de personas trasladadas** — datos mínimos autorizados | A/C | `ACT-02` `ACT-04` `ACT-05` | CU-01, CU-05, CU-06 | HU-111 | Sí | **Sí** |
| PT-095 | Consulta del manifiesto bajo necesidad de conocer, con registro | A/C | `ACT-05` `ACT-06` `ACT-12` | CU-06, CU-08 | HU-117, HU-118, HU-120 | Sí | No |
| PT-128 | Fundamentación del campo sensible: base legal y necesidad operativa | A | `ACT-01` | ⛔ falta `CU-19` | HU-112 | No | No |
| PT-129 | Persona sin documento de identidad: identificación alternativa o no identificada | A/C | `ACT-05` `(ACT-02)` | CU-01, CU-06 | HU-113 | Sí | Parc. |
| PT-130 | Cierre del manifiesto al despachar y **lista de abordo impresa** | A/C | `ACT-05` | CU-06 | HU-114 | Sí | **Sí** |
| PT-131 | Novedad del manifiesto en ruta: no se presentó, subió, bajó antes | C | `ACT-06` | CU-08 | HU-116 | Sí | No |
| PT-132 | Alcance de visibilidad del manifiesto por necesidad de conocer | A | `ACT-01` `(ACT-03)` | ⛔ falta `CU-19` | HU-117 | No | No |
| PT-133 | Reporte de accesos a manifiestos y alerta de patrón anómalo | A | `ACT-12` | CU-16 | HU-119 | No | No |
| PT-134 | **Hábeas data**: buscar todo lo guardado sobre una persona y exportarlo | A | `ACT-12` `[C]` | CU-16 | HU-121 | No | No |
| PT-135 | Rectificación por hábeas data **sin destruir el asiento original** | A | `ACT-12` `[C]` | CU-16 | HU-122 | No | No |
| PT-136 | Exportación de transparencia sin ningún dato personal | A | `ACT-08` `[C]` | CU-16 | HU-123 | No | No |
| PT-137 | **Depuración al vencer el plazo**, con aviso previo y verificación de la cadena | A | `ACT-01` `(ACT-12)` | ⛔ falta `CU-19` | HU-124 | No | No |
| PT-138 | Compatibilidad de personas externas con personal y con carga, por tramo | A | `ACT-04` | CU-04 | HU-125 | No | No |

Notas de esta sección:

- **`PT-131` es una pantalla de campo, no de oficina.** `RN-53` cierra el manifiesto al despachar: lo que pasa después es **novedad, no corrección**. La tentación del usuario es editar el manifiesto para que cuadre, y si el sistema lo permite el manifiesto deja de ser una declaración. La pantalla no ofrece editar; ofrece registrar qué pasó, sin señal y en pocos toques.
- **`PT-134`, `PT-135` y `PT-136` tienen un actor `[C]` sin catalogar**: el Oficial de Información Pública. Las tres historias lo nombran así. Hasta que se resuelva, el rol operante es el que la historia asigna en primer lugar. Registrado en §7.
- **`PT-137` es la única pantalla del sistema que destruye contenido**, y por eso es la que más cuidado exige: `RNF-17` fija en **0** los datos personales que sobrevivan la depuración, e `HU-124` exige que la cadena de auditoría siga verificando después. El aviso previo es obligatorio.
- **`PT-138` vive en el flujo de programación** aunque pertenezca a M-17: es la evaluación par a par y tramo a tramo de `HU-125`, y se recorre desde `PT-026`.

### 2.16 M-01 / M-02 Administración y operación

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-096 | Usuarios, puestos y asignaciones vigentes | A | `ACT-01` | ⛔ falta `CU-19` | ⛔ sin historia | No | No |
| PT-097 | Asignación puesto↔rol con control de acumulación incompatible | A | `ACT-01` `(ACT-08)` | ⛔ falta `CU-19` | HU-010 | No | No |
| PT-098 | Catálogos maestros (M-02) | A | `ACT-01` `(ACT-08)` | ⛔ falta `CU-19` | ⛔ sin historia | No | No |
| PT-099 | **Carga del parámetro normativo** con vigencia y respaldo documental | A | `ACT-01` | ⛔ falta `CU-19` | [HU-144](../02-requisitos/historias/HU-144-cargar-parametro-normativo-con-vigencia.md) | No | No |
| PT-100 | **Puesta en vigencia — doble control** | A | `ACT-08` | ⛔ falta `CU-19` | [HU-145](../02-requisitos/historias/HU-145-aprobar-la-puesta-en-vigencia-doble-control.md) · [HU-146](../02-requisitos/historias/HU-146-bloquear-que-quien-carga-apruebe-su-propia-carga.md) | No | No |
| PT-101 | Panel de salud: qué está mal y qué hacer | A | `ACT-01` | — | [RNF-20](../02-requisitos/no-funcionales/RNF-20-observabilidad-y-diagnostico.md) | No | No |
| PT-102 | Respaldo y restauración para alguien sin especialización | A | `ACT-01` | — | [RNF-09](../02-requisitos/no-funcionales/RNF-09-instalacion-respaldo-y-restauracion.md) | No | No |

> ### Hallazgo abierto `HB34-67` — el ciclo de vida del parámetro normativo no tiene ni caso de uso ni historia
>
> **`PT-099` + `PT-100` + `PT-092` son un trío**: se carga la tarifa de peaje con su vigencia y su respaldo, otro la pone en vigencia, y el histórico deja ver qué cambió y desde cuándo. De ese mecanismo cuelgan [`RNF-05`](../02-requisitos/no-funcionales/), el invariante M-01 del modelo de datos, `RN-39` a `RN-42` y el doble control de [`actores-y-roles.md`](../01-negocio/actores-y-roles.md) §4.3.
>
> **Ninguna de las tres tiene un solo criterio de aceptación en Gherkin**, mientras que registrar un arribo tiene doce. La pantalla desde la que se carga una tarifa —la que decide si `RNF-05` se cumple o se cablea— es hoy la peor documentada del sistema.
>
> **No se puede corregir desde este documento:** hace falta un `CU-19` de administración de parámetros normativos con doble control, y sus historias. **Se pide antes de que la implementación resuelva el doble control con un `if`.** Mientras tanto, las celdas afectadas van marcadas ⛔ y **estas pantallas no se envían a diseño**: sin criterio de aceptación, lo que se dibuje va a fijar la regla por accidente.

> ### ✅ Resuelto a medias — 2026-08-29
>
> **Las historias se escribieron**: [`HU-144`](../02-requisitos/historias/HU-144-cargar-parametro-normativo-con-vigencia.md), [`HU-145`](../02-requisitos/historias/HU-145-aprobar-la-puesta-en-vigencia-doble-control.md), [`HU-146`](../02-requisitos/historias/HU-146-bloquear-que-quien-carga-apruebe-su-propia-carga.md) y [`HU-147`](../02-requisitos/historias/HU-147-resolver-el-parametro-a-la-fecha-del-hecho.md), con sus criterios de aceptación. Las celdas HU de `PT-099` y `PT-100` siguieron diciendo «sin historia» después de que dejara de ser cierto: **este documento quedó desactualizado respecto de los artefactos que cita**, que es el modo normal en que una tabla derivada miente.
>
> Y lo que el hallazgo temía **no ocurrió**: el doble control no se resolvió con un `if`. Está en `ReglasDeDobleControl`, es puro, devuelve el intento en lugar de lanzar —para que el rechazo quede registrado igual— y tiene su clase de pruebas.
>
> **`CU-19` sigue sin existir**, y por eso las celdas CU siguen en ⛔. `PT-099` y `PT-100` se construyeron contra las historias, no contra un caso de uso: alcanza para las dos pantallas y **no alcanza para el flujo completo** —quién devuelve una carga rechazada, cómo se cierra una vigencia abierta, qué pasa con lo ya calculado cuando se aprueba algo retroactivo—. Lo último se declara hoy en la propia pantalla `PT-100`, porque no había dónde más ponerlo.
>
> El mismo ⛔ afecta a `PT-096`, `PT-097`, `PT-098`, `PT-128`, `PT-132` y `PT-137`: ninguna historia del backlog menciona parámetros, catálogos, usuarios ni puestos.

---

## 3. Cliente de campo

**Todas funcionan sin red. No es una característica: es la condición de operación** ([RNF-03](../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md), [RNF-12](../02-requisitos/no-funcionales/RNF-12-uso-en-campo.md)).

> Las tres últimas filas —`PT-129`, `PT-130`, `PT-131`— son **ecos** de la §2.15, puestas aquí para que el diseñador de campo vea su superficie completa en un solo sitio. **No se cuentan dos veces** en el recuento de la §5.

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-103 | Ingreso sin red contra las credenciales del paquete de misión | C | `ACT-06` `ACT-10` | CU-08 | HU-046 | Sí | No |
| PT-104 | **Mi misión** — raíz del motorista en el cliente de campo | C | `ACT-06` | CU-08 | HU-046 | Sí | No |
| PT-105 | **Registro en ruta: llegué, salí, estoy esperando** — [difícil §7.3](mapa-de-navegacion.md) | C | `ACT-06` | CU-08 | HU-047 | Sí | Parc. |
| PT-106 | Entrega de carga y de personas en ruta: **quién recibe**, con evidencia | C | `ACT-06` | CU-08 | HU-048, HU-115 | Sí | **Sí** |
| PT-107 | Paso por caseta de peaje | C | `ACT-06` | CU-14 | HU-049, HU-085 | Sí | No |
| PT-108 | Discrepancia de peaje y reclamo | C | `ACT-06` | CU-14 | HU-050, HU-086 | Sí | No |
| PT-109 | Abastecimiento de combustible con comprobante | C | `ACT-06` | CU-14 | HU-051, HU-082, HU-083 | Sí | Parc. |
| PT-110 | Consumo sin comprobante y gasto imprevisto | C | `ACT-06` | CU-14 | HU-052, HU-087 | Sí | No |
| PT-111 | Aviso de odómetro menor a la última lectura conocida | C | `ACT-06` | CU-08, CU-14 | HU-053 | Sí | No |
| PT-112 | Pendientes de envío y adjuntos en espera | C | `ACT-06` `ACT-10` | CU-11 | HU-054 | Sí | No |
| PT-113 | Solicitud de ampliación del alcance autorizado desde la ruta | C | `ACT-06` | CU-08 | HU-055 | Sí | No |
| PT-114 | Respaldo en papel: hoja de bitácora con folio | C | `ACT-06` `ACT-10` | CU-08 | HU-056 | Sí | **Sí** |
| PT-115 | Actualización de estado y última posición conocida | C | `ACT-06` | CU-08 | HU-057 | Sí | No |
| PT-116 | **Registro de interrupción en ruta** — avería, accidente, robo, otra | C | `ACT-06` | CU-09 | HU-058 | Sí | No |
| PT-117 | Desenlace de la interrupción, comunicado al motorista | C | `ACT-06` | CU-09 | HU-060 | Sí | No |
| PT-118 | Relevo de motorista en ruta con acta y corte de odómetro | C | `ACT-06` | CU-09, CU-07 | HU-045, HU-061 | Sí | **Sí** |
| PT-119 | Retorno y cierre de la bitácora desde el campo | C | `ACT-06` `ACT-10` | CU-10 | HU-062, HU-065 | Sí | Parc. |
| PT-120 | Estado de sincronización del dispositivo | C | `ACT-06` `ACT-10` | CU-11 | HU-066, HU-067 | Sí | No |
| PT-121 | Registro de la salida sin conectividad, en el predio | C | `ACT-05` `ACT-10` ⚠ #26 | CU-06 | HU-042 | Sí | **Sí** |
| PT-122 | Captura de solicitud en delegación sin red | C | `ACT-10` | CU-01 | HU-007 | Sí | **Sí** |
| PT-123 | **Digitación diferida desde el papel**, con foto del original | C | `ACT-10` | CU-11, CU-10 | HU-064, HU-007 | Sí | **Sí** |
| PT-124 | Constatación de la identificación institucional del vehículo | C | `ACT-14` `(ACT-13)` | CU-17 | HU-100 | Sí | **Sí** |
| PT-125 | Consulta de mis documentos: orden de misión y salvoconducto | C | `ACT-06` | CU-08, CU-03 | HU-017, HU-019 | Sí | No |
| PT-127 | **Mi delegación hoy** — raíz de `ACT-10` · misiones, pendientes, papeles por digitar | C | `ACT-10` | CU-01, CU-10, CU-11 | HU-007, HU-064, HU-066 | Sí | No |
| PT-129 | Persona sin documento de identidad *(dual — ficha en §2.15)* | A/C | `ACT-05` `(ACT-02)` | CU-01, CU-06 | HU-113 | Sí | Parc. |
| PT-130 | Cierre del manifiesto al despachar *(dual — ficha en §2.15)* | A/C | `ACT-05` | CU-06 | HU-114 | Sí | **Sí** |
| PT-131 | Novedad del manifiesto en ruta *(ficha en §2.15)* | C | `ACT-06` | CU-08 | HU-116 | Sí | No |

> **Corrección `HB34-70` — `PT-104` se estaba usando como dos raíces distintas.** El inventario la llamaba *«Mi misión, raíz única del cliente de campo»* y el [mapa §8.2](mapa-de-navegacion.md) la llamaba *«Mi delegación hoy»*. Son pantallas con propósitos opuestos: **una misión sin menú** contra **un tablero de varias misiones con cola de digitación**. `CLAUDE.md` declara los `PT-xxx` estables y no reciclables, y tampoco compartibles. Se crea **`PT-127` «Mi delegación hoy»** como raíz de `ACT-10`, y `PT-104` queda solo del motorista.
>
> **Corrección `HB34-73` — `PT-124` la ejecuta `ACT-14`.** La acción 23 de la matriz («Mantener expediente y vencimientos del vehículo») da `ACT-13` = **`C` consulta** y `ACT-14` = `E`. La constatación con fecha y fotografía es un asiento sobre el expediente del vehículo, no una consulta, así que la ejecuta `ACT-14` y `ACT-13` está presente porque tiene el vehículo delante. **Puede ser que la práctica sea la contraria** —y sería razonable—, pero entonces lo que hay que corregir es la matriz con una acción propia o una nota, no la pantalla decidiéndolo por su cuenta. Pregunta abierta en §7.

---

## 4. Superficie pública

| ID | Pantalla | Cli | Rol | CU | HU | Sin red | Papel |
|---|---|---|---|---|---|---|---|
| PT-126 | Verificación del documento por QR — **mínimo verificable, nunca el expediente** | P | `ACT-15` sin autenticar | CU-03, CU-05 | HU-019, HU-035 | No | No |

`[C]` Sujeta a que la institución acepte exponer un punto público en internet con despliegue on-premise — insumo abierto. **La vía degradada (huella impresa, código corto, consulta telefónica) sí se diseña hoy** y puede terminar siendo la única.

---

## 5. Recuento: qué se puede diseñar y qué no

> **Corrección `HB34-65` — no era una celda, eran tres.** El recuento anterior decía 27 bloqueadas + 8 parciales + 91 libres = 126. Sumaba, pero ninguno de los tres números era el correcto: sobre las mismas 126 filas, contando columna a columna, era **28 / 9 / 89**. La propia §5.2 ya enumeraba las 28 bajo el título «las 27 bloqueadas», y `PT-062` estaba marcada `Parc.` sin aparecer en ninguna lista de formatos. El diseño lo había señalado por su cuenta en [`mockups §5.4`](mockups/README.md) — *«vale recontar antes de usar ese número para planificar»*.

| Situación | Sobre las 126 filas originales | **Con las altas de `HB34-66` y `HB34-70`** | Qué significa para el diseñador |
|---|---|---|---|
| **Bloqueadas por el insumo #2** — replican un formato en papel | 28 *(decía 27)* | **29** | No se dibujan hasta tener el formato. Dibujarlas antes es garantizar que hay que rehacerlas |
| **Parcialmente bloqueadas** — una sección replica papel | 9 *(decía 8)* | **10** | Se diseña hoy la estructura, los controles nuevos y los mensajes; se deja el bloque de campos como marco vacío |
| **Sin equivalente en papel** — se diseñan ya | 89 *(decía 91)* | **99** | Trabajo disponible desde el primer día, incluidas **las cinco pantallas difíciles** |
| **Total** | 126 | **138** | |

**Las 29 bloqueadas** son `PT-007`, `020`, `023`, `024`, `034`, `035`, `036`, `037`, `039`, `040`, `041`, `042`, `044`, `047`, `048`, `049`, `074`, `077`, `080`, `081`, `094`, `106`, `114`, `118`, `121`, `122`, `123`, `124` y la nueva `130`.

**Las 10 parciales** son `PT-008`, `012`, `062`, `065`, `076`, `083`, `105`, `109`, `119` y la nueva `129`.

### 5.0 El desglose por cliente subcontaba el campo

El encabezado anterior decía «102 administrativo · 23 campo · 1 pública». Cuadraba solo porque **las duales `A/C` se contaban como administrativas**, y el [mapa §0.2](mapa-de-navegacion.md) es tajante en que son **dos productos distintos, no uno responsive**: una superficie dual hay que diseñarla dos veces.

| Columna `Cli` | Sobre las 126 originales | **Ahora** |
|---|---|---|
| `A` solo administrativo | 95 | **103** |
| `A/C` dual — se diseña en los dos clientes | 7 | **9** |
| `C` solo campo | 23 | **25** |
| `P` pública | 1 | **1** |

> **Las superficies de campo a diseñar son 34, no 25.** Son las 25 de la §3 más las 9 duales: `PT-004`, `037`, `040`, `041`, `042`, `094`, `095`, `129`, `130`. Es alrededor de un **tercio más de trabajo de campo** del que sugería el encabezado anterior — y el cliente de campo es el que tiene las restricciones duras: guantes, sol directo, batería contada y cero red.

### 5.1 Por dónde empezar

Las cinco pantallas difíciles son las que más valor destruyen si se diseñan mal, y **todas se pueden empezar hoy**. Cuatro no replican papel en absoluto; la quinta, `PT-105`, tiene una parte que sí — ver el aviso al pie de esta sección.

| Orden | Pantalla | Por qué primero |
|---|---|---|
| 1 | `PT-053` / `PT-054` **Cola de conflictos** | La más difícil del sistema. Si se deja para el final, se diseña bajo presión y mal |
| 2 | `PT-105` **Registro en ruta** | Decide la adopción. Si el motorista no la usa, todo lo demás da igual |
| 3 | `PT-013` **Bandeja de autorización** | Es el cuello de botella del proceso completo |
| 4 | `PT-064` **Conciliación galonaje contra kilometraje** | Es lo que el Tribunal Superior de Cuentas va a mirar |
| 5 | `PT-028` **Rechazo por licencia no habilitante** | Es el bloqueo de mayor valor legal, y el usuario no lo resuelve reintentando |

Después, el resto del cliente de campo (`PT-103` a `PT-127`, `PT-131`, quitando las que replican papel), porque comparte sistema de interacción con `PT-105` y porque tiene las restricciones más duras.

> ### Aviso `HB34-71` — qué parte de `PT-105` se diseña hoy y cuál no
>
> `PT-105` estaba marcada `Parc.` en la tabla y a la vez declarada *«no replica papel, se puede diseñar hoy»* en tres sitios. Las dos cosas son ciertas a medias, y el diseñador tenía derecho a saberlo **antes** de empezar, no a mitad del trabajo:
>
> | Parte de `PT-105` | Estado |
> |---|---|
> | Los tres botones *Llegué · Salí · Estoy esperando*, el área táctil, la legibilidad al sol, la confirmación de guardado sin red y el contador de pendientes | **Se diseña hoy.** No tiene equivalente en papel: es lo que el sistema hace y el talonario no |
> | El bloque de campos que reproduce la **hoja de bitácora** por hito | **Espera el insumo #2**, y además el **#46**: si el talonario trae folio propio, hay dos numeraciones que cruzar |
>
> Es la pantalla que el [mapa §7](mapa-de-navegacion.md) llama *«la navegación más importante del sistema»* y de la que dice que *«decide la adopción»*. El diseño ya la dibujó suponiendo que **el único dato obligatorio del hito es el odómetro** ([`mockups §3.1`](mockups/README.md)); si el talonario pide más campos por hito, esa suposición cae.
>
> **Queda abierto:** `brief-para-diseno.md` sigue situándola como trabajo #3 «disponible desde el primer día» sin este matiz. Ese archivo no se toca desde aquí — hay que corregirlo con el mismo texto.

### 5.2 Las 29 bloqueadas, y qué formato hay que pedirle a la institución

Esta lista es el contenido del **insumo #2** visto desde el diseño. Sirve para pedirle a la institución exactamente lo que hace falta, en lugar de "los formatos".

> **Corrección `HB34-65`.** El título decía «las 27» y la lista enumeraba 28. Además faltaba `PT-062`: estaba marcada `Parc.` y no aparecía en ninguna fila, de modo que su formato —el acta de relevo, que ya se pedía para `PT-118`— no se estaba pidiendo desde ella.

| Formato en papel que hay que conseguir | Pantallas que desbloquea |
|---|---|
| Requisición o solicitud de vehículo | PT-007, PT-122 (y PT-008, PT-012 parciales) |
| Orden de misión | PT-034, PT-035, PT-036, PT-037 |
| Permiso de circulación en día u hora inhábil y su salvoconducto | PT-020, PT-023, PT-024 |
| Hoja de salida / control de despacho del predio | PT-039, PT-121 |
| Acta de entrega-recepción del vehículo | PT-040 |
| Bitácora de vehículo (talonario, con su folio propio si lo trae — insumo #46) | PT-042, PT-114, PT-123 (y PT-105, PT-119 parciales) |
| Solicitud de fondo de combustible | PT-044 |
| Vale de combustible y su constancia de entrega | PT-047, PT-048, PT-041 |
| Acta de anulación de vale | PT-049 |
| Acta de relevo de motorista | PT-118 (y **PT-062** parcial — la resolución del mismo relevo desde oficina) |
| Alta de bien / ficha de inventario del vehículo | PT-074 (y PT-076 parcial) |
| Tarjeta de responsabilidad | PT-077 |
| Acta de descargo o baja de bien | PT-080, PT-081 |
| Acta de constatación física | PT-124 |
| Manifiesto de personas trasladadas y su lista de abordo | PT-094, **PT-130** (y **PT-129** parcial — la persona sin documento) |
| Constancia de entrega de carga | PT-106 |
| Descargo o liquidación de misión | PT-065 (parcial) |
| Registro de licencia del motorista | PT-083 (parcial) |
| Control de combustible del motorista | PT-109 (parcial) |

Complementan el insumo #2: **#46** (¿el talonario de bitácora trae folio propio? Si se conserva, hay dos numeraciones que cruzar) y **#70** (parque real de impresoras y tamaño de papel, que decide si el QR impreso es vía primaria o solo conveniencia).

---

## 6. Todo formato impreso, sin excepción

Aplica a los documentos que producen **PT-020**, PT-023, **PT-024**, PT-034 a PT-037, PT-040, PT-042, PT-047 a PT-049, PT-077, PT-080, PT-081, PT-094, PT-106, PT-114, PT-118, PT-124 y **PT-130**:

- **Folio único**, que no se duplica ni se recicla, aunque se haya emitido sin red ([RNF-21](../02-requisitos/no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md)).
- **Código QR de verificación**, grande y en posición fija.
- **Espacio para firma y sello**. No hay firma electrónica certificada en el país: la autorización es interna, con registro completo de quién, cuándo, desde dónde y sobre qué contenido.
- **Hash del documento electrónico en el pie**, legible a simple vista para el contraste manual cuando no hay datos móviles.
- **Legible en impresora matricial o láser común, tamaño carta, útil en blanco y negro.** Nada puede depender del color para significar.

> **Corrección `HB34-72` — faltaban `PT-020` y `PT-024`.** Ambas están marcadas `Papel = Sí` y ambas producen documento oficial con folio, y no estaban en esta lista.
>
> **`PT-024` es la sensible.** Una reemisión del permiso por cambio de elementos amparados **no es la edición del permiso vigente**: es un **documento nuevo, con folio nuevo, que declara «sustituye al folio X»**, y el anterior queda `ANULADO` con su asiento. Si esta pantalla no figura entre las que producen folio, el diseño va a dibujar un formulario de edición — y un folio que cambia de contenido sin dejar rastro es exactamente el `0` que [`RNF-21`](../02-requisitos/no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md) prohíbe.
>
> Se añade también **`PT-130`**, la lista de abordo que `HU-114` exige imprimir con folio y QR al cerrar el manifiesto.
>
> **Queda por confirmar** si `PT-041` (constancia de entrega del fondo), `PT-044` (solicitud de fondo) y `PT-121` (hoja de salida del predio) llevan folio propio o solo firma y sello. Lo decide el formato del insumo #2; hasta entonces no se los agrega a esta lista por si acaso, porque agregar un folio que no existe crea una numeración que nadie va a llevar.

**El salvoconducto (`PT-023`) es el caso más exigente del sistema.** Lo va a revisar un agente en carretera, de pie, posiblemente de noche, con luz de linterna. Los cuatro datos que ese agente necesita —vehículo, ventana temporal autorizada, autoridad que firmó y vigencia— van **en el tercio superior, en cuerpo grande**, antes que cualquier otra cosa. Todo lo demás es secundario a eso.

> **Tensión declarada, no resuelta ([`mockups §5.9`](mockups/README.md)).** Esta regla del tercio superior y la paridad pantalla ↔ papel pueden chocar: si el formato vigente del salvoconducto **no** pone esos cuatro datos arriba, hay que decir cuál gana antes de dibujar `PT-023`. La postura de este documento es que en el salvoconducto —y solo en él— **gana la legibilidad en carretera**, porque su lector no es el operador que conoce el formato sino un agente que lo ve por primera vez con una linterna. Requiere confirmación del PO junto con el insumo #2.

---

## 7. Lo que este inventario no cubre

### 7.1 Módulos sin historias, y por tanto sin pantallas

- **M-11 Mantenimiento y Taller** y **M-12 Incidentes** más allá del registro en ruta: el Bloque 3 no escribió historias para ellos todavía. Las pantallas de `ACT-11` Encargado de Mantenimiento se inventariarán cuando existan sus historias. Lo que sí está cubierto es lo que toca la misión: declarar el vehículo fuera de circulación (`PT-061`) y su estado operativo (`PT-072`, `PT-079`).
- **M-18 Peajes** en su faceta de administración del catálogo de puntos y tarifas: entra en `PT-098` y `PT-099` como catálogo y parámetro, sin pantalla propia hasta que haya historias.

### 7.2 `M-17` Traslado de personas externas — hueco cerrado, con tres cabos sueltos

> **`HB34-66`.** Hasta esta versión, las quince historias de M-17 (`HU-111` a `HU-125`) **no aparecían en ninguna fila**, y esta sección declaraba honestamente los huecos de M-11, M-12 y M-18 sin mencionar éste. Ya está inventariado: `PT-128` a `PT-138`, más el retrazado de `PT-093`, `PT-094`, `PT-095` y `PT-106`. Las quince historias tienen pantalla.

Lo que **no** queda resuelto:

| Cabo suelto | Consecuencia | Dónde se decide |
|---|---|---|
| **El Oficial de Información Pública no es un actor catalogado.** `HU-121`, `HU-122` y `HU-123` lo nombran con `[C]`. `PT-134`, `PT-135` y `PT-136` quedan asignadas al actor que la historia pone primero | La navegación de hábeas data no tiene raíz propia: hoy cuelga de la del auditor, que es **solo lectura y no rectifica nada**. `PT-135` rectifica | [`actores-y-roles.md`](../01-negocio/actores-y-roles.md) — necesita un `ACT-xx` nuevo o una nota expresa |
| **`PT-128`, `PT-132` y `PT-137` no tienen caso de uso.** Son actos de `ACT-01` sobre el dato personal: fundamentar el campo sensible, fijar el alcance de visibilidad y depurar | `RNF-17` exige hábeas data resuelto *«≤ 5 min desde la interfaz, sin intervención de desarrollo»* y **0** datos personales sobrevivientes a la depuración. Sin criterio de aceptación, esos umbrales no se verifican | El mismo `CU-19` que pide `HB34-67` |
| **El formato en papel del manifiesto y de la lista de abordo** (insumo #2) | `PT-094`, `PT-130` bloqueadas; `PT-129` parcial | Insumo #2 |

### 7.3 `HB34-67` — pantallas que siguen sin historia

De las catorce que el hallazgo señaló, dos se cerraron por trazabilidad (`PT-093` → `HU-118`, `PT-095` → `HU-117`/`HU-118`/`HU-120`) y dos eran correctas porque citan su `RNF` en lugar de una historia (`PT-101` → `RNF-20`, `PT-102` → `RNF-09`). **Quedan diez, y no todas pesan igual:**

| Pantallas | Qué falta | Gravedad |
|---|---|---|
| `PT-099` `PT-100` `PT-092` | **El ciclo de vida completo del parámetro normativo**: cargar, poner en vigencia con doble control, consultar el histórico. De ahí cuelgan `RNF-05`, el invariante M-01 del modelo, `RN-39` a `RN-42` y `actores-y-roles §4.3` | **Alta.** No se envían a diseño hasta tener `CU-19` |
| `PT-096` `PT-098` | Usuarios, puestos y catálogos maestros. Ninguna historia del backlog los menciona | Media |
| `PT-001` `PT-002` `PT-005` | Ingreso con selección de puesto, inicio del puesto, buscador con alcance de datos. Son **transversales**: implementan `R-1` y el alcance de datos de `actores-y-roles §3`, que es donde vive su criterio | Media. Se diseñan hoy citando esa sección como fuente |
| `PT-088` `PT-090` | Pista de auditoría y paquete de evidencia. `RNF-18` fija su criterio duro —*el mismo día y completo*—, pero no hay `HU` | Media |

**Lo que no se hizo desde aquí:** escribir `CU-19` ni las historias. Este documento no tiene autoridad sobre `docs/02-requisitos/`. **Se pide como insumo del Bloque 5.**

### 7.4 Preguntas abiertas que este inventario no puede cerrar

- **`HB34-73` — ¿quién ejecuta la constatación física del vehículo?** Hoy `PT-124` la ejecuta `ACT-14` porque la acción 23 de la matriz da a `ACT-13` solo consulta. Si la práctica institucional es que la haga el custodio —que es quien tiene el vehículo delante—, hace falta una **acción propia en la matriz**, no una excepción en la pantalla.
- **`HB34-69` / `DP-002` — la operación de las delegaciones pequeñas.** Mientras el insumo #26 siga abierto, `PT-121` y `PT-041` no son ejecutables por `ACT-10` y el camino es el escalamiento a sede. **Si el insumo se resuelve en contra, hay que rediseñar la navegación del actor que sostiene la operación rural**, no ajustar un permiso.
- **`PT-012` «Registro de salida de emergencia»** ([`mockups §5.3`](mockups/README.md)) — no está claro si pertenece al **Nivel 3, convalidación de emergencia**, que `DP-002` conserva, o si cae con el régimen de excepción suspendido. Por eso el diseño no la dibujó. Lo decide `DP-002` o un `DP-003`, no el inventario.
- **`PT-041` frente a `PT-048`** ([`mockups §5.5`](mockups/README.md)) — puede que sean el mismo acto visto desde dos lugares.
- **El cronograma de flota semanal** ([`mockups §4.b`](mockups/README.md)) — el diseño lo dibujó y **no está en este inventario**. Sin él, la única forma de saber si un vehículo está libre el jueves es abrir las misiones una por una. Si el PO lo acepta, entra como `PT-139`; el ID queda reservado y **no se usa para otra cosa**.

### 7.5 Lo que es del diseñador y no de aquí

- **El sistema visual** — tipografía, paleta, retícula, componentes. El stack de interfaz está diferido al Sprint 2 por [`ADR-000`](../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md), así que **los mockups deben ser agnósticos de tecnología**.
- **El orden de los campos de las 29 pantallas bloqueadas.** Lo fija el formato de la institución, no el diseño.
