# Equipo SCRUM de SIGTI

Diez especialidades materializadas como subagentes. Se invocan con la herramienta Agent.

## Nota metodológica

**SCRUM tiene tres roles: Product Owner, Scrum Master y Developers.** Arquitecto, DBA, QA y UX no son roles de SCRUM: son *sombreros de especialidad* dentro de "Developers". Modelarlos como roles separados genera silos y handoffs, que es exactamente lo que SCRUM quiere eliminar.

Aquí se modelan como **perspectivas de producción y revisión de artefactos**, que es lo que realmente son — y lo que encaja bien con subagentes, porque cada uno arranca con su contexto limpio y su prompt especializado.

El **PO y el SM son el usuario humano** y no se automatizan: son quienes deciden y priorizan.

## Roster

| Agente | Especialidad | Produce |
|---|---|---|
| [analista-requerimientos](analista-requerimientos.md) | Análisis de negocio | Procesos, `CU-xx`, `HU-xxx`, `RN-xx`, `CE-xx`, actores |
| [normativa-honduras](normativa-honduras.md) | Marco legal y control interno | Fichas `NRM-xx`, reglas derivadas, validación legal |
| [arquitecto-software](arquitecto-software.md) | Arquitectura | `ADR-xxx`, C4, estrategia de sincronización y auditoría |
| [modelador-datos](modelador-datos.md) | Modelo de datos | ER, diccionario de datos, temporalidad de parámetros |
| [disenador-ux](disenador-ux.md) | Experiencia e interfaz | Navegación, wireframes, formatos impresos |
| [qa-tester](qa-tester.md) | Calidad y revisión adversarial | Gherkin español, plan de pruebas, hallazgos |
| [dev-backend](dev-backend.md) | Backend | Dominio, reglas, API, auditoría, sincronización |
| [dev-frontend](dev-frontend.md) | Frontend | Web administrativa y cliente de campo offline |
| [devops-onpremise](devops-onpremise.md) | Infraestructura | Instalación, respaldo, restauración, actualización |
| [documentador-tecnico](documentador-tecnico.md) | Documentación | Manuales por rol, guía de campo, inducción |

## Dónde el equipo de agentes aporta valor real

Dos cosas, concretas:

1. **Aislamiento de contexto.** El especialista en normativa no necesita ver el código; el de backend no necesita el reglamento de viáticos completo. Cada uno arranca con su ventana limpia y su prompt especializado, y eso mejora la calidad de la salida de forma medible.

2. **Revisión adversarial.** Que `qa-tester` critique lo que produjo `analista-requerimientos`, y que `normativa-honduras` valide que ninguna regla contradiga el marco legal, genera hallazgos reales que una sola pasada no encuentra.

Lo que **no** aporta valor: tratar a los agentes como si tuvieran capacidad finita, velocidad o disponibilidad. No la tienen. Por eso este proyecto omite velocity, story points, burndown, planning poker, capacity planning y daily standup.

## Cadena de revisión

| Produce | Revisa | Buscando |
|---|---|---|
| `analista-requerimientos` | `qa-tester` | Huecos, ambigüedad, criterios no verificables, casos especiales sin cubrir |
| `analista-requerimientos` | `normativa-honduras` | Reglas que contradicen o ignoran el marco legal |
| `modelador-datos` | `arquitecto-software` | Modelos que no soportan temporalidad ni auditoría |
| `disenador-ux` | `analista-requerimientos` | Pantallas que perdieron campos del formato en papel |
| `dev-backend` / `dev-frontend` | `qa-tester` | Reglas implementadas de forma incompleta o distinta a lo especificado |

Los hallazgos van a `docs/05-calidad/hallazgos/` y se convierten en tareas del backlog. **Quien produjo el artefacto corrige; el revisor verifica.** No se resuelven en conversación y se olvidan.

## Convención común a todos

Todos leen `CLAUDE.md` antes de trabajar, escriben en español con el vocabulario del dominio hondureño, siguen las plantillas de `docs/plantillas/`, y **marcan `[C]` lo que no saben en lugar de inventarlo**.
