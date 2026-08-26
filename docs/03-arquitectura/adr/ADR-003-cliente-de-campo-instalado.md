# ADR-003 — El cliente de campo es una aplicación instalada, no una aplicación web

| Campo | Valor |
|---|---|
| **Estado** | Aceptada |
| **Fecha** | 2026-08-26 |
| **Decide** | Product Owner, sobre la [designación de LOKI del 2026-08-26](../../07-gestion/designaciones/2026-08-26-stack-y-arranque.md) |
| **Sprint** | 0 |

## Contexto

El motorista trabaja donde no hay señal. `RNF-03` no dice *«con soporte offline»*: dice **7 días continuos sin conectividad y 0 registros perdidos al sincronizar**. Más de 2 millones de personas del área rural hondureña no tienen acceso a internet (INE, EPHPM julio 2025), y las delegaciones están precisamente ahí.

La opción barata es una aplicación web instalable — una PWA. Un solo cliente, sin tienda de aplicaciones, sin ciclo de publicación. El argumento de costo es real y merece respuesta, no desprecio.

La respuesta es que **la plataforma web no puede cumplir tres requisitos**, y ninguno de los tres es negociable.

**Los equipos de campo son solo Android**, por decisión del Product Owner.

## Requisitos que la condicionan

- [`RNF-03`](../../02-requisitos/no-funcionales/RNF-03-operacion-sin-conectividad.md) — 7 días sin red, 0 registros perdidos, ≥ 200 fotografías por dispositivo
- [`RNF-08`](../../02-requisitos/no-funcionales/RNF-08-seguimiento-en-ruta.md) — ubicación y estado del vehículo con la aplicación en segundo plano
- [`RNF-12`](../../02-requisitos/no-funcionales/RNF-12-uso-en-campo.md) — ≤ 25 % de batería en 8 h con seguimiento activo, en gama baja
- [`RNF-15`](../../02-requisitos/no-funcionales/RNF-15-continuidad-ante-rotacion-de-personal.md) — continuidad ante rotación de personal

## Decisión

**El cliente de campo es una aplicación Android instalada, construida con React Native**, con SQLite cifrado como **fuente de verdad local** — no como caché.

Se escribe React Native directamente. **No se escribe «aplicación nativa Android» para superarlo después**: la decisión de plataforma y la de tecnología se toman juntas porque los argumentos son los mismos.

### Por qué la web no sirve — los tres puntos

1. **`RNF-08` — no hay geolocalización en segundo plano en la web.** Con la pantalla bloqueada, la captura se suspende. El seguimiento en ruta deja de existir justo cuando el motorista guarda el teléfono y maneja, que es todo el trayecto.
2. **`RNF-12` — un runtime web consume más que uno nativo** para el mismo ciclo de trabajo, y el umbral de batería es el requisito más ajustado del sistema.
3. **`RNF-03` — la cuota de almacenamiento del navegador es desalojable y la aplicación no la controla.** El sistema operativo puede reclamar ese espacio. Con 7 días de bitácora y 200 fotos sin sincronizar, un desalojo es pérdida de datos que ningún reintento recupera.

### Por qué React Native cumple los tres

React Native **no es web**. Compila a una aplicación Android: corre servicios en segundo plano mediante módulo nativo, escribe en el sistema de archivos del dispositivo y no tiene cuota desalojable. Los tres puntos se cumplen.

Y trae un argumento que es de `RNF`, no de comodidad: **un solo lenguaje entre oficina y campo**. `RNF-15` es continuidad ante rotación de personal, y con TypeScript en los dos lados las reglas puras y los esquemas de validación **se comparten de verdad, no se transcriben**. Una regla transcrita es una regla que va a divergir, y la divergencia entre lo que valida la oficina y lo que valida el campo es invisible hasta que produce un rechazo que nadie sabe explicar.

## Alternativas consideradas

| Alternativa | A favor | En contra | Por qué se descartó |
|---|---|---|---|
| **PWA / aplicación web instalable** | Un solo cliente, sin ciclo de publicación, sin tienda | Sin geolocalización en segundo plano (`RNF-08`); cuota de almacenamiento desalojable (`RNF-03`); mayor consumo de batería (`RNF-12`) | Falla tres requisitos, y el de la cuota desalojable es pérdida de datos, no degradación |
| **Kotlin nativo** | El mejor consumo de batería medible; acceso directo a todo el sistema operativo | Segundo lenguaje y segundo equipo; las reglas de dominio se transcriben del TypeScript de la oficina y divergen | `RNF-15` pesa más que el margen de batería. Y si el margen no alcanza, la contingencia de abajo recupera lo que hace falta sin pagar el costo completo |
| **Aplicación híbrida en WebView** (Capacitor, Cordova) | Reutiliza el código de la oficina casi entero | Hereda el runtime web: los mismos problemas de batería y de cuota, con una capa más | Resuelve la instalación pero no ninguno de los tres puntos |

## Consecuencias

**Positivas**

- Los tres requisitos que la web no podía cumplir quedan cumplidos
- Las reglas puras de dominio son **el mismo paquete TypeScript** en oficina y en campo
- La actualización de la aplicación puede distribuirse por APK firmado sin depender de una tienda, que es lo que conviene en una institución pública

**Negativas**

- **Dos aplicaciones cliente**, con dos ciclos de publicación y dos superficies de prueba. Comparten API y reglas de dominio, **no** código de interfaz — y esa frontera hay que sostenerla, porque la tentación de compartir componentes visuales va a aparecer
- Distribuir actualizaciones a dispositivos en 40 delegaciones es un problema operativo que no existía con la web
- **`RNF-12` es el único número donde React Native es medible peor que Kotlin.** Y es un umbral, no una aspiración

**Deuda aceptada**

**El riesgo de batería se acepta con plan de contingencia escrito, no con optimismo.**

`RNF-12` exige ≤ 25 % de batería en 8 h con seguimiento activo, en gama baja. Se mide **en el walking skeleton del Sprint 2**, no al final. Si no pasa:

> **Se baja el seguimiento en ruta a un módulo nativo propio. No se reescribe la aplicación.**

El seguimiento es la parte que consume, y es aislable. Reescribir todo el cliente porque un componente no rinde sería cambiar la decisión completa por su pieza más chica.

## Revisión

- **La medición de `RNF-12` falla en el walking skeleton** → se activa la contingencia del módulo nativo. Si con eso tampoco pasa, este ADR se reemplaza por uno que adopte Kotlin
- **El Product Owner incorpora equipos iOS** al alcance. El argumento de este ADR no cambia, pero sí el costo de distribución y las pruebas
- **React Native deja de soportar servicios en segundo plano** de forma estable en las versiones de Android que la institución usa

> El argumento del borrado de almacenamiento a los 7 días por ITP de iOS **queda deliberadamente fuera**. Los equipos son solo Android, y traerlo debilitaría el ADR con una objeción fácil de responder.
