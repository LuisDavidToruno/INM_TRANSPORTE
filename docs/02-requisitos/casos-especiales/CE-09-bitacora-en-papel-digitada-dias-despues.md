# CE-09 — En Erandique no hay señal: la bitácora se llenó a mano y llega a la delegación doce días después

| Campo | Valor |
|---|---|
| **Módulos** | M-16 Sincronización y Operación Desconectada, M-08 Ejecución y Bitácora, M-15 Formatos Oficiales e Impresión, M-09 Combustible, M-13 Liquidación, M-14 Auditoría, M-03 Flota |
| **Estados afectados** | `DESPACHADA`, `EN_RUTA`, `RETORNADA`, `LIQUIDADA` — y el vehículo en `EN_MISION` |
| **Frecuencia** | **Frecuente.** No es la excepción: en buena parte del país es el modo normal de operar |
| **Impacto** | Auditoría, operativo y financiero |
| **Resolución** | Definida en su mecánica. **Tres decisiones escaladas al PO**: cómputo del plazo de liquidación, plazo máximo de digitación y talonario preimpreso |

> **Este es el caso que decide la adopción del sistema.** Si el motorista de una delegación rural siente que el sistema le exige lo que su zona no le puede dar, vuelve al cuaderno y el expediente electrónico queda vacío justo donde el auditor va a mirar. Más de **2 millones de personas del área rural** de 5 años y más no tienen acceso a internet `[V]` ([INE, EPHPM julio 2025](../../01-negocio/normativa/NRM-09-realidad-operativa.md)), y la brecha no es solo de red: también de equipamiento.

## La situación

La Delegación de Gracias, Lempira despacha una misión de supervisión a Erandique, San Andrés y San Juan. Sale el lunes 3 de agosto a las 05:40 con odómetro **92,318**, fondo para seis días y un pickup doble cabina.

El motorista **no lleva el cliente de campo funcionando**. Puede ser por cualquiera de estas razones, y todas ocurren:

- El teléfono institucional no le fue asignado — en esa delegación hay tres para siete motoristas.
- El equipo salió con carga y se descargó el segundo día; en la casa donde pernoctó no hay energía estable.
- Lo lleva, pero la estación de servicio de Gracias le exige el formato de requisición de siempre y él lo llena en papel porque es lo que el despachador de la bomba le firma.
- Simplemente no lo usó. Lleva veintidós años llenando la hoja preimpresa y en el camino de tierra a San Juan no va a sacar un teléfono.

Así que llena **la hoja de bitácora en papel**: kilometraje de salida, hora, paradas, kilometraje al llegar a cada municipio, dos cargas de combustible con sus facturas grapadas, una novedad ("golpe en el guardabarros trasero al retroceder en San Andrés, miércoles por la tarde"), y el kilometraje de retorno **93,061** el viernes 7 a las 18:20.

Regresa el viernes por la tarde. Entrega llaves al portón. **El encargado de la delegación estaba en Santa Rosa de Copán y volvió el lunes.** La hoja quedó en el escritorio. Ese lunes hubo corte de energía; el martes se fue el enlace de datos y no volvió hasta el jueves siguiente.

**La bitácora se digita el martes 18 de agosto. Doce días después del hecho.**

Mientras tanto, entre el 7 y el 18 de agosto:

- El sistema cree que la misión sigue **`EN_RUTA`** y que el vehículo está **`EN_MISION`**.
- Por [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), ese pickup **no se puede asignar a nada**. Físicamente está en el patio de la delegación.
- El lunes 10 salió a Candelaria con otro motorista, **sin orden de misión**, porque había que salir y el sistema no lo dejaba.
- El plazo de liquidación, que arranca con `T-18`, o ya empezó a correr o no ha empezado. Nadie sabe cuál de las dos.

## Qué se hace hoy sin sistema

La hoja preimpresa del talonario se archiva en un fólder por mes. Al final del mes, o cuando la Gerencia Administrativa lo pide, alguien pasa los datos a una hoja de cálculo. Las facturas de combustible se grapan a la requisición. Si falta una, se busca al motorista y él dice cuánto fue.

Tres prácticas que nadie escribió nunca y que aparecen apenas se pregunta:

1. **El talonario preimpreso tiene su propio folio.** La hoja es el documento. Cuando llegue el sistema habrá **dos numeraciones** — la del talonario y la de [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — y si no se emparejan, el auditor tendrá dos series que no se cruzan. `[C]` insumo #2: obtener el talonario vigente y ver si trae folio impreso.
2. **Cuando el papel viene incompleto, se completa "de memoria".** El odómetro intermedio que no se anotó se deduce restando. Es el dato más sensible del sistema y se está fabricando.
3. **El vehículo se usa mientras el papeleo va atrás.** La salida a Candelaria del lunes 10 no es mala fe: es que la operación no se detiene por un trámite. **Cualquier sistema que la detenga va a ser evadido, no cumplido.**

## Por qué el flujo normal no lo cubre

Hay que decirlo con precisión, porque es fácil confundirse: **el sistema sí es offline-first** ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), premisa rectora 5). El problema **no es la falta de red**. Es la falta de **dispositivo, de batería, de costumbre o de voluntad** — y contra eso el offline-first no hace nada.

Son dos caminos distintos y el diseño los confunde en uno solo:

| Camino | Quién captura | Cuándo | Regla que gobierna |
|---|---|---|---|
| **Captura en campo sin red** | El propio `ACT-06` en su dispositivo | En el momento del hecho, sincroniza después | [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) + marca de diferido de [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) |
| **Papel digitado después** | `ACT-10`, que **no estuvo ahí** | Días después, transcribiendo | [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) — autor ≠ digitador |

Y además el flujo feliz asume que `T-18` ocurre cuando el vehículo entra al portón. Aquí el vehículo entra el viernes 7 y `T-18` se registra el 18. **Once días de divergencia entre el mundo físico y el expediente**, durante los cuales el sistema bloquea un vehículo que está disponible y libera a nadie.

## Regla de resolución

### 1. El papel lo emite el sistema, no lo sustituye

Antes de la salida, en `T-12`, el sistema imprime la **hoja de bitácora de la misión** junto con la orden de misión y los demás documentos de control ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), M-15). La hoja lleva:

- **Folio del rango de la delegación** ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)) y QR de verificación
- Encabezado **prellenado**: misión, vehículo con su correlativo institucional, motorista, ruta autorizada, odómetro de salida, ventana autorizada
- Las casillas vacías en **el mismo orden y con los mismos nombres** que la pantalla de digitación ([`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md): paridad pantalla↔papel)
- Espacio de firma del motorista y de quien recibe

Con esto la digitación deja de ser transcripción de un documento ajeno y pasa a ser **cierre de un documento que el sistema ya emitió y ya numeró**. Y desaparece la doble numeración del punto 1 de "qué se hace hoy".

`[C]` Si la institución conserva su talonario preimpreso, se registra la **correspondencia folio de talonario ↔ folio de sistema** como campo obligatorio de la digitación. No se elige uno y se descarta el otro: el auditor va a pedir el que él conoce.

### 2. El retorno físico libera el vehículo. La bitácora se digita después

Esta es la parte que decide si el sistema se usa. **El vehículo y el motorista no pueden quedar secuestrados por un trámite de oficina.**

Cuando el vehículo vuelve y no hay registro del motorista, `ACT-10` ejecuta [`T-18` con subtipo **"retorno constatado en oficina"**](../../03-arquitectura/estados/orden-de-mision.md) — subtipo que la máquina de estados ya tiene y que exige acta y adjunto. Se registra:

- **Fecha y hora del hecho**: el viernes 7 a las 18:20, la que consta en el papel o la que se constata físicamente
- **Fecha y hora de captura**: la real, no editable ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md))
- **Odómetro leído en el tablero al momento de la constatación**, con fotografía — no el que dice el papel, que se cotejará después
- Acta de recepción con novedades, o su falta declarada

Efectos inmediatos: el vehículo sale de `EN_MISION` y el motorista queda disponible. La Orden de Misión queda en `RETORNADA` **con marca `BITACORA_PENDIENTE_DE_DIGITACION`**, con responsable nombrado y plazo. Es el mismo mecanismo de marca sobre el expediente que la máquina de estados usa en `T-15` y que [`CE-02`](CE-02-averia-mecanica-en-ruta.md) usa para la interrupción: **una marca, no un estado inventado**.

**La marca bloquea `T-19` liquidar.** No bloquea el uso del vehículo. El control se ejerce donde tiene sentido —en el dinero— y no donde solo estorba.

### 3. La digitación es transcripción con autor identificado, y se distingue para siempre

`ACT-10` digita bajo [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md), que ya lo gobierna completo y **no hay que reescribir aquí**: quién digitó, quién es el autor del hecho, adjunto del original fotografiado —la foto con el móvil basta, exigir escáner en Gracias es exigir que la regla no se cumpla—, y la fecha del hecho que consta en el papel.

Dos cosas que este caso agrega y que `RN-47` no resuelve por sí sola:

**a) Lo que el papel no trae, no se inventa.** El odómetro intermedio de San Andrés que nadie anotó se registra como *no consignado en el original*. No se deduce restando. `RN-47` ya lo dice; aquí se subraya porque es la práctica corriente que hay que romper.

**b) El registro diferido es visible para siempre**, en pantalla y en todo reporte. Un hecho registrado el mismo día y uno reconstruido doce días después **no pueden pesar igual ante el auditor**, y hoy pesan igual porque el papel no distingue.

### 4. El odómetro se valida sobre la serie ordenada por fecha del hecho

Aquí hay una trampa que el flujo feliz no ve. Si la misión del 10 al 12 de agosto a Candelaria se digita **antes** que la del 3 al 7, la validación de continuidad del odómetro se hace contra una serie incompleta y da un resultado falso — y peor, un resultado falso que después nadie vuelve a revisar.

La continuidad del odómetro por vehículo se evalúa **sobre la serie ordenada por fecha del hecho**, nunca por orden de captura ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) punto 4, [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)). **Insertar un registro anterior reabre la validación de todos los posteriores**, y las incoherencias que aparezcan van a la cola de resolución de [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md), no se corrigen solas.

El bloqueo duro de [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) —retorno ≥ salida— se evalúa igual en digitación diferida. Si el papel trae un retorno menor al de salida, **no se edita el dato para que pase**: se registra como viene y se abre la discrepancia contra el papel ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) punto 3).

### 5. Y si el papel contradice lo constatado en el portón

En el punto 2 se leyó el odómetro del tablero: 93,061. El papel dice 93,061 también, o dice otra cosa. Si difieren, **es un conflicto de [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)**: ambas versiones quedan, con el adjunto y la fotografía del tablero como evidencia, y lo resuelve una persona. El papel no prevalece sobre lo constatado ni al revés.

### 6. El motorista no puede ser penalizado por la geografía

El indicador de oportunidad de registro que exige TSC-NOGECI V-10 `[V]` debe distinguir el motivo:

| Motivo del diferimiento | A quién se le imputa |
|---|---|
| Sin conectividad en la zona | **Condición de la delegación**, no incumplimiento del motorista |
| Sin dispositivo asignado | **Condición institucional** — es un dato de gestión, no una falta |
| Dispositivo asignado y disponible, no usado | Motorista |
| Papel entregado a tiempo, digitado tarde | **Delegación**, no motorista |

Sin esta distinción el indicador se convierte en un castigo a quien opera donde no hay señal, y el resultado es predecible: dejan de reportar.

### 7. Ninguna misión con la marca sobrevive al cierre del período

Vencido el plazo de digitación, la misión no se cierra en silencio: cierra por [`T-22` como `CERRADA_CON_HALLAZGO`](../../03-arquitectura/estados/orden-de-mision.md), entra al reporte de auditoría y queda con responsable identificado — mismo tratamiento que `PC-18` da a la convalidación vencida en [`CE-01`](CE-01-salida-de-emergencia-convalidada.md).

### Lo que este caso NO cubre

- **La misión que nunca se creó en el sistema** porque la delegación estaba sin red al despachar. Eso es *modo delegación desconectada*: deuda declarada, decisión del PO, insumo **#41**. No se resuelve aquí.
- **La salida sin orden de misión** del lunes 10 a Candelaria. Es uso no amparado: `M-12` y, si fue por emergencia, [`CE-01`](CE-01-salida-de-emergencia-convalidada.md) con convalidación. Este caso especial **elimina su causa más común** —el vehículo bloqueado por un trámite— pero no la legaliza.

## Decisiones escaladas al PO

### D-1 · ¿Desde cuándo corre el plazo de liquidación? `[C]`

`T-18` dispara el plazo de liquidación. Con once días de desfase, la respuesta cambia el resultado.

| Opción | Consecuencia | Costo |
|---|---|---|
| **A — Desde la fecha del hecho** (viernes 7) | Coherente con [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) y [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md). Pero la delegación recibe el expediente **ya vencido** y acumula hallazgos por algo que no controla | Nulo. Y desmoraliza a las delegaciones rurales |
| **B — Desde la fecha de captura** (18 de agosto) | Operativamente cómodo. Pero deja el plazo **a merced de quien digita**: digitar tarde alarga el plazo. Es un incentivo perverso servido en bandeja | Nulo, y abre un hueco de control |
| **C — Desde la fecha del hecho, con suspensión del cómputo mientras dure una incomunicación acreditada** *(recomendada)* | Respeta la fecha del hecho y no castiga la geografía. La suspensión es un hecho registrado, no una excusa verbal | Exige registrar **ventanas de incomunicación por delegación** con responsable que las declara y evidencia. Es un catálogo nuevo pequeño |

**Recomendación: C.** Y la ventana de incomunicación sirve además para el indicador del punto 6.

### D-2 · ¿Cuál es el plazo máximo de digitación diferida? `[C]`

Parámetro `plazo_maximo_digitacion_diferida`, en días hábiles del calendario de la delegación. **No se propone un número**: depende del ciclo real de las delegaciones más aisladas y del mapa de conectividad — insumos **#27** y **#11**. Vencido, la misión cierra por `T-22` con hallazgo.

### D-3 · ¿Se conserva el talonario preimpreso? `[C]`

Depende del insumo **#2** (formatos vigentes). Si se conserva, la correspondencia de folios es campo obligatorio. Si se retira, la hoja emitida por el sistema lo sustituye desde el día uno y hay que presupuestar impresión en cada delegación.

### D-4 · ¿Puede digitar quien después liquida?

[`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) lo deja `[C]` con advertencia registrada, y en una delegación de tres personas `ACT-10` es quien digita **y** quien liquida (`T-19`). Este caso lo agrava: la transcripción y su verificación quedan en la misma mano. **No se resuelve aquí** — es materia de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md), que es la autoridad sobre incompatibilidades. Se reporta como pregunta abierta a Auditoría Interna, ligada al insumo **#27**.

## Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:hoja-de-bitacora-impresa-con-folio` | El sistema emite la hoja de bitácora en papel al despachar, con folio del rango de la delegación, QR, encabezado prellenado y paridad exacta con la pantalla de digitación |
| `RN-c:retorno-constatado-libera-al-vehiculo` | El retorno físico constatado libera vehículo y motorista sin esperar la digitación; la misión queda con marca `BITACORA_PENDIENTE_DE_DIGITACION`, que bloquea la liquidación y no el uso |
| `RN-c:continuidad-de-odometro-por-fecha-del-hecho` | La continuidad del odómetro se evalúa sobre la serie ordenada por fecha del hecho; insertar un registro anterior reabre la validación de todos los posteriores |
| `RN-c:plazo-de-liquidacion-y-ventanas-de-incomunicacion` | El plazo de liquidación corre desde la fecha del hecho y se suspende durante ventanas de incomunicación acreditadas y registradas por delegación — pendiente de D-1 |
| `RN-c:correspondencia-de-folio-papel-sistema` | Cuando la institución conserva talonario preimpreso, la digitación exige el folio del talonario y el del sistema, y ambos quedan cruzados en el expediente — pendiente de D-3 |
| `RN-c:imputacion-del-registro-diferido` | El indicador de oportunidad de registro imputa el diferimiento a la delegación, a la institución o al motorista según motivo tipificado; nunca por defecto al motorista |

## Evidencia que debe quedar

Ante el TSC o Auditoría Interna, encadenado a la misma Orden de Misión:

1. **La hoja de bitácora original**, fotografiada o escaneada, con su folio y la firma del motorista — o la constancia de que no firmó, declarada como observación, no ocultada
2. **Quién digitó y cuándo**, y **quién es el autor del hecho**, como dos personas distintas y ambas identificadas ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md))
3. **Fecha del hecho y fecha de captura** de cada registro, con el desfase visible y su motivo tipificado ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md))
4. El **acta de retorno constatado en oficina** con la fotografía del odómetro del tablero, la hora de la constatación y quién constató
5. Los **campos no consignados en el original**, declarados como tales y no rellenados
6. Las **facturas de combustible** con su folio de asignación, y la conciliación de galonaje–kilometraje calculada sobre la serie ordenada por fecha del hecho ([`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md))
7. Los **conflictos abiertos y su resolución**, con ambas versiones conservadas ([`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md))
8. El **indicador de oportunidad de registro** de la delegación, con el motivo desagregado — que es la respuesta institucional a TSC-NOGECI V-10 `[V]`
9. Si venció el plazo: el **hallazgo** con responsable nombrado y el cierre por `T-22`

## Trazabilidad

- **Reglas**: [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) captura sin conectividad · [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) folios en el cliente · [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) cero sobrescritura · [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) fecha del hecho vs. captura · [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) digitación diferida · [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) cálculo a la fecha del hecho · [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) odómetro de retorno · [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) conciliación · [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) sin doble asignación · [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) cadena para el cierre · [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) documentos impresos con folio y QR · [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) asiento reverso
- **Reglas candidatas**: `RN-c:hoja-de-bitacora-impresa-con-folio`, `RN-c:retorno-constatado-libera-al-vehiculo`, `RN-c:continuidad-de-odometro-por-fecha-del-hecho`, `RN-c:plazo-de-liquidacion-y-ventanas-de-incomunicacion`, `RN-c:correspondencia-de-folio-papel-sistema`, `RN-c:imputacion-del-registro-diferido`
- **Normas**: [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) conectividad `[V]`, prácticas en papel `[I]` · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-10 Registro Oportuno `[V]` · [`NRM-08`](../../01-negocio/normativa/NRM-08-firma-electronica.md) firma manuscrita sobre impresión
- **Transiciones**: `T-12` emisión de documentos con folio · `T-18` subtipo **retorno constatado en oficina**, ejecutable por `ACT-10` · `T-19` bloqueada por la marca · `T-22` cierre con hallazgo · `W-07`, `W-08` estado operativo del vehículo
- **Puntos de control**: `PC-11` coherencia del odómetro · `PC-13` segregación de liquidación y cierre · `PC-16` registro del acto · `PC-18` acto pendiente que bloquea el cierre
- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E8, E11, E12 y E13
- **Actores**: `ACT-06` autor del hecho · `ACT-10` digita y constata el retorno · `ACT-04` liquida · `ACT-12` verifica el indicador · `ACT-05` recibe el vehículo
- **Insumos pendientes**: **#45** cómputo del plazo de liquidación y plazo máximo de digitación (D-1, D-2) · **#46** folio del talonario preimpreso (D-3) · **#47** digitador que también liquida (D-4) · **#2** formatos en papel vigentes · **#11** delegaciones y conectividad · **#27** dotación real de las delegaciones · **#41** modo delegación desconectada · **#32** plazos de liquidación
- **Casos especiales relacionados**: [`CE-01`](CE-01-salida-de-emergencia-convalidada.md) salida de emergencia convalidada · [`CE-02`](CE-02-averia-mecanica-en-ruta.md) marca sobre el expediente sin estado nuevo · [`CE-21`](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) conciliación galonaje–kilometraje · `CE-10` motorista incapacitado en ruta
- **Historias candidatas**: `HU-c:imprimir-hoja-de-bitacora-al-despachar`, `HU-c:constatar-retorno-en-oficina-sin-bitacora`, `HU-c:digitar-bitacora-en-papel-con-adjunto-del-original`, `HU-c:resolver-conflicto-entre-papel-y-registro-sincronizado`, `HU-c:consultar-indicador-de-oportunidad-de-registro-por-delegacion`
