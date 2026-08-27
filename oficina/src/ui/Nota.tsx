import type { ReactElement, ReactNode } from 'react';

import { CLASE_TONO } from './tipos';
import type { Tono } from './tipos';

/**
 * El aviso corto teñido, pegado a lo que explica.
 *
 * ── El hueco que llena, medido ──────────────────────────────────────────────
 * El contrato tenía `Banda` —el aviso de la página, ancho, con radio de panel— y `Pastilla`
 * —la etiqueta de estado—, pero **nada para el renglón teñido que acompaña a un dato**. Así
 * que cada pantalla lo escribía a mano componiendo `CLASE_TONO[tono]` con su propia caja:
 * **71 usos en 32 archivos, y 24 formas distintas de la misma idea**. Treinta y una eran
 * idénticas al carácter.
 *
 * ── Qué NO es ───────────────────────────────────────────────────────────────
 * **No reemplaza a `Banda`.** Aquélla es más grande —radio de panel, 14 px de relleno,
 * admite una acción— y ocupa el ancho de la página: es el aviso *de la pantalla*. Ésta es el
 * aviso *de un dato*: radio de control, 12 px, y va donde el dato está. Confundirlas se ve
 * enseguida — una banda metida dentro de una celda se lee como un error de maquetado.
 *
 * **No reemplaza a `Pastilla`.** De los 71 usos, unos veinte tenían forma de píldora
 * (`rounded-full`, `rounded-badge`): ésos no son notas mal escritas, son **pastillas** mal
 * escritas, y al migrarlos van a `Pastilla`.
 *
 * ── El ícono cambia la caja, y por eso es una prop ──────────────────────────
 * Doce de los usos ponían el ícono a la izquierda, y para eso la caja tiene que pasar a
 * `flex items-start`. Dejarlo librado a que cada quien agregue las clases es cómo se llega a
 * un ícono centrado en unos sitios y alineado arriba en otros. Con la prop, la decisión
 * está tomada una vez: **arriba**, porque el texto casi siempre ocupa más de un renglón.
 */
export interface NotaProps {
  tono: Tono;
  /** Ya construido, a 14 px: `<TriangleAlert className="tw:size-3.5" />`. */
  icono?: ReactNode;
  children: ReactNode;
}

export default function Nota({ tono, icono, children }: NotaProps): ReactElement {
  return (
    <p
      className={[
        CLASE_TONO[tono],
        'tw:rounded-control tw:px-3 tw:py-2 tw:text-xs',
        // Con ícono, la caja alinea ARRIBA: el texto de una nota casi siempre pasa de un
        // renglón, y centrado el ícono queda flotando a media altura.
        icono !== undefined ? 'tw:flex tw:items-start tw:gap-2' : '',
      ].join(' ')}
    >
      {icono !== undefined ? (
        <span className="tw:mt-0.5 tw:shrink-0" aria-hidden="true">
          {icono}
        </span>
      ) : null}
      {icono !== undefined ? <span className="tw:min-w-0">{children}</span> : children}
    </p>
  );
}
