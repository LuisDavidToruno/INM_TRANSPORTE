# 07 — Gestión

Cómo se conduce el proyecto: qué se hace, en qué orden, y qué se decidió por el camino.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| `roadmap.md` | Bloque 1 | Sprints, objetivos y dependencias |
| `backlog.md` | Bloque 3 | Historias priorizadas por valor y dependencia técnica |
| `sprints/` | Continuo | Un archivo por sprint: objetivo, alcance, resultado, retrospectiva |
| `decisiones-de-producto/` | Continuo | Decisiones del PO que no son de arquitectura, con su contexto y consecuencias |
| `insumos-pendientes.md` | **Bloque 0** | Documentos y datos que se necesitan de la institución piloto |

## Roadmap resumido

| Sprint | Objetivo |
|---|---|
| **0** | Descubrimiento y definición del negocio ← *aquí estamos* |
| 1 | Modelo de dominio, estados y experiencia; validación con la institución piloto |
| 2 | Arquitectura, elección de stack y walking skeleton — primer código |
| 3 | Catálogos maestros y gestión de flota (M-01 a M-05) |
| 4 | Solicitud de viaje y aprobaciones (M-06) — núcleo de valor |
| 5 | Programación, despacho, Orden de Misión y bitácora (M-07, M-08, M-15) |
| 6 | Combustible y viáticos con conciliación (M-09, M-10, M-13) |
| 7 | Traslado de carga y de personas externas (M-17), operación offline (M-16) |
| 8 | Reportes, transparencia, hardening, instalación on-premise y piloto |

## Sprint 0 por bloques

Se entrega en bloques con revisión del PO entre cada uno. No se avanza al siguiente sin que el anterior haya sido revisado.

| Bloque | Contenido | Estado |
|---|---|---|
| 0 | Andamiaje: `CLAUDE.md`, estructura `docs/`, subagentes, plantillas, fichas de normativa | En curso |
| 1 | Visión, glosario, actores y roles, mapa de procesos, máquina de estados, reglas `RN-xx` | Pendiente |
| 2 | Casos especiales `CE-xx` con su regla de resolución | Pendiente |
| 3 | Casos de uso, historias, no funcionales, backlog priorizado | Pendiente |
| 4 | Modelo de datos, navegación, wireframes, formatos impresos, reportes | Pendiente |

## Sobre las ceremonias

Este proyecto tiene **un Product Owner humano que también es el Scrum Master y el único desarrollador humano**, apoyado por subagentes especializados. En ese contexto:

**Se omiten** — velocity, story points, planning poker, burndown, capacity planning y daily standup. Los agentes no tienen capacidad finita, no se bloquean y no olvidan. Alimentar esas métricas consumiría tiempo sin informar ninguna decisión.

**Se conservan, porque sí deciden algo:**

- **Refinamiento** — antes de cada bloque o sprint: qué entra, qué se corta, qué falta por saber. Se registra en `sprints/`.
- **Revisión** — al cierre de cada bloque, el PO revisa y corrige. Es el gate real del proyecto.
- **Retrospectiva** — al cierre de cada sprint: qué produjo retrabajo y qué cambiamos. Breve y escrita.
- **Definition of Ready / Done** — como criterio duro, no como formalidad. Ver [../plantillas/](../plantillas/).
