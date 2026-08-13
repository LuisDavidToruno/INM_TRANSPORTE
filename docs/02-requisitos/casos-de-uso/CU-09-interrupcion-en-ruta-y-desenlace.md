# CU-09 — Registrar una interrupción en ruta y resolver su desenlace

| Campo | Valor |
|---|---|
| **Módulos** | M-08 Ejecución y Bitácora · M-12 Incidentes, Siniestros y Sanciones · M-11 Mantenimiento y Taller |
| **Actor principal** | `ACT-06` Motorista registra el hecho, sin conectividad |
| **Actores secundarios** | `ACT-04` Jefe de Transporte decide y registra el desenlace · `ACT-11` Encargado de Mantenimiento abre la orden de trabajo · `ACT-10` Encargado de Delegación apoya desde la delegación más cercana · `ACT-13` Custodio · `ACT-12` Auditor Interno (lectura) · `ACT-08` Gerencia Administrativa (cierre) |
| **Precondiciones** | La misión está `EN_RUTA`, o `DESPACHADA` con el hecho ocurrido antes de salir del predio. La bitácora está abierta. |
| **Postcondiciones** | Existe un evento `INTERRUPCION_EN_RUTA` tipificado, con hora del hecho, hora de captura, ubicación, odómetro, causa, descripción y fotografías. **La misión sigue `EN_RUTA` con la marca "interrumpida"**. El vehículo cambió de estado operativo desde la hora del hecho. Existe desenlace registrado, o la marca sigue viva con responsable y plazo. |
| **Disparador** | Un hecho impide continuar la misión según lo autorizado: avería mecánica, accidente de tránsito, sustracción del vehículo o de la carga, incapacidad del conductor, vía cerrada, condición de seguridad, retención del vehículo por autoridad. |

---

## Las dos cosas que este caso separa

**El hecho** ocurrió a una hora concreta y hay que registrarlo de inmediato, con o sin red. **La decisión** puede tardar horas y depende de personas que no están en la carretera.

Sin esa separación, el registro del hecho queda rehén de la decisión, y lo que pasa en la práctica es que no se registra nada hasta que se resuelve — dejando un hueco de días en el expediente ([`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)).

**`EN_RUTA → ANULADA` está prohibida y con razón:** el vehículo salió y hubo consumo real de recursos públicos. Anular sería borrar un hecho. La interrupción es la forma correcta de representar lo que efectivamente pasó.

---

## Flujo principal

1. Ocurre el hecho. `ACT-06` abre la misión en el dispositivo y toca *novedad*.
2. **Si la causa involucra personas —accidente con lesionados, sustracción con violencia—, el cliente muestra la guía de actuación antes de cualquier formulario.** Primero se atiende; después se captura. El registro mínimo se puede diferir sin perderse ([`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), guía provista en el paquete de misión por `EF-03`).
3. `ACT-06` registra el evento `INTERRUPCION_EN_RUTA` con: **hora del hecho**, hora de captura no editable, ubicación descrita —punto de referencia, no coordenadas obligatorias—, **odómetro**, causa del catálogo `causa_interrupcion`, descripción breve y fotografías. Todo sin ninguna conectividad ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RNF-03`](../no-funcionales/RNF-03-operacion-sin-conectividad.md)).
4. El registro **no pide, no sugiere y no admite atribución de responsabilidad**. Quién tuvo la culpa se determina en el expediente, no en la carretera y no por el conductor ([`RN-74`](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md)).
5. El evento **marca la misión como interrumpida y no le cambia el estado**. La Orden sigue `EN_RUTA`, con la lista de pendientes visible. Es el mismo mecanismo de marca que la máquina de estados usa para "anulación en trámite" en `T-15`: **una marca, no un estado inventado**.
6. Según la causa, el evento **cambia el estado operativo del vehículo desde la hora del hecho, no desde la hora de captura**: `EN_TALLER` por `W-07` o `NO_DISPONIBLE` por `W-08`, siempre con causa tipificada ([`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md)). Un vehículo averiado no puede aparecer como asignable para la misión de mañana.
7. Según la causa, se abre **expediente en M-12** con responsable y plazo, y/o **orden de trabajo correctiva en M-11**. El estado del vehículo lo registran los propios motoristas desde el campo ([`DP-001, D-08`](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).
8. En cuanto hay señal, el evento sincroniza y **notifica** a `ACT-04`, a la jefatura de la delegación y a la dependencia solicitante. Si no hay señal, **no se pierde**: espera en la cola del dispositivo ([`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md)).
9. `ACT-04` decide el desenlace y lo registra, con motivo. La única facultad que no espera es la del conductor de **detener la misión por riesgo inmediato**, que se convalida después ([`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).
10. **Toda interrupción exige desenlace explícito, tipificado y registrado**, del catálogo de [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md):

| Desenlace | Qué hace el sistema |
|---|---|
| **Continuar** con el mismo vehículo y conductor | Se levanta la marca, con constancia de quién lo autorizó y a qué hora. El tiempo perdido queda en el indicador |
| **Continuar con sustitución** de vehículo o de conductor | Ver A2 y A3 |
| **Retorno anticipado** | `T-18` subtipo retorno anticipado; la liquidación es por lo efectivamente ejecutado ([`CU-10`](CU-10-registrar-retorno-y-cerrar-bitacora.md), [`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md)) |
| **Retorno sin vehículo**, con la unidad resguardada, retenida o sustraída | `T-18` subtipo retorno sin vehículo, con expediente de incidente obligatorio ([`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)) |

11. La liquidación se hace **por lo efectivamente ejecutado**, con imputación por tramo cuando hubo más de un vehículo o conductor ([`RN-72`](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)).
12. **Ninguna misión con marca de interrupción sin desenlace puede quedar viva al cierre del período**: el sistema las lista para `ACT-04` y `ACT-08` con responsable y plazo, y lo no terminal al corte constituye el saldo de apertura del ejercicio siguiente ([`RN-97`](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md), [`CE-27`](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md)).

---

## Flujos alternos

**A1 — Avería mecánica que impide continuar** ([`CE-02`](../casos-especiales/CE-02-averia-mecanica-en-ruta.md), desde el paso 3)
1. Causa tipificada de falla; fotografías del vehículo y del punto.
2. El evento genera automáticamente la **orden de trabajo correctiva** en M-11 y lleva el vehículo a `EN_TALLER` o `NO_DISPONIBLE` (`W-07`, `W-08`).
3. Si el vehículo queda en un taller o en un predio ajeno, se registra **acta de resguardo**: dónde quedó, bajo responsabilidad de quién, y la **obligación de recuperación con responsable y plazo** ([`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)).
4. `[C]` Escala de severidad de fallas — insumo #35.

**A2 — Continúa con vehículo sustituto** (desde el paso 10)
1. Se abre un **tramo nuevo bajo la misma Orden de Misión**, con el vehículo y el motorista sustitutos, previa **revalidación completa de `BD-02`, `BD-03` y `BD-07` contra el paquete normativo congelado que lleva el dispositivo** ([`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md)).
2. La bitácora del vehículo original **se cierra en el punto de la avería con su odómetro**. Combustible, peajes y kilometraje se imputan a cada vehículo por separado: promediar dos vehículos distintos produce un rendimiento que no existe ([`RN-72`](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)).
3. La **categoría de peaje del vehículo entrante puede ser distinta**: el estimado del tramo restante se recalcula con el paquete congelado y se vuelve a congelar, con asiento de diferencia ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md), [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)).
4. Si la carga se transborda, se registra **acta de transbordo** con inventario, hora y firma de quien entrega y quien recibe. Si se resguarda en un tercer lugar, se registra dónde y bajo responsabilidad de quién. **La cadena de custodia no se interrumpe porque el vehículo se haya averiado** ([`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md), [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
5. **El fondo no se recalcula, se imputa.** Lo entregado ya está entregado; lo que cambia es a qué tramo se imputa cada consumo, y eso lo determina el odómetro y el vehículo de cada evento ([`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)).
6. **Ver la nota de hallazgo al pie**: hoy no existe transición que respalde este desenlace.

**A3 — Continúa con conductor sustituto** (desde el paso 10)
1. `T-17` relevo, con acta de traspaso: hora, lugar, **odómetro como corte de imputación**, identidad de quien entrega y de quien recibe, y motivo tipificado ([`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)). Detalle en [`CU-07`](CU-07-sustituir-vehiculo-o-motorista.md) A5.
2. El **fondo se traspasa solo por acta propia**, con conteo de folios uno por uno. Sin acta, permanece a nombre del receptor original.
3. Un consumo imputado a un folio ya traspasado es **alerta automática**.

**A4 — Accidente de tránsito** ([`CE-03`](../casos-especiales/CE-03-accidente-de-transito-en-mision.md), desde el paso 2)
1. Guía de actuación primero. Registro después, con las fotografías y la hora del hecho.
2. De los terceros y los lesionados solo se capturan **los datos mínimos del catálogo autorizado**, y toda consulta posterior a ese dato queda registrada ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), [`NRM-07`](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)).
3. **Ninguna casilla del formulario de campo pregunta de quién fue la culpa** ([`RN-74`](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md)).
4. Si la autoridad **retiene el vehículo**, el desenlace es retorno sin vehículo; el bien retenido **permanece en el registro** hasta su recuperación o descargo, y jamás se declara *dado de baja* por estar retenido ([`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)).
5. El incidente abierto impide el cierre limpio: el camino es `T-22`, y **el hallazgo no imputa responsabilidad a nadie** — es marca de seguimiento.

**A5 — Robo del vehículo o de la carga** ([`CE-04`](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md), desde el paso 3)
1. Se registra lo que se sepa, cuando se pueda. **El expediente de la misión puede haberse ido con el vehículo**: la reconstrucción se hace desde el servidor con lo último sincronizado, más el papel, más la declaración del motorista, todo declarado como tal.
2. Desenlace: retorno sin vehículo. El odómetro se declara **estimado y se marca como tal**; el expediente de incidente es obligatorio.
3. El bien sustraído no sale del registro ([`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)). `[C]` Responsabilidad patrimonial por el bien sustraído bajo custodia de misión — insumo #47.

**A6 — El motorista se incapacita en carretera** ([`CE-10`](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md), desde el paso 3)
1. Puede que el registro lo haga un tercero desde otro dispositivo: es legítimo, se aplica marcado como **"de dispositivo no portador"** y no automáticamente (§6.3, regla 4).
2. La custodia del vehículo **se cierra siempre**, aunque el conductor no pueda firmar: consta el impedimento y firman **dos personas presentes** más el receptor tipificado ([`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md)).
3. El dato de salud del servidor entra en el alcance de minimización de [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md): se registra el hecho, no el diagnóstico.
4. El retorno **no libera al motorista** si se registró con evento de incapacidad ([`RN-79`](../../01-negocio/reglas/RN-79-el-retorno-constatado-libera-al-vehiculo.md)).

**A7 — La interrupción se resuelve sola** (desde el paso 10)
1. La vía cerrada se abre a las dos horas. Se registra el desenlace *continuar* con su hora.
2. **La marca queda en el expediente** aunque el problema se haya resuelto, y el tiempo perdido entra en el indicador ([`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md)). Borrar la marca porque ya no duele es perder el único dato que explica por qué la misión llegó tarde.

---

## Flujos de excepción

**E1 — No hay forma de contactar a `ACT-04` para decidir el desenlace** (en el paso 9)
1. `ACT-06` registra el hecho **y la decisión que tomó**, con justificación obligatoria.
2. La falta de autorización previa **se resuelve en la liquidación**, con el mismo tratamiento que `T-17` da a la prórroga sin código: se convalida en plazo y la cronología se declara tal como ocurrió ([`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).
3. Si hay canal —radio, teléfono, un punto con señal—, `ACT-04` emite el **código de autorización fuera de línea** para la transición concreta (§6.6).
4. `[C]` Quién convalida y en qué plazo; quién puede ordenar el retorno anticipado — insumos #32 y #50.

**E2 — El desenlace exige cambiar el vehículo, y no hay transición que lo permita** (en A2)
1. `T-17` cubre prórroga, destino adicional y **relevo de motorista** — no cambio de vehículo.
2. Se registra el tramo nuevo como hechos de bitácora bajo la misma Orden y se deja constancia de que la transición no existe. **No se inventa un estado ni se fuerza `T-17` a significar algo que no dice.**
3. Ver la nota de hallazgo al pie.

**E3 — El desenlace no se decide y la marca queda viva** (en el paso 12)
1. La marca "interrumpida" permanece con **responsable y fecha límite asignados**, visible en el expediente y en el listado de la delegación.
2. `BD-08` y el incidente abierto impiden el cierre limpio; el camino es `T-22` cierre con hallazgo.
3. **No se puede cerrar el ejercicio con misiones en esta marca** ([`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md), [`RN-97`](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md)).

**E4 — Alguien intenta anular la misión** (en cualquier paso)
1. `EN_RUTA → ANULADA` **no existe**. El sistema no ofrece la acción.
2. El camino es `T-18` con subtipo retorno anticipado o retorno sin vehículo, y luego liquidar.

**E5 — El vehículo resguardado fuera de sede que nadie recupera** (desde A1 o A4)
1. La obligación de recuperación tiene responsable y plazo; su incumplimiento entra al **saldo de apertura del ejercicio siguiente** con antigüedad contada desde el hecho ([`RN-97`](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md)).
2. El vehículo permanece `NO_DISPONIBLE` con causa tipificada, nunca en un limbo sin causa: sin tipificación, ese estado se convierte en el cementerio donde se esconde la flota que nadie repara.

**E6 — La interrupción ocurre con la misión todavía `DESPACHADA`, antes de salir del predio** (en el paso 1)
1. [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) aplica a toda misión `DESPACHADA` o `EN_RUTA`.
2. Si no hubo consumo del fondo: `T-15` con devolución íntegra. Si hubo cualquier consumo: `T-16` hacia `RETORNADA`, y la misión se liquida aunque su kilometraje sea cero ([`CE-20`](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md)).

**E7 — El evento de interrupción entra en conflicto al sincronizar** (en el paso 8)
1. Caso real: la oficina, sin saber nada, gestionó otra cosa sobre la misma misión mientras el motorista estaba sin señal.
2. Ninguna versión se descarta ni se sobrescribe: ambas se conservan y el conflicto va a la cola de resolución humana ([`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md), [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md)).

---

## Reglas aplicables

| Regla | Qué gobierna en este caso |
|---|---|
| [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) | **Regla rectora**: evento tipificado, marca sin cambio de estado, desenlace obligatorio |
| [`RN-74`](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md) | El campo registra hechos, no culpas |
| [`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) | El bien retenido, sustraído o resguardado permanece en el registro |
| [`RN-71`](../../01-negocio/reglas/RN-71-traspaso-en-ruta-con-acta-y-corte-de-odometro.md) · [`RN-72`](../../01-negocio/reglas/RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md) | Acta de traspaso, corte de odómetro e imputación por tramo |
| [`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) · [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) | Sustitución con revalidación y recongelamiento |
| [`RN-73`](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) | Decisión tomada sin poder consultar, convalidada después |
| [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) | El vehículo averiado sale de circulación desde la hora del hecho |
| [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) · [`RN-69`](../../01-negocio/reglas/RN-69-inventario-de-la-carga-y-acta-de-entrega.md) | Custodia del vehículo y de la carga durante la interrupción |
| [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) · [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) | Datos de terceros de siniestro y de salud del servidor |
| [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) · [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) · [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) | Registro sin red, sin pérdida, con hora del hecho |
| [`RN-78`](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) · [`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) | Grado de cumplimiento del objeto e indicadores por causa tipificada |
| [`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) · [`RN-97`](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md) | Ninguna interrupción sin desenlace sobrevive al cierre del período |

---

## Notas de hallazgo — no se resuelven aquí

**1. No existe transición para sustituir el vehículo con la misión `EN_RUTA`.** Reportado desde [`CE-02`](../casos-especiales/CE-02-averia-mecanica-en-ruta.md), [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) y [`CU-07`](CU-07-sustituir-vehiculo-o-motorista.md) hacia [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md), autoridad en transiciones. Este caso de uso **describe el desenlace que la regla exige y la transición que no existe**, y no lo resuelve en silencio.

**2. Divergencia en el número de desenlaces.** [`CE-02`](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) enumera **tres** desenlaces —continuar con vehículo sustituto, abortar y retornar, y *quedar pendiente de resolución*—. [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) enumera **cuatro** —continuar, continuar con sustitución, retorno anticipado, retorno sin vehículo— y trata *pendiente de resolución* no como desenlace sino como **ausencia de desenlace**, que es lo que la marca viva significa.

Este caso de uso sigue a `RN-70`, por precedencia: en materia de negocio manda la regla. La distinción no es semántica — decide si el tablero de `ACT-04` cuenta "misiones resueltas" incluyendo las pendientes, que es exactamente el número que no debe inflarse. Se reporta hacia `CE-02` para que su tabla se alinee con la regla que de él derivó.

**3. `T-18` no tipifica el retorno del personal con el vehículo resguardado en sitio.** Ya listado como pendiente en el [índice de casos especiales](../casos-especiales/README.md). Es el desenlace de A1 punto 3 y hoy se registra como *retorno sin vehículo*, que dice algo distinto: el vehículo existe, está identificado y hay obligación de recuperarlo.

---

## Trazabilidad

- **Transiciones:** `T-17` relevo y prórroga · `T-18` subtipos retorno anticipado y retorno sin vehículo · `T-15` y `T-16` si el hecho ocurre en `DESPACHADA` · `T-22` cierre con hallazgo · `W-07`, `W-08` estado operativo del vehículo
- **Prohibida:** `EN_RUTA → ANULADA`
- **Criterios de hallazgo:** `H-06` incidente sin resolución en M-12 · `H-05` circulación en franja inhábil sobrevenida · `H-02` kilometraje fuera de umbral
- **Puntos de control de `PR-01`:** `PC-04`, `PC-05`, `PC-11`
- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E9 y E11; deriva a `PR-05` mantenimiento y `PR-06` incidentes
- **Casos especiales:** [`CE-02`](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) · [`CE-03`](../casos-especiales/CE-03-accidente-de-transito-en-mision.md) · [`CE-04`](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) · [`CE-10`](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) · [`CE-05`](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) · [`CE-06`](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) · [`CE-07`](../casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) · [`CE-20`](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) · [`CE-27`](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md)
- **Casos de uso relacionados:** [`CU-08`](CU-08-ejecucion-en-ruta-sin-conectividad.md) es el contexto de captura · [`CU-07`](CU-07-sustituir-vehiculo-o-motorista.md) la sustitución · [`CU-10`](CU-10-registrar-retorno-y-cerrar-bitacora.md) los desenlaces de retorno · [`CU-11`](CU-11-sincronizar-y-resolver-conflictos.md)
- **Normativa:** [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` registro sin conectividad · [`NRM-06`](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) guía de actuación en accidente y habilitación del sustituto · [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) custodia y permanencia del bien en el registro · [`NRM-07`](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) datos de terceros · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) registro oportuno
- **Requisitos no funcionales:** [`RNF-03`](../no-funcionales/RNF-03-operacion-sin-conectividad.md) · [`RNF-12`](../no-funcionales/RNF-12-uso-en-campo.md) · [`RNF-14`](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md)
- **Historias:** pendientes del Bloque 3
- **Insumos pendientes:** #35 (escala de severidad de fallas) · #47 (responsabilidad patrimonial por el bien sustraído bajo custodia) · #32 y #50 (quién convalida, en qué plazo, y quién ordena el retorno anticipado) · #1 (si un incidente abierto impide cerrar la misión)
