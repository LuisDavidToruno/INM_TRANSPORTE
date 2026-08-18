# Mockups de SIGTI — entrega de diseño, Sprint 0

Rama: `design/mockups-sprint-0` · carpeta: `docs/04-diseno/mockups/`

| Archivo | Qué es |
|---|---|
| `tablero-de-mockups.html` | Todas las pantallas en un solo archivo autocontenido. No necesita servidor, red ni instalación: se abre con doble clic en cualquier navegador |
| `README.md` | Este documento |

Fuentes que se usaron para construirlo: `docs/00-vision/glosario.md` (vocabulario), `docs/01-negocio/actores-y-roles.md` (roles, alcances y bloqueos), `docs/04-diseno/inventario-de-pantallas.md`, `docs/04-diseno/mapa-de-navegacion.md` y `docs/04-diseno/brief-para-diseno.md`. **Ningún documento fuera de esta carpeta fue modificado.**

---

## 1. Cómo se abre y se navega

Abrir `tablero-de-mockups.html`. Es un tablero: se hace zoom y paneo con los gestos del navegador (Ctrl/Cmd + rueda para zoom).

**El mapa índice está arriba, en la primera sección**, con enlaces a cada pantalla agrupados por rol. Cada sección tiene un enlace `↑ Mapa` para volver.

La navegación funciona: los botones llevan de una pantalla a otra. El recorrido completo se puede seguir presionando botones:

```
Requisición de vehículo → Enviar a autorización → Bandeja de la jefatura →
Autorizar → Tablero de misiones → Programar (vehículo + motorista) →
Vista previa con folio → Emitir → Despacho (revalidación, kilometraje) →
Transferir paquete al celular del motorista → Mi misión → Registro en ruta →
Cola de conflictos → Conciliación → Propuesta de cierre a Gerencia
```

También están enlazados los desvíos: el bloqueo por licencia no habilitante desde la asignación, la aprobación del fondo desde la misión con fondo pendiente, la firma del salvoconducto y su verificación pública por QR, y el rastro del expediente del auditor desde la conciliación.

Los mockups son **agnósticos de tecnología** (ADR-000): la navegación es por anclas, no hay estado ni framework. Nada de lo que se ve prejuzga el stack de interfaz.

---

## 2. Tabla de cobertura

Estado: **completa** = la pantalla se puede especificar a partir de esto · **esbozada** = la estructura y los mensajes están, faltan campos o estados secundarios · **no hecha** = no está en esta entrega.

| PT | Pantalla | Cliente | Historias | Estado |
|---|---|---|---|---|
| PT-006 | Mis solicitudes | administrativo | HU-001 | completa |
| PT-007 | Requisición de vehículo | administrativo | HU-001, HU-003 | esbozada — replica papel, ver §3 |
| PT-008 | Objeto del traslado: personas, carga o mixto | administrativo | HU-001, HU-002 | esbozada — como control dentro de PT-007 |
| PT-009 | Estimado de peajes desglosado por punto | administrativo | HU-005 | esbozada — aparece dentro de PT-007 y PT-014 |
| PT-013 | Bandeja de autorización | administrativo | HU-009, HU-012 | completa |
| PT-014 | Expediente en decisión | administrativo | HU-009, HU-011 | completa |
| PT-015 / PT-016 / PT-017 | Autorizar · rechazar · devolver | administrativo | HU-011, HU-013, HU-014, HU-015 | esbozada — los tres actos se distinguen visualmente en PT-013/PT-014; falta el formulario de motivo de cada uno |
| PT-021 | Firma del permiso de día u hora inhábil | administrativo (celular) | HU-016 | completa |
| PT-025 | Tablero de misiones / cola de programación | administrativo | HU-021 | completa |
| PT-026 | Asignación de vehículo: compatibilidad, documentación y estado | administrativo | HU-022, HU-023, HU-024 | completa |
| PT-027 | Declaración del motorista titular y relevos | administrativo | HU-025, HU-026 | esbozada — el titular sí, los relevos no |
| PT-028 | Rechazo por licencia no habilitante | administrativo | HU-025, HU-108 | completa |
| PT-031 | Constancia probatoria de las verificaciones practicadas | administrativo | HU-028 | esbozada — se muestra el bloque, no el documento |
| PT-034 | Vista previa con folio reservado, marcada «no válida» | administrativo | HU-029 | completa |
| PT-035 | Emisión del juego documental | administrativo | HU-031 a HU-034, HU-081 | esbozada — replica papel, ver §3 |
| PT-038 | Tablero de despacho del día | administrativo | HU-038 | completa |
| PT-039 | Acto de despacho: revalidación, kilometraje, inspección | administrativo | HU-038, HU-039 | esbozada — replica papel, ver §3 |
| PT-045 | Aprobación del fondo contra cuota y partida | administrativo | HU-072, HU-073 | completa |
| PT-050 | Ciclo de vida del vale y arqueo del fondo | administrativo | HU-074, HU-079, HU-080 | completa |
| PT-053 | Cola de conflictos | administrativo | HU-068 | completa |
| PT-054 | Comparador de dos versiones lado a lado | administrativo | HU-068 | completa |
| PT-055 | Resolución por lote con criterio declarado | administrativo | HU-068 | esbozada — la regla del lote se declara, el flujo no |
| PT-058 | Tablero de seguimiento en ruta, con antigüedad del dato | administrativo | HU-057 | completa |
| PT-059 | Detalle de la misión en ruta con sus hitos | administrativo | HU-047, HU-055 | esbozada |
| PT-061 | Recepción de la interrupción y resolución de su desenlace | administrativo | HU-058, HU-059, HU-060 | esbozada — entrada y acción, sin las tres salidas |
| PT-064 | Conciliación galonaje contra kilometraje | administrativo | HU-088, HU-084 | completa |
| PT-068 | Cadena de trazabilidad y propuesta de cierre | administrativo | HU-092 | esbozada — la propuesta se envía desde PT-064 |
| PT-082 | Padrón de motoristas con su habilitación vigente | administrativo | HU-105, HU-107 | completa |
| PT-085 | Vigencia de la habilitación y alertas anticipadas | administrativo | HU-107 | esbozada — la alerta se ve en el padrón |
| PT-089 | Rastro del expediente de extremo a extremo, con sus huecos | administrativo | HU-092 | completa |
| PT-090 | Exportación del paquete de evidencia | administrativo | — | esbozada — solo el punto de salida |
| PT-104 | Mi misión — raíz única del cliente de campo | de campo | HU-046 | completa |
| PT-105 | Registro en ruta: llegué, salí, estoy esperando | de campo | HU-047 | completa |
| PT-109 | Abastecimiento de combustible con comprobante | de campo | HU-051, HU-082, HU-083 | esbozada — replica papel en parte, ver §3 |
| PT-110 | Consumo sin comprobante | de campo | HU-052, HU-087 | esbozada — solo el acceso «no tengo comprobante» |
| PT-112 | Pendientes de envío y adjuntos en espera | de campo | HU-054 | esbozada — el contador, no la lista |
| PT-116 | Registro de interrupción en ruta | de campo | HU-058 | completa |
| PT-120 | Estado de sincronización del dispositivo | de campo | HU-066, HU-067 | esbozada — el indicador, no la pantalla |
| PT-123 | Digitación diferida desde el papel, con foto del original | de campo | HU-064, HU-007 | esbozada — replica papel, ver §3 |
| PT-126 | Verificación del documento por QR | pública | HU-019, HU-035 | completa |
| — | **Cronograma de flota semanal** (pantalla nueva, no está en el inventario) | administrativo | — | completa · ver §4.b |

Cobertura: **41 identificadores PT** tocados de los 126 del inventario — 21 completas, 20 esbozadas.

Todo lo que no aparece en esta tabla es **no hecha**. La lista está en §3.

---

## 3. Qué NO se hizo, y por qué

### 3.1 Las pantallas que replican un formato en papel (insumo #2)

El principio de paridad pantalla ↔ papel manda: los mismos campos, con los mismos nombres, en el mismo orden. **Sin el formato vigente en la mano, dibujar esas pantallas es garantizar que hay que rehacerlas.** Por eso ninguna se dibujó como definitiva.

Aun así se tocaron seis, porque sin ellas el flujo no se puede recorrer y no se entiende el resto. **Todas quedan marcadas como esbozadas y ninguna fija el orden de los campos.** Lo que se supuso en cada una:

| PT | Qué se dibujó igual | Qué se supuso — hay que confirmarlo contra el formato |
|---|---|---|
| PT-007 Requisición de vehículo | Estructura, agrupación, controles nuevos (objeto del traslado como segmentado, peajes estimados, aviso de tramo inhábil) | Que existen los campos: quién captura vs. solicitante de derecho, objeto, qué se traslada, peso, origen, destino, salida, retorno previsto y justificación. El **orden** es provisional |
| PT-034 / PT-035 Vista previa y juego documental | Bloques del documento, folio, QR, espacio de firma y sello, hash al pie | Que el encabezado lleva institución y unidad, y que los datos de vehículo, motorista, ruta, ventana y objeto van en el cuerpo. La maqueta del formato oficial **no** está resuelta |
| PT-039 Acto de despacho | Revalidación al momento de la salida, kilometraje de salida, novedades de inspección | Que la hoja de salida del predio pide kilometraje y novedades, y nada más en el momento del despacho |
| PT-109 Abastecimiento de combustible | Galones, monto, odómetro, foto del comprobante, salida «sin comprobante» | Que el control de combustible del motorista pide esos cuatro datos |
| PT-123 Digitación diferida | Fecha del hecho vs. fecha de captura, quién digitó, adjunto del original | Que se digita la **bitácora**; los campos mostrados son un subconjunto ilustrativo, no la bitácora completa |
| PT-105 (parcial) Registro en ruta | Toda la pantalla, incluido el odómetro | Que el único dato obligatorio del hito es el odómetro. Si el talonario de bitácora pide más campos por hito (insumo #46), esto cambia |

Las 21 restantes bloqueadas por el insumo #2 **no se tocaron**: PT-020, PT-023 (salvoconducto), PT-024, PT-036, PT-037, PT-040 (acta de entrega-recepción), PT-041, PT-042, PT-044, PT-047, PT-048, PT-049, PT-074, PT-077, PT-080, PT-081, PT-094 (manifiesto), PT-106, PT-114, PT-118, PT-121, PT-122, PT-124.

> **Nota sobre el salvoconducto (PT-023).** Es el documento más exigente del sistema y sigue sin dibujarse porque depende del formato. Sí se diseñó su **firma** (PT-021) y su **verificación** (PT-126), que no replican papel.

### 3.2 Módulos que quedaron fuera

| Fuera | Por qué |
|---|---|
| **M-11 Mantenimiento y taller** — pantallas de `ACT-11` | El inventario declara que el Bloque 3 no escribió historias para ellas. Solo se refleja el efecto sobre la programación: un vehículo «en taller» aparece como no disponible en el cronograma |
| **M-01 / M-02 Administración y catálogos** — PT-096 a PT-102, `ACT-01` | Usuarios, puestos, asignación puesto↔rol, catálogos, parámetros con vigencia, panel de salud, respaldo y restauración. Dependen del contrato de API de ARGOS (insumo #16) y del esquema de autorizaciones, ambos abiertos |
| **M-14 Auditoría, resto** — PT-088, PT-091, PT-092, PT-093 | Se hizo PT-089 (rastro) porque es el que valida el modelo de trazabilidad. Los otros son consultas con filtros sobre el mismo material |
| **M-17 Personas externas** — PT-094, PT-095 | El manifiesto replica papel y su consulta se rige por necesidad de conocer con registro. Merece diseñarse junto con el formato, no antes |
| **Vehículo como bien** — PT-072 a PT-081, PT-124 (`ACT-13`, `ACT-14`) | Expediente del vehículo, tarjeta de responsabilidad, descargo y constatación física. Mitad replica papel; la otra mitad depende de si existe unidad de Bienes separada (insumo F) |
| **Motoristas, resto** — PT-083, PT-084, PT-086, PT-087 | Captura de la licencia, tipos habilitados, conductor fuera del padrón e inhabilitación |
| **Liquidación, resto** — PT-063, PT-065, PT-066, PT-067, PT-069, PT-070, PT-071 | Se hizo PT-064, que es la difícil. Faltan las otras dos conciliaciones (fondo y peajes), el bloqueo por segregación al liquidar, y el cierre y el hallazgo posterior de `ACT-08` |
| **PT-012 Salida de emergencia** | No se dibujó a propósito: ver la contradicción en §5.3 |
| Resto del cliente de campo | PT-103, PT-106 a PT-108, PT-111, PT-113 a PT-115, PT-117 a PT-119, PT-125 |

---

## 4. Decisiones de diseño que no estaban en el brief

Estas son decisiones propias. Cada una llena un hueco; ninguna debe darse por aprobada.

**a. La marca de agua «NO VÁLIDA · VISTA PREVIA».** El inventario pide que la vista previa esté «marcada no válida», sin decir cómo. Se resolvió con una leyenda diagonal a baja opacidad sobre todo el cuerpo del documento, más el folio con la aclaración «reservado · no válido hasta la emisión» en la esquina. La diagonal es deliberada: sobrevive a la fotocopia y al blanco y negro, y no se puede recortar sin que se note. La opacidad se eligió para no estorbar la lectura de los campos.

**b. El cronograma de flota semanal es una pantalla nueva.** No está en el inventario. Al dibujar la asignación quedó claro que decidir *qué vehículo* exige ver la ocupación de la flota en el tiempo, y ni PT-025 (cola de programación) ni PT-029 (reserva exclusiva) la dan. **Propuesta: darle un PT propio.** Sin ella, la única forma de saber si un vehículo está libre el jueves es abrir las misiones una por una.

**c. Un cuarto estado visual: «propuesta sin emitir».** El cronograma lo dibuja con borde discontinuo. Existe implícitamente en PT-034 (folio reservado, «guardar sin emitir»), pero no está nombrado como estado en ningún artefacto. Si no se nombra, dos despachadores pueden reservar el mismo vehículo.

**d. El bloqueo y la advertencia no se distinguen por color.** Todo formato debe ser útil en blanco y negro, y el cliente de campo se usa a pleno sol: nada puede depender del color para significar. Se distinguen por **forma del icono** (octágono = bloqueo, triángulo = advertencia), por **texto explícito**, y sobre todo estructuralmente: en el bloqueo **la acción no existe**; en la advertencia la acción está y cobra el motivo escrito.

**e. El bloqueo por segregación no muestra botones deshabilitados.** En la bandeja de la jefatura, la fila de su propia solicitud no tiene «Autorizar» en gris: no tiene botón. En su lugar va el mensaje con el puesto competente. El mapa lo pide explícitamente para `ACT-07` e `I-10`; se generalizó a todos los bloqueos duros. Un botón deshabilitado invita a insistir, y cada insistencia es un asiento en la pista de auditoría.

**f. Teclado numérico propio, de dígitos grandes, en el registro en ruta.** El requisito es «área táctil para dedo con guante»; el teclado nativo del celular no lo cumple. Es una decisión con costo (hay que construirlo) y no estaba pedida.

**g. La confirmación de guardado es una pantalla completa, no un aviso pasajero.** El brief pide «confirmación inmediata y visible». Un aviso que se desvanece se pierde al sol, y el motorista que duda registra dos veces — y eso produce un conflicto de sincronización. Por eso ocupa la pantalla y hay que salir de ella tocando.

**h. El contador de pendientes se escribe en gris neutro, sin icono de alerta.** «Sin señal» no es un fallo. Ni el color de acento ni un triángulo aparecen ahí.

**i. Los peajes se muestran siempre punto por punto y con el identificador del tarifario**, incluso donde ocupa espacio caro (la pantalla del celular de la jefatura). Se decidió no ofrecer nunca el total solo, ni siquiera colapsado.

**j. La vía degradada de verificación va en la misma pantalla pública**, no en otra: el contraste del hash impreso y el teléfono de la institución están al pie de PT-126. Si esa vía termina siendo la única, la pantalla ya la contempla.

**k. En la conciliación, el punto fuera de escala se fija al borde del eje** y se dice «fuera de escala» con el número exacto al lado. La alternativa era comprimir el eje, y comprimirlo hace ilegibles las dos bandas de tolerancia asimétricas, que son lo que la pantalla tiene que enseñar.

**l. Nomenclatura de folios inventada para los mockups**: `SOL-2026-00512`, `OM-2026-00517`, `VC-2026-0210`, `SC-2026-0088`. La documentación exige folio único y correlativo, pero no define el formato. **Es una suposición**, no una propuesta cerrada.

**m. El tema oscuro viene del sistema de diseño de la marca y es una decisión con riesgo en el cliente de campo.** El requisito de campo es contraste alto y legibilidad a pleno sol; el sistema de diseño impone fondo oscuro. Se llevó el contraste al máximo dentro de esa paleta y se agrandó todo, pero **conviene validarlo con un motorista, en la calle, al mediodía** antes de darlo por bueno. Si falla, la salida es un tema claro solo para el cliente de campo.

---

## 5. Contradicciones y huecos encontrados en la documentación

No se corrigió nada. Se anota y se deja a quien tiene la autoridad según la precedencia de `CLAUDE.md`.

**5.1 El inventario y el mapa usan «conductor», que el glosario prohíbe.** PT-027 se llama «Declaración de conductores: titular y relevos» y PT-086 «Declaración de conductor fuera del padrón»; el mapa de navegación repite «Declarar conductores» en §4. El glosario es fuente de verdad y su tabla de términos prohibidos manda **motorista**. En los mockups se usó «motorista» siempre. Hay que decidir si se renombran esas dos pantallas o si «conductor» tiene ahí un sentido distinto que el glosario debería recoger.

**5.2 La matriz de permisos y la máquina de estados no dicen lo mismo sobre anular.** `actores-y-roles.md` §4.2 ya trae la corrección escrita (desde los estados terminales no sale ninguna transición), pero la **fila 15 de la matriz** sigue leyéndose como que `ACT-09` anula «en cualquier estado», con la salvedad viviendo en la nota y en el recuadro posterior. La fila y la nota deberían decir lo mismo sin que haya que leer las dos.

**5.3 PT-012 queda huérfana entre el régimen de excepción suspendido y el Nivel 3.** Las acciones 27 y 28 y el Nivel 2 están declarados **no implementados**, pero PT-012 «Registro de salida de emergencia para convalidación posterior» sigue en el inventario, y el Nivel 3 (convalidación de emergencia) sí se conserva. No queda claro si PT-012 pertenece al Nivel 3 —y entonces se diseña— o si cae con el régimen suspendido. **Por eso no se dibujó.**

**5.4 El recuento de pantallas bloqueadas no cierra a la primera.** §5 del inventario dice 27 bloqueadas + 8 parciales + 91 libres = 126. Contando las filas marcadas «Sí» en las tablas de §2 y §3 salen 28, y una de ellas (PT-037, emisión anticipada) está marcada como cliente `A/C`, así que puede estar contada en otra columna. Vale recontar antes de usar ese número para planificar.

**5.5 PT-041 y PT-048 se solapan.** «Entrega del fondo contra firma, dentro del despacho» (`ACT-07`/`ACT-05`) y «Entrega del fondo y registro de su custodia» (`ACT-07`) parecen el mismo acto visto desde dos lugares. Si son la misma pantalla en dos contextos, conviene decirlo; si son dos, conviene decir qué las diferencia, porque ambas replican el mismo formato en papel.

**5.6 `I-19` no dice qué pasa cuando `ACT-08` aprueba un fondo que su propio puesto solicitó por delegación.** La acción 9 de la matriz da la aprobación a `ACT-08` y escalada a `ACT-09`, e `I-19` declara el par incompatible como bloqueo duro `[C]`, pero el fondo es objeto **de período** y no de misión, así que la evaluación «mismo fondo, mismo período» necesita una regla que hoy no existe. En PT-045 se dibujó el control como informado y aprobable; **si la regla dice otra cosa, esa pantalla cambia.**

**5.7 Qué ve la jefatura sobre la flota.** El mapa dice que la jefatura no decide vehículo ni motorista y que la disponibilidad, si se muestra, es orientativa y no reserva nada. No queda claro si esos datos deben aparecer o no en PT-014. Se optó por **no mostrar flota** en el expediente en decisión, solo advertirlo por escrito.

**5.8 El umbral de antigüedad del espejo de ARGOS no existe todavía.** El bloqueo por «espejo detenido sobre el umbral» es un criterio con número, y el número es insumo abierto. En el mockup se muestra la antigüedad sin umbral, lo que significa que **la pantalla del bloqueo correspondiente aún no se puede redactar con precisión** — y un bloqueo sin número es exactamente lo que el propio brief prohíbe.

**5.9 Tensión real entre la paridad con el papel y el §6 del inventario, en el salvoconducto.** §6 exige que los cuatro datos que necesita el agente de tránsito vayan en el tercio superior y en cuerpo grande. La paridad con el papel exige reproducir el formato vigente campo por campo y en el mismo orden. **Si el formato actual no pone esos cuatro datos arriba, las dos reglas se contradicen** y hay que decir cuál gana antes de diseñar PT-023.

**5.10 La documentación no nombra la institución.** Habla de «institución pública hondureña» y de roles genéricos. En los mockups se usó «Instituto Nacional de Migración» por el nombre del repositorio. **Confirmar** — si es un sistema para varias instituciones, el nombre y el logotipo son configuración, no diseño.
