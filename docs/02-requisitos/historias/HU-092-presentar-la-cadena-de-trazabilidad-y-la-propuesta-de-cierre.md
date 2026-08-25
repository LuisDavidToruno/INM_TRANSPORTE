# HU-092 — Presentar la cadena de trazabilidad eslabón por eslabón y proponer la clasificación de cierre

| Campo | Valor |
|---|---|
| **Módulo** | M-13 Liquidación y Cierre · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-04 Jefe de Transporte |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta definir con Auditoría Interna qué eslabones son exigibles para el cierre y cuál es el mínimo no desactivable (insumo #1): eso **es** la lógica de la propuesta de cierre, no un parámetro. Faltan también el plazo de liquidación en días hábiles (insumo #32) y el `plazo_convalidacion` de actos sin autorización previa (insumo #1) |

## Historia

**Como** Jefe de Transporte
**quiero** ver la cadena de trazabilidad de la misión eslabón por eslabón, con cada uno marcado presente, ausente o no aplicable con fundamento, y la propuesta de clasificación de cierre con los datos que la dispararon
**para** entregar un descargo que se sostiene solo ante Auditoría Interna, sin tener que reconstruirlo a mano cada vez que alguien pregunta

## Contexto

La cadena es la que responde la pregunta del auditor:

```
solicitud → autorización → orden de misión → asignación de vehículo y motorista →
bitácora con odómetro de salida y retorno → asignación y consumo de combustible →
registro de peajes → liquidación
```

Dos distinciones deciden si esto sirve. Primera: **ausente no es lo mismo que pendiente de sincronización**. Una misión larga desconectada durante días es lo normal, no una anomalía, y no debe producir hallazgo por falta de datos que aún vienen en camino. Segunda: **la propuesta no cierra nada**. El sistema evalúa y propone; cerrar es acto de Gerencia Administrativa.

## Reglas que la gobiernan

- [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) — Lista de verificación eslabón por eslabón, con *no aplicable* fundamentado
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Se distingue *ausente* de *pendiente de sincronización*
- [RN-78](../../01-negocio/reglas/RN-78-grado-de-cumplimiento-del-objeto.md) — El grado de cumplimiento del objeto se declara por destino y consolidado
- [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) — El resultado económico se congela con los identificadores de las tablas paramétricas usadas
- [RN-73](../../01-negocio/reglas/RN-73-convalidacion-de-actos-sin-autorizacion-previa.md) — Los actos sin autorización previa se convalidan en plazo, con cronología real
- [RN-55](../../01-negocio/reglas/RN-55-habilitacion-vencida-durante-la-mision.md) — Un bloqueo duro que falló al revalidarse abre hallazgo automático
- [RN-81](../../01-negocio/reglas/RN-81-sigti-expone-hechos-a-argos.md) — El viático se muestra por su clave de vínculo; SIGTI no lo calcula ni lo espera

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Bitácora en papel pendiente de digitar
- [CE-11](../casos-especiales/CE-11-licencia-vence-durante-la-mision.md) — Bloqueo duro que falló al revalidarse
- [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) — Actos sin autorización previa
- [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) — Prórroga sin código de autorización

## Criterios de aceptación

```gherkin
# language: es
Característica: Cadena de trazabilidad y propuesta de clasificación de cierre

  Antecedentes:
    Dado una Orden de Misión "OM-2026-0512" en estado "RETORNADA"
    Y las tres conciliaciones calculadas: combustible, peajes, y kilometraje y tiempos

  Escenario: Se distingue el eslabón ausente del pendiente de sincronización
    Dado que la bitácora se llenó en papel y aún no se ha digitado
    Y que un dispositivo de la misión tiene 12 eventos pendientes de sincronizar
    Cuando el Jefe de Transporte abre el descargo conciliado de "OM-2026-0512"
    Entonces el eslabón "bitácora con odómetro de salida y retorno" se muestra como "pendiente de sincronización", no como "ausente"
    Y el sistema no genera hallazgo por ese eslabón
    Y muestra "12 eventos pendientes de sincronizar del dispositivo asignado a Wilmer Cáceres."

  Escenario: Se rechaza marcar un eslabón como no aplicable sin fundamento
    Cuando el Jefe de Transporte marca el eslabón "registro de peajes" como "no aplicable" sin fundamento escrito
    Entonces el sistema rechaza la marca
    Y muestra "No aplicable exige fundamento escrito. Ejemplo válido: la ruta autorizada no atraviesa ningún punto de peaje."

  Escenario: Se muestran los datos concretos que dispararon cada criterio
    Dado una desviación de rendimiento del "50.0" por ciento sin justificación aceptada
    Cuando el Jefe de Transporte abre la propuesta de clasificación
    Entonces el sistema muestra el criterio "H-01" como cumplido
    Y muestra "H-01: rendimiento observado 6.00 km/galón contra 12.00 esperado, desviación 50.0 % por debajo, sin justificación aceptada."
    Y muestra los criterios no cumplidos con el dato que los descarta

  Escenario: La propuesta no cierra la misión
    Cuando el Jefe de Transporte confirma la liquidación de "OM-2026-0512"
    Entonces la misión pasa a estado "LIQUIDADA"
    Y la propuesta de clasificación queda en la bandeja de Gerencia Administrativa
    Y la misión no queda cerrada

  Escenario: Se rechaza liquidar sin declarar el grado de cumplimiento del objeto
    Cuando el Jefe de Transporte intenta liquidar sin declarar el grado de cumplimiento por destino
    Entonces el sistema rechaza la liquidación
    Y muestra "Declare el grado de cumplimiento del objeto por destino y consolidado. Una misión puede cuadrar en dinero y no haber cumplido su objeto."

  Escenario: El resultado económico se congela con las tablas usadas
    Cuando la misión pasa a "LIQUIDADA"
    Entonces el sistema congela kilómetros, rendimiento real, costo de combustible, costo de peajes, desviación contra estimado y tiempos de espera
    Y conserva los identificadores de las tablas paramétricas usadas para cada cálculo

  Escenario: Un acto sin autorización previa se convalida con cronología real
    Dado una prórroga registrada en ruta el "2026-09-26" sin código de autorización
    Y un parámetro "plazo_convalidacion" de "3" días hábiles
    Cuando el Jefe de Transporte convalida la prórroga el "2026-09-29"
    Entonces el sistema registra la convalidación con fecha del acto original "26/09/2026" y fecha de la convalidación "29/09/2026"
    Y no genera ninguna autorización con fecha retroactiva

  Escenario: Sin convalidación en plazo el eslabón queda ausente y no subsanable
    Dado una prórroga sin convalidar al vencer el plazo
    Cuando el Jefe de Transporte abre el descargo conciliado
    Entonces el eslabón "autorización" figura como "ausente y no subsanable"
    Y la propuesta de clasificación incluye el criterio correspondiente

  Escenario: Un bloqueo duro que falló al revalidarse abre hallazgo sin revertir el hecho
    Dado que la licencia de "Wilmer Cáceres" venció el "2026-09-25", con la misión ya en ruta
    Cuando el servidor revalida al sincronizar
    Entonces el sistema no revierte ningún hecho de la misión
    Y abre hallazgo automático de tipo "H-07"
    Y notifica al Jefe de Transporte y al Auditor Interno
    Y muestra "La licencia de Wilmer Cáceres venció el 25/09/2026 con la misión en ruta. El vehículo ya salió: el hecho no se revierte, se registra."

  Escenario: El viático se muestra por clave de vínculo y no detiene la liquidación
    Dado un viático asociado gestionado en ARGOS, en estado "en trámite"
    Cuando el Jefe de Transporte liquida "OM-2026-0512"
    Entonces el sistema muestra el estado del viático por su clave de vínculo
    Y no calcula, no liquida y no espera el viático para continuar
```

## Fuera de alcance

- El cálculo de cada conciliación — es [HU-088](HU-088-conciliar-galonaje-contra-kilometraje.md), [HU-089](HU-089-conciliar-el-fondo-y-tipificar-sobrante-y-faltante.md) y [HU-090](HU-090-conciliar-peajes-punto-por-punto.md)
- El bloqueo por segregación — es [HU-091](HU-091-bloquear-la-liquidacion-por-segregacion-de-funciones.md)
- El acto de cerrar — es [HU-093](HU-093-cerrar-la-mision-con-la-cadena-completa.md) y [HU-094](HU-094-cerrar-con-hallazgo-tipificado.md)
- **Los viáticos**: los gestiona ARGOS. SIGTI conserva la clave de vínculo y muestra el estado ([DP-001 D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))

## Notas y pendientes

- `[I]` La cadena de eslabones exigida para el cierre es **implicación de requerimiento escrita por el equipo** a partir de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), **no articulado citable**. Corregido desde `[V]` por el hallazgo `HN1-06`. **No se eleva el nivel**
- `[C]` Plazo de liquidación en días hábiles, desde el retorno — insumo **#32**
- `[C]` `plazo_convalidacion` de actos sin autorización previa — insumo **#1**
- `[C]` `eslabones_exigidos_para_cierre`: qué eslabones son exigibles y cuál es el mínimo no desactivable — insumo **#1**, con Auditoría Interna
