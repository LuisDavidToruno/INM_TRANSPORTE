# CE-20 — La misión se cancela después de que ya se entregó el combustible

| Campo | Valor |
|---|---|
| **Módulos** | M-09 Combustible, M-07 Programación y Despacho, M-13 Liquidación, M-15 Formatos Oficiales, M-14 Auditoría |
| **Estados afectados** | `PROGRAMADA`, `DESPACHADA` — y su desenlace en `ANULADA` o `RETORNADA` |
| **Frecuencia** | Frecuente — la comisión que se pospone el domingo es rutina |
| **Impacto** | Financiero, de auditoría y presupuestario |
| **Resolución** | Definida, con tres decisiones `[C]` de `PROP-01` que solo cambian parámetros |

## La situación

Orden de Misión `OM-2026-00412`: Tegucigalpa → Danlí → El Paraíso, salida el lunes a las 6:00 a.m., pickup `INS-PU-014`, motorista Reynaldo Zelaya. El viernes por la tarde el Encargado de Combustible emitió la asignación con folio `FC-2026-01187` por **L 3,500.00** en efectivo, y el motorista **firmó la recepción** porque el lunes sale de madrugada y la oficina abre a las 8:00.

El domingo a las 4:00 p.m. llaman: el acto en El Paraíso se pospuso quince días. La misión no sale.

A partir de ahí hay dos mundos distintos:

- **Nada se movió.** El efectivo está íntegro en poder del motorista. El vehículo nunca salió del predio.
- **Ya hubo consumo.** El motorista cargó **L 1,200.00** el domingo en la estación del bulevar Fuerzas Armadas, justamente porque salía antes de que abriera la bomba institucional. Hay una factura, hay galones en el tanque y hay dinero público gastado en una misión que no existió.

El segundo mundo es el que decide el diseño. Y es el más común.

## Qué se hace hoy sin sistema

El motorista devuelve el efectivo el lunes en la mañana, o el martes, o cuando pasa por la oficina. A veces se levanta acta de devolución firmada por ambos; a veces el Encargado de Combustible lo mete en la caja y hace la anotación en su cuaderno. Los vales impresos con el número de misión se rompen o se guardan "por si sirven para la próxima".

Si ya hubo carga, lo habitual es **dejar el combustible cargado a la misión siguiente**: se conserva la factura y se aplica al viaje reprogramado. Nadie escribe que se hizo eso.

`[C]` **Hay que confirmarlo en la sesión con Gerencia Administrativa, el Encargado de Transporte y un motorista con años en el puesto (insumo #1).** Tres preguntas concretas: ¿existe formato de acta de devolución?, ¿en cuántos días hábiles debe devolverse?, ¿un vale nominado a una misión se ha reutilizado alguna vez en otra?

**El "a veces se levanta acta" y el "se pasa a la próxima misión" son exactamente los dos hallazgos.**

## Por qué el flujo normal no lo cubre

El flujo feliz de la anulación asume que anular es un acto administrativo sin consecuencia económica: se cancela y ya. Aquí **ya salió dinero público de la caja** y hay un documento con folio en manos de un servidor.

Y hay una asimetría que el flujo no ve: `T-15` (`DESPACHADA → ANULADA`) exige devolución íntegra. Si se consumió **un solo lempira**, la misión no se puede anular — tiene que liquidarse. Un sistema que ofreciera "Anular" como botón único produciría el borrado de un hecho económico, que es lo que la premisa rectora 3 prohíbe.

## Regla de resolución

La secuencia es la de `EF-06` de la [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md), que es la autoridad sobre transiciones. No se reescribe aquí: se aplica.

**Paso 1 — Solicitar la anulación no anula nada.** ACT-04 Jefe de Transporte registra la solicitud con motivo tipificado. El sistema marca *anulación en trámite*, lista los pendientes y **la misión sigue en `DESPACHADA`**. No hay estado intermedio inventado: hay una marca sobre un estado existente.

**Paso 2 — El sistema consulta el estado de la asignación**, no la palabra de nadie ([§10.1](../../03-arquitectura/estados/orden-de-mision.md), máquina de la asignación de fondo):

| Estado de la asignación | Camino disponible | Qué ocurre |
|---|---|---|
| `ENTREGADA` sin ningún consumo registrado | `V-05` devolver íntegra → **`T-15`** misión `ANULADA` | Acta de devolución firmada por quien entrega y quien recibe. Asiento reverso de la asignación ([RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)). Folios emitidos a `ANULADO`, nunca reciclados (`EF-02`) |
| `CONSUMIDA`, aunque sea parcialmente | **`T-15` no está disponible.** Camino obligatorio `T-16` → `RETORNADA` | La misión se **liquida** aunque no se haya ejecutado: hubo movimiento de fondos públicos |
| `EXTRAVIADA` | `T-16` → `RETORNADA` | Se liquida con acta de extravío. Ver [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md) |

**Paso 3 — El saldo del fondo no se libera por declaración.** La devolución incrementa el saldo disponible **solo cuando está constatada con constancia de recepción** ([RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), caso límite de devolución; [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), comportamiento 2). Una devolución anunciada por teléfono no libera un lempira.

**Paso 4 — Segregación, sin excepción.** Quien entregó el fondo (ACT-07) no puede ser quien liquida, y quien liquida no puede haber sido solicitante ni despachador (`PC-09`, `PC-13`, [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)). El acta de devolución la firman **dos personas distintas**: quien devuelve y quien recibe.

**Paso 5 — Los galones ya cargados se quedan en el tanque, no en el limbo.** El combustible consumido se liquida contra la misión anulada por [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) — con kilometraje cero o casi cero, lo que produce un rendimiento no concluyente, no un hallazgo falso ([RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), condiciones de aplicación). **No se traslada la carga a la misión reprogramada**: el hecho económico ocurrió en la misión que se canceló, y moverlo rompe la correlación consumo–kilometraje–misión autorizada que es justo lo que el auditor cruza ([NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md)).

**Paso 6 — La misión reprogramada es una misión nueva.** Con su propia Orden de Misión, su propia asignación y su propio folio. No se "revive" la anulada. Si la institución quiere ver la relación, la nueva declara *reprograma a `OM-2026-00412`* como vínculo, no como continuación de estado.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C20a` | *El plazo para devolver el fondo de una misión anulada se cuenta en días hábiles desde la anulación; vencido sin devolución constatada, se abre expediente de responsabilidad y toda nueva asignación al mismo receptor queda bloqueada.* | [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) deja el plazo `[C]`, y `H-04` castiga al cerrar — pero **nada impide seguir entregándole fondo a quien no devolvió el anterior**. Ese es el agujero |
| `RN-C20b` | *El sistema produce por período el reporte de compromisos liberados por anulación, para la conciliación con ARGOS.* | [RN-48](../../01-negocio/reglas/RN-48-datos-espejo-de-solo-lectura.md) prohíbe que SIGTI escriba en ARGOS. Si SIGTI anula un compromiso y no lo reporta, el descuadre aparece en SIAFI — el mismo patrón que originó [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md) |

## Evidencia que debe quedar

Encadenada a la misma Orden de Misión, la institución debe poder mostrar:

1. La solicitud de anulación con motivo tipificado, autor, rol ejercido y marca de tiempo
2. El **acta de devolución**: monto en efectivo devuelto, folios de vale devueltos uno por uno, quién entregó, quién recibió, y firma de ambos
3. El **asiento reverso** de la asignación, con valor anterior, valor nuevo y referencia al asiento revertido
4. Los folios emitidos marcados `ANULADO`, con referencia al acta — y la demostración de que **ninguno se reutilizó**
5. Si hubo consumo: la liquidación completa de la misión no ejecutada, con el comprobante de la carga y el odómetro del momento
6. La transición `T-15` o `T-16` en el diario, con su motivo y su autor
7. El movimiento del saldo del fondo, con la fecha en que se constató la devolución — no la fecha en que se anunció

## Trazabilidad

- **Autoridad de transiciones:** [`EF-06`, `T-15`, `T-16` y §10.1](../../03-arquitectura/estados/orden-de-mision.md)
- **Reglas:** [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [RN-26](../../01-negocio/reglas/RN-26-fondo-de-combustible-aprobado.md), [RN-27](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [RN-01](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md), [RN-54](../../01-negocio/reglas/RN-54-cuota-trimestral-de-compromiso.md)
- **Puntos de control:** `PC-08b` (entrega), `PC-09` (segregación de la entrega), `PC-13` (segregación de liquidación y cierre) de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-14
- **Actores:** ACT-04, ACT-06, ACT-07, ACT-08, ACT-13
- **Casos relacionados:** [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md), [CE-26](CE-26-sobrante-o-faltante-al-liquidar.md), [CE-23](CE-23-fondo-agotado-con-misiones-programadas.md)
- **Insumos:** #1 (formatos en papel y plazos), #7 / `PROP-01` (¿el sobrante se devuelve o se arrastra?)

> **Aviso de colisión de IDs.** La plantilla [`caso-especial.md`](../../plantillas/caso-especial.md) menciona en su ejemplo un `CE-13 — viaje cancelado con vales emitidos`. Si el lote `CE-01` a `CE-19` lo escribe, **es este mismo caso**: se consolida bajo `CE-20` y el otro ID queda marcado obsoleto. **No se recicla.**
