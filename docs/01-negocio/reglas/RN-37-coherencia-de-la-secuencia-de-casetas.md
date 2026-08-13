# RN-37 — La secuencia de pasos por caseta debe ser geográfica y temporalmente coherente con la ruta autorizada

| Campo | Valor |
|---|---|
| **Módulos** | M-18, M-08, M-13, M-14 |
| **Origen** | Norma [NRM-10](../normativa/NRM-10-peajes.md); [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — el auditor busca correlación |
| **Verificación** | `[V]` el orden geográfico de las estaciones del Corredor Logístico |
| **Tipo** | Advertencia con hallazgo |
| **Configurable** | Sí — `velocidad_media_maxima_por_tipo_vehiculo` y catálogo de distancias entre puntos |

## Enunciado

El sistema **debe** validar que la secuencia de pasos por caseta registrada en una misión sea coherente en tres dimensiones:

1. **Geográfica** — el orden de los puntos corresponde a un sentido de circulación posible. Zambrano → Siguatepeque → Yojoa es el sentido Tegucigalpa → San Pedro Sula `[V]`; una secuencia imposible es alerta.
2. **Temporal** — el intervalo entre dos casetas consecutivas es viable a la velocidad media máxima del tipo de vehículo. Un intervalo demasiado corto es físicamente imposible; uno excesivamente largo indica parada no registrada.
3. **Respecto de la ruta autorizada** — un peaje pagado en un punto que la ruta autorizada no atraviesa es un hallazgo.

Toda incoherencia **debe** producir alerta tipificada en la liquidación, sin bloquear el registro del paso.

## Justificación

[NRM-10](../normativa/NRM-10-peajes.md) lo exige textualmente, y explica por qué importa más que la suma de montos:

> *"El sistema debe correlacionar peaje × kilometraje × ruta autorizada. Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y el sistema tiene que producirlo solo. Esto es exactamente lo que busca el auditor del TSC: correlación, no comprobantes archivados."*

Es el mismo principio de [NRM-01](../normativa/NRM-01-control-interno-tsc.md): el auditor no revisa archivos, cruza datos. El sistema debe llegar al cruce antes que él.

## Condiciones de aplicación

Aplica a la liquidación de toda misión con pasos por caseta registrados, y al análisis agregado por vehículo, motorista y dependencia.

**No aplica** como bloqueo durante la ejecución: un paso incoherente ya ocurrió y debe registrarse tal cual. La regla observa, no impide.

La tercera dimensión se evalúa contra la **versión del alcance autorizado vigente a la fecha del hecho del paso** ([RN-77](RN-77-versionado-del-alcance-autorizado.md)), no contra el alcance original. **Un paso amparado por una extensión autorizada no es hallazgo.**

El **reordenamiento de destinos registrado con motivo** no constituye desviación de ruta si la secuencia resultante sigue siendo geográfica y temporalmente coherente ([RN-76](RN-76-estado-en-ruta-declarado-por-el-motorista.md)); el estimado de peajes se recalcula con el paquete normativo congelado.

## Comportamiento esperado

1. El catálogo de puntos incluye ubicación, corredor, kilómetro y sentido de cobro, lo que permite ordenar geográficamente `[V]`.
2. Cada incoherencia se tipifica: secuencia geográficamente imposible, intervalo inviable, punto fuera de ruta autorizada, paso duplicado, peaje sin kilometraje que lo respalde.
3. El cruce se hace además contra el **kilometraje de la bitácora**: si la misión declara 90 km recorridos y registra pasos por tres casetas separadas por cientos de kilómetros, la contradicción es doble y ambas reglas la señalan ([RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-31](RN-31-odometro-de-retorno.md)).
4. El sistema genera el **reporte de peajes por vehículo, motorista, dependencia y período** con estimado, pagado, desviación y evidencia — listo para entregar al TSC ([NRM-10](../normativa/NRM-10-peajes.md)).
5. Las desviaciones se tipifican con causa: cambio de tarifa entre aprobación y ejecución, ruta distinta a la autorizada, paso adicional no previsto, cobro en categoría equivocada, o peaje pagado sin paso registrado.

## Casos límite

- **Desvío legítimo por derrumbe, cierre de carretera o manifestación.** Honduras los tiene con regularidad. La incoherencia geográfica será real y justificada: el motorista debe poder registrar **evento en ruta con motivo** desde el campo, y ese evento es la explicación que la liquidación consume. Sin esa capacidad, la regla produciría hallazgos falsos en masa.
- **Reloj del dispositivo desajustado.** Puede fabricar intervalos imposibles. Se cruza contra la marca de tiempo del servidor al sincronizar y contra el ticket; si el reloj es no confiable ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)), la incoherencia temporal se marca como **no concluyente**.
- **Pasos registrados fuera de orden** porque el motorista los capturó todos al final del día. Es un problema de orden de captura, no de secuencia real: la validación usa la **fecha del hecho**, no el orden de ingreso ([RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
- **Misión con dos vehículos** por sustitución en ruta. La secuencia se valida por vehículo, no por misión: dos vehículos pueden pasar por la misma caseta a horas distintas legítimamente.
- **Tag CoviPass que registra un paso que el motorista no anotó.** Aparece en el estado de cuenta y no en la bitácora. Es la tipificación *peaje pagado sin paso registrado*, y puede significar tanto olvido como uso del vehículo fuera de misión — el segundo caso es el hallazgo grave.
- **Caseta cerrada o con libre paso ese día.** El estado del punto con vigencia ([RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md)) evita marcar como omisión un peaje que nadie cobró.
- **Misión de ruta abierta** sin ruta autorizada precisa. La tercera validación no aplica; las dos primeras sí. Se marca así explícitamente para que la ausencia de hallazgos no se lea como conformidad.

## Trazabilidad

- Normas: [NRM-10](../normativa/NRM-10-peajes.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-34](RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-66](RN-66-imputacion-externa-por-jerarquia-de-anclas.md), [RN-76](RN-76-estado-en-ruta-declarado-por-el-motorista.md), [RN-77](RN-77-versionado-del-alcance-autorizado.md), [RN-91](RN-91-categoria-y-tarifa-de-peaje-impresas-en-la-orden.md), [RN-95](RN-95-conciliacion-contra-fuentes-externas.md)
- Actores: ACT-04, ACT-06, ACT-08, ACT-12
- Casos especiales: [CE-08](../../02-requisitos/casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md), [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md), [CE-24](../../02-requisitos/casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md)
- Historias: pendientes — Bloque 4
