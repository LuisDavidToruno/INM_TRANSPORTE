# CE-23 — El fondo se acabó el 18 y hay misiones programadas hasta fin de mes

| Campo | Valor |
|---|---|
| **Módulos** | M-09 Combustible, M-07 Programación y Despacho, M-13 Liquidación, M-18 Peajes, M-20 Integraciones |
| **Estados afectados** | `APROBADA` y `PROGRAMADA` (emisión del fondo, `PC-08`), `DESPACHADA` (entrega, `PC-08b` y `EF-04`); ciclo del fondo del período y `V-01` de la asignación (§10.1) |
| **Frecuencia** | **Frecuente.** Y estacional: se concentra en el último mes de cada trimestre y en el último trimestre del ejercicio |
| **Impacto** | Operativo, financiero, **presupuestario** y de auditoría |
| **Resolución** | Definida. Criterio de prelación `[C]` (insumo #31). Reintegro de peculio propio `[C]` (insumo #37) |

## La situación

18 de septiembre. El fondo de combustible del mes para la sede fue aprobado por Gerencia Administrativa en **L 180,000**. Saldo disponible hoy: **L 12,400**.

En la programación quedan comprometidas hasta fin de mes:

| Misión | Estimado combustible | Peajes |
|---|---|---|
| Entrega de equipo a la Delegación de Choluteca, mañana | L 4,800 | — |
| Comisión Tegucigalpa → San Pedro Sula → Puerto Cortés, tres días | L 9,200 | 6 cruces por Zambrano, Siguatepeque y Yojoa |
| Cuatro movilizaciones urbanas de la semana | L 6,000 | — |
| **Total** | **L 20,000** | |

Faltan **L 7,600** y sobran tres misiones. La de Choluteca sale mañana a las seis: el vehículo y el motorista están reservados desde hace cuatro días (`EF-01`), la delegación ya avisó que recibe, y el equipo está empacado.

**Y hay dos maneras completamente distintas de que un fondo esté agotado.** Hoy nadie las distingue, y confundirlas cuesta una semana:

| | Qué pasó | Qué se puede hacer |
|---|---|---|
| **Fondo agotado** | Se consumió el monto que Administración aprobó para el período. La partida tiene saldo y la cuota del trimestre tiene espacio | Ampliación por el mismo circuito. Con firmas, en dos días |
| **Cuota del trimestre copada** | La **cuota trimestral de compromiso** de la unidad ejecutora está agotada. La partida anual muestra saldo — y ese número engaña — pero el trimestre no admite un compromiso más | **No se resuelve en SIGTI ni en la institución sola.** Exige reprogramación de cuota en SIAFI, gestionada por Gerencia Administrativa. Eso no ocurre en dos días |

Estamos a 18 de septiembre. Faltan doce días para el cierre del tercer trimestre. Es exactamente cuando ocurre el segundo caso.

## Qué se hace hoy sin sistema

`[C]` No verificado. Se levanta con Gerencia Administrativa y el Encargado de Transporte (insumos #7 y #1).

Lo que se observa en instituciones comparables, y ninguna de las cuatro cosas deja rastro:

1. **Se estira el combustible.** Se manda el viaje con menos galones de los que corresponde y el motorista completa de su bolsillo, confiando en que se lo reintegran. A veces se lo reintegran.
2. **Se pide prestado.** A otra dependencia, a otra delegación, o de la cisterna si la hay. El combustible entra al tanque y **no pasa por ningún folio**.
3. **Se despacha contra el fondo del mes siguiente.** El vale se emite en octubre y el consumo ocurrió en septiembre. El compromiso queda imputado al trimestre equivocado.
4. **Se cancelan misiones.** Y aquí está lo que importa: **se cancela la del que no llamó al gerente.**

> **Las dos reglas que nadie escribió.** La primera: *existe una prelación real cuando el dinero no alcanza, y hoy el criterio es quién presiona más.* La segunda: *el préstamo de combustible entre dependencias existe, es común, y no queda registrado en ninguna parte.*
>
> La segunda es la causa del síntoma que [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) detecta como *rendimiento imposiblemente bueno*: el microbús que hizo 23 km/galón cargó combustible que nadie anotó, y lo cargó porque su fondo estaba agotado.

## Por qué el flujo normal no lo cubre

Tres razones, y las tres son huecos reales.

**Primera: el bloqueo cae sobre una cartera, no sobre una misión.** [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) bloquea la asignación cuando no hay saldo suficiente, con `tolerancia_sobregiro` en cero. Está bien. Pero el flujo feliz imagina ese bloqueo sobre una solicitud aislada, y aquí cae sobre **siete misiones ya aprobadas y programadas**, con vehículo y motorista reservados, alguna con salvoconducto emitido y alguna con personas externas ya citadas. [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) resuelve el caso límite con una línea —*"se solicita ampliación, que sigue el mismo circuito"*— y **no dice qué pasa con las misiones mientras la ampliación se tramita, ni cuál se sacrifica si no llega.**

**Segunda: el saldo que el sistema muestra es cierto y es engañoso.** [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) calcula saldo como *aprobado − asignado + devoluciones liquidadas*. Ese número no ve la cartera comprometida. El 12 de septiembre el fondo mostraba L 34,000 y ya tenía L 41,000 en misiones programadas sin asignación emitida. Nadie mintió y la decisión de programar fue equivocada.

**Tercera: "fondo agotado" y "cuota copada" se le presentan al usuario igual.** [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) incorporó la validación contra la cuota trimestral, pero ninguna regla obliga a **distinguir las dos causas ante quien opera**. Decirle "fondo agotado" al Jefe de Transporte cuando lo agotado es la cuota lo manda a pedir una ampliación que nadie le puede aprobar, y pierde la semana que necesitaba para gestionar la reprogramación.

Y un cuarto punto, que no es hueco sino desconocimiento: **`PC-08` ya dice que el bloqueo es de la emisión del fondo, no de la misión.** Despachar sin fondo asignado *"es posible y queda como decisión registrada con responsable"*. Esa puerta existe, tiene su control, y hoy nadie sabe que está ahí ni bajo qué condiciones se usa.

## Regla de resolución

**1. El sistema dice cuál de los dos techos se topó, con el número de cada uno.** Nunca "fondo agotado" a secas:

| Restricción | Mensaje y salida |
|---|---|
| **Saldo del fondo agotado**, cuota del trimestre con espacio | Ampliación por el circuito de [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md): solicita ACT-04, aprueba ACT-08, **con segregación verificada por identidad de persona** — quien solicita la ampliación no la aprueba, y ninguno de los dos liquida el fondo |
| **Cuota trimestral copada** ([RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)) | *"Excede en L &lt;diferencia&gt; la cuota de compromiso del &lt;trimestre&gt; para la unidad ejecutora &lt;nombre&gt;, según ARGOS al &lt;fecha de sincronización&gt;."* **No se resuelve en SIGTI.** El sistema produce el reporte de comprometido por unidad ejecutora y trimestre que Gerencia Administrativa lleva a la gestión de reprogramación |
| **Dato de cuota no disponible** en el espejo | La verificación se registra como **no realizada con su causa** y el acto continúa ([RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)), advirtiendo la antigüedad del dato ([RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)). Un control que no se puede ejecutar se declara; no se finge cumplido |

**2. La alerta llega antes del agotamiento y sobre el saldo proyectado.** Umbral configurable —porcentaje de saldo o días de consumo promedio— que avisa a ACT-04 y ACT-08 mostrando **saldo contable menos estimados de las misiones aprobadas y programadas sin asignación emitida**, incluidos los peajes estimados por [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md). Un saldo de L 12,400 con L 20,000 comprometidos no es un saldo de L 12,400.

**3. Consolidar antes que cancelar.** Antes de plantear el sacrificio de una misión, el sistema ofrece las **consolidaciones posibles** (M-07): misiones con destino compatible y ventana traslapada bajo un mismo vehículo, con el ahorro estimado de cada una. Es el único camino que no le cuesta nada a nadie, y hoy se descubre por conversación de pasillo o no se descubre.

**4. Prelación explícita, decidida por persona, registrada.** Cuando el saldo no alcanza para la cartera, **el sistema no cancela nada por sí solo**. Presenta las misiones ordenadas por el criterio que la institución configuró, y ACT-04 decide con acuse motivado que queda en el expediente de cada misión afectada.

Criterio candidato, `[C]` a confirmar (insumo #31):

- misión con personas externas ya citadas o compromiso con terceros notificado
- misión con fecha inamovible: audiencia, entrega contractual, operativo interinstitucional
- antigüedad de la solicitud
- misión consolidable con otra ya financiada

Lo importante no es el orden exacto. Es que **el orden esté escrito antes de que haga falta**: en el momento lo resuelve quien tenga más jerarquía, y eso es precisamente lo que el sistema debe evitar.

**5. El bloqueo es de la emisión, no de la misión.** La misión sin fondo **no se anula sola**: queda en `PROGRAMADA` con la marca *sin fondo asignado*. Despachar así es posible como decisión registrada con responsable nominado y motivo (`PC-08`) — `[C]` confirmar si la institución lo admite, que el propio punto de control deja abierto. Si se despacha sin fondo, el consumo que ocurra **se registra igual** ([RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)) y se imputa al fondo que se constituya después, con la observación arrastrada hasta la liquidación. *Un hecho se registra aunque exceda cualquier cuota; lo que se controla es el compromiso previo* ([RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)).

**6. Se le cierra la puerta al préstamo invisible.** Todo combustible que entra al tanque de un vehículo institucional se registra como **abastecimiento con su fuente declarada** —otro fondo, otra dependencia, cisterna institucional, peculio del motorista— aunque no exista folio de este fondo. Es la candidata `RN-C21a` de [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md), y este caso es la segunda evidencia de que hace falta: allá era el síntoma, aquí es la causa.

**7. Nada se resuelve apagando el control.** `tolerancia_sobregiro` no se sube "por esta vez" y `control_cuota_trimestral` no se pone en *no verificar* para dejar pasar una misión. Ambos están sujetos a [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md): los carga ACT-01 y los pone en vigencia ACT-08. **Apagar un control de dinero no puede ser acto de una sola persona.** La salida legítima es el acuse motivado, que además es lo que sustenta el pedido de reprogramación de cuota.

**8. Misión que cruza el cierre de trimestre.** Sale el 28 de septiembre, retorna el 3 de octubre. El compromiso se imputa al **trimestre del acto que lo generó**, no al del retorno ([RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)). `[C]` confirmar el criterio con Gerencia Administrativa: es el tipo de detalle que cada institución resuelve distinto.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C23a` | *El saldo de un fondo se presenta siempre acompañado del **comprometido proyectado**: los estimados de combustible y peaje de las misiones aprobadas y programadas que aún no tienen asignación emitida. La alerta de agotamiento se dispara sobre el saldo proyectado, no sobre el contable.* | [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) comportamiento 2 define el saldo como *aprobado − asignado + devoluciones*. Ese número es correcto y ciego a la cartera. Ninguna regla obliga a mirar el agregado, que es donde se ve venir el problema con dos semanas de anticipación |
| `RN-C23b` | *Cuando el saldo no alcanza para la cartera de misiones programadas, el sistema presenta la cartera ordenada por el criterio de prelación configurado por la institución, y la decisión sobre cuáles se financian la toma una persona con acuse motivado registrado. El sistema no cancela misiones por sí solo ni ordena por jerarquía del solicitante.* | El insumo #31 registra la pregunta de prelación **para el vehículo** ([CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md)). El mismo hueco existe para el dinero y ninguna regla lo cubre: [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) solo bloquea |
| `RN-C23c` `[C]` | *El consumo de combustible pagado por el servidor de su propio peculio se registra como abastecimiento con fuente declarada; su reintegro, si la institución lo admite, sigue un circuito con segregación propia — quien autoriza el reintegro no es el beneficiario ni quien liquida.* | Insumo #37, abierto. La práctica existe con o sin regla. Si no se modela, el galón que el motorista pagó de su bolsillo desaparece del denominador de [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) |

### `[C]` Escalado al PO

| Decisión | Opciones y costo |
|---|---|
| **¿Se admite despachar sin fondo asignado?** (`PC-08` lo deja abierto) | **Sí, con responsable nominado**: la operación no se paraliza, pero hay dinero público comprometido sin cobertura previa y el TSC lo va a preguntar. **No**: control impecable y la delegación de Choluteca no recibe su equipo. Se propone *sí, con responsable nominado, motivo y marca visible hasta la liquidación* |
| **¿Cuál es el criterio de prelación?** (insumo #31) | Sin criterio escrito, decide la jerarquía. Costo de definirlo: una reunión. Costo de no definirlo: el sistema hereda el problema que venía a resolver |
| **¿Se reintegra combustible pagado de peculio propio?** (insumo #37) | **Sí**: hay que modelar el circuito de reembolso con su comprobación. **No**: hay que decirlo por escrito, porque la práctica ocurre igual y hoy queda fuera de todo registro |

## Evidencia que debe quedar

Ante el TSC — y antes que él, ante la propia Gerencia Administrativa en el cuadre del trimestre:

1. El **expediente del fondo del período** completo: aprobado, ampliaciones, asignado, consumido, devuelto y saldo, con fecha y actor de cada movimiento, y con la segregación *solicita ≠ aprueba ≠ liquida* verificada por identidad de persona
2. Por cada ampliación: solicitud de ACT-04 con su justificación operativa, aprobación de ACT-08, partida afectada, y **la verificación de cuota trimestral con todos sus insumos** — cuota consultada, saldo comprometido, monto del acto, trimestre, fecha de sincronización del espejo y resultado ([RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)). Guardar solo "verificado" no defiende a nadie
3. La lista de **misiones no ejecutadas o reprogramadas por falta de fondo**, con la decisión, el responsable y el motivo. Es lo que responde la pregunta *"¿por qué no se ejecutó lo programado?"* y lo que sostiene la solicitud de reprogramación de cuota
4. Las misiones **despachadas sin fondo asignado**, si las hubo: quién lo autorizó, con qué motivo, y contra qué fondo se imputó después el consumo
5. Todo **abastecimiento de fuente distinta al fondo**, con su fuente declarada — el préstamo entre dependencias deja de ser invisible
6. El **reporte por unidad ejecutora y trimestre**: cuota, comprometido por SIGTI, y actos aprobados por encima de cuota con su acuse ([RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)). Es el cuadre que va a la conciliación con ARGOS
7. Y la correlación de siempre: **consumo del período contra kilometraje contra misiones autorizadas**. Un fondo agotado explica por qué se movilizó menos; **no** puede ser la explicación de un período sin conciliación

## Trazabilidad

- **Autoridad de transiciones:** [`PC-08` emisión del fondo y `PC-08b` entrega](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) — el bloqueo es de la emisión, no de la misión; [`EF-01` reserva al programar, `EF-04` entrega al despachar, `T-08`, `T-12`, y §10.1 `V-01` emitir con folio](../../03-arquitectura/estados/orden-de-mision.md)
- **Reglas:** [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) y [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) (reglas eje), [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [RN-32](../../01-negocio/reglas/RN-32-entrega-de-combustible-contra-orden-de-mision.md), [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md), [RN-49](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md), [RN-50](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md), [RN-02](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md)
- **Reglas candidatas:** `RN-C23a`, `RN-C23b`, `RN-C23c`, y `RN-C21a` de [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md)
- **Normas:** [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) `[V]` que SIAFI asigna cuotas trimestrales de compromiso — `[I]` que SIGTI deba validar contra ellas, es implicación del equipo, no articulado; [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md); [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) para el estimado de peajes de la cartera
- **Decisiones:** [DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (SIGTI no compra combustible), [DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (la estructura presupuestaria es de ARGOS), [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), `PROP-01`
- **Actores:** ACT-04 (solicita ampliación y prioriza), ACT-07 (custodia y entrega el fondo), ACT-08 (aprueba y gestiona la cuota), ACT-10 (delegación con fondo propio), ACT-12
- **Casos relacionados:** [CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) — mismo hueco de prelación, sobre el vehículo; [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md); [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — este caso es la causa de aquel síntoma
- **Insumos:** #7 / `PROP-01` (fondo por período o por misión, sobrante, saldo entre misiones), #16 (si ARGOS expone cuota y comprometido del trimestre — de eso depende que [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) pueda pasar a *bloquear*), #31 (criterio de prelación), #37 (reembolso de peculio propio), #27 (delegaciones con fondo propio)
