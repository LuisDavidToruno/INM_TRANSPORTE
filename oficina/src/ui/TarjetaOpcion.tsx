import { ArrowRight } from 'lucide-react';
import type { ReactElement, ReactNode } from 'react';

import TileIcono from './TileIcono';
import { CLASE_TONO } from './tipos';
import type { Tono } from './tipos';

/**
 * Tarjeta de opción: una superficie con forma de panel que **es un botón**.
 * Para elegir por dónde empezar — el tipo de gira, cuál propuesta abrir.
 *
 * ── Por qué no es un `Panel` ────────────────────────────────────────────────
 * El canon es explícito: el Panel es para un objeto DISCRETO —un registro, un
 * cálculo, un gráfico— y la acción primaria de la vista NO vive adentro suyo.
 * Un lanzador no es ninguna de las dos cosas: no muestra un objeto, ES la
 * acción. Por eso tiene pieza propia en vez de un `interactiva` colgado del
 * Panel, que habría vuelto ambiguo qué significa un panel (2026-08-12).
 *
 * ── Es un `button`, no un div con `onClick` ─────────────────────────────────
 * Se alcanza con el tabulador, se anuncia como acción, responde al Enter y se
 * deshabilita de una sola forma mientras la acción corre. Una tarjeta
 * clickeable no hace nada de eso, y acá el clic es todo el punto de la
 * pantalla.
 *
 * ── El borde reacciona; la superficie no se mueve ───────────────────────────
 * Al pasar por encima cambia la línea, no la elevación: el sistema se separa
 * del lienzo por borde y no por sombra, y una tarjeta que levita al pasar el
 * puntero es la firma de la plantilla que estamos dejando atrás.
 */

export interface TarjetaOpcionProps {
  /** Ícono ya como elemento, para que la pieza no dependa de la librería. */
  icono: ReactNode;
  titulo: string;
  /** Marca junto al título — «Propuesta», «Nuevo». Opcional. */
  insignia?: string;
  /** Qué es, en una línea. */
  subtitulo: string;
  descripcion: string;
  /** Nota al pie, a la izquierda: de dónde viene o cuánto cuesta. */
  pie: string;
  /** Lo que dice el botón. En marcha se reemplaza por `textoOcupado`. */
  etiquetaAccion: string;
  textoOcupado?: string;
  tono: Tono;
  onElegir: () => void;
  ocupado?: boolean;
}

export default function TarjetaOpcion({
  icono,
  titulo,
  insignia,
  subtitulo,
  descripcion,
  pie,
  etiquetaAccion,
  textoOcupado = 'Creando…',
  tono,
  onElegir,
  ocupado = false,
}: TarjetaOpcionProps): ReactElement {
  return (
    <button
      type="button"
      onClick={onElegir}
      disabled={ocupado}
      className="tw:flex tw:h-full tw:w-full tw:min-w-0 tw:flex-col tw:gap-3 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:p-5 tw:text-left tw:shadow-loki tw:transition-colors tw:duration-150 tw:ease-loki tw:hover:border-linea-activa tw:disabled:opacity-60"
    >
      <div className="tw:flex tw:items-center tw:gap-3">
        <TileIcono tono={tono} tamano="lg">
          {icono}
        </TileIcono>
        <div className="tw:min-w-0">
          <span className="tw:flex tw:flex-wrap tw:items-center tw:gap-2">
            <h2 className="tw:text-base tw:font-semibold tw:text-tinta-hi">{titulo}</h2>
            {insignia !== undefined ? (
              <span
                className={`tw:rounded-badge tw:px-2 tw:py-0.5 tw:text-xs tw:font-semibold ${CLASE_TONO[tono]}`}
              >
                {insignia}
              </span>
            ) : null}
          </span>
          <p className="tw:text-ayuda tw:text-tinta-low">{subtitulo}</p>
        </div>
      </div>

      <p className="tw:text-sm tw:text-tinta-mid">{descripcion}</p>

      {/* `mt-auto` empuja el pie abajo: en una fila de opciones de alto distinto,
          los botones quedan alineados entre sí y no flotando a media tarjeta. */}
      <div className="tw:mt-auto tw:flex tw:items-center tw:justify-between tw:border-t tw:border-linea-suave tw:pt-3">
        <span className="tw:text-ayuda tw:text-tinta-low">{pie}</span>
        <span
          className={`tw:inline-flex tw:items-center tw:gap-1.5 tw:rounded-control tw:px-3 tw:py-1.5 tw:text-xs tw:font-semibold ${CLASE_TONO[tono]}`}
        >
          {ocupado ? textoOcupado : etiquetaAccion}
          {!ocupado && <ArrowRight className="tw:size-3.5" aria-hidden="true" />}
        </span>
      </div>
    </button>
  );
}
