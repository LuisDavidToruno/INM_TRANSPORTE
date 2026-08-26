# ADR-009 — Módulos verticales con reglas compartidas puras y guardas

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

## Contexto

SIGTI tiene 19 módulos vivos. La pregunta de cómo organizar el código tiene dos respuestas clásicas —por capa horizontal o por módulo vertical— y las dos se defienden con argumentos que suenan igual de bien en abstracto.

Acá se decide con **evidencia medida sobre un sistema real del mismo autor**, `SICOV_CORE8`, no con preferencia.

## Requisitos que la condicionan

- [`RNF-15`](../../02-requisitos/no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md) — continuidad ante rotación de personal: alguien nuevo tiene que poder entender un módulo
- [`RNF-05`](../../02-requisitos/no-funcionales/RNF-05-temporalidad-normativa.md) — las reglas resuelven a la fecha del hecho, lo que exige que reciban la fecha como parámetro
- [`RNF-09`](../../02-requisitos/no-funcionales/RNF-09-instalacion-respaldo-y-restauracion.md) — cada pieza móvil es una pieza que alguien tiene que entender a las 11 de la noche

## Decisión

### 1. Organización por módulo vertical, con los `M-xx` reales

Cuatro proyectos, con la regla de dependencia hecha cumplir por el compilador:

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

**Un proyecto nuevo se justifica cuando hay una regla de dependencia que hacer cumplir mecánicamente, no cuando hay un concepto nuevo que nombrar.** El concepto se separa con una carpeta; el compilador hace falta cuando la dependencia tiene que ser **imposible**, no solo desaconsejada.

### 2. La agrupación de módulos — decidida aquí

La designación delega explícitamente esta decisión, con criterio de cohesión. **No son 19 carpetas × 4 proyectos**: un módulo tiene carpeta **solo donde tiene algo**.

| Carpeta en `Dominio/` | Agrupa | Por qué |
|---|---|---|
| `M03_Flota/` | M-03 Flota + M-04 Documentación y cumplimiento | El vencimiento de matrícula y seguro es estado del vehículo, no una entidad aparte. Separarlos obliga a consultar dos módulos para saber si un vehículo puede salir |
| `M05_Motoristas/` | M-05 Padrón y habilitación | La matriz licencia↔vehículo es su invariante central |
| `M06_Solicitudes/` | M-06 | |
| `M07_ProgramacionYDespacho/` | M-07 | Donde vive la segregación de funciones al asignar |
| `M08_Ejecucion/` | M-08 Bitácora + M-19 Seguimiento en ruta | Los dos describen el vehículo en movimiento y comparten las mismas marcas de tiempo y posiciones. Separarlos duplicaría el modelo de evento en ruta |
| `M09_Combustible/` | M-09 | |
| `M11_Taller/` | M-11 Mantenimiento | Tiene su propio ciclo de vida y su propia indisponibilidad |
| `M12_Incidentes/` | M-12 Incidentes, siniestros y sanciones | |
| `M13_LiquidacionYCierre/` | M-13 + M-18 Peajes | El peaje se estima al programar y **se concilia al liquidar**; su categoría por ejes es un parámetro, no un agregado |
| `M15_FormatosOficiales/` | M-15 | Folios y correlativos (`RNF-21`) |
| `M17_PersonasExternas/` | M-17 | Minimización de datos y cadena de custodia: invariantes propios y severos |
| `Parametros/` | **La parte de M-02 que tiene invariantes** | Ver el punto 3 |
| `Organizacion/` | M-01 | Alcance de datos y capacidades (`ADR-008`) |

**Sin carpeta en `Dominio/`:** M-14 Reportes (lee, no tiene invariantes propios), M-16 Sincronización (vive en `Aplicacion`), M-20 Integraciones (`Datos` y `Aplicacion`), y los catálogos simples de M-02.

> **Si nacen 76 carpetas y la mitad tiene un archivo, la estructura ya falló.**

### 3. Divergencia deliberada con la designación: M-02 está partido en dos

La designación dice que `M-02 Catálogos Maestros` es *«dato sin invariantes: sin carpeta en Dominio, sin repositorio, sin caso de uso»*. **Eso es cierto para la mitad de M-02 y falso para la otra mitad**, y adoptarlo entero habría dejado sin dominio a un conjunto de reglas duras.

Las historias `HU-144` a `HU-150`, escritas al corregir el hallazgo `HB34-05`, describen los parámetros normativos con vigencia. Entre ellas:

- `HU-145` — la puesta en vigencia exige **doble control**
- `HU-146` — **bloquear que quien carga un parámetro apruebe su propia carga**
- `HU-147` — resolver el parámetro **a la fecha del hecho**
- `HU-148` — corrección retroactiva **con asiento de diferencia**

`HU-146` es segregación de funciones, que es bloqueo duro. Eso no es *dato sin invariantes*: es un agregado con reglas tan severas como las de una orden de misión.

**La partición que se adopta:**

| Parte de M-02 | Tratamiento |
|---|---|
| **Catálogos simples** — zonas, motivos de viaje, estaciones, tipos de carga | Dato sin invariantes. Se leen directo: sin repositorio, sin servicio, sin caso de uso |
| **Parámetros normativos con vigencia** — tarifas de peaje, categorías por ejes, matriz licencia↔vehículo, feriados, horario hábil, plazos | Carpeta `Parametros/` en `Dominio`, con sus invariantes de doble control y bitemporalidad ([`ADR-006`](ADR-006-temporalidad-bitemporal.md)) |

Se registra como divergencia explícita porque la designación es de LOKI y esta decisión la contradice en un punto. **La autoridad sobre qué reglas existen son las historias y las reglas de negocio**, no la designación — y las historias dicen que ahí hay invariantes.

### 4. La regla de dependencia de Clean Architecture, **sin su ceremonia**

**Se adopta y se hace cumplir mecánicamente:** `Sigti.Dominio` no referencia EF Core ni ASP.NET. El `.csproj` no lo permite y una prueba de arquitectura falla si alguien lo intenta.

**No se adopta la ceremonia:** interfaz por agregado, DTO en cada frontera, caso de uso como clase.

| | Sin ceremonia (lo decidido) | Con ceremonia |
|---|---|---|
| Por operación | 1 archivo | 3–4 archivos |
| Extra | — | Capa de mapeo entidad↔DTO en cada módulo |
| Evidencia | 47 de 62 pruebas de arquitectura de SICOV son sobre duplicación, no sobre capas | Fronteras explícitas, que ayudan a un equipo grande o muy rotativo |

> **Esta es la única decisión de este ADR que el Product Owner podría querer al revés**, y la designación lo dice así. Con 19 módulos y `RNF-15` hablando de rotación de personal, el argumento a favor de la ceremonia no es ridículo — pero la evidencia medida apunta al otro lado, y la contramedida que ataca el problema real (`Reglas/` con guardas) ya está en el plan. **Si el PO decide lo contrario, se registra acá con su motivo y se escribe el ADR que supera a este.**

### 5. Repositorios con intención, no genéricos

`PendientesDeLiquidar`, no `IRepository<T>`. **Tres o cuatro en todo el sistema.** Un repositorio genérico es una capa de indirección que no expresa ninguna intención y que hay que atravesar para leer cualquier cosa.

### 6. Distinción catálogo / agregado

Un catálogo es dato sin invariantes: se lee directo. **El aparato de dominio se reserva para lo que tiene reglas.** Envolver una tabla de aldeas en repositorio, servicio y caso de uso es ceremonia que no protege nada.

### 7. Carpeta `Reglas/` — nunca `Comun/`

```
Sigti.Dominio/Reglas/
├─ ReglasDeVigencia.cs      ← eje normativo de RNF-05
├─ ReglasDeFolio.cs         ← RNF-21
└─ ReglasDeConsumo.cs       ← galonaje ↔ kilometraje, M-09
```

*«Común»* es el nombre al que las cosas van a la deriva: nadie lo abre a preguntarse si la regla ya está ahí. `Reglas/` sí se abre, porque su nombre dice qué contiene.

**Funciones puras**: sin base de datos, sin HTTP, **sin `DateTime.Now` adentro**. La fecha entra como parámetro — `Reglas.CalcularX(datos, vigenteAl)` —, que es exactamente la firma que `ADR-006` y `ADR-007` necesitan.

### 8. Las guardas de arquitectura se escriben con el primer módulo

```
Sigti.Pruebas/Arquitectura/
├─ DominioNoConoceInfraestructura.cs   ← ni EF Core ni ASP.NET en Sigti.Dominio
├─ NingunaReglaLeeElReloj.cs           ← DateTime.Now dentro de Reglas/ = falla
├─ CadaReglaTieneSuPrueba.cs           ← en SICOV solo el 57 % las tenía
└─ NoExisteCarpetaComun.cs
```

- **`Reglas/` sin guardas es peor que no tenerla**: da la sensación de que el problema está resuelto.
- **La regla y su prueba nacen juntas.** La guarda se escribe el día que aparece el **segundo** consumidor, no después del defecto en vivo.
- **Una guarda que no encuentra nada debe fallar, no pasar.** Toda guarda lleva su aserción de cordura: *«revisé más de N archivos»*. Una guarda que pasa porque su patrón dejó de coincidir es peor que ninguna: certifica lo que ya se rompió.
- **Una guarda agregada al final certifica lo que ya se rompió.** Van con el primer módulo.

### 9. En la oficina y en el campo, la misma regla de frontera

```
oficina/src/                       campo/src/
├─ app/                            ├─ almacen/      ← SQLite cifrado. FUENTE DE VERDAD
├─ modulos/M06_Solicitudes/        ├─ sincronizacion/
├─ dominio/    ← vocabulario       ├─ dominio/      ← EL MISMO paquete que la oficina
├─ ui/         ← no importa de     ├─ pantallas/    ← leen del almacén, NUNCA de la red
└─ api/           modulos/         └─ ui/
```

**`modulos/X` no importa de `modulos/Y`.** Lo compartido baja a `dominio/`. Es la misma trampa que `Comun/` en el backend, con el mismo remedio.

**Ninguna pantalla de campo llama a la red. Ninguna.** Si una sola hace `await fetch(...)`, la aplicación funciona en la oficina y **falla en Gracias a Dios al tercer día** — que es donde `RNF-03` dice que tiene que funcionar.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **Capas horizontales** (`Servicios/`, `Repositorios/`, `Modelos/`) | Menos duplicación: una regla tiene un lugar obvio; frontera clara para equipo nuevo | Para entender el despacho hay que abrir cinco carpetas. Con 19 módulos, cada carpeta acumula decenas de archivos sin relación entre sí | `RNF-15` es poder leer un módulo entero y entenderlo. Las capas lo impiden |
| **Clean Architecture completa, con ceremonia** | Fronteras explícitas; independencia de framework demostrable | 3–4 archivos por operación y mapeo entidad↔DTO en 19 módulos | Ver el punto 4. Es la decisión reversible de este ADR |
| **Microservicios por módulo** | Despliegue independiente | 19 servicios que operar on-premise, sin equipo de TI en la delegación | `RNF-09` lo descalifica de entrada |
| **CQRS con bases separadas / event sourcing / mediador en cada llamada** | Patrones conocidos, escalan bien en otros contextos | Cada pieza móvil es una que alguien tiene que entender a las 11 de la noche siguiendo un documento | *Escalable* acá significa aguantar un acervo que nunca se borra, no repartir carga entre nodos |

## Consecuencias

**Positivas**

- Un módulo se lee entero y se entiende — que es `RNF-15` de forma directa
- La regla de dependencia la hace cumplir el compilador, no la revisión de código
- Las reglas puras se comparten **de verdad** entre oficina y campo, en el mismo paquete TypeScript
- Los repositorios con intención se leen como el dominio, no como una capa de acceso a datos

**Negativas**

- **Con verticales, el riesgo de que la misma regla se reimplemente en cada camino sube**, no baja. Es el costo real de esta decisión y hay que decirlo: `Reglas/` y las guardas **son la contramedida obligatoria, no un accesorio**
- Cuatro proyectos y sus referencias son más andamiaje que uno solo
- La agrupación de módulos del punto 2 es un juicio, y algún agrupamiento se va a revelar equivocado con el uso

**Deuda aceptada**

- **Sin las guardas, esta decisión es peor que la alternativa horizontal.** Si las guardas se posponen «para cuando haya más código», la duplicación gana — es lo que muestran las 47 de 62 pruebas de SICOV
- La agrupación de `M-08 + M-19` y de `M-13 + M-18` puede tener que separarse. Se acepta y se revisa con el uso

## El argumento honesto, y va acá

**Los verticales se eligen por cohesión —poder leer un módulo entero y entenderlo— no porque reduzcan la duplicación.** La reducen **menos** que las capas.

Y hay un caso medido en SICOV donde **la capa horizontal fue la solución**: la regla *«quien debe una liquidación no viaja»* nació dentro de un controlador, y desde ahí el otro asistente de captura no podía consumirla. Costó asignar a una persona con liquidación vencida a un destino en Madrid por **L. 73.047,96**, sin aviso. Se arregló **subiéndola** a la capa de aplicación.

Por eso `Sigti.Aplicacion` existe: es el lugar donde vive una regla que cruza módulos.

### Evidencia medida en SICOV_CORE8

- 62 pruebas de arquitectura. **47 de 62 nombran duplicación o divergencia**; solo 11 mencionan capas
- 21 clases `Reglas*`, de las cuales **12 tienen prueba propia (57 %)**
- El limpiador de HTML tenía **siete copias privadas**, y ninguna hacía lo mismo
- El descuento del último día estaba en **cuatro constantes con tres tipos distintos** (`float`, `decimal`, `double`)

> **No usar el argumento de los 775 archivos.** Una medición inicial atribuyó el tamaño de `SICOV_CORE8.AccesoDatos` a las capas horizontales. **Es falso: 592 de esos 775 son migraciones de EF generadas.** Código escrito a mano: 183. Es verificable en un minuto, y desmontarlo desacreditaría el resto de la evidencia.

## Revisión

- **El Product Owner decide adoptar la ceremonia de Clean.** Se escribe el ADR que supera a este, con el motivo
- **Una guarda empieza a fallar constantemente por razones legítimas** — señal de que la frontera está mal puesta, no de que la guarda sobre
- **La agrupación del punto 2 estorba en el uso real** — se ajusta y se registra
- **Aparece un segundo caso como el de la liquidación vencida**: una regla que nació en un módulo y otro no puede consumir. Es la señal de que `Reglas/` y `Aplicacion` no se están usando como corresponde
