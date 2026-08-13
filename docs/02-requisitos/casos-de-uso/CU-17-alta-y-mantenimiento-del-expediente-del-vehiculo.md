# CU-17 — Dar de alta y mantener el expediente del vehículo

| Campo | Valor |
|---|---|
| **Módulo** | M-03 Flota Vehicular · M-04 Documentación y Cumplimiento Vehicular (con M-02 catálogos y M-18 categoría de peaje) |
| **Proceso** | [`PR-02`](../../01-negocio/mapa-de-procesos.md) — Gestión del expediente del vehículo, habilitante de `PR-01` |
| **Actor principal** | `ACT-14` Encargado de Bienes Institucionales — para todo acto con consecuencia patrimonial: alta del bien, número de inventario, tarjeta de responsabilidad, traslados, constatación y descargo |
| **Actores secundarios** | `ACT-04` Jefe de Transporte (ficha técnica, documentación, vencimientos, habilitación operativa) · `ACT-13` Custodio del Vehículo (firma actas y constata) · `ACT-11` Encargado de Mantenimiento (declara estado operativo) · `ACT-08` Gerencia Administrativa (aprueba el descargo) · `ACT-01` Administrador (catálogos y parámetros con vigencia) · `ACT-12` Auditor Interno (consulta y exporta) |
| **Precondiciones** | Existe el documento que origina el ingreso del vehículo al patrimonio o al uso de la institución. Existen, vigentes a la fecha del hecho, los catálogos de `regimen_de_tenencia`, `estado_de_placa`, tipo de vehículo y la tabla de categorías de peaje ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`PR-09`](../../01-negocio/mapa-de-procesos.md)). El actor tiene rol vigente sobre el objeto *vehículo como bien*. |
| **Postcondiciones** | El vehículo existe en el registro con correlativo institucional único e irreciclable, título de tenencia vigente, ficha técnica evaluable, custodio vigente con acta, identificación constatada con fotografía y documentación con vencimientos programados. Su estado operativo es `DISPONIBLE` (`W-02`) o `NO_DISPONIBLE` **con causa tipificada**. Todo acto queda con autor, puesto, momento y huella del contenido. |
| **Disparador** | Ingreso de un vehículo por compra, donación, comodato, alquiler o traslado interinstitucional; o cualquier hecho posterior que modifique el expediente: cambio de custodio, constatación, renovación documental, cambio de estado de placa, traslado, fin de tenencia o descargo. |

**Por qué esto no es un CRUD.** Cuatro de sus actos producen efectos jurídicos y patrimoniales que un formulario de mantenimiento no representa: la **tarjeta de responsabilidad** determina sobre quién recae la deducción de responsabilidad; el **cambio de custodio** exige acta de entrega-recepción; la **constatación física** con fotografía es lo que se conciliará contra el registro de bienes; y el **descargo** es una baja patrimonial que quien la propone no puede aprobar (`I-17`). El orden importa: sin título y sin custodio no hay habilitación, y sin habilitación no hay misión.

## Flujo principal — alta e ingreso a la flota

1. `ACT-14` abre el expediente del vehículo y adjunta el **documento de origen**: acta de recepción de la compra, acta de donación, convenio de comodato, contrato de alquiler o resolución de traslado.
2. El sistema exige el **título de tenencia** ([`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)): régimen del catálogo configurable, titular, documento adjunto, rango de vigencia —con fecha de fin salvo en propiedad— y **rubros asumidos** (combustible, mantenimiento, llantas, seguro, peajes, multas y daños). **Sin título vigente el vehículo no se habilita en la flota.**
3. `ACT-14` registra la **identidad patrimonial**: correlativo institucional —obligatorio, único en la institución y **no reciclable** ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md))—, número de bien del inventario nacional, valor de adquisición y fuente de financiamiento ([NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[P]`).
4. `ACT-14` registra la **placa como dos datos distintos** ([`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md)): número asignado en el registro, si existe, y **estado de la placa física** del catálogo tipificado, con su rango de vigencia. El estado distinto de `CON_LAMINA` exige documento de respaldo con emisor, folio, adjunto y vigencia ([`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md), [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md)).
5. `ACT-04` completa la **ficha técnica**: tipo, marca, modelo, año, color, chasis/VIN, número de motor, **peso bruto vehicular en kg**, capacidad de pasajeros, capacidad de carga en kg y m³, **condición de articulado**, número de ejes, tipo de combustible y rendimiento esperado. El sistema advierte, en el momento de la captura, que sin peso bruto y sin condición de articulado **la habilitación licencia↔vehículo no se podrá evaluar** ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md)).
6. El sistema **deriva la categoría de peaje** de la ficha técnica contra la tabla vigente a la fecha del hecho ([`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)). Sin categoría resuelta el vehículo no es asignable — `BD-07`.
7. `ACT-14` emite la **tarjeta de responsabilidad**: designa a `ACT-13` como custodio, con **acta de entrega-recepción** firmada, fecha de inicio de la custodia, **odómetro**, nivel de combustible, accesorios y herramientas, y estado de la unidad ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)). Esa lectura constituye el primer valor del **kilometraje acumulado del expediente** ([`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md)).
8. `ACT-13` ejecuta la **constatación física inicial de identificación**, elemento por elemento y **con fotografía por elemento**: tres franjas azul–blanco–azul, leyenda "PROPIEDAD DEL ESTADO DE HONDURAS", siglas de la institución, numeración consecutiva y placas ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md)). **Una constatación sin fotografía no se acepta.** La captura funciona sin conectividad.
9. `ACT-04` registra la **documentación con vigencia**: matrícula, póliza de seguro, revisión mecánica, permisos, excepción de circulación si la tuviera ([`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md)) y documento de respaldo por falta de lámina. El sistema programa las **alertas anticipadas al puesto** —no a la persona— con los umbrales configurables ([`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md)).
10. `ACT-04` solicita la habilitación en flota. El sistema evalúa la **lista de comprobación completa** —título vigente, correlativo, ficha técnica evaluable, categoría de peaje resuelta, custodio vigente, documentación registrada, identificación constatada— y, si se cumple, ejecuta `W-02` `NO_DISPONIBLE → DISPONIBLE`. Mientras falte cualquier elemento, el vehículo permanece `NO_DISPONIBLE` **con la causa tipificada visible**, nunca con el estado vacío.
11. El sistema registra el acto con identidad, puesto, rol ejercido, marca de tiempo y huella del contenido ([`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md)), y el vehículo queda disponible para la programación (`CU-04`).

> El alta ingresa siempre por `W-01` a `NO_DISPONIBLE`. **Habilitar es un acto separado del alta**, y esa separación es deliberada: es lo que impide que un vehículo entre a la flota con la ficha a medio llenar y aparezca asignable el mismo día.

## Flujos alternos

**A1 — Cambio de custodio** (desde el paso 7, en cualquier momento posterior)

1. `ACT-14` inicia el traspaso de la tarjeta de responsabilidad por rotación, cambio de adscripción o cese del custodio.
2. El sistema exige **acta de entrega-recepción** con odómetro, estado, accesorios y herramientas, firmada por custodio saliente y entrante ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
3. Toda diferencia entre lo entregado y lo devuelto —herramienta faltante, daño nuevo— genera **registro de novedad** vinculado al expediente de M-12.
4. El registro anterior **no se sobrescribe**: se cierra su rango y se abre el nuevo. El historial de custodias es consultable por fecha: en cualquier momento del pasado se puede decir quién respondía por la unidad.
5. La custodia activa **bloquea el cierre de la asignación de puesto** del saliente ([`actores-y-roles.md` §2.4](../../01-negocio/actores-y-roles.md)).

**A2 — Constatación física periódica y resguardo previo a operativo** (`PR-14`)

1. `ACT-14` convoca la constatación por calendario de inventario, corte de conciliación de bienes o proximidad de Semana Santa —evento **recurrente y predecible** de fiscalización del TSC ([NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]`).
2. `ACT-13` captura desde el móvil, **sin conectividad**: fotografía por elemento, odómetro, ubicación y estado.
3. Transcurrido el parámetro `vigencia_constatacion_rotulacion`, la constatación **caduca** y el vehículo queda *identificación no constatada*, lo que **advierte con acuse** al despachar. El parámetro admite valor más corto para vehículos sin lámina, cuya rotulación es su única identificación visible ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md), [`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md)).
4. El sistema produce el reporte previo: vehículos autorizados a circular con su permiso y vehículos que deben estar resguardados **con responsable, fecha, odómetro y ubicación fotografiada**. **Sin evidencia, el vehículo figura como no confirmado, nunca como resguardado.**

**A3 — Renovación o actualización de documento** (desde el paso 9)

1. `ACT-04` registra el documento renovado con adjunto y nuevo rango de vigencia.
2. El sistema **cierra el rango anterior y abre uno nuevo**; no edita el vencimiento ([`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)).
3. Si el vehículo estaba `NO_DISPONIBLE` por documentación vencida y ya no queda causa activa, el sistema ofrece `W-02` a `ACT-04`.

**A4 — Cambio de estado de la placa** (desde el paso 4)

1. Llega la lámina, se retiene por autoridad, se extravía o se inicia trámite de reposición.
2. El sistema cierra el rango vigente y abre el nuevo con **fecha del hecho** distinta de la fecha de captura ([`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)) y motivo.
3. Una consulta por placa a una fecha pasada devuelve el vehículo que la tenía **a esa fecha**. Es lo que permite imputar una multa de marzo al vehículo correcto ([`RN-66`](../../01-negocio/reglas/RN-66-imputacion-externa-por-jerarquia-de-anclas.md)).
4. El estado `EN_TRAMITE_DE_REPOSICION` exige expediente del trámite: fecha de inicio, institución ante la que se gestiona, gestiones y resultado.

**A5 — Traslado del vehículo entre unidades o delegaciones** (desde el paso 7)

1. `ACT-14` registra el traslado con acta, unidad de origen, unidad de destino y designación del nuevo custodio.
2. El traslado **no es un préstamo**: el préstamo es expediente propio del bien, con receptor, fecha comprometida de devolución y actas ([`RN-63`](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md), [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md)). **Nunca se instrumenta como Orden de Misión.**

**A6 — Sustitución de la unidad por el arrendador** (régimen de alquiler)

1. El arrendador retira la unidad y entrega otra bajo el **mismo título de tenencia**.
2. La unidad entrante se da de alta como **vehículo nuevo**, con su propio correlativo y su **propia serie de odómetro** — el kilometraje no se arrastra ([`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md)).
3. Se levanta **acta de sustitución** y se revalidan las misiones ya programadas sobre la unidad saliente, recalculando y volviendo a congelar todo valor derivado ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md), [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md)).

**A7 — Descargo o baja del bien propio** (estado terminal `DADO_DE_BAJA`)

1. `ACT-14` **instruye el expediente de descargo**: causa —siniestro total, desuso, obsolescencia, pérdida no recuperada—, avalúo si corresponde, actas, fotografías y expediente de incidente vinculado cuando lo haya.
2. El sistema verifica que **todas las misiones del vehículo estén en estado terminal**. Con una sola misión abierta, el descargo no procede ([`orden-de-mision.md` §10.2](../../03-arquitectura/estados/orden-de-mision.md)).
3. El sistema verifica que no queden custodias ni obligaciones vivas sobre la unidad.
4. `ACT-08` aprueba y se ejecuta `W-14` desde `NO_DISPONIBLE`, o `W-15` desde `EN_TALLER` por irreparable. **`ACT-14`, que propuso, no puede aprobar** — `I-17`, bloqueo duro.
5. El expediente histórico **se conserva íntegro**: las misiones cerradas del vehículo siguen siendo consultables y auditables. El correlativo institucional **queda ocupado permanentemente**.

> `[C]` **Quién aprueba el descargo.** [`orden-de-mision.md` §10.2](../../03-arquitectura/estados/orden-de-mision.md) —autoridad en transiciones— dice `ACT-08` con acta. El [mapa de procesos](../../01-negocio/mapa-de-procesos.md) admite *"`ACT-08` o `ACT-09`"*. [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) deja el punto `[P]`: el Manual de Propiedad Estatal regula el descargo pero **no se pudo extraer el articulado**. Este caso de uso sigue a la autoridad en transiciones y usa `ACT-08`; la divergencia queda anotada como hallazgo, no resuelta aquí.

**A8 — Fin de tenencia de un bien ajeno: devolución al comodante o al arrendador**

1. `ACT-14` registra la devolución con **acta**, odómetro obligatorio, novedades y liquidación de daños.
2. El sistema conserva íntegro todo el historial del período de tenencia —bitácoras, consumos, incidentes, costos—: **no se va con el vehículo**.
3. El expediente **no debe cerrarse como `DADO_DE_BAJA`**: el bien no se descargó, se devolvió ([`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) §7).

> ⚠️ **Nota de hallazgo — falta el estado terminal `RETIRADO_DE_FLOTA`.** El ciclo de vida del vehículo tiene un único estado terminal, `DADO_DE_BAJA`, alcanzable solo por descargo (`W-14`, `W-15`), y **ambas transiciones suponen que el bien es de la institución**. Declarar *dado de baja* un pickup que se devolvió al comodante registra un descargo que nunca ocurrió sobre un bien que nunca estuvo en el inventario: **es un asiento falso**, y es exactamente el tipo de asiento que el TSC encuentra cruzando el inventario de bienes contra el padrón de flota. La autoridad en transiciones es [`docs/03-arquitectura/estados/`](../../03-arquitectura/estados/orden-de-mision.md) — **este caso de uso no crea el estado ni la transición**; reporta que hoy `A8` no tiene desenlace válido en la máquina. Hallazgo ya abierto en [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) y en el [índice de reglas](../../01-negocio/reglas/README.md).

**A9 — Reingreso de un vehículo recuperado tras robo**

1. El vehículo recuperado **reingresa con su correlativo original**, no con uno nuevo: el expediente es continuo, con el período de indisponibilidad registrado ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md)).
2. Ingresa a `NO_DISPONIBLE` con causa tipificada y requiere constatación física, revisión de `ACT-11` y nueva tarjeta de responsabilidad antes de `W-02`.

## Flujos de excepción

**E1 — Correlativo institucional duplicado** (en el paso 3)

1. El sistema **rechaza** el alta: el correlativo es único **por institución**, no por delegación.
2. Ocurre típicamente en la carga inicial, cuando dos delegaciones numeraron por su cuenta. Los duplicados **se resuelven antes de operar**, no después.
3. `[C]` Si la institución numera por delegación, el correlativo se compone de código de delegación + número y esa composición es el identificador único — insumo **#34**.

**E2 — Placa que ya existe en otro vehículo** (en el paso 4)

1. El sistema **advierte** e indica cuál es el otro vehículo, pero **permite guardar** exigiendo motivo escrito, que queda registrado ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md)).
2. La evaluación de duplicidad se hace **por rango de vigencia**: dos vehículos con el mismo número en rangos que no se traslapan no son duplicado ([`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md)).
3. Nunca se bloquea por este motivo: el correlativo institucional mantiene la operación funcionando mientras el registro vehicular resuelve.

**E3 — Falta el peso bruto vehicular o la condición de articulado** (en el paso 5)

1. El sistema **no habilita** el vehículo y explicita qué dato de la ficha técnica falta.
2. La consecuencia se muestra en el mismo mensaje: sin esos atributos, [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) **no puede evaluarse y bloqueará toda asignación**. El sistema **nunca asume el valor faltante**.

**E4 — El vehículo queda sin custodio vigente** (posterior al paso 7)

1. El custodio cesa en el cargo o se traslada; el espejo de Talento Humano lo detecta.
2. El sistema marca el vehículo como **custodia vacante**, alerta a `ACT-04` y a `ACT-14`, y **bloquea el despacho** transcurrido el plazo configurable `[C]` ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
3. Es incómodo y es correcto: un vehículo del Estado sin responsable identificado es un hallazgo esperando ocurrir.

**E5 — Se intenta dar de baja un vehículo con misiones abiertas** (en `A7`, paso 2)

1. **Bloqueo duro.** El sistema lista las misiones no terminales que lo impiden y su estado.
2. No hay pantalla de excepción: primero se cierran las misiones, después se descarga el bien.

**E6 — `ACT-14` intenta aprobar el descargo que él mismo propuso** (en `A7`, paso 4)

1. **Bloqueo duro** por `I-17`: quien propone la baja de un bien no la aprueba.
2. El mensaje nombra el conflicto con precisión e indica a qué puesto corresponde la aprobación.
3. El intento **se registra en la pista de auditoría** con el par de incompatibilidad detectado, y se genera tarea de resolución en el puesto competente ([`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)).

**E7 — `ACT-04` intenta ejecutar el alta del bien, la tarjeta de responsabilidad o el descargo** (en los pasos 3, 7 o en `A7`)

1. **Bloqueo por competencia.** El alta del bien, el número de inventario nacional, la tarjeta de responsabilidad, el traslado patrimonial y el descargo **no son competencia del Jefe de Transporte**: los ejecuta la unidad de Bienes, `ACT-14`, cuyo alcance es institucional pero restringido al objeto *vehículo como bien*.
2. `ACT-04` conserva íntegra su competencia sobre el **expediente operativo**: ficha técnica, documentación, vencimientos, estado operativo y habilitación en flota ([`actores-y-roles.md` §4](../../01-negocio/actores-y-roles.md), acciones 22 y 23).

**E8 — La institución no tiene unidad de Bienes separada** (afecta a todo el caso de uso)

1. `ACT-14` se mapea al mismo puesto que `ACT-08` y desaparece la separación proponer/aprobar del descargo.
2. Se activa el **control compensatorio** previsto en [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md): el expediente de descargo se marca como *acumulación vigilada* y se notifica a `ACT-12`.
3. `[C]` Confirmar con la institución si la unidad existe — pendiente ya registrado en la ficha de `ACT-14`.

**E9 — Vehículo robado, decomisado o no recuperado**

1. El bien **permanece en el registro** hasta su recuperación o su descargo formal ([`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)). No se borra ni se oculta.
2. Estado `NO_DISPONIBLE` con causa tipificada, expediente de incidente en M-12, denuncia y estado del proceso de deducción de responsabilidad ([CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md)).
3. La custodia se traslada al tercero que lo retiene —autoridad, taller, aseguradora— con acta y fecha. El vehículo **no queda sin custodio** ni sigue formalmente bajo el motorista.

**E10 — Un documento vence con el vehículo `EN_MISION`**

1. El vencimiento **no cambia el estado operativo por sí solo**: el vehículo ya salió.
2. Genera alerta y, **al retornar**, lo lleva a `NO_DISPONIBLE` con causa tipificada ([`orden-de-mision.md` §10.2](../../03-arquitectura/estados/orden-de-mision.md)).

**E11 — El título de tenencia vence antes que las misiones ya programadas** (desde el paso 2)

1. El sistema bloquea `W-02` mientras el título esté vencido, y bloquea la programación y el despacho de toda misión cuya ventana —con holgura— exceda la vigencia del título, indicando la **fecha concreta** ([`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)).
2. Las alertas de [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) debieron avisarlo antes: el bloqueo es la última línea, no la primera.

**E12 — Alerta de vencimiento crónica por trámite de placa detenido**

1. El trámite lleva años sin resolverse por el desabastecimiento nacional y la alerta se vuelve ruido.
2. Se admite marcarla como **reconocida con fundamento** por un período configurable, tras el cual reaparece. **Silenciarla para siempre no está disponible** ([`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md)).

## Reglas aplicables

| Regla | Qué aporta a este caso de uso |
|---|---|
| [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) | Correlativo institucional único e irreciclable; la placa **no es obligatoria ni única** |
| [`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md) | Número asignado y estado físico de la lámina son datos distintos, con historial y vigencia |
| [`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) | Lo que bloquea el despacho sin lámina no es la ausencia de placa: es la ausencia de respaldo |
| [`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) | Título de tenencia con régimen, vigencia y rubros; fin de tenencia ≠ descargo |
| [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) | Custodio vigente siempre, con acta de entrega-recepción y historial |
| [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) | Identificación del vehículo del Estado constatada con fecha y fotografía; la constatación caduca |
| [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) | Alertas anticipadas por documento, dirigidas al **puesto**, con umbrales configurables |
| [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) | Póliza y revisión rastreables y alertables; bloqueo configurable **apagado por defecto** |
| [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) | Solo se asigna desde `DISPONIBLE` |
| [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md) | Los atributos de la ficha técnica que la habilitación necesita: peso bruto, capacidad, articulado |
| [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) | Categoría de peaje derivada de la ficha, no del número de ejes por sí solo |
| [`RN-89`](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) | El kilometraje acumulado es atributo del expediente, no lectura del instrumento |
| [`RN-90`](../../01-negocio/reglas/RN-90-intervencion-del-instrumento-de-medicion.md) | Toda intervención del odómetro es evento con orden de trabajo y autorización nominativa |
| [`RN-63`](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md) | El préstamo es expediente del bien, nunca una Orden de Misión |
| [`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) | El bien retenido o sustraído no sale del registro |
| [`RN-58`](../../01-negocio/reglas/RN-58-regimen-de-uso-del-vehiculo.md) · [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) | Régimen de uso y excepción de circulación como atributos del vehículo, con acto y vigencia |
| [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) · [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) · [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) | Registro inmutable, anulación como asiento reverso, rangos cerrados que no se editan |
| [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | Segregación: propone el descargo ≠ aprueba el descargo (`I-17`) |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) · [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) | Catálogos y tablas con vigencia; todo se resuelve a la fecha del hecho |
| [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) | Fecha del hecho ≠ fecha de captura; digitación diferida con original adjunto |

## Anclas de estado y de control

- **Estados del vehículo:** `W-01` alta en flota · `W-02` habilitar · `W-11` inhabilitar · `W-09`/`W-12` ingreso a taller · `W-14`/`W-15` descargo · `DADO_DE_BAJA` terminal — [`orden-de-mision.md` §10.2](../../03-arquitectura/estados/orden-de-mision.md).
- **Bloqueos duros que este caso de uso alimenta:** `BD-03` documentación del vehículo vigente · `BD-07` estado y compatibilidad del vehículo.
- **Puntos de control de `PR-01` que dependen de este expediente:** `PC-05` vehículo asignable · `PC-06` póliza y revisión · `PC-07` compatibilidad · `PC-03` día u hora inhábil, vía excepción de circulación del vehículo.
- **Incompatibilidad:** `I-17` `ACT-14` propone el descargo × aprueba el descargo — bloqueo duro.

## Trazabilidad

- **Proceso:** `PR-02` gestión del expediente del vehículo · `PR-14` constatación física y resguardo de la flota · alimenta `PR-01` y `PR-05`
- **Casos especiales:** [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) comodato y alquiler · [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) sin placa metálica · [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) préstamo · [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) vehículo asignado a funcionario · [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) taller con misiones programadas · [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) odómetro inconsistente · [CE-04](../casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) robo
- **Normativa:** [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) identificación obligatoria `[V]`, tarjeta de responsabilidad y descargo `[P]`, articulado del Manual de Propiedad Estatal `[C]` · [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) matrícula, desabastecimiento de placas `[V]`, seguro y revisión no obligatorios `[V]` · [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) conciliación de bienes y segregación `[P]`
- **Decisiones:** [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-08 estado del vehículo desde el campo, D-11 expediente del vehículo como entidad de primera clase, D-13 seguro configurable
- **Actores:** `ACT-14` principal · `ACT-04` · `ACT-13` · `ACT-11` · `ACT-08` · `ACT-01` · `ACT-12`
- **Historias `HU-xxx`:** pendientes — se derivan de este caso de uso en el bloque de historias
- **Insumos pendientes:** **#1** reglamento interno de uso de vehículos · **#2** formatos en papel (acta de entrega-recepción, tarjeta de responsabilidad, descargo) · **#34** correlativo único por institución o compuesto por delegación · **#43** cómo se rotula una motocicleta del Estado · **#44** vehículos con excepción de rotulación y quién la concede · **#55** rotulación en comodato y alquiler · **#56** día inhábil en comodato y alquiler · **#57** modalidad de alquiler y sustitución de unidad · **#58** cómo se registra hoy la devolución al comodante · **#59** si el preventivo vencido bloquea o advierte · **#60** catálogo de documentos sustitutivos del IP · **#64** régimen de asignación permanente a funcionario
- **Insumo nuevo a registrar:** `[C]` **quién aprueba el descargo de un vehículo** — `ACT-08`, `ACT-09` o comisión, y con qué acto; y **qué formato de acta de descargo usa la institución**. Hoy [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) lo deja `[P]` y los artefactos internos divergen (ver nota de `A7`)
