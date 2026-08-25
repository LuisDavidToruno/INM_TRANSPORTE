# HU-101 — Alertar los vencimientos documentales del vehículo al puesto, y bloquear o advertir según el parámetro

| Campo | Valor |
|---|---|
| **Módulo** | M-04 Documentación y Cumplimiento Vehicular |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — faltan los `umbrales_alerta_vencimiento` por tipo de documento (insumo #1), la decisión sobre si el mantenimiento preventivo vencido bloquea o advierte (insumo #59) y el catálogo de documentos que la institución exige en el expediente vehicular (insumo #1) |

## Nota de corrección — hallazgo `HB34-12`

> **Qué estaba mal — dos cosas, y esta historia es la dueña del modelo del dato.**
>
> 1. **El bloqueo por póliza vencida estaba modelado como interruptor único.** Los `Antecedentes` declaraban *«los parámetros `bloqueo_por_poliza_vencida` y `bloqueo_por_revision_vencida` en apagado»*, sin dimensión de régimen de tenencia. [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) es explícita: *«**Excepción admitida: granularidad por régimen de tenencia.** `bloqueo_por_poliza_vencida` admite valor distinto según el régimen […] el contrato de alquiler normalmente **obliga** a mantener la póliza vigente.»* [HU-023](HU-023-documentacion-y-estado-operativo-del-vehiculo.md) lo modelaba bien y esta historia, que es la que define el parámetro, lo definía incompleto.
>
>    **El caso:** vehículo `INS-P-030`, régimen **alquilado**, póliza vencida el 15/08/2026, institución con el parámetro global en *apagado*. Implementando `HU-023` se **rechaza** la asignación; implementando esta historia se **permite** el despacho con advertencia. Es la misma póliza y el mismo día.
>
> 2. **Con el bloqueo apagado, esta historia solo advertía.** `RN-16` exige que el sistema *«advierte, **exige acuse del despachador** y registra quién continuó»*, y su comportamiento n.º 4 pide un **reporte de exposición** que sin ese registro no tiene de dónde salir. `HU-023` sí registra *«advertencia superada por el Jefe de Transporte»*.
>
> **Qué se corrige.** El parámetro pasa a tener valor **por régimen de tenencia** —propio, comodato, alquilado— y la advertencia superada exige **acuse nominativo registrado** con el nombre de quien continuó y el momento. La granularidad la manda `RN-16`; `HU-023` la aplica al programar y esta historia define el dato y su ciclo de vida.

## Historia

**Como** Jefe de Transporte
**quiero** recibir alertas anticipadas de cada vencimiento documental dirigidas al **puesto** y no a la persona, y que el bloqueo aplique según lo que la norma exige y no según lo que se suponga
**para** convertir un bloqueo del despacho en una gestión hecha a tiempo, y para no bloquear por documentos que la ley vigente no exige

## Contexto

La rotación de personal es alta: una alerta dirigida a quien ya no ocupa el cargo no llega a nadie. Por eso las alertas van al **puesto**.

Y hay una distinción normativa que el sistema no puede confundir: la **matrícula** y el **título de tenencia** son exigibles; el **seguro** y la **revisión mecánica no son obligatorios por ley vigente** `[V]`. Son rastreables y alertables, pero el bloqueo por ellos es una **regla configurable, apagada por defecto**. Encenderla es decisión de la institución, no del sistema.

El bloqueo es la última línea, no la primera: las alertas debieron avisarlo antes.

## Reglas que la gobiernan

- [RN-17](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) — Alertas anticipadas por documento, dirigidas al puesto, con umbrales configurables
- [RN-16](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md) — Póliza y revisión rastreables y alertables; bloqueo configurable **apagado por defecto y con valor propio por régimen de tenencia**. Con el bloqueo apagado el sistema **advierte, exige acuse del despachador y registra quién continuó** (corregido por `HB34-12`)
- [RN-62](../../01-negocio/reglas/RN-62-titulo-de-tenencia-con-vigencia-y-rubros.md) — El título vencido bloquea la habilitación y la programación de misiones que lo excedan
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — La renovación cierra el rango anterior y abre uno nuevo; no edita el vencimiento
- [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) — El vencimiento lleva el vehículo a `NO_DISPONIBLE` con causa tipificada
- [RN-24](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) — La excepción de circulación es documento con vigencia como cualquier otro

## Casos especiales que la afectan

- [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) — Indisponibilidad sobrevenida con misiones ya programadas
- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — Alerta crónica por trámite detenido
- [CE-15](../casos-especiales/CE-15-vehiculo-en-comodato-o-alquilado.md) — Vencimiento del título de tenencia antes que las misiones programadas

## Criterios de aceptación

```gherkin
# language: es
Característica: Vencimientos documentales del vehículo

  Antecedentes:
    Dado un vehículo "TR-0092" en estado "DISPONIBLE", régimen de tenencia "propio"
    Y umbrales de alerta de "60", "30" y "15" días
    Y los parámetros "bloqueo_por_poliza_vencida" y "bloqueo_por_revision_vencida"
      con valor por régimen de tenencia: "apagado" para "propio", "apagado" para "comodato"
      y "encendido" para "alquilado"

  Escenario: La alerta se dirige al puesto, no a la persona
    Dado una matrícula que vence el "2026-11-30"
    Cuando el sistema evalúa los vencimientos el "2026-10-01"
    Entonces genera alerta al puesto responsable de documentación vehicular
    Y muestra "Matrícula de TR-0092 vence el 30/11/2026, en 60 días."
    Y la alerta la ve quien ocupe el puesto, aunque haya cambiado el titular

  Escenario: Con el bloqueo apagado para el régimen, se advierte, se exige acuse y se registra quién continuó
    Dado una póliza de seguro vencida el "2026-08-31"
    Cuando el Encargado de Despacho "Mario Fúnez" despacha "TR-0092" el "2026-09-24"
    Entonces el sistema advierte "La póliza de TR-0092 venció el 31/08/2026. El seguro no es obligatorio por ley vigente y el bloqueo está apagado para el régimen propio. Si continúa, quedará registrado que usted despachó con la póliza vencida."
    Y exige el acuse de "Mario Fúnez" antes de permitir el despacho
    Y permite el despacho
    Y registra en el expediente "advertencia superada por Mario Fúnez el 24/09/2026"
    Y ese registro alimenta el reporte de exposición por póliza vencida

  Escenario: Se rechaza el despacho sin el acuse del despachador
    Dado una póliza de seguro vencida el "2026-08-31"
    Cuando el Encargado de Despacho intenta despachar "TR-0092" sin acusar la advertencia
    Entonces el sistema no ejecuta el despacho
    Y muestra "Acuse la advertencia de póliza vencida para continuar. La institución necesita saber quién asumió el despacho sin cobertura."

  Escenario: El mismo vencimiento bloquea cuando el régimen de tenencia lo exige
    Dado un vehículo "TR-0140" con régimen de tenencia "alquilado"
    Y una póliza vencida el "2026-08-31"
    Cuando el Encargado de Despacho intenta despachar "TR-0140" el "2026-09-24"
    Entonces el sistema rechaza el despacho
    Y muestra "La póliza del vehículo alquilado TR-0140 venció el 31/08/2026 y el bloqueo está activado para el régimen alquilado."
    Y el rechazo no se levanta con acuse

  Escenario: El valor del parámetro se resuelve por régimen, no por institución
    Dado un vehículo "TR-0092" en régimen "propio" y un vehículo "TR-0140" en régimen "alquilado"
    Y ambos con póliza vencida el "2026-08-31"
    Cuando el Encargado de Despacho intenta despachar los dos el "2026-09-24"
    Entonces "TR-0092" se despacha con advertencia y acuse
    Y "TR-0140" se rechaza
    Y el sistema no aplica un valor único de institución a los dos regímenes

  Escenario: El título de tenencia vencido bloquea con la fecha concreta
    Dado un título de comodato vigente hasta el "2026-11-14"
    Cuando el Jefe de Transporte programa una misión del "2026-11-10" al "2026-11-18"
    Entonces el sistema rechaza la programación
    Y muestra "El título de tenencia de TR-0092 vence el 14/11/2026, antes del retorno previsto el 18/11/2026."

  Escenario: La renovación no edita el vencimiento anterior
    Cuando el Jefe de Transporte registra la matrícula renovada con vigencia del "2026-12-01" al "2027-11-30" y adjunto
    Entonces el sistema cierra el rango anterior y abre uno nuevo
    Y el registro anterior permanece consultable
    Y no se modifica la fecha de vencimiento del registro anterior

  Escenario: Se ofrece rehabilitar cuando ya no queda causa activa
    Dado un vehículo "TR-0092" en "NO_DISPONIBLE" por documentación vencida
    Cuando el Jefe de Transporte registra el documento renovado y no queda ninguna causa activa
    Entonces el sistema ofrece habilitar el vehículo
    Y muestra "No quedan causas activas de indisponibilidad en TR-0092. ¿Habilitar en flota?"

  Escenario: Un documento que vence con el vehículo en misión no cambia su estado
    Dado un vehículo "TR-0092" en estado "EN_MISION"
    Cuando la matrícula vence el "2026-11-30" con la misión en curso
    Entonces el estado operativo del vehículo no cambia
    Y el sistema genera alerta
    Y al retornar lleva el vehículo a "NO_DISPONIBLE" con causa "matrícula vencida"

  Escenario: La alerta crónica se reconoce con fundamento por un período
    Dado una alerta de trámite de placa detenido desde hace "26" meses
    Cuando el Jefe de Transporte la reconoce con fundamento por "180" días
    Entonces la alerta se suprime hasta el vencimiento del período y reaparece después
    Y el sistema no ofrece silenciarla de forma permanente

  Escenario: Se rechaza registrar un documento sin adjunto ni rango de vigencia
    Cuando el Jefe de Transporte registra una revisión mecánica sin adjunto
    Entonces el sistema rechaza el registro
    Y muestra "Todo documento del expediente vehicular exige adjunto y rango de vigencia."
```

## Fuera de alcance

- El trámite de renovación ante la autoridad: SIGTI registra el resultado, no gestiona el trámite
- La habilitación en flota — es [HU-102](HU-102-habilitar-el-vehiculo-en-flota.md)
- El mantenimiento preventivo y sus vencimientos: pertenece a M-11
- El estado de la placa — es [HU-097](HU-097-registrar-la-placa-y-el-estado-de-la-lamina.md)
- **La aplicación del bloqueo al programar y al asignar** —con sus mensajes y su registro de advertencia superada— es [HU-023](HU-023-documentacion-y-estado-operativo-del-vehiculo.md). Delimitación de `HB34-12`: esta historia define **el dato, su parámetro por régimen y su ciclo de vida**; `HU-023` manda en **el acto de asignar**

## Notas y pendientes

- `[V]` Que el seguro y la revisión mecánica **no son obligatorios por ley vigente** — [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md). El bloqueo por ellos es configurable y está apagado por defecto ([DP-001 D-13](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))
- `[C]` `umbrales_alerta_vencimiento` por tipo de documento. El valor de referencia 60 / 30 / 15 días es propuesta, no dato confirmado — insumo **#1**
- `[C]` **¿El mantenimiento preventivo vencido bloquea o advierte?** — insumo **#59**
- `[C]` Qué documentos exige la institución en el expediente vehicular más allá de matrícula, seguro y revisión — insumo **#1**
- `[C]` Día inhábil y circulación en vehículos en comodato y alquiler — insumo **#56**
- `[C]` **Valor de `bloqueo_por_poliza_vencida` y `bloqueo_por_revision_vencida` para cada régimen de tenencia.** Se adopta como propuesta *apagado para propio y comodato, encendido para alquilado*, porque el contrato de alquiler normalmente obliga a mantener la póliza vigente (`RN-16`). **Es reversible** y se confirma contra los contratos vigentes — insumos **#57** y **#1**
