# CE-26 — Al liquidar la misión sobra dinero del fondo, o falta

| Campo | Valor |
|---|---|
| **Módulos** | M-09 Combustible, M-13 Liquidación, M-12 Incidentes y Sanciones, M-14 Auditoría, M-15 Formatos Oficiales |
| **Estados afectados** | `RETORNADA` (durante `T-19`), `LIQUIDADA`, y el desenlace `CERRADA` o `CERRADA_CON_HALLAZGO`. En la asignación de fondo ([§10.1](../../03-arquitectura/estados/orden-de-mision.md)): `CONSUMIDA`, `DEVUELTA`, `EXTRAVIADA`, `LIQUIDADA` |
| **Frecuencia** | El sobrante, **en casi todas las misiones**. El faltante, ocasional — y cuando ocurre, siempre importa |
| **Impacto** | Financiero, legal y de auditoría — hay dinero público en la mano de una persona |
| **Resolución** | Definida. Plazo de devolución, tolerancia y destino del sobrante `[C]` — decisiones abiertas de `PROP-01`, insumo #7 |

## La situación

**Sobrante y faltante no son el mismo caso con el signo cambiado.** Uno es un movimiento de caja; el otro es dinero público que no volvió. El sistema no puede tratarlos con la misma pantalla ni con el mismo trámite.

### El sobrante — el caso de todos los viernes

Misión de tres días a la delegación de Trojes, El Paraíso. Pickup `INS-PU-021`. El Encargado de Combustible entrega el fondo con folio `FC-2026-0412` por **L 4,500**, estimado sobre 62 galones al precio del boletín de la semana. El motorista firma la recepción el martes a las 6:15 de la mañana.

Vuelve el jueves a las 8:40 de la noche. Trae tres comprobantes: Texaco Danlí L 1,760, Uno El Paraíso L 1,320, Puma salida de Tegucigalpa L 780. **Suman L 3,860.** Le sobran **L 640 en efectivo**.

La caja de la Encargada de Combustible cierra a las 4:00 de la tarde. **El motorista se va a su casa el jueves con L 640 del Estado en la bolsa del pantalón**, y los devuelve el lunes a las 9 de la mañana — si el lunes no hay otra misión, y si no se le olvida.

Y hay una arruga más, que aparece siempre: de esos L 640 devuelve **L 500**, porque en Danlí compró una faja de alternador por L 140 para no quedarse tirado. Trae la factura. **El fondo es de combustible; la faja no es combustible, y en ninguna casilla del formato cabe.**

### El faltante — el caso que nadie quiere levantar

Misión a Puerto Lempira, ocho días, fondo por **L 6,000**. Al retornar: comprobantes por L 4,200 y devolución de L 900. **Faltan L 900** y la explicación es una de estas, y cada una es un expediente distinto:

| Lo que dice el motorista | Qué es en realidad |
|---|---|
| "Cargué en Puerto Lempira y no me dieron factura, solo un papel escrito a mano" | Consumo sin comprobante — [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md) |
| "Se me perdió el sobre" | Extravío, con acta. `V-06` de la [§10.1](../../03-arquitectura/estados/orden-de-mision.md) |
| "Me asaltaron en la terminal de Ceiba" | Incidente con denuncia — M-12 |
| "Pagué el hospedaje del ayudante porque no salió el viático" | Uso del fondo para un fin distinto al autorizado |
| Nada. Baja la mirada | **Faltante puro.** Es una obligación de reintegro a cargo de una persona con nombre |

Las cinco terminan igual en el cuadre — un hueco de L 900 — y **son cinco cosas jurídicamente distintas**. Un sistema que las guarda todas como "diferencia" le entrega al auditor un número sin significado.

### Y el faltante al revés

El motorista puso **L 350 de su bolsillo** en Iriona porque el fondo se acabó y había que volver. No es un consumo del fondo: es una **obligación de reintegro a favor del servidor**. `[C]` insumo #37 — si la institución admite y reembolsa esta figura. Si no se modela, se registra mal: se anota como consumo del fondo y el cuadre miente en los dos lados.

## Qué se hace hoy sin sistema

`[C]` No verificado con la institución — insumos #1 (reglamento interno), #2 (formatos en papel), #7 / `PROP-01`. Lo que se observa en instituciones comparables `[I]`:

- **El sobrante se devuelve en efectivo, en sobre, contra una firma en un cuaderno.** No siempre hay acta con folio. Cuando el monto es pequeño, con frecuencia **se arrastra**: queda en poder del motorista o del Encargado de Transporte "para la próxima". Esa es la regla que nadie escribió, y es exactamente la pregunta que `PROP-01` dejó abierta: *¿el sobrante se devuelve o se arrastra?*
- **Entre el retorno y la devolución hay un hueco de días que no está en ningún registro.** El dinero existe, no está en la caja, no está comprobado, y **para el papel no está en ninguna parte**. Ese hueco es donde nace el faltante.
- **El gasto imprevisto en ruta se resuelve con criterio y sin formato.** Una faja, una grúa, un parche de llanta, el lavado antes de devolver el vehículo. A veces se acepta con la factura; a veces se le descuenta al motorista; a veces se le devuelve el dinero de otro fondo.
- **El faltante se conversa antes de escribirse.** Se le da tiempo al motorista para que "lo reponga". Si repone, no queda registro de que hubo faltante. Si no repone, aparece la deducción por planilla — `[C]` si la institución la aplica y bajo qué procedimiento; requiere pronunciamiento de Auditoría Interna, no de Transporte.

**"Si repone, no queda registro" es el hallazgo.** Un control interno que se activa solo cuando la persona no coopera no es un control.

## Por qué el flujo normal no lo cubre

Tres razones, y ninguna se arregla con validaciones de formulario.

**1. El flujo feliz asume que la liquidación ocurre en el momento del retorno.** No ocurre. El retorno lo registra el motorista desde el campo, de noche, sin conectividad (`T-18`). La devolución del efectivo es un acto **presencial, en horario de caja, ante otra persona**. Son dos hechos con fechas distintas y [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) obliga a distinguirlas. Entre ambos el expediente queda en `RETORNADA` con dinero afuera, y **la máquina de la asignación no tiene ningún estado que represente esa situación** ([§10.1](../../03-arquitectura/estados/orden-de-mision.md) va de `CONSUMIDA` directo a `LIQUIDADA`).

**2. La identidad de [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) cuadra en los dos casos, y en uno de ellos no debería tranquilizar a nadie.**

```
monto asignado = monto consumido comprobado + saldo devuelto + diferencia explicada
```

`4,500 = 3,860 + 640 + 0` cuadra. `6,000 = 4,200 + 900 + 900 "explicada"` **también cuadra**, si alguien escribe cualquier cosa en el campo de motivo. La identidad no distingue una explicación de una excusa: eso lo hace la **tipificación del motivo**, y cada tipo tiene consecuencias jurídicas propias.

**3. Segregación.** Devolver dinero es un acto entre dos personas. Quien recibe la devolución no puede ser quien la entregó, ni quien liquida, ni el propio motorista — `I-08` e `I-10`, `PC-09` y `PC-13` de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md). En la sede eso son cuatro personas disponibles. **En una delegación de tres, no.** Ese es el insumo #26 y este caso lo toca de frente: si no hay a quién devolverle el dinero, el dinero se queda con el motorista.

## Regla de resolución

### 1. El saldo no devuelto es un estado, no un vacío

Se incorpora a la máquina de la asignación de fondo el estado **`PENDIENTE_DE_DEVOLUCION`**: hubo consumo parcial, la misión retornó, y hay dinero público en poder de una persona nominada.

| Atributo | Contenido |
|---|---|
| Monto | Asignado menos comprobado, calculado por el sistema |
| Responsable | La persona que firmó la recepción (`V-02`), no el rol |
| Fecha de inicio del plazo | La **fecha del hecho del retorno** (`T-18`), no la de captura ni la de sincronización |
| Plazo de devolución | Parámetro con vigencia, en días hábiles. `[C]` insumo #32 |
| Estado del plazo | Dentro de plazo · vencido |

Este estado es **visible en el tablero del Jefe de Transporte y de la Gerencia Administrativa**, agregado por persona: *quién tiene cuánto dinero del Estado en la mano, desde cuándo*. Hoy ese dato no existe en ninguna parte, y es la primera pregunta de un arqueo.

Vencido el plazo sin devolución ni comprobación, se dispara **`H-04`** ([§7.2](../../03-arquitectura/estados/orden-de-mision.md)) — *fondo entregado no devuelto ni comprobado al vencer el plazo de liquidación*, sin umbral. Entonces `T-21` deja de estar disponible y el único cierre posible es `T-22`.

### 2. El sobrante se resuelve con acta, y el acta es un documento con folio

La devolución es un acto propio, no un campo de la liquidación:

- Se registra con **folio del rango de la delegación**, monto, quién devuelve, **quién recibe** y fecha del hecho ([RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) para la versión impresa con QR).
- Exige **constancia de recepción** con la segregación de [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md): quien recibe ≠ quien entregó el fondo ≠ quien liquida ≠ el motorista.
- **La devolución parcial es válida y se registra tal cual.** Devolver L 500 de L 640 no es un error de captura: es una devolución de L 500 y un saldo de L 140 que sigue vivo, con su propia explicación pendiente. El sistema nunca redondea ni ajusta para cuadrar.
- Si la institución **arrastra** el saldo en vez de devolverlo, el acto es una **reasignación del saldo a la siguiente misión**, con folio nuevo y el mismo responsable — no es una devolución y no se registra como tal. `[C]` cuál de los dos regímenes rige, `PROP-01` / insumo #7. **El diseño soporta los dos; el parámetro decide, y el parámetro tiene vigencia por fecha** ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)).

**El sobrante, por sí solo, no es hallazgo.** Sobrar es lo normal: la estimación se hace con el precio del boletín y la ruta autorizada, y la realidad casi siempre cuesta menos. Marcar hallazgo por sobrar enseña a los motoristas a gastar el fondo completo, que es el incentivo exactamente contrario al que se busca.

Lo que sí se vigila es el **patrón agregado**: si la estimación sobra de forma sistemática por encima de un umbral configurable, el problema no está en la misión sino en la fórmula de estimación, y eso es una alerta al Jefe de Transporte — no un hallazgo contra el motorista.

### 3. El faltante se tipifica antes de calcularse, y cada tipo tiene su camino

El sistema **no ofrece un campo de diferencia en blanco**. Ofrece un catálogo configurable de motivos, y cada motivo declara qué respaldo exige, si constituye hallazgo y qué expediente abre:

| Motivo tipificado | Respaldo exigido | Hallazgo | Qué abre |
|---|---|---|---|
| Variación de precio respecto al estimado | Comprobante con precio unitario | No | Nada. Se recalcula contra el precio vigente a la fecha del hecho ([RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)) |
| Consumo sin comprobante | Declaración del motorista, odómetro y estación | Sí, `H-08` | Ver [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md) |
| Extravío del instrumento o del efectivo | Acta de extravío. `[C]` si exige denuncia — insumo #1 | Sí, `H-04` | `V-06` → `V-08`; expediente en M-12 |
| Robo o asalto | Denuncia ante autoridad competente | Sí, `H-06` | Expediente de incidente M-12 |
| Gasto imprevisto en ruta distinto de combustible | Factura a nombre de la institución y autorización del acto | `[C]` | Ver regla candidata `RN-C26c` |
| Fondo aplicado a un fin distinto al autorizado | Descargo del servidor | Sí, `H-04` | Deducción de responsabilidad — M-12 |
| **Sin causa identificada** | Descargo del servidor | Sí, `H-04` | **Obligación de reintegro a cargo de persona nominada** |
| Redondeo dentro de tolerancia | Ninguno | No | Nada. `tolerancia_diferencia_liquidacion`, valor inicial cero ([RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)) |

**El motivo "sin causa identificada" existe y es obligatorio que exista.** Si el catálogo no lo tiene, el liquidador elige el motivo más cercano que no genere problema, y el faltante desaparece del reporte.

### 4. La obligación de reintegro sobrevive al cierre de la misión

Este es el punto que el flujo feliz no puede resolver y que hay que decidir de frente: **la misión se cierra y el dinero sigue sin volver.**

La misión **no se queda abierta esperando el reintegro**. Cierra por `T-22` a `CERRADA_CON_HALLAZGO`, terminal e inmutable ([§8](../../03-arquitectura/estados/orden-de-mision.md)). Lo que queda vivo es la **obligación de reintegro**, que es una entidad propia de M-12 con su ciclo, su responsable, su monto, su plazo y su resolución — igual que el expediente de hallazgo de [§7.4](../../03-arquitectura/estados/orden-de-mision.md).

La razón es la misma que sostiene toda la máquina: un expediente que no puede cerrarse se abandona, y un expediente abandonado no produce el hallazgo que el auditor necesita ver ([RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)).

**Cuando el reintegro se paga**, meses después: se registra el pago contra la obligación, y sobre la misión cerrada se asienta el **asiento reverso económico** correspondiente ([RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [§8.3](../../03-arquitectura/estados/orden-de-mision.md)). El reverso afecta los acumulados **del período en que se registra, no los del período original**. La misión sigue siendo `CERRADA_CON_HALLAZGO` para siempre, y el reporte de esa misión muestra el valor original, el reverso y el resultado — nunca solo el resultado.

### 5. Lo que no se hace, aunque sea cómodo

- **No se compensa un sobrante de una misión contra un faltante de otra.** Son dos hechos económicos con dos responsables y dos fechas. Compensarlos hace desaparecer los dos.
- **No se ajusta el monto asignado hacia abajo para que cuadre.** El monto asignado está congelado desde `EF-04`; corregirlo es reescribir el pasado.
- **No se cierra automáticamente al vencer el plazo dando por devuelto lo que no volvió** ([RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), casos límite). Vencer el plazo genera alerta y escalamiento, nunca cuadre automático.
- **No se registra la devolución con la fecha del retorno** para evitar que el plazo aparezca vencido. Fecha del hecho es la fecha en que el dinero entró a la caja, y quien lo capture distinto está falsificando un dato ([RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)).

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C26a` | *El saldo no devuelto tras el retorno constituye un estado explícito con responsable nominado, monto y plazo en días hábiles contado desde la fecha del hecho del retorno; el sistema mantiene en todo momento el agregado de saldos pendientes por persona.* | [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) menciona el caso *"motorista que devuelve el saldo días después"* como caso límite y deja el plazo `[C]`. La [§10.1](../../03-arquitectura/estados/orden-de-mision.md) va de `CONSUMIDA` a `LIQUIDADA` sin estado intermedio. **Ninguna regla modela el dinero que está afuera**, que es justo lo que pregunta un arqueo |
| `RN-C26b` | *Todo faltante tipificado como sin causa identificada, aplicación a fin distinto o extravío genera una obligación de reintegro a cargo de persona nominada, con ciclo propio que sobrevive al cierre de la misión y se salda con asiento reverso sobre el expediente cerrado.* | [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) numeral 4 dice que el faltante *"genera automáticamente expediente de deducción de responsabilidad en M-12"* y marca `[C]` el procedimiento. **No existe la entidad obligación de reintegro** en ninguna regla ni en ninguna máquina de estados, y sin ella el cobro se pierde cuando la misión cierra |
| `RN-C26c` | *El gasto imprevisto en ruta distinto de combustible — repuesto, grúa, reparación menor, lavado — se registra con tipo, factura a nombre de la institución y autorización del acto; el parámetro institucional define si es admisible contra el fondo de combustible o exige fuente distinta.* | El fondo de [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md) es de combustible y [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) solo modela consumo con galones y estación. **Una faja de alternador comprada en Danlí no cabe en ningún campo** y termina registrada como faltante o como consumo falso. Pasa en toda misión larga `[I]` |
| `RN-C26d` | *El consumo pagado con recursos propios del servidor se registra con instrumento distinto del fondo y genera obligación de reintegro a favor del servidor, sin afectar el cuadre del fondo.* | Anunciada en los casos límite de [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) con `[C]` y depende del insumo #37. Sin ella, el pago de bolsillo se registra como consumo del fondo y **el cuadre miente en los dos lados a la vez** |

## Evidencia que debe quedar

Ante el TSC o ante Auditoría Interna, sobre una misión concreta:

1. El **acta de asignación** con folio, monto, base de la estimación, quién entregó, quién recibió y firma de recepción — `V-02` dentro de `T-12`
2. Cada **consumo** con galones, monto, precio unitario, estación, odómetro del momento y fotografía del comprobante ([RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md))
3. El **acta de devolución** con folio propio, monto, fecha del hecho de la recepción, quién devolvió y **quién recibió** — con la comprobación de segregación registrada, no solo declarada
4. El **cuadre desglosado** de [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md): asignado, comprobado, devuelto, diferencia — **con el motivo tipificado de cada diferencia**, su respaldo y quién lo aceptó
5. **La ventana de tiempo**: fecha del hecho del retorno, fecha del hecho de la devolución, plazo vigente aplicado y su parámetro. Es lo que demuestra si el dinero estuvo afuera dos días o dos meses
6. El **expediente de obligación de reintegro**, si lo hubo: monto, responsable, notificación, descargo del servidor, resolución y — si se pagó — el asiento reverso con su fecha y su efecto sobre los acumulados
7. El **arqueo del fondo por período** ([RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md)): aprobado, asignado, comprobado, devuelto y pendiente de devolución, con el detalle por persona de lo que está afuera
8. El **reporte de sobrantes recurrentes** por ruta y por vehículo, que es el que demuestra que la institución vigila su propia estimación y no solo a sus motoristas

## Trazabilidad

- **Autoridad de transiciones:** [`T-19` liquidar, `T-20` devolver liquidación, `T-21` y `T-22` cerrar](../../03-arquitectura/estados/orden-de-mision.md), [`EF-04`](../../03-arquitectura/estados/orden-de-mision.md) entrega del fondo al despachar, [`EF-06`](../../03-arquitectura/estados/orden-de-mision.md) anular con fondo entregado, [§10.1](../../03-arquitectura/estados/orden-de-mision.md) máquina de la asignación (`V-04` a `V-08`), [§7.2](../../03-arquitectura/estados/orden-de-mision.md) criterio `H-04`, [§8.3](../../03-arquitectura/estados/orden-de-mision.md) asiento reverso
- **Reglas:** [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) (regla eje), [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md), [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)
- **Reglas candidatas:** `RN-C26a`, `RN-C26b`, `RN-C26c`, `RN-C26d` — ninguna escrita
- **Puntos de control:** `PC-08` emisión con saldo suficiente, `PC-08b` entrega dentro del despacho, `PC-09` quien entrega ≠ quien despacha ≠ quien liquida, `PC-13` quien cierra ≠ quien liquidó, `PC-15` misiones anteriores sin liquidar, de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — TSC-NOGECI V-07 autorización, V-10 registro oportuno, V-14 conciliación periódica `[V]`; la exigencia de cadena trazable y paquete de evidencia es **implicación de requerimiento del equipo**, `[I]`. [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) — retorno de noche, sin señal, con la caja cerrada
- **Actores:** ACT-04 Jefe de Transporte (liquida), ACT-06 Motorista (recibe y devuelve), ACT-07 Encargado de Combustible (entrega y recibe la devolución), ACT-08 Gerencia Administrativa (cierra, autoriza reversos), ACT-10 Encargado de Delegación, ACT-12 Auditor Interno
- **Casos relacionados:** [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md), [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md), `CE-27` (cierre de ejercicio con hallazgo abierto), [CE-28](CE-28-hallazgo-posterior-sobre-mision-cerrada.md)
- **Insumos:** #1 (reglamento interno: procedimiento de reintegro y deducción), #2 (formato de acta de devolución en papel), #7 / `PROP-01` (sobrante se devuelve o se arrastra; saldo acumulado entre misiones), #26 (segregación en delegaciones pequeñas: a quién le devuelve el dinero el motorista de Trojes), #32 (plazo de liquidación y tolerancia), #37 (consumo pagado de bolsillo)
