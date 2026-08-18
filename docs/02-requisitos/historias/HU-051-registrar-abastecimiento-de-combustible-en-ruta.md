# HU-051 — Registrar el abastecimiento de combustible en ruta con su fuente declarada

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** registrar cada carga de combustible con galones, monto, estación, odómetro y foto del comprobante, aunque esté sin señal
**para** que la conciliación galonaje–kilometraje se haga con lo que realmente ocurrió y no me quede a mí un faltante que no puedo explicar

## Contexto

La conciliación de combustible contra kilometraje es el hallazgo más frecuente de auditoría en transporte del Estado. Depende de un dato que solo existe si se captura en la bomba: **el odómetro al momento de cargar**. Si se anota "de memoria" al retornar, el rendimiento por tramo no se puede calcular y toda la conciliación se vuelve un promedio sin valor probatorio.

**Todo ingreso de combustible es un abastecimiento con fuente declarada** — fondo de la misión, tanque institucional, peculio propio o donación en sitio. El nivel de tanque es dato de bitácora, no sustituto del abastecimiento ([RN-83](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)). Sin la fuente, un galón donado y un galón pagado con fondos públicos pesan igual en el reporte, y no lo son.

## Reglas que la gobiernan

- [RN-83](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) — **Regla rectora**: todo ingreso de combustible se registra como abastecimiento con fuente declarada
- [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) — Consumo con galones, monto, estación, odómetro y fotografía del comprobante
- [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) — La conciliación vigila la desviación en ambas direcciones
- [RN-84](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md) — El mismo comprobante no puede sostener dos consumos en la institución; se bloquea al registrarlo
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — El odómetro de la carga entra en la serie del expediente del vehículo
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Se registra sin ninguna conectividad

## Casos especiales que la afectan

- [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — Galonaje que no cuadra con el kilometraje
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — La estación no da factura o el comprobante se perdió — ver [HU-052](HU-052-ausencia-de-comprobante-y-gasto-imprevisto.md)
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — El odómetro de la carga es menor que la última lectura conocida — ver [HU-053](HU-053-odometro-menor-a-la-ultima-lectura.md)

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro de abastecimiento de combustible en ruta

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un vehículo "Pickup Hilux" con última lectura de odómetro conocida de "93061" km
    Y un fondo de la misión asignado por "L 4,500.00"
    Y que el dispositivo lleva 4 días sin conectividad

  Escenario: Se rechaza el abastecimiento sin fuente declarada
    Cuando "José Martínez" registra un abastecimiento de "15.0" galones sin declarar la fuente
    Entonces el sistema rechaza el registro
    Y muestra "Declare de dónde salió este combustible: fondo de la misión, tanque institucional, su propio bolsillo o donación en sitio. Sin fuente, el galón no se puede descargar."

  Escenario: Se rechaza el abastecimiento sin odómetro al momento de cargar
    Cuando "José Martínez" registra un abastecimiento de "15.0" galones sin ingresar odómetro
    Entonces el sistema rechaza el registro
    Y muestra "Falta el kilometraje del tablero al momento de cargar. Sin él no se puede calcular el rendimiento de este tramo."

  Escenario: Se rechaza reutilizar un comprobante ya registrado en la institución
    Dado un comprobante con emisor "UNO Choluteca" y número "F-0087412" ya registrado en la Orden de Misión "OM-2026-0430"
    Cuando "José Martínez" registra un abastecimiento con ese mismo comprobante
    Entonces el sistema rechaza el registro
    Y muestra "El comprobante UNO Choluteca F-0087412 ya está registrado en la Orden de Misión OM-2026-0430 del 03/05/2026. Verifique el número de la factura."

  Escenario: Abastecimiento con fondo de la misión, sin señal
    Cuando "José Martínez" registra un abastecimiento de "15.0" galones por "L 1,650.00" en la estación "UNO Choluteca", con odómetro "93280" y fotografía del comprobante
    Entonces el sistema registra el abastecimiento con fuente "fondo de la misión"
    Y descuenta "L 1,650.00" del saldo del fondo de la misión
    Y deja el registro en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: Abastecimiento pagado de peculio propio
    Cuando "José Martínez" registra un abastecimiento de "8.0" galones por "L 880.00" con fuente "peculio propio" y fotografía del comprobante
    Entonces el sistema registra el abastecimiento sin afectar el saldo del fondo de la misión
    Y abre la obligación de reintegro a favor de "José Martínez" por "L 880.00"
    Y muestra "Registrado como pago de su bolsillo. Presente el comprobante original al liquidar."

  Escenario: El nivel de tanque no sustituye el registro del abastecimiento
    Dado un nivel de tanque registrado en bitácora de "3/4" a las "16:00"
    Cuando el Encargado de Combustible concilia la Orden de Misión "OM-2026-0451"
    Entonces el sistema no computa ningún galón derivado del nivel de tanque
    Y considera únicamente los abastecimientos registrados con fuente declarada
```

## Fuera de alcance

- La asignación y entrega del fondo de combustible antes de salir — es de M-09 en el despacho
- La conciliación galonaje–kilometraje y sus umbrales de desviación — es de M-13, y sus umbrales dependen del insumo #32
- El circuito de reembolso del combustible pagado de peculio propio — depende del insumo #37

## Notas y pendientes

- `[C]` ¿La institución admite y reembolsa combustible pagado por el motorista de su bolsillo, y en qué plazo? — insumo #37
- `[C]` Umbrales de desviación de consumo, superior e inferior, que son independientes entre sí — insumo #32
- `[C]` Rendimiento esperado por tipo de vehículo: es parámetro con vigencia por fecha, nunca un número fijo en el código
