# RN-48 — Los datos provenientes de ARGOS y Talento Humano son espejo de solo lectura y no se editan desde SIGTI

| Campo | Valor |
|---|---|
| **Módulos** | M-20, M-01, M-05 |
| **Origen** | Decisión [DP-001 D-05](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) y [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — **artefacto autoridad en fronteras entre sistemas** |
| **Verificación** | Sin nivel normativo: la frontera entre sistemas es **decisión** de producto y de arquitectura, y una decisión no se verifica, se cita por su identificador (`HN1-20`). `[C]` el alcance definitivo del conjunto espejeado depende de los contratos de API de ARGOS (insumo #16) y de Talento Humano (insumo #17) |
| **Tipo** | Bloqueo duro |
| **Configurable** | No el bloqueo de edición. **Sí el catálogo de entidades espejeadas**, que se ajusta cuando el `ADR-001` mueve la propiedad de un dato |

## Nota de corrección — hallazgo `HB1-13`

> **Qué estaba mal.** La tabla de esta regla listaba *"Expediente del empleado, **licencias**, permisos, vacaciones, feriados"* como espejo de Talento Humano — la versión **vieja** de la tabla de `ADR-001`. Como esta regla es **bloqueo duro no configurable** (*"ninguna pantalla ni operación de SIGTI debe permitir editarlos"*), implementada literalmente **impedía capturar la licencia de conducir dentro de SIGTI**, y con ella se quedaban sin fuente de datos `BD-02`, [RN-09](RN-09-matriz-licencia-vehiculo.md) y [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md) — el bloqueo de mayor valor legal del sistema.
>
> **Qué manda.** [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) fue corregido el 2026-08-06 con una sección titulada *"La licencia de conducir es dato **PROPIO** de SIGTI, no espejo"*, y su razón es la que sostiene esta corrección: *"Si Talento Humano no registra la categoría con ese nivel de detalle... **el bloqueo no se puede sostener sobre el espejo**. Un control de esta criticidad legal no puede depender del modelo de datos de un sistema ajeno que no tiene motivo para mantenerlo."* Por la precedencia entre artefactos de `CLAUDE.md`, el `ADR-xxx` vigente es la autoridad en fronteras entre sistemas. Se corrige esta regla.
>
> **Consecuencia operativa, dicha de frente** (también del `ADR-001`): alguien de la institución tiene que **capturar y mantener las licencias dentro de SIGTI**, con su alerta de vencimiento. Es trabajo adicional real, y es el precio de que el bloqueo sea defendible ante un siniestro.
>
> **Nota de hallazgo abierta, no resuelta aquí.** `BD-02` de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md), apartado *"Dependencia del espejo"*, sigue afirmando que *"los datos de licencia vienen de Talento Humano (`ADR-001`)"* — atribuye al `ADR-001` lo contrario de lo que dice. Y `PR-01` §6 mantiene la frontera como `[C]` sin resolver. Ninguno de los dos está en esta carpeta; quedan señalados.

## Enunciado

Los datos cuyo dueño es ARGOS o Talento Humano **deben** almacenarse en SIGTI como **espejo local marcado como tal**, y **ninguna pantalla ni operación de SIGTI debe permitir editarlos**.

| Dato | Dueño | En SIGTI |
|---|---|---|
| Identidad del empleado, puesto, dependencia, alta y baja | Talento Humano | Espejo |
| Permisos, vacaciones, incapacidades, calendario de feriados | Talento Humano | Espejo |
| Niveles de autorización y jerarquía | ARGOS | Espejo |
| Estructura presupuestaria y cuota trimestral de compromiso | ARGOS | Espejo |
| Viáticos de una misión | ARGOS | Solo la clave de vínculo |
| **Licencia de conducir: número, categorías, vigencia, restricciones médicas, escaneo del documento** | **SIGTI** | **Propio — se captura y se mantiene dentro de SIGTI** |
| Habilitación por tipo de vehículo, historial de conducción e incidentes al volante | **SIGTI** | Propio |
| Vehículo, motorista como recurso de flota, solicitudes, misiones, bitácoras, combustible, peajes, mantenimiento, incidentes | **SIGTI** | Propio |

**La licencia de conducir queda expresamente excluida del conjunto espejeado.** El bloqueo de edición de esta regla **no la alcanza**: M-05 debe ofrecer pantalla de captura y mantenimiento de licencias, con su expediente, su escaneo adjunto y su alerta de vencimiento ([RN-17](RN-17-alertas-de-vencimiento-documental.md)).

`[C]` Si al obtener el contrato de API de Talento Humano (insumo #17) resulta que **sí** mantiene la categoría de licencia con el detalle que exige [NRM-06](../normativa/NRM-06-transito-y-licencias.md), la propiedad se puede reconsiderar en el `ADR-001` y esta tabla se ajusta. Hasta entonces, es dato propio.

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

- **Licencia de conducir mal capturada en SIGTI.** No es un dato espejo: se corrige **dentro** de SIGTI, por el rol facultado de M-05, con asiento de corrección ([RN-04](RN-04-anulacion-como-asiento-reverso.md)) y adjunto del documento. No hay origen externo al cual remitir al usuario.
- **Talento Humano que también guarda un número de licencia.** Puede ocurrir y no es contradicción: el de Talento Humano es un dato administrativo del expediente laboral; el de SIGTI es el que sostiene el bloqueo de asignación. Si difieren, el sistema **marca la divergencia y la eleva** ([RN-49](RN-49-reconciliacion-periodica-del-espejo.md)); no sobrescribe el dato propio con el espejeado.
- **Motorista que existe en la operación pero no en Talento Humano** — personal por contrato, apoyo de otra institución. `[C]` confirmar si existe la figura. De existir, se registra como entidad **propia de SIGTI** marcada como *no espejeada*, con la advertencia de que su disponibilidad no se verifica contra Talento Humano ([RN-12](RN-12-disponibilidad-del-motorista.md)).
- **Empleado dado de baja con misiones abiertas en SIGTI.** `[C]` pendiente explícito de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md). El espejo refleja la baja; SIGTI **no borra** las misiones ni reasigna la ejecución pasada. Exige sustitución para lo futuro ([RN-14](RN-14-sustitucion-de-motorista.md)).
- **Dato espejo que llega vacío o incompleto** desde el origen. No se completa localmente. Se marca como incompleto y bloquea las operaciones que lo requieren, indicando el sistema responsable.
- **Corrección urgente en día no laborable del sistema origen.** No hay atajo: la corrección se hace en el origen. Si eso paraliza una operación crítica, la salida es registrar la decisión operativa con su fundamento y cerrar con hallazgo, no editar el espejo.
- **ARGOS y Talento Humano con datos contradictorios sobre la misma persona.** SIGTI no arbitra: refleja ambos, marca la contradicción y la eleva. Elegir uno sería asumir una autoridad que no tiene.
- **Carga inicial antes de que existan los webhooks.** [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) prevé que los sistemas origen podrían no emitir eventos. En ese caso el mecanismo principal pasa a reconciliación periódica más frecuente ([RN-49](RN-49-reconciliacion-periodica-del-espejo.md)), con la ventana de desactualización documentada y aceptada por el PO.

## Trazabilidad

- Decisiones: [DP-001, D-05 y D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), sección *"La licencia de conducir es dato PROPIO de SIGTI, no espejo"*
- Norma que exige el dato propio de licencia: [NRM-06](../normativa/NRM-06-transito-y-licencias.md)
- Hallazgo que corrige esta regla: `HB1-13` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md)
- Reglas relacionadas: [RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-11](RN-11-restricciones-medicas-del-motorista.md), [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-02](RN-02-escalamiento-de-autorizacion.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)
- Actores: ACT-01, ACT-04, ACT-05, ACT-17
- Historias y casos especiales: pendientes — Bloque 2
