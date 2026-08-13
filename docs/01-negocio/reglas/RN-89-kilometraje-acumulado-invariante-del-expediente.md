# RN-89 — El kilometraje acumulado es atributo derivado del expediente del vehículo, independiente de la lectura de cualquier instrumento

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-08, M-11, M-09 |
| **Origen** | Casos especiales [CE-22](../../02-requisitos/casos-especiales/CE-22-odometro-inconsistente.md), [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md), [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) · Normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) y [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[P]` la exigencia de registros confiables y conciliables — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` el acumulado como invariante del expediente: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro + derivación |
| **Configurable** | No |

## Enunciado

El **kilometraje acumulado** del vehículo es **atributo derivado y propio de su expediente**, independiente de la lectura de cualquier instrumento. **No decrece nunca.**

Toda lectura pertenece a una **serie de instrumento** con **vigencia y unidad declarada** —kilómetros o millas—. El **reemplazo del instrumento cierra una serie y abre otra** ([`RN-90`](RN-90-intervencion-del-instrumento-de-medicion.md)); la vuelta de contador es un **reemplazo lógico** con motivo tipificado propio.

Toda lectura se almacena **normalizada, conservando la unidad original**.

La **continuidad se evalúa sobre la serie ordenada por fecha del hecho**: insertar un registro anterior **reabre la validación de todos los posteriores**.

El **plan de mantenimiento preventivo se calcula sobre el acumulado, jamás sobre la lectura**.

## Justificación

[`RN-31`](RN-31-odometro-de-retorno.md) enuncia el acumulado como comportamiento y caso límite de una regla de **validación de captura**. Pero la existencia del acumulado **no es una validación: es un invariante del expediente del vehículo**, y ninguna regla de M-03 lo obliga. Sin regla propia, `RN-31` se implementa entera **guardando solo lecturas**, y entonces cambiar un tablero borra la historia del vehículo.

Las consecuencias son concretas y caras:

- **Mantenimiento.** Si el plan se calcula sobre la lectura, cambiar un tablero **pospone indefinidamente un servicio**, y la falla que venga después es responsabilidad de quien autorizó el vehículo.
- **Unidad.** Asumir kilómetros sobre un tablero en millas produce un error del 60 % que nadie detecta hasta que la conciliación es absurda.
- **Digitación diferida.** Si la continuidad se evalúa por orden de captura, una bitácora digitada tarde parece un retroceso de odómetro y produce un conflicto falso ([`RN-79`](RN-79-el-retorno-constatado-libera-al-vehiculo.md), [`RN-80`](RN-80-hoja-de-bitacora-impresa-con-folio.md)).

## Condiciones de aplicación

Aplica a todo vehículo de la flota, en todo régimen de tenencia y de uso.

Aplica a la **unidad sustituta** entregada por un arrendador, que se da de alta como vehículo nuevo con **serie de odómetro propia** ([`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)).

## Comportamiento esperado

1. El expediente del vehículo expone el acumulado y, por separado, la lectura corriente del instrumento vigente con su unidad.
2. Cada lectura se registra con: valor, unidad, fecha del hecho, fuente —bitácora, acta, orden de trabajo, constatación— y serie a la que pertenece.
3. El acumulado se recalcula sobre la serie ordenada **por fecha del hecho**. Un registro insertado con fecha anterior dispara la revalidación de los posteriores y los conflictos van a **cola de resolución humana** ([`RN-45`](RN-45-cero-sobrescritura-silenciosa.md)).
4. Cuando el vehículo **no retorna**, el kilometraje se cierra en la **última lectura verificable, con su fuente**, y el tramo posterior queda **no determinado**. **Nunca se completa con distancia teórica** y la conciliación se declara **truncada** ([`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)).
5. El acumulado alimenta el mantenimiento preventivo, el costo por kilómetro y la conciliación galonaje–kilometraje ([`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md)).
6. Ningún proceso puede escribir el acumulado directamente: solo se deriva de lecturas y de eventos de intervención del instrumento.

## Casos límite

- **Contador que da la vuelta.** Reemplazo lógico con motivo propio: la serie se cierra en su valor máximo y se abre otra desde cero. El acumulado sigue creciendo.
- **Odómetro en millas.** La unidad se declara en la ficha del vehículo y toda lectura se conserva en su unidad original, además de normalizada. Cambiar el tablero por uno en kilómetros es un cambio de serie **y** de unidad.
- **Lectura inferior a la anterior dentro de la misma serie.** Es el caso de [`RN-31`](RN-31-odometro-de-retorno.md): exige justificación con respaldo, y si la causa es el instrumento, se resuelve por [`RN-90`](RN-90-intervencion-del-instrumento-de-medicion.md) — no por un motivo tipificado dentro de la bitácora.
- **Salto imposible** — 4.000 km entre dos misiones consecutivas de la misma semana. Es un dato que el sistema no puede aceptar en silencio: alerta con el detalle y va a resolución humana. Puede ser un dígito de más al digitar, y puede no serlo.
- **Migración inicial** de una flota sin historial. El acumulado se abre con la lectura de carga, declarada como **acumulado inicial estimado**, con su fecha. Declarado, no disfrazado de dato histórico.
- **Dos lecturas contradictorias para la misma fecha del hecho** —una de la bitácora, otra de una orden de trabajo. Ambas se conservan y el conflicto lo resuelve una persona.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · [NRM-09](../normativa/NRM-09-realidad-operativa.md) `[V]`
- Reglas relacionadas: [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md), [RN-72](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md), [RN-75](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md), [RN-79](RN-79-el-retorno-constatado-libera-al-vehiculo.md), [RN-90](RN-90-intervencion-del-instrumento-de-medicion.md)
- Casos especiales: [CE-22](../../02-requisitos/casos-especiales/CE-22-odometro-inconsistente.md) `RN-C22a` · [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) `RN-c:continuidad-de-odometro-por-fecha-del-hecho` · [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) `RN-c:ultima-lectura-verificable-de-odometro`
