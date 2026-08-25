# HU-128 — Registrar quién ocupa un puesto, con vigencia y tipo de ocupación, admitiendo el solape del traspaso

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — el solape máximo en la coocupación está `[C]` (insumo #27) |

## Historia

**Como** Administrador del Sistema
**quiero** registrar la asignación de una persona a un puesto con fecha de inicio, fecha de fin y tipo —titular, interino o por delegación—
**para** que los permisos de esa persona nazcan y mueran con la asignación, y para que el traspaso entre el saliente y el entrante se pueda hacer sin que ninguno de los dos preste su clave

## Contexto

La asignación de puesto es la bisagra entre la persona —que es **espejo de Talento Humano** y no se crea en SIGTI ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md))— y el permiso, que es del puesto.

Dos cardinalidades que la práctica impone ([actores-y-roles §2.3](../../01-negocio/actores-y-roles.md), autoridad):

- **Una persona ocupa varios puestos.** El Jefe de Transporte que además es custodio de dos vehículos. Sus permisos se acumulan; **sus incompatibilidades también, y se evalúan sobre la persona**.
- **Un puesto lo ocupan varias personas a la vez** durante el traspaso. Ambos ven lo mismo; **los actos de cada uno quedan a su propio nombre**.

Lo que hoy se hace en su lugar es prestar la clave, que destruye la trazabilidad de golpe y no deja rastro de que ocurrió.

## Reglas que la gobiernan

- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — La persona es espejo de Talento Humano; SIGTI no crea personas ni edita su identidad
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La verificación de incompatibilidad es por persona, aunque ocupe varios puestos
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El acto queda a nombre de quien lo ejecutó, no del puesto en abstracto
- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) — La disponibilidad de la persona se lee del espejo, no se declara en SIGTI
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El solape máximo de coocupación es parámetro con vigencia, no una constante

## Casos especiales que la afectan

- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — La persona figura de vacaciones o incapacitada en el espejo mientras ocupa el puesto
- **Caso especial candidato:** *persona con baja en Talento Humano y asignación de puesto todavía abierta en SIGTI* — la revocación de acceso depende de un evento espejado que puede no llegar

## Criterios de aceptación

```gherkin
# language: es
Característica: Ocupación de un puesto por una persona, con vigencia

  Antecedentes:
    Dado un puesto "Jefe de Transporte" vigente en la unidad "Unidad de Transporte"
    Y una persona "María López" con identidad espejada de Talento Humano y alta vigente
    Y un parámetro "solape_maximo_coocupacion" de "15" días vigente y aprobado

  Escenario: Se rechaza crear una persona desde SIGTI
    Cuando el Administrador del Sistema intenta dar de alta a la persona "Carlos Fúnez" que no existe en el espejo
    Entonces el sistema rechaza el alta
    Y muestra "Las personas provienen de Talento Humano y no se crean en SIGTI. Si Carlos Fúnez ya tiene alta, espere la sincronización o solicite la reconciliación. Última sincronización: 24/08/2026 03:10."

  Escenario: Se rechaza asignar a una persona con baja registrada en el espejo
    Dada una persona "Ramón Cáceres" con baja registrada en Talento Humano el "2026-07-31"
    Cuando el Administrador del Sistema intenta asignarlo al puesto "Jefe de Transporte" desde el "2026-09-01"
    Entonces el sistema rechaza la asignación
    Y muestra "Ramón Cáceres tiene baja en Talento Humano desde el 31/07/2026. No puede ocupar un puesto con vigencia posterior a su baja."

  Escenario: Se rechaza el solape de coocupación que excede el parámetro
    Dada una asignación de "Ramón Cáceres" como titular de "Jefe de Transporte" hasta el "2026-09-30"
    Cuando el Administrador del Sistema asigna a "María López" como titular del mismo puesto desde el "2026-08-15"
    Entonces el sistema rechaza la asignación
    Y muestra "El solape con la asignación de Ramón Cáceres sería de 46 días. El máximo configurado es de 15 días. Ajuste la fecha de inicio o cierre antes la asignación saliente."

  Escenario: Se rechaza una asignación sin fecha de inicio
    Cuando el Administrador del Sistema asigna a "María López" al puesto "Jefe de Transporte" sin fecha de inicio
    Entonces el sistema rechaza la asignación
    Y muestra "La asignación de puesto exige fecha de inicio. Los permisos se resuelven a la fecha del hecho y sin fecha de inicio no se pueden calcular."

  Escenario: Se acepta el solape del traspaso dentro del parámetro
    Dada una asignación de "Ramón Cáceres" como titular de "Jefe de Transporte" hasta el "2026-09-30"
    Cuando el Administrador del Sistema asigna a "María López" como titular del mismo puesto desde el "2026-09-20"
    Entonces el sistema acepta la asignación
    Y ambos ven los mismos expedientes entre el "2026-09-20" y el "2026-09-30"
    Y cada acto ejecutado en ese período queda a nombre de quien lo ejecutó, no del puesto

  Escenario: La asignación interina se distingue de la titular en el asiento
    Cuando el Administrador del Sistema asigna a "María López" al puesto "Jefe de Transporte" con tipo "interino" del "2026-10-01" al "2026-10-20"
    Entonces todo acto que ella ejecute en esa ventana registra el tipo de ocupación "interino"
    Y el documento impreso muestra la condición de interinato junto a la denominación del puesto

  Escenario: Una persona ocupa dos puestos y acumula sus permisos
    Dada una asignación vigente de "María López" al puesto "Jefe de Transporte"
    Cuando el Administrador del Sistema la asigna además al puesto "Custodio de vehículos de la Unidad de Transporte"
    Entonces el sistema acepta la asignación
    Y los permisos efectivos de "María López" son la unión de los roles de ambos puestos
    Y la evaluación de incompatibilidades se hace sobre "María López", no sobre cada puesto por separado

  Escenario: Fuera de la ventana de vigencia no hay permisos
    Dada una asignación de "María López" al puesto "Jefe de Transporte" del "2026-10-01" al "2026-10-20"
    Cuando "María López" intenta programar una misión el "2026-10-21"
    Entonces el sistema rechaza la operación
    Y muestra "Su asignación al puesto Jefe de Transporte terminó el 20/10/2026. No tiene ningún puesto vigente que faculte programar misiones."
```

## Fuera de alcance

- El otorgamiento de roles al puesto — es [HU-129](HU-129-otorgar-rol-al-puesto-con-alcance-y-vigencia.md)
- El cierre de la asignación con custodias y expedientes abiertos — es [HU-136](HU-136-cerrar-asignacion-de-puesto-con-acta.md)
- La suplencia por ausencia temporal — es [HU-134](HU-134-declarar-suplencia-con-vigencia-acotada.md)
- La sincronización y la reconciliación del espejo — es [HU-069](HU-069-el-espejo-nunca-diverge-en-silencio.md) y [HU-140](HU-140-espejo-de-empleados-de-solo-lectura.md)

## Notas y pendientes

- `[C]` **`solape_maximo_coocupacion`** — el "15" del criterio es dato de prueba. [actores-y-roles §2.3](../../01-negocio/actores-y-roles.md) lo deja expresamente abierto — insumo **#27**
- `[C]` Si el tipo de ocupación *"por delegación"* de [actores-y-roles §2.2](../../01-negocio/actores-y-roles.md) es una asignación de puesto o es el acto de delegación de [HU-135](HU-135-constituir-delegacion-de-firma-con-vigencia.md). **Se están modelando como cosas distintas** y hay que confirmarlo con el PO
- `[C]` Contrato de API de Talento Humano: qué eventos emite y con qué latencia llega la baja — insumo **#17**
- `[I]` Que el interinato deba figurar en el documento impreso es inferencia por analogía con la leyenda de delegación de [actores-y-roles §7.2](../../01-negocio/actores-y-roles.md); no consta exigido
