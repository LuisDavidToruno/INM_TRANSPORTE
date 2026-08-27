import { Check, ChevronDown, Search } from 'lucide-react';
import { useEffect, useId, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import type { KeyboardEvent, ReactElement } from 'react';

import { normalizarTexto } from './texto';

/**
 * Un selector de lista larga con búsqueda. Contrato 0.3.3.
 *
 * ── Por qué existe, si el `<select>` nativo ya trae búsqueda por tecleo ───────
 * Porque esa búsqueda va **por prefijo**, y con estos datos no sirve. Medido sobre las
 * oficinas del INM: **27 de 38 empiezan con «Delegación»**, y otras tres con «Aeropuerto».
 * Quien busca Tela, Roatán o Puerto Cortés teclea el nombre del lugar y no encuentra nada —
 * tiene que acordarse de la palabra que comparten veintisiete oficinas.
 *
 * O sea: la decisión anterior de usar el control nativo era buena y su razón está escrita; lo
 * que no se sostiene es la premisa, con **estos** nombres.
 *
 * ── En reposo es un select, y recién al abrirse aparece el buscador ───────────
 * El primer intento fue al revés —un campo de búsqueda permanente, con su lupa a la izquierda—
 * y quedó **feo entre campos que no lo son**: el valor desplazado, otro aspecto, otra altura.
 * Un formulario donde un control se ve distinto de sus vecinos se lee como un error, no como
 * una función.
 *
 * Ahora el disparador es un botón que usa el MISMO bloque de estilo que `input` y `select`
 * —declarado en el contrato, junto a ellos— y el campo de búsqueda vive **dentro del panel**,
 * que es donde sirve. En reposo, indistinguible de cualquier otro campo.
 *
 * ⚠️ Y ésa fue la causa técnica de lo feo, no sólo la estética: el contrato estiliza por hijo
 * DIRECTO (`.loki-campo > input`). Un selector con panel necesita un contenedor propio para
 * posicionarlo, y ahí el control deja de ser hijo directo y pierde de golpe alto, borde, fondo,
 * tipografía y foco. Por eso el disparador se declaró en el CSS del contrato en vez de llevar
 * clases pegadas acá.
 *
 * ── El panel va en un PORTAL al `body`, no donde se declara ──────────────────
 * Este selector vive dentro de un diálogo que tiene su propio desplazamiento. Montado en su
 * sitio, el panel se **recortaba contra el borde** del diálogo y se iba con el scroll: la lista
 * quedaba cortada por la mitad. Es la misma lección que la paleta de comandos ya tenía escrita.
 *
 * Colgado del `body` no hay ancestro que pueda recortarlo ni crear un contexto de apilamiento
 * que lo atrape. La contrapartida es que la posición hay que calcularla —y **recalcularla** en
 * scroll y resize, con `capture` para enterarse también del scroll de contenedores internos—.
 * Y se decide arriba o abajo según el espacio que quede: pegado al borde inferior, un panel de
 * 300px abajo no se vería.
 *
 * ── Qué NO es ────────────────────────────────────────────────────────────────
 * No reemplaza al `<select>` en todos lados. Para tres o cinco opciones el nativo sigue siendo
 * mejor: menos código, comportamiento táctil del sistema y accesibilidad de fábrica. Esto es
 * para listas donde **encontrar** es el trabajo.
 *
 * ── El filtro ignora acentos ─────────────────────────────────────────────────
 * En un sistema en español, escribir «roatan» y no encontrar «Roatán» se lee como que la
 * oficina no existe. Se normaliza a los dos lados — mismo criterio que la paleta de comandos.
 *
 * ── La agrupación se conserva al filtrar ─────────────────────────────────────
 * En este sistema la categoría de una oficina no es decoración: es **qué es** —una delegación,
 * un centro de atención, un control interior—, y hay 27 delegaciones que sin ella se
 * confunden. Los grupos vacíos desaparecen; los que quedan siguen rotulados.
 *
 * ── Accesibilidad ────────────────────────────────────────────────────────────
 * El patrón es el de un combobox con el campo de texto dentro del panel: el disparador anuncia
 * el valor y `aria-expanded`; abierto, el foco pasa al buscador, que lleva `role="combobox"`
 * con `aria-activedescendant` apuntando al resaltado — así un lector de pantalla anuncia la
 * opción mientras se navega con las flechas **sin mover el foco**, para poder seguir
 * escribiendo. Al cerrar, el foco vuelve al disparador.
 */

export interface OpcionBuscable {
  /** Valor que viaja al formulario. Se compara como texto. */
  readonly valor: string;
  /** Lo que se lee en la lista y en el disparador una vez elegida. */
  readonly etiqueta: string;
  /** Encabezado bajo el que se agrupa. Vacío = sin agrupar. */
  readonly grupo?: string;
  /** Texto extra que también se busca y no se muestra (municipio, código, alias). */
  readonly buscarTambien?: string;
}

/* Una sola definicion para todo el frontend - ver el porque en `app/texto`. */
const normalizar = normalizarTexto;

export default function SelectorBuscable({
  opciones,
  valor,
  onCambio,
  onBlur,
  id,
  vacio = 'Seleccione…',
  buscarPlaceholder = 'Escriba para buscar…',
  sinResultados = 'Nada coincide con lo que escribió.',
  deshabilitado = false,
  'aria-describedby': describedBy,
  'aria-invalid': invalido,
}: {
  readonly opciones: readonly OpcionBuscable[];
  /** El valor elegido, o cadena vacía. */
  readonly valor: string;
  readonly onCambio: (valor: string) => void;
  readonly onBlur?: () => void;
  readonly id?: string;
  /** Lo que dice el disparador cuando no hay nada elegido. */
  readonly vacio?: string;
  readonly buscarPlaceholder?: string;
  readonly sinResultados?: string;
  readonly deshabilitado?: boolean;
  readonly 'aria-describedby'?: string;
  readonly 'aria-invalid'?: boolean;
}): ReactElement {
  const idBase = useId();
  const idDisparador = id ?? `${idBase}-d`;
  const idLista = `${idBase}-lista`;

  const [abierto, setAbierto] = useState(false);
  const [caja, setCaja] = useState<{ x: number; y: number; ancho: number; arriba: boolean } | null>(null);
  const [consulta, setConsulta] = useState('');
  const [indice, setIndice] = useState(0);
  const contenedor = useRef<HTMLDivElement>(null);
  const disparador = useRef<HTMLButtonElement>(null);
  const buscador = useRef<HTMLInputElement>(null);
  const lista = useRef<HTMLUListElement>(null);
  const panel = useRef<HTMLDivElement>(null);

  /**
   * Dónde se cuelga el panel.
   *
   * ⚠️ **Al `body` NO**, que es la respuesta de manual y acá es justo la equivocada. Este
   * selector vive dentro de un `<dialog>` abierto con `showModal()`, y eso pone al diálogo en
   * el **top layer**: cualquier cosa colgada del `body` queda por debajo, y el `z-index` no
   * interviene — son capas distintas, no un mismo apilamiento. Se probó: el panel estaba en el
   * DOM, con sus opciones, y en pantalla no se veía.
   *
   * Colgándolo del propio `<dialog>` queda en el top layer con él, y sigue escapando del
   * contenedor con scroll que lo recortaba, que era el problema original. Sin diálogo alrededor
   * —el selector suelto en una pantalla— el `body` es lo correcto.
   */
  const [destino, setDestino] = useState<HTMLElement | null>(null);

  const elegida = opciones.find((o) => o.valor === valor);

  const filtradas = useMemo(() => {
    const q = normalizar(consulta.trim());
    if (q === '') return opciones;
    return opciones.filter(
      (o) => normalizar(`${o.etiqueta} ${o.buscarTambien ?? ''}`).includes(q),
    );
  }, [opciones, consulta]);

  // El índice se reajusta al filtrar: si estaba en el 7.º y quedan tres, apuntaría a la nada.
  useEffect(() => {
    setIndice((i) => (i >= filtradas.length ? 0 : i));
  }, [filtradas.length]);

  // La posición del panel, en coordenadas del viewport. Se recalcula mientras esté abierto:
  // el diálogo que lo contiene se desplaza, y con él el disparador.
  useLayoutEffect(() => {
    if (!abierto) { setCaja(null); return undefined; }
    function medir(): void {
      const r = disparador.current?.getBoundingClientRect();
      if (!r) return;
      const ALTO = 320;
      const abajo = window.innerHeight - r.bottom;
      const arriba = abajo < ALTO && r.top > abajo;
      setCaja({ x: r.left, y: arriba ? r.top : r.bottom, ancho: r.width, arriba });
    }
    medir();
    setDestino(disparador.current?.closest('dialog') ?? document.body);
    window.addEventListener('scroll', medir, true);
    window.addEventListener('resize', medir);
    return () => {
      window.removeEventListener('scroll', medir, true);
      window.removeEventListener('resize', medir);
    };
  }, [abierto]);

  // Al abrir, el foco va al buscador — que es lo que uno quiere hacer a continuación.
  useEffect(() => {
    if (abierto) buscador.current?.focus();
  }, [abierto]);

  // Cerrar al pulsar fuera. Sin esto la lista queda abierta sobre el resto del formulario.
  useEffect(() => {
    if (!abierto) return undefined;
    function fuera(ev: MouseEvent): void {
      const t = ev.target as Node;
      // El panel ya NO está dentro del contenedor: vive en el portal, así que hay que
      // preguntarle a los dos. Sin esto, pulsar una opción cuenta como «pulsó afuera».
      if (contenedor.current?.contains(t)) return;
      if (panel.current?.contains(t)) return;
      cerrar(false);
    }
    document.addEventListener('mousedown', fuera);
    return () => { document.removeEventListener('mousedown', fuera); };
  }, [abierto]);

  // Mantener a la vista la opción resaltada mientras se navega con el teclado.
  useEffect(() => {
    if (!abierto) return;
    lista.current?.querySelector('[data-resaltada="true"]')
      ?.scrollIntoView({ block: 'nearest' });
  }, [indice, abierto]);

  function cerrar(devolverFoco: boolean): void {
    setAbierto(false);
    setConsulta('');
    if (devolverFoco) disparador.current?.focus();
  }

  function elegir(o: OpcionBuscable): void {
    onCambio(o.valor);
    cerrar(true);
  }

  function alTeclearEnBuscador(ev: KeyboardEvent<HTMLInputElement>): void {
    if (ev.key === 'ArrowDown' || ev.key === 'ArrowUp') {
      ev.preventDefault();
      if (filtradas.length === 0) return;
      setIndice((i) =>
        ev.key === 'ArrowDown'
          ? (i + 1) % filtradas.length
          : (i - 1 + filtradas.length) % filtradas.length,
      );
      return;
    }
    if (ev.key === 'Enter') {
      ev.preventDefault();
      const o = filtradas[indice];
      if (o) elegir(o);
      return;
    }
    if (ev.key === 'Escape') {
      ev.preventDefault();
      // Sólo cierra la lista. Sin el `stopPropagation` el mismo Esc cerraría además el
      // diálogo que contiene al formulario, y se perdería lo escrito.
      ev.stopPropagation();
      cerrar(true);
    }
  }

  function alTeclearEnDisparador(ev: KeyboardEvent<HTMLButtonElement>): void {
    if (ev.key === 'ArrowDown' || ev.key === 'Enter' || ev.key === ' ') {
      ev.preventDefault();
      setAbierto(true);
    }
  }

  // Se recorre en orden respetando los grupos, para que el índice del teclado y lo que se ve
  // coincidan. Un `filter` por grupo dentro del render los desordenaría respecto del índice.
  const conGrupo: { grupo: string; items: { o: OpcionBuscable; i: number }[] }[] = [];
  filtradas.forEach((o, i) => {
    const g = o.grupo ?? '';
    const ultimo = conGrupo[conGrupo.length - 1];
    if (ultimo && ultimo.grupo === g) ultimo.items.push({ o, i });
    else conGrupo.push({ grupo: g, items: [{ o, i }] });
  });

  return (
    <div ref={contenedor} className="loki-selector tw:relative">
      <button
        ref={disparador}
        id={idDisparador}
        type="button"
        aria-haspopup="listbox"
        aria-expanded={abierto}
        aria-describedby={describedBy}
        aria-invalid={invalido}
        disabled={deshabilitado}
        onClick={() => { setAbierto((a) => !a); }}
        onKeyDown={alTeclearEnDisparador}
        onBlur={onBlur}
      >
        <span
          className={`tw:min-w-0 tw:flex-1 tw:truncate ${elegida ? '' : 'tw:text-tinta-low'}`}
        >
          {/*
            Tres estados, no dos. Hay un cuarto de segundo en que el formulario ya trae el valor
            —lo puso una edición— pero el catálogo todavía viaja: entonces no se conoce la
            etiqueta. Mostrar ahí el texto de vacío diría que no hay nada elegido, que es falso,
            y el usuario vuelve a elegir lo que ya estaba. Se dice que está cargando.
          */}
          {elegida ? elegida.etiqueta : valor !== '' && opciones.length === 0 ? 'Cargando…' : vacio}
        </span>
        <ChevronDown
          size={15}
          strokeWidth={1.8}
          aria-hidden="true"
          className="tw:shrink-0 tw:text-tinta-low"
        />
      </button>

      {abierto && caja !== null && destino !== null && createPortal(
        <div
          ref={panel}
          style={{
            position: 'fixed',
            left: caja.x,
            top: caja.arriba ? undefined : caja.y + 4,
            bottom: caja.arriba ? window.innerHeight - caja.y + 4 : undefined,
            width: caja.ancho,
          }}
          className="tw:z-50 tw:overflow-hidden tw:rounded-control tw:border tw:border-linea tw:bg-panel tw:shadow-lg">
          <div className="tw:relative tw:border-b tw:border-linea-suave tw:p-1.5">
            <Search
              size={15}
              strokeWidth={1.8}
              aria-hidden="true"
              className="tw:pointer-events-none tw:absolute tw:left-3.5 tw:top-1/2 tw:-translate-y-1/2 tw:text-tinta-low"
            />
            <input
              ref={buscador}
              type="text"
              role="combobox"
              aria-expanded
              aria-controls={idLista}
              aria-autocomplete="list"
              aria-label="Buscar en la lista"
              aria-activedescendant={filtradas[indice] ? `${idBase}-o${String(indice)}` : undefined}
              autoComplete="off"
              value={consulta}
              placeholder={buscarPlaceholder}
              onChange={(e) => { setConsulta(e.target.value); }}
              onKeyDown={alTeclearEnBuscador}
              className="tw:h-8 tw:w-full tw:rounded-control tw:border-0 tw:bg-transparent tw:pl-8 tw:pr-2 tw:text-cuerpo-2 tw:text-tinta-base tw:outline-none"
            />
          </div>

          <ul ref={lista} id={idLista} role="listbox" className="tw:max-h-64 tw:overflow-y-auto tw:py-1">
            {filtradas.length === 0 && (
              <li className="tw:px-3 tw:py-2 tw:text-cuerpo-2 tw:text-tinta-low">{sinResultados}</li>
            )}
            {conGrupo.map((g) => (
              <li key={g.grupo || '—'}>
                {g.grupo !== '' && (
                  <p className="tw:px-3 tw:pb-0.5 tw:pt-2 tw:text-ayuda tw:font-semibold tw:uppercase tw:tracking-wide tw:text-tinta-low">
                    {g.grupo}
                  </p>
                )}
                <ul>
                  {g.items.map(({ o, i }) => {
                    const resaltada = i === indice;
                    const esta = o.valor === valor;
                    return (
                      <li
                        key={o.valor}
                        id={`${idBase}-o${String(i)}`}
                        role="option"
                        aria-selected={esta}
                        data-resaltada={resaltada}
                        // `onMouseDown` y no `onClick`: el clic dispara antes el `blur` del
                        // buscador, y para entonces la lista ya se cerró y no hay qué pulsar.
                        onMouseDown={(e) => { e.preventDefault(); elegir(o); }}
                        onMouseEnter={() => { setIndice(i); }}
                        className={[
                          'tw:flex tw:cursor-pointer tw:items-center tw:gap-2 tw:px-3 tw:py-1.5 tw:text-cuerpo-2',
                          resaltada ? 'tw:bg-inset tw:text-tinta-hi' : 'tw:text-tinta-base',
                        ].join(' ')}
                      >
                        <Check
                          size={14}
                          strokeWidth={2}
                          aria-hidden="true"
                          className={esta ? 'tw:shrink-0 tw:text-ok-fg' : 'tw:shrink-0 tw:opacity-0'}
                        />
                        <span className="tw:min-w-0">{o.etiqueta}</span>
                      </li>
                    );
                  })}
                </ul>
              </li>
            ))}
          </ul>
        </div>,
        destino,
      )}
    </div>
  );
}
