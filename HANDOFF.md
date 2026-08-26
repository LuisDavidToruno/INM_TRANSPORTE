# Estado del trabajo

**Última actualización: 2026-08-26.**

Punto único de entrada para saber en qué va el proyecto. Si algo figura acá como abierto, está abierto; si se cierra, se saca de la lista el mismo día.

## Dónde está el proyecto

**Sprint 0 cerrado. Los cinco bloques están escritos, revisados y corregidos.**

**Hay stack, y hay autorización para programar.** La [designación de LOKI del 2026-08-26](docs/07-gestion/designaciones/2026-08-26-stack-y-arranque.md) fijó el stack y el PO autorizó el arranque. Eso activó la cláusula de revisión que [`ADR-000`](docs/03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md) escribió para sí mismo, y [`ADR-002`](docs/03-arquitectura/adr/ADR-002-adoptar-el-stack-tecnologico.md) lo supera formalmente.

**Ya hay código, y camina.** El walking skeleton atraviesa API → Aplicación → Dominio → SQL Server → bitácora encadenada, con **16 pruebas verdes**.

| Bloque | Qué produjo | Estado |
|---|---|---|
| 0 — Andamiaje | `CLAUDE.md`, 11 plantillas, 10 fichas normativas, 10 subagentes | ✅ Cerrado |
| 1 — Negocio | Visión, glosario, 17 actores, 14 procesos, máquina de estados, **97 reglas** | ✅ Revisado y corregido |
| 2 — Casos especiales | **28 casos** de la operación real, con su regla de resolución | ✅ Cerrado |
| 3 — Requisitos | 18 casos de uso, **150 historias** con Gherkin, 21 no funcionales, backlog | ✅ Revisado y corregido en `3f4ced4` |
| 4 — Diseño | Modelo de datos bitemporal con 43 entidades, 126 pantallas, **41 maquetadas** | ✅ Revisado y corregido en `3f4ced4` |

**410 documentos y 48,723 líneas de documentación · 25 archivos y 1,552 líneas de C# · 30 commits.**

El stack, en una línea: **.NET 10 + EF Core sobre SQL Server 2014 Standard** (restricción institucional, fuera de soporte), **React 19 + Vite** en oficina, **React Native + SQLite cifrado** en campo. El detalle, las funciones que 2014 no tiene y con qué se reemplazan están en la designación.

## Lo que está abierto

### ⚠️ Smart App Control puede volver a bloquear la ejecución de .NET

En `DESKTOP-GR4SG52` (Windows 11), Smart App Control está **activo** — `VerifiedAndReputablePolicyState = 1` en `HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy`.

El 2026-08-26 bloqueó durante horas la carga de **cualquier binario .NET recién compilado** con `0x800711C7`: `dotnet test`, `dotnet run` y `dotnet ef` fallaban por igual, y solo `dotnet build` funcionaba. **Se destrabó solo**, sin cambiar ninguna configuración: SAC consulta reputación en la nube y la actualizó.

**Puede repetirse con cualquier binario nuevo.** Si vuelve a aparecer `0x800711C7`, no es el código: es SAC evaluando un ensamblado que todavía no tiene reputación. Las salidas son esperar, trabajar en otra máquina, o instalar WSL2 — **apagar SAC es irreversible** sin reinstalar Windows, así que no es la primera opción.

### Tres precondiciones de bloqueo duro que el esqueleto todavía no evalúa

El hilo camina, pero **despacha sin verificar nada del vehículo ni del motorista**. Están declaradas en [`estados/orden-de-mision.md`](docs/03-arquitectura/estados/orden-de-mision.md) y no implementadas:

- **`BD-02`** — licencia habilitante y vigente durante todo el rango. **Es la que traslada responsabilidad directa a quien autorizó**
- **`BD-03`** — documentación del vehículo vigente
- **`BD-07`** — estado y compatibilidad del vehículo

Se cierran con `M-03` (ficha del vehículo) y `M-05` (motorista y matriz de licencias), que son las dos piezas siguientes.

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

### Documentación exigida por LOKI — falta la de raíz

Auditado el 2026-08-25, cuando la causa de las cuatro brechas era la misma: no había stack. Ya lo hay, y `CLAUDE.md` ya refleja el stack en su sección *Estado actual*.

Siguen ausentes **`ARQUITECTURA.md`** y **`DESPLIEGUE.md`** en la raíz. Ahora sí se pueden escribir — y el criterio lo fija la propia designación: **índice consolidado que remite a `docs/`, nunca contenido duplicado que después diverge.** Es el patrón con que ya se resolvieron `DECISIONES.md` y `HANDOFF.md`, y LOKI lo reconoció como correcto.

`DESPLIEGUE.md` tiene una dependencia real: el procedimiento de respaldo y restauración es **de dos piezas** —base más almacén de archivos, consistentes entre sí ([`ADR-004`](docs/03-arquitectura/adr/ADR-004-adjuntos-fuera-de-la-base.md))— y `RNF-09` exige que lo ejecute personal no especialista en ≤ 2 h. Escribirlo sin la instancia real confirmada sería escribir la mitad.

### Los informes de hallazgos siguen diciendo «Abierto»

Los cinco archivos de [`docs/05-calidad/hallazgos/`](docs/05-calidad/hallazgos/) conservan en su encabezado el estado con que se emitieron —*«Abierto»*, *«Ninguna corrección aplicada»*, *«Pendiente de corrección»*— pese a que las correcciones ya se aplicaron. Es la misma falla que este archivo tenía: trabajo cerrado figurando como abierto. Corregir el campo **Estado** de los cinco.

## Lo que está cerrado

**No confundir con lo anterior.** Esto ya se resolvió y no debe volver a listarse como pendiente:

- **Los ocho ADR y el C4 que la designación pedía.** Escritos el 2026-08-26: [`ADR-002`](docs/03-arquitectura/adr/ADR-002-adoptar-el-stack-tecnologico.md) a [`ADR-009`](docs/03-arquitectura/adr/ADR-009-modulos-verticales.md), más [`c4/contexto.md`](docs/03-arquitectura/c4/contexto.md) y [`c4/contenedores.md`](docs/03-arquitectura/c4/contenedores.md). `ADR-000` quedó marcado **Reemplazada por `ADR-002`**, sin editar su texto. Índice en [`adr/README.md`](docs/03-arquitectura/adr/README.md), con las **cinco decisiones irreversibles** señaladas
- **La selección de stack.** Cerrada por la designación del 2026-08-26, con autorización del PO para programar, y formalizada en `ADR-002`
- **Los 46 hallazgos de los Bloques 3 y 4**, incluidos los cinco críticos del Bloque 3 y las cuatro correcciones estructurales del Bloque 4 —`HB34-50` bitemporalidad, `HB34-51` alcance de la cadena de auditoría, `HB34-52` folio por dispositivo, `HB34-53` adjuntos clasificados y depurables—. Corregidos en `3f4ced4`, verificados en el texto de los artefactos. **M-01 y M-02 dejaron de estar sin cobertura**: las historias `HU-134` a `HU-150` cerraron el hueco y el total subió de 125 a 150
- **Los 48 hallazgos de la revisión del Bloque 1**, incluidos los 8 críticos. Corregidos en `fd12ba5` y commits siguientes
- **Los 19 hallazgos de los casos de uso** (`H-B3-001`), corregidos en `c8cb324`, en una sola pasada sobre los artefactos autoridad
- **La contradicción de la tarifa de peaje** — resuelta el 2026-08-24 en contra de la fuente comercial, por cinco fuentes concordantes
- **El bloqueante de exoneraciones para M-18** — cayó por otro lado: un pick-up es categoría liviana, L. 22
- **El hueco de M-17** — tenía cero historias siendo alcance confirmado; ahora tiene 15
- **La entrega de diseño** — 41 pantallas verificadas contra los requisitos, sin vocabulario prohibido, sin botones de eliminar, sin *continuar de todos modos* sobre bloqueos duros

## Cómo seguir

**El walking skeleton está cerrado y verificado.** `dotnet test` → **16 de 16 verdes**, salida limpia. Necesita SQL Server en `localhost` con autenticación integrada; la suite crea y borra `SIGTI_Pruebas` sola.

Lo que el esqueleto dejó probado, y que antes solo estaba escrito en un ADR:

| Unión | Estado |
|---|---|
| La cadena de hash no se bifurca con 20 escritores concurrentes sobre la misma cola | ✅ Verificado |
| EF Core en nivel 120 emite DDL que SQL Server 2014 acepta | ⚠️ Revisado por inspección del script, **no aplicado contra una instancia 2014 real** |
| ULID como clave agrupada en `binary(16)` | ✅ Persiste y recupera |
| El expediente se reconstruye desde su diario, sin columna de estado | ✅ Verificado de punta a punta |
| `BD-01` bloquea al solicitante de derecho, y el bloqueo sobrevive el viaje por la API | ✅ Verificado |

La migración inicial está en `src/Sigti.Datos/Migraciones/` y el script idempotente en [`entrega/sql/sigti-inicial.sql`](entrega/sql/sigti-inicial.sql).

**Lo que ya está ratificado:** `ADR-009` sin ceremonia de Clean, confirmado por el PO el 2026-08-26.

**Lo que sigue: músculo sobre el esqueleto.** Las dos primeras piezas son la **ficha del vehículo** (`M-03`) y el **motorista con su matriz de licencias** (`M-05`), porque `BD-02`, `BD-03` y `BD-07` son precondiciones que el esqueleto **todavía no evalúa**: hoy despacha sin verificar licencia habilitante, documentación vigente ni compatibilidad del vehículo. Están declaradas en el documento de estados y no implementadas — y `BD-02` es la que traslada responsabilidad directa a quien autorizó.

**Lo que falta medir, y no puede esperar al Sprint 6:** **`RNF-12` — ≤ 25 % de batería en 8 h con seguimiento activo, en gama baja.** Es el único número donde React Native es medible peor que Kotlin, y [`ADR-003`](docs/03-arquitectura/adr/ADR-003-cliente-de-campo-instalado.md) tiene la contingencia escrita: bajar el seguimiento a un módulo nativo, sin reescribir la aplicación. Esa contingencia sirve si el número llega ahora.

**Lo que hay que gestionar en paralelo:** la sesión de levantamiento con la institución. El paquete está listo en [`docs/07-gestion/levantamiento/`](docs/07-gestion/levantamiento/) — guion de dos horas, los 19 formatos, 12 preguntas ordenadas por impacto, y los 28 casos especiales redactados para leerle a un motorista. Al mismo tiempo, confirmar en la instancia real la edición exacta de SQL Server, el Service Pack y el cifrado de respaldo — **sin esa instancia, el paso de CI que sostiene toda la estrategia del motor no existe.**
