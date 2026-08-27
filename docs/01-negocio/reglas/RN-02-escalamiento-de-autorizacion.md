# RN-02 — Cuando el autorizador natural es el solicitante, la autorización escala al nivel inmediato superior

| Campo | Valor |
|---|---|
| **Módulos** | M-01, M-06, M-20 |
| **Origen** | Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) — MARCI, norma de segregación de funciones incompatibles `[C]` su numeración; complemento operativo de [RN-01](RN-01-segregacion-de-funciones.md) |
| **Verificación** | `[V]` la exigencia de autorización por servidor competente — `[C]` la jerarquía concreta de la institución |
| **Tipo** | Derivación + bloqueo duro |
| **Configurable** | Sí — la cadena de escalamiento es dato del espejo de ARGOS, parámetro `cadena_autorizacion` |

## Enunciado

Cuando el servidor que solicita una Orden de Misión es también el autorizador que correspondería según la cadena de autorización, el sistema **debe** desviar la solicitud al **nivel inmediato superior** de esa cadena, y **no debe** presentarla nunca al solicitante para su propia aprobación.

Si el solicitante es la **máxima autoridad** (ACT-09) y no existe nivel superior, la autorización recae en el **órgano de control definido por la institución** para ese supuesto. `[C]`

## Justificación

La segregación de [RN-01](RN-01-segregacion-de-funciones.md) bloquea, pero bloquear sin ofrecer camino produce el peor resultado posible: el usuario opera fuera del sistema y registra después. El escalamiento convierte el bloqueo en un flujo transitable.

[NRM-01](../normativa/NRM-01-control-interno-tsc.md) exige que toda salida de vehículo esté autorizada por **servidor competente**. Competente significa dos cosas a la vez: con nivel suficiente **y** sin conflicto de interés sobre el acto.

## Condiciones de aplicación

Aplica cuando se detecta identidad entre solicitante y autorizador en cualquier punto de la cadena, incluidos los niveles intermedios de aprobación múltiple.

**No aplica** cuando el solicitante actúa como capturador en nombre de otro servidor: en ese caso el solicitante de derecho es el servidor que requiere la movilización, y el capturador queda registrado como tal. El sistema **debe** distinguir ambos campos.

## Comportamiento esperado

1. Al enviar la solicitud, el sistema resuelve la cadena de autorización contra el espejo de ARGOS ([ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)).
2. Si el primer autorizador coincide con el solicitante, **avanza al siguiente nivel** y lo deja asentado: *"Escalado a <cargo> por coincidencia entre solicitante y autorizador (RN-02)."*
3. El escalamiento es **visible en el expediente y en el documento impreso**, no silencioso: quien reciba la orden en carretera debe poder ver por qué firmó quien firmó.
4. Si la cadena se agota sin encontrar autorizador válido, el sistema **bloquea** y muestra la ruta evaluada completa, para que el problema se resuelva en la configuración y no en el expediente.
5. El escalamiento **no altera** los niveles de monto ni de alcance definidos en ARGOS: el nivel superior autoriza con sus propias facultades.

## Casos límite

- **Toda la cadena está en la misma persona** (dependencia unipersonal). Bloqueo, con mensaje que identifica el punto de ruptura. `[C]` la institución debe designar un autorizador alterno por dependencia; sin eso, la dependencia no puede operar.
- **El nivel superior está de vacaciones o incapacitado.** La solicitud **escala al inmediato superior de ése**, por la misma vía y con el mismo fundamento que el resto de esta regla. Lo que **no** ocurre es un salto que se brinque niveles ni la aparición de una competencia que la jerarquía no da.

> **Corrección — hallazgo `HN1-16`. No eran dos posiciones incompatibles.**
>
> Este caso límite decía que la ausencia *«se resuelve con [`RN-07`](RN-07-delegacion-de-autorizacion.md), delegación vigente; **no con salto de nivel automático**»*, mientras [`actores-y-roles` §7.3](../actores-y-roles.md) prescribe que las solicitudes de un puesto ausente *«escalan automáticamente al puesto superior»*. El revisor las dio por irreconciliables y dejó la elección al PO.
>
> **Al leerlas juntas se ve que no chocan: mezclaban dos cosas distintas.**
>
> | | Qué hace | Qué necesita |
> |---|---|---|
> | **Delegar** ([`RN-07`](RN-07-delegacion-de-autorizacion.md)) | Da la facultad a alguien que **no la tendría** — un par, un subordinado, otro puesto | **Un acto**: vigencia, ámbito enumerado, autor |
> | **Escalar** (esta regla) | **Enruta** la decisión a quien **ya es competente** por jerarquía | Nada nuevo: la competencia ya existe |
>
> Y esta misma regla ya escala al inmediato superior sin acto de delegación cuando el solicitante es el autorizador. **Si eso vale ahí, vale igual ante una ausencia registrada:** en ambos casos el autorizador natural no puede actuar, y en ambos la decisión va a quien la jerarquía ya faculta.
>
> Lo que `RN-02` protegía —*«una autorización necesita competencia, y la competencia nace de un acto»*— sigue protegido, porque **el acto es el nombramiento en el puesto**, no una delegación adicional. Lo que sí queda prohibido es lo que la frase decía mal: **saltarse niveles**.
>
> `actores-y-roles` §7.3 lo remata con la garantía que faltaba decir en voz alta: *«ninguna misión queda trabada por una ausencia; ninguna autorización aparece firmada por quien no la firmó»*. El superior autoriza **con su propia identidad**, y el expediente registra que llegó ahí por escalamiento y por qué ausencia.
>
> `[C]` El plazo tras el cual escala sigue pendiente — pendiente **H** de `actores-y-roles` §9. Y la cadena concreta es dato del espejo de ARGOS, insumo **#16**.
- **La máxima autoridad solicita para sí.** No hay superior jerárquico. `[C]` confirmar con la institución quién autoriza: en la práctica suele ser el propio acuerdo de nombramiento o una instancia colegiada. Hasta que se confirme, el sistema exige registrar el **fundamento documental** del acto y marca la orden para revisión de ACT-12 Auditor Interno.
- **El autorizador es el cónyuge, pariente o subordinado directo del solicitante.** El conflicto de interés por parentesco **no** se detecta automáticamente: SIGTI no conoce esos vínculos. Se resuelve con declaración de excusa del autorizador, que el sistema registra y que dispara el escalamiento por la misma vía. `[C]` confirmar si la institución tiene régimen de excusa formal.
- **La cadena de ARGOS está desactualizada** porque la sincronización lleva días detenida. Aplica [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md): se advierte y, superado el umbral, se bloquea la autorización antes que autorizar contra una jerarquía que ya no existe.
- **Solicitud consolidada de varias dependencias.** El escalamiento se evalúa por cada solicitud componente; basta un conflicto para escalar la orden completa.

## Trazabilidad

- Norma: [NRM-01 — Control interno y auditoría](../normativa/NRM-01-control-interno-tsc.md)
- Decisión: [DP-001, D-05](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — niveles de autorización espejados de ARGOS
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-07](RN-07-delegacion-de-autorizacion.md), [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)
- Actores: ACT-02, ACT-03, ACT-08, ACT-09, ACT-12
- Historias y casos especiales: pendientes — Bloque 2
