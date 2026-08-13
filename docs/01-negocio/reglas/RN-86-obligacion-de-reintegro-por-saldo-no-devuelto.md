# RN-86 — El saldo no devuelto constituye obligación de reintegro con responsable nominado y ciclo propio que sobrevive al cierre de la misión

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-13, M-12, M-14 |
| **Origen** | Casos especiales [CE-26](../../02-requisitos/casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) y [CE-20](../../02-requisitos/casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de arqueo y de control de fondos entregados a servidores — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[C]` el plazo de devolución y el procedimiento de deducción de responsabilidad |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — parámetro `plazo_devolucion_saldo` en días hábiles, con vigencia |

## Enunciado

El saldo de fondo **no devuelto** tras el retorno constituye un **estado explícito** con **responsable nominado**, **monto** y **plazo en días hábiles contado desde la fecha del hecho del retorno**. El sistema **mantiene en todo momento el agregado de saldos pendientes por persona**.

Todo **faltante** tipificado como *sin causa identificada*, *aplicación a fin distinto* o *extravío* genera una **obligación de reintegro a cargo de persona nominada**, con **ciclo propio que sobrevive al cierre de la misión** y que se salda con **asiento reverso sobre el expediente cerrado** ([`RN-93`](RN-93-expediente-de-hallazgo-posterior.md)).

Mientras exista un saldo vencido o una obligación de reintegro abierta a cargo de una persona, **el sistema bloquea toda nueva asignación de fondo a esa persona**.

El **combustible pagado con recursos propios del servidor** genera obligación de reintegro **a favor** del servidor, con instrumento distinto del fondo y **sin afectar su cuadre** ([`RN-83`](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md)).

## Justificación

[`RN-29`](RN-29-liquidacion-de-combustible.md) menciona el caso *"motorista que devuelve el saldo días después"* como caso límite y deja el plazo `[C]`. La [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md) va de fondo consumido a liquidado sin estado intermedio. **Ninguna regla modela el dinero que está afuera** — que es exactamente lo que pregunta un arqueo.

Y hay un hueco peor: `RN-29` numeral 4 dice que el faltante *"genera automáticamente expediente de deducción de responsabilidad"*, pero **no existe la entidad obligación de reintegro** en ninguna regla ni en ninguna máquina de estados. Sin ella, **el cobro se pierde cuando la misión cierra**: el expediente se archiva, el hallazgo queda como marca, y el dinero no vuelve.

El bloqueo de nueva asignación cierra el agujero operativo de [CE-20](../../02-requisitos/casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md): hoy nada impide seguir entregándole fondo a quien no devolvió el anterior.

## Condiciones de aplicación

Aplica al saldo de toda asignación de fondo, sea la misión ejecutada, abortada o anulada.

Aplica a la **misión anulada con combustible ya entregado**: el plazo corre desde la anulación, y el fondo no consumido se devuelve con acta ([`RN-27`](RN-27-asignacion-de-combustible-con-folio.md)).

**No aplica** a la diferencia explicada y aceptada con causa tipificada y respaldo, que se cierra en la liquidación ([`RN-29`](RN-29-liquidacion-de-combustible.md)).

## Comportamiento esperado

1. La **fecha del hecho** de la devolución es **la fecha en que el dinero entró a la caja**, no la del retorno. Capturarla distinto es falsificar un dato ([`RN-46`](RN-46-fecha-del-hecho-y-fecha-de-captura.md)).
2. El **acta de devolución** lleva folio propio, monto, folios de vale devueltos **uno por uno**, quién devolvió y **quién recibió**, con la **segregación verificada por identidad de persona**, no solo declarada ([`RN-01`](RN-01-segregacion-de-funciones.md)).
3. **No se ajusta el monto asignado hacia abajo para que cuadre**: está congelado desde la entrega y corregirlo es reescribir el pasado.
4. **No se cierra automáticamente al vencer el plazo dando por devuelto lo que no volvió.** Vencer el plazo genera **alerta y escalamiento**, nunca cuadre automático.
5. La obligación de reintegro tiene: monto, responsable, notificación, descargo del servidor, resolución y —si se paga— el **asiento reverso con su fecha y su efecto sobre los acumulados**. Su determinación es materia del expediente y de quien corresponde ([`RN-74`](RN-74-sin-atribucion-de-responsabilidad-en-campo.md)); **no nace en la liquidación**.
6. El **arqueo del fondo por período** presenta: aprobado, asignado, comprobado, devuelto y **pendiente de devolución, con el detalle por persona de lo que está afuera**.
7. Las obligaciones abiertas al cierre del ejercicio integran el **saldo de apertura de control interno** del ejercicio siguiente, con antigüedad contada desde el hecho original ([`RN-97`](RN-97-saldo-de-apertura-de-control-interno.md)).
8. El sistema produce el **reporte de sobrantes recurrentes** por ruta y por vehículo: es el que demuestra que la institución vigila **su propia estimación** y no solo a sus motoristas.

## Casos límite

- **Sobrante recurrente en la misma ruta.** No es un problema del motorista: es una estimación mal calibrada. El reporte lo hace visible y el estimado se corrige, con vigencia.
- **Motorista que no vuelve a la sede en semanas** porque opera desde una delegación remota. El plazo corre igual; la salida es que la devolución se pueda constatar en la delegación, con su propia acta y su propio receptor.
- **Persona bloqueada que es la única disponible para una misión urgente.** El bloqueo se levanta solo por acto registrado de ACT-08 con motivo, no por decisión de quien programa. La excepción queda en el expediente y en el indicador.
- **Reintegro a favor del servidor que la institución tarda meses en pagar.** Tiene su propia antigüedad y su propio reporte. Un sistema que solo mide lo que el servidor le debe a la institución no es un sistema de control: es un sistema de cobro.
- **Faltante por robo del efectivo durante la misión.** Tipificado como sustracción ([`RN-75`](RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md)) con denuncia; no genera obligación de reintegro automática — la determinación es del expediente.
- **`[C]` Plazo de devolución.** Insumo #7, decisiones abiertas del fondo. Sin plazo definido, el sistema no puede decir si el dinero estuvo afuera dos días o dos meses, que es exactamente lo que el arqueo necesita.

## Trazabilidad

- Autoridad: [orden-de-mision.md §10.1](../../03-arquitectura/estados/orden-de-mision.md) — ciclo de la asignación de fondo
- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-27](RN-27-asignacion-de-combustible-con-folio.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-46](RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-74](RN-74-sin-atribucion-de-responsabilidad-en-campo.md), [RN-83](RN-83-todo-ingreso-de-combustible-es-un-abastecimiento.md), [RN-85](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md), [RN-93](RN-93-expediente-de-hallazgo-posterior.md), [RN-97](RN-97-saldo-de-apertura-de-control-interno.md)
- Casos especiales: [CE-26](../../02-requisitos/casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) `RN-C26a`, `RN-C26b`, `RN-C26d` · [CE-20](../../02-requisitos/casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) `RN-C20a`
- Insumos pendientes: #7 decisiones abiertas del fondo · #37 reintegro de peculio propio
