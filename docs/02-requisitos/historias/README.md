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

**41 refinadas · 69 en borrador.** Las que están en borrador **no lo están por descuido**: cada una declara qué insumo o decisión le falta. Cuatro lo dicen en el propio campo de estado.

Ejemplos de por qué una historia se queda en borrador y eso es lo correcto:

- `HU-008` emergencia convalidada — el `[C]` de quién convalida y en qué plazo **es la lógica misma**, no un parámetro que se rellena después
- `HU-012` autorización multinivel — sin el esquema de niveles de ARGOS (#16) no se cablea ningún umbral
- `HU-019` verificación en carretera — sin decidir si se expone un punto público con despliegue on-premise, **el QR no tiene a dónde apuntar**. La vía degradada — huella impresa, código corto, consulta telefónica — sí es implementable y queda separada
- `HU-037` emisión anticipada — necesita decisión de producto (`HB3-14`)

## Solapamientos entre lotes — delimitación

Las historias las escribieron **cuatro analistas en paralelo**, y ocho pares se solapan. `HU-021`–`HU-070` entran desde el **flujo de la misión**; `HU-071`–`HU-110` desde el **expediente y el dinero**. Ambos ángulos son legítimos, pero la duplicación hay que cerrarla antes de refinar.

**Los IDs no se reciclan.** La delimitación es por alcance, no por borrado:

| Par | Quién manda | Qué cubre el otro |
|---|---|---|
| `HU-041` ↔ `HU-076`, `HU-079` | `HU-041` — la entrega dentro de `T-12` es acto del despacho | `HU-076` la emisión en `PROGRAMADA`; `HU-079` el ciclo del instrumento |
| `HU-049`, `HU-050` ↔ `HU-085`, `HU-090` | `HU-049`/`HU-050` — el registro en carretera | `HU-085`/`HU-090` la conciliación y el reclamo |
| `HU-051`, `HU-052` ↔ `HU-082`, `HU-087` | `HU-051`/`HU-052` — la captura sin red | `HU-082`/`HU-087` la comprobación y la unicidad |
| `HU-053` ↔ `HU-084` | `HU-053` — la validación en captura | `HU-084` el acumulado del vehículo, independiente del instrumento |
| `HU-024` ↔ `HU-098` | `HU-024` — el bloqueo al programar | `HU-098` la derivación de la categoría desde la ficha técnica |
| `HU-025` ↔ `HU-109` | `HU-025` — la verificación al asignar | `HU-109` el expediente de habilitación |
| `HU-034` ↔ `HU-082` | `HU-034` — la hoja impresa | `HU-082` el registro del abastecimiento |
| `HU-032` ↔ `HU-081` | `HU-032` — la sección impresa de peajes | `HU-081` la categoría y tarifa esperadas |

**Regla general:** el lote de flujo manda en **el acto y su momento**; el lote de expediente manda en **el dato y su ciclo de vida**. Donde una historia necesite lo que la otra produce, lo referencia — no lo reimplementa.

## Lo que ninguna historia hace

**Ninguna cablea un número normativo.** Tarifas, umbrales de desviación, plazos, horarios hábiles, feriados, periodicidad del fondo y criterio de vencimiento de licencia van todos como **parámetro con vigencia**, con su insumo citado. Los números que aparecen en los escenarios Gherkin están marcados como ejemplo.

Es la premisa rectora #6 aplicada al nivel donde más fácil se rompe: un criterio de aceptación con un número adentro se convierte en una constante en el código, y nadie vuelve a preguntarse de dónde salió.
