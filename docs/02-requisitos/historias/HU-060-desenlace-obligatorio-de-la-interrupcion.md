# HU-060 — Registrar el desenlace obligatorio de toda interrupción, con responsable y plazo

| Campo | Valor |
|---|---|
| **Módulo** | M-08 Ejecución y Bitácora · M-12 Incidentes, Siniestros y Sanciones |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Jefe de Transporte
**quiero** que ninguna interrupción quede sin desenlace registrado, y que mientras no lo tenga aparezca con responsable nombrado y fecha límite
**para** no descubrir al cierre del ejercicio fiscal que hay once misiones interrumpidas que nadie resolvió y que ahora son mi hallazgo

## Contexto

**"Pendiente de resolución" no es un desenlace: es la ausencia de desenlace**, y es exactamente lo que la marca viva significa ([RN-70](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md)). La distinción no es semántica: decide si el tablero cuenta "misiones resueltas" incluyendo las pendientes, que es justo el número que no debe inflarse.

Los desenlaces son cuatro: **continuar**, **continuar con sustitución** de vehículo o de conductor, **retorno anticipado** y **retorno sin vehículo**.

**La marca queda en el expediente aunque el problema se haya resuelto solo.** La vía cerrada que se abrió a las dos horas deja marca y el tiempo perdido entra en el indicador: borrarla porque ya no duele es perder el único dato que explica por qué la misión llegó tarde ([RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md)).

Y la única facultad que no espera es la del conductor de **detener la misión por riesgo inmediato**, que se convalida después ([RN-73](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md)).

## Reglas que la gobiernan

- [RN-70](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) — **Regla rectora**: toda interrupción exige desenlace explícito, tipificado y registrado
- [RN-73](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) — La decisión tomada sin poder consultar se convalida después
- [RN-78](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) — La misión declara qué se hizo, qué no y por qué
- [RN-82](../../01-negocio/reglas/RN-82-indicadores-de-calidad-de-la-programacion.md) — El tiempo perdido se acumula por causa tipificada y se atribuye al responsable
- [RN-96](../../01-negocio/reglas/RN-96-cierre-de-ejercicio-como-corte-de-imputacion.md) · [RN-97](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md) — Ninguna interrupción sin desenlace sobrevive al cierre del período sin quedar en el saldo de apertura
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Quien conduce no decide el desenlace: lo decide quien tiene la facultad

## Casos especiales que la afectan

- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — Avería mecánica en ruta
- [CE-07](../casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) — Retorno anticipado: la misión se aborta
- [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md) — Cierre de ejercicio fiscal con hallazgo abierto
- [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) — La decisión tomada sin autorización previa que se convalida en plazo

## Criterios de aceptación

```gherkin
# language: es
Característica: Desenlace obligatorio de la interrupción en ruta

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con marca "interrumpida" desde el "2026-05-14" a las "11:40"
    Y una causa registrada de "avería mecánica"

  Escenario: Se rechaza liquidar la misión con la marca de interrupción sin desenlace
    Cuando el Jefe de Transporte intenta liquidar la Orden de Misión "OM-2026-0451"
    Entonces el sistema rechaza la liquidación
    Y muestra "La interrupción del 14/05/2026 a las 11:40 no tiene desenlace registrado. Registre qué se decidió: continuar, continuar con sustitución, retorno anticipado o retorno sin vehículo."

  Escenario: "Pendiente de resolución" no se admite como desenlace
    Cuando el Jefe de Transporte intenta registrar el desenlace "pendiente de resolución"
    Entonces el sistema no ofrece esa opción en el catálogo de desenlaces
    Y muestra "Si todavía no hay decisión, la marca sigue viva con responsable y fecha límite. No es un desenlace."

  Escenario: Se rechaza registrar el desenlace sin motivo
    Cuando el Jefe de Transporte registra el desenlace "retorno anticipado" sin motivo
    Entonces el sistema rechaza el registro
    Y muestra "El retorno anticipado exige motivo. Es lo que sustenta la liquidación por lo efectivamente ejecutado."

  Escenario: La marca viva lleva responsable nombrado y fecha límite
    Dado que han pasado "3" días desde la interrupción sin desenlace
    Cuando el Jefe de Transporte consulta el listado de su delegación
    Entonces la Orden de Misión "OM-2026-0451" aparece con "Interrumpida hace 3 días. Responsable: Jefe de Transporte. Fecha límite: 20/05/2026."
    Y el sistema escala la alerta al vencerse la fecha límite

  Escenario: La interrupción se resuelve sola y la marca no se borra
    Dada una interrupción por "vía cerrada" registrada a las "09:00"
    Cuando el Jefe de Transporte registra el desenlace "continuar" a las "11:00", con la constancia de quién lo autorizó
    Entonces el sistema levanta la marca de bloqueo pero conserva la interrupción en el expediente
    Y acumula "2 horas" de tiempo perdido en el indicador, atribuido a la causa "vía cerrada"

  Escenario: El conductor detiene la misión por riesgo inmediato sin poder consultar
    Dado que no hay señal, ni radio, ni teléfono para alcanzar al Jefe de Transporte
    Cuando "José Martínez" registra la decisión "detener la misión por condición de seguridad" con justificación obligatoria
    Entonces el sistema registra la decisión con la marca "sin autorización previa, pendiente de convalidación"
    Y la cronología queda declarada tal como ocurrió, sin reordenarse
    Y la convalidación queda pendiente para la liquidación, con responsable y plazo

  Escenario: No se puede cerrar el ejercicio con la marca viva
    Dada la Orden de Misión "OM-2026-0451" con marca "interrumpida" sin desenlace al "2026-12-31"
    Cuando la Gerencia Administrativa ejecuta el cierre del ejercicio
    Entonces el sistema lista la misión como no terminal al corte
    Y la incorpora al saldo de apertura del ejercicio siguiente, con antigüedad contada desde el "2026-05-14"
    Y no cambia su estado por efecto de la fecha de cierre
```

## Fuera de alcance

- El registro del hecho que interrumpió la misión — es [HU-058](HU-058-registrar-interrupcion-en-ruta.md)
- La sustitución del vehículo con la misión `EN_RUTA`: **no existe hoy transición que la respalde** — hallazgo abierto reportado hacia la máquina de estados desde `CU-09`
- El relevo de motorista, que sí tiene transición — es [HU-061](HU-061-relevo-de-motorista-en-ruta.md)
- Los subtipos de retorno — son [HU-062](HU-062-registrar-retorno-y-cerrar-bitacora.md) y [HU-065](HU-065-retorno-sin-vehiculo-y-permanencia-del-bien.md)

## Notas y pendientes

- `[C]` Quién convalida un acto de emergencia y en qué plazo; quién puede ordenar el retorno anticipado — insumos #32 y #50
- `[C]` Si un incidente abierto impide cerrar la misión o solo la marca — insumo #1
- **Hallazgo abierto:** `T-17` cubre prórroga, destino adicional y relevo de motorista, **no cambio de vehículo**. Mientras no exista la transición, el desenlace "continuar con sustitución de vehículo" se registra como hechos de bitácora bajo la misma Orden, con constancia de que la transición no existe. No se fuerza `T-17` a significar algo que no dice
