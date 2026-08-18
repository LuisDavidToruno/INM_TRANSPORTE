# HU-075 — Ampliar el fondo agotado y resolver la prelación sin que el sistema cancele misiones

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** solicitar la ampliación del fondo antes de que se agote y, si el dinero no alcanza para toda la cartera, decidir yo con acuse motivado qué misiones se sacrifican
**para** que ninguna misión quede cancelada por una decisión automática que después nadie pueda explicar ante la dependencia solicitante

## Contexto

El fondo se agota a mitad del período, siempre. Lo que hoy ocurre es que las misiones se despachan igual y el combustible aparece de donde puede: prestado de otra dependencia, cargado de la cisterna, o pagado por el motorista de su bolsillo. Ese es el origen real del **préstamo sin folio** que después se ve en la conciliación como un rendimiento imposiblemente bueno ([CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) → [CE-21](../casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md)).

Por eso la alerta se dispara sobre el **saldo proyectado** y no sobre el contable: cuando el contable llega a cero, el préstamo invisible ya ocurrió.

## Reglas que la gobiernan

- [RN-88](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) — La alerta se dispara sobre el saldo proyectado: contable menos estimados de combustible y peajes comprometidos
- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — La ampliación sigue el mismo circuito que la solicitud original, con la misma segregación
- [RN-56](../../01-negocio/reglas/RN-56-prelacion-entre-solicitudes-que-compiten.md) — Criterio de prelación configurable con vigencia; la decisión es humana y motivada
- [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) — El estimado de peajes entra en el comprometido
- [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) — Distinguir el fondo agotado de la cuota copada: son problemas con salidas distintas

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — Eje de la historia
- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — Mismo criterio de prelación aplicado a un recurso escaso distinto
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — Los sobrantes recurrentes indican una estimación mal calibrada, no un motorista honrado

## Criterios de aceptación

```gherkin
# language: es
Característica: Ampliación del fondo y prelación de la cartera de misiones

  Antecedentes:
    Dado un fondo "FND-2026-09-004" con monto aprobado de "L 200,000.00"
    Y un saldo contable de "L 34,500.00"
    Y 9 misiones "APROBADA" o "PROGRAMADA" sin asignación emitida, con estimado de combustible y peajes de "L 71,200.00"

  Escenario: La alerta se dispara antes del agotamiento contable
    Cuando el sistema evalúa el fondo "FND-2026-09-004"
    Entonces se dispara la alerta de agotamiento
    Y muestra "Saldo contable L 34,500.00. Comprometido L 71,200.00. Saldo proyectado L -36,700.00. Solicite ampliación."
    Y el saldo contable de L 34,500.00 no se presenta como saldo disponible

  Escenario: Las misiones programadas no se anulan mientras la ampliación se tramita
    Cuando el Jefe de Transporte solicita una ampliación de "L 80,000.00"
    Entonces las 9 misiones permanecen en su estado actual
    Y cada una queda con la marca "sin fondo asignado" visible
    Y ninguna misión cambia de estado por efecto de la solicitud de ampliación

  Escenario: El sistema no cancela ninguna misión por sí solo
    Dado que la ampliación se aprueba por "L 40,000.00" en lugar de los "L 80,000.00" solicitados
    Cuando el sistema evalúa la cartera contra el nuevo saldo de "L 74,500.00"
    Entonces el sistema presenta las 9 misiones ordenadas por el criterio de prelación vigente
    Y muestra "El saldo alcanza para 7 de 9 misiones. Decida cuáles se posponen: el sistema no cancela ninguna."
    Y ninguna misión queda cancelada automáticamente

  Escenario: Se ofrecen las consolidaciones antes de plantear el sacrificio
    Cuando el sistema presenta la cartera que excede el saldo
    Entonces muestra primero las consolidaciones posibles con su ahorro estimado
    Y muestra "Consolidando OM-2026-0512 y OM-2026-0518 hacia Comayagua el 24/09/2026 se ahorran L 3,850.00."

  Escenario: Se rechaza posponer una misión sin acuse motivado
    Cuando el Jefe de Transporte pospone la misión "OM-2026-0521" sin motivo escrito
    Entonces el sistema rechaza la acción
    Y muestra "Posponer una misión aprobada exige motivo escrito. El acuse queda en el expediente de la misión y se notifica a la dependencia solicitante."

  Escenario: La constancia queda en el expediente de cada misión desplazada
    Cuando el Jefe de Transporte pospone "OM-2026-0521" con motivo "Saldo insuficiente del fondo FND-2026-09-004; ampliación aprobada parcialmente el 22/09/2026"
    Entonces el expediente de "OM-2026-0521" conserva el motivo, el autor, su puesto y la marca de tiempo
    Y la dependencia solicitante recibe la notificación con ese mismo texto

  Escenario: No se confunde el fondo agotado con la cuota copada
    Dado que la cuota del trimestre "2026-T3" está copada y el fondo tiene saldo
    Cuando el Jefe de Transporte abre la solicitud de ampliación
    Entonces el sistema muestra "La cuota de compromiso del trimestre 2026-T3 está copada. Una ampliación de fondo no resuelve esto: requiere reprogramación de cuota gestionada por Gerencia Administrativa."
    Y no muestra el texto "fondo agotado"
```

## Fuera de alcance

- El cálculo de consolidaciones posibles: lo produce M-07 Programación y Despacho; aquí solo se consume
- El despacho de una misión sin fondo asignado — es [HU-077](HU-077-bloquear-la-emision-por-saldo-insuficiente.md)
- La reprogramación de cuota ante SIAFI: se gestiona fuera de SIGTI
- El registro del combustible que efectivamente entró al tanque sin folio — es [HU-083](HU-083-declarar-la-fuente-de-todo-abastecimiento.md)

## Notas y pendientes

- `[C]` **Criterio de prelación entre misiones que compiten** — `criterio_prelacion` se entrega vacío y configurable. **No se inventa un orden.** Insumo **#31**
- `[C]` Umbral `alerta_saldo_proyectado` — insumo **#1**
- `[C]` ¿Admite la institución despachar una misión con la marca *sin fondo asignado*? Propuesta escalada al PO en [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — insumo **#1**
