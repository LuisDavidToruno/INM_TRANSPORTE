# Brief de diseño — paquete de entrega

Para quien va a hacer los mockups. **No tienes que leer los 216 documentos del repositorio.** Este archivo te dice qué leer, en qué orden, y qué puedes empezar hoy.

## Qué es SIGTI en una frase

> Así como Talento Humano cuida de todo lo referente a los empleados, **SIGTI cuida de todo lo referente a los vehículos** de una institución pública hondureña — motos, buses, pickups, camiones.

No gestiona "viajes de personas": gestiona **movilizaciones de recursos institucionales**. Lo que se traslada puede ser personal, personas externas, carga, o una combinación.

## Lee esto, en este orden — unas dos horas

| # | Documento | Por qué |
|---|---|---|
| 1 | [`docs/04-diseno/README.md`](README.md) | **Empieza acá.** El principio rector y el contexto de uso real. Es lo que decide si un mockup sirve o no |
| 2 | [`docs/00-vision/glosario.md`](../00-vision/glosario.md) | Los nombres de las cosas. Un botón que dice "conductor" donde el formato dice "motorista" ya está mal |
| 3 | [`inventario-de-pantallas.md`](inventario-de-pantallas.md) | Las **138 pantallas**, con qué puedes diseñar hoy y qué está bloqueado |
| 4 | [`mapa-de-navegacion.md`](mapa-de-navegacion.md) | Cómo se recorren, por rol |
| 5 | [`docs/01-negocio/actores-y-roles.md`](../01-negocio/actores-y-roles.md) §1 | Quién es cada quien. Solo la sección de actores |

Lo demás lo consultas cuando lo necesites. **No lo leas de corrido.**

## Los tres clientes

No es un producto con vistas distintas. Son **tres productos** que comparten dominio.

| Cliente | Quién | Condición real de uso |
|---|---|---|
| **Administrativo** | Despacho, jefaturas, transporte, combustible, auditoría | Escritorio, conectado, mucha información a la vez, presión de tiempo |
| **De campo** | Motorista, encargado de delegación | Celular, **sin conectividad**, a pleno sol, a veces con guantes, con el vehículo detenido en carretera |
| **Público** | Verificador en carretera | Una sola pantalla, sin sesión, sin menú. El agente que detiene el vehículo y escanea el QR |

El tercero es una pantalla, pero **tratarlo como producto aparte es deliberado**: si se diseña como "una vista más" del sistema, alguien termina exponiendo el expediente completo detrás de un código QR.

## El principio que no se negocia

> El operador que lleva años llenando un formato preimpreso debe encontrar **los mismos campos, con los mismos nombres, en el mismo orden** en la pantalla.

Esto reduce el costo de adopción más que cualquier funcionalidad nueva. **Si propones "mejorar" el orden de los campos, la respuesta por defecto es no** — y quien lo proponga tiene que justificar por qué el costo de reaprendizaje vale la pena.

Consecuencia directa: **el inventario está partido en dos**, y esa es la información más útil que te damos.

## Qué puedes empezar hoy

**99 pantallas no replican ningún papel.** Se diseñan libremente y no dependen de nadie.

> **Ojo con dos cosas.** Nueve pantallas son **duales**: se usan en el cliente administrativo y en el de campo, y **hay que diseñarlas dos veces**. No existe "vista móvil" — son dos productos. Por eso las superficies de campo a diseñar son **34, no 25**.
>
> Y **seis pantallas están marcadas ⛔ en el inventario: no se envían a diseño todavía.** Son las del ciclo de vida del parámetro normativo y algunas de personas externas. No tienen criterio de aceptación escrito, y dibujarlas sin criterio **fija la regla por accidente**.

Empieza por estas cinco, en este orden:

### 1. Cola de conflictos de sincronización

La más difícil del sistema, y la que nadie diseña hasta que ya duele. Por eso va primera.

Dos dispositivos sin conexión entre sí registraron algo distinto sobre la misma misión. Hay que mostrar **ambas versiones lado a lado, en lenguaje del negocio, no de datos** — el usuario tiene que poder resolver **sin entender de sincronización**. Historia: `HU-068`.

### 2. Bandeja de autorización

La jefatura entra dos veces al día, a menudo desde el celular, y decide en dos toques. Necesita ver **las validaciones ya evaluadas sin abrir cada solicitud**. Historia: `HU-009`.

### 3. Registro en ruta del motorista

A pleno sol, con guantes, sin señal, con el vehículo detenido porque algo pasó. **Botones grandes, poco texto, la acción más frecuente a un toque.** Cero pantallas que se queden esperando respuesta del servidor. Historias: `HU-046` a `HU-057`.

### 4. Conciliación galonaje ↔ kilometraje

Tiene que mostrar la desviación **en ambas direcciones** — y explicar por qué un rendimiento *demasiado bueno* también es un hallazgo. (Significa que hubo un despacho que nadie anotó.) Historia: `HU-088`.

### 5. Rechazo por licencia no habilitante

El mensaje debe decir **qué categoría se necesita**, porque el usuario tendrá que resolverlo con una gestión administrativa — no reintentando. Historia: `HU-025`.

## Qué NO puedes empezar todavía

**29 pantallas replican un formato en papel** y están bloqueadas hasta que la institución entregue los formatos vigentes (insumo #2). Otras **10** lo están parcialmente — en esas, una sección replica papel y el resto no, así que **se puede diseñar la parte libre**: el inventario dice cuál.

Son las de captura: solicitud, bitácora, vale de combustible, liquidación, actas. **Es la mitad del sistema.**

La lista concreta de los 19 formatos que hay que pedir, con las pantallas que desbloquea cada uno, está en la §5.2 del [inventario](inventario-de-pantallas.md).

## Cinco cosas del dominio que cambian el diseño

1. **Todo funciona sin red en el cliente de campo.** No es un modo degradado: es el estado normal. Siete días sin conectividad, con captura completa. Nunca muestres un error técnico de red — muestra "pendiente de enviar".
2. **Todo documento oficial se imprime.** Folio, código QR de verificación, espacio para firma y sello. Legible en impresora matricial o láser común, tamaño carta, **útil en blanco y negro**.
3. **Los mensajes de error son parte del requisito, no decoración.** En este dominio el usuario suele tener que resolver el bloqueo con una gestión administrativa. "Operación no permitida" no sirve; "la licencia categoría C1 habilita hasta 7,500 kg y el vehículo requiere categoría C" sí.
4. **Nada se borra.** No diseñes botones de eliminar. Las correcciones son asientos reversos con motivo y autor.
5. **Hay bloqueos que no se pueden saltar.** No diseñes un "continuar de todos modos" para ellos. Son 26 reglas de bloqueo duro, y cinco no se desactivan ni por configuración.

## Dónde buscar el detalle de una pantalla

Cada fila del inventario enlaza sus **historias de usuario**. Cada historia trae los **criterios de aceptación en Gherkin español**, con los mensajes exactos y los caminos de rechazo.

Si necesitas saber qué campos existen y qué los restringe: [`docs/03-arquitectura/modelo-datos/`](../03-arquitectura/modelo-datos/).

Si una pantalla te parece rara, busca su **caso especial** `CE-xx`: son 28, describen la operación real, y casi siempre explican por qué la pantalla es así.

## Lo que no está decidido

No hay stack tecnológico elegido — se difiere al Sprint 2 por [`ADR-000`](../03-arquitectura/adr/ADR-000-diferir-seleccion-de-stack.md). **No propongas biblioteca de componentes ni framework de interfaz.** Los mockups son de baja fidelidad y agnósticos.
