# CE-07 — Pasando La Esperanza avisan que la jornada se suspendió, y el pickup da la vuelta

| Campo | Valor |
|---|---|
| **Módulos** | M-08 Ejecución y Bitácora, M-13 Liquidación y Cierre, M-09 Combustible, M-18 Peajes, M-07 Programación, M-17 Traslado de Personas Externas, M-19 Seguimiento en Ruta, M-14 Auditoría, M-16 Operación Desconectada |
| **Estados afectados** | `EN_RUTA` → `RETORNADA` por `T-18` **subtipo retorno anticipado** |
| **Frecuencia** | Frecuente — la causa más común no es la avería, es que la actividad en destino se cayó |
| **Impacto** | Financiero y de auditoría. Y de gestión: es el indicador de coordinación que ninguna institución lleva |
| **Resolución** | Definida. Una decisión al PO sobre quién puede ordenarlo |

## La situación

Lunes 06:10. Un pickup sale de Tegucigalpa hacia Gracias, Lempira, con dos técnicos y cuatro cajas de material didáctico, para una jornada de capacitación de tres días. Ventana autorizada: lunes a miércoles. Odómetro de salida 61,240. Fondo entregado para 640 km y tres días. Peajes estimados y congelados para los pasos de Zambrano y Siguatepeque en cada sentido.

A las 14:30, pasando La Esperanza, con 258 km recorridos, entra una llamada: **la jornada se suspendió**. El local no está disponible y los participantes no fueron convocados a tiempo. No hay nada que hacer en Gracias.

La comisión decide regresar. Llegan a Tegucigalpa a las 21:40 con odómetro 61,758 — 518 km en total, de los 640 estimados. En Siguatepeque se llenó el tanque a las 10:20, porque el viaje era de tres días. **Ese combustible está en el tanque, no consumido**, y es un bien público que alguien tiene que reconocer. Las cuatro cajas de material vuelven al edificio. Y hay dos noches de viático que no se durmieron, que no son asunto de este sistema.

## Qué se hace hoy sin sistema

Se avisa por teléfono, se da la vuelta, y al día siguiente empieza el problema de papel. La bitácora se cierra con el kilometraje real, y en el espacio de observaciones alguien escribe "**se suspendió la actividad**" — cuatro palabras que tienen que explicar una desviación de 122 km, un tanque lleno y un fondo de tres días liquidado en uno.

Los vales no consumidos se devuelven, a veces con acta. Las cajas de material vuelven al mismo lugar de donde salieron y nadie registra que volvieron: es material que en el papel salió y nunca regresó.

Y lo más caro: **nadie anota por qué se suspendió, ni de quién dependía que se suspendiera.** El costo de esa misión — combustible, peajes, dos días de dos técnicos, el vehículo bloqueado — no se le atribuye a nadie, así que no le duele a nadie, y vuelve a pasar el mes siguiente.

`[C]` **Quién puede ordenar el retorno anticipado**: ¿el jefe de la comisión, la jefatura de la dependencia solicitante, el Jefe de Transporte? ¿Y puede el motorista decidirlo solo ante un riesgo — derrumbe, bloqueo de carretera, tormenta, inseguridad? No se inventa. Insumo nuevo #50.

## Por qué el flujo normal no lo cubre

El camino feliz asume que la misión cumple su objeto. Aquí no lo cumple, y el sistema tiene que registrar eso **sin llamarlo error y sin llamarlo anulación**:

- **`EN_RUTA → ANULADA` está prohibida, y con razón.** El vehículo salió, se consumió combustible público, se pagaron dos peajes y hubo custodia de un bien. Anular sería borrar hechos económicos reales.
- **No es `T-15` ni `T-16`.** Esas dos cubren la misión que nunca salió — con devolución íntegra la primera, con consumo la segunda. Aquí el vehículo salió, rodó 518 km y volvió.
- **`T-18` tiene el subtipo, pero el subtipo no tiene consecuencias diseñadas.** La tabla lo lista — *"retorno anticipado: la misión se abortó en ruta"* — con motivo obligatorio y nada más. Todo lo que sigue queda por definir: qué pasa con el kilometraje que no se recorrió, con el combustible que quedó en el tanque, con la carga que no se entregó, con el objeto que no se cumplió, y con la reserva del vehículo que ahora sobra.
- **La conciliación de `EF-05` va a marcar desviación.** 518 km contra 640 estimados dispara la vigilancia de kilometraje **por debajo**, que es correcta y deliberada ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) vigila en ambas direcciones). Sin tratamiento, esta misión llega a la liquidación con `H-02` encima por haber hecho exactamente lo correcto.

## Regla de resolución

**1. El objeto de la misión es un dato que se cierra, no una suposición.** Al registrar el retorno se declara el **grado de cumplimiento del objeto** — cumplido, parcialmente cumplido, no cumplido — con **causa tipificada**. El catálogo de causas es configurable, y su primera versión debe distinguir lo que hoy nadie distingue:

   | Grupo | Causas |
   |---|---|
   | **Causa en destino** | La actividad se suspendió · no había quién recibiera · el destinatario no estaba · las condiciones del sitio no lo permitieron |
   | **Causa en la vía** | Derrumbe · inundación · vía cerrada o tomada · condiciones climáticas |
   | **Causa en el recurso** | Avería del vehículo — deriva a `CE-02` · incapacidad del motorista — `CE-10` · siniestro — `CE-03` · sustracción — `CE-04` |
   | **Causa institucional** | Se ordenó el regreso por necesidad de la institución · el vehículo se requirió para una emergencia |
   | **Causa en las personas** | Enfermedad de un comisionado · situación de seguridad |

   Esta tipificación es el producto del caso. Sin ella, "se suspendió la actividad" es todo lo que la institución sabrá nunca sobre su propia coordinación.

**2. La decisión tiene autor, hora y medio.** Se registra **quién ordenó el retorno**, a qué hora, por qué medio, y quién lo recibió. Si el motorista decidió solo — vía cerrada, riesgo inminente, sin señal — se registra así, con justificación obligatoria, y **se convalida al sincronizar**. Mismo tratamiento que `T-17` da a la prórroga sin código: no se puede exigir una autorización que físicamente no se puede pedir, y tampoco se puede fingir que existió.

**3. El retorno anticipado con causa tipificada es la justificación de la desviación.** El kilometraje por debajo del estimado y el rendimiento alterado **no producen `H-02` ni `H-01` por sí solos** cuando el retorno es anticipado y su causa está registrada y aceptada. La conciliación se recalcula contra la **ruta efectivamente autorizada hasta el punto de retorno**, no contra la ruta completa.

   Es el mismo principio que `CE-06`: un sistema que produce hallazgos por hacer lo correcto enseña a la gente a no registrar la verdad. Lo que sí queda es la marca: la misión no cumplió su objeto, y eso se ve.

**4. El combustible que quedó en el tanque se declara.** Se registra el **nivel de tanque al retorno** en el acta de recepción del vehículo, junto al odómetro. Ese remanente no es consumo de esta misión: es un activo que queda en el vehículo. La conciliación de `EF-05` lo separa explícitamente —

   ```
   consumido por la misión = entregado − devuelto en vales − remanente en tanque atribuible
   ```

   — y `[C]` queda cómo lo trata la institución: si el remanente se abona al fondo, si se le imputa a la siguiente misión de ese vehículo, o si simplemente se documenta. Se cruza con las decisiones abiertas de `PROP-01`, insumo #7. **Lo que no puede pasar es que un tanque lleno pagado con fondo de esta misión desaparezca del expediente.**

**5. Los vales no consumidos se devuelven con acta, sin excepción.** [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md) y el mismo circuito de `EF-06`: cada folio no consumido se cuenta, se devuelve a `ACT-07` y se registra en acta firmada por ambos. Un folio entregado que no aparece ni consumido ni devuelto al vencer el plazo de liquidación es `H-04`, sin umbral. Es el punto exacto donde `CE-20` y este caso comparten mecanismo.

**6. La carga que no se entregó **vuelve** y su regreso se registra.** Acta de reingreso con inventario, contra el inventario de salida, y con quién la recibe. La cadena de custodia se cierra donde empezó. Si parte se entregó y parte no, se declara **entrega parcial** con el detalle de qué quedó pendiente — y ese pendiente es lo que va a justificar la misión siguiente.

**7. Las personas vuelven, y el manifiesto lo dice.** Si hubo traslado de personas externas, el retorno anticipado se registra como **novedad sobre el manifiesto cerrado**, nunca como edición ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)). Si alguien de la comisión se quedó en destino por sus propios medios, eso también es novedad: una persona que sale en el manifiesto y no vuelve en él necesita una línea que lo explique.

**8. El vehículo y el motorista se liberan de inmediato, y eso vale dinero.** La ventana efectiva se recorta a la real, se liberan las reservas de `EF-01`, y **las solicitudes en cola que competían por ese vehículo se reevalúan**. Un pickup que volvió el lunes por la noche está disponible el martes: si el sistema no lo libera, ese vehículo pasa dos días bloqueado por una misión que ya no existe, mientras alguien recibe un "no hay unidades disponibles". Es de los pocos lugares donde el control interno y la eficiencia empujan en la misma dirección.

**9. La misión abortada no se reintenta: se cierra y se abre otra vinculada.** No hay retorno desde `RETORNADA` hacia `EN_RUTA`. Si la jornada se reprograma para la semana siguiente, es una **Orden de Misión nueva** con vínculo explícito a la abortada. El vínculo importa: permite ver el costo total de haber tenido que ir dos veces, que es el número que hace visible el problema de coordinación.

**10. Los viáticos son de ARGOS.** Dos noches menos de pernocta significan un ajuste de viático. **SIGTI no lo calcula, no lo estima y no lo muestra** ([DP-001, D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). Lo que hace es exponer, con la clave de vinculación de la Orden de Misión, **el hecho**: la ventana efectiva fue del lunes 06:10 al lunes 21:40, con retorno anticipado y causa tipificada. ARGOS resuelve lo suyo con ese dato.

**11. El retorno anticipado es un indicador, y es de los buenos.** Se acumula por causa, por dependencia solicitante y por destino. Una dependencia con seis misiones abortadas por "la actividad se suspendió" en un trimestre no tiene un problema de transporte: tiene un problema de planificación, y por primera vez hay evidencia para decirlo. El costo de cada misión abortada — combustible, peajes, kilometraje, días de vehículo — se totaliza y se atribuye.

### Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:grado-de-cumplimiento-del-objeto` | Toda misión cierra declarando el grado de cumplimiento de su objeto con causa tipificada del catálogo configurable; el grado es dato de cierre obligatorio, no observación libre |
| `RN-c:autoria-de-la-decision-de-abortar` | El retorno anticipado registra quién lo ordenó, cuándo y por qué medio; si lo decidió el motorista sin poder consultar, se registra así y se convalida al sincronizar |
| `RN-c:desviacion-amparada-por-retorno-anticipado` | La desviación de kilometraje y rendimiento derivada de un retorno anticipado con causa registrada y aceptada no produce hallazgo por sí sola; la conciliación se recalcula contra el trayecto efectivamente autorizado hasta el punto de retorno |
| `RN-c:remanente-de-combustible-en-tanque` | El nivel de tanque al retorno se registra en el acta de recepción y se separa del consumo de la misión en la conciliación; su destino contable es parámetro institucional |
| `RN-c:reingreso-de-carga-no-entregada` | La carga que vuelve sin entregarse se reingresa con acta e inventario contra el de salida; la entrega parcial declara qué quedó pendiente |
| `RN-c:liberacion-inmediata-de-reservas-por-retorno-anticipado` | El retorno anticipado recorta la ventana efectiva y libera vehículo y motorista de inmediato, reevaluando las solicitudes en cola que competían por ellos |
| `RN-c:vinculo-entre-mision-abortada-y-su-reintento` | La misión que repone a una abortada se vincula explícitamente a ella, y el costo acumulado de ambas se reporta junto |
| `RN-c:indicador-de-mision-abortada-por-causa` | Las misiones abortadas se acumulan por causa, dependencia solicitante y destino, con su costo atribuido, como indicador de calidad de la programación institucional |

## Escalamiento al PO

`[C]` **¿Quién puede ordenar el retorno anticipado?** Insumo nuevo #50.

| Opción | Costo |
|---|---|
| Solo `ACT-04` Jefe de Transporte | Coherente con `T-17`, pero la decisión suele ser de la dependencia que solicitó: es su actividad la que se cayó. Y en carretera, sin señal, `ACT-04` no existe |
| La jefatura de la dependencia solicitante (`ACT-03`) o `ACT-04`, indistintamente, con registro de cuál fue | Refleja la realidad. Costo: dos caminos de autorización para el mismo acto, que hay que poder distinguir en el expediente |
| Cualquiera de los dos, **más** el motorista por sí mismo cuando hay riesgo — vía cerrada, clima, seguridad — con convalidación posterior obligatoria | Es la única opción que no obliga al motorista a seguir manejando hacia un derrumbe esperando permiso. Costo: hay que tipificar qué cuenta como riesgo y auditar las convalidaciones |

**Recomendación del análisis**, no decisión: la tercera. La facultad del motorista de detener la misión por riesgo no debería depender de una autorización que no puede pedir, y el control real es la convalidación posterior con causa tipificada — el mismo mecanismo de `PC-18` para los actos de delegación por emergencia.

## Evidencia que debe quedar

1. Hora, lugar y odómetro del punto donde se decidió el retorno, y odómetro final de retorno
2. **Grado de cumplimiento del objeto y causa tipificada**, con quién ordenó el retorno, cuándo y por qué medio
3. **Correlación entre lo consumido y los kilómetros efectivamente recorridos hasta el punto de retorno** — la conciliación contra el trayecto real, no contra el planificado
4. Nivel de tanque al retorno y el tratamiento dado al remanente
5. Acta de devolución de vales no consumidos, folio por folio, firmada por el motorista y por `ACT-07`
6. Acta de reingreso de la carga no entregada, contra el inventario de salida
7. Novedad registrada sobre el manifiesto de personas, si lo hubo
8. Peajes efectivamente pagados hasta el punto de retorno, con su categoría y su tarifa congelada, y la constancia de que no se imputó ninguno posterior
9. La constancia de que la ventana efectiva real se expuso a ARGOS, **sin monto de viático calculado en SIGTI**
10. El vínculo con la misión que repuso a esta, si la hubo, y el costo acumulado de ambas

## Trazabilidad

- **Reglas**: [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) nada se borra · [`RN-06`](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) · [`RN-08`](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md) cierre con cadena completa · [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) reservas · [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) · [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) · [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) · [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) · [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)
- **Reglas candidatas**: las ocho de la sección anterior
- **Transiciones**: `T-18` **subtipo retorno anticipado** — la transición del caso · `T-19` liquidación · `T-21` o `T-22` según hallazgos · `W-06` o `W-07` para el vehículo según novedades
- **Prohibida**: `EN_RUTA → ANULADA` — el vehículo salió y hubo consumo real
- **No confundir con**: `T-15` anulación con devolución íntegra y `T-16` misión no ejecutada con consumo — en ambas el vehículo **nunca salió**
- **Efectos**: `EF-01` liberación de reservas · `EF-05` conciliación recalculada contra el trayecto real · `EF-06` circuito de devolución
- **Criterios de hallazgo**: `H-02` kilometraje — **amparado si la causa está registrada** · `H-01` consumo — igual · `H-04` fondo no devuelto ni comprobado en plazo, **este sí sin amparo**
- **Puntos de control**: `PC-11` coherencia del odómetro · `PC-13` segregación al liquidar y cerrar · `PC-16` registro del acto · `PC-18` convalidación de actos por emergencia
- **Fronteras**: [DP-001, D-01](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — el ajuste de viático por las noches no pernoctadas lo resuelve ARGOS
- **Normativa**: [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) · [`NRM-02`](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) · [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) · [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md)
- **Actores**: `ACT-06` registra y puede decidir ante riesgo · `ACT-03` dependencia solicitante · `ACT-04` autoriza y liquida · `ACT-07` recibe la devolución · `ACT-08` cierra · `ACT-16` ARGOS recibe la ventana efectiva
- **Casos especiales relacionados**: `CE-02` avería — desenlace "se aborta y retorna" · `CE-03` accidente · `CE-04` sustracción · `CE-05` relevo · `CE-06` extensión, el caso simétrico · `CE-20` misión cancelada con combustible entregado antes de salir · `CE-21` galonaje que no cuadra
- **Insumos**: **#50 nuevo** — quién puede ordenar el retorno anticipado y si el motorista puede decidirlo ante riesgo · #7 tratamiento del remanente de combustible, dentro de `PROP-01`
- **Historias candidatas**: `HU-c:registrar-retorno-anticipado-con-causa-tipificada`, `HU-c:declarar-grado-de-cumplimiento-del-objeto`, `HU-c:reingresar-carga-no-entregada`, `HU-c:liberar-recursos-por-retorno-anticipado`, `HU-c:reportar-misiones-abortadas-por-causa-y-dependencia`
