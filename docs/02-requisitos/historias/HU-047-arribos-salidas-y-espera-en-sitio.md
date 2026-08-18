# HU-047 — Registrar arribo y salida por destino y derivar la espera en sitio

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-19 Seguimiento en Ruta |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** declarar con un toque que llegué a un destino y que salgo de él, y que el tiempo que esperé lo calcule el sistema
**para** no cronometrar ni digitar horas en carretera y para que la espera que me hicieron hacer quede atribuida a quien la causó, no a mí

## Contexto

Hoy el tiempo de espera en sitio no queda en ningún lado. Una misión multi-destino que perdió tres horas porque la dependencia receptora no tenía a nadie para recibir la carga aparece en el reporte como una misión lenta, y el indicador castiga al motorista y al Jefe de Transporte por un hecho que no causaron ([CE-08](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md)).

**El estado en ruta lo declara el conductor desde un catálogo cerrado.** El sistema nunca lo infiere de la ausencia de movimiento ni de la ausencia de señal: un vehículo parado puede estar esperando, descargando o averiado, y confundirlos produce un indicador falso ([RN-76](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md)).

Se registra a pleno sol, con guantes, en un celular de gama baja y con la batería contada. Más de un minuto o más de tres toques por registro, y esto se llena en papel ([RNF-12](../no-funcionales/RNF-12-uso-en-campo.md)).

## Reglas que la gobiernan

- [RN-76](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md) — **Regla rectora**: el estado en ruta lo declara el motorista, nunca se infiere; la espera improductiva se tipifica y se atribuye
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Toda la captura se completa sin conectividad
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Hora del hecho y hora de captura son campos distintos
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — Los indicadores se acumulan por causa tipificada y se atribuyen al responsable
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — El odómetro de cada arribo alimenta la serie del expediente del vehículo

## Casos especiales que la afectan

- [CE-08](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) — Multi-destino con esperas prolongadas en sitio
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — Arribo a un destino que no venía en el alcance autorizado

## Criterios de aceptación

```gherkin
# language: es
Característica: Arribo, salida y espera en sitio declarados por el motorista

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con destinos "Delegación de Choluteca" y "Puesto Fronterizo El Amatillo"
    Y un motorista "José Martínez" en el dispositivo portador "DEL-CHO-03"
    Y que el dispositivo lleva 3 días sin conectividad

  Escenario: Se rechaza registrar la salida de un destino al que no se registró arribo
    Cuando "José Martínez" declara la salida del destino "Puesto Fronterizo El Amatillo"
    Entonces el sistema rechaza el registro
    Y muestra "No hay arribo registrado a Puesto Fronterizo El Amatillo. Registre primero la llegada."

  Escenario: Se rechaza el arribo sin odómetro
    Cuando "José Martínez" declara el arribo a "Delegación de Choluteca" sin ingresar odómetro
    Entonces el sistema rechaza el registro
    Y muestra "Falta el kilometraje del tablero al llegar. Es el dato con que se concilia el combustible de este tramo."

  Escenario: El sistema no infiere el estado por falta de movimiento ni de señal
    Dado que el vehículo lleva 5 horas sin reportar posición y sin señal
    Cuando el Jefe de Transporte consulta el estado en ruta de "OM-2026-0451"
    Entonces el sistema muestra el último estado declarado por el motorista y desde cuándo
    Y no declara al vehículo "detenido", "esperando" ni "en anomalía"

  Escenario: El tiempo en sitio se deriva de arribo y salida, sin pedirlo al motorista
    Dado un arribo declarado a "Delegación de Choluteca" a las "09:15" con odómetro "93061"
    Cuando "José Martínez" declara la salida de "Delegación de Choluteca" a las "12:45"
    Entonces el sistema deriva un tiempo en sitio de "3 horas 30 minutos"
    Y en ningún momento solicita al motorista que digite la duración

  Escenario: La espera improductiva se tipifica y se atribuye al destino responsable
    Dado un arribo declarado a "Delegación de Choluteca" a las "09:15"
    Cuando "José Martínez" declara la espera con la causa "destino sin personal para recibir"
    Entonces el sistema registra la espera como improductiva
    Y la atribuye al destino "Delegación de Choluteca" y a la dependencia responsable
    Y la excluye del indicador de puntualidad del motorista

  Esquema del escenario: Qué espera cuenta como improductiva
    Dado un arribo declarado a las "09:15" y una salida a las "12:45"
    Cuando "José Martínez" declara la causa "<causa>"
    Entonces la espera se clasifica como "<clasificacion>"

    Ejemplos:
      | causa                             | clasificacion |
      | destino sin personal para recibir | IMPRODUCTIVA  |
      | descarga en curso                 | PRODUCTIVA    |
      | trámite del destino en proceso    | IMPRODUCTIVA  |
      | tiempo de comida del motorista    | NO_IMPUTABLE  |
```

## Fuera de alcance

- El cálculo del costo-hora del vehículo inmovilizado: depende del insumo #51 y no se implementa con un número inventado
- La visualización del tablero de seguimiento — es [HU-057](HU-057-ultima-posicion-conocida-con-antiguedad.md)
- El arribo a un destino fuera del alcance autorizado — es [HU-055](HU-055-ampliar-alcance-autorizado-en-ruta.md)

## Notas y pendientes

- `[C]` Catálogo institucional de causas de espera y cuáles cuentan como improductivas — insumo #51
- `[C]` Ventana de atención de cada destino, sin la cual no se puede distinguir la espera evitable de la inevitable — insumo #51
