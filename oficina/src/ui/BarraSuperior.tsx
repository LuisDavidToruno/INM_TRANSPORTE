import { Search } from 'lucide-react';
import type { ReactElement, ReactNode } from 'react';

import Avatar from './Avatar';
import Enlace from './Enlace';
import type { Miga } from './tipos';

/**
 * Barra superior del contrato 0.3.3.
 * Canon: COMPONENTS.md §3
 *
 * Vive en `--nav-*`, igual que el riel: los dos forman el marco, y el marco NO
 * acompaña al tema de superficie. Al cambiar de tema repinta el contenido y el
 * marco se queda.
 *
 * ── Los tramos anteriores navegan ────────────────────────────────────────────
 * Un tramo que se ve como miga y no lleva a ninguna parte es la única pieza de
 * la interfaz que miente sobre lo que hace. Quien hace clic en «Catálogo» y
 * obtiene una selección de texto no concluye que la miga sea decorativa:
 * concluye que la aplicación no responde. Los tramos con destino se dibujan
 * subrayados para que se distingan de los que son sólo rótulo.
 *
 * ── El buscador es el disparador de la paleta, no un campo ───────────────────
 * Se ve como un campo porque es donde la gente busca, pero al hacer clic abre la
 * paleta de comandos. Para quien procesa 20+ solicitudes al día, ⌘K vale más que
 * cualquier menú — y el campo es lo que le enseña que existe.
 */

export interface BarraSuperiorProps {
  /** Marca institucional. Va a la izquierda, alineada con el riel. */
  marca?: { logo?: string; nombre: string; bajada: string };
  /**
   * Dónde está el usuario, de izquierda a derecha.
   *
   * Un tramo con `href` navega; una cadena suelta es sólo rótulo. El último
   * nunca se enlaza aunque traiga destino: ver el render.
   */
  migas: readonly Miga[];
  usuario: { nombre: string; rol: string; foto?: string };
  onBuscar(): void;
  /**
   * Qué se puede buscar, dicho en el propio disparador.
   *
   * Es una prop y no un texto fijo porque **el buscador no puede prometer más de
   * lo que hay**. Quien lee «referencia, empleado o pantalla», escribe el nombre
   * de un compañero y no encuentra nada, concluye que el sistema no tiene a esa
   * persona — no que el buscador no busca eso.
   *
   * El valor por defecto es el mínimo honesto: toda aplicación que monte esta
   * barra tiene pantallas.
   */
  etiquetaBusqueda?: string;
  /**
   * Oculta el disparador de búsqueda.
   *
   * Una aplicación que no monte la paleta de comandos no puede mostrar su
   * disparador: prometería una función que no existe, y un botón muerto es peor
   * que un botón ausente. `onBuscar` se queda como prop obligatoria (ver
   * `Shell`) porque el contrato del componente no cambia — sólo se le apaga la
   * única entrada por la que podría dispararse.
   */
  sinBusqueda?: boolean;
  /** Campana, selector de tema, lo que la aplicación quiera colgar a la derecha. */
  acciones?: ReactNode;
}

export default function BarraSuperior({
  marca,
  migas,
  usuario,
  onBuscar,
  etiquetaBusqueda = 'Buscar una pantalla…',
  sinBusqueda = false,
  acciones,
}: BarraSuperiorProps): ReactElement {
  return (
    <header className="loki-barra tw:flex tw:items-center tw:gap-3 tw:bg-nav-bg tw:pr-4">
      {/* `.brand` — 224px, alineada con el riel. El nombre va en SERIFA: es la
          única vez que la serifa aparece fuera de un título, y es a propósito:
          es la firma, no un encabezado. */}
      {marca !== undefined ? (
        <div className="loki-marca tw:flex tw:h-full tw:items-center tw:gap-2.5">
          {marca.logo !== undefined ? <img src={marca.logo} alt="" /> : null}
          <span className="tw:flex tw:flex-col tw:leading-[1.15]">
            <b>{marca.nombre}</b>
            <span>{marca.bajada}</span>
          </span>
        </div>
      ) : null}

      {/* `.miga` — gap 7px, 12px/400. El último tramo en `--nav-hi` y 600: es
          dónde estás, no por dónde pasaste. */}
      <nav aria-label="Ubicación" className="tw:flex tw:min-w-0 tw:items-center tw:gap-[7px]">
        {migas.map((m, i) => {
          const texto = typeof m === 'string' ? m : m.texto;
          const actual = i === migas.length - 1;
          // El último tramo NO se enlaza aunque traiga destino: un enlace a la
          // pantalla en la que ya estás promete un movimiento que no ocurre, y
          // en un lector de pantalla se anuncia como una salida más.
          const destino = actual || typeof m === 'string' ? null : m.href;

          return (
            // La clave lleva el índice: dos tramos pueden decir lo mismo —una
            // ficha llamada «Catálogo» bajo el catálogo— y con la etiqueta sola
            // React descartaría uno de los dos.
            <span key={`${i}:${texto}`} className="tw:flex tw:items-center tw:gap-[7px] tw:text-[12px]">
              {i > 0 ? <span className="loki-barra-sep">/</span> : null}
              {destino !== null ? (
                <Enlace href={destino} className="loki-foco loki-barra-miga-enlace">
                  {texto}
                </Enlace>
              ) : (
                <span
                  className={actual ? 'loki-barra-actual' : 'loki-barra-miga'}
                  // Lo que hace que un lector anuncie cuál de los tramos es
                  // dónde estás parado, y no sólo el último de una lista.
                  aria-current={actual ? 'page' : undefined}
                >
                  {texto}
                </span>
              )}
            </span>
          );
        })}
      </nav>

      {/* Se centra por sí solo con los `flex-1` de los costados, y no con un ancho
          fijo: con migas largas el buscador se corre en vez de desbordar.
          El `div` se conserva aun con `sinBusqueda`: es lo que empuja las
          acciones y el avatar al borde derecho. */}
      <div className="tw:flex tw:flex-1 tw:justify-center">
        {sinBusqueda ? null : (
          <button
            type="button"
            onClick={onBuscar}
            className="loki-foco loki-buscador tw:flex tw:items-center tw:gap-2 tw:rounded-control tw:px-3 tw:text-cuerpo-2"
          >
            <Search size={15} strokeWidth={1.8} aria-hidden={true} />
            <span className="tw:flex-1 tw:text-left">{etiquetaBusqueda}</span>
            {/* Las teclas se muestran, no se explican: es cómo se aprende el atajo. */}
            <kbd className="loki-tecla">⌘</kbd>
            <kbd className="loki-tecla">K</kbd>
          </button>
        )}
      </div>

      <div className="tw:flex tw:shrink-0 tw:items-center tw:gap-2">
        {acciones}
        <span className="tw:flex tw:items-center tw:gap-2 tw:pl-1">
          <Avatar nombre={usuario.nombre} url={usuario.foto} tamano={30} />
          {/* El rol va debajo del nombre y no en un tooltip: en este sistema la
              capacidad depende del rol, y verlo evita la mitad de las preguntas
              de «por qué no me deja». */}
          <span className="tw:hidden tw:leading-tight tw:sm:block">
            <span className="loki-barra-actual tw:block tw:text-cuerpo-2 tw:font-semibold">
              {usuario.nombre}
            </span>
            <span className="loki-barra-miga tw:block tw:text-ayuda">{usuario.rol}</span>
          </span>
        </span>
      </div>
    </header>
  );
}
