# ADR-006 — Temporalidad bitemporal desde el modelo inicial

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

## Contexto

El Tribunal Superior de Cuentas pregunta por hechos viejos. La pregunta típica no es *«¿cuánto cuesta el peaje de Villanueva?»* sino **«¿por qué esta liquidación de marzo pagó L. 22 de peaje?»**. Y esas dos preguntas tienen respuestas distintas si la tarifa cambió en abril.

Hay un segundo giro, y es el que la gente no ve venir: a veces el sistema **se equivocó** sobre lo que regía. Alguien cargó la tarifa nueva con la fecha de vigencia mal puesta, se liquidaron cuarenta misiones con ella, y tres semanas después se corrigió. Entonces hay **dos preguntas diferentes** sobre el mismo día de marzo:

1. *¿Qué decía el reglamento el día del viaje?* — la verdad normativa
2. *¿Qué creía el sistema el día que se liquidó?* — lo que explica el monto que efectivamente se pagó

Un solo eje de tiempo responde una y **falsifica la otra**. Si solo se guarda la vigencia normativa corregida, la liquidación de marzo queda inexplicable: el monto pagado no se deriva de ningún dato que el sistema conserve. Si solo se guarda lo que el sistema sabía, se pierde cuál era la norma real.

Este fue el hallazgo **`HB34-50`** de la revisión de arquitectura del Bloque 4: el modelo era unitemporal donde `RNF-05` pide dos ejes. Ya está corregido en el modelo de datos (decisión `D-13`); este ADR fija la decisión de arquitectura que lo sostiene.

## Requisitos que la condicionan

- [`RNF-05`](../../02-requisitos/no-funcionales/RNF-05-temporalidad-normativa.md) — todo cálculo usa la tabla vigente **a la fecha del hecho**, no a la de captura
- [`RNF-06`](../../02-requisitos/no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md) — reproducibilidad histórica: un reporte de marzo reemitido hoy da lo mismo que dio en marzo
- [`RNF-04`](../../02-requisitos/no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) — bitácora append-only con hash encadenado

## Decisión

**Los parámetros normativos son bitemporales, con los dos ejes modelados a mano.**

| Eje | Columnas | Qué responde |
|---|---|---|
| **Vigencia normativa** | `VigenteDesde` / `VigenteHasta` | Qué decía el reglamento el día del viaje |
| **Tiempo de transacción** | `RegistradoDesde` / `RegistradoHasta` | Qué sabía el sistema el día que se liquidó |

### Ninguno de los dos es nativo, y hay que decirlo

SQL Server 2014 **no tiene temporal tables** — llegaron en 2016. Se dice aquí explícitamente porque el error probable es que alguien implemente un eje a mano y **suponga que el otro viene puesto por el motor**. No viene ninguno. Los cuatro campos son propios, y las reglas que los mantienen también.

### Una corrección retroactiva no sobrescribe: abre un asiento de diferencia

Cuando se descubre que un parámetro estaba mal cargado, la fila anterior **no se actualiza**. Se cierra su `RegistradoHasta` y se inserta la versión corregida. Las liquidaciones ya emitidas siguen siendo explicables por la fila que regía cuando se emitieron, y la diferencia —si la hay— se registra como **asiento de diferencia**, no como una modificación silenciosa del pasado. Es `RN-04` aplicado a los parámetros: toda anulación es asiento reverso con motivo y autor.

### La firma de las reglas puras es la que la bitemporalidad necesita

[`ADR-009`](ADR-009-modulos-verticales.md) exige que las reglas de dominio sean puras y **reciban la fecha como parámetro**:

```
Reglas.CalcularX(datos, vigenteAl)
```

Esa es exactamente la firma que la resolución temporal necesita. No es coincidencia y conviene aprovecharla: **si una sola regla lee `DateTime.Now` por dentro, se pierden las dos cosas a la vez** — la pureza que hace la regla probable y la temporalidad que la hace correcta.

### Por qué no es diferible

Poner *«qué valor regía ese día»* encima de tablas que solo guardan el valor actual obliga a **inventar una historia que no se tiene**. No hay de dónde sacarla: el dato viejo se sobrescribió.

En SICOV_CORE8 se pagó tres veces. En la tercera, el sembrador de datos **se negó a sembrar valores** porque habría creado una vigencia falsa firmada por el sistema — justo en la tabla que existe para saber qué rigió y cuándo. Ese es el final del camino: una tabla de historia con historia inventada es peor que no tener la tabla, porque miente con autoridad.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **Un solo eje (vigencia normativa)** | La mitad de columnas, consultas más simples | No explica los montos ya emitidos cuando el parámetro se corrige; `RNF-06` deja de cumplirse tras la primera corrección retroactiva | Es exactamente el hallazgo `HB34-50` |
| **Temporal tables del motor** | El motor mantiene un eje solo; sintaxis limpia | **No existen en 2014.** Y aun existiendo, dan el eje de transacción, no el de vigencia normativa — el que más importa aquí | Descartada por el motor, y no habría resuelto lo principal |
| **Copiar el valor del parámetro dentro de cada transacción** (*snapshot* en la fila) | Reproducibilidad trivial: el monto trae su tarifa pegada | Duplica el dato en cada fila; una corrección de parámetro no puede propagarse ni auditarse; imposible responder *«qué regía»* sin recorrer transacciones | Resuelve `RNF-06` y rompe `RNF-05` |
| **Bitemporalidad solo en las tablas de parámetros, unitemporal en el resto** | Menos superficie, foco en donde duele | Es **lo que se decide**, no una alternativa: los agregados operativos no necesitan dos ejes | Se adopta como alcance, ver abajo |

## Alcance — dónde aplica y dónde no

**Bitemporal:** tarifas de peaje, categorías por número de ejes, matriz licencia↔vehículo, feriados, horario hábil, plazos, y todo parámetro con vigencia. Es el `M-02` que sí tiene invariantes — ver [`ADR-009`](ADR-009-modulos-verticales.md).

**No bitemporal:** los agregados operativos —orden de misión, bitácora, vale de combustible—. Su historia la lleva la bitácora append-only de `RNF-04`, que es otro mecanismo y responde otra pregunta.

Poner cuatro columnas de tiempo en todas las tablas sería el error simétrico al de `HB34-50`.

## Consecuencias

**Positivas**

- `RNF-05` y `RNF-06` se cumplen por construcción, no por disciplina
- Una liquidación vieja siempre es explicable, incluso después de corregir el parámetro que la produjo
- La corrección retroactiva deja de ser un procedimiento delicado y se vuelve una operación normal con su asiento

**Negativas**

- **Toda consulta de parámetro lleva dos condiciones de tiempo**, no una. Es fácil escribir la mitad y que funcione hasta la primera corrección
- Cuatro columnas más por tabla de parámetros, y sin compresión de datos en 2014 Standard
- La interfaz tiene que mostrar *dos* nociones de tiempo sin confundir al usuario. Es un problema de diseño real, no menor
- **Nada del motor ayuda.** Si la regla de mantenimiento tiene un defecto, el defecto se materializa en datos y no hay validación que lo ataje

**Deuda aceptada**

- **La corrección de los dos ejes depende enteramente de código propio.** Mientras no exista una guarda que verifique que ninguna consulta de parámetro resuelve con un solo eje, la disciplina es humana
- El inventario de parámetros y su antigüedad (`HU-149`) es lo que evita el otro fallo silencioso: un parámetro que nadie actualiza desde hace tres años y que el sistema aplica con confianza total

## Revisión

- **La institución migra a una versión con temporal tables.** Aun así, solo cubriría el eje de transacción — la reconsideración sería parcial
- **Aparece un agregado operativo que sí necesita dos ejes**, y el alcance de arriba deja de ser suficiente
- **Una auditoría del TSC pide una reconstrucción que este modelo no puede producir**
