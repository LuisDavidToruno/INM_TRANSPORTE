# HU-088 — Conciliar galonaje contra kilometraje con umbrales independientes en ambas direcciones

| Campo | Valor |
|---|---|
| **Módulo** | M-13 Liquidación y Cierre · M-09 Combustible |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — faltan `rendimiento_esperado` por vehículo con sus variantes por tipo de ruta y los seis umbrales de la escala de clasificación (insumos #1 y #19). Sin ellos el control produce hallazgos falsos y en tres meses nadie los mira |

## Nota de corrección — hallazgo `HB34-07`

> **Qué estaba mal.** Los `Antecedentes` declaraban dos umbrales —15 % superior, 20 % inferior— y la tabla del esquema del escenario clasificaba como `REVISAR` un caso de **16,7 % por debajo**, que con esos mismos umbrales es `CONFORME`. Y `REVISAR` era una tercera clasificación **sin ningún umbral que la definiera**: un revisor externo no podía determinar si `360 km / 36 galones` cumple o no cumple. Es el criterio central del control que más mira el auditor.
>
> Origen probable: la tabla se heredó de la plantilla de criterios Gherkin, que usa un solo umbral del 15 %. Al desdoblar el umbral en superior e inferior, la tabla no se recalculó.
>
> **Qué se corrige.** Se declara la escala completa —**tolerancia, banda de revisión y umbral de hallazgo, en ambas direcciones**— y **se recalcula la tabla contra los umbrales declarados**, no al revés. La fila de 16,7 % por debajo pasa a `CONFORME`, que es lo que los umbrales de la propia historia dicen, y se agregan filas que caen efectivamente en cada banda. Los valores siguen siendo ilustrativos y parámetros con vigencia (`RN-39`); lo que se corrige es que ahora son **coherentes entre sí**.
>
> **Qué es `REVISAR`, que antes no estaba escrito.** No es un hallazgo: es una desviación fuera de tolerancia que **exige tipificación de causa antes del cierre** y entra al indicador agregado por vehículo, **sin impedir el cierre limpio**. El hallazgo —`CONSUMO_EXCEDIDO` o `RENDIMIENTO_ANOMALO_SUPERIOR`— sí lo impide.

## Historia

**Como** Jefe de Transporte
**quiero** que la liquidación calcule el rendimiento observado contra el esperado del vehículo y marque la desviación **en ambas direcciones**, con umbrales superior e inferior independientes
**para** responder al hallazgo que el TSC va a levantar —incremento de consumo sin relación con el uso habitual— antes de que lo levante, y para detectar el despacho que nadie anotó

## Contexto

**Lo que el auditor busca no son comprobantes archivados: es correlación entre consumo, kilometraje y misión autorizada** `[V]` ([NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)). Un sistema que solo archiva facturas no responde a lo que se le va a preguntar. Esta historia existe para producir esa correlación.

La detección en ambas direcciones es lo que distingue este control de uno ingenuo:

| Dirección | Qué significa |
|---|---|
| **Rendimiento por debajo del esperado** | Más galones de los que el recorrido justifica: posible consumo no imputable a la misión |
| **Rendimiento por encima del esperado** | Menos galones de los que el recorrido exige. **Casi siempre significa un despacho que nadie anotó** — el vehículo cargó combustible que no pasó por ningún folio |

Un umbral único simétrico es un error de diseño: un exceso de consumo del 20 % y un ahorro del 20 % no significan lo mismo.

**La escala tiene tres bandas en cada dirección**, y las seis son parámetros con vigencia por fecha (`RN-39`), nunca constantes:

| Dirección | Dentro de la tolerancia | Banda de revisión | Sobre el umbral de hallazgo |
|---|---|---|---|
| **Rendimiento por debajo** del esperado | `CONFORME` | `REVISAR` | `CONSUMO_EXCEDIDO` |
| **Rendimiento por encima** del esperado | `CONFORME` | `REVISAR` | `RENDIMIENTO_ANOMALO_SUPERIOR` |

`REVISAR` **no es un hallazgo**: exige tipificar la causa antes del cierre y alimenta el indicador agregado por vehículo, pero **no impide el cierre limpio**. El hallazgo sí lo impide. Sin esa tercera banda escrita, el sistema solo podía decir *conforme* o *fraude*, y la operación real está llena de casos que no son ninguno de los dos.

## Reglas que la gobiernan

- [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) — **Regla eje.** Umbrales superior e inferior independientes; `rendimiento_esperado` por vehículo con vigencia
- [RN-83](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) — Entra al numerador todo abastecimiento, cualquiera sea su fuente
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — Los kilómetros se calculan sobre el acumulado del expediente, no sobre la lectura cruda
- [RN-72](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md) · [RN-71](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) — Con relevo o sustitución, cada vehículo se concilia por separado con sus cortes
- [RN-63](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md) — No entra al denominador el kilometraje recorrido bajo tenencia ajena
- [RN-76](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md) — La espera con motor encendido entra como variable del cálculo
- [RN-77](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md) — La desviación se evalúa contra el alcance vigente a la fecha de cada hecho

## Casos especiales que la afectan

- [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — El síntoma
- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — La causa del rendimiento imposiblemente bueno
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — Cuándo el cálculo no concluye
- [CE-08](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) — La espera prolongada declarada no produce hallazgo por sí sola
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Imputación por tramo

## Criterios de aceptación

```gherkin
# language: es
Característica: Conciliación de combustible contra kilometraje recorrido

  Antecedentes:
    Dado un vehículo "TR-0045" con rendimiento esperado registrado de "12.0" km por galón, vigente a la fecha del hecho
    Y una tolerancia de desviación inferior —rendimiento por debajo del esperado— del "20" por ciento
    Y una banda de revisión inferior que va de más del "20" hasta el "40" por ciento
    Y un umbral de hallazgo inferior por encima del "40" por ciento
    Y una tolerancia de desviación superior —rendimiento por encima del esperado— del "15" por ciento
    Y una banda de revisión superior que va de más del "15" hasta el "35" por ciento
    Y un umbral de hallazgo superior por encima del "35" por ciento

  Escenario: Consumo excedido genera hallazgo y bloquea el cierre limpio
    Dado una Orden de Misión "OM-2026-0468" en estado "RETORNADA"
    Y un recorrido de "240" km según el acumulado del expediente
    Y abastecimientos por "40.0" galones
    Cuando el Jefe de Transporte ejecuta la conciliación de la misión
    Entonces el rendimiento observado es "6.00" km por galón
    Y la desviación es del "50.0" por ciento por debajo del esperado
    Y el sistema genera un hallazgo de tipo "CONSUMO_EXCEDIDO"
    Y muestra "Rendimiento observado 6.00 km/galón contra 12.00 esperado. Desviación 50.0 % por debajo. Tipifique la causa."
    Y la misión no puede cerrarse limpia mientras el hallazgo esté abierto

  Escenario: Rendimiento imposiblemente bueno también genera hallazgo
    Dado una Orden de Misión "OM-2026-0491" en estado "RETORNADA"
    Y un recorrido de "600" km
    Y abastecimientos por "20.0" galones
    Cuando el Jefe de Transporte ejecuta la conciliación de la misión
    Entonces el rendimiento observado es "30.00" km por galón
    Y la desviación es del "150.0" por ciento por encima del esperado
    Y el sistema genera un hallazgo de tipo "RENDIMIENTO_ANOMALO_SUPERIOR"
    Y muestra "Rendimiento observado 30.00 km/galón contra 12.00 esperado. Hipótesis principal: abastecimiento no registrado. Revise fondo agotado, préstamo de otra dependencia, carga de cisterna o peculio del motorista."

  Escenario: No se corrige el estimado para que cuadre
    Cuando el Jefe de Transporte abre el hallazgo de "OM-2026-0491"
    Entonces el sistema no ofrece modificar el rendimiento esperado de la misión ya ejecutada
    Y muestra el desglose que sustenta la desviación: kilómetros por tramo, cargas con su odómetro y tiempo de espera en sitio

  Escenario: La espera prolongada declarada no produce hallazgo por sí sola
    Dado un recorrido de "180" km y abastecimientos de "22.0" galones
    Y "5" horas de espera en sitio con motor encendido, declaradas por el motorista
    Cuando el Jefe de Transporte ejecuta la conciliación
    Entonces la desviación se calcula con la espera como variable
    Y la clasificación resultante es "CONFORME"
    Y el sistema no genera hallazgo por la desviación de rendimiento
    Y muestra "Desviación amparada por espera en sitio con motor encendido de 5 h, declarada el 26/09/2026."

  Escenario: El cálculo no concluye con odómetro averiado
    Dado un odómetro declarado averiado durante la misión
    Cuando el Jefe de Transporte ejecuta la conciliación
    Entonces el resultado se marca "no concluyente"
    Y no se da por cumplida la conciliación
    Y el dato se conserva para el análisis agregado

  Escenario: Con sustitución de vehículo se concilia cada uno por separado
    Dado una misión con traspaso en ruta del vehículo "TR-0045" al "TR-0071"
    Y un acta de traspaso con odómetro de corte
    Cuando el Jefe de Transporte ejecuta la conciliación
    Entonces el sistema produce dos conciliaciones, una por vehículo, con sus propios cortes
    Y no produce un rendimiento agregado de la misión

  Escenario: No entra al denominador el kilometraje bajo tenencia ajena
    Dado "310" km recorridos bajo préstamo del vehículo a otra dependencia, con las dos lecturas del acta
    Y "432" km recorridos en misión
    Cuando el Jefe de Transporte ejecuta la conciliación de la misión
    Entonces el denominador de kilómetros es "432" km
    Y los "310" km bajo tenencia ajena quedan asentados y excluidos del cálculo

  Esquema del escenario: Clasificación de la desviación contra las seis bandas declaradas
    Dado un recorrido de "<km>" kilómetros y abastecimientos de "<galones>" galones
    Cuando se ejecuta la conciliación
    Entonces el rendimiento observado es "<observado>" km por galón
    Y la desviación es del "<desviacion>" por ciento "<direccion>" del esperado
    Y la clasificación es "<clasificacion>"

    Ejemplos:
      | km  | galones | observado | desviacion | direccion  | clasificacion                |
      | 360 | 31.0    | 11.61     | 3.2        | por debajo | CONFORME                     |
      | 360 | 36.0    | 10.00     | 16.7       | por debajo | CONFORME                     |
      | 360 | 42.0    | 8.57      | 28.6       | por debajo | REVISAR                      |
      | 240 | 40.0    | 6.00      | 50.0       | por debajo | CONSUMO_EXCEDIDO             |
      | 360 | 29.0    | 12.41     | 3.4        | por encima | CONFORME                     |
      | 360 | 24.0    | 15.00     | 25.0       | por encima | REVISAR                      |
      | 600 | 20.0    | 30.00     | 150.0      | por encima | RENDIMIENTO_ANOMALO_SUPERIOR |

  Escenario: REVISAR exige tipificar la causa pero no impide el cierre limpio
    Dado una Orden de Misión "OM-2026-0475" en estado "RETORNADA"
    Y un recorrido de "360" km y abastecimientos por "42.0" galones
    Cuando el Jefe de Transporte ejecuta la conciliación de la misión
    Entonces la clasificación es "REVISAR"
    Y muestra "Rendimiento observado 8.57 km/galón contra 12.00 esperado. Desviación 28.6 % por debajo, dentro de la banda de revisión. Tipifique la causa antes de cerrar."
    Y exige la tipificación de la causa antes de permitir el cierre
    Y no genera hallazgo
    Y la misión puede cerrarse limpia una vez tipificada la causa
    Y el caso entra al indicador agregado por vehículo

  Escenario: Las desviaciones recurrentes generan alerta agregada
    Dado 4 misiones del vehículo "TR-0045" con desviación fuera de umbral en 60 días
    Cuando el sistema evalúa el período
    Entonces genera una alerta agregada por vehículo
    Y muestra "TR-0045: 4 misiones con desviación fuera de umbral entre el 01/08/2026 y el 30/09/2026. El patrón se ve aquí, no en una misión aislada."
```

## Fuera de alcance

- La conciliación del fondo (asignado, consumido, comprobado, devuelto) — es [HU-089](HU-089-conciliar-el-fondo-y-tipificar-sobrante-y-faltante.md)
- La conciliación de peajes — es [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)
- La clasificación de cierre — es [HU-094](HU-094-cerrar-con-hallazgo-tipificado.md)
- Los viáticos: fuera de alcance, los gestiona ARGOS ([DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))

## Notas y pendientes

- `[C]` **`rendimiento_esperado` por vehículo y sus variantes por tipo de ruta.** Terreno de montaña, tráfico o aire acondicionado en operación prolongada degradan el rendimiento legítimamente; sin variantes el sistema produce hallazgos falsos y en tres meses nadie los mira — insumo **#1**, con el Jefe de Transporte
- `[C]` **Los seis umbrales de la escala** —tolerancia, banda de revisión y umbral de hallazgo, en cada dirección— insumos **#1** y **#19**. Los valores de los ejemplos son ilustrativos y no se cablean. Corregido por `HB34-07`: antes eran dos umbrales, la tercera clasificación no tenía ninguno y la tabla contradecía a los `Antecedentes`
- `[C]` **Efecto de `REVISAR` sobre el cierre.** Se adopta que **exige tipificar la causa y no impide el cierre limpio**; el hallazgo sí lo impide. Es la postura defendible —`REVISAR` significa *hay que mirarlo*, no *hay que sancionarlo*— y es **reversible**: si Auditoría Interna exige que la banda de revisión también bloquee el cierre limpio, se cambia el parámetro `efecto_banda_revision_sobre_cierre` sin tocar la clasificación — insumo **#19**
- `[C]` Límite de jornada de conducción, para la imputación por tramo — insumo **#48**
- `[V]` Que el auditor busca correlación entre consumo, kilometraje y misión autorizada, y que el patrón de hallazgo del TSC en flota es el incremento de consumo sin relación con el uso habitual — [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
