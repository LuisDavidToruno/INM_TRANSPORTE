# RN-35 — El costo de peajes se estima desglosado por punto antes de aprobar la solicitud

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-06, M-07 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md); decisión [DP-001 D-02](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[V]` la necesidad de estimar antes de aprobar — `[C]` la tarifa vigente y el régimen de exoneraciones |
| **Tipo** | Cálculo + bloqueo duro sobre la aprobación |
| **Configurable** | Sí — `estimacion_peaje_obligatoria_para_aprobar` |

## Enunciado

Antes de que la Orden de Misión pase a `APROBADA`, el sistema **debe** presentar al autorizador la **estimación de peajes de la ruta**: qué puntos atraviesa, **cuántas veces cada uno** (ida, retorno, paso repetido), categoría aplicada al vehículo, tarifa vigente a la fecha prevista y monto por punto.

La estimación **debe** mostrarse **desglosada por punto**, nunca como un total opaco.

Si la ruta declarada atraviesa puntos de peaje y la estimación no se puede calcular — categoría no resuelta ([RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)) o tarifa no vigente ([RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md)) — el sistema **debe** bloquear la aprobación e indicar el dato faltante.

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md): *"El sistema debe estimar el costo de peajes de la ruta antes de aprobar la solicitud"* y *"presentar el estimado desglosado por punto, no como total opaco. Quien autoriza tiene que poder verificar el cálculo."*

Un viaje Tegucigalpa → San Pedro Sula atraviesa las tres estaciones del Corredor Logístico; ida y vuelta son **6 cruces** `[V]`. Sin desglose, el autorizador no puede distinguir un estimado correcto de uno que duplicó un cruce, y el estimado deja de ser un control para volverse un trámite.

El estimado es además la base de la conciliación posterior ([RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)): sin él no hay contra qué comparar lo pagado.

## Condiciones de aplicación

Aplica a solicitudes cuya ruta declarada atraviesa al menos un punto de peaje activo.

**No aplica** a misiones dentro de zonas sin peajes. [NRM-10](../normativa/NRM-10-peajes.md) `[C]` no encontró evidencia de peajes en CA-1 Sur, CA-4 Occidente ni carreteras departamentales; la ausencia se modela como ausencia de puntos en el catálogo, no como excepción codificada.

## Comportamiento esperado

1. La ruta se declara con origen, destinos y retorno; el sistema resuelve los puntos atravesados contra el catálogo, considerando **sentido de cobro** cuando el punto lo tenga.
2. El desglose muestra por fila: punto, sentido, número de cruces, categoría, tarifa unitaria con su vigencia, y subtotal. Al pie, el total.
3. Un vehículo con exoneración vigente en un punto ([RN-38](RN-38-exoneracion-de-peaje.md)) estima **cero en ese punto**, con el fundamento visible en la misma fila. Un cero sin explicación es indistinguible de un error de cálculo.
4. El estimado se **congela al aprobar** junto con el identificador de la tabla de tarifas usada ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)).
5. Un cambio de ruta, de vehículo o de fechas **invalida el estimado** y exige recalcularlo antes de despachar.

## Casos límite

- **Ruta no declarada con precisión** — "gira por la zona norte". No se puede estimar. El sistema exige al menos los destinos principales; si la institución opera misiones de ruta abierta, esas misiones se marcan como **estimación no aplicable** con fundamento, y su conciliación posterior será por lo pagado, sin comparación. `[C]` confirmar si existen misiones de ruta abierta.
- **Paso repetido por la misma caseta** en una misión multi-destino. El conteo de cruces es lo que más se equivoca al hacerlo a mano, y es la razón del desglose. El sistema debe contar cruces, no puntos distintos.
- **Ruta alternativa que evita el peaje.** Legítima y frecuente. Si el motorista toma otra vía, el estimado no coincidirá con lo pagado: es una desviación tipificada como *ruta distinta a la autorizada*, que puede ser eficiencia o puede ser desvío. La tipificación no juzga; el reporte lo muestra.
- **Frontera con ARGOS.** `[C]` insumo #25: si el peaje se financia con el viático, es de ARGOS y M-18 se solapa. [NRM-10](../normativa/NRM-10-peajes.md) exige **resolver esta frontera antes de escribir historias de M-18**. La regla de estimación sobrevive en cualquier escenario — quien paga cambia, la necesidad de estimar no.
- **Tarifa que cambia entre la aprobación y el viaje.** El estimado congelado se conserva; la diferencia se tipifica como *cambio de tarifa entre aprobación y ejecución* en la conciliación.
- **Vehículo con tag CoviPass institucional.** El estimado no cambia; lo que cambia es el medio de pago y la evidencia. `[C]` insumo #24: si la institución tiene tags y a nombre de quién.

## Trazabilidad

- Norma: [NRM-10 — Peajes](../normativa/NRM-10-peajes.md)
- Decisión: [DP-001, D-02](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-38](RN-38-exoneracion-de-peaje.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)
- Actores: ACT-02, ACT-03, ACT-04, ACT-08
- Historias y casos especiales: pendientes — Bloque 2
