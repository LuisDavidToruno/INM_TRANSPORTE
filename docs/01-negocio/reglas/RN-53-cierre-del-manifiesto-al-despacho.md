# RN-53 — El manifiesto de personas y carga se cierra al despachar; los cambios en ruta se registran como novedad, no como edición

| Campo | Valor |
|---|---|
| **Módulos** | M-17, M-08, M-07, M-13 |
| **Origen** | Normas [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — registro oportuno; [NRM-06](../normativa/NRM-06-transito-y-licencias.md) — registro de carga; [NRM-02](../normativa/NRM-02-bienes-del-estado.md) — uso indebido |
| **Verificación** | `[V]` la exigencia de registrar tipo de carga, origen, destino, remitente y consignatario — `[V]` la prohibición de uso para tareas ajenas a la función |
| **Tipo** | Bloqueo duro sobre la edición; advertencia con hallazgo sobre las novedades |
| **Configurable** | No |

## Enunciado

Al despachar la misión, el manifiesto de personas trasladadas y de carga **se cierra**: la lista de personas a bordo y el detalle de carga (tipo, peso, origen, destino, remitente y consignatario) quedan fijados como lo autorizado.

Todo cambio posterior — persona que sube o baja, carga que se agrega o se entrega en un punto distinto — **debe** registrarse como **novedad de ruta** con fecha del hecho, motivo y quién la autorizó, y **no debe** modificar el manifiesto original.

La liquidación **debe** comparar manifiesto autorizado contra novedades y señalar las diferencias.

## Justificación

El manifiesto es la declaración de para qué salió el vehículo. Si se puede editar después, deja de ser una declaración y pasa a ser un resumen ajustado a lo que ocurrió — que es exactamente lo contrario de un control.

[NRM-02](../normativa/NRM-02-bienes-del-estado.md) `[V]` prohíbe el uso de vehículos del Estado para tareas ajenas a la función, *incluido el traslado de funcionarios, empleados y sus familias a residencias o asuntos personales*, y la Circular STLCC-ONADICI 022-03-2024 sobre uso indebido de vehículos `[V]` es reciente y específica. La única forma de detectar ese uso es comparar lo autorizado con lo ocurrido: sin manifiesto cerrado, no hay contra qué comparar.

[NRM-06](../normativa/NRM-06-transito-y-licencias.md) exige registrar del lado de la carga tipo, peso, origen, destino, remitente y consignatario, *"por trazabilidad operativa"*.

## Condiciones de aplicación

Aplica a toda misión con personas trasladadas o con carga.

**No aplica** a las misiones cuyo único objeto es el traslado del propio vehículo.

Las novedades se capturan desde el campo **sin conectividad** ([RN-43](RN-43-captura-de-campo-sin-conectividad.md)), porque es donde ocurren.

## Comportamiento esperado

1. Antes del despacho el manifiesto es editable y se valida contra compatibilidad ([RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md)) y capacidad ([RN-21](RN-21-capacidad-de-pasajeros-y-carga.md)).
2. Al despachar se congela ([RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)) y se imprime la versión que porta el motorista ([RN-25](RN-25-salvoconducto-con-folio-y-qr.md)).
3. Las novedades se tipifican: persona adicional, persona que no abordó, descenso en punto intermedio, carga adicional, entrega parcial, entrega a consignatario distinto.
4. Las entregas de carga registran **quién recibió**, con constancia — es el equivalente de la cadena de custodia aplicada a lo transportado.
5. La liquidación presenta manifiesto autorizado, novedades y resultado, tipificando las diferencias. Las que impliquen posible uso indebido se elevan a ACT-12 Auditor Interno.

## Casos límite

- **Persona adicional recogida en ruta.** Puede ser legítima (personal de la institución que se suma) o el caso clásico de uso indebido. El sistema **no juzga**: exige registrar quién autorizó el cambio y produce la comparación. La decisión es de la liquidación.
- **Exceso de capacidad producido por la novedad.** No se puede bloquear a un vehículo en ruta ([RN-21](RN-21-capacidad-de-pasajeros-y-carga.md)). Se registra, se alerta al Jefe de Transporte y produce hallazgo. Que el sistema lo señale es la única defensa institucional disponible.
- **Carga entregada a persona distinta del consignatario declarado.** Se registra el receptor real con constancia, no se corrige el consignatario. La diferencia es dato, no error.
- **Persona que baja antes del destino.** Se registra como descenso en punto intermedio con hora y lugar. Importa para el manifiesto de retorno y para el cálculo de tiempos (M-19).
- **Misión de recorrido con lista abierta** — transporte de personal en ruta fija con paradas. El manifiesto cerrado sería impracticable. `[C]` confirmar si la institución opera rutas de este tipo; de ser así, el manifiesto se cierra por tramo o se sustituye por conteo con puntos de abordaje, y esa variante debe modelarse explícitamente en lugar de forzar la regla general.
- **Traslado de personas externas con datos que no se pueden capturar completos** en el momento del abordaje. Se registra lo mínimo ([RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md)) y se completa después como novedad, con la marca de registro diferido ([RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
- **Novedad registrada después del retorno.** Si la bitácora ya se cerró, entra a la cola de conflictos ([RN-45](RN-45-cero-sobrescritura-silenciosa.md)) y se incorpora como **asiento de corrección sobre bitácora cerrada**, con su fecha del hecho. **No hay reapertura** — corregido por el hallazgo `HB1-04`; ver [RN-05](RN-05-registro-cerrado-no-se-edita.md) y [orden-de-mision.md §3.4](../../03-arquitectura/estados/orden-de-mision.md).

## Trazabilidad

- Normas: [NRM-02](../normativa/NRM-02-bienes-del-estado.md), [NRM-06](../normativa/NRM-06-transito-y-licencias.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-20](RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [RN-21](RN-21-capacidad-de-pasajeros-y-carga.md), [RN-51](RN-51-minimizacion-de-datos-de-personas-externas.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md)
- Actores: ACT-05, ACT-06, ACT-04, ACT-12
- Historias: [HU-114](../../02-requisitos/historias/HU-114-cerrar-el-manifiesto-al-despachar.md) cierre al despachar · [HU-115](../../02-requisitos/historias/HU-115-cadena-de-custodia-de-personas-externas.md) cadena de custodia · [HU-116](../../02-requisitos/historias/HU-116-registrar-novedades-del-manifiesto-en-ruta.md) novedades en ruta · [HU-125](../../02-requisitos/historias/HU-125-personas-externas-junto-con-carga-y-personal.md) junto con carga · Caso especial: [CE-18](../../02-requisitos/casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md)
