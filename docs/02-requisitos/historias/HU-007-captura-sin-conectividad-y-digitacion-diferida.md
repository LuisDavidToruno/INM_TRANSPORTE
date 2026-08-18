# HU-007 — Capturar y enviar la solicitud sin conectividad, y digitarla desde papel

| Campo | Valor |
|---|---|
| **Módulo** | M-16 Sincronización y Operación Desconectada (con M-06 Solicitudes de Transporte) |
| **Actor** | ACT-10 Encargado de Delegación |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** Encargado de Delegación
**quiero** capturar y enviar una solicitud de transporte sin ninguna conectividad, y digitar después las que se llenaron en el formato en papel
**para** que la delegación no quede paralizada por falta de señal ni tenga que operar por fuera del sistema, que es como hoy se pierde el rastro de las misiones

## Contexto

Más de dos millones de personas del área rural hondureña no tienen acceso a internet `[V]` [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md). Una delegación que no puede registrar una solicitud sin red **vuelve al papel**, y con el papel se pierde todo lo demás.

Offline-first no es "con soporte offline": el identificador se genera en el cliente, el correlativo sale del **rango de la delegación**, y los estimados se calculan con la tabla sincronizada localmente **declarando su antigüedad**.

Y hay un caso que no se puede negar: no había dispositivo, la solicitud se llenó a mano y alguien la digita tres días después. Eso no es una irregularidad — es la operación real. Lo que sí es exigible es que quede constancia de **quién digitó, cuándo, y con el original escaneado adjunto**, y que la **fecha del hecho no se confunda con la fecha de captura**.

## Reglas que la gobiernan

- [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) — Toda captura de campo se completa sin conectividad y nunca se pierde
- [RN-44](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) — Identificadores en el cliente; folios de rangos por delegación
- [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) — Fecha del hecho y fecha de captura son campos distintos, ambos obligatorios
- [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) — La digitación diferida deja constancia de quién digitó y del original escaneado
- [RN-45](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — Ningún conflicto de sincronización se resuelve por sobrescritura
- [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) — La antigüedad de las tablas locales se declara antes de mostrar cualquier estimado

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — Zona sin señal: registro en papel, digitado días después

## Criterios de aceptación

```gherkin
# language: es
Característica: Captura de solicitudes sin conectividad y digitación diferida
  Como Encargado de Delegación
  quiero registrar solicitudes sin red y digitar las de papel
  para que la delegación no opere por fuera del sistema

  Antecedentes:
    Dada una delegación "Puerto Lempira" con rango de correlativos "PLE-2026-00040" a "PLE-2026-00060" disponible localmente
    Y un dispositivo sin ninguna conectividad
    Y una tabla local de tarifas de peaje sincronizada el "2026-02-01"
    Y un umbral de antigüedad de tablas de "30" días

  Escenario: Se rechaza el envío diferido sin fecha del hecho
    Dado un formato en papel de solicitud llenado el "2026-03-10"
    Cuando el Encargado de Delegación digita la solicitud el "2026-03-14" sin declarar la fecha del hecho
    Entonces el sistema rechaza el registro
    Y muestra "Declare la fecha en que ocurrió el hecho. La fecha de captura (14/03/2026) no la sustituye (RN-46)."

  Escenario: Se rechaza la digitación diferida sin el original escaneado
    Dado un formato en papel de solicitud llenado el "2026-03-10"
    Cuando el Encargado de Delegación intenta cerrar la digitación sin adjuntar el original escaneado
    Entonces el sistema rechaza el cierre
    Y muestra "Adjunte el original escaneado del formato en papel. La digitación diferida no se registra sin su respaldo (RN-47)."

  Escenario: Se bloquea el envío con el rango de correlativos agotado
    Dado un rango local con el último correlativo disponible "PLE-2026-00060" ya consumido
    Cuando el Encargado de Delegación intenta enviar una solicitud nueva sin conectividad
    Entonces el sistema no ejecuta el envío
    Y muestra "El rango de correlativos de la delegación Puerto Lempira está agotado (PLE-2026-00040 a PLE-2026-00060). Solicite ampliación de rango a la sede antes de continuar."

  Escenario: Se advierte la antigüedad de la tabla local antes de mostrar el estimado
    Dada una fecha del dispositivo del "2026-03-14"
    Cuando el Encargado de Delegación consulta el estimado de peajes de una solicitud capturada sin red
    Entonces el sistema muestra "Tabla de tarifas con 41 días sin sincronizar; el umbral es de 30. El estimado puede no corresponder a la tarifa vigente."
    Y muestra esa advertencia antes del monto
    Y deja la advertencia asentada en el diario del expediente

  Escenario: Se captura y envía la solicitud completa sin ninguna conectividad
    Dado un borrador con objeto del traslado "Carga" de "400" kg y ventana del "2026-03-16 07:00" al "2026-03-16 18:00"
    Cuando el Encargado de Delegación envía la solicitud sin conectividad
    Entonces el expediente recibe el correlativo "PLE-2026-00041" del rango local
    Y el expediente pasa a estado "SOLICITADA"
    Y queda encolado para sincronizar
    Y ninguna parte del registro depende de haber alcanzado el servidor

  Escenario: La sincronización conserva las dos fechas y no altera la cronología
    Dado un expediente capturado sin red con fecha del hecho "2026-03-10 06:30" y fecha de captura "2026-03-14 09:15"
    Cuando el dispositivo recupera conectividad y sincroniza
    Entonces el servidor conserva la fecha del hecho "2026-03-10 06:30"
    Y conserva la fecha de captura "2026-03-14 09:15"
    Y ningún cálculo usa la fecha de captura como fecha del hecho

  Escenario: Un conflicto de sincronización va a cola de resolución humana
    Dado un expediente "PLE-2026-00041" modificado en la sede mientras el dispositivo estaba sin red
    Y una versión distinta del mismo expediente en el dispositivo
    Cuando el dispositivo sincroniza
    Entonces el sistema no sobrescribe ninguna de las dos versiones
    Y coloca el conflicto en la cola de resolución con ambas versiones visibles
    Y muestra "Conflicto en PLE-2026-00041: existen dos versiones. Requiere resolución de una persona; no se sobrescribe (RN-45)."
```

## Fuera de alcance

- La **autorización** sin conectividad con código de un solo uso — es parte de [HU-011](HU-011-registro-inmutable-de-la-autorizacion.md)
- La bitácora de ejecución en papel y su hoja impresa con folio ([`RN-80`](../../01-negocio/reglas/RN-80-hoja-de-bitacora-impresa-con-folio.md)) — es de M-08 y M-15
- El mecanismo de transporte y cifrado de la cola de sincronización: es capacidad requerida, no diseño de esta historia. El stack está diferido al Sprint 2 por [ADR-000](../../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md)
- La ampliación del rango de folios **sin conectividad**: queda declarada como pendiente, no resuelta aquí

## Notas y pendientes

- `[C]` Procedimiento de ampliación de rango de folios cuando la delegación está desconectada — insumo #1
- `[C]` **¿Puede digitar quien después liquida?** Pregunta abierta a Auditoría Interna — insumo #27. Si la respuesta es no, la digitación diferida adquiere una incompatibilidad más
- `[C]` Formato en papel vigente de la solicitud, para que la pantalla de digitación tenga **paridad exacta** con la hoja — insumo #2
- `[V]` La falta de conectividad en el área rural consta en [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) con fuente del INE (EPHPM julio 2025)
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) flujo alterno A2; premisa rectora 5 de `CLAUDE.md`
