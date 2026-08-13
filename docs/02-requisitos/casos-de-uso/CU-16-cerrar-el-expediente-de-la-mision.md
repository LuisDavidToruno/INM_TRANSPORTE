# CU-16 — Cerrar el expediente de la misión

| Campo | Valor |
|---|---|
| **Módulos** | M-13 Liquidación y Cierre · M-14 Reportes, Indicadores y Auditoría |
| **Actor principal** | `ACT-08` Gerencia Administrativa |
| **Actores secundarios** | `ACT-12` Auditor Interno — puede **requerir** el cierre con hallazgo, y su requerimiento obliga · `ACT-04` Jefe de Transporte — propone, no cierra · `ACT-11` Encargado de Mantenimiento y `ACT-14` Encargado de Bienes — expedientes vinculados |
| **Precondiciones** | La misión está en **`LIQUIDADA`**. **Todas las asignaciones de fondo vinculadas están conciliadas**, en cualquiera de sus dos formas — `CONCILIADA` o `CONCILIADA_CON_DESVIACION` (§10.1). El resultado económico está congelado (`INV-38`). Quien va a cerrar **no es quien liquidó** (`BD-06`) |
| **Postcondiciones** | El expediente queda en un **estado terminal e inmutable**: `CERRADA` o `CERRADA_CON_HALLAZGO`. La cadena de auditoría de la misión queda **sellada**. Los indicadores se consolidan en los acumulados del vehículo, del motorista y de la dependencia. El expediente queda disponible para exportación como **paquete de evidencia**. Si hubo hallazgo, existe además un **expediente de hallazgo** abierto con responsable de seguimiento y plazo |
| **Disparador** | `T-19` deja la misión en `LIQUIDADA` con una **propuesta de clasificación de cierre** en la bandeja de `ACT-08` |

> **La propuesta no cierra. Cerrar es un acto de `ACT-08`.** El sistema evalúa `H-01` a `H-08` y propone; la persona confirma con su justificación. Y a la inversa: **si algún criterio se cumple, `T-21` no está disponible** — quien cierra no elige entre cerrar limpio o con hallazgo; el criterio decide y él lo confirma.

> **`CERRADA_CON_HALLAZGO` no imputa responsabilidad a nadie, no sanciona y no debe presentarse como falta en ningún reporte.** Un vehículo robado en ruta produce hallazgo y nadie es culpable. Es una **marca de seguimiento**, y existe por una razón operativa dura: *un expediente que no puede cerrarse se abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver* ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)).

## Flujo principal

1. `ACT-08` abre el expediente en su bandeja de cierre. `T-21` / `T-22` — [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) E13.
2. El sistema verifica **`BD-06`: quien cierra ≠ quien liquidó.** `PC-13`. Bloqueo duro.
3. El sistema presenta, en una sola vista:
   - la **cadena de trazabilidad completa**, eslabón por eslabón, con *presente*, *ausente* o *no aplicable con fundamento* ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md));
   - el **resultado económico congelado** con los identificadores de las tablas paramétricas usadas;
   - las **tres conciliaciones** de `EF-05` —combustible, peajes, kilometraje y tiempos— con sus desviaciones y sus causas tipificadas;
   - el **grado de cumplimiento del objeto**, por destino y consolidado ([`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md));
   - la **propuesta de clasificación** con los criterios `H-01` a `H-08` evaluados y **los datos concretos que los dispararon o no**.
4. El sistema verifica que **todas las asignaciones de fondo estén conciliadas** (§10.1). Esta condición aplica **tanto a `T-21` como a `T-22`**: ni siquiera el cierre con hallazgo procede sobre una asignación sin conciliar.
5. El sistema verifica que **no haya expedientes vinculados abiertos que condicionen el resultado**: incidente en investigación (M-12), **reclamo de peaje ante la SAPP sin resolver** ([`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md)), orden de trabajo derivada de una novedad no atendida (M-11). `[C]` **cuáles condicionan el cierre y cuáles no** — insumo #1.
6. **No se cumple ningún criterio `H-nn`** → `ACT-08` ejecuta **`T-21` cerrar**. La misión pasa a `CERRADA`.
7. Efectos del cierre:
   - el expediente pasa a **inmutable**: no se modifica ningún dato, ni un odómetro, ni un monto, ni una fecha, ni un motivo, ni un adjunto, **ni siquiera una errata en un campo de texto** ([orden-de-mision.md §8.2](../../03-arquitectura/estados/orden-de-mision.md), [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md));
   - **se sella la cadena de auditoría**: el hash de la última transición cierra la cadena ([RNF-04](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md));
   - se **consolidan los indicadores** en los acumulados del vehículo, del motorista y de la dependencia, incluidos los de calidad de la programación por causa tipificada ([`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md));
   - el expediente queda **exportable como paquete de evidencia**: índice, documentos, adjuntos, hoja de cálculo y la **cadena representada explícitamente** ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), [RNF-18](../no-funcionales/RNF-18-paquetes-de-evidencia-para-auditoria.md));
   - se exponen los hechos a ARGOS por la **clave de vinculación** de la orden, sin escribir en el sistema origen ([`RN-81`](../../01-negocio/reglas/RN-81-sigti-expone-hechos-a-argos.md)). Si existe viático asociado, se muestra su estado por esa clave; SIGTI no lo calcula ni lo liquida ([DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).
8. Todo reporte que en adelante incluya esta misión declara su **fecha de corte de conocimiento** y es reproducible a esa fecha ([`RN-94`](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)).

## Flujos alternos

**A1 — Cierre con hallazgo (`T-22`)** (desde el paso 6)

1. Se cumple **al menos uno** de los criterios de la lista cerrada de §7.2. **`T-21` no está disponible**: el sistema no ofrece la opción de cerrar limpio.

   | ID | Criterio | Umbral |
   |---|---|---|
   | `H-01` | Desviación de consumo contra rendimiento esperado fuera de umbral, **en cualquier dirección**, sin justificación aceptada | Configurable por tipo de vehículo |
   | `H-02` | Kilometraje fuera de umbral respecto a la ruta autorizada, sin justificación aceptada | Configurable |
   | `H-03` | Paso por peaje incompatible con la ruta autorizada, o secuencia de casetas geográfica o temporalmente imposible | Cualquier caso |
   | `H-04` | Fondo entregado no devuelto ni comprobado al vencer el plazo de liquidación | Sin umbral |
   | `H-05` | Circulación en día u hora inhábil sin permiso vigente, detectada al conciliar | Sin umbral |
   | `H-06` | Incidente, siniestro, multa o pérdida del bien durante la misión, aún sin resolución en M-12 | Sin umbral |
   | `H-07` | Bloqueo duro que falló al revalidarse tras sincronizar una operación desconectada | Sin umbral |
   | `H-08` | Ausencia de comprobante obligatorio, o divergencia de sincronización resuelta descartando datos capturados en campo | Configurable |

2. `ACT-08` ejecuta `T-22` con **motivo obligatorio**, tipificando el hallazgo de un catálogo configurable y **consignando responsable** ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), comportamiento 2).
3. El sistema crea automáticamente el **expediente de hallazgo** con: criterio que lo originó, **datos concretos que lo dispararon**, responsable de seguimiento asignado y plazo. Notifica a `ACT-12` y a `ACT-08`.
4. **La misión está cerrada.** Es terminal e inmutable igual que `CERRADA`. Lo que queda abierto es el expediente de hallazgo, que es **otra entidad con su propio ciclo** (M-12, M-14).
5. **Resolver el hallazgo no cambia el estado de la misión.** El expediente de hallazgo se cierra; la misión sigue siendo `CERRADA_CON_HALLAZGO` para siempre. Que el hallazgo se haya resuelto se lee en el expediente de hallazgo, **no reescribiendo la historia de la misión**.
6. La misión entra en los reportes de control interno y en los paquetes de evidencia por período, vehículo, motorista y dependencia.
7. Hallazgos reiterados de un mismo motorista o vehículo dentro de un período configurable **marcan para revisión antes de nuevas asignaciones**. La recomendación de arquitectura es que **advierta, no que bloquee**: bloquear al motorista por un hallazgo aún no resuelto es sancionar antes de investigar. `[C]` insumo #1.

**A2 — `ACT-12` requiere el cierre con hallazgo** (desde el paso 3)

1. El requerimiento del Auditor Interno **obliga**, queda registrado con su fundamento, y **`ACT-08` no puede cerrar limpio contra él**.
2. `ACT-12` **no produce ningún acto de negocio**: requiere y verifica. Su límite es absoluto —solo lectura y exportación— y **sus propias consultas quedan registradas** ([`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), [actores-y-roles `ACT-12`](../../01-negocio/actores-y-roles.md)).

**A3 — Devolver la liquidación en lugar de cerrar (`T-20`)** (desde el paso 3)

1. `ACT-08` devuelve con **motivo obligatorio** y observaciones. La misión vuelve a `RETORNADA` y a la bandeja del liquidador ([CU-15](CU-15-liquidar-la-mision-y-conciliar.md) E7).
2. La liquidación anterior **se conserva íntegra como versión**. Existe porque la alternativa —cerrar mal y corregir por asiento reverso— es más costosa y más confusa.

**A4 — Hay una obligación de reintegro abierta** (en el paso 5)

1. **No impide cerrar.** La obligación de reintegro tiene **ciclo propio que sobrevive al cierre de la misión** y se salda con **asiento reverso sobre el expediente cerrado** ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), [`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)).
2. El cierre correspondiente es **`CERRADA_CON_HALLAZGO`** por `H-04` si el fondo no se devolvió ni comprobó al vencer el plazo. **Dejar el expediente abierto indefinidamente no es una alternativa**: es exactamente lo que produce el abandono.
3. Mientras la obligación esté abierta, la persona sigue bloqueada para recibir nueva asignación de fondo, y el monto figura en el **arqueo por persona** — el cierre de la misión no lo borra.

**A5 — Cierre de ejercicio fiscal con expedientes no terminales** (en cualquier paso) · [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md)

1. El **cierre de ejercicio es corte de imputación y de reporte; ningún expediente cambia de estado por una fecha** ([`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)). No hay cierre masivo automático el 31 de diciembre.
2. Lo no terminal al corte constituye el **saldo de apertura de control interno del ejercicio siguiente**, con antigüedad contada **desde el hecho original**, no desde el corte ([`RN-97`](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md)).
3. Los efectos económicos de un asiento reverso afectan **los acumulados del período en que se registra, no los del período original**: los históricos ya publicados siguen siendo reproducibles ([orden-de-mision.md §8.3](../../03-arquitectura/estados/orden-de-mision.md)).

**A6 — Carga inicial de expedientes históricos** (desde el paso 1)

1. La migración histórica **no se cierra por esta regla**: se marca como **expediente migrado** con el alcance de datos disponible y **se excluye de los indicadores de hallazgo** ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), casos límite).
2. `[C]` Si habrá migración histórica y con qué alcance — insumo #1.

## Flujos de excepción

**E1 — Quien intenta cerrar es quien liquidó** (en el paso 2)

1. **Bloqueo duro.** No se guarda nada. `PC-13`, `BD-06`, [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md).
2. El mensaje nombra el conflicto con precisión; el intento queda en la pista de auditoría con el par detectado y genera **tarea de resolución en el puesto competente**.
3. En delegación, el cierre **es de `ACT-08` sin excepción posible** — es una de las tres funciones que se ejercen desde la sede porque no exigen presencia física ([actores-y-roles §5.4 Nivel 1](../../01-negocio/actores-y-roles.md), [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)).

**E2 — Se cumple un criterio `H-nn` y se quiere cerrar limpio de todos modos** (en el paso 6)

1. **`T-21` no está disponible.** No hay "continuar de todos modos" y no existe la opción de desactivar un criterio **para una misión concreta**: desactivar por caso es exactamente lo que el control interno prohíbe ([orden-de-mision.md §7.2](../../03-arquitectura/estados/orden-de-mision.md)).
2. Los umbrales y el catálogo de criterios **sí** son configurables, pero como parámetro con vigencia sujeto a doble control: lo carga `ACT-01` y lo pone en vigencia `ACT-08` ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [actores-y-roles §4.3](../../01-negocio/actores-y-roles.md)).
3. **`CERRADA_CON_HALLAZGO` no es un cajón de sastre.** Si el criterio no está en la lista de §7.2, **no se cierra con hallazgo**. Un estado que absorbe todo lo que incomoda deja de significar algo en seis meses, y entonces el auditor deja de mirarlo.

**E3 — El hallazgo aparece meses después de cerrar** (posterior al paso 7) · [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md)

1. **La misión no se reabre y no cambia de estado.** Desde un terminal no sale ninguna transición. Nunca ([orden-de-mision.md §8.1 y §7.5](../../03-arquitectura/estados/orden-de-mision.md)).
2. Se abre un **expediente de hallazgo posterior** vinculado a la misión, con ciclo propio, que **no altera el estado ni los datos del objeto vinculado** ([`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)).
3. Si tiene efecto económico, se registran los **asientos reversos** que correspondan ([`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), §8.3): referencia explícita al asiento revertido —**no existe el reverso genérico "de la misión"**—, valor anterior y valor nuevo, autor, autorizador, motivo tipificado con fundamento adjunto, y `BD-06`: **quien autoriza el reverso no puede ser quien produjo el asiento revertido**.
4. **El expediente cerrado muestra el reverso, no lo esconde.** Todo reporte sobre esa misión presenta el valor original, el reverso y el valor resultante, con su cadena. Nunca solo el resultado.
5. La misión cerrada muestra desde entonces, de forma visible, que tiene hallazgos posteriores vinculados.
6. *La razón de no reabrir es dura y deliberada: si un estado terminal puede cambiar meses después, entonces ningún reporte histórico es reproducible, y un reporte no reproducible no sirve para rendir cuentas.* Por eso [`RN-94`](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) es un bloqueo no desactivable: **sin fecha de corte, no reabrir el expediente no sirve de nada, porque el reporte cambia igual.**

**E4 — La conciliación periódica contra una fuente externa produce una diferencia** (posterior al paso 7)

1. Estado de cuenta del tag de peaje, facturación del proveedor de combustible, reporte de multas: cada diferencia **abre expediente de hallazgo posterior** ([`RN-95`](../../01-negocio/reglas/RN-95-conciliacion-contra-fuentes-externas.md)).
2. Anexar evidencia a un expediente cerrado **está permitido; modificarlo no** ([`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)).
3. `[C]` Contratos con proveedores de combustible y de peaje para conciliar — insumo registrado en el índice de reglas.

**E5 — Hay un reclamo de peaje ante la SAPP sin resolver** (en el paso 5)

1. [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) es **bloqueo duro**: las discrepancias **no se dan por cerradas** hasta que el reclamo se resuelva. Un reclamo presentado y jamás respondido no debe ser indistinguible de uno que nadie presentó.
2. `T-21` deja en `[C]` qué expedientes vinculados condicionan el cierre — insumo #1.

> **Nota de hallazgo — `HB4-03`.** Si el reclamo de peaje bloqueara el cierre de la misión, la misión quedaría **atrapada en `LIQUIDADA` durante meses**: un reclamo ante la SAPP no se resuelve en el plazo de liquidación. Y no hay salida por `T-22`, porque **una discrepancia de clasificación no está entre los criterios `H-01` a `H-08`** —`H-03` cubre el *paso incompatible con la ruta*, no el *cobro en categoría equivocada*— y §7.2 declara la lista **cerrada**. El resultado sería exactamente el abandono que `CERRADA_CON_HALLAZGO` viene a evitar.
>
> **Interpretación que este caso de uso aplica:** el bloqueo de [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) recae sobre **el cierre de la discrepancia**, que es un objeto propio de M-18, **no sobre el cierre de la Orden de Misión**. La misión cierra; el reclamo sigue su curso y su resultado económico se registra después por asiento ([`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)).
>
> **No se resuelve aquí.** Hay dos artefactos autoridad involucrados —[orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) en precondiciones de `T-21` y en la lista `H-nn`, y [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) en la materia del reclamo—. Se propone: **(a)** que `T-21` precise que el reclamo de peaje pendiente **no** condiciona el cierre, o **(b)** que se incorpore un criterio `H-09` para *discrepancia de clasificación con reclamo pendiente*, con lo que la salida existiría. Sin una de las dos, el `[C]` del insumo #1 esconde un bloqueo sin salida.

**E6 — Queda una asignación de fondo sin conciliar** (en el paso 4)

1. **`T-21` y `T-22` exigen que todas estén conciliadas**, en cualquiera de las dos formas (§10.1). La desviación no impide conciliar: `CONCILIADA_CON_DESVIACION` **es** una conciliación, y dispara `H-01` en la misión.
2. Lo que no puede quedar es una asignación en `CONSUMIDA` o `LIQUIDADA` sin cruzar contra kilometraje. La salida es completar la conciliación, no cerrar sin ella.

**E7 — La persona que autorizó, despachó o liquidó ya no está en la institución** (en el paso 3)

1. **La autoría histórica no se reasigna jamás.** Cada asiento conserva **persona y puesto** congelados al momento del acto ([actores-y-roles §2.4](../../01-negocio/actores-y-roles.md)).
2. Cuando el auditor pregunta *"¿quién autorizó esto y con qué competencia?"*, el nombre solo no responde: la competencia estaba en el puesto, y el puesto pudo haber cambiado de titular tres veces desde entonces.
3. Los actos pendientes de decisión quedan atribuidos **al puesto**; quien lo ocupe los ve al entrar, y si el puesto queda vacante más allá del plazo parametrizable, **escalan al puesto superior** ([RNF-15](../no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md)).

**E8 — Quedan datos pendientes de sincronizar de dispositivos de esta orden** (en el paso 3)

1. El sistema **distingue *ausente* de *pendiente de sincronización*** y **bloquea el cierre** mientras haya dispositivos con datos pendientes de esa orden ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)).
2. **No se cierra con hallazgo por falta de datos que están en camino.** Cerrar con hallazgo un expediente incompleto por razones de red convierte el hallazgo en ruido.

## Reglas aplicables

| Regla | Qué aporta a este caso |
|---|---|
| [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) | **Regla eje.** Cadena completa para `CERRADA`; salida por `CERRADA_CON_HALLAZGO` con eslabón, motivo y responsable |
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | Quien cierra ≠ quien liquidó; el cierre es de `ACT-08` sin excepción posible |
| [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) · [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) | Inmutabilidad del expediente cerrado; toda corrección posterior es asiento reverso visible |
| [`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md) | El hallazgo posterior es expediente con ciclo propio; **no altera el estado ni los datos del objeto vinculado** |
| [`RN-94`](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) | Toda salida declara su fecha de corte y es reproducible a esa fecha. **No desactivable** |
| [`RN-95`](../../01-negocio/reglas/RN-95-conciliacion-contra-fuentes-externas.md) | Conciliación periódica contra fuentes externas; cada diferencia abre hallazgo posterior |
| [`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) · [`RN-97`](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md) | El cierre de ejercicio no cambia estados; lo no terminal es saldo de apertura |
| [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) | La obligación de reintegro **sobrevive al cierre** y se salda con asiento reverso |
| [`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) · [`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) | Grado de cumplimiento declarado al cerrar; indicadores por causa tipificada |
| [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) | Reclamo con estado y resultado económico — ver nota de hallazgo `HB4-03` |
| [`RN-81`](../../01-negocio/reglas/RN-81-sigti-expone-hechos-a-argos.md) | Los hechos se exponen a ARGOS por la clave de vinculación; SIGTI no escribe en el origen |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) | Umbrales y catálogo de criterios como parámetros con vigencia y doble control |
| [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) | Las consultas de `ACT-12` a datos de personas quedan registradas |

## Trazabilidad

- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) **E13 conciliación y cierre** · puntos de control **`PC-13`** (quien cierra ≠ quien liquidó), `PC-16` (registro de todo acto de autorización), `PC-17` (`ACT-12` solo lee y exporta), `PC-18`
- **Autoridad en transiciones:** [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — **`T-21`**, **`T-22`**, `T-20`; `BD-06`; **§7 `CERRADA_CON_HALLAZGO` y criterios `H-01` a `H-08`**; **§8 estados terminales e inmutabilidad**; §9 auditoría de transiciones; §10.1 exigencia de conciliación para cerrar; §3.4 transiciones prohibidas
- **Autoridad en actores e incompatibilidades:** [actores-y-roles.md](../../01-negocio/actores-y-roles.md) — matriz fila 14 (**cerrar misión: `E` exclusivo de `ACT-08`**), fila 18 y 19; §2.4 rotación con expedientes abiertos; **§4.2 con su corrección: desde un terminal no hay reapertura, ni por `ACT-09`**; §5.2 `I-07`, `I-12`
- **Casos especiales:** [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) (eje del caso), [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md), [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md), [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md), [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md), [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md), [CE-03](../casos-especiales/CE-03-accidente-de-transito-en-mision.md), [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md)
- **Casos de uso encadenados:** ← [CU-15](CU-15-liquidar-la-mision-y-conciliar.md)
- **Requisitos no funcionales:** [RNF-04](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md), [RNF-06](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md), [RNF-18](../no-funcionales/RNF-18-paquetes-de-evidencia-para-auditoria.md), [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md), [RNF-15](../no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md), [RNF-14](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md)
- **Normativa:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — `[I]` la cadena de eslabones exigida para el cierre y la exportación de paquetes de evidencia son **implicaciones de requerimiento escritas por el equipo**, no articulado citable ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), corregido desde `[V]` por `HN1-06`); `[V]` que el hallazgo típico del TSC en flota es el incremento de consumo sin relación con el uso habitual; `[P]` append-only y registro oportuno · [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) cierre de ejercicio · [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) descargo
- **Decisiones:** [DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (viáticos en ARGOS: se muestra el estado por la clave de vínculo, no se liquida), [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md), [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- **Historias:** pendientes — no escritas en este bloque
- **Insumos pendientes:** #1 (umbrales de `H-01`, `H-02` y `H-08`; **qué expedientes vinculados abiertos impiden cerrar**; si los hallazgos reiterados bloquean o advierten; plazo de retención documental; migración histórica) · #19 (informes previos de Auditoría Interna o del TSC sobre flota: **cada hallazgo pasado describe algo que salió mal en la operación real**) · #26 (pronunciamiento de Auditoría Interna) · contratos con proveedores de combustible y de peaje para la conciliación externa de [`RN-95`](../../01-negocio/reglas/RN-95-conciliacion-contra-fuentes-externas.md)
