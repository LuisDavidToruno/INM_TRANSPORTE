# RN-08 — Una Orden de Misión solo se cierra con su cadena de trazabilidad completa; incompleta, se cierra con hallazgo

| Campo | Valor |
|---|---|
| **Módulos** | M-13, M-14 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — vínculo en cadena trazable |
| **Verificación** | `[V]` |
| **Tipo** | Bloqueo duro con salida por `CERRADA_CON_HALLAZGO` |
| **Configurable** | Sí — parámetro `eslabones_exigidos_para_cierre`, con mínimo no desactivable |

## Enunciado

Para pasar a `CERRADA`, una Orden de Misión **debe** tener presentes y vinculados entre sí todos los eslabones aplicables de su cadena:

`solicitud → autorización → orden de misión → asignación de vehículo y motorista → bitácora con odómetro de salida y retorno → asignación y consumo de combustible → registro de peajes (si la ruta los atraviesa) → liquidación`

Si falta cualquier eslabón aplicable, el sistema **no debe** permitir `CERRADA`, pero **sí debe** permitir `CERRADA_CON_HALLAZGO`, identificando el eslabón faltante, el motivo y el responsable de la omisión.

Los eslabones **no aplicables** — combustible en una misión sin consumo, peajes en ruta sin casetas — se marcan como tales con fundamento, no se dan por cumplidos.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) exige vincular en cadena trazable cada eslabón *"con su documento y su firmante"*. El auditor del TSC no pide comprobantes sueltos: pide poder recorrer la cadena de una punta a la otra sobre un expediente concreto.

La salida por `CERRADA_CON_HALLAZGO` no es una debilidad de la regla, es lo que la hace funcionar. Un sistema que no permite cerrar expedientes imperfectos acumula expedientes abiertos que nadie mira, y los hallazgos quedan invisibles. Es preferible cerrar señalando la falta.

## Condiciones de aplicación

Aplica al cierre de toda Orden de Misión, en cualquier dependencia o delegación.

**No aplica** a órdenes en estado `ANULADA` antes de `DESPACHADA`: no hubo ejecución que trazar. Sí aplica a las canceladas en ruta, que sí consumieron recursos.

## Comportamiento esperado

1. El sistema presenta al liquidador una **lista de verificación de la cadena**, eslabón por eslabón, con su estado: presente, ausente, o no aplicable con fundamento.
2. El cierre con hallazgo exige **tipificar el hallazgo** de un catálogo configurable — falta de comprobante, odómetro inconsistente, peaje sin ticket, ausencia de autorización previa — y consignar responsable.
3. Todo cierre con hallazgo **notifica a ACT-12 Auditor Interno** y alimenta el reporte de hallazgos por vehículo, motorista, dependencia y período.
4. El expediente cerrado, con o sin hallazgo, es **exportable como paquete de evidencia**: índice, documentos, adjuntos y la cadena representada explícitamente ([NRM-01](../normativa/NRM-01-control-interno-tsc.md)).
5. Un vehículo o un motorista con hallazgos acumulados por encima de un umbral configurable genera **alerta al Jefe de Transporte**, no bloqueo automático.

## Casos límite

- **Peaje pagado sin ticket.** [NRM-10](../normativa/NRM-10-peajes.md) es explícita: *advertir cuando falte sin bloquear el cierre*. Se cierra con hallazgo tipificado, nunca se bloquea. Bloquear por esto hace que el sistema se abandone.
- **Misión ejecutada sin autorización previa.** Eslabón ausente y no subsanable: no se fabrica autorización retroactiva. Cierre con hallazgo y notificación obligatoria.
- **Consumo de combustible con comprobante ilegible.** El comprobante existe pero no prueba. Se registra como presente-con-observación y se decide en la liquidación si constituye hallazgo. `[C]` confirmar criterio con Auditoría Interna.
- **Misión larga cuyos registros de campo aún no han sincronizado.** No se cierra con hallazgo por falta de datos que están en camino. El sistema distingue *ausente* de *pendiente de sincronización* y bloquea el cierre mientras haya dispositivos con datos pendientes de esa orden ([RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)).
- **Misión de cortesía sin combustible ni peaje** — traslado corto dentro de la ciudad con el tanque ya cargado. El eslabón de combustible se marca *no aplicable* con fundamento; lo que no se admite es cerrarlo como *presente* con consumo cero.
- **Cierre masivo de expedientes antiguos** al poner en marcha el sistema. La carga inicial histórica no se cierra por esta regla: se marca como **expediente migrado** con el alcance de datos disponible, y se excluye de los indicadores de hallazgo. `[C]` confirmar si habrá migración histórica.

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-06](RN-06-transiciones-de-estado-de-la-orden.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)
- Actores: ACT-04, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
