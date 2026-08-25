# HU-085 — Registrar el paso por caseta y marcar la discrepancia de clasificación sin reclasificar al vehículo

| Campo | Valor |
|---|---|
| **Módulo** | M-18 Peajes · M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el contenido literal del Artículo 51 de la Ley de Tránsito (insumo #23), la lista oficial de exoneraciones (insumo #22), si la institución opera con tags (insumo #24) y qué se acepta hoy como descargo de peaje: ticket de caseta, declaración o ninguno (insumo #1) |

## Historia

**Como** Motorista
**quiero** registrar cada paso por caseta con la categoría y el monto que efectivamente me cobraron, aunque no coincidan con lo esperado y aunque no tenga señal
**para** que la diferencia quede documentada en el momento y sirva para reclamar, sin que nadie me descuente el sobrecosto ni cambie por eso la clasificación del vehículo

## Contexto

Dos decisiones de diseño sostienen esta historia y ninguna es opcional.

**La discrepancia nunca modifica la categoría del vehículo.** Un sistema que "aprende" del cobro de la caseta convierte el error de la caseta en la verdad institucional, y en tres meses el reclamo ya no ocurre nunca. La categoría es derivación de la ficha técnica y de la norma; el cobro es un hecho a registrar.

**El sobrecosto no se le imputa al motorista.** Si teme que le descuenten la diferencia, no la va a declarar: va a acomodar la suma para que cuadre. La no imputabilidad es la condición para que el dato exista.

## Reglas que la gobiernan

- [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) — La discrepancia se marca sola, **nunca** modifica la categoría del vehículo, y el sobrecosto no se imputa al motorista
- [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) — Tarifa como (punto × categoría × vigencia), resuelta a la fecha del hecho
- [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md) — La categoría se deriva de la ficha técnica
- [RN-38](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md) — El paso exonerado se registra igual, con monto cero y fundamento
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Captura en caseta sin conectividad
- [RN-92](../../01-negocio/reglas/RN-92-reclamo-por-discrepancia-de-peaje.md) — Las discrepancias alimentan un expediente de reclamo por punto, clase y período

## Casos especiales que la afectan

- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — Eje de la historia
- [CE-25](../casos-especiales/CE-25-comprobante-perdido-o-estacion-sin-factura.md) — El ticket puede no existir o no indicar la categoría
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — Pasos adicionales por destinos agregados en ruta

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro del paso por punto de peaje y discrepancia de clasificación

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "EN_RUTA"
    Y un vehículo "TR-0045" con categoría de peaje asignada "Liviano/Turismo"
    Y una tarifa esperada verificada de "L 22.00" en el punto "Zambrano"

  Escenario: Se registra la discrepancia cuando cobran categoría superior
    Cuando el motorista registra el paso por "Zambrano" con categoría cobrada "Vehículo de 2 Ejes" y monto "L 90.00"
    Entonces el sistema acepta el registro
    Y marca una discrepancia de clasificación con sobrecosto de "L 68.00"
    Y muestra "Registrado. Cobro en categoría Vehículo de 2 Ejes (L 90.00) frente a la asignada Liviano/Turismo (L 22.00). Diferencia L 68.00 registrada como sobrecosto por clasificación."

  Escenario: La discrepancia no modifica la categoría del vehículo
    Cuando el sistema marca la discrepancia del paso por "Zambrano"
    Entonces la categoría de peaje de "TR-0045" sigue siendo "Liviano/Turismo"
    Y el sistema no ofrece la opción de actualizar la categoría del vehículo a partir del cobro

  Escenario: El sobrecosto no se imputa al motorista
    Cuando el sistema registra el sobrecosto de "L 68.00"
    Entonces lo tipifica como "sobrecosto por clasificación"
    Y no genera obligación de reintegro a cargo de "Wilmer Cáceres"
    Y no genera ninguna marca de falta contra el motorista

  Escenario: El cobro en categoría inferior también se registra
    Cuando el motorista registra el paso por "Siguatepeque" con categoría cobrada "Montacargas Liviano" y monto "L 11.00"
    Entonces el sistema acepta el registro
    Y marca la discrepancia con diferencia de "L -11.00"
    Y muestra "Cobro por debajo de la categoría asignada. Registrado: callarlo expone a la institución a un cobro retroactivo."

  Escenario: El motorista no sabe con qué categoría le cobraron
    Cuando el motorista registra el paso por "Yojoa" con monto "L 90.00" sin declarar la categoría cobrada
    Entonces el sistema acepta el registro
    Y deriva la categoría probable "Vehículo de 2 Ejes" contra la tabla del punto a la fecha del paso
    Y la marca como "inferida, no declarada"

  Escenario: El remolque declarado no produce discrepancia falsa
    Dado que la programación de "OM-2026-0512" declara configuración "con remolque"
    Cuando el motorista registra el paso por "Zambrano" con categoría cobrada "Vehículo de 3 Ejes" y monto "L 134.00"
    Entonces el sistema no marca discrepancia
    Y registra el cobro como conforme a la configuración declarada para la misión

  Escenario: El paso exonerado se registra con monto cero y fundamento
    Dado un vehículo "TR-0090" con exoneración vigente en "Zambrano"
    Cuando el motorista registra el paso por "Zambrano"
    Entonces el sistema registra el paso con monto "L 0.00" y el fundamento de la exoneración
    Y muestra "El paso se registra aunque no haya cobro: sin él se rompe la secuencia de casetas que se concilia después."

  Escenario: Se rechaza el registro sin evidencia ni causa de su ausencia
    Cuando el motorista registra el paso por "Zambrano" sin fotografía del ticket y sin declarar por qué no la tiene
    Entonces el sistema rechaza el registro
    Y muestra "Adjunte la fotografía del ticket o declare la causa de su ausencia: la caseta no entregó ticket, se extravió, o el pago fue con tag."

  Escenario: El registro funciona sin conectividad, con el paquete congelado a bordo
    Dado un dispositivo sin señal desde hace "31" horas
    Cuando el motorista abre "registrar paso por peaje"
    Entonces el formulario precarga punto, categoría asignada con su fundamento y tarifa esperada desde el paquete congelado
    Y admite guardar el registro sin conexión

  Escenario: Con tag prepago la evidencia es el estado de cuenta
    Dado un vehículo con tag prepago asignado
    Cuando el motorista registra el paso con medio de pago "tag prepago"
    Entonces el sistema no exige fotografía del ticket
    Y marca la evidencia como "pendiente de conciliación contra estado de cuenta"
```

## Fuera de alcance

- La conciliación estimado contra pagado y la coherencia de la secuencia de casetas — es [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)
- El tratamiento de un punto con tarifa no verificada — es [HU-086](HU-086-no-emitir-discrepancia-sobre-tarifa-no-verificada.md)
- La presentación del reclamo ante la SAPP: SIGTI arma el expediente; la gestión es de la institución
- La derivación de la categoría del vehículo — es [HU-098](HU-098-completar-la-ficha-tecnica-que-habilita.md)

## Notas y pendientes

- 🔴 `[C]` **bloqueante — tarifa efectivamente vigente no confirmada** (insumo **#21**). Mientras la tarifa de un punto esté marcada como no verificada, la detección de discrepancia sobre ese punto es **no concluyente** — ver [HU-086](HU-086-no-emitir-discrepancia-sobre-tarifa-no-verificada.md)
- `[V]` Que la SAPP resolvió el 17/09/2025 que Hyundai H-100, Kia K2700 y Mercedes-Benz Sprinter se clasifican como livianos conforme al Art. 51 — [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §2
- `[C]` Contenido literal del Artículo 51 de la Ley de Tránsito — insumo **#23**
- `[C]` Lista oficial de exoneraciones — insumo **#22**
- `[C]` Si la institución tiene tags CoviPass y si COVI-H emite estado de cuenta empresarial a su nombre — insumo **#24**
- `[C]` ¿Qué se acepta hoy como descargo de peaje: ticket de caseta, declaración, ninguna? — insumo **#1**, [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) §8
