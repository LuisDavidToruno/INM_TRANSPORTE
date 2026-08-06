# Riesgos normativos y documentos por obtener

**Evaluación inicial 2026-08-06. Actualizado el mismo día tras la revisión del PO** — ver [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md).

Revisa este registro antes de tomar cualquier ficha `NRM-xx` como definitiva.

## Riesgos vigentes

| # | Riesgo | Impacto | Acción | Ficha |
|---|---|---|---|---|
| 7 | Reforma al **Art. 48 de la Ley de Tránsito** (2025) sobre categorías CD y CE | **Alto** — la matriz licencia↔vehículo es la validación de mayor valor legal del sistema, y el PO la confirmó como requisito ("nos tenemos que proteger con la ley") | Obtener el texto reformado antes de fijar la matriz. Modelarla como catálogo con vigencia, no cableada | [NRM-06](NRM-06-transito-y-licencias.md) |
| 8 | **Seguro obligatorio y revisión mecánica** están en anteproyecto; podrían aprobarse durante el desarrollo | Medio — el PO decidió gestionarlos igual, aunque no sean obligatorios | Administrar póliza y revisión con alertas de vencimiento. **Bloqueo como regla configurable, apagada por defecto** | [NRM-06](NRM-06-transito-y-licencias.md) |
| 11 | **Tarifas de peaje** cambian por revisión periódica de los contratos de concesión | Medio — afecta la estimación de costo de ruta | Modelar tarifas y categorías por ejes con **vigencia por rango de fechas**. Verificar quién publica los ajustes | `NRM-10` |
| 12 | **Códigos del objeto del gasto** (SEFIN) no verificados | Bajo — el PO indicó que se investigan sin problema, y la estructura presupuestaria la define ARGOS | Investigar cuando se necesite; no bloquea | [NRM-04](NRM-04-presupuesto-siafi.md) |
| 13 | Codificación de **feriados de octubre** ("feriado morazánico") no verificada | Bajo — el calendario se maneja junto con Talento Humano | Confirmar con Talento Humano, no con la ley | [NRM-09](NRM-09-realidad-operativa.md) |

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

Ordenados por prioridad real después de la revisión.

1. **Texto de la reforma al Art. 48 de la Ley de Tránsito** (2025) — categorías CD y CE
2. **Tarifas vigentes de peaje** por punto y categoría de ejes, y quién las publica
3. **Reglamento interno de uso de vehículos de la institución**, más los formatos de bitácora en papel en uso
4. **Ley de Tránsito, Decreto 205-2005** completa — requiere OCR
5. **MARCI — Marco Rector del Control Interno Institucional** (Acuerdo Administrativo 001-2008) — requiere OCR; extraer el catálogo completo de normas TSC-NOGECI
6. **Acuerdo No. 303 del 24/04/1981**, **Decreto 135-94** y **Decreto 48** — uso, circulación e identificación de automotores del Estado
7. **Circular STLCC-ONADICI No. 022-03-2024** y **Circular 003-2025-Presidencia-TSC** — uso indebido de vehículos

## Nota metodológica sobre las fuentes

Muchos PDF oficiales de `tsc.gob.hn`, `onadici.gob.hn` y varios de `sefin.gob.hn` son **escaneos sin capa de texto**. En esta investigación se verificó existencia, numeración y vigencia; en varios casos no fue posible transcribir el articulado.

**No se inventó ningún número de artículo, tarifa ni código presupuestario.** Donde falta el dato, está marcado `[C]` — y así debe permanecer hasta que alguien lo obtenga de la fuente.

## Cómo mantener este registro

- Cuando se obtenga un documento, actualiza su ficha `NRM-xx`, sube el nivel de verificación, anota la fecha, y **tacha la fila aquí**.
- Cuando aparezca una norma nueva, agrégala como riesgo **antes** de que impacte el desarrollo.
- **Los números de riesgo no se reciclan**, igual que los IDs de artefactos.
- Revisa este registro al inicio de cada sprint. Cinco minutos.
