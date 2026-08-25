# HU-074 — Registrar la entrega del fondo y su custodia, y el traspaso por rotación de personal

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor** | ACT-08 Gerencia Administrativa (entrega) · ACT-07 Encargado de Combustible (recibe) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — bloqueada por `PROP-01` / insumo #7: si la orden de pago lleva folio preimpreso o lo genera el sistema determina si el traspaso lista folios propios o folios de terceros. Falta también el formato en papel del acta de entrega y traspaso de fondo (insumo #2) |

## Historia

**Como** Gerencia Administrativa
**quiero** registrar a quién entrego el efectivo o las órdenes de pago del fondo aprobado, y que esa custodia se traspase con acta y saldo verificado cuando la persona rote
**para** que en cualquier momento se pueda decir quién responde por el dinero público que está fuera de caja

## Contexto

Entre la aprobación del fondo y la emisión del primer vale hay un acto que hoy no queda en ningún sistema: la entrega física del instrumento. Es el momento en que el dinero sale de Administración y entra a la custodia del Encargado de Combustible.

El fondo **no se cierra por rotación de personal**. Se traspasa con acta y saldo verificado, exactamente como se traspasa la custodia de un vehículo.

## Reglas que la gobiernan

- [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) — La entrega deja el instrumento bajo custodia de ACT-07 y activa el saldo disponible del fondo
- [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) — Modelo de acta de entrega-recepción aplicado al fondo en la rotación
- [RN-88](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) — La alerta de agotamiento se activa sobre el saldo proyectado, no sobre el contable
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Autor, puesto, momento y huella del contenido en cada acto
- [RN-86](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) — El arqueo por persona es lo que se verifica en el traspaso

## Casos especiales que la afectan

- [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — El agotamiento se anuncia sobre el proyectado, antes del contable
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — Saldos afuera al momento del traspaso

## Criterios de aceptación

```gherkin
# language: es
Característica: Entrega del fondo aprobado y custodia del instrumento

  Antecedentes:
    Dado un fondo "FND-2026-09-004" aprobado por "L 200,000.00" el "2026-09-16"
    Y una composición de "L 60,000.00" en efectivo y "L 140,000.00" en órdenes de pago
    Y "Nery Discua" designado como Encargado de Combustible del ámbito

  Escenario: Se rechaza emitir asignaciones antes de registrar la entrega
    Dado que el fondo "FND-2026-09-004" está en estado "APROBADO"
    Cuando el Encargado de Combustible intenta emitir una asignación contra ese fondo
    Entonces el sistema rechaza la emisión
    Y muestra "El fondo FND-2026-09-004 está aprobado pero no entregado. Registre la entrega del instrumento antes de emitir."

  Escenario: Se rechaza la entrega sin desglose por instrumento
    Cuando la Gerencia Administrativa registra la entrega de "L 200,000.00" sin desglosar efectivo y órdenes de pago
    Entonces el sistema rechaza el registro
    Y muestra "Desglose la entrega por instrumento: efectivo y órdenes de pago se concilian por separado."

  Escenario: Se registra la entrega y el fondo queda disponible
    Cuando la Gerencia Administrativa registra la entrega a "Nery Discua" de "L 60,000.00" en efectivo y "L 140,000.00" en órdenes de pago
    Entonces el fondo pasa a estado "ENTREGADO"
    Y el saldo disponible publicado es "L 200,000.00"
    Y queda registrado que "Nery Discua" tiene la custodia física desde el "2026-09-16"

  Escenario: La alerta de agotamiento se dispara sobre el proyectado
    Dado un saldo contable de "L 45,000.00" y misiones aprobadas sin asignación emitida por "L 38,000.00"
    Y un umbral "alerta_saldo_proyectado" del "20" por ciento del monto aprobado
    Cuando el sistema evalúa el fondo "FND-2026-09-004"
    Entonces se dispara la alerta de agotamiento
    Y muestra "Saldo proyectado L 7,000.00 sobre un fondo de L 200,000.00. Solicite ampliación."

  Escenario: Se bloquea el cierre de la asignación de puesto con custodia viva
    Dado que "Nery Discua" custodia "L 22,300.00" en efectivo y 14 vales emitidos sin entregar
    Cuando se intenta cerrar la asignación de puesto de "Nery Discua" por traslado
    Entonces el sistema bloquea el cierre
    Y muestra "Nery Discua custodia L 22,300.00 en efectivo y 14 vales emitidos del fondo FND-2026-09-004. Levante el acta de traspaso antes de cerrar la asignación de puesto."

  Escenario: El traspaso por rotación no cierra el fondo
    Cuando la Gerencia Administrativa registra el traspaso de custodia de "Nery Discua" a "Karla Ordóñez" con acta y saldo verificado de "L 22,300.00" y 14 folios de vale listados uno por uno
    Entonces el fondo "FND-2026-09-004" permanece en estado "ENTREGADO"
    Y la custodia queda a nombre de "Karla Ordóñez" desde la fecha del acta
    Y el registro anterior no se sobrescribe: se cierra su rango y se abre el nuevo

  Escenario: Se rechaza el traspaso sin listar los folios de vale bajo custodia
    Cuando la Gerencia Administrativa registra el traspaso de custodia sin listar los folios de vale emitidos y no entregados
    Entonces el sistema rechaza el traspaso
    Y muestra "Liste uno por uno los folios de vale bajo custodia. Un acta que solo dice un monto no permite verificar nada después."
```

## Fuera de alcance

- La aprobación del fondo — es [HU-072](HU-072-aprobar-fondo-verificando-cuota-trimestral.md)
- La emisión de asignaciones contra el fondo — es [HU-076](HU-076-emitir-la-asignacion-de-combustible-con-folio.md)
- El arqueo de caja física: SIGTI registra el hecho, no reemplaza el control de tesorería de la institución
- La entrega del instrumento **al motorista**, que ocurre dentro del despacho — es [HU-079](HU-079-entregar-el-fondo-contra-firma-dentro-del-despacho.md)

## Notas y pendientes

- `[C]` **¿La orden de pago tiene folio preimpreso o lo genera el sistema?** Determina si el traspaso lista folios propios o folios de terceros — insumo **#7 / `PROP-01`**
- `[C]` Formato en papel del acta de entrega y traspaso de fondo que usa hoy la institución — insumo **#2**
- `[P]` El control sobre fondos entregados a servidores públicos proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), con articulado no extraído
