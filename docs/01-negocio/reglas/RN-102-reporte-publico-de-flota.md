# RN-102 — El reporte público de flota se produce sin depuración manual, agregado o anonimizado

| Campo | Valor |
|---|---|
| **Módulos** | M-14, M-17, M-03 |
| **Origen** | Norma [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md) — implicación de requerimiento: *«reporte público de flota y viajes agregado o anonimizado, listo para publicar en el Portal Único de Transparencia sin trabajo manual de depuración»*. Hallazgo `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) |
| **Verificación** | `[V]` que la **LTAIP, Decreto No. 170-2006**, y su Reglamento están vigentes, y que el **Portal Único de Transparencia** del IAIP opera — [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md). `[P]` que la información de flota esté entre la de publicación de oficio: por observación directa del Portal, varias instituciones publican reglamentos de vehículos institucionales. `[C]` **el artículo y numeral exactos de información de oficio aplicables a flota** — debe leerse el articulado con el OIP institucional. `[I]` que la salida deba ser automática y sin depuración manual: es implicación de requerimiento del equipo |
| **Tipo** | Capacidad obligatoria del sistema — no es bloqueo de transición |
| **Configurable** | Sí — composición y periodicidad del reporte, con vigencia |

## Por qué existe esta regla — hallazgo `HN1-18`

[`RN-51`](RN-51-minimizacion-de-datos-de-personas-externas.md) crea la **separación estructural** entre dato de gestión pública y dato personal que hace posible este reporte. **Nadie obligaba a producirlo.** Una capacidad que existe y que nadie tiene que usar es una capacidad que no se construye.

## Enunciado

El sistema **debe** producir, sin depuración manual, un **reporte público de flota y viajes** apto para publicarse: solo datos de gestión pública, con las personas agregadas o anonimizadas.

**La separación no se hace al exportar. Ya está hecha en el modelo** ([`RN-51`](RN-51-minimizacion-de-datos-de-personas-externas.md)): el reporte se construye desde la partición pública y **no tiene acceso técnico** a la partición de datos personales. Es la diferencia entre filtrar y no tener qué filtrar.

| Va en el reporte | No va, nunca |
|---|---|
| Vehículo por correlativo institucional, tipo, dependencia asignataria | Nombres de personas trasladadas |
| Misiones por período: origen, destino, objeto del traslado, kilometraje | Identidades, teléfonos, direcciones |
| Consumo de combustible y costo por vehículo y período | Datos de salud, etnia, situación migratoria |
| Peajes pagados por punto y período | Manifiestos y listas de pasajeros |
| Estado documental de la flota, agregado | Restricciones médicas de motoristas |
| Número de misiones por dependencia | El motorista nominal de cada misión `[C]` |

## Justificación

Lo que la práctica produce cuando el reporte se arma a mano es una de dos cosas, y ambas son malas: **o no se publica** —y la institución incumple— **o se publica con datos personales dentro**, porque alguien exportó la tabla completa y borró columnas con prisa un viernes por la tarde. El segundo caso es peor: es una filtración con firma institucional.

Automatizarlo no es comodidad. Es el único modo de que la publicación no dependa de que nadie se equivoque nunca.

## Condiciones de aplicación

Aplica a la publicación de oficio en el Portal Único y a la respuesta de solicitudes de acceso a la información sobre flota.

**No aplica** al paquete de evidencia de auditoría ([`RN-98`](RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md)), que **sí** lleva datos personales cuando el alcance los contiene, va al TSC o a Auditoría Interna, y queda registrado como consulta ([`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md)). **Son dos salidas distintas y no se deben confundir**: publicar el paquete de auditoría sería exactamente la filtración que esta regla evita.

## Comportamiento esperado

1. El reporte se genera **a demanda y por período**, y declara el rango, la fecha de generación y quién lo generó.
2. Lleva una **nota de método** que dice qué se agregó y qué se omitió, y por qué. Un reporte público que no explica sus omisiones invita a suponer que oculta algo.
3. La generación **no requiere** permisos sobre datos personales. Quien produce el reporte público no necesita —ni obtiene— acceso a la partición protegida.
4. El formato es el que el Portal Único acepta, más una hoja de cálculo con los datos tabulares. `[C]` Qué formato exige hoy el Portal, y qué publica ya la institución.
5. Si el período no tiene datos, el reporte **se emite vacío y lo declara**. Un período sin misiones es información pública; un reporte ausente parece ocultamiento.

## Casos límite

- **Una sola misión en el período, a un destino que identifica al pasajero.** La agregación no protege cuando el conjunto es de uno: el destino más la fecha reconstruyen a la persona. En ese caso el reporte agrega al nivel inmediatamente superior —mes en lugar de día, región en lugar de municipio— **y declara que lo hizo**. Es el defecto clásico del dato anonimizado, y hay que resolverlo en el diseño y no en la revisión.
- **El objeto del traslado contiene un dato personal** — *«traslado de la señora X al hospital»*. El objeto es campo libre y por ahí se filtra lo que la partición impide. `[C]` Queda pendiente si el objeto se publica literal, se tipifica por catálogo, o se revisa antes de publicar. **Hasta que se decida, el objeto no se publica literal.**
- **La institución ya publica su reporte de flota en otro formato.** `[C]` Insumo abierto en [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md): qué publica hoy y cómo. El reporte se ajusta a lo que el OIP institucional ya entrega; no se le impone un formato nuevo.
- **Alguien pide por transparencia el manifiesto de una misión.** No lo entrega esta regla. Va por el circuito de acceso a la información, con la resolución del OIP, y toda consulta al dato queda registrada ([`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md)).

## Trazabilidad

- **Norma**: [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md) — LTAIP `[V]`, alcance de oficio `[C]`, la automatización `[I]`
- **Hallazgo que la origina**: `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- **Reglas relacionadas**: [`RN-51`](RN-51-minimizacion-de-datos-de-personas-externas.md) es su base estructural · [`RN-52`](RN-52-registro-de-consultas-a-manifiestos.md) · [`RN-98`](RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md) es la salida contraria y no se confunde con ésta
- **Decisión de producto**: [`DP-001` D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — alcance reducido en datos personales
- **Módulo principal**: M-14 Reportes, Indicadores y Auditoría
