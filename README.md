# SIGTI — Sistema de Gestión de Transporte Institucional

Sistema genérico de gestión de transporte y flota vehicular para **instituciones públicas de Honduras**. Despliegue on-premise, una instancia por institución.

Cubre el ciclo completo de una movilización institucional: solicitud, autorización, programación y despacho, ejecución con bitácora, control de combustible, viáticos, liquidación y cierre — con la trazabilidad que exige el control interno del Estado.

Lo que se traslada puede ser **personal de la institución, personas externas, o carga** (equipos, herramientas, insumos, materiales), o una combinación.

## Estado

**Sprint 0 — Descubrimiento y definición.** Sin código todavía. La elección del stack tecnológico está deliberadamente diferida al Sprint 2.

## Documentación

| Ruta | Contenido |
|---|---|
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
