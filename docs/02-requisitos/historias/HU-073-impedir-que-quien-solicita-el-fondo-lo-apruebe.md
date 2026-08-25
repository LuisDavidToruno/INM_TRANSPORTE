# HU-073 — Impedir que quien solicita el fondo sea quien lo aprueba o quien lo liquida

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-08 Gerencia Administrativa (y ACT-04 Jefe de Transporte como solicitante) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por `PROP-01` / insumo #7 como todo el ciclo del fondo, y falta confirmar con Auditoría Interna si el puesto de respaldo para el escalamiento se designa por adelantado o se resuelve caso a caso (insumo #1). Sin respaldo designado, el bloqueo por segregación deja a la delegación sin poder aprobar |

## Historia

**Como** Gerencia Administrativa
**quiero** que el sistema me impida aprobar un fondo que yo mismo solicité, y que impida que quien lo solicitó o lo aprobó sea quien lo liquide al cierre del período
**para** que el expediente del fondo resista una revisión de Auditoría Interna sin depender de que nadie se acuerde de la regla

## Contexto

La segregación de funciones de [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) razona **por Orden de Misión**. El fondo es un objeto **de período**, y por eso tiene su propio control: el numeral 4 de [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) declara la incompatibilidad *solicita × aprueba × liquida el fondo*, evaluada por **identidad de persona**, no por rol.

El fondo es dinero. Aquí la segregación es más importante, no menos.

## Reglas que la gobiernan

- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — Numeral 4 del enunciado: segregación propia del expediente del fondo. **Bloqueo duro no configurable**
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Marco de las incompatibilidades y del registro del intento bloqueado
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — Escalamiento cuando el aprobador natural es el solicitante
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El intento bloqueado también se registra

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — La urgencia por el fondo agotado es exactamente cuando alguien intenta saltarse el circuito
- No hay caso especial de delegación pequeña propio de esta historia: aprobar un fondo **no exige presencia física** y por eso se ejerce desde la sede ([DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md))

## Criterios de aceptación

```gherkin
# language: es
Característica: Segregación propia del expediente del fondo de combustible

  Antecedentes:
    Dado una solicitud de fondo "FND-2026-09-004" registrada por "Marvin Aguilar" el "2026-09-15"
    Y que "Marvin Aguilar" ocupa además, de forma temporal, el puesto con rol de Gerencia Administrativa

  Escenario: Se bloquea la aprobación por el mismo solicitante y no se guarda nada
    Cuando "Marvin Aguilar" intenta aprobar la solicitud "FND-2026-09-004"
    Entonces el sistema rechaza la aprobación
    Y muestra "Usted registró la solicitud de fondo FND-2026-09-004 el 15/09/2026. No puede aprobarla."
    Y no se guarda ningún dato de la aprobación
    Y el intento queda en la pista de auditoría con el par de incompatibilidad detectado

  Escenario: El bloqueo ofrece salida en el mismo acto
    Cuando "Marvin Aguilar" intenta aprobar la solicitud "FND-2026-09-004"
    Entonces el sistema genera una tarea de resolución en el puesto competente
    Y muestra "La aprobación corresponde al puesto <nombre del puesto> de la dependencia matriz. La solicitud queda pendiente en su bandeja."

  Escenario: Se bloquea la liquidación del período por quien solicitó el fondo
    Dado un fondo "FND-2026-09-004" solicitado por "Marvin Aguilar" y aprobado por "Sandra Padilla"
    Cuando "Marvin Aguilar" intenta liquidar el fondo al cierre del período
    Entonces el sistema rechaza la liquidación
    Y muestra "Usted solicitó el fondo FND-2026-09-004. No puede liquidarlo al cierre del período."

  Escenario: Se bloquea la liquidación del período por quien lo aprobó
    Dado un fondo "FND-2026-09-004" aprobado por "Sandra Padilla"
    Cuando "Sandra Padilla" intenta liquidar el fondo al cierre del período
    Entonces el sistema rechaza la liquidación
    Y muestra "Usted aprobó el fondo FND-2026-09-004 el 16/09/2026. No puede liquidarlo al cierre del período."

  Escenario: No existe pantalla de excepción para este bloqueo
    Cuando "Marvin Aguilar" intenta aprobar la solicitud "FND-2026-09-004"
    Entonces el sistema no ofrece la opción de continuar con acuse ni con motivo escrito
    Y no ofrece la opción de desactivar la verificación para este fondo

  Escenario: Se aprueba con personas distintas
    Dado una solicitud "FND-2026-09-004" registrada por "Marvin Aguilar"
    Cuando "Sandra Padilla" aprueba la solicitud "FND-2026-09-004"
    Entonces el sistema acepta la aprobación
    Y registra solicitante "Marvin Aguilar", aprobador "Sandra Padilla", puesto de cada uno y marca de tiempo
```

## Fuera de alcance

- La verificación de cuota trimestral — es [HU-072](HU-072-aprobar-fondo-verificando-cuota-trimestral.md)
- Las incompatibilidades por **Orden de Misión** (`I-01` a `I-17`) — se evalúan en emisión, entrega, liquidación y cierre: [HU-079](HU-079-entregar-el-fondo-contra-firma-dentro-del-despacho.md), [HU-091](HU-091-bloquear-la-liquidacion-por-segregacion-de-funciones.md), [HU-093](HU-093-cerrar-la-mision-con-la-cadena-completa.md)
- La definición del catálogo de puestos y su cadena de escalamiento: viene del espejo de ARGOS

## Notas y pendientes

- ⚠️ **Hallazgo abierto `HB4-01`.** El par *solicita fondo × aprueba fondo* **no figura** en la tabla `I-01` a `I-17` de [`actores-y-roles.md` §5.2](../../01-negocio/actores-y-roles.md), que es la **autoridad en incompatibilidades**. Hoy vive solo en el numeral 4 de [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md). Esta historia implementa la regla, **no resuelve la contradicción**: queda dirigida a `actores-y-roles.md` para que incorpore el par
- `[P]` La segregación de funciones y el control de fondos entregados a servidores provienen de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); la norma existe, no se pudo extraer el articulado. **No se eleva el nivel**
- `[C]` Confirmar con Auditoría Interna si el puesto de respaldo para el escalamiento se designa por adelantado o se resuelve caso a caso — insumo **#1**
