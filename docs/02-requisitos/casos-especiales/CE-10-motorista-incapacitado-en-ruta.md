# CE-10 — Pasando Bonito Oriental al motorista le da un dolor en el pecho y ya no puede seguir manejando

| Campo | Valor |
|---|---|
| **Módulos** | M-08 Ejecución y Bitácora, M-12 Incidentes, M-05 Motoristas y Habilitación, M-07 Programación y Despacho, M-09 Combustible, M-03 Flota, M-13 Liquidación, M-16 Operación Desconectada, M-20 Integraciones |
| **Estados afectados** | `EN_RUTA` — y el vehículo en `EN_MISION` |
| **Frecuencia** | Ocasional, y **grave siempre**. Planilla de motoristas envejecida, jornadas largas, calor y carreteras duras |
| **Impacto** | Legal, de auditoría, operativo y humano |
| **Resolución** | Definida. **Requiere ampliar `T-18`** y **dos decisiones escaladas al PO** |

> **No confundir con `CE-05`** (cambio de motorista planificado, pendiente de redacción), donde el relevo se planifica y se autoriza antes. Aquí la incapacidad ocurre **en carretera**, probablemente sin señal, con un vehículo del Estado y una carga que alguien tiene que custodiar en los próximos veinte minutos.
>
> Tampoco es [`CE-03`](CE-03-accidente-de-transito-en-mision.md). Si el motorista se desvaneció **y** el vehículo colisionó, aplican los dos casos a la vez y el expediente es uno solo.

## La situación

Sale de la Delegación de Tocoa, Colón, un camión 4x4 cargado con paneles solares, baterías y material de oficina para las oficinas de Iriona y Sangrelaya. Salió el jueves a las 04:50, odómetro **211,704**, con fondo para cinco días. El motorista es **Melvin, 54 años, licencia categoría C vigente**. Van con él un técnico de sistemas y una comisionada de la unidad ejecutora.

Pasando **Bonito Oriental**, sobre la carretera a Trujillo, Melvin se orilla: dolor en el pecho, sudoración fría, no puede sostener el volante. Odómetro **211,798**. Son las 06:40 de la mañana.

En los siguientes minutos hay que resolver cuatro cosas al mismo tiempo, y solo una de ellas le importa a una persona:

1. **Melvin necesita llegar a un hospital.** El más cercano con capacidad es el de Tocoa, 40 minutos atrás.
2. **Alguien tiene que mover ese camión.** El técnico tiene licencia **categoría B** — no habilita un camión. La comisionada no tiene licencia.
3. **La carga vale más que el vehículo** y está a la orilla de una carretera donde nadie se va a quedar cuidándola.
4. **El fondo de combustible lo lleva Melvin en la bolsa**, a su nombre, con su firma de recepción.

Y hay una quinta cosa que nadie piensa en ese momento y que aparece tres semanas después: **la incapacidad médica que emita el IHSS o el médico institucional va a cubrir desde el jueves.** Cuando ese dato baje del sistema de Talento Humano, el expediente va a mostrar a un motorista conduciendo un vehículo del Estado el día en que estaba incapacitado.

## Qué se hace hoy sin sistema

Se llama a la delegación por celular si hay señal, o desde la primera pulpería con teléfono. El técnico se lleva a Melvin a Tocoa en un carro particular que pare en la carretera, o en mototaxi hasta el desvío. El camión queda **con la comisionada** al lado de la vía, o se mete al patio de una gasolinera, o se le pide a la posta policial de Bonito Oriental que lo deje adentro.

De la delegación sale otro motorista en una moto o en lo que haya. Llega dos o tres horas después. Sigue el viaje o se devuelve.

Cuatro prácticas no escritas que hay que sacar a la luz:

- **El fondo se traspasa de bolsillo a bolsillo.** Melvin le entrega el efectivo o los vales al técnico, o al motorista que llega, y no queda acta. Cuando se liquide, la firma de recepción dirá "Melvin" y quien gastó fue otro. **Ese es el hallazgo.**
- **A veces maneja el que no debe.** El técnico con licencia B mueve el camión "solo hasta la gasolinera". Son cuatro kilómetros y es exactamente lo que [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) prohíbe sin excepción posible: si en esos cuatro kilómetros hay un percance, la responsabilidad va sobre quien lo autorizó, y sobre la institución.
- **Nadie lee el odómetro.** Se anota el de retorno y el tramo de Melvin y el del relevo quedan mezclados en un solo número. La conciliación de rendimiento sale sobre dos conductores distintos y no significa nada.
- **La incapacidad se tramita en Talento Humano por un carril y la misión se liquida por otro.** Nunca se cruzan. Nadie relaciona el expediente médico con el expediente de la misión, y por eso nunca se ha detectado la contradicción del punto 5.

`[C]` **Qué hace hoy la institución cuando no hay ningún motorista disponible para el relevo.** Es la pregunta que decide si el camión pasa la noche en la carretera. Insumo pendiente.

## Por qué el flujo normal no lo cubre

El flujo feliz asume que quien salió conduciendo es quien regresa conduciendo. Aquí se rompe eso, y encima se rompe el eslabón que sostiene todo lo demás: **la custodia**.

[`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) trasladó la custodia del vehículo a Melvin en el despacho, con constancia. La única forma prevista de devolverla es el retorno. **Melvin va camino a un hospital y puede que no esté en condiciones de firmar nada.** Un vehículo del Estado no puede quedar sin custodio identificado ni un minuto — es, textualmente, un hallazgo esperando ocurrir.

Y la máquina de estados no tiene la salida que este caso necesita: desde `EN_RUTA` existen `T-17` (prórroga o relevo) y `T-18` (retorno). `T-17` sirve si **llega** un motorista de relevo. Si no llega ninguno y el personal vuelve por otro medio dejando el camión resguardado en Bonito Oriental, no hay transición: `T-18` "retorno sin vehículo" está tipificado para **siniestro total, robo o decomiso**, y aquí el vehículo está intacto.

## Regla de resolución

### 0. Primero la persona. El sistema espera

Ninguna validación, bloqueo ni registro puede interponerse en la atención médica. **Todo el registro de este caso es posterior al hecho** y entra por la vía de la [digitación diferida](CE-09-bitacora-en-papel-digitada-dias-despues.md) o de la captura sin conectividad, con fecha del hecho distinta de fecha de captura ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)). Que el sistema exija el registro *en el momento* de una emergencia médica es la forma más rápida de que se abandone.

### 1. Un evento tipificado, sin cambiar el estado

Se registra el evento **`INCAPACIDAD_DEL_MOTORISTA_EN_RUTA`**, con: hora del hecho, ubicación descrita, **odómetro leído con fotografía del tablero**, naturaleza declarada de la incapacidad —sin diagnóstico ni dato clínico, ver punto 7—, quién queda al frente y qué se hizo con el vehículo y con la carga.

La Orden de Misión **sigue en `EN_RUTA`** y recibe la **marca de situación "interrumpida"**, igual que en [`CE-02`](CE-02-averia-mecanica-en-ruta.md): una marca sobre el expediente, no un estado inventado. Y el evento abre expediente en **M-12**.

**Quién lo registra.** Normalmente `ACT-06`, pero aquí `ACT-06` es el incapacitado. El sistema debe admitir que el evento lo registre **`ACT-10` Encargado de Delegación o `ACT-04` Jefe de Transporte** por lo informado desde la carretera —teléfono, radio, mensaje—, con constancia de **quién informó y por qué medio**, bajo el mismo esquema de [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md): el autor del hecho y el capturador son personas distintas y ambas quedan nombradas.

### 2. Quién puede conducir: aquí no se negocia

Este es el punto donde el sistema tiene que ser rígido aunque duela, porque es el que traslada responsabilidad penal y patrimonial.

| Quién | ¿Puede conducir? |
|---|---|
| Otro motorista del padrón con licencia habilitante y vigente | **Sí.** Relevo por `T-17`, con revalidación completa de [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) y [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) contra el paquete normativo congelado que lleva el dispositivo, y acta de traspaso de custodia con odómetro ([`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md), [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)) |
| Un acompañante con licencia **habilitante para ese tipo de vehículo** y vigente | **Sí**, incorporándolo como motorista con la misma revalidación. Ver punto 3 |
| El técnico con licencia categoría B frente a un camión | **No. Bloqueo duro, sin excepción configurable** ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), `PC-04`). No existe "solo hasta la gasolinera" |
| Un particular, un pariente, el mecánico del pueblo | **No.** Es uso indebido de vehículo del Estado |
| Una grúa o el propio taller trasladando la unidad | **Sí**, y no es conducción: es traslado de un bien. Se registra como movimiento de custodia a un tercero, con acta ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)) |

**El bloqueo se evalúa contra el paquete congelado en el dispositivo, sin red.** Si el sistema no puede verificar la licencia por falta de señal, no se levanta el bloqueo: se resuelve contra el paquete que el dispositivo ya lleva, que es exactamente para lo que existe.

### 3. Motorista eventual: la habilitación excepcional se registra, no se improvisa

Cuando el que puede conducir es un servidor que **no está en el padrón de motoristas** (M-05) —el técnico, la comisionada, un compañero de otra unidad—, no basta con que tenga la licencia correcta. El sistema debe permitir **incorporarlo como motorista de la misión** capturando su licencia con número, categorías, vigencia y restricciones, y **evaluando [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) y [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) sobre él con el mismo rigor**.

Dos consecuencias que hay que decir en voz alta:

- **La incorporación arrastra `I-11`.** Desde ese momento esa persona es motorista de esa misión, y **no puede autorizarla, despacharla, recibir el fondo ni liquidarla** — núcleo irreductible, [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md). Si la comisionada que conduce era quien iba a liquidar, deja de poder hacerlo. Sin aviso, esto se convierte en una autoliquidación perfecta.
- `[C]` **¿La institución admite que un servidor que no es motorista de planilla conduzca un vehículo oficial?** No es una pregunta de sistema, es de reglamento interno y de póliza. Escalada al PO — ver decisiones.

### 4. La custodia no puede quedar en el aire ni un minuto

La custodia temporal de [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) **se cierra siempre**, aunque Melvin no pueda firmar. El acta de traspaso por incapacidad registra: odómetro, nivel de combustible, herramientas y accesorios, estado de la unidad, fotografías, y **quién recibe**. Si el motorista no puede firmar, se hace constar así y firman **dos personas presentes** — la firma manuscrita sobre impresión es uno de los niveles previstos por [`NRM-08`](../../01-negocio/normativa/NRM-08-firma-electronica.md). **Fingir una firma que no ocurrió es peor que no tenerla.**

El receptor tipificado puede ser: el motorista de relevo, `ACT-10`, `ACT-13` custodio permanente, o **un tercero con acta** — posta policial, patio de estación de servicio, oficina municipal. Lo que **no** puede es no haber ninguno.

La **carga** sigue su propio hilo: acta de transbordo con inventario si se pasa a otro vehículo, o acta de resguardo con lugar y responsable si se queda ([`CE-02`](CE-02-averia-mecanica-en-ruta.md), regla candidata `RN-c:acta-de-transbordo-de-carga`). Si hay **personas externas** a bordo, el manifiesto cerrado de [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) recibe una **novedad**, no una edición.

### 5. El fondo de combustible cambia de manos con acta o no cambia de manos

El fondo está asignado a Melvin con folio y constancia de recepción ([`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md)). Tres salidas, en este orden de preferencia:

| Situación | Qué hace el sistema |
|---|---|
| Melvin está consciente y puede entregar | **Acta de traspaso de fondo** con folio, saldo en efectivo y vales no consumidos enumerados, quien entrega, quien recibe y dos testigos. El receptor pasa a responder por el saldo desde esa hora |
| Melvin no está en condiciones de entregar | El fondo **sigue a su nombre**. Se registra el hecho y el saldo estimado. Se emite **asignación nueva** al motorista de relevo si la misión continúa. La liquidación se hace **por asignación, no por misión** |
| El fondo se traspasó de hecho, sin acta | Se registra **como ocurrió**, con la hora y quién lo tomó, y queda como **observación de liquidación**. No se maquilla |

**La liquidación de esta misión es siempre por tramo y por responsable de fondo.** Mezclar los dos consumos produce un rendimiento que no existe — mismo criterio que [`CE-02`](CE-02-averia-mecanica-en-ruta.md) y que [`CE-21`](CE-21-galonaje-que-no-cuadra-con-kilometraje.md).

### 6. Cuatro desenlaces, no tres

| Desenlace | Qué hace el sistema |
|---|---|
| **Llega relevo y la misión continúa** | `T-17` relevo, con revalidación de `BD-02` contra el paquete congelado y acta de traspaso de custodia con odómetro. El tramo de Melvin se cierra en 211,798. Combustible y kilometraje se imputan por tramo |
| **Retorno anticipado conduciendo el relevo** | `T-18` subtipo **retorno anticipado**, con motivo `INCAPACIDAD_DEL_MOTORISTA`. Liquidación por lo efectivamente ejecutado |
| **El vehículo queda resguardado y el personal vuelve por otro medio** | **No hay transición hoy.** Ver el hallazgo abajo. El vehículo pasa a `NO_DISPONIBLE` con causa tipificada *resguardado fuera de sede*, y no vuelve a `DISPONIBLE` hasta que alguien lo traiga con acta de recepción y odómetro |
| **Queda pendiente de resolución esa noche** | La marca "interrumpida" permanece, con responsable y fecha límite. **Ninguna misión con esta marca sobrevive al cierre del período** |

### 7. Dato de salud: se registra el hecho, no el diagnóstico

SIGTI registra que **hubo una incapacidad que impidió continuar la conducción**, la hora y quién lo constató. **No registra diagnóstico, síntomas, resultados ni expediente clínico.** Eso es de Talento Humano y del IHSS, y SIGTI no lo reimplementa ([`DP-001`](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). El acceso a este evento se restringe por rol y toda consulta se registra, igual que en M-17 ([`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), criterio de minimización de [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)).

### 8. La incapacidad que llega después no puede convertir el pasado en falta

Semanas más tarde, el espejo de Talento Humano ([`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md), [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)) trae una incapacidad de Melvin que arranca **el jueves**, el mismo día de la misión. Sin tratamiento explícito, el sistema mostrará un motorista conduciendo un vehículo oficial mientras estaba incapacitado — y eso es un hallazgo fabricado por el propio sistema.

Regla: **la incapacidad sobrevenida no invalida retroactivamente el tramo ejecutado antes del hecho.** Cuando la fecha de inicio de una incapacidad coincide con una misión `EN_RUTA` que tiene un evento `INCAPACIDAD_DEL_MOTORISTA_EN_RUTA` registrado ese mismo día, el sistema **vincula ambos registros** y el expediente muestra la secuencia correcta: condujo habilitado hasta las 06:40, se incapacitó, no volvió a conducir. Si **no** existe tal evento, entonces sí hay contradicción y va a la cola de conflictos de [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) para resolución humana — nunca se resuelve en silencio en ninguna de las dos direcciones.

### 9. El motorista no vuelve a ser asignable por decisión de nadie en SIGTI

Melvin queda no disponible por [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) mientras la incapacidad esté vigente **según Talento Humano, que es la fuente**. SIGTI no decide cuándo vuelve. Lo que SIGTI sí hace es **no dejarlo asignable mientras tanto**, y alertar si la incapacidad no aparece en el espejo dentro del plazo configurado — porque un evento de incapacidad en ruta sin incapacidad registrada después es, o un trámite que nadie hizo, o un evento que no fue lo que se dijo.

`[C]` **¿Existe reevaluación de aptitud para conducir tras un evento de salud en ruta?** [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) cubre restricciones médicas de la licencia, pero no la aptitud posterior a un episodio. Escalado al PO.

## Hallazgo — falta la salida "vehículo resguardado fuera de sede"

`T-18` subtipo *retorno sin vehículo* está tipificado para **siniestro total, robo o decomiso**, y exige expediente de incidente de vehículo perdido. Aquí el vehículo **está intacto y localizado**, solo que no volvió ese día porque no había quién lo condujera.

Forzar este caso al subtipo existente falsea el expediente: declara perdido un bien que está en el patio de la posta de Bonito Oriental. Forzarlo a *retorno anticipado* es peor: declara un retorno que no ocurrió y le pide un odómetro de recepción a un vehículo que no llegó.

**No se resuelve en este documento**: [la máquina de estados es la autoridad en transiciones](../../../CLAUDE.md). Se reporta como **ampliación necesaria de `T-18`** dirigida a [`docs/03-arquitectura/estados/orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md): subtipo **retorno del personal con vehículo resguardado en sitio**, que exige acta de resguardo con lugar, receptor y odómetro fotografiado, lleva el vehículo a `NO_DISPONIBLE` con causa tipificada, y **deja abierta la obligación de recuperarlo** con responsable y plazo. El expediente no cierra hasta que la unidad esté de vuelta con acta de recepción.

## Decisiones escaladas al PO

### D-1 · ¿Puede conducir un servidor que no es motorista de planilla? `[C]`

| Opción | Consecuencia | Costo |
|---|---|---|
| **A — No, nunca** | Simple y defendible. Pero el camión pasa la noche en la carretera cada vez que no hay relevo disponible | Alto costo operativo y riesgo sobre la carga |
| **B — Sí, si la licencia habilita el tipo de vehículo y está vigente** | Resuelve la emergencia sin tocar el bloqueo duro de `RN-09` | Exige capturar licencia de no-motoristas y confirmar cobertura de la póliza `[C]` |
| **C — Sí, solo en emergencia declarada y con convalidación posterior** *(recomendada)* | Acota el uso al hecho excepcional y deja rastro. Se apoya en el mecanismo ya existente de [`CE-01`](CE-01-salida-de-emergencia-convalidada.md) y `PC-18`: el acto se ejecuta, y si no se convalida, la misión cierra con hallazgo | Exige el mecanismo de convalidación, que ya está diseñado |

**En las tres opciones, [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) y [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) siguen siendo bloqueo duro.** Lo que se decide es si la persona puede entrar como motorista, no si puede entrar sin licencia habilitante.

### D-2 · ¿Cubre la póliza a un conductor no registrado como motorista? `[C]`

Pregunta para la unidad que administra el seguro. Si la póliza exige conductor de planilla, la opción B y C de D-1 dejan de ser viables aunque el reglamento las permita. **Es la pregunta que puede cerrar la discusión antes de empezar.** Insumo pendiente.

### D-3 · ¿Existe reevaluación de aptitud tras un evento de salud en ruta? `[C]`

De existir, es un requisito de M-05 y un bloqueo adicional sobre la reincorporación. Frontera con Talento Humano — pertenece a ellos, SIGTI solo lo consume.

## Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:incapacidad-del-motorista-en-ruta` | La incapacidad del motorista en ruta se registra como evento tipificado sin conectividad, marca la misión como interrumpida sin cambiarle el estado, abre expediente en M-12 y habilita cuatro desenlaces |
| `RN-c:motorista-eventual-habilitado-en-ruta` | Incorporar a un servidor no perteneciente al padrón como motorista de una misión exige capturar y evaluar su licencia con el mismo rigor, y le aplica `I-11` desde ese momento |
| `RN-c:traspaso-de-custodia-por-incapacidad` | La custodia temporal se cierra siempre, aunque el motorista no pueda firmar: consta el impedimento y firman dos personas presentes más el receptor tipificado |
| `RN-c:traspaso-de-fondo-por-incapacidad-del-receptor` | El fondo asignado cambia de responsable solo con acta, folio, saldo enumerado y dos testigos; sin acta permanece a nombre del receptor original y la liquidación es por asignación |
| `RN-c:incapacidad-sobrevenida-no-invalida-el-tramo-ejecutado` | Una incapacidad que llega del espejo con fecha de inicio coincidente con una misión ejecutada se vincula al evento en ruta correspondiente; sin evento que la explique, es conflicto para resolución humana |
| `RN-c:vehiculo-resguardado-fuera-de-sede` | Un vehículo que no retorna por falta de conductor habilitado queda `NO_DISPONIBLE` con causa tipificada, acta de resguardo y obligación de recuperación con responsable y plazo — depende de la ampliación de `T-18` |
| `RN-c:minimizacion-del-dato-de-salud-del-servidor` | SIGTI registra la existencia de la incapacidad y su efecto operativo; nunca diagnóstico ni dato clínico, con acceso por rol y registro de consultas |

## Evidencia que debe quedar

Ante el TSC o Auditoría Interna, encadenado a la misma Orden de Misión:

1. El **evento de incapacidad** con hora del hecho, ubicación, odómetro fotografiado, quién informó y por qué medio, y quién lo capturó si no fue el motorista
2. El **acta de traspaso de custodia del vehículo**, con odómetro, estado, accesorios y receptor identificado — y, si el motorista no firmó, la constancia del impedimento con dos firmas
3. El **acta de transbordo o de resguardo de la carga**, con inventario y responsable
4. La **revalidación de licencia del conductor entrante**, con los datos concretos contra los que se evaluó y si se hizo contra el paquete congelado
5. El **acta de traspaso del fondo** con saldo enumerado, o la constancia de que no se traspasó y a nombre de quién quedó
6. La **liquidación por tramo y por responsable de fondo**, con el rendimiento calculado por conductor y no promediado
7. La **vinculación entre el evento en ruta y la incapacidad registrada en Talento Humano**, o el conflicto abierto si no coinciden
8. Si el vehículo quedó fuera de sede: el **acta de resguardo**, la causa tipificada del `NO_DISPONIBLE`, y el **acta de recepción al recuperarlo** con odómetro
9. Quién autorizó cada desenlace, cuándo, y si fue con código fuera de línea o con justificación diferida
10. Si condujo un servidor no perteneciente al padrón: la **convalidación posterior**, o el hallazgo por su ausencia

## Trazabilidad

- **Reglas**: [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) segregación e `I-11` · [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) habilitación · [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) disponibilidad · [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) sustitución · [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) vehículo no operativo · [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) custodia · [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) fondo y conciliación · [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) captura y diferimiento · [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md), [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) espejo · [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) personas y manifiesto · [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) asiento reverso
- **Reglas candidatas**: `RN-c:incapacidad-del-motorista-en-ruta`, `RN-c:motorista-eventual-habilitado-en-ruta`, `RN-c:traspaso-de-custodia-por-incapacidad`, `RN-c:traspaso-de-fondo-por-incapacidad-del-receptor`, `RN-c:incapacidad-sobrevenida-no-invalida-el-tramo-ejecutado`, `RN-c:vehiculo-resguardado-fuera-de-sede`, `RN-c:minimizacion-del-dato-de-salud-del-servidor`
- **Normas**: [`NRM-06`](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) licencias `[P]` · [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) custodia y uso del bien `[V]`/`[P]` · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) segregación y registro `[P]` · [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) conectividad `[V]` · [`NRM-08`](../../01-negocio/normativa/NRM-08-firma-electronica.md) firma manuscrita sobre impresión
- **Transiciones**: `T-17` relevo con revalidación · `T-18` subtipo retorno anticipado · **`T-18` requiere subtipo nuevo** *retorno con vehículo resguardado en sitio* — ver hallazgo · `W-08` vehículo que no retorna · `W-11` inhabilitar con causa tipificada
- **Prohibida**: `EN_RUTA → ANULADA` — el vehículo salió y hubo consumo real
- **Bloqueos duros que no se levantan aquí**: `BD-02` habilitación del motorista · `I-11` motorista sobre su propia misión
- **Puntos de control**: `PC-04` licencia habilitante y vigente · `PC-10` disponibilidad del motorista · `PC-11` coherencia del odómetro · `PC-16` registro del acto · `PC-18` acto pendiente de convalidación
- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E9 y E11
- **Actores**: `ACT-06` motorista incapacitado y motorista entrante · `ACT-10` registra desde la delegación · `ACT-04` decide el desenlace · `ACT-13` custodio permanente · `ACT-07` fondo · `ACT-17` fuente de la incapacidad
- **Insumos pendientes**: **#48** conductor no perteneciente al padrón (D-1) · **#49** cobertura de la póliza (D-2) · **#50** reevaluación de aptitud tras evento de salud (D-3) · **#51** qué se hace hoy sin relevo disponible · **#27** dotación real de las delegaciones · **#1** reglamento interno de uso de vehículos
- **Casos especiales relacionados**: [`CE-02`](CE-02-averia-mecanica-en-ruta.md) avería en ruta — misma mecánica de marca y desenlaces · [`CE-03`](CE-03-accidente-de-transito-en-mision.md) accidente de tránsito, concurrente si hubo colisión · `CE-05` cambio de motorista planificado — **no confundir** · [`CE-09`](CE-09-bitacora-en-papel-digitada-dias-despues.md) registro diferido, vía normal de captura de este caso · [`CE-11`](CE-11-licencia-vence-durante-la-mision.md) habilitación que decae en ruta · [`CE-13`](CE-13-motorista-no-disponible-por-talento-humano.md) indisponibilidad detectada antes de salir
- **Historias candidatas**: `HU-c:registrar-incapacidad-del-motorista-en-ruta`, `HU-c:incorporar-motorista-eventual-con-revalidacion-de-licencia`, `HU-c:traspasar-custodia-sin-firma-del-motorista`, `HU-c:traspasar-fondo-de-combustible-con-acta`, `HU-c:registrar-resguardo-de-vehiculo-fuera-de-sede`, `HU-c:conciliar-incapacidad-del-espejo-con-evento-en-ruta`
