# HU-070 — Recibir un registro de campo que llega después del cierre en oficina

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada · M-14 Reportes, Indicadores y Auditoría · M-13 Liquidación y Cierre |
| **Actor** | ACT-04 Jefe de Transporte · ACT-12 Auditor Interno |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta el criterio de imputación entre ejercicios fiscales y la ventana de apertura cuando el registro tardío cruza el corte (`NRM-04`), y el umbral de materialidad a partir del cual el registro tardío obliga a asiento de diferencia en lugar de nota al expediente (insumo #1) |

## Historia

**Como** Jefe de Transporte
**quiero** que el registro que llega del campo después de que la oficina ya liquidó o cerró la misión no se descarte ni sobreescriba lo cerrado
**para** que el hecho quede en el expediente por la vía correcta, sin reabrir un expediente terminal y sin que un reporte ya emitido cambie de contenido a mis espaldas

## Contexto

**Es el caso más frecuente y el que más tienta a implementar un descarte automático.** El motorista estuvo seis días sin señal; para cuando su diario llegó, la oficina ya había liquidado con lo que tenía.

Descartarlo es perder un hecho. Aplicarlo sobre lo cerrado es alterar un expediente terminal. La salida correcta es la tercera: **entra a la cola con su fecha del hecho**, y se resuelve por asiento de diferencia o se cierra con hallazgo ([RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).

**Si la misión está `CERRADA`, no se reabre ni por auditoría**: se abre un expediente de hallazgo posterior con su ciclo propio, y la misión cerrada muestra desde entonces que tiene hallazgos vinculados ([RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)).

Y sin la **fecha de corte de conocimiento** en todo reporte, no reabrir el expediente no sirve de nada: el reporte cambiaría igual y nadie podría reproducir el que se entregó el mes pasado ([RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md)).

## Reglas que la gobiernan

- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — **Regla rectora**: el registro cerrado no se edita
- [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) — La corrección retroactiva se hace por asiento de diferencia
- [RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md) — El hallazgo posterior es expediente con ciclo propio y no altera el estado del objeto vinculado
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Todo reporte declara su fecha de corte de conocimiento y es reproducible a esa fecha
- [RN-89](../../01-negocio/reglas/RN-89-kilometraje-acumulado-invariante-del-expediente.md) — Insertar un registro anterior reabre la validación de todos los posteriores
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Nada se descarta ni se sobrescribe

## Casos especiales que la afectan

- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — Hallazgo posterior sobre misión cerrada
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — La bitácora que llega después del cierre
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — El odómetro retroactivo que invalida la serie posterior
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — El registro que llega después del cierre del ejercicio

## Criterios de aceptación

```gherkin
# language: es
Característica: Registro de campo recibido después del cierre en oficina

  Antecedentes:
    Dado un dispositivo portador "DEL-CHO-03" que estuvo 6 días sin conectividad
    Y una Orden de Misión "OM-2026-0451" ya liquidada el "2026-05-20" con los datos disponibles a esa fecha

  Escenario: No se descarta el registro que llega tarde
    Cuando el dispositivo sincroniza el "2026-05-22" un abastecimiento con fecha del hecho "2026-05-15"
    Entonces el servidor no descarta el registro
    Y no lo aplica sobre la liquidación cerrada
    Y lo envía a la cola de resolución humana con su fecha del hecho "2026-05-15"

  Escenario: Se rechaza editar la liquidación cerrada para incorporarlo
    Cuando el Jefe de Transporte intenta modificar la liquidación de "OM-2026-0451" para incluir el abastecimiento
    Entonces el sistema rechaza la modificación
    Y muestra "La liquidación de OM-2026-0451 está cerrada. Registre un asiento de diferencia con su motivo y su respaldo."

  Escenario: El hecho se incorpora por asiento de diferencia
    Cuando el Jefe de Transporte registra el asiento de diferencia por el abastecimiento de "L 1,650.00" con fecha del hecho "2026-05-15"
    Entonces el sistema registra el asiento con valor anterior, valor nuevo, motivo, autor y fundamento
    Y la liquidación original permanece íntegra y consultable
    Y el expediente muestra ambos: lo liquidado y la diferencia posterior

  Escenario: Se rechaza reabrir una misión CERRADA
    Dada la Orden de Misión "OM-2026-0430" en estado "CERRADA"
    Cuando llega un registro de campo con fecha del hecho anterior al cierre
    Entonces el sistema no reabre la misión
    Y abre un expediente de hallazgo posterior con su ciclo propio
    Y la misión cerrada muestra desde entonces que tiene hallazgos vinculados
    Y muestra "OM-2026-0430 está cerrada y no se reabre. Se abrió el expediente de hallazgo posterior HP-2026-0012."

  Escenario: La inserción retroactiva reabre la validación de la serie posterior
    Dada una misión del "2026-05-03" al "2026-05-07" que se digita después que la del "2026-05-12"
    Cuando el registro retroactivo se aplica
    Entonces el sistema evalúa la continuidad del odómetro sobre la serie ordenada por fecha del hecho
    Y reabre la validación de todos los registros posteriores del mismo vehículo
    Y las incoherencias que aparezcan entran a la cola, sin corregirse solas

  Escenario: Un reporte ya emitido no cambia de contenido
    Dado un reporte de consumo del período "mayo 2026" emitido el "2026-06-01"
    Cuando llega el "2026-06-10" un registro con fecha del hecho "2026-05-15"
    Entonces el reporte emitido el "2026-06-01" sigue siendo reproducible con su fecha de corte de conocimiento
    Y un reporte nuevo del mismo período declara su propia fecha de corte "2026-06-10"
    Y las dos versiones del reporte son distintas, explicables y ambas correctas a su fecha

  Escenario: Un bloqueo duro falla al revalidarse en el servidor
    Dado un despacho ejecutado en modo desconectado con la documentación del vehículo vencida
    Cuando el registro sincroniza y el servidor revalida el bloqueo duro
    Entonces el sistema no revierte el hecho: el vehículo ya salió y la misión se ejecutó
    Y abre el hallazgo "H-07" con expediente propio
    Y notifica al Jefe de Transporte y al Auditor Interno
    Y el hallazgo no imputa responsabilidad a nadie: es marca de seguimiento
```

## Fuera de alcance

- El ciclo del expediente de hallazgo posterior — es de M-14 y M-12
- La reproducibilidad histórica de reportes en sí — es [RNF-06](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md)
- El cierre de ejercicio fiscal y el saldo de apertura — es de M-13 y [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md)

## Notas y pendientes

- `[C]` Criterio de imputación entre ejercicios fiscales y ventana de apertura, cuando el registro tardío cruza el corte — [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md)
- `[C]` Plazo de conservación de registros financieros y de bienes, que condiciona hasta cuándo un hallazgo posterior es exigible — insumo #71
- `[C]` Umbral de materialidad a partir del cual un registro tardío obliga a asiento de diferencia en lugar de nota al expediente — insumo #1
