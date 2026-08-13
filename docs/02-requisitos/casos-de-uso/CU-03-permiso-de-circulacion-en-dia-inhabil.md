# CU-03 — Solicitar y emitir el permiso de circulación en día u hora inhábil

| Campo | Valor |
|---|---|
| **Módulos** | M-04 Documentación y Cumplimiento Vehicular · M-15 Formatos Oficiales e Impresión |
| **Actor principal** | `ACT-09` Máxima Autoridad — firma el permiso. Facultad expresamente suya por norma `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) |
| **Actores secundarios** | `ACT-04` Jefe de Transporte y `ACT-10` Encargado de Delegación (proponen el permiso), `ACT-05` Encargado de Despacho (entrega el salvoconducto impreso al motorista), `ACT-06` Motorista (lo porta), `ACT-15` Verificador en Carretera (**actor no autenticado**, destinatario del QR), `ACT-14` Encargado de Bienes Institucionales (reporte previo de resguardo, `PR-14`), `ACT-17` Sistema de Talento Humano (espejo del calendario de feriados) |
| **Precondiciones** | 1. Existe un expediente en `APROBADA` con la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` viva ([`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md)). 2. El calendario de días hábiles, feriados y horario laboral de la **delegación** está cargado y vigente para las fechas de la misión — parámetro, nunca cableado ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)). 3. `ACT-09` tiene rol vigente de máxima autoridad sobre la institución. 4. La delegación tiene rango de folios disponible y **capacidad de impresión** |
| **Postcondiciones** | Existe un permiso de circulación **firmado por `ACT-09`**, vigente para ese vehículo, ese motorista, esa ruta y esa ventana temporal, registrado con identidad, puesto, rol ejercido, momento, origen y huella del contenido (`PC-16`). Existe su **salvoconducto impreso** con folio único, QR verificable, espacio de firma y sello, huella del documento electrónico y **vigencia explícita desde–hasta** ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)). La marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` queda extinguida. `BD-04` queda satisfecha para `T-12`, e `INV-19` será verificable en `DESPACHADA` |
| **Disparador** | La aprobación de una solicitud cuya ventana toca, total o parcialmente, día inhábil, feriado u hora inhábil ([CU-02](CU-02-autorizar-solicitud-de-transporte.md) E4). También el reporte previo a Semana Santa exigido por [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` |

## Flujo principal

1. El sistema muestra a `ACT-04` —o a `ACT-10` en su delegación— el expediente aprobado con la marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` y **los tramos inhábiles señalados uno por uno**: qué día, desde qué hora y hasta qué hora, contra el calendario vigente **a las fechas de la misión** ([`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md)).
2. El sistema verifica si el vehículo previsto tiene **excepción de servicio exceptuado** vigente —emergencia, seguridad, salud—, que es atributo **del vehículo, no del viaje** ([`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md)). Si la tiene, sigue el flujo alterno A1.
3. `ACT-04` prepara la solicitud de permiso: justificación institucional de la circulación en franja inhábil, ruta, ventana temporal completa, vehículo y motorista. El permiso **no exige que la misión esté programada**, pero sí exige que el vehículo y el motorista estén resueltos, porque el permiso es nominativo sobre ellos ([`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md); ver la nota de hallazgo `HCU-04`).
4. El sistema comprueba que **no exista ya un permiso vigente** que cubra ese mismo vehículo, motorista, ruta y ventana. Si existe, lo muestra y no genera uno nuevo: dos permisos para una misma circulación rompen la conciliación.
5. El expediente de permiso se encamina a `ACT-09`. La pantalla presenta lo indispensable —vehículo, motorista, ruta, tramos inhábiles, justificación— y **debe caber en un teléfono y resolverse en dos toques**. Si no, `ACT-09` delega informalmente su clave, que es exactamente el riesgo que se quiere evitar `[I]`.
6. `ACT-09` firma el permiso. El sistema verifica que quien ejecuta el acto sea **el titular de la máxima autoridad**: esta facultad se trata como **indelegable** ([`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md)). `[C]` Si la norma admite delegación formal — insumo #29. **Hasta confirmarlo el sistema no la permite.**
7. El sistema registra el acto con identidad, puesto, rol ejercido en ese momento, marca de tiempo del hecho y de captura, dispositivo y **huella del contenido firmado** — `PC-16`, [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md). No hay firma electrónica certificada: la autorización es interna, con registro completo (`DP-001` D-04).
8. El sistema **emite el salvoconducto** con: folio único tomado del **rango de la delegación** ([`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md)), QR de verificación, espacio de firma y sello, huella del documento electrónico, identificación del vehículo por **correlativo institucional y placa si existe** ([`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md)), motorista, ruta y **vigencia explícita: desde cuándo y hasta cuándo ampara** ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)).
9. El salvoconducto **se imprime**. El control en carretera es físico: el destinatario del papel y del QR es `ACT-15`, que **no se autentica y no ve el expediente** `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md).
10. La marca `REQUIERE_PERMISO_MAXIMA_AUTORIDAD` se extingue por existir el permiso que la cubre. **No se puede retirar a mano** por ninguna otra vía.
11. `ACT-05` entrega el salvoconducto impreso a `ACT-06` junto con la Orden de Misión, dentro de `T-12` ([CU-05](CU-05-emitir-orden-de-mision-y-documentos.md)). Al despachar, `BD-04` verifica que el permiso esté vigente y el salvoconducto emitido — `PC-03`, **bloqueo duro**.
12. En `DESPACHADA` se verifica `INV-19`: si la ventana toca día u hora inhábil, existe el permiso de la máxima autoridad y su salvoconducto impreso.

## Flujos alternos

**A1 — Vehículo de servicio exceptuado** (desde el paso 2)

1. La excepción es **atributo del vehículo**, con fundamento y vigencia registrados ([`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md)) `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md). No es una casilla que se marca por viaje.
2. La excepción **se registra en la Orden de Misión con su fundamento y su vigencia**, y el despacho la considera equivalente al permiso.
3. Ver la nota de hallazgo `HCU-03`: `BD-04` no contempla esta excepción y exigiría permiso en todos los casos.
4. `[C]` Si la institución tiene vehículos de servicio exceptuado y quién declara la condición — insumo #1.

**A2 — Misión de varios días con franjas inhábiles intermedias** (desde el paso 3)

1. Los fines de semana y feriados intermedios son tramos inhábiles. El permiso **debe cubrir la ventana completa**.
2. No se fracciona en permisos diarios salvo que la institución lo exija. `[C]` insumo #1.
3. La vigencia impresa del salvoconducto es la de la ventana completa, con sus tramos señalados.

**A3 — Emisión anticipada para delegación sin cobertura** (desde el paso 8)

1. `ACT-10` emite e imprime el salvoconducto **antes de salir**, con folio pre-asignado del rango de su delegación `[V]` [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md).
2. Si la misión se modifica después, el papel que porta `ACT-06` deja de corresponder: la página de verificación debe poder devolver **desactualizado**, no solo *vigente* o *anulado* ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), caso límite).
3. Un rango de folios que se agota estando desconectado es un incidente previsible: el sistema alerta por consumo del rango con anticipación configurable. `[C]` Procedimiento de ampliación de rango sin conectividad — insumo #1.

**A4 — Reporte previo a Semana Santa y a feriados largos** (disparador propio)

1. El TSC realiza **operativos de fiscalización vehicular específicamente en Semana Santa** `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md). Es el pico anual de riesgo, y es predecible.
2. El sistema produce, para el período: los vehículos **autorizados a circular con su permiso vigente**, y los vehículos que **deben estar resguardados con confirmación de resguardo**. Lo prepara `PR-14` bajo `ACT-14`.
3. La carga sobre `ACT-09` es baja en frecuencia pero crítica y **se concentra antes de fines de semana, feriados y Semana Santa**: el reporte existe para que firme en lote y con contexto, no expediente por expediente a última hora.
4. `[P]` Multas reportadas de L 5,000 a L 50,000 más posible decomiso; base legal exacta `[C]`.

**A5 — Verificación del salvoconducto en carretera** (desde el paso 9)

1. `ACT-15` detiene el vehículo y escanea el QR. **No se autentica.**
2. El sistema devuelve el **mínimo verificable**: folio, tipo de documento, institución, estado —vigente, anulado, vencido o desactualizado—, vehículo y ventana temporal autorizada, más la huella del documento. **Nunca** el expediente, ni nombres de personas trasladadas, ni montos ([`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md)).
3. Cada consulta queda registrada —fecha, hora y origen—, y **cada verificación fallida también**: un patrón de folios inexistentes consultados es información valiosa.
4. Si `ACT-15` no tiene datos móviles, quedan el **contraste visual de la huella impresa**, el código de verificación corto y la consulta telefónica a la institución `[I]`.
5. `[C]` Si la institución acepta exponer un punto de verificación público siendo el despliegue on-premise — pendiente G de [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md).

**A6 — Reimpresión del salvoconducto perdido en ruta** (desde el paso 11)

1. Se reimprime **con el mismo folio y el mismo contenido**, registrando quién, cuándo y por qué.
2. **No se emite folio nuevo**: dos folios para un mismo permiso rompen la conciliación. El conteo de impresiones es dato de auditoría.

## Flujos de excepción

**E1 — `ACT-09` no está y alguien intenta delegar la firma** (en el paso 6)

1. El sistema **no permite la delegación de esta facultad**. La norma dice *firmado por la máxima autoridad* `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md), y no consta si admite delegación formal.
2. El intento se registra en la pista de auditoría con identidad y momento.
3. Las salidas legítimas son dos: reprogramar la ventana a franja hábil, o esperar la firma. **No hay una tercera.**
4. `[C]` Insumo #29. Si la institución confirma que la firma es delegable, se habilita por [`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) con vigencia acotada y folio del acto que la confiere; **hasta entonces, no**.

**E2 — Cambia el vehículo, el motorista, la ruta o la ventana** (después del paso 8)

1. El permiso es **específico**. Si cambia cualquiera de esos cuatro elementos, **deja de cubrir la misión** y debe reemitirse.
2. El salvoconducto anterior pasa a `ANULADO` con referencia cruzada, y **la página de verificación refleja el cambio de inmediato**, para que un papel anulado no pase un control ([`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md)). El folio anulado **no se recicla**.
3. Se emite un permiso nuevo, con firma nueva de `ACT-09`. La firma anterior no se arrastra.
4. Disparadores frecuentes: vehículo que entra a taller ([CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md)), motorista no disponible ([CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md)), relevo de motorista ([CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md)), sustitución de vehículo con recálculo de valores congelados ([`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md)).

**E3 — La misión se reprograma a otra fecha** (después del paso 8)

1. **El permiso no se arrastra a la nueva fecha**: se reemite.
2. Es el error más fácil de cometer y el que un operativo del TSC detecta de inmediato al comparar la fecha del papel con la del control.
3. El sistema no permite despachar amparado en un permiso cuya vigencia no cubre la ventana efectiva — `BD-04`.

**E4 — El permiso no llega antes de la hora prevista de salida** (en el paso 11)

1. **La misión no se despacha.** `BD-04` es bloqueo duro y no admite "despachar de todos modos".
2. El mensaje es accionable: *"La Orden de Misión N.º &lt;folio&gt; circula el sábado &lt;fecha&gt;. No existe permiso de circulación vigente de la máxima autoridad. Trámite pendiente desde el &lt;fecha&gt; (`RN-23`)."*
3. El retraso se registra **contra el expediente del permiso, no contra el motorista**.

**E5 — La prórroga en ruta empuja la misión a franja inhábil no cubierta** (fuera de este caso de uso, con la misión `EN_RUTA`)

1. `BD-04` se reevalúa en `T-17` cuando la prórroga extiende la misión a franja inhábil.
2. **No es bloqueable**: el vehículo ya está en carretera y `ACT-06` no siempre podrá obtener la autorización desde ahí. Se registra el hecho con **justificación obligatoria** y la extensión no autorizada queda marcada.
3. Puede emitirse un **permiso sobreviniente** si `ACT-09` lo autoriza —con el código de autorización fuera de línea si no hay red—. Si no se emite, la misión cierra con hallazgo `H-05` *circulación en día u hora inhábil sin permiso vigente, detectada al conciliar*.
4. **Nunca se ajusta la hora registrada para que "quepa" en el horario hábil.** Ver [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md).

**E6 — La delegación no tiene capacidad de impresión** (en el paso 9)

1. **No hay excepción**: sin salvoconducto impreso no se despacha en día inhábil.
2. Es un **requisito de despliegue**, no una excepción a la regla. `[C]` Verificar que toda delegación tenga capacidad de impresión — insumo #27.

**E7 — El calendario de feriados no está completo** (en el paso 1)

1. `[C]` Existe legislación posterior sobre los feriados de octubre que [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) **no pudo verificar** — insumo #14.
2. **No se codifica ninguna suposición.** El calendario se carga con los feriados confirmados y la institución completa el resto.
3. Un feriado mal cargado produce misiones ilegales o bloqueos infundados: el sistema debe mostrar, junto a la evaluación, la **versión y vigencia del calendario** con que se resolvió.
4. `[C]` Horario hábil oficial de la institución, y horario propio de las delegaciones de atención continua o fronterizas — insumo #32.

## Reglas aplicables

| Regla | Qué gobierna en este caso de uso |
|---|---|
| [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) | Permiso vigente firmado por la máxima autoridad; bloqueo **del despacho**, marca en la aprobación |
| [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) | La excepción es atributo del vehículo, con fundamento y vigencia |
| [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) | Salvoconducto impreso con folio, QR, firma y sello, huella y vigencia explícita |
| [`RN-03`](../../01-negocio/reglas/RN-03-registro-inmutable-de-autorizacion.md) | Registro inmutable del acto de firma — `PC-16` |
| [`RN-07`](../../01-negocio/reglas/RN-07-delegacion-de-autorizacion.md) | Delegación con vigencia acotada — **no habilitada para esta facultad** `[C]` insumo #29 |
| [`RN-15`](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) | Identificación del vehículo por correlativo institucional; la placa no es obligatoria ni única |
| [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-40`](../../01-negocio/reglas/RN-40-calculo-a-la-fecha-del-hecho.md) | Calendario y horario hábil como parámetros con vigencia, evaluados a la fecha del hecho |
| [`RN-44`](../../01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) | Folios de rango por delegación, para emisión anticipada sin conectividad |
| [`RN-51`](../../01-negocio/reglas/RN-51-minimizacion-de-datos-de-personas-externas.md) | La verificación pública nunca expone nombres de personas trasladadas |
| [`RN-61`](../../01-negocio/reglas/RN-61-sustitucion-de-vehiculo-recalcula-valores-congelados.md) | La sustitución de vehículo obliga a reemitir lo congelado, incluido el permiso |

## Notas de hallazgo

**`HCU-03` — `BD-04` no contempla el vehículo de servicio exceptuado.** `BD-04` exige permiso y salvoconducto **en todos los casos** en que la ventana toque franja inhábil, mientras que `PC-03` de [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) y [`RN-24`](../../01-negocio/reglas/RN-24-vehiculo-de-servicio-exceptuado.md) reconocen la excepción del vehículo de servicio exceptuado `[V]` [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md). Con `BD-04` tal como está, una ambulancia institucional con excepción vigente **no podría despacharse un domingo**. Ya está registrado como `HB1-21` y sigue abierto. Este caso de uso opera el flujo alterno A1 y **lo deja señalado contra la máquina de estados**, que es la autoridad y es la que debe corregirse.

**`HCU-04` — qué ampara el salvoconducto: tres redacciones distintas.** `BD-04` dice *"vigente para esa ventana y ese vehículo"*; `PC-03` de `PR-01` dice *"ese vehículo, motorista y ventana"*; [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) dice *"ese vehículo, ese motorista, esa ruta y esa ventana temporal"*. La diferencia no es cosmética: decide si un **relevo de motorista** ([CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md)) invalida el permiso y obliga a reemitirlo. Es la divergencia ya listada como abierta en el índice de casos especiales. Este caso de uso adopta la redacción **más exigente** —vehículo, motorista, ruta y ventana— porque es la conservadora ante un operativo del TSC, y porque una regla `RN-xx` es autoridad en la materia de negocio; se reporta para que `BD-04` se alinee.

**`HCU-05` — el permiso se puede tramitar en `APROBADA`, pero no se puede emitir sin vehículo y motorista.** [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) afirma que *"el permiso no requiere que la misión esté programada: basta con que esté aprobada"*, y a la vez que el permiso es específico por vehículo, motorista, ruta y ventana — datos que solo existen tras `T-08` ([CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md)). Ambas afirmaciones no se pueden cumplir a la vez si el permiso es nominativo. Resolución adoptada aquí: **el expediente de permiso se abre en `APROBADA`** —lo que preserva la corrección `HB1-08` contra el deadlock— **y su emisión y firma requieren vehículo y motorista resueltos**. Se reporta a la regla, no se resuelve en silencio.

## Trazabilidad

- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapa E3 (marca), E6 y E8 (emisión y entrega); **variante V-06 salida en día u hora inhábil**; punto de control `PC-03`; proceso `PR-07` como trámite propio y `PR-14` para el reporte previo
- **Transiciones**: `T-02` y `T-05` producen la marca; `T-12` es donde `BD-04` bloquea; `T-17` la reevalúa en prórroga — [máquina de estados](../../03-arquitectura/estados/orden-de-mision.md)
- **Invariantes**: `INV-19` en `DESPACHADA`
- **Bloqueos**: `BD-04` salida en día u hora inhábil sin permiso de la máxima autoridad
- **Criterios de hallazgo**: `H-05` circulación en día u hora inhábil sin permiso vigente, detectada al conciliar
- **Actores**: [`actores-y-roles.md`](../../01-negocio/actores-y-roles.md) — `ACT-09` (facultad indelegable `[C]`), `ACT-04`, `ACT-05`, `ACT-06`, `ACT-10`, `ACT-14`, `ACT-15` **no autenticado**
- **Casos especiales**: [CE-05](../casos-especiales/CE-05-cambio-de-motorista-con-la-mision-en-curso.md) y [CE-13](../casos-especiales/CE-13-motorista-no-disponible-por-talento-humano.md) y [CE-16](../casos-especiales/CE-16-vehiculo-a-taller-con-misiones-programadas.md) (E2), [CE-06](../casos-especiales/CE-06-la-mision-se-extiende-mas-dias-destinos-o-kilometros.md) (E5), [CE-17](../casos-especiales/CE-17-vehiculo-sin-placa-metalica.md) (paso 8: el salvoconducto identifica por correlativo institucional, no por placa). **Descartados explícitamente:** [CE-20](../casos-especiales/CE-20-mision-cancelada-con-combustible-ya-entregado.md) a [CE-28](../casos-especiales/CE-28-hallazgo-posterior-sobre-mision-cerrada.md) — combustible, peajes y liquidación no condicionan la emisión del permiso
- **Normativa**: [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[V]` prohibición de uso en días y horas inhábiles y permiso firmado por la máxima autoridad; `[P]` rango de multas; `[C]` cita completa del Decreto 48 — **el eslabón más débil de la cadena está declarado en `RN-23`, hallazgo `HN1-19`** · [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` feriados del Art. 339 del Código del Trabajo y operativos del TSC en Semana Santa; `[C]` feriados de octubre · [NRM-08](../../01-negocio/normativa/NRM-08-firma-electronica.md) documentos que siguen requiriendo papel y página pública de verificación
- **Decisiones**: [DP-001](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) D-04 · premisa rectora 4 de `CLAUDE.md`
- **Insumos pendientes**: #1, #14, #27, #29, #32, pendiente G en [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md)
- **Aguas arriba**: [CU-02](CU-02-autorizar-solicitud-de-transporte.md) · **Aguas abajo**: [CU-04](CU-04-programar-mision-asignar-vehiculo-y-motorista.md), [CU-05](CU-05-emitir-orden-de-mision-y-documentos.md)
