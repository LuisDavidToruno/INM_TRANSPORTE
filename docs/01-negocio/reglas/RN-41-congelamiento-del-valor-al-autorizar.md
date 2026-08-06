# RN-41 — El valor calculado se congela al autorizar, junto con el identificador de la tabla de parámetros usada

| Campo | Valor |
|---|---|
| **Módulos** | M-13, M-18, M-09, M-14 |
| **Origen** | Premisa rectora 6 de `CLAUDE.md`; normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) y [NRM-10](../normativa/NRM-10-peajes.md) |
| **Verificación** | `[V]` |
| **Tipo** | Cálculo + bloqueo duro |
| **Configurable** | No |

## Enunciado

En el momento en que un valor calculado se somete a autorización y es autorizado, el sistema **debe congelarlo**: almacenar el resultado junto con:

1. El **identificador de la versión de la tabla o parámetro** usado y su vigencia
2. Los **valores unitarios** que lo componen (tarifa por punto, número de cruces, rendimiento esperado, umbral aplicado)
3. La **fecha del hecho** con la que se resolvió

Una consulta posterior **debe** mostrar el valor histórico congelado, **nunca un recálculo** con los parámetros actuales.

Lo mismo aplica a los **resultados de validación**: la evaluación de habilitación licencia ↔ vehículo se congela con la versión de matriz usada ([RN-09](RN-09-matriz-licencia-vehiculo.md)).

## Justificación

Sin congelamiento, dos consultas del mismo expediente en fechas distintas devuelven números distintos, y ningún expediente es defendible.

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) exige poder demostrar ante el TSC **qué se autorizó y con qué base**. Si el estimado de peajes que aprobó el jefe era de L 264 y hoy el sistema muestra L 372 porque la tarifa cambió, el expediente contradice a su propio firmante.

[NRM-10](../normativa/NRM-10-peajes.md) refuerza el punto al exigir que la corrección retroactiva **deje asiento de la diferencia, nunca sobrescriba el valor histórico**.

## Condiciones de aplicación

Aplica a: estimación de peajes al aprobar ([RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md)), monto de fondo de combustible aprobado ([RN-26](RN-26-fondo-de-combustible-aprobado.md)), asignaciones ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)), resultados de habilitación, calificación de día hábil, y todo valor que forme parte de un acto autorizado.

**No aplica** a los indicadores y reportes analíticos, que se recalculan por definición — pero esos reportes **deben** construirse sobre los valores congelados, no sobre recálculos.

## Comportamiento esperado

1. El congelamiento ocurre en el mismo acto de autorización, dentro del asiento de [RN-03](RN-03-registro-inmutable-de-autorizacion.md), y queda cubierto por la huella del contenido.
2. La consulta del expediente muestra el valor congelado y, si el parámetro cambió después, una **indicación explícita** de que el parámetro vigente hoy es distinto — sin alterar el valor mostrado.
3. Modificar un parámetro **no propaga** a lo congelado. La propagación, cuando corresponda, es un acto deliberado que sigue [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md).
4. Los valores congelados se exportan en el paquete de evidencia de auditoría con su procedencia completa.
5. Si un valor congelado no puede reproducirse a partir de los componentes almacenados, el sistema lo señala como **inconsistencia de congelamiento** — es un defecto grave y debe ser visible.

## Casos límite

- **Valor congelado antes de la ejecución que difiere de lo realmente pagado.** No hay contradicción: el estimado congelado y el pagado real son dos datos distintos que conviven. La conciliación explica la diferencia ([RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)); no se "corrige" el estimado para que cuadre.
- **Reprogramación de la misión a otra fecha.** El congelamiento se **invalida** y exige recálculo y nueva autorización. Arrastrar un estimado congelado a fechas distintas es peor que no congelar.
- **Autorización realizada sin conectividad** en el cliente de campo. El congelamiento se hace con los parámetros de la copia local, y se registra **la fecha de sincronización de esos parámetros**. Al sincronizar, si los parámetros locales estaban desactualizados, el sistema lo señala y decide un humano — nunca recalcula en silencio.
- **Corrección de un parámetro mal cargado** que afectó valores ya congelados. Los congelados permanecen; se genera el reporte de impacto y se resuelve por asiento de diferencia ([RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).
- **Migración de datos históricos.** Los expedientes migrados pueden no tener el identificador de parámetro usado. Se marcan como **valor migrado sin procedencia**, no se les fabrica una. La honestidad del dato importa más que la uniformidad del campo.
- **Resultado de validación congelado que hoy sería distinto.** Por ejemplo, un motorista habilitado con la matriz de 2025 que con la matriz reformada ya no lo estaría. El expediente conserva lo evaluado entonces; lo que no puede ocurrir es que una misión **futura** se apruebe con una validación congelada vieja ([RN-09](RN-09-matriz-licencia-vehiculo.md) revalida al despachar).

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-10](../normativa/NRM-10-peajes.md)
- Premisa rectora 6 de [CLAUDE.md](../../../CLAUDE.md)
- Reglas relacionadas: [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-03](RN-03-registro-inmutable-de-autorizacion.md)
- Actores: ACT-03, ACT-04, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
