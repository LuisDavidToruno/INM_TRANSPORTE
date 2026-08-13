# RNF-02 — El sistema soporta el crecimiento del acervo sin que nada se borre nunca

| Campo | Valor |
|---|---|
| **Categoría** | Rendimiento / Operabilidad / Portabilidad |
| **Prioridad** | Alto |
| **Origen** | Insumo #10 ("alto flujo") combinado con [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md): sin borrado físico, el acervo solo crece |
| **Afecta arquitectura** | **Sí — determinante para la selección de stack (Sprint 2).** Un motor de datos que no sostenga este crecimiento con operación simple queda descartado |

## Enunciado

SIGTI **no borra nada**: ni bitácoras, ni asientos de auditoría, ni fotografías de comprobantes, ni posiciones de ruta. El acervo crece de forma monótona durante todo el plazo de conservación. El sistema **debe** seguir cumpliendo [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md) cuando el acervo llegue al **triple** del juego de datos de referencia `JDR-1`, y **debe** permitir mover histórico a almacenamiento frío **sin volverlo inconsultable**.

Esto no es dimensionamiento de servidor: es una decisión de diseño. Un sistema que asume que el histórico se archiva y desaparece no sirve, porque la auditoría posterior sobre una misión cerrada ([`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md)) exige que el expediente completo siga ahí, con sus adjuntos.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Cumplimiento de los umbrales de [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md) con `3 × JDR-1` cargado | 100 % de las operaciones, sin excepción |
| Degradación de `p95` al pasar de `JDR-1` a `3 × JDR-1` | ≤ 50 % |
| Registros eliminados físicamente por cualquier proceso del sistema | **0.** No existe ninguna operación de borrado sobre datos operativos, de auditoría o de bienes |
| Crecimiento estimado de la base relacional | `[I]` ≈ 8 GB/año a volumen `JDR-1`, sin adjuntos |
| Crecimiento estimado de adjuntos (fotografías, escaneos) | `[I]` ≈ 30 GB/año a 5 fotos por misión y 300 KB por foto |
| Crecimiento estimado de posiciones de ruta | `[I]` ≈ 4 GB/año — es la serie que más crece y la de menor valor unitario |
| Aviso anticipado de agotamiento de disco | Cuando el espacio libre proyectado sea < 90 días, con aviso en la pantalla de estado ([`RNF-20`](RNF-20-observabilidad-y-diagnostico.md)) |
| Histórico movido a almacenamiento frío que quede inconsultable | **0.** Puede tardar más, no puede desaparecer |
| Tiempo de consulta de un expediente en almacenamiento frío | < 60 s, con aviso explícito de que se está recuperando |
| Densidad de la serie de posiciones tras un año | Se admite **reducción de densidad** (una posición cada 30 min en lugar de cada 5), **nunca eliminación de la serie**. La reducción queda registrada como asiento |

`[C]` El plazo de conservación —y por tanto el volumen total— depende del insumo #71. [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) lo deja abierto expresamente y exige que la retención sea **parámetro configurable, no cableada**.

## Cómo se verifica

1. **Corrida a `3 × JDR-1`**: se triplica el generador y se repite íntegra la batería de medición de [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md). Se compara curva contra curva.
2. **Auditoría de borrado**: revisión de todo el código y de los procedimientos del motor de datos buscando operaciones de eliminación sobre entidades operativas y de auditoría. El resultado esperado es cero, y se documenta la excepción de cualquier tabla técnica (sesiones, caché, cola de trabajos) que sí puede purgarse.
3. **Prueba de anulación**: se anula una orden de misión, un vale de combustible y una bitácora. Se verifica que en los tres casos el registro original sigue presente y aparece un asiento reverso con motivo y autor, conforme a [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md).
4. **Prueba de archivado y recuperación**: se archiva el primer año de `JDR-1`, se verifica que el expediente sigue abriéndose desde la interfaz normal, y se cronometra.
5. **Prueba de proyección de disco**: se llena el disco al 85 % y se verifica que la pantalla de estado avisa antes de que sea un problema, no cuando ya lo es.
6. **Prueba de reducción de densidad**: se ejecuta la reducción sobre la serie de posiciones del primer año y se verifica que el recorrido de la misión sigue siendo reconstruible y que quedó constancia de qué se redujo y cuándo.

## Consecuencia de no cumplirlo

Dos formas de fallar, ambas caras:

- **Falla silenciosa**: alguien "resuelve" la lentitud borrando histórico. En ese momento el sistema deja de satisfacer [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) y pierde su valor probatorio completo. Un acervo con un hueco no es un acervo parcial: es un acervo sin credibilidad.
- **Falla ruidosa**: el disco del servidor de la institución se llena un martes, la operación se detiene, y no hay equipo de TI en la delegación para diagnosticarlo.

## Trazabilidad

- Módulos: transversal — con impacto directo en M-08, M-14, M-19
- Reglas: [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [`RN-05`](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)
- Normativa: [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)
- Casos especiales: [`CE-27`](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md), [`CE-28`](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md)
- Requisitos relacionados: [`RNF-01`](RNF-01-rendimiento-de-consulta-y-operacion.md), [`RNF-04`](RNF-04-bitacora-append-only-con-hash-encadenado.md), [`RNF-17`](RNF-17-retencion-y-depuracion-diferenciada.md)
- Insumos: #67, #71
