# RN-39 — Ningún dato normativo se escribe en el código: todo es parámetro con vigencia por rango de fechas

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-02, y todos |
| **Origen** | Premisa rectora 6 de `CLAUDE.md`; normas [NRM-10](../normativa/NRM-10-peajes.md), [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md). Doble control de [actores-y-roles §4.3 e `I-13`](../actores-y-roles.md) — **artefacto autoridad en actores e incompatibilidades** |
| **Verificación** | `[V]` que las normas de este dominio cambian con frecuencia y de forma imprevisible — `[I]` el doble control carga↔aprobación: es diseño de control interno recogido por `actores-y-roles` y `mapa-de-procesos` PR-09, no articulado citable |
| **Tipo** | Bloqueo duro (regla de construcción y regla de operación, verificables por revisión y por prueba) |
| **Configurable** | No — es la regla que hace configurables a las demás, y su doble control no se desactiva |

## Nota de corrección — hallazgo `HB1-05`

> **Qué estaba mal.** El enunciado decía que el parámetro es *"modificable por ACT-01 Administrador del Sistema o por el rol facultado"*, sin mencionar aprobación alguna, y el comportamiento esperado solo exigía registrar *"quién lo cargó"*. Con eso, **una sola persona ponía en vigencia una tarifa de peaje** y alteraba la base de cálculo de todas las misiones del período: [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) la resuelve, [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md) estima con ella, [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) y [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md) concilian contra ella.
>
> **Qué manda.** [actores-y-roles §4.3](../actores-y-roles.md) titula la sección *"Doble control sobre parámetros normativos"* y es explícita: *"`ACT-01` **carga** el parámetro... `ACT-08` **aprueba** su puesta en vigencia. **Sin la aprobación, el parámetro existe pero no se aplica.**"* La nota 10 de su matriz de permisos lo repite: *"**Carga** el parámetro; **no lo pone en vigencia**."* Y `I-13` declara **núcleo irreductible** la incompatibilidad `ACT-01 × cualquier rol con facultad de autorizar, aprobar fondo o liquidar`, porque *"podría otorgarse a sí mismo la facultad y borrar el rastro"*.
>
> Esta regla es uno de los **bloqueos que no se pueden desactivar** del índice. El control que la hace defendible ante el TSC ahora está en su texto, que es donde tenía que estar.

## Enunciado

Todo dato de origen normativo o institucional **debe** existir como **parámetro con rango de vigencia** (`vigencia_desde`, `vigencia_hasta`), consultable y mantenible **sin cambio de código y sin reinicio del sistema**.

**Ningún parámetro normativo entra en vigencia con la sola acción de una persona.** El ciclo es de **doble control**, y sus dos actos los ejecutan personas distintas:

| Acto | Quién | Qué produce | Qué pasa si falta |
|---|---|---|---|
| **Cargar** | ACT-01 Administrador del Sistema, o el rol facultado de mantenimiento de catálogos | Valor, ámbito, rango de vigencia, **fuente**, fecha de verificación y **respaldo documental adjunto** — comunicado, acuerdo, tabla oficial | Sin carga no hay parámetro |
| **Aprobar la puesta en vigencia** | **ACT-08 Gerencia Administrativa** | Acto de aprobación con identidad, momento y huella del contenido aprobado ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)) | **El parámetro existe y no se aplica.** Ningún cálculo lo resuelve, en ninguna fecha |

Quien carga **no puede** aprobar, y quien aprueba **no puede** cargar sobre el mismo parámetro. Es el par `I-13` del núcleo irreductible: no se levanta por régimen de excepción, ni por delegación, ni por emergencia. ACT-01 **no puede** en ningún caso ostentar la facultad de aprobar.

Un parámetro cargado y no aprobado se presenta en estado **pendiente de aprobación**, visible en el tablero de ACT-08 y en el de ACT-12, y **no participa de ninguna resolución** de [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md).

Alcanza, como mínimo:

| Parámetro | Origen |
|---|---|
| Tarifas de peaje por punto y categoría | [NRM-10](../normativa/NRM-10-peajes.md) |
| Catálogo de categorías de peaje | [NRM-10](../normativa/NRM-10-peajes.md) |
| Estado operativo de cada punto de peaje | [NRM-10](../normativa/NRM-10-peajes.md) |
| Matriz licencia ↔ vehículo | [NRM-06](../normativa/NRM-06-transito-y-licencias.md) |
| Calendario de feriados y días hábiles | [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| Horario hábil por institución y delegación | [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| Umbrales de alerta de vencimiento | [NRM-06](../normativa/NRM-06-transito-y-licencias.md) |
| Umbrales de desviación de rendimiento | [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| Plazos de liquidación y de retención documental | [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| Rendimiento esperado por vehículo | `PROP-01` |

Un requisito, una historia o una prueba que contenga un número normativo literal **está mal escrito**.

## Justificación

Premisa rectora 6 de `CLAUDE.md`: *"Nada normativo se cablea."* Y la evidencia lo respalda de forma abrumadora:

- La tarifa de peaje se revisó tres veces en 2026 y **se revirtió** `[V]`.
- La Ley de Tránsito se reformó en 2025 en las categorías CD y CE `[V]`.
- El seguro obligatorio y la revisión mecánica son anteproyectos que pueden aprobarse en cualquier momento `[V]`.
- La legislación de feriados de octubre **no se pudo verificar** `[C]`.

Un número escrito en el código convierte cada cambio normativo en un despliegue, y cada despliegue tardío en una operación ilegal o en un cobro mal calculado. Peor: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) exige poder explicar ante el TSC **con qué regla se calculó** cada valor histórico, y un valor cableado no deja rastro de su versión.

**Y por qué el doble control.** Sacar el número del código lo vuelve fácil de cambiar — que es el objetivo — y por eso mismo lo vuelve un punto de fuga. *Una tarifa de peaje, un umbral de desviación de rendimiento o un plazo de liquidación **son dinero**: quien pueda cambiarlos solo puede alterar el resultado de toda conciliación pasada y futura* ([actores-y-roles §4.3](../actores-y-roles.md)) `[I]`. El administrador del sistema es precisamente el rol que se define **por exclusión de toda facultad de negocio**; darle la puesta en vigencia sería devolvérselas todas por la puerta de atrás. La parametrización sin doble control no es flexibilidad: es un control interno desmontado con buena letra.

## Condiciones de aplicación

Aplica a todo dato normativo o institucional que pueda cambiar sin que cambie la lógica del proceso.

**No aplica** a la estructura del proceso: el ciclo de vida de la Orden de Misión ([RN-06](RN-06-transiciones-de-estado-de-la-orden.md)) y la segregación de funciones ([RN-01](RN-01-segregacion-de-funciones.md)) **no** son parámetros. Volver configurable un control estructural es la forma elegante de desactivarlo.

## Comportamiento esperado

1. Todo parámetro registra: valor, ámbito, vigencia, **fuente**, fecha de verificación, **respaldo documental adjunto**, **quién lo cargó** y **quién aprobó su puesta en vigencia**, con los dos actos fechados por separado ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)).
2. Las vigencias de un mismo parámetro **no deben** solaparse ni dejar huecos. El sistema valida ambas cosas al guardar y rechaza la carga incoherente.
3. Un parámetro sin valor **vigente y aprobado** a la fecha del hecho **bloquea el cálculo** con mensaje accionable; nunca se sustituye por un valor por defecto ni por la versión pendiente de aprobación ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)).
4. Existe un **inventario de parámetros** consultable: cuáles hay, qué regla los usa, quién los cargó y quién los aprobó, cuáles están pendientes de aprobación, cuándo se verificaron por última vez, y cuáles llevan sin revisar más que el parámetro `plazo_revision_parametro` — **valor de referencia 12 meses** `[C]`, por ámbito y tipo de parámetro ([NRM-10](../normativa/NRM-10-peajes.md) exige la revisión periódica para tarifas; el plazo concreto lo fija la institución). El plazo **no se escribe dentro de esta regla**: sería el mismo defecto que la regla prohíbe.
5. Cambiar un parámetro **no altera** los valores ya congelados ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)); si debe alcanzarlos, se aplica [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md).
6. El **histórico completo de cambios de parámetros** es objeto de auditoría de primera clase para ACT-12: qué valor tenía, quién lo cargó, quién lo aprobó, desde cuándo rigió y qué cálculos lo usaron. La aprobación se registra en la pista append-only y **no se puede alterar ni borrar**, tampoco por ACT-01.
7. El intento de aprobar la propia carga **se bloquea y se registra** como intento de incompatibilidad `I-13` ([RN-01](RN-01-segregacion-de-funciones.md)).

## Casos límite

- **Parámetro que aún no se puede cargar** porque el dato normativo no está confirmado — la tarifa de peaje vigente `[C]`, la matriz definitiva de licencias `[C]`, el calendario de feriados de octubre `[C]`. El sistema debe **poder arrancar sin ellos**, bloqueando únicamente las operaciones que los requieren, con mensaje que identifica el insumo faltante. Arrancar con valores inventados es el peor resultado posible: se vuelven verdad institucional y nadie los vuelve a cuestionar.
- **Parámetro estructural disfrazado.** Alguien pedirá volver configurable la segregación de funciones o el bloqueo de licencia vencida "por si acaso". No se hace: [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) es explícito en que una excepción registrada sería evidencia en contra.
- **Vigencia abierta hacia el futuro** (`vigencia_hasta` vacía). Es lo normal en una tarifa vigente. Al cargar la siguiente, el sistema cierra la anterior con la fecha correspondiente, dejando asiento del cierre.
- **Corrección de un parámetro mal cargado.** No es cambio de vigencia: es corrección de un dato erróneo. Se hace por asiento reverso ([RN-04](RN-04-anulacion-como-asiento-reverso.md)), **exige la misma aprobación de ACT-08** que la carga original, y dispara el análisis de impacto sobre lo ya calculado ([RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).
- **Tarifa que cambia de un día para otro y ACT-08 no está disponible.** La operación no se detiene por eso: mientras la nueva no se apruebe, **rige la anterior**, que sí está aprobada y vigente. Nadie queda sin tabla; queda con la tabla que el doble control respalda. Lo que no se admite es aplicar la nueva "provisionalmente" y regularizar después. `[C]` si la institución quiere un plazo máximo de aprobación con alerta de vencimiento.
- **Parámetro cargado con `vigencia_desde` retroactiva.** Se admite cargarlo — una tarifa puede publicarse tarde —, pero **la aprobación es la que lo hace aplicable**, y el efecto sobre lo ya calculado se resuelve por [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), nunca recalculando en silencio.
- **Carga masiva inicial de catálogos en la implantación.** El doble control aplica igual, pero se aprueba **por lote con su acta**, no parámetro por parámetro. Un control que obliga a mil aprobaciones el primer día se desactiva el segundo.
- **Parámetro con ámbito** — un horario hábil distinto por delegación. El parámetro admite ámbito, y la resolución busca del más específico al más general. Sin ese eje, una delegación de horario continuo obligaría a duplicar reglas.
- **Prueba automatizada que fija un número.** Es legítima si el número es el **dato de prueba** cargado por la propia prueba; es un defecto si es una constante esperada del sistema. La diferencia se verifica: cambiar el parámetro debe cambiar el resultado esperado.

## Trazabilidad

- Normas: [NRM-10](../normativa/NRM-10-peajes.md), [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Premisa rectora 6 de [CLAUDE.md](../../../CLAUDE.md)
- Doble control e incompatibilidad `I-13`: [actores-y-roles §4.3 y §5.2](../actores-y-roles.md); proceso `PR-09` de [mapa-de-procesos](../mapa-de-procesos.md)
- Hallazgo que corrige esta regla: `HB1-05` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md). El `12` cableado que señala `HB1-19` se sustituyó por el parámetro `plazo_revision_parametro`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)
- Actores: ACT-01 (carga), ACT-08 (aprueba la vigencia), ACT-12 (audita)
- Historias y casos especiales: pendientes — Bloque 2
