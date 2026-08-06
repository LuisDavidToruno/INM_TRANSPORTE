# RN-46 — Fecha del hecho y fecha de captura son campos distintos, ambos obligatorios, y los cálculos usan la del hecho

| Campo | Valor |
|---|---|
| **Módulos** | M-08, M-09, M-16, M-14 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-10 Registro Oportuno; [NRM-09](../normativa/NRM-09-realidad-operativa.md) |
| **Verificación** | `[V]` |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — `desfase_maximo_sin_justificacion` (horas) entre hecho y captura |

## Enunciado

Todo registro de hecho operativo — salida, parada, arribo, consumo de combustible, paso por caseta, incidente, retorno — **debe** almacenar dos marcas temporales distintas y obligatorias:

1. **Fecha y hora del hecho**: cuándo ocurrió realmente
2. **Fecha y hora de captura**: cuándo se registró en el sistema

Todos los cálculos, validaciones, ordenamientos y resoluciones de parámetro **deben** usar la **fecha del hecho** ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)).

Cuando el desfase entre ambas supere el umbral configurado, el registro **debe** marcarse como **registro diferido** y exigir motivo.

## Justificación

TSC-NOGECI V-10 exige **Registro Oportuno**: la bitácora y el consumo se registran *en el momento del hecho*, no reconstruidos después. [NRM-01](../normativa/NRM-01-control-interno-tsc.md) va más lejos y lo convierte en requisito de sistema: *"registrar el momento real del hecho distinguiéndolo del momento de captura, para satisfacer TSC-NOGECI V-10 cuando el registro se hizo en papel y se digitó después"*.

Con la conectividad que documenta [NRM-09](../normativa/NRM-09-realidad-operativa.md), el desfase es inevitable. Lo que no es aceptable es que sea **invisible**: si el sistema guarda una sola fecha, no se puede distinguir un registro hecho en el momento de uno reconstruido tres semanas después, y ambos pesan igual ante el auditor — cuando no deberían.

## Condiciones de aplicación

Aplica a todo hecho operativo y a las autorizaciones emitidas en campo.

**No aplica** a los actos que ocurren dentro del sistema y son simultáneos por definición — una aprobación en línea, la carga de un parámetro. Ahí ambas fechas coinciden y se registran igual.

## Comportamiento esperado

1. La fecha del hecho se **propone** con la del dispositivo pero es **editable** por el capturador, porque el papel puede llenarse antes y digitarse después.
2. La fecha del hecho **no puede** ser posterior a la de captura. Un hecho futuro no es un hecho.
3. La fecha de captura **no es editable** por ningún rol: la asigna el sistema.
4. Los listados y bitácoras se ordenan por fecha del hecho; el orden de captura se conserva y es consultable, porque un orden de captura anómalo es señal de reconstrucción.
5. Los registros diferidos se identifican visualmente en el expediente y se contabilizan en el **indicador de oportunidad de registro** por dependencia y motorista — que es el indicador que responde a TSC-NOGECI V-10.

## Casos límite

- **Reloj del dispositivo desajustado.** La fecha del hecho declarada puede ser incorrecta sin mala fe. Al sincronizar se registra la desviación observada entre reloj del dispositivo y del servidor, y los registros afectados se marcan como *hora de dispositivo no confiable* ([RN-03](RN-03-registro-inmutable-de-autorizacion.md)). No se corrigen automáticamente: corregir desplazaría hechos entre días y podría cambiar la calificación de día hábil.
- **Hecho de fecha incierta** — el motorista recuerda "fue el martes o el miércoles". Se admite registrar un **rango** con marca de imprecisión; los cálculos que dependan de esa fecha se marcan **no concluyentes** ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)). Elegir un extremo en silencio sería inventar.
- **Desfase enorme pero legítimo** — delegación que estuvo tres semanas sin red. Se marca como diferido con motivo *sin conectividad*, y no debería contar como incumplimiento de registro oportuno del motorista sino como condición de la delegación. El indicador debe distinguir ambos motivos, o se convierte en un castigo a quien opera donde no hay señal.
- **Fecha del hecho retroactiva usada para eludir un control** — registrar una salida como si hubiera ocurrido en día hábil. Se detecta cruzando fecha del hecho, fecha de captura y ubicación; y el registro diferido con motivo obligatorio deja al capturador respondiendo por la afirmación.
- **Cruce de medianoche.** Un hecho a las 23:55 capturado a las 00:10 pertenece al día anterior. Con una sola fecha, el registro migraría de día y podría cambiar la aplicación de [RN-23](RN-23-permiso-de-circulacion-en-dia-inhabil.md).
- **Corrección de una fecha del hecho mal capturada.** Es una corrección de campo cubierto: asiento reverso y asiento nuevo ([RN-04](RN-04-anulacion-como-asiento-reverso.md)), nunca edición.

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-10; [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Reglas relacionadas: [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-43](RN-43-captura-de-campo-sin-conectividad.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-47](RN-47-digitacion-diferida-desde-papel.md)
- Actores: ACT-06, ACT-10, ACT-04, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
