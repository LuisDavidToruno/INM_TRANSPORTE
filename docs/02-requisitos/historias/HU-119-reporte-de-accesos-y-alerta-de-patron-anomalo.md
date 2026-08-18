# HU-119 — Consultar el reporte de accesos a manifiestos y resolver las alertas por patrón anómalo

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-12 Auditor Interno |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Borrador — faltan los umbrales de anomalía y el horario hábil institucional |

## Historia

**Como** Auditor Interno
**quiero** ver quién accedió a manifiestos por usuario, por registro y por período, y que el sistema me señale los patrones anómalos
**para** detectar el uso indebido de datos personales mientras todavía se puede corregir, y no cuando llega el reclamo

## Contexto

Registrar cada consulta ([HU-118](HU-118-registrar-cada-consulta-al-manifiesto.md)) sirve de poco si nadie mira el registro. [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) numeral 4 exige el reporte **y** la alerta: *"consultas masivas, consultas fuera de horario, consultas repetidas sobre una misma persona"*.

El punto delicado es el tono. Una jefatura que revisa todos los manifiestos del mes para armar un informe va a disparar la alerta, y va a tener razón. **La alerta no acusa: señala para revisión.** El responsable la resuelve anotando el motivo, y esa anotación queda ([RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md), caso límite). Una alerta que se cierra sin explicación no vale nada; una que castiga hace que la gente evite el sistema.

Los umbrales son **parámetros con vigencia** ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)): cuántas consultas sobre la misma persona en cuántos días, qué se considera masivo, qué es fuera de horario. Ninguno se escribe en el código.

## Reglas que la gobiernan

- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — **Regla rectora**: reporte por usuario, por registro y por período, con alerta ante patrones anómalos
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Los umbrales de anomalía y el horario hábil son parámetros con vigencia
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — El reporte declara su fecha de corte de conocimiento y es reproducible a esa fecha
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — La resolución de una alerta es un asiento, no una edición

## Requisitos no funcionales relacionados

- [RNF-14](../no-funcionales/RNF-14-control-de-acceso-por-puesto-y-registro-de-consultas.md) — Control de acceso por puesto y registro de consultas
- [RNF-06](../no-funcionales/RNF-06-reproducibilidad-historica-de-reportes.md) — Reproducibilidad histórica de reportes
- [RNF-18](../no-funcionales/RNF-18-paquetes-de-evidencia-para-auditoria.md) — Paquetes de evidencia para auditoría

## Criterios de aceptación

> Los nombres de personas externas de estos escenarios son **ficticios de prueba** y los umbrales son valores de ejemplo, no valores fijados.

```gherkin
# language: es
Característica: Reporte de accesos a manifiestos y alertas por patrón anómalo

  Antecedentes:
    Dado un umbral parametrizado de "5" consultas sobre la misma persona en "30" días naturales
    Y un umbral parametrizado de "100" registros para considerar una consulta masiva
    Y un horario hábil institucional parametrizado de "07:00" a "16:00" de lunes a viernes

  Escenario: Se rechaza cerrar una alerta sin motivo
    Dado una alerta de tipo "CONSULTAS REPETIDAS SOBRE UNA MISMA PERSONA" abierta el "2026-09-25"
    Cuando el Auditor Interno intenta cerrar la alerta sin registrar motivo
    Entonces el sistema rechaza el cierre
    Y muestra "Registre el motivo por el cual la alerta se considera resuelta. La anotación queda en el expediente de la alerta."

  Escenario: Se rechaza modificar el motivo ya registrado de una alerta cerrada
    Dado una alerta cerrada el "2026-09-26" con motivo "revisión mensual autorizada por la Gerencia Administrativa"
    Cuando el Auditor Interno intenta modificar ese motivo
    Entonces el sistema rechaza la modificación
    Y muestra "El motivo registrado no se edita. Agregue una anotación nueva si necesita corregirlo."

  Escenario: Se alerta por consultas repetidas sobre una misma persona
    Dado que un mismo usuario consultó "6" veces los registros de "Ana de Prueba Uno" entre el "2026-09-01" y el "2026-09-20"
    Cuando el sistema evalúa los patrones del período
    Entonces genera una alerta de tipo "CONSULTAS REPETIDAS SOBRE UNA MISMA PERSONA"
    Y la alerta indica el usuario, el conteo "6", el umbral "5" y el período evaluado
    Y no bloquea ningún acceso

  Escenario: Se alerta por consulta fuera del horario hábil
    Dado una consulta al manifiesto de "OM-2026-0451" el sábado "2026-09-19" a las "23:14"
    Cuando el sistema evalúa los patrones del período
    Entonces genera una alerta de tipo "CONSULTA FUERA DE HORARIO HÁBIL"
    Y la alerta indica la fecha, la hora, el usuario y el horario hábil vigente a esa fecha

  Escenario: La alerta legítima se resuelve con anotación y queda
    Dado una alerta de tipo "CONSULTA MASIVA" por la exportación de "412" manifiestos
    Cuando el Auditor Interno la cierra con el motivo "informe semestral de traslados solicitado por la Gerencia Administrativa, oficio de ejemplo GA-2026-118"
    Entonces el sistema cierra la alerta como "REVISADA"
    Y conserva el motivo, el autor y la fecha de la resolución de forma inmutable

  Escenario: El reporte de accesos declara su fecha de corte y es reproducible
    Cuando el Auditor Interno genera el reporte de accesos del "2026-01-01" al "2026-09-30" con fecha de corte "2026-10-01"
    Entonces el reporte declara la fecha de corte de conocimiento "2026-10-01"
    Y al regenerarlo el "2027-03-15" con la misma fecha de corte produce exactamente los mismos registros

  Escenario: El reporte se agrupa por usuario, por registro y por período
    Cuando el Auditor Interno genera el reporte de accesos del período agrupado por usuario
    Entonces el reporte muestra por usuario el total de consultas, su alcance y cuántas fueron con impresión
    Y permite cambiar el agrupamiento a manifiesto consultado y a período sin volver a extraer los datos
```

## Fuera de alcance

- El registro de la consulta — es [HU-118](HU-118-registrar-cada-consulta-al-manifiesto.md)
- La concesión o denegación del acceso — es [HU-117](HU-117-acceso-al-manifiesto-por-necesidad-de-conocer.md)
- La investigación disciplinaria que pueda derivar de una alerta: es del ámbito institucional, no del sistema
- El expediente de hallazgo posterior sobre una misión ya cerrada — lo gobierna [RN-93](../../01-negocio/reglas/RN-93-expediente-de-hallazgo-posterior.md)

## Notas y pendientes

- `[C]` **Umbrales de anomalía**: cuántas consultas sobre la misma persona, en cuántos días, y qué volumen se considera masivo. Los valores de los escenarios son **ejemplos**, no valores fijados. Sin acuerdo con Auditoría Interna, la alerta no se activa
- `[C]` **Horario hábil oficial de la institución** — pendiente desde [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md); es el mismo dato que necesita [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md)
- `[C]` A quién se notifica la alerta además del Auditor Interno. El Oficial de Información Pública sería el destinatario natural, pero **no existe como actor** en [actores-y-roles.md](../../01-negocio/actores-y-roles.md)
- `[I]` La tipificación de las alertas es derivación de [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) numeral 4, no un catálogo de norma
