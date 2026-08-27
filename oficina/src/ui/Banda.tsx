import type { ReactElement, ReactNode } from 'react';

import { CLASE_TONO } from './tipos';
import type { Tono } from './tipos';

/**
 * Banda de aviso dentro de la página. Canon: COMPONENTS.md §2.
 *
 * NO es el aviso de esquina (`avisar`): esto queda en el flujo del documento y
 * comunica una condición persistente de la vista — «7 solicitudes pasaron su
 * plazo» —, no el resultado de una acción que acaba de ocurrir.
 *
 * Por eso lleva su propia acción: la condición casi siempre tiene un camino para
 * resolverse, y esconderlo obliga a buscarlo.
 */
export interface BandaProps {
  tono?: Tono;
  accion?: { texto: string; onClick(): void };
  children: ReactNode;
}

export default function Banda({ tono = 'info', accion, children }: BandaProps): ReactElement {
  return (
    <div
      className={[
        CLASE_TONO[tono],
        // `.banda`: gap 11px, relleno 11px 14px, radio de PANEL (no de control)
        // y el texto ocupa el resto. La acción va subrayada, no como botón: es
        // una salida del aviso, no una acción de la página.
        'tw:flex tw:items-center tw:gap-[11px]',
        'tw:rounded-panel tw:border tw:px-3.5 tw:py-[11px] tw:text-cuerpo-2 tw:leading-[1.45]',
      ].join(' ')}
    >
      <p className="tw:min-w-0">{children}</p>
      {accion !== undefined ? (
        <button
          type="button"
          onClick={accion.onClick}
          className="loki-foco tw:shrink-0 tw:rounded-control tw:px-1 tw:text-cuerpo-2 tw:font-semibold tw:underline tw:underline-offset-2"
        >
          {accion.texto}
        </button>
      ) : null}
    </div>
  );
}
