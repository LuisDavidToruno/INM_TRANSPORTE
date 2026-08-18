# HU-022 — Impedir la asignación de un vehículo incompatible con el objeto del traslado

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho |
| **Actor** | ACT-04 Jefe de Transporte · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md) paso 3 y E6 · `T-08` · `BD-07` |

## Historia

**Como** Jefe de Transporte
**quiero** que el sistema evalúe la compatibilidad entre el tipo de vehículo y el objeto del traslado declarado —personas, carga o ambos— tramo por tramo, y me impida asignar un vehículo que no da la capacidad
**para** que ningún servidor se quede en el predio porque el pickup no tenía plazas, y para que no se cargue una tonelada en un vehículo cuya ficha técnica admite media

## Contexto

El sistema no mueve "viajes de personas": mueve **movilizaciones de recursos institucionales**, y el tipo de vehículo es el eje de compatibilidad. Hoy la decisión la toma el encargado de memoria: sabe que en el microbús caben veintiséis y que en la paila del pickup entra "lo que se pueda". Cuando la misión lleva ocho servidores **y** cuatro bidones de combustible, la memoria no alcanza: se evalúan dos dimensiones a la vez, y además compatibilidad entre objetos que no deben viajar juntos.

La capacidad se evalúa **por tramo**, no sobre el total de la misión: en un multi-destino, la configuración real del vehículo cambia cuando bajan pasajeros o se entrega carga.

## Reglas que la gobiernan

- [RN-20](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md) — El tipo de vehículo asignado debe ser compatible con el objeto del traslado declarado
- [RN-21](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) — No se excede la capacidad de pasajeros ni la de carga de la ficha técnica
- [RN-67](../../01-negocio/reglas/RN-67-matriz-de-compatibilidad-objeto-objeto.md) — Matriz de compatibilidad objeto × objeto evaluada par a par; **la ausencia de entrada bloquea**
- [RN-68](../../01-negocio/reglas/RN-68-compatibilidad-y-capacidad-por-tramo.md) — Compatibilidad y capacidad se evalúan por tramo, sobre la configuración real de cada uno

## Casos especiales que la afectan

- [CE-18](../casos-especiales/CE-18-carga-y-pasajeros-en-la-misma-mision.md) — Carga y pasajeros en la misma misión, con requisitos que compiten
- [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — Cuando el único vehículo compatible ya está tomado

## Criterios de aceptación

```gherkin
# language: es
Característica: Compatibilidad y capacidad del vehículo frente al objeto del traslado

  Antecedentes:
    Dado un vehículo "Pickup Toyota Hilux" con correlativo institucional "INS-P-014",
      tipo "Pickup", capacidad de "5" plazas y capacidad de carga de "1000" kg
    Y un vehículo "Microbús Toyota Coaster" con correlativo institucional "INS-B-003",
      tipo "Microbús", capacidad de "26" plazas y capacidad de carga de "300" kg
    Y una matriz de compatibilidad objeto × objeto vigente al "2026-09-10"

  Escenario: Se rechaza por plazas insuficientes
    Dada una solicitud "SOL-2026-0350" con objeto del traslado "8 servidores de la institución"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-014" a esa solicitud
    Entonces el sistema rechaza la asignación
    Y muestra "El vehículo INS-P-014 tiene 5 plazas y el traslado declara 8 ocupantes. Requiere un vehículo de al menos 8 plazas."
    Y propone los vehículos compatibles y libres en la ventana

  Escenario: Se rechaza por capacidad de carga excedida
    Dada una solicitud "SOL-2026-0351" con objeto del traslado "carga: 1200 kg de mobiliario de oficina"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-014" a esa solicitud
    Entonces el sistema rechaza la asignación
    Y muestra "El vehículo INS-P-014 admite 1,000 kg de carga y el traslado declara 1,200 kg."

  Escenario: Se rechaza el traslado mixto por incompatibilidad entre objetos
    Dada una solicitud "SOL-2026-0352" con objeto del traslado "4 servidores de la institución
      y 6 bidones de combustible de 5 galones"
    Y una entrada de la matriz que declara "personas" y "combustible envasado" como incompatibles
      en compartimiento común
    Cuando el Jefe de Transporte intenta asignar el "INS-B-003" a esa solicitud
    Entonces el sistema rechaza la asignación
    Y muestra "Personas y combustible envasado no pueden viajar en compartimiento común. Use un vehículo con paila separada o divida la misión."

  Escenario: Se rechaza cuando la matriz no tiene entrada para el par declarado
    Dada una solicitud "SOL-2026-0353" con objeto del traslado "2 servidores y 1 equipo de rayos X"
    Y que la matriz no tiene entrada para el par "personas" × "equipo médico especializado"
    Cuando el Jefe de Transporte intenta asignar el "INS-P-014" a esa solicitud
    Entonces el sistema rechaza la asignación
    Y muestra "No existe criterio registrado para trasladar personas junto con equipo médico especializado. Solicite a Catálogos Maestros la entrada de la matriz antes de programar."

  Escenario: Se rechaza por capacidad excedida en un tramo intermedio
    Dada una solicitud "SOL-2026-0354" de tres tramos:
      | tramo | origen        | destino       | ocupantes | carga_kg |
      | 1     | Tegucigalpa   | Comayagua     | 4         | 600      |
      | 2     | Comayagua     | Siguatepeque  | 6         | 600      |
      | 3     | Siguatepeque  | Tegucigalpa   | 6         | 0        |
    Cuando el Jefe de Transporte intenta asignar el "INS-P-014" a esa solicitud
    Entonces el sistema rechaza la asignación
    Y muestra "En el tramo 2 (Comayagua–Siguatepeque) el traslado declara 6 ocupantes y el vehículo INS-P-014 tiene 5 plazas."

  Escenario: Se acepta la asignación compatible en todos los tramos
    Dada una solicitud "SOL-2026-0355" con objeto del traslado "3 servidores y 400 kg de insumos"
      en un solo tramo
    Cuando el Jefe de Transporte asigna el "INS-P-014" a esa solicitud
    Entonces el sistema acepta la asignación
    Y registra el resultado de la evaluación con las plazas y los kilogramos comparados por tramo
```

## Fuera de alcance

- La declaración del objeto del traslado en la solicitud — es de M-06
- El mantenimiento de la matriz de compatibilidad y de los tipos de vehículo y de carga — es de M-02
- El peso **efectivo** medido en báscula y su desviación respecto a lo declarado — se registra en la ejecución (M-08)
- La habilitación del motorista para ese vehículo — es [HU-025](HU-025-habilitacion-de-quien-efectivamente-conduce.md)

## Notas y pendientes

- `[C]` **Qué tipos de carga exigen peso cierto y cuáles admiten estimación por rango** — insumo #63. Mientras no se resuelva, el sistema exige peso declarado y lo marca como *estimado* si el solicitante lo declara así.
- `[C]` Si la institución moviliza **carga peligrosa o especializada** y bajo qué régimen — insumo #38.
- `[C]` Si se admite **más de un vehículo simultáneo bajo una misma Orden de Misión** (convoy) — insumo #62. Hoy la salida ante capacidad insuficiente es dividir la misión.
