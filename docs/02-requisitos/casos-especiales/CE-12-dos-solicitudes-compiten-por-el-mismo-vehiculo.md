# CE-12 — Dos solicitudes aprobadas compiten por el único vehículo compatible disponible

| Campo | Valor |
|---|---|
| **Módulos** | M-07 Programación y Despacho, M-06 Solicitudes, M-03 Flota, M-14 Reportes |
| **Estados afectados** | `APROBADA`, `APROBADA → PROGRAMADA` (`T-08`), `APROBADA → ANULADA` (`T-09`) |
| **Frecuencia** | Frecuente — aparece la primera semana de operación |
| **Impacto** | Operativo y de control interno |
| **Resolución** | **`[C]` Escalada al PO** — el criterio de prelación no está definido (insumo #31). Lo que sí se define aquí es el comportamiento que no depende de ese criterio |

## La situación

Martes 12 de mayo. En la cola de programación hay dos solicitudes **ya autorizadas**:

| | Solicitud A | Solicitud B |
|---|---|---|
| Dependencia | Dirección de Delegaciones | Unidad de Almacén |
| Objeto del traslado | 3 servidores públicos a supervisión | 4 archivadores metálicos y 12 cajas de expedientes |
| Destino | San Marcos de Colón, Choluteca | La Esperanza, Intibucá |
| Ventana | 12/05 06:00 → 13/05 18:00 | 12/05 07:00 → 12/05 20:00 |
| Autorizada el | 08/05 a las 14:20 | 09/05 a las 09:05 |
| Tipo de vehículo requerido | Pickup 4x4 doble cabina | Pickup 4x4 |

La flota disponible ese día: `INM-0087` pickup 4x4 doble cabina, **el único compatible**. Los otros dos pickups están `EN_TALLER` — uno por cambio de embrague, otro esperando repuesto desde hace tres semanas. Quedan dos sedanes, que no sirven ni para la carretera de tierra hacia San Marcos de Colón ni para cargar archivadores.

Las dos solicitudes están aprobadas. Las dos son legítimas. Hay un vehículo.

## Qué se hace hoy sin sistema

`[C]` No confirmado con la institución — es exactamente el insumo #31.

Lo que ocurre de hecho en instituciones sin criterio escrito `[I]`:

- **Se resuelve por teléfono.** El Jefe de Transporte llama a las dos jefaturas y "acomoda".
- **Gana quien tiene más rango.** Si una solicitud viene de una Dirección y la otra de una Unidad, la Dirección se lleva el vehículo. Nadie lo escribe, todos lo saben.
- **Gana quien llegó primero al despacho físicamente.** El motorista de una de las dos aparece a las 5:40 de la mañana con la orden en la mano y las llaves ya están asignadas.
- **La solicitud perdedora desaparece.** No se anula formalmente, no se reprograma: se queda en el fólder y a los quince días alguien pregunta qué pasó con los archivadores.

El último punto es el daño real. La solicitud desplazada **no deja rastro**, y por eso la institución nunca puede demostrar cuántas movilizaciones no pudo atender por falta de flota — que es justamente el argumento con el que se pide presupuesto para comprar vehículos.

## Por qué el flujo normal no lo cubre

`T-08` reserva el vehículo en exclusiva (`EF-01`) y [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) impide que un segundo intento tome el mismo vehículo en ventanas traslapadas. Técnicamente el sistema **no falla**: la segunda programación se bloquea.

El problema es otro: **el sistema no dice quién tiene derecho**. Con la mecánica actual, gana quien hace clic primero. Eso no es un criterio, es una carrera — y una carrera que en la práctica gana quien tiene línea directa con el Jefe de Transporte.

Además:

- La máquina de estados es explícita en que **aprobar no reserva flota** (`T-05`): *"quien autoriza la pertinencia del viaje no es quien conoce la disponibilidad de la flota"*. La consecuencia deliberada de esa separación es que se aprueba más de lo que se puede atender, y la cola de `APROBADA` es donde se acumula el déficit.
- `T-09` exige motivo **tipificado** para anular una aprobada, e incluye "sin flota disponible". Pero nada obliga a usarlo cuando la causa real fue que otra solicitud se llevó el vehículo.

## Regla de resolución

### Lo que se define ahora, sin esperar el criterio de prelación

Estas cinco cosas no dependen de quién gane, y deben implementarse igual:

**1. El conflicto se muestra antes de adjudicar, no después.**
Al programar una solicitud, si el recurso elegido es el único compatible y existen otras solicitudes aprobadas con ventana traslapada que también lo requieren, el sistema **lista esas solicitudes en la pantalla de programación**, con su dependencia, motivo de viaje, fecha de autorización y destino. Programar sin ver a quién se está desplazando es el defecto de origen.

**2. Toda adjudicación de recurso en conflicto deja constancia.**
El diario de `T-08` registra, además de lo habitual: la lista de solicitudes desplazadas, el criterio invocado y el **motivo tipificado** de la adjudicación. Es la aplicación directa de [`RN-06`](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) — cada transición registra actor, rol, momento y motivo.

**3. La solicitud desplazada no se pierde ni se anula.**
Vuelve a la cola de programación con la marca **desplazada por conflicto de flota**, referencia a la Orden de Misión que se llevó el recurso, y un contador de veces desplazada. Una solicitud desplazada tres veces debe subir en la lista de trabajo del Jefe de Transporte sea cual sea el criterio que se adopte.

**4. Antes de adjudicar, el sistema evalúa consolidación.**
La máquina de estados ya prevé que **varias solicitudes aprobadas se atiendan con una sola Orden de Misión** (§0.3), con expediente rector y atribución de costo por solicitud vinculada. El sistema debe **proponer la consolidación** cuando dos solicitudes en conflicto tienen ventana compatible y rutas encadenables, y debe registrar por qué no se consolidó cuando no procede. En el ejemplo no procede: Choluteca al sur y La Esperanza al occidente, en sentidos opuestos.

**5. Lo que caduca alimenta el indicador de déficit.**
La solicitud desplazada que llega al inicio de su ventana sin programación caduca y se anula por `T-09` con motivo tipificado **sin flota disponible**, conservando la referencia a los desplazamientos que sufrió. El reporte de M-14 debe poder responder: *cuántas movilizaciones autorizadas no se ejecutaron por falta de flota, de qué dependencias, de qué tipo de vehículo y en qué meses.*

### Lo que se escala al PO — criterio de prelación `[C]`

**Insumo #31.** No se inventa aquí. Estas son las opciones razonables con su consecuencia:

| Opción | Cómo funciona | A favor | En contra |
|---|---|---|---|
| **A — Orden de autorización (FIFO)** | Gana la solicitud autorizada primero. En el ejemplo, la Solicitud A (08/05 14:20) | Objetivo, auditable, imposible de manipular salvo autorizando antes | Ignora la urgencia real. Una supervisión rutinaria desplaza un traslado de emergencia por haberse autorizado dos días antes |
| **B — Prioridad del motivo de viaje** | El catálogo `motivo_viaje` (M-02) lleva un nivel de prioridad configurable con vigencia. Gana el motivo de mayor prioridad; empate se rompe por FIFO | Refleja la misión institucional. Es parametrizable, no cableado — cumple [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) | Manipulable: el solicitante elige el motivo. Exige que alguien con autoridad fije y mantenga la prioridad de cada motivo |
| **C — Jerarquía de la dependencia solicitante** | Gana la dependencia de mayor nivel en el organigrama espejado de ARGOS | Es lo que ya pasa | **Institucionaliza el problema.** El sistema pasaría a ejecutar automáticamente el sesgo que el control interno pretende neutralizar. No recomendable |
| **D — Decisión discrecional registrada** | Decide ACT-04 Jefe de Transporte, con motivo tipificado obligatorio, lista de desplazadas y notificación automática a las dependencias desplazadas | No pretende eliminar el juicio operativo, que es real: el Jefe de Transporte sabe cosas que el catálogo no. Lo hace **visible y contable** | No previene el favoritismo; solo lo documenta. Requiere revisión periódica del patrón de adjudicaciones por ACT-12 |
| **E — Combinada (B con desempate D)** | Prioridad del motivo como orden por defecto; ACT-04 puede apartarse **justificando por escrito** y la excepción entra al reporte de auditoría | Da criterio por defecto y deja salida operativa con costo de justificación | Más piezas que mantener: catálogo de prioridades, motivos de excepción y reporte de excepciones |

**Recomendación del análisis, no decisión:** la opción **E**. Da un orden por defecto verificable y hace que apartarse cueste una justificación registrada, que es el mecanismo estándar del control interno. La opción **C** debería descartarse expresamente, porque convierte una práctica cuestionada en comportamiento del sistema.

**Costo de no decidir:** el sistema queda sin criterio y la adjudicación la resuelve el orden de llegada a la pantalla de programación — que es, en la práctica, la opción C con otro nombre.

### Reglas candidatas

**`RN-56` (candidata) — Prelación entre solicitudes que compiten por el mismo recurso.** `[C]` pendiente del criterio.

> Cuando dos o más solicitudes aprobadas requieren el mismo vehículo en ventanas traslapadas y no existe otro compatible disponible, el sistema resuelve la adjudicación aplicando el criterio de prelación **parametrizado con vigencia**, y ninguna adjudicación se completa sin registrar el criterio aplicado y las solicitudes desplazadas.

**`RN-57` (candidata) — Constancia de adjudicación y de desplazamiento.** No depende del `[C]` anterior y puede escribirse ya.

> Toda programación que desplace a otra solicitud aprobada registra en el diario la lista de desplazadas con su referencia. La solicitud desplazada conserva su aprobación, vuelve a la cola con marca y contador, y su eventual caducidad se anula con motivo tipificado *sin flota disponible*.

Ninguna de las 54 reglas vigentes cubre la prelación ni el registro del desplazamiento. [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) impide la doble asignación, pero es la consecuencia del conflicto, no su resolución.

## Evidencia que debe quedar

1. El **diario de `T-08`** de la misión adjudicada, con la lista de solicitudes desplazadas, el criterio invocado y el motivo tipificado.
2. La **notificación a la dependencia desplazada**, con acuse: cuándo se le informó y quién lo hizo. Sin esto, el reclamo de la Unidad de Almacén no tiene contraparte.
3. El **expediente de la solicitud desplazada** con su historial: cuántas veces fue desplazada, por qué misiones y en qué fechas.
4. Si caducó: la anulación por `T-09` con motivo **tipificado**, no de texto libre.
5. La **evaluación de consolidación**: que se evaluó y por qué no procedió. Es la defensa contra el hallazgo de "se pudo hacer un solo viaje y se hicieron dos".
6. El **reporte de disponibilidad de flota** del período: vehículos por estado operativo, días de indisponibilidad y órdenes de trabajo abiertas. Es la prueba de que el único vehículo compatible era realmente el único — el resto estaba en taller, con su expediente.
7. El **reporte de demanda no atendida** por dependencia, tipo de vehículo y período.

## Trazabilidad

- Reglas: [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md), [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md), [`RN-06`](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md), [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)
- Reglas candidatas: `RN-56` (prelación, `[C]`), `RN-57` (constancia de adjudicación y desplazamiento)
- Transiciones: `T-05`, `T-08`, `T-09`, `T-11` · [orden-de-mision.md §0.3](../../03-arquitectura/estados/orden-de-mision.md) consolidación
- Actores: ACT-04, ACT-10, ACT-03, ACT-12
- Casos especiales relacionados: `CE-16` (vehículo a mantenimiento con misiones programadas), `CE-19` (vehículo asignado permanentemente frente al de pool — reduce la flota de pool y agrava este caso)
- **Insumo #31** — criterio de prelación. Bloqueante para `RN-56`
