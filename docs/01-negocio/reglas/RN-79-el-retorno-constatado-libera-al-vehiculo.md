# RN-79 — El retorno físico constatado libera vehículo y motorista sin esperar la digitación de la bitácora

| Campo | Valor |
|---|---|
| **Módulos** | M-08, M-07, M-03, M-16, M-13 |
| **Origen** | Casos especiales [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) y [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) · Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md) · Premisa rectora 5 |
| **Verificación** | `[V]` la ausencia de conectividad en el área rural — [NRM-09](../normativa/NRM-09-realidad-operativa.md), INE EPHPM julio 2025. `[I]` la separación entre liberar el recurso y liquidar el expediente: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro (sobre la liquidación) + derivación (sobre la disponibilidad) |
| **Configurable** | No la liberación. Sí el plazo de digitación (parámetro `plazo_digitacion_bitacora`) |

## Enunciado

La **constatación física del retorno** del vehículo a su predio — con acta, odómetro fotografiado, hora de la constatación y quién constató — **debe** bastar para registrar el retorno (`T-18`) y devolver vehículo y motorista al conjunto asignable, **aunque la bitácora del viaje no esté digitada**.

La Orden de Misión queda con la marca **`BITACORA_PENDIENTE_DE_DIGITACION`**, que:

- **bloquea** la liquidación (`T-19`) y, por tanto, el cierre
- **no bloquea** la asignación del vehículo ni del motorista a una nueva Orden de Misión

Ninguna regla, parámetro ni configuración **debe** permitir que un trámite de digitación mantenga un vehículo fuera de servicio.

## Justificación

En una delegación con dos vehículos, dejar una unidad inmovilizada porque la bitácora de papel todavía no se digitó equivale a suprimir el 50 % de la capacidad de transporte de esa delegación por una razón administrativa. La consecuencia real y observada es que la siguiente salida se hace **sin Orden de Misión**, con lo cual el sistema no solo no controló ese viaje: empujó a que ocurriera fuera de él.

El control que interesa al Tribunal Superior de Cuentas no es que el vehículo esté congelado: es que **el kilometraje, el combustible y la custodia queden completos antes de liquidar**. Eso se garantiza bloqueando la liquidación, que es donde el dinero se cierra, no bloqueando la operación.

La constatación física con odómetro fotografiado es, además, **mejor evidencia** que la digitación: es un dato tomado del tablero por una persona identificada en un momento identificado, no una transcripción.

## Condiciones de aplicación

Aplica a todo retorno, con o sin conectividad, y a los subtipos de `T-18`: retorno normal, retorno anticipado y retorno sin vehículo.

**No aplica** al retorno sin vehículo — siniestro total, robo, decomiso, vehículo resguardado fuera de sede: ahí no hay unidad que liberar y rige [`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) y el estado `NO_DISPONIBLE` con causa tipificada.

**No libera al motorista** si el retorno se registró con evento de incapacidad ([`RN-70`](RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)) o si su habilitación venció durante la misión ([`RN-55`](RN-55-habilitacion-vencida-durante-la-mision.md)): esos son bloqueos propios y no los levanta el retorno.

## Comportamiento esperado

1. La constatación exige: identidad de quien constata, fecha y hora del hecho, odómetro **fotografiado del tablero**, nivel de tanque ([`RN-83`](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)), estado del vehículo y de sus accesorios, y recepción de la custodia ([`RN-22`](RN-22-custodia-del-vehiculo.md)).
2. Quien constata **no puede ser el motorista que retorna** ([`RN-01`](RN-01-segregacion-de-funciones.md)). Si en la delegación no hay otra persona, se registra así, con motivo, y el hecho entra al indicador de la delegación.
3. Registrada la constatación, el estado operativo del vehículo pasa de `EN_MISION` a `DISPONIBLE` por la vía automática de `T-18` ([orden-de-mision.md §10.2](../../03-arquitectura/estados/orden-de-mision.md)).
4. La marca `BITACORA_PENDIENTE_DE_DIGITACION` es **visible** en el expediente, en el listado de la delegación y en el reporte de oportunidad de registro ([`RN-82`](RN-82-indicadores-de-calidad-de-la-programacion.md)), con los días transcurridos desde la fecha del hecho.
5. Al digitarse la bitácora, la continuidad del odómetro se evalúa sobre la serie ordenada por **fecha del hecho** ([`RN-89`](RN-89-kilometraje-acumulado-invariante-del-expediente.md)); si en el intervalo el vehículo ya salió a otra misión, insertar el registro anterior **reabre la validación de los posteriores**, y las diferencias van a cola de resolución humana ([`RN-45`](RN-45-cero-sobrescritura-silenciosa.md)).
6. Vencido `plazo_digitacion_bitacora`, la misión no se cierra en silencio: alerta con escalamiento y, al cerrarse, cierre con hallazgo (`T-22`).

## Casos límite

- **El vehículo sale de nuevo antes de que se digite la bitácora anterior.** Es el caso normal y por eso existe esta regla. El odómetro de salida de la nueva misión se toma del tablero, no del sistema, y al digitarse la bitácora vieja el sistema concilia ambos y abre conflicto si no cuadran.
- **Retorno constatado con odómetro menor al de salida.** No se acepta silenciosamente ([`RN-31`](RN-31-odometro-de-retorno.md)); pero **tampoco impide la constatación**: se registra la lectura tal cual, se marca la inconsistencia y el vehículo se libera igual salvo que el instrumento esté declarado averiado ([`RN-90`](RN-90-intervencion-del-instrumento-de-medicion.md)).
- **La delegación pierde la hoja de papel.** El retorno ya está constatado y el vehículo operando. La bitácora se reconstruye por la vía de [`RN-85`](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md) con lo que exista, y lo que no se recupere se declara **perdido, no vacío**. La misión cierra con hallazgo.
- **Motorista que retorna y sale de inmediato en otra misión.** Se libera y se reasigna, con la revalidación completa de habilitaciones ([`RN-09`](RN-09-matriz-licencia-vehiculo.md), [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md)) y con el fondo de la misión anterior aún pendiente de liquidar: eso **sí** puede bloquear, por [`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), y es un bloqueo de dinero, no de trámite.
- **Constatación registrada sin conectividad.** Vale igual: `T-18` es ejecutable en el dispositivo ([orden-de-mision.md §6](../../03-arquitectura/estados/orden-de-mision.md)) y la liberación es local hasta que sincronice.

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `T-18`, `T-19`, §6 operación desconectada, §10.2 estado operativo del vehículo
- Norma: [NRM-09](../normativa/NRM-09-realidad-operativa.md) `[V]` la falta de conectividad rural
- Reglas relacionadas: [RN-22](RN-22-custodia-del-vehiculo.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md), [RN-80](RN-80-hoja-de-bitacora-impresa-con-folio.md), [RN-89](RN-89-kilometraje-acumulado-invariante-del-expediente.md)
- Casos especiales: [CE-09](../../02-requisitos/casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — candidata `RN-c:retorno-constatado-libera-al-vehiculo` · [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) — candidata `RN-c:liberacion-inmediata-de-reservas-por-retorno-anticipado`
- Actores: ACT-06 retorna · ACT-05 o ACT-10 constata · ACT-04 liquida
