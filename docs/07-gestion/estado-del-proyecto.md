# Estado del proyecto — Sprint 0

**Actualizado 2026-08-18.** 12 commits, 216 archivos de documentación.

## Dónde estamos

```mermaid
stateDiagram-v2
    direction TB

    state "SPRINT 0 — Descubrimiento y definicion" as S0 {
        [*] --> B0

        state "Bloque 0 · Andamiaje" as B0
        state "Bloque 1 · Negocio y reglas" as B1
        state "Bloque 2 · Casos especiales" as B2
        state "Bloque 3 · Requisitos" as B3
        state "Bloque 4 · Diseno y modelo" as B4

        B0 --> B1: revisado por el PO
        B1 --> B2: 48 hallazgos corregidos
        B2 --> B3: 19 hallazgos corregidos
        B3 --> B4: pendiente
        B4 --> [*]
    }

    S0 --> S1: cierre del Sprint 0
    state "SPRINT 1 · Validacion con la institucion" as S1
    S1 --> S2
    state "SPRINT 2 · Stack y walking skeleton" as S2
    S2 --> [*]: primer codigo

    note right of B0
        LISTO
        CLAUDE.md, 11 plantillas,
        10 fichas normativas,
        10 subagentes
    end note

    note right of B1
        LISTO
        Vision, glosario, 17 actores,
        14 procesos, maquina de estados,
        97 reglas RN-xx
    end note

    note right of B2
        LISTO
        28 casos especiales CE-xx
        de la operacion real
    end note

    note right of B3
        EN CURSO
        97 reglas · 21 RNF · 18 casos de uso
        Historias HU-xxx: escribiendose
        Backlog priorizado: pendiente
    end note

    note right of B4
        NO INICIADO
        Modelo de datos, navegacion,
        wireframes, formatos impresos
        Aqui viven los mockups
    end note
```

## Qué hace falta para empezar los mockups

```mermaid
flowchart TB
    subgraph LISTO["Ya disponible"]
        A["17 actores<br/>quien usa cada pantalla"]
        B["18 casos de uso<br/>los flujos y sus excepciones"]
        C["97 reglas<br/>que bloquea, que advierte,<br/>y con que mensaje exacto"]
        D["28 casos especiales<br/>las pantallas dificiles"]
        E["21 requisitos no funcionales<br/>restricciones de campo e impresion"]
    end

    subgraph FALTA["Falta, y lo podemos hacer nosotros"]
        F["Historias con Gherkin<br/>en curso"]
        G["Modelo de datos<br/>que campos existen"]
        H["Mapa de navegacion"]
    end

    subgraph BLOQUEA["Depende de la institucion"]
        I["INSUMO #2<br/>Formatos en papel vigentes:<br/>bitacora, requisicion, salida,<br/>vale, orden de mision, acta"]
    end

    LISTO --> M["MOCKUPS<br/>Bloque 4"]
    F --> M
    G --> M
    H --> M
    I --> M

    style I fill:#ffe0e0,stroke:#c00,stroke-width:3px
    style M fill:#e8f4ff,stroke:#06c,stroke-width:2px
```

## El bloqueante real es uno solo

**Insumo #2 — los formatos en papel que la institución usa hoy.**

El principio de diseño ya está fijado en [`docs/04-diseno/README.md`](../04-diseno/README.md):

> El operador que lleva años llenando un formato preimpreso debe encontrar **los mismos campos, con los mismos nombres, en el mismo orden** en la pantalla.

Sin los formatos, cualquier mockup es una invención que el personal no va a reconocer. Y la paridad pantalla↔papel reduce el costo de adopción más que cualquier funcionalidad.

**Qué pedir, concretamente:** bitácora de viaje, requisición de vehículo, boleta de salida, vale o cupón de combustible, orden de misión, acta de entrega-recepción. Fotos de los formularios llenos sirven — de hecho sirven más que los formularios en blanco, porque muestran cómo se usan de verdad.

## Lo que podemos adelantar sin ese insumo

| Artefacto | Estado |
|---|---|
| Historias de usuario con criterios Gherkin | En curso |
| Backlog priorizado con DoR verificada | Sigue |
| **Modelo conceptual de datos y diagrama ER** | Se puede hacer ya |
| **Mapa de navegación por rol** | Se puede hacer ya |
| Wireframes de las pantallas que no replican papel — cola de conflictos, tablero de seguimiento, conciliación | Se pueden hacer ya |
| Wireframes de captura — solicitud, bitácora, vale, liquidación | **Esperan el insumo #2** |

Es decir: **se puede avanzar bastante en el Bloque 4 sin los formatos**, pero las pantallas de captura —que son la mitad del sistema— no deberían dibujarse antes de verlos.

## Decisiones del PO pendientes

Ninguna bloquea los mockups, pero sí condicionan el diseño:

| # | Decisión | Impacto |
|---|---|---|
| 1 | Ratificar o revertir [DP-002](decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md) — segregación en delegaciones pequeñas | Alto. Decide si las delegaciones operan |
| 2 | Insumo #26 — pronunciamiento de **Auditoría Interna** | Alto. Tarda semanas; conviene moverlo ya |
| 3 | Insumo #36 — ¿hay cisterna o bidones de combustible? | Cambia el circuito completo de M-09 |
| 4 | `HB3-02` — ¿el reclamo de peaje cierra la misión o la marca con hallazgo? | Medio. Ya resuelto provisionalmente |
| 5 | `HB3-07` — ¿el salvoconducto ampara al motorista? | Medio. Ya resuelto provisionalmente |

## Insumos abiertos

**76 en total.** Los dos bloqueantes de verdad son el **#1** (reglamento interno de uso de vehículos) y el **#2** (formatos en papel). El resto tiene tratamiento provisional documentado.
