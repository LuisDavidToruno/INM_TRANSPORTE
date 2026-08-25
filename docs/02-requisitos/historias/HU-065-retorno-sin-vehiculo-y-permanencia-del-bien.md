# HU-065 — Registrar el retorno sin vehículo y mantener el bien en el registro

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-12 Incidentes, Siniestros y Sanciones · M-03 Flota Vehicular |
| **Actor** | ACT-06 Motorista · ACT-10 Encargado de Delegación · ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el régimen de responsabilidad patrimonial por el bien sustraído bajo custodia de misión (insumo #47) y el plazo de la obligación de recuperación con el puesto al que se asigna. Sin eso, el retorno sin vehículo no tiene desenlace de responsabilidad |

## Historia

**Como** Jefe de Transporte
**quiero** cerrar la ejecución de una misión en la que el personal volvió pero el vehículo no —porque lo retuvo la autoridad, se lo robaron o quedó siniestrado—
**para** que la misión se pueda liquidar por lo efectivamente ejecutado sin que el bien del Estado desaparezca del registro mientras nadie lo recupera

## Contexto

Es el desenlace que ningún sistema de transporte modela y que la realidad produce: el motorista vuelve en bus, el vehículo se quedó.

**El bien retenido, sustraído o resguardado permanece en el registro hasta su recuperación o descargo, y jamás se declara *dado de baja* por estar retenido** ([RN-75](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)). Declarar de baja un bien que solo está robado es un asiento falso, y es el mecanismo por el que las flotas del Estado se evaporan de los inventarios.

**El expediente de la misión puede haberse ido con el vehículo.** Cuando el dispositivo iba dentro, la reconstrucción se hace desde el servidor con lo último sincronizado, más el papel, más la declaración del motorista — **todo declarado como tal**, nunca presentado como registro de campo.

Y `RN-79` no aplica aquí: **no hay unidad que liberar.** Sí se libera el motorista, salvo impedimento propio.

## Reglas que la gobiernan

- [RN-75](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) — **Regla rectora**: el bien retenido o sustraído permanece en el registro hasta su recuperación o descargo
- [RN-70](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) — Retorno sin vehículo es uno de los cuatro desenlaces tipificados
- [RN-78](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) — La misión declara qué se cumplió, qué no y por qué
- [RN-79](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md) — Se libera al motorista, salvo impedimento propio; no hay vehículo que liberar
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — La custodia se cierra siempre, aun sin el bien presente
- [RN-97](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md) — La obligación de recuperación no terminal al corte pasa al saldo de apertura

## Casos especiales que la afectan

- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — Robo del vehículo o de la carga en misión
- [CE-03](../casos-especiales/CE-03-accidente-de-transito-en-mision.md) — Accidente con retención del vehículo por la autoridad
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — El vehículo queda resguardado en un taller o predio ajeno
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — La obligación de recuperación viva al cierre del ejercicio

## Criterios de aceptación

```gherkin
# language: es
Característica: Retorno sin vehículo

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con marca "interrumpida"
    Y un vehículo "Pickup Hilux" con última lectura de odómetro conocida de "93280" km

  Escenario: Se rechaza el retorno sin vehículo sin expediente de incidente
    Cuando el Jefe de Transporte registra el subtipo "retorno sin vehículo" sin vincular expediente de incidente
    Entonces el sistema rechaza el registro
    Y muestra "El retorno sin vehículo exige expediente de incidente vinculado. Sin él no hay quien responda por el bien."

  Escenario: Se rechaza dar de baja el vehículo por estar retenido
    Dado un vehículo "Pickup Hilux" en estado "NO_DISPONIBLE" por "retención por autoridad"
    Cuando el Encargado de Bienes Institucionales intenta declararlo dado de baja
    Entonces el sistema rechaza la baja
    Y muestra "Pickup Hilux está retenido por autoridad desde el 14/05/2026. Un bien retenido no se da de baja: permanece en el registro hasta su recuperación o descargo."

  Escenario: Se rechaza presentar como leído un odómetro que nadie leyó
    Cuando el Jefe de Transporte registra el odómetro final "93400" sin declararlo estimado
    Entonces el sistema rechaza el registro
    Y muestra "El vehículo no está presente. Declare el kilometraje como estimado y en qué se basa."

  Escenario: Retorno sin vehículo por retención de la autoridad
    Cuando el Jefe de Transporte registra el subtipo "retorno sin vehículo" con causa "retención por autoridad", expediente de incidente vinculado y odómetro estimado "93400"
    Entonces la Orden de Misión "OM-2026-0451" pasa a "RETORNADA"
    Y el vehículo "Pickup Hilux" pasa a "NO_DISPONIBLE" con causa tipificada
    Y permanece en el registro de la flota con la obligación de recuperación abierta, con responsable y plazo
    Y el odómetro se presenta en el expediente y en todo reporte como "estimado", nunca como leído

  Escenario: El motorista se libera aunque el vehículo no vuelva
    Cuando se registra el retorno sin vehículo de "OM-2026-0451"
    Entonces "José Martínez" queda disponible como motorista
    Y el sistema no espera la recuperación del vehículo para liberarlo

  Escenario: El motorista no se libera si hay evento de incapacidad
    Dado un evento de incapacidad del conductor registrado el "2026-05-14"
    Cuando se registra el retorno sin vehículo de "OM-2026-0451"
    Entonces "José Martínez" no queda disponible como motorista
    Y muestra "José Martínez no se reincorpora hasta que se resuelva el evento de salud registrado el 14/05/2026."

  Escenario: El expediente se fue con el vehículo y se reconstruye
    Dado que el dispositivo portador iba dentro del vehículo sustraído
    Cuando el Jefe de Transporte reconstruye la bitácora con lo último sincronizado, el papel y la declaración del motorista
    Entonces el sistema declara cada fuente de cada dato reconstruido
    Y no presenta ninguno de esos datos como registro de campo del dispositivo
    Y la misión solo puede cerrarse con hallazgo

  Escenario: La obligación de recuperación sigue viva al cierre del ejercicio
    Dada una obligación de recuperación abierta desde el "2026-05-14"
    Cuando la Gerencia Administrativa ejecuta el cierre del ejercicio "2026"
    Entonces el sistema incorpora la obligación al saldo de apertura del ejercicio "2027"
    Y le calcula la antigüedad desde el "2026-05-14", no desde la fecha de cierre
    Y no cambia el estado del expediente por efecto de la fecha
```

## Fuera de alcance

- La investigación del incidente y su ciclo en M-12 — se abre desde aquí, se gestiona allá
- La determinación de responsabilidad patrimonial por el bien sustraído — depende del insumo #47 según `CU-09`
- El estado terminal `RETIRADO_DE_FLOTA`, que hoy no existe en la máquina de estados del vehículo — hallazgo abierto
- El trámite ante la aseguradora

## Notas y pendientes

- `[C]` Responsabilidad patrimonial por el bien sustraído bajo custodia de misión — insumo #47 según `CU-09`
- `[C]` Plazo de la obligación de recuperación y a quién se asigna por puesto
- **Hallazgo abierto:** `T-18` no tipifica el **retorno del personal con el vehículo resguardado en sitio**, que hoy se registra como *retorno sin vehículo* y dice algo distinto: el vehículo existe, está identificado y hay obligación de recuperarlo. Reportado desde `CU-09` al índice de casos especiales
