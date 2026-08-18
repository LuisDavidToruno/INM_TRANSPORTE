# Backlog priorizado

**110 historias**, `HU-001` a `HU-110`. Índice completo en [`docs/02-requisitos/historias/`](../02-requisitos/historias/README.md).

Priorizado por **valor y dependencia técnica**, no por módulo. El orden de los módulos en `CLAUDE.md` es un mapa del dominio, no un plan de construcción.

## Criterio de priorización

1. **Lo que nada funciona sin ello va primero** — organización, catálogos, flota y motoristas. Sin vehículo y sin licencia no hay nada que programar.
2. **Después, el camino más corto a una misión completa.** Solicitar → autorizar → programar → despachar → retornar → liquidar → cerrar. Una misión que atraviesa el ciclo entero, aunque sea con la funcionalidad mínima, vale más que cinco módulos a medias.
3. **El dinero después del flujo**, porque el fondo se asigna a una misión que ya debe existir.
4. **Lo que produce evidencia para auditoría, al final** — pero antes del piloto, no después.

## Advertencia sobre el roadmap vigente

El [roadmap](README.md) coloca la **operación desconectada en el Sprint 7**, después de la bitácora del Sprint 5. **Eso no se sostiene.**

La captura sin red no es un módulo que se agrega al final: es una **propiedad del cliente de campo**. La bitácora del Sprint 5 ya se llena en carretera sin señal — si se construye asumiendo conectividad y se le agrega offline dos sprints después, hay que reescribirla entera, junto con el modelo de identificadores y la capa de sincronización.

**Propuesta: adelantar el núcleo de `M-16` al Sprint 5**, junto con la bitácora. Lo que sí puede esperar al Sprint 7 es la cola de conflictos avanzada y la reconciliación del espejo — no el almacenamiento local ni los identificadores generados en el cliente.

Es la clase de decisión que sale barata ahora y carísima después. Decide el PO.

## Asignación por sprint

### Sprint 3 — Catálogos, flota y motoristas

Es el cimiento: sin expediente de vehículo y sin licencia registrada, ninguna otra historia se puede probar.

| Rango | Qué entrega |
|---|---|
| `HU-096` – `HU-104` | Expediente del vehículo: alta con título de tenencia, placa y estado de lámina, ficha técnica, tarjeta de responsabilidad, constatación, vencimientos, habilitación, descargo y retiro de flota |
| `HU-105` – `HU-110` | Habilitación del motorista: licencia como dato propio, derivación de la matriz, vigencia y alertas, inhabilitación |

**`HU-105` es la historia más importante de este sprint.** Es la única fuente de `BD-02`, el bloqueo de mayor valor legal del sistema. Sin ella, la verificación de licencia no tiene de dónde leer.

**`HU-098` habilita a `HU-024`**: la ficha técnica es lo que permite derivar la categoría de peaje, que en el Sprint 5 bloquea la programación.

### Sprint 4 — Solicitud y autorización ⭐ núcleo de valor

| Rango | Qué entrega |
|---|---|
| `HU-001` – `HU-008` | Registrar la solicitud con su objeto de traslado, captura por encargo, estimado de peajes, captura sin red |
| `HU-009` – `HU-015` | Autorizar: bandeja, bloqueo de segregación y escalamiento, registro inmutable, delegación |
| `HU-016` – `HU-020` | Permiso de circulación en día u hora inhábil y salvoconducto |

**`HU-003` y `HU-010` van juntas o no van.** La primera produce el dato —capturador ≠ solicitante de derecho, declarado— y la segunda lo usa para bloquear. Separadas, el bloqueo queda ciego, que es exactamente el defecto `HB3-01` que acabamos de corregir.

### Sprint 5 — Programación, despacho y bitácora

| Rango | Qué entrega |
|---|---|
| `HU-021` – `HU-030` | Programar: compatibilidad, documentación, categoría de peaje, habilitación de quien conduce, reserva exclusiva |
| `HU-031` – `HU-037` | Emitir el juego documental, consumir folio, imprimir con paridad |
| `HU-038` – `HU-045` | Despachar, entregar el fondo contra firma, sustituir y relevar |
| `HU-062` – `HU-065` | Retorno y cierre de bitácora, incluido el retorno constatado |
| **`HU-046`, `HU-054`, `HU-066`** | **Adelantadas de `M-16`**: captura local, cola de pendientes, sincronización reanudable. Ver la advertencia de arriba |

### Sprint 6 — Combustible, peajes y liquidación

| Rango | Qué entrega |
|---|---|
| `HU-071` – `HU-080` | Fondo del período, cuota trimestral, emisión y entrega de la asignación |
| `HU-081` – `HU-087` | Consumo, abastecimiento sin red, paso por caseta, comprobantes |
| `HU-088` – `HU-095` | Conciliación galonaje↔kilometraje, peajes punto por punto, cierre con y sin hallazgo |

**`HU-086` es condición de las demás de peaje.** Declara *no concluyente* toda discrepancia sobre un punto cuya tarifa no está verificada. Sin ella, y con el insumo #21 abierto, **el detector produce reclamos falsos en masa el día uno**.

### Sprint 7 — Ejecución en ruta y seguimiento

| Rango | Qué entrega |
|---|---|
| `HU-047` – `HU-053`, `HU-055` – `HU-057` | Arribos, esperas en sitio, entregas, peajes y abastecimiento en ruta, última posición conocida |
| `HU-058` – `HU-061` | Interrupción en ruta y sus desenlaces |
| `HU-067` – `HU-070` | Cola de conflictos, reconciliación del espejo, registro que llega tarde |

### Sprint 8 — Integraciones, reportes y piloto

Historias por escribir: `M-20` integraciones con ARGOS y Talento Humano, `M-14` reportes y paquete de evidencia, `M-17` traslado de personas externas.

**`M-17` está sin cubrir en el backlog actual.** Es alcance confirmado por el PO en `DP-001`, y no se escribieron historias porque los casos de uso no lo desarrollaron. Hay que corregirlo antes de cerrar el Sprint 0.

## Estado de Definition of Ready

**41 marcadas `Refinada`, 69 `Borrador`** — pero ese conteo no es fiable.

Las historias las escribieron cuatro analistas en paralelo y **aplicaron la DoR con criterios distintos**: dos marcaron `Borrador` ante cualquier `[C]`; los otros dos aplicaron el criterio escrito, que admite un `[C]` cuando no bloquea el núcleo y hay insumo abierto.

El resultado es que hay historias perfectamente refinables marcadas en borrador. `HU-096` lo está porque no se sabe si el correlativo del vehículo se compone por delegación — pero eso es justo el **ejemplo válido** de la plantilla: la lógica no depende del valor.

**Acción propuesta:** una sesión de refinamiento de una hora con el PO, recorriendo las 69 en borrador y aplicando el criterio real. La estimación es que **la mayoría pasa a refinada sin tocar nada**.

Lo que sí está legítimamente bloqueado:

| Historia | Por qué el `[C]` *es* la lógica |
|---|---|
| `HU-008` | Quién convalida una emergencia y en qué plazo. Sin eso no hay flujo que implementar |
| `HU-012` | Sin el esquema de niveles de ARGOS no hay umbral que evaluar |
| `HU-019` | Sin decidir si se expone un punto público con despliegue on-premise, **el QR no tiene a dónde apuntar** |
| `HU-037` | Emisión anticipada: requiere decisión de producto `HB3-14` |
| `HU-071` – `HU-075` | La periodicidad del fondo define si el objeto es de período o de misión. Es estructural, no un parámetro |

## Lo que bloquea de verdad

De los **78 insumos abiertos**, solo dos bloquean construcción:

- **#2 formatos en papel** — condiciona `HU-031`, `HU-033`, `HU-034`, `HU-040` y todas las pantallas de captura del Bloque 4
- **#7 / `PROP-01`** periodicidad del fondo — condiciona el Sprint 6 completo

El resto tiene tratamiento provisional documentado y no impide avanzar.
