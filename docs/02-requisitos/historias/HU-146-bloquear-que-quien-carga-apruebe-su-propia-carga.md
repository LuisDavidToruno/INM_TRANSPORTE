# HU-146 — Impedir que quien carga un parámetro lo apruebe, y registrar cada intento

| Campo | Valor |
|---|---|
| **Módulo** | M-02 Catálogos Maestros · M-01 Organización y Seguridad |
| **Actor** | ACT-12 Auditor Interno (como beneficiario del control) · ACT-01 y ACT-08 como sujetos |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — el circuito lo fija `RN-39`; la numeración normativa de la exigencia sigue `[C]` |

## Historia

**Como** Auditor Interno
**quiero** que el sistema impida que la misma persona cargue y apruebe un parámetro, en cualquier orden, y que cada intento quede registrado con el par de incompatibilidad detectado
**para** que nadie pueda alterar por su cuenta la base de cálculo de toda conciliación pasada y futura, y para poder ver quién lo intentó

## Contexto

`I-13` es **núcleo irreductible**: *"`ACT-01` Administrador × cualquier rol con facultad de autorizar, aprobar fondo o liquidar. Absoluto, permanente. Podría otorgarse a sí mismo la facultad y borrar el rastro"* ([actores-y-roles §5.2](../../01-negocio/actores-y-roles.md)). `RN-39` lo lleva al terreno concreto: *"Quien carga **no puede** aprobar, y quien aprueba **no puede** cargar sobre el mismo parámetro. (…) `ACT-01` **no puede** en ningún caso ostentar la facultad de aprobar."*

Hay dos formas de vulnerarlo y el sistema tiene que cerrar las dos:

1. **La directa** — la misma persona carga y aprueba. Se bloquea en el acto.
2. **La indirecta** — se le otorga a `ACT-01` un rol con facultad de aprobar. Se bloquea al otorgar el rol ([HU-130](HU-130-rechazar-acumulacion-incompatible-de-roles.md)).

Y el **intento bloqueado es información de control, no ruido**: *"un mismo usuario intentando quince veces autorizar sus propias solicitudes es exactamente lo que Auditoría Interna quiere ver."*

Este bloqueo **no admite excepción, delegación, emergencia ni resolución de la máxima autoridad**.

## Reglas que la gobiernan

- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — Quien carga no aprueba; el intento de aprobar la propia carga se bloquea y se registra como `I-13`
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — Segregación como bloqueo duro, sin botón de continuar
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El intento se registra con identidad, momento y contenido pretendido
- [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) — Ni el Administrador del Sistema puede borrar el registro del intento
- [RN-07](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) — La delegación no levanta la incompatibilidad

## Casos especiales que la afectan

- Ninguno de los 28 vigentes. **Caso especial candidato:** *institución donde la unidad de informática y la Gerencia Administrativa son la misma persona* — es el escenario donde este bloqueo deja de operar por aritmética, igual que la delegación de tres personas

## Criterios de aceptación

```gherkin
# language: es
Característica: Doble control efectivo sobre parámetros normativos

  Antecedentes:
    Dado un parámetro "umbral_desviacion_consumo" con carga pendiente de "25" por ciento
    Y que la carga la hizo "Carlos Fúnez" el "2026-09-18"
    Y que "Carlos Fúnez" ocupa un puesto con el rol "ACT-01 Administrador del Sistema"

  Escenario: Se bloquea aprobar la propia carga
    Cuando "Carlos Fúnez" intenta aprobar la puesta en vigencia del "25" por ciento
    Entonces el sistema rechaza la aprobación
    Y muestra "Usted cargó este parámetro el 18/09/2026. La incompatibilidad I-13 es núcleo irreductible: quien carga no aprueba. Corresponde a la Gerencia Administrativa."
    Y registra el intento con persona, puesto, parámetro, par "I-13" y momento
    Y no ofrece continuar, forzar ni justificar

  Escenario: Se bloquea en sentido inverso: cargar sobre un parámetro que uno aprueba
    Dado que "Rolando Discua" aprobó la vigencia anterior de "umbral_desviacion_consumo"
    Cuando "Rolando Discua" intenta cargar una nueva vigencia de ese mismo parámetro
    Entonces el sistema rechaza la carga
    Y muestra "Usted aprueba la puesta en vigencia de umbral_desviacion_consumo. La incompatibilidad I-13 es simétrica: quien aprueba no carga sobre el mismo parámetro."

  Escenario: La delegación no levanta el bloqueo
    Dada una delegación vigente "DEL-2026-0040" que faculta a "Carlos Fúnez" para aprobar parámetros
    Cuando "Carlos Fúnez" intenta aprobar su propia carga por delegación
    Entonces el sistema rechaza la aprobación
    Y muestra "La delegación DEL-2026-0040 no levanta la incompatibilidad I-13, que es núcleo irreductible. Usted cargó este parámetro el 18/09/2026."

  Escenario: La emergencia no levanta el bloqueo
    Cuando "Carlos Fúnez" intenta aprobar su propia carga declarando emergencia por cambio de tarifa vigente desde mañana
    Entonces el sistema rechaza la aprobación
    Y muestra "El núcleo irreductible no admite emergencia. Mientras no se apruebe, rige la tarifa anterior; ninguna operación queda detenida."

  Escenario: El Administrador del Sistema no puede borrar el registro del intento
    Dados 15 intentos bloqueados de "Carlos Fúnez" sobre el mismo parámetro
    Cuando "Carlos Fúnez" intenta depurar la pista de auditoría
    Entonces el sistema rechaza la operación
    Y muestra "La pista de auditoría es append-only. El Administrador del Sistema no puede borrarla ni alterarla."
    Y registra también este intento

  Escenario: El patrón de intentos repetidos se presenta como hallazgo
    Dados 15 intentos bloqueados del mismo usuario sobre parámetros en 30 días
    Cuando el Auditor Interno abre el reporte de intentos bloqueados
    Entonces ve los 15 intentos agrupados por persona, parámetro y par detectado
    Y el sistema los señala como patrón, no como eventos sueltos
    Y el reporte es exportable como paquete de evidencia

  Escenario: La aprobación por una persona distinta sí procede
    Cuando "Rolando Discua", que no cargó el parámetro, aprueba la puesta en vigencia
    Entonces el sistema acepta la aprobación
    Y registra carga y aprobación con autores distintos y momentos distintos
```

## Fuera de alcance

- El rechazo del otorgamiento del rol que produciría la acumulación — es [HU-130](HU-130-rechazar-acumulacion-incompatible-de-roles.md)
- El bloqueo de segregación sobre una Orden de Misión concreta — es [HU-010](HU-010-bloqueo-de-segregacion-y-escalamiento.md)
- La pantalla de aprobación — es [HU-145](HU-145-aprobar-la-puesta-en-vigencia-doble-control.md)

## Notas y pendientes

- `[I]` El doble control carga ↔ aprobación es diseño de control interno de [actores-y-roles §4.3](../../01-negocio/actores-y-roles.md) y de `RN-39`; **no hay articulado citable**. `[P]` la exigencia general de segregación de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), con numeración `[C]` — insumo **#23**
- `[C]` **¿Qué se hace en una institución donde informática y Gerencia Administrativa recaen en la misma persona?** Por aritmética el control no se puede cumplir localmente, igual que en la delegación de tres personas. La postura provisional es la misma: **escalamiento**, no excepción. Requiere pronunciamiento de Auditoría Interna — insumo **#26**
- `[C]` Si la institución exige que el aprobador sea un puesto distinto o basta con una persona distinta. Esta historia bloquea **por persona**, coherente con `RN-01`
