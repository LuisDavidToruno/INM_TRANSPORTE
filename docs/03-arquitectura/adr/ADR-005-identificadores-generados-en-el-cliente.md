# ADR-005 — Los identificadores se generan en el cliente

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

## Contexto

Un motorista en Gracias a Dios registra la salida, tres paradas, dos incidentes y catorce fotografías. Nada de eso tocó un servidor. Cuando vuelve a haber señal —cuatro días después— todo eso tiene que llegar a la base **una sola vez**, aunque el envío se corte tres veces en el intento.

Eso obliga a que **el registro nazca con su identificador puesto**, en el dispositivo. Un identificador asignado por el servidor al insertar no sirve: el cliente necesita referenciar la salida desde la parada antes de que exista ningún servidor de por medio.

Hay una segunda cosa que decidir junto con esta, y que la designación marca como imposible de cambiar después: **cuál es la clave agrupada de las tablas**. No es la misma pregunta, pero la respuesta a una condiciona a la otra.

Y hay una tercera, que es la que la gente olvida: **identificador interno y folio no son lo mismo**, y confundirlos rompe el sistema en campo.

## Requisitos que la condicionan

- [`RNF-03`](../../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md) — 7 días sin red, **0 registros perdidos y 0 duplicados** al sincronizar
- [`RNF-21`](../../02-requisitos/no-funcionales/RNF-21-integridad-de-folios-y-correlativos.md) — integridad de folios y correlativos: **0 colisiones de identificador interno entre dispositivos**, **0 folios repetidos entre dispositivos de la misma delegación**

## Decisión

### 1. Todo registro nace con su identificador, generado en el dispositivo

Desde la primera tabla. **No se arranca con `int IDENTITY`**: aceptar identificadores del cliente después obliga a reescribir cada clave foránea del sistema, y para entonces hay 43 entidades pobladas.

### 2. El identificador es **ULID**, no GUID aleatorio

Esta es la parte que impone el motor. En SQL Server la clave primaria es **agrupada por omisión**, y un GUID aleatorio como clave agrupada **fragmenta el índice en cada inserto** — cada fila nueva cae en una página al azar y provoca división de página. En 2014 Standard **no hay compresión de datos** para amortiguarlo, y `RNF-02` habla de un acervo que nunca se borra.

ULID resuelve las dos cosas a la vez: se genera sin coordinación —como el GUID— y es **monótono en el tiempo** —como el `IDENTITY`—, así que los insertos van al final del índice.

> Si por alguna razón se prefiriera GUID, la salida es **separar la clave primaria de la clave agrupada**: primaria no agrupada sobre el GUID, agrupada sobre un `bigint` secuencial. Es más piezas y más explicación. ULID evita la discusión.

### 3. La clave de idempotencia de la ingesta va en el modelo, no en el sincronizador

Toda escritura que nace en campo lleva:

| Campo | Qué es |
|---|---|
| `id_dispositivo` | Identificador estable del dispositivo, no del usuario |
| `secuencia_dispositivo` | Secuencia **monótona** local, que nunca retrocede |

El par `(id_dispositivo, secuencia_dispositivo)` es **único**, y es lo que hace que reenviar el mismo lote tres veces produzca el mismo resultado que enviarlo una. Está en el modelo de datos porque es una **propiedad del registro**, no un detalle del transporte: si vive en el sincronizador, el día que se escriba un segundo camino de ingesta —una importación, una corrección administrativa— la idempotencia no lo cubre.

### 4. Identificador interno y folio son cosas distintas, y el cliente no asigna folio

| | Identificador interno | Folio |
|---|---|---|
| Qué es | Técnico y opaco | El número impreso que la institución cita en su descargo |
| Quién lo genera | El dispositivo | El servidor, contra el rango de la delegación |
| Cuándo | Al crear el registro | Al sincronizar |
| Tiene que ser | Único globalmente | **Secuencial y explicable** dentro de la delegación |

**El cliente no puede asignar folio definitivo en campo** (`RNF-21`). Tres dispositivos de la misma delegación, los tres desconectados, emitiendo el mismo tipo de documento, asignarían el mismo folio. En campo se imprime con **folio provisional marcado como tal**; el definitivo se fija al sincronizar.

Esto es también la razón por la que **no se empieza por el sincronizador**: los rangos de folio se diseñan antes.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **`int IDENTITY` del servidor** | Claves chicas, índice denso, legible en depuración | El cliente no puede crear nada sin servidor; imposible referenciar entre registros creados offline | Incompatible con `RNF-03`. Y migrarlo después reescribe todas las claves foráneas |
| **GUID aleatorio (`NEWID()`)** | Sin coordinación, universalmente entendido | Fragmenta la clave agrupada en cada inserto; sin compresión en 2014 Standard el costo es íntegro | Ver el punto 2. ULID da lo mismo sin el costo |
| **`NEWSEQUENTIALID()`** | Secuencial y no fragmenta | **Se genera en el servidor.** No sirve para el caso que motiva este ADR | No resuelve el problema |
| **Identificador compuesto `(delegacion, secuencia)` como clave** | Legible, ordenado por origen | Convierte cada clave foránea en compuesta; y una reorganización de delegaciones tocaría claves primarias | Complica todo el modelo para ganar legibilidad |

## Consecuencias

**Positivas**

- El cliente de campo crea expedientes completos y coherentes sin servidor, que es literalmente `RNF-03`
- Reenviar un lote es seguro por construcción: la idempotencia es del dato
- ULID ordena por tiempo de creación, lo que hace que los índices y los recorridos por fecha se comporten bien
- La separación identificador/folio deja el número impreso bajo control de la institución, que es lo que un auditor pide

**Negativas**

- **16 bytes por clave contra 4 de un `int`**, multiplicado por cada clave foránea e índice. Con ≈8 GB/año y sin compresión, se nota
- Un ULID no se dicta por teléfono. Las pantallas y los formatos impresos tienen que mostrar **folio**, nunca identificador interno — y eso hay que sostenerlo en el diseño
- **El reloj del dispositivo influye en el orden del ULID.** Un teléfono con la hora mal puesta genera identificadores fuera de secuencia. No rompe la unicidad, pero sí la propiedad de monotonía que motiva la elección. Ver [`ADR-007`](ADR-007-marcas-de-tiempo-en-utc.md)
- El folio provisional en campo **es visible para el usuario**, y hay que explicarle por qué el número cambia al sincronizar

**Deuda aceptada**

- **La secuencia monótona del dispositivo depende de que el dispositivo la conserve.** Un borrado de datos de la aplicación, una restauración de fábrica o una reinstalación pueden reiniciarla. La mitigación es que `id_dispositivo` cambie con la reinstalación, de modo que el par siga siendo único. **Esa regla hay que implementarla explícitamente**, y mientras no exista, la idempotencia tiene un agujero
- El folio provisional impreso circula en papel. Si alguien archiva ese impreso como definitivo, hay dos números para el mismo hecho. Se mitiga marcándolo visiblemente, no se elimina

## Revisión

- **La medición real de fragmentación o de tamaño de índice** contradice lo previsto
- **Aparece un caso de negocio que exige folio definitivo en campo** — habría que resolverlo con rangos por dispositivo, no reabriendo la generación de identificadores
- **La reinstalación de la aplicación resulta más frecuente de lo previsto** y la regla de `id_dispositivo` no alcanza
