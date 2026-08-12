# CE-05 — El motorista amanece con fiebre en Puerto Cortés y el bus tiene que salir a las seis

| Campo | Valor |
|---|---|
| **Módulos** | M-05 Motoristas y Habilitación, M-07 Programación y Despacho, M-08 Ejecución y Bitácora, M-09 Combustible, M-15 Formatos Oficiales, M-16 Operación Desconectada, M-13 Liquidación, M-14 Auditoría |
| **Estados afectados** | `EN_RUTA` — autotransición `T-17` subtipo relevo. El vehículo permanece `EN_MISION` |
| **Frecuencia** | Frecuente — enfermedad, fatiga, emergencia familiar, misión larga con relevo previsto |
| **Impacto** | Legal antes que operativo: conducir sin licencia habilitante traslada responsabilidad directa a quien lo autorizó |
| **Resolución** | Definida. Requiere una capacidad que hoy no está diseñada: **padrón de relevo dentro del paquete de misión**. Y deja un hallazgo de contradicción entre artefactos |

## La situación

Un bus institucional de 30 pasajeros sale un lunes de Tegucigalpa con 24 servidores hacia un operativo de cinco días en Puerto Cortés. Motorista: don Wilmer, licencia categoría D, vigente hasta noviembre. Salió con odómetro 214,880, fondo para los cinco días y salvoconducto porque el retorno cae domingo.

El miércoles a las 05:00 don Wilmer amanece con fiebre alta y dolor articular. En el CESAMO le diagnostican dengue y le extienden incapacidad por tres días. **No puede conducir.** El operativo tiene que moverse a las 06:00 hacia Omoa y el jueves regresa a Tegucigalpa con las 24 personas.

Las opciones que hay sobre la mesa a las 05:30, con don Wilmer en una cama:

- La delegación de Puerto Cortés tiene un motorista disponible, don Óscar. Pero don Óscar tiene **licencia categoría C**: habilita camiones, **no habilita un bus de 30 pasajeros**. No puede.
- Mandar un motorista desde Tegucigalpa: siete horas de viaje, y hay que moverlo en algo.
- Que conduzca uno de los servidores de la comisión que "tiene licencia". Es lo que a veces pasa, y es exactamente lo que no puede pasar.

Y mientras se decide: don Wilmer tiene en su poder el fondo de combustible que firmó el lunes, el vehículo bajo su custodia de misión, el teléfono institucional con la bitácora abierta, y la Orden de Misión impresa que lo nombra a él como conductor autorizado.

## Qué se hace hoy sin sistema

Se resuelve por teléfono. El Jefe de Transporte autoriza de palabra, se manda a alguien, y el papel se arregla al volver. La Orden de Misión impresa **sigue diciendo el nombre del motorista que ya no está conduciendo**, y si en la carretera hay un operativo de fiscalización, el documento no corresponde con quien va al volante.

El fondo se traspasa de mano a mano. Los vales que quedaban se los da don Wilmer al que llegó, sin contar y sin acta, porque son las seis de la mañana y el bus tiene que salir. Al liquidar, todo el consumo aparece bajo el motorista original, incluido el de los días que ya no manejó.

`[C]` **Si la institución tiene un procedimiento escrito de relevo en ruta y quién puede autorizarlo desde la delegación** — no se inventa. `[C]` **Si existe límite de jornada de conducción** (horas continuas al volante, descanso obligatorio) y si es norma o práctica. `NRM-06` cubre licencias y tránsito, **no** jornada de conducción. Insumo nuevo #48.

## Por qué el flujo normal no lo cubre

`T-17` sí contempla el relevo de motorista, y `RN-14` sí exige revalidar habilitaciones. El problema es **con qué datos se revalida a las 05:30 en Puerto Cortés**.

`BD-02` se evalúa "contra el paquete normativo congelado que lleva el dispositivo". El paquete congelado por `EF-03` incluye la matriz licencia↔vehículo, pero **el padrón de motoristas no está en la lista de lo que se congela**. El dispositivo lleva la matriz para evaluar, y no lleva a quién evaluar: los datos de licencia de don Óscar viven en el espejo de Talento Humano, en el servidor, y en Puerto Cortés puede no haber datos.

Además:

- **El fondo lo firmó otra persona.** `EF-04` lo entrega contra firma del motorista y ahí termina la cadena. No hay figura de traspaso de fondo entre motoristas en ruta, y `ACT-07` no está en Puerto Cortés para intervenir.
- **La custodia del vehículo es nominal.** `RN-22` la traslada al motorista al despachar. El traspaso en ruta corta la responsabilidad en dos tramos, y ese corte necesita un odómetro y una hora.
- **Los documentos impresos nombran a una persona.** `EF-02` permite reimprimir con el mismo folio y el mismo contenido, y emitir un documento nuevo que declara "sustituye al folio X". Ninguna de las dos cosas se puede hacer sin impresora en Puerto Cortés a las 05:30.
- **El motorista saliente tiene que volver.** Don Wilmer, enfermo, está a 250 km de su casa y ya no es parte de la tripulación.

## Regla de resolución

**1. El paquete de misión lleva padrón de relevo.** Se amplía `EF-03`: al despachar, el dispositivo recibe además el **subconjunto de motoristas habilitados** de las delegaciones que la ruta autorizada toca, con lo mínimo para evaluar `BD-02` sin red — identificador de persona, número y categoría de licencia, fecha de vencimiento, restricciones médicas registradas y su disponibilidad conocida al momento del despacho.

   Con eso, en Puerto Cortés y sin señal, el dispositivo puede responder la única pregunta que importa: **¿este motorista habilita este vehículo hasta el fin de esta misión?** En el caso de don Óscar responde **no**, con el dato concreto: licencia categoría C, el vehículo exige D según la matriz versión *v*, capacidad 30 pasajeros. Bloqueo duro, sin excepción configurable ([`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [DP-001 D-12](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)).

**2. Si el entrante no está en el padrón, se registra el relevo con evaluación diferida y marca visible.** Puede pasar: llega un motorista de otra delegación que la ruta no tocaba. Entonces el motorista entrante aporta los datos de su licencia y **fotografía de la licencia física**, el sistema registra la evaluación como **no verificable en el dispositivo**, y la revalidación completa ocurre al sincronizar. Si al revalidar falla `BD-02`, se cumple `H-07` y la misión cierra con hallazgo por `T-22`.

   Es la única salida honesta: no se puede exigir en Puerto Cortés un dato que solo existe en Tegucigalpa, y tampoco se puede fingir que se verificó. **Lo que no se hace nunca es dejar pasar el relevo en silencio.**

**3. La autorización sigue el mismo camino que la prórroga.** `T-17` exige autorización de `ACT-04`. Con señal de voz, se ejecuta con **código de autorización fuera de línea** (§6.6 de la máquina de estados). Sin ninguna forma de contacto, el motorista registra el hecho con justificación obligatoria y **la falta de autorización previa se resuelve en la liquidación**, con hallazgo si la institución lo tipifica. Lo que no cambia nunca es `BD-02`: la autorización de `ACT-04` **no levanta** el bloqueo de licencia. Un jefe no puede autorizar que alguien conduzca sin habilitación.

**4. El relevo es un corte de responsabilidad, con acta y con odómetro.** Se registra, sin red:

   | Dato | Por qué |
   |---|---|
   | Hora y lugar del traspaso | Delimita los tramos |
   | **Odómetro al traspaso** | Es el corte. Todo consumo y todo kilómetro se imputa al tramo del motorista que lo generó |
   | Identidad de quien entrega y de quien recibe | `RN-22`: la custodia de misión es nominal y no queda vacante ni un minuto |
   | Estado del vehículo y novedades declaradas | Lo que el entrante recibe es lo que va a devolver |
   | Motivo tipificado del relevo | Incapacidad, fatiga, emergencia, relevo previsto, sustitución por indisponibilidad |

   **La responsabilidad del tramo anterior no se transfiere** — así lo fija `T-17`. Don Wilmer responde por sus dos días; don quien llegue, por los suyos.

**5. El fondo se traspasa contado, no de mano a mano.** Se registra **acta de traspaso de fondo** con: folios de vales consumidos con su comprobante, folios no consumidos entregados uno por uno, y saldo en efectivo si lo hay. A partir de ahí el receptor es el responsable de ese saldo ([`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md)).

   `ACT-07` no está presente y no puede estarlo. **La segregación no se rompe**: quien entrega el fondo a la misión sigue siendo `ACT-07`, y este es un traspaso de custodia **dentro** de la misma asignación, entre dos personas que ocupan el mismo rol de receptor. Se registra como tal, y `ACT-07` lo convalida al recibir la liquidación. Un vale que aparece consumido en un tramo donde su folio ya había sido traspasado es una alerta automática.

**6. La conciliación se hace por tramo.** Galonaje y kilometraje se concilian contra el rendimiento esperado **por tramo de motorista**, no sobre la misión completa ([`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)). Es el mismo principio que `CE-02` establece para el vehículo sustituto: promediar dos conductas de manejo distintas produce un rendimiento que no describe a ninguno de los dos, y los indicadores de motorista quedan inservibles.

**7. El documento impreso se corrige, y hasta que se corrija hay un acta que lo acompaña.** Al sincronizar, la Orden de Misión se **reemite con folio nuevo declarando que sustituye al folio anterior** (`EF-02`), y el folio original queda vigente en su tramo, no anulado — amparó la circulación de dos días reales. Mientras no haya impresora, el vehículo circula con **acta de relevo manuscrita** en el formato preimpreso que va en el paquete físico, con folio del rango de la delegación ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)), firmada por quien entrega y quien recibe. Es lo que el motorista le enseña a `ACT-15` en carretera.

**8. El entrante entra al calendario, el saliente sale.** El relevo crea la reserva del motorista entrante sobre la ventana restante y libera la del saliente ([`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), `BD-11`). Si el entrante tenía otra misión programada que se traslapa, el conflicto se muestra con su titular y se resuelve por los caminos de `EF-01` — no se sobre-asigna, ni siquiera con advertencia.

**9. El regreso del motorista saliente es un traslado, y se registra como tal.** Si vuelve en el mismo vehículo, entra al manifiesto como **acompañante, no como tripulación**, y se registra como novedad sobre el manifiesto cerrado ([`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)). Si vuelve por otro medio, se registra cómo y con cargo a qué. Lo que no puede pasar es que una persona desaparezca del expediente en Puerto Cortés y reaparezca en Tegucigalpa sin que nada lo explique.

**10. El relevo previsto se planifica, para que el improvisado sea la excepción.** `T-14` ya admite "un motorista de relevo declarado en la programación". Para misiones que superan el umbral configurable de duración o de distancia, el sistema **propone** declarar motorista de relevo desde `T-08`, con sus habilitaciones validadas en tierra, con red y con tiempo. El relevo planificado no necesita nada de los puntos 2 y 7.

### Hallazgo — ¿el permiso de circulación en día inhábil nombra al motorista?

`BD-04` dice que el permiso debe estar *"vigente para esa ventana y ese vehículo"*. `PC-03` de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) dice *"salvoconducto vigente para ese vehículo, motorista y ventana"*. **No dicen lo mismo, y la diferencia decide este caso**: si el permiso nombra al motorista, el relevo de don Wilmer invalida el salvoconducto y el bus no puede circular el domingo.

Por la [precedencia de `CLAUDE.md`](../../../CLAUDE.md), la máquina de estados es autoridad en precondiciones: el permiso ampara **vehículo y ventana**, y el relevo no lo invalida. **No se corrige aquí** porque el artefacto a corregir está fuera de esta carpeta: se reporta contra `PR-01`, `PC-03`.

### Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:padron-de-relevo-en-el-paquete-de-mision` | El paquete de misión incluye los datos mínimos de habilitación de los motoristas de las delegaciones que toca la ruta, para que `BD-02` se pueda evaluar en campo sin conectividad |
| `RN-c:habilitacion-no-verificable-en-campo` | Cuando el entrante no está en el padrón del dispositivo, el relevo se registra con evaluación diferida marcada, foto de la licencia física, y revalidación obligatoria al sincronizar; el fallo posterior produce `H-07` |
| `RN-c:acta-de-relevo-con-corte-de-odometro` | Todo relevo de motorista en ruta exige acta con hora, lugar, odómetro, identidad de ambos y motivo tipificado; el odómetro del acta es el corte de imputación de kilometraje y consumo |
| `RN-c:traspaso-de-fondo-entre-motoristas` | El fondo se traspasa por conteo de folios con acta, dentro de la misma asignación; `ACT-07` convalida al liquidar. Un consumo imputado a un folio ya traspasado es alerta automática |
| `RN-c:conciliacion-por-tramo-de-motorista` | Rendimiento e indicadores de conducta de manejo se calculan por tramo de motorista, nunca promediando la misión completa |
| `RN-c:relevo-previsto-en-mision-larga` | Superado el umbral configurable de duración o distancia, el sistema propone declarar motorista de relevo en la programación, con habilitaciones validadas antes de la salida |

## Escalamiento al PO

`[C]` **¿Existe límite de jornada de conducción y quién lo controla?** Insumo nuevo #48. Sin respuesta, el sistema **mide y muestra** las horas al volante por tramo pero no bloquea. Opciones:

| Opción | Costo |
|---|---|
| Solo medir y mostrar | No previene el accidente por fatiga, que es real en misiones de siete horas continuas |
| Advertir al superar un umbral configurable | Requiere fijar el umbral sin norma que lo respalde: sería `[I]`, no `[V]` |
| Bloquear el despacho de misiones cuya ruta exige más horas continuas que el umbral, salvo que se declare motorista de relevo | Es el que produce el comportamiento correcto — empuja al relevo previsto — pero condiciona la operación con un número que hoy nadie puede verificar |

**Recomendación del análisis**, no decisión: la segunda hasta que exista el insumo #48, y la tercera después, con el umbral como parámetro con vigencia ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)).

## Evidencia que debe quedar

1. Acta de relevo con hora, lugar, **odómetro**, identidad de quien entrega y de quien recibe, y motivo tipificado
2. **La evaluación de `BD-02` del motorista entrante con sus insumos concretos**: número de licencia, categoría, vencimiento, versión de la matriz usada, atributos del vehículo, fecha de fin de rango evaluada — no la palabra "verificado"
3. Si la evaluación fue diferida: la marca, la foto de la licencia, y el resultado de la revalidación al sincronizar
4. Quién autorizó el relevo, cuándo, y si fue con código fuera de línea o con justificación diferida
5. Acta de traspaso de fondo con el detalle de folios consumidos y no consumidos
6. **Conciliación galonaje–kilometraje por tramo**, con el corte de odómetro que separa a los dos motoristas
7. La Orden de Misión reemitida, con la referencia cruzada al folio que sustituye, y el acta manuscrita que la precedió
8. El registro del regreso del motorista saliente y con cargo a qué se movió
9. Constancia de incapacidad o el respaldo del motivo del relevo, según lo que la institución exija — `[C]` insumo #48

## Trazabilidad

- **Reglas**: [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md) · [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-11`](../../01-negocio/reglas/RN-11-restricciones-medicas-del-motorista.md) habilitación · [`RN-12`](../../01-negocio/reglas/RN-12-disponibilidad-del-motorista.md) · [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md) · [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) **la regla rectora de este caso** · [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) · [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) · [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-29`](../../01-negocio/reglas/RN-29-liquidacion-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) · [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md) · [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md)
- **Reglas candidatas**: las seis de la sección anterior
- **Transiciones**: `T-17` subtipo relevo — es la transición del caso · `T-08`, `T-14` para el relevo previsto · `T-22` si falla la revalidación diferida
- **Bloqueos duros**: `BD-02` licencia, **no se levanta con autorización** · `BD-10` disponibilidad · `BD-11` solapamiento · `BD-06` segregación
- **Efectos**: `EF-01` reservas · `EF-02` reemisión con folio sustitutivo · `EF-03` **debe ampliarse con el padrón de relevo** · `EF-04` fondo
- **Criterios de hallazgo**: `H-07` bloqueo duro que falla al revalidar tras sincronizar
- **Puntos de control**: `PC-04` licencia · `PC-10` disponibilidad · `PC-03` — ver el hallazgo
- **Normativa**: [`NRM-06`](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) licencias `[P]` · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) · [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md)
- **Actores**: `ACT-06` saliente y entrante · `ACT-04` autoriza · `ACT-10` apoya desde la delegación · `ACT-07` convalida el fondo · `ACT-15` verifica en carretera · `ACT-08` cierra
- **Casos especiales relacionados**: `CE-02` avería y sustitución de vehículo · `CE-10` motorista incapacitado en ruta · `CE-11` licencia que vence durante la misión · `CE-13` motorista no disponible por Talento Humano · `CE-06` extensión de la misión
- **Insumo nuevo**: #48 — procedimiento institucional de relevo en ruta, quién lo autoriza desde delegación, y si existe límite de jornada de conducción
- **Historias candidatas**: `HU-c:relevar-motorista-en-ruta-sin-senal`, `HU-c:llevar-padron-de-relevo-en-el-paquete-de-mision`, `HU-c:traspasar-fondo-entre-motoristas-con-acta`, `HU-c:declarar-motorista-de-relevo-en-la-programacion`
