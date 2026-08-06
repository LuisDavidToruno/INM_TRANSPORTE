# RN-39 — Ningún dato normativo se escribe en el código: todo es parámetro con vigencia por rango de fechas

| Campo | Valor |
|---|---|
| **Módulos** | M-02, y todos |
| **Origen** | Premisa rectora 6 de `CLAUDE.md`; normas [NRM-10](../normativa/NRM-10-peajes.md), [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` que las normas de este dominio cambian con frecuencia y de forma imprevisible |
| **Tipo** | Bloqueo duro (regla de construcción, verificable por revisión y por prueba) |
| **Configurable** | No — es la regla que hace configurables a las demás |

## Enunciado

Todo dato de origen normativo o institucional **debe** existir como **parámetro con rango de vigencia** (`vigencia_desde`, `vigencia_hasta`), consultable y modificable por ACT-01 Administrador del Sistema o por el rol facultado, **sin cambio de código y sin reinicio del sistema**.

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

## Condiciones de aplicación

Aplica a todo dato normativo o institucional que pueda cambiar sin que cambie la lógica del proceso.

**No aplica** a la estructura del proceso: el ciclo de vida de la Orden de Misión ([RN-06](RN-06-transiciones-de-estado-de-la-orden.md)) y la segregación de funciones ([RN-01](RN-01-segregacion-de-funciones.md)) **no** son parámetros. Volver configurable un control estructural es la forma elegante de desactivarlo.

## Comportamiento esperado

1. Todo parámetro registra: valor, vigencia, **fuente**, fecha de verificación, y quién lo cargó ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)).
2. Las vigencias de un mismo parámetro **no deben** solaparse ni dejar huecos. El sistema valida ambas cosas al guardar y rechaza la carga incoherente.
3. Un parámetro sin valor vigente a la fecha del hecho **bloquea el cálculo** con mensaje accionable; nunca se sustituye por un valor por defecto ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)).
4. Existe un **inventario de parámetros** consultable: cuáles hay, qué regla los usa, cuándo se verificaron por última vez, y cuáles llevan más de 12 meses sin revisión ([NRM-10](../normativa/NRM-10-peajes.md) lo exige para tarifas).
5. Cambiar un parámetro **no altera** los valores ya congelados ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)); si debe alcanzarlos, se aplica [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md).

## Casos límite

- **Parámetro que aún no se puede cargar** porque el dato normativo no está confirmado — la tarifa de peaje vigente `[C]`, la matriz definitiva de licencias `[C]`, el calendario de feriados de octubre `[C]`. El sistema debe **poder arrancar sin ellos**, bloqueando únicamente las operaciones que los requieren, con mensaje que identifica el insumo faltante. Arrancar con valores inventados es el peor resultado posible: se vuelven verdad institucional y nadie los vuelve a cuestionar.
- **Parámetro estructural disfrazado.** Alguien pedirá volver configurable la segregación de funciones o el bloqueo de licencia vencida "por si acaso". No se hace: [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) es explícito en que una excepción registrada sería evidencia en contra.
- **Vigencia abierta hacia el futuro** (`vigencia_hasta` vacía). Es lo normal en una tarifa vigente. Al cargar la siguiente, el sistema cierra la anterior con la fecha correspondiente, dejando asiento del cierre.
- **Corrección de un parámetro mal cargado.** No es cambio de vigencia: es corrección de un dato erróneo. Se hace por asiento reverso ([RN-04](RN-04-anulacion-como-asiento-reverso.md)) y dispara el análisis de impacto sobre lo ya calculado ([RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).
- **Parámetro con ámbito** — un horario hábil distinto por delegación. El parámetro admite ámbito, y la resolución busca del más específico al más general. Sin ese eje, una delegación de horario continuo obligaría a duplicar reglas.
- **Prueba automatizada que fija un número.** Es legítima si el número es el **dato de prueba** cargado por la propia prueba; es un defecto si es una constante esperada del sistema. La diferencia se verifica: cambiar el parámetro debe cambiar el resultado esperado.

## Trazabilidad

- Normas: [NRM-10](../normativa/NRM-10-peajes.md), [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Premisa rectora 6 de [CLAUDE.md](../../../CLAUDE.md)
- Reglas relacionadas: [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)
- Actores: ACT-01, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
