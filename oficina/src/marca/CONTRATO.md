# La capa de marca — qué tiene que definir

LOKI separa **la estructura** (que no se toca) de **la marca** (que se
reemplaza). Todo lo que distingue a un proyecto de otro vive en una carpeta de
esta ruta, y el resto del sistema no sabe cuál está puesta.

## Cómo se re-marca

Tres pasos, ninguno toca un componente:

1. Copiá `argos/` a `mi-marca/`.
2. Cambiá los **valores** de los tokens. No los nombres.
3. En `src/estilos/index.css`, cambiá la única línea que importa la marca:
   `@import '../marca/argos/tokens.css';` → `@import '../marca/mi-marca/tokens.css';`

Si necesitás tocar un componente para que tu marca se vea bien, **falta un
token**: pedilo, no lo parchees en el componente. Esa es la regla que hace que
agregar un séptimo tema sea gratis y que el parche sea caro.

## Los 74 nombres

Un nombre que falte **no rompe el build**: deja un `var()` sin resolver, que el
navegador descarta en silencio y el elemento hereda un color de su padre. Se ve
como «ese badge quedó raro», no como un error. Por eso la comprobación es un
test (`npm run verificar-tokens`), no una lista que alguien repase.

### Invariantes de estructura — 14 · **iguales en todos los temas**

No son marca: son el esqueleto. Un tema **no** los reasigna.

| Grupo | Tokens |
|---|---|
| Familias tipográficas | `--sans` `--serif` `--mono` |
| Radios | `--r-panel` (8px) `--r-control` (6px) `--r-badge` (4px) |
| Curva de animación | `--ease` |
| Escala de espaciado | `--sp-1` 4 · `--sp-2` 8 · `--sp-3` 12 · `--sp-4` 16 · `--sp-5` 22 · `--sp-6` 30 · `--sp-7` 40 |

> La escala **no es lineal a propósito**: los saltos crecen (4·8·12·16·22·30·40)
> para que dos niveles vecinos nunca se confundan a simple vista. Una escala
> lineal deja pares indistinguibles y la decisión de cuál usar se vuelve arbitraria.

### Densidad — 5 × 2 modos

`comoda` (fila 44 px) y `compacta` (fila 36 px, +18 % de filas por pantalla).

`--h-control` `--h-control-sm` `--row-h` `--pad-panel` `--gap-grid`

> La densidad **no cambia tamaños de letra**. Sólo alturas de control, alto de
> fila, relleno y separación. Es un ajuste de apariencia, no un tema — y bajar
> el cuerpo de la letra para meter más filas es cómo una interfaz densa se
> vuelve ilegible.

### Por tema — 55 × 6 temas

Cada tema reasigna estos 55 valores. **Ninguno agrega una regla de componente.**

| Grupo | Cuántos | Tokens |
|---|---|---|
| Superficies | 4 | `--surface-canvas` `--surface-panel` `--surface-subtle` `--surface-inset` |
| Bordes | 4 | `--border` `--border-soft` `--border-input` `--border-hover` |
| Texto | 5 | `--text-hi` `--text-base` `--text-mid` `--text-low` `--text-axis` |
| Marca | 3 | `--brand-navy` `--accent` `--accent-ink` |
| Botón primario | 3 | `--btn-bg` `--btn-bg-hover` `--btn-fg` |
| Botón destructivo | 3 | `--btn-riesgo-bg` `--btn-riesgo-bg-hover` `--btn-riesgo-fg` |
| Foco y elevación | 4 | `--focus-ring` `--shadow` `--shadow-lift` `--overlay` |
| Tonos semánticos | 15 | `--ok-*` `--info-*` `--aviso-*` `--riesgo-*` `--neutro-*`, cada uno con `-bg` `-fg` `-bd` |
| Navegación | 7 | `--nav-bg` `--nav-hover` `--nav-active` `--nav-rail` `--nav-hi` `--nav-mid` `--nav-line` |
| Plazo | 3 | `--plazo-aldia` `--plazo-porvencer` `--plazo-vencido` |
| Tipo de operación | 4 | `--op-viatico` `--op-gira` `--op-liquidacion` `--op-anulacion` |

## Tres reglas que se rompen solas si no se saben

**1 · El botón destructivo NO se deriva del semáforo.** `--btn-riesgo-bg` y
`--riesgo-bg` son cosas distintas: uno es una **acción**, el otro es un **dato**.
El fondo de la pastilla de riesgo no aguanta texto blanco (3,68:1). Derivar uno
del otro produce un botón «Eliminar» ilegible que en la captura de pantalla se ve
bien.

**2 · Superficies, bordes y texto van como triplete RGB sin envolver.**
`--surface-panel: 255 255 255`, no `#ffffff`. Es lo que deja que Tailwind les
aplique opacidad (`tw:bg-panel/60`). Los tokens que ya llevan alfa propia
(`--focus-ring`, `--overlay`, `--shadow`) van como color completo y se consumen
con `var()` directo. Mezclar las dos formas rompe la utilidad o rompe el alfa.

**3 · `--border-input` tiene un piso de contraste; `--border` no.**
`--border-input` alcanza 3:1 sobre `--surface-panel` en los seis temas porque
WCAG 1.4.11 lo exige: es el límite de un control de formulario, y si no se ve,
el campo no se ve. `--border` es estructura decorativa y no está sujeto a ese
piso. Igualarlos «por prolijidad» engorda toda la interfaz de líneas duras.

## Lo que probablemente quieras renombrar

Los 4 tokens de **tipo de operación** (`--op-viatico`, `--op-gira`,
`--op-liquidacion`, `--op-anulacion`) son del dominio de ARGOS: viáticos y
liquidaciones del INM. Un proyecto de otro dominio los renombra a sus propios
tipos — son el único grupo de los 74 que no es genérico, y está acá porque un
sistema real necesita colorear sus objetos, no porque todo sistema tenga giras.

Si los renombrás, `npm run verificar-tokens` te va a marcar los usos que
quedaron colgando. Ese es el punto de que sea un test.
