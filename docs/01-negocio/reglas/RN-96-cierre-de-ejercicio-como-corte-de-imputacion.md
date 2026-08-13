# RN-96 — El cierre de ejercicio fiscal es un corte de imputación y de reporte; ningún expediente cambia de estado por efecto de una fecha

| Campo | Valor |
|---|---|
| **Módulos** | M-13, M-09, M-18, M-14, M-20 |
| **Origen** | Caso especial [CE-27](../../02-requisitos/casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) · Norma [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) |
| **Verificación** | `[I]` la exigencia de manejar cierre y apertura de ejercicio en [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) es **implicación de requerimiento escrita por el equipo**, no articulado citable. `[C]` el criterio de imputación entre ejercicios depende de SIAFI |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — fechas de corte legal y operativa, con vigencia |

## Enunciado

El **cierre de ejercicio fiscal** es un **corte de imputación y de reporte**. **No ejecuta ni habilita ninguna transición de la Orden de Misión. Ningún expediente cambia de estado por efecto de una fecha.**

La **Orden de Misión que cruza el corte no se divide**. Cada hecho económico se imputa al **ejercicio de su fecha del hecho** y se valora con la tabla vigente a esa fecha ([`RN-40`](RN-40-calculo-a-la-fecha-del-hecho.md)); la liquidación presenta el **desglose por ejercicio**.

Todo **compromiso no ejecutado** al cierre **se revierte con asiento**, y **todo folio reservado y no consumido se anula con acta**. **Ni el compromiso ni el folio se arrastran al ejercicio siguiente.**

## Justificación

Ninguna de las 54 reglas originales menciona el ejercicio fiscal. Y el problema del usuario en diciembre es real y urgente, de modo que **sin esta regla escrita la primera implementación va a poner un cierre masivo por fecha**, porque es lo que resuelve ese problema.

Un cierre masivo por fecha es exactamente lo que no puede ocurrir: cerraría en bloque misiones con criterios de hallazgo sin evaluar, con un motivo compartido por decenas de expedientes, y destruiría la evaluación individual que [`RN-08`](RN-08-cadena-de-trazabilidad-para-cierre.md) exige. Ante el Tribunal Superior de Cuentas, cincuenta expedientes cerrados el 31 de diciembre a la misma hora con el mismo motivo **son el hallazgo**, no su solución.

Separar el corte contable del ciclo del expediente permite que la contabilidad cierre a tiempo y que cada misión siga su vida hasta su cierre individual, evaluado.

## Condiciones de aplicación

Aplica a todo cierre de ejercicio y a la ventana de apertura del siguiente.

**No aplica** al cierre de trimestre a efectos de cuota ([`RN-54`](RN-54-cuota-trimestral-de-compromiso.md)), que es un corte de control distinto y no reversión de compromisos.

## Comportamiento esperado

1. El cierre produce un **acta de cierre de ejercicio**: fecha de corte legal y operativa aplicadas, parámetros vigentes usados, quién lo ejecutó y cuándo.
2. Se genera el **inventario de expedientes no terminales al corte**, por estado, con causa tipificada, responsable nominado y antigüedad, y su contraparte —el **saldo de apertura** del ejercicio siguiente— que **debe coincidir renglón por renglón** ([`RN-97`](RN-97-saldo-de-apertura-de-control-interno.md)).
3. Cada misión cerrada dentro de la ventana de cierre conserva su **evaluación individual** de criterios de hallazgo con los datos concretos y su justificación propia. **Nunca un motivo compartido por varios expedientes.**
4. Las misiones que cruzaron el corte se listan con su **desglose de imputación por ejercicio** y las tablas paramétricas usadas para cada hecho, para que el cálculo sea reproducible.
5. Se produce el **acta de anulación de folios** no consumidos, por rango y delegación, y el **reporte de reversión de compromisos** con su archivo de conciliación para ARGOS y SIAFI ([`RN-81`](RN-81-sigti-expone-hechos-a-argos.md)).
6. El **registro de cambios de parámetros en la ventana de cierre** —umbrales, plazos, tolerancias, con autor, fecha, valor anterior y nuevo— se produce como reporte propio. **Es la evidencia de que nadie aflojó un umbral en diciembre para cerrar limpio**, o de que alguien lo hizo y quedó a la vista.

## Casos límite

- **`[C]` La cuota del primer trimestre no está aprobada el 2 de enero**, y el vehículo de una delegación remota necesita cargar combustible ese día. [`RN-54`](RN-54-cuota-trimestral-de-compromiso.md) deja el comportamiento sin cuota vigente en advertencia → bloqueo, configurable. La respuesta honesta es que la institución **sigue operando**: si el sistema bloquea, el sistema se sortea; si no advierte, el gasto queda sin control. **Recomendación del análisis, no decisión:** advertencia registrada con responsable nominado durante una **ventana configurable de apertura de ejercicio**, y bloqueo al vencerla.
- **`[C]` Misión que sale el 28 de septiembre y retorna el 3 de octubre** — o que cruza el 31 de diciembre. El compromiso se imputa al **período del acto que lo generó**; los hechos económicos, a su propia fecha. Confirmar el criterio con la Gerencia Administrativa y contra SIAFI: es el tipo de detalle que cada institución resuelve distinto.
- **Fondo con saldo al cierre.** Se arquea y se devuelve según el procedimiento de la institución; las obligaciones de reintegro abiertas **no se extinguen con el ejercicio** ([`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)).
- **Presión por cerrar todo antes del 31.** El sistema no la resuelve; la hace visible. El indicador de misiones cerradas en la ventana de cierre, contra el promedio del año, es el dato que expone el cierre apurado.
- **Hallazgo descubierto después del cierre del ejercicio.** Expediente de hallazgo posterior ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)) con asiento imputado al ejercicio corriente y referencia al anterior.

## Trazabilidad

- Norma: [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) — la exigencia de manejar cierre y apertura de ejercicio se cita como `[I]`, implicación del propio equipo
- Reglas relacionadas: [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-54](RN-54-cuota-trimestral-de-compromiso.md), [RN-81](RN-81-sigti-expone-hechos-a-argos.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-94](RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md), [RN-97](RN-97-saldo-de-apertura-de-control-interno.md)
- Casos especiales: [CE-27](../../02-requisitos/casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — candidatas `RN-C27a`, `RN-C27b`, `RN-C27c`
- Insumos pendientes: criterio de imputación entre ejercicios con la Gerencia Administrativa y SIAFI
