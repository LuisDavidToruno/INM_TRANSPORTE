# HU-078 — Bloquear nueva asignación de fondo a quien tiene obligación de reintegro abierta

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-07 Encargado de Combustible |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Encargado de Combustible
**quiero** que el sistema me impida emitir una nueva asignación a un servidor que no devolvió ni comprobó el fondo anterior
**para** que el saldo afuera deje de crecer sobre las mismas personas, que es lo que hoy nadie puede impedir porque nadie lo ve

## Contexto

Hoy nada impide seguir entregándole fondo a quien no liquidó el anterior. El saldo se acumula sobre unas pocas personas y aparece recién cuando alguien hace el arqueo del período, meses después.

El bloqueo tiene una válvula deliberada: cuando la persona bloqueada es la única disponible para una misión urgente, el levantamiento es **acto registrado de Gerencia Administrativa con motivo**, nunca decisión de quien programa o de quien emite. La excepción queda en el expediente y en el indicador.

## Reglas que la gobiernan

- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — Obligación de reintegro con ciclo propio; bloqueo de nueva asignación; arqueo por persona
- [RN-32](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) — El receptor verificado es donde se evalúa el bloqueo
- [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) — Asignado vs. consumido vs. comprobado vs. devuelto: lo que define el saldo afuera
- [RN-14](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) — La salida legítima cuando no se levanta el bloqueo es reasignar el motorista
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El levantamiento es un acto con autor, puesto, motivo y momento

## Casos especiales que la afectan

- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — De dónde nace la obligación de reintegro
- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — El fondo agotado empuja a levantar bloqueos por urgencia
- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — El plazo de devolución corre desde la anulación

## Criterios de aceptación

```gherkin
# language: es
Característica: Bloqueo de asignación a servidor con obligación de reintegro abierta

  Antecedentes:
    Dado un motorista "Denis Fúnez" con una obligación de reintegro abierta de "L 3,400.00" originada en la misión "OM-2026-0468"
    Y un parámetro "plazo_devolucion_saldo" de "5" días hábiles
    Y una Orden de Misión "OM-2026-0540" en estado "PROGRAMADA" con "Denis Fúnez" como motorista

  Escenario: Se bloquea la emisión y se nombra la deuda con su origen
    Cuando el Encargado de Combustible intenta emitir la asignación de "OM-2026-0540" a "Denis Fúnez"
    Entonces el sistema rechaza la emisión
    Y muestra "Denis Fúnez tiene una obligación de reintegro abierta de L 3,400.00 desde la misión OM-2026-0468, con plazo vencido el 04/09/2026. No puede recibir nueva asignación."
    Y el intento queda registrado en el expediente de la obligación

  Escenario: Se bloquea también por saldo vencido aún sin obligación formalizada
    Dado un motorista "Óscar Banegas" con "L 1,850.00" entregados y sin comprobar, con plazo vencido hace "3" días hábiles
    Cuando el Encargado de Combustible intenta emitir una asignación a "Óscar Banegas"
    Entonces el sistema rechaza la emisión
    Y muestra "Óscar Banegas tiene L 1,850.00 sin comprobar de la misión OM-2026-0491, con plazo vencido el 18/09/2026."

  Escenario: El Encargado de Combustible no puede levantar el bloqueo
    Cuando el Encargado de Combustible intenta levantar el bloqueo de "Denis Fúnez"
    Entonces el sistema rechaza la acción
    Y muestra "El levantamiento del bloqueo es acto de Gerencia Administrativa con motivo escrito. Solicítelo o reasigne el motorista de la misión."

  Escenario: El Jefe de Transporte tampoco puede levantarlo al programar
    Cuando el Jefe de Transporte intenta levantar el bloqueo desde la pantalla de programación
    Entonces el sistema rechaza la acción
    Y muestra "El levantamiento del bloqueo no es decisión de quien programa. Corresponde a Gerencia Administrativa."

  Escenario: Gerencia Administrativa levanta el bloqueo con motivo y queda en el indicador
    Cuando la Gerencia Administrativa levanta el bloqueo de "Denis Fúnez" con motivo "Único motorista habilitado categoría C disponible para el traslado de equipo del 25/09/2026"
    Entonces el sistema permite emitir la asignación de "OM-2026-0540"
    Y la excepción queda en el expediente de la misión y en el expediente de la obligación
    Y la excepción figura en el indicador de levantamientos por persona y por período

  Escenario: Se rechaza el levantamiento sin motivo escrito
    Cuando la Gerencia Administrativa levanta el bloqueo de "Denis Fúnez" sin motivo escrito
    Entonces el sistema rechaza el levantamiento
    Y muestra "El levantamiento exige motivo escrito. Queda en el expediente y en el indicador."

  Escenario: Saldar la obligación libera a la persona
    Cuando se registra el reintegro de "L 3,400.00" de "Denis Fúnez" con acta y fecha del hecho "2026-09-24"
    Entonces la obligación queda saldada con asiento
    Y "Denis Fúnez" puede recibir nueva asignación de fondo
    Y el arqueo por persona deja de mostrarlo con saldo afuera
```

## Fuera de alcance

- La determinación de responsabilidad administrativa por el faltante: no nace aquí ([RN-74](../../01-negocio/reglas/RN-74-sin-atribucion-de-responsabilidad-en-campo.md))
- La tipificación del faltante al liquidar — es [HU-089](HU-089-conciliar-el-fondo-y-tipificar-sobrante-y-faltante.md)
- El descuento por planilla o cualquier gestión de cobro: es del ámbito de Talento Humano y Administración, fuera de SIGTI
- El cierre de la misión con la obligación abierta: **no lo impide** — es [HU-094](HU-094-cerrar-con-hallazgo-tipificado.md)

## Notas y pendientes

- `[C]` **`plazo_devolucion_saldo` en días hábiles** — insumo **#32**
- `[C]` ¿El sobrante se devuelve o se arrastra? Determina cuándo nace la obligación — insumo **#7 / `PROP-01`**
- `[C]` ¿Existe un tope de monto por debajo del cual no se formaliza obligación de reintegro? — insumo **#1**, con Auditoría Interna
- `[P]` El control de fondos entregados a servidores proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), con articulado no extraído
