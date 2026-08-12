# CE-19 — El pickup del Subdirector duerme en su casa y nunca tuvo una orden de misión

| Campo | Valor |
|---|---|
| **Módulos** | M-03 Flota, M-01 Organización y Seguridad, M-07 Despacho, M-08 Bitácora, M-09 Combustible, M-12 Incidentes y Sanciones, M-14 Auditoría, M-04 Documentación |
| **Estados afectados** | Todo el ciclo, y sobre todo **su ausencia**: uso continuo sin `T-02`, `T-05`, `T-12`, `T-14` ni `T-18` · Vehículo: `DISPONIBLE` que en realidad no lo está |
| **Frecuencia** | Frecuente — casi toda institución pública tiene vehículos bajo asignación permanente |
| **Impacto** | Legal, de auditoría y financiero. Es hallazgo recurrente del TSC y objeto de operativos en Semana Santa `[V]` |
| **Resolución** | Definida para el régimen, la habilitación, el combustible y el resguardo · `[C]` para la Orden de Misión permanente de período y para el resguardo domiciliario |

## La situación

La institución tiene dos mundos de flota y los administra como si fueran uno solo.

**El pool.** Doce vehículos en el predio de la sede en Tegucigalpa. Se piden con requisición, los despacha el Encargado de Despacho, vuelven al predio y las llaves se guardan bajo control. Cada salida tiene su papel.

**Los asignados.** El pickup doble cabina correlativo `INM-0007` está asignado al Subdirector de Delegaciones desde hace tres años, por memorando. Sale de la casa del Subdirector en la colonia Kennedy por la mañana, hace lo que haya que hacer, y vuelve a esa casa por la noche. **Nunca se emitió una orden de misión para eso.** La bitácora la llena la asistente a fin de mes, con kilometrajes redondeados a la centena. El combustible sale por un cupo mensual contra firma, sin misión asociada.

El Jueves Santo, el TSC monta operativo de fiscalización vehicular en la salida a Valle de Ángeles `[V]`. Ahí está el `INM-0007`, con franjas azul–blanco–azul, la leyenda **PROPIEDAD DEL ESTADO DE HONDURAS**, y la familia del Subdirector adentro. Sin salvoconducto, porque nadie pidió uno: en tres años el vehículo nunca necesitó permiso para nada.

Lo que sigue: acta, multa reportada entre **L 5,000 y L 50,000** más posible decomiso `[P]` — `[C]` la base legal exacta del rango —, y la institución sin un solo documento que explique bajo qué régimen ese vehículo estaba fuera del predio un día feriado.

**Cinco variantes más, todas ordinarias:**

1. **El funcionario conduce él mismo.** No hay motorista. Nadie verificó nunca si su licencia habilita ese vehículo ni si está vigente.
2. **Despacho lo asigna a otra misión.** Para el sistema el `INM-0007` está `DISPONIBLE`, porque nunca aparece programado. El Jefe de Transporte lo asigna un martes a una comisión a Comayagua. El vehículo está en Valle de Ángeles.
3. **El funcionario cesa en el cargo.** El vehículo aparece un mes después, con dos llantas distintas, el gato faltante y tres años de odómetro sin registrar.
4. **Llega una multa de tránsito** por exceso de velocidad en la carretera al norte, del 14 de mayo a las 4:20 de la tarde. Nadie puede decir quién conducía ese día.
5. **Un vehículo del pool prestado "de manera indefinida"** a una dirección. No es asignación permanente formal, no es pool. Es el limbo, y ahí está donde ocurren las cosas.

## Qué se hace hoy sin sistema

`[C]` La práctica de la institución no está confirmada — insumo #1 (**reglamento interno de uso de vehículos**, que es la fuente natural de todo este caso) e insumo #2 (formatos vigentes).

Lo que se observa como práctica común en instituciones públicas hondureñas `[I]`:

- La asignación permanente consta en un **memorando**, en la tarjeta de responsabilidad, o en nada. Casi nunca tiene fecha de vencimiento: se confiere y se olvida.
- **No se emite orden de misión para el uso diario.** La orden de misión se reserva para viajes al interior del país. El uso urbano cotidiano es invisible para el expediente.
- La **bitácora se llena en bloque a fin de mes**, de memoria, con kilometrajes redondeados. Es una reconstrucción, y el TSC la lee como tal.
- El combustible se entrega por **cupo mensual** — *"tantos galones al mes"* — contra la firma del funcionario o de su asistente, sin misión que lo respalde.
- El vehículo **pernocta en la residencia** del funcionario, y se justifica de palabra como resguardo y seguridad. Puede ser una práctica razonable; lo que no hay es un documento que lo autorice.
- Los vehículos asignados **no entran en la programación semanal**. Por eso nadie en la institución sabe cuál es la flota realmente disponible: el número que se reporta es el de la flota registrada.

**El cupo mensual contra firma es la regla que nadie escribió.** La institución ya decidió, de hecho, que el vehículo asignado consume combustible sin misión. Lo que nunca decidió es **contra qué se comprueba ese consumo**, y esa es exactamente la pregunta que llega en el requerimiento de auditoría.

## Por qué el flujo normal no lo cubre

- **El ciclo de vida de la Orden de Misión asume un evento discreto**: se solicita, se autoriza, se despacha, sale, retorna, se liquida, se cierra. El uso de un vehículo asignado **no tiene esa forma**: es continuo. Obligar a una orden de misión por cada salida a una reunión en el centro haría que el sistema se abandone en la primera semana.
- [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) distingue custodia permanente de custodia temporal por misión. Aquí la **custodia permanente es el uso mismo**: no hay traslado temporal que registrar, y por lo tanto no hay `T-12`, ni `T-14`, ni odómetro de salida, ni acta de recepción.
- [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) bloquea entregar combustible sin Orden de Misión aprobada. Aplicado literalmente al vehículo asignado, o bloquea la operación real o alguien lo desactiva — y desactivarlo mata el control sobre **toda** la flota, no sobre este vehículo.
- La sección 10.2 de la máquina de estados no tiene causa tipificada para *"asignado a funcionario"*. El vehículo queda `DISPONIBLE`, que es falso, y la variante 2 se produce sola.
- `BD-04` y [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) exigen permiso de la máxima autoridad para circular en día u hora inhábil, **evaluado en `T-12`**. Un vehículo que nunca pasa por `T-12` nunca es evaluado. La regla existe y no se aplica jamás sobre el vehículo que más la necesita.
- Y hay un problema de fondo: [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) existe para la excepción de circulación por **naturaleza del servicio** — emergencia, seguridad, salud `[V]`. **La asignación a un funcionario no es eso**, y confundirlas convertiría a `RN-24` en la puerta trasera por donde cualquier vehículo de jefatura queda exceptuado.

## Regla de resolución

### Punto de partida, no negociable

**Un vehículo asignado permanentemente sigue siendo bien del Estado.** La asignación es un **régimen de custodia**, no una excepción al control.

[NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` prohíbe el uso de vehículos del Estado en días y horas inhábiles y para tareas ajenas a la función, **incluido expresamente el traslado de funcionarios, empleados y sus familias a residencias o asuntos personales**. La prohibición no distingue entre vehículo de pool y vehículo asignado. La Circular STLCC-ONADICI No. 022-03-2024 sobre uso indebido de vehículos `[V]` es la referencia vigente sobre la materia.

Por lo tanto: **sigue necesitando orden de misión, bitácora y permiso para día inhábil.** Lo que este caso resuelve no es si se controla, sino **con qué instrumento**, porque el instrumento del pool no le sirve.

### 1. El régimen de uso es un atributo del vehículo, con acto y vigencia

Catálogo configurable con vigencia por rango de fechas, nunca cableado ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)):

| Régimen | Qué significa |
|---|---|
| `POOL` | Disponible para programación por Transporte. El caso ordinario |
| `ASIGNACION_PERMANENTE` | Adscrito a un funcionario determinado |
| `ASIGNACION_A_DEPENDENCIA` | Adscrito a una unidad, con custodio pero sin funcionario asignatario |
| `PRESTAMO_TEMPORAL` | El limbo de la variante 5, tipificado con fecha de fin obligatoria |
| `COMODATO` / `ALQUILADO` | `[C]` régimen abierto en [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) |

Todo régimen distinto de `POOL` exige registrar:

- **El acto que lo confiere**: folio, autoridad que lo emite, fundamento y adjunto del documento.
- **Vigencia por rango de fechas.** No existe "para siempre": un régimen sin fecha de fin se renueva o caduca. Al caducar, el vehículo **vuelve a `POOL`** y el sistema lo reporta.
- **Beneficiario**: funcionario o dependencia.
- **Si autoriza resguardo domiciliario**, con su fundamento.
- **Si autoriza que el asignatario conduzca.**

Un régimen sin acto que lo confiera es un hallazgo, y el sistema debe poder listarlos todos en una consulta. Hoy esa lista no existe en ninguna institución.

`RN-24` **no** se usa para esto: la excepción de circulación por naturaleza del servicio es otra cosa y se registra aparte.

### 2. El vehículo asignado sale del conjunto asignable

Estado operativo `NO_DISPONIBLE` con **causa tipificada nueva: bajo régimen de asignación**. Efecto directo:

- El Jefe de Transporte **no puede programarlo** para otra misión sin suspender o revocar el régimen, y eso es un acto registrado con autoridad competente. Se acaba la variante 2.
- Aparece, por primera vez, el número que hoy no existe en la institución: **flota efectivamente asignable ≠ flota registrada**. Ese número es lo que sostiene una solicitud de compra de vehículos ante Gerencia Administrativa, y también lo que revela cuántas unidades están fuera del control operativo.

### 3. Sigue necesitando Orden de Misión — pero no una por viaje

| Uso | Instrumento |
|---|---|
| Viaje fuera del ámbito habitual, misión al interior, ruta con peajes o con pernocta | **Orden de Misión ordinaria**, ciclo completo, sin ninguna excepción por régimen |
| Uso ordinario dentro del ámbito geográfico y del horario hábil autorizados | **Orden de Misión permanente de período** `[C]`: un folio, ámbito geográfico, ventana horaria hábil y vigencia acotada. Bajo ella cuelgan la bitácora diaria y los consumos |
| Circulación en día u hora inhábil | **Permiso de la máxima autoridad y salvoconducto impreso con folio y QR**, igual que cualquier otro vehículo. La orden permanente **no ampara** día inhábil — [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), `BD-04`, `PC-03` |
| Pernocta en la residencia del asignatario | Solo si el acto de asignación lo autoriza. Entrada y salida del domicilio se registran como **eventos de bitácora**, no como misiones |

`[C]` **La Orden de Misión permanente de período no existe hoy.** La máquina de estados es la autoridad sobre estados y transiciones, y este caso especial **no la modifica**. Se escala al PO con sus opciones:

| Opción | Costo |
|---|---|
| **(a)** Orden de Misión permanente de período | Cambio en la máquina de estados: un estado o subtipo que admite bitácora continua sin `T-18` por evento. Es el diseño que refleja la operación real |
| **(b)** Orden de Misión por jornada, generada automáticamente | Sin cambio estructural, pero produce ~250 expedientes al año por vehículo asignado y una liquidación diaria que nadie va a hacer |
| **(c)** Exceptuar del control el uso ordinario | **Descartada.** Es exactamente el vacío que produce el hallazgo, y dejaría el consumo de combustible sin ancla de kilometraje |

Recomendación del análisis: **(a)**, con vigencia máxima configurable y liquidación al cierre del período.

### 4. Bitácora diaria, no mensual

El odómetro se captura al **inicio y al final de cada jornada de uso**, desde el cliente de campo, sin conectividad ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)), con fecha del hecho distinta de fecha de captura ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)).

No es burocracia: **el odómetro es el único ancla que permite conciliar el galonaje** ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)), y el hallazgo típico del TSC en flota es el incremento de consumo sin relación con el uso habitual. Sobre un vehículo asignado, hoy, esa conciliación es imposible de hacer.

La captura la hace **quien conduce**, con su propia identidad. `[C]` confirmar si el sistema exige al funcionario asignatario capturarla él mismo o si se admite un registro delegado con constancia de quién digitó ([`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)).

### 5. Combustible con misión. Siempre

[`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) **no se desactiva.** Lo que cambia es contra qué se imputa el vale: contra la **Orden de Misión permanente vigente** del período, o contra la orden ordinaria del viaje. El cupo mensual sin misión desaparece como figura.

Y con eso entran en juego, sobre estos vehículos y por primera vez: folio y receptor por asignación ([`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md)), comprobación del consumo con odómetro y fotografía del comprobante ([`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)), liquidación del período ([`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)) y conciliación galonaje–kilometraje ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)).

La segregación se mantiene íntegra: **quien recibe el combustible no puede ser quien lo entrega ni quien liquida** ([`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), `PC-09`, `PC-13`). Que el receptor sea un Subdirector no cambia nada.

### 6. Quien conduce tiene que estar habilitado, sea quien sea

[`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) y [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) se verifican sobre **la persona que conduce**, no sobre el puesto. Un Subdirector que conduce un vehículo del Estado necesita licencia habilitante para esa categoría, vigente, exactamente igual que un motorista. **Bloqueo duro sin excepción configurable** — [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): *una excepción registrada sería evidencia en contra ante un siniestro*.

Consecuencia de segregación que hay que decir en voz alta: si el funcionario asignatario es a la vez **solicitante, custodio y conductor**, no puede además ser **quien autoriza** ni **quien liquida**. La autorización escala al nivel inmediato superior ([`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md), `PC-01`).

`[C]` **Quién autoriza la misión de un funcionario de alto nivel y de la máxima autoridad** — insumo #28. Es el mismo hueco, y en este caso es el más visible de la institución.

### 7. Semana Santa: el operativo es predecible, así que hay que llegar preparado

[NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` exige un **reporte previo**: qué vehículos están autorizados a circular con su permiso, y qué vehículos deben estar resguardados **con confirmación de resguardo**.

Los vehículos bajo asignación permanente son los que ese reporte tiene que mirar primero, porque son los únicos que ya están fuera del predio.

La **confirmación de resguardo** es un acto con fecha, responsable identificado y evidencia: fotografía del odómetro y del lugar de resguardo, con ubicación. Sin evidencia, el vehículo entra al reporte como **no confirmado** — que es un estado distinto de "resguardado", y así debe aparecer. Un reporte que dice "todo resguardado" sin evidencia es peor que uno que declara cinco pendientes.

### 8. Cuando termina la asignación

Cese, traslado, revocación o caducidad del régimen: **acta de entrega-recepción** con odómetro, estado, accesorios y herramientas ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[P]`).

El régimen **no se extingue porque el funcionario cese**: se extingue con el acta. Mientras no haya acta, el vehículo queda como **custodia vacante**, con alerta al Jefe de Transporte y bloqueo de despacho tras un plazo configurable ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), casos límite), y **el funcionario cesante sigue figurando como responsable**. Toda diferencia entre lo entregado y lo devuelto genera novedad vinculada a M-12.

Es el caso que produce los peores hallazgos: un vehículo del Estado que "está con" alguien que ya no trabaja en la institución.

### 9. Multas: sin bitácora no hay a quién imputar, y esa imposibilidad es el hallazgo

[NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[V]` exige registrar infracciones y multas asociadas al vehículo y al motorista, con estado de pago y **quién asume el costo**.

Bajo asignación permanente, la infracción se imputa a **quien conducía según la bitácora en la fecha y hora del hecho**. Si no hay bitácora de ese día, la infracción queda como **imputación no resuelta**, con responsable de resolverla y plazo. Nunca se asigna por deducción ni se paga en silencio con fondos institucionales.

Esa imposibilidad de imputar es el argumento más fuerte a favor de la bitácora diaria, y es más convincente que cualquier apelación al control interno: sin ella, la multa la termina pagando la institución.

### Lo que hay que confirmar

- `[C]` **¿Existe régimen formal de asignación permanente, quién lo confiere y con qué acto?** Insumo #64; el **reglamento interno de uso de vehículos** (insumo #1) es la fuente natural y hoy no lo tenemos.
- `[C]` **¿Autoriza la institución el resguardo domiciliario, y con qué fundamento?** [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) prohíbe `[V]` el traslado de funcionarios y sus familias a residencias. Que el vehículo **pernocte** en la residencia por resguardo es una figura distinta, y **no consta que esté regulada**. Este análisis no la valida ni la prohíbe: la registra como decisión pendiente de la máxima autoridad, y mientras tanto el sistema exige que el acto de asignación lo diga expresamente. Insumo #65.
- `[C]` **¿Se acepta la Orden de Misión permanente de período?** Decisión de producto con impacto en la máquina de estados — opciones y costo en la sección 3. Insumo #66.
- `[C]` **Quién autoriza la misión de un funcionario de alto nivel** — insumo #28.
- `[C]` **Base legal exacta del rango de multas** L 5,000 – L 50,000 `[P]` — pendiente abierto de [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md).
- `[C]` **¿Puede el asignatario conducir el vehículo, y con qué política?** Hoy ocurre de hecho; no consta política escrita.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C19a` | *El régimen de uso es atributo del vehículo, con acto que lo confiere, fundamento, beneficiario y vigencia acotada. Vencido sin renovación, el vehículo vuelve a pool y el hecho se reporta.* | Ninguna de las 54 reglas distingue pool de asignación permanente. Para el modelo actual, **todos los vehículos son de pool** |
| `RN-C19b` | *El vehículo bajo régimen distinto de pool sale del conjunto asignable con causa tipificada; retirarlo del régimen para asignarlo a una misión es un acto registrado de autoridad competente.* | §10.2 no tiene esa causa tipificada, y por eso el sistema cree disponible un vehículo que no lo está |
| `RN-C19c` | *Todo uso de un vehículo del Estado se ampara en una Orden de Misión. Ningún régimen de asignación exime de bitácora con odómetro, de permiso de día u hora inhábil, ni de imputar el combustible a una misión.* | Es la regla que hoy se incumple sin que ningún artefacto la enuncie. [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) la implica para combustible; nadie la enuncia para el uso |
| `RN-C19d` | *La habilitación para conducir se verifica sobre la persona que efectivamente conduce, cualquiera sea su puesto, y el conductor de cada jornada queda registrado.* | [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) y [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) se redactan alrededor del **motorista**. El funcionario que conduce no es motorista y hoy no lo alcanza ninguna verificación |
| `RN-C19e` | *La confirmación de resguardo ante operativo declarado exige responsable, fecha y evidencia con odómetro y ubicación. Sin evidencia, el vehículo figura como no confirmado, no como resguardado.* | [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) exige el reporte `[V]`; ninguna regla define qué cuenta como confirmación |

## Evidencia que debe quedar

Ante un requerimiento del TSC o de Auditoría Interna, la institución debe poder mostrar, por vehículo y por período:

1. **El acto que confiere el régimen**: folio, autoridad emisora, fundamento, beneficiario, vigencia, si autoriza resguardo domiciliario y si autoriza que el asignatario conduzca — con el documento adjunto.
2. La **tarjeta de responsabilidad o acta de entrega-recepción**, con odómetro y estado al momento de la entrega, y el **historial completo de custodios** consultable por rango de fechas.
3. La **Orden de Misión que ampara cada período de uso**, con folio y vigencia, y las órdenes ordinarias de cada viaje fuera del ámbito.
4. La **bitácora diaria** con odómetro de inicio y fin de jornada y **quién condujo cada día** — capturada con fecha del hecho, no reconstruida a fin de mes.
5. Los **vales imputados a misión**, sin ninguno huérfano, con su comprobación y la **conciliación galonaje–kilometraje** del período.
6. Los **permisos de circulación en día u hora inhábil** emitidos por la máxima autoridad, con salvoconducto, folio y QR, contrastados contra los días en que el vehículo efectivamente se movió según la bitácora. El contraste es el control, no el permiso por sí solo.
7. El **reporte previo a Semana Santa** con, por vehículo: permiso vigente, o confirmación de resguardo con evidencia, o la marca de no confirmado.
8. Las **infracciones y multas** registradas, con el conductor imputado según la bitácora de la fecha y hora del hecho, quién asume el costo y el estado de pago. Y las imputaciones no resueltas, con responsable y plazo.
9. El **acta de cierre del régimen** al cesar o trasladarse el funcionario, con odómetro, estado, accesorios y faltantes, y el expediente de M-12 por toda diferencia.
10. El **listado institucional de vehículos por régimen**, con los que tienen régimen sin acto que lo confiera o con vigencia caducada. Ese listado, producido antes de que lo pida el auditor, es la diferencia entre una observación y un hallazgo.

## Trazabilidad

- **Autoridad de estados:** §10.2 estado operativo del vehículo y `BD-04` de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md)
- **Autoridad de actores e incompatibilidades:** [actores-y-roles.md](../../01-negocio/actores-y-roles.md) — ACT-13 Custodio del Vehículo es rol adherido a un vehículo concreto, no a la estructura organizativa
- **Reglas:** [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md), [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md), [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md), [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md), [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)
- **Reglas candidatas:** `RN-C19a` a `RN-C19e`
- **Puntos de control:** `PC-01` (segregación solicitante ≠ autorizador), `PC-03` (salvoconducto), `PC-04` (licencia), `PC-09` y `PC-13` (segregación de fondo y liquidación) de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` prohibición de uso privado, permiso de la máxima autoridad, identificación obligatoria, operativos del TSC en Semana Santa y Circular STLCC-ONADICI No. 022-03-2024; `[P]` tarjeta de responsabilidad y rango de multas; `[C]` base legal del rango · [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[V]` registro de infracciones y de quién asume el costo · [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) segregación de funciones
- **Actores:** ACT-02, ACT-03, ACT-04, ACT-05, ACT-07, ACT-08, ACT-09, ACT-12, ACT-13, ACT-14
- **Casos relacionados:** [CE-16](CE-16-vehiculo-a-taller-con-misiones-programadas.md) (flota asignable frente a flota registrada), [CE-17](CE-17-vehiculo-sin-placa-metalica.md) (identificación del bien), [CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) (competencia por vehículo)
- **Insumos:** #1 (reglamento interno de uso de vehículos — **el más bloqueante de este caso**), #2 (formatos: tarjeta de responsabilidad y acta de entrega-recepción), #19 (informes previos de auditoría sobre uso de vehículos), #28 (quién autoriza la misión de la máxima autoridad), #29 (¿es delegable la firma del permiso de circulación en día u hora inhábil?), #32 (paquete de parámetros operativos), #64 (¿existe régimen formal de asignación permanente?), #65 (resguardo domiciliario), #66 (Orden de Misión permanente de período y conducción por el asignatario)
