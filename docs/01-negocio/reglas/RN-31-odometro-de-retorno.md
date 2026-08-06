# RN-31 — El odómetro de retorno no puede ser menor al de salida, y todo retroceso o salto exige justificación con respaldo

| Campo | Valor |
|---|---|
| **Módulos** | M-08, M-09, M-03 |
| **Origen** | Norma [NRM-09](../normativa/NRM-09-realidad-operativa.md) — detección de lecturas inconsistentes; [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` la exigencia de registrar odómetro de salida y retorno y detectar inconsistencias |
| **Tipo** | Bloqueo duro con salida por justificación autorizada |
| **Configurable** | Sí — `salto_maximo_km_por_dia` y `salto_maximo_km_por_hora` por tipo de vehículo |

## Enunciado

El sistema **no debe** aceptar una lectura de odómetro de retorno **menor** que la de salida de la misma misión, ni menor que la última lectura registrada para ese vehículo, salvo que se registre una **justificación con motivo tipificado, respaldo documental y autorización** de ACT-04 Jefe de Transporte.

Tampoco **debe** aceptar sin justificación un **salto** de kilometraje superior al máximo configurado para el tipo de vehículo en el tiempo transcurrido.

La lectura rechazada **no se descarta**: se conserva como lectura observada, junto con la justificación y la lectura finalmente aceptada.

## Justificación

[NRM-09](../normativa/NRM-09-realidad-operativa.md) exige registrar odómetro de salida y de retorno obligatoriamente, calcular rendimiento km/galón y *"detectar lecturas inconsistentes: retroceso de odómetro, saltos imposibles, y rendimientos anómalos en ambas direcciones"*.

El odómetro es el denominador de toda la conciliación de combustible ([RN-30](RN-30-conciliacion-galonaje-kilometraje.md)) y el testigo de la ruta efectivamente recorrida. Es también el dato sobre el que recae el mayor incentivo de manipulación. Un retroceso silenciosamente aceptado invalida meses de conciliación hacia atrás.

## Condiciones de aplicación

Aplica a todas las lecturas: salida, cargas de combustible, paradas registradas, retorno, ingreso y salida de taller, y constatación física.

Aplica **por vehículo en línea de tiempo**, no solo dentro de la misión: una lectura de retorno mayor que la de salida pero menor que una lectura posterior ya registrada por otra misión es igualmente inconsistente.

## Comportamiento esperado

1. Al capturar, el sistema muestra la **última lectura conocida** con su fecha y origen, para que el capturador vea contra qué se compara.
2. La inconsistencia se presenta con el cálculo explícito: *"Última lectura: 148,320 km el <fecha> (carga de combustible). Lectura ingresada: 147,900 km. Retroceso de 420 km."*
3. Los motivos tipificados incluyen, como mínimo: **reemplazo del odómetro o del tablero**, **falla del odómetro**, **error de captura previo**, **remolque del vehículo** (recorrido sin marcar), y **corrección de unidad** (millas/kilómetros). Cada motivo define el respaldo exigido.
4. Un reemplazo de odómetro registra la **lectura del instrumento anterior y la del nuevo**, y el kilometraje acumulado del vehículo se lleva como valor derivado, independiente de la lectura del instrumento.
5. Toda justificación queda en el expediente del vehículo y aparece en el reporte de auditoría de kilometraje.

## Casos límite

- **Reemplazo del tablero.** El caso legítimo por excelencia. El kilometraje acumulado del vehículo **no puede** depender de la lectura del instrumento: debe existir un acumulado propio que sobreviva al cambio, con el desfase registrado. Si esto no se modela desde el inicio, cada tablero cambiado corrompe el histórico.
- **Vehículo trasladado en grúa o plataforma.** Recorre distancia sin sumar odómetro. La bitácora debe permitir registrar el traslado como evento sin recorrido propio, para que la conciliación de peajes y ruta no lo interprete como incoherencia ([RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)).
- **Odómetro en millas.** Existen unidades importadas con tablero en millas. La ficha del vehículo debe declarar la unidad, y toda lectura se almacena normalizada con la unidad original conservada. Asumir kilómetros produce un error del 60% que nadie detecta hasta que la conciliación es absurda.
- **Odómetro que da la vuelta** al llegar a su máximo de dígitos. Ocurre en vehículos antiguos. Se trata como reemplazo lógico, con motivo tipificado propio.
- **Lectura de retorno igual a la de salida.** Técnicamente no viola la regla, pero significa que el vehículo no se movió mientras hubo consumo de combustible y tiempo de misión. **Debe** producir advertencia: es tan sospechoso como un retroceso.
- **Corrección tardía por el motorista.** No la aplica él ([RN-05](RN-05-registro-cerrado-no-se-edita.md)): registra solicitud de corrección con fotografía del tablero y ACT-04 resuelve con asiento.
- **Dos misiones simultáneas del mismo vehículo** por error de asignación. Producirá lecturas cruzadas incoherentes. La causa raíz se previene con [RN-13](RN-13-sin-doble-asignacion.md); la detección aquí es la red de seguridad.
- **Salto legítimo por misión de larga distancia.** Los máximos son por tipo de vehículo y por tiempo transcurrido, no absolutos. `[C]` los valores con el Jefe de Transporte; un motocicleta y un cabezal no tienen el mismo techo diario.

## Trazabilidad

- Normas: [NRM-09](../normativa/NRM-09-realidad-operativa.md), [NRM-01](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-05](RN-05-registro-cerrado-no-se-edita.md), [RN-13](RN-13-sin-doble-asignacion.md), [RN-22](RN-22-custodia-del-vehiculo.md)
- Actores: ACT-04, ACT-05, ACT-06, ACT-11, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
