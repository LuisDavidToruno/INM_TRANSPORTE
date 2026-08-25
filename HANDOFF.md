# Estado del trabajo

**Última actualización: 2026-08-25.**

Punto único de entrada para saber en qué va el proyecto. Si algo figura acá como abierto, está abierto; si se cierra, se saca de la lista el mismo día.

## Dónde está el proyecto

**Sprint 0 — Descubrimiento y definición. Los cinco bloques están escritos.**

No hay código, y no lo habrá hasta el Sprint 2: la selección de stack está deliberadamente diferida por [`ADR-000`](docs/03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md).

| Bloque | Qué produjo | Estado |
|---|---|---|
| 0 — Andamiaje | `CLAUDE.md`, 11 plantillas, 10 fichas normativas, 10 subagentes | ✅ Cerrado |
| 1 — Negocio | Visión, glosario, 17 actores, 14 procesos, máquina de estados, **97 reglas** | ✅ Revisado y corregido |
| 2 — Casos especiales | **28 casos** de la operación real, con su regla de resolución | ✅ Cerrado |
| 3 — Requisitos | 18 casos de uso, **125 historias** con Gherkin, 21 no funcionales, backlog | ⚠️ Escrito y revisado, **sin corregir** |
| 4 — Diseño | Modelo de datos con 43 entidades, 126 pantallas, **41 maquetadas** | ⚠️ Escrito y revisado, **sin corregir** |

**357 documentos, 41,880 líneas, 19 commits.**

## Lo que está abierto

### 46 hallazgos sin corregir

De las dos revisiones adversariales del 2026-08-24. Los informes completos están en [`docs/05-calidad/hallazgos/`](docs/05-calidad/hallazgos/).

**Del Bloque 3 — 21 hallazgos, 5 críticos.** Ninguno exige rehacer nada: cuatro son ediciones de historias y uno es escribir el lote de M-01 y M-02 que faltaba.

- `HB34-01` — dos historias son la misma y **se contradicen en un bloqueo duro**. Están en sprints distintos: el relevo de motorista se construiría dos veces con dos reglas
- `HB34-02` — la incompatibilidad *motorista sobre su propia misión*, declarada núcleo irreductible, **no tiene historia que la bloquee al asignar**
- `HB34-03` — dos historias bloquean donde la regla que citan **prohíbe bloquear**. Una delegación con cuatro días sin enlace no podría operar
- `HB34-04` y `HB34-05` — dependencias rotas entre sprints, y **M-01 y M-02 sin ninguna historia** pese a que el Sprint 3 se llama *Catálogos, flota y motoristas*

**Del Bloque 4 — 25 hallazgos.** Cuatro son los caros: los requisitos que **no se pueden agregar después** tienen agujero estructural.

- `HB34-50` — el modelo es **unitemporal** donde el requisito pide dos ejes de tiempo
- `HB34-51` — la cadena de auditoría se encadena por misión, así que **borrar una misión entera no rompe ninguna cadena**
- `HB34-52` — los rangos de folio son por delegación, no por dispositivo
- `HB34-53` — los adjuntos escapan a la depuración: **la foto del manifiesto con nombres manuscritos sobrevive**

> **Veredicto de ambas revisiones: no está listo para código, y no está lejos.** Las cuatro del Bloque 4 hay que cerrarlas antes de la primera línea — hoy cuestan una tarde.

### 82 insumos pendientes, de los cuales dos bloquean construcción

Registro completo en [`docs/07-gestion/insumos-pendientes.md`](docs/07-gestion/insumos-pendientes.md).

| # | Qué | A quién |
|---|---|---|
| **2** | **Formatos en papel vigentes** — 19 documentos. Bloquea 27 pantallas de captura | Institución. Lista lista en [`levantamiento/`](docs/07-gestion/levantamiento/) |
| **7** | **Periodicidad del fondo de combustible** — define si el objeto es de período o de misión. Es estructural | Gerencia Administrativa |
| **26** | Pronunciamiento de **Auditoría Interna** sobre segregación en delegaciones. **Es el que más tarda** | Auditoría Interna |
| **36** | ¿Hay cisterna o bidones de combustible? **Cambia el circuito completo de M-09** | Encargado de transporte |

**Un lote se cierra sin la institución:** cuatro PDF oficiales que sí tienen capa de texto y solo hay que descargar. Destraban la matriz licencia↔vehículo, el expediente del motorista y los códigos presupuestarios. *Veinte minutos con un navegador.*

### Decisiones pendientes del PO

1. **Ratificar o revertir [`DP-002`](docs/07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)** — segregación en delegaciones pequeñas
2. **El nombre de la institución en los mockups** — dicen *Instituto Nacional de Migración*; según `DP-001` el sistema es genérico y eso es configuración
3. **La contradicción del salvoconducto** — la paridad con el papel exige reproducir el formato; el requisito de campo exige que cuatro datos vayan arriba y en grande. Si el formato no los pone ahí, hay que decidir cuál gana
4. **¿El reclamo de peaje cierra la misión o la marca con hallazgo?** — resuelto provisionalmente a favor de lo primero
5. **Sesión de refinamiento** sobre las historias en borrador. Los cuatro analistas aplicaron el criterio de forma distinta; la mayoría pasaría sin tocarse

### Documentación exigida por LOKI

Auditado el 2026-08-25. `DECISIONES.md` y `HANDOFF.md` se escribieron ese día. Siguen ausentes **`ARQUITECTURA.md`** y **`DESPLIEGUE.md`**, y `README.md` y `CLAUDE.md` están `Parcial` — los cuatro por la misma causa: **no hay stack elegido**, así que no hay base de datos que nombrar, ni variables de entorno que listar, ni comandos de arranque. Se destraban solos cuando el stack se defina.

## Lo que está cerrado

**No confundir con lo anterior.** Esto ya se resolvió y no debe volver a listarse como pendiente:

- **Los 48 hallazgos de la revisión del Bloque 1**, incluidos los 8 críticos. Corregidos en `fd12ba5` y commits siguientes
- **Los 19 hallazgos de los casos de uso** (`H-B3-001`), corregidos en `c8cb324`, en una sola pasada sobre los artefactos autoridad
- **La contradicción de la tarifa de peaje** — resuelta el 2026-08-24 en contra de la fuente comercial, por cinco fuentes concordantes
- **El bloqueante de exoneraciones para M-18** — cayó por otro lado: un pick-up es categoría liviana, L. 22
- **El hueco de M-17** — tenía cero historias siendo alcance confirmado; ahora tiene 15
- **La entrega de diseño** — 41 pantallas verificadas contra los requisitos, sin vocabulario prohibido, sin botones de eliminar, sin *continuar de todos modos* sobre bloqueos duros

## Cómo seguir

**Lo inmediato, y no depende de nadie externo:** corregir los 46 hallazgos. Primero las cuatro del modelo de datos, porque son las únicas que se encarecen con el tiempo.

**Lo que hay que gestionar en paralelo:** la sesión de levantamiento con la institución. El paquete está listo en [`docs/07-gestion/levantamiento/`](docs/07-gestion/levantamiento/) — guion de dos horas, los 19 formatos, 12 preguntas ordenadas por impacto, y los 28 casos especiales redactados para leerle a un motorista.

**Lo que llega de afuera:** la definición de stack y arquitectura desde el proyecto coordinador. Cuando llegue, la matriz de evaluación ya existe: son los **nueve requisitos no funcionales determinantes**, en [`docs/02-requisitos/no-funcionales/`](docs/02-requisitos/no-funcionales/).
