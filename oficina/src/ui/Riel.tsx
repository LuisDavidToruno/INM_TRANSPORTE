import { PanelLeftClose, PanelLeftOpen } from 'lucide-react';
import type { ReactElement } from 'react';

import Enlace from './Enlace';
import type { GrupoNav, ItemNav } from './tipos';

/**
 * Riel de navegación del contrato 0.3.3.
 * Canon: COMPONENTS.md §3
 *
 * ── Vive en `--nav-*`, que es un eje aparte del tema de superficie ───────────
 * Al cambiar de tema el contenido repinta y el riel NO acompaña: lo enmarca. Es
 * lo que se comprobó en la vitrina del paquete y lo que hace que el riel sea el
 * rasgo estable de la marca entre los seis temas.
 *
 * ── La barra de 2 px del ítem activo es EL rasgo de marca ────────────────────
 * `left:-2px; top:7px; bottom:7px`, en `--nav-rail`. No es un subrayado ni un
 * fondo: es una barra pegada al borde izquierdo, y las medidas son del contrato.
 *
 * ── El menú lo arma el servidor: entre 4 y 52 ítems ──────────────────────────
 * Un empleado ve cuatro; un superadministrador, cincuenta y dos. Por eso el
 * `<nav>` tiene su propio desplazamiento y reserva el canal de la barra SIEMPRE
 * —exista o no desbordamiento—: sin eso, el ancho útil cambia según cuántos
 * ítems tenga el usuario y los íconos del riel plegado se corren unos píxeles
 * respecto del logo. Un defecto que depende de los datos es peor que uno
 * constante.
 */

export interface RielProps {
  grupos: GrupoNav[];
  plegado: boolean;
  onPlegar(): void;
  /** Ruta activa. La decide quien monta el riel, no el riel. */
  activo?: string;
  /** Pie del riel. El diseño pone al usuario ACÁ, no sólo en la barra. */
  usuario?: { nombre: string; rol: string; iniciales: string };
}

export default function Riel({
  grupos,
  plegado,
  onPlegar,
  activo,
  usuario,
}: RielProps): ReactElement {
  return (
    <aside
      className={[
        'loki-riel loki-aside',
        plegado ? 'loki-riel-plegado' : '',
        'tw:flex tw:flex-col tw:bg-nav-bg',
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <nav className="loki-scroll loki-riel-nav tw:flex-1 tw:py-2">
        {grupos.map((grupo, i) => (
          <div key={grupo.titulo ?? `g${i}`} className="tw:mb-1">
            {/* Plegado NO desaparece: el CSS lo reduce a un filete separador, que
                conserva la agrupación cuando el rótulo ya no cabe. */}
            {grupo.titulo !== undefined ? (
              <p className="loki-riel-titulo">{grupo.titulo}</p>
            ) : null}
            <ul>
              {grupo.items.map((item) => (
                <li key={item.href}>
                  <ItemRiel item={item} activo={item.href === activo} />
                </li>
              ))}
            </ul>
          </div>
        ))}
      </nav>

      {usuario !== undefined ? (
        <div className="loki-riel-pie">
          <span className="av" aria-hidden="true">
            {usuario.iniciales}
          </span>
          <span className="tx tw:min-w-0 tw:flex-1">
            <b>{usuario.nombre}</b>
            <span className="rol">{usuario.rol}</span>
          </span>
        </div>
      ) : null}

      <button
        type="button"
        onClick={onPlegar}
        title={plegado ? 'Expandir el menú' : 'Plegar el menú'}
        aria-label={plegado ? 'Expandir el menú' : 'Plegar el menú'}
        className="loki-foco loki-riel-item tw:flex tw:items-center tw:gap-2.5 tw:border-t tw:border-nav-line tw:px-3 tw:py-2.5 tw:text-cuerpo-2"
      >
        {plegado ? <PanelLeftOpen size={15} strokeWidth={1.8} /> : <PanelLeftClose size={15} strokeWidth={1.8} />}
        {!plegado ? <span>Plegar</span> : null}
      </button>
    </aside>
  );
}

function ItemRiel({
  item,
  activo,
}: {
  readonly item: ItemNav;
  readonly activo: boolean;
}): ReactElement {
  return (
    /* `Enlace` y no `<a>`: un ancla de documento recarga la aplicación entera,
       y el riel es justamente lo que se usa para salir de una pantalla cara.
       Medido en LOKI: desde una vista que tarda 18 segundos en dibujarse, cada
       clic en el riel tiraba ese trabajo y volvía a descargar la biblioteca que
       lo dibuja. `Enlace` usa `<Link>` para rutas internas y `<a>` para lo
       externo, así que el cambio no le quita nada al menú con destinos fuera
       de la aplicación. */
    <Enlace
      href={item.href}
      aria-current={activo ? 'page' : undefined}
      className={['loki-foco loki-riel-item', activo ? 'loki-riel-item-activo' : '']
        .filter(Boolean)
        .join(' ')}
    >
      <span className="tw:shrink-0">{item.icono}</span>

      {/* El rótulo se renderiza SIEMPRE: plegado, el CSS lo convierte en tooltip.
          Con `title` nativo el navegador tarda ~1s en mostrarlo, y con el riel
          plegado el usuario está barriendo íconos rápido. */}
      <span className="loki-riel-texto tw:min-w-0 tw:flex-1 tw:truncate">{item.texto}</span>

      {item.externo ? <span className="loki-riel-externo">{item.externo}</span> : null}

      {item.contador !== undefined && item.contador > 0 ? (
        <span
          className={[
            'loki-riel-contador',
            item.accionable === true ? 'loki-riel-contador-accionable' : '',
          ]
            .filter(Boolean)
            .join(' ')}
        >
          {item.contador}
        </span>
      ) : null}
    </Enlace>
  );
}
