# HU-150 — Poner en marcha una institución desde el mismo artefacto, con los parámetros que nadie ha confirmado vacíos y bloqueantes

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros · M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — es el requisito que hace del producto un producto; su verificación completa depende del Sprint 2 |

## Historia

**Como** Administrador del Sistema de una institución que instala SIGTI por primera vez
**quiero** configurar el nombre, las siglas, el escudo, el formato del correlativo vehicular, la leyenda de rotulación, el horario hábil y el calendario, sin tocar una línea de código
**para** que la institución opere con su propia identidad desde el primer día, y para que ningún parámetro que nadie confirmó se aplique con un valor inventado

## Contexto

`RNF-19` lo define y lo mide: *"la prueba de que se cumplió no es una declaración de intención: es poner en marcha una segunda institución con el mismo artefacto de despliegue y ver si funciona."* Cero apariciones del nombre de la institución piloto en el código, cero reglas condicionadas a una institución, **cero diferencia entre el artefacto de despliegue de dos instituciones**.

Y la parte incómoda, que es la que de verdad protege: **el valor por defecto inventado es el enemigo.** Un catálogo que se instala con una tarifa de ejemplo, un feriado supuesto o un plazo "razonable" produce el peor resultado posible: *"la institución opera meses sobre valores que nadie confirmó, y esos valores terminan en documentos oficiales y en descargos ante el TSC."*

Por eso: **todo parámetro cuyo valor real esté `[C]` se instala vacío**, y el sistema bloquea la operación que lo necesita diciendo **qué parámetro falta y quién debe proveerlo** — no lo estima.

Hoy están en esa condición, entre otros: tarifas de peaje (#21), exoneraciones (#22), feriados de octubre, matriz licencia↔vehículo (#20), horario hábil y plazos (#32), umbrales de desviación de consumo (#32), plazos de retención (#71).

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El sistema **debe poder arrancar sin los parámetros no confirmados**, bloqueando solo las operaciones que los requieren
- [RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) — La identidad del vehículo es el correlativo institucional, cuyo formato es configurable
- [RN-18](../../01-negocio/reglas/RN-18-rotulacion-del-vehiculo-del-estado.md) — La leyenda y las siglas de la rotulación son dato de la institución
- [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) — Los documentos oficiales llevan la identidad institucional, que no se cablea
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — Un parámetro vacío bloquea; no se extrapola ni se estima

## Casos especiales que la afectan

- [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) — El correlativo institucional es la identidad cuando no hay lámina, y su formato es configurable
- **Caso especial candidato:** *la institución quiere empezar a operar antes de tener las tarifas de peaje* — hoy la respuesta es que puede operar todo menos lo que necesite peajes, y conviene documentarlo

## Criterios de aceptación

```gherkin
# language: es
Característica: Puesta en marcha de una institución con parámetros vacíos y bloqueantes

  Antecedentes:
    Dada una instancia recién instalada desde el artefacto de despliegue
    Y ningún parámetro cargado
    Y ningún dato de identidad institucional configurado

  Escenario: Se bloquea la estimación de peajes con el catálogo de tarifas vacío
    Dada la identidad institucional ya configurada
    Cuando el Solicitante registra una solicitud con destino que atraviesa el punto "Zambrano"
    Entonces el sistema bloquea la estimación de peajes
    Y muestra "No hay tarifas de peaje cargadas para el punto Zambrano. El parámetro tarifa_peaje debe cargarlo el Administrador del Sistema y aprobarlo la Gerencia Administrativa. Insumo pendiente #21."
    Y no calcula cero ni usa ningún valor de ejemplo

  Escenario: Se bloquea la asignación de motorista con la matriz de licencias vacía
    Cuando el Jefe de Transporte intenta asignar un motorista a un vehículo
    Entonces el sistema bloquea la asignación
    Y muestra "No hay matriz licencia↔vehículo cargada. Sin ella RN-09 no puede verificar la habilitación y el control quedaría inoperante. Insumo pendiente #20."

  Escenario: Se bloquea la clasificación de día inhábil con el calendario vacío
    Cuando el Jefe de Transporte programa una misión para el "2026-10-03"
    Entonces el sistema bloquea la programación
    Y muestra "No hay calendario de días hábiles y feriados cargado. No se puede determinar si el 03/10/2026 requiere permiso de circulación de la máxima autoridad."

  Escenario: Se rechaza emitir un documento oficial sin identidad institucional configurada
    Cuando el Jefe de Transporte intenta emitir una Orden de Misión
    Entonces el sistema rechaza la emisión
    Y muestra "Configure la identidad institucional —nombre, siglas y pie de documento— antes de emitir documentos oficiales."

  Escenario: Se rechaza dar de alta un vehículo sin formato de correlativo configurado
    Cuando el Jefe de Transporte da de alta un vehículo
    Entonces el sistema rechaza el alta
    Y muestra "Configure el formato del correlativo institucional del vehículo. Es la identidad del bien y no puede improvisarse por vehículo."

  Escenario: La identidad institucional configurada se refleja en todos los documentos
    Cuando el Administrador del Sistema configura nombre, siglas, escudo, pie de documento y leyenda de rotulación
    Entonces los documentos oficiales emitidos muestran esa identidad
    Y al cambiar las siglas y reimprimir, el cambio aparece en el 100 % de los formatos

  Escenario: Lo que no depende de un parámetro faltante sí opera
    Dada la identidad institucional configurada y el catálogo de motivos de viaje aprobado
    Y las tarifas de peaje sin cargar
    Cuando el Solicitante registra una solicitud con destino sin puntos de peaje
    Entonces el sistema acepta la solicitud
    Y el bloqueo de peajes no alcanza a esta misión

  Escenario: Una segunda institución se instala con el mismo artefacto y otra configuración
    Dada una segunda instancia instalada desde el mismo artefacto de despliegue
    Cuando se cargan otro nombre, otras siglas, otras dependencias, otros tipos de vehículo y otro horario hábil
    Entonces el guion completo de una misión se ejecuta sin ninguna modificación de código
    Y ninguna consulta, reporte ni exportación de una instancia alcanza datos de la otra

  Escenario: El horario hábil admite un valor distinto por delegación
    Dado un horario hábil de institución del "08:00" al "16:00"
    Cuando el Administrador del Sistema carga para la "Delegación de Choluteca" un horario del "07:00" al "15:00"
    Entonces la resolución busca del ámbito más específico al más general
    Y las misiones de esa delegación se clasifican con su propio horario
```

## Fuera de alcance

- La carga de cada parámetro concreto — es [HU-144](HU-144-cargar-parametro-normativo-con-vigencia.md)
- La estructura institucional y sus dependencias — es [HU-126](HU-126-estructura-institucional-con-vigencia.md)
- El respaldo y la restauración de la instancia — es [`RNF-09`](../no-funcionales/RNF-09-instalacion-respaldo-y-restauracion.md)
- El artefacto de despliegue y su tecnología: `ADR-000` difiere el stack al Sprint 2; aquí se describe comportamiento observable

## Notas y pendientes

- `[C]` Tarifas de peaje (**#21**), exoneraciones (**#22**), matriz licencia↔vehículo (**#20**), horario hábil, plazos y umbrales (**#32**), plazos de retención (**#71**), feriados de octubre ([NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md)). **Ninguno se carga con valor de ejemplo**
- `[C]` **Correlativo institucional del vehículo: ¿único por institución o compuesto por delegación?** — insumo **#34**
- `[C]` **Horario hábil oficial** de la institución y si difiere por delegación — insumo **#32**
- `[C]` Tiempo de puesta en marcha de una segunda institución: `RNF-19` propone ≤ 1 jornada marcado `[C]`
- `[V]` Que la rotulación del vehículo del Estado lleva franjas, leyenda, siglas y correlativo — [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md). `[C]` **cómo se rotula una motocicleta** — insumo **#43**
