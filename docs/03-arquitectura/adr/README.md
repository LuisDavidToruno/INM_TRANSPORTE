# Decisiones de arquitectura (ADR)

Un ADR registra **una decisión de arquitectura, su contexto y sus consecuencias**. Se escribe cuando la decisión se toma, no después.

**Un ADR no se edita cuando se cambia de opinión.** Se escribe uno nuevo que lo supera, y el anterior se marca `Reemplazada por ADR-xxx`. El historial de decisiones equivocadas es parte de la documentación.

Plantilla obligatoria: [`docs/plantillas/adr.md`](../../plantillas/adr.md).

## Índice

| ADR | Decisión | Estado |
|---|---|---|
| [`ADR-000`](ADR-000-diferir-seleccion-de-stack.md) | Diferir la selección del stack tecnológico al Sprint 2 | **Reemplazada por `ADR-002`** |
| [`ADR-001`](ADR-001-integracion-argos-talento-humano.md) | Integración con ARGOS y Talento Humano por espejo local con webhooks | Aceptada |
| [`ADR-002`](ADR-002-adoptar-el-stack-tecnologico.md) | Adoptar el stack tecnológico en el Sprint 0 | Aceptada |
| [`ADR-003`](ADR-003-cliente-de-campo-instalado.md) | El cliente de campo es una aplicación instalada, no web | Aceptada |
| [`ADR-004`](ADR-004-adjuntos-fuera-de-la-base.md) | Fotografías y adjuntos fuera de la base de datos | Aceptada |
| [`ADR-005`](ADR-005-identificadores-generados-en-el-cliente.md) | Los identificadores se generan en el cliente | Aceptada |
| [`ADR-006`](ADR-006-temporalidad-bitemporal.md) | Temporalidad bitemporal desde el modelo inicial | Aceptada |
| [`ADR-007`](ADR-007-marcas-de-tiempo-en-utc.md) | Marcas de tiempo en UTC con el desfase del dispositivo | Aceptada |
| [`ADR-008`](ADR-008-permisos-como-capacidad.md) | Los permisos se publican como capacidad, nunca como rol | Aceptada |
| [`ADR-009`](ADR-009-modulos-verticales.md) | Módulos verticales con reglas compartidas puras y guardas | Aceptada |

Origen de `ADR-002` a `ADR-009`: la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md), autorizada por el Product Owner.

## Los cinco que no se pueden cambiar después

Ordenados por costo de revertirlos. Es la razón por la que se escribieron antes de la primera línea de código.

| # | Qué | Dónde | Qué cuesta revertirlo |
|---|---|---|---|
| 1 | La clave agrupada es **ULID**, no GUID aleatorio | `ADR-005` | Reescribir cada índice y cada clave foránea |
| 2 | `UseCompatibilityLevel(120)` en EF Core | `ADR-002` | Sin esto EF emite SQL que 2014 no entiende |
| 3 | Base de desarrollo en `COMPATIBILITY_LEVEL = 120` | `ADR-002` | Se desarrolla contra un estimador de cardinalidad que el destino no tiene |
| 4 | **Las dos parejas de fechas** de la bitemporalidad | `ADR-006` | Agregar el eje de vigencia después obliga a inventar una historia que no se tiene |
| 5 | **Rangos de folio por delegación** | `ADR-005` | El cliente no puede asignar folio definitivo en campo (`RNF-21`) |

## Decisión abierta

**`ADR-009` — ¿Clean Architecture con ceremonia o sin ella?** Se escribió **sin** ceremonia, que es lo que la evidencia medida en SICOV_CORE8 respalda. Es la única decisión de este lote que el Product Owner podría querer al revés. Si la cambia, se registra el motivo y se escribe el ADR que supera a `ADR-009`.
