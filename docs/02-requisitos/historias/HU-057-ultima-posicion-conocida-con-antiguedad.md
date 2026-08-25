# HU-057 — Mostrar la última posición conocida con su antigüedad, nunca como si fuera actual

| Campo | Valor |
|---|---|
| **Módulo** | M-19 Seguimiento en Ruta |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta la frecuencia de reporte de posición aceptable y quién asume el consumo de datos que genera (insumo #74), y el umbral de antigüedad a partir del cual el tablero degrada explícitamente el dato (insumo #68) |

## Historia

**Como** Jefe de Transporte
**quiero** ver en el tablero la última posición y el último estado declarado de cada vehículo, siempre con la antigüedad del dato a la vista
**para** decidir con información honesta y no dar por perdida una unidad que simplemente lleva ocho horas sin cobertura

## Contexto

Un tablero que muestra una posición de hace once horas como si fuera de ahora es peor que un tablero vacío: produce decisiones seguras sobre información falsa. En Honduras, con la cobertura que hay, el silencio de un vehículo es **la condición esperada**, no una anomalía.

**El silencio no es un estado.** No dispara ninguna transición automática, no cambia el estado de la misión y no permite inferir nada: ni que el vehículo se detuvo, ni que hubo un incidente, ni que el motorista no está reportando ([RN-76](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md)).

El seguimiento se alimenta **oportunistamente**: cuando hay señal, sube; cuando no la hay, no pasa nada. Su ausencia no es una anomalía ([RNF-08](../no-funcionales/RNF-08-seguimiento-en-ruta.md)).

## Reglas que la gobiernan

- [RN-76](../../01-negocio/reglas/RN-76-estado-en-ruta-declarado-por-el-motorista.md) — **Regla rectora**: el estado en ruta lo declara el motorista y nunca se infiere del silencio
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — La antigüedad se calcula sobre la hora del hecho, no sobre la de recepción
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Superado el umbral, el sistema degrada explícitamente antes de que alguien opere sobre el dato

## Casos especiales que la afectan

- [CE-08](../casos-especiales/CE-08-multi-destino-con-esperas-prolongadas-en-sitio.md) — El vehículo detenido horas en un destino, que no es una anomalía
- [CE-02](../casos-especiales/CE-02-averia-mecanica-en-ruta.md) — La avería que sí es una anomalía, y que solo se sabe porque el motorista la declaró

## Criterios de aceptación

```gherkin
# language: es
Característica: Seguimiento en ruta con antigüedad visible del dato

  Antecedentes:
    Dada la Orden de Misión "OM-2026-0451" en estado "EN_RUTA" con el vehículo "Pickup Hilux"
    Y una última posición recibida el "2026-05-14" a las "07:20" cerca de "Jícaro Galán"
    Y un último estado declarado por el motorista de "en ruta al destino" a las "07:18"

  Escenario: Se rechaza presentar la posición sin su antigüedad
    Cuando el Jefe de Transporte consulta el tablero de seguimiento a las "18:00" del "2026-05-14"
    Entonces el sistema muestra "Última posición conocida: cerca de Jícaro Galán, hace 10 horas 40 minutos."
    Y no presenta la posición como actual
    Y no muestra ningún indicador de "en línea" ni de "seguimiento activo"

  Escenario: El silencio no cambia el estado de la misión
    Dado que no se recibe ninguna posición desde hace "3" días
    Cuando el sistema evalúa la Orden de Misión "OM-2026-0451"
    Entonces la misión permanece en estado "EN_RUTA"
    Y el sistema no la marca como interrumpida, ni como incidente, ni la cierra por inactividad

  Escenario: El tablero declara explícitamente que no sabe nada
    Dado que no se recibe ninguna posición desde hace "3" días
    Cuando el Jefe de Transporte consulta el tablero de seguimiento
    Entonces el sistema muestra "Sin datos nuevos desde el 11/05/2026 a las 07:20. La zona no tiene cobertura; esto es esperable."
    Y muestra el último estado declarado por el motorista con su hora

  Escenario: El estado mostrado es el declarado, no el inferido
    Dado que el vehículo lleva "5" horas sin cambiar de posición
    Cuando el Jefe de Transporte consulta el estado de "OM-2026-0451"
    Entonces el sistema muestra el estado "esperando en sitio, declarado por el motorista a las 09:15"
    Y no muestra ningún estado calculado a partir de la falta de movimiento

  Escenario: Llegan de golpe las posiciones acumuladas al reconectar
    Dado que el dispositivo estuvo 4 días sin conectividad y acumuló "27" reportes de posición
    Cuando aparece señal y el dispositivo sincroniza
    Entonces el tablero ordena los reportes por la hora del hecho, no por la de recepción
    Y muestra el recorrido con la antigüedad de cada punto
```

## Fuera de alcance

- El componente de mapas: lo aporta ARGOS y SIGTI no lo reimplementa ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md))
- La frecuencia de reporte de posición y su consumo de datos — depende del insumo #74
- La alerta automática por vehículo fuera de ruta: no se diseña mientras el silencio sea la condición normal

## Notas y pendientes

- `[C]` Frecuencia de reporte de posición aceptable y quién asume el consumo de datos que genera — insumo #74
- `[C]` Umbral de antigüedad a partir del cual el tablero degrada explícitamente el dato — insumo #68
- `[I]` Que el tablero no infiera nada del silencio es deducción del equipo a partir de la realidad de cobertura descrita en [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md), no una exigencia normativa literal
