# Glosario del dominio

Fuente de verdad para nombrar entidades, campos, pantallas y botones. **Si un término no está aquí, no se usa en un artefacto** — primero entra al glosario.

Razón: el personal de la institución lleva años llenando formatos en papel con términos precisos. Si el sistema los renombra, la adopción falla — no por la funcionalidad, sino por el vocabulario.

---

## 1. La movilización

### Orden de Misión
**La entidad central del sistema.** Documento que autoriza y respalda una movilización institucional: qué se traslada, con qué vehículo, quién lo conduce, hacia dónde, cuándo, y con cargo a qué. Es la **unidad de control administrativo-contable**: todo consumo, gasto y evidencia cuelga de ella.

No es un "viaje". Un viaje es el hecho físico; la Orden de Misión es el expediente que lo autoriza, lo registra y lo cierra.

### Solicitud de Transporte
La necesidad expresada por un empleado antes de que exista Orden de Misión. Captura **el objeto del traslado** —personas, carga o mixto—, origen, destino, fechas y el tipo de vehículo requerido. Una solicitud aprobada y programada se convierte en Orden de Misión.

### Objeto del traslado
Qué se moviliza. Tres tipos: **personas** (personal institucional o externas), **carga** (equipos, herramientas, insumos, materiales) o **mixto**. Determina la compatibilidad con el tipo de vehículo.

### Misión
Uso coloquial de la Orden de Misión ya en ejecución. Aceptable en conversación e interfaz; en documentos formales se usa el término completo.

### Tramo
Segmento de una misión ejecutado con un vehículo y motorista determinados. Una misión tiene un solo tramo salvo que ocurra una sustitución en ruta —por avería, por ejemplo—, en cuyo caso el consumo y el kilometraje se imputan a cada vehículo por separado.

### Multi-destino
Misión con más de un punto de arribo. Cada destino tiene su propio arribo, tiempo en sitio y salida.

### Tiempo de espera en sitio
Período en que el vehículo permanece detenido en un destino sin operar. Se mide, porque es capacidad ociosa de la flota y porque explica desviaciones de tiempo respecto a lo programado.

### Despacho
Acto de autorizar la salida física del vehículo. Quien despacha verifica que la Orden de Misión esté completa, que el motorista esté habilitado y que la documentación del vehículo esté vigente.

### Bitácora
Registro cronológico de lo que realmente ocurrió durante la misión: salida, odómetros, paradas, arribos, eventos en ruta, entregas, incidentes y retorno. Es el documento que el auditor cruza contra el consumo.

En papel existe hoy como formato preimpreso. **La pantalla debe reproducirlo campo por campo.**

---

## 2. Las personas

### Motorista
Quien conduce el vehículo institucional **y pertenece al padrón de motoristas**. Es el término del dominio hondureño, y el que va en la interfaz. No se usa "chofer".

Como empleado pertenece a Talento Humano; como recurso de la flota pertenece a SIGTI.

### Quien conduce
**No es sinónimo de motorista, y la diferencia importa.**

`RN-57` verifica la habilitación sobre **quien efectivamente conduce**, cualquiera sea su puesto — y esa persona **puede no estar en el padrón**: el funcionario con vehículo asignado permanentemente, o quien releva en una emergencia.

Todo motorista es quien conduce; **no todo quien conduce es motorista**. En la interfaz se dice *"quien conduce"* o *"conductor declarado"* cuando el padrón no aplica, y **motorista** cuando sí.

> **Distinción incorporada tras el hallazgo 5.1 de la entrega de diseño.** El diseñador notó que el inventario y el mapa de navegación usaban "conductor" donde el glosario lo prohibía, y planteó la duda correcta: *o se renombran esas pantallas, o "conductor" tiene ahí un sentido distinto que el glosario debería recoger.*
>
> Era lo segundo. La prohibición original era correcta como sustituto de *motorista*, pero borraba una distinción que `RN-57` necesita — y que apareció justamente al cubrir el hueco de `CE-19`, donde el funcionario que conduce su propio vehículo no lo alcanzaba ninguna verificación de licencia.

### Jefatura inmediata
Superior jerárquico directo del solicitante, quien autoriza la solicitud en primera instancia.

### Gerencia Administrativa
Unidad que administra los recursos de la institución. Aprueba el fondo de combustible y es interlocutor de Auditoría.

### Máxima autoridad
Titular de la institución — Secretario del ramo, o Presidente o Gerente en descentralizadas. Es quien firma el permiso de circulación en día u hora inhábil.

### Custodio
Servidor a cuyo cargo queda un vehículo, con acta de entrega-recepción firmada. Responde por su uso y conservación. No es lo mismo que el motorista de una misión puntual.

### Dependencia
Unidad organizativa de la institución (dirección, gerencia, departamento). Define el alcance de datos de la mayoría de los roles.

### Delegación
Oficina de la institución fuera de la sede central, típicamente regional o fronteriza. Es donde la conectividad falla y donde la operación en papel sigue viva.

---

## 3. El vehículo

### Expediente del vehículo
Conjunto completo de información y ciclo de vida de un vehículo: identidad, ficha técnica, régimen de tenencia, documentación y vencimientos, seguro, revisión, mantenimiento, fallas, incidentes, custodios y asignaciones. **No es un catálogo: es un expediente**, en el mismo sentido en que Talento Humano lleva el expediente de un empleado.

### Ficha técnica
Atributos físicos del vehículo: tipo, marca, modelo, año, número de motor, chasis o VIN, combustible, **peso bruto vehicular**, número de ejes, capacidad de pasajeros, capacidad de carga, si es articulado. De ella se derivan la categoría de licencia requerida y la categoría de peaje.

### Peso bruto vehicular
Peso máximo autorizado del vehículo cargado, en kilogramos. Determina qué categoría de licencia habilita a conducirlo (`C1` hasta 7,500 kg; `C` por encima).

### Placa vs. matrícula
**Matrícula** es el registro del vehículo ante el Instituto de la Propiedad. **Placa** es la lámina metálica.

**No son lo mismo, y en Honduras un vehículo puede estar matriculado sin tener placa metálica** por el desabastecimiento nacional. Por eso `placa` no puede ser campo obligatorio ni único.

### Correlativo institucional
Numeración consecutiva que la institución asigna a cada vehículo, rotulada en la carrocería. Es el identificador que el personal usa en el día a día, más que la placa.

### Rotulación
Identificación obligatoria del vehículo del Estado: franjas azul–blanco–azul de 10 cm en las puertas, leyenda "PROPIEDAD DEL ESTADO DE HONDURAS" en letras de 2.54 cm, siglas de la institución y correlativo. **Es hallazgo frecuente de auditoría**, por eso se verifica con fecha y fotografía.

### Régimen de tenencia
Cómo la institución dispone del vehículo: propio, en **comodato** (cedido sin costo por otra entidad), alquilado, o donado.

### Vehículo de pool
Vehículo disponible para cualquier dependencia según programación, en contraposición al asignado permanentemente a un funcionario o unidad.

### Estado operativo
Situación del vehículo respecto a su disponibilidad: disponible, asignado, en misión, en taller, no disponible, dado de baja.

### Descargo
Baja formal de un bien del inventario de la institución, con acta y resolución. Se usa por desuso, siniestro total, robo o disposición final.

### Tarjeta de responsabilidad
Documento que formaliza la asignación de un bien a un servidor, quien responde por él. Se firma en la entrega y en cada cambio de custodio.

### Constatación física
Verificación presencial de que el vehículo existe, está donde dice el registro y en el estado declarado. Se documenta con acta, fotografía, odómetro y ubicación.

---

## 4. Habilitación para conducir

### Categoría de licencia
Clasificación que determina qué vehículos habilita a conducir una licencia. En Honduras: **A** (motocicletas), **B** (automóviles livianos), **B1** (triciclos y cuadriciclos), **C1** (carga hasta 7,500 kg), **C** (carga sobre 7,500 kg, no articulado), **D1** (autobuses hasta 25 pasajeros), **D** (autobuses), **CE** (furgón articulado).

### Matriz licencia ↔ vehículo
Tabla que resuelve qué categoría de licencia habilita a conducir qué vehículo, según su tipo y peso bruto. **Es la validación de mayor valor legal del sistema**: asignar un motorista sin licencia habilitante traslada responsabilidad directa a quien autorizó.

### Restricción médica
Condición anotada en la licencia que limita cuándo o cómo puede conducir su titular (uso de lentes, prohibición de conducción nocturna, etc.).

---

## 5. Combustible

### Fondo de combustible
Monto en efectivo u órdenes de pago que **Gerencia Administrativa aprueba** a solicitud del Jefe de Transporte, para cubrir el combustible de la operación de un período. **SIGTI no compra combustible**: gestiona la asignación y el consumo de este fondo.

### Orden de pago
Instrumento con el que la institución entrega el fondo. Tiene folio y responsable.

### Vale de combustible
Documento con folio que ampara la entrega de combustible a un motorista para una misión. Ciclo de vida: emitido → entregado con firma → canjeado con comprobante → conciliado; o bien anulado o extraviado con acta.

Es el mecanismo tradicional en el sector público hondureño. El modelo debe admitirlo, sin cerrarse a una tarjeta de flota futura.

### Requisición
Solicitud formal interna de un bien o insumo. En este dominio, la solicitud de combustible que precede a la entrega del vale.

### Odómetro
Instrumento que marca el kilometraje acumulado del vehículo. Se registra obligatoriamente a la salida y al retorno.

### Rendimiento
Kilómetros recorridos por galón consumido. Se compara contra el rendimiento esperado del vehículo para detectar desviaciones. **Una desviación hacia arriba también es hallazgo**: un rendimiento imposiblemente bueno suele significar un despacho de combustible no registrado.

---

## 6. Peajes

### Caseta o punto de peaje
Instalación donde se cobra por el uso de una carretera concesionada. En Honduras operan tres sobre la CA-5 Norte: **Zambrano**, **Siguatepeque** y **Yojoa**, a cargo de COVI-H.

### Categoría de peaje
Clasificación que determina la tarifa. **No se resuelve solo por número de ejes**: "Liviano/Turismo" y "Vehículo de 2 Ejes" tienen ambos dos ejes y pagan tarifas muy distintas. El discriminante es el tipo y peso del vehículo. Se **deriva de la ficha técnica**.

### CoviPass
Sistema de telepeaje prepago con TAG por radiofrecuencia operado por COVI-H. Permite el paso sin detenerse. No es obligatorio.

### Discrepancia de clasificación
Situación en que la caseta cobra una categoría distinta a la que corresponde al vehículo. Ocurre con panels y microbuses, y ya obligó a la SAPP a resolver por comunicado. Se registra con el ticket para respaldar el reclamo.

---

## 7. Control, cierre y auditoría

### Liquidación
Cierre económico de la misión: se concilia lo asignado contra lo consumido, se devuelve el saldo y se documenta cada gasto con su comprobante.

**Cuidado con el término:** en el sector público hondureño "liquidación" suele referirse a viáticos. En SIGTI se refiere al **fondo de combustible y peajes**. Los viáticos son de ARGOS.

### Conciliación
Cruce entre registros de distinto origen para detectar diferencias: galones despachados vs. galones consumidos vs. kilómetros recorridos vs. rendimiento esperado; peaje estimado vs. peaje pagado.

### Hallazgo
Diferencia o irregularidad detectada que requiere explicación. Puede originarse en la conciliación del sistema o en una auditoría. Una misión con hallazgo abierto no puede cerrarse normalmente: cierra como `CERRADA_CON_HALLAZGO`.

### Asiento reverso
Corrección de un registro mediante un asiento que lo anula, **conservando el original**. En SIGTI nada se borra ni se edita después de cerrado: se reversa, con motivo y autor.

### Bitácora de auditoría
Registro inmutable de toda operación del sistema: quién, qué, cuándo, desde dónde, valor anterior y valor nuevo. Es distinta de la bitácora del viaje.

### Folio
Número único y correlativo de un documento oficial. En SIGTI los documentos llevan folio, código QR de verificación y hash del documento electrónico.

### Salvoconducto
Documento impreso que autoriza la circulación de un vehículo del Estado en día u hora inhábil, firmado por la máxima autoridad. **El motorista debe portarlo**: el control en carretera es físico.

### Día y hora inhábil
Fuera del horario laboral, fines de semana y feriados. Circular en ese tiempo requiere permiso de la máxima autoridad, salvo servicios exceptuados (emergencia, seguridad, salud).

### Segregación de funciones
Principio de control interno según el cual quien solicita ≠ quien autoriza ≠ quien despacha ≠ quien entrega combustible ≠ quien liquida. Exigido por el MARCI. **En SIGTI es bloqueo duro, no advertencia.**

---

## 8. Presupuesto — términos que llegan desde ARGOS

### Unidad ejecutora
Nivel de la estructura presupuestaria que ejecuta el gasto.

### Objeto del gasto
Clasificación presupuestaria de en qué se gasta (combustible, mantenimiento, etc.).

### Viático
Asignación por permanecer fuera de la sede cumpliendo funciones. **Lo maneja ARGOS, no SIGTI.** Se incluye aquí solo para que nadie lo confunda con el fondo de combustible.

---

## 9. Integración y campo

### Espejo
Copia local de datos que pertenecen a otro sistema (ARGOS o Talento Humano). **De solo lectura desde SIGTI.** Se carga inicialmente por API y se mantiene con webhooks más reconciliación periódica.

### Fecha del hecho vs. fecha de captura
La primera es cuándo ocurrió; la segunda, cuándo se registró en el sistema. **Se guardan ambas, siempre.** Un registro de carretera puede capturarse tres días después, y eso es normal, no un error.

### Digitación diferida
Captura en el sistema de un formulario que se llenó en papel, con constancia de quién digitó, cuándo, y adjunto del original escaneado o fotografiado.

### Cola de conflictos
Lista de registros donde dos versiones entraron en conflicto al sincronizar. Se resuelven **por decisión humana**, nunca por sobrescritura automática.

---

## 10. Siglas

| Sigla | Significado |
|---|---|
| **ARGOS** | Sistema institucional de viáticos, presupuesto y autorizaciones. Sistema hermano de SIGTI |
| **TSC** | Tribunal Superior de Cuentas |
| **MARCI** | Marco Rector del Control Interno Institucional |
| **ONADICI** | Oficina Nacional de Desarrollo Integral del Control Interno |
| **SEFIN** | Secretaría de Finanzas |
| **SIAFI** | Sistema de Administración Financiera Integrada |
| **ONCAE** | Oficina Normativa de Contratación y Adquisiciones del Estado |
| **IAIP** | Instituto de Acceso a la Información Pública |
| **DNVT** | Dirección Nacional de Vialidad y Transporte — administra las licencias |
| **IP** | Instituto de la Propiedad — Registro Vehicular y placas |
| **IHTT** | Instituto Hondureño del Transporte Terrestre |
| **SAPP** | Superintendencia de Alianza Público-Privada — regula las concesiones viales |
| **SIT** | Secretaría de Infraestructura y Transporte — autoridad concedente |
| **COVI-H** | Concesionaria Vial Honduras — opera los peajes de la CA-5 Norte |
| **INE** | Instituto Nacional de Estadística |

---

## 11. Los estados de la Orden de Misión

> **Incorporado tras el hallazgo `HB1-26`.** Este glosario se declara *«fuente de verdad para nombrar entidades, campos, pantallas y botones»* y añade que *«si un término no está aquí, no se usa en un artefacto»*. **No definía ninguno de los diez estados del ciclo de vida**, así que por su propia regla ni `APROBADA` podía escribirse en ninguna parte.
>
> La definición **normativa** de cada estado —qué se puede y qué no, sus invariantes— está en [`orden-de-mision.md` §3](../03-arquitectura/estados/orden-de-mision.md), que es la **autoridad**. Lo de aquí es el vocabulario: cómo se nombra y qué significa para quien lo lee en pantalla. Si divergen, manda la máquina de estados.

| Estado | Qué significa para quien lo ve |
|---|---|
| `BORRADOR` | La necesidad se está capturando. Todavía no la ha visto nadie más y no compromete nada |
| `SOLICITADA` | Presentada y esperando pronunciamiento de la jefatura. **El contenido ya está congelado**: lo que se apruebe es exactamente lo que se envió |
| `APROBADA` | La jefatura la autorizó. **No reserva vehículo ni motorista**: autorizar la pertinencia del viaje no es lo mismo que comprometer la flota |
| `PROGRAMADA` | Tiene vehículo y motorista asignados y reservados, y folio reservado. Todavía no salió |
| `DESPACHADA` | Documentos emitidos, folio consumido, custodia del vehículo entregada al motorista. El vehículo está por salir |
| `EN_RUTA` | Salió. La autoridad del expediente vive en el dispositivo que porta el motorista |
| `RETORNADA` | El vehículo volvió y la bitácora está cerrada. **La ejecución no se reabre** |
| `LIQUIDADA` | El descargo conciliado está hecho: fondo, combustible, peajes y kilometraje cuadran o tienen su desviación explicada |
| `CERRADA` | El expediente terminó sin observaciones y es inmutable |
| `CERRADA_CON_HALLAZGO` | Terminó completo, **con una observación que el control interno debe seguir**. No imputa responsabilidad ni sanciona: un vehículo robado en ruta produce hallazgo y nadie es culpable |

Y las dos ramas que no son terminales del camino normal:

| Estado | Qué significa |
|---|---|
| `RECHAZADA` | La jefatura no la autorizó, con motivo obligatorio. No se reabre: se presenta una nueva |
| `ANULADA` | Se dejó sin efecto con motivo tipificado y autor. **Nunca se borra.** Una misión que ya salió no se anula — se liquida |

### Términos del ciclo que se usan mucho y no estaban definidos

| Término | Qué es |
|---|---|
| **Ventana efectiva** | El rango de la misión **más las holguras** previa y posterior. Es contra ésta que se valida la licencia, no contra la fecha de salida |
| **Holgura** | Los días de margen que se suman antes y después de la ventana solicitada, para reservas y para vigencia documental |
| **Paquete normativo congelado** | El conjunto de tablas —tarifas, calendario, matriz de licencias, rendimientos, umbrales— con que se calcula una misión, fijado al despachar. Aunque las tablas cambien mientras el vehículo está en ruta, esa misión sigue usando las suyas |
| **Dispositivo portador** | El equipo designado que lleva el expediente a campo. Hay **exactamente uno** por misión despachada, y es donde reside la autoridad del dato mientras dura la ejecución |
| **Sobregiro** | Consumo por encima del fondo asignado |
| **Cadena divergente** | Dos registros del mismo hecho que no coinciden al sincronizar. No se resuelve descartando en silencio |
| **Régimen de excepción** | El levantamiento declarado de incompatibilidades de segregación para delegaciones sin personal suficiente. ⛔ **Diseñado y no implementado** — ver [`DP-002`](../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md) |

### Sobre `approve` en la lista de términos prohibidos

Lo prohibido es **`approve` en inglés**, no el estado `APROBADA`. La prohibición apunta a que nadie escriba *«approve»* en una pantalla o en un campo; el estado canónico del ciclo de vida se llama `APROBADA` y así se escribe. El verbo que se usa en la interfaz es **autorizar**.

---

## Términos prohibidos

No se usan en artefactos, código ni interfaz. La columna derecha es lo que se usa en su lugar.

| No usar | Usar |
|---|---|
| driver, chofer | **motorista** |
| conductor, como sustituto de *motorista* | **motorista** — pero ver "Quien conduce": el término sí es válido cuando el padrón no aplica |
| trip, viaje (como entidad) | **Orden de Misión** |
| request | **solicitud** |
| log, logbook | **bitácora** |
| toll | **peaje** |
| fuel voucher | **vale de combustible** |
| department (por unidad interna) | **dependencia** |
| branch, sucursal | **delegación** |
| approve (como estado) | **autorizar** |
| mission order | **Orden de Misión** |

**Nombres de archivo** en kebab-case, sin tildes ni ñ. **Contenido** con tildes correctas.
