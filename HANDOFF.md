# Estado del trabajo

**Última actualización: 2026-08-26.**

Punto único de entrada para saber en qué va el proyecto. Si algo figura acá como abierto, está abierto; si se cierra, se saca de la lista el mismo día.

## Dónde está el proyecto

**Sprint 0 cerrado. Los cinco bloques están escritos, revisados y corregidos.**

**Hay stack, y hay autorización para programar.** La [designación de LOKI del 2026-08-26](docs/07-gestion/designaciones/2026-08-26-stack-y-arranque.md) fijó el stack y el PO autorizó el arranque. Eso activa la cláusula de revisión que [`ADR-000`](docs/03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md) escribió para sí mismo: **`ADR-000` queda superado de hecho, pero todavía no de derecho** — el `ADR-002` que lo supera no está escrito.

| Bloque | Qué produjo | Estado |
|---|---|---|
| 0 — Andamiaje | `CLAUDE.md`, 11 plantillas, 10 fichas normativas, 10 subagentes | ✅ Cerrado |
| 1 — Negocio | Visión, glosario, 17 actores, 14 procesos, máquina de estados, **97 reglas** | ✅ Revisado y corregido |
| 2 — Casos especiales | **28 casos** de la operación real, con su regla de resolución | ✅ Cerrado |
| 3 — Requisitos | 18 casos de uso, **150 historias** con Gherkin, 21 no funcionales, backlog | ✅ Revisado y corregido en `3f4ced4` |
| 4 — Diseño | Modelo de datos bitemporal con 43 entidades, 126 pantallas, **41 maquetadas** | ✅ Revisado y corregido en `3f4ced4` |

**398 documentos versionados, 47,604 líneas, 22 commits.**

El stack, en una línea: **.NET 10 + EF Core sobre SQL Server 2014 Standard** (restricción institucional, fuera de soporte), **React 19 + Vite** en oficina, **React Native + SQLite cifrado** en campo. El detalle, las funciones que 2014 no tiene y con qué se reemplazan están en la designación.

## Lo que está abierto

### Los nueve documentos de arquitectura que la designación pide — ninguno escrito

Es lo inmediato y no depende de nadie externo.

| Documento | Qué fija |
|---|---|
| `ADR-002` | Adoptar el stack. **Debe decir que 2014 no se eligió, se encontró**, y llevar el riesgo de motor sin parches a deuda aceptada, firmada por el PO |
| `ADR-003` | El cliente de campo es aplicación instalada, no web |
| `ADR-004` | Fotografías y adjuntos fuera de la base |
| `ADR-005` | Los identificadores se generan en el cliente |
| `ADR-006` | Bitemporalidad desde el modelo inicial |
| `ADR-007` | Marcas de tiempo en UTC con el desfase del dispositivo |
| `ADR-008` | Los permisos se publican como capacidad, nunca como rol |
| `ADR-009` | Módulos verticales con reglas compartidas puras y guardas |
| `docs/03-arquitectura/c4/` | Contexto y contenedores |

### La decisión que el PO no ha tomado, y que va antes del primer módulo

**¿Clean Architecture con ceremonia o sin ella?** La regla dura —`Sigti.Dominio` sin referencias a EF Core ni ASP.NET, con prueba de arquitectura que falla— se adopta sin discusión. Lo que está en duda es la ceremonia: interfaz por agregado, DTO en cada frontera, caso de uso como clase. La designación la descarta y la evidencia medida la respalda; con 19 módulos y `RNF-15` hablando de rotación de personal, el argumento contrario no es ridículo. **Preguntarle al PO antes de escribir el primer módulo**, y si decide lo contrario, registrarlo en `ADR-009` con su motivo.

### Cinco preguntas de la designación que bloquean

1. **Edición exacta y Service Pack** de la instancia 2014, y si el **cifrado de respaldo** está disponible ahí. Bloquea `RNF-13`
2. **¿La licencia tiene Software Assurance vigente?** Si la tiene, la actualización ya está pagada y buena parte de la designación sobra
3. **Insumo #73, reformulado**: dónde vive la llave del cifrado por columna, quién la custodia, y **quién probó una restauración completa con ella en otra máquina**
4. **Aceptación por escrito** del riesgo de operar un motor fuera de soporte con datos personales de ciudadanos
5. **Licenciamiento para la segunda institución.** No bloquea el piloto; bloquea la promesa de `RNF-19`

### 85 insumos pendientes, de los cuales cuatro pesan

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

### Documentación exigida por LOKI — destrabada, no escrita

Auditado el 2026-08-25, cuando la causa de las cuatro brechas era la misma: no había stack. **Ya lo hay**, así que **`ARQUITECTURA.md` y `DESPLIEGUE.md` se pueden escribir**, y `README.md` y `CLAUDE.md` pueden salir de `Parcial`. Nada de eso está hecho todavía.

`CLAUDE.md` además quedó desactualizado en su sección *Estado actual*: sigue diciendo que el stack está diferido al Sprint 2.

### Los informes de hallazgos siguen diciendo «Abierto»

Los cinco archivos de [`docs/05-calidad/hallazgos/`](docs/05-calidad/hallazgos/) conservan en su encabezado el estado con que se emitieron —*«Abierto»*, *«Ninguna corrección aplicada»*, *«Pendiente de corrección»*— pese a que las correcciones ya se aplicaron. Es la misma falla que este archivo tenía: trabajo cerrado figurando como abierto. Corregir el campo **Estado** de los cinco.

## Lo que está cerrado

**No confundir con lo anterior.** Esto ya se resolvió y no debe volver a listarse como pendiente:

- **La selección de stack.** Cerrada por la designación del 2026-08-26, con autorización del PO para programar. Falta el `ADR-002` que lo formalice, no la decisión
- **Los 46 hallazgos de los Bloques 3 y 4**, incluidos los cinco críticos del Bloque 3 y las cuatro correcciones estructurales del Bloque 4 —`HB34-50` bitemporalidad, `HB34-51` alcance de la cadena de auditoría, `HB34-52` folio por dispositivo, `HB34-53` adjuntos clasificados y depurables—. Corregidos en `3f4ced4`, verificados en el texto de los artefactos. **M-01 y M-02 dejaron de estar sin cobertura**: las historias `HU-134` a `HU-150` cerraron el hueco y el total subió de 125 a 150
- **Los 48 hallazgos de la revisión del Bloque 1**, incluidos los 8 críticos. Corregidos en `fd12ba5` y commits siguientes
- **Los 19 hallazgos de los casos de uso** (`H-B3-001`), corregidos en `c8cb324`, en una sola pasada sobre los artefactos autoridad
- **La contradicción de la tarifa de peaje** — resuelta el 2026-08-24 en contra de la fuente comercial, por cinco fuentes concordantes
- **El bloqueante de exoneraciones para M-18** — cayó por otro lado: un pick-up es categoría liviana, L. 22
- **El hueco de M-17** — tenía cero historias siendo alcance confirmado; ahora tiene 15
- **La entrega de diseño** — 41 pantallas verificadas contra los requisitos, sin vocabulario prohibido, sin botones de eliminar, sin *continuar de todos modos* sobre bloqueos duros

## Cómo seguir

**Lo inmediato, y no depende de nadie externo:** escribir los ocho ADR y el C4. `ADR-002`, `ADR-005` y `ADR-006` van primero — son los tres que la designación marca como imposibles de cambiar después: la clave agrupada, las dos parejas de fechas y el nivel de compatibilidad 120.

**Lo que hay que preguntarle al PO antes del primer módulo:** Clean con ceremonia o sin ella.

**Después de los ADR:** el walking skeleton del Sprint 2 — **una orden de misión de punta a punta**, solicitud → despacho → ejecución → liquidación, con su asiento en bitácora. Un hilo delgado que toca todas las capas, no un módulo completo. En ese mismo esqueleto se mide **`RNF-12`: ≤ 25 % de batería en 8 h con seguimiento activo, en gama baja**, que es el número que puede obligar a bajar el seguimiento a un módulo nativo. Hay que saberlo en el Sprint 2, no en el 6.

**Lo que hay que gestionar en paralelo:** la sesión de levantamiento con la institución. El paquete está listo en [`docs/07-gestion/levantamiento/`](docs/07-gestion/levantamiento/) — guion de dos horas, los 19 formatos, 12 preguntas ordenadas por impacto, y los 28 casos especiales redactados para leerle a un motorista. Al mismo tiempo, confirmar en la instancia real la edición exacta de SQL Server, el Service Pack y el cifrado de respaldo.
