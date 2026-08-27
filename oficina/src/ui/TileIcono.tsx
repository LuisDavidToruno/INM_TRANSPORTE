import type { ReactElement, ReactNode } from 'react';

import { CLASE_TONO } from './tipos';
import type { Tono } from './tipos';

/**
 * Cuadro de ícono teñido. Canon: COMPONENTS.md §2.
 * Acompaña a un estado vacío o encabeza una tarjeta; no es un botón.
 */
export interface TileIconoProps {
  tono: Tono;
  tamano?: 'md' | 'lg';
  children: ReactNode;
}

export default function TileIcono({ tono, tamano = 'md', children }: TileIconoProps): ReactElement {
  return (
    <span
      className={[
        CLASE_TONO[tono],
        tamano === 'md' ? 'loki-tile' : 'loki-tile-lg',
        'tw:inline-flex tw:items-center tw:justify-center tw:rounded-badge tw:border',
      ].join(' ')}
      aria-hidden="true"
    >
      {children}
    </span>
  );
}
