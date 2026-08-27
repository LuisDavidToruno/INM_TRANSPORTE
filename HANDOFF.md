# Estado del trabajo

**Última actualización: 2026-08-26.**

Punto único de entrada para saber en qué va el proyecto. Si algo figura acá como abierto, está abierto; si se cierra, se saca de la lista el mismo día.

## Dónde está el proyecto

**Sprint 0 cerrado. Los cinco bloques están escritos, revisados y corregidos.**

**Hay stack, y hay autorización para programar.** La [designación de LOKI del 2026-08-26](docs/07-gestion/designaciones/2026-08-26-stack-y-arranque.md) fijó el stack y el PO autorizó el arranque. Eso activó la cláusula de revisión que [`ADR-000`](docs/03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md) escribió para sí mismo, y [`ADR-002`](docs/03-arquitectura/adr/ADR-002-adoptar-el-stack-tecnologico.md) lo supera formalmente.

**Ya hay código, camina, y se ve.** El backend atraviesa API → Aplicación → Dominio → SQL Server → bitácora encadenada, con **60 pruebas**. Y hay **cuatro pantallas de oficina conectadas a la API real**, no a datos de muestra.

El circuito que funciona hoy, de punta a punta y verificado contra SQL Server:

```
solicitar → autorizar → cola de programación → asignar vehículo y motorista → programar
```

Con `BD-01`, `BD-02` y `BD-03` evaluándose de verdad, la caducidad de la aprobación, y la anulación con motivo tipificado.

| Bloque | Qué produjo | Estado |
|---|---|---|
| 0 — Andamiaje | `CLAUDE.md`, 11 plantillas, 10 fichas normativas, 10 subagentes | ✅ Cerrado |
| 1 — Negocio | Visión, glosario, 17 actores, 14 procesos, máquina de estados, **97 reglas** | ✅ Revisado y corregido |
| 2 — Casos especiales | **28 casos** de la operación real, con su regla de resolución | ✅ Cerrado |
| 3 — Requisitos | 18 casos de uso, **150 historias** con Gherkin, 21 no funcionales, backlog | ✅ Revisado y corregido en `3f4ced4` |
| 4 — Diseño | Modelo de datos bitemporal con 43 entidades, 126 pantallas, **41 maquetadas** | ✅ Revisado y corregido en `3f4ced4` |

**406 documentos de análisis · 4,177 líneas de C# de producción y 1,965 de pruebas · 2,558 de TypeScript propio · 66 pruebas en verde · 58 commits.** Las de C# excluyen las migraciones generadas; las de TypeScript, el sistema de diseño de LOKI. Las reglas de negocio son **103**.

Las líneas de C# excluyen las migraciones de EF, que son generadas. El TypeScript excluye el sistema de diseño de LOKI, que se copió, no se escribió.

El stack, en una línea: **.NET 10 + EF Core sobre SQL Server 2014 Standard** (restricción institucional, fuera de soporte), **React 19 + Vite** en oficina, **React Native + SQLite cifrado** en campo. El detalle, las funciones que 2014 no tiene y con qué se reemplazan están en la designación.

### Cómo levantarlo

```bash
dotnet run --project src/Sigti.Api --urls http://localhost:5199
```

```bash
cd oficina && npm run dev
```

La oficina necesita `oficina/.env.local` con `VITE_API=http://localhost:5199`. **Sin esa variable arranca con datos de muestra y lo dice en pantalla** — no finge estar conectada.

La base de desarrollo es `SIGTI_Desarrollo` en `localhost`, creada en `COMPATIBILITY_LEVEL 120` como exige `ADR-002`. Las migraciones se aplican con `dotnet ef database update -p src/Sigti.Datos -s src/Sigti.Datos`.

## Lo que está abierto

### ⚠️ Smart App Control puede volver a bloquear la ejecución de .NET

En `DESKTOP-GR4SG52` (Windows 11), Smart App Control está **activo** — `VerifiedAndReputablePolicyState = 1` en `HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy`.

El 2026-08-26 bloqueó durante horas la carga de **cualquier binario .NET recién compilado** con `0x800711C7`: `dotnet test`, `dotnet run` y `dotnet ef` fallaban por igual, y solo `dotnet build` funcionaba. **Se destrabó solo**, sin cambiar ninguna configuración: SAC consulta reputación en la nube y la actualizó.

**Puede repetirse con cualquier binario nuevo.** Si vuelve a aparecer `0x800711C7`, no es el código: es SAC evaluando un ensamblado que todavía no tiene reputación. Las salidas son esperar, trabajar en otra máquina, o instalar WSL2 — **apagar SAC es irreversible** sin reinstalar Windows, así que no es la primera opción.

**Lo que se aprendió el 2026-08-27, y acota las salidas.** El bloqueo puede durar **toda una sesión**: más de ochenta intentos, limpiando `bin`/`obj`, en Debug y en Release, y con la salida fuera del repositorio. **La ruta no importa** — SAC decide por reputación del binario, y cada compilación produce un hash nuevo.

Y no afecta solo a las pruebas: `dotnet run` de la API falla igual, con `Sigti.Datos.dll`. **Compilar sí funciona; lo que se bloquea es cargar el ensamblado.** O sea que con SAC en este estado se puede escribir y compilar, pero no ejecutar nada del proyecto — ni pruebas, ni API, ni verificación en pantalla.

SAC **no tiene lista de exclusiones**: es activo / evaluación / apagado, y de apagado no se vuelve. Con eso, las salidas reales son dos: **correrlo en la otra máquina**, o que el PO decida apagar SAC sabiendo que es de un solo sentido.

### `BD-12` cerrado en las tres capas, y verificado

Las restricciones médicas salieron de `BD-02` a un bloqueo propio, con el efecto decidido por el catálogo. **Suite en 66/66 el 2026-08-27**, incluidas las tres pruebas de `CatalogoProvisionalDeRestricciones` que en su momento no pudieron correr.

| Capa | Qué quedó |
|---|---|
| [`orden-de-mision.md`](docs/03-arquitectura/estados/orden-de-mision.md) | `BD-12` definido y listado en `T-08`, `T-12` y `T-17`; `BD-02` con sus dos condiciones |
| [`ReglasDeHabilitacion`](src/Sigti.Dominio/M05_Motoristas/ReglasDeHabilitacion.cs) | Sin la condición 3, sin el parámetro, sin el valor del enum |
| [`ReglasDeRestriccionMedica`](src/Sigti.Dominio/M05_Motoristas/ReglasDeRestriccionMedica.cs) | La evaluación nueva, con 3 pruebas |
| [`CatalogoProvisionalDeRestricciones`](src/Sigti.Aplicacion/M05_Motoristas/CatalogoProvisionalDeRestricciones.cs) | ⚠️ **Provisional** — insumo #42. Con 3 pruebas |
| [`EvaluacionDeAsignacion`](src/Sigti.Aplicacion/M07_ProgramacionYDespacho/EvaluacionDeAsignacion.cs) | `BD-12` aparte; **solo el bloqueo impide programar** |
| [`Asignacion.tsx`](oficina/src/modulos/M07_Programacion/Asignacion.tsx) | La advertencia con **acuse obligatorio**: el botón queda inerte hasta marcarlo (`RN-11`) |
| [`RechazoPorLicencia.tsx`](oficina/src/modulos/M07_Programacion/RechazoPorLicencia.tsx) | Distingue el bloqueo de `BD-12` del de `BD-02` |

**Lo único que falta es verlo en pantalla.** La advertencia y el acuse pasan las pruebas del dominio y compilan, pero **nadie los ha abierto en el navegador** — cuando SAC lo permitió, la API no llegó a levantarse. Antes de darlo por bueno de cara al usuario: levantar la API, elegir un conductor con restricción sin clasificar, y comprobar que la advertencia aparece y que el botón no se habilita sin marcar el acuse.

**Y una decisión que conviene mirar.** El catálogo provisional tipifica **una sola** restricción como bloqueante — *«conducción diurna únicamente»*, la única que `RN-11` sostiene contrastable por sistema. Todo lo demás advierte. Mientras el insumo #42 siga abierto ése va a ser el caso mayoritario: **hoy el sistema casi no bloquea por restricción médica**. Si la institución espera lo contrario, el arreglo no es tocar código — es conseguir el catálogo de la DNVT.

### `BD-07` sigue sin evaluarse, y `BD-04` a `BD-11` tampoco

`BD-02` y `BD-03` ya están implementadas y probadas — **`BD-12` también, ver arriba.** **`BD-07` no** — estado y compatibilidad del vehículo. Necesita dos cosas que todavía no existen:

- La **matriz de compatibilidad** entre el objeto del traslado y el tipo de vehículo (`M-02`)
- La **categoría de peaje** resuelta por vehículo (`M-18`, `NRM-10`) — sin ella el estimado de peajes no es verificable, y quien autoriza no puede comprobar el cálculo

Tampoco están `BD-04` (día u hora inhábil), `BD-05` (coherencia del odómetro), `BD-06` (segregación operativa), `BD-08` a `BD-11`. Todas declaradas en [`estados/orden-de-mision.md`](docs/03-arquitectura/estados/orden-de-mision.md) §4.

### La matriz licencia↔vehículo **está completa y es normativa**

Las **nueve** categorías del **Artículo 4 del Acuerdo 1012-2021** `[V]`, con la fuente en [`fuentes/`](docs/01-negocio/normativa/fuentes/acuerdo-1012-2021-permisos-de-conducir.pdf). Versión `ACUERDO-1012-2021-ART-4`.

`A` y `B1` se expresan por **clase normativa** —`MOTOCICLETA`, `TRICICLO_CUADRICICLO`, `AUTOMOVIL`, `CAMION`, `AUTOBUS`—, que es conjunto cerrado de la norma y **no es el tipo de vehículo del catálogo institucional**. Donde el Acuerdo no fija techo de masa o pasajeros, la entrada tampoco lo fija: el límite real lo pone la ficha técnica.

**Lo que queda abierto es el camino, no el dato:** el circuito de carga existe (`POST /parametros` y `POST /parametros/{id}/aprobar`, con doble control y asiento en bitácora), pero la matriz **no entra por él** — está escrita en C# en `ParametrosProvisionales`. Cargarla por el circuito y borrar esa clase es lo que le da doble control.

### Tres catálogos viven en código, y están marcados como provisionales

Ninguno finge ser otra cosa: los tres llevan su aviso en el propio archivo.

| Qué | Dónde | Qué falta para borrarlo |
|---|---|---|
| **Flota y padrón de motoristas** | `Sigti.Aplicacion/M03_Flota/CatalogoProvisionalDeFlota.cs` | `M-03` y `M-05` con sus repositorios. La firma que ve el cliente no cambia |
| **Documentación del vehículo** | Devuelta **conforme por omisión** en `EvaluacionDeAsignacion` y en el endpoint de programar | `M-04`. Hoy `BD-03` no puede bloquear por un vencimiento que el sistema no conoce, y eso queda dicho en el código en vez de fingir que se verificó |
| **El folio** | `ConsultaDeMisiones.FolioProvisional` — sale como `PROV-xxxxxx` | El circuito de rangos por delegación de `RNF-21`. Lleva prefijo para que nadie lo cite en un descargo |

### Las validaciones de `HU-009` no las calcula el servidor

La bandeja de autorización muestra un aviso que lo dice: la antigüedad del espejo de ARGOS y las misiones sin liquidar del solicitante necesitan `M-01` y `M-13`, que no existen.

**Una bandeja sin reparos y una bandeja que no sabe si los hay son cosas distintas**, y por eso el adaptador lo declara en lugar de devolver una lista vacía.

### Cinco preguntas de la designación que bloquean

1. **Edición exacta y Service Pack** de la instancia 2014, y si el **cifrado de respaldo** está disponible ahí. Bloquea `RNF-13`
2. **¿La licencia tiene Software Assurance vigente?** Si la tiene, la actualización ya está pagada y buena parte de la designación sobra
3. **Insumo #73, reformulado**: dónde vive la llave del cifrado por columna, quién la custodia, y **quién probó una restauración completa con ella en otra máquina**
4. **Aceptación por escrito** del riesgo de operar un motor fuera de soporte con datos personales de ciudadanos
5. **Licenciamiento para la segunda institución.** No bloquea el piloto; bloquea la promesa de `RNF-19`

### 83 insumos pendientes, de los cuales cuatro pesan

De 85 filas en las secciones abiertas del registro, dos ya están marcadas cerradas: el **#23** —los cuatro PDF, descargados— y el **#79** —`BE` existe, `DE` no—.

Registro completo en [`docs/07-gestion/insumos-pendientes.md`](docs/07-gestion/insumos-pendientes.md).

| # | Qué | A quién |
|---|---|---|
| **2** | **Formatos en papel vigentes** — 19 documentos. Bloquea 27 pantallas de captura | Institución. Lista lista en [`levantamiento/`](docs/07-gestion/levantamiento/) |
| **7** | **Periodicidad del fondo de combustible** — define si el objeto es de período o de misión. Es estructural | Gerencia Administrativa |
| **26** | Pronunciamiento de **Auditoría Interna** sobre segregación en delegaciones. **Es el que más tarda** | Auditoría Interna |
| **36** | ¿Hay cisterna o bidones de combustible? **Cambia el circuito completo de M-09** | Encargado de transporte |

**Ese lote ya se cerró:** los cuatro PDF oficiales están descargados en [`docs/01-negocio/normativa/fuentes/`](docs/01-negocio/normativa/fuentes/) desde el 2026-08-26, y la matriz licencia↔vehículo quedó `[V]`. **Queda sin explotar el Decreto 51-2025 y el clasificador de SEFIN**, que están bajados pero no leídos.

### Decisiones pendientes del PO

1. **Ratificar o revertir [`DP-002`](docs/07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)** — segregación en delegaciones pequeñas
2. **El nombre de la institución en los mockups** — dicen *Instituto Nacional de Migración*; según `DP-001` el sistema es genérico y eso es configuración
3. **La contradicción del salvoconducto** — la paridad con el papel exige reproducir el formato; el requisito de campo exige que cuatro datos vayan arriba y en grande. Si el formato no los pone ahí, hay que decidir cuál gana
4. **¿El reclamo de peaje cierra la misión o la marca con hallazgo?** — resuelto provisionalmente a favor de lo primero, y anotado `[C]` en la máquina de estados por si se revierte (`HB3-02`)
5. **¿Quién aprueba el descargo de un bien — `ACT-08` o también `ACT-09`?** El mapa de procesos admite los dos, la máquina de estados dice uno, y `NRM-02` está `[P]`: la norma no lo zanja, así que lo zanja el PO (`HB3-16`)
6. **Sesión de refinamiento** sobre las historias en borrador. Los cuatro analistas aplicaron el criterio de forma distinta; la mayoría pasaría sin tocarse

### Documentación exigida por LOKI — falta la de raíz

Auditado el 2026-08-25, cuando la causa de las cuatro brechas era la misma: no había stack. Ya lo hay, y `CLAUDE.md` ya refleja el stack en su sección *Estado actual*.

Siguen ausentes **`ARQUITECTURA.md`** y **`DESPLIEGUE.md`** en la raíz. Ahora sí se pueden escribir — y el criterio lo fija la propia designación: **índice consolidado que remite a `docs/`, nunca contenido duplicado que después diverge.** Es el patrón con que ya se resolvieron `DECISIONES.md` y `HANDOFF.md`, y LOKI lo reconoció como correcto.

`DESPLIEGUE.md` tiene una dependencia real: el procedimiento de respaldo y restauración es **de dos piezas** —base más almacén de archivos, consistentes entre sí ([`ADR-004`](docs/03-arquitectura/adr/ADR-004-adjuntos-fuera-de-la-base.md))— y `RNF-09` exige que lo ejecute personal no especialista en ≤ 2 h. Escribirlo sin la instancia real confirmada sería escribir la mitad.

### Los 113 hallazgos, cerrados

Los cinco informes de [`docs/05-calidad/hallazgos/`](docs/05-calidad/hallazgos/) conservaban el estado con que se emitieron —*«Abierto»*, *«Ninguna corrección aplicada»*, *«Pendiente de corrección»*—. Al verificarlos contra los artefactos vivos el 2026-08-26, **hallazgo por hallazgo y no contra el mensaje del commit**, apareció que el campo estaba mal **y también la suposición de que todo se había corregido**: veinte seguían abiertos. Se cerraron entre el 26 y el 27 de agosto.

| Informe | Corregidos | **Abiertos** |
|---|---|---|
| [`H-B1-001`](docs/05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md) QA del Bloque 1 | **28** | — |
| [`H-B1-002`](docs/05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) normativa del Bloque 1 | **20** | — |
| [`H-B3-001`](docs/05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md) casos de uso | **19** | — |
| [`H-B34-001`](docs/05-calidad/hallazgos/H-B34-001-revision-qa-bloque-3.md) QA del Bloque 3 | 21 citados, verificados por muestreo | — |
| [`H-B34-002`](docs/05-calidad/hallazgos/H-B34-002-revision-arquitectura-bloque-4.md) arquitectura del Bloque 4 | 25 citados, verificados por muestreo | — |
Cada informe lleva ahora su desglose. **Los tres que se señalaron como prioritarios fueron los primeros en cerrarse:**
Cada informe lleva ahora su desglose. **Los tres que se señalaron como prioritarios están los tres cerrados:**

1. ✅ **`HB1-01` quedó cerrado el 2026-08-26.** Faltaba el Nivel 3 de [`actores-y-roles`](docs/01-negocio/actores-y-roles.md) §5.4, que seguía diciendo *«el sistema no lo bloquea»* ante una emergencia. Lo zanjó el principio **P-2** de la máquina de estados, que separa lo que ese nivel mezclaba: los bloqueos duros rigen `T-05`, `T-08` y `T-12` —también en emergencia— y **nunca impiden registrar el hecho**. La salida sin red es el **código de autorización fuera de línea** del `§6.6`, que la propia autoridad nombra como la respuesta a la segregación en delegaciones pequeñas. **Queda declarado un hueco `[C]`**: si a las 03:15 la única persona disponible es el propio motorista, `I-11` no se levanta y ese caso no tiene salida escrita — es decisión del PO, no de diseño.
2. ✅ **`HN1-18` y `HN1-09` quedaron cerrados el 2026-08-26.** `HN1-18` eran **ocho** materias, no una. Cuatro pasaron a regla —[`RN-99`](docs/01-negocio/reglas/RN-99-constatacion-fisica-de-la-flota.md) constatación física de la flota, [`RN-100`](docs/01-negocio/reglas/RN-100-permisos-por-puesto-no-por-persona.md) permisos por puesto, [`RN-101`](docs/01-negocio/reglas/RN-101-cierre-de-asignacion-de-puesto.md) cierre de asignación con custodias, [`RN-102`](docs/01-negocio/reglas/RN-102-reporte-publico-de-flota.md) reporte público—, una entró en [`RN-43`](docs/01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), y **tres no eran huecos**: multas y siniestros son M-12, mantenimiento es M-11, y el TAG lo bloquea el insumo #24. El [`README` de reglas](docs/01-negocio/reglas/README.md) los declara ahora, que era lo que el hallazgo pedía de raíz. `HN1-09` se cierra con [`RN-98`](docs/01-negocio/reglas/RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md): la evidencia se entrega también por vehículo y por período, no solo por misión.
3. ✅ **`HN1-14` quedó cerrado el 2026-08-26.** [`RN-52`](docs/01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) declaraba `[V]` una exigencia del MARCI que `NRM-01` tiene `[C]` — **y su cabecera contradecía a su propio cuerpo**, que ya decía lo correcto. La verificación quedó separada en sus tres afirmaciones: `[V]` el hábeas data del Artículo 182, `[C]` la exigencia del MARCI, `[I]` que del hábeas data se siga registrar cada consulta. **Sigue siendo bloqueo duro y no configurable**, dicho expresamente en la regla. Se alinearon los otros cuatro artefactos que repetían la escalada.

**No queda ninguno abierto.** Los tres informes del Bloque 1 y de casos de uso están cerrados hallazgo por hallazgo; los dos de los Bloques 3 y 4 llevan sus 46 citados en el artefacto que corrigen, verificados por muestreo y **dicho así en su encabezado**, no como auditoría completa.

**Lo que el lote deja vivo no son hallazgos, sino trabajo que ellos destaparon:**

| Qué | Dónde queda |
|---|---|
| **Cinco pares de incompatibilidad sin regla** — `I-12`, `I-14`, `I-15`, `I-16`, `I-17` | Declarados en [`RN-01`](docs/01-negocio/reglas/RN-01-segregacion-de-funciones.md). `I-16` es postergación de `M-11`; los otros cuatro son huecos. **`I-12` es el que más pesa**: que un auditor con capacidad de ejecutar deja de ser auditor hoy solo lo dice un documento de actores |
| **Cuatro pendientes de `actores-y-roles` §9 sin número de insumo** — `G`, `I`, `J`, `K` | Dicho como tal en esa misma sección. `G` sostiene un bloqueo configurable de [`RN-25`](docs/01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) |
| **Insumo #93 — quién aprueba el descargo** | Registrado el 2026-08-27. `NRM-02` está `[P]` y no lo zanja: **es decisión del PO** |
| **El plazo de depuración del dato personal de un borrador descartado** | `[C]` abierto en [`RN-04`](docs/01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) tras `HB1-24` |

**No confundir con lo anterior.** Esto ya se resolvió y no debe volver a listarse como pendiente.

### Construido y verificado contra SQL Server

| Qué | Dónde |
|---|---|
| **El expediente de misión**, con el estado como proyección del diario (`P-1`). No hay columna de estado que se pueda desincronizar | `M07_ProgramacionYDespacho/OrdenDeMision.cs` |
| **`BD-01`** — segregación entre solicitante y autorizador, en el caso que el control no cubría antes de `HB3-01`: la asistente captura para su jefe | idem |
| **`BD-02`** — licencia habilitante y vigente **durante todo el rango**, holgura incluida, resuelta por clase normativa y atributos de la ficha | `M05_Motoristas/ReglasDeHabilitacion.cs` |
| **`BD-03`** — matrícula bloquea; la placa **no**; póliza y revisión configurables y apagadas | `M03_Flota/ReglasDeDocumentacion.cs` |
| **Caducidad de la aprobación** — si no se programa antes del inicio de la ventana, caduca | `OrdenDeMision.ExigirAprobacionVigente` |
| **`T-09`** — anulación con motivo **tipificado**; el comentario acompaña, no sustituye | `M07_ProgramacionYDespacho/MotivoDeAnulacion.cs` |
| **La bitácora encadenada**, serializada con `sp_getapplock` dentro de la transacción. **20 escritores concurrentes no bifurcan la cadena** | `Sigti.Datos/Bitacora/EscritorDeBitacora.cs` |
| **`M-02` bitemporal** — resolución a la fecha del hecho, bloqueo sin vigencia, doble control que registra también los intentos rechazados | `M02_Parametros/` |
| **`ReglasDeVigencia`** — los dos ejes en un solo lugar, para que ningún módulo implemente uno y suponga que el otro viene puesto | `Sigti.Dominio/Reglas/` |

### Cuatro pantallas de oficina, contra la API real

| Pantalla | Qué resuelve |
|---|---|
| **`PT-013`** Bandeja de autorización | Las validaciones se ven **en la lista**, no al abrir. Advertencia y bloqueo no comparten forma, no solo color |
| **`PT-014`** Expediente en decisión | En una sola pantalla, en el orden de la decisión. **No retira el botón de autorizar** con advertencias: `RN-50` lo prohíbe |
| **`PT-025`** Cola de programación | Caducidad visible antes de intentar, y depuración con motivo del catálogo |
| **`PT-026`/`PT-027`/`PT-028`** Asignación y rechazo | La evaluación corre **al elegir**. El rechazo nombra la categoría que se necesita y ofrece las salidas en la misma pantalla |

### Decisiones que quedaron hechas estructura, no comprobación

- **La ventana salió de `AsignacionDeMision`.** El agregado usa `Solicitud.Ventana` y el compilador impide pasar otra: quien programa no puede acortarla para que una licencia alcance
- **La API recibe identificadores de catálogo**, no la ficha técnica. Declarar 2,800 kg de un camión de 12,000 ya no se puede expresar
- **El respaldo documental de `M-02` es `required`.** El escenario de `HU-145` que rechaza una carga sin respaldo dejó de necesitar comprobación: ese estado no se construye
- **La evaluación de `BD-02` vive en un solo lugar.** El cliente pide el resultado; no lo calcula

### Del Sprint 0

- **Los ocho ADR y el C4** que la designación pedía. `ADR-000` marcado *Reemplazada por `ADR-002`*, sin editar su texto
- **Los 46 hallazgos de los Bloques 3 y 4**, y los 48 del Bloque 1, y los 19 de casos de uso
- **Los cuatro PDF oficiales** del insumo #23, descargados y versionados en [`fuentes/`](docs/01-negocio/normativa/fuentes/)
- **El insumo #79** — `BE` existe, `DE` no. Son nueve categorías, verificado en el Artículo 4

## Cómo seguir

**Lo inmediato, y no depende de nadie externo:**

1. **`PT-031` — constancia probatoria de las verificaciones practicadas.** Toda la evidencia ya está en el diario; falta el documento que el auditor pide. Es lo que convierte lo construido en algo defendible
2. **Aplicar el script contra una instancia SQL Server 2014 real.** Sigue siendo el único control que atrapa una migración que el destino rechaza, y **no existe**. Lo revisado hasta hoy es inspección del script, no aplicación
3. **Cargar la matriz de licencias por el circuito de `M-02`** y borrar `ParametrosProvisionales`
4. **`M-03` y `M-04`** — ficha del vehículo y su documentación. Destraban `BD-03` de verdad y `BD-07` a medias

**Lo que falta medir, y no puede esperar al Sprint 6:** **`RNF-12` — ≤ 25 % de batería en 8 h con seguimiento activo, en gama baja.** Es el único número donde React Native es medible peor que Kotlin, y [`ADR-003`](docs/03-arquitectura/adr/ADR-003-cliente-de-campo-instalado.md) tiene la contingencia escrita. Esa contingencia sirve si el número llega ahora. **No hay ni una línea del cliente de campo todavía.**

**Lo que hay que gestionar en paralelo:** la sesión de levantamiento con la institución. El paquete está listo en [`docs/07-gestion/levantamiento/`](docs/07-gestion/levantamiento/) — guion de dos horas, los 19 formatos, 12 preguntas ordenadas por impacto, y los 28 casos especiales redactados para leerle a un motorista. Al mismo tiempo, confirmar en la instancia real la edición exacta de SQL Server, el Service Pack y el cifrado de respaldo.
