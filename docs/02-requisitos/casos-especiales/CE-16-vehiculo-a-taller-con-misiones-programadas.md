# CE-16 — El vehículo entra a taller y ya tenía misiones programadas para esta semana

| Campo | Valor |
|---|---|
| **Módulos** | M-11 Mantenimiento, M-07 Programación y Despacho, M-03 Flota, M-09 Combustible, M-18 Peajes, M-13 Liquidación |
| **Estados afectados** | `APROBADA` (sin reserva), `PROGRAMADA` (`T-10`, `T-11`, `T-13`), `DESPACHADA` (`T-15`, `T-16`) · Vehículo: `W-07`, `W-09`, `W-12` |
| **Frecuencia** | Frecuente — es el evento que más reprogramaciones produce en una flota con años de uso |
| **Impacto** | Operativo, financiero y de auditoría |
| **Resolución** | Definida para la sustitución y el recálculo · `[C]` para la prelación entre misiones desplazadas y la escala de severidad de fallas |

## La situación

Martes 12 de mayo, predio de la sede en Tegucigalpa. El Encargado de Mantenimiento abre la orden de trabajo `OT-2026-0311` sobre el pickup doble cabina correlativo `INM-0042`: fuga en el cilindro maestro del clutch, detectada en la revisión de rutina del predio. El vehículo pasa a `EN_TALLER` por `W-09`. El mecánico dice "tres días". El repuesto viene de San Pedro Sula.

Ese pickup tiene comprometida la semana completa:

| Orden de Misión | Ventana | Estado | Fondo de combustible |
|---|---|---|---|
| `OM-2026-00517` | 13 de mayo · Tegucigalpa – Comayagua – Tegucigalpa | `PROGRAMADA` | Vales **emitidos**, no entregados |
| `OM-2026-00521` | 15 al 17 de mayo · Tegucigalpa – Choluteca, con entrega de mobiliario a la delegación | `PROGRAMADA` | Sin emitir |
| `OM-2026-00533` | 20 de mayo · Tegucigalpa – Danlí | `APROBADA` | Sin emitir |

La tercera no tiene reserva: aprobar no es programar, y `INV-11` lo dice expresamente. Pero el Jefe de Transporte ya la tenía apalabrada para ese pickup, en su cabeza. Cuando el pickup no vuelve, esa misión también se cae — solo que sin dejar rastro de que se cayó.

**Cuatro variantes, todas reales:**

1. **Correctivo súbito al retorno.** El motorista declara la novedad en el acta de recepción a las 5:40 de la tarde y el vehículo pasa `EN_MISION → EN_TALLER` por `W-07`. La misión de mañana sale a las 5:00 de la mañana y **el vale ya está entregado y firmado**.
2. **El taller devuelve tarde.** Tres días estimados, once reales, porque el repuesto no llegó. Las reservas de la segunda semana nunca se tocaron porque nadie las volvió a mirar.
3. **La falla se detecta con la orden ya `DESPACHADA`.** El mecánico ve el charco debajo del pickup a la hora de la salida, con el motorista adentro y el salvoconducto impreso en la guantera.
4. **Preventivo vencido pero el vehículo camina.** El mantenimiento de los 5,000 km se pasó hace 1,400 km. El vehículo funciona. Nadie sabe si eso bloquea la asignación o solo se anota.

## Qué se hace hoy sin sistema

`[C]` La práctica de la institución no está confirmada — insumo #2 (formatos vigentes) e insumo #35 (escala de severidad de fallas).

Lo que se observa como práctica común en instituciones públicas hondureñas `[I]`:

- La programación de la semana está en una **pizarra o un cuaderno** en la oficina de Transporte. El taller no la ve; la oficina de Transporte no ve la lista de vehículos en el taller.
- El mecánico **avisa de palabra** al Jefe de Transporte. Si el Jefe está en reunión o de comisión, no se entera nadie más.
- Se **tacha el número de vehículo en la orden de misión ya impresa**, se escribe el sustituto al margen y se pone la inicial de quien lo cambió. La orden dice un vehículo y sale otro.
- El **vale de combustible queda nominado al vehículo original y se consume en el sustituto**. Ahí se rompe la conciliación galonaje–kilometraje del período completo, y no de una misión: de los dos vehículos, para siempre.
- **Nadie recalcula el peaje.** Si el sustituto tiene más ejes que el pickup, el estimado con el que se autorizó dejó de significar algo.
- **Nadie reemite el salvoconducto.** El permiso de circulación en día inhábil se emitió para `INM-0042`; sale `INM-0068`. En un retén eso es un vehículo del Estado circulando en día inhábil sin permiso a su nombre.
- La misión desplazada se "acomoda" a favor de quien tiene más jerarquía.

**El tachado con iniciales al margen es la regla que nadie escribió.** La institución ya acepta que se sustituya el vehículo; lo que no tiene es constancia de quién lo autorizó, contra qué se revalidó y qué números cambiaron por ello.

## Por qué el flujo normal no lo cubre

[`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) resuelve limpiamente el **acto**: se evalúa en `T-08`, `T-10` y `T-12`, y mira el estado del vehículo en ese instante. Eso ya está, y funciona.

Lo que no está cubierto es el **efecto hacia atrás**:

- Ninguna regla se dispara cuando el estado del vehículo cambia **después** de que la reserva ya existe. `W-07`, `W-09` y `W-12` son transiciones del vehículo, y la sección 10.2 de la máquina de estados no dice nada sobre las Órdenes de Misión que quedan colgando. Lo más cercano — *"un vehículo con misiones abiertas no puede ser dado de baja"* — cubre `W-14` y `W-15`, no el taller.
- El resultado es un vehículo `EN_TALLER` **con reservas vivas**. `RN-19` las va a bloquear, sí: en `T-12`, la mañana de la salida, con la comisión esperando en el predio y el vale ya emitido. **El bloqueo llega correcto y tarde**, que es la peor combinación posible.
- La variante 3 es peor todavía: con la orden ya en `DESPACHADA`, `T-10` no está disponible. La corrección exige `T-15` o `T-16`, con devolución de fondo y acta — un procedimiento que nadie va a ejecutar a las 5 de la mañana si el sistema no lo pone delante.

## Regla de resolución

### 1. El ingreso a taller no se bloquea nunca

Es un hecho mecánico, y el principio **P-2** de la máquina de estados es explícito: no se bloquean los hechos consumados. El pickup tiene el clutch roto con reservas o sin ellas.

Lo que sí cambia es que `W-07`, `W-09` y `W-12` exigen dos datos que hoy no se piden:

- **Causa tipificada** — no texto libre. Sin tipificación no hay indicador de indisponibilidad de flota, y `NO_DISPONIBLE` se convierte en el cementerio donde se esconde la flota que nadie repara (§10.2).
- **Ventana de indisponibilidad estimada** — fecha estimada de alta. Sin ella el sistema no puede decir a quién afecta.

### 2. Mandar el vehículo a taller muestra su impacto antes de confirmarse

Al ejecutar la transición, **ACT-11 Encargado de Mantenimiento** ve la lista de Órdenes de Misión con reserva sobre ese vehículo dentro de la ventana estimada, con: folio, ventana, dependencia solicitante, motorista asignado, estado del fondo (sin emitir / emitido / entregado), estimado de peajes congelado, si hay salvoconducto emitido y si hay manifiesto de personas externas.

**ACT-11 no decide qué hacer con esas misiones** — no es su función, y decidirlo sería atravesar la segregación de [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md). Pero **no puede confirmar el ingreso a taller sin haber visto y acusado la lista**, y ese acuse queda registrado.

### 3. Cada reserva afectada se marca, no se rompe sola

La asignación pasa a **`RESERVA_EN_CONFLICTO`**. Es una marca sobre la asignación, **no un estado nuevo de la Orden de Misión**: la máquina de estados es la autoridad sobre estados y no se le agregan desde un caso especial. La misión sigue `PROGRAMADA` o `DESPACHADA`.

Efectos de la marca:

- **Impide `T-12`** sobre esa orden hasta que se resuelva. El despacho no se descubre imposible a las 5 de la mañana: está impedido desde el momento en que el vehículo entró al taller.
- **Notificación inmediata** a ACT-04 Jefe de Transporte, a ACT-10 Encargado de Delegación si la misión es de delegación, y a ACT-02 solicitante. El solicitante se entera del problema, no del fracaso.
- **Obliga a un desenlace explícito antes del inicio de la ventana.** La reserva en conflicto no expira en silencio ni se resuelve por el paso del tiempo.

### 4. Desenlace según el estado de la misión y del fondo

| Estado de la misión | Fondo | Camino |
|---|---|---|
| `APROBADA` | Cualquiera | **No hay reserva que romper** (`INV-11`). Solo se pierde la preferencia informal. Aun así, la cola de programación debe mostrar que el vehículo previsto no está disponible en esa ventana |
| `PROGRAMADA` | Sin emitir | **`T-10` sustitución de vehículo**, revalidando íntegramente `BD-02`, `BD-03`, `BD-07` y `BD-11` contra el sustituto — [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md). Si no hay vehículo compatible disponible: **`T-11`** devuelve la orden a `APROBADA` conservando su aprobación original, y la solicitud vuelve a la cola de programación |
| `PROGRAMADA` | Emitido, no entregado | Igual que arriba, **más** la anulación de la emisión. El vale nominado a la misión y al vehículo **se anula, no se reutiliza** (§10.1). La re-emisión contra el vehículo sustituto es un acto nuevo con folio nuevo, y la anulación es asiento reverso — [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md) |
| `PROGRAMADA` sin salida viable | Ninguno o devuelto | **`T-13`** anulación con motivo tipificado *"sin flota disponible"*. La tipificación importa: es el indicador de déficit de flota, y un motivo de texto libre aquí no produce ningún indicador |
| `DESPACHADA` | Entregado, **sin** consumo | **`T-15`**, con devolución íntegra y acta firmada por ACT-04, ACT-07 y ACT-13. Luego, misión nueva con folio nuevo — ver [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md) |
| `DESPACHADA` | Entregado, **con** consumo parcial | `T-15` **no está disponible**. El camino es **`T-16`** `DESPACHADA → RETORNADA`, misión marcada como no ejecutada, y liquidación por lo efectivamente consumido — [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md) |
| `EN_RUTA` | Cualquiera | **Fuera de este caso.** El vehículo ya salió y se averió en carretera: es [CE-02](CE-02-averia-mecanica-en-ruta.md) |

### 5. Sustituir el vehículo no es cambiar un nombre

Todo lo que se derivó del vehículo se recalcula, y el recálculo se vuelve a congelar:

| Qué se recalcula | Regla | Por qué |
|---|---|---|
| Habilitación del motorista | [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md) | El motorista habilitado para un pickup puede no estarlo para un camión. `BD-02` completo, no parcial |
| Compatibilidad y capacidad | [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md) | El mobiliario que cabía en la paila del pickup puede no caber en el sustituto, y al revés |
| Documentación del vehículo | [`RN-16`](../../01-negocio/reglas/RN-16-seguro-y-revision-mecanica.md), `BD-03` | Matrícula bloqueante; póliza y revisión según el parámetro de la institución |
| Categoría y estimado de peajes | [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md) | La categoría es del vehículo. Cambiar el vehículo cambia lo que se paga en cada punto de la ruta |
| Rendimiento esperado y galonaje | [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md) | El rendimiento es por vehículo. Conciliar el consumo del sustituto contra el rendimiento del original produce una desviación falsa |
| Custodia | [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md) | El traslado temporal de custodia se registra sobre el vehículo que efectivamente sale |
| Salvoconducto | [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), `PC-03` | El permiso es para **ese vehículo, ese motorista y esa ventana**. Se anula el folio anterior y se emite uno nuevo. En carretera el control es físico y el documento no coincide solo |

Los valores congelados al autorizar **no se sobrescriben**: el nuevo cálculo se congela por [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) y la diferencia queda como asiento por [`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md). La asignación original se conserva junto a la sustituta — [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md).

### 6. El taller devuelve tarde

La ventana estimada es una estimación, no un compromiso. Vencida sin alta:

- **Alerta** a ACT-04 y ACT-11 con la lista de reservas todavía en conflicto y las que entran en conflicto por la extensión.
- El vehículo **no vuelve a `DISPONIBLE` porque venció la fecha**. Solo vuelve por `W-10`, acto explícito de ACT-11 con la orden de trabajo cerrada — [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md). Una fecha estimada no repara un clutch.
- Se registra el **desfase entre indisponibilidad estimada y real** en el expediente del vehículo. Es el insumo del indicador de indisponibilidad de flota, que hoy no existe en ninguna parte y que es lo que permite sostener ante Gerencia Administrativa por qué hace falta un vehículo más.

### 7. Ver el choque antes de que ocurra

El mantenimiento preventivo tiene fecha o kilometraje objetivo conocido. El sistema debe poder listar, para un horizonte configurable, **las Órdenes de Misión programadas sobre vehículos cuyo preventivo vence dentro de la ventana de la misión**. Es la misma consulta que [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) hace con documentos, aplicada al mantenimiento. Hoy nadie en la institución puede armarla.

### Lo que hay que confirmar

- `[C]` **¿El mantenimiento preventivo vencido bloquea la asignación o solo advierte?** `BD-07` lo deja abierto expresamente — insumo #59, y el paquete de parámetros operativos es el #32. Opciones: (a) advertencia con acuse, costo cero, riesgo de que el preventivo nunca se haga; (b) bloqueo configurable con umbral en kilómetros o días, costo de inmovilizar flota en instituciones que no dan abasto con el taller. Recomendación del análisis: advertencia con acuse registrado más reporte de reincidencia por vehículo, y bloqueo activable por la institución.
- `[C]` **Escala de severidad de fallas** — insumo #35. Sin ella el sistema no distingue *"entra a taller el lunes"* de *"no se mueve de aquí"*, y ambas cosas hoy son el mismo `EN_TALLER`.
- `[C]` **Criterio de prelación** cuando hay un solo vehículo sustituto y dos misiones en conflicto — insumo #31. Es el mismo hueco de [CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) y se resuelve con la misma regla.
- `[C]` **¿La institución recurre a vehículo alquilado o en comodato** cuando la flota propia no alcanza? [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) deja abierto qué régimen les aplica — rotulación y prohibición de día inhábil incluidas.

### Reglas candidatas — no dar por escritas

| Candidata | Enunciado propuesto | Por qué falta |
|---|---|---|
| `RN-C16a` | *Toda transición del vehículo a un estado no habilitante de asignación exige causa tipificada y ventana de indisponibilidad estimada, y no se confirma sin que su ejecutor haya visto y acusado la lista de reservas afectadas.* | [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md) gobierna el acto de asignar. **Nada gobierna el efecto del cambio de estado sobre reservas ya constituidas** |
| `RN-C16b` | *La reserva afectada por indisponibilidad sobrevenida del vehículo se marca en conflicto, impide el despacho y obliga a un desenlace explícito registrado antes del inicio de la ventana. No expira en silencio.* | Sin esto el conflicto se descubre en `T-12`, la mañana de la salida, y se resuelve tachando la orden impresa |
| `RN-C16c` | *La sustitución de vehículo recalcula y vuelve a congelar todo valor derivado del vehículo — categoría y estimado de peaje, rendimiento esperado, habilitación, capacidad, custodia y salvoconducto — dejando asiento de diferencia contra el congelamiento anterior.* | [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md) exige revalidar habilitaciones, pero **no dice nada de los valores económicos congelados** por [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md) |
| `RN-C16d` | *El sistema reporta, para un horizonte configurable, las Órdenes de Misión programadas sobre vehículos con mantenimiento preventivo por vencer dentro de la ventana de la misión.* | [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md) cubre documentos con fecha de vencimiento, no umbrales de kilometraje |

Los identificadores `RN-C16x` son **candidatos**, con la convención de [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md): no ocupan número definitivo hasta que el PO los apruebe.

## Evidencia que debe quedar

Ante una auditoría, la institución debe poder mostrar, encadenado al expediente del vehículo y a cada Orden de Misión afectada:

1. La **orden de trabajo** con folio, causa tipificada, fecha y hora de ingreso, odómetro de ingreso y ventana de indisponibilidad estimada.
2. El **acuse de ACT-11** sobre la lista de reservas afectadas, con la lista exactamente como se le mostró y su marca de tiempo. Es la defensa de quien mandó el vehículo al taller.
3. Por cada misión afectada, la **transición ejecutada** — `T-10`, `T-11`, `T-13`, `T-15` o `T-16` — con actor, rol ejercido, motivo tipificado y marca de tiempo.
4. La **asignación original conservada junto a la sustituta**, nunca sobrescrita. Un registro que solo muestra el vehículo que salió hace imposible explicar por qué se programó otro.
5. Los **folios de vale anulados uno por uno**, con la demostración de que ninguno se reutilizó, y los folios re-emitidos contra el vehículo sustituto.
6. El **salvoconducto anterior anulado y el nuevo emitido**, si alguna parte de la ventana caía en día u hora inhábil.
7. El **recálculo del estimado de peajes** con la tabla vigente a la fecha del hecho, su nuevo congelamiento y el asiento de diferencia contra el anterior.
8. La **fecha real de alta del taller** (`W-10`) con la orden de trabajo cerrada y el odómetro de salida, contrastada contra la ventana estimada.
9. Para el período: el **reporte de indisponibilidad de flota** — cuántas misiones se desplazaron por taller, cuántas se anularon por falta de flota y qué vehículos las causaron.

## Trazabilidad

- **Autoridad de estados y transiciones:** [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md) — §10.2 estado operativo del vehículo (`W-07`, `W-09`, `W-10`, `W-12`), `T-10`, `T-11`, `T-13`, `T-15`, `T-16`, `INV-11`, principio `P-2`, §10.1 vales
- **Reglas:** [`RN-19`](../../01-negocio/reglas/RN-19-vehiculo-no-operativo-no-se-asigna.md), [`RN-14`](../../01-negocio/reglas/RN-14-sustitucion-de-motorista.md), [`RN-13`](../../01-negocio/reglas/RN-13-sin-doble-asignacion.md), [`RN-09`](../../01-negocio/reglas/RN-09-matriz-licencia-vehiculo.md), [`RN-10`](../../01-negocio/reglas/RN-10-licencia-vigente-en-todo-el-rango.md), [`RN-20`](../../01-negocio/reglas/RN-20-compatibilidad-vehiculo-objeto-del-traslado.md), [`RN-21`](../../01-negocio/reglas/RN-21-capacidad-de-pasajeros-y-carga.md), [`RN-22`](../../01-negocio/reglas/RN-22-custodia-del-vehiculo.md), [`RN-23`](../../01-negocio/reglas/RN-23-permiso-de-circulacion-en-dia-inhabil.md), [`RN-25`](../../01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md), [`RN-27`](../../01-negocio/reglas/RN-27-asignacion-de-combustible-con-folio.md), [`RN-04`](../../01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md), [`RN-30`](../../01-negocio/reglas/RN-30-conciliacion-galonaje-kilometraje.md), [`RN-33`](../../01-negocio/reglas/RN-33-categoria-de-peaje-derivada-de-ficha-tecnica.md), [`RN-34`](../../01-negocio/reglas/RN-34-tarifa-de-peaje-por-punto-categoria-vigencia.md), [`RN-35`](../../01-negocio/reglas/RN-35-estimacion-de-peajes-antes-de-aprobar.md), [`RN-41`](../../01-negocio/reglas/RN-41-congelamiento-del-valor-al-autorizar.md), [`RN-42`](../../01-negocio/reglas/RN-42-correccion-retroactiva-con-asiento-de-diferencia.md), [`RN-17`](../../01-negocio/reglas/RN-17-alertas-de-vencimiento-documental.md), [`RN-01`](../../01-negocio/reglas/RN-01-segregacion-de-funciones.md)
- **Reglas candidatas:** `RN-C16a`, `RN-C16b`, `RN-C16c`, `RN-C16d`
- **Bloqueos duros:** `BD-02`, `BD-03`, `BD-07`, `BD-11`
- **Puntos de control:** `PC-03`, `PC-05`, `PC-06`, `PC-07` de [PR-01](../../01-negocio/procesos/PR-01-movilizacion-institucional.md)
- **Normas:** [NRM-02](../../01-negocio/normativa/NRM-02-bienes-del-estado.md) `[P]` custodia y ciclo del bien · [NRM-06](../../01-negocio/normativa/NRM-06-transito-y-licencias.md) `[V]` documentación del vehículo
- **Decisiones:** [DP-001 D-08](../../07-gestion/decisiones-de-producto/DP-001-fronteras-con-sistemas-existentes.md) — el estado del vehículo lo registran también los motoristas desde el campo
- **Actores:** ACT-02, ACT-04, ACT-05, ACT-07, ACT-10, ACT-11, ACT-13
- **Casos relacionados:** [CE-02](CE-02-averia-mecanica-en-ruta.md) (avería en ruta), [CE-12](CE-12-dos-solicitudes-compiten-por-el-mismo-vehiculo.md) (prelación), [CE-13](CE-13-motorista-no-disponible-por-talento-humano.md) (mismo patrón del lado del motorista), [CE-20](CE-20-mision-cancelada-con-combustible-ya-entregado.md) (fondo ya entregado)
- **Insumos:** #2 (formatos en papel), #31 (criterio de prelación), #32 (paquete de parámetros operativos), #35 (escala de severidad de fallas), #59 (¿el preventivo vencido bloquea o advierte? y ventana de indisponibilidad estimada)

> **Hallazgo menor de trazabilidad.** `BD-07` y `BD-04` de [orden-de-mision.md](../../03-arquitectura/estados/orden-de-mision.md), y [CE-11](CE-11-licencia-vence-durante-la-mision.md), citan **insumo #1** para umbrales, horario hábil y holgura de retorno. En [insumos-pendientes.md](../../07-gestion/insumos-pendientes.md), el **#1** es el *reglamento interno de uso de vehículos* y el paquete de parámetros operativos es el **#32**. Este caso usa la numeración vigente del registro de insumos. Corresponde corregir las citas en los artefactos anteriores; no se corrigen aquí porque la máquina de estados es autoridad y la corrección debe hacerse en ella.
