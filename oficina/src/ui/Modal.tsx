import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import type { ReactElement, ReactNode } from 'react';

import Boton from './Boton';

/**
 * Modal del contrato 0.3.3.
 * Traducido de `.dlg` de `referencia/argos5/ui.css`.
 *
 * ── `<dialog>` nativo, y en un portal al `body` ──────────────────────────────
 * La trampa de foco, `Esc` y el retorno del foco los da el navegador. El portal
 * evita que un ancestro con `overflow` o un contexto de apilado lo saquen del
 * top layer — que es exactamente lo que le pasó a la paleta.
 *
 * ── Tres formas y sólo tres ──────────────────────────────────────────────────
 * Confirmar · destruir · capturar. Comparten esta anatomía; lo que cambia es el
 * rótulo, la banda y si pide escribir la referencia.
 *
 * ── El destructivo pide ESCRIBIR la referencia ───────────────────────────────
 * No basta un «Sí, eliminar»: un botón rojo se aprieta por reflejo después del
 * tercero. Escribir `SOL-01293` obliga a mirar QUÉ se está anulando. Y la banda
 * dice el alcance real del daño —«se anulan también 6 comprobantes»—, no «esta
 * acción no se puede deshacer».
 */

export interface ModalProps {
  abierto: boolean;
  onCerrar(): void;
  /** Dónde estás en el flujo. En el destructivo dice «Acción irreversible». */
  rotulo?: string;
  /** Nombra la acción Y su objeto: «Aprobar solicitud SOL-01293», nunca «¿Confirmar?». */
  titulo: string;
  /** Qué pasa al aceptar y qué pasa si se deshace. */
  descripcion?: string;
  destructivo?: boolean;
  /** Referencia que hay que escribir para habilitar el destructivo. */
  confirmacion?: string;
  ancho?: 'md' | 'lg';
  /** A la derecha; el secundario ANTES que el primario. */
  acciones: ReactNode;
  children?: ReactNode;
  /**
   * Cómo se llama la salida. Por defecto «Cancelar», que es lo correcto cuando el modal pide
   * una decisión: cancelarla es no tomarla.
   *
   * Pero no todos piden una decisión. En los de consulta —los criterios de categoría, las zonas
   * internacionales, la guía— no hay nada que cancelar: se leyó y se cierra. Llamar «Cancelar»
   * a esa salida sugiere que algo quedó a medias, y la alternativa era peor: dejar esos tres en
   * el diálogo anterior sólo por una palabra.
   */
  etiquetaCerrar?: string;
}

export default function Modal({
  abierto,
  onCerrar,
  rotulo,
  titulo,
  descripcion,
  destructivo = false,
  confirmacion,
  ancho = 'md',
  acciones,
  children,
  etiquetaCerrar = 'Cancelar',
}: ModalProps): ReactElement {
  const dialogo = useRef<HTMLDialogElement>(null);
  const [escrito, setEscrito] = useState('');

  useEffect(() => {
    const d = dialogo.current;
    if (d === null) return;
    if (abierto && !d.open) {
      d.showModal();
      setEscrito('');
    } else if (!abierto && d.open) {
      d.close();
    }
  }, [abierto]);

  const bloqueado = confirmacion !== undefined && escrito.trim() !== confirmacion;

  return createPortal(
    <dialog
      ref={dialogo}
      onClose={onCerrar}
      className={[
        'loki-dlg tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:shadow-lift',
        ancho === 'lg' ? 'loki-dlg-lg' : '',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <header className="tw:px-6 tw:pt-5 tw:pb-3.5">
        {rotulo !== undefined ? (
          <span
            className={[
              'tw:mb-[7px] tw:block tw:text-cabecera tw:font-semibold tw:tracking-[.08em] tw:uppercase',
              destructivo ? 'tw:text-riesgo-fg' : 'tw:text-tinta-mid',
            ].join(' ')}
          >
            {rotulo}
          </span>
        ) : null}
        {/* Serifa 16/600: nombra la acción y su objeto. */}
        <h2 className="tw:font-serif tw:text-[16px] tw:leading-[1.3] tw:font-semibold tw:text-tinta-hi">
          {titulo}
        </h2>
        {descripcion !== undefined ? (
          <p className="tw:mt-[7px] tw:text-cuerpo-2 tw:leading-[1.55] tw:text-tinta-mid">
            {descripcion}
          </p>
        ) : null}
      </header>

      {/* Cuerpo con desplazamiento propio: el pie y la cabecera se quedan. */}
      <div className="loki-dlg-cuerpo tw:px-6 tw:pb-1 tw:text-cuerpo-2 tw:leading-[1.6]">
        {children}

        {confirmacion !== undefined ? (
          <label className="tw:mt-4 tw:flex tw:flex-col tw:gap-1.5">
            <span className="tw:text-rotulo tw:font-semibold tw:text-tinta-mid">
              Escriba <code className="tw:font-mono tw:text-riesgo-fg">{confirmacion}</code> para
              confirmar
            </span>
            <input
              value={escrito}
              onChange={(e) => setEscrito(e.target.value)}
              autoComplete="off"
              className="loki-dlg-confirmacion tw:rounded-control tw:border tw:border-linea-campo tw:bg-panel tw:px-2.5 tw:font-mono tw:text-cuerpo tw:text-tinta-base"
            />
          </label>
        ) : null}
      </div>

      {/* Acciones a la derecha, secundario antes que primario. Nunca centradas
          ni a lo ancho: el ojo termina de leer a la derecha y ahí decide. */}
      <footer className="tw:flex tw:items-center tw:gap-2 tw:px-6 tw:pt-4 tw:pb-5">
        <span className="tw:flex-1" />
        <Boton variante="fantasma" onClick={onCerrar}>
          {etiquetaCerrar}
        </Boton>
        {/* `bloqueado` viaja al consumidor por el fieldset: así el modal decide
            si la confirmación está completa sin conocer la acción. */}
        <fieldset disabled={bloqueado} className="tw:flex tw:items-center tw:gap-2 tw:border-0">
          {acciones}
        </fieldset>
      </footer>
    </dialog>,
    document.body,
  );
}
