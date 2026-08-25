# HU-126 — Levantar la estructura de la institución —dependencias, delegaciones y unidades— con vigencia por rango de fechas

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el contrato de API de ARGOS (insumo #16) para saber qué parte de la estructura es espejo y qué parte es dato propio |

## Historia

**Como** Administrador del Sistema
**quiero** levantar la institución con sus dependencias, sus delegaciones territoriales y sus unidades organizativas, cada una con su rango de vigencia
**para** que el alcance de datos, el escalamiento de autorizaciones y todo reporte histórico se resuelvan contra la estructura que existía **a la fecha del hecho**, no contra el organigrama de hoy

## Contexto

Ésta es la primera pantalla que se usa el día de la implantación y no existe ninguna historia que la cubra: `HU-001` arranca dando por existente *"un rol vigente sobre la dependencia"* sin que nadie haya creado la dependencia.

La estructura tiene **dos ejes que coexisten y no se pueden colapsar en uno** ([actores-y-roles §3.1](../../01-negocio/actores-y-roles.md), autoridad en alcance de datos):

- **Eje jerárquico** — dependencia y sus unidades descendientes. Es de descendencia, no territorial.
- **Eje territorial** — la delegación agrupa unidades **de varias dependencias**. Una delegación fronteriza puede tener personal de tres dependencias distintas.

Y las reorganizaciones ocurren: una dependencia se fusiona, una delegación se cierra, una unidad cambia de adscripción. Si la estructura se edita en su lugar, el reporte de misiones de 2026 emitido en 2028 las atribuye a la dependencia que hoy existe, no a la que las originó. Ese es exactamente el defecto que [`RNF-06`](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md) existe para impedir.

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Todo dato institucional es parámetro con rango de vigencia, mantenible sin cambio de código
- [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) — Lo que provenga de la estructura de ARGOS es espejo de solo lectura y no se edita desde SIGTI
- [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) — Cada entidad espejada muestra su última sincronización
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Una unidad no se borra: se cierra su vigencia con motivo y autor
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Todo reporte se reproduce contra la estructura vigente a su fecha de corte

## Casos especiales que la afectan

- Ninguno de los 28 vigentes. **Caso especial candidato:** *reorganización institucional con misiones en ejecución y expedientes abiertos en la unidad que desaparece* — hoy no está documentado y ocurre en cada cambio de administración.

## Criterios de aceptación

```gherkin
# language: es
Característica: Estructura institucional con vigencia por rango de fechas

  Antecedentes:
    Dada una institución configurada con siglas "INST" y nombre "Instituto de ejemplo"
    Y una dependencia "Gerencia Administrativa" vigente desde el "2026-01-01"
    Y una delegación territorial "Delegación de Choluteca" vigente desde el "2026-01-01"

  Escenario: Se rechaza cerrar una unidad con puestos ocupados vigentes
    Dada una unidad "Unidad de Transporte" con 4 puestos ocupados vigentes
    Cuando el Administrador del Sistema intenta cerrar la vigencia de "Unidad de Transporte" al "2026-09-30"
    Entonces el sistema rechaza el cierre
    Y muestra "La Unidad de Transporte tiene 4 puestos con ocupante vigente al 30/09/2026. Reubique los puestos o cierre sus asignaciones antes de cerrar la unidad."
    Y no modifica ninguna vigencia

  Escenario: Se rechaza editar una unidad espejada de ARGOS
    Dada una dependencia "Gerencia Administrativa" marcada como espejo de ARGOS
    Cuando el Administrador del Sistema intenta cambiar su denominación
    Entonces el sistema rechaza la edición
    Y muestra "La Gerencia Administrativa es espejo de ARGOS y no se edita desde SIGTI. Última sincronización: 24/08/2026 03:10. Corrija en ARGOS."

  Escenario: Se rechaza una unidad cuya vigencia excede la de su dependencia madre
    Dada una dependencia "Gerencia Administrativa" con vigencia del "2026-01-01" al "2026-12-31"
    Cuando el Administrador del Sistema crea la unidad "Unidad de Transporte" con vigencia desde el "2026-06-01" hasta el "2027-06-30"
    Entonces el sistema rechaza la creación
    Y muestra "La unidad no puede estar vigente después que su dependencia. Gerencia Administrativa cierra el 31/12/2026."

  Escenario: Se rechaza borrar una unidad que originó expedientes
    Dada una unidad "Unidad de Archivo" con 37 solicitudes de transporte originadas
    Cuando el Administrador del Sistema intenta eliminar "Unidad de Archivo"
    Entonces el sistema rechaza la eliminación
    Y muestra "Una unidad organizativa no se elimina. Cierre su vigencia con motivo; los 37 expedientes que originó seguirán consultables bajo ella."

  Escenario: El cierre de vigencia conserva la unidad para el histórico
    Dada una unidad "Unidad de Archivo" sin puestos ocupados vigentes
    Cuando el Administrador del Sistema cierra su vigencia al "2026-09-30" con motivo "fusión con Unidad de Correspondencia"
    Entonces el sistema registra el cierre con autor, momento y motivo
    Y la unidad deja de ofrecerse para nuevas solicitudes a partir del "2026-10-01"
    Y las 37 solicitudes anteriores siguen mostrando "Unidad de Archivo" como unidad de origen

  Escenario: Una unidad pertenece a una dependencia y opera en una delegación a la vez
    Cuando el Administrador del Sistema crea la unidad "Oficina Regional de Trámites" adscrita a "Gerencia Administrativa" y operando en "Delegación de Choluteca"
    Entonces el sistema acepta la creación
    Y la unidad aparece en el alcance DEPENDENCIA de "Gerencia Administrativa"
    Y aparece también en el alcance DELEGACIÓN de "Delegación de Choluteca"

  Escenario: El reporte histórico usa la estructura vigente a la fecha del hecho
    Dada una unidad "Unidad de Archivo" cerrada el "2026-09-30"
    Y una misión originada por ella el "2026-08-14"
    Cuando el Auditor Interno emite el reporte de misiones con fecha de corte "2027-03-01"
    Entonces la misión del "2026-08-14" figura bajo "Unidad de Archivo"
    Y el reporte declara la fecha de corte de conocimiento
```

## Fuera de alcance

- La creación de puestos dentro de la unidad — es [HU-127](HU-127-crear-el-puesto-de-la-estructura.md)
- La sincronización del espejo y su degradación — es [HU-069](HU-069-el-espejo-nunca-diverge-en-silencio.md)
- Los umbrales y niveles de autorización de la estructura: **son propiedad de ARGOS** ([DP-001 D-05](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))
- La identidad institucional visible en los documentos oficiales — es [HU-150](HU-150-poner-en-marcha-la-institucion-con-parametros-vacios.md)

## Notas y pendientes

- `[C]` **Qué parte de la estructura es espejo de ARGOS y qué parte es dato propio de SIGTI.** [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) declara espejo la *jerarquía* y los *niveles de autorización*, pero **no dice nada de la delegación territorial**, que es un eje que ARGOS puede no tener. Mientras no se resuelva, la delegación se modela como dato propio de SIGTI — insumo **#16**
- `[C]` Mapa real de delegaciones de la institución y su dotación — insumo **#27**
- `[I]` Que la estructura deba tener vigencia por rango de fechas es derivación de [`RNF-05`](../no-funcionales/RNF-05-temporalidad-normativa.md) y [`RNF-06`](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md), no articulado normativo
- **Regla candidata:** *La estructura organizativa tiene vigencia por rango de fechas; ninguna unidad se elimina y ningún expediente cambia de unidad de origen por una reorganización posterior.* Ninguna de las 97 reglas lo enuncia — `RN-39` cubre parámetros normativos, no la estructura
