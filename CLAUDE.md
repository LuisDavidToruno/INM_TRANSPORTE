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

6. **Nada normativo se cablea.** Tarifas de viáticos, zonas, categorías, feriados, horario hábil, plazos de liquidación y matriz licencia↔vehículo son **parámetros con vigencia por rango de fechas**. Todo cálculo usa la tabla vigente **a la fecha del hecho**, no a la fecha de captura.

## Estado actual

**Sprint 0 — Descubrimiento y definición.** No hay código. **El stack tecnológico está deliberadamente diferido al Sprint 2.** Si surge una pregunta de tecnología antes de eso, respóndela en términos de *capacidades requeridas*, no de productos.

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
| M-09 | Combustible | Vales o tarjetas, consumo real, conciliación galonaje–kilometraje |
| M-10 | Viáticos y Gastos de Viaje | Cálculo, autorización, anticipo y liquidación conforme al reglamento vigente |
| M-11 | Mantenimiento y Taller | Preventivo, correctivo, llantas, repuestos, órdenes de trabajo, indisponibilidad |
| M-12 | Incidentes, Siniestros y Sanciones | Averías, accidentes, robos, multas, uso indebido, investigaciones |
| M-13 | Liquidación y Cierre | Resultado económico y operativo de la misión, desviaciones, cierre del expediente |
| M-14 | Reportes, Indicadores y Auditoría | Reportes operativos, de control interno y de auditoría; bitácora inmutable |
| M-15 | Formatos Oficiales e Impresión | Documentos físicos con folio y QR verificable |
| M-16 | Sincronización y Operación Desconectada | Captura sin conectividad y reconciliación al reconectar |
| M-17 | Traslado de Personas Externas | Manifiestos, cadena de custodia, minimización de datos, registro de consultas |

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
- **Datos personales**: no hay ley general vigente, pero sí hábeas data constitucional. Minimización, control por necesidad de conocer y registro de cada consulta en M-17.

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
