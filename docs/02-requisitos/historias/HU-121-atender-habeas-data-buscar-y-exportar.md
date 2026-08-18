# HU-121 — Localizar y exportar todos los registros de una persona para atender un hábeas data

| Campo | Valor |
|---|---|
| **Módulo** | M-17 Traslado de Personas Externas · M-14 Reportes, Indicadores y Auditoría |
| **Actor** | ACT-12 Auditor Interno · `[C]` Oficial de Información Pública — actor no catalogado |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — falta definir quién ejerce este rol en la institución |

## Historia

**Como** responsable institucional de atender una acción de hábeas data
**quiero** localizar en minutos todos los registros que el sistema guarda sobre una persona identificada y exportarlos
**para** que la institución pueda responder de forma expedita y no onerosa, como exige el Artículo 182 de la Constitución, sin depender de que alguien consulte la base de datos a mano

## Contexto

`[V]` El **hábeas data del Artículo 182 constitucional** (reforma de 2013) está vigente: toda persona tiene derecho a acceder de forma expedita y no onerosa a la información sobre sí misma contenida en bases de datos o registros públicos o privados y, en su caso, actualizarla, rectificarla o suprimirla. **Solo el titular puede interponer la acción.** Se regula conforme a la Ley sobre Justicia Constitucional y al Artículo 23 de la LTAIP `[V]` ([NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md)).

`[V]` No existe ley general de protección de datos personales vigente en Honduras, y [DP-001 D-14](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) decidió **no diseñar para anticipar la que está en trámite**. Esta historia no anticipa nada: implementa un derecho que ya se puede ejercer hoy.

Lo que la hace no trivial es que la búsqueda por identidad **es en sí misma un acceso a datos personales**, y por tanto se registra igual que cualquier otra ([RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md)). Una funcionalidad de "buscar todo sobre una persona" sin expediente que la respalde sería la herramienta de vigilancia más cómoda que el sistema podría ofrecer.

## Reglas que la gobiernan

- [RN-51](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) — Comportamiento esperado 5: buscar todos los registros de una persona, exportarlos y rectificarlos
- [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) — La búsqueda por identidad se registra, incluso si no devuelve resultados
- [RN-94](../../01-negocio/reglas/RN-94-fecha-de-corte-de-conocimiento-en-todo-reporte.md) — La exportación declara su fecha de corte de conocimiento

## Requisitos no funcionales relacionados

- [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) — **≤ 5 minutos** para localizar todos los registros de una persona identificada, desde la interfaz y sin intervención de desarrollo
- [RNF-18](../no-funcionales/RNF-18-paquetes-de-evidencia-para-auditoria.md) — Paquetes de evidencia exportables

## Criterios de aceptación

> Los nombres y números de identidad de estos escenarios son **ficticios de prueba**.

```gherkin
# language: es
Característica: Localización y exportación de los registros de una persona por hábeas data

  Antecedentes:
    Dado una persona externa "Ana de Prueba Uno" con identidad "0000-0000-00001"
    Y "12" Órdenes de Misión entre "2026-02-10" y "2026-09-18" cuyos manifiestos la incluyen
    Y un expediente de hábeas data "HD-2026-004" recibido el "2026-10-05"

  Escenario: Se rechaza la búsqueda por identidad sin expediente que la respalde
    Cuando el Auditor Interno intenta buscar todos los registros de la identidad "0000-0000-00001" sin indicar expediente
    Entonces el sistema rechaza la búsqueda
    Y muestra "La búsqueda por identidad requiere el expediente que la motiva. Indique el número de expediente de hábeas data o de la solicitud de información."

  Escenario: Se rechaza la búsqueda por identidad a un rol sin necesidad de conocer
    Dado un Encargado de Mantenimiento
    Cuando el Encargado de Mantenimiento intenta buscar los registros de la identidad "0000-0000-00001" con el expediente "HD-2026-004"
    Entonces el sistema deniega la búsqueda
    Y muestra "Su puesto no atiende solicitudes de hábeas data."
    Y registra el intento denegado

  Escenario: Se rechaza exportar sin confirmar la identidad cuando hay coincidencias parciales
    Dado que existen "3" personas con nombre parecido a "Ana de Prueba" y sin número de identidad registrado
    Cuando el Auditor Interno intenta exportar el resultado sin confirmar cuál corresponde al expediente "HD-2026-004"
    Entonces el sistema rechaza la exportación
    Y muestra "Hay 3 coincidencias parciales sin identidad confirmada. Confirme cuál corresponde a la persona solicitante antes de exportar: exportar de más entrega datos de terceros."

  Escenario: La búsqueda localiza todos los registros de la persona
    Cuando el Auditor Interno busca los registros de la identidad "0000-0000-00001" con el expediente "HD-2026-004"
    Entonces el sistema devuelve las "12" Órdenes de Misión que la incluyen
    Y por cada una muestra fecha, origen, destino, institución o condición declarada, vehículo y estado de la misión
    Y muestra además las novedades de ruta que la mencionan y las actas de entrega que la incluyen
    Y responde en un tiempo no mayor a "5" minutos sin intervención de desarrollo

  Escenario: La búsqueda por hábeas data se registra como consulta
    Cuando el Auditor Interno ejecuta la búsqueda de la identidad "0000-0000-00001" con el expediente "HD-2026-004"
    Entonces el sistema registra una consulta con alcance "HABEAS DATA", el expediente "HD-2026-004", la identidad del consultante, el rol, la fecha y la hora
    Y ese registro es inmutable

  Escenario: La búsqueda sin resultados también se registra y se informa
    Cuando el Auditor Interno busca la identidad "0000-0000-00009" con el expediente "HD-2026-005"
    Entonces el sistema muestra "Sin registros. El sistema no conserva ningún dato asociado a esa identidad."
    Y registra la consulta con resultado "0 coincidencias"

  Escenario: La exportación deja constancia de qué se entregó y a quién
    Cuando el Auditor Interno exporta el resultado del expediente "HD-2026-004" para entregarlo al titular
    Entonces el sistema genera el paquete con folio, fecha de corte de conocimiento "2026-10-05" y huella del contenido
    Y registra qué se entregó, a qué expediente y en qué fecha
    Y el paquete no incluye datos de ninguna otra persona trasladada en las mismas misiones
```

## Fuera de alcance

- La rectificación de lo encontrado — es [HU-122](HU-122-rectificar-por-habeas-data-sin-destruir-el-asiento.md)
- La depuración por retención — es [HU-124](HU-124-depurar-datos-personales-sin-romper-la-cadena.md)
- La exportación de transparencia sin datos personales — es [HU-123](HU-123-exportar-transparencia-sin-datos-personales.md)
- La tramitación legal de la acción de hábeas data: es institucional, no del sistema. SIGTI produce la evidencia
- Los datos de la persona que estén en **ARGOS o Talento Humano**: SIGTI los referencia por espejo ([RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)) y no responde por ellos

## Notas y pendientes

- `[C]` **Quién atiende un hábeas data en la institución.** El natural es el **Oficial de Información Pública**, que [NRM-07](../../01-negocio/normativa/NRM-07-transparencia-y-datos-personales.md) `[V]` identifica como el responsable de publicar y de atender solicitudes — pero **no existe como actor** en [actores-y-roles.md](../../01-negocio/actores-y-roles.md). Aquí se asignó provisionalmente a `ACT-12` Auditor Interno porque su alcance es de solo lectura con registro de cada consulta. **Actor candidato, no dar por catalogado.** Es lo que mantiene la historia en borrador
- `[C]` Si la búsqueda debe alcanzar también los **respaldos y los almacenes locales de los dispositivos** al momento de responder, o solo la base activa. [RNF-17](../no-funcionales/RNF-17-retencion-y-depuracion-diferenciada.md) lo exige para la depuración; para la localización no está resuelto
- `[C]` Plazo institucional de respuesta a un hábeas data — se toma de la Ley sobre Justicia Constitucional `[P]`; el articulado no se ha extraído
- **Regla candidata `RN-C17b`** — *Toda búsqueda por identidad de persona sobre el acervo de manifiestos exige un expediente que la motive, y se registra con ese expediente.* [RN-52](../../01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) exige registrar la consulta, **pero ninguna regla vigente exige un expediente que la justifique**. No darla por escrita
