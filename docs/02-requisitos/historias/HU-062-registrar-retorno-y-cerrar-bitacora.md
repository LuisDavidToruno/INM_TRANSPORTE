# HU-062 — Registrar el retorno y cerrar la bitácora desde el dispositivo, sin red y sin oficina

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** registrar yo mismo el retorno del vehículo, el kilometraje final y el acta de recepción, un sábado a las 21:00 y sin señal
**para** que la unidad quede libre para la siguiente misión sin esperar a que el lunes llegue alguien de oficina a teclear lo que yo ya sé

## Contexto

**`T-14` y `T-18` las ejecuta el motorista en su dispositivo y sin conectividad. No las ejecuta el despachador.** Una delegación sin despachador un sábado a las 21:00 tiene que poder registrar el retorno.

La razón es aritmética: en una delegación con dos vehículos, dejar una unidad inmovilizada porque la bitácora todavía no se digitó suprime el 50 % de la capacidad de transporte por una razón administrativa. La consecuencia observada es que **la siguiente salida se hace sin Orden de Misión** — con lo cual el sistema no solo no controló ese viaje: empujó a que ocurriera fuera de él.

**Quien constata el retorno no puede ser el motorista que retorna** ([RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)). Pero si en la delegación no hay otra persona, se registra así, con motivo, y el hecho entra al indicador de la delegación — **no bloquea el retorno**.

## Reglas que la gobiernan

- [RN-79](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md) — **Regla rectora**: el retorno libera vehículo y motorista sin esperar la digitación de la bitácora
- [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) · [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — Coherencia del odómetro de retorno y del kilometraje acumulado
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) · [RN-71](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) — La custodia se devuelve con constancia y se cierra siempre
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) · [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La bitácora cerrada no admite eventos nuevos; toda corrección es asiento
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Quien constata no es quien retorna
- [RN-78](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) — La misión declara el grado de cumplimiento de su objeto, por destino y consolidado
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Se ejecuta sin ninguna conectividad

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Bitácora en papel digitada días después
- [CE-07](../casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) — Retorno anticipado: la misión se aborta
- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — La misión no se ejecutó pero hubo consumo
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — Odómetro de retorno menor al de salida

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro del retorno y cierre de la bitácora

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con odómetro de salida "92480"
    Y un vehículo "Pickup Hilux" en estado operativo "EN_MISION"
    Y un motorista "José Martínez" en el dispositivo portador "DEL-CHO-03"
    Y que es sábado "2026-05-16" a las "21:00" y no hay nadie de oficina en la delegación
    Y que el dispositivo lleva 4 días sin conectividad

  Escenario: Se bloquea el odómetro de retorno menor al de salida
    Cuando "José Martínez" registra un odómetro de retorno de "92300"
    Entonces el sistema rechaza la captura
    Y muestra "El kilometraje de retorno (92,300) es menor al de salida (92,480). Verifique la lectura del tablero."
    Y no permite continuar el registro con ese valor

  Escenario: Se rechaza el subtipo de retorno distinto de normal sin motivo
    Cuando "José Martínez" selecciona el subtipo "retorno anticipado" sin motivo
    Entonces el sistema rechaza el registro
    Y muestra "El retorno anticipado exige motivo. Es lo que sustenta liquidar por lo efectivamente ejecutado."

  Escenario: Se rechaza registrar un evento de ruta después de cerrada la bitácora
    Dada la Orden de Misión "OM-2026-0451" ya en estado "RETORNADA"
    Cuando "José Martínez" intenta registrar un abastecimiento con fecha del hecho anterior al retorno
    Entonces el sistema rechaza el registro en la bitácora cerrada
    Y muestra "La bitácora de OM-2026-0451 está cerrada. Este hecho se registra como asiento de corrección, con motivo y respaldo."

  Escenario: El motorista registra el retorno completo un sábado, sin red ni oficina
    Cuando "José Martínez" registra el odómetro de retorno "93610" con fotografía del tablero, hora real "21:00" y acta de recepción con novedades
    Entonces la Orden de Misión "OM-2026-0451" pasa a estado "RETORNADA"
    Y la bitácora se cierra y no admite eventos nuevos
    Y el vehículo "Pickup Hilux" sale de "EN_MISION" y pasa a "DISPONIBLE"
    Y "José Martínez" vuelve a estar disponible como motorista
    Y todo el registro queda en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: No hay segunda persona para constatar el retorno
    Cuando "José Martínez" declara que no hay otra persona en la delegación para constatar el retorno, con motivo
    Entonces el sistema registra el retorno igual
    Y marca "constatación por segunda persona: no disponible en la delegación"
    Y el hecho entra al indicador de la delegación
    Y no bloquea el retorno

  Escenario: Se rechaza que el propio motorista figure como quien constata
    Cuando "José Martínez" intenta registrarse a sí mismo como quien constata el retorno
    Entonces el sistema rechaza el registro
    Y muestra "Quien retorna no puede constatar su propio retorno. Registre a otra persona o declare que no hay ninguna disponible."

  Escenario: Odómetro de retorno igual al de salida en misión ejecutada
    Cuando "José Martínez" registra un odómetro de retorno de "92480"
    Entonces el sistema acepta el registro
    Y exige justificación con causa tipificada
    Y marca la misión para revisión con "El vehículo retornó con el mismo kilometraje de salida. Explique por qué."
    Y genera el hallazgo "H-02" si no se justifica

  Escenario: La misión no se ejecutó pero el fondo ya se consumió
    Dada la Orden de Misión "OM-2026-0452" en estado "DESPACHADA" con "L 1,650.00" ya consumidos del fondo
    Cuando el Encargado de Delegación registra que la misión no se ejecutó, con motivo tipificado
    Entonces la misión pasa a "RETORNADA" y no se anula
    Y se cierra la bitácora sin eventos de ruta
    Y la misión queda marcada como "no ejecutada" para no contaminar los indicadores de kilometraje y rendimiento
```

## Fuera de alcance

- El retorno constatado en oficina, que se comporta distinto ante el odómetro — es [HU-063](HU-063-retorno-constatado-libera-al-vehiculo.md)
- El retorno sin vehículo — es [HU-065](HU-065-retorno-sin-vehiculo-y-permanencia-del-bien.md)
- La conciliación disparada por el retorno y la liquidación — son de M-13
- La entrega física del sobrante del fondo y de los comprobantes al Encargado de Combustible: se registra aquí, se liquida en M-09

## Notas y pendientes

- `[C]` Plazo de liquidación, desde cuándo corre y en qué calendario de días hábiles — insumo #32
- `[C]` Quién puede ordenar el retorno anticipado — insumo #50 según `CU-10`
- `[C]` Umbrales de desviación de kilometraje que disparan `H-01` y `H-02` — insumo #1
