# C4 — Contexto, contenedores y componentes

Vista estructural del sistema en los niveles del modelo C4. Los diagramas son **Mermaid dentro de los `.md`**: versionables, revisables en diff y renderizables en GitHub.

| Nivel | Archivo | Qué muestra |
|---|---|---|
| 1 — Contexto | [`contexto.md`](contexto.md) | SIGTI y los sistemas y personas con los que interactúa |
| 2 — Contenedores | [`contenedores.md`](contenedores.md) | Las piezas desplegables de SIGTI y cómo se comunican |
| 3 — Componentes | — | Se escribe cuando exista el primer módulo. Vacío es más honesto que a medias |

## Precedencia

Estos diagramas son **derivados**. Cuando contradigan a su fuente, manda la fuente:

| Materia | Autoridad |
|---|---|
| Actores, alcance de datos, incompatibilidades | [`docs/01-negocio/actores-y-roles.md`](../../01-negocio/actores-y-roles.md) |
| Fronteras con ARGOS y Talento Humano | [`ADR-001`](../adr/ADR-001-integracion-argos-talento-humano.md) y [`DP-001`](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| Stack y piezas desplegables | [`ADR-002`](../adr/ADR-002-adoptar-el-stack-tecnologico.md) |
| Transiciones de estado e invariantes | [`docs/03-arquitectura/estados/`](../estados/) |

**Los actores se citan, no se redefinen.** Si un diagrama necesita mostrar el alcance de datos de un actor, enlaza a `actores-y-roles.md` en lugar de copiar la tabla. Una tabla copiada es una tabla que va a divergir.

## Nivel de verificación

Todo lo que dependa de un insumo abierto va marcado **`[C]`** en el diagrama o en la nota que lo acompaña. Ningún elemento de estos diagramas declara un nivel superior al del artefacto que cita.
