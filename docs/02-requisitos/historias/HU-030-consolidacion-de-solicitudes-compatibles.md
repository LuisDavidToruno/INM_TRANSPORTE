# HU-030 — Consolidar varias solicitudes en una sola Orden de Misión sin fusionar sus expedientes

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 2 y A1 · `T-08` · `INV-13` |

## Historia

**Como** Jefe de Transporte
**quiero** atender con una sola Orden de Misión varias solicitudes aprobadas al mismo destino y en la misma ventana, conservando el expediente y la autorización de cada una
**para** producir el ahorro real de la flota sin perder la trazabilidad por dependencia que la liquidación y la auditoría necesitan

## Contexto

Tres unidades pidiendo el mismo día un vehículo a Comayagua es la escena diaria. Hoy se resuelve con una llamada, y el resultado es que la misión queda registrada a nombre de una sola dependencia: las otras dos "se fueron de colados" y su costo no aparece en ningún reporte.

**Consolidar no fusiona.** Cada solicitud conserva su expediente, su autorización y su dependencia. Una actúa como expediente rector y las demás quedan vinculadas; desde `PROGRAMADA` en adelante todas siguen las transiciones del rector, con **una sola excepción: la anulación**, porque una dependencia puede desistir sin que la misión entera se caiga.

El escalamiento por segregación se evalúa **por cada solicitud componente**: basta un conflicto para escalar la orden completa. Si la jefa que autorizó una de las solicitudes es también la solicitante de otra, la orden consolidada escala.

## Reglas que la gobiernan

- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — Cuando el autorizador natural es el solicitante, la autorización escala al nivel inmediato superior
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La segregación se evalúa sobre cada solicitud componente, no solo sobre la rectora
- [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) · [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) · [RN-68](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md) — La suma de los objetos consolidados debe caber, tramo por tramo
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — La consolidación lograda es indicador de calidad de la programación
- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — La vinculación y el desistimiento son transiciones registradas

## Casos especiales que la afectan

- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — Consolidar es el **primer** camino que se ofrece ante el conflicto por el recurso
- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Consolidar personas de una solicitud con carga de otra activa la matriz objeto × objeto

## Criterios de aceptación

```gherkin
# language: es
Característica: Consolidación de solicitudes compatibles bajo una sola Orden de Misión

  Antecedentes:
    Dado un vehículo "Microbús Toyota Coaster" con correlativo "INS-B-003" de "26" plazas
      y "300" kg de capacidad de carga
    Y una solicitud "SOL-2026-0370" de la Gerencia de Operaciones, aprobada,
      destino "Comayagua", ventana del "2026-09-15 06:00" al "2026-09-15 18:00", "6" ocupantes
    Y una solicitud "SOL-2026-0371" de la Unidad de Bienes, aprobada,
      destino "Comayagua", ventana del "2026-09-15 07:00" al "2026-09-15 17:00", "4" ocupantes

  Escenario: Se rechaza consolidar cuando la suma excede la capacidad en un tramo
    Dada una solicitud "SOL-2026-0372" de la Unidad de Informática, aprobada,
      destino "Comayagua", misma ventana, "18" ocupantes
    Cuando el Jefe de Transporte intenta consolidar "SOL-2026-0370", "SOL-2026-0371" y "SOL-2026-0372"
      en el "INS-B-003"
    Entonces el sistema rechaza la consolidación
    Y muestra "La consolidación suma 28 ocupantes en el tramo Tegucigalpa–Comayagua y el vehículo INS-B-003 tiene 26 plazas."

  Escenario: Se rechaza consolidar solicitudes con objetos incompatibles
    Dada una solicitud "SOL-2026-0373" de la Unidad de Almacén, aprobada, mismo destino y ventana,
      con objeto del traslado "8 bidones de combustible de 5 galones"
    Y una entrada de la matriz que declara "personas" y "combustible envasado"
      incompatibles en compartimiento común
    Cuando el Jefe de Transporte intenta consolidar "SOL-2026-0370" y "SOL-2026-0373" en el "INS-B-003"
    Entonces el sistema rechaza la consolidación
    Y muestra "Personas y combustible envasado no pueden viajar en compartimiento común: las solicitudes SOL-2026-0370 y SOL-2026-0373 no son consolidables en el vehículo INS-B-003."

  Escenario: La consolidación escala la autorización por conflicto en una solicitud componente
    Dado que la jefatura que autorizó "SOL-2026-0371" es la solicitante de "SOL-2026-0370"
    Cuando el Jefe de Transporte consolida "SOL-2026-0370" y "SOL-2026-0371"
    Entonces el sistema exige la autorización del nivel inmediato superior para la orden consolidada
    Y muestra "La solicitud SOL-2026-0370 fue solicitada por quien autorizó SOL-2026-0371. La orden consolidada escala al nivel inmediato superior."
    Y el escalamiento se imprimirá en la Orden de Misión

  Escenario: Se consolida conservando cada expediente y su autorización
    Cuando el Jefe de Transporte consolida "SOL-2026-0370" y "SOL-2026-0371" en el "INS-B-003"
      declarando "SOL-2026-0370" como expediente rector
    Entonces se crea una sola Orden de Misión con un solo folio reservado
    Y "SOL-2026-0371" queda vinculada al expediente rector
    Y cada solicitud conserva su dependencia solicitante y su autorización original
    Y la Orden de Misión listará las dos solicitudes vinculadas al imprimirse

  Escenario: Una dependencia desiste sin que caiga la misión
    Dada una Orden de Misión consolidada con "SOL-2026-0370" rectora y "SOL-2026-0371" vinculada
    Cuando la Unidad de Bienes desiste de "SOL-2026-0371" antes del despacho
    Entonces "SOL-2026-0371" pasa al estado "ANULADA" con motivo "desistimiento de la dependencia"
    Y la Orden de Misión permanece en "PROGRAMADA"
    Y el objeto del traslado de la misión se reduce a los "6" ocupantes de "SOL-2026-0370"
    Y el hecho queda registrado en el diario de la misión

  Escenario: El rector no se puede anular dejando huérfanas a las vinculadas
    Dada una Orden de Misión consolidada con "SOL-2026-0370" rectora y "SOL-2026-0371" vinculada
    Cuando el Jefe de Transporte intenta anular únicamente el expediente rector "SOL-2026-0370"
    Entonces el sistema rechaza la anulación
    Y muestra "SOL-2026-0370 es el expediente rector de una misión consolidada. Desvincule SOL-2026-0371 o anule la Orden de Misión completa."
```

## Fuera de alcance

- La **atribución de costo por solicitud vinculada** en la liquidación — depende del criterio de prorrateo, que no está definido
- La detección automática de oportunidades de consolidación: esta historia entrega la consolidación **manual asistida**, con las candidatas listadas por destino y ventana
- La consolidación con más de un vehículo simultáneo (convoy) — no está admitida en el modelo

## Notas y pendientes

- `[C]` **Criterio de prorrateo del costo entre solicitudes consolidadas** — insumo #1. Sin él, la liquidación consolidada reporta el costo total de la misión y **no** lo reparte: repartir con un criterio inventado produce un reporte por dependencia que nadie puede defender.
- `[C]` Si se admite **más de un vehículo simultáneo bajo una misma Orden de Misión** — insumo #62.
