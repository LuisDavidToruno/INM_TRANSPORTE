# ADR-000 — Diferir la selección del stack tecnológico al Sprint 2

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-06 |
| **Decide** | Product Owner |
| **Sprint** | 0 |

## Contexto

El proyecto arranca en greenfield absoluto: repositorio vacío, sin código heredado ni compromisos tecnológicos previos verificados con la institución. La tentación natural es elegir stack de inmediato — es la decisión que más se parece a "avanzar".

Pero las restricciones que realmente van a decidir el stack todavía se están descubriendo. En el Bloque 0 del Sprint 0 ya aparecieron cuatro que no eran obvias al inicio:

1. **Operación offline-first** durante días completos en zonas sin cobertura — más de 2 millones de personas del área rural hondureña no tienen acceso a internet (INE, EPHPM julio 2025)
2. **Bitácora append-only inmutable** exigida por el control interno del TSC
3. **Generación de documentos oficiales imprimibles** con folio, firma, sello y verificación por QR, porque el control en carretera es físico
4. **Parámetros normativos con vigencia por rango de fechas**, porque el reglamento de viáticos cambió el 23 de julio de 2026 y volverá a cambiar

Ninguna de esas restricciones es visible desde un requerimiento genérico de "sistema de gestión de flota". Elegir stack antes de conocerlas produce una arquitectura que pelea contra el problema durante todo el proyecto.

## Requisitos que la condicionan

Los `RNF-xx` se escriben en el Bloque 3. Las capacidades ya identificadas están registradas en [`docs/03-arquitectura/README.md`](../README.md) y forman la matriz de evaluación del futuro `ADR-001`.

## Decisión

**No se nombra ningún lenguaje, framework ni motor de base de datos hasta el Sprint 2.** Toda pregunta de tecnología que surja antes se responde en términos de *capacidades requeridas*.

Los subagentes de desarrollo (`dev-backend`, `dev-frontend`, `devops-onpremise`) no producen código antes del Sprint 2; su aporte previo es definir restricciones de viabilidad y operabilidad.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| Elegir stack ahora | Se puede empezar a codificar de inmediato; sensación de avance | La decisión se tomaría sin conocer las restricciones que la determinan, y el costo de revertirla crece cada sprint | El riesgo de elegir mal supera el beneficio de arrancar antes |
| Elegir "lo de siempre" del sector público hondureño | Personal capacitado disponible, licenciamiento probablemente ya adquirido | Es un argumento válido, pero es un *insumo* para la decisión, no la decisión. Y no responde a offline-first | Se incorpora como criterio de evaluación en `ADR-001` |
| Prototipo técnico en el Sprint 0 | Reduce incertidumbre con evidencia real | Consume el tiempo del Sprint 0, que se necesita para entender el dominio | Se traslada al Sprint 2 como walking skeleton, que es donde rinde |

## Consecuencias

**Positivas**

- El Sprint 0 se concentra en el dominio, que es donde está el riesgo real de este proyecto
- La decisión de stack llegará con una matriz de capacidades concreta en lugar de preferencias
- Los artefactos del Sprint 0 y 1 son independientes de la tecnología y sobreviven a cualquier elección

**Negativas**

- No hay código ejecutable hasta el Sprint 2. Si alguien externo mide avance por líneas de código, el proyecto parecerá detenido durante dos sprints. **Hay que gestionar esa expectativa explícitamente con la institución.**
- Si la institución ya tiene un compromiso tecnológico de facto que nadie mencionó, se descubrirá tarde. Mitigado por el insumo #9 de [`insumos-pendientes.md`](../../07-gestion/insumos-pendientes.md), que pregunta exactamente eso.

**Deuda aceptada**

Ninguna. Esta decisión no genera deuda técnica; la difiere.

## Revisión

Se reconsidera si la institución impone un stack por política de TI antes del Sprint 2. En ese caso el `ADR-001` documenta la restricción como dada, y evalúa qué capacidades quedan comprometidas y cómo se compensan.
