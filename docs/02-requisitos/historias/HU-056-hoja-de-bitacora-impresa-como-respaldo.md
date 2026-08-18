# HU-056 — Continuar la captura en la hoja de bitácora impresa cuando el dispositivo falla

| Campo | Valor |
|---|---|
| **Módulo** | M-15 Formatos Oficiales e Impresión · M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador |

## Historia

**Como** Motorista
**quiero** llevar siempre conmigo la hoja de bitácora impresa con folio, con las mismas casillas y en el mismo orden que la pantalla
**para** poder seguir registrando cuando el teléfono se queda sin batería, se moja o se daña, sin que se pierda ni el viaje ni el descargo

## Contexto

El híbrido digital-papel de SIGTI es **por diseño, no por parche**. El dispositivo se moja bajo la lluvia, se cae, se queda sin batería en un tramo de ocho horas sin dónde cargar. Que eso pase no puede significar que la misión quede sin bitácora.

La clave es la **paridad exacta**: las casillas del papel están en el mismo orden y con los mismos nombres que la pantalla ([RN-80](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)). Sin paridad, digitar el papel exige traducir, y traducir es donde se pierde el dato.

**Lo capturado antes del fallo no se pierde**: sigue en el dispositivo y sincroniza cuando se recupere. Si el dispositivo no se recupera, lo registrado en él se reconstruye desde el papel y **se declara así**.

## Reglas que la gobiernan

- [RN-80](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md) — **Regla rectora**: el despacho emite la hoja de bitácora en papel, con folio, QR y paridad exacta con la pantalla de digitación
- [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) — La digitación desde papel deja constancia de quién digitó y del original fotografiado
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Los folios se asignan de rangos por delegación, que funcionan sin conectividad
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Nada se pierde

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Bitácora en papel digitada días después
- [CE-22](../casos-especiales/CE-22-odometro-inconsistente.md) — El odómetro que el papel no trae y que nadie debe deducir

## Criterios de aceptación

```gherkin
# language: es
Característica: Hoja de bitácora impresa como respaldo del dispositivo

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" despachada el "2026-05-12" en la Delegación de Choluteca
    Y un rango de folios asignado a la Delegación de Choluteca del "CHO-2026-000401" al "CHO-2026-000600"

  Escenario: Se rechaza el despacho sin emitir la hoja de bitácora impresa
    Cuando el Encargado de Despacho intenta cerrar el despacho de "OM-2026-0451" sin emitir la hoja de bitácora
    Entonces el sistema rechaza el cierre del despacho
    Y muestra "Falta emitir e imprimir la hoja de bitácora con folio. Es el respaldo obligatorio si el teléfono falla en ruta."

  Escenario: La hoja impresa lleva folio, QR y hash del documento electrónico
    Cuando el Encargado de Despacho emite la hoja de bitácora de "OM-2026-0451"
    Entonces el sistema asigna el folio "CHO-2026-000401" del rango de la delegación
    Y la hoja impresa incluye el folio, el QR de verificación, el hash del documento electrónico y espacio de firma y sello

  Escenario: Las casillas del papel están en el mismo orden y con el mismo nombre que la pantalla
    Cuando el Encargado de Delegación abre la pantalla de digitación diferida del folio "CHO-2026-000401"
    Entonces la pantalla presenta las casillas en el mismo orden y con los mismos nombres que la hoja impresa
    Y no exige al digitador reordenar, agrupar ni interpretar ninguna casilla

  Escenario: El dispositivo se daña y lo capturado antes no se pierde
    Dado que el dispositivo "DEL-CHO-03" registró "18" eventos y quedó sin batería el "2026-05-14"
    Cuando el dispositivo se recupera el "2026-05-16" y aparece señal
    Entonces los "18" eventos sincronizan con su identificador y su secuencia original
    Y ninguno se duplica con lo digitado desde el papel, porque el identificador del cliente es la llave

  Escenario: El dispositivo no se recupera y la bitácora se reconstruye desde el papel
    Dado que el dispositivo "DEL-CHO-03" se perdió y no se recuperará
    Cuando el Encargado de Delegación digita la hoja de bitácora folio "CHO-2026-000401"
    Entonces el sistema registra los hechos con modo de captura "digitación diferida de papel"
    Y declara en el expediente que lo registrado en el dispositivo se reconstruyó desde el original
    Y muestra "Bitácora reconstruida desde el papel. El registro del dispositivo no se recuperó."

  Escenario: Lo que el papel no trae, no se deduce
    Dado un folio "CHO-2026-000401" sin odómetro anotado en el arribo a "Puesto Fronterizo El Amatillo"
    Cuando el Encargado de Delegación digita ese arribo
    Entonces el sistema registra el odómetro como "no consignado en el original"
    Y no permite calcularlo restando ni interpolando entre lecturas
    Y muestra "El original no trae este kilometraje. Se registra como no consignado; no lo deduzca."
```

## Fuera de alcance

- La digitación diferida completa con su imputación de causa — es [HU-064](HU-064-digitacion-diferida-desde-el-papel.md)
- El diseño gráfico del formato oficial y su verificación por QR — es de M-15 y [RNF-11](../no-funcionales/RNF-11-formatos-oficiales-imprimibles-y-verificables.md)
- El punto de verificación público del QR, que depende de una decisión de exposición externa del despliegue on-premise

## Notas y pendientes

- `[C]` Formatos en papel vigentes de la institución, campo por campo. Sin ellos no hay paridad que verificar — insumo #2
- `[C]` Parque real de impresoras en sede y delegaciones: matriciales, láser, tamaño de papel — insumo #70
- `[C]` Si la institución acepta exponer un punto público de verificación del QR siendo el despliegue on-premise — pendiente G
