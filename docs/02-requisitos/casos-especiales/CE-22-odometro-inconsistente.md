# CE-22 — El odómetro marca menos que la vez pasada, o marca un salto imposible, o le cambiaron el tablero

| Campo | Valor |
|---|---|
| **Módulos** | M-08 Bitácora, M-03 Flota, M-09 Combustible, M-11 Mantenimiento, M-13 Liquidación, M-14 Auditoría |
| **Estados afectados** | `DESPACHADA` (lectura de salida), `EN_RUTA` (cada carga y cada parada), `RETORNADA` (`PC-11`), `LIQUIDADA`; y fuera de misión: ingreso y salida de taller, constatación física |
| **Frecuencia** | El error de digitación es **frecuente**. El reemplazo de tablero es **ocasional**. La manipulación deliberada es **rara pero grave** |
| **Impacto** | Auditoría, financiero y patrimonial — el odómetro es el denominador de toda la conciliación de combustible |
| **Resolución** | Definida. Máximos de salto `[C]`. Bloqueo por odómetro averiado `[C]`, escalado al PO |

## La situación

Son tres cosas distintas que llegan por la misma pantalla y **no se resuelven igual**. Confundirlas es lo que corrompe el histórico.

### Retroceso

Pickup Toyota Hilux `INS-PU-014`. La última lectura registrada del vehículo es **148,940 km**, capturada en una carga de combustible en la estación de Villanueva. Hoy sale a Choluteca y el motorista lee el tablero: **148,540**. Cuatrocientos kilómetros menos que la última vez.

Nadie desarmó nada. Lo que pasó fue que en aquella carga el que digitó tecleó *148,940* donde el tablero decía *148,490*: se le corrió un dígito. **El error está en la lectura vieja, no en la de hoy** — y el sistema, si obliga a "cuadrar" hacia adelante, va a hacer que el error se vuelva permanente y que 450 km fantasma queden imputados al vehículo para siempre.

### Salto imposible

Microbús `INS-MB-003`. Misión de un día: Tegucigalpa → Danlí → Tegucigalpa, **184 km ida y vuelta** por la CA-6. La bitácora de retorno registra un recorrido de **1,240 km**. El motorista escribió *149,240* donde el tablero marcaba *148,240*.

Pero el mismo salto lo produce otra cosa completamente distinta: que el vehículo **sí** haya recorrido kilómetros que la misión no autorizaba. Un salto de 400 km en un vehículo autorizado a Danlí no se distingue de un error de dedo mirando solo el número. Se distingue cruzándolo contra la ruta autorizada, contra las casetas y contra el combustible.

### Reemplazo del tablero

Pickup `INS-PU-021`, con **212,450 km** acumulados. Se le quema el tablero — falla eléctrica corriente en unidades de más de diez años. El taller consigue un tablero de repuesto usado, lo instala, y el instrumento nuevo marca **63,180 km**.

Al día siguiente el vehículo sale a misión y el motorista registra 63,180 de salida. A partir de ahí:

- El vehículo **rejuvenece 149,270 km** en el inventario de bienes.
- El servicio preventivo de los 215,000 km **no se dispara nunca**.
- El rendimiento histórico del vehículo queda irreconciliable hacia atrás.
- Y ninguna de las tres cosas produce un error visible: el sistema simplemente empieza a contar desde otro lado.

Esto **no es una excepción a corregir. Es un hecho legítimo del bien**, y si el modelo no lo previó, cada tablero cambiado destruye el histórico de forma irreversible.

## Qué se hace hoy sin sistema

`[C]` No verificado con la institución. Hay que levantarlo con la bitácora en papel a la mesa (insumo #2) y con el Encargado de Transporte y el de Mantenimiento (insumo #1).

Lo que se observa en instituciones comparables:

En la hoja de bitácora el motorista escribe lo que marca el tablero, sin más. Cuando el número sale menor que el de la hoja anterior, ocurre una de dos: **se tacha y se "arregla" para que cuadre**, o nadie lo nota, porque la hoja anterior está en otro folder y la comparación exige buscarla.

Y cuando se cambia el tablero, la lectura vieja y la nueva quedan anotadas en la **orden de trabajo del taller** — si el mecánico se acordó de anotarlas. El kilometraje del vehículo se reinicia de hecho.

> **La regla que nadie escribió:** el kilometraje verdadero del vehículo no vive en ningún registro. Vive en la memoria del Encargado de Transporte y en una orden de trabajo archivada. Cuando esa persona rota de puesto, el dato se pierde — y es el dato que sostiene el valor del bien en el inventario y el plan de mantenimiento preventivo.

## Por qué el flujo normal no lo cubre

Porque el flujo feliz trata **la lectura del instrumento y el kilometraje del vehículo como si fueran la misma cosa**. No lo son, y la diferencia no es filosófica:

| | Lectura del instrumento | Kilometraje acumulado del vehículo |
|---|---|---|
| Qué es | Lo que marca un aparato instalado en el tablero | Cuánto ha rodado el bien desde que entró a la flota |
| Puede retroceder | Sí — reemplazo, vuelta de contador, cambio de unidad | **Nunca** |
| Sobrevive al taller | No | Sí |
| Sirve para | Capturar y verificar en campo | Mantenimiento, valor del bien, conciliación histórica |

[RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) ya dice lo correcto —*"el kilometraje acumulado del vehículo se lleva como valor derivado, independiente de la lectura del instrumento"*— pero lo dice como **comportamiento 4 y caso límite de una regla de validación de captura**. Una regla que se llama *"el odómetro de retorno no puede ser menor al de salida"* se puede implementar entera, y pasar sus pruebas, guardando únicamente lecturas. Ese es el hueco.

Y hay un segundo hueco: [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) admite *"reemplazo del odómetro o del tablero"* como **motivo tipificado dentro de la captura de una bitácora**. Es decir, el ajuste de kilometraje de un bien del Estado puede nacer desde el mismo formulario donde un motorista registra su retorno a las nueve de la noche. La intervención del odómetro es el acto con mayor incentivo de manipulación de toda la flota; no puede tener esa puerta.

## Regla de resolución

**1. Dos magnitudes separadas en el modelo, no una con correcciones.**

- La **lectura** pertenece siempre a una **serie de instrumento** con vigencia: instrumento 1 desde el alta del vehículo hasta su reemplazo, instrumento 2 desde ahí. Cada serie tiene lectura inicial, lectura de cierre y unidad declarada (km o millas).
- El **kilometraje acumulado** es atributo derivado del **expediente del vehículo** (M-03): suma de los recorridos de cada serie. No lo escribe nadie a mano y **no puede decrecer**.

Toda pregunta de negocio —mantenimiento preventivo, rendimiento, valor del bien, reporte al TSC— se responde contra el **acumulado**. La lectura del instrumento solo sirve para capturar y para verificar en el tablero.

**2. En campo se registra, no se resuelve.** El motorista captura la lectura observada más **fotografía del tablero**, sin conectividad ([RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md)). Si hay inconsistencia, el sistema se la muestra con el cálculo explícito —*"Última lectura conocida: 148,940 km el 14/07 (carga en Villanueva). Lectura ingresada: 148,540 km. Retroceso de 400 km."*— y le permite marcar un motivo probable. **Él no corrige nada** ([RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md)): registra y sigue viaje.

**3. La lectura observada nunca se descarta.** Se conserva junto con la justificación y con la lectura finalmente aceptada ([RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md), [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)). Guardar solo el número "bueno" es exactamente lo que hace el papel cuando se tacha.

**4. Los tres subcasos se resuelven distinto:**

| Subcaso | Cómo se detecta | Qué hace el sistema |
|---|---|---|
| **Retroceso** | Lectura menor que la última conocida del vehículo **en su línea de tiempo**, no solo dentro de la misión | `PC-11` bloquea el cierre de bitácora hasta justificar. Si el motivo resuelto es *error de captura previo*, **se corrige la lectura vieja** con asiento reverso ([RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md)) y se **recalculan** las conciliaciones de combustible y peaje afectadas con asiento de diferencia ([RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)) — nunca sobrescribiendo el resultado histórico |
| **Salto imposible** | Delta superior a `salto_maximo_km_por_dia` o `salto_maximo_km_por_hora` del **tipo de vehículo**, parámetro con vigencia ([RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md)). Una motocicleta y un cabezal no tienen el mismo techo | Mismo bloqueo de `PC-11`. Y **el sistema no adivina**: presenta el cruce que discrimina entre error de dedo y recorrido no autorizado — kilómetros de la ruta autorizada, secuencia de casetas ([RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md)), galones consumidos ([RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)) y posiciones de M-19. Un salto de 1,000 km sin combustible que lo sostenga es digitación; con combustible que lo sostenga es `H-02` |
| **Reemplazo del tablero** | **Evento propio del expediente del vehículo**, no una excepción de bitácora | Cierra la serie del instrumento anterior con su última lectura, abre serie nueva con su lectura inicial, registra el desfase, y **el acumulado no se toca**. Exige orden de trabajo de M-11, fotografía del tablero anterior y del nuevo, y autorización de ACT-04 con respaldo técnico de ACT-11. **No se puede originar desde el formulario de retorno** |

**5. Odómetro averiado: no se inventa lectura.** Es una falla del vehículo y se reporta a M-11. Mientras dure, el consumo se registra con odómetro *no disponible por falla*, referenciando el reporte ([RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md)), y la conciliación del período se marca **no concluyente** ([RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md)). Un hallazgo falso repetido hace que nadie vuelva a mirar los hallazgos.

**6. Lectura de retorno igual a la de salida:** advertencia, no bloqueo. Técnicamente no viola nada, pero significa que hubo consumo y tiempo de misión sin movimiento. Es tan sospechoso como un retroceso ([RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md)).

**7. Contador que da la vuelta y odómetro en millas** se tratan con la misma mecánica de series: la vuelta de contador es un **reemplazo lógico** con motivo tipificado propio; la unidad se declara en la ficha del vehículo y toda lectura se almacena normalizada **conservando la unidad original**. Asumir kilómetros sobre un tablero en millas produce un error del 60% que nadie detecta hasta que la conciliación es absurda.

**8. El plan de mantenimiento preventivo se calcula sobre el acumulado, jamás sobre la lectura.** Si no, cambiar un tablero pospone indefinidamente un servicio, y la falla que venga después es responsabilidad de quien autorizó el vehículo.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C22a` | *El kilometraje acumulado del vehículo es atributo derivado y propio de su expediente, independiente de la lectura de cualquier instrumento. Toda lectura pertenece a una serie de instrumento con vigencia y unidad declarada; el reemplazo del instrumento cierra una serie y abre otra, y el acumulado no decrece nunca.* | [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) lo enuncia como comportamiento y caso límite de una regla de **validación de captura**. La existencia del acumulado no es una validación: es un **invariante del expediente del vehículo**, y ninguna regla de M-03 ([RN-15](../../01-negocio/reglas/RN-15-identidad-del-vehiculo-y-placa.md) a [RN-22](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md)) lo obliga. Sin regla propia, `RN-31` se implementa entera guardando solo lecturas |
| `RN-C22b` | *La intervención del instrumento de medición —reemplazo, reparación, vuelta de contador, cambio de unidad— se registra como evento del expediente del vehículo, con orden de trabajo, lecturas antes y después, fotografía de ambos tableros y autorización nominativa. Ningún ajuste de kilometraje puede originarse fuera de ese evento.* | Hoy [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) permite que el ajuste nazca como motivo tipificado dentro de una bitácora. Es el acto con mayor incentivo de manipulación de toda la flota entrando por la puerta de menor control |
| `RN-C22c` `[C]` | *Un vehículo con odómetro declarado averiado no se programa a nueva misión, salvo autorización expresa y registrada de ACT-04 con motivo.* | [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) bloquea por **estado operativo**, y un odómetro roto no vuelve al vehículo `NO_DISPONIBLE`: rueda perfectamente. **Escalado al PO** — ver abajo |

### `[C]` Escalado al PO — odómetro averiado y programación

No hay respuesta correcta obvia y la decisión tiene costo en ambos sentidos:

| Opción | Costo |
|---|---|
| **Bloquear** la programación mientras el odómetro esté averiado | Una institución con flota vieja pierde unidades operativas por una falla que no impide rodar. En delegaciones con dos vehículos, paraliza |
| **Advertir** y dejar programar | El vehículo circula sin denominador: su consumo del período **no es conciliable** y el TSC lo va a ver como período sin control |
| **Parámetro configurable** con plazo — advierte, y bloquea si la falla lleva más de N días sin reparar | Es la que se propone. Exige fijar N `[C]` y aceptar que durante N días la conciliación queda no concluyente |

## Evidencia que debe quedar

Ante el TSC, la institución debe poder producir:

1. La **serie completa de lecturas del vehículo en línea de tiempo**, con el origen de cada una — salida, carga de combustible, parada, retorno, ingreso y salida de taller, constatación física —, no solo las de retorno
2. Por cada inconsistencia: **lectura observada, lectura aceptada, motivo tipificado, respaldo documental, quién resolvió y cuándo**. Las dos lecturas, siempre las dos
3. Por cada reemplazo de instrumento: orden de trabajo del taller, lectura de cierre del instrumento anterior, lectura inicial del nuevo, **desfase calculado**, fotografías de ambos tableros y autorizador nominado
4. El **kilometraje acumulado del vehículo a cualquier fecha pasada, reproducible** — es lo que sostiene el valor del bien en el inventario, el descargo cuando se dé de baja, y el plan de mantenimiento
5. Las conciliaciones de combustible **recalculadas** tras una corrección retroactiva, con su asiento de diferencia ([RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md)). Nunca la conciliación vieja sobrescrita
6. Los períodos marcados **no concluyentes** por odómetro averiado, con el reporte de falla que los sustenta. Un período sin dato declarado como tal se defiende; un período sin dato con números inventados no
7. Y lo que el auditor realmente busca: **kilómetros del período por vehículo contra galones contra misiones autorizadas**, con las inconsistencias resueltas **visibles**, no borradas

## Trazabilidad

- **Autoridad de transiciones:** [`PC-11` coherencia del odómetro](../../01-negocio/procesos/PR-01-movilizacion-institucional.md) — alerta bloqueante del cierre de bitácora; [`INV-29`, `T-18`, `T-19` y §10.2 estado operativo del vehículo](../../03-arquitectura/estados/orden-de-mision.md)
- **Reglas:** [RN-31](../../01-negocio/reglas/RN-31-odometro-de-retorno.md) (regla eje), [RN-04](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [RN-05](../../01-negocio/reglas/RN-05-registro-cerrado-no-se-edita.md), [RN-13](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), [RN-19](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md), [RN-28](../../01-negocio/reglas/RN-28-comprobacion-del-consumo-de-combustible.md), [RN-30](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [RN-37](../../01-negocio/reglas/RN-37-coherencia-de-la-secuencia-de-casetas.md), [RN-39](../../01-negocio/reglas/RN-39-parametros-normativos-con-vigencia.md), [RN-42](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [RN-43](../../01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), [RN-46](../../01-negocio/reglas/RN-46-fecha-del-hecho-y-fecha-de-captura.md), [RN-47](../../01-negocio/reglas/RN-47-digitacion-diferida-desde-papel.md), [RN-08](../../01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)
- **Reglas candidatas:** `RN-C22a`, `RN-C22b`, `RN-C22c`
- **Normas:** [NRM-09](../../01-negocio/normativa/NRM-09-realidad-operativa.md) `[V]` — *"detectar lecturas inconsistentes: retroceso de odómetro, saltos imposibles, y rendimientos anómalos en ambas direcciones"*; [NRM-01](../../01-negocio/normativa/NRM-01-control-interno-tsc.md) `[V]`; [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) — constatación física del bien
- **Actores:** ACT-04 (resuelve la inconsistencia), ACT-05 (verifica al retorno), ACT-06 (captura y fotografía), ACT-11 (respalda el reemplazo), ACT-12 (revisa), ACT-14 (valor del bien)
- **Casos relacionados:** [CE-21](CE-21-galonaje-que-no-cuadra-con-kilometraje.md) — el odómetro es su denominador; [CE-02](CE-02-averia-mecanica-en-ruta.md) — corte de odómetro por vehículo sustituto; [CE-25](CE-25-comprobante-perdido-o-estacion-sin-factura.md)
- **Insumos:** #1 (reglamento interno de uso de vehículos), #2 (formato de bitácora en papel — ahí está el campo de odómetro y lo que se hace hoy cuando no cuadra), #35 (escala de severidad de fallas: si el odómetro averiado es incapacitante), #19 (informes de auditoría). **A registrar `[C]`:** máximos de salto por tipo de vehículo y por unidad de tiempo; plazo máximo de odómetro averiado antes de bloquear programación
