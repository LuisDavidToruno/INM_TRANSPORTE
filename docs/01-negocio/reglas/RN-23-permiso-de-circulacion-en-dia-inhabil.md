# RN-23 — Circular en día u hora inhábil requiere permiso vigente firmado por la máxima autoridad

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-04, M-15 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — Acuerdo No. 303, Decreto 135-94, Circular 003-2025-Presidencia-TSC; calendario en [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` la prohibición y la exigencia de permiso de la máxima autoridad — `[C]` el horario hábil oficial de la institución |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — calendario `dia_habil` y `horario_habil` por institución y delegación, con vigencia |

## Enunciado

El sistema **no debe** aprobar ni despachar una misión cuya ventana de circulación caiga, total o parcialmente, en **día inhábil, feriado u hora inhábil**, si no existe un **permiso de circulación vigente firmado por la máxima autoridad** (ACT-09) que cubra ese vehículo, ese motorista, esa ruta y esa ventana temporal.

La excepción es el vehículo de servicio exceptuado, regulada por [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md).

El calendario de días hábiles, feriados y horario laboral **debe** ser parámetro con vigencia, **nunca cableado**.

## Justificación

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[V]`: está prohibido el uso de vehículos del Estado en días y horas inhábiles, y para circular en ellos se requiere **permiso firmado por la máxima autoridad** de la institución. Se reportan multas de L 5,000 a L 50,000 más posible decomiso `[P]`, y el TSC realiza **operativos de fiscalización en Semana Santa** `[V]`, de forma recurrente y predecible.

[NRM-09](../normativa/NRM-09-realidad-operativa.md) fija los feriados del Artículo 339 del Código del Trabajo `[V]` y advierte que existe legislación posterior sobre los feriados de octubre que **no se pudo verificar** — razón adicional para que el calendario sea dato y no código.

## Condiciones de aplicación

Aplica a la ventana completa de circulación, no solo a la hora de salida: una misión que sale el viernes a las 14:00 y retorna el sábado requiere permiso por el tramo del sábado.

Aplica a las misiones que **cruzan** el inicio de la inhabilidad (por ejemplo, retraso que hace que el vehículo circule después del cierre del horario hábil): en ese caso no hay bloqueo posible porque el vehículo ya está en ruta, y se aplica el tratamiento de los casos límite.

`[C]` El horario hábil oficial de la institución. [NRM-09](../normativa/NRM-09-realidad-operativa.md) señala 8:00–16:00 de lunes a viernes como práctica típica `[I]`, **no como dato de la institución**.

## Comportamiento esperado

1. Al programar, el sistema evalúa la ventana contra el calendario vigente **a las fechas de la misión** y señala los tramos inhábiles antes de que el usuario termine de capturar.
2. Si hay tramo inhábil, el flujo exige **solicitud de permiso de circulación**: justificación, vehículo, motorista, ruta, ventana temporal, y autorización de ACT-09.
3. Aprobado el permiso, se emite el **salvoconducto impreso** de [RN-25](RN-25-salvoconducto-con-folio-y-qr.md). Sin salvoconducto emitido, el despacho se bloquea.
4. El permiso es **específico**: si cambia el vehículo, el motorista, la ruta o la ventana, el permiso deja de cubrir la misión y debe reemitirse.
5. El sistema genera el **reporte previo a Semana Santa** exigido por [NRM-02](../normativa/NRM-02-bienes-del-estado.md): vehículos autorizados con su permiso y vehículos que deben estar resguardados con confirmación de resguardo.

## Casos límite

- **Calendario de feriados de octubre.** `[C]` [NRM-09](../normativa/NRM-09-realidad-operativa.md) no pudo verificar la legislación que reagrupó esos feriados. **No se codifica ninguna suposición**: el calendario se carga con los feriados confirmados y la institución completa el resto. Un feriado mal cargado produce misiones ilegales o bloqueos infundados.
- **Retraso que empuja el retorno a hora inhábil.** No es bloqueable: el vehículo está en ruta. Se registra el evento, se notifica al Jefe de Transporte y **se puede emitir un permiso sobreviniente** si la máxima autoridad lo autoriza. Si no se emite, la orden se cierra con hallazgo. Nunca se ajusta la hora registrada para que "quepa" en el horario hábil.
- **Delegación con horario distinto al de la sede.** El calendario es por institución **y por delegación**. Una delegación fronteriza o de atención continua puede tener horario propio, y ese dato debe existir antes de operar.
- **Misión de varios días.** Los fines de semana intermedios son tramos inhábiles. El permiso debe cubrir la ventana completa; no se fracciona en permisos diarios salvo que la institución lo exija. `[C]`
- **Permiso aprobado y misión reprogramada a otra fecha.** El permiso no se arrastra: se reemite. Es el error más fácil de cometer y el que un operativo del TSC detecta de inmediato al comparar fechas.
- **¿Puede la máxima autoridad delegar esta firma?** `[C]` sin resolver. [NRM-02](../normativa/NRM-02-bienes-del-estado.md) dice *firmado por la máxima autoridad*. Hasta confirmarlo, el sistema **no permite delegación** de esta facultad ([RN-07](RN-07-delegacion-de-autorizacion.md)).
- **Vehículo que solo se traslada del predio al taller un sábado.** Es circulación en día inhábil: requiere permiso igual. La brevedad del trayecto no es criterio en la norma.

## Trazabilidad

- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Reglas relacionadas: [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md), [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-07](RN-07-delegacion-de-autorizacion.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)
- Actores: ACT-04, ACT-05, ACT-09, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
