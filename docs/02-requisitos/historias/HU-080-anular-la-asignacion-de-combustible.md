# HU-080 — Anular la asignación de combustible cuando la misión se desprograma, se anula o se cancela con fondo ya entregado

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-07 Encargado de Combustible · ACT-08 Gerencia Administrativa |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Encargado de Combustible
**quiero** anular la asignación emitida con acta y asiento reverso, y que el sistema me impida anular cuando ya hubo cualquier consumo
**para** que un folio emitido nunca desaparezca del expediente y para que un movimiento de fondos públicos no se borre llamándolo cancelación

## Contexto

Es la situación más delicada del sistema: hay documentos con folio emitidos y, a veces, dinero público ya entregado. El caso típico es que el motorista llenó el tanque la tarde anterior y la misión se suspendió esa noche.

La distinción es dura: **si no hubo ningún consumo**, se devuelve todo con acta y la asignación va a `DEVUELTA` con asiento reverso. **Si hubo consumo, aunque sea un lempira**, no hay anulación posible: la misión se liquida aunque su kilometraje sea cero. Hubo movimiento de fondos públicos y anular sería borrar un hecho económico.

## Reglas que la gobiernan

- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Toda anulación es asiento reverso con referencia explícita, motivo, autor y autorizador
- [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) — El folio anulado no se recicla, no se reutiliza y no vuelve al rango
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — El plazo de devolución del saldo corre desde la anulación
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Los registros no se editan para que cuadren
- [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) — La misión con consumo se liquida, no se anula

## Casos especiales que la afectan

- [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) — Eje de la historia
- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — Desprogramación por indisponibilidad del vehículo
- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — Desprogramación por indisponibilidad del motorista

## Criterios de aceptación

```gherkin
# language: es
Característica: Anulación y devolución de la asignación de combustible

  Antecedentes:
    Dado una asignación "ASG-2026-00812" por "L 4,800.00" en 6 vales, folios "VC-01201" a "VC-01206"
    Y una Orden de Misión "OM-2026-0512"

  Escenario: Se anula la asignación EMITIDA al desprogramar la misión
    Dado que "ASG-2026-00812" está en estado "EMITIDA" y bajo custodia del Encargado de Combustible
    Cuando la Orden de Misión "OM-2026-0512" se desprograma
    Entonces la asignación "ASG-2026-00812" pasa a estado "ANULADA"
    Y se levanta acta de anulación con folio, motivo y autor
    Y el folio "ASG-2026-00812" queda anulado con referencia cruzada a "OM-2026-0512"
    Y el folio no vuelve al rango disponible

  Escenario: Se rechaza reutilizar un folio anulado
    Dado que "ASG-2026-00812" está en estado "ANULADA"
    Cuando el Encargado de Combustible intenta emitir una nueva asignación con el folio "ASG-2026-00812"
    Entonces el sistema rechaza la emisión
    Y muestra "El folio ASG-2026-00812 fue anulado el 23/09/2026 con referencia a OM-2026-0512. Los folios no se reciclan."

  Escenario: Se rechaza anular una asignación ENTREGADA que ya tuvo consumo
    Dado que "ASG-2026-00812" está en estado "ENTREGADA"
    Y un abastecimiento registrado de "8.0" galones por "L 1,040.00" el "2026-09-23"
    Cuando la Gerencia Administrativa intenta anular la misión "OM-2026-0512"
    Entonces el sistema rechaza la anulación
    Y muestra "La misión OM-2026-0512 registra consumo de L 1,040.00 el 23/09/2026. No se anula: se liquida, aunque su kilometraje sea cero."
    Y el sistema encamina la misión hacia RETORNADA para su liquidación

  Escenario: Devolución íntegra sin ningún consumo
    Dado que "ASG-2026-00812" está en estado "ENTREGADA" y no registra ningún consumo
    Cuando el motorista devuelve los 6 vales y se levanta acta firmada por "Wilmer Cáceres" y "Nery Discua"
    Entonces la asignación pasa a estado "DEVUELTA"
    Y el acta lista los folios "VC-01201" a "VC-01206" uno por uno
    Y se registra el asiento reverso con referencia explícita al asiento de entrega
    Y el saldo del fondo se restituye en "L 4,800.00"

  Escenario: Se rechaza el acta de devolución sin listar folio por folio
    Cuando se registra la devolución indicando únicamente el monto "L 4,800.00"
    Entonces el sistema rechaza el acta
    Y muestra "Liste los folios de vale devueltos uno por uno. Un acta que solo dice un monto no permite verificar qué instrumento quedó afuera."

  Escenario: La misión anulada con fondo entregado permanece visible con sus pendientes
    Dado que la misión "OM-2026-0512" estaba "DESPACHADA" cuando se decidió anularla
    Cuando la Gerencia Administrativa inicia la anulación
    Entonces la misión permanece en estado "DESPACHADA" con la marca "anulación en trámite"
    Y muestra la lista de devoluciones pendientes: instrumento de combustible, custodia del vehículo y documentos impresos
    Y no se crea ningún estado intermedio

  Escenario: El plazo de devolución corre desde la anulación
    Dado un parámetro "plazo_devolucion_saldo" de "5" días hábiles
    Cuando la anulación se registra el "2026-09-23"
    Entonces el plazo de devolución vence el "2026-09-30"
    Y el sistema muestra "Devolución del saldo pendiente. Plazo hasta el 30/09/2026."
```

## Fuera de alcance

- La liquidación de la misión no ejecutada con consumo — es [HU-089](HU-089-conciliar-el-fondo-y-tipificar-sobrante-y-faltante.md)
- La anulación de la Orden de Misión en sí: pertenece a la máquina de estados de la Orden (M-07)
- El reintegro del faltante y su cobro: fuera de SIGTI

## Notas y pendientes

- `[C]` **¿La institución exige devolución o destrucción con acta de los documentos impresos anulados?** — insumo **#1**
- `[C]` `plazo_devolucion_saldo` en días hábiles — insumo **#32**
- `[C]` Formato en papel del acta de anulación y del acta de devolución — insumo **#2**
