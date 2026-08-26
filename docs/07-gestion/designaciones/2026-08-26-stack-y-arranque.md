# Designación de LOKI — Stack de SIGTI y arranque de la programación

| Campo | Valor |
|---|---|
| **Emite** | LOKI — gobierno técnico (`github.com/LuisDavidToruno/LOKI`) |
| **Fecha** | 2026-08-26 |
| **Estado** | **Autorizada por el Product Owner para empezar a programar** |
| **Ejecuta** | La sesión de este repositorio |

LOKI no escribe código acá: emite la designación y esta sesión la ejecuta.

**Este documento consolida y reemplaza la designación del 2026-08-24.** Aquella
proponía `ADR-002` a `ADR-009`; ninguno se escribió, y dos decisiones del PO
cambiaron desde entonces. En vez de dejar dos documentos que se contradicen en
partes, acá va **uno solo, completo y ejecutable**. Si algo de la designación
anterior circuló, este texto manda.

---

## 1. Convenciones de este repositorio que hay que respetar

Verificadas en `CLAUDE.md` y `docs/plantillas/adr.md`:

- **Plantilla de ADR obligatoria:** tabla Estado/Fecha/Decide/Sprint, y
  secciones Contexto · Requisitos que la condicionan · Decisión · Alternativas
  consideradas · Consecuencias (positivas, negativas y **deuda aceptada**) ·
  Revisión.
- **Un ADR no se edita cuando se cambia de opinión.** Se escribe uno nuevo que
  lo supere.
- **Los `M-xx` no se reciclan.** `M-10` (Viáticos) está **retirado** — lo maneja
  ARGOS por `DP-001`. No aparece en ninguna carpeta.
- **Regla de precedencia:** los actores y roles se **citan** desde
  `docs/01-negocio/actores-y-roles.md`, no se reescriben.

---

## 2. Orden de arranque — lo que se decide antes de la primera tabla

Ordenado por lo que **no se puede cambiar después**.

| # | Decisión | Por qué no espera |
|---|---|---|
| 1 | **Clave agrupada: ULID o `bigint` secuencial** | `ADR-005` manda identificadores del cliente. Un **GUID aleatorio** como clave agrupada fragmenta el índice en cada inserto, y 2014 Standard no tiene compresión de datos. Cambiarla después es reescribir cada índice y cada clave foránea |
| 2 | **`UseCompatibilityLevel(120)`** en EF Core | Vale **150** por omisión (verificado en `Microsoft.EntityFrameworkCore.SqlServer` 10.0.10). Sin esto, EF emite SQL que 2014 no entiende |
| 3 | **Base de desarrollo en `COMPATIBILITY_LEVEL = 120`** | Restaura el estimador de cardinalidad de 2014. Verificado: SQL Server 2025 lo acepta |
| 4 | **Las dos parejas de fechas** de la bitemporalidad | Agregar el eje de vigencia después toca todas las consultas |
| 5 | **Rangos de folio por delegación** | El cliente **no puede** asignar folio definitivo en campo (`RNF-21`) |

**Después:** el walking skeleton del Sprint 2 atraviesa **una orden de misión de
punta a punta** —solicitud → despacho → ejecución → liquidación— con su asiento
en bitácora. No un módulo completo: un hilo delgado que toca todas las capas,
que es lo que valida el stack antes de invertir en él.

En ese mismo esqueleto se mide **`RNF-12`: ≤ 25 % de batería en 8 h con
seguimiento activo, en gama baja.** Es el número que puede obligar a bajar el
seguimiento a un módulo nativo, y hay que saberlo en el Sprint 2, no en el 6.

**Las guardas de arquitectura se escriben con el primer módulo, no al final.**
Una guarda agregada después certifica lo que ya se rompió.

---

## 3. El stack

| Capa | Qué | Qué lo fuerza |
|---|---|---|
| **Campo** | React Native + SQLite cifrado (SQLCipher) | `RNF-03` · `RNF-08` · `RNF-12` · `RNF-13` · `RNF-15` |
| **Oficina** | React 19 + Vite + TS + Tailwind, desde la plantilla `diseno/` de LOKI (contrato **0.3.3**) | Módulo C de LOKI |
| **Backend** | .NET 10 + ASP.NET Core + EF Core, `UseCompatibilityLevel(120)` | `RNF-09` · `RNF-19` |
| **Base** | **SQL Server 2014 Standard** — restricción institucional dada | Licencia existente · `RNF-02` |
| **Cifrado** | Respaldo nativo + columna en aplicación + BitLocker | `RNF-13` |
| **Adjuntos** | Sistema de archivos, hash en la base | `RNF-02` · `RNF-04` |

### 3.1 SQL Server 2014 es una restricción, no una elección

El PO confirmó que la institución tiene licencia de 2014 y **no hay presupuesto
para más**. Eso activa la cláusula de revisión que `ADR-000` escribió para sí
mismo. **No elegimos 2014, nos lo encontramos**, y el `ADR-002` debe decirlo así.

**Está fuera de soporte extendido desde el 2024-07-09.** No recibe parches de
seguridad, y el sistema guarda datos personales de ciudadanos (`M-17`) bajo
control interno del TSC. **Eso va a la sección de deuda aceptada del `ADR-002`,
firmado por el PO.** La arquitectura compensa funciones que faltan; no compensa
parches que no llegan.

### 3.2 Qué no tiene 2014, y con qué se reemplaza

| Función | Desde | Reemplazo |
|---|---|---|
| **TDE** | Standard 2019 | Cifrado de respaldo nativo (**sí existe en 2014**) + `ValueConverter` por columna + BitLocker |
| **Temporal tables** | 2016 | Los dos ejes de la bitemporalidad se modelan a mano |
| **Particionado** | Standard 2016 SP1 | **Filegroups** de solo lectura para el histórico frío |
| **Compresión de datos** | Standard 2016 SP1 | Nada. Los ≈8 GB/año ocupan más en disco |

**Confirmar en la instancia real**: edición exacta, Service Pack, y que el
cifrado de respaldo esté disponible. **Express queda descalificado por
aritmética** — 10 GB por base contra ≈8 GB/año de `RNF-02`.

### 3.3 Desarrollo en versión moderna: cómo se hace bien

**Decisión del PO: se desarrolla sobre SQL Server moderno y se genera el script
para 2014.** Es lo correcto — programar diez años sobre un motor sin parches es
peor que mantener compatibilidad.

Verificado sobre la instancia de desarrollo (**SQL Server 2025, 17.0.1000.7
RTM, Standard Developer**, nivel por omisión 170): **acepta
`COMPATIBILITY_LEVEL = 120`**.

Con las dos palancas del punto 2 y 3 del orden de arranque, desarrollar en 2025
es **más seguro** que desarrollar en 2014: mismo SQL emitido, mismo estimador de
cardinalidad, y encima un motor parcheado.

> **Lo que el nivel de compatibilidad NO bloquea: las funciones.** En un
> servidor 2025 con la base en nivel 120 se pueden crear temporal tables,
> particionar y activar TDE igual. El script las incluiría y **2014 las
> rechazaría al aplicar**, no al desarrollar.
>
> Se atrapa de una sola forma: **el script generado se aplica contra una
> instancia 2014 real antes de cada entrega.** Es un paso de CI.

**Y una cosa necesita 2014 de verdad:** la medición de `RNF-01` y `RNF-02`. El
nivel 120 acerca mucho, pero el motor 2025 trae mejoras que el nivel no
controla. **La compuerta de rendimiento se corre en 2014** — periódica, no
diaria.

---

## 4. Los ADR que hay que escribir

### `ADR-002` — Adoptar el stack tecnológico en el Sprint 0

**Supersede a `ADR-000`.** Editar `ADR-000` solo para marcarlo
`Reemplazada por ADR-002`.

- **Contexto:** `ADR-000` difería la selección porque las restricciones
  determinantes se estaban descubriendo. Hoy los 21 RNF están escritos y esas
  restricciones están sobre la mesa — que era la condición implícita. Y la
  institución impuso el motor, que es la cláusula de revisión explícita.
- **Decisión:** la tabla de la sección 3.
- **Alternativas:** mantener el diferimiento; PostgreSQL (cuesta cero, soportado,
  no multiplica licencia por institución). **El PO evaluó PostgreSQL y eligió
  2014 por la licencia existente.** Registrarlo así: la alternativa gratuita se
  vio y se descartó, no pasó desapercibida.
- **Deuda aceptada, explícita:** motor fuera de soporte con datos personales; y
  `RNF-19` dice que SIGTI no es del INM — **la segunda institución que no tenga
  licencia paga por adoptarlo.**

### `ADR-003` — El cliente de campo es una aplicación instalada, no web

**Escribirlo directamente con React Native.** No escribir «nativa Android» para
superarlo después.

El argumento es contra **la web**, y se mantiene entero:

1. `RNF-08` — no hay geolocalización en segundo plano en la web: la pantalla
   bloqueada suspende la captura.
2. `RNF-12` — un runtime web es peor que nativo para el mismo ciclo de trabajo.
3. `RNF-03` — la cuota del navegador es desalojable y no la controla la
   aplicación.

**React Native no es web:** compila a una aplicación Android, corre servicios en
segundo plano por módulo nativo, escribe en el sistema de archivos y no tiene
cuota desalojable. Los tres puntos se cumplen.

- **A favor, y es un argumento de `RNF`:** un solo lenguaje entre oficina y
  campo. `RNF-15` es continuidad ante rotación de personal, y las reglas puras
  y los esquemas de validación se comparten de verdad, no se transcriben.
- **En consecuencias negativas:** `RNF-12` exige ≤ 25 % de batería en 8 h. Es el
  único número donde React Native es medible peor que Kotlin. **Plan de
  contingencia escrito:** si no pasa, se baja el seguimiento a un módulo nativo
  propio — no se reescribe la aplicación.
- **Consecuencia:** dos aplicaciones cliente. Comparten API y reglas de dominio,
  no código de interfaz.

> El argumento del borrado a los 7 días por ITP de iOS **queda fuera**: los
> equipos son solo Android por decisión del PO. Incluirlo debilitaría el ADR con
> una objeción fácil.

### `ADR-004` — Fotografías y adjuntos fuera de la base

Enlaza `RNF-03` (≥200 fotos por dispositivo) y `RNF-09`.

≈30 GB/año de adjuntos contra ≈8 GB de relacional. Dentro de la base cuadruplican
el respaldo y sacan la restauración de las 2 h que `RNF-09` permite.

**Sistema de archivos plano, con la ruta y el hash en la base.** Nada de
FILESTREAM ni FileTable: agregan complejidad operativa que `RNF-09` no admite.

**No es diferible:** sacar blobs después es migración **más** reescritura del
plan de respaldo. Y cambia el procedimiento de restauración a *base + almacén de
archivos, consistentes entre sí* — eso se escribe desde el principio.

### `ADR-005` — Los identificadores se generan en el cliente

Enlaza `RNF-21` y `RNF-03`. **GUID o ULID desde la primera tabla.** Si se arranca
con `int IDENTITY`, aceptar identificadores del cliente después obliga a
reescribir cada clave foránea.

Incluir la **clave de idempotencia de la ingesta**: identificador estable del
cliente **más** secuencia monótona del dispositivo. Va en el modelo, no en el
sincronizador.

**Añadido por el motor:** ver el punto 1 del orden de arranque — la clave
agrupada no puede ser un GUID aleatorio.

### `ADR-006` — Temporalidad bitemporal desde el modelo inicial

Enlaza `RNF-05` y `RNF-06`. **Sin temporal tables, los dos ejes son propios:**

| Eje | Columnas | Qué responde |
|---|---|---|
| Vigencia normativa | `VigenteDesde` / `VigenteHasta` | Qué decía el reglamento el día del viaje |
| Tiempo de transacción | `RegistradoDesde` / `RegistradoHasta` | Qué sabía el sistema el día que se liquidó |

**Decir explícitamente que ninguno es nativo**, o alguien va a implementar uno y
suponer que el otro viene puesto.

**Por qué no es diferible:** poner «qué valor regía ese día» encima de tablas que
solo guardan el valor actual obliga a **inventar una historia que no se tiene**.
En SICOV lo pagaron tres veces, y en la tercera el seeder **se negó a sembrar
valores** porque habría creado una vigencia falsa firmada por el sistema — justo
en la tabla que existe para saber qué rigió y cuándo.

**Sinergia a aprovechar:** `ADR-009` ya exige que las reglas puras reciban la
fecha como parámetro. Esa es exactamente la firma que la bitemporalidad
necesita: `Reglas.CalcularX(datos, vigenteAl)`. Si una sola regla lee
`DateTime.Now`, se pierden las dos cosas a la vez.

### `ADR-007` — Marcas de tiempo en UTC con el desfase del dispositivo

Enlaza `RNF-03`, `RNF-04` y `RNF-08`.

En Honduras (UTC−6), `UtcNow.Date` y la fecha local **divergen a partir de las
18:00**. SICOV tiene una regla entera y una guarda (`UnSoloHoyTests`) porque eso
rompió un guard en vivo: el formulario proponía una fecha y la validación exigía
otra, sin forma de deducir qué faltaba.

Con marcas generadas en dispositivos, captura offline de 7 días y cadena de hash,
esto se decide **antes de la primera fila**. Definir «día hábil» en un solo lugar.

### `ADR-008` — Los permisos se publican como capacidad, nunca como rol

Enlaza `RNF-14` y `RNF-19`. El servidor publica **qué puede hacer** el usuario,
no **qué es**. Nunca un flag `esAdministrador`: obligaría al cliente a
implementar la regla del bypass, y la regla viviría en dos lados.

**Pesa el doble acá porque hay dos clientes.** Si cada uno deriva permisos de
roles, divergen — y la divergencia es invisible: **un botón ofrecido que el
servidor rechaza se lee como falla del sistema, no como regla.**

Con 40 delegaciones, los roles se multiplican y las capacidades no.

### `ADR-009` — Módulos verticales con reglas compartidas puras y guardas

**Lo que se decide:**

- Organización por **módulo vertical**, con los `M-xx` reales de `CLAUDE.md`.
  Los del núcleo operativo son `M-06`, `M-07`, `M-08`, `M-09`, `M-13` y `M-15`.
  **Si conviene agrupar varios bajo una carpeta, esa agrupación la decide quien
  escriba el ADR**, con criterio de cohesión y registrada ahí. LOKI no la
  propone: inventar el mapa de módulos una vez ya fue un error suyo.
- **Regla de dependencia de Clean Architecture, sin su ceremonia.** El dominio no
  conoce ORM ni framework web. Las cuatro capas canónicas con interfaz para todo
  y DTO en cada frontera **no** se adoptan. Ver la sección 8: es la única
  decisión que el PO podría querer al revés.
- **Repositorios con intención**, no genéricos. `PendientesDeLiquidar`, no
  `IRepository<T>`. Tres o cuatro en todo el sistema.
- **Distinción catálogo / agregado.** Un catálogo (aldea, municipio, asunto) es
  dato sin invariantes: se lee directo, sin repositorio ni servicio ni caso de
  uso. El aparato de dominio se reserva para lo que tiene reglas.
- **Carpeta `Reglas/` — nunca `Comun/`.** «Común» es el nombre al que las cosas
  van a la deriva; nadie lo abre a preguntarse si la regla ya está ahí.
  Funciones **puras**: sin base de datos, sin HTTP, sin `DateTime.Now` adentro.
- **La regla y su prueba nacen juntas.** La guarda se escribe el día que aparece
  el **segundo** consumidor, no después del defecto en vivo.
- **Una guarda que no encuentra nada debe fallar, no pasar.** Toda guarda lleva
  su aserción de cordura: «revisé más de N archivos».

**El argumento honesto, y va en el ADR:** los verticales se eligen por
**cohesión** —poder leer un módulo entero y entenderlo— **no** porque reduzcan
la duplicación. La reducen menos que las capas: con verticales el riesgo de que
la misma regla se reimplemente en cada camino **sube**. Por eso `Reglas/` y las
guardas son la contramedida obligatoria de esta decisión, no un accesorio.

**Evidencia medida en SICOV_CORE8:**

- 62 pruebas de arquitectura. **47 de 62 nombran duplicación o divergencia**;
  solo 11 mencionan capas.
- 21 clases `Reglas*`, de las cuales **12 tienen prueba propia (57 %)**.
- El limpiador de HTML tenía **siete copias privadas** y ninguna hacía lo mismo.
  El descuento del último día estaba en **cuatro constantes con tres tipos
  distintos** (`float`, `decimal`, `double`).
- **Y un caso donde la capa horizontal fue la solución:** la regla «quien debe
  una liquidación no viaja» nació dentro de un controlador, y desde ahí el otro
  wizard no podía consumirla. Costó asignar a una persona con liquidación vencida
  a un destino en Madrid por **L. 73.047,96**, sin aviso. Se arregló
  **subiéndola** a la capa de aplicación. Por eso `Sigti.Aplicacion` existe.

> **No usar el argumento de los 775 archivos.** Una medición inicial atribuyó el
> tamaño de `SICOV_CORE8.AccesoDatos` a las capas horizontales. **Es falso: 592
> de esos 775 son migraciones de EF generadas.** Código a mano: 183. Es
> verificable en un minuto y desmontarlo desacredita el resto.

### `docs/03-arquitectura/c4/` — contexto y contenedores

Hoy la carpeta está vacía. Crear `README.md`, `contexto.md` y `contenedores.md`
con diagramas Mermaid `C4Context` y `C4Container`.

- **Contexto (nivel 1):** SIGTI, una instancia por institución. Externos:
  **ARGOS** (viáticos, estructura presupuestaria, autorizaciones, mapas) y
  **Talento Humano** (expedientes, licencias, permisos, feriados), ambos por
  **espejo local de solo lectura sincronizado por webhooks** (`ADR-001`, no
  reabrir). Actores **citados** desde `docs/01-negocio/actores-y-roles.md`.
- **Contenedores (nivel 2):** back-office web, cliente de campo React Native,
  API, base de datos, almacén de archivos (`ADR-004`), bitácora append-only
  (`RNF-04`), espejo de sistemas externos, servicio de sincronización.
- **Marcar con `[C]`** todo lo que dependa de insumos abiertos.

---

## 5. La estructura concreta

### 5.1 Backend — cuatro proyectos

```
src/
├─ Sigti.Dominio/       ← CERO dependencias. Ni EF Core ni ASP.NET
├─ Sigti.Datos/         ← Dominio
├─ Sigti.Aplicacion/    ← Dominio + Datos
└─ Sigti.Api/           ← las tres
pruebas/Sigti.Pruebas/
campo/                  ← React Native
oficina/                ← React, desde diseno/
```

**Un proyecto nuevo se justifica cuando hay una regla de dependencia que hacer
cumplir mecánicamente, no cuando hay un concepto nuevo que nombrar.** El concepto
se separa con una carpeta; el compilador hace falta cuando la dependencia tiene
que ser **imposible**, no solo desaconsejada.

```
Sigti.Dominio/
├─ Reglas/                        ← puras: sin BD, sin HTTP, sin DateTime.Now
│  ├─ ReglasDeVigencia.cs         ← eje normativo de RNF-05
│  ├─ ReglasDeFolio.cs            ← RNF-21
│  └─ ReglasDeConsumo.cs          ← galonaje ↔ kilometraje, M-09
├─ Bitacora/                      ← M-14: qué ES un asiento y cómo encadena
│  ├─ Asiento.cs
│  └─ CadenaDeHash.cs             ← pura: dado el anterior y el contenido, el hash
├─ M03_Flota/ · M06_Solicitudes/ · M07_ProgramacionYDespacho/ · M13_LiquidacionYCierre/
```

```
Sigti.Datos/
├─ SigtiDbContext.cs · Migrations/ · Configuraciones/
├─ Cifrado/ConvertidorCifrado.cs  ← ValueConverter. El dominio no se entera
├─ Bitacora/EscritorDeBitacora.cs ← sp_getapplock + secuencia monótona
└─ Repositorios/                  ← con intención, tres o cuatro en total
```

```
Sigti.Aplicacion/
├─ M06_Solicitudes/ · M07_ProgramacionYDespacho/ · M13_LiquidacionYCierre/
└─ M16_Sincronizacion/            ← ingesta de eventos, no CRUD
```

**Tres advertencias:**

1. **No son 19 carpetas × 4 proyectos.** Un módulo tiene carpeta **solo donde
   tiene algo**. `M-02 Catálogos Maestros` es dato sin invariantes: sin carpeta
   en `Dominio`, sin repositorio, sin caso de uso. Si nacen 76 carpetas y la
   mitad tiene un archivo, la estructura ya falló.
2. **La agrupación de los `M-xx` la decide esta sesión**, con criterio de
   cohesión.
3. **`Reglas/` sin guardas es peor que no tenerla.**

**Guardas de arquitectura**, que son parte del diseño:

```
Sigti.Pruebas/Arquitectura/
├─ DominioNoConoceInfraestructura.cs   ← ni EF Core ni ASP.NET en Sigti.Dominio
├─ NingunaReglaLeeElReloj.cs           ← DateTime.Now dentro de Reglas/ = falla
├─ CadaReglaTieneSuPrueba.cs           ← en SICOV solo el 57 % las tenía
└─ NoExisteCarpetaComun.cs
```

### 5.2 Oficina — React desde `diseno/`

```
oficina/src/
├─ app/                 ← shell, rutas, migas
├─ modulos/M06_Solicitudes/ · modulos/M07_Despacho/
├─ dominio/             ← vocabulario compartido
├─ ui/                  ← el sistema de diseño. No importa de modulos/
└─ api/
```

**`modulos/X` no importa de `modulos/Y`.** Lo compartido baja a `dominio/`. Es
la misma trampa que `Comun/` en el backend, con el mismo remedio.

**Falta una pieza en la plantilla:** `diseno/` no trae **TanStack Query** y hace
falta. El estado de servidor es caché de consultas, no estado global. Nada de
Redux ni Zustand para datos que viven en la base.

### 5.3 Campo — local-first, y esto decide todo

```
campo/src/
├─ almacen/           ← SQLite cifrado. LA FUENTE DE VERDAD, no un caché
├─ sincronizacion/    ← bandeja de salida, reintentos, reconciliación
├─ dominio/           ← las MISMAS reglas puras que la oficina
├─ pantallas/         ← leen del almacén. NUNCA de la red
└─ ui/
```

**Ninguna pantalla llama a la red. Ninguna.** Escriben en SQLite; el motor de
sincronización se encarga cuando hay señal. Si una sola pantalla hace
`await fetch(...)`, la aplicación funciona en la oficina y **falla en Gracias a
Dios al tercer día** — que es donde `RNF-03` dice que tiene que funcionar.

Toda escritura entra a la **bandeja de salida** con id de cliente y secuencia
monótona (`ADR-005`). El motor empuja, reintenta y reconcilia.

`dominio/` es **el mismo paquete TypeScript** que la oficina. Eso es `RNF-15`
hecho código, y era el argumento fuerte de elegir React Native.

### 5.4 Base de datos

- **Esquemas de SQL Server como espejo de los módulos:** `flota.Vehiculo`,
  `mision.OrdenDeMision`, `bitacora.Asiento`, `catalogo.Zona`. Los permisos se
  otorgan **por esquema**, que es lo que `RNF-14` necesita con 40 delegaciones.
- **Cero `DELETE` en todo el sistema.** `RNF-02` lo pone como métrica:
  *«registros eliminados físicamente: 0»*. Toda anulación es asiento reverso
  (`RN-04`).
- **Histórico frío en filegroups de solo lectura**, respaldados una vez en lugar
  de cada noche. Se diseña ahora: mover datos entre filegroups después toca
  todas las claves foráneas.
- **La cadena de hash necesita un punto de serialización.** Es inherentemente
  secuencial: el asiento *n* necesita el hash del *n−1*. Con 60 concurrentes,
  dos transacciones que lean la misma cola **bifurcan la cadena** y deja de
  detectar alteraciones — lo único para lo que existe. Solución: **`sp_getapplock`
  sobre la cola dentro de la transacción**, más secuencia monótona. Sesenta
  usuarios no son sesenta escrituras por segundo.
  **No** calcularla en un interceptor de `SaveChanges` sin serializar: funciona
  con un usuario y bifurca en producción.
- **El cálculo del hash es puro y vive en el dominio; la serialización vive en
  `Datos`.** Así la verificación de la cadena se prueba sin base de datos, que
  es lo que una auditoría necesita poder hacer.
- **Cifrado por columna:** `ValueConverter` sobre las propiedades sensibles. El
  costo se acepta **columna por columna**: una columna cifrada **no se puede
  buscar, ordenar ni filtrar por rango**. Si el nombre de un pasajero externo va
  cifrado, buscar por nombre deja de funcionar. Se registra en el ADR qué
  consulta se sacrifica en cada caso.

---

## 6. «Robusta, segura y escalable» — traducido a umbrales

El PO pidió una arquitectura robusta, segura y escalable. Las tres palabras
sueltas no obligan a nada, así que van a los umbrales que ya existen y que
alguien puede **fallar**:

| Palabra | Mide | Umbral |
|---|---|---|
| **Robusta** | `RNF-03` | **0 registros perdidos** tras sincronizar, con 7 días sin red |
| | `RNF-09` | Restauración **probada** por no especialista, ≤ 2 h |
| | `RNF-10` | Disponibilidad y recuperación |
| **Segura** | `RNF-13` | Cifrado en tránsito y reposo, **sin modo de compatibilidad** |
| | `RNF-14` | Acceso por puesto y **registro de consultas** |
| | `RNF-04` | Bitácora append-only con cadena verificable |
| **Escalable** | `RNF-01` · `RNF-02` | Umbrales con **`3 × JDR-1`**, degradación de p95 ≤ 50 % |

**«Escalable» acá NO significa horizontal.** Una instancia por institución, 60
concurrentes, on-premise. Escalar es **aguantar un acervo que nunca se borra**,
no repartir carga entre nodos. Es la palabra que más rápido produce
microservicios, y `RNF-09` dice que en la delegación no hay quien los opere.

**Falta una cuarta palabra, y es la que filtra: operable.** `RNF-09` no es meta
de calidad, es **filtro de elegibilidad**. Un sistema robusto, seguro y escalable
que necesita un DBA para restaurarse **no es elegible**.

---

## 7. Lo que NO hay que hacer

- **No reabrir `ADR-001`.** El espejo local por webhooks está decidido.
- **No escribir `ADR-003` con «nativa Android»** para superarlo después.
- **No usar temporal tables, particionado ni TDE**, aunque el servidor de
  desarrollo los ofrezca. El nivel de compatibilidad no los apaga.
- **No inventar el contenido de `docs/03-arquitectura/seguridad/`.** Es un cuerpo
  propio y merece su propia sesión. Vacío es más honesto que a medias.
- **No duplicar bajo la raíz lo que ya vive en `docs/`.** La designación
  anterior decía «no crear los documentos de raíz que LOKI exige»; este
  repositorio los resolvió mejor de lo que esa instrucción proponía —
  `DECISIONES.md` y `HANDOFF.md` existen como **índices consolidados** que
  apuntan a `docs/`, no como copias. **Ese es el patrón correcto y se
  mantiene.** Faltan `ARQUITECTURA.md` y `DESPLIEGUE.md`: si se escriben, que
  sea con el mismo criterio — índice que remite, nunca contenido duplicado que
  después diverge.
- **No agregar CQRS con bases separadas, event sourcing, microservicios ni un
  mediador en cada llamada.** Cada pieza móvil es una pieza que alguien tiene
  que entender a las 11 de la noche siguiendo un documento.
- **No empezar por el sincronizador.** Los folios por delegación se diseñan
  antes: el cliente no puede asignar folio definitivo en campo.
- **No prometer portabilidad de base de datos** en ningún ADR.
- **No usar el argumento de los 775 archivos.**

---

## 8. La decisión que el PO todavía no tomó

**¿Clean Architecture con ceremonia o sin ella?**

Clean son dos cosas. **La regla se adopta y se hace cumplir mecánicamente:**
`Sigti.Dominio` no referencia EF Core ni ASP.NET, el `.csproj` no lo permite, y
una prueba de arquitectura falla si alguien lo intenta.

**La ceremonia** —interfaz por agregado, DTO en cada frontera, caso de uso como
clase— es lo que `ADR-009` descarta:

| | Sin ceremonia (lo escrito) | Con ceremonia |
|---|---|---|
| Por operación | 1 archivo | 3–4 archivos |
| Extra | — | Capa de mapeo entidad↔DTO en los 19 módulos |
| Evidencia | 47 de 62 pruebas de SICOV son sobre duplicación, no capas | Fronteras explícitas para equipo grande o rotativo |

Con 19 módulos y `RNF-15` hablando de rotación de personal, **el argumento a
favor de la ceremonia no es ridículo.** Pero la evidencia medida apunta al otro
lado, y la contramedida que sí ataca el problema real —`Reglas/` con guardas— ya
está en el plan.

**Si el PO decide lo contrario, se registra en `ADR-009` con su motivo y esta
designación no se opone.** Preguntarle antes de escribir el primer módulo.

---

## 9. Preguntas abiertas que bloquean

1. **Edición exacta y Service Pack** de la instancia 2014, y si el **cifrado de
   respaldo** está disponible ahí. Bloquea `RNF-13`.
2. **¿La licencia tiene Software Assurance vigente?** Si la tiene, la
   actualización de versión ya está pagada y buena parte de esta designación
   sobra.
3. **Insumo #73, reformulado:** dónde vive la llave del cifrado por columna,
   quién la custodia, y **quién probó una restauración completa con ella en otra
   máquina**. `RNF-09` asume que en la delegación no hay equipo de TI.
4. **Aceptación por escrito** del riesgo de operar un motor fuera de soporte con
   datos personales de ciudadanos.
5. **Licenciamiento para la segunda institución.** No bloquea el piloto; bloquea
   la promesa de `RNF-19`.
