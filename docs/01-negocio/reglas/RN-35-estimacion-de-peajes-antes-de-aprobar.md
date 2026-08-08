# RN-35 — El costo de peajes se estima desglosado por punto antes de aprobar la solicitud

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-06, M-07 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md); decisión [DP-001 D-02](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md). Momento del cálculo y del recálculo: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) `T-02`, `INV-07`, `T-08`, `BD-07`, `T-12` — **artefacto autoridad en transiciones e invariantes** |
| **Verificación** | `[I]` la necesidad de estimar antes de aprobar: es **implicación de requerimiento** escrita por el equipo en [NRM-10](../normativa/NRM-10-peajes.md), no articulado — corregido desde `[V]` por la regla de no escalar el nivel (`HN1-06`). `[V]` que un Tegucigalpa–San Pedro Sula ida y vuelta son 6 cruces por las tres estaciones del Corredor Logístico. `[C]` la tarifa vigente (insumo #21) y el régimen de exoneraciones (insumo #22) |
| **Tipo** | Cálculo. **Bloqueo del despacho**, no de la aprobación |
| **Configurable** | Sí — `estimacion_peaje_obligatoria_para_aprobar`, con valor inicial **apagado** mientras el insumo #21 siga abierto |

## Nota de corrección — hallazgos `HB1-09` y `HN1-10`

> **Qué estaba mal — el mismo error que tenía [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md).** La regla exigía para aprobar la *"categoría aplicada **al vehículo**"* y bloqueaba la aprobación si esa categoría no estaba resuelta. Pero **en `APROBADA` no hay vehículo asignado**: `INV-11` — *"aprobar no es programar"* —, y la asignación es `T-08`, posterior. La regla exigía un dato que en ese momento no puede existir.
>
> **Segundo bloqueo con el mismo defecto.** [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) ordena que el sistema *"arranca sin tarifas cargadas, bloqueando la estimación"* mientras el insumo #21 siga abierto. Con la redacción anterior, eso convertía un bloqueo de la **estimación** en bloqueo de la **aprobación**: el día de la puesta en marcha, **ninguna misión que atraviese la CA-5 se podía aprobar**.
>
> **Qué manda.** La máquina de estados ya reparte el cálculo en tres momentos, y la regla se alinea a ellos: `T-02` calcula y **congela** el estimado con el tipo de vehículo requerido; `T-08` **recalcula** con el vehículo asignado y **exige nueva autorización** si la diferencia supera el umbral; `T-12` bloquea si esa reautorización falta. Ese mecanismo hace innecesario bloquear la aprobación, y es más fino: detecta el cambio de costo en lugar de impedir el trámite.

## Enunciado

**La estimación se calcula dos veces, con dos bases distintas, y ninguna de las dos bloquea la aprobación.**

| Momento | Base de la categoría | Qué produce | Efecto |
|---|---|---|---|
| **Envío a autorización** (`T-02`) | Categoría del **tipo de vehículo requerido** ([RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)) | Estimado previo, desglosado por punto, **congelado** junto con el identificador de la tabla de tarifas usada (`INV-07`, [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)) | Se presenta al autorizador. Es lo que él autoriza |
| **Programación** (`T-08`) | Categoría del **vehículo asignado** | Estimado de despacho | Si difiere del congelado por encima del umbral configurable, **exige nueva autorización**: lo autorizado tenía un costo y ese costo cambió |
| **Despacho** (`T-12`) | — | — | **Bloqueo duro** si la diferencia superó el umbral y la reautorización no existe |

Antes de que la Orden de Misión pase a `APROBADA`, el sistema **debe** presentar al autorizador el estimado: qué puntos atraviesa, **cuántas veces cada uno** (ida, retorno, paso repetido), categoría aplicada **con la base que la determinó**, tarifa vigente a la fecha prevista y monto por punto.

La estimación **debe** mostrarse **desglosada por punto**, nunca como un total opaco.

Si el estimado **no se puede calcular** — no hay tarifa vigente cargada ([RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md)) o el tipo de vehículo requerido no tiene categoría derivada —, la Orden **se puede aprobar igual**, con el estimado marcado de forma visible como **no disponible y su causa**, y esa marca viaja al expediente. Lo que no se admite es presentar un estimado en cero ni un total sin fundamento: un cero indistinguible de un error es peor que la ausencia declarada.

La categoría **no resuelta del vehículo asignado** sí bloquea, pero en `T-08`: es `BD-07`, *"tiene categoría de peaje resuelta y vigente; sin ella el estimado no es verificable"*.

El parámetro `estimacion_peaje_obligatoria_para_aprobar` permite a la institución endurecer esto y exigir estimado calculado para aprobar. **Arranca apagado**, porque encenderlo sin tarifas cargadas detiene la operación. `[C]` cuándo lo enciende la institución — depende del insumo #21.

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md): *"El sistema debe estimar el costo de peajes de la ruta antes de aprobar la solicitud"* y *"presentar el estimado desglosado por punto, no como total opaco. Quien autoriza tiene que poder verificar el cálculo."*

Un viaje Tegucigalpa → San Pedro Sula atraviesa las tres estaciones del Corredor Logístico; ida y vuelta son **6 cruces** `[V]`. Sin desglose, el autorizador no puede distinguir un estimado correcto de uno que duplicó un cruce, y el estimado deja de ser un control para volverse un trámite.

El estimado es además la base de la conciliación posterior ([RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)): sin él no hay contra qué comparar lo pagado.

## Condiciones de aplicación

Aplica a solicitudes cuya ruta declarada atraviesa al menos un punto de peaje activo.

**No aplica** a misiones dentro de zonas sin peajes. [NRM-10](../normativa/NRM-10-peajes.md) `[C]` no encontró evidencia de peajes en CA-1 Sur, CA-4 Occidente ni carreteras departamentales; la ausencia se modela como ausencia de puntos en el catálogo, no como excepción codificada.

## Comportamiento esperado

1. La ruta se declara con origen, destinos y retorno; el sistema resuelve los puntos atravesados contra el catálogo, considerando **sentido de cobro** cuando el punto lo tenga.
2. El desglose muestra por fila: punto, sentido, número de cruces, categoría **y sobre qué se derivó — tipo de vehículo requerido o vehículo asignado —**, tarifa unitaria con su vigencia, y subtotal. Al pie, el total. Quien autoriza tiene que poder ver que el estimado se hizo sobre un tipo y no sobre una unidad concreta.
3. Un vehículo con exoneración vigente en un punto ([RN-38](RN-38-exoneracion-de-peaje.md)) estima **cero en ese punto**, con el fundamento visible en la misma fila. Un cero sin explicación es indistinguible de un error de cálculo.
4. El estimado se **congela al aprobar** junto con el identificador de la tabla de tarifas usada ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)).
5. Un cambio de ruta, de vehículo o de fechas **invalida el estimado** y exige recalcularlo antes de despachar. Si el recálculo supera el umbral respecto de lo autorizado, la reautorización es precondición del despacho (`T-12`).
6. El sistema arranca **sin tarifas cargadas** (insumo #21). En ese estado, todo estimado sale marcado *no disponible — sin tarifa vigente cargada*, y el tablero de parámetros lo muestra como insumo faltante. Es una condición visible, no un bloqueo silencioso.

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
- Momentos del cálculo: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) `T-02`, `INV-07`, `INV-11`, `T-08`, `BD-07`, `T-12`
- Hallazgos que corrigen esta regla: `HB1-09` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md); `HN1-06` y `HN1-10` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- Reglas relacionadas: [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-38](RN-38-exoneracion-de-peaje.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)
- Actores: ACT-02, ACT-03, ACT-04, ACT-08
- Historias y casos especiales: pendientes — Bloque 2
