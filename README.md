# SIGTI — Sistema de Gestión de Transporte Institucional

Sistema genérico de gestión de transporte y flota vehicular para **instituciones públicas de Honduras**. Despliegue on-premise, una instancia por institución.

Cubre el ciclo completo de una movilización institucional: solicitud, autorización, programación y despacho, ejecución con bitácora, control de combustible y peajes, liquidación y cierre — con la trazabilidad que exige el control interno del Estado.

Lo que se traslada puede ser **personal de la institución, personas externas, o carga** (equipos, herramientas, insumos, materiales), o una combinación.

**Los viáticos no son de este sistema.** Los maneja ARGOS, con el que SIGTI se integra — ver [DP-001](docs/07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

## Estado

**Sprint 0 — Descubrimiento y definición.** Los cinco bloques están escritos: 97 reglas de negocio, 28 casos especiales, 18 casos de uso, 125 historias con criterios de aceptación, 21 requisitos no funcionales, modelo de datos y 126 pantallas inventariadas.

Estado detallado y trabajo abierto en **[HANDOFF.md](HANDOFF.md)**.

## Cómo levantarlo en local

**Todavía no se puede: no hay código.**

La elección del stack tecnológico está **deliberadamente diferida al Sprint 2** por [ADR-000](docs/03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md), porque las restricciones que van a decidirla se estaban descubriendo durante el análisis. Cuando el stack se defina, esta sección lleva los comandos de arranque.

Mientras tanto, lo que sí se puede abrir sin instalar nada:

- **Los mockups** — [`docs/04-diseno/mockups/tablero-de-mockups.html`](docs/04-diseno/mockups/tablero-de-mockups.html). Un archivo autocontenido: se abre con doble clic, sin servidor ni red
- **La documentación** — los diagramas Mermaid renderizan directo en GitHub

## Documentación

| Ruta | Contenido |
|---|---|
| [HANDOFF.md](HANDOFF.md) | **Estado del trabajo: qué está abierto, qué está cerrado y qué sigue** |
| [DECISIONES.md](DECISIONES.md) | Índice de decisiones, con fecha, qué se decidió y por qué |
| [CLAUDE.md](CLAUDE.md) | Contexto del proyecto, premisas, módulos y convenciones |
| [docs/00-vision/](docs/00-vision/) | Visión de producto y glosario del dominio |
| [docs/01-negocio/](docs/01-negocio/) | Actores, procesos, reglas de negocio y normativa hondureña |
| [docs/02-requisitos/](docs/02-requisitos/) | Casos de uso, historias de usuario, casos especiales, no funcionales |
| [docs/03-arquitectura/](docs/03-arquitectura/) | ADRs, C4, modelo de datos, máquinas de estado, seguridad |
| [docs/04-diseno/](docs/04-diseno/) | Navegación, wireframes, formatos oficiales impresos |
| [docs/05-calidad/](docs/05-calidad/) | Escenarios Gherkin y plan de pruebas |
| [docs/06-operacion/](docs/06-operacion/) | Instalación on-premise, respaldos, manuales |
| [docs/07-gestion/](docs/07-gestion/) | Roadmap, sprints, backlog, decisiones de producto |
| [docs/plantillas/](docs/plantillas/) | Plantillas de todos los artefactos |

## Metodología

SCRUM con un Product Owner humano y un equipo de especialidades materializado en subagentes de Claude Code — ver [.claude/agents/](.claude/agents/).
