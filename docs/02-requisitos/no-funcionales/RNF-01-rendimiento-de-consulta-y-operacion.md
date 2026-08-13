# RNF-01 — Toda pantalla de trabajo responde bajo umbral con el acervo histórico completo en línea

| Campo | Valor |
|---|---|
| **Categoría** | Rendimiento |
| **Prioridad** | Alto |
| **Origen** | Insumo #10 resuelto por el PO: **"alto flujo"**. El volumen genera requisitos, no solo dimensionamiento |
| **Afecta arquitectura** | **Sí** — condiciona el modelo de consulta, la estrategia de índices y la separación entre lectura operativa y lectura analítica. No es determinante de stack por sí solo |

## Enunciado

El sistema **debe** sostener sus tiempos de respuesta con **todo el histórico consultable en línea**, no solo con el año en curso. La auditoría no pregunta por lo de esta semana: pregunta por una misión de hace tres años, y lo hace mientras la operación del día sigue corriendo.

El umbral se mide **contra el juego de datos de referencia `JDR-1`**, no contra una base vacía. Una pantalla que responde en 300 ms con 200 registros y en 40 segundos con 100,000 no cumple este requisito: no lo cumplió nunca, solo no se había probado.

## Juego de datos de referencia `JDR-1`

Toda medición de este `RNF` y del [`RNF-02`](RNF-02-volumen-y-crecimiento-del-acervo.md) se ejecuta contra este conjunto sintético.

`[I]` **Derivación** — las entradas están `[C]` (insumo #67); la aritmética es nuestra:

| Entrada | Valor supuesto | Nivel |
|---|---|---|
| Vehículos en flota | 200 | `[C]` #67 |
| Delegaciones y dependencias | 40 | `[C]` #67 |
| Usuarios con cuenta | 400, de ellos 60 concurrentes en hora pico | `[C]` #67 |
| Misiones por vehículo por semana | 2 | `[I]` |
| Años de operación en línea | 5 | `[C]` #71 (plazo de conservación) |

| Magnitud derivada | Volumen de `JDR-1` |
|---|---|
| Órdenes de misión | **100,000** (200 × 2 × 50 semanas × 5 años) |
| Eventos de bitácora | 1,500,000 (≈ 15 por misión) |
| Asientos de auditoría | 4,000,000 (≈ 40 por misión) |
| Consumos de combustible con comprobante | 250,000 |
| Cruces de peaje | 400,000 |
| Fotografías | 500,000, ≈ 150 GB a 300 KB cada una |
| Posiciones de seguimiento en ruta | 20,000,000 (ver [`RNF-08`](RNF-08-seguimiento-en-ruta.md)) |

**Si el insumo #67 devuelve cifras distintas, `JDR-1` se rehace y los umbrales se remiden.** No se ajusta el umbral para que la implementación pase.

## Métrica y umbral

Medido en el percentil 95 (`p95`), con `JDR-1` cargado y 60 usuarios concurrentes, sobre el hardware de referencia del [`RNF-09`](RNF-09-instalacion-respaldo-y-restauracion.md).

| Operación | Umbral `p95` |
|---|---|
| Búsqueda por folio o por placa | < 1 s |
| Listado de órdenes de misión con filtros (delegación, rango de fechas, estado) | < 2 s |
| Apertura del expediente de un vehículo con 5 años de historial | < 3 s |
| Verificación de bloqueos duros al asignar vehículo↔motorista (licencia, disponibilidad, documentación, compatibilidad) | < 1.5 s |
| Emisión de Orden de Misión con su documento imprimible | < 5 s |
| Tablero de seguimiento en ruta con 30 vehículos activos | < 3 s de carga inicial |
| Reporte de conciliación galonaje–kilometraje de una delegación, un mes | < 15 s |
| Cualquier reporte que exceda 15 s | Se genera **en segundo plano** con aviso al terminar. **Ninguna pantalla se queda colgada sin decir qué pasa** |
| Degradación de `p95` al pasar de 10 a 60 usuarios concurrentes | ≤ 100 % (no más del doble) |
| Consultas sin límite de paginación | **0.** Todo listado pagina |

`[C]` Los 60 concurrentes y los 400 usuarios salen de `JDR-1`, que depende del insumo #67.

## Cómo se verifica

1. **Carga de `JDR-1`**: existe un generador reproducible que puebla una instancia limpia con el juego de datos de referencia, incluidos los 5 años de asientos de auditoría encadenados. La carga es parte del repositorio, no un script de alguien.
2. **Medición individual**: se recorre la tabla de umbrales operación por operación, con la base ya cargada, y se registra `p50`, `p95` y `p99`. Se guarda el resultado fechado para comparar entre versiones.
3. **Prueba de concurrencia**: 60 sesiones simultáneas ejecutando el guion de un día hábil típico (consultar, solicitar, aprobar, despachar, registrar bitácora, liquidar) durante 30 minutos. Se mide degradación contra la corrida de 10 sesiones.
4. **Prueba de la consulta vieja**: se abre el expediente de una misión del primer año de `JDR-1`, no del último. Es la consulta que el auditor hace y la que nadie prueba.
5. **Prueba de regresión**: los umbrales se miden en cada entrega. Un incremento de `p95` superior al 20 % respecto de la entrega anterior es un defecto, aunque siga bajo umbral.

## Consecuencia de no cumplirlo

El Encargado de Transporte deja de consultar el sistema y vuelve a su cuaderno para saber qué vehículo está libre. A partir de ahí el sistema registra a destiempo lo que se decidió fuera de él, y la trazabilidad que exige el control interno queda documentando una ficción ordenada.

El modo de falla concreto es predecible: el sistema funciona bien el primer año, se vuelve lento el tercero, y para entonces rediseñar el modelo de consulta cuesta lo que costó construirlo.

## Trazabilidad

- Módulos: transversal — se mide sobre M-06, M-07, M-08, M-09, M-13, M-14, M-19
- Requisitos relacionados: [`RNF-02`](RNF-02-volumen-y-crecimiento-del-acervo.md), [`RNF-06`](RNF-06-reproducibilidad-historica-de-reportes.md), [`RNF-08`](RNF-08-seguimiento-en-ruta.md), [`RNF-18`](RNF-18-paquetes-de-evidencia-para-auditoria.md)
- Insumos: #67 (volumen operativo cifrado), #71 (plazo de conservación)
- Decide: `ADR` de selección de stack (Sprint 2), `ADR` de estrategia de reportes
