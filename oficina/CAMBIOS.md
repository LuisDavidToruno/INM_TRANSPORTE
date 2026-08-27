# Cambios del contrato

Qué trae cada versión de **este** sistema de diseño. Un número sin registro de
qué cambió es sólo un número.

> **Dos versiones, no una.** Este contrato desciende de la **entrega ARGOS
> 0.3.2** (05/08/2026), que es inmutable y no se renumera nunca: su número
> describe lo que ARGOS entregó, no lo que nosotros hicimos después. Los
> comentarios que dicen `Canon: handoff-argos-0.3.2/…` señalan procedencia y se
> quedan como están — ver `DEC-010` en `../DECISIONES.md`.

---

## 0.3.3 — 2026-08-25

Primera versión que se separa del número de la entrega. Todo lo de acá es
**aditivo**: las props nuevas son opcionales y conservan el comportamiento
anterior por omisión, así que una pantalla escrita contra 0.3.2 compila y se ve
igual sin tocar una línea.

### El nivel de encabezado deja de ser fijo

`Panel` y `Vacio` aceptan `nivel?: 2 | 3 | 4` — `3` por omisión, como antes.

Ambos emitían `h3` fijo, así que cualquiera colgado directo del `h1` de la
pantalla abría un hueco en el esquema del documento: quien recorre por
encabezados —la forma habitual de orientarse con lector de pantalla— no podía
saber si se había perdido un nivel o si nunca existió.

**El nivel cambia la etiqueta, nunca el tamaño.** Si un `h2` se viera más grande
que otro, ponerlo bien costaría un rediseño y nadie lo pondría.

### La tabla avisa cuando sigue hacia el costado

`Tabla` acepta `rotulo?: string`. Cuando el contenido no cabe, el contenedor
pasa a **región enfocable con nombre** y dos sombras marcan de qué lado queda
tabla fuera de la vista.

Medido en el catálogo de LOKI: a 375 px se veían dos de nueve columnas, con
868 px fuera de la vista y nada que lo dijera. Y el desplazamiento de un
`overflow` sólo lo alcanza quien tiene mouse — por teclado no había forma de
llegar a las columnas ocultas.

El `tabindex` se pone **sólo cuando desborda**: una parada de tabulación en algo
que no se puede desplazar la paga quien recorre por teclado en cada tabla que sí
cabe.

### La barra de ubicación navega

`BarraSuperior` recibe `Miga[]` en vez de `string[]`. Un tramo con `href` es un
destino y se dibuja subrayado; una cadena suelta es sólo rótulo, para el
agrupador que no es una pantalla. El último nunca se enlaza aunque traiga
destino, y lleva `aria-current="page"`.

Antes los tramos eran `<span>`: hacer clic en uno para volver devolvía una
selección de texto. Una miga que se ve como miga y no navega es la única pieza
de la interfaz que miente sobre lo que hace.

### El riel deja de recargar la aplicación

`Riel` usa `Enlace` en vez de un ancla de documento. Medido en LOKI: desde una
vista que tarda 18 segundos en dibujarse, cada clic en el riel tiraba ese
trabajo y volvía a descargar la biblioteca que lo dibuja.

### El shell se puede montar sin paleta y sin riel desplegado

`Shell` acepta `sinBusqueda?: boolean` y `plegadoPorOmision?: boolean`.

- `sinBusqueda` apaga el disparador de la barra **y** el atajo ⌘K a la vez. Una
  aplicación que no monte la paleta no puede mostrar su disparador: prometería
  una función que no existe, y un botón muerto es peor que uno ausente.
- `plegadoPorOmision` decide el arranque **sólo** cuando el usuario todavía no
  eligió; desde su primer clic manda su preferencia. Un riel con un solo destino
  no es navegación, es margen: medido, 78 px de ítems en 720 px de alto.

### Correcciones

| Qué | Por qué importaba |
|---|---|
| `Tabla` deja de envolver el slot vacío en un `<p>` | `Vacio` emite encabezado, texto y contenedor: el navegador cerraba el párrafo antes de tiempo y el texto se partía a media frase |
| `Esqueleto` documenta que **exige ancestro posicionado** | Va en `position: absolute`; sin ancestro posicionado se escapa al bloque contenedor inicial. Con varios esperando, se apilan todos en la misma caja y aparece una banda que tapa la pantalla — un síntoma que no se parece en nada a su causa |

### Clases nuevas en `estilos/index.css`

`.loki-tabla-marco` con sus dos sombras, `.loki-tabla-visor:focus-visible`, y
`.loki-barra-miga-enlace`.

---

## 0.3.2 — 05/08/2026

La entrega de ARGOS, traducida a React. Es el punto de partida de este
repositorio y no tiene entrada propia acá: lo que define está en su
`COMPONENTS.md`, que vive en el repositorio de ARGOS.
