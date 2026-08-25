# HU-135 — Constituir el acto de delegación: solo lo que se tiene, a un puesto, con acciones enumeradas y vigencia con fecha de fin

| Campo | Valor |
|---|---|
| **Módulo** | M-01 Organización y Seguridad |
| **Actor** | ACT-08 Gerencia Administrativa · ACT-03 Jefatura Inmediata (como puesto delegante) |
| **Prioridad** | Alta |
| **Sprint** | sin asignar |
| **Estado** | Borrador — tres condiciones de la delegación siguen `[C]` en la autoridad: si es al puesto o nominativa, si admite subdelegación y hasta qué nivel se puede delegar |

## Historia

**Como** titular de un puesto con facultad de autorizar
**quiero** constituir un acto de delegación que enumere exactamente qué acciones delego, en qué puesto, con qué límites y hasta qué fecha
**para** que la delegación sea un acto administrativo verificable y no una nota por correo, y para que el auditor que encuentre una firma que no es la del titular sepa de inmediato por qué es válida

## Contexto

[HU-015](HU-015-autorizacion-por-delegacion-de-firma.md) cubre el **uso** de la delegación al autorizar una solicitud, y deja fuera de alcance —textualmente— *"la delegación de facultades distintas de autorizar solicitudes (despachar, entregar combustible, liquidar)"* y el acto que la constituye. Esta historia cubre la **constitución del acto**, para cualquier facultad.

Siete condiciones que lo hacen auditable ([actores-y-roles §7.1](../../01-negocio/actores-y-roles.md), autoridad):

1. **Solo se delega lo que se tiene** a la fecha de la delegación.
2. **Se delega en un puesto**, no en una persona `[C]`.
3. **Ámbito acotado y enumerado.** Nunca *"todas mis facultades"*.
4. **Vigencia con fecha de inicio y fin, ambas requeridas.**
5. **Sin subdelegación** `[C]`.
6. **No levanta incompatibilidades**, y eso se verifica **también en el acto de delegar**.
7. **Revocable en cualquier momento**, sin invalidar lo ya ejecutado.

Y una facultad que hoy se trata como **indelegable**: la firma del permiso de circulación en día u hora inhábil, mientras no se confirme lo contrario.

## Reglas que la gobiernan

- [RN-07](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) — Vigencia acotada, constancia en el expediente, no rompe la segregación
- [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) — La verificación de incompatibilidad opera también al delegar, no solo al usar la delegación
- [RN-03](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) — El acto de delegación se registra con identidad, momento y huella del contenido
- [RN-23](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) — El permiso de día u hora inhábil lo firma la máxima autoridad; su delegabilidad no consta
- [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) — La vigencia de la delegación se evalúa a la fecha del acto delegado

## Casos especiales que la afectan

- Ninguno de los 28 vigentes. **Caso especial candidato:** *cambia el ocupante del puesto delegado durante la vigencia de la delegación* — [actores-y-roles §7.1](../../01-negocio/actores-y-roles.md) lo deja `[C]`: si la delegación sigue con el puesto, el acto lo firma alguien a quien el delegante nunca eligió

## Criterios de aceptación

```gherkin
# language: es
Característica: Constitución del acto de delegación de autoridad

  Antecedentes:
    Dado un puesto delegante "Subgerente de Operaciones" con el rol "ACT-03 Jefatura Inmediata" vigente
    Y un puesto delegado "Coordinador de la Unidad de Trámites", inmediato inferior del anterior
    Y un parámetro "duracion_maxima_delegacion" de "60" días vigente y aprobado

  Escenario: Se rechaza delegar una facultad que el delegante no tiene
    Cuando el titular de "Subgerente de Operaciones" delega la acción "aprobar fondo de combustible"
    Entonces el sistema rechaza la constitución
    Y muestra "El puesto Subgerente de Operaciones no ostenta la facultad de aprobar fondo de combustible al 01/10/2026. Solo se delega lo que se tiene."

  Escenario: Se rechaza la delegación sin acciones enumeradas
    Cuando el titular constituye una delegación con ámbito "todas mis facultades"
    Entonces el sistema rechaza la constitución
    Y muestra "Enumere las acciones delegadas una por una. No se admite la delegación de la totalidad de las facultades del puesto."

  Escenario: Se rechaza la delegación sin fecha de fin
    Cuando el titular constituye una delegación desde el "2026-10-01" sin fecha de fin
    Entonces el sistema rechaza la constitución
    Y muestra "La delegación exige fecha de inicio y fecha de fin. No se admiten delegaciones indefinidas."

  Escenario: Se rechaza la delegación que excede el tope de duración
    Cuando el titular constituye una delegación del "2026-10-01" al "2027-03-31"
    Entonces el sistema rechaza la constitución
    Y muestra "La delegación duraría 182 días y el máximo configurado es de 60. Renuévela con una nueva resolución motivada si la ausencia se prolonga."

  Escenario: Se rechaza delegar la firma del permiso de circulación en día u hora inhábil
    Dado un puesto delegante "Máxima Autoridad" con la facultad de firmar el permiso
    Cuando el titular la delega en el puesto "Gerente Administrativo"
    Entonces el sistema rechaza la constitución
    Y muestra "La firma del permiso de circulación en día u hora inhábil es facultad expresa de la máxima autoridad. Su delegabilidad no consta en la norma y el sistema la trata como indelegable hasta que la institución lo confirme."

  Escenario: Se rechaza delegar en un puesto que ya ostenta el rol incompatible
    Dado que el puesto "Coordinador de la Unidad de Trámites" ostenta "ACT-07 Encargado de Combustible"
    Cuando el titular le delega la acción "liquidar misión"
    Entonces el sistema rechaza la constitución
    Y muestra "El puesto Coordinador de la Unidad de Trámites entrega el fondo de combustible. La incompatibilidad I-10 —entrega fondo × liquida— es núcleo irreductible y la delegación no la levanta."

  Escenario: Se rechaza la subdelegación
    Dada una delegación vigente "DEL-2026-0034" a favor de "Coordinador de la Unidad de Trámites"
    Cuando su titular intenta delegar a su vez la acción recibida en otro puesto
    Entonces el sistema rechaza la constitución
    Y muestra "La facultad que usted ejerce proviene de la delegación DEL-2026-0034. No se admite subdelegación."

  Escenario: Se constituye la delegación con folio, ámbito y motivo
    Cuando el titular de "Subgerente de Operaciones" delega las acciones "autorizar solicitud de transporte" y "devolver solicitud para corrección" en "Coordinador de la Unidad de Trámites" del "2026-10-01" al "2026-10-20" con motivo "comisión oficial en el exterior"
    Entonces el sistema constituye la delegación con folio "DEL-2026-0034"
    Y registra puesto delegante, puesto delegado, acciones, límites, vigencia, motivo, autor y momento
    Y la delegación aparece en el tablero del Auditor Interno

  Escenario: La revocación surte efecto inmediato sin invalidar lo ejecutado
    Dada una delegación "DEL-2026-0034" vigente y 7 autorizaciones ejecutadas bajo ella
    Cuando el titular la revoca el "2026-10-12" con motivo "retorno anticipado de la comisión"
    Entonces el delegado no puede autorizar a partir de ese momento
    Y las 7 autorizaciones anteriores conservan su validez y su constancia de delegación
    Y el sistema muestra "Delegación DEL-2026-0034 revocada el 12/10/2026. 7 actos ejecutados bajo ella conservan plena validez."
```

## Fuera de alcance

- El uso de la delegación al autorizar una solicitud y su bandeja — es [HU-015](HU-015-autorizacion-por-delegacion-de-firma.md)
- La suplencia del puesto completo, que es otra figura — es [HU-134](HU-134-declarar-suplencia-con-vigencia-acotada.md)
- El trámite administrativo con que la institución emite el acto en papel: SIGTI registra el folio, no produce el acto
- La captura por encargo, que no es delegación — es [HU-003](HU-003-captura-por-encargo-y-solicitante-de-derecho.md)

## Notas y pendientes

- `[V]` Que el permiso de circulación en día u hora inhábil lo firma la máxima autoridad — [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md). `[C]` **su delegabilidad**: mientras no se confirme, indelegable — insumo **#29**
- `[C]` **¿Se delega en un puesto o debe ser nominativa a una persona?** Si es al puesto y el ocupante cambia durante la vigencia, el acto lo firma alguien a quien el delegante nunca eligió — insumo **#K de `actores-y-roles`**, pendiente de trasladar
- `[C]` **¿Hasta qué nivel jerárquico se puede delegar?** [actores-y-roles §7.1](../../01-negocio/actores-y-roles.md) propone *"mismo nivel o inmediato inferior"* marcado `[C]`
- `[C]` `duracion_maxima_delegacion` — el "60" es dato de prueba — insumo **#32**
- `[C]` Si la institución emite actos de delegación con folio propio y de qué serie — insumo **#1**
- `[P]` La admisibilidad de la delegación de firma en actos internos se apoya en [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) y [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md), con verificación parcial
