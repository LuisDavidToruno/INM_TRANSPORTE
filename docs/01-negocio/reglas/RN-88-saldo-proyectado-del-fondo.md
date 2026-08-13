# RN-88 — El saldo del fondo se presenta siempre con el comprometido proyectado, y la alerta de agotamiento se dispara sobre el saldo proyectado

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-18, M-13, M-20 |
| **Origen** | Caso especial [CE-23](../../02-requisitos/casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) · Normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) y [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) |
| **Verificación** | `[P]` la exigencia de control previo del compromiso — [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-04](../normativa/NRM-04-presupuesto-siafi.md). `[I]` el saldo proyectado como mecanismo: implicación de requerimiento del equipo |
| **Tipo** | Cálculo + advertencia |
| **Configurable** | Sí — umbrales `alerta_saldo_proyectado` y `tolerancia_sobregiro` |

## Enunciado

El saldo de un fondo de combustible **debe** presentarse **siempre acompañado del comprometido proyectado**: los estimados de combustible y de peaje de las misiones **aprobadas y programadas que aún no tienen asignación emitida**.

**La alerta de agotamiento se dispara sobre el saldo proyectado, no sobre el saldo contable.**

Ningún control de dinero **debe** poder desactivarse por acto de una sola persona: `tolerancia_sobregiro` y el control de cuota trimestral están sujetos a [`RN-39`](RN-39-parametros-normativos-con-vigencia.md) — los carga el administrador del sistema y los pone en vigencia la Gerencia Administrativa.

## Justificación

[`RN-26`](RN-26-fondo-de-combustible-aprobado.md) define el saldo como *aprobado − asignado + devoluciones*. Ese número **es correcto y es ciego a la cartera**: dice cuánto queda hoy, no cuánto se va a necesitar la semana que viene por misiones que ya se aprobaron.

Ninguna regla obliga a mirar el agregado, que es **donde se ve venir el problema con dos semanas de anticipación**. El resultado es que el fondo se descubre agotado el día en que una delegación viene por su vale, con doce misiones programadas por delante y ninguna forma ordenada de decidir cuáles se financian.

Con saldo proyectado, la Gerencia Administrativa tiene tiempo para pedir la ampliación o la reprogramación de cuota. Sin él, tiene un problema el mismo día en que lo descubre.

## Condiciones de aplicación

Aplica a todo fondo de combustible vigente y a toda pantalla o reporte que muestre su saldo.

**No aplica** al arqueo histórico de un período cerrado, donde el saldo proyectado ya no tiene sentido y lo que manda es el contable.

## Comportamiento esperado

1. El comprometido proyectado se calcula con los estimados congelados de las misiones aprobadas y programadas sin asignación emitida ([`RN-35`](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md)).
2. Toda vista del fondo muestra los cuatro números juntos: **aprobado, asignado, saldo contable y saldo proyectado**, con la cartera que lo compone consultable.
3. Al cruzar `alerta_saldo_proyectado`, el sistema notifica a la Gerencia Administrativa y al Jefe de Transporte con la cartera detallada y el déficit estimado.
4. Cuando el saldo no alcanza para la cartera, el sistema presenta las misiones ordenadas por el criterio de prelación y **la decisión la toma una persona con acuse motivado registrado** ([`RN-56`](RN-56-prelacion-entre-solicitudes-que-compiten.md)). **El sistema no cancela misiones por sí solo ni ordena por jerarquía del solicitante.**
5. **Nada se resuelve apagando el control**: `tolerancia_sobregiro` no se sube *"por esta vez"* y el control de cuota trimestral no se pone en *no verificar* para dejar pasar una misión. La salida legítima es el **acuse motivado**, que además es lo que sustenta el pedido de reprogramación de cuota.
6. El compromiso se valida además contra la **cuota trimestral** ([`RN-54`](RN-54-cuota-trimestral-de-compromiso.md)), no solo contra el saldo del fondo.

## Casos límite

- **Misión que cruza el cierre de trimestre** — sale el 28 de septiembre y retorna el 3 de octubre. El compromiso se imputa al **trimestre del acto que lo generó**, no al del retorno ([`RN-54`](RN-54-cuota-trimestral-de-compromiso.md), [`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)). `[C]` confirmar el criterio con la Gerencia Administrativa: es el tipo de detalle que cada institución resuelve distinto.
- **`[C]` ¿Se admite despachar sin fondo asignado?** Opciones y costo:

  | Opción | Costo |
  |---|---|
  | **Sí, con responsable nominado** | La operación no se paraliza, pero hay dinero público comprometido sin cobertura previa y el TSC lo va a preguntar |
  | **No** | Control impecable, y la delegación remota no recibe su equipo |

  **Se propone:** *sí, con responsable nominado, motivo y marca visible hasta la liquidación*. Decisión del PO.
- **Estimado muy por encima del consumo real**, de forma sistemática. El saldo proyectado exagera el problema y bloquea misiones innecesariamente. Se corrige calibrando el estimado con la serie histórica, no aflojando el umbral.
- **Ampliación de fondo aprobada pero no formalizada.** No entra al saldo hasta que exista el acto ([`RN-26`](RN-26-fondo-de-combustible-aprobado.md)). Se puede mostrar como *en trámite*, claramente separada.
- **Cuota trimestral aún no aprobada** al inicio del ejercicio. Ver [`RN-96`](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md): la ventana entre ejercicios tiene su propio tratamiento y su propia decisión pendiente.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` · [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) `[P]`
- Reglas relacionadas: [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-54](RN-54-cuota-trimestral-de-compromiso.md), [RN-56](RN-56-prelacion-entre-solicitudes-que-compiten.md), [RN-81](RN-81-sigti-expone-hechos-a-argos.md), [RN-96](RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md)
- Casos especiales: [CE-23](../../02-requisitos/casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — candidatas `RN-C23a`, `RN-C23b`
- Insumos pendientes: #7 decisiones abiertas del fondo · #31 criterio de prelación
