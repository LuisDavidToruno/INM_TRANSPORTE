# RN-40 — Todo cálculo usa el parámetro vigente a la fecha del hecho, no a la fecha de captura ni a la de consulta

| Campo | Valor |
|---|---|
| **Módulos** | M-02, M-18, M-09, M-13, M-07 |
| **Origen** | Premisa rectora 6 de `CLAUDE.md`; normas [NRM-10](../normativa/NRM-10-peajes.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` |
| **Tipo** | Cálculo |
| **Configurable** | No |

## Enunciado

Toda resolución de un parámetro con vigencia — tarifa de peaje, matriz licencia ↔ vehículo, calendario de días hábiles, umbral, plazo, categoría — **debe** hacerse contra la **fecha del hecho** al que se aplica.

La fecha del hecho es:

| Cálculo | Fecha del hecho |
|---|---|
| Tarifa de un paso por caseta | Fecha y hora del paso |
| Estimación de peajes previa | Fecha prevista de cada cruce |
| Habilitación licencia ↔ vehículo | Cada fecha del rango de la misión |
| Día u hora inhábil | Fecha y hora de circulación |
| Plazo de liquidación | Fecha de retorno |
| Rendimiento esperado | Fecha de la misión |

**Nunca** se usa la fecha de captura, la de sincronización, la de aprobación ni la de consulta.

Cuando un cálculo abarca varios días y el parámetro cambia en medio, **cada día se valora con el parámetro vigente ese día**, y el desglose se muestra al usuario, no solo el total.

## Justificación

Premisa rectora 6: *"Todo cálculo usa la tabla vigente a la fecha del hecho, no a la fecha de captura."*

En Honduras esto no es una sutileza. [NRM-10](../normativa/NRM-10-peajes.md) documenta que en 2026 hubo un aumento anunciado, suspendido, prorrogado, reanunciado y finalmente revertido, con propuesta de aplicación retroactiva. Un cálculo que use la fecha de captura devolvería resultados distintos según el día en que alguien digitó el ticket.

Y con la operación desconectada de [NRM-09](../normativa/NRM-09-realidad-operativa.md), la distancia entre el hecho y su captura puede ser de días o semanas: un registro digitado desde papel de una misión de hace un mes debe valorarse con las reglas de ese mes.

## Condiciones de aplicación

Aplica a todos los cálculos y validaciones que consuman parámetros con vigencia.

Aplica también a las **consultas históricas**: reabrir un expediente de hace dos años debe mostrar los valores de entonces, no un recálculo con los parámetros de hoy ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md) lo garantiza para los valores ya congelados).

## Comportamiento esperado

1. Toda función de cálculo recibe la fecha del hecho como argumento explícito. No existe resolución de parámetro "con la fecha actual" implícita.
2. El resultado informa **qué versión del parámetro se usó**, con su vigencia. Un cálculo que no puede decir con qué regla se hizo es indefendible ante auditoría.
3. Si no hay parámetro vigente a esa fecha, **bloquea** con mensaje accionable ([RN-39](RN-39-parametros-normativos-con-vigencia.md)); no aplica el más cercano ni el último conocido.
4. Los cálculos que abarcan un rango presentan **desglose por día o por evento**, con el parámetro aplicado en cada uno.
5. Las validaciones de habilitación se evalúan sobre **todo el rango** ([RN-10](RN-10-licencia-vigente-en-todo-el-rango.md)), no sobre una fecha representativa.

## Casos límite

- **Fecha del hecho desconocida o imprecisa.** Ocurre con digitación diferida desde papel mal llenado. El sistema exige la fecha del hecho como campo obligatorio ([RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md)); si el capturador solo puede aportar un rango, se registra el rango y el cálculo se marca **no concluyente**, en lugar de tomar un extremo en silencio.
- **Cambio de parámetro exactamente a medianoche del día del hecho.** El límite de la vigencia debe ser inequívoco: se define con precisión de fecha y hora, y la comparación es cerrada al inicio y abierta al final. Dejarlo ambiguo produce diferencias de un cruce de peaje que nadie logra explicar.
- **Estimación previa contra realidad posterior.** El estimado se calcula a la fecha *prevista* y el pago a la fecha *real*. Si la tarifa cambió en medio, ambos son correctos y difieren: la conciliación lo tipifica como *cambio de tarifa entre aprobación y ejecución* ([RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)).
- **Misión que cruza el cambio de vigencia de la matriz de licencias.** La habilitación se evalúa para cada día del rango. Si el motorista queda no habilitado a partir del día tres, se bloquea la misión completa: no existe la misión parcialmente habilitada.
- **Zona horaria y hora local.** Todos los hechos se registran con referencia horaria explícita. Un dispositivo de campo mal configurado puede desplazar un hecho de día, cambiando el parámetro aplicable y la calificación de día hábil. Se mitiga registrando ambas marcas de tiempo ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)).
- **Recálculo de un reporte histórico.** Debe reproducir exactamente el resultado original. Un reporte de 2024 que hoy da otro número es señal de que esta regla no se cumplió en alguna parte, y es la prueba más simple de verificarla.

## Trazabilidad

- Normas: [NRM-10](../normativa/NRM-10-peajes.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Premisa rectora 6 de [CLAUDE.md](../../../CLAUDE.md)
- Reglas relacionadas: [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- Actores: ACT-01, ACT-04, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
