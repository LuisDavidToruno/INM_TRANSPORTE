# HU-120 — Consultar la lista de abordo en el dispositivo sin conectividad, dejando registro local de la consulta

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Motorista
**quiero** abrir la lista de las personas que llevo a bordo sin ninguna señal, y que esa consulta quede registrada en el dispositivo
**para** poder verificar en un retén o en el destino quién va conmigo, sin que el sistema deje de registrar quién vio esos datos por el hecho de estar fuera de cobertura

## Contexto

[RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) resuelve este caso límite sin ambigüedad: *"El registro se genera localmente y sincroniza después. **No se admite acceso sin registro por estar fuera de línea: si el dispositivo no puede registrar, no muestra el dato.**"*

Es una decisión incómoda y correcta. La alternativa —mostrar el dato y no registrar la consulta porque no hay red— convertiría la falta de señal en la vía normal para consultar sin dejar rastro, en un país donde más de 2 millones de personas del área rural no tienen acceso a internet `[V]` ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), INE EPHPM julio 2025). O sea: la vía normal, a secas.

El dispositivo lleva **solo el manifiesto de su misión**, no el de otras. Y lo que lleva es la versión mínima indispensable para el control en carretera, no el manifiesto completo ([RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)).

## Reglas que la gobiernan

- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — **Regla rectora**: si el dispositivo no puede registrar la consulta, no muestra el dato
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Toda captura de campo, incluido el registro de la consulta, se completa sin conectividad y nunca se pierde
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — El identificador del registro de consulta se genera en el dispositivo
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — La consulta registra el momento en que ocurrió, no el de sincronización
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — El dispositivo lleva los datos mínimos indispensables, no el manifiesto completo
- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) — Solo el titular o el relevo declarado abre la misión en el dispositivo

## Requisitos no funcionales relacionados

- [RNF-03](../no-funcionales/RNF-03-operacion-sin-conectividad.md) — Operación sin conectividad
- [RNF-13](../no-funcionales/RNF-13-cifrado-en-transito-y-en-reposo.md) — Cifrado en reposo del almacén local
- [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) — La depuración alcanza los almacenes locales al siguiente contacto

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Lo que ocurre cuando el dispositivo no sirve y todo se resuelve en papel

## Criterios de aceptación

> Los nombres de personas externas de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Consulta de la lista de abordo en el dispositivo de campo

  Antecedentes:
    Dado un motorista "José Martínez" declarado titular de la Orden de Misión "OM-2026-0451"
    Y un paquete de misión entregado en el despacho el "2026-09-18" al dispositivo "DEL-CHO-03"
    Y un manifiesto cerrado con "3" personas externas
    Y que el dispositivo no ha tenido ninguna conectividad desde el "2026-09-18"

  Escenario: No se muestra el manifiesto si el dispositivo no puede registrar la consulta
    Dado que el almacenamiento local del dispositivo está lleno y no admite registros nuevos
    Cuando "José Martínez" intenta abrir la lista de abordo de "OM-2026-0451"
    Entonces el sistema no muestra la lista
    Y muestra "No hay espacio para registrar la consulta y por eso no se puede mostrar la lista. Libere espacio o envíe los pendientes cuando tenga señal."

  Escenario: No se muestra el manifiesto de una misión que no está en el dispositivo
    Cuando "José Martínez" intenta abrir la lista de abordo de la Orden de Misión "OM-2026-0468"
    Entonces el sistema rechaza la apertura
    Y muestra "La Orden de Misión OM-2026-0468 no está en este dispositivo. Solo puede consultar OM-2026-0451, entregada el 18/09/2026 en el despacho."

  Escenario: El dispositivo no lleva los campos que no se necesitan en carretera
    Cuando "José Martínez" abre la lista de abordo de "OM-2026-0451"
    Entonces el sistema muestra por persona: nombre, tipo y número de identificación, origen y destino
    Y no muestra ningún campo de clase salud, etnia, situación migratoria ni condición de vulnerabilidad
    Y muestra por separado a las "3" personas externas y a los "2" servidores de la institución

  Escenario: La consulta sin señal genera registro local pendiente de envío
    Dado que el dispositivo lleva 4 días sin conectividad
    Cuando "José Martínez" abre la lista de abordo de "OM-2026-0451" el "2026-09-22" a las "09:12"
    Entonces el sistema muestra la lista
    Y guarda un registro de consulta con identificador generado en el dispositivo
    Y registra "ocurrido_en" en "2026-09-22 09:12" y "capturado_en" no editable
    Y lo deja en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: Los registros de consulta se envían al recuperar señal y no se descartan
    Dado "7" registros de consulta pendientes de envío en el dispositivo "DEL-CHO-03"
    Cuando el dispositivo recupera conectividad el "2026-09-24"
    Entonces el sistema envía los "7" registros conservando su "ocurrido_en" original
    Y ninguno se descarta por antigüedad ni por conflicto con el servidor

  Escenario: La depuración de datos personales alcanza el almacén local
    Dado una depuración de datos personales ejecutada en el servidor el "2027-03-01"
    Cuando el dispositivo "DEL-CHO-03" vuelve a tener contacto con el servidor el "2027-03-06"
    Entonces el dispositivo aplica la depuración sobre los manifiestos que conserva localmente
    Y deja constancia de que la aplicó, con fecha y alcance
    Y el registro de consultas previo se conserva referenciando el identificador seudonimizado
```

## Fuera de alcance

- El envío y la resolución de conflictos de sincronización — es [HU-066](HU-066-sincronizar-sola-y-reanudable.md) y [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md)
- El contador de pendientes y el manejo del almacenamiento lleno en general — es [HU-054](HU-054-pendientes-de-envio-y-adjunto-pendiente.md)
- El registro de novedades del manifiesto — es [HU-116](HU-116-registrar-novedades-del-manifiesto-en-ruta.md)
- La ejecución de la depuración en el servidor — es [HU-124](HU-124-depurar-datos-personales-sin-romper-la-cadena.md)
- El mecanismo de almacenamiento local: `ADR-000` difiere el stack al Sprint 2; aquí se describe comportamiento observable

## Notas y pendientes

- `[C]` **Dispositivo de campo de referencia** y su capacidad de almacenamiento — insumo #69. Determina cuántos registros de consulta y adjuntos caben antes de que se dispare el escenario de rechazo
- `[C]` Qué hace el motorista cuando el dispositivo no puede mostrar la lista y hay un retén enfrente. La vía degradada es la **lista de abordo impresa** que porta ([HU-114](HU-114-cerrar-el-manifiesto-al-despachar.md)); confirmar que la institución la acepta como suficiente
- `[I]` Que el dispositivo lleve solo los campos indispensables es aplicación de [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) al almacén local, no una exigencia literal de norma
