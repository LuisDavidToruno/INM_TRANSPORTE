# ADR-007 — Marcas de tiempo en UTC con el desfase del dispositivo

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

## Contexto

Honduras está en **UTC−6** y no aplica horario de verano. Eso hace que la decisión parezca trivial —una sola zona, sin saltos— y es justamente por eso que se toma mal.

El problema es concreto: **a partir de las 18:00 hora local, `UtcNow.Date` y la fecha local son días distintos.** Un motorista que registra el retorno a las 19:30 del martes produce un registro que, en UTC, es del miércoles.

En SICOV_CORE8 esto rompió una validación en producción: el formulario proponía una fecha y la validación exigía otra, **sin forma de deducir qué faltaba**. Hoy hay una regla entera y una guarda dedicada (`UnSoloHoyTests`) para que no vuelva a pasar.

Acá el riesgo es mayor por tres cosas que SICOV no tiene: **las marcas se generan en dispositivos** cuyo reloj no controlamos, **la captura offline dura hasta 7 días** antes de llegar al servidor, y **hay una cadena de hash** cuyo orden depende del tiempo. Se decide antes de la primera fila.

## Requisitos que la condicionan

- [`RNF-03`](../../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md) — hasta 7 días entre la captura y la llegada al servidor
- [`RNF-04`](../../02-requisitos/no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) — bitácora append-only con hash encadenado
- [`RNF-08`](../../02-requisitos/no-funcionales/RNF-08-seguimiento-en-ruta.md) — ubicación y estado con marca temporal confiable

## Decisión

### 1. Toda marca de tiempo se **almacena en UTC**

Sin excepción, en la base y en el almacén local del dispositivo.

### 2. Toda marca generada en un dispositivo guarda **tres cosas, no una**

| Campo | Qué es |
|---|---|
| `momento_utc` | El instante, en UTC |
| `desfase_local` | El desfase del dispositivo al momento de capturar |
| `momento_recibido_utc` | Cuándo llegó al servidor |

El desfase se guarda **porque el hecho ocurrió en una hora local concreta** y hay reglas que dependen de eso: día y hora inhábil exige permiso firmado por la máxima autoridad, y esa regla se evalúa contra la hora que el motorista vivió, no contra UTC.

Guardar `momento_recibido_utc` separado es lo que permite distinguir *cuándo pasó* de *cuándo lo supimos*. Es el mismo par de preguntas de [`ADR-006`](ADR-006-temporalidad-bitemporal.md), aplicado a los hechos operativos.

### 3. **«Hoy» se define en un solo lugar**

Hay una única función que responde qué día es, y recibe la zona explícitamente. **Ninguna regla de dominio llama a `DateTime.Now`, `DateTime.UtcNow` ni `Date.now()` por dentro** — la fecha entra como parámetro, igual que en `ADR-006`.

Esto se hace cumplir con una guarda de arquitectura: `NingunaReglaLeeElReloj`. Un `DateTime.Now` dentro de `Reglas/` **falla la compilación de la suite**, no se detecta en revisión.

### 4. El reloj del dispositivo no es confiable, y el sistema lo asume

Un teléfono con la hora mal puesta produce marcas incorrectas y no hay forma de impedirlo en campo. Las tres defensas:

- **La secuencia monótona del dispositivo** (`ADR-005`) da el orden real de captura, independiente del reloj. **El orden de la cadena de hash lo fija la secuencia, no la marca de tiempo.**
- Al sincronizar, un desfase mayor a un umbral configurable entre `momento_utc` y `momento_recibido_utc` **se marca como anomalía** para revisión — no se rechaza el registro, porque rechazar datos de campo es perder datos de campo
- La aplicación sincroniza su hora con el servidor cuando hay señal, y registra cuándo lo hizo por última vez

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **Hora local en todo** (`datetime2` sin zona) | Lo que el usuario ve es lo que está guardado; consultas por día directas | Ambiguo por definición; si alguna vez hay una segunda institución en otra zona (`RNF-19`) el acervo queda inservible; comparar marcas de dos dispositivos deja de ser confiable | Ambigüedad estructural que no se puede reparar después |
| **Solo UTC, sin guardar el desfase** | Un campo, sin duplicación | Se pierde la hora local en la que ocurrió el hecho, y hay reglas normativas que dependen de ella. Reconstruirla asumiendo UTC−6 funciona **hasta** el primer dispositivo mal configurado o la primera institución fuera de zona | Pierde información que no se puede recuperar |
| **`DateTimeOffset` en todas las columnas** | Guarda instante y desfase en un solo tipo, nativo | Más ancho; y no resuelve `momento_recibido_utc`, que es una tercera cosa | Se adopta el tipo donde convenga, pero la decisión es qué se guarda, no con qué tipo |
| **Rechazar registros con reloj desviado** | Datos limpios | En campo, rechazar es perder. `RNF-03` dice 0 registros perdidos | Contradice el requisito central del cliente de campo |

## Consecuencias

**Positivas**

- Las marcas de dos dispositivos son comparables sin suposiciones
- Las reglas de hora inhábil se evalúan contra la hora que el motorista realmente vivió
- La cadena de hash queda ordenada por secuencia, no por un reloj que no controlamos — que es lo que la hace resistente
- `RNF-19` no se compromete: una institución en otra zona no rompe nada

**Negativas**

- **Tres campos donde ingenuamente iría uno**, en muchas tablas
- Toda consulta *«lo de hoy»* tiene que convertir, y es fácil escribir la versión que funciona en la oficina y falla a las 18:00
- La interfaz siempre muestra hora local y siempre guarda UTC: la conversión es una frontera que hay que sostener en dos clientes

**Deuda aceptada**

- **Las marcas capturadas con el reloj del dispositivo desviado quedan mal, y quedan.** La anomalía se detecta y se reporta, pero el dato no se corrige automáticamente: corregirlo sería inventar. La secuencia monótona preserva el **orden**, no el **instante**
- El umbral de desfase que dispara la anomalía es un parámetro más que alguien tiene que fijar y revisar `[C]`

## Revisión

- **Honduras adopta horario de verano** o cambia su desfase
- **Entra una institución en otra zona horaria** (`RNF-19`)
- **La anomalía de desfase se dispara con frecuencia alta** — señal de que la sincronización de hora del cliente no está funcionando y hace falta otro mecanismo
