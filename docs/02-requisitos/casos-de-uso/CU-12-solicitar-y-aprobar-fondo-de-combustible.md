# CU-12 — Solicitar y aprobar el fondo de combustible del período

| Campo | Valor |
|---|---|
| **Módulo** | M-09 Combustible |
| **Actor principal** | `ACT-04` Jefe de Transporte — solicita |
| **Actores secundarios** | `ACT-08` Gerencia Administrativa — aprueba · `ACT-07` Encargado de Combustible — recibe la custodia · `ACT-10` Encargado de Delegación — fondo de ámbito delegación · `ACT-16` Sistema ARGOS — partida, presupuesto anual y cuota trimestral · `ACT-12` Auditor Interno — consulta |
| **Precondiciones** | Existe un período de vigencia definido para el fondo y un ámbito al que imputarlo — institución, dependencia o delegación. El espejo de ARGOS tiene disponible la estructura presupuestaria ([`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)). El fondo del período anterior está cerrado o su estado es conocido |
| **Postcondiciones** | Existe un **fondo aprobado vigente** con monto, partida afectada, período, ámbito, aprobador y fecha, y con la verificación de cuota trimestral registrada con todos sus insumos. El instrumento —efectivo u órdenes de pago— queda bajo custodia de `ACT-07`. A partir de ese momento se pueden emitir asignaciones de misión ([CU-13](CU-13-emitir-y-entregar-asignacion-de-combustible.md)). En caso contrario, la solicitud queda denegada o devuelta, con motivo registrado |
| **Disparador** | Inicio del período operativo, o alerta de agotamiento disparada sobre el **saldo proyectado** del fondo vigente ([`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md)) |

> **SIGTI no compra combustible ni gestiona contratos con proveedores.** [DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) y `PROP-01` de [insumos-pendientes](../../07-gestion/insumos-pendientes.md) definen el alcance: la institución aprueba un **monto en efectivo o una cantidad de órdenes de pago**; SIGTI modela el fondo, su saldo, su consumo y su conciliación. Lo que aquí se solicita y se aprueba es dinero disponible, no una compra.

> **Este caso de uso no ejecuta ninguna transición `T-nn` de la Orden de Misión.** El fondo del período es un objeto propio de M-09 con su propio ciclo —solicitado → aprobado → entregado → agotado → cerrado ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), comportamiento 1)—, distinto de la máquina de la **asignación** de §10.1 de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md), que es por misión. Su punto de contacto con el proceso es `PC-08`: sin fondo vigente con saldo, no hay emisión.

## Flujo principal

1. `ACT-04` abre la solicitud de fondo para el período. El sistema propone el ámbito según su alcance de datos —institución o delegación, [actores-y-roles §3.2](../../01-negocio/actores-y-roles.md)— y el período según el parámetro `periodicidad_fondo` ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md)).
2. El sistema muestra el **cierre del fondo vigente o anterior**: aprobado, ampliaciones, asignado, consumido, comprobado, devuelto y saldo, con el detalle **por persona de lo que está afuera** ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md), comportamiento 6). Un fondo nuevo no se pide a ciegas sobre el anterior.
3. El sistema presenta el **saldo proyectado** del fondo vigente ([`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md)): saldo contable menos los estimados de combustible y de peajes ([`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md)) de las misiones `APROBADA` y `PROGRAMADA` que aún no tienen asignación emitida. *Un saldo de L 12,400 con L 20,000 comprometidos no es un saldo de L 12,400* — [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md).
4. `ACT-04` registra: monto o cantidad de órdenes de pago, **composición del instrumento** —efectivo, órdenes de pago o ambos—, justificación operativa del período, y la partida propuesta tomada del espejo de ARGOS ([`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md)). La solicitud queda en estado *solicitado*.
5. El sistema encamina la solicitud a `ACT-08`. `PC-16`: se registra persona, puesto, rol, marca de tiempo, origen y huella del contenido ([`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md)).
6. `ACT-08` abre la solicitud y ve, **antes de decidir**, los dos límites presupuestarios con su fecha de sincronización del espejo ([`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md), [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md)):

   | Límite | Origen |
   |---|---|
   | Presupuesto anual de la partida | Espejo de `ACT-16` |
   | **Cuota de compromiso del trimestre** en que cae la fecha del acto | Espejo de `ACT-16` |

   *Tener saldo en la partida anual no significa que el compromiso quepa en el trimestre.* El trimestre aplicable se determina por la **fecha del hecho** que genera el compromiso, nunca por la fecha de captura ([`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
7. El sistema verifica la **segregación propia del expediente del fondo**, por identidad de persona: quien solicita el fondo no puede ser quien lo aprueba, y ninguno de los dos puede ser quien lo liquida al cierre del período ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), numeral 4 del enunciado — **bloqueo duro no configurable**).
8. `ACT-08` aprueba. El sistema registra monto aprobado, fecha, aprobador, **partida contra la que se afecta**, período de vigencia, ámbito, y el resultado de la verificación de cuota **con todos sus insumos**: cuota consultada, saldo comprometido, monto del acto, trimestre, fecha de sincronización del espejo y resultado ([`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md), comportamiento 4). *Guardar solo "verificado" no defiende a nadie.*
9. Se registra la **entrega del efectivo o de las órdenes de pago a `ACT-07`**, que a partir de ahí lo tiene bajo custodia física ([`ACT-07`](../../01-negocio/actores-y-roles.md), límite duro). El fondo pasa a *entregado*.
10. El sistema publica el **saldo disponible** = aprobado − asignado + devoluciones liquidadas ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), comportamiento 2), y activa la alerta de agotamiento sobre el **proyectado**, no sobre el contable ([`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md)).
11. Desde este punto `ACT-07` puede emitir asignaciones contra el fondo — continúa en [CU-13](CU-13-emitir-y-entregar-asignacion-de-combustible.md).

## Flujos alternos

**A1 — Ampliación del fondo agotado a mitad de período** (desde el paso 1) · [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md)

1. La alerta de saldo proyectado dispara antes del agotamiento contable.
2. `ACT-04` solicita **ampliación**, que sigue **el mismo circuito** de este caso de uso, con la misma segregación verificada por identidad de persona ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md)).
3. Mientras la ampliación se tramita, **las misiones programadas no se anulan solas**: quedan en `PROGRAMADA` con la marca *sin fondo asignado* (`PC-08`).
4. Si el saldo no alcanza para la cartera completa, el sistema presenta las misiones ordenadas por el **criterio de prelación configurado** ([`RN-56`](../../01-negocio/reglas/RN-56-prelacion-entre-solicitudes-que-compiten.md)) y `ACT-04` decide con acuse motivado que queda en el expediente de cada misión afectada. **El sistema no cancela ninguna misión por sí solo.** `[C]` criterio de prelación — insumo #31.
5. Antes de plantear el sacrificio de una misión, el sistema ofrece las **consolidaciones posibles** con su ahorro estimado (M-07). Es el único camino que no le cuesta nada a nadie.

**A2 — Fondo de ámbito delegación** (desde el paso 1)

1. `ACT-10` solicita el fondo de su delegación. Las asignaciones solo pueden imputarse a fondos **de su ámbito**.
2. La **aprobación sigue siendo de `ACT-08`, sin excepción posible** ([actores-y-roles §5.4, Nivel 1](../../01-negocio/actores-y-roles.md); [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)). Aprobar un fondo no exige presencia física, y por eso es una de las tres funciones que se sacan de la delegación.
3. La cuota se verifica contra la **unidad ejecutora** a la que se imputa el fondo, no contra la delegación que lo opera ([`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)). `[C]` correspondencia entre delegaciones y unidades ejecutoras — insumo #27.

**A3 — Aprobación por encima de la cuota trimestral, con acuse** (desde el paso 8)

1. Con `control_cuota_trimestral` en *advertir* —su valor inicial—, el sistema informa **cuánto excede y de qué trimestre**, con la fecha de sincronización del espejo.
2. `ACT-08` continúa con **acuse nominativo y motivo escrito**, que quedan en el expediente del fondo.
3. El acto entra en el **reporte por unidad ejecutora y trimestre**: cuota, comprometido por SIGTI, y actos aprobados por encima de cuota con su acuse. Ese reporte es lo que sustenta el pedido de reprogramación de cuota ante SIAFI, gestionado por Gerencia Administrativa.

**A4 — Composición mixta del instrumento** (desde el paso 4)

1. Efectivo y órdenes de pago simultáneamente son **dos instrumentos con conciliación distinta**.
2. El fondo registra la composición, y **cada asignación indica con qué instrumento se hizo** ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), casos límite).

**A5 — Cambio de Jefe de Transporte o de Encargado de Combustible con fondo abierto** (en cualquier paso)

1. El fondo **no se cierra por rotación**. Se registra el traspaso de responsabilidad con **acta y saldo verificado**, análogo a la custodia del vehículo ([`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)).
2. La custodia física de efectivo, órdenes de pago o vales emitidos es **bloqueo duro del cierre de la asignación de puesto** ([actores-y-roles §2.4](../../01-negocio/actores-y-roles.md)).

## Flujos de excepción

**E1 — Quien solicita el fondo es quien lo aprobaría** (en el paso 7)

1. **Bloqueo duro no configurable.** El sistema impide consumar la aprobación y **no guarda nada**.
2. El mensaje nombra el conflicto con precisión: *"Usted registró la solicitud de fondo FND-2026-09-004 el 15/09/2026. No puede aprobarla."*
3. El intento se registra en la pista de auditoría con el par detectado, y se genera **tarea de resolución en el puesto competente**: escalamiento a la dependencia matriz ([`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md)) conforme a [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md).
4. *El fondo es dinero: aquí la segregación es más importante, no menos.*

> **Nota de hallazgo — `HB4-01`.** El par *solicita fondo × aprueba fondo* **no existe** en la tabla `I-01` a `I-17` de [actores-y-roles §5.2](../../01-negocio/actores-y-roles.md), que es la **autoridad en incompatibilidades**. Hoy vive únicamente en el numeral 4 del enunciado de [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), que lo declara control propio porque `RN-01` razona *por Orden de Misión* y el fondo es objeto *de período*. La propia `RN-26` lo dejó señalado en su nota `HN1-15`. **No se resuelve aquí:** este caso de uso no es autoridad en la materia. Queda dirigido a `actores-y-roles.md` para que incorpore el par —y con él `I-17` *propone descargo × aprueba descargo*, que tiene el mismo problema—.

**E2 — La cuota trimestral está copada, no el fondo** (en el paso 6) · [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md)

1. El sistema **distingue las dos causas ante quien opera**, y nunca dice "fondo agotado" a secas: *"Excede en L &lt;diferencia&gt; la cuota de compromiso del &lt;trimestre&gt; para la unidad ejecutora &lt;nombre&gt;, según ARGOS al &lt;fecha de sincronización&gt;."*
2. **Esto no se resuelve en SIGTI ni en la institución sola.** Exige reprogramación de cuota en SIAFI, gestionada por `ACT-08`.
3. El sistema produce el reporte de comprometido por unidad ejecutora y trimestre que `ACT-08` lleva a esa gestión.
4. Decirle "fondo agotado" al Jefe de Transporte cuando lo agotado es la cuota lo manda a pedir una ampliación que nadie le puede aprobar, y le hace perder la semana que necesitaba.

**E3 — El dato de cuota no está disponible en el espejo** (en el paso 6)

1. La verificación se registra como **no realizada, con su causa**, y el acto continúa ([`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)).
2. Se advierte la antigüedad del dato disponible ([`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md)).
3. *Un control que no se puede ejecutar se declara; no se finge cumplido ni detiene la operación de la institución por una frontera de integración que aún no existe.*

**E4 — Fondo aprobado sin partida presupuestaria resuelta** (en el paso 8)

1. La estructura presupuestaria la define ARGOS ([DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). Si el espejo no la tiene disponible, el fondo se registra con **partida pendiente**.
2. Se **bloquea el cierre del fondo** hasta que la partida se complete. La operación no se detiene; el cierre sí ([`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), casos límite).

**E5 — El fondo del período anterior tiene asignaciones sin liquidar** (en el paso 2)

1. El cierre del fondo anterior exige que **todas** sus asignaciones estén liquidadas ([`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)) o formalmente anuladas.
2. Un fondo anterior sin cerrar **no impide aprobar el siguiente**, pero se muestra con su detalle de saldos afuera por persona.
3. Las personas con **obligación de reintegro abierta o saldo vencido** quedan bloqueadas para recibir nueva asignación ([`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)); el levantamiento es acto registrado de `ACT-08` con motivo, nunca decisión de quien programa.

**E6 — Alguien intenta resolver el problema apagando el control** (en cualquier paso)

1. `tolerancia_sobregiro` **no se sube "por esta vez"** y `control_cuota_trimestral` **no se pone en *no verificar*** para dejar pasar una misión.
2. Ambos son parámetros sujetos a [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md): los **carga** `ACT-01` con respaldo documental y los **pone en vigencia** `ACT-08` ([actores-y-roles §4.3](../../01-negocio/actores-y-roles.md)). **Apagar un control de dinero no puede ser acto de una sola persona.**
3. El cambio queda en el histórico de parámetros, que `ACT-12` ve como objeto de auditoría de primera clase.

## Reglas aplicables

| Regla | Qué aporta a este caso |
|---|---|
| [`RN-26`](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) | Circuito solicitud → aprobación → entrega; saldo disponible; **segregación propia del fondo** (numeral 4, bloqueo duro no configurable) |
| [`RN-54`](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) | Doble límite: presupuesto anual **y cuota trimestral de compromiso**; verificación registrada con todos sus insumos |
| [`RN-88`](../../01-negocio/reglas/RN-88-saldo-proyectado-del-fondo.md) | La alerta se dispara sobre el saldo **proyectado**, no sobre el contable |
| [`RN-56`](../../01-negocio/reglas/RN-56-prelacion-entre-solicitudes-que-compiten.md) | Prelación cuando el dinero no alcanza para la cartera, con constancia de las desplazadas |
| [`RN-86`](../../01-negocio/reglas/RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md) | Arqueo del fondo con detalle por persona; bloqueo de nueva asignación a quien debe |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md) | Doble control sobre `tolerancia_sobregiro` y `control_cuota_trimestral` |
| [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) · [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) | El trimestre se determina por la fecha del hecho, no por la de captura |
| [`RN-48`](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) · [`RN-49`](../../01-negocio/reglas/RN-49-reconciliacion-periodica-del-espejo.md) · [`RN-50`](../../01-negocio/reglas/RN-50-degradacion-por-sincronizacion-detenida.md) | La partida y la cuota son espejo de ARGOS; un saldo sin fecha no es un saldo |
| [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) · [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) | Registro inmutable de la aprobación; toda corrección es asiento reverso |
| [`RN-02`](../../01-negocio/reglas/RN-02-escalamiento-de-autorizacion.md) | Escalamiento cuando el aprobador natural es el solicitante |
| [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) | Modelo de acta de traspaso aplicado al fondo en la rotación de personal |

## Trazabilidad

- **Proceso:** [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) — insumo previo a E7; punto de control **`PC-08`** (saldo suficiente en el fondo vigente como precondición de la emisión)
- **Autoridad en transiciones:** [orden-de-mision.md §10.1](../../03-arquitectura/estados/orden-de-mision.md) — el ciclo de la **asignación**; el ciclo del **fondo del período** vive en M-09 y no en esa máquina
- **Autoridad en actores e incompatibilidades:** [actores-y-roles.md](../../01-negocio/actores-y-roles.md) — matriz de permisos filas 8 y 9; §4.3 doble control de parámetros; §5.4 Nivel 1
- **Casos especiales:** [CE-23](../casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) (eje del caso), [CE-26](../casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md), [CE-27](../casos-especiales/CE-27-cierre-de-ejercicio-fiscal-con-hallazgo-abierto.md)
- **Casos de uso encadenados:** → [CU-13](CU-13-emitir-y-entregar-asignacion-de-combustible.md) emisión y entrega · [CU-15](CU-15-liquidar-la-mision-y-conciliar.md) liquidación
- **Normativa:** [NRM-04](../../01-negocio/normativa/NRM-04-presupuesto-siafi.md) `[V]` que SIAFI asigna cuotas trimestrales de compromiso — `[I]` que SIGTI deba validar contra ellas, es implicación de requerimiento del equipo, no articulado · [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) `[P]` segregación y control de fondos entregados a servidores
- **Decisiones:** [DP-001 D-03](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (SIGTI no compra combustible), [DP-001 D-09](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) (la estructura presupuestaria es de ARGOS), [DP-002](../../07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md), [ADR-001](../../03-arquitectura/adr/ADR-001-integracion-argos-talento-humano.md), `PROP-01`
- **Historias:** pendientes — no escritas en este bloque
- **Insumos pendientes:** #7 / `PROP-01` (¿fondo por período o por misión? ¿saldo acumulado entre misiones? ¿el sobrante se devuelve o se arrastra? ¿la orden de pago tiene folio preimpreso?) · #16 (si ARGOS expone cuota y comprometido del trimestre — de eso depende que `RN-54` pueda pasar a *bloquear*) · #31 (criterio de prelación) · #27 (delegaciones con fondo propio y su unidad ejecutora)
