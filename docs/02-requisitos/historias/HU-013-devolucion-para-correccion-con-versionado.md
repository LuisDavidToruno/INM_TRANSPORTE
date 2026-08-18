# HU-013 — Devolver la solicitud para corrección sin rechazarla

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-03 Jefatura Inmediata |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Jefatura Inmediata
**quiero** devolver la solicitud a borrador con un motivo obligatorio y visible, conservando el mismo número de expediente y creando una versión nueva
**para** que una observación menor no me obligue a rechazar, porque un histórico lleno de rechazos por errores de digitación esconde los rechazos que sí importan

## Contexto

En papel, la jefatura devuelve la hoja con una anotación al margen y el solicitante la corrige. Si el sistema solo ofreciera aprobar o rechazar, todo error de dedo terminaría como rechazo — y el indicador de rechazos, que debería señalar solicitudes improcedentes, pasaría a medir la calidad de la digitación.

La devolución **conserva el número de expediente**: es el mismo expediente en su versión 2, no uno nuevo. La versión anterior se conserva íntegra. Y los estimados congelados se anulan, porque al reenviar el contenido puede haber cambiado y con él el cálculo.

## Reglas que la gobiernan

- [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — `T-04` devuelve de `SOLICITADA` a `BORRADOR` con actor, rol, momento y motivo
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La versión anterior no se borra: se conserva íntegra
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — Los estimados congelados se anulan y se recalculan al reenviar
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — La devolución se registra con identidad, rol ejercido y momento
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — El número de expediente no se recicla ni se duplica: la versión 2 conserva el mismo número

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` toca este flujo. Constancia dejada

## Criterios de aceptación

```gherkin
# language: es
Característica: Devolución de la solicitud para corrección
  Como Jefatura Inmediata
  quiero devolver con motivo en lugar de rechazar
  para que el histórico de rechazos siga significando algo

  Antecedentes:
    Dado un expediente "CHO-2026-00087" en estado "SOLICITADA", versión "1"
    Y un estimado de peajes congelado de "L 150.00" con la tabla "TAR-2026-01"
    Y una Jefatura Inmediata "Rolando Discua" con competencia sobre el expediente

  Escenario: Se rechaza devolver sin motivo
    Cuando "Rolando Discua" intenta devolver el expediente sin escribir el motivo
    Entonces el sistema no ejecuta la devolución
    Y muestra "Escriba el motivo de la devolución. El solicitante debe saber qué corregir."
    Y el expediente permanece en estado "SOLICITADA"

  Escenario: Se advierte antes de invalidar autorizaciones parciales ya registradas
    Dada una autorización de nivel 1 ya registrada sobre el expediente
    Cuando "Rolando Discua" solicita devolver el expediente con motivo "Peso de la carga mal consignado"
    Entonces el sistema advierte "Devolver invalidará 1 autorización de nivel ya registrada. ¿Confirma?"
    Y no ejecuta la devolución hasta la confirmación

  Escenario: La devolución conserva el número y crea la versión 2
    Cuando "Rolando Discua" devuelve el expediente con motivo "El destino consigna Comayagua y el detalle dice Siguatepeque"
    Entonces el expediente vuelve a estado "BORRADOR"
    Y conserva el número "CHO-2026-00087"
    Y su versión pasa a "2"
    Y la versión "1" se conserva íntegra y consultable

  Escenario: El motivo es visible para el solicitante
    Cuando "Rolando Discua" devuelve el expediente con motivo "El destino consigna Comayagua y el detalle dice Siguatepeque"
    Entonces el Solicitante ve en su expediente "Devuelto por Rolando Discua el 14/03/2026: El destino consigna Comayagua y el detalle dice Siguatepeque."

  Escenario: Los estimados congelados se anulan al devolver
    Cuando "Rolando Discua" devuelve el expediente con motivo "Corrija el tipo de vehículo requerido"
    Entonces el estimado de peajes congelado de "L 150.00" queda anulado
    Y el expediente no muestra estimado congelado mientras esté en "BORRADOR"

  Escenario: El reenvío recalcula el estimado con la tabla vigente
    Dado un expediente devuelto, en versión "2", con un destino corregido
    Y una tabla de tarifas vigente "TAR-2026-02"
    Cuando el Solicitante reenvía la solicitud a autorización
    Entonces el sistema recalcula el estimado de peajes
    Y lo congela con el identificador "TAR-2026-02"
    Y el expediente conserva el número "CHO-2026-00087" en versión "2"
```

## Fuera de alcance

- El **rechazo** como decisión de fondo — es [HU-014](HU-014-rechazo-con-motivo-y-solicitud-vinculada.md)
- La corrección del contenido por el solicitante: vuelve al flujo de [HU-001](HU-001-registrar-solicitud-declarando-el-objeto-del-traslado.md) y [HU-004](HU-004-envio-a-autorizacion-con-numero-de-expediente-y-congelamiento.md)
- La anulación administrativa del expediente por Gerencia Administrativa (`T-07`) — es historia aparte del backlog de M-06
- El límite de cuántas veces puede devolverse un mismo expediente: no se define aquí

## Notas y pendientes

- `[C]` Si la institución quiere un **catálogo de motivos de devolución** además del texto libre — insumo #1. Por ahora el motivo es texto libre obligatorio
- `[I]` Que un histórico de rechazos contaminado por errores de digitación pierde valor de control es criterio de análisis, no exigencia normativa
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) flujo alterno A3; [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) flujo alterno A2; transición `T-04`
