# 02 — Requisitos

Qué debe hacer el sistema, expresado de forma verificable.

## Estructura

| Ruta | Estado | Contenido |
|---|---|---|
| `casos-de-uso/` | Bloque 3 | Casos de uso `CU-xx`, agrupados por módulo `M-xx` |
| `historias/` | Bloque 3 | Historias de usuario `HU-xxx` con criterios de aceptación |
| `casos-especiales/` | **Bloque 2** | Excepciones `CE-xx` de la operación real, cada una con su regla de resolución |
| `no-funcionales/` | Bloque 3 | Requisitos no funcionales `RNF-xx` |

## Los casos especiales son el bloque de mayor valor

El flujo feliz de una solicitud de transporte lo diseña cualquiera. Lo que hunde un sistema de este tipo es la realidad: el vehículo que se avería en ruta, el motorista cuya licencia venció ayer, el viaje cancelado después de emitir los vales, la bitácora que se llenó en papel porque no había señal, el odómetro que no cuadra.

Cada `CE-xx` documenta:
- La situación real, en el lenguaje de quien la vive
- Con qué frecuencia ocurre y qué se hace hoy sin sistema
- La regla de resolución, enlazada a su `RN-xx`
- Qué estado del ciclo de vida se ve afectado

**Ningún caso especial queda sin regla de resolución.** Si no sabemos cómo resolverlo, se marca `[C]` y se escala al PO — no se deja implícito.

## Criterio de completitud

Una historia está lista para desarrollo (Definition of Ready) cuando referencia su módulo, al menos una regla de negocio, y los casos especiales que la afectan. Ver [../plantillas/definition-of-ready.md](../plantillas/definition-of-ready.md).
