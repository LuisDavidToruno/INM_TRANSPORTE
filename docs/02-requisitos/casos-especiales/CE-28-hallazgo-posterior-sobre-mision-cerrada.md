# CE-28 — En noviembre aparece un hallazgo sobre una misión que se cerró en marzo

| Campo | Valor |
|---|---|
| **Módulos** | M-14 Reportes, Indicadores y Auditoría, M-12 Incidentes y Sanciones, M-13 Liquidación y Cierre, M-09 Combustible, M-18 Peajes, M-15 Formatos Oficiales |
| **Estados afectados** | `CERRADA` y `CERRADA_CON_HALLAZGO`. También `ANULADA` y `RECHAZADA`: los cuatro terminales admiten hallazgo posterior |
| **Frecuencia** | Ocasional por misión, **permanente como fenómeno** — siempre hay algo que se descubre después |
| **Impacto** | Legal, financiero y de auditoría. Es el caso donde se juega la credibilidad completa del expediente |
| **Resolución** | **Definida y cerrada. La misión no se reabre.** Contradicción detectada con [actores-y-roles.md](../../01-negocio/actores-y-roles.md) — ver sección final |

## La situación

Es el **14 de noviembre**. Auditoría Interna está revisando la muestra del primer semestre. Y aparecen tres cosas distintas, en tres días distintos.

### El comprobante que se usó dos veces

`OM-2026-00318` — Tegucigalpa → Choluteca → San Lorenzo, pickup `INS-PU-009`, tres días de marzo. Se liquidó el 2 de abril y **se cerró limpia** el 3 de abril: el consumo cuadró, el rendimiento dio 10.8 km/galón contra 11 esperados, todo dentro de umbral. Nadie tuvo nada que decir.

Al conciliar el estado de cuenta del proveedor contra los comprobantes registrados, aparece que la factura de la estación de San Lorenzo — **L 2,340, 31 galones** — está adjunta también a `OM-2026-00341`, de la delegación de Choluteca, cerrada el 11 de abril.

**El mismo papel sostiene dos consumos en dos expedientes de dos delegaciones distintas.** Uno de los dos consumos no existió. Los dos expedientes están cerrados, sellados e inmutables desde hace siete meses.

### La multa que llegó tarde

En **agosto** llega la notificación: infracción por exceso de velocidad en la CA-5, a la altura de Jícaro Galán, el **19 de marzo a las 14:20**. Vehículo `INS-PU-009`. La misión de ese día es `OM-2026-00318`, cerrada en abril. Hay un motorista con nombre conduciendo a esa hora en ese punto, y hay una multa a nombre de la institución que alguien tiene que pagar.

`H-06` — *incidente, siniestro, multa o pérdida ocurrido durante la misión y aún sin resolución* — se habría cumplido en abril **si el dato hubiera existido en abril**. No existía. Las multas de tránsito llegan meses después, y esa es su naturaleza, no una falla del proceso.

### El paso por caseta que no pertenece a ninguna misión

El estado de cuenta del peaje del período trae **dos cruces por Zambrano un domingo de mayo**, vehículo `INS-PU-012`. Ninguna Orden de Misión de ese fin de semana declara ese vehículo. Ninguna orden de misión, punto.

Este hallazgo **no tiene misión a la cual colgarse**. Es un hallazgo sobre el vehículo y sobre un período. Si el diseño solo sabe vincular observaciones a misiones, este caso — que es uso indebido de un bien del Estado, el hallazgo más serio de los tres — **no tiene dónde registrarse**.

## Qué se hace hoy sin sistema

`[C]` No verificado — insumos #1 y #19. Lo observado en instituciones comparables `[I]`:

- **Se corrige el papel.** Se busca el expediente físico, se saca la factura duplicada, se mete una nota, a veces se rehace la hoja de liquidación con la fecha original. **El expediente termina diciendo algo distinto de lo que decía cuando se cerró, y no queda rastro del cambio.** Ese es el peor resultado posible y es el más frecuente.
- **O no se corrige nada.** Se responde el oficio de auditoría con un descargo escrito, el descargo queda en el archivo de Auditoría Interna, y el expediente de la misión sigue diciendo lo mismo de siempre. **El hallazgo y el expediente viven en dos archivos que nunca se tocan.**
- **La multa se paga y se archiva aparte.** Casi nunca se vincula a la misión ni al motorista que conducía. Lo que se pierde con eso no es el dinero: es que **nadie puede ver que el mismo motorista acumuló cuatro multas en el año**, porque las cuatro están en cuatro folders distintos ordenados por fecha de pago.
- **El paso por caseta sin misión no se detecta**, porque nadie concilia el estado de cuenta del peaje contra las órdenes de misión. Si se detecta, es por casualidad.

**La regla que nadie escribió**: hoy el descubrimiento tardío no tiene procedimiento. Depende de si el auditor insiste, y de si el jefe de la dependencia decide que vale la pena.

## Por qué el flujo normal no lo cubre

Porque el flujo normal **termina**. `CERRADA` es terminal: desde ahí no sale ninguna transición ([§8.1](../../03-arquitectura/estados/orden-de-mision.md)), el expediente es inmutable hasta la errata de un campo de texto ([§8.2](../../03-arquitectura/estados/orden-de-mision.md)), y la cadena de auditoría está sellada.

Todo lo que el flujo feliz sabe hacer con un hallazgo — evaluarlo al liquidar, proponer la clasificación, cerrar por `T-22` — **solo funciona antes del cierre**. Después del cierre no hay transición, no hay liquidación que rehacer y no hay dónde poner el dato nuevo.

Y la salida cómoda, la que se va a proponer en la primera reunión de diseño, es **reabrir el expediente**. Hay que responderla antes de que se proponga:

> **`CERRADA` no se reabre. Ni por auditoría, ni por la máxima autoridad, ni por error propio.**
>
> La razón está escrita en [§7.5](../../03-arquitectura/estados/orden-de-mision.md) y es dura y deliberada: *si un estado terminal puede cambiar meses después, entonces ningún reporte histórico es reproducible, y un reporte no reproducible no sirve para rendir cuentas.*
>
> El reporte de ejecución del primer semestre que la institución entregó en julio tiene que poder volver a producirse en 2029, idéntico, con los mismos números. Si el expediente de marzo puede cambiar en noviembre, el reporte de julio deja de ser verificable — y con él, todos los demás. **Se pierde más de lo que se gana: se gana corregir un expediente y se pierde la confiabilidad de todo el archivo.**

## Regla de resolución

### 1. Lo que se abre es un expediente nuevo, no el expediente viejo

El descubrimiento tardío crea un **expediente de hallazgo posterior**: entidad propia de M-12 y M-14, con su ciclo de vida, su responsable y su resolución. Vive al lado del expediente cerrado; nunca dentro de él.

| Campo | Contenido |
|---|---|
| Origen del descubrimiento | Conciliación externa automática · revisión de Auditoría Interna · notificación de tercero · denuncia · hallazgo del TSC |
| Quién lo descubre y cuándo | Persona, puesto, fecha del descubrimiento |
| **Fecha del hecho original** | La del hecho observado — 19 de marzo — **distinta de la fecha del descubrimiento** ([RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)) |
| Criterio | Del catálogo `H-01` a `H-08` cuando aplique, o tipificación propia del hallazgo posterior |
| Objetos vinculados | Cero, una o **varias** misiones; vehículo; motorista; fondo; período |
| Fundamento documental | Adjunto obligatorio: el estado de cuenta, la notificación, el acta |
| Efecto económico | Monto y signo, si lo tiene |
| Responsable de seguimiento y plazo | Nominado, no un rol |
| Resolución | Con su fecha, su acto y su fundamento |

**Puede vincular más de una misión.** El comprobante duplicado toca dos expedientes de dos delegaciones: es **un** hallazgo con **dos** misiones vinculadas, y el asiento reverso se asienta solo sobre aquella donde el consumo no ocurrió. Un modelo que obligue a un hallazgo por misión duplica el expediente y pierde la relación, que es justamente la evidencia.

**Puede no vincular ninguna.** El paso por Zambrano del domingo se imputa al **expediente del vehículo** y al período. Es uso del bien fuera de misión autorizada, y es el hallazgo más grave de los tres.

### 2. El estado de la misión no cambia. Ni a `CERRADA_CON_HALLAZGO`

Esta es la tentación fina, y también se responde que no. `CERRADA` no pasa a `CERRADA_CON_HALLAZGO` por un descubrimiento posterior. **No existe esa transición y no se va a agregar**: `CERRADA_CON_HALLAZGO` significa *el criterio se evaluó al cerrar y se cumplía entonces*, y reescribirlo en noviembre destruye esa afirmación para todos los expedientes, no solo para este.

Lo que sí cambia es lo que el sistema **muestra junto a** la misión: un indicador visible de que tiene hallazgos posteriores vinculados, con su cantidad y su estado. Es un **dato derivado de otra entidad**, no un atributo del expediente cerrado, y por eso no viola [§8.2](../../03-arquitectura/estados/orden-de-mision.md).

Toda vista, exportación y paquete de evidencia de esa misión lo presenta desde entonces. **El expediente cerrado muestra el hallazgo, no lo esconde** — el mismo principio de [§8.3](../../03-arquitectura/estados/orden-de-mision.md) para los reversos.

### 3. El efecto económico se corrige con asiento reverso, con todas sus precondiciones

Los L 2,340 del consumo que no existió se revierten según [§8.3](../../03-arquitectura/estados/orden-de-mision.md) y [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), sin atajos:

- **Se revierte un asiento concreto**, identificado. *No existe el reverso genérico "de la misión"*.
- Lo autoriza **ACT-08 Gerencia Administrativa** o lo requiere **ACT-12 Auditor Interno**.
- **`BD-06`: quien autoriza el reverso no puede ser quien produjo el asiento revertido.** Siete meses después, con rotación de personal de por medio, esto se verifica contra **la identidad de la persona registrada en el asiento original**, no contra el titular actual del puesto ([§9.1](../../03-arquitectura/estados/orden-de-mision.md): el rol ejercido se guarda como copia, no como referencia).
- Motivo tipificado con fundamento documental adjunto; valor anterior y valor nuevo, ambos siempre.
- **El efecto va a los acumulados del período en que se registra el reverso — noviembre —, no a los de marzo.** El cierre del primer semestre ya publicado no se toca.
- Si además hay dinero que alguien debe devolver, se abre la **obligación de reintegro** nominada (`RN-C26b`, candidata en [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md)). El reverso corrige el registro; no cobra nada por sí solo.

Y lo que corresponde al documento impreso: la factura duplicada **no se despega ni se sustituye en el expediente**. Se registra la anulación del documento con referencia cruzada, y si procede se emite uno nuevo con **folio nuevo** que declara en su cuerpo *"sustituye al folio X"*. **Ambos se conservan y ambos se imprimen si se piden** ([§8.3](../../03-arquitectura/estados/orden-de-mision.md)).

### 4. Los indicadores se recalculan mostrando el ajuste, nunca en silencio

El rendimiento de `INS-PU-009` en marzo cambia: sin esos 31 galones fantasma, la misión no dio 10.8 km/galón sino cerca de **17** — que para un pickup es el otro extremo del umbral y habría disparado `H-01` en abril si el dato hubiera sido correcto.

Los acumulados del vehículo, del motorista y de la dependencia **se recalculan presentando el ajuste como tal** ([§8.3](../../03-arquitectura/estados/orden-de-mision.md)): valor original, reverso, valor resultante, con su cadena. Nunca solo el resultado.

**Y aquí aparece el requisito que sostiene todo lo anterior: la fecha de corte de conocimiento.** Todo reporte declara a qué fecha de conocimiento está construido, y puede producirse en dos modalidades:

| Modalidad | Qué muestra | Para qué sirve |
|---|---|---|
| **Como se conocía al corte** | El estado del dato a una fecha pasada | Reproducir el reporte de julio, idéntico, en 2029 |
| **Con hallazgos posteriores** | Lo anterior más los ajustes conocidos hasta hoy, identificados como capa aparte | Responderle al auditor hoy |

Sin esto, cualquier corrección posterior destruye la reproducibilidad, que es exactamente lo que la regla de no reabrir estaba protegiendo. **De nada sirve no reabrir el expediente si el reporte cambia igual.**

### 5. El descubrimiento tardío se programa, no se espera

Los tres casos aparecieron **porque alguien cruzó dos listas a mano en noviembre**. Un control que depende de eso funciona una vez al año, si el auditor tiene tiempo.

El sistema ejecuta **conciliaciones externas periódicas** como proceso propio, con su periodicidad configurable, y cada diferencia abre expediente de hallazgo posterior de forma automática:

| Fuente externa | Contra qué se cruza | Qué detecta |
|---|---|---|
| Estado de cuenta del proveedor de combustible | Comprobantes registrados por folio y monto | Comprobante duplicado, consumo no registrado, monto alterado |
| Estado de cuenta de peaje o CoviPass | Pasos declarados por misión | Circulación sin misión, paso no declarado ([RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)) — `[C]` insumo #24 |
| Notificaciones de infracción | Vehículo, fecha y hora contra misiones | Multa atribuible a una misión y a un motorista |
| Dictámenes de seguro y talleres | Expedientes de incidente | Resolución tardía de un `H-06` |

**El duplicado de San Lorenzo se habría detectado en abril, no en noviembre**, si el comprobante fuera único a nivel de institución al momento de registrarlo. Esa es la regla candidata `RN-C28d`, y es la más barata de las cuatro.

### 6. Cuánto tiempo hacia atrás — y la respuesta honesta

SIGTI **no impone su propio plazo** para abrir un hallazgo posterior. Los plazos de prescripción de responsabilidad administrativa, civil y penal son distintos entre sí y no los decide un sistema de transporte.

Lo que hace el sistema: al abrir un expediente de hallazgo posterior calcula la antigüedad del hecho y **marca de forma informativa** si excede el plazo de prescripción parametrizado ([§8.4](../../03-arquitectura/estados/orden-de-mision.md), `[C]` con Auditoría Interna). Informativo, **no bloqueante**. Un sistema que se niegue a registrar una observación por antigua está destruyendo evidencia, aunque lo haga con buena intención.

Y su límite real es la **retención**: vencido el plazo de conservación, la depuración es un acto autorizado, con acta y constancia de qué se depuró ([§8.4](../../03-arquitectura/estados/orden-de-mision.md)). Nunca automática.

### 7. Contradicción detectada — no se resuelve en silencio

> **Hallazgo.** [`actores-y-roles.md`, tabla de anulación y corrección](../../01-negocio/actores-y-roles.md) contiene la fila:
>
> | Orden de Misión | `CERRADA` | Nadie | *Reapertura excepcional por `ACT-09`*, notificada a `ACT-12` `[C]` |
>
> **Esa reapertura no existe.** La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) es la autoridad en transiciones, precondiciones e invariantes por la [precedencia entre artefactos](../../../CLAUDE.md), y dice en [§8.1](../../03-arquitectura/estados/orden-de-mision.md): *desde los terminales no sale ninguna transición. Nunca.* Y en [§7.5](../../03-arquitectura/estados/orden-de-mision.md): *la misión no se reabre y no cambia de estado*.
>
> **Corresponde corregir `actores-y-roles.md`**, que no es autoridad en esta materia. La reapertura excepcional por la máxima autoridad es exactamente el mecanismo que vacía la inmutabilidad: basta con que exista para que se use, y basta con que se use una vez para que ningún reporte histórico vuelva a ser defendible. Este caso especial se escribe asumiendo que **la reapertura no existe**.

Y una segunda cuestión, que **no se resuelve aquí porque la autoridad es otro documento**:

> `[C]` **¿Puede ACT-12 Auditor Interno abrir por sí mismo un expediente de hallazgo posterior?**
>
> `I-12` establece incompatibilidad absoluta entre ACT-12 y *cualquier rol ejecutor*, y `PC-17` de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) dice que **solo lee y exporta**. Pero un auditor que necesita pedirle al auditado que registre su propia observación no es un auditor independiente.
>
> La lectura de análisis es que **abrir el expediente de hallazgo posterior es un acto de auditoría, no una transacción de negocio**: no altera el expediente de la misión, no mueve dinero y no ejecuta ninguna `T-nn`. El **asiento reverso** sí es acto de negocio, y ese lo autoriza ACT-08 o lo requiere ACT-12 — como ya está previsto en [§8.3](../../03-arquitectura/estados/orden-de-mision.md). Pero **la autoridad en incompatibilidades es [actores-y-roles.md](../../01-negocio/actores-y-roles.md)**, y le corresponde a ese documento decirlo. Escalado al PO.

### 8. Lo que no se hace, aunque lo pida quien lo pida

- **No se reabre la misión.** Ni por oficio del TSC, ni por instrucción de la máxima autoridad, ni por error propio del sistema.
- **No se cambia `CERRADA` por `CERRADA_CON_HALLAZGO`** de forma retroactiva.
- **No se edita ni un dígito** del expediente cerrado: ni el odómetro, ni el monto, ni la fecha, ni una errata ([§8.2](../../03-arquitectura/estados/orden-de-mision.md)).
- **No se reemite un documento con el mismo folio y contenido distinto.**
- **No se recalculan los históricos ya publicados** para "dejarlos correctos". Se ajusta el período corriente y se muestra el ajuste.
- **No se cierra el expediente de hallazgo posterior sin resolución**, y su antigüedad se cuenta desde el hecho original, no desde el descubrimiento.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C28a` | *El expediente de hallazgo posterior es entidad con ciclo propio; puede vincular cero, una o varias misiones terminales, un vehículo, un motorista o un período; y ni su apertura ni su resolución alteran el estado ni los datos del objeto vinculado.* | [§7.5](../../03-arquitectura/estados/orden-de-mision.md) y [§8.2](../../03-arquitectura/estados/orden-de-mision.md) lo describen como efecto de la inmutabilidad, **pero ninguna de las 54 reglas lo enuncia**, y ninguna contempla el hallazgo sin misión vinculable — el paso por Zambrano del domingo |
| `RN-C28b` | *Todo reporte declara su fecha de corte de conocimiento y es reproducible a esa fecha. Los ajustes posteriores se presentan como capa identificada, nunca fundidos en el dato histórico.* | Es la condición que hace verdadera la promesa de [§7.5](../../03-arquitectura/estados/orden-de-mision.md) — *ningún reporte histórico sería reproducible*. **Nada la exige hoy**, y sin ella no reabrir el expediente no sirve de nada |
| `RN-C28c` | *El sistema ejecuta conciliaciones externas periódicas contra estados de cuenta de combustible, peaje, notificaciones de infracción y dictámenes; cada diferencia abre expediente de hallazgo posterior de forma automática.* | [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-14 exige conciliación periódica de registros `[V]`, y [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) solo concilia **hacia adentro**, galones contra kilómetros. **Nada concilia contra una fuente externa**, que es de donde vinieron los tres casos |
| `RN-C28d` | *Todo comprobante de consumo es único en la institución por emisor y número; su reutilización se bloquea al registrar y se detecta al conciliar, atravesando dependencias y delegaciones.* | [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) exige galones, monto, estación, odómetro y fotografía — **no exige que el comprobante sea único**. Es lo que permite que el mismo papel sostenga dos consumos en dos delegaciones. La detección debe cruzar el alcance de datos: dos delegaciones no se ven entre sí |
| `RN-C28e` | *Toda infracción de tránsito notificada se atribuye al vehículo y, cuando la fecha y hora caen dentro de una misión ejecutada, al motorista que la conducía, con acumulado por persona y por vehículo.* | M-12 cubre multas, pero **ninguna regla ata la multa notificada tarde a la misión y al conductor de ese momento**. Sin eso, cuatro multas del mismo motorista siguen siendo cuatro papeles sueltos |

## Evidencia que debe quedar

Cuando el TSC pregunte por `OM-2026-00318` en 2029, la institución debe poder entregar, encadenado:

1. El **paquete de evidencia de la misión tal como cerró en abril**, con su cadena sellada y sus hashes verificables — idéntico al que se pudo exportar el 4 de abril
2. El **expediente de hallazgo posterior**: quién lo descubrió, cómo, cuándo, contra qué fuente, con el estado de cuenta del proveedor adjunto
3. La **fecha del hecho original** y la **fecha del descubrimiento**, distintas y ambas registradas
4. El **asiento reverso** con referencia al asiento concreto revertido, valor anterior y valor nuevo, motivo tipificado, fundamento, autor, autorizador, y la verificación de `BD-06` contra la identidad del autor original
5. La **anulación del documento** duplicado con referencia cruzada, y el sustituto con folio nuevo si lo hubo — **con ambos conservados**
6. Los **indicadores del vehículo antes y después del ajuste**, con el ajuste identificado y el período al que se imputó
7. Las **dos versiones del reporte**: la del corte de julio, reproducida idéntica, y la actual con hallazgos posteriores — con la diferencia explicada
8. La **obligación de reintegro**, si la hubo: monto, responsable nominado, notificación, descargo, resolución
9. El **eslabón adicional en la cadena de auditoría** de la misión, encadenado al último y con el sello reabierto ([§8.3](../../03-arquitectura/estados/orden-de-mision.md))
10. Y el estado de la misión, que sigue siendo el mismo del 3 de abril: **`CERRADA`**

Esa última línea es la que hace defendible todo el resto. La institución no está diciendo *"nos equivocamos y lo arreglamos"*: está diciendo *"esto es lo que sabíamos entonces, esto es lo que supimos después, y aquí está cada paso entre una cosa y la otra"*.

## Trazabilidad

- **Autoridad de transiciones:** [§7.5](../../03-arquitectura/estados/orden-de-mision.md) hallazgo posterior al cierre, [§8.1](../../03-arquitectura/estados/orden-de-mision.md) terminales, [§8.2](../../03-arquitectura/estados/orden-de-mision.md) qué significa inmutable, [§8.3](../../03-arquitectura/estados/orden-de-mision.md) cómo se hace un asiento reverso, [§8.4](../../03-arquitectura/estados/orden-de-mision.md) retención y depuración, [§9.1 y §9.3](../../03-arquitectura/estados/orden-de-mision.md) registro y encadenamiento, [§7.2](../../03-arquitectura/estados/orden-de-mision.md) criterios `H-01` a `H-08`
- **Reglas:** [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) (regla eje), [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md), [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md), [RN-06](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md), [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)
- **Reglas candidatas:** `RN-C28a` a `RN-C28e` — ninguna escrita. `RN-C26b` (obligación de reintegro) de [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md)
- **Puntos de control:** `PC-13` (quien cierra ≠ quien liquidó), `PC-16` (registro de todo acto de autorización), `PC-17` (ACT-12 solo lee y exporta — **cuestionado en la sección 7**), de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-14 conciliación periódica `[V]`; pista append-only, asiento reverso y paquete de evidencia son **implicaciones de requerimiento del equipo**, `[I]`; plazo de retención `[C]`. [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) — conciliación de peajes. [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) — uso del bien del Estado fuera de misión autorizada
- **Contradicción detectada:** [actores-y-roles.md](../../01-negocio/actores-y-roles.md), fila *reapertura excepcional de misión `CERRADA` por `ACT-09`* — contradice [§8.1](../../03-arquitectura/estados/orden-de-mision.md). La máquina de estados es la autoridad. **Corresponde corregir `actores-y-roles.md`**
- **Actores:** ACT-12 Auditor Interno (descubre y requiere), ACT-08 Gerencia Administrativa (autoriza reversos), ACT-09 Máxima Autoridad, ACT-04 Jefe de Transporte, ACT-11, ACT-14
- **Casos relacionados:** [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md), [CE-27](CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md), [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md), [CE-03](CE-03-accidente-de-transito-en-mision.md)
- **Insumos:** #1 (plazo de prescripción y procedimiento de deducción de responsabilidad, con Auditoría Interna), #19 (informes de auditoría: **cada hallazgo pasado es un caso de descubrimiento tardío ya ocurrido**), #24 (estado de cuenta de peaje: ¿COVI-H emite estado de cuenta empresarial?, sin él la conciliación de peaje no existe). **Nuevo — llevar al PO:** ¿el proveedor de combustible entrega estado de cuenta consolidado por institución?; ¿por qué canal llegan las notificaciones de infracción de la DNVT y a quién?
