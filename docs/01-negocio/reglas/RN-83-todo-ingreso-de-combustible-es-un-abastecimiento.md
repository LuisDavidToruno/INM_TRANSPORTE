# RN-83 — Todo ingreso de combustible al tanque se registra como abastecimiento con fuente declarada, y el nivel de tanque es dato de bitácora

| Campo | Valor |
|---|---|
| **Módulos** | M-09, M-08, M-13, M-16 |
| **Origen** | Casos especiales [CE-21](../../02-requisitos/casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md), [CE-23](../../02-requisitos/casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md), [CE-01](../../02-requisitos/casos-especiales/CE-01-salida-de-emergencia-convalidada.md), [CE-26](../../02-requisitos/casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md), [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) · Norma [NRM-01](../normativa/NRM-01-control-interno-tsc.md) |
| **Verificación** | `[P]` la exigencia de registrar íntegramente las operaciones y de conciliar — [NRM-01](../normativa/NRM-01-control-interno-tsc.md). `[C]` el reintegro de combustible pagado por el servidor — insumo #37 |
| **Tipo** | Bloqueo duro |
| **Configurable** | Sí — catálogo `fuente_de_abastecimiento` |

## Enunciado

**Todo ingreso de combustible al tanque de un vehículo institucional se registra como abastecimiento**, cualquiera sea su **fuente de financiamiento**, declarada del catálogo configurable:

`FONDO_DE_LA_MISION` · `TANQUE_INSTITUCIONAL` · `OTRA_DEPENDENCIA` · `DONACION` · `PECULIO_DEL_SERVIDOR` · `TERCERO_EN_APOYO`

Y el **nivel de combustible del tanque a la salida y al retorno** es **dato obligatorio de la bitácora**, en la escala que el instrumento permita.

El abastecimiento con fuente distinta de `FONDO_DE_LA_MISION` **entra en el denominador de la conciliación galonaje–kilometraje** ([`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md)) y **no entra en el cuadre del fondo** ([`RN-29`](RN-29-liquidacion-de-combustible.md)) hasta que exista el acto que corresponda.

## Justificación

Las siete reglas de M-09 modelan el consumo **del fondo**. Un despacho desde el tanque de la institución no pasa por ningún folio y por eso **no existe para el sistema** — y es exactamente lo que produce un rendimiento imposiblemente bueno: el vehículo recorrió 900 km con 20 galones registrados porque los otros 40 salieron del tanque de la sede.

El efecto es peor que un dato faltante: [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) detecta una desviación y **señala un síntoma cuya causa el sistema no puede registrar**. El conciliador busca un fraude donde hay un procedimiento no modelado, y cuando el patrón se repite, deja de mirar el indicador.

Lo mismo con el nivel de tanque. [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) lo menciona en un caso límite y lo atribuye a [`RN-22`](RN-22-custodia-del-vehiculo.md), que trata de custodia. **Ninguna regla lo obliga.** Sin él, *"salió lleno y volvió vacío"* no se puede distinguir de un faltante, y la conciliación de una misión corta con tanque grande no significa nada.

## Condiciones de aplicación

Aplica a todo vehículo de la flota, en misión o fuera de ella.

Aplica al **combustible pagado por el servidor de su propio peculio**: se registra como abastecimiento con fuente declarada y genera **obligación de reintegro a favor del servidor** ([`RN-86`](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)), **sin afectar el cuadre del fondo** — que de otro modo mentiría en los dos lados a la vez.

**No aplica** al trasiego entre tanques institucionales, que es movimiento de existencias y tiene su propio circuito.

## Comportamiento esperado

1. El registro exige: fecha y hora del hecho, **galones**, **odómetro del momento**, fuente, y —cuando la fuente lo tenga— monto, estación y **comprobante** con su unicidad ([`RN-84`](RN-84-unicidad-del-comprobante-en-la-institucion.md)).
2. El **nivel de tanque** se captura a la salida y al retorno con la escala disponible: fracción del indicador, o galones si el instrumento lo permite. La escala usada se registra: un octavo de tanque no es lo mismo en un pickup que en un bus.
3. El **remanente en tanque al retorno** se separa del consumo de la misión en la conciliación. Su destino contable es **parámetro institucional**, no una decisión del liquidador.
4. La conciliación de [`RN-30`](RN-30-conciliacion-galonaje-kilometraje.md) usa **todos** los abastecimientos del período, no solo los del fondo, y expone la fuente de cada uno.
5. El abastecimiento desde `TANQUE_INSTITUCIONAL` descuenta de las existencias del tanque y se imputa al vehículo y a la misión, con responsable de despacho identificado — con la misma segregación de [`RN-01`](RN-01-segregacion-de-funciones.md).
6. El consumo que **excede el fondo asignado** se registra igual, marcado como **excedido**, con comprobante y odómetro; su cobertura se resuelve en la liquidación, nunca omitiendo el registro ([`RN-77`](RN-77-versionado-del-alcance-autorizado.md)).

## Casos límite

- **`[C]` ¿Se reintegra el combustible pagado de peculio propio?** Insumo #37. **Sí**: hay que modelar el circuito de reembolso con su comprobación y su segregación —quien autoriza el reintegro no es el beneficiario ni quien liquida. **No**: hay que decirlo **por escrito**, porque la práctica ocurre igual y hoy queda fuera de todo registro. Mientras no se decida, el abastecimiento se registra y el reintegro queda pendiente sin acto.
- **Vehículo que sale con tanque lleno y misión corta.** El consumo del fondo puede ser cero y el rendimiento no concluyente. No es hallazgo: es lo que el nivel de tanque permite explicar.
- **Donación de combustible en una emergencia.** Se registra con fuente `DONACION`, sin monto si no lo hay, y con la constancia de quién lo entregó. Un galón sin precio sigue siendo un galón en el denominador.
- **Abastecimiento sin comprobante.** Rige [`RN-85`](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md): causa tipificada, evidencia sustituta y calificación de suficiencia probatoria. **El registro del abastecimiento no se omite nunca por falta de papel.**
- **Nivel de tanque no consignado en la hoja de papel.** Se declara como campo no consignado ([`RN-80`](RN-80-hoja-de-bitacora-impresa-con-folio.md)); **no se estima**.
- **Manipulación consistente del odómetro** para simular buen rendimiento. Esta regla no la detiene; se mitiga con la fotografía del tablero y con el cruce contra peajes y ruta ([`RN-37`](RN-37-coherencia-de-la-secuencia-de-casetas.md)): un vehículo que declara 980 km pero solo cruzó una caseta dos veces está diciendo dos cosas incompatibles.

## Trazabilidad

- Norma: [NRM-01](../normativa/NRM-01-control-interno-tsc.md) `[P]`
- Reglas relacionadas: [RN-01](RN-01-segregacion-de-funciones.md), [RN-26](RN-26-fondo-de-combustible-aprobado.md), [RN-28](RN-28-comprobacion-del-consumo-de-combustible.md), [RN-29](RN-29-liquidacion-de-combustible.md), [RN-30](RN-30-conciliacion-galonaje-kilometraje.md), [RN-32](RN-32-entrega-de-combustible-contra-orden-de-mision.md), [RN-72](RN-72-imputacion-por-tramo-de-vehiculo-y-motorista.md), [RN-84](RN-84-unicidad-del-comprobante-en-la-institucion.md), [RN-85](RN-85-ausencia-de-comprobante-causa-y-descargo-alternativo.md), [RN-86](RN-86-obligacion-de-reintegro-por-saldo-no-devuelto.md)
- Casos especiales: [CE-21](../../02-requisitos/casos-especiales/CE-21-galonaje-que-no-cuadra-con-kilometraje.md) `RN-C21a`, `RN-C21b` · [CE-23](../../02-requisitos/casos-especiales/CE-23-fondo-agotado-con-misiones-programadas.md) `RN-C23c` · [CE-26](../../02-requisitos/casos-especiales/CE-26-sobrante-o-faltante-al-liquidar.md) `RN-C26d` · [CE-01](../../02-requisitos/casos-especiales/CE-01-salida-de-emergencia-convalidada.md) `RN-c:gasto-de-bolsillo-fuera-del-fondo` · [CE-07](../../02-requisitos/casos-especiales/CE-07-retorno-anticipado-la-mision-se-aborta.md) `RN-c:remanente-de-combustible-en-tanque`
- Insumos pendientes: #37 reintegro de combustible pagado de peculio propio
