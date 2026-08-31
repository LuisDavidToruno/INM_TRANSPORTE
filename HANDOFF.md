# Estado del trabajo

**Última actualización: 2026-08-31.**

Punto único de entrada para saber en qué va el proyecto. Si algo figura acá como abierto, está abierto; si se cierra, se saca de la lista el mismo día.

## Dónde está el proyecto

**Sprint 0 cerrado. Los cinco bloques están escritos, revisados y corregidos.**

**Hay stack, y hay autorización para programar.** La [designación de LOKI del 2026-08-26](docs/07-gestion/designaciones/2026-08-26-stack-y-arranque.md) fijó el stack y el PO autorizó el arranque. Eso activó la cláusula de revisión que [`ADR-000`](docs/03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md) escribió para sí mismo, y [`ADR-002`](docs/03-arquitectura/adr/ADR-002-adoptar-el-stack-tecnologico.md) lo supera formalmente.

**Ya hay código, camina, y se ve.** El backend atraviesa API → Aplicación → Dominio → SQL Server → bitácora encadenada, con **88 pruebas**. Y hay **seis pantallas de oficina conectadas a la API real**, no a datos de muestra.

El circuito que funciona hoy, de punta a punta y verificado contra SQL Server:

```
solicitar → autorizar → programar → despachar → en ruta → retornar → liquidar → CERRAR
```

Con `BD-01`, `BD-02`, `BD-03`, `BD-06` y `BD-12` evaluándose de verdad, la caducidad de la aprobación, y la anulación con motivo tipificado.

| Bloque | Qué produjo | Estado |
|---|---|---|
| 0 — Andamiaje | `CLAUDE.md`, 11 plantillas, 10 fichas normativas, 10 subagentes | ✅ Cerrado |
| 1 — Negocio | Visión, glosario, 17 actores, 14 procesos, máquina de estados, **97 reglas** | ✅ Revisado y corregido |
| 2 — Casos especiales | **28 casos** de la operación real, con su regla de resolución | ✅ Cerrado |
| 3 — Requisitos | 18 casos de uso, **150 historias** con Gherkin, 21 no funcionales, backlog | ✅ Revisado y corregido en `3f4ced4` |
| 4 — Diseño | Modelo de datos bitemporal con 43 entidades, 126 pantallas, **41 maquetadas** | ✅ Revisado y corregido en `3f4ced4` |

**406 documentos de análisis · 4,177 líneas de C# de producción y 1,965 de pruebas · 2,558 de TypeScript propio · **88 pruebas de backend y 19 del núcleo de campo** · 71 commits.** Las de C# excluyen las migraciones generadas; las de TypeScript, el sistema de diseño de LOKI. Las reglas de negocio son **103**.

Las líneas de C# excluyen las migraciones de EF, que son generadas. El TypeScript excluye el sistema de diseño de LOKI, que se copió, no se escribió.

El stack, en una línea: **.NET 10 + EF Core sobre SQL Server 2014 Standard** (restricción institucional, fuera de soporte), **React 19 + Vite** en oficina, **React Native + SQLite cifrado** en campo — del que hoy existe [el núcleo offline](campo/README.md), no la aplicación. El detalle, las funciones que 2014 no tiene y con qué se reemplazan están en la designación.

### Cómo levantarlo

```bash
dotnet run --project src/Sigti.Api --urls http://localhost:5199
```

```bash
cd oficina && npm run dev
```

La oficina necesita `oficina/.env.local` con `VITE_API=http://localhost:5199`. **Sin esa variable arranca con datos de muestra y lo dice en pantalla** — no finge estar conectada.

La base de desarrollo es `SIGTI_Desarrollo` en `localhost`, creada en `COMPATIBILITY_LEVEL 120` como exige `ADR-002`. Las migraciones se aplican con `dotnet ef database update -p src/Sigti.Datos -s src/Sigti.Datos`.

El núcleo del cliente de campo no necesita nada de lo anterior — ni base, ni API, ni Android:

```bash
cd campo && npm run verificar
```

## Lo que está abierto

### ⚠️ Smart App Control puede volver a bloquear la ejecución de .NET

En `DESKTOP-GR4SG52` (Windows 11), Smart App Control está **activo** — `VerifiedAndReputablePolicyState = 1` en `HKLM:\SYSTEM\CurrentControlSet\Control\CI\Policy`.

El 2026-08-26 bloqueó durante horas la carga de **cualquier binario .NET recién compilado** con `0x800711C7`: `dotnet test`, `dotnet run` y `dotnet ef` fallaban por igual, y solo `dotnet build` funcionaba. **Se destrabó solo**, sin cambiar ninguna configuración: SAC consulta reputación en la nube y la actualizó.

**Puede repetirse con cualquier binario nuevo.** Si vuelve a aparecer `0x800711C7`, no es el código: es SAC evaluando un ensamblado que todavía no tiene reputación. Las salidas son esperar, trabajar en otra máquina, o instalar WSL2 — **apagar SAC es irreversible** sin reinstalar Windows, así que no es la primera opción.

> ### 🔑 Antes de culpar a SAC: **baje lo que levantó**
>
> Verificado el 2026-08-27. Con `Sigti.Api` corriendo, `dotnet test` falla; **al detenerla, pasa al primer intento**. Buena parte de lo que se atribuyó a Smart App Control durante horas era esto.
>
> Son **dos errores distintos** y el mensaje los distingue:
>
> | Error | Qué es | Qué hacer |
> |---|---|---|
> | `MSB3027` — *«el archivo se ha bloqueado por: Sigti.Api (NNNN)»* | La API viva tomando los DLL | **Bajarla.** Es lo primero que hay que mirar |
> | `0x800711C7` — *«una directiva de Control de aplicaciones bloqueó este archivo»* | Smart App Control de verdad | Esperar, u otra máquina |
>
> **Regla operativa: al terminar de verificar en pantalla, detener la API antes de correr la suite.**

**Lo que se aprendió el 2026-08-29, y es lo mismo por tercera vez.** Volvió a pasar, se volvió
a diagnosticar desde cero, y se llegó a recomendar **apagar SAC** —lo que esta misma sección
desaconseja— sobre la teoría equivocada de que hacía falta reiniciar. Medido: el registro nunca
cambió, el uptime nunca se reinició, y la suite pasó con **los mismos binarios** que antes
fallaban.

> **Antes de diagnosticar `0x800711C7`, leer esta sección.** Tres sesiones perdieron horas en
> el mismo camino, y la tercera casi cuesta una decisión irreversible sobre la máquina.

**Lo que se aprendió el 2026-08-27, y acota las salidas.** El bloqueo puede durar **toda una sesión**: más de ochenta intentos, limpiando `bin`/`obj`, en Debug y en Release, y con la salida fuera del repositorio. **La ruta no importa** — SAC decide por reputación del binario, y cada compilación produce un hash nuevo.

Y no afecta solo a las pruebas: `dotnet run` de la API falla igual, con `Sigti.Datos.dll`. **Compilar sí funciona; lo que se bloquea es cargar el ensamblado.** O sea que con SAC en este estado se puede escribir y compilar, pero no ejecutar nada del proyecto — ni pruebas, ni API, ni verificación en pantalla.

SAC **no tiene lista de exclusiones**: es activo / evaluación / apagado, y de apagado no se vuelve. Con eso, las salidas reales son dos: **correrlo en la otra máquina**, o que el PO decida apagar SAC sabiendo que es de un solo sentido.

### El ciclo de vida de la misión llega al final

`T-20`, `T-21` y `T-22` están implementados y **verificados en pantalla contra la API real**. El sistema ya no se detiene en `LIQUIDADA`.

| Pieza | Estado |
|---|---|
| `OrdenDeMision.Cerrar` y `DevolverLiquidacion` | 5 pruebas · el invariante de §7.2 es **estructural**: `Cerrar` no recibe el estado destino |
| `POST /misiones/{id}/cerrar` · `/devolver-liquidacion` | Un solo endpoint para `T-21` y `T-22`, a propósito |
| [Cola de cierre](oficina/src/modulos/M13_Cierre/Cola.tsx) · [Cierre](oficina/src/modulos/M13_Cierre/Cierre.tsx) | Quinta y sexta pantalla de la oficina |
| Prueba de punta a punta | Recorre las **nueve** transiciones hasta `CERRADA` |

**Verificado en el navegador, no solo en pruebas:** se creó un expediente real, se llevó a `LIQUIDADA`, se cerró desde la pantalla, y se comprobó en la base que quedó `Cerrada` con `T-21` en el diario. Contra la API: `BD-06` devuelve **409** a quien liquidó, y un cierre con hallazgo sin justificación devuelve **409** con el criterio nombrado.

**Un defecto que solo apareció al pulsar el botón:** la pantalla seguía mostrando *«Liquidada»* y ofreciendo cerrar un expediente ya cerrado. Corregido — si el expediente dejó `LIQUIDADA`, la pantalla lo dice y ofrece volver a la cola.

**RESUELTO en parte.** ~~Los criterios `H-01` a `H-13` **no se detectan todavía**.~~ Hoy se evalúan cinco en el servidor y los otros ocho se declaran sin verificar, con lo que le falta a cada uno. Lo que sigue abierto es exactamente esa lista de ocho. Lo que decía el bloque original: `M-09`, `M-13` y `M-18` no existen, así que no hay conciliación de combustible, ni de peajes, ni cadena que evaluar — y **todo expediente cierra limpio**. La función que los calcula está marcada como provisional y devuelve lista vacía, en lugar de fingir una evaluación que no ocurrió.

### `M-01` arrancó por lo que sostiene todo lo demás: permisos por puesto

`RN-100` y `RN-101` estaban **escritas y sin ejecutar** desde que se cerraron los hallazgos. La primera ya no.

| Pieza | Qué defiende |
|---|---|
| `Organigrama` | **El permiso se concede al puesto, nunca a la persona.** Y se resuelve **a la fecha del hecho**, no a la de consulta |
| `Autoria` | **La autoría es de la persona y no se reasigna jamás** — con persona *y* puesto congelados |

**Por qué esto no es burocracia de modelado.** `NRM-09` `[V]`: la rotación en el sector público es alta, y Honduras cambió de gobierno en enero de 2026. Con el permiso colgando de la persona, cada rotación obliga a reconstruir a mano quién puede hacer qué — y lo que ocurre en la práctica es que **se copian los permisos del saliente al entrante** *«para que pueda trabajar»*, arrastrando toda la acumulación indebida que el saliente había juntado. La segregación de `RN-01` se pierde sin que nadie decida perderla.

**La mitad que se olvida** es el recíproco. El auditor no pregunta *«¿quién firmó?»*: pregunta **«¿quién autorizó esto y con qué competencia?»**. Guardar solo la persona deja el acto sin fundamento; solo el puesto, sin responsable. Por eso van los dos, y **congelados** — si el puesto fuera una referencia viva, una reestructuración reescribiría la historia sin que nadie lo pidiera.

**Ocho pruebas**, y dos merecen mención: que un acto de febrero se juzgue con la ocupación de febrero —sin eso, reevaluar un expediente viejo diría que quien lo autorizó no tenía competencia, y quedaría indefendible por un artefacto del sistema— y que la coocupación durante un traspaso esté permitida, porque el traspaso real dura días.

**La persistencia ya está**, y es un **espejo, no un maestro**. La tabla `organizacion.AsignacionDePuestoEspejo` existe y está aplicada; `ConsultaDelOrganigrama` arma el organigrama desde ella **completo**, sin filtrar por «vigentes hoy» —porque `RN-100` resuelve a la fecha del hecho, y filtrar en SQL impediría reevaluar un expediente de febrero.

**No hay endpoint de escritura, y es la decisión, no un pendiente.** `DP-001`: la estructura de puestos es de ARGOS y Talento Humano. `RN-48`: los datos de otro dueño se guardan marcados como espejo y **ninguna pantalla de SIGTI debe permitir editarlos**. Quien necesite corregir un puesto, lo corrige en ARGOS.

**Un espejo envejece, y eso se mide.** La fila lleva `ConfirmadoAl` —que un maestro no necesitaría— y `GET /organigrama/antiguedad` lo expone. **Devuelve nulo cuando nunca se confirmó**, deliberadamente distinto de cero días: una integración que jamás corrió y una que corrió hace un minuto no se pueden mostrar igual. Es la peor forma de fallar, en silencio y con buena cara.

**Lo que falta de `M-01`:** el **alcance de datos por dependencia**, `RN-101` (cierre de asignación con custodias activas) y **la integración que puebla el espejo** — hoy la tabla se llena sembrando, y el circuito real contra ARGOS no existe.

### `M-03` y `M-04` — la flota salió del código y `BD-03` empezó a bloquear

Mientras la flota vivía en un catálogo en código, **`BD-03` no podía bloquear**: la documentación provisional devolvía vencimientos de 2030 para todo, y el propio código lo declaraba en un comentario para no fingir que había verificado algo. `RN-103` estaba escrita y no se ejecutaba.

| Pieza | Qué |
|---|---|
| `FilaDeVehiculo` | Ficha técnica y documentación con **vencimientos reales**, en `flota.Vehiculo` |
| `ConsultaDeFlota` | Reemplaza al catálogo en la parte de vehículos |
| `GET /flota` | Sale de la base |
| `ResultadoDeDocumentacion.VenceElQueBloquea` | El mensaje de `BD-03` ahora dice **cuándo vence** |

**Verificado contra la API real:** con matrícula al 2027 programa (**200**); con matrícula al 2026-03-21 y ventana hasta el 23, devuelve **409** con `BD-03` y la fecha. Eso era imposible ayer.

**Un detalle que el mensaje cambió.** `BD-02` ya decía el vencimiento de la licencia y `BD-03` no decía nada — *«documentación vencida»* a secas. Con la fecha, quien programa sabe si le alcanza con esperar o tiene que cambiar de vehículo; sin ella tenía que ir a buscarla al expediente.

**La placa no lleva índice único**, a propósito: *«sin placa»* es estado válido por el desabastecimiento nacional, y un índice único sobre nulos rompería la flota real (`RN-15`). Lo único con índice único son las **siglas**, que son la identidad estable del bien.

**Lo que falta:** el **alta de vehículos** —hoy la flota se siembra solo en desarrollo, y esa siembra va en el arranque y no en una migración a propósito: una migración con datos los metería también en la instancia de la institución—. Y `M-04` completo: alertas de vencimiento (`RN-17`), renovaciones y adjuntos del documento.

### El cliente de campo arrancó por el núcleo, no por la pantalla

Primera línea de código de [`campo/`](campo/README.md). **No es la aplicación Android** — es la lógica que `RNF-03` no perdona, en TypeScript puro, con **19 pruebas** que corren en cualquier máquina con Node.

| Pieza | Qué defiende |
|---|---|
| [`DiarioLocal`](campo/nucleo/DiarioLocal.ts) | `P-1` — el dispositivo manda **transiciones**, nunca «el estado». Y lo que el servidor no acusó **sigue pendiente**: la sincronización se corta a la mitad más veces de las que termina |
| [`Conciliacion`](campo/nucleo/Conciliacion.ts) | [`RN-45`](docs/01-negocio/reglas/RN-45-cero-sobrescritura-silenciosa.md) — **cero sobrescritura silenciosa**. Las dos versiones se conservan y van a cola humana |
| [`SubrangoDeFolios`](campo/nucleo/Folios.ts) | [`RN-44`](docs/01-negocio/reglas/RN-44-identificadores-y-folios-en-el-cliente.md) y `RNF-21` — cierra `HB34-52`, que era **crítico**: dos dispositivos de Tocoa sin red tomaban el mismo folio |
| [`AlmacenSqlite`](campo/nucleo/AlmacenSqlite.ts) | [`ADR-003`](docs/03-arquitectura/adr/ADR-003-cliente-de-campo-instalado.md) — **fuente de verdad local, no caché**: lo capturado sobrevive a que Android mate el proceso, que en gama baja ocurre sin avisar |
| [`ColaDeAdjuntos`](campo/nucleo/ColaDeAdjuntos.ts) | [`RN-43`](docs/01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md) y [`ADR-004`](docs/03-arquitectura/adr/ADR-004-adjuntos-fuera-de-la-base.md) — los adjuntos van en **su propia cola** y no retienen al hecho: una foto pesa dos órdenes de magnitud más que la transición que respalda |

**Por qué se separó así, y no es solo circunstancia.** Esta máquina no tiene Android SDK, ni emulador, ni Java: una app React Native no se puede compilar ni ejecutar aquí, y escribirla entera habría producido cientos de líneas que nadie vio funcionar. Pero además **la regla de qué se captura, qué queda pendiente y qué es conflicto es la misma con o sin disco** — separada del almacenamiento se prueba en 70 ms en vez de en un dispositivo.

```bash
cd campo && npm run verificar
```

**Lo que falta**, con lo que más pesa arriba: la **aplicación React Native** —necesita máquina con SDK—, el **cifrado en reposo** —el esquema y la durabilidad están probados; que el archivo quede ilegible sin la clave, no—. Los **adjuntos diferidos** ya tienen su cola; lo que falta es **subirlos**, y un almacén de archivos en el servidor donde recibirlos (`ADR-004`). **El endpoint de sincronización ya existe** — ver abajo.

### `POST /sincronizacion` — el servidor ya recibe lo que el dispositivo capturó sin red

El circuito se cerró: hay un dispositivo que sabe qué mandar y un servidor que sabe recibirlo, **verificado contra la API real**.

| Envío | Respuesta | Diario |
|---|---|---|
| Primero | `aplicadas: [CAP001]` | Una `T-14` |
| **Reenvío** — el dispositivo no recibió el acuse | `yaConocidas: [CAP001]`, acusada igual | **Sigue habiendo una sola** |

**La idempotencia la garantiza la base, no una comprobación.** `IdDeCaptura` —el ULID que generó el dispositivo, `ADR-005`— tiene índice único con filtro. Un `SELECT` previo parece más limpio y es una condición de carrera: dos lotes del mismo dispositivo en vuelo a la vez pasarían los dos la comprobación. El índice no se equivoca, y **no se olvida al escribir el próximo endpoint**.

**El lote no es atómico, a propósito.** Se comprobó con un lote de dos donde el primero apuntaba a un expediente inexistente: el primero se rechazó con motivo legible, **el segundo entró**, y el expediente avanzó a `RETORNADA`. El dispositivo lleva siete días de trabajo encima; perderlo todo por un expediente que falta sería el fallo que este endpoint existe para evitar.

**Lo que el endpoint todavía no acepta, y lo dice:** solo `T-14` y `T-18`. La bitácora de paradas y eventos necesita `M-08`, que no está construido — aceptarla ahora sería fingir que existe.

### `POST /adjuntos` — el binario al sistema de archivos, el rastro a la base

`ADR-004` implementado y **verificado contra la API real**. La aritmética que lo decidió: ≈ 8 GB anuales de datos relacionales contra ≈ 30 GB de adjuntos. Meterlos en la base cuadruplicaría el respaldo y sacaría la restauración de las 2 h que `RNF-09` exige de personal no especialista.

| Caso | Respuesta |
|---|---|
| Subida correcta | **201** con la ruta relativa, y el archivo en `2026/03/` |
| Reenvío | **200** con `yaConocido` — el dispositivo lo saca de su cola igual |
| **Llegó truncado** | **409 con los dos hashes**, y **no se registró nada** |

**El hash se verifica, no solo se guarda.** Guardarlo sin comprobarlo lo volvería decorativo: un archivo truncado por la red de un retén quedaría registrado como íntegro y el defecto aparecería meses después, al armar el paquete de evidencia — cuando ya no se puede volver a tomar la foto.

**Va como formulario, no como JSON.** En base64 el binario crece un 33 %, y sobre la red de un retén ese tercio se paga en tiempo y en batería.

**El archivo se organiza por fecha del hecho, no de subida** (`P-4`). Un adjunto capturado el 20 de marzo y subido el 27 —siete días sin red— pertenece a marzo; ordenarlo por subida dispersaría una misma misión entre dos carpetas y el respaldo por período dejaría de coincidir con el expediente.

**La ruta que se guarda es relativa**, nunca absoluta: la institución puede mover el almacén a un disco más barato o de solo lectura sin tocar una fila.

**Lo que falta, y `ADR-004` lo pide explícitamente:** el **respaldo de dos piezas** —base y almacén, consistentes entre sí—, que el ADR exige *«desde el principio, no adaptado después»*. No está escrito, y es lo que `DESPLIEGUE.md` necesita para poder escribirse.

### `BD-12` cerrado en las tres capas, y verificado

Las restricciones médicas salieron de `BD-02` a un bloqueo propio, con el efecto decidido por el catálogo. **Suite en 66/66 el 2026-08-27**, incluidas las tres pruebas de `CatalogoProvisionalDeRestricciones` que en su momento no pudieron correr.

| Capa | Qué quedó |
|---|---|
| [`orden-de-mision.md`](docs/03-arquitectura/estados/orden-de-mision.md) | `BD-12` definido y listado en `T-08`, `T-12` y `T-17`; `BD-02` con sus dos condiciones |
| [`ReglasDeHabilitacion`](src/Sigti.Dominio/M05_Motoristas/ReglasDeHabilitacion.cs) | Sin la condición 3, sin el parámetro, sin el valor del enum |
| [`ReglasDeRestriccionMedica`](src/Sigti.Dominio/M05_Motoristas/ReglasDeRestriccionMedica.cs) | La evaluación nueva, con 3 pruebas |
| [`CatalogoProvisionalDeRestricciones`](src/Sigti.Aplicacion/M05_Motoristas/CatalogoProvisionalDeRestricciones.cs) | ⚠️ **Provisional** — insumo #42. Con 3 pruebas |
| [`EvaluacionDeAsignacion`](src/Sigti.Aplicacion/M07_ProgramacionYDespacho/EvaluacionDeAsignacion.cs) | `BD-12` aparte; **solo el bloqueo impide programar** |
| [`Asignacion.tsx`](oficina/src/modulos/M07_Programacion/Asignacion.tsx) | La advertencia con **acuse obligatorio**: el botón queda inerte hasta marcarlo (`RN-11`) |
| [`RechazoPorLicencia.tsx`](oficina/src/modulos/M07_Programacion/RechazoPorLicencia.tsx) | Distingue el bloqueo de `BD-12` del de `BD-02` |

**Lo único que falta es verlo en pantalla.** La advertencia y el acuse pasan las pruebas del dominio y compilan, pero **nadie los ha abierto en el navegador** — cuando SAC lo permitió, la API no llegó a levantarse. Antes de darlo por bueno de cara al usuario: levantar la API, elegir un conductor con restricción sin clasificar, y comprobar que la advertencia aparece y que el botón no se habilita sin marcar el acuse.

**Y una decisión que conviene mirar.** El catálogo provisional tipifica **una sola** restricción como bloqueante — *«conducción diurna únicamente»*, la única que `RN-11` sostiene contrastable por sistema. Todo lo demás advierte. Mientras el insumo #42 siga abierto ése va a ser el caso mayoritario: **hoy el sistema casi no bloquea por restricción médica**. Si la institución espera lo contrario, el arreglo no es tocar código — es conseguir el catálogo de la DNVT.

### La flota se reserva de verdad, y hay línea de tiempo por carriles

**`T-08` decía que reservaba y no reservaba nada.** La máquina de estados la describe como
*«aquí se reserva vehículo y motorista»* desde que se escribió; la identidad del vehículo
quedaba **sólo dentro del texto de evidencia, en prosa**. La misión se programaba, el
diario quedaba perfecto, y el vehículo se seguía ofreciendo libre. Duró así porque el
síntoma —una pantalla que no muestra ocupación— es indistinguible de una pantalla que no
la tiene.

**La reserva vive en el diario, no en una tabla de reservas.** `P-1`: el estado es la
proyección del diario. Una tabla aparte sería esa segunda copia con su forma propia de
desincronizarse — una misión anulada cuya reserva sobrevive deja un **vehículo fantasma
ocupado** y el sistema reporta falta de flota que no existe. Puesto en la transición,
**liberar es no volver a tomar**.

| Pieza | Qué |
|---|---|
| [`RecursosTomados`](src/Sigti.Dominio/M07_ProgramacionYDespacho/RecursosTomados.cs) | Lo que `T-08` toma. Identificadores, no la ficha: la ficha cambia y el diario es inmutable |
| [`ConsultaDeOcupacion`](src/Sigti.Aplicacion/M07_ProgramacionYDespacho/ConsultaDeOcupacion.cs) | La proyección. Ocupa por **lista blanca** —`PROGRAMADA`, `DESPACHADA`, `EN_RUTA`— y no por descarte |
| `GET /flota/ocupacion` | Recorta la ventana en SQL; devuelve las fechas **reales**, sin truncar |
| [`LineaDeCarriles`](oficina/src/ui/LineaDeCarriles.tsx) | La primitiva de ~24 pantallas. Una sola sirve ocupación, vigencias e hitos |
| [`Asignacion.tsx`](oficina/src/modulos/M07_Programacion/Asignacion.tsx) | `PT-026` con el cronograma **arriba** de la lista: *«¿cuál está libre?»* precede a *«¿este habilita?»* |

**Dos cosas salieron de mirar la pantalla, no de razonar sobre el código.** La primera: el
rótulo del lector de pantalla anunciaba «misión Correctivo» para un bloqueo de taller —
falso, y además sugiere que se puede reprogramar. La segunda, más grave: **dos barras
encimadas se dibujaban una sobre otra y sólo se veía la última**. El dibujo que existe
para revelar el solape lo estaba escondiendo. Ahora se apilan en subfilas, y dos barras
que **se tocan** cuentan como solape: el vehículo no puede estar volviendo de Danlí y
saliendo a Juticalpa el mismo día.

### `PT-038` — el tablero del despachador, que el dictamen marcó como el peor error

Estaba declarada **«completa»** en el inventario y maquetada como lista. Es la raíz de
`ACT-05` en el mapa de navegación — lo primero que abre el Encargado de Despacho — y **no
existía en código**.

**Cuatro listas, no una tabla ordenable.** Qué **sale** hoy (hay que entregar vehículo,
documentos y fondo), qué **vuelve** hoy (hay que recibirlo), qué está **afuera** (no se
puede contar con esos vehículos) y qué **debía haber vuelto y no volvió**. Cuatro acciones
con cuatro urgencias; una tabla con columna de estado obliga a filtrar mentalmente cada vez,
y el despachador la abre veinte veces al día.

**La cuarta es la que justifica la pantalla.** Una lista ordenada por fecha **no muestra un
retorno vencido**: no aparece arriba, aparece en el pasado, donde nadie mira.

| Pieza | Qué |
|---|---|
| [`ConsultaDelDiaDeDespacho`](src/Sigti.Aplicacion/M07_ProgramacionYDespacho/ConsultaDelDiaDeDespacho.cs) | Clasifica en las cuatro listas. Una misión atrasada se cuenta **una vez** |
| `GET /despacho/dia?fecha=` | La fecha se recibe. Una fecha mal formada **no es «hoy»** |
| [`Tablero.tsx`](oficina/src/modulos/M07_Programacion/Tablero.tsx) | `PT-038`, con el cronograma de la semana debajo |

**Detalles que decidieron el comportamiento:** una misión que debía salir ayer y sigue
`PROGRAMADA` **no salió**, así que sigue en «sale hoy» —con `==` desaparecería del tablero
justo cuando hay que ir a buscarla—. Y una fecha mal formada devuelve `400`: caer a hoy en
silencio haría que un enlace roto mostrara un tablero plausible del día equivocado, sobre el
que el despachador actuaría.

**⚠️ El eje de horas del dictamen NO se construyó, y no se finge.** El dictamen pide la
ráfaga de las 5:30 con ocho salidas encimadas. **La ventana de la misión es sólo fecha**: la
solicitud no declara hora de salida, así que no hay dato con el que ordenar el día por
dentro. Un eje de horas dibujado sobre medianoches sería un gráfico que miente con
precisión. Lo que sí se dibuja es el **cronograma de la semana**, que contesta la otra mitad
—qué se traslapa con qué— y para el que el dato existe.

### El estado operativo del vehículo, y con él `BD-07`

Lo venía citando como faltante en `T-11`, `T-15`, `T-16` y `BD-07`. §10.2 lo tiene
**completamente especificado** —ocho estados, con quién fija cada uno— y no existía.

**Es un diario, no una columna.** La pregunta de la auditoría no es *«¿en qué estado está?»*
sino *«¿por qué no estuvo disponible en abril, y quién lo decidió?»*, y un `estado_actual` la
borra cada vez que cambia. Además §10.2 exige **causa tipificada** para `NO_DISPONIBLE` y
**acta** para el préstamo y los dos terminales: eso no cabe en un enum guardado en el
vehículo.

**Las transiciones de la misión lo mueven, dentro de la misma transacción.** `T-08`/`T-10` →
`ASIGNADO`, `T-14` → `EN_MISION`, `T-11`/`T-13`/`T-18` → `DISPONIBLE`. Va en la capa de
aplicación porque son **dos agregados**, y dentro de la transacción por lo mismo que el
asiento de bitácora: una caída entre las dos dejaría un vehículo asignado a una misión que no
se guardó.

**Nulo no es disponible.** §10.2 lista *«alta reciente sin habilitar»* entre las causas de
`NO_DISPONIBLE`: dar por disponible el nulo haría que **el alta habilitara sola**. No bloquea
—hay expedientes anteriores— pero **deja dicho que no se verificó**.

### 🔎 §10.2 dice que sólo se programa desde `DISPONIBLE`, y eso rompe la operación

**Lo destapó la propia coordinación al empezar a funcionar**: en cuanto `T-08` empezó a poner
el vehículo en `ASIGNADO`, una prueba de `BD-11` cambió de bloqueo — ahora fallaba por
`BD-07`.

Leída al pie de la letra, la frase impide programar una misión de marzo para un vehículo
comprometido a una de diciembre: queda `ASIGNADO` desde hoy. **Todo el sistema está
construido sobre lo contrario** — `EF-01` reserva por ventana, `BD-11` compara ventanas, y el
cronograma de flota dibuja **varias barras por carril** justamente porque un vehículo tiene
varias misiones a lo largo del mes.

**Resolución adoptada:** `BD-07` bloquea sólo lo que vuelve al vehículo **inutilizable** —
`EN_TALLER`, `NO_DISPONIBLE`, `PRESTADO` y los dos terminales—. `ASIGNADO` y `EN_MISION`
pasan, y el solape lo decide `BD-11`, que además **nombra al titular**. Si `ASIGNADO`
bloqueara, taparía a `BD-11` con un mensaje mucho peor: *«está asignado»* en vez de *«lo tiene
la misión X de la delegación Y, del 20 al 23»*.

**Queda como decisión del PO**: o §10.2 se corrige, o hay que explicar cómo se programan dos
misiones de un mismo vehículo en meses distintos. No se resolvió en silencio.

**⚠️ La otra mitad de `BD-07` no se evalúa**: la compatibilidad entre lo que se mueve y el
tipo de vehículo necesita la matriz de `M-02`, que no existe, y el objeto del traslado es
texto libre — no hay nada estructurado contra lo que contrastarla.

**El estado ya se declara.** `POST /flota/{id}/estado`, con dos reglas que tienen consecuencia
patrimonial:

- **`ASIGNADO` y `EN_MISION` no se declaran a mano.** §10.2: *«permitir fijarlos a mano abre la
  puerta a un vehículo "en misión" sin misión que lo respalde»*.
- **Un vehículo con misiones abiertas no se da de baja.** Un expediente vivo colgando de un
  bien que ya no figura en el registro es un hallazgo que nadie puede explicar después.
- **De un terminal no se sale**, y el mensaje distingue cuál: revertir un descargo es un
  trámite del registro de bienes; una devolución de comodato ni siquiera es nuestra para
  revertirla.

**Esto es lo que vuelve real a `BD-07`.** Antes el estado sólo se movía solo y ningún vehículo
llegaba nunca a `EN_TALLER`: el bloqueo existía **sin poder alcanzar su condición**. Verificado
en pantalla — un vehículo en taller y uno prestado se pintan apagados en el cronograma
(opacidad medida, 0.55 contra 1) y el detalle del carril dice el estado.

**El historial va entero al cliente**, porque la pregunta es *«¿por qué no estuvo disponible en
abril?»* — y cada asiento dice si lo declaró una persona o lo fijó el sistema. Sin esa marca,
la afirmación de §10.2 sobre quién fija qué **no se puede auditar, sólo creer**.

### `RN-83` — todo ingreso de combustible, venga de donde venga

**RESUELTA.** El abastecimiento es ahora una entidad propia, colgada del **vehículo** —`RN-83`
aplica «a todo vehículo de la flota, en misión o fuera de ella»— con su fuente declarada.

**El hueco que cierra, con números.** Un vehículo recorre 900 km. El vale registra 20 galones y
los otros 40 salieron del tanque de la sede sin pasar por ningún folio. Con sólo los del fondo,
el rendimiento da **45 km/gal**: imposible, y `RN-30` lo marca como probable despacho no
registrado. **Y tenía razón** — lo que faltaba era poder registrarlo. Verificado punta a punta y
por mutación: contar sólo los del vale rompe la prueba.

| Fuente | Cuadre del fondo | Denominador de `RN-30` |
|---|---|---|
| `FondoDeLaMision` | sí | sí |
| `TanqueInstitucional` · `OtraDependencia` · `Donacion` · `TerceroEnApoyo` | no | sí |
| `PeculioDelServidor` | no, y **genera reintegro** (`RN-86`) | sí |

**El galón no se cuenta dos veces.** El asiento `V-04` del vale y el abastecimiento son *el
mismo hecho visto desde dos lados*: van en la misma transacción, y un índice único sobre la
referencia al asiento lo impone en la base. Dos filas apuntando al mismo `V-04` inflarían el
denominador y producirían una desviación inventada por el propio sistema.

**La composición por fuente se expone** — `RN-30` punto 4. Sin ella, cuarenta galones del tanque
de la sede y cuarenta comprados con el vale se leen igual. Va como dato, no sólo dentro de la
evidencia, y **también cuando el dictamen es `NoEvaluable`**: que no haya contra qué comparar no
borra de dónde salió cada galón.

**El enum no es un catálogo, y se dice por qué.** `RN-83` lo llama configurable, pero el
*comportamiento* cambia por fuente —cuadre, reintegro, denominador— y un valor cargado por
pantalla no sabría a cuál de los tres grupos pertenece. Lo configurable es cuáles usa la
institución; añadir una séptima es un cambio de código.

**Y a quien no genera factura no se le pide causa.** Una donación y el despacho del tanque de la
sede no traen papel: exigirles la causa de `RN-85` obligaría a escribir «no aplica» en cada
registro, y una casilla que siempre dice lo mismo deja de leerse — con ella se pierde la vez que
sí significaba algo.

---

### El nivel de tanque, y un reparo que deja de marcarse a mano

La otra mitad del enunciado de `RN-83`: **el nivel a la salida y al retorno es dato obligatorio
de bitácora**. Viaja en `T-14` y `T-18` como dato, con **su escala** — porque un octavo de tanque
no es lo mismo en un pickup que en un bus, y dos lecturas de escalas distintas no se restan.

Con eso, el reparo `NivelDeTanqueDispar` de `RN-30` **se calcula**. Antes quien conciliaba lo
marcaba a mano porque el sistema no tenía el dato, y una casilla que alguien olvida marcar deja
pasar un cálculo que no significa nada. Lo declarado sigue mandando: si el conciliador dice que
el tanque estaba dispar, lo estaba — él lo vio y el sistema sólo tiene dos números.

⚠️ **El umbral de «muy distinto» es una decisión de esta implementación, no de la norma.**
`RN-83` no fija cuánto y la institución no lo ha declarado. Un cuarto es lo que una aguja permite
leer sin discutir; queda `[C]`.

**Nulo es «no consignado», no cero** (`RN-80`). Sin una de las dos lecturas no hay diferencia que
medir y el reparo no se activa: estimarlo produciría un remanente inventado que después nadie
podría distinguir de uno medido. Y escalas distintas devuelven **nulo, no falso** — «no se puede
comparar» y «no hay diferencia» son cosas opuestas.

## §7.2 — el cierre proponía lo que le dijeran

**1405 pruebas en verde.**

### ⚠️ El hallazgo

§7.2 dice que **el sistema propone** la clasificación de cierre, y la precondición de `T-21`
es *«no se cumple ninguno de los criterios `H-nn`»*.

La detección vivía **en el navegador** y evaluaba **uno** de los trece criterios. El endpoint
recibía la lista de criterios **en el cuerpo de la petición**: quien llamara con la lista vacía
cerraba `CERRADA`, y el asiento decía que cerró limpio.

**Una precondición que declara el propio llamador no es una precondición** — es un comentario.
Y no hacía falta mala fe: la pantalla no sabía mirar doce de los trece, así que la lista salía
casi siempre vacía por ignorancia, no por elección.

### Cinco criterios evaluados, ocho declarados

| | |
|---|---|
| `H-01` desviación de consumo | ✅ sale de `RN-30`, que ya dictamina |
| `H-03` peaje incompatible con la ruta | ✅ sale de `RN-37` |
| `H-04` fondo entregado sin devolver | ✅ M-09, `EntregadasSinDevolver` |
| `H-05` circuló en franja inhábil sin permiso | ✅ calendario × permisos, contra la ventana real |
| `H-06` incidente sin desenlace | ✅ M-12 |
| `H-02` `H-07` a `H-13` | ⛔ **no verificado, con lo que le falta a cada uno nombrado** |

**`H-05` no es `BD-04` otra vez.** `BD-04` mira al despachar contra la ventana *solicitada*;
esto mira al conciliar contra lo que *efectivamente pasó* — una prórroga que metió el sábado,
un relevo que invalidó el permiso, o una salida que entró por sincronización con el bloqueo
evaluado en el dispositivo.

### La distinción que hace que esto valga algo

⚠️ **«No se cumple» y «nadie lo miró» no son lo mismo.** Con dos valores se vuelven
indistinguibles y el expediente cierra `CERRADA` afirmando trece verificaciones de las que hizo
cinco — que es peor que no verificar nada, porque el auditor lee un expediente revisado.

Tres valores, entonces, y dos consecuencias:

- **Lo no verificado NO produce hallazgo.** Marcar el expediente por lo que el sistema todavía
  no sabe mirar acusaría a la institución de una conducta que nadie constató.
- **Lo no verificado se muestra siempre**, sobre todo cuando cierra limpio, y **con lo que le
  falta**. Un «no verificado» sin motivo es un hueco que nadie va a poder cerrar porque nadie
  va a saber qué le falta. Va también en la respuesta del cierre: es lo que quien cierra acaba
  de firmar.

### El defecto que sólo se vio abriendo la pantalla

`H-03` salió declarado *«no verificado: todavía no hay quien juzgue»* — y **M-18 ya juzga**. El
mismo expediente mostraba arriba *«peaje fuera de la ruta autorizada»* y, dos paneles más abajo,
que nadie podía juzgar eso. **Dos afirmaciones contradictorias en la misma pantalla, y la
segunda la había escrito yo.**

Cableado a `RN-37`, el expediente `PROV-78BMFT` de la base de desarrollo **pasó de «cierra
limpio» a «cierra con hallazgo»**. El hallazgo estaba detectado desde hacía tiempo y no llegaba
al acto que importa.

Los tres estados salen del propio dictamen, que ya los distinguía: *«sin hallazgos no es lo
mismo que coherente; un dictamen que no pudo mirar nada no es conformidad, es silencio»*.

### `HB72-01` — §7.1 quedó atrás de su propio catálogo

La precondición de `T-21` dice *«no se cumple ninguno de los criterios **`H-01` a `H-08`**»*, y
lo mismo los efectos de `T-19` y el resumen de §7.2. Pero **el catálogo tiene trece**: `H-09` a
`H-13` se agregaron al corregir `HB1-15`, y esa corrección no actualizó las tres menciones.

No es cosmético. `H-09` existe porque [`RN-08`](docs/01-negocio/reglas/RN-08-cadena-de-trazabilidad-para-cierre.md)
dice textual que con un eslabón faltante *«no debe permitir `CERRADA`, pero sí
`CERRADA_CON_HALLAZGO`»*. Leída al pie de la letra, la precondición de `T-21` **deja cerrar
limpio un expediente con la cadena rota** — exactamente lo que `RN-08` prohíbe.

El código evalúa **los trece**, que es lo que el catálogo y las cinco reglas de origen exigen.
**La máquina de estados es la autoridad y le toca alinear las tres menciones**: levantado ahí,
no resuelto acá.

---

## `PT-022` — el jueves santo a las cinco de la tarde

**1384 pruebas en verde.**

`HU-020` cerrada. Era la última pantalla del hilo del permiso que faltaba, y la que
figuraba abierta en cuatro bloques seguidos.

### Por qué no es una comodidad

El Tribunal Superior de Cuentas hace operativos de fiscalización vehicular **específicamente
en Semana Santa**. Es el pico anual de riesgo, y es **predecible** — lo que lo vuelve el caso
más fácil de resolver bien y el más caro de resolver mal.

Un flujo que le exige a la máxima autoridad abrir veinte expedientes uno por uno a las cinco
de la tarde del jueves santo produce una de dos cosas: **permisos que no se firman y misiones
que salen sin amparo, o la clave prestada a un asistente**. La segunda es la que el sistema
entero está diseñado para evitar, y por eso el lote **rechaza de una vez** a quien no es la
máxima autoridad en lugar de dejar veinte intentos idénticos en la bitácora.

### Y por qué el reporte tiene tres listas

Un reporte que liste sólo los que circulan **deja al resto invisible**, y un vehículo del que
nadie confirmó dónde está es exactamente lo que un operativo encuentra. Las tres listas
—circulan, resguardados, exceptuados— **suman la flota entera y son excluyentes**: ésa es la
propiedad que hace útil el reporte, no un detalle de presentación, y tiene su comprobación
propia que sale en la respuesta.

Tres decisiones que sostienen eso:

- **Los incompletos no detienen a los completos.** Se firman los que están y **se nombra el
  que no, con folio y motivo**. «4 de 5 firmados» sin decir cuál faltó deja a quien firma
  buscando el que quedó, que es el que va a salir sin amparo.
- **Los no confirmados van arriba.** El orden es la mitad del valor: una lista alfabética de
  dieciocho obliga a buscar los tres que importan, y el jueves santo nadie los busca.
- **Sin evidencia fechada no se confirma un resguardo.** Misma disciplina de `RN-18`: sin ella
  lo único que queda registrado es que alguien dijo que el vehículo estaba ahí.

### El defecto que sólo apareció contra la base real

Las pruebas pasaban y el reporte cuadraba. Contra la base de desarrollo aparecieron dos
vehículos **dados de baja y retirados de flota** pidiendo confirmación de resguardo.

Pedirle a alguien que vaya a mirar dónde quedó un bien que ya se descargó del registro es
mandarlo a una tarea que puede ser imposible. **El daño no es la tarea de más**: cada uno de
esos infla «sin confirmar», y en una institución con años de historia son decenas — los tres
que de verdad nadie fue a mirar quedan enterrados entre ellos. Es el mismo defecto que el
orden de la lista existe para evitar, entrando por la otra puerta.

Quedan fuera los **dos estados terminales de §10.2** y nada más. `PRESTADO` sigue siendo bien
nuestro y devenga responsabilidad patrimonial; `EN_TALLER` es un lugar, y un vehículo que
nadie ubica no deja de estar perdido porque haya una orden de trabajo abierta. Y **nulo entra**:
«nunca se declaró estado» no es «no es flota».

⚠️ La mitad que se rompe sola es la otra: **listar con un criterio y contar con otro** haría
que la comprobación de que el reporte cuadra fallara siempre, o —peor— que pasara escondiendo
un vehículo. Por eso el conteo de la flota vive en el servicio junto al criterio, no en la
ruta, y hay una prueba de punta a punta que lo fija.

### El otro: «firmado» y «no se puede firmar» decían lo mismo

Un permiso ya firmado **seguía contando en «permisos que puede firmar hoy»**. La cifra no
bajaba al firmar, y la sesión de firma no termina nunca: quien firma vuelve a abrirla creyendo
que quedaron pendientes y encuentra los mismos.

La causa es que **los dos hechos entraban por el mismo campo**. `PorQueNoSeFirma` rechaza el
firmado por su cuenta —«ya está firmado, una segunda firma no agrega amparo»— y eso es cierto,
pero **decir eso ahí pinta de rojo, con mensaje de bloqueo, justamente lo que ya se resolvió**.
Son dos cosas opuestas: una es el problema y la otra es el resultado.

Quedaron separadas. `Firmado` es bandera propia; `PorQueNoSeFirma` es sólo lo accionable —y en
un permiso firmado, eso es «ya no cubre», que se arregla reemitiendo, no volviendo a firmar.

Los dos defectos son el mismo patrón: **un campo contestando dos preguntas distintas**. Ninguno
lo veía una prueba de dominio; el primero salió contra la base real y el segundo leyendo el
camino que la pantalla iba a recorrer.

---

## `INV-19` — el permiso firmado y el papel que quedó en el escritorio

**1355 pruebas en verde.**

### ⚠️ El hallazgo

El invariante de `DESPACHADA` dice: *«existe el permiso de la máxima autoridad **y su**
**salvoconducto impreso** — `BD-04`»*.

`BD-04` comprobaba el permiso **y nada más**. Una misión podía salir en franja inhábil con la
firma registrada en el sistema y **sin papel en la guantera** — que es lo único que un agente
en carretera puede pedir.

### Y por eso el acuse no es una formalidad

`RN-65` pide *«emitir, imprimir y **entregar contra acuse**»*, y sólo lo primero existía.
Emitir e imprimir son actos de oficina: entre la impresora y el vehículo el papel se pierde —
queda en el escritorio, se despacha antes de que salga la impresión, o se entrega a quien
pasaba por ahí.

**El acuse separa «el sistema emitió el papel» de «el motorista lo tiene»**, y en un operativo
sólo la segunda importa. `§10.2` describe `DESPACHADA` diciendo que *«el motorista ya tiene en
la mano … los documentos del vehículo … Firmó la recepción»*: esto es esa firma.

Tres cosas que el acuse rechaza:

- **No se acusa lo que no se emitió.** Una firma sobre un papel inexistente deja constancia de
  una entrega que no ocurrió — peor que no tener constancia.
- **No lo acusa alguien distinto del motorista de la orden.** El documento es nominativo: a
  otro nombre no prueba nada, y el papel viaja igual sin que conste quién lo lleva.
- **No se acusa dos veces.** Dejaría dos personas declarando haberlo recibido, y ninguna se
  podría sostener.

Y la comparación es **contra el motorista de la reserva**, no contra el del propio acuse: si se
comparara consigo mismo el bloqueo no dispararía nunca, que es el defecto que ya costó dos
veces en `RN-32`.

### `HB19-01` — cuatro invariantes de `DESPACHADA` sin bloqueo

`INV-19` quedó implementado. **Los otros cuatro no tienen quien los sostenga:**

| | |
|---|---|
| `INV-17` acta de entrega del vehículo firmada | ⛔ el acta existe desde `RN-22` y **`T-12` no la exige** |
| `INV-18` folio consumido y hash del impreso | ⛔ |
| `INV-20` fondo de combustible `ENTREGADA` con firma | ⛔ |
| `INV-22` un dispositivo portador designado | ⛔ |
| `INV-21` paquete normativo congelado — `EF-03` | ✅ nombra su bloqueo |
| `INV-23` segregación al despachar — `BD-06` | ✅ |

**No les inventé un `BD-nn` desde el código.** A diferencia de `INV-19`, que nombra el bloqueo
que lo hace cumplir, éstos no nombran ninguno — y los identificadores de bloqueo los asigna la
máquina de estados, que es la autoridad. Lo que falta decidir es si cada uno es bloqueo duro o
advertencia registrada, y con qué número. Levantado ahí, no resuelto acá.

---

## Los adjuntos entraban y no salían nunca

**1348 pruebas en verde.**

### ⚠️ El hallazgo

Iba a incrustar la fotografía en el paquete de identificación y no se podía: `AlmacenDeArchivos`
tenía `GuardarAsync` **y nada más**. No hay ruta, ni servicio, ni método que lea un adjunto.

Todo lo que el sistema exige adjuntar quedaba escrito y **fuera de alcance**:

| Lo que se exige adjuntar | Y no se podía abrir |
|---|---|
| El respaldo documental del parámetro normativo | `HU-145`: *«quien aprueba tiene que poder abrir el documento»* — **bloqueé la aprobación sin él esta mañana**, y no había forma de abrirlo |
| La fotografía de la constatación de rotulación | Obligatoria por `RN-18` — *«sin fotografía no debe aceptarse»* |
| El documento de respaldo de placa | Lo que el agente pide en carretera |
| El paquete de evidencia de una misión | Lo que un auditor viene a ver |

El primero es mío, de hace unas horas. Puse un bloqueo que exige que el adjunto **exista**, y
el motivo escrito era que quien aprueba pudiera **verlo**. No podía.

### Leer no es simétrico de escribir

Escribir un adjunto es un hecho del dispositivo. **Leerlo es un acceso**, y algunos llevan
datos personales: `RN-52` exige que la consulta quede asentada **antes** de mostrar. Un almacén
que devolviera bytes sin más convertiría el registro de consultas en una formalidad que se
salta pidiendo la foto directamente.

- **Lo desconocido cuenta como dato personal.** Una clasificación que nadie reconoce es un
  adjunto del que no se sabe qué contiene, y servirlo sin registrar sería decidir por omisión
  que no importa.
- **La respuesta dice que el acceso quedó asentado** (`X-Acceso-Registrado`). Quien consulta
  tiene derecho a saberlo ahora, no a descubrirlo después en un reporte.
- **La ruta no se confía aunque venga de la base.** Una fila con `..\..\` serviría cualquier
  archivo del servidor: la cadena de conexión, una clave. El almacén no decide en quién confiar.

### Y dos respuestas que decían 500

Pedir un adjunto sin decir quién, y pedir uno inexistente, devolvían **500** — que no le dice
nada a quien llama. Ahora son 400 y 404. Y la fila que existe con el archivo ausente devuelve
**502**, no 404: **no es «no existe»**, es el almacén mal montado o restaurado a medias
(`ADR-004`), y un 404 mandaría a buscar un adjunto que sí está registrado.

### Lo que esto cerró

El **quinto contenido de `RN-65`** —la fotografía del vehículo con su rotulación— ya está en el
paquete impreso. Y cuando no hay, el papel lo dice: *«sin fotografía: este documento describe
al vehículo y no permite compararlo»*.

### Lo que sigue abierto

- El **acuse de entrega** del paquete y del salvoconducto. `RN-65` pide *«emitir, imprimir y
  entregar contra acuse»*: se imprimen y no se registra quién los recibió.
- ~~**`PT-022`**, la firma en lote de feriado largo (`HU-020`).~~ **RESUELTO.**

---

## `RN-22` — el acta que contesta «¿quién lo tenía, y con qué?»

**1341 pruebas en verde.**

Smart App Control bloqueó el dll de pruebas más de 100 intentos; el bloque se comiteó
primero con la brecha declarada y se enmendó al ceder. El circuito se verificó **vivo contra la
API** mientras tanto, que es verificación real y no una promesa.

### Lo que faltaba

`BD-13` ya impedía despachar un vehículo sin custodio vigente. Lo que no existía era el
**traslado**: el acto por el cual ese custodio le entrega la unidad al motorista y se la vuelve
a recibir, con odómetro, nivel, accesorios, estado y constancia.

El sistema sabía **de quién es** el vehículo y no **quién lo tenía** — y la segunda es la que
hace falta cuando falta un gato o aparece un golpe. Sin cadena de custodia, *«la deducción de
responsabilidad no tiene sobre quién recaer»*, y ante el TSC eso **agrava en vez de atenuar**.

### El cotejo es el producto

Un acta de entrega con cinco elementos y una devolución con cuatro son, por separado, **dos
listas que nadie lee**. Al restarlas, el gato que no volvió tiene nombre, fecha y dos personas.

Cuatro decisiones que salen de ahí:

- **Ausente de la lista no es lo mismo que marcado ausente.** El gato que nadie anotó y el que
  no volvió son dos situaciones distintas.
- **El cotejo no se confunde por mayúsculas.** Quien llena el acta del retorno escribe «Gato
  Hidráulico» donde la de salida decía «gato hidráulico», y un cotejo sensible produciría **un
  faltante y un agregado inventados** — dos hallazgos falsos de una diferencia de caja.
- **Lo que aparece sin constar en la entrega se dice sin acusar**: suele ser algo que se olvidó
  anotar al salir, y a veces algo que el motorista repuso de su bolsillo.
- **Un odómetro que retrocede no son cero kilómetros.** Se reinició, se sustituyó, o alguien
  tecleó mal, y las tres exigen mirarlo. Devolver cero enterraría el problema dentro de un
  número que parece normal.

Y sin las dos actas **no hay cotejo**: nulo, no un cotejo vacío. Uno vacío se leería como «no
faltó nada», que es una afirmación que nadie hizo.

### ⚠️ `HB61-01` — el noveno efecto de `RN-61` es inalcanzable

Al engancharlo apareció la contradicción. `RN-61` dice *«toda sustitución sobre una Orden de
Misión ya `PROGRAMADA` **o posterior**»*, y `§10.2` sólo admite `T-10` de `PROGRAMADA` a
`PROGRAMADA`: después de despachar no hay transición de reasignación.

**El acta de entrega se levanta al despachar**, que es después del único punto donde `T-10`
existe. Un acta y una reasignación **no pueden coexistir**, así que el efecto sobre la custodia
**no puede dispararse nunca**.

La comprobación queda escrita —es correcta si el acta existiera por cualquier vía, y el día que
`§10.2` admita el relevo en ruta ya está—, pero **está declarada como inalcanzable**: en el
código, en la prueba y en la regla. La autoridad sobre transiciones es la máquina de estados, y
es `RN-61` la que dice de más. **No se resolvió en silencio.**

### Los nueve efectos de `RN-61`, al cierre

| | |
|---|---|
| Estimado de peajes, salvoconducto, vales de combustible | ✅ |
| Habilitación, compatibilidad, documentación, estado operativo | ✅ ya los revalida `T-10` |
| Rendimiento esperado | ✅ no hacía falta: se resuelve por vehículo a la fecha del hecho |
| Paquete de identificación | ✅ `PT-139` |
| **Custodia** | ⚠️ escrito y **inalcanzable** — `HB61-01` |

---

## `RN-18` y el paquete que el inventario no tenía

**1328 pruebas en verde.** Iba a cerrar `RN-65` con su documento impreso y no se podía armar
honestamente: el quinto contenido que la regla pide es *«fotografía vigente del vehículo con
su rotulación»*, y la rotulación era **otro booleano**.

### ⚠️ El segundo booleano del día

`IdentificacionInstitucionalVerificada`. Y `CLAUDE.md` lo pone entre las restricciones que
condicionan el diseño, con estas palabras: *«es campo verificable **con fecha y foto**: es
hallazgo frecuente de auditoría»*.

Un `true` no dice **cuándo** se miró, ni **quién**, ni deja nada que mostrar. Una constatación
de hace tres años se veía igual que una de ayer, y ante un operativo lo único que quedaba era
la palabra de alguien.

| Ahora | |
|---|---|
| **Una fila por elemento** | Franjas, leyenda, siglas, correlativo. Un vehículo puede tener las franjas y no la leyenda: **constatar tres no es constatar** |
| **Con fecha, foto y quién** | La foto es obligatoria y se comprueba que exista — `RN-18`: *«una constatación sin fotografía no debe aceptarse»* |
| **Caduca** | La pintura se despinta. Y **«caducada» no dice que la rotulación se haya borrado: dice que nadie la ha vuelto a mirar** |
| **Plazo más corto sin lámina** | Ahí la rotulación es la **única identificación visible**: si caducara al mismo ritmo, el vehículo que más depende de ella sería el que más tiempo pasa sin que nadie la mire |

Y una distinción que el booleano hacía imposible: **un elemento que no está es un hallazgo; uno
que nunca se miró es una tarea**. Reportar «no constatada» sobre un vehículo con la leyenda
borrada escondería el hallazgo detrás de la omisión.

### `HB65-01` — un documento que la regla exige y el inventario no tenía

`RN-65` dice que el despacho **debe emitir, imprimir y entregar contra acuse** el paquete de
identificación en carretera, y enumera sus cinco contenidos. **No tenía pantalla en el
inventario de 138.**

No es un descuido menor: es el único documento que identifica como bien del Estado a un
vehículo sin lámina metálica, y **hay desabastecimiento nacional**. Un agente que detiene ese
vehículo pide la lámina primero; sin ella, lo que queda es este papel o la palabra del
motorista.

Se le asignó **`PT-139`** y está construido. Queda `⛔ sin CU`: `CU-05` no lo menciona.

**Se arma, no se congela** — a diferencia del salvoconducto, que congela lo que ampara porque
materializa una firma. Éste no ampara nada: describe. Congelarlo produciría un papel que dice
que la rotulación se constató en marzo cuando en junio faltaba la leyenda.

### Lo que queda

- **La fotografía todavía no se incrusta** en el paquete. Las constataciones ya la guardan —es
  obligatoria—, y el documento sólo dice el estado. Declarado en el registro de pantallas.
- **De `RN-61` queda uno solo**: la custodia, que `RN-22` manda trasladar con constancia al
  sustituir el vehículo.
- El acuse de entrega del paquete (`RN-65` pide *«entregar contra acuse»*) **no está**: se
  imprime y no se registra quién lo recibió.

---

## `RN-64` y `RN-65` — el booleano que decía «hay una constancia»

**1317 pruebas en verde.** Cierra el penúltimo de los nueve efectos de `RN-61`.

### ⚠️ El hallazgo

*«Sin placa metálica es un estado válido — hay desabastecimiento nacional»*. Es premisa del
proyecto, y el sistema lo resolvía con un booleano: `TieneConstanciaSustitutaDePlaca`.

Eso dice **que hay una constancia** y nada más. Una vencida a mitad de la misión pasaba
**exactamente igual** que una vigente, y un permiso provisional de treinta días emitido hace
un año se veía idéntico a uno de la semana pasada. El agente que revisara el cuarto día de una
misión de cinco tendría enfrente un vehículo del Estado sin lámina y sin nada que lo explique.

### Lo que quedó

| | |
|---|---|
| **`RN-64`** — estado de la lámina tipificado | Seis valores. El **número de placa y el estado de la lámina son dos datos distintos**: el número puede existir aunque la lámina no |
| **`RN-65`** — respaldo con emisor, folio, adjunto y vigencia | Y el bloqueo evalúa la vigencia contra **todo el rango, extremos incluidos** — mismo patrón que `RN-10` |
| **Historial, no un registro que se pisa** | La pregunta del auditor es *«¿con qué documento circulaba este vehículo en marzo?»* |
| **El veredicto va resuelto** | Una lista de fechas obliga a hacer la resta a mano, y ésa es la resta que el sistema existe para no equivocar |

Tres distinciones que el booleano no podía hacer, y las tres tienen su prueba:

- **Sin fecha de vencimiento no es «vigente para siempre».** Un provisional sin fecha
  declarada es justo lo que hay que preguntar antes de despachar; tratarlo como indefinido
  convierte un dato faltante en una autorización.
- **«Todavía no rige» no es «vencido».** Uno no empezó y el otro terminó, y el arreglo difiere.
- **El respaldo sin documento adjunto no alcanza.** El agente pide el papel: uno que sólo
  existe como texto en una pantalla no se le puede mostrar. Es la misma distinción que costó
  el respaldo del parámetro normativo esta mañana — *el identificador de un adjunto no es el
  adjunto*.

### Y el defecto que la prueba atrapó al escribirla

`ConsultaDeFlota.PorIdAsync` **no cargaba los respaldos**. La regla nueva los evaluaba, y
llegaban vacíos siempre: el bloqueo habría dicho «sin respaldo» sobre un vehículo con uno
vigente, y **nadie habría podido despachar ningún vehículo sin lámina**.

Es el mismo defecto de siempre —la regla correcta con el otro extremo sin conectar— y la única
razón de que no llegara al commit es que la prueba de punta a punta cruza el servicio.

### Lo que queda de `RN-61`

De los nueve valores derivados, **queda uno**: la **custodia**, que `RN-22` manda trasladar
con constancia al sustituir el vehículo. El paquete de identificación en carretera —el
documento impreso de `RN-65`— tampoco está: la maquinaria de impresión, folio y QR ya existe
desde `PT-023`, así que es encadenar más que construir.

---

## `RN-32` — el bloqueo que se llamaba siempre con nulo

**1299 pruebas en verde.** Iba a cerrar lo que faltaba de `RN-61` y apareció algo más urgente
por el camino.

### ⚠️ El hallazgo

`RN-32` tiene un caso límite escrito con estas palabras: *«un vale de diésel para un vehículo
de gasolina es un error caro y perfectamente evitable»*. La regla existía, era correcta y
tenía sus pruebas de dominio.

**Y el servicio la llamaba siempre con `null`.** La razón estaba en un comentario del propio
código, escrito con honestidad y nunca resuelto: *«la ficha del vehículo no declara el
combustible que usa»*. Sin nada contra qué comparar, el bloqueo salía por la rama de «no
declarado» **en todos los casos**.

No es fraude: es desperdicio, o un motor arruinado.

| Qué se hizo | |
|---|---|
| `TipoDeCombustible` entra a la ficha del vehículo | Nulo sigue siendo válido — «la ficha no lo declara» es un estado real de una flota heredada |
| El servicio lo **resuelve**, ya no lo recibe | Recibirlo por parámetro dejaba la comparación en manos de quien emite: mandar el mismo valor a los dos lados la vuelve una tautología |
| La flota sembrada declara diésel | Sin eso, las pruebas de punta a punta seguían sin ejercitar el bloqueo |

La clase de pruebas donde va la nueva ya tenía un hermano suyo, con este comentario: *«la
prueba que atrapó el defecto real: el servicio pasaba el motorista de la orden a los dos lados
y la regla comparaba algo consigo mismo»*. **Es el mismo defecto, en la línea de al lado.**

### Y lo que faltaba de `RN-61`, con un lugar propio

Los efectos de la sustitución estaban repartidos: uno en el enrutador. `RN-61` lista **nueve**
valores derivados en cuatro módulos distintos, y **la forma en que una regla así se rompe es
que alguien agregue el décimo y no toque las nueve llamadas**. Ahora hay
`EfectosDeLaSustitucion`, y lo que falta está declarado en el mismo sitio.

| Efecto | |
|---|---|
| Estimado de peajes | ✅ recalcula y deja asiento de diferencia |
| **Salvoconducto** | ✅ el permiso se reemite si dejó de cubrir — el papel anterior queda **anulado de inmediato** |
| **Vales de combustible** | ✅ se **reportan** los que dejaron de corresponder |
| Habilitación, compatibilidad, documentación, estado operativo | ✅ ya los revalida `T-10` |
| Rendimiento esperado | ✅ **no hacía falta**: se resuelve por vehículo a la fecha del hecho, no se congela |
| Custodia | ⛔ no se traslada con constancia (`RN-22`) |
| Paquete de identificación en carretera | ⛔ no se re-emite (`RN-65`) |

**Los vales se reportan y no se anulan solos**, a propósito: uno ya entregado tiene dinero
público fuera de la caja, y anularlo exige el acta de devolución de `RN-27` —un acto con su
propia persona y su propio momento—. Lo inaceptable era que nadie se enterara.

### Y que se vea, que es la mitad que se olvida

El arrastre viaja en la **respuesta de la reasignación** y la pantalla lo dice de una vez:
cuánto cambió el peaje, que el salvoconducto quedó anulado y que el permiso nuevo espera
firma, y qué vales hay que resolver.

Sin eso, quien reasigna **se va creyendo que cambiar un vehículo es cambiar un vehículo**, y
el sábado descubre que la misión no puede salir.

---

## `RN-61` — el estimado que quedaba mal en silencio

**1296 pruebas en verde.** Era el hallazgo que yo mismo había levantado el bloque anterior:
`HU-018` pide que la sustitución de vehículo recalcule los valores congelados y lo remite a
`CU-04`/`M-07`, y **no estaba hecho en ninguno de los dos**.

### Por qué importa

El estimado congelado **es lo que el autorizador autorizó**. Sustituir un pick-up por un
camión de dos ejes puede duplicar el peaje de una ruta larga — en la prueba, la misma caseta
pasa de L 50.00 a L 150.00. Sin recalcular, la misión se liquida contra una cifra que ya no
corresponde a ningún vehículo real: **la conciliación cuadraría contra un número inventado.**

### Qué quedó construido

| | |
|---|---|
| **El recálculo** | Con la tabla vigente a la fecha del hecho (`P-4`). Se dispara solo, en `T-10 Reasignar` |
| **El asiento de diferencia** | Categoría anterior → nueva, total anterior → nuevo, diferencia, motivo, autor |
| **Lo anterior se conserva** | `RN-04`: las líneas viejas quedan marcadas como superadas, no borradas |
| **Y se ve** | En el mismo desglose de peajes del expediente, no en otra pantalla |

La ruta **no se vuelve a pedir**: los puntos y los cruces son de la ruta, no del vehículo. Lo
que cambia es la categoría. Volver a pedirla abriría la puerta a que una sustitución cambie en
silencio lo que se autorizó recorrer — que es justo lo que `RN-37` existe para detectar.

### ⚠️ Tres cosas que sólo aparecen al construirlo

**1 · El índice único hacía imposible la regla.** `IX_RutaAutorizada_MisionId_PuntoId` era
único sobre *todas* las filas, así que conservar el congelamiento anterior junto al nuevo era
imposible a nivel de base. La restricción **codificaba el supuesto de que una misión se
congela una vez** — cierto sólo hasta que se sustituye el vehículo. Ahora es filtrado sobre lo
vigente, que es lo que de verdad se quería decir.

**2 · Tres lectores había que enseñarles a filtrar.** El guardia de doble congelamiento, la
coherencia de `RN-37` y el desglose del expediente leían todas las filas. Sin el filtro,
`RN-37` se contestaría contra **dos rutas a la vez** y un desvío desaparecería por coincidir
con la ruta vieja.

**3 · El mismo defecto de siembra, otra vez.** `PeajesPruebas` sembraba su catálogo con
`if (!tabla.Any())`, así que mi clase nueva —al insertar su propia categoría— dejaba ese
bloque sin ejecutarse: **ocho pruebas fallaron con «la matriz no está cargada» mientras el
código que la siembra estaba ahí, intacto.** Es el mismo patrón que costó la Máxima Autoridad
esta mañana. Corregido con guardia por código, como ya lo hacía `CoherenciaDePeajesPruebas`.

Y un cuarto, de mi propia prueba: sembré la matriz con `RegistradoDesde` **en el futuro**, y la
bitemporalidad la descartó correctamente. El sistema tenía razón; la prueba no.

### Lo que `RN-61` pide y sigue sin hacerse

La regla lista **nueve** valores derivados. Se implementó el primero. Los otros:

| Valor derivado | Estado |
|---|---|
| Categoría de peaje y estimado | ✅ **hecho** |
| Habilitación del motorista, compatibilidad, documentación, estado operativo | ✅ ya lo revalida `T-10` |
| Rendimiento esperado galonaje–kilometraje | ⛔ no se recalcula |
| Custodia | ⛔ no se traslada con constancia |
| Salvoconducto de día inhábil | ⚠️ existe la reemisión (`PT-024`) pero **no se dispara sola** al reasignar |
| Folios de vale de combustible | ⛔ no se anulan ni re-emiten si cambia el tipo |
| Paquete de identificación en carretera | ⛔ no se re-emite |

El del salvoconducto es el más cercano a cerrarse y el más grave de los que faltan: la
maquinaria está entera, sólo falta encadenarla al mismo punto donde ahora se recongela el
peaje.

---

## `PT-024` — el permiso que dejó de cubrir, dicho antes del sábado

**1293 pruebas en verde.** Cierra la historia del permiso de circulación: tramitar, firmar,
imprimir y **reemitir**.

### El problema

`BD-04` ya bloqueaba el despacho con un permiso que dejó de cubrir la misión — y llegaba
**tarde**: el sábado por la mañana, con el vehículo cargado y la máxima autoridad sin
trabajar. Los disparadores son cotidianos: el vehículo entra a taller la víspera, el motorista
aparece con incapacidad, la misión se corre al fin de semana siguiente.

Ahora el expediente lo dice **el jueves**, en la pantalla donde el Jefe de Transporte ya está.

### Y dice qué cambió, no sólo que cambió

Cada elemento tiene su propio arreglo y su propia urgencia. *«El permiso ya no cubre»* manda a
comparar cuatro cosas a mano contra un papel; **el mensaje que no nombra el elemento convierte
una acción en una investigación.**

| Cambió | Qué dice |
|---|---|
| El vehículo | Nombra **los dos**: el amparado y el de hoy |
| El motorista | *«El permiso es nominativo. La firma anterior no se arrastra»* |
| El destino | Nombra los dos destinos |
| La ventana | *«La vigencia no se traslada»* — con las dos fechas |

### ⚠️ La distinción que apareció al verificarlo vivo

La primera versión mandaba a **reemitir** también cuando la misión sólo se había
desprogramado. Y eso está mal: si se reprograma con el mismo vehículo y el mismo motorista,
**el permiso vuelve a amparar solo**. Reemitirlo habría quemado un folio —que no se recicla—
y pedido otra firma de la máxima autoridad para nada.

El dominio ahora separa las dos cosas: `NoCubre.ExigeReemision`. **Falso significa espere;
verdadero significa actúe.** La pantalla ofrece el botón sólo cuando algo cambió de verdad, y
lo verifiqué vivo: desprogramar y volver a programar con lo mismo devuelve el amparo sin
tocar nada.

### Reemitir son tres cosas en un acto

1. **El salvoconducto anterior queda anulado**, con motivo y autor (`RN-04` — asiento reverso,
   no una bandera). El papel sigue impreso en la mano de alguien: el punto de verificación
   tiene que decir que no vale **de inmediato**, o un documento anulado pasa un control.
2. **El permiso anterior se desiste** y deja de contar para `BD-04`.
3. **El nuevo nace sin firma.** Es lo que más fácil se rompe: un permiso reemitido *parece*
   una corrección del anterior, y es un acto nuevo — lo que la máxima autoridad firmó fue
   **otro** vehículo con **otro** motorista.

Con referencia cruzada en los dos sentidos: sin ella un auditor ve dos folios, dos firmas y
dos salvoconductos, y tiene que reconstruir el orden por las fechas.

### La excepción deliberada

Un **relevo documentado en ruta no invalida** el permiso de la misión ya iniciada. El vehículo
está en la carretera: declarar el papel inválido no lo devuelve, y sí dejaría al motorista
relevado circulando sin nada. Lo que sí exige permiso reemitido es la circulación en franja
inhábil **posterior** al traspaso — que es de `M-08` y no se bloquea.

### Un defecto que salió al probar la reemisión

`GET /misiones/{id}/salvoconducto` usaba `Single` y **reventaba con 500** la primera vez que
se reemitió: suponía que una misión tiene a lo sumo un papel, y eso es cierto sólo hasta que
algo cambia. La unicidad es por **permiso**, no por misión. Ahora devuelve el vigente, y si
todos están anulados devuelve el último — la pantalla tiene que poder decir qué pasó.

### Lo que queda abierto

- **`RN-61` no se implementó acá**: la sustitución de vehículo debe recalcular y volver a
  congelar los valores derivados —el estimado de peajes cambia con la categoría por ejes—,
  con asiento de diferencia. `HU-018` lo pide y su propia sección de alcance lo remite a
  `CU-04` y `M-07`. **No está hecho en ninguno de los dos.**
- ~~**`PT-022`, la firma en lote de feriado largo** (`HU-020`).~~ **RESUELTO.**
- El hallazgo de `HU-018` sobre las tres redacciones de lo que el permiso ampara —`BD-04`
  dice dos elementos, `PC-03` tres y `RN-23` cuatro— **se resolvió a favor de `RN-23`** en el
  código: `Ampara` y `PorQueYaNoCubre` comparan los cuatro. La máquina de estados sigue
  diciendo dos: es la autoridad y **le toca alinearse**.

---

## `PT-023` — el primer documento físico del sistema

**1276 pruebas en verde.** Hasta acá todo vivía en pantalla. El salvoconducto sale por una
impresora, se dobla y se guarda en la guantera, y su único destinatario —el agente del TSC o
de la DNVT en un operativo— **no tiene usuario, no se autentica y no verá nunca el**
**expediente**. Es la premisa rectora 4 dejando de ser una frase.

### Lo que quedó construido

| | |
|---|---|
| **El papel** | Folio, vigencia explícita arriba y grande, vehículo por siglas institucionales, QR, huella, espacio de firma y sello |
| **El QR** | SVG generado en el cliente. `qrcode-generator`, sin dependencias transitivas: **on-premise y sin red que consultar** |
| **El código corto** | Ocho caracteres sin `I`, `O`, `0` ni `1` — se dicta por teléfono |
| **La verificación** | Por folio (lo que resuelve el QR) **y** por código corto (lo que se anota cuando no hay señal) |
| **La reimpresión** | Mismo folio, mismo contenido, misma huella. Con quién, cuándo y por qué |
| **La hoja de impresión** | A4, sin la cáscara, sin sombras. Es el patrón para el resto de `M-15` |

### ⚠️ Y un defecto mío, de media hora antes

`RN-25` obliga a un tercer estado además de vigente y anulado: **`DESACTUALIZADO`**. El
documento se imprime *antes* de salir —una delegación sin cobertura lo emite por anticipado— y
la misión puede cambiar después. El papel deja de corresponder **sin que nadie lo anule**.

Lo implementé, tiene sus pruebas de dominio, y al verificarlo vivo daba **`Vigente` siempre**.
La razón: contrastaba el papel contra la copia congelada del **permiso** — y las dos copias se
congelan en el mismo acto, así que no pueden diferir. El estado era **inalcanzable**.

Un relevo de motorista dejaba un papel que no ampara a nadie contestando *«documento válido»*
a quien lo verificara en la carretera, que es exactamente el daño que el estado existe para
evitar.

**Y al corregirlo apareció el defecto de abajo.** Contrastar contra la reserva de la misión
seguía dando `Vigente` después de desprogramar, porque `Reserva()` leía la última transición
con vehículo — y `T-11` **no borra** la reserva anterior: *«liberar es no volver a tomar»*, la
transición de `T-08` permanece en el diario (`P-3`) y simplemente deja de contar.

`ConsultaDeOcupacion` ya lo sabía y lo tenía escrito: la reserva sólo vale en `PROGRAMADA`,
`DESPACHADA` o `EN_RUTA`. Mi `Reserva()` no miraba el estado. Corregido con esa misma regla —
y **también afectaba a `PT-021`**: una misión desprogramada seguía pareciendo programada a la
hora de firmar el permiso.

La prueba de punta a punta que lo fija hace lo único que lo destapa: emitir, verificar,
desprogramar, **volver a verificar**.

### Una dependencia nueva, y por qué

`qrcode-generator` (15 KB, sin dependencias transitivas). La alternativa era escribir un
codificador QR a mano: más código que mantener y el riesgo de un QR sutilmente mal formado que
un teléfono de gama baja no lea al mediodía en una carretera. **Para un documento cuyo
propósito entero es ser escaneable, eso no se improvisa.** Se empaqueta en el bundle: no pide
red en ejecución, que es la condición de un despliegue on-premise.

### Lo que queda abierto

- **El folio es provisional.** No hay rangos de salvoconducto asignados por delegación
  (`RN-44`, insumo #34). El documento lo declara impreso, con su marca: **un folio inventado
  que se ve oficial es peor que uno que dice que no lo es.**
- **`[C]` pendiente G de `RN-25`**: si la institución acepta exponer el punto de verificación
  en internet o lo deja interno. Es configuración de despliegue, no parte del bloqueo — el
  documento lleva QR en los dos casos.
- **`PT-024`, la reemisión** por relevo: hoy se desiste y se abre otro trámite.
- ~~**`PT-022`, la firma en lote de feriado largo** (`HU-020`).~~ **RESUELTO.**
- La regla *«sin impresora no se despacha en día inhábil»* (`HU-017`) **no está implementada**:
  el sistema no sabe si la delegación tiene impresora. Es requisito de despliegue, `[C]`.

---

## `PT-020` y `PT-021` — el bloqueo que era una puerta sin llave

**1258 pruebas en verde.**

### ⚠️ El hallazgo

`BD-04` bloquea el despacho de toda misión que circule en día u hora inhábil sin permiso de la
máxima autoridad. Estaba escrito, probado y operando, con un mensaje cuidado que distingue
*«no hay ningún permiso registrado»* de *«hay permisos y ninguno ampara»*.

**Y nadie podía emitir un permiso.** `contexto.Permisos` sólo se leía: la tabla no tenía
escritor, ni endpoint, ni servicio. Cualquier misión que tocara un sábado, un domingo o un
feriado era **indespachable**, y el bloqueo decía «no hay ningún permiso registrado para esta
misión» sin que existiera forma de registrar uno.

La prueba de punta a punta lo tapaba: insertaba la fila directamente en la base. Ahora
atraviesa el circuito —abrir, intentar despachar con el trámite abierto, firma rechazada,
firma concedida, despacho— y por eso el hueco no puede volver.

### Abrir y firmar son dos actos, y ahí estaba la dificultad real

`RN-23` dice dos cosas que **no se cumplen a la vez sobre un solo acto**: el permiso no exige
que la misión esté programada, y el permiso es nominativo sobre vehículo, ruta y ventana. Si
naciera firmado habría que exigir el vehículo desde el principio, y el trámite no podría
adelantarse a la programación — que es justo lo que hay que poder hacer un viernes por la
tarde para una salida del sábado.

Se separan (resolución `HCU-05` de `CU-03`): **se abre sin vehículo, no se firma sin él.**

Y de ahí sale el invariante que sostiene todo: **un trámite `SOLICITADO` no entra en `BD-04`**
— `ConsultaDePermisos` filtra por estado. Si entrara, cualquiera destrabaría el domingo
abriendo un trámite y despachando sin esperar la firma, que es exactamente lo que el permiso
existe para impedir. Tiene su propia prueba de punta a punta.

### La firma es indelegable, y eso es una decisión, no una omisión

`[C]` insumo #29. Hasta que la institución confirme lo contrario, **el sistema no la permite**.
No es conservadurismo por comodidad: si se habilitara por defecto y después resultara
indelegable, cada permiso firmado por delegación sería un vehículo del Estado que circuló un
domingo sin amparo válido, y **no habría forma de repararlo hacia atrás**. Al revés sí se
repara — se habilita y se sigue.

### ⚠️ Dos cosas más que salieron por el camino

**No había ninguna máxima autoridad sembrada.** El rol existía en el enum, `RaizDelPuesto` lo
contemplaba, y ningún puesto lo tenía: nadie podía firmar la única facultad que `RN-23` le
reserva en exclusiva.

**Y la siembra estaba protegida con `if (!tabla.Any())`**, así que la fila nueva **no llegaba
nunca** a una base ya sembrada — se agregaba al código, la condición daba falso y no pasaba
nada. El síntoma es peor que la causa: la prueba fallaba con «no se pudo resolver el puesto de
quien firma» mientras el código declaraba el puesto. Las tres tablas del organigrama ahora se
siembran **fila por fila**, comparando por clave, y no sobreescriben lo que ya está.

### Lo que queda abierto

- **`PT-023`, el salvoconducto impreso, no se construyó.** Necesita folio oficial con QR
  verificable, y ni el rango de folios (`M-01`, insumo #34) ni la impresión (`M-15`) existen.
  El folio del permiso es **provisional** y va marcado `PC-PROV-…` para que nadie lo confunda
  con el correlativo del documento físico.
- **`PT-024`, la reemisión** por relevo de motorista. La regla ya opera —`Ampara` compara el
  motorista, así que un relevo invalida el permiso— pero **no hay pantalla para reemitirlo**:
  hoy se desiste y se abre otro trámite.
- ~~**`PT-022`, la firma en lote de feriado largo** (`HU-020`).~~ **RESUELTO.**
- El destino sigue representando la **ruta** que `RN-23` pide. Dos misiones a Choluteca por
  caminos distintos se ven iguales — `[C]` con Auditoría Interna, ya declarado en el dominio.

---

## `PT-099` y `PT-100` — el respaldo que nunca tuvo dónde estar

**1244 pruebas en verde**, y el circuito del parámetro normativo verificado vivo de punta a
punta: subir el documento, cargar con vigencia, y los cuatro desenlaces de la aprobación.

### ⚠️ El hallazgo: el identificador del adjunto no era el adjunto

`HU-145` pide rechazar la aprobación *«sin haber visto el respaldo documental»*. Al ir a
implementarlo apareció que **no se podía**: `RespaldoDocumental.Adjunto` es un `Ulid`
obligatorio —así que la columna nunca estaba vacía— y apuntaba a `mision.Adjunto`, una tabla
cuyo `IdTransicion` era **obligatorio**. Un respaldo de parámetro no cuelga de ninguna
transición de misión.

O sea: **no había ninguna fila que pudiera contener ese documento.** El identificador se
cargaba, la pantalla mostraba fuente y fecha de verificación —que son texto que alguien
escribió— y detrás no había nada. Un respaldo que no está **se veía igual que uno que sí**.

Y las pruebas lo reproducían sin notarlo: `Respaldo()` devolvía un `Ulid.NewUlid()` suelto y
pasaba, porque nadie comprobaba.

| Qué se hizo | Dónde |
|---|---|
| `IdTransicion` pasa a admitir nulo — un adjunto tiene **dos dueños posibles** | migración `AdjuntoDeParametro` |
| `POST /adjuntos` deja de exigirlo: ausente **no es inválido** | `Program.cs` |
| La aprobación comprueba el adjunto **contra la tabla**, no contra el tipo | `ServicioDeParametros` |
| Tercer rechazo del doble control, con qué hacer | `ReglasDeDobleControl` |

El rechazo nuevo va **antes** que el de identidad, y no es cosmético: un respaldo que no está
bloquea a cualquiera. Decirle a quien cargó «usted no puede aprobar» lo manda a buscar un
colega que se estrellaría contra el mismo muro, y recién ahí se sabría lo que faltaba.

### El «impacto estimado» de `HU-145` no se construyó como lo pide la historia

La historia pide una frase del tipo *«con 25 % dejarían de generarse 34 de los 41 hallazgos de
consumo del último trimestre»*. **No se sostiene de forma general**: sólo significa algo para
los parámetros que son umbrales de un cálculo, y nada para el formato del folio o el canal de
aviso. Producirla para unos y dejarla en blanco para otros haría que la ausencia se leyera
como «sin impacto», que es falso. Y recalcularla exige rehacer la conciliación de cada misión
del período: **una cifra aproximada, en una pantalla cuyo propósito es que alguien firme, es
exactamente el dato que después se cita como si fuera exacto.**

Se construyó en su lugar lo que **sí es cierto para todo parámetro y es exacto: desde cuándo
rige.** Si la vigencia arranca antes de hoy, aprobar no cambia el futuro — cambia la base de
cálculo de hechos **ya ocurridos y ya registrados**, porque `P-4` manda usar la tabla vigente
a la fecha del hecho. La pantalla lo declara, y cuenta las misiones con salida dentro de esa
ventana **diciendo que es una cota superior**, no una cuenta de afectadas.

### Dos defectos menores que salieron por el camino

- **`pedir()` forzaba `content-type: application/json` en toda petición**, incluidas las de
  `FormData`. El navegador tiene que escribir ese encabezado él, porque el valor lleva el
  `boundary` que separa las partes. La subida llegaba como formulario mal formado, sin archivo
  y sin campos, y el error no mencionaba el encabezado. No se había notado porque **nada subía
  archivos todavía**.
- Tres clases mías escritas `sm:tw:grid-cols-2` en vez de `tw:sm:grid-cols-2`. Con el prefijo
  invertido no generan nada: la rejilla de dos columnas simplemente no existía. `verificar-tokens`
  no lo detecta — comprueba tokens, no validez de clases.

### Lo que queda abierto

- **`CU-19` sigue sin existir.** `PT-099` y `PT-100` se construyeron contra `HU-144`/`HU-145`,
  no contra un caso de uso: alcanza para las dos pantallas y **no alcanza para el flujo
  completo** — quién devuelve una carga rechazada, cómo se cierra una vigencia abierta, qué
  pasa con lo ya calculado cuando se aprueba algo retroactivo.
- **`mision.Adjunto` admite hoy un adjunto sin ningún dueño.** `IdTransicion` nulo era
  necesario, pero falta la restricción que obligue a tener **exactamente uno** de los dos.
  Mientras no esté, un adjunto huérfano no lo detecta nadie.
- El inventario de pantallas decía «⛔ sin historia» de `PT-099` y `PT-100` **meses después de
  que dejara de ser cierto**. Corregido, y anotado en `HB34-67`: es el modo normal en que una
  tabla derivada miente.

---

## `PT-101` — el panel que dice qué está apagado

**1241 pruebas en verde.** Llevo ocho bloques escribiendo «este parámetro no está
configurado» en la pantalla donde aparece, y hasta ahora **ninguna pantalla los juntaba**.

### El problema que resuelve

Cada aviso por separado es correcto y **ninguno alcanza**: nadie recorre once pantallas para
saber qué le falta configurar. Sin un resumen, **un control apagado se descubre el día que
hacía falta** — que es tarde por definición.

Y dice **qué hacer**, no sólo qué falta. Es la mitad que convierte un tablero en algo
accionable: *«umbral no configurado»* manda a preguntar; *«acuérdelo con Auditoría Interna y
cárguelo acá»* se puede hacer.

Junta parámetros e integraciones en **una sola lista**, a propósito: a quien administra el
sistema le da igual si lo que falta es una clave o una integración, y separarlos en dos
pantallas haría que la segunda no la mire nadie.

### ⚠️ Y destapó algo del propio trabajo de estos bloques

La primera corrida dijo **«2 de 10 sin configurar»** — y era engañoso. Los otros ocho
aparecían con su casilla marcada, pero **cuatro son siembra de prueba que puse yo** para poder
verificar cada bloque: el formato del folio, el umbral de agotamiento, las causas
improductivas y el plazo de depuración.

**Un valor cargado y uno decidido se ven iguales en una casilla marcada.** Un administrador
habría visto un sistema listo que no lo está, y nadie habría vuelto a preguntar por esos
cuatro.

Se corrigió mostrando **el respaldo documental** de cada valor —que ya existía, porque
`HU-144` lo exige al cargar— y marcando los que se delatan solos. Ahora el panel dice: *«4
valores están cargados con respaldo de prueba. Cuentan como pendientes aunque el control
funcione.»*

La detección es una **heurística sobre el texto del respaldo**, así que avisa en vez de
bloquear: un respaldo legítimo que mencione «prueba» aparecería marcado, y eso es preferible
a que uno de prueba pase por decidido.

### Lo que el panel reporta hoy

| | |
|---|---|
| **Apagados** | Espejo de Talento Humano (no es configuración: hay que construirlo) · Umbral de degradación del seguimiento (#68) |
| **Con respaldo de prueba** | Formato del folio (#34) · Umbral de agotamiento (#34) · Causas improductivas (#103) · Plazo de depuración |
| **Configurados de verdad** | Canal de aviso (#102) · Catálogo de estados en ruta · Rangos de folio · Campos sensibles fundamentados |

Y `cero sin configurar` **no significa que todo funcione**: significa que no falta nada de
esta lista. La pantalla lo dice con esas palabras.

---

## M-17 — el ciclo de vida del dato personal (`PT-134` a `PT-137`)

**1241 pruebas en verde.** Las cuatro cosas que la Constitución permite pedir, y la que la
institución tiene que hacer por su cuenta: **qué guardan sobre mí**, **quién lo vio**,
**corríjanlo**, y **cuándo deja de guardarse**.

### `PT-137` es lo único del sistema que destruye contenido

Todo lo demás se reversa, se anula o se marca. Por eso lleva **tres bloqueos**, y ninguno es
una formalidad:

| Bloqueo | Por qué |
|---|---|
| **Sin plazo configurado no depura nada**, y no aplica ninguno por omisión | Un plazo por defecto sería el equipo decidiendo cuánto conserva la institución la identidad de quien trasladó. Es la decisión que `[C]` deja a Auditoría Interna y al OIP |
| **No toca lo financiero ni los bienes** | Se conservan por el plazo de fiscalización. Borrarlos dejaría al TSC sin con qué probar un asiento |
| **No se ejecuta sin aviso previo**, ni avisando el mismo día | Una destrucción silenciosa **es indistinguible de una pérdida de datos**: el día que falte un manifiesto de hace tres años, nadie podrá decir cuál de las dos ocurrió |

Y borra **las personas, no los manifiestos**: el manifiesto queda con su recuento y sus
novedades. El criterio de éxito es doble —cero datos personales sobrevivientes **y** la cadena
de auditoría verificando después—, y la primera mitad sola se logra borrando la base entera.

La simulación cuenta sin borrar, y **no exige aviso**: es precisamente lo que hay que ver
antes de avisar.

### `PT-136` no filtra: sale de otro origen

`RN-51` es explícita — *«no por filtrado en el reporte, sino por separación de origen»*. La
tabla de personas **ni se consulta**: lo único que cruza la frontera es cuántas personas se
trasladaron.

La diferencia importa porque **un filtro se puede olvidar**. Basta que alguien agregue una
columna al reporte para publicar nombres, y el error aparecería ya publicado. Verificado en
vivo: 28 traslados listos para publicar y **cero apariciones** de un nombre o una
identificación, ni en la respuesta ni en la página.

### `PT-134` contesta las dos preguntas, no una

**Qué guardan sobre mí** y **quién lo vio**. La segunda sólo se puede responder si cada acceso
quedó registrado — y es la que la institución no puede improvisar el día que se la pidan.

En la prueba, la búsqueda devolvió una aparición y **tres accesos**, con quién, con qué rol y
con qué alcance vio cada uno.

**Y la consulta misma queda registrada.** Atender un hábeas data implica leer datos
personales: no registrarla dejaría fuera del control justamente las consultas más sensibles.

### `PT-135` — rectificar no es corregir

El manifiesto original **queda intacto**. La rectificación es un asiento aparte con el valor
anterior, el nuevo, quién lo pidió y por qué — porque un manifiesto editado **deja de coincidir
con la lista impresa que el motorista llevó**, y esa discrepancia aparece años después sin
nadie que pueda explicarla.

Exige decir quién lo pidió: el hábeas data sólo lo puede interponer el titular, y sin ese dato
el cambio es indistinguible de una corrección interna sobre un dato personal.

`[C]` El **plazo de depuración** sigue sin fijarse. Se sembró 1095 días en desarrollo, que
**no es decisión de la institución**.

---

## M-17 — el manifiesto y sus novedades (`PT-095`, `PT-129`, `PT-131`)

**1218 pruebas en verde.** Sobre el núcleo del bloque anterior se construyó la entidad que le
faltaba: el manifiesto. Las cuatro pantallas de hábeas data y transparencia ya tienen sobre
qué operar.

### Exigir documento no impide que la persona suba: impide que figure

`HU-113` lo dice así — *«para que el traslado salga amparado y con constancia de quién iba,
**en lugar de que la persona suba sin figurar en ningún papel**»*.

El vehículo sale igual. Lo único que cambia es si queda constancia. Un campo obligatorio de
identidad produce manifiestos **con menos gente de la que viajó** — y eso es peor que uno con
una persona no identificada, porque el primero **miente** y el segundo declara lo que sabe.

Por eso `NoIdentificada` es un caso previsto, no un registro incompleto. Lo que sí se rechaza
es decir «documento» y no poner cuál — y el mensaje ofrece la salida en la misma frase.

### El manifiesto se cierra al despachar, y después no se toca

> *«Si se puede editar después, deja de ser una declaración y pasa a ser **un resumen ajustado
> a lo que ocurrió** — que es exactamente lo contrario de un control.»* — `RN-53`

Lo que pasa después se registra como **novedad de ruta** y se suma sin tocar el original. Por
eso el manifiesto lleva dos cifras: **declaradas** —lo autorizado, que no cambia— y
**efectivas**, que sale de las novedades. La liquidación compara las dos.

`Efectivas` se **calcula**, no se guarda: una segunda cifra almacenada se desincroniza de los
asientos que la sostienen.

**Y subir a alguien en ruta exige decir quién lo autorizó.** Es la novedad que más se presta
—el vehículo institucional que lleva a un conocido—: con autorización nombrada es la decisión
de alguien; sin ella, un favor que nadie firmó. Las otras dos no la piden: nadie autoriza que
alguien no llegue, y exigirlo convertiría el registro de un hecho en un trámite — con lo cual
el hecho dejaría de registrarse.

### El alcance no filtra: decide qué sale del servidor

Con «sólo cuántos van», los nombres **no viajan**. No se ocultan en la pantalla — no se cargan
en la respuesta. Si fueran y se escondieran en el cliente, cualquiera los vería abriendo las
herramientas del navegador, **y el asiento diría que sólo vio un número**.

Verificado en vivo: con recuento, `declaradas 2 · efectivas 1` y **cero nombres en la
respuesta**; con manifiesto completo, las dos personas —una identificada y una no— y la
novedad que explica la diferencia.

### Y la separación estructural se volvió concreta

El manifiesto es **tabla propia**, no columnas del expediente. `RN-51` lo exige para que la
gestión pública pueda exportarse sin los datos personales, y la diferencia es de fondo: si
vivieran juntos, la exportación de transparencia tendría que **filtrar** — y un filtro es algo
que alguien puede olvidar, o que una consulta nueva puede saltarse. Separadas, el reporte sale
de otra tabla y no hay nada que filtrar. Eso es lo que habilita `PT-136`.

---

## M-17 — el núcleo de datos personales (`PT-128`, `PT-133`)

**1200 pruebas en verde.** La sección más sensible del sistema no tenía nada: manifiestos,
minimización, registro de consultas. Se construyó el núcleo que hace cumplibles `RN-51` y
`RN-52` — sin él, las otras ocho pantallas de M-17 no tendrían sobre qué operar.

### La frase que gobierna todo el módulo

> *«Un dato que no se captura no se puede filtrar, no se puede publicar por error y **no se
> puede pedir por hábeas data**.»* — `RN-51`

Por eso el catálogo del manifiesto es **cerrado** —identificación, institución o condición que
motiva el traslado, origen y destino— y todo lo demás exige decir por qué.

### ⚠️ Activar un campo sensible **no se bloquea: se marca**

Va contra la intuición, y es lo que `HU-112` pide textualmente. Bloquear parece más seguro y
es peor: **quien necesita el dato hoy lo va a capturar igual** —en observaciones, en una
libreta, en un mensaje— y ahí queda fuera de todo control.

Marcado, el dato está dentro del sistema, con su acceso registrado, y aparece en el reporte
que revisa Auditoría Interna hasta que alguien lo fundamente.

**Y el fundamento exige las dos mitades.** La base legal sola autoriza capturar todo lo que la
norma no prohíba —que en un país sin ley de datos es casi todo—. La pregunta que limita de
verdad es la otra: *¿para qué operación del traslado hace falta este dato?* Hay campos que no
la pueden contestar.

La pantalla ofrece además **la salida que evita el problema**, arriba de la lista: si lo que
hace falta es operar el traslado —una camilla, un acompañante—, se registra como requerimiento
operativo **sin consignar el diagnóstico**. La necesidad se satisface sin capturar el dato.

### `PT-133` — la única respuesta posible a un hábeas data

> *«Si una persona pregunta quién accedió a sus datos, la única respuesta defendible es el
> registro de consultas. **Sin él, la institución no puede afirmar nada.**»* — `RN-52`

Y no poder afirmar nada no es quedar en empate: es **no poder demostrar que no hubo acceso
indebido**.

| Decisión | Por qué |
|---|---|
| El registro guarda **qué se mostró**, no sólo qué se abrió | Ver una lista de nombres y ver el manifiesto completo son dos accesos distintos al mismo registro |
| El **recuento no exige motivo** | Cuántas personas van es dato de gestión. Pedir justificación para verlo vuelve el control un trámite que se aprende a saltar escribiendo cualquier cosa — y ahí el registro entero deja de valer |
| Los accesos **sin motivo declarado se cuentan aparte** | Es la medida de cuánto del registro **no se puede auditar**: queda el rastro de quién miró y ninguna forma de juzgar si debía |
| «Marcado» **no acusa** | Un despachador que abre veinte manifiestos un lunes está trabajando. El reporte pone el número delante de alguien para que pregunte — uno que acusa se deja de leer tan rápido como uno que calla |

**Nadie está exento**, y ése es el punto: el administrador del sistema es justamente quien
podría borrar su propio rastro. En la prueba en vivo, `P-ADMIN` intentó ver la lista de
nombres sin declarar para qué y **se rechazó con 409**.

### La separación estructural, que no es un detalle de esquema

El registro de consultas vive en **tabla aparte** del manifiesto, a propósito: depurar los
datos personales al vencer su plazo (`PT-137`) **no puede borrar el rastro de quién los
consultó**. Si vivieran juntos, la depuración destruiría la única respuesta ante un hábeas
data — justo sobre los datos más viejos, que son los que más probablemente se reclamen.

Es la misma separación que `RN-51` exige entre datos personales y datos de gestión, y la que
permitirá `PT-136` —exportación de transparencia— **por origen y no por filtrado**.

`[C]` Sigue sin decidirse: no hay ley de datos personales vigente y `DP-001 D-14` decidió **no
diseñar para anticiparla**. Lo construido responde al hábeas data del Artículo 182, que sí
está vigente, y al control de acceso que el MARCI exige.

---

## `PT-056` y `PT-057` — **sección 2.9 cerrada**, y las 1170 en verde

M-16 completo: las seis pantallas de sincronización y conflictos. Y **SAC cedió del todo**,
así que corrió la suite entera —incluidas las 191 de punta a punta que la vez pasada no se
habían podido ejecutar— con **1170 en verde y cero fallos**.

### `PT-057` — el hecho que llega tarde toma uno de dos caminos, y ninguno es el descarte

`RN-45` lo nombra como *«el caso más frecuente y el que más tienta a implementar un descarte
automático»*. El destino depende **del estado de la misión**, no del hecho:

| La misión está | El hecho va a | Por qué |
|---|---|---|
| `LIQUIDADA` | La cola de conflictos | La cifra ya se emitió pero el expediente vive. De ahí sale un asiento de diferencia, y **la liquidación original queda íntegra** |
| `CERRADA` · `CERRADA_CON_HALLAZGO` | **Hallazgo posterior** | No se reabre. Reabrir haría que *«un reporte ya emitido cambie de contenido a espaldas»* de quien lo firmó |
| Cualquier otro | La cola | Es una divergencia común |

`CERRADA_CON_HALLAZGO` cuenta igual que `CERRADA`: tener un hallazgo previo no vuelve editable
el expediente — lo vuelve **un expediente cerrado con más historia**. Tratarlo distinto abriría
la puerta a reabrir por la vía de acumular hallazgos.

**Antes de esto, ese hecho iba a la cola de conflictos**, donde se le habría pedido a una
persona que «decida» entre dos versiones de algo que ya no se puede tocar — y la única salida
habría sido reabrir, que es justo lo prohibido.

Verificado en vivo: un `T-18` con fecha del hecho **15 de mayo** sobre una misión `Cerrada`
abrió el hallazgo `registro-de-campo-posterior-al-cierre` **fechado el 15 de mayo, no el día
de la sincronización** —`RN-46`; fecharlo el 30 de agosto lo pondría en un ejercicio al que no
pertenece— y la cola quedó en cero.

### ⚠️ `PT-056` destapó que falta medio control

El endpoint sólo cubría ARGOS. `PT-056` pide **los dos espejos**, y el de Talento Humano
**no está construido**: la disponibilidad del motorista **no se verifica contra vacaciones,
permisos ni incapacidades**. `BD-10` la evalúa contra lo que hay en el padrón.

Eso es exactamente el caso que `HU-069` existe para impedir — *«no despachar contra un espejo
viejo que dice que el motorista está activo cuando Talento Humano lo tiene de vacaciones desde
el lunes»*—, sólo que peor: no hay espejo viejo, **no hay espejo**. La pantalla lo declara en
vez de mostrar sólo el que sí existe, que se leería como si todo estuviera verificado.

Y el espejo **degrada, no bloquea**: `RN-50` no admite lectura —*«la operación no se impide: se
marca»*—. Una delegación con cuatro días sin enlace tiene que poder seguir operando.

---

## `PT-052` y `PT-055` — el estado que faltaba en `HU-067`

> **Verificado después de comitear.** El bloque se comiteó con autorización del PO sin haber
> podido correr la suite —Smart App Control llevaba hora y media bloqueando el ensamblado—.
> Cuando cedió, esto es lo que salió:
>
> | | |
> |---|---|
> | **969 pruebas en verde** | Todo el dominio, incluidas las 44 de M-16 |
> | **191 no se ejecutaron** | ⚠️ **No fallaron: no corrieron.** SAC seguía bloqueando `Sigti.Api.dll`, y son todas las de punta a punta, que lo cargan |
> | **0 fallos de código** | Se comprobó uno por uno: los 191 mensajes son el mismo `0x800711C7` |
> | **Snapshot correcto** | Un `migrations add` de prueba salió con `Up()` **vacío**, que es la señal de que el modelo y el snapshot coinciden |
>
> **Queda pendiente:** las 191 de punta a punta, y el circuito de retención en vivo —que un
> hecho quede retenido, se aplique solo cuando llegue su expediente, y pase a la cola si al
> llegar sigue sin entrar—. Las dos necesitan que SAC ceda con `Sigti.Api.dll`.

### ⚠️ Faltaba un estado que `HU-067` exige

La historia enumera **cuatro** respuestas por registro, y una no existía:
`EN_ESPERA_DE_PREDECESOR`. *«Dado que llegó el registro 41 y no llegó el 40 — **no lo aplica ni
lo rechaza**.»*

Hoy el hecho cuyo expediente no ha llegado **se rechazaba**, y el propio mensaje admitía que el
problema era de orden: *«tiene que sincronizarse antes que sus transiciones»*. O sea que
reintentar **sí** lo arregla — pero el rechazo lo devolvía al dispositivo sin decirle cuándo, y
el hecho capturado en campo se perdía igual.

**Y con la cola del bloque anterior habría sido peor:** sin distinguirlo, ese caso acabaría
encolado como conflicto, pidiéndole a una persona que **decida sobre algo que no es una
discrepancia** — sólo llegó en desorden.

Ahora el hecho se **retiene**, y al llegar lo que faltaba se aplica solo, **en orden del
hecho** — aplicarlos por orden de llegada produciría un retorno antes que su salida. Si el
expediente llega y aun así no entra, entonces sí pasa a la cola: ahí ya dejó de ser un hueco
de orden.

### Los intentos, que es lo que hace útil el panel

**Un registro con veinte intentos no espera un predecesor: espera algo que no va a llegar.**
Sin ese número, un hueco permanente se ve idéntico a uno que se cierra mañana, y nadie lo mira
hasta que el motorista pregunta por qué su registro nunca entró.

Por lo mismo, `PT-052` **no mezcla lo retenido con los desacuerdos**: lo primero se resuelve
solo, lo segundo espera a una persona. Juntarlos haría que alguien intentara «resolver» un
hueco de orden.

### `PT-055` — el lote va al final, no arriba

Exige criterio escrito —*«hacerlo sin declararlo es sobrescritura con más pasos»*— y se ofrece
**después** de la cola: ponerlo primero invita a resolver sin mirar. Es por misión, porque
«aceptar la versión de campo» sólo significa algo dentro de un expediente.

### ⚠️ La migración se escribió a mano

`dotnet ef` no arranca con SAC activo, así que `20260830022000_HechosRetenidos` —la migración,
su `Designer` y el bloque del snapshot— **están escritos a mano** con el formato que EF genera,
y la tabla se creó **por SQL directo**, registrándola en `__EFMigrationsHistory` con la misma
versión de producto que las demás.

La base está correcta **y el snapshot también**: un `migrations add` de prueba salió con
`Up()` vacío. Lo único que EF corrigió al normalizarlo fue **el orden** — había quedado antes
de `FilaDeConflicto` y el orden canónico es alfabético. Mismo contenido, sin efecto.

### Nota sobre el diagnóstico de SAC

Durante la espera, **el propio bucle de reintentos tenía tomado el dll** y hacía fallar la
compilación con `MSB3021` — «archivo en uso», que **no es SAC** y despista. Si aparece ese
error, revisar procesos `dotnet` colgados antes de culpar a Control de aplicaciones.

---

## M-16 — la cola de conflictos (`PT-053`, `PT-054`)

El mapa la llama *«la pantalla más difícil del sistema y la que nadie diseña hasta que ya
duele»*. No existía nada de M-16 en pantalla.

### ⚠️ El hecho capturado en campo se perdía

La sincronización **ya detectaba** las divergencias y las devolvía como rechazos con motivo
legible. El comentario de `HechoRechazado` decía, textual: *«el motivo tiene que ser legible:
alguien va a leerlo en una cola de conflictos»*. Y el `catch` prometía que el hecho *«queda
declarado para que alguien lo resuelva, en vez de desaparecer sin rastro»*.

**La cola no existía.** El rechazo viajaba en la respuesta HTTP y desaparecía en cuanto el
dispositivo la procesaba — así que el hecho **sí desaparecía sin rastro**, que es exactamente
lo que `RN-45` existe para impedir: *«ambas versiones deben conservarse»*.

Es la tercera vez en cuatro bloques que aparece la misma forma: **algo escrito, correcto, y
sin el otro extremo conectado.** El alcance de datos que no filtraba, `ExigirIntacto` que
nadie llamaba, y ahora un rechazo sin cola donde caer.

### El caso que define el diseño

> *«El motorista anotó odómetro 93,610 el 16 de mayo con foto del tablero; la delegación
> digitó 93,061 el 28 de mayo con foto del original. **Los dos son de buena fe. Uno de los dos
> está mal, y la diferencia son 549 kilómetros** que van a entrar en una conciliación de
> combustible.»*

Por eso ninguna resolución automática es aceptable, y por eso la pantalla muestra **tres datos
por versión**: quién la capturó, cuándo ocurrió el hecho y cuándo se registró. La distancia
entre los dos últimos es *«exactamente lo que permite decidir»* — una versión anotada en el
momento pesa distinto que una digitada del papel doce días después. En la prueba en vivo esa
distancia salió de tres meses.

### Las cuatro cosas que la pantalla se niega a hacer

| | Por qué |
|---|---|
| **No edita** | `R-6`. Y el usuario **va a buscar ese botón**: está puesto, y al pulsarlo contesta *«no se edita un registro; elija entre las versiones que existen»*. Omitirlo no evita que lo busque, sólo que lo busque más tiempo |
| **No combina** | Dos versiones que difieren en campos distintos son **dos conflictos**. Fusionarlas produciría un registro que nadie capturó |
| **No resuelve sola** | Lo que diverge son odómetros, galones y montos |
| **No habla de datos** | Ni *merge*, ni *timestamp*, ni *hash divergente*. Criterio de aceptación literal de `HU-068`, con prueba que lo verifica: quien la usa «no entiende de sincronización y no tiene por qué» |

### El lote, y lo que nunca entra en él

Resolver mil conflictos uno por uno no lo hace nadie; hacerlo sin declarar el criterio es
**sobrescritura con más pasos**. Así que el lote existe, exige criterio escrito, y **excluye
siempre odómetro, monto y autorización** — los tres que terminan en una conciliación contable.

La respuesta los **enumera siempre**, aunque estén vacíos: un lote que dice haber resuelto
«todo» sin mencionarlos hace creer que la cola quedó limpia, y los que frenan liquidaciones
siguen ahí sin que nadie los mire.

**1145 pruebas en verde** (29 nuevas). Verificado de punta a punta contra la base: un retorno
de campo rechazado **entró a la cola** en vez de perderse, se mostró lado a lado, el motivo de
tres letras se rechazó, la resolución quedó con autor y motivo, y **resolverlo dos veces se
bloqueó** — porque la segunda decisión pisaría a la primera sin dejar rastro, que es la misma
sobrescritura silenciosa cometida desde la propia cola.

`PT-052` y `PT-055` tienen su API (`/conflictos/por-dispositivo` y `/conflictos/lote`) y
**todavía no tienen pantalla**. `PT-056` y `PT-057` no se tocaron.

---

## `PT-009` y `PT-010` — lo que la jefatura necesita ver antes de firmar

Las dos van **dentro del expediente en decisión**, no en pantallas aparte. El mapa de
navegación las cuelga ahí, y `R-8` dice por qué: *«todo total tiene su desglose a un toque —
un total opaco no se puede autorizar ni conciliar»*. Una jefatura que tiene que navegar para
ver el costo **autoriza sin verlo**.

### Casi todo el trabajo ya estaba hecho

El estimado de peajes se congela al programar y se guarda punto por punto; el calendario ya
sabía resolver días y horas inhábiles, con sus nulos bien declarados. Lo que faltaba era
**exponerlo y mostrarlo** — dos consultas y dos paneles.

Vale anotarlo porque es lo contrario del patrón de los últimos bloques: acá no había una
regla escrita y sin llamar, sino una regla escrita, llamada, y **sin superficie donde se
viera**. El efecto para quien usa el sistema es el mismo.

### `PT-010` señala y no bloquea, y esa es toda la historia

`HU-006` lo dice en su título. El permiso de la máxima autoridad se gestiona después y
`BD-04` lo exige al despachar: bloquear en la solicitud adelantaría un control de otro
momento y **dejaría al solicitante sin poder ni pedir lo que ya sabe que necesita permiso**.
El panel usa tono de aviso, nunca de bloqueo — `R-4`: si los dos se parecen, se dejan de leer
los dos.

**Y declara qué mitad de `BD-04` no se pudo mirar.** Son dos y fallan por separado:

| Falta | Consecuencia |
|---|---|
| Feriados (insumo #14) | El calendario **subdeclara**: dará por hábil un 15 de septiembre |
| Horario hábil (insumo #1) | La hora **no se evalúa** — es la mitad que decide si salir a las cinco de la mañana exige salvoconducto |

Un panel que muestre «ningún tramo inhábil» sin decir cuál mitad no se miró **afirma algo que
nadie comprobó**. Hoy faltan las dos, y el panel lo dice cada vez.

### `PT-009`: lo parcial se dice, y se paga en efectivo

Las líneas sin valorar —sin tarifa cargada o sin categoría resuelta— **no se suman como cero**
y el total se marca `parcial`. Un total parcial presentado como completo subestima el costo, y
eso termina con **el motorista llegando a una caseta con menos de lo que necesita**.

Y se muestra el estimado **congelado**, no uno recalculado: si una tarifa cambió después, el
número que se autorizó sigue siendo el que se ve — que es lo que hace explicable la
liquidación dos años más tarde.

**1116 pruebas en verde**, y verificado en el navegador contra una misión real: **L 88.00
desglosados en Comayagua y Zambrano**, y **sábado y domingo señalados** dentro de la ventana
con holgura, con las dos mitades faltantes declaradas.

---

## El folio institucional y el congelamiento (`PT-006`, M-06)

El folio real **no existía en ninguna parte**: toda pantalla mostraba `PROV-XXXXXX`. Y el
congelamiento de `HU-004` tampoco. Los dos son de `T-02`, así que van juntos.

### El congelamiento: qué se autorizó, y no qué se ve hoy

> *«En papel, el expediente que firma la jefatura es el que tiene enfrente. En un sistema sin
> congelamiento, el solicitante puede cambiar el destino, la carga o la fecha después de que
> la firma quedó registrada, y la autorización pasa a amparar algo que nunca se autorizó.»*

Y no hace falta mala fe: basta con corregir una fecha «para que quede bien». Al enviar se
calcula la huella del contenido sometido; al autorizar se coteja **antes** de la transición —
después ya habría un acto de autoridad asentado que habría que reversar.

**Nulo no es «coincide»**: son los expedientes anteriores al congelamiento, y sobre ésos no
se puede afirmar nada. No bloquean —negarles la autorización detendría trabajo legítimo por
una función que no existía cuando se capturaron— y el cotejo los declara `SinCongelar`.

Verificado en vivo: se alteró el destino en la base después del envío, y la autorización se
detuvo con `RN-04` diciendo la salida concreta — devolver para corrección, no editar el
expediente «para dejarlo como estaba», **que destruiría la evidencia del cambio**.

### El folio: rangos pre-asignados, y el caso que rompe

`RNF-21` fija cuatro ceros —duplicados, reciclados, colisiones, huecos sin explicar— y nombra
la prueba que *«realmente rompe»*: **tres dispositivos de la misma delegación**, los tres
desconectados, emitiendo el mismo tipo de documento. La delegación **no alcanza** como unidad
de reserva: los tres emiten sin verse y la colisión aparece al sincronizar, con el papel ya
entregado en una caseta. Por eso el rango admite subrango por dispositivo, y hay una prueba
con ese nombre.

El contador avanza sobre lo **emitido**, no sobre lo vigente: anular el folio 5 no hace que el
siguiente vuelva a ser 5. *Un correlativo con huecos es normal; uno reutilizado es un
expediente que sustituye a otro.*

**El formato no se inventa.** `RNF-21` dice que «no se decide por inferencia» (insumo #34):
sin plantilla configurada no se compone folio, y se sigue mostrando el provisional **marcado**
como tal. Un `OM-CHO-2026-000123` plausible acabaría citado en un descargo.

### ⚠️ Seis llamadores componían el folio a mano, y divergieron

`FolioProvisional` era `internal` y su propio comentario lo advertía: *«dos copias del mismo
folio son dos folios que van a divergir el día que llegue el circuito real»*.

**Llegó, y divergieron.** El expediente al que se le emitió `OM-Delegacion de
Choluteca-2026-000001` seguía apareciendo como `PROV-3MNQS7` en el buscador, el despacho, la
ocupación y el seguimiento — porque esos seis seguían llamando al provisional. Se detectó
mirando la pantalla, no el código.

Ahora `FolioProvisional` es **privado** y el único camino es `Folio(fila)`, que sabe cuál de
los dos corresponde. El compilador impide que vuelva a pasar.

### ⚠️ `ExigirIntacto` estaba escrito y sin llamar

La regla del congelamiento existía en el dominio con su prueba en verde, y **ningún endpoint
la invocaba**: `aprobar` usaba el helper genérico de transiciones. Una regla probada que nadie
llama se ve igual de verde que una que funciona. Ahora `T-05` tiene endpoint propio, por lo
mismo que `T-14` y `T-19`: hace algo más que mover el estado.

**1116 pruebas en verde** (31 nuevas), y el circuito verificado de punta a punta contra la
base: rango asignado, solape rechazado nombrando el dispositivo, folio emitido, contenido
congelado, alteración detectada, y `BD-01` llegando con su camino de salida — que cierra
también la verificación que `PT-004` había dejado pendiente.

`[C]` **Insumo #34** sigue abierto: formato del correlativo y umbral de aviso. Se sembraron
valores de prueba en desarrollo (`OM-{delegacion}-{anio}-{numero}` y 20 %) **que no son
decisión de la institución**.

---

## `PT-004` — el patrón de bloqueo duro. **Sección 2.1 cerrada**

Las cinco transversales están: `PT-001`, `PT-002`, `PT-003`, `PT-004` y `PT-005`.

### `R-3` pide tres partes y sólo había dos

> *«Una pantalla de bloqueo tiene siempre tres partes: qué se impidió · por qué exactamente,
> con nombres y números · cuál es el camino de salida.»*

Los mensajes del dominio ya cubrían las dos primeras —dicen la placa, la categoría que falta,
el saldo y el monto—. **La tercera no la tenía nadie.** Y es la que decide qué pasa después:
*«un mensaje genérico produce una llamada a soporte; un mensaje preciso produce la acción
correcta»*. Sin ella, quien queda bloqueado sabe que no puede seguir y no sabe a quién buscar,
así que busca a quien tenga más cerca — y con frecuencia esa persona tampoco puede.

`ReglasDeLaSalida` transcribe el camino de las **trece** precondiciones de §10.2 sección 4,
desde su ficha. Ninguno inventado: un camino inventado manda a alguien a una oficina que no
resuelve nada, y lo manda **con la confianza de estar leyendo al sistema**.

Lo que no está documentado **devuelve nulo y la pantalla lo dice**. Rellenarlo con
«comuníquese con el administrador» convertiría el silencio en una instrucción, y sería falsa:
`ACT-01` no tiene acceso al negocio y no puede resolver un bloqueo de negocio. Hay una prueba
que lo impide — ningún camino puede nombrar al administrador ni a soporte.

### ⚠️ `W-xx`: siete bloqueos compartían un identificador comodín

Siete `throw new BloqueoDuro("W-xx", …)` en el estado operativo del vehículo. **`PT-004`
muestra ese identificador en pantalla**, y «W-xx» no le dice a nadie qué regla lo detuvo, no
se puede rastrear contra la autoridad y no se le puede documentar una salida. Resueltos:

| Dónde | Ahora emite | Por qué |
|---|---|---|
| `ExigirQuienLaFija` ×2 | `transicion.Id` | El dato estaba ahí y no se usaba |
| Causa obligatoria al declarar | `RN-60` | Su propio mensaje ya citaba la regla |
| Terminal según el régimen ×2 | `RN-62` | Es el hallazgo `HB3-17` |
| Transición inexistente · baja con misiones abiertas ×2 | `§10.2` | **No se les inventa un `W-nn`**: los de la tabla nombran transiciones que existen, y estas se disparan justo cuando no hay ninguna |

**Y quedó una guarda de arquitectura** que recorre `src/` y falla si algún bloqueo vuelve a
usar un identificador comodín. Verificada por mutación: reintroducir un `W-xx` la rompe.

### ⚠️ El identificador del servidor no coincidía consigo mismo

La rama de `CambioDeEstadoInvalido` en la API devolvía `precondicion = "10.2"` y el dominio
emite `"§10.2"`. **Dos textos distintos para la misma precondición**, así que el camino de
salida nunca se habría encontrado — el 409 llegaba sin la tercera parte y nadie sabría por qué.
Ahora la API usa la constante del dominio.

### El bloqueo dejó de ser un aviso que se va solo

`R-3` dice **«es una pantalla, no un cartel rojo»**, y la autorización lo mostraba con
`avisar.error(...)`: un toast que desaparece antes de que alguien pueda leer la placa o el
monto, y del todo si la persona parpadeó. Ahora se **retiene** hasta que se resuelva.

El componente **no ofrece ninguna acción que avance** — es la otra mitad de `R-4`: la
advertencia sí deja seguir cobrando el peaje de un motivo escrito, y el bloqueo no. *«Si se
parecen, el usuario deja de leer ambos.»* La única acción es volver.

**Las demás pantallas siguen mostrando el bloqueo como aviso flotante** — Puestos, Padrón,
Títulos, Programación. Queda dicho, no hecho.

### Por qué el patrón tiene pantalla propia

`PT-004` no tiene datos: se aplica dentro de otras pantallas. Pero un patrón que sólo existe
repartido **no se puede revisar** —nadie contesta «¿cómo se ve un bloqueo?» sin provocar uno—
y las siete historias que lo citan no tendrían nada que señalar. En `/bloqueos` se juzga el
texto **antes** de que alguien quede detenido frente a él en el predio a las seis de la mañana.

**1085 pruebas en verde.** Una prueba existente atrapó el cambio de `W-xx` —fijaba ese literal
como esperado— y se actualizó a la constante.

⚠️ **Verificación en vivo incompleta**: el catálogo `GET /bloqueos` se probó y responde con las
catorce precondiciones. El 409 con su camino de salida y la pantalla en el navegador **no se
alcanzaron a ver**: Smart App Control volvió a bloquear el binario recién compilado. Se
destraba solo — ver la sección de SAC más abajo.

---

## Bloque 4: el puesto vigente y su alcance (`PT-001`, `PT-002`, `PT-005`)

Cierra las transversales de la sección 2.1 salvo `PT-004`. Y destapó **la brecha más grande
que había en el sistema**.

### ⚠️ El alcance de datos estaba modelado y no filtraba nada

`AlcanceDeDatos` existía desde `M-01` con sus cuatro niveles, se otorgaba en cada
competencia, se guardaba y se podía consultar. **Y no aparecía en una sola consulta de
expedientes.** Las doce menciones del identificador en la API eran la siembra de
competencias; ninguna filtraba.

El efecto: **toda pantalla de lista mostraba los 28 expedientes a cualquier puesto**. La
pantalla que lo destapó es `PT-005`, que en el inventario se llama, literalmente, «Buscador
de expedientes **con alcance de datos aplicado**» — la última frase del nombre era la que
faltaba.

Medido en vivo, ya aplicado:

| Puesto | Nivel | Ve | Fuera |
|---|---|---|---|
| Jefe de Transporte | `Institucion` | 28 | 0 |
| Encargado de Delegación Choluteca | `Delegacion` | 9 | 19 |
| Custodio de flota | `Propio` | 0 | 28 |
| Jefatura Administrativa | `Dependencia` | 0 | 28 |

### `ReglasDelAlcance` falla cerrado, y esa es toda la clase

Cuando el alcance **no se puede resolver** —el puesto no está en el espejo de `ACT-16`, o
tiene alcance de delegación y es de sede— devuelve **nada** y dice por qué. Nunca todo.

Un control de acceso que ante la duda abre no es un control: funciona mientras nada falle, y
lo que falla es justamente el espejo, que viene de otro sistema. **Fallar cerrado hace que
alguien vea una lista vacía y llame; fallar abierto hace que vea los expedientes de toda la
institución y nadie se entere.**

Por eso `SePudoResolver` viaja con su `PorQueNo`: «no ve nada por permiso» y «no se pudo
saber qué ve» se ven idénticos en pantalla —una lista vacía— y sólo uno es correcto.

### ⚠️ `Rolando Discua` estaba cableado en nueve lugares

Cada acción que exige actor pasaba ese nombre **escrito a mano**: autorizar, programar,
despachar, desprogramar, anular, declarar el estado de un vehículo.

El daño obvio es que nada de lo que el sistema registra decía quién lo hizo. **El que
importa es otro: dejaba inerte toda la segregación de funciones.** `I-01` a `I-19` comparan
al actor de un acto contra los actos previos del mismo expediente — y si el actor es siempre
la misma constante, lo que comparan no es a nadie. Se construyó el control bloqueante
completo en el bloque de `M-01` y desde la oficina no podía disparar correctamente.

Ahora sale de `usarQuienEjecuta()`, que **lanza** si no hay puesto elegido. Devolver un valor
por omisión sería volver al mismo problema, y un asiento con el autor equivocado no se
corrige después: sólo se reversa.

### `PT-001` no finge autenticar

No hay contraseña ni verificación: elegir a otra persona no pide nada. **La pantalla lo dice
en un aviso**, en vez de disimularlo con una caja de contraseña que no valida. Una pantalla
que *parece* autenticar es peor que una que declara no hacerlo, porque hace creer que hay un
control donde no lo hay.

Mientras siga así, **el alcance filtra pero no protege**: el servidor recibe la persona como
parámetro y la cree.

### Lo que las pantallas declaran en vez de inventar

- **El mapa de navegación no declara raíz** para `ACT-11`, `ACT-13` ni `ACT-14`.
  `ReglasDeLaRaiz` devuelve nulo y la pantalla lo dice: elegirles una decidiría en el código
  algo que el diseño no decidió. Se ve en vivo con `P-TRANSPORTE`, que ocupa dos puestos y
  cuyo puesto de custodio entra sin pantalla propia.
- **«Pendientes de mi firma» no tiene `PT-xxx`** — el mapa la describe sin darle uno, y no se
  le inventa: los identificadores los declara el inventario y no se reciclan.
- **Un puesto con dos competencias tiene dos raíces**, y cuál manda no se decide acá: sería
  inventar una política de la institución y aplicarla en silencio a todos los puestos.

`[C]` **Insumo #104**: el corte por objeto de §3.3 no está modelado, y lo que hay hoy es
**más permisivo** que la regla.

**1067 pruebas en verde** (29 nuevas), y el flujo recorrido en el navegador: elegir puesto,
ver pendientes, y el mismo buscador devolviendo conjuntos distintos según el puesto.

---

## Bloque 3 de las 63: el seguimiento en ruta (M-19)

**`PT-058` y `PT-059`, con el módulo entero detrás**: no había `M19` en el dominio, ni
grupo `/seguimiento` en la API, ni tabla. El requisito estaba completo en el título de la
historia — *«mostrar la última posición conocida con su antigüedad, **nunca como si fuera
actual**»* — y eso es lo que gobernó el diseño.

### La antigüedad viaja con el dato, no la pone la pantalla

«Última posición conocida hace 10 h 40 min» y «última posición conocida» llevan al Jefe de
Transporte a decisiones distintas, y **la segunda afirma algo falso sin decir ninguna
mentira**. Por eso `ReglasDeLaFrescura` está en el dominio y no en el componente: si el
formato lo decidiera cada pantalla, tarde o temprano una lo omitiría.

**Cinco grados, y tres de ellos son formas distintas de no tener el dato:**

| Grado | Qué dice |
|---|---|
| `Fresco` | Dentro del umbral que la institución fijó |
| `Degradado` | Fuera del umbral. **No es una alarma** — el silencio es lo esperado |
| `NoSeClasifica` | Hay dato y no hay umbral. La antigüedad se muestra igual |
| `RelojAdelantado` | El hecho está fechado en el futuro |
| `NuncaHuboDato` | Ninguna declaración. **Distinto de una muy vieja** |

⚠️ **La antigüedad negativa no se aplasta a cero.** `Math.Max(0, ahora - hecho)` es la salida
cómoda y hace el peor daño posible: el dispositivo con el reloj roto aparecería como **el dato
más fresco del tablero** — justo el menos confiable, presentado como el mejor.

### Dos defectos que sólo aparecieron con datos reales

⚠️ **El tablero filtraba por la fecha planificada de salida.** El filtro venía copiado del
tablero del día de despacho, donde es correcto —aquel organiza el día por lo planificado—; acá
la pregunta es otra: *«¿qué vehículos están afuera AHORA?»*, y eso lo dice el diario (`P-1`).
La misión de prueba **tenía asiento `T-14`** y salida planificada para el 1 de septiembre: el
vehículo estaba circulando y el tablero lo escondía. El corte que sí corresponde es haber
salido alguna vez.

⚠️ **El tablero decía «sin estado declarado» cuando el motorista sí había declarado.** Tomaba
el último reporte por hora del hecho, y ése era un *arribo* —que no lleva estado—, aunque una
hora antes se hubiera declarado «en espera». **Son dos preguntas y hacían falta dos datos:**

| Pregunta | La contesta |
|---|---|
| ¿Cuándo supimos de él por última vez? | Cualquier reporte — un arribo es señal de vida |
| ¿Qué es lo último que declaró? | Sólo un reporte que traiga estado, **con la hora de ése** |

El detalle ya lo resolvía bien, así que las dos pantallas se contradecían. Y el efecto era
exactamente lo que `RN-76` existe para impedir: **afirmar que el motorista no declaró**.

### El tiempo en sitio se deriva, y dice cómo lo supo

`RN-76` prohíbe pedirle al motorista que lo cronometre. Un tiempo digitado es un tiempo
redondeado a la media hora, siempre a favor de quien lo digita, y no serviría para atribuirle
un costo a nadie — que es para lo que se mide. Tres modos de cerrar una estadía:

- `Declarada` — el motorista declaró la salida
- `DerivadaDelSiguienteEvento` — no la declaró y se dedujo. **Va marcada**: un tiempo deducido
  no puede leerse con la misma confianza que uno declarado
- `SinCerrar` — sigue en el sitio. El reloj corre, pero la salida es nula

Y una **salida sin arribo es un hueco visible**: rellenarla con la hora de la salida produciría
una estadía de cero minutos que se leería como «no esperó nada», lo contrario de «no sabemos
cuánto esperó».

### Espera ≠ espera improductiva, y `null` ≠ productiva

`EsImproductiva` es **`bool?`**. Nulo cuando no hay causa declarada, o cuando el catálogo no
está poblado. Colapsarlo a falso reportaría **cero horas improductivas** cuando lo que pasa es
que nadie las tipificó — la cifra más tranquilizadora posible, y falsa. Por eso `SinTipificar`
va **siempre al lado del total**.

`[C]` **Insumo #103**: cuáles causas cuentan como improductivas es decisión de la institución,
no del equipo — la clasificación asigna responsabilidad a una dependencia.

### Lo que el grupo NO hace

**No infiere nada del silencio.** No cierra misiones por inactividad, no marca interrupciones,
no deduce que un vehículo se detuvo, y **no hay ningún indicador de «en línea»** — `HU-057` lo
prohíbe en un escenario aparte. Con la cobertura que hay en el país, un punto verde
parpadeando convertiría la falta de señal en una alarma, y **las alarmas que suenan siempre se
dejan de mirar**.

El desfase entre el hecho y la captura tampoco es un error: **mide cuánto estuvo el
dispositivo sin cobertura** (`RN-43`), y la pantalla lo dice cuando pasa de una hora. En la
verificación en vivo los cinco hitos llegaron con 50 a 56 horas de desfase — el caso real.

**1038 pruebas en verde** (51 nuevas), verificado contra la base con una misión de dos
destinos: bloqueos de catálogo, de ventana, de `(0, 0)` y de destino faltante, los cuatro
disparando con su mensaje.

---

## Bloque 2 de las 63: la auditoría de `ACT-12` (M-14)

**Cuatro pantallas: `PT-003`, `PT-088`, `PT-089` y `PT-092`.** La primera ya estaba construida
sin que nadie lo supiera.

### ⚠️ `PT-003` ya existía, y se descubrió mirando el inventario

La bandeja de tareas escaladas de §5.3.B.3 **es** «Bandeja de tareas escaladas por segregación
de funciones» — el nombre del inventario, palabra por palabra. Se construyó desde la
especificación sin cruzarla con el inventario, y quedó sin registrar.

**Es el mismo patrón que el mapa de pantallas ya había destapado:** trece pantallas
construidas sin citar su `PT`. Mirar el inventario antes de construir, y no sólo después,
evita duplicar o dejar huérfano lo que ya está.

### `PT-089` — el rastro «con sus huecos visibles»

Esa frase del inventario **es todo el requisito**. Un rastro que sólo muestra lo que está no
sirve para auditar: lo que el TSC busca es dónde se cortó la cadena.

**Cuatro estados, no dos.** «Falta», «no correspondía» y «todavía no toca» se ven iguales en
una casilla vacía y no son lo mismo. Juntarlos produce los dos daños a la vez:

| Estado | Qué dice |
|---|---|
| `Presente` | Hay asiento, con autor y fecha |
| `Ausente` | **El hallazgo.** Correspondía, se pasó por la etapa, y no hay asiento |
| `NoAplica` | No correspondía — una misión sin fondo no tiene vale |
| `Pendiente` | Todavía no toca. La misión sigue su curso |

*«Completa»* exige **no tener huecos y no tener pendientes**: dar por completa una cadena con
eslabones pendientes cerraría un expediente vivo en el reporte.

### ⚠️ Dos defectos que sólo aparecieron con datos reales

La primera corrida contra la base los destapó de inmediato, y los dos eran **falsos positivos
en una pista de auditoría** — que es lo que hace que se deje de mirar:

| Defecto | Efecto |
|---|---|
| **Liquidar es `T-19`, no `T-20`** | Una misión `LIQUIDADA` salía con la liquidación marcada como hueco. El estado decía una cosa y la cadena lo contradecía |
| La bitácora se juzgaba contra el **despacho** | Toda misión `DESPACHADA` aparecía como hallazgo. La bitácora se abre en `T-14` al iniciar la ruta, no al despachar: se juzga contra el retorno |

⚠️ **Y el primero no estaba sólo ahí.** `ActosDeLaMision` —el mapa transición→función de la
segregación— tenía el mismo `T-20`. El efecto no se veía desde el bloqueo de liquidar, porque
ahí la función pretendida se pasa explícita; **se veía del otro lado**: quien ya había
liquidado no contaba como acto previo, y podía autorizar o despachar la misma misión **sin que
`I-07` ni `I-09` dispararan**. Un hueco de segregación que sólo se destapó porque otra
pantalla mostró la misma tabla desde otro ángulo.

**Van cinco errores en mapas de identificadores** —cuatro en el puente rol→función, uno acá—.
Es el punto más frágil del sistema y conviene tratarlo así: **el mapa se verifica contra el
código que emite los identificadores, no contra la memoria**.

### `PT-088` declara lo que le falta

La ficha de `ACT-12` enumera cinco fuentes y hoy existen tres. **Una pista que muestra tres sin
decir que faltan dos se lee como completa**, y quien audite concluiría que no hubo actos en
régimen de excepción cuando lo que pasa es que nadie los registra. La pantalla las nombra.

### `PT-092` — las dos parejas de fechas, completas

El eje **normativo** dice desde cuándo regía; el de **transacción**, desde cuándo lo supimos
(`ADR-006`). Mostrar sólo el primero impediría explicar por qué una liquidación de marzo usó
otro número. Y las versiones **sin aprobar se muestran** diciendo que no rigen: ocultarlas
haría que quien audite no viera lo que espera firma.

**987 pruebas en verde**, y el grupo `/auditoria` **no tiene un solo `MapPost`** — el límite
absoluto de `ACT-12` es sólo lectura y exportación.

---

## El canal de aviso — §5.3.B.3 completo

**Elegir el canal es cargar un parámetro, no tocar código.** Insumo #102 pasa de «no se puede
hacer nada hasta que la institución decida» a «la institución carga una clave».

### Lo que NO se hizo: elegir por la institución

El canal es `[C]`. **No se supuso uno «razonable»** —correo, por ejemplo— porque suponerlo
produce lo peor: un sistema que cree haber avisado y una persona que nunca recibió nada. Sin la
clave cargada, el resultado es `SinCanalConfigurado` y se dice con todas las letras.

### Tres razones distintas por las que un aviso no sale

| Resultado | Quién lo arregla |
|---|---|
| `SinCanalConfigurado` | **La institución**, eligiendo |
| `CanalNoImplementado` | **Quien programa**, construyéndolo |
| `Fallido` | **Quien opera** la infraestructura |

Juntarlas en «no se pudo avisar» mandaría a la persona equivocada a resolver el problema.

### `SoloBandeja` entrega de verdad, y no es un consuelo

Es el único canal implementado, y **es un canal legítimo**: en una delegación sin señal el
correo y el SMS no llegan, y más de dos millones de personas del área rural no tienen internet
(`P-5`). Lo que declara es que **el aviso depende de que la persona entre al sistema** — no que
no se avisó. Marcarlo como fallo diría que el sistema no hizo lo que se le pidió.

Correo institucional y mensaje de texto están en el enum y **no en `Implementados`**:
declararlos antes de construirlos haría que el sistema dijera «entregado» sobre un envío que
nunca salió.

### Medido en vivo, y es la prueba que importa

| Momento | Resultado |
|---|---|
| Bloqueo **antes** de cargar la clave | `SinCanalConfigurado` · *«no es que no contestara: es que nadie le escribió»* |
| Se carga `aviso.canal = SoloBandeja` y **la aprueba otra persona** (`HU-145`) | — |
| Bloqueo **después** | `Entregado por SoloBandeja`, tarea marcada como notificada |

**La tarea anterior conserva su resultado histórico.** No se «arregla» retroactivamente: el
aviso registra lo que pasó en ese momento, y reescribirlo diría que se avisó cuando no.

### Un aviso por destinatario, no por tarea

Un puesto puede estar coocupado durante un traspaso. Una sola fila por tarea diría que se
avisó cuando a una de las dos personas no le llegó.

### ⚠️ Smart App Control, y una corrección sobre lo que se dijo de él

**Lo que se afirmó y era falso:** que el cambio de configuración del PO «necesitaba
reiniciar» para tomar efecto, y que por eso seguía bloqueando.

**Lo que midió la máquina:** el registro siguió en `VerifiedAndReputablePolicyState = 1` todo
el tiempo, y el uptime **nunca se reinició** —pasó de 44.4 a 44.6 horas—. La suite corrió en
verde con **los mismos binarios** que minutos antes estaban bloqueados, sin recompilar nada de
.NET entre una corrida y la otra.

**Conclusión: no fue el reinicio ni el interruptor. Fue el tiempo** — exactamente lo que la
sección «Smart App Control puede volver a bloquear la ejecución de .NET» de este mismo
documento ya decía desde el 2026-08-26.

⚠️ **Y ése es el hallazgo que vale.** Esa sección estaba escrita, con el diagnóstico correcto
y con la advertencia de que **apagar SAC es irreversible y no es la primera opción**. No se
leyó: se re-diagnosticó desde cero durante horas y se terminó recomendando justo lo que el
documento desaconsejaba. **El HANDOFF sólo sirve si se lee antes de diagnosticar, no después.**

**Verificado tras la corrección: 978 pruebas en verde**, cero fallas, incluidas las 8 de
`ReglasDelAvisoPruebas`. El circuito completo se midió además en vivo por la API.

---

## La bandeja de tareas — §5.3.B.3 completo, salvo el canal

**El escalamiento dejó de terminar en un registro que nadie mira.** Ahora el acto bloqueado
*«queda visiblemente pendiente en la bandeja de alguien»*, con contador en el riel.

### La bandeja es el sistema de registro; el aviso es una cortesía

El documento pide las dos cosas y **no son intercambiables**. Un correo que no llega deja el
trabajo perdido y nadie se entera; una bandeja que se abre al entrar **no depende de que haya
red, servidor de correo ni teléfono** — y esto se despliega *on-premise* en instituciones
donde nada de eso está garantizado. Por eso la bandeja se construyó primero.

### No es una bandeja de segregación: es la bandeja

`TipoDeTarea` ya contempla `ReservaEnConflicto` (`RN-60` punto 3) y `PrestamoVencido`
(`RN-63` punto 4). **Las dos venían arrastrando el mismo pendiente** —el HANDOFF lo decía en
dos lugares distintos— y una bandeja específica de segregación las habría dejado esperando
otra vez. Encolarlas es ahora una llamada.

### Lo que impide que el escalamiento sea un trámite

| Regla | Por qué |
|---|---|
| **Quien la originó no la cierra** | La tarea existe porque a esa persona se le impidió el acto. Dejarla cerrarla sería apretar un botón; el escalamiento la puso en otra bandeja para que decida alguien más |
| **Cerrar exige decir qué se hizo** | *«Lo autorizó la Gerencia por oficio 2026-31»* y *«ya no hacía falta»* dejan el mismo rastro vacío si no se escribe |
| **Una cerrada no se vuelve a cerrar** | Dos resoluciones sobre el mismo hecho dejarían dos versiones de qué pasó |
| **«Resolver» y «ya no aplica» son dos botones** | Descartar dice que nadie tuvo que hacer nada. Juntarlos impide distinguir el control que operó del que se volvió innecesario |

**Encolar es idempotente por expediente, tipo y persona.** Quince intentos son quince asientos
en la pista —eso es lo que Auditoría quiere ver— pero **una sola tarea**, porque hay una sola
cosa que resolver.

### ⚠️ El canal de notificación no existe, y el dato lo dice

`NotificadoUtc` es **siempre nulo hoy**: no hay canal construido en ningún módulo. Y nulo
significa **«no se avisó»**, no «se avisó y no contestaron» — la distinción es lo que impide
que una bandeja llena se lea como gente que ignora su trabajo. La pantalla lo dice con todas
las letras: *«no es que no contestaran: es que nadie les escribió»*.

**Qué canal quiere la institución es insumo pendiente.** Correo institucional, SMS, o sólo la
bandeja — y en delegaciones sin conectividad las tres primeras no sirven, así que la respuesta
puede ser legítimamente «sólo la bandeja».

### Un defecto encontrado en vivo

⚠️ La bandeja mostró **«−4 días esperando»**: la tarea se encoló con la fecha del hecho de la
petición, posterior a hoy. **Una tarea cuya fecha del hecho todavía no llegó no ha esperado
nada**, y cero es la respuesta correcta; el negativo se lee como un error del sistema.

### El circuito, verificado de punta a punta

| Paso | |
|---|---|
| `P-ASISTENTE` despacha lo que solicitó | **409 I-02**, y encola |
| La tarea queda dirigida a `PUE-JEFE-TRANSPORTE` (`P-TRANSPORTE`) | sin avisar — no hay canal |
| **La cierra quien la originó** | **409** — *«el escalamiento la puso en otra bandeja justamente para que decida otra persona»* |
| Motivo insuficiente | **409** — *«“lo autorizó el jefe” y “ya no hacía falta” dejan el mismo rastro vacío»* |
| `P-TRANSPORTE` con motivo | **200** |
| Volver a cerrarla | **409** — *«dos versiones de qué pasó»* |

**970 pruebas en verde**, cero fallas. Y los días negativos ya dan **0**.

### La corrida que faltaba: Smart App Control

Durante el trabajo, **SAC bloqueó primero las pruebas y después la propia aplicación** —cada
compilación produce un binario sin firma ni reputación—. **Se destrabó solo, con el tiempo**,
igual que el 2026-08-26: el registro nunca cambió de estado y la máquina nunca se reinició.

Queda anotado porque **el síntoma es reconocible y engañoso**: `FileLoadException 0x800711C7`,
cero pruebas corriendo o fallas masivas **sin una sola aserción rota**. Lo que NO hay que hacer
es limpiar `bin`/`obj` para «arreglarlo»: cada limpieza produce binarios nuevos y lo empeora.

---
## §5.3.B.3 — el escalamiento, con el espejo trayendo la jerarquía

**Los tres saltos operan.** El bloqueo dejó de ser un callejón sin salida.

> *«La misión no queda trabada por un problema de organización: queda visiblemente pendiente
> en la bandeja de alguien.»* Y §5.4: *«bloquear sin alternativa no produce control: produce
> evasión»*.

### El espejo trae dos campos nuevos, y uno es maestro

| Tabla | Dueño | Qué aporta |
|---|---|---|
| `PuestoEspejo` | **ARGOS** | Denominación, unidad, **puesto superior**, delegación |
| `RespaldoDeSede` | **SIGTI** | A qué puesto de sede escala cada delegación |

La segunda no puede ser de ARGOS: **conoce la estructura, no nuestra política de control
interno**. Que Choluteca escale a tal puesto cuando su encargado queda bloqueado por
segregación es una decisión nuestra, no un dato del organigrama.

### Los tres saltos, medidos en vivo

| Caso | Salto | Mensaje |
|---|---|---|
| `P-ASISTENTE` bloqueado por `I-02` | **Puesto superior** | *«Queda pendiente de resolución en Jefe de Transporte (P-TRANSPORTE), que es el puesto superior dentro de su misma unidad»* |
| `P-CHOLUTECA` bloqueado por `I-19` | **Respaldo de sede** | *«… el respaldo de sede de su delegación. Los saltos anteriores no aplicaron: Jefe Regional de Choluteca está vacante»* |
| Sin superior ni respaldo | **Último recurso** | Gerencia Administrativa, con los dos motivos enumerados |

### Cuatro decisiones que las pruebas defienden

**El superior tiene que ser de la misma unidad.** §5.3.B.3 lo dice, y llamar primer salto a un
superior de otra unidad borraría la distinción con el segundo, que es justamente el rodeo por
sede.

**«O está vacante» incluye el caso en que el único ocupante es quien quedó bloqueado.**
Escalarle el acto a la misma persona es un callejón sin salida disfrazado de bandeja, y ocurre
de verdad: §5.4 describe la delegación donde una persona ocupa varios puestos.

**Cada fallo dice por qué.** Un escalamiento que siempre termina en Gerencia Administrativa sin
explicarse se lee como que la jerarquía no sirve, cuando lo que puede estar pasando es que el
puesto superior esté **vacante** — un problema de organización que alguien tiene que resolver, y
que sólo se ve si se dice. Se distinguen cuatro motivos: sin jerarquía en el espejo, sin
superior, superior de otra unidad, y superior o respaldo vacante.

**Nulo en `salto` es «no se resolvió», no «fue a Gerencia».** Los intentos anteriores al
escalamiento existen, y decir que fueron al último recurso sería inventar un encaminamiento que
nunca ocurrió. La pantalla los muestra como *«sin destino registrado»*.

### ⚠️ Lo que sigue faltando: la notificación

§5.3.B.3 pide dos cosas y sólo se entregó una. **El destino se resuelve y queda registrado; a
quien le toca resolverlo no se le avisa** — tiene que abrir `PT-091`. Las notificaciones no
están construidas en ningún módulo del sistema.

### ⚠️ La suite no se pudo correr completa

**199 de 958 pruebas fallaron, y ninguna por aserción**: todas son
`FileLoadException 0x800711C7` — *«una directiva de Control de aplicaciones bloqueó este
archivo»*— sobre `Sigti.Datos.dll` en la salida de pruebas. Es el mismo bloqueo de Windows que
ya apareció en este proyecto; esta vez no se destrabó cambiando de configuración, ni limpiando
`bin`/`obj`, ni construyendo a otra ruta.

**Lo que sí quedó verificado:** las 759 que no tocan la base pasan, incluidas las 10 nuevas del
escalamiento y las 55 de dominio de `M-01`. Y los tres saltos se midieron **en vivo contra la
base por la API**, que arranca sin problema.

**Las 199 hay que volver a correrlas** cuando la política deje pasar el binario. No están
verificadas y no se declaran verdes.

### Un defecto de lectura, corregido

`PT-091` mostraba *«quiso apruebafondo»* — el nombre del enum en minúscula. El servidor lo
publica así a propósito, porque va a la pista de auditoría y tiene que ser estable; **la
pantalla la lee Auditoría Interna**, y ahora dice *«quiso ejercer la aprobación del fondo»*.

---

## `I-19` — aprobar el fondo, y el hallazgo que se cierra

**Nueve pares disparando.** El último de los que tenían dónde hacerlo con lo que hay
construido.

### El fondo necesitaba su propio ensamblador

`ActosDeLaMision` no le sirve: **el fondo es objeto de período, no de misión**. Es exactamente
el motivo por el que `I-19` existe como par propio — *«`RN-01` razona por Orden de Misión y el
fondo es objeto de período, así que este par se caía entre las dos»*. Pasarle los actos de una
misión sería contestar sobre el objeto equivocado: un fondo cubre muchas y ninguna en
particular.

`ActosDelFondo` mapea sólo `F-01` y `F-02`. **Ampliar y cerrar no entran**, y no es una lista
incompleta: no son ninguna de las funciones que el MARCI separa, e inventarles una haría que
un cierre bloqueara una aprobación.

### ⚠️ Un comentario del código que había quedado vencido

`ReglasDelFondo` declaraba un **hallazgo abierto**: *«el par solicita fondo × aprueba fondo no
existe en la tabla `I-01`…`I-17` de actores-y-roles.md»*. **Ya existe: es `I-19`**, incorporado
tras `HN1-15` y `HB3-06`. El comentario quedó corregido y el hallazgo, cerrado.

**`RN-26.4` no se retira.** Sigue siendo control propio de `RN-26` y es la última línea si
alguien llama al dominio sin pasar por §5.3.B. Lo que le faltaba es lo que el control
bloqueante agrega alrededor: **el asiento en la pista, el par nombrado y el escalamiento**.

### Tres defectos del mensaje, corregidos

Y en §5.3.B.1 **el mensaje es el control**, así que valen:

| Decía | Por qué está mal |
|---|---|
| «el fondo de **Dependencia**» | Es el enum, no el ámbito declarado. Quien lee necesita saber **cuál** de sus fondos, no de qué tipo es |
| «solicitud **de el** fondo» | El ensamblador anteponía «de» a una referencia que ya traía preposición. Se lee como un error del sistema |
| «sobre este **expediente**» | Un fondo no es un expediente. Nombrar mal el objeto vuelve sospechoso al bloqueo justo cuando tiene que ser creíble |

Medido en vivo, ya se lee: *«Usted ya ejerció la solicitud del fondo (solicitud del fondo de
Delegacion de Danli, del 01/10/2026 al 31/10/2026 (F-01), el 05/09/2026). No puede además la
aprobación del fondo: es la incompatibilidad I-19.»* Y aprobado por otra persona: **200**.

### El recíproco que la prueba defiende

**Quien solicitó el transporte sí puede aprobar el fondo.** Son objetos distintos, e `I-03`
habla de *entregar* el fondo, no de aprobarlo. Sin ese recíproco, colapsar `Solicita` con
`SolicitaFondo` —que es justo el error que `I-19` existe para impedir— pasaría la prueba
principal.

---

## §5.3.B cableado en despachar y entregar el fondo

**Ocho de los diecinueve pares disparan hoy sobre expedientes reales.** Ayer eran tres.

### `T-12` despachar

Medido en vivo sobre una misión creada, aprobada, programada y despachada de punta a punta:

| Quién despacha | Ya había hecho | |
|---|---|---|
| `P-ASISTENTE` | la solicitud | **409 `I-02`** |
| `P-JEFATURA` | la autorización | **409 `I-05`** |
| José Ramón Cruz | **es quien va a conducir** | **409 `I-11`** · núcleo irreductible |
| `P-ENCARGADO` | nada | **200** |

**El caso del motorista es el que más valía cerrar.** Quien se despacha a sí mismo controla
los dos extremos del acto físico: entrega las llaves y las recibe. Y la comparación es contra
**quien va a conducir según esta petición**, no contra el diario: en el despacho todavía se
está decidiendo.

### `V-02` entregar el fondo

| Quién entrega | Ya había hecho | |
|---|---|---|
| `P-ASISTENTE` | la solicitud | **409 `I-03`** |
| `P-ENCARGADO` | el despacho | **409 `I-08`** |
| Wilmer Alvarado | **es quien conduce** | **409 `I-11`** · núcleo irreductible |
| `P-COMBUSTIBLE` | nada | **200** |

⚠️ **Lo que `BD-06` cubría, y lo que no.** `BD-06` exige que quien entrega no sea quien
emitió: es segregación **dentro del vale**. Los pares que **cruzan el vale con la misión** no
los veía nadie, *porque el vale no conoce el expediente y el expediente no conoce el vale*. El
endpoint entra por el vale, resuelve su misión y compara contra los actos de ésta.

### El helper declara qué función ejerce cada transición

`ConAsignacion` recibe ahora un `Funcion?`. Programar declara `EmiteOrdenDeMision` —hoy sólo
cruza con `I-14`, que está apagado, y se declara igual para que **encenderlo sea un parámetro
y no un cambio de código**—. Reasignar declara **nulo**, y eso dice «ninguna de las cinco», no
«se olvidó evaluarla»: cambiar el recurso de una misión ya programada no es solicitar,
autorizar, despachar, entregar el fondo ni liquidar.

### La pista, después de las pruebas

**9 intentos, 8 pares distintos**, y la reincidencia visible: `P-ASISTENTE` ×3,
`P-ENCARGADO` ×2, `P-JEFATURA` ×2. Es exactamente lo que §5.3.B.2 dice que Auditoría quiere
ver, y hasta ayer no existía.

### Lo que sigue faltando

- **Aprobar el fondo** (`I-19`, solicita el fondo × aprueba el fondo). Hoy lo cubre una
  comparación por nombre dentro de `M-09`; conectarlo al motor lo dejaría con pista y mensaje
  preciso como los demás.
- **El escalamiento sigue sin encolar.** Los dos primeros saltos exigen la jerarquía de
  puestos, que el espejo de ARGOS no trae.

---

## §5.3.B — el control bloqueante, y `PT-091`

**El otro momento del control.** El preventivo mira la acumulación de roles al otorgarlos y
sólo puede rechazar lo absoluto. Éste es donde el documento dice que *«se decide de verdad»*:
hay un expediente concreto y se compara persona contra persona.

### Lo que hasta ayer no bloqueaba en ninguna parte

`BD-01` cubre la autorización de una misión y **nada más**. Los demás pares no tenían dónde
dispararse. Medido en vivo sobre `PROV-00000C`, al liquidar:

| Quién | Ya había hecho | Resultado |
|---|---|---|
| `P-ASISTENTE` | la solicitud | **409 `I-04`** |
| `P-ENCARGADO` | el despacho | **409 `I-09`** |
| `P-JEFATURA` | la autorización | **409 `I-07`** · núcleo irreductible |
| `P-TRANSPORTE` | emitió la Orden de Misión | **200** — `I-14` está apagado |

Esa última fila es la que prueba que el bloqueo no es un «rechaza todo»: `ACT-04` emite y
liquida por diseño, y encender `I-14` por omisión lo dejaría sin operar.

### El mensaje es el del documento, literal

§5.3.B.1 pide precisión — *«un mensaje genérico produce una llamada a soporte; uno preciso
produce la acción correcta»*. Medido: *«Usted ya ejerció el despacho sobre este expediente
(despacho de … (T-12), el 12/03/2026). No puede además la liquidación: es la incompatibilidad
I-09. Quien despacha no declara cómo volvió.»* Y **no lleva nombres de tipos**: quien lo lee
está resolviendo un trámite, no depurando el sistema.

### La pista registra lo que NO pasó — `PT-091`

§5.3.B.2: *«el intento bloqueado es información de control, no ruido»*. Los tres intentos
quedaron con persona, función pretendida, expediente, par, contra qué chocó, referencia,
momento y origen. **El registro ocurre aunque el acto se impida**, y en su propia unidad de
trabajo: dejarlo al manejador de la excepción haría que el rollback se llevara el asiento
justo cuando ocurrió.

La pantalla pone **la reincidencia primero**: *«un mismo usuario intentando quince veces
autorizar sus propias solicitudes es exactamente lo que Auditoría Interna quiere ver»*, y
ordenar por fecha la esconde entre los aislados. Y cuando está vacía **no se lee como un
certificado**: cero no prueba que el control opere, prueba que no se ha necesitado.

### ⚠️ Un cuarto error del mismo tipo: `I-14` era inalcanzable

Lo destapó la prueba que exige **las dos ramas del parámetro configurable**. `I-14` estaba
escrito con las funciones de `I-07` —Autoriza × Liquida—, así que `I-07` disparaba primero por
ser el mismo par y de mayor nivel, **y el configurable no decidía nada nunca**.

Son cosas distintas: `ACT-03` se pronuncia sobre la **procedencia de la necesidad**; `ACT-04`
**emite la Orden de Misión**. Por eso el MARCI separa el primero de la liquidación —`I-07`,
núcleo irreductible— y no dice nada del segundo. Se agregó `EmiteOrdenDeMision`.

Cuatro errores en el puente rol→función en dos turnos, **todos encontrados por pruebas que
exigen las dos ramas o por leer la ficha en vez de suponer**. Es el punto más frágil de `M-01`
y conviene tratarlo así.

### ⚠️ El escalamiento está a medias, y lo dice

§5.3.B.3 pide tres saltos: puesto superior → respaldo de sede → `ACT-08`. **Los dos primeros
exigen la jerarquía de puestos**, y el espejo del organigrama sólo trae persona↔puesto.

El bloqueo nombra el destino que sí puede resolver y **declara qué le falta**, en vez de
inventar un destinatario: la misión quedaría *«visiblemente pendiente»* en la bandeja
equivocada. Y **todavía no encola nada** — la pista registra, no encamina.

Lo que destraba esto es que el espejo traiga `id_puesto_superior` y la unidad organizativa,
que el modelo de §2.2 ya declara y la integración con ARGOS no trae.

### Dónde está cableado, y dónde no

Hoy opera en **`T-20` liquidar**, que es donde convergen cinco pares. Falta cablearlo en
despachar (`T-12`), entregar el fondo (`V-02`) y aprobar el fondo. El motor y la pista ya
existen: cada uno es armar sus `ActosDelExpediente` y llamar.

---

## `M-01` — la segregación de funciones deja de ser un documento

**`PT-096` y `PT-097` entregadas, y con ellas la mitad de `M-01` que le faltaba al sistema.**
La otra mitad —quién ocupa qué puesto— ya existía como espejo de ARGOS desde el bloque 1 del
Sprint 2. Lo que no existía era **el rol**: ningún puesto tenía competencias, así que §5 de
`actores-y-roles.md` —*«la sección que hace o deshace este sistema»*— no podía operar.

### La frontera con ARGOS, y por qué esta mitad sí es nuestra

| Mitad | Dueño | Se edita en SIGTI |
|---|---|---|
| Quién ocupa qué puesto | ARGOS y Talento Humano | **No.** `RN-48`, `DP-001` |
| Qué facultades tiene cada puesto | **SIGTI** | Sí |

La segunda no puede ser de ARGOS: **ARGOS no sabe qué es despachar un vehículo** ni entregar
un vale de combustible, y esperar que lo modele sería pedirle que implemente nuestro dominio.
Por eso hay `POST /competencias` y no hay `POST` de ocupación.

### Los diecinueve pares, transcritos y no deducidos

Se podría haber escrito «las cinco funciones son mutuamente excluyentes» y derivar los diez
pares. **Eso habría perdido las tres cosas que hacen útil a la tabla**: que `I-14` es
configurable y está apagado, que `I-15` e `I-16` son advertencia y no bloqueo, y que cinco de
ellos —`I-07`, `I-10`, `I-11`, `I-12`, `I-13`— **no se levantan nunca**.

### ⚠️ Tres errores míos en el puente rol→función, encontrados contra las fichas

La tabla habla de **funciones** y el sistema asigna **roles**. Ese puente es donde se mete la
pata sin que ninguna prueba de la tabla lo note, porque la tabla estaría bien y el puente mal.
Los tres salieron de leer las fichas de §1 en vez de suponer:

| Lo que escribí | Lo que dice la ficha |
|---|---|
| `ACT-04` autoriza | *«No autoriza la necesidad (`ACT-03`), no despacha físicamente, no entrega el fondo, no cierra el expediente»*. Con `Autoriza` + `Liquida` activaba `I-07` —núcleo irreductible— **contra sí mismo**, y el rol quedaba inoperable |
| `ACT-13` conduce | Responde patrimonialmente por el bien, que es otra cosa. Activaba `I-11` sobre alguien que nunca se sube al vehículo |
| `I-19` = Solicita × Autoriza | **Era una copia literal de `I-01`.** El par es *solicita el fondo × aprueba el fondo*, y sin funciones propias el hueco del hallazgo `HB3-06` seguía abierto **con una fila que aparentaba cubrirlo** |

También estaban mal `I-15` —que es *custodio* × autoriza, no *conduce*— e `I-16`, que es
*ordena el mantenimiento × recibe conforme* y yo había escrito con las funciones de `I-17`.

**La prueba muerde**: reintroducir `Autoriza` en `ACT-04` hace fallar
`El_jefe_de_transporte_no_autoriza_ni_despacha_ni_entrega_fondo`, medido.

### Los dos momentos del control, y sólo uno es un no

§5.3 los separa, y presentarlos igual haría que quien asigna leyera cualquier advertencia como
un rechazo y dejara de asignar:

- **Preventivo, al otorgar el rol.** Si la acumulación activa `I-12` o `I-13` —absolutos— **se
  rechaza**. Medido en vivo: *«No se puede otorgar EncargadoDeDespacho a un puesto que ocupa
  P-AUDITORIA: la acumulación activa I-12 (Audita × Despacha), que es del núcleo
  irreductible»*. Y dispara **también sobre un puesto vacante**.
- **Todo lo demás pasa y queda vigilado.** *«No se puede prohibir de entrada que el Encargado
  de Delegación sea también Solicitante: sería inoperante»*. El bloqueo real llega al ejecutar
  el acto sobre un expediente concreto.

### La siembra reproduce §5.4 a propósito

El espejo se puebla por integración, que no existe, así que en desarrollo se siembra. **Y se
sembró el caso incómodo**: la sede tiene las cinco funciones repartidas y la delegación de
Choluteca las acumula en una sola persona. Una siembra donde todo cumple no muestra nunca el
problema que el sistema existe para ver.

La pantalla lo dice con las palabras del documento: **diez pares acumulados**, `I-07` e `I-10`
marcados núcleo irreductible, y la conclusión — *«cumplir la segregación completa exige cinco
personas distintas por misión, y una delegación de tres no puede cumplirla localmente por
aritmética, no por falta de voluntad. Lo que corresponde es el escalamiento a sede»*.

### Un defecto encontrado mirando la pantalla

⚠️ Un puesto con una competencia que **rige desde el mes que viene** aparecía como «1
competencia cerrada». **Son cosas opuestas** y las dos llegan como «no vigente»: una es
historia, la otra es una asignación programada que alguien espera que entre a funcionar.
Separadas en `cerradas` y `futuras`.

### Qué desbloquea, y qué falta todavía

De las siete que `M-01` iba a desbloquear, **dos están hechas**. `PT-098` a `PT-100` son
catálogos y parámetros de `M-02` —hay `POST /parametros` con doble control, falta la lectura—,
y `PT-101`/`PT-102` son operación, no organización.

**Lo que queda de `M-01` propiamente:** el control **bloqueante** de §5.3.B —impedir el acto
sobre un expediente concreto, registrar el intento y encolar el escalamiento— y la
autenticación, sin la cual `PT-001` no existe y el usuario sigue fijo en `App.tsx`.

El bloqueante es el que más vale: hoy la segregación se verifica **por nombre de persona en
texto libre** en cada módulo, y con `M-01` puede pasar a verificarse por competencia resuelta
a la fecha del hecho.

---

## Las 63 que se pueden hacer — bloque 1: los dos expedientes (M-03 y M-05)

**Seis entregadas: `PT-073`, `PT-075`, `PT-076`, `PT-082`, `PT-084`, `PT-085`.** El mapa se
movió solo: 63 → 57 pendientes, 21 construidas.

### Antes de escribir nada, el filtro que decide qué puede ser real

Se clasificaron las 63 contra la API que existe hoy. **Sólo unas 18 tienen datos detrás.** El
resto necesita backend que no está escrito: `M-01` no tiene usuarios ni puestos, `M-16` sólo
tiene `POST /sincronizacion` sin lectura, `M-17` no tiene manifiestos, `M-19` no tiene
posición, y no hay endpoint de auditoría ni de verificación por QR.

**Construir las otras 45 sería construir fachadas.** Una pantalla que muestra datos que no
existen es peor que la ficha «en desarrollo» que ya tenían, porque la ficha dice la verdad.

### El expediente del vehículo — `PT-073`, `PT-075`, `PT-076`

*«SIGTI cuida de todo lo referente a los vehículos»*. Acá converge lo que vivía repartido:
estado operativo con su **historial entero**, títulos de tenencia, préstamos, incidentes,
vencimientos y ficha técnica. Contesta lo que ninguna lista contestaba: **¿qué le ha pasado a
esta unidad?**

La ficha técnica no lista características: **dice qué decide cada campo** —el peso separa `B`
de `C1` y de `C`, la capacidad separa `D1` de `D`, el remolque exige categoría con `E`—, con la
advertencia de `CLAUDE.md` en su lugar: *«el remolque no es articulado»*.

`PT-075` y `PT-076` van dentro y no aparte: separarlas obligaría a abrir tres páginas para
contestar «¿qué es este vehículo?».

**Y declara los dos capítulos que le faltan.** Mantenimiento (`M-11`) y el detalle de
siniestros de `M-12` no tienen historias —§7.1 del inventario—, así que no hay datos. Un
expediente con esas secciones vacías diría que la unidad nunca entró a taller, que es distinto
de que nadie lo haya registrado.

### El padrón de motoristas — `PT-082` y `PT-085`

Las dos en una pantalla: un padrón que no dice cuándo vence cada licencia obliga a abrir uno
por uno para saber con quién se puede contar, que es justo lo que existe para evitar.

**Lo que no muestra es tan importante como lo que muestra.** `RN-52`: quien despacha ve *que*
hay restricción, **no el diagnóstico**. La columna dice «tiene, sin detallar» y el dato clínico
no sale del expediente de Talento Humano.

Y «fuera del padrón» **no es irregular**: `RN-57` verifica sobre quien efectivamente conduce.
Se dice en la fila para que no se lea como un hueco.

### `PT-084` — y el fixture que mintió

La matriz licencia↔vehículo sostiene `BD-02`, que traslada responsabilidad legal directa a quien
autoriza, así que **el cliente no la deriva**: se agregó `GET /matriz-de-licencias` y la
pantalla pinta lo que el servidor resolvió.

⚠️ **La primera versión del endpoint mintió, y vale registrar cómo.** Evaluaba la matriz contra
fichas técnicas de muestra escritas en el propio endpoint, y publicó que **`B1` y `C1` no
habilitaban nada**. Las dos tienen entrada en la matriz: lo que faltaba era un triciclo y un
camión liviano entre los ejemplos. Un catálogo normativo que depende de qué casos se le
ocurrieron a quien lo escribió no es un catálogo.

Se rehízo **contra la flota real**, que además contesta la pregunta que de verdad se hace:
*«con una licencia B, ¿cuáles de nuestras unidades puedo conducir?»*. Y la fila vacía se
explica, porque **«no tenemos autobuses» y «el umbral no alcanza» piden cosas distintas**:

| | |
|---|---|
| `B1` | ninguna — *la flota no tiene triciclos ni cuadriciclos* |
| `C1` | ninguna — *sí hay camiones, lo que no alcanza es el umbral de 7,500 kg* |

Medido en vivo, y confirma la regla de `CLAUDE.md`: **`INS-P-021` —pick-up con plataforma
enganchada— aparece bajo `BE` y nunca bajo `B`.**

### Lo que queda, y por qué no se hizo de un tirón

**57 pendientes, de las cuales ~12 más tienen API y ~45 necesitan backend primero.** Las que
siguen siendo hacederas hoy: `PT-006` mis solicitudes, `PT-071` hallazgo posterior —API
completa—, `PT-089` rastro del expediente, `PT-058`/`PT-059` seguimiento en ruta, `PT-065`
conciliación del fondo, `PT-009` estimado de peajes, `PT-033` sustitución en `DESPACHADA`.

Las otras no son trabajo de frontend: son módulos sin escribir.

---

## Las 138 pantallas, navegables — mapa y fichas «en desarrollo»

**El inventario ya no es sólo un documento.** Las 138 pantallas de
[`inventario-de-pantallas.md`](docs/04-diseno/inventario-de-pantallas.md) se recorren desde
`/pantallas`: la construida abre su ruta real y la que falta abre una ficha que dice **por qué**
no está.

### Se genera del documento, no se copia

`npm run generar-inventario` lee el markdown y produce `inventario.generado.ts`. Una lista
escrita a mano en el frontend **es una lista que va a divergir**: el día que el inventario dé de
alta una pantalla, la aplicación seguiría diciendo 138 y nadie se enteraría.

El generador **se valida contra lo que el propio documento declara** —138 filas, 99/29/10 por
papel, 103/9/25/1 por cliente— y falla si el conteo no da. Y `npm run verificar` corre
`--verificar`, que falla si el generado quedó atrás. **Probado**: al cambiar una fila del
markdown, la verificación se cae en el momento.

### Lo que el mapa deja ver

| Situación | |
|---|---|
| Construidas | 17 |
| A medias | 13 |
| **En desarrollo — se pueden escribir hoy** | **63** |
| Falta el formato en papel | 20 |
| Cliente de campo | 25 |

**El número que importa es 63, no 108.** De lo que falta, 20 no se destraban programando —esperan
el insumo #2— y 25 no son de la oficina. Meter los tres en una sola cifra produce una cola que no
se puede planificar.

### ⚠️ El conteo de bloqueadas NO da los 29 del documento, y se dice

Aparecen 20. La diferencia se muestra en la propia pantalla porque **un número que discrepa de la
autoridad sin explicarse obliga a elegir a cuál creerle**:

- **2 se construyeron igual**, sin el formato a la vista: `PT-074` y `PT-081`. Las dos quedaron
  marcadas *a medias* con la advertencia de que puede haber que rehacerlas contra el papel.
- **7 son del cliente de campo** (`PT-106`, `114`, `118`, `121`, `122`, `123`, `124`): no esperan
  un formato, esperan un cliente entero.

### ⚠️ El desfase inverso: seis pantallas construidas que el inventario no tiene

Préstamos, Peajes, Incidentes, Conciliación, Saldo de apertura y Cierre de ejercicio. **No son
sobras**: cada una salió de una regla escrita después del inventario. De dos de ellas §7.1 ya
avisaba —*«M-12 más allá del registro en ruta»*, *«M-18 sin pantalla propia hasta que haya
historias»*— y se construyeron igual; de las otras cuatro el documento no dice nada porque son
posteriores. El mapa las lista aparte, y **ninguna cuenta dentro de las 138**.

**Y `PT-139` ya está construido.** El cronograma de flota semanal que el inventario dejó fuera
esperando que el PO lo acepte —*«el ID queda reservado y no se usa para otra cosa»*— vive hoy en
`/despacho` y en la asignación. Hay una pantalla en uso con una decisión sin tomar.

### La ficha de la que falta

No dice «próximamente». Dice qué pantalla es, **qué la origina** —`CU`, `HU`, roles, si funciona
sin red— y en cuál de cuatro situaciones está, porque cada una la destraba alguien distinto:

| | Quién la destraba |
|---|---|
| En desarrollo | Quien programa. Nada más la frena |
| Falta el formato | La institución, entregando el papel |
| Cliente de campo | Un cliente que no existe |
| A medias | Quien programa, y se nombra **qué mitad** falta |

Las filas que el inventario dejó sin trazar se muestran como *«el inventario no lo trazó»* y no
como un guion: un guion suelto se lee como «ninguno».

### La trazabilidad pantalla↔`PT`, que antes no existía

Trece pantallas construidas no citaban su `PT-xxx`, así que el inventario era incontrastable
contra el código: no había forma de decir cuántas de las 138 estaban hechas. Ahora vive en
`registro.ts`, **escrito a mano y no derivado**, porque es una afirmación sobre el código y
derivarla del documento daría por construida una pantalla por estar inventariada.

Una ruta puede cubrir varias pantallas del inventario —la asignación resuelve `PT-026`, `027`,
`028` y `031` en un solo recorrido— y eso es correcto: partirlas obligaría a ir y volver entre
cuatro páginas para tomar una decisión.

---

## Las seis pantallas que tenían el bloqueo invisible

**RESUELTAS.** El arreglo de la capa de avisos era central, así que las seis quedaron
cubiertas por él. Lo que faltaba era **comprobarlo una por una**, y comprobar reveló que en
dos de ellas el motivo del rechazo no llegaba al aviso ni siquiera con el aviso visible.

### `Cola` se tragaba el motivo antes de mostrarlo

⚠️ Los dos manejadores de la cola de programación —desprogramar y anular— eran
`onError: () => avisar.error('No se pudo…')`: **descartaban el error sin mirarlo**. Con la capa
arreglada el aviso ya se veía, y lo que se veía era «No se pudo desprogramar», que no se puede
accionar.

Medido en vivo, ahora dice: *«La transición T-11 exige el estado Programada, y el expediente
está en Aprobada»*. Eran **los dos únicos `onError` de todo el frontend que no reciben el
error**; el resto ya lo pasaba.

### El 409 no siempre se llama `precondicion`

⚠️ El adaptador leía sólo `cuerpo.precondicion` y caía en `'desconocida'`. Pero la API nombra
el rechazo según de qué familia sea, **y a propósito**: una transición inválida trae
`transicion`, el fondo trae `movimiento`, una carga rechazada trae `motivo`, la aprobación
vencida trae `caducada`. Cada nombre dice por dónde se sale del rechazo.

Resultado: `Expediente`, que antepone el identificador, imprimía **«desconocida — La transición
T-06 exige…»** en pantalla. Corregido en `pedir`, que ahora recorre las cinco formas.

Y al corregirlo apareció lo contrario: **«T-06 — La transición T-06 exige…»**. El prefijo ahora
vive en `BloqueoDuro.paraMostrar` y se antepone **sólo si el mensaje no lo dice ya**: un `BD-xx`
no aparece en su propio texto y sin prefijo no hay cómo citarlo; una transición sí.

### Qué se verificó en vivo, y qué no

| Pantalla | Bloqueo provocado | Resultado |
|---|---|---|
| `Padron` | Terminal con misiones sin cerrar | ✅ *«tiene 3 misión(es) sin cerrar»* |
| `Cola` | Carrera: desprogramada por detrás | ✅ *«T-11 exige el estado Programada»* |
| `Expediente` | Carrera: aprobada por detrás | ✅ *«T-06 exige el estado Solicitada»* |
| `Fondos` | Segregación: aprobar lo que uno pidió | ✅ *«quien pide y quien autoriza tienen que ser dos personas distintas»* |
| `PanelDeVales` | — | ⚠️ **sólo por auditoría de código** |
| `PanelDeAbastecimientos` | — | ⚠️ **sólo por auditoría de código** |

Los dos últimos no se pudieron provocar: **la emisión del vale bloquea antes de enviar**. La
precondición del fondo aprobado y vigente se evalúa en el cliente y el botón queda inhabilitado
con el motivo escrito en el propio diálogo, así que la petición nunca sale. Sus manejadores ya
pasaban `e.message` —igual que los cuatro verificados— y la visibilidad del aviso no depende de
la pantalla, pero **eso es un argumento, no una medición**, y queda dicho como tal.

### Estado de desarrollo tocado al probar

Las carreras se provocaron contra la base de desarrollo: `PROV-B22434` quedó `Aprobada` en vez
de `Programada`, hay un fondo de septiembre de más en `Solicitado`, y una misión de prueba
`PROV-3ZD7R2`. Sin datos reales y sin nada en producción.

---

## El panel de títulos de tenencia, y un defecto del sistema de diseño

**RESUELTO.** `RN-62` ya se puede cargar y mirar desde la oficina: `/titulos`, dentro de M-03,
pegado al padrón porque es la respuesta a «¿es nuestro?» y de ella cuelga cuál de los dos
terminales ofrece el propio padrón al dar de baja.

### La pantalla contesta cobertura, no lista

La pregunta no es «qué títulos hay» sino **cuántos controles están apagados**. Mientras un
vehículo no tenga título el sistema advierte y deja pasar, así que cada uno sin él es
`RN-62` sin evaluar en esa unidad — y un control apagado que nadie ve es indistinguible de uno
que nunca hizo falta. Los avisos de cabecera **nombran qué deja de evaluarse**, no dicen
«faltan datos».

### Tres situaciones que se veían iguales y no lo son

| Situación | Qué exige |
|---|---|
| **Nunca tuvo título** | Llenar un dato de alta |
| **Título vencido** | Recuperar un bien ajeno que ya debía volver |
| **Vence pronto** | Renovar antes de que frene la próxima misión |

⚠️ Las dos primeras **llegaban idénticas del servidor**: las dos sin título vigente. Se agregó
`ultimo` —el más reciente, esté vigente o no— porque sin él el comodato corrido de plazo queda
escondido entre los vehículos a los que sólo les falta un dato de alta, que es exactamente el
que hay que ver. Va con prueba propia.

**Y una cuarta que no es un hueco:** el vehículo ya dado de baja o retirado. No le queda
ningún control que encender —no se le va a programar nada y su terminal ya ocurrió—, así que
no cuenta en el conteo ni se le ofrece registrar. Contarlo inflaría el número justo hasta
volverlo inservible para decidir por dónde empezar. Lo decide el servidor (`fueraDeLaFlota`),
como `inutilizable` en el cronograma: la lista de terminales es de §10.2 y duplicarla en el
cliente la dejaría divergir.

### ⚠️ El sistema de diseño escondía los bloqueos duros

**`Modal` es un `<dialog>` abierto con `showModal()`, que va a la capa superior del navegador
y se pinta encima de todo `z-index`** — incluido el 999999999 de sonner. Un `avisar.*`
disparado con un modal abierto quedaba **debajo del modal, invisible**.

El efecto era peor que un problema de pintura, y es la parte que importa: **el éxito se veía
y el bloqueo no.** El éxito cierra el modal, así que el aviso aparecía; el bloqueo lo deja
abierto, así que no. Quien apretaba «Guardar» sobre una precondición incumplida veía que no
pasaba nada, y el motivo del rechazo aparecía recién si se rendía y cancelaba.

**Alcanzaba a siete pantallas**, todas las que muestran un 409 desde dentro de un modal:
`Padron`, `Titulos`, `Expediente` de autorización, `Cola` de programación, `Fondos`,
`PanelDeAbastecimientos` y `PanelDeVales`.

Arreglado en `avisos.tsx`, en un solo lugar: la capa de avisos es un `popover`, que también
vive en la capa superior. Lo que **no** funcionó, medido en vivo y por eso vale anotarlo:

- Promover al montar. La capa superior se ordena por **orden de entrada**, y el modal se abre
  después.
- Promover desde `avisar.*`. Corre **antes** de que sonner monte el nodo del aviso, y el aviso
  seguía debajo. Un cuadro después tampoco alcanza.
- Registrar la función de promoción desde el efecto en una variable de módulo: la referencia
  terminaba en el sustituto vacío.

Lo que sí: **promover cuando el nodo del aviso entra al DOM**, con un `MutationObserver` sobre
la capa. Se probó un `setTimeout` de 500 ms —funcionaba— y se descartó: es la duración de una
animación disfrazada de constante, y el día que cambie vuelve a fallar en silencio.

### Dos cosas más que salieron de mirar la pantalla

**`declararEstado` tiraba la advertencia a la basura.** Devolvía `Promise<void>` y el servidor
venía diciendo que no pudo verificar si el terminal correspondía. Ahora el padrón la muestra,
**gana al «quedó listo»** y dura 15 segundos: quien acaba de dar de baja un vehículo tiene que
alcanzar a leer que `HB3-17` no juzgó.

**Los siete rubros arrancan en «sin pactar», no en «la institución».** Suponer que pagamos
nosotros es exactamente la conclusión que hay que dejar en manos de quien llena el formulario.

---

## `RN-62` — el título de tenencia (M-03)

**RESUELTA.** Era el insumo #100, y era lo que le faltaba a la corrección `HB3-17` para operar:
hasta ahora **siempre advertía en vez de juzgar**, porque el régimen de tenencia no existía en
ninguna parte del sistema.

### El título es una serie, no una columna del vehículo

Un vehículo que pasa de comodato a propiedad **conserva el título anterior**. Las misiones de ese
período se hicieron bajo comodato y sus rubros los cubría el cedente; reescribir el régimen
borraría el contexto contable de todo lo ya ejecutado. De la serie manda **el que regía a la
fecha del hecho** (`P-4`), no el vigente hoy.

Dos títulos vigentes a la vez se bloquean al registrar: el vehículo estaría en dos regímenes al
mismo tiempo y **la pregunta de si el bien es del Estado no tendría respuesta**. Lo que cambia el
régimen es cerrar el anterior y abrir el nuevo.

### Lo que el título decide

| Control | Qué impone |
|---|---|
| **Habilitación** | Sin título vigente el vehículo no se habilita: no consta bajo qué régimen lo tenemos |
| **Programación** | La ventana de la misión tiene que caber **entera** dentro de la vigencia |
| **Terminal correcto** (`HB3-17`) | El descargo es de bienes propios; el retiro de flota, de ajenos |
| **Imputación** | El rubro que cubre el titular **no se carga a nuestro presupuesto** |

La programación sigue el patrón de `RN-10` con la licencia: **no alcanza con que el título esté
vigente el día de la salida**. Medido en vivo: *«La ventana de la misión (28/12/2026 al
05/01/2027) excede la vigencia del título de tenencia, que rige del 01/01/2026 al 31/12/2026
(Comodato, Secretaría de Salud) […] tiene que cubrir todo el rango, o el vehículo dejaría de ser
nuestro para usarlo a mitad de la misión»*.

### `HB3-17` ya juzga — medido en vivo

| Caso | Antes | Ahora |
|---|---|---|
| Comodato → `DadoDeBaja` | pasaba | **409** *«sería un asiento falso sobre un bien ajeno […] Lo que corresponde es RETIRADO_DE_FLOTA»* |
| Comodato → `RetiradoDeFlota` | pasaba | 200 |
| Propiedad → `RetiradoDeFlota` | pasaba | **409** *«sale del registro por DESCARGO»* |
| **Sin título** → cualquier terminal | pasaba mudo | 200 **con advertencia nombrada** |

Ese último es deliberado y es el mismo criterio de `BD-07`: **sin título se advierte, no se
bloquea**. Frenar el descargo de toda la flota por un dato de alta que nadie llenó sería peor que
el asiento que se quiere evitar. Pero la advertencia **dice cuál mitad no se evaluó**, no calla.

### «Sin pactar» no es «la institución»

Los siete rubros —combustible, mantenimiento, llantas, seguro, peajes, multas, daños— tienen tres
valores, no dos. `SinPactar` **responde nulo**, no «la institución»: es el rubro que aparece
cuando llega la factura y empieza la discusión con el contrato en la mano, y suponer que lo
pagamos nosotros es exactamente la conclusión que hay que dejar en manos de quien pregunte. La
ficha los lista aparte de los del titular.

### Dos decisiones del dominio que vale nombrar

**La propiedad es el único régimen sin fecha de fin.** Ponerle una haría que el vehículo se
inhabilitara solo el día que alguien eligió sin que ninguna norma lo mandara. Y los demás **sí la
exigen**: un comodato que no vence es una apropiación.

**Sólo la propiedad hace propio el bien.** `DonacionEnTramite` todavía no lo es: hasta que el
traspaso se perfeccione, darlo de baja del registro sería anticipar un título que no está.

### ⚠️ Una nota vencida en la propia regla

`RN-62` dice en sus casos límite que el estado terminal `RETIRADO_DE_FLOTA` es *«inexistente»* y
que la regla *«no lo crea: describe el comportamiento que debe tener cuando exista»*. **Ya
existe** — está en §10.2 y en el enum `EstadoOperativo` desde las transiciones `W-xx`. La nota de
hallazgo abierta que la regla declara **ya no aplica**, y conviene cerrarla en el documento para
que nadie vuelva a diseñar alrededor de una carencia que no está.

---

## `W-01`..`W-19` — la tabla de transiciones del estado operativo

**RESUELTA.** Era el cuello de botella que tres reglas esperaban: M-12, `RN-63` y `RN-60`
necesitaban mover el estado del vehículo y ninguna podía, porque el código no tenía la tabla.

### El comentario que lo bloqueaba estaba equivocado

`EstadoDeLaFlota.AnotarAsync` decía: *«no valida la transición entre estados, y es deliberado:
§10.2 **no publica una tabla de transiciones permitidas del vehículo** como sí lo hace para la
misión, y inventarla acá sería escribir la regla en la capa que menos autoridad tiene»*.

**Sí la publica.** El diagrama de §10.2 enumera `W-01` a `W-19` más `W-16b`. Quien escribió eso
miró la tabla de estados y no el diagrama de transiciones. Lo que faltaba era **transcribirla**,
no inventarla — y el argumento del comentario sigue siendo correcto: si la tabla del código y el
diagrama difieren, manda el documento y el código es el defecto.

### Lo que la tabla impone ahora

| Control | De dónde sale |
|---|---|
| La transición existe en el diagrama | `W-01`..`W-19` |
| `ASIGNADO` y `EN_MISION` **sólo los fija el sistema** | *«permitir fijarlos a mano abre la puerta a un vehículo "en misión" sin misión»* |
| `NO_DISPONIBLE` exige **causa tipificada** | *«sin tipificación, este estado se convierte en el cementerio donde se esconde la flota que nadie repara»* |
| Terminales y préstamo exigen **acta** | `NRM-02` |
| Sin misiones abiertas para los dos terminales | §10.2 |
| **El descargo es de bienes propios; el retiro, de ajenos** | corrección `HB3-17` |

Ese último es el que más importa: declarar *«dado de baja del registro de bienes del Estado»* un
vehículo en comodato es **un asiento falso sobre un bien ajeno, detectable cruzando el inventario
institucional contra el padrón de flota**. Sin régimen declarado **se advierte, no se bloquea**:
frenar el descargo de toda la flota por un dato de alta que nadie llenó sería peor que el asiento
que se quiere evitar.

**El bloqueo enumera los destinos legales.** Medido en vivo: *«§10.2 no contempla ir de Prestado
a EnTaller. Desde Prestado se puede ir a: Disponible (W-17 devolución del préstamo)»*. Un
«transición no permitida» a secas obliga a quien opera a adivinar el camino.

### Una contradicción abierta entre `RN-60` y §10.2

⚠️ **`RN-60` presupone una transición que el diagrama no tiene.** La regla habla de
indisponibilidad *sobrevenida* sobre un vehículo con reservas —*«toda Orden de Misión ya
PROGRAMADA o DESPACHADA sobre ese vehículo debe marcarse en conflicto»*— pero §10.2 sólo deja ir
a taller desde `DISPONIBLE` (`W-09`) o `NO_DISPONIBLE` (`W-12`): **no hay `ASIGNADO →
EN_TALLER`**.

§10.2 es la autoridad sobre transiciones, y agregar la que falta desde el código sería escribir
en el documento. Lo que hace el sistema es **registrar el expediente igual** —el conflicto, el
acuse y el bloqueo del despacho operan— y **declarar en el expediente que el asiento de estado
no se pudo poner**, con el porqué. La contradicción la resuelve quien tenga autoridad sobre
§10.2, no este turno.

### Dos hallazgos más, salidos de correr las pruebas

⚠️ **El sistema anotaba `ASIGNADO` sobre vehículos sin estado declarado**, que §10.2 no
contempla —`W-01` dice que el vehículo nace `NO_DISPONIBLE`—. No se bloqueó, porque **`BD-07` ya
decidió otra cosa**: con estado nulo no bloquea, lo declara en el diario. Esa decisión es de la
máquina de estados y no se contradice desde acá; y si `BD-07` dejó programar, negarse a anotar la
consecuencia dejaría la misión programada y el vehículo sin asiento. **Sólo se tolera para las
automáticas**: una persona que declara un estado sobre un vehículo sin historial sigue teniendo
que empezar por `W-01`.

**Cuatro pruebas existentes saltaban directo al estado que querían** —`EnTaller` sobre un
vehículo recién sembrado— y pasaban porque el endpoint no validaba nada. Ahora recorren el camino
legal: `W-01` alta → `W-02` habilitar → operar, que es además lo que hace el alta real. Y el
historial pasó de dos asientos a cuatro: **conserva el camino entero**, que es lo que contesta
«¿por qué no estuvo disponible?».

⚠️ **El régimen de tenencia no existe como campo del vehículo.** `RN-62` lo pide con vigencia y
rubros, y sin él la verificación del terminal correcto siempre advierte en vez de juzgar. Es lo
que falta para que la corrección de `HB3-17` opere de verdad.

---

## `RN-60` — la indisponibilidad sobrevenida y sus reservas en conflicto (M-11)

**RESUELTA.** Es el corazón de M-11 y **cierra el hueco que M-12 y `RN-63` dejaron dos veces**:
hasta ahora, un vehículo podía irse al taller con misiones programadas encima y el despacho
seguía saliendo.

### El acuse es lo que convierte el hecho en una decisión

*«Antes de confirmar la indisponibilidad, el sistema muestra las Órdenes de Misión afectadas:
folio, dependencia solicitante, ventana, motorista y objeto. **Quien ejecuta acusa**»*. Sin ese
paso, el conflicto aparece después y nadie lo decidió.

**Y la lista se congela.** `RN-60` punto 2: *«se conserva exactamente como se presentó, con su
marca de tiempo. **No se reconstruye después**»*. Hay prueba: se reasigna la misión a otro
vehículo después del acuse y la reserva **sigue en el expediente con el estado que tenía al
acusar**. Si se recalculara, habría desaparecido — y quien acusó habría acusado sobre una lista
que ya no consta.

### La marca impide el despacho, de verdad

Verificado de punta a punta: misión programada → vehículo a taller con acuse → **`T-12`
bloqueado** → desenlace registrado → despacho sale. *«Una reserva en conflicto no expira en
silencio ni se resuelve por el paso del tiempo»*.

El bloqueo entra a `Despachar` como **parámetro obligatorio y sin omisión**, siguiendo el
argumento que la propia máquina de estados usa para la custodia: *«es la diferencia entre "no
hay custodio" y "nadie preguntó", y en un bloqueo duro las dos no pueden verse igual»*. Un
`bool` por omisión dejaría que un llamador nuevo despachara sin consultar, y el bloqueo se
apagaría solo. Diecisiete pruebas existentes tuvieron que declarar que consultaron — que es
exactamente el efecto buscado.

### El alta mide la gestión del taller

`RN-60` punto 6 — fecha real, **orden de trabajo cerrada y odómetro de salida**, contrastados
contra la ventana estimada: *«la desviación sistemática entre estimado y real es indicador de la
gestión del taller»*. La desviación va **nula** mientras el vehículo no vuelva: suponerla haría
que el indicador midiera estimaciones contra sí mismas.

⚠️ **`ConflictoDeReserva` ya existía en M-07** con otro significado —el solape de `BD-11`—, así
que el de esta regla se llama `ConflictoPorIndisponibilidad`. Son dos conflictos distintos: uno
es que dos misiones quieren el mismo recurso, el otro que el recurso no está.

⚠️ **El bloqueo del despacho no tiene identificador `BD-xx`.** La máquina de estados es la
autoridad sobre los bloqueos duros del despacho y no lo cataloga; `RN-60` sí lo declara. Se
implementó citando la regla, y **queda como hallazgo para que la autoridad lo incorpore** — es
el mismo patrón que la nota abierta de `RN-63` sobre `actores-y-roles.md`.

✅ **La indisponibilidad ya mueve el estado operativo**, en la misma transacción y validada contra
la tabla `W-xx`. Con una excepción declarada: cuando §10.2 no contempla la transición —el caso
`ASIGNADO → EN_TALLER`— el expediente se registra igual y dice por qué el asiento no se puso. Ver
[`W-01`..`W-19`](#w-01w-19--la-tabla-de-transiciones-del-estado-operativo).

⚠️ **`horizonte_reservas_afectadas` no está declarado.** `RN-60` lo declara configurable; hoy el
horizonte **es la ventana estimada de la indisponibilidad**, que es lo defendible sin inventarlo
— las reservas que caen dentro son exactamente las que quedan en el aire.

⚠️ **No hay notificación a ACT-04 ni a la dependencia solicitante** (`RN-60` punto 3), ni el
reporte de misiones sobre vehículos con preventivo por vencer (punto 7). El primero es el mismo
pendiente de notificaciones que arrastran otras reglas; el segundo necesita el plan de
mantenimiento, que es el resto de M-11.

⚠️ **`causa_indisponibilidad` es texto validado, no catálogo** — `RN-60` lo declara
configurable. Tercer catálogo en la misma situación, con `causa_interrupcion` (#96) y
`motivo_de_prestamo` (#98).

---

## `RN-63` — el préstamo de vehículo como expediente del bien

**RESUELTA.** Y con ella **el bloqueo del cierre del período queda completo**: era la última de
las dos fuentes que `RN-97` punto 4 declaraba con poder de bloqueo y que no podía disparar.

| | Antes de M-12 | Con M-12 | Con `RN-63` |
|---|---|---|---|
| Fuentes consultables del saldo | 5 de 10 | 7 de 10 | **8 de 10** |
| Fuentes bloqueantes que disparan | 0 de 2 | 1 de 2 | **2 de 2** |

### Nunca una Orden de Misión, y la diferencia es la tenencia

*«Cuando el vehículo se cede **con motorista de la institución propietaria**, sí es una Orden de
Misión con motivo apoyo institucional: ahí no se cedió la tenencia, se prestó un servicio»*. El
endpoint lo bloquea: modelarlo como préstamo diría que la unidad salió del alcance de la
institución cuando su propio motorista iba al volante.

### El entregable de la regla

`RN-63` punto 7 lo llama así: *«en cualquier fecha del período, el sistema responde **quién
respondía por la unidad**»*. Se resuelve **por la fecha, no por el estado de hoy** — cuando llega
una multa de agosto la pregunta no es quién tiene el vehículo hoy, es quién lo tenía ese día.

Medido en vivo: el 20 de abril responde *«Ana Discua, Jefe de Transporte de la Secretaría de
Salud»*; el 1 de abril, antes del préstamo, responde la institución propietaria.

### La segregación, con la nota de hallazgo que la propia regla deja abierta

Quien autoriza **no puede** ser el receptor —sería la misma persona decidiendo entregarse a sí
misma un vehículo del Estado— y quien firma la devolución **no puede** ser quien recibió: el acta
dejaría de ser constatación para volverse autodeclaración de que devolvió en orden. La
comparación ignora mayúsculas y espacios, para que el rodeo no pueda ser tipográfico.

⚠️ **El par no está en `actores-y-roles.md`**, que es la autoridad sobre incompatibilidades. La
propia `RN-63` punto 2 lo dice: *«su lugar propio es `actores-y-roles.md` y desde `CE-14` se
propuso como par `I-c`. **Nota de hallazgo abierta** hasta que se incorpore allí»*. Se implementó
acá porque la regla es bloqueo duro y no puede quedar sin efecto esperando al documento — pero
mientras el par no esté en la autoridad, esta comprobación es lo único que lo sostiene.

### Lo que el expediente calcula

**Los kilómetros bajo tenencia ajena no entran en la conciliación galonaje–kilometraje** (`RN-30`,
`RN-63` punto 3): no hubo consumo nuestro contra esos kilómetros. Salen de las dos lecturas de
odómetro, y son **nulos** mientras no haya acta de devolución: con una sola lectura no hay
recorrido que medir.

**La rotulación se reconstata al devolver.** La identificación del vehículo del Estado es
hallazgo frecuente de auditoría, y uno que vuelve sin ella volvió distinto de como salió.

**Los rubros sin pactar van nombrados**, no supuestos: un rubro sin pactar es el que aparece
cuando llega la multa, con el vehículo ya devuelto.

⚠️ **El préstamo no cambia el estado operativo del vehículo.** `RN-63` punto 1 manda pasarlo a un
estado que no habilita asignación, con el circuito de reservas afectadas de `RN-60`. Es el mismo
hueco que dejó M-12: las transiciones `W-xx` no están identificadas en código.

⚠️ **`RN-63` punto 6 —el salvoconducto si la ventana comprende día inhábil— no está.** La regla
exige el permiso de `RN-23` o la constancia de que el préstamo se limitó a días hábiles.

⚠️ **El escalamiento diario por mora no notifica.** Los días de mora se calculan y se ven en la
pantalla y en el saldo, pero `RN-63` punto 4 pide alerta con escalamiento, y no hay quién la
emita: es el mismo pendiente de notificaciones que arrastran otras reglas.

⚠️ **`motivo_de_prestamo` es texto validado, no catálogo** — `RN-63` lo declara configurable.
Mismo estado que `causa_interrupcion` (insumo #96).

---

## M-12 — Incidentes, siniestros y sanciones

**RESUELTO el núcleo.** Era el módulo funcional entero más grande sin construir: once reglas de
negocio escritas y cero líneas de código. Se construyeron las tres que lo definen — `RN-70`,
`RN-74` y `RN-75` — con su expediente, su diario `I-01`..`I-08`, sus bienes y sus gestiones.

### El bloqueo del cierre por fin dispara

**Es lo que este módulo paga.** `RN-97` punto 4 le da poder de bloqueo del cierre del período a
dos fuentes, y el saldo de apertura las declaraba *«no consultables»* porque no existían como
registro: el bloqueo estaba escrito y no podía disparar.

Medido contra la base de desarrollo, con una sustracción sembrada:

| | Antes de M-12 | Después |
|---|---|---|
| Fuentes consultables del saldo | 5 de 10 | **7 de 10** |
| Producir el saldo con una interrupción viva | 201 | **409** |

Y el circuito completo cierra: registrar el desenlace desde la pantalla levanta la marca, y el
saldo se produce. **Pero el expediente sigue en el inventario** con causa `BienNoRecuperado`,
porque `RN-75` conserva los bienes hasta su recuperación o su descargo. Las dos reglas actuando
juntas.

### `RN-74` — el módulo que no pregunta de quién fue la culpa

No hay **un solo campo** de responsabilidad, culpa o dolo: ni en el dominio, ni en la fila, ni en
el contrato de la API, ni en la pantalla. La regla explica por qué: *«un motorista que acaba de
tener un accidente, a la orilla de la carretera, con un tercero gritándole, no está en
condiciones de calificar jurídicamente lo que pasó — y no le corresponde»*.

Y el argumento que decide: *«si registrar el hecho implica autoinculparse, **el hecho no se
registra**. Y un accidente no registrado es peor que cualquier atribución mal hecha»*.

Lo más cerca que el módulo llega es `I-07`, que **adjunta el acto de otra instancia** con su
número y su emisor. Sin los dos, bloquea: sin ellos no es un acto, es una atribución hecha por
quien no tiene competencia. Y no se puede reemplazar — un acto posterior que lo revoque se
adjunta como hecho nuevo (`RN-42`).

### `RN-70` — la interrupción marca y no cambia el estado

*«El evento marca la misión como interrumpida y **no le cambia el estado**. La Orden de Misión
sigue `EN_RUTA`: el vehículo salió y hubo consumo real de recursos públicos»*. Ningún método del
servicio toca la máquina de estados de la misión.

El desenlace se registra **una sola vez y con constancia**: los cuatro tipos de la regla, ni uno
más. Un quinto «otro» dejaría la mitad de las interrupciones resueltas sin decir cómo.

**`Interrumpe` lo declara quien registra**, no se deduce del tipo: una avería leve que se
resolvió en la orilla no interrumpió, y una que dejó el vehículo en la carretera sí. Deducirlo
exigiría desenlace a expedientes que no lo necesitan.

### `RN-75` — el bien no sale del registro

*«Permanece en el registro patrimonial hasta su recuperación o su descargo formal. **Nunca se
elimina**»*. Las dos salidas son cambios de estado, no bajas, y el descargo exige **acto formal**
con número y autoridad: sin él sería una baja sin respaldo sobre un bien del Estado (`NRM-02`).

Cerrar el expediente con bienes afuera **no se puede sin declararlo**. Declararlo queda escrito;
ignorarlos los haría desaparecer de la vista sin que la recuperación ni el descargo hubieran
ocurrido — el mismo abandono silencioso que `RN-97` persigue.

**La custodia se exige donde se sabe.** De una retención por autoridad se conoce quién tiene el
bien y bajo qué expediente —el acta lo dice— y no declararlo bloquea. De una **sustracción puede
no saberse nada**, y exigir la ubicación impediría registrar el robo: el peor resultado posible.

### Una prueba que empezó a fallar con razón

`El_acta_declara_el_saldo_que_cita_y_sus_diferencias` producía el saldo sin declarar bloqueantes,
y pasaba **porque las interrupciones no existían**. Al construirse M-12 empezó a fallar con un
409 legítimo. El arreglo fue declararlos con motivo, que es lo que `RN-97` punto 4 prevé — y es
además lo realista: en una institución siempre hay algo abierto al corte.

⚠️ **`causa_interrupcion` es texto validado, no catálogo.** `RN-70` lo declara configurable y la
institución no ha declarado sus causas. Cablear una lista obligaría a un despliegue cada vez que
aparezca una que nadie previó. Mismo estado que `tipo_de_hallazgo_posterior` de `RN-93`.

⚠️ **El incidente no cambia el estado operativo del vehículo.** `RN-70` punto 3 y `RN-75` punto 3
mandan pasarlo a `NO_DISPONIBLE` **desde la hora del hecho**, por `W-07` o `W-08`. El estado
operativo existe (`M03_Flota`) pero las transiciones `W-xx` no están identificadas en código, y
acoplarlo a ciegas movería flota sin la precondición que la máquina de estados exige.

⚠️ **Falta el circuito de folios a bordo.** `RN-75` manda pasarlos a `SUSTRAIDO` —distinto de
anulado—, que su verificación por QR responda eso, y que todo uso posterior sea alerta
automática. Hoy los bienes se registran a mano; **la lista no se produce desde la asignación**.

⚠️ **`RN-63` sigue sin construir**, y es la otra mitad del bloqueo del cierre. Es la fuente
`PrestamoVencido`, la única que queda con poder de bloqueo declarado y sin poder disparar.

⚠️ **Ocho de las once reglas de M-12 ya estaban.** `RN-43`, `RN-66`, `RN-69`, `RN-86`, `RN-93`,
`RN-95` y `RN-97` se construyeron en turnos anteriores desde otros módulos. Lo que faltaba era el
expediente mismo, que es lo que se hizo.

---

### `RN-96` — el cierre de ejercicio como corte de imputación

**RESUELTA.** Y lo que se construyó es, sobre todo, **lo que el cierre no hace**: *«no ejecuta
ni habilita ninguna transición de la Orden de Misión. **Ningún expediente cambia de estado por
efecto de una fecha**»*.

La ficha nombra el riesgo con precisión: *«sin esta regla escrita la primera implementación va a
poner un cierre masivo por fecha, porque es lo que resuelve ese problema»*. No hay un solo
método, endpoint ni botón que cierre misiones, y **la prueba que sostiene toda la regla** produce
el acta y verifica que el diario de un expediente en ruta quedó idéntico, asiento por asiento.

#### Lo que el acta produce

| `RN-96` | Qué hace |
|---|---|
| **1** · acta con folio | Dos cortes —legal y operativo— con su bloqueo: el operativo anterior al legal deja los días de en medio sin ejercicio al que imputarse |
| **2** · inventario y saldo | Cuadra renglón por renglón contra `RN-97`. **Es la comprobación que existía sin nada contra qué correr** |
| **3** · evaluación individual | Detecta motivos de cierre compartidos por varios expedientes |
| **4** · desglose por ejercicio | La misión que cruza **no se divide**; sus hechos se imputan a su propia fecha, con la tabla que los valoró |
| **5** · folios no consumidos | Se listan; anular es un acto aparte que cita el acta |
| **6** · parámetros de la ventana | Quién movió qué umbral en diciembre, de qué valor a cuál |

#### El cierre en bloque se detecta, y sin acusar a quien sí evaluó

El motivo se compara **normalizado** —espacios y mayúsculas—, porque quien cierra cincuenta
expedientes copiando y pegando no escribe idéntico cada vez. Lo que **no** se hace es buscar
parecidos: dos motivos que dicen lo mismo con otras palabras son dos evaluaciones, y presumir lo
contrario produciría el hallazgo contra quien hizo bien el trabajo. Ambas cosas tienen prueba.

La ventana entre el primero y el último va en el hallazgo: **minutos son peor que días**. Y una
sola misión con dos asientos de cierre no cuenta — se casa por misión distinta, verificado por
mutación.

#### El indicador de apuro, y lo que se negó a decir

*«El sistema no la resuelve; la hace visible»*. El promedio del año **excluye la ventana**: si la
mitad de los cierres del año caen en diciembre, un promedio que los cuente diría que diciembre
fue normal. Romper eso hace caer la prueba.

Sin cierres fuera de la ventana el indicador **se declara no evaluable**, no infinito. La pantalla
lo dice así: *«no es que el ritmo fuera normal — es que no hay medida»*.

#### `RN-96` punto 6, medido en vivo

Se busca por el eje de **transacción** —cuándo se registró— y no por el de vigencia. Un umbral
cargado el 28 de diciembre con vigencia retroactiva a enero es exactamente el caso que la regla
quiere ver, y buscarlo por `VigenteDesde` lo dejaría fuera. La bitemporalidad de `ADR-006` es lo
que hace posible la consulta.

Sembrado contra la base de desarrollo, la pantalla lo dijo entero: **«5 → 15, registrado el 28 de
diciembre por P-ADMIN, sin aprobar · rige desde el 01 de enero — con vigencia retroactiva»**.

#### Dos defectos que solo se vieron con la pantalla abierta

**El acta decía «coincide con el saldo de apertura» sin haber saldo alguno.** La lista de
diferencias estaba vacía porque no había contra qué compararla, no porque cuadrara — la misma
mentira que `RN-97` persigue cuando un inventario se ve completo estando incompleto. Ahora el
acta lleva el folio del saldo que cita, **nulo declarado**, y observa que no se cuadró contra
nada. Verificado por mutación.

**Cambiar el ejercicio no movía los cortes**, y el inventario salía en cero: se estaba mirando un
año contra el corte de otro. Con el arreglo, 2026 devuelve los mismos **23 renglones** que dio
`RN-97` — los dos caminos llegan al mismo número.

#### El vale entregado no se anula, y decirlo importa

`V-03` solo corre sobre un vale `Emitida`. Un vale **entregado** y sin consumir al 31 de diciembre
es dinero fuera de la caja al cierre —un problema mayor que el folio ocioso, no menor— y `RN-96`
no lo alcanza. Va en la lista **marcado**, fuera del monto por anular, con su camino nombrado:
devolución con acta u obligación de reintegro (`RN-86`). Contarlo en el monto diría que ese dinero
vuelve al fondo por efecto del acta, que es falso.

#### La ventana de cierre es parámetro con vigencia, y sin valor por omisión

Era una constante de 15 días. Ahora sale de `cierre.ventana_de_cierre_dias`, resuelta **a la fecha
del corte legal** (`RN-40`) y con el eje de transacción en el **momento del acta**: reabrir una
producida en enero reproduce lo que se vio ese día, no lo que se cargó después.

El acta declara **de qué versión salió**: *«cierre.ventana_de_cierre_dias = 45 días, versión vigente
desde el 01/01/2026»*. Un indicador que no dice contra qué ventana se midió no se puede reproducir
ni discutir años después. Medido en vivo: cerrar la versión de 15 y abrir una de 45 mueve la ventana
de 31 a 61 días sin tocar una línea.

**No tiene valor por omisión, y es deliberado.** Un «15 razonable» calcularía los motivos
compartidos y el ritmo de cierre contra un número que nadie declaró, y un lector no podría
distinguirlos de los que sí se midieron. Sin el parámetro, esos dos reportes salen **sin medir**, no
en cero, y el acta y la pantalla lo dicen con esas palabras. Cero y negativos tampoco resuelven: una
ventana de cero no deja dónde buscar, y saldría como «no hubo hallazgos».

`CatalogoDeParametros` ganó `ResolverSiHay`: **bloquea lo que decide un número que alguien va a
cobrar; devuelve nulo lo que decide si un reporte se puede evaluar.** El que bloquea sigue
bloqueando.

⚠️ **Insumo #93: nadie ha cargado la ventana.** Mientras tanto los dos controles están apagados —
declarados, pero apagados. La ficha no fija ningún número, así que no se inventa.

#### Las fechas de corte también, y producir dejó de aceptarlas

Venían del cliente: cualquiera podía pedir cualquier corte. Ahora salen de
`cierre.corte_legal_dia_y_mes` —día y mes en formato `MM-DD`— y de
`cierre.corte_operativo_dias_despues`.

**El legal se guarda como día y mes, no como fecha completa.** El parámetro rige para *todos* los
ejercicios: guardar «2026-12-31» obligaría a cargar una versión por año, y el primer enero que
nadie la cargara dejaría al sistema sin corte. Lo que la institución decide una vez es *«cerramos
el 31 de diciembre»*.

**El operativo se guarda como días después.** Cae en el año siguiente, y un «01-15» tendría que
adivinar a cuál — una institución que cerrara el 30 de junio rompería la adivinanza. De paso hace
imposible por construcción el caso que el bloqueo de cortes existe para impedir.

Se resuelven al **31 de diciembre del ejercicio**, no al corte: resolverlos a la fecha del corte
sería circular, porque hace falta el corte para saberla.

`POST /cierre-de-ejercicio` **ya no recibe fechas.** Producir el documento del cierre contra un
corte que alguien escribió en el momento lo haría afirmar sobre todo lo demás contra un criterio
que nadie autorizó. La vista previa **sí** las admite —es «qué pasaría si»— y el acta lo declara:
*«Cortes impuestos en la vista previa — NO son los parámetros de la institución»*.

**El 29 de febrero de un año no bisiesto se rechaza, no se corre al 28.** Correrlo movería el corte
un día sin que nadie lo decidiera, y ese día tiene hechos. Verificado por mutación. Quien cargue
`31-12` invirtiendo el formato lee *«dice mes 31, que no existe»*, que le dice exactamente qué hizo
mal.

Medido en vivo contra la base de desarrollo: sin los parámetros la pantalla explica qué falta
cargar y no arma nada; con ellos, el ejercicio 2026 da **corte legal 31/12/2026, operativo
15/01/2027** y los mismos **23 renglones**.

⚠️ **Insumo #94: nadie los ha cargado**, así que hoy la pantalla de cierre no arma ninguna acta.
Es el insumo #86 aterrizado a dos claves concretas.

**Tres pruebas de punta a punta cambiaron de significado al cablear esto:** pasaban con listas
vacías por falta de parámetro, no por ausencia de hallazgos. Ahora siembran la ventana y **asertan
que está** antes de mirar el resultado, para que no puedan volver a pasar por la razón equivocada.

⚠️ **El criterio de imputación entre ejercicios sigue `[C]`.** La ficha lo dice: *«confirmar con la
Gerencia Administrativa y contra SIAFI — es el tipo de detalle que cada institución resuelve
distinto»*. Hoy el ejercicio sale del año de la fecha del hecho, que es lo defendible sin norma.

#### El reporte de reversión de compromisos para ARGOS y SIAFI

**RESUELTO** (`RN-96` punto 5, `RN-81`). La razón de que exista la da `RN-81` textual: *«`RN-48`
prohíbe que SIGTI escriba en ARGOS, y hace bien. Pero de esa prohibición no se sigue que SIGTI
pueda **callar**: si SIGTI anula un compromiso de combustible y no lo reporta, el descuadre
aparece en SIAFI y nadie sabe de dónde vino»*.

**Reporta lo que se revirtió, no lo que se listó.** Sale de los folios que el acta listó *y que se
anularon*: uno listado y todavía sin anular no liberó nada, y reportarlo haría que SIAFI
revirtiera un dinero que en SIGTI sigue comprometido — el descuadre simétrico.

**El liberado va neto**, que es el caso límite que `RN-81` nombra: *«se expone el compromiso
liberado **neto**, con el detalle de lo ejecutado, no el bruto. El detalle es lo que permite
conciliar»*. Se calcula y no se recibe, para que nadie pueda escribir el bruto; y nunca es
negativo — un vale sobreejecutado libera nada, no «menos que cero», porque SIAFI leería eso como
un compromiso nuevo. Romperlo hace caer cuatro pruebas.

**El renglón sin partida presupuestaria va marcado, no omitido.** `RN-26` deja registrar el fondo
sin partida cuando el espejo de ARGOS no la tiene; ese renglón no se puede imputar en SIAFI, pero
omitirlo haría que el total no cuadrara contra la anulación que sí ocurrió. Va con la columna
**vacía y no en cero**: quien importe el archivo tiene que poder separarlo de una partida «0».

**`RN-94` está cableada.** El reporte declara período del hecho y corte de conocimiento, y el
corte entra como parámetro: el mismo período con un corte anterior a la anulación no la ve, con
uno posterior sí, y volver a pedir el primero da lo mismo que la primera vez. Hay prueba de las
tres cosas.

⚠️ **El CSV no es el formato de SIAFI**, y lo dice en la pantalla. `RN-81` punto 3: sin contrato de
API conocido —insumos #16 y #17— el mecanismo inicial es el reporte con formato acordado. Este es
el mínimo que se concilia a mano, que es lo que la regla prevé para una institución sin ARGOS.
Separador punto y coma porque el decimal local es la coma, montos con punto e invariante porque
los lee otro sistema, y campos escapados igual: una delegación llamada «Choluteca; sur» partiría
la fila.

⚠️ **La clave de vinculación con ARGOS no existe como campo.** `RN-81` punto 1 la exige —se
establece al crear la orden y no cambia en todo su ciclo— y el modelo de datos la nombra, pero el
código no la tiene. Hoy va el ULID de la misión: sirve dentro de SIGTI y **ARGOS no lo va a
reconocer**. Es el hueco que hay que cerrar antes de que este archivo sirva para lo que se hizo.

⚠️ **El combustible no se valora contra tabla paramétrica**, y el acta lo dice en vez de fingirlo:
su monto sale del comprobante. El hecho sin comprobante queda **nombrado** como hueco. El peaje sí
trae tabla —la tarifa vigente a la fecha— y el que no la tiene es el que pasó por una caseta que
el catálogo no conocía.

---

### `RN-97` — el saldo de apertura de control interno

**RESUELTA.** Es la regla que impide el abandono, y la propia ficha explica por qué hacía
falta: *«sin saldo de apertura, el mecanismo de olvido es automático y no requiere mala fe:
llega enero, el sistema arranca con reportes en cero, y una misión interrumpida en noviembre,
un préstamo vencido en agosto y una obligación de reintegro de mayo simplemente dejan de
aparecer en ninguna pantalla. **Nadie decidió abandonarlos: se abandonaron solos**»*.

#### La antigüedad no se reinicia, y ése es el punto

Se cuenta **desde el hecho original**, nunca desde el corte. `RN-97` lo llama *«la parte que
hace incómoda a la regla, y por eso mismo la que sirve»*: un expediente que llega al tercer
ejercicio con 800 días **no se puede presentar como pendiente reciente**.

El arrastre entre saldos conserva la fecha del hecho del saldo anterior —ni siquiera una
corrección de dato la mueve— y suma uno al contador. Un renglón que aparece en tres saldos
consecutivos **se ve como tal**. Verificado por mutación: romper cualquiera de las dos cosas
hace caer pruebas.

#### Las diez fuentes van, incluidas las cinco que no se pueden contar

| Fuente | Estado |
|---|---|
| Misiones sin cerrar · vales sin liquidar · obligaciones · hallazgos · imputaciones externas | **Se consultan** |
| Préstamos vencidos (`RN-63`) · interrupciones sin desenlace (`RN-70`) · reclamos de peaje (`RN-92`) · expedientes de M-12 · bitácoras sin digitar | **Declaradas, no omitidas** |

**Omitir en silencio las cinco que faltan sería el mismo abandono con formato de reporte.** El
documento dice cuáles no se pudieron consultar y por qué, y la pantalla lo pone arriba — no en
una nota al pie.

Medido contra la base de desarrollo: 23 renglones al 31/12/2026, el más viejo de 294 días,
L 8,400.00, y **5 de 10 fuentes sin consultar**.

#### El documento y el bloqueo

Se produce con **folio**, uno por ejercicio: un segundo dejaría dos inventarios del mismo corte
y el acta de cierre no podría citar cuál. Todo renglón exige **responsable nominado** —*«un
expediente sin responsable es un expediente muerto»*— y **causa tipificada**.

`RN-97` punto 4 impide cerrar el período con préstamos vencidos o interrupciones sin desenlace.
Se pueden **declarar explícitamente** con motivo, que no es lo mismo que ignorarlos.

#### Un defecto que salió al escribir la prueba

`ExigirCierrePosible` **confiaba en que quien llamara filtrara** los renglones bloqueantes. Un
endpoint nuevo que se olvidara de filtrar habría dejado el bloqueo sin efecto, o habría
detenido el cierre por cualquier pendiente. Ahora **el filtro vive dentro de la regla**.

✅ **El bloqueo del cierre ya dispara, entero.** Era *«la consecuencia más importante de este
turno y no está resuelta»*. Sus dos fuentes estuvieron declaradas y vacías durante varios turnos:
**M-12** trajo las interrupciones sin desenlace y **`RN-63`** los préstamos vencidos. Las dos
verificadas con un 409 contra la base de desarrollo. Las fuentes consultables del saldo pasaron
de **5 de 10 a 8 de 10**.

✅ **`RN-96` ya corre esa comprobación.** El acta de cierre cuadra el inventario contra el saldo
congelado renglón por renglón, y declara **el folio del saldo que cita** — nulo cuando no hay
ninguno, que no es lo mismo que cuadrar. Ver [`RN-96`](#rn-96--el-cierre-de-ejercicio-como-corte-de-imputacion).

⚠️ **Los renglones de misión se citan por ULID.** La orden de misión sigue sin folio (`RN-44`),
y un renglón que se cita con un identificador que nadie reconoce no sirve en un acta.

⚠️ **El responsable sale de quien ejecutó el último acto**, no de una asignación de
seguimiento. Es lo mejor que se puede afirmar hoy —quien tiene el expediente en la mano— pero
`RN-97` prevé reasignarlo a la jefatura cuando la persona ya no está, y eso necesita el
organigrama vivo de `M-01`.

⚠️ **La causa se deriva, no se declara.** Hoy sale de un criterio del código —lo no atribuible
a un vehículo es *fuera del control institucional*, el resto es *pendiente de gestión*—. La
regla la quiere tipificada por quien produce el saldo, y eso es una pantalla de captura que no
está.

---

### `RN-93` — el expediente de hallazgo posterior

**RESUELTA**, y con ella queda cableado el pendiente que `RN-95` dejó: **cada diferencia de
una conciliación abre expediente de forma automática**.

`RN-93` explica por qué existe: *«basta con que la reapertura de un expediente cerrado exista
para que se use, y basta con que se use una vez para que **ningún reporte histórico vuelva a
ser reproducible**. El expediente de hallazgo posterior es la salida que permite corregir el
efecto económico sin destruir la reproducibilidad»*.

#### Lo que nunca hace

**Tocar el objeto vinculado.** El servicio no tiene una sola escritura sobre el expediente de
la misión, y eso es deliberado: una `CERRADA` no se reabre, ni por auditoría. Lo que se
entrega a quien la pide es el paquete sellado tal como cerró **más** este expediente — es más
información, no menos.

La pantalla de cierre **muestra** los hallazgos vinculados (§7.5) consultándolos desde el
hallazgo, no guardando una marca en la misión: guardar algo en un expediente cerrado sería
modificarlo.

#### El asiento reverso, con el contenido completo de §8.3

| Exigencia | Qué impide |
|---|---|
| Referencia al asiento **exacto** | *«No existe el reverso genérico de la misión»*: sin destinatario, nadie sabe si ya se revirtió |
| Valor anterior **siempre** | El reporte muestra tres valores; sin el anterior sólo puede mostrar dos |
| Valor nuevo **incluso nulo** | Nulo es un valor: *sin valor correcto conocido* ≠ no declarado |
| Quien autoriza ≠ quien registró | `BD-06`. Corregirse a sí mismo un asiento cerrado es lo que la inmutabilidad impide |
| Imputación al período **corriente** | Reimputar al original haría que un reporte publicado diera otro número según cuándo se pida |
| Un asiento se revierte **una vez** | Un segundo duplicaría el efecto económico, y esa corrección de más no la rastrea nadie |

Lo último lo impone un índice único de la base, no una comprobación que el próximo endpoint
pueda olvidar. Verificado por mutación: quitar el control de imputación rompe dos pruebas.

#### Las dos fechas, y por qué son dos

`RN-93`: *«fecha del hecho y fecha del descubrimiento son campos distintos y ambos
obligatorios»*, y la antigüedad se cuenta **desde el hecho** — *«evita el incentivo perverso
más obvio: descubrir tarde para que el indicador se vea mejor»*.

El tiempo entre las dos **es un indicador por sí mismo**: un hallazgo de hace ocho meses
descubierto ayer dice algo del control, no sólo del hecho. Medido contra la base viva: hecho
del 15/03, descubierto el 20/11, **250 días después**.

#### Cero misiones es el caso interesante

El paso por caseta de un domingo, el consumo de un vehículo que ese día no tenía orden.
**La ausencia de misión es el hallazgo** (`RN-59`). El expediente se abre con cero misiones, el
vehículo cuando se resolvió y el período — pero **algo** tiene que vincular: sin ninguno de los
cuatro, un hallazgo no se puede investigar ni reportar.

#### La resolución tiene que ser cierta

Tres salidas: **con asiento reverso**, **real sin efecto económico** (el vehículo que circuló
sin orden — hay hallazgo y no hay monto), y **sin efecto** (era un error del propio
descubridor). Declarar «con reverso» sin reversos diría que se corrigió algo que nadie tocó;
declarar «sin efecto» habiendo revertido dinero sería falso **de una forma que ningún reporte
podría detectar después**. Las dos se bloquean, y la mutación lo confirma.

El error del descubridor **se cierra, no se borra**: borrarlo dejaría a quien fue señalado sin
constancia de que se le señaló y de que no procedía.

#### Y el pendiente de `RN-95`, cerrado

La conciliación ahora abre un expediente por cada diferencia, en ambos sentidos, con la fecha
del hecho de la línea y la del descubrimiento de la conciliación. Probado contra la base viva:
una línea del proveedor de marzo, conciliada en noviembre, abre su expediente sola con el
vehículo resuelto y el período — y su reverso queda imputado a noviembre con los tres valores.

⚠️ **El paquete de evidencia sellado no está** — `RN-93` punto 7: *«debe poder entregarse
idéntico al que se pudo exportar el día del cierre»*. La cadena de hash de la bitácora existe
y el sello por cola funciona; lo que falta es el **exportador** que arma el paquete y lo firma.
Sin él, «se entrega el paquete tal como cerró» es una promesa que el sistema no puede cumplir.

⚠️ **§8.3 dice que el reverso «reabre el sello de la cadena con un eslabón adicional».** Hoy el
asiento reverso vive en su propio expediente y **no se encadena a la bitácora de la misión**.
Es coherente con no tocar el expediente cerrado, pero no es lo que §8.3 describe — y decidir
cuál de las dos lecturas manda es del PO.

⚠️ **Los indicadores «antes y después del ajuste» no están** (`RN-93` punto 5). Existe el
ajuste del período como capa identificada; lo que falta es mostrarlo junto al indicador del
vehículo, que es de `M-14` reportes.

⚠️ **`tipo_de_hallazgo_posterior` es texto libre validado, no catálogo.** La regla lo declara
configurable. Hoy se exige que venga y no esté vacío, pero nada garantiza que dos expedientes
del mismo tipo lo escriban igual — y lo que no se agrupa no produce indicador.

⚠️ **`RN-96` y `RN-97` siguen pendientes.** El criterio de imputación entre ejercicios
fiscales es `[C]` y depende de SIAFI; el saldo de apertura de control interno arrastra los
expedientes abiertos con su antigüedad, y esa consulta no existe.

---

### `RN-95` — la conciliación contra fuentes externas

**RESUELTA**, y con ella el mínimo de `RN-66` y de `RN-93` que necesitaba. Nace `M-14`.

`RN-30` concilia **hacia adentro**: galones contra kilómetros, ambos registrados por nosotros.
`RN-95` lo dice sin rodeos: *«una conciliación que solo compara nuestros datos con nuestros
datos verifica coherencia interna, no veracidad. **Un registro completo y coherente puede ser
completamente falso**, y solo la fuente externa lo revela»*.

#### Tres listas, en ambos sentidos

| Lista | Qué es | Qué abre |
|---|---|---|
| Coincidentes | Cuadran | Nada |
| Solo en la fuente | El emisor lo cobra y no lo tenemos | Expediente |
| Solo en SIGTI | Lo registramos y el emisor no lo reporta | Expediente |

**Las dos últimas, no una.** Conciliar en un solo sentido dejaría fuera el caso más grave —
*«puede ser un comprobante falso, o una estación que no reportó»*—. Y **la conciliación no
presume cuál**: eso lo decide quien investiga.

El casado es por **comprobante** primero (`RN-84` lo hace único en la institución) y por
vehículo–monto–fecha después, con tolerancia, para las estaciones que no numeran el cupón.
Un asiento se casa **una sola vez**, y eso es lo que hace aparecer el **comprobante
duplicado** — uno de los tres casos que originaron `CE-28`. Verificado por mutación.

#### La jerarquía de anclas, con la placa última

`RN-66`: número de bien → chasis → motor → correlativo → **placa**. La placa va última porque
se reasigna y porque hay vehículos circulando sin ella (`RN-15`); resolver por placa primero
**atribuiría la multa del año pasado al vehículo que hoy tiene esa chapa**. Verificado por
mutación: invertir el orden rompe dos pruebas.

Lo que no se resuelve **queda no resuelto, nunca asignado por parecido**, con responsable y
plazo obligatorios. Y el expediente dice **por cuál ancla** se resolvió: no es lo mismo haber
resuelto por número de bien que por placa — la segunda admite discusión, y la advertencia va
en el texto.

**El padrón gana cuatro campos**: `BienDelInventario`, `Chasis`, `Motor` y
`CorrelativoInstitucional`. Nulos para toda la flota cargada, porque son datos de alta.

#### «No disponible» no es «conciliada»

Una institución sin tag de peaje no tiene estado de cuenta que conciliar. Ejecutar contra esa
fuente produciría **cero diferencias sobre cero líneas**, y ese cero se lee después como
conformidad — así que se bloquea, y declararla no disponible **exige decir por qué**.

#### El retraso es dato visible

`RN-95` punto 5: *«una fuente sin conciliar durante meses es en sí misma una observación de
control interno»*. Cuatro estados, y se dicen distinto:

- **Nunca conciliada** — no es cero días de retraso: es el peor caso.
- **Atrasada** sobre su periodicidad, con los días.
- **Sin periodicidad declarada** `[C]` — el retraso se mide y no se juzga.
- **No disponible** — con su razón, y diciendo que no es lo mismo que conciliada.

Cada ejecución guarda su **fecha de corte de conocimiento** (`RN-94`) y **el documento fuente**
(`RN-95` punto 6): sin ellos, dos ejecuciones con datos distintos se ven idénticas y una
diferencia no se puede volver a comprobar contra el papel del que salió.

#### Un defecto que salió al probarlo contra la base viva

El retraso decía **«hace -7 días»** cuando la última conciliación quedaba con fecha posterior a
hoy. Eso no describe nada. Ahora lo dice como lo que es: *o la fecha se capturó mal, o el reloj
del servidor no es el que se cree* — y esa fuente cuenta como atrasada, porque de ella no se
sabe nada.

⚠️ **`RN-66` completo sigue pendiente.** La regla además atribuye al **tenedor vigente a la
fecha del hecho** cuando el vehículo estaba prestado (`RN-63`) y al **conductor registrado** de
esa fecha y hora (`RN-57`). Eso necesita el expediente de préstamo y la jornada declarada, que
no existen. Hoy la imputación se resuelve al **vehículo** y ahí se detiene.

⚠️ **`RN-93` completo sigue pendiente.** Lo construido es el expediente que nace de una
conciliación —con responsable, plazo y resolución que no se borra—. `RN-93` gobierna también el
que abre un auditor revisando misiones de marzo en noviembre, que es más.

⚠️ **Las infracciones y las actas no tienen asiento propio contra el que conciliar.** La regla
las manda cruzar contra la bitácora y los expedientes de `M-12`. Hoy toda línea suya cae en
«solo en la fuente» y se resuelve al vehículo por la jerarquía — que es correcto (una multa
**no** tiene contraparte nuestra) pero no es la conciliación completa que `RN-95` describe.

⚠️ **Los contratos de integración con proveedores de combustible y de peaje no tienen insumo.**
`RN-95` lo dice literalmente: *«hay que abrirlo»*. Hoy las líneas entran por la API, es decir,
alguien las digita o las carga a mano — que `RN-95` admite (*«el formato no exime de
conciliar»*) pero que no es integración.

⚠️ **El período de calibración de la primera conciliación no está.** `RN-95` lo admite
declarado, con las diferencias agrupadas por causa. Lo que sí se respetó es lo que no admite:
**apagar la conciliación hasta que «los datos estén limpios»**.

---

### `RN-37` — la coherencia de la secuencia de casetas

**RESUELTA. M-18 queda completo**: las seis reglas del módulo corren.

`NRM-10` lo pide textual: *«El sistema debe correlacionar peaje × kilometraje × ruta
autorizada. Un peaje de Yojoa en una misión autorizada a Choluteca es un hallazgo, y **el
sistema tiene que producirlo solo**. Esto es exactamente lo que busca el auditor del TSC:
correlación, no comprobantes archivados»*.

Verificado contra la base de desarrollo: ruta congelada hasta Comayagua, paso registrado en
Siguatepeque → hallazgo, con el mensaje que nombra las dos lecturas posibles.

#### Cuatro dimensiones, y el dictamen dice cuáles pudo mirar

| Dimensión | Qué cruza | Cuándo no se evalúa |
|---|---|---|
| Geográfica | Salto sobre una caseta activa intermedia | Algún punto sin corredor y kilómetro |
| Temporal | Velocidad implícita entre casetas contiguas | Sin `velocidad_media_maxima` `[C]`, o reloj no confiable |
| Contra la ruta autorizada | Peaje en punto no congelado al aprobar | Sin estimado congelado — ruta abierta |
| Contra el kilometraje | El piso de km que la secuencia obliga vs. `T-18 − T-14` | Sin los dos odómetros |

**«Sin hallazgos» no es «coherente».** `Coherente` exige las dos cosas: cero hallazgos **y**
las cuatro dimensiones evaluadas. `RN-37` lo pide para la ruta abierta y vale para todas: *«se
marca así explícitamente para que la ausencia de hallazgos no se lea como conformidad»*. Un
dictamen que no pudo mirar nada no es conformidad: es silencio. Verificado por mutación.

#### El defecto que la primera versión tenía

**Marcaba como incoherente todo viaje de ida y vuelta.** La dimensión geográfica contaba
*cambios de sentido*, y un retorno tiene uno — así que la regla habría producido un hallazgo
en cada misión del año, que es literalmente lo que ella misma advierte.

Lo detectó la prueba `Un_viaje_de_ida_y_vuelta_por_las_mismas_casetas_es_COHERENTE`, escrita
antes de mirar el resultado. Reescrita: lo imposible **no es cambiar de sentido** —eso es el
retorno, y dos veces es `CE-08`— sino **saltar sobre una caseta que había que cruzar**. Esa
lectura además encuentra la omisión, que es la otra mitad de lo que el auditor busca.

#### Sin poder declarar un desvío, la regla no se podía encender

`RN-37`, casos límite: Honduras tiene derrumbes y cierres de carretera con regularidad, y sin
la capacidad de declararlos la regla *«produciría hallazgos falsos en masa»*. Un control que
grita todos los días muere en tres meses, igual que el rendimiento inventado de `RN-30`.

Por eso entra `POST /peajes/desvios` — **el mínimo que `RN-37` necesita de `RN-76`**, no
`RN-76` completo. Un desvío que cubre todos los pasos de una incoherencia la marca justificada
y **no la borra**: que existió y que alguien la explicó son dos hechos, y el auditor pregunta
por los dos. Verificado por mutación.

#### Lo que hubo que construir para poder cruzar

- **`Corredor` y `Kilometro` en el punto** — `RN-37` punto 1: es lo que permite ordenar
  geográficamente. Nulos dejan la dimensión sin evaluar; deducir el orden del **orden de
  captura** invertiría la respuesta en toda misión de retorno.
- **El estimado congelado** (`RN-35` punto 4, `RN-41`) — es lo único contra lo que la tercera
  dimensión puede juzgar. Se congela una sola vez: dos rutas autorizadas dejarían la pregunta
  sin respuesta única.
- **Un dictamen por vehículo, no por misión** — en una sustitución en ruta, dos vehículos
  pasan por la misma caseta a horas distintas legítimamente. Meterlos en la misma secuencia
  fabricaría intervalos imposibles a partir de dos viajes correctos.

**Y el orden es por fecha del hecho, no por captura** (`RN-46`): el motorista que anota todos
los pasos al final del día no cometió una incoherencia de secuencia.

#### Un texto que había quedado mintiendo

La pantalla de cierre decía *«todavía no se evalúan la coherencia de la ruta contra los peajes
(M-18)»* — justo debajo del panel que acababa de evaluarla y de mostrar un hallazgo. Corregido:
ahora dice que `RN-37` sí se evalúa y que lo que falta es la cadena de trazabilidad de `M-14`.

⚠️ **`velocidad_media_maxima_por_tipo_vehiculo` no está definida — `[C]`.** La regla la quiere
**por tipo de vehículo** —un bus y una moto no van igual— y el parámetro es hoy una sola cifra
nula. Las otras tres dimensiones se evalúan igual y el dictamen dice que la temporal no.

⚠️ **`RN-76` completo sigue pendiente.** El estado en ruta declarado por el motorista incluye
el estado del vehículo, las esperas en sitio y el seguimiento de `M-19`. Acá está sólo el
hecho que la liquidación consume.

⚠️ **La tipificación «peaje pagado sin paso registrado» no se puede producir todavía.** Sale
del estado de cuenta del tag CoviPass, que es conciliación contra fuente externa (`RN-95`) y
no existe. `RN-37` la nombra como el hallazgo grave —uso del vehículo fuera de misión—, y hoy
el sistema no lo puede ver.

⚠️ **El reporte de peajes por vehículo, motorista, dependencia y período** (`RN-37` punto 4)
no está. El dictamen es por misión; el agregado listo para entregar al TSC es otra vista.

⚠️ **El hallazgo de `RN-37` no bloquea el cierre limpio.** La regla es «advertencia con
hallazgo» y los criterios de cierre de la misión son `H-01`…`H-11` de §7.2 — **la autoridad es
la máquina de estados**, y agregarle un criterio desde M-18 sería resolver en silencio algo
que le toca decidir al PO. El dictamen se muestra junto al cierre para que quien cierra lo vea
antes de decidir.

---

### M-18 Peajes — `RN-33` a `RN-36` y `RN-38`

**RESUELTAS cinco de las seis reglas del módulo.** Catálogo con vigencias, derivación de
categoría, tarifa a la fecha del hecho, estimación desglosada, exoneración y paso por caseta
con discrepancia. Falta `RN-37` — la coherencia de la secuencia de casetas contra la bitácora.

#### El error que el módulo existe para no cometer

`NRM-10`, con evidencia `[V]`: *«un vehículo liviano tiene 2 ejes y paga L. 22. Un "Vehículo
de 2 Ejes" paga L. 90. **Ambos tienen dos ejes**»*. Y la consecuencia, textual: *«cualquier
modelo que use `numero_ejes` como única llave para resolver la tarifa está mal y va a cobrar
cuatro veces de más a cada pickup de la flota»*.

**No hay una sola línea de aritmética sobre ejes en todo el módulo.** La derivación es una
**tabla cargada** con filas por prioridad: la excepción nominal de la SAPP —H-100, K2700,
Sprinter— es una fila de prioridad alta, no un caso especial en el código. Medido: un pickup
de 2 ejes estima L 44 por dos cruces; un camión de 2 ejes, L 180.

#### El otro error: que el cobro de la caseta se vuelva la verdad

Entre agosto y septiembre de 2025 COVI-H cobró **L 90 en lugar de L 22** a los H-100, K2700 y
Sprinter, y la SAPP tuvo que ordenarle suspenderlo el 17/09/2025 `[V]`. `RN-36` es taxativa:
*«si el sistema ajustara la categoría del vehículo al cobro recibido, el error de la caseta se
volvería la verdad institucional y **el reclamo nunca ocurriría**»*.

El paso guarda **las dos categorías en columnas separadas**. `/peajes/discrepancias` es el
insumo del expediente ante la SAPP. Probado contra la base de desarrollo con el caso exacto:
L 90 pagados, L 22 esperados, L 68 de diferencia, y la categoría del vehículo intacta.

#### Nada vale cero por omisión

| Situación | Qué hace |
|---|---|
| Sin tarifa cargada | Línea **no valorada**, con mensaje accionable: punto, categoría, fecha y a quién pedírsela |
| Sin categoría resuelta | Igual, y nombra **el atributo que falta** |
| Sin estado del punto declarado | No se supone activo. Suponerlo estimaría de más sobre una caseta que quizá cerró |
| Punto cerrado | Cero **con fundamento**, y dice que no es exoneración del vehículo |
| Vehículo exonerado | Cero **con el fundamento en la misma línea** |

`RN-34` es explícita: *«el sistema no debe calcular un valor por defecto»*. Un cero
indistinguible de un error es peor que la ausencia declarada. Y el estimado **no bloquea la
aprobación** (`RN-35`): sale marcado no disponible con su causa, porque el sistema arranca sin
tarifas cargadas y detener toda aprobación pararía la institución por un dato de catálogo.

#### El desglose es la regla, no una preferencia

Tegucigalpa → San Pedro Sula atraviesa las tres estaciones del Corredor Logístico; ida y
vuelta son **6 cruces** `[V]`. Sin desglose el autorizador no puede distinguir un estimado
correcto de uno que duplicó un cruce, y *«el estimado deja de ser un control para volverse un
trámite»*. El sistema cuenta **cruces, no puntos distintos**.

#### Las vigencias soportan lo que ya pasó

2026: anuncio el 08/01, suspensión hacia el 15/01, prórroga al 15/02, nuevo anuncio el 27/02 y
confirmación de la SIT el 28/02 de que no habría incremento. La tabla admite **vigencias
cortas**, **cierre anticipado** de una vigencia abierta y **aumentos retroactivos** — el eje de
transacción de `ADR-006` reproduce el número que se pagó y el corregido, que son dos preguntas
legítimas. Verificado por mutación.

⚠️ **La tarifa vigente hoy sigue sin confirmarse — `[C]`, insumo #21.** `NRM-10` instruye **no
cargar ninguna tarifa** hasta confirmarla con COVI-H o la SAPP: hay contradicción abierta entre
el comunicado de la SIT y un agregador comercial. Lo sembrado en desarrollo lleva fuente
`[C] SIN CONFIRMAR — dato de desarrollo` a propósito, y la pantalla lo muestra.

⚠️ **El Artículo 51 de la Ley de Tránsito no se pudo transcribir — `[C]`, insumo #23.** El PDF
oficial es un escaneo sin capa de texto, así que la matriz de derivación **no se puede fijar**.
Toda categoría sale marcada **provisional**, siempre, y eso viaja al estimado: una categoría
provisional mostrada igual que una firme se cita después como si lo fuera.

⚠️ **La lista oficial de exoneraciones no existe — `[C]`, insumo #22.** `NRM-10` la califica
como *«lo que decide cómo se construye M-18»*. El valor por defecto es **paga**, y ninguna
exoneración se carga sola: la suposición contraria —«somos del Estado, no pagamos»— es la más
probable y la más costosa, porque produce estimados en cero y un motorista pagando de su
bolsillo en Zambrano.

⚠️ **La frontera con ARGOS sigue abierta — `[C]`, insumo #25.** Si el peaje se financia con el
viático es de ARGOS y M-18 se solapa. `NRM-10` exige resolverlo **antes de escribir historias
de M-18**. La estimación sobrevive en cualquier escenario: quien paga cambia, la necesidad de
estimar no.

⚠️ **`FichaTecnica` gana `NumeroDeEjes`, nulo para toda la flota cargada.** Es dato de alta y
`M-03` no tiene pantalla de alta. Sin él la categoría queda **no resuelta** —no adivinada— y
`BD-07` no deja programar ese vehículo.

⚠️ **`RN-37` queda pendiente**: la coherencia de la secuencia de casetas contra la bitácora. Es
lo que detecta el odómetro manipulado —*«un vehículo que declara 980 km pero sólo cruzó una
caseta dos veces está diciendo dos cosas incompatibles»*—. El dato ya está: cada paso lleva su
odómetro y su momento. Falta el cruce.

**Hallazgo de método:** la primera prueba de mutación sobre la derivación **no hizo fallar
nada** — el peso discriminaba antes que la clase, así que ese campo no estaba cubierto por
ninguna prueba. Se agregó una con dos filas que difieren **sólo en la clase**, y entonces sí
muerde.

---

### Las existencias del tanque institucional — `RN-83` punto 5

**RESUELTA.** El tanque tiene libro, el despacho descuenta, y lo que se declaró salido sin
que ningún tanque lo anotara sale en una lista.

**`FuenteDeAbastecimiento.TanqueInstitucional` existía desde `RN-83`, se podía elegir en la
pantalla, y no descontaba de ninguna parte.** La regla dice *«descuenta de las existencias del
tanque»* y no había existencias: el galón quedaba imputado al vehículo y el tanque de la sede
no se enteraba. **Igual de invisible que antes de la regla, con la apariencia de estar
registrado** — que es peor.

#### La existencia es la suma del libro

P-1 aplicado a una cantidad. Seis asientos: `E-01` recibir, `E-02` despachar a vehículo,
`E-03`/`E-04` trasiego, `E-05` constatar, `E-06` ajustar. **No hay columna**
`existencia_actual`: se desincroniza el primer día en que dos despachos entren a la vez, y
desde ahí el arqueo compara la realidad contra un número que ya no es la suma de nada.

#### Los bloqueos del despacho

| Qué | Por qué |
|---|---|
| No se despacha lo que no hay | Un libro en negativo no describe ningún tanque: describe ingresos que nadie asentó |
| Quien despacha ≠ quien recibe | `RN-83` punto 5 remite a `RN-01`. Es el control más elemental de una bomba y el más fácil de perder |
| Un tanque despacha su combustible | Llenar un diésel del tanque de gasolina cuadra en galones y es imposible en la realidad |

El despacho y el abastecimiento van **en la misma transacción**: si el despacho falla, el
abastecimiento tampoco entra, y no queda un galón imputado a un vehículo contra un tanque que
nunca lo soltó. Verificado por mutación — romper el descuento hace caer cuatro pruebas.

#### Lo que NO se bloquea, y es el punto

El motorista que declara desde el campo *«cargué de la cisterna»* reporta un **hecho
consumado**: no tiene el tanque a mano, no puede firmar el despacho, y `RN-83` prohíbe omitir
el registro. Rechazarlo no devolvería el combustible al tanque — **lo sacaría del denominador
de `RN-30`, que es donde más falta hace**.

Ese galón entra y queda en `/tanques/despachos-sin-respaldo`: **el préstamo invisible de
`CE-23`, vuelto lista**. Al probarlo contra la base de desarrollo, el reporte encontró de
entrada un abastecimiento preexistente —35 galones declarados del tanque, sin ningún tanque
que los registrara— que hasta hoy no lo veía nadie.

#### El arqueo mide y no ajusta

Misma disciplina que `RN-86` punto 4 impone al plazo vencido: **nunca cuadre automático**. Un
arqueo que corrige el libro por su cuenta hace desaparecer la diferencia en el mismo acto que
la descubre, y la única pregunta que un arqueo existe para contestar —*¿cuánto falta?*— deja
de tener respuesta.

`E-05` deja constancia de lo medido y nombra la diferencia **aunque sea cero**. `E-06` es otro
acto, de otro, con motivo tipificado —merma técnica, error de registro, faltante sin causa,
sustracción— y fundamento escrito. **No hay opción «diferencia» a secas**, por la misma razón
que `CE-26` §3 da para el faltante del fondo.

Y un tanque nunca arqueado **no está cuadrado: está sin verificar**. La diferencia se muestra
nula, no cero.

#### El trasiego mueve los dos lados o ninguno

`RN-83` lo saca expresamente del abastecimiento: *«es movimiento de existencias y tiene su
propio circuito»*. Registrar sólo la salida haría que el combustible se evaporara del sistema
entero en vez de sólo de un tanque — **la forma exacta en que un faltante se disfraza de
traslado**. Los dos asientos entran en una sola transacción.

⚠️ **Que la institución tenga almacenamiento propio no está confirmado — `[C]`, insumo #36.**
`HU-041` advierte que *«cambiaría el circuito completo de M-09»*, y `RN-28` lo repite. Si no
lo tiene, no se da de alta ningún tanque y el panel no aparece. Lo que no podía seguir pasando
es que la fuente se declarara y no descontara de nada.

⚠️ **Con qué documento se despacha desde la cisterna es `[C]`** — insumo #1, vía `HU-083`. Hoy
el asiento lleva persona, puesto, vehículo, receptor y motivo; el **folio** del vale de
despacho, si la institución lo usa, no está.

⚠️ **El rango de merma admisible no existe — `[C]`, insumo #1.** `RN-69` usa merma esperada de
catálogo para carga a granel y acá no hay equivalente. El sistema registra la merma declarada
y **no puede decir si es razonable**.

⚠️ **La capacidad del tanque se guarda y no se comprueba.** Un ingreso que rebalse no se puede
rechazar —el combustible ya entró, y rechazar el asiento lo saca del libro, no del tanque—, así
que lo único útil sería una alerta, y las alertas persistidas son de `M-14`. Está dicho en el
código en vez de fingir una validación.

---

### El circuito de reintegro — `RN-86`

**RESUELTA.** La obligación de reintegro existe como entidad con ciclo propio, el bloqueo de
nueva asignación corre, y el arqueo por persona contesta la primera pregunta de un arqueo.

**`RN-29` numeral 4 daba la entidad por existente y no existía.** La regla decía que el
faltante *«genera automáticamente expediente de deducción de responsabilidad»*, y no había
obligación de reintegro en ninguna regla ni en ninguna máquina de estados — así que, en
palabras de `RN-86`, **el cobro se perdía cuando la misión cerraba**: el expediente se
archiva, el hallazgo queda como marca, y el dinero no vuelve.

Ahora la obligación vive **fuera del expediente de la misión**, con seis movimientos —`R-01`
nominar, `R-02` notificar, `R-03` descargo, `R-04` resolver, `R-05` dejar sin efecto, `R-06`
pagar— y su propio diario. La misión cierra por `T-22` y la obligación sigue viva.

**No nace en la liquidación**, y eso es de `RN-86` punto 5 y de `RN-74`: quien liquida
constata el hueco; nominar a una persona responsable de él es otro acto, de otro, con su
competencia registrada.

#### Las dos mitades del bloqueo, que no son la misma

`RN-86` las enumera aparte y `HU-078` le da un escenario a cada una:

| Qué bloquea | Qué es |
|---|---|
| Obligación abierta a cargo | Una deuda que **alguien determinó** |
| Saldo **vencido** | Dinero que no volvió y que **todavía nadie determinó** |

Bloquear sólo por la primera dejaría pasar todo el intervalo entre que el plazo vence y que
alguien se sienta a nominar — que es, según `CE-26`, **justo donde nace el faltante**.

El bloqueo entra por `AsignacionDeCombustible.Emitir`, igual que `RN-32` y `RN-26`: quien
construya otra puerta de emisión lo hereda sin acordarse de llamarlo. Y el parámetro que lo
alimenta es obligatorio, no opcional — **un endpoint nuevo que se olvide de pasarlo no
compila**, en vez de emitir sin verificar. Verificado por mutación.

#### Lo que deliberadamente NO bloquea

- **El saldo dentro de plazo.** El motorista que volvió anoche tiene dinero afuera y está en
  su derecho.
- **La obligación a favor del servidor.** Negarle un vale a quien puso de su bolsillo sería
  castigarlo por haber puesto. `CE-26`: *«un sistema que solo mide lo que el servidor le debe
  a la institución no es un sistema de control: es un sistema de cobro»*.

#### Pagar antes de que se resuelva salda, y no borra

`CE-26` nombra la práctica: *«se le da tiempo al motorista para que lo reponga; si repone, no
queda registro de que hubo faltante»*, y sentencia que **un control que se activa sólo cuando
la persona no coopera no es un control**. Por eso `R-06` se admite desde cualquier estado
vivo: pagar salda la deuda y el asiento de nominación sigue ahí con su causa.

El **abono parcial baja el saldo y no avanza el ciclo** — el sistema nunca redondea ni ajusta
para cuadrar. Y cobrar de más se rechaza: si la institución recibió un excedente, es otro
hecho económico con su propio asiento.

#### La válvula, y por qué es por misión

Un bloqueo sin salida se esquiva por fuera del sistema —emitiendo a nombre de otro motorista—
y entonces el registro **miente sobre quién recibió el dinero**, que es peor que no haber
bloqueado. El levantamiento es acto de ACT-08 con motivo escrito, **atado a una orden**: uno
por persona sin fecha de fin sería un permiso permanente que nadie se acuerda de revocar.
Queda en el indicador que `RN-86` pide, no sepultado dentro del vale que lo usó.

#### El plazo en días hábiles

`CalendarioDeDiasHabiles.SumarDiasHabiles`. Hábiles y no corridos porque **la devolución es un
acto presencial en horario de caja**: un plazo corrido vencería el sábado a alguien que no
tiene a quién entregarle el dinero, y el bloqueo caería sobre quien no pudo cumplir. El día de
partida no cuenta — el motorista que retorna el jueves a las 8:40 de la noche no tuvo el
jueves. `[I]`, práctica común; el articulado es del insumo #32.

⚠️ **`plazo_devolucion_saldo` no está definido — `[C]`, insumo #32.** Y nulo no es cero: con
cero, todo saldo estaría vencido el mismo día del retorno y el bloqueo caería sobre la flota
entera por un dato que nadie entregó. Mientras siga nulo, **el arqueo muestra quién tiene
cuánto y desde cuándo** —que es la primera pregunta y hoy no la contesta nadie— pero **no
bloquea por saldo**. La mitad de las obligaciones nominadas funciona sin el parámetro.

⚠️ **`CE-26` §1 propone el estado `PENDIENTE_DE_DEVOLUCION` en la máquina de la asignación, y
§10.1 —que es la autoridad— no lo tiene.** El saldo afuera se **calcula** en vez de agregarse
al enum: la sustancia de `CE-26` es que el hueco sea visible, y eso lo da el arqueo. Agregarle
un estado a la máquina autoridad desde el módulo que la consume sería resolver la
contradicción en silencio. **Queda como hallazgo para el PO** — decidir si §10.1 lo incorpora
o si `CE-26` se corrige.

⚠️ **Que quien levanta el bloqueo sea ACT-08 no se verifica.** El mapa rol↔puesto es de la
institución, `[C]` insumo #1. Se registra persona, puesto declarado, fecha y motivo — que es
lo que después permite revisar quién los firmó. No se finge que el puesto se validó.

⚠️ **El arqueo muestra el ULID del motorista, no su nombre.** El padrón lo tiene y el arqueo
todavía no lo cruza. `HU-078` espera leer *«de la misión OM-2026-0491»*, y la orden de misión
**tampoco tiene folio** (`RN-44` reserva rangos por delegación para eso). Se muestra lo que
identifica sin ambigüedad en vez de inventar un correlativo.

**Lo que queda fuera y es de otro:** el cobro por planilla es de Talento Humano y
Administración (`HU-078`, fuera de alcance), y la tipificación completa del faltante al
liquidar es `HU-089`. `CausaDelReintegro` sólo tiene las tres causas que `RN-86` declara
generadoras más el peculio — tenerlo aparte impide que el día que crezca el catálogo de
liquidación crezca con él la lista de cosas que nominan a una persona.

---

### El remanente en tanque — `RN-83` punto 3 y `CE-07`

**RESUELTA.** `ReglasDelRemanente` calcula lo que quedó en el tanque, y la conciliación lo
**separa del consumo de la misión**.

**La fórmula es de `CE-07`**, textual: `consumido por la misión = entregado − devuelto en vales
− remanente en tanque atribuible`. Traducido a lo que el sistema tiene: lo que la misión quemó
es lo que entró al tanque menos lo que quedó de más.

**Sin esa resta, un vehículo que vuelve con el tanque servido aparece consumiendo de más** — de
un combustible que sigue en el tanque, a la vista de cualquiera que abra la tapa. Medido: sale a
un cuarto, carga 60 galones, vuelve a tres cuartos. Antes: 60 galones consumidos. Ahora:
**abastecidos 60, consumidos 30**, y los otros 30 declarados como remanente.

Y el caso inverso, que `RN-30` nombra: sale lleno y vuelve a un cuarto habiendo cargado 20 →
**consumió 65**, porque quemó los 45 que ya llevaba. Sin el nivel, ese exceso no se veía.
Verificado por mutación.

**La ficha técnica gana `CapacidadDeTanqueGalones`.** Es dato del fabricante, no de la
institución, y por eso vive ahí y no en los parámetros. Sin ella, las lecturas en fracción **no
se convierten**: un octavo no es una cantidad hasta saber de qué tanque, y suponer una capacidad
produciría un remanente que entra directo al denominador del rendimiento y que después nadie
distinguiría de uno medido.

**Nulo es «no se pudo calcular», no cero**, y la explicación va siempre — un remanente ausente
sin razón se lee como un tanque que no se movió. Cuando no se puede, el consumido iguala a lo
abastecido porque **es lo mejor que se puede afirmar**, y la evidencia dice que eso no es lo
mismo que un remanente de cero.

**La pantalla muestra las dos cifras** cuando difieren. Mostrar sólo la consumida escondería que
parte de lo abastecido sigue en el tanque; mostrar sólo la abastecida haría que el vehículo
pareciera consumir de más.

⚠️ **El destino contable del remanente sigue sin decidirse — `[C]`.** `CE-07` nombra las tres
salidas —se abona al fondo, se imputa a la siguiente misión de ese vehículo, o sólo se
documenta— y deja abierta cuál rige (insumo #7). El sistema **documenta**, que es la única de
las tres que se puede hacer sin saber cuál manda, y no mueve ningún cuadre. Lo que el caso
prohíbe —*«que un tanque lleno pagado con fondo de esta misión desaparezca del expediente»*— ya
no puede pasar.

⚠️ **La capacidad del tanque no está cargada para la flota real.** Se sembró en las pruebas
para poder ejercer la conversión; en desarrollo los vehículos la tienen nula, así que sus
lecturas en fracción caen en «no calculable». Es un dato de alta de vehículo, y `M-03` no tiene
pantalla de alta.

---

### El nivel de tanque desde el campo

**RESUELTA.** `campo/nucleo/SalidaYRetorno.ts` prepara `T-14` y `T-18` con odómetro **y nivel**,
y la sincronización lo lleva hasta el asiento.

**⚠️ El nivel llegaba y se descartaba en silencio.** La API lo aceptaba desde que `RN-83` se
construyó, pero la ruta de sincronización —**la única que el cliente de campo usa**— armaba el
odómetro sin él. El dato se tecleaba en el predio, se sincronizaba, y no aparecía en ninguna
parte.

Es el peor de los tres modos de fallar: no hay error, no hay hueco visible, y el reparo
`NivelDeTanqueDispar` de `RN-30` **nunca se activaba porque el nivel nunca estaba**. Verificado
por mutación.

**«No lo leí» y «marcaba cero» son cosas opuestas**, y un campo numérico vacío no las
distingue. El módulo de campo obliga a elegir entre las dos: un nivel con su escala, o una
ausencia **con su razón**. `RN-80` manda declarar el campo no consignado y no estimarlo, y
declararlo sin decir por qué deja la ausencia sin nada que reclamar — no se sabe si faltó
porque el indicador estaba averiado o porque nadie se acordó, y sólo la primera se corrige.

La razón viaja **con la lectura**, porque *es* la lectura en su forma ausente, y llega hasta el
diario: *«nivel de tanque NO CONSIGNADO (`RN-83`): el indicador está averiado — orden de trabajo
2026-0071»*.

**Lo demás que el módulo defiende**, y que hasta hoy nadie ejercía desde el campo:

- **En fracción del indicador el nivel va de 0 a 1.** Quince en fracción es un error de escala
  —quien lo tecleó quiso decir galones—, y aceptarlo daría un remanente de mil quinientos por
  ciento que nadie podría interpretar.
- **El odómetro de retorno menor que el de salida se detiene en el predio**, donde quien lo
  tecleó tiene el tablero delante. **Salvo en el retorno constatado**, donde el vehículo ya
  está en el predio y negarse lo dejaría secuestrado por un trámite (`RN-79`, `HB3-04`).
- **Volver con el mismo odómetro exige justificación.** No bloquea el hecho, pero no pasa en
  silencio: es el patrón de la misión que nunca se hizo.

---

### La captura de abastecimientos en el cliente de campo

**RESUELTA.** `campo/nucleo/AbastecimientoEnRuta.ts`, y `A-01` entra por `POST /sincronizacion`.

**El galón que desaparecía.** El motorista que llena de una donación camino a La Mosquitia, o
que pone de su bolsillo porque el vale no alcanzó, **no tenía dónde anotarlo**. Ese galón no
llegaba al denominador de `RN-30`, y su ausencia se lee como rendimiento imposiblemente bueno
— es decir, como si alguien hubiera despachado combustible sin registrarlo. **Que es verdad:**
lo que faltaba era poder registrarlo donde ocurre, que es sin red.

**`A-01` no es una transición**, y el código lo dice: un abastecimiento no mueve un expediente
ni un vale, es un registro que cuelga del vehículo y puede llegar **sin misión**. Viaja por el
mismo canal porque eso da una sola cola, una sola idempotencia y un solo acuse — los tres que
`RNF-03` obliga a que funcionen sin fallo.

**Tercer diario, misma regla de idempotencia.** El abastecimiento gana su `IdDeCaptura` con
índice único, y la comprobación de «esto ya llegó» ahora mira los tres. Verificado por
mutación: quitar el tercero rompe la prueba del reenvío.

**Y las comprobaciones se comparten.** `CargaDeCombustible.ts` guarda lo que toda carga exige
—galones, estación, odómetro que no retrocede— porque el motorista hace **el mismo acto** en
los dos casos: mete combustible al tanque y anota el tablero. Duplicarlas las dejaría
divergir, y la primera vez que alguien corrija una y no la otra el mismo dato quedaría
aceptado por una puerta y rechazado por la otra.

**A quien no genera factura no se le pide causa**, igual que en el servidor: el tanque de la
sede y una donación no emiten papel. La regla está escrita **una vez de cada lado** porque el
dispositivo tiene que poder decidirlo sin red.

⚠️ **El expediente es opcional sólo para `A-01`.** Las demás transiciones lo siguen exigiendo,
y el endpoint lo rechaza con motivo si falta.

---

### La pantalla de abastecimientos

`PanelDeAbastecimientos`, montado en el **cierre** —donde se concilia y el numerador tiene que
verse— y en la **programación**, donde el vehículo está en ruta y carga.

**Va al lado de los vales, no dentro**, porque son dos preguntas distintas: el vale contesta *qué
se hizo con el dinero del fondo*; esto contesta *cuántos galones entraron a este tanque*.
Mezclarlos haría que el despacho del tanque de la sede pareciera un movimiento de caja, y no lo
es — no salió de ningún folio.

**La composición se calla cuando todo vino del fondo.** Decir «100% del fondo» en cada misión
entrena a saltarse la línea, y con ella se pierde la vez que sí decía algo.

**El comprobante sólo aparece donde debería haber papel.** Mostrarlo siempre haría que la casilla
se rellenara con «no aplica» y dejara de leerse.

Verificado en pantalla: registrar 35 galones del tanque institucional sobre una misión que tenía
40 del vale llevó el denominador de **40 a 75 galones**, y la composición apareció desglosada por
fuente — que es exactamente lo que `RN-30` punto 4 manda exponer.

---

### ⚠️ Los consumos anteriores a `RN-83` no tenían abastecimiento

Y sin él sus galones desaparecían del denominador: **el dictamen decía «la misión no cargó
combustible» sobre una misión que sí cargó**. Una afirmación falsa es peor que un hueco.

Corregido con una migración de datos que crea el abastecimiento de cada `V-04` histórico. El
vehículo sale de la reserva de la misión; **los expedientes anteriores a `RecursosTomados` no se
rellenan** — inventarles un vehículo dejaría el galón cargado a un tanque que quizá no fue el
suyo, y eso es peor que dejarlos fuera.

---

⚠️ **Lo que `RN-83` todavía no hace:**

- **No descuenta de las existencias del tanque institucional** (punto 5). Eso es un inventario de
  combustible que no está construido: el abastecimiento se imputa al vehículo, pero del otro lado
  no hay de qué restar.
- **El reintegro se marca y no se tramita.** `RN-86` y el insumo #37 (`[C]`) deciden si la
  institución reintegra. Mientras tanto queda registrado: la práctica ocurre igual y hoy quedaría
  fuera de todo registro.
- **El remanente en tanque al retorno no se separa del consumo** (punto 3). Su destino contable
  es parámetro institucional y no está declarado.
- **No hay pantalla.** Los abastecimientos que no pasan por vale se registran por API, y el
  cliente de campo tampoco los captura todavía.

---

### `RN-30` — la conciliación galonaje–kilometraje

**RESUELTA.** `ReglasDeConciliacion` calcula el dictamen y `V-09`/`V-10` lo aplican. Con esto la
conciliación deja de ser un booleano que mandaba el cliente.

**Lo que el auditor pregunta**, según `NRM-01` citado por la regla: *«el auditor no busca
comprobantes, busca correlación entre consumo, kilometraje y misión autorizada. Un sistema que
solo archiva facturas no responde a lo que se le va a preguntar»*. Este cálculo es esa
correlación.

**Las dos direcciones, y la segunda es la que importa.** Un control ingenuo busca consumo de
más. `RN-30` exige también lo contrario: **un rendimiento imposiblemente bueno casi siempre
significa un despacho que no se registró** — los galones anotados no explican los kilómetros
porque el vehículo cargó de una fuente que nadie apuntó.

**Los dos umbrales son independientes**, que es literal en la regla: *«un exceso de consumo del
20% y un ahorro del 20% no significan lo mismo»*. Verificado por mutación: igualarlos rompe la
prueba.

| Dictamen | Qué significa |
|---|---|
| `NoEvaluable` | **No se pudo comparar.** No es «conforme» — un control que tranquiliza sin haber comparado es peor que ninguno |
| `NoConcluyente` | Se calculó y el resultado no significa nada: odómetro averiado, nivel de tanque dispar, espera con motor encendido. **Se conserva** para el agregado, que `RN-30` declara válido |
| `DentroDeUmbral` | · |
| `ConsumoExcesivo` | Más galones de los que el recorrido justifica |
| `RendimientoImposible` | Menos galones de los que el recorrido exige |

**Quien concilia ya no elige.** Antes la petición llevaba `dentroDeUmbral`, y eso dejaba a quien
revisa decidiendo si su propio caso era hallazgo: en seis meses no habría una sola desviación.
Es el mismo invariante de §7.2 sobre el cierre — el criterio decide, la persona lo confirma con
su causa. Y la causa se exige **sólo si hubo hallazgo**: pedirla siempre enseña a rellenar el
campo con cualquier cosa.

**El `rendimiento_esperado` sigue siendo `[C]`** y se devuelve nulo — un pick-up y un bus no se
parecen en nada, y `RN-30` advierte lo que pasa al inventarlo: *«el sistema producirá hallazgos
falsos y en tres meses nadie los mirará»*. Lo que sí hay es la **propuesta del histórico del
propio vehículo**, que la regla autoriza expresamente, marcada como propuesta y con su origen
viajando hasta el asiento. Sin ella la conciliación no correría nunca y el control existiría sin
funcionar.

⚠️ **La propuesta compara el vehículo consigo mismo.** Si el desvío es constante desde siempre,
la media ya lo incorporó y todo se ve conforme. Eso no se arregla con más datos del mismo
vehículo: se arregla con el valor institucional y con el agregado por dependencia.

**Los umbrales sí se declaran, y no contradice lo anterior:** el esperado es un hecho sobre un
vehículo concreto que sólo la institución conoce; los umbrales son cuánta desviación se tolera
antes de mirar, y ahí la regla fija la forma. Los números siguen siendo `[C]` y la versión lo
dice.

**Y el cierre dejó de mentir.** La pantalla afirmaba *«consumo dentro de umbral, ruta coherente,
fondo comprobado y cadena de trazabilidad completa»* — cuatro verificaciones, ninguna existía, y
un expediente cerrado sobre esa frase parecía revisado. Ahora `H-01` **se detecta de verdad**
desde los vales conciliados con desviación, y lo que no se evalúa —peajes de `M-18`,
trazabilidad de `M-14`— se dice.

⚠️ **Lo que la conciliación todavía no ve:**

- **Sólo entran los galones del fondo.** `RN-83` manda contar *todo* abastecimiento, venga de
  donde venga. Un despacho desde el tanque institucional no pasa por ningún folio y **no existe
  para el cálculo** — y es exactamente lo que produce un rendimiento imposiblemente bueno. Sin
  `RN-83`, la regla señala un síntoma cuya causa el sistema no puede registrar.
- **Los tres reparos se declaran a mano.** El nivel de tanque es de `RN-83` y la espera con
  motor encendido de `M-19`; ninguno existe, así que quien concilia los marca.
- **Sustitución de vehículo a mitad de misión.** `RN-30` exige conciliar cada vehículo por
  separado con sus propios cortes de odómetro. `T-10` reasigna **sin registrar corte**, así que
  el corte no existe y no se puede partir. Queda dicho en vez de partir por un punto inventado.
- **El reporte de conciliación periódica** de `NRM-01` y la **alerta agregada** por vehículo,
  motorista o dependencia son de `M-14`. `RN-30` dice que el patrón se ve ahí, «no en una misión
  aislada».

---

### El consumo desde el cliente de campo — `V-04` sin red

**RESUELTA.** `campo/nucleo/ConsumoEnRuta.ts` prepara la carga en la estación, y
`POST /sincronizacion` la recibe. §10.1 dice que `V-04` **se ejecuta sin conectividad**, y eso
no es comodidad: la estación camino a La Mosquitia no tiene señal, y un consumo capturado de
memoria tres días después llega sin odómetro — el dato con el que `RN-30` sabe *dónde* se fue
la diferencia.

**La línea que traza el módulo de campo:** comprueba **sólo lo que la persona con el surtidor
delante puede corregir** —galones, estación, un odómetro que retrocede contra su última
lectura—. El saldo del fondo, `RN-32` y `BD-06` **no**: el dispositivo no tiene esos datos y no
los va a tener sin red. Fingir que los valida daría por conforme lo que nadie comprobó.

**`V-04` es de otro agregado**, y por eso viaja con `idAsignacion`. Una misión lleva varios
vales; mandar sólo el expediente obligaría al servidor a adivinar a cuál cargarle el galón.

---

## ⚠️ La idempotencia de la sincronización estaba rota, y `V-04` lo destapó

La comprobación de «esto ya llegó» usaba un `Contains` sobre `IdDeCaptura`, que lleva
convertidor de valor a `binary(16)`. Con `UseCompatibilityLevel(120)` esa traducción **devuelve
vacío en vez de fallar**: la consulta corría, no encontraba nada, y **cada reenvío pasaba por
nuevo**.

En las transiciones de misión no se notaba porque la **máquina de estados frenaba el duplicado**
—`T-14` sobre una misión ya en ruta es inválida— y el hecho terminaba en `rechazadas`. La prueba
que existía contaba transiciones y daba 1: **pasaba por el motivo equivocado**.

Y la diferencia importa: **un hecho rechazado nunca se acusa**, así que el dispositivo lo
reintentaría para siempre — justo lo que `RNF-03` existe para impedir. Con `V-04` se vio de
golpe, porque un vale admite varias cargas y ahí no hay máquina de estados que lo frene: el
duplicado llegaba hasta el índice único y devolvía un 500.

**Corregido con una búsqueda por punto** —una por hecho, sobre índice único— en los dos
diarios. Un lote de siete días son decenas de hechos; traer la tabla entera para filtrar en
memoria sí sería caro, porque el diario de vales crece con cada carga de la institución.

Las dos pruebas de reenvío verificadas por mutación. La de misión ahora exige `yaConocidas` y
`rechazadas` vacío, no sólo que el conteo dé 1.

**Y el 500 deja de ser mudo**: en desarrollo la respuesta lleva tipo y mensaje interno. Así es
como este defecto sobrevivió sin que nadie lo notara. En la institución no sale.

**Se auditó el resto del código:** los otros siete `Contains` corren **en memoria**, sobre listas
ya materializadas —conjuntos de estados terminales, días hábiles, feriados—, así que no pasan por
la traducción y no tienen este defecto. El de la sincronización era el único que iba a SQL sobre
una propiedad convertida.

---

**`RN-85` ahora se modela de verdad:** un consumo sin comprobante exige **causa declarada**, y
la causa viaja desde el dispositivo hasta el asiento. Sin ella el registro decía que faltaba el
papel pero no si eso se podía defender. `[C]` el catálogo de causas tipificadas no existe — hoy
es texto libre.

---

### `M-09` en pantalla — el fondo, el vale y sus bloqueos

**RESUELTA.** `/combustible` para el fondo del período, y un `PanelDeVales` montado en la
programación —donde se emite y se entrega— y en el cierre —donde se liquida y se concilia.

**El fondo NO es una tabla, y es deliberado.** No se comparan fondos: se mira *uno* y se actúa
sobre él. La pregunta al abrir es «¿cuánto queda del de Choluteca?», no «¿cuál de los ocho
tiene más». Una tabla optimiza la comparación y entierra el saldo entre columnas de igual peso.

**El panel va donde se sufre el bloqueo.** `T-19` y `T-21` se niegan por los vales; sin la
lista delante, quien cierra recibe un «no se puede» sin objeto al que ir.

Verificado contra la API real, no deducido:

| Qué | Qué contestó |
|---|---|
| Solicitar y aprobar un fondo | Saldo `L 45,000.00`, partida, «solicita X · aprueba Y» |
| Aprobar siendo quien lo solicitó | `RN-26.4` — *«quien pide y quien autoriza tienen que ser dos personas distintas»* |
| Emitir con receptor equivocado | `RN-32` — nombra al asignado y manda a `RN-14` |
| Emitir sin fondo vigente del ámbito | *«Sin fondo no se emite un solo vale: la salida es solicitarlo»* |
| Entregar sin despachar | `EF-04` — *«el vale existe emitido y no sale de la custodia»* |
| Emitir bien | Vale `EMITIDA`, aviso de `INV-34`, saldo `L 47,500.00` |

**Tres defectos propios, encontrados al mirar la pantalla:**

1. **El nombre del operador estaba cableado**, y con una sola identidad `RN-26.4` disparaba
   contra el propio operador: **ningún fondo podía pasar de `Solicitado`**. Cada acto ahora
   declara quién lo ejecuta. ⚠️ Es un **registro, no un control**: nada impide teclear otro
   nombre. Cuando exista `M-01`, el actor sale del usuario autenticado y ese campo se va.
2. **«No hay ningún fondo» se decía mientras todavía cargaba** — un negativo definitivo
   afirmado sin saberlo, que manda a solicitar un fondo que ya existe.
3. **Se ofrecía «Entregar» sobre una misión sin despachar**, y el servidor lo rechazaba. Las
   acciones se acotan por el estado de la misión, y lo que no cabe todavía **se dice** en vez
   de dejar un vale sin botones que parezca atascado.

De paso, dos correcciones de contrato:

- **`POST /combustible` ya no recibe `IdVehiculo`.** Servía sólo para rotular la respuesta, y
  pedirlo obliga al cliente a conocer la reserva — justo lo que `RN-32` manda que precargue el
  servidor. El próximo que leyera el contrato iba a creer que era contra ese valor que se valida.
- **El receptor viene precargado con el motorista de la orden**, que es lo que `RN-32` pide, y
  cambiarlo avisa antes de intentarlo. La precarga no cierra la puerta al caso que la regla
  existe para atrapar: si el que llega es otro, se cambia y el bloqueo dispara.

**⚠️ Lo que la pantalla NO hace:**

- **El folio se teclea.** Debería salir del rango de la delegación (`RN-44`) —lo que permite
  emitirlo sin conectividad—, pero ese rango vive en el cliente de campo y la oficina no lo
  consume. Es lo que `RN-27` prevé para la institución con folios preimpresos, así que el
  circuito funciona; lo que falta es el otro esquema.
- **El umbral de conciliación lo decide quien concilia**, y la pantalla lo dice con todas sus
  letras. No hay umbrales cargados ni rendimiento esperado.
- **No hay pantalla de consumo de campo.** `V-04` se registra desde la oficina, y su lugar es
  el dispositivo del motorista (`M-16`).

---

### `M-09` — el circuito del combustible, de punta a punta

**RESUELTA.** El fondo del período, el vale con folio y la máquina §10.1 completa, `V-01` a
`V-10`. Con esto **`T-15` y `T-16` dejan de estar bloqueadas** — eran las dos transiciones que
no existían porque no había combustible que devolver.

| Capa | Qué |
|---|---|
Dominio | `FondoDeCombustible` (`F-01`…`F-06`), `AsignacionDeCombustible` (`V-01`…`V-10`), `ReglasDelFondo`, `ReglasDeEmisionDeCombustible` |
Datos | Esquema `combustible`, cuatro tablas, folio único por índice y `IdDeCaptura` único filtrado |
Aplicación | `ServicioDeCombustible` — vale y asiento de bitácora en la misma transacción |
API | `/fondos` y `/combustible`, doce endpoints |

**El saldo no es una columna.** Es `aprobado − asignado + devoluciones constatadas`, y las tres
cifras salen de asientos. Una columna de saldo es un número que alguien pudo haber editado, y
toda la razón de ser de `RN-26` es que ese número se pueda auditar.

**La segregación del fondo vive en `RN-26` y no en `RN-01`.** Es la corrección del hallazgo
`HN1-15`: `RN-01` se aplica *«sobre una misma Orden de Misión»* y el fondo es un objeto de
**período**. Leída como está escrita, `RN-01` no lo alcanza — y la incompatibilidad más
sensible del circuito de dinero quedaba enunciada sin regla que la sostuviera.

**El consumo va como asiento y no como total**, porque el motorista carga varias veces, cada
una con su odómetro. Un campo `galones_consumidos` contesta *cuánto* y pierde *dónde*, que es
justo lo que `RN-30` necesita.

Verificado punta a punta contra la base: el recorrido completo del dinero con cinco actos y
cinco personas distintas, el saldo bajando al emitir y volviendo al devolver, el vale anulado
devolviendo todo su valor, y el consumo reenviado por el dispositivo **que no se cuenta dos
veces** — la unicidad la impone la base, no una comprobación que se olvida.

**Dos defectos encontrados al escribir, no al revisar:**

1. **`Rechazada` y `Anulada` están declaradas después de `Cerrada` en el enum de estados**, así
   que comparar por orden dejaba emitir un vale contra una misión anulada — un desembolso sin
   expediente al cual imputarlo. Las ramas van antes que el orden. Verificado por mutación.
2. **El servicio pasaba el motorista de la orden a los dos lados de `RN-32`**, así que la regla
   comparaba algo consigo mismo y el bloqueo no podía disparar nunca. En el dominio la regla
   siempre estuvo bien: sólo se ve cruzando el servicio, y por eso la prueba que lo atrapa es
   punta a punta. Verificado por mutación.

**Y un hueco de `INV-17` que `T-15` destapó.** `T-15` ocurre **antes** de `T-14`, así que no
había odómetro contra el cual probar que el vehículo nunca salió. `INV-17` exige el del acta de
entrega y `T-12` no lo capturaba; ahora sí, y es contra ése que comparan `T-15` y `T-16`.

**⚠️ Lo que M-09 todavía NO hace, y no se finge:**

- **La conciliación no decide.** `V-09` contra `V-10` lo resuelve quien llama, porque los
  umbrales de desviación por tipo de vehículo son `[C]` (insumos #1 y #19) y el rendimiento
  esperado no está cargado. Calcularlo contra un umbral inexistente devolvería siempre
  «conforme», y **una conciliación que siempre concilia es peor que ninguna**.
- **La cuota trimestral de compromiso (`RN-54`) no se verifica.** Necesita el espejo
  presupuestario de ARGOS, que no existe. El asiento de aprobación lo dice con todas sus
  letras, para que dentro de dos años nadie confunda «se verificó y pasó» con «no se verificó».
- **El tipo de combustible del vehículo no se comprueba.** La ficha de `M-03` no lo declara —
  no hay columna—, así que `RN-32` recibe nulo y **dice que no evaluó**, en vez de suponer que
  coincide. Un vale de diésel para un vehículo de gasolina hoy pasa.
- **`RN-83` abastecimientos, `RN-30` conciliación galonaje–kilometraje, `RN-84`/`RN-85`
  comprobantes y `RN-86` reintegro**: no están. `RN-83` es el que hace que el combustible del
  tanque institucional deje de ser invisible, y sin él `RN-30` señalaría un fraude donde hay un
  procedimiento no modelado.
- **No hay pantalla.** Todo el circuito se opera por API.

**⚠️ Hallazgo que `RN-26` hereda y sigue abierto:** el par *solicita fondo × aprueba fondo* no
existe en la tabla `I-01`…`I-17` de `actores-y-roles.md`, que es la autoridad en
incompatibilidades y también razona por misión. Se ejecuta igual, porque no ejecutarlo mientras
se decide dejaría el hueco abierto en el dinero.

---

### `PT-072` — el padrón de flota, y con él `M-03` deja de existir sólo en la API

**RESUELTA.** `/flota` en la oficina. La tabla contesta las dos preguntas que se hacen al abrir un
padrón —*con cuáles puedo contar* y *quién responde por cada uno*— y desde ahí se declara el
estado operativo.

**Es una de las 23 pantallas donde la tabla es la forma correcta**: compara elementos homogéneos
por atributos homogéneos. La disponibilidad en el tiempo no se duplica acá — vive en los
cronogramas de `PT-026` y `PT-038`.

**Los dos terminales van en su propio recuadro**, separados de «en taller» y «prestado», porque
de ellos no se vuelve. Y no son lo mismo: el descargo extingue un bien propio, el retiro devuelve
uno que nunca lo fue. Declarar descargado un vehículo en comodato es un asiento falso sobre un
bien ajeno.

Verificado en pantalla: dar de baja `INS-P-014` devuelve *«tiene 4 misión(es) sin cerrar»* y el
expediente queda intacto; cerrar el taller de `INS-M-007` lo devuelve a `Disponible` y la tabla se
refresca sola.

**Se sembraron las custodias de desarrollo.** No había ninguna: `BD-13` bloqueaba el despacho de
los cuatro vehículos y eso no era una decisión, era un hueco de la semilla. `INS-C-002` queda
deliberadamente sin custodio para que el bloqueo y su pastilla roja sigan siendo alcanzables.

**⚠️ Y esto no es la ficha del vehículo.** El expediente completo —documentación con
vencimientos, mantenimiento, incidentes, custodios históricos, alta de vehículos— necesita `M-04`,
`M-11` y `M-12`, y ninguno existe. Hoy la flota se da de alta por la semilla de desarrollo.

**⚠️ Hallazgo: un vehículo sin estado declarado no lo frena nada.** `BD-07` deja constancia de
que no pudo evaluarse y la programación sigue. §10.2 cuenta el «alta reciente sin habilitar» entre
las causas de `NO_DISPONIBLE`, así que **o el nulo debe bloquear, o §10.2 sobra en ese punto**. Se
dejó como está porque hay expedientes anteriores al estado operativo, y cambiarlo en silencio
convertiría una decisión de producto en un efecto secundario.

### `BD-05` — el odómetro, y con él `T-14` y `T-18` dejan de ser un `Registrar` pelado

*«El hallazgo típico del TSC en flota es el incremento de consumo de combustible sin relación
con el uso habitual, y el odómetro es **el único ancla** que tiene el sistema para
detectarlo.»* No había ninguno.

| Regla | Tratamiento |
|---|---|
| Salida < última lectura conocida del **vehículo** | **Bloqueo.** Error de digitación o retroceso |
| Retorno < salida, `T-18` **ordinario** | **Bloqueo.** Físicamente imposible |
| Retorno < salida, `T-18` **constatado** | **No bloquea.** Se marca y el vehículo se libera |
| Retorno = salida | **Exige justificación.** Es el patrón de la misión que nunca se hizo |
| Con acta de sustitución de odómetro | **No comparable.** Las dos lecturas van al asiento |

**La referencia cruza misiones.** No es la lectura de este expediente: es la última del
**vehículo**, venga de donde venga. Un odómetro que retrocede entre dos misiones distintas es
exactamente el fraude que el control existe para detectar. Y se toma la **más alta**, no la
más reciente: con marcas de tiempo que vienen de dispositivos que estuvieron días sin red,
«la última en el tiempo» puede llegar después de una lectura mayor.

**Bloquear en `T-18` es una excepción a `P-2`, y la puso la autoridad.** Una lectura de
retorno menor que la de salida no es un hecho consumado que registrar: es **un número mal
tecleado**, y hay alguien con el tablero delante que puede corregirlo. En el **constatado**
no bloquea —hallazgo `HB3-04`—: ahí el vehículo ya está en el predio y negarse a registrarlo
**lo deja secuestrado por un trámite** mientras la delegación se queda sin unidad.

**El odómetro va como DATO en la transición**, no dentro del texto del motivo — `BD-05` lo
vuelve a leer para comparar el retorno contra la salida, y sacarlo de una cadena sería el
mismo error que tenía la reserva de `T-08` antes de `RecursosTomados`.

**Se revalida en el servidor aunque se evalúe en el dispositivo.** `BD-05` corre «sin red»,
pero el dispositivo sólo conoce su propia lectura: la referencia que cruza misiones sólo la
tiene el servidor. El lote de sincronización lleva el odómetro, y un `T-14` sin él **se
rechaza** en vez de entrar sin ancla.

**⚠️ Tres reglas de `BD-05` no se evalúan, y ninguna es un bloqueo.** Kilómetros contra la
**distancia estimada** por un factor configurable —en las dos direcciones, porque `NRM-01`
vigila el exceso y el defecto— y el **salto imposible respecto al tiempo**. No hay distancia
estimada en el sistema (sale del mapa de ARGOS o de una tabla de rutas) ni umbral de
velocidad (`[C]`). Su ausencia no deja pasar nada que debiera detenerse; lo que deja es **sin
detectar el hallazgo `H-02`**.

**⚠️ El acta de sustitución se modela pero no se produce.** El circuito que la levanta es de
`M-11`, que no existe. Se modeló igual porque sin ella `BD-05` sería **un bloqueo sin salida**
— que es el hallazgo `HB3-02`, ya corregido una vez en este mismo documento.

Verificado por mutación: al anular la comparación de retroceso caen las dos caras del caso —
la que bloquea en el ordinario y la que **no** bloquea en el constatado.

### La ventana de la misión ya lleva hora

Era el campo que **dos necesidades independientes** pedían: `BD-04` no podía juzgar la *hora*
inhábil y `PT-038` no podía ordenar el día del despachador. Está.

**Anulable en el tipo, exigida en el endpoint.** El dominio tiene que poder representar los
expedientes creados antes del campo; lo que no puede es dejar entrar uno nuevo sin horas.
`POST /misiones` devuelve `400` sin ellas. **No se fabricó ningún valor por omisión**: un
`08:00` inventado se ve idéntico a uno declarado, y sobre él se juzgaría `BD-04` y se
ordenaría el tablero.

**Las dos o ninguna.** Media ventana con hora es peor que ninguna, porque parece completa.

| Qué cambió | Dónde |
|---|---|
| `VentanaDeMision` gana `HoraDeSalida` y `HoraDeRetorno` | `M03_Flota/FichaTecnica.cs` |
| `HorarioHabil` — la jornada de la institución | `M02_Parametros/CalendarioDeDiasHabiles.cs` |
| `BD-04` evalúa la hora, y **dice cuál mitad no pudo mirar** | `OrdenDeMision.ExigirPermisoSiCirculaEnDiaInhabil` |
| El tablero ordena la ráfaga **por hora** dentro del día | `ConsultaDelDiaDeDespacho` |
| `time(0)` en dos columnas nulables | migración `HoraDeSalidaYRetorno` |

**⚠️ La hora se evalúa sólo si están los DOS lados.** Que la misión declare sus horas **y**
que la institución declare su horario hábil. Falta cualquiera y no se juzga — y el asiento
del diario **dice cuál faltó**, porque un «BD-04 no aplica» a secas es indistinguible de uno
que verificó las dos mitades.

**Hoy falta el segundo**: `HorarioHabil` está en **nulo** en el calendario provisional. El
horario oficial es el insumo #1, `[C]`. Nulo **no es «todo el día es hábil»**: es «no se
sabe», y de lo segundo no se deduce que una salida a las cinco de la mañana no necesite
salvoconducto. El día que se cargue, la hora empieza a evaluarse **sin tocar una línea**.

**⚠️ Sólo se evalúan los dos extremos declarados**, no las noches intermedias. Una misión de
cuatro días está fuera de la jornada todas sus madrugadas, y evaluarlas haría que **toda**
misión de más de un día exigiera permiso — con lo cual la mitad del *día* de `BD-04`
quedaría sin sentido. Es además lo que el control real mira: el agente detiene al que sale a
las cinco, no al que durmió en Danlí. `[C]` **si la institución entiende otra cosa**: la
ficha dice *«cualquier parte de la ventana»* y no aclara el pernocte.

**`HorarioHabil` no cruza la medianoche.** Un turno de 22:00 a 06:00 no se puede expresar, y
no se finge que sí. Es decisión de producto, no detalle del tipo.

Verificado por mutación —al anular la comparación horaria caen tres pruebas— y en la
interfaz: dos salidas del mismo día ordenadas **05:30 antes de 14:15**, y las que no declaran
hora al final de su día en vez de tratarse como medianoche.

### La custodia vacante: tener tarjeta abierta no es tener custodio

`BD-13` miraba la tarjeta de responsabilidad y la encontraba **abierta** — nadie la cerró,
porque la persona ya no está para firmarla. Y despachaba. El vehículo salía a nombre de
alguien que ya no trabaja en la institución: **el mismo daño que `BD-13` existe para evitar,
por otro camino**. Cuando aparece el golpe o la multa, no hay a quién imputarla.

Es el caso límite que `RN-22` nombra —*«custodio que cesa en el cargo dejando el vehículo
asignado»*— y que `RN-101` explica: *«la institución pierde la deducción de responsabilidad
por un trámite que no se hizo»*. **Se destrabó hoy**: hacían falta el registro de custodias y
el espejo del organigrama, y los dos existen desde esta semana.

**Advierte y no bloquea, y eso está decidido afuera.** `RN-22` pone el bloqueo *«tras un
plazo configurable»*, y el plazo es `[C]`. Mientras no se decida, hay alerta. Inventarlo
dejaría vehículos varados contra un número que nadie acordó.

**⚠️ La distinción que decide si esto sirve o es ruido:** sólo se advierte cuando el espejo
**conoce** a la persona y ninguno de sus puestos está vigente. Si no sabe nada de ella —la
integración no corrió, o esa dependencia no se sincronizó— no se dice nada. **Hoy el espejo
está prácticamente vacío**: sin esa guarda, la advertencia saldría en cada despacho de la
institución y en una semana nadie la leería. Es la misma razón por la que la antigüedad del
espejo devuelve nulo en vez de cero — ausencia de dato no es dato de ausencia.

Verificado por mutación: al quitar la guarda, las dos pruebas de la distinción fallan.

### La jefatura ya puede decir que no

**Podía aprobar y nada más.** Una solicitud improcedente no se rechazaba, una incompleta no
se devolvía, y quien la pidió no podía retirarla: el único camino era hacia adelante. La
bandeja de `PT-013` ofrecía **media función de autoridad** — y el botón «Rechazar con
motivo» estaba ahí desde el principio, **sin `onClick`**: un control muerto que parecía
operable.

| Transición | Qué | Motivo |
|---|---|---|
| `T-06` rechazar | **Terminal.** De `RECHAZADA` no sale nada | Catálogo **y** texto libre |
| `T-04` devolver | Vuelve a `BORRADOR` y se reenvía por `T-02` | Libre |
| `T-07` desistir | Quien pidió retira. **Sin segregación** | Libre |
| `T-03` descartar | Un borrador que nunca se envió | Libre |

**Rechazar y devolver no son lo mismo, y confundirlas cuesta.** `T-06` dice «no» y no se
deshace —*«la negativa queda documentada y no se borra reabriendo el expediente»*—; `T-04`
dice «así no». Con una sola de las dos, o una solicitud arreglable muere, o una improcedente
da vueltas para siempre. Por eso son dos botones con pesos visuales distintos y no uno con
bandera.

**`T-07` no exige segregación, y es deliberado.** `BD-01` existe para que nadie autorice lo
que él mismo pidió; retirar lo propio es lo contrario. Exigir un tercero obligaría a molestar
a la jefatura para deshacer algo que no llegó a nada.

**El catálogo de motivos de rechazo NO es un `enum`.** `HU-014` lo declara configurable por
la institución y sus valores de ejemplo — insumo #1, `[C]`. El dominio **recibe** el catálogo
y sólo impone que el motivo esté en él; la pantalla lo **pide al servidor** en vez de
cablearlo, porque una lista duplicada en el cliente se separa de la que el servidor valida y
el rechazo fallaría al guardar, no al elegir.

Es la diferencia con `MotivoDeAnulacion`, que sí es cerrado: aquella tipificación **es** el
indicador de déficit de flota y un catálogo que crece deja de ser comparable entre períodos.

⚠️ **Se cargan cuatro valores de ejemplo y no una lista vacía.** Un catálogo vacío haría
**imposible rechazar**: entre una lista provisional marcada como tal y una función de
autoridad que no se puede ejercer, la primera es el error menor — y es reversible con una
carga de datos, no con código.

⚠️ **Cuatro efectos de la autoridad no ocurren, y ninguno se finge.** Liberar el número de
expediente sin reciclarlo (`M-01`); la acción *«crear nueva solicitud a partir de esta»* que
preserva el vínculo (`M-06`); el **versionado** del expediente al devolver —el diario guarda
el rastro de la devolución, pero no una versión 1 del contenido frente a una 2—; y la
precondición de `T-04` sobre autorizaciones de nivel ya registradas, que hoy es **vacua**
porque el escalamiento de `RN-02` no existe y sólo hay un `T-05`.

### 🔎 El dominio acepta una solicitud sin dependencia

`DatosDeLaSolicitud` recibe `Dependencia`, `ObjetoDelTraslado` y `Destino` como `string` y
**no rechaza la cadena vacía**. Hay un expediente sembrado con `dependencia: ""` que lo
prueba. Una solicitud sin dependencia **no se puede encaminar a ninguna jefatura** —no hay
competencia contra la cual verificar— ni aparece en ningún reporte por dependencia.

Se detectó porque el diálogo de rechazo decía literalmente **«vuelve a , se corrige»**. Lo que
se corrigió es la pantalla: nueve mensajes de la oficina interpolaban ese campo y ahora pasan
por `laDependencia()`, que sustituye el hueco por algo que **encaja gramaticalmente y dice la
verdad**. El hueco del dominio **no se tocó**: es otra decisión y es tuya.

### El despacho ya tiene sus dos controles que faltaban: `BD-13` y `BD-04`

**`BD-13` — sin custodio vigente no se despacha.** No es una formalidad: la operación que
`T-12` describe **no se puede ejecutar**. Sin custodio no hay de quién recibir el bien ni a
quién devolverlo, y el acta de entrega queda sin una de sus dos firmas. La custodia es una
**tabla con rango de fechas** —`RN-22` exige el historial completo consultable, y una
columna `custodio_actual` contesta el presente y borra el pasado, que es el que pregunta la
auditoría.

**`BD-04` — circular en día inhábil exige permiso de la máxima autoridad.** El permiso
ampara **vehículo, motorista, ruta y ventana**, la lectura más exigente de las tres que
convivían (`HB3-07`), y de ahí sale sin regla aparte que **un relevo de motorista lo
invalide**. La excepción de servicio exceptuado es **atributo del vehículo** (`RN-24`,
`HB3-08`): una ambulancia con excepción vigente sale un domingo, y **el uso de la excepción
queda registrado**.

**Los dos parámetros son obligatorios y no anulables.** «No hay custodio» y «nadie preguntó»
no pueden verse igual en un bloqueo duro. El compilador obligó a que cada llamador
contestara, y rompió cuatro pruebas de despacho — que era la intención.

| Pieza | Qué |
|---|---|
| [`CustodiaDelVehiculo`](src/Sigti.Dominio/M03_Flota/CustodiaDelVehiculo.cs) + `flota.Custodia` | La cadena de custodia permanente, con vigencia |
| [`CalendarioDeDiasHabiles`](src/Sigti.Dominio/M02_Parametros/CalendarioDeDiasHabiles.cs) | Qué es inhábil. Parámetro, nunca cableado |
| [`PermisoDeCirculacion`](src/Sigti.Dominio/M07_ProgramacionYDespacho/PermisoDeCirculacion.cs) + `mision.PermisoDeCirculacion` | El salvoconducto, con las cuatro cosas que ampara |
| [`ServicioExceptuado`](src/Sigti.Dominio/M03_Flota/ServicioExceptuado.cs) | La excepción, en columnas del vehículo |

**⚠️ El calendario provisional SUBDECLARA lo inhábil.** Declara lunes a viernes hábiles y
**la lista de feriados está vacía**: con esto, una misión que sale el 15 de septiembre pasa
`BD-04` sin permiso. No se cargan porque la autoridad lo prohíbe —*«nunca se cablean los
feriados… existe legislación posterior sobre los de octubre que no se pudo verificar»*,
insumo #14 `[C]`—. La versión se llama `PROVISIONAL-SIN-FERIADOS` y **aparece en el diario
de cada despacho**, para que el asiento se pueda auditar.

**⚠️ Tres cosas de `BD-04` no se evalúan ni se fingen.** La **hora** inhábil: la ventana de
la misión no lleva horas, así que no hay contra qué contrastar un horario hábil (insumo #1
`[C]`). El **salvoconducto impreso** que debe emitirse junto con la Orden de Misión: es de
`M-15`. Y la **ruta** se compara por el destino declarado, que es lo único que el expediente
lleva — dos misiones a Choluteca por caminos distintos se ven iguales; `[C]` con Auditoría
Interna.

**La custodia TEMPORAL tampoco existe** — el traslado al motorista al despachar, que se
extingue al retorno. Es un registro distinto de la permanente, y mezclarlos haría imposible
contestar quién respondía por el bien en ruta: la respuesta correcta son las dos personas.

**🔎 Hallazgo en la autoridad, corregido el 2026-08-28 con autorización del PO.** La sección
de `BD-13` citaba `EF-05` para el traslado de custodia, y `EF-05` es *«Conciliación disparada
al retornar»*. Se verificó que era **la única cita equivocada**: las otras cuatro del documento
y las quince de casos de uso y casos especiales apuntan todas a la conciliación.

La referencia pasa a la lista de efectos de `T-12`, que es donde el traslado **sí está
descrito**. **No se creó `EF-08`**: los identificadores no se insertan a conveniencia, y un
identificador nuevo no implementa nada. Queda como **decisión abierta del PO** si lo merece —
el argumento a favor es que el traslado hace convivir dos registros, el permanente y el
temporal de la misión, y el temporal no está construido.

Las dos anclas internas del documento se verificaron contra los 96 encabezados: **ninguna
rota**.

### `PROGRAMADA` tiene sus cuatro salidas

La máquina de estados dice que desde `PROGRAMADA` **se puede** *«reasignar vehículo o
motorista (`T-10`), desprogramar liberando recursos (`T-11`), despachar, anular»*. Las
cuatro existen.

**`T-10` no es `T-11` + `T-08`.** El rodeo pierde dos cosas: devuelve la misión a la cola
—donde otro puede tomarle el vehículo entre medio— y `EF-02` **anula el folio reservado**.
La ficha de `T-10` es explícita: *«el folio reservado no cambia: es el mismo expediente»*.

**Es el caso de borde de `BD-11`, y el único.** Acá la misión **está ocupando** mientras se
evalúa —a diferencia de `T-08`, que sale de `APROBADA`—: sin la exclusión de
`ReservasDeAsync`, chocaría contra su propia reserva y ningún cambio sería posible. La
exclusión estaba escrita anticipando esto; ahora se ejerce contra la base.

**`T-10` NO revisa la caducidad de la aprobación, y `T-08` sí.** No es una omisión: sería
al revés. La misión ya está programada y legítimamente a punto de salir, y si el vehículo
se avería la mañana de la salida, cambiarlo es la única maniobra que le queda a la
institución. Revisar caducidad ahí se la quitaría justo el día que la necesita.

**El motivo es tipificado y tiene catálogo propio** —`VehiculoATaller`,
`MotoristaNoDisponible`, `CambioDeRequerimiento`, `Consolidacion`—, distinto del de
anulación. Miden cosas distintas: el de anulación mide **déficit de flota**, éste mide
**fiabilidad de la flota y del padrón**. Un vehículo que entra a taller tres veces al mes no
es déficit —hay vehículos—, es un vehículo malo, y mezclarlos haría que el reporte de
déficit contara averías.

La reasignación **reusa la pantalla de asignación**: misma decisión, mismas reglas, una sola
implementación de la selección, el cronograma y la lectura del resultado. Se decide por el
estado del expediente, no por una ruta aparte. La cabecera dice **cuál tiene hoy** —el
diario ahora expone `vehiculoTomado` por transición— porque elegir a ciegas es cómo se
reasigna al mismo que ya estaba.

Las tres salidas van en la fila, **en orden de daño**: cambiar recurso, devolver a la cola,
anular. Con el mismo peso visual, la más destructiva se usa cuando bastaba la anterior.

### `BD-11` bloquea, y `T-11`/`T-13` dan la vuelta

**El sistema dejó de sobre-asignar.** `EF-01` es taxativo —*«no sobre-asigna, ni siquiera
con advertencia»*— y la regla estaba escrita sin implementar: se podían programar dos
misiones sobre el mismo pick-up los mismos días.

**La aritmética del solape vive en el dominio, no en el `WHERE`.** Puesta en SQL no se
puede ejercer sin base, y los casos de borde se prueban a través de tres capas o no se
prueban. Los extremos son **inclusivos de los dos lados**: una misión que retorna el jueves
y otra que sale el jueves chocan. Verificado mutando el operador a `<` — las dos pruebas de
borde fallan, que es lo que las hace valer algo.

El bloqueo **nombra al titular** —folio, dependencia y franja—, porque las cuatro salidas
de `EF-01` empiezan todas por saber a quién llamar. Y va también en la **vista previa**: las
salidas se deciden antes de apretar el botón.

**`T-11` desprogramar y `T-13` anular una programada.** Hasta hoy una misión programada no
se podía deshacer: un vehículo asignado por error quedaba tomado hasta que alguien lo
despachara. `T-11` la devuelve a `APROBADA` conservando su aprobación —no se obliga a la
jefatura a firmar otra vez por un problema de flota— y **es el paso obligado de la cuarta
salida de `EF-01`**: desplazar por prioridad pasa por devolver a la cola, nunca por quitar
el vehículo en silencio.

| Pieza | Qué |
|---|---|
| [`ReservaDeRecurso`](src/Sigti.Dominio/M07_ProgramacionYDespacho/ReservaDeRecurso.cs) | El solape, con los tres datos que `EF-01` exige mostrar |
| `ConsultaDeOcupacion.ReservasDeAsync` | Trae por **recurso**, sin filtrar por fecha: el solape es la regla |
| `OrdenDeMision.Desprogramar` / `AnularProgramada` | `T-11` y `T-13`. La primera es reversible vía `T-08`; la segunda no se vuelve |
| [`ConflictoDeAgenda.tsx`](oficina/src/modulos/M07_Programacion/ConflictoDeAgenda.tsx) | Nombra al titular y ofrece la única salida que existe |
| [`Cola.tsx`](oficina/src/modulos/M07_Programacion/Cola.tsx) | Tercer segmento «Programadas», con las dos salidas distinguidas |

**Nueve pruebas existentes empezaron a fallar, y tenían que fallar**: todas programaban
sobre el mismo motorista en la misma franja. Eran nueve dobles asignaciones que pasaban
porque la regla no existía. **El arreglo no fue debilitar la regla** — fue que cada prueba
tenga su par vehículo+motorista.

**⚠️ La ventana reservada es más angosta que la que `EF-01` prescribe.** Las holguras
institucionales —previa y posterior, por institución y tipo de vehículo— son el **insumo #1,
`[C]`**. Hoy se reserva `[salida, retorno + holgura declarada]`, el mismo rango que evalúa
`BD-02`. Mientras no se decidan, **el bloqueo deja pasar solapes que después bloqueará**. Es
la dirección segura del error: inventar valores bloquearía misiones legítimas contra
números que nadie decidió.

**Dos efectos de `T-11` no ocurren**, y no por descuido: anular el folio reservado (`EF-02`)
necesita los rangos por delegación de `M-01`; y devolver el vehículo a `DISPONIBLE` necesita
el estado operativo de `M-03`. Ninguno de los dos se finge.

**Dos cosas más salieron de mirar la pantalla.** La lista de salida decía *«vehículos
**libres** que la licencia habilita»* y el servidor sólo había filtrado por licencia — ahora
filtra por las dos, porque una salida que vuelve a bloquear no es salida. Y los diálogos
tenían **dos botones de salida**: `Modal` ya rinde el suyo, y encima el llamador agregaba
otro. El de anulación venía así desde antes.

### `BD-08`, `BD-09` y `BD-10` siguen sin evaluarse

`BD-02` y `BD-03` ya están implementadas y probadas — **`BD-04`, `BD-11`, `BD-12` y `BD-13` también, ver arriba.** **`BD-07` no** — estado y compatibilidad del vehículo. Necesita dos cosas que todavía no existen:

- La **matriz de compatibilidad** entre el objeto del traslado y el tipo de vehículo (`M-02`)
- La **categoría de peaje** resuelta por vehículo (`M-18`, `NRM-10`) — sin ella el estimado de peajes no es verificable, y quien autoriza no puede comprobar el cálculo

Tampoco están `BD-04` (día u hora inhábil), `BD-05` (coherencia del odómetro), `BD-06` (segregación operativa), `BD-08` a `BD-11`. Todas declaradas en [`estados/orden-de-mision.md`](docs/03-arquitectura/estados/orden-de-mision.md) §4.

### La matriz licencia↔vehículo **está completa y es normativa**

Las **nueve** categorías del **Artículo 4 del Acuerdo 1012-2021** `[V]`, con la fuente en [`fuentes/`](docs/01-negocio/normativa/fuentes/acuerdo-1012-2021-permisos-de-conducir.pdf). Versión `ACUERDO-1012-2021-ART-4`.

`A` y `B1` se expresan por **clase normativa** —`MOTOCICLETA`, `TRICICLO_CUADRICICLO`, `AUTOMOVIL`, `CAMION`, `AUTOBUS`—, que es conjunto cerrado de la norma y **no es el tipo de vehículo del catálogo institucional**. Donde el Acuerdo no fija techo de masa o pasajeros, la entrada tampoco lo fija: el límite real lo pone la ficha técnica.

**Lo que queda abierto es el camino, no el dato:** el circuito de carga existe (`POST /parametros` y `POST /parametros/{id}/aprobar`, con doble control y asiento en bitácora), pero la matriz **no entra por él** — está escrita en C# en `ParametrosProvisionales`. Cargarla por el circuito y borrar esa clase es lo que le da doble control.

### ⚠️ `M-05` está escrito y **la suite no pudo correr**

El padrón de motoristas sale de la base y `CatalogoProvisionalDeFlota` **se borró**. Compila —los cinco proyectos, incluido el de pruebas—, pero **Smart App Control bloqueó `dotnet test` y `dotnet ef`** durante veinte intentos con `0x800711C7`, y **la migración `PadronDeConductores` no se aplicó a la base**.

**Antes de confiar en esto:**

```bash
dotnet ef database update -p src/Sigti.Datos -s src/Sigti.Datos
dotnet test
```

Deben pasar **80**. Este cambio **borra un archivo del que dependían cuatro pruebas** y toca `BD-02`, que es la precondición que traslada responsabilidad legal: no se da por bueno hasta verlo.

### Un catálogo sigue en código, y está marcado como provisional

Eran tres. Salieron la flota y el padrón. El que queda no finge ser otra cosa: lleva su aviso en el propio archivo.

| Qué | Dónde | Qué falta para borrarlo |
|---|---|---|
| **Padrón de motoristas** | `CatalogoProvisionalDeFlota` — ya solo los conductores | `M-05` |
| **El folio** | `ConsultaDeMisiones.FolioProvisional` — sale como `PROV-xxxxxx` | El circuito de rangos por delegación. El **consumo** ya está en [`SubrangoDeFolios`](campo/nucleo/Folios.ts); falta **repartirlos**, que es de `M-01` |
| **El catálogo de restricciones médicas** | `CatalogoProvisionalDeRestricciones` | Insumo **#42** — la DNVT no tiene fuente pública |

### ⚠️ El tono secundario de las seis pantallas no se estaba aplicando

`text-[var(--txt-2)]` y otras siete clases apuntaban a **variables que no existen en
ninguna hoja**. Un `var()` sin definir no falla: la propiedad queda inválida y el color se
hereda. Medido en el navegador: antes el texto secundario computaba `rgb(217,224,234)`,
idéntico al del padre; ahora `rgb(159,173,192)`.

Se veía bien porque casi siempre iba en `text-xs`, y un cuerpo más chico se lee más claro.
Por eso duró seis pantallas sin que nadie lo notara: **el síntoma de un token roto es que
no hay síntoma.**

Las ocho eran vocabulario **inventado por las pantallas** — `ui/` estaba limpio. Es el
problema del que advierten `CLASE_TONO` y `TOKEN_TONO`, y la propia vitrina lo tiene
escrito: *«si necesitás un color que no está, falta un token: pedilo, no lo escribas a
mano»*. 60 clases en 9 archivos, sustituidas por las utilidades reales del contrato.

**Lo que queda de esto:** no hay nada que impida volver a escribirlo. Una comprobación que
falle cuando una clase arbitraria referencia una variable no declarada cerraría la puerta;
hoy la única defensa es acordarse.

### De las dos validaciones de `HU-009`, una ya se calcula

**La antigüedad del espejo del organigrama es real**: la bandeja la consulta a `/organigrama/antiguedad` y la muestra en la cabecera. Dejó de ser el texto fijo que no medía nada.

**No hay umbral cableado, y es a propósito.** `RN-50` marca `umbral_advertencia_desincronizacion` como **`[C]`, por confirmar con el PO y con Talento Humano**. La pantalla dice el número de días; **quién lo considera demasiado es de la regla**, no del componente. El único caso que sí se puede juzgar sin umbral —que **nunca** se haya confirmado— es el único que sube de tono.

Y en ningún caso impide autorizar: `HB1-10` corrigió justamente eso. Verificado en el navegador con los tres caminos —nueve días, nunca confirmado, y la consulta caída—: **en los tres el expediente sigue listado y autorizable**.

**Lo que sigue faltando** son los reparos **por expediente** —misiones sin liquidar del solicitante—, que necesitan `M-13`. Por eso el adaptador no devuelve una lista vacía: **una bandeja sin reparos y una que no sabe si los hay son cosas distintas**.

### Cinco preguntas de la designación que bloquean

1. **Edición exacta y Service Pack** de la instancia 2014, y si el **cifrado de respaldo** está disponible ahí. Bloquea `RNF-13`
2. **¿La licencia tiene Software Assurance vigente?** Si la tiene, la actualización ya está pagada y buena parte de la designación sobra
3. **Insumo #73, reformulado**: dónde vive la llave del cifrado por columna, quién la custodia, y **quién probó una restauración completa con ella en otra máquina**
4. **Aceptación por escrito** del riesgo de operar un motor fuera de soporte con datos personales de ciudadanos
5. **Licenciamiento para la segunda institución.** No bloquea el piloto; bloquea la promesa de `RNF-19`

### 83 insumos pendientes, de los cuales cuatro pesan

De 85 filas en las secciones abiertas del registro, dos ya están marcadas cerradas: el **#23** —los cuatro PDF, descargados— y el **#79** —`BE` existe, `DE` no—.

Registro completo en [`docs/07-gestion/insumos-pendientes.md`](docs/07-gestion/insumos-pendientes.md).

| # | Qué | A quién |
|---|---|---|
| **2** | **Formatos en papel vigentes** — 19 documentos. Bloquea 27 pantallas de captura | Institución. Lista lista en [`levantamiento/`](docs/07-gestion/levantamiento/) |
| **7** | **Periodicidad del fondo de combustible** — define si el objeto es de período o de misión. Es estructural | Gerencia Administrativa |
| **26** | Pronunciamiento de **Auditoría Interna** sobre segregación en delegaciones. **Es el que más tarda** | Auditoría Interna |
| **36** | ¿Hay cisterna o bidones de combustible? **Cambia el circuito completo de M-09** | Encargado de transporte |

**Ese lote ya se cerró:** los cuatro PDF oficiales están descargados en [`docs/01-negocio/normativa/fuentes/`](docs/01-negocio/normativa/fuentes/) desde el 2026-08-26, y la matriz licencia↔vehículo quedó `[V]`. **Queda sin explotar el Decreto 51-2025 y el clasificador de SEFIN**, que están bajados pero no leídos.

### Decisiones pendientes del PO

1. **Ratificar o revertir [`DP-002`](docs/07-gestion/decisiones-de-producto/DP-002-segregacion-en-delegaciones-pequenas.md)** — segregación en delegaciones pequeñas
2. **El nombre de la institución en los mockups** — dicen *Instituto Nacional de Migración*; según `DP-001` el sistema es genérico y eso es configuración
3. **La contradicción del salvoconducto** — la paridad con el papel exige reproducir el formato; el requisito de campo exige que cuatro datos vayan arriba y en grande. Si el formato no los pone ahí, hay que decidir cuál gana
4. **¿El reclamo de peaje cierra la misión o la marca con hallazgo?** — resuelto provisionalmente a favor de lo primero, y anotado `[C]` en la máquina de estados por si se revierte (`HB3-02`)
5. **¿Quién aprueba el descargo de un bien — `ACT-08` o también `ACT-09`?** El mapa de procesos admite los dos, la máquina de estados dice uno, y `NRM-02` está `[P]`: la norma no lo zanja, así que lo zanja el PO (`HB3-16`)
6. **Sesión de refinamiento** sobre las historias en borrador. Los cuatro analistas aplicaron el criterio de forma distinta; la mayoría pasaría sin tocarse

### Documentación exigida por LOKI — falta la de raíz

Auditado el 2026-08-25, cuando la causa de las cuatro brechas era la misma: no había stack. Ya lo hay, y `CLAUDE.md` ya refleja el stack en su sección *Estado actual*.

Siguen ausentes **`ARQUITECTURA.md`** y **`DESPLIEGUE.md`** en la raíz. Ahora sí se pueden escribir — y el criterio lo fija la propia designación: **índice consolidado que remite a `docs/`, nunca contenido duplicado que después diverge.** Es el patrón con que ya se resolvieron `DECISIONES.md` y `HANDOFF.md`, y LOKI lo reconoció como correcto.

`DESPLIEGUE.md` tiene una dependencia real: el procedimiento de respaldo y restauración es **de dos piezas** —base más almacén de archivos, consistentes entre sí ([`ADR-004`](docs/03-arquitectura/adr/ADR-004-adjuntos-fuera-de-la-base.md))— y `RNF-09` exige que lo ejecute personal no especialista en ≤ 2 h. Escribirlo sin la instancia real confirmada sería escribir la mitad.

### Los 113 hallazgos, cerrados

Los cinco informes de [`docs/05-calidad/hallazgos/`](docs/05-calidad/hallazgos/) conservaban el estado con que se emitieron —*«Abierto»*, *«Ninguna corrección aplicada»*, *«Pendiente de corrección»*—. Al verificarlos contra los artefactos vivos el 2026-08-26, **hallazgo por hallazgo y no contra el mensaje del commit**, apareció que el campo estaba mal **y también la suposición de que todo se había corregido**: veinte seguían abiertos. Se cerraron entre el 26 y el 27 de agosto.

| Informe | Corregidos | **Abiertos** |
|---|---|---|
| [`H-B1-001`](docs/05-calidad/hallazgos/H-B1-001-revision-qa-bloque-1.md) QA del Bloque 1 | **28** | — |
| [`H-B1-002`](docs/05-calidad/hallazgos/H-B1-002-revision-normativa-bloque-1.md) normativa del Bloque 1 | **20** | — |
| [`H-B3-001`](docs/05-calidad/hallazgos/H-B3-001-hallazgos-de-casos-de-uso.md) casos de uso | **19** | — |
| [`H-B34-001`](docs/05-calidad/hallazgos/H-B34-001-revision-qa-bloque-3.md) QA del Bloque 3 | 21 citados, verificados por muestreo | — |
| [`H-B34-002`](docs/05-calidad/hallazgos/H-B34-002-revision-arquitectura-bloque-4.md) arquitectura del Bloque 4 | 25 citados, verificados por muestreo | — |
Cada informe lleva ahora su desglose. **Los tres que se señalaron como prioritarios fueron los primeros en cerrarse:**
Cada informe lleva ahora su desglose. **Los tres que se señalaron como prioritarios están los tres cerrados:**

1. ✅ **`HB1-01` quedó cerrado el 2026-08-26.** Faltaba el Nivel 3 de [`actores-y-roles`](docs/01-negocio/actores-y-roles.md) §5.4, que seguía diciendo *«el sistema no lo bloquea»* ante una emergencia. Lo zanjó el principio **P-2** de la máquina de estados, que separa lo que ese nivel mezclaba: los bloqueos duros rigen `T-05`, `T-08` y `T-12` —también en emergencia— y **nunca impiden registrar el hecho**. La salida sin red es el **código de autorización fuera de línea** del `§6.6`, que la propia autoridad nombra como la respuesta a la segregación en delegaciones pequeñas. **Queda declarado un hueco `[C]`**: si a las 03:15 la única persona disponible es el propio motorista, `I-11` no se levanta y ese caso no tiene salida escrita — es decisión del PO, no de diseño.
2. ✅ **`HN1-18` y `HN1-09` quedaron cerrados el 2026-08-26.** `HN1-18` eran **ocho** materias, no una. Cuatro pasaron a regla —[`RN-99`](docs/01-negocio/reglas/RN-99-constatacion-fisica-de-la-flota.md) constatación física de la flota, [`RN-100`](docs/01-negocio/reglas/RN-100-permisos-por-puesto-no-por-persona.md) permisos por puesto, [`RN-101`](docs/01-negocio/reglas/RN-101-cierre-de-asignacion-de-puesto.md) cierre de asignación con custodias, [`RN-102`](docs/01-negocio/reglas/RN-102-reporte-publico-de-flota.md) reporte público—, una entró en [`RN-43`](docs/01-negocio/reglas/RN-43-captura-de-campo-sin-conectividad.md), y **tres no eran huecos**: multas y siniestros son M-12, mantenimiento es M-11, y el TAG lo bloquea el insumo #24. El [`README` de reglas](docs/01-negocio/reglas/README.md) los declara ahora, que era lo que el hallazgo pedía de raíz. `HN1-09` se cierra con [`RN-98`](docs/01-negocio/reglas/RN-98-paquete-de-evidencia-por-vehiculo-o-periodo.md): la evidencia se entrega también por vehículo y por período, no solo por misión.
3. ✅ **`HN1-14` quedó cerrado el 2026-08-26.** [`RN-52`](docs/01-negocio/reglas/RN-52-registro-de-consultas-a-manifiestos.md) declaraba `[V]` una exigencia del MARCI que `NRM-01` tiene `[C]` — **y su cabecera contradecía a su propio cuerpo**, que ya decía lo correcto. La verificación quedó separada en sus tres afirmaciones: `[V]` el hábeas data del Artículo 182, `[C]` la exigencia del MARCI, `[I]` que del hábeas data se siga registrar cada consulta. **Sigue siendo bloqueo duro y no configurable**, dicho expresamente en la regla. Se alinearon los otros cuatro artefactos que repetían la escalada.

**No queda ninguno abierto.** Los tres informes del Bloque 1 y de casos de uso están cerrados hallazgo por hallazgo; los dos de los Bloques 3 y 4 llevan sus 46 citados en el artefacto que corrigen, verificados por muestreo y **dicho así en su encabezado**, no como auditoría completa.

**Lo que el lote deja vivo no son hallazgos, sino trabajo que ellos destaparon:**

| Qué | Dónde queda |
|---|---|
| **Cinco pares de incompatibilidad sin regla** — `I-12`, `I-14`, `I-15`, `I-16`, `I-17` | Declarados en [`RN-01`](docs/01-negocio/reglas/RN-01-segregacion-de-funciones.md). `I-16` es postergación de `M-11`; los otros cuatro son huecos. **`I-12` es el que más pesa**: que un auditor con capacidad de ejecutar deja de ser auditor hoy solo lo dice un documento de actores |
| **Cuatro pendientes de `actores-y-roles` §9 sin número de insumo** — `G`, `I`, `J`, `K` | Dicho como tal en esa misma sección. `G` sostiene un bloqueo configurable de [`RN-25`](docs/01-negocio/reglas/RN-25-salvoconducto-con-folio-y-qr.md) |
| **Insumo #93 — quién aprueba el descargo** | Registrado el 2026-08-27. `NRM-02` está `[P]` y no lo zanja: **es decisión del PO** |
| **El plazo de depuración del dato personal de un borrador descartado** | `[C]` abierto en [`RN-04`](docs/01-negocio/reglas/RN-04-anulacion-como-asiento-reverso.md) tras `HB1-24` |

**No confundir con lo anterior.** Esto ya se resolvió y no debe volver a listarse como pendiente.

### Construido y verificado contra SQL Server

| Qué | Dónde |
|---|---|
| **El expediente de misión**, con el estado como proyección del diario (`P-1`). No hay columna de estado que se pueda desincronizar | `M07_ProgramacionYDespacho/OrdenDeMision.cs` |
| **`BD-01`** — segregación entre solicitante y autorizador, en el caso que el control no cubría antes de `HB3-01`: la asistente captura para su jefe | idem |
| **`BD-02`** — licencia habilitante y vigente **durante todo el rango**, holgura incluida, resuelta por clase normativa y atributos de la ficha | `M05_Motoristas/ReglasDeHabilitacion.cs` |
| **`BD-03`** — matrícula bloquea; la placa **no**; póliza y revisión configurables y apagadas | `M03_Flota/ReglasDeDocumentacion.cs` |
| **Caducidad de la aprobación** — si no se programa antes del inicio de la ventana, caduca | `OrdenDeMision.ExigirAprobacionVigente` |
| **`T-09`** — anulación con motivo **tipificado**; el comentario acompaña, no sustituye | `M07_ProgramacionYDespacho/MotivoDeAnulacion.cs` |
| **La bitácora encadenada**, serializada con `sp_getapplock` dentro de la transacción. **20 escritores concurrentes no bifurcan la cadena** | `Sigti.Datos/Bitacora/EscritorDeBitacora.cs` |
| **`M-02` bitemporal** — resolución a la fecha del hecho, bloqueo sin vigencia, doble control que registra también los intentos rechazados | `M02_Parametros/` |
| **`ReglasDeVigencia`** — los dos ejes en un solo lugar, para que ningún módulo implemente uno y suponga que el otro viene puesto | `Sigti.Dominio/Reglas/` |

### Seis pantallas de oficina, contra la API real

| Pantalla | Qué resuelve |
|---|---|
| **`PT-013`** Bandeja de autorización | Las validaciones se ven **en la lista**, no al abrir. Advertencia y bloqueo no comparten forma, no solo color |
| **`PT-014`** Expediente en decisión | En una sola pantalla, en el orden de la decisión. **No retira el botón de autorizar** con advertencias: `RN-50` lo prohíbe |
| **`PT-025`** Cola de programación | Caducidad visible antes de intentar, y depuración con motivo del catálogo |
| **`PT-026`/`PT-027`/`PT-028`** Asignación y rechazo | La evaluación corre **al elegir**. El rechazo nombra la categoría que se necesita y ofrece las salidas en la misma pantalla |
| **Cola de cierre** | Los liquidados sin cerrar, con el aviso de que lo que no cierre al corte pasa al **saldo de apertura** del ejercicio siguiente (`RN-97`) |
| **Cierre** | **No hay dos botones.** El criterio decide si el cierre lleva hallazgo; quien cierra confirma con su justificación. Y la salida que no es cerrar: devolver la liquidación |

### Decisiones que quedaron hechas estructura, no comprobación

- **La ventana salió de `AsignacionDeMision`.** El agregado usa `Solicitud.Ventana` y el compilador impide pasar otra: quien programa no puede acortarla para que una licencia alcance
- **La API recibe identificadores de catálogo**, no la ficha técnica. Declarar 2,800 kg de un camión de 12,000 ya no se puede expresar
- **El respaldo documental de `M-02` es `required`.** El escenario de `HU-145` que rechaza una carga sin respaldo dejó de necesitar comprobación: ese estado no se construye
- **La evaluación de `BD-02` vive en un solo lugar.** El cliente pide el resultado; no lo calcula

### Del Sprint 0

- **Los ocho ADR y el C4** que la designación pedía. `ADR-000` marcado *Reemplazada por `ADR-002`*, sin editar su texto
- **Los 46 hallazgos de los Bloques 3 y 4**, y los 48 del Bloque 1, y los 19 de casos de uso
- **Los cuatro PDF oficiales** del insumo #23, descargados y versionados en [`fuentes/`](docs/01-negocio/normativa/fuentes/)
- **El insumo #79** — `BE` existe, `DE` no. Son nueve categorías, verificado en el Artículo 4

## Cómo seguir

**Lo inmediato, y no depende de nadie externo:**

1. **`PT-031` — constancia probatoria de las verificaciones practicadas.** Toda la evidencia ya está en el diario; falta el documento que el auditor pide. Es lo que convierte lo construido en algo defendible
2. **Aplicar el script contra una instancia SQL Server 2014 real.** Sigue siendo el único control que atrapa una migración que el destino rechaza, y **no existe**. Lo revisado hasta hoy es inspección del script, no aplicación
3. **Cargar la matriz de licencias por el circuito de `M-02`** y borrar `ParametrosProvisionales`
4. **`M-03` y `M-04`** — ficha del vehículo y su documentación. Destraban `BD-03` de verdad y `BD-07` a medias

**Lo que falta medir, y no puede esperar al Sprint 6:** **`RNF-12` — ≤ 25 % de batería en 8 h con seguimiento activo, en gama baja.** Es el único número donde React Native es medible peor que Kotlin, y [`ADR-003`](docs/03-arquitectura/adr/ADR-003-cliente-de-campo-instalado.md) tiene la contingencia escrita. Esa contingencia sirve si el número llega ahora. **No hay ni una línea del cliente de campo todavía.**

**Lo que hay que gestionar en paralelo:** la sesión de levantamiento con la institución. El paquete está listo en [`docs/07-gestion/levantamiento/`](docs/07-gestion/levantamiento/) — guion de dos horas, los 19 formatos, 12 preguntas ordenadas por impacto, y los 28 casos especiales redactados para leerle a un motorista. Al mismo tiempo, confirmar en la instancia real la edición exacta de SQL Server, el Service Pack y el cifrado de respaldo.
