# HU-024 — Bloquear la programación de un vehículo sin categoría de peaje resuelta

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-18 Peajes |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 6 y nota `HCU-06` · `T-08` · `BD-07` |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema me impida programar una misión con un vehículo cuya categoría de peaje no está resuelta y vigente
**para** no descubrirlo el día del despacho, con el motorista ya en el predio, cuando la Orden de Misión no se puede imprimir porque no hay tarifa esperada que imprimir

## Contexto

La Orden de Misión impresa debe llevar, por punto de peaje, la categoría asignada al vehículo con su fundamento y la tarifa esperada ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)). Es lo que permite al motorista resolver la discrepancia **en la caseta, donde ocurre**, y no tres semanas después en la liquidación.

La categoría se deriva de la ficha técnica —tipo, número de ejes, peso, configuración— y **no del número de ejes por sí solo** `[P]` [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md). Si el vehículo no la tiene resuelta, no hay tarifa esperada; sin tarifa esperada el documento no cumple `RN-91`; y sin documento no hay salida. **Esta historia adelanta el bloqueo al momento de programar**, que es donde todavía hay tiempo de completar la ficha técnica. Es la corrección registrada en `HB3-09`: dejarlo como advertencia solo traslada el choque al predio.

## Reglas que la gobiernan

- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — La categoría se deriva de la ficha técnica, no del número de ejes por sí solo
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — La tarifa se resuelve por punto × categoría × vigencia, a la fecha del hecho
- [RN-91](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) — La Orden impresa lleva, por punto, la categoría asignada y la tarifa esperada del paquete congelado
- [RN-38](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) — La exoneración es dato por vehículo, punto, fundamento y vigencia; nunca una constante
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Tarifas y categorías son parámetros con vigencia por rango de fechas

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — En la caseta cobran una categoría que no corresponde: sin categoría asignada no hay contra qué comparar

## Criterios de aceptación

```gherkin
# language: es
Característica: Categoría de peaje resuelta como precondición de la programación

  Antecedentes:
    Dada una ruta autorizada Tegucigalpa–San Pedro Sula que atraviesa los puntos de peaje
      "Zambrano" y "Taulabé"
    Y una tabla de tarifas por punto y categoría vigente al "2026-09-10"
    Y una misión con ventana del "2026-09-10" al "2026-09-11"

  Escenario: Se rechaza por vehículo sin categoría de peaje resuelta
    Dado un vehículo "Camión Isuzu FVR" con correlativo "INS-C-002" sin categoría de peaje asignada
    Cuando el Jefe de Transporte intenta asignar el "INS-C-002" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "El vehículo INS-C-002 no tiene categoría de peaje resuelta. Complete la ficha técnica en el expediente del vehículo: sin categoría no hay tarifa esperada y la Orden de Misión no se podrá emitir."
    Y no propone continuar de todos modos

  Escenario: Se rechaza por categoría asignada cuya vigencia no cubre la salida
    Dado un vehículo "Microbús Toyota Coaster" con correlativo "INS-B-003"
      y categoría de peaje "Categoría 2" con vigencia del "2025-01-01" al "2026-08-31"
    Cuando el Jefe de Transporte intenta asignar el "INS-B-003" a esa misión
    Entonces el sistema rechaza la asignación
    Y muestra "La categoría de peaje del vehículo INS-B-003 dejó de estar vigente el 31/08/2026 y la misión sale el 10/09/2026."

  Escenario: Se acepta con categoría resuelta y se calcula el estimado por punto
    Dado un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014"
      y categoría de peaje "Categoría 1" vigente, con fundamento "ficha técnica: 2 ejes, 4 ruedas, peso bruto 2,800 kg"
    Cuando el Jefe de Transporte asigna el "INS-P-014" a esa misión
    Entonces el sistema acepta la asignación
    Y calcula el estimado de peajes desglosado por punto para "Zambrano" y "Taulabé"
    Y registra el identificador y la vigencia de la tabla de tarifas usada

  Escenario: Punto de peaje con tarifa marcada como no verificada
    Dado un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014" con categoría resuelta
    Y que la tarifa del punto "Taulabé" está marcada como "no verificada"
    Cuando el Jefe de Transporte asigna el "INS-P-014" a esa misión
    Entonces el sistema acepta la asignación
    Y muestra la advertencia "La tarifa del punto Taulabé está marcada como no verificada: se usará como referencia y la discrepancia sobre ese punto no será concluyente."
    Y el estimado del punto queda rotulado "tarifa no verificada — referencia"

  Escenario: Vehículo con exoneración registrada en un punto
    Dado un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014" con categoría resuelta
    Y una exoneración vigente para ese vehículo en el punto "Zambrano", con fundamento registrado
    Cuando el Jefe de Transporte asigna el "INS-P-014" a esa misión
    Entonces el estimado del punto "Zambrano" es "0.00" lempiras
    Y el punto queda marcado como "exonerado" con su fundamento
```

## Fuera de alcance

- La resolución de la categoría en la ficha técnica del vehículo — es del expediente del vehículo (M-03)
- La carga y el mantenimiento de la tabla de tarifas por punto y vigencia — es de M-02 y M-18
- La captura del monto efectivamente cobrado en la caseta y el reclamo por discrepancia — son de M-08, M-18 y M-13
- El recálculo del estimado cuando se sustituye el vehículo — es [HU-043](HU-043-sustituir-vehiculo-o-motorista-en-programada.md)

## Notas y pendientes

- `[C]` **Tarifa de peaje efectivamente vigente**, confirmada con la concesionaria o la SAPP — insumo #21. No se carga ninguna tarifa sin esto.
- `[C]` **Lista oficial de exoneraciones** — insumo #22: define si el vehículo administrativo del Estado paga o no.
- `[C]` **¿El peaje se financia con el viático o es gasto de misión separado?** — insumo #25. Si va en el viático, la frontera con ARGOS cambia.
- `[P]` El criterio de clasificación por atributos del vehículo proviene de [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md); falta el texto del articulado — insumo #23.
