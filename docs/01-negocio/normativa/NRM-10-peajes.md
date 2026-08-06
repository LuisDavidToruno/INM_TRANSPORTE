# NRM-10 — Peajes: puntos, tarifas y clasificación vehicular

| Campo | Valor |
|---|---|
| **Ámbito** | Puntos de peaje del país, tarifas, clasificación del vehículo, exoneraciones, medios de pago |
| **Módulos afectados** | M-18 Peajes, M-03 Flota, M-06 Solicitudes, M-08 Bitácora, M-13 Liquidación |
| **Última verificación** | 2026-08-06 |
| **Riesgo de cambio** | **Alto** — la tarifa se revisa cada enero y en 2026 hubo tres reversiones |

## ⚠️ Corrección al supuesto de partida

El encargo original fue "cómo clasifica a los vehículos por la cantidad de sus ejes". **La investigación demuestra que la clasificación NO es puramente por ejes: es combinada.** `[V]`

Un vehículo liviano tiene 2 ejes y paga **L. 22**. Un "Vehículo de 2 Ejes" paga **L. 90**. Ambos tienen dos ejes. El discriminante entre las dos categorías es el **tipo y peso del vehículo**, no el conteo de ejes.

**Consecuencia dura de diseño:** cualquier modelo que use `numero_ejes` como única llave para resolver la tarifa está mal y va a cobrar cuatro veces de más a cada pickup de la flota.

## 1. Matriz de categorías `[V]`

Publicada por la **SAPP** (Superintendencia de Alianza Público-Privada), ente regulador del contrato de concesión. Son **once categorías**, no seis como reporta la prensa.

| Categoría (nombre literal de la fuente) | Tarifa publicada |
|---|---|
| Liviano/Turismo | L. 22.00 |
| Vehículo de 2 Ejes | L. 90.00 |
| Vehículo de 3 Ejes | L. 134.00 |
| Vehículo de 4 Ejes | L. 179.00 |
| Vehículo de 5 Ejes | L. 224.00 |
| Vehículo de 6 Ejes | L. 269.00 |
| Vehículo de 7 Ejes | L. 314.00 |
| Vehículo de 8 Ejes | L. 359.00 |
| Vehículo de 9 Ejes | L. 403.00 |
| Montacargas Liviano | L. 11.00 |
| Montacarga Pesado | L. 45.00 |

Tres cosas que esta tabla obliga:

- **La escala de ejes llega a 9**, no a 6. La prensa reporta hasta 6 porque es lo que circula habitualmente.
- **Hay categorías que no son de ejes** (montacargas). El catálogo debe ser tabla abierta, no un enumerado de 2 a 9.
- `[I]` La progresión de 2 a 9 ejes es casi lineal (~L. 45 por eje adicional). **No implementar como fórmula**: es una tabla publicada, y una fórmula inferida se vuelve falsa al primer ajuste asimétrico.

## 2. El criterio legal de clasificación — hallazgo central `[V]`

**Comunicado de la SAPP del 17 de septiembre de 2025**, tras denuncias ciudadanas recibidas desde el 27 de agosto:

- COVI-H estaba reclasificando **Hyundai H-100, Kia K2700 y Mercedes-Benz Sprinter** a categoría superior, cobrándoles **L. 90** en lugar de **L. 22**.
- La SAPP resolvió que deben clasificarse como **vehículos livianos conforme al Artículo 51 de la Ley de Tránsito**, y ordenó a COVI-H suspender el cobro el mismo 17 de septiembre a las 10:00 a.m.

**Por qué esto importa más que la tarifa:** la categoría de peaje se ancla en la **misma norma** que ya sustenta la matriz licencia↔vehículo de [NRM-06](NRM-06-transito-y-licencias.md). Los atributos que esa ficha ya exige — tipo, **peso bruto vehicular en kg**, capacidad de pasajeros, articulado — son **los mismos** que determinan la categoría de peaje. No hay modelo nuevo que inventar: la categoría se **deriva de la ficha técnica del vehículo**.

**Y esto también:** la flota típica de una institución pública hondureña — pickups, panels tipo H-100 o K2700, microbuses Sprinter — cae **exactamente** en la zona gris que la SAPP tuvo que resolver. Es previsible que a un vehículo institucional le cobren mal en la caseta.

`[C]` **No se pudo transcribir el Artículo 51.** El PDF oficial de la Ley de Tránsito en `tsc.gob.hn` es un escaneo sin capa de texto. La pertinencia del artículo está `[V]` por el comunicado de la SAPP que lo invoca; su contenido literal queda `[C]`. **Requiere OCR — el mismo trabajo resuelve el Art. 48 pendiente en NRM-06.**

## 3. Puntos de peaje

### Corredor Logístico — CA-5 Norte, operado por COVI-H `[V]`

| Estación | Ubicación | Departamento | Operación desde |
|---|---|---|---|
| **Zambrano** | km 37, sector Zambrano | Francisco Morazán | 26/06/2014 `[P]` |
| **Siguatepeque** | km 125 de la CA-5 | Comayagua | 15/11/2015 `[P]` |
| **Yojoa** | km 182, recta de La Barca, Santa Cruz de Yojoa | Cortés | 13/05/2015 `[P]` |

Ubicaciones y departamentos `[V]` por fuentes concordantes. Kilometrajes y fechas `[P]`, de fuente no oficial.

- **Concesionaria:** Concesionaria Vial Honduras S.A. de C.V. (COVI-H) — Hidalgo e Hidalgo (Ecuador) + Construcción y Administración (Perú). `[V]`
- **Alcance:** 391.82 km — Goascorán a Villa de San Antonio, y Tegucigalpa–San Pedro Sula–Puerto Cortés. `[V]`
- **Un viaje Tegucigalpa → San Pedro Sula atraviesa las tres estaciones.** Ida y vuelta = **6 cruces**. `[V]`
- Tráfico 2023 `[V]`: Zambrano 35%, Yojoa 34%, Siguatepeque 31%. Composición: 73% livianos, 11% buses de 2 ejes, 8% camiones de 5 ejes.

### Canal Seco (Goascorán – Villa de San Antonio) — sin cobro `[V]`

Tramo de ~100.49 km de la misma concesión. COVI-H no lo recibió por problemas en las obras civiles y **no opera cobro ahí**. Reclama al Estado ~L. 1,000 millones. `[P]`

### Corredor Turístico — ADASA — probablemente sin cobro `[P]`

- **Concesionaria:** Autopistas del Atlántico (ADASA), grupo Grodco (Colombia). Tramos La Barca–El Progreso, SPS–El Progreso, El Progreso–Tela, Tela–La Ceiba. `[V]`
- **Estado según la propia SAPP: "EN PROCESO DE TERMINACIÓN ANTICIPADA".** `[V]`
- Caseta identificada en **San Manuel, Cortés** `[P]`. Concesión cancelada en 2018 tras 421 días de protestas y quema de casetas `[P]`. Reclamo de USD 179.4 millones contra el Estado `[P]`.
- `[C]` **No se pudo confirmar si hoy se cobra en algún punto del Corredor Turístico.** Una fuente comercial afirma que Honduras tiene una sola carretera de peaje (la CA-5 Norte), lo que es concordante con el estado de terminación anticipada.

### Otros y prospectiva

- `[C]` Se mencionan casetas antiguas en San Pedro Sula (salida a Puerto Cortés, Puente Chamelecón, El Polvorín/La Lima). Sin verificar si operan.
- `[C]` No se encontró evidencia de peajes en CA-1 Sur, CA-4 Occidente ni carreteras departamentales.
- `[P]` Hay proyectos en cartera (ampliación CA-4, Corredor Turístico SPS–Trujillo, Corredor Agrícola, autopistas elevadas en Tegucigalpa y SPS). **El catálogo de puntos debe ser ampliable en producción, sin cambio de código.**

## 4. Tarifas — y por qué no se puede cargar ninguna todavía

### Serie histórica verificada

| Momento | Liviano | 2 ejes | 3 | 4 | 5 | 6 | Nivel |
|---|---|---|---|---|---|---|---|
| Aprobación concesión (2012–13) | L. 16 | L. 64 | — | — | L. 160 | — | `[P]` |
| ~2018–19 | L. 19 | L. 76 | — | — | — | L. 229 | `[P]` |
| **Congelada desde 2020/21 — la que publica la SAPP** | **22** | **90** | **134** | **179** | **224** | **269** | `[V]` |
| Anunciada 15/01/2025 — **no aplicada** | 29 | — | — | — | — | 350 | `[V]` como anuncio |
| Anunciada 15/01/2026, luego marzo — **suspendida** | 31 | 122 | 184 | 245 | 306 | 367 | `[V]` como anuncio |

### `[C]` La tarifa vigente hoy NO está verificada

Cronología de 2026:

1. **08/01** — COVI anuncia aumento efectivo el 15 de enero, retroactivo, incluyendo subsidios pendientes de 2024 y 2025 `[V]`
2. **~15/01** — la SIT suspende el ajuste hasta el 26 de enero por instrucción presidencial `[P]`
3. Periodo de gracia extendido al **15/02** `[P]`
4. **27–28/02** — COVI anuncia el aumento a partir de marzo, "el incremento no es discrecional, sino contractual" `[V]`
5. **28/02** — la **SIT confirma que no habrá incremento para ninguna categoría** `[V]`, corroborado por tres medios

**Contradicción abierta:** un agregador comercial publica que desde marzo de 2026 rigen L. 31/122/184/245/306/367, lo que choca de frente con el comunicado de la SIT.

**Lectura:** el comunicado de la SIT es más fiable — es la autoridad concedente, es fuente primaria y está corroborado. Pero **no se da por resuelto**: no hay ninguna fuente de abril a agosto de 2026 que confirme qué se cobra hoy, y el acuerdo del 28 de febrero no declara fecha de vencimiento.

> **No cargar ninguna tarifa al sistema hasta confirmarla con COVI-H o la SAPP.** Es el dato más volátil de toda la ficha.

`[V]` **La tarifa que ve el usuario es política, no contractual.** El Estado debe USD 14 millones (>L. 364 millones) a COVI por subsidio de 2024 y 2025 para mantenerla congelada. Cuando el subsidio se corte, la tarifa salta de golpe.

## 5. Exoneraciones — inconcluso, con conclusión de trabajo

**Lo verificado** `[V]`: existe régimen de exoneración. En 2023, **224,994 vehículos** pasaron con libre paso por las tres casetas — el **2% del tráfico total** (~14.2 millones). Las categorías que COVI menciona como ejemplo son "ambulancias, policía, etc." — **la lista completa no está publicada en ninguna fuente consultable**.

`[P]` Una fuente no oficial enumera: motocicletas, bomberos, unidades públicas o privadas de transporte o rescate de personas en emergencia, policía, ejército e instituciones humanitarias.

> ### `[I]` Conclusión de trabajo
>
> **La exoneración se perfila como funcional (emergencia y rescate), no institucional (por ser del Estado).** Ninguna fuente exonera a vehículos administrativos de una institución pública. El 2% de tráfico exonerado es coherente con un universo restringido de ambulancias, patrullas, bomberos y unidades militares — **no** con toda la flota estatal.
>
> **Un pickup institucional en misión administrativa PAGA peaje.** M-18 se diseña asumiendo que se paga.
>
> Esto **no está verificado** y no debe presentarse como tal. Hay evidencia de que existen exonerados, evidencia de que son de emergencia y rescate, y **ausencia total de evidencia** de exoneración general para el Estado. La ausencia de evidencia no es evidencia de ausencia.

`[C]` No se localizó la cláusula del contrato de concesión que regula exoneraciones. El sitio de COVI-H devuelve HTTP 403 a consulta automatizada.

**Diseño defensivo:** aunque la exoneración institucional no aplique hoy, el sistema modela **"vehículo exonerado en el punto X con fundamento Y y vigencia Z"** como dato, no como constante. SIGTI es genérico: una institución con ambulancias o unidades de rescate lo necesita desde el día uno.

## 6. Medios de pago

- **Efectivo en lempiras** en las tres casetas `[V]`
- **CoviPass** — sistema **prepago con TAG RFID**, paso sin detenerse, descuento de cuenta prepagada. **Sí existe telepeaje en Honduras.** No es obligatorio. `[V]`
- Tarjeta electrónica en caseta `[P]`

`[C]` **Facturación no verificada.** No se pudo determinar si COVI-H emite factura fiscal en caseta, si CoviPass emite estado de cuenta o factura consolidada, ni si existe modalidad empresarial a nombre de la institución. `covih.com` bloquea la consulta.

`[I]` **Riesgo:** si el ticket de caseta no es documento fiscal a nombre de la institución, el descargo de peajes será el punto débil de la liquidación ante auditoría. Un **tag corporativo con estado de cuenta mensual** a nombre de la institución sería el mecanismo más defendible — y produce el dato de conciliación automáticamente.

## 7. Regulación y mecanismo de ajuste

| Elemento | Dato | Nivel |
|---|---|---|
| Ley marco | **Ley de Promoción de la Alianza Público-Privada, Decreto 143-2010**, vigente desde 16/09/2010 | `[V]` |
| Creación de COALIANZA | Artículo 11 de esa ley | `[P]` |
| Liquidación de COALIANZA | PCM-064-2019, 17/12/2019 | `[P]` un solo origen |
| **Regulador vigente** | **SAPP** — Superintendencia de Alianza Público-Privada | `[V]` |
| **Autoridad concedente** | **SIT** — Secretaría de Infraestructura y Transporte. Negocia y firma los congelamientos tarifarios | `[V]` |
| Aprobación del Congreso | 19/12/2012; La Gaceta 08/03/2013; operación desde 12/10/2015 | `[V]` |
| **No hay tarifa fijada por ley** — nace del contrato de concesión | | `[V]` |

**Ajuste anual** `[V]`: revisión anual con base en el IPC en dólares (cláusula 9.4 del contrato `[P]`, conocida solo por prensa). COVI declara que la fórmula integra IPC de Honduras, IPC de Estados Unidos y variación del tipo de cambio `[V]` como declaración; `[C]` la fórmula exacta. En la práctica se resuelve por **negociación política entre la SIT y COVI**, con subsidio del Estado si se congela.

**Consecuencia para SIGTI:** la tarifa cambia **al menos una vez al año, en enero, con alta probabilidad de aplicación retroactiva o de reversión a mitad de proceso**. Es exactamente el caso de la premisa rectora #6: parámetro con vigencia por rango de fechas, cálculo a la fecha del hecho, y **soporte para corrección retroactiva** — porque en 2026 estuvo a punto de ocurrir.

## 8. Realidad operativa `[C]`

**No se encontró ninguna fuente** sobre cómo las instituciones públicas hondureñas manejan el pago y la liquidación de peajes. Todo esto son preguntas, no hallazgos:

- `[C]` ¿El motorista paga de su bolsillo y liquida después, o se le entrega efectivo por adelantado junto con el de combustible?
- `[C]` ¿El peaje se financia con el viático — y entonces es asunto de ARGOS — o es gasto de misión separado?
- `[C]` ¿La institución tiene tags CoviPass? ¿A nombre de quién?
- `[C]` ¿Qué objeto del gasto se usa para peajes?
- `[C]` ¿Qué se acepta hoy como descargo: el ticket de caseta, una declaración jurada, ninguna?

`[I]` Por analogía con el mecanismo de vale de combustible de [NRM-09](NRM-09-realidad-operativa.md), lo más probable es entrega de efectivo contra liquidación con tickets. Es inferencia.

> **Frontera con ARGOS sin resolver:** si el peaje resulta ser un componente del viático, M-18 se solapa con lo que maneja ARGOS. **Resolver esta frontera antes de escribir historias de M-18.** Ver [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

## 9. Implicaciones de requerimiento

### Catálogo y parametrización

- **El sistema debe** mantener un **catálogo de puntos de peaje** con nombre, operador, carretera o corredor, departamento, kilómetro, coordenadas, sentido de cobro y **estado operativo con vigencia** (activo, suspendido, cerrado). Sin el estado con vigencia no se puede recalcular un viaje pasado por una caseta que ya no existe.
- **El sistema debe** mantener el **catálogo de categorías como tabla abierta**, no como enumerado de 2 a 9 ejes. Debe admitir "Liviano/Turismo", "Vehículo de N Ejes" hasta 9, montacargas y categorías futuras **sin cambio de código**.
- **El sistema debe** almacenar la tarifa como **(punto × categoría × rango de vigencia)** y calcular **siempre a la fecha del hecho**. Prohibido cablear tarifas. Prohibido derivarlas de una fórmula por eje.
- **El sistema debe** soportar **corrección retroactiva de una tarifa ya aplicada**, recalculando las misiones afectadas y **dejando asiento de la diferencia** — nunca sobrescribiendo el valor histórico.
- **El sistema debe** registrar la **fuente y fecha de verificación de cada tarifa cargada**, y alertar cuando una lleve más de 12 meses sin revisar.

### Clasificación del vehículo

- **El sistema debe** asignar a cada vehículo su **categoría de peaje derivada de la ficha técnica** — tipo, peso bruto vehicular, número de ejes, capacidad, articulado — **y no del número de ejes por sí solo**.
- **El sistema debe** tratar la categoría de peaje como **atributo con vigencia y fundamento registrado**, porque la SAPP ya reclasificó vehículos por resolución y volverá a hacerlo.
- **El sistema debe** permitir registrar un **cobro en categoría distinta a la asignada**, marcarlo como **discrepancia de clasificación**, conservar el ticket y habilitar el reclamo ante la SAPP.

### Estimación previa a la aprobación

- **El sistema debe** estimar el **costo de peajes de la ruta antes de aprobar la solicitud**: qué puntos atraviesa, cuántas veces cada uno (ida, retorno, paso repetido), con la tarifa vigente a la fecha prevista.
- **El sistema debe** presentar el estimado **desglosado por punto**, no como total opaco. Quien autoriza tiene que poder verificar el cálculo.
- **El sistema debe** contemplar que un vehículo exonerado genere estimado cero en los puntos donde aplique, con el fundamento visible.

### Registro durante la ejecución

- **El sistema debe** permitir al motorista registrar **cada paso por caseta sin conectividad**: punto, fecha y hora, categoría cobrada, monto, medio de pago y **foto del ticket**.
- **El sistema debe** distinguir el **medio de pago**, porque determina qué evidencia existe y de dónde sale la conciliación.
- **El sistema debe** soportar el **TAG prepago como instrumento institucional** con su ciclo de vida: asignación a vehículo, saldo, recargas y estado de cuenta conciliable contra misiones.

### Conciliación — lo que va a pedir el auditor

- **El sistema debe** conciliar **estimado contra pagado por misión**, con causa tipificada de cada desviación: cambio de tarifa entre aprobación y ejecución, ruta distinta a la autorizada, paso adicional no previsto, cobro en categoría equivocada, o peaje pagado sin paso registrado.
- **El sistema debe** correlacionar **peaje × kilometraje × ruta autorizada**. Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y el sistema tiene que producirlo solo. *Esto es exactamente lo que busca el auditor del TSC: correlación, no comprobantes archivados.*
- **El sistema debe** validar la **coherencia geográfica y temporal** de la secuencia de casetas. El orden Zambrano → Siguatepeque → Yojoa es el sentido Tegucigalpa → San Pedro Sula; una secuencia imposible o un intervalo inviable entre dos casetas es una alerta.
- **El sistema debe** generar **reporte de peajes por vehículo, motorista, dependencia y período**, con estimado, pagado, desviación y evidencia — listo para entregar al TSC.
- **El sistema debe** conservar el ticket como adjunto vinculado al paso, y **advertir cuando falte sin bloquear el cierre**. El motorista no siempre podrá conseguirlo, y bloquear la liquidación por eso hace que el sistema se abandone.

## 10. Pendientes, en orden de prioridad

1. `[C]` **Tarifa efectivamente vigente hoy.** Contradicción abierta entre el comunicado de la SIT (28/02/2026, sin aumento) y un agregador comercial. **No cargar tarifas hasta confirmar con COVI-H o la SAPP.**
2. `[C]` **Lista oficial de exoneraciones** y si alcanza a vehículos administrativos del Estado. **Es lo que decide cómo se construye M-18.**
3. `[C]` **Texto del Artículo 51 de la Ley de Tránsito** — criterio legal de liviano vs. pesado. Requiere OCR; **el mismo trabajo resuelve el Art. 48** pendiente en NRM-06.
4. `[C]` **PDF de tarifas de la SAPP** — escaneo sin capa de texto; OCR para contrastar contra el HTML.
5. `[C]` **Facturación**: ¿factura fiscal en caseta? ¿CoviPass empresarial con estado de cuenta a nombre de la institución? Requiere consulta manual o telefónica a COVI-H.
6. `[C]` Si hoy se cobra peaje en el Corredor Turístico.
7. `[C]` Cláusula 9.4 y fórmula exacta de indexación — solo conocida por prensa.
8. `[C]` Verificar PCM-064-2019.
9. `[C]` Toda la realidad operativa de la sección 8, incluida la frontera peaje ↔ viático ↔ ARGOS.

## Fuentes

Todas consultadas el 2026-08-06.

**Oficiales**
- [SAPP — Corredor Logístico](https://www.sapp.gob.hn/es/corredor-log%C3%ADstico) — origen de la matriz de categorías
- [SAPP — Comunicado de reclasificación vehicular, 17/09/2025](https://www.sapp.gob.hn/post/sapp-atiende-denuncias-ciudadanas-y-comunica-suspensi%C3%B3n-de-incremento-en-cobro-de-peaje-a-veh%C3%ADculos)
- [SAPP — PDF de tarifas](https://www.sapp.gob.hn/_files/ugd/c9b6ca_31b0ccb34e0040ecb428bb395330097b.pdf) — **escaneo sin capa de texto, no transcribible**
- [TSC — Ley de Tránsito, Decreto 205-2005](https://www.tsc.gob.hn/web/leyes/Ley-de-Transito.pdf) — **escaneo sin capa de texto**
- [SAR — Decreto 143-2010, Ley de Promoción de la APP](https://www.sar.gob.hn/download/decreto-no-143-2010-no-32317-de-16-de-septiembre-de-2010-ley-de-promocion-de-la-alianza-publico-privada/)
- [Banco Mundial PPP — Contrato de concesión Corredor Logístico](https://ppp.worldbank.org/public-private-partnership/es/node/8097)
- `covih.com` — **inaccesible, HTTP 403**

**Prensa**
- [El Heraldo — Gobierno confirma que no habrá aumento, 28/02/2026](https://www.elheraldo.hn/honduras/gobierno-confirma-no-habra-aumento-peaje-covi-ninguna-categoria-honduras-ca-5-casetas-GO29507616)
- [El Heraldo — Nuevas tarifas anunciadas para 15/01/2026](https://www.elheraldo.hn/honduras/cuales-son-nuevas-tarifas-peaje-partir-del-15-enero-2026-OE28868677)
- [El Heraldo — Deuda de USD 14 millones por subsidio](https://www.elheraldo.hn/portada/peaje-gobierno-covi-honduras-deuda-aumento-AF28872460)
- [Infobae — COVI anuncia aumento desde marzo, 28/02/2026](https://www.infobae.com/honduras/2026/02/28/concesionaria-vial-de-honduras-anuncia-aumento-en-la-tarifa-del-peaje-desde-marzo/)
- [Dinero HN — Recaudación 2023 y vehículos exonerados](https://dinero.hn/cobro-de-peaje-en-ca5-dejo-796-millones-de-lempiras-en-2023-covi-honduras/)
- [La Prensa — L1,000 millones reclamados por Canal Seco](https://www.laprensa.hn/portada/canal-seco-covi-honduras-cobro-peaje-indemnizacion-AP29263232)
- [Criterio.hn — Arbitraje del Corredor Turístico](https://criterio.hn/corrupcion-represion-y-arbitraje-ante-el-ciadi-la-historia-detras-del-fracasado-peaje-en-el-corredor-turistico-de-honduras/)

**Secundarias / comerciales** — usadas solo como `[P]` o `[I]`, nunca como base de un dato: XplorHonduras, TollGuru.
