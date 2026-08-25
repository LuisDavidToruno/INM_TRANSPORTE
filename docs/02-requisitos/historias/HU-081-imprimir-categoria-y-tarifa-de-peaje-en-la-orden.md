# HU-081 — Llevar impresa en la Orden de Misión la categoría y la tarifa esperada de cada punto de peaje

| Campo | Valor |
|---|---|
| **Módulo** | M-18 Peajes · M-15 Formatos Oficiales e Impresión |
| **Actor** | ACT-06 Motorista (destinatario) · ACT-07 Encargado de Combustible (emite el paquete) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el texto del Artículo 51 de la Ley de Tránsito, criterio legal de liviano frente a pesado, cuyo PDF oficial es un escaneo sin capa de texto (insumo #23), la lista oficial de exoneraciones (insumo #22) y si la institución opera con tag prepago (insumo #24): con tag no hay ticket y la Orden debe indicarlo |

## Historia

**Como** Motorista
**quiero** que la Orden de Misión que llevo impresa muestre, punto por punto, la categoría con que se clasifica mi vehículo y la tarifa esperada
**para** tener algo que decir en la caseta cuando me cobran de más, en lugar de pagar y discutirlo tres semanas después cuando ya no se puede reclamar

## Contexto

La clasificación de peaje **no es solo por ejes**: un vehículo liviano y un "Vehículo de 2 Ejes" tienen ambos dos ejes y pagan L. 22 y L. 90 respectivamente `[V]`. La flota típica de una institución pública hondureña —pickups, panels tipo H-100 o K2700, microbuses Sprinter— cae exactamente en la zona gris que la SAPP tuvo que resolver por comunicado el 17/09/2025 `[V]`.

Es previsible que a un vehículo institucional le cobren mal. Con el fundamento en la mano, el motorista puede objetar en el momento. Sin eso, no tiene nada.

Los valores impresos salen del **paquete normativo congelado** al despachar, no de la tabla que el servidor tenga hoy.

## Reglas que la gobiernan

- [RN-91](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) — Categoría asignada y tarifa esperada, por punto, impresas en la Orden
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — La categoría se deriva de la ficha técnica, **no del número de ejes por sí solo**
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — Tarifa como (punto × categoría × rango de vigencia)
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El paquete normativo se congela al despachar y es el que se usa en ruta
- [RN-38](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) — La exoneración se imprime con su fundamento, nunca como un cero sin explicación
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Todo documento de control en carretera lleva folio, QR, firma, sello y hash

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Eje de la historia
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — Destinos agregados en ruta: el recálculo usa el paquete congelado

## Criterios de aceptación

```gherkin
# language: es
Característica: Categoría y tarifa de peaje impresas en la Orden de Misión

  Antecedentes:
    Dado un vehículo "TR-0045" tipo "Pickup", peso bruto "2,800" kg, "2" ejes, no articulado
    Y una categoría de peaje derivada "Liviano/Turismo" con su fundamento registrado
    Y una ruta autorizada de Tegucigalpa a San Pedro Sula, ida y vuelta
    Y los puntos de peaje "Zambrano", "Siguatepeque" y "Yojoa"

  Escenario: Se rechaza despachar un vehículo sin categoría de peaje resuelta
    Dado un vehículo "TR-0071" cuya ficha técnica no tiene peso bruto vehicular
    Cuando el Encargado de Despacho intenta despachar una misión con "TR-0071"
    Entonces el sistema rechaza el despacho
    Y muestra "El vehículo TR-0071 no tiene categoría de peaje resuelta: falta el peso bruto vehicular en su ficha técnica."

  Escenario: La Orden impresa detalla cada paso con su categoría y su tarifa
    Cuando el sistema imprime la Orden de Misión "OM-2026-0512"
    Entonces el documento lista 6 pasos por caseta
    Y cada paso muestra punto, sentido, categoría "Liviano/Turismo" y tarifa esperada "L 22.00"
    Y muestra el total esperado de peajes "L 132.00"
    Y no presenta el total sin el desglose por punto

  Escenario: La Orden impresa indica el fundamento de la categoría
    Cuando el sistema imprime la Orden de Misión "OM-2026-0512"
    Entonces el documento muestra "Categoría Liviano/Turismo derivada de: tipo Pickup, peso bruto 2,800 kg, 2 ejes, no articulado."

  Escenario: La tarifa impresa es la del paquete congelado, no la del servidor
    Dado que el paquete normativo de "OM-2026-0512" se congeló el "2026-09-24" con tarifa "L 22.00" para "Liviano/Turismo"
    Y que el servidor cargó después una tarifa distinta con vigencia desde el "2026-10-01"
    Cuando el motorista consulta la Orden en su dispositivo el "2026-09-26"
    Entonces la tarifa mostrada sigue siendo "L 22.00"
    Y el documento indica la versión de la tabla de tarifas usada

  Escenario: Un punto exonerado se imprime con su fundamento, no como cero mudo
    Dado que el vehículo "TR-0090" tiene exoneración registrada en "Zambrano" con fundamento y vigencia
    Cuando el sistema imprime la Orden de Misión de "TR-0090"
    Entonces el paso por "Zambrano" muestra tarifa esperada "L 0.00"
    Y muestra el fundamento de la exoneración y su vigencia

  Escenario: Una tarifa no verificada se imprime marcada como tal
    Dado que la tarifa del punto "Yojoa" está marcada como "no verificada"
    Cuando el sistema imprime la Orden de Misión "OM-2026-0512"
    Entonces el paso por "Yojoa" muestra "Tarifa referencial no verificada — verifique el monto cobrado en caseta"
    Y el total esperado se presenta como referencial

  Escenario: Extensión de ruta en ruta recalcula con el paquete congelado
    Dado que se autoriza un destino adicional a "Puerto Cortés" el "2026-09-26"
    Cuando el sistema recalcula el estimado de peajes
    Entonces usa la tabla del paquete congelado del "2026-09-24"
    Y no usa la tabla vigente en el servidor al momento del recálculo
```

## Fuera de alcance

- El registro del paso efectivo por caseta — es [HU-085](HU-085-registrar-el-paso-por-caseta-y-marcar-discrepancia.md)
- La conciliación estimado contra pagado — es [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)
- La carga y mantenimiento de la tabla de tarifas: es acto de catálogo con doble control ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md))
- La derivación de la categoría al dar de alta el vehículo — es [HU-098](HU-098-completar-la-ficha-tecnica-que-habilita.md)

## Notas y pendientes

- 🔴 `[C]` **bloqueante — la tarifa efectivamente vigente no está confirmada.** Contradicción abierta entre el comunicado de la SIT del 28/02/2026 (*no habrá incremento para ninguna categoría*, `[V]`) y lo que publica un agregador comercial. **No se carga ninguna tarifa hasta confirmarla con COVI-H o la SAPP** — insumo **#21**, [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §4 y §10
- `[V]` La matriz de once categorías publicada por la SAPP, y que la clasificación es combinada y no puramente por ejes — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §1 y §2
- `[C]` El texto del Artículo 51 de la Ley de Tránsito, criterio legal de liviano frente a pesado. El PDF oficial es un escaneo sin capa de texto — insumo **#23**
- `[C]` Lista oficial de exoneraciones — insumo **#22**. Se modela como dato por vehículo, punto, fundamento y vigencia; **nunca como constante**
- `[C]` Si la institución opera con tag prepago: con tag no hay ticket y la Orden debe indicarlo — insumo **#24**
