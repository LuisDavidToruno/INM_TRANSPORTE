# CE-02 — A la altura de Catacamas el pickup pierde los frenos y la misión no puede seguir

| Campo | Valor |
|---|---|
| **Módulos** | M-08 Ejecución y Bitácora, M-11 Mantenimiento y Taller, M-12 Incidentes, M-09 Combustible, M-18 Peajes, M-13 Liquidación, M-16 Operación Desconectada |
| **Estados afectados** | `EN_RUTA` — y el vehículo en `EN_MISION` |
| **Frecuencia** | Ocasional — flota con años de uso y carreteras en mal estado |
| **Impacto** | Operativo, financiero y de auditoría |
| **Resolución** | Definida. Requiere ampliar la máquina de estados: ver "Hallazgo" |

## La situación

Una comisión sale de Tegucigalpa hacia Puerto Lempira con equipo de cómputo para una delegación. Salió el martes a las 05:20 con odómetro 148,340 y fondo para siete días. A la altura de Catacamas, sobre el km 210, el pickup pierde el sistema de frenos. El motorista se orilla, no hay señal de datos estable, y la carga —tres computadoras y un UPS— sigue a bordo, a media carretera, sin custodia distinta a la del propio motorista.

Ya se pasó por dos casetas y se cargó combustible una vez en Juticalpa. El odómetro marca 148,551.

Desde ahí puede pasar cualquiera de tres cosas, y cada una deja el expediente en un sitio distinto:

- Llega una grúa, el vehículo se remolca a un taller de Juticalpa y **la delegación de Catacamas presta otro pickup** para que la misión continúe.
- No hay vehículo sustituto y **la misión se aborta**: la carga se resguarda y todos vuelven.
- No se resuelve ese día. El vehículo queda en el patio de un taller, el motorista pernocta, y **la decisión se toma mañana**.

Mientras tanto: se entregó fondo por siete días, se pagaron dos peajes, y el kilometraje de retorno no va a corresponder con nada.

## Qué se hace hoy sin sistema

El motorista llama cuando consigue señal. El Jefe de Transporte anota en un cuaderno. La bitácora se completa a mano al regresar, con el kilometraje del punto de la avería, y se adjunta el reporte del taller si el taller lo emitió. El fondo se liquida "como se pueda" y los vales sobrantes se devuelven — a veces con acta, a veces no.

**El "a veces no" es exactamente el hallazgo de auditoría.** Y hay otro peor, que nadie escribe: cuando se presta el vehículo de la delegación de Catacamas, ese vehículo **no está en ninguna orden de misión**. Circula, gasta combustible y paga peajes bajo un expediente que nombra a otro vehículo. Si lo detiene una comisión de fiscalización, el papel que lleva el motorista no corresponde al vehículo que conduce.

## Por qué el flujo normal no lo cubre

El flujo feliz asume que la misión termina donde estaba planeada, con el vehículo que salió y el motorista que salió. Aquí se rompen las tres cosas a la vez.

Además, la máquina de estados **no tiene salida para esto**: desde `EN_RUTA` solo existen `T-17` (prórroga o relevo) y `T-18` (retorno), y `EN_RUTA → ANULADA` está prohibida con razón — el vehículo salió y hubo consumo real de recursos públicos. Y el registro tiene que hacerse **sin conectividad**, desde la carretera, por alguien que en ese momento está resolviendo un problema mecánico y no quiere pelear con una aplicación.

## Regla de resolución

**1. Un evento de bitácora, no un estado nuevo.** El motorista registra el evento tipificado **`INTERRUPCION_EN_RUTA`** desde el cliente de campo, **sin conexión** (`RN-43`), con la mínima fricción posible: hora del hecho, ubicación descrita, odómetro, causa tipificada, y fotografías. La Orden de Misión **sigue en `EN_RUTA`** y recibe la **marca de situación "interrumpida"**, con la lista de pendientes visible. Es el mismo mecanismo que la máquina de estados ya usa para "anulación en trámite" en `T-15`: una marca sobre el expediente, no un estado inventado.

**2. Registrar la interrupción congela la ejecución y abre los desenlaces que este caso admite.**

> **Corrección — hallazgo `HB3-13`. «Pendiente de resolución» no es un desenlace: es su ausencia.**
>
> Esta sección listaba tres desenlaces y el tercero era *«queda pendiente de resolución»*. [`RN-70`](../../01-negocio/reglas/RN-70-interrupcion-en-ruta-con-desenlace-obligatorio.md) lista cuatro y **ninguno es ése** — porque esa misma regla exige *«desenlace explícito, tipificado y registrado»* y remata que ninguna misión con marca de interrupción **sin desenlace** puede quedar viva al cierre del período.
>
> **La consecuencia era medible y mala.** Un tablero que cuenta *«interrupciones con desenlace»* incluiría las que no lo tienen, y el Jefe de Transporte vería resueltas misiones que siguen abiertas. El indicador que existe para vigilar la cola la estaría escondiendo.
>
> **Este caso es un subconjunto de `RN-70`, y ahora lo dice.** No todos los desenlaces del catálogo aplican a una avería: *«continuar con el mismo vehículo y conductor»* está descartado por definición — el vehículo perdió los frenos.

| Desenlace | Qué hace el sistema |
|---|---|
| **Continúa con vehículo sustituto** | Se abre un **tramo nuevo** bajo la misma Orden de Misión, con el vehículo y el motorista sustitutos, previa revalidación completa de `BD-02`, `BD-03` y `BD-12` |
| **Se aborta la misión y se retorna** | `T-18` con subtipo **retorno anticipado**. Ver `CE-07`: la liquidación es por lo efectivamente ejecutado |
| **Retorno sin el vehículo**, resguardado o retenido en sitio | Es el caso probable de Catacamas: la unidad no puede moverse. [`RN-75`](../../01-negocio/reglas/RN-75-bien-retenido-o-sustraido-no-sale-del-registro.md) — **el bien no sale del registro** por estar lejos |
| ~~Queda pendiente de resolución~~ | ⛔ **No es un desenlace.** Es el **estado mientras no hay ninguno**: la marca «interrumpida» permanece, con responsable y fecha límite, y **no se puede cerrar el ejercicio con misiones así** ([`RN-97`](../../01-negocio/reglas/RN-97-saldo-de-apertura-de-control-interno.md)). Cuenta como **pendiente**, nunca como resuelta |
**3. En los tres desenlaces, y también mientras queda pendiente, el vehículo sale de circulación.** El evento genera automáticamente la orden de trabajo correctiva en M-11 y lleva el vehículo a `EN_TALLER` o `NO_DISPONIBLE` con causa tipificada (`W-07`, `W-08` de la máquina de estado operativo; `RN-19`). Un vehículo averiado no puede aparecer como asignable para la misión de mañana.

**4. La custodia de la carga es un hecho registrable.** Si la carga se transborda al vehículo sustituto, se registra **acta de transbordo** con inventario, hora y firma de quien entrega y quien recibe. Si se resguarda en un tercer lugar, se registra dónde y bajo responsabilidad de quién. La cadena de custodia no se interrumpe porque el vehículo se haya averiado (`RN-22` para el vehículo; para la carga ver la regla candidata).

**5. El fondo no se recalcula, se imputa.** Lo entregado ya está entregado. Lo que cambia es a qué tramo se imputa cada consumo, y eso lo determina el odómetro y el vehículo de cada evento (`RN-28`, `RN-29`, `RN-30`). La conciliación galonaje–kilometraje se hace **por vehículo**, no por misión: promediar dos vehículos distintos produce un rendimiento que no existe.

**6. Nada de esto exige señal.** Todos los registros del punto 1 al 4 se capturan en el dispositivo y se sincronizan después (`RN-43`, `RN-44`, `RN-45`). La decisión del desenlace sí necesita a `ACT-04`; si no hay forma de contactarlo, el motorista registra el hecho y la decisión que tomó, y la falta de autorización previa se resuelve en la liquidación —mismo tratamiento que `T-17` da a la prórroga sin código.

### Hallazgo — la máquina de estados no cubre la sustitución de vehículo en ruta

`RN-14` dice que la sustitución **de motorista o de vehículo** revalida las habilitaciones y conserva la asignación original, y le atribuye los módulos M-07 **y M-08**. Pero `T-17`, la única autotransición de `EN_RUTA`, cubre solo prórroga, destino adicional y **relevo de motorista**. No hay transición que permita cambiar el vehículo con la misión en curso.

No se resuelve en este documento: [la máquina de estados es la autoridad en transiciones](../../../CLAUDE.md). Se reporta como **ampliación necesaria de `T-17`** —o transición nueva— dirigida a `docs/03-arquitectura/estados/orden-de-mision.md`, con el mismo tratamiento de relevo: revalidación de bloqueos duros contra el paquete congelado, acta de traspaso de custodia con odómetro, y conservación de la asignación original.

### Reglas candidatas

| Candidata | Enunciado |
|---|---|
| `RN-c:interrupcion-en-ruta` | La interrupción en ruta se registra como evento sin conectividad, marca la misión como interrumpida sin cambiarle el estado, y habilita exactamente tres desenlaces tipificados |
| `RN-c:imputacion-de-consumo-por-tramo` | Cuando una misión se ejecuta con más de un vehículo, combustible, peajes y kilometraje se imputan por tramo y por vehículo; el rendimiento nunca se calcula sobre la misión completa |
| `RN-c:acta-de-transbordo-de-carga` | Todo traslado de la carga entre vehículos durante la misión exige acta con inventario, hora, y firma de quien entrega y quien recibe |
| `RN-c:mision-interrumpida-no-cierra-ejercicio` | Ninguna misión con marca de interrupción sin desenlace puede quedar viva al cierre del período; el sistema las lista con responsable y plazo |

## Evidencia que debe quedar

Ante una auditoría, encadenado a la misma Orden de Misión:

1. El evento de interrupción con hora del hecho, ubicación, odómetro, causa tipificada y fotografías
2. El acta de devolución del fondo no consumido, o la justificación de su consumo (`RN-29`)
3. La orden de trabajo correctiva y su resultado, con el vehículo fuera de disponibilidad desde la hora del evento
4. La bitácora de **cada vehículo** involucrado, cerrada con su propio odómetro
5. El acta de transbordo o de resguardo de la carga
6. La revalidación de licencia y documentación del vehículo sustituto, con los datos concretos contra los que se evaluó (`§9.2` de la máquina de estados)
7. Quién autorizó el desenlace, cuándo, y si fue con código fuera de línea
8. Los peajes pagados por cada vehículo, con su categoría y su tarifa congelada

## Trazabilidad

- **Reglas**: `RN-04` anulación como asiento reverso · `RN-14` sustitución de motorista o vehículo · `RN-19` vehículo no operativo no se asigna · `RN-22` custodia del vehículo · `RN-28`, `RN-29`, `RN-30` combustible · `RN-31` odómetro de retorno · `RN-33`, `RN-34`, `RN-41` peajes y congelamiento · `RN-43`, `RN-44`, `RN-45` operación desconectada · `RN-46` fecha del hecho
- **Reglas candidatas**: `RN-c:interrupcion-en-ruta`, `RN-c:imputacion-de-consumo-por-tramo`, `RN-c:acta-de-transbordo-de-carga`, `RN-c:mision-interrumpida-no-cierra-ejercicio`
- **Transiciones**: `T-17` (ampliación pendiente para vehículo sustituto) · `T-18` subtipo retorno anticipado · `W-07`, `W-08` del estado operativo del vehículo
- **Prohibida**: `EN_RUTA → ANULADA` — el vehículo salió
- **Puntos de control**: `PC-04`, `PC-05`, `PC-11`
- **Proceso**: `PR-01` etapas E9 y E11; deriva a `PR-05` mantenimiento
- **Actores**: `ACT-06` registra · `ACT-04` decide el desenlace · `ACT-11` abre la orden de trabajo · `ACT-10` apoya desde la delegación más cercana
- **Casos especiales relacionados**: `CE-05` cambio de motorista en curso · `CE-06` extensión de la misión · `CE-07` retorno anticipado · `CE-10` motorista incapacitado en ruta
- **Historias candidatas**: `HU-c:registrar-interrupcion-en-ruta-sin-senal`, `HU-c:sustituir-vehiculo-en-mision-activa`, `HU-c:liquidar-mision-ejecutada-por-tramos`
