# ADR-001 — Integración con ARGOS y Talento Humano por espejo local con webhooks

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-06 |
| **Decide** | Product Owner |
| **Sprint** | 0 |

## Contexto

SIGTI no vive solo. La institución ya opera dos sistemas que poseen datos que SIGTI necesita:

- **ARGOS** — viáticos, estructura presupuestaria, niveles de autorización, componente de mapas. Administrado por el mismo PO.
- **Talento Humano** — expediente del empleado, licencias, permisos, vacaciones, incapacidades, calendario de feriados.

El principio del PO es explícito: **no replicamos lo que otro sistema ya hace** ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).

Pero SIGTI tiene una restricción que complica la integración ingenua: **debe operar en campo sin conectividad durante días**. Un diseño que consulte a ARGOS o a Talento Humano en cada operación no es viable — y ni siquiera en la oficina sería sensato, porque acopla la disponibilidad de SIGTI a la de dos sistemas ajenos.

Además, estos datos **cambian poco**: un motorista no cambia de categoría de licencia todas las semanas, ni la estructura de autorizaciones se reorganiza a diario.

## Decisión

**Espejo local sincronizado por eventos.**

1. **Carga inicial** completa por API al poner en marcha la integración.
2. **Copia local** en la base de datos de SIGTI, marcada como *espejo*: **de solo lectura desde SIGTI**. Ninguna pantalla del sistema permite editarla.
3. **Webhooks** del sistema origen notifican los cambios cuando ocurren, y SIGTI los propaga a su copia.
4. Toda operación de SIGTI trabaja **contra la copia local**, nunca contra una llamada remota en línea.

### Qué dato pertenece a quién

| Dato | Dueño | En SIGTI |
|---|---|---|
| Expediente del empleado, licencias, permisos, vacaciones, feriados | Talento Humano | Espejo |
| Niveles de autorización y jerarquía | ARGOS | Espejo |
| Estructura presupuestaria | ARGOS | Espejo |
| Viáticos de una misión | ARGOS | Solo la clave de vínculo |
| Vehículo, motorista-como-recurso-de-flota, solicitudes, misiones, bitácoras, combustible, peajes, mantenimiento, incidentes | **SIGTI** | Propio |

La distinción importa: **el empleado pertenece a Talento Humano; su rol como motorista dentro de la flota pertenece a SIGTI.** Un empleado espejeado puede tener, en SIGTI, datos propios que Talento Humano no conoce — historial de conducción, incidentes al volante, vehículos que tiene habilitados.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| Consulta en línea en cada operación | Siempre el dato más fresco; sin lógica de sincronización | Imposible en campo sin red. Acopla la disponibilidad de SIGTI a dos sistemas ajenos. Latencia en cada pantalla | Incompatible con el requisito de operación desconectada |
| Sincronización periódica programada (cada N horas) | Simple de implementar; no requiere que el origen emita eventos | Ventana de desactualización garantizada. Un permiso aprobado hoy podría no verse hasta mañana, y se asignaría a un motorista no disponible | Aceptable como respaldo, no como mecanismo principal |
| Base de datos compartida entre sistemas | Cero latencia, cero sincronización | Acopla los esquemas: un cambio en ARGOS rompe SIGTI. Anula la frontera entre sistemas | Descartado por acoplamiento |
| Replicar los datos y dejar de sincronizar | Trivial | Divergencia inmediata y silenciosa. Es exactamente lo que produce los hallazgos de auditoría | Descartado |

## Consecuencias

**Positivas**

- SIGTI opera aunque ARGOS o Talento Humano estén caídos
- El requisito de operación desconectada se cumple sin excepciones para datos externos
- La frontera entre sistemas queda explícita y auditable
- Sin duplicación de funcionalidad: cada sistema mantiene lo suyo

**Negativas — y hay que enfrentarlas, no minimizarlas**

- **Los webhooks se pierden.** Una caída de red, un reinicio, un despliegue del origen, y un evento no llega. Si el único mecanismo es el webhook, el espejo diverge en silencio, que es la peor forma de fallar.
- **Divergencia silenciosa es el riesgo real** de este patrón. Un motorista que Talento Humano dio de baja pero que SIGTI sigue asignando a misiones no es un problema técnico: es un problema legal.
- Depende de que ARGOS y Talento Humano **emitan** webhooks. Si no lo hacen hoy, hay que construirlo del lado de ellos.
- Cada dato espejeado hay que decidir si es de solo lectura o si SIGTI puede complementarlo, y esa decisión hay que sostenerla en el tiempo.

**Mitigaciones obligatorias**

Estas no son opcionales; sin ellas la decisión es imprudente:

1. **Reconciliación periódica completa** — una comparación total contra el origen, programada (por ejemplo diaria de madrugada), que detecte y corrija divergencias. El webhook es el camino rápido; la reconciliación es la red de seguridad.
2. **Marca de última sincronización visible** por entidad espejeada. Si un dato lleva demasiado tiempo sin confirmarse, el sistema lo señala en pantalla en lugar de fingir que está al día.
3. **Cola de eventos con reintento** y registro de los que fallaron, revisable por un administrador.
4. **Bitácora de sincronización**: qué cambió, cuándo llegó, de qué evento vino. Sin esto, depurar una divergencia es imposible.
5. **Degradación explícita**: si la sincronización está detenida más allá de un umbral, el sistema advierte antes de permitir operaciones sensibles — como asignar un motorista.

## Pendiente antes de implementar

- `[C]` Contrato de API y catálogo de eventos de ARGOS — insumo #16
- `[C]` Contrato de API de Talento Humano — insumo #17
- `[C]` ¿Ambos sistemas emiten webhooks hoy, o hay que construirlos?
- `[C]` Autenticación entre sistemas: qué esquema usan ARGOS y Talento Humano
- `[C]` Qué ocurre con un empleado dado de baja que tiene misiones abiertas en SIGTI

## Revisión

Se reconsidera si alguno de los sistemas origen no puede emitir eventos — en ese caso el mecanismo principal pasa a ser reconciliación periódica más frecuente, con la ventana de desactualización documentada y aceptada explícitamente por el PO.

## Relación con otras decisiones

- [ADR-000](ADR-000-diferir-seleccion-de-stack.md) — esta decisión es de **integración**, no de stack. Es agnóstica a la tecnología y sigue vigente cualquiera sea la elección del Sprint 2.
- [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — define qué sistema posee qué.
