# CE-15 — Ese pickup no es de la institución: lo cedió un proyecto y hay que devolverlo en septiembre

| Campo | Valor |
|---|---|
| **Módulos** | M-03 Flota, M-04 Documentación y Cumplimiento, M-02 Catálogos Maestros, M-11 Mantenimiento, M-18 Peajes, M-09 Combustible, M-07 Programación, M-13 Liquidación, M-14 Auditoría, M-15 Formatos Oficiales |
| **Estados afectados** | Todo el ciclo de vida del vehículo. Bloquea `T-08` y `T-12` de las órdenes cuya ventana exceda la vigencia del título |
| **Frecuencia** | Frecuente. Casi toda institución pública opera con al menos una unidad que no es suya |
| **Impacto** | Patrimonial, financiero y de auditoría |
| **Resolución** | Definida en lo operativo. **Dos preguntas normativas abiertas en `NRM-02` que no se resuelven aquí**, más un estado terminal que falta |

> **`CE-15` no es `CE-14`, aunque se confundan todo el tiempo.** En [`CE-14`](CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) el vehículo **es nuestro** y se va. Aquí el vehículo **no es nuestro** y está con nosotros, operando todos los días, gastando nuestro combustible y llevando a nuestro personal.

## La situación

### A · Comodato

Un proyecto de cooperación cede a la institución **dos pickups 4x4 doble cabina por 24 meses**, del 1 de octubre al 30 de septiembre. Llegan con:

- **Placa particular**, no placa nacional
- El **logotipo del proyecto** en las puertas, y ninguna franja azul–blanco–azul
- **Póliza a nombre del proyecto**, con vencimiento en marzo
- Un convenio que dice que el mantenimiento corre por cuenta de la institución y que todo siniestro debe notificarse dentro de 48 horas
- Sin **número de bien del inventario nacional**, porque no son bienes de la institución

A los ocho meses el proyecto cambia de coordinador. A los veinte meses nadie en la institución recuerda que hay que devolverlos. **Y hay que devolverlos en septiembre, en buen estado, con acta.**

### B · Alquiler

La institución alquila **un microbús de 25 pasajeros por 45 días** para una ronda de capacitaciones en Comayagua, Siguatepeque y La Paz. Tarifa diaria. El contrato incluye mantenimiento y **excluye combustible**. Tiene tope de kilometraje mensual y un excedente que se cobra aparte.

En el día 12 la rentadora **cambia la unidad**: el microbús entra a servicio y traen otro, de otro modelo, con otra placa y otro odómetro.

## Qué se hace hoy sin sistema

Los vehículos que no son propiedad de la institución **no entran al inventario de flota**, porque el inventario es de bienes y estos no son bienes. Quedan en una hoja de cálculo del Jefe de Transporte, o en ninguna parte.

Tres prácticas no escritas, y la segunda es la peor cosa que hay en este documento:

1. **Operan sin bitácora.** "Es del proyecto, no lleva bitácora nuestra." Resultado: no hay kilometraje, no hay rendimiento, no hay historial de uso, y cuando hay que devolverlo no hay con qué demostrar en qué se usó.
2. **El combustible se le carga al correlativo de otro vehículo.** Como el pickup del proyecto no existe en el sistema, la factura se imputa a un vehículo que sí existe. **Eso no solo esconde un consumo: destruye el rendimiento del vehículo inocente para siempre**, y va a disparar alertas de desviación de [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) que nadie va a poder explicar dos años después.
3. **La póliza del tercero vence y nadie se entera**, porque "el seguro es del proyecto". Un siniestro con la póliza vencida sobre un bien ajeno es responsabilidad patrimonial directa de la institución.

`[C]` **Cómo se registra hoy la devolución al comodante y quién la autoriza.** Insumo pendiente.

## Por qué el flujo normal no lo cubre

Buena parte del modelo asume **propiedad del Estado**. La ficha maestra que exige [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) incluye *número de bien del inventario nacional, valor de adquisición y fuente de financiamiento* — tres campos que un vehículo alquilado no tiene y nunca va a tener. Si son obligatorios, el vehículo no se puede dar de alta, y si no se puede dar de alta, vuelve la hoja de cálculo.

Y el ciclo de vida del vehículo **no tiene salida para esto**. Su único estado terminal es `DADO_DE_BAJA`, al que se llega por **descargo** (`W-14`, `W-15`). Un pickup que se devuelve al comodante en septiembre no está dado de baja: **está devuelto**. Declararlo dado de baja sería declarar el descargo de un bien que no está en nuestro inventario.

Finalmente, hay dos preguntas normativas que [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) deja **expresamente abiertas** `[C]`: *"Régimen aplicable a vehículos en comodato o alquilados: ¿les aplica la rotulación y la prohibición de día inhábil?"*. **No se resuelven en este documento.**

## Regla de resolución

### 1. El régimen de tenencia no es una etiqueta: es un título con vigencia

Todo vehículo de la flota tiene un **título de tenencia** con rango de fechas, exactamente igual que cualquier otro dato normativo del sistema ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)):

| Campo | Contenido |
|---|---|
| **Régimen** | propio · comodato · alquilado · donado — catálogo configurable de M-02 |
| **Entidad titular** | Quién es el dueño real, con su identificación |
| **Documento que lo sustenta** | Convenio de comodato, contrato de alquiler, acta de donación, escritura — con folio y **adjunto obligatorio** |
| **Vigencia** | Desde / **hasta**. En régimen propio, *hasta* queda abierto |
| **Obligación de devolución** | Sí/No, con fecha comprometida y estado en que debe devolverse |
| **Responsabilidad económica por rubro** | Combustible · mantenimiento preventivo · correctivo · llantas · seguro · peajes · multas · daños. Uno por uno, con *institución · titular · según contrato adjunto* |
| **Restricciones de uso pactadas** | Ámbito geográfico, propósito, tope de kilometraje, y qué pasa al excederlo |

**Ningún vehículo se habilita en la flota (`W-02`) sin título de tenencia vigente y su documento adjunto.** Y el correlativo institucional se le exige igual que a cualquier otro ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md), que ya declara aplicar *"sea propio, en comodato, alquilado o donado"*).

Con esto muere la práctica #2 de arriba: **el vehículo existe, tiene correlativo, y el combustible se le imputa a él.**

### 2. La misión no puede exceder la vigencia del título

Es el mismo patrón de [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), y por la misma razón: **no basta que el título esté vigente el día de salida, tiene que estarlo el último día de la misión.** Una misión de ocho días que arranca el 26 de septiembre con un comodato que vence el 30 pone a un servidor público conduciendo, el 4 de octubre, un vehículo sobre el que la institución ya no tiene título.

**Bloqueo duro en `T-08` programar y `T-12` despachar**, evaluado como los demás bloqueos: con los datos del momento y contra el paquete congelado si es en campo. Y alerta anticipada de vencimiento del título con los umbrales de [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) — el título de tenencia es un documento con vencimiento como cualquier otro, y de los que más caro salen.

### 3. Lo operativo aplica igual. Sin excepciones y sin discusión

Un vehículo alquilado que lleva personal de la institución en carretera es, para todo efecto operativo, un vehículo de la flota:

| Aplica | Por qué |
|---|---|
| **Bitácora, odómetro de salida y retorno** ([`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md)) | Es lo único que después demuestra en qué se usó, y lo que la rentadora va a cobrar por exceso de kilometraje |
| **Imputación de combustible al vehículo** ([`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)) | El fondo es público, gastado por la institución. El dueño del carro es irrelevante para eso |
| **Custodia con tarjeta de responsabilidad** ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), que declara aplicar *"en cualquier régimen de tenencia"*) | Si se daña un bien ajeno, la deducción de responsabilidad necesita saber quién lo tenía |
| **Habilitación del motorista** ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md)) | La categoría de licencia depende del vehículo, no de quién lo posee. Bloqueo duro igual |
| **Compatibilidad y capacidad** ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)) | La ficha técnica es del vehículo |
| **Peajes: categoría derivada de la ficha técnica** ([`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md)) | La caseta cobra por el vehículo, no por su dueño |
| **Incidentes, siniestros y multas** (M-12) | Con la particularidad del punto 6 |
| **Seguimiento en ruta** (M-19) | Si lleva personal, hay que saber dónde está |

### 4. Lo patrimonial no aplica, y forzarlo produce registros falsos

| No aplica | Qué se hace en su lugar |
|---|---|
| Número de bien del inventario nacional, valor de adquisición, fuente de financiamiento | **Campos no obligatorios**, resueltos por el régimen. En su lugar: entidad titular y su identificación del bien, si la tiene |
| **Descargo y baja** (`W-14`, `W-15`) | **Devolución al titular**, con acta. No se descarga un bien que no está en nuestro inventario — ver el hallazgo |
| Constatación física para conciliar contra el registro de bienes | Se hace igual, pero concilia contra el **título de tenencia**, no contra el inventario nacional |

### 5. El costo de tenencia entra al costo por kilómetro, o la comparación no existe

El canon de alquiler es un costo del vehículo tan real como el combustible. Si no se registra, la institución nunca va a poder responder la única pregunta que justifica la decisión: **¿sale más caro alquilar o mantener flota propia?**

El expediente registra el canon con su período y su prorrateo, y **M-13/M-14 acumulan costo total de tenencia por vehículo y por kilómetro**, distinguiendo régimen. Para el comodato el canon es cero y el costo son mantenimiento, combustible y seguro — que no es cero, y que a veces sorprende.

`[C]` **La modalidad de alquiler y su instrumento contractual**. [`NRM-05`](../../01-negocio/normativa/NRM-05-contrataciones-oncae.md) está **fuera del alcance** para la contratación en sí `[V]` — SIGTI no compra nada — pero de esa misma ficha se conserva la implicación de requerimiento `[I]`: *soportar alquiler de vehículos con su contrato, tarifa, período y responsable, aplicándole el mismo control de bitácora y combustible que a la flota propia*. **SIGTI registra el contrato como dato del vehículo; no lo gestiona ni lo licita.**

### 6. Seguro, mantenimiento y multas: se dirigen a quien el contrato diga

**Seguro.** [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) ya prevé la póliza a nombre de tercero y exige registrar al titular. Lo que este caso agrega: **la alerta de vencimiento también se dirige al titular externo**, y el parámetro de bloqueo de `RN-16` —apagado por defecto para flota propia, porque la póliza no es obligatoria por ley vigente `[V]`— **debe poder configurarse con valor distinto por régimen de tenencia**. Circular sin seguro un bien propio es un riesgo; circular sin seguro un bien ajeno que hay que devolver en buen estado es una responsabilidad patrimonial que la institución asume sin haberlo decidido.

**Mantenimiento.** La orden de trabajo de M-11 se dirige según el rubro declarado en el título: si el contrato de alquiler incluye mantenimiento, la orden va **al arrendador** y no consume presupuesto propio. Abrir un correctivo contra el fondo institucional por algo que ya se pagó en el canon es pagar dos veces, y es hallazgo.

**Multas.** Llegan a nombre del **titular**, no de la institución. El expediente de M-12 se abre igual y resuelve el conductor y la misión **a la fecha del hecho de la infracción** ([`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)), porque quien va a reclamar es el titular y va a reclamar con fecha y hora.

### 7. La exoneración de peaje no se hereda. Por defecto, paga

[`RN-38`](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) es explícita: la exoneración es **dato por vehículo, punto, fundamento y vigencia; nunca una constante**. Un fundamento que ampare a vehículos propiedad del Estado **no se traslada automáticamente** a un microbús de placa particular alquilado a una rentadora.

**Postura del sistema: sin exoneración registrada con fundamento y vigencia, el vehículo paga.** No es una interpretación de la norma —la lista oficial de exoneraciones sigue siendo el insumo **#22** `[C]`— es la posición conservadora: estimar de más un peaje cuesta una diferencia en la liquidación; estimar de menos deja al motorista sin efectivo en la caseta.

### 8. Cuando la rentadora cambia la unidad a mitad del contrato

El día 12 llega otro microbús. **No es el mismo vehículo**: otra placa, otra ficha técnica, otro odómetro, y posiblemente **otra categoría de peaje** si cambia la configuración de ejes.

- Se da de alta **como vehículo nuevo en la flota**, con su propio correlativo institucional y su propio expediente, **bajo el mismo título de tenencia** (el contrato es uno).
- **La serie de odómetro no continúa.** Cada vehículo tiene la suya. Empalmarlas produce un salto imposible que va a bloquear [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) y a arruinar el rendimiento de ambos.
- Se registra **acta de sustitución de unidad**, con odómetro y estado de la que sale y de la que entra.
- **Las misiones ya programadas con la unidad saliente se reprograman** (`T-11` → `T-08`), con **revalidación completa** de habilitación del motorista, compatibilidad, capacidad y categoría de peaje ([`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) aplica el mismo criterio a la sustitución de vehículo).
- La unidad saliente pasa a **fin de tenencia**, no a `DADO_DE_BAJA`.

### 9. La devolución cierra el expediente, y sin acta no cierra

Acta de devolución con odómetro fotografiado, estado, inventario de accesorios, novedades y **liquidación de daños si los hay**. Alerta anticipada de la fecha comprometida con escalamiento, igual que el préstamo vencido de [`CE-14`](CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) — porque el comodato de 24 meses que nadie recuerda devolver es el mismo fenómeno con otro nombre.

### 10. Y hay una cosa que nadie ve: ese vehículo es invisible en carretera

El microbús alquilado **no tiene franjas, ni leyenda, ni siglas, ni correlativo pintado**. Para el verificador en carretera (`ACT-15`) es un vehículo particular con gente adentro. Lo único que acredita que se trata de una movilización institucional es **el documento impreso que lleva el motorista**: la Orden de Misión con folio y QR verificable ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)).

Eso hace que M-15 pese **más** en estos vehículos que en la flota propia, no menos. Y es un argumento fuerte para la decisión D-1.

## Las dos preguntas que este caso NO resuelve

[`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) las deja abiertas `[C]` y **no se responden inventando**. Se documentan las posturas provisionales, marcadas como tales.

### D-1 · ¿Aplica la rotulación del Estado a un vehículo en comodato o alquilado? `[C]`

La obligación verificada `[V]` —franjas azul–blanco–azul, leyenda *"PROPIEDAD DEL ESTADO DE HONDURAS"*, siglas y numeración consecutiva— está redactada para vehículos **propiedad del Estado**. Un vehículo alquilado no lo es, y rotularlo podría además incumplir el contrato con la rentadora.

| Opción | Consecuencia | Costo |
|---|---|---|
| **A** — No aplica a comodato ni alquiler | Coherente con la letra de la obligación. Deja circulando vehículos oficiales indistinguibles | Depende enteramente de M-15: sin el documento impreso no hay cómo acreditar nada |
| **B** — Aplica a comodato, no a alquiler de corto plazo | Distingue por permanencia, que es el criterio de sentido común | Requiere definir el umbral de permanencia, que es otro `[C]` |
| **C** — Identificación alterna removible (magnético o adhesivo) con siglas y correlativo | Resuelve la visibilidad sin dañar el bien ajeno ni incumplir contrato | Costo material menor. **No tiene respaldo normativo: es propuesta nuestra, nivel `[I]`** |

**Postura provisional del sistema, y es solo eso**: se registra el estado de rotulación **igual**, con el régimen de tenencia marcado, y la advertencia se emite **con esa aclaración** — que es exactamente lo que [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) ya dispone. **No se convierte en bloqueo mientras la pregunta esté abierta.**

### D-2 · ¿Aplica la prohibición de circular en día u hora inhábil? `[C]`

Aquí el riesgo no es simétrico y hay que decirlo.

- La prohibición y el permiso de la máxima autoridad están verificados `[V]` para vehículos del Estado.
- La Circular STLCC-ONADICI 022-03-2024 `[V]` es sobre **uso indebido de vehículos**, y el uso indebido de un vehículo alquilado con fondos públicos es igual de indebido.
- Se reportan multas de **L 5,000 a L 50,000** más posible decomiso `[P]`, con base legal exacta `[C]`.

**Costo de equivocarse en cada dirección**: exigir un salvoconducto que quizá no era exigible cuesta una firma. No exigirlo cuando sí lo era cuesta la multa, el decomiso de un bien ajeno que hay que devolver, y el hallazgo.

**Postura provisional del sistema, marcada `[I]` y configurable por régimen de tenencia**: se aplica [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) igual que a la flota propia. **Es la posición conservadora, no una afirmación normativa**, y se retira o se confirma cuando el PO obtenga la respuesta de la unidad de Bienes o de la ONADICI.

## Hallazgo — falta el estado terminal de fin de tenencia

El ciclo de vida del vehículo tiene un solo estado terminal, `DADO_DE_BAJA`, alcanzable por `W-14` (descargo) y `W-15` (descargo por irreparable). **Ambos suponen que el bien es de la institución.**

Un pickup devuelto al comodante, un microbús devuelto a la rentadora, o una unidad sustituida por el arrendador a mitad de contrato **no están dados de baja**: salieron de la flota conservando su existencia y su dueño. Declararlos `DADO_DE_BAJA` registra un descargo que nunca ocurrió, sobre un bien que nunca estuvo en el inventario. Es un asiento falso, y es el tipo de asiento falso que el TSC encuentra cruzando el inventario de bienes contra el padrón de flota.

Se reporta a [`docs/03-arquitectura/estados/orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md) —autoridad en transiciones— como **estado terminal nuevo `RETIRADO_DE_FLOTA`**, alcanzable desde `NO_DISPONIBLE`, con causa tipificada (*devolución de comodato · fin de contrato de alquiler · sustitución de unidad por el arrendador · devolución a la institución propietaria*), **acta de devolución con odómetro obligatoria**, y conservación completa del historial: bitácoras, consumos, incidentes y costos del período en que estuvo con nosotros no se van con el vehículo.

## Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:titulo-de-tenencia-con-vigencia` | Todo vehículo de la flota tiene título de tenencia con régimen, titular, documento adjunto y rango de vigencia; sin título vigente no se habilita en la flota |
| `RN-c:mision-dentro-de-la-vigencia-del-titulo` | Ninguna misión se programa ni despacha si su ventana excede la vigencia del título de tenencia del vehículo — bloqueo duro, mismo patrón que `RN-10` |
| `RN-c:responsabilidad-economica-por-rubro-segun-tenencia` | El título declara quién asume combustible, mantenimiento, llantas, seguro, peajes, multas y daños; M-11 y M-13 dirigen la orden y el cargo según ese dato |
| `RN-c:costo-de-tenencia-en-el-costo-por-kilometro` | El canon de alquiler y los costos asociados al comodato se prorratean e integran al costo total por vehículo y por kilómetro, distinguiendo régimen |
| `RN-c:sustitucion-de-unidad-por-el-arrendador` | La unidad sustituta se da de alta como vehículo nuevo bajo el mismo título, con serie de odómetro propia, acta de sustitución y revalidación de las misiones programadas |
| `RN-c:fin-de-tenencia-no-es-descargo` | La salida de un vehículo ajeno de la flota se registra como fin de tenencia con acta de devolución, nunca como descargo o baja — depende del estado terminal nuevo |
| `RN-c:bloqueo-de-seguro-configurable-por-regimen` | El parámetro de bloqueo por póliza vencida admite valor distinto según régimen de tenencia |
| `RN-c:exoneracion-de-peaje-no-se-hereda` | La exoneración exige fundamento y vigencia registrados para **ese** vehículo; sin ellos, paga |

## Evidencia que debe quedar

Ante el TSC o Auditoría Interna:

1. El **título de tenencia** con su documento adjunto: convenio de comodato, contrato de alquiler o acta de donación, con vigencia y titular
2. El **acta de recepción** de la unidad, con odómetro fotografiado, estado, accesorios y documentos recibidos
3. La **bitácora completa del período**, con el mismo nivel de detalle que la flota propia
4. La **imputación de combustible al vehículo correcto**, con su conciliación galonaje–kilometraje propia
5. El **costo de tenencia** acumulado y el costo por kilómetro, con el canon y su prorrateo
6. Las **órdenes de trabajo** dirigidas según el rubro pactado, con constancia de que no se cargó al presupuesto lo que cubría el contrato
7. Los **peajes pagados** con su categoría, su tarifa congelada y la ausencia o presencia de exoneración con fundamento
8. Las **alertas de vencimiento** del título y de la póliza del titular, con su destinatario y su acuse
9. Si hubo sustitución de unidad: el **acta de sustitución** y las dos series de odómetro separadas
10. El **acta de devolución** con odómetro, novedades y liquidación de daños — y el expediente cerrado en `RETIRADO_DE_FLOTA`, no en `DADO_DE_BAJA`
11. La **posición adoptada sobre rotulación y día inhábil**, con el acto de la institución que la respalda una vez resueltas D-1 y D-2

## Trazabilidad

- **Reglas**: [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) aplica a todo régimen · [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) póliza a nombre de tercero · [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) vencimientos · [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) rotulación con `[C]` de régimen · [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) estado operativo · [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) custodia en cualquier régimen · [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) habilitación · [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) compatibilidad y capacidad · [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) día inhábil y documento impreso · [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) combustible y odómetro · [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-38`](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) peajes · [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) vigencia y fecha del hecho · [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) sustitución con revalidación
- **Reglas candidatas**: `RN-c:titulo-de-tenencia-con-vigencia`, `RN-c:mision-dentro-de-la-vigencia-del-titulo`, `RN-c:responsabilidad-economica-por-rubro-segun-tenencia`, `RN-c:costo-de-tenencia-en-el-costo-por-kilometro`, `RN-c:sustitucion-de-unidad-por-el-arrendador`, `RN-c:fin-de-tenencia-no-es-descargo`, `RN-c:bloqueo-de-seguro-configurable-por-regimen`, `RN-c:exoneracion-de-peaje-no-se-hereda`
- **Normas**: [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) — identificación del vehículo `[V]`, prohibición de circulación inhábil `[V]`, **régimen de comodato y alquiler expresamente abierto `[C]`**, ficha maestra del bien `[I]` *(implicación de requerimiento)*, multas L 5,000–50,000 `[P]` · [`NRM-05`](../../01-negocio/normativa/NRM-05-contrataciones-oncae.md) — **contratación fuera de alcance `[V]`**; alquiler con contrato, tarifa y período como dato del vehículo `[I]` · [`NRM-06`](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) licencias y seguro no obligatorio por ley vigente `[V]` · [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md) categorías y exoneraciones `[C]`
- **Estados**: **estado terminal nuevo pendiente** `RETIRADO_DE_FLOTA` con causa tipificada — ver hallazgo · `W-02` habilitar bloqueado sin título vigente · `W-14`, `W-15` **no aplican** a régimen distinto de propio
- **Puntos de control**: `PC-05` vehículo asignable · `PC-06` póliza y revisión · `PC-03` día u hora inhábil · `PC-07` compatibilidad
- **Actores**: `ACT-04` programa y asigna · `ACT-14` Encargado de Bienes, titular del expediente de tenencia · `ACT-13` custodio · `ACT-11` mantenimiento dirigido por rubro · `ACT-08` autoriza el ingreso a flota `[C]` · `ACT-15` verificador en carretera, que no puede identificarlo a la vista
- **Insumos pendientes**: **#55** rotulación en comodato y alquiler (D-1, zona gris de `NRM-02`) · **#56** día inhábil en comodato y alquiler (D-2, zona gris de `NRM-02`) · **#57** modalidad de alquiler, contrato tipo y sustitución de unidad · **#58** cómo se registra hoy la devolución al comodante · **#22** lista oficial de exoneraciones de peaje · **#1** reglamento interno de uso de vehículos · **#34** correlativo institucional único o por delegación
- **Casos especiales relacionados**: [`CE-14`](CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) el préstamo del bien propio — **caso inverso, se confunden constantemente** · [`CE-12`](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) competencia por la flota, donde el alquiler suele ser la salida · [`CE-21`](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) rendimiento contaminado por imputación al vehículo equivocado · [`CE-11`](CE-11-licencia-vence-durante-la-mision.md) vigencia que decae durante la misión, mismo patrón que el título de tenencia
- **Historias candidatas**: `HU-c:registrar-titulo-de-tenencia-de-un-vehiculo`, `HU-c:bloquear-mision-que-excede-la-vigencia-del-titulo`, `HU-c:dirigir-orden-de-trabajo-segun-responsabilidad-contractual`, `HU-c:registrar-sustitucion-de-unidad-por-el-arrendador`, `HU-c:retirar-vehiculo-de-la-flota-por-fin-de-tenencia`, `HU-c:consultar-costo-total-por-kilometro-por-regimen-de-tenencia`
