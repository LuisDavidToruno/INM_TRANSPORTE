/**
 * Genera `src/pantallas/inventario.generado.ts` desde el inventario de pantallas.
 *
 * ── Por qué se genera y no se escribe ───────────────────────────────────────
 * `docs/04-diseno/inventario-de-pantallas.md` es la autoridad sobre qué pantallas existen y
 * cuáles están bloqueadas. Una copia escrita a mano en el frontend **es una copia que va a
 * divergir**: el día que el inventario dé de alta una pantalla, la aplicación seguiría diciendo
 * que hay 138 y nadie se enteraría. Acá se lee el documento y punto.
 *
 * ── La verificación ─────────────────────────────────────────────────────────
 * Con `--verificar` no escribe: compara y falla si el archivo generado quedó atrás. Va en
 * `npm run verificar`, así que el desfase se ve en el momento y no meses después.
 *
 * ── Lo que este script NO decide ────────────────────────────────────────────
 * **Cuáles están construidas.** Eso vive en `src/pantallas/registro.ts`, escrito a mano, porque
 * es una afirmación sobre el código y no sobre el documento. Derivarla de acá sería inventarse
 * que una pantalla existe porque está inventariada.
 */

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const aqui = path.dirname(fileURLToPath(import.meta.url));
const raiz = path.resolve(aqui, '..', '..');

const ORIGEN = path.join(raiz, 'docs', '04-diseno', 'inventario-de-pantallas.md');
const DESTINO = path.join(aqui, '..', 'src', 'pantallas', 'inventario.generado.ts');

/** Lo que el propio documento declara en su encabezado y en su §5. Si el conteo no da esto,
 *  o el documento cambió —y hay que actualizar estos números— o el parser se rompió. */
const ESPERADO = {
  total: 138,
  papel: { No: 99, Sí: 29, 'Parc.': 10 },
  cliente: { A: 103, 'A/C': 9, C: 25, P: 1 },
};

function extraer() {
  const lineas = fs.readFileSync(ORIGEN, 'utf8').split('\n');

  let seccion = '(sin sección)';
  const filas = [];
  const vistos = new Set();

  for (const l of lineas) {
    const sub = l.match(/^###\s+(\d+\.\d+)\s+(.+?)\s*$/);
    if (sub) { seccion = `${sub[1]} ${sub[2]}`; continue; }

    const sec = l.match(/^##\s+(\d+)\.\s+(.+?)\s*$/);
    if (sec) { seccion = `${sec[1]} ${sec[2]}`; continue; }

    if (!/^\|\s*PT-\d{3}\s*\|/.test(l)) continue;

    const c = l.split('|').slice(1, -1).map((x) => x.trim());
    if (c.length < 8) throw new Error(`Fila con columnas inesperadas: ${l.slice(0, 70)}`);

    const id = c[0];

    // `PT-138` aparece dos veces: la §2.5 la repite como eco de la §2.15. El propio
    // documento avisa que «no se cuenta dos veces en el recuento de la §5».
    if (vistos.has(id)) continue;
    vistos.add(id);

    filas.push({
      id,
      nombre: limpiar(c[1]),
      cliente: limpiar(c[2]),
      roles: c[3].replace(/`/g, '').trim(),
      cu: limpiar(c[4]),
      hu: limpiar(c[5]),
      sinRed: limpiar(c[6]),
      papel: limpiar(c[7]),
      seccion,
    });
  }

  return filas;
}

/** Quita negritas, notas entre paréntesis en cursiva y enlaces de markdown. */
function limpiar(celda) {
  return celda
    .replace(/\*\*/g, '')
    .replace(/\s*—?\s*\[.*?\]\(.*?\)\s*/g, '')
    .replace(/\s*\*\(.*?\)\*\s*/g, '')
    .replace(/`/g, '')
    .trim();
}

function contar(filas, campo) {
  return filas.reduce((a, f) => ({ ...a, [f[campo]]: (a[f[campo]] ?? 0) + 1 }), {});
}

function comprobarContraElDocumento(filas) {
  const problemas = [];

  if (filas.length !== ESPERADO.total) {
    problemas.push(`total ${filas.length}, el documento declara ${ESPERADO.total}`);
  }

  for (const [campo, esperado] of [['papel', ESPERADO.papel], ['cliente', ESPERADO.cliente]]) {
    const real = contar(filas, campo);
    for (const [k, v] of Object.entries(esperado)) {
      if (real[k] !== v) problemas.push(`${campo} «${k}»: ${real[k] ?? 0}, el documento declara ${v}`);
    }
  }

  if (problemas.length > 0) {
    throw new Error(
      'El conteo no coincide con lo que declara el inventario:\n  - ' +
      problemas.join('\n  - ') +
      '\n\nO el documento cambió —y hay que actualizar ESPERADO en este script— o el parser se rompió.',
    );
  }
}

function generar(filas) {
  const cuerpo = filas
    .map((f) => `  ${JSON.stringify(f)},`)
    .join('\n')
    .replace(/"([a-zA-ZáéíóúñÁÉÍÓÚÑ]+)":/g, '$1:');

  return `/* eslint-disable */
// ⚠️ ARCHIVO GENERADO — no editar a mano.
//
// Sale de \`docs/04-diseno/inventario-de-pantallas.md\`, que es la autoridad sobre qué
// pantallas existen. Para regenerarlo: \`npm run generar-inventario\`.
//
// \`npm run verificar\` falla si este archivo quedó atrás respecto del documento.

import type { PantallaInventariada } from './tipos';

/** Las ${filas.length} pantallas del inventario, en el orden en que el documento las lista. */
export const INVENTARIO: readonly PantallaInventariada[] = [
${cuerpo}
];
`;
}

const filas = extraer();
comprobarContraElDocumento(filas);
const contenido = generar(filas);

if (process.argv.includes('--verificar')) {
  const actual = fs.existsSync(DESTINO) ? fs.readFileSync(DESTINO, 'utf8') : '';

  if (actual !== contenido) {
    console.error(
      '✗ El inventario de pantallas del código quedó atrás respecto del documento.\n' +
      '  Corra: npm run generar-inventario',
    );
    process.exit(1);
  }

  console.log(`✓ Inventario: ${filas.length} pantallas, al día con el documento.`);
} else {
  fs.mkdirSync(path.dirname(DESTINO), { recursive: true });
  fs.writeFileSync(DESTINO, contenido);
  console.log(`✓ Generadas ${filas.length} pantallas en src/pantallas/inventario.generado.ts`);
}
