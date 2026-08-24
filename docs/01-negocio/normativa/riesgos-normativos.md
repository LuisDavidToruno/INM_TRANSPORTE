# Riesgos normativos y documentos por obtener

**Evaluación inicial 2026-08-06. Actualizado el mismo día tras la revisión del PO** — ver [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

**Segunda ronda de investigación pública: 2026-08-24.** Se reevaluaron los riesgos 7, 12, 14, 15 y 17, y se abrieron los riesgos 18 a 21.

Revisa este registro antes de tomar cualquier ficha `NRM-xx` como definitiva.

## Riesgos vigentes

| # | Riesgo | Impacto | Acción | Ficha |
|---|---|---|---|---|
| 7 | ~~Reforma al **Art. 48 de la Ley de Tránsito** (2025) sobre categorías CD y CE~~ **Reformulado el 2026-08-24: el Art. 48 no contiene la matriz.** El riesgo real es que **no se ha leído el Acuerdo No. 1012-2021**, *Reglamento Especial en Materia de Permisos de Conducir*, del 04/06/2021 — que es donde vive la matriz licencia↔vehículo | **Alto** — sin cambio. Es la validación de mayor valor legal del sistema, y el PO la confirmó como requisito ("nos tenemos que proteger con la ley") | Obtener el **Acuerdo 1012-2021**, no el Art. 48. Las ocho categorías conocidas ya están `[V]` por fuentes concordantes; faltan los umbrales literales y `BE`, que solo tiene `[P]`. Modelarla como catálogo con vigencia, no cableada | [NRM-06](NRM-06-transito-y-licencias.md) |
| 8 | **Seguro obligatorio y revisión mecánica** están en anteproyecto; podrían aprobarse durante el desarrollo | Medio — el PO decidió gestionarlos igual, aunque no sean obligatorios | Administrar póliza y revisión con alertas de vencimiento. **Bloqueo como regla configurable, apagada por defecto** | [NRM-06](NRM-06-transito-y-licencias.md) |
| 14 | ~~Contradicción abierta entre el comunicado de la SIT y un agregador comercial~~ **Contradicción resuelta el 2026-08-24 en contra del agregador**, por cinco fuentes. Riesgo remanente: **el congelamiento tarifario es condicional y sin plazo, y no hay evidencia entre marzo y agosto de 2026** | **Medio** — bajó de Alto. Ya hay una tarifa de trabajo respaldada por el regulador; lo que falta es confirmar que sigue vigente | **Cargar la tabla de once categorías como juego de referencia marcado `[P]`**, con fuente y fecha visibles. **No promoverla a producción** sin confirmar con COVI-H o la SAPP | [NRM-10](NRM-10-peajes.md) |
| 15 | **No se conoce la lista oficial de exoneraciones** de peaje ni si alcanza a vehículos administrativos del Estado. **Cuatro vías de investigación agotadas el 2026-08-24 sin resultado** | **Medio** — bajó de Alto. Ya no decide cómo se construye M-18: la exoneración se modela como dato del vehículo y el diseño es el mismo pague o no. Sigue decidiendo **cuánto estima** el sistema | **No hay más que investigar.** Solicitud formal a la SAPP o consulta a COVI-H. Conclusión de trabajo: **el vehículo administrativo paga, y paga como liviano — L. 22** `[V]` en la parte de categoría | [NRM-10](NRM-10-peajes.md) |
| 16 | **Tarifas de peaje se revisan cada enero**, con aplicación retroactiva o reversión a mitad de proceso — ocurrió en 2025 y 2026. **Confirmado el 2026-08-24**: se negocia un ajuste **gradual a cuatro años** | Medio | Vigencia por rango de fechas, cálculo a la fecha del hecho, y **soporte de corrección retroactiva con asiento de diferencia**. Si se cierra el acuerdo, los cuatro tramos se cargan de una vez | [NRM-10](NRM-10-peajes.md) |
| 17 | ~~**Dos PDF oficiales son escaneos sin capa de texto**~~ **Falso en la mayoría de los casos.** El 2026-08-24 se diagnosticaron cinco PDF: **cuatro tienen capa de texto** (Ley de Tránsito, Decreto 51-2025, Acuerdo 1012-2021, Objetos del Gasto) y **uno es escaneo real** (Circular 003-2025-Presidencia-TSC) | **Bajo** — bajó de Medio. No hace falta OCR para casi nada | **Abrir cuatro archivos con un lector de PDF.** El entorno de investigación no tiene shell ni renderizador; una persona con navegador cierra esto en veinte minutos | [NRM-06](NRM-06-transito-y-licencias.md), [NRM-10](NRM-10-peajes.md), [NRM-04](NRM-04-presupuesto-siafi.md) |
| 12 | **Códigos del objeto del gasto** (SEFIN) no verificados. **Avance parcial el 2026-08-24**: grupo **35600 Combustibles y Lubricantes** y subcódigos localizados `[P]`. Faltan mantenimiento, llantas, repuestos, seguros y peajes | Bajo — sin cambio | Confirmar tres cosas antes de usarlo: que aplica a Administración Central y no solo a municipios, que el clasificador de 2019 sigue vigente en 2026, y la transcripción literal | [NRM-04](NRM-04-presupuesto-siafi.md) |
| 13 | Codificación de **feriados de octubre** ("feriado morazánico") no verificada | Bajo — el calendario se maneja junto con Talento Humano | Confirmar con Talento Humano, no con la ley | [NRM-09](NRM-09-realidad-operativa.md) |
| **18** | **Falta la categoría de licencia `BE`** en `CLAUDE.md` y en el marco del proyecto, que enumera ocho. El Acuerdo 1012-2021 la contempla como categoría propia `[P]` — *vehículos de categoría B enganchados a remolque* | **Alto** — un pickup que remolca plataforma, generador o lancha cae en BE, no en B. Si la matriz se cablea con ocho categorías, **el bloqueo duro que el PO pidió como protección legal falla justo donde más se necesita** | Confirmar BE contra el Acuerdo 1012-2021, y verificar si existe una **DE** análoga. **El analista corrige `CLAUDE.md` y las reglas afectadas; esta ficha no las toca** | [NRM-06](NRM-06-transito-y-licencias.md) |
| **19** | **La atribución del criterio de clasificación de peaje al "Artículo 51 de la Ley de Tránsito" no está corroborada.** Se revisaron tres fuentes el 2026-08-24 y ninguna la sostiene; un índice jurídico ubica el Art. 51 en el capítulo de licencias de conducir | Medio — no cambia el diseño, pero **es una afirmación `[V]` que no lo era**. Si sobrevive en una regla o en el código, nadie vuelve a cuestionarla | Degradado a `[C]` en [NRM-10](NRM-10-peajes.md). **Verificar si alguna `RN-xx` cita el Art. 51 y corregirla** | [NRM-10](NRM-10-peajes.md) |
| **20** | **Placas con RFID desde el último trimestre de 2026**, con puntos de lectura en retenes, fronteras **y estaciones de peaje** `[P]`. Y **más de 990,000 vehículos — 27 % del parque — circulan hoy sin identificación** `[P]` | Medio — **confirma** la premisa de "sin placa metálica es estado válido" y abre una fuente futura de datos de paso independiente de la declaración del motorista | **No diseñar nada hoy.** Vigilar el despliegue. Evitar modelar el paso por caseta de forma que solo admita captura manual | [NRM-06](NRM-06-transito-y-licencias.md), [NRM-10](NRM-10-peajes.md) |
| **21** | **Contradicción sobre dónde van las franjas azul–blanco–azul**: la ficha dice *"puertas laterales"*, las fuentes consultadas el 2026-08-24 dicen *"partes laterales"* | Bajo — pero es lo que decide si la **motocicleta del Estado** tiene obligación de rotulación, que es el insumo #43 | Leer el Acuerdo 303 o la Circular 003-2025-Presidencia-TSC — **la única que sí requiere OCR**. Mientras tanto, no cablear ubicaciones esperadas por tipo de vehículo | [NRM-02](NRM-02-bienes-del-estado.md) |

## Riesgos cerrados en la revisión del PO

| # | Riesgo original | Por qué se cierra |
|---|---|---|
| 1 | Acuerdo 401-2026, Reglamento de Viáticos | **Viáticos salen del alcance.** Los maneja ARGOS. Comunicación por API |
| 2 | Códigos del objeto del gasto | Reclasificado como riesgo #12, prioridad baja |
| 3 | Vigencia del Decreto 157-2022 sobre compra directa de combustible | **SIGTI no compra combustible.** Administración aprueba un monto o unas órdenes de pago; el sistema gestiona la asignación y el consumo de ese fondo |
| 4 | Posible nueva Ley de Contratación del Estado | **Fuera de alcance.** Es problema de otros sistemas de la institución |
| 5 | Feriados de octubre | Reclasificado como riesgo #13, prioridad baja |
| 6 | Interfaz con SIAFI | **Integración diferida.** No se inicia con ella |
| 9 | Ley de Protección de Datos Personales pendiente en el Congreso | **No se diseña para anticiparla.** Se conserva solo el control de acceso por rol y el registro de consultas, que exige el MARCI de todas formas |
| 10 | Permiso especial del IHTT para traslado de carga | **No se requiere.** Confirmado por el PO |

## Documentos a obtener

**Reordenados el 2026-08-24**, y separados por lo que cuesta conseguirlos. Los cuatro primeros son enlaces directos que solo hay que abrir.

### Se abren con un navegador — URL conocida, PDF con capa de texto

| # | Documento | URL | Desbloquea |
|---|---|---|---|
| 1 | **Acuerdo No. 1012-2021** — Reglamento Especial en Materia de Permisos de Conducir | `tsc.gob.hn/web/leyes/Acuerdo-No-1012-2021.pdf` | **La matriz licencia↔vehículo.** Riesgos 7 y 18 |
| 2 | **Decreto No. 51-2025** — reforma por adición al Art. 48 | `tsc.gob.hn/web/leyes/Decreto-51-2025.pdf` | Campos y alertas del expediente del motorista en M-05 |
| 3 | **Ley de Tránsito, Decreto 205-2005** completa | `tsc.gob.hn/web/leyes/Ley-de-Transito.pdf` | Art. 51 (riesgo 19) y el resto del articulado |
| 4 | **Objetos del Gasto** (SEFIN) | `sefin.gob.hn/wp-content/uploads/SAMI/docs/CLASIFICADORES/Objetos-del-Gasto-2019.pdf` | Riesgo 12 |

### Requieren OCR de verdad

5. **Circular 003-2025-Presidencia-TSC** — uso, circulación e identificación de vehículos del Estado. **Escaneo de imagen confirmado.** Riesgo 21 e insumo #43
6. **MARCI — Marco Rector del Control Interno Institucional** (Acuerdo Administrativo 001-2008) — extraer el catálogo completo de normas TSC-NOGECI
7. **Acuerdo No. 303 del 24/04/1981**, **Decreto 135-94** y **Decreto 48**
8. **Circular STLCC-ONADICI No. 022-03-2024**

### No se consiguen investigando — hay que preguntar

9. **Lista oficial de exoneraciones de peaje** — solicitud formal a la SAPP o consulta a COVI-H. Cuatro vías web agotadas
10. **Confirmación de la tarifa de peaje vigente hoy** — COVI-H o SAPP
11. **Facturación de peajes**: factura fiscal en caseta, CoviPass empresarial, estado de cuenta institucional — `covih.com` bloquea toda consulta automatizada
12. **Catálogo oficial de restricciones médicas de la DNVT** — sin fuente pública. Consulta directa a la DNVT
13. **Reglamento interno de uso de vehículos de la institución**, más los formatos de bitácora en papel en uso

### Localizados pero no consultados

14. **Disposiciones Generales del Presupuesto 2026** — `sefin.gob.hn/wp-content/uploads/Presupuesto/2026/Proyecto/index_html_files/Disposiciones-Generales-2026.pdf`. Es la vía para los topes sobre combustible y vehículos, y para saber si exigen asegurar la flota

## Nota metodológica sobre las fuentes

**Corregida el 2026-08-24.** La afirmación de que *"los PDF oficiales son escaneos sin capa de texto"* era una generalización, y costó cara: hizo abandonar cuatro documentos legibles.

**El diagnóstico correcto es por documento:**

- **Con capa de texto** — Ley de Tránsito, Decreto 51-2025, Acuerdo 1012-2021, Objetos del Gasto de SEFIN. Generados desde InDesign u ofimática, con streams comprimidos en FlateDecode. **Los buscadores indexan su contenido.** El conversor web del entorno de investigación no los descomprime; **un lector de PDF normal sí.**
- **Escaneo de imagen real** — Circular 003-2025-Presidencia-TSC: JPEG embebido, digitalizado en Canon iR1643i II. **Esta sí requiere OCR.**

**La lección operativa:** antes de declarar un PDF ilegible, mirar si el buscador devuelve texto de su interior. Si lo devuelve, el documento tiene texto y el problema es la herramienta.

**No se inventó ningún número de artículo, tarifa ni código presupuestario.** Donde falta el dato, está marcado `[C]` — y así debe permanecer hasta que alguien lo obtenga de la fuente.

**Sitios que bloquean la consulta automatizada** (verificado 2026-08-24): `covih.com` en todas sus rutas (HTTP 403), `ppp.worldbank.org` (403), `laprensa.hn` de forma intermitente (403). El sitio de la SAPP **migró de dominio**: rutas indexadas en `sapp.gob.hn` devuelven 404, mientras que el sitio anterior en `www.sapp.gob.hn` sigue respondiendo. **Dos comunicados relevantes sobre tarifas de peaje quedaron inaccesibles por esa migración.**

## Cómo mantener este registro

- Cuando se obtenga un documento, actualiza su ficha `NRM-xx`, sube el nivel de verificación, anota la fecha, y **tacha la fila aquí**.
- Cuando aparezca una norma nueva, agrégala como riesgo **antes** de que impacte el desarrollo.
- **Los números de riesgo no se reciclan**, igual que los IDs de artefactos.
- Revisa este registro al inicio de cada sprint. Cinco minutos.
