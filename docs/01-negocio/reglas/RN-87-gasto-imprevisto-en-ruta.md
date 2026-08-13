# RN-87 — El gasto imprevisto en ruta distinto de combustible se registra con tipo, factura a nombre de la institución y autorización del acto

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-13, M-11, M-08 |
| **Origen** | Caso especial [CE-26](../../02-requisitos/casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de autorización y respaldo de todo desembolso — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` que la práctica ocurre en toda misión larga: inferencia del análisis, no norma |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `tipo_de_gasto_en_ruta` y parámetro `admite_gasto_en_ruta_contra_fondo_de_combustible` |

## Enunciado

El gasto imprevisto realizado durante la misión y **distinto de combustible** —repuesto, grúa, reparación menor, lavado, ponchera, estacionamiento— **debe** registrarse con:

1. **Tipo** del catálogo configurable
2. **Factura a nombre de la institución**, o descargo alternativo con su causa ([`RN-85`](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md))
3. **Autorización del acto**, con quién autorizó, cuándo y por qué medio — o su convalidación posterior ([`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md))
4. **Vinculación al vehículo**, para que integre su costo de operación

El parámetro institucional define si el gasto es **admisible contra el fondo de combustible** o **exige fuente distinta**.

## Justificación

El fondo de [`RN-26`](RN-26-fondo-de-combustible-aprobado.md) es de combustible, y [`RN-28`](RN-28-comprobacion-del-consumo-de-combustible.md) solo modela consumo con galones y estación. **Una faja de alternador comprada en Danlí no cabe en ningún campo.**

Y como no cabe, termina registrada de una de dos formas, ambas falsas: como **faltante** —y el motorista queda debiendo dinero que gastó en el vehículo— o como **consumo de combustible** por un monto que no corresponde a ningún galón, con lo que la conciliación galonaje–kilometraje se envenena.

Pasa en toda misión larga `[I]`. Un vehículo institucional de flota vieja, a 300 km de la sede, con una falla que un taller de pueblo resuelve por L 400: la alternativa a comprar el repuesto es dejar el vehículo ahí. Nadie va a elegir eso, y el sistema tiene que poder registrar lo que se eligió.

El gasto además pertenece al **costo de operación del vehículo** y hoy no llega a él. Un vehículo que consume repuestos cada dos misiones es información de mantenimiento, no una anomalía del fondo.

## Condiciones de aplicación

Aplica a todo desembolso hecho durante la ejecución de la misión, distinto de combustible y de peaje.

Aplica también cuando lo paga el servidor de su peculio, con la obligación de reintegro a su favor ([`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).

**No aplica** al peaje, que tiene su propio circuito ([`RN-34`](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-91`](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)), ni al viático y sus gastos conexos, que son de ARGOS ([DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).

## Comportamiento esperado

1. El registro se puede hacer **sin conectividad**, con fecha del hecho, monto, tipo, proveedor, odómetro y fotografía del comprobante.
2. El gasto **no entra** en la conciliación galonaje–kilometraje ([`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md)): no es combustible y contaminarla con él haría el indicador inservible.
3. El gasto **sí entra** en el **cuadre del fondo** si el parámetro lo admite; si no, se registra igual y queda como gasto pendiente de fuente, sin afectar el cuadre.
4. El gasto de tipo mantenimiento o repuesto **alimenta el expediente del vehículo en M-11** y su costo por kilómetro. Si corresponde, dispara la apertura de orden de trabajo para revisar lo reparado en ruta.
5. El gasto se dirige según el **rubro asumido del título de tenencia** ([`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)): lo que cubre el contrato de alquiler no se imputa al presupuesto de la institución.
6. La liquidación presenta los gastos en ruta **como renglón propio**, separado del combustible y de los peajes.

## Casos límite

- **Reparación mayor en ruta** que excede el umbral configurado. Exige autorización expresa antes de ejecutarse, o convalidación con la interrupción registrada ([`RN-70`](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)). Un motor rectificado en la carretera no es un gasto imprevisto: es una decisión de gestión.
- **Factura a nombre del motorista** porque el taller no factura a la institución. Es descargo alternativo con su causa y su calificación de suficiencia ([`RN-85`](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md)).
- **Gasto que resulta ser de un vehículo distinto** — el motorista apoyó a otra unidad. Se imputa al vehículo que lo recibió, con su misión si la tenía; la vinculación es al bien, no a quien pagó.
- **Lavado del vehículo antes de devolverlo.** Es gasto en ruta si ocurrió en misión, y es dato: el lavado sistemático antes de cada retorno tiene una explicación que vale la pena conocer.
- **Institución que no admite ningún gasto contra el fondo de combustible.** Es una configuración válida. El gasto se registra igual y queda pendiente de fuente; **lo que no se admite es que no se registre**.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · Decisión: [DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-73](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md), [RN-85](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)
- Casos especiales: [CE-26](../../02-requisitos/casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — candidata `RN-C26c`
