import { EllipsisVertical } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import type { ReactElement, ReactNode } from 'react';

/**
 * Menú de acciones secundarias de una fila. Contrato 0.3.3.
 *
 * ── Por qué existe ───────────────────────────────────────────────────────────
 * Una bandeja revisora ofrece hasta SEIS acciones sobre la misma fila
 * (Presupuesto: aprobar · corregir · rechazar · devolver · cancelar sin presupuesto ·
 * expediente). Dibujarlas todas en línea empuja la tabla a 1771 px cuando el visor
 * tiene 1390 —medido— y las últimas quedan detrás del desplazamiento horizontal:
 * presentes en el DOM, invisibles en la práctica.
 *
 * La bandeja de Razor resuelve esto desde hace tiempo con el mismo reparto que acá:
 * **la acción principal en línea y el resto en un kebab**. No es un adorno; es lo que
 * permite que la columna de acciones quepa.
 *
 * ── Va en un PORTAL, y no es opcional ────────────────────────────────────────
 * El visor de la tabla lleva `overflow: auto` —lo necesita para su desplazamiento
 * horizontal—, así que un panel posicionado dentro de él se **recorta**: se midió
 * el menú ocupando de 460 a 610 px contra un visor que termina en 462. Es el mismo
 * defecto que la nota de `Tabla` advierte y el que ya se había cobrado a la paleta
 * de comandos. Con el portal al `body` y posición fija, el menú vive fuera de ese
 * contexto y se dibuja entero.
 *
 * Y por eso mismo se cierra al desplazar o redimensionar: al estar anclado a
 * coordenadas de pantalla, si la fila se mueve el panel se quedaría flotando en el
 * lugar equivocado.
 *
 * ── Lo que NO hace ───────────────────────────────────────────────────────────
 * No decide qué acciones hay ni cuál es la principal: recibe las que le pasan, en el
 * orden en que se las pasan. Esa decisión es del servidor (canon §1 A9), y meterla acá
 * sería reimplementar la tabla de la escalera bidireccional.
 */

export interface MenuAccionesProps {
  /** Rótulo accesible: incluye el objeto («Más acciones de la solicitud 1274»). */
  etiqueta: string;
  children: ReactNode;
}

/** Ancho del panel. Se necesita ANTES de medirlo para poder alinearlo a la derecha. */
const ANCHO = 190;

export default function MenuAcciones({ etiqueta, children }: MenuAccionesProps): ReactElement {
  const [pos, setPos] = useState<{ top: number; left: number } | null>(null);
  const disparador = useRef<HTMLButtonElement>(null);
  const panel = useRef<HTMLDivElement>(null);

  const abierto = pos !== null;

  function alternar(): void {
    if (abierto) {
      setPos(null);
      return;
    }
    const r = disparador.current?.getBoundingClientRect();
    if (r === undefined) return;
    // Alineado a la derecha del disparador, y sujeto al borde de la ventana para que
    // el último botón de la fila no lo empuje fuera de la pantalla.
    setPos({ top: r.bottom + 4, left: Math.min(r.right - ANCHO, window.innerWidth - ANCHO - 8) });
  }

  useEffect(() => {
    if (!abierto) return;

    const cerrar = (): void => setPos(null);
    const alTeclear = (e: KeyboardEvent): void => {
      if (e.key === 'Escape') cerrar();
    };
    const alClicar = (e: MouseEvent): void => {
      const t = e.target as Node;
      if (panel.current?.contains(t) === true) return;
      if (disparador.current?.contains(t) === true) return;
      cerrar();
    };

    document.addEventListener('keydown', alTeclear);
    document.addEventListener('mousedown', alClicar);
    // `capture` para enterarse también del desplazamiento de la tabla, que no burbujea.
    window.addEventListener('scroll', cerrar, true);
    window.addEventListener('resize', cerrar);
    return () => {
      document.removeEventListener('keydown', alTeclear);
      document.removeEventListener('mousedown', alClicar);
      window.removeEventListener('scroll', cerrar, true);
      window.removeEventListener('resize', cerrar);
    };
  }, [abierto]);

  return (
    <>
      <button
        ref={disparador}
        type="button"
        aria-label={etiqueta}
        aria-expanded={abierto}
        aria-haspopup="menu"
        onClick={alternar}
        className="loki-foco loki-control-sm tw:flex tw:items-center tw:justify-center tw:rounded-control tw:border tw:border-linea tw:px-1.5 tw:text-tinta-mid tw:hover:text-tinta-hi"
      >
        <EllipsisVertical size={14} aria-hidden="true" />
      </button>

      {pos !== null
        ? createPortal(
            /* Se cierra al elegir: la acción abre un diálogo de confirmación encima, y
               dejar el menú abierto detrás lo deja asomando al cerrar el diálogo. */
            <div
              ref={panel}
              role="menu"
              onClick={() => setPos(null)}
              style={{ top: pos.top, left: pos.left, width: ANCHO }}
              className="loki-flotante tw:fixed tw:z-50 tw:flex tw:flex-col tw:gap-1 tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:p-1"
            >
              {children}
            </div>,
            document.body,
          )
        : null}
    </>
  );
}
