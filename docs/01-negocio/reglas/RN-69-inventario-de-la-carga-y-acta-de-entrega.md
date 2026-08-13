# RN-69 — La carga se declara con inventario al despacho, se entrega con acta, y toda diferencia se declara como faltante

| Campo | Valor |
|---|---|
| **Módulos** | M-06, M-08, M-12, M-17, M-15 |
| **Origen** | Casos especiales [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md), [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md), [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md), [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) · Normas [NRM-06](../normativa/NRM-06-transito-y-licencias.md) y [NRM-02](../normativa/NRM-02-bienes-del-estado.md) |
| **Verificación** | `[V]` la obligación de registrar remitente y consignatario de la carga — [NRM-06](../normativa/NRM-06-transito-y-licencias.md). `[I]` el inventario unitario y el acta de faltante: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — umbral a partir del cual se exige identificación unitaria |

## Enunciado

El objeto del traslado que sea **bien inventariable** —equipo, herramienta, mobiliario, insumo con número de bien— **debe** declararse con **identificación unitaria, cantidad y responsable de entrega**, tanto al despachar como al arribar.

**Ninguna entrega en destino se registra como conforme si el inventario de arribo difiere del de salida.** La diferencia se declara como **faltante** y abre expediente en M-12.

Todo movimiento de la carga entre vehículos durante la misión —transbordo por avería, por relevo, por incapacidad— **debe** constar en **acta con inventario, hora, y firma de quien entrega y de quien recibe**.

La carga que **vuelve sin entregarse** se reingresa con acta e inventario contrastado contra el de salida; la **entrega parcial** declara expresamente qué quedó pendiente.

## Justificación

[`RN-22`](RN-22-custodia-del-vehiculo.md) resuelve la custodia **del vehículo**. Nada resuelve la custodia **de lo que el vehículo lleva**, que es a menudo lo más caro del viaje: el equipo de cómputo de una delegación, las herramientas de una brigada, los insumos de un operativo.

El efecto de la ausencia es directo: si algo falta al llegar, no hay contra qué contrastarlo. El faltante se convierte en una conversación entre dos personas y en un expediente que no puede probar qué salió. Ante el Tribunal Superior de Cuentas, la pregunta no es si el motorista es honesto: es **qué salió y qué llegó**, y eso solo lo responde un inventario.

El acta de transbordo tiene la misma lógica: el momento en que la carga cambia de vehículo, en la carretera, de noche, es exactamente el punto de la cadena donde la custodia se pierde sin que nadie lo note.

## Condiciones de aplicación

Aplica a todo bien inventariable y a toda carga cuyo valor o naturaleza supere el umbral configurado por la institución.

**No aplica** a los efectos personales del motorista ni de los pasajeros.

**No aplica** a la carga a granel no inventariable —agua, arena, material de construcción— que se declara por cantidad y unidad de medida, sin identificación unitaria, pero **sí** con acta de entrega.

## Comportamiento esperado

1. La solicitud declara la lista de objetos con su ficha: tipo de carga del catálogo, identificación unitaria cuando corresponda, cantidad, peso y volumen, y **cuál es el objeto principal** ([`RN-67`](RN-67-matriz-de-compatibilidad-objeto-objeto.md)).
2. El despacho genera el **manifiesto de carga** impreso con folio y QR ([`RN-25`](RN-25-salvoconducto-con-folio-y-qr.md)), cerrado al despachar ([`RN-53`](RN-53-cierre-del-manifiesto-al-despacho.md)); los cambios en ruta son novedades, no ediciones.
3. La entrega en destino registra: inventario de arribo, identidad y cargo del **consignatario**, hora y constancia de recepción. El sistema compara contra el de salida y, si difieren, **impide registrar la entrega como conforme**.
4. Toda acta —entrega, transbordo, reingreso— se puede levantar **sin conectividad** y se imprime con folio del rango de la delegación ([`RN-44`](RN-44-identificadores-y-folios-en-el-cliente.md)).
5. El faltante abre expediente en M-12 con responsable y plazo, y **no imputa responsabilidad a nadie por sí solo**: la determinación es materia del expediente y de quien corresponda ([`RN-74`](RN-74-sin-atribucion-de-responsabilidad-en-campo.md)).
6. En misión multi-destino, la entrega se registra **por destino**, y el grado de cumplimiento se consolida al cierre ([`RN-78`](RN-78-grado-de-cumplimiento-del-objeto.md)).

## Casos límite

- **Sustracción de parte de la carga.** El faltante se declara contra el inventario de salida y se encadena al evento de sustracción ([`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)), con constancia de denuncia ante autoridad.
- **Consignatario que se niega a firmar** o no está. La entrega no se registra como conforme: se registra el intento con hora, quién atendió y por qué no se recibió, y la carga vuelve o se resguarda con acta.
- **Carga que se incorpora en un destino intermedio.** Se registra como novedad, entra al inventario del tramo siguiente y dispara la reevaluación de [`RN-68`](RN-68-compatibilidad-y-capacidad-por-tramo.md).
- **Formato de acta de entrega de la institución.** `[C]` insumo #2 — si existe en papel, ese formato es el diseño de la pantalla, campo por campo. Hasta tenerlo, los campos son los mínimos de esta regla.
- **Diferencia por merma legítima** en carga a granel. Se declara con la merma esperada del catálogo y su fundamento; fuera del rango, es faltante.
- **Bien inventariable sin número de bien asignado.** Se declara por descripción, marca, modelo y serie, y el hecho de que no tenga número es en sí mismo un dato para el inventario institucional.

## Trazabilidad

- Normas: [NRM-06](../normativa/NRM-06-transito-y-licencias.md) `[V]` remitente y consignatario · [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]`
- Reglas relacionadas: [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md), [RN-22](RN-22-custodia-del-vehiculo.md), [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-44](RN-44-identificadores-y-folios-en-el-cliente.md), [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md), [RN-67](RN-67-matriz-de-compatibilidad-objeto-objeto.md), [RN-68](RN-68-compatibilidad-y-capacidad-por-tramo.md), [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md), [RN-78](RN-78-grado-de-cumplimiento-del-objeto.md)
- Casos especiales: [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) `RN-c:inventario-unitario-de-la-carga`, `RN-c:entrega-con-faltante-declarado` · [CE-02](../../02-requisitos/casos-especiales/CE-02-averia-mecanica-en-ruta.md) `RN-c:acta-de-transbordo-de-carga` · [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) `RN-c:reingreso-de-carga-no-entregada`
- Insumos pendientes: #2 formatos en papel vigentes
