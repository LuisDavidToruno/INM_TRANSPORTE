# CU-15 — Liquidar la misión y conciliar

| Campo | Valor |
|---|---|
| **Módulos** | M-13 Liquidación y Cierre · M-09 Combustible · M-18 Peajes |
| **Actor principal** | `ACT-04` Jefe de Transporte · `ACT-10` Encargado de Delegación en su ámbito |
| **Actores secundarios** | `ACT-07` Encargado de Combustible — aporta la liquidación del fondo que entregó, **no elabora el descargo** · `ACT-06` Motorista — aporta comprobantes y remanente, **no liquida su propia misión** · `ACT-08` Gerencia Administrativa — puede devolver la liquidación (`T-20`) · `ACT-12` Auditor Interno — consulta |
| **Precondiciones** | La misión está en **`RETORNADA`** tras `T-18` o `T-16`. La bitácora está cerrada (`INV-30`). La **conciliación automática ya está calculada** y sus desviaciones tipificadas (`EF-05`, `INV-32`). El plazo de liquidación está corriendo desde el retorno |
| **Postcondiciones** | La misión está en **`LIQUIDADA`** con su **resultado económico congelado** junto con los identificadores de las tablas paramétricas usadas (`INV-38`), toda desviación fuera de umbral tipificada y justificada (`INV-35`), todas las asignaciones de fondo `LIQUIDADAS` (`INV-34`), y una **propuesta de clasificación de cierre** evaluada contra `H-01` a `H-08`. **La propuesta no cierra nada** |
| **Disparador** | `T-18` registrar retorno dispara `EF-05` y abre el plazo de liquidación, con alerta y escalamiento al vencerse |

> **Lo que aquí se liquida es combustible y peajes. Los viáticos están fuera de alcance** y los gestiona ARGOS ([DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). SIGTI conserva la clave de vínculo y muestra el estado del viático por esa clave; no lo calcula, no lo liquida y no lo espera para continuar ([`RN-81`](../../01-negocio/reglas/RN-81-sigti-expone-hechos-a-argos.md)).

> **Lo que el auditor busca no son comprobantes archivados: es correlación entre consumo, kilometraje y misión autorizada** ([NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)). Este caso de uso existe para producir esa correlación. Un sistema que solo archiva facturas no responde a lo que se le va a preguntar.

## Flujo principal

1. `ACT-04` abre el **descargo conciliado** de la misión. `T-19` — [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) E12.
2. El sistema verifica la **segregación** por identidad de persona, `PC-13` y `BD-06`: quien liquida ≠ el motorista (`I-11`), ≠ quien entregó el fondo (`I-10`, **núcleo irreductible**), ≠ quien despachó (`I-09`), ≠ quien autorizó la necesidad (`I-07`, **núcleo irreductible**). El par *emite la Orden × liquida la misma misión* (`I-14`) es **configurable y está apagado por defecto**.
3. El sistema verifica `BD-08`: **no hay divergencias de sincronización sin resolver** en esta misión. Liquidar sobre dos versiones del retorno produce un número que no significa nada, y ese número acabaría en un reporte del TSC.
4. El sistema presenta la **lista de verificación de la cadena de trazabilidad**, eslabón por eslabón, con su estado *presente*, *ausente* o *no aplicable con fundamento* ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)):

   `solicitud → autorización → orden de misión → asignación de vehículo y motorista → bitácora con odómetro de salida y retorno → asignación y consumo de combustible → registro de peajes → liquidación`

5. **Liquidación del fondo de combustible** ([`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)): asignado vs. entregado vs. consumido vs. comprobado vs. saldo devuelto. Cada consumo con su comprobante, estación, odómetro del momento y fotografía. `ACT-07` aporta la liquidación del fondo que entregó; **no elabora el descargo** (matriz de permisos, fila 13, nota 8).
6. **Conciliación galonaje ↔ kilometraje** ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)):

   ```
   rendimiento observado = kilómetros recorridos / galones consumidos
   ```

   contra el **rendimiento esperado del vehículo vigente a la fecha del hecho**, del paquete congelado (`EF-03`). **La desviación se detecta en ambas direcciones, con umbrales superior e inferior independientes.** Un umbral único simétrico es un error de diseño: un exceso de consumo del 20 % y un ahorro del 20 % no significan lo mismo.

   | Dirección | Qué significa |
   |---|---|
   | **Rendimiento por debajo del esperado** | Más galones de los que el recorrido justifica: posible consumo no imputable a la misión |
   | **Rendimiento por encima del esperado** | Menos galones de los que el recorrido exige. **Casi siempre significa un despacho que nadie anotó**: el vehículo cargó combustible que no pasó por ningún folio — [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) explica la causa, [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) el síntoma |

   Entran en el numerador **todos los abastecimientos, cualquiera sea su fuente** ([`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)). **No entra en el denominador** el kilometraje recorrido bajo tenencia ajena ([`RN-63`](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)).
7. **Conciliación de peajes** ([NRM-10](../../01-negocio/normativa/NRM-10-peajes.md), `EF-05`): estimado vs. pagado **punto por punto**, con **causa tipificada de cada diferencia** — cambio de tarifa entre aprobación y ejecución, ruta distinta a la autorizada, paso adicional no previsto, cobro en categoría equivocada, o peaje pagado sin paso registrado. Se verifica además:
   - **correlación peaje × kilometraje × ruta autorizada** — un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y el sistema debe producirlo solo;
   - **coherencia geográfica y temporal** de la secuencia de casetas ([`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)), evaluada contra el **alcance vigente a la fecha de cada hecho** ([`RN-77`](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md));
   - las **discrepancias de clasificación**, que se agregan al **expediente de reclamo** por punto, clase de vehículo y período ([`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md)) — el sobrecosto se registra tipificado y **no se le imputa al motorista** ([`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md)).
8. **Conciliación de kilometraje y tiempos**: recorrido según bitácora vs. distancia de la ruta autorizada; tiempos de espera en sitio (M-19) vs. lo previsto; coherencia entre hora de salida, eventos de ruta y hora de retorno. El kilometraje se calcula sobre el **acumulado del expediente del vehículo**, no sobre la lectura cruda del instrumento ([`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md)).
9. `ACT-04` **tipifica y justifica cada desviación fuera de umbral** (`INV-35`). Una desviación amparada por causa registrada y aceptada —retorno anticipado, extensión autorizada, espera improductiva declarada— **no produce hallazgo por sí sola**.
10. Se registra el **saldo devuelto** con acta: folios de vale devueltos **uno por uno**, monto, quién devolvió, **quién recibió**, con la segregación verificada por identidad de persona. La **fecha del hecho de la devolución es la fecha en que el dinero entró a la caja**, no la del retorno ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)). **El monto asignado no se ajusta hacia abajo para que cuadre**: está congelado desde la entrega y corregirlo es reescribir el pasado.
11. Todas las asignaciones de fondo vinculadas quedan en **`LIQUIDADA`** (`V-07`, o `V-08` con acta de extravío). `INV-34`.
12. Se declara el **grado de cumplimiento del objeto de la misión**, por destino y consolidado ([`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md)). Una misión puede cuadrar en dinero y no haber cumplido su objeto; son cosas distintas y ambas se declaran.
13. El sistema **congela el resultado económico** con los identificadores de las tablas paramétricas usadas (`INV-38`, [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)), calcula los indicadores de la misión —kilómetros, rendimiento real km/galón, costo de combustible, costo de peajes, desviación contra estimado, tiempos de espera— y **propone la clasificación de cierre** evaluando `H-01` a `H-08`.
14. La misión pasa a **`LIQUIDADA`** (`T-19`). Continúa en [CU-16](CU-16-cerrar-el-expediente-de-la-mision.md). **Cerrar es acto de `ACT-08`, no de quien liquidó.**

## Flujos alternos

**A1 — Misión no ejecutada con consumo (`T-16`)** (desde el paso 1) · [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md)

1. El vehículo nunca salió, pero el fondo entregado ya se consumió parcial o totalmente —el caso típico: el motorista llenó el tanque la tarde anterior y la misión se suspendió esa noche—.
2. **No es una anulación.** Hubo movimiento de fondos públicos; anular sería borrar un hecho económico. La misión **se liquida aunque su kilometraje sea cero**.
3. La conciliación se limita a **fondo entregado vs. consumido vs. devuelto**. La conciliación galonaje↔kilometraje **no aplica** y se marca como tal, no se da por cumplida.
4. La misión queda marcada **no ejecutada**, para que no contamine los indicadores operativos de kilometraje y rendimiento.

**A2 — Sobrante al liquidar** (desde el paso 10) · [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md)

1. Se registra la devolución con acta y su fecha del hecho real.
2. `[C]` **¿El sobrante se devuelve o se arrastra?** Decisión abierta de `PROP-01` — insumo #7. Hasta que se confirme, el sistema modela ambos esquemas.
3. El **sobrante recurrente en la misma ruta no es un problema del motorista: es una estimación mal calibrada.** El sistema produce el reporte de sobrantes recurrentes por ruta y por vehículo, y el estimado se corrige **con vigencia**. *Un sistema que solo mide lo que el servidor le debe a la institución no es un sistema de control: es un sistema de cobro.*

**A3 — Faltante al liquidar** (desde el paso 10) · [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md)

1. El faltante se **tipifica**: diferencia explicada y aceptada con respaldo —que se cierra aquí—, o *sin causa identificada*, *aplicación a fin distinto* o *extravío*.
2. Los tres últimos generan **obligación de reintegro a cargo de persona nominada**, con **ciclo propio que sobrevive al cierre de la misión** y que se salda con asiento reverso sobre el expediente cerrado ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), [`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)).
3. La **determinación de responsabilidad no nace en la liquidación**: es materia del expediente y de quien corresponde ([`RN-74`](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md)).
4. Mientras la obligación esté abierta, esa persona **no recibe nueva asignación de fondo** ([CU-13](CU-13-emitir-y-entregar-asignacion-de-combustible.md) E2).

**A4 — Hubo relevo de motorista o sustitución de vehículo** (desde el paso 6) · [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md), [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md)

1. Kilometraje, combustible y peajes se **imputan por tramo** ([`RN-72`](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)).
2. El **odómetro del acta de traspaso es el corte de imputación** ([`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)).
3. Cada vehículo se concilia **por separado**, con sus propios cortes. Un cálculo agregado mezclaría dos rendimientos y no significaría nada.

**A5 — Liquidación en delegación** (desde el paso 1)

1. `ACT-10` liquida en su ámbito. **La segregación no se relaja**: si la delegación no puede segregar localmente, la función incompatible se ejerce desde la sede — escalamiento, no régimen de excepción ([DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)).
2. Si la bitácora llegó en papel, la liquidación espera la digitación diferida con su original adjunto ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)); pero **el retorno físico constatado ya liberó al vehículo y al motorista** sin esperar ese trámite ([`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md)) — [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md).

**A6 — Misión que cruza el cierre de trimestre o de ejercicio** (desde el paso 13) · [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md)

1. El **compromiso** se imputa al trimestre del **acto que lo generó** —la aprobación del fondo o de la misión—, no al de la fecha de retorno ([`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)). `[C]` confirmar con Gerencia Administrativa: es el tipo de detalle que cada institución resuelve distinto.
2. El **cierre de ejercicio es corte de imputación y de reporte; ningún expediente cambia de estado por una fecha** ([`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)).

## Flujos de excepción

**E1 — Hay divergencias de sincronización sin resolver** (en el paso 3)

1. **`BD-08` bloquea la liquidación.** Bloqueo duro.
2. La resolución es un **acto humano registrado**: qué versión se toma, cuál se descarta, por qué y con qué autoridad. **La versión descartada no se borra** ([`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)).
3. Si la resolución descartó datos capturados en campo, eso **cuenta para el criterio de hallazgo `H-08`**.

**E2 — Falta el ticket de un paso por caseta** (en el paso 7) · [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md)

1. **Advierte, no bloquea** — `PC-14` y [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md): *bloquear el cierre por un ticket faltante hace que el sistema se abandone*.
2. La falta queda registrada, **cuenta para el criterio de hallazgo `H-08`**, y cada discrepancia lleva marcada su **fuerza probatoria** —con ticket fotografiado, con estado de cuenta, o solo declarada— para que el auditor sepa cuánto pesa el expediente sin abrir cada adjunto ([`RN-85`](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md)).

**E3 — El rendimiento es imposiblemente bueno** (en el paso 6) · [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md)

1. **No se "corrige" el estimado para que cuadre**, y no se cierra en silencio. Se tipifica la causa probable y se muestra el desglose que la sustenta: kilómetros por tramo, cargas con su odómetro, tiempo de espera en sitio.
2. Se investiga la hipótesis principal: **abastecimiento no registrado** —fondo agotado, préstamo de otra dependencia, carga de cisterna, peculio del motorista— ([`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)).
3. Si no se justifica, dispara **`H-01`** y la asignación queda `CONCILIADA_CON_DESVIACION` (`V-10`).
4. Las **desviaciones recurrentes** del mismo vehículo, motorista o dependencia generan alerta agregada. **El patrón se ve ahí, no en una misión aislada.**

**E4 — El comprobante ya sostenía otro consumo** (en el paso 5)

1. Detectado al registrarlo, o al sincronizar si se capturó sin red: **bloqueo por unicidad institucional** ([`RN-84`](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md)).
2. El consumo no se elimina: se marca sin comprobante válido y se resuelve como faltante o como descargo alternativo, con hallazgo.

**E5 — Quien intenta liquidar entregó el fondo, despachó, autorizó o condujo** (en el paso 2)

1. **Bloqueo duro. No se guarda nada.** `I-07`, `I-09`, `I-10`, `I-11`. `PC-13`, [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md).
2. El mensaje **nombra el conflicto con precisión** y el intento queda en la pista de auditoría con el par detectado. *Un mismo usuario intentando quince veces liquidar misiones cuyo fondo entregó es exactamente lo que Auditoría Interna quiere ver.*
3. Se genera **tarea de resolución en el puesto competente**: puesto superior, puesto de sede designado como respaldo, o `ACT-08`. La misión no queda trabada; queda visiblemente pendiente en la bandeja de alguien.
4. `I-07` e `I-10` son **núcleo irreductible: no los levanta ningún régimen, ninguna delegación y ninguna resolución de la máxima autoridad.**

**E6 — Vence el plazo de liquidación** (desde el paso 1)

1. Alerta y **escalamiento**, nunca cierre automático ni cuadre automático ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), comportamiento 4).
2. Si el fondo entregado **no fue devuelto ni comprobado** al vencer el plazo, se cumple **`H-04`** y la misión ya no podrá cerrarse limpia.
3. `[C]` El plazo en días hábiles — insumo #32, paquete de parámetros operativos.

**E7 — `ACT-08` devuelve la liquidación (`T-20`)** (desde el paso 14)

1. Motivo obligatorio con las observaciones. La misión vuelve a `RETORNADA`.
2. **La liquidación anterior se conserva íntegra como versión.** Existe porque la alternativa —cerrar mal y corregir por asiento reverso— es más costosa y más confusa.

**E8 — Faltan datos que están en camino** (en el paso 4)

1. El sistema **distingue *ausente* de *pendiente de sincronización*** y no produce hallazgo por falta de datos que aún no llegaron ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), casos límite; [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)).
2. Una misión larga desconectada durante días **es lo normal, no una anomalía**.

**E9 — Se ejecutaron actos sin autorización previa** (en el paso 9) · [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md), [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md)

1. Prórroga sin código de autorización, salida de emergencia, circulación en franja inhábil no cubierta por el salvoconducto.
2. Se **convalida en plazo**, y la **cronología se declara tal como ocurrió** ([`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)). **No se fabrica autorización retroactiva.**
3. Sin convalidación en plazo: **`H-05`** —circulación en día u hora inhábil sin permiso vigente— o eslabón *autorización* ausente y no subsanable ([`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)).

**E10 — Un bloqueo duro falló al revalidarse en el servidor tras sincronizar** (en el paso 4)

1. Licencia vencida durante la misión, motorista no disponible, documentación vencida: el servidor revalida al sincronizar y **no revierte el hecho —el vehículo ya salió—**, sino que **abre hallazgo automático** y notifica a `ACT-04` y `ACT-12`.
2. Se cumple **`H-07`**. La misión ya no podrá cerrarse limpia ([`RN-55`](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md)) — [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md).

## Nota de hallazgo — `HB4-02`: "liquidar" una asignación no es "cuadrarla"

`T-19` exige, como precondición, que **todas las asignaciones de fondo vinculadas estén `LIQUIDADAS`** ([orden-de-mision.md §10.1](../../03-arquitectura/estados/orden-de-mision.md)), y §10.1 define `LIQUIDADA` como *"cuadran asignado, consumido, comprobado y saldo devuelto"*.

Leído literalmente, **una misión cuyo fondo no cuadra nunca puede salir de `RETORNADA`**: el motorista que no devolvió el saldo ni presentó comprobantes deja la asignación en `CONSUMIDA` para siempre, y con ella el expediente entero. Eso contradice frontalmente a [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), que crea una obligación de reintegro **"con ciclo propio que sobrevive al cierre de la misión"** —lo que presupone que la misión **sí puede cerrar con el faltante abierto**— y a la razón de ser de `CERRADA_CON_HALLAZGO`: *un expediente que no puede cerrarse se abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver*.

**Lectura que este caso de uso aplica, y que hay que confirmar en el artefacto autoridad:** *liquidar* una asignación significa **declarar su resultado**, incluido el faltante con su tipificación y su obligación de reintegro nominada — no significa que el resultado sea cero. Con esa lectura, `V-07` procede y `INV-34` se satisface.

**No se resuelve aquí.** [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) es la autoridad en invariantes y precondiciones. Queda dirigido a esa especificación para que precise la redacción de `LIQUIDADA` en §10.1, o para que declare expresamente que el faltante bloquea la liquidación de la misión — que sería la decisión contraria y también hay que escribirla.

## Reglas aplicables

| Regla | Qué aporta a este caso |
|---|---|
| [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) | **Regla eje.** Conciliación galonaje↔kilometraje con **umbrales independientes en ambas direcciones** |
| [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) | Asignado vs. consumido vs. comprobado vs. devuelto; toda diferencia explicada |
| [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) | Lista de verificación de la cadena, eslabón por eslabón, con *no aplicable* fundamentado |
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | `I-07`, `I-09`, `I-10`, `I-11` sobre esta misión, por identidad de persona |
| [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) | Saldo afuera, obligación de reintegro con ciclo propio, arqueo por persona |
| [`RN-85`](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) · [`RN-84`](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md) | Ausencia con causa y fuerza probatoria; unicidad institucional del comprobante |
| [`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) · [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) · [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) | Discrepancia de clasificación, coherencia de la secuencia de casetas, expediente de reclamo |
| [`RN-72`](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md) · [`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) | Imputación por tramo con el odómetro del acta como corte |
| [`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) · [`RN-77`](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md) | Grado de cumplimiento del objeto; alcance vigente a la fecha del hecho |
| [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) · [`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) | Cálculo con el paquete congelado; resultado económico congelado; corrección con asiento de diferencia |
| [`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) · [`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) | Imputación del compromiso al trimestre del acto; cierre de ejercicio como corte |
| [`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) · [`RN-55`](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) | Convalidación con cronología real; habilitación vencida en ruta cierra con hallazgo |
| [`RN-81`](../../01-negocio/reglas/RN-81-sigti-expone-hechos-a-argos.md) | Los hechos se exponen a ARGOS por la clave de vinculación; SIGTI no escribe en el origen |

## Trazabilidad

- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) **E12 liquidación: el descargo conciliado**, E13 conciliación · puntos de control **`PC-13`** (segregación de quien liquida y quien cierra), **`PC-14`** (ticket faltante advierte), `PC-11` (coherencia del odómetro), `PC-18`
- **Autoridad en transiciones:** [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — **`T-19`**, `T-16`, `T-18`, `T-20`; `BD-06`, **`BD-08`**; **`EF-05` conciliación**, `EF-03`; `INV-34` a `INV-38`; §7.2 criterios `H-01` a `H-08`; **§10.1 `V-07`, `V-08`, `V-09`, `V-10`**
- **Autoridad en actores e incompatibilidades:** [actores-y-roles.md](../../01-negocio/actores-y-roles.md) — matriz fila 13 con notas 7 y 8; §5.2 pares `I-07`, `I-09`, `I-10`, `I-11`, `I-14`
- **Casos especiales:** [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md), [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md), [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md), [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md), [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md), [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md), [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md), [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md), [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md), [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md)
- **Casos de uso encadenados:** ← [CU-14](CU-14-registrar-consumo-de-combustible-y-peaje.md) · → [CU-16](CU-16-cerrar-el-expediente-de-la-mision.md)
- **Requisitos no funcionales:** [RNF-06](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md), [RNF-05](../no-funcionales/RNF-05-temporalidad-normativa.md), [RNF-18](../no-funcionales/RNF-18-paquetes-de-evidencia-para-auditoria.md)
- **Normativa:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — `[V]` que el auditor busca **correlación entre consumo, kilometraje y misión autorizada**, no comprobantes; `[P]` segregación de funciones y control de fondos entregados a servidores · [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) — conciliación estimado vs. pagado; el ticket faltante advierte y no bloquea · [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) `[V]` cuotas trimestrales de compromiso, `[I]` la validación desde SIGTI · [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- **Decisiones:** [DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (**viáticos fuera de alcance**), [DP-001 D-02](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md), [DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md), [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)
- **Historias:** pendientes — no escritas en este bloque
- **Insumos pendientes:** #1 y #19 (umbrales de desviación de combustible, kilometraje y peaje; criterio sobre comprobante ilegible) · #32 (plazo de liquidación en días hábiles) · #7 / `PROP-01` (sobrante: se devuelve o se arrastra; plazo de devolución del saldo) · #37 (reintegro de peculio propio) · #48 (límite de jornada de conducción, para la imputación por tramo)
