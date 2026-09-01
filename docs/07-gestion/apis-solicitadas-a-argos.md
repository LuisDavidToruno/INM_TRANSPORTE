# APIs que SIGTI necesita de `ARGOS_API`

| Campo | Valor |
|---|---|
| **Fecha** | 2026-08-31 |
| **Pide** | SIGTI |
| **Provee** | `ARGOS_API` |
| **Estado** | Solicitado — pendiente de construir del lado de `ARGOS_API` |

Todo lo de abajo se verificó **contra `SICOVBD` en vivo**, no contra suposiciones. Cada petición
dice de qué tabla sale y qué se rompe hoy sin ella.

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
