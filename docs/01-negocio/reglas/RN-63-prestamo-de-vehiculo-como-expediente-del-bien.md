# RN-63 — El préstamo de un vehículo es un expediente del bien con receptor nombrado, fecha de devolución comprometida y actas; nunca una Orden de Misión

| Campo | Valor |
|---|---|
| **Módulos** | M-03, M-04, M-12, M-14 |
| **Origen** | Caso especial [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) · Norma [NRM-02](../normativa/NRM-02-bienes-del-estado.md) |
| **Verificación** | `[P]` el deber de custodia continua e identificable del bien del Estado — [NRM-02](../normativa/NRM-02-bienes-del-estado.md). `[I]` la separación préstamo / Orden de Misión: decisión de producto del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `motivo_de_prestamo` y umbrales de escalamiento por mora |

## Enunciado

La cesión temporal de la **tenencia** de un vehículo a otra dependencia o a otra institución **debe** modelarse como **expediente de préstamo** del bien, con:

1. **Acto autorizante** con folio, firmante identificado y documento adjunto
2. **Responsable receptor nombrado**, con cargo e institución, y su constancia de recepción
3. **Ventana con fecha de devolución comprometida**
4. **Acta de entrega**: odómetro fotografiado, nivel de combustible, inventario de accesorios, documentos entregados, estado de rotulación fotografiado y daños preexistentes
5. **Rubros pactados**: quién asume combustible, peajes, mantenimiento, multas y daños
6. **Acta de devolución** con odómetro fotografiado, novedades y **reconstatación de rotulación**

El préstamo **no debe** modelarse como Orden de Misión. En cambio, cuando el vehículo se cede **con motorista de la institución propietaria**, sí es una Orden de Misión con motivo *apoyo institucional*: ahí no se cedió la tenencia, se prestó un servicio.

El vehículo **no vuelve a `DISPONIBLE`** sin acta de devolución.

## Justificación

Una Orden de Misión tiene motorista, ruta, objeto del traslado y bitácora. **Un préstamo no tiene nada de eso**: tiene un receptor, una ventana y un compromiso de devolución. Forzarlo dentro del molde de la misión produce un expediente lleno de campos vacíos y, lo que importa más, **rompe la cadena de custodia**: durante semanas nadie puede decir quién respondía por la unidad.

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) exige que en cualquier fecha se pueda identificar al responsable de un bien del Estado. El préstamo es precisamente el período en que esa pregunta es más difícil y más probable que la hagan.

La distinción con el *apoyo institucional* no es formal: si va nuestro motorista, la responsabilidad de conducción, el combustible y la bitácora siguen siendo nuestros, y eso es una misión.

## Condiciones de aplicación

Aplica al préstamo entre dependencias de la misma institución y al préstamo a otra institución.

**No aplica** al comodato ni al alquiler recibidos, que son **títulos de tenencia** ([`RN-62`](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md)) y no préstamos otorgados.

**No aplica** a la reasignación permanente de un vehículo entre dependencias, que es cambio de ámbito y se rige por [`actores-y-roles.md`](../actores-y-roles.md).

## Comportamiento esperado

1. Iniciado el préstamo, el estado operativo del vehículo pasa a un estado que **no habilita asignación**, con el circuito completo de [`RN-60`](RN-60-indisponibilidad-sobrevenida-y-reservas.md): causa tipificada, ventana y acuse sobre las reservas afectadas.
2. **Quien autoriza el préstamo no puede ser el responsable receptor**, ni quien firma la devolución puede ser quien recibió. Es una incompatibilidad de segregación; su lugar propio es [`actores-y-roles.md`](../actores-y-roles.md) — autoridad en la materia — y desde [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) se propuso como par `I-c`. **Nota de hallazgo abierta** hasta que se incorpore allí.
3. El **kilometraje recorrido bajo tenencia ajena** se asienta con las dos lecturas de odómetro —entrega y devolución— y **no entra** en la conciliación galonaje–kilometraje del vehículo ([`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md)): no hubo consumo nuestro contra esos kilómetros.
4. **Vencida la fecha de devolución comprometida**, el préstamo alerta con escalamiento diario y entra al reporte de auditoría con los días de mora. **No puede cerrarse el período con préstamos vencidos** ([`RN-97`](RN-97-saldo-de-apertura-de-control-interno.md)).
5. Las infracciones y daños de la ventana se imputan al **tenedor a la fecha del hecho** ([`RN-66`](RN-66-imputacion-externa-por-jerarquia-de-anclas.md)), sin extinguir la responsabilidad de la institución propietaria.
6. Si la ventana comprende día u hora inhábil, el sistema exige el salvoconducto o la constancia de que el préstamo se limitó a días hábiles ([`RN-23`](RN-23-permiso-de-circulacion-en-dia-inhabil.md)).
7. En cualquier fecha del período, el sistema responde **quién respondía por la unidad**. Esa consulta es el entregable de la regla.

## Casos límite

- **Vehículo con orden de trabajo abierta, documentación por vencer dentro de la ventana, o incidente bajo investigación.** Bloqueo si la documentación vence dentro de la ventana —misma lógica de [`RN-10`](RN-10-licencia-vigente-en-todo-el-rango.md)— y advertencia registrada en los demás casos.
- **Alcance de datos de la dependencia tenedora.** El préstamo debería ampliar temporalmente su visibilidad sobre ese vehículo y retraerla al vencer. **No se resuelve aquí**: el alcance de datos es materia de [`actores-y-roles.md`](../actores-y-roles.md), que es la autoridad. **Nota de hallazgo abierta.**
- **Préstamo que se prorroga.** Es una nueva ventana con acto propio, no una edición de la anterior. La mora acumulada del compromiso original no se borra.
- **Devolución sin acta porque el receptor dejó el vehículo estacionado.** El préstamo sigue abierto y el vehículo `NO_DISPONIBLE` hasta que alguien constate y firme. Un vehículo que aparece no es un vehículo devuelto.
- **Préstamo a otra institución con nuestro motorista.** No es préstamo: es Orden de Misión con motivo *apoyo institucional*, con toda la bitácora, el combustible y la custodia nuestros.

## Trazabilidad

- Norma: [NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[P]`
- Autoridad de incompatibilidades y alcance de datos: [actores-y-roles.md](../actores-y-roles.md)
- Reglas relacionadas: [RN-22](RN-22-custodia-del-vehiculo.md), [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-58](RN-58-regimen-de-uso-del-vehiculo.md), [RN-60](RN-60-indisponibilidad-sobrevenida-y-reservas.md), [RN-62](RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md), [RN-97](RN-97-saldo-de-apertura-de-control-interno.md)
- Casos especiales: [CE-14](../../02-requisitos/casos-especiales/CE-14-vehiculo-prestado-entre-dependencias-o-instituciones.md) — candidatas `RN-c:prestamo-de-vehiculo-como-expediente-del-bien`, `RN-c:apoyo-con-motorista-propio-es-mision`, `RN-c:kilometraje-bajo-tenencia-ajena`, `RN-c:prestamo-vencido-no-devuelto`, `RN-c:devolucion-solo-con-acta-y-odometro`, `I-c:autoriza-prestamo × recibe-el-vehiculo`
