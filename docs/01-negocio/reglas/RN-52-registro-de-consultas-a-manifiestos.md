# RN-52 — Toda consulta a manifiestos y listas de pasajeros se registra: quién vio qué y cuándo

| Campo | Valor |
|---|---|
| **Módulos** | M-17, M-14, M-01 |
| **Origen** | Norma [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md); [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — acceso restringido a activos y registros |
| **Verificación** | `[V]` que el **hábeas data del Artículo 182 constitucional** está vigente y que el Artículo 23 de la LTAIP lo reconoce — [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md). `[C]` que el **MARCI** exija control de acceso y registro de consultas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) tiene esa familia por confirmar, con los códigos a tomar del ejemplar impreso de la institución. `[I]` que del hábeas data **se siga** la obligación de registrar cada consulta: es implicación de requerimiento del equipo, no articulado |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |


## Nota de corrección — hallazgo `HN1-14`

> **Qué estaba mal.** La cabecera declaraba `[V]` *«que el MARCI exige control de acceso y registro de consultas, aun sin ley de datos»*. [`NRM-01`](../normativa/NRM-01-control-interno-tsc.md) tiene esa familia del MARCI —*«acceso restringido a activos y registros»*— marcada `[C]`, con los códigos y títulos exactos por tomar del ejemplar impreso de la institución. Era la escalada de nivel que [`CLAUDE.md`](../../../CLAUDE.md) prohíbe: **ningún artefacto declara un nivel superior al de la ficha que cita.**
>
> **La cabecera contradecía a su propio cuerpo.** La justificación de esta regla ya decía lo correcto —hábeas data `[V]`, acceso restringido del MARCI `[C]`—. Lo que había que corregir era la etiqueta, y la etiqueta es lo que el auditor lee.
>
> **Qué se corrigió.** La verificación se separó en sus tres afirmaciones, porque no todas tienen el mismo respaldo: el hábeas data está `[V]`; la exigencia del MARCI queda `[C]`; y **que del hábeas data se siga la obligación de registrar cada consulta es `[I]`** — es implicación de requerimiento escrita por el equipo en [`NRM-07`](../normativa/NRM-07-transparencia-y-datos-personales.md), no articulado citable. El hallazgo proponía un `[P]` único; se fue más lejos porque un `[P]` plano habría subestimado el hábeas data y sobrestimado la inferencia.
>
> **Lo que NO cambia: sigue siendo bloqueo duro y sigue sin ser configurable.** Bajar el nivel de verificación no debilita el control, y conviene decirlo para que nadie lea esta corrección como permiso para relajarlo. El fundamento operativo es independiente del MARCI: el hábeas data del Artículo 182 está vigente `[V]`, solo el titular puede interponerlo, y **si una persona pregunta quién accedió a sus datos, la única respuesta defendible es el registro de consultas.** Sin él la institución no puede afirmar nada. Es también lo que [`DP-001` D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) conservó a propósito al reducir el alcance.
## Enunciado

El acceso a manifiestos, listas de pasajeros y cualquier dato personal de personas trasladadas **debe** estar restringido por **rol y por necesidad de conocer**, y **cada consulta debe registrarse** con: identidad del consultante, rol, fecha y hora, registro consultado y alcance de lo mostrado.

El registro de consultas **debe** ser inmutable y consultable por ACT-12 Auditor Interno.

Ningún rol, incluido ACT-01 Administrador del Sistema, **debe** poder consultar estos datos sin dejar rastro.

## Justificación

[NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md), incluso después de la reducción de alcance de [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md), conserva expresamente esta exigencia: *"control de acceso por necesidad de conocer sobre listas de pasajeros, con registro de cada consulta: quién vio qué lista y cuándo. Aun sin ley de datos, esto es exigible por el MARCI y protege ante un hábeas data."*

El hábeas data del Artículo 182 constitucional está vigente `[V]` y solo el titular puede interponerlo. Si una persona pregunta quién accedió a sus datos, la única respuesta defendible es el registro de consultas. Sin él, la institución no puede afirmar nada.

Y desde el control interno: el acceso restringido a registros es una norma del MARCI `[C]` cuya numeración exacta debe tomarse del ejemplar impreso de la institución.

## Condiciones de aplicación

Aplica a datos personales de personas trasladadas (M-17) y a las restricciones médicas del expediente del motorista ([RN-11](RN-11-restricciones-medicas-del-motorista.md)), que son datos de salud.

**No aplica** a los datos de gestión pública — vehículo, ruta, costo, unidad ejecutora, objeto del viaje —, que son públicos por transparencia.

**No aplica** a la consulta agregada o anonimizada, que por construcción no expone a nadie.

## Comportamiento esperado

1. El acceso se concede por rol **y** por ámbito: una jefatura de la dependencia A no ve manifiestos de la dependencia B sin fundamento.
2. La consulta se registra aunque el resultado sea vacío: una búsqueda por nombre que no devuelve nada también revela interés.
3. El registro distingue el **alcance**: ver el manifiesto completo no es lo mismo que ver el conteo de pasajeros. Ambos se registran, con su nivel.
4. Existe reporte de **accesos por usuario, por registro y por período**, y alerta ante patrones anómalos — consultas masivas, consultas fuera de horario, consultas repetidas sobre una misma persona.
5. El registro de consultas se conserva por el plazo de retención configurado y **sobrevive** a la depuración de los datos personales consultados: se debe poder demostrar quién los vio incluso después de depurarlos.

## Casos límite

- **Consulta desde el cliente de campo sin conectividad.** El registro se genera localmente y sincroniza después ([RN-43](RN-43-captura-de-campo-sin-conectividad.md)). No se admite acceso sin registro por estar fuera de línea: si el dispositivo no puede registrar, no muestra el dato.
- **Impresión del manifiesto.** Es un acceso, y además genera un objeto fuera de control. Se registra como consulta **con impresión**, y el documento lleva folio ([RN-25](RN-25-salvoconducto-con-folio-y-qr.md)).
- **Exportación masiva para auditoría.** Se registra igual, indicando volumen y destino. El auditor no es una excepción al registro: es quien más necesita que el registro exista.
- **Depuración de datos personales por retención.** Los datos se seudonimizan, pero el registro de consultas previo se conserva referenciando el identificador seudonimizado. Borrar el registro de consultas al depurar destruiría la única prueba de trazabilidad de acceso.
- **Volumen de registros de consulta.** En una institución de alto flujo, superará al de los datos mismos. Es un costo aceptado; lo que debe dimensionarse es el almacenamiento, no relajarse la regla.
- **Consulta legítima que el reporte marca como anómala** — una jefatura revisando todos los manifiestos del mes para un informe. La alerta no acusa: señala para revisión. El responsable la resuelve anotando el motivo, y esa anotación queda.
- **Acceso técnico directo a la base de datos**, fuera de la aplicación. Escapa a esta regla por construcción. Se mitiga con control de credenciales y registro de la plataforma; y debe estar explícitamente documentado como riesgo residual. `[C]` quién administra el servidor on-premise en cada institución.

## Trazabilidad

- Normas: [NRM-07](../normativa/NRM-07-transparencia-y-datos-personales.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Decisión: [DP-001, D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md), [RN-11](RN-11-restricciones-medicas-del-motorista.md), [RN-03](RN-03-registro-inmutable-de-autorizacion.md)
- Actores: ACT-01, ACT-03, ACT-05, ACT-12
- Historias: [HU-117](../../02-requisitos/historias/HU-117-acceso-al-manifiesto-por-necesidad-de-conocer.md) necesidad de conocer · [HU-118](../../02-requisitos/historias/HU-118-registrar-cada-consulta-al-manifiesto.md) registro de cada consulta · [HU-119](../../02-requisitos/historias/HU-119-reporte-de-accesos-y-alerta-de-patron-anomalo.md) reporte y alertas · [HU-120](../../02-requisitos/historias/HU-120-consultar-el-manifiesto-sin-conectividad.md) consulta sin red · [HU-121](../../02-requisitos/historias/HU-121-atender-habeas-data-buscar-y-exportar.md) hábeas data
