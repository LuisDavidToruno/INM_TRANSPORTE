# NRM-06 — Tránsito, licencias, matrícula y siniestros

| Campo | Valor |
|---|---|
| **Ámbito** | Habilitación del motorista, matrícula del vehículo, seguro, revisión, responsabilidad ante accidente |
| **Módulos afectados** | M-03, M-04, M-05, M-07, M-12 |
| **Última verificación** | 2026-08-06 |
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
| Ley de Tránsito | Decreto No. 205-2005 | Vigente | `[V]` |
| Reforma al Artículo 48 — permisos de conducir categorías CD y CE | aprobada agosto 2025 | Vigente | `[V]` `[C]` texto |
| DNVT — Dirección Nacional de Vialidad y Transporte | administra licencias | — | `[V]` |
| Instituto de la Propiedad (IP) — Registro Vehicular | matrícula y placas | — | `[V]` |
| IHTT — Instituto Hondureño del Transporte Terrestre | permisos de explotación y especiales | — | `[V]` |

## Categorías de licencia `[V]`

Ocho categorías vigentes. Fuentes concordantes (portal de trámites y prensa); `[C]` contrastar contra el texto de la Ley de Tránsito y la DNVT.

| Categoría | Habilita a conducir |
|---|---|
| **A** | Ciclomotores y motocicletas de motor o eléctricas |
| **B** | Automóviles livianos no comprendidos en A ni B1 |
| **B1** | Triciclos y cuadriciclos de motor (mototaxi, cuatrimoto) |
| **C1** | Vehículos de carga de **hasta 7,500 kg** |
| **C** | Vehículos de carga **superiores a 7,500 kg**, no articulados |
| **D1** | Autobuses de **hasta 25 pasajeros** |
| **D** | Autobuses |
| **CE** | Furgón de carga pesada **articulado** |

Esta es la tabla operativamente decisiva para M-07. Se modela como **catálogo configurable con vigencia**, no cableada: la reforma de 2025 ya modificó las categorías CD y CE, y se reportó reducción de la edad mínima para CE. `[C]` obtener el texto reformado antes de fijar la matriz completa.

## Seguro obligatorio y revisión vehicular — hallazgo importante

- **El seguro de responsabilidad civil vehicular NO es obligatorio en Honduras.** `[V]` La iniciativa de la DNVT (marzo 2026) es un **anteproyecto en fase técnica**, sin norma vigente. Ha habido proyectos de ley recurrentes desde al menos 2015 sin aprobarse. Una fuente comercial afirma lo contrario; se descarta por baja fiabilidad y contradicción con fuentes oficiales y periodísticas.
- **La revisión técnica o mecánica vehicular obligatoria tampoco está vigente** a nivel nacional; hay propuestas legislativas en curso. `[V]`
- `[I]` No obstante, las instituciones públicas **sí suelen asegurar** su flota como salvaguarda de bienes del Estado, y las Disposiciones Generales del Presupuesto pueden exigirlo. `[C]`

**Consecuencia de diseño:** póliza y revisión son campos **rastreables y alertables pero no bloqueantes por defecto**. El bloqueo se implementa como **regla configurable**, lista para activarse el día que se apruebe cualquiera de las leyes en trámite.

## Placas metálicas — realidad operativa `[V]`

Hay **desabastecimiento prolongado de placas metálicas**. En marzo de 2026 el Congreso aprobó su compra directa a través del Instituto de la Propiedad, y hay reportes de miles de vehículos circulando sin placa metálica durante años.

**Un campo `placa` obligatorio y único rompería el sistema en la realidad hondureña actual.**

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

## Zonas grises y pendientes

- `[C]` **Texto de la reforma al Art. 48 (2025)** sobre categorías CD y CE. Es el pendiente más importante de esta ficha: sin él, la matriz definitiva no se puede fijar. Registrado como insumo #20 y riesgo #7.
- `[C]` Contraste de la tabla de las 8 categorías contra el texto oficial de la Ley de Tránsito y la DNVT.
- `[C]` Si las Disposiciones Generales del Presupuesto 2026 exigen asegurar la flota.
- **Vigilar** el avance del anteproyecto de seguro obligatorio y de la revisión mecánica obligatoria: si se aprueban, la regla configurable se activa sin cambiar código.

## Fuentes

- [Ley de Tránsito, Decreto 205-2005](https://www.tsc.gob.hn/web/leyes/Ley-de-Transito.pdf) — consultado 2026-08-06
- [Tipos de licencia de conducir en Honduras](https://www.televicentro.com/tipos-de-licencia-de-conducir-en-honduras-conoce-cual-necesitas-2025-04-25) — consultado 2026-08-06
- [Congreso reforma Art. 48 Ley de Tránsito, categorías CD y CE](https://x.com/Congreso_HND/status/1953563672728809867) — consultado 2026-08-06
- [DNVT impulsa seguro vehicular obligatorio — aún anteproyecto](https://www.tunota.com/honduras-hoy/dnvt-impulsa-seguro-vehicular-obligatorio-aumento-accidentes-2026-03-21) — consultado 2026-08-06
- [Proponen revisión mecánica obligatoria](https://extradigital.hn/proponen-revision-mecanica-obligatoria-para-reducir-accidentes-en-honduras/) — consultado 2026-08-06
- [Congreso aprueba compra directa de placas vehiculares, marzo 2026](https://www.infobae.com/honduras/2026/03/20/congreso-aprueba-compra-directa-de-placas-vehiculares-para-enfrentar-desabastecimiento-en-honduras/) — consultado 2026-08-06
- [IHTT — permisos de explotación y certificados de operación](https://tramites.diger.gob.hn/landingPage/DetalleTramite/34) — consultado 2026-08-06
