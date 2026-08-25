# HU-148 — Corregir retroactivamente un parámetro viendo el impacto antes de confirmar, y con asiento de diferencia por cada expediente

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros · M-13 Liquidación y Cierre |
| **Actor** | ACT-08 Gerencia Administrativa |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — depende de la decisión abierta sobre reapertura de ejercicios en ARGOS |

## Historia

**Como** Gerencia Administrativa
**quiero** ver, **antes de confirmar**, cuántos expedientes y qué monto alcanza una corrección retroactiva de parámetro, y que al confirmarla se genere un asiento de diferencia por cada uno sin tocar ni un solo valor histórico
**para** poder responder ante el Tribunal Superior de Cuentas con qué regla se calculó cada valor y por qué cambió, en lugar de tener que explicar una discrepancia que la institución no cometió

## Contexto

No es hipotético. En enero de 2026 COVI anunció un aumento **retroactivo** que incluía subsidios pendientes de 2024 y 2025 `[V]` ([NRM-10](../../01-negocio/normativa/NRM-10-peajes.md)). Estuvo a punto de ocurrir y volverá a intentarse.

El daño de hacerlo mal es **irreversible** ([`RNF-05`](../no-funcionales/RNF-05-temporalidad-normativa.md)): si el catálogo se edita en su lugar, el día que suba la tarifa **todos los reportes de los años anteriores cambian de monto retroactivamente**. Nadie lo nota en el momento; se nota cuando el auditor compara el reporte de hoy con el descargo que la institución presentó hace dos años.

Cuatro obligaciones de `RN-42`, y la tercera es la que más se olvida:

1. **Conservar intacto** el valor histórico y su procedencia.
2. Producir un **reporte de impacto** antes de confirmar.
3. Generar un **asiento de diferencia** por cada expediente alcanzado.
4. **No aplicar** el recálculo automático y silencioso sobre expedientes cerrados.

Y hay un tipo de diferencia que no es económica: **el feriado corregido**. Si un día tratado como hábil resulta feriado, misiones pasadas circularon sin permiso de la máxima autoridad. Eso es diferencia de **cumplimiento**, y la consecuencia debe estar prevista antes de que alguien corrija el calendario a la ligera.

## Reglas que la gobiernan

- [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) — **Regla rectora**: la corrección genera asiento de diferencia; nunca sobrescribe el valor histórico
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El valor congelado al autorizar no se reescribe
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Nada se borra
- [RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md) — El hallazgo posterior no altera el estado del objeto vinculado
- [RN-96](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) — El cierre de ejercicio es corte de imputación; SIGTI informa, no decide su reapertura

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — La diferencia a favor de la institución genera derecho de reclamo, no un simple ajuste
- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — Los expedientes cerrados no se tocan; se listan
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — La corrección alcanza un ejercicio cerrado

## Criterios de aceptación

```gherkin
# language: es
Característica: Corrección retroactiva de parámetro con asiento de diferencia

  Antecedentes:
    Dada una tarifa del punto "Zambrano", categoría "liviana", de L 22.00 vigente del "2026-01-01" al "2026-06-30"
    Y 412 misiones con pasos por ese punto en ese rango
    Y 380 de ellas en estado "CERRADA" y 32 en estado "LIQUIDADA"
    Y una corrección cargada que fija L 24.00 desde el "2026-01-01", pendiente de aprobación

  Escenario: Se rechaza confirmar la corrección sin haber visto el reporte de impacto
    Cuando la Gerencia Administrativa intenta confirmar la corrección sin abrir el reporte de impacto
    Entonces el sistema rechaza la confirmación
    Y muestra "Revise el reporte de impacto antes de confirmar: 412 expedientes alcanzados, efecto estimado L 4,120.00. Es el control que detiene un error de digitación de fecha antes de causar daño."

  Escenario: El reporte de impacto se presenta antes de confirmar
    Cuando la Gerencia Administrativa abre el reporte de impacto
    Entonces ve 412 expedientes, el período alcanzado y el monto total de la diferencia
    Y ve el desglose entre 380 expedientes cerrados y 32 liquidados
    Y ve que los cerrados no se recalculan automáticamente

  Escenario: Se detecta el error de digitación de año antes de causar daño
    Dada una corrección cargada con vigencia desde el "2025-01-01" en lugar de "2026-01-01"
    Cuando la Gerencia Administrativa abre el reporte de impacto
    Entonces el sistema muestra "Vigencia desde el 01/01/2025. Alcanza 1,847 expedientes de dos ejercicios fiscales por L 18,470.00. Verifique el año antes de confirmar."
    Y la Gerencia Administrativa puede devolver la carga sin confirmarla

  Escenario: Los expedientes abiertos se recalculan con asiento; los cerrados no se tocan
    Cuando la Gerencia Administrativa confirma la corrección
    Entonces los 32 expedientes "LIQUIDADA" se recalculan y cada uno recibe su asiento de diferencia
    Y los 380 "CERRADA" no se modifican
    Y el sistema los lista para decisión expediente por expediente
    Y muestra "32 expedientes recalculados con asiento. 380 cerrados listados para su decisión: no se tocan sin acto expreso."

  Escenario: Cada asiento de diferencia es navegable en ambos sentidos
    Dado un asiento de diferencia sobre la misión "OM-2026-0451"
    Cuando el Auditor Interno lo consulta desde el expediente
    Entonces ve valor anterior L 22.00, valor nuevo L 24.00, diferencia L 2.00, el parámetro que la origina y quién autorizó la corrección
    Y desde el parámetro puede llegar a los 32 asientos que produjo

  Escenario: El valor histórico permanece intacto
    Cuando el Auditor Interno consulta la misión "OM-2026-0451" después de la corrección
    Entonces el monto originalmente congelado sigue siendo L 22.00 con su versión de tabla
    Y el asiento de diferencia figura por separado
    Y el sistema no presenta un único valor "corregido" que oculte el original

  Escenario: La corrección de un umbral no borra los hallazgos ya emitidos
    Dado un umbral de desviación corregido retroactivamente de "15" a "25" por ciento
    Y 41 hallazgos de consumo emitidos bajo el umbral anterior
    Cuando la Gerencia Administrativa confirma la corrección
    Entonces los 41 hallazgos permanecen
    Y se marcan como "emitidos bajo umbral anterior" con el nuevo resultado anotado
    Y ninguno desaparece

  Escenario: El feriado corregido produce diferencia de cumplimiento, no económica
    Dado un día "2026-10-03" tratado como hábil
    Y 7 misiones que circularon ese día sin permiso de la máxima autoridad
    Cuando la Gerencia Administrativa confirma la corrección del calendario que lo declara feriado
    Entonces el sistema genera 7 hallazgos de cumplimiento
    Y muestra "7 misiones circularon el 03/10/2026 sin permiso de circulación en día inhábil. Se generan hallazgos de cumplimiento; no hay efecto económico."
    Y ninguna misión cambia de estado

  Escenario: La corrección masiva es reversible como conjunto
    Dada una corrección aplicada sobre 32 expedientes
    Cuando la Gerencia Administrativa detecta un error y revierte la corrección
    Entonces el sistema genera un asiento de reversión por cada uno de los 32
    Y no deja ningún expediente a medio corregir
    Y la cadena de asientos queda visible en su totalidad
```

## Fuera de alcance

- La afectación contable del ejercicio: pertenece a ARGOS ([DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). **SIGTI informa; no decide si el ejercicio se reabre**
- El reclamo por discrepancia de peaje — es [HU-050](HU-050-discrepancia-de-peaje-y-reclamo.md)
- La carga del parámetro corregido — es [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md)
- La reapertura de un expediente terminal: **no existe** ([`RN-93`](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md))

## Notas y pendientes

- `[V]` Que las tarifas de peaje pueden aplicarse retroactivamente, y el anuncio retroactivo de COVI de enero de 2026 — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md)
- `[C]` **Criterio de imputación entre ejercicios fiscales y ventana de apertura** — [`RN-96`](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)
- `[C]` **Procedimiento y plazo para reclamar un peaje mal cobrado** ante el concesionario o la SAPP — insumo **#77**
- `[C]` Si la Gerencia Administrativa quiere decidir los 380 expedientes cerrados uno por uno o por lote con acta. `RN-42` dice *"expediente por expediente"*; con 380 eso puede ser impracticable y hay que confirmarlo
