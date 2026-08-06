# Riesgos normativos y documentos por obtener

**Fecha de evaluación: 2026-08-06.** Revisa este registro antes de tomar cualquier ficha `NRM-xx` como definitiva.

## Riesgos priorizados

| # | Riesgo | Impacto | Acción | Ficha |
|---|---|---|---|---|
| 1 | **Acuerdo 401-2026 (Reglamento de Viáticos, 23/07/2026)** tiene semanas de vigencia y cambia tarifas y posiblemente reglas de liquidación | **Crítico** — M-10 no se puede cerrar sin él | Obtener el texto **antes** de diseñar el módulo. Parametrizar todo con vigencia por fecha | [NRM-03](NRM-03-viaticos.md) |
| 2 | Códigos del **objeto del gasto** no verificados | Alto — sin ellos no hay imputación ni conciliación | Obtener el Manual de Clasificadores Presupuestarios vigente de SEFIN | [NRM-04](NRM-04-presupuesto-siafi.md) |
| 3 | Vigencia del **Decreto 157-2022** (compra directa de combustible) | Alto — determina el modelo de proveedores de M-09 | Confirmar con la Gerencia Administrativa | [NRM-05](NRM-05-contrataciones-oncae.md) |
| 4 | Posible **nueva Ley de Contratación del Estado** anunciada en 2024, sin evidencia de aprobación | Medio | Confirmar con ONCAE si el Decreto 74-2001 sigue vigente | [NRM-05](NRM-05-contrataciones-oncae.md) |
| 5 | Codificación de **feriados de octubre** ("feriado morazánico") no verificada | Medio — afecta cálculo de viáticos y permisos de día inhábil | Verificar antes de codificar el calendario | [NRM-09](NRM-09-realidad-operativa.md) |
| 6 | **Interfaz con SIAFI**: no se sabe si existe integración disponible para terceros | Medio — cambia el alcance de M-13 | Planificar conciliación por archivo como plan base | [NRM-04](NRM-04-presupuesto-siafi.md) |
| 7 | Reforma al **Art. 48 de la Ley de Tránsito** (2025) sobre categorías CD y CE | Alto — la matriz licencia↔vehículo es la validación de mayor valor legal | Obtener el texto reformado antes de codificar la matriz | [NRM-06](NRM-06-transito-y-licencias.md) |
| 8 | **Seguro obligatorio y revisión mecánica** están en anteproyecto; podrían aprobarse durante el desarrollo | Bajo si se diseña bien | Implementar el bloqueo como **regla configurable**, lista para activarse | [NRM-06](NRM-06-transito-y-licencias.md) |
| 9 | **Ley de Protección de Datos Personales** pendiente en el Congreso desde 2018 | Medio | Diseñar M-17 con minimización y registro de consultas desde ya; retro-adaptar sale más caro | [NRM-07](NRM-07-transparencia-y-datos-personales.md) |
| 10 | ¿Permiso especial del **IHTT** para traslado institucional de carga? Zona gris | Medio — afecta el alcance del módulo de carga | Consultar antes de cerrar el diseño de carga | [NRM-06](NRM-06-transito-y-licencias.md) |

## Documentos a obtener, en orden de prioridad

Los tres primeros son **bloqueantes**.

1. **Acuerdo No. 401-2026** — Reglamento de Viáticos del Poder Ejecutivo (23/07/2026) con sus tablas de zonas, categorías y tarifas — `sefin.gob.hn`
2. **Manual de Clasificadores Presupuestarios vigente** y Clasificador de Objetos del Gasto — `sefin.gob.hn/wp-content/uploads/SAMI/docs/CLASIFICADORES/`
3. **Reglamento interno de uso de vehículos de la institución** (o el modelo del TSC o de AMHON si no existe), más los formatos de bitácora en papel actualmente en uso
4. **MARCI — Marco Rector del Control Interno Institucional** (Acuerdo Administrativo 001-2008) — requiere OCR; extraer el catálogo completo de normas TSC-NOGECI
5. **Acuerdo No. 303 del 24/04/1981**, **Decreto 135-94** y **Decreto 48** — régimen de uso, circulación e identificación de automotores del Estado
6. **Ley de Tránsito, Decreto 205-2005** con la reforma al Art. 48 (2025) — requiere OCR
7. **Disposiciones Generales del Presupuesto 2026** y **Acuerdo 360-2026** — artículos sobre combustible, vehículos y viáticos
8. **Circular STLCC-ONADICI No. 022-03-2024** (uso indebido de vehículos) y **Circular 003-2025-Presidencia-TSC**

## Nota metodológica sobre las fuentes

Muchos PDF oficiales de `tsc.gob.hn`, `onadici.gob.hn` y varios de `sefin.gob.hn` son **escaneos sin capa de texto**. En esta investigación se verificó existencia, numeración y vigencia; en varios casos no fue posible transcribir el articulado.

**No se inventó ningún número de artículo, tarifa ni código presupuestario.** Donde falta el dato, está marcado `[C]` — y así debe permanecer hasta que alguien lo obtenga de la fuente.

## Cómo mantener este registro

- Cuando se obtenga un documento, actualiza su ficha `NRM-xx`, sube el nivel de verificación, anota la fecha, y **tacha la fila aquí**.
- Cuando aparezca una norma nueva, agrégala como riesgo **antes** de que impacte el desarrollo.
- Revisa este registro al inicio de cada sprint. Cinco minutos.
