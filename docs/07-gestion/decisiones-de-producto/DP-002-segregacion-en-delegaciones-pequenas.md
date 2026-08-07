# DP-002 — Segregación de funciones en delegaciones con personal insuficiente

| Campo | Valor |
|---|---|
| **Fecha** | 2026-08-06 |
| **Estado** | **Decisión provisional del equipo. Requiere ratificación del PO y pronunciamiento de Auditoría Interna** |
| **Origen** | Hallazgo `HN1-01` de la revisión normativa del Bloque 1 |
| **Sprint / Bloque** | Sprint 0 / Bloque 1 |

## El problema

El MARCI exige cinco funciones de control en personas distintas: solicitar, autorizar, despachar, entregar combustible, liquidar. Eso implica **diez pares incompatibles y un mínimo de cinco personas por misión**.

**Una delegación regional con tres empleados no puede cumplirlo. Por aritmética, no por falta de voluntad.**

Si el sistema aplica el bloqueo absoluto sin salida, la delegación no opera y alguien desactiva el control en la primera semana del piloto. Si el sistema ofrece una excepción fácil, el control no existe.

## Lo que pasó, y por qué esto es un hallazgo y no un diseño

Tres artefactos del Bloque 1, escritos en paralelo por especialidades distintas, dieron **tres respuestas incompatibles**:

| Artefacto | Postura |
|---|---|
| [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) | "La regla no se relaja" |
| [orden-de-mision.md §3.3](../../03-arquitectura/estados/orden-de-mision.md) | "La solución **no es** una excepción configurable: es el escalamiento a sede. Una excepción registrada es evidencia en contra ante el TSC" |
| [actores-y-roles.md §5.4](../../01-negocio/actores-y-roles.md) + [PR-01 PC-18](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) + acciones 27–28 de la matriz | Diseñan un **régimen de excepción** que levanta expresamente las incompatibilidades I-02 a I-06, I-08 e I-09 |

La tercera versión es la que un desarrollador implementaría, porque está en la matriz de permisos como acción disponible. **Y es la única que ninguna regla `RN-xx` gobierna.**

El riesgo, dicho por el revisor: *el día del hallazgo, la institución tendría en su propia documentación la prueba de que sabía que el control estaba levantado y no supo decidir si podía levantarlo.*

## Lo que se investigó

- `[V]` Las guías de ONADICI tratan la **Separación de Funciones Incompatibles** como norma propia del MARCI.
- `[P]` La Guía General del MARCI (3ª ed.) reconoce la dificultad de las entidades pequeñas y propone **delegar a mandos medios o a personal independiente** — que es exactamente el escalamiento, no un levantamiento de incompatibilidades.
- `[C]` **No se encontró nada que respalde levantar incompatibilidades a cambio de controles compensatorios.**

Limitación: `onadici.gob.hn` devuelve un certificado TLS de otro dominio y no permite descargar la fuente principal. El Manual de Normas de Control Interno del TSC se descargó pero **no tiene capa de texto**. Ambos se agregan al trabajo de OCR del insumo #23.

## Decisión provisional

**Se adopta el Nivel 1 — escalamiento a sede. El Nivel 2 queda diseñado pero NO se implementa.**

### Qué se construye

Cuando una delegación no puede segregar localmente, **la función incompatible la ejerce remotamente un puesto de la sede central**. Las tres funciones que no requieren presencia física —autorizar, aprobar fondo, cerrar— salen de la delegación por diseño. Sin conectividad, opera el código de autorización fuera de línea.

### Qué NO se construye por ahora

El régimen de excepción del Nivel 2: la resolución de la máxima autoridad que levanta pares de incompatibilidad enumerados, con convalidación posterior.

**No se borra el diseño** — está trabajado y es bueno. Queda como contingencia documentada, marcada como no implementable hasta que esta decisión se ratifique.

### Por qué esta dirección y no la otra

Es la conservadora y la reversible. Construir el escalamiento y descubrir que hacía falta el régimen de excepción cuesta un sprint. Construir el régimen de excepción y descubrir que el TSC no lo acepta cuesta el hallazgo.

Y con la evidencia de hoy, el escalamiento es lo único que tiene respaldo `[P]` en las propias guías del MARCI.

## Lo que hace falta para cerrar esta decisión

| # | Qué | A quién | Bloquea |
|---|---|---|---|
| 1 | **Pronunciamiento sobre si acepta el régimen de excepción con controles compensatorios** | **Auditoría Interna de la institución** | La operación de toda delegación pequeña. Insumo #26 |
| 2 | Mapa de delegaciones con dotación real de personal, y qué puesto de sede respalda a cada una | Talento Humano / Gerencia Administrativa | Saber si el Nivel 1 alcanza. Insumo #27 |
| 3 | OCR de la Guía General del MARCI y del Manual de Normas del TSC | — | Cerrar el `[C]` sobre controles compensatorios. Insumo #23 |

**En la práctica, lo que Auditoría Interna acepte pesa más que lo que diga la guía.** Por eso el punto 1 es el que hay que mover primero, y es el que más tarda.

## Si se ratifica al revés

Si Auditoría Interna avala el régimen de excepción, se revierte esta decisión escribiendo un `DP-003` que la supersede — no editando ésta. Y hay que:

1. Escribir la `RN-54` que gobierne el régimen: qué pares levanta, quién lo declara, con qué vigencia, y qué convalidación exige
2. Corregir `RN-01` y `orden-de-mision.md §3.3`, que hoy dicen por escrito que ese régimen no existe
3. Mantener intacto el **núcleo irreductible**: I-07, I-10, I-11, I-12, I-13 no se levantan ni en ese escenario

## Trazabilidad

- Hallazgo: [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md), `HN1-01`
- Reglas afectadas: [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md)
- Artefactos afectados: `actores-y-roles.md` §5.4, `PR-01` PC-18 y matriz acciones 27–28, `orden-de-mision.md` §3.3
- Insumos: #23, #26, #27
