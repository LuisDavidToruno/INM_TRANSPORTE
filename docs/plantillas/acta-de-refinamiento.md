# Plantilla — Acta de refinamiento

Archivo: `docs/07-gestion/sprints/sprint-<n>/refinamiento-<fecha>.md`

El refinamiento es la única ceremonia de este proyecto que produce decisiones. El acta existe para que esas decisiones no se pierdan entre una sesión y la siguiente — que es exactamente lo que pasa cuando se discuten en conversación y no se escriben.

**Corta y concreta.** Si el acta pasa de una página, la sesión fue una discusión, no un refinamiento.

---

## Esqueleto

```markdown
# Refinamiento — Sprint <n> — <YYYY-MM-DD>

**Participantes:** <PO, y qué especialidades se consultaron>
**Alcance de la sesión:** <qué bloque o conjunto de historias se refinó>

## Decisiones

| # | Decisión | Motivo | Consecuencia |
|---|---|---|---|

## Historias refinadas

| ID | Título | Resultado |
|---|---|---|
| HU-xxx | … | Lista / Requiere división / Bloqueada por `[C]` |

## Preguntas abiertas

| # | Pregunta | A quién | Bloquea | Registrada en insumos |
|---|---|---|---|---|

## Cambios de alcance

<Qué entró, qué salió, y por qué. Si no hubo, dilo explícitamente.>

## Acuerdos para la siguiente sesión
```

---

## Ejemplo completo

# Refinamiento — Sprint 0 — 2026-08-06

**Participantes:** Product Owner. Consultadas las especialidades de análisis de requerimientos, normativa hondureña y proceso.
**Alcance de la sesión:** definición del alcance general del producto y estructura del Sprint 0.

## Decisiones

| # | Decisión | Motivo | Consecuencia |
|---|---|---|---|
| 1 | El sistema deja de ser específico del INM y pasa a ser genérico para instituciones públicas hondureñas | Aplicabilidad más amplia; el proceso de transporte es sustancialmente el mismo en todas | Nada institucional-específico se cablea; todo va a catálogos configurables |
| 2 | El objeto del traslado incluye carga (equipos, herramientas, insumos), no solo personas | Es la operación real: muchos viajes mueven cosas, no gente | La entidad central deja de ser "viaje de pasajeros" y pasa a ser Orden de Misión con objeto de traslado tipificado |
| 3 | El traslado de personas externas entra al núcleo, no como módulo opcional | Decisión del PO | M-17 se diseña desde el inicio con minimización de datos y registro de consultas |
| 4 | La selección de stack se difiere al Sprint 2 | Las restricciones que la determinan aún se están descubriendo | Registrada como `ADR-000`. Ningún artefacto del Sprint 0 y 1 depende de tecnología |
| 5 | El Sprint 0 se entrega por bloques con revisión del PO entre cada uno | Reduce retrabajo; el PO mantiene el control del contenido | No se avanza a un bloque sin revisión del anterior |
| 6 | Se omiten velocity, story points, burndown y daily standup | Un solo humano más subagentes: esas métricas no informan ninguna decisión | Se conservan refinamiento, revisión, retrospectiva y DoR/DoD |
| 7 | El despliegue es on-premise, una instancia por institución | Los servidores son internos de cada institución | Multi-dependencia dentro de la instancia, no multi-institución |

## Historias refinadas

Ninguna todavía. El Sprint 0 produce artefactos de definición; las historias se escriben en el Bloque 3.

## Preguntas abiertas

| # | Pregunta | A quién | Bloquea | Registrada en insumos |
|---|---|---|---|---|
| 1 | Texto y tablas del Acuerdo 401-2026 de viáticos | Gerencia Administrativa / SEFIN | M-10 completo | Sí, #3 |
| 2 | Formatos en papel vigentes de bitácora, requisición y vale | Encargado de transporte | Bloque 4 completo | Sí, #2 |
| 3 | Niveles de autorización por destino, monto y jerarquía | Gerencia Administrativa | M-01, M-06 | Sí, #4 |
| 4 | Códigos del objeto del gasto que usa la institución | Gerencia Administrativa | Imputación presupuestaria | Sí, #8 |
| 5 | ¿Hay compromiso tecnológico previo de la unidad de TI? | Unidad de Informática | `ADR-001` en Sprint 2 | Sí, #9 |

## Cambios de alcance

**Entró:** traslado de carga como objeto de primera clase; traslado de personas externas al núcleo.
**Salió:** la especificidad institucional del INM.
**Se difirió:** toda decisión tecnológica.

## Acuerdos para la siguiente sesión

1. El PO revisa el Bloque 0 (andamiaje, plantillas, fichas de normativa) antes de que arranque el Bloque 1.
2. El PO gestiona la sesión de levantamiento con la institución piloto — dos horas, con los formatos en papel sobre la mesa, y con un motorista de años en el puesto presente. Es la fuente del Bloque 2.
3. El Bloque 1 arranca aunque falten insumos: lo que dependa de ellos se marca `[C]` y no se inventa.
