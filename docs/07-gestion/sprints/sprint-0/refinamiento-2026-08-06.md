# Refinamiento — Sprint 0 — 2026-08-06

**Participantes:** Product Owner. Consultadas las especialidades de análisis de requerimientos, normativa hondureña y diseño de proceso.
**Alcance de la sesión:** definición del alcance general del producto y estructura del Sprint 0.

## Decisiones

| # | Decisión | Motivo | Consecuencia |
|---|---|---|---|
| 1 | El sistema deja de ser específico del INM y pasa a ser genérico para instituciones públicas hondureñas | Aplicabilidad más amplia; el proceso de transporte es sustancialmente el mismo en todas | Nada institucional-específico se cablea; todo va a catálogos configurables |
| 2 | El objeto del traslado incluye carga (equipos, herramientas, insumos), no solo personas | Es la operación real: muchos viajes mueven cosas, no gente | La entidad central deja de ser "viaje de pasajeros" y pasa a ser **Orden de Misión** con objeto de traslado tipificado |
| 3 | El **tipo de vehículo** es el eje de compatibilidad del sistema | Determina qué vehículo puede atender qué necesidad | M-02 modela tipos con atributos que permiten resolver compatibilidad por regla, no por etiqueta |
| 4 | El traslado de personas externas entra al núcleo, no como módulo opcional | Decisión del PO | M-17 se diseña desde el inicio con minimización de datos y registro de consultas |
| 5 | La selección de stack se difiere al Sprint 2 | Las restricciones que la determinan aún se están descubriendo | Registrada como [ADR-000](../../../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md) |
| 6 | El Sprint 0 se entrega por bloques con revisión del PO entre cada uno | Reduce retrabajo; el PO mantiene el control del contenido | No se avanza a un bloque sin revisión del anterior |
| 7 | Se omiten velocity, story points, burndown y daily standup | Un solo humano más subagentes: esas métricas no informan ninguna decisión | Se conservan refinamiento, revisión, retrospectiva y DoR/DoD |
| 8 | El despliegue es on-premise, una instancia por institución | Los servidores son internos de cada institución | Multi-dependencia y multi-delegación dentro de la instancia; no multi-institución |
| 9 | Hay institución piloto real | El análisis se ancla en documentos y procesos existentes | Se levanta la lista de [insumos pendientes](../../insumos-pendientes.md); lo que falte se marca `[C]`, no se inventa |

## Historias refinadas

Ninguna todavía. El Sprint 0 produce artefactos de definición; las historias se escriben en el Bloque 3.

## Preguntas abiertas

| # | Pregunta | A quién | Bloquea | En insumos |
|---|---|---|---|---|
| 1 | Texto y tablas del Acuerdo 401-2026 de viáticos | Gerencia Administrativa / SEFIN | M-10 completo | #3 |
| 2 | Formatos en papel vigentes de bitácora, requisición y vale | Encargado de transporte | Bloque 4 completo | #2 |
| 3 | Niveles de autorización por destino, monto y jerarquía | Gerencia Administrativa | M-01, M-06 | #4 |
| 4 | Códigos del objeto del gasto que usa la institución | Gerencia Administrativa | Imputación presupuestaria | #8 |
| 5 | ¿Hay compromiso tecnológico previo de la unidad de TI? | Unidad de Informática | `ADR-001` en Sprint 2 | #9 |

## Cambios de alcance

**Entró:** traslado de carga como objeto de primera clase; traslado de personas externas al núcleo.
**Salió:** la especificidad institucional del INM.
**Se difirió:** toda decisión tecnológica.

## Resultado del Bloque 0

Entregado: `CLAUDE.md`, estructura completa de `docs/`, 11 plantillas con ejemplo real del dominio, 9 fichas normativas `NRM-01` a `NRM-09` con nivel de verificación y fuentes, registro de riesgos normativos, lista de insumos pendientes, `ADR-000`, y los 10 subagentes del equipo en `.claude/agents/`.

## Acuerdos para la siguiente sesión

1. El PO revisa el Bloque 0 antes de que arranque el Bloque 1.
2. El PO gestiona la sesión de levantamiento con la institución piloto: dos horas, con los formatos en papel sobre la mesa, y con un motorista de años en el puesto presente. **Es la fuente del Bloque 2.**
3. El Bloque 1 arranca aunque falten insumos: lo que dependa de ellos se marca `[C]` y no se inventa.
