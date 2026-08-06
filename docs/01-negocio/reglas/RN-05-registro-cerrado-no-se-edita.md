# RN-05 — Un registro cerrado no se edita, y ningún rol operativo modifica autorizaciones ni bitácoras cerradas

| Campo | Valor |
|---|---|
| **Módulos** | M-08, M-13, M-14, M-01 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[V]` |
| **Tipo** | Bloqueo duro |
| **Configurable** | No |

## Enunciado

Una vez que una bitácora se cierra, una liquidación se aprueba o una Orden de Misión alcanza el estado `LIQUIDADA` o `CERRADA`, **ningún rol** puede modificar sus campos.

Adicionalmente, ACT-06 Motorista y cualquier otro rol operativo **no deben** poder modificar en ningún momento: actos de autorización, asignaciones de combustible emitidas por ACT-07, permisos de circulación, ni bitácoras ya cerradas — incluidas las propias.

Todo cambio posterior al cierre se hace por [RN-04](RN-04-anulacion-como-asiento-reverso.md) (asiento reverso) o por [RN-42](RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) (asiento de diferencia), y solo por el rol facultado.

## Justificación

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) es explícita: *"El sistema debe impedir que un rol operativo (motorista) edite bitácoras cerradas o modifique autorizaciones."*

El motorista es quien tiene el incentivo directo sobre el dato más sensible del sistema: el odómetro. Si puede reabrir su propia bitácora, la conciliación galonaje–kilometraje de [RN-30](RN-30-conciliacion-galonaje-kilometraje.md) deja de probar nada.

## Condiciones de aplicación

Aplica a partir del evento de cierre de cada artefacto:

| Artefacto | Evento que lo cierra | Quién puede reabrir |
|---|---|---|
| Bitácora de misión | Registro de retorno confirmado por ACT-05 | ACT-04, con motivo y asiento |
| Asignación de combustible | Constancia de recepción firmada | ACT-07 solo por anulación |
| Liquidación | Aprobación de ACT-08 | Nadie: solo asiento de diferencia |
| Orden de Misión | Estado `CERRADA` | Nadie |

`[C]` Confirmar con la institución si ACT-04 Jefe de Transporte es efectivamente quien puede reabrir una bitácora, o si esa facultad es de ACT-08 Gerencia Administrativa.

## Comportamiento esperado

1. Los campos de un registro cerrado se presentan en **solo lectura**, sin controles de edición deshabilitados: la acción no existe en la interfaz ni en la operación de fondo.
2. Un intento de escritura sobre un registro cerrado — incluido el que llegue por sincronización desde un dispositivo de campo — se **rechaza y se envía a la cola de conflictos** de [RN-45](RN-45-cero-sobrescritura-silenciosa.md), nunca se aplica.
3. La **reapertura**, donde exista, es un acto autorizado y registrado: quién reabrió, motivo tipificado, y qué campos se modificaron después. La orden queda marcada como *reabierta* de forma visible y permanente.
4. Una liquidación aprobada **nunca se reabre**. Las diferencias posteriores se resuelven con asiento de diferencia.
5. Ningún ajuste técnico de ACT-01 Administrador del Sistema puede escribir sobre registros cerrados. Si un defecto obliga a corregir datos, se corrige por los mismos asientos que usa el negocio, y queda registrado.

## Casos límite

- **El motorista se equivocó al digitar el odómetro de retorno y ya cerró.** No lo corrige él. Registra una **solicitud de corrección** con la lectura correcta y evidencia — fotografía del tablero —, y ACT-04 resuelve con asiento. Ver [RN-31](RN-31-odometro-de-retorno.md).
- **Registro de campo que llega tarde**, después de que la oficina cerró la bitácora por retorno confirmado. El registro no se descarta ni se aplica: entra a la cola de conflictos con su fecha del hecho, y quien resuelve decide si amerita reapertura. Perder un dato de campo es peor que reabrir.
- **Cierre automático por vencimiento de plazo.** Si la institución configura cierre automático de bitácoras a los N días, el registro tardío queda permanentemente fuera. `[C]` confirmar si se desea cierre automático; recomendación: no cerrar automáticamente, solo alertar.
- **Auditoría solicita anexar evidencia a un expediente cerrado.** Anexar evidencia **no es editar**: se permite adjuntar documentos a un expediente cerrado, siempre como anexo fechado y firmado por quien lo agrega, sin alterar ningún campo.
- **Corrección exigida por el propio TSC tras un hallazgo.** Se ejecuta como asiento de diferencia con referencia al número de informe de auditoría, no como edición. El expediente debe mostrar que el cambio nació de un hallazgo externo.
- **Vehículo que retorna, sale de nuevo el mismo día y el despachador cerró la bitácora anterior.** Son dos misiones distintas y dos bitácoras distintas. Si operativamente se manejó como una sola, se resuelve con reapertura autorizada, no reutilizando la bitácora cerrada.

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md)
- Reglas relacionadas: [RN-03](RN-03-registro-inmutable-de-autorizacion.md), [RN-04](RN-04-anulacion-como-asiento-reverso.md), [RN-31](RN-31-odometro-de-retorno.md), [RN-45](RN-45-cero-sobrescritura-silenciosa.md)
- Actores: ACT-01, ACT-04, ACT-06, ACT-07, ACT-08, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
