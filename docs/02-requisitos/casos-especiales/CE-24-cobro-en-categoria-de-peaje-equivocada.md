# CE-24 — En Siguatepeque le cobraron L 90 a un microbús que paga L 22

| Campo | Valor |
|---|---|
| **Módulos** | M-18 Peajes, M-03 Flota, M-08 Bitácora, M-13 Liquidación, M-15 Formatos Oficiales, M-16 Operación Desconectada |
| **Estados afectados** | `EN_RUTA` (registro del paso), `RETORNADA` (conciliación de `EF-05`), `LIQUIDADA`; y con estado de cuenta, meses después de `CERRADA` |
| **Frecuencia** | **Previsible y recurrente** — la flota típica de una institución pública cae exactamente en la zona gris que la SAPP tuvo que resolver `[V]` |
| **Impacto** | Financiero pequeño por viaje, **acumulativo y reclamable**; y de auditoría |
| **Resolución** | Definida. Tarifa vigente `[C]` — insumo #21, bloqueante |

## La situación

Microbús Mercedes-Benz Sprinter `INS-MB-003`, 15 plazas, dos ejes. Su categoría de peaje asignada es **Liviano/Turismo: L 22** por cruce.

Sale de Tegucigalpa hacia San Pedro Sula con seis funcionarios y una audiencia a las dos de la tarde.

| Caseta | Le corresponde | Le cobraron |
|---|---|---|
| Zambrano, km 37 | L 22 | L 22 |
| Siguatepeque, km 125 | L 22 | **L 90** |
| Yojoa, km 182 | L 22 | L 22 |
| Siguatepeque, de retorno | L 22 | **L 90** |

En Siguatepeque el cajero lo clasificó como *"Vehículo de 2 Ejes"*. El motorista no discutió: había fila de furgones atrás, iba con hora de audiencia, y **no llevaba nada en la mano que dijera qué categoría le corresponde a su vehículo**. Pagó, guardó el ticket, siguió.

Son **L 68 de más por cruce**. Dos cruces por viaje, dos viajes al mes, cinco vehículos de la flota en el mismo supuesto — los Sprinter, los Kia K2700 y los Hyundai H-100 de reparto: cerca de **L 1,400 al mes**, unos **L 16,000 al año**.

No es dinero que quiebre a nadie. Es dinero que **nadie puede reclamar**, porque no existe el registro que lo demuestre.

> **El precedente no es hipotético.** `[V]` Entre el 27 de agosto y el 17 de septiembre de 2025, la SAPP recibió denuncias ciudadanas porque COVI-H estaba reclasificando **Hyundai H-100, Kia K2700 y Mercedes-Benz Sprinter** a categoría superior, cobrándoles L 90 en lugar de L 22 — cuatro veces de más. La SAPP resolvió que son **vehículos livianos** y ordenó suspender el cobro el mismo 17 de septiembre a las 10:00 a.m.
>
> Esos tres modelos son, exactamente, la flota liviana de carga y personal de una institución pública hondureña. Ver [NRM-10 §2](../../01-negocio/normativa/NRM-10-peajes.md).

## Qué se hace hoy sin sistema

`[C]` No verificado — [NRM-10 §8](../../01-negocio/normativa/NRM-10-peajes.md) es explícita: *"no se encontró ninguna fuente sobre cómo las instituciones públicas hondureñas manejan el pago y la liquidación de peajes"*. Insumos #24 y #25.

Lo que se observa:

El motorista paga y guarda el ticket, si se lo dan. Al liquidar entrega el fajo y quien liquida **suma**. Nadie compara el monto del ticket contra la tarifa que le correspondía, porque para hacerlo harían falta dos cosas que no están en ninguna parte: la tabla de tarifas vigente y **la categoría del vehículo**.

Si el motorista reclama en la caseta, discute con un cajero que no decide nada, con fila atrás y con pasajeros mirando. La instrucción informal es *"no discutás, pagá y seguí"*.

Y el reclamo ante la SAPP, cuando ocurre, lo presenta **un ciudadano por su cuenta**. Así fue en 2025: denuncias ciudadanas. Ninguna institución pública reclamó institucionalmente — y no fue porque no le cobraran mal.

> **La regla que nadie escribió:** *la categoría de peaje del vehículo no está escrita en ninguna parte.* No aparece en la matrícula, ni en la tarjeta de circulación, ni en el expediente del vehículo, ni en la orden de misión. Existe únicamente en el criterio del cajero de turno. Por eso el motorista no puede discutir: no tiene con qué.

## Por qué el flujo normal no lo cubre

**Porque la liquidación cuadra.** El ticket dice L 90 y en la liquidación entran L 90. Sumado y comprobado. Es el mismo patrón que [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md): cuadrar caja no es conciliar. El dinero cuadra y el cobro está mal.

**Y porque el flujo feliz, si se le deja, aprende del error.** La estimación previa ([RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md)) dijo L 22 por cruce; lo pagado fue L 90. Un sistema razonable "corregiría" la categoría del vehículo para que el estimado del mes siguiente cuadre. Ese sistema razonable convierte **el error de la caseta en la verdad institucional**: en tres meses la flota entera está clasificada como la caseta quiere, el estimado deja de tener desviación, y el reclamo no ocurre nunca. [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) lo prohíbe expresamente, y esa prohibición es el corazón del caso.

**Y porque el momento útil es la caseta, no la liquidación.** En la liquidación se reclama; en la caseta se evita. Hoy el motorista llega ahí sin dato, y todo el diseño de M-18 mira hacia atrás.

## Regla de resolución

**1. La categoría viaja con el motorista, impresa.** La Orden de Misión impresa ([RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), M-15: folio, QR verificable, firma y sello) lleva, por cada punto de peaje de la ruta:

- la **categoría de peaje asignada al vehículo** y su fundamento,
- la **tarifa esperada** a la fecha de la misión, del paquete normativo congelado al despachar (`EF-03`),
- y la referencia al **precedente**: la resolución de la SAPP del 17/09/2025 sobre clasificación de vehículos livianos.

`[V]` es que la SAPP resolvió y ordenó suspender el cobro. El **contenido literal del Artículo 51 de la Ley de Tránsito es `[C]`** — el PDF oficial es un escaneo sin capa de texto ([NRM-10 §2](../../01-negocio/normativa/NRM-10-peajes.md), insumo #23). **El documento impreso cita la resolución de la SAPP, que es lo verificado; no cita articulado que el equipo no ha leído.**

Con eso en la mano el motorista tiene algo que decir en la caseta. Sin eso, no tiene nada — y es el único momento en que el cobro indebido se puede evitar en vez de reclamar.

**2. El paso se registra en campo, sin conectividad.** El formulario precarga punto, categoría esperada y monto esperado; el motorista consigna **categoría cobrada y monto pagado**, y fotografía el ticket ([RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md), [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)). Si difieren, la discrepancia se marca sola: no depende de que alguien la note al liquidar.

**3. La discrepancia nunca modifica la categoría del vehículo.** [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md), enunciado. El cobro es un **hecho a registrar**; la clasificación es una **derivación de la ficha técnica y de la norma** ([RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md)). Son cosas de distinta naturaleza y el sistema no las mezcla.

**4. El sobrecosto no se le imputa al motorista.** Se registra en la liquidación como **sobrecosto por clasificación**, tipificado ([RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md)). Esto no es un gesto de generosidad: si el motorista teme que le descuenten los L 68, **no va a declarar la discrepancia** — va a acomodar la suma para que cuadre. La no imputabilidad es la condición para que el dato exista.

**5. La agregación es el producto.** El sistema agrupa las discrepancias por **punto, clase de vehículo y período**. Un reclamo con un ticket es una queja. Un reclamo con 180 pasos documentados, la ficha técnica de cada vehículo, el monto acumulado y el precedente de la SAPP es un **expediente que se gana**. Eso es lo que hoy no tiene ninguna institución, y es la razón por la que el reclamo de 2025 lo tuvieron que hacer ciudadanos.

**6. El cobro en categoría inferior también se registra.** Le cobraron de menos: es discrepancia igual. Callarla expone a la institución a un cobro retroactivo y contradice el principio de registro fiel ([RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md)).

**7. Reclasificación legítima: no toda diferencia es error.** El pickup que sale con remolque **sí** cambia de categoría. La **configuración del vehículo para la misión** se declara al programar y el estimado la usa; si va declarada, el cobro superior no es discrepancia ([RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md), condiciones de aplicación). Sin este campo, cada misión con remolque produce una discrepancia falsa, y tres falsas seguidas hacen que nadie vuelva a mirar las verdaderas.

**8. Motorista que no sabe con qué categoría le cobraron.** Es lo más común: el ticket puede no indicarla. Se registra el **monto pagado** y el sistema deriva la categoría probable contra la tabla del punto y la fecha, marcándola **inferida, no declarada** ([RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md)).

**9. Pago con tag CoviPass.** La categoría la aplica el sistema del tag y la evidencia no es un ticket sino el **estado de cuenta**. La conciliación es mensual contra ese estado, y una discrepancia detectada ahí se registra con **fecha del hecho igual a la del paso** y fecha de captura posterior ([RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)), y **puede reabrir el reclamo aunque la orden esté cerrada**: anexar evidencia a un expediente cerrado está permitido ([RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)). `[C]` insumo #24: si COVI-H emite estado de cuenta empresarial a nombre de la institución.

**10. Sin ticket se advierte, no se bloquea** — `PC-14` y [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md): *bloquear el cierre por un ticket faltante hace que el sistema se abandone*. Pero sin ticket el reclamo es más débil, y **eso tiene que verse**: cada discrepancia lleva su **fuerza probatoria** marcada — con ticket fotografiado, con estado de cuenta, o solo declarada. Ver [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md).

**11. Si la SAPP resuelve a favor de la caseta**, entonces la categoría asignada estaba mal: se corrige **abriendo nueva vigencia** con la resolución como fundamento, **sin reescribir los pasos anteriores** ([RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)).

### El aviso que condiciona todo lo anterior

**No se carga ninguna tarifa sin confirmar la vigente** ([NRM-10 §4 y §10](../../01-negocio/normativa/NRM-10-peajes.md), insumo #21). Hoy hay contradicción abierta entre el comunicado de la SIT del 28/02/2026 —*no habrá incremento para ninguna categoría*, `[V]`, corroborado por tres medios— y un agregador comercial que publica L 31 / 122 / 184 / 245 / 306 / 367 desde marzo.

Si el sistema arranca con la tabla equivocada, **produce discrepancias falsas en masa**: le dirá al motorista que esperaba L 22 cuando lo correcto eran L 31, marcará cada cruce del país como cobro indebido, y el primer reclamo institucional se caerá solo — con el costo reputacional de haberlo presentado. **Un detector de discrepancias montado sobre una tabla no verificada es peor que no tener detector.**

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C24a` | *La Orden de Misión impresa incluye, por cada punto de peaje de la ruta, la categoría de peaje asignada al vehículo con su fundamento y la tarifa esperada a la fecha de la misión, tomadas del paquete normativo congelado al despachar.* | [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) estima y desglosa **para quien autoriza**. [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) obliga a imprimir los documentos de control en carretera. Ninguna de las dos pone el dato **en la mano del motorista en la caseta**, que es el único momento en que el cobro indebido se puede evitar |
| `RN-C24b` | *El reclamo por discrepancia de clasificación es un objeto propio con estado, destinatario, fecha de presentación, resolución y resultado económico. Las discrepancias que lo integran no se dan por cerradas hasta que él se resuelve.* | [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md), comportamiento 4, describe un **reporte** —*"listo para presentar ante la SAPP"*— no un objeto con ciclo de vida. Sin estado, un reclamo presentado y jamás respondido no se distingue de uno que nadie presentó, y la recuperación del sobrecosto no tiene dónde registrarse |
| `RN-C24c` | *Ninguna tarifa entra en vigencia sin fuente y fecha de verificación registradas. Mientras la tarifa de un punto esté marcada como no verificada, la detección de discrepancia sobre ese punto se presenta como **no concluyente**.* | [NRM-10 §9](../../01-negocio/normativa/NRM-10-peajes.md) exige registrar fuente y fecha de verificación de cada tarifa y alertar a los 12 meses. [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md) resuelve la tarifa por punto × categoría × vigencia, pero **no condiciona la detección a la confianza del dato** |

## Evidencia que debe quedar

1. Por cada paso por caseta: punto, fecha y hora, **categoría esperada con su fundamento**, categoría cobrada, monto esperado, monto pagado, diferencia, medio de pago, y fotografía del ticket o referencia a la línea del estado de cuenta
2. La **ficha técnica que sustenta la categoría**: tipo, peso bruto vehicular en kg, número de ejes, capacidad de pasajeros, articulado — con la vigencia del atributo y el fundamento de su asignación ([RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md))
3. El **identificador y la vigencia de la tabla de tarifas usada**, congelada al despachar (`EF-03`), con su fuente y fecha de verificación — para que el cálculo sea reproducible dos años después, cuando la tarifa ya cambió tres veces
4. El **expediente de reclamo**: discrepancias agrupadas por punto y período, monto acumulado, precedente invocado (comunicado SAPP del 17/09/2025 `[V]`), fecha de presentación, estado y resolución
5. El **sobrecosto por clasificación** tipificado en cada liquidación, con la constancia de que no se imputó al motorista
6. La **fuerza probatoria** de cada discrepancia, para que el auditor sepa cuánto pesa el expediente sin tener que abrir cada adjunto
7. Y la correlación que pide el TSC: **peajes pagados del período contra ruta autorizada contra kilometraje** ([RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)). Un sobrecosto detectado, tipificado y reclamado es control interno funcionando. Un sobrecosto no detectado es dinero público pagado de más sin que nadie lo notara — y eso sí es hallazgo

## Trazabilidad

- **Autoridad de transiciones:** [`EF-03` congelamiento del paquete normativo al despachar, `EF-05` conciliación al retornar, `H-03` paso por peaje incompatible con la ruta autorizada](../../03-arquitectura/estados/orden-de-mision.md)
- **Puntos de control:** [`PC-14`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) — falta el ticket de un paso por caseta: **advertencia, no bloquea el cierre**
- **Reglas:** [RN-36](../../01-negocio/reglas/RN-36-discrepancia-de-clasificacion-en-caseta.md) (regla eje), [RN-33](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [RN-34](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [RN-35](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-38](../../01-negocio/reglas/RN-38-exoneracion-de-peaje.md), [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-40](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [RN-41](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-25](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)
- **Reglas candidatas:** `RN-C24a`, `RN-C24b`, `RN-C24c`
- **Normas:** [NRM-10](../../01-negocio/normativa/NRM-10-peajes.md) — matriz de once categorías `[V]`; comunicado SAPP 17/09/2025 sobre H-100, K2700 y Sprinter `[V]`; contenido literal del Art. 51 de la Ley de Tránsito `[C]`; tarifa vigente hoy `[C]`; lista de exoneraciones `[C]`. [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) — correlación, no comprobantes
- **Actores:** ACT-06 (registra el paso y la discrepancia), ACT-04 (arma el expediente de reclamo), ACT-08 (lo presenta), ACT-12
- **Casos relacionados:** [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md) — la discrepancia sin ticket; [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — el mismo patrón de "cuadra la caja, no cuadra el hecho"; [CE-22](CE-22-odometro-inconsistente.md)
- **Insumos:** #21 (**tarifa efectivamente vigente — bloqueante: sin esto no se carga ninguna tarifa**), #22 (lista oficial de exoneraciones), #23 (OCR de los Arts. 48 y 51 de la Ley de Tránsito), #24 (tags CoviPass, facturación en caseta, estado de cuenta empresarial), #25 (si el peaje se financia con el viático — **frontera con ARGOS a resolver antes de escribir historias de M-18**)
