# RN-91 — La Orden de Misión impresa lleva, por cada punto de peaje de la ruta, la categoría asignada al vehículo y la tarifa esperada

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-15, M-07, M-08 |
| **Origen** | Caso especial [CE-24](../../02-requisitos/casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) · Norma [NRM-10](../normativa/NRM-10-peajes.md) · Premisa rectora 4 (híbrido digital–papel por diseño) |
| **Verificación** | `[P]` la existencia de tarifas por punto y categoría — [NRM-10](../normativa/NRM-10-peajes.md) `[P]`. `[C]` las tarifas concretas vigentes, insumo #21. `[I]` la obligación de imprimirlas: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro sobre la emisión del documento |
| **Configurable** | No la obligación de imprimir. Sí la plantilla del formato ([M-15](../../../CLAUDE.md)) |

## Enunciado

El documento impreso de la Orden de Misión **debe** incluir, por cada punto de peaje previsto en la ruta autorizada:

1. El **punto de peaje** con su nombre y su ubicación
2. La **categoría de peaje** asignada al vehículo, con el **fundamento** que la sustenta — tipo de vehículo, número de ejes, peso bruto vehicular o condición de articulado, según [`RN-33`](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)
3. La **tarifa esperada** a la fecha de la misión, tomada del paquete normativo congelado al despachar
4. La marca de **exoneración** si el vehículo la tiene para ese punto, con su fundamento ([`RN-38`](RN-38-exoneracion-de-peaje.md))
5. El **identificador y la vigencia** de la tabla de tarifas usada

Ningún documento de Orden de Misión **debe** emitirse sin esta sección cuando la ruta autorizada atraviesa al menos un punto de peaje.

Cuando la tarifa de un punto esté marcada como **no verificada** ([NRM-10 §9](../normativa/NRM-10-peajes.md)), el documento la imprime igual, rotulada *tarifa no verificada — referencia*, y la detección de discrepancia sobre ese punto se presenta como **no concluyente**.

## Justificación

[`RN-35`](RN-35-estimacion-de-peajes-antes-de-aprobar.md) estima y desglosa el costo de peajes **para quien autoriza**. [`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md) obliga a imprimir los documentos de control en carretera. Ninguna de las dos pone el dato **en la mano del motorista, en la caseta** — que es el único lugar y el único momento en que un cobro en categoría equivocada se puede evitar.

Discutir la clasificación en la caseta, con el papel institucional que dice *categoría 2, L \<x\>, fundamento: 2 ejes, PBV \<y\> kg*, tiene una probabilidad de éxito que discutirla de memoria no tiene. Y si aun así cobran de más, el motorista sabe **en el acto** que hay discrepancia y la registra como tal, en vez de descubrirse tres semanas después al conciliar.

El costo de imprimir cinco líneas más es cero. El costo de no imprimirlas es el sobrecosto repetido en cada cruce de todos los vehículos de la flota, que además nadie reclama porque nadie lo detecta.

## Condiciones de aplicación

Aplica a toda Orden de Misión cuya ruta autorizada atraviese al menos un punto de peaje del catálogo.

**No aplica** cuando la ruta autorizada no cruza ningún punto — el documento omite la sección completa, no imprime una sección vacía.

Aplica también a la **reimpresión** por sustitución de vehículo: el documento nuevo lleva la categoría y la tarifa del vehículo sustituto, recalculadas y vueltas a congelar ([`RN-61`](RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)).

## Comportamiento esperado

1. Al despachar, el sistema resuelve categoría por [`RN-33`](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) y tarifa por [`RN-34`](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) a la **fecha de la misión**, y las **congela** junto con el identificador de la tabla ([`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md)).
2. Si el vehículo **no tiene categoría de peaje resuelta**, el despacho se bloquea: sin categoría no hay tarifa esperada, y sin tarifa esperada el documento no cumple esta regla. La salida es completar la ficha técnica, no imprimir sin el dato.
3. El documento impreso lleva folio y QR verificable ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md)), de modo que el operador de la caseta pueda confirmar que el papel corresponde a una misión real.
4. Junto a cada punto, el impreso deja un **espacio de captura manual**: monto efectivamente cobrado y número de ticket. Es lo que el motorista llena en la caseta, sin señal, y lo que después se digita o se fotografía.
5. Al registrar el paso por caseta, el sistema compara lo pagado contra lo esperado congelado y aplica [`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md).
6. La sección impresa incluye la **instrucción de actuación**: exigir el ticket, anotar el monto, no discutir más allá de presentar el documento, y registrar la discrepancia. El sobrecosto **nunca** se imputa al motorista ([`RN-85`](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md), [`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).

## Casos límite

- **Destino agregado en ruta** que suma casetas no previstas. El impreso original no las trae. Se registran como paso por caseta fuera del impreso, amparado por la extensión autorizada ([`RN-77`](RN-77-versionado-del-alcance-autorizado.md)), y la tarifa esperada se resuelve al conciliar con la tabla vigente a la fecha del hecho — no con la del despacho, si son distintas.
- **Tarifa que cambia entre el despacho y el paso por la caseta**, en misión larga. Manda la vigente **a la fecha del hecho del paso** ([`RN-40`](RN-40-calculo-a-la-fecha-del-hecho.md)). El impreso queda desactualizado y eso no es discrepancia: es cambio de tarifa, y se tipifica distinto.
- **Vehículo con tag o medio electrónico de pago.** El impreso lleva igual la categoría y la tarifa esperada; la conciliación se hace después contra la línea del estado de cuenta. `[C]` insumo #24 — si la institución tiene tags y a nombre de quién.
- **Ruta con punto de peaje opcional** — hay vía alterna sin peaje. El impreso los marca como *previsto* y *alterno*; no pasar por el previsto no es incoherencia de secuencia ([`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md)) si la alterna es geográficamente compatible.
- **Tabla de tarifas no cargada** para un punto de la ruta. El documento imprime *tarifa no disponible* y el paso se registra sin esperado. La detección de discrepancia queda **no concluyente**: un detector montado sobre una tabla no verificada produce reclamos falsos en masa y destruye la credibilidad del primero que sí era cierto.

## Trazabilidad

- Norma: [NRM-10](../normativa/NRM-10-peajes.md) `[P]` — tarifas por punto, categoría y vigencia; §9 fuente y fecha de verificación
- Reglas relacionadas: [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md), [RN-38](RN-38-exoneracion-de-peaje.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-92](RN-92-reclamo-por-discrepancia-de-peaje.md)
- Casos especiales: [CE-24](../../02-requisitos/casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — candidatas `RN-C24a` y `RN-C24c`
- Insumos pendientes: #21 tarifa vigente · #22 exoneraciones · #24 tags de peaje
- Actores: ACT-05 despacha e imprime · ACT-06 registra en la caseta · ACT-12 concilia
