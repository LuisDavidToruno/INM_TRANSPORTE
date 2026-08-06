# Plantilla — ADR (Architecture Decision Record)

Archivo: `docs/03-arquitectura/adr/ADR-xxx-slug-corto.md`

Un ADR registra **una decisión de arquitectura, su contexto y sus consecuencias**. Se escribe cuando la decisión se toma, no después. Su valor real aparece dos años más tarde, cuando alguien pregunta "¿por qué está hecho así?" y la respuesta no depende de que quien decidió siga en el proyecto.

**Los ADR no se editan cuando cambian de opinión.** Se escribe uno nuevo que supersede al anterior, y el anterior se marca `Reemplazada por ADR-xxx`. El historial de decisiones equivocadas es parte de la documentación.

---

## Esqueleto

```markdown
# ADR-xxx — <Decisión en una línea, en presente afirmativo>

| Campo | Valor |
|---|---|
| **Estado** | Propuesta / Aceptada / Rechazada / Obsoleta / Reemplazada por ADR-xxx |
| **Fecha** | <YYYY-MM-DD> |
| **Decide** | <quién> |
| **Sprint** | <número> |

## Contexto

<La situación que obliga a decidir. Incluye las restricciones reales: normativas,
operativas, de personal, de presupuesto. Sin contexto, la decisión parece arbitraria.>

## Requisitos que la condicionan

- [RNF-xx](../../02-requisitos/no-funcionales/RNF-xx-slug.md) — <por qué pesa aquí>

## Decisión

<Qué se decidió. Directo, sin rodeos.>

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|

## Consecuencias

**Positivas** — …
**Negativas** — <sé honesto; un ADR sin consecuencias negativas no fue una decisión, fue una preferencia>
**Deuda aceptada** — <lo que sabemos que habrá que rehacer, y bajo qué señal>

## Revisión

<Qué evento nos haría reconsiderar esta decisión.>
```

---

## Ejemplo completo

# ADR-000 — Diferir la selección del stack tecnológico al Sprint 2

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-06 |
| **Decide** | Product Owner |
| **Sprint** | 0 |

## Contexto

El proyecto arranca en greenfield absoluto: repositorio vacío, sin código heredado ni compromisos tecnológicos previos con la institución. La tentación natural es elegir stack de inmediato — es la decisión que más se parece a "avanzar".

Pero las restricciones que realmente van a decidir el stack todavía se están descubriendo. En el Sprint 0 ya aparecieron cuatro que no eran obvias al inicio: operación offline-first durante días completos en zonas sin cobertura, bitácora append-only inmutable exigida por control interno, generación de documentos oficiales imprimibles con verificación por QR, y parámetros normativos con vigencia por rango de fechas porque el reglamento de viáticos cambió hace tres semanas.

Ninguna de esas restricciones es visible desde un requerimiento genérico de "sistema de gestión de flota". Elegir stack antes de conocerlas produce una arquitectura que pelea contra el problema durante todo el proyecto.

## Requisitos que la condicionan

- `RNF-03` — Operación sin conectividad durante 7 días continuos, sin pérdida de datos
- `RNF-07` — Bitácora de auditoría append-only con hash encadenado
- `RNF-11` — Instalación y respaldo operables sin equipo de TI dedicado
- `RNF-14` — Parámetros normativos con vigencia temporal

## Decisión

**No se nombra ningún lenguaje, framework ni motor de base de datos hasta el Sprint 2.** Toda pregunta de tecnología que surja antes se responde en términos de *capacidades requeridas*.

Las capacidades identificadas se registran en [`docs/03-arquitectura/README.md`](../README.md) a medida que aparecen, y forman la matriz de evaluación contra la cual se decidirá en el `ADR-001`.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| Elegir stack ahora | Se puede empezar a codificar de inmediato; sensación de avance | La decisión se tomaría sin conocer las restricciones que la determinan; el costo de revertirla crece cada sprint | El riesgo de elegir mal supera el beneficio de arrancar antes |
| Elegir "lo de siempre" en el sector público hondureño | Personal capacitado disponible, licenciamiento probablemente ya adquirido | Es un argumento válido — pero es un *insumo* para la decisión, no la decisión. Y no responde a offline-first | Se incorpora como criterio de evaluación en `ADR-001`, no como decisión previa |
| Prototipo técnico en Sprint 0 | Reduce incertidumbre con evidencia real | Consume el tiempo del Sprint 0, que se necesita para entender el dominio | Se traslada al Sprint 2 como walking skeleton, que es donde rinde |

## Consecuencias

**Positivas**

- El Sprint 0 se concentra en el dominio, que es donde está el riesgo real de este proyecto
- La decisión de stack llegará con una matriz de capacidades concreta en lugar de preferencias
- Los artefactos del Sprint 0 y 1 son independientes de la tecnología y sobreviven a cualquier elección

**Negativas**

- No hay código ejecutable hasta el Sprint 2. Si alguien externo mide avance por líneas de código, este proyecto parecerá detenido durante dos sprints. Hay que gestionar esa expectativa explícitamente.
- Si la institución ya tiene un compromiso tecnológico de facto que nadie mencionó, se descubrirá tarde. Mitigación: el insumo #9 de [`insumos-pendientes.md`](../../07-gestion/insumos-pendientes.md) pregunta exactamente eso.

**Deuda aceptada**

Ninguna. Esta decisión no genera deuda técnica; la difiere.

## Revisión

Se reconsidera si la institución impone un stack por política de TI antes del Sprint 2. En ese caso el `ADR-001` documenta la restricción como dada y evalúa qué capacidades quedan comprometidas y cómo se compensan.
