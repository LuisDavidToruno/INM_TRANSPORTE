import type { ReactElement, ReactNode } from 'react';

/**
 * Panel del contrato 0.3.3.
 * Canon: handoff-argos-0.3.2/COMPONENTS.md §2
 *
 * ── PARA QUÉ SIRVE, que es lo que más se copia mal ───────────────────────────
 * Para un objeto DISCRETO: un registro, un cálculo, un gráfico.
 * **Nunca para dividir una página.** Envolver secciones de una pantalla en
 * paneles es exactamente lo que devuelve el aspecto de plantilla que este
 * sistema está dejando atrás.
 *
 * ── Poca sombra, mucho borde ─────────────────────────────────────────────────
 * El panel se separa del lienzo por línea, no por elevación. `--shadow` apenas
 * lo despega. La elevación (`--shadow-lift`) se reserva para lo que de verdad
 * flota: menú, aviso, modal, cajón. Tarjetas con sombra sobre gris es la firma
 * de la plantilla que estamos dejando atrás.
 *
 * ── La acción primaria de la vista NO va acá ─────────────────────────────────
 * `acciones` es para las secundarias del objeto («Recalcular»). La primaria vive
 * en la cabecera de la página.
 */

export interface PanelProps {
  titulo?: string;
  /** QUÉ registro es. No repite el título. */
  subtitulo?: string;
  /** Secundarias, a la derecha de la cabecera. */
  acciones?: ReactNode;
  /**
   * Nivel del encabezado del título. `3` por omisión, que es lo correcto para
   * un panel que cuelga de una sección con su propio `h2`.
   *
   * Un panel colgado directo del `h1` de la página necesita `nivel={2}`: sin eso
   * el documento salta de h1 a h3, y quien recorre la página por encabezados
   * —la forma habitual de orientarse con lector de pantalla— no puede saber si
   * se perdió un nivel intermedio o si nunca existió.
   *
   * Cambia la etiqueta, **nunca el tamaño**: el nivel dice dónde encaja el panel
   * en el esquema del documento, no cuánto pesa en la pantalla. Si un `h2` se
   * viera más grande que otro, el nivel dejaría de ser gratis y nadie lo
   * pondría bien.
   */
  nivel?: 2 | 3 | 4;
  /** Sin relleno: el contenido llega al borde. Tabla, lista, estado vacío. */
  flush?: boolean;
  children: ReactNode;
}

export default function Panel({
  titulo,
  subtitulo,
  acciones,
  nivel = 3,
  flush = false,
  children,
}: PanelProps): ReactElement {
  const hayCabecera = titulo !== undefined || acciones !== undefined;
  const Encabezado = `h${nivel}` as 'h2' | 'h3' | 'h4';

  return (
    <section className="tw:min-w-0 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:shadow-loki">
      {hayCabecera ? (
        // `.panel>header`: relleno --pad-panel, gap 12px y alineado al CENTRO —
        // no arriba: con una sola línea de título el botón de acciones quedaba
        // pegado al borde superior.
        <header className="loki-pad-panel tw:flex tw:items-center tw:gap-3 tw:border-b tw:border-linea-suave">
          <div className="tw:min-w-0 tw:flex-1">
            {titulo !== undefined ? (
              // Sans 600 — la serifa se reserva a título de página y de sección;
              // en un panel rompe la lectura. La etiqueta la decide `nivel`; el
              // tamaño no cambia con ella, a propósito.
              <Encabezado className="tw:font-sans tw:text-titulo tw:font-semibold tw:text-tinta-hi">
                {titulo}
              </Encabezado>
            ) : null}
            {subtitulo !== undefined ? (
              // 11px en --text-low, no 12.5 en --text-mid: es un pie del título,
              // no un segundo título.
              <p className="tw:mt-0.5 tw:text-ayuda tw:text-tinta-low">{subtitulo}</p>
            ) : null}
          </div>
          {acciones !== undefined ? (
            <div className="tw:flex tw:shrink-0 tw:items-center tw:gap-2">{acciones}</div>
          ) : null}
        </header>
      ) : null}

      {/* `flush` no es «sin padding» a secas: es para el contenido que TIENE que
          llegar al borde para leerse bien — una tabla cuyas filas se cortarían
          con aire a los costados, una lista, un estado vacío centrado. */}
      {/* `.flush` recorta y redondea las esquinas de abajo: sin eso, una tabla a
          sangre se sale por las esquinas del panel. */}
      <div
        className={
          flush
            ? 'tw:min-w-0 tw:overflow-hidden tw:rounded-b-panel'
            : 'loki-cuerpo-panel'
        }
      >
        {children}
      </div>
    </section>
  );
}
