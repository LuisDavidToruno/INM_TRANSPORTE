# NRM-06 — Tránsito, licencias, matrícula y siniestros

| Campo | Valor |
|---|---|
| **Ámbito** | Habilitación del motorista, matrícula del vehículo, seguro, revisión, responsabilidad ante accidente |
| **Módulos afectados** | M-03, M-04, M-05, M-07, M-12 |
| **Última verificación** | **2026-08-24** (previa: 2026-08-06) |
| **Riesgo de cambio** | **Alto** — hay reforma reciente y varias iniciativas en trámite |

> ## ✅ CONFIRMADA COMO NÚCLEO DEL SISTEMA
>
> Revisión del PO del 2026-08-06 — ver [DP-001, decisiones D-11, D-12, D-13 y D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).
>
> - **La matriz licencia ↔ vehículo se implementa con bloqueo duro.** Palabras del PO: *"nos tenemos que proteger con la ley también"*. Sin excepción configurable.
> - **Seguro y revisión mecánica se gestionan igual**, aunque no sean obligatorios por ley vigente: póliza, vigencia, aseguradora, alertas. El bloqueo queda como regla configurable **apagada por defecto**.
> - **El permiso especial del IHTT para traslado de carga sale del alcance.** El PO confirmó que no se requiere.
>
> Esto se enmarca en la definición del producto: *así como Talento Humano cuida de todo lo referente a los empleados, SIGTI cuida de todo lo referente a los vehículos*.

Esta ficha contiene la **validación de mayor valor legal del sistema**: la matriz licencia ↔ vehículo.

## Marco normativo

| Norma | Referencia | Vigencia | Verificación |
|---|---|---|---|
| Ley de Tránsito | Decreto Legislativo No. **205-2005**, del **16/08/2005**, publicado en La Gaceta el **03/01/2006** | Vigente | `[V]` numeración y fechas |
| **Reglamento Especial en Materia de Permisos de Conducir** | **Acuerdo No. 1012-2021**, del **04/06/2021** | Vigente | `[V]` numeración y fecha; `[P]` articulado |
| Reforma anterior al Art. 48 | Decreto No. **118-2021**, del **19/12/2021**, publicado en La Gaceta el **06/01/2022** | Vigente | `[P]` |
| **Reforma por adición al Art. 48** | Decreto No. **51-2025** — aprobado **07/08/2025**, ejecutoriado **13/08/2025**, publicado en **La Gaceta No. 36,940 del 11/09/2025** | Vigente desde su publicación | `[V]` numeración, fechas y vigencia; `[P]` articulado |
| DNVT — Dirección Nacional de Vialidad y Transporte | administra licencias | — | `[V]` |
| Instituto de la Propiedad (IP) — Registro Vehicular | matrícula y placas | — | `[V]` |
| IHTT — Instituto Hondureño del Transporte Terrestre | permisos de explotación, especiales y **Certificado de Conductor Profesional** | — | `[V]` |

> ### 🔴 Corrección de fondo del 2026-08-24 — el Art. 48 **no** contiene la matriz
>
> La versión anterior de esta ficha y el riesgo #7 asumían que la matriz licencia↔vehículo dependía del texto del **Artículo 48**. **Es falso.** `[P]`
>
> - El **Art. 48 regula los requisitos para obtener el permiso** (edad, exámenes, experiencia, antecedentes). No dice qué vehículo habilita cada categoría.
> - **La matriz licencia↔vehículo está en el Acuerdo No. 1012-2021**, *Reglamento Especial en Materia de Permisos de Conducir*, del 04/06/2021.
>
> Corroborado desde dos lados: el propio comunicado de la SAPP sobre reclasificación de peaje se funda en *"la Ley de Tránsito **y el Reglamento Especial en Materia de Permisos de Conducir**"* `[V]` — no en un artículo suelto de la ley.
>
> **Consecuencia:** obtener el texto del Decreto 51-2025 **no desbloquea la matriz**. Son dos pendientes distintos, y el bloqueante real es el Acuerdo 1012-2021.

## Categorías de licencia — Acuerdo 1012-2021 `[V]` esquema / `[P]` umbrales literales

Fuentes concordantes: editorial jurídica que transcribe el reglamento, portal oficial de trámites y prensa especializada. **No se pudo abrir el PDF oficial del acuerdo** (ver *Limitación de herramienta*), por lo que los umbrales literales quedan `[P]`.

| Categoría | Habilita a conducir | Nivel |
|---|---|---|
| **A** | Ciclomotores y motocicletas de motor o eléctricas | `[V]` |
| **B1** | Triciclos y cuadriciclos motorizados (mototaxi, cuatrimoto) | `[V]` |
| **B** | Vehículos livianos, **masa máxima autorizada ≤ 3,500 kg**, diseñados para **no más de 8 personas además del conductor**, no comprendidos en A ni B1 | `[V]` |
| **BE** | Vehículos de la **categoría B enganchados a un remolque** | `[V]` |
| **C1** | Vehículos de carga **no articulados** cuya masa máxima **no exceda de 7,500 kg** | `[V]` |
| **C** | Vehículos de carga **no articulados superiores a 7,500 kg** | `[V]` |
| **CE** | Vehículos de la **categoría C enganchados a remolque o semirremolque** (cisternas, plataformas, furgones) | `[V]` |
| **D1** | Autobuses de **hasta 25 pasajeros** | `[V]` |
| **D** | Autobuses de **26 pasajeros o más** | `[V]` — precisión nueva: la ficha anterior decía solo "autobuses" |

**Fuente en el repositorio:** [`fuentes/acuerdo-1012-2021-permisos-de-conducir.pdf`](fuentes/acuerdo-1012-2021-permisos-de-conducir.pdf) — La Gaceta No. 35,661 del 19 de julio de 2021, **Artículo 4**. Descargado del sitio del TSC el 2026-08-26; tiene capa de texto.

> ### ✅ Resuelto el 2026-08-26: la categoría **BE** existe, y **no hay `DE`**
>
> El hallazgo decía que `CLAUDE.md` enumeraba ocho categorías y omitía `BE`, con la categoría en `[P]`. **Confirmado contra la fuente oficial**: el Artículo 4 crea `BE` bajo el epígrafe *«PERMISOS PARA CONDUCIR VEHÍCULOS CON REMOLQUES O SEMIREMOLQUES»*. Son **nueve** categorías, y **ninguna `DE`** — el epígrafe solo contempla `BE` y `CE`.
>
> Lo que el hallazgo no había visto, y es peor: **el eje normativo es *«enganchado a un remolque»*, no *«articulado»***. Un pick-up con plataforma enganchada requiere `BE` y no es articulado en ningún sentido. `BD-02` describía el atributo como *«si es articulado»* y con eso **ese caso pasaba el bloqueo duro**. Corregido en [`estados/orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md).
>
> Corregidos también `CLAUDE.md` y el enumerado `CategoriaDeLicencia` del dominio.

`[P]` Fuente concordante adicional: *los permisos tipo C, D y CE solo se expiden a quien ya es titular de C1 o D1 con al menos dos años de experiencia en esas categorías.* Esto convierte la matriz en un **grafo de prerrequisitos**, no en una lista plana — relevante si el sistema alguna vez valida progresión de categorías.

## Reforma por adición al Art. 48 — Decreto 51-2025 `[P]`

Requisitos que la reforma añade para las categorías **C, D y CE**. Fuentes concordantes: índice oficial de La Gaceta No. 36,940 y base de datos jurídica; corroborado por dos medios. **No se pudo leer el texto oficial del decreto.**

| Requisito | Contenido | Nivel |
|---|---|---|
| Edad mínima | **21 años** para C1 y D1; **23 años** para CE | `[P]` |
| Experiencia previa | Escalonada por categoría; se reporta un mínimo en la categoría anterior antes de ascender | `[P]` |
| Salud | Certificados médicos y psicológicos en centros certificados; exámenes **toxicológicos y de glucosa**; **electrocardiograma para mayores de 40 años** | `[P]` |
| Habilitación profesional | **Certificado de Conductor Profesional** emitido por el **IHTT** (seguridad vial, mecánica, primeros auxilios) | `[P]` |
| Antecedentes | Sin antecedentes penales; en pleno ejercicio de los derechos ciudadanos | `[P]` |
| Identidad | DNI, carnet de residencia para extranjeros, o carnet diplomático con pasaporte | `[P]` |

> **Contradicción de fuentes, no resuelta.** Parte de la prensa habla de categorías **«CD y CE»** y otra parte de **«C, D y CE»**. Las bases jurídicas dicen **«C, D y CE»**, y ninguna fuente del reglamento de permisos contempla una categoría llamada «CD». **Lectura:** «CD» es casi con certeza una errata periodística por «C, D». Más fiables las bases jurídicas, que trabajan sobre el texto de La Gaceta. **Se marca como no resuelto** hasta leer el decreto; y `CLAUDE.md` y el riesgo #7, que hoy dicen "categorías CD y CE", arrastran esa errata.

**Qué cambia esto para SIGTI:** la reforma toca el **expediente del motorista** (edad, exámenes periódicos, certificado del IHTT, antecedentes), no la compatibilidad vehículo↔licencia. Son campos y alertas de M-05, no reglas de bloqueo de M-07.

## Seguro obligatorio y revisión vehicular — hallazgo importante

- **El seguro de responsabilidad civil vehicular NO es obligatorio en Honduras.** `[V]` La iniciativa de la DNVT (marzo 2026) es un **anteproyecto en fase técnica**, sin norma vigente. Ha habido proyectos de ley recurrentes desde al menos 2015 sin aprobarse. Una fuente comercial afirma lo contrario; se descarta por baja fiabilidad y contradicción con fuentes oficiales y periodísticas.
- **La revisión técnica o mecánica vehicular obligatoria tampoco está vigente** a nivel nacional; hay propuestas legislativas en curso. `[V]`
- `[I]` No obstante, las instituciones públicas **sí suelen asegurar** su flota como salvaguarda de bienes del Estado, y las Disposiciones Generales del Presupuesto pueden exigirlo. `[C]`

**Consecuencia de diseño:** póliza y revisión son campos **rastreables y alertables pero no bloqueantes por defecto**. El bloqueo se implementa como **regla configurable**, lista para activarse el día que se apruebe cualquiera de las leyes en trámite.

## Placas metálicas — realidad operativa `[V]`

Hay **desabastecimiento prolongado de placas metálicas**. En marzo de 2026 el Congreso aprobó su compra directa a través del Instituto de la Propiedad, y hay reportes de miles de vehículos circulando sin placa metálica durante años.

**Actualización 2026-08-24 — la magnitud ahora está cifrada** `[P]`, fuente periodística única (El Heraldo, 30/07/2026):

- **Más de 990,000 vehículos circulan sin identificación: ~27 % de un parque de 3.6 millones.** No es un caso de borde: es uno de cada cuatro vehículos del país.
- El IP proyecta distribuir placas nuevas **a partir del último trimestre de 2026**.
- Las placas nuevas incorporarían **RFID**, con puntos de lectura electrónica: 10 antes de cerrar 2026 y 30 más durante 2027, ubicados en retenes, fronteras **y estaciones de peaje**.

**Un campo `placa` obligatorio y único rompería el sistema en la realidad hondureña actual.** La cifra confirma la premisa; no la debilita.

`[I]` **Señal para M-18 y M-19, no requisito todavía.** Si los lectores RFID llegan a las casetas de peaje, aparece una fuente de datos de paso **independiente de lo que declare el motorista** — exactamente el tipo de evidencia que el auditor prefiere. No hay nada que diseñar hoy: no se sabe si el dato será accesible a terceros, ni en qué formato, ni bajo qué convenio. **Se registra para no rediseñar el modelo de paso por caseta cuando ocurra.**

`[C]` Si los vehículos del Estado quedan incluidos, exceptuados o priorizados en el reemplazo de placas. La fuente no lo dice.

## IHTT `[V]`

El IHTT tiene competencia en todo el territorio y emite **Permiso de Explotación**, **Certificado de Operación** y **Permiso Especial (privado)** — incluido el permiso especial de carga general y carga especializada para personas naturales o jurídicas **cuyo giro no es el transporte pero necesitan movilizar carga**.

~~`[C] importante` Determinar si el traslado institucional de carga requiere permiso especial del IHTT.~~ **Resuelto: el PO confirmó que no se requiere ese permiso.** Sale del alcance (D-14).

## Implicaciones de requerimiento

- **El sistema debe** mantener el **expediente del motorista**: número de licencia, categoría o categorías, fecha de emisión, **fecha de vencimiento**, restricciones médicas y adjunto escaneado.
- **El sistema debe** **bloquear la asignación** de un motorista a un vehículo cuya categoría no esté habilitada por su licencia, y bloquear si la licencia estará vencida en cualquier fecha del rango de la misión. Asignar a un motorista sin licencia habilitante **traslada responsabilidad directa a quien autorizó**.
- **El sistema debe** clasificar cada vehículo con los atributos que determinan la categoría requerida: tipo, **peso bruto vehicular en kg**, capacidad de pasajeros, y si es articulado.
- **El sistema debe** alertar con anticipación configurable (60 / 30 / 15 días) el vencimiento de licencias, matrícula, permisos y pólizas.
- **El sistema debe** registrar matrícula y placa, y **tolerar el estado "sin placa metálica"** con el documento sustitutivo o constancia del IP como adjunto.
- **El sistema debe** tratar póliza de seguro y revisión mecánica como campos **opcionales pero rastreables**, con alerta de vencimiento si existen, y con el bloqueo implementado como regla configurable.
- **El sistema debe** incluir un **módulo de accidente o siniestro** con: fecha, hora, lugar con coordenadas, motorista, ocupantes, terceros involucrados, daños, lesionados, número de reporte policial o de la DNVT, denuncia, aseguradora si aplica, fotografías, croquis, y estado del proceso de deducción de responsabilidad.
- **El sistema debe** proveer al motorista una **guía de actuación en accidente accesible sin conexión** desde el móvil, y capturar el reporte inicial offline.
- **El sistema debe** registrar **infracciones y multas de tránsito** asociadas al vehículo y al motorista, con estado de pago y quién asume el costo.
- **El sistema debe** registrar del lado de la carga: tipo, peso, origen, destino, remitente y consignatario — por trazabilidad operativa, no por exigencia del IHTT.

## Limitación de herramienta — leer con cuidado

**Corrección metodológica del 2026-08-24.** La afirmación repetida en este repositorio de que *"los PDF del TSC son escaneos sin capa de texto"* es **cierta solo en parte**, y tratarla como universal hizo abandonar documentos que sí se pueden leer.

| Documento | Diagnóstico real | Nivel |
|---|---|---|
| `Ley-de-Transito.pdf` (TSC) | **Tiene texto.** Generado desde InDesign, streams comprimidos con FlateDecode. Los buscadores indexan su contenido. No se pudo leer por limitación del entorno, no del documento | `[V]` |
| `Decreto-51-2025.pdf` (TSC) | **Tiene texto.** InDesign, una página, metadatos del 11/09/2025 | `[V]` |
| `Acuerdo-No-1012-2021.pdf` (TSC) | **Tiene texto.** Indexado por buscadores con su encabezado de La Gaceta | `[V]` |
| `Objetos-del-Gasto-2019.pdf` (SEFIN) | **Tiene texto.** Indexado con su encabezado de columnas | `[V]` |
| `Circular_003-025_PRESIDENCIA-TSC.pdf` | **Escaneo de imagen real.** JPEG 1725×2221 embebido, digitalizado en Canon iR1643i II, procesado con *Paper Capture* | `[V]` |
| PDF de tarifas de la SAPP | No se pudo diagnosticar en esta ronda | `[C]` |

**Qué falta, entonces:** no OCR, sino **una descarga y una extracción de texto local**. Cuatro de los cinco documentos se abren con cualquier lector de PDF. **El entorno de este agente no tiene shell ni renderizador de PDF**; el conversor web recibe los streams comprimidos y no los descomprime.

> **Esto reduce el insumo #23 de "trabajo de OCR" a "abrir cuatro archivos".** Cualquier persona con un navegador cierra el 80 % de este pendiente en veinte minutos.

## Zonas grises y pendientes

- `[C]` **Texto oficial del Acuerdo No. 1012-2021** — es el bloqueante **real** de la matriz licencia↔vehículo, no el Art. 48. Confirmar categorías, umbrales de masa y pasajeros, y si existe una categoría **DE** análoga a BE.
- `[P]` → `[C]` **Texto oficial del Decreto 51-2025.** Ya no bloquea la matriz; bloquea los campos y alertas del expediente del motorista en M-05.
- `[C]` **Existencia y alcance exactos de la categoría BE.** Una sola fuente.
- `[C]` Si el Acuerdo 1012-2021 fue reformado después de 2021.
- `[C]` **Artículo 51 de la Ley de Tránsito.** Un índice jurídico lo ubica en el *Capítulo I, De las licencias de conducir, arts. 45 a 52*, y una fuente lo asocia a la licencia del motociclista y a la complementariedad con tratados internacionales — **no** a la clasificación liviano/pesado que [NRM-10](NRM-10-peajes.md) le atribuía. Ver la contradicción abierta en esa ficha.
- `[C]` Si las Disposiciones Generales del Presupuesto 2026 exigen asegurar la flota.
- `[C]` Catálogo oficial de **restricciones médicas** de la DNVT (insumo #42) — **buscado el 2026-08-24 sin resultado.** Se confirma `[V]` que el trámite exige exámenes general, visual, psicológico y de tipo sanguíneo en centros autorizados por la DNVT, pero **no existe fuente pública del catálogo de códigos de restricción** que se estampan en la licencia. Es consulta directa a la DNVT, no investigación documental.
- **Vigilar** el avance del anteproyecto de seguro obligatorio y de la revisión mecánica obligatoria: si se aprueban, la regla configurable se activa sin cambiar código.
- **Vigilar** el despliegue de placas RFID desde el último trimestre de 2026.

## Fuentes

Consultadas el **2026-08-24** salvo indicación en contrario.

**Oficiales y jurídicas**
- [Ley de Tránsito, Decreto 205-2005](https://www.tsc.gob.hn/web/leyes/Ley-de-Transito.pdf) — tiene capa de texto; no legible con las herramientas disponibles
- [Decreto No. 51-2025 — reforma al Art. 48](https://www.tsc.gob.hn/web/leyes/Decreto-51-2025.pdf) — tiene capa de texto; no legible con las herramientas disponibles
- [Acuerdo No. 1012-2021 — Reglamento Especial en Materia de Permisos de Conducir](https://www.tsc.gob.hn/web/leyes/Acuerdo-No-1012-2021.pdf) — **la fuente de la matriz**; tiene capa de texto
- [La Gaceta No. 36,940 del 11/09/2025 — índice del Decreto 51-2025](https://leyes.hn/gaceta/36940)
- [vLex Honduras — Decreto No. 51-2025](https://hn.vlex.com/vid/decreto-no-51-2025-1091441440)
- [vLex Honduras — Ley de Tránsito, índice de articulado](https://hn.vlex.com/vid/ley-transito-710588633) — texto tras muro de pago; el índice sitúa el Art. 51 en el capítulo de licencias
- [Colección Legis — Reglamento Especial en Materia de Permisos de Conducir](https://coleccionlegis.com/muestra/documento/lectura/reglamento-especial-en-materia-de-permisos-de-conducir/) — transcripción de las categorías
- [Portal de Trámites — permisos C1, C, C+E, D1 y D](https://tramites.diger.gob.hn/landingPage/DetalleTramite/72)

**Prensa**
- [Metro — Congreso reforma requisitos para licencias C, D y CE, 08/08/2025](https://www.metro.hn/noticias/nacion/congreso-nacional-reforma-requisitos-para-licencias-c-d-y-ce/)
- [La Tribuna — Congreso aprueba reducción de edad para licencias de vehículos pesados, 07/08/2025](https://www.latribuna.hn/2025/08/07/congreso-aprueba-reduccion-de-edad-para-licencias-de-vehiculos-pesados/) — HTTP 403 a consulta automatizada
- [Radio HRN — tipos de licencia y equivalencias, 12/05/2025](https://www.radiohrn.hn/tipos-de-licencias-de-conducir-en-honduras-equivalencias-permisos-manejo-ser-2025-05-12)
- [El Heraldo — placas inteligentes con RFID, 30/07/2026](https://www.elheraldo.hn/portada/placas-inteligentes-lectores-electronicos-sera-nuevo-control-vehicular-honduras-HB31569493)
- [La Prensa — un millón de vehículos sin placas](https://www.laprensa.hn/honduras/un-millon-vehiculos-sin-placas-ip-2026-entrega-listas-BG31730706)

**Consultadas el 2026-08-06**
- [Tipos de licencia de conducir en Honduras](https://www.televicentro.com/tipos-de-licencia-de-conducir-en-honduras-conoce-cual-necesitas-2025-04-25) — consultado 2026-08-06
- [Congreso reforma Art. 48 Ley de Tránsito](https://x.com/Congreso_HND/status/1953563672728809867) — consultado 2026-08-06
- [DNVT impulsa seguro vehicular obligatorio — aún anteproyecto](https://www.tunota.com/honduras-hoy/dnvt-impulsa-seguro-vehicular-obligatorio-aumento-accidentes-2026-03-21) — consultado 2026-08-06
- [Proponen revisión mecánica obligatoria](https://extradigital.hn/proponen-revision-mecanica-obligatoria-para-reducir-accidentes-en-honduras/) — consultado 2026-08-06
- [Congreso aprueba compra directa de placas vehiculares, marzo 2026](https://www.infobae.com/honduras/2026/03/20/congreso-aprueba-compra-directa-de-placas-vehiculares-para-enfrentar-desabastecimiento-en-honduras/) — consultado 2026-08-06
- [IHTT — permisos de explotación y certificados de operación](https://tramites.diger.gob.hn/landingPage/DetalleTramite/34) — consultado 2026-08-06
