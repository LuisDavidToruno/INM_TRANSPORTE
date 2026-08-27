# RN-99 — La flota se constata físicamente con acta y comisión, y se concilia contra el registro de bienes

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-14, M-04, M-16 |
| **Origen** | Normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-15 *Inventarios Periódicos* y Circular CGR-010-2026 — y [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — *Manual de Propiedad Estatal*. Hallazgo `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) |
| **Verificación** | `[V]` que la **Circular CGR-010-2026** de la Contaduría General de la República, del 04/06/2026, sobre conciliación de bienes del ejercicio 2026, está vigente — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[P]` **TSC-NOGECI V-15**: la tabla de normas NOGECI de la ficha está verificada por citas en informes de auditoría del propio TSC, no por el texto. `[P]` que el *Manual de Propiedad Estatal* regule movimientos de inventario y descargo — [NRM-02](../normativa/NRM-02-bienes-del-estado.md), articulado no extraído. `[I]` **el acta, la comisión verificadora y la captura móvil**: implicación de requerimiento del equipo. `[C]` la periodicidad exigible y el formato oficial del acta |
| **Tipo** | Capacidad obligatoria con efecto sobre el estado operativo del vehículo |
| **Configurable** | Sí — `periodicidad_constatacion` y `efecto_constatacion_vencida`, con vigencia por rango de fechas |

## Por qué existe esta regla — hallazgo `HN1-18`

`actores-y-roles.md` creó **`ACT-14` Encargado de Bienes Institucionales** precisamente para esto, y **ninguna regla lo gobernaba**. [`RN-18`](RN-18-rotulacion-del-vehiculo-del-estado.md) cubre la constatación de **rotulación** —que las franjas y la leyenda estén— y eso no es el inventario: un vehículo puede estar impecablemente rotulado y no estar donde el registro de bienes dice que está.

> **Precisión sobre el nivel de verificación.** El hallazgo afirmaba que esta obligación es `[V]` por *«NOGECI V-15 y Circular CGR-010-2026»*. Solo la Circular está `[V]`. **La tabla NOGECI de [`NRM-01`](../normativa/NRM-01-control-interno-tsc.md) está `[P]`** —verificada por citas en informes del TSC, no por el texto normativo—, y el nivel no sube al bajar de la ficha a la regla ([`CLAUDE.md`](../../../CLAUDE.md)). Se corrige aquí en lugar de heredar la escalada.

## Enunciado

Todo vehículo de la flota **debe** tener **constatación física vigente**: una verificación presencial, registrada con acta, que confirme que el bien existe, dónde está, en qué estado, y que sus datos coinciden con el registro de bienes.

La constatación **caduca**. Vencida la periodicidad configurada, el vehículo queda con **constatación vencida** y el sistema aplica el efecto configurado en `efecto_constatacion_vencida`.

## Qué registra el acta de constatación

| Dato | Nota |
|---|---|
| **Fecha y hora del hecho** | No la de captura ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)) |
| **Comisión verificadora** | Las personas que constataron, con nombre y puesto. **No es una sola firma** |
| **Correlativo institucional, placa, motor, chasis/VIN** | Contra la ficha maestra; toda discrepancia se declara |
| **Odómetro** | Es el dato que más se contradice con la bitácora, y por eso el que más vale |
| **Ubicación** | Dónde estaba realmente el vehículo |
| **Estado físico y fotografías** | Captura móvil, con o sin red ([`RN-43`](RN-43-captura-de-campo-sin-conectividad.md)) |
| **Custodio que lo presentó** | Quién respondió por el bien en el acto ([`RN-22`](RN-22-custodia-del-vehiculo.md)) |
| **Hallazgos** | Tipificados; cada uno con su desenlace |
| **Resultado de la conciliación** contra el registro de bienes | Conforme, o con diferencia declarada |

## Justificación

La constatación física es el único control que **no depende de lo que el propio sistema registró**. Todo lo demás —bitácora, consumo, kilometraje— lo alimenta la operación; si la operación se equivocó o mintió, el sistema reproduce el error con toda coherencia interna. La verificación presencial es el punto donde el registro se contrasta con el mundo.

Y es hallazgo frecuente: el bien que figura en inventario y nadie sabe dónde está no aparece por una conciliación contable, aparece cuando alguien va a buscarlo.

**El odómetro constatado es el dato de mayor valor probatorio de toda esta regla.** Una constatación que registra 84.320 km sobre un vehículo cuya bitácora acumula 91.500 km detecta, de un solo golpe, o un uso no registrado o un kilometraje inflado — que es exactamente lo que sostiene la conciliación de [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md).

## Condiciones de aplicación

Aplica a **todo vehículo del registro**, cualquiera sea su régimen de tenencia: propio, comodato, alquilado o asignado por otra institución. Un bien ajeno bajo custodia de la institución se constata igual — con mayor razón, porque hay que devolverlo.

**No aplica** a vehículos ya retirados de la flota con descargo firme, cuya evidencia se conserva pero no se vuelve a constatar.

## Comportamiento esperado

1. La constatación la ejecuta **`ACT-14` Encargado de Bienes Institucionales**, o la comisión que la máxima autoridad designe. **No la ejecuta quien tiene la custodia del vehículo**: eso sería constatarse a sí mismo.
2. Se captura **sin conectividad** y sincroniza después ([`RN-43`](RN-43-captura-de-campo-sin-conectividad.md)). Las delegaciones sin red son justamente donde el bien lleva más tiempo sin que nadie lo mire.
3. Cada **diferencia** contra el registro de bienes abre un hallazgo con desenlace obligatorio: se corrige el registro, se localiza el bien, o se abre expediente de pérdida. **Ninguna diferencia se cierra sin desenlace.**
4. La constatación vencida **no borra** la anterior. Se conserva el histórico completo: la serie de constataciones de un vehículo es evidencia por sí misma.
5. El resultado alimenta el paquete de evidencia por vehículo y período ([`RN-98`](RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md)).

## Casos límite

- **El vehículo está en misión el día de la constatación.** No es una diferencia: es un estado. El acta lo registra como *constatado en ruta* con la Orden de Misión que lo respalda, o difiere la constatación de esa unidad y lo declara. Lo que no puede es darse por constatado sin verlo.
- **El vehículo no aparece.** No se marca *«no constatado»* y se sigue. Se abre expediente de pérdida y el vehículo pasa a `NO_DISPONIBLE`: un bien que no se encuentra no se despacha. Ver [`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md).
- **El odómetro constatado es menor que el último de bitácora.** Puede ser sustitución de odómetro, reinicio o error de captura. Se registra tal cual —**nunca se ajusta el acta para que cuadre**— y se resuelve por el circuito de [`RN-31`](RN-31-odometro-de-retorno.md). El acta es un hecho constatado, y un hecho constatado no se corrige para que encaje con el registro: es el registro el que tiene que explicarse.
- **La institución no tiene periodicidad definida.** `[C]` Es insumo pendiente con Auditoría Interna y con la Gerencia Administrativa. Mientras no se defina, `efecto_constatacion_vencida` arranca en **advertencia**, nunca en bloqueo: bloquear la flota entera por un parámetro que la institución no ha fijado es paralizarla por una decisión que no tomó.
- **El vehículo está en comodato y el comodante hace su propia constatación.** Se registra la constatación propia igualmente. La del tercero no sustituye la obligación de la institución sobre un bien que tiene bajo custodia.

## Trazabilidad

- **Normas**: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-15 `[P]`, Circular CGR-010-2026 `[V]` · [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — Manual de Propiedad Estatal `[P]`
- **Hallazgo que la origina**: `HN1-18` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- **Reglas relacionadas**: [`RN-18`](RN-18-rotulacion-del-vehiculo-del-estado.md) rotulación — es otra constatación, no ésta · [`RN-22`](RN-22-custodia-del-vehiculo.md) custodio · [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) · [`RN-31`](RN-31-odometro-de-retorno.md) · [`RN-43`](RN-43-captura-de-campo-sin-conectividad.md) · [`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) · [`RN-98`](RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md)
- **Actor**: `ACT-14` Encargado de Bienes Institucionales — [actores-y-roles.md](../actores-y-roles.md)
- **Módulo principal**: M-03 Flota Vehicular
