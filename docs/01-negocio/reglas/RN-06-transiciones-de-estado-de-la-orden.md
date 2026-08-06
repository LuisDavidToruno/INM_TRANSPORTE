# RN-06 — La Orden de Misión solo transita por los estados definidos, y cada transición registra actor, rol, momento y motivo

| Campo | Valor |
|---|---|
| **Módulos** | M-06, M-07, M-08, M-13, M-14 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-07 y V-10; ciclo de vida definido en `CLAUDE.md` |
| **Verificación** | `[V]` la exigencia de autorización y registro oportuno — `[I]` la denominación de los estados es del proyecto |
| **Tipo** | Bloqueo duro |
| **Configurable** | No — el ciclo de vida es estructural |

## Enunciado

La Orden de Misión **debe** transitar únicamente por el ciclo:

```
BORRADOR → SOLICITADA → APROBADA → PROGRAMADA → DESPACHADA → EN_RUTA → RETORNADA → LIQUIDADA → CERRADA
```

con las ramas `RECHAZADA`, `ANULADA` y `CERRADA_CON_HALLAZGO`.

Cualquier transición no contemplada **debe** rechazarse. Toda transición efectuada **debe** registrar actor, rol y cargo vigentes, marca de tiempo y motivo — el motivo es obligatorio en `RECHAZADA`, `ANULADA` y `CERRADA_CON_HALLAZGO`, y en todo salto hacia atrás.

## Justificación

El ciclo de vida es el esqueleto de la cadena de trazabilidad que exige [NRM-01](../normativa/NRM-01-control-interno-tsc.md): `solicitud → autorización → orden de misión → bitácora → vale de combustible → liquidación`. Si un expediente puede saltar de `SOLICITADA` a `EN_RUTA`, la cadena se rompe y no hay forma de demostrar que hubo autorización previa a la salida del vehículo.

Los estados no son adorno de interfaz: son la evidencia de que el control ocurrió **antes** del hecho y no se reconstruyó después.

## Condiciones de aplicación

Aplica a toda Orden de Misión, incluidas las creadas en campo sin conectividad, que transitan localmente por los mismos estados y se validan de nuevo al sincronizar.

**No aplica** a las solicitudes de transporte previas a la emisión de la orden, que tienen su propio ciclo más simple en M-06.

## Comportamiento esperado

1. Cada transición valida sus **precondiciones**: no hay `APROBADA` sin autorizador válido ([RN-01](RN-01-segregacion-de-funciones.md), [RN-02](RN-02-escalamiento-de-autorizacion.md)); no hay `PROGRAMADA` sin vehículo y motorista habilitados ([RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md)); no hay `DESPACHADA` sin odómetro de salida; no hay `LIQUIDADA` sin conciliación de combustible ([RN-29](RN-29-liquidacion-de-combustible.md)).
2. Las transiciones hacia atrás permitidas son explícitas y acotadas: `APROBADA → SOLICITADA` por devolución del autorizador, y `PROGRAMADA → APROBADA` por cambio de asignación. Ambas exigen motivo.
3. `ANULADA` es alcanzable desde cualquier estado **anterior a `DESPACHADA`**. A partir de `DESPACHADA` el vehículo ya salió: la orden se **cancela en ruta** y sigue hasta `LIQUIDADA`, porque hubo consumo real que descargar.
4. `CERRADA_CON_HALLAZGO` se alcanza desde `LIQUIDADA` cuando queda alguna desviación sin justificar. **El expediente se cierra igual**: un expediente que no puede cerrarse se abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver.
5. El sistema expone la **línea de tiempo del expediente** con todas las transiciones, incluidas las rechazadas.

## Casos límite

- **La misión se ejecuta sin haber pasado por `APROBADA`** — emergencia real, autorización verbal. El sistema no permite la transición. Se registra como **misión no autorizada** desde el inicio, transita el ciclo con esa marca y termina obligatoriamente en `CERRADA_CON_HALLAZGO`. Nunca se le fabrica una aprobación retroactiva.
- **Misión multi-destino que retorna parcialmente** (deja carga en un punto y sigue). No hay `RETORNADA` parcial: las paradas y entregas son eventos de M-08 dentro de `EN_RUTA`. Ver [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md).
- **Vehículo que no retorna**: avería total, siniestro o robo. La orden **no queda colgada en `EN_RUTA`**. Transita a `RETORNADA` con marca de *retorno no efectuado* vinculada al expediente de incidente de M-12, y se liquida lo consumido.
- **Cambio de vehículo o motorista después de `DESPACHADA`.** No retrocede el estado: se registra como **sustitución en ruta**, con revalidación completa de habilitación ([RN-14](RN-14-sustitucion-de-motorista.md)) y nueva constancia de custodia ([RN-22](RN-22-custodia-del-vehiculo.md)).
- **Sincronización que trae una transición ya superada** — el dispositivo estuvo días sin red y envía `EN_RUTA` cuando el servidor ya tiene `RETORNADA`. No se aplica ni se descarta: entra a la cola de conflictos con su fecha del hecho ([RN-45](RN-45-cero-sobrescritura-silenciosa.md)).
- **Orden aprobada que nunca se ejecuta** — el viaje se suspende antes de salir. Transita a `ANULADA` con motivo, y arrastra la resolución de folios y asignaciones de combustible ya emitidos ([RN-27](RN-27-asignacion-de-combustible-con-folio.md)).

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-29](RN-29-liquidacion-de-combustible.md)
- Actores: ACT-02, ACT-03, ACT-04, ACT-05, ACT-06, ACT-08
- Historias y casos especiales: pendientes — Bloque 2
