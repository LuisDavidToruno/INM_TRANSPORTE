# RN-56 — La adjudicación de un recurso escaso aplica el criterio de prelación parametrizado y deja constancia de las solicitudes desplazadas

| Campo | Valor |
|---|---|
| **Módulos** | M-07, M-06, M-09, M-14 |
| **Origen** | Casos especiales [CE-12](../../02-requisitos/casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md), [CE-16](../../02-requisitos/casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) y [CE-23](../../02-requisitos/casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de criterios documentados para la asignación de recursos públicos. `[C]` el criterio concreto de prelación — insumo #31, decisión del PO |
| **Tipo** | Derivación + bloqueo duro sobre la constancia |
| **Configurable** | Sí — parámetro `criterio_prelacion` con vigencia por rango de fechas |

## Enunciado

Cuando dos o más solicitudes aprobadas requieren el **mismo recurso escaso** —el único vehículo compatible disponible, o el saldo remanente de un fondo de combustible— y no existe otro que las satisfaga a todas, el sistema **debe** presentar la cartera ordenada por el **criterio de prelación parametrizado con vigencia**, y:

1. **Ninguna adjudicación se completa sin registrar el criterio aplicado y la lista de solicitudes desplazadas**
2. **La decisión la toma una persona con acuse motivado registrado.** El sistema no adjudica por sí solo, no cancela misiones por sí solo y **no ordena por jerarquía del solicitante**
3. **Apartarse del orden propuesto es admisible y exige justificación registrada**

La solicitud desplazada **conserva su aprobación**, vuelve a la cola con marca y contador de desplazamientos, y su eventual caducidad se anula con motivo tipificado *sin recurso disponible* — nunca por vencimiento silencioso.

## Justificación

Ninguna de las 54 reglas originales cubre la prelación ni el registro del desplazamiento. [`RN-13`](RN-13-sin-doble-asignacion.md) impide la doble asignación, que es **la consecuencia del conflicto, no su resolución**. [`RN-26`](RN-26-fondo-de-combustible-aprobado.md) bloquea cuando no hay saldo, y tampoco dice quién se queda sin él.

Sin criterio escrito, la adjudicación la resuelve el orden de llegada a la pantalla de programación o —lo que es peor y ocurre— la jerarquía de quien solicita. Es exactamente el problema que el sistema venía a resolver, heredado con otro nombre.

El registro del desplazamiento tiene un destinatario concreto: la dependencia desplazada. Sin notificación con acuse, su reclamo no tiene contraparte y el hecho no existe para nadie más que para ella.

## Condiciones de aplicación

Aplica al conflicto por vehículo, por motorista y por saldo de fondo.

Aplica también cuando el conflicto lo origina una **indisponibilidad sobrevenida** ([`RN-60`](RN-60-indisponibilidad-sobrevenida-y-reservas.md)) o una **extensión de misión** que invade la reserva de otra ([`RN-77`](RN-77-versionado-del-alcance-autorizado.md)).

**No aplica** cuando existe recurso para todas: ahí no hay adjudicación que justificar.

## Comportamiento esperado

1. Detectado el conflicto, el sistema **evalúa primero la consolidación**: si dos solicitudes pueden atenderse en un solo viaje, lo propone. La evaluación queda registrada aunque no proceda — es la defensa contra el hallazgo *"se pudo hacer un solo viaje y se hicieron dos"*.
2. Si no procede, presenta la cartera ordenada por `criterio_prelacion` **a la fecha del hecho** ([`RN-40`](RN-40-calculo-a-la-fecha-del-hecho.md)), con los datos que sustentan el orden.
3. La adjudicación registra en el diario: criterio invocado, lista de desplazadas con su referencia, quién decidió, cuándo y —si se apartó del orden— la justificación.
4. Cada dependencia desplazada recibe **notificación con acuse**: cuándo se le informó y quién lo hizo.
5. El expediente de la solicitud desplazada acumula su historial: cuántas veces fue desplazada, por qué misiones y en qué fechas. Ese contador **entra en el criterio** si la institución así lo define.
6. El sistema produce el **reporte de demanda no atendida** por dependencia, tipo de vehículo y período, y el **reporte de disponibilidad de flota** — vehículos por estado, días de indisponibilidad y órdenes de trabajo abiertas. Juntos son la prueba de que el único vehículo compatible era realmente el único.

## Casos límite

- **`[C]` El criterio de prelación no está definido — insumo #31.** Es el `[C]` que aparece la primera semana de operación. Opciones y costo, para decisión del PO:

  | Opción | Costo |
  |---|---|
  | Orden de llegada de la solicitud | Verificable y neutro, pero ignora la urgencia real y premia al que solicita primero por costumbre |
  | Fecha de necesidad más próxima | Refleja la operación, pero se manipula declarando urgencia |
  | Jerarquía del solicitante | **Debe descartarse expresamente**: convierte en comportamiento del sistema una práctica cuestionada |
  | Tipo de misión ponderado por catálogo | Requiere que la institución clasifique y pondere sus motivos de viaje; es trabajo previo real |
  | Orden por defecto verificable, con desviación justificada y registrada | Es el mecanismo estándar del control interno: da un orden y hace que apartarse cueste una justificación |

  **Recomendación del análisis, no decisión:** la última. **Costo de no decidir:** el sistema queda sin criterio y adjudica el orden de llegada a la pantalla, que es la tercera opción con otro nombre.
- **Conflicto disparado desde la carretera** por una extensión de misión. La extensión **nunca** desplaza automáticamente a la otra: el sistema abre el conflicto y lo resuelve ACT-04, con ACT-08 si hay que escalar prioridad.
- **Solicitud desplazada que ya no tiene sentido** cuando el recurso se libera. Se anula con motivo tipificado, no caduca en silencio.
- **Recurso escaso que es el dinero.** El mismo mecanismo, con el saldo proyectado de [`RN-88`](RN-88-saldo-proyectado-del-fondo.md) como base. **Nada se resuelve apagando el control**: no se sube la tolerancia de sobregiro ni se desactiva el control de cuota trimestral ([`RN-54`](RN-54-cuota-trimestral-de-compromiso.md)) para dejar pasar una misión.
- **Una sola persona decide y también es la solicitante de una de las carteras en conflicto.** [`RN-01`](RN-01-segregacion-de-funciones.md) lo impide; la decisión escala ([`RN-02`](RN-02-escalamiento-de-autorizacion.md)).

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-02](RN-02-escalamiento-de-autorizacion.md), [RN-13](RN-13-sin-doble-asignacion.md), [RN-19](RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-54](RN-54-cuota-trimestral-de-compromiso.md), [RN-60](RN-60-indisponibilidad-sobrevenida-y-reservas.md), [RN-88](RN-88-saldo-proyectado-del-fondo.md)
- Casos especiales: [CE-12](../../02-requisitos/casos-especiales/CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — candidatas `RN-56` y `RN-57` originales · [CE-16](../../02-requisitos/casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) · [CE-23](../../02-requisitos/casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) `RN-C23b`
- Insumos pendientes: #31 criterio de prelación
- Actores: ACT-04 adjudica · ACT-08 escala prioridad · ACT-03 dependencia desplazada
