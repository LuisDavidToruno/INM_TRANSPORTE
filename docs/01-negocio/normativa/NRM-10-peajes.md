# NRM-10 — Peajes: puntos, tarifas y clasificación vehicular

| Campo | Valor |
|---|---|
| **Ámbito** | Puntos de peaje del país, tarifas, clasificación del vehículo, exoneraciones, medios de pago |
| **Módulos afectados** | M-18 Peajes, M-03 Flota, M-06 Solicitudes, M-08 Bitácora, M-13 Liquidación |
| **Última verificación** | **2026-08-24** (previa: 2026-08-06) |
| **Riesgo de cambio** | **Alto** — la tarifa se revisa cada enero y en 2026 hubo tres reversiones |

> ## Qué cambió el 2026-08-24
>
> | Punto | Antes | Ahora |
> |---|---|---|
> | Tarifa vigente | `[C]` contradicción abierta | `[P]` **congelada en la tabla de L. 22 / 90 / 134…**; la contradicción con el agregador comercial queda **resuelta en contra del agregador** |
> | ¿Paga un pickup institucional? | `[I]` conclusión de trabajo | `[V]` **paga, y paga como liviano: L. 22** |
> | Fundamento legal de la clasificación | `[V]` "Artículo 51 de la Ley de Tránsito" | 🔴 **No corroborado.** Se degrada a `[C]` y se identifica el fundamento probable: el **Acuerdo 1012-2021** |
> | Lista de exoneraciones | `[C]` | `[C]` — **sin avance**, no hay fuente oficial |

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

**Comunicado de la SAPP del 17 de septiembre de 2025**, tras denuncias ciudadanas recibidas desde el 27 de agosto: `[V]`

- COVI-H estaba reclasificando **Hyundai H-100, Kia K2700, Mercedes-Benz Sprinter** y similares a categoría superior, cobrándoles **L. 90** en lugar de **L. 22**.
- La SAPP ordenó a COVI-H suspender ese cobro el **17 de septiembre a las 10:00 a.m.** y aplicarles **L. 22** como **vehículos livianos**.

### 🔴 Corrección del 2026-08-24 — el "Artículo 51" no está corroborado

La versión anterior de esta ficha afirmaba `[V]` que la SAPP fundó la reclasificación en el **Artículo 51 de la Ley de Tránsito**. **Se revisaron tres fuentes el 2026-08-24 y ninguna sostiene esa atribución:**

| Fuente | Qué dice sobre el fundamento |
|---|---|
| Comunicado de la SAPP, 17/09/2025 | No transcribe ni cita el Art. 51 |
| La Prensa, cobertura del comunicado | Cita literalmente: *"en cumplimiento de la **Ley de Tránsito y el Reglamento Especial en Materia de Permisos de Conducir**"* — sin número de artículo |
| El Heraldo, cobertura del comunicado | Menciona *"análisis técnicos jurídicos"*; **no cita ninguna norma** |

Y hay evidencia en contra: un índice jurídico ubica el **Art. 51 dentro del *Capítulo I, De las licencias de conducir* (arts. 45 a 52)**, y lo asocia al permiso del motociclista y a la complementariedad con tratados internacionales. `[P]`

> **Se degrada de `[V]` a `[C]`.** No se afirma que el Art. 51 sea irrelevante — se afirma que **nadie verificó que lo fuera**, y que esta ficha lo dio por bueno sin respaldo. Es exactamente la escalada silenciosa que `CLAUDE.md` prohíbe.

### El fundamento que sí está corroborado `[V]`

La cita de La Prensa apunta al **Reglamento Especial en Materia de Permisos de Conducir — Acuerdo No. 1012-2021**, que es también la fuente de la matriz licencia↔vehículo de [NRM-06](NRM-06-transito-y-licencias.md).

Encaja aritméticamente: ese reglamento define la **categoría B como vehículo liviano de masa máxima autorizada ≤ 3,500 kg** `[P]`, y los tres modelos que la SAPP mandó reclasificar están por debajo de ese umbral.

**La conclusión de diseño de la versión anterior se mantiene, y ahora con mejor fundamento:** la categoría de peaje y la categoría de licencia **se resuelven contra los mismos atributos de la ficha técnica** — tipo, masa máxima autorizada en kg, capacidad de pasajeros, articulado. No hay modelo nuevo que inventar.

### 🟢 Dato nuevo y directamente operativo `[V]`

En la cobertura del mismo comunicado, la SAPP precisa que **los pick-ups están comprendidos en la categoría de vehículo liviano** — L. 22.

**Es la respuesta al caso más frecuente de toda la flota institucional hondureña.** La estimación de peajes de M-18 ya no depende de un supuesto: el pickup en misión administrativa se estima a tarifa de liviano.

**Y sigue siendo cierto** que la flota típica — pickups, panels H-100 o K2700, microbuses Sprinter — cae exactamente en la zona gris que la SAPP tuvo que resolver por resolución. El cobro erróneo en caseta es previsible, y la función de *discrepancia de clasificación* de la §9 es un requisito, no un lujo.

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

### `[P]` La tarifa vigente hoy — contradicción resuelta, verificación incompleta

**Actualizado 2026-08-24.** Cronología completa de 2026:

| Fecha | Hecho | Nivel |
|---|---|---|
| **14/01** | **Vence el acuerdo de congelamiento suscrito en 2024** con el gobierno de Xiomara Castro | `[V]` |
| **08/01** | COVI anuncia aumento efectivo el 15 de enero — **~41 % en todas las categorías** | `[V]` |
| **~15/01** | La SIT suspende el ajuste hasta el 26 de enero por instrucción presidencial | `[P]` |
| **enero** | **La SAPP declara que "no ha autorizado ningún tipo de ajuste"** y califica el alza de **unilateral** | `[P]` — título del comunicado indexado, página no accesible |
| — | Periodo de gracia extendido al **15/02** | `[P]` |
| **27/01** | Toma posesión el gobierno de **Nasry Asfura**; cambia la contraparte negociadora | `[V]` |
| **27–28/02** | COVI anuncia el aumento a partir de marzo: *"el incremento no es discrecional, sino contractual"* | `[V]` |
| **28/02** | **La SIT confirma que no habrá incremento para ninguna categoría** | `[V]`, cuatro medios |
| **02/03** | **El Secretario de Infraestructura y Transporte, Aníbal Ehrler, confirma que la tarifa se mantiene mientras dure la negociación**, sostenida por un **subsidio parcial del Estado**. Se negocia un **ajuste gradual a cuatro años** y el pago de un subsidio pendiente de 2025 superior a **USD 10 millones** | `[V]`, dos medios concordantes |
| **03–08** | **Ninguna fuente.** Seis meses sin noticia sobre peajes | — |
| **24/08** | **La SAPP publica en su sitio la tabla de once categorías con L. 22 como tarifa de liviano, presentada como tarifas actuales** — sin fecha de vigencia ni de actualización | `[V]` como publicación; `[C]` como fecha |

### La contradicción se resuelve en contra del agregador comercial

El agregador comercial publicaba que desde marzo de 2026 rigen L. 31/122/184/245/306/367. **Cinco fuentes lo contradicen:** el comunicado de la SIT del 28/02, la declaración de Ehrler del 02/03, dos coberturas independientes de esa declaración, y la propia tabla que la SAPP publica hoy.

**Lectura:** el agregador reprodujo el aumento **anunciado** en enero y nunca lo corrigió cuando el aumento se revirtió. Es el modo de fallo típico de una fuente comercial: registra el anuncio, no el desenlace. **Se descarta como fuente de tarifa.**

### Por qué esto sigue siendo `[P]` y no `[V]`

Tres huecos que no se pueden rellenar sin consulta directa:

1. **Seis meses sin evidencia.** La última confirmación es del 02/03/2026. Hoy es 24/08/2026.
2. **El congelamiento es condicional, no un plazo.** Ehrler lo ató a *"mientras duren las conversaciones"*, y estimó *"un par de semanas o meses"*. Ese plazo ya venció con creces. **Puede haber terminado sin cobertura de prensa.**
3. **La tabla de la SAPP no lleva fecha.** Un sitio institucional que publica *"tarifas actuales"* sin fecha de actualización no permite distinguir vigencia de desactualización. Es exactamente el defecto que la §9 de esta ficha le prohíbe a SIGTI.

> **No cargar ninguna tarifa a producción hasta confirmarla con COVI-H o la SAPP.** Sigue siendo el dato más volátil de la ficha.
>
> **Pero ya se puede desbloquear el diseño de M-18.** La tabla de once categorías con L. 22 de base es la mejor hipótesis disponible, respaldada por el regulador. **Cárguese como juego de datos de referencia marcado `[P]`, con su fuente y su fecha de verificación visibles en pantalla** — que es justamente lo que la §9 exige del sistema. Diseñar contra una tarifa marcada como no confirmada es correcto; diseñar contra ninguna tarifa es parálisis.

### `[V]` El congelamiento es político y tiene precio

La estructura del conflicto quedó explícita en marzo. Palabras del Secretario Ehrler: *"la discusión no es si las tarifas se ajustan, porque contractualmente siempre se ajustan, sino **cómo y quién paga**"*.

- El contrato de concesión establece **revisión automática cada 15 de enero** conforme a fórmula pactada. `[P]`
- Desde 2020 el Estado **subsidia** el ajuste para que no llegue al usuario. `[V]`
- La deuda acumulada por ese subsidio ronda **USD 10–14 millones**. `[V]` — las fuentes difieren; ver contradicción abajo.
- El gobierno de Asfura declara que **no seguirá subsidiando indefinidamente** y negocia trasladar el alza en cuatro tramos anuales. `[V]`

> **Contradicción menor, no resuelta.** La deuda por subsidio se reporta como **USD 14 millones** (enero, El Heraldo, correspondiente a 2024 y 2025) y como **más de USD 10 millones** (marzo, Hondudiario, correspondiente solo a 2025). **Lectura:** son perímetros distintos, no cifras rivales — pero ninguna fuente lo dice, y no se resuelve aquí.

**Consecuencia dura para SIGTI:** cuando el acuerdo se cierre, es probable que la tarifa suba **en cuatro escalones anuales conocidos de antemano**. Un modelo de tarifa con vigencia por rango de fechas puede cargar los cuatro tramos el día que se publiquen. Un modelo con "tarifa actual" tendría que reeditarse cada enero. **Es el argumento más concreto a favor de la premisa rectora #6 que ha aparecido en toda la investigación.**

`[V]` **La tarifa que ve el usuario es política, no contractual.** El Estado debe USD 14 millones (>L. 364 millones) a COVI por subsidio de 2024 y 2025 para mantenerla congelada. Cuando el subsidio se corte, la tarifa salta de golpe.

## 5. Exoneraciones — inconcluso, con conclusión de trabajo

**Lo verificado** `[V]`: existe régimen de exoneración. En 2023, **224,994 vehículos** pasaron con libre paso por las tres casetas — el **2% del tráfico total** (~14.2 millones). Las categorías que COVI menciona como ejemplo son "ambulancias, policía, etc." — **la lista completa no está publicada en ninguna fuente consultable**.

`[P]` Una fuente no oficial enumera: motocicletas, bomberos, unidades públicas o privadas de transporte o rescate de personas en emergencia, policía, ejército e instituciones humanitarias.

> ### Conclusión de trabajo — reforzada el 2026-08-24
>
> **La exoneración es funcional (emergencia y rescate), no institucional (por ser del Estado).** `[I]` Ninguna fuente exonera a vehículos administrativos de una institución pública. El 2 % de tráfico exonerado es coherente con un universo restringido de ambulancias, patrullas, bomberos y unidades militares — **no** con toda la flota estatal.
>
> **Un pickup institucional en misión administrativa PAGA peaje, y paga como liviano: L. 22.** `[V]` — la parte de *"paga como liviano"* ahora sí está verificada: la SAPP declaró expresamente que los **pick-ups** están comprendidos en la categoría de vehículo liviano.
>
> La parte de *"paga"* sigue siendo `[I]`. Hay evidencia de que existen exonerados, evidencia de que son de emergencia y rescate, y **ausencia total de evidencia** de exoneración general para el Estado. La ausencia de evidencia no es evidencia de ausencia — pero el diseño ya no está a ciegas: si el pickup pagara, paga L. 22; si estuviera exonerado, paga cero. **M-18 se construye igual en ambos casos**, porque la exoneración se modela como dato del vehículo, no como constante.

### `[C]` Búsqueda del 2026-08-24 — sin avance, y por qué

Se intentaron cuatro vías. **Ninguna produjo la lista oficial.**

| Vía | Resultado |
|---|---|
| Contrato de concesión en el portal PPP del Banco Mundial | **HTTP 403** a consulta automatizada |
| Sitio de COVI-H (`covih.com`, `covih.com/tarifas`, `covih.com/covipass`) | **HTTP 403** en todas las rutas probadas |
| Sitio de la SAPP — sección de marco legal, comunicados y noticias | Rutas indexadas por buscadores devuelven **HTTP 404** al ser consultadas. El sitio parece haber migrado de dominio y los enlaces indexados están rotos |
| Búsqueda de resoluciones de la SAPP sobre exoneración o libre paso | Sin resultados. Todo remite a la misma enumeración de una fuente comercial ya fichada como `[P]` |

**Diagnóstico:** este pendiente **no se cierra con investigación web**. La lista de exoneraciones vive en el contrato de concesión y en el reglamento de operación de las estaciones, y ninguno de los dos está publicado en formato consultable. **Se cierra con una solicitud formal a la SAPP —** que como regulador está obligada a responder — **o con una consulta al área de atención al cliente de COVI-H.** Es una gestión de la institución, no una tarea de investigación.

`[P]` **Precedente útil para argumentar la solicitud:** el mismo patrón de exoneración funcional aparece en otra norma del dominio. En el régimen de circulación de vehículos del Estado en días inhábiles, los exceptuados del control son los destinados a **emergencias, seguridad, defensa y salud**, más los de la CONAPREMM — y **no** los vehículos administrativos. **Dos regímenes distintos, el mismo criterio: exonera la función, no el dueño.** Refuerza la conclusión de trabajo sin verificarla.

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

**Reordenados el 2026-08-24.** Se separa lo que se cierra con investigación de lo que solo se cierra preguntando.

### Se cierran preguntando — no hay más que investigar

1. `[C]` **Lista oficial de exoneraciones** y si alcanza a vehículos administrativos del Estado. **Cuatro vías agotadas** — ver §5. Requiere solicitud formal a la SAPP o consulta a COVI-H.
2. `[P]` **Confirmar que la tarifa congelada sigue vigente hoy.** Ya no es una contradicción abierta: es un hueco de seis meses sobre un congelamiento que era condicional y sin plazo. Una llamada a COVI-H lo cierra.
3. `[C]` **Facturación**: ¿factura fiscal en caseta? ¿CoviPass empresarial con estado de cuenta a nombre de la institución? `covih.com` bloquea toda consulta automatizada; **solo se resuelve por teléfono o en ventanilla.**
4. `[C]` Toda la realidad operativa de la §8, incluida la frontera peaje ↔ viático ↔ ARGOS.

### Se cierran con acceso a un documento

5. `[C]` **Texto oficial del Acuerdo No. 1012-2021** — es el fundamento probable de la clasificación de peaje **y** la fuente de la matriz de [NRM-06](NRM-06-transito-y-licencias.md). **El PDF tiene capa de texto**; no se pudo abrir por limitación del entorno de investigación, no del documento. Ver *Limitación de herramienta* en NRM-06.
6. `[C]` **Artículo 51 de la Ley de Tránsito** — **degradado de prioridad.** Ya no se sostiene que sea el criterio de liviano vs. pesado; ver la corrección en §2. Vale la pena leerlo para descartar, no para fundar.
7. `[C]` **PDF de tarifas de la SAPP** — contrastar contra el HTML y, sobre todo, **buscar si trae fecha de vigencia**, que es lo que hoy falta.
8. `[C]` Cláusula 9.4 y fórmula exacta de indexación — solo conocida por prensa.
9. `[C]` Si hoy se cobra peaje en el Corredor Turístico.
10. `[C]` Verificar PCM-064-2019.

### Nuevos, abiertos el 2026-08-24

11. `[C]` **¿Se cerró la negociación SIT–COVI del ajuste gradual a cuatro años?** Si se cerró, hay cuatro tramos tarifarios con fecha conocida que se pueden cargar de una vez como parámetros con vigencia. Es el mejor escenario posible para el modelo de la §9.
12. `[C]` **¿Habrá lectura RFID de placa en las estaciones de peaje, y será el dato accesible a la institución?** Ver [NRM-06](NRM-06-transito-y-licencias.md). Si lo fuera, el paso por caseta dejaría de depender de la declaración del motorista. **No se diseña nada hoy**; se registra para no rehacer el modelo después.

## Fuentes

### Añadidas el 2026-08-24

**Oficiales**
- [SAPP — Corredor Logístico, tabla de tarifas](https://www.sapp.gob.hn/es/corredor-log%C3%ADstico) — **reconsultada el 2026-08-24**: sigue publicando L. 22 como tarifa de liviano, presentada como actual, **sin fecha de vigencia ni de actualización**
- [SAPP — Comunicado de reclasificación vehicular, 17/09/2025](https://www.sapp.gob.hn/post/sapp-atiende-denuncias-ciudadanas-y-comunica-suspensi%C3%B3n-de-incremento-en-cobro-de-peaje-a-veh%C3%ADculos) — **reconsultado el 2026-08-24: no cita el Art. 51**
- `sapp.gob.hn/comunicado/` — *"SAPP NO AUTORIZÓ AJUSTES A TARIFAS DE PEAJE"*. **Indexado por buscadores, HTTP 404 al consultar.** El sitio parece haber migrado y los enlaces indexados están rotos
- `sapp.gob.hn/descartan-aumento-a-la-tarifa-de-peaje-en-la-carretera-ca-5/` — misma situación: indexado, HTTP 404
- `covih.com`, `covih.com/tarifas`, `covih.com/covipass` — **HTTP 403 en todas las rutas**
- `ppp.worldbank.org` — contrato de concesión: **HTTP 403**

**Prensa — cronología de 2026**
- [Dinero HN — COVI aumenta 41 % la tarifa, 08/01/2026](https://dinero.hn/covi-honduras-aumenta-en-41-la-tarifa-de-peaje-en-la-carretera-ca-5/) — tabla completa del aumento anunciado y vencimiento del acuerdo de 2024 el 14/01
- [La Prensa — COVI y Gobierno retoman diálogo](https://www.laprensa.hn/portada/covi-honduras-gobierno-dialogan-ajuste-tarifas-peaje-2026-AO29500145)
- [Hondumedios — Gobierno y COVI acuerdan mantener tarifas](https://hondumedios.hn/gobierno-y-covi-honduras-acuerdan-mantener-tarifas-de-peaje/) — **sin plazo ni vigencia declarada**
- [Hondudiario — Subsidio parcial frena el incremento, 02/03/2026](https://www.hondudiario.com/2026/03/02/subsidio-parcial-de-la-ca-5-frena-incremento-del-peaje-mientras-se-negocia-nuevo-acuerdo/) — **la fuente más reciente y más precisa**: subsidio parcial, ajuste gradual a cuatro años, deuda >USD 10 M
- [El Espectador HN — El Gobierno de Asfura confirma que la tarifa se mantendrá durante la negociación, 02/03/2026](https://elespectador.hn/tarifa-peaje-ca5-se-mantendra-negociacion/) — declaración del Secretario Aníbal Ehrler
- [Proceso Digital — Gobierno negocia aumento paulatino y revisa subsidio pendiente](https://proceso.hn/gobierno-negocia-aumento-paulatino-del-peaje-en-la-ca-5-y-revisa-subsidio-pendiente-por-mas-de-10-millones/)
- [La Prensa — SAPP anuncia suspensión del aumento a vehículos de trabajo](https://www.laprensa.hn/honduras/sapp-anuncia-suspension-aumento-cobro-peaje-vehiculos-trabajo-honduras-BA27411110) — **cita el fundamento: Ley de Tránsito y Reglamento Especial en Materia de Permisos de Conducir**
- [El Heraldo — La tarifa para transporte liviano vuelve a L. 22](https://www.elheraldo.hn/honduras/tarifa-peaje-transporte-liviano-vuelve-22-lempiras-informa-sapp-CB27407498) — **confirma que los pick-ups son categoría liviana**

**Descartada como fuente de tarifa**
- TollGuru — publicó el aumento anunciado en enero y **no registró la reversión**. Modo de fallo típico del agregador comercial: registra el anuncio, no el desenlace.

### Consultadas el 2026-08-06

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
