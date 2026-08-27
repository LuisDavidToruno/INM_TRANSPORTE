# RN-103 — La matrícula del vehículo debe estar vigente durante todo el rango de la misión

| Campo | Valor |
|---|---|
| **Módulos** | M-04, M-03, M-07 |
| **Origen** | Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — matrícula y Registro Vehicular del Instituto de la Propiedad. Hallazgo `HN1-11` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) |
| **Verificación** | `[V]` que el **Instituto de la Propiedad** lleva el Registro Vehicular y que la matrícula es su documento — [NRM-06](../normativa/NRM-06-transito-y-licencias.md). `[V]` que la ficha exige **registrar** matrícula y **alertar** su vencimiento. **`[I]` que el vencimiento deba bloquear el despacho**: la ficha no lo dice y el articulado de la Ley de Tránsito **no se pudo extraer** — riesgo #17. Es decisión de control interno del equipo, no obligación legal citable. `[C]` el articulado |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — `bloqueo_matricula_vencida`, **encendido por defecto**. Es la configuración opuesta a la de [`RN-16`](RN-16-seguro-y-revision-mecanica.md), y abajo está el porqué |

## Por qué existe esta regla — hallazgo `HN1-11`

`BD-03` declaraba la matrícula *«Sí, duro»* y `PC-05` de [`PR-01`](../procesos/PR-01-movilizacion-institucional.md) lo repetía. **Ninguna `RN-xx` lo establecía.** [`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md) cubre identidad y placa, [`RN-16`](RN-16-seguro-y-revision-mecanica.md) cubre póliza y revisión, [`RN-17`](RN-17-alertas-de-vencimiento-documental.md) cubre las alertas. La matrícula no la cubría nadie.

**Y el contraste era el problema.** El Bloque 1 fue escrupuloso en **no** bloquear por seguro ni por revisión, precisamente porque verificó `[V]` que no son obligatorios. Con la matrícula hizo lo contrario **sin verificar nada**, en el mismo documento donde había explicado con cuidado por qué el seguro no bloquea. Un bloqueo duro sostenido por nada escrito, al lado de una no-obligación sostenida con fuente.

## Enunciado

Un vehículo cuya **matrícula venza dentro del rango de la misión** —ventana solicitada más la holgura posterior— **no debe** poder programarse ni despacharse.

Se mide igual que la licencia en [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md): **no basta que esté vigente el día de salida**. Un vehículo cuya matrícula vence el miércoles no habilita una misión que retorna el viernes.

## Por qué se mantiene el bloqueo aunque el nivel sea `[I]`

Es la pregunta que el hallazgo dejó abierta, y merece respuesta explícita en vez de quedar implícita en una tabla.

**El seguro y la revisión no bloquean porque la institución no puede resolverlos siempre**: dependen de que exista obligación legal y de que haya presupuesto y proveedor. Bloquear por algo que no es exigible paraliza la flota por una decisión que nadie tomó.

**La matrícula es distinta: la institución puede renovarla.** Es un trámite propio, sobre un bien propio, con plazo conocido y alertable ([`RN-17`](RN-17-alertas-de-vencimiento-documental.md)). Un vehículo del Estado circulando con matrícula vencida es un hallazgo de auditoría cómodo de levantar y difícil de defender.

**Pero el nivel es `[I]` y por eso es configurable.** No podemos afirmar que la ley lo exija — no extrajimos el articulado. Lo que sí podemos es **fijar el valor por defecto en la postura conservadora** y dejar el interruptor a la vista, en lugar de cablear una obligación que no verificamos. Si la institución tiene una razón operativa para operar con matrículas vencidas, que sea una decisión suya, registrada y con nombre.

## Condiciones de aplicación

Aplica en la programación (`T-08`), la reasignación (`T-10`) y el despacho (`T-12`), donde `BD-03` ya se evalúa.

**No aplica** a la placa metálica: *«sin placa»* es **estado válido** por el desabastecimiento nacional, y lo resuelve [`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md) con el documento sustitutivo del IP. **Matrícula y placa son cosas distintas** y confundirlas bloquearía media flota.

## Comportamiento esperado

1. El bloqueo dice **cuándo vence** y **hasta cuándo llega el rango evaluado**, igual que `BD-02`. *«Documentación vencida»* a secas no le sirve a quien programa.
2. La alerta previa de [`RN-17`](RN-17-alertas-de-vencimiento-documental.md) llega antes de que el bloqueo aparezca. Si el usuario se entera por el bloqueo, la alerta falló.
3. La evaluación se congela con el expediente: qué matrícula, qué vencimiento, qué rango se evaluó ([`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md)).
4. Si `bloqueo_matricula_vencida` está apagado, **la advertencia sigue apareciendo y se acusa**. Apagar el bloqueo no apaga el registro.

## Casos límite

- **La matrícula está en trámite de renovación.** No es lo mismo que vencida, y el sistema no debe tratarlo igual. Se registra el trámite con su constancia adjunta y su fecha estimada; mientras el adjunto exista y la fecha no se haya pasado, **advierte en vez de bloquear**. `[C]` confirmar si el IP emite constancia de trámite y con qué validez.
- **La misión se prorroga y el nuevo fin de rango cae después del vencimiento.** Se revalida en `T-17` igual que `BD-02` y `BD-03`. La prórroga no es la puerta trasera del bloqueo.
- **Vehículo en comodato o alquilado**, cuya matrícula gestiona el tercero. El bloqueo aplica igual: quien responde ante un retén es la institución que lo tiene. Lo que cambia es a quién se le reclama, y eso lo resuelve [`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md).
- **Vehículo de servicio exceptuado.** [`RN-24`](RN-24-vehiculo-de-servicio-exceptuado.md) exime de la rotulación, **no de la matrícula**. La excepción es sobre la identificación visible, no sobre el registro del bien.

## Trazabilidad

- **Norma**: [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — `[V]` el Registro Vehicular y la obligación de registrar y alertar; **`[I]` el bloqueo**; `[C]` el articulado
- **Hallazgo que la origina**: `HN1-11` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- **Bloqueo**: `BD-03` de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) · `PC-05` de [PR-01](../procesos/PR-01-movilizacion-institucional.md)
- **Reglas relacionadas**: [`RN-15`](RN-15-identidad-del-vehiculo-y-placa.md) placa ≠ matrícula · [`RN-16`](RN-16-seguro-y-revision-mecanica.md) el contraste que motivó el hallazgo · [`RN-17`](RN-17-alertas-de-vencimiento-documental.md) · [`RN-24`](RN-24-vehiculo-de-servicio-exceptuado.md) · [`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)
- **Módulo principal**: M-04 Documentación y Cumplimiento Vehicular
