# HU-009 — Ver la bandeja de solicitudes con todas las validaciones a la vista antes de decidir

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-03 Jefatura Inmediata |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Jefatura Inmediata
**quiero** abrir mi bandeja de solicitudes pendientes y ver, antes de pronunciarme, el objeto del traslado con su detalle, los tramos inhábiles, el estimado de peajes desglosado, las misiones sin liquidar del solicitante y la antigüedad de la estructura con que se resolvió mi competencia
**para** autorizar con el mismo conocimiento que tendría si el expediente completo estuviera sobre mi escritorio, y no firmar a ciegas un total que no puedo verificar

## Contexto

En papel, la jefatura firma lo que le ponen enfrente y rara vez tiene a mano el histórico del solicitante. El sistema puede hacer algo que el papel no: **poner el control antes de la firma en vez de después**.

Tres datos cambian decisiones y hoy nadie los ve en el momento de firmar: el **estimado de peajes desglosado** —que es gasto que se compromete—, las **misiones anteriores sin liquidar** del mismo solicitante, y la **antigüedad del espejo de ARGOS** con que se resolvió quién es competente para autorizar.

Una advertencia que nadie ve no es un control. Por eso, cuando la institución configura el comportamiento como advertencia y la jefatura continúa igual, **la advertencia y el nombre de quien continuó quedan visibles en el expediente**.

## Reglas que la gobiernan

- [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) — El estimado desglosado por punto se pone a la vista de quien autoriza
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — Se declara la antigüedad del espejo de ARGOS antes de permitir continuar
- [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) — Cada entidad espejada muestra su última sincronización
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — Los tramos inhábiles se muestran señalados; la aprobación **no** se bloquea por ellos
- [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) — Las misiones sin liquidar del solicitante son dato de control, con su antigüedad
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — Si el expediente incluye personas externas, la consulta de la jefatura queda registrada

## Casos especiales que la afectan

- Ninguno se materializa aquí. Constancia dejada: [CE-12](../casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) y [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) quedan **descartados explícitamente**: aprobar no reserva flota ni fondo (`INV-11`), y ambos se materializan al programar

## Criterios de aceptación

```gherkin
# language: es
Característica: Bandeja de autorización con el contexto completo del expediente
  Como Jefatura Inmediata
  quiero ver todas las validaciones antes de pronunciarme
  para no autorizar lo que no puedo verificar

  Antecedentes:
    Dada una Jefatura Inmediata "Rolando Discua" con competencia sobre la dependencia "Subgerencia de Operaciones"
    Y un expediente "CHO-2026-00087" en estado "SOLICITADA" con salida prevista el "2026-03-20 07:00"
    Y una fecha del sistema del "2026-03-14 08:00"

  Escenario: Se bloquea la autorización con el espejo de la jerarquía detenido más allá del umbral
    Dado un espejo de la estructura de ARGOS sincronizado por última vez el "2026-03-10 06:00"
    Y un umbral de bloqueo de "72" horas
    Cuando "Rolando Discua" abre el expediente "CHO-2026-00087"
    Entonces el sistema no ofrece la acción de autorizar
    Y muestra "La estructura de autorización lleva 98 horas sin sincronizar y el umbral de bloqueo es de 72. No se autoriza contra una jerarquía que puede ya no existir (RN-50)."

  Escenario: Se advierte la antigüedad del espejo bajo el umbral de bloqueo y queda asentada
    Dado un espejo de la estructura de ARGOS sincronizado por última vez el "2026-03-12 06:00"
    Y un umbral de advertencia de "24" horas
    Cuando "Rolando Discua" abre el expediente "CHO-2026-00087"
    Entonces el sistema muestra "Su competencia se resolvió con una estructura de 50 horas de antigüedad."
    Y registra la advertencia en el diario del expediente antes de permitir continuar

  Escenario: Se muestran las misiones anteriores sin liquidar del solicitante
    Dado un solicitante de derecho "Marvin Cálix" con la misión "OM-2026-0331" en estado "RETORNADA" sin liquidar desde el "2026-02-02"
    Cuando "Rolando Discua" abre el expediente "CHO-2026-00087"
    Entonces el sistema muestra "Marvin Cálix tiene 1 misión sin liquidar: OM-2026-0331, 41 días de antigüedad."

  Escenario: Configurado como bloqueo, la misión sin liquidar impide autorizar
    Dado un parámetro institucional de misiones sin liquidar configurado como "bloqueo"
    Y un solicitante de derecho con la misión "OM-2026-0331" sin liquidar
    Cuando "Rolando Discua" intenta autorizar el expediente "CHO-2026-00087"
    Entonces el sistema no ejecuta la autorización
    Y muestra "Marvin Cálix tiene la misión OM-2026-0331 sin liquidar desde el 02/02/2026. La institución configuró este control como bloqueo."

  Escenario: Configurado como advertencia, continuar deja constancia con nombre
    Dado un parámetro institucional de misiones sin liquidar configurado como "advertencia"
    Y un solicitante de derecho con la misión "OM-2026-0331" sin liquidar
    Cuando "Rolando Discua" autoriza el expediente "CHO-2026-00087"
    Entonces el sistema ejecuta la autorización
    Y el expediente muestra "Autorizado por Rolando Discua con advertencia vigente: 1 misión sin liquidar del solicitante."
    Y esa constancia es visible en el expediente y en su versión impresa

  Escenario: El estimado de peajes se presenta desglosado, nunca como total opaco
    Dado un estimado congelado de "L 150.00" con 4 pasos por punto de peaje
    Cuando "Rolando Discua" abre el expediente "CHO-2026-00087"
    Entonces el sistema muestra las 4 líneas con punto, fecha prevista de paso, categoría y tarifa
    Y muestra el identificador de la tabla de tarifas usada

  Escenario: La bandeja ordena por salida más próxima y señala las urgentes
    Dados los expedientes "CHO-2026-00087" con salida el "2026-03-20 07:00", "CHO-2026-00090" con salida el "2026-03-15 06:00" marcado urgente y "CHO-2026-00091" con salida el "2026-03-16 08:00"
    Cuando "Rolando Discua" abre su bandeja de pendientes
    Entonces el orden es "CHO-2026-00090", "CHO-2026-00091", "CHO-2026-00087"
    Y "CHO-2026-00090" aparece señalado como urgente y con salida dentro de las próximas 24 horas
```

## Fuera de alcance

- El acto de autorizar, rechazar o devolver — son [HU-011](HU-011-registro-inmutable-de-la-autorizacion.md), [HU-014](HU-014-rechazo-con-motivo-y-solicitud-vinculada.md) y [HU-013](HU-013-devolucion-para-correccion-con-versionado.md)
- La decisión sobre **vehículo y motorista**: no es de la jefatura. Es del Jefe de Transporte en [CU-04](../casos-de-uso/CU-04-programar-mision-asignar-vehiculo-y-motorista.md)
- La disponibilidad de flota que se muestra en pantalla es **orientativa y no reserva nada** (`INV-11`)
- El detalle nominal de personas externas: se muestra bajo control de acceso y con registro de consulta, según las historias de M-17

## Notas y pendientes

- `[C]` Si las misiones sin liquidar **bloquean o advierten**, y el plazo máximo de liquidación — insumos #1 y #32
- `[C]` Umbrales de advertencia y de bloqueo por sincronización detenida del espejo — insumo #16
- `[C]` Antelación mínima que marca una solicitud como urgente y nivel adicional que su autorización exige — insumo #32
- Trazabilidad: [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) pasos 1 a 4 y excepción E5; puntos de control `PC-15`, `PC-02`
