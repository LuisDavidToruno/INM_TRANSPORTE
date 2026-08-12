# CE-18 — Van seis personas y el mobiliario de la delegación, y en el vehículo solo cabe una de las dos cosas

| Campo | Valor |
|---|---|
| **Módulos** | M-06 Solicitudes, M-07 Programación y Despacho, M-02 Catálogos, M-03 Flota, M-17 Traslado de Personas Externas, M-08 Ejecución, M-13 Liquidación |
| **Estados afectados** | `BORRADOR → SOLICITADA` (`T-02`, `BD-09`), `APROBADA → PROGRAMADA` (`T-08`, `BD-07`), `PROGRAMADA → DESPACHADA` (`T-12`), novedad en `EN_RUTA` |
| **Frecuencia** | Frecuente — es el caso ordinario de una institución con delegaciones y presupuesto de combustible ajustado |
| **Impacto** | Operativo, legal y de auditoría — y de riesgo de vida |
| **Resolución** | Definida para la evaluación, las salidas y el orden de reducción · `[C]` para convoy bajo una misma orden y para carga peligrosa |

## La situación

La Dirección de Delegaciones solicita una misión de Tegucigalpa a Trojes, El Paraíso, pasando por Danlí: hay que instalar la oficina de la delegación. En la solicitud, el objeto del traslado son dos cosas a la vez:

- **Seis personas**: cuatro de la dirección, el técnico de informática y el motorista.
- **Carga**: tres escritorios metálicos y dos archivadores — unos 400 kg — más catorce cajas de expedientes.

El único vehículo disponible con tracción para el tramo a Trojes es el pickup doble cabina 4x4 correlativo `INM-0055`. Su ficha técnica dice: **cinco plazas homologadas en cabina, incluido el motorista**, y **800 kg de capacidad de carga en la paila**.

Por separado, las dos cosas caben. Juntas no: sobra una persona, y esa persona no tiene dónde ir salvo la paila, encima de los escritorios. Eso es exactamente lo que se hace hoy y exactamente lo que produce lesionados.

**Y hay cinco variantes más, todas ordinarias:**

1. **Compiten por naturaleza, no por espacio.** Hay que llevar cuatro bidones de combustible a la planta eléctrica de la delegación **y** cuatro personas. El peso alcanza de sobra. La incompatibilidad es de otra clase.
2. **La carga aparece en el predio, a las cinco de la mañana.** La orden decía seis personas. Alguien baja con dos cajas de equipo de cómputo "que ya que van". El motorista no va a discutir con un director.
3. **Multi-destino que cambia la ocupación en cada tramo.** El mobiliario se entrega en Trojes y libera la paila. Pero de regreso hay que traer de Danlí un generador dañado que no estaba en la solicitud. La ida cumple. El retorno no.
4. **Personas externas junto con personal de la institución.** En el retorno viajan dos personas ajenas a la institución. Son otro objeto de traslado, con manifiesto, minimización de datos y registro de consultas propios.
5. **Peso desconocido.** La solicitud dice *"catorce cajas de expedientes"*. Nadie sabe cuánto pesan y nadie las va a pesar.

## Qué se hace hoy sin sistema

`[C]` La práctica de la institución no está confirmada — insumo #2 (formatos vigentes).

Lo que se observa como práctica común en instituciones públicas hondureñas `[I]`:

- La **requisición de vehículo en papel** tiene casilla para *número de personas* y una línea para *descripción de la carga*. **No tiene casilla para peso ni para volumen.** Esa ausencia es la razón por la que nadie los declara: no hay dónde escribirlos.
- La línea de carga se llena con *"materiales varios"* o *"equipo de oficina"*. Nadie firma un peso, y por lo tanto nadie es responsable de un exceso.
- **La decisión de si cabe la toma el motorista en el predio, a ojo.** Es la persona con menos jerarquía de todas las involucradas y la única que ve el problema.
- Si no cabe, se amarra con lazo y se va. **Y las personas que no caben en la cabina viajan en la paila.**
- Cuando hay accidente con un lesionado en la paila, la responsabilidad no se queda en el motorista: recae sobre quien autorizó la misión — y no hay ningún documento donde conste que alguien evaluó la capacidad.

**El formato en papel es un documento de requisitos, y aquí lo es por lo que no tiene.** La casilla de peso que nunca existió es la regla que nadie escribió: la institución nunca decidió que el peso no importaba; simplemente nunca tuvo dónde anotarlo.

## Por qué el flujo normal no lo cubre

[`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) y [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) bloquean, y bloquean bien. [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) incluso nombra este caso literalmente: *"la paila no es capacidad de pasajeros… si alguien pretende trasladar personas en la paila, la regla debe bloquear"*.

El problema no es que falte el bloqueo. Es esto:

- **El bloqueo llega sin salida.** La necesidad es legítima: hay que instalar una delegación y hay un solo pickup con tracción. Si el sistema dice *"no se puede"* y no ofrece un camino, la misión sale igual — fuera del sistema, con la orden tachada o sin orden. Ese es el modo real de fallar de este caso, y es peor que no tener sistema.
- **Ninguna regla resuelve la competencia entre objetos.** [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) evalúa *tipo de vehículo × tipo de objeto*. [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) evalúa cantidades contra la ficha técnica. **Nada evalúa un objeto contra otro**, ni decide cuál cede cuando ambos caben por separado y no juntos.
- **La incompatibilidad por naturaleza no tiene matriz.** Bidones de combustible junto a personas es el ejemplo que la propia [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) usa en su justificación — y sin embargo su matriz no tiene forma de expresarlo, porque solo cruza vehículo contra objeto.
- Aquí se pone a prueba la **premisa rectora 1** del producto: el sistema no gestiona viajes de personas, gestiona **movilizaciones de recursos institucionales**, y lo trasladado puede ser personal, personas externas, carga o una combinación. Si la combinación se evalúa como si fuera un solo objeto homogéneo, la premisa está escrita pero no implementada.

## Regla de resolución

### 1. El objeto del traslado es una lista, no un campo

La solicitud declara **uno o más objetos de traslado**, cada uno con su ficha propia:

| Objeto | Datos mínimos obligatorios |
|---|---|
| Personal de la institución | Número de personas, y condiciones especiales si las hay |
| Personas externas | Número, datos mínimos del catálogo autorizado ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)) y manifiesto ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)) |
| Carga | Tipo del catálogo, peso cierto o estimado por rango, volumen, y condiciones que requiera: sujeción, ventilación, refrigeración, autorización especial |
| El propio vehículo | Traslado a taller o entre delegaciones — no evalúa compatibilidad ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), condiciones de aplicación) |

El conteo de personas **incluye siempre al motorista** ([`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)). Sin la lista completa no se puede programar: no se evalúa lo que no se declara.

### 2. La evaluación es por tramo y por configuración

En una misión multi-destino, cada tramo tiene su propia ocupación. [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) ya lo establece para la carga que se entrega y libera capacidad; **aquí se extiende a la carga que se incorpora**:

| Tramo | Ocupación |
|---|---|
| Tegucigalpa → Danlí | 6 personas + 400 kg mobiliario + 14 cajas |
| Danlí → Trojes | 6 personas + 400 kg mobiliario + 14 cajas |
| Trojes → Danlí | 6 personas (mobiliario entregado) |
| Danlí → Tegucigalpa | 6 personas + generador dañado, **si se autoriza** |

Evaluar solo el peor tramo bloquearía misiones perfectamente ejecutables. Evaluar solo la salida deja pasar el retorno imposible. Se evalúan **todos**.

### 3. El bloqueo obliga a proponer — no dice "no" a secas

Cuando la configuración no cabe, el sistema presenta las salidas ya validadas contra las mismas reglas, con lo que cada una implica:

| Salida | Qué implica |
|---|---|
| **Otro tipo de vehículo compatible con todos los objetos** | Revalida `BD-02` — la categoría de licencia puede cambiar ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md)) —, `BD-03`, `BD-07` y `BD-09`. Recalcula la categoría y el estimado de peaje ([`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md)) y el rendimiento esperado ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)). Puede exigir cambiar de motorista |
| **División en dos Órdenes de Misión hermanas** | Es la salida operativa real que [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) ya nombra. Dos folios, dos asignaciones, dos vales, dos liquidaciones, vinculadas explícitamente entre sí. **Cuesta más combustible y más peajes**, y el sistema debe mostrar ese costo antes de que se elija, no después |
| **Convoy: dos vehículos bajo la misma Orden de Misión** | `[C]` — ver abajo. Exigiría dos motoristas habilitados, dos bitácoras y dos imputaciones de consumo |
| **Reducción del alcance** | Menos carga o menos personas, con **quién lo decidió registrado**. Hoy esta decisión la toma el motorista en el predio y no queda escrita en ninguna parte |
| **Diferir un objeto a otra fecha** | La carga viaja después. Genera una solicitud vinculada, no una nota mental |

### 4. Qué cede cuando hay que ceder

La solicitud declara **cuál objeto es el propósito principal de la misión**. La reducción se aplica primero al objeto que no es el propósito principal, y la decisión queda con autor y motivo.

**Y hay un límite que no se negocia:** la configuración **nunca** se resuelve trasladando personas fuera de plazas homologadas. Una paila, una tina, un espacio de carga o el piso de un panel **no son capacidad de pasajeros** ([`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)), y no existe parámetro configurable, autorización jerárquica ni situación de emergencia que lo levante. Es la única manera real de "hacer caber" gente, y es la que produce los lesionados. `[I]` — es regla de producto derivada de [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) y de la premisa rectora 2, no una cita de norma.

### 5. Falta una segunda matriz: objeto contra objeto

La matriz vigente de [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) cruza *tipo de vehículo × tipo de objeto*. Hace falta además una matriz **objeto × objeto**, con la misma mecánica: compatible, compatible con condiciones, incompatible; catálogo configurable con vigencia; y **ausencia de entrada significa bloqueo, no permiso** — interpretar el vacío como autorización es cómo se cuelan las combinaciones peligrosas.

Pares que la institución tendrá que definir `[C]`:

| Par | Por qué importa |
|---|---|
| Personas + combustible en bidones | El ejemplo que la propia [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) usa para justificar su existencia |
| Personas + material químico o carga peligrosa | `[C]` insumo #38 — la institución debe declarar si moviliza este tipo de carga |
| Personal de la institución + persona bajo custodia | `[C]` insumo #39. Es traslado de M-17 con requisitos propios |
| Personas externas + personal de la institución | Compatible, **con condiciones**: manifiesto que los distingue ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)), minimización ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)) y registro de consultas ([`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)) |
| Carga suelta sin sujeción + personas en cabina abierta a la paila | Condición de sujeción obligatoria, impresa en la orden y acusada por el despachador |

Las **condiciones** de un par *compatible con condiciones* se imprimen en la orden de misión y el despachador acusa haberlas leído ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md)).

### 6. La carga que aparece a las cinco de la mañana

Es la variante que más presión operativa genera, y [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) ya la trata como **novedad de despacho** que dispara la reevaluación. Aquí se precisa el desenlace:

- Se reevalúan compatibilidad ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md)) y capacidad ([`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md)) sobre la configuración nueva, tramo por tramo.
- Si pasa: se registra la novedad con **quién la ordenó, quién la aceptó y a qué hora**. La carga que sube sin que nadie firme es la que aparece en el acta del accidente sin dueño.
- Si no pasa: **no sale con esa carga**. La orden se reformula o la carga se difiere, y esa decisión también se registra con autor. El sistema no ofrece un botón para "continuar de todos modos": [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) es bloqueo duro.
- El fondo ya emitido y el estimado de peajes **no cambian** por agregar carga, salvo que la reformulación cambie el vehículo — y entonces se recalcula todo ([CE-16](CE-16-vehiculo-a-taller-con-misiones-programadas.md), sección 5).

Después de la salida ya no hay bloqueo posible: un vehículo en ruta que recoge carga o un pasajero adicional se registra como **novedad**, no como edición del manifiesto ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)), y produce hallazgo en la liquidación.

### 7. El peso que nadie quiere declarar

[`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) admite estimación por rango del catálogo de tipos de carga, marcada como estimada. Bien. Pero hay un incentivo perverso que hay que nombrar: **si declarar más peso bloquea, se declara menos peso.**

Por eso el peso y la ocupación **efectivos** se capturan al despachar y se comparan contra los declarados. Y la desviación no se trata como falta de la misión: se acumula como **indicador por dependencia solicitante**. Castigar la misión hace que se mienta mejor; medir la dependencia hace que se declare mejor.

`[C]` Qué tipos de carga exigen peso cierto y cuáles admiten rango — [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) lo deja abierto, a levantar con el Jefe de Transporte — insumo #63.

### Lo que hay que confirmar

- `[C]` **¿Admite el modelo más de un vehículo simultáneo bajo una misma Orden de Misión?** Es decisión de arquitectura, no de análisis, y este caso la fuerza. Opciones: **(a)** un vehículo por orden y el convoy se modela como misiones hermanas vinculadas — costo: dos expedientes, dos liquidaciones, y el rendimiento del convoy no se ve junto; **(b)** varios vehículos por orden, con bitácora e imputación de consumo por vehículo — costo: cambia el modelo de datos, el despacho, la liquidación y toda la conciliación de [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md). **Escalado al PO — insumo #62.** Nótese que [CE-02](CE-02-averia-mecanica-en-ruta.md) ya introduce varios vehículos por orden, pero **en secuencia**, no simultáneos: no es el mismo problema.
- `[C]` **¿Moviliza la institución carga peligrosa o especializada, y bajo qué régimen?** — insumo #38. Mientras no se confirme, el catálogo la marca *requiere autorización especial* y bloquea ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md)). **No se infiere ninguna regla de manejo de carga peligrosa.**
- `[C]` **¿Traslada personas bajo custodia o menores?** — insumo #39. Cambia la matriz objeto × objeto y los requisitos de M-17.
- `[C]` **Margen de tolerancia de carga.** [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) lo fija en cero inicial. Confirmar si la institución quiere activarlo, con qué valor y con qué fundamento registrado. **Un margen invisible es la forma de que la regla deje de significar algo.**
- `[C]` **¿Existe un formato de acta de entrega de carga** en destino, con inventario y firma del consignatario? [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) exige registrar remitente y consignatario `[V]`; el formato en papel de la institución, si existe, es el diseño de esa pantalla — insumo #2.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C18a` | *Existe una matriz de compatibilidad entre objetos de traslado (objeto × objeto), configurable y con vigencia, evaluada par a par sobre todos los objetos declarados. La ausencia de entrada bloquea.* | [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) solo cruza vehículo × objeto. **Personas junto a combustible en bidones no se puede expresar hoy** |
| `RN-C18b` | *La capacidad y la compatibilidad se evalúan por tramo sobre la configuración de cada tramo, incluida la carga que se incorpora en un destino intermedio.* | [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) contempla la carga que **se entrega** y libera capacidad; no la que **se recoge** |
| `RN-C18c` | *Todo bloqueo por capacidad o compatibilidad presenta las salidas validadas con su costo, y registra cuál se eligió, quién la eligió y cuándo.* | Sin esto el bloqueo empuja la operación fuera del sistema, que es la peor forma de cumplir la regla |
| `RN-C18d` | *La solicitud declara el objeto principal de la misión; la reducción se aplica primero al objeto no principal. En ningún caso la configuración se resuelve trasladando personas fuera de plazas homologadas.* | Ninguna regla dice **qué cede** cuando dos objetos legítimos compiten por el mismo vehículo |
| `RN-C18e` | *El peso y la ocupación efectivos se capturan al despachar y su desviación contra lo declarado se acumula como indicador por dependencia solicitante.* | [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) pide comparar declarado contra efectivo, pero no dice qué se hace con la diferencia |

## Evidencia que debe quedar

Encadenado a la misma Orden de Misión, la institución debe poder mostrar al auditor del TSC:

1. La **solicitud con la lista completa de objetos de traslado** y sus fichas: número de personas incluido el motorista, tipo de carga del catálogo, peso y volumen declarados, y cuál es el objeto principal.
2. El **resultado de la evaluación** de compatibilidad y capacidad **por tramo**, con la versión de la matriz aplicada y su vigencia a la fecha del hecho ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)). Es la prueba de que alguien evaluó, que hoy no existe en ningún papel.
3. Si hubo bloqueo: el **registro del bloqueo con su dato concreto**, las salidas ofrecidas, cuál se eligió, quién la eligió y cuándo.
4. Si se dividió la misión: los **folios de las misiones hermanas** y su vínculo explícito.
5. Las **condiciones impresas** de todo par *compatible con condiciones* y el acuse del despachador de haberlas leído.
6. El **manifiesto cerrado al despacho** con folio y QR, distinguiendo personal de la institución de personas externas ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)), y el **registro de quién consultó ese manifiesto** ([`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)).
7. El **peso y la ocupación efectivos** capturados al despachar, frente a los declarados.
8. Toda **carga o persona incorporada después del despacho**, registrada como novedad con hora, lugar, quién la ordenó y quién la aceptó — nunca como edición del manifiesto.
9. El **acta de entrega de la carga** en destino, con inventario y constancia del consignatario ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) para el vehículo; para la carga, la regla candidata de acta de transbordo de [CE-02](CE-02-averia-mecanica-en-ruta.md)).

## Trazabilidad

- **Autoridad de bloqueos:** `BD-07` y `BD-09` de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md)
- **Reglas:** [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md), [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md), [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md), [`RN-52`](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)
- **Reglas candidatas:** `RN-C18a` a `RN-C18e`
- **Puntos de control:** `PC-07` (compatibilidad vehículo ↔ objeto del traslado) y `PC-12` (manifiesto y cadena de custodia) de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[V]` registro del lado de la carga: tipo, peso, origen, destino, remitente y consignatario · [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` capacidad en la ficha maestra · [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) para personas externas
- **Premisas rectoras:** 1 (movilizaciones de recursos, no viajes de personas) y 2 (el tipo de vehículo es el eje de compatibilidad)
- **Actores:** ACT-02, ACT-03, ACT-04, ACT-05, ACT-06, ACT-10
- **Casos relacionados:** [CE-02](CE-02-averia-mecanica-en-ruta.md) (transbordo de carga en ruta), [CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) (competencia por el vehículo entre misiones), [CE-16](CE-16-vehiculo-a-taller-con-misiones-programadas.md) (recálculo al sustituir vehículo)
- **Insumos:** #2 (formatos en papel — la casilla de peso que no existe), #38 (carga peligrosa), #39 (personas bajo custodia y menores), #62 (¿convoy bajo una misma Orden de Misión?), #63 (tipos de carga que exigen peso cierto)
