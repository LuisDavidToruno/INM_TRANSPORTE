# HU-113 — Registrar en el manifiesto a la persona que no porta documento de identidad, sin bloquear el traslado

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-06 Solicitudes de Transporte |
| **Actor** | ACT-05 Encargado de Despacho |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — depende del catálogo mínimo de datos (insumo nuevo a registrar) |

## Historia

**Como** Encargado de Despacho
**quiero** poder registrar en el manifiesto a una persona que no trae tarjeta de identidad, con una identificación alternativa o como no identificada
**para** que el traslado salga amparado y con constancia de quién iba, en lugar de que la persona suba sin figurar en ningún papel

## Contexto

Es el caso límite que [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) declara **frecuente**: *"Persona sin documento de identidad. El manifiesto debe admitir identificación alternativa o registro como no identificada con descripción mínima. Exigir un número de identidad bloquearía traslados legítimos."*

Y el modo de fallar es conocido: si el sistema exige un número de 13 dígitos, en el predio a las cinco de la mañana **alguien inventa el número** o la persona sube sin registrarse. Las dos salidas son peores que un registro incompleto y honesto. Un dato falso en el manifiesto es exactamente el tipo de asiento que un auditor del TSC encuentra y que la institución no puede explicar.

El campo obligatorio y único vuelve a ser el enemigo, igual que con la placa metálica ([RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md)): la realidad hondureña no lo sostiene.

## Reglas que la gobiernan

- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Identificación alternativa o registro como no identificada; la descripción mínima **no** es descripción de rasgos étnicos ni de condición
- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) — El dato que no se puede capturar completo al abordar se registra al mínimo y se completa después **como novedad**, con marca de registro diferido
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — El completamiento posterior conserva `ocurrido_en` del abordaje y `capturado_en` del momento real de digitación
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Completar el dato no sobrescribe el registro original: lo complementa

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Personas externas que se suman en el retorno, cuando ya nadie está en la oficina para verificar nada

## Criterios de aceptación

> Todos los nombres y números de identidad de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Identificación de la persona externa que no porta documento

  Antecedentes:
    Dado un catálogo "tipo_identificacion_persona_externa" vigente al "2026-09-01" con los valores
      | valor                          |
      | tarjeta de identidad           |
      | pasaporte                      |
      | carné institucional            |
      | constancia de la autoridad     |
      | no identificada                |
    Y una Orden de Misión "OM-2026-0451" en estado "PROGRAMADA"

  Escenario: Se rechaza dejar la identificación en blanco sin declarar el tipo
    Cuando el Encargado de Despacho registra a "Ana de Prueba Uno" sin tipo de identificación y sin número
    Entonces el sistema rechaza el registro
    Y muestra "Elija el tipo de identificación. Si la persona no porta documento, registre 'no identificada' con la descripción mínima; nunca deje el campo vacío ni invente un número."

  Escenario: Se rechaza un número de identidad con formato imposible
    Cuando el Encargado de Despacho registra a "Beto de Prueba Dos" con tipo "tarjeta de identidad" y número "1111111111111"
    Entonces el sistema rechaza el registro
    Y muestra "El número no corresponde al formato de tarjeta de identidad. Si no dispone del documento, registre 'no identificada' en lugar de aproximar el número."

  Escenario: Se rechaza una descripción mínima con rasgos étnicos o de condición
    Cuando el Encargado de Despacho registra a una persona con tipo "no identificada" y descripción mínima "mujer indígena lenca en situación de calle"
    Entonces el sistema rechaza la descripción
    Y muestra "La descripción mínima solo admite lo necesario para distinguir a la persona en el manifiesto. No registre etnia, nacionalidad de origen migratorio ni condición de vulnerabilidad."

  Escenario: Se registra a la persona no identificada con descripción mínima
    Cuando el Encargado de Despacho registra en "OM-2026-0451" una persona con tipo "no identificada", nombre declarado "Carla de Prueba Tres" y descripción mínima "adulta, viaja con la comisión de la alcaldía de ejemplo"
    Entonces el sistema acepta el registro
    Y marca la ficha como "IDENTIFICACIÓN PENDIENTE"
    Y permite despachar la misión

  Escenario: La identificación pendiente se completa después como novedad
    Dado una ficha marcada "IDENTIFICACIÓN PENDIENTE" en el manifiesto cerrado de "OM-2026-0451", abordaje del "2026-09-18" a las "05:40"
    Cuando el Encargado de Despacho registra el "2026-09-21" el número de tarjeta de identidad "0000-0000-00003" de esa persona
    Entonces el sistema registra una novedad de tipo "completamiento de identificación"
    Y conserva "ocurrido_en" en "2026-09-18 05:40" y "capturado_en" en "2026-09-21"
    Y no modifica el manifiesto cerrado
    Y retira la marca "IDENTIFICACIÓN PENDIENTE" de la ficha

  Escenario: La misión cierra con hallazgo si la identificación queda pendiente
    Dado una Orden de Misión "OM-2026-0451" en estado "RETORNADA" con "1" ficha en "IDENTIFICACIÓN PENDIENTE"
    Cuando la Gerencia Administrativa intenta cerrar el expediente de la misión
    Entonces el sistema permite el cierre como "CERRADA_CON_HALLAZGO"
    Y muestra "1 persona externa quedó sin identificar. La misión cierra con hallazgo de tipo IDENTIFICACIÓN INCOMPLETA."
```

## Fuera de alcance

- El catálogo completo de campos del manifiesto — es [HU-111](HU-111-registrar-manifiesto-de-personas-externas.md)
- El régimen especial de menores y personas bajo custodia: **no se diseña** hasta el insumo #39
- La verificación de la identidad contra el RNP: no hay integración disponible; el dato es el que capturó la institución

## Notas y pendientes

- `[C]` **Formato del número de tarjeta de identidad** que la institución valida hoy en sus formatos en papel — insumo #2. Mientras no se confirme, la validación de formato es **parámetro configurable**, no una expresión en el código
- `[C]` **¿Traslada la institución menores o personas bajo custodia?** — insumo #39. En esos supuestos la identificación alternativa probablemente sea una constancia de autoridad, no una decisión del despachador
- `[C]` **¿Cerrar con hallazgo o bloquear el cierre** cuando queda una identificación pendiente? Aquí se propuso hallazgo por [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md); confirmar con Auditoría Interna
- `[I]` La marca "IDENTIFICACIÓN PENDIENTE" es derivación de [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) y [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md), no una figura de norma
