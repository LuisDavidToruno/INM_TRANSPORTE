# CE-08 — Cinco horas cuarenta en el portón de la bodega de San Lorenzo, con el motor encendido

| Campo | Valor |
|---|---|
| **Módulos** | M-19 Seguimiento en Ruta, M-08 Ejecución y Bitácora, M-07 Programación y Despacho, M-09 Combustible, M-18 Peajes, M-13 Liquidación, M-14 Reportes e Indicadores, M-16 Operación Desconectada |
| **Estados afectados** | `EN_RUTA`, con `T-17` si la espera obliga a extender la ventana |
| **Frecuencia** | **Frecuente.** En misiones de reparto y de comisión es la norma, no la excepción |
| **Impacto** | Operativo y financiero. Y de auditoría por vía indirecta: la espera es la explicación de la desviación de consumo que hoy nadie puede probar |
| **Resolución** | Definida. Requisito explícito del PO ([DP-001, D-06](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md)). Una decisión al PO sobre ventanas de atención en destino |

## La situación

Miércoles 05:40. Un camión liviano sale de Tegucigalpa con material de oficina para cuatro dependencias del sur. Odómetro 88,120. Ruta autorizada, en este orden: **Sabanagrande, San Lorenzo, Nacaome, Choluteca**, y retorno el mismo día. Ventana autorizada: 05:30 a 18:00. Distancia estimada 380 km. Es una misión de un solo día, con un solo motorista, cuatro arribos y cuatro entregas.

Lo que pasa de verdad:

| Destino | Arribo | Salida | En sitio | Qué ocurrió |
|---|---|---|---|---|
| Sabanagrande | 06:55 | 07:25 | 30 min | Entrega normal. Descargaron entre los dos, firmaron el acta |
| **San Lorenzo** | 08:50 | **14:30** | **5 h 40 min** | El jefe de bodega andaba en una reunión en Choluteca. Nadie más estaba autorizado a firmar la recepción. El motorista esperó en el portón |
| Nacaome | 15:15 | 17:05 | 1 h 50 min | Entrega grande: 62 cajas, descargadas por el motorista y un ordenanza |
| Choluteca | 18:40 | 19:10 | 30 min | **Llegó después del horario de recepción.** Dejó el material sin acta firmada, o lo trajo de vuelta |

Retorno a Tegucigalpa: **23:50**. La ventana autorizada cerraba a las 18:00.

En San Lorenzo, a 38 grados, el motorista mantuvo el motor encendido buena parte de la espera para no cocinarse dentro de la cabina. Cuando el camión vuelve, la cuenta dice: **372 km recorridos y 31 galones consumidos**. El rendimiento sale muy por debajo de lo esperado para ese vehículo, y el sistema, si no sabe nada de las cinco horas cuarenta, lo va a marcar como desviación de consumo sin justificación — es decir, `H-01`.

Mientras tanto, en la sede, el Jefe de Transporte miró el tablero a las 11:00 y vio un camión detenido en San Lorenzo desde las 08:50. **No supo si estaba esperando, si se había averiado, o si el motorista se había ido a hacer un mandado.** No hizo nada. Una llamada a las 09:30 a la dependencia de San Lorenzo habría desatascado el asunto y ahorrado cuatro horas de camión, un motorista, y el retorno a medianoche.

## Qué se hace hoy sin sistema

La bitácora en papel tiene casillas de **arribo** y **salida** por destino. Casi siempre se llenan al final del día, de memoria, con horas redondeadas. El tiempo en sitio nunca se totaliza porque a nadie le sirve de nada tenerlo suelto en una hoja.

La espera se cuenta después, de palabra: *"es que en San Lorenzo nos tuvieron toda la mañana"*. No queda por escrito, no se atribuye a nadie, y **no llega nunca a la liquidación**, que es justo donde haría falta para explicar los 31 galones.

Y hay una consecuencia silenciosa: como las esperas no se miden, la programación sigue asumiendo que una entrega toma media hora. Se siguen programando cuatro destinos en un día que no caben en un día.

`[C]` **Si las dependencias y bodegas tienen horario de recepción comprometido**, y si alguien puede confirmar la disponibilidad del receptor antes de que el vehículo salga. Insumo nuevo #51.

## Por qué el flujo normal no lo cubre

El seguimiento en ruta existe (M-19) y la bitácora registra arribos. El vacío está en otra parte:

1. **El sistema no puede inferir la espera, y no debe intentarlo.** `EF-07` es explícito: *"el servidor no debe inferir nada del silencio posterior"*. Un vehículo que no reporta movimiento puede estar esperando, sin señal, con el dispositivo descargado, o averiado. Sin declaración del motorista, "detenido" no significa nada — y un tablero que muestra "en espera" cuando en realidad hay un vehículo averiado es peor que un tablero vacío.
2. **La espera no está tipificada.** Descargar 62 cajas durante 1 h 50 min y esperar 5 h 40 min a que aparezca quien firme son dos cosas distintas con el mismo aspecto: el vehículo detenido en un punto. Medirlas juntas produce un indicador que no sirve para decidir nada.
3. **Nadie recibe la señal a tiempo.** La única persona que puede desatascar la espera de San Lorenzo está en Tegucigalpa, y se entera al día siguiente.
4. **La espera es la causa de tres desviaciones distintas** — consumo, ventana y cumplimiento — y hoy no está enganchada a ninguna. `RN-30` ya nombra el problema: *"tiempo prolongado de motor encendido en espera... M-19 mide los tiempos de espera en sitio: esa medición es la que explica la desviación. **Sin ella, el hallazgo sería infundado**"*. La medición está prometida y no está diseñada.
5. **El cumplimiento del objeto es por destino, no por misión.** Tres entregas conformes y una no atendida no es "misión cumplida" ni "misión no cumplida".

## Regla de resolución

**1. El motorista declara su estado; el sistema no lo adivina.** `ACT-06` actualiza su propio estado desde el dispositivo, con un catálogo cerrado y corto:

   | Estado en ruta | Qué significa |
   |---|---|
   | `EN_TRANSITO` | Se está moviendo hacia el siguiente destino |
   | `EN_SITIO_OPERANDO` | Llegó y está haciendo lo que vino a hacer: cargando, descargando, entregando, esperando a la comisión que atiende su diligencia |
   | `EN_SITIO_ESPERANDO` | Llegó y **no puede operar**: no hay quién reciba, no hay quién descargue, el sitio está cerrado |
   | `DETENIDO_POR_NOVEDAD` | Se detuvo por algo que no es un destino: falla, retén, clima, descanso obligado |

   El estado se declara **sin conectividad** y se encola con la marca de tiempo del dispositivo ([`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md)). El tablero muestra siempre la **antigüedad del dato** — nunca una posición vieja presentada como actual.

**2. Un toque. No más.** Cambiar de estado y registrar arribo o salida de un destino tiene que costar **un toque, en la pantalla principal, sin escribir nada**. La regla operativa de `PR-01` es inapelable: *"todo lo que le exija a `ACT-06` más de un minuto o más de tres toques por registro se llenará en papel y se digitará después, mal"*. Un cronómetro que el motorista tenga que administrar es un cronómetro que nadie va a usar. **El tiempo en sitio no se captura: se deriva** de arribo y salida, que sí se capturan.

**3. La distinción entre operar y esperar es el producto del caso.** Solo `EN_SITIO_ESPERANDO` cuenta como **espera improductiva**, y solo esa se atribuye. Al declararla, el motorista elige una causa del catálogo configurable en un segundo toque: *no hay quién reciba · no hay quién descargue · el sitio está cerrado · falta documentación en destino · esperando instrucción · otra*. Y el sistema registra **a qué destino y a qué dependencia** se estuvo esperando.

   Sin esa atribución, el indicador dice "el camión estuvo parado seis horas" y todo el mundo entiende que el problema es del motorista. **Con la atribución, dice quién lo tuvo parado.**

**4. Motor encendido: un toque más, y evita un hallazgo.** Al declarar espera, se registra si el motor quedó encendido — sí, no, parcialmente. No se pide duración exacta ni consumo estimado: **eso es un cálculo del sistema, no una carga para el motorista.** Este dato es lo que convierte los 31 galones de una anomalía en un hecho explicado. Engancha directo con [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) y con `CE-21`: la conciliación galonaje–kilometraje incorpora el tiempo de espera con motor encendido como **variable de la explicación**, no como excusa escrita a mano tres semanas después.

**5. Superado el umbral, alguien en la sede se entera.** La espera improductiva que supera un umbral configurable — por institución, por tipo de misión, parámetro con vigencia ([`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)) — genera aviso a `ACT-04` y a la dependencia solicitante **en cuanto haya señal**. Sin señal no se pierde: se encola y llega tarde, pero llega, y queda con la hora del hecho.

   El valor de esto no es de control: es operativo. Una espera de cinco horas casi siempre se resuelve con una llamada desde la sede, si alguien en la sede sabe que está ocurriendo.

**6. Cada destino tiene su propio cumplimiento.** En misión multi-destino, el grado de cumplimiento del objeto — el mecanismo que `CE-07` establece — **es por destino**: atendido, atendido parcialmente, no atendido, con causa tipificada y con acta de entrega y quién recibió, o la constancia de que no hubo quién recibiera. Choluteca en el ejemplo queda **no atendido por arribo fuera de horario de recepción**, con el material devuelto y su acta de reingreso.

   La misión, en conjunto, cierra como **parcialmente cumplida**. Y esa es la verdad, que hoy no aparece en ningún lado.

**7. Reordenar los destinos no es desviarse de la ruta.** Si el motorista invierte el orden porque una bodega cierra a mediodía, la secuencia sigue siendo legítima mientras sea **geográfica y temporalmente coherente** ([`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)). Se registra el reordenamiento con su motivo; si cambia los pasos por caseta previstos, el estimado se recalcula con el **paquete congelado** (`EF-03`, [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md)). Lo que dispara `H-03` es la incoherencia, no el reordenamiento justificado.

**8. La espera que rompe la ventana es una extensión, y se trata como tal.** El retorno a las 23:50 excede la ventana autorizada de 18:00 y puede caer en hora inhábil según el calendario de la delegación. Se aplica `CE-06` completo: `T-17`, revalidación de habilitaciones contra la nueva fecha de fin, `BD-04` con justificación registrada si no se pudo tramitar el permiso, y `H-05` si no se justifica. **El motorista no puede parar de trabajar a las 18:00 en Nacaome**, pero sí puede registrar por qué se pasó.

**9. La espera medida vuelve a la programación, que es donde sirve.** Los tiempos en sitio históricos por destino y por tipo de entrega alimentan el estimado de duración de las misiones siguientes. Programar cuatro entregas en un día deja de ser un acto de fe. Y el reporte de espera improductiva acumulada por dependencia y por destino es, junto con el conflicto de reserva de `EF-01` y las misiones abortadas de `CE-07`, uno de los pocos indicadores que una institución puede llevar a una gestión presupuestaria con evidencia propia.

**10. Lo que la espera cuesta se cuantifica y se atribuye.** Horas de vehículo inmovilizado, horas de motorista, y combustible consumido en ralentí. `[C]` el costo-hora de vehículo, que es dato de la institución. Sin él, el indicador se expresa en horas, que ya es infinitamente más de lo que hay hoy.

### Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:estado-en-ruta-declarado-por-el-motorista` | El estado del vehículo en ruta lo declara `ACT-06` desde un catálogo cerrado, sin conectividad y con un toque. **El sistema nunca infiere el estado a partir de la ausencia de movimiento o de señal**, y todo dato mostrado exhibe su antigüedad |
| `RN-c:tiempo-en-sitio-derivado-de-arribo-y-salida` | El tiempo en sitio se deriva de los eventos de arribo y salida por destino, con el reloj del dispositivo. Nunca se pide al motorista que lo cronometre ni que lo digite |
| `RN-c:espera-improductiva-tipificada-y-atribuida` | La espera en que el vehículo no puede operar se tipifica por causa y se atribuye al destino y a la dependencia responsable; solo esa cuenta como espera improductiva en los indicadores |
| `RN-c:motor-encendido-en-espera-como-variable-de-conciliacion` | El motor encendido durante la espera se registra con un toque y entra como variable en la conciliación galonaje–kilometraje. Una desviación de consumo con espera prolongada registrada no produce hallazgo por sí sola |
| `RN-c:aviso-por-espera-sobre-umbral` | La espera improductiva que supera el umbral configurable notifica a `ACT-04` y a la dependencia solicitante en cuanto haya señal, sin perderse si no la hay |
| `RN-c:cumplimiento-por-destino-en-mision-multidestino` | En misión multi-destino el grado de cumplimiento se declara por destino, con acta de entrega o constancia de no atención; la misión cierra con el consolidado |
| `RN-c:reordenamiento-de-destinos-justificado` | El cambio de orden de los destinos se registra con motivo y no constituye desviación de ruta si la secuencia sigue siendo geográfica y temporalmente coherente; el estimado de peajes se recalcula con el paquete congelado |
| `RN-c:tiempos-en-sitio-historicos-alimentan-la-programacion` | La duración estimada de una misión multi-destino se calcula con los tiempos en sitio históricos del destino y del tipo de operación, no con un valor fijo |

## Escalamiento al PO

`[C]` **¿El destino compromete una ventana de atención?** Insumo nuevo #51. Es la pregunta que decide si este caso se previene o solo se documenta.

| Opción | Costo |
|---|---|
| No se modela: el vehículo llega cuando llega | Es lo que hay hoy. La espera se mide y se atribuye, pero nadie la evita. El indicador sirve para reclamar después, no para no perder la mañana |
| Cada dependencia y bodega registra su **horario de recepción**, y la programación advierte si el arribo estimado cae fuera | Barato de modelar — es un catálogo con vigencia más — y evita el caso de Choluteca, que es una entrega perdida entera. Costo: alguien tiene que mantener el catálogo actualizado, y un horario desactualizado es peor que ninguno |
| Además del horario, **confirmación del receptor antes del despacho**: hay quién reciba ese día | Evita también el caso de San Lorenzo, que es el caro. Costo: agrega un paso al despacho que depende de un tercero fuera de la Gerencia Administrativa, y puede trabar salidas |

**Recomendación del análisis**, no decisión: la segunda de entrada, con la tercera como **advertencia no bloqueante** para misiones de reparto. Trabar un despacho porque una bodega no contestó el teléfono es cambiar un problema por otro peor.

## Evidencia que debe quedar

1. Por cada destino: **arribo y salida con hora y odómetro**, y el tiempo en sitio derivado de ambos
2. Tipificación de cada período en sitio — operando o esperando — con su causa y la dependencia a la que se esperó
3. Registro de motor encendido durante la espera
4. **La conciliación galonaje–kilometraje con el tiempo de espera incorporado como explicación de la desviación** — que es exactamente lo que convierte un hallazgo infundado en un hecho documentado
5. Acta de entrega por destino con quién recibió, o constancia de no atención con su causa
6. Grado de cumplimiento por destino y consolidado de la misión
7. Si hubo reordenamiento de destinos: el motivo y el recálculo del estimado de peajes con el paquete congelado
8. Si la espera rompió la ventana autorizada: la extensión registrada, su autorización o su justificación, y el tratamiento de la franja inhábil
9. El aviso emitido por espera sobre umbral, con la hora en que se generó y la hora en que se entregó
10. Acta de reingreso del material no entregado

## Trazabilidad

- **Reglas**: [`RN-06`](../../01-negocio/reglas/RN-06-transiciones-de-estado-de-la-orden.md) · [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md) si la espera empuja a franja inhábil · [`RN-28`](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) **la regla que este caso hace verificable** · [`RN-31`](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) · [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [`RN-37`](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md) · [`RN-39`](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) · [`RN-43`](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [`RN-45`](../../01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md), [`RN-46`](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [`RN-47`](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md) · [`RN-53`](../../01-negocio/reglas/RN-53-cierre-del-manifiesto-al-despacho.md) si se trasladan personas
- **Reglas candidatas**: las ocho de la sección anterior, más `RN-c:antiguedad-visible-del-dato-de-ruta` ya levantada en `PR-01` E10
- **Transiciones**: `T-14` abre la bitácora y el seguimiento · `T-17` si la espera obliga a extender la ventana · `T-18` con `EF-05`, donde la espera entra a la conciliación · `T-19` calcula los tiempos de espera como indicador de la misión
- **Bloqueos duros**: `BD-04` franja inhábil sobrevenida · `BD-05` coherencia del odómetro
- **Efectos**: `EF-03` paquete congelado para el recálculo de peajes · `EF-05` conciliación de kilometraje y tiempos · `EF-07` **la fuente primaria es el dispositivo y el servidor no infiere nada del silencio**
- **Criterios de hallazgo**: `H-01` consumo — **amparado si la espera está registrada** · `H-03` caseta incoherente · `H-05` franja inhábil sin permiso
- **Puntos de control**: `PC-11` coherencia del odómetro · `PC-14` falta de ticket de caseta
- **Proceso**: [`PR-01`](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) etapas **E9 bitácora** y **E10 seguimiento en ruta**
- **Fronteras**: [DP-001, D-06](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — tiempos de espera en sitio medidos y visibles; el componente de mapas es el de ARGOS, `[C]` insumo #18
- **Normativa**: [`NRM-09`](../../01-negocio/normativa/NRM-09-realidad-operativa.md) conectividad y horarios · [`NRM-01`](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) correlación consumo–kilometraje–misión · [`NRM-10`](../../01-negocio/normativa/NRM-10-peajes.md)
- **Actores**: `ACT-06` declara su estado · `ACT-04` ve el tablero y desatasca · `ACT-03` dependencia solicitante notificada · `ACT-10` recibe en delegación · `ACT-08` cierra · `ACT-16` ARGOS aporta el componente de mapas
- **Casos especiales relacionados**: `CE-06` extensión de la misión · `CE-07` retorno anticipado y grado de cumplimiento · `CE-21` galonaje que no cuadra con kilometraje — **este caso es la mitad de su explicación** · `CE-12` competencia por el mismo vehículo, que la espera agrava
- **Insumos**: **#51 nuevo** — horarios de recepción de dependencias y bodegas, y si se puede confirmar receptor antes del despacho · #18 componente de mapas de ARGOS · #1 costo-hora de vehículo y umbral de espera
- **Historias candidatas**: `HU-c:declarar-estado-en-ruta-con-un-toque`, `HU-c:registrar-arribo-y-salida-por-destino`, `HU-c:tipificar-y-atribuir-la-espera-improductiva`, `HU-c:avisar-a-la-sede-por-espera-sobre-umbral`, `HU-c:ver-tablero-de-ruta-con-antiguedad-del-dato`, `HU-c:declarar-cumplimiento-por-destino`
