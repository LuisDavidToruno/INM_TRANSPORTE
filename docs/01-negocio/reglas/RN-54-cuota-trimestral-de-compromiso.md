# RN-54 — El compromiso de gasto se valida contra la cuota trimestral, no solo contra el presupuesto anual

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-18, M-13, M-20 |
| **Origen** | Norma [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) — programación financiera de SIAFI, cuotas trimestrales de compromiso; decisión [DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — la estructura presupuestaria la posee ARGOS; [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md) — llega como espejo local |
| **Verificación** | `[V]` que el módulo de programación financiera de SIAFI asigna **cuotas trimestrales de compromiso** por Gerencia Administrativa, unidad ejecutora, clase de gasto y fuente de financiamiento: [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) marca `[V]` esa sección. `[I]` que SIGTI deba validar contra ella — es implicación de requerimiento escrita por el equipo, no articulado. `[C]` si ARGOS expone la cuota y el saldo comprometido del trimestre (insumo #16). `[C]` los topes específicos sobre combustible y vehículos del Acuerdo 360-2026 |
| **Tipo** | Advertencia con acuse, escalable a bloqueo por configuración |
| **Configurable** | Sí — `control_cuota_trimestral` con valores *no verificar / advertir / bloquear*, valor inicial **advertir**; ámbito por unidad ejecutora |

## Por qué existe esta regla — hallazgo `HN1-07`

> [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) quedó reducida por [DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md), pero conserva **una obligación expresamente dentro del alcance de SIGTI**:
>
> *"Lo que sí queda vigente para SIGTI: entender que el gasto está sujeto a **cuota trimestral de compromiso**, no solo a presupuesto anual. Ese dato viene de ARGOS, pero **SIGTI debe respetarlo al aprobar la asignación de fondos de combustible y peajes**."*
>
> Ninguna de las 53 reglas anteriores citaba `NRM-04`, y [RN-26](RN-26-fondo-de-combustible-aprobado.md) controla el saldo del **fondo** —objeto interno de SIGTI— que no es la cuota. El resultado práctico: la Gerencia Administrativa aprueba un fondo, SIGTI lo registra conforme a `RN-26`, y el compromiso no cabe en la cuota del trimestre. **El descuadre aparece en ARGOS o en SIAFI, no en SIGTI, y SIGTI queda como el sistema que lo permitió.**
>
> El silencio no era una decisión: o se escribe la regla, o se escribe con fundamento que el control es exclusivo de ARGOS. Se escribe la regla, con el control **configurable** porque depende de un dato que hoy no se sabe si ARGOS entrega.

## Enunciado

Todo acto de SIGTI que **compromete gasto** —aprobación de un fondo de combustible ([RN-26](RN-26-fondo-de-combustible-aprobado.md)), ampliación de fondo, y el estimado de peajes que se autoriza con la misión ([RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md))— **debe** verificarse contra **dos límites, no uno**:

| Límite | Origen del dato | Qué pasa si se excede |
|---|---|---|
| **Presupuesto anual** de la partida | Espejo de ARGOS | Según `control_cuota_trimestral` |
| **Cuota de compromiso del trimestre** en que cae la fecha del compromiso | Espejo de ARGOS | Según `control_cuota_trimestral` |

El trimestre aplicable se determina por la **fecha del hecho** que genera el compromiso —la fecha de aprobación del fondo o la de la misión autorizada—, nunca por la fecha de captura ([RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md)).

**SIGTI no calcula la cuota, no la modifica y no arbitra.** La consume del espejo, la muestra, la contrasta y deja constancia. [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) es explícita: SIGTI es **no autoritativo frente a SIAFI**; produce insumos y concilia, **nunca lo sustituye ni lo "corrige"**.

Si el dato de cuota **no está disponible** en el espejo, la verificación se registra como **no realizada, con su causa**, y el acto continúa. Un control que no se puede ejecutar se declara; no se finge cumplido ni detiene la operación de la institución por una frontera de integración que aún no existe.

## Justificación

[NRM-04](../normativa/NRM-04-presupuesto-siafi.md) `[V]`: *"el gasto en combustible y viáticos no está limitado solo por el presupuesto anual, sino por la cuota del trimestre. Un sistema que solo controla contra el presupuesto anual permitirá comprometer gasto que la institución no puede ejecutar."*

Es el patrón que produce el descuadre de fin de trimestre: la partida anual tiene saldo, la cuota no, y la institución termina con compromisos que no puede devengar. Quien responde por ese descuadre es la Gerencia Administrativa, y la pregunta que le harán es en qué sistema se aprobó el compromiso.

La verificación es barata —una comparación contra un dato que ya viene espejeado— y su ausencia es cara.

## Condiciones de aplicación

Aplica a los actos de SIGTI que comprometen gasto de combustible y de peajes.

**No aplica** al registro del **consumo** ya ocurrido: un consumo es un hecho, y un hecho se registra aunque exceda cualquier cuota. Lo que se controla es el **compromiso previo**, no la realidad consumada ([RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), principio de registro oportuno).

**No aplica** a los viáticos, que son de ARGOS ([DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).

`[C]` Si ARGOS expone cuota asignada y saldo comprometido por unidad ejecutora y trimestre, y con qué periodicidad se actualiza — insumo #16. De la respuesta depende si esta regla puede pasar a *bloquear*.

## Comportamiento esperado

1. Antes de aprobar un fondo o una ampliación, el sistema **muestra los dos saldos** —anual y del trimestre— con la fecha de última sincronización del espejo ([RN-49](RN-49-reconciliacion-periodica-del-espejo.md)). Un saldo sin fecha no es un saldo.
2. Si el monto excede la cuota del trimestre, el sistema informa **cuánto excede y de qué trimestre**: *"El fondo solicitado por L &lt;monto&gt; excede en L &lt;diferencia&gt; la cuota de compromiso del &lt;trimestre&gt; para la unidad ejecutora &lt;nombre&gt;, según ARGOS al &lt;fecha de sincronización&gt;."*
3. Con `control_cuota_trimestral` en *advertir* — su valor inicial —, la aprobación continúa con **acuse nominativo y motivo**, que quedan en el expediente del fondo. Con el valor *bloquear*, no continúa.
4. Toda verificación de cuota queda registrada con sus insumos: cuota consultada, saldo comprometido, monto del acto, trimestre, fecha de sincronización del espejo y resultado. Guardar solo "verificado" no defiende a nadie ante el TSC.
5. El sistema reporta, por unidad ejecutora y trimestre: cuota, comprometido por SIGTI, y actos aprobados por encima de cuota con su acuse. Es el cuadre que la Gerencia Administrativa lleva a la conciliación con ARGOS.
6. El cambio de `control_cuota_trimestral` es un parámetro sujeto a [RN-39](RN-39-parametros-normativos-con-vigencia.md): lo carga ACT-01 y lo pone en vigencia ACT-08. Apagar un control de dinero no puede ser acto de una sola persona.

## Casos límite

- **Misión que cruza el cierre de trimestre.** Sale el 28 de marzo y retorna el 3 de abril. El **compromiso** se imputa al trimestre de la fecha del acto que lo generó — la aprobación del fondo o de la misión —, no al de la fecha de retorno. El consumo posterior se liquida contra ese compromiso. `[C]` confirmar el criterio con la Gerencia Administrativa: es exactamente el tipo de detalle que cada institución resuelve distinto.
- **Cierre y apertura de ejercicio fiscal.** Misiones que cruzan el 31 de diciembre y fondos no liquidados al cierre. [NRM-04](../normativa/NRM-04-presupuesto-siafi.md) lo señala como requisito y **no se resuelve en esta regla**: pertenece a la liquidación y a la conciliación con ARGOS. Queda anotado para el bloque que trate M-13 a fondo.
- **ARGOS caído o espejo desactualizado al momento de aprobar.** No se detiene la aprobación: se advierte con la antigüedad del dato ([RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)) y se registra que la verificación se hizo sobre un saldo de fecha X.
- **Reprogramación de cuota a mitad de trimestre.** Ocurre. El dato nuevo llega por el espejo y **no reescribe** las verificaciones ya registradas: cada una conserva la cuota contra la que se evaluó ([RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](RN-41-congelamiento-del-valor-al-autorizar.md)).
- **Delegación con fondo propio.** La cuota se verifica contra la **unidad ejecutora** a la que se imputa el fondo, no contra la delegación que lo opera. `[C]` confirmar la correspondencia entre delegaciones y unidades ejecutoras.
- **Emergencia que exige comprometer por encima de la cuota.** No se resuelve apagando el parámetro. Se aprueba con acuse motivado y queda en el reporte del comportamiento 5, que es lo que la Gerencia Administrativa necesita para pedir la reprogramación en SIAFI.
- **La institución declara que el control de cuota es exclusivo de ARGOS.** Es una respuesta legítima bajo [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md). Entonces `control_cuota_trimestral` se fija en *no verificar* — con el doble control de [RN-39](RN-39-parametros-normativos-con-vigencia.md) — y la decisión queda **escrita y fechada**, que es lo que hoy faltaba.

## Trazabilidad

- Norma: [NRM-04 — Presupuesto y finanzas (SEFIN / SIAFI)](../normativa/NRM-04-presupuesto-siafi.md) — cuotas trimestrales de compromiso `[V]`
- Decisiones: [DP-001, D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md); [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md)
- Hallazgo que origina esta regla: `HN1-07` de [H-B1-002](../../05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md)
- Reglas relacionadas: [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-35](RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-39](RN-39-parametros-normativos-con-vigencia.md), [RN-40](RN-40-calculo-a-la-fecha-del-hecho.md), [RN-48](RN-48-datos-espejo-de-solo-lectura.md), [RN-50](RN-50-degradacion-por-sincronizacion-detenida.md)
- Actores: ACT-04, ACT-08, ACT-12
- Insumos: #16 (contrato de API de ARGOS), #7 (`PROP-01`)
- Historias y casos especiales: pendientes — Bloque 2
