# RN-30 — El rendimiento galonaje–kilometraje se concilia contra el esperado del vehículo, con desviación detectada en ambas direcciones

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-13, M-14 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — patrón de hallazgo del TSC; [NRM-09](../normativa/NRM-09-realidad-operativa.md); `PROP-01` |
| **Verificación** | `[V]` que el auditor busca correlación consumo–kilometraje–misión, no comprobantes |
| **Tipo** | Cálculo + advertencia con hallazgo |
| **Configurable** | Sí — `rendimiento_esperado` por vehículo con vigencia, y `umbral_desviacion_rendimiento` superior e inferior |

## Enunciado

Al liquidar una misión, y periódicamente por vehículo, el sistema **debe** calcular:

```
rendimiento observado = kilómetros recorridos / galones consumidos
```

y compararlo contra el **rendimiento esperado** del vehículo vigente a la fecha del hecho.

La desviación **debe** detectarse en **ambas direcciones**:

- **Rendimiento por debajo del esperado** — más galones de los que el recorrido justifica: posible consumo no imputable a la misión.
- **Rendimiento por encima del esperado** — menos galones de los que el recorrido exige: **posible despacho de combustible no registrado**, kilometraje inflado, o consumo cargado a otra fuente.

Ambas desviaciones, superado el umbral, **deben** producir hallazgo tipificado en la liquidación.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) es inequívoca sobre lo que busca el auditor: *"el auditor no busca comprobantes, busca correlación entre consumo, kilometraje y misión autorizada. Un sistema que solo archiva facturas no responde a lo que se le va a preguntar."* El patrón de hallazgo documentado es el **incremento de consumo sin relación con el uso habitual de la flota**.

La detección en ambas direcciones es lo que distingue este control de un control ingenuo. `PROP-01` y [NRM-09](../normativa/NRM-09-realidad-operativa.md) lo exigen explícitamente: *"rendimientos anómalos en ambas direcciones"*. **Un rendimiento imposiblemente bueno casi siempre significa un despacho que no se registró** — el vehículo cargó combustible que nadie anotó, y por eso los galones registrados no alcanzan a explicar los kilómetros.

## Condiciones de aplicación

Aplica a toda misión con odómetro de salida y retorno y con consumo registrado.

**No aplica** de forma concluyente cuando: el odómetro está averiado ([RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-90](RN-90-intervencion-del-instrumento-de-medicion.md)), el vehículo arrastra saldo de tanque entre misiones, o la misión terminó con el tanque en nivel muy distinto al inicial. En esos casos el cálculo se marca **no concluyente** y se conserva para el análisis agregado, que sí es válido.

**No entra en el denominador** el kilometraje recorrido **bajo tenencia ajena** —vehículo prestado o cedido— que se asienta con las dos lecturas del acta y se excluye del cálculo, porque no hubo consumo nuestro contra esos kilómetros ([RN-63](RN-63-prestamo-de-vehiculo-como-expediente-del-bien.md), [RN-72](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md)).

**Sí entra en el numerador todo abastecimiento**, cualquiera sea su fuente de financiamiento — fondo de la misión, tanque institucional, otra dependencia, donación o peculio del servidor ([RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)). Sin esa cobertura, esta regla señala un síntoma cuya causa el sistema no puede registrar.

## Comportamiento esperado

1. El `rendimiento_esperado` es un parámetro **por vehículo con vigencia**, ajustable por tipo de terreno o de ruta. `[C]` la institución debe fijarlo; el sistema puede proponerlo a partir del histórico del propio vehículo, marcando la propuesta como tal.
2. Los umbrales superior e inferior son independientes y configurables. Un umbral único simétrico es un error de diseño: un exceso de consumo del 20% y un ahorro del 20% no significan lo mismo.
3. La desviación se tipifica con causa probable y se muestra junto con el desglose que la sustenta: kilómetros por tramo, cargas con su odómetro, tiempo de espera en sitio (M-19).
4. El sistema produce el **reporte de conciliación periódica** que exige [NRM-01](../normativa/NRM-01-control-interno-tsc.md): galones despachados por vale, galones facturados por el proveedor, kilómetros según bitácora y rendimiento esperado, con desviaciones marcadas en ambas direcciones.
5. Las desviaciones recurrentes de un mismo vehículo, motorista o dependencia generan alerta agregada, que es donde el patrón se ve — no en una misión aislada.

## Casos límite

- **Terreno de montaña, tráfico o aire acondicionado en operación prolongada.** Degradan el rendimiento legítimamente. El esperado debe admitir **variantes por tipo de ruta**; sin eso, el sistema producirá hallazgos falsos y en tres meses nadie los mirará. `[C]` levantar con el Jefe de Transporte.
- **Vehículo que sale con el tanque lleno y retorna con el tanque lleno**, habiendo cargado en ruta. El cálculo es correcto. El problema aparece cuando sale lleno y retorna vacío: los galones consumidos exceden a los cargados. Por eso el **nivel de combustible a la salida y al retorno es dato obligatorio de la bitácora** — obligación que vive en [RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md), no en [RN-22](RN-22-custodia-del-vehiculo.md), que trata de custodia.
- **Tiempo prolongado de motor encendido en espera** — vehículo esperando con aire acondicionado durante horas. Consume sin recorrer. El motor encendido durante la espera **se registra con un toque y entra como variable del cálculo** ([RN-76](RN-76-estado-en-ruta-declarado-por-el-motorista.md)): una desviación con espera prolongada registrada **no produce hallazgo por sí sola**. Sin esa medición, el hallazgo sería infundado.
- **Desviación amparada por una causa registrada y aceptada** — retorno anticipado, extensión autorizada, espera improductiva. No produce hallazgo por sí sola: la conciliación se evalúa contra el **alcance vigente a la fecha de cada hecho** ([RN-77](RN-77-versionado-del-alcance-autorizado.md), [RN-78](RN-78-grado-de-cumplimiento-del-objeto.md)).
- **Rendimiento excelente por descenso continuo** en una ruta de bajada. Legítimo. La tipificación de causa debe incluirlo, y el analista decide.
- **Vehículo nuevo sin histórico.** El esperado se toma de la ficha técnica del fabricante o del tipo de vehículo, marcado como valor provisional hasta acumular histórico propio.
- **Sustitución de vehículo a mitad de misión.** Cada vehículo se concilia por separado, con sus propios cortes de odómetro ([RN-14](RN-14-sustitucion-de-motorista.md)). Un cálculo agregado de la misión mezclaría dos rendimientos y no significaría nada.
- **Manipulación del odómetro.** El sistema detecta retrocesos y saltos ([RN-31](RN-31-odometro-de-retorno.md)), pero un odómetro alterado de forma consistente es indetectable por esta vía. Se mitiga con fotografía del tablero en salida y retorno, y con el cruce contra peajes y ruta ([RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)).

## Trazabilidad

- Normas: [NRM-01](../normativa/NRM-01-control-interno-tsc.md), [NRM-09](../normativa/NRM-09-realidad-operativa.md)
- Decisión: `PROP-01` en [insumos-pendientes](../../07-gestion/insumos-pendientes.md)
- Reglas relacionadas: [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-37](RN-37-coherencia-de-la-secuencia-de-casetas.md)
- Actores: ACT-04, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
