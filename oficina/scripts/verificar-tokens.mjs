import { readFileSync, readdirSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

/**
 * Auditoría de tokens: NINGUNA `var(--x)` sin resolver.
 *
 * ── Por qué esto es un test y no una revisión ────────────────────────────────
 * Un `var(--token-que-no-existe)` **no rompe nada**. El navegador descarta la
 * declaración entera y el elemento hereda el color de su padre. En tema claro
 * eso suele ser invisible —un panel blanco sobre lienzo casi blanco se ve
 * igual—, así que el defecto viaja hasta que alguien abre el tema `navy` y ve
 * un bloque del color equivocado.
 *
 * Es exactamente el modo de falla que este proyecto ya vivió: al portar los
 * componentes apareció una clase (`text-exito-fg`) cuyo token no existe en
 * ninguna capa. Nadie lo había notado.
 *
 * ── Qué NO comprueba ─────────────────────────────────────────────────────────
 * Que el valor sea el correcto. Un `--surface-panel` definido en rosa pasa esta
 * auditoría sin chistar. Eso lo cubre el contraste, que se mide, y el ojo, que
 * mira. Acá sólo se comprueba que todo lo que se usa, exista.
 */

const RAIZ = new URL('..', import.meta.url).pathname.replace(/^\/([A-Za-z]:)/, '$1');

/** Las que declara el propio Tailwind en tiempo de compilación. No son nuestras. */
const AJENAS = /^--tw-/;

function archivos(dir, exts, salida = []) {
  for (const nombre of readdirSync(dir)) {
    if (nombre === 'node_modules' || nombre.startsWith('.')) continue;
    const ruta = join(dir, nombre);
    if (statSync(ruta).isDirectory()) archivos(ruta, exts, salida);
    else if (exts.some((e) => nombre.endsWith(e))) salida.push(ruta);
  }
  return salida;
}

const fuentes = archivos(join(RAIZ, 'src'), ['.css', '.ts', '.tsx']);

/* ── Lo que está DEFINIDO ─────────────────────────────────────────────────────
   Un token se define con `--nombre:`. Vale tanto en `:root` como dentro de un
   `@theme`, y por eso se busca la forma y no el bloque: da igual dónde esté
   declarado mientras la cascada lo alcance. */
const definidos = new Set();
for (const ruta of fuentes.filter((f) => f.endsWith('.css'))) {
  const css = readFileSync(ruta, 'utf8');
  for (const [, nombre] of css.matchAll(/(--[a-zA-Z0-9-]+)\s*:/g)) definidos.add(nombre);
}

/**
 * Borra los comentarios conservando la CUENTA DE LÍNEAS.
 *
 * Hace falta porque este proyecto explica sus decisiones en comentarios largos, y
 * varios de ellos citan tokens como ejemplo — incluido un `var(--token-inexistente)`
 * que documenta, justamente, cómo se detecta un token ausente. Sin esto la
 * auditoría reporta la explicación del defecto como si fuera el defecto.
 *
 * Se reemplaza por espacios en vez de quitarse: si las líneas se corrieran, cada
 * hallazgo apuntaría a una línea que no es, y un informe que manda al lugar
 * equivocado se deja de leer a la segunda vez.
 */
function sinComentarios(texto) {
  return texto
    .replace(/\/\*[\s\S]*?\*\//g, (m) => m.replace(/[^\n]/g, ' '))
    .replace(/^\s*\/\/.*$/gm, (m) => ' '.repeat(m.length));
}

/* ── Lo que se USA ────────────────────────────────────────────────────────────
   Se barre también el TSX: un `style={{ color: 'var(--x)' }}` falla igual de
   silencioso que el mismo `var()` en una hoja de estilos, y es más fácil de
   pasar por alto porque no está donde uno busca.

   ⚠️ `var(--x, respaldo)` NO se reporta, y es deliberado: un `var()` con respaldo
   no puede quedar sin resolver — si el token falta, se usa el respaldo, que es
   exactamente lo que su autor declaró que pasara. Es como `FilaKpis` pasa el
   número de columnas (`var(--kpi-cols, 4)`), escribiéndolo por `style` en tiempo
   de ejecución: el token no existe en ninguna hoja y no tiene por qué existir.
   El precio de esta exención es que un respaldo con el nombre mal escrito pasa
   inadvertido; se acepta porque ahí el resultado sigue siendo el declarado. */
const faltantes = new Map();
for (const ruta of fuentes) {
  const texto = sinComentarios(readFileSync(ruta, 'utf8'));
  const lineas = texto.split('\n');

  lineas.forEach((linea, i) => {
    // El `(?!\s*,)` es lo que deja pasar los que traen respaldo.
    for (const [, nombre] of linea.matchAll(/var\(\s*(--[a-zA-Z0-9-]+)\s*(?!\s*,)\)/g)) {
      if (AJENAS.test(nombre) || definidos.has(nombre)) continue;
      if (!faltantes.has(nombre)) faltantes.set(nombre, []);
      faltantes.get(nombre).push(`${relative(RAIZ, ruta).replace(/\\/g, '/')}:${i + 1}`);
    }
  });
}

/* ── Cordura ──────────────────────────────────────────────────────────────────
   Una auditoría que no encuentra NADA que auditar tiene que fallar, no pasar.
   Un `glob` mal escrito o una carpeta movida la dejarían en verde sin haber
   mirado un solo archivo, y un verde así es peor que no tenerla: además
   tranquiliza. */
if (definidos.size < 70) {
  console.error(
    `✗ La auditoría encontró sólo ${definidos.size} tokens definidos. El contrato tiene 74 ` +
      `más los del @theme: o se movió la capa de marca, o este script está mirando donde no es.`,
  );
  process.exit(1);
}

if (faltantes.size === 0) {
  console.log(`✓ Tokens: ${definidos.size} definidos · 0 sin resolver (${fuentes.length} archivos).`);
  process.exit(0);
}

console.error(`✗ ${faltantes.size} token(s) usados y NO definidos:\n`);
for (const [nombre, usos] of [...faltantes].sort()) {
  console.error(`  ${nombre}`);
  for (const uso of usos) console.error(`      ${uso}`);
}
console.error(
  '\nUn var() sin resolver no falla: el navegador lo descarta y el elemento hereda\n' +
    'el color de su padre. Si el token es nuevo, va en src/marca/<marca>/tokens.css\n' +
    'y en los SEIS temas — definirlo en uno solo deja los otros cinco rotos.',
);
process.exit(1);
