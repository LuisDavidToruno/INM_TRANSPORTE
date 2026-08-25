# HU-134 — Declarar una suplencia con vigencia acotada, en vez de prestar la clave

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-01 Administrador del Sistema · ACT-08 Gerencia Administrativa |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — el tope máximo de duración de la suplencia está `[C]` |

## Historia

**Como** Gerencia Administrativa
**quiero** declarar que una persona suple a otra en un puesto durante una ventana de fechas concreta, y que sus actos queden firmados **como suplente**
**para** que la ausencia no trabe la operación y para que nadie tenga que prestar su clave, que es la práctica que destruye la trazabilidad sin dejar rastro de que ocurrió

## Contexto

Alguien sale de comisión, se incapacita o toma vacaciones, y el proceso se traba. La respuesta informal es prestar la clave. **El sistema debe ofrecer una alternativa mejor y más cómoda que prestar la clave** ([actores-y-roles §7](../../01-negocio/actores-y-roles.md)), porque si es peor o más lenta, la clave se presta igual.

[`RNF-15`](../no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md) lo fija como umbral: *"Puesto vacante que impida operar: **0**. Existe suplencia con vigencia por rango de fechas, receptor identificado y registro — nunca 'prestar el usuario'."* Y: *"Cuentas compartidas creadas para cubrir una vacante: **0**."*

**La suplencia no es lo mismo que la delegación de firma.** La suplencia cubre el **puesto completo** durante la ausencia del titular; la delegación acota **acciones enumeradas** conservando el titular su puesto ([HU-135](HU-135-constituir-delegacion-de-firma-con-vigencia.md)). Las dos existen y no se sustituyen.

Lo que ninguna de las dos hace es levantar una incompatibilidad: si el suplente ya solicitó la misión, no la autoriza por ser suplente.

## Reglas que la gobiernan

- [RN-07](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) — Vigencia acotada, constancia en el expediente, y **no rompe la segregación**
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La incompatibilidad se evalúa sobre la persona del suplente, no sobre el puesto suplido
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El acto del suplente registra su identidad, su condición de suplente y el puesto suplido
- [RN-12](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) — La ausencia del titular se lee del espejo de Talento Humano
- [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) — El tope máximo de duración de una suplencia es parámetro con vigencia

## Casos especiales que la afectan

- [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) — La ausencia registrada en Talento Humano es el disparador natural de la suplencia
- [CE-10](../casos-especiales/CE-10-motorista-incapacitado-en-ruta.md) — La incapacidad súbita no viene con suplencia previamente firmada

## Criterios de aceptación

```gherkin
# language: es
Característica: Suplencia de un puesto con vigencia acotada

  Antecedentes:
    Dado un puesto "Jefe de Transporte" ocupado por "María López" como titular
    Y un parámetro "duracion_maxima_suplencia" de "90" días vigente y aprobado
    Y una persona "Carlos Fúnez" ocupando el puesto "Encargado de Programación"

  Escenario: Se rechaza la suplencia sin fecha de fin
    Cuando la Gerencia Administrativa declara a "Carlos Fúnez" suplente de "Jefe de Transporte" desde el "2026-10-01" sin fecha de fin
    Entonces el sistema rechaza la declaración
    Y muestra "La suplencia exige fecha de inicio y fecha de fin. No se admiten suplencias indefinidas."

  Escenario: Se rechaza la suplencia que excede el tope configurado
    Cuando la Gerencia Administrativa declara la suplencia del "2026-10-01" al "2027-06-30"
    Entonces el sistema rechaza la declaración
    Y muestra "La suplencia duraría 272 días y el máximo configurado es de 90. Una ausencia más larga se resuelve con una asignación de puesto interina, no con una suplencia."

  Escenario: Se rechaza la suplencia que produce una incompatibilidad absoluta
    Dado que "Carlos Fúnez" ostenta el rol "ACT-12 Auditor Interno"
    Cuando la Gerencia Administrativa lo declara suplente de "Jefe de Transporte"
    Entonces el sistema rechaza la declaración
    Y muestra "Carlos Fúnez ostenta ACT-12 Auditor Interno. La incompatibilidad I-12 es núcleo irreductible y la suplencia no la levanta."

  Escenario: Se rechaza el acto del suplente que viola una incompatibilidad por misión
    Dada una suplencia vigente de "Carlos Fúnez" en "Jefe de Transporte" del "2026-10-01" al "2026-10-20"
    Y una solicitud "SOL-2026-00417" registrada por "Carlos Fúnez" el "2026-09-28"
    Cuando "Carlos Fúnez" intenta autorizarla el "2026-10-05" actuando como suplente
    Entonces el sistema rechaza la autorización
    Y muestra "Usted registró la solicitud SOL-2026-00417 el 28/09/2026. La suplencia no levanta la incompatibilidad I-01. Corresponde al puesto superior."

  Escenario: El suplente no puede operar antes del inicio de la ventana
    Dada una suplencia vigente del "2026-10-01" al "2026-10-20"
    Cuando "Carlos Fúnez" intenta programar una misión el "2026-09-30"
    Entonces el sistema rechaza la operación
    Y muestra "Su suplencia en el puesto Jefe de Transporte rige del 01/10/2026 al 20/10/2026."

  Escenario: El suplente no puede operar después del fin de la ventana
    Cuando "Carlos Fúnez" intenta programar una misión el "2026-10-21"
    Entonces el sistema rechaza la operación
    Y muestra "Su suplencia en el puesto Jefe de Transporte terminó el 20/10/2026."

  Escenario: El acto del suplente queda firmado como suplente, no como el titular
    Dada una suplencia vigente del "2026-10-01" al "2026-10-20"
    Cuando "Carlos Fúnez" emite la Orden de Misión "OM-2026-0512" el "2026-10-07"
    Entonces el asiento registra a "Carlos Fúnez", su puesto propio y su condición de suplente del puesto "Jefe de Transporte"
    Y el documento impreso muestra "Por suplencia del puesto Jefe de Transporte, del 01/10/2026 al 20/10/2026, folio SUP-2026-014."
    Y en ningún lugar figura "María López" como autora del acto

  Escenario: La ausencia registrada en Talento Humano propone la suplencia
    Dada una incapacidad de "María López" registrada en Talento Humano del "2026-10-01" al "2026-10-14"
    Cuando el evento llega al espejo
    Entonces el sistema propone a la Gerencia Administrativa declarar suplencia en el puesto "Jefe de Transporte" por esa ventana
    Y muestra "María López tiene ausencia registrada del 01/10/2026 al 14/10/2026. El puesto Jefe de Transporte tiene 4 actos pendientes de decisión."
    Y no declara ninguna suplencia por sí solo
```

## Fuera de alcance

- La delegación de acciones enumeradas conservando el titular — es [HU-135](HU-135-constituir-delegacion-de-firma-con-vigencia.md)
- El uso de la firma delegada al autorizar una solicitud — es [HU-015](HU-015-autorizacion-por-delegacion-de-firma.md)
- La asignación interina del puesto, que es otra figura — es [HU-128](HU-128-ocupar-un-puesto-con-vigencia.md)
- El escalamiento automático cuando no hay suplente ni delegación — es [HU-127](HU-127-crear-el-puesto-de-la-estructura.md)

## Notas y pendientes

- `[C]` **`duracion_maxima_suplencia`** — el "90" del criterio es dato de prueba. [actores-y-roles §7.1](../../01-negocio/actores-y-roles.md) deja el tope expresamente abierto — insumo **#32**
- `[C]` **¿Se admite la suplencia sobre el puesto o debe ser nominativa?** ¿Se admite subdelegación? — insumo **#K de `actores-y-roles`**, pendiente de trasladar a `insumos-pendientes.md`
- `[C]` **Quién declara la suplencia**: esta historia la atribuye a `ACT-08`. La matriz de permisos de [actores-y-roles §4.1](../../01-negocio/actores-y-roles.md) acción 26 marca `ACT-01` como ejecutor y `ACT-08` como aprobador para *administrar usuarios, puestos y roles*, y **no distingue la suplencia**. Confirmar con el PO
- `[I]` La figura de suplencia como distinta de la delegación de firma es propuesta de esta historia, derivada de [`RNF-15`](../no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md). No está modelada en `actores-y-roles`
- **Regla candidata:** *La suplencia de un puesto tiene fecha de inicio y fin obligatorias, tope máximo parametrizado, no levanta ninguna incompatibilidad, y todo acto ejecutado bajo ella se firma como suplencia con su folio.* `RN-07` gobierna la delegación de autorización, no la suplencia del puesto completo
