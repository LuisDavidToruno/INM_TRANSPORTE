# RN-75 — El bien retenido, sustraído o no recuperado permanece en el registro patrimonial hasta su recuperación o su descargo formal

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-12, M-15, M-14 |
| **Origen** | Casos especiales [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) y [CE-03](../../02-requisitos/casos-especiales/CE-03-accidente-de-transito-en-mision.md) · Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) |
| **Verificación** | `[P]` el deber de conservar el registro y la custodia del bien del Estado hasta su descargo formal — [NRM-02](../normativa/NRM-02-bienes-del-estado.md) |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `causa_de_no_disponibilidad_del_bien` |

## Enunciado

La sustracción, el decomiso, la retención por autoridad o la pérdida de un vehículo o de parte de la carga durante una misión **debe** registrarse como **evento tipificado con constancia de denuncia o de acta ante autoridad**, y el bien **permanece en el registro patrimonial** hasta su **recuperación** o su **descargo formal**. **Nunca se elimina.**

Mientras dure la situación, el expediente **debe** conservar: **ubicación conocida**, **autoridad custodia**, **número de expediente** y **gestiones de recuperación** con responsable y plazo.

Los **documentos con folio e instrumentos de pago** que iban a bordo pasan a estado **`SUSTRAIDO`** —distinto de anulado—; su verificación por QR responde que están sustraídos, y **todo uso posterior imputado a la misión es alerta automática**.

## Justificación

Un bien del Estado que desaparece del sistema porque desapareció físicamente es el peor asiento posible: la institución pierde la unidad y además pierde la capacidad de demostrar que la tenía, que denunció y que gestionó su recuperación.

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) es clara en que el descargo de un bien es un acto formal con procedimiento. Un vehículo robado no está descargado: está robado, que es un estado distinto y con obligaciones vivas.

El estado `SUSTRAIDO` de los folios resuelve un problema concreto y frecuente: los vales de combustible que iban en la guantera. Si se marcan como *anulados*, se confunden con los anulados por desprogramación; si no se marcan, alguien los cobra y el consumo entra a la liquidación de una misión que ya no existía.

## Condiciones de aplicación

Aplica al vehículo, a la carga, a los documentos con folio, a los instrumentos de pago y al dispositivo portador de la captura de campo.

Aplica también al bien **retenido por autoridad** sin sustracción: un vehículo decomisado en un operativo o retenido tras un accidente.

**No aplica** al bien devuelto en el mismo acto, que se registra como novedad sin abrir el circuito.

## Comportamiento esperado

1. El evento se registra con hora del hecho, hora de captura, lugar, subtipo y descripción de lo sustraído o retenido, con fotografías cuando sea posible ([`RN-70`](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)).
2. La **constancia de denuncia o acta** se adjunta con número, autoridad receptora y fecha. Su ausencia no impide registrar el evento, pero sí genera obligación con plazo.
3. El estado operativo del vehículo pasa a `NO_DISPONIBLE` con causa tipificada, desde la **hora del hecho**, con el circuito de reservas afectadas de [`RN-60`](RN-60-indisponibilidad-sobrevenida-y-reservas.md).
4. El sistema solicita el **bloqueo de folios e instrumentos** y registra la hora de la solicitud. La lista de folios que iban a bordo se produce automáticamente desde la asignación.
5. La **última lectura verificable del odómetro** cierra el kilometraje, con su fuente. El tramo posterior queda **no determinado** y **nunca se completa con distancia teórica**; la conciliación se declara **truncada** ([`RN-72`](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md), [`RN-89`](RN-89-kilometraje-acumulado-invariante-del-expediente.md)).
6. Perdido el **dispositivo portador**, el sistema declara la **ventana de datos no recuperada**, admite digitación diferida desde papel con constancia ([`RN-47`](RN-47-digitacion-diferida-desde-papel.md)), y marca como **perdido —no como vacío—** lo que no se recupere.
7. La documentación y la póliza se **congelan a la fecha del hecho** ([`RN-40`](RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](RN-41-congelamiento-del-valor-al-autorizar.md)): el expediente muestra si estaban vigentes **ese día**, le convenga o no a la institución.
8. La misión no cierra limpio: `T-22` con hallazgo, que **no imputa responsabilidad a nadie** ([`RN-74`](RN-74-sin-atribucion-de-responsabilidad-en-campo.md)).

## Casos límite

- **Recuperación del bien.** Se registra con **acta de recepción y odómetro**; el kilometraje del período no determinado **sigue siendo no determinado**, y la diferencia entre la última lectura verificable y la de recuperación se asienta como tramo bajo tenencia desconocida.
- **Descargo formal.** Solo con el acto administrativo que lo dispone, adjunto. Hasta entonces, el bien sigue en el registro con su antigüedad contada y aparece en el saldo de apertura del ejercicio siguiente ([`RN-97`](RN-97-saldo-de-apertura-de-control-interno.md)).
- **Estado terminal `RETIRADO_DE_FLOTA` inexistente.** La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) es la autoridad y hoy no lo tiene; se reportó como ampliación necesaria desde [CE-15](../../02-requisitos/casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md). **Nota de hallazgo abierta.** Declarar *dado de baja* un bien que solo está robado sería un asiento falso.
- **Folio sustraído que aparece cobrado.** Alerta automática con el consumo, la estación, el monto y la fecha, dirigida a ACT-07 y a ACT-12, y expediente de hallazgo posterior ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)) si la misión ya cerró.
- **Carga sustraída parcialmente.** El faltante se declara contra el inventario de salida ([`RN-69`](RN-69-inventario-de-la-carga-y-acta-de-entrega.md)) y cada bien conserva su identificación unitaria en el expediente.
- **Vehículo retenido que la autoridad libera sin acta.** No se registra la devolución sin constancia; se registra el hecho de que fue liberado sin acta, que es un dato distinto y también relevante.

## Trazabilidad

- Norma: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]`
- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `W-08`, `T-18` subtipo retorno sin vehículo, `T-22`
- Reglas relacionadas: [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md), [RN-60](RN-60-indisponibilidad-sobrevenida-y-reservas.md), [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md), [RN-70](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md), [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [RN-89](RN-89-kilometraje-acumulado-invariante-del-expediente.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md)
- Casos especiales: [CE-04](../../02-requisitos/casos-especiales/CE-04-robo-de-vehiculo-o-de-carga-en-mision.md) — candidatas `RN-c:sustraccion-de-bien-en-mision`, `RN-c:folio-e-instrumento-de-pago-sustraido`, `RN-c:ultima-lectura-verificable-de-odometro`, `RN-c:reconstruccion-de-expediente-por-perdida-del-dispositivo` · [CE-03](../../02-requisitos/casos-especiales/CE-03-accidente-de-transito-en-mision.md) `RN-c:bien-del-estado-retenido-por-autoridad`
