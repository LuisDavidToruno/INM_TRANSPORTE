# HU-098 — Completar la ficha técnica que habilita la matriz licencia↔vehículo y la categoría de peaje

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular · M-18 Peajes |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — faltan el texto de la reforma al Art. 48 (2025), del que depende la matriz definitiva licencia↔vehículo, y el contenido literal del Art. 51, criterio legal de liviano frente a pesado (insumos #20 y #23). Ambos son escaneos sin capa de texto y requieren OCR. Sin ellos, la ficha técnica no puede derivar ni la habilitación ni la categoría de peaje |

## Historia

**Como** Jefe de Transporte
**quiero** capturar la ficha técnica con peso bruto vehicular, capacidad, número de ejes y condición de articulado, y que el sistema me advierta en el momento qué controles quedarán inoperantes si falta alguno
**para** que ningún vehículo llegue al despacho con un dato faltante que impide evaluar la licencia del motorista o resolver su tarifa de peaje

## Contexto

La ficha técnica no es un inventario descriptivo: es la fuente de dos controles.

El primero es la **matriz licencia↔vehículo**, el control de mayor valor legal del sistema, que se resuelve por tipo, **peso bruto vehicular en kg**, capacidad de pasajeros y condición de articulado — **nunca por el nombre comercial del modelo**.

El segundo es la **categoría de peaje**, que se ancla en la misma norma. Y aquí está el error que hay que evitar: la clasificación **no es puramente por ejes**. Un vehículo liviano tiene 2 ejes y paga L. 22; un "Vehículo de 2 Ejes" paga L. 90 `[V]`. Cualquier modelo que use el número de ejes como única llave va a cobrar cuatro veces de más a cada pickup de la flota.

**El sistema nunca asume un valor faltante.**

## Reglas que la gobiernan

- [RN-09](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) — Los atributos que la habilitación necesita: peso bruto, capacidad, articulado
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — Categoría derivada de la ficha técnica, **no del número de ejes por sí solo**; atributo con vigencia y fundamento
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — Capacidad de pasajeros y de carga como límites evaluables
- [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) — El rendimiento esperado es parámetro por vehículo con vigencia
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La derivación se resuelve contra la tabla vigente a la fecha del hecho
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — La ficha declara la unidad del odómetro

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Las capacidades de la ficha son las que se evalúan por tramo
- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — La flota institucional cae en la zona gris que la SAPP tuvo que resolver
- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — La unidad sustituida por el arrendador tiene ficha propia

## Criterios de aceptación

```gherkin
# language: es
Característica: Ficha técnica del vehículo y derivaciones que la necesitan

  Antecedentes:
    Dado un vehículo "TR-0092" en estado "NO_DISPONIBLE"
    Y una matriz licencia↔vehículo vigente
    Y una matriz de derivación de categoría de peaje vigente

  Escenario: Se advierte en el momento de la captura qué control queda inoperante
    Cuando el Jefe de Transporte guarda la ficha técnica de "TR-0092" sin peso bruto vehicular
    Entonces el sistema advierte en el momento de la captura
    Y muestra "Sin peso bruto vehicular no se puede evaluar la matriz licencia↔vehículo, y toda asignación de motorista quedará bloqueada."

  Escenario: Se rechaza habilitar sin peso bruto ni condición de articulado
    Cuando el Jefe de Transporte solicita la habilitación de "TR-0092" sin peso bruto vehicular
    Entonces el sistema rechaza la habilitación
    Y muestra "Falta el peso bruto vehicular en la ficha técnica de TR-0092. Sin ese dato la matriz licencia↔vehículo no puede evaluarse."
    Y el sistema no asume ningún valor por defecto

  Escenario: La categoría de peaje no se deriva del número de ejes por sí solo
    Cuando el Jefe de Transporte captura tipo "Pickup", peso bruto "2,800" kg, "2" ejes, no articulado
    Entonces el sistema deriva la categoría "Liviano/Turismo"
    Y muestra "Categoría Liviano/Turismo derivada de: tipo Pickup, peso bruto 2,800 kg, 2 ejes, no articulado."
    Y no deriva "Vehículo de 2 Ejes" por tener dos ejes

  Escenario: Dos vehículos de dos ejes reciben categorías distintas
    Dado un vehículo "TR-0098" tipo "Camión", peso bruto "9,500" kg, "2" ejes, no articulado
    Cuando el sistema deriva su categoría de peaje
    Entonces la categoría es "Vehículo de 2 Ejes"
    Y la categoría de "TR-0092", también de 2 ejes, sigue siendo "Liviano/Turismo"

  Escenario: Se rechaza asignar el vehículo sin categoría de peaje resuelta
    Dado un vehículo cuya ficha no permite derivar categoría
    Cuando el Jefe de Transporte intenta programar una misión con ese vehículo
    Entonces el sistema rechaza la asignación
    Y muestra "TR-0092 no tiene categoría de peaje resuelta. Complete la ficha técnica."

  Escenario: La categoría se registra con vigencia y fundamento
    Cuando el sistema deriva la categoría de peaje de "TR-0092"
    Entonces la registra con su fundamento, la versión de la matriz usada y su rango de vigencia
    Y una reclasificación posterior por resolución de la SAPP cierra el rango anterior y abre uno nuevo

  Escenario: Se rechaza la ficha sin la unidad del odómetro declarada
    Cuando el Jefe de Transporte guarda la ficha técnica sin declarar si el odómetro está en kilómetros o en millas
    Entonces el sistema rechaza el guardado
    Y muestra "Declare la unidad del odómetro. Asumir kilómetros en una unidad importada en millas produce un error del 60 % que nadie detecta hasta que la conciliación es absurda."

  Escenario: El rendimiento esperado de un vehículo nuevo se marca provisional
    Cuando el Jefe de Transporte registra un rendimiento esperado de "12.0" km por galón tomado de la ficha del fabricante
    Entonces el sistema lo registra con vigencia y lo marca "valor provisional hasta acumular histórico propio"

  Escenario: La capacidad declarada es la que limita el objeto del traslado
    Dado una ficha con capacidad de pasajeros "5" y capacidad de carga "1,000" kg
    Cuando el Jefe de Transporte programa una misión con 7 pasajeros
    Entonces el sistema rechaza la programación
    Y muestra "TR-0092 admite 5 pasajeros. La misión declara 7."

  Escenario: Se completa la ficha y el vehículo queda listo para habilitación
    Cuando el Jefe de Transporte captura tipo, marca, modelo, año, color, chasis o VIN, número de motor, peso bruto, capacidad de pasajeros, capacidad de carga en kg y m³, condición de articulado, número de ejes, tipo de combustible y rendimiento esperado
    Entonces la ficha técnica queda evaluable
    Y la causa "ficha técnica incompleta" desaparece del estado del vehículo
```

## Fuera de alcance

- El alta patrimonial del vehículo — es [HU-096](HU-096-dar-de-alta-el-vehiculo-con-titulo-de-tenencia.md)
- La habilitación en flota — es [HU-102](HU-102-habilitar-el-vehiculo-en-flota.md)
- La matriz licencia↔vehículo en sí y su carga con doble control — es [HU-106](HU-106-derivar-los-tipos-de-vehiculo-habilitados.md)
- La tabla de tarifas de peaje — es [HU-086](HU-086-no-emitir-discrepancia-sobre-tarifa-no-verificada.md)

## Notas y pendientes

- `[V]` Que la clasificación de peaje es combinada y no puramente por ejes; matriz de once categorías de la SAPP — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §1 y §2
- `[C]` Contenido literal del Artículo 51 de la Ley de Tránsito, criterio legal de liviano frente a pesado. El PDF oficial es un escaneo sin capa de texto; requiere OCR — insumo **#23**
- `[C]` Texto de la reforma al Art. 48 (2025), del que depende la matriz definitiva licencia↔vehículo — insumos **#20** y **#23**
- `[C]` `rendimiento_esperado` por vehículo y sus variantes por tipo de ruta — insumo **#1**
- El catálogo de categorías de peaje es **tabla abierta**: incluye montacargas y admite categorías futuras sin cambio de código
