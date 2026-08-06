# RN-12 — No se asigna un motorista con permiso, vacaciones o incapacidad vigente según el espejo de Talento Humano

| Campo | Valor |
|---|---|
| **Módulos** | M-05, M-07, M-20 |
| **Origen** | Decisión [DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) y [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md); norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` la decisión de producto — `[C]` el contrato de API de Talento Humano (insumo #17) |
| **Tipo** | Bloqueo duro |
| **Configurable** | No el bloqueo. Sí el catálogo `tipo_ausencia` y su efecto |

## Enunciado

Antes de asignar un motorista a una Orden de Misión, el sistema **debe** verificar contra el espejo local de Talento Humano que la persona no tenga, en ningún día del rango de la misión, una ausencia registrada de tipo permiso, vacaciones, incapacidad, suspensión o baja.

Si existe ausencia que cubra total o parcialmente el rango, la asignación **se bloquea**.

Si el motorista fue **dado de baja** en Talento Humano y tiene misiones abiertas en SIGTI, el sistema **debe** señalarlo y exigir sustitución antes de continuar.

## Justificación

[ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) lo dice sin rodeos: *"Un motorista que Talento Humano dio de baja pero que SIGTI sigue asignando a misiones no es un problema técnico: es un problema legal."*

Despachar a alguien que está de vacaciones o incapacitado produce dos daños simultáneos: la institución expone a un servidor que no debía estar trabajando, y el registro de asistencia contradice al registro de flota — contradicción que un auditor detecta cruzando dos sistemas.

## Condiciones de aplicación

Aplica a la asignación, al despacho y a la sustitución en ruta.

**No aplica** a ausencias que la institución defina como compatibles con la conducción (por ejemplo, permiso de horas ya cumplido antes de la ventana de la misión). El catálogo `tipo_ausencia` define el efecto de cada tipo.

## Comportamiento esperado

1. La verificación se hace **contra la copia local**, nunca por llamada remota en línea ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)), para que funcione con la red caída y en campo.
2. El bloqueo indica tipo de ausencia y rango, **sin exponer el motivo médico**: *"El motorista <nombre> registra ausencia de tipo <incapacidad> del <fecha> al <fecha>."*
3. El sistema muestra, junto al bloqueo, la **marca de última sincronización** del espejo de Talento Humano, para que el despachador sepa cuán fresco es el dato.
4. El calendario de disponibilidad del padrón de motoristas es consultable por ACT-04 y ACT-05: ausencias, misiones ya asignadas y descansos.
5. Cuando la ausencia se registra **después** de programada la misión, el sistema notifica al despacho y marca la asignación como *en conflicto*, exigiendo resolución antes del despacho.

## Casos límite

- **Ausencia notificada mientras el motorista está en ruta.** No se cancela la misión desde el escritorio: se notifica al Jefe de Transporte y al motorista, se registra el conflicto y se decide operativamente. El expediente conserva la contradicción — es información de auditoría, no un error a esconder.
- **El espejo lleva días sin sincronizar.** Aplica [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md): superado el umbral, se bloquea la asignación con mensaje explícito. Es preferible detener el despacho a asignar contra datos de personal que ya no reflejan la realidad.
- **Motorista que no existe en Talento Humano** — personal por contrato, apoyo de otra institución. `[C]` confirmar si esta figura existe. Si existe, su disponibilidad no se puede verificar por esta vía y debe gestionarse en SIGTI con registro propio, marcado como *disponibilidad no verificada contra Talento Humano*.
- **Ausencia parcial de un solo día dentro de una misión de cinco.** Bloquea igual: la misión requiere al motorista todos los días. Si operativamente el día de ausencia no requiere conducción, se acorta el rango de la misión o se sustituye.
- **Vacaciones aprobadas que el motorista renuncia a tomar** para cubrir una misión. No se resuelve en SIGTI: Talento Humano debe reflejar la reprogramación. SIGTI **no edita el espejo** ([RN-48](RN-48-datos-espejo-de-solo-lectura.md)).
- **Baja del motorista con misión en curso.** Se exige sustitución y se registra el hecho; la bitácora y el consumo ya registrados **permanecen a su nombre**. Reasignar retroactivamente la ejecución a otra persona sería falsear el registro.

## Trazabilidad

- Decisiones: [DP-001, D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-13](RN-13-sin-doble-asignacion.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-49](RN-49-reconciliacion-periodica-del-espejo.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)
- Actores: ACT-04, ACT-05, ACT-06
- Historias y casos especiales: pendientes — Bloque 2
