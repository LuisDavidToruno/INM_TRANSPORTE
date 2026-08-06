# RN-48 — Los datos provenientes de ARGOS y Talento Humano son espejo de solo lectura y no se editan desde SIGTI

| Campo | Valor |
|---|---|
| **Módulos** | M-20, M-01, M-05 |
| **Origen** | Decisión [DP-001 D-05](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) y [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) |
| **Verificación** | `[V]` la decisión de producto y la decisión de arquitectura |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Los datos cuyo dueño es ARGOS o Talento Humano **deben** almacenarse en SIGTI como **espejo local marcado como tal**, y **ninguna pantalla ni operación de SIGTI debe permitir editarlos**.

| Dato | Dueño | En SIGTI |
|---|---|---|
| Expediente del empleado, licencias, permisos, vacaciones, feriados | Talento Humano | Espejo |
| Niveles de autorización y jerarquía | ARGOS | Espejo |
| Estructura presupuestaria | ARGOS | Espejo |
| Viáticos de una misión | ARGOS | Solo la clave de vínculo |
| Vehículo, motorista como recurso de flota, solicitudes, misiones, bitácoras, combustible, peajes, mantenimiento, incidentes | **SIGTI** | Propio |

SIGTI **sí puede** mantener datos **propios** sobre una entidad espejeada — historial de conducción, incidentes al volante, vehículos habilitados — siempre que queden claramente separados del espejo.

Toda operación de SIGTI **debe** ejecutarse contra la copia local, **nunca** contra una llamada remota en línea.

## Justificación

[ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) lo decide y lo fundamenta: consultar en línea en cada operación es *"imposible en campo sin red"* y acopla la disponibilidad de SIGTI a dos sistemas ajenos.

La distinción de propiedad es la que evita el peor resultado: dos sistemas afirmando cosas distintas sobre la misma persona. *"El empleado pertenece a Talento Humano; su rol como motorista dentro de la flota pertenece a SIGTI."*

Si SIGTI pudiera editar el espejo, la próxima sincronización revertiría el cambio y el usuario aprendería que el sistema "borra lo que uno escribe" — o peor, la edición sobreviviría y las dos fuentes divergirían en silencio.

## Condiciones de aplicación

Aplica a todo dato del catálogo de entidades espejeadas.

`[C]` El alcance definitivo depende de los contratos de API de ARGOS (insumo #16) y Talento Humano (insumo #17), aún no disponibles.

## Comportamiento esperado

1. Los campos espejo se presentan en solo lectura, **identificados como provenientes del sistema origen**, con su marca de última sincronización visible.
2. Cuando un dato espejo está mal, el sistema ofrece **enlace o instrucción para corregirlo en el origen**, no un campo editable. Un dato erróneo que no se puede corregir en ninguna parte es peor que uno de solo lectura.
3. Los datos propios de SIGTI sobre una entidad espejeada se almacenan por separado y **sobreviven** a cualquier resincronización.
4. Ninguna operación crítica depende de una llamada remota: si ARGOS o Talento Humano están caídos, SIGTI opera ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)).
5. La bitácora de sincronización registra qué cambió, cuándo llegó y de qué evento vino.

## Casos límite

- **Motorista que existe en la operación pero no en Talento Humano** — personal por contrato, apoyo de otra institución. `[C]` confirmar si existe la figura. De existir, se registra como entidad **propia de SIGTI** marcada como *no espejeada*, con la advertencia de que su disponibilidad no se verifica contra Talento Humano ([RN-12](RN-12-disponibilidad-del-motorista.md)).
- **Empleado dado de baja con misiones abiertas en SIGTI.** `[C]` pendiente explícito de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md). El espejo refleja la baja; SIGTI **no borra** las misiones ni reasigna la ejecución pasada. Exige sustitución para lo futuro ([RN-14](RN-14-sustitucion-de-motorista.md)).
- **Dato espejo que llega vacío o incompleto** desde el origen. No se completa localmente. Se marca como incompleto y bloquea las operaciones que lo requieren, indicando el sistema responsable.
- **Corrección urgente en día no laborable del sistema origen.** No hay atajo: la corrección se hace en el origen. Si eso paraliza una operación crítica, la salida es registrar la decisión operativa con su fundamento y cerrar con hallazgo, no editar el espejo.
- **ARGOS y Talento Humano con datos contradictorios sobre la misma persona.** SIGTI no arbitra: refleja ambos, marca la contradicción y la eleva. Elegir uno sería asumir una autoridad que no tiene.
- **Carga inicial antes de que existan los webhooks.** [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) prevé que los sistemas origen podrían no emitir eventos. En ese caso el mecanismo principal pasa a reconciliación periódica más frecuente ([RN-49](RN-49-reconciliacion-periodica-del-espejo.md)), con la ventana de desactualización documentada y aceptada por el PO.

## Trazabilidad

- Decisiones: [DP-001, D-05 y D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- Reglas relacionadas: [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-02](RN-02-escalamiento-de-autorizacion.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)
- Actores: ACT-01, ACT-04, ACT-05
- Historias y casos especiales: pendientes — Bloque 2
