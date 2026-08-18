# HU-050 — Marcar la discrepancia de clasificación en caseta y abrir el reclamo

| Campo | Valor |
|---|---|
| **Módulo** | M-18 Peajes · M-13 Liquidación y Cierre |
| **Actor** | ACT-06 Motorista marca la discrepancia · ACT-04 Jefe de Transporte gestiona el reclamo |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** marcar con un toque, en la caseta misma, que me cobraron una categoría distinta a la que trae impresa mi Orden de Misión
**para** que el sobrecosto no me lo descuenten a mí en la liquidación y para que la institución pueda reclamarlo con el ticket como prueba

## Contexto

La discrepancia se detecta **donde ocurre**, no dos meses después conciliando a mano. Si el motorista no la marca en el momento, en la liquidación aparece un peaje pagado de más sin explicación, y quien responde por la diferencia es él.

**El ticket es la única prueba del reclamo.** Sin fotografía del ticket con la categoría cobrada impresa, no hay reclamo defendible ante el concesionario ni ante la SAPP.

El reclamo **es un objeto con estado y resultado económico propio**: la discrepancia no se cierra porque alguien la mire, se cierra cuando el reclamo llega a un desenlace ([RN-92](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md)).

## Reglas que la gobiernan

- [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) — La discrepancia de clasificación se registra en el punto donde ocurre, con la categoría cobrada
- [RN-92](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) — **Regla rectora**: el reclamo es objeto con estado y resultado económico; las discrepancias no cierran sin él
- [RN-91](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) — La tarifa esperada impresa es el término contra el que se compara
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — La categoría del vehículo se deriva de su ficha técnica, no se digita por misión
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La marca se hace sin conectividad

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Cobro en categoría de peaje equivocada
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — El ticket que sostenía el reclamo se perdió

## Criterios de aceptación

```gherkin
# language: es
Característica: Discrepancia de clasificación en caseta y su reclamo

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un vehículo "Camión Isuzu FVR" con categoría de peaje "3 ejes" y tarifa esperada de "L 90.00" en "Peaje Jícaro Galán"
    Y que el dispositivo lleva 2 días sin conectividad

  Escenario: Se rechaza marcar discrepancia sin la categoría que cobraron
    Cuando "José Martínez" marca discrepancia en "Peaje Jícaro Galán" sin indicar la categoría cobrada
    Entonces el sistema rechaza la marca
    Y muestra "Indique en qué categoría le cobraron. Sin ese dato no se puede reclamar al concesionario."

  Escenario: Se rechaza marcar discrepancia sin fotografía del ticket
    Cuando "José Martínez" marca discrepancia en "Peaje Jícaro Galán" con categoría cobrada "4 ejes" y sin fotografía del ticket
    Entonces el sistema rechaza la marca
    Y muestra "Conserve el ticket y tómele foto. Es la única prueba del reclamo. Si la caseta no lo entregó, declárelo como comprobante ausente."

  Escenario: Se rechaza liquidar la misión con la discrepancia sin reclamo abierto
    Dada una discrepancia registrada en "Peaje Jícaro Galán" por "L 30.00"
    Cuando el Jefe de Transporte intenta liquidar la Orden de Misión "OM-2026-0451"
    Entonces el sistema rechaza la liquidación
    Y muestra "Hay 1 discrepancia de peaje por L 30.00 sin reclamo abierto. Abra el reclamo o declare por qué no se reclama."

  Escenario: Marca de discrepancia en la caseta, sin señal
    Cuando "José Martínez" marca discrepancia en "Peaje Jícaro Galán" con categoría cobrada "4 ejes", monto cobrado "L 120.00" y fotografía del ticket
    Entonces el sistema registra la discrepancia con diferencia de "L 30.00" contra la tarifa esperada
    Y no descuenta el sobrecosto al motorista en la liquidación mientras el reclamo esté vivo
    Y deja el registro en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: El reclamo se resuelve a favor de la institución
    Dado un reclamo abierto por "L 30.00" sobre "Peaje Jícaro Galán"
    Cuando el Jefe de Transporte registra el desenlace "reintegro recibido" por "L 30.00"
    Entonces el reclamo pasa a estado "CERRADO CON REINTEGRO"
    Y la discrepancia deja de bloquear la liquidación de "OM-2026-0451"

  Escenario: El reclamo se resuelve en contra y la categoría del vehículo estaba mal
    Dado un reclamo abierto por "L 30.00" sobre "Peaje Jícaro Galán"
    Cuando el Jefe de Transporte registra el desenlace "reclamo rechazado: el vehículo clasifica en 4 ejes"
    Entonces el reclamo pasa a estado "CERRADO SIN REINTEGRO"
    Y el sistema advierte "La categoría de peaje registrada en la ficha técnica de Camión Isuzu FVR podría estar equivocada. Revísela antes del próximo despacho."
```

## Fuera de alcance

- La corrección de la categoría de peaje en la ficha técnica del vehículo — es de M-03
- El trámite externo ante la SAPP o el concesionario: el sistema registra el reclamo y su desenlace, no lo tramita
- La conciliación contra el estado de cuenta del concesionario — depende del insumo #75

## Notas y pendientes

- `[C]` ¿Ante quién se presenta el reclamo, con qué formato y en qué plazo? La ficha [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) no cubre el procedimiento de reclamación
- `[C]` ¿Quién por puesto queda responsable del reclamo en la institución? — sin dueño, el reclamo nace muerto
- `[P]` [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) sostiene la clasificación por ejes y la existencia de discrepancias; el articulado no se pudo extraer
