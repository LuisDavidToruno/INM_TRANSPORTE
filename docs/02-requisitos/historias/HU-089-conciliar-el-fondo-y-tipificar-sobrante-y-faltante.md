# HU-089 — Conciliar el fondo de la misión y tipificar el sobrante y el faltante

| Campo | Valor |
|---|---|
| **Módulo** | M-13 Liquidación y Cierre · M-09 Combustible |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta si el sobrante se devuelve o se arrastra (`PROP-01` / insumo #7): decide la tipificación misma del sobrante, no un parámetro. Faltan también el `plazo_devolucion_saldo` y el plazo de liquidación en días hábiles (insumo #32) |

## Historia

**Como** Jefe de Transporte
**quiero** conciliar asignado, entregado, consumido, comprobado y devuelto, y tipificar toda diferencia con su causa
**para** que ninguna liquidación cuadre bajando el monto asignado, y para que el sobrante recurrente se lea como lo que es: una estimación mal calibrada, no un motorista honrado

## Contexto

**El monto asignado no se ajusta hacia abajo para que cuadre.** Está congelado desde la entrega y corregirlo es reescribir el pasado. Lo que se explica es la diferencia, no el número de partida.

Y hay un principio que decide qué clase de sistema es este: el **sobrante recurrente en la misma ruta no es un problema del motorista, es una estimación mal calibrada**. El sistema produce el reporte de sobrantes recurrentes por ruta y por vehículo, y el estimado se corrige con vigencia. *Un sistema que solo mide lo que el servidor le debe a la institución no es un sistema de control: es un sistema de cobro.*

La devolución tiene fecha del hecho propia: **es la fecha en que el dinero entró a la caja**, no la del retorno.

## Reglas que la gobiernan

- [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) — Asignado vs. entregado vs. consumido vs. comprobado vs. devuelto; toda diferencia explicada
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — El faltante genera obligación nominada con ciclo propio que sobrevive al cierre
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — La fecha del hecho de la devolución es la del ingreso a caja
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El monto entregado está congelado desde la entrega
- [RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) — La determinación de responsabilidad no nace en la liquidación
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Quien devuelve ≠ quien recibe la devolución, verificado por identidad de persona

## Casos especiales que la afectan

- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — Eje de la historia
- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Misión no ejecutada que igual se liquida
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — Consumo sin comprobante válido

## Criterios de aceptación

```gherkin
# language: es
Característica: Liquidación del fondo de la misión

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "RETORNADA"
    Y una asignación "ASG-2026-00812" con "L 4,800.00" entregados en 6 vales, folios "VC-01201" a "VC-01206"

  Escenario: Se rechaza ajustar hacia abajo el monto asignado para que cuadre
    Dado un consumo comprobado de "L 3,950.00" y un saldo devuelto de "L 500.00"
    Cuando el Jefe de Transporte intenta modificar el monto entregado a "L 4,450.00"
    Entonces el sistema rechaza la modificación
    Y muestra "El monto entregado de L 4,800.00 está congelado desde la entrega del 24/09/2026. Explique la diferencia de L 350.00; no la borre."

  Escenario: Se tipifica el faltante y se nomina al responsable
    Dado un consumo comprobado de "L 3,950.00" y un saldo devuelto de "L 500.00"
    Cuando el Jefe de Transporte tipifica la diferencia de "L 350.00" como "sin causa identificada"
    Entonces el sistema crea una obligación de reintegro de "L 350.00" a cargo de "Wilmer Cáceres"
    Y la obligación tiene ciclo propio que sobrevive al cierre de la misión
    Y "Wilmer Cáceres" queda bloqueado para recibir nueva asignación de fondo

  Escenario: Se rechaza cerrar la conciliación con una diferencia sin tipificar
    Dado una diferencia de "L 350.00"
    Cuando el Jefe de Transporte intenta cerrar la conciliación del fondo sin tipificar la diferencia
    Entonces el sistema rechaza la acción
    Y muestra "Tipifique la diferencia de L 350.00: diferencia explicada y aceptada con respaldo, sin causa identificada, aplicación a fin distinto, o extravío."

  Escenario: La liquidación no determina responsabilidad
    Cuando el Jefe de Transporte tipifica una diferencia como "sin causa identificada"
    Entonces el sistema registra la obligación de reintegro nominada
    Y no declara falta, dolo, negligencia ni responsabilidad administrativa
    Y muestra "Obligación de reintegro registrada. La determinación de responsabilidad corresponde al expediente que corresponda, no a esta liquidación."

  Escenario: La fecha del hecho de la devolución es la del ingreso a caja
    Dado un retorno registrado el "2026-09-26"
    Cuando se registra la devolución de "L 500.00" que entró a caja el "2026-09-30"
    Entonces el sistema registra fecha del hecho "30/09/2026"
    Y no toma la fecha de retorno como fecha del hecho de la devolución

  Escenario: Se rechaza el acta de devolución sin listar los folios uno por uno
    Cuando se registra la devolución indicando solo el monto "L 500.00"
    Entonces el sistema rechaza el acta
    Y muestra "Liste los folios de vale devueltos uno por uno."

  Escenario: Quien devuelve no puede ser quien recibe la devolución
    Cuando "Wilmer Cáceres" registra la recepción de su propia devolución
    Entonces el sistema rechaza el registro
    Y muestra "Wilmer Cáceres devolvió el saldo de OM-2026-0512. No puede figurar como quien lo recibió."

  Escenario: Misión no ejecutada con consumo se liquida igual
    Dado una misión "OM-2026-0530" que nunca salió, con "L 1,040.00" consumidos la tarde anterior
    Cuando el Jefe de Transporte liquida "OM-2026-0530"
    Entonces la conciliación se limita a fondo entregado vs. consumido vs. devuelto
    Y la conciliación galonaje contra kilometraje se marca "no aplica", no "cumplida"
    Y la misión queda marcada "no ejecutada" para no contaminar los indicadores de kilometraje y rendimiento

  Escenario: El sobrante recurrente se lee como estimación mal calibrada
    Dado 5 misiones de la misma ruta con sobrante promedio de "L 620.00" en 90 días
    Cuando el sistema evalúa el período
    Entonces produce el reporte de sobrantes recurrentes por ruta y por vehículo
    Y muestra "Ruta Tegucigalpa–Comayagua: sobrante promedio L 620.00 en 5 misiones. Revise el estimado y corríjalo con vigencia."
    Y no genera ninguna marca contra los motoristas involucrados
```

## Fuera de alcance

- La conciliación de rendimiento — es [HU-088](HU-088-conciliar-galonaje-contra-kilometraje.md)
- La conciliación de peajes — es [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)
- El bloqueo de nueva asignación por reintegro abierto — es [HU-078](HU-078-bloquear-asignacion-a-quien-debe-reintegro.md)
- El cobro del reintegro y cualquier deducción por planilla: fuera de SIGTI
- Los viáticos: los gestiona ARGOS

## Notas y pendientes

- ⚠️ **Hallazgo abierto `HB4-02`.** El estado `LIQUIDADA` de una asignación está definido como *"cuadran asignado, consumido, comprobado y saldo devuelto"*. Leído literalmente, una misión con faltante nunca podría salir de `RETORNADA`, lo que contradice a [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) y produce el abandono del expediente. **Esta historia aplica la lectura de [CU-15](../casos-de-uso/CU-15-liquidar-la-mision-y-conciliar.md): liquidar es declarar el resultado, incluido el faltante con su obligación nominada.** Queda dirigido a [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md), que es la autoridad
- `[C]` **¿El sobrante se devuelve o se arrastra?** — insumo **#7 / `PROP-01`**. El sistema modela ambos esquemas hasta que se confirme
- `[C]` `plazo_devolucion_saldo` y plazo de liquidación en días hábiles — insumo **#32**
- `[C]` `tolerancia_diferencia_liquidacion`, con valor inicial cero — insumo **#1**
