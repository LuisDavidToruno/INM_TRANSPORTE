# CU-08 — Registrar la ejecución en ruta sin conectividad

| Campo | Valor |
|---|---|
| **Módulos** | M-08 Ejecución y Bitácora · M-16 Sincronización y Operación Desconectada · M-19 Seguimiento en Ruta |
| **Actor principal** | `ACT-06` Motorista, en el dispositivo portador |
| **Actores secundarios** | `ACT-04` Jefe de Transporte (autoriza prórroga cuando se le puede alcanzar) · `ACT-10` Encargado de Delegación · `ACT-15` Verificador en Carretera (lee el papel, no el sistema) · `ACT-09` Máxima Autoridad (permiso de día inhábil sobrevenido) |
| **Precondiciones** | La misión está `EN_RUTA` (`INV-24` a `INV-28`). El dispositivo portador recibió en `T-12` el paquete de misión: expediente, documentos, **paquete normativo congelado** (`EF-03`), puntos de peaje de la ruta con su categoría y tarifa esperada, estaciones, catálogo de tipificaciones de evento y guía de actuación en accidente. El motorista autenticado es el titular o un relevo declarado. |
| **Postcondiciones** | La bitácora local contiene todos los hechos ocurridos, cada uno con identificador generado en el cliente, número de secuencia monotónica del dispositivo, `ocurrido_en`, `capturado_en`, actor, rol ejercido y hash encadenado con el evento anterior. Ningún registro se perdió. Todos están en estado de sincronización `PENDIENTE_DE_ENVIO` hasta que haya red. |
| **Disparador** | El vehículo sale del predio y cada hecho de la operación ocurre: una parada, un arribo, una espera, una carga de combustible, un paso por caseta, una entrega, una novedad. |

---

## Por qué este caso decide el proyecto

**Todo lo que aquí se describe ocurre sin red.** Más de 2 millones de personas del área rural hondureña no tienen acceso a internet ([`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md), INE EPHPM julio 2025 `[V]`). El cliente debe sostener **7 días continuos** de captura completa sin conectividad, ≥ 20 órdenes de misión y ≥ 200 fotografías almacenadas localmente, con **cero pérdida de registros** ([`RNF-03`](../no-funcionales/RNF-03-operacion-sin-conectividad.md)).

Y ocurre en condiciones concretas: a pleno sol, con guantes, en un celular de gama baja, con la batería contada ([`RNF-12`](../no-funcionales/RNF-12-uso-en-campo.md)). **Todo lo que exija a `ACT-06` más de un minuto o más de tres toques por registro se llenará en papel y se digitará después, mal** ([`PR-01`, E9](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)). Si este caso se resuelve mal, el motorista vuelve al papel y el resto del sistema da igual ([`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)).

**El servidor puede no saber absolutamente nada durante días, y eso es lo que el diseño espera.** El silencio no es un estado: no dispara ninguna transición automática y no permite inferir nada.

---

## Flujo principal

1. `ACT-06` abre la misión en el dispositivo. **No se requiere conectividad ni para autenticarse ni para operar**: el paquete de misión ya trae lo necesario.
2. El sistema muestra la pantalla de misión: destinos en orden previsto, ventana autorizada, odómetro de salida ya registrado, y las acciones disponibles como botones grandes de un toque — *llegué*, *salgo*, *cargué combustible*, *pasé caseta*, *entregué*, *novedad*.
3. **Arribo a destino.** `ACT-06` toca *llegué*. El sistema registra el evento con la hora del reloj del dispositivo como `ocurrido_en`, pide odómetro y nada más. El estado en ruta lo **declara el conductor desde un catálogo cerrado**; el sistema nunca lo infiere de la ausencia de movimiento ni de la ausencia de señal ([`RN-76`](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md)).
4. **Espera en sitio.** El tiempo en sitio **se deriva** de los eventos de arribo y salida por destino. Nunca se pide al conductor que lo cronometre ni que lo digite. Si la espera impide operar, se tipifica por causa y se atribuye al destino y a la dependencia responsable: solo esa cuenta como **espera improductiva** en los indicadores ([`CE-08`](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md)).
5. **Entrega o recepción de carga o de personas.** Se registra con acta, inventario y firma de quien recibe, capturada en la pantalla. Toda diferencia contra el inventario declarado se registra como **faltante**, no se ajusta el inventario ([`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md)). Los cambios en el manifiesto de personas externas se registran como **novedad, no como edición** ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)).
6. **Paso por caseta de peaje.** El sistema presenta el punto que corresponde según la ruta del paquete, con **la categoría asignada al vehículo y la tarifa esperada** ya en pantalla —las mismas que van impresas en la Orden que el motorista lleva en la mano ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)). `ACT-06` confirma o corrige monto, indica medio de pago y toma **foto del ticket**.
7. **Carga de combustible.** Se registra galones, monto, estación, **odómetro al momento de cargar** y fotografía del comprobante ([`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)). Todo ingreso de combustible es un abastecimiento con **fuente declarada** —fondo de la misión, tanque institucional, peculio propio, donación en sitio—; el nivel de tanque es dato de bitácora, no sustituto del abastecimiento ([`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)).
8. **Gasto imprevisto distinto de combustible** —una llanta, un lavado obligatorio en un retén, un parqueo—: se registra con tipo, factura y la autorización del acto si la hubo ([`RN-87`](../../01-negocio/reglas/RN-87-gasto-imprevisto-en-ruta.md)).
9. Cada evento se guarda localmente con: identificador **generado en el cliente** (UUID), número de **secuencia monotónica del dispositivo** —que es lo que define el orden, no el reloj—, `ocurrido_en`, `capturado_en` no editable, actor, rol ejercido, dispositivo, modo de captura `desconectada sincronizada`, y **hash encadenado con el evento anterior del mismo dispositivo** ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [`RNF-04`](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md)).
10. El dispositivo muestra en todo momento **cuántos registros están pendientes de enviar** y desde cuándo. Nunca presenta como sincronizado algo que no lo está.
11. Cuando aparece señal, la sincronización arranca sola y en segundo plano, sin interrumpir la captura. Su resultado se trata en [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md).
12. El seguimiento en ruta (M-19) se alimenta **oportunistamente**: cuando hay señal, `ACT-04` ve dónde está el vehículo; cuando no la hay, el tablero muestra la **última posición conocida con su antigüedad**, nunca una posición vieja presentada como actual ([`RNF-08`](../no-funcionales/RNF-08-seguimiento-en-ruta.md)). **Su ausencia no es un estado ni una anomalía.**
13. La ejecución termina con el registro del retorno, en [`CU-10`](CU-10-registrar-retorno-y-cerrar-bitacora.md).

---

## Flujos alternos

**A1 — La misión se extiende: más días, más destinos o más kilómetros** (desde el paso 3)
1. `ACT-06` intenta registrar un arribo a un destino no previsto, o la ventana autorizada está por vencer.
2. Corresponde `T-17`. Si hay forma de alcanzar al autorizador —radio, teléfono, un punto con señal—, `ACT-04` genera un **código de autorización fuera de línea** para esa misión y esa transición, con ventana de validez corta y un solo uso. El dispositivo lo **verifica sin conectividad** (§6.6).
3. En prórroga, el dispositivo **revalida `BD-02` y `BD-03` contra la nueva fecha de fin, usando el paquete normativo congelado**. Si la licencia del motorista vence dentro de la ventana ampliada, **la prórroga se bloquea**: la salida es el relevo o el retorno anticipado. `BD-02` no tiene excepción configurable y prorrogar no puede ser la puerta trasera que lo evita.
4. Cada extensión produce una **versión del alcance autorizado**, y toda validación posterior usa la vigente a la fecha del hecho ([`RN-77`](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md)). El expediente muestra la ventana original y la ampliada; no se sobrescribe.
5. Si se agregan destinos, el estimado de peajes se recalcula **con el paquete congelado**, no con la tabla actual del servidor.
6. Ver [`CE-06`](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md).

**A2 — No hay forma de obtener el código de autorización** (desde A1, paso 2)
1. Sin señal, sin radio y sin teléfono, no se puede exigir una autorización que físicamente no se puede pedir — pero tampoco se puede fingir que existió.
2. `ACT-06` registra el hecho como **evento en ruta con justificación obligatoria**, y la falta de autorización previa **se resuelve en la liquidación**, con hallazgo si la institución así lo tipifica ([`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).
3. Si la extensión hace que la misión toque día u hora inhábil no cubierto por el salvoconducto vigente, la circulación queda **fuera del amparo del permiso**: se registra con justificación, la extensión no autorizada queda marcada, y se resuelve en la liquidación con hallazgo `H-05` si no se justifica.

**A3 — En la caseta cobran una categoría distinta a la asignada** (desde el paso 6)
1. `ACT-06` compara lo cobrado contra la tarifa esperada impresa en la Orden y marca **discrepancia de clasificación** con un toque, indicando la categoría que le cobraron.
2. **Conserva el ticket y lo fotografía.** El ticket es la única prueba del reclamo.
3. La discrepancia habilita el **reclamo ante la SAPP**, que es un objeto con estado y resultado económico propio: las discrepancias no cierran sin él ([`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md), [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md), [`CE-24`](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md)).

**A4 — La estación no da factura, o el comprobante se perdió o se borró** (desde el paso 7)
1. El consumo **se registra igual**: no registrar el galón para evitar la falta del papel es peor que la falta del papel.
2. Se declara **causa tipificada** de la ausencia y se aporta lo que exista —foto de la bomba, del surtidor, del odómetro, recibo manuscrito—; el sistema distingue *pendiente* de *ausente* ([`RN-85`](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md), [`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md)).
3. La falta de un ticket de caseta **advierte pero no bloquea** el cierre (`PC-14`, [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md)): bloquear por eso hace que el sistema se abandone.

**A5 — Paso por punto de peaje que no venía en el paquete** (desde el paso 6)
1. El motorista lo registra como **punto no previsto**, con nombre tal como aparece en el ticket, monto, hora y foto.
2. La resolución del punto contra el catálogo y su tarifa se hace al sincronizar, con la tabla **vigente a la fecha del hecho** y el paquete congelado ([`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)).
3. Un peaje incompatible con la ruta autorizada, o una secuencia de casetas geográfica o temporalmente imposible, produce hallazgo `H-03` al conciliar ([`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)) — pero **el registro nunca se impide**.

**A6 — Aparece señal a mitad de la ruta** (desde el paso 11)
1. La sincronización es **oportunista y parcial**: sube lo que alcance, en orden de secuencia, y sigue capturando mientras tanto.
2. Un corte a mitad de sincronización no duplica nada al reintentar: el identificador del cliente es la llave de idempotencia.
3. Lo que el servidor devuelva como conflicto **no altera lo capturado en el dispositivo**: la autoridad del expediente reside en el dispositivo portador mientras la misión está `EN_RUTA` (`INV-27`).

**A7 — Relevo de motorista en ruta** (desde el paso 1)
1. El motorista entrante se autentica en el mismo dispositivo portador; el acta de traspaso con odómetro es el **corte de imputación** entre tramos ([`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)). Ver [`CU-07`](CU-07-sustituir-vehiculo-o-motorista.md) A5.
2. Desde el corte, todo evento se imputa al tramo del motorista entrante ([`RN-72`](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)).
3. Si ambos registran el mismo paso por caseta desde dispositivos distintos, ambos registros son válidos y describen el mismo hecho: se detecta como **posible duplicado por punto y ventana temporal** y lo resuelve una persona en [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md).

---

## Flujos de excepción

**E1 — Ocurre un hecho que impide continuar la misión** (en cualquier paso)
1. Avería, accidente, sustracción, incapacidad del conductor, vía cerrada, condición de seguridad: el camino es [`CU-09`](CU-09-interrupcion-en-ruta-y-desenlace.md).
2. Ante causas con personas involucradas, el cliente **muestra la guía de actuación antes de cualquier formulario**, y el registro mínimo se puede diferir sin perderse. Primero se atiende; después se captura ([`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)).

**E2 — El odómetro capturado es menor que la última lectura conocida** (en los pasos 3, 6, 7)
1. Es error de digitación o retroceso del instrumento. `BD-05` **bloquea la captura de ese valor** y lo dice en lenguaje del negocio: "el último kilometraje registrado de este vehículo es 93,061; el que está ingresando es menor".
2. Aquí bloquear **es corregir, no ocultar**: es la única excepción al principio `P-2`, y solo aplica al error material.
3. La única salida es que exista **acta previa de sustitución o reinicio de odómetro** registrada por `ACT-11` antes de la salida, con la lectura del instrumento retirado y del instalado. Entonces el kilometraje acumulado se calcula sumando tramos ([`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md), [`RN-90`](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md), [`CE-22`](../casos-especiales/CE-22-odometro-inconsistente.md)).
4. Si el odómetro se avería **durante** la misión, se declara el hecho como evento, se registra la última lectura válida y los tramos siguientes se estiman **declarados como estimados**, nunca presentados como leídos.

**E3 — El kilometraje recorrido se dispara respecto a la ruta autorizada** (en el paso 3)
1. **No bloquea.** Se exige justificación y se marca la misión para revisión; deriva en `H-02` al conciliar.
2. La desviación se vigila **en ambas direcciones**: recorrer mucho menos de lo estimado es tan señalable como recorrer mucho más ([`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)).

**E4 — El almacenamiento del dispositivo se llena** (en el paso 9)
1. Las fotografías se comprimen automáticamente y el cliente avisa con anticipación del techo ([`RNF-12`](../no-funcionales/RNF-12-uso-en-campo.md), [`RNF-03`](../no-funcionales/RNF-03-operacion-sin-conectividad.md)).
2. **Ningún registro se descarta jamás.** Si no cabe una fotografía, se conserva el evento y la fotografía queda declarada **pendiente**, que no es lo mismo que **ausente** ([`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md)).
3. El evento sin su adjunto sincroniza igual; el adjunto sube después, vinculado por el identificador del evento.

**E5 — El reloj del dispositivo está corrido** (en el paso 9)
1. El **orden de los hechos lo define la secuencia monotónica**, no el reloj: un reloj puede retroceder, la secuencia no.
2. Si `ocurrido_en` resulta mayor que `capturado_en` fuera de tolerancia, el dato es incoherente y va a la **cola de conflictos**; no se corrige solo.
3. El servidor mide y registra el desfase del reloj del dispositivo en cada sincronización. Ese desfase queda en el expediente: **permite corregir el análisis sin corregir el dato**.

**E6 — Se acaba la batería, el dispositivo se daña o se moja** (en cualquier paso)
1. La captura continúa en la **hoja de bitácora impresa con folio y QR** que se emitió en `T-12`, cuyas casillas están en el mismo orden y con los mismos nombres que la pantalla ([`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)).
2. Lo capturado antes del fallo **no se pierde**: sigue en el dispositivo y sincroniza cuando se recupere; si el dispositivo no se recupera, lo registrado en él se reconstruye desde el papel y se declara así.
3. La digitación posterior se rige por [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md): quién digitó, quién es el autor del hecho, adjunto del original fotografiado, y la fecha del hecho que consta en el papel. **Lo que el papel no trae, no se inventa**: el odómetro intermedio que nadie anotó se registra como *no consignado en el original*, no se deduce restando ([`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)).
4. El diferimiento **se imputa a la causa correcta**: sin conectividad es condición de la delegación; sin dispositivo asignado es condición institucional. Ninguna de las dos es falta del motorista. Sin esa distinción, el indicador castiga a quien opera donde no hay señal y el resultado es predecible: dejan de reportar.

**E7 — Conduce alguien distinto del motorista declarado** (en el paso 1)
1. La habilitación se verifica sobre **quien efectivamente conduce**, cualquiera sea su puesto ([`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md)). No es regla del padrón de motoristas: es regla de quien va al volante.
2. El dispositivo solo acepta al titular o a un relevo declarado en la programación con su verificación de licencia registrada.
3. Si condujo alguien no habilitado, el hecho **se registra igual** —ya ocurrió— con justificación obligatoria, y produce hallazgo al conciliar. Ocultarlo es peor que registrarlo.

**E8 — El paquete de misión no trae un catálogo necesario** (en los pasos 4, 6, 8)
1. El evento se registra con la tipificación más cercana disponible más texto libre; **nunca se impide el registro por falta de catálogo**.
2. La resolución definitiva se hace al sincronizar, y si exige criterio humano va a la cola de [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md).

**E9 — Un agente de tránsito o una comisión del TSC detiene el vehículo** (en cualquier paso)
1. El control en carretera es **físico**: `ACT-15` verifica la Orden de Misión, el salvoconducto y el manifiesto impresos, por folio, QR y hash. **No se autentica en el sistema y no ve el expediente**: solo folio, tipo de documento, institución, vigente o anulado, vehículo, ventana autorizada y hash. Nunca nombres de personas trasladadas ni montos.
2. El motorista registra el retén como evento de bitácora con hora y ubicación; si deriva en multa o retención del vehículo, es interrupción ([`CU-09`](CU-09-interrupcion-en-ruta-y-desenlace.md)).
3. `[C]` Si la institución acepta exponer un punto de verificación público siendo el despliegue on-premise; alternativa sin exposición externa: contraste del hash impreso más consulta telefónica `[I]` ([`PR-01`, E8](../../01-negocio/procesos/PR-01-movilizacion-institucional.md), pendiente G).

---

## Reglas aplicables

| Regla | Qué gobierna en este caso |
|---|---|
| [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) | **Regla rectora**: toda captura de campo se completa sin ninguna conectividad y nunca se pierde |
| [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) | Identificadores generados en el cliente; folios de rangos por delegación |
| [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) | Adjunto pendiente ≠ ausente; nada se resuelve por sobrescritura |
| [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) | `ocurrido_en` y `capturado_en` obligatorios y distintos; el cálculo usa la del hecho |
| [`RN-76`](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md) | Estado declarado con un toque; tiempo en sitio derivado; espera improductiva tipificada |
| [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) · [`RN-83`](../../01-negocio/reglas/RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md) | Consumo con galones, monto, estación, odómetro y foto; todo ingreso es abastecimiento con fuente |
| [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) · [`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) · [`RN-92`](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) | El motorista resuelve la discrepancia de peaje donde ocurre |
| [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) · [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) · [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) | Tarifa y coherencia calculadas con el paquete congelado y la fecha del hecho |
| [`RN-77`](../../01-negocio/reglas/RN-77-versionado-del-alcance-autorizado.md) · [`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) | Extensión versionada; acto sin autorización previa se convalida y se declara tal como ocurrió |
| [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) · [`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) · [`RN-90`](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md) | Coherencia del odómetro y kilometraje acumulado como atributo del expediente |
| [`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) · [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) · [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) | Entregas de carga y de personas en ruta |
| [`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) · [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) | Papel emitido por el sistema y digitación diferida como transcripción |
| [`RN-85`](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) · [`RN-87`](../../01-negocio/reglas/RN-87-gasto-imprevisto-en-ruta.md) | Comprobante ausente y gasto imprevisto |
| [`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) | Habilitación de quien va al volante |

**Requisitos no funcionales determinantes:** [`RNF-03`](../no-funcionales/RNF-03-operacion-sin-conectividad.md) 7 días sin red, cero pérdida · [`RNF-12`](../no-funcionales/RNF-12-uso-en-campo.md) sol, guantes, gama baja, batería · [`RNF-04`](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) hash encadenado desde el dispositivo · [`RNF-08`](../no-funcionales/RNF-08-seguimiento-en-ruta.md) antigüedad visible del dato de ruta · [`RNF-13`](../no-funcionales/RNF-13-cifrado-en-transito-y-en-reposo.md) el celular perdido no es una fuga · [`RNF-16`](../no-funcionales/RNF-16-idioma-accesibilidad-y-mensajes.md) mensajes que dicen qué hacer.

---

## Lo que este caso de uso prohíbe explícitamente

| Prohibido | Por qué |
|---|---|
| Exigir conectividad para cualquier registro de la lista | `RNF-03`, premisa rectora 5 |
| Inferir el estado del vehículo del silencio o de la falta de movimiento | [`RN-76`](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md) |
| Mostrar una posición sin su antigüedad | [`RN-76`](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md), [`RNF-08`](../no-funcionales/RNF-08-seguimiento-en-ruta.md) |
| Descartar un registro por falta de espacio, de catálogo o de adjunto | [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) |
| Editar desde oficina lo capturado en campo mientras la misión está `EN_RUTA` | `INV-27` |
| Cerrar la misión por inactividad o por silencio | §3.4, transiciones prohibidas |
| Anular una misión que ya salió | `EN_RUTA → ANULADA` no existe |
| Usar la tabla de tarifas actual del servidor en lugar del paquete congelado | `EF-03`, [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) |

---

## Trazabilidad

- **Transiciones:** `T-17` prórroga, destino adicional y relevo, con código fuera de línea · `T-18` cierra la ejecución en [`CU-10`](CU-10-registrar-retorno-y-cerrar-bitacora.md) · `EF-07` captura desconectada
- **Invariantes:** `INV-24` a `INV-28` del estado `EN_RUTA`
- **Bloqueos duros:** `BD-05` odómetro · `BD-02` y `BD-03` revalidados en la prórroga
- **Secciones de la máquina de estados:** §6.1 qué puede el cliente desconectado · §6.2 se sincroniza el diario, nunca el estado · §6.4 tres marcas de tiempo · §6.5 qué muestra el sistema mientras no sabe nada · §6.6 código de autorización fuera de línea
- **Puntos de control de `PR-01`:** `PC-11` coherencia del odómetro · `PC-14` falta de ticket de caseta · `PC-12` manifiesto
- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E9 y E10
- **Casos especiales:** [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) · [`CE-06`](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) · [`CE-08`](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) · [`CE-24`](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) · [`CE-25`](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) · [`CE-22`](../casos-especiales/CE-22-odometro-inconsistente.md) · [`CE-21`](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) · [`CE-02`](../casos-especiales/CE-02-averia-mecanica-en-ruta.md)
- **Casos de uso relacionados:** [`CU-06`](CU-06-despachar-y-registrar-salida.md) entrega el paquete de misión · [`CU-09`](CU-09-interrupcion-en-ruta-y-desenlace.md) · [`CU-10`](CU-10-registrar-retorno-y-cerrar-bitacora.md) · [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md)
- **Normativa:** [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` ausencia de conectividad rural, papel y paridad pantalla↔papel · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) registro oportuno TSC-NOGECI V-10 y fecha del hecho ≠ fecha de captura · [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md) discrepancias y tickets · [`NRM-06`](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) guía de actuación y habilitación
- **Historias:** pendientes del Bloque 3
- **Insumos pendientes:** #67 (duración máxima real de misión: si supera 7 días, sube el umbral de `RNF-03`) · #69 (dispositivo de campo de referencia) · #2 (formatos en papel vigentes) · #24 y #25 (medio de pago de peajes: efectivo anticipado, peculio propio o tag prepago) · pendiente G (punto de verificación público del QR)
