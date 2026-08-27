# Dictamen de elementos visuales

| Campo | Valor |
|---|---|
| **Ámbito** | Las **138 pantallas** del [inventario](inventario-de-pantallas.md). Para cada una: cuál es el elemento visual primario adecuado y por qué |
| **Qué lo origina** | Objeción del PO: *«No es posible que de 50 pantallas, las 50 sean solo tablas en el front.»* Verificada: `mockups/tablero-de-mockups.html` (1,4 MB, 41 identificadores `PT`) tiene **un solo `canvas` y cuatro `svg`** — cero líneas de tiempo, cero calendarios, cero mapas, cero gráficos. Las dos pantallas que el inventario llama «Tablero» (`PT-038`, `PT-058`) se maquetaron como listas |
| **Qué hace este documento** | **Señala.** No corrige mockups, no toca el inventario, no escribe código |
| **Autoridad** | Ninguna sobre reglas de negocio. Donde este dictamen y una `RN-xx`, un estado o la matriz de permisos se rocen, manda la autoridad de `CLAUDE.md` |
| **Fecha** | 2026-08-27 |

---

## 1. El criterio

**El elemento primario lo decide el acto que el usuario ejecuta en esa pantalla, no la forma del dato.** La pregunta es siempre *«¿qué tiene que ver para decidir?»*:

| Si el usuario… | El elemento primario es |
|---|---|
| compara filas y elige una | **tabla** |
| necesita ver **solapes en el tiempo** | **línea de tiempo por carriles** — un carril por recurso |
| necesita ver **disponibilidad a lo largo de días** | **calendario** |
| necesita ver **dónde** está algo | **mapa** — integrado de ARGOS, nunca propio |
| necesita ver una **desviación o una tendencia** | **gráfico** |
| necesita comparar **dos versiones** | **comparador lado a lado** |
| **captura** datos | **formulario** |
| produce un **documento que se imprime** | **maqueta de papel** con folio, QR y espacio de firma |
| lee un expediente y **decide sobre él** | **ficha de decisión** — una pantalla, sin pestañas |
| **no puede** hacer algo | **pantalla de mensaje** — qué se impidió, por qué, cómo salir (`R-3`) |

**No se fuerza variedad.** Una lista de decisión con columnas ordenables es la respuesta correcta 23 veces en este sistema, y meter un gráfico donde no aporta es peor que la tabla. Lo que estaba mal no es que hubiera tablas: es que fueran **todas**.

### 1.1 Tres restricciones que atraviesan todo el dictamen

**a. ARGOS posee el componente de mapas** ([`DP-001`](../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). Donde este dictamen dice «mapa», el dictamen es **integrar el componente de ARGOS**, no construir uno. SIGTI aporta la capa de negocio —puntos de peaje, hitos de la misión, antigüedad del dato— y ARGOS aporta el lienzo. Ninguna pantalla de este documento autoriza construir mapa propio.

**b. El cliente de campo es otro producto y otro stack.** Las 34 superficies de campo corren en React Native sobre gama baja, con [`RNF-12`](../02-requisitos/no-funcionales/RNF-12-uso-en-campo.md) exigiendo **≤ 25 % de batería en 8 h** y [`RNF-03`](../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md) exigiendo cero red. Ahí el dictamen es deliberadamente **conservador**: ningún mapa, ningún gráfico, ninguna animación, ninguna carga diferida. Un mapa en campo cuesta GPS activo, tiles y memoria; el motorista necesita saber que se guardó su registro, no verse en un mapa que ya sabe dónde está. Donde la columna LOKI dice «no aplica», es porque **LOKI es del cliente administrativo y no cruza el puente**.

**c. Híbrido digital-papel por diseño.** Las pantallas de M-15 y las 29 bloqueadas por el insumo #2 no producen una vista: producen un **documento físico**. Su elemento primario es la maqueta impresa con folio, QR, espacio de firma y sello, y hash al pie (§6 del inventario). El orden de sus campos lo fija el formato de la institución, no el diseño.

### 1.2 Cómo se lee la columna LOKI

Sobre el sistema de diseño real (`oficina/src/ui/index.ts`):

| Valor | Significa |
|---|---|
| **Existe** | Se arma con lo que ya hay: `Tabla`, `Panel`, `Campo`, `Nota`, `Pastilla`, `FilaKpis`, `Modal`, `RastreadorEtapas`, `CajonExpediente`, `Segmentado`, `SelectorBuscable`, `CampoFecha`, `RangoFechas`, `TarjetaOpcion`, `Vacio`, `Paginador` |
| **Existe + falta** | La estructura existe; falta la pieza que lleva el peso de la decisión |
| **Construir** | No hay nada equivalente en LOKI |
| **Integrar** | Lo aporta ARGOS |
| **Campo** | Cliente de campo — LOKI no aplica, se diseña en React Native |

> **Hallazgo verificado, y no menor:** el barril de LOKI documenta un módulo de gráficos con carga dinámica (`graficos/nucleo`), pero **la carpeta `oficina/src/ui/graficos/` no existe en este repositorio**. Hoy no hay ni un componente de gráfico disponible. Toda pantalla de este dictamen que pida gráfico pide construcción, no importación.

---

## 2. Dictamen por módulo

### 2.1 Transversales

| PT | Elemento primario | Secundarios | Por qué — la decisión que habilita | LOKI |
|---|---|---|---|---|
| PT-001 | Tarjetas de puesto vigente | Aviso de puesto único | Elige **con qué puesto trabaja**, y de eso depende toda la raíz (`R-1`): son 2–3 opciones, no una lista que se ordena | Existe (`TarjetaOpcion`) |
| PT-002 | Tabla de pendientes del puesto | Contadores por tipo, alertas con plazo | Responde *«¿qué me toca ahora?»* eligiendo una fila (`R-2`). Es legítimamente una tabla | Existe |
| PT-003 | Tabla de tareas escaladas | Antigüedad en días, motivo del escalamiento, quién escaló | Elige cuál destraba primero; el criterio es la antigüedad comparada | Existe |
| PT-004 | Pantalla de mensaje de bloqueo | Camino de salida, a quién acudir | No hay dato que comparar: hay algo que **no se puede hacer**. `R-3` exige tres partes y ninguna es tabular | Existe (`Panel`+`Nota`) · versión de campo aparte |
| PT-005 | Tabla de resultados agrupada por tipo de expediente | Paleta de comandos, filtros de alcance | Busca uno entre muchos y lo abre | Existe (`PaletaComandos`) |

### 2.2 M-06 Solicitud de transporte

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-007 | **Formulario réplica del papel** | Maqueta del formato original al lado como referencia | El operador debe encontrar los mismos campos en el mismo orden. Bloqueada por insumo #2 | Existe · orden `[C]` |
| PT-008 | Formulario condicional | Segmentado personas/carga/mixto · **barra de ocupación** peso y plazas contra la capacidad del tipo | Declara qué se mueve y necesita ver si **cabe** antes de enviar; un número suelto no dice si cabe | Existe + falta barra de capacidad |
| PT-009 | Desglose de líneas por punto de peaje | **Mapa de ARGOS** con los puntos sobre la ruta · identificador del tarifario vigente | `R-8`: el total no se puede autorizar. Se decide punto por punto con su categoría por ejes | Existe + **integrar** mapa |
| PT-010 | **Calendario de la ventana solicitada** con días y horas inhábiles sombreados | Aviso sin bloqueo, enlace al trámite del permiso | La decisión es *«¿mi viaje toca día inhábil?»*, y eso es una relación con el calendario, no una lista de fechas | **Construir** |
| PT-006 | Tabla de mis solicitudes | Etapa por fila, antigüedad | Elige cuál consulta o corrige | Existe |
| PT-011 | Ficha de confirmación con el expediente congelado | Constancia con número, aviso de congelamiento | Es un acto irreversible: lo que se ve es **qué se está congelando** | Existe |
| PT-012 | Formulario mínimo de convalidación | Aviso del plazo, quién convalida | Captura de un hecho ya ocurrido. Parcial · pertenencia al Nivel 3 sin resolver (`DP-002`) | Existe · `[C]` |

### 2.3 M-06 Autorización

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-013 | **Tabla** — y es la respuesta correcta | Validaciones evaluadas en la fila · desglose de peajes expandible · misiones sin liquidar con antigüedad · antigüedad del espejo de ARGOS | Compara solicitudes y decide una por una, ordenadas por salida más próxima. La tabla no es el problema aquí: el problema sería que la fila no traiga las tres validaciones que cambian la decisión (§7.2 del mapa) | Existe · construida |
| PT-014 | **Ficha de decisión en una pantalla** | Desglose de peajes, línea de tiempo de la solicitud, tres actos visualmente distintos | Dos toques: uno para entender, uno para decidir. Sin pestañas y sin tabla: es **un** expediente, no varios | Existe · construida |
| PT-015 | Ficha del acto + constancia inmutable | Vista previa de la constancia impresa | Ve exactamente **sobre qué contenido** firma, porque no hay firma electrónica certificada | Existe |
| PT-016 | Formulario de motivo | Aviso de terminalidad, enlace a solicitud nueva vinculada | `RECHAZADA` mata el expediente: lo que hay que ver es la consecuencia, no una lista | Existe |
| PT-017 | **Comparador lado a lado** versión enviada ↔ versión corregida | Formulario de motivo de devolución | Devolver conserva el expediente y **versiona**: el solicitante tiene que ver qué le piden cambiar, no leer un párrafo | **Construir** |
| PT-018 | Escalera de competencia (etapas con el nivel alcanzado y el que falta) | Umbral superado con su cifra y su tabla vigente | Necesita ver **hasta dónde llega su firma y quién sigue**, que es una secuencia, no una tabla | Existe + extender `RastreadorEtapas` |
| PT-019 | **Franja de vigencia de la delegación** contra la fecha del acto | Documento que respalda la delegación | La única pregunta es si el acto cae dentro del rango delegado. Una fecha de inicio y una de fin en texto obligan a hacer la cuenta mentalmente | **Construir** (primitiva de carriles) |

### 2.4 M-04 / M-15 Permiso de circulación en día u hora inhábil

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-020 | Formulario réplica del papel | **Calendario de días inhábiles** con la ventana amparada marcada · elementos amparados | Tramita un permiso para un rango que hay que ver contra el calendario institucional. Bloqueada #2 para los campos; el calendario se diseña hoy | Existe + **construir** calendario |
| PT-021 | **Ficha de firma en dos toques, cuerpo grande** | Vista previa del salvoconducto, motivo de la circulación | `ACT-09` entra desde el celular y pocas veces: si no cabe en una pantalla, delega su clave — que es el riesgo que se quiere evitar | Existe |
| PT-022 | **Tabla con selección múltiple** | Calendario del feriado como encabezado · reporte previo por misión | Firma en lote: compara y marca varias. Es tabla legítima, con el matiz de que el reporte previo va **antes** de firmar | Existe |
| PT-023 | **Maqueta de papel — el caso más exigente del sistema** | QR grande en posición fija, hash al pie | Lo lee un agente en carretera, de pie, de noche, con linterna. Los cuatro datos —vehículo, ventana autorizada, autoridad que firmó, vigencia— en el tercio superior y en cuerpo grande | **Construir** hoja · tensión con la paridad papel declarada en §6 del inventario, sin resolver |
| PT-024 | **Comparador lado a lado** folio anterior ↔ folio nuevo | Maqueta del documento nuevo con la leyenda «sustituye al folio X» | Es un documento **nuevo**, no la edición del vigente. Si la pantalla parece un formulario de edición, se pierde el rastro que `RNF-21` exige | **Construir** |

### 2.5 M-07 Programación y asignación

Es el módulo donde la omisión duele más: **cuatro de sus diez pantallas deciden sobre solapes en el tiempo y todas se maquetaron como listas.**

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-025 | Tabla de la cola | **Barra de caducidad** de la aprobación por fila · panel lateral con la ocupación del día | Elige qué programa primero; el criterio es cuánto le queda a la aprobación antes de caducar, y una fecha en texto no transmite urgencia | Existe + falta barra · construida sin ella |
| PT-026 | **Línea de tiempo por carriles de la flota** — un carril por vehículo, barras por misión | Tabla de candidatos compatibles · bloqueo de compatibilidad · documentación y estado operativo | Decidir **qué vehículo** exige ver quién está ocupado, cuándo y con qué hueco. Con una lista, la única forma de saber si el pick-up está libre el jueves es abrir las misiones una por una — el propio diseño lo detectó y tuvo que inventar el cronograma (`mockups §4.b`) | **Construir** · construida sin ella |
| PT-027 | Tabla de candidatos con su habilitación | **Franja de vigencia de la licencia contra la ventana de la misión** · relevos | Elige titular y relevos comparando habilitación; la vigencia se decide viendo si la licencia cubre **todo** el rango, incluida la holgura | Existe + **construir** franja |
| PT-028 | **Pantalla de mensaje de bloqueo** | Franja que muestra dónde cae el vencimiento respecto a la ventana · tabla de conductores que sí habilitan · tabla de vehículos que la licencia sí habilita · versión de la matriz usada | El usuario **no lo resuelve reintentando**: tiene que hacer una gestión administrativa. La pantalla existe para decirle exactamente qué categoría se necesita y qué caminos tiene (§7.5) | Existe · construida · falta la franja |
| PT-029 | **Línea de tiempo por carriles** | Ficha del titular de la reserva, motivo del conflicto | El conflicto con el titular de una reserva exclusiva **es** un solape en el tiempo. Dibujarlo como lista es esconder el dato de la decisión | **Construir** |
| PT-030 | **Línea de tiempo por carriles** + **mapa de ARGOS** | Tabla de solicitudes candidatas, capacidad remanente | Consolidar exige que dos solicitudes coincidan **en tiempo y en ruta**: son dos preguntas y ninguna es tabular | **Construir** + **integrar** |
| PT-031 | Maqueta del documento de constancia | Lista de verificaciones con marca de tiempo y tabla normativa usada | Es material probatorio: se imprime y se anexa | **Construir** hoja |
| PT-032 | **Línea de tiempo por carriles** | Comparador antes ↔ después de la sustitución, motivo | Sustituir en `PROGRAMADA` es encontrar un hueco equivalente en otro carril | **Construir** |
| PT-033 | **Línea de tiempo por carriles** + línea de tiempo de la misión en curso | Mapa con el punto de la sustitución, corte de odómetro | Con la misión ya `DESPACHADA` hay que ver **dónde va** y qué queda partido en dos | **Construir** + **integrar** |
| PT-138 | **Cuadrícula de compatibilidad par a par por tramo** | Tabla de tramos, motivos de incompatibilidad | `HU-125` evalúa pares (externa ↔ personal ↔ carga) tramo a tramo: es una matriz, y una lista de pares la vuelve ilegible en cuanto hay cuatro personas | **Construir** |

### 2.6 M-15 Emisión de documentos

Los cuatro son documentos. Su elemento primario es la hoja, no la pantalla.

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-034 | **Maqueta de papel con marca de agua «NO VÁLIDA»** | Folio reservado con su aclaración, botón de emitir | `R-10`: el usuario verifica el documento **antes** de gastar el folio | **Construir** hoja |
| PT-035 | **Maqueta del juego documental** (orden de misión, peajes, advertencias, bitácora) | Lista de piezas del juego con su estado de impresión | Emite varias hojas de una vez: lo que decide es qué piezas se imprimen y en qué orden salen del cajón | **Construir** hoja |
| PT-036 | **Maqueta con marca de reimpresión** | Tabla del historial de impresiones: quién, cuándo, cuántas | Mismo folio, y el historial es control interno: la reimpresión repetida es un patrón que Auditoría mira | **Construir** hoja |
| PT-037 | **Maqueta emitida sin conectividad** | **Medidor del rango de folios** de la delegación: consumidos y restantes | Con rangos de folio por delegación (ADR irreversible), agotar el rango sin verlo venir deja a la delegación sin poder emitir | **Construir** hoja + medidor · dual, versión de campo aparte |

### 2.7 M-07 / M-08 Despacho y retorno

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-038 | **Línea de tiempo del día por carriles** — salidas y retornos previstos sobre el eje de horas | Tabla de la ráfaga de la mañana · antigüedad del dato si está degradado | Es la raíz del despachador a las 5:30 de la mañana con ocho salidas encimadas. La pregunta es *«¿qué sale ahora y qué se me traslapa?»*, y una lista ordenada por hora no muestra el traslape. **El inventario la llama «Tablero» y se maquetó como lista** | **Construir** |
| PT-039 | Formulario réplica de la hoja de salida | Panel de revalidación **con marca de tiempo de cada verificación** · kilometraje · novedades de inspección | Entre programar y salir pasan días: hay que ver *qué se revalidó y cuándo*, no solo el resultado | Existe · orden `[C]` |
| PT-040 | **Maqueta del acta** | **Croquis del vehículo para marcar daños** · visor de fotos de la inspección · dos firmas | El acta la firman dos personas y su contenido es el estado físico del bien: un campo de texto libre no sirve para reclamar después | **Construir** hoja + croquis |
| PT-041 | **Maqueta de la constancia de entrega** | Estado de la entrega para el despachador, que **ve y no ejecuta** (`I-08`) | Es custodia de efectivo o vale contra firma. Para `ACT-05` no hay botón: hay espera | **Construir** hoja · folio `[C]` |
| PT-042 | Formulario réplica de la bitácora | **Línea de tiempo de la misión** para cotejar hitos contra lo que trae el motorista · visor de evidencia | Cierra la bitácora cotejando lo registrado en ruta con lo que llega en papel | Existe + **construir** línea de tiempo |
| PT-043 | Formulario de resguardo en sitio | **Mapa de ARGOS** con dónde quedó el bien · ficha de quién lo custodia | El dato de la decisión es **dónde** quedó el vehículo y bajo la custodia de quién | Existe + **integrar** |

### 2.8 M-09 Combustible

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-044 | Formulario réplica | **Gráfico de consumo de los períodos anteriores** para dimensionar la solicitud | Pedir un fondo sin ver el consumo histórico produce un número inventado que después se amplía de urgencia | Existe + **construir** gráfico |
| PT-045 | **Barra de la cuota**: comprometido, ejecutado y disponible sobre el techo de la partida | Tabla de solicitudes del período · bloqueo `I-19` | La decisión es *«¿cabe en la cuota?»*. Tres cifras sueltas obligan a hacer la resta; la barra la muestra hecha | **Construir** |
| PT-046 | Tabla ordenable por prelación | Barra de saldo remanente, motivo de la ampliación | Resuelve un orden entre solicitudes que compiten: comparar filas y ordenarlas es exactamente una tabla | Existe |
| PT-047 | **Maqueta del vale con folio** | Saldo del fondo, bloqueo por reintegro pendiente | Es el documento que el motorista se lleva | **Construir** hoja |
| PT-048 | **Maqueta de la constancia de entrega** | Firma del receptor, saldo tras la entrega | Custodia contra firma | **Construir** hoja |
| PT-049 | **Maqueta del acta de anulación** | Estado del vale, motivo, bloqueo si ya fue canjeado | `R-5`: la anulación es un asiento, y el acta se archiva | **Construir** hoja |
| PT-050 | **Tablero por estado del vale** — columnas emitido · entregado · canjeado · conciliado · anulado, con conteo y monto | **Cuadre del arqueo**: saldo teórico contra contado · gráfico del saldo en el tiempo | Es un objeto con ciclo de vida y un fondo que hay que cuadrar. Una tabla plana de vales esconde justo lo que se busca: **dónde se está atascando el dinero** | **Construir** |
| PT-051 | Tabla de abastecimientos con su fuente | **Agrupador de comprobantes repetidos** (unicidad) · visor del comprobante | Declara fuente y verifica que ningún comprobante se use dos veces: comparar filas es lo correcto, pero el duplicado tiene que saltar solo | Existe + falta agrupador |

### 2.9 M-16 Sincronización y conflictos

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-052 | Tabla de dispositivos | **Carriles de última sincronización** por dispositivo · antigüedad en días | Elige a qué delegación llamar. La tabla sirve; los carriles muestran de un vistazo cuál lleva nueve días callado | Existe + **construir** carriles |
| PT-053 | **Tabla ordenada por impacto y luego antigüedad** — y es correcta | Qué bloquea cada conflicto, en voz alta · contador de alto impacto excluido del lote | Elige cuál resuelve primero comparando impacto. Aquí la tabla no es pereza: es la forma correcta de una cola priorizada (§7.1, punto 10) | Existe |
| PT-054 | **Comparador lado a lado, campo por campo** | **Las dos fotografías visibles al mismo tiempo**, no detrás de un clic · quién capturó, cuándo ocurrió, cuándo se registró · motivo obligatorio | Es la pantalla más difícil del sistema. Lo que resuelve el conflicto en la práctica es la foto del tablero contra la foto del original, y eso es imposible en una tabla o en pestañas | **Construir** — la pieza de mayor valor del sistema |
| PT-055 | Tabla con selección múltiple | **Panel de exclusión**: los conflictos de odómetro, monto y autorización que el lote deja fuera, contados y nombrados | 180 conflictos uno por uno no los resuelve nadie; lo que hay que ver es **qué no entra en el lote** | Existe + falta panel |
| PT-056 | Tarjetas de estado del espejo con su antigüedad | Minigráfico de antigüedad en el tiempo · umbral `[C]` | Son dos o tres integraciones, no una lista: lo que decide es si el espejo está lo bastante fresco para autorizar | Existe (`FilaKpis`) + **construir** minigráfico |
| PT-057 | **Comparador**: lo que se cerró ↔ lo que llegó después | Ficha de la decisión, motivo | Mismo problema que `PT-054` con otra causa: no se edita el cierre, se elige o se asienta | **Construir** |

### 2.10 M-19 Seguimiento en ruta

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-058 | **Mapa integrado de ARGOS** con la flota en ruta | Tabla con la **antigüedad del dato** por vehículo · línea de tiempo del día · estado declarado por el motorista | La pregunta literal es *«¿dónde está cada vehículo?»*. **El inventario la llama «Tablero de seguimiento» y se maquetó como lista.** La antigüedad del dato es obligatoria en las dos vistas: un punto en el mapa sin decir que es de hace seis horas miente | **Integrar** (`DP-001`) — no construir mapa propio |
| PT-059 | **Línea de tiempo de la misión** con sus hitos | Mapa de ARGOS · tabla de hitos · tiempos de espera en sitio | Los hitos son una secuencia con duraciones —incluida la espera en sitio, que después ampara desviaciones de combustible— y eso no se lee en filas | **Construir** + **integrar** |
| PT-060 | **Comparador**: alcance autorizado ↔ alcance solicitado | Mapa con el tramo nuevo · versionado · quién autoriza la ampliación | Autoriza una diferencia; lo que hay que ver es exactamente la diferencia | **Construir** + **integrar** |
| PT-061 | Ficha del hecho con las tres salidas | Visor de la evidencia enviada por el motorista · mapa · guía de actuación | El motorista mandó cuatro datos y una foto: la oficina decide el desenlace. Lo que se ve es **un** hecho, no una lista | Existe + **construir** visor |
| PT-062 | Ficha de resolución del relevo | Línea de tiempo con el corte de odómetro · verificación de habilitación del relevo · maqueta del acta | Resuelve desde oficina un acto que ocurre en carretera: lo que decide es si el relevo está habilitado y dónde se parte la bitácora. Parcial | Existe + **construir** |

### 2.11 M-13 Liquidación y cierre

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-063 | **Tabla** — correcta | Qué bloquea cada misión, con el número de divergencias y enlace directo | Elige qué liquida; el criterio es qué está desbloqueado | Existe · construida |
| PT-064 | **Gráfico de desviación con dos bandas de tolerancia visiblemente asimétricas** | Desglose por tramo y por carga · tabla de rendimiento esperado con su vigencia · N bloques si hubo sustitución · estado *no concluyente* con odómetro averiado · alerta agregada por vehículo | Es lo que va a mirar el Tribunal Superior de Cuentas. La desviación se mira **en las dos direcciones** y significan cosas distintas: un rendimiento demasiado bueno casi siempre es un despacho que nadie anotó. Un número en una celda no enseña eso; dos bandas asimétricas sí | **Construir** — no hay módulo de gráficos en el repo |
| PT-065 | **Cuadre a dos columnas**: entregado ↔ justificado, con la diferencia tipificada | Tabla de comprobantes · sobrante y faltante con su tipificación | Conciliar es cerrar una diferencia: verla partida en dos columnas con el saldo al medio es la forma del acto. Parcial | **Construir** |
| PT-066 | **Tabla de cotejo punto por punto**: estimado ↔ cobrado | Visor del recibo · mapa del punto discrepante · tarifario vigente con su fecha | Compara líneas y marca las que no cuadran: tabla legítima, siempre que la diferencia esté calculada en la fila | Existe + **integrar** |
| PT-067 | Pantalla de mensaje de bloqueo | El par incompatible con la misión concreta · quién sí puede liquidar | `I-10` es núcleo irreductible: la acción no existe, no está en gris | Existe |
| PT-068 | **Cadena de eslabones con los huecos visibles** | Propuesta de cierre · destino: Gerencia Administrativa, no un botón de cerrar | La propuesta de cierre se sostiene o se cae según la cadena esté completa: los eslabones que faltan tienen que **verse como huecos**, no omitirse | Existe + extender `RastreadorEtapas` |
| PT-069 | Ficha de cierre con la cadena a la vista | Hallazgos detectados, motivo | Un acto irreversible sobre un expediente | Existe · construida |
| PT-070 | Formulario de tipificación del hallazgo | Catálogo de tipos, cadena con lo que falta | Clasifica, y la clasificación después se reporta | Existe |
| PT-071 | Ficha del expediente nuevo **vinculado** al cerrado | Vínculo visible entre ambos expedientes | No hay reapertura: lo que hay que ver es la relación entre dos expedientes, y un enlace suelto no la comunica | Existe + falta vínculo |

### 2.12 M-03 / M-04 Flota y expediente del vehículo

> *«Así como Talento Humano cuida de todo lo referente a los empleados, SIGTI cuida de todo lo referente a los vehículos.»* Un expediente de primera clase **no se ve como una tabla de atributos.**

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-072 | **Tabla del padrón** — correcta | Carriles de disponibilidad de los próximos días · barra de composición por estado operativo | Compara vehículos y elige uno | Existe + **construir** carriles |
| PT-073 | **Línea de tiempo de la vida del vehículo**: altas, custodios, documentos, mantenimientos, incidentes, misiones | Ficha de datos permanentes · cajón de expediente por materia · vencimientos | Es el ciclo de vida completo de una entidad de primera clase. La pregunta del jefe de transporte es *«¿qué le ha pasado a este vehículo?»*, y esa es una historia en el tiempo, no un formulario de solo lectura | **Construir** · `CajonExpediente` como base |
| PT-074 | Formulario réplica del alta de bien | Visor del título de tenencia · régimen de tenencia | Captura con respaldo documental | Existe · orden `[C]` |
| PT-075 | Formulario del estado de la lámina | Visor de fotos · **identificadores alternativos** cuando no hay placa | Sin placa es estado válido: la pantalla tiene que hacer evidente **con qué se identifica** el vehículo mientras tanto | Existe |
| PT-076 | Formulario de ficha técnica | **Cuadro de derivación**: peso bruto y ejes → categoría de licencia exigida y categoría de peaje | Estos campos no son inventario: **habilitan o no** al motorista y fijan la tarifa. Si la consecuencia no se ve al digitar, se digita mal. Parcial | Existe + **construir** cuadro |
| PT-077 | **Maqueta de la tarjeta de responsabilidad** | **Línea de custodios en el tiempo** · firma del receptor | Documento que se firma y traspaso que deja historia | **Construir** hoja + carriles |
| PT-078 | **Línea de vigencias por documento** — matrícula, seguro, revisión, permisos | Tabla de vencimientos · alertas dirigidas al puesto · qué bloquea y qué solo advierte | La decisión es *«¿qué se me vence y cuándo?»*: cinco fechas en filas obligan a compararlas mentalmente; cinco bandas sobre un eje muestran el mes crítico | **Construir** |
| PT-079 | **Lista de verificación con semáforo** | Enlace a cada condición no cumplida | Habilitar es cumplir N condiciones: se ve como una lista de comprobación, no como una tabla que se ordena | Existe |
| PT-080 | **Maqueta del acta de descargo** | Resolución que lo respalda, estado final del bien | Documento oficial | **Construir** hoja |
| PT-081 | **Maqueta del acta de retiro** | Vigencia del comodato o alquiler, devolución al propietario | Documento oficial sobre un bien ajeno | **Construir** hoja |

### 2.13 M-05 Motoristas y habilitación

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-082 | **Tabla del padrón** — correcta | **Franja de vigencia** de la licencia por fila · calendario de disponibilidad · categorías habilitadas | Compara motoristas y elige uno | Existe + **construir** franja · maquetada sin ella |
| PT-083 | Formulario de captura de la licencia | **Cotejo con la fotografía de la licencia al lado**, campo por campo | Se digita desde el documento físico: tenerlo a la vista mientras se digita evita el error que después bloquea un despacho. Parcial | Existe + **construir** cotejo |
| PT-084 | **Matriz licencia × tipo de vehículo** — nueve categorías `A`, `B1`, `B`, `C1`, `C`, `D1`, `D`, `BE`, `CE` | Nota de que `BE` es B con remolque y de que **el remolque no es articulado** · norma citada | Es una relación de dos ejes. En lista, el caso que el bloqueo existe para impedir —el pick-up con plataforma— se pierde. `[V]` Artículo 4 del Acuerdo 1012-2021 | **Construir** |
| PT-085 | **Línea de vigencias de las habilitaciones** | Tabla de próximos vencimientos, alertas anticipadas | Anticipar es ver el futuro cercano en un eje. Hoy el mockup no la dibuja: dice que «la alerta se ve en el padrón», y eso no permite planificar | **Construir** |
| PT-086 | Formulario con el mismo rigor del padrón | Franja de vigencia · pantalla de bloqueo si no habilita | El conductor declarado se somete a la misma verificación (`RN-57`) | Existe |
| PT-087 | **Línea de tiempo por carriles de las misiones afectadas** por el rango de la inhabilitación | Formulario de causa · tabla de reasignación | Inhabilitar a alguien rompe N misiones futuras: hay que ver **cuáles caen** antes de firmar | **Construir** |

### 2.14 M-14 Auditoría y reportes

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-088 | **Tabla con filtros** — correcta | Histograma de actividad por fecha y actor para encontrar el pico | Filtra y rastrea: es exactamente comparar filas | Existe + **construir** histograma |
| PT-089 | **Cadena de eslabones de extremo a extremo, con los huecos visibles** | Cada eslabón con quién, cuándo, con qué puesto · exportación | El auditor busca **dónde se rompe la cadena**. Una tabla de eventos ordenada por fecha oculta el hueco, que es justo lo que se busca | Existe + extender `RastreadorEtapas` |
| PT-090 | Ficha de armado del paquete con su inventario de piezas | Maqueta del índice del PDF · estado de cada anexo · sello de tiempo | `RNF-18` exige **el mismo día y completo**: no puede ser un botón que produce un CSV, tiene que verse qué lleva el paquete | Existe + **construir** hoja de índice |
| PT-091 | Tabla de intentos bloqueados | **Gráfico de frecuencia** por par incompatible y por actor | El intento aislado es ruido; el patrón es el control. Ver qué par se intenta 40 veces al mes es lo que produce una acción | Existe + **construir** gráfico |
| PT-092 | **Línea de vigencias de las versiones del parámetro** | Comparador entre dos versiones · respaldo documental · quién puso en vigencia | La pregunta es *«¿qué tabla estaba vigente el día del hecho?»* (`R-7`), que es una posición en un eje de tiempo. ⛔ sin `CU-19` | **Construir** · `[C]` |
| PT-093 | **Tabla** — correcta | Filtros por usuario, registro y período | Registro de consultas: se filtra y se lee | Existe |

### 2.15 M-17 Traslado de personas externas

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-094 | **Maqueta del manifiesto** | Tabla de personas con datos mínimos · aviso de registro de la consulta | Documento con folio que viaja en el vehículo. Bloqueada #2 | **Construir** hoja · dual |
| PT-095 | **Ficha con revelación mínima**: solo lo que el puesto necesita conocer | Aviso visible de que la consulta queda registrada · motivo de la consulta | La minimización es el diseño: una tabla completa de datos personales es exactamente lo que `RN-52` quiere impedir | Existe · dual |
| PT-128 | Formulario con bloqueo: no se activa el campo sin base legal ni necesidad operativa | Catálogo de campos sensibles, respaldo | Es el punto donde `RNF-17` se cumple o se pierde. ⛔ sin `CU-19` | Existe · `[C]` |
| PT-129 | Formulario mínimo con **tres salidas explícitas**: documento, identificación alternativa o no identificada | Visor de la foto del documento alternativo | Es el caso frecuente, no el borde: nunca dejar el campo en blanco ni inventarlo. Parcial · dual | Campo + existe |
| PT-130 | **Maqueta de la lista de abordo con folio y QR** | Cierre del manifiesto, aviso de que a partir de aquí no se edita | Se imprime y se entrega. `RN-53` | **Construir** hoja · dual |
| PT-131 | **Botonera de campo con las tres novedades** — no se presentó · subió · bajó antes | Confirmación de guardado sin red | La pantalla **no ofrece editar**: si lo ofreciera, el manifiesto deja de ser una declaración. Tres botones, sin señal, sin formulario | Campo |
| PT-132 | **Matriz rol × ámbito de visibilidad** | Nota de necesidad de conocer, alcance de datos | Quién ve qué manifiestos es una relación de dos ejes. ⛔ sin `CU-19` | **Construir** · `[C]` |
| PT-133 | **Gráfico de accesos por usuario y período con la desviación marcada** | Tabla de accesos · umbral de patrón anómalo `[C]` | Un registro que nadie mira no es un control: la anomalía es un patrón, y un patrón no se ve en filas | **Construir** · `[C]` |
| PT-134 | **Ficha agregada por persona**: todo lo guardado, agrupado por origen | Exportación · cronómetro implícito de `RNF-17` (≤ 5 min) | Es **un** sujeto con muchas fuentes, no muchas filas comparables. Actor `[C]` sin catalogar | Existe · `[C]` |
| PT-135 | **Comparador lado a lado**: asiento original ↔ rectificación | Motivo, quién rectificó, el original **intacto** | Rectificar sin destruir es literalmente mostrar los dos. Si la pantalla parece un formulario de edición, se destruye el asiento. Actor `[C]` | **Construir** · `[C]` |
| PT-136 | **Vista previa del entregable sin ningún dato personal** | Verificación explícita de que no queda ninguno · formato de publicación | Se publica: lo que hay que ver es exactamente lo que va a salir. Actor `[C]` | **Construir** hoja · `[C]` |
| PT-137 | **Ficha de confirmación con el inventario de lo que se va a destruir** | Aviso previo obligatorio · verificación de la cadena de auditoría antes y después · doble confirmación | Es la **única pantalla del sistema que destruye contenido**. Lo que se ve no es un formulario: es el alcance del daño y la prueba de que la cadena sigue verificando. ⛔ sin `CU-19` | Existe + **construir** verificación · `[C]` |
| PT-138 | *Ficha en §2.5* | | | |

### 2.16 M-01 / M-02 Administración y operación

> Las siete están marcadas ⛔ en el inventario o sin historia. El dictamen es del elemento, **no autoriza dibujarlas**: sin `CU-19` lo que se dibuje fija la regla por accidente.

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-096 | Tabla de usuarios y puestos | **Línea de vigencias de las asignaciones** de puesto | Los permisos son del puesto y el puesto tiene vigencia: hay que ver quién ocupaba qué el día del hecho. ⛔ | Existe + **construir** · `[C]` |
| PT-097 | **Matriz de incompatibilidades rol × rol**, con las celdas prohibidas marcadas | Tabla de asignaciones vigentes · pantalla de bloqueo al acumular | La segregación de funciones es una relación entre pares: en lista, el par incompatible no salta. ⛔ | **Construir** · `[C]` |
| PT-098 | Tabla de catálogos + formulario | Vigencia por elemento, uso actual antes de desactivar | Mantiene listas: es tabla. ⛔ | Existe · `[C]` |
| PT-099 | Formulario de carga con vigencia | **Línea de vigencias** para ver dónde cae la nueva respecto a las anteriores · visor del respaldo documental | Cargar una tarifa sin ver contra qué rango se empalma es como se producen dos tablas vigentes el mismo día. ⛔ **No se dibuja hasta `CU-19`** | Existe + **construir** · `[C]` |
| PT-100 | **Comparador: vigente ↔ propuesto** | Acto de doble control con los dos nombres · fecha desde la que aplica | Aprobar a ciegas un parámetro normativo es el fallo que `RNF-05` existe para evitar. ⛔ | **Construir** · `[C]` |
| PT-101 | Tarjetas de estado con **qué hacer**, no solo qué está mal | Minigráficos de tendencia por indicador | Está escrita para alguien sin especialización: una tabla de métricas no le dice qué hacer | Existe (`FilaKpis`) + **construir** minigráfico |
| PT-102 | **Línea de tiempo de respaldos con el RPO a la vista** — «si restaura ahora, pierde lo de las últimas N horas» | Asistente paso a paso de restauración · verificación del respaldo | En muchas instituciones no hay equipo de TI. Lo que hay que ver no es una lista de archivos: es **cuánto se perdería**. `[I]` — inferencia de diseño sobre `RNF-09` | **Construir** |

### 2.17 Cliente de campo

**LOKI no aplica a ninguna: son React Native.** El dictamen aquí es conservador por `RNF-12`: ningún mapa, ningún gráfico, ninguna animación, ninguna carga diferida, ninguna dependencia de red para pintar.

| PT | Elemento primario | Secundarios | Por qué |
|---|---|---|---|
| PT-103 | Formulario mínimo con teclado grande | Aviso de que funciona sin red | Entra al vehículo detenido: dos campos y nada más |
| PT-104 | **Ficha de la única misión + la acción principal a un toque en el tercio superior** | Tarjetas grandes para el resto de actos · contador neutro de pendientes | Una misión, sin menú, sin pestañas, sin perfil. La acción más frecuente donde llega el pulgar con guante |
| PT-105 | **Tres botones grandes**: Llegué · Salí · Estoy esperando | Teclado numérico de dígitos grandes para el odómetro · **confirmación de guardado a pantalla completa** · contador informativo | Decide la adopción. Un dato pedido, todo lo demás inferido. La confirmación ocupa la pantalla porque un aviso que se desvanece se pierde al sol — y el que duda registra dos veces y produce un conflicto. Parcial |
| PT-106 | Formulario mínimo de quién recibe | Firma en pantalla · foto de la entrega · maqueta de la constancia | Cierra la cadena de custodia en destino: nombre, puesto, institución, lugar y hora |
| PT-107 | **Ficha del punto esperado con confirmación en un toque** | Tarifa esperada del paquete normativo congelado · acceso a discrepancia | El punto y la tarifa ya viajan en el dispositivo: solo confirma. **Sin mapa** |
| PT-108 | Formulario de tres campos | Foto del recibo · «sigo mi ruta» | Reclama después; ahora solo deja constancia |
| PT-109 | Formulario de cuatro campos | Foto del comprobante · salida «no tengo comprobante» | Galones, monto, odómetro y foto. Parcial |
| PT-110 | Formulario declarativo mínimo | Aviso de que se resuelve en liquidación | Nada bloquea por falta de comprobante |
| PT-111 | **Pantalla de pregunta con la última lectura a la vista** | Confirmar o corregir | **No es un error**: puede que el tablero se haya reemplazado. Si se ve como error, el motorista deja de registrar |
| PT-112 | Lista simple de pendientes | Contador en gris neutro, sin icono de alerta | «Sin señal» no es un fallo (`R-9`) |
| PT-113 | Formulario mínimo de solicitud | Aviso de que la respuesta llega cuando haya señal | **Sin mapa**: escribe a dónde le piden ir |
| PT-114 | **Maqueta de la hoja de bitácora con folio** | Impresión o compartir · numeración cruzada con el talonario (insumo #46) | Respaldo en papel: el cuarto puente |
| PT-115 | **Ficha de estado con selección en un toque** | Última posición capturada, mostrada **como texto y hora**, no como mapa | La posición se captura y se envía; dibujar el mapa en campo cuesta GPS activo, tiles y memoria, y el motorista ya sabe dónde está |
| PT-116 | **Cuatro botones grandes**: avería · accidente · robo · otra | Foto · ubicación · guía de actuación sin red · nada más obligatorio | El usuario está estresado, posiblemente de noche, posiblemente con un lesionado. Menos campos posibles; el resto lo resuelve la oficina |
| PT-117 | Pantalla de mensaje con la instrucción en cuerpo grande | Quién decidió y a qué hora | Recibe una orden, no captura nada |
| PT-118 | **Maqueta del acta de relevo** | Corte de odómetro · dos firmas | Traslada custodia en carretera |
| PT-119 | Formulario mínimo de retorno | Odómetro final, novedades. Parcial | Cierra la bitácora desde el campo |
| PT-120 | Lista simple con contador neutro | Reanudable, nunca iniciada por el usuario | Informativo (`R-9`) |
| PT-121 | Formulario réplica de la hoja de salida | Folio del rango anticipado · escalamiento a sede como camino por defecto (⚠ #26) | Despacho sin red en el predio |
| PT-122 | Formulario réplica de la requisición | Foto del original | Captura lo que llegó en papel |
| PT-123 | **Cotejo a dos paneles: la foto del original a la vista y los campos al lado** | Fecha del hecho **y** fecha de captura, ambas obligatorias y visibles · quién digitó | Aquí se juega la adopción rural. Digitar mirando otra pantalla es cómo se producen los conflictos de sincronización que después nadie puede resolver |
| PT-124 | **Guía de captura fotográfica**: las tomas exigidas —franjas azul-blanco-azul, leyenda, siglas, correlativo— una por una | Maqueta del acta de constatación · fecha y ubicación | Es hallazgo frecuente de auditoría, y lo que se verifica es visual. Una lista de casillas produce fotos inservibles |
| PT-125 | **Visor del documento con el QR grande**, legible sin red | Salvoconducto y orden de misión, ambos completos | Se lo enseña a un agente: tiene que verse como el papel |
| PT-127 | Lista agrupada por pendiente: misiones, papeles por digitar, pendientes de envío | Contadores por grupo · escalamiento a sede visible con **de quién** es | Raíz de `ACT-10`: varias misiones y una cola de digitación. Lo opuesto a `PT-104`, y por eso no comparten identificador |

### 2.18 Superficie pública

| PT | Elemento primario | Secundarios | Por qué | LOKI |
|---|---|---|---|---|
| PT-126 | **Ficha de verificación de un vistazo**: folio, vigente o anulado, vehículo, ventana autorizada | Vía degradada al pie: huella impresa para contraste manual, código corto, teléfono | La lee un agente en carretera en cinco segundos, sin sesión. **Mínimo verificable, nunca el expediente.** `[C]` la exposición pública está sin confirmar; la vía degradada sí se diseña hoy y puede terminar siendo la única | Existe |

---

## 3. Reparto resultante

| Elemento primario | Pantallas | % |
|---|---|---|
| **Formulario** | 27 | 20 % |
| **Tabla** | 23 | 17 % |
| **Maqueta de papel** | 20 | 14 % |
| **Ficha de decisión** | 19 | 14 % |
| **Línea de tiempo** (7 por carriles · 4 de vigencias · 3 de misión, expediente y respaldos) | 14 | 10 % |
| **Comparador lado a lado** (incluye cuadres y cotejos) | 9 | 7 % |
| **Pantalla de mensaje** (bloqueo, aviso, pregunta) | 5 | 4 % |
| **Matriz o cuadrícula** | 4 | 3 % |
| **Gráfico** | 3 | 2 % |
| **Cadena de eslabones** | 3 | 2 % |
| **Botonera de campo** | 3 | 2 % |
| **Lista simple de campo** | 3 | 2 % |
| **Mapa integrado de ARGOS** | 1 primaria (+8 como secundario) | 1 % |
| **Calendario** | 1 primaria (+5 como secundario) | 1 % |
| **Tablero por estado** · **lista de verificación** · **guía fotográfica** | 1 + 1 + 1 | 2 % |
| **Total** | **138** | |

**La tabla es el elemento primario correcto en 23 de 138 pantallas: el 17 %, no el 100 %.** La objeción del PO está confirmada por el dictamen: el mockup usó una sola forma para catorce necesidades distintas.

---

## 4. Lo que hay que construir, ordenado por cuántas pantallas lo necesitan

Es la lista que decide en qué invertir. El conteo incluye uso primario y secundario.

| # | Qué | Pantallas | Notas |
|---|---|---|---|
| 1 | **Hoja de documento imprimible** — folio, QR en posición fija, espacio de firma y sello, hash al pie, marca de agua «no válida», útil en blanco y negro sobre matricial | ~28 | Es una sola pieza para 20 documentos. **El marco se construye hoy; el contenido de las 29 bloqueadas espera el insumo #2.** Empezar por el marco es trabajo que no se rehace |
| 2 | **Línea de tiempo por carriles** — un carril por recurso, barras sobre un eje de tiempo | ~24 | **La de mayor apalancamiento por unidad de trabajo:** una sola primitiva sirve tres usos distintos — ocupación de flota (`PT-026`, `029`, `030`, `032`, `033`, `038`, `072`, `087`), vigencias de documentos y licencias (`PT-019`, `078`, `085`, `092`, `096`, `099`, `027`, `082`) e hitos de una misión o expediente (`PT-059`, `073`, `102`, `042`, `062`). Y es exactamente lo que hoy falta: cero en el mockup |
| 3 | **Comparador lado a lado con evidencia pareada** | ~14 | `PT-054` es la más difícil del sistema: ambas versiones campo por campo, **las dos fotografías visibles al mismo tiempo**, tres marcas de tiempo distintas, motivo obligatorio y ninguna acción de editar. Sirve también a `PT-017`, `024`, `057`, `060`, `065`, `100`, `123`, `135` |
| 4 | **Visor de evidencia fotográfica** — zoom, sin red, pareable | ~14 | Cruza los dos clientes. En campo es la mitad del valor de cada registro; en oficina es lo que resuelve el conflicto y lo que prueba el hallazgo |
| 5 | **Superficie táctil de campo** — área para dedo con guante, contraste a pleno sol, teclado numérico de dígitos grandes, confirmación a pantalla completa | 34 superficies | Otro producto y otro stack: no se resuelve con LOKI. Cuenta aparte porque es la inversión que decide la adopción |
| 6 | **Mapa** | 9 | **Se integra el de ARGOS (`DP-001`), no se construye.** Lo que sí construye SIGTI es la capa de negocio encima: antigüedad del dato, hitos, puntos de peaje. Y **no baja al cliente de campo** |
| 7 | **Gráfico** | ~8 | Empezar por el de `PT-064`: dos bandas de tolerancia **asimétricas**, punto fuera de escala fijado al borde con su valor al lado, y el estado *no concluyente* como tercer estado. Después la barra de cuota (`PT-045`) y la frecuencia de patrón (`PT-091`, `PT-133`). **No hay módulo de gráficos en el repositorio hoy** |
| 8 | **Matriz o cuadrícula de dos ejes** | ~6 | Licencia × tipo de vehículo (`PT-084`), incompatibilidad rol × rol (`PT-097`), rol × ámbito (`PT-132`), compatibilidad par a par por tramo (`PT-138`) |
| 9 | **Cadena de eslabones con huecos visibles** | ~6 | Extiende `RastreadorEtapas`, que hoy es lineal y sin huecos. `PT-089` y `PT-068` la necesitan; `PT-018`, `PT-050`, `PT-069` y `PT-073` la aprovechan |
| 10 | **Calendario de disponibilidad** | ~6 | `PT-010` primario; `PT-020`, `022`, `044`, `082`, `085` secundario |
| 11 | Piezas menores, una pantalla cada una | 8 | Croquis de daños (`PT-040`), medidor de rango de folios (`PT-037`), tablero por estado del vale (`PT-050`), cuadro de derivación de ficha técnica (`PT-076`), barra de capacidad (`PT-008`), agrupador de duplicados (`PT-051`), panel de exclusión del lote (`PT-055`), guía de captura fotográfica (`PT-124`) |

---

## 5. Pantallas cuyo mockup actual está equivocado

Hay que rehacerlas. Ordenadas por daño.

| PT | Cómo está | Qué debe ser | Gravedad |
|---|---|---|---|
| **PT-038** Tablero de despacho del día | Declarada **«completa»** y maquetada como lista | **Línea de tiempo del día por carriles.** Es la raíz del despachador en la ráfaga de la mañana y no muestra el traslape, que es su única razón de existir | **Alta** — el inventario la llama «Tablero» |
| **PT-058** Tablero de seguimiento en ruta | Declarada **«completa»** y maquetada como lista | **Mapa integrado de ARGOS** + tabla con la antigüedad del dato. El módulo se llama «Seguimiento en ruta» y la pregunta es *dónde* | **Alta** — el inventario la llama «Tablero» |
| **PT-026** Asignación de vehículo | «Completa» sin ocupación de flota. **También construida así** en `oficina/src/modulos/M07_Programacion/Asignacion.tsx` | **Línea de tiempo por carriles** como elemento primario. El propio diseño lo detectó y tuvo que inventar un «cronograma de flota semanal» fuera del inventario (`mockups §4.b`) | **Alta** — el código ya heredó la omisión |
| **PT-050** Ciclo de vida del vale y arqueo | «Completa» como tabla | **Tablero por estado** con conteo y monto + cuadre del arqueo. Una tabla plana no muestra dónde se atasca el fondo | Alta |
| **PT-045** Aprobación del fondo | «Completa» sin la cuota visible | **Barra de cuota**: comprometido, ejecutado, disponible. La decisión es «¿cabe?» y hoy hay que hacer la resta | Alta |
| **PT-085** Vigencia de la habilitación | «Esbozada — la alerta se ve en el padrón», es decir, no existe | **Línea de vigencias propia.** Anticipar vencimientos sin eje de tiempo no es anticipar | Alta |
| **PT-025** Cola de programación | «Completa» como tabla. **También construida así** | Tabla **correcta**, pero le falta la barra de caducidad por fila y el panel de ocupación | Media — se corrige sin rehacer |
| **PT-082** Padrón de motoristas | «Completa» como tabla | Tabla **correcta**, le falta la franja de vigencia por fila | Media |
| **PT-059** Detalle de misión en ruta | «Esbozada» | **Línea de tiempo de la misión** + mapa. Los hitos con sus duraciones son la pantalla | Media |
| **PT-089** Rastro del expediente | «Completa» | **Cadena con huecos visibles.** Verificar que no sea una lista cronológica: una lista ordenada por fecha **esconde** el eslabón que falta | Media — verificar antes de rehacer |
| **PT-055** Resolución por lote | «Esbozada — la regla se declara, el flujo no» | Falta el **panel de exclusión**: los conflictos de odómetro, monto y autorización que el lote deja fuera, contados y nombrados | Media |
| **PT-090** Paquete de evidencia | «Esbozada — solo el punto de salida» | Ficha de armado con el inventario de piezas. `RNF-18` prohíbe que sea un botón que produce un archivo | Media |
| **PT-028** Rechazo por licencia | «Completa» y construida. El mensaje y los caminos de salida están bien resueltos | Añadir la **franja de vigencia** que muestra dónde cae el vencimiento respecto a la ventana de la misión | Baja — añadido, no rehecho |
| **PT-064** Conciliación | «Completa», con eje y bandas dibujados (`mockups §4.k`) | **Probablemente correcta.** Es el único `canvas` del tablero. Verificar que las dos bandas sean visiblemente **asimétricas** y que exista el tercer estado *no concluyente* | Baja — verificar |

**Sobre el «cronograma de flota semanal»** que el diseño dibujó fuera del inventario (`mockups §4.b`, `PT-139` reservado): este dictamen sostiene que **no es una pantalla nueva, es el elemento primario que le falta a seis pantallas existentes** — `PT-026`, `029`, `030`, `032`, `033` y `038`. Si además el PO quiere una consulta autónoma de ocupación de flota, entonces sí procede `PT-139`, y su elemento primario sería la misma primitiva. La decisión es del PO; el componente hay que construirlo en cualquiera de los dos casos.

**De las seis pantallas ya construidas** (`PT-013`, `PT-014`, `PT-025`, `PT-026`/`027`/`028`, `PT-063`, `PT-069`): cuatro tienen el elemento primario correcto —las dos bandejas, el expediente en decisión y el cierre—. Las que hay que corregir son `PT-025` (añadido) y `PT-026` (rehacer el elemento primario).

---

## 6. Honestidad: qué dictaminé con poca información

**a. Las nueve pantallas marcadas ⛔ sin caso de uso ni criterio de aceptación** — `PT-092`, `096`, `097`, `098`, `099`, `100`, `128`, `132`, `137`. Dicté su elemento a partir del invariante del módulo y de los `RNF` que las citan, **no de criterios de aceptación, porque no existen**. Es exactamente el riesgo que el inventario declara: dibujarlas antes de `CU-19` fija la regla por accidente. **El dictamen del elemento no autoriza dibujarlas.** `[C]`

**b. Las 29 pantallas bloqueadas por el insumo #2.** Dicté «formulario réplica» o «maqueta de papel» porque eso se deduce del principio de paridad, pero **no puedo dictaminar sus elementos secundarios sin ver el formato**. Tres casos donde el formato podría cambiar el dictamen: `PT-039` (si la hoja de salida trae inspección por partes, el elemento es una lista de verificación, no un formulario), `PT-042` y `PT-121`. Y `PT-105` depende además del insumo #46: si el talonario pide más campos por hito, el bloque de captura cambia aunque los tres botones no. `[C]`

**c. No verifiqué el HTML del tablero pantalla por pantalla.** Son 1,4 MB. Me apoyé en la tabla de cobertura de `mockups/README.md`, en sus decisiones §4 y en el conteo ya verificado de un `canvas` y cuatro `svg`. Donde ese conteo no basta para concluir —`PT-089` y `PT-064`— marqué **«verificar»** en lugar de «rehacer».

**d. `PT-102` respaldo y restauración.** El dictamen de mostrar el RPO en un eje de tiempo es **inferencia de diseño `[I]`** sobre `RNF-09`. Ninguna historia lo pide; me apoyo en que la pantalla está escrita para alguien sin especialización y en que la única pregunta que importa al restaurar es cuánto se pierde.

**e. `PT-134`, `PT-135` y `PT-136`.** El Oficial de Información Pública **no es un actor catalogado**. Dicté sobre el acto, no sobre el rol. Si el actor se cataloga con navegación propia, `PT-134` podría necesitar una raíz de búsqueda por persona que hoy no dictaminé. `[C]`

**f. `PT-012`.** Sin resolver si pertenece al Nivel 3 que `DP-002` conserva o si cae con el régimen suspendido. Dicté el elemento; si cae, el dictamen sobra. `[C]`

**g. `PT-084` matriz licencia × tipo de vehículo.** Las nueve categorías son `[V]` (Artículo 4 del Acuerdo 1012-2021). **El otro eje —el catálogo de tipos de vehículo de la institución— es `[C]`**, y de su tamaño depende si la matriz cabe en pantalla o necesita agrupación. No lo sé.

**h. Umbrales sin número.** `PT-056` (antigüedad del espejo de ARGOS), `PT-064` (bandas de tolerancia), `PT-133` (patrón anómalo). El elemento visual es correcto sin el número, pero **no se puede dibujar la escala** sin él, y un bloqueo sin cifra es lo que `R-3` prohíbe. `[C]`

**i. El cliente de campo lo dicté sin datos del parque de dispositivos.** No sé el modelo, ni el tamaño de pantalla, ni si el componente de mapas de ARGOS funciona sin red. Por eso fui **deliberadamente conservador**: ningún mapa y ningún gráfico en campo. Si resulta que el parque es mejor de lo que supongo, este dictamen se puede relajar; al revés no. `[C]` — registrable junto al insumo #70.

**j. Los mockups y el sistema de diseño chocan en el tema oscuro** (`mockups §4.m`). No lo dictamino: no es elemento visual primario, es sistema visual, y no lo puedo resolver sin probarlo con un motorista al mediodía.
