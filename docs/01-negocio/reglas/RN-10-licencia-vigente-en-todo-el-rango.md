# RN-10 — La licencia del motorista debe estar vigente durante todo el rango de la misión, no solo el día de salida

| Campo | Valor |
|---|---|
| **Módulos** | M-05, M-07 |
| **Origen** | Norma [NRM-06](../normativa/NRM-06-transito-y-licencias.md); decisión [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) |
| **Verificación** | `[P]` la exigencia de licencia vigente y habilitante — [NRM-06](../normativa/NRM-06-transito-y-licencias.md) deja pendiente el texto reformado del Art. 48 (insumo #20). `[I]` la formulación *"bloquear si la licencia estará vencida en cualquier fecha del rango"*: es **implicación de requerimiento escrita por el equipo**, no articulado citable. Corregido desde `[V]` por la regla de no escalar el nivel de verificación |
| **Tipo** | Bloqueo duro |
| **Configurable** | **No** |

## Enunciado

El sistema **no debe** permitir asignar ni despachar un motorista cuya licencia venza en **cualquier fecha comprendida entre la fecha de salida y la fecha prevista de retorno**, ambas inclusive, más la holgura de retorno configurada `[C]` insumo #1.

**Todo cambio de la ventana de la misión revalida la vigencia contra la nueva fecha de fin** — prórroga, destino adicional que la desplace, reprogramación o extensión por cualquier causa — y una licencia que quede vencida dentro del nuevo rango **bloquea el cambio**. La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), autoridad en transiciones, lo exige en `T-17`.

La comprobación se hace sobre **quien efectivamente conduce**, no sobre quien ostenta el puesto de motorista ([RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md)).

Esta regla gobierna la **asignación**. El vencimiento **sobrevenido** con la misión ya `EN_RUTA` se rige por [RN-55](RN-55-habilitacion-vencida-durante-la-mision.md).

## Justificación

[NRM-06](../normativa/NRM-06-transito-y-licencias.md): *"bloquear si la licencia estará vencida en cualquier fecha del rango de la misión"*. La formulación es deliberada. Validar solo la fecha de salida es el error clásico: una misión de cinco días que sale el día anterior al vencimiento deja al motorista conduciendo cuatro días con licencia vencida, un vehículo del Estado sin cobertura y a la institución con responsabilidad directa.

Para el TSC y la DNVT lo relevante es el día del hecho, no el día del despacho.

## Condiciones de aplicación

Aplica a toda asignación, despacho, sustitución en ruta y extensión de misión.

Aplica a **todas** las categorías que la asignación necesita: si la habilitación se sostiene en la categoría C y esa categoría vence a mitad del rango, bloquea, aunque otras categorías de la misma licencia sigan vigentes.

## Comportamiento esperado

1. La comprobación se hace sobre `[fecha_salida, fecha_retorno_prevista]` y se **repite en el despacho** contra las fechas reales, no solo en la programación.
2. El mensaje de bloqueo es concreto: *"La licencia N.º <número>, categoría <X>, del motorista <nombre> vence el <fecha>. La misión retorna el <fecha>. No puede ser asignado (RN-10)."*
3. El sistema propone **motoristas alternos habilitados** en el mismo acto de bloqueo. Bloquear sin alternativa es lo que empuja a operar fuera del sistema.
4. Las alertas anticipadas de vencimiento de [RN-17](RN-17-alertas-de-vencimiento-documental.md) deben haber avisado antes: esta regla es la última línea, no la primera.
5. Toda comprobación queda registrada con la fecha de vencimiento consultada y el origen del dato (espejo de Talento Humano o expediente propio de SIGTI).

## Casos límite

- **Misión sin fecha de retorno cierta** — comisión abierta, apoyo indefinido a un operativo. El sistema exige una **fecha máxima prevista**; sin ella no hay rango que evaluar. La misión se extiende por actos sucesivos, cada uno revalidado.
- **La licencia vence exactamente el día de retorno.** Vence *el* día: se considera vigente hasta el final de ese día salvo que la norma diga lo contrario. `[C]` confirmar el criterio de vencimiento (inicio o fin del día) contra el texto de la Ley de Tránsito. Hasta entonces el sistema aplica el criterio configurado en parámetro `criterio_vencimiento_licencia`, con valor inicial *fin del día* y advertencia visible.
- **El motorista renueva la licencia durante la misión.** Real y frecuente. No basta la promesa: la extensión se autoriza cuando el dato renovado consta en el expediente, con adjunto. Mientras tanto, se bloquea y se sustituye ([RN-14](RN-14-sustitucion-de-motorista.md)).
- **Licencia vencida detectada en ruta.** El vehículo no puede seguir conducido por esa persona. Se registra el evento como incidente de M-12, se sustituye al motorista si es posible, y si no lo es, se registra la decisión operativa adoptada y quién la tomó. La orden se cierra con hallazgo ([RN-08](RN-08-cadena-de-trazabilidad-para-cierre.md)).
- **Dato de vencimiento desactualizado** porque la sincronización con Talento Humano está detenida. Aplica [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md): superado el umbral, la asignación de motoristas se bloquea antes que operar sobre datos de habilitación no confiables.
- **Misión que cruza un cambio de régimen de categorías** por reforma a la Ley de Tránsito. La vigencia de la licencia y la matriz son parámetros distintos: se evalúa la vigencia con el dato del documento, y la habilitación con la matriz vigente a la fecha del hecho ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)).

## Trazabilidad

- Norma: [NRM-06 — Tránsito, licencias, matrícula y siniestros](../normativa/NRM-06-transito-y-licencias.md)
- Decisión: [DP-001, D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)
- Reglas relacionadas: [RN-09](RN-09-matriz-licencia-vehiculo.md), [RN-14](RN-14-sustitucion-de-motorista.md), [RN-17](RN-17-alertas-de-vencimiento-documental.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md), [RN-55](RN-55-habilitacion-vencida-durante-la-mision.md), [RN-57](RN-57-habilitacion-de-quien-efectivamente-conduce.md), [RN-77](RN-77-versionado-del-alcance-autorizado.md)
- Actores: ACT-04, ACT-05, ACT-06
- Casos especiales: [CE-11](../../02-requisitos/casos-especiales/CE-11-licencia-vence-durante-la-mision.md), [CE-06](../../02-requisitos/casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md), [CE-19](../../02-requisitos/casos-especiales/CE-19-vehiculo-asignado-a-funcionario-frente-al-pool.md)
- Historias: pendientes — Bloque 4
