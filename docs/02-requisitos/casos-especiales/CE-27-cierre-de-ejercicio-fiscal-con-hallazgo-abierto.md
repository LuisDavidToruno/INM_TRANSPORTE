# CE-27 — Llega el 31 de diciembre y hay misiones sin cerrar, hallazgos sin resolver y dinero sin devolver

| Campo | Valor |
|---|---|
| **Módulos** | M-13 Liquidación y Cierre, M-09 Combustible, M-14 Auditoría, M-20 Integraciones (ARGOS / SIAFI), M-18 Peajes, M-15 Formatos Oficiales |
| **Estados afectados** | **Todos los no terminales.** En particular `EN_RUTA`, `RETORNADA` y `LIQUIDADA` al momento del corte. También el fondo del período y los rangos de folios ([`EF-02`](../../03-arquitectura/estados/orden-de-mision.md)) |
| **Frecuencia** | **Una vez al año, sin falta.** No es un caso raro: es un evento programado que todavía no tiene dueño en el diseño |
| **Impacto** | Financiero, presupuestario, legal y de auditoría |
| **Resolución** | Definida en su principio. Criterio de imputación contra SIAFI y fecha de corte operativa `[C]` — insumos #8 resuelto vía ARGOS, #16, y confirmación con Gerencia Administrativa |

## La situación

Es el **10 de diciembre**. La Gerencia Administrativa circula el memorando de cierre: *toda liquidación pendiente se presenta antes del 20; las órdenes de pago no utilizadas se devuelven el 22; después del 23 no se emite fondo salvo emergencia autorizada por la máxima autoridad*.

El Jefe de Transporte abre su tablero y esto es lo que tiene:

| Situación | Cuántas | Por qué no cierra |
|---|---|---|
| Misiones en `RETORNADA` sin liquidar | 14 | Faltan comprobantes, o el motorista anda de vacaciones, o los datos de campo no han sincronizado |
| Misiones `EN_RUTA` que cruzan el corte | 3 | Una salió el 29 a Gracias, Lempira, y retorna el 4 de enero. Otra lleva equipo a Puerto Lempira y vuelve por mar |
| Misiones `LIQUIDADA` con criterio `H-nn` disparado | 6 | Cuatro por desviación de consumo, una por circulación en día inhábil sin permiso, una por peaje incoherente |
| Misiones `LIQUIDADA` con expediente vinculado abierto | 2 | Un choque en la CA-5 el 12 de noviembre. El dictamen del ajustador no ha llegado y no va a llegar este año |
| Saldo de fondo sin devolver ni comprobar | **L 18,400** en manos de 5 motoristas | [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md) |
| Folios de vale reservados y no consumidos | 27 | Misiones desprogramadas en noviembre |

Y encima de todo eso: **SIAFI cierra el ejercicio**. Los compromisos no ejecutados se revierten. La cuota del cuarto trimestre muere el 31 y la del primer trimestre del año siguiente todavía no está aprobada. El 2 de enero, cuando el motorista de Gracias cargue combustible en Santa Rosa de Copán, va a estar gastando de un fondo que administrativamente ya no existe.

**Y hay una tentación, y tiene nombre.** Nadie quiere empezar el año arrastrando seis hallazgos y dos investigaciones. La conversación que ocurre en diciembre en toda institución pública es: *"cerremos eso limpio y lo vemos en enero"*.

## Qué se hace hoy sin sistema

`[C]` No verificado con la institución — insumos #1 y #19 (los informes de Auditoría Interna de diciembre y enero son la mejor fuente). Lo que se observa `[I]`:

- **El cierre operativo real ocurre antes del cierre legal.** La institución se autoimpone una fecha de corte — el 20, el 22 de diciembre — que **no está en ninguna norma** y que es la que verdaderamente manda sobre la operación. Nadie la escribió; se transmite por memorando cada año.
- **Los expedientes que no se pueden cerrar se abandonan.** No se marcan, no se listan, no se les asigna responsable: simplemente dejan de mirarse. En marzo nadie recuerda por qué la misión de noviembre sigue en `RETORNADA`. **Ese abandono es el hallazgo**, y es exactamente lo que [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) existe para impedir.
- **Los que sí se cierran, se cierran en lote y con la misma frase.** "Se cierra por fin de ejercicio". Una firma, treinta expedientes, un solo motivo. Ese acto es indistinguible de un cierre sin revisión, porque **es** un cierre sin revisión.
- **El saldo del fondo se cuadra con un ajuste.** `[C]` cómo exactamente — es una de las preguntas que hay que hacer sentado frente a la Gerencia Administrativa, con el libro de caja del año pasado abierto.
- **La misión que cruza el año se imputa completa al ejercicio en que salió, o completa al que la liquidó** — según quién la registre y con qué apuro. Nadie parte el gasto por fecha del hecho.

## Por qué el flujo normal no lo cubre

**El flujo feliz de la Orden de Misión no conoce el calendario.** La máquina de estados va de `BORRADOR` a `CERRADA` sin que en ningún punto aparezca el ejercicio fiscal. Y sin embargo el ejercicio fiscal es real, tiene fecha, y el 31 de diciembre a la medianoche cambia el marco económico de todo lo que esté a medio camino.

Chocan tres cosas que el diseño hasta ahora no puso en la misma mesa:

**1. El expediente vive en el tiempo de la misión; el presupuesto vive en el tiempo del ejercicio.** Una misión de ocho días es una unidad de control administrativo indivisible ([premisa 1](../../../CLAUDE.md)). El presupuesto no la ve así: ve gastos con fecha, y cada fecha cae de un lado o del otro de la medianoche del 31.

**2. `CERRADA` es terminal e inmutable, y el ejercicio empuja a cerrar antes de tiempo.** Cerrar una misión el 22 de diciembre para que no cruce el año, sabiendo que faltan comprobantes, es cerrar mal de forma irreversible: después solo queda el asiento reverso ([§8.3](../../03-arquitectura/estados/orden-de-mision.md)), que es más caro y más confuso que haber esperado.

**3. Los parámetros cambian el 1 de enero.** Tarifas de peaje, precio del combustible, umbrales, cuota trimestral. La misión a Gracias cruza Zambrano el 29 de diciembre con la tarifa de un año y regresa el 4 de enero con la de otro. [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) es tajante: **cada hecho usa la tabla vigente a su propia fecha**. Un sistema que liquide la misión completa con una sola tabla produce un número incorrecto en cualquiera de las dos direcciones que elija.

Y una razón más, que es la que define este caso: **`CERRADA_CON_HALLAZGO` existe precisamente para esto.** [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) lo dice sin adornos — *un sistema que no permite cerrar expedientes imperfectos acumula expedientes abiertos que nadie mira, y los hallazgos quedan invisibles*. Diciembre es el mes en que esa acumulación se vuelve masiva.

## Regla de resolución

### 1. El cierre de ejercicio no cierra misiones

**Ninguna transición de la Orden de Misión se ejecuta por efecto del calendario.** No existe `T-nn` disparada por una fecha. Cerrar es siempre un acto de ACT-08 Gerencia Administrativa sobre un expediente concreto, con su evaluación de `H-01` a `H-08` y su justificación propia ([§7.3](../../03-arquitectura/estados/orden-de-mision.md)).

El cierre de ejercicio es un **corte de imputación**: separa a qué período pertenece cada hecho económico. No es un cierre de expedientes, no es una purga y no es un plazo que mate nada.

Una misión `EN_RUTA` el 31 de diciembre a las 11:59 sigue `EN_RUTA` el 1 de enero a las 00:01. **No hay estado "cerrada por fin de año" y no se va a agregar.**

### 2. El corte tiene dos fechas y las dos son parámetros

| Fecha | Qué es | Nivel |
|---|---|---|
| **Cierre del ejercicio fiscal** | Fin del período presupuestario del Estado, 31 de diciembre | `[P]` — la norma existe ([NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md)); no se extrajo articulado y **no se cita ningún artículo** |
| **Fecha de corte operativo de la institución** | El 20, el 22 de diciembre: el día en que Administración deja de emitir fondo y exige las liquidaciones | `[C]` insumo #1. Hoy vive en un memorando anual |

Las dos son **parámetros con vigencia por rango de fechas** ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)), configurables por ejercicio. Ni el 31 de diciembre ni el 20 de diciembre se escriben en el código. La fecha de corte operativo cambia cada año por memorando, y un sistema que la tenga fija obliga a un despliegue cada diciembre.

### 3. El tablero de cierre se emite con anticipación, no el 30

Con la anticipación configurada — `[C]` insumo #1, la recomendación es no menos de 45 días — el sistema emite el **estado del ejercicio**, dirigido a ACT-08 y a ACT-04, con lo que va a cruzar el corte y **quién responde por cada cosa**:

| Bloque | Contenido | Acción exigida |
|---|---|---|
| Misiones no terminales | Por estado, con antigüedad en días desde el retorno y responsable nominado | Liquidar, cerrar o declarar por qué no se puede |
| Misiones que cruzan el corte por ventana programada | Salida antes y retorno después | Declarar imputación prevista |
| Saldos pendientes de devolución | Por persona, con monto y días fuera de caja ([CE-26](CE-26-sobrante-o-faltante-al-liquidar.md)) | Devolver o abrir obligación de reintegro |
| Folios reservados y no consumidos | Por rango y delegación ([`EF-02`](../../03-arquitectura/estados/orden-de-mision.md)) | Anular con acta |
| Expedientes de hallazgo abiertos | Con criterio, antigüedad, responsable de seguimiento y plazo vencido o no | Resolver o arrastrar declarándolo |
| Expedientes vinculados que bloquean cierre | Incidentes en investigación, reclamos de peaje ante la SAPP, órdenes de trabajo | Declarar el bloqueo con fundamento |
| Compromisos no ejecutados | Fondo aprobado y no asignado, asignado y no consumido | Reversión ([RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md), [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md)) |

**Lo que hace útil a este tablero no es la lista: es el responsable nominado en cada renglón.** Una lista sin nombres es la misma pila de expedientes abandonados, ahora en pantalla.

### 4. La misión que cruza el corte no se parte: se imputa hecho por hecho

**La Orden de Misión es una y sigue siendo una.** No se divide en dos expedientes, no se cierra el 31 y se abre otra el 1. Partirla rompe la unidad de control administrativo-contable y deja dos medias bitácoras que no cuadran con ningún odómetro.

Lo que se reparte entre ejercicios son los **hechos económicos**, cada uno con su fecha del hecho ([RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)) y su tabla paramétrica vigente a esa fecha ([RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)):

- El combustible cargado en Santa Rosa de Copán el **2 de enero** se valora y se imputa con el ejercicio nuevo.
- El paso por Zambrano del **29 de diciembre** usa la tarifa vigente ese día; el del **4 de enero**, la del día 4. **Una sola misión puede usar dos tablas de tarifas, y eso es correcto, no un error de datos.**
- La liquidación consolida ambos y **muestra el desglose por ejercicio**, nunca un total que esconda el corte.

`[C]` **Qué criterio usa SIAFI para el gasto de una misión a caballo** — compromiso a la fecha de la asignación del fondo, o devengo a la fecha del consumo. **No se infiere.** SIGTI es no autoritativo frente a SIAFI ([NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md)) y debe reflejar el criterio que la Gerencia Administrativa aplique, no proponerle uno. Va al PO con dos opciones y su costo:

| Opción | Qué implica | Costo |
|---|---|---|
| Imputar todo al ejercicio del compromiso | Un solo período por misión, más simple de conciliar | El gasto de enero aparece en el año anterior; el arqueo de enero no cuadra contra caja |
| Imputar cada hecho a su fecha | Correcto contra el devengo y contra [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) | Una misión aparece en dos ejercicios; el archivo de conciliación con SIAFI debe soportarlo |

La recomendación de análisis es la segunda, **porque es la única compatible con [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)**, que no es negociable. Pero la decisión es de quien concilia contra SIAFI.

### 5. Lo que puede cerrar, cierra — una por una, con su propio motivo

El sistema puede **agrupar el trabajo** en una pantalla de cierre asistido: presentar juntas las misiones listas, con su evaluación de criterios ya calculada. Lo que no puede es producir **un acto de cierre en lote**.

Cada expediente registra:

- Su propia evaluación de `H-01` a `H-08` con los datos concretos que la sustentan ([§9.2](../../03-arquitectura/estados/orden-de-mision.md))
- Su propia justificación de ACT-08
- Su propia marca de tiempo y su propio eslabón en la cadena de auditoría

**"Se cierra por fin de ejercicio" no es un motivo válido de cierre**, y el catálogo de motivos no lo va a contener. El fin de ejercicio explica *cuándo* se cerró, nunca *por qué* se pudo cerrar.

### 6. Lo que no puede cerrar limpio, cierra con hallazgo — y eso es lo correcto

Las seis misiones con criterio disparado **cierran por `T-22` a `CERRADA_CON_HALLAZGO`**. No hay decisión que tomar: si un criterio de [§7.2](../../03-arquitectura/estados/orden-de-mision.md) se cumple, **`T-21` no está disponible**.

Y aquí va el bloqueo que este caso existe para dejar escrito:

> **El cierre de ejercicio no habilita ninguna excepción.** No se puede desactivar un criterio `H-nn` para una misión concreta ni para un lote de diciembre. La lista de criterios es configurable en sus umbrales y ampliable, **nunca desactivable por caso** ([§7.2](../../03-arquitectura/estados/orden-de-mision.md)). Cambiar un umbral en diciembre para que seis misiones cierren limpias es un acto configurable, sí — y queda registrado con autor, fecha, valor anterior y valor nuevo, y aparece en el reporte de cambios de parámetros que se le entrega al auditor. **Se puede hacer y no se puede esconder.**

Cerrar con hallazgo **no imputa responsabilidad a nadie** ([§7.1](../../03-arquitectura/estados/orden-de-mision.md)). Es lo que permite que el expediente termine y que la observación siga viva en el lugar correcto: el expediente de hallazgo, que es otra entidad con su propio ciclo y sobrevive al cambio de año.

### 7. Lo que no puede cerrar de ninguna forma, se declara

Las dos misiones con el choque de la CA-5 en investigación no cierran: `T-21` exige que no haya expedientes vinculados abiertos que condicionen el resultado, y `H-06` no permite `T-21` de todos modos. Pero **tampoco se abandonan**:

- Quedan en `LIQUIDADA` con una **declaración de permanencia** al cierre del ejercicio: causa tipificada, expediente que la bloquea, responsable de seguimiento y fecha estimada de resolución.
- Entran en el **saldo de apertura de control interno** del ejercicio siguiente: la lista de lo que el año nuevo hereda, con su antigüedad contada desde el hecho original.
- Su antigüedad **no se reinicia con el cambio de año.** Una misión de noviembre que sigue abierta en marzo tiene cuatro meses, no tres.

### 8. El fondo y los folios sí cierran con el ejercicio

- **Fondo:** se ejecuta el arqueo del período ([RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) agregada): aprobado, asignado, comprobado, devuelto y **pendiente**. Los L 18,400 en manos de cinco motoristas **no se dan por devueltos ni se castigan contra el resultado**: se convierten en obligaciones de reintegro nominadas que cruzan el año (`RN-C26b`, candidata en [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md)).
- **Folios:** los 27 reservados y no consumidos se **anulan con acta** al corte, no se arrastran. `[C]` si el correlativo de folio es anual y reinicia con el ejercicio — insumo #2, se responde mirando el talonario del año pasado. **Los folios anulados no se reciclan** ([`EF-02`](../../03-arquitectura/estados/orden-de-mision.md)).
- **Compromisos no ejecutados:** se revierten con asiento, no se borran ([RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)), y quedan en el archivo de conciliación con SIAFI del período ([NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md)).

### 9. Enero también es un caso

El 2 de enero la cuota del primer trimestre **puede no estar aprobada todavía**. El vehículo de Gracias necesita cargar combustible ese día de todos modos.

[RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) valida el compromiso contra la cuota trimestral, y sin cuota vigente el comportamiento es **advertencia → bloqueo, configurable**. `[C]` qué hace la institución en la ventana entre ejercicios: la respuesta honesta es que sigue operando. Si el sistema bloquea, el sistema se sortea; si no advierte, el gasto queda sin control. La recomendación de análisis es **advertencia registrada con responsable nominado durante una ventana configurable de apertura de ejercicio**, y bloqueo al vencerla. Decisión del PO.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C27a` | *El cierre de ejercicio fiscal es un corte de imputación y de reporte; no ejecuta ni habilita ninguna transición de la Orden de Misión. Ningún expediente cambia de estado por efecto de una fecha.* | Ninguna de las 54 reglas menciona el ejercicio fiscal. [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) exige *"manejar el cierre y apertura de ejercicio fiscal"* — **implicación de requerimiento del propio equipo, `[I]`**, no articulado citable. Sin esta regla escrita, la primera implementación va a poner un cierre masivo por fecha porque es lo que resuelve el problema del usuario en diciembre |
| `RN-C27b` | *La Orden de Misión que cruza el corte no se divide. Cada hecho económico se imputa al ejercicio de su fecha del hecho y se valora con la tabla vigente a esa fecha; la liquidación presenta el desglose por ejercicio.* | [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) y [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) lo implican pero **ninguna habla de imputación entre períodos presupuestarios**. El criterio final depende de SIAFI, `[C]` |
| `RN-C27c` | *Todo compromiso no ejecutado al cierre del ejercicio se revierte con asiento; todo folio reservado y no consumido se anula con acta. Ni el compromiso ni el folio se arrastran al ejercicio siguiente.* | [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) controla el compromiso contra la cuota, **no su reversión al cierre**. [`EF-02`](../../03-arquitectura/estados/orden-de-mision.md) modela la anulación de folios por desprogramación, no por fin de ejercicio |
| `RN-C27d` | *Los expedientes no terminales y los hallazgos abiertos al corte constituyen el saldo de apertura de control interno del ejercicio siguiente, con responsable nominado y antigüedad contada desde el hecho original, que no se reinicia con el cambio de ejercicio.* | Es la regla que impide el abandono, y **no existe**. [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) resuelve el expediente individual; nada resuelve el inventario de lo que queda vivo al cambiar de año |

## Evidencia que debe quedar

Lo que la institución le muestra al TSC sobre el ejercicio cerrado:

1. El **acta de cierre de ejercicio** de SIGTI: fecha de corte legal y operativa aplicadas, parámetros vigentes usados, quién la ejecutó y cuándo
2. El **inventario de expedientes no terminales al corte**, por estado, con causa tipificada, responsable nominado y antigüedad — y su contraparte: el **saldo de apertura** del ejercicio siguiente, que debe coincidir renglón por renglón
3. Por cada misión cerrada en la ventana de cierre: su **evaluación individual** de `H-01` a `H-08` con los datos concretos, y la justificación propia de ACT-08. Nunca un motivo compartido por varios expedientes
4. La lista de **misiones que cruzaron el corte** con su desglose de imputación por ejercicio, y las tablas paramétricas usadas para cada hecho — para que el cálculo sea reproducible
5. El **arqueo del fondo del período**: aprobado, asignado, comprobado, devuelto, pendiente por persona, y las obligaciones de reintegro abiertas con su antigüedad
6. El **acta de anulación de folios** no consumidos, por rango y delegación
7. El **reporte de reversión de compromisos** y el archivo de conciliación con SIAFI del período ([NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md))
8. El **registro de cambios de parámetros** en la ventana de cierre: umbrales, plazos, tolerancias. Con autor, fecha, valor anterior y valor nuevo. **Es la evidencia de que nadie aflojó un umbral en diciembre para cerrar limpio**, o de que alguien lo hizo y quedó a la vista
9. Los **expedientes de hallazgo arrastrados**, cada uno con criterio, responsable, plazo y estado — y la demostración de que la misión asociada está en estado terminal aunque el hallazgo siga abierto

## Trazabilidad

- **Autoridad de transiciones:** [`T-19`, `T-21`, `T-22`](../../03-arquitectura/estados/orden-de-mision.md); [§7.1 a §7.4](../../03-arquitectura/estados/orden-de-mision.md) `CERRADA_CON_HALLAZGO` y la prohibición de desactivar criterios por caso; [§8](../../03-arquitectura/estados/orden-de-mision.md) inmutabilidad; [`EF-02`](../../03-arquitectura/estados/orden-de-mision.md) folios; [§10.1](../../03-arquitectura/estados/orden-de-mision.md) fondo; [§11 pendiente 9](../../03-arquitectura/estados/orden-de-mision.md) qué expedientes vinculados impiden cerrar
- **Reglas:** [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) (regla eje del caso), [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md), [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md), [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md)
- **Reglas candidatas:** `RN-C27a`, `RN-C27b`, `RN-C27c`, `RN-C27d` — ninguna escrita
- **Puntos de control:** `PC-13` (quien cierra ≠ quien liquidó), `PC-15` (misiones anteriores sin liquidar), `PC-18` (actos por emergencia pendientes de convalidación bloquean el cierre), de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) — cuotas trimestrales de compromiso `[V]`; cierre y apertura de ejercicio, misiones que cruzan el 31 de diciembre y reversión de compromisos son **implicaciones de requerimiento del equipo**, `[I]`; el ejercicio fiscal como período legal, `[P]`, sin cita de articulado. [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-14 conciliación periódica y V-10 registro oportuno `[V]`
- **Actores:** ACT-08 Gerencia Administrativa (ejecuta el cierre y cada `T-21` / `T-22`), ACT-04 Jefe de Transporte, ACT-07 Encargado de Combustible, ACT-10 Encargado de Delegación, ACT-12 Auditor Interno, ACT-16 Sistema ARGOS
- **Casos relacionados:** [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md), [CE-28](CE-28-hallazgo-posterior-sobre-mision-cerrada.md), [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md)
- **Insumos:** #1 (memorando de cierre del año pasado: contiene la fecha de corte operativa real y los plazos que nadie escribió como norma), #2 (¿el correlativo de folios es anual?), #16 (contrato de ARGOS: cómo se recibe la cuota trimestral y el estado del ejercicio), #19 (informes de auditoría de enero), #32 (plazos y umbrales). **Nuevo — llevar al PO:** criterio de imputación de la misión a caballo contra SIAFI, y comportamiento en la ventana entre ejercicios sin cuota aprobada
