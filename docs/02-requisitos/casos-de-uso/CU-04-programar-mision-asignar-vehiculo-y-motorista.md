# CU-04 — Programar la misión: asignar vehículo y motorista

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor principal** | `ACT-04` Jefe de Transporte · `ACT-10` Encargado de Delegación en su ámbito territorial |
| **Actores secundarios** | `ACT-11` Encargado de Mantenimiento (declara la indisponibilidad del vehículo), `ACT-06` Motorista (recurso asignado), `ACT-08` Gerencia Administrativa (única que puede desplazar una programación existente), `ACT-07` Encargado de Combustible (recibe la propuesta de asignación de fondo), `ACT-13` Custodio del Vehículo, `ACT-17` Sistema de Talento Humano (espejo de disponibilidad), `ACT-16` Sistema ARGOS |
| **Precondiciones** | 1. Existe al menos una solicitud en `APROBADA` con `INV-09`, `INV-10` e `INV-11` satisfechos. 2. **La aprobación no ha caducado.** 3. El espejo de `ACT-17` no está desactualizado más allá del umbral configurable; si lo está, el sistema advierte antes de permitir asignar y la advertencia se imprime en el documento ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)). 4. Existe rango de folios disponible para la delegación. 5. Están vigentes, a las fechas de la misión, la matriz licencia↔vehículo, la matriz de compatibilidad de M-02, el calendario y la tabla de tarifas de peaje |
| **Postcondiciones** | El expediente está en `PROGRAMADA` con `INV-12` a `INV-16` verificables: exactamente un vehículo y un motorista titular asignados, más los relevos declarados con su propia verificación de licencia; **reserva exclusiva** de ambos sobre la ventana efectiva; el resultado de `BD-02` a `BD-04` registrado en el diario **con los datos concretos contra los que se evaluó**; el folio de la Orden de Misión **reservado, no consumido**; el estado operativo del vehículo en `ASIGNADO`. Existe la propuesta de asignación de fondo de combustible. **No hay ningún documento oficial emitido todavía** |
| **Disparador** | Una solicitud aprobada cae en la cola de programación de Transporte (`PR-01` E4) |

> Es la transición con más precondiciones del sistema y **el punto de mayor riesgo legal de todo el proceso**: asignar un motorista sin licencia habilitante traslada responsabilidad directa a quien autorizó `[P]` [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md).

## Flujo principal

1. `ACT-04` abre la cola de programación: solicitudes en `APROBADA`, con la **fecha de caducidad de cada aprobación** visible y las que salen antes señaladas.
2. `ACT-04` revisa las **oportunidades de consolidación**: dos o más solicitudes al mismo destino, en la misma ventana, con objetos de traslado compatibles, atendidas con un solo vehículo. Es el camino preferente y el que produce el ahorro real. Ver A1.
3. `ACT-04` selecciona un vehículo. El sistema evalúa la **compatibilidad tipo de vehículo ↔ objeto del traslado** — `BD-07`, `PC-07`: plazas suficientes, o capacidad de carga en peso y volumen, o **ambas dimensiones a la vez** en traslado mixto ([`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md), [`RN-67`](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md), [`RN-68`](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md)). **El tipo de vehículo es el eje de compatibilidad**, no la marca ni el modelo.
4. El sistema verifica el **estado operativo**: el vehículo está `DISPONIBLE`, no `EN_TALLER` ni `NO_DISPONIBLE`, sin orden de trabajo abierta que lo inmovilice — `BD-07`, `PC-05`, [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md). El estado lo declara `ACT-11`, no `ACT-04`. `[C]` Si el mantenimiento preventivo vencido bloquea o advierte — insumo #1.
5. El sistema verifica la **documentación del vehículo vigente durante todo el rango de la misión**, no solo el día de salida — `BD-03`, `PC-05`, `PC-06`:

   | Documento | Efecto | Regla |
   |---|---|---|
   | Matrícula | **Bloqueo duro** | `BD-03` |
   | Placa metálica ausente | **No bloquea** — es estado válido | [`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md), [`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) |
   | Póliza de seguro y revisión mecánica | **Advertencia registrada**; bloqueo solo si la institución lo activó, **apagado por defecto** | [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) |
   | Identificación institucional constatada — franjas, leyenda, siglas, correlativo | **Advertencia con la fecha de la última constatación** | [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) |
   | Título de tenencia | **Bloqueo**: ninguna misión excede su vigencia | [`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) |

6. El sistema verifica que el vehículo tenga **categoría de peaje resuelta y vigente**, derivada de la ficha técnica y no del número de ejes por sí solo ([`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)). Sin ella no hay tarifa esperada, y sin tarifa esperada la Orden de Misión no se puede emitir ([`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md)). Ver la nota de hallazgo `HCU-06`.
7. `ACT-04` selecciona el motorista. El sistema evalúa `BD-02` **licencia habilitante y vigente durante todo el rango**, en tres condiciones que deben cumplirse **las tres** — `PC-04`:
   1. **Habilitación por categoría**, resuelta contra la matriz licencia↔vehículo vigente a la fecha de salida prevista y **por los atributos de la ficha técnica**: tipo, peso bruto vehicular en kilogramos, capacidad de pasajeros y si es articulado ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md)).
   2. **Vigencia en todo el rango**: `vencimiento de la licencia ≥ fin de la ventana de la misión, incluida la holgura posterior`. Una licencia que vence el miércoles no habilita una misión que retorna el viernes ([`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md)).
   3. **Restricciones médicas compatibles** con las condiciones de la misión ([`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md)). `[C]` Catálogo de restricciones de la DNVT — insumo #42.

   **Bloqueo duro sin excepción configurable.** La licencia es **dato propio de SIGTI**, no espejo de `ACT-17` ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)).
8. El sistema aplica la verificación **sobre quien efectivamente va a conducir, cualquiera sea su puesto** ([`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md)). Si el conductor no pertenece al padrón de motoristas —funcionario asignatario, servidor de otra dependencia— el sistema exige identidad, número de licencia, categoría, vencimiento, restricciones y **fotografía de la licencia física**, y evalúa `RN-09` y `RN-10` **con el mismo rigor**. Ningún régimen de uso, jerarquía ni excepción operativa exime de esta verificación.
9. El sistema verifica `BD-10` **disponibilidad del motorista** contra el espejo de `ACT-17` — `PC-10`: sin vacaciones, permiso, incapacidad ni ausencia solapada con la ventana; sin otra misión asignada en esa franja; sin suspensión de habilitación derivada de un expediente de M-12 ([`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md)).
10. El sistema verifica `BD-11` **sin solapamiento de reserva** de vehículo ni de motorista, **incluidas las holguras**: `ventana_efectiva = [salida − holgura_previa, retorno + holgura_posterior]` ([`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), `EF-01`). `[C]` Valores de las holguras por institución y tipo de vehículo — insumo #1.
11. `ACT-04` confirma. El sistema ejecuta **`T-08` programar y asignar** y aplica sus efectos:
    - `EF-01` **reserva exclusiva** de vehículo y motorista sobre la ventana efectiva. El sistema **no sobre-asigna, ni siquiera con advertencia**.
    - `EF-02` **reserva del folio** de la Orden de Misión del rango de la delegación. Reservado, **no consumido**: se consume al despachar ([CU-05](CU-05-emitir-orden-de-mision-y-documentos.md)).
    - Registro del resultado de **cada** verificación con los datos concretos contra los que se evaluó: número de licencia, categoría, fecha de vencimiento, versión de la matriz licencia↔vehículo, atributos del vehículo usados, vencimientos consultados y fecha de fin de rango evaluada. **No basta con guardar "verificado: sí": esto es la defensa de quien autorizó ante un siniestro.**
    - El vehículo pasa a `ASIGNADO` (`W-03`); el motorista queda comprometido en esa franja.
    - Se **recalcula el estimado de peajes** con la tarifa vigente a la fecha ahora programada. Si difiere del **estimado ratificado en la aprobación** —congelado en `T-02` (`INV-07`) y ratificado en `T-05`, `HB1-17`— por encima del umbral configurable, **se exige nueva autorización antes de despachar**: lo autorizado tenía un costo y ese costo cambió.
    - Se genera la **propuesta de asignación de fondo de combustible**, que sigue su propia máquina (`V-01`) y se emite bajo `ACT-07` — `PC-08`.
12. El expediente queda en `PROGRAMADA`. En este estado **no se puede salir, no se puede entregar el fondo de combustible y no se puede imprimir la Orden de Misión como documento válido**: solo una vista previa marcada visiblemente como **no válida para circulación**.

## Flujos alternos

**A1 — Consolidación de solicitudes compatibles** (desde el paso 2)

1. Varias solicitudes aprobadas se atienden con una sola Orden de Misión. **Consolidar no fusiona**: cada solicitud conserva su expediente y su autorización.
2. Una actúa como **expediente rector** y las demás quedan vinculadas; desde `PROGRAMADA` en adelante todas siguen las transiciones del rector.
3. La **única transición que cada solicitud consolidada conserva por separado es `ANULADA`**: una dependencia puede desistir sin que la misión se caiga.
4. La liquidación se hace por misión, con **atribución de costo por solicitud vinculada** para el reporte por dependencia. `[C]` Criterio de prorrateo — insumo #1.
5. El escalamiento por segregación se evalúa **por cada solicitud componente**: basta un conflicto para escalar la orden completa ([`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md)).

**A2 — Reasignación de vehículo o de motorista** (desde el paso 12)

1. `ACT-04` ejecuta `T-10`, que exige **todas las precondiciones de `T-08` para el recurso entrante**: `BD-02`, `BD-03`, `BD-07`, `BD-10`, `BD-11` se revalidan íntegras ([`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md)).
2. Se libera la reserva del saliente y se crea la del entrante. **Se conserva la trazabilidad de la asignación original**: el diario muestra a quién se había asignado, por qué se cambió y a quién se asignó (`DP-001` D-07).
3. Motivo obligatorio y **tipificado**: vehículo a taller, motorista no disponible, cambio de requerimiento, consolidación.
4. **El folio reservado no cambia**: es el mismo expediente.
5. Si el recurso sustituido es el vehículo, **todo valor derivado se recalcula y se vuelve a congelar con asiento de diferencia**: categoría de peaje, tarifas esperadas, rendimiento esperado ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)). Y si había permiso de circulación, deja de cubrir la misión y se reemite ([CU-03](CU-03-permiso-de-circulacion-en-dia-inhabil.md) E2).

**A3 — Desprogramar y devolver a la cola** (desde el paso 12)

1. `ACT-04` ejecuta `T-11` con motivo obligatorio, siempre que no se haya despachado.
2. Se liberan las reservas de vehículo y motorista; el vehículo vuelve a `DISPONIBLE`.
3. **El folio reservado se anula**; al reprogramar se reserva uno nuevo. Un folio reservado que no se consume **se anula, no se recicla ni se devuelve al rango**.
4. La solicitud vuelve a `APROBADA` **conservando su aprobación original**: no se vuelve a autorizar.

**A4 — Motoristas de relevo declarados en la programación** (desde el paso 7)

1. `INV-12` admite motoristas de relevo además del titular, **cada uno con su propia verificación de licencia** contra el mismo vehículo y el mismo rango.
2. El padrón de relevo se incorpora al paquete de misión para que el relevo en ruta se pueda validar **sin conectividad** ([CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md)).

**A5 — Conflicto por el mismo recurso** (en el paso 10)

El sistema **no encola en silencio ni rechaza**: muestra el conflicto con su titular —qué misión tiene tomado el recurso, de qué dependencia, en qué franja— y ofrece cuatro caminos **en este orden** (`EF-01`):

1. **Consolidar**, si las misiones comparten ruta compatible y hay capacidad.
2. **Asignar otro recurso**: el sistema propone vehículos compatibles y motoristas habilitados libres en la franja.
3. **Reprogramar** una de las dos, con acuerdo registrado de la dependencia afectada.
4. **Escalar la prioridad.** Solo `ACT-08` puede desplazar una programación existente, y hacerlo **libera la primera misión a `APROBADA` mediante `T-11`**, con motivo obligatorio *desplazada por prioridad superior* y notificación a la dependencia afectada. Nunca se le quita el vehículo a una misión sin devolverla explícitamente a la cola: una misión que pierde su vehículo en silencio se descubre el día de la salida, en el predio.

Cada conflicto registrado con su resolución **es la medición del déficit de flota**, y es uno de los pocos indicadores que la institución puede llevar a una gestión presupuestaria con evidencia ([`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md)).

**A6 — Programación en delegación** (desde el paso 1)

1. `ACT-10` programa dentro de su ámbito territorial con las mismas precondiciones. **Requiere red**: asignar contra datos viejos es asignar mal.
2. Si la sincronización lleva demasiado detenida, el sistema **advierte antes de permitir asignar** y la advertencia queda registrada y **se imprime en el documento** ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)).
3. `ACT-10` **no levanta el núcleo irreductible** `I-07`, `I-10`, `I-11` ni bajo ningún régimen. El régimen de excepción está **suspendido** por [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md): la vía es el escalamiento a sede.

## Flujos de excepción

**E1 — La licencia no habilita el vehículo, o vence dentro del rango** (en el paso 7) — **bloqueo duro**

1. `BD-02` impide la asignación. **No existe la opción de forzarlo, no hay excepción configurable y no la habilita ninguna jerarquía** (`DP-001` D-12: *"una excepción registrada sería evidencia en contra ante un siniestro"*).
2. El sistema muestra el motivo concreto: categoría que tiene contra categoría requerida por los atributos del vehículo, o fecha de vencimiento contra fecha de fin de rango evaluada.
3. `ACT-04` sustituye el motorista o el vehículo por `T-10`, o desprograma por `T-11`.
4. **La verificación se repite en el despacho** (`T-12`) y en toda prórroga (`T-17`): entre programar y salir pueden pasar días, y una licencia puede vencer en el medio. Ver [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md).
5. **Licencia en trámite de renovación con comprobante de la DNVT**: se registra como habilitación provisional que **no levanta el bloqueo**. `[C]` [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) no lo resuelve — insumo #20.

**E2 — El vehículo entra a taller con misiones ya programadas** (después del paso 12)

1. `ACT-11` declara la indisponibilidad. La indisponibilidad sobrevenida exige **causa, ventana estimada y desenlace explícito de cada reserva afectada**: ninguna queda sin resolver ([`RN-60`](../../01-negocio/reglas/RN-60-indisponibilidad-sobrevenida-y-reservas.md)).
2. Cada misión afectada se resuelve por `T-10` sustituyendo vehículo, o por `T-11` volviendo a la cola.
3. La sustitución **recalcula y vuelve a congelar** categoría de peaje, tarifas esperadas y rendimiento, con asiento de diferencia ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)). Ver [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md).

**E3 — El motorista queda no disponible** (en el paso 9 o después)

1. `BD-10` bloquea la asignación; si ya estaba asignado, la novedad llega por el espejo de `ACT-17`.
2. La misión **se cubre con otro motorista por `T-10`, revalidando `BD-02` y `BD-10`**, y la asignación original **queda en el historial** (`DP-001` D-07). Ver [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md).
3. `[C]` Qué ocurre con un empleado dado de baja en `ACT-17` que tiene misiones abiertas en SIGTI — pendiente abierto de [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), insumo #17.

**E4 — El vehículo no tiene placa metálica** (en el paso 5)

1. **Es un estado válido**: hay desabastecimiento nacional `[V]` [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md). Un campo `placa` obligatorio y único rompería el sistema.
2. El estado de la placa es **dato tipificado con historial y vigencia**, distinto del número asignado ([`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md)).
3. Sin lámina se exige **respaldo vigente en todo el rango** —constancia o documento sustitutivo del IP— y **paquete de identificación impreso y acusado** ([`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md)); el umbral de caducidad de la constatación de rotulación es más corto en este caso ([`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md)).
4. La imputación externa de peajes, multas y siniestros se resuelve por **jerarquía de anclas a la fecha del hecho, con la placa en último lugar** ([`RN-66`](../../01-negocio/reglas/RN-66-imputacion-externa-por-jerarquia-de-anclas.md)). Ver [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md).

**E5 — Póliza o revisión mecánica vencidas** (en el paso 5)

1. **No son obligatorias por ley vigente** `[V]` [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md): son rastreables y alertables, y el bloqueo es **regla configurable apagada por defecto** ([`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md), `DP-001` D-13).
2. Si la institución no activó el bloqueo, `ACT-04` puede continuar y la **advertencia queda visible en el expediente con su nombre**. Una advertencia que nadie ve no es un control: es lo que el auditor pregunta.
3. El valor del parámetro puede ser **distinto por régimen de tenencia** —propio, comodato, alquilado— ([`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)). Ver [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md).

**E6 — No hay ningún vehículo compatible libre, o dos solicitudes compiten por el único** (en el paso 3)

1. El sistema aplica el **criterio de prelación parametrizado** y deja **constancia de las solicitudes desplazadas** ([`RN-56`](../../01-negocio/reglas/RN-56-prelacion-entre-solicitudes-que-compiten.md)).
2. `[C]` **El criterio de prelación no está definido** — insumo #31. Sin criterio explícito lo resuelve quien tenga más jerarquía, que es exactamente lo que el sistema debe evitar. **No se inventa.**
3. Si no hay salida, `ACT-04` anula por `T-09` con **motivo tipificado**: sin flota disponible, sin motorista habilitado, caducada por falta de programación, desistimiento, causa externa. La tipificación es el indicador de déficit de flota; un texto libre aquí no produce ningún indicador. Ver [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md).

**E7 — La aprobación caducó sin programarse** (en el paso 1)

1. Vencida la ventana solicitada sin programación, el sistema marca la aprobación como caducada y **exige a `ACT-04` anularla por `T-09` con motivo tipificado**.
2. Una cola de aprobadas que nadie depura **oculta el déficit real de flota**, que es justamente el indicador que la institución necesita.

**E8 — El estimado de peajes recalculado difiere de lo autorizado por encima del umbral** (en el paso 11)

1. El sistema **exige nueva autorización antes de despachar**: lo que se autorizó tenía un costo y ese costo cambió.
2. La diferencia se conserva como asiento; el valor histórico **no se sobrescribe** ([`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).
3. `[C]` Umbral — insumo #1. `[C]` Tarifa efectivamente vigente — insumo #21.

**E9 — El vehículo está bajo un régimen que no es el de pool** (en el paso 3)

1. **Asignado a funcionario**: el uso se ampara igualmente en Orden de Misión ([`RN-59`](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md)) y quien conduce se verifica igual ([`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md)). Ver [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md).
2. **Prestado a otra dependencia o institución**: el préstamo es **expediente del bien con receptor, fecha comprometida y actas — nunca una Orden de Misión** ([`RN-63`](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md)). El vehículo no está disponible para programar. Ver [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md).
3. **Comodato o alquilado**: ninguna misión excede la vigencia del título de tenencia ([`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)).

## Reglas aplicables

| Regla | Qué gobierna en este caso de uso |
|---|---|
| [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) | `BD-02` en sus tres condiciones. **Bloqueo duro sin excepción** |
| [`RN-57`](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) | La habilitación se verifica sobre **quien conduce**, no sobre el puesto de motorista |
| [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md), [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) | `BD-10`, `BD-11` y la revalidación íntegra al sustituir |
| [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md), [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md), [`RN-18`](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) | `BD-03` en su parte configurable y las alertas anticipadas |
| [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md), [`RN-60`](../../01-negocio/reglas/RN-60-indisponibilidad-sobrevenida-y-reservas.md), [`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) | Estado operativo, indisponibilidad sobrevenida y recálculo al sustituir |
| [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md), [`RN-67`](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md), [`RN-68`](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md) | `BD-07` compatibilidad y capacidad, por tramo y objeto a objeto |
| [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) | Todo vehículo tiene custodio vigente antes de comprometerse a una misión |
| [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) | Categoría resuelta, recálculo del estimado y asiento de la diferencia |
| [`RN-56`](../../01-negocio/reglas/RN-56-prelacion-entre-solicitudes-que-compiten.md), [`RN-82`](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) | Prelación entre solicitudes que compiten e indicadores de calidad de la programación |
| [`RN-58`](../../01-negocio/reglas/RN-58-regimen-de-uso-del-vehiculo.md), [`RN-59`](../../01-negocio/reglas/RN-59-todo-uso-se-ampara-en-orden-de-mision.md), [`RN-62`](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md), [`RN-63`](../../01-negocio/reglas/RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md), [`RN-64`](../../01-negocio/reglas/RN-64-estado-de-la-placa-tipificado.md), [`RN-65`](../../01-negocio/reglas/RN-65-sin-lamina-respaldo-y-paquete-de-identificacion.md) | Régimen de uso, tenencia, préstamo y estado de la placa |
| [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) | Degradación explícita si el espejo de `ACT-17` lleva detenido más del umbral |

## Notas de hallazgo

**`HCU-06` — la categoría de peaje sin resolver: ¿advierte o bloquea?** Tres artefactos dicen tres cosas. `PR-01` E5 y `PC-06` la tratan como **advertencia** (*"sin ella el estimado de peajes no es confiable"*); `T-08` la lista entre sus **precondiciones** y `BD-07` la exige *"resuelta y vigente"*, es decir **bloqueo**; [`RN-91`](../../01-negocio/reglas/RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md) bloquea **en el despacho**. Este caso de uso sigue a la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), que es la autoridad en precondiciones: **bloquea en `T-08`**. Se reporta contra `PR-01` E5 y `PC-06`, que no son autoridad en la materia. La consecuencia práctica de dejarlo como advertencia sería llegar al despacho con un documento que `RN-91` no deja imprimir, y descubrirlo con el motorista ya en el predio.

**`HCU-07` — `PR-01` E5 y el diagrama 3.1 evalúan la licencia antes que la disponibilidad del vehículo; `T-08` no fija orden.** No es contradicción de contenido —se evalúan todas— pero sí de presentación: si el sistema corta en el primer bloqueo, `ACT-04` corrige de a uno y vuelve a chocar. Recomendación operativa, no hallazgo normativo: **evaluar el conjunto completo y presentar todos los bloqueos y advertencias a la vez**, con los datos concretos de cada uno.

## Trazabilidad

- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas E4 y E5; puntos de control `PC-04`, `PC-05`, `PC-06`, `PC-07`, `PC-10`, `PC-16`; variantes V-01, V-02, V-04
- **Transiciones**: `T-08` programar y asignar, `T-09` anular aprobada, `T-10` reasignar recurso, `T-11` liberar recursos — [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)
- **Invariantes**: `INV-12` a `INV-16`
- **Bloqueos**: `BD-02` licencia, `BD-03` documentación, `BD-07` estado y compatibilidad, `BD-10` disponibilidad del motorista, `BD-11` sin solapamiento de reserva
- **Efectos**: `EF-01` reserva de vehículo y motorista, `EF-02` reserva del folio · Máquinas secundarias: `V-01` asignación de fondo, `W-03` estado operativo del vehículo
- **Actores**: [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) — `ACT-04`, `ACT-06`, `ACT-07`, `ACT-08`, `ACT-10`, `ACT-11`, `ACT-13`, `ACT-16`, `ACT-17`; `I-15` custodio que autoriza la salida de su propio vehículo, **advertencia con motivo escrito**
- **Casos especiales**: [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) (E1), [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) (E6), [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) (E3), [CE-14](../casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) y [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) y [CE-19](../casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md) (E9), [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) (E2), [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) (E4), [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) (paso 3), [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) (A4). **Descartado explícitamente:** [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) — el fondo agotado bloquea la **emisión de la asignación** (`PC-08`), no la programación; despachar sin fondo asignado es posible y queda como decisión registrada con responsable `[C]`
- **Normativa**: [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[P]` matriz licencia↔vehículo, pendiente el texto reformado del Art. 48 · [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` identificación del vehículo del Estado y tarjeta de responsabilidad `[P]` · [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) `[P]` categoría por atributos del vehículo · [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) `[P]` registro de los insumos de la verificación
- **Decisiones**: [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-07, D-08, D-12, D-13 · [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md) · [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — **la licencia es dato propio de SIGTI**
- **Insumos pendientes**: #1, #17, #20, #21, #23, #27, #31, #42 en [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md)
- **Aguas arriba**: [CU-02](CU-02-autorizar-solicitud-de-transporte.md) · **Aguas abajo**: [CU-05](CU-05-emitir-orden-de-mision-y-documentos.md), y el despacho `T-12`
