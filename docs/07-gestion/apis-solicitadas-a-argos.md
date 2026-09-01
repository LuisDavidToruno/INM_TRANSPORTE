# APIs que SIGTI necesita de `ARGOS_API`

| Campo | Valor |
|---|---|
| **Fecha** | 2026-08-31 |
| **Pide** | SIGTI |
| **Provee** | `ARGOS_API` |
| **Respondido** | 2026-09-01 |
| **Estado** | **A-1, A-2, B-1 y C-1 entregados. B-2 bloqueado — el dato no existe.** |

Todo lo de abajo se verificó **contra `SICOVBD` en vivo**, no contra suposiciones. Cada petición
dice de qué tabla sale y qué se rompe hoy sin ella.

> **Los bloques «▶ Respuesta de `ARGOS_API`» los escribió el otro equipo el 2026-09-01.** Todo lo
> demás es la petición original de SIGTI, sin tocar: lo que se pidió y lo que se entregó tienen
> que poder leerse por separado.

---

## ▶ Respuesta de `ARGOS_API` — 2026-09-01

**Entregado y funcionando** contra `SICOVBD`. Se verificó cada premisa de esta lista contra la
base antes de construir: **el informe resultó exacto en todo lo comprobable.**

| # | Endpoint | Quién puede llamarlo | Estado |
|---|---|---|---|
| A-1 | `GET /api/v1/organizacion/jefaturas` | cualquier token | ✅ 16 unidades |
| A-2 | `GET /api/v1/personal/disponibilidad` | **token de sistema** | ✅ |
| B-1 | `GET /api/v1/presupuesto/gerencia/{id}` | **token de sistema** | ✅ |
| C-1 | `GET /api/v1/geografia/*` (4 rutas) | cualquier token | ✅ 18 / 298 / 895 / 7 |
| B-2 | `GET /api/v1/viaticos/gira/{id}` | — | ❌ **bloqueado**, ver abajo |

**Las 44 columnas nuevas entraron a las pruebas de contrato** — 96 columnas verificadas en total.
127 pruebas, ninguna omitida.

### ⚠️ Disponibilidad y presupuesto exigen token de sistema

No es una traba burocrática. **Una incapacidad dice algo de la salud de una persona**, y cuánto
tiene asignado una gerencia no es información que deba abrir la cuenta de cualquiera que trabaje
en el INM. Es el mismo criterio con que el padrón completo ya exigía credencial de sistema:
**lo que no es sobre quien pregunta, exige credencial de sistema.**

SIGTI ya se autentica así, o sea que no cambia nada de su lado.

### Cómo verlo

El servicio publica su contrato en `/openapi/v1.json` y una página para probarlo a mano en
**`/swagger`**, las dos sin token — describen la forma, no los datos. Hay además una colección de
Postman en `ARGOS_API/postman/`, con un script que captura el token solo.

## ⚠️ La regla que acota esta lista

**`ARGOS_API` es de solo lectura sobre `SICOVBD`** — es la primera de sus dos reglas no
negociables. Por eso acá **sólo se piden `GET`**. Nada de lo que SIGTI necesite *escribir* en
ARGOS puede resolverse en ese proyecto: se escribe en ARGOS, o no se escribe.

Eso tiene una consecuencia concreta, y está al final en «lo que esta lista no puede resolver».

## Lo que ya existe y no hay que construir

`GET /api/v1/calendario/feriados` **ya está**, y SIGTI todavía no lo consume: hoy los feriados
viven como parámetros propios de SIGTI. `SICOVBD.Feriados` tiene 22 filas. Eso es trabajo del
lado de SIGTI, no una petición.

---

## A. Bloquean el mínimo viable

### A-1 · Responsable de cada unidad — la jefatura inmediata

```
GET /api/v1/organizacion/jefaturas
```

**Por qué bloquea.** El escalamiento de §5.3.B.3 necesita saber a quién sube una tarea vencida.
Hoy los **21 puestos espejados tienen `Superior = null`** — los 7 que sí lo tienen son de la
siembra de desarrollo, no de ARGOS. El escalamiento no encuentra a nadie.

**De dónde sale, y ya existe:**

| Origen | Campos |
|---|---|
| `Unidades` | `UnidadID`, `EmpleadoResponsableID`, `SubResponsableUsuarioID`, `ResponsableAusente`, `AutoAprobacionDelJefe` |
| `DesignacionAprobadorUnidad` (4 filas) | `UnidadID`, `EmpleadoDesignadoID`, `Activo`, `AlcanceTodaGerencia` |

**Que venga como ARGOS lo tiene —por unidad—, no traducido a «superior de cada puesto».** SIGTI
resuelve la correspondencia de su lado; una traducción hecha allá se vuelve una regla de SIGTI
viviendo en otro repositorio.

⚠️ `ResponsableAusente` y el sub-responsable **no son adorno**: si el titular está ausente, el
escalamiento tiene que ir al que lo cubre. Que vengan los dos, y el estado de ausencia.

#### ▶ Respuesta de `ARGOS_API` — entregado, con dos advertencias

Devuelve las **16 unidades, todas con responsable declarado**, cada una con titular, suplente,
estado de ausencia, `autoAprobacionDelJefe` y sus designados. Viene **por unidad**, como se
pidió: la correspondencia con los puestos la resuelve SIGTI.

**Y viene por unidad también porque no hay otra forma.** Se buscó la jerarquía de puestos en
`SICOVBD` y **no existe**: la tabla `Cargo` son cuatro columnas —`CargoID`, `Nombre`,
`Descripcion`, `OlimpoId`— y ninguna apunta a un puesto superior ni a una unidad. Un barrido de
todo el esquema por columnas tipo `superior`, `padre`, `jefe` o `parent` no devolvió nada
utilizable. Lo que sí existe, y es real, es el responsable declarado de cada unidad.

> ⚠️ **El suplente no va a llegar, y hay que saberlo antes de construir sobre él.**
> `Unidades.SubResponsableUsuarioID` está **nulo en las 16 unidades**, y `ResponsableAusente` en
> `false` en las 16. ARGOS tiene las columnas y **no las puebla**.
>
> El endpoint las expone —el día que ARGOS empiece a usarlas llegan solas— pero **hoy el segundo
> salto del escalamiento de §5.3.B.3 no puede funcionar**, exista o no el endpoint. Que ARGOS
> empiece a poblarlas es una petición aparte, y va dirigida al equipo de ARGOS, no a `ARGOS_API`.

> ⚠️ **ARGOS tiene designaciones duplicadas.** Las 4 filas de `DesignacionAprobadorUnidad` son
> **dos pares**: la misma persona designada dos veces en la misma unidad, con una hora de
> diferencia en `FechaCreacion` (unidad 4 / empleado 197, y unidad 10 / empleado 6). Las cuatro
> están además con `Activo = false`.
>
> `ARGOS_API` devuelve **la más reciente de cada par**, para que SIGTI no cuente cuatro designados
> donde hay dos. Pero **el dato sucio sigue en ARGOS y hay que corregirlo allá** — esto es una
> curita, no el arreglo.

### A-2 · Disponibilidad del personal — incapacidades, vacaciones y bloqueos

```
GET /api/v1/personal/disponibilidad?desde=YYYY-MM-DD&hasta=YYYY-MM-DD
```

**Por qué bloquea.** Hoy SIGTI puede despachar en misión a alguien que ARGOS tiene **de
incapacidad**. Ninguno de los dos sistemas ve al otro, y el que se entera es el que se quedó
esperando el vehículo.

**De dónde sale:** `DisponibilidadEmpleado` (9 filas) — `EmpleadoID`, `FechaInicio`, `FechaFin`,
`TipoOcupacion`, `EstadoID`, `SolicitudUsuarioDetalleID`.

`TipoOcupacion` tiene hoy exactamente tres valores, verificados: **`INCAPACIDAD`**,
**`VACACIONES`**, **`BLOQUEO_GERENTE`**.

**Que venga el tipo tal cual, sin colapsarlo a un booleano «disponible».** Las tres cosas no se
tratan igual: una incapacidad es un impedimento, un bloqueo del gerente es una decisión, y SIGTI
necesita poder decir cuál de las dos fue.

⚠️ **Una pregunta que no pude contestar desde la base, y es del equipo de ARGOS:** ¿el personal
comprometido en una **gira** produce fila en `DisponibilidadEmpleado`? Los tres valores de
`TipoOcupacion` sugieren que **no**, y `DetalleAsignacionGira` sólo guarda `CantidadEmpleados`
—un número, no quiénes—. Si no la produce, **la doble movilización sigue siendo posible** y hace
falta una segunda fuente.

#### ▶ Respuesta de `ARGOS_API` — entregado, y la pregunta abierta tiene respuesta

El endpoint acepta `desde` y `hasta`; sin ellos, el mes que viene. Devuelve los períodos que
**se solapan** con el rango —no los que empiezan dentro—, porque una incapacidad que arrancó el
mes pasado y sigue vigente es justamente la que hay que ver. Un rango invertido responde **400**
en vez de una lista vacía, que se leería como «no hay nadie de incapacidad».

`tipo` va **tal como ARGOS lo escribe**, sin colapsarlo a booleano y sin traducirlo a un
enumerado: si ARGOS agrega un cuarto valor, un enumerado lo volvería un error de
deserialización en vez de un dato que SIGTI puede mostrar aunque no lo conozca.

> ⚠️ **Seis de las nueve filas están anuladas, y no se devuelven.** `DisponibilidadEmpleado`
> conserva los registros eliminados en la misma tabla: **6 de 9 tienen `EstadoID = 34`
> («Eliminado»)**. Devolverlas haría que SIGTI impidiera despachar a alguien que **sí** está
> disponible.
>
> Se filtran, y hay una prueba de contrato que verifica que el 34 sigue significando «Eliminado»
> — si ese catálogo se reordena, salta la alarma en vez de empezar a bloquear gente en silencio.

> ### ✅ La pregunta abierta: **no, una gira no produce fila**
>
> Verificado contra la base:
>
> - Las **9 filas** de `DisponibilidadEmpleado` tienen `SolicitudUsuarioDetalleID` **nulo**, las
>   nueve, sin excepción.
> - `TipoOcupacion` tiene exactamente los tres valores que ustedes encontraron —
>   `INCAPACIDAD` (3), `VACACIONES` (2), `BLOQUEO_GERENTE` (4)— y ninguno corresponde a una gira.
> - No hay ninguna tabla de `Gira` que registre **quién** viaja: un barrido de columnas de
>   empleado en tablas de gira sólo devuelve `DetalleAsignacionGira.CantidadEmpleados`, un
>   número, y respaldos viejos.
>
> **Conclusión: la doble movilización sigue siendo posible y este endpoint no la detecta.**
> ARGOS no registra por persona quién va en una gira, así que no hay nada que `ARGOS_API` pueda
> leer para resolverlo. Hace falta una segunda fuente, y esa decisión es del equipo de ARGOS.
>
> Conviene que quede escrito así de claro: **si SIGTI construye su control de doble movilización
> sobre este endpoint, va a pasar todas las pruebas y no va a detectar nada.**

---

## B. Cierran fronteras que `DP-001` dejó declaradas y sin implementar

### B-1 · Presupuesto por gerencia

```
GET /api/v1/presupuesto/gerencia/{gerenciaId}?anio=YYYY
```

**Para qué.** El fondo de combustible de `M-09` se imputa contra presupuesto, y
`ReversionDeCompromisos` produce el reporte de reversión sin poder cotejarlo contra el saldo real.

**De dónde sale:** `PresupuestosGerencia` (14 filas) — `GerenciaID`, `Anio`, `MontoAsignado`,
`MontoReservado`, `MontoEjecutado`, `FechaActualizacion`, `Activo`.

**Que venga `MontoReservado` separado de `MontoEjecutado`.** Reservado y ejecutado son estados
distintos del mismo dinero, y un solo «disponible» calculado allá borra la diferencia justo donde
importa.

#### ▶ Respuesta de `ARGOS_API` — entregado

Los **tres montos van separados y no se calcula ningún «disponible»**, exactamente como se pidió.
Sin `anio`, devuelve todos los ejercicios que la gerencia tenga cargados, del más reciente al más
viejo.

`fechaActualizacion` se declara en **UTC**: ARGOS lo guarda sin zona, y sin declararla un
consumidor en otra zona lo correría unas horas.

Una gerencia sin presupuesto cargado devuelve **lista vacía**, que es distinto de que la gerencia
no exista — el endpoint no inventa un 404 para eso.

> Exige **token de sistema**. Ver el bloque de la respuesta general arriba.

### B-2 · Viáticos asociados a una movilización

```
GET /api/v1/viaticos/gira/{giraId}
```

**Para qué.** `DP-001` D-01 sacó los viáticos de SIGTI y conservó **el vínculo**: una Orden de
Misión puede tener viáticos en ARGOS, y los dos sistemas comparten una clave para cruzarlos.

**De dónde sale:** `Gira` (19), `LiquidacionesViatico` (4), `Viatico` (105), `ViaticosVigencias`.

⚠️ **La clave compartida no existe todavía.** `Gira.Mision` está **vacía en las 19 filas** —la
revisé—. Sin clave, el cruce no se puede hacer en ningún sentido. Y **poblarla es escribir en
ARGOS**, que este proyecto no puede hacer. Ver el final.

#### ▶ Respuesta de `ARGOS_API` — ❌ no se construyó, y es deliberado

**Coincidimos con el diagnóstico, así que no se construyó nada.**

Un endpoint que devolviera viáticos cruzados por una clave vacía respondería `200` con lista
vacía siempre, para todas las giras. Eso es peor que no tenerlo: SIGTI lo integraría, pasaría
sus pruebas, y concluiría que **ninguna misión tiene viáticos asociados** — cuando la verdad es
que nadie puede saberlo todavía.

**El desbloqueo no está en `ARGOS_API`.** Poblar `Gira.Mision` es escribir en `SICOVBD`, y esa
es la regla que este servicio no relaja. El camino es un endpoint **en ARGOS mismo**, o que
ARGOS lea de SIGTI. Es la decisión que ustedes ya dejaron planteada en el punto 3 del final, y
sigue abierta.

Cuando la clave exista, el endpoint es directo: la consulta ya está identificada y sólo hace
falta su prueba de contrato.

---

## C. Catálogos que SIGTI no debería reconstruir

### C-1 · Geografía de Honduras

```
GET /api/v1/geografia/departamentos
GET /api/v1/geografia/municipios?departamento=
GET /api/v1/geografia/ciudades?municipio=
GET /api/v1/geografia/zonas
```

**Para qué.** Hoy el destino de una solicitud de SIGTI es **texto libre**. Con texto libre no se
concilia una ruta contra los peajes que la cruzan, ni se agrupa un reporte por departamento.

**De dónde sale, y es mucho:** `Departamento` (18), `Municipio` (298), `Ciudad` (895),
`Aldea` (3727), `ZonaGeografica` (7), `Pais` (194).

Reconstruir 3,727 aldeas en SIGTI sería exactamente lo que el principio rector de `DP-001`
prohíbe. **Prioridad menor que A y B** — se puede pilotear con texto libre; no se puede pilotear
sin jefaturas.

⚠️ Si esto llega, conviene mirar la estructura de `AsignacionGira`: ya resuelve destino con
`CiudadDestinoID`, `MunicipioID`, `ZonaGeograficaID`, `PaisDestinoID` y `EsInternacional`.

#### ▶ Respuesta de `ARGOS_API` — entregado, con una corrección al pedido

Se entregaron las cuatro rutas. Verificado: **18 departamentos, 298 municipios, 895 ciudades,
7 zonas.** Cada municipio trae su `zonaGeograficaId`, así que agrupar por región no exige volver
a decidir qué municipio cae en cuál. Las zonas traen `esInternacional`.

> ### ⚠️ Corrección: **una ciudad no cuelga de un municipio en ARGOS**
>
> Se pidió `GET /geografia/ciudades?municipio=`. **Ese filtro no se puede construir**: la tabla
> `Ciudad` se relaciona con `Pais` y **no tiene `MunicipioID`**. Sus columnas son `CiudadID`,
> `PaisID`, `Nombre`, `ISO3166_2`, `EsCapital`, `EsPrincipal`, `EsActivo`, `GooglePlaceID`,
> `IATA`, `UNLocode`, `Latitud`, `Longitud`.
>
> El endpoint quedó como **`GET /geografia/ciudades?pais=`**, y lo dice en su propia
> documentación en vez de fingir un filtro que devolvería cualquier cosa.
>
> Esto choca con lo que ustedes señalaron de `AsignacionGira`, que sí tiene `CiudadDestinoID` y
> `MunicipioID` **como columnas separadas** — o sea que en ARGOS ciudad y municipio son dos ejes
> independientes, no una jerarquía. Si SIGTI necesita «las ciudades de este municipio», ese
> vínculo **hay que crearlo en ARGOS**; no hay dato que `ARGOS_API` pueda leer para deducirlo.

`Aldea` (3.727) y `Pais` (194) **no se expusieron**: no los pidió ninguna de las cuatro rutas de
esta lista. Si hacen falta, es agregar dos rutas más — díganlo y se agregan con su prueba de
contrato.

---

## Lo que esta lista NO pide, y por qué

Tres cosas aparecieron al revisar `SICOVBD` que **no son peticiones: son decisiones suyas**. Las
dejo escritas en vez de resolverlas por mi cuenta.

### 1. ARGOS ya tiene peajes, y `DP-001` se los dio a SIGTI

`CasetaPeaje` (9), `TarifaPeaje` (1), `CategoriaVehiculoPeaje` (7), `GiraCasetaPeaje`,
`SolicitudUsuarioCasetasPeaje`. SIGTI tiene `peajes.Punto` (6) y `peajes.Tarifa` (12).

`DP-001` D-02 creó M-18 en SIGTI, y el principio rector del mismo documento dice **no replicar lo
que otro sistema ya hace**. Las dos cosas no pueden ser ciertas a la vez. **No lo resolví en
silencio**: o SIGTI es el dueño y ARGOS consume, o al revés, y eso lo decide el PO.

### 2. `Gira` ya lleva combustible y peaje

`OrdenCombustible`, `CombustibleEfectivo`, `IsCombustibleEfectivo`, `Peaje`,
`CategoriaVehiculoPeajeID`. Es coherente con lo que usted describió —que ARGOS hace *lo mínimo*
de transporte, y por eso nació SIGTI—, pero conviene decidir **cuándo ARGOS deja de llevarlo**, o
los dos van a tener una cifra distinta del mismo viaje.

### 3. Nada de esto puede escribirse desde `ARGOS_API`

La clave compartida de B-2 hay que poblarla **en ARGOS**. Igual cualquier cosa que SIGTI deba
devolverle: que la misión se cerró, cuánto combustible se consumió, qué peaje se pagó.

`ARGOS_API` es de solo lectura **a propósito**, y esa regla es lo que hace que no pueda dañar el
sistema del que depende toda la gestión de viáticos de la institución. **No la relaje para esto.**
El camino es un endpoint en ARGOS mismo, o que ARGOS lea de SIGTI. Es una decisión aparte, y hay
que tomarla.

#### ▶ Respuesta de `ARGOS_API` — de acuerdo con las tres, y siguen abiertas

Las tres son decisiones del PO y **ninguna se resolvió por cuenta de `ARGOS_API`**. Sobre la
tercera, en particular: la regla de solo lectura no se relajó y no se va a relajar por esto.
Quedó anotada en el `HANDOFF.md` de `ARGOS_API` como pregunta pendiente, con la distinción que
importa el día que lleguen los `POST`:

- Si el sistema escribe **en su propia base** —tablas nuevas, del sistema principal— la regla
  sobrevive intacta: lee ARGOS, escribe lo suyo.
- Si escribe **en `SICOVBD`**, eso rompe la regla fundacional. Romperla tiene que ser una
  decisión tomada, no una que ocurrió.

---

## Resumen para pasar al otro proyecto

| # | Endpoint | Sale de | Sin esto |
|---|---|---|---|
| A-1 | `GET /organizacion/jefaturas` | `Unidades`, `DesignacionAprobadorUnidad` | El escalamiento no encuentra a nadie |
| A-2 | `GET /personal/disponibilidad` | `DisponibilidadEmpleado` | Se despacha a alguien de incapacidad |
| B-1 | `GET /presupuesto/gerencia/{id}` | `PresupuestosGerencia` | El fondo no se imputa contra saldo real |
| B-2 | `GET /viaticos/gira/{id}` | `Gira`, `LiquidacionesViatico`, `Viatico` | El vínculo de `DP-001` D-01 no existe |
| C-1 | `GET /geografia/*` | `Departamento`, `Municipio`, `Ciudad`, `ZonaGeografica` | El destino sigue siendo texto libre |

**A-1 y A-2 son las que bloquean.** B y C mejoran; A destraba.

Y recuerde la regla del propio `ARGOS_API`: **toda consulta nueva agrega su prueba de contrato.**
Es la alarma que reemplaza al compilador el día que ARGOS renombre una columna.

---

## ▶ Cierre de `ARGOS_API` — 2026-09-01

Cuatro de las cinco peticiones entregadas. **Lo que ahora bloquea a SIGTI ya no está del lado de
`ARGOS_API`:** está en datos que ARGOS no tiene o no puebla.

### Lo que SIGTI puede integrar hoy

| # | Ruta | Token |
|---|---|---|
| A-1 | `GET /api/v1/organizacion/jefaturas` | cualquiera |
| A-2 | `GET /api/v1/personal/disponibilidad?desde=&hasta=` | **sistema** |
| B-1 | `GET /api/v1/presupuesto/gerencia/{id}?anio=` | **sistema** |
| C-1 | `GET /api/v1/geografia/{departamentos,municipios,ciudades,zonas}` | cualquiera |

### ⚠️ Lo que SIGTI **no** debería construir todavía, aunque el endpoint exista

1. **El segundo salto del escalamiento** (§5.3.B.3, «respaldo cuando el titular está ausente»).
   `SubResponsableUsuarioID` está nulo en las 16 unidades y `ResponsableAusente` en `false` en
   las 16. El campo llega vacío, siempre.
2. **El control de doble movilización.** ARGOS no registra por persona quién va en una gira.
   Construirlo sobre `/personal/disponibilidad` produciría un control que pasa todas las pruebas
   y no detecta nada.
3. **Cualquier cruce por `Gira.Mision`.** La clave está vacía en las 19 filas.

En los tres casos el endpoint responde `200` con datos correctos. **El problema no se ve
fallando: se ve funcionando y vacío**, que es la forma más cara de descubrirlo.

### Peticiones que `ARGOS_API` le devuelve al equipo de ARGOS

Ninguna es de este servicio — las tres son escritura o corrección en `SICOVBD`:

| # | Qué | Por qué importa |
|---|---|---|
| 1 | Poblar `Unidades.SubResponsableUsuarioID` y `ResponsableAusente` | Sin esto el escalamiento a un suplente no existe |
| 2 | Corregir las designaciones duplicadas en `DesignacionAprobadorUnidad` | 4 filas que son 2 designaciones |
| 3 | Registrar **por persona** quién va en una gira, o poblar `Gira.Mision` | Sin una de las dos, ni doble movilización ni cruce de viáticos |

### Y una advertencia para las dos partes

El día que ARGOS renombre una columna de las que este contrato lee, **nada falla en
compilación**: falla en producción devolviendo listas vacías. Las pruebas de contrato de
`ARGOS_API` son la alarma que reemplaza al compilador, y **sólo suenan si corren contra
`SICOVBD`**. Un `dotnet test` que reporte pruebas *omitidas* no verificó el esquema — salió en
verde sin haber mirado nada.
