# RN-36 — Un cobro en categoría distinta a la asignada se registra como discrepancia de clasificación y habilita el reclamo

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-13 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md) — comunicado SAPP 17/09/2025 |
| **Verificación** | `[V]` que la reclasificación indebida ocurrió y fue resuelta por la SAPP |
| **Tipo** | Derivación + advertencia |
| **Configurable** | No |

## Enunciado

Al registrar un paso por caseta, el motorista **debe** poder consignar la **categoría con la que efectivamente le cobraron** y el monto pagado.

Si esa categoría difiere de la categoría asignada al vehículo ([RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)), el sistema **debe** marcar el paso como **discrepancia de clasificación**, conservar el ticket como evidencia, y habilitar el expediente de **reclamo ante la SAPP**.

Una discrepancia **no debe** modificar automáticamente la categoría asignada al vehículo.

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md) documenta el hecho: entre agosto y septiembre de 2025, COVI-H reclasificó Hyundai H-100, Kia K2700 y Mercedes-Benz Sprinter a categoría superior, cobrándoles **L 90 en lugar de L 22** — cuatro veces de más. La SAPP resolvió el 17/09/2025 que deben clasificarse como livianos conforme al Artículo 51 de la Ley de Tránsito y ordenó suspender el cobro `[V]`.

La ficha concluye: *"la flota típica de una institución pública hondureña cae exactamente en la zona gris que la SAPP tuvo que resolver. Es previsible que a un vehículo institucional le cobren mal en la caseta."*

Si el sistema ajustara la categoría del vehículo al cobro recibido, el error de la caseta se volvería la verdad institucional y el reclamo nunca ocurriría. El cobro es un hecho a registrar; la clasificación correcta es una derivación de la ficha técnica y de la norma.

## Condiciones de aplicación

Aplica a todo paso por caseta con pago en efectivo, tarjeta o tag.

**No aplica** cuando el vehículo va en configuración distinta a la habitual — con remolque, por ejemplo — y la categoría cobrada corresponde a esa configuración declarada en la misión.

## Comportamiento esperado

1. El formulario de paso por caseta precarga la categoría esperada y el monto esperado, y permite consignar los efectivamente cobrados. La captura funciona **sin conectividad** ([RN-43](RN-43-captura-de-campo-sin-conectividad.md)).
2. Detectada la discrepancia, el sistema exige **fotografía del ticket** cuando exista, y la conserva vinculada al paso.
3. Se genera un registro de discrepancia con: punto, fecha y hora, categoría esperada y cobrada, monto esperado y pagado, diferencia, y evidencia.
4. El expediente de reclamo agrupa discrepancias por punto y período, con el fundamento de clasificación del vehículo ([RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)), listo para presentar ante la SAPP.
5. La diferencia pagada de más se registra en la liquidación como **sobrecosto por clasificación**, tipificado y no imputable al motorista.

## Casos límite

- **La SAPP resuelve a favor de la caseta.** Entonces la categoría asignada al vehículo estaba mal: se corrige **abriendo nueva vigencia** con la resolución como fundamento ([RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)), sin reescribir los pasos anteriores ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)).
- **Discrepancias sistemáticas del mismo punto con toda una clase de vehículos.** Es exactamente el patrón de 2025. El sistema debe **agregarlas y alertar**, no tratarlas como incidentes aislados: un reclamo con 200 pasos documentados pesa distinto que uno con un ticket.
- **Cobro en categoría inferior a la asignada** — le cobraron de menos. También es discrepancia y también se registra. Callarla expone a la institución a un cobro retroactivo y contradice el principio de registro fiel.
- **Motorista que no sabe con qué categoría le cobraron.** Frecuente: el ticket puede no indicarla. Se registra el **monto pagado**, y el sistema deriva la categoría probable comparando contra la tabla de tarifas de ese punto y fecha. Esa derivación se marca como **inferida**, no como declarada.
- **Sin ticket.** [NRM-10](../normativa/NRM-10-peajes.md) es clara: se advierte, **no se bloquea el cierre**. Sin ticket, el reclamo será más débil, y eso debe verse en el expediente.
- **Pago con tag CoviPass.** La categoría la aplica el sistema del tag, y la evidencia es el estado de cuenta, no un ticket. `[C]` insumo #24: si COVI-H emite estado de cuenta empresarial. El diseño debe conciliar contra estado de cuenta cuando exista.
- **Discrepancia detectada meses después al revisar el estado de cuenta.** Se registra con fecha del hecho igual a la del paso y fecha de captura posterior ([RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md)), y puede reabrir el reclamo aunque la orden esté cerrada — anexar evidencia a un expediente cerrado está permitido ([RN-05](RN-05-registro-cerrado-no-se-edita.md)).

## Trazabilidad

- Norma: [NRM-10 — Peajes](../normativa/NRM-10-peajes.md)
- Reglas relacionadas: [RN-33](RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)
- Actores: ACT-04, ACT-06, ACT-08
- Historias y casos especiales: pendientes — Bloque 2
