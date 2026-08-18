# HU-091 — Bloquear la liquidación de quien entregó el fondo, despachó, autorizó o condujo la misión

| Campo | Valor |
|---|---|
| **Módulo** | M-13 Liquidación y Cierre |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Auditor Interno
**quiero** que el sistema impida liquidar una misión a quien entregó su fondo, la despachó, autorizó la necesidad o la condujo, y que cada intento quede registrado
**para** que la segregación de funciones deje de depender de que alguien la recuerde, y para ver el patrón de quien insiste

## Contexto

*Un mismo usuario intentando quince veces liquidar misiones cuyo fondo entregó es exactamente lo que Auditoría Interna quiere ver.* Por eso el intento bloqueado no se descarta: se registra con el par de incompatibilidad detectado.

Dos de estos pares son **núcleo irreductible**: `I-07` autoriza × liquida e `I-10` entrega fondo × liquida. **No los levanta ningún régimen de excepción, ninguna delegación y ninguna resolución de la máxima autoridad.** Quien entrega el dinero no puede ser quien declara en qué se gastó.

Y el bloqueo no puede dejar la misión trabada: ofrece escalamiento en el acto, para que quede visiblemente pendiente en la bandeja de alguien en lugar de perderse.

## Reglas que la gobiernan

- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — `I-07`, `I-09`, `I-10`, `I-11` evaluadas por identidad de persona sobre esta misión
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — Escalamiento al puesto competente en el mismo acto del bloqueo
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El intento bloqueado se registra con autor, puesto, momento y par detectado
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Las divergencias de sincronización no se resuelven sobrescribiendo
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — La versión descartada de una divergencia no se borra

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — En delegación pequeña la tentación de acumular funciones es máxima
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Un relevo agrega personas a la lista de incompatibles de la misión

## Criterios de aceptación

```gherkin
# language: es
Característica: Segregación de funciones en la liquidación de la misión

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "RETORNADA"
    Y que "Nery Discua" entregó el fondo de esa misión
    Y que "Rosa Interiano" la despachó
    Y que "Wilmer Cáceres" la condujo
    Y que "Ana Suazo" autorizó la necesidad como jefatura inmediata

  Escenario: Se bloquea a quien entregó el fondo, sin excepción posible
    Cuando "Nery Discua" intenta liquidar "OM-2026-0512"
    Entonces el sistema rechaza la liquidación y no guarda nada
    Y muestra "Nery Discua entregó el fondo de OM-2026-0512 el 24/09/2026. Quien entrega el fondo no puede liquidar la misión. Esta incompatibilidad no admite excepción."
    Y el sistema no ofrece continuar con acuse, con delegación ni con resolución de la máxima autoridad

  Escenario: Se bloquea a quien autorizó la necesidad, sin excepción posible
    Cuando "Ana Suazo" intenta liquidar "OM-2026-0512"
    Entonces el sistema rechaza la liquidación y no guarda nada
    Y muestra "Ana Suazo autorizó la solicitud que originó OM-2026-0512 el 20/09/2026. Quien autoriza no puede liquidar. Esta incompatibilidad no admite excepción."

  Escenario: Se bloquea a quien despachó
    Cuando "Rosa Interiano" intenta liquidar "OM-2026-0512"
    Entonces el sistema rechaza la liquidación y no guarda nada
    Y muestra "Rosa Interiano despachó OM-2026-0512 el 24/09/2026. Quien despacha no puede liquidar la misma misión."

  Escenario: Se bloquea al motorista de la misión
    Cuando "Wilmer Cáceres" intenta liquidar "OM-2026-0512"
    Entonces el sistema rechaza la liquidación y no guarda nada
    Y muestra "Wilmer Cáceres condujo OM-2026-0512. Un motorista no liquida su propia misión."

  Escenario: El intento queda en la pista de auditoría con el par detectado
    Cuando "Nery Discua" intenta liquidar "OM-2026-0512"
    Entonces el intento queda registrado con persona, puesto, marca de tiempo, misión y par de incompatibilidad "I-10"
    Y el Auditor Interno puede consultar los intentos bloqueados por persona y por período

  Escenario: El bloqueo ofrece escalamiento y la misión no queda trabada
    Cuando el sistema bloquea la liquidación por incompatibilidad
    Entonces genera una tarea de resolución en el puesto competente
    Y ofrece el puesto superior de la misma unidad, el puesto de sede designado como respaldo de la delegación, o Gerencia Administrativa
    Y muestra "La liquidación de OM-2026-0512 queda pendiente en la bandeja de <puesto>."

  Escenario: En delegación la segregación no se relaja, se escala
    Dado una delegación donde una sola persona ejerce despacho y liquidación
    Cuando esa persona intenta liquidar una misión que despachó
    Entonces el sistema rechaza la liquidación
    Y muestra "En esta delegación la función incompatible se ejerce desde la sede. No existe régimen de excepción por tamaño de la delegación."

  Escenario: Se bloquea la liquidación con divergencias de sincronización sin resolver
    Dado dos cadenas de eventos divergentes para el retorno de "OM-2026-0512"
    Cuando el Jefe de Transporte intenta liquidar la misión
    Entonces el sistema rechaza la liquidación
    Y muestra "OM-2026-0512 tiene una divergencia de sincronización sin resolver sobre el odómetro de retorno. Resuélvala antes de liquidar: liquidar sobre dos versiones del retorno produce un número que no significa nada."

  Escenario: La resolución de la divergencia conserva la versión descartada
    Cuando el Jefe de Transporte resuelve la divergencia tomando la versión del dispositivo portador
    Entonces el sistema registra qué versión se tomó, cuál se descartó, por qué y con qué autoridad
    Y la versión descartada se conserva íntegra
    Y el descarte de datos capturados en campo cuenta para el criterio de hallazgo al cerrar

  Escenario: Persona sin ninguna incompatibilidad liquida normalmente
    Cuando "Marvin Aguilar", que no entregó fondo, no despachó, no autorizó y no condujo, liquida "OM-2026-0512"
    Entonces el sistema permite la liquidación
    Y registra al liquidador con su puesto y su marca de tiempo
```

## Fuera de alcance

- El cálculo de las conciliaciones — es [HU-088](HU-088-conciliar-galonaje-contra-kilometraje.md), [HU-089](HU-089-conciliar-el-fondo-y-tipificar-sobrante-y-faltante.md) y [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)
- El bloqueo de quien cierra frente a quien liquidó — es [HU-093](HU-093-cerrar-la-mision-con-la-cadena-completa.md)
- La segregación propia del expediente del **fondo del período** — es [HU-073](HU-073-impedir-que-quien-solicita-el-fondo-lo-apruebe.md)
- La definición del catálogo de puestos de respaldo: viene del espejo de ARGOS

## Notas y pendientes

- El par `I-14` *emite la Orden × liquida la misma misión* es **configurable y está apagado por defecto**. `[C]` confirmar si la institución quiere encenderlo — insumo **#1**
- `[C]` Designación previa del puesto de sede que actúa como respaldo de cada delegación — insumo **#1**, ver [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)
- `[P]` La segregación de funciones proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) (MARCI/TSC); la norma existe, el articulado no se pudo extraer. **No se eleva el nivel**
- La **autoridad en actores e incompatibilidades** es [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) §5.2. Esta historia no define pares nuevos: los consume
