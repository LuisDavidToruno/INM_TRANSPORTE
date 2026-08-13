# RN-81 — SIGTI expone a ARGOS los hechos con la clave de vinculación de la Orden de Misión, y no escribe en el sistema origen

| Campo | Valor |
|---|---|
| **Módulos** | M-20, M-13, M-09, M-18 |
| **Origen** | Casos especiales [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md), [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md), [CE-20](../../02-requisitos/casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) · [DP-001 D-01 y D-05](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) · [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) |
| **Verificación** | `[I]` la frontera entre sistemas es decisión de producto propia, no norma. `[C]` los contratos de API de ARGOS — insumos #16 y #17 |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — periodicidad y formato de los reportes de exposición |

## Enunciado

SIGTI **debe** exponer a ARGOS, con la **clave de vinculación de la Orden de Misión**, los hechos que ARGOS necesita para resolver lo suyo:

| Hecho | Por qué lo necesita ARGOS |
|---|---|
| **Ventana efectiva** de la misión — inicio y fin reales, con retorno anticipado o prórroga | Ajuste del viático |
| **Grado de cumplimiento y causa** ([`RN-78`](RN-78-grado-de-cumplimiento-del-objeto.md)) | Justificación del gasto |
| **Compromisos liberados por anulación** de misión o de asignación | Evitar el descuadre en SIAFI |
| **Compromiso y ejecución** de combustible y peajes por período y estructura presupuestaria | Conciliación de cuota trimestral ([`RN-54`](RN-54-cuota-trimestral-de-compromiso.md)) |

SIGTI **no debe** escribir en ARGOS ni en Talento Humano ([`RN-48`](RN-48-datos-espejo-de-solo-lectura.md)), **ni calcular, estimar o mostrar el viático**.

La exposición **debe** ser reproducible por período con **fecha de corte de conocimiento** ([`RN-94`](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)).

## Justificación

[`RN-48`](RN-48-datos-espejo-de-solo-lectura.md) prohíbe que SIGTI escriba en ARGOS, y hace bien. Pero de esa prohibición no se sigue que SIGTI pueda **callar**: si SIGTI anula un compromiso de combustible y no lo reporta, el descuadre aparece en SIAFI y nadie sabe de dónde vino. Es exactamente el patrón que originó [`RN-54`](RN-54-cuota-trimestral-de-compromiso.md): un control que existe en un sistema y no llega al otro.

Con la ventana efectiva pasa lo mismo del otro lado. Dos noches menos de pernocta significan un ajuste de viático que SIGTI **no debe** calcular ([DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)) pero que ARGOS no puede hacer si nadie le dice que la misión retornó el lunes en vez del miércoles.

La regla dibuja la frontera de forma operativa: **SIGTI es dueño del hecho, ARGOS es dueño del efecto económico del hecho sobre el viático y el presupuesto.**

## Condiciones de aplicación

Aplica a toda Orden de Misión con vinculación a ARGOS.

**No aplica** a las misiones sin efecto en viático ni en compromiso presupuestario, que se exponen igual pero sin renglones económicos.

**No aplica** a Talento Humano, cuya relación es de consumo de espejo en un solo sentido ([`RN-48`](RN-48-datos-espejo-de-solo-lectura.md), [`RN-49`](RN-49-reconciliacion-periodica-del-espejo.md)).

## Comportamiento esperado

1. La clave de vinculación se establece al crear la Orden de Misión y **no cambia** durante todo su ciclo, ni siquiera al sustituir vehículo o motorista.
2. Cada hecho expuesto lleva: clave de vinculación, tipo de hecho, **fecha del hecho**, fecha de captura y valores. La exposición no reinterpreta: entrega el hecho.
3. La exposición ocurre por **reporte por período** y, cuando los contratos de API lo permitan, por **notificación**. `[C]` insumos #16 y #17: sin contrato conocido, el mecanismo inicial es el reporte con formato acordado.
4. El **reporte de compromisos liberados por anulación** se produce por período para la conciliación con ARGOS y SIAFI, con el detalle por Orden de Misión y por objeto del gasto.
5. Si la exposición falla o queda pendiente, el hecho **no se pierde**: se encola y se reporta la antigüedad de la cola ([`RN-50`](RN-50-degradacion-por-sincronizacion-detenida.md)).
6. **Ninguna pantalla de SIGTI muestra montos de viático**, ni siquiera informativos. La frontera se defiende en la interfaz o no se defiende.

## Casos límite

- **Misión que se extiende después de que el viático ya se pagó.** SIGTI expone la ventana nueva con su fecha de hecho; qué hace ARGOS con eso es de ARGOS. SIGTI no calcula la diferencia ni la insinúa.
- **Anulación de una misión cuyo compromiso ya se ejecutó parcialmente.** Se expone el compromiso liberado **neto**, con el detalle de lo ejecutado, no el bruto. El detalle es lo que permite conciliar.
- **ARGOS fuera de servicio durante días.** La misión se opera igual, la exposición se encola y la degradación se declara explícitamente antes de operar ([`RN-50`](RN-50-degradacion-por-sincronizacion-detenida.md)).
- **Institución sin ARGOS.** El sistema es genérico y se despliega en instituciones distintas: la exposición se configura como inactiva y los reportes quedan disponibles para su uso manual. La regla no presupone la existencia del otro sistema, presupone la del hecho.
- **Corrección posterior de un hecho ya expuesto.** Se expone la corrección como hecho nuevo con referencia al anterior ([`RN-42`](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)), nunca reemplazando la exposición previa.

## Trazabilidad

- Decisiones: [DP-001 D-01, D-05, D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) · [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- Reglas relacionadas: [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md), [RN-54](RN-54-cuota-trimestral-de-compromiso.md), [RN-77](RN-77-versionado-del-alcance-autorizado.md), [RN-78](RN-78-grado-de-cumplimiento-del-objeto.md), [RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md), [RN-96](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)
- Casos especiales: [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) `RN-c:notificacion-de-cambio-de-ventana-a-argos` · [CE-20](../../02-requisitos/casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) `RN-C20b` · [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md)
- Insumos pendientes: #16 y #17 contratos de API de ARGOS y Talento Humano
