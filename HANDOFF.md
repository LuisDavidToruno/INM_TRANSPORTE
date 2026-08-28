# Estado del trabajo

**Última actualización: 2026-08-27.**

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

**Lo que falta para que esto sirva de verdad:** los criterios `H-01` a `H-13` **no se detectan todavía**. `M-09`, `M-13` y `M-18` no existen, así que no hay conciliación de combustible, ni de peajes, ni cadena que evaluar — y **todo expediente cierra limpio**. La función que los calcula está marcada como provisional y devuelve lista vacía, en lugar de fingir una evaluación que no ocurrió.

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
