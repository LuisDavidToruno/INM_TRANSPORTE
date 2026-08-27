import type { ReactElement } from 'react';

/**
 * La escala tipográfica del contrato, mostrada con texto real.
 * Traducida de la sección «Tipografía» de la vitrina del paquete.
 *
 * ── Tres familias, un rol cada una ───────────────────────────────────────────
 * La serifa aparece SÓLO en título de página y de sección. En tabla o en botón
 * rompe la lectura: en texto denso ralentiza el escaneo y todo pesa igual.
 *
 * ── Se muestra con la frase real, no con «Aa» ────────────────────────────────
 * Un espécimen tipográfico con dos letras no dice nada sobre cómo se lee una
 * bandeja. Cada fila usa el texto que de verdad aparece en el sistema.
 */

interface Fila {
  rol: string;
  muestra: string;
  ficha: string;
  clase: string;
}

const FILAS: Fila[] = [
  { rol: 'Título de página', muestra: 'Solicitudes por atender', ficha: 'serif · 26 / 600 · 1.15', clase: 'tw:font-serif tw:text-pagina tw:font-semibold tw:text-tinta-hi' },
  { rol: 'Título de sección', muestra: 'Reglas del contrato', ficha: 'serif · 19 / 600', clase: 'tw:font-serif tw:text-seccion tw:font-semibold tw:text-tinta-hi' },
  { rol: 'Título de panel', muestra: 'Desglose del cálculo', ficha: 'sans · 14 / 600', clase: 'tw:font-sans tw:text-titulo tw:font-semibold tw:text-tinta-hi' },
  { rol: 'Cuerpo', muestra: 'El borde izquierdo de cada fila marca el plazo de atención.', ficha: 'sans · 13 / 400 · 1.5', clase: 'tw:font-sans tw:text-cuerpo tw:text-tinta-base' },
  { rol: 'Cuerpo secundario', muestra: 'Se recalcula por zona × categoría × duración.', ficha: 'sans · 12.5 / 400 · 1.55', clase: 'tw:font-sans tw:text-cuerpo-2 tw:text-tinta-base' },
  { rol: 'Celda de tabla', muestra: 'Gerencia de Operaciones', ficha: 'sans · 12.5 / 400', clase: 'tw:font-sans tw:text-cuerpo-2 tw:text-tinta-base' },
  { rol: 'Rótulo de campo', muestra: 'Fecha de finalización', ficha: 'sans · 11.5 / 600', clase: 'tw:font-sans tw:text-rotulo tw:font-semibold tw:text-tinta-mid' },
  { rol: 'Cabecera de tabla', muestra: 'FECHA DE INICIO', ficha: 'sans · 10 / 600 · .08em · versalitas', clase: 'tw:font-sans tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase' },
  { rol: 'Pastilla', muestra: 'Revisión Inicial Viáticos', ficha: 'sans · 11 / 500', clase: 'tw:font-sans tw:text-pastilla tw:font-medium tw:text-tinta-base' },
  { rol: 'Ayuda y pie', muestra: 'Hábiles, a partir de hoy.', ficha: 'sans · 11 / 400', clase: 'tw:font-sans tw:text-ayuda tw:text-tinta-low' },
  { rol: 'Cifra de KPI', muestra: '2.14 M', ficha: 'mono · 26 / 600 · tabular', clase: 'tw:font-mono tw:text-kpi tw:font-semibold tw:tabular-nums tw:text-tinta-hi' },
  { rol: 'Importe en tabla', muestra: 'L. 49,990.65', ficha: 'mono · 12 / 500 · tabular', clase: 'tw:font-mono tw:text-importe tw:font-medium tw:tabular-nums tw:text-tinta-base' },
  { rol: 'Referencia', muestra: 'MSV-26-1255-GT', ficha: 'mono · 11.5 / 500', clase: 'tw:font-mono tw:text-rotulo tw:font-medium tw:text-acento-ink' },
];

export default function EscalaTipografica(): ReactElement {
  return (
    <div className="tw:flex tw:flex-col">
      {FILAS.map((f) => (
        <div
          key={f.rol}
          className="tw:flex tw:items-baseline tw:gap-4 tw:border-b tw:border-linea-suave tw:py-2.5"
        >
          <span className="loki-especimen-rol tw:shrink-0 tw:text-ayuda tw:text-tinta-low">
            {f.rol}
          </span>
          <span className={`tw:min-w-0 tw:flex-1 tw:truncate ${f.clase}`}>{f.muestra}</span>
          <span className="tw:shrink-0 tw:font-mono tw:text-ayuda tw:text-tinta-axis">
            {f.ficha}
          </span>
        </div>
      ))}
      <p className="tw:pt-3 tw:text-cuerpo-2 tw:text-tinta-low">
        Los importes llevan <strong>tres</strong> clases, no una:{' '}
        <code className="tw:font-mono tw:text-tinta-mid">font-mono</code> +{' '}
        <code className="tw:font-mono tw:text-tinta-mid">tabular-nums</code> +{' '}
        <code className="tw:font-mono tw:text-tinta-mid">text-right</code>. Sin las tres los
        decimales no alinean y la columna deja de ser comparable de un vistazo.
      </p>
    </div>
  );
}
