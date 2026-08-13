# RN-66 — Toda imputación externa se resuelve contra el vehículo por la jerarquía de anclas declarada, a la fecha del hecho, y se atribuye al conductor de ese momento

| Campo | Valor |
|---|---|
| **Módulos** | M-12, M-18, M-03, M-14, M-09 |
| **Origen** | Casos especiales [CE-17](../../02-requisitos/casos-especiales/CE-17-vehiculo-sin-placa-metalica.md), [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md), [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md), [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de conciliar registros con fuentes externas — [NRM-01](../normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-14. `[I]` la jerarquía de anclas: implicación de requerimiento del equipo |
| **Tipo** | Derivación + bloqueo duro |
| **Configurable** | Sí — parámetro `jerarquia_anclas_imputacion` |

## Enunciado

Toda **imputación externa** —notificación de infracción de tránsito, línea de estado de cuenta de peaje o de combustible, reclamo de seguro, acta de autoridad— **debe** resolverse contra un vehículo de la flota siguiendo una **jerarquía de anclas declarada y configurable**, cuyo orden inicial es:

1. Número de bien del inventario nacional
2. Chasis / VIN
3. Número de motor
4. Correlativo institucional
5. **Número de placa — en último lugar**, y resuelto **a la fecha del hecho** contra el historial de [`RN-64`](RN-64-estado-de-la-placa-tipificado.md)

Resuelto el vehículo, la imputación **debe** atribuirse además:

- al **tenedor vigente a la fecha del hecho** cuando el vehículo estaba prestado o cedido ([`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)) — sin que ello extinga la responsabilidad de la institución propietaria
- al **conductor registrado** de esa fecha y hora, cuando el hecho cae dentro de una jornada con conductor declarado ([`RN-57`](RN-57-habilitacion-de-quien-efectivamente-conduce.md))

Una imputación que **no se resuelve** no se asigna por parecido: queda **no resuelta**, con responsable de seguimiento y plazo.

## Justificación

**El mundo exterior indexa por placa.** La multa llega con una placa, el estado de cuenta del peaje trae una placa, el acta de la DNVT trae una placa. Y el sistema tiene vehículos sin lámina, vehículos cuyo número cambió, y vehículos alquilados cuya placa es del arrendador. [`RN-36`](RN-36-discrepancia-de-clasificacion-en-caseta.md) y [`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md) **presuponen** la atribución al vehículo; nada dice cómo se hace cuando la placa no alcanza.

La consecuencia de no tener la regla es previsible y ya ocurre en papel: cuatro multas del mismo motorista siguen siendo cuatro papeles sueltos, y el vehículo sin placa acumula imputaciones que nadie sabe dónde poner — o peor, que alguien pone donde le parece.

*"Una imputación sin resolver documentada es defendible; una imputación asignada por parecido no lo es."*

## Condiciones de aplicación

Aplica a toda imputación proveniente de una fuente externa a SIGTI, se descubra el mismo día o meses después.

Cuando la fecha del hecho de la imputación cae dentro de una Orden de Misión ya `CERRADA`, la imputación **no reabre el expediente**: abre expediente de hallazgo posterior ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)).

**No aplica** a los hechos registrados dentro del sistema — un paso por caseta capturado por el motorista ya está imputado al despachar.

## Comportamiento esperado

1. La resolución recorre la jerarquía en orden y se detiene en el primer ancla que identifica un solo vehículo. El sistema registra **qué ancla resolvió la imputación**, no solo el resultado.
2. Si un ancla devuelve más de un vehículo, o si la placa a esa fecha corresponde a más de un rango, la imputación queda **no resuelta** y va a cola de resolución humana. No se elige "el más probable".
3. Resuelto el vehículo, el sistema busca la Orden de Misión que cubría la fecha y hora del hecho y, dentro de ella, el conductor de esa jornada. Si no hay misión que la cubra, eso mismo es el hallazgo: el vehículo circuló sin amparo ([`RN-59`](RN-59-todo-uso-se-ampara-en-orden-de-mision.md)).
4. Toda imputación resuelta genera **acumulados por vehículo y por persona**: infracciones, montos, estado de pago y quién asume el costo. Cuatro multas del mismo conductor dejan de ser cuatro papeles y pasan a ser un indicador.
5. El sistema reporta las **imputaciones no resueltas** con antigüedad, responsable asignado y estado. El reporte en cero es la prueba de control; con renglones, es trabajo pendiente identificado.
6. Ninguna atribución al conductor determina por sí sola responsabilidad económica: eso exige acto administrativo ([`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).

## Casos límite

- **Multa de un vehículo prestado.** Se imputa al **tenedor a la fecha del hecho**, y el acta de préstamo declara quién asumía multas ([`RN-63`](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)). La institución propietaria sigue siendo responsable ante la autoridad; el sistema registra ambas cosas.
- **Paso por caseta un domingo, sin misión.** Es el caso de [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md): un hallazgo **sin misión vinculable**. El expediente de hallazgo posterior admite vincular cero misiones; el vehículo y el período bastan.
- **Placa que a la fecha del hecho pertenecía a otro vehículo.** El historial con rangos lo resuelve. Sin él, la multa se imputaría al vehículo equivocado y el error sería indetectable.
- **Vehículo alquilado con placa del arrendador.** El ancla que resuelve es el chasis o el correlativo institucional, no la placa; y el título de tenencia declara quién asume la multa.
- **Imputación de un período anterior al despliegue del sistema.** Se registra como no resuelta con causa *anterior al alcance del registro*. Declarada, no oculta.
- **Tag de peaje a nombre de un tercero.** `[C]` insumo #24. Mientras no se confirme, las líneas del estado de cuenta se resuelven por el vehículo asociado al tag en el catálogo, con la misma jerarquía.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]` — conciliación de registros
- Reglas relacionadas: [RN-15](RN-15-identidad-del-vehiculo-y-placa.md), [RN-36](RN-36-discrepancia-de-clasificacion-en-caseta.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md), [RN-59](RN-59-todo-uso-se-ampara-en-orden-de-mision.md), [RN-63](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md), [RN-64](RN-64-estado-de-la-placa-tipificado.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-95](RN-95-conciliacion-contra-fuentes-externas.md)
- Casos especiales: [CE-17](../../02-requisitos/casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) `RN-C17d` · [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) `RN-c:imputacion-de-multa-por-tenedor-a-la-fecha-del-hecho` · [CE-28](../../02-requisitos/casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) `RN-C28e`
- Insumos pendientes: #24 tags de peaje
