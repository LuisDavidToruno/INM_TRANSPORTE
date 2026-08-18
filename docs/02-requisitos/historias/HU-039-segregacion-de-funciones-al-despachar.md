# HU-039 — Bloquear el despacho cuando quien despacha ejerce otra función de control sobre la misma misión

| Campo | Valor |
|---|---|
| **Módulo** | M-07 Programación y Despacho · M-01 Organización y Seguridad |
| **Actor** | ACT-05 Encargado de Despacho · ACT-10 Encargado de Delegación en su ámbito |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Refinada |
| **Deriva de** | [CU-06](../casos-de-uso/CU-06-despachar-y-registrar-salida.md) paso 4 y E4 · `T-12` · `BD-06` · `I-02`, `I-05`, `I-08`, `I-11` |

## Historia

**Como** Auditor Interno
**quiero** que el sistema impida despachar a quien también solicitó, autorizó, va a conducir o va a entregar el combustible de esa misma misión, verificándolo **por identidad de persona y no por rol asignado**
**para** que la cadena de control del Estado no se rompa por la vía más simple: la misma persona con dos sombreros

## Contexto

La segregación de funciones es mandato del control interno del Estado, no una buena práctica: quien solicita ≠ quien autoriza ≠ quien despacha ≠ quien entrega combustible ≠ quien liquida.

Verificar por rol no sirve. Si el sistema comprueba que "el usuario tiene rol de Encargado de Despacho", pasa sin problema la persona que además tiene rol de Solicitante en esa misma misión. **La verificación es por identidad**: la persona concreta, contra el conjunto de actos ya registrados en ese expediente.

En delegaciones con dotación insuficiente la tentación es un régimen de excepción. **No se ofrece** ([DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)): la salida es el escalamiento a sede —la función incompatible la ejerce remotamente alguien de la sede— y, si la delegación está desconectada, el código de autorización fuera de línea. Tres pares son **núcleo irreductible** y no los levanta ningún régimen: `I-07`, `I-10` e `I-11`.

## Reglas que la gobiernan

- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — **Regla rectora**: un mismo servidor no ejerce dos funciones de control sobre la misma Orden de Misión
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El acto de despacho registra identidad, rol ejercido, momento, origen y huella
- [RN-07](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) — La delegación de autorización no rompe la segregación
- [RN-32](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md) — Quien entrega el fondo no es quien despacha

La matriz de incompatibilidades es autoridad de [`actores-y-roles.md §5.2`](../../01-negocio/actores-y-roles.md): esta historia **no la copia**, la aplica.

## Casos especiales que la afectan

- [CE-09](../casos-especiales/CE-09-bitacora-en-papel-digitada-dias-despues.md) — En delegación de tres personas es donde el conflicto aparece a diario
- [CE-01](../casos-especiales/CE-01-salida-de-emergencia-convalidada.md) — Ni la emergencia levanta el núcleo irreductible: se convalida el acto, no la incompatibilidad

## Criterios de aceptación

```gherkin
# language: es
Característica: Segregación de funciones en el acto de despacho

  Antecedentes:
    Dada una Orden de Misión "OM-2026-0451"
    Y que "Sandra Paz" registró la solicitud "SOL-2026-0370" de esa misión
    Y que "Carlos Rodríguez" autorizó esa solicitud
    Y que "José Martínez" es el motorista asignado
    Y que "Delmy Cruz" es la Encargada de Combustible que entregará el fondo

  Escenario: Se rechaza el despacho ejecutado por quien solicitó la misión
    Dado que "Sandra Paz" tiene también el rol de Encargada de Despacho
    Cuando "Sandra Paz" intenta despachar "OM-2026-0451"
    Entonces el sistema rechaza el despacho
    Y muestra "Sandra Paz registró la solicitud SOL-2026-0370 de esta misión. Quien solicita no puede despachar. Solicite el despacho a otro Encargado de Despacho o escale a sede."
    Y registra el intento con el par de incompatibilidad detectado

  Escenario: Se rechaza el despacho ejecutado por quien autorizó la misión
    Dado que "Carlos Rodríguez" tiene también el rol de Encargado de Despacho
    Cuando "Carlos Rodríguez" intenta despachar "OM-2026-0451"
    Entonces el sistema rechaza el despacho
    Y muestra "Carlos Rodríguez autorizó esta misión. Quien autoriza no puede despachar."

  Escenario: Se rechaza el despacho ejecutado por el propio motorista
    Dado que "José Martínez" tiene también el rol de Encargado de Despacho
    Cuando "José Martínez" intenta despachar "OM-2026-0451"
    Entonces el sistema rechaza el despacho
    Y muestra "José Martínez es el motorista de esta misión. El motorista no puede despachar, autorizar, entregar el fondo ni liquidar su propia misión."
    Y el bloqueo no admite excepción por régimen de delegación

  Escenario: Se rechaza que quien despacha entregue además el fondo
    Dado que "Delmy Cruz" despachó "OM-2026-0451"
    Cuando "Delmy Cruz" intenta registrar la entrega del fondo de combustible de esa misión
    Entonces el sistema rechaza la entrega
    Y muestra "Delmy Cruz despachó esta misión. Quien despacha no puede entregar el fondo."

  Escenario: El escalamiento a sede resuelve la falta de dotación en la delegación
    Dado que la Delegación Choluteca tiene un solo servidor con roles operativos
    Y que ese servidor registró la solicitud de "OM-2026-0451"
    Cuando el Encargado de Despacho de la sede despacha "OM-2026-0451" de forma remota
    Entonces el sistema acepta el despacho
    Y registra que el acto se ejerció por escalamiento a sede, con la identidad de quien lo ejecutó

  Escenario: Despacho conforme, sin incompatibilidad
    Dado un Encargado de Despacho "Mario Fúnez" sin participación previa en la misión
    Cuando "Mario Fúnez" despacha "OM-2026-0451"
    Entonces el sistema acepta el despacho
    Y registra identidad, rol ejercido, momento, origen y huella del acto
```

## Fuera de alcance

- La definición de la matriz de incompatibilidades y su mantenimiento — es de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) y de M-01
- La segregación en la liquidación y en la aprobación del fondo — son de M-13 y M-09
- El mecanismo del código de autorización fuera de línea — es de M-16

## Notas y pendientes

- `[C]` **Pronunciamiento de Auditoría Interna sobre el régimen de excepción en delegaciones** — insumo #26. Hasta que exista, `DP-002` mantiene la excepción **suspendida**.
- `[C]` **Mapa de delegaciones con dotación real de personal** y qué puesto de la sede respalda a cada una — insumo #27. Sin esto, el escalamiento a sede no se puede configurar.
- `[C]` **¿Existe régimen formal de excusa por conflicto de interés** entre solicitante y autorizador? — insumo #30. Si existe, es una incompatibilidad más.
- `[P]` La enumeración de funciones incompatibles proviene de [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); el articulado no se pudo extraer.
