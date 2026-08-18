# HU-116 — Registrar como novedad de ruta a quien no abordó, a quien se sumó y a quien bajó antes del destino

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-08 Ejecución y Bitácora · M-16 Sincronización y Operación Desconectada |
| **Actor** | ACT-06 Motorista |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Motorista
**quiero** registrar en ruta y sin señal que alguien no se presentó, que subió una persona más o que alguien bajó antes del destino
**para** que la diferencia entre lo autorizado y lo ocurrido quede escrita por mí en el momento, y no la tenga que explicar de memoria en la liquidación

## Contexto

Este es el punto donde el control se gana o se pierde. La tentación natural del usuario es **corregir el manifiesto** para que cuadre con lo que pasó. Si el sistema lo permite, el manifiesto deja de ser una declaración y pasa a ser un resumen ajustado ([RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)).

La persona que se suma en ruta puede ser perfectamente legítima —un servidor de la institución que se agrega en Danlí— o el caso clásico de uso indebido que persigue la Circular STLCC-ONADICI 022-03-2024 `[V]`. **El sistema no juzga**: exige registrar quién autorizó el cambio y produce la comparación. La decisión es de la liquidación.

Y ocurre donde no hay señal, así que la captura tiene que funcionar completamente desconectada ([RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)).

## Reglas que la gobiernan

- [RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) — **Regla rectora**: novedad tipificada con fecha del hecho, motivo y quién autorizó; nunca edición del manifiesto
- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — La novedad se captura sin ninguna conectividad y nunca se pierde
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — `ocurrido_en` y `capturado_en`, ambos obligatorios
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — No se puede bloquear a un vehículo en ruta: el exceso sobrevenido se registra, se alerta y produce hallazgo
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — La novedad tardía va a cola de resolución humana, no sobrescribe
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — La bitácora cerrada no se reabre: se corrige con asiento de corrección
- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — La persona que sube en ruta se registra con el mismo catálogo mínimo

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Las personas y la carga que aparecen después del despacho
- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — La novedad anotada en papel y digitada al retorno
- [CE-07](../casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) — Retorno anticipado con personas a bordo

## Criterios de aceptación

> Todos los nombres de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Novedades del manifiesto durante la ejecución de la misión

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0451" en estado "EN_RUTA"
    Y un manifiesto cerrado el "2026-09-18" a las "05:52" con "3" personas externas y "2" servidores
    Y un vehículo con "7" plazas homologadas incluido el motorista
    Y un motorista "José Martínez" con el dispositivo sin conectividad desde el "2026-09-18"

  Escenario: Se rechaza editar el manifiesto desde el dispositivo
    Cuando el Motorista intenta borrar del manifiesto a "Carla de Prueba Tres"
    Entonces el sistema rechaza la edición
    Y muestra "El manifiesto se cerró el 18/09/2026 a las 05:52 y no se edita. Registre una novedad de tipo 'persona que no abordó'."

  Escenario: Se rechaza una novedad sin tipificar
    Cuando el Motorista registra una novedad con el texto libre "cambios en el pasaje" y sin tipo
    Entonces el sistema rechaza la novedad
    Y muestra "Elija el tipo de novedad: persona adicional, persona que no abordó, descenso en punto intermedio, carga adicional, entrega parcial o entrega a consignatario distinto."

  Escenario: Se rechaza la persona adicional sin quién autorizó el cambio
    Cuando el Motorista registra una novedad de tipo "persona adicional" con la persona "Beto de Prueba Dos" y sin declarar quién autorizó
    Entonces el sistema rechaza la novedad
    Y muestra "Registre quién ordenó incorporar a esta persona: nombre y puesto. Una persona que sube sin que nadie la autorice es la que aparece en el acta del accidente sin dueño."

  Escenario: Se registra que una persona no abordó
    Cuando el Motorista registra una novedad de tipo "persona que no abordó" para "Ana de Prueba Uno", en "Tegucigalpa, predio institucional", el "2026-09-18" a las "05:58", motivo "no se presentó a la hora de salida"
    Entonces el sistema guarda la novedad con identificador generado en el dispositivo
    Y registra "ocurrido_en" en "2026-09-18 05:58" y "capturado_en" no editable
    Y el manifiesto cerrado conserva las "3" personas externas originales
    Y la deja en estado de sincronización "PENDIENTE_DE_ENVIO"

  Escenario: Se registra el descenso en un punto intermedio
    Cuando el Motorista registra una novedad de tipo "descenso en punto intermedio" para "Beto de Prueba Dos", en "Zamorano, desvío a la carretera principal", el "2026-09-18" a las "08:14"
    Entonces el sistema guarda la novedad con lugar y hora del hecho
    Y el destino declarado de esa persona en el manifiesto permanece sin cambios
    Y la liquidación mostrará la diferencia entre destino autorizado y descenso real

  Escenario: La persona adicional que excede la capacidad no bloquea, pero alerta y produce hallazgo
    Dado que a bordo van ya "7" ocupantes incluido el motorista
    Cuando el Motorista registra una novedad de tipo "persona adicional" para "Carla de Prueba Tres", autorizada por "jefatura de la dependencia solicitante"
    Entonces el sistema acepta la novedad
    Y muestra "Con esta persona el vehículo lleva 8 ocupantes y tiene 7 plazas homologadas. La novedad queda registrada y genera hallazgo de EXCESO DE OCUPACIÓN."
    Y notifica al Jefe de Transporte al recuperar la conectividad
    Y genera un hallazgo que impide cerrar la misión mientras esté abierto

  Escenario: La novedad registrada después del cierre de la bitácora entra como asiento de corrección
    Dado una Orden de Misión "OM-2026-0451" con la bitácora ya cerrada el "2026-09-20"
    Cuando el Motorista sincroniza el "2026-09-22" una novedad ocurrida el "2026-09-18" a las "08:14"
    Entonces el sistema no reabre la bitácora
    Y la coloca en la cola de conflictos para resolución humana
    Y una vez resuelta la incorpora como "asiento de corrección sobre bitácora cerrada" con su fecha del hecho
```

## Fuera de alcance

- El cierre del manifiesto al despachar — es [HU-114](HU-114-cerrar-el-manifiesto-al-despachar.md)
- El acta de entrega en el destino — es [HU-115](HU-115-cadena-de-custodia-de-personas-externas.md)
- La comparación manifiesto contra novedades y la tipificación de las diferencias en la liquidación — es de [CU-15](../casos-de-uso/CU-15-liquidar-la-mision-y-conciliar.md)
- La resolución de la cola de conflictos de sincronización — es [HU-068](HU-068-cola-de-conflictos-dos-versiones-lado-a-lado.md) y el resto de [CU-11](../casos-de-uso/CU-11-sincronizar-y-resolver-conflictos.md)

## Notas y pendientes

- `[C]` **¿Opera la institución rutas de lista abierta** con paradas donde suben y bajan personas? — insumo #40. Si es así, el volumen de novedades haría impracticable esta historia y habría que modelar conteo por punto de abordaje ([RN-53](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md), caso límite)
- `[C]` Quién puede autorizar la incorporación de una persona en ruta y por qué medio queda constancia cuando no hay señal. La propuesta es registrar nombre y puesto de quien lo ordenó, sin exigir su acuse — insumo #32 (convalidación de actos sin autorización previa) es el más cercano
- `[I]` Que el exceso de ocupación sobrevenido genere hallazgo bloqueante del cierre es derivación de [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) y [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)
