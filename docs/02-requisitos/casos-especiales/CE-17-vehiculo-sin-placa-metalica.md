# CE-17 — El vehículo lleva dieciocho meses circulando sin placa metálica y hay que despacharlo igual

| Campo | Valor |
|---|---|
| **Módulos** | M-03 Flota, M-04 Documentación y Cumplimiento, M-15 Formatos Oficiales, M-18 Peajes, M-12 Incidentes y Sanciones, M-07 Despacho |
| **Estados afectados** | `PROGRAMADA` (`T-08`), `DESPACHADA` (`T-12`), `LIQUIDADA` (`T-19`) · el expediente del vehículo en cualquier estado operativo |
| **Frecuencia** | Frecuente — hay reportes de miles de vehículos circulando sin placa metálica durante años `[V]` |
| **Impacto** | Operativo, legal y de auditoría |
| **Resolución** | Definida para la identidad, el respaldo documental y la identificación en carretera · `[C]` para el catálogo de documentos sustitutivos y su aceptación en retén |

## La situación

El microbús correlativo `INM-0091`, adquirido en enero de 2026 para la delegación de La Ceiba, se recibió del proveedor **con la placa en trámite**: el Instituto de la Propiedad no tenía láminas que entregar. En marzo de 2026 el Congreso aprobó la compra directa de placas vehiculares para enfrentar el desabastecimiento `[V]`. Hoy, agosto, el microbús sigue sin lámina.

El vehículo está rotulado como manda la norma `[V]`: tres franjas azul–blanco–azul en las puertas laterales, la leyenda **PROPIEDAD DEL ESTADO DE HONDURAS**, las siglas de la institución y el correlativo `0091`. Está matriculado. Tiene número de placa asignado en el registro. **Lo que no tiene es la lámina metálica atornillada.**

Y hay que despacharlo: sale el jueves con nueve personas de la delegación hacia Tocoa, cruza dos puntos de peaje y regresa el viernes por la tarde.

**Cuatro situaciones distintas que hoy se ven todas iguales:**

1. **Nunca tuvo placa.** Vehículo nuevo, matrícula vigente, número de placa asignado en el registro, sin lámina.
2. **Tenía y la perdió.** Una de las dos láminas se desprendió en el tramo de tierra hacia Iriona. Circula con una sola, o con ninguna.
3. **Está en trámite de reposición**, con constancia del IP que tiene su propia fecha de vencimiento — y esa constancia vence antes que la misión regrese.
4. **No tiene ni número de placa asignado.** Es el caso más duro, porque ahí no hay nada que escribir en el estado de cuenta del peaje ni en el acta de un accidente.

La confusión entre **matrícula** (el registro del vehículo) y **placa metálica** (la lámina) es la que hace que estas cuatro se traten como una sola. No son lo mismo, y solo la primera es bloqueante.

## Qué se hace hoy sin sistema

`[C]` La práctica de la institución no está confirmada — insumo #2 (formatos vigentes).

Lo que se observa como práctica común en instituciones públicas hondureñas `[I]`:

- El motorista lleva **la fotocopia del certificado de matrícula y la constancia del trámite en la guantera**, dentro de un fólder plástico. Ese fólder es el vehículo, a efectos de un retén.
- Se pega **una impresión con el número de placa en el parabrisas o en el vidrio trasero**. Nadie sabe si eso vale ante un agente y todos lo hacen.
- El control interno de flota está en una hoja de cálculo **cuya columna llave es la placa**. Los vehículos sin placa aparecen como `EN TRÁMITE`, `S/P`, `PENDIENTE` o en blanco — y como todos comparten valor de llave, **el mismo vehículo aparece dos veces o no aparece**.
- En los reportes internos y en la conversación diaria, el vehículo se identifica por **el número pintado en la puerta**. Ese número — la numeración consecutiva institucional — ya es, de hecho, la identidad real. Solo que ningún papel lo dice.
- Las **multas de tránsito llegan por placa**. Si no hay placa, no llegan; y cuando llegan de una placa reciclada, nadie sabe si corresponden a este vehículo o al que la tuvo antes.
- El **ticket de peaje** se guarda con el número de misión escrito a lapicero al reverso, porque no hay otro modo de saber a qué vehículo pertenece.

**El número pintado en la puerta es la regla que nadie escribió.** La institución ya opera con el correlativo como identidad; lo que falta es que el sistema lo reconozca y deje de tratar la placa como llave.

## Por qué el flujo normal no lo cubre

[`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) ya resuelve lo estructural, y es la decisión más importante del modelo de datos de flota: **el correlativo institucional es obligatorio y único; la placa no es obligatoria ni única** `[V]`. `BD-03` lo confirma en la tabla de documentación: la placa metálica **no bloquea**, y el estado *sin placa metálica* es válido.

Lo que el flujo feliz no cubre es todo lo que viene después de admitir ese estado:

- **No hay catálogo del documento sustitutivo.** `BD-03` exige *"adjunto de constancia o documento sustitutivo del IP"*, pero no dice qué documentos son ni qué se hace cuando el adjunto vence a mitad de la misión.
- **No hay regla que ponga el respaldo en la mano del motorista.** [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) lo menciona como caso límite — *"el sistema debe permitir imprimir la constancia adjunta junto con la orden de misión"* — pero permitir no es exigir, y en el predio a las 5 de la mañana lo que no está en el paquete impreso no viaja.
- **El mundo exterior indexa por placa.** Peajes, multas, pólizas y actas policiales identifican al vehículo por placa. Ninguna regla dice contra qué se resuelve la imputación cuando ese dato no existe. [`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) y [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) presuponen que el paso por caseta se puede atribuir a un vehículo, y esa atribución es exactamente lo que se rompe aquí.
- **La rotulación pasa a ser la única identificación visible**, y hoy es apenas una advertencia con caducidad configurable ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md)).

## Regla de resolución

### 1. La identidad es el correlativo. Siempre, en todas partes

Ya es [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) y no se toca: correlativo institucional obligatorio, único por institución, no reciclable. Toda orden de misión, bitácora, vale, ticket, reporte y pantalla identifica al vehículo por **correlativo + placa si existe + marca/modelo**, en ese orden. **Nunca solo por placa.**

`[C]` Confirmar si la institución numera por delegación o por institución — insumo #34. Si numera por delegación, el identificador único es la composición delegación + número, y esa composición es la que se pinta y la que se imprime.

**Lo que el sistema no hace: inventar una placa provisional.** Un identificador interno con formato de placa termina transcrito en un ticket de peaje o en un acta policial como si lo fuera, y a partir de ahí nadie puede desenredar a qué vehículo corresponde qué. El correlativo debe verse claramente distinto de una placa, y eso es una virtud del diseño, no una limitación.

### 2. La placa es un dato con estado, historial y vigencia — no un texto

| Estado | Qué es cierto | Efecto |
|---|---|---|
| `CON_LAMINA` | Número asignado y lámina física instalada | Ninguno |
| `ASIGNADA_SIN_LAMINA` | El registro le asignó número; la lámina no se ha entregado | Exige documento de respaldo vigente |
| `EN_TRAMITE` | Trámite abierto ante el IP, sin número asignado aún | Exige documento de respaldo vigente **y** expediente del trámite |
| `SIN_PLACA` | Ni número ni lámina, sin trámite abierto | Exige respaldo **y** genera alerta permanente al Jefe de Transporte: es la situación menos defendible |
| `NO_APLICA` | Régimen de tenencia que no lo exige | Exige fundamento registrado |

El **historial de placas con rangos de vigencia** no es un lujo: es la única forma de decidir si la multa del 12 de marzo corresponde a este vehículo o al que tuvo esa placa antes. Ya lo exige [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) como caso límite; aquí es la pieza central.

### 3. Lo que bloquea no es la ausencia de placa: es la ausencia de respaldo

Esta es la inversión que hay que sostener. Un vehículo sin lámina **opera con normalidad**. Un vehículo sin lámina **y sin documento que explique por qué** no se despacha, porque el motorista sale a la carretera sin nada que mostrar.

El documento de respaldo se registra con **emisor, tipo del catálogo, folio, fecha de emisión, fecha de vencimiento si la tiene y adjunto escaneado o fotografiado**. Sin adjunto no hay respaldo: un campo de texto que diga "en trámite" no le sirve a nadie en un retén de la DNVT.

`[C]` **El catálogo exacto de documentos que el IP emite hoy y su vigencia no está tipificado.** [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) los menciona genéricamente como *"documento sustitutivo o constancia del IP"* `[V]` sin nombrarlos. **No se inventa el catálogo aquí**: se crea como catálogo configurable vacío, y la institución lo carga con los documentos que efectivamente recibe. Registrado como **insumo #60**.

La vigencia del respaldo se evalúa contra **todo el rango de la misión**, con el mismo criterio de `BD-02` y `BD-03`: no basta que esté vigente el día de la salida. Y sus vencimientos generan alerta anticipada por [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md), con los umbrales de referencia de 60 / 30 / 15 días `[V]`.

### 4. El motorista no sale con las manos vacías

El **paquete de identificación en carretera** se imprime y se entrega al despachar (`T-12`), y su entrega se acusa. Para un vehículo sin lámina contiene, además de la orden de misión y del salvoconducto si la ventana lo requiere:

- **Copia impresa del documento de respaldo vigente**, con su adjunto reproducido.
- **Ficha de identificación del vehículo**: correlativo institucional, marca, modelo, año, color, número de chasis/VIN, número de motor, número de bien del inventario nacional y número de placa asignado si existe.
- **Folio único y QR verificable** — [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md). El QR resuelve a la ficha pública mínima del vehículo: correlativo, institución y vigencia del respaldo. **Sin datos personales del motorista ni de los pasajeros** ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)).

El QR es lo que separa un papel fotocopiable de algo que el agente puede verificar en el momento. La premisa rectora 4 lo dice: el control en carretera es físico, y el híbrido digital-papel es diseño, no parche.

`[C]` **No consta que la DNVT acepte la constancia del IP en un retén.** [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) no lo resuelve, y este análisis **no lo afirma**. Lo que el sistema garantiza no es la aceptación: es que el motorista **portaba** el respaldo, que se le entregó, y que queda constancia con marca de tiempo de quién se lo entregó. Eso es lo que defiende a la institución si el vehículo termina en un acta.

### 5. Sin placa, la rotulación es la única identificación visible

[NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) exige `[V]` franjas, leyenda, siglas y numeración consecutiva, y es hallazgo de auditoría frecuente que se verifica físicamente en operativos.

En un vehículo con lámina, la rotulación borrosa es una observación. **En un vehículo sin lámina, la rotulación es lo único que dice que es del Estado.** Por eso el umbral de caducidad de la constatación de [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) debe ser **más corto** para vehículos sin lámina, y la constatación con fecha y fotografía pasa de conveniente a necesaria.

`[C]` **Cómo se rotula una motocicleta** — insumo #43. El Acuerdo 303 describe franjas en *puertas laterales*, que una moto no tiene. Y las motocicletas son precisamente las que más frecuentemente circulan sin placa ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md), casos límite). La combinación *sin placa y sin rotulación aplicable* deja una moto del Estado sin ninguna identificación externa, y ese es un caso abierto que debe resolver el PO.

### 6. Jerarquía de anclas para todo lo que viene de afuera

Peajes, multas, pólizas, siniestros y actas indexan por placa. Cuando la placa no existe o cambió, la imputación se resuelve contra el vehículo por una **jerarquía declarada**, en este orden:

| Orden | Ancla | Nota |
|---|---|---|
| 1 | **Correlativo institucional** | Siempre existe. Es el ancla interna |
| 2 | **Chasis / VIN** | Ancla física. Es lo que usa la aseguradora cuando no hay placa. [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) ya lo exige en la ficha maestra `[V]` |
| 3 | **Número de bien del inventario nacional** | Ancla contable ante Bienes Nacionales y el TSC |
| 4 | **Identificador del tag de peaje**, si existe | `[C]` insumo #24: ¿la institución tiene tags y a nombre de quién se emiten? |
| 5 | **Placa vigente a la fecha del hecho** | Resuelta contra el historial, no contra el valor actual — [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) |

La placa es el **último** ancla, no el primero. Y cuando se usa, se resuelve **a la fecha del hecho**: una multa de marzo se imputa contra la placa que el vehículo tenía en marzo, no contra la que tiene hoy.

**Peajes sin placa.** [`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) y [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) necesitan atribuir el paso a un vehículo. Si el descargo se hace con **ticket físico**, el ticket se fotografía y se vincula a la Orden de Misión desde el cliente de campo ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)) — la misión es el ancla, y el vehículo se deriva de ella. Si se hace con **estado de cuenta de tag**, el tag debe estar asociado al correlativo en la ficha del vehículo. `[C]` insumo #24: sin esa respuesta, el descargo de peajes de un vehículo sin placa no es defendible ante el TSC.

**Multas.** [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) exige `[V]` registrar infracciones asociadas al vehículo y al motorista, con estado de pago y quién asume el costo. Una multa recibida sin placa se registra igual, imputada por la jerarquía anterior, y si no se puede imputar con certeza queda como **imputación no resuelta** con responsable asignado. Nunca se asigna al vehículo por parecido.

### Lo que hay que confirmar

- `[C]` **Catálogo de documentos sustitutivos que emite el IP** y su vigencia. No tipificado en [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md). **Insumo #60** — bloqueante para configurar el catálogo, no para el diseño.
- `[C]` **¿La DNVT acepta el documento sustitutivo en un retén?** No consta — insumo #61. Opciones: (a) asumir que sí y no imprimir nada — costo: el motorista queda expuesto y la institución sin evidencia; (b) imprimir siempre el paquete, costo marginal de papel. **Recomendación: (b)**, es la opción conservadora y la única que produce evidencia.
- `[C]` **¿Tiene la institución tags de peaje y a nombre de quién?** — insumo #24.
- `[C]` **Rotulación de motocicletas** — insumo #43.
- `[C]` **Régimen de vehículos en comodato o alquilados** — [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), zona gris abierta. Un vehículo alquilado circula con placa particular y sin franjas: no le aplica nada de este caso, o le aplica todo. Hoy no se sabe cuál.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C17a` | *El estado de la placa es un dato tipificado con historial y rangos de vigencia, que distingue el número asignado en el registro de la presencia física de la lámina.* | [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) admite el estado *sin placa* pero **no lo tipifica**, y esa distinción es la que decide qué se imprime y contra qué se concilia |
| `RN-C17b` | *El estado sin lámina exige documento de respaldo del catálogo, con emisor, folio, adjunto y vigencia cubriendo todo el rango de la misión. Lo que bloquea el despacho no es la ausencia de placa, sino la ausencia de respaldo.* | `BD-03` exige el adjunto pero no fija su vigencia ni qué ocurre cuando vence a mitad del rango |
| `RN-C17c` | *El despacho de un vehículo sin lámina entrega e imprime el paquete de identificación en carretera con folio y QR verificable, y la entrega se acusa.* | [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) lo **permite**; nadie lo **exige**. Lo que no está en el paquete impreso no viaja |
| `RN-C17d` | *Toda imputación externa — peaje, multa, siniestro, póliza — se resuelve contra el vehículo por la jerarquía de anclas declarada, con la placa en último lugar y resuelta a la fecha del hecho.* | [`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) y [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) presuponen la atribución; nada dice cómo se hace sin placa |
| `RN-C17e` | *El umbral de caducidad de la constatación de rotulación es más corto para vehículos sin lámina, donde la rotulación es la única identificación visible del bien del Estado.* | [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) usa un umbral único para toda la flota |

## Evidencia que debe quedar

Ante una auditoría, la institución debe poder mostrar:

1. La **ficha del vehículo** con correlativo institucional, chasis/VIN, número de motor y número de bien del inventario nacional — los anclas que no dependen de la placa.
2. El **historial de placas con rangos de vigencia**, que permite responder a qué vehículo corresponde una multa, un ticket o un acta de una fecha determinada.
3. El **documento de respaldo vigente a la fecha de cada misión**, con su adjunto, emisor, folio y fechas — y la demostración de que la vigencia se evaluó **a la fecha del hecho**, no a la de la consulta.
4. La **constancia de entrega del paquete de identificación** al motorista en cada despacho, con folio, QR emitido y acuse con marca de tiempo.
5. La **constatación de rotulación** con fecha y fotografía, dentro del umbral vigente ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md)).
6. El **expediente del trámite ante el IP**: fecha de inicio, gestiones realizadas y su resultado. Es lo que responde a la pregunta que el auditor va a hacer — *¿por qué este vehículo lleva dieciocho meses sin placa?* — con una gestión documentada en vez de un encogimiento de hombros.
7. Las **advertencias de placa duplicada** que se hayan producido, con el motivo por el que se guardó igual ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md)).
8. Las **imputaciones externas no resueltas** — multas o pasos por caseta que no se pudieron atribuir — con su responsable asignado y su estado. Una imputación sin resolver documentada es defendible; una imputación asignada por parecido no lo es.

## Trazabilidad

- **Autoridad de bloqueos:** `BD-03` de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — *"Placa metálica: no bloquea. Sin placa metálica es estado válido"* `[V]`
- **Reglas:** [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md), [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md), [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-36`](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md), [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)
- **Reglas candidatas:** `RN-C17a` a `RN-C17e`
- **Puntos de control:** `PC-05` de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) — documentación del vehículo en asignación y despacho
- **Normas:** [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[V]` desabastecimiento de placas y compra directa de marzo 2026; `[V]` obligación de registrar matrícula y tolerar el estado sin placa · [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` identificación obligatoria del vehículo del Estado y numeración consecutiva
- **Premisas rectoras:** 3 (trazabilidad inmutable), 4 (híbrido digital-papel por diseño)
- **Actores:** ACT-01, ACT-04, ACT-05, ACT-06, ACT-13, ACT-14
- **Casos relacionados:** [CE-03](CE-03-accidente-de-transito-en-mision.md) (acta e imputación tras accidente), [CE-16](CE-16-vehiculo-a-taller-con-misiones-programadas.md) (recálculo de peaje al sustituir vehículo)
- **Insumos:** #2 (formatos en papel), #24 (tags de peaje y facturación), #34 (correlativo por institución o por delegación), #43 (rotulación de motocicletas), #60 (catálogo de documentos sustitutivos del IP), #61 (aceptación del documento sustitutivo en retén)
