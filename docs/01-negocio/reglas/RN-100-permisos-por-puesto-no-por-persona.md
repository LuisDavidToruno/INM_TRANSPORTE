# RN-100 — Los permisos se conceden al puesto, nunca a la persona; la autoría histórica es de la persona y no se reasigna

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-14, M-03, M-07 |
| **Origen** | Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md) — implicación de requerimiento: *«roles y permisos por puesto, no por persona»*. Diseño de [actores-y-roles.md §2](../actores-y-roles.md), **artefacto autoridad en actores y alcance de datos**. Regla candidata 1 de su §8. Hallazgo `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) |
| **Verificación** | `[V]` que Honduras celebró elecciones generales en noviembre de 2025 con cambio de gobierno en enero de 2026, y que la rotación de personal en el sector público es alta tras un cambio de administración — [NRM-09](../normativa/NRM-09-realidad-operativa.md). `[I]` que la respuesta correcta sea modelar el permiso sobre el puesto: es diseño de control interno del equipo, no articulado citable |
| **Tipo** | Bloqueo duro — no existe la concesión directa a una persona |
| **Configurable** | No |

## Por qué existe esta regla — hallazgo `HN1-18`

El modelo está escrito con detalle en [actores-y-roles.md §2](../actores-y-roles.md) y **ninguna regla lo obligaba**. Es el mecanismo que absorbe la rotación, y la rotación es el evento que la propia [NRM-09](../normativa/NRM-09-realidad-operativa.md) señala como más frecuente y más dañino si el sistema no lo previó.

## Enunciado

**El permiso se concede a un puesto. Una persona ejerce un permiso porque ocupa un puesto que lo tiene, y por ninguna otra vía.**

No existe la concesión de permiso a un usuario nominal. Un usuario sin asignación de puesto vigente **no tiene ningún permiso**, aunque exista, esté activo y tenga contraseña.

Y su recíproco, que es la mitad que se suele olvidar:

**La autoría de un asiento es de la persona, y no se reasigna jamás.** Todo acto queda registrado con **la persona y el puesto que ocupaba en ese momento**, ambos congelados en el asiento.

## Por qué se guardan los dos

Cuando el auditor pregunta *«¿quién autorizó esto y con qué competencia?»*, el nombre solo no responde. **La competencia estaba en el puesto**, y el puesto pudo haber cambiado de manos tres veces desde entonces. Guardar solo la persona deja el acto sin fundamento; guardar solo el puesto deja el acto sin responsable.

## Justificación

Cuando el permiso cuelga de la persona, cada rotación obliga a reconstruir a mano quién puede hacer qué — y en una institución que rota tras cada cambio de administración eso significa que **el sistema de permisos se degrada solo**. Lo que ocurre en la práctica es conocido: se copian los permisos del saliente al entrante *«para que pueda trabajar»*, y con ellos se copia toda la acumulación indebida que el saliente había juntado en años. La segregación de funciones de [`RN-01`](RN-01-segregacion-de-funciones.md) se pierde sin que nadie tome la decisión de perderla.

Con el permiso en el puesto, el alta del entrante es un solo acto —**ocupa el puesto**— y hereda exactamente la competencia que ese puesto tiene definida, ni más ni menos.

## Condiciones de aplicación

Aplica a todo permiso del sistema, incluidos los de consulta.

**No aplica** a `ACT-01` Administrador del Sistema en lo que hace a la administración técnica, que igualmente **no ejecuta transacciones de negocio** ni puede alterar la pista de auditoría — [actores-y-roles.md](../actores-y-roles.md), `I-13`.

## Comportamiento esperado

1. Los permisos efectivos de una persona se calculan **a la fecha del hecho**, no a la fecha de consulta ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)). Un acto de marzo se juzga con la ocupación de puesto vigente en marzo.
2. La asignación de puesto tiene **vigencia con fecha de inicio y fin**. Una asignación sin fin es indefinida, no eterna: se cierra cuando la persona deja el puesto.
3. **Coocupación**: dos personas pueden ocupar el mismo puesto simultáneamente durante un traspaso. El solape es acotado y se registra; ambas tienen los permisos del puesto y **cada acto queda a nombre de quien lo hizo**.
4. Un puesto **vacante** conserva sus pendientes, que quedan atribuidos al puesto y visibles para quien lo ocupe. Ver [`RN-101`](RN-101-cierre-de-asignacion-de-puesto.md).
5. La segregación de funciones de [`RN-01`](RN-01-segregacion-de-funciones.md) **se evalúa sobre la identidad de la persona, no sobre el puesto**. Una persona que ocupa dos puestos compatibles en el organigrama sigue sin poder ejercer dos funciones incompatibles sobre la misma misión.

## Casos límite

- **La persona deja el puesto con actos suyos en curso.** Su autoría no se toca. Los pendientes de decisión pasan al puesto. Ver [`RN-101`](RN-101-cierre-de-asignacion-de-puesto.md).
- **El puesto se suprime o se fusiona en una reestructuración.** Los asientos históricos conservan el puesto tal como se llamaba entonces. **Un puesto suprimido no se borra del catálogo**: se cierra con vigencia, porque los actos que autorizó siguen existiendo y tienen que poder explicarse.
- **Una persona ocupa dos puestos a la vez** — frecuente en delegaciones. Sus permisos son la unión de ambos, y `RN-01` sigue bloqueando por identidad. La acumulación de puestos no es una vía para levantar incompatibilidades.
- **El entrante necesita ver lo que hizo el saliente.** Lo ve: los pendientes son del puesto. Lo que **no** obtiene es la autoría de aquello, que sigue siendo del saliente.
- **`[C]` La estructura de puestos es propiedad de ARGOS y de Talento Humano** ([`DP-001`](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). SIGTI consume el espejo y **no crea puestos**. Si el espejo está desactualizado, aplica la degradación de [`RN-50`](RN-50-degradacion-por-sincronizacion-detenida.md).

## Trazabilidad

- **Norma**: [NRM-09](../normativa/NRM-09-realidad-operativa.md) — implicación de requerimiento, `[I]`; la rotación en sí, `[V]`
- **Hallazgo que la origina**: `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- **Autoridad del diseño**: [actores-y-roles.md §2](../actores-y-roles.md)
- **Reglas relacionadas**: [`RN-01`](RN-01-segregacion-de-funciones.md) · [`RN-07`](RN-07-delegacion-de-autorizacion.md) · [`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-48`](RN-48-datos-espejo-de-solo-lectura.md) · [`RN-50`](RN-50-degradacion-por-sincronizacion-detenida.md) · [`RN-101`](RN-101-cierre-de-asignacion-de-puesto.md)
- **Módulo principal**: M-01 Organización y Seguridad
