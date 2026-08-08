# RN-23 — Circular en día u hora inhábil requiere permiso vigente firmado por la máxima autoridad

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-04, M-15 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — Acuerdo No. 303, Decreto 135-94, Decreto 48; calendario en [NRM-09](../normativa/NRM-09-realidad-operativa.md). Momento del bloqueo: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) `BD-04`, `T-02`, `T-12` — **artefacto autoridad en transiciones y precondiciones** |
| **Verificación** | `[V]` la prohibición de circular en día u hora inhábil y la exigencia de permiso de la máxima autoridad — [NRM-02](../normativa/NRM-02-bienes-del-estado.md) marca `[V]` esa sección. **Eslabón más débil, dicho expresamente** (`HN1-19`): el Decreto 135-94 y el Decreto 48 están `[P]` en la tabla de la ficha, y la cita completa del Decreto 48 está `[C]`. `[P]` el rango de multas. `[C]` el horario hábil oficial de la institución |
| **Tipo** | Bloqueo duro **del despacho**; marca de la aprobación |
| **Configurable** | Sí — calendario `dia_habil` y `horario_habil` por institución y delegación, con vigencia. El **bloqueo del despacho** no es configurable |

## Nota de corrección — hallazgo `HB1-08`

> **Qué estaba mal.** La regla bloqueaba **la aprobación** además del despacho. `PR-01` E3 dispara el trámite del permiso ante la máxima autoridad (`PR-07`) **después** de aprobar. Resultado: **deadlock** — no se podía aprobar sin permiso y no se podía tramitar el permiso sin aprobar. Y el deadlock tenía una salida peor que el bloqueo: el usuario declara la salida en día hábil para poder avanzar, que es exactamente el fraude que la regla existe para impedir.
>
> **Qué manda.** La máquina de estados es taxativa y coherente: `BD-04` *"se evalúa en `T-12`"*; `T-02` *"se avisa desde aquí, **aunque el bloqueo sea en `T-12`**"*; y `T-05` **no** lista `BD-04` entre sus precondiciones. `PC-03` de `PR-01` lo dice igual de bien: *"marca en E3; bloqueo del despacho en E8"*. Se corrige esta regla, no la autoridad.
>
> **Sobre la ficha.** [NRM-02](../normativa/NRM-02-bienes-del-estado.md) contiene una implicación de requerimiento que dice *"el sistema debe bloquear la **aprobación** de una salida en día inhábil"*. Es redacción del equipo, no articulado: la norma exige permiso **para circular**, no para autorizar. Marcarla `[I]` y situar el bloqueo en el despacho no debilita el control — lo hace ejecutable.

## Enunciado

El sistema **no debe despachar** una misión cuya ventana de circulación caiga, total o parcialmente, en **día inhábil, feriado u hora inhábil**, si no existe un **permiso de circulación vigente firmado por la máxima autoridad** (ACT-09) que cubra ese vehículo, ese motorista, esa ruta y esa ventana temporal, con su **salvoconducto emitido e impreso** ([RN-25](RN-25-salvoconducto-con-folio-y-qr.md)).

**La aprobación no se bloquea.** Cuando la ventana toca franja inhábil, la Orden de Misión:

| Momento | Qué hace el sistema |
|---|---|
| Captura y envío a autorización (`T-02`) | Determina que la ventana toca franja inhábil y **avisa** al solicitante y al autorizador, con los tramos señalados |
| Autorización (`T-05`) | **Aprueba con normalidad** y la Orden queda con la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD`, visible en el expediente y en la bandeja de Transporte |
| Trámite del permiso | La marca **dispara** la solicitud de permiso de circulación ante ACT-09, que es un expediente propio |
| Despacho (`T-12`) | **Bloqueo duro** si el permiso no está vigente y el salvoconducto no está emitido |

La marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` **no se puede retirar a mano**: se extingue únicamente cuando existe el permiso vigente que la cubre, o cuando la ventana se reprograma a franja hábil.

La excepción es el vehículo de servicio exceptuado, regulada por [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md).

El calendario de días hábiles, feriados y horario laboral **debe** ser parámetro con vigencia, **nunca cableado** ([RN-39](RN-39-parametros-normativos-con-vigencia.md)).

## Justificación

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[V]`: está prohibido el uso de vehículos del Estado en días y horas inhábiles, y para circular en ellos se requiere **permiso firmado por la máxima autoridad** de la institución. Se reportan multas de L 5,000 a L 50,000 más posible decomiso `[P]`, y el TSC realiza **operativos de fiscalización en Semana Santa** `[V]`, de forma recurrente y predecible.

[NRM-09](../normativa/NRM-09-realidad-operativa.md) fija los feriados del Artículo 339 del Código del Trabajo `[V]` y advierte que existe legislación posterior sobre los feriados de octubre que **no se pudo verificar** — razón adicional para que el calendario sea dato y no código.

## Condiciones de aplicación

Aplica a la ventana completa de circulación, no solo a la hora de salida: una misión que sale el viernes a las 14:00 y retorna el sábado requiere permiso por el tramo del sábado.

Aplica a las misiones que **cruzan** el inicio de la inhabilidad (por ejemplo, retraso que hace que el vehículo circule después del cierre del horario hábil): en ese caso no hay bloqueo posible porque el vehículo ya está en ruta, y se aplica el tratamiento de los casos límite.

`[C]` El horario hábil oficial de la institución. [NRM-09](../normativa/NRM-09-realidad-operativa.md) señala 8:00–16:00 de lunes a viernes como práctica típica `[I]`, **no como dato de la institución**.

## Comportamiento esperado

1. Desde la captura, y de nuevo al programar, el sistema evalúa la ventana contra el calendario vigente **a las fechas de la misión** y señala los tramos inhábiles antes de que el usuario termine de capturar. El aviso temprano es lo que da tiempo a tramitar el permiso.
2. Si hay tramo inhábil, la aprobación deja la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` y el flujo abre la **solicitud de permiso de circulación**: justificación, vehículo, motorista, ruta, ventana temporal, y autorización de ACT-09. El permiso **no** requiere que la misión esté programada: basta con que esté aprobada.
3. Aprobado el permiso, se emite el **salvoconducto impreso** de [RN-25](RN-25-salvoconducto-con-folio-y-qr.md) junto con la Orden de Misión, dentro de `T-12`. Sin permiso vigente, el despacho se bloquea con mensaje accionable: *"La Orden de Misión N.º &lt;folio&gt; circula el sábado &lt;fecha&gt;. No existe permiso de circulación vigente de la máxima autoridad. Trámite pendiente desde el &lt;fecha&gt; (RN-23)."*
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
- **Misión aprobada el jueves para salir el sábado, sin permiso todavía.** Es el caso que la redacción anterior hacía imposible. La jefatura **aprueba**; la Orden queda marcada; el trámite del permiso corre en paralelo a la programación; el despacho del sábado se bloquea si el permiso no llegó. Nadie tiene que falsear la fecha de salida para que el expediente avance.
- **Permiso que llega después de la hora prevista de salida.** La misión no se despacha hasta tenerlo. El retraso se registra contra el expediente del permiso, no contra el motorista.
- **Vehículo de servicio exceptuado** ([RN-24](RN-24-vehiculo-de-servicio-exceptuado.md)). **Nota de hallazgo abierta:** `BD-04` de la máquina de estados **no contempla la excepción** y exige permiso y salvoconducto en todos los casos, con lo que una ambulancia institucional con excepción vigente no podría despacharse un domingo (`HB1-21`). La divergencia queda señalada; `BD-04` está fuera de esta carpeta y no se toca aquí. Mientras no se resuelva, la excepción se registra en la Orden con su fundamento y **el despacho la considera equivalente al permiso**.

## Trazabilidad

- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Momento del bloqueo: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) `BD-04` (*"se evalúa en `T-12`"*), `T-02`, `T-05`, `T-12`
- Hallazgos que corrigen esta regla: `HB1-08` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md); `HN1-19` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md). Hallazgo abierto señalado: `HB1-21`
- Reglas relacionadas: [RN-24](RN-24-vehiculo-de-servicio-exceptuado.md), [RN-25](RN-25-salvoconducto-con-folio-y-qr.md), [RN-07](RN-07-delegacion-de-autorizacion.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)
- Actores: ACT-04, ACT-05, ACT-09, ACT-10
- Historias y casos especiales: pendientes — Bloque 2
