# CE-06 — "Ya que el carro anda por allá": la misión de tres días termina en seis, con dos destinos que nadie autorizó

| Campo | Valor |
|---|---|
| **Módulos** | M-07 Programación y Despacho, M-08 Ejecución y Bitácora, M-09 Combustible, M-18 Peajes, M-04 Documentación, M-15 Formatos, M-19 Seguimiento en Ruta, M-13 Liquidación, M-20 Integraciones |
| **Estados afectados** | `EN_RUTA` — autotransición `T-17`, repetible |
| **Frecuencia** | **Frecuente.** Es probablemente el caso especial más común de todos |
| **Impacto** | Financiero y de auditoría, con un flanco de control interno: la extensión es la vía natural para saltarse el circuito de autorización |
| **Resolución** | Definida. Deja un hallazgo contra la máquina de estados y dos decisiones al PO |

## La situación

Un pickup sale el martes de Tegucigalpa hacia San Pedro Sula con mobiliario para la delegación. Ventana autorizada: martes 06:00 a jueves 17:00. Ruta autorizada: CA-5, con paso por las casetas de Zambrano, Siguatepeque y Yojoa en cada sentido. Distancia estimada 480 km ida y vuelta. Fondo entregado para eso. Salvoconducto: no lleva, porque la ventana es de días hábiles.

El miércoles a mediodía, el Jefe de Transporte llama al motorista: *"ya que el carro anda por allá"*, que pase por Puerto Cortés a dejar cuatro cajas de formularios que pidió aquella delegación, y que de regreso recoja en Omoa la documentación del año pasado que hay que traer a la sede. Con eso el retorno ya no es el jueves: es el **sábado** a media mañana.

El resultado, sumado:

| | Autorizado | Real |
|---|---|---|
| Días | 3 | 5 |
| Destinos | 1 | 3 |
| Kilómetros | 480 | 830 |
| Retorno | jueves, día hábil | **sábado, día inhábil** |
| Pasos por caseta | 6 | 8 |
| Fondo | para 480 km | insuficiente desde el jueves por la tarde |

Y hay tres cosas más que nadie está mirando el miércoles a mediodía:

- **Ese pickup tiene misión programada para el viernes** a Danlí. El viernes alguien va a estar esperando en el predio.
- La licencia del motorista vence el **sábado**. Estaba bien para una misión que retornaba el jueves.
- Las cajas de Puerto Cortés las pidió una dependencia que **nunca metió una solicitud de transporte**. Su necesidad entró al sistema por la puerta de atrás.

## Qué se hace hoy sin sistema

Se autoriza por teléfono o por mensaje y se arregla al volver. La bitácora se completa a mano con los kilómetros reales. Cuando el fondo no alcanza, **el motorista pone de su bolsillo** y después pelea el reembolso, o consigue que le fíen en una estación conocida.

Al liquidar, el kilometraje real no cuadra contra la ruta autorizada y aparece un ticket de caseta de un punto que la Orden de Misión no menciona. Entonces empieza la explicación por escrito, tres semanas después, cuando ya nadie se acuerda de la hora exacta ni de quién dijo qué.

`[C]` **Si la institución admite y reembolsa combustible pagado por el motorista de su bolsillo** — insumo #37, ya registrado. `[C]` **Si la extensión de misión tiene autorizador distinto según su magnitud** (un día más lo autoriza el Jefe de Transporte; tres días y dos destinos, ¿también?). Insumo nuevo #49.

## Por qué el flujo normal no lo cubre

`T-17` existe para esto y cubre las tres situaciones: más ventana, más destino, relevo. Pero está escrita como **un registro**, y aquí lo que ocurre es que **la misión aprobada deja de ser la misión que se está ejecutando**. Los puntos ciegos:

1. **`T-17` no revalida `BD-02` ni `BD-03` en la prórroga.** Las revalida solo en el relevo de motorista. Pero `BD-02` exige que la licencia esté vigente **durante todo el rango, incluida la holgura posterior**, y la prórroga **mueve el fin del rango**. La licencia que era válida para una ventana que cerraba el jueves puede no serlo para una que cierra el sábado. Lo mismo la matrícula (`BD-03`). Ver el hallazgo.
2. **La reserva del vehículo no se estira sola.** `EF-01` reserva sobre la ventana efectiva. Extender la misión invade la reserva de la misión del viernes, y hoy nada dice qué pasa con la segunda.
3. **El destino adicional no tiene solicitante ni autorizador propios.** Un destino agregado en ruta atiende la necesidad de alguien. Si esa necesidad no queda atribuida a una dependencia y a un autorizador, `BD-01` se evade legalmente: el circuito de autorización se saltó, y nadie mintió.
4. **El fondo se agotó y nadie puede ampliarlo desde la carretera.** `PC-08` sitúa la ampliación del fondo en `ACT-08`, en la sede, con red.
5. **La cuota trimestral.** `RN-54` valida el compromiso contra la cuota del trimestre. La extensión compromete gasto adicional que nadie validó contra nada.

## Regla de resolución

**1. La extensión es una enmienda al expediente, no una anotación.** Cada `T-17` produce una **versión de la misión autorizada**, con su alcance, su ventana, su ruta, su estimado de peajes y su autorizador. El expediente conserva todas y muestra **cuál estaba vigente en cada momento**. La original nunca se pierde.

   De aquí sale el criterio que resuelve la mitad de los problemas de liquidación: **toda validación posterior se hace contra la versión vigente a la fecha del hecho**, no contra la original ni contra la última. El paso por la caseta de Puerto Cortés el jueves a las 15:00 se evalúa contra la versión que estaba vigente el jueves a las 15:00. Es la aplicación literal de [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) al alcance de la misión, no solo a las tarifas.

**2. Toda prórroga revalida `BD-02` y `BD-03` contra la nueva fecha de fin.** Con el padrón y la matriz del paquete congelado, sin red. En el ejemplo, la licencia vence el sábado: si la holgura posterior empuja el rango más allá del sábado, **bloquea**, y la misión no se puede extender con ese motorista — el camino es el relevo de `CE-05`, o el retorno anticipado de `CE-07`. No es negociable: `BD-02` no tiene excepción configurable.

**3. El destino adicional identifica a quién sirve y quién lo autoriza.** Se registra la **dependencia requirente**, el objeto del traslado que se agrega, y el autorizador. `BD-01` aplica igual que en una solicitud nueva: **quien pidió el destino adicional no puede ser quien lo autoriza**. Si el destino atiende a una dependencia distinta de la solicitante original, el sistema lo marca para que la liquidación pueda atribuirle el costo.

   Este punto no es burocracia. *"Ya que el carro anda por allá"* es el mecanismo por el cual una necesidad no autorizada, no presupuestada y a veces no institucional entra a un vehículo del Estado. El sistema no lo impide — a veces es lo más eficiente que puede hacer la institución — pero **lo deja escrito con nombre y apellido**.

**4. Si la extensión toca día u hora inhábil, se aplica `BD-04` tal como está escrito.** El motorista no puede conseguir la firma de `ACT-09` un viernes por la tarde desde Omoa. Entonces: se registra el hecho con justificación obligatoria, **la extensión a franja inhábil queda marcada**, y se resuelve en la liquidación con `H-05` si no se justifica. Si sí hay forma de tramitarlo, se emite el permiso y su salvoconducto se imprime en la delegación más cercana con folio del rango de esa delegación ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)).

**5. El estimado se recalcula con el paquete congelado, y la desviación se explica sola.** Los dos pasos adicionales por caseta se estiman con las tarifas del paquete de `EF-03`, no con la tabla que el servidor tenga hoy ([`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)). Y la conciliación de `EF-05` compara contra la **ruta autorizada vigente al momento de cada paso**: un ticket de Puerto Cortés en una misión cuya versión vigente incluye Puerto Cortés **no es `H-03`**. Sin esta regla, cada extensión legítima produciría un hallazgo falso, y los hallazgos falsos son los que enseñan a la gente a ignorar el sistema.

**6. El conflicto con la misión del viernes se muestra el miércoles, no el viernes.** Registrar la prórroga recalcula la ventana efectiva del vehículo y del motorista y **dispara el conflicto de reserva** (`EF-01`, [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md)) contra la misión de Danlí. Se resuelve por los cuatro caminos de `EF-01` — consolidar, asignar otro recurso, reprogramar, o escalar prioridad con `T-11` — y **la dependencia afectada se entera por el sistema**, no por un servidor esperando en el predio.

   Aquí hay un detalle offline que importa: la prórroga se registra en el dispositivo y puede tardar horas o días en sincronizar. **El conflicto se dispara al sincronizar, con la fecha del hecho.** Y por eso la extensión de ventana es de los pocos eventos que justifica intentar sincronización oportunista agresiva en cuanto haya un rastro de señal.

**7. El fondo no se amplía en la carretera: el gasto excedente se registra y se resuelve.** Se registra el consumo con su comprobante, su odómetro y su estación ([`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)) aunque exceda lo asignado, **con marca de excedido sobre el fondo**. Si lo pagó el motorista de su bolsillo, se registra así y entra al circuito de reembolso que la institución tenga — `[C]` insumo #37. Lo que no se hace nunca es dejar el gasto sin registrar porque el fondo ya se había agotado: un galón consumido y no registrado es un galón que aparece como faltante en el descargo de otro.

**8. La cuota trimestral se valida donde se puede validar.** `RN-54` es advertencia que escala a bloqueo, y en ruta no hay contra qué validar. Regla: la extensión registra el **compromiso adicional estimado** y su validación contra la cuota ocurre al sincronizar; si la cuota no lo soporta, es una alerta a `ACT-08` y una marca en la liquidación, **nunca un bloqueo retroactivo del gasto ya hecho**. No se puede desautorizar combustible que ya se quemó.

**9. Los viáticos adicionales no son de SIGTI.** Cinco días en lugar de tres significa dos noches más de viático. **Eso lo resuelve ARGOS** ([DP-001, D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). Lo que SIGTI hace, y es todo lo que debe hacer:

   - Registra el hecho: la ventana efectiva cambió de *X* a *Y*, autorizada por *Z*, en tal momento.
   - **Expone ese hecho con la clave de vinculación** de la Orden de Misión, para que ARGOS lo tome y gestione lo que le corresponde.
   - **No calcula el viático, no lo estima, no lo muestra y no lo liquida.** Ni siquiera "como referencia": un número de viático mostrado en SIGTI se convierte en el número que la gente usa, y entonces hay dos verdades sobre el mismo pago.

**10. La extensión repetida es un indicador, no una anécdota.** `T-17` es repetible. Una misión con cuatro prórrogas, o una dependencia cuyas misiones se extienden sistemáticamente, es un dato de planificación deficiente que hoy nadie mide. Se acumula por dependencia, por motorista y por tipo de misión, y alimenta los reportes de M-14.

### Hallazgo — `T-17` no revalida los bloqueos duros al mover el fin de la ventana

`T-17` lista entre sus precondiciones la autorización de `ACT-04` o `ACT-09`, y `BD-02` **solo para el relevo**. `BD-02` y `BD-03`, en cambio, exigen vigencia **durante todo el rango de la misión**, y la prórroga cambia el rango. Una misión prorrogada puede terminar con licencia vencida o matrícula vencida sin que ninguna precondición lo detecte, y eso aparecería después como `H-07` — que llega tarde, porque el vehículo ya circuló.

Por la [precedencia de `CLAUDE.md`](../../../CLAUDE.md), **la máquina de estados es la autoridad y este documento no la corrige**. Se reporta como **ampliación necesaria de `T-17`** dirigida a [`docs/03-arquitectura/estados/orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md): toda prórroga que mueva el fin de la ventana revalida `BD-02` y `BD-03` contra la nueva fecha, con el paquete congelado, y bloquea si fallan. Relacionado con `CE-11`.

### Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:versionado-del-alcance-autorizado` | Cada extensión produce una versión del alcance autorizado — ventana, destinos, ruta, estimado — con su autorizador y su vigencia. Toda validación posterior se hace contra la versión vigente a la fecha del hecho |
| `RN-c:revalidacion-de-habilitaciones-al-prorrogar` | Toda prórroga que mueva el fin de la ventana revalida licencia y documentación del vehículo contra la nueva fecha de fin, y bloquea si fallan |
| `RN-c:destino-adicional-con-dependencia-y-autorizador` | Todo destino agregado en ruta identifica la dependencia requirente, el objeto que se agrega y su autorizador, que no puede ser quien lo pidió |
| `RN-c:conciliacion-contra-el-alcance-vigente` | La coherencia de casetas, kilometraje y ruta se evalúa contra el alcance vigente al momento de cada hecho; un paso amparado por una extensión autorizada no es hallazgo |
| `RN-c:consumo-excedido-sobre-el-fondo` | El consumo que excede el fondo asignado se registra igual, marcado como excedido, con su comprobante y su odómetro; su cobertura se resuelve en la liquidación, nunca omitiendo el registro |
| `RN-c:notificacion-de-cambio-de-ventana-a-argos` | El cambio de ventana efectiva se expone a ARGOS con la clave de vinculación de la Orden de Misión. SIGTI no calcula, no estima ni muestra el viático |
| `RN-c:indicador-de-extension-recurrente` | La frecuencia de extensiones se acumula por dependencia, motorista y tipo de misión, y se reporta como indicador de calidad de la programación |

## Escalamiento al PO

**Decisión 1 — `[C]` ¿La magnitud de la extensión cambia quién la autoriza?** Insumo nuevo #49.

| Opción | Costo |
|---|---|
| Siempre `ACT-04`, salvo franja inhábil que exige `ACT-09` | Es lo que hoy dice `T-17`. Riesgo: se puede duplicar la duración y el costo de una misión sin que el nivel que la aprobó original se entere |
| Umbral configurable: pasado cierto porcentaje de días, kilómetros o costo estimado, la extensión escala al autorizador de la solicitud original | Coherente con `PC-02` (nivel competente) y con `RN-54`. Costo: el umbral es un parámetro más que la institución tiene que fijar, y en ruta puede no haber a quién escalar |
| Reautorización completa por el circuito original | Impracticable desde la carretera. Garantiza que se haga por teléfono y se registre falso |

**Recomendación del análisis**, no decisión: la segunda, con el umbral como parámetro con vigencia y con la salida ya prevista por `T-17` — si no hay forma de obtener la autorización, se registra el hecho y se resuelve en la liquidación.

**Decisión 2 — `[C]` ¿Qué pasa con la misión desplazada?** Cuando la extensión invade la reserva de otra misión ya programada, `EF-01` da cuatro caminos pero no dice **quién elige** cuando la elección se dispara desde la carretera, con el vehículo a 300 km. La recomendación es que la extensión **nunca** desplace automáticamente a la otra: el sistema abre el conflicto y quien resuelve es `ACT-04`, con `ACT-08` si hay que escalar prioridad.

## Evidencia que debe quedar

1. La versión original del alcance autorizado y **cada enmienda**, con su autorizador, su momento y su motivo tipificado
2. La revalidación de licencia y documentación contra la nueva fecha de fin, con sus insumos concretos
3. Para cada destino adicional: dependencia requirente, objeto del traslado y autorizador — **la evidencia de que no se saltó el circuito**
4. **Correlación de consumo, kilometraje y pasos por caseta contra el alcance vigente en cada momento** — que es lo que el auditor busca, y lo que sin versionado es imposible de sostener
5. Si hubo circulación en franja inhábil: el permiso, o la justificación registrada y su tratamiento como `H-05`
6. El consumo excedido sobre el fondo, con comprobante, odómetro y estación, y cómo se cubrió
7. El compromiso adicional y su validación contra la cuota trimestral, o la alerta de que no la soportaba
8. El registro del conflicto con la misión desplazada y cómo se resolvió
9. La constancia de que el cambio de ventana se expuso a ARGOS, **sin monto de viático calculado en SIGTI**

## Trazabilidad

- **Reglas**: [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) autorización · [`RN-06`](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) · [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) · [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) · [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) · [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) · [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) · [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) · [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) · [`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) cuota trimestral
- **Reglas candidatas**: las siete de la sección anterior
- **Transiciones**: `T-17` prórroga y destino adicional, **repetible** — la transición del caso · `T-11` si hay que desplazar la misión en conflicto · `T-18` si en vez de extender se aborta, ver `CE-07`
- **Bloqueos duros**: `BD-01` segregación en el destino adicional · `BD-02`, `BD-03` — **ver el hallazgo** · `BD-04` franja inhábil · `BD-11` solapamiento
- **Efectos**: `EF-01` reservas y conflicto · `EF-03` paquete congelado · `EF-05` conciliación contra el alcance vigente
- **Criterios de hallazgo**: `H-02` kilometraje fuera de umbral · `H-03` caseta incompatible — **no aplica si el alcance vigente la ampara** · `H-05` franja inhábil sin permiso · `H-01` desviación de consumo
- **Puntos de control**: `PC-02` nivel competente · `PC-03` salvoconducto · `PC-08` ampliación del fondo · `PC-11` odómetro · `PC-16` registro del acto de autorización
- **Fronteras**: [DP-001, D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — **viáticos fuera de alcance, los maneja ARGOS**. SIGTI expone el cambio de ventana, no calcula el viático adicional
- **Normativa**: [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) · [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) día inhábil · [`NRM-04`](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) cuota trimestral · [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md)
- **Actores**: `ACT-06` registra · `ACT-04` autoriza · `ACT-09` firma el permiso de franja inhábil · `ACT-08` cuota y prioridad · `ACT-10` imprime en delegación · `ACT-16` ARGOS recibe el cambio de ventana
- **Casos especiales relacionados**: `CE-02` avería · `CE-05` relevo de motorista · `CE-07` retorno anticipado · `CE-08` espera prolongada en sitio · `CE-11` licencia que vence durante la misión · `CE-12` dos solicitudes compiten por el mismo vehículo · `CE-21` galonaje que no cuadra
- **Insumos**: #37 reembolso de combustible pagado por el motorista — ya registrado · **#49 nuevo** — si la magnitud de la extensión cambia el nivel autorizador
- **Historias candidatas**: `HU-c:prorrogar-mision-en-ruta-sin-senal`, `HU-c:agregar-destino-con-dependencia-requirente`, `HU-c:ver-conflicto-de-reserva-provocado-por-una-prorroga`, `HU-c:registrar-consumo-excedido-sobre-el-fondo`, `HU-c:exponer-cambio-de-ventana-a-argos`
