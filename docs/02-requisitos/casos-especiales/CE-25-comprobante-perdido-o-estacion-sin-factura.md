# CE-25 — En Wampusirpe no dan factura, y el ticket de Catacamas se borró solo

| Campo | Valor |
|---|---|
| **Módulos** | M-09 Combustible, M-18 Peajes, M-13 Liquidación, M-15 Formatos Oficiales, M-16 Operación Desconectada, M-14 Auditoría |
| **Estados afectados** | `EN_RUTA` (registro del consumo), `RETORNADA`, `LIQUIDADA`, y el desenlace `CERRADA` o `CERRADA_CON_HALLAZGO` |
| **Frecuencia** | **Frecuente.** En misiones a zona rural es casi la norma, no la excepción |
| **Impacto** | Auditoría y financiero. **Y de adopción**: es el caso que decide si el sistema se usa o se abandona |
| **Resolución** | Definida. Mecanismo de constancia `[C]` — escalado al PO. Umbrales `[C]` |

## La situación

Pickup `INS-PU-009`. Misión de siete días a La Mosquitia: Tegucigalpa → Juticalpa → Catacamas → Puerto Lempira, entrega de equipo a la delegación.

| Dónde | Galones | Monto | Qué le dieron |
|---|---|---|---|
| Estación en Juticalpa | 12 | L 1,460 | Factura con RTN de la institución. Perfecta |
| Estación en Catacamas | 10 | L 1,220 | Ticket térmico de la bomba |
| Expendio en Wampusirpe | 8 | L 1,320 | **Nada.** Venden por bidón, no hay talonario, no hay RTN |
| Estación en Juticalpa, de retorno | 14 | L 1,700 | Factura |
| Caseta de Zambrano, ida y vuelta | — | L 44 | Un ticket de los dos. El otro no aparece |

Al vaciar la guantera en la liquidación, el ticket de Catacamas **está en blanco**: el papel térmico se decoloró con el calor de siete días.

Resultado: de **L 5,700** consumidos en combustible, **L 2,540 no tienen descargo formal**. Y el galón entró al tanque en los tres casos. El vehículo hizo los 1,340 km.

## Qué se hace hoy sin sistema

`[C]` No verificado. Se levanta con Auditoría Interna y con un motorista de años en el puesto (insumos #1 y #19).

Lo que se observa, y las cuatro salidas son malas:

1. **Se consigue una factura de otra bomba por el mismo monto.** Es la salida clásica. Y es exactamente el fraude que el TSC busca — solo que la mayoría de las veces la usa gente honesta para descargar un gasto real que no pudo comprobar.
2. **El encargado "se lo pasa" si es poco y se lo descuenta si es mucho.** El umbral no está escrito, no es el mismo dos meses seguidos, y depende de quién liquide.
3. **El motorista paga de su bolsillo lo que no puede comprobar.** La falta de un ticket se convierte en un descuento de salario sin ningún acto administrativo que lo sustente.
4. **En viáticos existe el mecanismo de constancia o declaración jurada** para gastos sin factura en zonas sin comercio formalizado `[C]` ([NRM-03](../../01-negocio/normativa/NRM-03-viaticos.md), cuya admisibilidad y forma en el reglamento vigente están sin confirmar). Nadie lo usa para combustible, porque nadie sabe si vale.

> **Las dos reglas que nadie escribió.** La primera: *hay un umbral tácito de tolerancia a la falta de comprobante, y varía según el monto y según quién liquide.* La segunda: *cuando el gasto no se puede descargar, el costo se traslada al motorista sin resolución que lo respalde.*
>
> Y la consecuencia de la segunda es la primera salida: **un motorista que sabe que le van a descontar un gasto real consigue el papel como sea.** El sistema que bloquea sin ofrecer salida no evita el gasto sin comprobante: lo convierte en un gasto con comprobante de conveniencia, que ya no se puede detectar.

## Por qué el flujo normal no lo cubre

[RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) ya resolvió la mitad, y la resolvió bien: los cinco campos duros son obligatorios, la fotografía del comprobante es **exigible pero no bloqueante**, *"porque en zonas rurales el comprobante a veces no existe o es ilegible, y bloquear ahí significa que el consumo no se registra en absoluto — que es peor"*. Lo mismo hace `PC-14` para el ticket de caseta.

Lo que falta es lo otro:

**Primero: el flujo feliz confunde "no hay comprobante" con "no hay evidencia".** No son lo mismo, ni de cerca. Un consumo sin factura pero con odómetro del momento, hora y ubicación del dispositivo, fotografía del surtidor con el galonaje visible, y coherencia con el rendimiento del vehículo tiene **más valor probatorio** que una factura suelta sin ninguna de esas cosas. Hoy el sistema no puede decir eso: su descargo es binario.

**Segundo: no distingue las causas.** [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) tipifica la ausencia en la liquidación y aplica un umbral único. Pero *"en Wampusirpe no emiten factura"*, *"el papel térmico se borró"*, *"lo perdí"* y *"no lo pedí"* son cuatro hechos distintos con cuatro consecuencias distintas, y hoy caen todos en el mismo saco.

**Tercero: el mecanismo alternativo de descargo se menciona en dos fichas normativas y ninguna regla lo recoge.** [NRM-03](../../01-negocio/normativa/NRM-03-viaticos.md) dice que existe. [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) lo lista entre los documentos que seguirán siendo de papel con firma manuscrita. [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) lo deja en `[C]` dentro de condiciones de aplicación. Nadie lo modeló.

**Cuarto: el traslado del costo al motorista no tiene procedimiento.** Y es el hueco que produce la conducta que el sistema venía a evitar.

## Regla de resolución

**1. El consumo se registra siempre. Sin excepción.** El galón entró al tanque: es un hecho. Un consumo no registrado por falta de papel **desaparece del denominador** de [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) y produce el *rendimiento imposiblemente bueno* de [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md). Callar un consumo real es un daño mayor que registrarlo sin factura.

**2. La ausencia se registra con causa tipificada, de catálogo configurable:**

| Causa | Qué la caracteriza | Descargo exigible y consecuencia |
|---|---|---|
| **El expendio no emite comprobante fiscal** | Zona sin comercio formalizado, venta por bidón, no inscrito | Habilita la constancia alternativa. **No es falta del motorista** |
| **Comprobante deteriorado o ilegible** | Papel térmico decolorado, mojado, roto | Si el motorista lo fotografió al momento de la carga — que [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) ya exige —, **hay evidencia aunque el papel ya no exista**. La foto es el descargo |
| **Comprobante extraviado** | Existió y se perdió | Declaración del motorista más evidencia sustituta. Si la foto se tomó al momento, la pérdida del papel es irrelevante |
| **No se solicitó** | Descuido | Es la única causa que debería tener consecuencia sobre el motorista, y aun así por la vía del punto 6 |
| **Emitido a nombre del motorista** y no de la institución | Frecuente | No es ausencia sino **defecto de descargo**. `[C]` con Auditoría Interna si lo aceptan ([RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)) |

**3. Se califica la suficiencia probatoria, no la presencia del papel.** Cada consumo acumula la evidencia que efectivamente se capturó:

- fotografía del comprobante, tomada **al momento de la carga**
- fotografía del surtidor o de la bomba con el galonaje visible
- lectura de odómetro del momento ([RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md))
- fecha, hora y ubicación del dispositivo al capturar — el mismo dato que [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) ya usa para detectar una foto tomada a 200 km de la estación declarada
- coherencia del galonaje con la capacidad del tanque
- coherencia con el rendimiento del vehículo y con la ruta autorizada

El sistema califica y **se lo muestra a quien liquida**, que hoy decide con criterio y sin dato. Es la misma lógica de [CE-24](CE-24-cobro-en-categoria-de-peaje-equivocada.md) para los pasos por caseta sin ticket: la fuerza probatoria se mide y se ve.

**4. Constancia de gasto sin comprobante, marcada como tal para el auditor.** `[C]` El mecanismo existe según [NRM-03](../../01-negocio/normativa/NRM-03-viaticos.md); su admisibilidad y su forma en el reglamento vigente **no están confirmadas**, y [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) lo ubica entre los documentos que seguirán siendo de papel con firma manuscrita. Con ese nivel de verificación, el sistema:

- genera el formato impreso con **folio, QR de verificación, espacio de firma y sello, y hash del contenido electrónico** ([RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), M-15),
- lo vincula al consumo concreto, a la asignación de folio y a la Orden de Misión,
- exige firma del motorista **y aval de una segunda persona** — jefatura inmediata o ACT-10 en delegación —, porque un descargo que se autofirma no es un descargo,
- y lo marca **explícitamente como descargo alternativo en todo reporte**. Jamás se cuenta como factura ni se suma con las facturas en un mismo total. El auditor tiene que ver de inmediato por qué vía se descargó ese gasto.

**5. Advertir sin bloquear — pero advertir no es callar.** El cierre no se bloquea por un ticket que el motorista no pudo conseguir ([RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), `PC-14`, [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md)). La consecuencia escala:

| Situación | Qué dispara |
|---|---|
| Consumo aislado, causa justificada, suficiencia probatoria alta | Observación en la liquidación. La misión cierra normal por `T-21` |
| Monto sin descargo por encima del umbral configurado de la misión | **`H-08`** — la misión solo puede cerrar por `T-22` como `CERRADA_CON_HALLAZGO` ([RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)) |
| **Patrón**: el mismo motorista, la misma ruta o la misma estación con ausencia recurrente | Alerta agregada a ACT-04 y ACT-12. Una misión no lo muestra; seis meses sí |

El umbral es parámetro con vigencia ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)), no un número en el código, y su valor es `[C]`.

**6. El costo no se le traslada al motorista sin acto administrativo.** Si la institución decide que un gasto sin descargo lo asume el servidor, eso es una **responsabilidad determinada**: procedimiento, descargo del interesado, resolución y notificación. **No es un ajuste que hace el liquidador en la hoja.** `[C]` insumo #1 — qué dice el reglamento interno de uso de vehículos. Hoy ocurre informalmente, y es precisamente el incentivo que fabrica la factura de conveniencia.

**7. Y la razón de fondo, escrita para que no se revierta en la primera revisión de seguridad:** bloquear la liquidación por un comprobante que el motorista no pudo conseguir **no produce comprobantes: produce comprobantes falsos, o produce que el consumo no se registre**. Las dos cosas son peores que un gasto sustentado con evidencia sustituta y marcado como tal. La trazabilidad inmutable prevalece sobre la comodidad del usuario en los puntos críticos; esto no es comodidad, es la diferencia entre un dato real y un dato inventado.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C25a` | *La ausencia de comprobante se registra con **causa tipificada** de catálogo configurable, y la causa determina el descargo exigible y la consecuencia. "No lo emiten", "se deterioró", "se extravió" y "no se solicitó" no son el mismo hecho.* | [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) comportamiento 5 tipifica la **ausencia** en la liquidación, no su causa, y aplica un umbral único. Sin causa, una zona sin comercio formalizado y un descuido reciben el mismo tratamiento |
| `RN-C25b` | *Todo consumo lleva una **calificación de suficiencia probatoria** derivada de la evidencia efectivamente capturada, independiente de la existencia del comprobante fiscal, y esa calificación se presenta a quien liquida y a quien audita.* | Hoy el descargo es binario: hay foto o no hay foto. El auditor no evalúa presencia de papel, evalúa si el gasto está sustentado. Esta regla convierte en dato lo que hoy es criterio no registrado del liquidador |
| `RN-C25c` `[C]` | *La constancia de gasto sin comprobante es documento oficial con folio, QR, hash, firma del servidor y aval de un segundo, vinculada al consumo, y se identifica como **descargo alternativo** en todo reporte y total.* | [NRM-03](../../01-negocio/normativa/NRM-03-viaticos.md) y [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) mencionan el mecanismo; **ninguna regla lo recoge**. [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) lo deja en `[C]` |
| `RN-C25d` `[C]` | *La imputación al servidor de un gasto sin descargo exige acto administrativo con procedimiento y resolución. No puede originarse en la liquidación.* | Ninguna regla lo cubre. Es el hueco que produce la factura de conveniencia |

### `[C]` Escalado al PO

| Decisión | Opciones y costo |
|---|---|
| **¿La institución admite constancia o declaración jurada como descargo de combustible?** ([NRM-03](../../01-negocio/normativa/NRM-03-viaticos.md) lo prevé para viáticos, `[C]` su forma) | **Sí, con tope de monto y aval de segunda persona**: el gasto real se descarga por vía legítima y queda marcado. **No**: hay que decirlo por escrito y aceptar que el motorista seguirá pagando de su bolsillo o consiguiendo papel. Se propone *sí, con tope*, y se consulta a Auditoría Interna antes de fijar el tope |
| **¿Cuál es el umbral de monto sin descargo que fuerza cierre con hallazgo?** | Muy bajo: casi toda misión rural cierra con hallazgo y el hallazgo pierde significado. Muy alto: el control no existe. Se propone fijarlo con Auditoría Interna sobre datos reales de tres meses de operación |
| **¿Se acepta comprobante a nombre del motorista?** ([RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)) | Si no se acepta, hay que decir qué se hace con los que ya existen |

## Evidencia que debe quedar

1. Por cada consumo o paso sin comprobante: **causa tipificada, evidencia sustituta capturada, calificación de suficiencia probatoria**, y quién aceptó el descargo, con qué fundamento y cuándo
2. La **constancia firmada** con su folio, QR verificable y hash, marcada como **descargo alternativo** — nunca sumada con las facturas en un mismo total
3. El total del período **separado en dos números**: gasto con comprobante formal y gasto con descargo alternativo, por motorista, vehículo, dependencia, estación y punto de peaje
4. La **conciliación de rendimiento del período incluyendo esos galones**. Este es el punto que le importa al auditor del TSC: un consumo sin factura **pero contabilizado** sostiene la correlación galones–kilómetros–misión autorizada. Un consumo omitido por falta de papel **la rompe**, y rompe también los períodos anteriores y siguientes
5. Las **alertas de patrón** — motorista, ruta o estación con ausencia recurrente — y qué se hizo con cada una
6. Si hubo imputación al servidor: el **acto administrativo** con su procedimiento y resolución, no un ajuste en la liquidación
7. Y la marca de **fuerza probatoria de cada paso por caseta sin ticket**, que es lo que sostiene o debilita el reclamo de [CE-24](CE-24-cobro-en-categoria-de-peaje-equivocada.md)

## Trazabilidad

- **Autoridad de transiciones:** [`H-08` ausencia de comprobante obligatorio según la política de la institución, `T-21` y `T-22`, §7.2](../../03-arquitectura/estados/orden-de-mision.md); §10.1 `V-07` liquidar con comprobantes y `V-08` liquidar con acta
- **Puntos de control:** [`PC-14`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) — advertencia que **no bloquea el cierre**; [`PC-13`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) — quien liquida no es quien consumió ni quien entregó el fondo
- **Reglas:** [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md) (regla eje), [RN-29](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md), [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md), [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md), [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md), [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)
- **Reglas candidatas:** `RN-C25a`, `RN-C25b`, `RN-C25c`, `RN-C25d`
- **Normas:** [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) TSC-NOGECI V-10 registro oportuno `[V]`, y *"el auditor no busca comprobantes, busca correlación"*; [NRM-03](../../01-negocio/normativa/NRM-03-viaticos.md) mecanismo de constancia o declaración jurada `[C]` — su admisibilidad y forma no están verificadas; [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) documentos que seguirán en papel con firma manuscrita; [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md); [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) — *advertir cuando falte el ticket, sin bloquear el cierre* `[V]`
- **Actores:** ACT-06 (registra y declara), ACT-04 (liquida y acepta el descargo), ACT-07, ACT-10 (avala en delegación), ACT-12 (patrón y hallazgo)
- **Casos relacionados:** [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — el consumo omitido por falta de papel es lo que produce el rendimiento imposible; [CE-24](CE-24-cobro-en-categoria-de-peaje-equivocada.md) — la discrepancia sin ticket; [CE-23](CE-23-fondo-agotado-con-misiones-programadas.md) — el pago de peculio propio; [CE-22](CE-22-odometro-inconsistente.md)
- **Insumos:** #1 (reglamento interno: qué se hace hoy con el gasto no descargado), #2 (formato de control de combustible en papel), #19 (informes de auditoría — cuántos hallazgos son por comprobante faltante), #24 (facturación en caseta y estado de cuenta), #37 (reembolso de peculio propio), #32 (umbrales y plazos). **A registrar `[C]`:** admisibilidad y forma de la constancia de gasto sin comprobante para combustible; tope de monto; quién avala
