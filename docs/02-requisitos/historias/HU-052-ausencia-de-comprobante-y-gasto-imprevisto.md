# HU-052 — Registrar la ausencia de comprobante y el gasto imprevisto en ruta

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible · M-13 Liquidación y Cierre · M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** registrar igual el gasto cuando la estación no me dio factura, cuando el ticket se me borró, o cuando tuve que pagar una llanta en el camino
**para** que el hecho quede en el expediente con su causa declarada, en lugar de desaparecer del sistema por no tener el papel

## Contexto

**No registrar el galón para evitar la falta del papel es peor que la falta del papel.** El gasto ocurrió; el fondo público se movió. Un gasto que no se registra por miedo a la observación es un gasto que el auditor encuentra en la caja y no en el sistema, que es la peor forma de que aparezca.

El sistema distingue tres cosas que hoy se confunden en una sola: comprobante **pendiente de subir** —existe, la foto no ha sincronizado—, comprobante **ausente con causa** —no existe y se declara por qué— y comprobante **perdido** —existió y ya no ([RN-85](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md)).

El gasto imprevisto distinto de combustible —una llanta, un lavado obligatorio en un retén, un parqueo— tiene su propio registro con tipo, factura y la autorización del acto si la hubo ([RN-87](../../01-negocio/reglas/RN-87-gasto-imprevisto-en-ruta.md)).

## Reglas que la gobiernan

- [RN-85](../../01-negocio/reglas/RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) — **Regla rectora**: la ausencia de comprobante lleva causa tipificada y admite descargo alternativo con folio
- [RN-87](../../01-negocio/reglas/RN-87-gasto-imprevisto-en-ruta.md) — El gasto imprevisto se registra con tipo, factura y autorización del acto
- [RN-73](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) — El gasto ejecutado sin poder consultar se convalida después, con la cronología declarada tal como ocurrió
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Adjunto pendiente no es lo mismo que adjunto ausente
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Se registra sin ninguna conectividad

## Casos especiales que la afectan

- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — Comprobante perdido o estación sin factura
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — Sobrante o faltante al liquidar
- [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) — El acto ejecutado sin autorización previa que se convalida después

## Criterios de aceptación

```gherkin
# language: es
Característica: Comprobante ausente y gasto imprevisto en ruta

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un fondo de la misión con saldo de "L 2,850.00"
    Y que el dispositivo lleva 5 días sin conectividad

  Escenario: Se rechaza declarar comprobante ausente sin causa tipificada
    Cuando "José Martínez" registra un abastecimiento de "15.0" galones y declara el comprobante ausente sin seleccionar causa
    Entonces el sistema rechaza la declaración
    Y muestra "Seleccione por qué no hay comprobante: la estación no lo emitió, se perdió, se borró o quedó ilegible. La causa es lo que sostiene el descargo."

  Escenario: Se rechaza el gasto imprevisto sin tipo declarado
    Cuando "José Martínez" registra un gasto imprevisto de "L 1,200.00" sin indicar el tipo
    Entonces el sistema rechaza el registro
    Y muestra "Indique de qué fue el gasto: llanta, parqueo, lavado, grúa u otro. Sin tipo no se puede imputar al objeto del gasto."

  Escenario: Comprobante ausente por estación que no emite factura
    Cuando "José Martínez" registra un abastecimiento de "15.0" galones por "L 1,650.00" con causa de ausencia "la estación no emitió comprobante" y fotografía del surtidor y del odómetro
    Entonces el sistema registra el abastecimiento
    Y marca el comprobante como "AUSENTE con causa declarada"
    Y no lo marca como "PENDIENTE de subir"
    Y muestra "Registrado sin comprobante. Al liquidar deberá presentar descargo alternativo con folio."

  Escenario: Adjunto que existe pero todavía no ha subido
    Dado un abastecimiento registrado con fotografía del comprobante tomada en el dispositivo
    Cuando el Jefe de Transporte consulta el expediente de "OM-2026-0451" desde la sede
    Entonces el sistema muestra el comprobante como "PENDIENTE de envío desde el dispositivo, capturado el 14/05/2026"
    Y no lo presenta como ausente

  Escenario: Gasto imprevisto ejecutado sin poder consultar al autorizador
    Cuando "José Martínez" registra un gasto imprevisto de tipo "llanta" por "L 1,200.00", con factura fotografiada y la justificación "llanta reventada en el km 84, sin señal para consultar"
    Entonces el sistema registra el gasto con la marca "sin autorización previa, pendiente de convalidación"
    Y descuenta "L 1,200.00" del saldo del fondo de la misión
    Y la convalidación queda pendiente para la liquidación, con responsable y plazo

  Escenario: El comprobante nunca llega y la misión se cierra con hallazgo
    Dado un abastecimiento con comprobante declarado "PENDIENTE" el "2026-05-14"
    Cuando el Encargado de Combustible liquida la Orden de Misión "OM-2026-0451" sin que el adjunto haya llegado
    Entonces el sistema genera el hallazgo "H-08"
    Y muestra "El comprobante del abastecimiento del 14/05/2026 nunca llegó. La misión solo puede cerrarse con hallazgo."
```

## Fuera de alcance

- El formato del descargo alternativo con folio y su límite de monto — depende del insumo #1 y de Auditoría Interna
- La conciliación contra el estado de cuenta del proveedor de combustible — depende del insumo #75
- La aprobación del descargo: la decide quien liquida, no el motorista

## Notas y pendientes

- `[C]` ¿Se admite constancia como descargo? ¿Con qué tope de monto y qué umbral de hallazgo? — insumo #1, a Auditoría Interna
- `[C]` Quién convalida un acto ejecutado sin autorización previa y en qué plazo — insumo #32
- `[C]` Catálogo institucional de tipos de gasto imprevisto admitidos en ruta — insumo #1
