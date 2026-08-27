import { ArrowDown, ArrowUp, ChevronRight } from 'lucide-react';
import { Fragment, useEffect, useMemo, useRef, useState } from 'react';
import CampoBusqueda from './CampoBusqueda';
import type { ReactElement, ReactNode } from 'react';

import type { ColumnaDef, Plazo } from './tipos';
import { normalizarTexto } from './texto';
import { EsqueletoTabla } from './Esqueleto';
import Paginador from './Paginador';

/**
 * Tabla de registro del contrato 0.3.3.
 * Traducida de `.tw table` / `tr.r` de `referencia/argos5/ui.css`.
 *
 * ── La marca de plazo es lo que permite escanear sin leer ────────────────────
 * Barra de 3 px en el borde izquierdo de la fila, desde `data-plazo`. El oficial
 * barre la columna y ve cuáles están vencidas sin leer la columna de plazo. Sin
 * ella hay que leer 31 filas para encontrar las 7 que importan.
 *
 * ── Las acciones aparecen en `:hover` Y en `:focus-within` ───────────────────
 * Sólo con `:hover` serían inalcanzables por teclado, y el recorrido completo
 * por teclado es requisito. Es la clase de detalle que no se nota hasta que
 * alguien no puede usar la pantalla.
 *
 * ── El detalle expandido va como `<tr>` HERMANO, no en un modal ──────────────
 * Para poder compararlo con las filas vecinas. Un modal tapa justamente el
 * contexto contra el que se está comparando.
 *
 * ── El desplazamiento horizontal vive en el contenedor de la tabla ───────────
 * Nunca en la página: si la página se desplaza a lo ancho, se pierde el riel.
 * Este defecto apareció tres veces durante el diseño; su causa habitual es un
 * ancestro con `overflow:hidden` puesto para recortar esquinas redondeadas.
 */

export interface TablaProps<T> {
  columnas: ColumnaDef<T>[];
  filas: T[];
  /** Clave estable de cada fila. */
  claveDe(fila: T): string;
  cargando?: boolean;
  /**
   * La carga falló, así que la tabla NO puede afirmar que no hay nada.
   *
   * ⚠️ Existe porque `filas` casi siempre se deriva con `data?.items ?? []`, y ese `?? []`
   * vuelve una carga FALLIDA indistinguible de una lista vacía: la tabla mostraba su texto de
   * vacío —«No tiene solicitudes en planificación»— justo debajo de la banda que decía «no se
   * pudieron cargar». Las dos a la vez, y la tranquilizadora es la que se cree. Medido el
   * 2026-08-08 en Mis Solicitudes, con un fallo de red corriente.
   *
   * Quien la usa pasa `cargaFallida={error !== null}`. La tabla se queda sin cuerpo; explicar
   * el fallo es de la pantalla, que es la que sabe qué pasó.
   */
  cargaFallida?: boolean;
  /** Marca de 3 px en el borde izquierdo. */
  plazoDe?(fila: T): Plazo | undefined;
  seleccion?: { ids: string[]; onChange(ids: string[]): void };
  expansion?: { render(fila: T): ReactNode };
  /** Ocultas hasta `:hover` y `:focus-within`. */
  accionesFila?(fila: T): ReactNode;
  /**
   * Cómo se llama la tabla cuando hay que desplazarla a lo ancho.
   *
   * Sólo se usa si el contenido no cabe. En ese caso el contenedor pasa a ser
   * una región enfocable —para que el desplazamiento horizontal exista también
   * por teclado, no sólo con mouse— y una región sin nombre no sirve de nada:
   * el lector de pantalla anunciaría «región» y el usuario no sabría dentro de
   * qué entró.
   */
  rotulo?: string;
  onAbrirFila?(fila: T): void;

  /**
   * Buscador sobre el texto que devuelve `textoDe`. **Sin esta prop no hay
   * buscador** — así la vitrina y las tablas cortas quedan exactamente como antes.
   *
   * El texto lo arma quien llama y no se deriva de las celdas: una celda es un
   * árbol de React del que no se puede sacar texto de forma fiable, y además hay
   * datos por los que conviene buscar sin mostrarlos en columna (el número de
   * memorándum cuando está bajo el nombre, por ejemplo).
   */
  busqueda?: { etiqueta: string; textoDe(fila: T): string };

  /** Pagina cada N filas. Sin esto se dibujan todas. */
  porPagina?: number;

  /** Qué decir cuando no hay ninguna fila. La falta de coincidencias se dice sola. */
  vacio?: ReactNode;
}

/*
 * Quita acentos y baja a minúsculas: en es-HN se escribe «viaticos» y se busca «Viáticos».
 * Comparar sin normalizar es la forma más rápida de que el buscador parezca roto.
 *
 * Vive en `app/texto` porque esto estaba escrito CINCO veces —acá, en la paleta, en los dos
 * selectores buscables y en el clasificador de tonos—. Ver el porqué allá.
 */
const normalizar = normalizarTexto;

export default function Tabla<T>({
  columnas,
  filas,
  claveDe,
  cargando = false,
  cargaFallida = false,
  plazoDe,
  seleccion,
  expansion,
  accionesFila,
  onAbrirFila,
  busqueda,
  porPagina,
  rotulo = 'Tabla de datos',
  vacio = 'No hay nada que mostrar.',
}: TablaProps<T>): ReactElement {
  const [abiertas, setAbiertas] = useState<Set<string>>(new Set());
  const [filtro, setFiltro] = useState('');
  const [orden, setOrden] = useState<{ id: string; desc: boolean } | null>(null);
  const [pagina, setPagina] = useState(0);

  const marco = useRef<HTMLDivElement>(null);
  const visor = useRef<HTMLDivElement>(null);
  const [desborda, setDesborda] = useState(false);

  /**
   * Avisar que la tabla sigue hacia el costado.
   *
   * Medido el 2026-08-25 en el catálogo de LOKI: a 375 px se veían dos de nueve
   * columnas, con 868 px de tabla fuera de la vista y nada en pantalla que lo
   * dijera — la tabla parecía tener dos columnas y el resto no existía. Y el
   * desplazamiento horizontal de un `overflow` sólo lo alcanza quien tiene
   * mouse o trackpad: sin `tabindex` no hay forma de llegar por teclado a las
   * columnas ocultas.
   *
   * El `tabindex` se pone SÓLO cuando desborda: una parada de tabulación en un
   * contenedor que no se puede desplazar es una parada que no hace nada, y
   * quien recorre por teclado la paga en cada tabla que sí cabe.
   *
   * Las dos sombras se marcan con atributos en el DOM en vez de con estado de
   * React: el desplazamiento dispara este cálculo en cada cuadro, y volver a
   * renderizar la tabla entera sesenta veces por segundo para encender una
   * sombra sale carísimo. `desborda` sí es estado porque cambia poco y decide
   * atributos que React tiene que escribir.
   */
  useEffect(() => {
    const v = visor.current;
    const m = marco.current;
    if (v === null || m === null) return;

    const medir = (): void => {
      const sobra = v.scrollWidth - v.clientWidth;
      // Un píxel de tolerancia: con el zoom del navegador `scrollWidth` y
      // `clientWidth` difieren en fracciones sin que haya nada oculto.
      const hay = sobra > 1;
      setDesborda(hay);
      m.toggleAttribute('data-mas-izq', hay && v.scrollLeft > 1);
      m.toggleAttribute('data-mas-der', hay && v.scrollLeft < sobra - 1);
    };

    medir();
    v.addEventListener('scroll', medir, { passive: true });

    // Dos observados y no uno: el hueco cambia al redimensionar la ventana o
    // plegar el riel, y la tabla cambia sola cuando llegan las filas.
    const ro = typeof ResizeObserver === 'undefined' ? null : new ResizeObserver(medir);
    ro?.observe(v);
    if (v.firstElementChild !== null) ro?.observe(v.firstElementChild);

    return () => {
      v.removeEventListener('scroll', medir);
      ro?.disconnect();
    };
  }, [cargando, columnas.length]);

  const filtradas = useMemo(() => {
    if (busqueda === undefined || filtro.trim() === '') return filas;
    const q = normalizar(filtro.trim());
    return filas.filter((f) => normalizar(busqueda.textoDe(f)).includes(q));
  }, [filas, filtro, busqueda]);

  const ordenadas = useMemo(() => {
    if (orden === null) return filtradas;
    const col = columnas.find((c) => c.id === orden.id);
    if (col?.valorOrden === undefined) return filtradas;
    const valor = col.valorOrden;
    // Copia: ordenar sobre el arreglo del servidor mutaría la caché de la consulta.
    return [...filtradas].sort((a, b) => {
      const va = valor(a);
      const vb = valor(b);
      const cmp =
        typeof va === 'number' && typeof vb === 'number'
          ? va - vb
          : String(va).localeCompare(String(vb), 'es');
      return orden.desc ? -cmp : cmp;
    });
  }, [filtradas, orden, columnas]);

  const totalPaginas = porPagina === undefined ? 1 : Math.max(1, Math.ceil(ordenadas.length / porPagina));
  // Si el filtro dejó menos páginas que la actual, se vuelve a la primera en el
  // render en vez de mostrar una página vacía y un paginador que dice «3 de 1».
  const paginaActual = Math.min(pagina, totalPaginas - 1);
  const visibles =
    porPagina === undefined
      ? ordenadas
      : ordenadas.slice(paginaActual * porPagina, (paginaActual + 1) * porPagina);

  function alternarOrden(id: string): void {
    setOrden((o) => {
      if (o?.id !== id) {
        // El primer clic de una columna numérica ordena DESCENDENTE: en una bandeja
        // lo primero que se busca es lo más vencido, no lo más holgado.
        const col = columnas.find((c) => c.id === id);
        return { id, desc: col?.numerica === true };
      }
      return o.desc ? null : { id, desc: true };
    });
    setPagina(0);
  }

  if (cargando) {
    // Las MISMAS columnas: así los anchos y la alineación coinciden celda por
    // celda y no hay salto al llegar el dato.
    return (
      <div ref={marco} className="loki-tabla-marco">
        <div ref={visor} className="loki-tabla-visor">
          <EsqueletoTabla columnas={columnas as ColumnaDef[]} filas={5} />
        </div>
      </div>
    );
  }

  const alternar = (clave: string): void =>
    setAbiertas((s) => {
      const n = new Set(s);
      if (n.has(clave)) n.delete(clave);
      else n.add(clave);
      return n;
    });

  const alternarSeleccion = (clave: string): void => {
    if (seleccion === undefined) return;
    const hay = seleccion.ids.includes(clave);
    seleccion.onChange(
      hay ? seleccion.ids.filter((i) => i !== clave) : [...seleccion.ids, clave],
    );
  };

  const columnasTotales =
    columnas.length +
    (seleccion !== undefined ? 1 : 0) +
    (expansion !== undefined ? 1 : 0) +
    (accionesFila !== undefined ? 1 : 0);

  return (
    <div className="tw:flex tw:flex-col tw:gap-3">
      {busqueda !== undefined ? (
        /* Modo FILTRA: las filas ya están acá, así que se recorta al tipear. Volver a
           la página 1 no es cosmético — filtrando desde la página 4 el resultado
           puede tener 3 páginas y la tabla se vería vacía. */
        <CampoBusqueda
          etiqueta={busqueda.etiqueta}
          valor={filtro}
          onCambio={(v) => {
            setFiltro(v);
            setPagina(0);
          }}
        />
      ) : null}

      <div ref={marco} className="loki-tabla-marco">
        <div
          ref={visor}
          className="loki-tabla-visor"
          // Los tres atributos van juntos o no van: `region` sin nombre no
          // orienta a nadie, y un nombre sin `tabindex` nombra algo a lo que
          // el teclado no llega. Cuando la tabla cabe entera no hay región que
          // anunciar ni nada que desplazar, así que no se pone ninguno.
          {...(desborda
            ? { tabIndex: 0, role: 'region' as const, 'aria-label': rotulo }
            : {})}
        >
          <table className="loki-tabla">
            <thead>
              <tr>
                {seleccion !== undefined ? <th className="loki-th loki-col-ck" /> : null}
                {expansion !== undefined ? <th className="loki-th loki-col-exp" /> : null}
                {columnas.map((c) => {
                  const ordena = c.ordenable === true && c.valorOrden !== undefined;
                  const dir = orden?.id === c.id ? (orden.desc ? 'desc' : 'asc') : null;
                  return (
                    <th
                      key={c.id}
                      style={c.ancho !== undefined ? { width: c.ancho } : undefined}
                      // `aria-sort` es lo que hace que un lector de pantalla anuncie el
                      // orden; sin él, la flecha sólo existe para quien ve la pantalla.
                      aria-sort={
                        dir === 'asc' ? 'ascending' : dir === 'desc' ? 'descending' : undefined
                      }
                      className={`loki-th ${c.numerica === true ? 'tw:text-right' : 'tw:text-left'}`}
                    >
                      {ordena ? (
                        <button
                          type="button"
                          onClick={() => alternarOrden(c.id)}
                          className="loki-foco tw:inline-flex tw:items-center tw:gap-1 tw:hover:text-tinta-hi"
                        >
                          {c.cabecera}
                          {dir === 'asc' ? <ArrowUp size={11} aria-hidden="true" /> : null}
                          {dir === 'desc' ? <ArrowDown size={11} aria-hidden="true" /> : null}
                        </button>
                      ) : (
                        c.cabecera
                      )}
                    </th>
                  );
                })}
                {accionesFila !== undefined ? <th className="loki-th loki-th-acts" /> : null}
              </tr>
            </thead>
            <tbody>
              {/* Sin datos por un fallo NO se dice «no hay nada»: no se sabe. */}
              {visibles.length === 0 && !cargaFallida ? (
                <tr>
                  <td className="loki-td" colSpan={columnasTotales}>
                    {filtro.trim() !== '' ? (
                      <p className="loki-vacio-texto tw:mx-auto tw:py-6 tw:text-center">
                        Nada coincide con la búsqueda.
                      </p>
                    ) : (
                      /* El slot `vacio` puede traer un componente con encabezado y
                         acción —`Vacio` emite `h3`, `p` y un contenedor— y eso no
                         cabe dentro de un `<p>`: el navegador lo cierra antes de
                         tiempo, React avisa del anidamiento inválido, y el texto
                         se parte a media frase. Va en un contenedor neutro, y sin
                         el ancho máximo de 34ch que sí quiere el mensaje corto. */
                      <div className="tw:py-6 tw:text-center">{vacio}</div>
                    )}
                  </td>
                </tr>
              ) : null}
              {visibles.map((fila) => {
              const clave = claveDe(fila);
              const plazo = plazoDe?.(fila);
              const elegida = seleccion?.ids.includes(clave) ?? false;
              return (
                <Fragment key={clave}>
                  <tr
                    className="loki-fila-r"
                    data-plazo={plazo}
                    data-sel={elegida ? '' : undefined}
                  >
                    {seleccion !== undefined ? (
                      <td className="loki-td">
                        <button
                          type="button"
                          role="checkbox"
                          aria-checked={elegida}
                          aria-label={`Seleccionar ${clave}`}
                          onClick={() => alternarSeleccion(clave)}
                          className="loki-ck"
                        >
                          <svg viewBox="0 0 16 16" width="11" height="11" aria-hidden="true">
                            <path
                              d="M3 8.5l3.2 3.2L13 5"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="2.2"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                            />
                          </svg>
                        </button>
                      </td>
                    ) : null}

                    {expansion !== undefined ? (
                      <td className="loki-td">
                        <button
                          type="button"
                          onClick={() => alternar(clave)}
                          aria-expanded={abiertas.has(clave)}
                          aria-label={abiertas.has(clave) ? 'Contraer detalle' : 'Ver detalle'}
                          className={`loki-exp ${abiertas.has(clave) ? 'loki-exp-abierto' : ''}`}
                        >
                          <ChevronRight size={15} strokeWidth={1.8} />
                        </button>
                      </td>
                    ) : null}

                    {columnas.map((c) => (
                      <td
                        key={c.id}
                        className={`loki-td ${c.numerica === true ? 'loki-td-n' : ''}`}
                        onClick={c.id === columnas[0]?.id ? () => onAbrirFila?.(fila) : undefined}
                      >
                        {c.celda?.(fila)}
                      </td>
                    ))}

                    {accionesFila !== undefined ? (
                      <td className="loki-td loki-td-acts">
                        <div className="loki-acts">{accionesFila(fila)}</div>
                      </td>
                    ) : null}
                  </tr>

                  {expansion !== undefined && abiertas.has(clave) ? (
                    <tr>
                      <td
                        className="loki-td-det"
                        colSpan={
                          columnas.length +
                          (seleccion !== undefined ? 1 : 0) +
                          1 +
                          (accionesFila !== undefined ? 1 : 0)
                        }
                      >
                        {expansion.render(fila)}
                      </td>
                    </tr>
                  ) : null}
                </Fragment>
              );
            })}
            </tbody>
          </table>
        </div>
      </div>

      {porPagina !== undefined && totalPaginas > 1 ? (
        /* Se dice el rango Y los números: cuántas quedan, y adónde se puede ir. Compartido
           con la tabla de las pantallas — antes cada una tenía el suyo y ya habían
           divergido en el texto. */
        <Paginador
          pagina={paginaActual}
          totalPaginas={totalPaginas}
          onCambio={setPagina}
          desde={paginaActual * porPagina + 1}
          hasta={Math.min((paginaActual + 1) * porPagina, ordenadas.length)}
          total={ordenadas.length}
        />
      ) : null}
    </div>
  );
}
