# HU-136 — Cerrar la asignación de un puesto con el acta que enumera todo lo que queda abierto

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema · ACT-03 Jefatura Inmediata (levanta el acta) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — necesita la regla candidata de cierre de asignación con custodias activas, que hoy no existe |

## Historia

**Como** Jefatura Inmediata del servidor que deja el puesto
**quiero** que al cerrar su asignación el sistema me presente y clasifique **todo** lo que queda abierto —custodias físicas, actos pendientes de decisión, misiones en ejecución— y me impida cerrar mientras haya custodia física sin entregar
**para** que ninguna rotación deje vehículos sin custodio, vales vivos sin dueño ni expedientes huérfanos, y para que el saliente pierda sus permisos el mismo día

## Contexto

Éste es el escenario que la rotación produce todos los meses y el que más daño hace si no se previó. Honduras tuvo elecciones en noviembre de 2025 y cambio de gobierno en enero de 2026 `[V]` ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)); la rotación se concentra ahí `[I]`.

**El principio** ([actores-y-roles §2.4](../../01-negocio/actores-y-roles.md), autoridad): *"el expediente no es de la persona, es del puesto y de su unidad. La autoría histórica sí es de la persona y no se reasigna jamás."*

La clasificación de lo abierto, con su tratamiento:

| Tipo | Tratamiento |
|---|---|
| **Custodia física** — vehículos bajo tarjeta de responsabilidad, vales emitidos sin canjear, efectivo del fondo, llaves | **Bloqueo duro.** No se cierra sin acta de entrega-recepción |
| **Actos pendientes de decisión** | Quedan atribuidos **al puesto**; escalan al superior si el puesto queda vacante más allá del plazo |
| **Misiones en ejecución** | **No se interrumpen.** Continúan bajo el puesto |
| **Autoría histórica** | **No se toca.** Persona **y** puesto, ambos congelados en el asiento |

`RNF-15` lo exige en números: *"vehículos que queden sin custodio tras una baja: **0**"*, *"expedientes abiertos que queden sin responsable: **0**"*.

## Reglas que la gobiernan

- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — Todo vehículo tiene custodio vigente, y el cambio de custodia exige constancia
- [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) — Todo vale tiene responsable receptor y estado; no puede quedar sin dueño
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — El saldo no devuelto es obligación con responsable y ciclo propio que sobrevive al cierre
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — Lo pendiente escala al puesto superior transcurrido el plazo parametrizado
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — La autoría histórica registra persona y puesto y no se modifica
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — Una misión no cambia de estado porque alguien deje su puesto

## Casos especiales que la afectan

- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Si el saliente es el motorista de una misión en ruta, es sustitución en ruta y no un asunto de esta historia
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — El saldo del fondo que el saliente no devolvió
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — Lo no entregado constituye saldo de apertura del ejercicio siguiente

## Criterios de aceptación

```gherkin
# language: es
Característica: Cierre de la asignación de un puesto con acta de lo pendiente

  Antecedentes:
    Dada una persona "Ramón Cáceres" ocupando el puesto "Jefe de Transporte" desde el "2024-02-01"
    Y 40 vehículos bajo su tarjeta de responsabilidad
    Y 2 vales de combustible emitidos y no canjeados a su nombre
    Y 5 solicitudes de fondo pendientes de firma en ese puesto
    Y 3 misiones en estado "EN_RUTA" programadas por él

  Escenario: Se rechaza cerrar la asignación con custodias físicas activas
    Cuando el Administrador del Sistema intenta cerrar la asignación de "Ramón Cáceres" al "2026-09-30"
    Entonces el sistema rechaza el cierre
    Y muestra "No se cierra la asignación con custodia física activa: 40 vehículos bajo tarjeta de responsabilidad y 2 vales emitidos sin canjear. Levante el acta de entrega-recepción o el acta de entrega unilateral."
    Y presenta el inventario de las 42 custodias con su identificación

  Escenario: Se rechaza cerrar la asignación sin receptor identificado en el acta
    Cuando la Jefatura Inmediata genera el acta de entrega-recepción sin indicar receptor
    Entonces el sistema rechaza la generación
    Y muestra "Indique el receptor de cada custodia: la persona entrante o la jefatura inmediata como depositario transitorio."

  Escenario: El acta de cierre enumera y clasifica todo lo abierto antes de confirmar
    Cuando la Jefatura Inmediata inicia el cierre de la asignación de "Ramón Cáceres"
    Entonces el sistema presenta 4 bloques: 42 custodias físicas, 5 actos pendientes de decisión, 3 misiones en ejecución y la autoría histórica
    Y indica que las custodias son bloqueo duro
    Y indica que la autoría histórica no se reasigna

  Escenario: Los actos pendientes quedan atribuidos al puesto, no a la persona
    Dada el acta de entrega-recepción firmada por el receptor
    Cuando el Administrador del Sistema cierra la asignación al "2026-09-30"
    Entonces las 5 solicitudes de fondo quedan atribuidas al puesto "Jefe de Transporte"
    Y quien ocupe el puesto las ve al entrar
    Y ninguna aparece a nombre de "Ramón Cáceres"

  Escenario: Las misiones en ruta no se interrumpen por el cierre
    Cuando el Administrador del Sistema cierra la asignación al "2026-09-30"
    Entonces las 3 misiones "EN_RUTA" siguen en "EN_RUTA"
    Y quedan bajo el puesto "Jefe de Transporte"
    Y el sistema muestra "3 misiones EN_RUTA continúan bajo el puesto. No se interrumpe ninguna operación en carretera."

  Escenario: El cierre revoca los permisos el mismo día
    Dado el cierre registrado al "2026-09-30"
    Cuando "Ramón Cáceres" intenta programar una misión el "2026-10-01"
    Entonces el sistema rechaza la operación
    Y muestra "Su asignación al puesto Jefe de Transporte terminó el 30/09/2026."

  Escenario: El acta de cierre se emite con folio y queda en el expediente
    Cuando el Administrador del Sistema completa el cierre
    Entonces el sistema emite el acta de cierre de asignación con folio y QR verificable
    Y el acta enumera las 42 custodias con su receptor, los 5 actos reatribuidos al puesto y las 3 misiones en ejecución
    Y el acta queda consultable desde el expediente de la persona y desde el del puesto

  Escenario: La autoría histórica no cambia tras el cierre
    Dada una Orden de Misión "OM-2025-0188" autorizada por "Ramón Cáceres" el "2025-06-12"
    Cuando el Auditor Interno la consulta el "2027-03-01"
    Entonces el asiento muestra "Ramón Cáceres" y el puesto "Jefe de Transporte" que ocupaba el "2025-06-12"
    Y el asiento es idéntico al que se consultaba antes del cierre
```

## Fuera de alcance

- El caso en que el saliente se fue y no entregó — es [HU-137](HU-137-entrega-unilateral-con-hallazgo-abierto.md)
- El traspaso masivo de custodias en una sola operación — es [HU-138](HU-138-traspaso-masivo-de-custodias-con-acta.md)
- La inmutabilidad de la autoría en la pista de auditoría — es [HU-139](HU-139-autoria-historica-que-jamas-se-reasigna.md)
- El acta de entrega-recepción del vehículo como bien — es [HU-099](HU-099-emitir-tarjeta-de-responsabilidad-y-traspasar-custodia.md)

## Notas y pendientes

- `[P]` La exigencia de acta de entrega-recepción y tarjeta de responsabilidad se apoya en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); el articulado exacto no se pudo extraer
- `[C]` Plazo tras el cual los actos pendientes de un puesto vacante escalan al superior — insumo **#32**
- `[C]` Quién es el depositario transitorio válido cuando no hay persona entrante — insumo **#27**
- **Regla candidata:** *No se cierra una asignación de puesto con custodias físicas activas sin acta de entrega-recepción o acta de entrega unilateral con hallazgo abierto.* Es la candidata 2 de [actores-y-roles §8](../../01-negocio/actores-y-roles.md) y **ninguna de las 97 la recoge**: `RN-22` gobierna la custodia del vehículo, no el cierre de la asignación de puesto ni los vales ni el efectivo
