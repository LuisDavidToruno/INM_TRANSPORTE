# Documentación de SIGTI

Índice general. La numeración de carpetas indica el orden lógico de lectura, no de creación.

| Carpeta | Qué contiene | Responsable principal |
|---|---|---|
| [00-vision/](00-vision/) | Visión de producto, objetivos medibles, glosario del dominio | Product Owner |
| [01-negocio/](01-negocio/) | Actores y roles, mapa de procesos, reglas de negocio, normativa hondureña | `analista-requerimientos`, `normativa-honduras` |
| [02-requisitos/](02-requisitos/) | Casos de uso, historias de usuario, casos especiales, requisitos no funcionales | `analista-requerimientos` |
| [03-arquitectura/](03-arquitectura/) | ADRs, diagramas C4, modelo de datos, máquinas de estado, seguridad | `arquitecto-software`, `modelador-datos` |
| [04-diseno/](04-diseno/) | Mapa de navegación, wireframes, formatos oficiales impresos | `disenador-ux` |
| [05-calidad/](05-calidad/) | Escenarios Gherkin en español, plan de pruebas, hallazgos de revisión | `qa-tester` |
| [06-operacion/](06-operacion/) | Instalación on-premise, respaldos, restauración, manuales | `devops-onpremise`, `documentador-tecnico` |
| [07-gestion/](07-gestion/) | Roadmap, sprints, backlog, decisiones de producto, insumos pendientes | Product Owner |
| [plantillas/](plantillas/) | Plantillas de todos los artefactos, con ejemplo real del dominio | — |

## Reglas de la documentación

1. **Un artefacto, un archivo, un ID.** Nada de documentos monolíticos que acumulan todo.
2. **Trazabilidad hacia arriba.** Toda historia enlaza sus reglas; toda regla enlaza su norma; todo ADR enlaza sus requisitos no funcionales.
3. **Nivel de verificación explícito** en toda afirmación normativa: `[V]` `[P]` `[C]` `[I]`. Ver [CLAUDE.md](../CLAUDE.md).
4. **Diagramas en Mermaid**, dentro del `.md`. Nunca imágenes binarias.
5. **Lo que no está escrito, no existe.** Las decisiones de conversación se escriben en `07-gestion/decisiones-de-producto/` el mismo día.
6. **Los huecos se marcan, no se rellenan con suposiciones.** Un dato faltante de la institución piloto va como `[C]` y se registra en `07-gestion/insumos-pendientes.md`.
