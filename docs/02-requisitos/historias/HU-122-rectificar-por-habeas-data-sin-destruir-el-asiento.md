# HU-122 — Rectificar un dato personal por hábeas data sin destruir el registro contable original

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-12 Auditor Interno · `[C]` Oficial de Información Pública — actor no catalogado |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta definir quién ejerce este rol y quién lo autoriza |

## Historia

**Como** responsable institucional de atender una acción de hábeas data
**quiero** rectificar el dato personal que una persona señala como incorrecto, dejando el manifiesto original intacto
**para** satisfacer el derecho de rectificación del Artículo 182 constitucional sin romper la cadena de auditoría que el Tribunal Superior de Cuentas va a revisar

## Contexto

Aquí chocan dos normas del mismo Estado, y el diseño tiene que satisfacer las dos ([RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md)):

| Norma | Exige |
|---|---|
| [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) | Conservar todo por el plazo de prescripción. **Nada se borra físicamente.** Cadena de auditoría verificable |
| [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) | Hábeas data con derecho de actualización, rectificación y, en su caso, supresión `[V]` |

La resolución no es procedimental, es estructural. [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) lo dice: rectificar **dejando traza de la rectificación sin destruir el registro contable original**. Y [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) ya lo tenía resuelto para todo el sistema: **ningún registro se borra; toda corrección es un asiento**.

El error caro sería permitir editar el manifiesto cerrado. Un manifiesto editable después del despacho no sirve para detectar uso indebido de vehículos ([RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)) — y si además se puede editar por vía de hábeas data, esa vía se convertiría en la puerta trasera para corregir lo que la liquidación señaló.

## Reglas que la gobiernan

- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Rectificar dejando traza, **sin destruir el registro contable original**
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Toda corrección es asiento reverso con motivo y autor; nada se borra físicamente
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — El manifiesto cerrado no se edita, tampoco por esta vía
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — La rectificación es también un acceso y se registra
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — Un reporte reproducido a una fecha de corte anterior sigue mostrando el valor de entonces

## Requisitos no funcionales relacionados

- [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) — **0** rectificaciones de hábeas data aplicadas sobre el registro original
- [RNF-04](../no-funcionales/RNF-04-bitacora-append-only-con-hash-encadenado.md) — La cadena de hash no se rompe
- [RNF-06](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md) — Reproducibilidad histórica de reportes

## Criterios de aceptación

> Los nombres y números de identidad de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Rectificación por hábeas data sobre manifiestos cerrados

  Antecedentes:
    Dado un manifiesto cerrado de la Orden de Misión "OM-2026-0451", cerrado el "2026-09-18" a las "05:52"
    Y una ficha con nombre "Ana de Prueva Uno" e identidad "0000-0000-00001"
    Y un expediente de hábeas data "HD-2026-004" en el que la titular señala que su nombre está mal escrito
    Y una liquidación de "OM-2026-0451" ya cerrada

  Escenario: Se rechaza editar el manifiesto cerrado
    Cuando el Auditor Interno intenta corregir el nombre directamente sobre la ficha del manifiesto
    Entonces el sistema rechaza la edición
    Y muestra "El manifiesto se cerró el 18/09/2026 a las 05:52 y no se edita. Registre un asiento de rectificación vinculado al expediente de hábeas data."

  Escenario: Se rechaza suprimir el registro de la persona
    Cuando el Auditor Interno intenta suprimir la ficha de "Ana de Prueva Uno" del manifiesto de "OM-2026-0451"
    Entonces el sistema rechaza la supresión
    Y muestra "Ningún registro se borra físicamente. La supresión del dato personal se resuelve por seudonimización según la política de retención, no por borrado."

  Escenario: Se rechaza la rectificación sin expediente ni autoridad que la ordene
    Cuando el Auditor Interno registra una rectificación del nombre sin indicar expediente ni autoridad
    Entonces el sistema rechaza la rectificación
    Y muestra "Indique el expediente que la motiva y la autoridad que la ordena. Una rectificación sin fundamento registrado es indistinguible de una alteración."

  Escenario: Se rechaza rectificar un dato de gestión pública por esta vía
    Cuando el Auditor Interno intenta rectificar el destino declarado de la misión "Danlí" por "Trojes" invocando el expediente "HD-2026-004"
    Entonces el sistema rechaza la rectificación
    Y muestra "El destino de la misión es dato de gestión pública, no dato personal. Corríjalo por la vía del expediente de la misión, si procede."

  Escenario: La rectificación se registra como asiento y conserva el valor original
    Cuando el Auditor Interno registra la rectificación del nombre de "Ana de Prueva Uno" a "Ana de Prueba Uno", con expediente "HD-2026-004", autoridad "resolución institucional de ejemplo" y fecha "2026-10-08"
    Entonces el sistema crea un asiento de rectificación con dato anterior, dato nuevo, expediente, autoridad, autor y fecha
    Y el manifiesto cerrado conserva "Ana de Prueva Uno" como valor original
    Y las vistas actuales muestran "Ana de Prueba Uno" con indicación de "dato rectificado el 08/10/2026"

  Escenario: La cadena de auditoría verifica sin ruptura después de la rectificación
    Cuando se ejecuta el verificador de la cadena sobre el período que contiene "OM-2026-0451"
    Entonces la cadena verifica sin ninguna ruptura
    Y el sello emitido antes de la rectificación sigue siendo válido

  Escenario: El reporte reproducido a la fecha de corte anterior no cambia
    Dado un reporte de traslados generado con fecha de corte "2026-09-30"
    Cuando se regenera ese reporte el "2026-11-15" con la misma fecha de corte "2026-09-30"
    Entonces el reporte muestra "Ana de Prueva Uno", tal como se conocía a esa fecha
    Y los conteos, montos y estructura son idénticos a los de la primera generación

  Escenario: La rectificación no altera la liquidación
    Cuando el Auditor Interno registra la rectificación del expediente "HD-2026-004"
    Entonces la liquidación cerrada de "OM-2026-0451" conserva sus conteos y montos sin cambio
    Y la Orden de Misión no cambia de estado
```

## Fuera de alcance

- La localización y exportación previas — es [HU-121](HU-121-atender-habeas-data-buscar-y-exportar.md)
- La seudonimización por vencimiento del plazo de retención — es [HU-124](HU-124-depurar-datos-personales-sin-romper-la-cadena.md)
- La corrección de datos operativos de la misión — la gobiernan [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md) y [RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)
- Los datos de la persona en ARGOS o Talento Humano: son espejo de solo lectura ([RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)); la rectificación se pide al sistema origen

## Notas y pendientes

- `[C]` **Quién ejerce este rol y quién autoriza la rectificación.** El Oficial de Información Pública no está catalogado como actor ([actores-y-roles.md](../../01-negocio/actores-y-roles.md)); y **quien atiende el hábeas data no debería ser quien ejecuta la rectificación** si se quiere sostener la segregación de funciones. Es lo que mantiene la historia en borrador
- `[C]` Si la supresión total puede ser ordenada judicialmente y qué se hace entonces con el asiento contable. La propuesta —seudonimizar, nunca borrar— sale de [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) y [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), pero **una orden judicial de supresión no está analizada**
- `[C]` Plazos de retención y de seudonimización — insumo #71, con Auditoría Interna y el OIP
- `[I]` Que la rectificación no altere la liquidación ni el estado de la orden es derivación de [RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md) aplicada por analogía, no una cita de norma
