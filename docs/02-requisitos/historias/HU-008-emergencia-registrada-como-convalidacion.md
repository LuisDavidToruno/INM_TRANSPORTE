# HU-008 — Registrar la salida de emergencia como convalidación posterior

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-02 Solicitante; típicamente ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por insumo #32 (quién convalida y en qué plazo) |

## Historia

**Como** Encargado de Delegación
**quiero** registrar una salida que ya ocurrió por emergencia, con su causal clasificada y la fecha real de salida en el pasado, presentada como **convalidación posterior y no como autorización previa**
**para** que el expediente refleje lo que efectivamente pasó, en lugar de obligarme a inventar una cronología que cuadre con el sistema

## Contexto

La emergencia existe: un traslado de un compañero accidentado a las once de la noche no espera a que el sistema tenga autorizador disponible. Lo que decide si el sistema sirve es qué hace **al día siguiente**.

Un sistema que solo admite autorización previa obliga al usuario a **antedatar**. Y un expediente con fechas ajustadas para que "cuadre" es peor que no tener expediente: es un documento falso firmado por un servidor público.

Por eso el registro se rotula, en pantalla y en el papel, como **convalidación posterior**. La cronología se declara tal como ocurrió. La emergencia **no levanta el núcleo irreductible de incompatibilidades** (`I-07`, `I-10`, `I-11`), y el sistema **mide la frecuencia de esta vía por dependencia**: si la emergencia se vuelve la forma normal de saltarse a la jefatura, el control desapareció.

## Reglas que la gobiernan

- [RN-73](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) — El acto ejecutado sin autorización previa se convalida en plazo, con la cronología declarada tal como ocurrió
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho y fecha de captura son campos distintos, ambos obligatorios
- [RN-59](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md) — Todo uso se ampara en Orden de Misión, incluida la salida de emergencia
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La emergencia no levanta la segregación de funciones
- [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) — Sin cadena completa, el expediente cierra con hallazgo, nunca en silencio
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Nada se borra; la vía de emergencia queda registrada aunque después se rechace

## Casos especiales que la afectan

- [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) — Salida de emergencia sin autorización previa, convalidada después

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro de la salida de emergencia para convalidación
  Como Encargado de Delegación
  quiero registrar una salida ya ocurrida con su cronología real
  para que el expediente no exija una fecha falsa

  Antecedentes:
    Dado un catálogo de causales de emergencia con las entradas "Traslado de personal lesionado", "Atención de desastre" y "Requerimiento de autoridad superior"
    Y una fecha del sistema del "2026-03-14 08:00"

  Escenario: Se rechaza la marca de emergencia sin causal del catálogo
    Dado un borrador con salida declarada el "2026-03-13 23:10"
    Cuando el Encargado de Delegación marca la solicitud como emergencia sin seleccionar causal
    Entonces el sistema rechaza la marca
    Y muestra "Seleccione la causal de emergencia del catálogo y describa el motivo. Una emergencia sin causal clasificada no se convalida."

  Escenario: Se rechaza ajustar la fecha de salida a una posterior a la captura
    Dado un expediente marcado como emergencia con salida real el "2026-03-13 23:10"
    Cuando el Encargado de Delegación intenta cambiar la fecha de salida a "2026-03-14 09:00"
    Entonces el sistema rechaza el cambio
    Y muestra "La salida ocurrió el 13/03/2026 23:10. La cronología se declara tal como ocurrió; no se ajusta ninguna fecha para que el expediente cuadre (RN-73)."

  Escenario: El expediente se rotula como convalidación posterior, no como autorización previa
    Dado un borrador marcado como emergencia con causal "Traslado de personal lesionado"
    Y una salida real declarada el "2026-03-13 23:10"
    Cuando el Encargado de Delegación envía la solicitud
    Entonces el sistema ejecuta el envío
    Y el expediente se rotula "Convalidación posterior de acto sin autorización previa"
    Y ese rótulo aparece también en la versión impresa del expediente

  Escenario: La emergencia no levanta la segregación de funciones
    Dado un expediente de emergencia cuyo solicitante de derecho es "Rolando Discua"
    Cuando "Rolando Discua" intenta convalidar su propio expediente
    Entonces el sistema no ejecuta la convalidación
    Y muestra "Quien solicita no convalida. La emergencia no levanta la segregación de funciones (RN-01). El expediente escala al nivel inmediato superior."

  Escenario: La misión no cierra mientras la convalidación esté pendiente
    Dado un expediente de emergencia en estado "RETORNADA" sin convalidación registrada
    Cuando se intenta cerrar la misión
    Entonces el sistema no ejecuta el cierre
    Y muestra "La misión se ejecutó sin autorización previa y la convalidación sigue pendiente. No puede cerrarse (PC-18)."

  Escenario: Vencido el plazo, la misión cierra con hallazgo y no en silencio
    Dado un expediente de emergencia con salida real el "2026-03-13 23:10"
    Y un plazo de convalidación configurado en "5" días hábiles
    Y ninguna convalidación registrada al "2026-03-23"
    Cuando el sistema evalúa el vencimiento del plazo
    Entonces la misión queda en estado "CERRADA_CON_HALLAZGO"
    Y notifica a Gerencia Administrativa y a Auditoría Interna
    Y el hallazgo indica "Acto ejecutado sin autorización previa, no convalidado dentro del plazo de 5 días hábiles."

  Escenario: La frecuencia del uso de la vía de emergencia se expone por dependencia
    Dada la dependencia "Subgerencia de Operaciones" con "9" expedientes marcados como emergencia en el trimestre
    Y un total de "20" expedientes de esa dependencia en el mismo trimestre
    Cuando Auditoría Interna consulta el reporte de control interno del trimestre
    Entonces el reporte muestra la dependencia con "45" por ciento de misiones por vía de emergencia
    Y ordena las dependencias por ese porcentaje de mayor a menor
```

## Fuera de alcance

- El acto de **convalidar** en sí: quién lo ejecuta y con qué facultades queda pendiente del insumo #32. Esta historia produce el expediente convalidable y el bloqueo del cierre, no el acto
- La captura sin conectividad, que es el escenario más frecuente de la emergencia — es [HU-007](HU-007-captura-sin-conectividad-y-digitacion-diferida.md)
- Los hallazgos de ejecución posteriores (consumo, kilometraje, peajes) — son de M-13 y M-14

## Notas y pendientes

- `[C]` **Qué puesto convalida un acto de emergencia y en qué plazo máximo** — insumo #32. **Es el dato que bloquea esta historia**: aquí el `[C]` *es* la lógica, no un parámetro. Por la [DoR](../../plantillas/definition-of-ready.md), la historia no entra a sprint sin él. El plazo de "5 días hábiles" de los criterios es **ejemplo**, no valor adoptado
- `[C]` Contenido del catálogo de causales de emergencia — insumo #1
- `[P]` La exigencia de autorización por servidor competente proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), verificada parcialmente
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) excepción E4; [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) excepción E6; punto de control `PC-18`
