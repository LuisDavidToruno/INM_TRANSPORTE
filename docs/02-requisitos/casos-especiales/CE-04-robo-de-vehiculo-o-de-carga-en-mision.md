# CE-04 — Sobre la CA-13 se llevan el pickup con la carga, y el expediente de la misión se va con él

| Campo | Valor |
|---|---|
| **Módulos** | M-12 Incidentes y Siniestros, M-03 Flota, M-04 Documentación, M-08 Bitácora, M-09 Combustible, M-18 Peajes, M-15 Formatos, M-13 Liquidación, M-14 Auditoría, M-16 Operación Desconectada |
| **Estados afectados** | `EN_RUTA` → `RETORNADA` subtipo **retorno sin vehículo**; vehículo a `NO_DISPONIBLE` por `W-08`; bloqueo de `CERRADA` por `H-06` |
| **Frecuencia** | Raro pero grave — y con dos variantes de muy distinta frecuencia: el robo del vehículo completo es raro; el faltante de carga es más común de lo que se admite |
| **Impacto** | Legal y patrimonial antes que nada; luego operativo, financiero y de auditoría |
| **Resolución** | Definida en lo operativo. `[C]` en tres puntos: plazos y destinatarios del aviso institucional, condicionamiento del cierre, y responsabilidad del custodio |

## La situación

**Variante A — se llevan el vehículo.** Martes, 19:10. Un pickup doble cabina sale de Tegucigalpa hacia Trujillo con dos motores fuera de borda y repuestos para la delegación de Colón. Sobre la CA-13, pasando Sabá, dos sujetos armados en motocicleta lo interceptan en un tramo sin alumbrado. Bajan al motorista y al acompañante, y se van con el vehículo, la carga, la carpeta con la Orden de Misión impresa y los vales, el tag de peaje pegado al parabrisas, y **el teléfono institucional donde estaba corriendo la bitácora**.

El motorista camina hasta un negocio en la carretera y desde ahí llama. La última sincronización del dispositivo fue en Tegucigalpa, antes de salir: los dos pasos por caseta, la carga de combustible en Guaimaca y las tres paradas registradas **están en un aparato que ya no existe**. El último odómetro que el servidor conoce es el de salida, 96,420.

**Variante B — se llevan solo la carga.** El mismo vehículo, otra misión. El motorista pernocta en Tocoa. A las 06:00 encuentra forzado el candado del cajón: faltan los dos motores fuera de borda; el vehículo está intacto y arranca. La misión puede continuar. Lo que no puede continuar es la entrega: en la delegación de Colón alguien va a firmar "recibido conforme" por algo que no llegó.

En ambas variantes hay un dato que la institución va a necesitar y que hoy nadie captura el mismo día: **el estado de la póliza del vehículo a la fecha del hecho**. Si venció en marzo y no se renovó — que es legal, la póliza no es obligatoria ([`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md), [DP-001 D-13](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)) — la pérdida la absorbe entera el Estado.

## Qué se hace hoy sin sistema

El motorista denuncia en la posta policial más cercana y consigue la constancia. Llama al Jefe de Transporte, que avisa a la Gerencia Administrativa. Se abre una carpeta física. Con el tiempo, y si el vehículo no aparece, se inicia el trámite de descargo del bien.

Lo que casi nunca queda registrado, y es lo que el auditor pregunta:

- **La hora exacta del hecho y el último kilometraje verificable.** Sin eso no hay forma de saber qué parte del combustible entregado se consumió en la misión y qué parte se consumió después, con el vehículo ya en manos de otro.
- **Qué llevaba el vehículo.** La carga se describe en la solicitud como "repuestos y equipo", no como un inventario con marca, serie y valor. Al momento de reclamar, o de responder, nadie sabe exactamente qué se perdió.
- **Qué instrumentos de pago iban a bordo.** Los vales impresos con folio y el tag de peaje siguen siendo utilizables por quien se llevó el vehículo. Nadie los bloquea porque nadie lleva la cuenta de cuáles iban dentro.

`[C]` **A quién y en qué plazo debe avisar la institución** — Auditoría Interna, la Dirección General de Bienes Nacionales, el TSC, el asegurador si hay póliza — y si existe procedimiento escrito de descargo por robo. `NRM-02` cubre el régimen de bienes del Estado pero **no** el plazo concreto de aviso por sustracción. No se inventa: insumo nuevo #46.

`[C]` **Si el motorista responde patrimonialmente por el bien bajo su custodia de misión**, y bajo qué régimen. Es una pregunta con consecuencia laboral y hoy la respuesta la da el criterio de quien esté a cargo. Insumo nuevo #47.

## Por qué el flujo normal no lo cubre

Cuatro cosas se rompen a la vez, y ninguna la contempla el camino feliz:

1. **El vehículo no vuelve, y puede no volver nunca.** `T-18` tiene el subtipo "retorno sin vehículo", pero fue pensado para un siniestro donde el vehículo está en un predio y se puede leer el odómetro. Aquí **no hay odómetro final que leer**, ni estimable con honestidad: el vehículo siguió rodando sin la institución.
2. **La fuente primaria del expediente se perdió.** `EF-07` establece que al salir, la fuente primaria pasa a ser el dispositivo portador. Si el dispositivo se va con el vehículo, el servidor queda con una versión que sabe incompleta y **no hay a qué sincronizarla**. La máquina de estados no dice qué hacer con eso.
3. **Hay folios vivos en manos de terceros.** `EF-02` tipifica el folio como reservado, consumido o anulado. Un vale sustraído no es ninguno de los tres: no se consumió por la misión, y anularlo sin más borraría el hecho de que existió y circuló.
4. **El motorista es la víctima, y el sistema le va a pedir que declare.** Igual que en `CE-03`, lo que escriba hoy puede leerse en un juzgado o en un expediente de responsabilidad administrativa dentro de dos años.

## Regla de resolución

**1. El evento se llama por su nombre y se declara sin señal.** Se tipifica **`SUSTRACCION`** con dos subtipos: **del vehículo** y **de la carga**. Se registra desde cualquier dispositivo — no necesariamente el portador, que puede haberse perdido — y **sin conectividad** ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)). El registro mínimo, en este orden: hora del hecho, lugar descrito, si hubo violencia o armas, si hay personas lesionadas, qué se llevaron, y qué instrumentos iban a bordo. Fecha del hecho y fecha de captura son campos distintos y ambos obligatorios ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)) — aquí la distancia entre las dos puede ser de horas.

**2. La primera pantalla no pide datos: muestra qué hacer.** Mismo criterio que `CE-03`: resguardarse, verificar personas, denunciar en la posta más cercana, avisar a la institución. El formulario viene después. Y **no existe campo de atribución de culpa**: los campos son de hecho observable. Un sistema que le pide a un servidor asaltado que declare si fue negligente produce evidencia contra su propia institución.

**3. El vehículo sale de la flota en el acto, y no se da de baja.** `W-08` lo lleva a `NO_DISPONIBLE` con causa tipificada "sustraído — bajo denuncia" ([`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md)). **No pasa a `DADO_DE_BAJA`**: el descargo es un acto formal de bienes del Estado con acta, que ejecuta `ACT-08` con intervención de `ACT-14`, y ocurre cuando la institución lo resuelve — no cuando el vehículo desaparece ([`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), `[P]`). Mientras tanto el expediente del vehículo sigue vivo, con número de denuncia, autoridad receptora y gestiones de recuperación. Un bien del Estado sustraído **sigue siendo un bien del que hay que responder**.

**4. El kilometraje se cierra en la última lectura verificable, y lo que sigue no se estima.** El odómetro de cierre es el último valor registrado por una fuente que la institución controla: la última sincronización, un ticket de caseta, una carga de combustible con odómetro, o la lectura de salida si no hay nada posterior. Ese valor se marca **"última lectura verificable"** y el tramo posterior queda como **kilometraje no determinado**.

   No se rellena con la distancia teórica de la ruta. Si se rellenara, el rendimiento km/galón de esa misión sería un número inventado que después alguien compara contra el umbral de `H-01` y produce un hallazgo sobre un fraude que no existió. **La conciliación de esta misión se hace hasta el punto del hecho y se declara truncada.**

**5. Los folios y los instrumentos de pago que iban a bordo pasan a `SUSTRAIDO`.** Estado nuevo en la máquina de folios de `EF-02`, distinto de `ANULADO`: anular dice "este documento nunca surtió efecto"; sustraído dice "este documento existe, está fuera de nuestro control, y cualquier uso posterior no es nuestro". Efectos:

   | Instrumento | Qué hace el sistema |
   |---|---|
   | Vales o fondo entregado no consumido | Pasan a `SUSTRAIDO`. **No se liquidan como consumo ni como devolución**: se liquidan como pérdida, con el evento de sustracción como respaldo ([`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)) |
   | Tag prepago de peaje | Se marca para bloqueo ante el concesionario, con hora de la solicitud y saldo al momento del hecho. `[C]` si la institución tiene tags — insumo #24 |
   | Orden de Misión y salvoconducto impresos | Pasan a `SUSTRAIDO`. Circula por el país un salvoconducto con folio y QR de esta institución: la **verificación del QR debe responder que el documento está sustraído**, no que es válido |
   | Placas y matrícula del vehículo | Se registran en el expediente del incidente para el aviso a la autoridad |

   **Todo consumo o paso por caseta imputado a esta misión con fecha posterior a la hora del hecho es una alerta automática, no un dato.** Es exactamente el patrón que delata que el vale se usó después del robo, o que el robo se declaró más tarde de lo que ocurrió.

**6. Si se perdió el dispositivo portador, el expediente se reconstruye y se declara reconstruido.** El sistema calcula y muestra la **ventana de datos perdida**: desde la última sincronización exitosa hasta la hora del hecho. Lo que se pueda recuperar entra por digitación diferida desde el papel — la hoja de bitácora manual, los tickets que el motorista traía encima, el comprobante de la estación ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)), con constancia de quién digitó y del original escaneado. Lo que no se pueda recuperar **se marca como perdido, no se deja en blanco**. Un campo vacío se lee como "no ocurrió"; un campo marcado como perdido se lee como lo que es.

**7. La misión termina por `T-18` subtipo "retorno sin vehículo".** Motivo obligatorio, expediente de incidente de M-12 vinculado y obligatorio, bitácora cerrada en el punto del hecho. En la **variante B** la misión **no** termina así: continúa, porque el vehículo está operativo. Lo que se registra es un **acta de faltante** contra el inventario de carga, y la entrega en destino se hace **con faltante declarado** — nunca con "recibido conforme".

**8. La carga necesita inventario, no descripción.** Este caso deja al descubierto un vacío: hoy el objeto del traslado se declara como texto. Sin inventario con identificación unitaria — marca, serie o código de bien, cantidad, y quién lo entrega — no hay forma de acreditar qué se perdió. Se levanta como **regla candidata**, no se da por escrita.

**9. Documentación y póliza se congelan a la fecha del hecho.** El expediente muestra si la matrícula, la póliza y la revisión estaban vigentes **el martes a las 19:10**, no el día que alguien consulte ([`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)). Si había póliza vigente, se dispara el recordatorio de aviso al asegurador dentro del plazo contractual, tratado como parámetro con vigencia ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)) y nunca como número fijo.

**10. La misión no cierra limpio.** `H-06` se cumple: pérdida del bien durante la misión sin resolución en M-12. `T-21` no está disponible; el camino es `T-22`, cierre con hallazgo, y el hallazgo **no imputa responsabilidad a nadie** — es marca de seguimiento. La eventual responsabilidad se determina en el expediente de M-12 y por quien corresponda.

### Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:sustraccion-de-bien-en-mision` | La sustracción del vehículo o de parte de la carga durante la misión se registra como evento tipificado con denuncia ante autoridad, mantiene el bien en el registro patrimonial hasta su recuperación o su descargo formal, y nunca lo elimina |
| `RN-c:folio-e-instrumento-de-pago-sustraido` | Los documentos con folio e instrumentos de pago que iban a bordo pasan a estado `SUSTRAIDO`, distinto de anulado; su verificación por QR responde que están sustraídos, y todo uso posterior imputado a la misión es alerta automática |
| `RN-c:ultima-lectura-verificable-de-odometro` | Cuando el vehículo no retorna, el kilometraje se cierra en la última lectura verificable y el tramo posterior queda como no determinado. **Nunca se completa con distancia teórica**, y la conciliación se declara truncada |
| `RN-c:reconstruccion-de-expediente-por-perdida-del-dispositivo` | Perdido el dispositivo portador, el sistema declara la ventana de datos no recuperada, admite digitación diferida desde papel con constancia, y marca como perdido —no como vacío— lo que no se recupera |
| `RN-c:inventario-unitario-de-la-carga` | El objeto del traslado que sea bien inventariable se declara con identificación unitaria, cantidad y responsable de entrega, tanto al despachar como al arribar; la diferencia entre ambos genera acta de faltante |
| `RN-c:entrega-con-faltante-declarado` | Ninguna entrega en destino se registra como conforme si el inventario de arribo difiere del de salida; el faltante se declara y abre expediente en M-12 |

## Escalamiento al PO

`[C]` **¿Quién responde patrimonialmente por el vehículo sustraído bajo custodia de misión?** Insumo nuevo #47. Opciones y costo:

| Opción | Costo |
|---|---|
| El sistema no modela responsabilidad patrimonial: solo registra hechos y deja el expediente a la instancia que corresponda | Es lo consistente con no capturar culpa en campo. Riesgo: la institución esperaba que SIGTI le dijera quién paga, y no se lo va a decir |
| El sistema registra una **determinación de responsabilidad** como resultado del expediente de M-12, con su acto y su autor | Requiere modelar el procedimiento administrativo, que es materia de Talento Humano y de la Gerencia Administrativa, no de transporte |
| Se cablea que el custodio de misión responde siempre | Descartado: no hay norma verificada que lo sostenga, y el motorista asaltado a mano armada no es responsable de nada |

**Recomendación del análisis**, no decisión: la primera, con el gancho de la segunda — el expediente de M-12 admite adjuntar el acto de determinación cuando exista, sin que SIGTI lo produzca.

## Evidencia que debe quedar

Ante una auditoría, encadenado a la misma Orden de Misión:

1. Evento de sustracción con hora del hecho, hora de captura, lugar, subtipo y descripción de lo sustraído
2. Constancia de denuncia con número, autoridad receptora y fecha
3. **Última lectura verificable del odómetro, con su fuente**, y el tramo declarado como no determinado
4. **Correlación entre lo consumido y lo recorrido hasta el punto del hecho** — que es lo que el auditor busca — y la constancia de que nada posterior se imputó a la misión
5. Listado de folios e instrumentos que iban a bordo, con su paso a `SUSTRAIDO` y la hora de la solicitud de bloqueo
6. Inventario de la carga al despacho y acta de faltante, en la variante B
7. Estado de matrícula, póliza y revisión **a la fecha del hecho**
8. Expediente de M-12 con responsable, plazo, gestiones de recuperación y resultado
9. Acta de descargo del bien, si se llegó a ese punto, o la constancia de que el trámite sigue abierto
10. Declaración de la ventana de datos perdida y los originales escaneados de lo digitado en diferido

## Trazabilidad

- **Reglas**: [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) nada se borra · [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) póliza · [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) · [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) custodia · [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) folio y QR · [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) · [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) · [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) · [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) · [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) · [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) · [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) · [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) si iban personas externas
- **Reglas candidatas**: las seis de la sección anterior
- **Transiciones**: `T-18` subtipo retorno sin vehículo · `W-08` incidente o no retorno · `W-14` descargo, solo tras acta · `T-22` cierre con hallazgo
- **Efectos**: `EF-02` folios — requiere el estado `SUSTRAIDO` · `EF-05` conciliación, que aquí se declara truncada · `EF-07` fuente primaria en el dispositivo
- **Criterios de hallazgo**: `H-06` pérdida del bien · `H-04` si el fondo entregado no se comprueba en plazo
- **Puntos de control**: `PC-11` coherencia del odómetro · `PC-16` registro de todo acto de autorización
- **Normativa**: [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) bienes del Estado `[P]` · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) control interno · [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) conectividad · [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md) tag y casetas
- **Actores**: `ACT-06` declara · `ACT-04` coordina · `ACT-07` bloquea vales · `ACT-08` cierra y ejecuta el descargo · `ACT-12` es notificado · `ACT-13` custodio permanente · `ACT-14` responde por el bien
- **Casos especiales relacionados**: `CE-02` avería en ruta · `CE-03` accidente · `CE-07` retorno anticipado · `CE-20` misión cancelada con combustible entregado
- **Insumos nuevos**: #46 destinatarios y plazos del aviso institucional por sustracción de un bien del Estado · #47 régimen de responsabilidad patrimonial del custodio de misión
- **Historias candidatas**: `HU-c:declarar-sustraccion-desde-el-campo`, `HU-c:bloquear-folios-e-instrumentos-de-pago-sustraidos`, `HU-c:reconstruir-expediente-sin-dispositivo`, `HU-c:registrar-acta-de-faltante-de-carga`
