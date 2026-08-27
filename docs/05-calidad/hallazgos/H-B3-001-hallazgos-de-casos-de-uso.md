# H-B3-001 — Hallazgos surgidos al escribir los casos de uso

| Campo | Valor |
|---|---|
| **Origen** | Bloque 3, redacción de `CU-01` a `CU-18` por cuatro analistas en paralelo |
| **Fecha** | 2026-08-13 |
| **Estado** | **17 de 19 corregidos y verificados. 2 siguen abiertos** — desglose en la sección siguiente |
| **Verificación de cierre** | 2026-08-26, contra los artefactos vivos |


## Estado de corrección — verificado el 2026-08-26

Diecisiete de los diecinueve están corregidos y verificados contra los artefactos vivos. **Dos siguen abiertos**, y uno de los cerrados lo está a reserva de una decisión del PO.

| Hallazgo | Estado | Dónde se comprueba |
|---|---|---|
| `HB3-01` | Corregido | `BD-01` compara hoy contra los tres — creador, remitente y **solicitante de derecho**. Es el bloqueo que el código implementa |
| `HB3-02` | Corregido **a reserva del PO** | [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) y `orden-de-mision.md` §7.2. Se descartó abrir un `H-09` para no marcar a la institución por un error del concesionario. **Si el PO prefiere lo contrario, se revierte** — está anotado como `[C]` en la propia máquina de estados |
| `HB3-03` · `HB3-06` · `HB3-12` | Corregidos | [`CU-12`](../../02-requisitos/casos-de-uso/CU-12-solicitar-y-aprobar-fondo-de-combustible.md), [`CU-13`](../../02-requisitos/casos-de-uso/CU-13-emitir-y-entregar-asignacion-de-combustible.md), `orden-de-mision.md` |
| `HB3-04` | Corregido | `PC-11` de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) distingue el **retorno constatado en oficina**, donde el odómetro incoherente no bloquea la captura: se registra, se marca y se bloquea la liquidación. Cierra también `HB1-22` |
| `HB3-05` | Corregido | [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) e `I-11`: quien conduce no pudo haber ejercido ninguna de las funciones 2 a 5 |
| `HB3-07` · `HB3-08` | Corregidos | [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md), [`CU-03`](../../02-requisitos/casos-de-uso/CU-03-permiso-de-circulacion-en-dia-inhabil.md) |
| `HB3-09` | Corregido | [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) |
| `HB3-10` · `HB3-11` | Corregidos | [`CU-05`](../../02-requisitos/casos-de-uso/CU-05-emitir-orden-de-mision-y-documentos.md), [`CU-16`](../../02-requisitos/casos-de-uso/CU-16-cerrar-el-expediente-de-la-mision.md), `PR-01` |
| `HB3-14` | Corregido | [`HU-037`](../../02-requisitos/historias/HU-037-emision-anticipada-en-delegacion-sin-cobertura.md) |
| `HB3-15` | Corregido | [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md): *«El permiso no requiere que la misión esté programada: basta con que esté aprobada»*, y el caso de la misión aprobada el jueves para salir el sábado quedó escrito |
| `HB3-17` | Corregido | [`HU-104`](../../02-requisitos/historias/HU-104-retirar-de-flota-un-bien-ajeno.md) — devolver un comodato ya no obliga a un asiento falso sobre un bien ajeno |
| `HB3-18` | Corregido | [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) |
| `HB3-19` | Corregido | [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) ya no usa *«puesto superior»*: el escalamiento se nombra por el nivel de la cadena |
| **`HB3-13`** | **Abierto** | La interrupción sigue teniendo tres desenlaces en `CE-02` y cuatro en [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), que trata *pendiente de resolución* como si fuera un desenlace. Sin zanjarlo, el tablero del Jefe de Transporte infla el conteo de misiones resueltas |
| **`HB3-16`** | **Abierto** | [`mapa-de-procesos`](../../01-negocio/mapa-de-procesos.md) `PR-02` sigue admitiendo que aprueben el descargo `ACT-08` **o** `ACT-09`; la máquina de estados dice `ACT-08`. `NRM-02` sigue `[P]`, así que la norma no zanja: **es decisión de producto** |
Ninguno de estos hallazgos lo encontró una revisión. Los encontró **alguien usando el diseño** para escribir un flujo concreto y chocando con que no cerraba.

Todos los casos de uso siguieron al artefacto autoridad según la [precedencia de `CLAUDE.md`](../../../CLAUDE.md) y dejaron la divergencia anotada. **Ninguno la resolvió en silencio.**

## Críticos — bloquean o rompen un control

### `HB3-01` — La segregación no bloquea en el escenario más común

`BD-01` compara al autorizador contra **quien creó** y **quien envió** el expediente. Pero `RN-02` establece que cuando la solicitud se captura por encargo, el **solicitante de derecho** es otra persona.

**El caso que ocurre todos los días:** la asistente captura la solicitud para su jefe, y el jefe la autoriza. `BD-01` leída literalmente **no bloquea** — y sin embargo `I-01` sí se está violando.

- **Detectado en:** `CU-01`, `CU-02`
- **Autoridad:** máquina de estados
- **Resolución adoptada por los CU:** verificar contra los tres — creador, remitente y solicitante de derecho
- **Corrección pendiente en:** `orden-de-mision.md`, `BD-01`

### `HB3-02` — Una discrepancia de peaje deja el expediente atrapado para siempre

`RN-92` declara bloqueo duro: las discrepancias de peaje no cierran sin el reclamo resuelto. Pero **un reclamo ante la SAPP tarda meses**, y la discrepancia de clasificación **no está entre los criterios `H-01` a `H-08`**, cuya lista está declarada cerrada.

No hay salida ni por `T-21` ni por `T-22`. **El expediente queda atrapado en `LIQUIDADA`.**

Es exactamente el escenario que `RN-08` evitaba al admitir `CERRADA_CON_HALLAZGO`: *un expediente que no puede cerrarse se abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver.* Una regla escrita después lo reintrodujo.

- **Detectado en:** `CU-16`
- **Salidas propuestas:** que `T-21` precise que el reclamo no condiciona el cierre, **o** que se incorpore un `H-09`
- **Decide:** la máquina de estados

### `HB3-03` — Una misión con faltante de caja nunca sale de `RETORNADA`

`T-19` exige que todas las asignaciones estén `LIQUIDADAS`, y §10.1 define `LIQUIDADA` como *"cuadran asignado, consumido, comprobado y saldo devuelto"*. Leído literalmente, **una misión con faltante no puede liquidarse nunca** — lo que contradice a `RN-86`, cuya obligación de reintegro *"sobrevive al cierre de la misión"*.

- **Detectado en:** `CU-15`
- **Resolución adoptada:** liquidar es **declarar** el resultado, faltante incluido
- **Corrección pendiente en:** `orden-de-mision.md`, redacción de `T-19` y §10.1

### `HB3-04` — `BD-05` contra `RN-79`: el retorno constatado

`BD-05` bloquea la captura de un odómetro de retorno menor al de salida. `RN-79` dice que la constatación *"se registra tal cual, se marca la inconsistencia y el vehículo se libera igual"*. **Son incompatibles.**

- **Detectado en:** `CU-10`
- **Autoridad:** máquina de estados; `CU-10` siguió a `BD-05`
- **Salida sugerida:** distinguir el `T-18` ordinario del subtipo *retorno constatado en oficina*

### `HB3-05` — Quien se habilita a sí mismo

**No existe ningún par `I-nn` que cubra "habilita × es habilitado".** En una delegación pequeña, el `ACT-04` que también conduce puede darse por buena su propia licencia — el único dato del que depende `BD-02`, el bloqueo de mayor valor legal del sistema.

- **Detectado en:** `CU-18`
- **Tratamiento provisional:** se permite con motivo escrito y marca de acumulación vigilada
- **Decide:** `actores-y-roles.md` + pronunciamiento de Auditoría Interna

### `HB3-06` — El fondo: solicita × aprueba tampoco existe

El par *solicita fondo × aprueba fondo* no está en `I-01` a `I-17`. Hoy vive solo en el numeral 4 de `RN-26`, porque `RN-01` razona **por Orden de Misión** y el fondo es objeto **de período**. `I-17` tiene el mismo problema.

- **Detectado en:** `CU-12`. Ya señalado como `HN1-15`
- **Decide:** `actores-y-roles.md`

## Altos — contradicciones que hay que zanjar

### `HB3-07` — El salvoconducto tiene tres redacciones

| Artefacto | Qué ampara |
|---|---|
| `BD-04` | vehículo y ventana |
| `PC-03` | vehículo, motorista y ventana |
| `RN-23` | vehículo, motorista, ruta y ventana |

No es cosmético: **decide si un relevo de motorista invalida el permiso** en carretera, frente a un agente que lo revisa físicamente.

- **Detectado en:** `CU-03`, `CU-06`. Ya abierto desde el Bloque 1
- **Resolución adoptada:** la más exigente — `RN-23`: vehículo, motorista, ruta y ventana. **Un relevo invalida el permiso.** Corregido en `BD-04` el 2026-08-25, tras una primera corrección que había adoptado la lectura contraria

### `HB3-08` — `BD-04` ignora el vehículo de servicio exceptuado

`PC-03` y `RN-24` reconocen la excepción para emergencia, seguridad y salud; `BD-04` no. **Una ambulancia con excepción vigente no podría despacharse un domingo.**

- **Detectado en:** `CU-03`. Ya abierto como `HB1-21`, sin corregir

### `HB3-09` — La categoría de peaje: advertencia o bloqueo

`PR-01` E5 y `PC-06` dicen **advertencia**; `T-08` y `BD-07` la exigen como precondición (**bloqueo**); `RN-91` bloquea en el despacho. Tres momentos y dos severidades.

- **Detectado en:** `CU-04`. Resolución adoptada: bloqueo en `T-08`, siguiendo la máquina de estados

### `HB3-10` — Quién emite la Orden de Misión, y cuándo

`PR-01` E6 atribuye la emisión a `ACT-04` en `PROGRAMADA`. La máquina de estados dice que en `PROGRAMADA` no se puede imprimir documento válido, y que `EF-02` consume el folio y emite **dentro de `T-12`**, que ejecuta `ACT-05`.

Misma clase de error que `HB1-06`, el del fondo entregado antes de tiempo.

- **Detectado en:** `CU-05`. Corrección pendiente en `PR-01`

### `HB3-11` — Quién cierra la bitácora

`PR-01` E11 dice `ACT-05`; `T-18` la atribuye a `ACT-06` o `ACT-10`. **Una delegación sin despachador un sábado a las 21:00 tiene que poder registrar el retorno.**

- **Detectado en:** `CU-10`. Corrección pendiente en `PR-01`

### `HB3-12` — Cambio de vehículo con la misión `EN_RUTA`

`T-17` no lo cubre. Ya conocido por avería (`CE-02`), ahora alcanzado por segundo camino: **la decisión administrativa**, sin avería de por medio.

- **Detectado en:** `CU-07`

## Medios

- **`HB3-13`** — La interrupción tiene tres desenlaces según `CE-02` y cuatro según `RN-70`, que trata *pendiente de resolución* como ausencia de desenlace. Decide si el tablero del Jefe de Transporte infla el conteo de misiones resueltas. `CU-09` siguió a `RN-70`.
- **`HB3-14`** — "Emisión anticipada" solo es compatible con `INV-15` bajo la lectura de ejecutar `T-12` sin red con folio del rango local. Si la institución necesita imprimir con la misión aún `PROGRAMADA`, hace falta **decisión de producto explícita**.
- **`HB3-15`** — `RN-23` dice que basta estar `APROBADA` para tramitar el permiso, y a la vez que el permiso es nominativo por vehículo y motorista — que solo existen tras `T-08`. Resolución adoptada: el expediente se abre en `APROBADA`, la firma exige vehículo y motorista resueltos.
- **`HB3-16`** — Quién aprueba el descargo de un bien: la máquina de estados dice `ACT-08`, el mapa de procesos admite `ACT-08` o `ACT-09`, `NRM-02` está `[P]`.
- **`HB3-17`** — Falta el estado terminal `RETIRADO_DE_FLOTA`. Hoy devolver un comodato solo puede registrarse como `DADO_DE_BAJA`, que es **asiento falso sobre un bien ajeno**. Ya abierto desde el Bloque 2.
- **`HB3-18`** — Ningún artefacto define cómo se distingue la **vista previa impresa** del documento válido. Escalado al PO, sin regla que lo gobierne.
- **`HB3-19`** — "Puesto superior" frente a "nivel de la cadena de ARGOS": dos formas de nombrar el escalamiento de `RN-02`.

## Qué hacer con esto

Ocho de los diecinueve tocan la **máquina de estados**, que es artefacto autoridad. Conviene una **sola pasada de corrección** sobre ella, no parches sucesivos — es lo que se aprendió en el Bloque 1.

Tres requieren decisión del PO antes de poder corregirse: `HB3-02` (qué salida se abre para el reclamo de peaje), `HB3-05` y `HB3-06` (dos incompatibilidades que faltan en la matriz, y que además dependen del pronunciamiento de Auditoría Interna del insumo #26), y `HB3-14` (si se necesita imprimir con la misión aún `PROGRAMADA`).

Dos ya venían del Bloque 1 sin corregir: `HB3-08` y `HB3-07`.
