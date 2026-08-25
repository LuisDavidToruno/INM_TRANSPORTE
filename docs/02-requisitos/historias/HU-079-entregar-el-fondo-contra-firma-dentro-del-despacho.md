# HU-079 — Entregar el instrumento contra firma dentro del despacho, con segregación por identidad de persona

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-07 Programación y Despacho |
| **Actor** | ACT-07 Encargado de Combustible |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta confirmar si un Encargado de Delegación puede recibir el instrumento en nombre del motorista, previsto en `RN-32` numeral 3 y **no confirmado** (insumo #1), y el formato en papel de la constancia de recepción del vale (insumo #2) |

## Historia

**Como** Encargado de Combustible
**quiero** entregar el vale o el efectivo al motorista **dentro del acto de despacho** y contra su firma de recepción, con el sistema verificando que yo no sea quien despacha ni quien liquidará
**para** que el dinero público no salga de custodia antes de que el vehículo salga, y para que quien entrega no sea nunca quien declara en qué se gastó

## Contexto

Emisión y entrega son **dos momentos distintos**. En `PROGRAMADA` el instrumento existe con folio pero no sale de la custodia del Encargado de Combustible; entregarlo antes dejaría fondo público en manos de alguien cuya misión aún puede no despacharse — que es exactamente [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md).

La incompatibilidad *entrega fondo × liquida* (`I-10`) es **núcleo irreductible**: no la levanta ningún régimen de excepción, ninguna delegación y ninguna resolución de la máxima autoridad. Quien entrega el dinero no puede ser quien declara en qué se gastó.

## Reglas que la gobiernan

- [RN-32](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) — La entrega ocurre dentro del despacho, contra firma del motorista de la orden
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — `I-08` despacha × entrega fondo · `I-10` entrega fondo × liquida · `I-11` motorista × entrega fondo de su propia misión
- [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) — Constancia de recepción como parte del folio
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — El despacho traslada también la custodia del vehículo, con acta
- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — El bloqueo ofrece escalamiento en el acto, para no trabar el despacho

## Casos especiales que la afectan

- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Por qué la entrega no se anticipa
- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — Despacho con la marca *sin fondo asignado*
- [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) — Receptor distinto por relevo declarado

## Criterios de aceptación

```gherkin
# language: es
Característica: Entrega del instrumento de combustible dentro del despacho

  Antecedentes:
    Dado una asignación "ASG-2026-00812" en estado "EMITIDA" por "L 4,800.00" en vales
    Y una Orden de Misión "OM-2026-0512" en estado "PROGRAMADA" con salida prevista el "2026-09-24"
    Y un motorista titular "Wilmer Cáceres"
    Y "Nery Discua" como Encargado de Combustible y "Rosa Interiano" como Encargada de Despacho

  Escenario: Se rechaza entregar antes del despacho
    Dado que la Orden "OM-2026-0512" sigue en estado "PROGRAMADA"
    Cuando "Nery Discua" intenta entregar el instrumento a "Wilmer Cáceres"
    Entonces el sistema rechaza la entrega
    Y muestra "La Orden de Misión OM-2026-0512 no ha sido despachada. La entrega del fondo ocurre dentro del acto de despacho, no antes."

  Escenario: Se bloquea si quien entrega es quien despacha
    Dado que "Rosa Interiano" ejecuta el despacho de "OM-2026-0512"
    Cuando "Rosa Interiano" intenta entregar el instrumento "ASG-2026-00812"
    Entonces el sistema rechaza la entrega y no guarda nada
    Y muestra "Rosa Interiano está ejecutando el despacho de OM-2026-0512. Quien despacha no puede entregar el fondo de la misma misión."
    Y el intento queda en la pista de auditoría con el par de incompatibilidad detectado

  Escenario: Se bloquea si quien entrega es el motorista de la misión
    Cuando "Wilmer Cáceres" intenta entregarse a sí mismo el instrumento "ASG-2026-00812"
    Entonces el sistema rechaza la entrega y no guarda nada
    Y muestra "Wilmer Cáceres es el motorista de OM-2026-0512. No puede entregar el fondo de su propia misión."

  Escenario: Se bloquea si quien entrega será quien liquide, sin excepción posible
    Dado que "Nery Discua" figura como liquidador designado de "OM-2026-0512"
    Cuando "Nery Discua" intenta entregar el instrumento "ASG-2026-00812"
    Entonces el sistema rechaza la entrega
    Y muestra "Nery Discua figura como liquidador de OM-2026-0512. Quien entrega el fondo no puede liquidar la misión. Esta incompatibilidad no admite excepción."
    Y el sistema no ofrece opción de continuar con acuse, con delegación ni con resolución de la máxima autoridad

  Escenario: El bloqueo ofrece escalamiento y la misión no queda trabada
    Cuando el sistema bloquea la entrega por incompatibilidad
    Entonces ofrece el puesto superior de la misma unidad, el puesto de sede designado como respaldo de la delegación, o Gerencia Administrativa
    Y la entrega queda visiblemente pendiente en la bandeja del puesto elegido

  Escenario: Se rechaza entregar a quien no es el motorista de la orden
    Cuando "Nery Discua" intenta entregar el instrumento "ASG-2026-00812" a "Óscar Banegas"
    Entonces el sistema rechaza la entrega
    Y muestra "Óscar Banegas no es el motorista titular ni de relevo declarado de OM-2026-0512."

  Escenario: Se entrega contra firma dentro del despacho
    Dado que "Rosa Interiano" está ejecutando el despacho de "OM-2026-0512"
    Cuando "Nery Discua" entrega "L 4,800.00" en vales a "Wilmer Cáceres" contra firma de recepción
    Entonces la asignación "ASG-2026-00812" pasa a estado "ENTREGADA"
    Y queda congelado el monto entregado de "L 4,800.00" junto con la misión
    Y quedan registrados quién entregó, quién recibió, quién despachó y la marca de tiempo
    Y la Orden de Misión puede pasar a "DESPACHADA"

  Escenario: Se entrega al motorista de relevo declarado en la programación
    Dado un motorista de relevo "Marlon Zelaya" declarado en la programación de "OM-2026-0512" con su licencia verificada
    Cuando "Nery Discua" entrega el instrumento a "Marlon Zelaya" contra firma
    Entonces el sistema acepta la entrega
    Y registra al receptor como motorista de relevo, con la verificación de su licencia
```

## Fuera de alcance

- La emisión de la asignación — es [HU-076](HU-076-emitir-la-asignacion-de-combustible-con-folio.md)
- El acta de entrega del vehículo con odómetro, herramientas y documentos: pertenece al despacho (M-07/M-08)
- La revalidación de licencia y documentación al despachar: pertenece al despacho — la fuente del dato de licencia es [HU-105](HU-105-capturar-la-licencia-como-dato-propio-de-sigti.md)
- La firma electrónica certificada: **no se usa**. La constancia es la firma física sobre el impreso más el registro del usuario autenticado

## Notas y pendientes

- `[C]` **¿Puede un Encargado de Delegación recibir el instrumento en nombre del motorista?** Previsto en [`RN-32`](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) numeral 3, **no confirmado** — insumo **#1**
- `[C]` Formato en papel de la constancia de recepción del vale — insumo **#2**
- `[P]` La segregación de funciones proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) (MARCI/TSC); la norma existe, el articulado no se pudo extraer. **No se eleva el nivel**
