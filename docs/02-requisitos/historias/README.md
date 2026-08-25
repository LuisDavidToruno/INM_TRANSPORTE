# Historias de usuario `HU-xxx`

**125 historias**, `HU-001` a `HU-125`, todas con criterios de aceptación en **Gherkin español**. Sin huecos en la numeración.

Cada historia es una **rebanada entregable** de un caso de uso: produce valor por sí sola y se termina en un sprint. No es una pantalla ni un campo.

Plantillas: [historia de usuario](../../plantillas/historia-de-usuario.md) · [criterios Gherkin](../../plantillas/criterios-aceptacion-gherkin.md) · [Definition of Ready](../../plantillas/definition-of-ready.md).

## Reparto por caso de uso

| Rango | Caso de uso | Módulo |
|---|---|---|
| `HU-001` – `HU-008` | [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) registrar solicitud | M-06 |
| `HU-009` – `HU-015` | [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) autorizar | M-06 |
| `HU-016` – `HU-020` | [CU-03](../casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) permiso de circulación | M-04, M-15 |
| `HU-021` – `HU-030` | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) programar | M-07 |
| `HU-031` – `HU-037` | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) emitir | M-15, M-07 |
| `HU-038` – `HU-042` | [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) despachar | M-07, M-08, M-09 |
| `HU-043` – `HU-045` | [CU-07](../casos-de-uso/CU-07-sustituir-vehiculo-o-motorista.md) sustituir | M-07, M-08 |
| `HU-046` – `HU-057` | [CU-08](../casos-de-uso/CU-08-ejecucion-en-ruta-sin-conectividad.md) ejecución sin red | M-08, M-16, M-19 |
| `HU-058` – `HU-061` | [CU-09](../casos-de-uso/CU-09-interrupcion-en-ruta-y-desenlace.md) interrupción | M-08, M-12, M-11 |
| `HU-062` – `HU-065` | [CU-10](../casos-de-uso/CU-10-registrar-retorno-y-cerrar-bitacora.md) retorno | M-08 |
| `HU-066` – `HU-070` | [CU-11](../casos-de-uso/CU-11-sincronizar-y-resolver-conflictos.md) sincronizar | M-16 |
| `HU-071` – `HU-075` | [CU-12](../casos-de-uso/CU-12-solicitar-y-aprobar-fondo-de-combustible.md) fondo | M-09 |
| `HU-076` – `HU-080` | [CU-13](../casos-de-uso/CU-13-emitir-y-entregar-asignacion-de-combustible.md) emisión y entrega | M-09, M-07 |
| `HU-081` – `HU-087` | [CU-14](../casos-de-uso/CU-14-registrar-consumo-de-combustible-y-peaje.md) consumo y peaje | M-09, M-18, M-08 |
| `HU-088` – `HU-092` | [CU-15](../casos-de-uso/CU-15-liquidar-la-mision-y-conciliar.md) liquidar | M-13, M-09, M-18 |
| `HU-093` – `HU-095` | [CU-16](../casos-de-uso/CU-16-cerrar-el-expediente-de-la-mision.md) cerrar | M-13, M-14 |
| `HU-096` – `HU-104` | [CU-17](../casos-de-uso/CU-17-alta-y-mantenimiento-del-expediente-del-vehiculo.md) expediente del vehículo | M-03, M-04 |
| `HU-105` – `HU-110` | [CU-18](../casos-de-uso/CU-18-registrar-y-mantener-la-habilitacion-del-motorista.md) habilitación del motorista | M-05 |
| `HU-111` – `HU-125` | **Traslado de personas externas** — sin caso de uso previo, escritas para tapar el hueco detectado al armar el backlog | **M-17** |

## Estado de Definition of Ready

**41 refinadas · 84 en borrador**, sobre 125 historias. El conteo anterior —«41 refinadas · 69 en borrador»— nunca incorporó las 15 historias de M-17 (`HB34-11`); el real antes de esta corrección era 46 y 79, y bajan cinco por los hallazgos de `H-B34-001`.

**Las 84 en borrador declaran, cada una en su propio campo `Estado`, qué insumo o decisión le falta.** Hasta la corrección de `HB34-17`, 65 decían únicamente «Borrador»: tenían notas `[C]` al pie, pero un `[C]` de parámetro no es lo mismo que declarar qué bloquea la historia, y el `DoR` exige lo segundo. La afirmación que este `README` hacía —*«no lo están por descuido: cada una declara qué le falta»*— no se sostenía; ahora sí.

Ejemplos de por qué una historia se queda en borrador y eso es lo correcto:

- `HU-008` emergencia convalidada — el `[C]` de quién convalida y en qué plazo **es la lógica misma**, no un parámetro que se rellena después
- `HU-012` autorización multinivel — sin el esquema de niveles de ARGOS (#16) no se cablea ningún umbral
- `HU-019` **y `HU-035`** verificación en carretera — sin decidir si se expone un punto público con despliegue on-premise, **el QR no tiene a dónde apuntar**. La vía degradada — huella impresa, código corto, consulta telefónica — sí es implementable y queda separada. `HU-035` bajó a borrador por `HB34-06`: declaraba el mismo `[C]` bloqueante y estaba `Refinada`
- `HU-037` emisión anticipada — necesita decisión de producto (`HB3-14`)
- `HU-071` a `HU-075` el ciclo del fondo — la periodicidad decide si el objeto es de período o de misión. Es estructural (`PROP-01` / insumo #7)
- `HU-106` tipos de vehículo habilitados — sin el texto de la reforma al Art. 48 la matriz licencia↔vehículo no se puede fijar, y esa matriz **es** la historia

### Las seis que bajaron de `Refinada` por la revisión `H-B34-001`

| Historia | Hallazgo | Qué estaba mal |
|---|---|---|
| [`HU-045`](HU-045-relevo-de-motorista-en-ruta.md) | `HB34-01`, `HB34-02`, `HB34-06`, `HB34-21` | Convertía en hallazgo posterior el bloqueo duro de `RN-10`, no evaluaba `I-11` sobre el entrante y se contradecía consigo misma en sus antecedentes |
| [`HU-004`](HU-004-envio-a-autorizacion-con-numero-de-expediente-y-congelamiento.md) | `HB34-03` | Bloqueaba el envío donde `RN-50` prohíbe bloquear, citando `RN-50` |
| [`HU-009`](HU-009-bandeja-de-autorizacion-con-validaciones-a-la-vista.md) | `HB34-03` | Retiraba la acción de autorizar donde `RN-50` prohíbe bloquear |
| [`HU-035`](HU-035-verificacion-en-carretera-por-qr.md) | `HB34-06` | Mismo `[C]` bloqueante que `HU-019` y estado opuesto |
| [`HU-041`](HU-041-emision-y-entrega-del-fondo-de-combustible.md) | `HB34-04` | Su precondición completa —la entidad *fondo*— se construye después |
| [`HU-088`](HU-088-conciliar-galonaje-contra-kilometraje.md) | `HB34-07` | La clasificación central no era determinable: `REVISAR` no tenía umbral y la tabla contradecía a los antecedentes |

`HU-045` y `HU-088` ya estaban en borrador o volvieron a él; las cinco que estaban marcadas `Refinada` sin estarlo eran `HU-004`, `HU-009`, `HU-035`, `HU-041` y `HU-045`.

## Solapamientos entre lotes — delimitación

Las historias las escribieron **cuatro analistas en paralelo**, y **doce** pares se solapan: los ocho detectados al armar este índice más los cuatro que encontró la revisión `H-B34-001`. `HU-021`–`HU-070` entran desde el **flujo de la misión**; `HU-071`–`HU-110` desde el **expediente y el dinero**. Ambos ángulos son legítimos, pero la duplicación hay que cerrarla antes de refinar.

**Los IDs no se reciclan.** La delimitación es por alcance, no por borrado:

| Par | Quién manda | Qué cubre el otro |
|---|---|---|
| `HU-045` ↔ `HU-061` | `HU-045` — el **acto** del traspaso en ruta: acta, corte de odómetro, custodia, fondo, código de autorización fuera de línea | `HU-061` la **revalidación de la habilitación del entrante**: matriz, vigencia en todo el rango, relevo declarado, segregación `I-11` |
| `HU-019` ↔ `HU-035` | `HU-035` — el **mecanismo** genérico de verificación pública por QR: respuesta mínima, registro de consultas, minimización | `HU-019` lo **propio del salvoconducto**: qué ampara, qué lo invalida y qué se muestra de él |
| `HU-023` ↔ `HU-101` | `HU-023` — el **acto** de asignar con documentación vencida | `HU-101` el **dato**: el parámetro de bloqueo por régimen de tenencia y su ciclo de vida |
| `HU-044` ↔ `HU-080` | `HU-044` — la **reversión de la misión**, y el texto del bloqueo por consumo | `HU-080` el **ciclo del instrumento**: folio, acta de devolución uno por uno, asiento reverso, plazo |
| `HU-041` ↔ `HU-076`, `HU-079` | `HU-041` — la entrega dentro de `T-12` es acto del despacho | `HU-076` la emisión en `PROGRAMADA`; `HU-079` el ciclo del instrumento |
| `HU-049`, `HU-050` ↔ `HU-085`, `HU-090` | `HU-049`/`HU-050` — el registro en carretera | `HU-085`/`HU-090` la conciliación y el reclamo |
| `HU-051`, `HU-052` ↔ `HU-082`, `HU-087` | `HU-051`/`HU-052` — la captura sin red | `HU-082`/`HU-087` la comprobación y la unicidad |
| `HU-053` ↔ `HU-084` | `HU-053` — la validación en captura | `HU-084` el acumulado del vehículo, independiente del instrumento |
| `HU-024` ↔ `HU-098` | `HU-024` — el bloqueo al programar | `HU-098` la derivación de la categoría desde la ficha técnica |
| `HU-025` ↔ `HU-109` | `HU-025` — la verificación al asignar | `HU-109` el expediente de habilitación |
| `HU-034` ↔ `HU-082` | `HU-034` — la hoja impresa | `HU-082` el registro del abastecimiento |
| `HU-032` ↔ `HU-081` | `HU-032` — la sección impresa de peajes | `HU-081` la categoría y tarifa esperadas |

**Regla general:** el lote de flujo manda en **el acto y su momento**; el lote de expediente manda en **el dato y su ciclo de vida**. Donde una historia necesite lo que la otra produce, lo referencia — no lo reimplementa.

**Cada par va a un solo sprint.** `HB34-10` mostró que los ocho pares originales quedaron partidos entre sprints y que en cuatro la historia que manda se construye **después** de la subordinada. Eso produce o una reescritura o dos caminos de captura para el mismo hecho — y en combustible, donde la unicidad del comprobante es un control, dos caminos de captura es un agujero. La corrección de secuencia es del backlog; la delimitación, que es lo que se puede escribir desde aquí, está aplicada en los artefactos y no solo declarada en esta tabla.

### `RN-67` se aplica en tres momentos, y no es un par

No hay contradicción de comportamiento —la ausencia de entrada en la matriz bloquea en las tres—, pero faltaba la frontera (`HB34-15`):

| Momento | Historia | Contra qué evalúa |
|---|---|---|
| Envío de la solicitud (M-06) | [`HU-002`](HU-002-bloqueo-de-compatibilidad-del-objeto-del-traslado.md) | El **tipo de vehículo requerido** y los objetos declarados |
| Asignación del vehículo (M-07) | [`HU-022`](HU-022-compatibilidad-vehiculo-objeto-del-traslado.md) | La **ficha técnica del vehículo concreto**, tramo por tramo |
| Programación con personas externas (M-17 · M-07) | [`HU-125`](HU-125-personas-externas-junto-con-carga-y-personal.md) | El par **personas externas × personal × carga**, con manifiesto y minimización |

**Nadie es dueño de poblar la matriz**: es catálogo de M-02, y M-02 no tiene ninguna historia (`HB34-05`). Es un hueco de cobertura, no de delimitación.

### `I-11` se comprueba en dos direcciones

`RN-01` bloquea **el segundo acto, sea cual sea el orden**. La mitad que faltaba escrita era la que dispara al **declarar o sustituir al conductor** (`HB34-02`):

| Disparo | Historias |
|---|---|
| Por el **acto de control** —autorizar, despachar, aprobar o entregar el fondo, liquidar— | `HU-010`, `HU-039`, `HU-073`, `HU-079`, `HU-091` |
| Por la **asignación del motorista** —declarar, sustituir, relevar— | `HU-025`, `HU-043`, `HU-045`, `HU-061` |

`HU-027` no lo evalúa: la reserva es exclusividad de franja, no habilitación de la persona, y remite a `HU-025`.

## Lo que ninguna historia hace

**Ninguna cablea un número normativo.** Tarifas, umbrales de desviación, plazos, horarios hábiles, feriados, periodicidad del fondo y criterio de vencimiento de licencia van todos como **parámetro con vigencia**, con su insumo citado. Los números que aparecen en los escenarios Gherkin están marcados como ejemplo.

Es la premisa rectora #6 aplicada al nivel donde más fácil se rompe: un criterio de aceptación con un número adentro se convierte en una constante en el código, y nadie vuelve a preguntarse de dónde salió.

La revisión `H-B34-001` verificó esta afirmación sobre los 919 escenarios y **no encontró ningún número normativo cableado**. Lo que sí encontró, en `HU-088`, es el fallo hermano y menos visible: **umbrales declarados como parámetro pero incoherentes con los ejemplos de la propia historia** (`HB34-07`). Un parámetro bien declarado con una tabla que lo contradice deja el criterio indeterminable, que a efectos del `DoR` es igual de malo que cablearlo. Corregido.

## Correcciones aplicadas de `H-B34-001`

Cada historia tocada lleva su **nota de corrección** visible citando el hallazgo. Resumen:

| Hallazgo | Historias corregidas |
|---|---|
| `HB34-01` licencia del relevo — bloqueo duro de `RN-10` frente a `RN-55` | `HU-045`, `HU-061` |
| `HB34-02` `I-11` al asignar o sustituir al motorista | `HU-025`, `HU-043`, `HU-045`, `HU-061`, `HU-027` (delimitación) |
| `HB34-03` `RN-50` es advertencia con acuse, no bloqueo | `HU-004`, `HU-009` |
| `HB34-06` verificación por QR: delimitación, `DESACTUALIZADO` y mensaje de folio | `HU-019`, `HU-035`, `HU-045` |
| `HB34-07` escala de clasificación de la conciliación | `HU-088` |
| `HB34-12` bloqueo por póliza vencida por régimen de tenencia, con acuse | `HU-101`, `HU-023` (delimitación) |
| `HB34-13` sección de casos especiales en M-17 | `HU-118`, `HU-119`, `HU-121`, `HU-122`, `HU-123`, `HU-124` |
| `HB34-15` delimitación de `RN-67` | `HU-002`, `HU-022`, `HU-125` |
| `HB34-16` un solo texto para el bloqueo por consumo | `HU-044`, `HU-080` |
| `HB34-17` razón del borrador en el campo `Estado` | las 65 que decían solo «Borrador» |
| `HB34-19` mensajes de rechazo especificados | `HU-104`, `HU-043` |
| `HB34-20` camino de rechazo | `HU-005`, `HU-054`, `HU-066` |
| `HB34-21` antecedentes contradictorios | `HU-045` |

**No se corrigen desde las historias** y quedan abiertos hacia sus artefactos: `HB34-04`, `HB34-08`, `HB34-09`, `HB34-10`, `HB34-11` y `HB34-18` son de [`backlog.md`](../../07-gestion/backlog.md); `HB34-05` es el lote de M-01 y M-02 que falta escribir; `HB34-14` es de los `RNF-xx`. `HB34-03` deja además dos contradicciones hacia arriba: `RNF-07` y los casos límite de `RN-10` reescriben a `RN-50` como bloqueo, y `RN-50` es la autoridad.
