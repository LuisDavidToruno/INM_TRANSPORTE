import { cloneElement, isValidElement, useId } from 'react';
import type { ReactElement, ReactNode } from 'react';

/**
 * Campo de formulario del contrato 0.3.3.
 * Canon: handoff-argos-0.3.2/COMPONENTS.md §2
 *
 * ── El error dice la CAUSA, no «campo requerido» ─────────────────────────────
 * «Sin una fecha posterior al inicio no se puede calcular la duración de la
 * gira», no «Campo inválido». Un mensaje genérico obliga al usuario a probar
 * formatos hasta que uno pasa.
 *
 * `zod` valida la forma; el servidor valida la regla, y su mensaje aterriza en
 * ESTE campo — no en un cartel genérico arriba del formulario.
 *
 * ── La ayuda dice qué EFECTO tiene el dato ───────────────────────────────────
 * «Determina la tarifa diaria junto con la categoría», no «Ingrese la zona».
 * Repetir la etiqueta en la ayuda gasta una línea y no informa nada.
 *
 * ── Se marca lo OBLIGATORIO, nunca lo opcional ───────────────────────────────
 * En este sistema casi todo lo es, así que marcar lo opcional sería marcar casi
 * nada y el usuario no sabría dónde mirar.
 */

/**
 * Lo que `Campo` le pone al control para que la etiqueta y el error lo alcancen.
 * Sólo hace falta nombrarlo cuando se usa la forma de función (ver `children`).
 */
export interface PropsDelControl {
  id: string;
  'aria-describedby': string | undefined;
  'aria-invalid': true | undefined;
}

export interface CampoProps {
  etiqueta: string;
  obligatorio?: boolean;
  /** Qué efecto tiene el dato. No repite la etiqueta. */
  ayuda?: string;
  /** La CAUSA, no «campo requerido». */
  error?: string;
  /** Códigos e importes: todo lo que se compara en columna. */
  mono?: boolean;
  /**
   * El control: input, select, textarea.
   *
   * <b>Dos formas.</b> Lo normal es pasar el control directamente —`Campo` le
   * inyecta el `id` y los `aria-*` por su cuenta:
   *
   * ```tsx
   * <Campo etiqueta="Usuario"><input name="usuario" /></Campo>
   * ```
   *
   * Cuando el control NO es el hijo directo —porque lleva un envoltorio, o
   * porque es un componente compuesto— se pasa una <b>función</b> y se colocan
   * esas propiedades donde de verdad va el control:
   *
   * ```tsx
   * <Campo etiqueta="Contraseña">
   *   {(p) => (
   *     <span className="relative">
   *       <input {...p} type="password" />
   *       <button type="button">ver</button>
   *     </span>
   *   )}
   * </Campo>
   * ```
   *
   * La forma de función existe porque inyectar a ciegas en un envoltorio pone el
   * `id` en el `<span>` y deja el `htmlFor` de la etiqueta apuntando a algo que
   * no es un control: se ve arreglado y no lo está.
   */
  children: ReactNode | ((props: PropsDelControl) => ReactNode);
}

export default function Campo({
  etiqueta,
  obligatorio = false,
  ayuda,
  error,
  mono = false,
  children,
}: CampoProps): ReactElement {
  const idControl = useId();
  const idAyuda = useId();
  const idError = useId();

  const hayError = error !== undefined && error !== '';
  const hayAyuda = ayuda !== undefined && ayuda !== '';

  /**
   * El error DESPLAZA a la ayuda —no se acumulan— así que sólo uno de los dos
   * existe en el DOM y `aria-describedby` apunta a uno solo, nunca a los dos.
   */
  const idDescripcion = hayError ? idError : hayAyuda ? idAyuda : undefined;

  const propsDelControl: PropsDelControl = {
    id: idControl,
    'aria-describedby': idDescripcion,
    'aria-invalid': hayError ? true : undefined,
  };

  /**
   * Se resuelve el control y se averigua si el `htmlFor` va a apuntar a algo.
   *
   * Un `htmlFor` colgado —apuntando a un `id` que nadie tiene— es peor que no
   * ponerlo: el marcado se lee como si el campo estuviera asociado y no lo está.
   * Por eso, cuando no se puede garantizar la asociación, se omite el `htmlFor`
   * y se avisa en desarrollo en vez de dejarlo mintiendo.
   */
  let control: ReactNode;
  /** `undefined` = no hay a qué apuntar, así que la etiqueta va sin `htmlFor`. */
  let idParaEtiqueta: string | undefined;

  if (typeof children === 'function') {
    // La función coloca las propiedades donde va el control de verdad.
    control = children(propsDelControl);
    idParaEtiqueta = idControl;
  } else if (isValidElement<Record<string, unknown>>(children)) {
    // Si el hijo ya trae `id` propio, manda el suyo: se respeta y la etiqueta
    // apunta ahí. Pisarlo rompería cualquier referencia externa a ese control.
    const idPropio = children.props['id'];
    const idFinal = typeof idPropio === 'string' && idPropio !== '' ? idPropio : idControl;
    control = cloneElement(children, { ...propsDelControl, id: idFinal });
    idParaEtiqueta = idFinal;

    if (import.meta.env.DEV && typeof children.type !== 'string') {
      // Un componente propio recibe estas propiedades como props; si no las
      // reenvía a su control, la asociación no ocurre y nadie se entera.
      console.warn(
        `[LOKI] Campo «${etiqueta}»: el hijo es un componente, no un control nativo. ` +
          'Si no reenvía id/aria-* a su input, use la forma de función de `children`.',
      );
    }
  } else {
    // Fragmento, arreglo o texto: no hay a quién ponerle el `id`.
    control = children;
    idParaEtiqueta = undefined;

    if (import.meta.env.DEV) {
      console.warn(
        `[LOKI] Campo «${etiqueta}»: children no es un elemento único, así que la etiqueta ` +
          'no se puede asociar. Use la forma de función de `children`.',
      );
    }
  }

  return (
    <div
      className={[
        // Sin `gap`: las separaciones las ponen el rótulo (6px) y la ayuda (5px),
        // que son distintas. Un gap uniforme las igualaría y el rótulo quedaría
        // tan lejos de su campo como la ayuda.
        'loki-campo tw:flex tw:flex-col',
        mono ? 'loki-campo-mono' : '',
        hayError ? 'loki-campo-error' : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      {/* Sans, no versalitas: el campo se lee de corrido con su dato. */}
      {/* `.field label`: 11.5/600, 6px de separación con el control. */}
      <label
        // El `htmlFor` sólo va cuando se sabe que hay un control con ese `id`.
        // Colgado sería peor que ausente: parecería asociado sin estarlo.
        htmlFor={idParaEtiqueta}
        className="tw:mb-1.5 tw:block tw:text-rotulo tw:leading-none tw:font-semibold tw:text-tinta-mid"
      >
        {etiqueta}
        {obligatorio ? (
          <span className="tw:text-riesgo-fg" aria-hidden="true">
            {' *'}
          </span>
        ) : null}
      </label>

      {control}

      {/* El error DESPLAZA a la ayuda en vez de acumularse: dos líneas de texto
          bajo un control compiten entre sí, y la que importa cuando hay error es
          la que dice por qué. */}
      {hayError ? (
        <p id={idError} role="alert" className="tw:mt-[5px] tw:flex tw:items-center tw:gap-[5px] tw:text-ayuda tw:text-riesgo-fg">
          <span className="loki-icono-error" aria-hidden="true">
            <svg viewBox="0 0 16 16" fill="none">
              <circle cx="8" cy="8" r="6.5" stroke="currentColor" strokeWidth="1.6" />
              <path d="M8 4.8v3.6M8 11h.01" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
            </svg>
          </span>
          {error}
        </p>
      ) : hayAyuda ? (
        <p id={idAyuda} className="tw:mt-[5px] tw:text-ayuda tw:text-tinta-low">
          {ayuda}
        </p>
      ) : null}
    </div>
  );
}
