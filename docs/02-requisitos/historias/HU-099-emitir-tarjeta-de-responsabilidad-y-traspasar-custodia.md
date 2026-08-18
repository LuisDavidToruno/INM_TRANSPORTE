# HU-099 — Emitir la tarjeta de responsabilidad y traspasar la custodia del vehículo con acta

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular |
| **Actor** | ACT-14 Encargado de Bienes Institucionales · ACT-13 Custodio del Vehículo |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Encargado de Bienes Institucionales
**quiero** emitir la tarjeta de responsabilidad designando al custodio, y traspasarla con acta de entrega-recepción cuando la persona rote
**para** que en cualquier momento del pasado se pueda decir quién respondía por la unidad, que es lo primero que se pregunta cuando aparece un daño o falta una herramienta

## Contexto

La **tarjeta de responsabilidad** determina sobre quién recae la deducción de responsabilidad. No es un campo del catálogo del vehículo: es un acto con consecuencia jurídica.

Un vehículo del Estado sin responsable identificado es un hallazgo esperando ocurrir. Por eso la custodia vacante bloquea el despacho transcurrido un plazo, y por eso la custodia viva bloquea el cierre de la asignación de puesto de quien se va. Es incómodo y es correcto.

La lectura de odómetro del acta inicial constituye el **primer valor del kilometraje acumulado del expediente**.

## Reglas que la gobiernan

- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — Custodio vigente siempre, con acta de entrega-recepción e historial consultable por fecha
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — La lectura del acta constituye el primer valor del acumulado
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El registro anterior no se sobrescribe: se cierra su rango y se abre el nuevo
- [RN-75](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) — El bien retenido por un tercero traslada la custodia a ese tercero, con acta
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Autor, puesto, momento y huella del contenido

## Casos especiales que la afectan

- [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) — El vehículo asignado permanentemente a un funcionario tiene custodio propio
- [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — La custodia se traslada al tercero que retiene el bien
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — El ingreso a taller también mueve la custodia

## Criterios de aceptación

```gherkin
# language: es
Característica: Tarjeta de responsabilidad y custodia del vehículo

  Antecedentes:
    Dado un vehículo "TR-0092" con título de tenencia vigente y ficha técnica completa
    Y "Karla Ordóñez" designada como custodia

  Escenario: Se rechaza la tarjeta de responsabilidad sin acta con odómetro
    Cuando el Encargado de Bienes emite la tarjeta de responsabilidad sin registrar el odómetro
    Entonces el sistema rechaza la emisión
    Y muestra "El acta de entrega-recepción exige odómetro, nivel de combustible, accesorios, herramientas y estado de la unidad."

  Escenario: La lectura del acta inicial constituye el kilometraje acumulado
    Cuando el Encargado de Bienes emite la tarjeta de responsabilidad de "TR-0092" con odómetro "12,450" km
    Entonces el kilometraje acumulado del expediente de "TR-0092" queda en "12,450" km
    Y la custodia de "Karla Ordóñez" inicia en la fecha del acta

  Escenario: Se rechaza el traspaso sin firma del custodio saliente y del entrante
    Cuando el Encargado de Bienes registra el traspaso a "Julio Meza" sin la firma de "Karla Ordóñez"
    Entonces el sistema rechaza el traspaso
    Y muestra "El acta de entrega-recepción exige la firma del custodio saliente y del entrante."

  Escenario: La diferencia entre lo entregado y lo devuelto genera novedad
    Dado un acta inicial con "1" llanta de repuesto, "1" gato y "1" juego de herramientas
    Cuando el custodio entrante recibe sin el juego de herramientas
    Entonces el sistema registra una novedad vinculada al expediente de incidentes
    Y muestra "Falta el juego de herramientas registrado en el acta del 15/01/2026. Se generó novedad NOV-2026-0088."

  Escenario: El historial de custodias es consultable por fecha
    Cuando se consulta quién respondía por "TR-0092" el "2026-04-18"
    Entonces el sistema devuelve "Karla Ordóñez", con la fecha de inicio y de fin de su custodia
    Y no devuelve al custodio actual

  Escenario: El registro anterior no se sobrescribe
    Cuando el Encargado de Bienes registra el traspaso a "Julio Meza"
    Entonces el rango de custodia de "Karla Ordóñez" se cierra
    Y se abre un rango nuevo para "Julio Meza"
    Y el registro de "Karla Ordóñez" permanece íntegro

  Escenario: La custodia viva bloquea el cierre de la asignación de puesto
    Dado que "Karla Ordóñez" custodia 3 vehículos
    Cuando se intenta cerrar su asignación de puesto por traslado
    Entonces el sistema bloquea el cierre
    Y muestra "Karla Ordóñez custodia 3 vehículos: TR-0045, TR-0092 y TR-0098. Traspase la custodia antes de cerrar la asignación de puesto."

  Escenario: La custodia vacante alerta y bloquea el despacho tras el plazo
    Dado que el custodio de "TR-0092" cesó en el cargo el "2026-09-01"
    Y un plazo configurable de "15" días
    Cuando el Encargado de Despacho intenta despachar "TR-0092" el "2026-09-24"
    Entonces el sistema rechaza el despacho
    Y muestra "TR-0092 tiene custodia vacante desde el 01/09/2026. Designe custodio antes de despachar."

  Escenario: El bien retenido por un tercero no queda sin custodio
    Dado un vehículo "TR-0098" retenido por autoridad el "2026-09-20"
    Cuando el Encargado de Bienes registra la retención
    Entonces la custodia se traslada al tercero que lo retiene, con acta y fecha
    Y el vehículo no queda sin custodio ni sigue formalmente bajo el motorista
    Y el vehículo permanece en el registro

  Escenario: El Jefe de Transporte no emite la tarjeta de responsabilidad
    Cuando el Jefe de Transporte intenta emitir la tarjeta de responsabilidad de "TR-0092"
    Entonces el sistema rechaza la acción
    Y muestra "La tarjeta de responsabilidad es competencia de la unidad de Bienes."
```

## Fuera de alcance

- El acta de entrega del vehículo al motorista dentro del despacho: es un traslado de custodia **operativa** de la misión, no de la tarjeta de responsabilidad (M-07/M-08)
- La deducción de responsabilidad por daños: fuera de SIGTI
- El alta del vehículo — es [HU-096](HU-096-dar-de-alta-el-vehiculo-con-titulo-de-tenencia.md)
- La constatación de identificación — es [HU-100](HU-100-constatar-la-identificacion-institucional.md)

## Notas y pendientes

- `[C]` Formato en papel de la tarjeta de responsabilidad y del acta de entrega-recepción vigentes en la institución — insumo **#2**
- `[C]` Plazo configurable de custodia vacante antes de bloquear el despacho — insumo **#1**
- `[C]` Régimen de asignación permanente de vehículo a funcionario y su efecto sobre la custodia — insumo **#64**
- `[P]` La tarjeta de responsabilidad proviene de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md); el Manual de Propiedad Estatal la regula pero **no se pudo extraer el articulado**. **No se eleva el nivel**
