import type { ComponentType, ReactElement } from 'react';

/**
 * Lo que puede identificar a una opción.
 *
 * Incluye `boolean` porque hay cortes que de verdad son un sí/no —«Activas» y
 * «Archivadas» son `archivadas: false | true`— y obligarlos a inventar cadenas
 * sólo para entrar acá les agregaría una traducción en los dos sentidos.
 */
export type ValorSegmentado = string | number | boolean;

/**
 * Una opción del conmutador. Todo lo que acompaña al rótulo es opcional porque
 * cada pantalla usa lo suyo: unas llevan cuenta, otra lleva ícono, otra nada.
 */
export type OpcionSegmentada<T extends ValorSegmentado> = {
  readonly valor: T;
  readonly etiqueta: string;
  /**
   * Lo que acompaña al rótulo en letra chica.
   *
   * Se llama `nota` y no `cuenta` a propósito: casi siempre es un número —«3»
   * pendientes— pero el selector de densidad pone ahí la altura de fila («44px»).
   * Nombrarlo `cuenta` habría obligado a la siguiente pantalla a mentir.
   */
  readonly nota?: string | number | null;
  readonly icono?: ComponentType<{ className?: string; 'aria-hidden'?: 'true' }>;
  /** Texto del `title`, para lo que no cabe en el rótulo. */
  readonly ayuda?: string;
};

/**
 * Conmutador segmentado: un grupo de opciones excluyentes, la activa hundida.
 *
 * **Por qué existe.** Estaba copiado en 15 pantallas, y por eso el arreglo de
 * abajo costaba 15 ediciones en vez de una. Ese es el motivo de fondo: la
 * próxima corrección se aplicaría a 14 y nadie notaría la que faltó.
 *
 * **El arreglo que trajo la extracción.** El rótulo y su nota se anunciaban
 * pegados —«Asignadas a mí1»— porque la separación que ve el ojo la pone un
 * `gap` del CSS, y el CSS no existe en la capa de texto. El separador `sr-only`
 * la pone donde sí cuenta. Va fuera del flujo (`position:absolute`), así que no
 * mueve un píxel.
 *
 * **Es excluyente, y eso decide cuándo usarlo.** Filtros que se COMBINAN entre sí
 * no van acá: metidos en un segmento sugieren que elegir uno apaga el otro.
 */
export default function Segmentado<T extends ValorSegmentado>({
  etiqueta,
  valor,
  onCambio,
  opciones,
  className,
}: {
  /** Nombre del grupo para quien no ve la pantalla: «Listas de la bandeja», «Vista». */
  readonly etiqueta: string;
  readonly valor: T;
  readonly onCambio: (valor: T) => void;
  readonly opciones: readonly OpcionSegmentada<T>[];
  readonly className?: string;
}): ReactElement {
  return (
    <div
      role="group"
      aria-label={etiqueta}
      className={[
        'tw:inline-flex tw:w-fit tw:max-w-full tw:flex-wrap tw:rounded-control tw:border tw:border-linea tw:bg-inset tw:p-0.5',
        className ?? '',
      ].join(' ').trim()}
    >
      {opciones.map((o) => {
        const activa = o.valor === valor;
        const Icono = o.icono;
        const tieneNota = o.nota !== undefined && o.nota !== null;

        return (
          <button
            // Hay pantallas donde el valor de «todas» es la cadena vacía, y una
            // clave vacía no distingue nada si mañana aparece otra.
            key={o.valor === '' ? '(sin filtro)' : String(o.valor)}
            type="button"
            aria-pressed={activa}
            onClick={() => onCambio(o.valor)}
            {...(o.ayuda ? { title: o.ayuda } : {})}
            className={[
              'loki-seg-btn loki-foco tw:flex tw:items-center tw:gap-[7px] tw:px-3 tw:text-cuerpo-2',
              activa
                ? 'tw:bg-panel tw:font-semibold tw:text-tinta-hi tw:shadow-loki'
                : 'tw:font-medium tw:text-tinta-mid',
            ].join(' ')}
          >
            {Icono && <Icono className="tw:size-3" aria-hidden="true" />}
            {o.etiqueta}
            {tieneNota && (
              <>
                <span className="tw:sr-only">: </span>
                <i className="tw:font-mono tw:text-[10.5px] tw:not-italic tw:opacity-70">
                  {o.nota}
                </i>
              </>
            )}
          </button>
        );
      })}
    </div>
  );
}
