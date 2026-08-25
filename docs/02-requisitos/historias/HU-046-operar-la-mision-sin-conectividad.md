# HU-046 — Operar la Orden de Misión en el dispositivo sin ninguna conectividad

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada · M-08 Ejecución y Bitácora |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta la duración máxima real de la misión que ejecuta la institución (insumo #67): si supera 7 días, sube el umbral de `RNF-03` y cambia el dimensionamiento del cliente de campo. Falta también el dispositivo de campo de referencia (insumo #69) |

## Historia

**Como** Motorista
**quiero** abrir mi Orden de Misión y registrar cada hecho de la ruta sin ninguna señal de datos ni de voz
**para** no tener que llenar la bitácora en papel y digitarla días después, que es lo que hago hoy y lo que deja el expediente incompleto ante el Tribunal Superior de Cuentas

## Contexto

Esta es la historia que decide si el sistema se usa. Más de 2 millones de personas del área rural hondureña no tienen acceso a internet `[V]` ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), INE EPHPM julio 2025). Una misión a Gracias a Dios, Olancho o la Mosquitia pasa días completos fuera de cobertura.

**La ausencia de red no es una excepción del sistema: es la condición normal de operación.** Todo lo que exija conectividad para registrarse se llenará en papel, y el papel se digita tarde, mal o nunca ([CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md)).

El dispositivo recibe en el despacho el **paquete de misión**: expediente, documentos, paquete normativo congelado, puntos de peaje de la ruta con su categoría y tarifa esperada, estaciones, catálogo de tipificaciones y guía de actuación en accidente. Con eso opera solo.

## Reglas que la gobiernan

- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — **Regla rectora**: toda captura de campo se completa sin ninguna conectividad y nunca se pierde
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — El identificador de cada registro se genera en el cliente; es la llave de idempotencia al sincronizar
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — `ocurrido_en` y `capturado_en` son campos distintos, ambos obligatorios
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — Todo cálculo en ruta usa el paquete normativo congelado, no la tabla actual del servidor
- [RN-57](../../01-negocio/reglas/RN-57-habilitacion-de-quien-efectivamente-conduce.md) — El dispositivo solo acepta al titular o a un relevo declarado en la programación

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Bitácora en papel digitada días después: es lo que esta historia existe para evitar
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — La misión se extiende más días de los previstos y el dispositivo sigue solo

## Criterios de aceptación

```gherkin
# language: es
Característica: Operación de la misión en el dispositivo sin conectividad

  Antecedentes:
    Dado un motorista "José Martínez" declarado titular de la Orden de Misión "OM-2026-0451"
    Y un paquete de misión entregado en el despacho el "2026-05-12" al dispositivo "DEL-CHO-03"
    Y que el dispositivo no ha tenido ninguna conectividad desde el "2026-05-12"

  Escenario: Se rechaza abrir una misión que no viene en el paquete
    Dado que el dispositivo lleva 4 días sin conectividad
    Cuando "José Martínez" intenta abrir la Orden de Misión "OM-2026-0468"
    Entonces el sistema rechaza la apertura
    Y muestra "La Orden de Misión OM-2026-0468 no está en este dispositivo. Solo puede registrar OM-2026-0451, entregada el 12/05/2026 en el despacho. Si le asignaron otra misión, solicítela en el despacho antes de salir."

  Escenario: Se rechaza el acceso de quien no es el motorista declarado
    Dado que el dispositivo lleva 4 días sin conectividad
    Y un servidor "Carlos Fúnez" que no figura como titular ni como relevo declarado de "OM-2026-0451"
    Cuando "Carlos Fúnez" intenta abrir la Orden de Misión "OM-2026-0451"
    Entonces el sistema rechaza el acceso
    Y muestra "Usted no está declarado como motorista ni como relevo de esta misión. Solo José Martínez puede registrar en ella."

  Escenario: El motorista se autentica sin ninguna señal
    Dado que el dispositivo lleva 7 días sin conectividad
    Cuando "José Martínez" se autentica en el dispositivo "DEL-CHO-03"
    Entonces el sistema abre la Orden de Misión "OM-2026-0451"
    Y muestra los destinos en el orden previsto, la ventana autorizada y el odómetro de salida ya registrado
    Y no solicita en ningún momento conexión de datos

  Escenario: Cada hecho registrado sin red queda completo y pendiente de envío
    Dado que el dispositivo lleva 4 días sin conectividad
    Cuando "José Martínez" registra el arribo al destino "Delegación de Choluteca" con odómetro "93061"
    Entonces el sistema guarda el evento con identificador generado en el dispositivo
    Y le asigna el número de secuencia siguiente del dispositivo
    Y registra "ocurrido_en" con la hora del hecho y "capturado_en" no editable
    Y lo deja en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: La captura continúa después del octavo día sin red
    Dado que el dispositivo lleva 9 días sin conectividad
    Cuando "José Martínez" registra un abastecimiento de combustible
    Entonces el sistema registra el abastecimiento sin ninguna restricción
    Y muestra "Lleva 9 días sin enviar. 34 registros pendientes. Se enviarán solos cuando haya señal."
```

## Fuera de alcance

- La generación y entrega del paquete de misión en el despacho — es de [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md)
- El envío de lo capturado y la resolución de conflictos — es [HU-066](HU-066-sincronizar-sola-y-reanudable.md) y siguientes
- El contador de pendientes y el manejo del almacenamiento lleno — es [HU-054](HU-054-pendientes-de-envio-y-adjunto-pendiente.md)
- El mecanismo de sincronización: `ADR-000` difiere el stack al Sprint 2; aquí se describe comportamiento observable

## Notas y pendientes

- `[C]` Duración máxima real de la misión que ejecuta la institución. Si supera 7 días, sube el umbral de [RNF-03](../no-funcionales/RNF-03-operacion-sin-conectividad.md) — insumo #67
- `[C]` Dispositivo de campo de referencia: qué celular tienen hoy los motoristas y quién paga el plan de datos — insumo #69
- `[I]` La autenticación sin red se resuelve contra credenciales del paquete. Sin firma electrónica certificada en el país, la autorización es interna con registro completo de quién, cuándo y sobre qué contenido
