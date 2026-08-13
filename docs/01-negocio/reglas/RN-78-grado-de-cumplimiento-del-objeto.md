# RN-78 — Toda misión cierra declarando el grado de cumplimiento de su objeto, por destino y consolidado, con causa tipificada

| Campo | Valor |
|---|---|
| **Módulos** | M-13, M-08, M-14 |
| **Origen** | Casos especiales [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) y [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de evaluar el resultado de la ejecución frente a lo autorizado — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[I]` el grado de cumplimiento como dato obligatorio: implicación de requerimiento del equipo |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogos `grado_de_cumplimiento` y `causa_de_incumplimiento` |

## Enunciado

Toda Orden de Misión **debe** cerrar declarando el **grado de cumplimiento de su objeto**, tomado de un catálogo configurable, con **causa tipificada** cuando el grado no sea total.

El grado es **dato de cierre obligatorio, no observación de texto libre**. Sin él, la misión no se liquida.

En **misión multi-destino**, el grado se declara **por destino**, con **acta de entrega o constancia de no atención**, y la misión cierra con el **consolidado**.

El **retorno anticipado** registra **quién lo ordenó, cuándo y por qué medio**. Si lo decidió el conductor sin poder consultar, se registra así y se convalida al sincronizar ([`RN-73`](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).

## Justificación

Hoy una misión se liquida por su resultado económico —cuánto se gastó, cuánto se devolvió— y **nada dice si sirvió para algo**. Se puede cerrar limpia una misión de 600 km que no entregó nada porque la bodega estaba cerrada, y el expediente no lo va a decir en ningún lado excepto quizá en una observación que nadie agrega.

Con el grado declarado y tipificado aparece la información que la institución no tiene y que necesita: **cuántas misiones se abortaron, por qué causa, de qué dependencia y a qué destino**, con su costo atribuido. Una dependencia con seis misiones abortadas por *"la actividad se suspendió"* en un trimestre no tiene un problema de transporte: tiene un problema de planificación, y por primera vez hay evidencia para decirlo.

Es, además, una de las pocas cosas que una unidad de transporte puede llevar a una gestión presupuestaria con evidencia propia.

## Condiciones de aplicación

Aplica a toda Orden de Misión que llegue a `RETORNADA`, por ejecución normal o por retorno anticipado.

Aplica también a la misión **no ejecutada con consumo** (`T-16`), cuyo grado es *no ejecutada* con su causa.

**No aplica** a la misión anulada antes del despacho, que nunca tuvo ejecución que evaluar.

## Comportamiento esperado

1. El grado se declara al registrar el retorno o al liquidar, según lo defina la institución, y es requisito de `T-19`.
2. La **desviación de kilometraje y de rendimiento derivada de un retorno anticipado con causa registrada y aceptada no produce hallazgo por sí sola**: la conciliación se recalcula contra el trayecto efectivamente autorizado hasta el punto de retorno ([`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md), [`RN-77`](RN-77-versionado-del-alcance-autorizado.md)).
3. La misión que **repone a una abortada** se vincula explícitamente a ella, y el **costo acumulado de ambas se reporta junto**. La reprogramada es una misión nueva con su propio folio; no se "revive" la anulada.
4. El costo de cada misión abortada —combustible, peajes, kilometraje, días de vehículo— **se totaliza y se atribuye** a la dependencia solicitante y al destino.
5. Las misiones abortadas y los grados parciales se acumulan por causa, dependencia y destino como **indicador de calidad de la programación institucional** ([`RN-82`](RN-82-indicadores-de-calidad-de-la-programacion.md)).
6. En multi-destino, un destino no atendido exige **constancia de no atención** con hora, quién atendió o la constancia de que no había quién, y causa tipificada.

## Casos límite

- **`[C]` ¿Quién puede ordenar el retorno anticipado?** Insumo #50. Opciones y costo:

  | Opción | Costo |
  |---|---|
  | Solo ACT-04 Jefe de Transporte | Coherente con `T-17`, pero la decisión suele ser de la dependencia solicitante: es su actividad la que se cayó. Y en carretera sin señal, ACT-04 no existe |
  | La jefatura de la dependencia solicitante o ACT-04, indistintamente, con registro de cuál fue | Refleja la realidad. Costo: dos caminos de autorización para el mismo acto, distinguibles en el expediente |
  | Cualquiera de los dos, **más el conductor por sí mismo cuando hay riesgo** —vía cerrada, clima, seguridad— con convalidación posterior obligatoria | La única opción que no obliga a seguir manejando hacia un derrumbe esperando permiso. Hay que tipificar qué cuenta como riesgo y auditar las convalidaciones |

  **Recomendación del análisis, no decisión:** la tercera.
- **Grado declarado *total* con un destino no atendido.** El consolidado se deriva de los destinos; el sistema no admite un total que contradiga sus partes.
- **Misión cuyo objeto se cumplió parcialmente en un destino** — se entregó la mitad de la carga. Es entrega parcial con lo pendiente declarado ([`RN-69`](RN-69-inventario-de-la-carga-y-acta-de-entrega.md)), no un destino atendido.
- **Causa que no está en el catálogo.** Se registra la causa más próxima y se solicita la entrada nueva. Texto libre como única causa no se admite: un catálogo que no se puede agregar produce datos basura, y texto libre no produce indicador.
- **Retorno anticipado por falla del vehículo.** El grado es parcial o nulo, y la causa apunta al vehículo, no a la dependencia. La atribución del costo sigue a la causa.

## Trazabilidad

- Autoridad: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — `T-16`, `T-18` subtipo retorno anticipado, `T-19`
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-69](RN-69-inventario-de-la-carga-y-acta-de-entrega.md), [RN-73](RN-73-convalidacion-de-actos-sin-autorizacion-previa.md), [RN-76](RN-76-estado-en-ruta-declarado-por-el-motorista.md), [RN-77](RN-77-versionado-del-alcance-autorizado.md), [RN-79](RN-79-el-retorno-constatado-libera-al-vehiculo.md), [RN-82](RN-82-indicadores-de-calidad-de-la-programacion.md)
- Casos especiales: [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) — candidatas `RN-c:grado-de-cumplimiento-del-objeto`, `RN-c:autoria-de-la-decision-de-abortar`, `RN-c:desviacion-amparada-por-retorno-anticipado`, `RN-c:vinculo-entre-mision-abortada-y-su-reintento` · [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) `RN-c:cumplimiento-por-destino-en-mision-multidestino`
- Insumos pendientes: #50 quién puede ordenar el retorno anticipado
