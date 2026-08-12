# CE-01 — El vehículo salió a las tres de la mañana y la autorización se firmó el lunes

| Campo | Valor |
|---|---|
| **Módulos** | M-06 Solicitudes, M-07 Programación y Despacho, M-08 Ejecución y Bitácora, M-09 Combustible, M-14 Auditoría, M-16 Operación Desconectada |
| **Estados afectados** | `BORRADOR`, `SOLICITADA`, `APROBADA`, `PROGRAMADA`, `DESPACHADA`, `EN_RUTA`, y bloqueo de `CERRADA` |
| **Frecuencia** | Frecuente — es la variante `V-05` del proceso `PR-01`, la que más se usa y peor se documenta |
| **Impacto** | Operativo, financiero, legal y de auditoría |
| **Resolución** | Definida, con dos puntos `[C]`: plazo máximo de convalidación y puesto convalidante |

## La situación

Domingo, 03:15. La Delegación de Danlí recibe por teléfono la instrucción de movilizar de inmediato a cuatro servidores y dos equipos al punto fronterizo de Las Manos, a 42 km. La jefatura inmediata del solicitante duerme en Tegucigalpa y no contesta. El Encargado de Combustible no está en la delegación y la caja con los vales está bajo llave en su escritorio. El Encargado de Despacho tampoco: el fin de semana la delegación la sostienen dos personas.

El Encargado de Delegación abre el portón, entrega las llaves del pickup, anota en un cuaderno "salida 03:40, unidad 14, motorista Núñez, comisión Las Manos", y el vehículo sale. En el trayecto el motorista carga L 900 de diésel en una estación de Danlí y paga de su bolsillo, porque no hay vale.

El lunes a las 8:00 alguien tiene que convertir eso en un expediente. La salida ya ocurrió, el combustible ya se consumió, y la autorización todavía no existe.

## Qué se hace hoy sin sistema

Se llena la requisición de vehículo con fecha del sábado y se le pide la firma a la jefatura el lunes, como si se hubiera firmado antes. El formato en papel no tiene casilla para "esto se autorizó después", así que la práctica es **antedatar**: la solicitud queda con una fecha que no corresponde al hecho.

Ese es el problema real, y no es de forma: un expediente antedatado que después se examina —porque el vehículo tuvo un accidente esa madrugada, por ejemplo— convierte una emergencia legítima en un documento falso. La institución pierde una defensa que tenía.

El reembolso del combustible pagado por el motorista se resuelve "cuando se pueda", a veces contra el ticket, a veces sin nada. `[C]` Insumo #37: si la institución admite y reembolsa consumo pagado por el motorista, y contra qué documento.

## Por qué el flujo normal no lo cubre

El flujo normal es `solicitar → autorizar → programar → despachar → salir`, y cada paso exige el anterior. Aquí el orden físico fue el inverso: primero salió el vehículo. Además:

- La segregación de funciones exige cinco personas distintas y a las 03:15 hay dos ([`actores-y-roles.md §5.2`](../../01-negocio/actores-y-roles.md), pares `I-01` a `I-17`).
- `BD-04` exige permiso de circulación en día inhábil firmado por `ACT-09` **antes** de despachar. Un domingo a las tres de la mañana no se firma nada.
- No hubo asignación de fondo, y `RN-32` dice que no se entrega combustible sin Orden de Misión aprobada. Aquí no hubo entrega: hubo gasto personal.
- El registro se hizo sin conectividad y con la única herramienta disponible: un cuaderno.

## Regla de resolución

**1. El expediente se crea con marca `EMERGENCIA` y causal tipificada.** Se registra offline (`RN-43`), con motivo obligatorio del catálogo configurable y fecha de salida en el pasado. El sistema **no lo bloquea**. Bloquear aquí solo produce expedientes antedatados fuera del sistema, que es exactamente lo que se quiere eliminar.

**2. Cada transición lleva su `ocurrido_en` real, no la hora de captura.** `T-08`, `T-12` y `T-14` se registran con la hora material en que se entregaron las llaves y salió el vehículo, y con modo de captura `digitación diferida` (`RN-46`, `RN-47`). La diferencia entre `ocurrido_en` y `capturado_en` queda visible en el expediente; no se disimula.

**3. La autorización se registra como convalidación, nunca como autorización previa.** Es la única transición del sistema cuyo `ocurrido_en` es **posterior** al de la transición siguiente en la máquina. El expediente debe mostrarlo así, con la leyenda explícita "convalidación posterior a la ejecución", y el documento impreso debe llevarla también. El orden de aplicación en el servidor sigue siendo el de la máquina de estados; lo que se invierte es la cronología real, y esa inversión es un dato, no un error.

**4. El núcleo irreductible de segregación no se levanta.** El Encargado de Delegación puede ejercer materialmente las funciones de sede, pero **no puede convalidar su propio acto** ni ser el motorista de la misión que despachó (`RN-01`, pares `I-07`, `I-10`, `I-11`). Convalida un puesto de sede central designado. Sin red se usa el código de autorización fuera de línea (`§6.6` de la máquina de estados).

**5. La misión no cierra sin convalidación.** `PC-18`: la falta de convalidación bloquea `T-21`, no bloquea el acto. Vencido el plazo, la misión cierra por `T-22` como `CERRADA_CON_HALLAZGO`. Nunca se cierra en silencio.

**6. El permiso de circulación en día inhábil también se convalida.** Se registra el hecho de haber circulado en franja inhábil sin salvoconducto y se resuelve en la liquidación con hallazgo `H-05` si no hay convalidación de `ACT-09` (`RN-23`, `RN-25`).

**7. El combustible pagado por el motorista es un circuito aparte.** No se registra como consumo de un fondo que nunca se entregó. Se registra como **gasto pendiente de reembolso**, con ticket, estación, galones y odómetro (`RN-28`), y queda fuera de la conciliación del fondo (`RN-29`) hasta que exista el acto de reembolso.

**8. La frecuencia de la emergencia es un indicador, no una nota al pie.** El sistema mide salidas en régimen de emergencia por dependencia, por delegación y por mes, y las expone en el reporte de control interno de `ACT-08` y `ACT-12`. Si esta variante se vuelve la vía normal para saltarse a `ACT-03`, el control desapareció y hay que poder verlo en un número.

### Reglas candidatas

Ninguna de las 54 reglas vigentes gobierna la convalidación. Se proponen:

| Candidata | Enunciado |
|---|---|
| `RN-c:convalidacion-con-plazo-maximo` | Toda salida en régimen de emergencia se convalida dentro de un plazo configurable; vencido el plazo la convalidación **no se rechaza**: se registra y la misión cierra con hallazgo. Ya propuesta en `PR-01 §9` |
| `RN-c:cronologia-invertida-declarada` | Cuando la marca de tiempo del hecho de una transición es posterior a la de la transición que le sigue en la máquina, el expediente lo declara explícitamente y lo imprime. Ningún acto se presenta como previo si fue posterior |
| `RN-c:gasto-de-bolsillo-fuera-del-fondo` | El consumo pagado por el motorista se registra como gasto pendiente de reembolso, con comprobante, y no entra en la conciliación del fondo hasta que se reembolsa `[C]` insumo #37 |

## Escalamiento al PO

`[C]` **Qué puesto convalida y en qué plazo máximo** — insumos #32 y pendiente D de `PR-01`. Opciones vistas y su costo:

| Opción | Costo |
|---|---|
| Convalida la jefatura inmediata del solicitante | Es la más natural, pero un domingo puede estar tan incomunicada como al momento del hecho, y el plazo se consume |
| Convalida un puesto de turno de sede central designado | Resuelve la disponibilidad, pero exige que la institución tenga ese turno definido y que ARGOS lo refleje |
| Convalida `ACT-08` Gerencia Administrativa siempre | Concentra la carga en un puesto y lo vuelve cuello de botella, pero es inequívoco |

`[C]` **Qué ocurre si la única persona disponible a las 03:15 es el propio motorista.** El par `I-11` no se levanta ni en emergencia, de modo que hoy ese caso no tiene salida escrita. Es un hueco real, no un detalle.

## Evidencia que debe quedar

Ante el TSC, encadenado a la misma Orden de Misión:

1. La causal de emergencia tipificada y quién la declaró, con hora del hecho y hora de captura visiblemente distintas
2. Quién ordenó verbalmente la movilización y por qué canal, declarado por quien recibió la orden
3. El acto de convalidación: quién, cuándo, con qué código si fue fuera de línea, y sobre qué contenido (`RN-03`)
4. El intervalo entre la salida y la convalidación, medido, y si respetó el plazo
5. La convalidación del permiso de circulación en día inhábil, o el hallazgo `H-05` si no la hubo
6. Los tickets del combustible pagado por el motorista y el acto de reembolso, o su ausencia declarada
7. El reporte de frecuencia de emergencias de esa delegación en el período

## Trazabilidad

- **Reglas**: `RN-01` segregación · `RN-03` registro inmutable de autorización · `RN-06` transiciones · `RN-08` cadena para cierre · `RN-23` permiso en día inhábil · `RN-25` salvoconducto con folio · `RN-28` comprobación del consumo · `RN-32` combustible contra orden aprobada · `RN-43` captura sin conectividad · `RN-46` fecha del hecho y de captura · `RN-47` digitación diferida
- **Reglas candidatas**: `RN-c:convalidacion-con-plazo-maximo`, `RN-c:cronologia-invertida-declarada`, `RN-c:gasto-de-bolsillo-fuera-del-fondo`
- **Transiciones**: `T-02` con marca `EMERGENCIA` · `T-05` como convalidación · `T-08`, `T-12`, `T-14` con `ocurrido_en` retroactivo · `T-21` bloqueada · `T-22` si vence el plazo
- **Bloqueos y criterios**: `BD-01`, `BD-04`, `BD-06`, `H-05`, `H-07`
- **Puntos de control**: `PC-03`, `PC-16`, `PC-18`
- **Proceso**: `PR-01` variante `V-05`
- **Actores**: `ACT-10` ejecuta · `ACT-03` o puesto de sede convalida `[C]` · `ACT-08` y `ACT-12` reciben notificación · `ACT-09` convalida el permiso de circulación
- **Casos especiales relacionados**: `CE-06` extensión de la misión · `CE-09` bitácora en papel
- **Historias candidatas**: `HU-c:registrar-solicitud-en-regimen-de-emergencia`, `HU-c:convalidar-salida-de-emergencia`, `HU-c:reportar-frecuencia-de-emergencias-por-dependencia`
