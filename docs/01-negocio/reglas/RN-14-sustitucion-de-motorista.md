# RN-14 — La sustitución de motorista o de vehículo revalida todas las habilitaciones y conserva la asignación original

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-08, M-05, M-03 |
| **Origen** | Decisión [DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) y [NRM-06](../normativa/NRM-06-transito-y-licencias.md) |
| **Verificación** | `[V]` la decisión de producto — `[V]` la exigencia de habilitación |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Nota de corrección — hallazgo `HB1-02`

> **Qué estaba mal.** El caso límite *"sustitución que rompe la segregación de funciones"* degradaba a **advertencia registrada, no bloqueo** el caso en que el motorista entrante es quien autorizó la orden, y lo dejaba `[C]` pendiente de Auditoría Interna. Pero [actores-y-roles §5.2](../actores-y-roles.md) — autoridad en incompatibilidades — declara `I-11` **núcleo irreductible que no se levanta nunca**. Una regla no puede dejar en `[C]` la aplicación de un par que la autoridad declara irreductible.
>
> **Qué se corrigió.** Ese caso límite es ahora bloqueo duro, coherente con `I-11` y con la corrección espejo de [RN-01](RN-01-segregacion-de-funciones.md).

## Enunciado

Cuando se sustituye el motorista o el vehículo de una Orden de Misión, el sistema **debe**:

1. **Revalidar íntegramente** las comprobaciones de habilitación sobre el recurso entrante: matriz licencia ↔ vehículo ([RN-09](RN-09-matriz-licencia-vehiculo.md)), vigencia de licencia en el rango restante ([RN-10](RN-10-licencia-vigente-en-todo-el-rango.md)), restricciones médicas ([RN-11](RN-11-restricciones-medicas-del-motorista.md)), disponibilidad ([RN-12](RN-12-disponibilidad-del-motorista.md)), estado operativo del vehículo ([RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md)), compatibilidad y capacidad ([RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md)) **y segregación de funciones sobre esa misión** ([RN-01](RN-01-segregacion-de-funciones.md), par `I-11`).
2. **Conservar la asignación original** en el expediente, con su rango de vigencia, quién la sustituyó, cuándo y por qué.
3. Registrar el **corte de odómetro y de combustible** en el momento de la sustitución, si el vehículo ya salió.

La sustitución **no debe** implementarse como edición del campo motorista o vehículo de la orden.

## Justificación

[DP-001 D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): *"Cuando un motorista no está disponible, el sistema debe permitir cubrir la misión con otro sin perder la trazabilidad de la asignación original."*

Sin corte de odómetro, la conciliación de [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) atribuye a una sola persona el consumo de dos. Y si la sustitución se hace editando el campo, ante un accidente el expediente dirá que conducía quien no conducía — con consecuencias directas de responsabilidad.

## Condiciones de aplicación

Aplica a sustituciones antes y después del despacho. Después del despacho, la sustitución de vehículo suele implicar también un expediente de incidente o de mantenimiento (M-11, M-12) que debe quedar vinculado.

**No aplica** al relevo planificado de motoristas dentro de una misma orden ([RN-13](RN-13-sin-doble-asignacion.md), turnos), que es un mecanismo distinto y previsto desde la programación — aunque comparte la exigencia de corte de odómetro y traspaso de custodia.

## Comportamiento esperado

1. La sustitución es una acción explícita con motivo tipificado: indisponibilidad del motorista, avería del vehículo, siniestro, decisión operativa.
2. Si el recurso entrante no pasa alguna comprobación, la sustitución se **bloquea** con la misma dureza que la asignación inicial. La urgencia no habilita: [NRM-06](../normativa/NRM-06-transito-y-licencias.md) no admite excepción, y `I-11` tampoco.
3. El expediente muestra la **secuencia de asignaciones** con sus rangos, kilometraje recorrido por cada una y combustible consumido por cada una.
4. Los documentos impresos de la orden emitidos antes de la sustitución quedan **desactualizados**: el sistema los marca y emite versión nueva con folio propio, referenciando la anterior ([RN-04](RN-04-anulacion-como-asiento-reverso.md)).
5. La custodia del vehículo se traspasa con constancia ([RN-22](RN-22-custodia-del-vehiculo.md)); el combustible asignado al motorista saliente se liquida o se traspasa con acta ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)).

## Casos límite

- **Sustitución en carretera sin conectividad.** Se registra en el dispositivo con los datos del entrante; **la revalidación completa no se puede hacer sin el padrón actualizado**. El cliente de campo valida contra la copia local que tenga y marca el resultado como *validación con datos locales de fecha X*. Al sincronizar, si la revalidación falla, la orden se marca para revisión y se cierra con hallazgo. Nunca se permite operar como si la validación hubiera pasado.
- **Vehículo averiado a mitad de ruta y el sustituto lo alcanza horas después.** Hay un intervalo en que la misión no tiene vehículo operativo. Se registran ambos cortes de odómetro, el tiempo de espera en sitio (M-19) y el consumo de cada vehículo por separado.
- **El motorista saliente conserva vales o efectivo de combustible.** No se traspasa informalmente: se liquida contra la asignación original o se reasigna con folio nuevo y constancia de recepción del entrante.
- **Sustitución que rompe la segregación de funciones** — el motorista entrante es quien autorizó, despachó, entregó el fondo o liquidó la orden. **Bloqueo duro** por [RN-01](RN-01-segregacion-de-funciones.md), par `I-11` del **núcleo irreductible**. No hay advertencia con acuse, no hay `[C]` pendiente y no hay urgencia habilitante: quien ejerció una función de control sobre la misión no la conduce. La salida es otro motorista habilitado, o que la función de control la reasuma otro puesto por [RN-02](RN-02-escalamiento-de-autorizacion.md) antes de que el interesado tome el volante. Si no hay ninguna de las dos, la misión no se despacha.
- **Sustitución del vehículo por otro de tipo distinto** que ya no es compatible con la carga o los pasajeros. Bloqueo por [RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) y [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md). La misión se reformula, no se fuerza.
- **Sustitución retroactiva** — el despachador descubre al día siguiente que salió otro motorista. Se registra con fecha del hecho anterior a la fecha de captura ([RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md)), se marca como registro diferido y genera hallazgo por incumplimiento de registro oportuno (TSC-NOGECI V-10).

## Trazabilidad

- Normas: [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Decisión: [DP-001, D-07](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Incompatibilidad: `I-11` de [actores-y-roles §5.2](../actores-y-roles.md) — núcleo irreductible
- Hallazgo que corrige esta regla: `HB1-02` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md)
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-09](RN-09-matriz-licencia-vehiculo.md) a [RN-13](RN-13-sin-doble-asignacion.md), [RN-22](RN-22-custodia-del-vehiculo.md), [RN-31](RN-31-odometro-de-retorno.md)
- Actores: ACT-04, ACT-05, ACT-06, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
