# SIGTI — Sistema de Gestión de Transporte Institucional

Contexto permanente del proyecto. Léelo antes de producir cualquier artefacto.

## Qué es

Sistema **genérico de gestión de transporte para instituciones públicas hondureñas**. Se despliega **on-premise** en los servidores internos de cada institución: una instancia por institución, con múltiples dependencias y delegaciones dentro de ella.

El repositorio se llama `INM_TRANSPORTE` por su origen, pero **el producto no es específico del Instituto Nacional de Migración**. Nunca escribas reglas, catálogos ni textos atados a una sola institución: todo lo institucional-específico va en catálogos configurables.

## Premisas rectoras

1. **El sistema no gestiona "viajes de personas". Gestiona movilizaciones de recursos institucionales.** Lo trasladado puede ser personal de la institución, personas externas, carga (equipos, herramientas, insumos, materiales) o una combinación. La unidad de control administrativo-contable es la **Orden de Misión**.

2. **El tipo de vehículo es el eje de compatibilidad** entre lo que se necesita mover y la flota disponible. Toda asignación se resuelve contra esa compatibilidad.

3. **Todo registro puede ser requerido por el Tribunal Superior de Cuentas o por Auditoría Interna.** La trazabilidad inmutable prevalece sobre la comodidad del usuario en los puntos críticos: kilometraje, combustible, viáticos y custodia del vehículo. Nada se borra físicamente; toda anulación es un asiento reverso con motivo y autor.

4. **Híbrido digital-papel por diseño, no por parche.** El control en carretera es físico. Todo documento tiene versión imprimible con folio, QR de verificación, espacio de firma y sello, y hash del documento electrónico.

5. **Offline-first, no "con soporte offline".** Más de 2 millones de personas del área rural hondureña no tienen acceso a internet (INE, EPHPM julio 2025). El cliente de campo debe registrar salida, bitácora, consumo, incidentes y fotos sin ninguna conectividad.

6. **Nada normativo se cablea.** Tarifas de peaje, categorías por número de ejes, feriados, horario hábil, plazos y matriz licencia↔vehículo son **parámetros con vigencia por rango de fechas**. Todo cálculo usa la tabla vigente **a la fecha del hecho**, no a la fecha de captura.

7. **No replicamos lo que otro sistema ya hace.** ARGOS posee viáticos, estructura presupuestaria, niveles de autorización y el componente de mapas. Talento Humano posee el expediente del empleado, permisos, vacaciones y feriados. SIGTI se integra con ellos — no los reimplementa. Ver [DP-001](docs/07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

## La frase que define el producto

> Así como Talento Humano cuida de todo lo referente a los empleados, **SIGTI cuida de todo lo referente a los vehículos** — motos, buses, pickups, camiones.

El expediente del vehículo es una entidad de primera clase con ciclo de vida completo, no un catálogo: documentación y vencimientos, seguro, revisión, mantenimiento, fallas, incidentes, especificaciones técnicas, custodios y asignaciones.

## Estado actual

**Sprint 0 cerrado. Hay stack y hay autorización para programar** — designación de LOKI del 2026-08-26, fijada en [`ADR-002`](docs/03-arquitectura/adr/ADR-002-adoptar-el-stack-tecnologico.md). Todavía no hay código.

| Capa | Qué |
|---|---|
| Campo | React Native + SQLite cifrado (SQLCipher) |
| Oficina | React 19 + Vite + TypeScript + Tailwind |
| Backend | .NET 10 + ASP.NET Core + EF Core, `UseCompatibilityLevel(120)` |
| Base | **SQL Server 2014 Standard** — restricción institucional dada, no elección. Fuera de soporte desde el 2024-07-09 |

Las diez decisiones de arquitectura están en [`docs/03-arquitectura/adr/`](docs/03-arquitectura/adr/README.md). **Las cinco irreversibles** —clave agrupada ULID, nivel de compatibilidad 120 en EF Core y en la base de desarrollo, las dos parejas de fechas de la bitemporalidad, y rangos de folio por delegación— están marcadas ahí. No las contradigas sin escribir el ADR que las supere.

`ADR-000` (diferir el stack) queda **reemplazado**. No lo edites: se marca y se supera, nunca se reescribe.

## Módulos

| # | Módulo | Responsabilidad |
|---|---|---|
| M-01 | Organización y Seguridad | Institución, dependencias, delegaciones, unidades, usuarios, roles, alcance de datos |
| M-02 | Catálogos Maestros | Tipos de vehículo y de carga, categorías de licencia, motivos de viaje, zonas, tarifas, estaciones |
| M-03 | Flota Vehicular | Ficha del vehículo, ficha técnica, régimen de tenencia, asignación, estado operativo |
| M-04 | Documentación y Cumplimiento Vehicular | Vencimientos de matrícula, seguro, revisión, permisos, salvoconductos |
| M-05 | Motoristas y Habilitación | Padrón, licencias y categorías, restricciones médicas, capacitaciones, disponibilidad |
| M-06 | Solicitudes de Transporte | Captura y encamina la necesidad de movilización y el objeto del traslado |
| M-07 | Programación y Despacho | Asignación vehículo↔motorista, conflictos, consolidaciones, emisión de Orden de Misión |
| M-08 | Ejecución y Bitácora | Salida, kilometrajes, paradas, arribos, eventos en ruta, entregas, retorno |
| M-09 | Combustible | Fondo aprobado por Administración, asignación a misiones, consumo, conciliación galonaje–kilometraje |
| ~~M-10~~ | ~~Viáticos y Gastos de Viaje~~ | **Retirado.** Lo maneja ARGOS — ver [DP-001](docs/07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| M-11 | Mantenimiento y Taller | Preventivo, correctivo, llantas, repuestos, órdenes de trabajo, indisponibilidad |
| M-12 | Incidentes, Siniestros y Sanciones | Averías, accidentes, robos, multas, uso indebido, investigaciones |
| M-13 | Liquidación y Cierre | Resultado económico y operativo de la misión, desviaciones, cierre del expediente |
| M-14 | Reportes, Indicadores y Auditoría | Reportes operativos, de control interno y de auditoría; bitácora inmutable |
| M-15 | Formatos Oficiales e Impresión | Documentos físicos con folio y QR verificable |
| M-16 | Sincronización y Operación Desconectada | Captura sin conectividad y reconciliación al reconectar |
| M-17 | Traslado de Personas Externas | Manifiestos, cadena de custodia, minimización de datos, registro de consultas |
| M-18 | Peajes | Puntos de peaje del país, tarifas con vigencia, clasificación por número de ejes, estimación y conciliación |
| M-19 | Seguimiento en Ruta | Ubicación y estado de cada vehículo en tiempo real, multi-destino, tiempos de espera en sitio, actualización por el motorista |
| M-20 | Integraciones | ARGOS (viáticos, presupuesto, autorizaciones, mapas), Talento Humano (expedientes, permisos, feriados), Almacén (diferido) |

**Los IDs no se reciclan.** M-10 queda retirado, no reasignado.

## Ciclo de vida de la Orden de Misión

```
BORRADOR → SOLICITADA → APROBADA → PROGRAMADA → DESPACHADA → EN_RUTA → RETORNADA → LIQUIDADA → CERRADA
```

Ramas: `RECHAZADA`, `ANULADA`, `CERRADA_CON_HALLAZGO`. Cada transición registra actor, rol, marca de tiempo y motivo.

## Restricciones normativas que condicionan el diseño

Las fichas completas están en [docs/01-negocio/normativa/](docs/01-negocio/normativa/). Lo que nunca debes olvidar al diseñar:

- **Segregación de funciones (MARCI/TSC)**: quien solicita ≠ quien autoriza ≠ quien despacha ≠ quien entrega combustible ≠ quien liquida. Es bloqueo duro, no advertencia.
- **Matriz licencia ↔ vehículo** (categorías A, B, B1, C1, C, D1, D, CE). Asignar un motorista sin licencia habilitante o vencida traslada responsabilidad directa a quien autorizó. Bloqueo duro.
- **Días y horas inhábiles**: circular requiere permiso firmado por la máxima autoridad. Genera salvoconducto impreso.
- **Sin placa metálica es un estado válido** — hay desabastecimiento nacional. Un campo `placa` obligatorio y único rompe el sistema.
- **Seguro y revisión mecánica no son obligatorios por ley vigente**: rastreables y alertables, pero el bloqueo es **regla configurable**.
- **Identificación del vehículo del Estado** (franjas azul–blanco–azul, leyenda, siglas, correlativo) es campo verificable con fecha y foto: es hallazgo frecuente de auditoría.
- **Peajes**: el país tiene puntos de peaje con tarifas que clasifican por **número de ejes**. Cada vehículo de la flota debe tener su categoría de peaje resuelta. Ver `NRM-10`.
- **Sin firma electrónica certificada.** La autorización es interna: usuario autenticado o código gestionado por el sistema, con registro completo de quién, cuándo, desde dónde y sobre qué contenido.
- **Datos personales**: se conserva el control de acceso por rol y el registro de consultas — exigido por el MARCI. **No** se diseña para anticipar la ley de datos personales pendiente en el Congreso.

## Convenciones

### Identificadores

Estables, nunca se reciclan. Si un artefacto se descarta, su ID queda marcado como obsoleto pero no se reutiliza.

| Prefijo | Artefacto | Ubicación |
|---|---|---|
| `RN-xx` | Regla de negocio | `docs/01-negocio/reglas/` |
| `CE-xx` | Caso especial / excepción | `docs/02-requisitos/casos-especiales/` |
| `CU-xx` | Caso de uso | `docs/02-requisitos/casos-de-uso/` |
| `HU-xxx` | Historia de usuario | `docs/02-requisitos/historias/` |
| `RNF-xx` | Requisito no funcional | `docs/02-requisitos/no-funcionales/` |
| `ADR-xxx` | Decisión de arquitectura | `docs/03-arquitectura/adr/` |
| `NRM-xx` | Ficha de normativa | `docs/01-negocio/normativa/` |
| `M-xx` | Módulo funcional | este documento |
| `ACT-xx` | Actor / rol | `docs/01-negocio/actores-y-roles.md` |

### Precedencia entre artefactos

Incorporada tras los hallazgos `HB1-01` a `HB1-05`: cuatro especialidades escribiendo en paralelo produjeron cuatro respuestas distintas a la misma pregunta, y nada decía cuál mandaba.

**Cuando dos artefactos se contradicen, manda el que es autoridad sobre esa materia:**

| Materia | Autoridad |
|---|---|
| Transiciones de estado, precondiciones, invariantes, bloqueos duros | `docs/03-arquitectura/estados/` |
| Actores, incompatibilidades, matriz de permisos, alcance de datos | `docs/01-negocio/actores-y-roles.md` |
| Todo lo demás del negocio | La regla `RN-xx` correspondiente |
| Fronteras entre sistemas | El `ADR-xxx` vigente |
| Alcance del producto | El `DP-xxx` vigente |

**Las tablas derivadas citan su origen en lugar de reescribirlo.** Si un proceso necesita mostrar quién puede ejecutar una transición, enlaza a la tabla de transiciones — no la copia. Una tabla copiada es una tabla que va a divergir.

Cuando encuentres una contradicción: **no la resuelvas en silencio en el artefacto que estés tocando.** Corrige el que no es autoridad, y si la autoridad es la que está mal, dilo como hallazgo.

### Trazabilidad obligatoria

- Toda **historia de usuario** referencia al menos una regla de negocio y el módulo al que pertenece.
- Toda **regla de negocio** derivada de norma cita su ficha `NRM-xx` y su nivel de verificación.
- Todo **caso especial** tiene una regla de resolución explícita. Ninguno queda abierto.
- Todo **ADR** enlaza los requisitos no funcionales que lo motivan.

### Nivel de verificación normativa

Marca siempre la afirmación normativa con su nivel. **Nunca inventes números de ley, artículos, tarifas ni códigos presupuestarios.**

- `[V]` Verificado con fuente oficial o fuentes concordantes
- `[P]` Parcialmente verificado — la norma existe, no se pudo extraer el articulado
- `[C]` Por confirmar con la institución
- `[I]` Inferencia o práctica común, no norma

#### El nivel nunca sube al bajar de nivel de abstracción

Incorporado tras el hallazgo `HN1-03`. **Ningún artefacto puede declarar un nivel de verificación superior al de la ficha `NRM-xx` que cita.** Si la ficha dice `[C]`, la regla dice `[C]`; si la ficha dice `[I]`, la historia dice `[I]`.

El patrón que hay que evitar es la escalada silenciosa: la ficha marca `[C]`, el documento de análisis lo repite como `[P]`, la regla lo declara `[V]`, y el código lo implementa como obligación legal. Nadie mintió en ningún paso, y el resultado es falso.

**Tampoco se cita como `[V]` una implicación de requerimiento** escrita por el propio equipo. Que la ficha diga "el sistema debe X" no verifica que la norma exija X: verifica que nosotros lo dedujimos. Eso es `[I]`, salvo que la norma lo diga literalmente.

### Diagramas

**Mermaid dentro de los `.md`.** Versionable, revisable en diff, renderiza en GitHub. Nada de imágenes binarias ni herramientas externas.

- Procesos de negocio → `flowchart` con carriles por actor
- Ciclos de vida → `stateDiagram-v2`
- Modelo de datos → `erDiagram`
- Arquitectura → `C4Context` / `C4Container`
- Interacciones → `sequenceDiagram`

### Idioma y vocabulario

**Todo en español**, con el vocabulario real del dominio hondureño. Usa: orden de misión, vale de combustible, bitácora, viático, dependencia, jefatura inmediata, Gerencia Administrativa, motorista, salvoconducto, descargo, requisición, unidad ejecutora, objeto del gasto.

No uses: "driver", "trip", "request" ni traducciones literales del inglés. El personal de la institución debe reconocer los términos de sus formatos en papel.

Nombres de archivo en kebab-case sin tildes ni ñ. Contenido con tildes correctas.

## El equipo

El usuario es **ingeniero en sistemas** y actúa como Product Owner y Scrum Master. Los subagentes en [.claude/agents/](.claude/agents/) cubren las especialidades del equipo de desarrollo. Invócalos con la herramienta Agent cuando la tarea corresponda a su especialidad.

Con un solo humano, la velocity, los story points, el burndown y el daily standup son ceremonia sin valor: **se omiten**. Lo que sí aporta valor es el aislamiento de contexto por especialidad y la revisión adversarial entre ellas.

## Cómo trabajar aquí

- El Sprint 0 se entrega **por bloques**, con revisión del PO entre cada uno. No avances al siguiente bloque sin que el anterior haya sido revisado.
- Cuando falte un dato de la institución piloto, **no lo inventes**: márcalo `[C]` y regístralo en `docs/07-gestion/insumos-pendientes.md`.
- Cuando una decisión de producto se tome en conversación, escríbela en `docs/07-gestion/decisiones-de-producto/` el mismo día. Lo que no queda escrito, se pierde.
