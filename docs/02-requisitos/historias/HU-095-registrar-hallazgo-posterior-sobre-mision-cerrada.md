# HU-095 — Registrar un hallazgo posterior sobre una misión cerrada, sin reabrirla

| Campo | Valor |
|---|---|
| **Módulo** | M-14 Reportes, Indicadores y Auditoría · M-13 Liquidación y Cierre |
| **Actor** | ACT-08 Gerencia Administrativa · ACT-12 Auditor Interno |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Gerencia Administrativa
**quiero** abrir un expediente de hallazgo posterior vinculado a una misión cerrada, con sus asientos reversos si tiene efecto económico, sin que la misión cambie de estado ni de datos
**para** que los reportes históricos ya entregados sigan siendo reproducibles, que es la única forma de que sirvan para rendir cuentas

## Contexto

El estado de cuenta del tag de peaje llega el mes siguiente. La facturación del proveedor de combustible no cuadra tres meses después. El reporte de multas aparece medio año más tarde. Todo eso es normal y ninguno de esos hechos puede reescribir un expediente cerrado.

*La razón de no reabrir es dura y deliberada: si un estado terminal puede cambiar meses después, entonces ningún reporte histórico es reproducible.*

**El expediente cerrado muestra el reverso, no lo esconde.** Todo reporte sobre esa misión presenta el valor original, el reverso y el valor resultante, con su cadena. Nunca solo el resultado.

## Reglas que la gobiernan

- [RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md) — El hallazgo posterior es expediente con ciclo propio y **no altera el estado ni los datos del objeto vinculado**
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Anexar evidencia a un expediente cerrado está permitido; modificarlo no
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Asiento reverso con referencia explícita al asiento revertido, valor anterior y nuevo, autor, autorizador y motivo
- [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) — La corrección retroactiva deja asiento de diferencia, nunca sobrescribe
- [RN-95](../../01-negocio/reglas/RN-95-conciliacion-contra-fuentes-externas.md) — Cada diferencia contra una fuente externa abre hallazgo posterior
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Todo reporte declara su fecha de corte y es reproducible a esa fecha
- [RN-96](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) · [RN-97](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md) — El cierre de ejercicio no cambia estados; lo no terminal es saldo de apertura

## Casos especiales que la afectan

- [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — Eje de la historia
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — Corte de ejercicio con expedientes no terminales
- [CE-24](../casos-especiales/CE-24-cobro-en-categoria-de-peaje-equivocada.md) — El estado de cuenta del tag revela discrepancias después del cierre
- [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) — La obligación de reintegro se salda por asiento sobre el expediente cerrado

## Criterios de aceptación

```gherkin
# language: es
Característica: Hallazgo posterior sobre expediente cerrado

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0468" en estado "CERRADA" desde el "2026-10-02"
    Y un costo de peajes congelado de "L 132.00"

  Escenario: Se rechaza reabrir la misión cerrada
    Cuando la Gerencia Administrativa intenta reabrir "OM-2026-0468"
    Entonces el sistema rechaza la acción
    Y muestra "OM-2026-0468 está en estado terminal desde el 02/10/2026. Desde un terminal no sale ninguna transición. Abra un expediente de hallazgo posterior."

  Escenario: Se abre el hallazgo posterior vinculado, sin cambiar el estado de la misión
    Cuando la Gerencia Administrativa abre un hallazgo posterior sobre "OM-2026-0468" por diferencia contra el estado de cuenta del tag
    Entonces se crea un expediente de hallazgo posterior con ciclo propio
    Y "OM-2026-0468" sigue en estado "CERRADA"
    Y ningún dato de "OM-2026-0468" cambia

  Escenario: Se puede anexar evidencia a un expediente cerrado
    Cuando el Auditor Interno anexa el estado de cuenta del tag de octubre a "OM-2026-0468"
    Entonces el sistema acepta el anexo
    Y el anexo queda vinculado con su fecha de incorporación
    Y ningún dato existente del expediente se modifica

  Escenario: Se rechaza un asiento reverso sin referencia explícita al asiento revertido
    Cuando la Gerencia Administrativa registra un reverso "de la misión OM-2026-0468"
    Entonces el sistema rechaza el asiento
    Y muestra "Indique el asiento concreto que se revierte. No existe el reverso genérico de una misión."

  Escenario: Se bloquea el reverso autorizado por quien produjo el asiento revertido
    Dado que "Marvin Aguilar" produjo el asiento de peajes de "L 132.00"
    Cuando "Marvin Aguilar" intenta autorizar su reverso
    Entonces el sistema rechaza la autorización
    Y muestra "Marvin Aguilar produjo el asiento que se pretende revertir. Quien autoriza el reverso no puede ser quien lo produjo."

  Escenario: El asiento reverso se registra completo
    Cuando la Gerencia Administrativa registra el reverso del costo de peajes con valor anterior "L 132.00", valor nuevo "L 200.00", motivo tipificado y fundamento adjunto
    Entonces el asiento queda con referencia al asiento revertido, valor anterior, valor nuevo, autor, autorizador y motivo
    Y el efecto económico afecta los acumulados del período en que se registra, no los del período original

  Escenario: El expediente cerrado muestra el reverso, no lo esconde
    Cuando se consulta el expediente de "OM-2026-0468" después del reverso
    Entonces el sistema presenta el valor original "L 132.00", el reverso y el valor resultante "L 200.00", con su cadena
    Y no presenta solo el valor resultante
    Y la misión muestra visiblemente que tiene hallazgos posteriores vinculados

  Escenario: Los reportes históricos ya entregados siguen siendo reproducibles
    Dado un reporte emitido con fecha de corte "2026-10-05"
    Cuando se vuelve a generar el mismo reporte a la fecha de corte "2026-10-05" después del reverso
    Entonces el resultado es idéntico al original
    Y un reporte a fecha de corte "2026-11-30" refleja el reverso

  Escenario: El cierre de ejercicio no cambia el estado de ningún expediente
    Dado 14 misiones en estado "LIQUIDADA" al "2026-12-31"
    Cuando se ejecuta el corte de ejercicio
    Entonces ninguna de las 14 misiones cambia de estado
    Y las 14 constituyen el saldo de apertura de control interno del ejercicio siguiente
    Y su antigüedad se cuenta desde el hecho original, no desde el corte

  Escenario: Diferencia detectada en la conciliación mensual contra fuente externa
    Dado un estado de cuenta del tag con "L 200.00" contra "L 132.00" registrados en "OM-2026-0468"
    Cuando se ejecuta la conciliación mensual contra la fuente externa
    Entonces el sistema abre un hallazgo posterior por la diferencia de "L 68.00"
    Y muestra "Diferencia de L 68.00 entre el estado de cuenta del tag de octubre 2026 y lo registrado en OM-2026-0468."
```

## Fuera de alcance

- El cierre de la misión — es [HU-093](HU-093-cerrar-la-mision-con-la-cadena-completa.md) y [HU-094](HU-094-cerrar-con-hallazgo-tipificado.md)
- El ciclo interno del expediente de hallazgo: pertenece a M-12 e M-14
- La deducción de responsabilidad administrativa: fuera de SIGTI
- La reprogramación presupuestaria de los efectos económicos del reverso: la resuelve ARGOS

## Notas y pendientes

- `[C]` Contratos con proveedores de combustible y de peaje contra los cuales conciliar mensualmente — pendiente registrado en el índice de reglas
- `[C]` Si COVI-H emite estado de cuenta empresarial a nombre de la institución — insumo **#24**
- `[C]` Catálogo `tipo_de_hallazgo_posterior` — insumo **#1**, con Auditoría Interna
- `[C]` Fechas de corte legal y operativa del ejercicio fiscal, con vigencia — insumo **#1**
- `[C]` Cómo se imputa el compromiso de una misión que cruza el cierre de trimestre: al trimestre del acto que lo generó, no al del retorno. Confirmar con Gerencia Administrativa — es el tipo de detalle que cada institución resuelve distinto — insumo **#1**
