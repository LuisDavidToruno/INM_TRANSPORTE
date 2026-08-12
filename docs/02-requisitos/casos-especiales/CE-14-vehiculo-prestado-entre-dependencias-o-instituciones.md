# CE-14 — "Préstenos el microbús dos semanas": el vehículo se va y sigue siendo responsabilidad nuestra

| Campo | Valor |
|---|---|
| **Módulos** | M-03 Flota, M-04 Documentación y Cumplimiento, M-01 Organización y Alcance de Datos, M-12 Incidentes y Sanciones, M-09 Combustible, M-18 Peajes, M-14 Auditoría, M-15 Formatos Oficiales |
| **Estados afectados** | Estado operativo del vehículo: `DISPONIBLE` → `NO_DISPONIBLE`. Bloquea `T-08` y `T-12` de toda orden que lo involucre |
| **Frecuencia** | Frecuente el préstamo entre dependencias. Ocasional el interinstitucional, y es el de mayor riesgo |
| **Impacto** | Auditoría, legal y patrimonial |
| **Resolución** | Definida. **Requiere causa tipificada nueva en `W-11`** y **tres decisiones escaladas al PO** |

> **Bajo un mismo nombre hay dos casos distintos.** El préstamo **entre dependencias de la misma institución** es un movimiento interno: el vehículo sigue dentro de SIGTI y sigue operando bajo Órdenes de Misión. El préstamo **a otra institución** saca el vehículo del control operativo de SIGTI **sin sacarlo de la responsabilidad de la institución ante el TSC**. Son problemas diferentes y se resuelven diferente.

## La situación

### A · Entre dependencias

El microbús de 15 pasajeros de la Delegación de Choluteca, correlativo institucional **042**, odómetro **187,220**. La Delegación de Nacaome tiene el suyo en taller esperando repuestos desde hace tres semanas y tiene jornadas comprometidas en San Lorenzo, Amapala y Alianza.

El Jefe de Transporte resuelve por teléfono: "que se lo lleven dos semanas". El motorista de Nacaome llega el lunes, recibe las llaves, y se va.

Desde ese momento, en el sistema de Choluteca ese microbús aparece **disponible**. Alguien lo va a programar para el jueves y el jueves el microbús está en Amapala.

### B · Entre instituciones

La Secretaría de Salud está haciendo una jornada de vacunación en Marcovia y Namasigüe y no tiene con qué mover brigadas. Pide prestado el mismo microbús **por cinco días, con su propio motorista**. Hay una nota firmada y hay un compromiso verbal de "devolverlo lleno".

Sale el lunes con odómetro **187,410**. Vuelve el lunes siguiente —dos días tarde— con odómetro **189,050**: **1,640 kilómetros** que nadie de la institución recorrió, sin una sola bitácora, sin un solo comprobante de combustible, con dos pasos por caseta que no están en ningún expediente.

Y trae dos cosas más: un rayón nuevo en la puerta corrediza, y **una boleta de infracción de la DNVT de un sábado**, con el vehículo rotulado *"PROPIEDAD DEL ESTADO DE HONDURAS"* y las siglas de **nuestra** institución en las puertas.

## Qué se hace hoy sin sistema

**Entre dependencias**: se resuelve por teléfono o por WhatsApp. A veces hay memorándum, casi nunca hay acta con odómetro. El vehículo desaparece del control de la delegación de origen y aparece en el de la receptora sin ningún registro que una las dos cosas.

**Entre instituciones**: hay una nota de solicitud y, si el Jefe de Transporte es cuidadoso, una respuesta autorizando. El acta de entrega, cuando existe, dice "en buen estado" y no dice odómetro.

Las prácticas no escritas, que son las que importan:

1. **Nadie apunta el odómetro al entregar.** Es la única cosa que después no se puede reconstruir, y es la única que decide si alguien recorrió 1,640 km de la institución o de su bolsillo.
2. **El préstamo "de dos semanas" no tiene mecanismo de vencimiento.** Hay unidades prestadas hace años que ya nadie recuerda que son de otra institución. **Es un hallazgo clásico del TSC y no requiere mala fe: requiere que nadie tenga una fecha en la mano.**
3. **La multa y el daño se discuten después, entre jefes.** Sin acta con estado y fotografías del momento de la entrega, la discusión no tiene sobre qué apoyarse y la deducción de responsabilidad se cae.
4. `[C]` **Quién autoriza cada tipo de préstamo.** [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) menciona el préstamo interinstitucional dentro del ciclo del bien y exige acta y resolución, pero **eso es una implicación de requerimiento redactada por el equipo, no el articulado** — nivel `[I]`. El articulado del *Manual de Propiedad Estatal* no se pudo extraer `[P]`. **No se inventa aquí quién firma.**

## Por qué el flujo normal no lo cubre

El modelo entero de SIGTI asume que **cada movimiento del vehículo es una Orden de Misión**. El préstamo no lo es: no tiene objeto del traslado, ni ruta, ni motorista propio, ni liquidación. Es un acto **sobre el bien**, no sobre una movilización. Meterlo a la fuerza en el ciclo `BORRADOR → … → CERRADA` produce un expediente que miente en todos sus campos.

Y el estado operativo del vehículo tampoco lo tiene resuelto. [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) reconoce el caso —*"vehículo prestado a otra dependencia… el estado se acompaña de la dependencia tenedora"*— y [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) dice que *"es una asignación que ocupa ventana igual que una misión"*. Pero **`NO_DISPONIBLE` exige causa tipificada** y su enumeración vigente no incluye el préstamo. Sin tipificación, este estado se convierte en el cementerio donde se esconde la flota.

## Regla de resolución

### 1. Primero, la pregunta que separa los dos casos: ¿va con motorista nuestro?

| Situación | Cómo se modela |
|---|---|
| Va **con motorista de la institución**, a apoyar a otra dependencia o institución | **Es una Orden de Misión**, con motivo de viaje *apoyo institucional* y la entidad beneficiaria como dato. No es préstamo del bien: el vehículo y el motorista siguen bajo nuestro control, nuestra bitácora, nuestro fondo y nuestra liquidación. **Se prefiere siempre esta forma** |
| Va **sin motorista nuestro**: lo conduce personal ajeno | **Es un préstamo del bien.** Sale del control operativo de SIGTI. Todo lo que sigue en este documento aplica a este caso |

Esta distinción es la mitad de la solución. Un préstamo con motorista propio disfrazado de "préstamo" es lo que borra la bitácora de 1,640 kilómetros.

### 2. El expediente de préstamo, un objeto de M-03 con vida propia

No es una misión, no es un mantenimiento: es un registro nuevo sobre el vehículo, con:

- **Tipo**: entre dependencias · entre instituciones
- **Acto que lo autoriza**: quién, con qué cargo, en qué documento, con folio y adjunto `[C]` el nivel competente
- **Entidad o dependencia receptora** y **persona responsable receptora** nombrada, con cargo e identificación
- **Ventana comprometida**: fecha de entrega y **fecha de devolución comprometida** — obligatoria, sin excepción
- **Propósito declarado**
- **Acta de entrega** con: odómetro fotografiado, nivel de combustible, llantas incluida la de repuesto, gato, triángulos, herramienta, documentos entregados (matrícula, póliza, tarjeta de circulación), **estado de rotulación con fotografía**, y daños preexistentes fotografiados
- **Quién asume**: combustible, peajes, mantenimiento, multas y daños — ver decisión D-2
- **Acta de devolución**, con los mismos elementos

Al abrirse, la custodia se traslada al responsable receptor con constancia — [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) ya lo cubre expresamente para el préstamo entre dependencias y para el traslado a un tercero. **En ningún momento el vehículo queda sin custodio identificado.**

### 3. Préstamo entre dependencias: el vehículo no se va del sistema, se va de la agenda

El vehículo **sigue `DISPONIBLE`**, pero **para la dependencia tenedora, no para la de origen** ([`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md)). Concretamente:

- El préstamo **ocupa ventana en la agenda del vehículo** igual que una misión ([`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md)): Choluteca no lo puede programar para el jueves porque el sistema ya sabe que está en Nacaome.
- El **alcance de datos** de M-01 se amplía temporalmente: Nacaome ve, programa y despacha ese vehículo **durante la ventana del préstamo y solo durante ella**. Al vencer, el alcance se retrae solo.
- **La bitácora, el fondo y la liquidación siguen siendo de SIGTI**, ejecutados por la dependencia tenedora. **No se pierde ni un kilómetro.**
- El **custodio permanente** (`ACT-13`) puede cambiar o no, pero el cambio, si ocurre, es con acta.

Este caso, bien modelado, deja de ser un problema. El difícil es el otro.

### 4. Préstamo entre instituciones: el vehículo sale del control, no de la responsabilidad

**Estado del vehículo: `NO_DISPONIBLE` con causa tipificada `PRESTADO_A_OTRA_INSTITUCION`** — causa que hoy no existe en la enumeración y que hay que agregar; ver el hallazgo. No es asignable a ninguna misión propia, y ese es el punto.

Cinco consecuencias que el sistema tiene que sostener:

**a) El kilometraje bajo tenencia ajena se registra como tal y no contamina el rendimiento.** El delta entre odómetro de entrega y de devolución —**1,640 km**— se asienta como *kilometraje bajo tenencia de tercero*, con las dos lecturas y sus fotografías. **Nunca entra en la conciliación galonaje–kilometraje** de [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md): promediar kilómetros que otra institución recorrió con combustible que no fue nuestro produce un rendimiento inventado, y peor, produce un rendimiento inventado que después dispara alertas falsas para siempre. Es el mismo criterio de imputación por tramo de [`CE-02`](CE-02-averia-mecanica-en-ruta.md) y [`CE-21`](CE-21-galonaje-que-no-cuadra-con-kilometraje.md).

**b) La fecha de devolución tiene alarma y escalamiento.** Vencida, alerta diaria al Jefe de Transporte, al custodio y a la Gerencia Administrativa, y el préstamo entra al reporte de auditoría como **préstamo vencido no devuelto**, con los días de mora contados. Es lo único que impide el préstamo que dura seis años. `[C]` los umbrales de escalamiento.

**c) Los vencimientos documentales siguen corriendo y siguen siendo nuestros.** Matrícula, póliza y revisión no se congelan porque el vehículo esté prestado ([`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md)). Las alertas se emiten igual y se dirigen **también al responsable receptor**. Un vehículo nuestro circulando con matrícula vencida es un hallazgo nuestro, lo maneje quien lo maneje.

**d) Multas e infracciones se imputan por la fecha del hecho contra la ventana del préstamo.** La boleta del sábado llega a nombre del vehículo, es decir, a nombre nuestro. El sistema abre expediente en **M-12** y resuelve el tenedor **a la fecha del hecho de la infracción** ([`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)) — exactamente el mismo mecanismo que resuelve tarifas por vigencia. Si esa fecha cae dentro de la ventana del préstamo, el expediente nombra al responsable receptor y a su institución. **Eso no extingue nuestra responsabilidad frente al TSC: la documenta.**

**e) Circulación en día inhábil bajo tenencia ajena.** El microbús circuló un sábado con las franjas azul–blanco–azul y nuestras siglas. [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) exige permiso firmado por la máxima autoridad `[V]` y el salvoconducto es de la institución **propietaria**, no del tenedor. El acta de préstamo debe consignar expresamente **si la ventana comprende días u horas inhábiles** y, en ese caso, **el salvoconducto se emite antes de entregar el vehículo** o se hace constar que el préstamo se otorga solo para días hábiles. Es advertencia bloqueante de la entrega, con la excepción de vehículo de servicio exceptuado de [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) — que, como advierte esa misma regla, sigue al vehículo y puede producir circulación inhábil legítima para un uso que no lo es.

### 5. La devolución no es un correo: es un acta con odómetro

Sin acta de devolución con odómetro leído y fotografiado, el préstamo **no se cierra** y el vehículo **no vuelve a `DISPONIBLE`**. Toda diferencia entre lo entregado y lo devuelto —el rayón en la puerta corrediza, la llanta de repuesto que no está, el nivel de combustible— genera **registro de novedad vinculado a M-12** ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) punto 5) y, si corresponde, inicia deducción de responsabilidad.

Y se **reconstata la rotulación** con fotografía, porque su constatación caduca ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md)) y porque es hallazgo frecuente en operativos `[V]`.

### 6. Segregación: quien presta no recibe, y quien recibe no cierra su propio préstamo

La matriz de incompatibilidades vigente (`I-01` a `I-17`) **no contempla el préstamo**, porque se construyó sobre la cadena de la misión. Aquí hace falta, como mínimo, que **quien autoriza el préstamo no sea el responsable receptor** y que **quien firma el acta de devolución no sea quien la recibió**. Se propone como incompatibilidad nueva, y **no se resuelve en este documento**: [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) es la autoridad sobre incompatibilidades.

## Hallazgo — `NO_DISPONIBLE` no tiene causa tipificada para el préstamo

La máquina de estados enumera las causas de `NO_DISPONIBLE`: *documentación vencida, incidente bajo investigación, resguardo ordenado, sin custodio, en trámite de descargo, alta reciente sin habilitar*. **El préstamo no está**, pese a que [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) lo trata como caso límite y [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) exige que ocupe ventana.

Se reporta a [`docs/03-arquitectura/estados/orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md) como **ampliación de las causas tipificadas de `W-11`**: `PRESTADO_A_OTRA_INSTITUCION`, con expediente de préstamo obligatorio, fecha de devolución comprometida y retorno a `DISPONIBLE` únicamente por acta de devolución con odómetro. Sin tipificación, el préstamo se esconde detrás de un `NO_DISPONIBLE` genérico y desaparece del radar — que es exactamente cómo nace el préstamo de seis años.

## Decisiones escaladas al PO

### D-1 · ¿Quién autoriza cada tipo de préstamo? `[C]`

No se infiere. Lo que se puede afirmar es que hay **al menos dos niveles distintos** —el préstamo entre delegaciones no puede exigir lo mismo que ceder un bien a otra institución— y que [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) exige *acta y resolución* en los movimientos del bien, con nivel `[I]` porque es implicación de requerimiento nuestra, no articulado extraído.

| Opción | Consecuencia |
|---|---|
| **A** — Jefe de Transporte para ambos | Ágil, y deja la disposición de un bien del Estado en un nivel probablemente insuficiente |
| **B** — Jefe de Transporte entre dependencias; Gerencia Administrativa entre instituciones | Proporcional. Requiere confirmar competencia |
| **C** — Máxima Autoridad para el interinstitucional, con resolución | Máxima defensa en auditoría, más lento |

Requiere el **insumo #1** (reglamento interno de uso de vehículos) y consulta a la unidad de Bienes. **Hasta que se confirme, el sistema exige el acto autorizante como adjunto obligatorio sin fijar quién lo firma** — el campo existe, el titular es configurable.

### D-2 · ¿Quién paga combustible, peajes, mantenimiento, multas y daños durante el préstamo? `[C]`

Es lo que hoy se acuerda de palabra ("nos lo devuelven lleno") y lo que después nadie puede probar. La resolución de sistema no es decidir quién paga: es **exigir que quede escrito en el acta** como campos tipificados, uno por rubro, con las opciones *institución propietaria · entidad receptora · según convenio adjunto*. Sin eso, el acta no se puede firmar.

### D-3 · ¿Puede prestarse un vehículo con obligaciones abiertas? `[C]`

Un vehículo con orden de trabajo abierta, con documentación por vencer dentro de la ventana del préstamo, o con incidente bajo investigación. **Recomendación**: bloqueo si la documentación vence dentro de la ventana —es la misma lógica de [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), vigencia durante todo el rango— y advertencia registrada en los demás casos.

## Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:prestamo-de-vehiculo-como-expediente-del-bien` | El préstamo de un vehículo es un expediente de M-03 con acto autorizante, receptor nombrado, ventana con fecha de devolución comprometida y actas de entrega y devolución; nunca se modela como Orden de Misión |
| `RN-c:apoyo-con-motorista-propio-es-mision` | Cuando el vehículo se cede con motorista de la institución, se modela como Orden de Misión con motivo de apoyo institucional, no como préstamo del bien |
| `RN-c:kilometraje-bajo-tenencia-ajena` | El kilometraje recorrido bajo tenencia de un tercero se asienta con las dos lecturas de odómetro y **no entra** en la conciliación galonaje–kilometraje del vehículo |
| `RN-c:prestamo-vencido-no-devuelto` | Vencida la fecha de devolución comprometida, el préstamo alerta con escalamiento diario y entra al reporte de auditoría con los días de mora; no puede cerrarse el período con préstamos vencidos |
| `RN-c:alcance-de-datos-temporal-por-prestamo` | El préstamo entre dependencias amplía el alcance de datos de la dependencia tenedora sobre ese vehículo solo durante la ventana, y se retrae automáticamente al vencer |
| `RN-c:imputacion-de-multa-por-tenedor-a-la-fecha-del-hecho` | La infracción se imputa al tenedor vigente **a la fecha del hecho de la infracción**, sin que ello extinga la responsabilidad de la institución propietaria |
| `RN-c:devolucion-solo-con-acta-y-odometro` | Un préstamo no se cierra ni el vehículo vuelve a `DISPONIBLE` sin acta de devolución con odómetro fotografiado y reconstatación de rotulación |
| `I-c:autoriza-prestamo × recibe-el-vehiculo` | Quien autoriza un préstamo no puede ser el responsable receptor, ni quien firma la devolución puede ser quien recibió — propuesto a `actores-y-roles.md` |

## Evidencia que debe quedar

Ante el TSC o Auditoría Interna:

1. El **acto que autorizó el préstamo**, con folio, firmante identificado y adjunto
2. El **acta de entrega** con odómetro fotografiado, nivel de combustible, inventario de accesorios, documentos entregados, estado de rotulación fotografiado y daños preexistentes
3. La **persona responsable receptora** nombrada, con cargo e institución, y su constancia de recepción
4. La **fecha de devolución comprometida** y, si hubo mora, los días contados y el escalamiento emitido
5. El **acta de devolución** con odómetro, novedades encontradas y reconstatación de rotulación
6. El **kilometraje bajo tenencia ajena** asentado por separado, con la razón por la que no entra en la conciliación de rendimiento
7. Los rubros pactados: **quién asumía combustible, peajes, mantenimiento, multas y daños**, con su respaldo
8. Los **expedientes de infracción o daño** abiertos, con el tenedor resuelto a la fecha del hecho y el estado de la deducción de responsabilidad
9. Si la ventana comprendió día u hora inhábil: el **salvoconducto emitido**, o la constancia de que el préstamo se limitó a días hábiles
10. La **cadena de custodia continua**: en cualquier fecha del período se puede decir quién respondía por la unidad

## Trazabilidad

- **Reglas**: [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) el préstamo ocupa ventana · [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) estado con dependencia tenedora · [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) custodia en préstamo y a terceros · [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) correlativo institucional · [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) vencimientos · [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) rotulación con caducidad de constatación · [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) día inhábil y salvoconducto · [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) conciliación · [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) odómetro · [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) fecha del hecho · [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) asiento reverso · [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) segregación
- **Reglas candidatas**: `RN-c:prestamo-de-vehiculo-como-expediente-del-bien`, `RN-c:apoyo-con-motorista-propio-es-mision`, `RN-c:kilometraje-bajo-tenencia-ajena`, `RN-c:prestamo-vencido-no-devuelto`, `RN-c:alcance-de-datos-temporal-por-prestamo`, `RN-c:imputacion-de-multa-por-tenedor-a-la-fecha-del-hecho`, `RN-c:devolucion-solo-con-acta-y-odometro`, `I-c:autoriza-prestamo × recibe-el-vehiculo`
- **Normas**: [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) — identificación del vehículo `[V]`, prohibición de circulación inhábil `[V]`, préstamo interinstitucional dentro del ciclo del bien `[I]` *(implicación de requerimiento del equipo, no articulado)*, tarjeta de responsabilidad y descargo `[P]`, articulado del Manual de Propiedad Estatal no extraído `[P]` · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) segregación y registro `[P]`
- **Estados**: **ampliación pendiente** de las causas tipificadas de `W-11` con `PRESTADO_A_OTRA_INSTITUCION` — ver hallazgo · `W-02` retorno a `DISPONIBLE` solo por acta de devolución
- **Puntos de control**: `PC-05` vehículo asignable · `PC-03` día u hora inhábil · `PC-16` registro del acto
- **Actores**: `ACT-04` gestiona el préstamo · `ACT-08` autoriza el interinstitucional `[C]` · `ACT-13` custodio permanente · `ACT-14` Encargado de Bienes · `ACT-10` en el préstamo entre delegaciones · `ACT-09` firma el salvoconducto si la ventana toca día inhábil · `ACT-12` verifica
- **Insumos pendientes**: **#52** quién autoriza cada tipo de préstamo (D-1) · **#53** rubros económicos del préstamo (D-2) · **#54** préstamo de vehículo con obligaciones abiertas (D-3) · **#1** reglamento interno de uso de vehículos · formatos vigentes de acta de entrega-recepción y tarjeta de responsabilidad (`NRM-02`, zonas grises)
- **Casos especiales relacionados**: [`CE-15`](CE-15-vehiculo-en-comodato-o-alquilado.md) régimen de tenencia distinto de propio — **el préstamo y el comodato no son lo mismo y se confunden todo el tiempo** · [`CE-02`](CE-02-averia-mecanica-en-ruta.md) préstamo informal de vehículo de otra delegación en plena misión · [`CE-12`](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) competencia por la flota · [`CE-21`](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) rendimiento contaminado
- **Historias candidatas**: `HU-c:registrar-prestamo-de-vehiculo-con-acta-de-entrega`, `HU-c:ampliar-alcance-de-datos-por-prestamo-entre-dependencias`, `HU-c:cerrar-prestamo-con-acta-de-devolucion-y-odometro`, `HU-c:alertar-prestamo-vencido-no-devuelto`, `HU-c:imputar-infraccion-al-tenedor-a-la-fecha-del-hecho`, `HU-c:consultar-historial-de-tenencia-de-un-vehiculo`
