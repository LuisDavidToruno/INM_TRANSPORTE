# RN-29 — La liquidación concilia asignado contra consumido contra saldo devuelto, y la diferencia debe quedar explicada

| Campo | Valor |
|---|---|
| **Módulos** | M-13, M-09 |
| **Origen** | `PROP-01` de [insumos-pendientes](../../07-gestion/insumos-pendientes.md); norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-14 |
| **Verificación** | `[V]` la exigencia de conciliación periódica de registros |
| **Tipo** | Cálculo + bloqueo duro |
| **Configurable** | Sí — `tolerancia_diferencia_liquidacion`, con valor inicial cero |

## Enunciado

Al liquidar una Orden de Misión, el sistema **debe** verificar la identidad:

```
monto asignado = monto consumido comprobado + saldo devuelto + diferencia explicada
```

Si `diferencia explicada` es distinta de cero, **debe** tener motivo tipificado, monto, responsable y respaldo. Si queda diferencia **sin explicar** por encima de la tolerancia configurada, la orden **no debe** poder pasar a `LIQUIDADA`: solo a `CERRADA_CON_HALLAZGO`.

La misma identidad se verifica en **galones** cuando la asignación se hizo en galones y no en dinero.

## Justificación

`PROP-01`, paso 5: *"al cerrar la misión se concilian: monto asignado vs. monto consumido vs. comprobantes vs. saldo devuelto"*. TSC-NOGECI V-14 exige **Conciliación Periódica de Registros** — bitácoras contra vales contra facturas del proveedor.

Una liquidación que solo suma comprobantes no concilia nada. La pregunta que responde esta regla es: del dinero que salió del fondo, ¿cuánto volvió como combustible comprobado y cuánto volvió como efectivo? Lo que no encaja en ninguna de las dos categorías es, por definición, lo que hay que explicar.

## Condiciones de aplicación

Aplica a toda Orden de Misión que tuvo asignación de combustible, incluidas las **canceladas en ruta**, que consumieron parcialmente.

Aplica también al **cierre del fondo** ([RN-26](RN-26-fondo-de-combustible-aprobado.md)), donde la identidad se verifica agregada por período.

`[C]` `PROP-01` deja abierto qué ocurre con el sobrante: se devuelve o se arrastra. El diseño soporta ambos; el parámetro decide.

## Comportamiento esperado

1. La liquidación presenta el cuadre desglosado por asignación: folio, monto asignado, consumos con su comprobante, devolución constatada y diferencia.
2. La devolución de saldo **exige constancia de recepción** del receptor, con la segregación de [RN-01](RN-01-segregacion-de-funciones.md): quien liquida no puede ser quien recibió ni quien entregó.
3. Los motivos tipificados de diferencia son catálogo configurable — por ejemplo: variación de precio en estación, consumo sin comprobante, pérdida o extravío del instrumento, error de captura. Cada motivo define si exige respaldo y si constituye hallazgo.
4. Una diferencia atribuida a **faltante de efectivo** genera automáticamente expediente de deducción de responsabilidad en M-12. `[C]` confirmar el procedimiento con Auditoría Interna.
5. El liquidador (ACT-04 o ACT-08 según configuración) no puede haber sido solicitante, despachador, receptor ni entregador de combustible de esa orden.

## Casos límite

- **Precio del combustible distinto al estimado.** No es una diferencia inexplicada: es una variación de precio, tipificada, con el comprobante como respaldo. El sistema debe distinguirla del faltante.
- **Motorista que devuelve el saldo días después.** La orden queda en `RETORNADA` con liquidación pendiente. `[C]` el plazo de liquidación con la institución; vencido, alerta y escalamiento — nunca cierre automático que dé por devuelto lo que no volvió.
- **Consumo mayor que lo asignado, pagado por el motorista de su bolsillo.** Es un reembolso, no un consumo del fondo. Se registra como consumo con instrumento *fondo propio del servidor* y genera obligación de reintegro a favor del motorista. `[C]` confirmar si la institución admite y reembolsa esta figura; es práctica común `[I]` y si no se modela, se registra mal.
- **Misión cancelada con vales ya entregados.** Se liquida lo consumido y se devuelve o anula el resto ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)). La orden anulada **igual se liquida**: hubo movimiento de fondos.
- **Diferencia de centavos por redondeo.** Es la razón por la que existe `tolerancia_diferencia_liquidacion`. Su valor inicial es cero y subirlo es una decisión registrada con fundamento, no un ajuste de conveniencia.
- **Liquidación de una misión cuyos consumos aún no sincronizan.** No se liquida con datos incompletos: el sistema distingue *ausente* de *pendiente de sincronización* y bloquea ([RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)).
- **Institución que arrastra saldo entre misiones.** La identidad se verifica al cierre del **período del fondo**, no de cada misión; cada misión concilia consumo contra kilometraje ([RN-30](RN-30-conciliacion-galonaje-kilometraje.md)) pero no cuadra caja. El parámetro debe dejar esto explícito para no producir hallazgos falsos.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-14
- Decisión: [DP-001, D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); `PROP-01`
- Reglas relacionadas: [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md)
- Actores: ACT-04, ACT-06, ACT-07, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
