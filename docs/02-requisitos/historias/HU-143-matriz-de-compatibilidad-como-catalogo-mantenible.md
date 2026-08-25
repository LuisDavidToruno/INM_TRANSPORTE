# HU-143 — Mantener las matrices de compatibilidad como catálogo, donde la casilla vacía bloquea en lugar de permitir

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros |
| **Actor** | ACT-01 Administrador del Sistema (carga) · ACT-08 Gerencia Administrativa (aprueba) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — el contenido de la matriz depende del reglamento interno de la institución (insumo #1) y del régimen de carga peligrosa (insumo #38) |

## Historia

**Como** Administrador del Sistema
**quiero** mantener la matriz de compatibilidad **objeto × objeto** y la de **objeto × tipo de vehículo** como catálogo con vigencia, donde **la ausencia de entrada bloquea**
**para** que el sistema no permita trasladar personas junto a bidones de combustible solo porque nadie escribió nunca que estuviera prohibido

## Contexto

`RN-67` es explícita en el punto que define el diseño: *"Existe matriz de compatibilidad objeto × objeto, evaluada par a par; **la ausencia de entrada bloquea**."* Es la decisión correcta y también la incómoda: una matriz recién instalada, sin ninguna entrada, bloquea todo. Por eso es una historia de catálogo y no un detalle de la programación: **alguien tiene que poder cargarla y aprobarla el día de la implantación, sin desarrollo**.

El ejemplo que la propia `RN-20` usa —*personas junto a bidones de combustible*— no se podía expresar con la matriz vehículo × objeto y por eso nació `RN-67`. La combinatoria crece rápido: con 12 tipos de objeto son 66 pares, y cada uno necesita su decisión.

Y hay un matiz de `RN-68` que hay que respetar aquí: **la compatibilidad se evalúa por tramo**, sobre la configuración real de cada tramo. La matriz da la regla; el tramo da los datos.

## Reglas que la gobiernan

- [RN-67](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md) — Matriz objeto × objeto evaluada par a par; la ausencia de entrada bloquea
- [RN-68](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md) — Compatibilidad y capacidad se evalúan por tramo
- [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) — El tipo de vehículo debe ser compatible con el objeto declarado
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — La matriz es parámetro con vigencia y doble control, mantenible sin desarrollo
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La evaluación usa la matriz vigente a la fecha del hecho

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Es el caso que originó `RN-67` y `RN-68`
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — La extensión cambia la configuración de tramos y obliga a reevaluar

## Criterios de aceptación

```gherkin
# language: es
Característica: Matrices de compatibilidad como catálogo mantenible

  Antecedentes:
    Dado un catálogo "Tipos de objeto del traslado" con las entradas "Personal de la institución", "Personas externas", "Carga general" y "Combustible en recipiente" vigentes
    Y una matriz de compatibilidad objeto × objeto sin ninguna entrada cargada

  Escenario: La casilla vacía bloquea la solicitud, no la permite
    Cuando el Solicitante registra una solicitud con "Personal de la institución" y "Carga general" en el mismo tramo
    Entonces el sistema rechaza la solicitud
    Y muestra "No hay entrada de compatibilidad cargada para el par Personal de la institución × Carga general. La ausencia de entrada bloquea. Solicite a la Gerencia Administrativa que resuelva este par."
    Y no ofrece continuar de todos modos

  Escenario: Se rechaza una entrada de matriz sin resolución declarada
    Cuando el Administrador del Sistema carga el par "Personal de la institución × Carga general" sin declarar si es compatible, incompatible o compatible con condición
    Entonces el sistema rechaza la carga
    Y muestra "Declare la resolución del par: compatible, incompatible o compatible con condición. Una entrada sin resolución no resuelve nada."

  Escenario: Se rechaza una entrada compatible con condición sin la condición escrita
    Cuando el Administrador del Sistema carga el par "Personal de la institución × Carga general" como "compatible con condición" sin describir la condición
    Entonces el sistema rechaza la carga
    Y muestra "Describa la condición que debe cumplirse. El motorista tiene que poder leerla en la Orden de Misión impresa."

  Escenario: El par incompatible bloquea con el motivo cargado en el catálogo
    Dada una entrada "Personas externas × Combustible en recipiente" cargada como "incompatible" con motivo "riesgo de inflamable en habitáculo compartido" y aprobada
    Cuando el Solicitante registra una solicitud con ambos objetos en el mismo tramo
    Entonces el sistema rechaza la solicitud
    Y muestra "Personas externas y Combustible en recipiente son incompatibles en el mismo tramo: riesgo de inflamable en habitáculo compartido. Separe en dos tramos o en dos misiones."

  Escenario: El par compatible con condición deja la condición impresa en la Orden de Misión
    Dada una entrada "Personal de la institución × Carga general" como "compatible con condición" con condición "la carga debe ir asegurada y separada del habitáculo"
    Cuando el Jefe de Transporte emite la Orden de Misión
    Entonces la condición aparece en la Orden de Misión impresa
    Y aparece también en el paquete de misión que recibe el dispositivo del motorista

  Escenario: La matriz cargada no aplica hasta ser aprobada
    Cuando el Administrador del Sistema carga 66 pares en una importación
    Entonces los 66 quedan en estado "PENDIENTE DE APROBACIÓN"
    Y ninguno participa de ninguna evaluación
    Y el sistema muestra "66 pares cargados y pendientes de aprobación de la Gerencia Administrativa. Mientras tanto siguen bloqueando por ausencia de entrada vigente."

  Escenario: La carga masiva de implantación se aprueba por lote con su acta
    Dados 66 pares pendientes de aprobación
    Cuando la Gerencia Administrativa los aprueba por lote el "2026-09-25" con acta adjunta
    Entonces los 66 quedan vigentes desde su fecha de inicio
    Y el sistema emite el acta de aprobación por lote con folio, autor y el detalle de los 66 pares
    Y no exige aprobar par por par

  Escenario: La compatibilidad se evalúa por tramo y no por la misión completa
    Dada una misión con dos tramos, el primero con "Personas externas" y el segundo con "Combustible en recipiente"
    Cuando el Jefe de Transporte programa la misión
    Entonces el sistema acepta la programación
    Y muestra "Los objetos no coinciden en ningún tramo. La evaluación es por tramo, sobre la configuración real de cada uno."

  Escenario: La corrección de la matriz no revierte misiones ya programadas
    Dada una misión programada el "2026-10-05" con el par evaluado como compatible
    Cuando el par se corrige a "incompatible" el "2026-11-02"
    Entonces la misión programada conserva su evaluación y su referencia a la versión de matriz usada
    Y el sistema lista las misiones afectadas para que la Gerencia Administrativa decida caso por caso
```

## Fuera de alcance

- Los atributos del tipo de vehículo — es [HU-142](HU-142-tipos-de-vehiculo-con-atributos-de-compatibilidad.md)
- La verificación de compatibilidad al registrar la solicitud — es [HU-002](HU-002-bloqueo-de-compatibilidad-del-objeto-del-traslado.md)
- La verificación al asignar el vehículo — es [HU-022](HU-022-compatibilidad-vehiculo-objeto-del-traslado.md)
- Las capacidades de pasajeros y carga, que son de la ficha técnica y no de la matriz

## Notas y pendientes

- `[C]` **Contenido real de la matriz.** Los pares de los criterios son **datos de prueba**. La institución debe resolver los 66 pares antes de operar — insumo **#1**
- `[C]` **¿Moviliza la institución carga peligrosa o especializada, y bajo qué régimen?** Cambia los tipos de objeto y por tanto el tamaño de la matriz — insumo **#38**
- `[C]` **¿Realiza traslados de personas bajo custodia o de menores?** Agrega tipos de objeto con reglas propias — insumo **#39**
- `[I]` La aprobación **por lote con acta** en la carga masiva de implantación la propone [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) en sus casos límite: *"un control que obliga a mil aprobaciones el primer día se desactiva el segundo"*
