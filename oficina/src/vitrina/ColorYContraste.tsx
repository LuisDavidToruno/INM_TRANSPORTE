import { useMemo } from 'react';
import type { ReactElement } from 'react';

import { useTema } from '../tema/TemaProvider';

/**
 * Contraste de los 21 pares, MEDIDO en vivo — no declarado.
 * Traducido de `ACCEPTANCE.md §1` y de la sección «Color y contraste» de la vitrina.
 *
 * ── Por qué se mide acá y no se copia un informe ─────────────────────────────
 * Un número escrito en un documento envejece al primer cambio de token. Esto lee
 * lo que el navegador REALMENTE resuelve, con el tema que esté puesto, y se
 * recalcula al cambiarlo. Es la prueba de que el contrato aguanta en los seis —
 * corriendo dentro de ARGOS, no en un PDF.
 *
 * ── La doble sonda va en el PADRE, y ese detalle es el que importa ───────────
 * Un `var(--token-inexistente)` NO deja el centinela en el nodo: la sustitución
 * es inválida, la propiedad toma el valor garantizado-inválido y, como `color`
 * se hereda, el nodo termina con el color del PADRE. Con el centinela en el
 * propio nodo, las dos sondas devuelven lo mismo y un token AUSENTE se reporta
 * como que cumple — que fue exactamente el defecto que encontramos en 0.3.1.
 *
 * Con el centinela en el padre, el valor inválido hereda el centinela y las dos
 * sondas difieren. Ahí se detecta.
 */

type Par = readonly [rotulo: string, fg: string, bg: string, meta: number, muestra: string];

const PARES: Par[] = [
  ['Título sobre panel', 'rgb(var(--text-hi))', 'rgb(var(--surface-panel))', 4.5, 'Aa'],
  ['Cuerpo sobre panel', 'rgb(var(--text-base))', 'rgb(var(--surface-panel))', 4.5, 'Aa'],
  ['Secundario sobre panel', 'rgb(var(--text-mid))', 'rgb(var(--surface-panel))', 4.5, 'Aa'],
  ['Ayuda sobre panel', 'rgb(var(--text-low))', 'rgb(var(--surface-panel))', 4.5, 'Aa'],
  ['Cuerpo sobre lienzo', 'rgb(var(--text-base))', 'rgb(var(--surface-canvas))', 4.5, 'Aa'],
  ['Cuerpo sobre sutil', 'rgb(var(--text-base))', 'rgb(var(--surface-subtle))', 4.5, 'Aa'],
  ['Eje de gráfico', 'rgb(var(--text-axis))', 'rgb(var(--surface-panel))', 3, 'Aa'],
  ['Acción primaria', 'var(--btn-fg)', 'var(--btn-bg)', 4.5, 'Guardar'],
  ['Acento como texto', 'var(--accent-ink)', 'rgb(var(--surface-panel))', 4.5, 'Enlace'],
  ['Pastilla · ok', 'var(--ok-fg)', 'var(--ok-bg)', 4.5, 'Aprobado'],
  ['Pastilla · info', 'var(--info-fg)', 'var(--info-bg)', 4.5, 'En revisión'],
  ['Pastilla · aviso', 'var(--aviso-fg)', 'var(--aviso-bg)', 4.5, 'Corrección'],
  ['Pastilla · riesgo', 'var(--riesgo-fg)', 'var(--riesgo-bg)', 4.5, 'Vencido'],
  ['Pastilla · neutro', 'var(--neutro-fg)', 'var(--neutro-bg)', 4.5, 'Cancelado'],
  ['Riel · texto activo', 'var(--nav-hi)', 'var(--nav-bg)', 4.5, 'Inicio'],
  ['Riel · texto inactivo', 'var(--nav-mid)', 'var(--nav-bg)', 4.5, 'Catálogos'],
  ['Borde de campo', 'rgb(var(--border-input))', 'rgb(var(--surface-panel))', 3, '—'],
  ['Borde de campo activo', 'rgb(var(--border-hover))', 'rgb(var(--surface-panel))', 3, '—'],
  ['Borde de plazo vencido', 'var(--plazo-vencido)', 'rgb(var(--surface-panel))', 3, '—'],
  ['Botón destructivo', 'var(--btn-riesgo-fg)', 'var(--btn-riesgo-bg)', 4.5, 'Anular'],
  ['Botón destructivo hover', 'var(--btn-riesgo-fg)', 'var(--btn-riesgo-bg-hover)', 4.5, 'Anular'],
];

function resolver(css: string): number[] | null {
  const padre = document.createElement('div');
  padre.style.cssText = 'position:absolute;width:0;height:0;opacity:0;pointer-events:none';
  const nodo = document.createElement('span');
  padre.appendChild(nodo);
  document.body.appendChild(padre);
  const sondear = (centinela: string): string => {
    padre.style.color = centinela;
    nodo.style.color = css;
    return getComputedStyle(nodo).color;
  };
  const a = sondear('rgb(0, 0, 0)');
  const b = sondear('rgb(255, 0, 255)');
  padre.remove();
  if (a !== b) return null; // el valor no se aplicó: token ausente o inválido
  const m = a.match(/[\d.]+/g);
  if (m === null) return null;
  const n = m.map(Number);
  return n.length > 3 ? n : [n[0]!, n[1]!, n[2]!, 1];
}

const sobre = (fg: number[], bg: number[]): number[] =>
  [0, 1, 2].map((i) => fg[i]! * fg[3]! + bg[i]! * (1 - fg[3]!));

const luz = (c: number[]): number => {
  const f = (v: number): number => {
    const x = v / 255;
    return x <= 0.03928 ? x / 12.92 : ((x + 0.055) / 1.055) ** 2.4;
  };
  return 0.2126 * f(c[0]!) + 0.7152 * f(c[1]!) + 0.0722 * f(c[2]!);
};

const razon = (a: number[], b: number[]): number => {
  const x = luz(a);
  const y = luz(b);
  return (Math.max(x, y) + 0.05) / (Math.min(x, y) + 0.05);
};

export default function ColorYContraste(): ReactElement {
  const { tema } = useTema();

  // Se recalcula al cambiar de tema: ésa es toda la gracia. `tema` en las
  // dependencias no es decorativo — sin él la tabla mostraría los valores del
  // primer tema para siempre.
  const filas = useMemo(
    () =>
      PARES.map(([rotulo, fgT, bgT, meta, muestra]) => {
        const bgR = resolver(bgT);
        const fgR = resolver(fgT);
        const panel = resolver('rgb(var(--surface-panel))');
        if (bgR === null || fgR === null || panel === null) {
          return { rotulo, muestra, meta, medido: null, cumple: false, fgT, bgT };
        }
        // Los tonos de estado en temas oscuros llevan alfa propia, así que su
        // contraste depende de la superficie sobre la que caen. Se componen
        // sobre el panel antes de medir, igual que hace el navegador al pintar.
        const bg = bgR[3]! < 1 ? sobre(bgR, panel) : bgR;
        const fg = fgR[3]! < 1 ? sobre(fgR, bg) : fgR;
        const r = razon(fg, bg);
        return { rotulo, muestra, meta, medido: r, cumple: r >= meta, fgT, bgT };
      }),
    [tema],
  );

  const fallos = filas.filter((f) => !f.cumple).length;

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      <p
        className={[
          'tw:self-start tw:rounded-badge tw:border tw:px-2 tw:py-[3px] tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:uppercase',
          fallos === 0 ? 'tono-ok' : 'tono-riesgo',
        ].join(' ')}
      >
        {fallos === 0
          ? `Los ${filas.length} pares cumplen`
          : `${fallos} de ${filas.length} no cumplen`}
      </p>

      <div className="tw:overflow-x-auto">
        <table className="tw:w-full tw:text-cuerpo-2">
          <thead>
            <tr className="tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase">
              <th className="loki-celda tw:py-2 tw:text-left">Par</th>
              <th className="loki-celda tw:py-2 tw:text-left">Muestra</th>
              <th className="loki-celda tw:py-2 tw:text-right">Medido</th>
              <th className="loki-celda tw:py-2 tw:text-left">Objetivo</th>
              <th className="loki-celda tw:py-2 tw:text-left">Veredicto</th>
            </tr>
          </thead>
          <tbody>
            {filas.map((f) => (
              <tr key={f.rotulo} className="tw:border-t tw:border-linea-suave">
                <td className="loki-celda tw:py-1.5 tw:text-tinta-base">{f.rotulo}</td>
                <td className="loki-celda tw:py-1.5">
                  <span
                    className="tw:inline-flex tw:items-center tw:justify-center tw:rounded-badge tw:px-2 tw:py-1 tw:text-cuerpo-2"
                    style={{ color: f.fgT, background: f.bgT }}
                  >
                    {f.muestra}
                  </span>
                </td>
                <td className="loki-celda tw:py-1.5 tw:text-right tw:font-mono tw:tabular-nums tw:text-tinta-base">
                  {f.medido === null ? 'sin resolver' : `${f.medido.toFixed(2)}:1`}
                </td>
                <td className="loki-celda tw:py-1.5 tw:font-mono tw:text-tinta-low">
                  ≥ {f.meta}:1
                </td>
                <td className="loki-celda tw:py-1.5">
                  <span
                    className={[
                      'tw:rounded-badge tw:border tw:px-1.5 tw:py-[2px] tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:uppercase',
                      f.cumple ? 'tono-ok' : 'tono-riesgo',
                    ].join(' ')}
                  >
                    {f.cumple ? 'Cumple' : f.medido === null ? 'Token ausente' : 'Falla'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <p className="tw:text-cuerpo-2 tw:text-tinta-low">
        <strong>El piso de 3:1 aplica al borde de campo, no al de panel.</strong> WCAG 1.4.11 pide
        3:1 para el límite de un componente de formulario — de ahí que{' '}
        <code className="tw:font-mono tw:text-tinta-mid">--border-input</code> sea notablemente más
        marcado que <code className="tw:font-mono tw:text-tinta-mid">--border</code>. El borde de un
        panel es estructura decorativa: subirlo a 3:1 enmarcaría cada panel como una caja.
      </p>
    </div>
  );
}
