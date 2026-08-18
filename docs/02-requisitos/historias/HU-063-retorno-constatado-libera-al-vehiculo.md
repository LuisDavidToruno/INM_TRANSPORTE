# HU-063 — Constatar el retorno en oficina y liberar el vehículo sin esperar la bitácora

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-03 Flota Vehicular |
| **Actor** | ACT-10 Encargado de Delegación · ACT-05 Encargado de Despacho |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Encargado de Delegación
**quiero** constatar físicamente que el vehículo volvió al predio y registrarlo aunque el motorista no haya podido registrar nada
**para** poder asignar esa unidad a la misión de mañana sin que un trámite de digitación me deje la mitad de la flota secuestrada

## Contexto

El vehículo entró al predio y no hay registro del motorista: se quedó sin batería, no llevaba dispositivo, o la zona no tenía señal ([CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)).

El control que interesa al Tribunal Superior de Cuentas **no es que el vehículo esté congelado**: es que el kilometraje, el combustible y la custodia queden completos **antes de liquidar**. Por eso la marca `BITACORA_PENDIENTE_DE_DIGITACION` **bloquea la liquidación y no bloquea la asignación del vehículo**. El control se ejerce donde tiene sentido —en el dinero— y no donde solo estorba ([RN-79](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md)).

**Aquí el odómetro se comporta distinto que en el retorno ordinario.** En el `T-18` ordinario, un odómetro menor al de salida es error de digitación y **bloquea**. En el subtipo *retorno constatado*, la lectura es **evidencia física tomada por un tercero identificado con fotografía del tablero**: se registra tal cual, se marca la inconsistencia y **el vehículo se libera igual** — porque ya está en el predio, y bloquear solo lo deja secuestrado por un trámite. Esta distinción resuelve la tensión reportada entre `BD-05` y `RN-79`.

## Reglas que la gobiernan

- [RN-79](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md) — **Regla rectora**: el retorno físico constatado libera vehículo y motorista sin esperar la digitación
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) · [RN-90](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md) — El kilometraje acumulado es del expediente, no del instrumento
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho —la que consta o se constata— distinta de la de captura, no editable
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — La custodia se cierra con acta y novedades declaradas o su falta declarada
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Lo constatado y lo que después diga el papel son dos versiones; ninguna sobrescribe a la otra
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Quien constata no es el motorista que retorna

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Bitácora en papel digitada días después
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — Odómetro inconsistente
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — El fondo no devuelto, que sí bloquea la salida siguiente

## Criterios de aceptación

```gherkin
# language: es
Característica: Retorno constatado en oficina

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con odómetro de salida "92480"
    Y un vehículo "Pickup Hilux" en estado operativo "EN_MISION"
    Y que el motorista "José Martínez" no registró el retorno porque el dispositivo se quedó sin batería

  Escenario: Se rechaza la constatación sin fotografía del tablero
    Cuando el Encargado de Delegación registra el retorno constatado con odómetro "93610" sin fotografía del tablero
    Entonces el sistema rechaza el registro
    Y muestra "El retorno constatado exige fotografía del tablero. La lectura la está tomando usted, no el motorista: la foto es lo que la sostiene."

  Escenario: Se rechaza que el propio motorista constate su retorno
    Cuando "José Martínez" intenta ejecutar el retorno constatado en oficina de su propia misión
    Entonces el sistema rechaza la ejecución
    Y muestra "El retorno constatado lo registra quien recibe el vehículo, no quien lo condujo."

  Escenario: Se rechaza liquidar con la bitácora pendiente de digitación
    Dada la Orden de Misión "OM-2026-0451" en estado "RETORNADA" con marca "BITACORA_PENDIENTE_DE_DIGITACION"
    Cuando el Jefe de Transporte intenta liquidar la misión
    Entonces el sistema rechaza la liquidación
    Y muestra "La bitácora de OM-2026-0451 está pendiente de digitación desde hace 3 días. Responsable: Encargado de Delegación de Choluteca."

  Escenario: La constatación libera el vehículo de inmediato
    Cuando el Encargado de Delegación registra el retorno constatado con odómetro leído "93610", fotografía del tablero, hora del hecho "2026-05-16 21:00" y acta de recepción
    Entonces la Orden de Misión "OM-2026-0451" pasa a "RETORNADA" con marca "BITACORA_PENDIENTE_DE_DIGITACION"
    Y el vehículo "Pickup Hilux" sale de "EN_MISION" y pasa a "DISPONIBLE"
    Y "José Martínez" queda disponible como motorista
    Y la fecha de captura se registra no editable y distinta de la fecha del hecho

  Escenario: El vehículo se asigna a una misión nueva con la bitácora todavía pendiente
    Dada la marca "BITACORA_PENDIENTE_DE_DIGITACION" sobre "OM-2026-0451"
    Cuando el Encargado de Despacho asigna "Pickup Hilux" a la Orden de Misión "OM-2026-0470"
    Entonces el sistema acepta la asignación
    Y toma el odómetro de salida del tablero, no del sistema
    Y muestra "Pickup Hilux tiene una bitácora pendiente de digitación de OM-2026-0451. No impide esta salida."

  Escenario: Odómetro constatado menor al de salida — se registra igual y el vehículo se libera
    Cuando el Encargado de Delegación registra el retorno constatado con odómetro leído "92300" y fotografía del tablero
    Entonces el sistema registra la lectura tal cual, sin bloquear
    Y marca la inconsistencia con "El kilometraje leído (92,300) es menor al de salida (92,480). Se registró tal como está en el tablero y queda para resolver."
    Y el vehículo "Pickup Hilux" pasa igualmente a "DISPONIBLE"
    Y la inconsistencia entra a la cola de resolución humana y bloquea la liquidación

  Escenario: Odómetro constatado menor con el instrumento declarado averiado
    Dado un evento previo de "odómetro averiado en ruta" registrado el "2026-05-14"
    Cuando el Encargado de Delegación registra el retorno constatado con odómetro leído "92300"
    Entonces el sistema registra la lectura como "estimado, instrumento declarado averiado"
    Y el vehículo pasa a "NO_DISPONIBLE" por instrumento de medición averiado
    Y muestra "El odómetro está declarado averiado. El vehículo no se asigna hasta que se intervenga el instrumento con acta."

  Escenario: El fondo no devuelto sí bloquea la salida siguiente
    Dada una obligación de reintegro abierta por "L 1,200.00" a nombre de "José Martínez" por la misión "OM-2026-0451"
    Cuando el Encargado de Despacho intenta asignar a "José Martínez" a la Orden de Misión "OM-2026-0470"
    Entonces el sistema rechaza la asignación
    Y muestra "José Martínez tiene L 1,200.00 sin devolver ni comprobar de la misión OM-2026-0451. Es un bloqueo de dinero, no de trámite."
```

## Fuera de alcance

- La digitación posterior de la bitácora desde el papel — es [HU-064](HU-064-digitacion-diferida-desde-el-papel.md)
- La resolución del conflicto entre lo constatado y lo que después traiga el papel — es [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- La obligación de reintegro y su ciclo — es de M-09 y M-13

## Notas y pendientes

- `[C]` Plazo máximo de digitación diferida antes de que la misión cierre con hallazgo — insumo #32
- `[C]` ¿Puede digitar formularios en papel quien después liquida esa misma misión? En una delegación de tres personas es la misma persona — insumo #47, a Auditoría Interna
- **Hallazgo reportado y resuelto en el alcance de esta historia:** la tensión entre `BD-05` (bloqueo duro de captura) y `RN-79` (la constatación se registra igual) se resuelve distinguiendo el `T-18` **ordinario** del subtipo **retorno constatado**. La decisión corresponde a `orden-de-mision.md` como autoridad en bloqueos: esta historia la refleja, no la crea
