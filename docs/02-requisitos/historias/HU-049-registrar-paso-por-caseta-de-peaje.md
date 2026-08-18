# HU-049 — Registrar el paso por caseta de peaje contra la tarifa esperada

| Campo | Valor |
|---|---|
| **Módulo** | M-18 Peajes · M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** confirmar con un toque el paso por cada caseta, con la categoría y la tarifa que ya vienen impresas en mi Orden de Misión, y fotografiar el ticket
**para** descargar el gasto de peaje con evidencia y no tener que discutir en la liquidación cuánto debí pagar en cada punto

## Contexto

El país tiene puntos de peaje cuya tarifa clasifica por **número de ejes** ([NRM-10](../../01-negocio/normativa/NRM-10-peajes.md)). Cada vehículo de la flota tiene su categoría de peaje resuelta desde su ficha técnica, y esa categoría con su tarifa esperada **va impresa en la Orden de Misión que el motorista lleva en la mano** ([RN-91](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)).

El motorista está en la caseta, sin señal, con el cobrador esperando. El registro tiene que ser confirmar o corregir un monto ya en pantalla, indicar medio de pago y tomar la foto del ticket. Nada más.

**La falta de un ticket de caseta advierte pero no bloquea el cierre.** Bloquear por eso hace que el sistema se abandone (`PC-14`, [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md)).

## Reglas que la gobiernan

- [RN-91](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) — La Orden impresa lleva, por punto, la categoría asignada y la tarifa esperada del paquete congelado
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — La tarifa se resuelve por punto, categoría y vigencia
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — Se calcula con el paquete congelado y con la tabla vigente a la fecha del hecho
- [RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) — Una secuencia de casetas imposible produce hallazgo al conciliar, pero nunca impide el registro
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Se registra sin ninguna conectividad
- [RN-84](../../01-negocio/reglas/RN-84-unicidad-del-comprobante-en-la-institucion.md) — Un mismo ticket no puede sostener dos registros en la institución

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Cobro en categoría equivocada — tratado en [HU-050](HU-050-discrepancia-de-peaje-y-reclamo.md)
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — Ticket perdido o caseta que no lo entrega — tratado en [HU-052](HU-052-ausencia-de-comprobante-y-gasto-imprevisto.md)

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro del paso por caseta de peaje en ruta

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un vehículo "Camión Isuzu FVR" con categoría de peaje "3 ejes" resuelta de su ficha técnica
    Y un paquete de misión congelado el "2026-05-12" con el punto "Peaje Jícaro Galán" y tarifa esperada de "L 90.00" para 3 ejes
    Y que el dispositivo lleva 2 días sin conectividad

  Escenario: Se rechaza registrar el mismo ticket dos veces en la institución
    Dado un ticket con emisor "COVI-H" y número "A-4471928" ya registrado en la Orden de Misión "OM-2026-0430"
    Cuando "José Martínez" registra el paso por "Peaje Jícaro Galán" con ese mismo número de ticket
    Entonces el sistema rechaza el registro
    Y muestra "El ticket COVI-H A-4471928 ya está registrado en la Orden de Misión OM-2026-0430 del 03/05/2026. Verifique el número impreso."

  Escenario: Se rechaza el registro sin medio de pago
    Cuando "José Martínez" confirma el paso por "Peaje Jícaro Galán" sin indicar medio de pago
    Entonces el sistema rechaza el registro
    Y muestra "Indique cómo pagó: efectivo del fondo de la misión, tag institucional o de su propio bolsillo. De eso depende a quién se le descarga o se le reintegra."

  Escenario: Confirmación de la tarifa esperada, sin señal
    Cuando "José Martínez" confirma el paso por "Peaje Jícaro Galán" con monto "L 90.00", medio de pago "efectivo del fondo" y fotografía del ticket
    Entonces el sistema registra el paso con hora del hecho y categoría "3 ejes"
    Y usa la tarifa del paquete congelado, no la tabla actual del servidor
    Y deja el registro en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: Paso por un punto de peaje que no venía en el paquete
    Cuando "José Martínez" registra un paso por el punto no previsto "Peaje Río Nance" con monto "L 45.00" y fotografía del ticket
    Entonces el sistema acepta el registro con el nombre tal como aparece en el ticket
    Y lo marca como "punto no previsto, pendiente de resolver contra el catálogo"
    Y la resolución del punto y su tarifa se hará al sincronizar con la tabla vigente al "2026-05-14"

  Escenario: Secuencia de casetas geográficamente imposible
    Dado un paso registrado por "Peaje Jícaro Galán" a las "10:20"
    Cuando "José Martínez" registra un paso por "Peaje Zambrano" a las "10:45"
    Entonces el sistema registra el paso sin impedirlo
    Y marca la secuencia para revisión con "El tiempo entre Jícaro Galán y Zambrano no alcanza para el recorrido. Se revisará al conciliar."
    Y genera el hallazgo "H-03" al conciliar la misión

  Escenario: Sin ticket, el paso se registra igual
    Cuando "José Martínez" registra el paso por "Peaje Jícaro Galán" con monto "L 90.00" y declara que la caseta no entregó ticket
    Entonces el sistema registra el paso
    Y marca el comprobante como "ausente con causa declarada", distinto de "pendiente de subir"
    Y advierte sin bloquear "Sin ticket este peaje se descarga con constancia. No impide cerrar la misión."
```

## Fuera de alcance

- La discrepancia de clasificación cobrada en caseta y su reclamo — es [HU-050](HU-050-discrepancia-de-peaje-y-reclamo.md)
- El cálculo del estimado de peajes al autorizar y su congelamiento — es de M-18 en la programación
- La conciliación de lo registrado contra el estado de cuenta del concesionario — depende del insumo #75

## Notas y pendientes

- `[C]` ¿La institución tiene tags de peaje? ¿A nombre de quién? ¿El concesionario emite factura fiscal en caseta o estado de cuenta empresarial? — insumo #24
- `[C]` ¿El peaje se financia con el viático de ARGOS o es gasto de misión separado? — insumo #25. Si va en el viático, este registro cambia de dueño
- `[P]` [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) sostiene la clasificación por número de ejes; el tarifario vigente por punto no se pudo extraer completo
