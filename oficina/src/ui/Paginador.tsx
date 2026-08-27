import { ChevronLeft, ChevronRight } from 'lucide-react';
import type { ReactElement } from 'react';

/**
 * Paginador con NÚMEROS de página.
 *
 * ── Lo que reemplaza ─────────────────────────────────────────────────────────
 *
 * Las dos tablas del frontend paginaban con «‹ 3 / 12 ›»: se sabía dónde se estaba y no se
 * podía ir a ningún lado. Llegar a la página 8 desde la 3 eran cinco pulsaciones, y volver
 * otras cinco. Con números es una.
 *
 * ── Por qué compartido ───────────────────────────────────────────────────────
 *
 * Porque son dos tablas —la del contrato y la de las pantallas— y ya habían divergido: una
 * decía «21–40 de 137» y la otra «Página 2 de 7 · 137 registros». Escrito dos veces, el
 * próximo arreglo se aplica a una y nadie nota la que faltó; es exactamente el motivo por el
 * que existe `Segmentado`.
 *
 * ── Se dicen las dos cosas, y no es redundancia ──────────────────────────────
 *
 * El RANGO («21–40 de 137») contesta cuántas quedan, que es lo que importa trabajando una
 * bandeja. Los NÚMEROS contestan dónde estoy y adónde puedo ir. Son preguntas distintas: la
 * primera se responde con filas, la segunda con páginas.
 */

/**
 * Los números que se dibujan, con `null` donde hay un salto.
 *
 * Se exporta para poder probarla: los bordes —primera página, última, y el salto que aparece
 * de un lado y no del otro— son fáciles de escribir mal, y un paginador mal recortado no
 * falla: simplemente esconde páginas a las que después nadie llega.
 *
 * @param actual Página actual, base 0.
 * @param total Cantidad de páginas. Siempre ≥ 1 cuando el paginador se dibuja.
 */
export function ventanaDePaginas(actual: number, total: number): readonly (number | null)[] {
  // Hasta siete entran todas, y mostrarlas completas es mejor que recortar: los puntos
  // suspensivos sólo valen la pena cuando de verdad ahorran ancho.
  if (total <= 7) return Array.from({ length: total }, (_, i) => i);

  const paginas = new Set<number>([0, total - 1, actual]);
  // Las vecinas inmediatas: sin ellas, avanzar de a una obliga a volver a los chevrons.
  if (actual - 1 > 0) paginas.add(actual - 1);
  if (actual + 1 < total - 1) paginas.add(actual + 1);

  // Cerca de un extremo el recorte deja hueco de un solo lado, y sin esto la tira cambiaría de
  // ancho al moverse: los números bailarían bajo el dedo justo cuando se los está pulsando.
  // Son CUATRO y no tres para que el total dé siempre siete celdas, en cualquier página —está
  // afirmado en la prueba, no confiado a este comentario.
  if (actual <= 2) {
    paginas.add(1);
    paginas.add(2);
    paginas.add(3);
    paginas.add(4);
  }
  if (actual >= total - 3) {
    paginas.add(total - 2);
    paginas.add(total - 3);
    paginas.add(total - 4);
    paginas.add(total - 5);
  }

  const ordenadas = [...paginas].filter((p) => p >= 0 && p < total).sort((a, b) => a - b);

  const salida: (number | null)[] = [];
  let previa: number | null = null;
  for (const p of ordenadas) {
    // Un salto de exactamente uno se rellena con la página que falta: «1 … 3» ocupa lo mismo
    // que «1 2 3» y esconde una página sin necesidad.
    if (previa !== null && p - previa === 2) salida.push(previa + 1);
    else if (previa !== null && p - previa > 2) salida.push(null);
    salida.push(p);
    previa = p;
  }
  return salida;
}

export default function Paginador({
  pagina,
  totalPaginas,
  onCambio,
  desde,
  hasta,
  total,
}: {
  /** Página actual, base 0. */
  readonly pagina: number;
  readonly totalPaginas: number;
  readonly onCambio: (pagina: number) => void;
  /** Primera fila visible, base 1 — para el rango. */
  readonly desde: number;
  /** Última fila visible, base 1. */
  readonly hasta: number;
  /** Filas que hay en total tras filtrar. */
  readonly total: number;
}): ReactElement {
  return (
    <nav
      // `aria-label` y no un `role=group`: para quien no ve la pantalla esto es navegación,
      // y así aparece en la lista de regiones en vez de quedar como un grupo de botones.
      aria-label="Paginación"
      className="tw:flex tw:flex-wrap tw:items-center tw:justify-between tw:gap-3"
    >
      <p className="tw:text-xs tw:text-tinta-mid">
        <span className="tw:font-mono tw:tabular-nums">
          {desde}–{hasta}
        </span>{' '}
        de <span className="tw:font-mono tw:tabular-nums">{total}</span>
      </p>

      <div className="tw:flex tw:items-center tw:gap-1">
        <button
          type="button"
          onClick={() => onCambio(Math.max(0, pagina - 1))}
          disabled={pagina === 0}
          aria-label="Página anterior"
          className="loki-foco loki-control-sm tw:rounded-control tw:border tw:border-linea tw:px-2 tw:text-tinta-mid tw:disabled:opacity-40 tw:hover:text-tinta-hi"
        >
          <ChevronLeft size={14} aria-hidden="true" />
        </button>

        {ventanaDePaginas(pagina, totalPaginas).map((p, i) =>
          p === null ? (
            <span
              // La clave lleva el índice porque puede haber DOS saltos y `null` no distingue.
              key={`salto-${i}`}
              aria-hidden="true"
              className="tw:px-1 tw:text-xs tw:text-tinta-low"
            >
              …
            </span>
          ) : (
            <button
              key={p}
              type="button"
              onClick={() => onCambio(p)}
              // Lo que dice el número es su posición, base 1; la de adentro es base 0.
              aria-label={`Página ${p + 1}`}
              // Es la página en la que se está, no un botón «apretado»: `aria-current` es lo
              // que un lector de pantalla anuncia como «actual».
              {...(p === pagina ? { 'aria-current': 'page' as const } : {})}
              className={[
                'loki-foco loki-control-sm tw:min-w-[26px] tw:rounded-control tw:px-1.5 tw:font-mono tw:text-xs tw:tabular-nums',
                p === pagina
                  ? 'tw:border tw:border-linea tw:bg-inset tw:font-semibold tw:text-tinta-hi'
                  : 'tw:text-tinta-mid tw:hover:text-tinta-hi',
              ].join(' ')}
            >
              {p + 1}
            </button>
          ),
        )}

        <button
          type="button"
          onClick={() => onCambio(Math.min(totalPaginas - 1, pagina + 1))}
          disabled={pagina >= totalPaginas - 1}
          aria-label="Página siguiente"
          className="loki-foco loki-control-sm tw:rounded-control tw:border tw:border-linea tw:px-2 tw:text-tinta-mid tw:disabled:opacity-40 tw:hover:text-tinta-hi"
        >
          <ChevronRight size={14} aria-hidden="true" />
        </button>
      </div>
    </nav>
  );
}
