# RNF-03 — El cliente de campo opera sin conectividad durante toda una misión

| Campo | Valor |
|---|---|
| **Categoría** | Disponibilidad / Operabilidad |
| **Prioridad** | Crítico |
| **Origen** | Realidad de conectividad rural hondureña — ver [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) |
| **Afecta arquitectura** | **Sí** — es determinante para el `ADR` de selección de stack |

## Enunciado

El cliente de campo **debe** permitir al motorista y al encargado de delegación registrar la operación completa de una misión —salida, odómetros, paradas, arribos, entregas, consumo de combustible, incidentes y fotografías— **sin ninguna conectividad**, durante al menos **7 días continuos**, y sincronizar sin pérdida de datos al reconectar.

Esto es **offline-first**, no "soporte offline": la ausencia de red es el estado normal esperado en operación de campo, no una degradación.

**Por qué 7 días y no 2:** una misión a Gracias a Dios, Olancho o la Mosquitia no dura una tarde. El umbral no sale de una costumbre de la industria — sale de la duración real de la misión más larga que la institución ejecuta, y del hecho `[V]` de que más de 2 millones de personas del área rural hondureña no tienen acceso a internet (INE, EPHPM julio 2025). `[C]` La duración máxima real de misión está sujeta al insumo #67; si resulta mayor a 7 días, este umbral sube.

## Métrica y umbral

| Métrica | Umbral |
|---|---|
| Duración máxima sin conectividad conservando plena funcionalidad de captura | ≥ 7 días |
| Órdenes de misión almacenables localmente sin sincronizar | ≥ 20 |
| Fotografías almacenables localmente | ≥ 200, con compresión automática |
| Pérdida de datos tras sincronización | **0 registros. Sin excepción.** |
| Sobrescritura silenciosa de un dato en conflicto | **0. Todo conflicto va a cola de resolución humana.** |
| Tiempo de sincronización de una misión completa con 20 fotos, en 3G | < 3 minutos |
| Funciones que requieren conectividad obligatoria | Solo autorización de nuevas solicitudes y consulta de datos que no estaban precargados |

## Cómo se verifica

1. **Prueba de campo simulada**: se configura un dispositivo con una misión de 5 días y 4 destinos, se pone en modo avión, se registra la operación completa incluidas 30 fotos, y se reconecta. Se compara registro a registro contra lo capturado.
2. **Prueba de conflicto**: el mismo vehículo recibe registros desde dos dispositivos distintos sin conexión entre sí. Se verifica que ambos lleguen al servidor, que ninguno se pierda, y que el conflicto aparezca en la cola de resolución con ambas versiones visibles.
3. **Prueba de interrupción**: se corta la red a mitad de una sincronización. Se verifica que al reintentar no se dupliquen registros ni se pierdan los que ya habían subido.
4. **Prueba de volumen**: 20 misiones acumuladas sin sincronizar; se mide tiempo y consumo de almacenamiento.

## Consecuencia de no cumplirlo

El sistema no se usa en campo. El motorista vuelve al papel, el encargado de delegación digita a destiempo desde memoria, y el registro pierde el valor probatorio que exige la norma TSC-NOGECI V-10 sobre registro oportuno — que es precisamente la razón de existir del sistema.

Este es el requisito que más probabilidades tiene de decidir el fracaso del proyecto si se subestima.

## Trazabilidad

- Módulos: M-08, M-16
- Reglas: [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md), [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md)
- Historias: pendientes del Bloque 3
- Casos especiales: [`CE-09`](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) (bitácora en papel por falta de señal), [`CE-02`](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) (avería en ruta sin señal)
- Requisitos relacionados: [`RNF-12`](RNF-12-uso-en-campo.md), [`RNF-21`](RNF-21-integridad-de-folios-y-correlativos.md), [`RNF-13`](RNF-13-cifrado-en-transito-y-en-reposo.md)
- Decide: `ADR` de stack, `ADR` de estrategia de sincronización, `ADR` de identificadores generados en cliente

### Nota sobre la trazabilidad de la plantilla

Este requisito se reutiliza **tal cual** del ejemplo de [`docs/plantillas/requisito-no-funcional.md`](../../plantillas/requisito-no-funcional.md). Se corrigieron únicamente tres referencias que quedaron desactualizadas en la plantilla, sin tocar enunciado, métricas ni verificación:

| En la plantilla | Corregido a | Motivo |
|---|---|---|
| `CE-18` = bitácora en papel por falta de señal | `CE-09` | `CE-18` es *carga y pasajeros en la misma misión*. Ver el [índice de casos especiales](../casos-especiales/README.md) |
| `CE-07` = avería en ruta sin señal | `CE-02` | `CE-07` es *retorno anticipado* |
| `ADR-001` = selección de stack | `ADR` de stack, sin número | `ADR-001` ya está tomado por la integración con ARGOS y Talento Humano; el stack está diferido por [`ADR-000`](../../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md) y su ADR aún no tiene número asignado |

**La plantilla debe corregirse.** Queda como hallazgo para `docs/05-calidad/hallazgos/`.
