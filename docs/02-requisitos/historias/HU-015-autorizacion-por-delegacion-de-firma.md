# HU-015 — Autorizar por delegación de firma vigente y acotada

| Campo | Valor |
|---|---|
| **Módulo** | M-06 Solicitudes de Transporte (con M-01 Organización y Seguridad) |
| **Actor** | ACT-03 Jefatura Inmediata, actuando como delegado |
| **Prioridad** | Media |
| **Sprint** | sin asignar |
| **Estado** | Refinada |

## Historia

**Como** servidor a quien la jefatura titular delegó formalmente la facultad de autorizar durante su ausencia
**quiero** ver la bandeja del titular marcada como tal y registrar mis autorizaciones indicando que actúo **por delegación**, con el folio del acto que la confiere
**para** que la operación no se detenga cuando el titular está de misión o de vacaciones, sin que la firma quede sin fundamento documental

## Contexto

Hoy esto se resuelve con una nota que circula por correo, o peor, con el titular prestando su clave. Un sistema que no ofrece delegación formal **produce** delegación informal, y la delegación informal es indistinguible de una suplantación.

La delegación tiene tres condiciones que la hacen auditable: **vigencia acotada** por fechas, **folio del acto administrativo** que la confiere, y **constancia en el expediente**. Quien lea la Orden de Misión después debe poder ver por qué firmó quien firmó.

Lo que la delegación **no** hace: romper la segregación. Si el delegado es el solicitante de derecho del expediente, el bloqueo opera igual.

## Reglas que la gobiernan

- [RN-07](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) — La delegación tiene vigencia acotada, consta en el expediente y **no rompe la segregación**
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — El delegado que es solicitante de derecho queda bloqueado igual
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — La autorización registra que se actuó por delegación, con el folio del acto y su vigencia
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La vigencia de la delegación se evalúa a la fecha del acto de autorización
- [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md) — Vencida la delegación, las autorizaciones ya registradas conservan su validez y su constancia

## Casos especiales que la afectan

- Ninguno de los 28 `CE-xx` toca este flujo. Constancia dejada. El caso vecino —la firma **indelegable** del permiso de circulación— se trata en [HU-016](HU-016-tramite-y-firma-del-permiso-de-circulacion.md)

## Criterios de aceptación

```gherkin
# language: es
Característica: Autorización ejercida por delegación de firma
  Como delegado del autorizador titular
  quiero autorizar dejando constancia de la delegación
  para que la firma tenga fundamento documental verificable

  Antecedentes:
    Dado un autorizador titular "Rolando Discua", Subgerente de Operaciones
    Y un servidor "Marvin Cálix" con delegación de autorización a su favor
    Y un acto de delegación con folio "DEL-2026-0012", vigente del "2026-03-16" al "2026-03-27"
    Y un expediente "CHO-2026-00087" en estado "SOLICITADA"

  Escenario: Se rechaza la autorización con delegación fuera de vigencia
    Dada una fecha del sistema del "2026-03-30"
    Cuando "Marvin Cálix" intenta autorizar el expediente "CHO-2026-00087" por delegación
    Entonces el sistema no ejecuta la autorización
    Y muestra "La delegación DEL-2026-0012 venció el 27/03/2026. Solicite un acto de delegación vigente o remita el expediente al titular."

  Escenario: Se rechaza registrar una delegación sin folio del acto que la confiere
    Cuando el Administrador del Sistema intenta registrar una delegación a favor de "Marvin Cálix" sin folio del acto
    Entonces el sistema rechaza el registro
    Y muestra "Indique el folio del acto administrativo que confiere la delegación. Una delegación sin acto no se registra (RN-07)."

  Escenario: Se rechaza registrar una delegación sin fecha de fin
    Cuando el Administrador del Sistema intenta registrar una delegación a favor de "Marvin Cálix" con vigencia abierta
    Entonces el sistema rechaza el registro
    Y muestra "La delegación requiere fecha de fin. No se admiten delegaciones de vigencia indefinida."

  Escenario: Se bloquea al delegado que es el solicitante de derecho
    Dada una fecha del sistema del "2026-03-18"
    Y un expediente cuyo solicitante de derecho es "Marvin Cálix"
    Cuando "Marvin Cálix" intenta autorizar el expediente por delegación
    Entonces el sistema no ejecuta la autorización
    Y muestra "La delegación de firma no levanta la segregación. Usted es el solicitante de derecho de este expediente (RN-01, RN-07)."

  Escenario: La bandeja del titular se muestra marcada como delegada
    Dada una fecha del sistema del "2026-03-18"
    Cuando "Marvin Cálix" abre la bandeja de pendientes de "Rolando Discua"
    Entonces el sistema muestra "Bandeja de Rolando Discua — usted actúa por delegación DEL-2026-0012, vigente hasta el 27/03/2026."

  Escenario: La autorización por delegación consta en el expediente y en el impreso
    Dada una fecha del sistema del "2026-03-18"
    Cuando "Marvin Cálix" autoriza el expediente "CHO-2026-00087" por delegación
    Entonces el expediente pasa a estado "APROBADA"
    Y registra "Autorizado por Marvin Cálix, por delegación de Rolando Discua, acto DEL-2026-0012 vigente del 16/03/2026 al 27/03/2026"
    Y esa constancia aparece en la versión impresa de la Orden de Misión

  Escenario: La delegación vencida no invalida lo ya autorizado
    Dada una autorización registrada el "2026-03-18" por delegación "DEL-2026-0012"
    Y una fecha del sistema del "2026-04-10"
    Cuando Auditoría Interna consulta el expediente "CHO-2026-00087"
    Entonces la autorización figura como válida
    Y muestra la vigencia que la delegación tenía a la fecha del acto
```

## Fuera de alcance

- La delegación de la **firma del permiso de circulación en día u hora inhábil**: esa facultad se trata como **indelegable** hasta que la institución confirme lo contrario — ver [HU-016](HU-016-tramite-y-firma-del-permiso-de-circulacion.md) e insumo #29
- La **captura por encargo**, que no es delegación de firma — es [HU-003](HU-003-captura-por-encargo-y-solicitante-de-derecho.md)
- El flujo administrativo con que la institución emite el acto de delegación: SIGTI registra el folio, no produce el acto
- La delegación de facultades distintas de autorizar solicitudes (despachar, entregar combustible, liquidar): fuera de esta historia

## Notas y pendientes

- `[C]` Si la institución emite actos de delegación con folio propio y de qué serie — insumo #1
- `[C]` Quién registra la delegación en el sistema: el Administrador del Sistema carga el dato, pero su puesta en vigencia requiere aprobación de Gerencia Administrativa por el doble control de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) — confirmar con la institución
- `[P]` La admisibilidad de la delegación de firma en actos internos se apoya en [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) y [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), con verificación parcial
- Trazabilidad: [CU-02](../casos-de-uso/CU-02-autorizar-solicitud-de-transporte.md) flujo alterno A4; punto de control `PC-16`
