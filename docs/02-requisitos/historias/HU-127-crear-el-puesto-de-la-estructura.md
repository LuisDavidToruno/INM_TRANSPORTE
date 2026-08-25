# HU-127 — Crear el puesto como plaza de la estructura, que existe aunque esté vacante

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — depende de [HU-126](HU-126-estructura-institucional-con-vigencia.md) y del insumo #16 |

## Historia

**Como** Administrador del Sistema
**quiero** crear el **puesto** —"Encargado de Transporte de la Delegación de Choluteca"— como una plaza de la estructura con su unidad y su superior jerárquico, independiente de quién lo ocupe
**para** que los permisos, los expedientes pendientes y el escalamiento de autorizaciones cuelguen del puesto y sobrevivan a la rotación de la persona

## Contexto

**Persona, puesto y rol son tres cosas distintas**, y confundirlas es lo que rompe el sistema en el sector público hondureño, donde la rotación es alta y se concentra tras el cambio de administración ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[I]` sobre la rotación; `[V]` que hubo elecciones en noviembre de 2025 y cambio de gobierno en enero de 2026).

[actores-y-roles §2.2](../../01-negocio/actores-y-roles.md) —autoridad en esta materia— lo dice sin matices: *"Los permisos se asignan al puesto. Siempre."* Y el puesto **existe aunque esté vacante**: una solicitud pendiente de autorizar en un puesto vacante no desaparece, espera a que alguien lo ocupe o escala al superior.

El dolor actual: cuando alguien se va, se piden "los permisos del anterior" y se copian a mano. En seis meses nadie sabe por qué el auxiliar de bodega puede aprobar fondos.

## Reglas que la gobiernan

- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La verificación de incompatibilidad se hace **por persona**; el puesto es donde vive la competencia
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — El escalamiento se resuelve por el puesto superior, que debe existir en la estructura
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Toda autorización registra el rol y la competencia con que se ejerció
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El puesto es dato institucional con vigencia, no una constante del código
- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — Si el puesto proviene de la estructura de ARGOS, es espejo de solo lectura

## Casos especiales que la afectan

- Ninguno de los 28 vigentes. **Caso especial candidato:** *puesto autorizante vacante con solicitudes esperando* — [actores-y-roles §2.4](../../01-negocio/actores-y-roles.md) lo resuelve con escalamiento al puesto superior tras un plazo parametrizable, pero no hay `CE-xx` que lo documente

## Criterios de aceptación

```gherkin
# language: es
Característica: El puesto como plaza de la estructura organizativa

  Antecedentes:
    Dada una unidad "Unidad de Transporte" adscrita a "Gerencia Administrativa"
    Y un puesto "Jefe de Transporte" vigente en esa unidad

  Escenario: Se rechaza un puesto sin unidad organizativa
    Cuando el Administrador del Sistema crea el puesto "Encargado de Despacho" sin unidad organizativa
    Entonces el sistema rechaza la creación
    Y muestra "Todo puesto pertenece a una unidad organizativa. Sin unidad no se puede resolver el alcance de datos ni el escalamiento."

  Escenario: Se rechaza un puesto cuyo superior jerárquico es él mismo
    Cuando el Administrador del Sistema fija como superior de "Jefe de Transporte" al propio "Jefe de Transporte"
    Entonces el sistema rechaza la asignación
    Y muestra "Un puesto no puede ser su propio superior jerárquico. El escalamiento de RN-02 quedaría en ciclo."

  Escenario: Se rechaza un ciclo en la cadena jerárquica
    Dado un puesto "Encargado de Despacho" con superior "Jefe de Transporte"
    Cuando el Administrador del Sistema fija como superior de "Jefe de Transporte" a "Encargado de Despacho"
    Entonces el sistema rechaza la asignación
    Y muestra "La cadena jerárquica quedaría en ciclo: Jefe de Transporte → Encargado de Despacho → Jefe de Transporte."

  Escenario: Se rechaza cerrar la vigencia de un puesto con actos pendientes de decisión
    Dado un puesto "Jefe de Transporte" con 3 solicitudes de fondo pendientes de firmar
    Cuando el Administrador del Sistema intenta cerrar la vigencia del puesto al "2026-09-30"
    Entonces el sistema rechaza el cierre
    Y muestra "El puesto Jefe de Transporte tiene 3 actos pendientes de decisión. Reasígnelos a otro puesto o cierre el puesto después de resolverlos."

  Escenario: Se crea el puesto vacante y queda disponible para ocupación
    Cuando el Administrador del Sistema crea el puesto "Encargado de Transporte de la Delegación de Choluteca" en la unidad "Oficina Regional de Trámites" con superior "Jefe de Transporte"
    Entonces el sistema acepta la creación
    Y el puesto figura en estado "VACANTE"
    Y puede recibir roles antes de tener ocupante

  Escenario: El puesto vacante retiene los actos pendientes y los escala vencido el plazo
    Dado un puesto "Jefe de Transporte" en estado "VACANTE" desde el "2026-09-01"
    Y un parámetro "plazo_escalamiento_por_vacante" de "3" días hábiles vigente y aprobado
    Cuando transcurren 3 días hábiles con una solicitud pendiente de decisión en ese puesto
    Entonces el sistema escala la solicitud al puesto superior
    Y registra el escalamiento como tal, diferenciado de una autorización ordinaria
    Y muestra al superior "Escalada por vacancia del puesto Jefe de Transporte desde el 01/09/2026."

  Escenario: Dos puestos distintos pueden tener la misma denominación en unidades distintas
    Dado un puesto "Encargado de Despacho" en la unidad "Unidad de Transporte"
    Cuando el Administrador del Sistema crea "Encargado de Despacho" en la unidad "Oficina Regional de Trámites"
    Entonces el sistema acepta la creación
    Y los dos puestos son distintos e independientes en permisos y en alcance de datos
```

## Fuera de alcance

- Asignar una persona al puesto — es [HU-128](HU-128-ocupar-un-puesto-con-vigencia.md)
- Otorgar roles al puesto — es [HU-129](HU-129-otorgar-rol-al-puesto-con-alcance-y-vigencia.md)
- El cierre de la asignación con custodias abiertas — es [HU-136](HU-136-cerrar-asignacion-de-puesto-con-acta.md)
- Los niveles de autorización por monto o destino: propiedad de ARGOS ([DP-001 D-05](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))

## Notas y pendientes

- `[C]` `plazo_escalamiento_por_vacante` — el valor de "3 días hábiles" del criterio es **dato de prueba**, no valor confirmado. Es parámetro con vigencia ([HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md)) — insumo **#32**
- `[C]` **¿El puesto viene de ARGOS o lo crea SIGTI?** Si la estructura de puestos es espejo, esta historia se reduce a consultar y a complementar. Sin el contrato de API no se puede decidir — insumo **#16**
- `[C]` Qué puesto de sede central respalda a cada delegación — insumo **#27**
- `[I]` El modelo persona ≠ puesto ≠ rol es diseño de control interno recogido por [actores-y-roles §2](../../01-negocio/actores-y-roles.md), no articulado citable
