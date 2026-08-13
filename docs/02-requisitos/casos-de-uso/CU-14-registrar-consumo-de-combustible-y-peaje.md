# CU-14 — Registrar consumo de combustible y paso por peaje

| Campo | Valor |
|---|---|
| **Módulos** | M-09 Combustible · M-18 Peajes · M-08 Ejecución y Bitácora · M-16 Sincronización y Operación Desconectada |
| **Actor principal** | `ACT-06` Motorista |
| **Actores secundarios** | `ACT-10` Encargado de Delegación — digitación diferida · `ACT-04` Jefe de Transporte — cola de conflictos y seguimiento · `ACT-07` Encargado de Combustible — recibe comprobantes y remanente al retorno |
| **Precondiciones** | La misión está en **`EN_RUTA`** tras `T-14`. La bitácora está abierta y admite eventos (`INV-26`). Hay **un dispositivo portador designado** (`INV-22`) con el **paquete de misión y el paquete normativo congelado** a bordo (`EF-03`): tarifas por punto y categoría, categoría de peaje del vehículo, rendimiento esperado, umbrales, estaciones y puntos de peaje de la ruta. El motorista lleva la Orden de Misión impresa |
| **Postcondiciones** | Cada abastecimiento y cada paso por caseta quedan registrados como **eventos de bitácora** con su fecha del hecho, su fecha de captura, su odómetro y su evidencia gráfica. La asignación de fondo pasa a **`CONSUMIDA`** (`V-04`), total o parcialmente. Nada de esto cambia el estado de la Orden de Misión |
| **Disparador** | El motorista carga combustible, o cruza un punto de peaje, o incurre en un gasto operativo en ruta |

> **Esto ocurre en carretera y sin red.** El escenario de diseño es un teléfono con señal intermitente o nula, batería limitada, a plena luz del sol, y un servidor que puede no saber nada durante días ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]`; `EF-07`; `INV-27`). **Todo lo que le exija al motorista más de un minuto o más de tres toques por registro se llenará en papel y se digitará después, mal.** El silencio del servidor no es una anomalía: es lo que el diseño espera.

> **Ninguno de estos registros ejecuta una transición `T-nn`.** Son eventos de la bitácora abierta y transiciones de la máquina secundaria de la asignación de fondo (`V-04`). El estado de la Orden sigue siendo `EN_RUTA` hasta `T-18`.

## Flujo principal

### A. Registrar un abastecimiento de combustible (`V-04`)

1. `ACT-06` está en la estación. Abre *registrar abastecimiento* en el dispositivo. **La pantalla funciona sin ninguna conectividad** ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)).
2. Captura, con el formulario precargado con la misión, el vehículo y su último odómetro conocido:

   | Dato | Por qué está |
   |---|---|
   | **Galones** y **monto** | Numerador y costo de la conciliación |
   | **Estación** — del catálogo o texto libre si no está | Cruce contra ruta autorizada y contra facturación del proveedor |
   | **Odómetro al momento de cargar** | Es el ancla. Sin él el galón no se puede correlacionar con nada |
   | **Instrumento y medio de pago** — folio del vale, efectivo, orden de pago, tarjeta | De él depende qué evidencia existe |
   | **Fotografía del comprobante** | [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) |
   | **Fuente del combustible** | [`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) — ver paso 3 |

3. `ACT-06` declara la **fuente**: fondo de esta misión, otro fondo, cisterna institucional, otra dependencia, donación o **peculio del servidor**. *Todo ingreso de combustible al tanque se registra como abastecimiento con su fuente declarada, aunque no exista folio de este fondo* ([`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)). Es lo que le cierra la puerta al **préstamo invisible** — [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md).
4. El dispositivo valida la **coherencia del odómetro** contra la última lectura que trae en su paquete (`BD-05`). Retroceso → bloqueo de captura, que aquí **es corregir, no ocultar**. Salto grande → **no bloquea**: justificación obligatoria y marca para revisión (principio `P-2`).
5. El sistema registra `ocurrido_en`, `capturado_en` y el **modo de captura**, y numera el evento con la **secuencia monotónica del dispositivo** — no con el reloj ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [orden-de-mision.md §6.4](../../03-arquitectura/estados/orden-de-mision.md)).
6. La asignación de fondo vinculada pasa a **`CONSUMIDA`** (`V-04`). Puede ser consumo parcial: no se exige agotarla.
7. El **nivel del tanque a la salida y al retorno** ya es dato obligatorio de la bitácora ([`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)); sin él, el vehículo que sale lleno y retorna vacío produce una conciliación que no significa nada.

### B. Registrar un paso por punto de peaje

8. `ACT-06` se acerca a la caseta. El formulario **precarga desde el paquete congelado**: punto de peaje, **categoría asignada al vehículo con su fundamento**, y **tarifa esperada** a la fecha del paso ([`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), `EF-03`). Los mismos datos están impresos en la Orden de Misión que lleva en la mano ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)).
9. `ACT-06` consigna **categoría cobrada**, **monto pagado**, **medio de pago**, y **fotografía del ticket**.
10. Si categoría o monto cobrados difieren de lo esperado, el sistema **marca la discrepancia de clasificación por sí solo** ([`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md)). No depende de que alguien la note al liquidar, que es cuando ya no se puede evitar.
11. **La discrepancia nunca modifica la categoría del vehículo.** El cobro es un hecho a registrar; la clasificación es una derivación de la ficha técnica y de la norma. Un sistema que "aprende" del cobro de la caseta convierte el error de la caseta en la verdad institucional, y en tres meses el reclamo ya no ocurre nunca — [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md).
12. **El sobrecosto no se le imputa al motorista.** Se registra tipificado como *sobrecosto por clasificación*. Si el motorista teme que le descuenten la diferencia, no va a declararla: va a acomodar la suma para que cuadre. La no imputabilidad es la condición para que el dato exista.
13. **El cobro en categoría inferior también se registra.** Callarlo expone a la institución a un cobro retroactivo y contradice el registro fiel.

### C. Sincronizar

14. Cuando hay señal, el dispositivo envía **el diario completo de transiciones y eventos**, no el estado. El servidor aplica **en orden de secuencia del dispositivo**, descarta duplicados por identificador, retiene los que esperan predecesor, y **mide y registra el desfase del reloj del dispositivo** ([orden-de-mision.md §6.2 y §6.3](../../03-arquitectura/estados/orden-de-mision.md)).
15. **Ningún conflicto se resuelve por sobrescritura**: todo va a cola de resolución humana para `ACT-04` ([`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)). Lo que no se aplica, se conserva y se muestra.

## Flujos alternos

**A1 — El dispositivo falla, o no lo hay: se registra en papel** (desde el paso 1) · [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)

1. `ACT-06` llena la **hoja de bitácora impresa** que salió con folio y QR desde el despacho, con paridad exacta con la pantalla de digitación ([`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)).
2. `ACT-10` digita después con **fecha del hecho tomada del papel** y **fecha de captura del momento de la digitación**, constancia de quién digitó y **adjunto del original escaneado o fotografiado** ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md), modo de captura *digitación diferida de papel*).
3. La diferencia entre ambas fechas **es visible en el expediente, no se disimula**.
4. *El papel no es un fracaso del sistema: es parte del diseño.*

**A2 — Pago de peaje con tag prepago** (desde el paso 9) · [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) punto 9

1. La categoría la aplica el sistema del tag y **no hay ticket**: la evidencia es el **estado de cuenta**.
2. La conciliación es mensual contra ese estado ([`RN-95`](../../01-negocio/reglas/RN-95-conciliacion-contra-fuentes-externas.md)), con **fecha del hecho igual a la del paso** y fecha de captura posterior ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
3. Una discrepancia detectada ahí **puede alimentar el reclamo aunque la Orden ya esté cerrada**: anexar evidencia a un expediente cerrado está permitido; **modificarlo no** ([`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md), [`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)).

**A3 — Reclasificación legítima: el vehículo lleva remolque** (en el paso 10)

1. La **configuración del vehículo para la misión** se declara al programar y el estimado la usa. Si va declarada, el cobro superior **no es discrepancia** ([`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md)).
2. Sin este campo, cada misión con remolque produce una discrepancia falsa, y **tres falsas seguidas hacen que nadie vuelva a mirar las verdaderas**.

**A4 — Punto de peaje con exoneración registrada** (en el paso 8)

1. La exoneración es dato **por vehículo, punto, fundamento y vigencia**, nunca una constante ([`RN-38`](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md)).
2. El paso se registra igual, con monto cero y el fundamento de la exoneración. Un paso no registrado rompe la secuencia de casetas que después se concilia.

**A5 — Gasto imprevisto en ruta distinto del combustible** (desde el paso 1)

1. Llanta, grúa, reparación menor, lavado obligatorio para ingresar a una instalación: se registra con **tipo, factura y autorización del acto** ([`RN-87`](../../01-negocio/reglas/RN-87-gasto-imprevisto-en-ruta.md)).
2. **No se disfraza de combustible** para que cuadre el vale. Ese es el atajo que destruye la conciliación de [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md).
3. Si la avería impide continuar, se abre además el evento de interrupción en ruta con desenlace obligatorio ([`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)) — [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md).

**A6 — Espera prolongada en sitio con motor encendido** (en cualquier momento) · [CE-08](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md)

1. El motorista **declara su propio estado**: se movió, llegó, quedó en espera. Nunca se infiere ([`RN-76`](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md), [DP-001 D-06](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).
2. El **motor encendido durante la espera se registra con un toque** y entra como variable del cálculo de rendimiento: una desviación con espera prolongada registrada **no produce hallazgo por sí sola** ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)). Sin esa medición, el hallazgo sería infundado.

**A7 — Prórroga o destino adicional en ruta (`T-17`)** (en cualquier momento) · [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md)

1. Si se agregan destinos, el estimado de peajes se recalcula **con el paquete congelado**, no con la tabla actual del servidor.
2. La extensión produce una **versión del alcance autorizado**, y toda validación posterior usa la vigente a la fecha de cada hecho ([`RN-77`](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md)).
3. `T-17` **revalida `BD-02` contra la nueva fecha de fin**. Si la licencia vence dentro de la ventana ampliada, la prórroga **se bloquea**: la salida es el relevo o el retorno anticipado.
4. Sin señal se usa el **código de autorización fuera de línea**. Si no hay forma de obtenerlo, el motorista registra el hecho con justificación obligatoria y la falta de autorización previa **se resuelve en la liquidación** ([`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)). *No se puede exigir en carretera lo que solo se puede firmar en la oficina, pero tampoco se puede fingir que existió.*

## Flujos de excepción

**E1 — El odómetro no es coherente** (en el paso 4) · [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md)

1. Retroceso respecto a la última lectura conocida → **bloqueo duro de captura** (`BD-05`, [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md)). Es error de digitación o retroceso del instrumento.
2. **La única salida** es que exista un **acta previa de intervención del instrumento** —sustitución o reinicio— registrada por `ACT-11` con la lectura del retirado y la del instalado, con orden de trabajo y autorización nominativa ([`RN-90`](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md)). Entonces el kilometraje se calcula sumando tramos ([`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md)) y el bloqueo no aplica.
3. **No es un permiso para saltarse la validación: es un hecho mecánico que hay que poder registrar.**
4. Con el odómetro averiado, el abastecimiento **se registra igual** y el cálculo de rendimiento de esa misión se marca **no concluyente** ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)). `[C]` plazo máximo de operación con odómetro averiado.

**E2 — La estación no emite factura, o el comprobante se perdió** (en el paso 2) · [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md)

1. **El consumo se registra igual.** Lo que se captura es la **causa tipificada** de la ausencia y la **suficiencia probatoria** de lo que sí hay: fotografía del surtidor, del odómetro, ubicación, hora.
2. Se admite **descargo alternativo con folio** cuando la institución lo permita ([`RN-85`](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md)).
3. `[C]` Si la institución admite constancia como descargo, con qué tope y qué umbral de hallazgo — insumo #1, con Auditoría Interna.

**E3 — El comprobante ya fue registrado en otra misión** (en el paso 2)

1. Todo comprobante es **único en la institución por emisor y número**, y su reutilización **se bloquea al registrarlo** ([`RN-84`](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md)).
2. Sin conectividad el dispositivo no puede verificar la unicidad institucional: el duplicado se detecta **al sincronizar**, abre conflicto para `ACT-04` y alimenta el criterio de hallazgo.
3. *El control barato se ejecuta al registrar; el caro, ocho meses después conciliando a mano.*

**E4 — El fondo se agotó en ruta y el motorista paga de su peculio** (en el paso 3) · [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md)

1. Se registra como **abastecimiento con fuente declarada *peculio del servidor***, con su comprobante ([`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)).
2. Genera **obligación de reintegro a favor del servidor**, con instrumento distinto del fondo y **sin afectar su cuadre** ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).
3. Si no se modela, **el galón que el motorista pagó de su bolsillo desaparece del denominador** de [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), y la misión aparece con un rendimiento imposiblemente bueno que no significa lo que parece.
4. `[C]` Si la institución reintegra combustible pagado de peculio propio y bajo qué circuito — insumo #37. **La práctica ocurre con o sin regla.**

**E5 — Combustible prestado por otra dependencia o cargado de la cisterna institucional** (en el paso 3)

1. Se registra como abastecimiento con esa fuente. **No hay folio de este fondo y aun así el hecho existe.**
2. Es la causa del síntoma que [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) detecta como *rendimiento imposiblemente bueno*: el vehículo cargó combustible que nadie anotó.

**E6 — El motorista no sabe con qué categoría le cobraron** (en el paso 9)

1. Es lo más común: el ticket puede no indicarla.
2. Se registra el **monto pagado** y el sistema **deriva la categoría probable** contra la tabla del punto y la fecha, marcándola **inferida, no declarada** ([`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md)).

**E7 — Secuencia de casetas geográfica o temporalmente imposible** (en el paso 14)

1. **No se bloquea el registro** (`P-2`). El vehículo ya pasó por donde pasó.
2. La misión queda marcada y la incoherencia se resuelve en la conciliación, con hallazgo `H-03` si no se justifica ([`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)). *Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y el sistema debe producirlo solo.*
3. La evaluación se hace contra el **alcance vigente a la fecha del hecho**: un reordenamiento de destinos justificado y autorizado no es desviación ([`RN-77`](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md)).

**E8 — Dos dispositivos registran hechos de la misma misión** (en el paso 14)

1. Ocurre: el teléfono del motorista se dañó y el Encargado de Delegación registró desde el suyo.
2. Se aplica automáticamente la cadena del **dispositivo portador**; la otra se registra marcada como *de dispositivo no portador*. Si hay conflicto, se aplica la **primera cadena recibida**, la segunda **se conserva íntegra** como cadena divergente, y se abre conflicto para `ACT-04` con ambas versiones lado a lado, campo por campo.
3. La misión queda **"con divergencia pendiente"** y **`BD-08` impide liquidarla** hasta que una persona resuelva, con constancia de qué versión se toma y por qué. **La versión descartada no se borra.**

**E9 — La tarifa cargada en el sistema no es la efectivamente vigente**

1. Riesgo declarado en [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md): hay contradicción abierta entre el comunicado de la SIT del 28/02/2026 —*no habrá incremento para ninguna categoría*, `[V]`— y lo que publica un agregador comercial.
2. **Un detector de discrepancias montado sobre una tabla no verificada es peor que no tener detector**: marcaría cada cruce del país como cobro indebido y el primer reclamo institucional se caería solo.
3. Mientras la tarifa de un punto esté marcada como **no verificada**, la detección de discrepancia sobre ese punto se presenta como **no concluyente**. `[C]` tarifa efectivamente vigente — insumo #21; [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §4, §9 y §10.

## Reglas aplicables

| Regla | Qué aporta a este caso |
|---|---|
| [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) | **Regla eje del consumo.** Galones, monto, estación, odómetro y fotografía del comprobante |
| [`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) | Todo ingreso al tanque es abastecimiento con **fuente declarada**; nivel de tanque como dato de bitácora |
| [`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) | **Regla eje del peaje.** La discrepancia se marca y **nunca** modifica la categoría del vehículo |
| [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) | Categoría y tarifa esperada en la mano del motorista, en la caseta |
| [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) · [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) · [`RN-38`](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) | Categoría derivada de la ficha técnica; tarifa por punto × categoría × vigencia; exoneración con fundamento |
| [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) · [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) · [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) | Captura sin red, cero sobrescritura, doble fecha, digitación diferida con original adjunto |
| [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) · [`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) · [`RN-90`](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md) | Coherencia del odómetro y tratamiento de la intervención del instrumento |
| [`RN-84`](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md) · [`RN-85`](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) | Unicidad institucional del comprobante; ausencia con causa y descargo alternativo |
| [`RN-87`](../../01-negocio/reglas/RN-87-gasto-imprevisto-en-ruta.md) | Gasto imprevisto con tipo, factura y autorización del acto |
| [`RN-76`](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md) · [`RN-77`](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md) · [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) | Estado declarado, versionado del alcance, interrupción con desenlace |
| [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) | Cálculo con el paquete normativo congelado, a la fecha del hecho |

## Trazabilidad

- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) **E9 ejecución y bitácora**, E10 seguimiento en ruta · puntos de control **`PC-11`** (coherencia del odómetro), `PC-14` (falta de ticket advierte, no bloquea)
- **Autoridad en transiciones:** [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — estado `EN_RUTA` (`INV-24` a `INV-28`), `T-14`, `T-17`, `T-18`; `BD-05`; `EF-03`, `EF-07`; **§6 operación desconectada**; **§10.1 `V-04`**
- **Autoridad en actores:** [actores-y-roles.md](../../01-negocio/actores-y-roles.md) — `ACT-06` (dispositivo, frecuencia, límite duro), matriz filas 11 y 12
- **Casos especiales:** [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) y [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) (ejes), [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md), [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md), [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md), [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md), [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md), [CE-08](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md)
- **Casos de uso encadenados:** ← [CU-13](CU-13-emitir-y-entregar-asignacion-de-combustible.md) · → [CU-15](CU-15-liquidar-la-mision-y-conciliar.md)
- **Requisitos no funcionales:** [RNF-03](../no-funcionales/RNF-03-operacion-sin-conectividad.md), [RNF-12](../no-funcionales/RNF-12-uso-en-campo.md), [RNF-05](../no-funcionales/RNF-05-temporalidad-normativa.md), [RNF-11](../no-funcionales/RNF-11-formatos-oficiales-imprimibles-y-verificables.md)
- **Normativa:** [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` más de 2 millones de personas del área rural sin acceso a internet; registro en papel y digitación diferida · [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) `[V]` que la SAPP resolvió sobre clasificación de vehículos livianos el 17/09/2025; **el articulado del Art. 51 de la Ley de Tránsito es `[C]`** — el PDF oficial es un escaneo sin capa de texto · [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) `[P]` registro oportuno, fecha del hecho distinta de la de captura
- **Decisiones:** [DP-001 D-06](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (estado en ruta declarado por el motorista), [DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (estado del vehículo desde campo), [DP-001 D-02](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (peajes dentro de alcance)
- **Historias:** pendientes — no escritas en este bloque
- **Insumos pendientes:** #21 (tarifa de peaje efectivamente vigente — **condiciona toda la detección de discrepancias**) · #22 (lista oficial de exoneraciones) · #23 (Art. 51 y catálogo de restricciones de la DNVT) · #24 (tags prepago y estado de cuenta) · #37 (reintegro de peculio propio) · #2 (formatos en papel de bitácora y de vale) · #1 (¿se admite constancia como descargo? plazo con odómetro averiado)
