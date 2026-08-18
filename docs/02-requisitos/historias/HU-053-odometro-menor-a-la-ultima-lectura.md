# HU-053 — Bloquear el odómetro menor a la última lectura conocida durante la ejecución

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-03 Flota Vehicular |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** que el dispositivo me avise en el momento cuando digito un kilometraje menor al último registrado de ese vehículo
**para** corregir el dedazo ahí mismo, en lugar de que me lo devuelvan tres semanas después cuando ya no recuerdo qué marcaba el tablero

## Contexto

El kilometraje es la base de la conciliación de combustible, del programa de mantenimiento preventivo y del control de uso del vehículo. Un dígito equivocado en la bomba corrompe los tres.

**Aquí bloquear es corregir, no ocultar.** Es la única excepción al principio de que el sistema nunca impide registrar un hecho: un odómetro que retrocede no es un hecho, es un error material de digitación o un instrumento intervenido. `BD-05` bloquea la captura de ese valor, en el dispositivo y sin conectividad.

**La única salida legítima es el acta previa de sustitución o reinicio del odómetro**, levantada por el Encargado de Mantenimiento antes de la salida, con la lectura del instrumento retirado y del instalado. Entonces el kilometraje acumulado se calcula sumando tramos ([RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md), [RN-90](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md)).

**Esta historia cubre el bloqueo en la captura de ruta y en el `T-18` ordinario.** El subtipo *retorno constatado en oficina* se comporta distinto y está en [HU-063](HU-063-retorno-constatado-libera-al-vehiculo.md).

## Reglas que la gobiernan

- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — **Regla rectora**: el kilometraje acumulado es atributo del expediente del vehículo, independiente de la lectura del instrumento
- [RN-90](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md) — Toda intervención del odómetro es evento con orden de trabajo y autorización nominativa
- [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) — Coherencia del odómetro de retorno
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La validación se ejecuta en el dispositivo, sin consultar al servidor

## Casos especiales que la afectan

- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — Odómetro inconsistente
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — El odómetro intermedio que nadie anotó en el papel

## Criterios de aceptación

```gherkin
# language: es
Característica: Coherencia del odómetro en la captura de ruta

  Antecedentes:
    Dado un vehículo "Pickup Hilux" con última lectura de odómetro conocida de "93061" km al "2026-05-13"
    Y la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con odómetro de salida "92480"
    Y que el dispositivo lleva 4 días sin conectividad

  Escenario: Se bloquea el odómetro menor a la última lectura conocida
    Cuando "José Martínez" registra un abastecimiento con odómetro "9306"
    Entonces el sistema rechaza la captura del valor
    Y muestra "El último kilometraje registrado de este vehículo es 93,061. El que está ingresando es 9,306, que es menor. Verifique el tablero."
    Y no guarda el registro con ese valor

  Escenario: El bloqueo se ejecuta sin ninguna conectividad
    Dado que el dispositivo lleva 7 días sin conectividad
    Cuando "José Martínez" registra un arribo con odómetro "92900"
    Entonces el sistema rechaza la captura del valor
    Y muestra "El último kilometraje registrado de este vehículo es 93,061. El que está ingresando es 92,900, que es menor. Verifique el tablero."
    Y no requiere consultar al servidor para aplicar el bloqueo

  Escenario: Se acepta el valor menor si existe acta previa de sustitución del odómetro
    Dada un acta de sustitución de odómetro registrada por el Encargado de Mantenimiento el "2026-05-13", con lectura del instrumento retirado "93061" y del instalado "0"
    Cuando "José Martínez" registra un arribo con odómetro "215"
    Entonces el sistema acepta el registro
    Y calcula el kilometraje acumulado del expediente sumando tramos: "93,061 + 215"
    Y muestra "Odómetro nuevo instalado el 13/05/2026. Kilometraje acumulado del vehículo: 93,276 km."

  Escenario: El odómetro se avería durante la misión
    Cuando "José Martínez" declara el evento "odómetro averiado en ruta" con última lectura válida "93280" y hora del hecho
    Entonces el sistema registra el evento
    Y los tramos siguientes admiten kilometraje declarado como estimado
    Y todo valor posterior se presenta en el expediente y en los reportes como "estimado", nunca como leído

  Escenario: Odómetro igual al anterior en un tramo con recorrido declarado
    Cuando "José Martínez" registra un arribo a "Puesto Fronterizo El Amatillo" con odómetro "93061"
    Entonces el sistema acepta el registro
    Y exige justificación con causa tipificada
    Y marca el tramo para revisión con "El kilometraje no cambió desde el registro anterior. Explique por qué."
```

## Fuera de alcance

- El levantamiento del acta de sustitución o reinicio del odómetro — es de M-11 y del Encargado de Mantenimiento
- El bloqueo en el subtipo *retorno constatado en oficina*, que se comporta distinto — es [HU-063](HU-063-retorno-constatado-libera-al-vehiculo.md)
- La conciliación del odómetro con la serie ordenada por fecha del hecho al digitar tarde — es [HU-064](HU-064-digitacion-diferida-desde-el-papel.md)

## Notas y pendientes

- `[C]` Plazo máximo de operación con odómetro averiado antes de que el vehículo salga de circulación — insumo registrado desde `RN-90`
- `[I]` La tolerancia de "odómetro igual" para tramos cortos dentro del mismo predio es parámetro con vigencia por fecha, no un número fijo
