# CU-10 — Registrar el retorno y cerrar la bitácora

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora |
| **Actor principal** | `ACT-06` Motorista, en el dispositivo y **sin conectividad** |
| **Actores secundarios** | `ACT-10` Encargado de Delegación en digitación diferida y en retorno constatado · `ACT-05` Encargado de Despacho recibe el vehículo · `ACT-07` Encargado de Combustible recibe sobrante y comprobantes · `ACT-13` Custodio · `ACT-11` Encargado de Mantenimiento si hay novedades · `ACT-04` Jefe de Transporte recibe la conciliación |
| **Precondiciones** | La misión está `EN_RUTA` con bitácora abierta (`INV-24` a `INV-28`), o `DESPACHADA` con consumo y sin ejecución (camino `T-16`). |
| **Postcondiciones** | La misión está `RETORNADA` (`INV-29` a `INV-33`): hay odómetro y hora real de retorno, la bitácora está **cerrada** y no admite eventos nuevos, existe acta de recepción con novedades, la conciliación `EF-05` está calculada y sus desviaciones tipificadas, y **el vehículo ya no está `EN_MISION`**. El plazo de liquidación empezó a correr. |
| **Disparador** | El vehículo entra al predio, o se constata físicamente su retorno sin que el motorista lo haya registrado. |

---

## La regla que decide si el sistema se usa

**`T-14` y `T-18` las ejecuta el motorista `ACT-06` en su dispositivo y sin conectividad. No las ejecuta el despachador.**

Una delegación sin despachador un sábado a las 21:00 tiene que poder registrar el retorno. Y **el retorno físico constatado libera el vehículo y al motorista sin esperar la digitación de la bitácora** ([`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md)).

La razón es aritmética: en una delegación con dos vehículos, dejar una unidad inmovilizada porque la bitácora de papel todavía no se digitó suprime el 50 % de su capacidad de transporte por una razón administrativa. La consecuencia real y observada es que **la siguiente salida se hace sin Orden de Misión** — con lo cual el sistema no solo no controló ese viaje: empujó a que ocurriera fuera de él.

El control que interesa al Tribunal Superior de Cuentas no es que el vehículo esté congelado: es que el kilometraje, el combustible y la custodia queden completos **antes de liquidar**. Por eso la marca `BITACORA_PENDIENTE_DE_DIGITACION` **bloquea `T-19` y no bloquea la asignación del vehículo**. El control se ejerce donde tiene sentido —en el dinero— y no donde solo estorba.

---

## Flujo principal

1. `ACT-06` llega al predio y abre la misión en el dispositivo. **No necesita señal, ni que haya alguien de oficina presente.**
2. Registra el **odómetro de retorno**, con fotografía del tablero, y la **hora real del hecho**. El dispositivo evalúa `BD-05` localmente.
3. Selecciona el **subtipo de retorno**: normal, anticipado, sin vehículo, o constatado en oficina. Todos exigen motivo obligatorio salvo el normal.
4. Registra el **acta de recepción del vehículo con novedades declaradas**: estado general, daños nuevos con fotografía, nivel de tanque, herramientas, llanta de repuesto, documentos a bordo. El nivel de tanque es dato de bitácora, no sustituto del registro de abastecimiento ([`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)).
5. **Devuelve la custodia de la misión** contra firma a `ACT-05`, a `ACT-13` o a quien reciba. `ACT-13` recupera la custodia patrimonial permanente ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
6. Entrega a `ACT-07` —de quien los recibió— el **sobrante del fondo** y los comprobantes físicos: tickets de peaje, facturas de combustible, actas de entrega. Las asignaciones vinculadas pasan a pendientes de liquidación ([`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)).
7. Se aplica `T-18`. La misión pasa a `RETORNADA` y **la bitácora se cierra**: no admite eventos nuevos y toda corrección posterior es un asiento ([`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)).
8. El **vehículo sale de `EN_MISION`**: pasa a `DISPONIBLE` (`W-06`), a `EN_TALLER` si las novedades lo requieren (`W-07`), o a `NO_DISPONIBLE` si hay incidente bajo investigación (`W-08`). Las novedades declaradas por el motorista pueden generar orden de trabajo en M-11 ([`DP-001, D-08`](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).
9. El **motorista vuelve a disponible**, salvo que el retorno se haya registrado con evento de incapacidad o su habilitación haya vencido durante la misión ([`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md), [`RN-55`](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md)).
10. La misión declara el **grado de cumplimiento de su objeto, por destino y consolidado** ([`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md)). Un retorno no dice por sí solo si la misión sirvió para algo.
11. Al aplicarse en el servidor, `EF-05` **dispara la conciliación completa** —combustible, peajes, kilometraje y tiempos—, con desviaciones tipificadas **en ambas direcciones**. Ninguna conciliación bloquea el registro del retorno; todas alimentan la liquidación y los criterios de hallazgo ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)).
12. Empieza a correr el **plazo de liquidación**, en días hábiles según el calendario de la delegación, con alerta y escalamiento al vencerse. `[C]` el plazo — insumo #32.

---

## Flujos alternos

**A1 — Retorno el sábado a las 21:00, sin despachador en la delegación** (desde el paso 1)
1. `ACT-06` ejecuta `T-18` **completo desde su dispositivo, sin red y sin nadie de oficina**. Esta es la condición normal de operación en delegación, no una excepción.
2. La constatación por una segunda persona se registra cuando exista. Quien constata **no puede ser el motorista que retorna** ([`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)); si en la delegación no hay otra persona, **se registra así, con motivo**, y el hecho entra al indicador de la delegación — no bloquea el retorno.
3. La liberación del vehículo es **local hasta que sincronice**: el dispositivo ya lo trata como disponible, y el servidor lo confirma al recibir el diario ([`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md)).

**A2 — Retorno constatado en oficina** (desde el paso 1)
1. El vehículo entró al predio y **no hay registro del motorista** — se quedó sin batería, no llevaba dispositivo, o la zona no tenía señal ([`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)).
2. `ACT-10` o `ACT-05` ejecuta `T-18` subtipo **retorno constatado en oficina**, que exige acta y adjunto, registrando: **fecha y hora del hecho** —la que consta en el papel o la que se constata físicamente—, fecha y hora de captura reales y no editables, **odómetro leído del tablero con fotografía** —no el que dice el papel, que se cotejará después—, y acta de recepción con novedades o su falta declarada.
3. **Efecto inmediato: el vehículo sale de `EN_MISION` y el motorista queda disponible.** La Orden queda `RETORNADA` con la marca **`BITACORA_PENDIENTE_DE_DIGITACION`**, con responsable nombrado y plazo.
4. La marca **bloquea `T-19` liquidar** y **no bloquea** la asignación del vehículo ni del motorista a una nueva Orden de Misión ([`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md)).
5. La marca es **visible** en el expediente, en el listado de la delegación y en el reporte de oportunidad de registro, con los días transcurridos desde la fecha del hecho.

**A3 — La bitácora se digita días después desde el papel** (después de A2)
1. `ACT-10` digita bajo [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md): quién digitó, **quién es el autor del hecho** —que no es el mismo—, adjunto del original fotografiado con el móvil, y la fecha del hecho que consta en el papel.
2. **Lo que el papel no trae, no se inventa.** El odómetro intermedio que nadie anotó se registra como *no consignado en el original*; no se deduce restando.
3. **El registro diferido es visible para siempre**, en pantalla y en todo reporte: un hecho registrado el mismo día y uno reconstruido doce días después no pueden pesar igual ante el auditor.
4. La continuidad del odómetro se evalúa **sobre la serie ordenada por fecha del hecho**, nunca por orden de captura. **Insertar un registro anterior reabre la validación de todos los posteriores**, y las incoherencias van a la cola de resolución humana ([`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md), [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)).
5. El motivo del diferimiento se imputa a quien corresponde: sin conectividad es **condición de la delegación**; sin dispositivo asignado es **condición institucional**; papel entregado a tiempo y digitado tarde es de la **delegación**, no del motorista.
6. `[C]` Desde cuándo corre el plazo de liquidación y cuál es el plazo máximo de digitación diferida — [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) D-1 y D-2, insumo #32.

**A4 — Retorno anticipado: la misión se abortó** (desde el paso 3)
1. Subtipo con motivo obligatorio ([`CE-07`](../casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md)).
2. La liquidación es **por lo efectivamente ejecutado**, y la misión declara su grado de cumplimiento por destino: qué se hizo, qué no y por qué ([`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md)).
3. Las reservas y las asignaciones futuras vinculadas se liberan de inmediato: el vehículo vuelve al conjunto asignable ese mismo día ([`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md)).
4. `[C]` Quién puede ordenar el retorno anticipado — insumo #50.

**A5 — Retorno sin vehículo** (desde el paso 3)
1. Siniestro total, robo, decomiso o retención por autoridad. **Exige expediente de incidente vinculado en M-12.**
2. El odómetro se declara **estimado y se marca como tal**; nunca se presenta como leído.
3. El vehículo pasa a `NO_DISPONIBLE` por `W-08` con causa tipificada y **permanece en el registro** hasta su recuperación o descargo ([`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)).
4. **`RN-79` no aplica**: no hay unidad que liberar. Sí se libera el motorista, salvo impedimento propio.

**A6 — La misión no se ejecutó pero hubo consumo** (desde el paso 1)
1. El vehículo nunca salió y el fondo entregado ya se consumió parcial o totalmente — el caso típico es que el motorista llenó el tanque la tarde anterior y la misión se suspendió esa noche.
2. `ACT-04` o `ACT-10` ejecuta `T-16` hacia `RETORNADA`, con motivo tipificado. **No es una anulación**: hubo movimiento de fondos públicos y anular sería borrar un hecho económico.
3. Se cierra la bitácora sin eventos de ruta y la conciliación se limita a fondo entregado contra consumido contra devuelto. La misión queda marcada como **no ejecutada** para que no contamine los indicadores de kilometraje y rendimiento ([`CE-20`](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md)).

**A7 — El vehículo vuelve con novedades que exigen taller** (desde el paso 4)
1. La novedad declarada por el motorista lleva el vehículo a `EN_TALLER` (`W-07`) y puede generar orden de trabajo en M-11.
2. El vencimiento de documentación no cambia el estado del vehículo mientras está `EN_MISION` —ya salió—, pero **al retornar lo lleva a `NO_DISPONIBLE`** ([`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md)).

---

## Flujos de excepción

**E1 — El odómetro de retorno es menor que el de salida** (en el paso 2)
1. `BD-05` **bloquea la captura**: es físicamente imposible, y bloquear aquí es corregir, no ocultar.
2. **Única salida:** que exista **acta previa de sustitución o reinicio de odómetro** registrada por `ACT-11` antes de la salida, con la lectura del instrumento retirado y del instalado. Entonces el kilometraje acumulado se calcula sumando tramos y el bloqueo no aplica ([`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md), [`RN-90`](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md), [`CE-22`](../casos-especiales/CE-22-odometro-inconsistente.md)).
3. **Ver la nota de hallazgo al pie**: [`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md) dice otra cosa para el caso de la constatación.

**E2 — El odómetro de retorno es igual al de salida en una misión ejecutada** (en el paso 2)
1. **Permitido, pero exige justificación**: es el patrón de la misión que nunca se hizo.
2. Se registra y se marca para revisión; deriva en `H-02` si no se justifica.

**E3 — El kilometraje recorrido está muy por encima o por debajo de lo estimado** (en el paso 11)
1. **No bloquea el registro del retorno.** Un sistema que se niega a registrar que el vehículo volvió con 900 km de más no evita el problema: lo deja fuera del expediente, que es exactamente lo que el auditor busca y no encuentra (principio `P-2`).
2. Exige justificación con causa tipificada y marca la misión; la desviación se vigila **en ambas direcciones** (`H-01`, `H-02`).

**E4 — No hay quien reciba el vehículo ni firme el acta** (en el paso 5)
1. La custodia **se cierra igual**: consta el impedimento y firman dos personas presentes, o se declara la recepción pendiente con responsable nombrado ([`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md), [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
2. **No se detiene el retorno por falta de firma.** Un tramo en el que nadie responde por el vehículo es exactamente el tramo del que va a preguntar la auditoría si algo pasó.

**E5 — El papel contradice lo constatado en el portón** (después de A2 y A3)
1. En la constatación se leyó 93,061 del tablero; el papel dice otra cosa.
2. Es **conflicto de [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)**: ambas versiones quedan, con el adjunto y la fotografía del tablero como evidencia, y **lo resuelve una persona**. El papel no prevalece sobre lo constatado ni al revés. Ver [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md).

**E6 — La delegación perdió la hoja de bitácora en papel** (después de A2)
1. El retorno ya está constatado y el vehículo operando: **eso no se deshace**.
2. La bitácora se reconstruye con lo que exista por la vía de [`RN-85`](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md), y **lo que no se recupere se declara perdido, no vacío**. La misión cierra con hallazgo.

**E7 — Vence el plazo de digitación con la marca todavía viva** (después de A2)
1. La misión **no se cierra en silencio**: alerta con escalamiento y, al cerrarse, `T-22` `CERRADA_CON_HALLAZGO`, con responsable identificado y entrada al reporte de auditoría.
2. Mismo tratamiento que `PC-18` da a la convalidación vencida en [`CE-01`](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md).

**E8 — Alguien intenta reabrir la ejecución después de `RETORNADA`** (en el paso 7)
1. `RETORNADA → EN_RUTA` **no existe**: reabrir permitiría agregar eventos con fecha del hecho anterior sin control.
2. El camino es un **asiento de corrección sobre la bitácora cerrada**, con valor anterior, valor nuevo, motivo, autor y fundamento ([`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)).

**E9 — El motorista sale a otra misión antes de que se digite la bitácora anterior** (después de A2)
1. **Es el caso normal y por eso existe `RN-79`.** El odómetro de salida de la nueva misión se toma del tablero, no del sistema; al digitarse la bitácora vieja, el sistema concilia ambos y abre conflicto si no cuadran.
2. Lo que **sí** puede bloquear la nueva salida es el **dinero**: fondo de la misión anterior no devuelto ni comprobado, que es obligación de reintegro con responsable y ciclo propio ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)). Es un bloqueo de dinero, no de trámite.

---

## Reglas aplicables

| Regla | Qué gobierna en este caso |
|---|---|
| [`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md) | **Regla rectora**: el retorno constatado libera vehículo y motorista sin esperar la digitación |
| [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) · [`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) · [`RN-90`](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md) | Odómetro, kilometraje acumulado e intervención del instrumento |
| [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) · [`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) | Devolución de la custodia con constancia |
| [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) · [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) | Bitácora cerrada; corrección solo por asiento |
| [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) · [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) · [`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) | `T-18` sin red, digitación diferida y hoja de papel emitida por el sistema |
| [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) | Papel contra tablero: conflicto humano, no sobrescritura |
| [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) · [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) · [`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) · [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) | Entrega del sobrante, conciliación y reintegro |
| [`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) · [`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) | Grado de cumplimiento e indicadores por causa |
| [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) · [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) · [`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) | Estado del vehículo al retornar |
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | Quien constata no es quien retorna |
| [`RN-55`](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) | Habilitación vencida en ruta: no detiene, pero cierra con hallazgo |

---

## Nota de hallazgo — no se resuelve aquí

**`BD-05` bloquea la captura; `RN-79` dice que la constatación se registra igual.**

- La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) establece en `BD-05` que *"odómetro de retorno < odómetro de salida"* es **bloqueo duro de captura** en `T-18`, con una única salida: acta previa de sustitución o reinicio de odómetro registrada por `ACT-11`.
- [`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md), en sus casos límite, dice que un retorno constatado con odómetro menor al de salida *"no se acepta silenciosamente, pero **tampoco impide la constatación**: se registra la lectura tal cual, se marca la inconsistencia y el vehículo se libera igual salvo que el instrumento esté declarado averiado"*.

Son incompatibles en el caso concreto: o se bloquea la captura o se registra tal cual. **Por precedencia, la autoridad en precondiciones y bloqueos es la máquina de estados**, y este caso de uso sigue a `BD-05` en el flujo principal (E1).

Pero la tensión es real y merece decisión, no arbitraje silencioso: si se bloquea, un vehículo que volvió queda sin registrar su retorno hasta que alguien de mantenimiento levante un acta —justo el efecto que `RN-79` existe para evitar—; si se registra tal cual, se admite en la bitácora un dato imposible. **La salida razonable, que no se adopta aquí porque no corresponde a este artefacto, es distinguir el bloqueo del `T-18` ordinario del subtipo *retorno constatado en oficina*, donde la lectura es evidencia física tomada por un tercero identificado.** Se reporta hacia `orden-de-mision.md` §`BD-05`.

---

## Trazabilidad

- **Transiciones:** `T-18` con sus cuatro subtipos · `T-16` misión no ejecutada con consumo · `T-19` queda bloqueada por la marca de bitácora pendiente · `W-06`, `W-07`, `W-08` estado operativo del vehículo
- **Invariantes y efectos:** `INV-29` a `INV-33` · `EF-05` conciliación disparada al retornar
- **Bloqueos duros:** `BD-05` odómetro · `BD-08` divergencias de sincronización bloquean `T-19`
- **Prohibidas:** `RETORNADA → EN_RUTA` · `EN_RUTA → ANULADA`
- **Criterios de hallazgo:** `H-01`, `H-02`, `H-04`, `H-05`, `H-08`
- **Puntos de control de `PR-01`:** `PC-11` coherencia del odómetro · `PC-18` convalidación vencida
- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapa E11
- **Casos especiales:** [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) · [`CE-07`](../casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) · [`CE-22`](../casos-especiales/CE-22-odometro-inconsistente.md) · [`CE-20`](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) · [`CE-21`](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) · [`CE-26`](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) · [`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) · [`CE-03`](../casos-especiales/CE-03-accidente-de-transito-en-mision.md) · [`CE-04`](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md)
- **Casos de uso relacionados:** [`CU-08`](CU-08-ejecucion-en-ruta-sin-conectividad.md) lo precede · [`CU-09`](CU-09-interrupcion-en-ruta-y-desenlace.md) desemboca aquí en dos de sus desenlaces · [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md) resuelve las divergencias que impiden liquidar
- **Normativa:** [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` conectividad rural, papel y coherencia del odómetro · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) registro oportuno y conciliación · [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) custodia y permanencia del bien
- **Requisitos no funcionales:** [`RNF-03`](../no-funcionales/RNF-03-operacion-sin-conectividad.md) · [`RNF-12`](../no-funcionales/RNF-12-uso-en-campo.md) · [`RNF-04`](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) · [`RNF-11`](../no-funcionales/RNF-11-formatos-oficiales-imprimibles-y-verificables.md)
- **Historias:** pendientes del Bloque 3
- **Insumos pendientes:** #32 (plazo de liquidación, plazo máximo de digitación diferida, desde cuándo corre) · #50 (quién ordena el retorno anticipado) · #27 (¿puede digitar quien después liquida? — pregunta abierta a Auditoría Interna) · #1 (tolerancias y umbrales)
