import type { ReactElement } from 'react';
import { useMemo, useState } from 'react';
import { Link } from 'react-router';
import { LayoutList } from 'lucide-react';

import { CampoBusqueda, Nota, Panel, Pastilla, Segmentado, Vacio } from '../ui';
import { ETIQUETA, TONO } from './EnDesarrollo';
import { BLOQUEADAS, PANTALLAS, RESERVADA_PT_139, SIN_INVENTARIAR } from './registro';
import type { PantallaConSituacion } from './registro';
import type { SituacionDePantalla } from './tipos';

/**
 * Las 138 pantallas del sistema, todas de una vez.
 *
 * ── Qué contesta ────────────────────────────────────────────────────────────
 * **Cuánto falta, y de qué tipo es lo que falta.** No es lo mismo una pantalla sin empezar que
 * una que espera un formato en papel: la primera se destraba programando y la segunda no se
 * destraba programando nunca. Verlas juntas en un solo número las mezcla en una cola que no se
 * puede planificar.
 *
 * ── Por qué se lee del inventario y no de una lista propia ──────────────────
 * `docs/04-diseno/inventario-de-pantallas.md` es la autoridad. Esta pantalla renderiza lo que
 * ese documento dice, generado por `npm run generar-inventario`, y `npm run verificar` falla si
 * el generado quedó atrás. Una lista escrita a mano acá diría 138 para siempre.
 */
export default function Mapa(): ReactElement {
  const [filtro, setFiltro] = useState('');
  const [situacion, setSituacion] = useState<SituacionDePantalla | 'todas'>('todas');

  // Se tipa con las cinco claves y no con `Record<string, number>`: un índice ancho obliga a
  // comprobar cada acceso contra `undefined`, y acá no hay ninguna clave que pueda faltar.
  const conteo = useMemo(() => {
    const c: Record<SituacionDePantalla, number> = {
      construida: 0, parcial: 0, pendiente: 0, bloqueada: 0, campo: 0,
    };
    PANTALLAS.forEach((p) => { c[p.situacion] += 1; });
    return c;
  }, []);

  const filas = useMemo(() => {
    const busqueda = filtro.trim().toLowerCase();

    return PANTALLAS.filter((p) => situacion === 'todas' || p.situacion === situacion).filter(
      (p) =>
        busqueda === '' ||
        [p.id, p.nombre, p.seccion, p.roles, p.hu, p.cu, ETIQUETA[p.situacion]]
          .join(' ')
          .toLowerCase()
          .includes(busqueda),
    );
  }, [filtro, situacion]);

  // Agrupadas por sección del documento, que hace de módulo. Una lista plana de 138 filas no
  // se lee: lo que se busca casi siempre es «qué falta de combustible».
  const grupos = useMemo(() => {
    const m = new Map<string, PantallaConSituacion[]>();
    filas.forEach((p) => m.set(p.seccion, [...(m.get(p.seccion) ?? []), p]));
    return [...m.entries()];
  }, [filas]);

  const listas = conteo.construida + conteo.parcial;

  return (
    <div className="tw:flex tw:flex-col tw:gap-5">
      <header className="tw:flex tw:flex-col tw:gap-1">
        <h1 className="tw:text-xl tw:font-semibold tw:tracking-tight">Mapa de pantallas</h1>
        <p className="tw:text-sm tw:text-tinta-mid">
          Las <b>{PANTALLAS.length}</b> pantallas que el inventario declara, con lo que hay
          construido de cada una. <b>{listas} tienen algo</b> y {PANTALLAS.length - listas} no
          existen todavía.
        </p>
      </header>

      {/* Lo que separa una cola de trabajo de un número: sólo una parte se destraba
          programando, y decir cuál es todo el valor de esta pantalla. */}
      <Nota tono="info">
        De las {PANTALLAS.length - listas} que faltan, <b>{conteo.bloqueada} esperan un formato
        en papel</b> —el insumo #2, y eso no lo destraba quien programa— y{' '}
        <b>{conteo.campo} son del cliente de campo</b>, que todavía no tiene ninguna interfaz.
        Quedan <b>{conteo.pendiente}</b> que se pueden escribir hoy mismo.
      </Nota>

      {/* El conteo de arriba NO da los 29 que declara el inventario, y callarlo obligaría a
          elegir a cuál de los dos creerle. */}
      <Nota tono="aviso">
        <b>El inventario declara {BLOQUEADAS.segunElInventario} bloqueadas y acá aparecen{' '}
        {conteo.bloqueada}.</b> La diferencia no es un error de ninguno de los dos:{' '}
        <b>{BLOQUEADAS.yaConstruidas.length} se construyeron igual</b>, sin el formato a la
        vista ({BLOQUEADAS.yaConstruidas.join(', ')}) — y puede haber que rehacerlas cuando
        llegue el papel —, y <b>{BLOQUEADAS.deCampo.length} son del cliente de campo</b>, que no
        espera un formato sino un cliente entero.
      </Nota>

      <div className="tw:grid tw:gap-3 tw:sm:grid-cols-5">
        {(['construida', 'parcial', 'pendiente', 'bloqueada', 'campo'] as const).map((s) => (
          <Recuento
            key={s}
            situacion={s}
            cantidad={conteo[s]}
            activo={situacion === s}
            onElegir={() => setSituacion(situacion === s ? 'todas' : s)}
          />
        ))}
      </div>

      <div className="tw:flex tw:flex-col tw:gap-3 tw:sm:flex-row tw:sm:items-center">
        <div className="tw:grow">
          <CampoBusqueda
            etiqueta="Buscar por PT, nombre, módulo o historia…"
            valor={filtro}
            onCambio={setFiltro}
          />
        </div>

        <Segmentado
          etiqueta="Situación"
          valor={situacion}
          onCambio={setSituacion}
          opciones={[
            { valor: 'todas', etiqueta: 'Todas' },
            { valor: 'pendiente', etiqueta: 'Se pueden hacer' },
            { valor: 'construida', etiqueta: 'Hechas' },
          ]}
        />
      </div>

      {grupos.length === 0 ? (
        <Vacio
          icono={<LayoutList />}
          titulo="Ninguna pantalla coincide"
          descripcion="Pruebe con el identificador completo, o limpie los filtros."
        />
      ) : (
        grupos.map(([seccion, pantallas]) => (
          <Panel key={seccion} titulo={`${seccion} · ${pantallas.length}`}>
            <ul className="tw:flex tw:flex-col">
              {pantallas.map((p) => (
                <Fila key={p.id} pantalla={p} />
              ))}
            </ul>
          </Panel>
        ))
      )}

      <FueraDelInventario />
    </div>
  );
}

function Recuento({
  situacion,
  cantidad,
  activo,
  onElegir,
}: {
  situacion: SituacionDePantalla;
  cantidad: number;
  activo: boolean;
  onElegir(): void;
}): ReactElement {
  return (
    <button
      type="button"
      onClick={onElegir}
      aria-pressed={activo}
      className={`loki-foco tw:flex tw:flex-col tw:items-start tw:gap-1 tw:rounded-panel tw:border tw:p-3 tw:text-left tw:transition-colors ${
        activo ? 'tw:border-acento tw:bg-panel-sutil' : 'tw:border-linea tw:bg-panel'
      }`}
    >
      <span className="tw:text-2xl tw:font-semibold tw:tabular-nums">{cantidad}</span>
      <Pastilla tono={TONO[situacion]}>{ETIQUETA[situacion]}</Pastilla>
    </button>
  );
}

/**
 * Una fila.
 *
 * **Toda pantalla es navegable**, exista o no: la construida abre la real y la que falta abre
 * su ficha con el motivo. Una lista donde la mitad de las filas no se puede tocar enseña a no
 * tocarlas.
 */
function Fila({ pantalla }: { pantalla: PantallaConSituacion }): ReactElement {
  const destino = pantalla.ruta ?? `/pantallas/${pantalla.id.toLowerCase()}`;

  return (
    <li>
      <Link
        to={destino}
        className="loki-fila-inicio loki-foco tw:flex tw:flex-wrap tw:items-center tw:gap-x-3 tw:gap-y-1 tw:rounded tw:px-2 tw:py-2 tw:text-sm"
      >
        <span className="tw:w-16 tw:shrink-0 tw:font-mono tw:text-xs tw:text-tinta-mid">
          {pantalla.id}
        </span>

        <span className="tw:grow tw:min-w-48">{pantalla.nombre}</span>

        {/* La ruta se muestra cuando existe: dice que se puede abrir y adónde va. */}
        {pantalla.ruta !== null && (
          <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{pantalla.ruta}</span>
        )}

        <Pastilla tono={TONO[pantalla.situacion]}>{ETIQUETA[pantalla.situacion]}</Pastilla>
      </Link>
    </li>
  );
}

/**
 * Lo construido que el inventario no tiene.
 *
 * Va en esta pantalla y no en un documento aparte porque **es el desfase inverso** —código
 * adelante del documento— y un mapa que sólo muestra el desfase en un sentido da a entender que
 * el otro no existe.
 */
function FueraDelInventario(): ReactElement {
  return (
    <Panel titulo={`Construidas que el inventario no tiene · ${SIN_INVENTARIAR.length + 1}`}>
      <div className="tw:flex tw:flex-col tw:gap-3">
        <p className="tw:text-sm tw:text-tinta-mid">
          No son sobras: cada una salió de una regla escrita <b>después</b> del inventario, y su
          ausencia allá es un hueco del documento. Ninguna cuenta dentro de las{' '}
          {PANTALLAS.length}.
        </p>

        {SIN_INVENTARIAR.map((p) => (
          <div key={p.ruta} className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-linea tw:pl-3">
            <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
              <Link to={p.ruta} className="loki-foco tw:font-medium tw:underline-offset-2 tw:hover:underline">
                {p.nombre}
              </Link>
              <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">{p.ruta}</span>
            </div>
            <span className="tw:text-xs tw:text-tinta-mid">{p.porQue}</span>
          </div>
        ))}

        {/* El caso más raro: construida, sin ID asignado, y con un ID reservado esperándola. */}
        <div className="tw:flex tw:flex-col tw:gap-0.5 tw:border-l-2 tw:border-aviso-fg tw:pl-3">
          <div className="tw:flex tw:flex-wrap tw:items-baseline tw:gap-x-3 tw:text-sm">
            <Link
              to={RESERVADA_PT_139.ruta}
              className="loki-foco tw:font-medium tw:underline-offset-2 tw:hover:underline"
            >
              {RESERVADA_PT_139.nombre}
            </Link>
            <span className="tw:font-mono tw:text-xs tw:text-tinta-mid">
              {RESERVADA_PT_139.ruta}
            </span>
            <Pastilla tono="aviso">{RESERVADA_PT_139.id} reservado</Pastilla>
          </div>
          <span className="tw:text-xs tw:text-tinta-mid">
            El diseño lo dibujó y el inventario lo dejó fuera esperando que el PO lo acepte:{' '}
            <i>«si el PO lo acepta, entra como PT-139; el ID queda reservado»</i>.{' '}
            <b>Ya está construido</b>, así que hay una pantalla en uso sin la decisión tomada.
          </span>
        </div>
      </div>
    </Panel>
  );
}
