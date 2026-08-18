# HU-087 — Registrar el consumo sin comprobante con causa tipificada, y bloquear el comprobante ya usado

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-13 Liquidación y Cierre |
| **Actor** | ACT-06 Motorista (registra) · ACT-04 Jefe de Transporte (resuelve al liquidar) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** poder registrar un consumo cuando la estación no emitió factura o el comprobante se perdió, declarando la causa y aportando la evidencia que sí tengo
**para** que el hecho quede registrado en lugar de desaparecer, y para que el sistema me impida usar dos veces el mismo comprobante

## Contexto

**El consumo se registra igual.** Lo que se captura es la causa tipificada de la ausencia y la suficiencia probatoria de lo que sí hay: fotografía del surtidor, del odómetro, ubicación y hora. Bloquear el registro por un comprobante faltante no hace aparecer el comprobante: hace que el consumo se omita o se acomode a otro vale.

Del otro lado está el control barato: **todo comprobante es único en la institución por emisor y número**, y su reutilización se bloquea al registrarlo. El control barato se ejecuta al registrar; el caro, ocho meses después conciliando a mano.

## Reglas que la gobiernan

- [RN-85](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) — Causa tipificada, fuerza probatoria y descargo alternativo con folio
- [RN-84](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md) — Unicidad institucional por emisor y número; bloqueo al registrar
- [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) — La fotografía del comprobante es parte del registro del consumo
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — El duplicado detectado al sincronizar abre conflicto, no sobrescribe
- [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) — La ausencia cuenta para el criterio de hallazgo al cerrar

## Casos especiales que la afectan

- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — Eje de la historia
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — El comprobante llega después con la digitación diferida
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — El consumo sin comprobante válido se resuelve como faltante o como descargo alternativo

## Criterios de aceptación

```gherkin
# language: es
Característica: Consumo sin comprobante y unicidad institucional del comprobante

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "EN_RUTA"
    Y un comprobante "Factura 00341, emisor Uno Zambrano" ya registrado en la misión "OM-2026-0468"

  Escenario: Se rechaza registrar el consumo sin comprobante y sin causa declarada
    Cuando el motorista registra "12.0" galones por "L 1,560.00" sin comprobante y sin declarar la causa
    Entonces el sistema rechaza el registro
    Y muestra "Declare la causa de la ausencia del comprobante: la estación no emite factura, el comprobante se extravió, el comprobante es ilegible, o el pago fue con tag."

  Escenario: El consumo se registra con causa tipificada y evidencia sustitutiva
    Cuando el motorista registra "12.0" galones por "L 1,560.00" con causa "la estación no emite factura", fotografía del surtidor, fotografía del odómetro en "84,730" km, ubicación y hora
    Entonces el sistema acepta el registro
    Y marca la fuerza probatoria como "sin comprobante, con evidencia gráfica del surtidor y del odómetro"
    Y muestra "Registrado sin comprobante. La causa y la evidencia quedan en el expediente y contarán en la evaluación de cierre."

  Escenario: Se rechaza reutilizar un comprobante ya registrado
    Cuando el motorista registra un consumo con el comprobante "Factura 00341, emisor Uno Zambrano"
    Entonces el sistema rechaza el registro
    Y muestra "La factura 00341 de Uno Zambrano ya sostiene un consumo de L 1,430.00 en la misión OM-2026-0468 del 12/09/2026."

  Escenario: El duplicado capturado sin red se detecta al sincronizar
    Dado un dispositivo sin conectividad que no puede verificar la unicidad institucional
    Cuando el dispositivo sincroniza y el comprobante resulta duplicado
    Entonces el sistema no sobrescribe ninguno de los dos registros
    Y abre un conflicto en la cola de resolución del Jefe de Transporte con ambos consumos lado a lado
    Y el duplicado alimenta el criterio de hallazgo de la misión

  Escenario: El consumo con comprobante inválido no se elimina
    Dado un consumo de "L 1,560.00" cuyo comprobante resultó duplicado
    Cuando el Jefe de Transporte resuelve el conflicto al liquidar
    Entonces el consumo permanece registrado
    Y queda marcado como "sin comprobante válido"
    Y se resuelve como faltante o como descargo alternativo, con hallazgo

  Escenario: Descargo alternativo con folio dentro del tope configurado
    Dado un parámetro "tope_descargo_alternativo" de "L 500.00"
    Cuando el Jefe de Transporte emite un descargo alternativo por "L 420.00" para un consumo sin comprobante
    Entonces el sistema acepta el descargo con folio, motivo y autor
    Y la fuerza probatoria del consumo pasa a "descargo alternativo con folio"

  Escenario: Se rechaza el descargo alternativo por encima del tope
    Cuando el Jefe de Transporte emite un descargo alternativo por "L 1,560.00"
    Entonces el sistema rechaza el descargo
    Y muestra "El descargo alternativo admite hasta L 500.00. Por encima de ese monto el consumo se resuelve como faltante con obligación de reintegro."

  Escenario: La falta de ticket de peaje advierte y no bloquea el cierre
    Dado un paso por caseta de "L 22.00" sin fotografía del ticket, con causa "la caseta no entregó ticket"
    Cuando el Jefe de Transporte liquida la misión
    Entonces el sistema no bloquea la liquidación
    Y advierte "1 paso por caseta sin ticket. Cuenta para el criterio de hallazgo al cerrar."
```

## Fuera de alcance

- La captura general del abastecimiento — es [HU-082](HU-082-registrar-abastecimiento-sin-conectividad.md)
- La tipificación del faltante y la obligación de reintegro — es [HU-089](HU-089-conciliar-el-fondo-y-tipificar-sobrante-y-faltante.md)
- La clasificación de cierre con hallazgo — es [HU-094](HU-094-cerrar-con-hallazgo-tipificado.md)
- La validación fiscal del comprobante contra el SAR: no hay integración disponible

## Notas y pendientes

- `[C]` **¿Admite la institución una constancia como descargo alternativo, con qué tope y con qué umbral de hallazgo?** — insumo **#1**, con Auditoría Interna. Los parámetros `tope_descargo_alternativo` y `umbral_hallazgo_sin_descargo` se entregan **vacíos**
- `[C]` Criterio sobre comprobante ilegible: ¿equivale a ausencia o admite verificación posterior? — insumo **#19**
- `[C]` `clave_unicidad_comprobante` por tipo de comprobante: qué combinación de emisor y número identifica unívocamente cada tipo — insumo **#1**
- `[V]` Que el ticket de peaje faltante debe advertir sin bloquear el cierre — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §9: *bloquear la liquidación por eso hace que el sistema se abandone*
