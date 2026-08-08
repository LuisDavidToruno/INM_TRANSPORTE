# RN-19 — Un vehículo solo se asigna desde `DISPONIBLE`, y solo se despacha desde `ASIGNADO`

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-07, M-11 |
| **Origen** | Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — control de bienes; decisión [DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md). Catálogo de estados y sus transiciones `W-01` a `W-15`: [orden-de-mision.md §10.2](../../03-arquitectura/estados/orden-de-mision.md) — **artefacto autoridad en transiciones, precondiciones y bloqueos duros** |
| **Verificación** | `[I]` regla operativa — `[I]` la exigencia de control del estado operativo del bien: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) la recoge como implicación de requerimiento escrita por el equipo, no como articulado citable. Corregido desde `[V]` por la regla de no escalar el nivel (`HN1-06`) |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — el catálogo `estado_operativo_vehiculo` marca, por estado, si **habilita asignación**; el estado exigido para despachar **no** es configurable: lo fija `T-12` |

## Nota de corrección — hallazgo `HB1-07`

> **Qué estaba mal — dos cosas, y cada una detenía la operación completa.**
>
> 1. **El despacho.** La regla exigía estado `DISPONIBLE` para *asignar y despachar*. Pero programar (`T-08`) **saca** al vehículo de `DISPONIBLE` y lo deja en `ASIGNADO` (`W-03`), y `T-12` exige como precondición que *"su estado operativo sigue siendo `ASIGNADO`"*. Implementada literalmente, **ningún despacho era posible**: el vehículo nunca vuelve a estar `DISPONIBLE` entre la programación y el despacho.
> 2. **El retorno.** La regla decía que el retorno a disponible *"debe ser un acto explícito de ACT-11 Encargado de Mantenimiento, **nunca automático**"*. §10.2 define `W-06` `EN_MISION → DISPONIBLE` como **automático** por el registro de retorno, y advierte que *"`ASIGNADO` y `EN_MISION` **los fija el sistema**, no una persona"*. Con la redacción anterior, toda misión que retornaba sin novedad dejaba el vehículo atascado en `EN_MISION` esperando a un jefe de taller que no participó en nada, y al día siguiente no se podía programar.
>
> **Qué manda.** Por la precedencia entre artefactos de `CLAUDE.md`, la máquina de estados es la autoridad. Se corrige esta regla, no la autoridad. La exigencia de acto explícito de ACT-11 queda acotada a `EN_TALLER → DISPONIBLE` (`W-10`), que es donde tiene sentido.

## Enunciado

El estado operativo del vehículo ([orden-de-mision.md §10.2](../../03-arquitectura/estados/orden-de-mision.md)) determina qué se puede hacer con él, y cada acto exige un estado distinto:

| Acto | Transición de la Orden de Misión | Estado operativo exigido | Estado en que queda |
|---|---|---|---|
| **Asignar** — programar o reasignar | `T-08`, `T-10` | **`DISPONIBLE`**, único estado desde el que se puede programar (`BD-07`) | `ASIGNADO` (`W-03`) |
| **Despachar** | `T-12` | **`ASIGNADO`** | `ASIGNADO` — el vehículo aún no ha salido del predio |
| **Registrar salida** | `T-14` | `ASIGNADO` | `EN_MISION` (`W-05`) |
| **Registrar retorno** | `T-18` | `EN_MISION` | `DISPONIBLE`, `EN_TALLER` o `NO_DISPONIBLE` según las novedades |

El sistema **no debe** permitir **asignar** un vehículo cuyo estado operativo vigente no esté marcado en el catálogo como **habilitante de asignación** — hoy, únicamente `DISPONIBLE`.

El sistema **no debe** permitir **despachar** un vehículo cuyo estado operativo vigente no sea `ASIGNADO` a esa misma Orden de Misión. Un vehículo que pasó a `EN_TALLER` o a `NO_DISPONIBLE` entre la programación y el despacho **no se despacha**: se sustituye por `T-10`, previa liberación de la reserva.

Los estados que **no** habilitan asignación son, como mínimo: `EN_TALLER`, `NO_DISPONIBLE` con su causa tipificada — falla reportada, siniestro, robo, trámite de descargo, sin custodio, resguardo por disposición superior —, `DADO_DE_BAJA`, y `ASIGNADO` o `EN_MISION` respecto de **otra** misión ([RN-13](RN-13-sin-doble-asignacion.md)).

**Cómo se cambia el estado.**

- La apertura de una orden de trabajo en M-11 y el reporte de una falla incapacitante desde el campo cambian el estado **automáticamente**.
- `ASIGNADO` y `EN_MISION` **los fija el sistema** como consecuencia de las transiciones de la Orden de Misión. Ninguna persona los fija a mano: un vehículo "en misión" sin misión no debe poder existir.
- El retorno **desde `EN_TALLER`** a `DISPONIBLE` (`W-10`) **sí debe** ser acto explícito de ACT-11 Encargado de Mantenimiento, con la orden de trabajo cerrada. Nunca se deriva del solo cierre administrativo del expediente de taller.
- El retorno **desde `NO_DISPONIBLE`** (`W-02`) exige acto explícito del rol que corresponda a la causa tipificada que lo puso ahí.

## Justificación

Asignar un vehículo que está en el taller produce una misión imposible que alguien resolverá tomando otro vehículo sin registrarlo — y a partir de ahí el kilometraje, el combustible y los peajes quedan atribuidos al vehículo equivocado. Toda la conciliación de [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) se contamina.

[DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) establece que el estado del vehículo lo registran **también los propios motoristas desde el campo**: la falla se reporta donde ocurre, no cuando el vehículo llega al taller.

## Condiciones de aplicación

Aplica a asignación, despacho y sustitución, **con el estado exigido propio de cada acto** según la tabla del enunciado.

**No aplica** al traslado del propio vehículo al taller o desde el taller, que es una misión legítima de un vehículo no disponible. Ese traslado se modela como **orden de misión de tipo traslado a taller**, que el sistema admite explícitamente y que exige motorista habilitado igual ([RN-09](RN-09-matriz-licencia-vehiculo.md)).

## Comportamiento esperado

1. El estado operativo es un dato con **historial y vigencia**: quién o qué transición lo cambió, cuándo, por qué y con qué documento de respaldo.
2. El bloqueo informa el estado, el acto que se pretendía y el expediente que lo origina: *"El vehículo <correlativo> está en estado <en taller> desde el <fecha> por la orden de trabajo N.º <folio>. No puede asignarse (RN-19)."* Al despachar: *"El vehículo <correlativo> ya no está asignado a la Orden de Misión N.º <folio>: pasó a <en taller> el <fecha>. Sustituya el vehículo (RN-14) antes de despachar."*
3. Una falla reportada desde el campo por el motorista con severidad **incapacitante** cambia el estado de inmediato y **libera las reservas** de las misiones futuras que dependían de ese vehículo, notificando al despacho. Las misiones afectadas vuelven a la cola de programación por `T-11`; no se anulan por sí solas.
4. El sistema muestra la **fecha estimada de retorno a disponible** cuando el taller la registre, para que la programación pueda planificar en lugar de solo tropezar con el bloqueo.
5. Los períodos de indisponibilidad alimentan el indicador de **disponibilidad de flota** por vehículo, tipo y dependencia.

## Casos límite

- **Falla no incapacitante** — un vidrio roto, aire acondicionado dañado. No bloquea, pero se registra y se muestra como observación al despachar. `[C]` la escala de severidad con el Encargado de Mantenimiento; **no se inventa**.
- **Vehículo que se avería a mitad de ruta.** No se le cambia el estado para "bloquear" una misión que ya está ocurriendo. Se registra el evento en la bitácora, se cambia el estado a partir de ese momento, y la misión sigue su ciclo hasta liquidarse ([RN-06](RN-06-transiciones-de-estado-de-la-orden.md)). Es el caso especial más importante de este módulo y merece su propio `CE-xx` en el Bloque 2.
- **Retorno a disponible sin que el taller cierre la orden de trabajo.** No se permite: sería un vehículo operando con reparación abierta. Si el taller lo libera provisionalmente, lo registra como **liberación condicionada** con restricciones anotadas, que se muestran al despachar.
- **Misión que retorna sin novedades.** El vehículo vuelve a `DISPONIBLE` **automáticamente** con el registro de retorno (`W-06`), sin intervención de ACT-11. Exigir un acto humano aquí dejaría la flota inmovilizada por un trámite que nadie tiene motivo para hacer.
- **Misión que retorna con novedades declaradas por el motorista.** El vehículo pasa a `EN_TALLER` (`W-07`) o a `NO_DISPONIBLE` (`W-08`), también de forma automática desde el acta de recepción — [DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md): el estado lo registran los propios motoristas desde el campo. La salida de esos estados sí es acto explícito.
- **Vehículo resguardado por operativo de Semana Santa.** Es `NO_DISPONIBLE` con causa tipificada, fundamento y vigencia acotada, alimentado por el reporte que exige [NRM-02](../normativa/NRM-02-bienes-del-estado.md). Al vencer el período de resguardo el sistema **propone** el retorno a `DISPONIBLE`; la confirmación la da el rol que ordenó el resguardo. `[C]` confirmar con la institución si acepta el retorno automático al vencer la vigencia del resguardo.
- **Vehículo que pasa a taller entre la programación y el despacho.** Es el caso que la redacción anterior hacía imposible de resolver. El despacho se bloquea por estado distinto de `ASIGNADO`, y la salida es `T-10` sustitución de vehículo ([RN-14](RN-14-sustitucion-de-motorista.md)), no forzar el despacho.
- **Vehículo prestado a otra dependencia dentro de la misma institución.** No es indisponible para la institución, pero sí para la dependencia de origen. El estado se acompaña de la dependencia tenedora, y la agenda de [RN-13](RN-13-sin-doble-asignacion.md) refleja la ocupación.
- **Vehículo robado que aparece.** El retorno a disponible exige el acta correspondiente y la resolución del expediente de M-12; no basta con cambiar el estado.

## Trazabilidad

- Norma: [NRM-02 — Bienes del Estado](../normativa/NRM-02-bienes-del-estado.md)
- Decisión: [DP-001, D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Estados operativos y transiciones `W-01` a `W-15`: [orden-de-mision.md §10.2](../../03-arquitectura/estados/orden-de-mision.md); precondiciones `BD-07` (`T-08`, `T-10`) y `T-12`
- Hallazgo que corrige esta regla: `HB1-07` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md)
- Reglas relacionadas: [RN-13](RN-13-sin-doble-asignacion.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-22](RN-22-custodia-del-vehiculo.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md)
- Actores: ACT-04, ACT-05, ACT-06, ACT-11
- Historias y casos especiales: pendientes — Bloque 2
