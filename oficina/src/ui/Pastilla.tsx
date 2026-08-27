import type { ReactElement, ReactNode } from 'react';

import { CLASE_TONO } from './tipos';
import type { Tono } from './tipos';

/**
 * Pastilla de estado del contrato 0.3.3.
 * Canon: handoff-argos-0.3.2/COMPONENTS.md §2
 *
 * ── `children` es OBLIGATORIO, y no por comodidad ────────────────────────────
 * El color nunca viaja solo. Una pastilla sin texto obliga al usuario a aprender
 * un código de colores que nadie le enseñó, y deja de funcionar en daltonismo,
 * en un monitor mal calibrado y en una fotocopia — que en este sistema es un
 * destino real: el tema `gris` existe para imprimir.
 *
 * ── No envuelve nunca ────────────────────────────────────────────────────────
 * `whitespace-nowrap` es del contrato: la etapa más larga del flujo
 * («Procesamiento Paralelo 6a — Viáticos») tiene que caber en su columna sin
 * partirse en dos líneas y descuadrar el alto de la fila.
 *
 * ── El tono se decide por identificador, jamás por el texto ──────────────────
 * Quien use esto debe resolver el tono con `ESTADO_INTERNO[id]` o
 * `ESTADO_CLIENTE[id]`, nunca comparando cadenas: «Por aprobar» contiene
 * «aprob» y una comparación la pintaría de aprobada.
 */

export interface PastillaProps {
  tono: Tono;
  /** OBLIGATORIO: el color nunca viaja solo. */
  children: ReactNode;
  /**
   * El punto marca un ESTADO. `punto={false}` es un rótulo — y la diferencia no
   * es de gusto (decisión 2026-08-12).
   *
   * ── Qué hace el punto, de verdad ────────────────────────────────────────────
   * No es una ayuda de accesibilidad: un punto teñido no distingue mejor que un
   * texto teñido, y quien no separa los colores sigue leyendo el texto — que por
   * eso es obligatorio. Lo que hace es una señal de GÉNERO: dice «esto reporta
   * una condición viva del objeto», igual que un testigo encendido. Ponerlo en
   * un rótulo lo gasta, y entonces deja de decir nada en los dos lados.
   *
   * ── Cómo se decide, sin discutirlo pantalla por pantalla ────────────────────
   * Es ESTADO (con punto) si se cumple una:
   *   · el tono **se calcula del dato** — el color cambia con la condición;
   *   · el tono tiene **valencia** (`ok` · `aviso` · `riesgo`) — nombrar algo
   *     bueno o malo sólo tiene sentido sobre una condición;
   *   · el texto nombra una condición aunque el tono sea neutro: «Guardando…»,
   *     «Sin cambios», «Tiene cuenta».
   * Es RÓTULO (sin punto) el resto: una propiedad que no cambia («Internacional»,
   * «Conductor», «Tránsito»), un código o nombre, una zona, una categoría, un
   * conteo («31 personas», «3 unidades»).
   *
   * Al 2026-08-12: 165 con punto, 82 sin.
   */
  punto?: boolean;
}

export default function Pastilla({ tono, children, punto = true }: PastillaProps): ReactElement {
  return (
    <span
      className={[
        // `.pill` de ui.css: gap 6px, relleno 3px 8px, 11px/500 con interlínea 1.35.
        CLASE_TONO[tono],
        'tw:inline-flex tw:items-center tw:gap-1.5',
        'tw:rounded-badge tw:border tw:border-transparent tw:px-2 tw:py-[3px]',
        // La familia se DECLARA, no se hereda. El contrato fija la pastilla en sans, y una
        // celda marcada como numérica impone monoespaciada a todo su contenido: ahí la
        // pastilla salía en otra tipografía que su vecina de la columna de al lado. Se vio
        // en «Solicitudes de Mi Equipo», con la del plazo en mono y la del estado en sans.
        'tw:font-sans tw:text-pastilla tw:leading-[1.35] tw:font-medium tw:whitespace-nowrap',
      ].join(' ')}
    >
      {punto ? (
        /* El punto hereda el color del texto: un tono nuevo no necesita una regla
           nueva, que es la condición para que agregar un séptimo tema sea gratis. */
        <span className="loki-punto" aria-hidden="true" />
      ) : null}
      {children}
    </span>
  );
}
