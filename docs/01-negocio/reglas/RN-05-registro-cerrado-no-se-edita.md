# RN-05 — Un registro cerrado no se edita, y ningún rol operativo modifica autorizaciones ni bitácoras cerradas

| Campo | Valor |
|---|---|
| **Módulos** | M-08, M-13, M-14, M-01 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — implicación de requerimiento. Máquina de estados [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) §2 `RETORNADA` y §3.4 — **artefacto autoridad en transiciones e invariantes** |
| **Verificación** | `[I]` — [NRM-01](../normativa/NRM-01-control-interno-tsc.md) recoge la exigencia como *implicación de requerimiento* escrita por el equipo, no como articulado citable. El nivel se corrigió de `[V]` a `[I]` por la regla de no escalar el nivel al bajar de abstracción |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Nota de corrección — hallazgo `HB1-04`

> **Qué estaba mal.** La tabla de condiciones de aplicación otorgaba a ACT-04 la facultad de **reabrir la bitácora cerrada** "con motivo y asiento", y dos casos límite la usaban como salida. [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) §3.4 lo prohíbe expresamente y con fundamento: *"`RETORNADA → EN_RUTA` — La ejecución no se reabre. **Reabrir permitiría agregar eventos con fecha del hecho anterior sin control**."* El estado `RETORNADA` lo repite: *"No se puede: volver a `EN_RUTA`. Modificar odómetros o eventos capturados — solo corregirlos por asiento."*
>
> **Qué manda.** La máquina de estados es la autoridad. **La reapertura de bitácora no existe en SIGTI.** La corrección posterior es siempre un **asiento**, nunca una reapertura.
>
> La contradicción no era cosmética: la reapertura devolvía a ACT-04 la capacidad de escribir sobre el kilometraje, que es el denominador de [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) y justamente el dato que esta regla dice proteger. También se retira el `[C]` que preguntaba *si ACT-04 o ACT-08 puede reabrir*: era la pregunta equivocada, porque nadie reabre.

## Enunciado

Una vez que una bitácora se cierra, una liquidación se aprueba o una Orden de Misión alcanza el estado `LIQUIDADA` o `CERRADA`, **ningún rol** puede modificar sus campos.

**No existe la reapertura.** Ni de la bitácora, ni de la liquidación, ni del expediente. Un artefacto cerrado se corrige **solo hacia adelante**, agregando registros nuevos que se refieren a él y que nunca alteran lo anterior.

Adicionalmente, ACT-06 Motorista y cualquier otro rol operativo **no deben** poder modificar en ningún momento: actos de autorización, asignaciones de combustible emitidas por ACT-07, permisos de circulación, ni bitácoras ya cerradas — incluidas las propias.

Todo cambio posterior al cierre se hace por [RN-04](RN-04-anulacion-como-asiento-reverso.md) (asiento reverso), por **asiento de corrección de dato**, o por [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) (asiento de diferencia), y solo por el rol facultado.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) es explícita: *"El sistema debe impedir que un rol operativo (motorista) edite bitácoras cerradas o modifique autorizaciones."*

El motorista es quien tiene el incentivo directo sobre el dato más sensible del sistema: el odómetro. Si puede reabrir su propia bitácora, la conciliación galonaje–kilometraje de [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) deja de probar nada.

## Condiciones de aplicación

Aplica a partir del evento de cierre de cada artefacto. **Ninguna fila admite reapertura:**

| Artefacto | Evento que lo cierra | Única corrección posterior | Quién la autoriza |
|---|---|---|---|
| Bitácora de misión | Registro del retorno (`T-18` o `T-16`), ejecutado por ACT-06 desde el dispositivo portador | **Asiento de corrección de dato** sobre bitácora cerrada | ACT-04, con motivo tipificado y respaldo |
| Asignación de combustible | Constancia de recepción firmada | Anulación con acta, o liquidación de lo consumido | ACT-07 la anulación; ACT-04 la liquidación |
| Liquidación | Aprobación de ACT-08 | Devolución de la liquidación (`T-20`) si aún no se cerró; **asiento de diferencia** si ya se cerró | ACT-08 |
| Orden de Misión | Estado `CERRADA` o `CERRADA_CON_HALLAZGO` | **Asiento reverso** y expediente de hallazgo posterior | ACT-08 lo autoriza; ACT-12 puede requerirlo |

El cierre de la bitácora es **efecto** del registro del retorno, no un acto separado de la oficina. La recepción física del vehículo por ACT-05 produce el **acta de recepción**, que es un documento distinto y no cierra ni abre la bitácora.

## Comportamiento esperado

1. Los campos de un registro cerrado se presentan en **solo lectura**, sin controles de edición deshabilitados: la acción no existe en la interfaz ni en la operación de fondo.
2. Un intento de escritura sobre un registro cerrado — incluido el que llegue por sincronización desde un dispositivo de campo — se **rechaza y se envía a la cola de conflictos** de [RN-45](RN-45-cero-sobrescritura-silenciosa.md), nunca se aplica.
3. El **asiento de corrección** es un acto autorizado y registrado: quién lo hizo, motivo tipificado, respaldo adjunto, valor registrado y valor correcto — **ambos, siempre**. No reemplaza el original: el expediente muestra la lectura observada, el asiento y el valor resultante, con su cadena. El sistema **no ofrece en ninguna parte** una acción de reapertura, ni siquiera a ACT-08.
4. Una liquidación aprobada **nunca se reabre**. Las diferencias posteriores se resuelven con asiento de diferencia.
5. Ningún ajuste técnico de ACT-01 Administrador del Sistema puede escribir sobre registros cerrados. Si un defecto obliga a corregir datos, se corrige por los mismos asientos que usa el negocio, y queda registrado.

## Casos límite

- **El motorista se equivocó al digitar el odómetro de retorno y ya cerró.** No lo corrige él. Registra una **solicitud de corrección** con la lectura correcta y evidencia — fotografía del tablero —, y ACT-04 resuelve con asiento. Ver [RN-31](RN-31-odometro-de-retorno.md).
- **Registro de campo que llega tarde**, después del cierre de la bitácora. Caso concreto: misión de cuatro días, retorno registrado el viernes a las 18:00; el sábado sincroniza el teléfono y llega un consumo de 12 galones con `ocurrido_en` = miércoles 14:30 y fotografía del comprobante. El registro **no se descarta ni se aplica sobre la bitácora**: entra a la cola de conflictos de [RN-45](RN-45-cero-sobrescritura-silenciosa.md) con su fecha del hecho, y quien resuelve lo incorpora como **asiento de corrección sobre bitácora cerrada**, con el dato original de campo íntegro. Perder un dato de campo es peor que corregirlo por asiento; reabrir la bitácora es peor que las dos cosas.
- **Cierre automático por vencimiento de plazo.** No existe. La máquina de estados prohíbe el cierre automático por inactividad, y el registro del retorno lo ejecuta siempre una persona. Lo que sí hay es **alerta** por misión sin sincronizar desde N días, dirigida a ACT-04 y ACT-10: informativa, no dispara transiciones. `[C]` los umbrales de alerta.
- **Auditoría solicita anexar evidencia a un expediente cerrado.** Anexar evidencia **no es editar**: se permite adjuntar documentos a un expediente cerrado, siempre como anexo fechado y firmado por quien lo agrega, sin alterar ningún campo.
- **Corrección exigida por el propio TSC tras un hallazgo.** Se ejecuta como asiento de diferencia con referencia al número de informe de auditoría, no como edición. El expediente debe mostrar que el cambio nació de un hallazgo externo.
- **Vehículo que retorna, sale de nuevo el mismo día y ya se cerró la bitácora anterior.** Son dos misiones distintas y dos bitácoras distintas. No se reutiliza ni se reabre la cerrada: la segunda salida exige su propia Orden de Misión, su propio folio y su propia bitácora. Si operativamente se manejó como una sola, lo que corresponde es registrar la segunda misión con su fecha del hecho y dejar el asiento de corrección que explique el desfase — nunca estirar la bitácora anterior para que quepan los dos viajes.

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md) `[I]`
- Autoridad en transiciones: [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) §2 `RETORNADA`, §3.4 y §8.2
- Hallazgo que corrige esta regla: `HB1-04` de [H-B1-001](../../05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md)
- Reglas relacionadas: [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md), [RN-53](RN-53-cierre-del-manifiesto-al-despacho.md)
- Actores: ACT-01, ACT-04, ACT-06, ACT-07, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
