# 05 — Calidad

Cómo sabemos que el sistema hace lo que debe.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| `features/` | Bloque 3 | Escenarios Gherkin en español, uno por historia o grupo de historias |
| `plan-de-pruebas.md` | Sprint 2 | Estrategia, niveles, cobertura y criterios de aceptación de release |
| `hallazgos/` | Continuo | Hallazgos de revisión adversarial, con su estado |

## Gherkin en español

Los escenarios se escriben en español porque los va a leer el personal de la institución durante la validación, no solo el desarrollador. Palabras clave: `Característica`, `Antecedentes`, `Escenario`, `Esquema del escenario`, `Dado`, `Cuando`, `Entonces`, `Y`, `Pero`, `Ejemplos`.

Ver la plantilla y un ejemplo completo del dominio en [../plantillas/criterios-aceptacion-gherkin.md](../plantillas/criterios-aceptacion-gherkin.md).

## Revisión adversarial

En este proyecto la revisión no es una lectura de cortesía. Cada artefacto lo revisa una especialidad distinta a la que lo produjo:

| Artefacto producido por | Lo revisa | Buscando |
|---|---|---|
| `analista-requerimientos` | `qa-tester` | Huecos, ambigüedad, criterios no verificables, casos especiales no cubiertos |
| `analista-requerimientos` | `normativa-honduras` | Reglas que contradicen o ignoran el marco legal |
| `modelador-datos` | `arquitecto-software` | Modelos que no soportan la temporalidad o la auditoría exigida |
| `disenador-ux` | `analista-requerimientos` | Pantallas que perdieron campos del formato en papel |
| Código (Sprint 2+) | `qa-tester` | Reglas de negocio implementadas de forma incompleta o distinta a lo especificado |

Los hallazgos se registran en `hallazgos/` y se convierten en tareas del backlog. No se resuelven en conversación y se olvidan.

## Qué se prueba con especial rigor

Estas son las áreas donde un error tiene consecuencia legal o financiera, no solo molestia:

1. **Segregación de funciones** — que el sistema realmente impida que la misma persona solicite y autorice.
2. **Matriz licencia ↔ vehículo** — que no permita asignar un motorista sin licencia habilitante o vencida.
3. **Cálculo de viáticos con tarifas versionadas** — que use la tabla vigente a la fecha del viaje, no a la de captura.
4. **Conciliación combustible ↔ kilometraje** — que detecte las desviaciones que busca el auditor.
5. **Bitácora inmutable** — que ninguna ruta del sistema permita alterar o borrar un registro cerrado.
6. **Sincronización offline** — que ningún dato capturado en campo se pierda ni se sobrescriba en silencio.
