import { useEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import type { ReactElement } from 'react';

import { normalizarTexto } from './texto';
import CampoBusqueda from './CampoBusqueda';

/**
 * Paleta de comandos del contrato 0.3.3.
 * Canon: COMPONENTS.md §3
 *
 * ── Para quién es ────────────────────────────────────────────────────────────
 * Para quien procesa 20+ solicitudes al día vale más que cualquier menú: llegar
 * a una pantalla con tres teclas en vez de con tres clics, cientos de veces.
 *
 * ── `<dialog>` nativo, igual que el modal ────────────────────────────────────
 * La trampa de foco, `Esc` y el retorno del foco al cerrar los da el navegador.
 * Reimplementarlos a mano es la forma más común de dejar una paleta que atrapa
 * el teclado y no lo devuelve.
 *
 * ── Y va en un PORTAL al `body`, no donde se declara ─────────────────────────
 * Montada dentro del shell apareció **sin velo y con el contenido dibujándose
 * encima** — o sea, fuera del top layer, que es justo lo que `showModal()` da.
 * En una reproducción aislada con la misma anidación el diálogo SÍ entraba al
 * top layer, así que la causa está en el montaje y no en la estructura CSS.
 *
 * El portal lo resuelve por construcción en vez de por diagnóstico: colgado del
 * `body` no hay ancestro que pueda recortarlo ni crear un contexto de apilado
 * que lo atrape. Es además lo que se hace normalmente con un modal, así que no
 * es un parche: es dónde debía estar desde el principio.
 *
 * ── El filtro ignora acentos ─────────────────────────────────────────────────
 * En un sistema en español, escribir «viaticos» y no encontrar «Viáticos» se lee
 * como que la pantalla no existe. Se normaliza a los dos lados.
 */

export interface ItemPaleta {
  texto: string;
  tipo: 'pantalla' | 'accion' | 'referencia';
  atajo?: string;
  onEjecutar(): void;
}

export interface PaletaComandosProps {
  abierta: boolean;
  onCerrar(): void;
  grupos: { titulo: string; items: ItemPaleta[] }[];
  /**
   * Qué se puede buscar acá, en el propio campo.
   *
   * Es una prop y no un texto fijo porque **el campo no puede prometer más de lo que hay**: el
   * valor por defecto nombra referencias y empleados, y montada sobre el menú la paleta sólo
   * tiene pantallas. Quien escribe el nombre de un compañero y no encuentra nada concluye que
   * el sistema no lo tiene, no que la paleta no busca eso.
   */
  etiquetaBusqueda?: string;
}

const ETIQUETA_TIPO: Record<ItemPaleta['tipo'], string> = {
  pantalla: 'Pantalla',
  accion: 'Acción',
  referencia: 'Referencia',
};

/* Una sola definición para todo el frontend — ver el porqué en `app/texto`. */
const normalizar = normalizarTexto;

export default function PaletaComandos({
  abierta,
  onCerrar,
  grupos,
  etiquetaBusqueda = 'Buscar referencia, empleado o pantalla…',
}: PaletaComandosProps): ReactElement {
  const dialogo = useRef<HTMLDialogElement>(null);
  const lista = useRef<HTMLUListElement>(null);
  const [consulta, setConsulta] = useState('');
  const [indice, setIndice] = useState(0);

  // Aplanado con el grupo al lado: la navegación con flechas es sobre UNA lista
  // (el usuario no piensa en grupos al bajar), pero el render los conserva.
  const filtrados = useMemo(() => {
    const q = normalizar(consulta.trim());
    const salida: { grupo: string; item: ItemPaleta }[] = [];
    for (const g of grupos) {
      for (const item of g.items) {
        if (q === '' || normalizar(item.texto).includes(q)) {
          salida.push({ grupo: g.titulo, item });
        }
      }
    }
    return salida;
  }, [grupos, consulta]);

  useEffect(() => {
    const d = dialogo.current;
    if (d === null) return;
    if (abierta && !d.open) {
      d.showModal();
      setConsulta('');
      setIndice(0);
    } else if (!abierta && d.open) {
      d.close();
    }
  }, [abierta]);

  // El índice se reajusta al filtrar: si el usuario estaba en el 7.º y el filtro
  // deja tres, quedaría seleccionando algo que ya no se ve.
  useEffect(() => {
    setIndice((i) => (i >= filtrados.length ? 0 : i));
  }, [filtrados.length]);

  /**
   * La lista sigue al resaltado.
   *
   * Sin esto, bajar con la flecha movía la selección pero **no la lista**: medido con teclado
   * real, a la decimoquinta pulsación el elemento activo estaba 381 px por debajo del borde
   * visible y `scrollTop` seguía en 0. Quien navega con el teclado —que es justamente para
   * quien existe esta paleta— dejaba de ver qué tenía elegido y pulsaba Enter a ciegas.
   *
   * `block: 'nearest'` y no `'center'`: sólo corrige lo mínimo para que entre, así la lista no
   * salta cada vez que se mueve una posición dentro de lo ya visible. Eso además lo hace seguro
   * cuando el índice lo cambia el ratón: si el elemento ya se ve, no hace nada.
   */
  useEffect(() => {
    lista.current
      ?.querySelector('.loki-paleta-item-activo')
      ?.scrollIntoView({ block: 'nearest' });
  }, [indice, filtrados.length]);

  function alTeclado(ev: React.KeyboardEvent<HTMLDialogElement>): void {
    if (ev.key === 'ArrowDown') {
      ev.preventDefault();
      setIndice((i) => (filtrados.length === 0 ? 0 : (i + 1) % filtrados.length));
    } else if (ev.key === 'ArrowUp') {
      ev.preventDefault();
      setIndice((i) => (filtrados.length === 0 ? 0 : (i - 1 + filtrados.length) % filtrados.length));
    } else if (ev.key === 'Enter') {
      ev.preventDefault();
      const elegido = filtrados[indice];
      if (elegido !== undefined) {
        onCerrar();
        elegido.item.onEjecutar();
      }
    }
  }

  let grupoPintado = '';

  return createPortal(
    <dialog
      ref={dialogo}
      onClose={onCerrar}
      onKeyDown={alTeclado}
      /*
        Clic en el velo = cerrar, que es lo que hace toda paleta de comandos y lo que la gente
        ya intenta. Un `<dialog>` NO lo trae: sin esto la única salida es `Esc`, y quien la
        abrió sin querer —el atajo se pulsa por costumbre— y usa el ratón queda atrapado.

        `e.target === dialogo` distingue el velo del contenido: los clics en el fondo llegan con
        el propio `<dialog>` como destino, y los de adentro con el elemento que se pulsó. Sin esa
        comparación, elegir un comando cerraría por partida doble.
      */
      onClick={(e) => {
        if (e.target === dialogo.current) dialogo.current?.close();
      }}
      aria-label="Paleta de comandos"
      className="loki-paleta tw:rounded-panel tw:border tw:border-linea tw:bg-panel tw:shadow-lift"
    >
      {/*
        El campo del sistema (regla D12), sin marco: la caja la pone el propio diálogo, y su
        borde inferior es parte del diseño de la paleta. Lo que sí trae y acá no había: la ✕
        para limpiar y el apagado de la ✕ nativa de Chrome.

        `Escape` se reclama para que CIERRE, que es lo que se espera de una paleta — no limpiar
        y quedarse abierta. Y sólo `Escape`: las flechas y Enter viajan solas al `<dialog>` por
        bubbling, y procesarlas también acá las ejecutaría dos veces.
      */}
      <CampoBusqueda
        etiqueta={etiquetaBusqueda}
        valor={consulta}
        onCambio={setConsulta}
        marco={false}
        autoFoco
        className="loki-paleta-campo tw:w-full tw:border-b tw:border-linea-suave tw:px-4"
        alTeclado={(e) => {
          if (e.key === 'Escape') {
            e.preventDefault();
            dialogo.current?.close();
          }
        }}
      />

      <ul ref={lista} className="loki-scroll loki-paleta-lista">
        {filtrados.length === 0 ? (
          <li className="tw:px-4 tw:py-6 tw:text-center tw:text-cuerpo-2 tw:text-tinta-mid">
            Nada coincide con «{consulta}».
          </li>
        ) : (
          filtrados.map(({ grupo, item }, i) => {
            const encabeza = grupo !== grupoPintado;
            grupoPintado = grupo;
            return (
              <li key={`${grupo}-${item.texto}`}>
                {encabeza ? (
                  <p className="tw:px-4 tw:pt-3 tw:pb-1 tw:text-cabecera tw:font-semibold tw:tracking-wide tw:text-tinta-mid tw:uppercase">
                    {grupo}
                  </p>
                ) : null}
                <button
                  type="button"
                  // `mousemove` y no `mouseenter`: al abrir, el puntero puede estar
                  // ya encima de un ítem y robaría la selección del teclado sin que
                  // el usuario haya movido el ratón.
                  onMouseMove={() => setIndice(i)}
                  onClick={() => {
                    onCerrar();
                    item.onEjecutar();
                  }}
                  className={[
                    'loki-paleta-item tw:flex tw:w-full tw:items-center tw:gap-3 tw:px-4 tw:text-left tw:text-cuerpo-2',
                    i === indice ? 'loki-paleta-item-activo' : '',
                  ]
                    .filter(Boolean)
                    .join(' ')}
                >
                  <span className="tw:min-w-0 tw:flex-1 tw:truncate tw:text-tinta-base">
                    {item.texto}
                  </span>
                  <span className="tw:shrink-0 tw:text-ayuda tw:text-tinta-low">
                    {ETIQUETA_TIPO[item.tipo]}
                  </span>
                  {item.atajo !== undefined ? (
                    <kbd className="loki-tecla loki-tecla-clara tw:shrink-0">{item.atajo}</kbd>
                  ) : null}
                </button>
              </li>
            );
          })
        )}
      </ul>

      {/* El recordatorio de teclas se esconde donde NO hay teclado: en un teléfono instruye
          sobre teclas que no existen, y encima ocupa un renglón del poco alto disponible. Se
          decide por `hover: none` y no por ancho, porque lo que importa es el dispositivo —una
          tableta con teclado acoplado sí las tiene, y una ventana angosta en un escritorio
          también. Mismo criterio que las acciones de fila de la tabla. */}
      {/* ⚠️ El `display` NO va como utilidad acá: `tw:flex` le gana a la regla del CSS y la
          media query que esconde el pie en táctil no tendría efecto. Vive junto a esa media
          query, que es el mismo arreglo que ya tenían las acciones de fila de la tabla. */}
      <footer className="loki-paleta-pie tw:items-center tw:gap-3 tw:border-t tw:border-linea-suave tw:px-4 tw:py-2 tw:text-ayuda tw:text-tinta-low">
        <span>
          <kbd className="loki-tecla loki-tecla-clara">↑</kbd>{' '}
          <kbd className="loki-tecla loki-tecla-clara">↓</kbd> moverse
        </span>
        <span>
          <kbd className="loki-tecla loki-tecla-clara">Enter</kbd> abrir
        </span>
        <span>
          <kbd className="loki-tecla loki-tecla-clara">Esc</kbd> cerrar
        </span>
      </footer>
    </dialog>,
    document.body,
  );
}
