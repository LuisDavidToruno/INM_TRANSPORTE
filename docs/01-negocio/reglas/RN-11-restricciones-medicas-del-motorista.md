# RN-11 — Las restricciones médicas de la licencia deben ser compatibles con las condiciones de la misión

| Campo | Valor |
|---|---|
| **Módulos** | M-05, M-07 |
| **Origen** | Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — expediente del motorista con restricciones médicas |
| **Verificación** | `[P]` la existencia de restricciones anotadas en la licencia — `[C]` el catálogo oficial de restricciones de la DNVT |
| **Tipo** | Bloqueo duro para restricciones marcadas como incompatibilizantes; advertencia para el resto |
| **Configurable** | Sí — catálogo `restriccion_medica` con efecto (bloqueo / advertencia) y vigencia |

## Enunciado

Toda restricción médica anotada en la licencia del motorista, o registrada en su expediente por dictamen, **debe** evaluarse contra las condiciones declaradas de la misión — horario nocturno, duración continua, tipo de vehículo, terreno.

Si la restricción está marcada en el catálogo como **incompatibilizante** para esa condición, el sistema **debe bloquear** la asignación. Si no lo está, **debe advertir** al despachador y registrar que la advertencia fue vista y por quién.

## Justificación

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) exige que el expediente del motorista incluya *restricciones médicas*. Registrar un dato y no usarlo en la decisión es peor que no tenerlo: ante un siniestro nocturno de un motorista con restricción de conducción diurna, el expediente prueba que la institución **sabía**.

La distinción bloqueo/advertencia existe porque las restricciones no son homogéneas: "usar lentes correctores" no se puede verificar por sistema y no debe bloquear; "conducción diurna únicamente" sí es contrastable contra la ventana horaria de la misión.

## Condiciones de aplicación

Aplica en la programación, en el despacho y en la sustitución en ruta.

**No aplica** cuando la misión no declara condiciones evaluables: si no hay ventana horaria definida, no se puede evaluar una restricción de conducción diurna. En ese caso el sistema **exige declarar la ventana horaria** antes de programar, en lugar de omitir la evaluación.

## Comportamiento esperado

1. El catálogo de restricciones define, por cada una: código, descripción, condición de misión que evalúa, y efecto (bloqueo o advertencia).
2. El bloqueo identifica la restricción y la condición que la activa: *"El motorista tiene restricción <conducción diurna>. La misión prevé circulación entre las 19:00 y las 23:00 del <fecha>. Asignación bloqueada (RN-11)."*
3. La advertencia **no se puede cerrar sin acuse**: queda registrado quién la vio, cuándo y con qué justificación decidió continuar. Ese registro es parte del expediente y se muestra en la liquidación.
4. Las restricciones se tratan como **dato de salud**: acceso restringido por necesidad de conocer, con registro de consultas ([RN-52](RN-52-registro-de-consultas-a-manifiestos.md) y [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md)). El despachador ve *que existe una restricción operativa aplicable*, no el diagnóstico.
5. Las restricciones con vigencia — dictamen temporal tras una lesión — se evalúan contra el rango de la misión igual que la licencia ([RN-10](RN-10-licencia-vigente-en-todo-el-rango.md)).

## Casos límite

- **Catálogo oficial de restricciones no disponible.** `[C]` No existe fuente verificada del catálogo de la DNVT. Hasta obtenerlo, la institución carga las restricciones que aparecen en las licencias de su padrón, marcando su efecto. **No se inventan códigos.**
- **Restricción registrada como texto libre en la licencia escaneada.** No es evaluable automáticamente. Se registra y produce advertencia genérica al asignar, hasta que alguien la clasifique en el catálogo. Nunca se ignora por no estar tipificada.
- **Incapacidad médica vigente** informada por Talento Humano. No es una restricción de licencia: es indisponibilidad, y bloquea por [RN-12](RN-12-disponibilidad-del-motorista.md). Las dos reglas pueden activarse a la vez y ambas deben mostrarse.
- **La misión se convierte en nocturna sobre la marcha** por retraso. La restricción incompatibilizante no puede aplicarse retroactivamente para bloquear un vehículo que ya está en ruta. Se registra el evento como novedad de M-08, se notifica al Jefe de Transporte y se resuelve operativamente — pernocta o sustitución. La orden se liquida con esa observación.
- **Restricción que exige acompañante o adaptación del vehículo.** Es una condición del vehículo, no solo del motorista: se evalúa contra la ficha técnica. `[C]` confirmar si la institución tiene vehículos adaptados.
- **Motorista que aporta dictamen que levanta la restricción.** Solo surte efecto cuando el dato queda actualizado en el expediente con adjunto. La palabra del motorista no modifica la evaluación.

## Trazabilidad

- Normas: [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md)
- Reglas relacionadas: [RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-10](RN-10-licencia-vigente-en-todo-el-rango.md), [RN-12](RN-12-disponibilidad-del-motorista.md), [RN-52](RN-52-registro-de-consultas-a-manifiestos.md)
- Actores: ACT-04, ACT-05, ACT-06
- Historias y casos especiales: pendientes — Bloque 2
