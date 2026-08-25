# Backlog priorizado

**150 historias**, `HU-001` a `HU-150`, sin huecos. Índice completo en [`docs/02-requisitos/historias/`](../02-requisitos/historias/README.md).

**Reescrito el 2026-08-25** tras la revisión adversarial del Bloque 3. La versión anterior tenía siete defectos de secuencia — historias asignadas a un sprint cuya precondición estaba en otro posterior — y se había quedado congelada en 110 historias.

Priorizado por **valor y dependencia técnica**, no por módulo. El mapa de módulos de `CLAUDE.md` describe el dominio; no es un plan de construcción.

## Criterio de priorización

1. **Lo que nada funciona sin ello va primero** — organización, catálogos y parámetros. Sin puestos con rol y sin parámetros con vigencia, ninguna historia posterior se puede probar.
2. **Después, el camino más corto a una misión completa.** Una misión que atraviesa el ciclo entero, aunque sea con lo mínimo, vale más que cinco módulos a medias.
3. **El dinero después del flujo**, porque el fondo se asigna a una misión que ya debe existir.
4. **Nada se programa antes que aquello de lo que depende.** Suena obvio; la versión anterior lo incumplía en cuatro lugares.

## Lo que cambió respecto de la versión anterior

| Hallazgo | Qué estaba mal | Cómo queda |
|---|---|---|
| `HB34-11` | El backlog decía 110 historias y nunca incorporó las 15 de M-17 | **150 historias**, incluidas M-17 y el lote nuevo de M-01 y M-02 |
| `HB34-05` | M-01 y M-02 sin ninguna historia, y el Sprint 3 se llamaba *Catálogos, flota y motoristas* | **25 historias nuevas** `HU-126` a `HU-150`. Pasan a ser el **Sprint 3 completo** |
| `HB34-04` | `HU-041` en Sprint 5 exigía el fondo, que estaba en Sprint 6 | El fondo del período sube al **Sprint 5**, antes de la entrega |
| `HB34-08` | El código de autorización fuera de línea se consumía en Sprint 5 y se generaba en Sprint 7 | `HU-055` sube al **Sprint 5** |
| `HB34-09` | La sincronización entraba en Sprint 5 sin el protocolo de huecos ni la cola de conflictos | `HU-067` y `HU-068` suben al **Sprint 5**. Sin ellas, llegan registros fuera de orden y el comportamiento es indefinido |
| `HB34-10` | De los 12 pares delimitados, en cuatro la historia que manda iba después | Cada par queda en el **mismo sprint**, y la que manda no va después |
| `HB34-18` | Las 150 decían *Sprint: sin asignar* | La asignación vive **acá**, no en cada ficha. Un sprint se decide una vez, no 150 |

## Asignación por sprint

### Sprint 3 — Organización, catálogos y parámetros

**Es el cimiento, y antes no existía.** Sin puestos con rol vigente y sin parámetros con vigencia aprobada, ninguna historia posterior se puede probar — `HU-001` ya daba por existente *"un catálogo de motivos de viaje vigente"* y *"un rol vigente sobre la dependencia"*.

| Rango | Qué entrega |
|---|---|
| `HU-126` – `HU-140` | Estructura institucional, puestos, roles con alcance y vigencia, suplencia, traspaso de custodias, autoría histórica inmutable |
| `HU-141` – `HU-150` | Catálogos con vigencia, tipos de vehículo con atributos que resuelven compatibilidad, parámetros normativos y su **doble control** |

**`HU-145` y `HU-146` son las críticas de este sprint.** Son la aprobación de la puesta en vigencia y el bloqueo de la autoaprobación. Sin ellas, quien administra el sistema **podría subir por sí solo el umbral de desviación de consumo y hacer desaparecer los hallazgos de auditoría sin tocar un dato operativo.**

`HU-139` sostiene `RNF-15`: la autoría congela **persona y puesto**, y ninguno se reasigna jamás.

### Sprint 4 — Flota y motoristas

| Rango | Qué entrega |
|---|---|
| `HU-096` – `HU-104` | Expediente del vehículo: alta con título de tenencia, placa y estado de lámina, ficha técnica, tarjeta de responsabilidad, constatación, vencimientos, descargo y retiro de flota |
| `HU-105` – `HU-110` | Habilitación del motorista: licencia como dato propio, derivación de la matriz, vigencia y alertas |

**`HU-105` es la historia más importante del sprint.** Es la única fuente de `BD-02`, el bloqueo de mayor valor legal del sistema.

**`HU-098` habilita a `HU-024`:** la ficha técnica es lo que permite derivar la categoría de peaje, que bloquea la programación dos sprints después.

### Sprint 5 — Solicitud y autorización ⭐ núcleo de valor

| Rango | Qué entrega |
|---|---|
| `HU-001` – `HU-008` | Registrar la solicitud, captura por encargo, estimado de peajes, captura sin red |
| `HU-009` – `HU-015` | Autorizar: bandeja, bloqueo de segregación, escalamiento, registro inmutable, delegación |
| `HU-016` – `HU-020` | Permiso de circulación en día u hora inhábil y salvoconducto |

**`HU-003` y `HU-010` van juntas o no van.** La primera produce el dato —capturador ≠ solicitante de derecho, declarado— y la segunda lo usa para bloquear. Separadas, el bloqueo queda ciego: es el defecto `HB3-01`.

### Sprint 6 — Programación, despacho y bitácora

| Rango | Qué entrega |
|---|---|
| `HU-021` – `HU-030` | Programar: compatibilidad, documentación, categoría de peaje, habilitación de quien conduce, reserva exclusiva |
| `HU-031` – `HU-037` | Emitir el juego documental, consumir folio, imprimir con paridad |
| `HU-038` – `HU-045` | Despachar, entregar el fondo contra firma, sustituir y relevar |
| `HU-062` – `HU-065` | Retorno y cierre de bitácora, incluido el retorno constatado |
| **`HU-046`, `HU-054`, `HU-055`** | **Adelantadas de M-16 y M-08.** Captura local, cola de pendientes y **código de autorización fuera de línea** — que `HU-045`, `HU-037` y `HU-039` consumen como bloqueo |
| **`HU-066`, `HU-067`, `HU-068`** | **Adelantadas de M-16.** Sincronización reanudable, **retención por hueco de secuencia** y **cola de conflictos**. Las tres juntas o ninguna |
| `HU-071` – `HU-075` | **Adelantado de M-09.** El fondo del período, que `HU-041` exige para poder emitir |

**Este sprint absorbió cinco adelantos, y no es casualidad.** Es donde el sistema deja de ser formularios y empieza a operar en carretera: ahí convergen el offline, el folio, el fondo y la autorización sin red.

> **Sobre la operación desconectada.** No es un módulo que se agregue al final: es una **propiedad del cliente de campo**. La bitácora de este sprint ya se llena sin señal. Construirla asumiendo conectividad y agregarle offline después obliga a reescribirla entera, junto con el modelo de identificadores y la capa de sincronización.

### Sprint 7 — Combustible, peajes y liquidación

| Rango | Qué entrega |
|---|---|
| `HU-076` – `HU-080` | Emisión y entrega de la asignación de combustible |
| `HU-081` – `HU-087` | Consumo, abastecimiento sin red, paso por caseta, comprobantes |
| `HU-088` – `HU-095` | Conciliación galonaje↔kilometraje, peajes punto por punto, cierre con y sin hallazgo |

**`HU-086` es condición de las demás de peaje.** Declara *no concluyente* toda discrepancia sobre un punto cuya tarifa no está verificada. Sin ella, y con la tarifa aún `[P]`, **el detector produce reclamos falsos en masa el día uno.**

### Sprint 8 — Ejecución en ruta, seguimiento y personas externas

| Rango | Qué entrega |
|---|---|
| `HU-047` – `HU-053`, `HU-056`, `HU-057` | Arribos, esperas en sitio, entregas, peajes y abastecimiento en ruta, última posición conocida |
| `HU-058` – `HU-061` | Interrupción en ruta y sus desenlaces |
| `HU-069`, `HU-070` | Reconciliación del espejo, registro que llega tarde |
| `HU-111` – `HU-125` | **Traslado de personas externas**: manifiesto, cadena de custodia, necesidad de conocer, registro de consultas, hábeas data, depuración |

### Sprint 9 — Integraciones, reportes y piloto

Historias por escribir: `M-20` integraciones con ARGOS y Talento Humano, `M-14` reportes y paquete de evidencia, `M-11` mantenimiento y taller, `M-12` incidentes.

## Requisitos no funcionales sin historia — `HB34-14`

Eran nueve. Tras el lote de M-01 y M-02 quedan **cinco**, y hay que decidir qué hacer con cada uno:

| `RNF` | Qué exige | Cómo se verifica |
|---|---|---|
| `RNF-01` | Toda pantalla responde bajo umbral con el acervo histórico completo | **No necesita historia.** Es criterio de aceptación técnico de todo sprint |
| `RNF-10` | La caída del servidor no detiene la operación de campo | **Necesita historia.** Escribirla junto con el Sprint 6 |
| `RNF-16` | Español del dominio, y ningún mensaje de bloqueo deja al usuario sin saber qué hacer | **No necesita historia.** Es Definition of Done de cada una |
| `RNF-20` | Una sola pantalla le dice a alguien sin especialización qué está mal y qué hacer | **Necesita historia.** Va con `M-01`, Sprint 3 |
| `RNF-21` | Ningún folio se duplica ni se recicla, aunque se emita sin red | Tiene historias, pero **su batería de prueba no cubre el caso que rompe**: dos dispositivos de la **misma** delegación, ambos desconectados. Corregir la prueba |

## Estado de Definition of Ready

**41 refinadas · 109 en borrador**, sobre 150.

El conteo ahora es fiable: las **65 historias que decían solo «Borrador» declaran su razón concreta**, y las cinco que estaban marcadas `Refinada` sin estarlo bajaron.

Las 25 nuevas de M-01 y M-02 entran todas en borrador, y con motivo: dependen de los contratos de API de ARGOS y Talento Humano (#16, #17), de la matriz de licencias y las tarifas (#20, #21, #32), y del pronunciamiento de Auditoría Interna (#26, #27).

**Lo que legítimamente no se puede refinar** son las historias donde el `[C]` *es* la lógica, no un parámetro: quién convalida una emergencia y en qué plazo, el esquema de niveles de ARGOS, si se expone un punto público para el QR, la emisión anticipada, y la periodicidad del fondo — que define si el objeto es de período o de misión, y eso es estructural.

## Lo que bloquea de verdad

De los insumos abiertos, **dos bloquean construcción**:

- **#2 formatos en papel** — condiciona las historias de impresión del Sprint 6 y 29 pantallas del diseño
- **#7 / `PROP-01` periodicidad del fondo** — condiciona el Sprint 5 y el 7

El resto tiene tratamiento provisional documentado y no impide avanzar.
