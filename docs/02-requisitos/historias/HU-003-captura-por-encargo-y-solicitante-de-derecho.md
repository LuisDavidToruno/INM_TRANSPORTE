# HU-003 — Declarar la captura por encargo y el solicitante de derecho

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte |
| **Actor** | ACT-02 Solicitante (en calidad de capturador) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** servidor que captura una solicitud por encargo de otro —típicamente la asistente de la unidad que la llena para su jefatura—
**quiero** declarar expresamente que capturo por encargo y quién es el servidor que requiere la movilización
**para** que la segregación de funciones se evalúe contra quien realmente solicita, y no contra quien solo digitó

## Contexto

Es el escenario más frecuente de la operación real y **el más peligroso del sistema**: la asistente captura la solicitud para su jefe, y el jefe la autoriza. Leída literalmente, la precondición `BD-01` compara al autorizador contra *quien creó* y *quien envió* el expediente — y el jefe no hizo ninguna de las dos cosas. **La segregación no bloquearía**, aunque la incompatibilidad `I-01` sí se estaría violando. Es el hallazgo [`HB3-01`](../../05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md).

Esta historia es el **habilitador de dato** de [HU-010](HU-010-bloqueo-de-segregacion-y-escalamiento.md): sin el solicitante de derecho registrado como campo propio, el bloqueo de segregación no tiene contra qué comparar.

La captura por encargo **se declara, no se infiere**. El sistema no adivina que alguien captura para su jefe a partir de la estructura organizativa: si nadie lo declara, el capturador **es** el solicitante de derecho y así se registra.

## Reglas que la gobiernan

- [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) — Distinción entre capturador y solicitante de derecho; el escalamiento se resuelve sobre el solicitante de derecho
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Un mismo servidor no ejerce dos funciones de control sobre la misma Orden de Misión. Bloqueo duro
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — Ambas identidades quedan registradas de forma inmutable con su puesto y momento
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Enviada la solicitud, la declaración de encargo ya no se edita

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` cubre este flujo. La constancia queda dejada: el caso que lo motiva es el hallazgo [`HB3-01`](../../05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md), no un caso especial

## Criterios de aceptación

```gherkin
# language: es
Característica: Capturador y solicitante de derecho como identidades distintas
  Como capturador por encargo
  quiero declarar por quién capturo
  para que la segregación se evalúe sobre quien realmente solicita

  Antecedentes:
    Dada una servidora "Karla Zelaya", asistente de la dependencia "Subgerencia de Operaciones"
    Y un servidor "Rolando Discua", Subgerente de Operaciones, jefatura inmediata de esa dependencia
    Y un servidor "Marvin Cálix", analista de la misma dependencia

  Escenario: Se rechaza declarar captura por encargo sin identificar al solicitante de derecho
    Dado un borrador creado por "Karla Zelaya"
    Cuando "Karla Zelaya" marca la solicitud como capturada por encargo sin indicar por quién
    Entonces el sistema rechaza la declaración
    Y muestra "Indique el servidor que requiere la movilización. La captura por encargo se declara con nombre; sin él, usted queda como solicitante de derecho."

  Escenario: Se rechaza al solicitante de derecho sin puesto vigente
    Dado un servidor "Elmer Padilla" cuyo puesto figura como cesado el "2026-02-28" en el espejo de Talento Humano
    Y un borrador creado por "Karla Zelaya" el "2026-03-14"
    Cuando "Karla Zelaya" declara a "Elmer Padilla" como solicitante de derecho
    Entonces el sistema rechaza la declaración
    Y muestra "Elmer Padilla no tiene puesto vigente al 14/03/2026. No puede figurar como solicitante de derecho."

  Escenario: Sin declaración de encargo, el capturador es el solicitante de derecho
    Dado un borrador creado por "Marvin Cálix" sin declaración de captura por encargo
    Cuando "Marvin Cálix" envía la solicitud a autorización
    Entonces el sistema registra a "Marvin Cálix" como capturador y como solicitante de derecho
    Y no infiere ningún encargo a partir de la estructura organizativa

  Escenario: Se registra la captura por encargo con las dos identidades
    Dado un borrador creado por "Karla Zelaya"
    Y una declaración de captura por encargo a favor de "Rolando Discua"
    Cuando "Karla Zelaya" envía la solicitud a autorización
    Entonces el sistema registra a "Karla Zelaya" como capturadora y como remitente
    Y registra a "Rolando Discua" como solicitante de derecho
    Y la cadena de autorización se resuelve sobre "Rolando Discua"
    Y el expediente muestra las tres identidades en pantalla y en su versión impresa

  Escenario: La declaración de encargo no se edita después del envío
    Dado un expediente en estado "SOLICITADA" con solicitante de derecho "Rolando Discua"
    Cuando "Karla Zelaya" intenta cambiar el solicitante de derecho a "Marvin Cálix"
    Entonces el sistema rechaza el cambio
    Y muestra "El contenido sustantivo está congelado desde el envío. Para corregirlo, la jefatura debe devolver el expediente a borrador (T-04), lo que incrementa su versión."
```

## Fuera de alcance

- El **bloqueo** de la autorización cuando el autorizador coincide con alguna de las tres identidades — es [HU-010](HU-010-bloqueo-de-segregacion-y-escalamiento.md)
- La corrección de `BD-01` en la máquina de estados: es materia de la autoridad en transiciones, no de esta historia. Aquí solo se produce el dato que `BD-01` necesita
- La delegación **de autorización** ([`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md)): capturar por encargo no es delegar firma. Son cosas distintas y no se mezclan — ver [HU-015](HU-015-autorizacion-por-delegacion-de-firma.md)

## Notas y pendientes

- `[C]` Si la institución exige respaldo documental del encargo (correo, memorando) o basta la declaración en el sistema — insumo #1
- `[P]` La exigencia de que las funciones de control recaigan en servidores distintos proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), verificada parcialmente. Esta historia **no** eleva ese nivel
- **Hallazgo abierto:** `BD-01` sigue redactada contra creador y remitente. Corrección pendiente en [`orden-de-mision.md`](../../03-arquitectura/estados/orden-de-mision.md), registrada como [`HB3-01`](../../05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md)
- Trazabilidad: [CU-01](../casos-de-uso/CU-01-registrar-solicitud-de-transporte.md) flujo alterno A1; [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) nota `HCU-01`
