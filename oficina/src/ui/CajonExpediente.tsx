import { X } from 'lucide-react';
import { useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';
import type { ReactElement, ReactNode } from 'react';

/**
 * Cajón del expediente — contrato 0.3.3.
 *
 * ── Por qué NO es un modal ───────────────────────────────────────────────────
 * Es la distinción que el contrato marca explícitamente, y no es cosmética. Un
 * modal INTERRUMPE: bloquea la pantalla y obliga a resolverlo antes de seguir. El
 * expediente se consulta **mientras se trabaja la fila** — el oficial mira el
 * desglose con la bandeja todavía a la vista, decide, y actúa. Metido en un modal,
 * cada consulta obliga a cerrar y volver a buscar la fila.
 *
 * Por eso son 512 px y no pantalla completa: lo que queda a la izquierda tiene que
 * seguir siendo legible. Un cajón que tapa todo es un modal con otro nombre.
 *
 * ── Por qué `<dialog>` igual ─────────────────────────────────────────────────
 * Aunque conceptualmente no sea un diálogo, usa el elemento nativo por lo que trae
 * gratis y es difícil de replicar bien: **trampa de foco, cierre con Esc y retorno
 * del foco al elemento que lo abrió**. Escribir eso a mano es de donde salen los
 * paneles de los que no se puede salir con el teclado.
 *
 * El portal al `body` evita que un ancestro con `overflow` o un contexto de
 * apilado lo saque del top layer — lo mismo que se resolvió en `Modal`.
 *
 * Anatomía y animación en `estilos/index.css` → `.loki-cajon-exp`.
 */

export interface CajonExpedienteProps {
  readonly abierto: boolean;
  /** Referencia del expediente. Va sobre el título, en versalitas. */
  readonly referencia: string | null;
  readonly titulo: string;
  readonly onCerrar: () => void;
  readonly children: ReactNode;
  /** Acciones del pie, a la derecha. El secundario ANTES que el primario. */
  readonly acciones?: ReactNode;
}

export default function CajonExpediente({
  abierto,
  referencia,
  titulo,
  onCerrar,
  children,
  acciones,
}: CajonExpedienteProps): ReactElement {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const el = ref.current;
    if (el === null) return;
    if (abierto && !el.open) el.showModal();
    else if (!abierto && el.open) el.close();
  }, [abierto]);

  return createPortal(
    <dialog
      ref={ref}
      // `close` cubre TODAS las formas de cerrar, incluida la tecla Esc que maneja
      // el navegador. Sin esto React no se entera de que se cerró: el estado sigue
      // diciendo «abierto» y el cajón no vuelve a abrirse nunca.
      onClose={onCerrar}
      // El velo cierra al tocarlo. Sólo cuando el clic cae en el propio `<dialog>`
      // —que es el área del velo— y no en algo de adentro, o cerraría al soltar el
      // puntero tras seleccionar texto.
      onClick={(e) => {
        if (e.target === ref.current) onCerrar();
      }}
      aria-label={titulo}
      className="loki-cajon-exp tw:border-l tw:border-linea tw:bg-panel"
    >
      <div className="tw:flex tw:h-full tw:flex-col">
        <header className="tw:flex tw:shrink-0 tw:items-start tw:gap-3 tw:border-b tw:border-linea tw:px-5 tw:py-3.5">
          <div className="tw:min-w-0 tw:flex-1">
            {referencia !== null ? (
              <p className="tw:mb-0.5 tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:text-tinta-mid tw:uppercase">
                Expediente{' '}
                <span className="tw:font-mono tw:text-btn-fg tw:normal-case">{referencia}</span>
              </p>
            ) : null}
            <h2 className="tw:font-serif tw:text-[16px] tw:leading-[1.3] tw:font-semibold tw:text-tinta-hi">
              {titulo}
            </h2>
          </div>
          <button
            type="button"
            onClick={onCerrar}
            aria-label="Cerrar el expediente"
            className="loki-cajon-exp-x tw:rounded-control tw:p-1 tw:text-tinta-mid"
          >
            <X size={18} strokeWidth={1.8} aria-hidden="true" />
          </button>
        </header>

        <div className="loki-scroll tw:min-h-0 tw:flex-1 tw:overflow-y-auto tw:px-5 tw:py-4 tw:text-cuerpo-2">
          {children}
        </div>

        {acciones !== undefined ? (
          <footer className="tw:flex tw:shrink-0 tw:items-center tw:gap-2 tw:border-t tw:border-linea tw:bg-canvas tw:px-5 tw:py-3">
            <span className="tw:flex-1" />
            {acciones}
          </footer>
        ) : null}
      </div>
    </dialog>,
    document.body,
  );
}
