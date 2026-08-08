# RN-06 — La Orden de Misión solo transita por los estados definidos, y cada transición registra actor, rol, momento y motivo

| Campo | Valor |
|---|---|
| **Módulos** | M-06, M-07, M-08, M-13, M-14 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-07 y V-10; ciclo de vida definido en `CLAUDE.md`. Tabla de transiciones `T-01` a `T-22`, transiciones prohibidas y estados terminales: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — **artefacto autoridad en transiciones, precondiciones e invariantes** |
| **Verificación** | `[P]` la exigencia de autorización previa y de registro oportuno: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) marca `[P]` el catálogo NOGECI donde viven V-07 y V-10, verificado por citas en informes del TSC pero sin articulado extraído. Corregido desde `[V]` por la regla de no escalar el nivel (`HN1-06`) — `[I]` la denominación de los estados es del proyecto |
| **Tipo** | Bloqueo duro |
| **Configurable** | No — el ciclo de vida es estructural |

## Nota de corrección — hallazgos `HB1-03` y `HB1-12`

> Esta regla es **estructural y no configurable**, y no coincidía con la máquina de estados que la implementa. Es la peor combinación posible, y por eso se corrigen los cuatro puntos.
>
> | # | Qué decía esta regla | Qué dice la autoridad |
> |---|---|---|
> | 1 | *"`ANULADA` es alcanzable desde cualquier estado **anterior a `DESPACHADA`**"*, justificado con *"a partir de `DESPACHADA` **el vehículo ya salió**"* | **Premisa falsa.** En `DESPACHADA` el vehículo *"todavía no ha salido del predio"*; la salida es `T-14`. Y `T-15` `DESPACHADA → ANULADA` **existe**, la ejecutan **ACT-04 + ACT-07 + ACT-13** y exige devolución íntegra |
> | 2 | *"No aplica a las solicitudes previas a la emisión de la orden, que tienen su propio ciclo en M-06"* | §0.3: *"Es **un solo expediente con dos fases, no dos entidades que se copian**. Partirlo en dos rompe la cadena trazable que exige `NRM-01`"* |
> | 3 | Listaba `APROBADA → SOLICITADA` como transición hacia atrás | **Esa transición no existe.** La devolución es `T-04` `SOLICITADA → BORRADOR`. Y faltaba `T-20` `LIQUIDADA → RETORNADA`, que sí existe |
> | 4 | *"Cambio de vehículo o motorista después de `DESPACHADA`: **no retrocede el estado**, se registra como sustitución en ruta"* | En `DESPACHADA` **no se puede** cambiar de vehículo ni motorista sin revertir antes a `PROGRAMADA` con devolución de lo entregado. `T-17` relevo solo aplica desde `EN_RUTA` |
>
> **Qué manda.** Por la precedencia entre artefactos de `CLAUDE.md`, la máquina de estados es autoridad en transiciones. **Esta regla no reescribe su tabla: la referencia.** Una tabla copiada es una tabla que va a divergir.

## Enunciado

La Orden de Misión **debe** transitar únicamente por el ciclo:

```
BORRADOR → SOLICITADA → APROBADA → PROGRAMADA → DESPACHADA → EN_RUTA → RETORNADA → LIQUIDADA → CERRADA
```

con las ramas `RECHAZADA`, `ANULADA` y `CERRADA_CON_HALLAZGO`.

**El catálogo de transiciones válidas es el de [orden-de-mision.md §3](../../03-arquitectura/estados/orden-de-mision.md) — `T-01` a `T-22` — y su lista de transiciones prohibidas de §3.4.** Esta regla no lo reproduce: obliga a respetarlo. Cualquier transición que no esté en esa tabla **debe** rechazarse.

Toda transición efectuada **debe** registrar actor, rol y cargo vigentes, marca de tiempo del hecho y de captura, y motivo — el motivo es obligatorio en `RECHAZADA`, `ANULADA` y `CERRADA_CON_HALLAZGO`, y en toda transición hacia atrás.

**Es un solo expediente con dos fases**, no dos entidades que se copian: la solicitud (`BORRADOR`, `SOLICITADA`, `APROBADA`, M-06) y la orden (`PROGRAMADA` en adelante, M-07 y siguientes) son el **mismo** expediente con el **mismo** identificador. Partirlo en dos rompe la cadena trazable que exige [NRM-01](../normativa/NRM-01-control-interno-tsc.md).

## Justificación

El ciclo de vida es el esqueleto de la cadena de trazabilidad que exige [NRM-01](../normativa/NRM-01-control-interno-tsc.md): `solicitud → autorización → orden de misión → bitácora → vale de combustible → liquidación`. Si un expediente puede saltar de `SOLICITADA` a `EN_RUTA`, la cadena se rompe y no hay forma de demostrar que hubo autorización previa a la salida del vehículo.

Los estados no son adorno de interfaz: son la evidencia de que el control ocurrió **antes** del hecho y no se reconstruyó después.

## Condiciones de aplicación

Aplica a toda Orden de Misión, incluidas las creadas en campo sin conectividad, que transitan localmente por los mismos estados y se validan de nuevo al sincronizar.

**Aplica también a la fase de solicitud** — `BORRADOR`, `SOLICITADA`, `APROBADA`, `RECHAZADA` —, que no tiene ciclo propio ni entidad separada. Es la misma máquina.

## Comportamiento esperado

1. Cada transición valida sus **precondiciones**: no hay `APROBADA` sin autorizador válido ([RN-01](RN-01-segregacion-de-funciones.md), [RN-02](RN-02-escalamiento-de-autorizacion.md)); no hay `PROGRAMADA` sin vehículo y motorista habilitados ([RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md)); no hay `DESPACHADA` sin odómetro de salida; no hay `LIQUIDADA` sin conciliación de combustible ([RN-29](RN-29-liquidacion-de-combustible.md)).
2. Las transiciones hacia atrás permitidas son explícitas, acotadas y **todas exigen motivo**: `T-04` `SOLICITADA → BORRADOR` por devolución del autorizador, `T-11` `PROGRAMADA → APROBADA` por liberación de recursos, y `T-20` `LIQUIDADA → RETORNADA` por devolución de la liquidación (ACT-08). **No existe** `APROBADA → SOLICITADA`.
3. **`ANULADA` es alcanzable hasta `DESPACHADA` inclusive.** El corte no es `DESPACHADA`: es la **salida del vehículo** (`T-14`). Mientras el vehículo no ha salido, la anulación existe y tiene requisitos crecientes:

   | Estado | Transición | Quién | Qué exige |
   |---|---|---|---|
   | `BORRADOR` | `T-03` | ACT-02 | Motivo. Descarte previo al circuito de control |
   | `SOLICITADA` | `T-07` | ACT-02 desistimiento · ACT-08 anulación administrativa | Motivo tipificado |
   | `APROBADA` | `T-09` | ACT-04 · ACT-02 · ACT-08 | Motivo tipificado — alimenta el indicador de déficit de flota |
   | `PROGRAMADA` | `T-13` | ACT-04 · ACT-08 | Motivo; liberación de reservas y folio |
   | `DESPACHADA` **sin consumo** | `T-15` | **ACT-04 + ACT-07 + ACT-13**, los tres | **Devolución íntegra** del fondo o vales con acta, devolución de la custodia con odómetro, y devolución o destrucción constatada de los documentos impresos |
   | `DESPACHADA` **con cualquier consumo** | `T-16` → `RETORNADA` | ACT-04 · ACT-10 | **No se anula: se liquida.** Hubo movimiento de fondos públicos y anular sería borrar un hecho económico |

   **Desde `EN_RUTA` en adelante la anulación no existe.** El vehículo salió: el camino es `T-18` con subtipo *retorno anticipado* y después liquidar. `LIQUIDADA → ANULADA`, `CERRADA → *` y `CERRADA_CON_HALLAZGO → CERRADA` son transiciones **prohibidas**; lo que procede es el asiento reverso ([RN-04](RN-04-anulacion-como-asiento-reverso.md)) y, si el hecho es material, su propio expediente de hallazgo.
4. `CERRADA_CON_HALLAZGO` se alcanza desde `LIQUIDADA` cuando queda alguna desviación sin justificar. **El expediente se cierra igual**: un expediente que no puede cerrarse se abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver.
5. El sistema expone la **línea de tiempo del expediente** con todas las transiciones, incluidas las rechazadas.

## Casos límite

- **La misión se ejecuta sin haber pasado por `APROBADA`** — emergencia real, autorización verbal. El sistema no permite la transición. Se registra como **misión no autorizada** desde el inicio, transita el ciclo con esa marca y termina obligatoriamente en `CERRADA_CON_HALLAZGO`. Nunca se le fabrica una aprobación retroactiva.
- **Misión multi-destino que retorna parcialmente** (deja carga en un punto y sigue). No hay `RETORNADA` parcial: las paradas y entregas son eventos de M-08 dentro de `EN_RUTA`. Ver [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md).
- **Vehículo que no retorna**: avería total, siniestro o robo. La orden **no queda colgada en `EN_RUTA`**. Transita a `RETORNADA` con marca de *retorno no efectuado* vinculada al expediente de incidente de M-12, y se liquida lo consumido.
- **Cambio de vehículo o motorista con la misión `DESPACHADA` y el vehículo aún en el predio.** El vehículo no arranca a las 07:15 y se cambia por otro. **Sí retrocede el estado**: se devuelve lo entregado — vale y documentos impresos —, la misión vuelve a `PROGRAMADA` por `T-11`, se reasigna por `T-10` y se despacha de nuevo. El folio reservado **se anula** y se consume uno nuevo (`EF-02`): el folio es la unidad de trazabilidad del combustible ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)) y no puede quedar cubriendo dos configuraciones distintas de la misma misión. Revalidación completa de habilitación ([RN-14](RN-14-sustitucion-de-motorista.md)) y nueva constancia de custodia ([RN-22](RN-22-custodia-del-vehiculo.md)).
- **Relevo de motorista con la misión `EN_RUTA`.** Ahí sí no retrocede el estado: es `T-17`, con acta de traspaso de custodia, odómetro, y revalidación de `BD-02` contra el **paquete normativo congelado** que lleva el dispositivo. La responsabilidad del tramo anterior no se transfiere.
- **Sincronización que trae una transición ya superada** — el dispositivo estuvo días sin red y envía `EN_RUTA` cuando el servidor ya tiene `RETORNADA`. No se aplica ni se descarta: entra a la cola de conflictos con su fecha del hecho ([RN-45](RN-45-cero-sobrescritura-silenciosa.md)).
- **Orden aprobada que nunca se ejecuta** — el viaje se suspende antes de salir. Transita a `ANULADA` con motivo por la transición que corresponda a su estado (tabla del comportamiento 3), y arrastra la resolución de folios y asignaciones de combustible ya emitidos ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)).
- **La misión se suspendió después de despachar y el motorista ya había llenado el tanque la tarde anterior.** Es el caso especial más frecuente de la operación real, y **no es una anulación**: es `T-16`, `DESPACHADA → RETORNADA`, con kilometraje cero, bitácora sin eventos de ruta, conciliación limitada a entregado contra consumido contra devuelto, y la misión marcada como **no ejecutada** para que no contamine los indicadores de rendimiento. *"Si se consumió aunque sea un lempira, la misión no se anula: se liquida"* (`EF-06`).

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-07 y V-10 `[P]`
- Catálogo de transiciones, transiciones prohibidas y estados terminales: [orden-de-mision.md §3, §3.4 y §8](../../03-arquitectura/estados/orden-de-mision.md)
- Hallazgos que corrigen esta regla: `HB1-03` y `HB1-12` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md); `HN1-06` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-29](RN-29-liquidacion-de-combustible.md)
- Actores: ACT-02, ACT-03, ACT-04, ACT-05, ACT-06, ACT-07, ACT-08, ACT-10, ACT-13
- Historias y casos especiales: pendientes — Bloque 2
