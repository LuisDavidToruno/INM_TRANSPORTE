# LOKI · Sistema de diseño

**Así se ven y así se construyen los frontends nuevos.**

No es una librería que se instala ni una guía de estilo que se lee: es un proyecto
que corre. Se abre, se mira, se copia.

```bash
npm install
npm run dev      # http://localhost:5180
```

---

## Qué es esto

Tres cosas en un solo proyecto:

| | Qué contiene | Para qué sirve |
|---|---|---|
| **Sistema de diseño** | 74 tokens, 6 temas, 2 densidades, ~30 componentes | Que la decisión de cómo se ve un botón se tome **una vez** |
| **Vitrina** | La galería, en `/sistema-diseno` | Que se pueda ver sin leer código, y que no mienta |
| **Cuatro pantallas** | Tablero, bandeja, formulario, acceso | Que se vea la **composición**, que la galería no enseña |

La piel es la de **ARGOS** (entrega 0.3.2), que ya está auditada:
21 pares de contraste × 6 temas, 126 combinaciones, cero fallos. Esa auditoría
**corre en el navegador** al abrir la vitrina — no es un número copiado de un PDF.

---

## Por qué las cuatro pantallas, y no sólo la galería

Una galería muestra piezas sueltas y todas parecen igual de importantes. Lo que
no puede mostrar es lo que más se hace mal:

- **Dónde va cada cosa en una página** (`/tablero`).
- **Los tres estados que nadie diseña** — cargando, sin datos, y la carga fallida
  (`/bandeja`, con un conmutador para verlos de un clic).
- **Qué dice un mensaje de error** (`/formulario`).
- **Qué NO dice un formulario de acceso** (`/acceso`).

Ese último es el ejemplo más corto de por qué esto existe: el mensaje de error
del login es el mismo exista el usuario o no. Decir «ese usuario no existe» le
confirma a quien prueba nombres cuáles son reales. Es una decisión de diseño con
consecuencia de seguridad, y no se toma dos veces si está resuelta acá.

---

## De dónde viene y dónde vive

Este sistema de diseño nació dentro del repositorio de ARGOS. **Ya no vive
ahí.** Su copia canónica es esta — `LOKI/diseno/` — y ARGOS pasa a ser un
consumidor más: el primero, y el que aportó la piel, pero no el dueño.

Esa es la razón de que el esqueleto se llame `loki-*` y la marca siga
llamándose `argos`. Son dos capas distintas y ahora tienen dueños distintos:
la estructura es de LOKI, la piel es de cada sistema que la use.

No importa nada de afuera de su propia carpeta: tiene su `package.json`, su
`node_modules` y su lockfile.

Arrancar un proyecto nuevo desde acá es copiar la carpeta:

```bash
cp -r diseno ~/donde-sea/mi-proyecto
cd ~/donde-sea/mi-proyecto
rm -rf node_modules dist
git init && npm ci        # `ci`, no `install`: respeta el lockfile
npm run verificar && npm run build
```

**Está comprobado, no supuesto.** Se extrajo a un directorio limpio fuera del
repositorio y se compiló desde cero: el CSS y el JS salieron con **el mismo hash**
que dentro del repositorio, o sea byte por byte idénticos. La extracción no
cambia nada.

Las versiones están **fijadas exactas** (sin `^`) justamente para eso: una copia
sacada dentro de seis meses instala lo mismo que hoy. El precio es que las
actualizaciones no llegan solas — se suben a mano, que en una plantilla es lo que
se quiere.

> El repositorio de ARGOS conserva una entrada suya en `.claude/launch.json`
> que aquí no hace falta: `npm run dev` funciona solo.
>
> Y dos comentarios citan la entrega de diseño 0.3.2 como procedencia
> (`src/marca/argos/tokens.css`, `src/ui/tipos.ts`). Dicen explícitamente que ese
> documento vive en el repositorio de ARGOS y no acá — son una referencia, no un
> archivo que falte.

### Para convertirlo en un proyecto propio

**Tres cosas y ninguna más**:

1. **Re-marcá.** Copiá `src/marca/argos/` a `src/marca/mi-marca/`, cambiá los
   valores de los tokens, y cambiá la única línea que lo importa en
   `src/estilos/index.css`. Ningún componente sabe qué marca está puesta — por
   eso es una línea y no una auditoría. Lo que hay que definir está en
   [`src/marca/CONTRATO.md`](src/marca/CONTRATO.md).
2. **Borrá `src/ejemplos/` y `src/vitrina/`** cuando ya no los necesites, y
   sacá sus rutas de `src/app/App.tsx`. (Conviene tenerlos un rato: son la
   referencia más rápida.)
3. **Reemplazá el vocabulario del dominio** en `src/ui/tipos.ts` — `ESTADO` y
   `ETAPA` son de un flujo de viáticos. Los tokens `--op-*` de la marca van en
   el mismo viaje.

---

## Versión

Este contrato va por **0.3.3**. Qué trae cada versión está en
[`CAMBIOS.md`](CAMBIOS.md).

Ese número es **nuestro** y no el de la entrega de ARGOS, que se quedó en 0.3.2
y no se renumera nunca: describe lo que ARGOS entregó, no lo que hicimos después
sobre eso. Hasta el 0.3.3 coincidían, y esa coincidencia las hacía parecer la
misma cosa. Los comentarios `Canon: handoff-argos-0.3.2/…` señalan procedencia
y se quedan como están.

## Las cinco reglas que no se negocian

Están explicadas con su ejemplo en la vitrina. En corto:

**1 · El estado se decide por identificador, nunca por el texto.**
«Por aprobar» contiene «aprob». Una comparación de cadenas la pinta de aprobada
— justo lo contrario de lo que dice.

**2 · El color nunca viaja solo.**
Cada pastilla lleva texto además del punto. Quien no distingue rojo de verde
tiene que poder usar el sistema igual.

**3 · Los importes llevan tres clases: `font-mono`, `tabular-nums`, `text-right`.**
Sin las tres los decimales no alinean, y la columna deja de poder leerse de un
vistazo — que es para lo único que existe una columna de importes.

**4 · El error dice la causa, no «campo requerido».**
Un mensaje se lee una vez; una devolución cuesta dos días.

**5 · La acción sin permiso se muestra deshabilitada con el motivo, nunca oculta.**
Ocultarla obliga a adivinar si falta un permiso o si el sistema está roto, y las
dos hipótesis terminan en interrumpir a alguien.

Y una de composición, la que devuelve el aspecto de plantilla si se ignora:
**el `Panel` es para un objeto discreto** —un registro, un cálculo, un gráfico—
**nunca para dividir una página en zonas.**

Y una de estructura, que no se ve y por eso se olvida: **`Panel` y `Vacio`
emiten `h3`; el que cuelga directo del `h1` de la pantalla necesita
`nivel={2}`.** Un documento que salta de h1 a h3 deja a quien recorre por
encabezados —la forma habitual de orientarse con lector de pantalla— sin saber
si se perdió un nivel o si nunca existió. El nivel cambia la etiqueta y nunca el
tamaño, así que ponerlo bien no cuesta una línea de CSS. Las pantallas de
`src/ejemplos/` ya lo hacen; copiá de ahí.

En la misma familia: **una `Tabla` que puede no caber a lo ancho lleva
`rotulo`.** Cuando el contenido desborda, el contenedor pasa a región enfocable
—si no, las columnas ocultas no existen para quien navega por teclado— y una
región sin nombre anuncia «región» y nada más.

Y: **el tramo de la barra de ubicación que nombra una pantalla lleva su
`href`.** Una miga que se ve como miga y no navega es la única pieza de la
interfaz que miente sobre lo que hace: quien hace clic y obtiene una selección
de texto no concluye que sea decorativa, concluye que la aplicación no responde.
El tramo que es sólo rótulo —un agrupador que no es una pantalla— se pasa como
cadena suelta y se dibuja sin subrayado. Las dos formas están en `MIGAS` de
`src/app/App.tsx`.

---

## Cómo está armado

```
src/
├─ marca/argos/      ← LA CAPA REEMPLAZABLE. tokens.css + 4 woff2 + logo
│  └─ CONTRATO.md       qué debe definir una marca nueva
├─ estilos/          ← entrada de Tailwind + el reset + las clases de componente
├─ tema/             ← el ÚNICO lugar que escribe el tema
├─ ui/               ← los componentes. `index.ts` es la API pública
├─ vitrina/          ← la galería (borrable)
├─ ejemplos/         ← las 4 pantallas (borrables)
└─ app/App.tsx       ← el shell: menú, rutas, paleta de comandos
```

**Lo que no está en `src/ui/index.ts` es privado**, aunque el archivo lo exporte.

### Dos decisiones heredadas que conviene entender antes de cambiar

**El prefijo `tw:` en todas las utilidades.** En ARGOS existe porque Tailwind
convive con Bootstrap. Acá no hay Bootstrap y se conserva igual: es lo que
permite mover un componente entre ARGOS y un proyecto nuevo **copiándolo**, sin
reescribir cada clase. Si tu proyecto no va a convivir con nada, se quita en una
corrida de find-replace.

**Sin preflight, con un reset explícito.** Por eso el reset de
`src/estilos/index.css` está a la vista y acotado a `[data-loki-shell]`. En una
plantilla eso es una ventaja: se sabe exactamente qué se resetea. Y deja el
bundle montable dentro de una página ajena sin desarmarla.

---

## Verificación

```bash
npm run verificar     # tipos + tokens
```

**`verificar-tokens`** comprueba que ninguna `var(--x)` quede sin resolver. Es un
test y no una revisión porque **un token inexistente no rompe nada**: el
navegador descarta la declaración y el elemento hereda el color del padre. En
tema claro eso suele ser invisible, y el defecto viaja hasta que alguien abre el
tema `navy`.

No es teórico: al portar los componentes apareció una clase (`text-exito-fg`)
cuyo token no existe en ninguna capa — sigue así en ARGOS, donde ese tilde de
confirmación no se pinta de verde y nadie lo había notado.

Lo que la verificación **no** cubre: que los valores sean los correctos. Un
`--surface-panel` en rosa pasa sin chistar. Eso lo cubren el contraste, que se
mide en la vitrina, y el ojo, que mira.

---

## Gráficos: ECharts, y sólo si hace falta

Las gráficas son **ECharts**, no barras de CSS. `KpiDato` tiene dos ranuras —
`serie` (barras, respaldo del paquete de diseño) y `grafico` (el componente
real), y la segunda gana. Las pantallas usan la segunda.

**La librería se carga con `import()` dinámico**, así que vive en su propio trozo
de 568 KB que **sólo baja cuando una gráfica se monta**. Un proyecto que salga de
LOKI y no dibuje nada no paga un byte. Medido: agregar ECharts subió el trozo
principal 2,7 KB.

Lo que hay que saber al escribir una gráfica está en la vitrina, sección
**Gráficos**. Lo más importante: **un canvas no entiende `var(--accent)`**. Acá es
donde el contrato de tokens deja de viajar por CSS y hay que resolverlo a mano
con `leerColor()` — que envuelve los tripletes RGB del contrato, porque pasarlos
crudos da un color inválido y **ECharts no falla**: dibuja con su color por
omisión. Y el tema es una dependencia del gráfico: si no se reconstruye, la
gráfica se queda del color viejo mientras el resto de la página ya cambió.

## Movimiento: sólo el que orienta

La regla que gobierna las microanimaciones de acá: **existen para explicar un
cambio, no para adornarlo**. Si no responden «¿qué pasó?» o «¿dónde estoy?»,
sobran — y en una interfaz que se usa ocho horas, lo que sobra molesta.

De ahí las tres duraciones: **120 ms** para lo que sigue al dedo, **180 ms** para
lo que informa, **220 ms** para lo que reemplaza una pantalla. Por encima de unos
250 ms el movimiento deja de leerse como respuesta y empieza a leerse como espera.

Hay cuatro y ninguna más: la entrada de una ruta, el filete del ítem activo del
riel (que crece, no aparece), la marca del índice de la vitrina, y el
desplazamiento suave al saltar a una sección.

**Nada de eso necesita su propio `prefers-reduced-motion`**: la regla global de la
capa `base` ya neutraliza animaciones, transiciones **y** scroll suave para todo
el árbol. Escribir la excepción en cada componente es cómo se termina olvidando
en uno.

> El índice de la vitrina usa `IntersectionObserver` con una franja delgada a la
> altura de los ojos (`rootMargin: -45% 0px -50% 0px`). Sin esa franja, «activa»
> sería siempre la primera sección visible, y el índice marcaría una que ya quedó
> fuera de la pantalla. Y observa el `<main>`, no la ventana: con el `root` por
> omisión no dispara nunca y el índice se queda quieto, sin ningún error.

## Lo que falta

- **Sin pruebas de componente.** Hay chequeo de tipos y de tokens; no hay
  `vitest`. Un proyecto que salga de acá debería agregarlas.
- **El trozo principal sigue en 1,16 MB** (315 KB comprimido). ECharts ya está
  fuera; el resto no se partió porque **se intentó y empeoró** — el detalle, con
  los números, está en `vite.config.ts`. El camino correcto es cargar las rutas
  con `lazy()`, no listar dependencias a mano.
- **Tres cosas quedaron sin verificar por el entorno, no por el código.** Todas
  dependen de que el navegador esté **visible**: con la pestaña oculta no corren
  `requestAnimationFrame`, ni `IntersectionObserver`, ni el desplazamiento suave.
  Se comprueban a ojo en medio minuto:
  1. Que las gráficas **dibujen** (la opción y el color sí se verificaron: seis
     temas, seis colores distintos en la línea).
  2. Que el **índice siga la lectura** al recorrer la vitrina.
  3. Que **`Esc` cierre el modal** — el cableado es correcto y es comportamiento
     nativo del navegador.
