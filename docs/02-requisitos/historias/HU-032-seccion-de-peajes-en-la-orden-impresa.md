# HU-032 — Imprimir en la Orden de Misión la sección de peajes por punto, con espacio de captura manual

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión · M-18 Peajes |
| **Actor** | ACT-05 Encargado de Despacho (emite) · ACT-06 Motorista (usa el impreso en la caseta) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-05](../casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md) pasos 8 y 9, E8 · `T-12` · `EF-03` |

## Historia

**Como** Motorista
**quiero** llevar impresa, por cada punto de peaje de mi ruta, la categoría asignada a mi vehículo con su fundamento y la tarifa esperada, más un espacio para anotar el monto cobrado y el número de ticket
**para** poder reclamar en la caseta, en el momento y con el papel en la mano, cuando me cobren una categoría que no corresponde — y para no terminar poniendo la diferencia de mi bolsillo

## Contexto

La escena es concreta: el motorista llega a la caseta con un pickup y le cobran tarifa de camión. Sin nada impreso, paga y sigue. La diferencia aparece semanas después en la liquidación, donde ya no se puede reclamar a nadie, y termina imputándose al motorista o quedando como faltante sin explicación.

Con la categoría, el fundamento y la tarifa esperada impresas, el reclamo ocurre **donde el hecho ocurre**. Y con el espacio de captura manual, la discrepancia entra al expediente aunque el vehículo esté en zona sin señal.

**El sobrecosto nunca se imputa al motorista.** Eso se imprime también, porque es lo que le permite sostener la posición frente al cobrador.

## Reglas que la gobiernan

- [RN-91](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) — **Regla rectora**: la Orden impresa lleva, por punto, categoría asignada y tarifa esperada del paquete congelado
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — La categoría se deriva de la ficha técnica y su fundamento se imprime
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — Tarifa por punto × categoría × vigencia
- [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) — El cobro en categoría distinta se registra como discrepancia y habilita el reclamo
- [RN-38](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) — La exoneración se imprime con su fundamento y vigencia
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — Se imprime el valor del paquete congelado, con el identificador de la tabla usada

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — En la caseta cobran una categoría que no corresponde
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Sin señal, el espacio de captura manual es el único registro del hecho

## Criterios de aceptación

```gherkin
# language: es
Característica: Sección de peajes de la Orden de Misión impresa

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451" con ruta autorizada Tegucigalpa–San Pedro Sula
      que atraviesa los puntos de peaje "Zambrano" y "Taulabé"
    Y un vehículo "Pickup Toyota Hilux" con correlativo "INS-P-014",
      categoría de peaje "Categoría 1" con fundamento "ficha técnica: 2 ejes, 4 ruedas, peso bruto 2,800 kg"
    Y una tabla de tarifas identificada como "TAR-2026-01", vigente desde el "2026-01-15"

  Escenario: Se rechaza la emisión si algún punto de la ruta no tiene categoría resuelta para el vehículo
    Dado un vehículo "Camión Isuzu FVR" con correlativo "INS-C-002" sin categoría de peaje asignada
    Cuando el Encargado de Despacho intenta emitir la Orden de Misión con ese vehículo
    Entonces el sistema rechaza la emisión
    Y muestra "El vehículo INS-C-002 no tiene categoría de peaje resuelta: la Orden de Misión no puede imprimir la tarifa esperada. Complete la ficha técnica."

  Escenario: La sección de peajes imprime punto, categoría, fundamento y tarifa esperada
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la Orden impresa contiene una fila por cada punto de peaje de la ruta, en orden de recorrido
    Y cada fila muestra el nombre y la ubicación del punto, la categoría "Categoría 1",
      el fundamento "ficha técnica: 2 ejes, 4 ruedas, peso bruto 2,800 kg" y la tarifa esperada
    Y el pie de la sección declara "Tabla de tarifas TAR-2026-01, vigente desde el 15/01/2026"

  Escenario: Cada punto trae espacio de captura manual y la instrucción de actuación
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces cada fila de peaje incluye casillas en blanco para "monto cobrado" y "número de ticket"
    Y la sección imprime la instrucción "Exija el ticket. Anote el monto cobrado. Presente este documento. Si el cobro difiere de la tarifa esperada, registre la discrepancia."
    Y la sección imprime "El sobrecosto por clasificación errónea no se imputa al motorista."

  Escenario: Punto con tarifa marcada como no verificada
    Dado que la tarifa del punto "Taulabé" está marcada como "no verificada" en la tabla "TAR-2026-01"
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la fila del punto "Taulabé" imprime la tarifa con el rótulo "tarifa no verificada — referencia"
    Y la Orden advierte "La discrepancia sobre este punto se evaluará como no concluyente."

  Escenario: Punto sin tabla cargada
    Dado que no existe tarifa cargada para el punto "Taulabé" en la tabla "TAR-2026-01"
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la fila del punto "Taulabé" imprime "tarifa no disponible"
    Y conserva las casillas de captura manual de monto cobrado y número de ticket

  Escenario: Punto exonerado
    Dada una exoneración vigente del vehículo "INS-P-014" en el punto "Zambrano",
      con fundamento registrado
    Cuando el Encargado de Despacho emite la Orden de Misión "OM-2026-0451"
    Entonces la fila del punto "Zambrano" imprime "EXONERADO" con su fundamento y vigencia
    Y la tarifa esperada de ese punto es "0.00" lempiras
```

## Fuera de alcance

- La resolución de la categoría del vehículo — es de M-03 y de [HU-024](HU-024-categoria-de-peaje-resuelta-para-programar.md)
- La digitación del monto cobrado y del ticket al retornar, y el expediente de reclamo — son de M-08, M-18 y M-13
- La conciliación de peajes contra el estado de cuenta del concesionario — es de M-13 y M-14
- El uso de dispositivos de prepago en caseta — depende de un insumo abierto

## Notas y pendientes

- `[C]` **Tarifa de peaje efectivamente vigente** — insumo #21. Hay contradicción entre fuentes; **no se carga ninguna tarifa sin esto**.
- `[C]` **Lista oficial de exoneraciones** — insumo #22.
- `[C]` **¿La institución tiene dispositivos de prepago en caseta y a nombre de quién?** ¿Se emite factura fiscal en caseta o estado de cuenta empresarial? — insumo #24. Cambia qué se le imprime al motorista.
- `[P]` La clasificación por atributos del vehículo proviene de [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md); falta el articulado — insumo #23.
