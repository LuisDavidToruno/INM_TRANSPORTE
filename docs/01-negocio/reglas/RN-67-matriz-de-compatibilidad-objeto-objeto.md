# RN-67 — Existe una matriz de compatibilidad entre objetos del traslado, evaluada par a par sobre todos los declarados

| Campo | Valor |
|---|---|
| **Módulos** | M-06, M-07, M-02, M-17 |
| **Origen** | Caso especial [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) · Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) · Premisa rectora 1 |
| **Verificación** | `[P]` la obligación de registrar remitente, consignatario y naturaleza de la carga — [NRM-06](../normativa/NRM-06-transito-y-licencias.md). `[I]` la matriz objeto × objeto: implicación de requerimiento del equipo, sin articulado citable |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `matriz_compatibilidad_objeto_objeto` con vigencia por rango de fechas |

## Enunciado

Además de la compatibilidad **vehículo × objeto** de [`RN-20`](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), el sistema **debe** evaluar la compatibilidad **objeto × objeto**: para toda Orden de Misión con más de un objeto del traslado declarado, se evalúan **todos los pares** posibles contra una matriz configurable con vigencia por rango de fechas.

Cada par tiene uno de tres resultados:

| Resultado | Efecto |
|---|---|
| `COMPATIBLE` | Continúa |
| `COMPATIBLE_CON_CONDICIONES` | Continúa, **imprime las condiciones** y exige acuse del despachador |
| `INCOMPATIBLE` | **Bloquea** |

La **ausencia de entrada** en la matriz para un par declarado **bloquea**. No se resuelve por defecto como compatible.

La solicitud **debe** declarar el objeto principal de la misión.

## Justificación

[`RN-20`](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) solo cruza **vehículo × objeto**. El ejemplo que la propia `RN-20` usa para justificarse — *personas junto a bidones de combustible* — **hoy no se puede expresar en el sistema**: un pickup es compatible con personas, y es compatible con combustible en bidones; nada dice que personas y combustible juntos no lo sean.

El riesgo no es hipotético en la operación real: el mismo vehículo lleva al personal de una delegación y, en la misma paila, el combustible de la planta eléctrica, los agroquímicos incautados o el equipo pesado sin amarre. Cada una de esas combinaciones tiene una razón concreta para no ocurrir, y ninguna está escrita en ningún lado.

El bloqueo por ausencia de entrada es deliberado. Una matriz que asume compatible lo que no conoce es una matriz que no bloquea nada el primer año, mientras el catálogo se llena.

## Condiciones de aplicación

Aplica a toda Orden de Misión con dos o más objetos del traslado declarados, de cualquier naturaleza: personal de la institución, personas externas ([M-17](../../../CLAUDE.md)), carga inventariable, carga a granel, animales, materiales peligrosos.

Aplica también a la carga o a las personas **incorporadas después del despacho**, que se registran como novedad ([`RN-69`](RN-69-inventario-de-la-carga-y-acta-de-entrega.md)) y se evalúan por tramo ([`RN-68`](RN-68-compatibilidad-y-capacidad-por-tramo.md)).

**No aplica** a la misión con un solo objeto declarado, donde basta `RN-20`.

## Comportamiento esperado

1. La matriz se resuelve **a la fecha de la misión**, con la versión vigente a esa fecha ([`RN-40`](RN-40-calculo-a-la-fecha-del-hecho.md)), y la versión aplicada se registra junto con el resultado ([`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md)).
2. Al bloquear, el sistema muestra **el par concreto** y el fundamento de la entrada: *"Personas (7) e inflamables en envase no homologado (2 bidones, 40 gal) — INCOMPATIBLE. Fundamento: \<texto de la entrada\>"*. Un bloqueo sin el dato concreto empuja la operación fuera del sistema, que es la peor forma de cumplir una regla.
3. Junto al bloqueo, el sistema presenta las **salidas validadas** con su costo: dividir en misiones hermanas, sustituir por un vehículo compatible con ambos objetos, o reprogramar. Registra **cuál se eligió, quién la eligió y cuándo**.
4. Cuando se dividen, las misiones hermanas se vinculan explícitamente entre sí por folio.
5. Los pares `COMPATIBLE_CON_CONDICIONES` imprimen sus condiciones en el documento de despacho, y el despachador acusa haberlas leído. El acuse queda con su marca de tiempo.
6. El resultado completo de la evaluación —cada par, su veredicto y la versión de la matriz— se conserva en el expediente. Es la prueba de que alguien evaluó, que hoy no existe en ningún papel.

## Casos límite

- **Objeto que aparece en ruta** y no estaba declarado. Se evalúa contra los ya embarcados, en el tramo correspondiente. Si resulta `INCOMPATIBLE`, el sistema no puede impedir un hecho físico ya ocurrido: lo registra, lo marca y produce hallazgo. Negarse a registrarlo solo lo haría invisible.
- **Personas bajo custodia o menores.** `[C]` insumo #39 — si la institución los traslada. Cambia la matriz y arrastra requisitos propios de M-17. Mientras no se confirme, no hay entradas para esos objetos y por tanto **bloquea**, que es la posición correcta.
- **Carga que deja de ser incompatible al entregarse un objeto en un destino intermedio.** Es exactamente lo que resuelve [`RN-68`](RN-68-compatibilidad-y-capacidad-por-tramo.md): la evaluación es por tramo, no por misión.
- **Matriz sin entrada para un par frecuente**, que bloquea la operación todos los días. No se resuelve aflojando la regla: se resuelve cargando la entrada, con fundamento y vigencia, por ACT-01 y con puesta en vigencia de ACT-08 ([`RN-39`](RN-39-parametros-normativos-con-vigencia.md)).
- **Dos objetos legítimos que compiten** y la matriz dice `COMPATIBLE` pero la capacidad no alcanza. Es capacidad, no compatibilidad: rige [`RN-21`](RN-21-capacidad-de-pasajeros-y-carga.md), y la reducción se aplica **primero al objeto no principal**. En ningún caso la configuración se resuelve trasladando personas fuera de plazas homologadas.

## Trazabilidad

- Norma: [NRM-06](../normativa/NRM-06-transito-y-licencias.md) `[P]` · Premisa rectora 1 y 2
- Reglas relacionadas: [RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md), [RN-68](RN-68-compatibilidad-y-capacidad-por-tramo.md), [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md)
- Casos especiales: [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — candidatas `RN-C18a`, `RN-C18c`, `RN-C18d`
- Insumos pendientes: #39 traslado de personas bajo custodia o menores
- Actores: ACT-02 declara los objetos · ACT-05 despacha y acusa condiciones · ACT-01 mantiene la matriz
